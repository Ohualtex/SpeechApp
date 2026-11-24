namespace SpeechApp;

internal sealed partial class SpeechForm
{
    private static readonly Color FocusTint = Color.FromArgb(226, 218, 248);

    protected override bool ProcessDialogKey(Keys keyData)
    {
        bool tabShortcut = keyData is (Keys.Control | Keys.Tab) or (Keys.Control | Keys.Shift | Keys.Tab);
        bool tabArrow = (recognitionTab.Focused || playbackTab.Focused) &&
            keyData is Keys.Left or Keys.Right or Keys.Up or Keys.Down;
        if (!tabShortcut && !tabArrow) return base.ProcessDialogKey(keyData);

        WorkspaceTab next = recognitionTab.Checked ? playbackTab : recognitionTab;
        next.Checked = true;
        next.Focus();
        return true;
    }

    private enum ActionStyle { Secondary, Primary, Stop }

    private sealed class ActionButton : Button
    {
        private readonly Color readyColor;
        private readonly Color focusColor;
        private readonly bool filled;

        public ActionButton(ActionStyle style = ActionStyle.Secondary)
        {
            filled = style != ActionStyle.Secondary;
            (readyColor, focusColor, Color hover, Color pressed) = style switch
            {
                ActionStyle.Primary => (Accent, Color.FromArgb(61, 49, 141), Color.FromArgb(67, 56, 153), Color.FromArgb(54, 44, 131)),
                ActionStyle.Stop => (Color.FromArgb(173, 48, 70), Color.FromArgb(133, 32, 51), Color.FromArgb(151, 39, 59), Color.FromArgb(119, 29, 47)),
                _ => (Surface, FocusTint, Color.FromArgb(239, 235, 247), Color.FromArgb(228, 222, 240))
            };

            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Padding = new Padding(12, 5, 12, 5);
            Margin = new Padding(0, 0, 8, 0);
            MinimumSize = new Size(68, 36);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.MouseOverBackColor = hover;
            FlatAppearance.MouseDownBackColor = pressed;
            UseVisualStyleBackColor = false;

            EnabledChanged += (_, _) => UpdateAppearance();
            GotFocus += (_, _) => UpdateAppearance();
            LostFocus += (_, _) => UpdateAppearance();
            ChangeUICues += (_, _) => UpdateAppearance();
            UpdateAppearance();
        }

        protected override bool ShowFocusCues => false;

        private void UpdateAppearance()
        {
            bool keyboardFocus = Enabled && Focused && base.ShowFocusCues;
            BackColor = Enabled ? (keyboardFocus ? focusColor : readyColor) : Color.FromArgb(238, 235, 242);
            ForeColor = Enabled ? (filled ? Color.White : Ink) : Color.FromArgb(122, 113, 133);
            FlatAppearance.BorderSize = Enabled && !filled ? 1 : 0;
            FlatAppearance.BorderColor = Border;
        }
    }

    private sealed class OptionSelector : ComboBox
    {
        public OptionSelector() => DrawMode = DrawMode.OwnerDrawFixed;

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            bool field = (e.State & DrawItemState.ComboBoxEdit) != 0;
            bool highlighted = !field && (e.State & DrawItemState.Selected) != 0;
            bool keyboardFocus = field && Focused && ShowFocusCues;
            Color fill = highlighted ? SystemColors.Highlight : keyboardFocus ? FocusTint : BackColor;
            using var background = new SolidBrush(fill);
            e.Graphics.FillRectangle(background, e.Bounds);
            if (e.Index >= 0)
            {
                Color ink = highlighted ? SystemColors.HighlightText : Enabled ? ForeColor : SystemColors.GrayText;
                var textBounds = Rectangle.Inflate(e.Bounds, -3, 0);
                TextRenderer.DrawText(e.Graphics, GetItemText(Items[e.Index]), Font, textBounds, ink,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis |
                    TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
            }
        }
    }

    private sealed class WorkspaceTab : RadioButton
    {
        public WorkspaceTab(string label)
        {
            Text = label;
            Appearance = Appearance.Button;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            FlatAppearance.CheckedBackColor = Surface;
            FlatAppearance.MouseOverBackColor = Color.FromArgb(239, 235, 247);
            FlatAppearance.MouseDownBackColor = Color.FromArgb(228, 222, 240);
            UseVisualStyleBackColor = false;
            BackColor = Canvas;
            ForeColor = Muted;
            Size = new Size(162, 38);
            Margin = Padding.Empty;
            TextAlign = ContentAlignment.MiddleCenter;
            GotFocus += (_, _) => UpdateFocusTint();
            LostFocus += (_, _) => UpdateFocusTint();
            ChangeUICues += (_, _) => UpdateFocusTint();
        }

        protected override bool ShowFocusCues => false;

        private void UpdateFocusTint()
        {
            bool keyboardFocus = Focused && base.ShowFocusCues;
            BackColor = keyboardFocus ? FocusTint : Canvas;
            FlatAppearance.CheckedBackColor = keyboardFocus ? FocusTint : Surface;
        }

        protected override void OnCheckedChanged(EventArgs e)
        {
            Font = Checked ? LabelFont : null;
            ForeColor = Checked ? Accent : Muted;
            base.OnCheckedChanged(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (!Checked) return;

            using var underline = new SolidBrush(Accent);
            e.Graphics.FillRectangle(underline, 0, Height - 3, Width, 3);
        }
    }
}
