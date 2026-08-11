using System;
using System.IO;

namespace SOACSForgeWorks.Core
{
    public static class BackupManager
    {
        public static string BackupDatabase(string reason)
        {
            RepositoryManager.EnsureRepository();
            Directory.CreateDirectory(RepositoryManager.BackupsFolder);
            string source = RepositoryManager.DatabaseFile;
            if (!File.Exists(source)) return string.Empty;
            string safeReason = string.IsNullOrWhiteSpace(reason) ? "Manual" : reason.Trim();
            foreach (char c in Path.GetInvalidFileNameChars()) safeReason = safeReason.Replace(c, '_');
            string path = Path.Combine(RepositoryManager.BackupsFolder, "ForgeWorksInventory_" + safeReason + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xml.bak");
            File.Copy(source, path, true);
            return path;
        }
    }
}
