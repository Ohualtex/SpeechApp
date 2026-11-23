using Whisper.net;

namespace SpeechApp.Audio;

internal static class WhisperTranscriber
{
    public static Task<string> TranscribeAsync(string modelPath, Stream wave, string language, CancellationToken token)
        => Task.Run(async () =>
        {
            using var factory = WhisperFactory.FromPath(modelPath);
            await using var processor = factory.CreateBuilder().WithLanguage(language).Build();
            var segments = new List<string>();
            await foreach (var segment in processor.ProcessAsync(wave, token).ConfigureAwait(false))
            {
                if (!string.IsNullOrWhiteSpace(segment.Text)) segments.Add(segment.Text.Trim());
            }
            return string.Join(" ", segments);
        }, token);
}
