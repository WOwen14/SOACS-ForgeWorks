using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace SOACSForgeWorks.Core
{
    public static class FeedbackManager
    {
        public const string ApplicationVersion = "3.1.4 RC1";

        public static string CreateFeedback(string category, string comments, string activeWorkspace, bool includeScreenshot, bool includeRepositoryHealth, bool includeSystemInfo, Form owner)
        {
            RepositoryManager.EnsureRepository();
            string root = RepositoryManager.FeedbackFolder;
            Directory.CreateDirectory(root);
            string folderName = DateTime.Now.ToString("yyyy-MM-dd_HHmmss") + "_" + MakeSafe(category);
            string folder = Path.Combine(root, folderName);
            Directory.CreateDirectory(folder);

            File.WriteAllText(Path.Combine(folder, "Feedback.xml"), BuildFeedbackXml(category, comments, activeWorkspace), Encoding.UTF8);

            if (includeSystemInfo)
                File.WriteAllText(Path.Combine(folder, "SystemInfo.txt"), BuildSystemInfo(activeWorkspace), Encoding.UTF8);

            if (includeRepositoryHealth)
                File.WriteAllText(Path.Combine(folder, "RepositoryHealth.txt"), BuildRepositoryHealth(), Encoding.UTF8);

            if (includeScreenshot && owner != null)
            {
                try { CaptureWindow(owner, Path.Combine(folder, "Screenshot.png")); }
                catch (Exception ex) { File.WriteAllText(Path.Combine(folder, "Screenshot_Error.txt"), ex.ToString(), Encoding.UTF8); }
            }

            return folder;
        }

        public static string BuildSystemInfo(string activeWorkspace)
        {
            var sb = new StringBuilder();
            sb.AppendLine("SOACS ForgeWorks Feedback Diagnostics");
            sb.AppendLine("Version: " + ApplicationVersion);
            sb.AppendLine("Active Workspace: " + NullSafe(activeWorkspace));
            sb.AppendLine("Repository Profile: " + NullSafe(InventoryStore.Workstation == null ? "" : InventoryStore.Workstation.RepositoryProfile));
            sb.AppendLine("Repository Root: " + NullSafe(RepositoryManager.RepositoryRoot));
            sb.AppendLine("Operator: " + NullSafe(InventoryStore.CurrentOperator));
            sb.AppendLine("Windows User: " + NullSafe(InventoryStore.CurrentDomainUser));
            sb.AppendLine("Computer: " + NullSafe(InventoryStore.CurrentMachineName));
            sb.AppendLine("Date/Time: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine("OS Version: " + Environment.OSVersion);
            sb.AppendLine("64-bit OS: " + Environment.Is64BitOperatingSystem);
            try
            {
                var s = Screen.PrimaryScreen;
                sb.AppendLine("Primary Screen: " + s.Bounds.Width + "x" + s.Bounds.Height);
                sb.AppendLine("Working Area: " + s.WorkingArea.Width + "x" + s.WorkingArea.Height);
            }
            catch { }
            try
            {
                using (var g = Graphics.FromHwnd(IntPtr.Zero))
                {
                    double scale = g.DpiX / 96.0 * 100.0;
                    sb.AppendLine("DPI X: " + g.DpiX.ToString("0"));
                    sb.AppendLine("DPI Y: " + g.DpiY.ToString("0"));
                    sb.AppendLine("Display Scaling Estimate: " + scale.ToString("0") + "%");
                }
            }
            catch { }
            sb.AppendLine("Database Version: " + InventoryStore.CurrentDatabaseVersion);
            sb.AppendLine("Read Only: " + InventoryStore.IsReadOnly);
            return sb.ToString();
        }

        public static string BuildRepositoryHealth()
        {
            var h = RepositoryManager.CheckHealth();
            var sb = new StringBuilder();
            sb.AppendLine("Repository Health");
            sb.AppendLine("Profile: " + NullSafe(h.ProfileName));
            sb.AppendLine("Root: " + NullSafe(h.RepositoryRoot));
            sb.AppendLine("Available: " + h.RepositoryAvailable);
            sb.AppendLine("Database Folder OK: " + h.DatabaseFolderOk);
            sb.AppendLine("Attachments Folder OK: " + h.AttachmentsFolderOk);
            sb.AppendLine("Photos Folder OK: " + h.PhotosFolderOk);
            sb.AppendLine("Reports Folder OK: " + h.ReportsFolderOk);
            sb.AppendLine("Logs Folder OK: " + h.LogsFolderOk);
            sb.AppendLine("Backups Folder OK: " + h.BackupsFolderOk);
            sb.AppendLine("Feedback Folder OK: " + h.FeedbackFolderOk);
            sb.AppendLine("Free Space: " + RepositoryManager.FormatBytes(h.FreeSpaceBytes));
            sb.AppendLine("Message: " + NullSafe(h.Message));
            return sb.ToString();
        }

        private static string BuildFeedbackXml(string category, string comments, string activeWorkspace)
        {
            return "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                   "<Feedback>\r\n" +
                   "  <Version>" + Xml(ApplicationVersion) + "</Version>\r\n" +
                   "  <Category>" + Xml(category) + "</Category>\r\n" +
                   "  <Workspace>" + Xml(activeWorkspace) + "</Workspace>\r\n" +
                   "  <RepositoryProfile>" + Xml(InventoryStore.Workstation == null ? "" : InventoryStore.Workstation.RepositoryProfile) + "</RepositoryProfile>\r\n" +
                   "  <RepositoryRoot>" + Xml(RepositoryManager.RepositoryRoot) + "</RepositoryRoot>\r\n" +
                   "  <Operator>" + Xml(InventoryStore.CurrentOperator) + "</Operator>\r\n" +
                   "  <WindowsUser>" + Xml(InventoryStore.CurrentDomainUser) + "</WindowsUser>\r\n" +
                   "  <Computer>" + Xml(InventoryStore.CurrentMachineName) + "</Computer>\r\n" +
                   "  <Submitted>" + Xml(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")) + "</Submitted>\r\n" +
                   "  <Comments>" + Xml(comments) + "</Comments>\r\n" +
                   "</Feedback>\r\n";
        }

        public static void CaptureWindow(Form owner, string filePath)
        {
            Rectangle bounds = owner.Bounds;
            using (var bmp = new Bitmap(bounds.Width, bounds.Height))
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
                bmp.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        public static string FeedbackRoot
        {
            get { Directory.CreateDirectory(RepositoryManager.FeedbackFolder); return RepositoryManager.FeedbackFolder; }
        }

        private static string MakeSafe(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "Feedback";
            foreach (char c in Path.GetInvalidFileNameChars()) value = value.Replace(c, '_');
            return value.Trim().Replace(" ", "_");
        }

        private static string Xml(string value)
        {
            if (value == null) return "";
            return value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;");
        }

        private static string NullSafe(string value) { return string.IsNullOrWhiteSpace(value) ? "--" : value; }
    }
}
