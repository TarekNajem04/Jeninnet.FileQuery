'use strict';

const vscode = require('vscode');
const { execFile } = require('node:child_process');
const { promisify } = require('node:util');
const { buildArgs, parseMatches, parseExplanations } = require('./runner');

const execFileAsync = promisify(execFile);
const OUTPUT_NAME = 'Jeninnet FileQuery';

/** @type {vscode.OutputChannel | undefined} */
let output;

/**
 * Registers the extension commands.
 *
 * @param {vscode.ExtensionContext} context The extension context.
 */
function activate(context) {
    output = vscode.window.createOutputChannel(OUTPUT_NAME);
    context.subscriptions.push(output);
    context.subscriptions.push(
        vscode.commands.registerCommand('jeninnet.testPattern', () => runPattern({ explain: false })),
        vscode.commands.registerCommand('jeninnet.explainPattern', () => runPattern({ explain: true }))
    );
}

/**
 * Deactivates the extension.
 */
function deactivate() {
    output?.dispose();
    output = undefined;
}

/**
 * Prompts for a pattern and runs it against the selected workspace folder.
 *
 * @param {{explain: boolean}} options Whether to explain each file instead of listing matches.
 * @returns {Promise<void>}
 */
async function runPattern({ explain }) {
    const folder = await pickWorkspaceFolder();

    if (!folder) {
        return;
    }

    const configuration = vscode.workspace.getConfiguration('jeninnet', folder.uri);
    const pattern = await vscode.window.showInputBox({
        title: explain ? 'Explain File Pattern' : 'Test File Pattern',
        prompt: 'GitIgnore, Glob, or Regex patterns separated by ";" (leave empty for every file)',
        value: configuration.get('defaultPattern', '!**'),
        ignoreFocusOut: true
    });

    if (pattern === undefined) {
        return;
    }

    const executable = configuration.get('executable', 'dotnet-filequery');
    const args = buildArgs({
        root: folder.uri.fsPath,
        pattern: pattern.trim(),
        explain,
        ignoreCase: configuration.get('ignoreCase', false)
    });

    output.clear();
    output.show(true);

    try {
        const { stdout } = await vscode.window.withProgress(
            {
                location: vscode.ProgressLocation.Window,
                title: 'Jeninnet FileQuery'
            },
            () => execFileAsync(executable, args, {
                cwd: folder.uri.fsPath,
                maxBuffer: 16 * 1024 * 1024,
                timeout: 120000,
                windowsHide: true
            })
        );

        writeResult(pattern, stdout, explain);
    } catch (error) {
        const message = describeFailure(error, executable);
        output.appendLine(`Error: ${message}`);
        vscode.window.showErrorMessage(message);
    }
}

/**
 * Writes the tool output to the channel.
 *
 * @param {string} pattern The pattern that was evaluated.
 * @param {string} stdout Captured standard output.
 * @param {boolean} explain Whether the output contains explanations.
 */
function writeResult(pattern, stdout, explain) {
    const label = pattern.length > 0 ? pattern : '(every file)';

    if (explain) {
        const rows = parseExplanations(stdout);
        const included = rows.filter((row) => row.outcome === 'include').length;
        output.appendLine(`Pattern: ${label} — ${rows.length} file(s), ${included} included`);

        for (const row of rows) {
            const marker = row.outcome === 'include' ? '[include]' : '[exclude]';
            output.appendLine(`${marker} ${row.path} — ${row.reason}`);
        }

        return;
    }

    const matches = parseMatches(stdout);
    output.appendLine(`Pattern: ${label} — ${matches.length} match(es)`);

    for (const match of matches) {
        output.appendLine(match);
    }
}

/**
 * Turns a process failure into an actionable message.
 *
 * @param {any} error The failure raised by execFile.
 * @param {string} executable The configured executable.
 * @returns {string} The message shown to the user.
 */
function describeFailure(error, executable) {
    if (error && error.code === 'ENOENT') {
        return `The executable "${executable}" was not found. Install the tool with "dotnet tool install --global Jeninnet.FileQuery.Tool" or set "jeninnet.executable".`;
    }

    const standardError = error && typeof error.stderr === 'string' ? error.stderr.trim() : '';
    return standardError.length > 0 ? standardError : String(error && error.message ? error.message : error);
}

/**
 * Picks the workspace folder to query.
 *
 * @returns {Promise<vscode.WorkspaceFolder | undefined>} The selected folder, or undefined.
 */
async function pickWorkspaceFolder() {
    const folders = vscode.workspace.workspaceFolders;

    if (!folders || folders.length === 0) {
        vscode.window.showWarningMessage('Open a folder before running a Jeninnet query.');
        return undefined;
    }

    if (folders.length === 1) {
        return folders[0];
    }

    return vscode.window.showWorkspaceFolderPick({ placeHolder: 'Workspace folder to query' });
}

module.exports = {
    activate,
    deactivate
};
