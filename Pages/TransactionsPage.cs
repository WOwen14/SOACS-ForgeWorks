using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SOACSForgeWorks.Pages
{
    public class TransactionsPage : UserControl, ILiveRefreshable
    {
        DataGridView grid;
        private static Dictionary<string, bool> visibleColumns;

        public TransactionsPage() { Build(); LoadGrid(); }

        private void Build()
        {
            BackColor = Theme.Background;
            var top = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = Theme.Background }; Controls.Add(top);
            top.Controls.Add(new Label { Text = "Transactions", Dock = DockStyle.Left, Width = 280, ForeColor = Theme.Text, Font = new Font("Segoe UI Semibold", 21, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft });

            var refresh = new Button { Text = "Refresh", Dock = DockStyle.Right, Width = 110, Height = 34, BackColor = Theme.Panel2, ForeColor = Theme.Text, FlatStyle = FlatStyle.Flat };
            refresh.Click += (s, e) => LoadGrid(); top.Controls.Add(refresh);

            var columns = new Button { Text = "Columns", Dock = DockStyle.Right, Width = 120, Height = 34, BackColor = Theme.Panel2, ForeColor = Theme.Text, FlatStyle = FlatStyle.Flat };
            columns.Click += (s, e) => ShowColumnChooser(); top.Controls.Add(columns);

            grid = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Theme.Background, ForeColor = Color.Black, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            Controls.Add(grid); grid.BringToFront();
        }

        private void EnsureColumnDefaults()
        {
            if (visibleColumns != null) return;
            visibleColumns = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            string[] all = AllColumnNames();
            foreach (string c in all) visibleColumns[c] = true;
            visibleColumns["WindowsUser"] = false;
            visibleColumns["Workstation"] = false;
            visibleColumns["Barcode"] = false;
            visibleColumns["Reason"] = false;
        }

        private string[] AllColumnNames()
        {
            return new string[] { "Time", "Operator", "WindowsUser", "Workstation", "ForgeId", "Barcode", "ItemName", "NSN", "PartNumber", "SerialNumber", "Project", "Category", "Action", "Delta", "OldQty", "NewQty", "From", "To", "Reason", "Notes" };
        }

        private InventoryItem FindItem(TransactionRecord t)
        {
            foreach (var i in InventoryStore.Database.Items)
            {
                if (!string.IsNullOrWhiteSpace(t.ForgeId) && string.Equals(i.ForgeId, t.ForgeId, StringComparison.OrdinalIgnoreCase)) return i;
                if (!string.IsNullOrWhiteSpace(t.Barcode) && string.Equals(i.Barcode, t.Barcode, StringComparison.OrdinalIgnoreCase)) return i;
            }
            return null;
        }

        private void LoadGrid()
        {
            if (grid == null) return;
            EnsureColumnDefaults();
            grid.Columns.Clear(); grid.Rows.Clear();
            string[] all = AllColumnNames();
            foreach (var c in all)
            {
                bool show;
                if (visibleColumns.TryGetValue(c, out show) && show) grid.Columns.Add(c, c);
            }
            foreach (var t in InventoryStore.Database.Transactions)
            {
                InventoryItem item = FindItem(t);
                var values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                values["Time"] = t.Time;
                values["Operator"] = t.Operator;
                values["WindowsUser"] = t.WindowsUser;
                values["Workstation"] = t.Workstation;
                values["ForgeId"] = t.ForgeId;
                values["Barcode"] = t.Barcode;
                values["ItemName"] = t.ItemName;
                values["NSN"] = item == null ? "" : item.Nsn;
                values["PartNumber"] = item == null ? "" : item.PartNumber;
                values["SerialNumber"] = item == null ? "" : item.SerialNumber;
                values["Project"] = t.Project;
                values["Category"] = item == null ? "" : item.Category;
                values["Action"] = t.Action;
                values["Delta"] = t.Delta;
                values["OldQty"] = t.OldQuantity;
                values["NewQty"] = t.NewQuantity;
                values["From"] = t.FromLocation;
                values["To"] = t.ToLocation;
                values["Reason"] = t.Reason;
                values["Notes"] = t.Notes;

                object[] row = new object[grid.Columns.Count];
                for (int i = 0; i < grid.Columns.Count; i++) row[i] = values[grid.Columns[i].Name];
                grid.Rows.Add(row);
            }
        }

        private void ShowColumnChooser()
        {
            EnsureColumnDefaults();
            using (var f = new Form())
            {
                f.Text = "Transaction Columns";
                f.StartPosition = FormStartPosition.CenterParent;
                f.Size = new Size(340, 520);
                f.BackColor = Theme.Background;
                f.ForeColor = Theme.Text;

                var list = new CheckedListBox { Dock = DockStyle.Fill, CheckOnClick = true, BackColor = Color.White, ForeColor = Color.Black };
                foreach (string c in AllColumnNames())
                {
                    bool show = true;
                    visibleColumns.TryGetValue(c, out show);
                    list.Items.Add(c, show);
                }
                f.Controls.Add(list);

                var bottom = new Panel { Dock = DockStyle.Bottom, Height = 48, BackColor = Theme.Panel };
                f.Controls.Add(bottom);
                var ok = new Button { Text = "Apply", DialogResult = DialogResult.OK, Width = 90, Height = 30, Left = 130, Top = 9, BackColor = Theme.Gold, ForeColor = Color.Black, FlatStyle = FlatStyle.Flat };
                var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Width = 90, Height = 30, Left = 225, Top = 9, BackColor = Theme.Panel2, ForeColor = Theme.Text, FlatStyle = FlatStyle.Flat };
                bottom.Controls.Add(ok); bottom.Controls.Add(cancel);
                f.AcceptButton = ok; f.CancelButton = cancel;

                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    for (int i = 0; i < list.Items.Count; i++) visibleColumns[list.Items[i].ToString()] = list.GetItemChecked(i);
                    LoadGrid();
                }
            }
        }

        public void RefreshData()
        {
            LoadGrid();
        }

    }
}
