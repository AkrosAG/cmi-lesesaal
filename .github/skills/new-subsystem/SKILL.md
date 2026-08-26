---
name: new-subsystem
description: >
  Scaffolds a new IDesign-compliant subsystem for the Viaduc .NET backend.
  Use this skill when asked to create a new subsystem, add a new service, or
  add a new domain area following the IDesign Method layered architecture.
  It creates the Contract, Access, Manager, and Host projects (and optionally
  an Engine project), with correct namespaces, stub classes, and project files.
allowed-tools: shell
---

# new-subsystem skill

Scaffolds all the boilerplate for a new IDesign subsystem: directories, `.csproj`
files, stub C# classes, and `Properties/AssemblyInfo.cs` for each layer.

## What this skill does

Runs `New-Subsystem.ps1` from this skill's directory. The script creates:

| Layer | Project | What is generated |
|-------|---------|-------------------|
| Contract | `CMI.Contract.<Subsystem>` | `I<Subsystem>Manager.cs` interface stub |
| Access | `CMI.Access.<Subsystem>` | `<Subsystem>Access.cs` class stub |
| Engine *(optional)* | `CMI.Engine.<Subsystem>` | Placeholder engine class (rename with gerund prefix) |
| Manager | `CMI.Manager.<Subsystem>` | `<Subsystem>Manager.cs`, `<Subsystem>Service.cs`, `Infrastructure/ContainerConfigurator.cs` |
| Host | `CMI.Host.<Subsystem>` | `Program.cs` (Topshelf bootstrap) |

## How to invoke

```powershell
# Minimum — Contract + Access + Manager + Host
& ".github/skills/new-subsystem/New-Subsystem.ps1" -Subsystem "MySubsystem"

# With Engine project
& ".github/skills/new-subsystem/New-Subsystem.ps1" -Subsystem "MySubsystem" -IncludeEngine
```

## After running the script

The script creates all files on disk but **cannot add projects to the Visual Studio solution** — that step must be done manually.

### Required manual steps

1. **Add all generated projects to `Viaduc.sln`** in Visual Studio:
   - Right-click the appropriate solution folder → *Add → Existing Project*
   - Add them in this order: Contract → Access → Engine (if created) → Manager → Host
   - Place them in the matching solution folders (Contract, Access, Engine, Manager, Host)

2. **Register the new subsystem** in `CMI.Contract.Monitoring` if it should appear in the monitoring dashboard (`MonitoredServices` enum)

3. **Add MassTransit queue constants** in `CMI.Contract.Messaging` (`BusConstants`) for each message queue the Manager will consume

4. **Wire up Consumers** in `ContainerConfigurator.cs` and register their queues in `<Subsystem>Service.cs`

5. **Add the Host project** to the build server and deployment pipeline

6. **Write the actual business logic** — replace stubs with real implementations

## IDesign rules enforced by this scaffold

- `Contract` contains only the public interface — no business logic, no Access/Engine references
- `Access` references `Contract` but is **not referenced** by `Contract`
- `Engine` (if used) references `Access` but is **not referenced** by `Contract` or `Access`
- `Manager` references `Contract` + `Access` + `Engine` (if present) — never the reverse
- `Host` references only `Manager` — it is a thin bootstrap wrapper
- No project references cross subsystem boundaries except via `CMI.Contract.*`
