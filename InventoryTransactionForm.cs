using System;
using System.Drawing;
using System.Windows.Forms;

namespace SOACSForgeWorks
{
    public class InventoryTransactionForm : Form
    {
        private readonly InventoryItem item;
        private ComboBox cboAction, cboProject, cboLocation;
        private NumericUpDown qtyBox;
        private TextBox txtReason, txtNotes;
        private Label lblSummary;

        public InventoryTransactionForm(InventoryItem item, string defaultAction)
        {
            this.item = item;
            Text = "Inventory Transaction";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(560, 460);
            MinimumSize = new Size(520, 420);
            BackColor = Theme.Background;
            Font = Theme.NormalFont;
            Build(defaultAction);
        }

        private void Build(string defaultAction)
        {
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4, ColumnCount = 1, Padding = new Padding(14), BackColor = Theme.Background };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            Controls.Add(root);

            lblSummary = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = Theme.Text,
                Font = Theme.HeaderFont,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = item == null ? "No item selected" : item.ForgeId + "  |  " + item.ItemName + "\nOn Hand: " + item.Quantity + "    Reserved: " + item.ReservedQuantity + "    Available: " + item.AvailableQuantity
            };
            root.Controls.Add(lblSummary, 0, 0);

            var form = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, BackColor = Color.White, Padding = new Padding(14) };
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.Controls.Add(form, 0, 1);

            cboAction = Combo(new object[] { "Receive", "Issue", "Return", "Scrap", "Cycle Count", "Reserve", "Move", "Inspect" });
            AddRow(form, "Action", cboAction);
            cboAction.Text = string.IsNullOrWhiteSpace(defaultAction) ? "Receive" : defaultAction;
            cboAction.SelectedIndexChanged += (s, e) => UpdateLocationEnabled();

            qtyBox = new NumericUpDown { Dock = DockStyle.Top, Minimum = 0, Maximum = 1000000, Value = 1, Height = 28 };
            AddRow(form, "Quantity", qtyBox);

            cboProject = Combo(null);
            cboProject.Items.Add("");
            foreach (var p in InventoryStore.Database.Projects) cboProject.Items.Add(p.Name);
            if (item != null) cboProject.Text = item.Project;
            AddRow(form, "Project", cboProject);

            cboLocation = Combo(null);
            foreach (var l in InventoryStore.Database.Locations) cboLocation.Items.Add(l.Name);
            if (item != null) cboLocation.Text = item.Location;
            AddRow(form, "New Location", cboLocation);

            txtReason = new TextBox { Dock = DockStyle.Top, Height = 28 };
            AddRow(form, "Reason", txtReason);

            txtNotes = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical };
            AddRow(form, "Notes", txtNotes, 90);

            root.Controls.Add(new Label { Text = "All quantity and reservation changes are written to transaction history.", Dock = DockStyle.Fill, ForeColor = Theme.Muted, TextAlign = ContentAlignment.MiddleLeft }, 0, 2);

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Theme.Background, Padding = new Padding(0, 5, 0, 0) };
            var ok = Button("Commit"); ok.Click += (s, e) => Commit(); buttons.Controls.Add(ok);
            var cancel = Button("Cancel"); cancel.Click += (s, e) => DialogResult = DialogResult.Cancel; buttons.Controls.Add(cancel);
            root.Controls.Add(buttons, 0, 3);
            UpdateLocationEnabled();
        }

        private ComboBox Combo(object[] items)
        {
            var cb = new ComboBox { Dock = DockStyle.Top, Height = 28, DropDownStyle = ComboBoxStyle.DropDown };
            if (items != null) cb.Items.AddRange(items);
            return cb;
        }

        private void AddRow(TableLayoutPanel form, string label, Control input, int height = 38)
        {
            int row = form.RowCount++;
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
            form.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleLeft }, 0, row);
            form.Controls.Add(input, 1, row);
        }

        private Button Button(string text)
        {
            var b = new Button { Text = text, Width = 100, Height = 34, BackColor = Theme.Panel2, ForeColor = Theme.Text, FlatStyle = FlatStyle.Flat, Margin = new Padding(6, 0, 0, 0) };
            b.FlatAppearance.BorderColor = Theme.Border;
            return b;
        }

        private void UpdateLocationEnabled()
        {
            string action = Convert.ToString(cboAction.Text);
            cboLocation.Enabled = string.Equals(action, "Move", StringComparison.OrdinalIgnoreCase);
        }

        private void Commit()
        {
            if (item == null) { DialogResult = DialogResult.Cancel; return; }
            string action = Convert.ToString(cboAction.Text).Trim();
            int qty = (int)qtyBox.Value;
            string project = Convert.ToString(cboProject.Text).Trim();
            string reason = Convert.ToString(txtReason.Text).Trim();
            string notes = Convert.ToString(txtNotes.Text).Trim();
            string combinedNotes = (string.IsNullOrWhiteSpace(reason) ? "" : "Reason: " + reason + ". ") + notes;
            if (!string.IsNullOrWhiteSpace(project)) item.Project = project;

            if (string.Equals(action, "Receive", StringComparison.OrdinalIgnoreCase)) InventoryStore.AdjustQuantity(item, qty, "RECEIVE", combinedNotes);
            else if (string.Equals(action, "Issue", StringComparison.OrdinalIgnoreCase)) InventoryStore.AdjustQuantity(item, -qty, "ISSUE", combinedNotes);
            else if (string.Equals(action, "Return", StringComparison.OrdinalIgnoreCase)) InventoryStore.AdjustQuantity(item, qty, "RETURN", combinedNotes);
            else if (string.Equals(action, "Scrap", StringComparison.OrdinalIgnoreCase)) InventoryStore.AdjustQuantity(item, -qty, "SCRAP", combinedNotes);
            else if (string.Equals(action, "Cycle Count", StringComparison.OrdinalIgnoreCase)) InventoryStore.SetAbsoluteQuantity(item, qty, combinedNotes);
            else if (string.Equals(action, "Reserve", StringComparison.OrdinalIgnoreCase)) InventoryStore.SetReservedQuantity(item, qty, project, combinedNotes);
            else if (string.Equals(action, "Move", StringComparison.OrdinalIgnoreCase)) InventoryStore.MoveItem(item, Convert.ToString(cboLocation.Text).Trim(), combinedNotes);
            else InventoryStore.AddTransaction(item, "INSPECT", 0, combinedNotes);

            DialogResult = DialogResult.OK;
        }
    }
}
