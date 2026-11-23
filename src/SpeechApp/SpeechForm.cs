using System.Speech.Synthesis;
using SpeechApp.Audio;

namespace SpeechApp;

internal sealed partial class SpeechForm : Form
{
    private static readonly TimeSpan RecordingLimit = TimeSpan.FromMinutes(2);
    private readonly System.Windows.Forms.Timer captureTimer = new() { Interval = 40 };
    private SpeechPlayer? player;
    private Prompt? currentPrompt;
    private MicrophoneCapture? capture;
    private CancellationTokenSource? transcription;
    private Activity activity;
    private bool closeAfterTranscription;
    private string? modelPath;

    public SpeechForm()
    {
        Text = "SpeechApp";
        Font = new Font("Segoe UI", 10);
        AutoScaleMode = AutoScaleMode.Dpi;
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(800, 490);
        ClientSize = new Size(1030, 600);
        Controls.Add(BuildLayout());

        language.Items.AddRange([new RecognitionLanguage("Turkish", "tr"), new RecognitionLanguage("English", "en")]);
        language.SelectedIndex = 0;
        string defaultModel = Path.GetFullPath(Path.Combine("models", "ggml-base.bin"));
        if (File.Exists(defaultModel)) modelPath = defaultModel;

        chooseModel.Click += (_, _) => ChooseModel();
        record.Click += (_, _) => StartRecording();
        stopRecording.Click += async (_, _) => await StopOrCancelAsync();
        clearTranscript.Click += (_, _) => { transcript.Clear(); SetStatus(recognitionStatus, "Ready to record."); };
        speak.Click += (_, _) => BeginSpeaking();
        pause.Click += (_, _) => ChangePause();
        stopSpeech.Click += (_, _) => StopSpeaking();
        clearSource.Click += (_, _) => { source.Clear(); source.Focus(); };
        source.TextChanged += (_, _) => RefreshControls();
        transcript.TextChanged += (_, _) => RefreshControls();
        captureTimer.Tick += async (_, _) => await CaptureTickAsync();
        Shown += (_, _) => { InitializeVoices(); source.Focus(); };
        FormClosing += (_, e) =>
        {
            if (transcription is null) return;
            e.Cancel = true;
            closeAfterTranscription = true;
            CancelTranscription();
        };
        FormClosed += (_, _) =>
        {
            captureTimer.Dispose();
            capture?.Dispose();
            player?.Dispose();
            tips.Dispose();
        };
        SetStatus(recognitionStatus, "Record up to two minutes, then stop to transcribe.");
        SetStatus(speechStatus, "Loading installed voices...");
        RefreshControls();
    }

    private void InitializeVoices()
    {
        try
        {
            player = new SpeechPlayer();
            player.Finished += (prompt, cancelled, error) =>
            {
                // The synthesizer can finish on a worker thread.
                if (IsHandleCreated && !IsDisposed)
                    BeginInvoke(() => CompleteSpeech(prompt, cancelled, error));
            };
            voices.Items.AddRange(player.GetVoices().Cast<object>().ToArray());
            if (voices.Items.Count > 0)
            {
                voices.SelectedIndex = 0;
                SetStatus(speechStatus, "Ready to read.");
            }
            else SetStatus(speechStatus, "No compatible Windows reading voices are installed.", error: true);
        }
        catch (Exception error)
        {
            SetStatus(speechStatus, "Cannot load Windows voices. " + error.Message, error: true);
        }
        RefreshControls();
    }

    private void ChooseModel()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Choose a multilingual Whisper model",
            Filter = "Whisper model (*.bin)|*.bin",
            CheckFileExists = true
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        modelPath = dialog.FileName;
        SetStatus(recognitionStatus, "Ready to record.");
        RefreshControls();
    }

    private void StartRecording()
    {
        if (activity != Activity.Idle || modelPath is null) return;
        if (!File.Exists(modelPath))
        {
            modelPath = null;
            SetStatus(recognitionStatus, "The model file could not be found. Choose it again.", error: true);
            RefreshControls();
            return;
        }

        try
        {
            capture = new MicrophoneCapture();
            capture.Start();
            SetActivity(Activity.Recording);
            SetStatus(recognitionStatus, "Recording  0:00");
            captureTimer.Start();
        }
        catch (Exception error)
        {
            EndCapture();
            SetStatus(recognitionStatus, error.Message, error: true);
        }
    }

    private async Task CaptureTickAsync()
    {
        if (activity != Activity.Recording || capture is null) return;
        try
        {
            capture.Drain();
            TimeSpan elapsed = capture.Duration;
            SetStatus(recognitionStatus, $"Recording  {elapsed.Minutes}:{elapsed.Seconds:00}");
            if (elapsed >= RecordingLimit) await StopOrCancelAsync();
        }
        catch (Exception error)
        {
            EndCapture();
            SetStatus(recognitionStatus, error.Message, error: true);
        }
    }

    private async Task StopOrCancelAsync()
    {
        if (activity == Activity.Transcribing) { CancelTranscription(); return; }
        if (activity != Activity.Recording || capture is null || modelPath is null) return;

        captureTimer.Stop();
        using var lifetime = new CancellationTokenSource();
        transcription = lifetime;
        try
        {
            using MemoryStream wave = capture.Finish();
            capture.Dispose();
            capture = null;
            SetActivity(Activity.Transcribing);
            SetStatus(recognitionStatus, "Transcribing on this computer...");
            string code = ((RecognitionLanguage)language.SelectedItem!).Code;
            string text = await WhisperTranscriber.TranscribeAsync(modelPath, wave, code, lifetime.Token);
            lifetime.Token.ThrowIfCancellationRequested();
            if (text.Length > 0)
            {
                if (transcript.TextLength > 0) transcript.AppendText(Environment.NewLine + Environment.NewLine);
                transcript.AppendText(text);
            }
            SetStatus(recognitionStatus, text.Length == 0 ? "No speech was recognized. Try another recording." : "Transcription complete.");
        }
        catch (OperationCanceledException) when (lifetime.IsCancellationRequested)
        {
            SetStatus(recognitionStatus, "Transcription cancelled.");
        }
        catch (Exception error)
        {
            SetStatus(recognitionStatus, "Could not transcribe the recording. " + error.Message, error: true);
        }
        finally
        {
            transcription = null;
            EndCapture();
            if (closeAfterTranscription) Close();
        }
    }

    private void CancelTranscription()
    {
        transcription?.Cancel();
        SetActivity(Activity.Cancelling);
        SetStatus(recognitionStatus, "Cancelling transcription...");
    }

    private void EndCapture()
    {
        captureTimer.Stop();
        capture?.Dispose();
        capture = null;
        SetActivity(Activity.Idle);
    }

    private void BeginSpeaking()
    {
        if (activity != Activity.Idle || player is null || voices.SelectedItem is not string voice) return;
        try
        {
            currentPrompt = player.Start(source.Text.Trim(), voice);
            SetActivity(Activity.Speaking);
            SetStatus(speechStatus, "Reading aloud.");
        }
        catch (Exception error)
        {
            SetStatus(speechStatus, "Cannot play speech. Check the audio output. " + error.Message, error: true);
        }
    }

    private void ChangePause()
    {
        try
        {
            if (activity == Activity.Speaking)
            {
                player!.Pause();
                SetActivity(Activity.Paused);
                SetStatus(speechStatus, "Reading paused.");
            }
            else if (activity == Activity.Paused)
            {
                player!.Resume();
                SetActivity(Activity.Speaking);
                SetStatus(speechStatus, "Reading aloud.");
            }
        }
        catch (Exception error) { SetStatus(speechStatus, error.Message, error: true); }
    }

    private void StopSpeaking()
    {
        try
        {
            player!.Stop();
            SetActivity(Activity.Stopping);
            SetStatus(speechStatus, "Stopping playback...");
        }
        catch (Exception error) { SetStatus(speechStatus, error.Message, error: true); }
    }

    private void CompleteSpeech(Prompt prompt, bool cancelled, Exception? error)
    {
        if (!ReferenceEquals(currentPrompt, prompt)) return;
        currentPrompt = null;
        SetActivity(Activity.Idle);
        SetStatus(speechStatus, error?.Message ?? (cancelled ? "Playback stopped." : "Reading complete."), error is not null);
    }

    private void SetActivity(Activity next)
    {
        activity = next;
        RefreshControls();
    }

    private void RefreshControls()
    {
        bool idle = activity == Activity.Idle;
        chooseModel.Enabled = language.Enabled = idle;
        record.Enabled = idle && modelPath is not null;
        modelStatus.Text = modelPath is null ? "Choose a local multilingual model to begin." : Path.GetFileName(modelPath);
        stopRecording.Enabled = activity is Activity.Recording or Activity.Transcribing;
        stopRecording.Text = activity is Activity.Transcribing or Activity.Cancelling ? "Cancel" : "Stop";
        clearTranscript.Enabled = idle && transcript.TextLength > 0;
        source.ReadOnly = !idle;
        speak.Enabled = idle && voices.Items.Count > 0 && !string.IsNullOrWhiteSpace(source.Text);
        voices.Enabled = idle && voices.Items.Count > 0;
        clearSource.Enabled = idle && source.TextLength > 0;
        pause.Enabled = stopSpeech.Enabled = activity is Activity.Speaking or Activity.Paused;
        pause.Text = activity == Activity.Paused ? "Resume" : "Pause";
    }

    private void SetStatus(Label label, string message, bool error = false)
    {
        if (label.Text == message) return;
        label.Text = message;
        label.ForeColor = error ? ErrorInk : Muted;
        tips.SetToolTip(label, message);
    }

    private sealed record RecognitionLanguage(string Name, string Code)
    {
        public override string ToString() => Name;
    }
}
