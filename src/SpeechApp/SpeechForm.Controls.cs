namespace SpeechApp;

internal enum Activity { Idle, Recording, Transcribing, Cancelling, Speaking, Paused, Stopping }

internal sealed partial class SpeechForm : Form
{
    private readonly WorkspaceTab recognitionTab = new("Speech to Text");
    private readonly WorkspaceTab playbackTab = new("Text to Speech");
    private readonly RichTextBox source = new() { AccessibleName = "Text to read", DetectUrls = false };
    private readonly RichTextBox transcript = new() { AccessibleName = "Transcript", DetectUrls = false, ReadOnly = true };
    private readonly OptionSelector voices = new() { DropDownStyle = ComboBoxStyle.DropDownList, AccessibleName = "Reading voice" };
    private readonly OptionSelector language = new() { DropDownStyle = ComboBoxStyle.DropDownList, AccessibleName = "Recognition language" };
    private readonly ActionButton speak = new() { Text = "Read aloud" };
    private readonly ActionButton pause = new() { Text = "Pause" };
    private readonly ActionButton stopSpeech = new() { Text = "Stop" };
    private readonly ActionButton clearSource = new() { Text = "Clear" };
    private readonly ActionButton record = new() { Text = "Start recording" };
    private readonly ActionButton stopRecording = new() { Text = "Stop" };
    private readonly ActionButton clearTranscript = new() { Text = "Clear" };
    private readonly ActionButton chooseModel = new() { Text = "Choose model..." };
    private readonly Label modelStatus = new() { AutoEllipsis = true };
    private readonly Label speechStatus = new() { AutoEllipsis = true };
    private readonly Label recognitionStatus = new() { AutoEllipsis = true };
    private readonly Label activityStatus = new() { AutoSize = true, AccessibleName = "Current audio activity" };
    private readonly ToolTip tips = new();
}
