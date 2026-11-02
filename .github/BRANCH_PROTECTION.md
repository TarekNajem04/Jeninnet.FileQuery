# Branch Protection and Repository Security

This repository requires owner-managed branch protection for `main`. (`master`/`develop` are not
used; create protection for any additional long-lived branch if one is introduced.)

## Required Branch Protection

Enable these settings for each protected branch:

- Require a pull request before merging.
- Require at least one approval.
- Require review from CODEOWNERS.
- Dismiss stale pull request approvals when new commits are pushed.
- Require conversation resolution before merge.
- Require status checks to pass before merging.
- Require branches to be up to date before merging.
- Include administrators.
- Restrict who can push to the branch.
- Disable force pushes.
- Disable branch deletion.
- Do not allow bypassing the above settings.

Only `@TarekNajem04` may approve, merge, or push to protected branches.

Recommended required status checks:

- `Format`
- `Build & Test (ubuntu-latest)`
- `Build & Test (windows-latest)`
- `Build & Test (macos-latest)`
- `Architecture Tests`
- `VS Code Extension Tests`
- `FileQuery Action`
- `Native AOT Smoke (linux-x64)`
- `Analyzer Package`
- `Build Package`
- `Long-Running Stability`
- `Package Validation`
- `Build Documentation`
- `Build, Test, Analyze` (SonarCloud, when the token is configured)

`auto-approve-dependabot.yml` is the documented exception to the “no auto-merge” rule: it is
restricted to Dependabot-authored patch updates and still requires the protected-branch checks.

## Repository Settings

Enable:

- Dependency graph.
- Dependabot alerts.
- Dependabot security updates.
- Secret scanning.
- Push protection.
- Code scanning with CodeQL.
- GitHub Actions default workflow permissions: read-only.
- Pull request review dismissal for stale approvals.

Disable:

- Direct pushes to protected branches.
- Force pushes.
- Branch deletion.
- Admin bypass for branch protection.
- Auto-merge except the Dependabot patch-update workflow documented above.

## GitHub Actions Policy

Repository-level Actions permissions must default to read-only. Workflows declare `contents: read`
by default and grant write permissions only at job scope when required, such as GitHub Pages
deployment or release creation.

Changes under `.github/workflows/` require owner review through CODEOWNERS and must be merged only after required checks pass.

## Environment Protection

For each deployment environment, require owner approval, prevent self-review, and restrict deployments to protected branches only. Apply this at minimum to `github-pages` and any package publishing or production environments.
