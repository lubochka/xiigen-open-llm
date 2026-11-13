param(
  [string]$OutputDir = "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\cursor-ext-path-b-ui-screenshot-01",
  [switch]$AllowRestore
)

$ErrorActionPreference = "Stop"
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

Add-Type -AssemblyName System.Drawing
Add-Type @"
using System;
using System.Runtime.InteropServices;

public static class NativeWindowCapture {
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
  $rect = New-Object NativeWindowCapture+RECT
  $ok = [NativeWindowCapture]::GetWindowRect($Handle, [ref]$rect)
  if (-not $ok) {
    return $null
  }

  return [pscustomobject]@{
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

  return [pscustomobject]@{
    sampled_pixels = $count
    mean_brightness = [Math]::Round($mean, 2)
    brightness_variance = [Math]::Round($variance, 2)
    non_white_samples = $nonWhite
    non_black_samples = $nonBlack
  }
}

$allWindows = Get-Process |
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
      visible = [NativeWindowCapture]::IsWindowVisible($handle)
      minimized = [NativeWindowCapture]::IsIconic($handle)
      rect = Get-WindowRectObject -Handle $handle
    }
  }

$inventoryPath = Join-Path $OutputDir "window-inventory-detailed.json"
$allWindows | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $inventoryPath -Encoding UTF8

$target = $allWindows |
  Where-Object {
    $_.process_name -eq "Cursor" -and
    $_.title -like "*[Extension Development Host]*" -and
    $_.title -like "*XIIGen Smart Skills*"
  } |
  Select-Object -First 1

$restoredTargetWindow = $false

if ($null -ne $target -and ($target.minimized -or -not $target.visible) -and $AllowRestore) {
  [void][NativeWindowCapture]::ShowWindow([IntPtr]$target.handle, 9)
  Start-Sleep -Milliseconds 750
  [void][NativeWindowCapture]::SetForegroundWindow([IntPtr]$target.handle)
  Start-Sleep -Milliseconds 750
  $restoredTargetWindow = $true

  $allWindows = Get-Process |
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
        visible = [NativeWindowCapture]::IsWindowVisible($handle)
        minimized = [NativeWindowCapture]::IsIconic($handle)
        rect = Get-WindowRectObject -Handle $handle
      }
    }

  $allWindows | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $inventoryPath -Encoding UTF8
  $target = $allWindows |
    Where-Object {
      $_.process_name -eq "Cursor" -and
      $_.title -like "*[Extension Development Host]*" -and
      $_.title -like "*XIIGen Smart Skills*"
    } |
    Select-Object -First 1
}

$secretPattern = "(?i)(secret|password|passwd|token|apikey|api[_-]?key|credential|private[_-]?key|bearer)"
$preflight = [ordered]@{
  schema = "xiigen.cursor_window_screenshot_preflight.v1"
  generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
  capture_scope = "single Cursor window by HWND using PrintWindow; no whole-desktop screenshot"
  row23_external_ai_submission = "not performed"
  unrelated_windows_present = @($allWindows | Where-Object { $_.process_name -ne "Cursor" } | Select-Object process_name, title)
  whole_desktop_screenshot_allowed = $false
  allow_restore = [bool]$AllowRestore
  restored_target_window = $restoredTargetWindow
  target_found = $null -ne $target
  target = $target
  title_secret_pattern_detected = if ($null -ne $target) { [bool]($target.title -match $secretPattern) } else { $false }
  safe_to_capture_target_window = $false
  reason = ""
}

if ($null -eq $target) {
  $preflight.reason = "No visible Cursor Extension Development Host window for XIIGen Smart Skills was found."
  $preflight | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $OutputDir "redaction-preflight.json") -Encoding UTF8
  Write-Output "BLOCKED: target Cursor Extension Development Host window not found."
  exit 20
}

if ($target.minimized -or -not $target.visible) {
  $preflight.reason = "Target Cursor window is minimized or not visible."
  $preflight | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $OutputDir "redaction-preflight.json") -Encoding UTF8
  Write-Output "BLOCKED: target Cursor window minimized or not visible."
  exit 21
}

if ($target.title -match $secretPattern) {
  $preflight.reason = "Target Cursor title matched secret-like wording; screenshot refused."
  $preflight | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $OutputDir "redaction-preflight.json") -Encoding UTF8
  Write-Output "BLOCKED: target Cursor title matched secret-like wording."
  exit 22
}

if ($null -eq $target.rect -or $target.rect.width -le 0 -or $target.rect.height -le 0) {
  $preflight.reason = "Target Cursor window rectangle is unavailable or invalid."
  $preflight | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $OutputDir "redaction-preflight.json") -Encoding UTF8
  Write-Output "BLOCKED: invalid target Cursor window rectangle."
  exit 23
}

$preflight.safe_to_capture_target_window = $true
$preflight.reason = "Target capture is restricted to the Cursor Extension Development Host HWND. Unrelated windows are present, so whole-desktop capture remains blocked."
$preflight | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $OutputDir "redaction-preflight.json") -Encoding UTF8

$bitmap = New-Object System.Drawing.Bitmap($target.rect.width, $target.rect.height)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$hdc = $graphics.GetHdc()
$printOk = $false
try {
  $printOk = [NativeWindowCapture]::PrintWindow([IntPtr]$target.handle, $hdc, 2)
}
finally {
  $graphics.ReleaseHdc($hdc)
  $graphics.Dispose()
}

$screenshotPath = Join-Path $OutputDir "cursor-window-screenshot.png"
$bitmap.Save($screenshotPath, [System.Drawing.Imaging.ImageFormat]::Png)
$stats = Get-ImageStats -Bitmap $bitmap
$bitmap.Dispose()

$capture = [ordered]@{
  schema = "xiigen.cursor_window_screenshot_capture.v1"
  generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
  print_window_returned = $printOk
  screenshot_path = $screenshotPath
  target = $target
  image_stats = $stats
  blank_or_failed = (-not $printOk) -or ($stats.non_black_samples -eq 0) -or ($stats.brightness_variance -lt 1)
  notes = @(
    "Screenshot is single-window capture of Cursor Extension Development Host only.",
    "Target window was restored before capture: $restoredTargetWindow.",
    "No Row23 prompt was submitted to external AI/chat in this runner.",
    "This screenshot alone proves only a visible Cursor Extension Development Host window, not full Cursor readiness."
  )
}

$capture | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $OutputDir "screenshot-capture.json") -Encoding UTF8

if ($capture.blank_or_failed) {
  Write-Output "PARTIAL: screenshot file written but PrintWindow/image stats indicate blank or failed capture."
  exit 24
}

Write-Output "PASS: Cursor Extension Development Host window screenshot captured."
exit 0
