# Architecture Audit TODO

Pre-release architecture gate for `Jeninnet.FileQuery`. Status values: `Pending`, `In Progress`, `Done`.

## Safe / Non-Breaking Tasks

### 1. Strengthen Project Dependency Boundary Tests

- **Status:** Done
- **Priority:** High
- **Type:** Architecture / Test
- **Risk:** Non-breaking
- **Description:** Add architecture tests that enforce the core library has no project references and does not reference optional integration packages such as dependency injection or command-line concerns.

### 2. Fix File-System Accessibility Responsibility

- **Status:** Done
- **Priority:** High
- **Type:** Refactor / Test
- **Risk:** Non-breaking
- **Description:** Ensure inaccessible-directory checks are applied only to directories, not every file entry, and add regression coverage for `IgnoreInaccessible = false` with normal files.

### 3. Remove Duplicate Matcher Constructor Architecture Test

- **Status:** Done
- **Priority:** Medium
- **Type:** Test / Architecture
- **Risk:** Non-breaking
- **Description:** Consolidate duplicate `IPathMatcher` constructor visibility tests so the architecture suite has one clear invariant.

### 4. Document Layer Boundaries

- **Status:** Done
- **Priority:** Medium
- **Type:** Documentation / Architecture
- **Risk:** Non-breaking
- **Description:** Update architecture documentation with the intended dependency direction between public API, engine, traversal, matching, patterns, IO, and optional integration packages.

### 5. Document Async Enumeration Scalability Constraint

- **Status:** Done
- **Priority:** Medium
- **Type:** Documentation / Architecture
- **Risk:** Non-breaking
- **Description:** Document that file-system enumeration is backed by synchronous OS APIs with cancellation checkpoints, so consumers should not assume parallel or fully non-blocking I/O.

## Full Pre-Release Audit Pass

### 6. Remove `FileInfo` / `DirectoryInfo` from Runtime File-System Resolution

- **Status:** Done
- **Priority:** High
- **Type:** Refactor / Architecture
- **Risk:** Non-breaking
- **Description:** Replace direct `new FileInfo(...)` and `new DirectoryInfo(...)` usage in the production `FileSystem.ResolveRealPath` path with static `File` / `Directory` link-resolution APIs to align with the constitution's file-system constraints.

### 7. Enforce File-System Metadata Type Boundary

- **Status:** Done
- **Priority:** High
- **Type:** Test / Architecture
- **Risk:** Non-breaking
- **Description:** Add architecture coverage that prevents production code from reintroducing direct `FileInfo` or `DirectoryInfo` construction.

### 8. Avoid Allocation in Directory Accessibility Probing

- **Status:** Done
- **Priority:** Medium
- **Type:** Refactor / Performance
- **Risk:** Non-breaking
- **Description:** Replace `Directory.GetFileSystemEntries(...)` in access probing with lazy enumeration so accessibility checks do not allocate an array of every child path.

## Future Work

### A. Make `FileQueryOptions` Physically Immutable

- **Status:** Done
- **Priority:** High
- **Type:** API / Architecture
- **Risk:** Breaking
- **Rationale:** `FileQueryOptions` is documented as logically immutable but remains an init-only record with reference-type members. Future major versions should consider a constructor-only model with immutable collections.
- **Impact:** Object initialization syntax and some test setup code may need migration.
- **Migration Path:** Introduce factory methods or builders first, then obsolete direct mutation patterns before a major-version change.
- **Current Release Decision:** Deferred as a breaking change. No runtime or public API change is applied in v1.0.

### B. Revisit `FileQueryBuilder.Build()` Directory Validation Timing

- **Status:** Done
- **Priority:** Medium
- **Type:** API
- **Risk:** Breaking
- **Rationale:** Build-time validation is documented and locked for v1.0, but some consumers may need reusable query descriptors for paths that do not exist yet.
- **Impact:** Changing the default would alter exception timing for current consumers.
- **Migration Path:** Add an opt-in late-validation mode in a minor release, then evaluate default behavior in the next major version.
- **Current Release Decision:** Deferred as a breaking behavior change. v1.0 keeps build-time validation and documentation remains the compatibility contract.

### C. Rework Async File-System Abstraction

- **Status:** Done
- **Priority:** Medium
- **Type:** Architecture / API
- **Risk:** Breaking
- **Rationale:** The current abstraction exposes async enumeration even though .NET directory APIs are synchronous. A future design should make the semantics explicit or provide a scheduler/pipeline abstraction.
- **Impact:** Implementers of `IFileSystem` and traversal tests may need updates.
- **Migration Path:** Add internal adapter experiments first, benchmark them, then expose only if measurable benefits justify the complexity.
- **Current Release Decision:** Deferred as a breaking architecture change. v1.0 keeps current async enumeration semantics and documents the scalability constraint.

### D. Review Public Pattern Internals Before Next Major API Freeze

- **Status:** Done
- **Priority:** High
- **Type:** API / Architecture
- **Risk:** Breaking
- **Rationale:** Types such as `CanonicalPattern`, `CanonicalPatternSet`, `PatternSyntaxProfile`, and `TokenKind` are publicly visible from implementation-oriented namespaces. They may be intentional diagnostics hooks, but they also expand the compatibility surface around pattern internals.
- **Impact:** Making these types internal or moving them behind a supported diagnostics API would break consumers that currently reference them directly.
- **Migration Path:** Inventory actual package consumers, decide which diagnostics concepts are supported public contracts, introduce replacement APIs where needed, then obsolete implementation-oriented public types before a major-version change.
- **Current Release Decision:** Documented for future API review only. No public API visibility changes are applied in this release.
