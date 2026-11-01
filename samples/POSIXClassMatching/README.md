# POSIXClassMatching Sample

Use POSIX named character classes inside a Glob bracket expression.

**What it teaches:** `[[:digit:]]` and other POSIX classes in Glob patterns.

```csharp
.UsingGlob()
.Where("**/*[[:digit:]].*")
```

## Run

```bash
dotnet run --project samples/POSIXClassMatching/POSIXClassMatching.csproj
```

Expected: **2 files** — `assets/logo1.png` and `assets/logo2.png`.

## Supported classes

`[:digit:]`, `[:alpha:]`, `[:alnum:]`, `[:space:]`, `[:blank:]`, `[:upper:]`, `[:lower:]`,
`[:print:]`, `[:graph:]`, `[:punct:]`, `[:cntrl:]`, and `[:xdigit:]`.

The class is evaluated inside the compiled segment matcher; no regex engine is involved.

## Related

- [Glob reference](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/reference/glob-patterns.md)
- [All samples](../README.md)
