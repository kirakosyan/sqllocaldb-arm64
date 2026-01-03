# SQL Server LocalDB on Windows ARM (WoA): why `(localdb)\…` fails and how to connect reliably

This repo is a small **.NET console** sample that demonstrates a common issue on **Windows 11 on ARM (Surface / Snapdragon / WoA)**:

- **SSMS can connect** to LocalDB using `(localdb)\MSSQLLocalDB` or by selecting the instance.
- Your **ARM64 .NET app may fail** to connect using `(localdb)\MSSQLLocalDB`.
- The same app **can connect using the named pipe**, e.g. `np:\\.\pipe\LOCALDB#...\tsql\query`.

The catch: the `LOCALDB#...` part of the pipe name **changes when LocalDB restarts**, so you **can’t hardcode** it in `appsettings.local.json` and forget it.

This guide shows:
1) **What’s happening**
2) **How to confirm it with `sqllocaldb`**
3) **A practical workaround**: resolve the pipe dynamically at runtime (ARM-only)

---

## Why `(localdb)\MSSQLLocalDB` fails in ARM64 processes

`(localdb)\MSSQLLocalDB` is not a “real network address”. It’s a shortcut that the SQL client expands by calling LocalDB instance resolution components to discover the **current named pipe**.

On Windows on ARM:
- LocalDB is typically **x64 under emulation**.
- Some ARM64 client stacks can’t perform the LocalDB “instance name → pipe” resolution (architecture mismatch).
- Result: `(localdb)\MSSQLLocalDB` fails, but direct pipe connections work.

SSMS often works because it’s commonly running as an **x64** process and can resolve `(localdb)` successfully.

---

## Prerequisites

- Windows 11 on ARM
- SQL Server Express **LocalDB** installed (often via Visual Studio Installer → “Data storage and processing” / LocalDB)
- .NET 10 SDK

---

## Quick start

### 1) Verify LocalDB is available
Open PowerShell:

```powershell
sqllocaldb versions
sqllocaldb info


## Installing SQL Server Express LocalDB (if it’s not available)

If `sqllocaldb` isn’t found (or `sqllocaldb versions` returns nothing), LocalDB is probably not installed.

### 1) Confirm it’s missing

Open PowerShell:

create and start the default instance
sqllocaldb create MSSQLLocalDB
sqllocaldb start MSSQLLocalDB
sqllocaldb info MSSQLLocalDB