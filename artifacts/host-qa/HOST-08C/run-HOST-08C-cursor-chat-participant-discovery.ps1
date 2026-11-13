[CmdletBinding()]
param(
    [string]$CoreRoot = "C:\Projects\llm_mvp_core",
    [string]$CursorRepo = "C:\Projects\cursor-xiigen-smart-skills",
    [string]$CursorExe = "C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe"
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"

$runId = (Get-Date).ToUniversalTime().ToString("yyyyMMdd-HHmmss")
$runRoot = Join-Path $CoreRoot "artifacts\host-qa\HOST-08C\$runId"
$cursorProbeRoot = Join-Path $CursorRepo "reports\HOST-08C-probe\extension-host\$runId"
New-Item -ItemType Directory -Force -Path $runRoot, $cursorProbeRoot | Out-Null

$stdoutPath = Join-Path $runRoot "cursor-chat-participant.stdout.log"
$stderrPath = Join-Path $runRoot "cursor-chat-participant.stderr.log"
$summaryPath = Join-Path $runRoot "cursor-chat-participant-command-summary.json"
$coreReportPath = Join-Path $runRoot "HOST-08C-cursor-chat-participant-qa.json"
$hostReportPath = Join-Path $CursorRepo "reports\HOST-08C-cursor-chat-participant-qa.json"
$userDataDir = Join-Path $env:TEMP "xiigen-HOST-08C-$runId-user-data"
$extensionsDir = Join-Path $env:TEMP "xiigen-HOST-08C-$runId-extensions"
New-Item -ItemType Directory -Force -Path $userDataDir, $extensionsDir | Out-Null
Remove-Item -LiteralPath $hostReportPath -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $coreReportPath -Force -ErrorAction SilentlyContinue

$env:HOST08C_CURSOR_QA_DIR = $cursorProbeRoot
$env:HOST08C_CURSOR_REPORT_PATH = $hostReportPath

$cursorArgs = @(
    "--extensionDevelopmentPath", $CursorRepo,
    "--extensionTestsPath", (Join-Path $CursorRepo "test\host-qa\HOST-08C-chatParticipantDiscovery.js"),
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
    work_order = "HOST-08C-CURSOR-CHAT-PARTICIPANT-QA-PATH"
    command = "$CursorExe <HOST-08C Extension Host chat discovery args>"
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

if (-not (Test-Path -LiteralPath $coreReportPath)) {
    Write-Error "HOST-08C report was not produced: $coreReportPath"
    exit 1
}

exit 0
