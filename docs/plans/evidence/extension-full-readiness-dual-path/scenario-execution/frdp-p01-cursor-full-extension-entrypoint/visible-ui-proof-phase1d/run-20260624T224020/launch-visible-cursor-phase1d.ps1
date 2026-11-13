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
$ProcessInfo.UseShellExecute = $true
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
[void]$Process.Start()

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
try {
  Add-Type @"
using System;
using System.Runtime.InteropServices;
public class Win32WindowTools {
  [DllImport("user32.dll")]
  public static extern bool SetForegroundWindow(IntPtr hWnd);
  [DllImport("user32.dll")]
  public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
}
"@
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

$ScreenshotError = $null
try {
  Add-Type -AssemblyName System.Windows.Forms
  Add-Type -AssemblyName System.Drawing
  $Bounds = [System.Windows.Forms.SystemInformation]::VirtualScreen
  $Bitmap = New-Object System.Drawing.Bitmap $Bounds.Width, $Bounds.Height
  $Graphics = [System.Drawing.Graphics]::FromImage($Bitmap)
  $Graphics.CopyFromScreen($Bounds.X, $Bounds.Y, 0, 0, $Bounds.Size)
  $Bitmap.Save($ScreenshotPath, [System.Drawing.Imaging.ImageFormat]::Png)
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

$ValidationPresent = Test-Path $ValidationPath
$FailurePresent = Test-Path $FailurePath
$Capture = [pscustomobject]@{
  schema_version = "frdp-p01-visible-ui-phase1d-desktop-capture/v1"
  generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
  status = if ($ValidationPresent -and $ScreenshotInfo) { "ACCEPTABLE_EVIDENCE" } elseif ($ValidationPresent) { "NEEDS_FIX" } else { "BLOCKED_WITH_NEXT_ACTION" }
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
  observed_state = $ObservedState
  validation_json_present = $ValidationPresent
  harness_failure_json_present = $FailurePresent
  screenshot = $ScreenshotInfo
  screenshot_error = $ScreenshotError
  launched_process_id = $Process.Id
  launched_process_exit_observed = $ExitObserved
  launched_process_exit_code = if ($ExitObserved) { $Process.ExitCode } else { $null }
  matching_processes_before_capture = @($MatchingProcesses | Select-Object ProcessId, Name, CommandLine)
  foreground_attempts = $ForegroundAttempts
  scoped_processes_stopped_after_capture = @($ScopedProcessesAfterCapture | Select-Object ProcessId, Name, CommandLine)
  command = $CursorExe + " " + (($CursorArgs | ForEach-Object { '"' + $_ + '"' }) -join " ")
  stdout_log = $null
  stderr_log = $null
  stdio_redirected = $false
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
