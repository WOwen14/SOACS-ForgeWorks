# SOACS ForgeWorks

<p align="center">
  <img src="Assets/GitHub-Logo.jpg" alt="SOACS ForgeWorks" width="400">
</p>

**Mission fabrication operations, inventory, project-material management, and reporting for disconnected or controlled Windows environments.**

SOACS ForgeWorks is a Windows desktop application designed around day-to-day fabrication-shop workflows. It combines inventory and serialized-asset management with projects, reservations, kits/BOMs, attachments, borrowing, transaction history, reporting, and operator accountability rather than forcing those workflows into a generic inventory product.

## Current baseline

| Item | Baseline |
| --- | --- |
| Version | **3.1.4 RC1** |
| Status | **Live / User testing** |
| Platform | Windows |
| Application | Windows Forms |
| Framework | .NET Framework 4.8 |
| Build target | Any CPU |
| Data | SQLite with local or shared repository profiles |
| External packages | No NuGet packages required |

The v3.1.4 RC1 baseline concentrates on operational readiness, tester feedback, repository health, and layout reliability at 1920×1080 with 125% display scaling.

## Core capabilities

### Inventory and assets

- Nomenclature, part number, NSN, CAGE code, MRL, location, quantity, and notes
- Serialized-item and asset tracking
- Procurement, vendor, manufacturer, lead-time, cost, and reorder information
- Low-inventory, out-of-stock, borrowed, and reorder-required visibility
- Multi-document attachments and item photos
- Barcode/scanner-assisted and manual item workflows

### Projects, kits, and material

- Project status and priority tracking
- Project material reservations and parts lists
- Reusable kit/BOM requirements
- On-hand, available, shortage, and readiness calculations
- Borrowed-item and Borrowed By project tracking

### Operations and accountability

- Receive, issue, borrow, move, remove, and adjust-quantity workflows
- Reason and notes capture for quantity adjustments
- Recent-operation and transaction history
- Windows-user and computer accountability data
- Live refresh across major workspaces for shared-database use

### Reporting and administration

- Inventory, low-stock, out-of-stock, borrowed-item, storage, project, transaction, audit, and executive reports
- Report column selection and saved templates
- CSV export and Windows Print Preview / Print to PDF
- Repository profiles, health verification, repair actions, and backups
- Feedback packages with version, environment, diagnostics, comments, and optional application screenshot

## Repository architecture

ForgeWorks uses a configurable Repository Root so operational data is not scattered across unrelated workstation paths. The repository manager maintains folders for:

- Database
- Attachments
- Photos
- Reports
- Logs
- Backups
- Configuration
- Temporary data
- Feedback packages

Repository profiles support Production, Test Lab, Standalone, and Offline configurations. The current baseline supports local and shared repository locations. Automatic offline fallback, synchronization queues, and conflict handling remain planned work and are not represented as completed capabilities.

## Build

### Requirements

- Windows
- Visual Studio 2019 or later with .NET Framework desktop development support
- .NET Framework 4.8 Developer Pack

### Build steps

1. Clone or download the repository.
2. Open `SOACSForgeWorks.csproj` in Visual Studio.
3. Select `Debug` or `Release` and `Any CPU`.
4. Build the project.
5. Find the output under `bin\Debug\` or `bin\Release\`.

The project uses standard .NET Framework references and is suitable for offline development environments.

## Documentation

- [Operator Guide](docs/OPERATOR_GUIDE.md)
- [Administrator Guide](docs/ADMIN_GUIDE.md)
- [Help](docs/HELP_ME.md)
- [What's New](docs/WHATS_NEW.md)
- [Changelog](docs/CHANGELOG.md)
- [Known Issues](docs/KNOWN_ISSUES.md)
- [Roadmap](docs/ROADMAP.md)
- [Database Reference](docs/DATABASE.md)
- [Test Plan](docs/TEST_PLAN.md)

## Repository safety

This public repository is intended to contain source code, documentation, and synthetic examples only.

Do **not** commit operational inventories, customer or project identifiers, real asset records, user feedback packages, attachments, photographs, database files, backups, reports, logs, credentials, or other controlled information.

Generated runtime data, build output, workstation settings, temporary files, Visual Studio workspace files, and test repositories are excluded through `.gitignore`.

## Development workflow

- `main` — stable reviewed or user-test baseline
- `develop` — integrated development
- `feature/*` — isolated feature work
- `fix/*` — defect corrections

New work should merge into `develop` first. Tested changes can then be promoted to `main` through a pull request.

## Current limitations

- PDF output uses Windows Print Preview / Print to PDF rather than a built-in PDF writer.
- Spreadsheet export is CSV-compatible; native XLSX export is not currently implemented.
- Multi-user shared-database behavior remains under active hardening.
- Automatic offline fallback and synchronization remain planned capabilities.

---

**SOACS ForgeWorks**  
**Mission Fabrication Operations**
