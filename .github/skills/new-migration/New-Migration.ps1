<#
.SYNOPSIS
    Creates a new SQL Server database migration for the cmi-lesesaal project.

.DESCRIPTION
    Automates the three-step migration workflow:
      1. Creates the SQL script file in CMI/Access/Sql.Lesesaal/SqlDbScripts/
      2. Registers it as an EmbeddedResource in CMI.Access.Sql.Lesesaal.csproj
      3. Increments sollVersion in DbUpgrader.cs

.PARAMETER Description
    Optional description included as a comment at the top of the SQL file.

.EXAMPLE
    .\New-Migration.ps1 -Description "Add NotificationSentAt column to Orders table"
#>
param(
    [Parameter(Mandatory = $false)]
    [string]$Description = ""
)

$ErrorActionPreference = "Stop"

# Locate repo root
$repoRoot = git -C $PSScriptRoot rev-parse --show-toplevel 2>$null
if (-not $repoRoot) {
    Write-Error "Not inside a git repository. Run this script from within the cmi-lesesaal repo."
    exit 1
}

# Paths
$dbUpgraderPath  = Join-Path $repoRoot "CMI\Access\Sql.Lesesaal\DbUpgrader.cs"
$sqlScriptsDir   = Join-Path $repoRoot "CMI\Access\Sql.Lesesaal\SqlDbScripts"
$csprojPath      = Join-Path $repoRoot "CMI\Access\Sql.Lesesaal\CMI.Access.Sql.Lesesaal.csproj"

foreach ($p in @($dbUpgraderPath, $sqlScriptsDir, $csprojPath)) {
    if (-not (Test-Path $p)) {
        Write-Error "Expected path not found: $p`nAre you running this from the cmi-lesesaal repository?"
        exit 1
    }
}

# Read current sollVersion
$upgraderContent = Get-Content $dbUpgraderPath -Raw
if ($upgraderContent -notmatch 'private readonly int sollVersion = (\d+);') {
    Write-Error "Could not find 'sollVersion' in $dbUpgraderPath"
    exit 1
}
$currentVersion = [int]$Matches[1]
$newVersion     = $currentVersion + 1
$oldPadded      = $currentVersion.ToString("D4")
$newPadded      = $newVersion.ToString("D4")
$scriptName     = "${oldPadded}_TO_${newPadded}.sql"
$scriptPath     = Join-Path $sqlScriptsDir $scriptName

# Guard: script must not already exist
if (Test-Path $scriptPath) {
    Write-Error "Migration script already exists: $scriptPath`nIncrement sollVersion manually if you need to create another one."
    exit 1
}

# Step 1: Create the SQL file
$header = "-- Migration: $oldPadded -> $newPadded"
if ($Description) { $header += "`n-- $Description" }
$header += "`n-- Created: $(Get-Date -Format 'yyyy-MM-dd')"

$sqlTemplate = @"
$header

-- Replace this comment with your SQL statements.
-- Use GO as a batch separator between statements.
-- Write statements idempotently where possible.
--
-- Example:
--   IF NOT EXISTS (
--       SELECT 1 FROM sys.columns
--       WHERE object_id = OBJECT_ID('dbo.YourTable') AND name = 'YourColumn'
--   )
--   BEGIN
--       ALTER TABLE dbo.YourTable ADD YourColumn NVARCHAR(200) NULL
--   END
--   GO

"@

[System.IO.File]::WriteAllText($scriptPath, $sqlTemplate, [System.Text.Encoding]::UTF8)
Write-Host "Created SQL script:   $scriptPath"

# Step 2: Register as EmbeddedResource in .csproj
$csprojContent = [System.IO.File]::ReadAllText($csprojPath, [System.Text.Encoding]::UTF8)

# Each existing script is in its own <ItemGroup>. Insert a new <ItemGroup> after
# the closing </ItemGroup> of the last SqlDbScripts entry.
$lastSqlEntry  = [regex]::Match(
    $csprojContent,
    '(?s)<ItemGroup>\s*<EmbeddedResource Include="SqlDbScripts\\[^"]+"\s*/>\s*</ItemGroup>(?!.*<EmbeddedResource Include="SqlDbScripts\\)',
    [System.Text.RegularExpressions.RegexOptions]::Singleline
)

if (-not $lastSqlEntry.Success) {
    Write-Warning "Could not find the last SqlDbScripts EmbeddedResource block in $csprojPath"
    Write-Warning "Please manually add the following line inside an <ItemGroup>:"
    Write-Warning "  <EmbeddedResource Include=`"SqlDbScripts\$scriptName`" />"
} else {
    $insertAt    = $lastSqlEntry.Index + $lastSqlEntry.Length
    $newItemGroup = "`r`n  <ItemGroup>`r`n    <EmbeddedResource Include=`"SqlDbScripts\$scriptName`" />`r`n  </ItemGroup>"
    $updatedCsproj = $csprojContent.Insert($insertAt, $newItemGroup)
    [System.IO.File]::WriteAllText($csprojPath, $updatedCsproj, [System.Text.Encoding]::UTF8)
    Write-Host "Registered in csproj: $csprojPath"
}

# Step 3: Increment sollVersion in DbUpgrader.cs
$updatedUpgrader = $upgraderContent -replace `
    'private readonly int sollVersion = \d+;', `
    "private readonly int sollVersion = $newVersion;"

[System.IO.File]::WriteAllText($dbUpgraderPath, $updatedUpgrader, [System.Text.Encoding]::UTF8)
Write-Host "Updated sollVersion:  $currentVersion -> $newVersion in DbUpgrader.cs"

# Summary
Write-Host ""
Write-Host "Migration $scriptName scaffolded successfully."
Write-Host ""
Write-Host "Next steps:"
Write-Host "  1. Edit the SQL file and add your schema changes:"
Write-Host "     $scriptPath"
Write-Host "  2. Build the solution to verify the embedded resource compiles:"
Write-Host "     msbuild cmi-lesesaal.sln /p:Configuration=Release"
Write-Host "  3. Test the migration against a dev database before committing."