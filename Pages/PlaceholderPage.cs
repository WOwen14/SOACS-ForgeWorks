using System.Drawing;
using System.Windows.Forms;

namespace SOACSForgeWorks.Pages
{
    public class PlaceholderPage : UserControl, ILiveRefreshable
    {
        public PlaceholderPage(string title, string message)
        {
            BackColor = Theme.Background;
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Panel, Padding = new Padding(28) };
            panel.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, panel.ClientRectangle, Theme.Border, ButtonBorderStyle.Solid);
            Controls.Add(panel);
            panel.Controls.Add(new Label { Text = title, Dock = DockStyle.Top, Height = 54, ForeColor = Theme.Text, Font = new Font("Segoe UI Semibold", 22, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft });
            panel.Controls.Add(new Label { Text = message, Dock = DockStyle.Top, Height = 90, ForeColor = Theme.Muted, Font = Theme.NormalFont, TextAlign = ContentAlignment.TopLeft });
            panel.Controls.Add(new Label { Text = "This is a modular UserControl placeholder. Functionality will be re-added one page at a time after the layout foundation is verified.", Dock = DockStyle.Top, Height = 90, ForeColor = Theme.Gold, Font = Theme.NormalFont, TextAlign = ContentAlignment.TopLeft });
        }

        public void RefreshData()
        {
            // Placeholder page.
        }

    }
}
