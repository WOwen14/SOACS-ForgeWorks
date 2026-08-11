using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using SOACSForgeWorks.Core;

namespace SOACSForgeWorks
{
    public class FeedbackForm : Form
    {
        private readonly string activeWorkspace;
        private ComboBox category;
        private TextBox comments;
        private CheckBox includeScreenshot;
        private CheckBox includeHealth;
        private CheckBox includeSystem;
        private TextBox details;

        public FeedbackForm(string activeWorkspace)
        {
            this.activeWorkspace = string.IsNullOrWhiteSpace(activeWorkspace) ? "Unknown" : activeWorkspace;
            Text = "Submit ForgeWorks Feedback";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(820, 700);
            MinimumSize = new Size(760, 640);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Theme.Background;
            ForeColor = Theme.Text;
            Font = Theme.NormalFont;
            Build();
            Theme.ApplyToTree(this);
        }

        private void Build()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 6,
                ColumnCount = 1,
                BackColor = Theme.Background,
                Padding = new Padding(16)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 170));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
            Controls.Add(root);

            root.Controls.Add(new Label { Text = "Submit Feedback", Dock = DockStyle.Fill, ForeColor = Theme.Gold, Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft }, 0, 0);

            details = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Text = FeedbackManager.BuildSystemInfo(activeWorkspace), BackColor = Theme.Input, ForeColor = Theme.Text };
            root.Controls.Add(details, 0, 1);

            var top = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, BackColor = Theme.Background };
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 210));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
            root.Controls.Add(top, 0, 2);
            top.Controls.Add(new Label { Text = "Category", Dock = DockStyle.Fill, ForeColor = Theme.Muted, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
            category = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            category.Items.AddRange(new object[] { "Bug", "UI / Layout", "Enhancement", "Performance", "Question" });
            category.SelectedIndex = 0;
            top.Controls.Add(category, 1, 0);
            var copy = Button("Copy Technical Details");
            copy.Dock = DockStyle.Fill;
            copy.Margin = new Padding(8, 4, 0, 4);
            copy.Click += (s, e) => { try { Clipboard.SetText(details.Text); } catch { } };
            top.Controls.Add(copy, 3, 0);

            comments = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical, Text = "Describe what happened, what you expected, and what you were doing when you noticed it." };
            root.Controls.Add(comments, 0, 3);

            // Options and action buttons use separate rows. This avoids the fixed-width
            // collision that clipped controls on 1920x1080 and 125% DPI displays.
            var options = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Theme.Background,
                Padding = new Padding(0, 4, 0, 0)
            };
            includeScreenshot = Check("Include Screenshot", true);
            includeHealth = Check("Repository Health", true);
            includeSystem = Check("System Info", true);
            foreach (var c in new[] { includeScreenshot, includeHealth, includeSystem })
            {
                c.AutoSize = true;
                c.Dock = DockStyle.None;
                c.Margin = new Padding(0, 0, 28, 0);
                options.Controls.Add(c);
            }
            root.Controls.Add(options, 0, 4);

            var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, BackColor = Theme.Background, Padding = new Padding(0, 4, 0, 0) };
            var submit = Button("Submit"); submit.Width = 110; submit.Click += SubmitFeedback; actions.Controls.Add(submit);
            var cancel = Button("Cancel"); cancel.Width = 100; cancel.Click += (s, e) => Close(); actions.Controls.Add(cancel);
            root.Controls.Add(actions, 0, 5);
        }

        private CheckBox Check(string text, bool checkedValue)
        {
            return new CheckBox { Text = text, Checked = checkedValue, Dock = DockStyle.Fill, ForeColor = Theme.Text, TextAlign = ContentAlignment.MiddleLeft };
        }

        private Button Button(string text)
        {
            var b = new Button { Text = text, Height = 36, BackColor = Theme.Panel2, ForeColor = Theme.Text, FlatStyle = FlatStyle.Flat, Margin = new Padding(6) };
            b.FlatAppearance.BorderColor = Theme.Border;
            return b;
        }

        private void SubmitFeedback(object sender, EventArgs e)
        {
            try
            {
                string folder = FeedbackManager.CreateFeedback(Convert.ToString(category.SelectedItem), comments.Text, activeWorkspace, includeScreenshot.Checked, includeHealth.Checked, includeSystem.Checked, Owner as Form);
                var result = MessageBox.Show("Feedback saved:\r\n" + folder + "\r\n\r\nOpen the feedback folder?", "ForgeWorks Feedback", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (result == DialogResult.Yes) Process.Start(new ProcessStartInfo(folder) { UseShellExecute = true });
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to save feedback:\r\n" + ex.Message, "ForgeWorks Feedback", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
