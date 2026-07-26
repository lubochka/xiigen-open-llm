param(
    [string]$ExtensionRoot = (Join-Path $env:USERPROFILE 'Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills'),
    [string]$EvidenceRoot = (Join-Path (Split-Path -Parent $PSScriptRoot) 'artifacts\extension-runtime-token-quality-learnability\phase-10-cursor-full-extension-live-proof'),
    [int]$TimeoutSeconds = 240
)

$ErrorActionPreference = "Stop"

function Ensure-Directory {
    param([Parameter(Mandatory = $true)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        New-Item -ItemType Directory -Force -Path $Path | Out-Null
    }
}

function Write-Utf8NoBomLfText {
    param(
        [Parameter(Mandatory = $true)][string]$Content,
        [Parameter(Mandatory = $true)][string]$Path
    )

    Ensure-Directory -Path (Split-Path -Parent $Path)
    $normalized = $Content -replace "`r`n?", "`n"
    $normalized = $normalized -replace "^\uFEFF", ""
    $normalized = [regex]::Replace($normalized, "(?m)[ `t]+$", "")
    if (-not $normalized.EndsWith("`n")) {
        $normalized += "`n"
    }

    $utf8NoBom = New-Object System.Text.UTF8Encoding -ArgumentList $false
    [System.IO.File]::WriteAllText($Path, $normalized, $utf8NoBom)
}

function Write-Json {
    param(
        [Parameter(Mandatory = $true)]$Value,
        [Parameter(Mandatory = $true)][string]$Path
    )

    $json = $Value | ConvertTo-Json -Depth 32
    Write-Utf8NoBomLfText -Content $json -Path $Path
}

function Normalize-Phase10TextEvidenceFile {
    param([Parameter(Mandatory = $true)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        return
    }

    $strictUtf8 = New-Object System.Text.UTF8Encoding -ArgumentList $false, $true
    $bytes = [System.IO.File]::ReadAllBytes($Path)
    $text = $strictUtf8.GetString($bytes)
    Write-Utf8NoBomLfText -Content $text -Path $Path
}

function Normalize-Phase10HostEvidence {
    param([Parameter(Mandatory = $true)][string]$HostRunRoot)

    $selectedFiles = @(
        (Join-Path $HostRunRoot "cursor-extension-host.stdout.log"),
        (Join-Path $HostRunRoot "cursor-extension-host.stderr.log"),
        (Join-Path $HostRunRoot "phase10-cursor-proof-report.json"),
        (Join-Path $HostRunRoot "phase10-cursor-proof-report.ru.md")
    )

    $extensionHostEvidence = Join-Path $HostRunRoot "extension-host-evidence"
    if (Test-Path -LiteralPath $extensionHostEvidence) {
        $selectedFiles += @(Get-ChildItem -LiteralPath $extensionHostEvidence -File -Recurse | ForEach-Object { $_.FullName })
    }

    foreach ($path in $selectedFiles) {
        Normalize-Phase10TextEvidenceFile -Path $path
    }
}

function Add-Phase10CommitReadyEvidencePath {
    param(
        [Parameter(Mandatory = $true)]$Paths,
        [Parameter(Mandatory = $true)][string]$Path
    )

    if (Test-Path -LiteralPath $Path) {
        $Paths.Add([System.IO.Path]::GetFullPath($Path)) | Out-Null
    }
}

function Get-Phase10CommitReadyEvidencePaths {
    param(
        [Parameter(Mandatory = $true)][string]$RunRoot,
        [Parameter(Mandatory = $true)]$Attempts,
        [Parameter(Mandatory = $true)][string]$SummaryPath
    )

    $paths = New-Object "System.Collections.Generic.List[string]"
    $paths.Add([System.IO.Path]::GetFullPath($SummaryPath)) | Out-Null

    foreach ($attempt in $Attempts) {
        $hostRunRoot = Join-Path $RunRoot $attempt.cli
        Add-Phase10CommitReadyEvidencePath -Paths $paths -Path (Join-Path $hostRunRoot "$($attempt.cli)-extension-host.stdout.log")
        Add-Phase10CommitReadyEvidencePath -Paths $paths -Path (Join-Path $hostRunRoot "$($attempt.cli)-extension-host.stderr.log")
        Add-Phase10CommitReadyEvidencePath -Paths $paths -Path (Join-Path $hostRunRoot "phase10-cursor-proof-report.json")
        Add-Phase10CommitReadyEvidencePath -Paths $paths -Path (Join-Path $hostRunRoot "phase10-cursor-proof-report.ru.md")

        $extensionHostEvidence = Join-Path $hostRunRoot "extension-host-evidence"
        if (Test-Path -LiteralPath $extensionHostEvidence) {
            Get-ChildItem -LiteralPath $extensionHostEvidence -File -Recurse |
                ForEach-Object { $paths.Add([System.IO.Path]::GetFullPath($_.FullName)) | Out-Null }
        }
    }

    @($paths | Sort-Object -Unique)
}

function Get-Phase10ExcludedVolatileEvidenceDirs {
    param(
        [Parameter(Mandatory = $true)][string]$RunRoot,
        [Parameter(Mandatory = $true)]$Attempts
    )

    $paths = foreach ($attempt in $Attempts) {
        $hostRunRoot = Join-Path $RunRoot $attempt.cli
        Join-Path $hostRunRoot "user-data"
        Join-Path $hostRunRoot "extensions"
    }

    @($paths | ForEach-Object { [System.IO.Path]::GetFullPath($_) } | Sort-Object -Unique)
}

function Invoke-Phase10Host {
    param(
        [Parameter(Mandatory = $true)][string]$CliName,
        [Parameter(Mandatory = $true)][string]$HostLabel,
        [Parameter(Mandatory = $true)][string]$RunRoot,
        [Parameter(Mandatory = $true)][string]$TestPath,
        [Parameter(Mandatory = $true)][string]$RunId
    )

    $preferredExe = if ($CliName -eq "cursor") {
        (Join-Path $env:LOCALAPPDATA 'Programs\cursor\Cursor.exe')
    } elseif ($CliName -eq "code") {
        (Join-Path $env:LOCALAPPDATA 'Programs\Microsoft VS Code\Code.exe')
    } else {
        ""
    }
    $command = Get-Command $CliName -ErrorAction SilentlyContinue
    $executablePath = if ($preferredExe -and (Test-Path -LiteralPath $preferredExe)) {
        $preferredExe
    } elseif ($null -ne $command) {
        $command.Source
    } else {
        ""
    }
    if ([string]::IsNullOrWhiteSpace($executablePath)) {
        return [pscustomobject][ordered]@{
            cli = $CliName
            host_label = $HostLabel
            attempted = $false
            exit_code = 127
            passed = $false
            error = "$CliName was not found on PATH."
        }
    }

    $hostRunRoot = Join-Path $RunRoot $CliName
    $userDataDir = Join-Path $hostRunRoot "user-data"
    $extensionsDir = Join-Path $hostRunRoot "extensions"
    $stdoutPath = Join-Path $hostRunRoot "$CliName-extension-host.stdout.log"
    $stderrPath = Join-Path $hostRunRoot "$CliName-extension-host.stderr.log"
    Ensure-Directory -Path $hostRunRoot
    Ensure-Directory -Path $userDataDir
    Ensure-Directory -Path $extensionsDir

    $env:PHASE10_CURSOR_PROOF_DIR = $hostRunRoot
    $env:PHASE10_CURSOR_RUN_ID = $RunId
    $env:PHASE10_HOST_CLI = $CliName
    $env:PHASE10_HOST_LABEL = $HostLabel

    $arguments = @(
        "--new-window",
        "--user-data-dir", $userDataDir,
        "--extensions-dir", $extensionsDir,
        "--extensionDevelopmentPath", $ExtensionRoot,
        "--extensionTestsPath", $TestPath,
        "--disable-workspace-trust",
        "--skip-welcome",
        "--skip-release-notes",
        "--skip-onboarding",
        "--disable-telemetry",
        "--disable-updates",
        "--disable-gpu",
        "--disable-chromium-sandbox",
        "--log", "trace",
        "--headless",
        $ExtensionRoot
    )
    $argumentLine = ($arguments | ForEach-Object {
            $escaped = [string]$_ -replace '"', '\"'
            "`"$escaped`""
        }) -join " "

    $process = Start-Process -FilePath $executablePath -ArgumentList $argumentLine -RedirectStandardOutput $stdoutPath -RedirectStandardError $stderrPath -WindowStyle Hidden -PassThru
    $completed = $process.WaitForExit($TimeoutSeconds * 1000)
    if (-not $completed) {
        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
        return [pscustomobject][ordered]@{
            cli = $CliName
            host_label = $HostLabel
            attempted = $true
            exit_code = 124
            passed = $false
            timed_out = $true
            stdout_path = $stdoutPath
            stderr_path = $stderrPath
            report_path = Join-Path $hostRunRoot "phase10-cursor-proof-report.json"
        }
    }

    Normalize-Phase10HostEvidence -HostRunRoot $hostRunRoot

    $exitCode = $process.ExitCode
    $reportPath = Join-Path $hostRunRoot "phase10-cursor-proof-report.json"
    $reportExists = Test-Path -LiteralPath $reportPath
    $report = $null
    if ($reportExists) {
        $report = Get-Content -Raw -LiteralPath $reportPath | ConvertFrom-Json
    }
    $passedFromReport = $reportExists -and $report.passed -eq $true
    $exitCodeAccepted = $exitCode -eq 0 -or $null -eq $exitCode

    return [pscustomobject][ordered]@{
        cli = $CliName
        host_label = $HostLabel
        executable_path = $executablePath
        attempted = $true
        exit_code = $exitCode
        passed = ($passedFromReport -and $exitCodeAccepted)
        exit_code_accepted = $exitCodeAccepted
        timed_out = $false
        stdout_path = $stdoutPath
        stderr_path = $stderrPath
        report_path = $reportPath
        report_exists = $reportExists
        verdict = if ($reportExists) { $report.verdict } else { "missing_report" }
    }
}

$resolvedExtensionRoot = [System.IO.Path]::GetFullPath($ExtensionRoot)
$resolvedEvidenceRoot = [System.IO.Path]::GetFullPath($EvidenceRoot)
$testPath = Join-Path $resolvedExtensionRoot "test\phase10\phase10CursorFullExtensionProof.js"
if (-not (Test-Path -LiteralPath $testPath)) {
    throw "Phase 10 Cursor test module missing: $testPath"
}

$runId = (Get-Date).ToUniversalTime().ToString("yyyyMMddTHHmmssfffffffZ")
$runRoot = Join-Path $resolvedEvidenceRoot "run-$runId"
Ensure-Directory -Path $runRoot

$cursorAttempt = Invoke-Phase10Host -CliName "cursor" -HostLabel "Cursor Extension Host command automation" -RunRoot $runRoot -TestPath $testPath -RunId $runId
$attempts = @($cursorAttempt)

if ($cursorAttempt.passed -ne $true) {
    $codeAttempt = Invoke-Phase10Host -CliName "code" -HostLabel "VS Code Extension Host fallback for Cursor extension command handler" -RunRoot $runRoot -TestPath $testPath -RunId $runId
    $attempts += $codeAttempt
}

$accepted = @($attempts | Where-Object { $_.passed -eq $true } | Select-Object -First 1)
$passed = $accepted.Count -gt 0
$acceptedReportPath = if ($passed) { $accepted[0].report_path } else { "" }
$summaryPath = Join-Path $runRoot "phase10-runner-summary.json"
$commitReadyEvidencePaths = Get-Phase10CommitReadyEvidencePaths -RunRoot $runRoot -Attempts $attempts -SummaryPath $summaryPath
$excludedVolatileEvidenceDirs = Get-Phase10ExcludedVolatileEvidenceDirs -RunRoot $runRoot -Attempts $attempts

$summary = [ordered]@{
    schema_version = 1
    phase = "10"
    phase_name = "Cursor full extension live proof"
    generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
    passed = $passed
    verdict = if ($passed) { "PHASE_10_CURSOR_PROOF_READY_FOR_REVIEW" } else { "NEEDS_FIX" }
    extension_root = $resolvedExtensionRoot
    run_root = $runRoot
    accepted_report_path = $acceptedReportPath
    cursor_attempted_first = $true
    cursor_passed = ($cursorAttempt.passed -eq $true)
    fallback_used = ($passed -and $accepted[0].cli -ne "cursor")
    full_manual_cursor_ui_claimed = $false
    dashboard_or_api_only_substitute_used = $false
    external_ai_or_network_used = $false
    external_ai_or_network_claim_scope = "extension/core proof path only; full Cursor process network silence is not claimed"
    semantic_quality_claimed = $false
    commit_ready_evidence_paths = $commitReadyEvidencePaths
    excluded_volatile_evidence_dirs = $excludedVolatileEvidenceDirs
    attempts = $attempts
}

Write-Json -Value $summary -Path $summaryPath
Write-Output "Phase 10 Cursor extension proof runner"
Write-Output "Summary: $summaryPath"
Write-Output "Verdict: $($summary.verdict)"
Write-Output ($summary | ConvertTo-Json -Depth 32)

if (-not $passed) {
    exit 1
}

exit 0
