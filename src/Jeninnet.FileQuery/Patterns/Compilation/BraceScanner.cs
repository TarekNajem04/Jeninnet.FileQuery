//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Compilation;

/// <summary>
/// Scans brace-pattern text while skipping escaped characters and character-class contents.
/// </summary>
internal ref struct BraceScanner {
    private readonly ReadOnlySpan<char> _text;
    private readonly int _end;

    /// <summary>
    /// Initializes a scanner over a pattern range.
    /// </summary>
    /// <param name="text">The pattern text.</param>
    /// <param name="start">The first position to scan.</param>
    /// <param name="end">The exclusive end position.</param>
    public BraceScanner(ReadOnlySpan<char> text, int start, int end) {
        _text = text;
        _end = end;
        Position = start;
    }

    /// <summary>
    /// Gets the position of the current significant character.
    /// </summary>
    public int Position { get; private set; }

    /// <summary>
    /// Gets the current significant character.
    /// </summary>
    public readonly char Current => _text[Position];

    /// <summary>
    /// Moves to the next significant character, skipping escapes and character classes.
    /// </summary>
    /// <returns><see langword="true"/> when another significant character is available.</returns>
    public bool MoveNext() {
        while(Position < _end) {
            var c = _text[Position];

            if(c == '\\') {
                Position += 2;
                continue;
            }

            if(c == '[') {
                SkipCharacterClass();
                continue;
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Advances past the current significant character.
    /// </summary>
    public void Advance() => Position++;

    /// <summary>
    /// Moves the scan position to a specific index.
    /// </summary>
    /// <param name="position">The new scan position.</param>
    public void MoveTo(int position) => Position = position;

    private void SkipCharacterClass() {
        Position++;

        while(Position < _end) {
            var c = _text[Position];

            if(c == '\\') {
                Position += 2;
                continue;
            }

            if(c == ']') {
                Position++;
                return;
            }

            Position++;
        }
    }
}
