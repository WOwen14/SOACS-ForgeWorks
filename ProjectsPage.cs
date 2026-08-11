using System;
using System.Drawing;
using System.Windows.Forms;

namespace SOACSForgeWorks.Pages
{
    public class ProjectsPage : UserControl, ILiveRefreshable
    {
        private DataGridView projectGrid;
        private DataGridView itemGrid;
        private DataGridView transactionGrid;
        private TextBox txtName;
        private TextBox txtCode;
        private TextBox txtOwner;
        private TextBox txtNotes;
        private ComboBox cboStatus;
        private ComboBox cboPriority;
        private DateTimePicker dueDate;
        private Label projectSummary;
        private ProjectRecord selectedProject;

        public ProjectsPage()
        {
            Build();
            LoadProjects();
        }

        private void Build()
        {
            BackColor = Theme.Background;

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Theme.Background,
                Padding = new Padding(0)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(root);

            root.Controls.Add(new Label
            {
                Text = "Projects",
                Dock = DockStyle.Fill,
                ForeColor = Theme.Text,
                Font = new Font("Segoe UI Semibold", 20, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);

            var ribbon = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Theme.Background,
                Padding = new Padding(0, 4, 0, 4),
                Margin = new Padding(0)
            };
            root.Controls.Add(ribbon, 0, 1);
            AddRibbonButton(ribbon, "+ New Project", (s, e) => NewProject());
            AddRibbonButton(ribbon, "Save Project", (s, e) => SaveProject());
            AddRibbonButton(ribbon, "Delete Project", (s, e) => DeleteProject());
            AddRibbonButton(ribbon, "Assign Item", (s, e) => AssignItemToProject());
            AddRibbonButton(ribbon, "Remove Item", (s, e) => RemoveSelectedItemFromProject());
            AddRibbonButton(ribbon, "Open Item", (s, e) => OpenSelectedItem());
            AddRibbonButton(ribbon, "Refresh", (s, e) => LoadProjects());

            var body = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Theme.Background
            };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 420));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.Controls.Add(body, 0, 2);

            body.Controls.Add(BuildProjectListPanel(), 0, 0);
            body.Controls.Add(BuildProjectWorkspace(), 1, 0);
        }

        private Control BuildProjectListPanel()
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Panel, Padding = new Padding(12), Margin = new Padding(0, 0, 12, 0) };
            p.Controls.Add(new Label { Text = "Project List", Dock = DockStyle.Top, Height = 28, ForeColor = Theme.Text, Font = Theme.HeaderFont });

            projectGrid = StandardGrid();
            projectGrid.Dock = DockStyle.Fill;
            projectGrid.SelectionChanged += (s, e) => SelectProjectFromGrid();
            projectGrid.DoubleClick += (s, e) => SelectProjectFromGrid();
            p.Controls.Add(projectGrid);
            projectGrid.BringToFront();
            return p;
        }

        private Control BuildProjectWorkspace()
        {
            var workspace = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Theme.Background
            };
            workspace.RowStyles.Add(new RowStyle(SizeType.Absolute, 190));
            workspace.RowStyles.Add(new RowStyle(SizeType.Percent, 58));
            workspace.RowStyles.Add(new RowStyle(SizeType.Percent, 42));

            workspace.Controls.Add(BuildProjectDetailPanel(), 0, 0);
            workspace.Controls.Add(BuildAssignedItemsPanel(), 0, 1);
            workspace.Controls.Add(BuildTransactionPanel(), 0, 2);
            return workspace;
        }

        private Control BuildProjectDetailPanel()
        {
            var p = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 4, BackColor = Theme.Panel, Padding = new Padding(12), Margin = new Padding(0, 0, 0, 8) };
            p.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 23));
            p.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 23));
            p.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 23));
            p.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 31));
            p.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            p.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
            p.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
            p.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            p.Controls.Add(new Label { Text = "Project Details", Dock = DockStyle.Fill, ForeColor = Theme.Text, Font = Theme.HeaderFont, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
            projectSummary = new Label { Text = "No project selected", Dock = DockStyle.Fill, ForeColor = Theme.Gold, Font = Theme.SmallFont, TextAlign = ContentAlignment.MiddleRight };
            p.SetColumnSpan(projectSummary, 3);
            p.Controls.Add(projectSummary, 1, 0);

            txtName = new TextBox { Dock = DockStyle.Fill };
            txtCode = new TextBox { Dock = DockStyle.Fill };
            txtOwner = new TextBox { Dock = DockStyle.Fill };
            dueDate = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short };
            cboStatus = Combo("Active", "On Hold", "Complete", "Archived");
            cboPriority = Combo("Low", "Normal", "High", "Critical");
            txtNotes = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical };

            p.Controls.Add(LabelWrap("Project Name", txtName), 0, 1);
            p.Controls.Add(LabelWrap("Project Code", txtCode), 1, 1);
            p.Controls.Add(LabelWrap("Owner / Lead", txtOwner), 2, 1);
            p.Controls.Add(LabelWrap("Due Date", dueDate), 3, 1);
            p.Controls.Add(LabelWrap("Status", cboStatus), 0, 2);
            p.Controls.Add(LabelWrap("Priority", cboPriority), 1, 2);
            p.Controls.Add(LabelWrap("Notes", txtNotes), 2, 2);
            p.SetColumnSpan(txtNotes.Parent, 2);

            return p;
        }

        private Control BuildAssignedItemsPanel()
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Panel, Padding = new Padding(12), Margin = new Padding(0, 0, 0, 8) };
            p.Controls.Add(new Label { Text = "Assigned Parts / Pieces / Inventory", Dock = DockStyle.Top, Height = 28, ForeColor = Theme.Text, Font = Theme.HeaderFont });
            itemGrid = StandardGrid();
            itemGrid.Dock = DockStyle.Fill;
            itemGrid.DoubleClick += (s, e) => OpenSelectedItem();
            p.Controls.Add(itemGrid);
            itemGrid.BringToFront();
            return p;
        }

        private Control BuildTransactionPanel()
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Panel, Padding = new Padding(12), Margin = new Padding(0) };
            p.Controls.Add(new Label { Text = "Project Transaction History", Dock = DockStyle.Top, Height = 28, ForeColor = Theme.Text, Font = Theme.HeaderFont });
            transactionGrid = StandardGrid();
            transactionGrid.Dock = DockStyle.Fill;
            p.Controls.Add(transactionGrid);
            transactionGrid.BringToFront();
            return p;
        }

        private ComboBox Combo(params string[] values)
        {
            var c = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            c.Items.AddRange(values);
            if (c.Items.Count > 0) c.SelectedIndex = 0;
            return c;
        }

        private Control LabelWrap(string label, Control child)
        {
            var p = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, BackColor = Theme.Panel, Margin = new Padding(4, 0, 4, 0) };
            p.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
            p.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            p.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, ForeColor = Theme.Muted, Font = Theme.SmallFont, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
            p.Controls.Add(child, 0, 1);
            return p;
        }

        private void AddRibbonButton(FlowLayoutPanel ribbon, string text, EventHandler handler)
        {
            var b = new Button { Text = text, Width = 118, Height = 38, BackColor = Theme.Panel2, ForeColor = Theme.Text, FlatStyle = FlatStyle.Flat, Margin = new Padding(0, 0, 8, 0) };
            b.FlatAppearance.BorderColor = Theme.Border;
            bool writeAction = text != "Open Item" && text != "Refresh";
            if (InventoryStore.IsReadOnly && writeAction) { b.Enabled = false; b.Text = text + " (RO)"; }
            b.Click += handler;
            ribbon.Controls.Add(b);
        }

        private DataGridView StandardGrid()
        {
            var g = new DataGridView
            {
                BackgroundColor = Theme.Background,
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
            Theme.ApplyGridTheme(g);
            g.DefaultCellStyle.Font = Theme.SmallFont;
            return g;
        }

        private void LoadProjects()
        {
            if (projectGrid == null) return;
            projectGrid.Columns.Clear();
            projectGrid.Rows.Clear();
            projectGrid.Columns.Add("Name", "Project");
            projectGrid.Columns.Add("Status", "Status");
            projectGrid.Columns.Add("Priority", "Priority");
            projectGrid.Columns["Name"].FillWeight = 170;
            projectGrid.Columns["Status"].FillWeight = 85;
            projectGrid.Columns["Priority"].FillWeight = 85;

            foreach (var p in InventoryStore.Database.Projects)
            {
                projectGrid.Rows.Add(p.Name, p.Status, p.Priority);
            }

            if (selectedProject != null)
            {
                for (int r = 0; r < projectGrid.Rows.Count; r++)
                {
                    if (string.Equals(Convert.ToString(projectGrid.Rows[r].Cells[0].Value), selectedProject.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        projectGrid.Rows[r].Selected = true;
                        projectGrid.CurrentCell = projectGrid.Rows[r].Cells[0];
                        break;
                    }
                }
            }
            else if (projectGrid.Rows.Count > 0)
            {
                projectGrid.Rows[0].Selected = true;
                projectGrid.CurrentCell = projectGrid.Rows[0].Cells[0];
            }
            SelectProjectFromGrid();
        }

        private void SelectProjectFromGrid()
        {
            if (projectGrid == null || projectGrid.CurrentRow == null) return;
            string name = Convert.ToString(projectGrid.CurrentRow.Cells[0].Value);
            selectedProject = FindProject(name);
            LoadProjectDetails();
            LoadAssignedItems();
            LoadTransactions();
        }

        private ProjectRecord FindProject(string name)
        {
            foreach (var p in InventoryStore.Database.Projects)
                if (string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)) return p;
            return null;
        }

        private void LoadProjectDetails()
        {
            if (selectedProject == null)
            {
                ClearDetails();
                return;
            }
            txtName.Text = selectedProject.Name ?? "";
            txtCode.Text = selectedProject.ProjectCode ?? "";
            txtOwner.Text = selectedProject.Owner ?? "";
            txtNotes.Text = selectedProject.Notes ?? "";
            SetCombo(cboStatus, selectedProject.Status, "Active");
            SetCombo(cboPriority, selectedProject.Priority, "Normal");
            dueDate.Value = selectedProject.DueDate == DateTime.MinValue ? DateTime.Today : selectedProject.DueDate;
            int itemCount = InventoryStore.ItemsForProject(selectedProject.Name).Count;
            projectSummary.Text = itemCount + " assigned item(s)";
        }

        private void SetCombo(ComboBox combo, string value, string fallback)
        {
            combo.SelectedItem = string.IsNullOrWhiteSpace(value) ? fallback : value;
            if (combo.SelectedIndex < 0) combo.SelectedItem = fallback;
            if (combo.SelectedIndex < 0 && combo.Items.Count > 0) combo.SelectedIndex = 0;
        }

        private void ClearDetails()
        {
            txtName.Text = "";
            txtCode.Text = "";
            txtOwner.Text = "";
            txtNotes.Text = "";
            cboStatus.SelectedIndex = 0;
            cboPriority.SelectedIndex = 1;
            dueDate.Value = DateTime.Today;
            projectSummary.Text = "No project selected";
            LoadAssignedItems();
            LoadTransactions();
        }

        private void NewProject()
        {
            selectedProject = null;
            projectGrid.ClearSelection();
            ClearDetails();
            txtName.Focus();
        }

        private void SaveProject()
        {
            string oldName = selectedProject == null ? "" : selectedProject.Name;
            string name = txtName.Text.Trim();
            if (name.Length == 0)
            {
                MessageBox.Show("Project name is required.", "Project", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ProjectRecord existing = FindProject(name);
            if (selectedProject == null && existing != null)
            {
                MessageBox.Show("A project with that name already exists.", "Project", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (selectedProject != null && existing != null && !object.ReferenceEquals(existing, selectedProject))
            {
                MessageBox.Show("A different project with that name already exists.", "Project", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (selectedProject == null)
            {
                selectedProject = new ProjectRecord();
                InventoryStore.Database.Projects.Add(selectedProject);
            }

            selectedProject.Name = name;
            selectedProject.ProjectCode = string.IsNullOrWhiteSpace(txtCode.Text) ? name.Replace(" ", "-").ToUpperInvariant() : txtCode.Text.Trim();
            selectedProject.Owner = txtOwner.Text.Trim();
            selectedProject.Status = Convert.ToString(cboStatus.SelectedItem);
            selectedProject.Priority = Convert.ToString(cboPriority.SelectedItem);
            selectedProject.DueDate = dueDate.Value.Date;
            selectedProject.Notes = txtNotes.Text.Trim();

            if (!string.IsNullOrWhiteSpace(oldName) && !string.Equals(oldName, selectedProject.Name, StringComparison.OrdinalIgnoreCase))
            {
                foreach (var item in InventoryStore.Database.Items)
                    if (string.Equals(item.Project, oldName, StringComparison.OrdinalIgnoreCase)) item.Project = selectedProject.Name;
            }

            InventoryStore.Save();
            LoadProjects();
        }

        private void DeleteProject()
        {
            if (selectedProject == null) return;
            int assigned = InventoryStore.ItemsForProject(selectedProject.Name).Count;
            string msg = "Delete project '" + selectedProject.Name + "'?";
            if (assigned > 0) msg += "\n\n" + assigned + " inventory item(s) are assigned to this project. They will be moved to General.";
            if (MessageBox.Show(msg, "Delete Project", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            foreach (var item in InventoryStore.Database.Items)
                if (string.Equals(item.Project, selectedProject.Name, StringComparison.OrdinalIgnoreCase)) item.Project = "General";
            InventoryStore.Database.Projects.Remove(selectedProject);
            InventoryStore.AddProject("General");
            selectedProject = null;
            InventoryStore.Save();
            LoadProjects();
        }

        private void LoadAssignedItems()
        {
            if (itemGrid == null) return;
            itemGrid.Columns.Clear();
            itemGrid.Rows.Clear();
            itemGrid.Columns.Add("ForgeId", "Forge ID");
            itemGrid.Columns.Add("ItemName", "Item");
            itemGrid.Columns.Add("PartNumber", "Part #");
            itemGrid.Columns.Add("Serial", "Serial");
            itemGrid.Columns.Add("Category", "Category");
            itemGrid.Columns.Add("Location", "Location");
            itemGrid.Columns.Add("Quantity", "Qty");
            itemGrid.Columns.Add("Status", "Status");
            if (selectedProject == null) return;

            foreach (var i in InventoryStore.ItemsForProject(selectedProject.Name))
                itemGrid.Rows.Add(i.ForgeId, i.ItemName, i.PartNumber, i.SerialNumber, i.Category, i.Location, i.Quantity, InventoryStore.GetStatus(i));
            if (itemGrid.Columns.Count > 0)
            {
                itemGrid.Columns["ItemName"].FillWeight = 150;
                itemGrid.Columns["Quantity"].FillWeight = 45;
            }
        }

        private void LoadTransactions()
        {
            if (transactionGrid == null) return;
            transactionGrid.Columns.Clear();
            transactionGrid.Rows.Clear();
            transactionGrid.Columns.Add("Time", "Time");
            transactionGrid.Columns.Add("ForgeId", "Forge ID");
            transactionGrid.Columns.Add("Item", "Item");
            transactionGrid.Columns.Add("Action", "Action");
            transactionGrid.Columns.Add("Delta", "Delta");
            transactionGrid.Columns.Add("Old", "Old");
            transactionGrid.Columns.Add("New", "New");
            transactionGrid.Columns.Add("Notes", "Notes");
            if (selectedProject == null) return;

            foreach (var t in InventoryStore.Database.Transactions)
            {
                InventoryItem item = FindItem(t.ForgeId);
                if (item == null || !string.Equals(item.Project, selectedProject.Name, StringComparison.OrdinalIgnoreCase)) continue;
                transactionGrid.Rows.Add(t.Time.ToString("M/d/yy HH:mm"), t.ForgeId, t.ItemName, t.Action, t.Delta, t.OldQuantity, t.NewQuantity, t.Notes);
            }
        }

        private InventoryItem FindItem(string forgeId)
        {
            foreach (var i in InventoryStore.Database.Items) if (string.Equals(i.ForgeId, forgeId, StringComparison.OrdinalIgnoreCase)) return i;
            return null;
        }

        private InventoryItem SelectedAssignedItem()
        {
            if (itemGrid == null || itemGrid.CurrentRow == null) return null;
            string id = Convert.ToString(itemGrid.CurrentRow.Cells[0].Value);
            return FindItem(id);
        }

        private void AssignItemToProject()
        {
            if (selectedProject == null)
            {
                MessageBox.Show("Select or save a project first.", "Assign Item", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var f = new Form())
            {
                f.Text = "Assign Inventory to Project";
                f.StartPosition = FormStartPosition.CenterParent;
                f.FormBorderStyle = FormBorderStyle.FixedDialog;
                f.MinimizeBox = false;
                f.MaximizeBox = false;
                f.ClientSize = new Size(520, 150);
                f.BackColor = Theme.Panel;

                var label = new Label { Text = "Select inventory item", Left = 14, Top = 14, Width = 480, Height = 22, ForeColor = Theme.Text, Font = Theme.SmallFont };
                var combo = new ComboBox { Left = 14, Top = 40, Width = 490, DropDownStyle = ComboBoxStyle.DropDownList };
                foreach (var i in InventoryStore.Database.Items)
                    combo.Items.Add(i.ForgeId + " | " + i.ItemName + " | " + i.PartNumber + " | Current Project: " + i.Project);
                if (combo.Items.Count > 0) combo.SelectedIndex = 0;
                var ok = new Button { Text = "Assign", Left = 314, Top = 92, Width = 90, Height = 32, DialogResult = DialogResult.OK };
                var cancel = new Button { Text = "Cancel", Left = 414, Top = 92, Width = 90, Height = 32, DialogResult = DialogResult.Cancel };
                f.Controls.Add(label); f.Controls.Add(combo); f.Controls.Add(ok); f.Controls.Add(cancel);
                f.AcceptButton = ok; f.CancelButton = cancel;

                if (f.ShowDialog(this) == DialogResult.OK && combo.SelectedIndex >= 0)
                {
                    InventoryItem item = InventoryStore.Database.Items[combo.SelectedIndex];
                    string oldProject = item.Project;
                    item.Project = selectedProject.Name;
                    InventoryStore.AddTransaction(item, "PROJECT ASSIGN", 0, item.Quantity, item.Quantity, item.Location, item.Location, "Project changed from " + oldProject + " to " + selectedProject.Name);
                    InventoryStore.Save();
                    LoadProjects();
                }
            }
        }

        private void RemoveSelectedItemFromProject()
        {
            if (selectedProject == null) return;
            InventoryItem item = SelectedAssignedItem();
            if (item == null) return;
            if (MessageBox.Show("Remove '" + item.ItemName + "' from project '" + selectedProject.Name + "'?", "Remove Item", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            string oldProject = item.Project;
            item.Project = "General";
            InventoryStore.AddProject("General");
            InventoryStore.AddTransaction(item, "PROJECT REMOVE", 0, item.Quantity, item.Quantity, item.Location, item.Location, "Project changed from " + oldProject + " to General");
            InventoryStore.Save();
            LoadProjects();
        }

        private void OpenSelectedItem()
        {
            InventoryItem item = SelectedAssignedItem();
            if (item == null) return;
            using (var f = new ItemEditorForm(item, new OperationContext()))
            {
                if (f.ShowDialog() == DialogResult.OK)
                {
                    InventoryStore.Save();
                    LoadProjects();
                }
            }
        }

        public void RefreshData()
        {
            LoadProjects();
        }

    }
}
