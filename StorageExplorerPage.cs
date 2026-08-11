using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SOACSForgeWorks.Pages
{
    public class StorageExplorerPage : UserControl, ILiveRefreshable
    {
        private TreeView tree;
        private DataGridView grid;
        private Label selectedLabel;
        private Label summaryLabel;
        private TextBox searchBox;

        public StorageExplorerPage()
        {
            Build();
            LoadTree();
            LoadItems(null);
        }

        private void Build()
        {
            BackColor = Theme.Background;
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Theme.Background
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(root);

            root.Controls.Add(new Label
            {
                Text = "Storage Explorer",
                Dock = DockStyle.Fill,
                ForeColor = Theme.Text,
                Font = new Font("Segoe UI Semibold", 20, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);

            var top = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                BackColor = Theme.Panel,
                Padding = new Padding(10, 6, 10, 6)
            };
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
            root.Controls.Add(top, 0, 1);

            selectedLabel = new Label { Dock = DockStyle.Fill, ForeColor = Theme.Gold, Font = Theme.HeaderFont, TextAlign = ContentAlignment.MiddleLeft, Text = "All Storage Locations" };
            top.Controls.Add(selectedLabel, 0, 0);

            searchBox = new TextBox { Dock = DockStyle.Fill };
            searchBox.TextChanged += (s, e) => LoadItems(CurrentLocation());
            top.Controls.Add(searchBox, 1, 0);

            var refresh = ActionButton("Refresh", (s, e) => { LoadTree(); LoadItems(CurrentLocation()); });
            top.Controls.Add(refresh, 2, 0);
            var move = ActionButton("Move Selected", (s, e) => MoveSelected());
            top.Controls.Add(move, 3, 0);

            var body = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Theme.Background,
                Padding = new Padding(0)
            };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 310));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.Controls.Add(body, 0, 2);

            var left = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Panel, Padding = new Padding(10) };
            body.Controls.Add(left, 0, 0);

            var help = new Label
            {
                Dock = DockStyle.Top,
                Height = 58,
                Text = "Browse the configured storage structure. Create and maintain locations under Administration.",
                ForeColor = Theme.Muted,
                Font = Theme.SmallFont,
                TextAlign = ContentAlignment.MiddleLeft
            };
            left.Controls.Add(help);

            tree = new TreeView
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(20, 25, 25),
                ForeColor = Theme.Text,
                BorderStyle = BorderStyle.FixedSingle,
                HideSelection = false,
                Font = Theme.NormalFont
            };
            tree.AfterSelect += (s, e) => LoadItems(CurrentLocation());
            left.Controls.Add(tree);
            tree.BringToFront();

            var right = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, BackColor = Theme.Background, Padding = new Padding(0) };
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            body.Controls.Add(right, 1, 0);

            summaryLabel = new Label { Dock = DockStyle.Fill, ForeColor = Theme.Muted, Font = Theme.SmallFont, TextAlign = ContentAlignment.MiddleLeft };
            right.Controls.Add(summaryLabel, 0, 0);

            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Theme.Background,
                ForeColor = Color.Black,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.None,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 36,
                RowTemplate = { Height = 30 },
                EnableHeadersVisualStyles = false
            };
            grid.ColumnHeadersDefaultCellStyle.BackColor = Theme.Panel2;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Theme.Text;
            grid.ColumnHeadersDefaultCellStyle.Font = Theme.HeaderFont;
            grid.DefaultCellStyle.Font = Theme.NormalFont;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(235, 238, 240);
            grid.DoubleClick += (s, e) => OpenSelected();
            right.Controls.Add(grid, 0, 1);
        }

        private void SetSafeSplitterDistance(SplitContainer split, int preferredDistance)
        {
            if (split == null || split.IsDisposed || split.Width <= 0) return;

            int minDistance = split.Panel1MinSize;
            int maxDistance = split.Width - split.Panel2MinSize - split.SplitterWidth;
            if (maxDistance < minDistance)
            {
                // If the window is temporarily too small during layout, do nothing.
                return;
            }

            int safeDistance = Math.Max(minDistance, Math.Min(preferredDistance, maxDistance));
            if (split.SplitterDistance != safeDistance)
                split.SplitterDistance = safeDistance;
        }

        private Button ActionButton(string text, EventHandler handler)
        {
            var b = new Button { Text = text, Dock = DockStyle.Fill, Height = 30, BackColor = Theme.Panel2, ForeColor = Theme.Text, FlatStyle = FlatStyle.Flat, Margin = new Padding(6,0,0,0) };
            b.FlatAppearance.BorderColor = Theme.Border;
            b.Click += handler;
            return b;
        }

        private void LoadTree()
        {
            if (tree == null) return;
            tree.BeginUpdate();
            tree.Nodes.Clear();
            var root = new TreeNode("All Storage") { Tag = "" };
            tree.Nodes.Add(root);

            foreach (var loc in InventoryStore.Database.Locations)
                AddLocationNode(root, loc.Name);

            root.Expand();
            tree.EndUpdate();
        }

        private void AddLocationNode(TreeNode root, string location)
        {
            if (string.IsNullOrWhiteSpace(location)) return;
            string normalized = location.Replace("\\", "/").Replace(">", "/");
            string[] parts = normalized.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            TreeNode current = root;
            string path = "";
            foreach (string raw in parts)
            {
                string part = raw.Trim();
                if (part.Length == 0) continue;
                path = path.Length == 0 ? part : path + " / " + part;
                TreeNode found = null;
                foreach (TreeNode n in current.Nodes)
                    if (string.Equals(n.Text, part, StringComparison.OrdinalIgnoreCase)) { found = n; break; }
                if (found == null)
                {
                    found = new TreeNode(part) { Tag = path };
                    current.Nodes.Add(found);
                }
                current = found;
            }
        }

        private string CurrentLocation()
        {
            if (tree == null || tree.SelectedNode == null) return "";
            return Convert.ToString(tree.SelectedNode.Tag) ?? "";
        }

        private void LoadItems(string location)
        {
            if (grid == null) return;
            string loc = location ?? "";
            selectedLabel.Text = loc.Length == 0 ? "All Storage Locations" : loc;

            grid.SuspendLayout();
            grid.Columns.Clear();
            grid.Rows.Clear();
            string[] cols = { "ForgeId", "ItemName", "PartNumber", "Category", "Location", "Quantity", "Minimum", "Status", "Project" };
            foreach (var c in cols) grid.Columns.Add(c, c);

            int count = 0;
            int totalQty = 0;
            int low = 0;
            string search = searchBox == null ? "" : searchBox.Text.Trim().ToLowerInvariant();

            foreach (var i in InventoryStore.Database.Items)
            {
                if (loc.Length > 0 && !LocationMatches(i.Location, loc)) continue;
                if (search.Length > 0)
                {
                    string blob = ((i.ForgeId ?? "") + " " + (i.ItemName ?? "") + " " + (i.PartNumber ?? "") + " " + (i.Nsn ?? "") + " " + (i.SerialNumber ?? "") + " " + (i.Category ?? "") + " " + (i.Location ?? "") + " " + (i.Project ?? "")).ToLowerInvariant();
                    if (!blob.Contains(search)) continue;
                }
                grid.Rows.Add(i.ForgeId, i.ItemName, i.PartNumber, i.Category, i.Location, i.Quantity, i.Minimum, InventoryStore.GetStatus(i), i.Project);
                count++;
                totalQty += i.Quantity;
                if (InventoryStore.GetStatus(i) == "Low" || InventoryStore.GetStatus(i) == "Out") low++;
            }

            if (grid.Columns.Count > 0)
            {
                grid.Columns["ForgeId"].FillWeight = 70;
                grid.Columns["ItemName"].FillWeight = 150;
                grid.Columns["Location"].FillWeight = 150;
                grid.Columns["Quantity"].FillWeight = 60;
                grid.Columns["Minimum"].FillWeight = 60;
            }
            summaryLabel.Text = count + " items shown  •  Total quantity: " + totalQty + "  •  Low/Out: " + low + "  •  Double-click an item to edit";
            grid.ResumeLayout();
        }

        private bool LocationMatches(string itemLocation, string selectedLocation)
        {
            if (string.IsNullOrWhiteSpace(selectedLocation)) return true;
            string item = NormalizeLocation(itemLocation);
            string selected = NormalizeLocation(selectedLocation);
            return item.Equals(selected, StringComparison.OrdinalIgnoreCase) || item.StartsWith(selected + " / ", StringComparison.OrdinalIgnoreCase);
        }

        private string NormalizeLocation(string location)
        {
            if (location == null) return "";
            return location.Replace("\\", "/").Replace(">", "/").Replace("/", " / ").Trim();
        }

        private InventoryItem SelectedItem()
        {
            if (grid.CurrentRow == null) return null;
            string id = Convert.ToString(grid.CurrentRow.Cells[0].Value);
            foreach (var i in InventoryStore.Database.Items)
                if (string.Equals(i.ForgeId, id, StringComparison.OrdinalIgnoreCase)) return i;
            return null;
        }

        private void OpenSelected()
        {
            var item = SelectedItem();
            if (item == null) return;
            using (var f = new ItemEditorForm(item, new OperationContext()))
                if (f.ShowDialog() == DialogResult.OK)
                {
                    LoadTree();
                    LoadItems(CurrentLocation());
                }
        }

        private void MoveSelected()
        {
            var item = SelectedItem();
            if (item == null) return;
            string start = CurrentLocation();
            if (string.IsNullOrWhiteSpace(start)) start = item.Location;
            string newLocation = Prompt("Move Item", "Move " + item.ForgeId + " to location/bin:", start);
            if (string.IsNullOrWhiteSpace(newLocation)) return;
            InventoryStore.MoveItem(item, newLocation, "Moved from Storage Explorer");
            LoadTree();
            LoadItems(CurrentLocation());
        }

        private string Prompt(string title, string label, string value)
        {
            using (var f = new Form())
            {
                f.Text = title;
                f.StartPosition = FormStartPosition.CenterParent;
                f.Size = new Size(460, 165);
                f.BackColor = Theme.Background;
                f.Font = Theme.NormalFont;
                var l = new Label { Text = label, Left = 14, Top = 12, Width = 420, Height = 24, ForeColor = Theme.Text };
                var t = new TextBox { Left = 14, Top = 42, Width = 410, Text = value ?? "" };
                var hint = new Label { Text = "Tip: use hierarchy like Forge / Room A / Cabinet 3 / Drawer 2 / Bin D", Left = 14, Top = 70, Width = 420, Height = 20, ForeColor = Theme.Muted, Font = Theme.SmallFont };
                var ok = new Button { Text = "OK", Left = 244, Top = 100, Width = 85, DialogResult = DialogResult.OK };
                var cancel = new Button { Text = "Cancel", Left = 339, Top = 100, Width = 85, DialogResult = DialogResult.Cancel };
                f.Controls.Add(l); f.Controls.Add(t); f.Controls.Add(hint); f.Controls.Add(ok); f.Controls.Add(cancel); f.AcceptButton = ok; f.CancelButton = cancel;
                return f.ShowDialog(this) == DialogResult.OK ? t.Text.Trim() : "";
            }
        }

        public void RefreshData()
        {
            LoadTree();
            LoadItems(CurrentLocation());
        }

    }
}
