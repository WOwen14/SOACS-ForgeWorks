using System;
using System.Drawing;
using System.Windows.Forms;

namespace SOACSForgeWorks
{
    public class AdjustQuantityForm : Form
    {
        private NumericUpDown qtyBox;
        private ComboBox reasonBox;
        private TextBox notesBox;

        public int NewQuantity { get; private set; }
        public string Reason { get; private set; }
        public string Notes { get; private set; }

        public AdjustQuantityForm(InventoryItem item)
        {
            Text = "Adjust Quantity";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(520, 330);
            BackColor = Theme.Background;
            ForeColor = Theme.Text;
            Font = Theme.NormalFont;

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 7,
                ColumnCount = 1,
                Padding = new Padding(18),
                BackColor = Theme.Background
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 12));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            Controls.Add(root);

            root.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Text = (item == null ? "Unknown Item" : item.ForgeId + "  |  " + item.ItemName),
                ForeColor = Theme.Gold,
                Font = Theme.HeaderFont,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);

            root.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Text = "Current Quantity: " + (item == null ? "--" : item.Quantity.ToString()),
                ForeColor = Theme.Text,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 1);

            qtyBox = new NumericUpDown
            {
                Dock = DockStyle.Fill,
                Minimum = 0,
                Maximum = 1000000,
                Value = item == null ? 0 : Math.Max(0, item.Quantity),
                BackColor = Theme.Input,
                ForeColor = Theme.Text,
                Font = Theme.NormalFont
            };
            root.Controls.Add(Wrap("New Quantity", qtyBox), 0, 2);

            reasonBox = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDown,
                BackColor = Theme.Input,
                ForeColor = Theme.Text,
                FlatStyle = FlatStyle.Flat
            };
            reasonBox.Items.AddRange(new object[] { "Cycle count correction", "Entry mistake", "Found stock", "Damaged", "Lost", "Used but not issued", "Manual correction", "Other" });
            reasonBox.SelectedIndex = 0;
            root.Controls.Add(Wrap("Reason", reasonBox), 0, 3);

            notesBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                BackColor = Theme.Input,
                ForeColor = Theme.Text,
                BorderStyle = BorderStyle.FixedSingle
            };
            root.Controls.Add(Wrap("Notes", notesBox), 0, 4);

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Theme.Background };
            var save = Button("Save Adjustment", true);
            var cancel = Button("Cancel", false);
            save.Click += (s, e) =>
            {
                NewQuantity = (int)qtyBox.Value;
                Reason = reasonBox.Text.Trim();
                Notes = notesBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(Reason))
                {
                    MessageBox.Show("Enter a reason for the quantity adjustment.", "Reason Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                DialogResult = DialogResult.OK;
                Close();
            };
            cancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            buttons.Controls.Add(save);
            buttons.Controls.Add(cancel);
            root.Controls.Add(buttons, 0, 6);
        }

        private Control Wrap(string label, Control child)
        {
            var p = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, BackColor = Theme.Background };
            p.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));
            p.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            p.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, ForeColor = Theme.Muted, Font = Theme.SmallFont }, 0, 0);
            p.Controls.Add(child, 0, 1);
            return p;
        }

        private Button Button(string text, bool primary)
        {
            var b = new Button
            {
                Text = text,
                Width = primary ? 150 : 100,
                Height = 34,
                BackColor = primary ? Theme.Gold : Theme.Panel2,
                ForeColor = primary ? Color.Black : Theme.Text,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(8, 4, 0, 4)
            };
            b.FlatAppearance.BorderColor = Theme.Border;
            return b;
        }
    }
}
