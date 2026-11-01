'use strict';

const test = require('node:test');
const assert = require('node:assert/strict');
const {
    buildArgs,
    splitLines,
    parseMatches,
    parseExplanations
} = require('../src/runner');

test('buildArgs requests relative matches for a workspace query', () => {
    const args = buildArgs({ root: '/repo', pattern: '**;!*.cs' });

    assert.deepEqual(args, ['--root', '/repo', '--relative', '**;!*.cs']);
});

test('buildArgs switches to explain mode', () => {
    const args = buildArgs({ root: '/repo', pattern: '**;!*.cs', explain: true });

    assert.deepEqual(args, ['--root', '/repo', '--explain', '**;!*.cs']);
    assert.ok(!args.includes('--relative'));
});

test('buildArgs includes traversal and change-set options', () => {
    const args = buildArgs({
        root: '/repo',
        pattern: '!*.md',
        ignoreCase: true,
        depth: 2,
        changedSince: 'HEAD~1'
    });

    assert.deepEqual(
        args,
        ['--root', '/repo', '--ignore-case', '--depth', '2', '--changed-since', 'HEAD~1', '--relative', '!*.md']
    );
});

test('buildArgs supports root-only searches', () => {
    const args = buildArgs({ noRecursive: true });

    assert.deepEqual(args, ['--no-recursive', '--relative']);
});

test('splitLines removes CRLF and blank lines', () => {
    assert.deepEqual(splitLines('a.txt\r\n\r\nb.txt\n'), ['a.txt', 'b.txt']);
});

test('parseMatches returns the emitted paths', () => {
    assert.deepEqual(parseMatches('src/a.cs\nsrc/b.cs\n'), ['src/a.cs', 'src/b.cs']);
});

test('parseExplanations reads outcome, path, and reason', () => {
    const rows = parseExplanations(
        'include\tsrc/a.cs\tInclude by GitIgnore pattern.\n' +
        'exclude\tbin/a.dll\tExclude by GitIgnore pattern.\n'
    );

    assert.deepEqual(rows, [
        { outcome: 'include', path: 'src/a.cs', reason: 'Include by GitIgnore pattern.' },
        { outcome: 'exclude', path: 'bin/a.dll', reason: 'Exclude by GitIgnore pattern.' }
    ]);
});

test('parseExplanations ignores malformed lines', () => {
    assert.deepEqual(parseExplanations('not an explanation\ninclude\tfile.cs\n'), []);
});
