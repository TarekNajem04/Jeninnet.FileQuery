'use strict';

/**
 * Builds the `dotnet-filequery` argument list for a workspace query.
 *
 * @param {object} options Query options.
 * @param {string} [options.root] Workspace root.
 * @param {string} [options.pattern] Semicolon-separated patterns.
 * @param {boolean} [options.explain] Emit include/exclude reasons instead of matches.
 * @param {boolean} [options.ignoreCase] Match without case sensitivity.
 * @param {number} [options.depth] Maximum recursion depth.
 * @param {boolean} [options.noRecursive] Search the root directory only.
 * @param {string} [options.changedSince] Git revision baseline.
 * @returns {string[]} The arguments passed to the tool.
 */
function buildArgs(options = {}) {
    const args = [];

    if (options.root) {
        args.push('--root', options.root);
    }

    if (options.ignoreCase) {
        args.push('--ignore-case');
    }

    if (typeof options.depth === 'number') {
        args.push('--depth', String(options.depth));
    }

    if (options.noRecursive) {
        args.push('--no-recursive');
    }

    if (options.changedSince) {
        args.push('--changed-since', options.changedSince);
    }

    if (options.explain) {
        args.push('--explain');
    } else {
        args.push('--relative');
    }

    if (options.pattern) {
        args.push(options.pattern);
    }

    return args;
}

/**
 * Splits tool output into non-empty lines, removing CRLF carriage returns.
 *
 * @param {string} stdout Captured standard output.
 * @returns {string[]} The output lines.
 */
function splitLines(stdout) {
    return String(stdout ?? '')
        .split('\n')
        .map((line) => line.replace(/\r$/, ''))
        .filter((line) => line.length > 0);
}

/**
 * Parses normal match output.
 *
 * @param {string} stdout Captured standard output.
 * @returns {string[]} The matched paths, relative to the root.
 */
function parseMatches(stdout) {
    return splitLines(stdout);
}

/**
 * Parses explain output lines shaped as `outcome<TAB>path<TAB>reason`.
 *
 * @param {string} stdout Captured standard output.
 * @returns {{outcome: string, path: string, reason: string}[]} The parsed explanations.
 */
function parseExplanations(stdout) {
    const rows = [];

    for (const line of splitLines(stdout)) {
        const fields = line.split('\t');

        if (fields.length < 3) {
            continue;
        }

        rows.push({
            outcome: fields[0],
            path: fields[1],
            reason: fields.slice(2).join(' ')
        });
    }

    return rows;
}

module.exports = {
    buildArgs,
    splitLines,
    parseMatches,
    parseExplanations
};
