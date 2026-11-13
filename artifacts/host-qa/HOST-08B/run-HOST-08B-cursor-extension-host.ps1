[CmdletBinding()]
param(
    [string]$CoreRoot = "C:\Projects\llm_mvp_core",
    [string]$CursorRepo = "C:\Projects\cursor-xiigen-smart-skills",
    [string]$CursorExe = "C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe"
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"

$runId = (Get-Date).ToUniversalTime().ToString("yyyyMMdd-HHmmss")
$runRoot = Join-Path $CoreRoot "artifacts\host-qa\HOST-08B\$runId"
$cursorProbeRoot = Join-Path $CursorRepo "reports\HOST-08B-probe\extension-host\$runId"
New-Item -ItemType Directory -Force -Path $runRoot, $cursorProbeRoot | Out-Null

$stdoutPath = Join-Path $runRoot "cursor-extension-host.stdout.log"
$stderrPath = Join-Path $runRoot "cursor-extension-host.stderr.log"
$summaryPath = Join-Path $runRoot "cursor-extension-host-command-summary.json"
$coreReportPath = Join-Path $runRoot "HOST-08B-cursor-extension-host-qa.json"
$hostReportPath = Join-Path $CursorRepo "reports\HOST-08B-cursor-extension-host-qa.json"
$userDataDir = Join-Path $env:TEMP "xiigen-HOST-08B-$runId-user-data"
$extensionsDir = Join-Path $env:TEMP "xiigen-HOST-08B-$runId-extensions"
New-Item -ItemType Directory -Force -Path $userDataDir, $extensionsDir | Out-Null
Remove-Item -LiteralPath $hostReportPath -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $coreReportPath -Force -ErrorAction SilentlyContinue

$env:HOST08B_CURSOR_QA_DIR = $cursorProbeRoot
$env:HOST08B_CURSOR_REPORT_PATH = $hostReportPath
$env:PHASE40_CURSOR_RUNTIME_EVIDENCE_DIR = $cursorProbeRoot

$cursorArgs = @(
    "--extensionDevelopmentPath", $CursorRepo,
    "--extensionTestsPath", (Join-Path $CursorRepo "test\host-qa\HOST-08B-extensionHostQA.js"),
    "--user-data-dir", $userDataDir,
    "--extensions-dir", $extensionsDir,
    "--disable-workspace-trust",
    "--skip-welcome",
    "--skip-release-notes",
    "--skip-onboarding",
    "--disable-telemetry",
    "--disable-updates",
    "--log", "trace",
    "--headless",
    "--new-window",
    $CursorRepo
)

$start = Get-Date
$exitCode = $null
$launchError = $null
$processStoppedAfterEvidence = $false
$processId = $null
try {
    $process = Start-Process -FilePath $CursorExe -ArgumentList $cursorArgs -RedirectStandardOutput $stdoutPath -RedirectStandardError $stderrPath -WindowStyle Hidden -PassThru
    $processId = $process.Id
    $deadline = (Get-Date).AddSeconds(75)
    while ((Get-Date) -lt $deadline) {
        if (Test-Path -LiteralPath $hostReportPath) {
            Start-Sleep -Seconds 2
            break
        }
        if ($process.HasExited) {
            break
        }
        Start-Sleep -Milliseconds 500
    }
    if ($process.HasExited) {
        $exitCode = $process.ExitCode
    }
    elseif (Test-Path -LiteralPath $hostReportPath) {
        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
        $processStoppedAfterEvidence = $true
    }
    else {
        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
        $exitCode = 124
    }
}
catch {
    $launchError = $_.Exception.Message
    $exitCode = 1
}
$elapsedMs = [int]((Get-Date) - $start).TotalMilliseconds

if (Test-Path -LiteralPath $hostReportPath) {
    Copy-Item -LiteralPath $hostReportPath -Destination $coreReportPath -Force
}

$summary = [ordered]@{
    schema_version = 1
    work_order = "HOST-08B-CURSOR-EXTENSION-HOST-QA-AUTOMATION"
    command = "$CursorExe <HOST-08B Extension Host args>"
    command_args = $cursorArgs
    exit_code = $exitCode
    elapsed_ms = $elapsedMs
    launch_error = $launchError
    process_id = $processId
    process_stopped_after_evidence = $processStoppedAfterEvidence
    stdout_ref = $stdoutPath
    stderr_ref = $stderrPath
    host_report_ref = $hostReportPath
    core_report_ref = $coreReportPath
    cursor_probe_root = $cursorProbeRoot
    core_run_root = $runRoot
}
$summary | ConvertTo-Json -Depth 32 | Set-Content -LiteralPath $summaryPath -Encoding UTF8

if ($exitCode -ne 0) {
    exit $exitCode
}

if (-not (Test-Path -LiteralPath $coreReportPath)) {
    Write-Error "HOST-08B report was not produced: $coreReportPath"
    exit 1
}

$report = Get-Content -Raw -LiteralPath $coreReportPath | ConvertFrom-Json
if ($report.passed -ne $true) {
    exit 1
}

exit 0
