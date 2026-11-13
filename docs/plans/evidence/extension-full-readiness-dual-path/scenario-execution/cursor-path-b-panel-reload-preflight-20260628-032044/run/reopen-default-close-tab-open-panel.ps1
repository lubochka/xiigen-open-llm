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
public static class NativeWin4 {
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
  [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT4 rect);
  [DllImport("user32.dll")] public static extern bool SetCursorPos(int X, int Y);
  [DllImport("user32.dll")] public static extern void mouse_event(uint flags, uint dx, uint dy, uint data, UIntPtr extraInfo);
}
public struct RECT4 { public int Left; public int Top; public int Right; public int Bottom; }
"@

function Get-DefaultDevHost {
  $candidates = Get-CimInstance Win32_Process |
    Where-Object {
      $_.Name -eq "Cursor.exe" -and
      $_.CommandLine -match "--extensionDevelopmentPath" -and
      $_.CommandLine -match "C:\\Projects\\cursor-xiigen-smart-skills" -and
      $_.CommandLine -notmatch "clean-cursor-user-data"
    }
  foreach ($candidate in $candidates) {
    $process = Get-Process -Id $candidate.ProcessId -ErrorAction SilentlyContinue
    if ($process -and $process.MainWindowHandle -ne 0) {
      return $process
    }
  }
  return $null
}

function Focus-Window([IntPtr]$Handle) {
  [void][NativeWin4]::ShowWindow($Handle, 9)
  Start-Sleep -Milliseconds 300
  [void][NativeWin4]::SetForegroundWindow($Handle)
  Start-Sleep -Milliseconds 800
}

function Save-WindowScreenshot([IntPtr]$Handle, [string]$Path) {
  $rect = New-Object RECT4
  if (-not [NativeWin4]::GetWindowRect($Handle, [ref]$rect)) {
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

function Click-WindowPoint([IntPtr]$Handle, [int]$X, [int]$Y) {
  $rect = New-Object RECT4
  if (-not [NativeWin4]::GetWindowRect($Handle, [ref]$rect)) {
    throw "GetWindowRect failed."
  }
  [void][NativeWin4]::SetCursorPos($rect.Left + $X, $rect.Top + $Y)
  Start-Sleep -Milliseconds 200
  [NativeWin4]::mouse_event(0x0002, 0, 0, 0, [UIntPtr]::Zero)
  Start-Sleep -Milliseconds 100
  [NativeWin4]::mouse_event(0x0004, 0, 0, 0, [UIntPtr]::Zero)
  Start-Sleep -Milliseconds 800
}

function Send-CommandPaletteCommand([string]$CommandText) {
  [System.Windows.Forms.SendKeys]::SendWait("{F1}")
  Start-Sleep -Seconds 1
  [System.Windows.Forms.SendKeys]::SendWait($CommandText)
  Start-Sleep -Seconds 1
  [System.Windows.Forms.SendKeys]::SendWait("{ENTER}")
}

$extensionRepo = "C:\Projects\cursor-xiigen-smart-skills"
$cursorExe = "C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe"
$runLog = [ordered]@{
  started_at = (Get-Date).ToString("o")
  evidence_folder = $EvidenceFolder
  actions = @()
}

$allDevHosts = Get-Process Cursor -ErrorAction SilentlyContinue |
  Where-Object { $_.MainWindowTitle -like "*[Extension Development Host]*cursor-xiigen-smart-skills*Cursor*" }
foreach ($devHost in $allDevHosts) {
  [void]$devHost.CloseMainWindow()
  $runLog.actions += "requested CloseMainWindow for dev host pid $($devHost.Id)"
}
Start-Sleep -Seconds 5

Start-Process -FilePath $cursorExe -ArgumentList @("--extensionDevelopmentPath", $extensionRepo, "--new-window", $extensionRepo)
$runLog.actions += "started default-profile Cursor Extension Development Host"

$devHostProcess = $null
for ($i = 0; $i -lt 35; $i++) {
  Start-Sleep -Seconds 1
  $devHostProcess = Get-DefaultDevHost
  if ($devHostProcess) { break }
}
if (-not $devHostProcess) {
  throw "Default-profile Extension Development Host did not appear within 35 seconds."
}

Focus-Window $devHostProcess.MainWindowHandle
Save-WindowScreenshot $devHostProcess.MainWindowHandle (Join-Path $EvidenceFolder "after-default-devhost-reopen.png")

Click-WindowPoint $devHostProcess.MainWindowHandle 328 53
$runLog.actions += "clicked active XIIGen editor-tab close coordinate"
Save-WindowScreenshot $devHostProcess.MainWindowHandle (Join-Path $EvidenceFolder "after-default-close-restored-tab.png")

Focus-Window $devHostProcess.MainWindowHandle
Send-CommandPaletteCommand "XIIGen: Open Smart Skills Panel"
$runLog.actions += "sent F1 command: XIIGen: Open Smart Skills Panel"
Start-Sleep -Seconds 10

$devHostProcess = Get-DefaultDevHost
if (-not $devHostProcess) {
  throw "Default-profile Extension Development Host disappeared before final screenshot."
}
Focus-Window $devHostProcess.MainWindowHandle
$finalScreenshot = Join-Path $EvidenceFolder "after-default-reopen-command-panel.png"
Save-WindowScreenshot $devHostProcess.MainWindowHandle $finalScreenshot

$runLog.completed_at = (Get-Date).ToString("o")
$runLog.final_window = @{
  id = $devHostProcess.Id
  title = $devHostProcess.MainWindowTitle
  handle = ("0x{0:X}" -f $devHostProcess.MainWindowHandle.ToInt64())
}
$runLog.screenshots = @(
  (Join-Path $EvidenceFolder "after-default-devhost-reopen.png"),
  (Join-Path $EvidenceFolder "after-default-close-restored-tab.png"),
  $finalScreenshot
)

$runLogPath = Join-Path $EvidenceFolder "ui-default-reopen-run-log.json"
$runLog | ConvertTo-Json -Depth 6 | Set-Content -Path $runLogPath -Encoding UTF8
$runLog | ConvertTo-Json -Depth 6
