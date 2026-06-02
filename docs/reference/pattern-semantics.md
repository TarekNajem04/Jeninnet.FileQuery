# Pattern Semantics

Jeninnet.FileQuery supports GitIgnore, Glob, Regex, and Hybrid matching. In Hybrid mode, patterns are classified before compilation and evaluated through the unified matcher pipeline.

| Feature | GitIgnore | Glob | Regex |
|---------|-----------|------|-------|
| Recursive wildcard | `**` matches across directories | `**` is supported by the compiled glob pipeline | Use explicit regex such as `.*` |
| Single segment wildcard | `*` | `*` | Use regex syntax |
| Single character wildcard | Not the primary dialect feature | `?` | Use regex syntax |
| Character classes | Limited by GitIgnore rules | `[a-z]`, `[0-9]` | Native regex classes |
| Negation | `!pattern` re-includes later matches | Not a native glob feature | Not a native regex feature |
| Directory-only rule | Trailing slash targets directories | Use explicit path patterns | Match normalized path text |
| Precedence | Later rules win | Later compiled matches win through resolver | Later compiled matches win through resolver |

Use GitIgnore for ordered include/exclude rules, Glob for shell-style file names, Regex for advanced text matching, and Hybrid when input may contain more than one dialect.
