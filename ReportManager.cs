using System;
using System.IO;
using System.Text;

namespace SOACSForgeWorks.Core
{
    public static class ReportManager
    {
        public static string SaveReport(string suggestedFileName, string contents)
        {
            RepositoryManager.EnsureRepository();
            string folder = RepositoryManager.GetReportFolder(DateTime.Now);
            string safe = MakeSafeFileName(string.IsNullOrWhiteSpace(suggestedFileName) ? "ForgeWorksReport.txt" : suggestedFileName);
            string path = Path.Combine(folder, safe);
            File.WriteAllText(path, contents ?? string.Empty, Encoding.UTF8);
            return path;
        }

        public static string GetReportFolder(DateTime when)
        {
            return RepositoryManager.GetReportFolder(when);
        }

        private static string MakeSafeFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
            return name.Trim();
        }
    }
}
