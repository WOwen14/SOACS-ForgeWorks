using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SOACSForgeWorks.Pages
{
    public class AboutPage : UserControl, ILiveRefreshable
    {
        private PictureBox logo;
        public AboutPage() { Build(); }

        private void Build()
        {
            BackColor = Theme.Background;
            Controls.Clear();
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, BackColor = Theme.Background, Padding = new Padding(14) };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
            Controls.Add(root);

            var left = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Panel, Padding = new Padding(18) };
            root.Controls.Add(left, 0, 0);
            logo = new PictureBox { Dock = DockStyle.Top, Height = 230, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Theme.Panel };
            string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "ForgeWorksLogo.png");
            if (File.Exists(logoPath)) logo.Image = Image.FromFile(logoPath);
            left.Controls.Add(logo);
            left.Controls.Add(new Label { Text = "SOACS\nFORGEWORKS", Dock = DockStyle.Top, Height = 86, ForeColor = Theme.Gold, Font = new Font("Segoe UI Semibold", 22, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter });
            left.Controls.Add(new Label { Text = "Mission Fabrication Operations", Dock = DockStyle.Fill, ForeColor = Theme.Muted, Font = Theme.NormalFont, TextAlign = ContentAlignment.TopCenter });

            var right = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Background, Padding = new Padding(24, 6, 6, 6) };
            root.Controls.Add(right, 1, 0);
            right.Controls.Add(new Label { Text = BuildInfoText(), Dock = DockStyle.Top, Height = 230, ForeColor = Theme.Text, Font = Theme.NormalFont, TextAlign = ContentAlignment.TopLeft });
            right.Controls.Add(new Label { Text = "Operator Resources", Dock = DockStyle.Top, Height = 44, ForeColor = Theme.Gold, Font = new Font("Segoe UI Semibold", 16, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft });

            var buttonFlow = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 90, FlowDirection = FlowDirection.LeftToRight, WrapContents = true, BackColor = Theme.Background };
            right.Controls.Add(buttonFlow);
            var op = Btn("Operator Guide"); op.Click += (s, e) => ShowDoc("OPERATOR_GUIDE.md", DefaultOperatorGuide()); buttonFlow.Controls.Add(op);
            var help = Btn("Help Me"); help.Click += (s, e) => ShowDoc("HELP_ME.md", DefaultHelpMe()); buttonFlow.Controls.Add(help);
            var whats = Btn("What's New"); whats.Click += (s, e) => ShowDoc("WHATS_NEW.md", DefaultWhatsNew()); buttonFlow.Controls.Add(whats);
            var admin = Btn("Admin Guide"); admin.Click += (s, e) => ShowDoc("ADMIN_GUIDE.md", DefaultAdminGuide()); buttonFlow.Controls.Add(admin);

            right.Controls.Add(new Label { Text = "ForgeWorks is part of the SOACS application suite and is designed for offline shop inventory, project material tracking, serialized asset control, reporting, and audit accountability.", Dock = DockStyle.Top, Height = 90, ForeColor = Theme.Muted, Font = Theme.NormalFont, TextAlign = ContentAlignment.TopLeft });

            var footer = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Theme.Background };
            root.Controls.Add(footer, 0, 1); root.SetColumnSpan(footer, 2);
            var openDocs = Btn("Open Docs Folder"); openDocs.Width = 160; openDocs.Click += (s, e) => OpenDocsFolder(); footer.Controls.Add(openDocs);
        }

        private string BuildInfoText()
        {
            return "Version: 3.1.4 RC1 Operational Readiness\r\n" +
                   "Database Version: " + InventoryStore.CurrentDatabaseVersion + "\r\n" +
                   "Database Mode: " + (InventoryStore.Workstation.UseSharedDatabase ? "Shared" : "Local") + "\r\n" +
                   "Workstation Mode: " + InventoryStore.Workstation.WorkstationMode + "\r\n" +
                   "Operator: " + InventoryStore.CurrentOperator + "\r\n" +
                   "Windows User: " + InventoryStore.CurrentDomainUser + "\r\n" +
                   "Workstation: " + InventoryStore.CurrentMachineName + "\r\n" +
                   "Build Date: 2026-07-09\r\n" +
                   "Scanner: Ready";
        }

        private Button Btn(string text)
        {
            var b = new Button { Text = text, Width = 140, Height = 42, BackColor = Theme.Panel2, ForeColor = Theme.Text, FlatStyle = FlatStyle.Flat, Margin = new Padding(8) };
            b.FlatAppearance.BorderColor = Theme.Border;
            return b;
        }

        private string DocsPath { get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "docs"); } }

        private void ShowDoc(string fileName, string fallback)
        {
            try
            {
                string path = Path.Combine(DocsPath, fileName);
                string text = File.Exists(path) ? File.ReadAllText(path) : fallback;
                using (var f = new Form { Text = fileName.Replace("_", " ").Replace(".md", ""), Width = 900, Height = 700, StartPosition = FormStartPosition.CenterScreen, BackColor = Theme.Background })
                {
                    var owner = FindForm();
                    if (owner != null) f.Icon = owner.Icon;
                    var box = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Both, ReadOnly = true, BackColor = Theme.Input, ForeColor = Theme.Text, Font = new Font("Consolas", 10), Text = text, WordWrap = false };
                    f.Controls.Add(box);
                    if (owner != null) f.ShowDialog(owner); else f.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to open " + fileName + ":\r\n" + ex.Message, "ForgeWorks Help", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OpenDocsFolder()
        {
            try { Directory.CreateDirectory(DocsPath); System.Diagnostics.Process.Start(DocsPath); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Open Docs Folder"); }
        }

        private string DefaultOperatorGuide() { return "SOACS ForgeWorks Operator Guide\r\n\r\nUse Operations to scan or type an item identifier. Use Inventory for item records. Use Projects for project parts lists. Use Reports to print, preview, or export reports."; }
        private string DefaultHelpMe() { return "Help Me\r\n\r\n1. Select Operations.\r\n2. Choose an operation.\r\n3. Scan or type a Forge ID, barcode, serial number, part number, or NSN.\r\n4. Press Enter.\r\n\r\nFor reports, select Reports, choose a report type, choose columns, then print or export."; }
        private string DefaultWhatsNew() { return "What's New\r\n\r\nv1.0.0 adds the Reporting & Intelligence Center, print preview, CSV export, saved report templates, Project Parts Lists, transaction reports, audit reports, storage reports, and an About page with Operator Guide, Help Me, and What's New."; }
        private string DefaultAdminGuide() { return "Admin Guide\r\n\r\nUse Administration for categories, locations, projects, and dynamic fields. Use Settings for workstation mode, database path, shared database mode, read-only viewer mode, and backup settings."; }

        public void RefreshData()
        {
            // Static help/about page.
        }

    }
}
