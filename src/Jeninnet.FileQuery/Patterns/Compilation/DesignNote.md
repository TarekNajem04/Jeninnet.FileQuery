Pattern Compiler Design Note
----------------------------

The pattern compiler accepts exactly one pattern per call. It does not
interpret separators such as ';', ',', or whitespace. Splitting a
multi-pattern string is the responsibility of higher-level utilities
(e.g., PatternListParser or CompiledPatternSet).

Rationale:
- Keeps the compiler focused on pattern syntax, not list syntax.
- Avoids ambiguity around escaping and quoting.
- Matches the design of GitIgnore, Glob, and other pattern engines.
- Ensures consistent behavior across pattern types.
