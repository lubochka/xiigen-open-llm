param(
  [Parameter(Mandatory = $true)]
  [string]$EvidenceFolder
)

$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class NativeWin3 {
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
  [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT3 rect);
  [DllImport("user32.dll", CharSet=CharSet.Unicode)] public static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);
}
public struct RECT3 { public int Left; public int Top; public int Right; public int Bottom; }
"@

function Get-WindowTitle([IntPtr]$Handle) {
  $buffer = New-Object System.Text.StringBuilder 1024
  [void][NativeWin3]::GetWindowText($Handle, $buffer, $buffer.Capacity)
  return $buffer.ToString()
}

function Focus-Window([IntPtr]$Handle) {
  [void][NativeWin3]::ShowWindow($Handle, 9)
  Start-Sleep -Milliseconds 300
  [void][NativeWin3]::SetForegroundWindow($Handle)
  Start-Sleep -Milliseconds 800
}

function Save-WindowScreenshot([IntPtr]$Handle, [string]$Path) {
  $rect = New-Object RECT3
  if (-not [NativeWin3]::GetWindowRect($Handle, [ref]$rect)) {
    throw "GetWindowRect failed."
  }
  $width = [Math]::Max(1, $rect.Right - $rect.Left)
  $height = [Math]::Max(1, $rect.Bottom - $rect.Top)
  $bitmap = New-Object System.Drawing.Bitmap $width, $height
  $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
  $graphics.CopyFromScreen($rect.Left, $rect.Top, 0, 0, $bitmap.Size)
  $bitmap.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png)
  $graphics.Dispose()
  $bitmap.Dispose()
}

function Send-CommandPaletteCommand([string]$CommandText) {
  [System.Windows.Forms.SendKeys]::SendWait("^+p")
  Start-Sleep -Seconds 1
  [System.Windows.Forms.SendKeys]::SendWait($CommandText)
  Start-Sleep -Seconds 1
  [System.Windows.Forms.SendKeys]::SendWait("{ENTER}")
}

function Find-DevHostProcessForUserData([string]$UserDataDir) {
  $escaped = [Regex]::Escape($UserDataDir)
  $candidates = Get-CimInstance Win32_Process |
    Where-Object { $_.Name -eq "Cursor.exe" -and $_.CommandLine -match $escaped -and $_.CommandLine -match "--extensionDevelopmentPath" }
  foreach ($candidate in $candidates) {
    $process = Get-Process -Id $candidate.ProcessId -ErrorAction SilentlyContinue
    if ($process -and $process.MainWindowHandle -ne 0) {
      return $process
    }
  }
  return $null
}

$extensionRepo = "C:\Projects\cursor-xiigen-smart-skills"
$cursorExe = "C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe"
$userDataDir = Join-Path $EvidenceFolder "clean-cursor-user-data"
$runLog = [ordered]@{
  started_at = (Get-Date).ToString("o")
  evidence_folder = $EvidenceFolder
  clean_user_data_dir = $userDataDir
  actions = @()
}

New-Item -ItemType Directory -Path $userDataDir -Force | Out-Null

$oldHosts = Get-Process Cursor -ErrorAction SilentlyContinue |
  Where-Object { $_.MainWindowTitle -like "*[Extension Development Host]*cursor-xiigen-smart-skills*Cursor*" }
foreach ($oldHost in $oldHosts) {
  $runLog.actions += "requested CloseMainWindow for old dev host pid $($oldHost.Id)"
  [void]$oldHost.CloseMainWindow()
}
Start-Sleep -Seconds 5

$args = @(
  "--user-data-dir=$userDataDir",
  "--extensionDevelopmentPath",
  $extensionRepo,
  "--new-window",
  $extensionRepo
)
Start-Process -FilePath $cursorExe -ArgumentList $args
$runLog.actions += "started Cursor with clean user-data and extensionDevelopmentPath"

$newHost = $null
for ($i = 0; $i -lt 30; $i++) {
  Start-Sleep -Seconds 1
  $newHost = Find-DevHostProcessForUserData $userDataDir
  if ($newHost) { break }
}
if (-not $newHost) {
  throw "Clean Extension Development Host did not appear within 30 seconds."
}

Focus-Window $newHost.MainWindowHandle
$runLog.clean_host_window = @{
  id = $newHost.Id
  title = $newHost.MainWindowTitle
  handle = ("0x{0:X}" -f $newHost.MainWindowHandle.ToInt64())
  title_from_handle = (Get-WindowTitle $newHost.MainWindowHandle)
}
Save-WindowScreenshot $newHost.MainWindowHandle (Join-Path $EvidenceFolder "after-clean-devhost-launch.png")

Send-CommandPaletteCommand "XIIGen: Open Smart Skills Panel"
$runLog.actions += "sent command palette command in clean dev host: XIIGen: Open Smart Skills Panel"
Start-Sleep -Seconds 10

$newHost = Find-DevHostProcessForUserData $userDataDir
if (-not $newHost) {
  throw "Clean Extension Development Host disappeared before final screenshot."
}
Focus-Window $newHost.MainWindowHandle
$finalScreenshot = Join-Path $EvidenceFolder "after-clean-devhost-open-panel.png"
Save-WindowScreenshot $newHost.MainWindowHandle $finalScreenshot

$runLog.completed_at = (Get-Date).ToString("o")
$runLog.final_window = @{
  id = $newHost.Id
  title = $newHost.MainWindowTitle
  handle = ("0x{0:X}" -f $newHost.MainWindowHandle.ToInt64())
  title_from_handle = (Get-WindowTitle $newHost.MainWindowHandle)
}
$runLog.screenshots = @(
  (Join-Path $EvidenceFolder "after-clean-devhost-launch.png"),
  $finalScreenshot
)

$runLogPath = Join-Path $EvidenceFolder "ui-clean-devhost-run-log.json"
$runLog | ConvertTo-Json -Depth 6 | Set-Content -Path $runLogPath -Encoding UTF8
$runLog | ConvertTo-Json -Depth 6
