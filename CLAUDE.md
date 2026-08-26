# CMI Lesesaal — Claude Code Instructions

## Architecture rules (read first)

@.github/copilot-instructions.md

## Scoped rules (load when relevant)

For C# backend work:
@.github/instructions/dotnet-backend.instructions.md

For Angular frontend work:
@.github/instructions/angular.instructions.md

For database migrations:
@.github/instructions/db-migrations.instructions.md

For contract design:
@.github/instructions/contracts.instructions.md

## Claude-specific notes

- When making architectural decisions in C#, always verify against the IDesign call rules before proposing a solution. Suggest the correct layer for new code before writing it.
- When asked to add a feature, first identify: which Manager owns this use case, which Engine (if any) encapsulates the business logic, which Access component reads/writes the data. State this plan before writing code.
