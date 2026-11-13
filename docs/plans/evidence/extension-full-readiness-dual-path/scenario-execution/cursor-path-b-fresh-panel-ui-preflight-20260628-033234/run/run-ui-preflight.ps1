param(
  [Parameter(Mandatory=$true)][string]$EvidenceRoot
)
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"
$Screens = Join-Path $EvidenceRoot "screenshots"
$LogPath = Join-Path $EvidenceRoot "action-log.md"
New-Item -ItemType Directory -Force -Path $Screens | Out-Null
$events = New-Object System.Collections.Generic.List[string]
function Add-Event([string]$Text) {
  $stamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
  $line = "- $stamp - $Text"
  $events.Add($line) | Out-Null
  Write-Output $line
}
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class UiNative {
  [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
  [DllImport("user32.dll")] public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
}
"@
function Get-TargetWindow {
  $matches = Get-Process Cursor -ErrorAction SilentlyContinue | Where-Object {
    $_.MainWindowTitle -like "*[Extension Development Host]*" -and $_.MainWindowTitle -like "*cursor-xiigen-smart-skills*"
  } | Sort-Object Id
  if (-not $matches) { throw "No Cursor Extension Development Host window for cursor-xiigen-smart-skills was found." }
  return $matches[0]
}
function Capture-Window([System.IntPtr]$Hwnd, [string]$Name) {
  $rect = New-Object UiNative+RECT
  if (-not [UiNative]::GetWindowRect($Hwnd, [ref]$rect)) { throw "GetWindowRect failed for $Hwnd" }
  $width = [Math]::Max(1, $rect.Right - $rect.Left)
  $height = [Math]::Max(1, $rect.Bottom - $rect.Top)
  $bmp = New-Object System.Drawing.Bitmap $width, $height
  $gfx = [System.Drawing.Graphics]::FromImage($bmp)
  try {
    $gfx.CopyFromScreen($rect.Left, $rect.Top, 0, 0, (New-Object System.Drawing.Size $width, $height))
    $path = Join-Path $Screens $Name
    $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    Add-Event "Screenshot saved: screenshots/$Name"
    return $path
  } finally {
    $gfx.Dispose()
    $bmp.Dispose()
  }
}
$target = Get-TargetWindow
Add-Event "Focused target window: PID $($target.Id), title: $($target.MainWindowTitle)"
[UiNative]::ShowWindowAsync($target.MainWindowHandle, 9) | Out-Null
Start-Sleep -Milliseconds 500
[UiNative]::SetForegroundWindow($target.MainWindowHandle) | Out-Null
Start-Sleep -Milliseconds 1000
Capture-Window $target.MainWindowHandle "00-host-before-command.png" | Out-Null
Add-Event "Opened command palette with Ctrl+Shift+P."
[System.Windows.Forms.SendKeys]::SendWait("^+p")
Start-Sleep -Milliseconds 900
[System.Windows.Forms.SendKeys]::SendWait("^a")
Start-Sleep -Milliseconds 150
Add-Event "Typed exact panel command label filter: XIIGen: Open Smart Skills Panel."
[System.Windows.Forms.SendKeys]::SendWait("XIIGen: Open Smart Skills Panel")
Start-Sleep -Milliseconds 1300
Capture-Window $target.MainWindowHandle "01-command-palette-open-smart-skills-panel-choice.png" | Out-Null
Add-Event "Pressed Enter to choose the visible filtered command."
[System.Windows.Forms.SendKeys]::SendWait("{ENTER}")
Start-Sleep -Milliseconds 4500
Capture-Window $target.MainWindowHandle "02-panel-after-command.png" | Out-Null
$events | Set-Content -Path $LogPath -Encoding UTF8
Write-Output "EVIDENCE_ROOT=$EvidenceRoot"
