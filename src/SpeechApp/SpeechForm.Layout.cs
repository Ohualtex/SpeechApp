namespace SpeechApp;

internal sealed partial class SpeechForm
{
    private static readonly Color Canvas = Color.FromArgb(248, 247, 250);
    private static readonly Color Surface = Color.White;
    private static readonly Color Ink = Color.FromArgb(31, 28, 41);
    private static readonly Color Muted = Color.FromArgb(91, 84, 104);
    private static readonly Color Border = Color.FromArgb(207, 201, 216);
    private static readonly Color Accent = Color.FromArgb(82, 71, 170);
    private static readonly Color ErrorInk = Color.FromArgb(164, 40, 62);
    private static readonly Font TitleFont = new("Segoe UI", 18, FontStyle.Bold);
    private static readonly Font LabelFont = new("Segoe UI", 10, FontStyle.Bold);
    private static readonly Font TextFont = new("Segoe UI", 12);

    private Control BuildLayout()
    {
        BackColor = Canvas;
        ForeColor = Ink;
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(24, 14, 24, 20)
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 2, Margin = new Padding(0, 0, 0, 12)
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        header.Controls.Add(new Label { Text = "SpeechApp", Font = TitleFont, AutoSize = true, Margin = Padding.Empty }, 0, 0);
        activityStatus.Anchor = AnchorStyles.Right;
        activityStatus.ForeColor = Accent;
        header.Controls.Add(activityStatus, 1, 0);
        root.Controls.Add(header, 0, 0);

        StyleButton(record, primary: true);
        StyleButton(speak, primary: true);
        foreach (Button button in new[] { chooseModel, stopRecording, clearTranscript, pause, stopSpeech, clearSource })
            StyleButton(button, primary: false);

        var tabs = new TabControl
        {
            Dock = DockStyle.Fill, Margin = Padding.Empty, DrawMode = TabDrawMode.OwnerDrawFixed,
            SizeMode = TabSizeMode.Fixed, ItemSize = new Size(162, 38), AccessibleName = "Speech tools"
        };
        tabs.DrawItem += DrawTab;
        tabs.SelectedIndexChanged += (_, _) => tabs.Invalidate();
        tabs.GotFocus += (_, _) => tabs.Invalidate();
        tabs.LostFocus += (_, _) => tabs.Invalidate();
        tabs.TabPages.Add(new TabPage("Speech to Text") { BackColor = Canvas });
        tabs.TabPages.Add(new TabPage("Text to Speech") { BackColor = Canvas });
        tabs.TabPages[0].Controls.Add(BuildWorkspace("Transcript", transcript, recognitionStatus,
            BuildOptions("Language", language, includeModel: true), record, stopRecording, clearTranscript));
        tabs.TabPages[1].Controls.Add(BuildWorkspace("Text to read", source, speechStatus,
            BuildOptions("Voice", voices, includeModel: false), speak, pause, stopSpeech, clearSource));
        root.Controls.Add(tabs, 0, 1);
        return root;
    }

    private static void DrawTab(object? sender, DrawItemEventArgs e)
    {
        var tabs = (TabControl)sender!;
        bool selected = tabs.SelectedIndex == e.Index;
        using var background = new SolidBrush(selected ? Surface : Canvas);
        e.Graphics.FillRectangle(background, e.Bounds);
        var labelBounds = Rectangle.Inflate(e.Bounds, -7, -4);
        TextRenderer.DrawText(e.Graphics, tabs.TabPages[e.Index].Text, selected ? LabelFont : tabs.Font,
            labelBounds, selected ? Accent : Muted, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        if (selected)
        {
            using var accent = new SolidBrush(Accent);
            e.Graphics.FillRectangle(accent, e.Bounds.Left, e.Bounds.Bottom - 3, e.Bounds.Width, 3);
            if (tabs.Focused && (e.State & DrawItemState.Focus) != 0)
            {
                using var focus = new Pen(Border);
                e.Graphics.DrawRectangle(focus, labelBounds);
            }
        }
    }

    private Control BuildOptions(string label, ComboBox selector, bool includeModel)
    {
        var options = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, AutoSize = true, ColumnCount = includeModel ? 4 : 3,
            Margin = new Padding(0, 0, 0, 12)
        };
        options.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        options.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 228));
        options.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        var caption = Caption(label);
        caption.Anchor = AnchorStyles.Left;
        caption.Margin = new Padding(0, 0, 12, 0);
        options.Controls.Add(caption, 0, 0);
        selector.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        selector.Margin = Padding.Empty;
        options.Controls.Add(selector, 1, 0);

        if (includeModel)
        {
            options.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            modelStatus.Dock = DockStyle.Fill;
            modelStatus.Margin = new Padding(12, 0, 12, 0);
            modelStatus.ForeColor = Muted;
            modelStatus.TextAlign = ContentAlignment.MiddleRight;
            chooseModel.Anchor = AnchorStyles.Right;
            chooseModel.Margin = Padding.Empty;
            options.Controls.Add(modelStatus, 2, 0);
            options.Controls.Add(chooseModel, 3, 0);
        }
        return options;
    }

    private static Control BuildWorkspace(string name, RichTextBox editor, Label status, Control options,
        params Button[] buttons)
    {
        var workspace = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, BackColor = Canvas, Padding = new Padding(18),
            ColumnCount = 1, RowCount = 4
        };
        workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        workspace.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        workspace.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        workspace.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        workspace.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        workspace.Controls.Add(options, 0, 0);
        workspace.Controls.Add(Caption(name), 0, 1);

        var editorFrame = new Panel
        {
            Dock = DockStyle.Fill, Padding = new Padding(12, 8, 12, 8), BackColor = Surface,
            Margin = new Padding(0, 0, 0, 14)
        };
        editorFrame.Paint += (_, e) => ControlPaint.DrawBorder(e.Graphics, editorFrame.ClientRectangle, Border, ButtonBorderStyle.Solid);
        editor.Dock = DockStyle.Fill;
        editor.Font = TextFont;
        editor.BackColor = Surface;
        editor.ForeColor = Ink;
        editor.BorderStyle = BorderStyle.None;
        editorFrame.Controls.Add(editor);
        workspace.Controls.Add(editorFrame, 0, 2);

        var footer = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 2, Margin = Padding.Empty
        };
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        var actions = new FlowLayoutPanel
        {
            AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false, Margin = Padding.Empty
        };
        actions.Controls.AddRange(buttons);
        status.Dock = DockStyle.Fill;
        status.ForeColor = Muted;
        status.TextAlign = ContentAlignment.MiddleRight;
        status.Margin = new Padding(12, 0, 0, 0);
        footer.Controls.Add(actions, 0, 0);
        footer.Controls.Add(status, 1, 0);
        workspace.Controls.Add(footer, 0, 3);
        return workspace;
    }

    private static Label Caption(string text) =>
        new() { Text = text, Font = LabelFont, AutoSize = true, Margin = new Padding(0, 0, 0, 8) };

    private static void StyleButton(Button button, bool primary)
    {
        button.AutoSize = true;
        button.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        button.Padding = new Padding(12, 5, 12, 5);
        button.Margin = new Padding(0, 0, 8, 0);
        button.MinimumSize = new Size(68, 36);
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = primary ? 0 : 1;
        button.FlatAppearance.BorderColor = Border;
        button.FlatAppearance.MouseOverBackColor = primary ? Color.FromArgb(67, 56, 153) : Color.FromArgb(239, 235, 247);
        button.FlatAppearance.MouseDownBackColor = primary ? Color.FromArgb(54, 44, 131) : Color.FromArgb(228, 222, 240);
        button.UseVisualStyleBackColor = false;
        button.BackColor = primary ? Accent : Surface;
        button.ForeColor = primary ? Color.White : Ink;
        button.EnabledChanged += (_, _) => button.BackColor = button.Enabled && primary ? Accent : button.Enabled ? Surface : Canvas;
    }
}
