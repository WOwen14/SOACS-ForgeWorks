using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SOACSForgeWorks.Pages
{
    public class InventoryPage : UserControl, ILiveRefreshable
    {
        DataGridView grid;
        TextBox searchBox;
        ComboBox categoryFilter, locationFilter, statusFilter;
        Label resultLabel;
        Label totalItemsLabel, totalValueLabel, lowItemsLabel, borrowedItemsLabel;
        Label summaryTitle, summaryId, summaryQty, summaryLocation, summaryProject, summaryStatus, summaryCost, summaryVendor, summaryAttachments;
        PictureBox summaryPhoto;
        ListView attachmentList;
        string quickFilter = "All";
        HashSet<string> hiddenColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        bool loadingGrid;

        public InventoryPage()
        {
            LoadHiddenColumns();
            Build();
            LoadFilters();
            LoadGrid();
            UpdateSummary(null);
        }

        private void Build()
        {
            BackColor = Theme.Background;
            Padding = new Padding(0);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Theme.Background,
                Padding = new Padding(0)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 96));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(root);

            var title = new Label
            {
                Text = "Inventory Workspace",
                Dock = DockStyle.Fill,
                ForeColor = Theme.Text,
                Font = new Font("Segoe UI Semibold", 20, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            root.Controls.Add(title, 0, 0);

            var top = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 5,
                BackColor = Theme.Panel,
                Padding = new Padding(10, 8, 10, 8),
                Margin = new Padding(0, 0, 0, 8)
            };
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14));
            root.Controls.Add(top, 0, 1);

            searchBox = new TextBox { Dock = DockStyle.Fill, Height = 30 };
            searchBox.TextChanged += (s, e) => LoadGrid();
            top.Controls.Add(LabelWrap("Global Search: Forge ID, nomenclature, NSN, MRL, CAGE, vendor, notes", searchBox), 0, 0);

            categoryFilter = Combo(); categoryFilter.SelectedIndexChanged += (s, e) => LoadGrid();
            top.Controls.Add(LabelWrap("Category", categoryFilter), 1, 0);

            locationFilter = Combo(); locationFilter.SelectedIndexChanged += (s, e) => LoadGrid();
            top.Controls.Add(LabelWrap("Location", locationFilter), 2, 0);

            statusFilter = Combo(); statusFilter.Items.AddRange(new object[] { "All", "Available", "Low", "Out", "Reserved" }); statusFilter.SelectedIndex = 0; statusFilter.SelectedIndexChanged += (s, e) => LoadGrid();
            top.Controls.Add(LabelWrap("Status", statusFilter), 3, 0);

            var rightTop = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, BackColor = Theme.Panel, Margin = new Padding(4, 0, 4, 0) };
            rightTop.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            rightTop.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            resultLabel = new Label { Dock = DockStyle.Fill, ForeColor = Theme.Gold, Font = Theme.SmallFont, TextAlign = ContentAlignment.MiddleRight };
            var chooser = StyledButton("Columns", 92, 28);
            chooser.Dock = DockStyle.Right;
            chooser.Click += (s, e) => ShowColumnChooser();
            rightTop.Controls.Add(resultLabel, 0, 0);
            rightTop.Controls.Add(chooser, 0, 1);
            top.Controls.Add(rightTop, 4, 0);

            var main = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Theme.Background,
                Padding = new Padding(0)
            };
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68));
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
            root.Controls.Add(main, 0, 2);

            var left = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = Theme.Background, Padding = new Padding(0, 0, 8, 0) };
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            main.Controls.Add(left, 0, 0);

            var statPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, BackColor = Theme.Background, Margin = new Padding(0, 0, 0, 8) };
            statPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            statPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            statPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            statPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            totalItemsLabel = StatCard(statPanel, "Parts", "0", 0);
            totalValueLabel = StatCard(statPanel, "Inventory Value", "$0.00", 1);
            lowItemsLabel = StatCard(statPanel, "Low Inventory", "0", 2);
            borrowedItemsLabel = StatCard(statPanel, "Borrowed", "0", 3);
            left.Controls.Add(statPanel, 0, 0);

            var quickPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Theme.Background,
                Padding = new Padding(0, 2, 0, 4),
                Margin = new Padding(0)
            };
            left.Controls.Add(quickPanel, 0, 1);
            AddQuickFilter(quickPanel, "All");
            AddQuickFilter(quickPanel, "Low Inventory");
            AddQuickFilter(quickPanel, "Borrowed");
            AddQuickFilter(quickPanel, "Needs Reorder");
            AddQuickFilter(quickPanel, "Has Documents");
            AddQuickFilter(quickPanel, "Has Photo");
            AddQuickFilter(quickPanel, "No Vendor");
            AddQuickFilter(quickPanel, "Out");

            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.None,
                EnableHeadersVisualStyles = false
            };
            Theme.ApplyGridTheme(grid);
            grid.SelectionChanged += (s, e) => { if (!loadingGrid) UpdateSummary(SelectedItem()); };
            grid.DoubleClick += (s, e) => EditSelected();
            grid.CellMouseDown += Grid_CellMouseDown;
            grid.ContextMenuStrip = BuildContextMenu();
            left.Controls.Add(grid, 0, 2);

            var right = BuildSummaryPane();
            main.Controls.Add(right, 1, 0);
        }

        private Label StatCard(TableLayoutPanel parent, string caption, string value, int column)
        {
            var card = CardPanel();
            card.Margin = new Padding(column == 0 ? 0 : 6, 0, column == 3 ? 0 : 6, 0);
            var c = new Label { Text = caption.ToUpperInvariant(), Dock = DockStyle.Top, Height = 22, ForeColor = Theme.Muted, Font = Theme.SmallFont, TextAlign = ContentAlignment.MiddleLeft };
            var v = new Label { Text = value, Dock = DockStyle.Fill, ForeColor = Theme.Gold, Font = new Font("Segoe UI Semibold", 16, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft };
            card.Controls.Add(v);
            card.Controls.Add(c);
            parent.Controls.Add(card, column, 0);
            return v;
        }

        private Control BuildSummaryPane()
        {
            var right = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, BackColor = Theme.Background, Padding = new Padding(8, 0, 0, 0) };
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 240));
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 118));
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 118));
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));

            var itemCard = CardPanel();
            itemCard.Margin = new Padding(0, 0, 0, 8);
            var itemLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Theme.Panel, Padding = new Padding(0) };
            itemLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            itemLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            summaryPhoto = new PictureBox { Dock = DockStyle.Fill, BackColor = Theme.Input, SizeMode = PictureBoxSizeMode.Zoom, Margin = new Padding(0, 0, 10, 0) };
            itemLayout.Controls.Add(summaryPhoto, 0, 0);
            var details = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 7, ColumnCount = 1, BackColor = Theme.Panel };
            for (int i = 0; i < 7; i++) details.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / 7f));
            summaryTitle = DetailLabel("Select an inventory item");
            summaryId = DetailLabel("Forge ID:");
            summaryQty = DetailLabel("Qty:");
            summaryLocation = DetailLabel("Location:");
            summaryProject = DetailLabel("Project:");
            summaryStatus = DetailLabel("Status:");
            summaryAttachments = DetailLabel("Documents:");
            details.Controls.Add(summaryTitle, 0, 0);
            details.Controls.Add(summaryId, 0, 1);
            details.Controls.Add(summaryQty, 0, 2);
            details.Controls.Add(summaryLocation, 0, 3);
            details.Controls.Add(summaryProject, 0, 4);
            details.Controls.Add(summaryStatus, 0, 5);
            details.Controls.Add(summaryAttachments, 0, 6);
            itemLayout.Controls.Add(details, 1, 0);
            itemCard.Controls.Add(itemLayout);
            right.Controls.Add(itemCard, 0, 0);

            var procCard = CardPanel();
            procCard.Margin = new Padding(0, 0, 0, 8);
            procCard.Controls.Add(CardTitle("Procurement"));
            var procBody = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1, BackColor = Theme.Panel, Padding = new Padding(0, 26, 0, 0) };
            procBody.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            procBody.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            procBody.RowStyles.Add(new RowStyle(SizeType.Percent, 34));
            summaryCost = DetailLabel("Unit Cost:");
            summaryVendor = DetailLabel("Vendor:");
            procBody.Controls.Add(summaryCost, 0, 0);
            procBody.Controls.Add(summaryVendor, 0, 1);
            procBody.Controls.Add(DetailLabel("Lead Time / Last Purchase:"), 0, 2);
            procCard.Controls.Add(procBody);
            right.Controls.Add(procCard, 0, 1);

            var docCard = CardPanel();
            docCard.Margin = new Padding(0, 0, 0, 8);
            docCard.Controls.Add(CardTitle("Documents"));
            attachmentList = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, BackColor = Theme.Input, ForeColor = Theme.Text, BorderStyle = BorderStyle.None, Font = Theme.SmallFont, HeaderStyle = ColumnHeaderStyle.Nonclickable, Margin = new Padding(0), MultiSelect = false };
            attachmentList.Columns.Add("File", 190);
            attachmentList.Columns.Add("Type", 54);
            attachmentList.Columns.Add("Date", 70);
            attachmentList.DoubleClick += (s, e) => OpenSelectedAttachment();
            var docBody = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 26, 0, 0), BackColor = Theme.Panel };
            docBody.Controls.Add(attachmentList);
            docCard.Controls.Add(docBody);
            right.Controls.Add(docCard, 0, 2);

            var hintCard = CardPanel();
            hintCard.Margin = new Padding(0, 0, 0, 8);
            hintCard.Controls.Add(CardTitle("Quick Actions"));
            var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, BackColor = Theme.Panel, Padding = new Padding(0, 30, 0, 0) };
            AddSideAction(actions, "Open Item", (s, e) => EditSelected());
            AddSideAction(actions, "Open Documents", (s, e) => OpenSelectedAttachment());
            AddSideAction(actions, "Receive", (s, e) => OpenTransaction("Receive"));
            AddSideAction(actions, "Issue", (s, e) => OpenTransaction("Issue"));
            AddSideAction(actions, "Adjust Qty", (s, e) => OpenAdjustQuantity());
            AddSideAction(actions, "Refresh", (s, e) => RefreshAll());
            hintCard.Controls.Add(actions);
            right.Controls.Add(hintCard, 0, 3);

            var footer = new Label { Text = "Tip: double-click an inventory row to open the Part Workspace. Right-click for common actions.", Dock = DockStyle.Fill, ForeColor = Theme.Muted, Font = Theme.SmallFont, TextAlign = ContentAlignment.MiddleLeft, BackColor = Theme.Background };
            right.Controls.Add(footer, 0, 4);
            return right;
        }

        private Panel CardPanel()
        {
            return new Panel { Dock = DockStyle.Fill, BackColor = Theme.Panel, Padding = new Padding(10), BorderStyle = BorderStyle.FixedSingle };
        }

        private Label CardTitle(string text)
        {
            return new Label { Text = text.ToUpperInvariant(), Dock = DockStyle.Top, Height = 24, ForeColor = Theme.Gold, BackColor = Theme.Panel, Font = Theme.HeaderFont, TextAlign = ContentAlignment.MiddleLeft };
        }

        private Label DetailLabel(string text)
        {
            return new Label { Text = text, Dock = DockStyle.Fill, ForeColor = Theme.Text, Font = Theme.SmallFont, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true };
        }

        private void AddSideAction(FlowLayoutPanel panel, string text, EventHandler handler)
        {
            var b = StyledButton(text, 150, 30);
            b.Margin = new Padding(0, 0, 0, 6);
            bool writeAction = text != "Open Item" && text != "Open Documents" && text != "Refresh";
            if (InventoryStore.IsReadOnly && writeAction) b.Enabled = false;
            b.Click += handler;
            panel.Controls.Add(b);
        }

        private ComboBox Combo()
        {
            var c = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            c.BackColor = Theme.Input;
            c.ForeColor = Theme.Text;
            c.FlatStyle = FlatStyle.Flat;
            return c;
        }

        private Control LabelWrap(string label, Control child)
        {
            var p = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, BackColor = Theme.Panel, Margin = new Padding(4, 0, 4, 0) };
            p.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            p.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            p.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, ForeColor = Theme.Muted, Font = Theme.SmallFont, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true }, 0, 0);
            p.Controls.Add(child, 0, 1);
            return p;
        }

        private Button StyledButton(string text, int width, int height)
        {
            var b = new Button { Text = text, Width = width, Height = height, BackColor = Theme.Panel2, ForeColor = Theme.Text, FlatStyle = FlatStyle.Flat, Font = Theme.SmallFont };
            b.FlatAppearance.BorderColor = Theme.Border;
            b.FlatAppearance.MouseOverBackColor = Theme.InputFocus;
            return b;
        }

        private void AddQuickFilter(FlowLayoutPanel panel, string text)
        {
            var b = StyledButton(text, Math.Max(96, text.Length * 8 + 22), 34);
            b.Margin = new Padding(0, 0, 8, 0);
            b.Click += (s, e) => { quickFilter = text; LoadGrid(); };
            panel.Controls.Add(b);
        }

        private ContextMenuStrip BuildContextMenu()
        {
            var menu = new ContextMenuStrip();
            menu.BackColor = Theme.Panel2;
            menu.ForeColor = Theme.Text;
            menu.Items.Add("Open Item", null, (s, e) => EditSelected());
            menu.Items.Add("Receive", null, (s, e) => OpenTransaction("Receive"));
            menu.Items.Add("Issue", null, (s, e) => OpenTransaction("Issue"));
            menu.Items.Add("Borrow", null, (s, e) => OpenTransaction("Borrow"));
            menu.Items.Add("Move", null, (s, e) => OpenTransaction("Move"));
            menu.Items.Add("Adjust Quantity", null, (s, e) => OpenAdjustQuantity());
            menu.Items.Add("Open Documents", null, (s, e) => OpenSelectedAttachment());
            menu.Items.Add("Column Chooser", null, (s, e) => ShowColumnChooser());
            return menu;
        }

        private void Grid_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
            {
                grid.ClearSelection();
                grid.Rows[e.RowIndex].Selected = true;
                grid.CurrentCell = grid.Rows[e.RowIndex].Cells[Math.Max(0, e.ColumnIndex)];
                UpdateSummary(SelectedItem());
            }
        }

        private void LoadFilters()
        {
            object cat = categoryFilter.SelectedItem, loc = locationFilter.SelectedItem, st = statusFilter.SelectedItem;
            categoryFilter.Items.Clear(); categoryFilter.Items.Add("All"); foreach (var c in InventoryStore.Database.Categories) categoryFilter.Items.Add(c.Name); categoryFilter.SelectedItem = cat ?? "All"; if (categoryFilter.SelectedIndex < 0) categoryFilter.SelectedIndex = 0;
            locationFilter.Items.Clear(); locationFilter.Items.Add("All"); foreach (var l in InventoryStore.Database.Locations) locationFilter.Items.Add(l.Name); locationFilter.SelectedItem = loc ?? "All"; if (locationFilter.SelectedIndex < 0) locationFilter.SelectedIndex = 0;
            if (st != null) statusFilter.SelectedItem = st;
        }

        private void RefreshAll()
        {
            LoadFilters();
            LoadGrid();
            UpdateSummary(SelectedItem());
        }

        private void LoadGrid()
        {
            if (grid == null) return;
            loadingGrid = true;
            try
            {
                string selectedId = SelectedItem() == null ? null : SelectedItem().ForgeId;
                grid.SuspendLayout();
                grid.Columns.Clear(); grid.Rows.Clear();
                string[] cols = { "ForgeId", "Nomenclature", "PartNumber", "CAGE", "NSN", "MRL", "Quantity", "Available", "Minimum", "Status", "Project", "Location", "UnitCost", "Value", "Vendor", "Documents", "Photo", "LastUpdated" };
                foreach (var c in cols) grid.Columns.Add(c, HeaderText(c));
                int count = 0, low = 0, borrowed = 0;
                decimal totalValue = 0m;
                foreach (var i in InventoryStore.Database.Items)
                {
                    if (!Matches(i)) continue;
                    var status = InventoryStore.GetStatus(i);
                    if (status == "Low" || status == "Out") low++;
                    if (i.BorrowedItem || i.BorrowedQuantity > 0) borrowed++;
                    decimal value = i.Quantity * i.Cost;
                    totalValue += value;
                    int docs = InventoryStore.GetItemAttachments(i.ForgeId).Count;
                    string photo = string.IsNullOrWhiteSpace(i.PhotoPath) ? "" : Path.GetFileName(i.PhotoPath);
                    int row = grid.Rows.Add(i.ForgeId, i.ItemName, i.PartNumber, i.CageCode, i.Nsn, i.Mrl, i.Quantity, i.AvailableQuantity, i.Minimum, status, i.Project, i.Location, Money(i.Cost), Money(value), i.PreferredVendor ?? i.Vendor, docs, photo, DisplayDate(i.LastUpdated));
                    grid.Rows[row].Tag = i;
                    count++;
                }
                ApplyColumnWeights();
                ApplyColumnVisibility();
                if (resultLabel != null) resultLabel.Text = count + " shown";
                if (totalItemsLabel != null) totalItemsLabel.Text = InventoryStore.Database.Items.Count.ToString();
                if (totalValueLabel != null) totalValueLabel.Text = Money(totalValue);
                if (lowItemsLabel != null) lowItemsLabel.Text = low.ToString();
                if (borrowedItemsLabel != null) borrowedItemsLabel.Text = borrowed.ToString();
                if (!string.IsNullOrWhiteSpace(selectedId)) SelectRowByForgeId(selectedId);
                grid.ResumeLayout();
            }
            finally
            {
                loadingGrid = false;
            }
            UpdateSummary(SelectedItem());
        }

        private string HeaderText(string name)
        {
            if (name == "ForgeId") return "Forge ID";
            if (name == "PartNumber") return "Part #";
            if (name == "UnitCost") return "Unit Cost";
            if (name == "LastUpdated") return "Updated";
            return name;
        }

        private void ApplyColumnWeights()
        {
            SetWeight("ForgeId", 70); SetWeight("Nomenclature", 180); SetWeight("PartNumber", 90); SetWeight("CAGE", 65); SetWeight("NSN", 95);
            SetWeight("MRL", 90); SetWeight("Quantity", 58); SetWeight("Available", 65); SetWeight("Minimum", 58); SetWeight("Status", 75);
            SetWeight("Project", 100); SetWeight("Location", 115); SetWeight("UnitCost", 70); SetWeight("Value", 78); SetWeight("Vendor", 105);
            SetWeight("Documents", 70); SetWeight("Photo", 75); SetWeight("LastUpdated", 90);
        }

        private void SetWeight(string name, float weight)
        {
            if (grid.Columns.Contains(name)) grid.Columns[name].FillWeight = weight;
        }

        private void ApplyColumnVisibility()
        {
            foreach (DataGridViewColumn c in grid.Columns) c.Visible = !hiddenColumns.Contains(c.Name);
        }

        private void ShowColumnChooser()
        {
            if (grid == null || grid.Columns.Count == 0) return;
            using (var f = new Form())
            {
                f.Text = "Inventory Column Chooser";
                f.StartPosition = FormStartPosition.CenterParent;
                f.Size = new Size(340, 460);
                f.BackColor = Theme.Background;
                f.Font = Theme.NormalFont;
                var list = new CheckedListBox { Dock = DockStyle.Fill, BackColor = Theme.Input, ForeColor = Theme.Text, CheckOnClick = true, BorderStyle = BorderStyle.FixedSingle };
                foreach (DataGridViewColumn c in grid.Columns) list.Items.Add(c.Name + " - " + c.HeaderText, c.Visible);
                var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 44, FlowDirection = FlowDirection.RightToLeft, BackColor = Theme.Background };
                var ok = StyledButton("OK", 80, 30); ok.DialogResult = DialogResult.OK;
                var cancel = StyledButton("Cancel", 80, 30); cancel.DialogResult = DialogResult.Cancel;
                bottom.Controls.Add(ok); bottom.Controls.Add(cancel);
                f.Controls.Add(list); f.Controls.Add(bottom); f.AcceptButton = ok; f.CancelButton = cancel;
                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    hiddenColumns.Clear();
                    for (int idx = 0; idx < list.Items.Count; idx++)
                    {
                        var text = list.Items[idx].ToString();
                        var name = text.Split(new string[] { " - " }, StringSplitOptions.None)[0];
                        if (!list.GetItemChecked(idx)) hiddenColumns.Add(name);
                    }
                    SaveHiddenColumns();
                    ApplyColumnVisibility();
                }
            }
        }

        private void LoadHiddenColumns()
        {
            hiddenColumns.Clear();
            string raw = InventoryStore.Workstation == null ? "" : InventoryStore.Workstation.InventoryHiddenColumns;
            if (string.IsNullOrWhiteSpace(raw)) return;
            foreach (var part in raw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)) hiddenColumns.Add(part.Trim());
        }

        private void SaveHiddenColumns()
        {
            if (InventoryStore.Workstation == null) return;
            InventoryStore.Workstation.InventoryHiddenColumns = string.Join(";", new List<string>(hiddenColumns).ToArray());
            InventoryStore.SaveWorkstationSettings();
        }

        private void SelectRowByForgeId(string forgeId)
        {
            foreach (DataGridViewRow r in grid.Rows)
            {
                var item = r.Tag as InventoryItem;
                if (item != null && item.ForgeId == forgeId)
                {
                    r.Selected = true;
                    grid.CurrentCell = r.Cells[0];
                    break;
                }
            }
        }

        private void UpdateSummary(InventoryItem item)
        {
            if (summaryTitle == null) return;
            if (item == null)
            {
                summaryTitle.Text = "Select an inventory item";
                summaryId.Text = "Forge ID:"; summaryQty.Text = "Qty:"; summaryLocation.Text = "Location:"; summaryProject.Text = "Project:"; summaryStatus.Text = "Status:"; summaryCost.Text = "Unit Cost:"; summaryVendor.Text = "Vendor:"; summaryAttachments.Text = "Documents:";
                if (summaryPhoto != null) summaryPhoto.Image = null;
                if (attachmentList != null) attachmentList.Items.Clear();
                return;
            }
            summaryTitle.Text = item.ItemName ?? "Unnamed Item";
            summaryId.Text = "Forge ID: " + (item.ForgeId ?? "");
            summaryQty.Text = "Qty / Available: " + item.Quantity + " / " + item.AvailableQuantity;
            summaryLocation.Text = "Location: " + (item.Location ?? "");
            summaryProject.Text = "Project: " + (item.Project ?? "");
            summaryStatus.Text = "Status: " + InventoryStore.GetStatus(item);
            summaryCost.Text = "Unit Cost / Value: " + Money(item.Cost) + " / " + Money(item.Cost * item.Quantity);
            summaryVendor.Text = "Vendor: " + (string.IsNullOrWhiteSpace(item.PreferredVendor) ? item.Vendor : item.PreferredVendor);
            var attachments = InventoryStore.GetItemAttachments(item.ForgeId);
            summaryAttachments.Text = "Documents: " + attachments.Count;
            LoadPhoto(item);
            LoadAttachments(attachments);
        }

        private void LoadPhoto(InventoryItem item)
        {
            try
            {
                if (summaryPhoto.Image != null) { var old = summaryPhoto.Image; summaryPhoto.Image = null; old.Dispose(); }
                string path = item == null ? "" : item.PhotoPath;
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                {
                    using (var img = Image.FromFile(path)) summaryPhoto.Image = new Bitmap(img);
                }
            }
            catch { if (summaryPhoto != null) summaryPhoto.Image = null; }
        }

        private void LoadAttachments(List<AttachmentRecord> attachments)
        {
            if (attachmentList == null) return;
            attachmentList.Items.Clear();
            foreach (var a in attachments)
            {
                var li = new ListViewItem(a.FileName ?? a.OriginalFileName ?? "Document");
                li.SubItems.Add(a.FileType ?? "");
                li.SubItems.Add(a.Added == DateTime.MinValue ? "" : a.Added.ToString("M/d/yy"));
                li.Tag = a;
                attachmentList.Items.Add(li);
            }
        }

        private void OpenSelectedAttachment()
        {
            if (attachmentList == null || attachmentList.SelectedItems.Count == 0) return;
            var rec = attachmentList.SelectedItems[0].Tag as AttachmentRecord;
            if (rec == null) return;
            string path = InventoryStore.ResolveAttachmentPath(rec);
            if (!File.Exists(path)) { MessageBox.Show("Attachment file not found:\n" + path); return; }
            try { Process.Start(new ProcessStartInfo(path) { UseShellExecute = true }); }
            catch (Exception ex) { MessageBox.Show("Unable to open attachment.\n" + ex.Message); }
        }

        private string DisplayDate(DateTime dt)
        {
            if (dt == DateTime.MinValue) return "";
            return dt.ToString("M/d/yy HH:mm");
        }

        private string Money(decimal value)
        {
            return value.ToString("C");
        }

        private bool Matches(InventoryItem i)
        {
            string s = searchBox == null ? "" : searchBox.Text.Trim().ToLowerInvariant();
            if (s.Length > 0)
            {
                string custom = "";
                foreach (var v in i.CustomValues) custom += " " + (v.Value ?? "");
                string blob = ((i.ForgeId ?? "") + " " + (i.Barcode ?? "") + " " + (i.ItemName ?? "") + " " + (i.PartNumber ?? "") + " " + (i.CageCode ?? "") + " " + (i.Nsn ?? "") + " " + (i.Mrl ?? "") + " " + (i.SerialNumber ?? "") + " " + (i.Project ?? "") + " " + (i.BorrowedBy ?? "") + " " + (i.DocumentPath ?? "") + " " + (i.Notes ?? "") + " " + (i.Vendor ?? "") + " " + (i.PreferredVendor ?? "") + " " + (i.VendorPartNumber ?? "") + " " + (i.Manufacturer ?? "") + " " + (i.ManufacturerPartNumber ?? "") + custom).ToLowerInvariant();
                if (!blob.Contains(s)) return false;
            }
            if (categoryFilter != null && Convert.ToString(categoryFilter.SelectedItem) != "All" && !string.Equals(i.Category, Convert.ToString(categoryFilter.SelectedItem), StringComparison.OrdinalIgnoreCase)) return false;
            if (locationFilter != null && Convert.ToString(locationFilter.SelectedItem) != "All" && !string.Equals(i.Location, Convert.ToString(locationFilter.SelectedItem), StringComparison.OrdinalIgnoreCase)) return false;
            if (statusFilter != null && Convert.ToString(statusFilter.SelectedItem) != "All" && !string.Equals(InventoryStore.GetStatus(i), Convert.ToString(statusFilter.SelectedItem), StringComparison.OrdinalIgnoreCase)) return false;
            if (quickFilter == "Low Inventory" && InventoryStore.GetStatus(i) != "Low") return false;
            if (quickFilter == "Out" && InventoryStore.GetStatus(i) != "Out") return false;
            if (quickFilter == "Borrowed" && !(i.BorrowedItem || i.BorrowedQuantity > 0)) return false;
            if (quickFilter == "Needs Reorder" && !i.ReorderRequired) return false;
            if (quickFilter == "Has Documents" && InventoryStore.GetItemAttachments(i.ForgeId).Count == 0 && string.IsNullOrWhiteSpace(i.DocumentPath)) return false;
            if (quickFilter == "Has Photo" && string.IsNullOrWhiteSpace(i.PhotoPath)) return false;
            if (quickFilter == "No Vendor" && !string.IsNullOrWhiteSpace(i.Vendor) && !string.IsNullOrWhiteSpace(i.PreferredVendor)) return false;
            return true;
        }

        private InventoryItem SelectedItem()
        {
            if (grid == null || grid.CurrentRow == null) return null;
            var item = grid.CurrentRow.Tag as InventoryItem;
            if (item != null) return item;
            string id = Convert.ToString(grid.CurrentRow.Cells[0].Value);
            foreach (var i in InventoryStore.Database.Items) if (i.ForgeId == id) return i;
            return null;
        }

        private void AddItem()
        {
            if (InventoryStore.IsReadOnly) { MessageBox.Show("Read-Only Viewer mode cannot add inventory."); return; }
            using (var f = new ItemEditorForm(null, new OperationContext()))
                if (f.ShowDialog() == DialogResult.OK) RefreshAll();
        }

        private void EditSelected()
        {
            var item = SelectedItem(); if (item == null) return;
            using (var f = new ItemEditorForm(item, new OperationContext())) if (f.ShowDialog() == DialogResult.OK) RefreshAll();
        }

        private void OpenTransaction(string defaultAction)
        {
            if (InventoryStore.IsReadOnly) { MessageBox.Show("Read-Only Viewer mode cannot perform inventory transactions."); return; }
            var item = SelectedItem(); if (item == null) return;
            using (var f = new InventoryTransactionForm(item, defaultAction))
                if (f.ShowDialog(this) == DialogResult.OK) RefreshAll();
        }

        private void OpenAdjustQuantity()
        {
            if (InventoryStore.IsReadOnly) { MessageBox.Show("Read-Only Viewer mode cannot adjust inventory."); return; }
            var item = SelectedItem(); if (item == null) return;
            using (var f = new AdjustQuantityForm(item))
                if (f.ShowDialog(this) == DialogResult.OK) RefreshAll();
        }

        private void DeleteSelected()
        {
            if (InventoryStore.IsReadOnly) { MessageBox.Show("Read-Only Viewer mode cannot delete inventory."); return; }
            var item = SelectedItem(); if (item == null) return;
            if (MessageBox.Show("Delete " + item.ForgeId + " - " + item.ItemName + "?\n\nThis removes the inventory record but keeps the delete transaction.", "Delete Item", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            InventoryStore.DeleteItem(item); RefreshAll();
        }

        public void RefreshData()
        {
            RefreshAll();
        }
    }
}
