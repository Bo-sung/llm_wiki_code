<#
.SYNOPSIS
    Deploy llm-wiki to a remote Mac mini via SSH/SCP.

.DESCRIPTION
    Builds, publishes, uploads, and activates a new release on the Mac mini.
    shared/data and shared/.env are never overwritten.
    Seed files (prompts, rules, templates) are uploaded only when missing on remote.

.PARAMETER RemoteHost
    SSH hostname or IP address of the Mac mini.

.PARAMETER User
    SSH username on the Mac mini.

.PARAMETER Port
    SSH port (default: 22).

.PARAMETER Runtime
    .NET runtime identifier (default: osx-arm64).
    Use osx-x64 for Intel Mac mini.

.PARAMETER RemoteBase
    Base deployment directory on the Mac mini (default: ~/apps/llm-wiki).

.PARAMETER Configuration
    Build configuration (default: Release).

.PARAMETER SelfContained
    Publish as self-contained (default: false).
    Set to $true if .NET is not installed on the Mac mini.

.PARAMETER SkipTests
    Skip local dotnet test before deploy.

.PARAMETER SkipSmokeTest
    Skip remote smoke test after deploy.

.EXAMPLE
    .\deploy-to-mac.ps1 -RemoteHost 192.168.1.100 -User myuser

.EXAMPLE
    .\deploy-to-mac.ps1 -RemoteHost my-mac.local -User myuser -Port 2222 -Runtime osx-x64

.EXAMPLE
    .\deploy-to-mac.ps1 -RemoteHost my-mac.local -User myuser -SkipTests -SelfContained
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$RemoteHost,

    [Parameter(Mandatory = $true)]
    [string]$User,

    [int]$Port = 22,

    [ValidateSet("osx-arm64", "osx-x64")]
    [string]$Runtime = "osx-arm64",

    [string]$RemoteBase = "~/apps/llm-wiki",

    [ValidateSet("Release", "Debug")]
    [string]$Configuration = "Release",

    [switch]$SelfContained,

    [switch]$SkipTests,

    [switch]$SkipSmokeTest,

    # Full path to dotnet on the Mac mini.
    # Auto-detected from /usr/local/share/dotnet and /opt/homebrew/bin when empty.
    # Set this if dotnet is installed to a non-standard location.
    # Example: -RemoteDotnetExe "/home/user/.dotnet/dotnet"
    [string]$RemoteDotnetExe = "",

    # ── Public vault Git configuration (all optional) ─────────────────────────
    # These write shared/config/public-vault.env on the Mac mini.
    # Git credentials (tokens, SSH keys) are NOT passed here.
    # Authentication must be configured on the Mac mini separately.

    # GitHub remote URL for the public vault repo.
    # Example: git@github.com:user/llm-wiki-vault-public.git
    [string]$PublicVaultRepoUrl = "",

    # Default branch for the public vault repo.
    [string]$PublicVaultBranch = "main",

    # Absolute path on Mac mini for the public vault directory.
    # Default: $RemoteBase/public-vault (resolved at runtime).
    [string]$PublicVaultPath = "",

    # Git author name to configure in the vault repo.
    [string]$GitAuthorName = "",

    # Git author email to configure in the vault repo.
    [string]$GitAuthorEmail = "",

    # When set, initialises the public vault directory:
    # git init, set origin remote, create .gitignore and README.md.
    # Does NOT push, pull, or copy shared/data.
    [switch]$SetupPublicVault
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# ─── Helpers ────────────────────────────────────────────────────────────────

function Write-Step([string]$msg) {
    Write-Host "`n==> $msg" -ForegroundColor Cyan
}

function Write-OK([string]$msg) {
    Write-Host "[OK]   $msg" -ForegroundColor Green
}

function Write-Warn([string]$msg) {
    Write-Host "[WARN] $msg" -ForegroundColor Yellow
}

function Write-Fail([string]$msg) {
    Write-Host "[FAIL] $msg" -ForegroundColor Red
}

function Invoke-SSH([string]$cmd) {
    $result = ssh -p $Port -o "BatchMode=yes" -o "ConnectTimeout=10" "${User}@${RemoteHost}" $cmd
    if ($LASTEXITCODE -ne 0) {
        throw "SSH command failed (exit $LASTEXITCODE): $cmd"
    }
    return $result
}

function Invoke-SSH-NoThrow([string]$cmd) {
    $result = ssh -p $Port -o "BatchMode=yes" -o "ConnectTimeout=10" "${User}@${RemoteHost}" $cmd 2>&1
    return @{ Output = $result; ExitCode = $LASTEXITCODE }
}

# Upload a single seed file only when the remote path does not exist.
# Never overwrites. Warns if local file is missing.
function Copy-SeedFile([string]$localPath, [string]$remotePath, [string]$label) {
    if (-not (Test-Path $localPath)) {
        Write-Warn "Seed local missing, skip: $label"
        return
    }
    $check = Invoke-SSH-NoThrow "test -f $remotePath && echo exists || echo missing"
    if ($check.Output -match "exists") {
        Write-OK "Seed exists, skip:   $label"
        return
    }
    Write-Host "  Seed missing, upload: $label"
    $scpArgs = @("-P", $Port, $localPath, "${User}@${RemoteHost}:${remotePath}")
    & scp @scpArgs
    if ($LASTEXITCODE -ne 0) {
        Write-Warn "Seed upload failed (non-fatal): $label"
    } else {
        Write-OK "Seed uploaded:       $label"
    }
}

# ─── Script root (project root = two levels up from this file) ───────────────

$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\.." )).Path
$Timestamp   = (Get-Date -Format "yyyyMMdd-HHmmss")
$ArtifactDir = Join-Path $ProjectRoot "artifacts\publish\$Runtime"
$LocalData   = Join-Path $ProjectRoot "data"

Write-Step "llm-wiki deploy — $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
Write-Host "  host       : ${User}@${RemoteHost}:${Port}"
Write-Host "  runtime    : $Runtime"
Write-Host "  self-cont  : $SelfContained"
Write-Host "  config     : $Configuration"
Write-Host "  remote     : $RemoteBase"
Write-Host "  timestamp  : $Timestamp"

# ─── Step 1/8: Local tests ───────────────────────────────────────────────────

if (-not $SkipTests) {
    Write-Step "1/9  Running local tests"
    Push-Location $ProjectRoot
    try {
        dotnet test --configuration $Configuration --verbosity minimal
        if ($LASTEXITCODE -ne 0) {
            Write-Fail "Tests failed. Aborting deploy."
            exit 1
        }
        Write-OK "Tests passed"
    }
    finally {
        Pop-Location
    }
} else {
    Write-Warn "1/9  Skipping local tests (-SkipTests)"
}

# ─── Step 2/8: Publish ──────────────────────────────────────────────────────

Write-Step "2/9  Publishing"

if (Test-Path $ArtifactDir) {
    Remove-Item $ArtifactDir -Recurse -Force
}

$SelfContainedFlag = if ($SelfContained) { "true" } else { "false" }
$CliProject        = Join-Path $ProjectRoot "src\LlmWiki.Cli\LlmWiki.Cli.csproj"
$CaptureApiProject = Join-Path $ProjectRoot "src\LlmWiki.CaptureApi\LlmWiki.CaptureApi.csproj"

Push-Location $ProjectRoot
try {
    dotnet publish $CliProject `
        -c $Configuration `
        -r $Runtime `
        --self-contained $SelfContainedFlag `
        -o $ArtifactDir
    if ($LASTEXITCODE -ne 0) {
        Write-Fail "Publish (Cli) failed. Aborting."
        exit 1
    }

    dotnet publish $CaptureApiProject `
        -c $Configuration `
        -r $Runtime `
        --self-contained $SelfContainedFlag `
        -o $ArtifactDir
    if ($LASTEXITCODE -ne 0) {
        Write-Fail "Publish (CaptureApi) failed. Aborting."
        exit 1
    }
}
finally {
    Pop-Location
}

$ArtifactCount = (Get-ChildItem $ArtifactDir -Recurse -File).Count
Write-OK "Published $ArtifactCount file(s) to $ArtifactDir"

# ─── Step 3/8: Create remote directories ────────────────────────────────────

Write-Step "3/9  Creating remote directories"

$RemoteRelease = "$RemoteBase/releases/$Timestamp"
$SharedData    = "$RemoteBase/shared/data"

# Release dir + shared/logs
Invoke-SSH "mkdir -p $RemoteRelease $RemoteBase/shared/logs"

# shared/data subtree — mkdir -p only, never delete or overwrite.
# processed/ and failed/ are created on demand by InboxMover; not pre-created here.
$dataDirs = @(
    "$SharedData/Inbox/links",
    "$SharedData/Inbox/raw_clips",
    "$SharedData/Inbox/mobile",
    "$SharedData/Notes/References",
    "$SharedData/Notes/Projects",
    "$SharedData/Notes/Tech",
    "$SharedData/Notes/AI",
    "$SharedData/Notes/GameDev",
    "$SharedData/Sources/webpages",
    "$SharedData/Sources/youtube",
    "$SharedData/Sources/pdfs",
    "$SharedData/System/prompts",
    "$SharedData/Templates",
    "$SharedData/Index"
)
Invoke-SSH ("mkdir -p " + ($dataDirs -join " "))
Write-OK "Remote directories ready (shared/data structure preserved)"

# Check .env existence — warn only, do not create
$EnvCheck = Invoke-SSH-NoThrow "test -f $RemoteBase/shared/.env && echo exists || echo missing"
if ($EnvCheck.Output -match "missing") {
    Write-Warn "shared/.env not found on Mac mini."
    Write-Warn "Create it manually: $RemoteBase/shared/.env"
    Write-Warn "Use .env.example as a template."
}

# ─── Step 4/9: Ensure .NET runtime on remote ────────────────────────────────
#
# Detection order:
#   1. -RemoteDotnetExe (user-specified, validated immediately)
#   2. /usr/local/share/dotnet/dotnet  (official macOS installer)
#   3. /opt/homebrew/bin/dotnet        (Homebrew, Apple Silicon)
#   4. $HOME/.dotnet/dotnet            (dotnet-install.sh default output)
#   5. Not found → install .NET 8 runtime via dotnet-install.sh → ~/.dotnet/dotnet

Write-Step "4/9  Ensuring .NET runtime on remote"

$RemoteDotnetExePath = $null  # resolved here; used in smoke test and Done section

if ($SelfContained) {
    Write-OK "self-contained deploy — .NET runtime not required on remote"
} else {
    # Resolve remote home dir (needed to expand ~/.dotnet without relying on PATH)
    $homeRes = Invoke-SSH-NoThrow "echo `$HOME"
    $remoteHome = ($homeRes.Output | Select-Object -First 1).Trim()

    if ($RemoteDotnetExe) {
        # User-specified path: validate immediately
        $chk = Invoke-SSH-NoThrow "test -x '$RemoteDotnetExe' && echo ok || echo no"
        if ($chk.Output -match "ok") {
            $RemoteDotnetExePath = $RemoteDotnetExe
            Write-OK "dotnet (user-specified): $RemoteDotnetExePath"
        } else {
            Write-Fail "Specified -RemoteDotnetExe not found or not executable: $RemoteDotnetExe"
            exit 1
        }
    } else {
        $probePaths = @(
            "/usr/local/share/dotnet/dotnet",  # official macOS installer
            "/opt/homebrew/bin/dotnet",         # Homebrew (Apple Silicon)
            "$remoteHome/.dotnet/dotnet"        # dotnet-install.sh default
        )
        foreach ($probe in $probePaths) {
            $chk = Invoke-SSH-NoThrow "test -x '$probe' && echo ok || echo no"
            if ($chk.Output -match "ok") {
                $RemoteDotnetExePath = $probe
                Write-OK "dotnet found: $RemoteDotnetExePath"
                break
            }
        }

        if (-not $RemoteDotnetExePath) {
            Write-Host "  .NET runtime not found. Installing .NET 8 runtime via official script ..."
            Write-Host "  Target: $remoteHome/.dotnet  (may take 1-3 minutes)"

            $dotnetInstallDir = "$remoteHome/.dotnet"
            $installCmd = (
                "curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh && " +
                "chmod +x /tmp/dotnet-install.sh && " +
                "/tmp/dotnet-install.sh --channel 8.0 --runtime dotnet --install-dir '$dotnetInstallDir' && " +
                "rm /tmp/dotnet-install.sh && echo INSTALL_OK"
            )
            $install = Invoke-SSH-NoThrow $installCmd
            if ($install.Output -match "INSTALL_OK") {
                $RemoteDotnetExePath = "$dotnetInstallDir/dotnet"
                Write-OK ".NET 8 runtime installed: $RemoteDotnetExePath"
            } else {
                Write-Fail ".NET 8 runtime installation failed"
                Write-Host $install.Output
                exit 1
            }
        }
    }

    # Verify: use --list-runtimes (works with runtime-only install).
    # dotnet --version requires an SDK and fails when only the runtime is installed.
    $verRes = Invoke-SSH-NoThrow "'$RemoteDotnetExePath' --list-runtimes 2>&1"
    $runtimeLine = ($verRes.Output | Where-Object { $_ -match "NETCore" } | Select-Object -First 1).Trim()
    if ($runtimeLine) {
        Write-OK "dotnet runtime: $runtimeLine"
    } else {
        Write-Fail "dotnet runtime not found at $RemoteDotnetExePath"
        Write-Host $verRes.Output
        exit 1
    }
}

# ─── Step 5/9: Initialize seed files ────────────────────────────────────────
#
# Policy:
#   - Upload only when the remote file does not exist.
#   - Never overwrite existing remote files (user may have edited them).
#   - data/ as a whole is never copied; only these specific seed files.

Write-Step "5/9  Initializing seed files"

$seedFiles = @(
    @{
        Local  = Join-Path $LocalData "System\prompts\refine_note.md"
        Remote = "$SharedData/System/prompts/refine_note.md"
        Label  = "System/prompts/refine_note.md"
    },
    @{
        Local  = Join-Path $LocalData "System\rules.md"
        Remote = "$SharedData/System/rules.md"
        Label  = "System/rules.md"
    },
    @{
        Local  = Join-Path $LocalData "System\taxonomy.md"
        Remote = "$SharedData/System/taxonomy.md"
        Label  = "System/taxonomy.md"
    },
    @{
        Local  = Join-Path $LocalData "Templates\note_template.md"
        Remote = "$SharedData/Templates/note_template.md"
        Label  = "Templates/note_template.md"
    },
    @{
        Local  = Join-Path $LocalData "Templates\source_template.md"
        Remote = "$SharedData/Templates/source_template.md"
        Label  = "Templates/source_template.md"
    }
)

foreach ($seed in $seedFiles) {
    Copy-SeedFile -localPath $seed.Local -remotePath $seed.Remote -label $seed.Label
}

# ─── Step 5/8: Upload publish artifacts ─────────────────────────────────────

Write-Step "6/9  Uploading publish artifacts"

# On Windows, scp -r copies the directory including its name.
$ScpSource = $ArtifactDir.TrimEnd('\')

$scpArgs = @("-P", $Port, "-r", $ScpSource, "${User}@${RemoteHost}:${RemoteRelease}/")
& scp @scpArgs
if ($LASTEXITCODE -ne 0) {
    Write-Fail "SCP upload failed. Aborting."
    exit 1
}

Write-OK "Upload complete → $RemoteRelease"

# ─── Step 7/9: Update current symlink + upload scripts/mac ──────────────────

Write-Step "7/9  Updating current symlink"

# scp -r copies the directory as a subdirectory. Detect layout.
$PublishedDirName    = Split-Path $ArtifactDir -Leaf
$ActualReleasePath   = "$RemoteRelease/$PublishedDirName"
$EffectiveCurrentPath = $RemoteRelease  # updated below

$DirCheck = Invoke-SSH-NoThrow "test -f $ActualReleasePath/LlmWiki.Cli.dll && echo sub || echo flat"
if ($DirCheck.Output -match "sub") {
    Invoke-SSH "ln -sfn $ActualReleasePath $RemoteBase/current"
    $EffectiveCurrentPath = $ActualReleasePath
    Write-OK "current → $ActualReleasePath"
} else {
    Invoke-SSH "ln -sfn $RemoteRelease $RemoteBase/current"
    $EffectiveCurrentPath = $RemoteRelease
    Write-OK "current → $RemoteRelease"
}

# Upload scripts/mac/ alongside the app binaries so launchd scripts
# are available at current/scripts/mac/ after deployment.
$ScriptsMacDir = Join-Path $ProjectRoot "scripts\mac"
if (Test-Path $ScriptsMacDir) {
    Write-Host "  Uploading scripts/mac ..."
    Invoke-SSH "mkdir -p '$EffectiveCurrentPath/scripts'"
    $scpMacArgs = @("-P", $Port, "-r", $ScriptsMacDir, "${User}@${RemoteHost}:${EffectiveCurrentPath}/scripts/")
    & scp @scpMacArgs
    if ($LASTEXITCODE -ne 0) {
        Write-Warn "scripts/mac upload failed (non-fatal — launchd scripts will not be present)"
    } else {
        Write-OK "scripts/mac → $EffectiveCurrentPath/scripts/mac"
    }
} else {
    Write-Warn "scripts/mac not found locally — skipping"
}

# ─── Step 7/8: Verify shared data path ──────────────────────────────────────

Write-Step "8/9  Verifying shared data path"

$DataCheck = Invoke-SSH-NoThrow "test -d $SharedData && echo ok || echo missing"
if ($DataCheck.Output -match "ok") {
    Write-OK "shared/data exists"
} else {
    Write-Warn "shared/data does not exist — creating now"
    Invoke-SSH "mkdir -p $SharedData"
}

# ─── Step 8/8: Smoke test ────────────────────────────────────────────────────

if ($SkipSmokeTest) {
    Write-Warn "9/9  Skipping smoke test (-SkipSmokeTest)"
} else {
    Write-Step "9/9  Running remote smoke test"

    # current symlink
    $r = Invoke-SSH-NoThrow "test -L $RemoteBase/current && echo ok || echo missing"
    if ($r.Output -match "ok") { Write-OK "current symlink exists" }
    else { Write-Fail "current symlink missing"; exit 1 }

    # shared/data
    $r = Invoke-SSH-NoThrow "test -d $SharedData && echo ok || echo missing"
    if ($r.Output -match "ok") { Write-OK "shared/data exists" }
    else { Write-Fail "shared/data missing"; exit 1 }

    # shared/data/Inbox/links
    $r = Invoke-SSH-NoThrow "test -d $SharedData/Inbox/links && echo ok || echo missing"
    if ($r.Output -match "ok") { Write-OK "shared/data/Inbox/links exists" }
    else { Write-Fail "shared/data/Inbox/links missing"; exit 1 }

    # shared/data/Notes/References
    $r = Invoke-SSH-NoThrow "test -d $SharedData/Notes/References && echo ok || echo missing"
    if ($r.Output -match "ok") { Write-OK "shared/data/Notes/References exists" }
    else { Write-Fail "shared/data/Notes/References missing"; exit 1 }

    # shared/data/System/prompts
    $r = Invoke-SSH-NoThrow "test -d $SharedData/System/prompts && echo ok || echo missing"
    if ($r.Output -match "ok") { Write-OK "shared/data/System/prompts exists" }
    else { Write-Fail "shared/data/System/prompts missing"; exit 1 }

    # refine_note.md — warn only
    $r = Invoke-SSH-NoThrow "test -f $SharedData/System/prompts/refine_note.md && echo ok || echo missing"
    if ($r.Output -match "ok") { Write-OK "refine_note.md exists" }
    else { Write-Warn "refine_note.md not found — Gemini prompt will use built-in fallback" }

    # shared/.env — warn only
    $r = Invoke-SSH-NoThrow "test -f $RemoteBase/shared/.env && echo ok || echo missing"
    if ($r.Output -match "ok") { Write-OK "shared/.env exists" }
    else { Write-Warn "shared/.env not found — app runs in fallback mode" }

    # app binary
    if ($SelfContained) {
        $r = Invoke-SSH-NoThrow "test -f $RemoteBase/current/LlmWiki.Cli && echo ok || echo missing"
    } else {
        $r = Invoke-SSH-NoThrow "test -f $RemoteBase/current/LlmWiki.Cli.dll && echo ok || echo missing"
    }
    if ($r.Output -match "ok") { Write-OK "App binary found" }
    else { Write-Fail "App binary not found at $RemoteBase/current/"; exit 1 }

    # scripts/mac/install-launchd-watch.sh
    $r = Invoke-SSH-NoThrow "test -f $RemoteBase/current/scripts/mac/install-launchd-watch.sh && echo ok || echo missing"
    if ($r.Output -match "ok") { Write-OK "install-launchd-watch.sh found" }
    else { Write-Warn "install-launchd-watch.sh not found at current/scripts/mac/ — launchd setup may fail" }

    # CaptureApi binary
    $r = Invoke-SSH-NoThrow "test -f $RemoteBase/current/LlmWiki.CaptureApi.dll && echo ok || echo missing"
    if ($r.Output -match "ok") { Write-OK "LlmWiki.CaptureApi.dll found" }
    else { Write-Warn "LlmWiki.CaptureApi.dll not found — Capture API not deployed" }

    # process-once --dry-run (does not touch real files)
    # Pass LLM_WIKI_ENV_FILE so the app loads shared/.env without shell sourcing.
    # If .env is absent the app warns and continues in fallback mode — not a failure.
    $sharedEnvFile = "$RemoteBase/shared/.env"
    Write-Host "  Running: process-once --dry-run"
    if ($SelfContained) {
        $DryRun = Invoke-SSH-NoThrow "cd $RemoteBase/current && LLM_WIKI_ENV_FILE=$sharedEnvFile LLM_WIKI_ROOT=$SharedData ./LlmWiki.Cli process-once --dry-run 2>&1"
    } else {
        $DryRun = Invoke-SSH-NoThrow "cd $RemoteBase/current && LLM_WIKI_ENV_FILE=$sharedEnvFile LLM_WIKI_ROOT=$SharedData '$RemoteDotnetExePath' LlmWiki.Cli.dll process-once --dry-run 2>&1"
    }

    if ($DryRun.ExitCode -eq 0) {
        Write-OK "process-once --dry-run succeeded"
        Write-Host $DryRun.Output
    } else {
        Write-Fail "process-once --dry-run failed (exit $($DryRun.ExitCode))"
        Write-Host $DryRun.Output
        exit 1
    }
}

# ─── Optional: Public vault configuration ────────────────────────────────────
#
# Runs when any public vault param is supplied OR when -SetupPublicVault is set.
# Writes shared/config/public-vault.env on the Mac mini.
# NEVER transmits or stores git credentials (tokens, keys, passwords).

$hasVaultParams = ($PublicVaultRepoUrl -or $PublicVaultBranch -ne "main" -or
                   $PublicVaultPath -or $GitAuthorName -or $GitAuthorEmail -or $SetupPublicVault)

if ($hasVaultParams) {

    # Resolve effective paths (params may be empty — apply defaults here)
    $effectiveVaultPath = if ($PublicVaultPath) { $PublicVaultPath } else { "$RemoteBase/public-vault" }
    $effectiveBranch    = if ($PublicVaultBranch) { $PublicVaultBranch } else { "main" }

    Write-Step "Optional: Writing public vault config"

    $configDir  = "$RemoteBase/shared/config"
    $configFile = "$configDir/public-vault.env"
    Invoke-SSH "mkdir -p '$configDir'"

    # Backup existing config if present
    $bkCheck = Invoke-SSH-NoThrow "test -f '$configFile' && echo exists || echo missing"
    if ($bkCheck.Output -match "exists") {
        $bkPath = "$configFile.$(Get-Date -Format 'yyyyMMdd-HHmmss').bak"
        Invoke-SSH "cp '$configFile' '$bkPath'"
        Write-OK "Backed up existing config → $bkPath"
    }

    # Write public-vault.env (no credentials, no secrets)
    $envLines = @(
        "PUBLIC_VAULT_REPO_URL=$PublicVaultRepoUrl",
        "PUBLIC_VAULT_BRANCH=$effectiveBranch",
        "PUBLIC_VAULT_PATH=$effectiveVaultPath",
        "GIT_AUTHOR_NAME=$GitAuthorName",
        "GIT_AUTHOR_EMAIL=$GitAuthorEmail"
    )
    $envContent = $envLines -join "\n"
    Invoke-SSH "printf '$envContent\n' > '$configFile'"
    Write-OK "public-vault.env → $configFile"

    if ($SetupPublicVault) {
        Write-Step "Optional: Initialising public vault directory"

        # Create directory
        Invoke-SSH "mkdir -p '$effectiveVaultPath'"
        Write-OK "Directory: $effectiveVaultPath"

        # git init if not already a repo
        $gitCheck = Invoke-SSH-NoThrow "test -d '$effectiveVaultPath/.git' && echo yes || echo no"
        if ($gitCheck.Output -match "no") {
            Invoke-SSH "git -C '$effectiveVaultPath' init -b '$effectiveBranch'"
            Write-OK "git init (branch: $effectiveBranch)"
        } else {
            Write-OK "Already a git repository"
        }

        # Set git author config
        if ($GitAuthorName) {
            Invoke-SSH "git -C '$effectiveVaultPath' config user.name '$GitAuthorName'"
            Write-OK "git user.name: $GitAuthorName"
        }
        if ($GitAuthorEmail) {
            Invoke-SSH "git -C '$effectiveVaultPath' config user.email '$GitAuthorEmail'"
            Write-OK "git user.email: $GitAuthorEmail"
        }

        # Set origin remote
        if ($PublicVaultRepoUrl) {
            $remoteCheck = Invoke-SSH-NoThrow "git -C '$effectiveVaultPath' remote get-url origin 2>/dev/null && echo exists || echo missing"
            if ($remoteCheck.Output -match "missing") {
                Invoke-SSH "git -C '$effectiveVaultPath' remote add origin '$PublicVaultRepoUrl'"
                Write-OK "remote added: origin → $PublicVaultRepoUrl"
            } else {
                Invoke-SSH "git -C '$effectiveVaultPath' remote set-url origin '$PublicVaultRepoUrl'"
                Write-OK "remote updated: origin → $PublicVaultRepoUrl"
            }
        }

        # Create .gitignore if not present
        $giCheck = Invoke-SSH-NoThrow "test -f '$effectiveVaultPath/.gitignore' && echo exists || echo missing"
        if ($giCheck.Output -match "missing") {
            $gitignore = "Inbox/\nSources/\nSystem/private/\n.env\n*.log\n.DS_Store\n.obsidian/workspace*\n.obsidian/cache/\n.trash/\n"
            Invoke-SSH "printf '$gitignore' > '$effectiveVaultPath/.gitignore'"
            Write-OK ".gitignore created"
        } else {
            Write-OK ".gitignore already exists — not overwritten"
        }

        # Create README.md if not present
        $readmeCheck = Invoke-SSH-NoThrow "test -f '$effectiveVaultPath/README.md' && echo exists || echo missing"
        if ($readmeCheck.Output -match "missing") {
            $readme = "# LLM Wiki — Public Vault\n\nPublic knowledge notes generated by llm-wiki pipeline.\n"
            Invoke-SSH "printf '$readme' > '$effectiveVaultPath/README.md'"
            Write-OK "README.md created"
        } else {
            Write-OK "README.md already exists — not overwritten"
        }

        Write-Host ""
        Write-Host "  Public vault initialised: $effectiveVaultPath"
        Write-Host "  Next: authenticate git on Mac mini, then 'git push -u origin $effectiveBranch'"
    }
}

# ─── Done ────────────────────────────────────────────────────────────────────

Write-Step "Deploy complete"
Write-Host ""
Write-Host "  Release   : $RemoteRelease"
Write-Host "  Current   : $RemoteBase/current"
Write-Host "  Shared    : $RemoteBase/shared/"
Write-Host ""
$envPrefix = "LLM_WIKI_ENV_FILE=$RemoteBase/shared/.env LLM_WIKI_ROOT=$SharedData"
Write-Host "Manual run on Mac mini:"
Write-Host "  cd $RemoteBase/current"
if ($SelfContained) {
    Write-Host "  $envPrefix ./LlmWiki.Cli process-once"
    Write-Host "  $envPrefix ./LlmWiki.Cli watch"
} elseif ($RemoteDotnetExePath) {
    Write-Host "  $envPrefix '$RemoteDotnetExePath' LlmWiki.Cli.dll process-once"
    Write-Host "  $envPrefix '$RemoteDotnetExePath' LlmWiki.Cli.dll watch"
} else {
    Write-Host "  $envPrefix dotnet LlmWiki.Cli.dll process-once"
    Write-Host "  $envPrefix dotnet LlmWiki.Cli.dll watch"
}
Write-Host ""
Write-Host "Rollback:"
Write-Host "  ls $RemoteBase/releases"
Write-Host "  ln -sfn $RemoteBase/releases/{previous} $RemoteBase/current"
