# SOACS ForgeWorks Administrator Guide
Version: 3.1.4 RC1

## Purpose
This guide supports administrators responsible for configuring, maintaining, backing up, and troubleshooting SOACS ForgeWorks.

## Repository Manager
ForgeWorks stores operational data under a Repository Root. The Repository Manager controls paths for:
- Database
- Attachments
- Photos
- Reports
- Logs
- Backups
- Feedback
- Config
- Temp

## Repository Profiles
Profiles allow ForgeWorks to point at different repository roots such as Production, Test Lab, Standalone, or Offline. Use profiles to separate test data from operational data.

## Feedback Center
Feedback reports are stored under:

`<RepositoryRoot>\Feedback\yyyy-MM-dd_HHmmss_Category`

Each report may include:
- Feedback.xml
- Screenshot.png
- SystemInfo.txt
- RepositoryHealth.txt

Administrators can open the Feedback folder from the Administration workspace.

## Backup Guidance
Back up the full Repository Root, not only the database. Attachments, photos, reports, logs, and feedback all live under the repository.

## Permissions
Operators need read/write permission to the active Repository Root. Read-only viewers may be granted read-only access when write operations are not required.

## Troubleshooting
If documents, photos, reports, or feedback do not save, verify:
1. The active repository profile points to the expected Data Root.
2. The workstation has write permission.
3. Repository Health reports all folders as OK.
4. Disk space is available.
