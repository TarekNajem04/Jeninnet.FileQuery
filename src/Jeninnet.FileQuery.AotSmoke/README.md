# Jeninnet.FileQuery.AotSmoke

Native AOT smoke host for the core engine. It creates a deterministic temporary tree, runs a real
query through the AOT-compiled library, verifies paths and `FileQueryEntry` metadata, and exits
non-zero on any mismatch.

This project is not published as a package. It is the CI proof that the "AOT-ready" design claim
stays true.

## What it verifies

- `FileQuery.From(...).UsingGlob().Where(...)` compiles and executes under Native AOT.
- Matching returns the expected files in deterministic order.
- `ExecuteEntries()` still populates `Length` and other metadata.
- Trimming/AOT warnings are treated as errors during publish.

## Publish and run

The CI job publishes `linux-x64`:

```bash
dotnet publish src/Jeninnet.FileQuery.AotSmoke/Jeninnet.FileQuery.AotSmoke.csproj \
  -c Release \
  -r linux-x64 \
  --self-contained true \
  -p:PublishAot=true \
  -p:TrimmerSingleWarn=false \
  -p:IlcTreatWarningsAsErrors=true \
  -o artifacts/aot

./artifacts/aot/Jeninnet.FileQuery.AotSmoke
```

Expected output:

```text
AOT smoke passed: bin/c.cs, src/a.cs
```

For a local Windows check, use `-r win-x64` and run the produced `.exe`.

## Project settings

| Property | Value | Why |
|---|---|---|
| `PublishAot` | `true` | Publish the host and the engine as native code |
| `PublishTrimmed` | `true` | Exercise the trimmer with the real query path |
| `TrimMode` | `full` | No assembly-level trim exceptions |
| `InvariantGlobalization` | `true` | Keeps the smoke host deterministic |
| `IlcTreatWarningsAsErrors` | `true` | AOT warnings fail the build |
| `TrimmerSingleWarn` | `false` | Show every trim warning instead of a summary |

## Design notes

- The host uses only the public API; internal test hooks are not needed.
- The temporary tree is deleted in `finally`, including on failure.
- The query is small but exercises classification, compilation, traversal, matching, ordering, and
  metadata projection.
- The host returns `0` on success and `1` with a diagnostic on failure.

See the [Native AOT guide](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/guides/native-aot.md)
for the full pipeline and limitations.
