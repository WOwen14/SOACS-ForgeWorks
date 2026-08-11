# Contributing to SOACS ForgeWorks

ForgeWorks is maintained as a mission-focused application with a controlled stable baseline and an active development branch.

## Branches

- `main` — stable fielded / user-test baseline
- `develop` — integrated development
- `feature/<description>` — isolated feature or change work
- `fix/<description>` — isolated defect correction

Do not develop new features directly on `main`.

## Recommended Workflow

1. Start from the latest `develop` branch.
2. Create a focused `feature/` or `fix/` branch.
3. Make the smallest practical change that solves the requirement.
4. Build and test the application locally.
5. Verify existing inventory, project, kit, reporting, repository, and database workflows are not unintentionally affected.
6. Merge the completed work into `develop` through a pull request.
7. Promote a tested baseline from `develop` to `main` through a separate pull request.

## Development Environment

ForgeWorks is a Windows Forms application targeting **.NET Framework 4.8**.

Primary project:

`SOACSForgeWorks.csproj`

The application is designed to support disconnected environments. Avoid introducing cloud, internet, or package-manager dependencies unless the requirement explicitly calls for them and an offline deployment method is documented.

## Data Safety

Never commit operational or user-generated ForgeWorks data.

This includes:

- SQLite database files
- WAL/SHM database sidecars
- attachments
- photos
- generated reports
- logs
- backups
- workstation configuration
- repository profiles
- exported operational data

Use sanitized test data when a test fixture is required.

## Versioning

Keep version information synchronized across:

- project/application metadata
- About page
- `CHANGELOG.md`
- `WHATS_NEW.md`
- README status
- GitHub release tag/title

The current source snapshot is **v3.1.3**. Older source locations still contain historical version strings and should be normalized as future code maintenance is performed.

## Pull Requests

A pull request should clearly identify:

- the operational problem or defect being addressed
- the files/workflows affected
- test steps performed
- known limitations or follow-on work
- whether database/schema behavior changes

Changes affecting inventory quantities, transactions, project material, kits, repository paths, backups, or database structure require additional regression testing because they affect persistent operational data.

## Documentation

Update the appropriate documentation when behavior changes:

- `CHANGELOG.md`
- `WHATS_NEW.md`
- `OPERATOR_GUIDE.md`
- `ADMIN_GUIDE.md`
- `DATABASE.md`
- `KNOWN_ISSUES.md`
- `TEST_PLAN.md`

## Release Promotion

A release candidate should be validated on `develop` before promotion to `main`. Once merged to `main`, create a GitHub release whose tag and title match the actual application/source version.
