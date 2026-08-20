# SOACS ForgeWorks Known Issues

**Current baseline:** v3.1.4 RC1  
**Status:** Live / User testing

## Open limitations

- PDF output is provided through Windows Print Preview / Print to PDF rather than a built-in PDF writer.
- Spreadsheet export is CSV-compatible; native XLSX export is not currently implemented.
- Multi-user shared-database behavior remains under active hardening and should be validated against the intended network repository before broader deployment.
- Automatic offline fallback, synchronization queues, and conflict handling are planned capabilities and are not included in the current baseline.

## Validation focus

The v3.1.4 RC1 user-test cycle is specifically monitoring:

- Layout behavior at 1920×1080 and 125% display scaling
- Shared-repository refresh behavior
- Serialized-item and project-material workflows
- Attachment, photo, backup, and feedback repository paths
- Reporting accuracy after receive, issue, borrow, move, and quantity-adjustment transactions

## Resolved in v3.1.4 RC1

- Operations now opens at Step 1 instead of auto-scrolling to the scanner input.
- Inventory summary values have DPI-safe vertical space.
- Inventory columns use readable starting widths with horizontal scrolling on laptop displays.
- Feedback options and Cancel/Submit actions no longer clip at 125% scaling.
- Status-bar spacing was compacted so Last Sync and the clock remain visible at 1920×1080.
- Feedback submissions are stored under the active repository's `Feedback` folder.

See [CHANGELOG.md](CHANGELOG.md) for the complete version history.
