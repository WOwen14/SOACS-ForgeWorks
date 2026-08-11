using System;
using System.Drawing;
using System.Windows.Forms;

namespace SOACSForgeWorks
{
    public class RemoveInventoryForm : Form
    {
        private readonly InventoryItem item;
        private NumericUpDown qtyBox;
        private TextBox reasonBox;
        private TextBox notesBox;

        public RemoveInventoryForm(InventoryItem item)
        {
            this.item = item;
            Text = "Remove Inventory";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(520, 360);
            MinimumSize = new Size(480, 320);
            BackColor = Theme.Background;
            Font = Theme.NormalFont;
            Build();
        }

        private void Build()
        {
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 5, ColumnCount = 1, Padding = new Padding(16), BackColor = Theme.Background };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            Controls.Add(root);

            var summary = new Label
            {
                Text = (item == null ? "No item selected" : item.ForgeId + "  " + item.ItemName + "\nOn Hand: " + item.Quantity + "    Project: " + item.Project),
                Dock = DockStyle.Fill,
                ForeColor = Theme.Text,
                Font = Theme.HeaderFont,
                TextAlign = ContentAlignment.MiddleLeft
            };
            root.Controls.Add(summary, 0, 0);

            qtyBox = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 1, Maximum = item == null ? 1 : Math.Max(1, item.Quantity), Value = 1, BackColor = Theme.Input, ForeColor = Theme.Text };
            root.Controls.Add(Wrap("How many are being removed?", qtyBox), 0, 1);

            reasonBox = new TextBox { Dock = DockStyle.Fill, Text = "Removed from inventory", BackColor = Theme.Input, ForeColor = Theme.Text, BorderStyle = BorderStyle.FixedSingle };
            root.Controls.Add(Wrap("Reason", reasonBox), 0, 2);

            notesBox = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical, BackColor = Theme.Input, ForeColor = Theme.Text, BorderStyle = BorderStyle.FixedSingle };
            root.Controls.Add(Wrap("Notes", notesBox), 0, 3);

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Theme.Background };
            root.Controls.Add(buttons, 0, 4);
            var remove = Btn("Remove"); remove.Click += (s, e) => Remove(); buttons.Controls.Add(remove);
            var cancel = Btn("Cancel"); cancel.Click += (s, e) => DialogResult = DialogResult.Cancel; buttons.Controls.Add(cancel);
        }

        private Control Wrap(string label, Control child)
        {
            var p = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, BackColor = Theme.Background };
            p.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            p.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            p.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, ForeColor = Theme.Muted, Font = Theme.SmallFont }, 0, 0);
            p.Controls.Add(child, 0, 1);
            return p;
        }

        private Button Btn(string text)
        {
            var b = new Button { Text = text, Width = 112, Height = 34, BackColor = Theme.Panel2, ForeColor = Theme.Text, FlatStyle = FlatStyle.Flat, Margin = new Padding(6, 8, 0, 0) };
            b.FlatAppearance.BorderColor = Theme.Border;
            return b;
        }

        private void Remove()
        {
            if (item == null) return;
            int qty = Convert.ToInt32(qtyBox.Value);
            string msg = "Remove " + qty + " from:\n\n" + item.ForgeId + "\n" + item.ItemName + "\n\nCurrent Qty: " + item.Quantity + "\nNew Qty: " + Math.Max(0, item.Quantity - qty);
            if (MessageBox.Show(this, msg, "Confirm Remove Inventory", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            InventoryStore.RemoveInventory(item, qty, reasonBox.Text, notesBox.Text);
            DialogResult = DialogResult.OK;
        }
    }
}
