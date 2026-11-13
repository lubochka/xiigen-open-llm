$ErrorActionPreference = 'Continue'

$runTs = '20260624T041937Z'
$root = "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-14-fixes\real-program-ui-proof\p14-fix-13-real-desktop-screen-control-$runTs"

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class P14MouseUser32 {
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
  [DllImport("user32.dll")] public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);
  [DllImport("user32.dll")] public static extern bool SetCursorPos(int X, int Y);
  [DllImport("user32.dll")] public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);
  [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
}
"@

function Get-Window([string]$pattern) {
  Get-Process |
    Where-Object { $_.MainWindowHandle -ne 0 -and ($_.ProcessName -match $pattern -or $_.MainWindowTitle -match $pattern) } |
    Sort-Object StartTime -Descending |
    Select-Object -First 1
}

function Get-Rect([System.Diagnostics.Process]$p) {
  $rect = New-Object P14MouseUser32+RECT
  [P14MouseUser32]::GetWindowRect([IntPtr]$p.MainWindowHandle, [ref]$rect) | Out-Null
  return $rect
}

function Activate([System.Diagnostics.Process]$p) {
  [P14MouseUser32]::ShowWindowAsync([IntPtr]$p.MainWindowHandle, 9) | Out-Null
  Start-Sleep -Milliseconds 400
  [P14MouseUser32]::SetForegroundWindow([IntPtr]$p.MainWindowHandle) | Out-Null
  Start-Sleep -Milliseconds 1000
}

function Click-At([int]$x, [int]$y) {
  [P14MouseUser32]::SetCursorPos($x, $y) | Out-Null
  Start-Sleep -Milliseconds 150
  [P14MouseUser32]::mouse_event(0x0002, 0, 0, 0, [UIntPtr]::Zero)
  Start-Sleep -Milliseconds 80
  [P14MouseUser32]::mouse_event(0x0004, 0, 0, 0, [UIntPtr]::Zero)
  Start-Sleep -Milliseconds 300
}

function Capture([System.Diagnostics.Process]$p, [string]$path) {
  $rect = Get-Rect $p
  $width = $rect.Right - $rect.Left
  $height = $rect.Bottom - $rect.Top
  $bmp = New-Object System.Drawing.Bitmap($width, $height)
  $g = [System.Drawing.Graphics]::FromImage($bmp)
  try {
    $g.CopyFromScreen($rect.Left, $rect.Top, 0, 0, $bmp.Size)
    $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
  } finally {
    $g.Dispose()
    $bmp.Dispose()
  }
  return @{ left = $rect.Left; top = $rect.Top; right = $rect.Right; bottom = $rect.Bottom; width = $width; height = $height }
}

$recapture = [ordered]@{ run_timestamp_utc = $runTs; attempts = @{} }

try {
  $claude = Get-Window 'claude'
  if ($null -eq $claude) { throw 'Claude window not found' }
  Activate $claude
  $r = Get-Rect $claude
  $x = [int](($r.Left + $r.Right) / 2)
  $y = [int]($r.Bottom - 65)
  Click-At $x $y
  [System.Windows.Forms.Clipboard]::SetText("P14-REAL-CLAUDE-$runTs`r`nPhase 14.13 visible-app marker only; not submitted.")
  [System.Windows.Forms.SendKeys]::SendWait('^v')
  Start-Sleep -Seconds 1
  $path = "$root\raw\claude\window-claude-real-marker-recapture.png"
  $rect = Capture $claude $path
  $recapture.attempts.claude = [ordered]@{ ok = $true; click = @{ x = $x; y = $y }; screenshot_path = $path; screenshot_rect = $rect }
} catch {
  $recapture.attempts.claude = [ordered]@{ ok = $false; error = $_.Exception.Message }
}

try {
  $codex = Get-Window 'Codex'
  if ($null -eq $codex) { throw 'Codex window not found' }
  Activate $codex
  $r = Get-Rect $codex
  Click-At ([int](($r.Left + $r.Right) / 2)) ([int](($r.Top + $r.Bottom) / 2))
  [System.Windows.Forms.SendKeys]::SendWait('^{END}')
  Start-Sleep -Seconds 1
  $inputX = [int](($r.Left + $r.Right) / 2)
  $inputY = [int]($r.Bottom - 55)
  Click-At $inputX $inputY
  [System.Windows.Forms.Clipboard]::SetText("P14-REAL-CODEX-$runTs")
  [System.Windows.Forms.SendKeys]::SendWait('^v')
  Start-Sleep -Seconds 1
  $path = "$root\raw\codex\window-codex-real-marker-recapture.png"
  $rect = Capture $codex $path
  $recapture.attempts.codex = [ordered]@{ ok = $true; input_click = @{ x = $inputX; y = $inputY }; screenshot_path = $path; screenshot_rect = $rect }
} catch {
  $recapture.attempts.codex = [ordered]@{ ok = $false; error = $_.Exception.Message }
}

$recapture | ConvertTo-Json -Depth 8 | Set-Content -Path "$root\logs\recapture-claude-codex.json" -Encoding UTF8
Write-Host "RECATURE_ROOT=$root"
