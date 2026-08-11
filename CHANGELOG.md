# SOACS ForgeWorks Changelog

## v3.1.3 - Inventory Workspace Update

### Added
- Redesigned Inventory Workspace with search and quick filters.
- Added item summary and procurement summary panels.
- Added document preview in the Inventory Workspace.
- Added Inventory column chooser with saved workstation preferences.
- Added right-click inventory actions for faster user workflow.

### Changed
- Application project metadata updated to version 3.1.3.0.
- Continued refinement of the repository-managed data architecture introduced in v3.0.x.

### Status
- Current source baseline.
- Live as a standalone deployment for user testing.

## v3.1.1 - Repository Profile Hotfix

### Fixed
- Replaced obsolete `LiveDataBus.Publish(...)` call with the existing `LiveDataBus.NotifyDataChanged()` method.
- Removed the unused legacy `txtDocument` field left over from the single-document attachment workflow.

### Changed
- Updated build labels from v3.1.0 to v3.1.1.


## v2.3.7 - Procurement & Cost Tracking

### Added
- Added a Procurement tab to the Item Workspace.
- Added Unit Cost, Standard Cost, Last Purchase Price, Preferred Vendor, Vendor Part Number, Manufacturer, Manufacturer Part Number, Lead Time, Last Purchase Date, Last Vendor, and Reorder Quantity fields.
- Added Inventory Value and Low Inventory Reorder Cost report support.
- Extended existing Low Inventory reporting with procurement/cost data so reorder cost can be calculated from item records.

### Changed
- Database model version increased to support procurement planning fields.
- Existing legacy Vendor/Cost fields remain backward compatible and map to the new Preferred Vendor / Unit Cost workflow.

## v2.0.2 - Operations Search + Adjust Quantity

### Added
- Added Adjust Qty operation with reason and notes prompt.
- Added Use Selected workflow for Operations part search.

### Changed
- Clarified Operations search as Find Part When Barcode Is Unknown.
- Search results now show Forge ID, nomenclature, part number, NSN, quantity, and location.
- Improved Operations layout to keep Scan/Type and search panels visible.

### Fixed
- Reduced Operations button width to prevent clipping/wrapping issues.

## v1.0.0 Reporting Preview

### Added
- Reporting & Intelligence Center.
- Inventory, project, transaction, audit, storage, and executive reports.
- Project Parts List report.
- Project Transaction report.
- Print Preview and direct Print support.
- CSV export support.
- Report Builder-style column chooser.
- Saved report templates (`.fwrpt`).
- Project filter and time range filter for reports.
- About page with ForgeWorks logo, Operator Guide, Help Me, What's New, and Admin Guide buttons.

### Changed
- Reports page is now a report center instead of a single static grid.
- Versioning moved to semantic versioning format.
- Documentation is consolidated under the `docs` folder.

### Fixed
- Reduces report clutter by allowing users to hide Windows User, Workstation, and other columns when not needed.

## v1.0.1 - Startup Hardening

### Fixed
- Prevented early UI BeginInvoke calls before a control handle is created.
- Corrected Report Center column chooser startup timing issue.

### Changed
- Updated application build labels to v1.0.1.
- Added startup logging support under Logs/startup.log.


## v1.1.0 Development Foundation
- Fixed startup RefreshShellStatus compile issue.
- Added StartupManager and StatusManager foundation.
- Added AuditManager and DatabaseManager wrappers for future SOACS shell standardization.
- Updated version labels and navigation footer.

## v1.1.1 Operations + Photo Capture

### Added
- Rebuilt Operations page around large task buttons instead of a dropdown.
- Added top-down workflow: operation, transaction details, item input, result/activity.
- Added Keep Qty, Keep Project, and Keep Location options for processing multiple items.
- Added OperationContext handoff so Create New Item passes quantity, project, location, notes, and scanned barcode into Item Workspace.
- Added Take Photo option in Item Workspace photo area.
- Added camera capture helper using Windows Camera plus Browse/Use Photo workflow for offline compatibility.

### Fixed
- Create New Item now starts with the Operations quantity instead of defaulting to one/zero.
- Reduced operator back-and-forth on the Operations page.

### Notes
- Native webcam preview requires a DirectShow/MediaCapture dependency. This build uses the Windows Camera handoff path to remain offline and no-NuGet compatible.

## v1.1.2 - Stabilization / Constructor Fix

### Fixed
- Removed the legacy ItemEditorForm string constructor that caused CS0121 ambiguous constructor errors.
- Updated Inventory, Operations, Projects, and Storage Explorer to use OperationContext consistently.
- Preserved Operations-to-Item Workspace handoff for scanned barcode, quantity, project, and location context.

### Changed
- Item Workspace now has one standard constructor path for both new and existing items.

## v1.1.3 - Operations UI Polish

### Changed
- Reworked the Operations Center layout to reduce unused whitespace and keep the workflow more compact.
- Updated Operations input fields, dropdowns, and activity list to use softer dark theme colors instead of harsh white controls.
- Improved Last Item panel visibility and spacing.
- Reduced top panel height and adjusted Activity/Workflow layout balance.

### Fixed
- Operations page layout felt spread out with large unused right-side space.
- White input boxes and activity area were harsh against the dark SOACS theme.

## v1.1.4 - Theme + Operations Layout Fix

### Changed
- Applied the softer dark-theme input styling across the application so text boxes, combo boxes, lists, and grids no longer appear as harsh white controls on the dark background.
- Expanded the Operations Center layout so all operation buttons are fully visible.
- Reworked the Operations transaction detail controls so Qty/Count, Project, Location/Move To, and Notes remain editable.

### Fixed
- Fixed the bottom row of Operations buttons being clipped.
- Fixed transaction detail entry controls being effectively unusable after the v1.1.3 UI polish pass.
## v1.1.5 - Shell + Operations + Camera Fix

### Fixed
- Reworked the SOACS header layout so the ForgeWorks logo is the left-most brand element.
- Added ForgeWorks icon support for the application, taskbar, and executable.
- Corrected Operations Center transaction detail clipping and field layout.
- Improved Operations layout scaling at smaller workstation window sizes.
- Updated camera workflow so captured Windows Camera photos can be accepted directly with Use Latest Camera Photo and attached to the item.

### Changed
- Updated build metadata to v1.1.5.
- Improved About page document viewer to use the dark SOACS theme.


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

## v2.0.1 - Kits Page Fix

### Fixed
- Rebuilt the Kits page layout so action buttons are no longer clipped or covered by the requirements grid.
- Moved kit actions into a dedicated right-side action panel with readable button text.
- Fixed inventory search results in Add Kit Requirement so they display Forge ID, nomenclature, part number, NSN, quantity, and location instead of `SOACSForgeWorks.InventoryItem`.

### Changed
- Added a readable `InventoryItem.ToString()` fallback for list and combo display.
