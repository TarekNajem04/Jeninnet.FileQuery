# Test Scenarios

## src\Jeninnet.FileQuery\Patterns\Canonical\CanonicalPatternInput.cs

### CanonicalPatternInput (Constructor)

- Scenario: Default constructor
- Inputs: patterns = null, typedPatterns = null, interpretationMode = default
- Expected Result: Patterns is empty, TypedPatterns is _emptyTypedPatterns, InterpretationMode is Hybrid

- Scenario: Explicit non-empty patterns
- Inputs: patterns = ["a", "b"], typedPatterns = null, interpretationMode = Hybrid
- Expected Result: Patterns contains ["a", "b"], TypedPatterns is _emptyTypedPatterns

- Scenario: Explicit typed patterns
- Inputs: patterns = null, typedPatterns = { PatternKind.Glob: ["*.txt"] }, interpretationMode = Hybrid
- Expected Result: Patterns is empty, TypedPatterns contains PatternKind.Glob: ["*.txt"]

- Scenario: Typed patterns with null list
- Inputs: patterns = null, typedPatterns = { PatternKind.Glob: null }, interpretationMode = Hybrid
- Expected Result: TypedPatterns contains PatternKind.Glob: []

- Scenario: Multiple typed patterns
- Inputs: patterns = null, typedPatterns = { PatternKind.Glob: ["a"], PatternKind.Regex: ["b"] }, interpretationMode = Hybrid
- Expected Result: TypedPatterns contains PatternKind.Glob: ["a"], PatternKind.Regex: ["b"]

- Scenario: Explicit interpretation mode
- Inputs: patterns = null, typedPatterns = null, interpretationMode = PatternInterpretationMode.Regex
- Expected Result: InterpretationMode is PatternInterpretationMode.Regex

- Scenario: Empty typed patterns dictionary
- Inputs: patterns = null, typedPatterns = {}, interpretationMode = Hybrid
- Expected Result: TypedPatterns is _emptyTypedPatterns

## src\Jeninnet.FileQuery\Patterns\Canonical\PatternCanonicalizer.cs

### Canonicalize

- Scenario: Input is null
- Inputs: null
- Expected Result: ArgumentNullException

- Scenario: Empty input
- Inputs: CanonicalPatternInput(patterns: [], typedPatterns: [])
- Expected Result: CanonicalPatternSet with empty Patterns list

- Scenario: Typed patterns only
- Inputs: CanonicalPatternInput(patterns: [], typedPatterns: { PatternKind.Glob: ["*.txt"] })
- Expected Result: CanonicalPatternSet containing CanonicalPattern("*.txt", PatternKind.Glob)

- Scenario: Raw patterns only
- Inputs: CanonicalPatternInput(patterns: ["*.cs"], typedPatterns: [])
- Expected Result: CanonicalPatternSet containing CanonicalPattern("*.cs", null)

- Scenario: Duplicate patterns (Typed vs Typed)
- Inputs: CanonicalPatternInput(patterns: [], typedPatterns: { PatternKind.Glob: ["*.txt", "*.txt"] })
- Expected Result: CanonicalPatternSet containing one CanonicalPattern("*.txt", PatternKind.Glob)

- Scenario: Duplicate patterns (Raw vs Raw)
- Inputs: CanonicalPatternInput(patterns: ["*.cs", "*.cs"], typedPatterns: [])
- Expected Result: CanonicalPatternSet containing one CanonicalPattern("*.cs", null)

- Scenario: Overlap (Raw and Typed same string)
- Inputs: CanonicalPatternInput(patterns: ["test"], typedPatterns: { PatternKind.Regex: ["test"] })
- Expected Result: CanonicalPatternSet containing CanonicalPattern("test", PatternKind.Regex) and CanonicalPattern("test", null)

- Scenario: Multiple Types
- Inputs: CanonicalPatternInput(patterns: [], typedPatterns: { PatternKind.Glob: ["*.txt"], PatternKind.Regex: ["\\.log$"] })
- Expected Result: CanonicalPatternSet containing CanonicalPattern("*.txt", PatternKind.Glob) and CanonicalPattern("\\.log$", PatternKind.Regex)

## src\Jeninnet.FileQuery\Patterns\Compilation\HybridPatternCompiler.cs

### HybridPatternCompiler (Constructor)

- Scenario: Valid dependency injection
- Inputs: Mock IPatternCompiler for git, glob, and regex
- Expected Result: Fields _git, _glob, _regex are assigned correctly

### Select

- Scenario: Input pattern type is Glob
- Inputs: ClassifiedPattern with Type = PatternKind.Glob
- Expected Result: Returns the injected glob compiler instance

- Scenario: Input pattern type is Regex
- Inputs: ClassifiedPattern with Type = PatternKind.Regex
- Expected Result: Returns the injected regex compiler instance

- Scenario: Input pattern type is GitIgnore (Default)
- Inputs: ClassifiedPattern with Type = PatternKind.GitIgnore
- Expected Result: Returns the injected git compiler instance

- Scenario: Input pattern type is Unknown/Other (Default)
- Inputs: ClassifiedPattern with Type = (PatternKind)99
- Expected Result: Returns the injected git compiler instance

## src\Jeninnet.FileQuery\Patterns\PatternTypeComparer.cs

### Equals

- Scenario: Same PatternKind values
- Inputs: PatternKind.Glob, PatternKind.Glob
- Expected Result: True

- Scenario: Different PatternKind values
- Inputs: PatternKind.Glob, PatternKind.Regex
- Expected Result: False

### GetHashCode

- Scenario: Valid PatternKind value
- Inputs: PatternKind.GitIgnore
- Expected Result: Matches obj.GetHashCode()

## src\Jeninnet.FileQuery\Patterns\Syntax\PatternToken.cs

### LiteralToken (Constructor)
- Scenario: Constructor initialization
- Inputs: "test"
- Expected Result: Text property matches input

### RegularExpressionToken (Constructor)
- Scenario: Constructor initialization
- Inputs: ".*"
- Expected Result: Pattern property matches input

### WildcardToken (Constructor)
- Scenario: Constructor initialization
- Inputs: None
- Expected Result: Token is successfully created

### RecursiveWildcardToken (Constructor)
- Scenario: Constructor initialization
- Inputs: None
- Expected Result: Token is successfully created

### SingleCharToken (Constructor)
- Scenario: Constructor initialization
- Inputs: None
- Expected Result: Token is successfully created

### EscapeToken (Constructor)
- Scenario: Constructor initialization
- Inputs: 'a'
- Expected Result: Escaped property matches input

### CharacterClassToken (Constructor)
- Scenario: Constructor initialization
- Inputs: new CharacterClass(...)
- Expected Result: Value property matches input

### RegularExpressionToken.ToString

- Scenario: Default regex token
- Inputs: RegularExpressionToken("a.*b")
- Expected Result: "a.*b"

### WildcardToken.ToString

- Scenario: Wildcard string representation
- Inputs: new WildcardToken()
- Expected Result: "*"

### RecursiveWildcardToken.ToString

- Scenario: Recursive wildcard string representation
- Inputs: new RecursiveWildcardToken()
- Expected Result: "**"

### SingleCharToken.ToString

- Scenario: Single char token
- Inputs: new SingleCharToken()
- Expected Result: "?"

### EscapeToken.ToString

- Scenario: Escape token
- Inputs: new EscapeToken('*')
- Expected Result: "*"

### CharacterClassToken.ToString

- Scenario: Positive character class
- Inputs: CharacterClassToken(new CharacterClass(IsNegated: false, ...))
- Expected Result: "[…]"

- Scenario: Negative character class
- Inputs: CharacterClassToken(new CharacterClass(IsNegated: true, ...))
- Expected Result: "[!…]"

## src\Jeninnet.FileQuery\Patterns\Tokenization\EscapeReader.cs

### TryRead

- Scenario: Not an escape character
- Inputs: pattern = "abc", i = 0
- Expected Result: False, token remains null

- Scenario: Backslash at end of string
- Inputs: pattern = "\\", i = 0
- Expected Result: False

- Scenario: Escaped non-escapable character
- Inputs: pattern = "\\a", i = 0
- Expected Result: False

- Scenario: Valid escape character (e.g., '\*')
- Inputs: pattern = "\\*", i = 0
- Expected Result: True, token is EscapeToken('*'), i is 2

- Scenario: Valid escape character (e.g., '\')
- Inputs: pattern = "\\\\", i = 0
- Expected Result: True, token is EscapeToken('\\'), i is 2

### IsEscapable

- Scenario: All valid escapable characters
- Inputs: '*', '?', '!', '#', '[', ']', '\\'
- Expected Result: True for all

- Scenario: Invalid escapable characters
- Inputs: 'a', 'b', '1'
- Expected Result: False for all

## src\Jeninnet.FileQuery\Patterns\Tokenization\LiteralReader.cs

### TryRead

- Scenario: Empty pattern
- Inputs: pattern = "", i = 0
- Expected Result: False

- Scenario: Pattern starting with wildcard
- Inputs: pattern = "*abc", i = 0
- Expected Result: False

- Scenario: Simple literal string
- Inputs: pattern = "abc", i = 0
- Expected Result: True, token is LiteralToken("abc"), i is 3

- Scenario: Literal string followed by wildcard
- Inputs: pattern = "abc*def", i = 0
- Expected Result: True, token is LiteralToken("abc"), i is 3

### IsLiteral

- Scenario: Valid literal characters
- Inputs: 'a', '1', '_', '-'
- Expected Result: True

- Scenario: Wildcard/special characters
- Inputs: '*', '?', '[', ']', '/', '\\'
- Expected Result: False
