# SOACS ForgeWorks Test Plan

**Baseline under test:** v3.1.4 RC1  
**Test type:** Manual release-candidate and user-workflow verification

Use synthetic test data only. Do not place operational inventories, customer identifiers, credentials, attachments, photographs, or reports in the public repository.

## Test record

Record the following for each test session:

- Application version
- Date and tester
- Computer and Windows version
- Display resolution and scaling
- Repository profile and repository type: local or shared
- Pass, Fail, or Blocked result
- Defect description and reproduction steps

## 1. Startup and shell

1. Launch the application and confirm the splash screen completes without error.
2. Confirm the application opens maximized and the Dashboard is the initial workspace.
3. Verify the header, navigation, status bar, active repository profile, Last Sync value, and clock are visible.
4. Repeat the shell check at 1920×1080 using 100% and 125% display scaling.
5. Open each navigation workspace and confirm controls do not clip or overlap.

## 2. Inventory and item records

1. Create a synthetic item with nomenclature, part number, NSN, CAGE, MRL, location, and quantity.
2. Add procurement, vendor, manufacturer, lead-time, cost, and reorder information.
3. Save, close, reopen, and verify the item values persist.
4. Confirm global search finds the item by multiple identifiers.
5. Exercise Low Inventory, Borrowed, Needs Reorder, Has Documents, Has Photo, No Vendor, and Out filters.
6. Change visible inventory columns, restart the application, and verify the workstation preference persists.

## 3. Operations

1. Open Operations and confirm Step 1 is visible without automatic scrolling.
2. Receive inventory and verify quantity and transaction history.
3. Issue inventory and verify quantity and transaction history.
4. Borrow inventory to a synthetic project and verify Borrowed By, available quantity, and reorder state.
5. Move inventory and verify the new location.
6. Adjust quantity with a reason and notes and verify the audit entry.
7. Remove inventory and confirm the required confirmation and transaction record.
8. Verify both scanner-assisted and manual-search workflows.

## 4. Projects, reservations, and kits

1. Create a synthetic project with status and priority.
2. Reserve material and verify availability calculations.
3. Create a reusable kit/BOM requirement.
4. Confirm required, on-hand, available, shortage, and readiness values.
5. Verify project parts and project transaction reports.

## 5. Attachments and photos

1. Add multiple synthetic documents to an item.
2. Open, print, remove, and open the attachment folder.
3. Add or capture an item photo and verify its repository path.
4. Confirm attachment and photo metadata persists after restart.

## 6. Reports

1. Generate inventory, low-stock, out-of-stock, borrowed-item, storage, project, transaction, audit, and executive reports.
2. Exercise each available transaction time filter.
3. Hide and show report columns.
4. Save and reload a report template.
5. Export CSV and verify the selected columns and values.
6. Open Print Preview and print to a printer or Microsoft Print to PDF.

## 7. Repository management

1. Verify Production, Test Lab, Standalone, and Offline profiles are available.
2. Switch between synthetic repository profiles and confirm the displayed root and health state change correctly.
3. Run repository Verify/Repair and confirm required folders are present.
4. Create a backup and verify it is written under the active repository.
5. If a shared test repository is available, validate refresh behavior from two authorized test workstations.

## 8. Feedback Center

1. Open Feedback Center and confirm all controls are fully visible at 100% and 125% scaling.
2. Submit synthetic feedback with comments and an application screenshot.
3. Verify version, environment, repository health, and system-information files are included.
4. Confirm the package is saved beneath the active repository's `Feedback` folder.
5. Verify Administration can open the Feedback repository folder.

## Exit criteria

- No unresolved defect blocks startup, inventory transactions, repository access, or data persistence.
- Primary operator workflows complete with accurate transaction and report results.
- No critical controls clip at the supported test resolution and scaling.
- Known limitations are documented and accepted for the release-candidate test cycle.
