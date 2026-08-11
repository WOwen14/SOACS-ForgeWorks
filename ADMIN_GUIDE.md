# SOACS ForgeWorks Admin Guide

## Workstation Mode
Operator Mode allows data entry and transactions. Read-Only Viewer mode allows viewing inventory, projects, reports, and history without making changes.

## Database Path
Use Settings to select local or shared database mode. For small teams, a shared network path can be used with retry handling and backups.

## Backups
ForgeWorks creates backup files before database upgrades and when backups are manually requested.

## Locations and Categories
Maintain categories, storage locations, projects, and dynamic fields from Administration.

## Documentation
All operator and admin documentation is stored in the `docs` folder and is opened from the About page.


## v2.0.0 PC1 - User Experience, Kits, and Theme Completion
- Added dedicated Kits module for reusable kit requirements and readiness checks.
- Added kit readiness grid showing required, on-hand, available, shortage, and status.
- Applied unified dark grid theme to remove remaining white alternating rows.
- Improved Operations page spacing, larger operation buttons, taller input area, and clearer keep options.
- Updated shell/versioning to v2.0.0 Production Candidate 1.
- Prepared DaggerBridge-style documentation framework for Operator Guide, Help Me, What's New, and Admin Guide.

## Repository Manager

ForgeWorks v3.0 introduces a single Repository Root. This can be a local folder or a network share. The application creates and maintains these subfolders automatically: Database, Attachments, Photos, Reports, Logs, Backups, Config, and Temp.

To configure it:
1. Open Settings.
2. Enter or browse to the Repository Root.
3. Select whether it is a shared/network repository.
4. Click Save Settings.
5. Click Verify Repo to confirm the folder structure and available free space.

For a shared shop setup, install the application locally on each workstation and point each workstation to the same Repository Root.


## v3.0.1 - Repository Integration Phase 2

- Added AttachmentManager, PhotoManager, ReportManager, LogManager, and BackupManager services.
- Routed item attachments through the Repository Manager.
- Routed item camera/photo saves through the Repository Manager photo repository.
- Added Repository Health panel to Administration with verify/repair, open repository, and backup actions.
- Continued centralizing file access around the Data Root repository structure.


## Repository Profiles
ForgeWorks v3.1.0 supports repository profiles. Use Settings > Repository Profile to select Production, Test Lab, Standalone, or Offline. Click Switch Profile to reconnect ForgeWorks to the selected repository. Profiles are stored as XML in the workstation configuration folder and can be edited later as the deployment matures.
