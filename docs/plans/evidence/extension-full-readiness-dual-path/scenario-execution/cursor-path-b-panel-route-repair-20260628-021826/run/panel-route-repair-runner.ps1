param(
  [ValidateSet("Prepare", "PrepareTyped", "Activate", "CaptureOnly")]
  [string]$Mode,
  [string]$EvidenceRoot = "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\cursor-path-b-panel-route-repair-20260628-021826",
  [string]$SearchText = "Open Smart Skills Panel"
)

$ErrorActionPreference = "Stop"
$screenshotsDir = Join-Path $EvidenceRoot "screenshots"
$logsDir = Join-Path $EvidenceRoot "logs"
New-Item -ItemType Directory -Force -Path $screenshotsDir, $logsDir | Out-Null

$eventsPath = Join-Path $logsDir "runner-events.json"
$events = [System.Collections.Generic.List[object]]::new()
if (Test-Path -LiteralPath $eventsPath) {
  $previous = Get-Content -LiteralPath $eventsPath -Raw | ConvertFrom-Json
  foreach ($event in @($previous)) { $events.Add($event) | Out-Null }
}

function Add-Event([string]$name, [hashtable]$data = @{}) {
  $events.Add([pscustomobject]@{
    time = (Get-Date).ToString("o")
    name = $name
    data = $data
  }) | Out-Null
}

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class Win32PanelRouteRepair {
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
  $length = [Win32PanelRouteRepair]::GetWindowTextLength($handle)
  if ($length -le 0) { return "" }
  $builder = [System.Text.StringBuilder]::new($length + 1)
  [void][Win32PanelRouteRepair]::GetWindowText($handle, $builder, $builder.Capacity)
  return $builder.ToString()
}

function Get-TopLevelWindows {
  $windows = [System.Collections.Generic.List[object]]::new()
  $callback = [Win32PanelRouteRepair+EnumWindowsProc]{
    param([IntPtr]$hWnd, [IntPtr]$lParam)
    if ([Win32PanelRouteRepair]::IsWindowVisible($hWnd)) {
      $title = Get-WindowTitle $hWnd
      if ($title) {
        [uint32]$windowPid = 0
        [void][Win32PanelRouteRepair]::GetWindowThreadProcessId($hWnd, [ref]$windowPid)
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
  [void][Win32PanelRouteRepair]::EnumWindows($callback, [IntPtr]::Zero)
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
  [void][Win32PanelRouteRepair]::ShowWindow($handle, 9)
  Start-Sleep -Milliseconds 300
  [void][Win32PanelRouteRepair]::SetForegroundWindow($handle)
  Start-Sleep -Milliseconds 700
  return $handle
}

function Capture-Window([IntPtr]$handle, [string]$path) {
  $rect = New-Object Win32PanelRouteRepair+RECT
  if (-not [Win32PanelRouteRepair]::GetWindowRect($handle, [ref]$rect)) {
    throw "GetWindowRect failed for $handle"
  }
  $width = [Math]::Max(1, $rect.Right - $rect.Left)
  $height = [Math]::Max(1, $rect.Bottom - $rect.Top)
  $bitmap = [System.Drawing.Bitmap]::new($width, $height)
  $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
  $graphics.CopyFromScreen($rect.Left, $rect.Top, 0, 0, [System.Drawing.Size]::new($width, $height))
  $bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
  $graphics.Dispose()
  $bitmap.Dispose()
  return [pscustomobject]@{ path = $path; width = $width; height = $height; left = $rect.Left; top = $rect.Top }
}

function Save-WindowInventory([string]$path, [string]$stage) {
  [pscustomobject]@{
    schemaVersion = 1
    stage = $stage
    capturedAt = (Get-Date).ToString("o")
    cursorWindows = @(Get-TopLevelWindows | Where-Object { $_.processName -eq "Cursor" -or $_.title -like "*Cursor*" })
  } | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $path -Encoding UTF8
}

function Send-KeyChord([string]$keys, [int]$delayMs = 700) {
  [System.Windows.Forms.SendKeys]::SendWait($keys)
  Start-Sleep -Milliseconds $delayMs
}

$target = Get-TargetCursorWindow
if (-not $target) { throw "TARGET_WINDOW_NOT_FOUND" }
$handle = Focus-Window -handleHex $target.handle
Add-Event "target_window_focused" @{ mode = $Mode; title = $target.title; handle = $target.handle }

if ($Mode -eq "Prepare") {
  Save-WindowInventory -path (Join-Path $EvidenceRoot "window-inventory.json") -stage "prepare_before"
  Send-KeyChord "{ESC}" 300
  Send-KeyChord "{ESC}" 300
  Send-KeyChord "{ESC}" 500
  $shot1 = Capture-Window -handle $handle -path (Join-Path $screenshotsDir "01-before-command-palette.png")
  Add-Event "screenshot" @{ stage = "before_command_palette"; path = $shot1.path }

  Send-KeyChord "{F1}" 900
  Send-KeyChord "^a" 250
  [System.Windows.Forms.Clipboard]::SetText($SearchText)
  Add-Event "clipboard_set_for_exact_panel_search" @{ text = $SearchText; scenarioPrompt = $false }
  Send-KeyChord "^v" 1200
  $shot2 = Capture-Window -handle $handle -path (Join-Path $screenshotsDir "02-panel-command-before-activation.png")
  Add-Event "screenshot" @{ stage = "panel_command_before_activation"; path = $shot2.path }

  [pscustomobject]@{
    status = "PREPARE_COMPLETED"
    searchText = $SearchText
    activationPerformed = $false
    scenarioPromptTyped = $false
    scenarioPromptSubmitted = $false
    screenshots = @($shot1.path, $shot2.path)
  } | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $logsDir "prepare-summary.json") -Encoding UTF8
  $events | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $eventsPath -Encoding UTF8
  Write-Output "PREPARE_COMPLETED"
  exit 0
}

if ($Mode -eq "PrepareTyped") {
  Save-WindowInventory -path (Join-Path $logsDir "window-inventory-typed-prepare.json") -stage "typed_prepare_before"
  Send-KeyChord "{ESC}" 300
  Send-KeyChord "{ESC}" 300
  Send-KeyChord "{ESC}" 500
  $shot1 = Capture-Window -handle $handle -path (Join-Path $screenshotsDir "05-typed-before-command-palette.png")
  Add-Event "screenshot" @{ stage = "typed_before_command_palette"; path = $shot1.path }

  Send-KeyChord "{F1}" 900
  Send-KeyChord "^a" 250
  [System.Windows.Forms.SendKeys]::SendWait($SearchText)
  Start-Sleep -Milliseconds 1400
  Add-Event "typed_exact_panel_search" @{ text = $SearchText; scenarioPrompt = $false }
  $shot2 = Capture-Window -handle $handle -path (Join-Path $screenshotsDir "06-typed-panel-command-before-activation.png")
  Add-Event "screenshot" @{ stage = "typed_panel_command_before_activation"; path = $shot2.path }

  [pscustomobject]@{
    status = "PREPARE_TYPED_COMPLETED"
    searchText = $SearchText
    activationPerformed = $false
    scenarioPromptTyped = $false
    scenarioPromptSubmitted = $false
    screenshots = @($shot1.path, $shot2.path)
  } | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $logsDir "prepare-typed-summary.json") -Encoding UTF8
  $events | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $eventsPath -Encoding UTF8
  Write-Output "PREPARE_TYPED_COMPLETED"
  exit 0
}

if ($Mode -eq "Activate") {
  $shot3 = Capture-Window -handle $handle -path (Join-Path $screenshotsDir "03-immediate-before-enter.png")
  Add-Event "screenshot" @{ stage = "immediate_before_enter"; path = $shot3.path }
  Send-KeyChord "{ENTER}" 3500
  Start-Sleep -Seconds 3
  $target = Get-TargetCursorWindow
  if ($target) {
    $handle = Focus-Window -handleHex $target.handle
    Add-Event "target_window_refocused_after_enter" @{ title = $target.title; handle = $target.handle }
  }
  $shot4 = Capture-Window -handle $handle -path (Join-Path $screenshotsDir "04-after-panel-command-activation.png")
  Add-Event "screenshot" @{ stage = "after_panel_command_activation"; path = $shot4.path }
  Save-WindowInventory -path (Join-Path $logsDir "window-inventory-after-activation.json") -stage "after_activation"

  [pscustomobject]@{
    status = "ACTIVATE_COMPLETED"
    activationPerformed = $true
    scenarioPromptTyped = $false
    scenarioPromptSubmitted = $false
    screenshots = @($shot3.path, $shot4.path)
  } | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $logsDir "activate-summary.json") -Encoding UTF8
  $events | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $eventsPath -Encoding UTF8
  Write-Output "ACTIVATE_COMPLETED"
  exit 0
}

if ($Mode -eq "CaptureOnly") {
  Start-Sleep -Seconds 5
  $target = Get-TargetCursorWindow
  if (-not $target) { throw "TARGET_WINDOW_NOT_FOUND_FOR_CAPTURE_ONLY" }
  $handle = Focus-Window -handleHex $target.handle
  $shot = Capture-Window -handle $handle -path (Join-Path $screenshotsDir "07-late-after-panel-command-activation.png")
  Add-Event "screenshot" @{ stage = "late_after_panel_command_activation"; path = $shot.path }
  Save-WindowInventory -path (Join-Path $logsDir "window-inventory-late-after-activation.json") -stage "late_after_activation"
  [pscustomobject]@{
    status = "CAPTURE_ONLY_COMPLETED"
    scenarioPromptTyped = $false
    scenarioPromptSubmitted = $false
    screenshots = @($shot.path)
  } | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $logsDir "capture-only-summary.json") -Encoding UTF8
  $events | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $eventsPath -Encoding UTF8
  Write-Output "CAPTURE_ONLY_COMPLETED"
  exit 0
}
