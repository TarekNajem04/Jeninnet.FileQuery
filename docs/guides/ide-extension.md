# Jeninnet FileQuery for VS Code

The **Jeninnet FileQuery** extension brings the engine's pattern tester into Visual Studio Code. It
drives the [`dotnet-filequery`](global-tool.md) tool against the active workspace, so you can check a
GitIgnore, Glob, or Regex pattern and see exactly why files match or fail without leaving the editor.

The extension is intentionally thin:

- no runtime npm dependencies;
- no editor-specific pattern logic;
- every query runs through the same global tool and compiled engine you use in CI.

---

## Requirements

- VS Code **1.85** or newer.
- The `dotnet-filequery` global tool:

  ```bash
  dotnet tool install --global Jeninnet.FileQuery.Tool
  dotnet-filequery --version
  ```

The extension reports the exact install command when the executable cannot be found.

---

## Install the extension

### From a VSIX

```bash
cd extensions/vscode
npx @vscode/vsce package
code --install-extension jeninnet-filequery-0.1.0.vsix
```

### During development

Open `extensions/vscode` in VS Code and press **F5** to launch the Extension Development Host.

---

## Commands

Open the Command Palette (`Ctrl+Shift+P` / `Cmd+Shift+P`) and run:

| Command | Result |
|---|---|
| **Jeninnet: Test File Pattern** | Lists matching files as workspace-relative paths. |
| **Jeninnet: Explain File Pattern** | Lists every evaluated file as `[include]` or `[exclude]`, with the deciding reason. |

Both commands prompt for a pattern, run the tool once, and write the result to the **Jeninnet
FileQuery** output channel. A status-bar progress indicator appears while the query runs.

In a multi-root workspace, the extension first asks which folder to query.

---

## Pattern model in one minute

Patterns use the engine's GitIgnore rule order: each matching rule changes the outcome, and the last
match wins. The common starting point is:

```text
**              exclude everything
!**/*.cs        re-include C# files at any depth
```

In the extension prompt, separate patterns with `;`:

```text
**;!**/*.cs
```

An empty pattern evaluates every file. Patterns can mix dialects in one query:

```text
**;!**/*.cs;r:^src/.*\.cs$
```

The `r:` prefix marks a Regex pattern. Glob patterns support `*`, `**`, `?`, character classes,
POSIX classes, and `{a,b}` alternation.

---

## Examples

### 1. Find all C# files

```text
**;!**/*.cs
```

**Test File Pattern** output:

```text
Pattern: **;!**/*.cs — 42 match(es)
src/Program.cs
src/Engine/FileQueryEngine.cs
tests/FileQueryEngineTests.cs
...
```

### 2. Find source files but exclude tests

```text
**;!**/*.cs;**/*Tests.cs
```

`**` excludes everything, `!**/*.cs` includes C# files, and `**/*Tests.cs` excludes them again. All
three are GitIgnore rules, so the last matching GitIgnore rule decides.

### 3. Explain why `bin/` is excluded

```text
**;!**/*.cs;bin/
```

**Explain File Pattern** output:

```text
Pattern: **;!**/*.cs;bin/ — 120 file(s), 42 included
[include] src/Program.cs — Include by GitIgnore pattern.
[exclude] bin/App.dll — Exclude by GitIgnore pattern.
[exclude] bin/App.pdb — Exclude by GitIgnore pattern.
```

The reason column comes from `dotnet-filequery --explain`; it names the pattern kind and the rule
that decided the outcome.

### 4. Find image assets case-insensitively

Set `jeninnet.ignoreCase` to `true`, then use:

```text
**;!**/*.{png,jpg,jpeg,svg,webp}
```

`IgnoreCase` also folds character-class literals and ranges, so `[a-z]` matches `A`–`Z`.

### 5. Match a source subtree with Regex

```text
r:^src/.*\.cs$
```

Regex patterns match the full workspace-relative path; add `^` and `$` when you need anchoring.

### 6. Check a single extension without leaving the editor

```text
**;!**/*.cs
```

Use **Explain File Pattern** when the count is surprising. The output shows which rule included or
excluded each file, including files outside `src/`.

### 7. Work in a multi-root workspace

Run either command. When multiple workspace folders are open, choose the folder in the picker.
The pattern is evaluated relative to that folder, and paths in the output are relative to it.

### 8. Prefill a team pattern

Add this to `.vscode/settings.json`:

```json
{
  "jeninnet.defaultPattern": "**;!**/*.{cs,vb,fs}",
  "jeninnet.ignoreCase": true
}
```

Every prompt starts with the team's standard pattern.

---

## Settings

| Setting | Default | Description |
|---|---|---|
| `jeninnet.executable` | `dotnet-filequery` | Command or absolute path used to run the tool. |
| `jeninnet.defaultPattern` | `!**` | Pattern prefilled in the prompt. Use `;` for multiple patterns. |
| `jeninnet.ignoreCase` | `false` | Passes `--ignore-case` to the tool. |

Example for a custom tool location:

```json
{
  "jeninnet.executable": "C:\\Users\\me\\.dotnet\\tools\\dotnet-filequery.exe"
}
```

The executable setting is useful when the tool is installed under a different shim, when using a
local build, or when a CI image exposes it outside `PATH`.

---

## Output and limits

- Results go to the **Jeninnet FileQuery** output channel (`View → Output`).
- The extension runs the tool once per command; it is on-demand feedback, not a per-keystroke
  watcher.
- Standard output is capped at **16 MB** per run.
- Each invocation times out after **120 seconds**.
- Very large workspaces should be narrowed with patterns or a more specific workspace folder.

---

## Troubleshooting

| Symptom | Cause | Fix |
|---|---|---|
| `The executable "dotnet-filequery" was not found` | Tool not installed or not on `PATH` | Run `dotnet tool install --global Jeninnet.FileQuery.Tool` or set `jeninnet.executable` |
| `Open a folder before running a Jeninnet query` | VS Code has no workspace folder | Open the repository folder, then run the command again |
| Zero matches | `**` excluded everything and no later rule re-included | Add a negation such as `!**/*.cs` |
| A file you expected is excluded | A later positive rule matched it | Run **Explain File Pattern** and read the last matching rule |
| Output stops at 16 MB | Result set is too large for the channel | Narrow the folder or pattern |
| Run times out after two minutes | Very large tree or slow disk | Add exclusions such as `bin/`, `obj/`, `node_modules/`, or narrow the root |

---

## How it works

```text
Command Palette
  → prompt for pattern + workspace folder
  → buildArgs() in src/runner.js
  → dotnet-filequery --root <folder> [--explain] [--ignore-case] <pattern>
  → parse output in src/runner.js
  → write to the Jeninnet FileQuery output channel
```

`src/runner.js` contains only argument construction and output parsing, with no `vscode` import, so
the same contract can back another editor front end. `src/extension.js` contains the VS Code
integration: commands, input box, folder picker, progress, output channel, and error messages.

---

## Development

```bash
cd extensions/vscode
node --test "test/*.test.js"
```

The tests cover:

- argument construction for match/explain, case-insensitivity, depth, non-recursive, and
  changed-since options;
- line splitting and CRLF removal;
- match output parsing;
- tab-separated explanation parsing.

The editor UI itself is verified manually in the Extension Development Host (`F5`).

### Packaging

```bash
npx @vscode/vsce package
npx @vscode/vsce publish --packagePath jeninnet-filequery-0.1.0.vsix
```

The Marketplace publisher is `jeninnet`; the first publish needs a publisher and a
`VSCE_PAT` access token. The extension version is independent of the NuGet package version.

---

## Scope and limitations

- **VS Code only.** The Visual Studio extension remains a follow-up; it needs a different VSIX and
  `Microsoft.VisualStudio.Shell` integration.
- **On-demand queries.** There is no file watcher or per-keystroke evaluation.
- **No inline decorations.** Results are written to the output channel, not painted in the explorer.
- **No diff-aware command in the UI.** The underlying `runner.js` understands `changedSince`, but the
  current commands expose only pattern, case sensitivity, and executable.
- **Tool output is the source of truth.** If the CLI and extension ever disagree, the CLI is the
  contract.

---

## See also

- [Global tool](global-tool.md) — all CLI options and exit codes.
- [Diff-aware queries](diff-aware-queries.md) — restrict a query to changed files from the CLI.
- [Git ignore files](gitignore-files.md) — repository ignore semantics.
- [Pattern overview](../reference/pattern-overview.md) — dialect selection and precedence.
- [Extension source](https://github.com/TarekNajem04/Jeninnet.FileQuery/tree/main/extensions/vscode)
