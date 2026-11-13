param(
  [string]$OutputDir = "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\cursor-ext-path-b-same-row23-visible-ui-route-01"
)

$ErrorActionPreference = "Stop"
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName UIAutomationClient
Add-Type @"
using System;
using System.Runtime.InteropServices;

public static class XiigenCursorNative {
  [StructLayout(LayoutKind.Sequential)]
  public struct RECT {
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;
  }

  [DllImport("user32.dll")]
  public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

  [DllImport("user32.dll")]
  public static extern bool IsWindowVisible(IntPtr hWnd);

  [DllImport("user32.dll")]
  public static extern bool IsIconic(IntPtr hWnd);

  [DllImport("user32.dll")]
  public static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);

  [DllImport("user32.dll")]
  public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

  [DllImport("user32.dll")]
  public static extern bool SetForegroundWindow(IntPtr hWnd);
}
"@

function Get-WindowRectObject {
  param([IntPtr]$Handle)
  $rect = New-Object XiigenCursorNative+RECT
  $ok = [XiigenCursorNative]::GetWindowRect($Handle, [ref]$rect)
  if (-not $ok) { return $null }
  [pscustomobject]@{
    left = $rect.Left
    top = $rect.Top
    right = $rect.Right
    bottom = $rect.Bottom
    width = $rect.Right - $rect.Left
    height = $rect.Bottom - $rect.Top
  }
}

function Get-ImageStats {
  param([System.Drawing.Bitmap]$Bitmap)
  $sampleStepX = [Math]::Max(1, [Math]::Floor($Bitmap.Width / 80))
  $sampleStepY = [Math]::Max(1, [Math]::Floor($Bitmap.Height / 80))
  $count = 0
  $sum = 0.0
  $sumSq = 0.0
  $nonWhite = 0
  $nonBlack = 0
  for ($x = 0; $x -lt $Bitmap.Width; $x += $sampleStepX) {
    for ($y = 0; $y -lt $Bitmap.Height; $y += $sampleStepY) {
      $c = $Bitmap.GetPixel($x, $y)
      $brightness = ($c.R + $c.G + $c.B) / 3.0
      $sum += $brightness
      $sumSq += ($brightness * $brightness)
      if ($brightness -lt 245) { $nonWhite++ }
      if ($brightness -gt 10) { $nonBlack++ }
      $count++
    }
  }
  $mean = if ($count -gt 0) { $sum / $count } else { 0 }
  $variance = if ($count -gt 0) { ($sumSq / $count) - ($mean * $mean) } else { 0 }
  [pscustomobject]@{
    sampled_pixels = $count
    mean_brightness = [Math]::Round($mean, 2)
    brightness_variance = [Math]::Round($variance, 2)
    non_white_samples = $nonWhite
    non_black_samples = $nonBlack
  }
}

function Convert-BoundingRect {
  param($Rect)
  if ($null -eq $Rect) { return $null }
  [pscustomobject]@{
    left = [Math]::Round($Rect.Left, 0)
    top = [Math]::Round($Rect.Top, 0)
    right = [Math]::Round($Rect.Right, 0)
    bottom = [Math]::Round($Rect.Bottom, 0)
    width = [Math]::Round($Rect.Width, 0)
    height = [Math]::Round($Rect.Height, 0)
  }
}

$windows = Get-Process |
  Where-Object { $_.MainWindowHandle -ne 0 -and $_.MainWindowTitle } |
  Sort-Object ProcessName, Id |
  ForEach-Object {
    $handle = [IntPtr]$_.MainWindowHandle
    [pscustomobject]@{
      id = $_.Id
      process_name = $_.ProcessName
      title = $_.MainWindowTitle
      handle = $_.MainWindowHandle
      path = $_.Path
      visible = [XiigenCursorNative]::IsWindowVisible($handle)
      minimized = [XiigenCursorNative]::IsIconic($handle)
      rect = Get-WindowRectObject -Handle $handle
    }
  }

$windowsPath = Join-Path $OutputDir "window-inventory-detailed.json"
$windows | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $windowsPath -Encoding UTF8

$target = $windows |
  Where-Object {
    $_.process_name -eq "Cursor" -and
    $_.title -like "*[Extension Development Host]*" -and
    $_.title -like "*XIIGen Smart Skills*"
  } |
  Select-Object -First 1

$restored = $false
if ($null -ne $target -and ($target.minimized -or -not $target.visible)) {
  [void][XiigenCursorNative]::ShowWindow([IntPtr]$target.handle, 9)
  Start-Sleep -Milliseconds 750
  [void][XiigenCursorNative]::SetForegroundWindow([IntPtr]$target.handle)
  Start-Sleep -Milliseconds 750
  $restored = $true
  $windows = Get-Process |
    Where-Object { $_.MainWindowHandle -ne 0 -and $_.MainWindowTitle } |
    Sort-Object ProcessName, Id |
    ForEach-Object {
      $handle = [IntPtr]$_.MainWindowHandle
      [pscustomobject]@{
        id = $_.Id
        process_name = $_.ProcessName
        title = $_.MainWindowTitle
        handle = $_.MainWindowHandle
        path = $_.Path
        visible = [XiigenCursorNative]::IsWindowVisible($handle)
        minimized = [XiigenCursorNative]::IsIconic($handle)
        rect = Get-WindowRectObject -Handle $handle
      }
    }
  $windows | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $windowsPath -Encoding UTF8
  $target = $windows |
    Where-Object {
      $_.process_name -eq "Cursor" -and
      $_.title -like "*[Extension Development Host]*" -and
      $_.title -like "*XIIGen Smart Skills*"
    } |
    Select-Object -First 1
}

$secretPattern = "(?i)(secret|password|passwd|token|apikey|api[_-]?key|credential|private[_-]?key|bearer)"
$preflight = [ordered]@{
  schema = "xiigen.cursor_visible_route_preflight.v1"
  generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
  target_found = $null -ne $target
  restored_target_window = $restored
  target = $target
  unrelated_windows_present = @($windows | Where-Object { $_.process_name -ne "Cursor" } | Select-Object process_name, title)
  whole_desktop_screenshot_allowed = $false
  no_secret_title_pattern = if ($null -ne $target) { -not [bool]($target.title -match $secretPattern) } else { $false }
  row23_external_ai_submission = "not performed"
  can_capture_single_window = $false
  reason = ""
}

if ($null -eq $target) {
  $preflight.reason = "No Cursor Extension Development Host window titled XIIGen Smart Skills was found."
  $preflight | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $OutputDir "redaction-preflight.json") -Encoding UTF8
  Write-Output "BLOCKED: target Cursor Extension Development Host window not found."
  exit 20
}

if ($target.minimized -or -not $target.visible) {
  $preflight.reason = "Target Cursor window is minimized or not visible after restore attempt."
  $preflight | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $OutputDir "redaction-preflight.json") -Encoding UTF8
  Write-Output "BLOCKED: target Cursor window minimized or not visible."
  exit 21
}

if ($target.title -match $secretPattern) {
  $preflight.reason = "Target Cursor title matched secret-like wording; capture refused."
  $preflight | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $OutputDir "redaction-preflight.json") -Encoding UTF8
  Write-Output "BLOCKED: target Cursor title matched secret-like wording."
  exit 22
}

$preflight.can_capture_single_window = $true
$preflight.reason = "Target capture is restricted to one Cursor Extension Development Host HWND; whole desktop capture remains blocked."
$preflight | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $OutputDir "redaction-preflight.json") -Encoding UTF8

$bitmap = New-Object System.Drawing.Bitmap($target.rect.width, $target.rect.height)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$hdc = $graphics.GetHdc()
$printOk = $false
try {
  $printOk = [XiigenCursorNative]::PrintWindow([IntPtr]$target.handle, $hdc, 2)
}
finally {
  $graphics.ReleaseHdc($hdc)
  $graphics.Dispose()
}

$screenshotPath = Join-Path $OutputDir "cursor-window-before-route-decision.png"
$bitmap.Save($screenshotPath, [System.Drawing.Imaging.ImageFormat]::Png)
$stats = Get-ImageStats -Bitmap $bitmap
$bitmap.Dispose()

$capture = [ordered]@{
  schema = "xiigen.cursor_visible_route_screenshot.v1"
  generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
  screenshot_path = $screenshotPath
  target = $target
  print_window_returned = $printOk
  image_stats = $stats
  blank_or_failed = (-not $printOk) -or ($stats.non_black_samples -eq 0) -or ($stats.brightness_variance -lt 1)
  notes = @(
    "Single-window Cursor capture only.",
    "No Row23 prompt typed or submitted by this inspection script.",
    "This capture supports route decision only, not readiness."
  )
}
$capture | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $OutputDir "screenshot-capture-before-route-decision.json") -Encoding UTF8

$uiaRecords = New-Object System.Collections.Generic.List[object]
$uiaSummary = [ordered]@{
  schema = "xiigen.cursor_visible_route_uia_summary.v1"
  generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
  uia_root_found = $false
  local_panel_candidates = @()
  external_ai_candidates = @()
  editable_candidates = @()
  submit_candidates = @()
  focus_safe_local_route_found = $false
  reason = ""
}

try {
  $root = [System.Windows.Automation.AutomationElement]::FromHandle([IntPtr]$target.handle)
  if ($null -ne $root) {
    $uiaSummary.uia_root_found = $true
    $condition = [System.Windows.Automation.Condition]::TrueCondition
    $elements = $root.FindAll([System.Windows.Automation.TreeScope]::Descendants, $condition)
    $limit = [Math]::Min($elements.Count, 750)
    for ($i = 0; $i -lt $limit; $i++) {
      $el = $elements.Item($i)
      $name = $el.Current.Name
      $automationId = $el.Current.AutomationId
      $className = $el.Current.ClassName
      $controlType = $el.Current.ControlType.ProgrammaticName
      $rect = Convert-BoundingRect -Rect $el.Current.BoundingRectangle
      $record = [pscustomobject]@{
        index = $i
        name = $name
        automation_id = $automationId
        class_name = $className
        control_type = $controlType
        is_enabled = $el.Current.IsEnabled
        is_keyboard_focusable = $el.Current.IsKeyboardFocusable
        bounding_rect = $rect
      }
      $uiaRecords.Add($record) | Out-Null

      $joined = "$name $automationId $className $controlType"
      if ($joined -match "(?i)XIIGen Smart Skills|Prompt for XIIGen|Core API response|runtimeProof") {
        $uiaSummary.local_panel_candidates += $record
      }
      if ($joined -match "(?i)New Agent|GPT-|Agent|Plan, Build|for skills|external|chat") {
        $uiaSummary.external_ai_candidates += $record
      }
      if ($controlType -match "Edit|Document" -and $rect -ne $null -and $record.is_enabled) {
        $uiaSummary.editable_candidates += $record
      }
      if ($joined -match "(?i)Submit" -and $rect -ne $null -and $record.is_enabled) {
        $uiaSummary.submit_candidates += $record
      }
    }
  }
}
catch {
  $uiaSummary.reason = "UI Automation enumeration failed: $($_.Exception.Message)"
}

$uiaRecords | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $OutputDir "uia-tree-snapshot.json") -Encoding UTF8

$hasLocalPanel = @($uiaSummary.local_panel_candidates).Count -gt 0
$hasExternalAi = @($uiaSummary.external_ai_candidates).Count -gt 0
$hasEditable = @($uiaSummary.editable_candidates).Count -gt 0
$hasSubmit = @($uiaSummary.submit_candidates).Count -gt 0
if ($hasLocalPanel -and $hasEditable -and $hasSubmit -and -not $hasExternalAi) {
  $uiaSummary.focus_safe_local_route_found = $true
  $uiaSummary.reason = "UI Automation found local panel edit/submit controls and no external AI candidates."
}
elseif ($hasLocalPanel -and $hasEditable -and $hasSubmit -and $hasExternalAi) {
  $uiaSummary.reason = "UI Automation sees both local panel candidates and external AI/chat candidates in the same window; automation is not focus-safe without a control-specific route."
}
elseif (-not $hasLocalPanel) {
  $uiaSummary.reason = "UI Automation did not expose enough XIIGen Smart Skills panel text/control identity to prove a local-only route."
}
else {
  $uiaSummary.reason = "UI Automation did not expose a complete enabled local edit plus submit route."
}

$uiaSummary | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $OutputDir "uia-route-summary.json") -Encoding UTF8

if ($capture.blank_or_failed) {
  Write-Output "PARTIAL: screenshot file written but PrintWindow/image stats indicate blank or failed capture."
  exit 24
}

if ($uiaSummary.focus_safe_local_route_found) {
  Write-Output "PASS: focus-safe UI Automation local route appears available."
  exit 0
}

Write-Output "BLOCKED: focus-safe local UI route not proven by UI Automation. $($uiaSummary.reason)"
exit 30
