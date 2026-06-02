# Publishing Plan — GitHub & NuGet

Target repository: `github.com/TarekNajem04/Jeninnet.FileQuery`
Packages: `Jeninnet.FileQuery`, `Jeninnet.FileQuery.CommandLine`, `Jeninnet.FileQuery.DependencyInjection`

Each step is self-contained. Complete them in order.

---

## Phase 1 — Repository Preparation (Local)

### Step 1.1 — Verify the solution builds and all tests pass

```powershell
cd "C:\My Files\My Projects\Jeninnet.FileQuery"
dotnet build -c Release
dotnet test  -c Release --no-build
```

All 235 tests must pass. Fix any failure before continuing.

---

### Step 1.2 — Initialize Git

```powershell
cd "C:\My Files\My Projects\Jeninnet.FileQuery"
git init
git branch -M main
```

---

### Step 1.3 — Verify `.gitignore` excludes build artifacts

Open `.gitignore` and confirm these lines are present. Add any that are missing:

```
bin/
obj/
artifacts/
*.user
.vs/
*.DotSettings.user
```

---

### Step 1.4 — Stage and make the initial commit

```powershell
git add .
git commit -m "chore: initial commit — Jeninnet.FileQuery v1.0.0"
```

---

## Phase 2 — GitHub Repository Setup

### Step 2.1 — Create the repository on GitHub

1. Go to https://github.com/new
2. Owner: `TarekNajem04`
3. Repository name: `Jeninnet.FileQuery`
4. Visibility: **Public**
5. Do NOT initialize with README, .gitignore, or license (your local repo already has these)
6. Click **Create repository**

---

### Step 2.2 — Add the remote and push

```powershell
git remote add origin https://github.com/TarekNajem04/Jeninnet.FileQuery.git
git push -u origin main
```

---

### Step 2.3 — Add a repository description and topics on GitHub

Go to the repository page → gear icon next to "About":
- Description: `High-performance file system querying with GitIgnore, Glob, and Regex pattern support for .NET`
- Topics: `dotnet`, `file-search`, `glob`, `gitignore`, `regex`, `nuget`, `csharp`, `pattern-matching`
- Website: (leave empty for now; add NuGet package URL after publishing)

---

### Step 2.4 — Create the GitHub Actions workflows

You already have workflow files under `.github/workflows/`. Verify these exist
and contain valid YAML:

| File | Purpose |
|------|---------|
| `build.yml` | Builds and runs tests on every push |
| `ci.yml` | Full CI pipeline including benchmarks |
| `nuget-publish.yml` | Publishes to NuGet on version tag push |
| `docs-build.yml` | Builds documentation |

For `nuget-publish.yml`, set it to trigger on tag push matching `v*.*.*`:

```yaml
on:
  push:
    tags:
      - 'v*.*.*'
```

---

### Step 2.5 — Add the NuGet API key as a GitHub secret

1. Go to https://www.nuget.org/account/apikeys
2. Create a new API key:
   - Name: `GitHub Actions — Jeninnet.FileQuery`
   - Glob pattern: `Jeninnet.FileQuery*`
   - Expiry: 365 days
3. Copy the key immediately (shown only once)
4. Go to your GitHub repository → Settings → Secrets and variables → Actions
5. Click **New repository secret**
   - Name: `NUGET_API_KEY`
   - Value: paste the key
6. Click **Add secret**

---

## Phase 3 — NuGet Package Preparation

### Step 3.1 — Verify package metadata in each `.csproj`

Open each of the three project files and confirm these properties are set:

**`src/Jeninnet.FileQuery/Jeninnet.FileQuery.csproj`**
```xml
<Version>1.0.0</Version>
<PackageId>Jeninnet.FileQuery</PackageId>
<Description>High-performance file system querying with GitIgnore, Glob, and Regex pattern support.</Description>
<PackageTags>file-search;file-query;glob;gitignore;regex;filesystem</PackageTags>
<RepositoryUrl>https://github.com/TarekNajem04/Jeninnet.FileQuery</RepositoryUrl>
<PackageProjectUrl>https://github.com/TarekNajem04/Jeninnet.FileQuery</PackageProjectUrl>
<PackageLicenseExpression>MIT</PackageLicenseExpression>
<PackageReadmeFile>README.md</PackageReadmeFile>
<PackageReleaseNotes>https://github.com/TarekNajem04/Jeninnet.FileQuery/releases/tag/v1.0.0</PackageReleaseNotes>
<IncludeSymbols>true</IncludeSymbols>
<SymbolPackageFormat>snupkg</SymbolPackageFormat>
<PublishRepositoryUrl>true</PublishRepositoryUrl>
<EmbedUntrackedSources>true</EmbedUntrackedSources>
```

Apply the same `RepositoryUrl`, `PackageProjectUrl`, `PackageLicenseExpression`,
`PackageReleaseNotes`, `IncludeSymbols`, `SymbolPackageFormat`, `PublishRepositoryUrl`,
and `EmbedUntrackedSources` to the CommandLine and DependencyInjection projects.

---

### Step 3.2 — Build the packages locally to verify

```powershell
dotnet pack src/Jeninnet.FileQuery/Jeninnet.FileQuery.csproj `
    -c Release `
    --output artifacts/packages

dotnet pack src/Jeninnet.FileQuery.CommandLine/Jeninnet.FileQuery.CommandLine.csproj `
    -c Release `
    --output artifacts/packages

dotnet pack src/Jeninnet.FileQuery.DependencyInjection/Jeninnet.FileQuery.DependencyInjection.csproj `
    -c Release `
    --output artifacts/packages
```

Verify `artifacts/packages/` contains six files:
```
Jeninnet.FileQuery.1.0.0.nupkg
Jeninnet.FileQuery.1.0.0.snupkg
Jeninnet.FileQuery.CommandLine.1.0.0.nupkg
Jeninnet.FileQuery.CommandLine.1.0.0.snupkg
Jeninnet.FileQuery.DependencyInjection.1.0.0.nupkg
Jeninnet.FileQuery.DependencyInjection.1.0.0.snupkg
```

---

### Step 3.3 — Inspect each package with NuGet Package Explorer (optional but recommended)

Download from: https://github.com/NuGetPackageExplorer/NuGetPackageExplorer

Open each `.nupkg` and verify:
- The `lib/net10.0/` folder contains the assembly
- `README.md` appears in the package root
- `LICENSE` appears in the package root
- Symbol package (`.snupkg`) is non-empty

---

## Phase 4 — First Release

### Step 4.1 — Push a version tag to trigger NuGet publish

```powershell
git tag v1.0.0
git push origin v1.0.0
```

The `nuget-publish.yml` workflow will:
1. Build in Release configuration
2. Run all tests
3. Pack the three projects
4. Push `.nupkg` and `.snupkg` to NuGet.org using `NUGET_API_KEY`

---

### Step 4.2 — Create a GitHub Release

1. Go to https://github.com/TarekNajem04/Jeninnet.FileQuery/releases/new
2. Tag: `v1.0.0` (select the tag you just pushed)
3. Title: `v1.0.0 — Initial Release`
4. Body: copy from `CHANGELOG.md`
5. Attach the six files from `artifacts/packages/` as release assets
6. Click **Publish release**

---

### Step 4.3 — Verify on NuGet.org

NuGet indexing typically takes 5–30 minutes. After that:

1. Go to https://www.nuget.org/packages/Jeninnet.FileQuery
2. Verify the README renders correctly
3. Verify the package description, tags, and repository URL are correct
4. Test installation in a scratch project:

```powershell
mkdir TestInstall
cd TestInstall
dotnet new console
dotnet add package Jeninnet.FileQuery --version 1.0.0
```

---

## Phase 5 — Post-Release Checklist

| Task | Done |
|------|------|
| Update `FUNDING.yml` with a sponsorship link if desired | ☐ |
| Pin the repository to your GitHub profile | ☐ |
| Add the NuGet badge to `README.md`: `[![NuGet](https://img.shields.io/nuget/v/Jeninnet.FileQuery)](https://www.nuget.org/packages/Jeninnet.FileQuery)` | ☐ |
| Add a CI badge: `[![CI](https://github.com/TarekNajem04/Jeninnet.FileQuery/actions/workflows/ci.yml/badge.svg)](...)` | ☐ |
| Fill in empty documentation files under `docs/` | ☐ |
| Open milestone `v1.1` on GitHub Issues | ☐ |

---

## Version Strategy

| Version | When to publish |
|---------|----------------|
| `1.0.0` | Initial release |
| `1.0.x` | Bug fixes only; no API changes |
| `1.1.0` | New features; no breaking changes |
| `2.0.0` | Breaking API changes |

Always update `<Version>` in all three `.csproj` files simultaneously.
Commit with message: `chore: bump version to X.Y.Z` before tagging.
