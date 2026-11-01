# Jeninnet FileQuery for VS Code

Test [Jeninnet.FileQuery](../../src/Jeninnet.FileQuery/README.md) patterns against your workspace
without leaving the editor, and see why each file is included or excluded.

## Requirements

- VS Code 1.85 or newer.
- The `dotnet-filequery` global tool
  ([Jeninnet.FileQuery.Tool](../../src/Jeninnet.FileQuery.Tool/README.md)):

  ```bash
  dotnet tool install --global Jeninnet.FileQuery.Tool
  ```

  Verify it with `dotnet-filequery --version`. If the tool is installed under a different command,
  set `jeninnet.executable` accordingly.

## Commands

| Command | Description |
|---|---|
| `Jeninnet: Test File Pattern` | Prompts for a pattern and lists the matching files, relative to the workspace folder. |
| `Jeninnet: Explain File Pattern` | Prompts for a pattern and shows every evaluated file as `[include]` or `[exclude]` with the reason reported by the engine. |

Both commands run against the selected workspace folder. In a multi-root workspace you are asked
which folder to query. Patterns use the engine's GitIgnore rule order; start with `**` to ignore
everything and re-include files with `!`:

```text
**;!*.cs
```

An empty pattern queries every file. Multiple patterns are separated with `;`.

## Settings

| Setting | Default | Description |
|---|---|---|
| `jeninnet.executable` | `dotnet-filequery` | Command used to run the tool. |
| `jeninnet.defaultPattern` | `!**` | Pattern prefilled in the prompt. |
| `jeninnet.ignoreCase` | `false` | Match patterns without case sensitivity. |

## Packaging and publishing

The extension has no runtime npm dependencies, so packaging needs only the VS Code extension CLI:

```bash
npx @vscode/vsce package
```

This produces `jeninnet-filequery-0.1.0.vsix`, installable with:

```bash
code --install-extension jeninnet-filequery-0.1.0.vsix
```

Publishing to the Visual Studio Marketplace uses the `jeninnet` publisher:

```bash
npx @vscode/vsce publish --packagePath jeninnet-filequery-0.1.0.vsix
```

The first publish requires a Marketplace publisher and an access token; pass it through the
`VSCE_PAT` environment variable.

## Scope

This release ships the **VS Code** extension. The Visual Studio extension remains a follow-up: it
needs a different packaging and debugging model (VSIX + `Microsoft.VisualStudio.Shell`), and the
shared logic in `src/runner.js` is intentionally free of editor APIs so a Visual Studio front end
can reuse the same argument and output contracts.

## Development

```bash
node --test test
```

The tests cover argument construction and output parsing only; the editor integration is exercised
manually with the Extension Development Host (`F5`).

## License

MIT — see [LICENSE](../../LICENSE).
