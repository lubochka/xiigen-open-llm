$ErrorActionPreference = "Stop"

$RunRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$CursorExe = "C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe"
$ExtensionRoot = "C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills"
$TestPath = Join-Path $RunRoot "visible-ui-phase1d-extension-test.js"
$UserDataDir = Join-Path $RunRoot "user-data"
$ExtensionsDir = Join-Path $RunRoot "extensions"
$ValidationPath = Join-Path $RunRoot "visible-ui-phase1d-validation.json"
$FailurePath = Join-Path $RunRoot "visible-ui-phase1d-harness-failure.json"
$ScreenshotPath = Join-Path $RunRoot "cursor-visible-ui-desktop-screenshot.png"
$CaptureJsonPath = Join-Path $RunRoot "visible-ui-phase1d-desktop-capture.json"
$StdoutLogPath = Join-Path $RunRoot "cursor-stdout.log"
$StderrLogPath = Join-Path $RunRoot "cursor-stderr.log"
$PrelaunchBlockerJsonPath = Join-Path $RunRoot "visible-ui-phase1d-prelaunch-blocker.json"
$PrelaunchBlockerMdPath = Join-Path $RunRoot "visible-ui-phase1d-prelaunch-blocker.md"

$KnownErrorDialogPatterns = @(
  "A JavaScript error occurred in the main process",
  "EPIPE"
)

try {
  Add-Type @'
using System;
using System.Text;
using System.Runtime.InteropServices;
public class Win32WindowToolsPhase1D {
  [DllImport("user32.dll")]
  public static extern bool SetForegroundWindow(IntPtr hWnd);
  [DllImport("user32.dll")]
  public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
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
}
'@
} catch {
  $Blocker = [pscustomobject]@{
    schema_version = "frdp-p01-visible-ui-phase1d-prelaunch-blocker/v1"
    generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
    status = "BLOCKED_WITH_NEXT_ACTION"
    current_task = "Pre-launch clean-environment gate for visible Cursor Phase 1D proof."
    blocker_reason = "window_enumeration_unavailable"
    enumeration_error = $_.Exception.Message
    cursor_launched = $false
    no_stale_or_global_window_close_attempted = $true
    next_action = "Fix local window-enumeration support or run from an interactive desktop session, then retry once."
  }
  $Blocker | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $PrelaunchBlockerJsonPath -Encoding UTF8
  @(
    "# Phase 1D Prelaunch Blocker",
    "",
    "Status: BLOCKED_WITH_NEXT_ACTION",
    "",
    "The launcher did not start Cursor because visible top-level window enumeration failed.",
    "",
    "No stale or global Error/Cursor windows were closed.",
    "",
    "Next action: Fix local window-enumeration support or run from an interactive desktop session, then retry once.",
    "",
    "Enumeration error:",
    "",
    "-----",
    $_.Exception.Message,
    "-----"
  ) | Set-Content -LiteralPath $PrelaunchBlockerMdPath -Encoding UTF8
  Write-Output $Blocker.status
  Write-Output $PrelaunchBlockerJsonPath
  Write-Output $PrelaunchBlockerMdPath
  exit 3
}

function ConvertTo-CommandLineArgument {
  param([string]$Value)
  $Quote = [string][char]34
  $Backslash = [string][char]92
  if ($Value.Contains(" ") -or $Value.Contains("`t") -or $Value.Contains($Quote)) {
    $Escaped = $Value.Replace($Quote, $Backslash + $Quote)
    return $Quote + $Escaped + $Quote
  }
  return $Value
}

function Get-NativeWindowTitle {
  param([IntPtr]$Handle)
  $Length = [Win32WindowToolsPhase1D]::GetWindowTextLength($Handle)
  if ($Length -le 0) {
    return ""
  }
  $Builder = New-Object System.Text.StringBuilder ($Length + 1)
  [void][Win32WindowToolsPhase1D]::GetWindowText($Handle, $Builder, $Builder.Capacity)
  return $Builder.ToString()
}

function Get-NativeWindowClassName {
  param([IntPtr]$Handle)
  $Builder = New-Object System.Text.StringBuilder 256
  [void][Win32WindowToolsPhase1D]::GetClassName($Handle, $Builder, $Builder.Capacity)
  return $Builder.ToString()
}

function Get-ChildWindowTexts {
  param([IntPtr]$ParentHandle)
  $Script:ChildWindowTextBuffer = @()
  $ChildCallback = [Win32WindowToolsPhase1D+EnumWindowsProc]{
    param([IntPtr]$Handle, [IntPtr]$Param)
    $ChildTitle = Get-NativeWindowTitle $Handle
    if (-not [string]::IsNullOrWhiteSpace($ChildTitle)) {
      $Script:ChildWindowTextBuffer += [pscustomobject]@{
        window_handle = $Handle.ToInt64()
        class_name = Get-NativeWindowClassName $Handle
        text = $ChildTitle
      }
    }
    return $true
  }
  [void][Win32WindowToolsPhase1D]::EnumChildWindows($ParentHandle, $ChildCallback, [IntPtr]::Zero)
  return @($Script:ChildWindowTextBuffer)
}

function Get-VisibleTopLevelWindowSnapshots {
  $Script:VisibleTopLevelWindowSnapshotsBuffer = @()
  $WindowCallback = [Win32WindowToolsPhase1D+EnumWindowsProc]{
    param([IntPtr]$Handle, [IntPtr]$Param)
    if (-not [Win32WindowToolsPhase1D]::IsWindowVisible($Handle)) {
      return $true
    }
    $WindowPid = [uint32]0
    [void][Win32WindowToolsPhase1D]::GetWindowThreadProcessId($Handle, [ref]$WindowPid)
    $Title = Get-NativeWindowTitle $Handle
    $ClassName = Get-NativeWindowClassName $Handle
    $ChildTexts = @(Get-ChildWindowTexts $Handle)
    $Script:VisibleTopLevelWindowSnapshotsBuffer += [pscustomobject]@{
      process_id = [int]$WindowPid
      window_handle = $Handle.ToInt64()
      title = $Title
      class_name = $ClassName
      child_texts = $ChildTexts
    }
    return $true
  }
  [void][Win32WindowToolsPhase1D]::EnumWindows($WindowCallback, [IntPtr]::Zero)
  return @($Script:VisibleTopLevelWindowSnapshotsBuffer)
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
  foreach ($Pattern in $KnownErrorDialogPatterns) {
    if ($CombinedText.IndexOf($Pattern, [StringComparison]::OrdinalIgnoreCase) -ge 0) {
      $Reasons += ("text_contains_" + ($Pattern -replace "[^A-Za-z0-9]+", "_").Trim([char]"_"))
    }
  }
  $CompactText = (($CombinedText -replace "\s+", " ").Trim())
  if ($CompactText.Length -gt 500) {
    $CompactText = $CompactText.Substring(0, 500)
  }
  return [pscustomobject]@{
    matched = $Reasons.Count -gt 0
    reasons = $Reasons
    matched_text_excerpt = $CompactText
  }
}

function Get-KnownErrorDialogDetections {
  param(
    [string]$Phase,
    [object[]]$Snapshots
  )
  $Matches = @()
  foreach ($Snapshot in $Snapshots) {
    $MatchInfo = Get-KnownErrorDialogMatch $Snapshot
    if (-not $MatchInfo.matched) {
      continue
    }
    $Matches += [pscustomobject]@{
      phase = $Phase
      detected_at_utc = (Get-Date).ToUniversalTime().ToString("o")
      process_id = $Snapshot.process_id
      window_handle = $Snapshot.window_handle
      title = $Snapshot.title
      class_name = $Snapshot.class_name
      reasons = $MatchInfo.reasons
      matched_text_excerpt = $MatchInfo.matched_text_excerpt
      action = "detect_only_no_close"
    }
  }
  return @($Matches)
}

function Invoke-KnownErrorDialogDetectionNoClose {
  param([string]$Phase)
  $Snapshots = @(Get-VisibleTopLevelWindowSnapshots)
  $Matches = @(Get-KnownErrorDialogDetections -Phase $Phase -Snapshots $Snapshots)
  return [pscustomobject]@{
    phase = $Phase
    scanned_at_utc = (Get-Date).ToUniversalTime().ToString("o")
    visible_top_level_window_count = $Snapshots.Count
    matched_known_error_dialog_count = $Matches.Count
    matches = $Matches
    action = "detect_only_no_close"
  }
}

function Get-CursorProcessesNotTiedToNewRun {
  @(Get-CimInstance Win32_Process | Where-Object {
    $_.Name -eq "Cursor.exe" -and (
      -not $_.CommandLine -or
      ([string]$_.CommandLine).IndexOf($RunRoot, [StringComparison]::OrdinalIgnoreCase) -lt 0
    )
  } | ForEach-Object {
    [pscustomobject]@{
      process_id = [int]$_.ProcessId
      name = $_.Name
      executable_path = $_.ExecutablePath
      command_line = $_.CommandLine
      creation_date = $_.CreationDate
    }
  })
}

function Write-PrelaunchBlocker {
  param(
    [object[]]$VisibleErrorDialogs,
    [object[]]$CursorProcessesNotTiedToNewRun,
    [string]$EnumerationError,
    [string]$ScannedAtUtc
  )

  $Reasons = @()
  if (@($VisibleErrorDialogs).Count -gt 0) {
    $Reasons += "visible_top_level_error_windows_present"
  }
  if (@($CursorProcessesNotTiedToNewRun).Count -gt 0) {
    $Reasons += "cursor_processes_not_tied_to_new_run_present"
  }
  if (-not [string]::IsNullOrWhiteSpace($EnumerationError)) {
    $Reasons += "prelaunch_enumeration_error"
  }

  $Blocker = [pscustomobject]@{
    schema_version = "frdp-p01-visible-ui-phase1d-prelaunch-blocker/v1"
    generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
    status = "BLOCKED_WITH_NEXT_ACTION"
    current_task = "Pre-launch clean-environment gate for visible Cursor Phase 1D proof."
    prelaunch_gate = "FAILED"
    scanned_at_utc = $ScannedAtUtc
    blocker_reasons = $Reasons
    visible_top_level_error_windows = @($VisibleErrorDialogs)
    cursor_processes_not_tied_to_new_run = @($CursorProcessesNotTiedToNewRun)
    prelaunch_enumeration_error = $EnumerationError
    cursor_launched = $false
    no_stale_or_global_window_close_attempted = $true
    error_dialog_close_attempts = @()
    next_action = "Close or investigate the listed stale/global Cursor/Error windows outside this harness, then start a new single run."
    product_readiness_claimed = $false
    full_cursor_readiness_claimed = $false
    git_stage_commit_push = $false
  }
  $Blocker | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $PrelaunchBlockerJsonPath -Encoding UTF8

  $MdLines = @(
    "# Phase 1D Prelaunch Blocker",
    "",
    "Status: BLOCKED_WITH_NEXT_ACTION",
    "",
    "The launcher did not start Cursor because the desktop was not clean before launch.",
    "",
    "No stale or global Error/Cursor windows were closed.",
    "",
    "Next action: Close or investigate the listed stale/global Cursor/Error windows outside this harness, then start a new single run.",
    "",
    "Scanned at UTC: $ScannedAtUtc",
    "",
    "Visible top-level Error windows: $(@($VisibleErrorDialogs).Count)",
    ""
  )
  foreach ($Window in @($VisibleErrorDialogs)) {
    $ReasonText = $Window.reasons -join ", "
    $MdLines += "- pid=$($Window.process_id); handle=$($Window.window_handle); title=$($Window.title); class=$($Window.class_name); reasons=$ReasonText; excerpt=$($Window.matched_text_excerpt)"
  }
  $MdLines += @(
    "",
    "Cursor.exe processes not tied to this new run: $(@($CursorProcessesNotTiedToNewRun).Count)",
    ""
  )
  foreach ($CursorProcess in @($CursorProcessesNotTiedToNewRun)) {
    $MdLines += "- pid=$($CursorProcess.process_id); exe=$($CursorProcess.executable_path); command_line=$($CursorProcess.command_line)"
  }
  if (-not [string]::IsNullOrWhiteSpace($EnumerationError)) {
    $MdLines += @(
      "",
      "Enumeration error:",
      "",
      "-----",
      $EnumerationError,
      "-----"
    )
  }
  $MdLines | Set-Content -LiteralPath $PrelaunchBlockerMdPath -Encoding UTF8

  Write-Output $Blocker.status
  Write-Output $PrelaunchBlockerJsonPath
  Write-Output $PrelaunchBlockerMdPath
  exit 3
}

$PrelaunchScannedAtUtc = (Get-Date).ToUniversalTime().ToString("o")
$PrelaunchVisibleErrorDialogs = @()
$PrelaunchCursorProcessesNotTiedToNewRun = @()
$PrelaunchEnumerationError = $null
try {
  $PrelaunchWindowSnapshots = @(Get-VisibleTopLevelWindowSnapshots)
  $PrelaunchVisibleErrorDialogs = @(Get-KnownErrorDialogDetections -Phase "prelaunch_clean_environment_gate" -Snapshots $PrelaunchWindowSnapshots)
  $PrelaunchCursorProcessesNotTiedToNewRun = @(Get-CursorProcessesNotTiedToNewRun)
} catch {
  $PrelaunchEnumerationError = $_.Exception.Message
}

if (
  @($PrelaunchVisibleErrorDialogs).Count -gt 0 -or
  @($PrelaunchCursorProcessesNotTiedToNewRun).Count -gt 0 -or
  -not [string]::IsNullOrWhiteSpace($PrelaunchEnumerationError)
) {
  Write-PrelaunchBlocker `
    -VisibleErrorDialogs $PrelaunchVisibleErrorDialogs `
    -CursorProcessesNotTiedToNewRun $PrelaunchCursorProcessesNotTiedToNewRun `
    -EnumerationError $PrelaunchEnumerationError `
    -ScannedAtUtc $PrelaunchScannedAtUtc
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
[void]$Process.Start()
$StdoutDrainTask = $Process.StandardOutput.BaseStream.CopyToAsync($StdoutStream)
$StderrDrainTask = $Process.StandardError.BaseStream.CopyToAsync($StderrStream)

$ObservedState = "timeout_waiting_for_validation"
$Deadline = (Get-Date).AddSeconds(75)
while ((Get-Date) -lt $Deadline) {
  if (Test-Path $ValidationPath) {
    $ObservedState = "validation_json_written"
    break
  }
  if (Test-Path $FailurePath) {
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
$ErrorDialogCloseAttempts = @()
$PreCaptureErrorDialogDetectionNoClose = @()
try {
  foreach ($Match in $MatchingProcesses) {
    $GuiProcess = Get-Process -Id $Match.ProcessId -ErrorAction SilentlyContinue
    if ($GuiProcess -and $GuiProcess.MainWindowHandle -ne 0) {
      [void][Win32WindowToolsPhase1D]::ShowWindow($GuiProcess.MainWindowHandle, 9)
      [void][Win32WindowToolsPhase1D]::SetForegroundWindow($GuiProcess.MainWindowHandle)
      $ForegroundAttempts += [pscustomobject]@{
        process_id = $Match.ProcessId
        main_window_handle = $GuiProcess.MainWindowHandle
        attempted = $true
      }
    }
  }
} catch {
  $ForegroundAttempts += [pscustomobject]@{
    process_id = $null
    attempted = $false
    error = $_.Exception.Message
  }
}

Start-Sleep -Seconds 2

$PreCaptureDetectionDeadline = (Get-Date).AddSeconds(4)
do {
  try {
    $PreCaptureErrorDialogDetectionNoClose += Invoke-KnownErrorDialogDetectionNoClose -Phase "pre_copy_from_screen_no_close"
  } catch {
    $PreCaptureErrorDialogDetectionNoClose += [pscustomobject]@{
      phase = "pre_copy_from_screen_no_close"
      scanned_at_utc = (Get-Date).ToUniversalTime().ToString("o")
      visible_top_level_window_count = $null
      matched_known_error_dialog_count = $null
      matches = @()
      action = "detect_only_no_close"
      error = $_.Exception.Message
    }
  }
  Start-Sleep -Milliseconds 300
} while ((Get-Date) -lt $PreCaptureDetectionDeadline)

$ScreenshotError = $null
$ScreenshotCapturedAtUtc = $null
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

$ScreenshotInfo = $null
if (Test-Path $ScreenshotPath) {
  $ScreenshotItem = Get-Item $ScreenshotPath
  $ScreenshotInfo = [pscustomobject]@{
    path = $ScreenshotItem.FullName
    bytes = $ScreenshotItem.Length
    last_write_time_utc = $ScreenshotItem.LastWriteTimeUtc.ToString("o")
    captured_at_utc = $ScreenshotCapturedAtUtc
  }
}

$ExitObserved = $false
if ($Process.WaitForExit(15000)) {
  $ExitObserved = $true
}

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
} catch {
}
try {
  $StderrStream.Flush()
} catch {
}
$StdoutStream.Dispose()
$StderrStream.Dispose()

$ValidationPresent = Test-Path $ValidationPath
$FailurePresent = Test-Path $FailurePath
$ValidationAllPassed = $false
$ValidationReadError = $null
if ($ValidationPresent) {
  try {
    $ValidationObject = Get-Content -LiteralPath $ValidationPath -Raw | ConvertFrom-Json
    $ValidationAllPassed = $ValidationObject.all_passed -eq $true
  } catch {
    $ValidationReadError = $_.Exception.Message
  }
}
$PreCaptureErrorDialogMatches = @($PreCaptureErrorDialogDetectionNoClose | ForEach-Object { $_.matches })
$KnownErrorDialogDetectedPostLaunch = @($PreCaptureErrorDialogMatches).Count -gt 0
$EpipeDialogDetected = @($PreCaptureErrorDialogMatches | Where-Object {
  (($_.reasons -join "`n").IndexOf("EPIPE", [StringComparison]::OrdinalIgnoreCase) -ge 0) -or
  ([string]$_.matched_text_excerpt).IndexOf("EPIPE", [StringComparison]::OrdinalIgnoreCase) -ge 0
}).Count -gt 0
$StdoutLogInfo = $null
if (Test-Path $StdoutLogPath) {
  $StdoutLogItem = Get-Item $StdoutLogPath
  $StdoutLogInfo = [pscustomobject]@{
    path = $StdoutLogItem.FullName
    bytes = $StdoutLogItem.Length
    last_write_time_utc = $StdoutLogItem.LastWriteTimeUtc.ToString("o")
  }
}
$StderrLogInfo = $null
if (Test-Path $StderrLogPath) {
  $StderrLogItem = Get-Item $StderrLogPath
  $StderrLogInfo = [pscustomobject]@{
    path = $StderrLogItem.FullName
    bytes = $StderrLogItem.Length
    last_write_time_utc = $StderrLogItem.LastWriteTimeUtc.ToString("o")
  }
}
$ScreenshotManualClean = $null
$Capture = [pscustomobject]@{
  schema_version = "frdp-p01-visible-ui-phase1d-desktop-capture/v2"
  generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
  status = if ($KnownErrorDialogDetectedPostLaunch -or $EpipeDialogDetected) { "NEEDS_FIX" } elseif ($ScreenshotManualClean -eq $false) { "NEEDS_FIX" } elseif ($ValidationPresent -and -not $ValidationAllPassed) { "NEEDS_FIX" } elseif ($ValidationPresent -and $ScreenshotInfo) { "ACCEPTABLE_EVIDENCE" } elseif ($ValidationPresent) { "NEEDS_FIX" } else { "BLOCKED_WITH_NEXT_ACTION" }
  evidence_type = if ($ScreenshotInfo) { "true_desktop_ui_screenshot_attempt" } else { "desktop_screenshot_not_captured" }
  current_task = "Visible local Cursor UI/render proof for frdp-p01-cursor-full-extension-entrypoint panel marker result."
  not_the_task = @(
    "product readiness",
    "full Cursor readiness",
    "semantic quality",
    "token savings",
    "online prompt send",
    "git stage/commit/push"
  )
  run_root = $RunRoot
  cursor_executable = $CursorExe
  extension_root = $ExtensionRoot
  extension_tests_path = $TestPath
  prelaunch_clean_environment_gate = [pscustomobject]@{
    status = "PASSED"
    scanned_at_utc = $PrelaunchScannedAtUtc
    visible_top_level_error_window_count = @($PrelaunchVisibleErrorDialogs).Count
    cursor_processes_not_tied_to_new_run_count = @($PrelaunchCursorProcessesNotTiedToNewRun).Count
    cursor_launched_after_gate = $true
    no_stale_or_global_window_close_attempted = $true
  }
  observed_state = $ObservedState
  validation_json_present = $ValidationPresent
  validation_all_passed = $ValidationAllPassed
  validation_read_error = $ValidationReadError
  harness_failure_json_present = $FailurePresent
  screenshot = $ScreenshotInfo
  screenshot_error = $ScreenshotError
  screenshot_captured_before_cleanup = $ScreenshotCapturedAtUtc -ne $null
  screenshot_captured_at_utc_before_cleanup = $ScreenshotCapturedAtUtc
  screenshot_manual_inspection_required = $true
  screenshot_manual_clean = $ScreenshotManualClean
  screenshot_manual_clean_required_for_acceptance = $true
  screenshot_overrides_launcher_stdout = $true
  known_error_dialog_detected_post_launch_no_close = $KnownErrorDialogDetectedPostLaunch
  epipe_dialog_detected = $EpipeDialogDetected
  launched_process_id = $Process.Id
  launched_process_exit_observed = $ExitObserved
  launched_process_exit_code = if ($ExitObserved) { $Process.ExitCode } else { $null }
  matching_processes_before_capture = @($MatchingProcesses | Select-Object ProcessId, Name, CommandLine)
  foreground_attempts = $ForegroundAttempts
  pre_capture_error_dialog_detection_no_close = $PreCaptureErrorDialogDetectionNoClose
  error_dialog_close_attempts = $ErrorDialogCloseAttempts
  scoped_processes_stopped_after_capture = @($ScopedProcessesAfterCapture | Select-Object ProcessId, Name, CommandLine)
  command = $CursorExe + " " + (($CursorArgs | ForEach-Object { ConvertTo-CommandLineArgument $_ }) -join " ")
  stdout_log = $StdoutLogPath
  stderr_log = $StderrLogPath
  stdout_log_info = $StdoutLogInfo
  stderr_log_info = $StderrLogInfo
  stdout_drain_completed = $StdoutDrainCompleted
  stderr_drain_completed = $StderrDrainCompleted
  stdout_drain_error = $StdoutDrainError
  stderr_drain_error = $StderrDrainError
  stdio_redirected = $true
  online_prompt_or_message_sent_by_harness = $false
  product_readiness_claimed = $false
  full_cursor_readiness_claimed = $false
  git_stage_commit_push = $false
}

$Capture | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $CaptureJsonPath -Encoding UTF8

Write-Output $Capture.status
Write-Output $CaptureJsonPath

if ($Capture.status -eq "BLOCKED_WITH_NEXT_ACTION") {
  exit 3
}
if ($Capture.status -eq "NEEDS_FIX") {
  exit 2
}
