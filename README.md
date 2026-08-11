# SOACS ForgeWorks v1.0.0 Reporting Preview

SOACS ForgeWorks is an offline-capable inventory, project material, fabrication shop, and reporting application for the Command Forge.

## Build Notes
- Visual Studio compatible source package.
- .NET Framework 4.8 WinForms application.
- Portable-friendly design.
- Supports local and shared database configuration.

## New in v1.0.0
- Reporting & Intelligence Center
- Print Preview / Print
- CSV export
- Project parts lists
- Transaction logs
- Audit reports
- Storage reports
- Executive dashboard report
- Report column chooser
- Saved report templates
- About page with logo and operator resources

## Documentation
See the `docs` folder.


## v1.1.5 Shell + Operations + Camera Fix
This build begins separating shell/startup/status/audit/database responsibilities from MainForm so ForgeWorks can grow into a maintainable SOACS suite application.


## v3.0.1 - Repository Integration Phase 2

- Added AttachmentManager, PhotoManager, ReportManager, LogManager, and BackupManager services.
- Routed item attachments through the Repository Manager.
- Routed item camera/photo saves through the Repository Manager photo repository.
- Added Repository Health panel to Administration with verify/repair, open repository, and backup actions.
- Continued centralizing file access around the Data Root repository structure.
