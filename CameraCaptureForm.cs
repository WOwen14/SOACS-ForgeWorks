using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SOACSForgeWorks.Core;

namespace SOACSForgeWorks
{
    public class CameraCaptureForm : Form
    {
        private TextBox pathBox;
        private PictureBox previewBox;
        private Label statusLabel;
        private readonly string forgeId;
        public string SelectedPhotoPath { get; private set; }

        public CameraCaptureForm() : this(string.Empty) { }

        public CameraCaptureForm(string forgeId)
        {
            this.forgeId = forgeId ?? string.Empty;
            Text = "Take Item Photo";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(760, 560);
            MinimumSize = new Size(680, 500);
            BackColor = Theme.Background;
            Font = Theme.NormalFont;
            Build();
        }

        private void Build()
        {
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 5, ColumnCount = 1, Padding = new Padding(18), BackColor = Theme.Background };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
            Controls.Add(root);

            root.Controls.Add(new Label { Text = "Item Photo Capture", Dock = DockStyle.Fill, ForeColor = Theme.Text, Font = new Font("Segoe UI Semibold", 18, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
            root.Controls.Add(new Label
            {
                Text = "Click Open Camera, take the picture, then click Use Latest Camera Photo. ForgeWorks will pull the newest image from the Windows Camera Roll, copy it into the ForgeWorks photo folder, and attach it to the item automatically. You can also Browse to an existing image.",
                Dock = DockStyle.Fill,
                ForeColor = Theme.Muted,
                Font = Theme.NormalFont
            }, 0, 1);

            previewBox = new PictureBox { Dock = DockStyle.Fill, BackColor = Theme.Input, BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom };
            root.Controls.Add(previewBox, 0, 2);

            pathBox = new TextBox { Dock = DockStyle.Fill, BackColor = Theme.Input, ForeColor = Theme.Text, BorderStyle = BorderStyle.FixedSingle };
            root.Controls.Add(pathBox, 0, 3);

            var buttons = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 6, RowCount = 1, BackColor = Theme.Background };
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145));
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            root.Controls.Add(buttons, 0, 4);

            statusLabel = new Label { Dock = DockStyle.Fill, Text = "Ready", ForeColor = Theme.Muted, TextAlign = ContentAlignment.MiddleLeft };
            buttons.Controls.Add(statusLabel, 0, 0);
            var camera = MakeButton("Open Camera"); camera.Click += (s, e) => OpenCamera(); buttons.Controls.Add(camera, 1, 0);
            var latest = MakeButton("Use Latest Camera Photo"); latest.Width = 160; latest.Click += (s, e) => UseLatestCameraPhoto(); buttons.Controls.Add(latest, 2, 0);
            var browse = MakeButton("Browse"); browse.Click += (s, e) => Browse(); buttons.Controls.Add(browse, 3, 0);
            var ok = MakeButton("Use Photo"); ok.Click += (s, e) => UsePhoto(); buttons.Controls.Add(ok, 4, 0);
            var cancel = MakeButton("Cancel"); cancel.Click += (s, e) => DialogResult = DialogResult.Cancel; buttons.Controls.Add(cancel, 5, 0);
        }

        private Button MakeButton(string text)
        {
            var b = new Button { Text = text, Width = 105, Height = 34, BackColor = Theme.Panel2, ForeColor = Theme.Text, FlatStyle = FlatStyle.Flat, Margin = new Padding(5, 9, 0, 0) };
            b.FlatAppearance.BorderColor = Theme.Border;
            return b;
        }

        private void OpenCamera()
        {
            try
            {
                Process.Start("microsoft.windows.camera:");
                statusLabel.Text = "Camera opened. Take the picture, then return here and click Use Latest Camera Photo.";
            }
            catch
            {
                MessageBox.Show(this, "Windows Camera could not be opened on this workstation.", "ForgeWorks", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void Browse()
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Select Item Photo";
                ofd.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All Files|*.*";
                if (ofd.ShowDialog(this) == DialogResult.OK) SetCandidatePhoto(ofd.FileName, false);
            }
        }

        private void UseLatestCameraPhoto()
        {
            string latest = FindLatestCameraPhoto();
            if (string.IsNullOrWhiteSpace(latest) || !File.Exists(latest))
            {
                MessageBox.Show(this, "No recent camera photo was found. Take a picture with Windows Camera first, or use Browse.", "ForgeWorks", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            SetCandidatePhoto(latest, false);
            statusLabel.Text = "Latest camera photo loaded. Click Use Photo to attach it to the item.";
        }

        private string FindLatestCameraPhoto()
        {
            string user = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string pictures = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            string[] dirs = new string[]
            {
                Path.Combine(pictures, "Camera Roll"),
                Path.Combine(pictures, "CameraRoll"),
                Path.Combine(user, "OneDrive", "Pictures", "Camera Roll"),
                Path.Combine(user, "OneDrive - Personal", "Pictures", "Camera Roll"),
                pictures
            };
            string[] exts = new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp" };
            return dirs.Where(Directory.Exists)
                       .SelectMany(d => exts.SelectMany(ext => Directory.GetFiles(d, ext)))
                       .OrderByDescending(File.GetLastWriteTime)
                       .FirstOrDefault();
        }

        private void SetCandidatePhoto(string path, bool copied)
        {
            pathBox.Text = path;
            LoadPreview(path);
            statusLabel.Text = copied ? "Photo saved to ForgeWorks photo folder." : "Photo selected. Click Use Photo to attach it to the item.";
        }

        private void LoadPreview(string path)
        {
            try
            {
                if (previewBox.Image != null) { var old = previewBox.Image; previewBox.Image = null; old.Dispose(); }
                if (File.Exists(path))
                {
                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var img = Image.FromStream(fs))
                    {
                        previewBox.Image = new Bitmap(img);
                    }
                }
            }
            catch { }
        }

        private void UsePhoto()
        {
            var p = pathBox.Text.Trim();
            if (p.Length == 0 || !File.Exists(p))
            {
                MessageBox.Show(this, "Select or capture a valid photo first.", "ForgeWorks", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                SelectedPhotoPath = CopyPhotoToForgeWorks(p);
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "The photo could not be attached. " + ex.Message, "ForgeWorks", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private string CopyPhotoToForgeWorks(string sourcePath)
        {
            string stored = PhotoManager.SaveItemPhoto(forgeId, sourcePath);
            if (string.IsNullOrWhiteSpace(stored)) throw new IOException("Photo could not be saved to the ForgeWorks repository.");
            return stored;
        }
    }
}
