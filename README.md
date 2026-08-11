# SOACS ForgeWorks

**Mission-focused fabrication operations, inventory, project material, and asset management for disconnected and constrained environments.**

ForgeWorks is a Windows desktop application I developed as part of the SOACS software suite to support real fabrication-shop workflows. It combines inventory control, project material tracking, kits/BOMs, serialized assets, attachments and photos, borrowing, reporting, audit history, and repository management in a single offline-capable application.

## Current Status

- **Current source build:** v3.1.3
- **Status:** Live / Standalone User Testing
- **Platform:** Windows desktop
- **UI:** Windows Forms
- **Framework:** .NET Framework 4.8
- **Architecture:** Offline-capable with local/shared data support
- **Primary data store:** SQLite

> The current source snapshot contains older version labels in some historical UI/documentation locations. The project metadata and `WHATS_NEW.md` identify v3.1.3 as the current source build. Version labels are being normalized as part of ongoing development.

## Purpose

ForgeWorks was built around the actual workflow of a fabrication shop rather than adapting a generic commercial inventory package to the mission. The goal is to reduce manual tracking, improve accountability, and give users a single place to manage material, projects, kits, serialized equipment, documents, photos, transactions, and reporting.

## Core Capabilities

### Inventory & Asset Management
- Nomenclature, part numbers, NSN, CAGE, MRL, serial numbers, barcodes, and Forge IDs
- Quantity adjustment with reason and notes tracking
- Storage locations and inventory search
- Procurement and cost fields
- Low-stock and reorder visibility
- Serialized asset control

### Operations
- Scan or type item identifiers
- Issue, remove, borrow, move, and adjust inventory
- Operator-focused transaction workflow
- Project/location/quantity context retention for repetitive work
- Windows user and workstation accountability

### Projects & Kits
- Project material tracking
- Project parts lists
- Reusable kit/BOM requirements
- Kit readiness and shortage calculations
- Reservations and project allocation workflows

### Documents & Photos
- Item document attachments
- Photo capture and photo repository support
- Repository-managed attachments and photos
- Document preview within the Inventory Workspace

### Reporting & Audit
- Inventory reports
- Project parts lists
- Project transaction reports
- Borrowed-item reports
- Audit and transaction history
- Storage reports
- Executive dashboard reporting
- CSV export
- Print preview and printing
- Saved report templates
- Report column selection

### Repository Management
The v3.x architecture introduces a configurable data repository that centralizes application data and supporting files, including:

- Database
- Attachments
- Photos
- Reports
- Logs
- Backups
- Configuration
- Temporary working data

Repository health, verification/repair, backup, and path management are exposed through the application administration workflow.

## v3.1.3 Highlights

The current v3.1.3 source build includes:

- Redesigned Inventory Workspace
- Search and quick filters
- Item and procurement summaries
- Document preview
- Inventory column chooser with saved workstation preferences
- Right-click inventory actions for faster user workflow

## Build

ForgeWorks is a Visual Studio-compatible **.NET Framework 4.8 WinForms** application.

Open `SOACSForgeWorks.csproj` in a compatible Visual Studio installation and build using the desired Debug or Release configuration.

The application is designed to remain usable in offline environments and does not depend on cloud services for its core workflow.

## Documentation

The repository includes operational and engineering documentation:

- [`OPERATOR_GUIDE.md`](OPERATOR_GUIDE.md) — operator workflow and application use
- [`ADMIN_GUIDE.md`](ADMIN_GUIDE.md) — administration and configuration
- [`DATABASE.md`](DATABASE.md) — database information
- [`CHANGELOG.md`](CHANGELOG.md) — development history
- [`WHATS_NEW.md`](WHATS_NEW.md) — current feature highlights
- [`KNOWN_ISSUES.md`](KNOWN_ISSUES.md) — known issues
- [`ROADMAP.md`](ROADMAP.md) — planned development
- [`TEST_PLAN.md`](TEST_PLAN.md) — testing framework

## Repository Workflow

- `main` — stable fielded/user-test baseline
- `develop` — integrated development
- `feature/<description>` — isolated feature work

Changes intended for the stable baseline should be tested on `develop` and promoted to `main` through a pull request.

## Development Approach

ForgeWorks is developed through short feedback loops with the people using the application. Working builds are placed in front of users, friction points are identified from the real workflow, and capability is iterated based on that feedback.

## Release Note

A GitHub release was initially created as `v3.0.1` based on an older README label. Review of the current source confirmed that the repository snapshot is **v3.1.3**. The release/tag should be normalized to v3.1.3 so GitHub release metadata matches the actual source baseline.

---

**Engineering the Warfighter's Advantage.**
