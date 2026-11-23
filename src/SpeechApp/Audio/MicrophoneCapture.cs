using System.Runtime.InteropServices;
using static SpeechApp.Audio.WaveInput;

namespace SpeechApp.Audio;

internal sealed class MicrophoneCapture : IDisposable
{
    private const int SampleRate = 16000;
    private const int BufferSize = 4096;
    private static readonly uint HeaderSize = (uint)Marshal.SizeOf<WaveHeader>();

    private readonly MemoryStream pcm = new();
    private readonly byte[] scratch = new byte[BufferSize];
    private readonly List<(IntPtr Header, IntPtr Data)> buffers = [];
    private readonly Queue<IntPtr> pending = new();
    private IntPtr device;

    public TimeSpan Duration => TimeSpan.FromSeconds(pcm.Length / (SampleRate * 2d));

    public void Start()
    {
        var format = new WaveFormat
        {
            FormatTag = 1, Channels = 1, SamplesPerSecond = SampleRate,
            AverageBytesPerSecond = SampleRate * 2, BlockAlignment = 2, BitsPerSample = 16
        };
        Check(WaveInOpen(out device, uint.MaxValue, ref format, IntPtr.Zero, IntPtr.Zero, 0));
        for (int i = 0; i < 4; i++)
        {
            IntPtr data = Marshal.AllocHGlobal(BufferSize);
            IntPtr header = Marshal.AllocHGlobal((int)HeaderSize);
            buffers.Add((header, data));
            Marshal.StructureToPtr(new WaveHeader { Data = data, BufferLength = BufferSize }, header, false);
            Check(WaveInPrepareHeader(device, header, HeaderSize));
            Check(WaveInAddBuffer(device, header, HeaderSize));
            pending.Enqueue(header);
        }
        Check(WaveInStart(device));
    }

    public void Drain() => Drain(requeue: true);

    private void Drain(bool requeue)
    {
        // WinMM completes the queued buffers in capture order.
        for (int remaining = pending.Count; remaining > 0 && pending.TryPeek(out var pointer); remaining--)
        {
            var header = Marshal.PtrToStructure<WaveHeader>(pointer);
            if ((header.Flags & 1) == 0) break;
            pending.Dequeue();
            int count = checked((int)header.BytesRecorded);
            if (count > BufferSize || count % 2 != 0) throw new IOException("The microphone returned invalid audio.");
            Marshal.Copy(header.Data, scratch, 0, count);
            pcm.Write(scratch.AsSpan(0, count));
            if (requeue)
            {
                Check(WaveInAddBuffer(device, pointer, HeaderSize));
                pending.Enqueue(pointer);
            }
        }
    }

    public MemoryStream Finish()
    {
        if (device == IntPtr.Zero) throw new InvalidOperationException("Start recording first.");
        Check(WaveInReset(device));
        Drain(requeue: false);
        ReleaseDevice();
        if (pcm.Length == 0) throw new IOException("No audio was captured. Check the microphone and try again.");

        // Whisper expects a 16 kHz, 16-bit PCM WAV stream.
        int dataLength = checked((int)pcm.Length);
        var wave = new MemoryStream(44 + dataLength);
        using (var writer = new BinaryWriter(wave, System.Text.Encoding.ASCII, leaveOpen: true))
        {
            writer.Write("RIFF"u8);
            writer.Write(36 + dataLength);
            writer.Write("WAVEfmt "u8);
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)1);
            writer.Write(SampleRate);
            writer.Write(SampleRate * 2);
            writer.Write((short)2);
            writer.Write((short)16);
            writer.Write("data"u8);
            writer.Write(dataLength);
            pcm.Position = 0;
            pcm.CopyTo(wave);
        }
        wave.Position = 0;
        return wave;
    }

    private void ReleaseDevice()
    {
        if (device == IntPtr.Zero) return;
        // Return native buffers before freeing memory still referenced by the driver.
        WaveInReset(device);
        foreach (var (header, data) in buffers)
        {
            WaveInUnprepareHeader(device, header, HeaderSize);
            Marshal.FreeHGlobal(header);
            Marshal.FreeHGlobal(data);
        }
        WaveInClose(device);
        device = IntPtr.Zero;
        buffers.Clear();
        pending.Clear();
    }

    public void Dispose()
    {
        ReleaseDevice();
        pcm.Dispose();
    }
}
