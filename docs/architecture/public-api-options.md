# Public API Options Inspection

`FileQuery` intentionally keeps `FileQueryOptions` internal. The public contract exposes a stable query descriptor (`RootPath`) and sends execution details through `IFileQueryEngine`.

This preserves room to change internal matching, traversal, and compilation options without breaking consumers before the 1.0 release. Advanced inspection should be added only through a dedicated read-only diagnostics API, not by exposing mutable options.

Decision: keep `FileQuery.Options` internal for 1.0.
