-- SOACS ForgeWorks SQLite schema foundation
-- v0.9 adds the planned production schema for migration to a native SQLite provider.
-- The current offline build keeps the project dependency-free and stores data through the ForgeDatabase model.

PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS SchemaInfo (
    Id INTEGER PRIMARY KEY CHECK (Id = 1),
    DatabaseVersion INTEGER NOT NULL,
    LastUpgradeUtc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Items (
    ForgeId TEXT PRIMARY KEY,
    Barcode TEXT,
    ItemName TEXT NOT NULL,
    PartNumber TEXT,
    Nsn TEXT,
    SerialNumber TEXT,
    Category TEXT,
    Location TEXT,
    Quantity INTEGER NOT NULL DEFAULT 0,
    ReservedQuantity INTEGER NOT NULL DEFAULT 0,
    UnitOfMeasure TEXT DEFAULT 'Each',
    Cost NUMERIC DEFAULT 0,
    StandardCost NUMERIC DEFAULT 0,
    LastPurchasePrice NUMERIC DEFAULT 0,
    Vendor TEXT,
    PreferredVendor TEXT,
    VendorPartNumber TEXT,
    Manufacturer TEXT,
    ManufacturerPartNumber TEXT,
    LeadTimeDays INTEGER DEFAULT 0,
    PurchaseDate TEXT,
    LastPurchaseDate TEXT,
    LastVendor TEXT,
    ReorderQuantity INTEGER DEFAULT 0,
    WarrantyExpiration TEXT,
    Status TEXT,
    Project TEXT,
    Notes TEXT,
    PhotoPath TEXT,
    AttachmentFolder TEXT,
    DateAdded TEXT,
    LastUpdated TEXT,
    LastScanned TEXT
);

CREATE TABLE IF NOT EXISTS ItemSerials (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ForgeId TEXT NOT NULL,
    SerialNumber TEXT NOT NULL,
    Status TEXT,
    Location TEXT,
    Project TEXT,
    Notes TEXT,
    FOREIGN KEY (ForgeId) REFERENCES Items(ForgeId)
);

CREATE TABLE IF NOT EXISTS Projects (
    Name TEXT PRIMARY KEY,
    ProjectCode TEXT,
    Status TEXT,
    Priority TEXT,
    Owner TEXT,
    DueDate TEXT,
    Notes TEXT
);

CREATE TABLE IF NOT EXISTS ProjectReservations (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Time TEXT NOT NULL,
    ForgeId TEXT NOT NULL,
    Project TEXT NOT NULL,
    Quantity INTEGER NOT NULL DEFAULT 0,
    Notes TEXT,
    FOREIGN KEY (ForgeId) REFERENCES Items(ForgeId)
);

CREATE TABLE IF NOT EXISTS ProjectBom (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Project TEXT NOT NULL,
    ForgeId TEXT NOT NULL,
    ItemName TEXT,
    RequiredQuantity INTEGER NOT NULL DEFAULT 0,
    ReservedQuantity INTEGER NOT NULL DEFAULT 0,
    Notes TEXT,
    FOREIGN KEY (ForgeId) REFERENCES Items(ForgeId)
);

CREATE TABLE IF NOT EXISTS Transactions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Time TEXT NOT NULL,
    ForgeId TEXT,
    Barcode TEXT,
    ItemName TEXT,
    Action TEXT,
    Delta INTEGER,
    OldQuantity INTEGER,
    NewQuantity INTEGER,
    FromLocation TEXT,
    ToLocation TEXT,
    Operator TEXT,
    Project TEXT,
    Reason TEXT,
    Notes TEXT
);

CREATE TABLE IF NOT EXISTS Locations (Name TEXT PRIMARY KEY, Notes TEXT);
CREATE TABLE IF NOT EXISTS Categories (Name TEXT PRIMARY KEY, Notes TEXT);
CREATE TABLE IF NOT EXISTS CustomFields (Name TEXT PRIMARY KEY, Type TEXT);
CREATE TABLE IF NOT EXISTS CustomFieldValues (ForgeId TEXT NOT NULL, Name TEXT NOT NULL, Value TEXT, PRIMARY KEY (ForgeId, Name));
CREATE TABLE IF NOT EXISTS Attachments (Id INTEGER PRIMARY KEY AUTOINCREMENT, ForgeId TEXT, FilePath TEXT, Description TEXT, Added TEXT);
CREATE TABLE IF NOT EXISTS AuditLog (Id INTEGER PRIMARY KEY AUTOINCREMENT, Time TEXT, Operator TEXT, Workstation TEXT, Action TEXT, Notes TEXT);

-- v1.2.0 field additions for future SQLite implementation
-- Items: Nomenclature label maps to existing ItemName field.
-- Items additional columns: CageCode TEXT, Mrl TEXT, DocumentPath TEXT,
-- BorrowedItem INTEGER, BorrowedQuantity INTEGER, BorrowedBy TEXT,
-- BorrowedFromProject TEXT, ReorderRequired INTEGER.
-- BOM/Kit requirements: KitName TEXT, RequiredQuantity INTEGER, Notes TEXT.

-- v2.3.6 Attachment metadata table (future SQLite implementation)
CREATE TABLE IF NOT EXISTS ItemAttachments (
    AttachmentID TEXT PRIMARY KEY,
    ForgeID TEXT NOT NULL,
    FileName TEXT,
    OriginalFileName TEXT,
    RelativePath TEXT,
    FilePath TEXT,
    FileType TEXT,
    DocumentCategory TEXT,
    Description TEXT,
    AddedBy TEXT,
    AddedDate TEXT,
    FileSizeBytes INTEGER
);
