using System;
using System.Collections.Generic;
using System.IO;

namespace SOACSForgeWorks.Core
{
    [Serializable]
    public class RepositoryProfile
    {
        public string Name { get; set; }
        public string DataRoot { get; set; }
        public string Notes { get; set; }
        public string ColorName { get; set; }
        public bool ReadOnly { get; set; }
        public bool IsNetwork { get; set; }
        public RepositoryProfile()
        {
            Name = "Standalone";
            DataRoot = RepositoryManager.DefaultRepositoryRoot;
            Notes = "Local standalone repository.";
            ColorName = "Yellow";
        }
        public override string ToString() { return Name; }
    }

    [Serializable]
    public class RepositoryProfileCollection
    {
        public List<RepositoryProfile> Profiles { get; set; }
        public RepositoryProfileCollection() { Profiles = new List<RepositoryProfile>(); }
    }

    public class RepositoryHealth
    {
        public string ProfileName { get; set; }
        public string RepositoryRoot { get; set; }
        public string DatabaseFolder { get; set; }
        public string AttachmentsFolder { get; set; }
        public string PhotosFolder { get; set; }
        public string ReportsFolder { get; set; }
        public string LogsFolder { get; set; }
        public string BackupsFolder { get; set; }
        public bool RepositoryAvailable { get; set; }
        public bool DatabaseFolderOk { get; set; }
        public bool AttachmentsFolderOk { get; set; }
        public bool PhotosFolderOk { get; set; }
        public bool ReportsFolderOk { get; set; }
        public bool LogsFolderOk { get; set; }
        public bool BackupsFolderOk { get; set; }
        public long FreeSpaceBytes { get; set; }
        public string Message { get; set; }
    }

    public static class RepositoryManager
    {
        public const string DatabaseFileName = "ForgeWorksInventory.xml";

        public static string DefaultRepositoryRoot
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "SOACS", "ForgeWorks"); }
        }

        public static string DefaultOfflineRoot
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "SOACS", "ForgeWorks_Offline"); }
        }

        public static string RepositoryRoot
        {
            get { return ResolveRepositoryRoot(InventoryStore.Workstation); }
        }

        public static string DatabaseFolder { get { return Path.Combine(RepositoryRoot, "Database"); } }
        public static string DatabaseFile { get { return Path.Combine(DatabaseFolder, DatabaseFileName); } }
        public static string AttachmentsFolder { get { return Path.Combine(RepositoryRoot, "Attachments"); } }
        public static string ItemAttachmentsFolder { get { return Path.Combine(AttachmentsFolder, "Items"); } }
        public static string ProjectAttachmentsFolder { get { return Path.Combine(AttachmentsFolder, "Projects"); } }
        public static string KitAttachmentsFolder { get { return Path.Combine(AttachmentsFolder, "Kits"); } }
        public static string PhotosFolder { get { return Path.Combine(RepositoryRoot, "Photos"); } }
        public static string ReportsFolder { get { return Path.Combine(RepositoryRoot, "Reports"); } }
        public static string LogsFolder { get { return Path.Combine(RepositoryRoot, "Logs"); } }
        public static string BackupsFolder { get { return Path.Combine(RepositoryRoot, "Backups"); } }
        public static string ConfigFolder { get { return Path.Combine(RepositoryRoot, "Config"); } }
        public static string TempFolder { get { return Path.Combine(RepositoryRoot, "Temp"); } }

        public static string ResolveRepositoryRoot(WorkstationSettings settings)
        {
            try
            {
                if (settings != null)
                {
                    if (settings.UseRepositoryManager && !string.IsNullOrWhiteSpace(settings.RepositoryDataRoot))
                        return settings.RepositoryDataRoot.Trim();

                    // Backward compatibility: if a database file was chosen before v3.0, use that folder as a legacy repository root.
                    if (!string.IsNullOrWhiteSpace(settings.DatabasePath))
                    {
                        string dir = Path.GetDirectoryName(settings.DatabasePath.Trim());
                        if (!string.IsNullOrWhiteSpace(dir)) return dir;
                    }
                }
            }
            catch { }
            return DefaultRepositoryRoot;
        }

        public static void EnsureRepository()
        {
            EnsureRepository(RepositoryRoot);
        }

        public static void EnsureRepository(string root)
        {
            if (string.IsNullOrWhiteSpace(root)) root = DefaultRepositoryRoot;
            Directory.CreateDirectory(root);
            Directory.CreateDirectory(Path.Combine(root, "Database"));
            Directory.CreateDirectory(Path.Combine(root, "Attachments"));
            Directory.CreateDirectory(Path.Combine(root, "Attachments", "Items"));
            Directory.CreateDirectory(Path.Combine(root, "Attachments", "Projects"));
            Directory.CreateDirectory(Path.Combine(root, "Attachments", "Kits"));
            Directory.CreateDirectory(Path.Combine(root, "Photos"));
            Directory.CreateDirectory(Path.Combine(root, "Reports"));
            Directory.CreateDirectory(Path.Combine(root, "Logs"));
            Directory.CreateDirectory(Path.Combine(root, "Backups"));
            Directory.CreateDirectory(Path.Combine(root, "Config"));
            Directory.CreateDirectory(Path.Combine(root, "Temp"));
        }

        public static string GetItemAttachmentFolder(string forgeId)
        {
            string safe = MakeSafeName(string.IsNullOrWhiteSpace(forgeId) ? "Unassigned" : forgeId);
            string dir = Path.Combine(ItemAttachmentsFolder, safe);
            Directory.CreateDirectory(dir);
            return dir;
        }

        public static string GetPhotoFolder(string forgeId)
        {
            string safe = MakeSafeName(string.IsNullOrWhiteSpace(forgeId) ? "Unassigned" : forgeId);
            string dir = Path.Combine(PhotosFolder, safe);
            Directory.CreateDirectory(dir);
            return dir;
        }

        public static string GetReportFolder(DateTime when)
        {
            string dir = Path.Combine(ReportsFolder, when.Year.ToString("0000"), when.ToString("MMMM"));
            Directory.CreateDirectory(dir);
            return dir;
        }

        public static string MakeRelativePath(string fullPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fullPath)) return "";
                Uri rootUri = new Uri(RepositoryRoot.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar);
                Uri fileUri = new Uri(fullPath);
                return Uri.UnescapeDataString(rootUri.MakeRelativeUri(fileUri).ToString()).Replace('/', Path.DirectorySeparatorChar);
            }
            catch { return fullPath; }
        }

        public static string ResolvePath(string relativeOrFullPath)
        {
            if (string.IsNullOrWhiteSpace(relativeOrFullPath)) return "";
            try
            {
                if (Path.IsPathRooted(relativeOrFullPath)) return relativeOrFullPath;
                return Path.Combine(RepositoryRoot, relativeOrFullPath.Replace('/', Path.DirectorySeparatorChar));
            }
            catch { return relativeOrFullPath; }
        }

        public static string ProfilesFile
        {
            get
            {
                string appConfig = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SOACS", "ForgeWorks");
                Directory.CreateDirectory(appConfig);
                return Path.Combine(appConfig, "ForgeWorks_RepositoryProfiles.xml");
            }
        }

        public static List<RepositoryProfile> LoadProfiles()
        {
            try
            {
                EnsureDefaultProfilesFile();
                using (var fs = File.OpenRead(ProfilesFile))
                {
                    var list = (RepositoryProfileCollection)new System.Xml.Serialization.XmlSerializer(typeof(RepositoryProfileCollection)).Deserialize(fs);
                    if (list != null && list.Profiles != null && list.Profiles.Count > 0) return list.Profiles;
                }
            }
            catch { }
            return CreateDefaultProfiles();
        }

        public static void SaveProfiles(List<RepositoryProfile> profiles)
        {
            if (profiles == null || profiles.Count == 0) profiles = CreateDefaultProfiles();
            Directory.CreateDirectory(Path.GetDirectoryName(ProfilesFile));
            using (var fs = File.Create(ProfilesFile))
                new System.Xml.Serialization.XmlSerializer(typeof(RepositoryProfileCollection)).Serialize(fs, new RepositoryProfileCollection { Profiles = profiles });
        }

        public static void EnsureDefaultProfilesFile()
        {
            if (!File.Exists(ProfilesFile)) SaveProfiles(CreateDefaultProfiles());
        }

        public static List<RepositoryProfile> CreateDefaultProfiles()
        {
            return new List<RepositoryProfile>
            {
                new RepositoryProfile { Name = "Production", DataRoot = DefaultRepositoryRoot, Notes = "Primary ForgeWorks repository. Update this to the shared network repository when available.", ColorName = "Green", IsNetwork = true },
                new RepositoryProfile { Name = "Test Lab", DataRoot = Path.Combine(DefaultRepositoryRoot, "TestLab"), Notes = "Isolated test repository for new builds and training.", ColorName = "Blue" },
                new RepositoryProfile { Name = "Standalone", DataRoot = DefaultRepositoryRoot, Notes = "Local standalone repository.", ColorName = "Yellow" },
                new RepositoryProfile { Name = "Offline", DataRoot = DefaultOfflineRoot, Notes = "Local offline repository. Future synchronization will use this profile.", ColorName = "Orange" }
            };
        }

        public static RepositoryProfile FindProfile(string name)
        {
            var profiles = LoadProfiles();
            foreach (var p in profiles)
                if (string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)) return p;
            return profiles.Count > 0 ? profiles[0] : new RepositoryProfile();
        }

        public static RepositoryProfile CurrentProfile
        {
            get
            {
                string name = InventoryStore.Workstation == null ? "Standalone" : InventoryStore.Workstation.RepositoryProfile;
                return FindProfile(string.IsNullOrWhiteSpace(name) ? "Standalone" : name);
            }
        }

        public static void SwitchProfile(string profileName)
        {
            var profile = FindProfile(profileName);
            EnsureRepository(profile.DataRoot);
            InventoryStore.SetRepositorySettings(InventoryStore.Workstation.OperatorName, InventoryStore.Workstation.WorkstationMode, profile.DataRoot, profile.IsNetwork, InventoryStore.Workstation.AutoRefreshSeconds, profile.Name);
        }

        public static RepositoryHealth CheckHealth()
        {
            var health = new RepositoryHealth();
            health.ProfileName = InventoryStore.Workstation == null ? "Standalone" : InventoryStore.Workstation.RepositoryProfile;
            health.RepositoryRoot = RepositoryRoot;
            health.DatabaseFolder = DatabaseFolder;
            health.AttachmentsFolder = AttachmentsFolder;
            health.PhotosFolder = PhotosFolder;
            health.ReportsFolder = ReportsFolder;
            health.LogsFolder = LogsFolder;
            health.BackupsFolder = BackupsFolder;
            try
            {
                EnsureRepository();
                health.RepositoryAvailable = Directory.Exists(RepositoryRoot);
                health.DatabaseFolderOk = Directory.Exists(DatabaseFolder);
                health.AttachmentsFolderOk = Directory.Exists(AttachmentsFolder);
                health.PhotosFolderOk = Directory.Exists(PhotosFolder);
                health.ReportsFolderOk = Directory.Exists(ReportsFolder);
                health.LogsFolderOk = Directory.Exists(LogsFolder);
                health.BackupsFolderOk = Directory.Exists(BackupsFolder);
                health.FreeSpaceBytes = GetFreeSpace(RepositoryRoot);
                health.Message = "Repository healthy. Active profile: " + health.ProfileName + ".";
            }
            catch (Exception ex)
            {
                health.Message = ex.Message;
            }
            return health;
        }

        public static string FormatBytes(long bytes)
        {
            if (bytes < 0) return "Unknown";
            string[] units = new[] { "B", "KB", "MB", "GB", "TB" };
            double value = bytes;
            int unit = 0;
            while (value >= 1024 && unit < units.Length - 1) { value /= 1024; unit++; }
            return value.ToString(unit == 0 ? "0" : "0.0") + " " + units[unit];
        }

        private static long GetFreeSpace(string root)
        {
            try
            {
                string path = Path.GetPathRoot(Path.GetFullPath(root));
                if (string.IsNullOrWhiteSpace(path)) return -1;
                var drive = new DriveInfo(path);
                return drive.AvailableFreeSpace;
            }
            catch { return -1; }
        }

        private static string MakeSafeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "Unassigned";
            foreach (char c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
            return name.Trim();
        }
    }
}
