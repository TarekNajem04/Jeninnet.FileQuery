# Public API Options Inspection

`FileQuery` keeps its `Options` property internal. `FileQueryOptions` is the public immutable
configuration snapshot, but the query descriptor exposes only `RootPath`; callers configure options
through the fluent builder and `FileQueryOptionsConfig`.

This preserves room to evolve internal matching/traversal composition without exposing mutable
state. Advanced inspection should be added through a dedicated read-only diagnostics API, not by
exposing mutable options.

Decision: keep `FileQuery.Options` internal for v2.x.
