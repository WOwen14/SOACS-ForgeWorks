using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SOACSForgeWorks.Pages
{
    public class ScanPage : UserControl, ILiveRefreshable
    {
        private TextBox inputBox;
        private TextBox qtyBox;
        private TextBox notesBox;
        private ComboBox projectBox;
        private ComboBox locationBox;
        private Label statusLabel;
        private Label instructionLabel;
        private Label currentOperationLabel;
        private Label lastItemLabel;
        private Label workflowLabel;
        private Label quickTipsLabel;
        private string currentOperation = "Lookup Item";
        private CheckBox keepQtyBox;
        private CheckBox keepProjectBox;
        private CheckBox keepLocationBox;
        private Timer focusTimer;
        private Panel scrollHost;
        private TableLayoutPanel operationButtons;
        private TextBox partSearchBox;
        private DataGridView partSearchGrid;
        private DataGridView recentGrid;
        private readonly List<InventoryItem> currentSearchItems = new List<InventoryItem>();

        public ScanPage()
        {
            Build();
        }

        private void Build()
        {
            SuspendLayout();
            BackColor = Theme.Background;
            Padding = Padding.Empty;
            Controls.Clear();

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Theme.Background,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(root);

            root.Controls.Add(new Label
            {
                Text = "Operations Workspace",
                Dock = DockStyle.Fill,
                ForeColor = Theme.Text,
                Font = new Font("Segoe UI Semibold", 22, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0)
            }, 0, 0);

            // Scrollable workspace: header, navigation, and status bar stay fixed while
            // the Operations cards can grow naturally. This prevents DPI/scale clipping
            // at 125% and smaller window sizes.
            scrollHost = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Theme.Background,
                Padding = new Padding(6, 0, 6, 6),
                Margin = Padding.Empty
            };
            root.Controls.Add(scrollHost, 0, 1);

            var workspace = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Theme.Background,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64));
            workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36));
            scrollHost.Controls.Add(workspace);
            scrollHost.Resize += (s, e) =>
            {
                // Keep cards inside the viewport and leave room for the vertical scrollbar.
                workspace.Width = Math.Max(1000, scrollHost.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 8);
            };
            workspace.Width = Math.Max(1000, scrollHost.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 8);

            var left = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Theme.Background,
                Padding = new Padding(0, 0, 8, 0),
                Margin = Padding.Empty
            };
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 235));
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 255));
            // Taller scan card prevents the scanner/search helper text from clipping at 125% DPI.
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 250));
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 240));
            workspace.Controls.Add(left, 0, 0);

            var right = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Theme.Background,
                Padding = new Padding(8, 0, 0, 0),
                Margin = Padding.Empty
            };
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 235));
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 350));
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 275));
            workspace.Controls.Add(right, 1, 0);

            operationButtons = BuildOperationButtonGrid();
            left.Controls.Add(Card("1. CHOOSE OPERATION", operationButtons), 0, 0);
            AddOperationButton("Lookup Item", 0, 0, "F2", "Lookup Item");
            AddOperationButton("Receive Inventory", 1, 0, "F3", "Receive Inventory");
            AddOperationButton("Issue Inventory", 2, 0, "F4", "Issue Inventory");
            AddOperationButton("Borrow Item", 3, 0, "F5", "Borrow Item");
            AddOperationButton("Move Inventory", 0, 1, "F6", "Move Inventory");
            AddOperationButton("Cycle Count", 1, 1, "F7", "Cycle Count");
            AddOperationButton("Adjust Qty", 2, 1, "F8", "Adjust Qty");
            AddOperationButton("Create New Item", 3, 1, "F9", "Create New Item");
            AddOperationButton("Return to Stock", 0, 2, "F10", "Return to Stock");
            AddOperationButton("Scrap / Dispose", 1, 2, "F11", "Scrap / Dispose");

            left.Controls.Add(Card("2. TRANSACTION DETAILS", BuildTransactionPanel()), 0, 1);
            left.Controls.Add(Card("3. SCAN OR TYPE ITEM", BuildScanInputPanel()), 0, 2);
            left.Controls.Add(Card("4. RECENT OPERATIONS", BuildRecentOperationsGrid()), 0, 3);

            right.Controls.Add(Card("CURRENT OPERATION / LAST ITEM", BuildCurrentOperationPanel()), 0, 0);
            right.Controls.Add(Card("FIND PART WHEN BARCODE IS UNKNOWN", BuildPartSearchPanel()), 0, 1);
            workflowLabel = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = Theme.Muted,
                BackColor = Theme.Panel,
                Font = Theme.NormalFont,
                Padding = new Padding(14),
                TextAlign = ContentAlignment.TopLeft
            };
            right.Controls.Add(Card("5. WORKFLOW GUIDANCE", workflowLabel), 0, 2);

            LoadDropDowns();
            SelectOperation("Lookup Item");
            HookContextUpdateEvents();

            Load += (s, e) => ResetWorkspaceToTop();
            VisibleChanged += (s, e) => { if (Visible) ResetWorkspaceToTop(); };
            focusTimer = new Timer { Interval = 3000 };
            focusTimer.Tick += (s, e) =>
            {
                if (!Visible || inputBox == null) return;
                if (partSearchBox != null && partSearchBox.Focused) return;
                if (partSearchGrid != null && partSearchGrid.Focused) return;
                if (projectBox != null && projectBox.Focused) return;
                if (locationBox != null && locationBox.Focused) return;
                if (qtyBox != null && qtyBox.Focused) return;
                if (notesBox != null && notesBox.Focused) return;
                if (!ContainsFocus) inputBox.Focus();
            };
            focusTimer.Start();

            Theme.ApplyToTree(this);
            ResumeLayout(true);
        }


        private void ResetWorkspaceToTop()
        {
            // Focusing the scanner input inside an AutoScroll panel can cause WinForms
            // to scroll the focused control into view. Focus it first so HID scanner
            // input still works, then explicitly restore the workspace to Step 1.
            if (scrollHost == null || inputBox == null) return;
            try
            {
                BeginInvoke(new Action(() =>
                {
                    try
                    {
                        inputBox.Focus();
                        scrollHost.AutoScrollPosition = Point.Empty;
                    }
                    catch { }
                }));
            }
            catch { }
        }

        private TableLayoutPanel BuildOperationButtonGrid()
        {
            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 3,
                BackColor = Theme.Panel,
                Padding = new Padding(8),
                Margin = Padding.Empty
            };
            for (int c = 0; c < 4; c++) grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            for (int r = 0; r < 3; r++) grid.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
            return grid;
        }

        private Control BuildTransactionPanel()
        {
            var outer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Theme.Panel,
                Padding = new Padding(12, 10, 12, 10),
                Margin = Padding.Empty
            };
            outer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
            outer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));

            // DPI-safe transaction layout.  The previous version used short fixed
            // rows that collapsed at 125% display scaling and clipped text boxes.
            // This layout keeps controls in two clean columns with the Keep boxes
            // beside the matching fields.
            var fields = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 3,
                BackColor = Theme.Panel,
                Padding = new Padding(0, 0, 14, 0),
                Margin = Padding.Empty
            };
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 72));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 72));
            fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 84));
            fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

            qtyBox = new TextBox { Dock = DockStyle.Fill, Text = "1", Font = Theme.NormalFont, Enabled = true, ReadOnly = false, Margin = new Padding(0, 2, 0, 0) };
            projectBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDown, Font = Theme.NormalFont, Enabled = true, Margin = new Padding(0, 2, 0, 0) };
            locationBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDown, Font = Theme.NormalFont, Enabled = true, Margin = new Padding(0, 2, 0, 0) };
            notesBox = new TextBox { Dock = DockStyle.Fill, Font = Theme.NormalFont, Enabled = true, ReadOnly = false, Multiline = true, ScrollBars = ScrollBars.Vertical, Margin = new Padding(0, 2, 0, 0) };
            StyleInput(qtyBox);
            StyleCombo(projectBox);
            StyleCombo(locationBox);
            StyleInput(notesBox);

            keepQtyBox = MakeKeepBox("Keep", true);
            keepProjectBox = MakeKeepBox("Keep", true);
            keepLocationBox = MakeKeepBox("Keep", true);
            var keepNotesBox = MakeKeepBox("Keep", false);

            fields.Controls.Add(WrapSmall("Qty / Count *", qtyBox), 0, 0);
            fields.Controls.Add(keepQtyBox, 1, 0);
            fields.Controls.Add(WrapSmall("Project / Borrowed By *", projectBox), 2, 0);
            fields.Controls.Add(keepProjectBox, 3, 0);

            fields.Controls.Add(WrapSmall("Location / Move To *", locationBox), 0, 1);
            fields.Controls.Add(keepLocationBox, 1, 1);
            fields.Controls.Add(WrapSmall("Notes / Reason", notesBox), 2, 1);
            fields.Controls.Add(keepNotesBox, 3, 1);

            var keepAll = new CheckBox
            {
                Text = "Keep selections for next operation",
                Checked = true,
                Dock = DockStyle.Fill,
                AutoSize = false,
                ForeColor = Theme.Text,
                BackColor = Theme.Panel,
                Font = Theme.SmallFont,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 6, 0, 0)
            };
            keepAll.CheckedChanged += (s, e) =>
            {
                keepQtyBox.Checked = keepAll.Checked;
                keepProjectBox.Checked = keepAll.Checked;
                keepLocationBox.Checked = keepAll.Checked;
                keepNotesBox.Checked = keepAll.Checked;
            };
            fields.Controls.Add(keepAll, 0, 2);
            fields.SetColumnSpan(keepAll, 4);

            outer.Controls.Add(fields, 0, 0);

            quickTipsLabel = new Label
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.Panel2,
                ForeColor = Theme.Muted,
                Font = Theme.NormalFont,
                Padding = new Padding(16, 12, 12, 12),
                TextAlign = ContentAlignment.TopLeft
            };
            var quick = CardInner("QUICK TIPS", quickTipsLabel);
            outer.Controls.Add(quick, 1, 0);
            return outer;
        }

        private CheckBox MakeKeepBox(string text, bool check)
        {
            var cb = new CheckBox
            {
                Text = text,
                AutoSize = false,
                Dock = DockStyle.Fill,
                ForeColor = Theme.Text,
                BackColor = Theme.Panel,
                Checked = check,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = Theme.SmallFont,
                Margin = new Padding(8, 20, 0, 0)
            };
            return cb;
        }

        private Control BuildScanInputPanel()
        {
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.Panel,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(12, 12, 12, 12),
                Margin = Padding.Empty
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 43));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 68));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 57));

            var scanBox = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = Theme.Panel2, Padding = new Padding(16, 14, 16, 14), Margin = new Padding(0, 0, 8, 0), MinimumSize = new Size(0, 155) };
            scanBox.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, scanBox.ClientRectangle, Theme.BorderSoft, ButtonBorderStyle.Dashed);
            scanBox.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            scanBox.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            scanBox.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            scanBox.Controls.Add(new Label { Text = "SCAN ITEM BARCODE", Dock = DockStyle.Fill, ForeColor = Theme.Text, Font = Theme.HeaderFont, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
            scanBox.Controls.Add(new Label { Text = "||||||||||||||||||||||||||||||||", Dock = DockStyle.Fill, ForeColor = Theme.Green, Font = new Font("Consolas", 20, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
            scanBox.Controls.Add(new Label { Text = "Ready for scanner. Most USB scanners act like a keyboard and send Enter.", Dock = DockStyle.Fill, ForeColor = Theme.Muted, Font = Theme.SmallFont, TextAlign = ContentAlignment.TopLeft }, 0, 2);
            panel.Controls.Add(scanBox, 0, 0);

            panel.Controls.Add(new Label { Text = "— OR —", Dock = DockStyle.Fill, ForeColor = Theme.Muted, Font = Theme.HeaderFont, TextAlign = ContentAlignment.MiddleCenter }, 1, 0);

            var typeBox = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, BackColor = Theme.Panel2, Padding = new Padding(16, 14, 16, 14), Margin = new Padding(8, 0, 0, 0), MinimumSize = new Size(0, 155) };
            typeBox.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, typeBox.ClientRectangle, Theme.BorderSoft, ButtonBorderStyle.Dashed);
            typeBox.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            typeBox.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            typeBox.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            typeBox.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            typeBox.Controls.Add(new Label { Text = "TYPE / SEARCH ITEM", Dock = DockStyle.Fill, ForeColor = Theme.Text, Font = Theme.HeaderFont, TextAlign = ContentAlignment.BottomLeft }, 0, 0);
            inputBox = new TextBox { Dock = DockStyle.Fill, Font = new Font("Consolas", 12, FontStyle.Bold), Margin = new Padding(0, 4, 0, 0), Enabled = true, ReadOnly = false };
            StyleInput(inputBox);
            inputBox.KeyDown += InputBox_KeyDown;
            typeBox.Controls.Add(inputBox, 0, 1);
            statusLabel = new Label { Text = "● SCANNER READY", Dock = DockStyle.Fill, ForeColor = Theme.Green, Font = Theme.SmallFont, TextAlign = ContentAlignment.MiddleLeft };
            typeBox.Controls.Add(statusLabel, 0, 2);
            instructionLabel = new Label { Text = "Enter Forge ID, barcode, serial, part number, NSN, or nomenclature and press Enter.", Dock = DockStyle.Fill, ForeColor = Theme.Muted, Font = Theme.SmallFont, TextAlign = ContentAlignment.TopLeft, Padding = new Padding(0, 6, 0, 0), AutoEllipsis = true };
            typeBox.Controls.Add(instructionLabel, 0, 3);
            panel.Controls.Add(typeBox, 2, 0);
            return panel;
        }

        private Control BuildCurrentOperationPanel()
        {
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Theme.Panel,
                Padding = new Padding(0),
                Margin = Padding.Empty
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));

            currentOperationLabel = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = Theme.Text,
                BackColor = Theme.Panel2,
                Font = Theme.NormalFont,
                Padding = new Padding(14),
                TextAlign = ContentAlignment.TopLeft
            };
            panel.Controls.Add(CardInner("CURRENT OPERATION", currentOperationLabel), 0, 0);

            lastItemLabel = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = Theme.Text,
                BackColor = Theme.Panel2,
                Font = Theme.NormalFont,
                Padding = new Padding(14),
                TextAlign = ContentAlignment.TopLeft
            };
            panel.Controls.Add(CardInner("LAST ITEM", lastItemLabel), 1, 0);
            return panel;
        }

        private void AddOperationButton(string label, int col, int row, string shortcut, string operationValue)
        {
            var b = new Button
            {
                Text = label + "\r\n" + shortcut,
                Dock = DockStyle.Fill,
                BackColor = Theme.Panel2,
                ForeColor = Theme.Text,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(6),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI Semibold", 10.0F, FontStyle.Bold),
                Tag = operationValue
            };
            b.FlatAppearance.BorderColor = Theme.Border;
            b.Enabled = !InventoryStore.IsReadOnly || operationValue == "Lookup Item";
            b.Click += (s, e) => SelectOperation(Convert.ToString(((Button)s).Tag));
            operationButtons.Controls.Add(b, col, row);
        }

        private void SelectOperation(string op)
        {
            currentOperation = InventoryStore.IsReadOnly ? "Lookup Item" : op;
            foreach (Control c in operationButtons.Controls)
            {
                var b = c as Button;
                if (b == null) continue;
                bool selected = Convert.ToString(b.Tag) == currentOperation;
                b.BackColor = selected ? Theme.Gold : Theme.Panel2;
                b.ForeColor = selected ? Color.Black : Theme.Text;
            }
            UpdateInstruction();
            UpdateCurrentOperationCard(null);
            if (inputBox != null) inputBox.Focus();
        }

        private Control Card(string title, Control content)
        {
            var p = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = Theme.Panel, Margin = new Padding(0, 0, 0, 10), Padding = new Padding(10), ColumnCount = 1, RowCount = 2 };
            p.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            p.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            p.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, p.ClientRectangle, Theme.Border, ButtonBorderStyle.Solid);
            p.Controls.Add(new Label { Text = title, Dock = DockStyle.Fill, ForeColor = Theme.Gold, Font = Theme.HeaderFont, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(0,0,0,2) }, 0, 0);
            content.Dock = DockStyle.Fill;
            p.Controls.Add(content, 0, 1);
            return p;
        }

        private Control CardInner(string title, Control content)
        {
            var p = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = Theme.Panel2, Margin = new Padding(0), Padding = new Padding(8), ColumnCount = 1, RowCount = 2 };
            p.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            p.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            p.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, p.ClientRectangle, Theme.BorderSoft, ButtonBorderStyle.Solid);
            p.Controls.Add(new Label { Text = title, Dock = DockStyle.Fill, ForeColor = Theme.Gold, Font = Theme.HeaderFont, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(0,0,0,2) }, 0, 0);
            content.Dock = DockStyle.Fill;
            p.Controls.Add(content, 0, 1);
            return p;
        }

        private Control WrapSmall(string label, Control child)
        {
            var p = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = Theme.Panel, Padding = new Padding(0, 0, 12, 4), ColumnCount = 1, RowCount = 2 };
            p.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
            p.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            p.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, ForeColor = Theme.Muted, Font = Theme.SmallFont, TextAlign = ContentAlignment.BottomLeft }, 0, 0);
            child.Dock = DockStyle.Fill;
            child.Enabled = true;
            child.TabStop = true;
            p.Controls.Add(child, 0, 1);
            return p;
        }

        private void StyleInput(TextBox t)
        {
            t.BackColor = Theme.Input;
            t.ForeColor = Theme.Text;
            t.BorderStyle = BorderStyle.FixedSingle;
            t.GotFocus += (s, e) => ((TextBox)s).BackColor = Theme.InputFocus;
            t.LostFocus += (s, e) => ((TextBox)s).BackColor = Theme.Input;
        }

        private void StyleCombo(ComboBox c)
        {
            c.BackColor = Theme.Input;
            c.ForeColor = Theme.Text;
            c.FlatStyle = FlatStyle.Flat;
            c.GotFocus += (s, e) => ((ComboBox)s).BackColor = Theme.InputFocus;
            c.LostFocus += (s, e) => ((ComboBox)s).BackColor = Theme.Input;
        }

        private void LoadDropDowns()
        {
            try
            {
                string proj = projectBox == null ? "" : projectBox.Text;
                string loc = locationBox == null ? "" : locationBox.Text;
                if (projectBox != null)
                {
                    projectBox.Items.Clear();
                    projectBox.Items.Add("General Inventory");
                    foreach (var p in InventoryStore.LoadProjects()) projectBox.Items.Add(p.Name);
                    if (!string.IsNullOrWhiteSpace(proj) && projectBox.Items.Contains(proj)) projectBox.SelectedItem = proj;
                    else if (projectBox.Items.Count > 0) projectBox.SelectedIndex = 0;
                }
                if (locationBox != null)
                {
                    locationBox.Items.Clear();
                    foreach (var l in InventoryStore.Database.Locations) locationBox.Items.Add(l.Name);
                    if (!string.IsNullOrWhiteSpace(loc) && locationBox.Items.Contains(loc)) locationBox.SelectedItem = loc;
                    else if (locationBox.Items.Count > 0) locationBox.SelectedIndex = 0;
                }
            }
            catch { }
        }

        private Control BuildPartSearchPanel()
        {
            var p = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1, BackColor = Theme.Panel, Padding = new Padding(0, 2, 0, 0) };
            p.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            p.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            p.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            partSearchBox = new TextBox { Dock = DockStyle.Fill, BackColor = Theme.Input, ForeColor = Theme.Text, BorderStyle = BorderStyle.FixedSingle, Font = Theme.NormalFont };
            partSearchBox.Text = "";
            StyleInput(partSearchBox);
            partSearchBox.TextChanged += (s, e) => LoadPartSearchResults();
            p.Controls.Add(partSearchBox, 0, 0);
            partSearchGrid = CreateThemedGrid();
            partSearchGrid.Columns.Add("ForgeId", "Forge ID");
            partSearchGrid.Columns.Add("Nomenclature", "Nomenclature");
            partSearchGrid.Columns.Add("Qty", "Qty On Hand");
            partSearchGrid.Columns.Add("Location", "Location");
            partSearchGrid.Columns[0].Width = 105;
            partSearchGrid.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            partSearchGrid.Columns[2].Width = 92;
            partSearchGrid.Columns[3].Width = 130;
            partSearchGrid.DoubleClick += (s, e) => UseSelectedSearchResult();
            p.Controls.Add(partSearchGrid, 0, 1);
            var useButton = new Button { Dock = DockStyle.Fill, Text = "Use Selected For Current Operation", FlatStyle = FlatStyle.Flat, BackColor = Theme.Panel2, ForeColor = Theme.Text, Font = Theme.SmallFont };
            useButton.FlatAppearance.BorderColor = Theme.Border;
            useButton.Click += (s, e) => UseSelectedSearchResult();
            p.Controls.Add(useButton, 0, 2);
            Load += (s, e) => LoadPartSearchResults();
            return p;
        }

        private DataGridView CreateThemedGrid()
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
            };
            Theme.ApplyGridTheme(grid);
            return grid;
        }

        private Control BuildRecentOperationsGrid()
        {
            recentGrid = CreateThemedGrid();
            recentGrid.Columns.Add("Time", "Time");
            recentGrid.Columns.Add("Operation", "Operation");
            recentGrid.Columns.Add("Item", "Item");
            recentGrid.Columns.Add("Qty", "Qty");
            recentGrid.Columns.Add("ProjectLocation", "Project / Location");
            recentGrid.Columns.Add("User", "User");
            recentGrid.Columns[0].Width = 90;
            recentGrid.Columns[1].Width = 120;
            recentGrid.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            recentGrid.Columns[3].Width = 65;
            recentGrid.Columns[4].Width = 210;
            recentGrid.Columns[5].Width = 130;
            return recentGrid;
        }

        private void LoadPartSearchResults()
        {
            if (partSearchGrid == null) return;
            partSearchGrid.Rows.Clear();
            currentSearchItems.Clear();
            string q = partSearchBox == null ? "" : partSearchBox.Text;
            foreach (var item in InventoryStore.SearchItems(q))
            {
                int row = partSearchGrid.Rows.Add(item.ForgeId, item.ItemName, item.Quantity, item.Location);
                partSearchGrid.Rows[row].Tag = item;
                currentSearchItems.Add(item);
            }
        }

        private void UseSelectedSearchResult()
        {
            if (partSearchGrid == null || partSearchGrid.CurrentRow == null) return;
            var item = partSearchGrid.CurrentRow.Tag as InventoryItem;
            if (item == null) return;
            ProcessInput(item.ForgeId);
            if (inputBox != null) { inputBox.Clear(); inputBox.Focus(); }
        }

        private void UpdateInstruction()
        {
            if (instructionLabel != null)
            {
                instructionLabel.Text = BuildInstructionText(currentOperation);
                instructionLabel.ForeColor = Theme.Muted;
            }
            if (workflowLabel != null) workflowLabel.Text = BuildWorkflowText(currentOperation);
            if (quickTipsLabel != null) quickTipsLabel.Text = BuildQuickTipsText(currentOperation);
        }

        private string BuildInstructionText(string op)
        {
            switch (op)
            {
                case "Receive Inventory": return "Ready to receive inventory. Scan a barcode or type/search a part and press Enter.";
                case "Issue Inventory": return "Ready to issue inventory. Select project, quantity, then scan or search the part.";
                case "Borrow Item": return "Ready to borrow inventory. Select borrowing project, quantity, then scan or search the part.";
                case "Move Inventory": return "Ready to move inventory. Select destination location, then scan or search the part.";
                case "Cycle Count": return "Ready for cycle count. Enter the physical count, then scan or search the part.";
                case "Adjust Qty": return "Ready to adjust quantity. Scan/search the part, then enter the new quantity and reason.";
                case "Create New Item": return "Ready to create a new part. Enter starting quantity/project/location, then scan the new barcode.";
                case "Return to Stock": return "Ready to return stock. Enter quantity and scan/search the part.";
                case "Scrap / Dispose": return "Ready to scrap/dispose inventory. Enter quantity/reason and scan/search the part.";
                default: return "Lookup mode. Scan or type an identifier to open the Part Workspace. No inventory transaction is created.";
            }
        }

        private string BuildQuickTipsText(string op)
        {
            switch (op)
            {
                case "Receive Inventory": return "• Scan a barcode to identify the part\r\n\r\n• Enter quantity and location\r\n\r\n• Review details on the right\r\n\r\n• Confirm transaction is logged";
                case "Issue Inventory": return "• Select the project using the part\r\n\r\n• Enter quantity to issue\r\n\r\n• Scan or search the part\r\n\r\n• Notes become transaction history";
                case "Borrow Item": return "• Project field means Borrowed By\r\n\r\n• Borrowing reduces source availability\r\n\r\n• Borrowed report shows replacements needed";
                case "Cycle Count": return "• Enter the actual physical count\r\n\r\n• ForgeWorks records any variance\r\n\r\n• Use for official inventory checks";
                case "Adjust Qty": return "• Use for quick corrections\r\n\r\n• A reason is required\r\n\r\n• Adjustment is logged with user/time";
                default: return "• Scan or type an identifier\r\n\r\n• Search if barcode is unknown\r\n\r\n• Double-click search results to use them";
            }
        }

        private string BuildWorkflowText(string op)
        {
            switch (op)
            {
                case "Receive Inventory": return "RECEIVE INVENTORY\r\n\r\n1  Enter quantity\r\n2  Select or confirm location\r\n3  Scan item barcode\r\n4  Review details\r\n5  Confirm complete";
                case "Issue Inventory": return "ISSUE INVENTORY\r\n\r\n1  Select project\r\n2  Enter quantity\r\n3  Scan or search part\r\n4  Review on-hand quantity\r\n5  Confirm complete";
                case "Borrow Item": return "BORROW INVENTORY\r\n\r\n1  Select borrowing project\r\n2  Enter quantity\r\n3  Scan/search source part\r\n4  Review borrowed impact\r\n5  Transaction is logged";
                case "Move Inventory": return "MOVE INVENTORY\r\n\r\n1  Select new location\r\n2  Scan/search part\r\n3  Confirm location change\r\n4  Move transaction is logged";
                case "Cycle Count": return "CYCLE COUNT\r\n\r\n1  Count physical inventory\r\n2  Enter actual count\r\n3  Scan/search part\r\n4  Variance is recorded\r\n5  Quantity is reconciled";
                case "Adjust Qty": return "ADJUST QUANTITY\r\n\r\n1  Scan/search part\r\n2  Enter new quantity\r\n3  Enter reason\r\n4  Adjustment is logged";
                case "Create New Item": return "CREATE NEW PART\r\n\r\n1  Enter starting quantity\r\n2  Select project/location\r\n3  Scan new barcode\r\n4  Part Workspace opens prefilled\r\n5  Save new part";
                case "Return to Stock": return "RETURN TO STOCK\r\n\r\n1  Enter return quantity\r\n2  Select location\r\n3  Scan/search part\r\n4  Inventory increases";
                case "Scrap / Dispose": return "SCRAP / DISPOSE\r\n\r\n1  Enter quantity\r\n2  Add reason/notes\r\n3  Scan/search part\r\n4  Inventory decreases";
                default: return "LOOKUP ITEM\r\n\r\n1  Scan or type an identifier\r\n2  If found, the Part Workspace opens\r\n3  No inventory transaction is created\r\n4  Use search when barcode is unknown";
            }
        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            string code = inputBox.Text.Trim();
            if (code.Length == 0) return;
            ProcessInput(code);
            inputBox.Clear();
            if (!keepQtyBox.Checked) qtyBox.Text = "1";
            if (!keepProjectBox.Checked && projectBox.Items.Count > 0) projectBox.SelectedIndex = 0;
            if (!keepLocationBox.Checked && locationBox.Items.Count > 0) locationBox.SelectedIndex = 0;
            inputBox.Focus();
        }

        private OperationContext CurrentContext(string code)
        {
            int qty;
            if (!int.TryParse(qtyBox.Text, out qty)) qty = 1;
            if (qty < 0) qty = Math.Abs(qty);
            return new OperationContext
            {
                OperationType = currentOperation,
                ScannedValue = code,
                Quantity = qty,
                Project = projectBox.Text.Trim(),
                Location = locationBox.Text.Trim(),
                Notes = notesBox.Text.Trim()
            };
        }

        private void ProcessInput(string code)
        {
            string op = InventoryStore.IsReadOnly ? "Lookup Item" : currentOperation;
            AddActivity("INPUT", code, "", "", InventoryStore.CurrentOperator);
            var item = InventoryStore.FindByScan(code);

            if (op == "Create New Item")
            {
                if (InventoryStore.IsReadOnly) { ShowMessage("Read-Only Viewer mode cannot create new records.", true); return; }
                OpenNewItem(CurrentContext(code));
                return;
            }

            if (item == null)
            {
                if (InventoryStore.IsReadOnly) { ShowMessage("No matching item found. Read-Only Viewer mode cannot create records.", true); return; }
                var answer = MessageBox.Show("No item was found for:\n\n" + code + "\n\nCreate a new ForgeWorks part?", "Create New Part", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (answer == DialogResult.Yes) OpenNewItem(CurrentContext(code));
                else ShowMessage("No matching part found for " + code + ".", true);
                return;
            }

            RunOperation(item, op, CurrentContext(code));
        }

        private void OpenNewItem(OperationContext ctx)
        {
            using (var f = new ItemEditorForm(null, ctx))
            {
                if (f.ShowDialog() == DialogResult.OK)
                {
                    ShowMessage("Part created and saved for " + ctx.ScannedValue + ".", false);
                    AddActivity("NEW ITEM", ctx.ScannedValue, ctx.Quantity.ToString(), ctx.Project + " / " + ctx.Location, InventoryStore.CurrentOperator);
                    LiveDataBus.NotifyDataChanged();
                }
                else ShowMessage("New part was cancelled for " + ctx.ScannedValue + ".", true);
            }
        }

        private void RunOperation(InventoryItem item, string op, OperationContext ctx)
        {
            int qty = ctx.Quantity;
            string noteSuffix = string.IsNullOrWhiteSpace(ctx.Notes) ? "" : " | " + ctx.Notes;
            if (!string.IsNullOrWhiteSpace(ctx.Project)) noteSuffix = "Project: " + ctx.Project + noteSuffix;
            string activityAction = op;
            string activityQty = qty.ToString();

            switch (op)
            {
                case "Receive Inventory": InventoryStore.AdjustQuantity(item, qty, "RECEIVE", "Operations receive. " + noteSuffix); activityAction = "RECEIVE"; activityQty = "+" + qty; break;
                case "Issue Inventory": InventoryStore.AdjustQuantity(item, -qty, "ISSUE", "Operations issue. " + noteSuffix); activityAction = "ISSUE"; activityQty = "-" + qty; break;
                case "Borrow Item": InventoryStore.BorrowItem(item, qty, ctx.Project, "Operations borrow. " + noteSuffix); activityAction = "BORROW"; activityQty = "-" + qty; break;
                case "Move Inventory": InventoryStore.MoveItem(item, ctx.Location, "Operations move. " + noteSuffix); activityAction = "MOVE"; activityQty = "—"; break;
                case "Cycle Count": InventoryStore.SetAbsoluteQuantity(item, qty, "Operations cycle count. " + noteSuffix); activityAction = "COUNT"; activityQty = "=" + qty; break;
                case "Adjust Qty": RunAdjustQuantity(item, ctx); ShowItem(item, ctx, op); LiveDataBus.NotifyDataChanged(); return;
                case "Return to Stock": InventoryStore.AdjustQuantity(item, qty, "RETURN", "Operations return. " + noteSuffix); activityAction = "RETURN"; activityQty = "+" + qty; break;
                case "Scrap / Dispose": InventoryStore.AdjustQuantity(item, -qty, "SCRAP", "Operations scrap/dispose. " + noteSuffix); activityAction = "SCRAP"; activityQty = "-" + qty; break;
                default: using (var f = new ItemEditorForm(item, new OperationContext())) f.ShowDialog(); activityAction = "LOOKUP"; activityQty = "—"; break;
            }
            AddActivity(activityAction, item.ForgeId + " - " + item.ItemName, activityQty, ctx.Project + " / " + ctx.Location, InventoryStore.CurrentOperator);
            ShowItem(item, ctx, op);
            LiveDataBus.NotifyDataChanged();
        }

        private void RunAdjustQuantity(InventoryItem item, OperationContext ctx)
        {
            int oldQty = item.Quantity;
            using (var f = new AdjustQuantityForm(item))
            {
                if (f.ShowDialog() != DialogResult.OK)
                {
                    AddActivity("ADJUST CANCEL", item.ForgeId + " - " + item.ItemName, "—", ctx.Project + " / " + ctx.Location, InventoryStore.CurrentOperator);
                    return;
                }
                string note = "Operations adjust quantity. Reason: " + f.Reason;
                if (!string.IsNullOrWhiteSpace(f.Notes)) note += " | " + f.Notes;
                if (!string.IsNullOrWhiteSpace(ctx.Project)) note += " | Project: " + ctx.Project;
                InventoryStore.SetAbsoluteQuantity(item, f.NewQuantity, note);
                AddActivity("ADJUST", item.ForgeId + " - " + item.ItemName + " | " + f.Reason, oldQty + "→" + f.NewQuantity, ctx.Project + " / " + ctx.Location, InventoryStore.CurrentOperator);
                item.Quantity = f.NewQuantity;
            }
        }

        private void ShowItem(InventoryItem item, OperationContext ctx, string operation)
        {
            string available = Convert.ToString(item.Quantity - item.ReservedQuantity);
            if (currentOperationLabel != null)
            {
                currentOperationLabel.Text = operation +
                    "\r\n\r\nOperator\r\n" + InventoryStore.CurrentOperator +
                    "\r\n\r\nProject\r\n" + ctx.Project +
                    "\r\n\r\nLocation\r\n" + ctx.Location +
                    "\r\n\r\nQty\r\n" + ctx.Quantity;
            }
            if (lastItemLabel != null)
            {
                lastItemLabel.Text = item.ForgeId +
                    "\r\n" + item.ItemName +
                    "\r\n\r\nLocation: " + item.Location +
                    "\r\nQty On Hand: " + item.Quantity +
                    "\r\nAvailable: " + available +
                    "\r\nStatus: " + InventoryStore.GetStatus(item) +
                    "\r\n\r\n" + DateTime.Now.ToString("HH:mm:ss");
            }
            ShowMessage(item.ForgeId + " - " + item.ItemName + " | Qty " + item.Quantity + " | " + InventoryStore.GetStatus(item), false);
        }

        private void UpdateCurrentOperationCard(InventoryItem item)
        {
            if (currentOperationLabel != null)
            {
                currentOperationLabel.Text = currentOperation +
                    "\r\n\r\nOperator\r\n" + InventoryStore.CurrentOperator +
                    "\r\n\r\nProject\r\n" + (projectBox == null ? "General Inventory" : projectBox.Text) +
                    "\r\n\r\nLocation\r\n" + (locationBox == null ? "—" : locationBox.Text) +
                    "\r\n\r\nQty\r\n" + (qtyBox == null ? "1" : qtyBox.Text);
            }
            if (lastItemLabel != null)
            {
                if (item == null) lastItemLabel.Text = "Last item: none\r\n\r\nQty: —\r\nProject: —\r\nLocation: —\r\nTime: —";
                else lastItemLabel.Text = item.ForgeId + "\r\n" + item.ItemName + "\r\n\r\nLocation: " + item.Location + "\r\nQty On Hand: " + item.Quantity;
            }
        }

        private void ShowMessage(string text, bool warning)
        {
            if (statusLabel != null)
            {
                statusLabel.ForeColor = warning ? Theme.Gold : Theme.Green;
                statusLabel.Text = warning ? "● ATTENTION" : "● SCANNER READY";
            }
            if (instructionLabel != null)
            {
                instructionLabel.ForeColor = warning ? Theme.Gold : Theme.Muted;
                instructionLabel.Text = text;
            }
        }

        private void AddActivity(string action, string detail, string qty, string projectLocation, string user)
        {
            if (recentGrid == null) return;
            recentGrid.Rows.Insert(0, DateTime.Now.ToString("HH:mm:ss"), action, detail, qty, projectLocation, string.IsNullOrWhiteSpace(user) ? InventoryStore.CurrentOperator : user);
            if (recentGrid.Rows.Count > 100) recentGrid.Rows.RemoveAt(recentGrid.Rows.Count - 1);
        }

        private void HookContextUpdateEvents()
        {
            if (qtyBox != null) qtyBox.TextChanged += (s, e) => UpdateCurrentOperationCard(null);
            if (projectBox != null) projectBox.TextChanged += (s, e) => UpdateCurrentOperationCard(null);
            if (locationBox != null) locationBox.TextChanged += (s, e) => UpdateCurrentOperationCard(null);
        }

        public void RefreshData()
        {
            LoadDropDowns();
            LoadPartSearchResults();
        }
    }
}
