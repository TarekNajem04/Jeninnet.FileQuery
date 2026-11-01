# VS Code Extension

The `extensions/vscode` folder contains the **Jeninnet FileQuery** extension for Visual Studio
Code. It drives the [`dotnet-filequery`](global-tool.md) tool from the editor so you can test
patterns against the current workspace and see why files match or fail.

The extension has no runtime npm dependencies: it is plain JavaScript and calls the global tool,
so the engine version always matches the tool you have installed.

---

## Installation

1. Install the global tool:

   ```bash
   dotnet tool install --global Jeninnet.FileQuery.Tool
   ```

2. Package and install the extension:

   ```bash
   cd extensions/vscode
   npx @vscode/vsce package
   code --install-extension jeninnet-filequery-0.1.0.vsix
   ```

   During development, open the folder in VS Code and press `F5` (Extension Development Host)
   instead of packaging.

---

## Commands

| Command | What it does |
|---|---|
| `Jeninnet: Test File Pattern` | Prompts for a pattern and lists the matching workspace files (relative paths). |
| `Jeninnet: Explain File Pattern` | Prompts for a pattern and lists **every** evaluated file as `[include]` or `[exclude]`, with the reason reported by the engine. |

Both commands ask which workspace folder to query when more than one is open. Patterns use the
engine's GitIgnore rule order, so start with `**` to ignore everything and re-include with `!`:

```text
**;!*.cs
```

An empty pattern evaluates every file. Multiple patterns are separated with `;`.

The explain output is produced by `dotnet-filequery --explain`; it shows the final outcome and
the responsible pattern kind (for example, `Include by GitIgnore pattern.`).

---

## Settings

| Setting | Default | Description |
|---|---|---|
| `jeninnet.executable` | `dotnet-filequery` | Command used to run the tool. Point it at a custom shim when the tool is not on `PATH`. |
| `jeninnet.defaultPattern` | `!**` | Pattern prefilled in the prompt. |
| `jeninnet.ignoreCase` | `false` | Match patterns without case sensitivity. |

---

## Behaviour and limits

- The extension runs the tool once per invocation; it is on-demand feedback rather than a
  per-keystroke loop. Large workspaces report progress in the status bar and write results to the
  **Jeninnet FileQuery** output channel.
- Standard output is capped at 16 MB and each run times out after two minutes.
- Missing executable errors include the exact `dotnet tool install` command to fix them.
- The tests cover argument construction and output parsing (`node --test "test/*.test.js"`); the
  editor integration is verified manually in the Extension Development Host.

---

## Packaging and publishing

```bash
npx @vscode/vsce package                     # jeninnet-filequery-0.1.0.vsix
npx @vscode/vsce publish --packagePath jeninnet-filequery-0.1.0.vsix
```

Publishing uses the `jeninnet` Marketplace publisher and the `VSCE_PAT` environment variable for
the access token. The extension version is independent of the NuGet package version.

---

## Visual Studio

This release ships the VS Code extension. The Visual Studio extension is a follow-up: it needs a
different packaging and debugging model (VSIX with `Microsoft.VisualStudio.Shell`). The shared
logic in `src/runner.js` is editor-agnostic (argument construction and output parsing), so a
Visual Studio front end can reuse the same contract.

---

## See also

- [Global Tool](global-tool.md) — the CLI reference.
- [Diff-Aware Queries](diff-aware-queries.md) — restricting a query to changed files.
