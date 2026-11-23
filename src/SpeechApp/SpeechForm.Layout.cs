namespace SpeechApp;

internal sealed partial class SpeechForm
{
    private static readonly Color Canvas = Color.FromArgb(241, 243, 249);
    private static readonly Color Surface = Color.White;
    private static readonly Color Ink = Color.FromArgb(28, 34, 53);
    private static readonly Color Muted = Color.FromArgb(76, 86, 111);
    private static readonly Color Accent = Color.FromArgb(72, 66, 167);
    private static readonly Color ErrorInk = Color.FromArgb(164, 40, 62);
    private static readonly Font TitleFont = new("Segoe UI", 24, FontStyle.Bold);
    private static readonly Font SectionFont = new("Segoe UI", 15, FontStyle.Bold);
    private static readonly Font TextFont = new("Segoe UI", 12);

    private Control BuildLayout()
    {
        BackColor = Canvas;
        ForeColor = Ink;
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 2, Padding = new Padding(24, 14, 24, 24)
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var title = new Label { Text = "SpeechApp", Font = TitleFont, AutoSize = true, Margin = new Padding(4, 5, 0, 14) };
        root.Controls.Add(title, 0, 0);
        root.SetColumnSpan(title, 2);

        StyleText(transcript, "Speech will appear here after a recording.");
        StyleText(source, "Enter the text you want to hear.");
        StyleButton(record, primary: true);
        StyleButton(speak, primary: true);
        foreach (Button button in new[] { chooseModel, stopRecording, clearTranscript, pause, stopSpeech, clearSource })
            StyleButton(button, primary: false);

        var recognition = BuildSection("Speech to Text", language, transcript, recognitionStatus,
            ModelControls(), Actions(record, stopRecording, clearTranscript));
        var playback = BuildSection("Text to Speech", voices, source, speechStatus, null,
            Actions(speak, pause, stopSpeech, clearSource));
        root.Controls.Add(recognition, 0, 1);
        root.Controls.Add(playback, 1, 1);
        return root;
    }

    private static Control BuildSection(string name, ComboBox selector, RichTextBox editor, Label status,
        Control? extra, Control buttons)
    {
        var card = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, BackColor = Surface, ColumnCount = 1,
            Padding = new Padding(16), Margin = new Padding(4), GrowStyle = TableLayoutPanelGrowStyle.AddRows
        };
        card.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        card.Controls.Add(new Label { Text = name, Font = SectionFont, AutoSize = true, Margin = new Padding(0, 0, 0, 10) });
        card.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        selector.Dock = DockStyle.Top;
        selector.Margin = new Padding(0, 0, 0, 8);
        card.Controls.Add(selector);
        card.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        if (extra is not null)
        {
            card.Controls.Add(extra);
            card.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }
        editor.Dock = DockStyle.Fill;
        editor.Margin = new Padding(0, 4, 0, 10);
        card.Controls.Add(editor);
        card.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        card.Controls.Add(buttons);
        card.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        status.Dock = DockStyle.Top;
        status.Height = 40;
        status.ForeColor = Muted;
        status.Margin = new Padding(0, 8, 0, 0);
        card.Controls.Add(status);
        card.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        return card;
    }

    private Control ModelControls()
    {
        var line = new TableLayoutPanel
        {
            AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top,
            ColumnCount = 2, RowCount = 1, Margin = new Padding(0, 0, 0, 6)
        };
        line.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        line.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        modelStatus.Dock = DockStyle.Fill;
        modelStatus.ForeColor = Muted;
        modelStatus.TextAlign = ContentAlignment.MiddleLeft;
        line.Controls.Add(modelStatus, 0, 0);
        line.Controls.Add(chooseModel, 1, 0);
        return line;
    }

    private static FlowLayoutPanel Actions(params Button[] buttons)
    {
        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = true, Margin = Padding.Empty
        };
        toolbar.Controls.AddRange(buttons);
        return toolbar;
    }

    private static void StyleText(RichTextBox editor, string description)
    {
        editor.Font = TextFont;
        editor.BackColor = Color.FromArgb(246, 247, 252);
        editor.ForeColor = Ink;
        editor.BorderStyle = BorderStyle.None;
        editor.AccessibleDescription = description;
    }

    private static void StyleButton(Button button, bool primary)
    {
        button.AutoSize = true;
        button.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        button.Padding = new Padding(10, 5, 10, 5);
        button.Margin = new Padding(0, 0, 6, 6);
        button.MinimumSize = new Size(64, 34);
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.UseVisualStyleBackColor = false;
        button.BackColor = primary ? Accent : Canvas;
        button.ForeColor = primary ? Color.White : Ink;
    }
}
