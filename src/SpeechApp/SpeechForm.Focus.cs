namespace SpeechApp;

internal sealed partial class SpeechForm
{
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


    private sealed class ActionButton : Button
    {
        protected override bool ShowFocusCues => false;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (!Focused || !base.ShowFocusCues) return;

            using var focus = new Pen(BackColor == Accent ? Color.White : Accent);
            e.Graphics.DrawRectangle(focus, Rectangle.Inflate(ClientRectangle, -3, -3));
        }
    }

    private sealed class OptionSelector : ComboBox
    {
        public OptionSelector() => DrawMode = DrawMode.OwnerDrawFixed;

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            bool field = (e.State & DrawItemState.ComboBoxEdit) != 0;
            bool highlighted = !field && (e.State & DrawItemState.Selected) != 0;
            using var background = new SolidBrush(highlighted ? SystemColors.Highlight : BackColor);
            e.Graphics.FillRectangle(background, e.Bounds);
            if (e.Index >= 0)
            {
                Color ink = highlighted ? SystemColors.HighlightText : Enabled ? ForeColor : SystemColors.GrayText;
                var textBounds = Rectangle.Inflate(e.Bounds, -3, 0);
                TextRenderer.DrawText(e.Graphics, GetItemText(Items[e.Index]), Font, textBounds, ink,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis |
                    TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
            }

            if (field && Focused && ShowFocusCues)
            {
                using var focus = new Pen(Accent);
                e.Graphics.DrawRectangle(focus, Rectangle.Inflate(e.Bounds, -1, -1));
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
        }

        protected override bool ShowFocusCues => false;

        protected override void OnCheckedChanged(EventArgs e)
        {
            Font = Checked ? LabelFont : null;
            ForeColor = Checked ? Accent : Muted;
            base.OnCheckedChanged(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (Checked)
            {
                using var underline = new SolidBrush(Accent);
                e.Graphics.FillRectangle(underline, 0, Height - 3, Width, 3);
            }

            if (Focused && base.ShowFocusCues)
            {
                using var focus = new Pen(Accent);
                e.Graphics.DrawRectangle(focus, Rectangle.Inflate(ClientRectangle, -3, -4));
            }
        }
    }
}
