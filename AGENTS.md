# CMI Lesesaal — Agent Instructions

This file provides tool-agnostic instructions for any autonomous agent (GitHub Copilot, Claude Code, OpenCode, Jules, etc.) working in this repository.

## Project overview

cmi-lesesaal is the reading room management system developed by CM Informatik AG, Evelix GmbH, and Akros AG. It combines a .NET Framework SOA backend (C#, MassTransit message bus) with Angular frontend clients. The backend follows the **IDesign Method** — see `.github/copilot-instructions.md` for the full architecture rules.

## Before making any changes

1. Read `.github/copilot-instructions.md` — it contains the architecture rules, naming conventions, and guardrails that all code must follow
2. For C# changes, also read `.github/instructions/dotnet-backend.instructions.md`
3. For Angular changes, also read `.github/instructions/angular.instructions.md`
4. For DB schema changes, also read `.github/instructions/db-migrations.instructions.md`
5. For contract changes, also read `.github/instructions/contracts.instructions.md`

## Build commands

### .NET backend (run from repo root)
```powershell
nuget restore cmi-lesesaal.sln
msbuild cmi-lesesaal.sln /t:Rebuild /p:Configuration=Release
```

### Angular clients (run from the relevant client directory)
```bash
# web-core must be built first
cd CMI/Web.Clients/web-core && npm ci --legacy-peer-deps && npm run build

# then web-frontend or web-management
cd CMI/Web.Clients/web-frontend && npm ci --legacy-peer-deps && npm run build
cd CMI/Web.Clients/web-management && npm ci --legacy-peer-deps && npm run build
```

## Test commands

### .NET tests
```powershell
$testAssemblies = Get-ChildItem -Recurse "Test" -Filter "*.Tests.dll" |
    Where-Object { $_.FullName -notlike "*.Performance.Tests.dll" }
& packages\NUnit.ConsoleRunner.3.16.0\tools\nunit3-console.exe $testAssemblies
```

### Angular tests (from the relevant client directory)
```bash
npm run test-github   # single run, ChromeHeadless, with coverage — mirrors CI
npm run test          # interactive watch mode
```

## Git workflow

- All changes go via **feature branches** — never commit directly to `develop` or `master`
- Branch naming: `feature/DLS-XXXX-short-description` (hyphens, no spaces)
- PR title: `feature/DLS-XXXX <short description>` targeting `develop`
- Open a **Pull Request** targeting `develop`; at least one approved review is required before merging
- PRs to `develop` trigger the CI workflow automatically
- **Do not commit anything that does not build**, including lint errors

## Key rules (summary — see instruction files for full detail)

### Architecture (IDesign — non-negotiable)
- Client (`CMI.Web.*`) → Manager only (one per use case)
- Manager → its own Engine(s) and Access (synchronous, in-process)
- Manager → another Manager only via **MassTransit async message** (never direct reference)
- Engine → Access only (no Engine-to-Engine, no Access-to-Access)
- Inter-layer data: primitives and DTOs only — no behaviour-carrying objects

### Database migrations
- New migration = new `NNNN_TO_MMMM.sql` in `CMI/Access/Sql.Lesesaal/SqlDbScripts/`
- Set **Build Action: Embedded Resource** in the `.csproj`
- Increment `sollVersion` in `DbUpgrader.cs` (currently at 99)
- Do NOT add `UPDATE VERSION SET DbVersion` — DbUpgrader does this automatically
- Use the skill: `& ".github/skills/new-migration/New-Migration.ps1" -Description "..."`

## Important file locations

| Purpose | Path |
|---------|------|
| Architecture & guardrails | `.github/copilot-instructions.md` |
| .NET / IDesign detail | `.github/instructions/dotnet-backend.instructions.md` |
| Angular conventions | `.github/instructions/angular.instructions.md` |
| DB migration workflow | `.github/instructions/db-migrations.instructions.md` |
| Contract design rules | `.github/instructions/contracts.instructions.md` |
| SQL migration scripts | `CMI/Access/Sql.Lesesaal/SqlDbScripts/` |
| DB version controller | `CMI/Access/Sql.Lesesaal/DbUpgrader.cs` |
| CI workflow | `.github/workflows/lesesaal-full-ci.yml` |
