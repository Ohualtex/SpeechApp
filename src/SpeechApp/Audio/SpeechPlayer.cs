using System.Speech.Synthesis;

namespace SpeechApp.Audio;

internal sealed class SpeechPlayer : IDisposable
{
    private readonly SpeechSynthesizer synthesizer = new();

    public SpeechPlayer()
    {
        synthesizer.SpeakCompleted += (_, e) => Finished?.Invoke(e.Prompt, e.Cancelled, e.Error);
    }

    public event Action<Prompt, bool, Exception?>? Finished;

    public IReadOnlyList<string> GetVoices() => synthesizer.GetInstalledVoices()
        .Where(voice => voice.Enabled)
        .OrderByDescending(voice => voice.VoiceInfo.Culture.TwoLetterISOLanguageName == "tr")
        .Select(voice => voice.VoiceInfo.Name)
        .ToArray();

    public Prompt Start(string text, string voice)
    {
        synthesizer.SelectVoice(voice);
        synthesizer.SetOutputToDefaultAudioDevice();
        return synthesizer.SpeakAsync(text);
    }

    public void Pause() => synthesizer.Pause();
    public void Resume() => synthesizer.Resume();
    public void Stop()
    {
        synthesizer.SpeakAsyncCancelAll();
        if (synthesizer.State == SynthesizerState.Paused) synthesizer.Resume();
    }

    public void Dispose() => synthesizer.Dispose();
}
