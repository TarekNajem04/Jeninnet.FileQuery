# Jeninnet.FileQuery.Analyzers

A Roslyn analyzer and code fix for `Jeninnet.FileQuery` pattern literals.

```xml
<PackageReference Include="Jeninnet.FileQuery.Analyzers" Version="2.0.0" PrivateAssets="all" />
```

The analyzer reports malformed compile-time patterns passed to `FileQueryBuilder.Where`:

```csharp
FileQuery.From(root).Where(PatternKind.GitIgnore, ["src/[a-z.cs"]); // JNQ1001
```

A code fix closes a character class when appending `]` is safe.

See [the analyzer guide](https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/guides/roslyn-analyzers.md)
for rules, severities, limits, and configuration.
