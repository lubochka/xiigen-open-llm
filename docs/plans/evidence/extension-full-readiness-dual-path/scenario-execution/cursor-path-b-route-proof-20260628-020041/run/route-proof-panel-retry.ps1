param(
  [string]$EvidenceRoot = "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\cursor-path-b-route-proof-20260628-020041",
  [string]$SearchText = "Open Smart Skills Panel"
)

$ErrorActionPreference = "Stop"
$screenshotsDir = Join-Path $EvidenceRoot "screenshots"
$logsDir = Join-Path $EvidenceRoot "logs"
New-Item -ItemType Directory -Force -Path $screenshotsDir, $logsDir | Out-Null

$events = [System.Collections.Generic.List[object]]::new()
function Add-Event([string]$name, [hashtable]$data = @{}) {
  $events.Add([pscustomobject]@{ time = (Get-Date).ToString("o"); name = $name; data = $data }) | Out-Null
}

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class Win32RouteProofRetry {
  public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
  [DllImport("user32.dll")] public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
  [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr hWnd);
  [DllImport("user32.dll")] public static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);
  [DllImport("user32.dll")] public static extern int GetWindowTextLength(IntPtr hWnd);
  [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
  [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);
  [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
}
"@

function Get-WindowTitle([IntPtr]$handle) {
  $length = [Win32RouteProofRetry]::GetWindowTextLength($handle)
  if ($length -le 0) { return "" }
  $builder = [System.Text.StringBuilder]::new($length + 1)
  [void][Win32RouteProofRetry]::GetWindowText($handle, $builder, $builder.Capacity)
  return $builder.ToString()
}

function Get-TopLevelWindows {
  $windows = [System.Collections.Generic.List[object]]::new()
  $callback = [Win32RouteProofRetry+EnumWindowsProc]{
    param([IntPtr]$hWnd, [IntPtr]$lParam)
    if ([Win32RouteProofRetry]::IsWindowVisible($hWnd)) {
      $title = Get-WindowTitle $hWnd
      if ($title) {
        [uint32]$windowPid = 0
        [void][Win32RouteProofRetry]::GetWindowThreadProcessId($hWnd, [ref]$windowPid)
        $process = Get-Process -Id $windowPid -ErrorAction SilentlyContinue
        $windows.Add([pscustomobject]@{
          handle = ("0x{0:X}" -f $hWnd.ToInt64())
          processId = [int]$windowPid
          processName = if ($process) { $process.ProcessName } else { $null }
          title = $title
        }) | Out-Null
      }
    }
    return $true
  }
  [void][Win32RouteProofRetry]::EnumWindows($callback, [IntPtr]::Zero)
  return @($windows)
}

function Get-TargetCursorWindow {
  $target = @(Get-TopLevelWindows | Where-Object {
    $_.processName -eq "Cursor" -and
    $_.title -like "*[Extension Development Host]*" -and
    $_.title -like "*cursor-xiigen-smart-skills*"
  } | Select-Object -First 1)
  if ($target.Count -gt 0) { return $target[0] }
  return $null
}

function Focus-Window([string]$handleHex) {
  $handle = [IntPtr]([Convert]::ToInt64($handleHex.Replace("0x", ""), 16))
  [void][Win32RouteProofRetry]::ShowWindow($handle, 9)
  Start-Sleep -Milliseconds 250
  [void][Win32RouteProofRetry]::SetForegroundWindow($handle)
  Start-Sleep -Milliseconds 500
  return $handle
}

function Capture-Window([IntPtr]$handle, [string]$path) {
  $rect = New-Object Win32RouteProofRetry+RECT
  if (-not [Win32RouteProofRetry]::GetWindowRect($handle, [ref]$rect)) { throw "GetWindowRect failed" }
  $width = [Math]::Max(1, $rect.Right - $rect.Left)
  $height = [Math]::Max(1, $rect.Bottom - $rect.Top)
  $bitmap = [System.Drawing.Bitmap]::new($width, $height)
  $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
  $graphics.CopyFromScreen($rect.Left, $rect.Top, 0, 0, [System.Drawing.Size]::new($width, $height))
  $bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
  $graphics.Dispose()
  $bitmap.Dispose()
  return $path
}

function Send-KeyChord([string]$keys, [int]$delayMs = 700) {
  [System.Windows.Forms.SendKeys]::SendWait($keys)
  Start-Sleep -Milliseconds $delayMs
}

$target = Get-TargetCursorWindow
if (-not $target) { throw "TARGET_WINDOW_NOT_FOUND" }
$handle = Focus-Window -handleHex $target.handle
Add-Event "target_window_focused_for_panel_retry" @{ title = $target.title; handle = $target.handle }

Send-KeyChord "{ESC}" 700
Send-KeyChord "^{F4}" 1200
$target = Get-TargetCursorWindow
if ($target) {
  $handle = Focus-Window -handleHex $target.handle
  Add-Event "after_escape_and_close_editor_shortcut" @{ title = $target.title }
}
Capture-Window -handle $handle -path (Join-Path $screenshotsDir "05-panel-retry-after-close-shortcut.png") | Out-Null

Send-KeyChord "^+p" 900
Send-KeyChord "^a" 300
[System.Windows.Forms.Clipboard]::SetText($SearchText)
Add-Event "clipboard_set_for_exact_panel_search" @{ text = $SearchText; scenarioPrompt = $false }
Send-KeyChord "^v" 900
Capture-Window -handle $handle -path (Join-Path $screenshotsDir "06-panel-retry-command-palette.png") | Out-Null

Send-KeyChord "{ENTER}" 5000
Start-Sleep -Seconds 6
$target = Get-TargetCursorWindow
if ($target) {
  $handle = Focus-Window -handleHex $target.handle
  Add-Event "after_panel_command_enter" @{ title = $target.title }
}
Capture-Window -handle $handle -path (Join-Path $screenshotsDir "07-panel-retry-route-result.png") | Out-Null

@{
  schemaVersion = 1
  capturedAt = (Get-Date).ToString("o")
  cursorWindows = @(Get-TopLevelWindows | Where-Object { $_.processName -eq "Cursor" -or $_.title -like "*Cursor*" })
} | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $logsDir "window-inventory-panel-retry-final.json") -Encoding UTF8

$events | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $logsDir "panel-retry-events.json") -Encoding UTF8
@{
  status = "PANEL_RETRY_COMPLETED"
  searchText = $SearchText
  scenarioPromptTyped = $false
  scenarioPromptSubmitted = $false
  screenshots = @(
    (Join-Path $screenshotsDir "05-panel-retry-after-close-shortcut.png"),
    (Join-Path $screenshotsDir "06-panel-retry-command-palette.png"),
    (Join-Path $screenshotsDir "07-panel-retry-route-result.png")
  )
} | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $logsDir "panel-retry-summary.json") -Encoding UTF8

Write-Output "PANEL_RETRY_COMPLETED"
