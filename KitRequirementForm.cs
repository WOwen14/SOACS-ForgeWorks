using System;
using System.Drawing;
using System.Windows.Forms;

namespace SOACSForgeWorks
{
    public class KitRequirementForm : Form
    {
        private TextBox kitBox, searchBox, qtyBox, notesBox;
        private ListBox resultList;
        private InventoryItem selectedItem;

        public KitRequirementForm()
        {
            Text = "Add Kit Requirement";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(640, 520);
            BackColor = Theme.Background;
            Font = Theme.NormalFont;
            Build();
        }

        private void Build()
        {
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 6, ColumnCount = 1, Padding = new Padding(16), BackColor = Theme.Background };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            Controls.Add(root);

            kitBox = TextBox("A-Kit");
            root.Controls.Add(Wrap("Kit / Requirement Name", kitBox), 0, 0);
            searchBox = TextBox(""); searchBox.TextChanged += (s, e) => LoadResults();
            root.Controls.Add(Wrap("Search inventory item by nomenclature, NSN, part number, barcode, or MRL", searchBox), 0, 1);
            resultList = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.Input,
                ForeColor = Theme.Text,
                Font = Theme.NormalFont,
                FormattingEnabled = true,
                HorizontalScrollbar = true
            };
            resultList.SelectedIndexChanged += (s, e) => selectedItem = resultList.SelectedItem as InventoryItem;
            resultList.Format += (s, e) =>
            {
                var i = e.ListItem as InventoryItem;
                if (i != null)
                {
                    e.Value = string.Format("{0} | {1} | Part: {2} | NSN: {3} | Qty: {4} | Location: {5}",
                        i.ForgeId,
                        string.IsNullOrWhiteSpace(i.ItemName) ? "Unnamed Item" : i.ItemName,
                        string.IsNullOrWhiteSpace(i.PartNumber) ? "No Part #" : i.PartNumber,
                        string.IsNullOrWhiteSpace(i.Nsn) ? "No NSN" : i.Nsn,
                        i.Quantity,
                        i.Location);
                }
            };
            root.Controls.Add(Wrap("Matching Inventory", resultList), 0, 2);
            qtyBox = TextBox("1");
            root.Controls.Add(Wrap("Required Quantity", qtyBox), 0, 3);
            notesBox = TextBox(""); notesBox.Multiline = true;
            root.Controls.Add(Wrap("Notes", notesBox), 0, 4);
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Theme.Background };
            root.Controls.Add(buttons, 0, 5);
            var save = Btn("Add Requirement"); save.Click += (s, e) => SaveRequirement(); buttons.Controls.Add(save);
            var cancel = Btn("Cancel"); cancel.Click += (s, e) => DialogResult = DialogResult.Cancel; buttons.Controls.Add(cancel);
            LoadResults();
        }

        private TextBox TextBox(string text) { return new TextBox { Dock = DockStyle.Fill, Text = text, BackColor = Theme.Input, ForeColor = Theme.Text, BorderStyle = BorderStyle.FixedSingle }; }
        private Button Btn(string text) { var b = new Button { Text = text, Width = 140, Height = 34, BackColor = Theme.Panel2, ForeColor = Theme.Text, FlatStyle = FlatStyle.Flat, Margin = new Padding(6, 8, 0, 0) }; b.FlatAppearance.BorderColor = Theme.Border; return b; }
        private Control Wrap(string label, Control child)
        {
            var p = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, BackColor = Theme.Background };
            p.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            p.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            p.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, ForeColor = Theme.Muted, Font = Theme.SmallFont }, 0, 0);
            p.Controls.Add(child, 0, 1);
            return p;
        }
        private void LoadResults()
        {
            resultList.Items.Clear();
            foreach (var i in InventoryStore.SearchItems(searchBox == null ? "" : searchBox.Text)) resultList.Items.Add(i);
            if (resultList.Items.Count > 0) resultList.SelectedIndex = 0;
        }
        private void SaveRequirement()
        {
            if (selectedItem == null) { MessageBox.Show(this, "Select an inventory item first."); return; }
            int qty; if (!int.TryParse(qtyBox.Text, out qty)) qty = 1;
            InventoryStore.AddKitRequirement(kitBox.Text, selectedItem, qty, notesBox.Text);
            DialogResult = DialogResult.OK;
        }
    }
}
