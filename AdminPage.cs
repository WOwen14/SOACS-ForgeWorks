using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using SOACSForgeWorks.Core;

namespace SOACSForgeWorks.Pages
{
    public class AdminPage : UserControl, ILiveRefreshable
    {
        ListBox fields, categories, locations;
        TextBox fieldName, categoryName, locationName;
        ComboBox fieldType;

        public AdminPage() { Build(); LoadAll(); }

        private void Build()
        {
            BackColor = Theme.Background;
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1, BackColor = Theme.Background };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 285));
            Controls.Add(root);

            root.Controls.Add(new Label { Text = "Administration", Dock = DockStyle.Fill, ForeColor = Theme.Text, Font = new Font("Segoe UI Semibold", 20, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
            var panels = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, BackColor = Theme.Background, Padding = new Padding(0, 4, 0, 0) };
            panels.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36)); panels.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32)); panels.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
            root.Controls.Add(panels, 0, 1);

            panels.Controls.Add(BuildFieldPanel(), 0, 0);
            panels.Controls.Add(BuildPanel("Categories", "Add category", out categoryName, out categories, AddCategory, DeleteCategory), 1, 0);
            panels.Controls.Add(BuildPanel("Locations", "Add location/bin", out locationName, out locations, AddLocation, DeleteLocation), 2, 0);
            root.Controls.Add(BuildRepositoryPanel(), 0, 2);
        }

        private Control BuildRepositoryPanel()
        {
            var p = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Theme.Panel,
                Padding = new Padding(16),
                Margin = new Padding(6)
            };
            p.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48));
            p.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28));
            p.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24));

            var health = RepositoryManager.CheckHealth();
            var shortRoot = health.RepositoryRoot;
            if (!string.IsNullOrWhiteSpace(shortRoot) && shortRoot.Length > 72)
                shortRoot = shortRoot.Substring(0, 69) + "...";

            var infoPanel = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 8, ColumnCount = 2, BackColor = Theme.Panel, Padding = new Padding(0, 0, 8, 0) };
            infoPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            infoPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            infoPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            for (int i = 1; i < 8; i++) infoPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 27));

            var title = new Label { Text = "Repository Summary", Dock = DockStyle.Fill, ForeColor = Theme.Text, Font = Theme.HeaderFont, TextAlign = ContentAlignment.MiddleLeft };
            infoPanel.Controls.Add(title, 0, 0); infoPanel.SetColumnSpan(title, 2);
            AddRepositoryInfoRow(infoPanel, 1, "Profile", health.ProfileName);
            AddRepositoryInfoRow(infoPanel, 2, "Root", shortRoot, health.RepositoryRoot);
            AddRepositoryInfoRow(infoPanel, 3, "Database", health.DatabaseFolderOk ? "OK" : "Missing");
            AddRepositoryInfoRow(infoPanel, 4, "Attachments", health.AttachmentsFolderOk ? "OK" : "Missing");
            AddRepositoryInfoRow(infoPanel, 5, "Photos / Reports", (health.PhotosFolderOk ? "Photos OK" : "Photos Missing") + "   " + (health.ReportsFolderOk ? "Reports OK" : "Reports Missing"));
            AddRepositoryInfoRow(infoPanel, 6, "Logs / Backups", (health.LogsFolderOk ? "Logs OK" : "Logs Missing") + "   " + (health.BackupsFolderOk ? "Backups OK" : "Backups Missing"));
            AddRepositoryInfoRow(infoPanel, 7, "Free Space", RepositoryManager.FormatBytes(health.FreeSpaceBytes));
            p.Controls.Add(infoPanel, 0, 0);

            var tree = new TreeView { Dock = DockStyle.Fill, BackColor = Theme.Input, ForeColor = Theme.Text, BorderStyle = BorderStyle.FixedSingle, HideSelection = false };
            var rootNode = tree.Nodes.Add("Repository");
            rootNode.Nodes.Add("Database");
            rootNode.Nodes.Add("Attachments");
            rootNode.Nodes.Add("Photos");
            rootNode.Nodes.Add("Reports");
            rootNode.Nodes.Add("Logs");
            rootNode.Nodes.Add("Backups");
            var profilesNode = rootNode.Nodes.Add("Profiles");
            foreach (var profile in RepositoryManager.LoadProfiles()) profilesNode.Nodes.Add(profile.Name);
            rootNode.Expand();
            p.Controls.Add(tree, 1, 0);

            var buttons = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 6, ColumnCount = 1, BackColor = Theme.Panel, Padding = new Padding(8, 0, 0, 0) };
            for (int i = 0; i < 5; i++) buttons.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            buttons.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var open = Button("Open Repository");
            open.Dock = DockStyle.Fill;
            open.Click += (s, e) => { try { Process.Start(new ProcessStartInfo(RepositoryManager.RepositoryRoot) { UseShellExecute = true }); } catch { } };

            var verify = Button("Verify / Repair");
            verify.Dock = DockStyle.Fill;
            verify.Click += (s, e) => { RepositoryManager.EnsureRepository(); MessageBox.Show("Repository verified and missing folders were recreated.", "ForgeWorks"); RefreshData(); };

            var backup = Button("Backup Now");
            backup.Dock = DockStyle.Fill;
            backup.Click += (s, e) => { var path = InventoryStore.CreateBackupCopy(); MessageBox.Show("Backup created:\r\n" + path, "ForgeWorks"); };

            var profiles = Button("Open Profiles");
            profiles.Dock = DockStyle.Fill;
            profiles.Click += (s, e) => { try { Process.Start(new ProcessStartInfo(Path.GetDirectoryName(RepositoryManager.ProfilesFile)) { UseShellExecute = true }); } catch { } };

            var copy = Button("Copy Root Path");
            copy.Dock = DockStyle.Fill;
            copy.Click += (s, e) => { try { Clipboard.SetText(RepositoryManager.RepositoryRoot ?? string.Empty); } catch { } };

            buttons.Controls.Add(open, 0, 0);
            buttons.Controls.Add(verify, 0, 1);
            buttons.Controls.Add(backup, 0, 2);
            buttons.Controls.Add(profiles, 0, 3);
            buttons.Controls.Add(copy, 0, 4);
            p.Controls.Add(buttons, 2, 0);
            return p;
        }

        private void AddRepositoryInfoRow(TableLayoutPanel panel, int row, string label, string value, string tooltip = null)
        {
            var left = new Label { Text = label, Dock = DockStyle.Fill, ForeColor = Theme.Muted, Font = Theme.SmallFont, TextAlign = ContentAlignment.MiddleLeft };
            var right = new Label { Text = value ?? string.Empty, Dock = DockStyle.Fill, ForeColor = Theme.Text, Font = Theme.NormalFont, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true };
            panel.Controls.Add(left, 0, row);
            panel.Controls.Add(right, 1, row);
            if (!string.IsNullOrWhiteSpace(tooltip))
            {
                var tip = new ToolTip();
                tip.SetToolTip(right, tooltip);
            }
        }

        private Control BuildFieldPanel()
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Panel, Padding = new Padding(14), Margin = new Padding(6) };
            p.Controls.Add(new Label { Text = "Custom Fields", Dock = DockStyle.Top, Height = 30, ForeColor = Theme.Text, Font = Theme.HeaderFont });
            p.Controls.Add(new Label { Text = "Fields appear on the item editor and inventory grid immediately.", Dock = DockStyle.Top, Height = 34, ForeColor = Theme.Muted, Font = Theme.SmallFont });
            fieldName = new TextBox { Dock = DockStyle.Top, Height = 28 }; p.Controls.Add(fieldName); fieldName.BringToFront();
            p.Controls.Add(new Label { Text = "Field name", Dock = DockStyle.Top, Height = 20, ForeColor = Theme.Muted, Font = Theme.SmallFont });
            fieldType = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList, Height = 28 };
            fieldType.Items.AddRange(new object[] { "Text", "Number", "Date", "Yes/No" }); fieldType.SelectedIndex = 0;
            p.Controls.Add(fieldType); fieldType.BringToFront();
            p.Controls.Add(new Label { Text = "Field type", Dock = DockStyle.Top, Height = 20, ForeColor = Theme.Muted, Font = Theme.SmallFont });
            var add = Button("Add Field"); add.Dock = DockStyle.Top; add.Click += AddField; p.Controls.Add(add); add.BringToFront();
            var del = Button("Delete Selected Field"); del.Dock = DockStyle.Top; del.Click += DeleteField; p.Controls.Add(del); del.BringToFront();
            fields = new ListBox { Dock = DockStyle.Fill }; p.Controls.Add(fields); fields.BringToFront();
            return p;
        }

        private Control BuildPanel(string title, string inputLabel, out TextBox input, out ListBox list, EventHandler addHandler, EventHandler deleteHandler)
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Panel, Padding = new Padding(14), Margin = new Padding(6) };
            p.Controls.Add(new Label { Text = title, Dock = DockStyle.Top, Height = 30, ForeColor = Theme.Text, Font = Theme.HeaderFont });
            p.Controls.Add(new Label { Text = inputLabel, Dock = DockStyle.Top, Height = 22, ForeColor = Theme.Muted, Font = Theme.SmallFont });
            input = new TextBox { Dock = DockStyle.Top, Height = 28 }; p.Controls.Add(input); input.BringToFront();
            var add = Button("Add"); add.Dock = DockStyle.Top; add.Click += addHandler; p.Controls.Add(add); add.BringToFront();
            var del = Button("Delete Selected"); del.Dock = DockStyle.Top; del.Click += deleteHandler; p.Controls.Add(del); del.BringToFront();
            list = new ListBox { Dock = DockStyle.Fill }; p.Controls.Add(list); list.BringToFront();
            return p;
        }

        private Button Button(string text) { var b = new Button { Text = text, Height = 34, BackColor = Theme.Panel2, ForeColor = Theme.Text, FlatStyle = FlatStyle.Flat, Margin = new Padding(0, 6, 0, 0) }; b.FlatAppearance.BorderColor = Theme.Border; return b; }

        private void LoadAll()
        {
            fields.Items.Clear(); foreach (var f in InventoryStore.Database.CustomFields) fields.Items.Add(f.Name + "  (" + f.Type + ")");
            categories.Items.Clear(); foreach (var c in InventoryStore.Database.Categories) categories.Items.Add(c.Name);
            locations.Items.Clear(); foreach (var l in InventoryStore.Database.Locations) locations.Items.Add(l.Name);
        }

        private void AddField(object sender, EventArgs e)
        {
            string n = fieldName.Text.Trim(); if (n.Length == 0) return;
            foreach (var f in InventoryStore.Database.CustomFields) if (string.Equals(f.Name, n, StringComparison.OrdinalIgnoreCase)) return;
            InventoryStore.Database.CustomFields.Add(new CustomField { Name = n, Type = Convert.ToString(fieldType.SelectedItem) }); InventoryStore.Save(); fieldName.Clear(); LoadAll();
        }
        private void DeleteField(object sender, EventArgs e)
        {
            if (fields.SelectedItem == null) return; string text = fields.SelectedItem.ToString(); string selected = text.Contains("  (") ? text.Substring(0, text.IndexOf("  (")) : text;
            if (MessageBox.Show("Delete custom field '" + selected + "'?\n\nThis also removes that value from all inventory items.", "Delete Field", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            InventoryStore.Database.CustomFields.RemoveAll(f => string.Equals(f.Name, selected, StringComparison.OrdinalIgnoreCase));
            foreach (var i in InventoryStore.Database.Items) i.CustomValues.RemoveAll(v => string.Equals(v.Name, selected, StringComparison.OrdinalIgnoreCase));
            InventoryStore.Save(); LoadAll();
        }
        private void AddCategory(object sender, EventArgs e) { InventoryStore.AddCategory(categoryName.Text); InventoryStore.Save(); categoryName.Clear(); LoadAll(); }
        private void DeleteCategory(object sender, EventArgs e)
        {
            if (categories.SelectedItem == null) return; string selected = categories.SelectedItem.ToString();
            if (MessageBox.Show("Delete category '" + selected + "'?", "Delete Category", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            InventoryStore.Database.Categories.RemoveAll(c => string.Equals(c.Name, selected, StringComparison.OrdinalIgnoreCase)); InventoryStore.Save(); LoadAll();
        }
        private void AddLocation(object sender, EventArgs e) { InventoryStore.AddLocation(locationName.Text); InventoryStore.Save(); locationName.Clear(); LoadAll(); }
        private void DeleteLocation(object sender, EventArgs e)
        {
            if (locations.SelectedItem == null) return; string selected = locations.SelectedItem.ToString();
            if (MessageBox.Show("Delete location '" + selected + "'?", "Delete Location", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            InventoryStore.Database.Locations.RemoveAll(l => string.Equals(l.Name, selected, StringComparison.OrdinalIgnoreCase)); InventoryStore.Save(); LoadAll();
        }

        public void RefreshData()
        {
            LoadAll();
        }

    }
}
