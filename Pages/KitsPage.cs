using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace SOACSForgeWorks.Pages
{
    public class KitsPage : UserControl, ILiveRefreshable
    {
        private ComboBox kitBox;
        private TextBox searchBox;
        private DataGridView grid;
        private Label statusLabel;

        public KitsPage()
        {
            Build();
        }

        private void Build()
        {
            BackColor = Theme.Background;
            Padding = new Padding(12);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                BackColor = Theme.Background
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            Controls.Add(root);

            root.Controls.Add(new Label
            {
                Text = "Kits",
                Dock = DockStyle.Fill,
                ForeColor = Theme.Text,
                Font = Theme.TitleFont,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);

            var body = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 2,
                BackColor = Theme.Background
            };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 210));
            root.Controls.Add(body, 0, 1);

            var left = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                BackColor = Theme.Background,
                Padding = new Padding(0, 0, 12, 0)
            };
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            body.Controls.Add(left, 0, 0);

            var filters = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 2,
                BackColor = Theme.Background,
                Padding = new Padding(0, 2, 0, 8)
            };
            filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
            filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
            left.Controls.Add(filters, 0, 0);

            kitBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDown, Font = Theme.NormalFont };
            searchBox = new TextBox { Dock = DockStyle.Fill, Font = Theme.NormalFont };
            searchBox.TextChanged += (s, e) => LoadGrid();
            filters.Controls.Add(Wrap("Kit Template", kitBox), 0, 0);
            filters.Controls.Add(Wrap("Search requirements / inventory", searchBox), 1, 0);

            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Theme.Background
            };
            grid.Columns.Add("Kit", "Kit");
            grid.Columns.Add("ForgeId", "Forge ID");
            grid.Columns.Add("Nomenclature", "Nomenclature");
            grid.Columns.Add("PartNumber", "Part #");
            grid.Columns.Add("NSN", "NSN");
            grid.Columns.Add("MRL", "MRL");
            grid.Columns.Add("Required", "Required");
            grid.Columns.Add("OnHand", "On Hand");
            grid.Columns.Add("Available", "Available");
            grid.Columns.Add("Shortage", "Shortage");
            grid.Columns.Add("Status", "Status");
            grid.Columns.Add("Notes", "Notes");
            Theme.ApplyGridTheme(grid);
            left.Controls.Add(grid, 0, 1);

            var actions = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 8,
                ColumnCount = 1,
                BackColor = Theme.Panel,
                Padding = new Padding(12)
            };
            for (int i = 0; i < 8; i++) actions.RowStyles.Add(new RowStyle(SizeType.Absolute, i == 0 ? 34 : 48));
            body.Controls.Add(actions, 1, 0);

            actions.Controls.Add(new Label
            {
                Text = "KIT ACTIONS",
                Dock = DockStyle.Fill,
                ForeColor = Theme.Text,
                Font = Theme.HeaderFont,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);

            var add = Btn("Add Requirement"); add.Click += (s, e) => AddRequirement(); actions.Controls.Add(add, 0, 1);
            var remove = Btn("Remove Selected"); remove.Click += (s, e) => RemoveSelected(); actions.Controls.Add(remove, 0, 2);
            var check = Btn("Check Readiness"); check.Click += (s, e) => CheckReadiness(); actions.Controls.Add(check, 0, 3);
            var export = Btn("Export CSV"); export.Click += (s, e) => ExportCsv(); actions.Controls.Add(export, 0, 4);

            actions.Controls.Add(new Label
            {
                Text = "Build and reserve actions are planned for the next kit workflow pass.",
                Dock = DockStyle.Fill,
                ForeColor = Theme.Muted,
                Font = Theme.SmallFont,
                TextAlign = ContentAlignment.TopLeft
            }, 0, 5);

            statusLabel = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = Theme.Muted,
                BackColor = Theme.Background,
                Font = Theme.NormalFont,
                TextAlign = ContentAlignment.MiddleLeft
            };
            root.Controls.Add(statusLabel, 0, 2);

            kitBox.SelectedIndexChanged += (s, e) => LoadGrid();
            Theme.ApplyToTree(this);
            LoadKits();
            LoadGrid();
        }

        private Control Wrap(string label, Control child)
        {
            var p = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, BackColor = Theme.Background, Padding = new Padding(0, 0, 12, 0) };
            p.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
            p.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            p.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, ForeColor = Theme.Gold, Font = Theme.SmallFont, TextAlign = ContentAlignment.BottomLeft }, 0, 0);
            child.Dock = DockStyle.Fill;
            p.Controls.Add(child, 0, 1);
            return p;
        }

        private Button Btn(string text)
        {
            var b = new Button { Text = text, Dock = DockStyle.Fill, Height = 40, BackColor = Theme.Panel2, ForeColor = Theme.Text, FlatStyle = FlatStyle.Flat, Margin = new Padding(0, 5, 0, 5), Font = Theme.NormalFont };
            b.FlatAppearance.BorderColor = Theme.Border;
            return b;
        }

        private void LoadKits()
        {
            string current = kitBox.Text;
            kitBox.Items.Clear();
            kitBox.Items.Add("All Kits");
            var names = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var b in InventoryStore.KitRequirements("")) if (!string.IsNullOrWhiteSpace(b.KitName)) names.Add(b.KitName);
            foreach (var n in names) kitBox.Items.Add(n);
            if (!string.IsNullOrWhiteSpace(current) && kitBox.Items.Contains(current)) kitBox.SelectedItem = current;
            else kitBox.SelectedIndex = 0;
        }

        private void LoadGrid()
        {
            if (grid == null) return;
            grid.Rows.Clear();
            string kit = kitBox == null ? "" : kitBox.Text;
            if (kit == "All Kits") kit = "";
            string search = searchBox == null ? "" : searchBox.Text.Trim().ToLowerInvariant();
            int ok = 0, shortCount = 0, total = 0;
            foreach (var req in InventoryStore.KitRequirements(kit))
            {
                var item = InventoryStore.FindByScan(req.ForgeId);
                string nomenclature = item == null ? req.ItemName : item.ItemName;
                string blob = (req.KitName + " " + req.ForgeId + " " + nomenclature + " " + (item == null ? "" : item.PartNumber + " " + item.Nsn + " " + item.Mrl)).ToLowerInvariant();
                if (search.Length > 0 && !blob.Contains(search)) continue;
                int onHand = item == null ? 0 : item.Quantity;
                int available = item == null ? 0 : item.Quantity - item.ReservedQuantity;
                int shortage = Math.Max(0, req.RequiredQuantity - available);
                string status = shortage == 0 ? "READY" : "SHORT";
                if (shortage == 0) ok++; else shortCount++;
                total++;
                int row = grid.Rows.Add(req.KitName, req.ForgeId, nomenclature, item == null ? "" : item.PartNumber, item == null ? "" : item.Nsn, item == null ? "" : item.Mrl, req.RequiredQuantity, onHand, available, shortage, status, req.Notes);
                grid.Rows[row].Tag = req;
            }
            statusLabel.Text = total + " requirement(s) shown  •  Ready: " + ok + "  •  Short: " + shortCount;
            Theme.ApplyGridTheme(grid);
        }

        private void AddRequirement()
        {
            if (InventoryStore.IsReadOnly) { MessageBox.Show("Read-only viewer mode cannot add kit requirements.", "Read Only"); return; }
            using (var f = new KitRequirementForm())
            {
                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    LoadKits();
                    LoadGrid();
                }
            }
        }

        private void RemoveSelected()
        {
            if (InventoryStore.IsReadOnly) { MessageBox.Show("Read-only viewer mode cannot remove kit requirements.", "Read Only"); return; }
            if (grid.CurrentRow == null || grid.CurrentRow.Tag == null) return;
            var req = grid.CurrentRow.Tag as BomRecord;
            if (req == null) return;
            if (MessageBox.Show("Remove selected kit requirement?", "Remove Requirement", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            InventoryStore.Database.BomItems.Remove(req);
            InventoryStore.Save();
            LoadKits();
            LoadGrid();
        }

        private void CheckReadiness()
        {
            LoadGrid();
            MessageBox.Show(statusLabel.Text, "Kit Readiness");
        }

        private void ExportCsv()
        {
            using (var sfd = new SaveFileDialog { Filter = "CSV files (*.csv)|*.csv", FileName = "ForgeWorks_Kit_Readiness.csv" })
            {
                if (sfd.ShowDialog(this) != DialogResult.OK) return;
                var sb = new StringBuilder();
                for (int c = 0; c < grid.Columns.Count; c++)
                {
                    if (c > 0) sb.Append(',');
                    sb.Append(Escape(grid.Columns[c].HeaderText));
                }
                sb.AppendLine();
                foreach (DataGridViewRow r in grid.Rows)
                {
                    for (int c = 0; c < grid.Columns.Count; c++)
                    {
                        if (c > 0) sb.Append(',');
                        sb.Append(Escape(Convert.ToString(r.Cells[c].Value)));
                    }
                    sb.AppendLine();
                }
                File.WriteAllText(sfd.FileName, sb.ToString());
            }
        }

        private string Escape(string s)
        {
            s = s ?? "";
            if (s.Contains(",") || s.Contains("\"") || s.Contains("\n")) return "\"" + s.Replace("\"", "\"\"") + "\"";
            return s;
        }

        public void RefreshData()
        {
            LoadKits();
            LoadGrid();
        }

    }
}
