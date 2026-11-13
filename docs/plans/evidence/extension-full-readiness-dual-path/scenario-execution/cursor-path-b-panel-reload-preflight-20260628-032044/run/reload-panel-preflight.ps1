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
public static class NativeWin {
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
  [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);
  [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
  [DllImport("user32.dll", CharSet=CharSet.Unicode)] public static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);
}
public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
"@

function Get-WindowTitle([IntPtr]$Handle) {
  $buffer = New-Object System.Text.StringBuilder 1024
  [void][NativeWin]::GetWindowText($Handle, $buffer, $buffer.Capacity)
  return $buffer.ToString()
}

function Get-DevHostProcess {
  $matches = Get-Process Cursor -ErrorAction SilentlyContinue |
    Where-Object { $_.MainWindowTitle -like "*[Extension Development Host]*cursor-xiigen-smart-skills*Cursor*" } |
    Sort-Object Id
  if (-not $matches) {
    throw "Extension Development Host window was not found."
  }
  return $matches[0]
}

function Focus-Window([IntPtr]$Handle) {
  [void][NativeWin]::ShowWindow($Handle, 9)
  Start-Sleep -Milliseconds 300
  [void][NativeWin]::SetForegroundWindow($Handle)
  Start-Sleep -Milliseconds 700
}

function Send-CommandPaletteCommand([string]$CommandText) {
  [System.Windows.Forms.SendKeys]::SendWait("^+p")
  Start-Sleep -Milliseconds 800
  [System.Windows.Forms.SendKeys]::SendWait($CommandText)
  Start-Sleep -Milliseconds 800
  [System.Windows.Forms.SendKeys]::SendWait("{ENTER}")
}

function Save-WindowScreenshot([IntPtr]$Handle, [string]$Path) {
  $rect = New-Object RECT
  if (-not [NativeWin]::GetWindowRect($Handle, [ref]$rect)) {
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

$runLog = [ordered]@{
  started_at = (Get-Date).ToString("o")
  evidence_folder = $EvidenceFolder
  actions = @()
}

New-Item -ItemType Directory -Path $EvidenceFolder -Force | Out-Null
$beforePath = Join-Path $EvidenceFolder "before-reload-window.png"
$afterPath = Join-Path $EvidenceFolder "after-reload-open-panel.png"

$hostProcess = Get-DevHostProcess
$runLog.initial_window = @{
  id = $hostProcess.Id
  title = $hostProcess.MainWindowTitle
  handle = ("0x{0:X}" -f $hostProcess.MainWindowHandle.ToInt64())
}

Focus-Window $hostProcess.MainWindowHandle
Save-WindowScreenshot $hostProcess.MainWindowHandle $beforePath
$runLog.actions += "captured before-reload screenshot"

Send-CommandPaletteCommand "Developer: Reload Window"
$runLog.actions += "sent command palette command: Developer: Reload Window"
Start-Sleep -Seconds 8

$hostProcess = Get-DevHostProcess
Focus-Window $hostProcess.MainWindowHandle
$runLog.after_reload_window = @{
  id = $hostProcess.Id
  title = $hostProcess.MainWindowTitle
  handle = ("0x{0:X}" -f $hostProcess.MainWindowHandle.ToInt64())
  foreground_title = (Get-WindowTitle ([NativeWin]::GetForegroundWindow()))
}

Send-CommandPaletteCommand "XIIGen: Open Smart Skills Panel"
$runLog.actions += "sent command palette command: XIIGen: Open Smart Skills Panel"
Start-Sleep -Seconds 8

$hostProcess = Get-DevHostProcess
Focus-Window $hostProcess.MainWindowHandle
Save-WindowScreenshot $hostProcess.MainWindowHandle $afterPath
$runLog.actions += "captured after-reload/open-panel screenshot"
$runLog.completed_at = (Get-Date).ToString("o")
$runLog.screenshots = @($beforePath, $afterPath)

$runLogPath = Join-Path $EvidenceFolder "ui-run-log.json"
$runLog | ConvertTo-Json -Depth 6 | Set-Content -Path $runLogPath -Encoding UTF8
$runLog | ConvertTo-Json -Depth 6
