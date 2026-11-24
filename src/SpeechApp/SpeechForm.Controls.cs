namespace SpeechApp;

internal enum Activity { Idle, Recording, Transcribing, Cancelling, Speaking, Paused, Stopping }

internal sealed partial class SpeechForm : Form
{
    private readonly RichTextBox source = new() { AccessibleName = "Text to read", DetectUrls = false };
    private readonly RichTextBox transcript = new() { AccessibleName = "Transcript", DetectUrls = false, ReadOnly = true };
    private readonly ComboBox voices = new() { DropDownStyle = ComboBoxStyle.DropDownList, AccessibleName = "Reading voice" };
    private readonly ComboBox language = new() { DropDownStyle = ComboBoxStyle.DropDownList, AccessibleName = "Recognition language" };
    private readonly Button speak = new() { Text = "Read aloud" };
    private readonly Button pause = new() { Text = "Pause" };
    private readonly Button stopSpeech = new() { Text = "Stop" };
    private readonly Button clearSource = new() { Text = "Clear" };
    private readonly Button record = new() { Text = "Start recording" };
    private readonly Button stopRecording = new() { Text = "Stop" };
    private readonly Button clearTranscript = new() { Text = "Clear" };
    private readonly Button chooseModel = new() { Text = "Choose model..." };
    private readonly Label modelStatus = new() { AutoEllipsis = true };
    private readonly Label speechStatus = new() { AutoEllipsis = true };
    private readonly Label recognitionStatus = new() { AutoEllipsis = true };
    private readonly Label activityStatus = new() { AutoSize = true, AccessibleName = "Current audio activity" };
    private readonly ToolTip tips = new();
}
