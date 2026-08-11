using System;
using System.IO;

namespace SOACSForgeWorks.Core
{
    public static class LogManager
    {
        public static void Write(string logName, string message)
        {
            try
            {
                RepositoryManager.EnsureRepository();
                string safe = string.IsNullOrWhiteSpace(logName) ? "forgeworks.log" : logName;
                foreach (char c in Path.GetInvalidFileNameChars()) safe = safe.Replace(c, '_');
                if (!safe.EndsWith(".log", StringComparison.OrdinalIgnoreCase)) safe += ".log";
                Directory.CreateDirectory(RepositoryManager.LogsFolder);
                File.AppendAllText(Path.Combine(RepositoryManager.LogsFolder, safe), DateTime.Now.ToString("s") + "  " + (message ?? string.Empty) + Environment.NewLine);
            }
            catch { }
        }
    }
}
