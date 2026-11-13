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
$StdoutLogPath = Join-Path $RunRoot "cursor-stdout.log"
$StderrLogPath = Join-Path $RunRoot "cursor-stderr.log"
$KnownErrorDialogPatterns = @(
  "A JavaScript error occurred in the main process",
  "EPIPE"
)
$ScreenshotCapturedAtUtc = $null
$CleanupStartedAtUtc = $null

try {
  Add-Type @"
using System;
using System.Text;
using System.Runtime.InteropServices;
public class Phase1DPrelaunchWindowTools {
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
"@

  function Get-PrelaunchWindowTitle {
    param([IntPtr]$Handle)
    $Length = [Phase1DPrelaunchWindowTools]::GetWindowTextLength($Handle)
    if ($Length -le 0) {
      return ""
    }
    $Builder = New-Object System.Text.StringBuilder ($Length + 1)
    [void][Phase1DPrelaunchWindowTools]::GetWindowText($Handle, $Builder, $Builder.Capacity)
    return $Builder.ToString()
  }

  function Get-PrelaunchWindowClassName {
    param([IntPtr]$Handle)
    $Builder = New-Object System.Text.StringBuilder 256
    [void][Phase1DPrelaunchWindowTools]::GetClassName($Handle, $Builder, $Builder.Capacity)
    return $Builder.ToString()
  }

  function Get-PrelaunchChildWindowTexts {
    param([IntPtr]$ParentHandle)
    $Script:PrelaunchChildWindowTextBuffer = @()
    $ChildCallback = [Phase1DPrelaunchWindowTools+EnumWindowsProc]{
      param([IntPtr]$Handle, [IntPtr]$Param)
      $ChildTitle = Get-PrelaunchWindowTitle $Handle
      if (-not [string]::IsNullOrWhiteSpace($ChildTitle)) {
        $Script:PrelaunchChildWindowTextBuffer += [pscustomobject]@{
          window_handle = $Handle.ToInt64()
          class_name = Get-PrelaunchWindowClassName $Handle
          text = $ChildTitle
        }
      }
      return $true
    }
    [void][Phase1DPrelaunchWindowTools]::EnumChildWindows($ParentHandle, $ChildCallback, [IntPtr]::Zero)
    return @($Script:PrelaunchChildWindowTextBuffer)
  }

  function Get-PrelaunchVisibleTopLevelWindowSnapshots {
    $Script:PrelaunchVisibleWindowSnapshotsBuffer = @()
    $WindowCallback = [Phase1DPrelaunchWindowTools+EnumWindowsProc]{
      param([IntPtr]$Handle, [IntPtr]$Param)
      if (-not [Phase1DPrelaunchWindowTools]::IsWindowVisible($Handle)) {
        return $true
      }
      $WindowPid = [uint32]0
      [void][Phase1DPrelaunchWindowTools]::GetWindowThreadProcessId($Handle, [ref]$WindowPid)
      $Script:PrelaunchVisibleWindowSnapshotsBuffer += [pscustomobject]@{
        process_id = [int]$WindowPid
        window_handle = $Handle.ToInt64()
        title = Get-PrelaunchWindowTitle $Handle
        class_name = Get-PrelaunchWindowClassName $Handle
        child_texts = @(Get-PrelaunchChildWindowTexts $Handle)
      }
      return $true
    }
    [void][Phase1DPrelaunchWindowTools]::EnumWindows($WindowCallback, [IntPtr]::Zero)
    return @($Script:PrelaunchVisibleWindowSnapshotsBuffer)
  }
} catch {
  $PrelaunchWindowToolLoadError = $_.Exception.Message
}

$PrelaunchWindowScanError = $null
$PrelaunchVisibleWindows = @()
try {
  $PrelaunchVisibleWindows = @(Get-PrelaunchVisibleTopLevelWindowSnapshots)
} catch {
  $PrelaunchWindowScanError = $_.Exception.Message
}
$PrelaunchCursorProcesses = @(Get-CimInstance Win32_Process | Where-Object { $_.Name -eq "Cursor.exe" })
$PrelaunchErrorOr32770Windows = @($PrelaunchVisibleWindows | Where-Object {
  $CombinedParts = @($_.title, $_.class_name)
  $CombinedParts += @($_.child_texts | ForEach-Object { $_.text })
  $CombinedText = (($CombinedParts | Where-Object { -not [string]::IsNullOrWhiteSpace([string]$_) }) -join "`n")
  ($_.title -eq "Error") -or
  ($_.class_name -eq "#32770") -or
  ($CombinedText.IndexOf("A JavaScript error occurred in the main process", [StringComparison]::OrdinalIgnoreCase) -ge 0) -or
  ($CombinedText.IndexOf("EPIPE", [StringComparison]::OrdinalIgnoreCase) -ge 0)
})
$PrelaunchClean = ($PrelaunchCursorProcesses.Count -eq 0) -and ($PrelaunchErrorOr32770Windows.Count -eq 0) -and (-not $PrelaunchWindowScanError) -and (-not $PrelaunchWindowToolLoadError)
$PrelaunchCheck = [pscustomobject]@{
  schema_version = "frdp-p01-visible-ui-row12-prelaunch-check/v1"
  generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
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
$PrelaunchCheck | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $PrelaunchCheckPath -Encoding UTF8

if (-not $PrelaunchClean) {
  $Capture = [pscustomobject]@{
    schema_version = "frdp-p01-visible-ui-row12-desktop-capture/v1"
    generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
    status = "BLOCKED_WITH_NEXT_ACTION"
    evidence_type = "prelaunch_blocked_no_cursor_launch"
    run_root = $RunRoot
    cursor_executable = $CursorExe
    extension_root = $ExtensionRoot
    prelaunch_clean = $false
    prelaunch_check = $PrelaunchCheck
    prelaunch_check_path = $PrelaunchCheckPath
    launched_cursor = $false
    validation_json_present = $false
    validation_all_passed = $false
    screenshot = $null
    pre_capture_error_dialog_detection_no_close = @()
    error_dialog_close_attempts = @()
    stdout_log = $StdoutLogPath
    stderr_log = $StderrLogPath
    stdout_log_info = $null
    stderr_log_info = $null
    stdio_redirected = $false
    screenshot_before_cleanup = $false
    online_prompt_or_message_sent_by_harness = $false
    token_savings_claimed = $false
    semantic_quality_claimed = $false
    readiness_claimed = $false
    product_readiness_claimed = $false
    full_cursor_readiness_claimed = $false
    git_stage_commit_push = $false
    next_action = "Close/stop preexisting Cursor.exe processes or visible Error/#32770 dialogs, then rerun once from a clean prelaunch state."
  }
  $Capture | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $CaptureJsonPath -Encoding UTF8
  Write-Output $Capture.status
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

function ConvertTo-CommandLineArgument {
  param([string]$Value)
  if ($Value -match '[\s"]') {
    $Escaped = $Value -replace '"', '\"'
    return '"' + $Escaped + '"'
  }
  return $Value
}

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
$PreCaptureErrorDialogSweepAttempts = @()
try {
  Add-Type @"
using System;
using System.Text;
using System.Runtime.InteropServices;
public class Win32WindowTools {
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
  [DllImport("user32.dll")]
  public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
}
"@

  function Get-NativeWindowTitle {
    param([IntPtr]$Handle)
    $Length = [Win32WindowTools]::GetWindowTextLength($Handle)
    if ($Length -le 0) {
      return ""
    }
    $Builder = New-Object System.Text.StringBuilder ($Length + 1)
    [void][Win32WindowTools]::GetWindowText($Handle, $Builder, $Builder.Capacity)
    return $Builder.ToString()
  }

  function Get-NativeWindowClassName {
    param([IntPtr]$Handle)
    $Builder = New-Object System.Text.StringBuilder 256
    [void][Win32WindowTools]::GetClassName($Handle, $Builder, $Builder.Capacity)
    return $Builder.ToString()
  }

  function Get-ChildWindowTexts {
    param([IntPtr]$ParentHandle)
    $Script:ChildWindowTextBuffer = @()
    $ChildCallback = [Win32WindowTools+EnumWindowsProc]{
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
    [void][Win32WindowTools]::EnumChildWindows($ParentHandle, $ChildCallback, [IntPtr]::Zero)
    return @($Script:ChildWindowTextBuffer)
  }

  function Get-VisibleTopLevelWindowSnapshots {
    $Script:VisibleTopLevelWindowSnapshotsBuffer = @()
    $WindowCallback = [Win32WindowTools+EnumWindowsProc]{
      param([IntPtr]$Handle, [IntPtr]$Param)
      if (-not [Win32WindowTools]::IsWindowVisible($Handle)) {
        return $true
      }
      $WindowPid = [uint32]0
      [void][Win32WindowTools]::GetWindowThreadProcessId($Handle, [ref]$WindowPid)
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
    [void][Win32WindowTools]::EnumWindows($WindowCallback, [IntPtr]::Zero)
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

  function Invoke-KnownErrorDialogSweep {
    param([string]$Phase)
    $Snapshots = @(Get-VisibleTopLevelWindowSnapshots)
    $Matches = @()
    foreach ($Snapshot in $Snapshots) {
      $MatchInfo = Get-KnownErrorDialogMatch $Snapshot
      if (-not $MatchInfo.matched) {
        continue
      }
      $Attempt = [pscustomobject]@{
        phase = $Phase
        detected_at_utc = (Get-Date).ToUniversalTime().ToString("o")
        process_id = $Snapshot.process_id
        window_handle = $Snapshot.window_handle
        title = $Snapshot.title
        class_name = $Snapshot.class_name
        reasons = $MatchInfo.reasons
        matched_text_excerpt = $MatchInfo.matched_text_excerpt
        action = "detect_only_no_close_before_desktop_capture"
        close_attempted = $false
        close_error = $null
      }
      $Matches += $Attempt
      Start-Sleep -Milliseconds 350
    }
    return [pscustomobject]@{
      phase = $Phase
      scanned_at_utc = (Get-Date).ToUniversalTime().ToString("o")
      visible_top_level_window_count = $Snapshots.Count
      matched_known_error_dialog_count = $Matches.Count
      matches = $Matches
    }
  }

  foreach ($Match in $MatchingProcesses) {
    $GuiProcess = Get-Process -Id $Match.ProcessId -ErrorAction SilentlyContinue
    if ($GuiProcess -and $GuiProcess.MainWindowHandle -ne 0) {
      [void][Win32WindowTools]::ShowWindow($GuiProcess.MainWindowHandle, 9)
      [void][Win32WindowTools]::SetForegroundWindow($GuiProcess.MainWindowHandle)
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

$PreCaptureSweepDeadline = (Get-Date).AddSeconds(4)
do {
  try {
    $PreCaptureErrorDialogSweepAttempts += Invoke-KnownErrorDialogSweep -Phase "pre_copy_from_screen"
  } catch {
    $PreCaptureErrorDialogSweepAttempts += [pscustomobject]@{
      phase = "pre_copy_from_screen"
      scanned_at_utc = (Get-Date).ToUniversalTime().ToString("o")
      visible_top_level_window_count = $null
      matched_known_error_dialog_count = $null
      matches = @()
      error = $_.Exception.Message
    }
  }
  Start-Sleep -Milliseconds 300
} while ((Get-Date) -lt $PreCaptureSweepDeadline)

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
$PreCaptureDetectedMatches = @($PreCaptureErrorDialogSweepAttempts | ForEach-Object { $_.matches } | Where-Object { $_ })
$KnownErrorDialogDetectedNoClose = @($PreCaptureDetectedMatches).Count -gt 0
$KnownErrorDialogDetectedOrClosed = $KnownErrorDialogDetectedNoClose -or (@($ErrorDialogCloseAttempts).Count -gt 0)
$EpipeDialogDetected = @($PreCaptureDetectedMatches | Where-Object {
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
  schema_version = "frdp-p01-visible-ui-row12-desktop-capture/v1"
  generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
  status = if ($KnownErrorDialogDetectedOrClosed -or $EpipeDialogDetected) { "NEEDS_FIX" } elseif ($ScreenshotManualClean -eq $false) { "NEEDS_FIX" } elseif ($ValidationPresent -and -not $ValidationAllPassed) { "NEEDS_FIX" } elseif ($ValidationPresent -and $ScreenshotInfo) { "ACCEPTABLE_EVIDENCE" } elseif ($ValidationPresent) { "NEEDS_FIX" } else { "BLOCKED_WITH_NEXT_ACTION" }
  evidence_type = if ($ScreenshotInfo) { "true_desktop_ui_screenshot_attempt" } else { "desktop_screenshot_not_captured" }
  current_task = "Visible local Cursor UI/render proof for frdp-p01-cursor-token-savings-baseline row12 workload marker result."
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
  prelaunch_clean = $PrelaunchClean
  prelaunch_check = $PrelaunchCheck
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
  screenshot_before_cleanup = ($null -ne $ScreenshotCapturedAtUtc) -and ($null -ne $CleanupStartedAtUtc) -and ([DateTime]::Parse($ScreenshotCapturedAtUtc) -le [DateTime]::Parse($CleanupStartedAtUtc))
  screenshot_manual_inspection_required = $true
  screenshot_manual_clean = $ScreenshotManualClean
  screenshot_manual_clean_required_for_acceptance = $true
  screenshot_overrides_launcher_stdout = $true
  known_error_dialog_detected_or_closed = $KnownErrorDialogDetectedOrClosed
  pre_capture_error_dialog_detected_no_close = $KnownErrorDialogDetectedNoClose
  epipe_dialog_detected = $EpipeDialogDetected
  launched_process_id = $Process.Id
  launched_process_exit_observed = $ExitObserved
  launched_process_exit_code = if ($ExitObserved) { $Process.ExitCode } else { $null }
  matching_processes_before_capture = @($MatchingProcesses | Select-Object ProcessId, Name, CommandLine)
  foreground_attempts = $ForegroundAttempts
  pre_capture_error_dialog_sweep_attempts = $PreCaptureErrorDialogSweepAttempts
  pre_capture_error_dialog_detection_no_close = $PreCaptureErrorDialogSweepAttempts
  error_dialog_close_attempts = $ErrorDialogCloseAttempts
  scoped_processes_stopped_after_capture = @($ScopedProcessesAfterCapture | Select-Object ProcessId, Name, CommandLine)
  command = $CursorExe + " " + (($CursorArgs | ForEach-Object { '"' + $_ + '"' }) -join " ")
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
  token_savings_claimed = $false
  semantic_quality_claimed = $false
  readiness_claimed = $false
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
