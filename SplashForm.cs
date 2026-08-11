using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SOACSForgeWorks
{
    public class SplashForm : Form
    {
        private Timer timer;
        private ProgressBar progress;
        private Label status;
        private readonly Stopwatch stopwatch = new Stopwatch();
        private const int MinimumSplashMilliseconds = 4200;
        private const int ReadyHoldMilliseconds = 1800;

        public SplashForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(680, 420);
            BackColor = Theme.Header;

            var border = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Header, Padding = new Padding(2) };
            Controls.Add(border);
            var body = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Background, Padding = new Padding(28) };
            border.Controls.Add(body);

            const int contentWidth = 624;
            var logo = new PictureBox { Size = new Size(150, 150), SizeMode = PictureBoxSizeMode.Zoom, Location = new Point((contentWidth - 150) / 2, 28) };
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "ForgeWorksLogo.png");
            if (File.Exists(path)) logo.Image = Image.FromFile(path);
            body.Controls.Add(logo);

            var title = new Label { Text = "SOACS FORGEWORKS", ForeColor = Theme.Gold, Font = new Font("Segoe UI Semibold", 30, FontStyle.Bold), AutoSize = false, TextAlign = ContentAlignment.MiddleCenter, Location = new Point(0, 186), Size = new Size(contentWidth, 55) };
            body.Controls.Add(title);

            var sub = new Label { Text = "MISSION FABRICATION OPERATIONS", ForeColor = Theme.Muted, Font = Theme.HeaderFont, AutoSize = false, TextAlign = ContentAlignment.MiddleCenter, Location = new Point(0, 238), Size = new Size(contentWidth, 30) };
            body.Controls.Add(sub);

            var tag = new Label { Text = "SCAN  •  TRACK  •  BUILD  •  ISSUE", ForeColor = Theme.Gold, Font = Theme.HeaderFont, AutoSize = false, TextAlign = ContentAlignment.MiddleCenter, Location = new Point(0, 282), Size = new Size(contentWidth, 30) };
            body.Controls.Add(tag);

            status = new Label { Text = "Initializing...", ForeColor = Theme.Muted, Font = Theme.SmallFont, AutoSize = false, TextAlign = ContentAlignment.MiddleCenter, Location = new Point(0, 322), Size = new Size(contentWidth, 20) };
            body.Controls.Add(status);

            progress = new ProgressBar { Location = new Point((contentWidth - 285) / 2, 348), Size = new Size(285, 14), Minimum = 0, Maximum = 100, Value = 0 };
            body.Controls.Add(progress);

            var copy = new Label { Text = "v3.1.4 RC1  |  © 2026 SOACS", ForeColor = Theme.Muted, Font = Theme.SmallFont, AutoSize = false, TextAlign = ContentAlignment.MiddleCenter, Location = new Point(0, 368), Size = new Size(contentWidth, 22) };
            body.Controls.Add(copy);

            timer = new Timer { Interval = 50 };
            timer.Tick += Timer_Tick;
            Shown += (s, e) => { stopwatch.Start(); timer.Start(); };
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            int elapsed = (int)stopwatch.ElapsedMilliseconds;
            int value = Math.Min(100, (elapsed * 100) / MinimumSplashMilliseconds);
            progress.Value = value;

            if (value < 15) status.Text = "Initializing application shell...";
            else if (value < 30) status.Text = "Loading database services...";
            else if (value < 45) status.Text = "Loading inventory and projects...";
            else if (value < 60) status.Text = "Applying SOACS theme...";
            else if (value < 75) status.Text = "Preparing live data engine...";
            else if (value < 92) status.Text = "Stabilizing workspace layout...";
            else status.Text = "Ready.";

            if (elapsed >= MinimumSplashMilliseconds)
            {
                progress.Value = 100;
                status.Text = "Ready.";
                if (elapsed >= MinimumSplashMilliseconds + ReadyHoldMilliseconds)
                {
                    timer.Stop();
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
        }
    }
}
