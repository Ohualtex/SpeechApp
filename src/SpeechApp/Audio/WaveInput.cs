using System.Runtime.InteropServices;
using System.Text;

namespace SpeechApp.Audio;

internal static class WaveInput
{
    internal static void Check(uint code)
    {
        if (code == 0) return;
        var text = new StringBuilder(256);
        WaveInGetErrorText(code, text, (uint)text.Capacity);
        throw new IOException("Check your microphone and Windows microphone permissions. " + text);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal struct WaveFormat
    {
        public ushort FormatTag, Channels;
        public uint SamplesPerSecond, AverageBytesPerSecond;
        public ushort BlockAlignment, BitsPerSample, ExtraSize;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct WaveHeader
    {
        public IntPtr Data;
        public uint BufferLength, BytesRecorded;
        public UIntPtr User;
        public uint Flags, Loops;
        public IntPtr Next;
        public UIntPtr Reserved;
    }
    [DllImport("winmm.dll", EntryPoint = "waveInOpen")]
    internal static extern uint WaveInOpen(out IntPtr device, uint id, ref WaveFormat format, IntPtr callback, IntPtr instance, uint flags);
    [DllImport("winmm.dll", EntryPoint = "waveInPrepareHeader")]
    internal static extern uint WaveInPrepareHeader(IntPtr device, IntPtr header, uint size);
    [DllImport("winmm.dll", EntryPoint = "waveInUnprepareHeader")]
    internal static extern uint WaveInUnprepareHeader(IntPtr device, IntPtr header, uint size);
    [DllImport("winmm.dll", EntryPoint = "waveInAddBuffer")]
    internal static extern uint WaveInAddBuffer(IntPtr device, IntPtr header, uint size);
    [DllImport("winmm.dll", EntryPoint = "waveInStart")]
    internal static extern uint WaveInStart(IntPtr device);
    [DllImport("winmm.dll", EntryPoint = "waveInStop")]
    internal static extern uint WaveInStop(IntPtr device);
    [DllImport("winmm.dll", EntryPoint = "waveInReset")]
    internal static extern uint WaveInReset(IntPtr device);
    [DllImport("winmm.dll", EntryPoint = "waveInClose")]
    internal static extern uint WaveInClose(IntPtr device);
    [DllImport("winmm.dll", EntryPoint = "waveInGetErrorTextW", CharSet = CharSet.Unicode)]
    internal static extern uint WaveInGetErrorText(uint code, StringBuilder text, uint length);
}
