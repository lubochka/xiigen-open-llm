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
public static class NativeWin2 {
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
  [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT2 rect);
  [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
  [DllImport("user32.dll", CharSet=CharSet.Unicode)] public static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);
}
public struct RECT2 { public int Left; public int Top; public int Right; public int Bottom; }
"@

function Get-WindowTitle([IntPtr]$Handle) {
  $buffer = New-Object System.Text.StringBuilder 1024
  [void][NativeWin2]::GetWindowText($Handle, $buffer, $buffer.Capacity)
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
  [void][NativeWin2]::ShowWindow($Handle, 9)
  Start-Sleep -Milliseconds 300
  [void][NativeWin2]::SetForegroundWindow($Handle)
  Start-Sleep -Milliseconds 700
}

function Send-CommandPaletteCommand([string]$CommandText) {
  [System.Windows.Forms.SendKeys]::SendWait("^+p")
  Start-Sleep -Milliseconds 900
  [System.Windows.Forms.SendKeys]::SendWait($CommandText)
  Start-Sleep -Milliseconds 900
  [System.Windows.Forms.SendKeys]::SendWait("{ENTER}")
}

function Save-WindowScreenshot([IntPtr]$Handle, [string]$Path) {
  $rect = New-Object RECT2
  if (-not [NativeWin2]::GetWindowRect($Handle, [ref]$rect)) {
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

$hostProcess = Get-DevHostProcess
Focus-Window $hostProcess.MainWindowHandle
[System.Windows.Forms.SendKeys]::SendWait("^1")
Start-Sleep -Milliseconds 700
[System.Windows.Forms.SendKeys]::SendWait("^w")
Start-Sleep -Seconds 2
$runLog.actions += "focused first editor group and sent Ctrl+W to close stale panel tab"
Save-WindowScreenshot $hostProcess.MainWindowHandle (Join-Path $EvidenceFolder "after-close-stale-panel.png")

Send-CommandPaletteCommand "Developer: Restart Extension Host"
$runLog.actions += "sent command palette command: Developer: Restart Extension Host"
Start-Sleep -Seconds 8

$hostProcess = Get-DevHostProcess
Focus-Window $hostProcess.MainWindowHandle
Send-CommandPaletteCommand "XIIGen: Open Smart Skills Panel"
$runLog.actions += "sent command palette command: XIIGen: Open Smart Skills Panel"
Start-Sleep -Seconds 10

$hostProcess = Get-DevHostProcess
Focus-Window $hostProcess.MainWindowHandle
$finalScreenshot = Join-Path $EvidenceFolder "after-close-restart-reopen-panel.png"
Save-WindowScreenshot $hostProcess.MainWindowHandle $finalScreenshot

$runLog.completed_at = (Get-Date).ToString("o")
$runLog.final_window = @{
  id = $hostProcess.Id
  title = $hostProcess.MainWindowTitle
  handle = ("0x{0:X}" -f $hostProcess.MainWindowHandle.ToInt64())
  foreground_title = (Get-WindowTitle ([NativeWin2]::GetForegroundWindow()))
}
$runLog.screenshots = @(
  (Join-Path $EvidenceFolder "after-close-stale-panel.png"),
  $finalScreenshot
)

$runLogPath = Join-Path $EvidenceFolder "ui-restart-run-log.json"
$runLog | ConvertTo-Json -Depth 6 | Set-Content -Path $runLogPath -Encoding UTF8
$runLog | ConvertTo-Json -Depth 6
