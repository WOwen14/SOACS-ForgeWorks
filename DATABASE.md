
## Database Version 6 - Procurement Fields

Added procurement planning fields to `InventoryItem` / `Items`:

- Unit Cost (`Cost`)
- Standard Cost
- Last Purchase Price
- Preferred Vendor
- Vendor Part Number
- Manufacturer
- Manufacturer Part Number
- Lead Time Days
- Last Purchase Date
- Last Vendor
- Reorder Quantity

Existing Cost/Vendor values are preserved and used as defaults for the new procurement workflow.

# Database Notes

Current application database version: 3

## Core Data Areas
- Items
- Item serial records
- Projects
- Transactions
- Reservations
- BOM items
- Attachments
- Audit log
- Categories
- Locations
- Custom fields
- Workstation settings

## v1.0.0 Reporting Impact
No destructive schema changes. Reporting uses existing inventory, project, transaction, audit, storage, reservation, and BOM data.


## v1.2.0 - Parts, Borrowing, Attachments, and Kit Requirements

### Added
- Renamed item Name workflow label to Nomenclature.
- Added CAGE Code and MRL / Master Requirements List fields.
- Added part document attachment path and Attach Document workflow.
- Added Remove Inventory action from Item Workspace with confirmation dialog.
- Added Borrow Item operation to Operations Center.
- Added Borrowed Item tracking, Borrowed By project, borrowed quantity, and reorder-required tracking.
- Added Borrowed Items report for reorder/return review.
- Added reusable Kit Requirements / Shortages report.
- Added Add Kit Requirement workflow from Reports Center.
- Added part search panel on Operations Center for issuing/removing/borrowing by nomenclature, NSN, part number, barcode, serial, MRL, or project.

### Changed
- Inventory and reports now expose Nomenclature, CAGE, MRL, borrowed status, and document fields.
- Database schema version incremented for new part and borrowing metadata.

### Notes
- Borrowing subtracts the borrowed quantity from the source item availability, records the borrowing project, and marks the item for reorder review.
- Kit requirement checks compare required quantity against available inventory and report OK or SHORT.


## Database Version 5 - Attachments

### Attachment Storage Model
Documents are stored on disk under the ForgeWorks data folder and referenced by metadata in the database. Files are not stored as database BLOBs.

Default folder structure:

```text
ForgeWorksData
├── ForgeWorksInventory.xml
├── Backups
└── Attachments
    └── Items
        └── FG-000001
            ├── Drawing.pdf
            ├── Datasheet.pdf
            └── Photo.jpg
```

### Attachment Metadata
Each attachment record tracks:

- AttachmentId
- ForgeId
- FileName
- OriginalFileName
- RelativePath
- FilePath
- FileType
- DocumentCategory
- Description
- AddedBy
- Added
- FileSizeBytes

### Portability
Relative paths are used so the complete ForgeWorksData folder can be moved to another workstation or network share without breaking attachment references.

## v3.0.0 - Repository Manager Foundation

ForgeWorks now uses a Repository Root to organize all mutable data.

```text
RepositoryRoot
├── Database
│   └── ForgeWorksInventory.xml
├── Attachments
│   ├── Items
│   ├── Projects
│   └── Kits
├── Photos
├── Reports
├── Logs
├── Backups
├── Config
└── Temp
```

The workstation configuration stores the Repository Root and Repository Profile. Application modules should use RepositoryManager for path resolution rather than hard-coded paths.


## v3.0.1 - Repository Integration Phase 2

- Added AttachmentManager, PhotoManager, ReportManager, LogManager, and BackupManager services.
- Routed item attachments through the Repository Manager.
- Routed item camera/photo saves through the Repository Manager photo repository.
- Added Repository Health panel to Administration with verify/repair, open repository, and backup actions.
- Continued centralizing file access around the Data Root repository structure.
