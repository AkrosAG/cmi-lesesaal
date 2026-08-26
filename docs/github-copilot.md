# GitHub Copilot in cmi-lesesaal

## Two tools, two different purposes

GitHub Copilot comes in two forms you will encounter when working on this project. Understanding the difference helps you choose the right one for each task.

---

## Copilot Chat (inside Visual Studio)

Copilot Chat is embedded in the IDE and is tightly integrated with your open solution.

**What it is good at:**
- Completing, explaining, or refactoring the code you are currently looking at
- Answering questions about a selected method, class, or error message
- Generating boilerplate from a description while you have the right file open
- Inline ghost-text suggestions as you type

**Its limitations:**
- It only sees what Visual Studio hands it (the active file, selected code, compiler errors)
- It cannot run commands, build, or test
- It cannot search across the whole repository on its own
- It has no persistent memory between sessions

---

## GitHub Copilot CLI (terminal / PowerShell window)

The CLI is a fully agentic tool you run from a terminal. It can operate on the entire repository, not just the file you have open.

**What it is good at:**
- Reading, searching, and editing files across the whole codebase
- Running real commands: `msbuild`, `nuget`, `npm`, `git`, `gh`, PowerShell scripts, tests
- Completing multi-step engineering tasks end-to-end (e.g. "add a DB migration and wire it up")
- Creating and reviewing pull requests
- Deep research using GitHub search and the web
- Running parallel sub-agents for large tasks
- Connecting to external tools via MCP servers

**How it reads project context:**  
The CLI automatically reads `.github/copilot-instructions.md` at the start of every session. That file contains the architecture rules, build commands, conventions, and other project-specific context for this repository — keep it up to date.

---

## Choosing the right tool

| Task | Tool |
|------|------|
| Write or complete a method | Copilot Chat in VS |
| Explain a class or a compiler error | Copilot Chat in VS |
| Quick inline code completion while typing | VS inline suggestions |
| Add a DB migration and update `sollVersion` | Copilot CLI |
| Refactor a service to comply with IDesign rules | Copilot CLI |
| Build the solution, run tests | Copilot CLI |
| Create or review a pull request | Copilot CLI |
| Multi-file feature spanning backend and Angular | Copilot CLI |

---

## Getting started with the CLI

Install and launch from any terminal in the repository root:

```powershell
# Launch
copilot
```

Useful first commands once inside:

```
/help          — show all available commands
/instructions  — view which instruction files are loaded
/model         — switch AI model
/review        — run a code review on current changes
```

The project instructions file is at `.github/copilot-instructions.md`. It is read automatically — you do not need to paste it in manually.

---

## YouTrack MCP integration

With the MCP server configured, Copilot can read and update YouTrack issues directly. This means you can say:

```
Implement DLS-123 following our IDesign rules
```

...and Copilot will fetch the issue, read the acceptance criteria, and start coding.

### Example prompts once active

```
Read DLS-123 and tell me what needs to be implemented
Implement the acceptance criteria from DLS-123
Create a branch and PR for DLS-123
Mark DLS-123 as In Progress
Add a comment to DLS-123: "Implementation started, PR coming today"
Log 3 hours on DLS-123
```

### How it works

The YouTrack MCP server is registered **per developer machine** in `~/.copilot/mcp-config.json`. There is nothing to commit to the repository — each developer runs a one-time setup command.

### One-time setup (per developer)

**Step 1 — Get a YouTrack permanent token**

1. Log into https://cmiag.myjetbrains.com/youtrack → click your avatar → **Profile**
2. **Account Security** tab → **Tokens** → **New token**
3. Name it `copilot-mcp`, grant the `YouTrack` scope
4. Copy the token (it starts with `perm:`)

**Step 2 — Register the MCP server**

Run this once in your terminal (outside Copilot, replace the token placeholder):

```powershell
copilot mcp add youtrack `
  --env YOUTRACK_URL=https://cmiag.myjetbrains.com/youtrack `
  --env YOUTRACK_TOKEN=perm:your-token-here `
  -- npx -y @promtior/youtrack-mcp-extended
```

This writes the configuration to `~/.copilot/mcp-config.json` — your token never touches the repository.

**Step 3 — Verify**

Start Copilot CLI and run `/mcp` — `youtrack` should appear as connected.

> **Note:** Each developer configures this individually. The token is personal and must not be committed to the repository.
