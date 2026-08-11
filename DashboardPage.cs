using System;
using System.Drawing;
using System.Windows.Forms;
using SOACSForgeWorks;

namespace SOACSForgeWorks.Pages
{
    public class DashboardPage : UserControl, ILiveRefreshable
    {
        private ComboBox timeFilter;
        private ComboBox projectFilter;
        private TableLayoutPanel cards;
        private Panel recentHost;
        private Panel summaryHost;
        private bool _loadingProjectFilter;
        private bool _refreshingDashboard;

        public DashboardPage()
        {
            BackColor = Theme.Background;
            Build();
            RefreshDashboard();
        }

        private void Build()
        {
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, BackColor = Theme.Background };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 132));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(root);

            root.Controls.Add(new Label { Dock = DockStyle.Fill, Text = "ForgeWorks Dashboard", ForeColor = Theme.Text, Font = new Font("Segoe UI Semibold", 21, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft }, 0, 0);

            var filters = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Background, Padding = new Padding(0, 6, 0, 8) };
            root.Controls.Add(filters, 0, 1);

            filters.Controls.Add(new Label { Text = "Timeframe", Left = 0, Top = 12, Width = 75, Height = 24, ForeColor = Theme.Muted, Font = Theme.SmallFont });
            timeFilter = new ComboBox { Left = 78, Top = 7, Width = 155, DropDownStyle = ComboBoxStyle.DropDownList };
            timeFilter.Items.AddRange(new object[] { "Today", "Last 48 Hours", "Last 7 Days", "This Month", "All Time" });
            timeFilter.SelectedIndex = 0;
            timeFilter.SelectedIndexChanged += (s, e) =>
            {
                if (_loadingProjectFilter || _refreshingDashboard) return;
                RefreshDashboard();
            };
            filters.Controls.Add(timeFilter);

            filters.Controls.Add(new Label { Text = "Project", Left = 255, Top = 12, Width = 55, Height = 24, ForeColor = Theme.Muted, Font = Theme.SmallFont });
            projectFilter = new ComboBox { Left = 312, Top = 7, Width = 235, DropDownStyle = ComboBoxStyle.DropDownList };
            projectFilter.SelectedIndexChanged += (s, e) =>
            {
                if (_loadingProjectFilter || _refreshingDashboard) return;
                RefreshDashboard();
            };
            filters.Controls.Add(projectFilter);

            var refresh = new Button { Text = "Refresh", Left = 560, Top = 5, Width = 100, Height = 30, BackColor = Theme.Panel2, ForeColor = Theme.Text, FlatStyle = FlatStyle.Flat };
            refresh.Click += (s, e) => { LoadProjectFilter(); RefreshDashboard(); };
            filters.Controls.Add(refresh);

            cards = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 7, RowCount = 1, BackColor = Theme.Background, Padding = new Padding(0, 0, 0, 14) };
            for (int i = 0; i < 7; i++) cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 7f));
            root.Controls.Add(cards, 0, 2);

            var lower = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Theme.Background };
            lower.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            lower.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            root.Controls.Add(lower, 0, 3);

            recentHost = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Background };
            summaryHost = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Background };
            lower.Controls.Add(recentHost, 0, 0);
            lower.Controls.Add(summaryHost, 1, 0);

            LoadProjectFilter();
        }

        private void LoadProjectFilter()
        {
            if (projectFilter == null) return;

            _loadingProjectFilter = true;
            try
            {
                string current = projectFilter.SelectedItem == null ? "All Projects" : projectFilter.SelectedItem.ToString();
                projectFilter.BeginUpdate();
                try
                {
                    projectFilter.Items.Clear();
                    projectFilter.Items.Add("All Projects");
                    foreach (var p in InventoryStore.Database.Projects)
                        if (!string.IsNullOrWhiteSpace(p.Name)) projectFilter.Items.Add(p.Name);
                    int idx = projectFilter.Items.IndexOf(current);
                    projectFilter.SelectedIndex = idx >= 0 ? idx : 0;
                }
                finally
                {
                    projectFilter.EndUpdate();
                }
            }
            finally
            {
                _loadingProjectFilter = false;
            }
        }

        private bool InSelectedTimeframe(DateTime dt)
        {
            string f = timeFilter == null || timeFilter.SelectedItem == null ? "Today" : timeFilter.SelectedItem.ToString();
            DateTime now = DateTime.Now;
            if (f == "Today") return dt.Date == now.Date;
            if (f == "Last 48 Hours") return dt >= now.AddHours(-48);
            if (f == "Last 7 Days") return dt >= now.AddDays(-7);
            if (f == "This Month") return dt.Year == now.Year && dt.Month == now.Month;
            return true;
        }

        private bool InSelectedProject(string project)
        {
            if (projectFilter == null || projectFilter.SelectedItem == null) return true;
            string p = projectFilter.SelectedItem.ToString();
            if (p == "All Projects") return true;
            return string.Equals(project ?? "", p, StringComparison.OrdinalIgnoreCase);
        }

        private string SelectedTimeCaption()
        {
            return timeFilter == null || timeFilter.SelectedItem == null ? "Today" : timeFilter.SelectedItem.ToString();
        }

        private void RefreshDashboard()
        {
            if (_refreshingDashboard) return;
            _refreshingDashboard = true;
            try
            {
            var db = InventoryStore.Database;
            int low = 0, outItems = 0, received = 0, issued = 0, moved = 0, totalItems = 0;
            foreach (var i in db.Items)
            {
                if (!InSelectedProject(i.Project)) continue;
                totalItems++;
                if (i.Quantity <= i.Minimum) low++;
                if (i.Quantity <= 0) outItems++;
            }
            foreach (var t in db.Transactions)
            {
                if (!InSelectedTimeframe(t.Time)) continue;
                if (!InSelectedProject(t.Project)) continue;
                if (t.Action == "RECEIVE" || t.Action == "RETURN") received += Math.Abs(t.Delta);
                if (t.Action == "ISSUE") issued += Math.Abs(t.Delta);
                if (t.Action == "MOVE") moved++;
            }

            cards.Controls.Clear();
            cards.Controls.Add(Card("TOTAL ITEMS", totalItems.ToString(), ProjectCaption(), Theme.Green), 0, 0);
            cards.Controls.Add(Card("LOW STOCK", low.ToString(), "Current selected project", low > 0 ? Theme.Gold : Theme.Green), 1, 0);
            cards.Controls.Add(Card("OUT NOW", outItems.ToString(), "Current zero quantity", outItems > 0 ? Color.IndianRed : Theme.Green), 2, 0);
            cards.Controls.Add(Card("RECEIVED", received.ToString(), SelectedTimeCaption(), Theme.Green), 3, 0);
            cards.Controls.Add(Card("ISSUED", issued.ToString(), SelectedTimeCaption(), Theme.Gold), 4, 0);
            cards.Controls.Add(Card("MOVED", moved.ToString(), SelectedTimeCaption(), Theme.Green), 5, 0);
            cards.Controls.Add(Card("DATABASE", InventoryStore.Workstation != null && InventoryStore.Workstation.UseSharedDatabase ? "SHARED" : "LOCAL", "Mode", Theme.Green), 6, 0);

            recentHost.Controls.Clear();
            summaryHost.Controls.Clear();
            recentHost.Controls.Add(ListPanel("RECENT TRANSACTIONS - " + SelectedTimeCaption(), Recent()));
            summaryHost.Controls.Add(ListPanel("DASHBOARD FILTERS", new[] {
                "Timeframe: " + SelectedTimeCaption(),
                "Project: " + ProjectCaption(),
                "Totals refresh when filters change",
                "Out Now and Low Stock are current inventory state",
                "Received, Issued, and Moved are based on selected timeframe"
            }));
            }
            finally
            {
                _refreshingDashboard = false;
            }
        }

        private string ProjectCaption()
        {
            if (projectFilter == null || projectFilter.SelectedItem == null) return "All Projects";
            return projectFilter.SelectedItem.ToString();
        }

        private string[] Recent()
        {
            var list = new System.Collections.Generic.List<string>();
            foreach (var t in InventoryStore.Database.Transactions)
            {
                if (!InSelectedTimeframe(t.Time)) continue;
                if (!InSelectedProject(t.Project)) continue;
                list.Add(t.Time.ToString("MM/dd HH:mm") + "  " + t.Action + "  " + t.ItemName + "  Qty " + t.NewQuantity + "  " + (t.Project ?? ""));
                if (list.Count >= 10) break;
            }
            if (list.Count == 0) list.Add("No transactions for selected filters");
            return list.ToArray();
        }

        private Control Card(string heading, string value, string caption, Color valueColor)
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Panel2, Margin = new Padding(0, 0, 10, 0), Padding = new Padding(14) };
            p.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, p.ClientRectangle, Theme.Border, ButtonBorderStyle.Solid);
            p.Controls.Add(new Label { Text = heading, Dock = DockStyle.Top, Height = 25, ForeColor = Theme.Text, Font = Theme.SmallFont });
            p.Controls.Add(new Label { Text = value, Dock = DockStyle.Top, Height = 45, ForeColor = valueColor, Font = new Font("Segoe UI Semibold", 20, FontStyle.Bold) });
            p.Controls.Add(new Label { Text = caption, Dock = DockStyle.Fill, ForeColor = Theme.Muted, Font = Theme.SmallFont });
            return p;
        }

        private Control ListPanel(string heading, string[] items)
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Panel, Margin = new Padding(0, 0, 12, 0), Padding = new Padding(18) };
            p.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, p.ClientRectangle, Theme.Border, ButtonBorderStyle.Solid);
            p.Controls.Add(new Label { Text = heading, Dock = DockStyle.Top, Height = 34, ForeColor = Theme.Text, Font = Theme.HeaderFont });
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, BackColor = Theme.Panel };
            foreach (var item in items) flow.Controls.Add(new Label { Text = "●  " + item, Width = 620, Height = 30, ForeColor = Theme.Muted, Font = Theme.NormalFont });
            p.Controls.Add(flow); flow.BringToFront(); return p;
        }

        public void RefreshData()
        {
            LoadProjectFilter();
            RefreshDashboard();
        }

    }
}
