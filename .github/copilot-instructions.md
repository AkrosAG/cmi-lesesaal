# CMI Lesesaal

CMI Lesesaal is the reading room management system by **CM Informatik AG**, **Evelix GmbH**, and **Akros AG**. The solution combines a .NET Framework backend (IDesign-style SOA with MassTransit/RabbitMQ) with Angular clients for the public reading-room portal and the internal management application.

Repository documentation lives in `readme.md` and `docs/`, especially:
- `docs/architecture.md`
- `docs/requirements.md`
- `docs/connection-ais.md`
- `docs/connection-dir.md`

## Domain glossary

| Term | Meaning |
|---|---|
| **Lesesaal** | Reading room / reading room management context |
| **Bestellung** | Order or consultation request submitted by a user |
| **Konsultationsgesuch** | Consultation request for archival material |
| **AIS** | Archivinformationssystem — master system for archival metadata |
| **DIR** | Digital Information Repository — master system for digital primary data |
| **Gebrauchskopie** | Usage copy prepared for consultation or download |
| **RabbitMQ** | Message bus used by MassTransit for asynchronous communication |

## Scoped instruction files

These files are loaded automatically when you edit matching files:

| File | Applies to |
|------|-----------|
| `.github/instructions/dotnet-backend.instructions.md` | `CMI/**/*.cs` — IDesign rules, naming, smells, scaffolding |
| `.github/instructions/angular.instructions.md` | `CMI/Web.Clients/**` — Angular conventions and web-core workflow |
| `.github/instructions/db-migrations.instructions.md` | `CMI/Access/Sql.Lesesaal/**` — SQL Server migration workflow and checklist |
| `.github/instructions/contracts.instructions.md` | `CMI/Contract/**/*.cs` — contract design, operation counts, factoring |

## Architecture — core guardrails

The backend follows **volatility-based decomposition** (IDesign Method). Independent axes of change belong in separate components. Do not decompose by functionality or entity names alone.

### Layers

```
CMI.Web.*       ← Client     — who interacts; calls one Manager per use case
CMI.Manager.*   ← Manager    — what sequence to run; orchestrates Engines + Access
CMI.Engine.*    ← Engine     — how a business activity is performed
CMI.Access.*    ← Access     — how to access a resource
CMI.Utilities.* ← Utilities  — cross-cutting infrastructure
CMI.Contract.*  ← Contracts  — public interfaces + DTOs across subsystem boundaries
CMI.Host.*      ← Host       — Windows Service wrapper for each Manager
```

### Practical note

The repository has grown over multiple years and not every area adheres perfectly to the intended architecture. Treat deviations in existing code as technical debt, not as patterns to copy into new code.

### Rules that must not be broken

1. **Clients call only Managers**
2. **Managers do not call other Managers synchronously** — use asynchronous messaging
3. **Only primitives and plain DTOs cross layer boundaries**

For the full backend rules and review checklist, see `.github/instructions/dotnet-backend.instructions.md`.

## Build commands

### .NET (repo root)
```powershell
nuget restore cmi-lesesaal.sln
msbuild cmi-lesesaal.sln /t:Rebuild /p:Configuration=Release
```

### Angular (from each client directory)
```bash
npm ci --legacy-peer-deps
npm run build
npm run build-prod
```

Build order: `web-core` must be built before `web-frontend` or `web-management`.

## Test commands

### .NET
```powershell
$testAssemblies = Get-ChildItem -Recurse "Test" -Filter "*.Tests.dll" |
    Where-Object { $_.FullName -like "*\bin\Release\*" -and $_.FullName -notlike "*\obj\*" -and $_.FullName -notlike "*.Performance.Tests.dll" } |
    Select-Object -ExpandProperty FullName
& "packages\NUnit.ConsoleRunner.3.16.0\tools\nunit3-console.exe" $testAssemblies
```

### Angular
```bash
npm run test-github
npm run test
```

## Database migrations

SQL Server schema migrations are managed by `CMI.Access.Sql.Lesesaal`. Scripts are embedded resources named `NNNN_TO_MMMM.sql`, and `DbUpgrader.cs` currently targets schema version `99` with scripts up to `0098_TO_0099.sql`. Full workflow: `.github/instructions/db-migrations.instructions.md`.

## Repository documentation

- `readme.md`
- `docs/architecture.md`
- `docs/requirements.md`
- `docs/connection-ais.md`
- `docs/connection-dir.md`

## Git workflow

- Use feature branches and open a Pull Request into `develop`
- CI for the main repository flow is defined in `.github/workflows/lesesaal-full-ci.yml`
- Do not commit anything that does not build
- Branch naming: `feature/<short-description>`

## Autonomous execution policy

Execute immediately without asking for confirmation for routine repository-scoped work inside `C:\DEV\CMInformatik\cmi-lesesaal\`, including:

- Builds such as `nuget restore cmi-lesesaal.sln` and `msbuild cmi-lesesaal.sln /t:Rebuild /p:Configuration=Release`
- Tests such as `packages\NUnit.ConsoleRunner.3.16.0\tools\nunit3-console.exe` and Angular test commands
- Linting and npm build commands in `CMI\Web.Clients\`
- Reading, creating, editing, or deleting source files inside the repository
- Non-destructive `git` operations on feature branches
- `gh` read operations and GitHub issue / pull request inspection
- Repository scripts such as `.github/skills/new-migration/New-Migration.ps1`

Always ask before:

- Merging pull requests or pushing to shared branches such as `develop`
- Any force-push
- Bulk deletion of files or directories
- Running commands outside the repository root
- System-level configuration changes
- Destructive database operations that are not reversible by a migration
- Writing secrets or credentials into files

## Issue tracking

Use **GitHub Issues** in the repository: <https://github.com/AkrosAG/cmi-lesesaal>.

When a task refers to an issue, use the GitHub issue number and repository context instead of external trackers. Do not assume ticket prefixes such as `PVW-XXXX`.