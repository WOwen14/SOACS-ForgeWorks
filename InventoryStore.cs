using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using SOACSForgeWorks.Core;

namespace SOACSForgeWorks
{
    [Serializable]
    public class InventoryItem
    {
        public string ForgeId { get; set; }
        public string Barcode { get; set; }
        public string ItemName { get; set; }
        public string CageCode { get; set; }
        public string Mrl { get; set; }
        public string DocumentPath { get; set; }
        public bool BorrowedItem { get; set; }
        public int BorrowedQuantity { get; set; }
        public string BorrowedBy { get; set; }
        public string BorrowedFromProject { get; set; }
        public bool ReorderRequired { get; set; }
        public string PartNumber { get; set; }
        public string Nsn { get; set; }
        public string SerialNumber { get; set; }
        public string Category { get; set; }
        public string Location { get; set; }
        public int Quantity { get; set; }
        public int Minimum { get; set; }
        public int Maximum { get; set; }
        public int ReservedQuantity { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal Cost { get; set; }
        public decimal StandardCost { get; set; }
        public decimal LastPurchasePrice { get; set; }
        public string Vendor { get; set; }
        public string PreferredVendor { get; set; }
        public string VendorPartNumber { get; set; }
        public string Manufacturer { get; set; }
        public string ManufacturerPartNumber { get; set; }
        public int LeadTimeDays { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime LastPurchaseDate { get; set; }
        public string LastVendor { get; set; }
        public int ReorderQuantity { get; set; }
        public DateTime WarrantyExpiration { get; set; }
        public string AttachmentFolder { get; set; }
        [XmlIgnore]
        public int AvailableQuantity { get { return Quantity - ReservedQuantity; } }
        public string Status { get; set; }
        public string Project { get; set; }
        public string Notes { get; set; }
        public string PhotoPath { get; set; }
        public List<ItemSerialRecord> Serials { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime LastScanned { get; set; }
        public List<CustomValue> CustomValues { get; set; }
        public InventoryItem()
        {
            CustomValues = new List<CustomValue>();
            Serials = new List<ItemSerialRecord>();
            UnitOfMeasure = "Each";
            Status = "Available";
            DateAdded = DateTime.Now;
            LastUpdated = DateTime.Now;
        }

        public override string ToString()
        {
            var name = string.IsNullOrWhiteSpace(ItemName) ? "Unnamed Item" : ItemName;
            var part = string.IsNullOrWhiteSpace(PartNumber) ? "No Part #" : PartNumber;
            var nsn = string.IsNullOrWhiteSpace(Nsn) ? "No NSN" : Nsn;
            return string.Format("{0} | {1} | Part: {2} | NSN: {3} | Qty: {4} | Location: {5}",
                ForgeId, name, part, nsn, Quantity, Location);
        }
    }

    [Serializable]
    public class ItemSerialRecord
    {
        public string SerialNumber { get; set; }
        public string Status { get; set; }
        public string Location { get; set; }
        public string Project { get; set; }
        public string Notes { get; set; }
    }

    [Serializable]
    public class ReservationRecord
    {
        public DateTime Time { get; set; }
        public string ForgeId { get; set; }
        public string Project { get; set; }
        public int Quantity { get; set; }
        public string Notes { get; set; }
    }

    [Serializable]
    public class AttachmentRecord
    {
        public string AttachmentId { get; set; }
        public string ForgeId { get; set; }
        public string FilePath { get; set; }
        public string RelativePath { get; set; }
        public string FileName { get; set; }
        public string OriginalFileName { get; set; }
        public string FileType { get; set; }
        public string DocumentCategory { get; set; }
        public string Description { get; set; }
        public string AddedBy { get; set; }
        public long FileSizeBytes { get; set; }
        public DateTime Added { get; set; }

        public AttachmentRecord()
        {
            AttachmentId = Guid.NewGuid().ToString("N");
            Added = DateTime.Now;
            DocumentCategory = "Other";
        }
    }

    [Serializable]
    public class BomRecord
    {
        public string Project { get; set; }
        public string KitName { get; set; }
        public string ForgeId { get; set; }
        public string ItemName { get; set; }
        public int RequiredQuantity { get; set; }
        public int ReservedQuantity { get; set; }
        public string Notes { get; set; }
    }

    [Serializable]
    public class WorkstationSettings
    {
        public string OperatorName { get; set; }
        public string WindowsUser { get; set; }
        public string DomainUser { get; set; }
        public string MachineName { get; set; }
        public string WorkstationMode { get; set; }
        public string DatabasePath { get; set; }
        public bool UseSharedDatabase { get; set; }
        public int AutoRefreshSeconds { get; set; }
        public bool UseRepositoryManager { get; set; }
        public string RepositoryProfile { get; set; }
        public string RepositoryDataRoot { get; set; }
        public string OfflineRoot { get; set; }
        public string InventoryHiddenColumns { get; set; }
        public WorkstationSettings()
        {
            OperatorName = Environment.UserName;
            WindowsUser = Environment.UserName;
            DomainUser = Environment.UserDomainName + "\\" + Environment.UserName;
            MachineName = Environment.MachineName;
            WorkstationMode = "Operator";
            DatabasePath = "";
            UseSharedDatabase = false;
            AutoRefreshSeconds = 30;
            UseRepositoryManager = true;
            RepositoryProfile = "Standalone";
            RepositoryDataRoot = RepositoryManager.DefaultRepositoryRoot;
            OfflineRoot = RepositoryManager.DefaultOfflineRoot;
            InventoryHiddenColumns = "";
        }
    }

    [Serializable]
    public class CustomField { public string Name { get; set; } public string Type { get; set; } }
    [Serializable]
    public class CustomValue { public string Name { get; set; } public string Value { get; set; } }

    [Serializable]
    public class CategoryRecord { public string Name { get; set; } public string Notes { get; set; } }
    [Serializable]
    public class LocationRecord { public string Name { get; set; } public string Notes { get; set; } }

    [Serializable]
    public class ProjectRecord
    {
        public string Name { get; set; }
        public string ProjectCode { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public string Owner { get; set; }
        public DateTime DueDate { get; set; }
        public string Notes { get; set; }

        public ProjectRecord()
        {
            Status = "Active";
            Priority = "Normal";
        }
    }

    [Serializable]
    public class TransactionRecord
    {
        public DateTime Time { get; set; }
        public string ForgeId { get; set; }
        public string Barcode { get; set; }
        public string ItemName { get; set; }
        public string Action { get; set; }
        public int Delta { get; set; }
        public int OldQuantity { get; set; }
        public int NewQuantity { get; set; }
        public string FromLocation { get; set; }
        public string ToLocation { get; set; }
        public string Operator { get; set; }
        public string WindowsUser { get; set; }
        public string Workstation { get; set; }
        public string Project { get; set; }
        public string Reason { get; set; }
        public string Notes { get; set; }
    }

    [Serializable]
    public class AuditRecord
    {
        public DateTime Time { get; set; }
        public string Operator { get; set; }
        public string WindowsUser { get; set; }
        public string Workstation { get; set; }
        public string Action { get; set; }
        public string ForgeId { get; set; }
        public string ItemName { get; set; }
        public string Notes { get; set; }
    }

    [Serializable]
    public class ForgeDatabase
    {
        public List<InventoryItem> Items { get; set; }
        public List<CustomField> CustomFields { get; set; }
        public List<TransactionRecord> Transactions { get; set; }
        public List<CategoryRecord> Categories { get; set; }
        public List<LocationRecord> Locations { get; set; }
        public List<ProjectRecord> Projects { get; set; }
        public List<ReservationRecord> Reservations { get; set; }
        public List<AttachmentRecord> Attachments { get; set; }
        public List<BomRecord> BomItems { get; set; }
        public List<AuditRecord> AuditLog { get; set; }
        public int DatabaseVersion { get; set; }
        public ForgeDatabase()
        {
            Items = new List<InventoryItem>();
            CustomFields = new List<CustomField>();
            Transactions = new List<TransactionRecord>();
            Categories = new List<CategoryRecord>();
            Locations = new List<LocationRecord>();
            Projects = new List<ProjectRecord>();
            Reservations = new List<ReservationRecord>();
            Attachments = new List<AttachmentRecord>();
            BomItems = new List<BomRecord>();
            AuditLog = new List<AuditRecord>();
            DatabaseVersion = 1;
        }
    }

    public static class InventoryStore
    {
    public static void WriteStartupLog(string message)
        {
            LogManager.Write("startup", message);
        }


        public static ForgeDatabase Database { get; private set; }
        public static WorkstationSettings Workstation { get; private set; }
        public static string DefaultDataFolder { get { return RepositoryManager.DefaultRepositoryRoot; } }
        public static string SettingsFolder { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SOACS", "ForgeWorks"); } }
        public static string SettingsFile { get { return Path.Combine(SettingsFolder, "ForgeWorks_Workstation.config.xml"); } }
        public static string RepositoryRoot { get { return RepositoryManager.RepositoryRoot; } }
        public static string DataFolder { get { return RepositoryManager.RepositoryRoot; } }
        public static string DataFile
        {
            get
            {
                if (Workstation != null && !Workstation.UseRepositoryManager && !string.IsNullOrWhiteSpace(Workstation.DatabasePath)) return Workstation.DatabasePath;
                return RepositoryManager.DatabaseFile;
            }
        }
        public static string BackupFile { get { return Path.Combine(RepositoryManager.BackupsFolder, "ForgeWorksInventory.xml.bak"); } }
        public static string AttachmentsFolder { get { return RepositoryManager.AttachmentsFolder; } }
        public static string ItemAttachmentsFolder { get { return RepositoryManager.ItemAttachmentsFolder; } }
        public static bool IsReadOnly { get { return Workstation != null && string.Equals(Workstation.WorkstationMode, "Read-Only Viewer", StringComparison.OrdinalIgnoreCase); } }
        public const int CurrentDatabaseVersion = 6;

        static InventoryStore() { LoadWorkstationSettings(); Load(); }

        public static void LoadWorkstationSettings()
        {
            try
            {
                Directory.CreateDirectory(SettingsFolder);
                if (File.Exists(SettingsFile))
                {
                    using (var fs = File.OpenRead(SettingsFile)) Workstation = (WorkstationSettings)new XmlSerializer(typeof(WorkstationSettings)).Deserialize(fs);
                }
                if (Workstation == null) Workstation = new WorkstationSettings();
                Workstation.WindowsUser = Environment.UserName;
                Workstation.DomainUser = Environment.UserDomainName + "\\" + Environment.UserName;
                Workstation.MachineName = Environment.MachineName;
                if (string.IsNullOrWhiteSpace(Workstation.OperatorName)) Workstation.OperatorName = Workstation.DomainUser;
                if (string.IsNullOrWhiteSpace(Workstation.WorkstationMode)) Workstation.WorkstationMode = "Operator";
                if (string.IsNullOrWhiteSpace(Workstation.RepositoryProfile)) Workstation.RepositoryProfile = "Standalone";
                if (string.IsNullOrWhiteSpace(Workstation.RepositoryDataRoot)) Workstation.RepositoryDataRoot = RepositoryManager.DefaultRepositoryRoot;
                if (string.IsNullOrWhiteSpace(Workstation.OfflineRoot)) Workstation.OfflineRoot = RepositoryManager.DefaultOfflineRoot;
                if (string.IsNullOrWhiteSpace(Workstation.InventoryHiddenColumns)) Workstation.InventoryHiddenColumns = "";
                if (Workstation.AutoRefreshSeconds < 5) Workstation.AutoRefreshSeconds = 30;
                RepositoryManager.EnsureRepository();
            }
            catch { Workstation = new WorkstationSettings(); }
        }

        public static void SaveWorkstationSettings()
        {
            Directory.CreateDirectory(SettingsFolder);
            using (var fs = File.Create(SettingsFile)) new XmlSerializer(typeof(WorkstationSettings)).Serialize(fs, Workstation ?? new WorkstationSettings());
        }

        public static void SetWorkstationSettings(string operatorName, string mode, string databasePath, bool shared, int refreshSeconds)
        {
            Workstation = Workstation ?? new WorkstationSettings();
            Workstation.OperatorName = string.IsNullOrWhiteSpace(operatorName) ? Environment.UserName : operatorName.Trim();
            Workstation.WorkstationMode = string.IsNullOrWhiteSpace(mode) ? "Operator" : mode.Trim();
            Workstation.DatabasePath = string.IsNullOrWhiteSpace(databasePath) ? "" : databasePath.Trim();
            Workstation.UseSharedDatabase = shared;
            Workstation.AutoRefreshSeconds = refreshSeconds < 5 ? 30 : refreshSeconds;
            SaveWorkstationSettings();
            Load();
        }
        public static void SetRepositorySettings(string operatorName, string mode, string repositoryRoot, bool shared, int refreshSeconds, string profile)
        {
            Workstation = Workstation ?? new WorkstationSettings();
            Workstation.OperatorName = string.IsNullOrWhiteSpace(operatorName) ? Environment.UserName : operatorName.Trim();
            Workstation.WorkstationMode = string.IsNullOrWhiteSpace(mode) ? "Operator" : mode.Trim();
            Workstation.RepositoryDataRoot = string.IsNullOrWhiteSpace(repositoryRoot) ? RepositoryManager.DefaultRepositoryRoot : repositoryRoot.Trim();
            Workstation.RepositoryProfile = string.IsNullOrWhiteSpace(profile) ? "Standalone" : profile.Trim();
            Workstation.UseRepositoryManager = true;
            Workstation.UseSharedDatabase = shared;
            Workstation.AutoRefreshSeconds = refreshSeconds < 5 ? 30 : refreshSeconds;
            Workstation.DatabasePath = "";
            RepositoryManager.EnsureRepository();
            SaveWorkstationSettings();
            Load();
        }


        public static void Load()
        {
            RepositoryManager.EnsureRepository();
            Directory.CreateDirectory(Path.GetDirectoryName(DataFile));
            if (File.Exists(DataFile))
            {
                using (var fs = File.OpenRead(DataFile)) Database = (ForgeDatabase)new XmlSerializer(typeof(ForgeDatabase)).Deserialize(fs);
                NormalizeDatabase();
                MigrateDatabaseIfNeeded();
            }
            else { Database = new ForgeDatabase(); Seed(); Save(); }
        }

        private static void NormalizeDatabase()
        {
            if (Database.Items == null) Database.Items = new List<InventoryItem>();
            if (Database.CustomFields == null) Database.CustomFields = new List<CustomField>();
            if (Database.Transactions == null) Database.Transactions = new List<TransactionRecord>();
            if (Database.Categories == null) Database.Categories = new List<CategoryRecord>();
            if (Database.Locations == null) Database.Locations = new List<LocationRecord>();
            if (Database.Projects == null) Database.Projects = new List<ProjectRecord>();
            if (Database.Reservations == null) Database.Reservations = new List<ReservationRecord>();
            if (Database.Attachments == null) Database.Attachments = new List<AttachmentRecord>();
            if (Database.BomItems == null) Database.BomItems = new List<BomRecord>();
            if (Database.AuditLog == null) Database.AuditLog = new List<AuditRecord>();
            if (Database.DatabaseVersion <= 0) Database.DatabaseVersion = 1;
            foreach (var item in Database.Items)
            {
                if (item.CustomValues == null) item.CustomValues = new List<CustomValue>();
                if (item.Serials == null) item.Serials = new List<ItemSerialRecord>();
                if (string.IsNullOrWhiteSpace(item.UnitOfMeasure)) item.UnitOfMeasure = "Each";
                if (item.ReservedQuantity < 0) item.ReservedQuantity = 0;
                if (item.ReservedQuantity > item.Quantity) item.ReservedQuantity = item.Quantity;
                if (item.DateAdded == DateTime.MinValue) item.DateAdded = DateTime.Now;
                if (item.LastUpdated == DateTime.MinValue) item.LastUpdated = item.DateAdded;
                if (string.IsNullOrWhiteSpace(item.Status)) item.Status = "Available";
                if (item.BorrowedQuantity < 0) item.BorrowedQuantity = 0;
                if (item.BorrowedQuantity > item.Quantity) item.BorrowedQuantity = item.Quantity;
                if (item.BorrowedItem && string.IsNullOrWhiteSpace(item.BorrowedFromProject)) item.BorrowedFromProject = item.Project;
                if (string.IsNullOrWhiteSpace(item.PreferredVendor)) item.PreferredVendor = item.Vendor;
                if (string.IsNullOrWhiteSpace(item.ManufacturerPartNumber)) item.ManufacturerPartNumber = item.PartNumber;
                if (item.LastPurchaseDate == DateTime.MinValue && item.PurchaseDate != DateTime.MinValue) item.LastPurchaseDate = item.PurchaseDate;
                if (item.LastPurchasePrice <= 0 && item.Cost > 0) item.LastPurchasePrice = item.Cost;
                if (item.StandardCost <= 0 && item.Cost > 0) item.StandardCost = item.Cost;
                if (item.ReorderQuantity < 0) item.ReorderQuantity = 0;
                if (item.LeadTimeDays < 0) item.LeadTimeDays = 0;
            }
            foreach (var a in Database.Attachments)
            {
                if (string.IsNullOrWhiteSpace(a.AttachmentId)) a.AttachmentId = Guid.NewGuid().ToString("N");
                if (string.IsNullOrWhiteSpace(a.FileName)) a.FileName = Path.GetFileName(string.IsNullOrWhiteSpace(a.FilePath) ? a.RelativePath : a.FilePath);
                if (string.IsNullOrWhiteSpace(a.OriginalFileName)) a.OriginalFileName = a.FileName;
                if (string.IsNullOrWhiteSpace(a.FileType)) a.FileType = Path.GetExtension(a.FileName).TrimStart('.').ToUpperInvariant();
                if (string.IsNullOrWhiteSpace(a.DocumentCategory)) a.DocumentCategory = GuessDocumentCategory(a.FileName);
                if (string.IsNullOrWhiteSpace(a.AddedBy)) a.AddedBy = CurrentOperator;
                if (a.Added == DateTime.MinValue) a.Added = DateTime.Now;
            }
            EnsureDefaultLists();
        }

        private static void MigrateDatabaseIfNeeded()
        {
            if (Database == null) return;
            if (Database.DatabaseVersion >= CurrentDatabaseVersion) return;
            RepositoryManager.EnsureRepository();
            if (File.Exists(DataFile))
            {
                string backupName = "ForgeWorksInventory_PreUpgrade_v" + Database.DatabaseVersion + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xml.bak";
                File.Copy(DataFile, Path.Combine(RepositoryManager.BackupsFolder, backupName), true);
            }
            if (Database.DatabaseVersion < 2)
            {
                foreach (var item in Database.Items)
                {
                    if (item.Serials == null) item.Serials = new List<ItemSerialRecord>();
                    if (string.IsNullOrWhiteSpace(item.UnitOfMeasure)) item.UnitOfMeasure = "Each";
                    if (item.ReservedQuantity < 0) item.ReservedQuantity = 0;
                    if (item.ReservedQuantity > item.Quantity) item.ReservedQuantity = item.Quantity;
                }
                if (Database.Reservations == null) Database.Reservations = new List<ReservationRecord>();
                if (Database.Attachments == null) Database.Attachments = new List<AttachmentRecord>();
            }
            if (Database.DatabaseVersion < 3)
            {
                if (Database.BomItems == null) Database.BomItems = new List<BomRecord>();
            }
            if (Database.DatabaseVersion < 4)
            {
                foreach (var item in Database.Items)
                {
                    if (item.BorrowedQuantity < 0) item.BorrowedQuantity = 0;
                    if (item.BorrowedQuantity > item.Quantity) item.BorrowedQuantity = item.Quantity;
                    if (item.BorrowedItem && string.IsNullOrWhiteSpace(item.BorrowedFromProject)) item.BorrowedFromProject = item.Project;
                }
            }
            if (Database.DatabaseVersion < 5)
            {
                if (Database.Attachments == null) Database.Attachments = new List<AttachmentRecord>();
                foreach (var item in Database.Items)
                {
                    if (!string.IsNullOrWhiteSpace(item.DocumentPath) && File.Exists(item.DocumentPath))
                    {
                        bool exists = false;
                        foreach (var a in Database.Attachments)
                            if (EqualsIgnore(a.ForgeId, item.ForgeId) && EqualsIgnore(a.FilePath, item.DocumentPath)) exists = true;
                        if (!exists)
                        {
                            Database.Attachments.Add(new AttachmentRecord
                            {
                                ForgeId = item.ForgeId,
                                FilePath = item.DocumentPath,
                                RelativePath = "",
                                FileName = Path.GetFileName(item.DocumentPath),
                                OriginalFileName = Path.GetFileName(item.DocumentPath),
                                FileType = Path.GetExtension(item.DocumentPath).TrimStart('.').ToUpperInvariant(),
                                DocumentCategory = GuessDocumentCategory(item.DocumentPath),
                                AddedBy = CurrentOperator,
                                Added = DateTime.Now,
                                Description = "Migrated legacy attached document"
                            });
                        }
                    }
                }
            }
            if (Database.DatabaseVersion < 6)
            {
                foreach (var item in Database.Items)
                {
                    if (string.IsNullOrWhiteSpace(item.PreferredVendor)) item.PreferredVendor = item.Vendor;
                    if (string.IsNullOrWhiteSpace(item.ManufacturerPartNumber)) item.ManufacturerPartNumber = item.PartNumber;
                    if (item.LastPurchaseDate == DateTime.MinValue && item.PurchaseDate != DateTime.MinValue) item.LastPurchaseDate = item.PurchaseDate;
                    if (item.LastPurchasePrice <= 0 && item.Cost > 0) item.LastPurchasePrice = item.Cost;
                    if (item.StandardCost <= 0 && item.Cost > 0) item.StandardCost = item.Cost;
                    if (item.ReorderQuantity < 0) item.ReorderQuantity = 0;
                    if (item.LeadTimeDays < 0) item.LeadTimeDays = 0;
                }
            }
            Database.DatabaseVersion = CurrentDatabaseVersion;
            Save();
        }

        public static void Save()
        {
            if (IsReadOnly) return;
            SaveWithRetry();
            LiveDataBus.NotifyDataChanged();
        }

        private static void SaveWithRetry()
        {
            RepositoryManager.EnsureRepository();
            Directory.CreateDirectory(Path.GetDirectoryName(DataFile));
            Exception last = null;
            for (int attempt = 0; attempt < 5; attempt++)
            {
                try
                {
                    if (File.Exists(DataFile)) File.Copy(DataFile, BackupFile, true);
                    string temp = DataFile + ".tmp";
                    using (var fs = File.Create(temp)) new XmlSerializer(typeof(ForgeDatabase)).Serialize(fs, Database);
                    if (File.Exists(DataFile)) File.Copy(temp, DataFile, true); else File.Move(temp, DataFile);
                    try { if (File.Exists(temp)) File.Delete(temp); } catch { }
                    return;
                }
                catch (Exception ex)
                {
                    last = ex;
                    System.Threading.Thread.Sleep(150 * (attempt + 1));
                }
            }
            throw new IOException("ForgeWorks database is locked or unavailable. Try again in a few seconds. Last error: " + (last == null ? "Unknown" : last.Message), last);
        }

        private static void DemandWriteAccess()
        {
            if (IsReadOnly) throw new InvalidOperationException("This workstation is in Read-Only Viewer mode. Changes are disabled.");
        }


        public static List<InventoryItem> LoadItems()
        {
            if (Database == null) Load();
            NormalizeDatabase();
            return new List<InventoryItem>(Database.Items);
        }

        public static List<ProjectRecord> LoadProjects()
        {
            if (Database == null) Load();
            NormalizeDatabase();
            return new List<ProjectRecord>(Database.Projects);
        }


        public static string CurrentDomainUser
        {
            get
            {
                try { return Environment.UserDomainName + "\\" + Environment.UserName; }
                catch { return Environment.UserName; }
            }
        }

        public static string CurrentMachineName
        {
            get
            {
                try { return Environment.MachineName; }
                catch { return "UNKNOWN"; }
            }
        }

        public static string CurrentOperator
        {
            get
            {
                if (Workstation != null && !string.IsNullOrWhiteSpace(Workstation.OperatorName)) return Workstation.OperatorName;
                return CurrentDomainUser;
            }
        }

        public static void AddAudit(string action, InventoryItem item, string notes)
        {
            if (Database == null) return;
            if (Database.AuditLog == null) Database.AuditLog = new List<AuditRecord>();
            Database.AuditLog.Insert(0, new AuditRecord
            {
                Time = DateTime.Now,
                Operator = CurrentOperator,
                WindowsUser = CurrentDomainUser,
                Workstation = CurrentMachineName,
                Action = action,
                ForgeId = item == null ? "" : item.ForgeId,
                ItemName = item == null ? "" : item.ItemName,
                Notes = notes
            });
            if (Database.AuditLog.Count > 5000) Database.AuditLog.RemoveRange(5000, Database.AuditLog.Count - 5000);
        }

        public static string NextForgeId()
        {
            int max = 0;
            foreach (var i in Database.Items)
            {
                if (!string.IsNullOrEmpty(i.ForgeId) && i.ForgeId.StartsWith("FG-"))
                { int n; if (int.TryParse(i.ForgeId.Substring(3), out n) && n > max) max = n; }
            }
            return "FG-" + (max + 1).ToString("000000");
        }

        public static InventoryItem FindByScan(string scan)
        {
            if (scan == null) return null; scan = scan.Trim();
            foreach (var i in Database.Items)
                if (EqualsIgnore(i.Barcode, scan) || EqualsIgnore(i.ForgeId, scan) || EqualsIgnore(i.PartNumber, scan) || EqualsIgnore(i.Nsn, scan) || EqualsIgnore(i.SerialNumber, scan))
                { i.LastScanned = DateTime.Now; if (!IsReadOnly) Save(); return i; }
            return null;
        }

        public static void AddOrUpdateItem(InventoryItem item, bool isNew)
        {
            DemandWriteAccess();
            if (item == null) return;
            if (string.IsNullOrWhiteSpace(item.ForgeId)) item.ForgeId = NextForgeId();
            if (isNew && !Database.Items.Contains(item)) { item.DateAdded = DateTime.Now; Database.Items.Add(item); }
            item.LastUpdated = DateTime.Now;
            if (string.IsNullOrWhiteSpace(item.Status)) item.Status = GetStatus(item);
            AddCategory(item.Category);
            AddLocation(item.Location);
            AddProject(item.Project);
            AddAudit(isNew ? "ITEM ADD" : "ITEM EDIT", item, isNew ? "Item record created" : "Item record updated");
            Save();
        }

        public static void DeleteItem(InventoryItem item)
        {
            DemandWriteAccess();
            if (item == null) return;
            Database.Items.Remove(item);
            AddTransaction(item, "DELETE", 0, item.Quantity, 0, item.Location, item.Location, "Item deleted from inventory");
        }

        public static void AddTransaction(InventoryItem item, string action, int delta, string notes)
        {
            int oldQty = item == null ? 0 : item.Quantity - delta;
            int newQty = item == null ? 0 : item.Quantity;
            AddTransaction(item, action, delta, oldQty, newQty, item == null ? "" : item.Location, item == null ? "" : item.Location, notes);
        }

        public static void AddTransaction(InventoryItem item, string action, int delta, int oldQty, int newQty, string fromLocation, string toLocation, string notes)
        {
            DemandWriteAccess();
            if (item == null) return;
            Database.Transactions.Insert(0, new TransactionRecord
            {
                Time = DateTime.Now,
                ForgeId = item.ForgeId,
                Barcode = item.Barcode,
                ItemName = item.ItemName,
                Action = action,
                Delta = delta,
                OldQuantity = oldQty,
                NewQuantity = newQty,
                FromLocation = fromLocation,
                ToLocation = toLocation,
                Operator = CurrentOperator,
                WindowsUser = CurrentDomainUser,
                Workstation = CurrentMachineName,
                Project = item.Project,
                Reason = action,
                Notes = notes
            });
            item.LastUpdated = DateTime.Now;
            item.Status = GetStatus(item);
            AddAudit(action, item, notes);
            Save();
        }

        public static void AdjustQuantity(InventoryItem item, int delta, string action, string notes)
        {
            DemandWriteAccess();
            if (item == null) return;
            int old = item.Quantity;
            item.Quantity += delta;
            if (item.Quantity < 0) item.Quantity = 0;
            int actualDelta = item.Quantity - old;
            AddTransaction(item, action, actualDelta, old, item.Quantity, item.Location, item.Location, notes);
        }

        public static void MoveItem(InventoryItem item, string newLocation, string notes)
        {
            DemandWriteAccess();
            if (item == null || string.IsNullOrWhiteSpace(newLocation)) return;
            string oldLoc = item.Location;
            item.Location = newLocation.Trim();
            AddLocation(item.Location);
            AddTransaction(item, "MOVE", 0, item.Quantity, item.Quantity, oldLoc, item.Location, notes);
        }

        public static void SetReservedQuantity(InventoryItem item, int reserved, string project, string notes)
        {
            DemandWriteAccess();
            if (item == null) return;
            if (reserved < 0) reserved = 0;
            if (reserved > item.Quantity) reserved = item.Quantity;
            item.ReservedQuantity = reserved;
            Database.Reservations.Insert(0, new ReservationRecord { Time = DateTime.Now, ForgeId = item.ForgeId, Project = project ?? item.Project, Quantity = reserved, Notes = notes });
            AddTransaction(item, "RESERVE", 0, item.Quantity, item.Quantity, item.Location, item.Location, notes);
        }

        public static string GetStatus(InventoryItem item)
        {
            if (item == null) return "Unknown";
            if (item.Quantity <= 0) return "Out";
            if (item.AvailableQuantity <= 0 && item.ReservedQuantity > 0) return "Reserved";
            if (item.Minimum > 0 && item.AvailableQuantity <= item.Minimum) return "Low";
            return "Available";
        }


        public static List<InventoryItem> SearchItems(string search)
        {
            var results = new List<InventoryItem>();
            if (search == null) search = "";
            search = search.Trim().ToLowerInvariant();
            foreach (var i in Database.Items)
            {
                string blob = ((i.ForgeId ?? "") + " " + (i.Barcode ?? "") + " " + (i.ItemName ?? "") + " " + (i.PartNumber ?? "") + " " + (i.Nsn ?? "") + " " + (i.Mrl ?? "") + " " + (i.CageCode ?? "") + " " + (i.SerialNumber ?? "") + " " + (i.Category ?? "") + " " + (i.Location ?? "") + " " + (i.Project ?? "") + " " + (i.Notes ?? "")).ToLowerInvariant();
                if (search.Length == 0 || blob.Contains(search)) results.Add(i);
            }
            return results;
        }

        public static List<AttachmentRecord> GetItemAttachments(string forgeId)
        {
            var results = new List<AttachmentRecord>();
            if (Database == null || Database.Attachments == null || string.IsNullOrWhiteSpace(forgeId)) return results;
            foreach (var a in Database.Attachments)
                if (EqualsIgnore(a.ForgeId, forgeId)) results.Add(a);
            return results;
        }

        public static int AddItemAttachments(InventoryItem item, IEnumerable<string> filePaths, string description)
        {
            DemandWriteAccess();
            if (item == null || string.IsNullOrWhiteSpace(item.ForgeId) || filePaths == null) return 0;
            if (Database.Attachments == null) Database.Attachments = new List<AttachmentRecord>();
            int count = 0;
            string itemDir = AttachmentManager.GetItemFolder(item.ForgeId);
            foreach (var sourcePath in filePaths)
            {
                if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath)) continue;
                string destination = AttachmentManager.SaveItemAttachment(item.ForgeId, sourcePath);
                if (string.IsNullOrWhiteSpace(destination) || !File.Exists(destination)) continue;
                var info = new FileInfo(destination);
                var rec = new AttachmentRecord
                {
                    AttachmentId = Guid.NewGuid().ToString("N"),
                    ForgeId = item.ForgeId,
                    FilePath = destination,
                    RelativePath = AttachmentManager.MakeRelativePath(destination),
                    FileName = Path.GetFileName(destination),
                    OriginalFileName = Path.GetFileName(sourcePath),
                    FileType = Path.GetExtension(destination).TrimStart('.').ToUpperInvariant(),
                    DocumentCategory = AttachmentManager.GuessDocumentCategory(destination),
                    AddedBy = CurrentOperator,
                    Added = DateTime.Now,
                    FileSizeBytes = info.Length,
                    Description = description ?? ""
                };
                Database.Attachments.Insert(0, rec);
                if (string.IsNullOrWhiteSpace(item.DocumentPath)) item.DocumentPath = destination;
                item.AttachmentFolder = itemDir;
                count++;
            }
            if (count > 0)
            {
                AddAudit("ATTACHMENT ADD", item, count + " document(s) attached to " + item.ForgeId);
                Save();
            }
            return count;
        }

        public static void RemoveItemAttachment(InventoryItem item, AttachmentRecord attachment, bool deleteFile)
        {
            DemandWriteAccess();
            if (attachment == null || Database == null || Database.Attachments == null) return;
            Database.Attachments.Remove(attachment);
            string path = ResolveAttachmentPath(attachment);
            if (deleteFile) AttachmentManager.DeleteAttachmentFile(attachment);
            if (item != null)
            {
                AddAudit("ATTACHMENT REMOVE", item, "Removed document " + attachment.FileName);
                if (EqualsIgnore(item.DocumentPath, path)) item.DocumentPath = "";
            }
            Save();
        }

        public static string ResolveAttachmentPath(AttachmentRecord attachment)
        {
            return AttachmentManager.Resolve(attachment);
        }

        public static string GetItemAttachmentFolder(string forgeId)
        {
            return AttachmentManager.GetItemFolder(forgeId);
        }

        public static string GuessDocumentCategory(string fileName)
        {
            return AttachmentManager.GuessDocumentCategory(fileName);
        }

        private static string MakeSafeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "Attachment";
            foreach (char c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
            return name.Trim();
        }

        private static string GetUniqueFilePath(string path)
        {
            if (!File.Exists(path)) return path;
            string dir = Path.GetDirectoryName(path);
            string name = Path.GetFileNameWithoutExtension(path);
            string ext = Path.GetExtension(path);
            for (int i = 1; i < 10000; i++)
            {
                string candidate = Path.Combine(dir, name + "_" + i + ext);
                if (!File.Exists(candidate)) return candidate;
            }
            return Path.Combine(dir, name + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ext);
        }

        private static string MakeRelativeToDataFolder(string path)
        {
            try
            {
                return RepositoryManager.MakeRelativePath(path);
            }
            catch { return path; }
        }

        public static string CreateBackupCopy()
        {
            Save();
            return BackupManager.BackupDatabase("Manual");
        }

        public static void SetAbsoluteQuantity(InventoryItem item, int newQuantity, string notes)
        {
            DemandWriteAccess();
            if (item == null) return;
            if (newQuantity < 0) newQuantity = 0;
            int old = item.Quantity;
            item.Quantity = newQuantity;
            AddTransaction(item, "CYCLE COUNT", item.Quantity - old, old, item.Quantity, item.Location, item.Location, notes);
        }

        public static void AddCategory(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return; name = name.Trim();
            foreach (var c in Database.Categories) if (EqualsIgnore(c.Name, name)) return;
            Database.Categories.Add(new CategoryRecord { Name = name, Notes = "" });
        }

        public static void AddLocation(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return; name = name.Trim();
            foreach (var l in Database.Locations) if (EqualsIgnore(l.Name, name)) return;
            Database.Locations.Add(new LocationRecord { Name = name, Notes = "" });
        }

        public static void AddProject(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            name = name.Trim();
            foreach (var p in Database.Projects) if (EqualsIgnore(p.Name, name)) return;
            Database.Projects.Add(new ProjectRecord { Name = name, ProjectCode = name.Replace(" ", "-").ToUpperInvariant(), Status = "Active", Priority = "Normal", Owner = "", Notes = "" });
        }

        public static List<InventoryItem> ItemsForProject(string projectName)
        {
            var results = new List<InventoryItem>();
            if (string.IsNullOrWhiteSpace(projectName)) return results;
            foreach (var i in Database.Items) if (EqualsIgnore(i.Project, projectName)) results.Add(i);
            return results;
        }


        public static void RemoveInventory(InventoryItem item, int quantity, string reason, string notes)
        {
            DemandWriteAccess();
            if (item == null) return;
            if (quantity < 0) quantity = Math.Abs(quantity);
            if (quantity > item.Quantity) quantity = item.Quantity;
            AdjustQuantity(item, -quantity, "REMOVE", (string.IsNullOrWhiteSpace(reason) ? "Manual remove inventory." : reason) + (string.IsNullOrWhiteSpace(notes) ? "" : " | " + notes));
        }

        public static void BorrowItem(InventoryItem item, int quantity, string borrowedByProject, string notes)
        {
            DemandWriteAccess();
            if (item == null || string.IsNullOrWhiteSpace(borrowedByProject)) return;
            if (quantity < 0) quantity = Math.Abs(quantity);
            if (quantity <= 0) quantity = 1;
            if (quantity > item.Quantity) quantity = item.Quantity;
            int oldQty = item.Quantity;
            string source = string.IsNullOrWhiteSpace(item.Project) ? "General" : item.Project;
            item.BorrowedItem = true;
            item.BorrowedBy = borrowedByProject.Trim();
            item.BorrowedFromProject = source;
            item.BorrowedQuantity += quantity;
            item.ReorderRequired = true;
            item.Quantity -= quantity;
            if (item.Quantity < 0) item.Quantity = 0;
            AddTransaction(item, "BORROW", -quantity, oldQty, item.Quantity, item.Location, item.Location, "Borrowed by " + item.BorrowedBy + " from " + source + ". " + (notes ?? ""));
        }

        public static List<InventoryItem> BorrowedItems()
        {
            var results = new List<InventoryItem>();
            foreach (var i in Database.Items) if (i.BorrowedItem || i.BorrowedQuantity > 0) results.Add(i);
            return results;
        }

        public static List<BomRecord> KitRequirements(string kitName)
        {
            var results = new List<BomRecord>();
            if (Database.BomItems == null) return results;
            foreach (var b in Database.BomItems)
                if (string.IsNullOrWhiteSpace(kitName) || EqualsIgnore(b.KitName, kitName) || EqualsIgnore(b.Project, kitName)) results.Add(b);
            return results;
        }

        public static void AddKitRequirement(string kitName, InventoryItem item, int requiredQuantity, string notes)
        {
            DemandWriteAccess();
            if (item == null || string.IsNullOrWhiteSpace(kitName)) return;
            if (requiredQuantity < 0) requiredQuantity = 0;
            if (Database.BomItems == null) Database.BomItems = new List<BomRecord>();
            Database.BomItems.Add(new BomRecord { KitName = kitName.Trim(), Project = kitName.Trim(), ForgeId = item.ForgeId, ItemName = item.ItemName, RequiredQuantity = requiredQuantity, ReservedQuantity = 0, Notes = notes ?? "" });
            AddAudit("KIT REQUIREMENT ADD", item, kitName + " requires " + requiredQuantity + " of " + item.ItemName);
            Save();
        }

        public static void AddBomItem(string project, InventoryItem item, int requiredQuantity, string notes)
        {
            DemandWriteAccess();
            if (item == null || string.IsNullOrWhiteSpace(project)) return;
            if (requiredQuantity < 0) requiredQuantity = 0;
            if (Database.BomItems == null) Database.BomItems = new List<BomRecord>();
            Database.BomItems.Add(new BomRecord { Project = project.Trim(), KitName = project.Trim(), ForgeId = item.ForgeId, ItemName = item.ItemName, RequiredQuantity = requiredQuantity, ReservedQuantity = 0, Notes = notes ?? "" });
            Save();
        }

        public static List<BomRecord> BomForProject(string project)
        {
            var results = new List<BomRecord>();
            if (Database.BomItems == null || string.IsNullOrWhiteSpace(project)) return results;
            foreach (var b in Database.BomItems) if (EqualsIgnore(b.Project, project)) results.Add(b);
            return results;
        }

        private static bool EqualsIgnore(string a, string b) { return string.Equals(a ?? "", b ?? "", StringComparison.OrdinalIgnoreCase); }

        private static void EnsureDefaultLists()
        {
            AddCategory("Raw Material"); AddCategory("Electronics"); AddCategory("Hardware"); AddCategory("Consumable"); AddCategory("Tooling"); AddCategory("Finished Product");
            AddLocation("Receiving"); AddLocation("Main Shelf"); AddLocation("Tool Room"); AddLocation("Printer Area"); AddLocation("CNC Area"); AddLocation("Issued");
            AddProject("General"); AddProject("Prototype"); AddProject("R&D"); AddProject("CV-22"); AddProject("MC-130J");
        }

        private static void Seed()
        {
            Database.CustomFields.Add(new CustomField { Name = "Material", Type = "Text" });
            Database.CustomFields.Add(new CustomField { Name = "Thickness", Type = "Text" });
            Database.CustomFields.Add(new CustomField { Name = "Vendor", Type = "Text" });
            EnsureDefaultLists();
            var demo = new InventoryItem { ForgeId = "FG-000001", Barcode = "DEMO-001", ItemName = "Demo Aluminum Sheet", PartNumber = "AL-125", Category = "Raw Material", Location = "Main Shelf", Quantity = 10, Minimum = 3, Status = "Available", Project = "General", Notes = "Sample record for initial testing." };
            Database.Items.Add(demo);
            Database.Transactions.Insert(0, new TransactionRecord { Time = DateTime.Now, ForgeId = demo.ForgeId, Barcode = demo.Barcode, ItemName = demo.ItemName, Action = "SEED", Delta = 0, OldQuantity = 10, NewQuantity = 10, FromLocation = demo.Location, ToLocation = demo.Location, Operator = CurrentOperator, WindowsUser = CurrentDomainUser, Workstation = CurrentMachineName, Project = demo.Project, Reason = "SEED", Notes = "Initial demo inventory item" });
        }
    }
}
