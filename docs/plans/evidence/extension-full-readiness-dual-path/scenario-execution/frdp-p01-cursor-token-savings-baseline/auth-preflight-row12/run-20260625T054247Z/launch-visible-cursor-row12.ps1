$ErrorActionPreference = "Stop"

$RunRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$CursorExe = "C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe"
$ExtensionRoot = "C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills"
$UserDataDir = Join-Path $RunRoot "user-data"
$ExtensionsDir = Join-Path $RunRoot "extensions"
$TestPath = Join-Path $RunRoot "visible-ui-row12-extension-test.js"
$LaunchJsonPath = Join-Path $RunRoot "visible-ui-row12-launch.json"
$ScreenshotPath = Join-Path $RunRoot "screenshots\cursor-initial-visible-window.png"

New-Item -ItemType Directory -Force $UserDataDir | Out-Null
New-Item -ItemType Directory -Force $ExtensionsDir | Out-Null
New-Item -ItemType Directory -Force (Split-Path -Parent $ScreenshotPath) | Out-Null

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

function ConvertTo-CommandLineArgument {
  param([string]$Value)
  if ($Value -match '[\s"]') {
    $Escaped = $Value -replace '"', '\"'
    return '"' + $Escaped + '"'
  }
  return $Value
}

$ProcessInfo = New-Object System.Diagnostics.ProcessStartInfo
$ProcessInfo.FileName = $CursorExe
$ProcessInfo.WorkingDirectory = $ExtensionRoot
$ProcessInfo.UseShellExecute = $true
$ProcessInfo.CreateNoWindow = $false
$ProcessInfo.WindowStyle = [System.Diagnostics.ProcessWindowStyle]::Normal
$ProcessInfo.Arguments = ($CursorArgs | ForEach-Object { ConvertTo-CommandLineArgument $_ }) -join " "

$Process = New-Object System.Diagnostics.Process
$Process.StartInfo = $ProcessInfo
[void]$Process.Start()

Start-Sleep -Seconds 12

$MatchingProcesses = @(Get-CimInstance Win32_Process | Where-Object {
  $_.Name -eq "Cursor.exe" -and $_.CommandLine -and $_.CommandLine.Contains($RunRoot)
})

$ForegroundAttempts = @()
$Windows = New-Object System.Collections.Generic.List[object]
try {
  Add-Type @"
using System;
using System.Text;
using System.Runtime.InteropServices;
public class Row12LaunchWindowTools {
  [DllImport("user32.dll")]
  public static extern bool SetForegroundWindow(IntPtr hWnd);
  [DllImport("user32.dll")]
  public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
  public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
  [DllImport("user32.dll")]
  public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
  [DllImport("user32.dll")]
  public static extern bool IsWindowVisible(IntPtr hWnd);
  [DllImport("user32.dll", CharSet=CharSet.Auto)]
  public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
  [DllImport("user32.dll", CharSet=CharSet.Auto)]
  public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
  [DllImport("user32.dll")]
  public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
}
"@

  $ScopedProcessIds = @($MatchingProcesses | ForEach-Object { [int]$_.ProcessId })
  [Row12LaunchWindowTools]::EnumWindows({
    param([IntPtr]$Handle, [IntPtr]$Param)
    if (-not [Row12LaunchWindowTools]::IsWindowVisible($Handle)) {
      return $true
    }
    $WindowPid = 0
    [void][Row12LaunchWindowTools]::GetWindowThreadProcessId($Handle, [ref]$WindowPid)
    if ($ScopedProcessIds -notcontains [int]$WindowPid) {
      return $true
    }
    $TitleBuilder = New-Object System.Text.StringBuilder 512
    $ClassBuilder = New-Object System.Text.StringBuilder 256
    [void][Row12LaunchWindowTools]::GetWindowText($Handle, $TitleBuilder, $TitleBuilder.Capacity)
    [void][Row12LaunchWindowTools]::GetClassName($Handle, $ClassBuilder, $ClassBuilder.Capacity)
    [void][Row12LaunchWindowTools]::ShowWindow($Handle, 9)
    [void][Row12LaunchWindowTools]::SetForegroundWindow($Handle)
    $Windows.Add([pscustomobject]@{
      hwnd = $Handle.ToInt64()
      process_id = [int]$WindowPid
      class_name = $ClassBuilder.ToString()
      title = $TitleBuilder.ToString()
    })
    $Script:ForegroundAttempts += [pscustomobject]@{
      process_id = [int]$WindowPid
      window_handle = $Handle.ToInt64()
      attempted = $true
    }
    return $true
  }, [IntPtr]::Zero) | Out-Null
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

$Capture = [pscustomobject]@{
  schema_version = "frdp-p01-row12-auth-preflight-launch/v1"
  generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
  row12_run_folder = $RunRoot
  status = if ($MatchingProcesses.Count -gt 0 -and $Windows.Count -gt 0) { "CURSOR_VISIBLE_WINDOW_FOUND" } else { "CURSOR_VISIBLE_WINDOW_NOT_FOUND_WITH_EVIDENCE" }
  cursor_executable = $CursorExe
  extension_root = $ExtensionRoot
  extension_tests_path = $TestPath
  fresh_user_data_dir = $UserDataDir
  fresh_extensions_dir = $ExtensionsDir
  launched_process_id = $Process.Id
  matching_processes = @($MatchingProcesses | Select-Object ProcessId, Name, CommandLine)
  visible_windows = $Windows
  foreground_attempts = $ForegroundAttempts
  screenshot = $ScreenshotInfo
  screenshot_error = $ScreenshotError
  command = $CursorExe + " " + (($CursorArgs | ForEach-Object { '"' + $_ + '"' }) -join " ")
  opens_xiigen_panel = $false
  types_or_submits_row12_prompt = $false
  external_ai_send = $false
  no_secrets_captured = $true
}

$Capture | ConvertTo-Json -Depth 8 | Set-Content -Path $LaunchJsonPath -Encoding UTF8
Write-Output $Capture.status
Write-Output $LaunchJsonPath

if ($Capture.status -ne "CURSOR_VISIBLE_WINDOW_FOUND") {
  exit 3
}
