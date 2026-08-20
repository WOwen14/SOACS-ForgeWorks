using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace SOACSForgeWorks.Pages
{
    public class ReportsPage : UserControl, ILiveRefreshable
    {
        private DataGridView grid;
        private ComboBox reportBox;
        private ComboBox projectBox;
        private ComboBox rangeBox;
        private CheckedListBox columnBox;
        private TextBox searchBox;
        private Label titleLabel;
        private string currentReportTitle = "Report";
        private List<string[]> printRows = new List<string[]>();
        private string[] printHeaders = new string[0];
        private int printRowIndex;

        private readonly string[] inventoryColumns = new[] { "Forge ID", "Nomenclature", "Part Number", "CAGE", "NSN", "MRL", "Serial", "Barcode", "Category", "Location", "Project", "Qty", "Reserved", "Available", "Min", "Status", "Preferred Vendor", "Vendor Part Number", "Unit Cost", "Standard Cost", "Last Purchase Price", "Lead Time", "Reorder Qty", "Extended Value", "Est Reorder Cost", "Document" };
        private readonly string[] transactionColumns = new[] { "Time", "Action", "Forge ID", "Item", "NSN", "Part Number", "Project", "Delta", "Old Qty", "New Qty", "From", "To", "Operator", "Windows User", "Workstation", "Notes" };
        private readonly string[] projectColumns = new[] { "Project", "Code", "Status", "Priority", "Owner", "Due Date", "Items", "Reserved Qty", "On Hand Qty", "Notes" };
        private readonly string[] storageColumns = new[] { "Location", "Forge ID", "Item", "NSN", "Part Number", "Project", "Qty", "Status" };
        private readonly string[] auditColumns = new[] { "Time", "Operator", "Windows User", "Workstation", "Action", "Forge ID", "Item", "Notes" };
        private readonly string[] borrowedColumns = new[] { "Forge ID", "Nomenclature", "Part Number", "NSN", "Project", "Borrowed By", "Borrowed Qty", "On Hand", "Reorder", "Location", "Notes" };
        private readonly string[] kitColumns = new[] { "Kit", "Forge ID", "Nomenclature", "Part Number", "NSN", "Required", "On Hand", "Available", "Shortage", "Status", "Notes" };

        public ReportsPage()
        {
            Build();
            LoadFilters();
            ResetColumnsForReport();
            LoadReport();
        }

        private void Build()
        {
            BackColor = Theme.Background;
            Controls.Clear();

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3, BackColor = Theme.Background };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 270));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            Controls.Add(root);

            var header = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Background, Padding = new Padding(0, 0, 0, 8) };
            root.Controls.Add(header, 0, 0); root.SetColumnSpan(header, 2);
            titleLabel = new Label { Text = "Report Center", Dock = DockStyle.Top, Height = 42, ForeColor = Theme.Text, Font = new Font("Segoe UI Semibold", 22, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft };
            header.Controls.Add(titleLabel);
            header.Controls.Add(new Label { Text = "Print reports, export filtered data, and build reusable report layouts.", Dock = DockStyle.Top, Height = 24, ForeColor = Theme.Muted, Font = Theme.SmallFont, TextAlign = ContentAlignment.MiddleLeft });

            var left = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Panel, Padding = new Padding(12) };
            root.Controls.Add(left, 0, 1);

            var leftFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, BackColor = Theme.Panel };
            left.Controls.Add(leftFlow);

            reportBox = new ComboBox { Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            reportBox.Items.AddRange(new object[] { "Entire Inventory", "Filtered Inventory", "Inventory Value", "Low Stock", "Low Inventory Reorder Cost", "Out of Stock", "Project Parts List", "Project Transactions", "All Transactions", "Storage Location Report", "Audit Log", "Borrowed Items", "Kit Requirements / Shortages", "Executive Dashboard" });
            reportBox.SelectedIndex = 0;
            reportBox.SelectedIndexChanged += (s, e) => { ResetColumnsForReport(); LoadReport(); };
            leftFlow.Controls.Add(LabelWrap("Report Type", reportBox));

            rangeBox = new ComboBox { Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            rangeBox.Items.AddRange(new object[] { "Today", "Last 48 Hours", "Last 7 Days", "This Month", "All Time" });
            rangeBox.SelectedIndex = 4;
            rangeBox.SelectedIndexChanged += (s, e) => LoadReport();
            leftFlow.Controls.Add(LabelWrap("Time Range", rangeBox));

            projectBox = new ComboBox { Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            projectBox.SelectedIndexChanged += (s, e) => LoadReport();
            leftFlow.Controls.Add(LabelWrap("Project", projectBox));

            searchBox = new TextBox { Width = 220 };
            searchBox.TextChanged += (s, e) => LoadReport();
            leftFlow.Controls.Add(LabelWrap("Search / Filter", searchBox));

            leftFlow.Controls.Add(new Label { Text = "Columns", Width = 220, Height = 24, ForeColor = Theme.Gold, Font = new Font("Segoe UI Semibold", 10, FontStyle.Bold), Margin = new Padding(0, 12, 0, 0) });
            columnBox = new CheckedListBox { Width = 220, Height = 230, CheckOnClick = true, BackColor = Color.White, ForeColor = Color.Black };
            columnBox.ItemCheck += (s, e) => ScheduleLoadReport();
            leftFlow.Controls.Add(columnBox);

            var reset = Btn("Reset Columns", 220); reset.Click += (s, e) => { ResetColumnsForReport(); LoadReport(); }; leftFlow.Controls.Add(reset);
            var saveTemplate = Btn("Save Template", 220); saveTemplate.Click += (s, e) => SaveTemplate(); leftFlow.Controls.Add(saveTemplate);
            var loadTemplate = Btn("Load Template", 220); loadTemplate.Click += (s, e) => LoadTemplate(); leftFlow.Controls.Add(loadTemplate);
            var addKit = Btn("Add Kit Requirement", 220); addKit.Click += (s, e) => AddKitRequirement(); leftFlow.Controls.Add(addKit);

            grid = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Theme.Background, ForeColor = Color.Black, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells, ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, RowHeadersVisible = false };
            root.Controls.Add(grid, 1, 1);

            var footer = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, BackColor = Theme.Background };
            root.Controls.Add(footer, 0, 2); root.SetColumnSpan(footer, 2);
            var refresh = Btn("Refresh", 120); refresh.Click += (s, e) => { LoadFilters(); LoadReport(); }; footer.Controls.Add(refresh);
            var csv = Btn("Export CSV", 120); csv.Click += (s, e) => ExportCsv(); footer.Controls.Add(csv);
            var preview = Btn("Print Preview", 130); preview.Click += (s, e) => PrintPreview(); footer.Controls.Add(preview);
            var print = Btn("Print", 100); print.Click += (s, e) => PrintReport(); footer.Controls.Add(print);
        }

        private void LoadFilters()
        {
            string selected = projectBox == null ? "All Projects" : Convert.ToString(projectBox.SelectedItem);
            projectBox.Items.Clear();
            projectBox.Items.Add("All Projects");
            foreach (var p in InventoryStore.Database.Projects) if (!string.IsNullOrWhiteSpace(p.Name)) projectBox.Items.Add(p.Name);
            int idx = projectBox.Items.IndexOf(selected); projectBox.SelectedIndex = idx >= 0 ? idx : 0;
        }

        private Control LabelWrap(string label, Control child)
        {
            var p = new Panel { Width = 236, Height = 54, BackColor = Theme.Panel, Margin = new Padding(0, 0, 0, 8) };
            p.Controls.Add(new Label { Text = label, Dock = DockStyle.Top, Height = 18, ForeColor = Theme.Muted, Font = Theme.SmallFont });
            child.Dock = DockStyle.Bottom; p.Controls.Add(child); child.BringToFront();
            return p;
        }

        private Button Btn(string text, int width)
        {
            var b = new Button { Text = text, Width = width, Height = 34, BackColor = Theme.Panel2, ForeColor = Theme.Text, FlatStyle = FlatStyle.Flat, Margin = new Padding(6, 6, 0, 0) };
            b.FlatAppearance.BorderColor = Theme.Border;
            return b;
        }

        private string[] ColumnsForCurrentReport()
        {
            string r = Convert.ToString(reportBox.SelectedItem);
            if (r == "All Transactions" || r == "Project Transactions") return transactionColumns;
            if (r == "Project Parts List") return inventoryColumns;
            if (r == "Storage Location Report") return storageColumns;
            if (r == "Audit Log") return auditColumns;
            if (r == "Borrowed Items") return borrowedColumns;
            if (r == "Kit Requirements / Shortages") return kitColumns;
            if (r == "Executive Dashboard") return new[] { "Metric", "Value" };
            return inventoryColumns;
        }

        private void ResetColumnsForReport()
        {
            if (columnBox == null) return;
            columnBox.Items.Clear();
            foreach (string c in ColumnsForCurrentReport())
            {
                bool check = true;
                if (c == "Windows User" || c == "Workstation" || c == "Barcode") check = false;
                columnBox.Items.Add(c, check);
            }
        }

        private List<string> SelectedColumns()
        {
            var cols = new List<string>();
            foreach (var item in columnBox.CheckedItems) cols.Add(Convert.ToString(item));
            if (cols.Count == 0) cols.AddRange(ColumnsForCurrentReport());
            return cols;
        }


        private void ScheduleLoadReport()
        {
            if (IsDisposed) return;
            if (IsHandleCreated)
            {
                BeginInvoke((Action)LoadReport);
            }
            else
            {
                Load += (s, e) => LoadReport();
            }
        }

        private void LoadReport()
        {
            if (grid == null || reportBox == null || columnBox == null) return;
            grid.Columns.Clear(); grid.Rows.Clear();
            string report = Convert.ToString(reportBox.SelectedItem);
            string project = projectBox == null ? "All Projects" : Convert.ToString(projectBox.SelectedItem);
            string search = searchBox == null ? "" : searchBox.Text.Trim().ToLowerInvariant();
            currentReportTitle = report;
            if (!string.IsNullOrWhiteSpace(project) && project != "All Projects") currentReportTitle += " - " + project;
            titleLabel.Text = "Report Center - " + currentReportTitle;

            var columns = SelectedColumns();
            foreach (var c in columns) grid.Columns.Add(SafeKey(c), c);

            if (report == "All Transactions" || report == "Project Transactions")
            {
                foreach (var t in InventoryStore.Database.Transactions)
                {
                    if (!WithinRange(t.Time)) continue;
                    if (project != "All Projects" && !EqualsIgnore(t.Project, project)) continue;
                    if (!MatchesSearch(TransactionBlob(t), search)) continue;
                    AddRow(columns, c => TransactionValue(t, c));
                }
                return;
            }
            if (report == "Audit Log")
            {
                foreach (var a in InventoryStore.Database.AuditLog)
                {
                    if (!WithinRange(a.Time)) continue;
                    if (!MatchesSearch(AuditBlob(a), search)) continue;
                    AddRow(columns, c => AuditValue(a, c));
                }
                return;
            }
            if (report == "Borrowed Items")
            {
                foreach (var i in InventoryStore.BorrowedItems())
                {
                    if (project != "All Projects" && !EqualsIgnore(i.BorrowedBy, project) && !EqualsIgnore(i.Project, project)) continue;
                    if (!MatchesSearch(ItemBlob(i), search)) continue;
                    AddRow(columns, c => BorrowedValue(i, c));
                }
                return;
            }
            if (report == "Kit Requirements / Shortages")
            {
                string kitFilter = project == "All Projects" ? "" : project;
                foreach (var b in InventoryStore.KitRequirements(kitFilter))
                {
                    InventoryItem item = FindItemByForgeId(b.ForgeId);
                    if (!MatchesSearch((b.KitName + " " + b.ItemName + " " + (item == null ? "" : ItemBlob(item))), search)) continue;
                    AddRow(columns, c => KitValue(b, item, c));
                }
                return;
            }
            if (report == "Executive Dashboard")
            {
                AddMetric("Total Inventory Items", InventoryStore.Database.Items.Count.ToString());
                AddMetric("Low Stock Items", CountItems(i => i.Quantity <= i.Minimum).ToString());
                AddMetric("Out of Stock Items", CountItems(i => i.Quantity <= 0).ToString());
                AddMetric("Active Projects", CountProjects(p => EqualsIgnore(p.Status, "Active")).ToString());
                AddMetric("Inventory Value", TotalInventoryValue().ToString("C"));
                AddMetric("Estimated Low Stock Reorder Cost", TotalLowStockReorderCost().ToString("C"));
                AddMetric("Transactions In Range", CountTransactions(t => WithinRange(t.Time)).ToString());
                AddMetric("Issued In Range", CountTransactions(t => WithinRange(t.Time) && t.Action.ToUpperInvariant().Contains("ISSUE")).ToString());
                AddMetric("Received In Range", CountTransactions(t => WithinRange(t.Time) && t.Action.ToUpperInvariant().Contains("RECEIVE")).ToString());
                return;
            }
            if (report == "Storage Location Report")
            {
                foreach (var i in InventoryStore.Database.Items)
                {
                    if (project != "All Projects" && !EqualsIgnore(i.Project, project)) continue;
                    if (!MatchesSearch(ItemBlob(i), search)) continue;
                    AddRow(columns, c => StorageValue(i, c));
                }
                return;
            }
            foreach (var i in InventoryStore.Database.Items)
            {
                if (project != "All Projects" && !EqualsIgnore(i.Project, project)) continue;
                if ((report == "Low Stock" || report == "Low Inventory Reorder Cost") && !(i.Quantity <= i.Minimum)) continue;
                if (report == "Inventory Value" && i.Quantity <= 0) continue;
                if (report == "Out of Stock" && i.Quantity > 0) continue;
                if (!MatchesSearch(ItemBlob(i), search)) continue;
                AddRow(columns, c => ItemValue(i, c));
            }
        }

        private void AddMetric(string metric, string value)
        {
            if (grid.Columns.Count == 0) { grid.Columns.Add("Metric", "Metric"); grid.Columns.Add("Value", "Value"); }
            grid.Rows.Add(metric, value);
        }

        private int CountItems(Predicate<InventoryItem> match) { int n = 0; foreach (var i in InventoryStore.Database.Items) if (match(i)) n++; return n; }
        private int CountProjects(Predicate<ProjectRecord> match) { int n = 0; foreach (var p in InventoryStore.Database.Projects) if (match(p)) n++; return n; }
        private int CountTransactions(Predicate<TransactionRecord> match) { int n = 0; foreach (var t in InventoryStore.Database.Transactions) if (match(t)) n++; return n; }
        private decimal TotalInventoryValue() { decimal n = 0m; foreach (var i in InventoryStore.Database.Items) n += i.Quantity * i.Cost; return n; }
        private decimal TotalLowStockReorderCost() { decimal n = 0m; foreach (var i in InventoryStore.Database.Items) if (i.Quantity <= i.Minimum) n += EstimatedReorderCost(i); return n; }
        private decimal EstimatedReorderCost(InventoryItem i) { if (i == null) return 0m; int target = i.ReorderQuantity > 0 ? i.ReorderQuantity : i.Minimum; int needed = Math.Max(0, target - i.Quantity); return needed * i.Cost; }

        private bool WithinRange(DateTime time)
        {
            string r = rangeBox == null ? "All Time" : Convert.ToString(rangeBox.SelectedItem);
            DateTime now = DateTime.Now;
            if (r == "Today") return time.Date == now.Date;
            if (r == "Last 48 Hours") return time >= now.AddHours(-48);
            if (r == "Last 7 Days") return time >= now.AddDays(-7);
            if (r == "This Month") return time.Year == now.Year && time.Month == now.Month;
            return true;
        }

        private bool MatchesSearch(string blob, string search) { return string.IsNullOrWhiteSpace(search) || (blob ?? "").ToLowerInvariant().Contains(search); }
        private bool EqualsIgnore(string a, string b) { return string.Equals(a ?? "", b ?? "", StringComparison.OrdinalIgnoreCase); }
        private string SafeKey(string s) { return (s ?? "").Replace(" ", "_").Replace("/", "_"); }

        private void AddRow(List<string> columns, Func<string, object> getValue)
        {
            object[] row = new object[columns.Count];
            for (int i = 0; i < columns.Count; i++) row[i] = getValue(columns[i]);
            grid.Rows.Add(row);
        }

        private string ItemBlob(InventoryItem i) { return (i.ForgeId + " " + i.ItemName + " " + i.PartNumber + " " + i.CageCode + " " + i.Nsn + " " + i.Mrl + " " + i.SerialNumber + " " + i.Barcode + " " + i.Category + " " + i.Location + " " + i.Project + " " + i.Vendor + " " + i.PreferredVendor + " " + i.VendorPartNumber + " " + i.Manufacturer + " " + i.ManufacturerPartNumber + " " + i.Notes); }
        private string TransactionBlob(TransactionRecord t) { return (t.Time + " " + t.Action + " " + t.ForgeId + " " + t.ItemName + " " + t.Project + " " + t.Operator + " " + t.WindowsUser + " " + t.Workstation + " " + t.Notes); }
        private string AuditBlob(AuditRecord a) { return (a.Time + " " + a.Operator + " " + a.WindowsUser + " " + a.Workstation + " " + a.Action + " " + a.ForgeId + " " + a.ItemName + " " + a.Notes); }

        private object ItemValue(InventoryItem i, string c)
        {
            switch (c)
            {
                case "Forge ID": return i.ForgeId; case "Item": return i.ItemName; case "Nomenclature": return i.ItemName; case "Part Number": return i.PartNumber; case "CAGE": return i.CageCode; case "NSN": return i.Nsn; case "MRL": return i.Mrl; case "Serial": return i.SerialNumber; case "Barcode": return i.Barcode; case "Category": return i.Category; case "Location": return i.Location; case "Project": return i.Project; case "Qty": return i.Quantity; case "Reserved": return i.ReservedQuantity; case "Available": return i.AvailableQuantity; case "Min": return i.Minimum; case "Status": return InventoryStore.GetStatus(i); case "Vendor": return string.IsNullOrWhiteSpace(i.PreferredVendor) ? i.Vendor : i.PreferredVendor; case "Preferred Vendor": return string.IsNullOrWhiteSpace(i.PreferredVendor) ? i.Vendor : i.PreferredVendor; case "Vendor Part Number": return i.VendorPartNumber; case "Unit Cost": return i.Cost.ToString("C"); case "Cost": return i.Cost.ToString("C"); case "Standard Cost": return i.StandardCost.ToString("C"); case "Last Purchase Price": return i.LastPurchasePrice.ToString("C"); case "Lead Time": return i.LeadTimeDays; case "Reorder Qty": return i.ReorderQuantity; case "Extended Value": return (i.Quantity * i.Cost).ToString("C"); case "Est Reorder Cost": return EstimatedReorderCost(i).ToString("C"); case "Document": return string.IsNullOrWhiteSpace(i.DocumentPath) ? "" : Path.GetFileName(i.DocumentPath); default: return "";
            }
        }
        private object StorageValue(InventoryItem i, string c) { if (c == "Location") return i.Location; return ItemValue(i, c); }
        private InventoryItem FindItemByForgeId(string forgeId)
        {
            foreach (var i in InventoryStore.Database.Items)
                if (EqualsIgnore(i.ForgeId, forgeId)) return i;
            return null;
        }

        private object TransactionValue(TransactionRecord t, string c)
        {
            InventoryItem item = FindItemByForgeId(t.ForgeId);
            switch (c)
            {
                case "Time": return t.Time; case "Action": return t.Action; case "Forge ID": return t.ForgeId; case "Item": return t.ItemName; case "NSN": return item == null ? "" : item.Nsn; case "Part Number": return item == null ? "" : item.PartNumber; case "Project": return t.Project; case "Delta": return t.Delta; case "Old Qty": return t.OldQuantity; case "New Qty": return t.NewQuantity; case "From": return t.FromLocation; case "To": return t.ToLocation; case "Operator": return t.Operator; case "Windows User": return t.WindowsUser; case "Workstation": return t.Workstation; case "Notes": return t.Notes; default: return "";
            }
        }
        private object AuditValue(AuditRecord a, string c)
        {
            switch (c)
            {
                case "Time": return a.Time; case "Operator": return a.Operator; case "Windows User": return a.WindowsUser; case "Workstation": return a.Workstation; case "Action": return a.Action; case "Forge ID": return a.ForgeId; case "Item": return a.ItemName; case "Notes": return a.Notes; default: return "";
            }
        }


        private object BorrowedValue(InventoryItem i, string c)
        {
            switch (c)
            {
                case "Forge ID": return i.ForgeId;
                case "Nomenclature": return i.ItemName;
                case "Part Number": return i.PartNumber;
                case "NSN": return i.Nsn;
                case "Project": return i.Project;
                case "Borrowed By": return i.BorrowedBy;
                case "Borrowed Qty": return i.BorrowedQuantity;
                case "On Hand": return i.Quantity;
                case "Reorder": return i.ReorderRequired ? "YES" : "";
                case "Location": return i.Location;
                case "Notes": return i.Notes;
                default: return "";
            }
        }

        private object KitValue(BomRecord b, InventoryItem item, string c)
        {
            int onHand = item == null ? 0 : item.Quantity;
            int available = item == null ? 0 : item.AvailableQuantity;
            int shortage = Math.Max(0, b.RequiredQuantity - available);
            switch (c)
            {
                case "Kit": return string.IsNullOrWhiteSpace(b.KitName) ? b.Project : b.KitName;
                case "Forge ID": return b.ForgeId;
                case "Nomenclature": return item == null ? b.ItemName : item.ItemName;
                case "Part Number": return item == null ? "" : item.PartNumber;
                case "NSN": return item == null ? "" : item.Nsn;
                case "Required": return b.RequiredQuantity;
                case "On Hand": return onHand;
                case "Available": return available;
                case "Shortage": return shortage;
                case "Status": return shortage > 0 ? "SHORT" : "OK";
                case "Notes": return b.Notes;
                default: return "";
            }
        }

        private void AddKitRequirement()
        {
            if (InventoryStore.IsReadOnly) { MessageBox.Show(this, "Read-Only Viewer mode cannot add kit requirements."); return; }
            using (var f = new KitRequirementForm())
            {
                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    reportBox.SelectedItem = "Kit Requirements / Shortages";
                    LoadReport();
                }
            }
        }

        private void SaveTemplate()
        {
            using (var sfd = new SaveFileDialog { Filter = "ForgeWorks Report Template (*.fwrpt)|*.fwrpt", FileName = "ForgeWorks_Report_Template.fwrpt" })
            {
                if (sfd.ShowDialog() != DialogResult.OK) return;
                var sb = new StringBuilder();
                sb.AppendLine(Convert.ToString(reportBox.SelectedItem));
                sb.AppendLine(Convert.ToString(rangeBox.SelectedItem));
                sb.AppendLine(Convert.ToString(projectBox.SelectedItem));
                foreach (string c in SelectedColumns()) sb.AppendLine(c);
                File.WriteAllText(sfd.FileName, sb.ToString());
            }
        }

        private void LoadTemplate()
        {
            using (var ofd = new OpenFileDialog { Filter = "ForgeWorks Report Template (*.fwrpt)|*.fwrpt" })
            {
                if (ofd.ShowDialog() != DialogResult.OK) return;
                string[] lines = File.ReadAllLines(ofd.FileName);
                if (lines.Length < 3) return;
                SelectCombo(reportBox, lines[0]); SelectCombo(rangeBox, lines[1]); SelectCombo(projectBox, lines[2]);
                ResetColumnsForReport();
                for (int i = 0; i < columnBox.Items.Count; i++) columnBox.SetItemChecked(i, false);
                for (int l = 3; l < lines.Length; l++) for (int i = 0; i < columnBox.Items.Count; i++) if (Convert.ToString(columnBox.Items[i]) == lines[l]) columnBox.SetItemChecked(i, true);
                LoadReport();
            }
        }

        private void SelectCombo(ComboBox box, string value) { int idx = box.Items.IndexOf(value); if (idx >= 0) box.SelectedIndex = idx; }

        private void ExportCsv()
        {
            using (var sfd = new SaveFileDialog { Filter = "CSV File (*.csv)|*.csv", FileName = currentReportTitle.Replace(" ", "_") + ".csv" })
            {
                if (sfd.ShowDialog() != DialogResult.OK) return;
                var sb = new StringBuilder();
                for (int c = 0; c < grid.Columns.Count; c++) { if (c > 0) sb.Append(','); sb.Append(Csv(grid.Columns[c].HeaderText)); } sb.AppendLine();
                foreach (DataGridViewRow r in grid.Rows)
                {
                    if (r.IsNewRow) continue;
                    for (int c = 0; c < grid.Columns.Count; c++) { if (c > 0) sb.Append(','); sb.Append(Csv(Convert.ToString(r.Cells[c].Value))); }
                    sb.AppendLine();
                }
                File.WriteAllText(sfd.FileName, sb.ToString());
            }
        }
        private string Csv(string s) { s = s ?? ""; return "\"" + s.Replace("\"", "\"\"") + "\""; }

        private void PreparePrintData()
        {
            var headers = new List<string>(); foreach (DataGridViewColumn c in grid.Columns) headers.Add(c.HeaderText); printHeaders = headers.ToArray();
            printRows.Clear();
            foreach (DataGridViewRow r in grid.Rows)
            {
                if (r.IsNewRow) continue;
                string[] vals = new string[grid.Columns.Count];
                for (int i = 0; i < grid.Columns.Count; i++) vals[i] = Convert.ToString(r.Cells[i].Value);
                printRows.Add(vals);
            }
            printRowIndex = 0;
        }

        private PrintDocument CreatePrintDocument()
        {
            PreparePrintData();
            var doc = new PrintDocument();
            doc.DocumentName = "SOACS ForgeWorks - " + currentReportTitle;
            doc.PrintPage += PrintPage;
            return doc;
        }

        private void PrintPreview()
        {
            using (var doc = CreatePrintDocument())
            using (var preview = new PrintPreviewDialog { Document = doc, Width = 1100, Height = 800 }) preview.ShowDialog();
        }

        private void PrintReport()
        {
            using (var doc = CreatePrintDocument())
            using (var dlg = new PrintDialog { Document = doc, UseEXDialog = true }) if (dlg.ShowDialog() == DialogResult.OK) doc.Print();
        }

        private void PrintPage(object sender, PrintPageEventArgs e)
        {
            int x = e.MarginBounds.Left, y = e.MarginBounds.Top;
            var titleFont = new Font("Segoe UI Semibold", 14, FontStyle.Bold);
            var font = new Font("Segoe UI", 8);
            e.Graphics.DrawString("SOACS ForgeWorks - " + currentReportTitle, titleFont, Brushes.Black, x, y); y += 28;
            e.Graphics.DrawString("Generated: " + DateTime.Now + "    Operator: " + InventoryStore.CurrentOperator, font, Brushes.Black, x, y); y += 24;
            string header = string.Join(" | ", printHeaders);
            e.Graphics.DrawString(header, font, Brushes.Black, x, y); y += 18;
            e.Graphics.DrawLine(Pens.Black, x, y, e.MarginBounds.Right, y); y += 8;
            while (printRowIndex < printRows.Count)
            {
                string line = string.Join(" | ", printRows[printRowIndex]);
                if (line.Length > 180) line = line.Substring(0, 180) + "...";
                e.Graphics.DrawString(line, font, Brushes.Black, x, y);
                y += 16; printRowIndex++;
                if (y > e.MarginBounds.Bottom - 20) { e.HasMorePages = true; return; }
            }
            e.HasMorePages = false;
        }

        public void RefreshData()
        {
            LoadFilters();
            LoadReport();
        }

    }
}
