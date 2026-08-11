using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using SOACSForgeWorks.Pages;
using SOACSForgeWorks.Core;

namespace SOACSForgeWorks
{
    public class MainForm : Form
    {
        private TableLayoutPanel rootLayout;
        private Panel headerPanel;
        private Panel navPanel;
        private Panel contentPanel;
        private Panel statusPanel;
        private Label statusClock;
        private Label statusInventory;
        private Label statusProjects;
        private Label statusSync;
        private Timer liveRefreshTimer;
        private DateTime lastSeenDatabaseWriteUtc = DateTime.MinValue;
        private readonly Dictionary<string, UserControl> pages = new Dictionary<string, UserControl>();
        private readonly Dictionary<string, Button> navButtons = new Dictionary<string, Button>();
        private Timer clockTimer;
        private string activePage = "";
        private bool applicationReady;
        private bool _refreshingAllPages;

        public MainForm()
        {
            Text = "SOACS ForgeWorks v3.1.1 Repository Profile Hotfix";
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            AutoScaleMode = AutoScaleMode.Dpi;
            MinimumSize = new Size(1240, 780);
            BackColor = Theme.Background;
            Font = Theme.NormalFont;
            Opacity = 0;
            ShowInTaskbar = false;
            LoadApplicationIcon();

            SetSafeStartupBounds();
            LiveDataBus.SetApplicationReady(false);
            BuildShell();
            BuildPages();
            ShowPage("Dashboard");
            EnableDoubleBufferingForTree(this);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            try
            {
                BeginInvoke(new Action(CompleteStartupAfterFirstPaint));
            }
            catch (Exception ex)
            {
                InventoryStore.WriteStartupLog("OnShown startup scheduling failed: " + ex.Message);
                CompleteStartupAfterFirstPaint();
            }
        }

        private void CompleteStartupAfterFirstPaint()
        {
            try
            {
                applicationReady = false;
                LiveDataBus.SetApplicationReady(false);
                StartupManager.InitializeAfterShown(this);
                Theme.ApplyToTree(this);
                RefreshStatusMetrics();
                EnableDoubleBufferingForTree(this);

                applicationReady = true;
                LiveDataBus.SetApplicationReady(true);
                InitializeLiveDataEngine();
                SafeRefreshAllPages("startup complete");
                FadeInMainWindow();
                InventoryStore.WriteStartupLog("ForgeWorks startup complete. Main window shown after lifecycle initialization.");
            }
            catch (Exception ex)
            {
                applicationReady = true;
                LiveDataBus.SetApplicationReady(true);
                InventoryStore.WriteStartupLog("CompleteStartupAfterFirstPaint failed: " + ex.Message);
            }
        }

        private void SetSafeStartupBounds()
        {
            Rectangle work = Screen.PrimaryScreen.WorkingArea;
            int width = Math.Min(1540, Math.Max(1240, work.Width - 70));
            int height = Math.Min(900, Math.Max(780, work.Height - 70));
            Size = new Size(width, height);
            Location = new Point(work.Left + (work.Width - width) / 2, work.Top + (work.Height - height) / 2);
        }

        private void BuildShell()
        {
            SuspendLayout();
            Controls.Clear();

            rootLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.Background,
                ColumnCount = 2,
                RowCount = 3,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 196F));
            rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 86F));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            Controls.Add(rootLayout);

            headerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.Header,
                Padding = new Padding(14, 6, 14, 6),
                Margin = Padding.Empty
            };
            rootLayout.Controls.Add(headerPanel, 0, 0);
            rootLayout.SetColumnSpan(headerPanel, 2);
            BuildHeader();

            navPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.Panel,
                Padding = new Padding(0, 8, 0, 0),
                Margin = Padding.Empty
            };
            rootLayout.Controls.Add(navPanel, 0, 1);
            BuildNavigation();

            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.Background,
                Padding = new Padding(16, 12, 16, 12),
                Margin = Padding.Empty
            };
            rootLayout.Controls.Add(contentPanel, 1, 1);

            statusPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.Header,
                Padding = new Padding(12, 0, 12, 0),
                Margin = Padding.Empty
            };
            rootLayout.Controls.Add(statusPanel, 0, 2);
            rootLayout.SetColumnSpan(statusPanel, 2);
            BuildStatusBar();

            ResumeLayout(true);
        }

        private void BuildHeader()
        {
            headerPanel.Controls.Clear();
            headerPanel.Padding = new Padding(10, 6, 14, 6);

            var headerLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = Theme.Header,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92F));
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 560F));
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 330F));
            headerPanel.Controls.Add(headerLayout);

            var logo = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Theme.Header,
                Margin = new Padding(0, 0, 10, 0)
            };
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "ForgeWorksLogo.png");
            if (File.Exists(path)) logo.Image = Image.FromFile(path);
            headerLayout.Controls.Add(logo, 0, 0);

            var titleBlock = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                BackColor = Theme.Header,
                Margin = Padding.Empty,
                Padding = new Padding(0, 6, 0, 0)
            };
            titleBlock.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            titleBlock.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            headerLayout.Controls.Add(titleBlock, 1, 0);

            titleBlock.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Text = "SOACS FORGEWORKS",
                ForeColor = Theme.Gold,
                Font = new Font("Segoe UI Semibold", 24F, FontStyle.Bold),
                TextAlign = ContentAlignment.BottomLeft
            }, 0, 0);
            titleBlock.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Text = "MISSION FABRICATION OPERATIONS",
                ForeColor = Theme.Muted,
                Font = Theme.SmallFont,
                TextAlign = ContentAlignment.TopLeft
            }, 0, 1);

            headerLayout.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Text = "SCAN  •  TRACK  •  BUILD  •  ISSUE",
                ForeColor = Theme.Gold,
                Font = Theme.HeaderFont,
                TextAlign = ContentAlignment.MiddleCenter
            }, 2, 0);

            headerLayout.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Text = "USER: " + InventoryStore.CurrentOperator.ToUpperInvariant() + "\r\nPC: " + InventoryStore.CurrentMachineName.ToUpperInvariant() + "\r\nBUILD: v3.1.1  |  " + InventoryStore.Workstation.WorkstationMode.ToUpperInvariant(),
                ForeColor = Theme.Muted,
                Font = Theme.SmallFont,
                TextAlign = ContentAlignment.MiddleRight
            }, 3, 0);
        }

        private void LoadApplicationIcon()
        {
            try
            {
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "ForgeWorks.ico");
                if (File.Exists(iconPath)) Icon = new Icon(iconPath);
            }
            catch { }
        }

        private void BuildStatusBar()
        {
            statusPanel.Controls.Clear();

            var leftFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                Width = 1320,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Theme.Header,
                Padding = new Padding(0),
                Margin = Padding.Empty
            };
            statusPanel.Controls.Add(leftFlow);

            AddStatusChip(leftFlow, "● READY", Theme.Green, 95);
            AddStatusChip(leftFlow, "PROFILE: " + (InventoryStore.Workstation == null ? "Standalone" : InventoryStore.Workstation.RepositoryProfile).ToUpperInvariant(), GetProfileColor(), 170);
            AddStatusChip(leftFlow, InventoryStore.Workstation.UseSharedDatabase ? "DATABASE: SHARED" : "DATABASE: LOCAL", Theme.Muted, 150);
            AddStatusChip(leftFlow, InventoryStore.IsReadOnly ? "READ-ONLY VIEWER" : "OPERATOR MODE", InventoryStore.IsReadOnly ? Theme.Gold : Theme.Muted, 160);
            AddStatusChip(leftFlow, "SCANNER READY", Theme.Muted, 150);
            statusInventory = AddStatusChip(leftFlow, "INVENTORY: --", Theme.Muted, 140);
            statusProjects = AddStatusChip(leftFlow, "PROJECTS: --", Theme.Muted, 130);
            AddStatusChip(leftFlow, "BACKUPS: MANUAL", Theme.Gold, 145);
            statusSync = AddStatusChip(leftFlow, "LIVE REFRESH: ON", Theme.Green, 160);

            statusClock = new Label
            {
                Dock = DockStyle.Right,
                Width = 200,
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = Theme.Muted,
                Font = Theme.SmallFont
            };
            statusPanel.Controls.Add(statusClock);

            clockTimer = new Timer { Interval = 1000 };
            clockTimer.Tick += (s, e) => statusClock.Text = DateTime.Now.ToString("M/d/yyyy  HH:mm:ss");
            clockTimer.Start();
            statusClock.Text = DateTime.Now.ToString("M/d/yyyy  HH:mm:ss");
        }

        private Color GetProfileColor()
        {
            try
            {
                var p = RepositoryManager.CurrentProfile;
                string c = p == null ? "" : p.ColorName;
                if (string.Equals(c, "Green", StringComparison.OrdinalIgnoreCase)) return Theme.Green;
                if (string.Equals(c, "Blue", StringComparison.OrdinalIgnoreCase)) return Color.DeepSkyBlue;
                if (string.Equals(c, "Orange", StringComparison.OrdinalIgnoreCase)) return Color.Orange;
                if (string.Equals(c, "Red", StringComparison.OrdinalIgnoreCase)) return Color.IndianRed;
                return Theme.Gold;
            }
            catch { return Theme.Gold; }
        }

        private Label AddStatusChip(FlowLayoutPanel host, string text, Color color, int width)
        {
            var label = new Label
            {
                Text = text,
                Width = width,
                Height = 32,
                Margin = new Padding(0, 0, 10, 0),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = color,
                Font = Theme.SmallFont
            };
            host.Controls.Add(label);
            return label;
        }

        private void BuildNavigation()
        {
            navPanel.Controls.Clear();

            var version = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                Text = "SOACS ForgeWorks v3.1.1\r\nRepository Profiles",
                ForeColor = Theme.Muted,
                Font = Theme.SmallFont,
                TextAlign = ContentAlignment.MiddleCenter
            };
            navPanel.Controls.Add(version);

            string[] names =
            {
                "Dashboard", "Operations", "Inventory", "Storage Explorer", "Projects", "Kits", "Transactions",
                "Reports", "Administration", "Settings", "About"
            };

            for (int i = names.Length - 1; i >= 0; i--)
                AddNavButton(names[i]);
        }

        private void AddNavButton(string name)
        {
            string icon = GetNavIcon(name);
            var b = new Button
            {
                Text = icon + "  " + name,
                Tag = name,
                Height = 48,
                Dock = DockStyle.Top,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(16, 0, 0, 0),
                ForeColor = Theme.Text,
                BackColor = Theme.Panel,
                Font = Theme.NormalFont,
                Margin = Padding.Empty
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Theme.Panel2;
            b.FlatAppearance.MouseDownBackColor = Theme.Border;
            b.Click += (s, e) => ShowPage((string)((Button)s).Tag);
            navButtons[name] = b;
            navPanel.Controls.Add(b);
        }

        private string GetNavIcon(string name)
        {
            switch (name)
            {
                case "Dashboard": return "⌂";
                case "Operations": return "▣";
                case "Inventory": return "▤";
                case "Storage Explorer": return "▦";
                case "Projects": return "▸";
                case "Kits": return "★";
                case "Transactions": return "≡";
                case "Reports": return "◫";
                case "Administration": return "⚙";
                case "Settings": return "◉";
                case "About": return "?";
                default: return "•";
            }
        }

        private void BuildPages()
        {
            // Lazy page loading: only the Dashboard is created during startup.
            // Heavy pages are created the first time the operator opens them.
            pages.Clear();
            pages["Dashboard"] = new DashboardPage();
        }

        private UserControl CreatePage(string name)
        {
            switch (name)
            {
                case "Dashboard": return new DashboardPage();
                case "Operations": return new ScanPage();
                case "Inventory": return new InventoryPage();
                case "Storage Explorer": return new StorageExplorerPage();
                case "Projects": return new ProjectsPage();
                case "Kits": return new KitsPage();
                case "Transactions": return new TransactionsPage();
                case "Reports": return new ReportsPage();
                case "Administration": return new AdminPage();
                case "Settings": return new SettingsPage();
                case "About": return new AboutPage();
                default: return new DashboardPage();
            }
        }

        private void ShowPage(string name)
        {
            if (contentPanel == null) return;
            activePage = name;
            foreach (var kv in navButtons)
            {
                bool selected = kv.Key == activePage;
                kv.Value.BackColor = selected ? Theme.Panel2 : Theme.Panel;
                kv.Value.ForeColor = selected ? Theme.Gold : Theme.Text;
                kv.Value.FlatAppearance.BorderSize = selected ? 1 : 0;
                kv.Value.FlatAppearance.BorderColor = Theme.Gold;
                kv.Value.Font = selected ? new Font("Segoe UI Semibold", 10F, FontStyle.Bold) : Theme.NormalFont;
            }
            contentPanel.SuspendLayout();
            contentPanel.Controls.Clear();
            if (!pages.ContainsKey(name))
            {
                pages[name] = CreatePage(name);
                EnableDoubleBufferingForTree(pages[name]);
            }
            UserControl page = pages.ContainsKey(name) ? pages[name] : pages["Dashboard"];
            page.Dock = DockStyle.Fill;
            contentPanel.Controls.Add(page);
            Theme.ApplyToTree(page);
            contentPanel.ResumeLayout(true);
            RefreshStatusMetrics();
        }


        private void FadeInMainWindow()
        {
            try
            {
                ShowInTaskbar = true;
                var fadeTimer = new Timer { Interval = 18 };
                fadeTimer.Tick += (s, e) =>
                {
                    if (Opacity >= 0.98)
                    {
                        Opacity = 1;
                        fadeTimer.Stop();
                        fadeTimer.Dispose();
                        return;
                    }
                    Opacity += 0.12;
                };
                fadeTimer.Start();
            }
            catch
            {
                Opacity = 1;
                ShowInTaskbar = true;
            }
        }

        private void EnableDoubleBufferingForTree(Control root)
        {
            if (root == null) return;
            try
            {
                var prop = typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (prop != null) prop.SetValue(root, true, null);
            }
            catch { }
            foreach (Control child in root.Controls) EnableDoubleBufferingForTree(child);
        }

        private void InitializeLiveDataEngine()
        {
            LiveDataBus.DataChanged += (s, e) => SafeRefreshAllPages("local save");
            lastSeenDatabaseWriteUtc = GetDatabaseWriteTimeUtc();
            int seconds = 15;
            try { seconds = Math.Max(5, InventoryStore.Workstation.AutoRefreshSeconds); } catch { }
            liveRefreshTimer = new Timer { Interval = seconds * 1000 };
            liveRefreshTimer.Tick += (s, e) => CheckForExternalDatabaseChanges();
            liveRefreshTimer.Start();
        }

        private DateTime GetDatabaseWriteTimeUtc()
        {
            try
            {
                if (File.Exists(InventoryStore.DataFile)) return File.GetLastWriteTimeUtc(InventoryStore.DataFile);
            }
            catch { }
            return DateTime.MinValue;
        }

        private void CheckForExternalDatabaseChanges()
        {
            if (!applicationReady) return;
            DateTime stamp = GetDatabaseWriteTimeUtc();
            if (stamp == DateTime.MinValue || stamp <= lastSeenDatabaseWriteUtc) return;
            lastSeenDatabaseWriteUtc = stamp;
            try { InventoryStore.Load(); }
            catch (Exception ex) { InventoryStore.WriteStartupLog("Live refresh database reload failed: " + ex.Message); return; }
            SafeRefreshAllPages("shared database sync");
        }

        private void SafeRefreshAllPages(string reason)
        {
            if (IsDisposed) return;
            if (!IsHandleCreated) return;
            if (!applicationReady && reason != "startup complete") return;
            try
            {
                if (InvokeRequired) { BeginInvoke(new Action(() => RefreshAllPages(reason))); return; }
                RefreshAllPages(reason);
            }
            catch (Exception ex)
            {
                InventoryStore.WriteStartupLog("Live refresh failed: " + ex.Message);
            }
        }

        private void RefreshAllPages(string reason)
        {
            if (_refreshingAllPages) return;
            _refreshingAllPages = true;
            try
            {
            foreach (var page in pages.Values)
            {
                var refreshable = page as ILiveRefreshable;
                if (refreshable != null)
                {
                    try { refreshable.RefreshData(); } catch (Exception ex) { InventoryStore.WriteStartupLog("RefreshData failed on " + page.GetType().Name + ": " + ex.Message); }
                }
            }
            RefreshStatusMetrics();
            if (statusSync != null) statusSync.Text = "LAST SYNC: " + DateTime.Now.ToString("HH:mm:ss");
            }
            finally
            {
                _refreshingAllPages = false;
            }
        }

        internal void RefreshShellStatus()
        {
            RefreshStatusMetrics();
        }

        internal void SafeRefreshStatusMetrics()
        {
            RefreshStatusMetrics();
        }

        private void RefreshStatusMetrics()
        {
            try
            {
                var items = InventoryStore.LoadItems();
                var projects = InventoryStore.LoadProjects();
                if (statusInventory != null) statusInventory.Text = "INVENTORY: " + items.Count;
                if (statusProjects != null) statusProjects.Text = "PROJECTS: " + projects.Count;
            }
            catch
            {
                if (statusInventory != null) statusInventory.Text = "INVENTORY: --";
                if (statusProjects != null) statusProjects.Text = "PROJECTS: --";
            }
        }
    }
}
