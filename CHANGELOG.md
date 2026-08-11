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

# SOACS ForgeWorks Changelog

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
- Updated build metadata to v2.0.1.

## v2.1.0 - Live Data Engine + Full Screen Startup

### Added
- Live Data Engine that broadcasts inventory/database changes after saves and quantity updates.
- Automatic refresh support for Dashboard, Inventory, Storage Explorer, Projects, Kits, Transactions, Reports, and Administration pages.
- Background database change watcher for shared database setups.
- Footer sync status indicator showing the last live refresh time.

### Changed
- Application now opens maximized by default for better operator use.
- Operations Center layout adjusted so Scan or Type Item remains visible in maximized and windowed modes.
- Updated build metadata to v2.1.0.

### Notes
- Manual Refresh buttons remain available as a fallback, but normal saves and quantity changes now trigger live refresh automatically.

## v2.1.1 - Startup Stability

### Fixed
- Delayed Live Refresh startup until after the main window completes its first paint.
- Prevented refresh events from firing while the application is still initializing.
- Reduced startup flicker/glitching caused by early redraws and timer-based refresh events.

### Changed
- Splash screen now stays visible long enough to complete the progress bar.
- Splash screen now displays startup status messages and closes after a minimum three-second startup presentation.
- Updated displayed build version to v2.1.1.

## v2.1.2 - Application Operations Workspace Console

### Fixed
- Reduced startup flicker by hiding the main window until shell initialization completes.
- Delayed Live Refresh until after the first stable layout pass.
- Prevented heavier pages from loading during initial startup.

### Changed
- Added lazy page loading so only Dashboard is created at launch.
- Added double buffering pass across the form/control tree.
- Splash screen now runs longer, reaches 100%, and displays clearer startup progress states.

### Notes
- This release focuses on startup polish and reducing visible UI construction during launch.

## v2.3.0 - Operations Workspace Console

### Added
- Redesigned Operations Center as a task-focused workspace.
- Added separated scan-vs-type input section for clearer operator workflow.
- Added Current Operation / Last Item panel.
- Added dynamic workflow guidance based on selected operation.
- Updated application subtitle to Mission Fabrication Operations.

### Changed
- Rebuilt Operations layout to reduce clipping and improve usability.
- Reorganized operation buttons into a cleaner two-row workspace layout.
- Improved part search area for cases where barcode is unknown.
- Extended splash screen behavior so the progress reaches 100% and holds before launch.

### Fixed
- Operator Guide/Help document viewer now fails safely instead of crashing the application.
- Operations input area visibility improved for maximized and standard window sizes.

## v2.3.0 - Operations Workspace Console

### Added
- Tighter Operations button grid with keyboard shortcut hints.
- Larger Scan / Type item workspace.
- Search results grid with Forge ID, nomenclature, quantity, and location columns.
- Recent Operations grid with time, operation, item, quantity, and user.
- Expanded Current Operation card with operator, quantity, project, location, and last item context.
- Dynamic workflow checklist wording for each operation.

### Changed
- Application subtitle changed to **Mission Fabrication Operations**.
- Transaction Details spacing increased for better readability.
- Operations page card layout refined to feel more like an operator console.

### Fixed
- Operations page scan/search area crowding.
- Search results display now uses a grid instead of list text.
- Splash and About metadata updated to v2.3.0.


## v2.3.1 - Operations Workspace Scroll/Fit

### Fixed
- Rebuilt Operations Workspace host as a scrollable card workspace to prevent clipping at 125% display scaling and smaller windows.
- Increased Operations card heights for transaction details, scan/type item, search, current operation, workflow guidance, and recent operations.
- Kept application header, left navigation, and status bar fixed while Operations content scrolls safely.

### Changed
- Operations Workspace now uses a reusable scroll-safe layout approach intended to become the SOACS workspace pattern.

## v2.3.2 - Operations Layout Fit
- Reworked Operations transaction details layout to prevent clipped fields at 125% display scaling.
- Increased Operations workspace card heights and spacing while keeping fixed shell/header/status layout.
- Improved Keep checkbox placement and added a Keep selections helper row.
- Minor scan/search card spacing adjustments for readability.

## v2.3.3 - Operations Scan Fit Polish

### Fixed
- Increased Operations workspace scanner/search card height so helper/status text no longer clips at 125% display scaling.
- Increased internal padding for scanner and type/search panels.
- Adjusted Operations workspace row heights so scan/search receives more usable vertical space without changing functionality.

### Changed
- Minor UI-only polish to Operations workspace layout. No workflow or data behavior changed.


## v2.3.4 - Project List + Splash Polish

### Changed
- Simplified the Projects navigation list to Project, Status, and Priority only.
- Widened the Projects list panel to reduce column crowding.
- Centered the splash screen content block so logo, title, subtitle, loading text, and progress bar align properly.

### Fixed
- Removed unnecessary Project Code and Item Count columns from the left Projects list.
- Applied the unified dark grid theme to the Projects page grids.


## v2.3.5 - Live Refresh Recursion Fix

### Fixed
- Fixed Dashboard project filter refresh recursion that could cause System.StackOverflowException.
- Added guard logic so project filter rebuilds do not trigger dashboard refresh repeatedly.
- Added global refresh re-entry protection during Live Refresh events.
- Added LiveDataBus re-entry protection to prevent nested data change notifications.

### Changed
- No functional workflow changes. This is a stability-only release.


## v2.3.6 - Multi-Document Attachments + Branding

### Added
- Added multi-document attachment support in the Item Workspace.
- Added Documents / Attachments tab with Add Documents, Open, Print, Remove, and Open Folder actions.
- Added attachment metadata tracking: file name, type, category, added by, date added, and notes.
- Added portable attachment folder structure under ForgeWorksData\Attachments\Items\<Forge ID>.

### Changed
- Replaced the single attached document workflow with a multi-document attachment manager.
- Updated ForgeWorks logo assets with the latest approved SOACS ForgeWorks logo.
- Updated application version to v2.3.6.

### Notes
- Documents are stored on disk, not inside the database. The database stores metadata and paths only.
- Open and Print use the default Windows application registered for each file type.

## v3.0.0 - Repository Manager Foundation

### Added
- Added Repository Manager foundation with a single configurable Repository Root.
- Added automatic repository folder creation for Database, Attachments, Photos, Reports, Logs, Backups, Config, and Temp.
- Added repository-aware path resolution for the inventory database and item attachments.
- Added Repository Profile, Repository Root, and shared/network repository settings to Settings.
- Added repository health information including repository paths and free space.

### Changed
- ForgeWorks now derives storage paths from the Repository Root instead of scattered local paths.
- Backups are now stored under the repository Backups folder.
- Item attachments are now stored under the repository Attachments folder.

### Notes
- This is Phase 1 of the distributed operations architecture. Offline mode and synchronization are planned future phases.


## v3.0.1 - Repository Integration Phase 2

- Added AttachmentManager, PhotoManager, ReportManager, LogManager, and BackupManager services.
- Routed item attachments through the Repository Manager.
- Routed item camera/photo saves through the Repository Manager photo repository.
- Added Repository Health panel to Administration with verify/repair, open repository, and backup actions.
- Continued centralizing file access around the Data Root repository structure.


## v3.1.0 - Repository Profile Manager

### Added
- Repository Profile dropdown in Settings.
- Default profiles: Production, Test Lab, Standalone, and Offline.
- XML-backed profile storage under the workstation configuration folder.
- Switch Profile workflow with confirmation and repository reload.
- Profile color/status indicator in the status bar.
- Administration Repository Health now shows active profile and profile list.

### Changed
- Repository status now reports the active profile.
- Build/version labels updated to v3.1.0.

## v3.1.2 - Administration Workspace Polish

### Fixed
- Fixed Repository action button clipping on the Administration page at 125% display scaling.
- Increased Repository section height and spacing.

### Changed
- Added Repository Summary layout with clearer profile, root, health, and free-space details.
- Added Copy Root Path action.
- Improved Repository Root display with ellipsis and full-path tooltip.


## v3.1.3 - Inventory Workspace Modernization

### Added
- Modern Inventory Workspace layout with statistics cards, quick filters, item preview, procurement summary, and attachment preview.
- Global inventory search across Forge ID, nomenclature, NSN, MRL, CAGE, vendor, manufacturer part number, notes, and custom values.
- Quick filters for Low Inventory, Borrowed, Needs Reorder, Has Documents, Has Photo, No Vendor, and Out.
- Column chooser for the Inventory grid with per-workstation persistence.
- Right-click action menu for Open Item, Receive, Issue, Borrow, Move, Adjust Quantity, Open Documents, and Column Chooser.

### Changed
- Inventory page renamed visually to Inventory Workspace.
- Inventory grid focuses on high-value columns and moves detail information into the summary pane.
- Item selection now updates a live summary card without requiring the item editor to be opened.

### Fixed
- Reduced reliance on cramped all-column grid layouts in the Inventory page.
