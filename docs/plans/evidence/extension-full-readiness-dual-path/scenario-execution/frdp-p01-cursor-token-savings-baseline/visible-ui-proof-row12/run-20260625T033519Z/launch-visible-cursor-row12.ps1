$ErrorActionPreference = "Stop"

$RunRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$CursorExe = "C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe"
$ExtensionRoot = "C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills"
$TestPath = Join-Path $RunRoot "visible-ui-row12-extension-test.js"
$UserDataDir = Join-Path $RunRoot "user-data"
$ExtensionsDir = Join-Path $RunRoot "extensions"
$ValidationPath = Join-Path $RunRoot "visible-ui-row12-validation.json"
$FailurePath = Join-Path $RunRoot "visible-ui-row12-harness-failure.json"
$ScreenshotPath = Join-Path $RunRoot "cursor-visible-ui-row12-desktop-screenshot.png"
$CaptureJsonPath = Join-Path $RunRoot "visible-ui-row12-desktop-capture.json"
$PrelaunchCheckPath = Join-Path $RunRoot "visible-ui-row12-prelaunch-check.json"
$CommandResultPath = Join-Path $RunRoot "visible-ui-row12-command-result.json"
$ReportPath = Join-Path $RunRoot "visible-ui-row12-report.md"
$BlockedPacketPath = Join-Path $RunRoot "visible-ui-row12-blocked-packet.md"
$StdoutLogPath = Join-Path $RunRoot "cursor-stdout.log"
$StderrLogPath = Join-Path $RunRoot "cursor-stderr.log"
$Marker = "FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1"
$ScenarioId = "frdp-p01-cursor-token-savings-baseline"
$PairId = "frdp-p01-row12-cursor-token-baseline-v1"
$PromptHash = "sha256:7140fd94d9e6df18b4abef4f13317146ef41a5db7ba1953e8579ffcb783cb374"
$KnownErrorDialogPatterns = @("A JavaScript error occurred in the main process", "EPIPE")
$ScreenshotCapturedAtUtc = $null
$CleanupStartedAtUtc = $null

function Clear-PreviousRunArtifacts {
  $ResolvedRunRoot = [System.IO.Path]::GetFullPath($RunRoot)
  $GeneratedPaths = @(
    $ValidationPath,
    $FailurePath,
    $ScreenshotPath,
    $CaptureJsonPath,
    $PrelaunchCheckPath,
    $CommandResultPath,
    $ReportPath,
    $BlockedPacketPath,
    $StdoutLogPath,
    $StderrLogPath,
    (Join-Path $RunRoot "extension-host-evidence"),
    $UserDataDir,
    $ExtensionsDir
  )
  foreach ($Path in $GeneratedPaths) {
    $ResolvedPath = [System.IO.Path]::GetFullPath($Path)
    if (-not $ResolvedPath.StartsWith($ResolvedRunRoot, [StringComparison]::OrdinalIgnoreCase)) {
      throw "Refusing to clear artifact outside run folder: $ResolvedPath"
    }
    if (Test-Path -LiteralPath $ResolvedPath) {
      Remove-Item -LiteralPath $ResolvedPath -Recurse -Force
    }
  }
}

function Write-JsonFile {
  param([string]$Path, [object]$Value)
  $Parent = Split-Path -Parent $Path
  if ($Parent) {
    New-Item -ItemType Directory -Force -Path $Parent | Out-Null
  }
  $Value | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $Path -Encoding UTF8
}

function Get-JsonObject {
  param([string]$Path)
  if (-not (Test-Path -LiteralPath $Path)) {
    return $null
  }
  return Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json
}

function Get-FileInfoObject {
  param([string]$Path)
  if (-not (Test-Path -LiteralPath $Path)) {
    return $null
  }
  $Item = Get-Item -LiteralPath $Path
  return [pscustomobject]@{
    path = $Item.FullName
    bytes = $Item.Length
    last_write_time_utc = $Item.LastWriteTimeUtc.ToString("o")
  }
}

function ConvertTo-CommandLineArgument {
  param([string]$Value)
  if ($Value -match '[\s"]') {
    $Escaped = $Value -replace '"', '\"'
    return '"' + $Escaped + '"'
  }
  return $Value
}

function Add-WindowTools {
  if ("Row12WindowTools" -as [type]) {
    return
  }
  Add-Type @"
using System;
using System.Text;
using System.Runtime.InteropServices;
public class Row12WindowTools {
  public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
  [DllImport("user32.dll")]
  public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
  [DllImport("user32.dll")]
  public static extern bool EnumChildWindows(IntPtr hWndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);
  [DllImport("user32.dll", SetLastError=true, CharSet=CharSet.Auto)]
  public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
  [DllImport("user32.dll", SetLastError=true, CharSet=CharSet.Auto)]
  public static extern int GetWindowTextLength(IntPtr hWnd);
  [DllImport("user32.dll", SetLastError=true, CharSet=CharSet.Auto)]
  public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
  [DllImport("user32.dll")]
  public static extern bool IsWindowVisible(IntPtr hWnd);
  [DllImport("user32.dll")]
  public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
  [DllImport("user32.dll")]
  public static extern bool SetForegroundWindow(IntPtr hWnd);
  [DllImport("user32.dll")]
  public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
}
"@
}

function Get-NativeWindowTitle {
  param([IntPtr]$Handle)
  $Length = [Row12WindowTools]::GetWindowTextLength($Handle)
  if ($Length -le 0) {
    return ""
  }
  $Builder = New-Object System.Text.StringBuilder ($Length + 1)
  [void][Row12WindowTools]::GetWindowText($Handle, $Builder, $Builder.Capacity)
  return $Builder.ToString()
}

function Get-NativeWindowClassName {
  param([IntPtr]$Handle)
  $Builder = New-Object System.Text.StringBuilder 256
  [void][Row12WindowTools]::GetClassName($Handle, $Builder, $Builder.Capacity)
  return $Builder.ToString()
}

function Get-ChildWindowTexts {
  param([IntPtr]$ParentHandle)
  $Script:Row12ChildWindowTextBuffer = @()
  $ChildCallback = [Row12WindowTools+EnumWindowsProc]{
    param([IntPtr]$Handle, [IntPtr]$Param)
    $ChildTitle = Get-NativeWindowTitle $Handle
    if (-not [string]::IsNullOrWhiteSpace($ChildTitle)) {
      $Script:Row12ChildWindowTextBuffer += [pscustomobject]@{
        window_handle = $Handle.ToInt64()
        class_name = Get-NativeWindowClassName $Handle
        text = $ChildTitle
      }
    }
    return $true
  }
  [void][Row12WindowTools]::EnumChildWindows($ParentHandle, $ChildCallback, [IntPtr]::Zero)
  return @($Script:Row12ChildWindowTextBuffer)
}

function Get-VisibleTopLevelWindowSnapshots {
  $Script:Row12VisibleWindowSnapshotsBuffer = @()
  $WindowCallback = [Row12WindowTools+EnumWindowsProc]{
    param([IntPtr]$Handle, [IntPtr]$Param)
    if (-not [Row12WindowTools]::IsWindowVisible($Handle)) {
      return $true
    }
    $WindowPid = [uint32]0
    [void][Row12WindowTools]::GetWindowThreadProcessId($Handle, [ref]$WindowPid)
    $Script:Row12VisibleWindowSnapshotsBuffer += [pscustomobject]@{
      process_id = [int]$WindowPid
      window_handle = $Handle.ToInt64()
      title = Get-NativeWindowTitle $Handle
      class_name = Get-NativeWindowClassName $Handle
      child_texts = @(Get-ChildWindowTexts $Handle)
    }
    return $true
  }
  [void][Row12WindowTools]::EnumWindows($WindowCallback, [IntPtr]::Zero)
  return @($Script:Row12VisibleWindowSnapshotsBuffer)
}

function Get-KnownErrorDialogMatch {
  param([object]$Snapshot)
  $CombinedParts = @($Snapshot.title, $Snapshot.class_name)
  $CombinedParts += @($Snapshot.child_texts | ForEach-Object { $_.text })
  $CombinedText = (($CombinedParts | Where-Object { -not [string]::IsNullOrWhiteSpace([string]$_) }) -join "`n")
  $Reasons = @()
  if ($Snapshot.title -eq "Error") {
    $Reasons += "title_exact_error"
  }
  if ($Snapshot.class_name -eq "#32770") {
    $Reasons += "class_exact_32770"
  }
  foreach ($Pattern in $KnownErrorDialogPatterns) {
    if ($CombinedText.IndexOf($Pattern, [StringComparison]::OrdinalIgnoreCase) -ge 0) {
      $Reasons += ("text_contains_" + ($Pattern -replace '[^A-Za-z0-9]+', '_').Trim('_'))
    }
  }
  $CompactText = (($CombinedText -replace '\s+', ' ').Trim())
  if ($CompactText.Length -gt 500) {
    $CompactText = $CompactText.Substring(0, 500)
  }
  return [pscustomobject]@{
    matched = $Reasons.Count -gt 0
    reasons = $Reasons
    matched_text_excerpt = $CompactText
  }
}

function Write-Report {
  param([string]$Status, [string]$NextAction)
  $Validation = Get-JsonObject $ValidationPath
  $Capture = Get-JsonObject $CaptureJsonPath
  $Failure = Get-JsonObject $FailurePath
  $ScreenshotInfo = Get-FileInfoObject $ScreenshotPath
  $EvidenceFiles = @(Get-ChildItem -LiteralPath $RunRoot -Recurse -File | ForEach-Object { $_.FullName })
  $WhatOpened = if ($Capture -and $Capture.launched_cursor) { "The real local Cursor executable was launched visibly: $CursorExe" } else { "The launcher attempted the real local Cursor executable: $CursorExe" }
  $Proven = if ($Status -eq "HOST_VISIBLE_ACCEPT_CANDIDATE") {
    "Local Cursor launched, the XIIGen Smart Skills panel command returned row12 host/core/panel evidence, the exact marker and prompt hash were validated, and a non-empty desktop screenshot was captured before cleanup."
  } elseif ($Failure) {
    "A local launch/harness failure was documented with logs and failure JSON; host-visible acceptance was not proven."
  } else {
    "A local blocked or partial run was documented with logs; host-visible acceptance was not proven."
  }
  $Report = @"
# Row12 Visible Cursor Proof Report

status: $Status
generated_at_utc: $((Get-Date).ToUniversalTime().ToString("o"))
run_folder: $RunRoot

## What Was Opened On The Computer

$WhatOpened

## Marker And Workload

- marker: $Marker
- scenario_id: $ScenarioId
- pair_id: $PairId
- method: prompt-to-task
- prompt_hash: $PromptHash

## What Was Proven

$Proven

## What Was Not Proven

- This does not prove product readiness.
- This does not prove token savings.
- This does not prove semantic quality improvement.
- This does not prove full extension readiness.
- This does not prove local learnability.
- This does not prove training, checkpoint promotion, or locator update.

## Evidence Summary

- validation_json: $(if (Test-Path -LiteralPath $ValidationPath) { $ValidationPath } else { "missing" })
- validation_all_passed: $(if ($Validation) { $Validation.all_passed } else { "n/a" })
- desktop_capture_json: $(if (Test-Path -LiteralPath $CaptureJsonPath) { $CaptureJsonPath } else { "missing" })
- screenshot: $(if ($ScreenshotInfo) { "$($ScreenshotInfo.path) ($($ScreenshotInfo.bytes) bytes)" } else { "missing" })
- command_result_json: $(if (Test-Path -LiteralPath $CommandResultPath) { $CommandResultPath } else { "missing" })
- stdout_log: $StdoutLogPath
- stderr_log: $StderrLogPath
- harness_failure_json: $(if (Test-Path -LiteralPath $FailurePath) { $FailurePath } else { "none" })

## Files Created

$($EvidenceFiles | ForEach-Object { "- $_" } | Out-String)

## Next Smallest Action

$NextAction
"@
  $Report | Set-Content -LiteralPath $ReportPath -Encoding UTF8
  if ($Status -ne "HOST_VISIBLE_ACCEPT_CANDIDATE") {
    $Report | Set-Content -LiteralPath $BlockedPacketPath -Encoding UTF8
  } elseif (Test-Path -LiteralPath $BlockedPacketPath) {
    Remove-Item -LiteralPath $BlockedPacketPath -Force
  }
}

Clear-PreviousRunArtifacts

$PrelaunchWindowToolLoadError = $null
try {
  Add-WindowTools
} catch {
  $PrelaunchWindowToolLoadError = $_.Exception.Message
}

$PrelaunchWindowScanError = $null
$PrelaunchVisibleWindows = @()
try {
  if (-not $PrelaunchWindowToolLoadError) {
    $PrelaunchVisibleWindows = @(Get-VisibleTopLevelWindowSnapshots)
  }
} catch {
  $PrelaunchWindowScanError = $_.Exception.Message
}

$PrelaunchCursorProcesses = @(Get-CimInstance Win32_Process | Where-Object { $_.Name -eq "Cursor.exe" })
$PrelaunchErrorOr32770Windows = @($PrelaunchVisibleWindows | Where-Object {
  $Match = Get-KnownErrorDialogMatch $_
  $Match.matched
})
$PrelaunchClean = ($PrelaunchCursorProcesses.Count -eq 0) -and ($PrelaunchErrorOr32770Windows.Count -eq 0) -and (-not $PrelaunchWindowScanError) -and (-not $PrelaunchWindowToolLoadError)
$PrelaunchCheck = [pscustomobject]@{
  schema_version = "frdp-p01-visible-ui-row12-prelaunch-check/v1"
  generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
  marker = $Marker
  scenario_id = $ScenarioId
  prelaunch_clean = $PrelaunchClean
  cursor_process_count = $PrelaunchCursorProcesses.Count
  cursor_processes = @($PrelaunchCursorProcesses | Select-Object ProcessId, Name, CommandLine)
  visible_error_or_32770_window_count = $PrelaunchErrorOr32770Windows.Count
  visible_error_or_32770_windows = $PrelaunchErrorOr32770Windows
  visible_top_level_window_count = $PrelaunchVisibleWindows.Count
  window_tool_load_error = $PrelaunchWindowToolLoadError
  window_scan_error = $PrelaunchWindowScanError
  dirty_blocks_launch = -not $PrelaunchClean
}
Write-JsonFile $PrelaunchCheckPath $PrelaunchCheck

if (-not (Test-Path -LiteralPath $CursorExe)) {
  $Capture = [pscustomobject]@{
    schema_version = "frdp-p01-visible-ui-row12-desktop-capture/v1"
    generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
    status = "HOST_VISIBLE_BLOCKED_WITH_EVIDENCE"
    evidence_type = "cursor_executable_missing"
    marker = $Marker
    scenario_id = $ScenarioId
    run_root = $RunRoot
    cursor_executable = $CursorExe
    launched_cursor = $false
    validation_json_present = $false
    screenshot = $null
    external_ai_send = $false
    product_readiness_claimed = $false
    token_savings_claimed = $false
    semantic_quality_claimed = $false
    next_action = "Install or locate Cursor.exe, then rerun this same bounded row12 visible proof."
  }
  Write-JsonFile $CaptureJsonPath $Capture
  Write-Report -Status "HOST_VISIBLE_BLOCKED_WITH_EVIDENCE" -NextAction $Capture.next_action
  Write-Output "HOST_VISIBLE_BLOCKED_WITH_EVIDENCE"
  Write-Output $CaptureJsonPath
  exit 3
}

if (-not $PrelaunchClean) {
  $Capture = [pscustomobject]@{
    schema_version = "frdp-p01-visible-ui-row12-desktop-capture/v1"
    generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
    status = "HOST_VISIBLE_BLOCKED_WITH_EVIDENCE"
    evidence_type = "prelaunch_blocked_no_cursor_launch"
    marker = $Marker
    scenario_id = $ScenarioId
    run_root = $RunRoot
    cursor_executable = $CursorExe
    prelaunch_clean = $false
    prelaunch_check_path = $PrelaunchCheckPath
    launched_cursor = $false
    validation_json_present = $false
    screenshot = $null
    external_ai_send = $false
    product_readiness_claimed = $false
    token_savings_claimed = $false
    semantic_quality_claimed = $false
    next_action = "Close/stop preexisting Cursor.exe processes or visible Error/#32770 dialogs, then rerun once from a clean prelaunch state."
  }
  Write-JsonFile $CaptureJsonPath $Capture
  Write-Report -Status "HOST_VISIBLE_BLOCKED_WITH_EVIDENCE" -NextAction $Capture.next_action
  Write-Output "HOST_VISIBLE_BLOCKED_WITH_EVIDENCE"
  Write-Output $CaptureJsonPath
  exit 3
}

New-Item -ItemType Directory -Force $UserDataDir | Out-Null
New-Item -ItemType Directory -Force $ExtensionsDir | Out-Null

$CursorArgs = @(
  "--new-window",
  "--user-data-dir", $UserDataDir,
  "--extensions-dir", $ExtensionsDir,
  "--extensionDevelopmentPath", $ExtensionRoot,
  "--extensionTestsPath", $TestPath,
  "--disable-workspace-trust",
  "--skip-welcome",
  "--skip-release-notes",
  "--skip-onboarding",
  "--disable-telemetry",
  "--disable-updates",
  "--disable-gpu",
  $ExtensionRoot
)

$ProcessInfo = New-Object System.Diagnostics.ProcessStartInfo
$ProcessInfo.FileName = $CursorExe
$ProcessInfo.WorkingDirectory = $ExtensionRoot
$ProcessInfo.UseShellExecute = $false
$ProcessInfo.RedirectStandardOutput = $true
$ProcessInfo.RedirectStandardError = $true
$ProcessInfo.CreateNoWindow = $false
$ProcessInfo.WindowStyle = [System.Diagnostics.ProcessWindowStyle]::Normal
$ProcessInfo.Arguments = ($CursorArgs | ForEach-Object { ConvertTo-CommandLineArgument $_ }) -join " "

$Process = New-Object System.Diagnostics.Process
$Process.StartInfo = $ProcessInfo
$StdoutStream = [System.IO.File]::Open($StdoutLogPath, [System.IO.FileMode]::Create, [System.IO.FileAccess]::Write, [System.IO.FileShare]::ReadWrite)
$StderrStream = [System.IO.File]::Open($StderrLogPath, [System.IO.FileMode]::Create, [System.IO.FileAccess]::Write, [System.IO.FileShare]::ReadWrite)
$ProcessStarted = $false
$LaunchError = $null
try {
  [void]$Process.Start()
  $ProcessStarted = $true
  $StdoutDrainTask = $Process.StandardOutput.BaseStream.CopyToAsync($StdoutStream)
  $StderrDrainTask = $Process.StandardError.BaseStream.CopyToAsync($StderrStream)
} catch {
  $LaunchError = $_.Exception.Message
}

if (-not $ProcessStarted) {
  $StdoutStream.Dispose()
  $StderrStream.Dispose()
  $Capture = [pscustomobject]@{
    schema_version = "frdp-p01-visible-ui-row12-desktop-capture/v1"
    generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
    status = "HOST_VISIBLE_BLOCKED_WITH_EVIDENCE"
    evidence_type = "cursor_launch_failed"
    marker = $Marker
    scenario_id = $ScenarioId
    run_root = $RunRoot
    cursor_executable = $CursorExe
    launched_cursor = $false
    launch_error = $LaunchError
    command = $CursorExe + " " + (($CursorArgs | ForEach-Object { '"' + $_ + '"' }) -join " ")
    stdout_log = $StdoutLogPath
    stderr_log = $StderrLogPath
    external_ai_send = $false
    product_readiness_claimed = $false
    token_savings_claimed = $false
    semantic_quality_claimed = $false
    next_action = "Investigate the local Cursor launch error, then rerun this same bounded row12 visible proof."
  }
  Write-JsonFile $CaptureJsonPath $Capture
  Write-Report -Status "HOST_VISIBLE_BLOCKED_WITH_EVIDENCE" -NextAction $Capture.next_action
  Write-Output "HOST_VISIBLE_BLOCKED_WITH_EVIDENCE"
  Write-Output $CaptureJsonPath
  exit 3
}

$ObservedState = "timeout_waiting_for_validation"
$Deadline = (Get-Date).AddSeconds(90)
while ((Get-Date) -lt $Deadline) {
  if (Test-Path -LiteralPath $ValidationPath) {
    $ObservedState = "validation_json_written"
    break
  }
  if (Test-Path -LiteralPath $FailurePath) {
    $ObservedState = "harness_failure_json_written"
    break
  }
  Start-Sleep -Seconds 1
}

Start-Sleep -Seconds 5

$MatchingProcesses = @(Get-CimInstance Win32_Process | Where-Object {
  $_.Name -eq "Cursor.exe" -and $_.CommandLine -and $_.CommandLine.Contains($RunRoot)
})

$ForegroundAttempts = @()
foreach ($Match in $MatchingProcesses) {
  $GuiProcess = Get-Process -Id $Match.ProcessId -ErrorAction SilentlyContinue
  if ($GuiProcess -and $GuiProcess.MainWindowHandle -ne 0) {
    [void][Row12WindowTools]::ShowWindow($GuiProcess.MainWindowHandle, 9)
    [void][Row12WindowTools]::SetForegroundWindow($GuiProcess.MainWindowHandle)
    $ForegroundAttempts += [pscustomobject]@{
      process_id = $Match.ProcessId
      main_window_handle = $GuiProcess.MainWindowHandle
      attempted = $true
    }
  }
}

Start-Sleep -Seconds 2

$PreCaptureErrorDialogSweepAttempts = @()
try {
  $Snapshots = @(Get-VisibleTopLevelWindowSnapshots)
  foreach ($Snapshot in $Snapshots) {
    $MatchInfo = Get-KnownErrorDialogMatch $Snapshot
    if ($MatchInfo.matched) {
      $PreCaptureErrorDialogSweepAttempts += [pscustomobject]@{
        detected_at_utc = (Get-Date).ToUniversalTime().ToString("o")
        process_id = $Snapshot.process_id
        window_handle = $Snapshot.window_handle
        title = $Snapshot.title
        class_name = $Snapshot.class_name
        reasons = $MatchInfo.reasons
        matched_text_excerpt = $MatchInfo.matched_text_excerpt
        action = "detect_only_no_close_before_desktop_capture"
        close_attempted = $false
      }
    }
  }
} catch {
  $PreCaptureErrorDialogSweepAttempts += [pscustomobject]@{
    detected_at_utc = (Get-Date).ToUniversalTime().ToString("o")
    error = $_.Exception.Message
  }
}

$ScreenshotError = $null
try {
  Add-Type -AssemblyName System.Windows.Forms
  Add-Type -AssemblyName System.Drawing
  $Bounds = [System.Windows.Forms.SystemInformation]::VirtualScreen
  $Bitmap = New-Object System.Drawing.Bitmap $Bounds.Width, $Bounds.Height
  $Graphics = [System.Drawing.Graphics]::FromImage($Bitmap)
  $Graphics.CopyFromScreen($Bounds.X, $Bounds.Y, 0, 0, $Bounds.Size)
  $Bitmap.Save($ScreenshotPath, [System.Drawing.Imaging.ImageFormat]::Png)
  $ScreenshotCapturedAtUtc = (Get-Date).ToUniversalTime().ToString("o")
  $Graphics.Dispose()
  $Bitmap.Dispose()
} catch {
  $ScreenshotError = $_.Exception.Message
}

$ScreenshotInfo = Get-FileInfoObject $ScreenshotPath
$ExitObserved = $false
if ($Process.WaitForExit(15000)) {
  $ExitObserved = $true
}

$CleanupStartedAtUtc = (Get-Date).ToUniversalTime().ToString("o")
$ScopedProcessesAfterCapture = @(Get-CimInstance Win32_Process | Where-Object {
  $_.Name -eq "Cursor.exe" -and $_.CommandLine -and $_.CommandLine.Contains($RunRoot)
})
foreach ($Scoped in $ScopedProcessesAfterCapture) {
  Stop-Process -Id $Scoped.ProcessId -Force -ErrorAction SilentlyContinue
}

$StdoutDrainCompleted = $false
$StderrDrainCompleted = $false
$StdoutDrainError = $null
$StderrDrainError = $null
try {
  if (-not $Process.HasExited) {
    [void]$Process.WaitForExit(5000)
  }
} catch {
}
try {
  $StdoutDrainCompleted = $StdoutDrainTask.Wait(5000)
} catch {
  $StdoutDrainError = $_.Exception.Message
}
try {
  $StderrDrainCompleted = $StderrDrainTask.Wait(5000)
} catch {
  $StderrDrainError = $_.Exception.Message
}
try {
  $StdoutStream.Flush()
  $StderrStream.Flush()
} catch {
}
$StdoutStream.Dispose()
$StderrStream.Dispose()

$ValidationPresent = Test-Path -LiteralPath $ValidationPath
$FailurePresent = Test-Path -LiteralPath $FailurePath
$ValidationAllPassed = $false
$ValidationReadError = $null
if ($ValidationPresent) {
  try {
    $ValidationObject = Get-JsonObject $ValidationPath
    $ValidationAllPassed = $ValidationObject.all_passed -eq $true
  } catch {
    $ValidationReadError = $_.Exception.Message
  }
}

$KnownErrorDialogDetected = @($PreCaptureErrorDialogSweepAttempts | Where-Object { $_.reasons -and $_.reasons.Count -gt 0 }).Count -gt 0
$ScreenshotNonEmpty = $ScreenshotInfo -and $ScreenshotInfo.bytes -gt 0
$ScreenshotBeforeCleanup = ($null -ne $ScreenshotCapturedAtUtc) -and ($null -ne $CleanupStartedAtUtc) -and ([DateTime]::Parse($ScreenshotCapturedAtUtc) -le [DateTime]::Parse($CleanupStartedAtUtc))
$Status = if ($ValidationPresent -and $ValidationAllPassed -and $ScreenshotNonEmpty -and $ScreenshotBeforeCleanup -and (-not $KnownErrorDialogDetected)) {
  "HOST_VISIBLE_ACCEPT_CANDIDATE"
} elseif ($FailurePresent -or $ValidationPresent -or $ScreenshotNonEmpty) {
  "NEEDS_NARROW_FIX"
} else {
  "HOST_VISIBLE_BLOCKED_WITH_EVIDENCE"
}
$NextAction = if ($Status -eq "HOST_VISIBLE_ACCEPT_CANDIDATE") {
  "Parent/architect review of the row12 visible-proof packet; do not continue to another scenario from this worker."
} elseif ($FailurePresent) {
  "Open visible-ui-row12-harness-failure.json and cursor logs, fix the narrow harness/extension invocation issue, then rerun this same row12 visible proof."
} elseif (-not $ScreenshotNonEmpty) {
  "Fix desktop screenshot capture on this host and rerun this same row12 visible proof."
} else {
  "Inspect validation and desktop capture JSON, apply the smallest row12-only harness fix, then rerun."
}

$Capture = [pscustomobject]@{
  schema_version = "frdp-p01-visible-ui-row12-desktop-capture/v1"
  generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
  status = $Status
  evidence_type = if ($ScreenshotNonEmpty) { "true_desktop_ui_screenshot_attempt" } else { "desktop_screenshot_not_captured" }
  marker = $Marker
  scenario_id = $ScenarioId
  pair_id = $PairId
  prompt_hash = $PromptHash
  current_task = "Visible local Cursor UI proof for row12 marker/workload through xiigenSmartSkills.panel."
  not_the_task = @("product readiness", "token savings claim", "semantic quality improvement", "full extension readiness", "local learnability", "external AI", "git stage/commit/push")
  run_root = $RunRoot
  cursor_executable = $CursorExe
  extension_root = $ExtensionRoot
  extension_tests_path = $TestPath
  launched_cursor = $ProcessStarted
  launched_process_id = $Process.Id
  launched_process_exit_observed = $ExitObserved
  launched_process_exit_code = if ($ExitObserved) { $Process.ExitCode } else { $null }
  prelaunch_clean = $PrelaunchClean
  prelaunch_check_path = $PrelaunchCheckPath
  observed_state = $ObservedState
  validation_json_present = $ValidationPresent
  validation_all_passed = $ValidationAllPassed
  validation_read_error = $ValidationReadError
  harness_failure_json_present = $FailurePresent
  screenshot = $ScreenshotInfo
  screenshot_error = $ScreenshotError
  screenshot_captured_at_utc = $ScreenshotCapturedAtUtc
  cleanup_started_at_utc = $CleanupStartedAtUtc
  screenshot_before_cleanup = $ScreenshotBeforeCleanup
  known_error_dialog_detected = $KnownErrorDialogDetected
  pre_capture_error_dialog_detection_no_close = $PreCaptureErrorDialogSweepAttempts
  matching_processes_before_capture = @($MatchingProcesses | Select-Object ProcessId, Name, CommandLine)
  foreground_attempts = $ForegroundAttempts
  scoped_processes_stopped_after_capture = @($ScopedProcessesAfterCapture | Select-Object ProcessId, Name, CommandLine)
  command = $CursorExe + " " + (($CursorArgs | ForEach-Object { '"' + $_ + '"' }) -join " ")
  stdout_log = $StdoutLogPath
  stderr_log = $StderrLogPath
  stdout_log_info = Get-FileInfoObject $StdoutLogPath
  stderr_log_info = Get-FileInfoObject $StderrLogPath
  stdout_drain_completed = $StdoutDrainCompleted
  stderr_drain_completed = $StderrDrainCompleted
  stdout_drain_error = $StdoutDrainError
  stderr_drain_error = $StderrDrainError
  stdio_redirected = $true
  external_ai_send = $false
  online_prompt_or_message_sent_by_harness = $false
  product_readiness_claimed = $false
  token_savings_claimed = $false
  semantic_quality_claimed = $false
  full_cursor_readiness_claimed = $false
  local_learnability_claimed = $false
  git_stage_commit_push = $false
  next_action = $NextAction
}

Write-JsonFile $CaptureJsonPath $Capture
Write-Report -Status $Status -NextAction $NextAction

Write-Output $Status
Write-Output $CaptureJsonPath
if ($Status -eq "HOST_VISIBLE_ACCEPT_CANDIDATE") {
  exit 0
}
if ($Status -eq "NEEDS_NARROW_FIX") {
  exit 2
}
exit 3
