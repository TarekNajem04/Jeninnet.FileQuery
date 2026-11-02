# JSON Output

`dotnet-filequery --json` writes **newline-delimited JSON** (one object per line) so scripts can
consume results incrementally without buffering the whole query.

---

## Commands

```bash
dotnet-filequery --root ./repo --json
dotnet-filequery --root ./repo --json "**/*.cs"
dotnet-filequery --root ./repo --json --explain "**;!*.cs"
dotnet-filequery --root ./repo --json --changed-since HEAD
```

`--json` composes with diff-aware queries (`--changed-since` / `--changed-since-date`) and with
`--explain`. It cannot be combined with `--null` or `--count`.

`--relative` is not needed in JSON mode: every object contains both the absolute `path` and the
root-relative `relativePath`.

---

## Normal result schema

```json
{
  "path": "C:\\repo\\src\\Program.cs",
  "relativePath": "src/Program.cs",
  "length": 1834,
  "createdUtc": "2026-11-01T10:15:30.1234567Z",
  "modifiedUtc": "2026-11-01T11:02:10.7654321Z",
  "attributes": ["Archive"],
  "isDirectory": false
}
```

| Field | Type | Description |
|---|---|---|
| `path` | string | Absolute path reported by the operating system. |
| `relativePath` | string | Root-relative path with forward slashes and no trailing slash. |
| `length` | number | Entry size in bytes. |
| `createdUtc` | string | Creation timestamp in UTC (ISO 8601). |
| `modifiedUtc` | string | Last-write timestamp in UTC (ISO 8601). |
| `attributes` | string[] | Active `FileAttributes` flag names. |
| `isDirectory` | boolean | Whether the entry is a directory. |

---

## Explain schema

With `--explain`, each line describes one evaluated **file** and adds:

```json
{
  "path": "C:\\repo\\a.txt",
  "relativePath": "a.txt",
  "entryKind": "File",
  "outcome": "exclude",
  "reason": "Exclude by GitIgnore pattern.",
  "length": 7,
  "createdUtc": "2026-11-01T10:15:30.1234567Z",
  "modifiedUtc": "2026-11-01T10:15:30.1234567Z",
  "attributes": ["Archive"],
  "isDirectory": false,
  "patternKind": "GitIgnore",
  "pattern": "**",
  "patternIndex": 0
}
```

| Field | Type | Description |
|---|---|---|
| `entryKind` | string | `File` for explain output. |
| `outcome` | string | `include`, `exclude`, or `nomatch`. |
| `reason` | string | Human-readable explanation. |
| `patternKind` | string or null | Dialect of the deciding pattern. |
| `pattern` | string or null | Source text of the deciding pattern. |
| `patternIndex` | number or null | Zero-based source pattern index. |

Metadata fields are present when the enumeration supplies them; timestamps and attributes are
omitted if the platform cannot report them.

---

## Stability

The field names and value types above form the consumer schema. New fields may be added in a
compatible release; existing fields keep their names and semantics. Consumers should ignore
unknown fields.
