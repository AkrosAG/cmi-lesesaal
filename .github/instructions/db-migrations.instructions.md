---
applyTo: "CMI/Access/Sql.Lesesaal/**"
---

# Database Migrations - Rules & Workflow

Applies when working in `CMI/Access/Sql.Lesesaal/` or when any schema change is required.

## How migrations work

Schema migrations run **automatically at application startup**. The flow is:

1. `CMI.Web.Management` `Startup.cs` calls `UpgradeDb()` before handling any request
2. `DbUpgrader` (in `CMI.Access.Sql.Lesesaal`) reads `VERSION.DbVersion` from the SQL Server database
3. It runs every embedded SQL script in order from `istVersion + 1` up to `sollVersion`
4. After each script succeeds, it updates `VERSION.DbVersion` to the new version number
5. On failure, it sets `DbVersion = 9999` to block further startup attempts until the problem is fixed

Scripts are **embedded resources** inside `CMI.Access.Sql.Lesesaal.dll`, located at:
```
CMI/Access/Sql.Lesesaal/SqlDbScripts/
```

The current target version (`sollVersion`) is hardcoded in `CMI/Access/Sql.Lesesaal/DbUpgrader.cs` and is currently `99`.

## Script naming convention

```
NNNN_TO_MMMM.sql
```

Where `NNNN` is the current version and `MMMM = NNNN + 1`. Examples:
```
0098_TO_0099.sql   <- most recent
0099_TO_0100.sql   <- what the next migration would be named
```

## Adding a new migration - automated (recommended)

Use the `new-migration` skill to scaffold all three steps automatically:

```powershell
# From the repo root - Copilot will run this via the skill
& ".github/skills/new-migration/New-Migration.ps1" -Description "Your description here"
```

This creates the SQL file, registers it in the `.csproj`, and increments `sollVersion` in one step.
Then edit the generated SQL file to add your actual schema changes.

## Adding a new migration - complete checklist (manual)

1. **Find the current `sollVersion`** in `CMI/Access/Sql.Lesesaal/DbUpgrader.cs`
2. **Create the SQL script** at `CMI/Access/Sql.Lesesaal/SqlDbScripts/NNNN_TO_MMMM.sql`
   - `NNNN` = current `sollVersion`
   - `MMMM` = `sollVersion + 1`
3. **Set Build Action to `Embedded Resource`** in `CMI/Access/Sql.Lesesaal/CMI.Access.Sql.Lesesaal.csproj`:
   ```xml
   <EmbeddedResource Include="SqlDbScripts\NNNN_TO_MMMM.sql" />
   ```
4. **Increment `sollVersion`** in `DbUpgrader.cs` to match `MMMM`
5. **Do NOT** add `UPDATE VERSION SET DbVersion = MMMM` at the end of your script - `DbUpgrader` does this automatically after the script completes successfully

## SQL script rules

- Use `GO` as a batch separator between statements - `DbUpgrader` splits on it automatically
- The script runs inside a single `SqlConnection`; each batch separated by `GO` is executed as a separate `ExecuteNonQuery`
- Write scripts to be **idempotent** where practical (use `IF NOT EXISTS`, `IF OBJECT_ID(...) IS NULL`, etc.)
- Test the script manually against a dev database before committing

## Example script structure

```sql
-- Add a new column to an existing table
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Orders') AND name = 'NewColumn'
)
BEGIN
    ALTER TABLE dbo.Orders ADD NewColumn NVARCHAR(200) NULL
END
GO

-- Create a new table
IF OBJECT_ID('dbo.NewTable', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.NewTable (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    )
END
GO
```

## Version error states

| `DbVersion` value | Meaning |
|-------------------|---------|
| 0 | Empty database - initial setup, `0000_TO_0001.sql` will run first |
| 1-99 | Normal; shows current schema version |
| > `sollVersion` | **Error** - database is newer than the deployed application; startup is blocked |
| 9999 | **Error** - a previous migration failed mid-way; manual intervention required |

If `DbVersion = 9999`, identify the failed migration from the application logs, fix the script or database state manually, then reset `DbVersion` to the last known-good version.

## Database scope

There is no separate Oracle database in cmi-lesesaal. All application schema changes are managed through `DbUpgrader` in `CMI.Access.Sql.Lesesaal`.