---
name: new-migration
description: >
  Creates a new SQL Server database migration for the cmi-lesesaal project.
  Use this skill when asked to add a database migration, create a new migration,
  alter the database schema, or add/modify tables, columns, indexes or constraints
  in the cmi-lesesaal SQL Server database.
allowed-tools: shell
---

# new-migration skill

Use this skill to add a new SQL Server schema migration to the cmi-lesesaal project.

## What this skill does

Runs `New-Migration.ps1` from this skill's directory. The script:

1. Reads the current `sollVersion` from `CMI/Access/Sql.Lesesaal/DbUpgrader.cs`
2. Creates a new empty SQL script at `CMI/Access/Sql.Lesesaal/SqlDbScripts/NNNN_TO_MMMM.sql`
3. Registers it as an `<EmbeddedResource>` in `CMI.Access.Sql.Lesesaal.csproj`
4. Increments `sollVersion` in `DbUpgrader.cs`

## How to invoke

Run the script from the repository root:

```powershell
& ".github/skills/new-migration/New-Migration.ps1" -Description "Your migration description"
```

The `-Description` parameter is optional but recommended - it becomes a comment at the top of the generated SQL file.

## After running the script

1. Open the generated SQL file (`CMI/Access/Sql.Lesesaal/SqlDbScripts/NNNN_TO_MMMM.sql`)
2. Replace the placeholder comment with the actual SQL statements
3. Use `GO` as a batch separator between statements
4. Write statements **idempotently** where practical - use `IF NOT EXISTS`, `IF OBJECT_ID(...) IS NULL`, etc.
5. Verify the new `sollVersion` in `DbUpgrader.cs` matches `MMMM`
6. Build the solution to confirm the embedded resource is picked up correctly

## Rules to follow

- Never add `UPDATE VERSION SET DbVersion = ...` - `DbUpgrader` handles this automatically
- Each SQL script runs in a single `SqlConnection`; each `GO`-separated block is a separate `ExecuteNonQuery`
- Script names must follow the exact pattern: `NNNN_TO_MMMM.sql` (zero-padded to 4 digits)
- If the script fails mid-way at runtime, `DbVersion` is set to `9999` - manual intervention is needed to recover