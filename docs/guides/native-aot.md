# Native AOT Validation

The repository validates Native AOT compatibility by publishing a real query host with
`PublishAot` and running it. AOT and trimming warnings are treated as errors.

---

## Smoke host

`src/Jeninnet.FileQuery.AotSmoke` creates a temporary directory tree, runs a Glob query through the
public API, compares the results, and reads entry metadata. It exits with a non-zero code on any
mismatch.

Publish it on Linux:

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

The project sets `PublishAot`, `PublishTrimmed`, `TrimMode=full`, `InvariantGlobalization`, and
`IlcTreatWarningsAsErrors` by default. The CI job publishes and runs the binary on
`ubuntu-latest` with the `linux-x64` runtime identifier.

---

## What the pipeline guarantees

- The library and its public query path publish without trimming or AOT warnings.
- The AOT-compiled binary executes a real traversal, pattern compilation, matching, and metadata
  projection.
- Warnings such as `IL2026`, `IL3050`, or `IL2104` fail the job.

---

## Limits

- CI validates `linux-x64` only. Windows and macOS use the same managed code but are not
  AOT-published in every build.
- Third-party `IFileSystem` implementations are the consumer's responsibility; the built-in
  `FileSystem` is the validated implementation.
- The smoke host uses the public API and invariant globalization. Applications that add
  culture-sensitive formatting or reflection-based serializers must validate their own code.
- Native toolchain requirements (Clang, `zlib`, linker) are provided by the Ubuntu runner. Local
  publishing requires the platform's Native AOT prerequisites.
