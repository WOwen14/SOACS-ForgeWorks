using System.Drawing;

namespace SOACSForgeWorks
{
    internal static class Theme
    {
        public static readonly Color Background = Color.FromArgb(12, 16, 17);
        public static readonly Color Panel = Color.FromArgb(18, 23, 25);
        public static readonly Color Panel2 = Color.FromArgb(24, 29, 31);
        public static readonly Color Header = Color.FromArgb(4, 5, 6);
        public static readonly Color Border = Color.FromArgb(54, 61, 64);
        public static readonly Color BorderSoft = Color.FromArgb(45, 52, 55);
        public static readonly Color Input = Color.FromArgb(29, 35, 37);
        public static readonly Color InputFocus = Color.FromArgb(34, 41, 44);
        public static readonly Color Gold = Color.FromArgb(181, 158, 72);
        public static readonly Color Text = Color.FromArgb(232, 235, 236);
        public static readonly Color Muted = Color.FromArgb(165, 172, 176);
        public static readonly Color Green = Color.FromArgb(91, 184, 72);
        public static readonly Color GridAlt = Color.FromArgb(22, 28, 30);
        public static readonly Color GridRow = Color.FromArgb(14, 19, 20);
        public static readonly Color GridSelect = Color.FromArgb(181, 158, 72);
        public static readonly Font TitleFont = new Font("Segoe UI Semibold", 24F, FontStyle.Bold);
        public static readonly Font HeaderFont = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
        public static readonly Font NormalFont = new Font("Segoe UI", 10F, FontStyle.Regular);
        public static readonly Font SmallFont = new Font("Segoe UI", 8.5F, FontStyle.Regular);

        public static void ApplyToTree(System.Windows.Forms.Control root)
        {
            if (root == null) return;
            ApplyToControl(root);
            foreach (System.Windows.Forms.Control child in root.Controls)
                ApplyToTree(child);
        }

        private static void ApplyToControl(System.Windows.Forms.Control c)
        {
            var tb = c as System.Windows.Forms.TextBox;
            if (tb != null)
            {
                tb.BackColor = Input;
                tb.ForeColor = Text;
                tb.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                if (!tb.ReadOnly)
                {
                    tb.GotFocus += (s, e) => ((System.Windows.Forms.TextBox)s).BackColor = InputFocus;
                    tb.LostFocus += (s, e) => ((System.Windows.Forms.TextBox)s).BackColor = Input;
                }
                return;
            }

            var cb = c as System.Windows.Forms.ComboBox;
            if (cb != null)
            {
                cb.BackColor = Input;
                cb.ForeColor = Text;
                cb.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                cb.GotFocus += (s, e) => ((System.Windows.Forms.ComboBox)s).BackColor = InputFocus;
                cb.LostFocus += (s, e) => ((System.Windows.Forms.ComboBox)s).BackColor = Input;
                return;
            }

            var lb = c as System.Windows.Forms.ListBox;
            if (lb != null)
            {
                lb.BackColor = Input;
                lb.ForeColor = Text;
                lb.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                return;
            }

            var clb = c as System.Windows.Forms.CheckedListBox;
            if (clb != null)
            {
                clb.BackColor = Input;
                clb.ForeColor = Text;
                clb.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                return;
            }

            var dgv = c as System.Windows.Forms.DataGridView;
            if (dgv != null)
            {
                ApplyGridTheme(dgv);
                return;
            }
        }
        public static void ApplyGridTheme(System.Windows.Forms.DataGridView dgv)
        {
            if (dgv == null) return;
            dgv.BackgroundColor = Background;
            dgv.GridColor = BorderSoft;
            dgv.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            dgv.EnableHeadersVisualStyles = false;
            dgv.RowHeadersVisible = false;
            dgv.AllowUserToResizeRows = false;
            dgv.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.None;
            dgv.RowTemplate.Height = 30;

            dgv.DefaultCellStyle.BackColor = GridRow;
            dgv.DefaultCellStyle.ForeColor = Text;
            dgv.DefaultCellStyle.SelectionBackColor = GridSelect;
            dgv.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;
            dgv.DefaultCellStyle.Font = NormalFont;
            dgv.DefaultCellStyle.Padding = new System.Windows.Forms.Padding(4, 2, 4, 2);

            dgv.AlternatingRowsDefaultCellStyle.BackColor = GridAlt;
            dgv.AlternatingRowsDefaultCellStyle.ForeColor = Text;
            dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = GridSelect;
            dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Panel2;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Text;
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = Panel2;
            dgv.ColumnHeadersDefaultCellStyle.SelectionForeColor = Text;
            dgv.ColumnHeadersDefaultCellStyle.Font = HeaderFont;
            dgv.ColumnHeadersHeight = 34;
            dgv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            dgv.RowHeadersDefaultCellStyle.BackColor = Panel2;
            dgv.RowHeadersDefaultCellStyle.ForeColor = Text;
            dgv.RowsDefaultCellStyle.BackColor = GridRow;
            dgv.RowsDefaultCellStyle.ForeColor = Text;
            dgv.RowsDefaultCellStyle.SelectionBackColor = GridSelect;
            dgv.RowsDefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;
        }

    }
}
