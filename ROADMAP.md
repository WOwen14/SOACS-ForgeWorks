# SOACS ForgeWorks Roadmap

## Completed
- SOACS shell and branding
- Inventory management
- Operations workflow
- Project management
- Storage Explorer
- User audit logging
- Dashboard filters
- Transaction column chooser
- Reporting & Intelligence Center

## In Progress
- Multi-user shared database hardening
- Serialized asset workflow
- Project reservations and BOMs

## Planned
- Work orders
- Tool checkout
- Calibration tracking
- Barcode/QR label printing
- Kit management
- Report scheduling


## v1.1.0 Development Foundation
- Fixed startup RefreshShellStatus compile issue.
- Added StartupManager and StatusManager foundation.
- Added AuditManager and DatabaseManager wrappers for future SOACS shell standardization.
- Updated version labels and navigation footer.

## Completed in v1.1.5

- Shell/header branding pass.
- Operations Center layout correction.
- Camera photo accept/attach workflow.


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


## v3.0.1 - Repository Integration Phase 2

- Added AttachmentManager, PhotoManager, ReportManager, LogManager, and BackupManager services.
- Routed item attachments through the Repository Manager.
- Routed item camera/photo saves through the Repository Manager photo repository.
- Added Repository Health panel to Administration with verify/repair, open repository, and backup actions.
- Continued centralizing file access around the Data Root repository structure.


## v3.1.0 Completed
- Repository Profile Manager foundation.
- Profile switching workflow.
- Profile-aware repository health.

## Next
- Offline detection and local repository fallback.
- Synchronization queue and conflict handling.
