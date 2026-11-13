$ErrorActionPreference = 'Continue'

$runTs = '20260624T041937Z'
$root = "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-14-fixes\real-program-ui-proof\p14-fix-13-real-desktop-screen-control-$runTs"

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class P14InputUser32 {
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
  [DllImport("user32.dll")] public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);
  [DllImport("user32.dll")] public static extern bool SetCursorPos(int X, int Y);
  [DllImport("user32.dll")] public static extern void mouse_event(uint dwFlags, uint dx, uint dy, int dwData, UIntPtr dwExtraInfo);
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
  $rect = New-Object P14InputUser32+RECT
  [P14InputUser32]::GetWindowRect([IntPtr]$p.MainWindowHandle, [ref]$rect) | Out-Null
  return $rect
}

function Activate([System.Diagnostics.Process]$p) {
  [P14InputUser32]::ShowWindowAsync([IntPtr]$p.MainWindowHandle, 9) | Out-Null
  Start-Sleep -Milliseconds 400
  [P14InputUser32]::SetForegroundWindow([IntPtr]$p.MainWindowHandle) | Out-Null
  Start-Sleep -Milliseconds 1000
}

function Click-At([int]$x, [int]$y) {
  [P14InputUser32]::SetCursorPos($x, $y) | Out-Null
  Start-Sleep -Milliseconds 150
  [P14InputUser32]::mouse_event(0x0002, 0, 0, 0, [UIntPtr]::Zero)
  Start-Sleep -Milliseconds 80
  [P14InputUser32]::mouse_event(0x0004, 0, 0, 0, [UIntPtr]::Zero)
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

$attempts = [ordered]@{ run_timestamp_utc = $runTs; attempts = @{} }

try {
  $claude = Get-Window 'claude'
  if ($null -eq $claude) { throw 'Claude window not found' }
  Activate $claude
  $r = Get-Rect $claude
  $x = [int](($r.Left + $r.Right) / 2)
  $y = [int]($r.Bottom - 64)
  Click-At $x $y
  Start-Sleep -Milliseconds 500
  [System.Windows.Forms.SendKeys]::SendWait('P14-REAL-CLAUDE-20260624T041937Z')
  Start-Sleep -Seconds 1
  $path = "$root\raw\claude\window-claude-real-marker-direct-keys.png"
  $rect = Capture $claude $path
  $attempts.attempts.claude_direct_keys = [ordered]@{ ok = $true; click = @{ x = $x; y = $y }; screenshot_path = $path; screenshot_rect = $rect }
} catch {
  $attempts.attempts.claude_direct_keys = [ordered]@{ ok = $false; error = $_.Exception.Message }
}

try {
  $codex = Get-Window 'Codex'
  if ($null -eq $codex) { throw 'Codex window not found' }
  Activate $codex
  $r = Get-Rect $codex
  $contentX = [int]($r.Left + (($r.Right - $r.Left) * 0.72))
  $contentY = [int]($r.Top + (($r.Bottom - $r.Top) * 0.55))
  Click-At $contentX $contentY
  for ($i = 0; $i -lt 18; $i++) {
    [P14InputUser32]::mouse_event(0x0800, 0, 0, -480, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds 80
  }
  [System.Windows.Forms.SendKeys]::SendWait('{PGDN}{PGDN}{PGDN}^{END}')
  Start-Sleep -Seconds 1
  $path = "$root\raw\codex\window-codex-real-marker-scroll-bottom.png"
  $rect = Capture $codex $path
  $attempts.attempts.codex_scroll_bottom = [ordered]@{ ok = $true; click = @{ x = $contentX; y = $contentY }; screenshot_path = $path; screenshot_rect = $rect }
} catch {
  $attempts.attempts.codex_scroll_bottom = [ordered]@{ ok = $false; error = $_.Exception.Message }
}

$attempts | ConvertTo-Json -Depth 8 | Set-Content -Path "$root\logs\recapture-keyboard-scroll.json" -Encoding UTF8
Write-Host "KEYBOARD_SCROLL_ROOT=$root"
