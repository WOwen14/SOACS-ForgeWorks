using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using SOACSForgeWorks.Core;

namespace SOACSForgeWorks.Pages
{
    public class SettingsPage : UserControl, ILiveRefreshable
    {
        Label status, repositoryInfo;
        TextBox txtOperator, txtRepositoryRoot, txtRefresh;
        ComboBox cboMode, cboProfile;
        CheckBox chkShared;

        public SettingsPage() { Build(); }

        private void Build()
        {
            BackColor = Theme.Background; Padding = new Padding(0);
            Controls.Add(new Label { Text = "Settings", Dock = DockStyle.Top, Height = 58, ForeColor = Theme.Text, Font = new Font("Segoe UI Semibold", 21, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft });

            var panel = new Panel { Dock = DockStyle.Top, Height = 500, BackColor = Theme.Panel, Padding = new Padding(22), Margin = new Padding(0, 0, 0, 12) };
            panel.Paint += (s,e)=>ControlPaint.DrawBorder(e.Graphics, panel.ClientRectangle, Theme.Border, ButtonBorderStyle.Solid);
            Controls.Add(panel); panel.BringToFront();
            panel.Controls.Add(new Label { Text = "Workstation and Repository", Dock = DockStyle.Top, Height = 34, ForeColor = Theme.Text, Font = Theme.HeaderFont });

            var grid = new TableLayoutPanel { Dock = DockStyle.Top, Height = 250, ColumnCount = 2, RowCount = 6, BackColor = Theme.Panel };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            panel.Controls.Add(grid); grid.BringToFront();

            txtOperator = new TextBox { Dock = DockStyle.Fill, Text = InventoryStore.Workstation.OperatorName };
            cboMode = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            cboMode.Items.AddRange(new object[] { "Operator", "Read-Only Viewer" });
            cboMode.SelectedItem = InventoryStore.Workstation.WorkstationMode == "Read-Only Viewer" ? "Read-Only Viewer" : "Operator";
            cboProfile = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            LoadProfileDropdown();
            txtRepositoryRoot = new TextBox { Dock = DockStyle.Fill, Text = InventoryStore.RepositoryRoot };
            chkShared = new CheckBox { Dock = DockStyle.Fill, Text = "Shared/network repository", ForeColor = Theme.Text, Checked = InventoryStore.Workstation.UseSharedDatabase };
            txtRefresh = new TextBox { Dock = DockStyle.Left, Width = 100, Text = InventoryStore.Workstation.AutoRefreshSeconds.ToString() };

            AddRow(grid, 0, "Operator Name", txtOperator);
            AddRow(grid, 1, "Workstation Mode", cboMode);
            AddRow(grid, 2, "Repository Profile", cboProfile);
            AddRow(grid, 3, "Repository Root", txtRepositoryRoot);
            AddRow(grid, 4, "Repository Type", chkShared);
            AddRow(grid, 5, "Auto Refresh Seconds", txtRefresh);

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 46, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
            panel.Controls.Add(buttons); buttons.BringToFront();
            var save = Btn("Save Settings"); save.Click += (s,e)=>SaveSettings(); buttons.Controls.Add(save);
            var switchProfile = Btn("Switch Profile"); switchProfile.Click += (s,e)=>SwitchProfile(); buttons.Controls.Add(switchProfile);
            var browse = Btn("Browse Root"); browse.Click += (s,e)=>BrowseRepositoryRoot(); buttons.Controls.Add(browse);
            var verify = Btn("Verify Repo"); verify.Click += (s,e)=>VerifyRepository(); buttons.Controls.Add(verify);
            var backup = Btn("Create Backup"); backup.Click += (s,e)=>CreateBackup(); buttons.Controls.Add(backup);
            var open = Btn("Open Repository"); open.Click += (s,e)=>OpenFolder(); buttons.Controls.Add(open);
            var reload = Btn("Reload Database"); reload.Click += (s,e)=>{ InventoryStore.Load(); status.Text = "Database reloaded from repository."; }; buttons.Controls.Add(reload);

            repositoryInfo = new Label
            {
                Text = BuildRepositoryInfo(),
                Dock = DockStyle.Top,
                Height = 120,
                ForeColor = Theme.Muted,
                Font = Theme.SmallFont
            };
            panel.Controls.Add(repositoryInfo); repositoryInfo.BringToFront();

            status = new Label { Text = "Ready.", Dock = DockStyle.Fill, ForeColor = Theme.Green, Font = Theme.NormalFont, TextAlign = ContentAlignment.TopLeft };
            panel.Controls.Add(status);
        }


        private void LoadProfileDropdown()
        {
            cboProfile.Items.Clear();
            foreach (var profile in RepositoryManager.LoadProfiles())
                cboProfile.Items.Add(profile.Name);
            string current = string.IsNullOrWhiteSpace(InventoryStore.Workstation.RepositoryProfile) ? "Standalone" : InventoryStore.Workstation.RepositoryProfile;
            if (cboProfile.Items.Contains(current)) cboProfile.SelectedItem = current;
            else if (cboProfile.Items.Count > 0) cboProfile.SelectedIndex = 0;
            cboProfile.SelectedIndexChanged += (s, e) =>
            {
                var profile = RepositoryManager.FindProfile(Convert.ToString(cboProfile.SelectedItem));
                if (profile != null)
                {
                    txtRepositoryRoot.Text = profile.DataRoot;
                    chkShared.Checked = profile.IsNetwork;
                    status.ForeColor = Theme.Muted;
                    status.Text = "Profile selected. Click Switch Profile to reconnect to this repository.";
                }
            };
        }

        private void SwitchProfile()
        {
            try
            {
                string selected = Convert.ToString(cboProfile.SelectedItem);
                if (string.IsNullOrWhiteSpace(selected)) return;
                var profile = RepositoryManager.FindProfile(selected);
                if (profile == null) return;
                var result = MessageBox.Show(this,
                    "Switch repository profile to '" + profile.Name + "'?\n\nRepository Root:\n" + profile.DataRoot + "\n\nForgeWorks will reconnect to the selected repository and reload data.",
                    "Switch Repository Profile", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result != DialogResult.Yes) return;
                RepositoryManager.SwitchProfile(profile.Name);
                txtRepositoryRoot.Text = InventoryStore.RepositoryRoot;
                repositoryInfo.Text = BuildRepositoryInfo();
                status.ForeColor = Theme.Green;
                status.Text = "Repository profile switched to " + profile.Name + ".";
                LiveDataBus.NotifyDataChanged();
                var form = FindForm() as MainForm;
                if (form != null) form.RefreshShellStatus();
            }
            catch (Exception ex)
            {
                status.ForeColor = Color.IndianRed;
                status.Text = "Profile switch failed: " + ex.Message;
            }
        }

        private string BuildRepositoryInfo()
        {
            var health = RepositoryManager.CheckHealth();
            return "Profile: " + (InventoryStore.Workstation == null ? "Standalone" : InventoryStore.Workstation.RepositoryProfile) +
                   "\nRepository root: " + InventoryStore.RepositoryRoot +
                   "\nDatabase file: " + InventoryStore.DataFile +
                   "\nAttachments: " + RepositoryManager.AttachmentsFolder +
                   "\nReports: " + RepositoryManager.ReportsFolder +
                   "\nBackups: " + RepositoryManager.BackupsFolder +
                   "\nFree space: " + RepositoryManager.FormatBytes(health.FreeSpaceBytes) +
                   "\nSettings file: " + InventoryStore.SettingsFile;
        }

        private void AddRow(TableLayoutPanel grid, int row, string label, Control control)
        {
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            grid.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, ForeColor = Theme.Muted, Font = Theme.SmallFont, TextAlign = ContentAlignment.MiddleLeft }, 0, row);
            grid.Controls.Add(control, 1, row);
        }

        private Button Btn(string t) { var b = new Button { Text = t, Width = 130, Height = 36, BackColor = Theme.Panel2, ForeColor = Theme.Text, FlatStyle = FlatStyle.Flat, Margin = new Padding(0, 4, 8, 4) }; b.FlatAppearance.BorderColor = Theme.Border; return b; }

        private void SaveSettings()
        {
            try
            {
                int refresh; if (!int.TryParse(txtRefresh.Text, out refresh)) refresh = 30;
                InventoryStore.SetRepositorySettings(txtOperator.Text, Convert.ToString(cboMode.SelectedItem), txtRepositoryRoot.Text, chkShared.Checked, refresh, Convert.ToString(cboProfile.SelectedItem));
                repositoryInfo.Text = BuildRepositoryInfo();
                status.ForeColor = Theme.Green;
                status.Text = "Repository settings saved. ForgeWorks is now using the selected Data Root.";
            }
            catch (Exception ex) { status.ForeColor = Color.IndianRed; status.Text = "Settings save failed: " + ex.Message; }
        }

        private void BrowseRepositoryRoot()
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select the ForgeWorks Repository Root. This can be a local folder or a network share.";
                if (!string.IsNullOrWhiteSpace(txtRepositoryRoot.Text) && Directory.Exists(txtRepositoryRoot.Text)) fbd.SelectedPath = txtRepositoryRoot.Text;
                if (fbd.ShowDialog(this) == DialogResult.OK) txtRepositoryRoot.Text = fbd.SelectedPath;
            }
        }

        private void VerifyRepository()
        {
            try
            {
                RepositoryManager.EnsureRepository(txtRepositoryRoot.Text);
                var health = RepositoryManager.CheckHealth();
                repositoryInfo.Text = BuildRepositoryInfo();
                status.ForeColor = health.RepositoryAvailable ? Theme.Green : Color.IndianRed;
                status.Text = health.Message;
            }
            catch (Exception ex) { status.ForeColor = Color.IndianRed; status.Text = "Repository verification failed: " + ex.Message; }
        }

        private void CreateBackup()
        {
            try { string path = InventoryStore.CreateBackupCopy(); status.ForeColor = Theme.Green; status.Text = "Backup created:\n" + path; }
            catch (Exception ex) { status.ForeColor = Color.IndianRed; status.Text = "Backup failed: " + ex.Message; }
        }
        private void OpenFolder()
        {
            try { RepositoryManager.EnsureRepository(); Process.Start(InventoryStore.RepositoryRoot); }
            catch (Exception ex) { status.ForeColor = Color.IndianRed; status.Text = "Could not open repository: " + ex.Message; }
        }

        public void RefreshData()
        {
            if (repositoryInfo != null) repositoryInfo.Text = BuildRepositoryInfo();
        }
    }
}
