# Common PowerShell Module

Shared PowerShell utilities used by Jeninnet repository automation scripts.

## Purpose

This module provides common functionality to keep repository scripts:

- Consistent.
- Maintainable.
- Easy to extend.

## Design Rules

- Only shared functionality belongs here.
- Avoid adding helpers for future assumptions.
- Keep functions focused and reusable.
- Changes should benefit multiple scripts.

## Usage

Import the module:

```powershell
Import-Module "./scripts/common/Common.psd1"
