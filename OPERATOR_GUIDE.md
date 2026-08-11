# SOACS ForgeWorks Operator Guide

## Dashboard
Use the Dashboard for current inventory, projects, recent activity, and quick status.

## Operations
Use Operations when you are receiving, issuing, moving, counting, returning, scrapping, or looking up inventory.

1. Select the current operation.
2. Scan a barcode or type a Forge ID, barcode, serial number, part number, or NSN.
3. Press Enter.
4. Complete the prompted workflow.

## Inventory
Use Inventory to view and manage item records. Quantity changes should normally be performed from Operations or transaction workflows so they create a transaction history.

## Projects
Use Projects to manage project records and view parts or pieces assigned to a project.

## Storage Explorer
Use Storage Explorer to browse inventory by location.

## Transactions
Use Transactions to review inventory movement and accountability history.

## Reports
Use Reports to print inventory reports, transaction logs, parts lists per project, storage reports, audit logs, and executive summaries.

## Printing Reports
1. Open Reports.
2. Select a Report Type.
3. Select Time Range and Project if needed.
4. Choose visible columns.
5. Click Print Preview, Print, or Export CSV.

## Operations Workflow Update

Use Operations from top to bottom: choose the operation, enter quantity/project/location/notes, then scan or type the item identifier and press Enter. For Create New Item, those values are passed into the Item Workspace automatically. Use Keep Qty/Project/Location when processing multiple items with the same context.

## Item Photos

In Item Workspace, use Browse Photo to select an existing image, Take Photo to open the camera/photo helper, or Clear Photo to remove the current image path.

## Camera Photo Workflow

1. Open an item in the Item Workspace.
2. Click Take Photo.
3. Click Open Camera and take the photo using Windows Camera.
4. Return to ForgeWorks and click Use Latest Camera Photo.
5. Confirm the preview and click Use Photo.
6. ForgeWorks copies the image into the ForgeWorks photo folder and attaches it to the item automatically.


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


## v2.0.0 PC1 - User Experience, Kits, and Theme Completion
- Added dedicated Kits module for reusable kit requirements and readiness checks.
- Added kit readiness grid showing required, on-hand, available, shortage, and status.
- Applied unified dark grid theme to remove remaining white alternating rows.
- Improved Operations page spacing, larger operation buttons, taller input area, and clearer keep options.
- Updated shell/versioning to v2.0.0 Production Candidate 1.
- Prepared DaggerBridge-style documentation framework for Operator Guide, Help Me, What's New, and Admin Guide.


## Documents / Attachments

Each part record can now hold multiple attachments. Open an item and select the **Documents / Attachments** tab.

Available actions:

- **Add Documents**: Attach one or more files to the part record.
- **Open**: Open the selected document using the default Windows application.
- **Print**: Send the selected document to the default print handler for that file type.
- **Remove**: Remove the selected attachment from ForgeWorks and the item attachment folder.
- **Open Folder**: Open the item's attachment directory in File Explorer.

Supported examples include PDF drawings, datasheets, Word documents, Excel files, photos, CAD exports, STL files, DXF files, ZIP files, and other technical documentation.

## Procurement Tab

Each part record now includes a **Procurement** tab. Use this tab to maintain cost and supplier information for reports and reorder planning.

Fields include:

- Unit Cost
- Standard Cost
- Last Purchase Price
- Preferred Vendor
- Vendor Part Number
- Manufacturer
- Manufacturer Part Number
- Lead Time (Days)
- Last Purchase Date
- Last Vendor
- Reorder Quantity

The Low Inventory and Inventory Value reports use these values to estimate inventory value and replenishment cost.


## v3.0.1 - Repository Integration Phase 2

- Added AttachmentManager, PhotoManager, ReportManager, LogManager, and BackupManager services.
- Routed item attachments through the Repository Manager.
- Routed item camera/photo saves through the Repository Manager photo repository.
- Added Repository Health panel to Administration with verify/repair, open repository, and backup actions.
- Continued centralizing file access around the Data Root repository structure.
