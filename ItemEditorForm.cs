using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SOACSForgeWorks
{
    public class ItemEditorForm : Form
    {
        private InventoryItem item;
        private TextBox txtForgeId, txtBarcode, txtName, txtCage, txtPart, txtNsn, txtMrl, txtSerial, txtQty, txtReserved, txtAvailable, txtMin, txtMax, txtStatus, txtUnit, txtNotes, txtPhoto, txtBorrowQty;
        private TextBox txtUnitCost, txtStandardCost, txtLastPurchasePrice, txtPreferredVendor, txtVendorPartNumber, txtManufacturer, txtManufacturerPartNumber, txtLeadTimeDays, txtLastVendor, txtReorderQuantity;
        private DateTimePicker dtpLastPurchaseDate;
        private CheckBox chkBorrowed, chkReorder;
        private ComboBox cboCategory, cboLocation, cboProject, cboBorrowedBy;
        private PictureBox photoBox;
        private readonly Dictionary<string, Control> customBoxes = new Dictionary<string, Control>();
        private OperationContext pendingContext;
        private ListView attachmentsList;
        private readonly List<string> pendingAttachmentFiles = new List<string>();

        public ItemEditorForm(InventoryItem existing, OperationContext context)
        {
            item = existing;
            pendingContext = context;
            Text = existing == null ? "Add ForgeWorks Item" : "Item Workspace";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(960, 720);
            MinimumSize = new Size(860, 620);
            BackColor = Theme.Background;
            Font = Theme.NormalFont;
            Build(context == null ? null : context.ScannedValue);
        }

        private void Build(string scanCode)
        {
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(14), BackColor = Theme.Background };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            Controls.Add(root);

            var left = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Panel, Padding = new Padding(12), Margin = new Padding(0, 0, 10, 0) };
            root.Controls.Add(left, 0, 0);
            left.Controls.Add(new Label { Text = "ITEM PHOTO", Dock = DockStyle.Top, Height = 28, ForeColor = Theme.Gold, Font = Theme.HeaderFont, TextAlign = ContentAlignment.MiddleCenter });
            photoBox = new PictureBox { Dock = DockStyle.Top, Height = 220, BackColor = Color.Black, SizeMode = PictureBoxSizeMode.Zoom, BorderStyle = BorderStyle.FixedSingle };
            left.Controls.Add(photoBox); photoBox.BringToFront();
            var browse = Button("Browse Photo"); browse.Dock = DockStyle.Top; browse.Click += (s, e) => BrowsePhoto(); left.Controls.Add(browse); browse.BringToFront();
            var take = Button("Take Photo"); take.Dock = DockStyle.Top; take.Click += (s, e) => TakePhoto(); left.Controls.Add(take); take.BringToFront();
            var clear = Button("Clear Photo"); clear.Dock = DockStyle.Top; clear.Click += (s, e) => { txtPhoto.Text = ""; LoadPhotoPreview(); }; left.Controls.Add(clear); clear.BringToFront();
            left.Controls.Add(new Label { Text = (InventoryStore.IsReadOnly ? "Read-Only Viewer mode: item details can be viewed but not changed." : "Scanner-enabled fields: click Scan Barcode, Scan Serial, or Scan Part Number, then scan the label. Quantity changes are handled through Inventory Transactions, not this workspace."), Dock = DockStyle.Fill, ForeColor = Theme.Muted, Font = Theme.SmallFont, TextAlign = ContentAlignment.TopLeft, Padding = new Padding(4, 16, 4, 4) });

            var right = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Theme.Background };
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            root.Controls.Add(right, 1, 0);

            var tabs = new TabControl { Dock = DockStyle.Fill };
            right.Controls.Add(tabs, 0, 0);

            var coreTab = new TabPage("General");
            var invTab = new TabPage("Inventory");
            var customTab = new TabPage("Dynamic Fields");
            var procTab = new TabPage("Procurement");
            var attachTab = new TabPage("Documents / Attachments");
            var notesTab = new TabPage("Notes / History");
            tabs.TabPages.Add(coreTab); tabs.TabPages.Add(invTab); tabs.TabPages.Add(procTab); tabs.TabPages.Add(attachTab); tabs.TabPages.Add(customTab); tabs.TabPages.Add(notesTab);

            var core = NewEditorTable(); coreTab.Controls.Add(core);
            txtForgeId = AddTextRow(core, "Forge ID");
            txtForgeId.ReadOnly = true;
            txtBarcode = AddScannerTextRow(core, "Barcode / QR", "Scan Barcode");
            txtName = AddTextRow(core, "Nomenclature");
            txtPart = AddScannerTextRow(core, "Part Number", "Scan Part #");
            txtCage = AddTextRow(core, "CAGE Code");
            txtNsn = AddTextRow(core, "NSN");
            txtMrl = AddTextRow(core, "MRL / Master Requirements List");
            txtSerial = AddScannerTextRow(core, "Serial Number", "Scan Serial");
            cboCategory = AddComboRow(core, "Category");
            cboLocation = AddComboRow(core, "Location");
            cboProject = AddComboRow(core, "Project");
            txtUnit = AddTextRow(core, "Unit of Measure");
            txtPhoto = AddTextRow(core, "Photo Path");
            txtPhoto.TextChanged += (s, e) => LoadPhotoPreview();

            var inv = NewEditorTable(); invTab.Controls.Add(inv);
            txtQty = AddTextRow(inv, "Current Quantity"); txtQty.ReadOnly = true;
            txtReserved = AddTextRow(inv, "Reserved Quantity");
            txtAvailable = AddTextRow(inv, "Available Quantity"); txtAvailable.ReadOnly = true;
            txtMin = AddTextRow(inv, "Minimum");
            txtMax = AddTextRow(inv, "Maximum");
            txtStatus = AddTextRow(inv, "Status"); txtStatus.ReadOnly = true;
            chkBorrowed = AddCheckRow(inv, "Borrowed Item");
            txtBorrowQty = AddTextRow(inv, "Borrowed Qty");
            cboBorrowedBy = AddComboRow(inv, "Borrowed By");
            chkReorder = AddCheckRow(inv, "Reorder Required");
            txtReserved.TextChanged += (s, e) => RefreshInventoryReadouts();
            txtQty.TextChanged += (s, e) => RefreshInventoryReadouts();

            var proc = NewEditorTable(); procTab.Controls.Add(proc);
            AddSectionHeader(proc, "Pricing");
            txtUnitCost = AddTextRow(proc, "Unit Cost");
            txtStandardCost = AddTextRow(proc, "Standard Cost");
            txtLastPurchasePrice = AddTextRow(proc, "Last Purchase Price");
            AddSectionHeader(proc, "Vendor / Manufacturer");
            txtPreferredVendor = AddTextRow(proc, "Preferred Vendor");
            txtVendorPartNumber = AddTextRow(proc, "Vendor Part Number");
            txtManufacturer = AddTextRow(proc, "Manufacturer");
            txtManufacturerPartNumber = AddTextRow(proc, "Manufacturer Part Number");
            AddSectionHeader(proc, "Supply Planning");
            txtLeadTimeDays = AddTextRow(proc, "Lead Time (Days)");
            dtpLastPurchaseDate = AddDateRow(proc, "Last Purchase Date");
            txtLastVendor = AddTextRow(proc, "Last Vendor");
            txtReorderQuantity = AddTextRow(proc, "Reorder Quantity");

            var custom = NewEditorTable(); customTab.Controls.Add(custom);
            foreach (var f in InventoryStore.Database.CustomFields)
                customBoxes[f.Name] = AddCustomRow(custom, f);
            if (InventoryStore.Database.CustomFields.Count == 0)
                custom.Controls.Add(new Label { Text = "No custom fields have been created yet. Go to Administration to add fields on the fly.", ForeColor = Theme.Muted, Dock = DockStyle.Top, Height = 40 }, 0, 0);

            BuildAttachmentsTab(attachTab);

            var notesRoot = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, Padding = new Padding(12) };
            notesRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
            notesRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            notesTab.Controls.Add(notesRoot);
            txtNotes = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical };
            notesRoot.Controls.Add(LabelWrap("Notes", txtNotes), 0, 0);
            var history = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true };
            history.Columns.Add("Time", 135); history.Columns.Add("Action", 100); history.Columns.Add("Old", 55); history.Columns.Add("New", 55); history.Columns.Add("Notes", 400);
            if (item != null)
            {
                foreach (var t in InventoryStore.Database.Transactions)
                    if (string.Equals(t.ForgeId, item.ForgeId, StringComparison.OrdinalIgnoreCase))
                    {
                        var li = new ListViewItem(t.Time.ToString("M/d/yy HH:mm"));
                        li.SubItems.Add(t.Action); li.SubItems.Add(t.OldQuantity.ToString()); li.SubItems.Add(t.NewQuantity.ToString()); li.SubItems.Add(t.Notes);
                        history.Items.Add(li);
                    }
            }
            notesRoot.Controls.Add(LabelWrap("Recent History", history), 0, 1);

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, Height = 58, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 10, 0, 0), BackColor = Theme.Background };
            right.Controls.Add(buttons, 0, 1);
            var save = Button("Save"); save.Enabled = !InventoryStore.IsReadOnly; save.Click += (s, e) => SaveItem(); buttons.Controls.Add(save);
            var cancel = Button("Cancel"); cancel.Click += (s, e) => DialogResult = DialogResult.Cancel; buttons.Controls.Add(cancel);
            var removeBtn = Button("Remove Inventory"); removeBtn.Enabled = !InventoryStore.IsReadOnly && item != null; removeBtn.Click += (s, e) => RemoveInventoryFromItem(); buttons.Controls.Add(removeBtn);
            var attachBtn = Button("Add Documents"); attachBtn.Width = 132; attachBtn.Enabled = !InventoryStore.IsReadOnly; attachBtn.Click += (s, e) => AddDocuments(); buttons.Controls.Add(attachBtn);
            var historyBtn = Button("History"); historyBtn.Click += (s, e) => MessageBox.Show(this, "Transaction history is shown on the Notes / History tab. Quantity changes are performed from Inventory, Operations, Storage Explorer, or Projects.", "Item Workspace", MessageBoxButtons.OK, MessageBoxIcon.Information); buttons.Controls.Add(historyBtn);

            if (item != null) Fill();
            else
            {
                txtForgeId.Text = InventoryStore.NextForgeId();
                txtBarcode.Text = scanCode ?? "";
                int startingQty = pendingContext == null ? 0 : Math.Max(0, pendingContext.Quantity);
                txtQty.Text = startingQty.ToString();
                txtReserved.Text = "0";
                txtAvailable.Text = startingQty.ToString();
                txtMin.Text = "0";
                txtMax.Text = "0";
                txtUnit.Text = "Each";
                txtStatus.Text = startingQty > 0 ? "Available" : "Out";
                txtUnitCost.Text = "0.00";
                txtStandardCost.Text = "0.00";
                txtLastPurchasePrice.Text = "0.00";
                txtPreferredVendor.Text = "";
                txtVendorPartNumber.Text = "";
                txtManufacturer.Text = "";
                txtManufacturerPartNumber.Text = "";
                txtLeadTimeDays.Text = "0";
                dtpLastPurchaseDate.Checked = false;
                dtpLastPurchaseDate.Value = DateTime.Today;
                txtLastVendor.Text = "";
                txtReorderQuantity.Text = "0";
                txtBorrowQty.Text = "0";
                chkBorrowed.Checked = false;
                chkReorder.Checked = false;
                if (cboCategory.Items.Count > 0) cboCategory.SelectedIndex = 0;
                if (pendingContext != null && !string.IsNullOrWhiteSpace(pendingContext.Location)) cboLocation.Text = pendingContext.Location;
                else if (cboLocation.Items.Count > 0) cboLocation.SelectedIndex = 0;
                if (pendingContext != null && !string.IsNullOrWhiteSpace(pendingContext.Project)) cboProject.Text = pendingContext.Project;
                else if (cboProject.Items.Count > 0) cboProject.SelectedIndex = 0;
                if (pendingContext != null && !string.IsNullOrWhiteSpace(pendingContext.Notes)) txtNotes.Text = pendingContext.Notes;
            }
            LoadPhotoPreview();
        }

        private TableLayoutPanel NewEditorTable()
        {
            var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, AutoScroll = true, Padding = new Padding(14), BackColor = Color.White };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            return table;
        }

        private Control LabelWrap(string label, Control child)
        {
            var p = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, BackColor = Color.White };
            p.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            p.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            p.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, ForeColor = Color.Black, Font = Theme.HeaderFont }, 0, 0);
            p.Controls.Add(child, 0, 1);
            return p;
        }

        private TextBox AddTextRow(TableLayoutPanel root, string label)
        {
            int row = root.RowCount++; root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.Controls.Add(new Label { Text = label, Height = 32, Dock = DockStyle.Fill, ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleLeft }, 0, row);
            var tb = new TextBox { Dock = DockStyle.Top, Height = 28, Margin = new Padding(0, 3, 0, 6) };
            root.Controls.Add(tb, 1, row); return tb;
        }

        private void AddSectionHeader(TableLayoutPanel root, string text)
        {
            int row = root.RowCount++; root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var lbl = new Label { Text = text.ToUpperInvariant(), Dock = DockStyle.Top, Height = 30, ForeColor = Theme.Gold, Font = Theme.HeaderFont, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(0, 8, 0, 0) };
            root.Controls.Add(lbl, 0, row);
            root.SetColumnSpan(lbl, 2);
        }

        private DateTimePicker AddDateRow(TableLayoutPanel root, string label)
        {
            int row = root.RowCount++; root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.Controls.Add(new Label { Text = label, Height = 32, Dock = DockStyle.Fill, ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleLeft }, 0, row);
            var dt = new DateTimePicker { Dock = DockStyle.Top, Height = 28, Format = DateTimePickerFormat.Short, Margin = new Padding(0, 3, 0, 6), ShowCheckBox = true, Checked = false };
            root.Controls.Add(dt, 1, row); return dt;
        }

        private TextBox AddScannerTextRow(TableLayoutPanel root, string label, string buttonText)
        {
            int row = root.RowCount++; root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.Controls.Add(new Label { Text = label, Height = 32, Dock = DockStyle.Fill, ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleLeft }, 0, row);
            var panel = new TableLayoutPanel { Dock = DockStyle.Top, Height = 34, ColumnCount = 2, Margin = new Padding(0, 3, 0, 6) };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112));
            var tb = new TextBox { Dock = DockStyle.Fill, Height = 28 };
            var scan = new Button { Text = buttonText, Dock = DockStyle.Fill, Height = 28, BackColor = Theme.Panel2, ForeColor = Theme.Text, FlatStyle = FlatStyle.Flat };
            scan.FlatAppearance.BorderColor = Theme.Border;
            scan.Click += (s, e) => BeginScanToField(tb, label);
            panel.Controls.Add(tb, 0, 0);
            panel.Controls.Add(scan, 1, 0);
            root.Controls.Add(panel, 1, row);
            return tb;
        }

        private void BeginScanToField(TextBox target, string label)
        {
            target.Focus();
            target.SelectAll();
            MessageBox.Show(this, "Scan the label now. The scanned value will be placed in: " + label + ".", "Scanner Input", MessageBoxButtons.OK, MessageBoxIcon.Information);
            target.Focus();
            target.SelectAll();
        }


        private TextBox AddDocumentRow(TableLayoutPanel root, string label)
        {
            int row = root.RowCount++; root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.Controls.Add(new Label { Text = label, Height = 32, Dock = DockStyle.Fill, ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleLeft }, 0, row);
            var panel = new TableLayoutPanel { Dock = DockStyle.Top, Height = 34, ColumnCount = 2, Margin = new Padding(0, 3, 0, 6) };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112));
            var tb = new TextBox { Dock = DockStyle.Fill, Height = 28 };
            var browse = new Button { Text = "Browse", Dock = DockStyle.Fill, Height = 28, BackColor = Theme.Panel2, ForeColor = Theme.Text, FlatStyle = FlatStyle.Flat };
            browse.FlatAppearance.BorderColor = Theme.Border;
            browse.Click += (s, e) => BrowseDocument();
            panel.Controls.Add(tb, 0, 0);
            panel.Controls.Add(browse, 1, 0);
            root.Controls.Add(panel, 1, row);
            return tb;
        }

        private CheckBox AddCheckRow(TableLayoutPanel root, string label)
        {
            int row = root.RowCount++; root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.Controls.Add(new Label { Text = label, Height = 32, Dock = DockStyle.Fill, ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleLeft }, 0, row);
            var cb = new CheckBox { Dock = DockStyle.Top, Height = 28, Margin = new Padding(0, 6, 0, 6) };
            root.Controls.Add(cb, 1, row);
            return cb;
        }

        private ComboBox AddComboRow(TableLayoutPanel root, string label)
        {
            int row = root.RowCount++; root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.Controls.Add(new Label { Text = label, Height = 32, Dock = DockStyle.Fill, ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleLeft }, 0, row);
            var cb = new ComboBox { Dock = DockStyle.Top, Height = 28, Margin = new Padding(0, 3, 0, 6), DropDownStyle = ComboBoxStyle.DropDown };
            if (label == "Category") foreach (var c in InventoryStore.Database.Categories) cb.Items.Add(c.Name);
            if (label == "Location") foreach (var l in InventoryStore.Database.Locations) cb.Items.Add(l.Name);
            if (label == "Project" || label == "Borrowed By") foreach (var p in InventoryStore.Database.Projects) cb.Items.Add(p.Name);
            root.Controls.Add(cb, 1, row); return cb;
        }

        private Control AddCustomRow(TableLayoutPanel root, CustomField field)
        {
            int row = root.RowCount++; root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.Controls.Add(new Label { Text = field.Name + " (" + field.Type + ")", Height = 32, Dock = DockStyle.Fill, ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleLeft }, 0, row);
            Control input;
            if (string.Equals(field.Type, "Yes/No", StringComparison.OrdinalIgnoreCase))
            {
                input = new CheckBox { Dock = DockStyle.Top, Height = 28, Margin = new Padding(0, 6, 0, 6) };
            }
            else if (string.Equals(field.Type, "Date", StringComparison.OrdinalIgnoreCase))
            {
                input = new DateTimePicker { Dock = DockStyle.Top, Height = 28, Format = DateTimePickerFormat.Short, Margin = new Padding(0, 3, 0, 6) };
            }
            else
            {
                input = new TextBox { Dock = DockStyle.Top, Height = 28, Margin = new Padding(0, 3, 0, 6) };
            }
            root.Controls.Add(input, 1, row);
            return input;
        }

        private Button Button(string t)
        {
            var b = new Button { Text = t, Width = 112, Height = 34, BackColor = Theme.Panel2, ForeColor = Theme.Text, FlatStyle = FlatStyle.Flat, Margin = new Padding(6, 0, 0, 0) };
            b.FlatAppearance.BorderColor = Theme.Border; return b;
        }


        private void BuildAttachmentsTab(TabPage tab)
        {
            tab.BackColor = Theme.Background;
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1, Padding = new Padding(12), BackColor = Theme.Background };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            tab.Controls.Add(root);

            root.Controls.Add(new Label
            {
                Text = "Attach drawings, manuals, datasheets, photos, CAD files, purchase documents, test reports, or other files to this part record.",
                Dock = DockStyle.Fill,
                ForeColor = Theme.Muted,
                Font = Theme.SmallFont,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);

            attachmentsList = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = false,
                BackColor = Theme.Panel,
                ForeColor = Theme.Text
            };
            attachmentsList.Columns.Add("File Name", 260);
            attachmentsList.Columns.Add("Type", 110);
            attachmentsList.Columns.Add("Category", 150);
            attachmentsList.Columns.Add("Added By", 150);
            attachmentsList.Columns.Add("Added", 120);
            attachmentsList.Columns.Add("Notes", 280);
            attachmentsList.DoubleClick += (s, e) => OpenSelectedAttachment();
            root.Controls.Add(attachmentsList, 0, 1);

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, BackColor = Theme.Background, Padding = new Padding(0, 8, 0, 0) };
            root.Controls.Add(buttons, 0, 2);
            var add = Button("Add Documents"); add.Width = 132; add.Enabled = !InventoryStore.IsReadOnly; add.Click += (s, e) => AddDocuments(); buttons.Controls.Add(add);
            var open = Button("Open"); open.Click += (s, e) => OpenSelectedAttachment(); buttons.Controls.Add(open);
            var print = Button("Print"); print.Click += (s, e) => PrintSelectedAttachment(); buttons.Controls.Add(print);
            var remove = Button("Remove"); remove.Enabled = !InventoryStore.IsReadOnly; remove.Click += (s, e) => RemoveSelectedAttachment(); buttons.Controls.Add(remove);
            var folder = Button("Open Folder"); folder.Width = 120; folder.Click += (s, e) => OpenAttachmentFolder(); buttons.Controls.Add(folder);

            RefreshAttachmentsList();
        }

        private void RefreshAttachmentsList()
        {
            if (attachmentsList == null) return;
            attachmentsList.Items.Clear();
            string forgeId = txtForgeId == null ? (item == null ? "" : item.ForgeId) : txtForgeId.Text.Trim();
            if (!string.IsNullOrWhiteSpace(forgeId))
            {
                foreach (var a in InventoryStore.GetItemAttachments(forgeId))
                {
                    var li = new ListViewItem(string.IsNullOrWhiteSpace(a.FileName) ? Path.GetFileName(InventoryStore.ResolveAttachmentPath(a)) : a.FileName);
                    li.SubItems.Add(a.FileType ?? "");
                    li.SubItems.Add(a.DocumentCategory ?? "Other");
                    li.SubItems.Add(a.AddedBy ?? "");
                    li.SubItems.Add(a.Added == DateTime.MinValue ? "" : a.Added.ToString("M/d/yy HH:mm"));
                    li.SubItems.Add(a.Description ?? "");
                    li.Tag = a;
                    attachmentsList.Items.Add(li);
                }
            }
            foreach (var pending in pendingAttachmentFiles)
            {
                var li = new ListViewItem(Path.GetFileName(pending));
                li.SubItems.Add(Path.GetExtension(pending).TrimStart('.').ToUpperInvariant());
                li.SubItems.Add(InventoryStore.GuessDocumentCategory(pending));
                li.SubItems.Add(InventoryStore.CurrentOperator);
                li.SubItems.Add("Pending Save");
                li.SubItems.Add("Will attach when item is saved");
                li.Tag = pending;
                attachmentsList.Items.Add(li);
            }
        }

        private void AddDocuments()
        {
            if (InventoryStore.IsReadOnly)
            {
                MessageBox.Show(this, "This workstation is in Read-Only Viewer mode.", "ForgeWorks", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Attach Documents to Part";
                ofd.Multiselect = true;
                ofd.Filter = "Common Documents|*.pdf;*.doc;*.docx;*.xls;*.xlsx;*.ppt;*.pptx;*.txt;*.csv;*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.dxf;*.dwg;*.stl;*.step;*.stp;*.iges;*.igs;*.zip|All Files|*.*";
                if (ofd.ShowDialog(this) != DialogResult.OK) return;
                if (item == null)
                {
                    foreach (var f in ofd.FileNames) if (!pendingAttachmentFiles.Contains(f)) pendingAttachmentFiles.Add(f);
                    RefreshAttachmentsList();
                    MessageBox.Show(this, "Document(s) will be attached when this new item is saved.", "ForgeWorks", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    int count = InventoryStore.AddItemAttachments(item, ofd.FileNames, "Added from Item Workspace");
                    RefreshAttachmentsList();
                    MessageBox.Show(this, count + " document(s) attached.", "ForgeWorks", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private AttachmentRecord SelectedAttachmentRecord()
        {
            if (attachmentsList == null || attachmentsList.SelectedItems.Count == 0) return null;
            return attachmentsList.SelectedItems[0].Tag as AttachmentRecord;
        }

        private void OpenSelectedAttachment()
        {
            var rec = SelectedAttachmentRecord();
            string path = rec == null ? null : InventoryStore.ResolveAttachmentPath(rec);
            if (string.IsNullOrWhiteSpace(path) && attachmentsList != null && attachmentsList.SelectedItems.Count > 0)
                path = attachmentsList.SelectedItems[0].Tag as string;
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                MessageBox.Show(this, "The selected document could not be found.", "ForgeWorks", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try { Process.Start(new ProcessStartInfo(path) { UseShellExecute = true }); }
            catch (Exception ex) { MessageBox.Show(this, "Unable to open document: " + ex.Message, "ForgeWorks", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void PrintSelectedAttachment()
        {
            var rec = SelectedAttachmentRecord();
            string path = rec == null ? null : InventoryStore.ResolveAttachmentPath(rec);
            if (string.IsNullOrWhiteSpace(path) && attachmentsList != null && attachmentsList.SelectedItems.Count > 0)
                path = attachmentsList.SelectedItems[0].Tag as string;
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                MessageBox.Show(this, "The selected document could not be found.", "ForgeWorks", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try { Process.Start(new ProcessStartInfo(path) { UseShellExecute = true, Verb = "print" }); }
            catch (Exception ex) { MessageBox.Show(this, "Unable to print document. Windows must have a default print action for this file type. " + ex.Message, "ForgeWorks", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void RemoveSelectedAttachment()
        {
            if (InventoryStore.IsReadOnly) return;
            if (attachmentsList == null || attachmentsList.SelectedItems.Count == 0) return;
            var pending = attachmentsList.SelectedItems[0].Tag as string;
            if (!string.IsNullOrWhiteSpace(pending))
            {
                pendingAttachmentFiles.Remove(pending);
                RefreshAttachmentsList();
                return;
            }
            var rec = SelectedAttachmentRecord();
            if (rec == null) return;
            if (MessageBox.Show(this, "Remove this attachment from the item record?\n\nChoose Yes to remove it from ForgeWorks. The file will also be deleted from the attachment folder.", "Remove Attachment", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            InventoryStore.RemoveItemAttachment(item, rec, true);
            RefreshAttachmentsList();
        }

        private void OpenAttachmentFolder()
        {
            string forgeId = txtForgeId == null ? (item == null ? "" : item.ForgeId) : txtForgeId.Text.Trim();
            if (string.IsNullOrWhiteSpace(forgeId)) return;
            string folder = InventoryStore.GetItemAttachmentFolder(forgeId);
            Directory.CreateDirectory(folder);
            try { Process.Start(new ProcessStartInfo(folder) { UseShellExecute = true }); }
            catch (Exception ex) { MessageBox.Show(this, "Unable to open folder: " + ex.Message, "ForgeWorks", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void BrowseDocument()
        {
            AddDocuments();
        }

        private void RemoveInventoryFromItem()
        {
            if (item == null) return;
            using (var f = new RemoveInventoryForm(item))
            {
                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    Fill();
                    RefreshInventoryReadouts();
                    MessageBox.Show(this, "Inventory removed and transaction logged.", "ForgeWorks", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void BrowsePhoto()
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Select Item Photo";
                ofd.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All Files|*.*";
                if (ofd.ShowDialog(this) == DialogResult.OK) txtPhoto.Text = ofd.FileName;
            }
        }

        private void TakePhoto()
        {
            using (var f = new CameraCaptureForm(string.IsNullOrWhiteSpace(txtForgeId.Text) ? item.ForgeId : txtForgeId.Text.Trim()))
            {
                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    txtPhoto.Text = f.SelectedPhotoPath;
                    LoadPhotoPreview();
                }
            }
        }

        private void LoadPhotoPreview()
        {
            if (photoBox == null || txtPhoto == null) return;
            try
            {
                if (photoBox.Image != null) { var old = photoBox.Image; photoBox.Image = null; old.Dispose(); }
                string p = txtPhoto.Text.Trim();
                if (File.Exists(p)) photoBox.Image = Image.FromFile(p);
            }
            catch { }
        }

        private void Fill()
        {
            txtForgeId.Text = item.ForgeId;
            txtBarcode.Text = item.Barcode;
            txtName.Text = item.ItemName;
            txtCage.Text = item.CageCode;
            txtPart.Text = item.PartNumber;
            txtNsn.Text = item.Nsn;
            txtMrl.Text = item.Mrl;
            txtSerial.Text = item.SerialNumber;
            cboCategory.Text = item.Category;
            cboLocation.Text = item.Location;
            txtQty.Text = item.Quantity.ToString();
            txtReserved.Text = item.ReservedQuantity.ToString();
            txtAvailable.Text = item.AvailableQuantity.ToString();
            txtMin.Text = item.Minimum.ToString();
            txtMax.Text = item.Maximum.ToString();
            txtStatus.Text = InventoryStore.GetStatus(item);
            cboProject.Text = item.Project;
            txtUnit.Text = item.UnitOfMeasure;
            txtPhoto.Text = item.PhotoPath;
            txtUnitCost.Text = item.Cost.ToString("0.00");
            txtStandardCost.Text = item.StandardCost.ToString("0.00");
            txtLastPurchasePrice.Text = item.LastPurchasePrice.ToString("0.00");
            txtPreferredVendor.Text = string.IsNullOrWhiteSpace(item.PreferredVendor) ? item.Vendor : item.PreferredVendor;
            txtVendorPartNumber.Text = item.VendorPartNumber;
            txtManufacturer.Text = item.Manufacturer;
            txtManufacturerPartNumber.Text = string.IsNullOrWhiteSpace(item.ManufacturerPartNumber) ? item.PartNumber : item.ManufacturerPartNumber;
            txtLeadTimeDays.Text = item.LeadTimeDays.ToString();
            dtpLastPurchaseDate.Checked = item.LastPurchaseDate != DateTime.MinValue || item.PurchaseDate != DateTime.MinValue;
            dtpLastPurchaseDate.Value = item.LastPurchaseDate != DateTime.MinValue ? item.LastPurchaseDate : (item.PurchaseDate != DateTime.MinValue ? item.PurchaseDate : DateTime.Today);
            txtLastVendor.Text = item.LastVendor;
            txtReorderQuantity.Text = item.ReorderQuantity.ToString();
            txtBorrowQty.Text = item.BorrowedQuantity.ToString();
            chkBorrowed.Checked = item.BorrowedItem;
            cboBorrowedBy.Text = item.BorrowedBy;
            chkReorder.Checked = item.ReorderRequired;
            txtNotes.Text = item.Notes;
            foreach (var v in item.CustomValues) if (customBoxes.ContainsKey(v.Name)) SetCustomValue(customBoxes[v.Name], v.Value);
        }

        private void SaveItem() { SaveItem(true); }

        private void SaveItem(bool close)
        {
            if (InventoryStore.IsReadOnly) { MessageBox.Show(this, "This workstation is in Read-Only Viewer mode.", "ForgeWorks", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            int qty, reserved, min, max, borrowedQty, leadTimeDays, reorderQty;
            decimal unitCost, standardCost, lastPurchasePrice;
            if (!int.TryParse(txtQty.Text, out qty)) qty = 0;
            if (!int.TryParse(txtReserved.Text, out reserved)) reserved = 0;
            if (!int.TryParse(txtMin.Text, out min)) min = 0;
            if (!int.TryParse(txtMax.Text, out max)) max = 0;
            if (!int.TryParse(txtBorrowQty.Text, out borrowedQty)) borrowedQty = 0;
            if (!int.TryParse(txtLeadTimeDays.Text, out leadTimeDays)) leadTimeDays = 0;
            if (!int.TryParse(txtReorderQuantity.Text, out reorderQty)) reorderQty = 0;
            if (!decimal.TryParse((txtUnitCost.Text ?? "").Replace("$", ""), out unitCost)) unitCost = 0m;
            if (!decimal.TryParse((txtStandardCost.Text ?? "").Replace("$", ""), out standardCost)) standardCost = 0m;
            if (!decimal.TryParse((txtLastPurchasePrice.Text ?? "").Replace("$", ""), out lastPurchasePrice)) lastPurchasePrice = 0m;
            bool isNew = item == null; if (isNew) item = new InventoryItem();
            int oldQty = item.Quantity;
            item.ForgeId = txtForgeId.Text.Trim();
            item.Barcode = txtBarcode.Text.Trim();
            item.ItemName = txtName.Text.Trim();
            item.CageCode = txtCage.Text.Trim();
            item.PartNumber = txtPart.Text.Trim();
            item.Nsn = txtNsn.Text.Trim();
            item.Mrl = txtMrl.Text.Trim();
            item.SerialNumber = txtSerial.Text.Trim();
            item.Category = cboCategory.Text.Trim();
            item.Location = cboLocation.Text.Trim();
            item.Quantity = qty;
            item.ReservedQuantity = Math.Max(0, Math.Min(reserved, qty));
            item.Minimum = min;
            item.Maximum = max;
            item.Status = InventoryStore.GetStatus(item);
            item.Project = cboProject.Text.Trim();
            item.UnitOfMeasure = txtUnit.Text.Trim();
            item.PhotoPath = txtPhoto.Text.Trim();
            item.Cost = Math.Max(0m, unitCost);
            item.StandardCost = Math.Max(0m, standardCost);
            item.LastPurchasePrice = Math.Max(0m, lastPurchasePrice);
            item.PreferredVendor = txtPreferredVendor.Text.Trim();
            item.Vendor = item.PreferredVendor;
            item.VendorPartNumber = txtVendorPartNumber.Text.Trim();
            item.Manufacturer = txtManufacturer.Text.Trim();
            item.ManufacturerPartNumber = txtManufacturerPartNumber.Text.Trim();
            item.LeadTimeDays = Math.Max(0, leadTimeDays);
            item.LastPurchaseDate = dtpLastPurchaseDate.Checked ? dtpLastPurchaseDate.Value.Date : DateTime.MinValue;
            item.PurchaseDate = item.LastPurchaseDate;
            item.LastVendor = txtLastVendor.Text.Trim();
            item.ReorderQuantity = Math.Max(0, reorderQty);
            // Legacy single-document path is preserved for backward compatibility.
            if (string.IsNullOrWhiteSpace(item.DocumentPath)) item.DocumentPath = "";
            item.BorrowedItem = chkBorrowed.Checked;
            item.BorrowedQuantity = Math.Max(0, Math.Min(borrowedQty, qty));
            item.BorrowedBy = cboBorrowedBy.Text.Trim();
            item.BorrowedFromProject = item.BorrowedItem && string.IsNullOrWhiteSpace(item.BorrowedFromProject) ? item.Project : item.BorrowedFromProject;
            item.ReorderRequired = chkReorder.Checked || item.BorrowedItem;
            item.Notes = txtNotes.Text.Trim();
            item.CustomValues.Clear();
            foreach (var kv in customBoxes) item.CustomValues.Add(new CustomValue { Name = kv.Key, Value = GetCustomValue(kv.Value) });
            InventoryStore.AddOrUpdateItem(item, isNew);
            if (pendingAttachmentFiles.Count > 0)
            {
                InventoryStore.AddItemAttachments(item, pendingAttachmentFiles.ToArray(), "Added from Item Workspace");
                pendingAttachmentFiles.Clear();
                RefreshAttachmentsList();
            }
            InventoryStore.AddTransaction(item, isNew ? "ADD" : "EDIT", qty - oldQty, oldQty, qty, item.Location, item.Location, isNew ? "Item created" : "Item updated");
            if (close) DialogResult = DialogResult.OK;
        }

        private void RefreshInventoryReadouts()
        {
            int qty, reserved;
            if (!int.TryParse(txtQty == null ? "0" : txtQty.Text, out qty)) qty = 0;
            if (!int.TryParse(txtReserved == null ? "0" : txtReserved.Text, out reserved)) reserved = 0;
            if (reserved < 0) reserved = 0;
            if (reserved > qty) reserved = qty;
            if (txtAvailable != null) txtAvailable.Text = (qty - reserved).ToString();
            if (txtStatus != null)
            {
                if (qty <= 0) txtStatus.Text = "Out";
                else if (qty - reserved <= 0 && reserved > 0) txtStatus.Text = "Reserved";
                else if (txtMin != null)
                {
                    int min; if (!int.TryParse(txtMin.Text, out min)) min = 0;
                    txtStatus.Text = (min > 0 && qty - reserved <= min) ? "Low" : "Available";
                }
            }
        }

        private string GetCustomValue(Control c)
        {
            var cb = c as CheckBox; if (cb != null) return cb.Checked ? "Yes" : "No";
            var dt = c as DateTimePicker; if (dt != null) return dt.Value.ToShortDateString();
            return c.Text;
        }

        private void SetCustomValue(Control c, string value)
        {
            var cb = c as CheckBox; if (cb != null) { cb.Checked = string.Equals(value, "Yes", StringComparison.OrdinalIgnoreCase) || string.Equals(value, "True", StringComparison.OrdinalIgnoreCase); return; }
            var dt = c as DateTimePicker; DateTime d; if (dt != null && DateTime.TryParse(value, out d)) { dt.Value = d; return; }
            c.Text = value;
        }
    }
}
