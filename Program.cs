using System;
using System.Windows.Forms;

namespace SOACSForgeWorks
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            using (var splash = new SplashForm()) splash.ShowDialog();
            Application.Run(new MainForm());
        }
    }
}
