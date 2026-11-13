$ErrorActionPreference = 'Continue'

$runTs = '20260624T041937Z'
$root = "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-14-fixes\real-program-ui-proof\p14-fix-13-real-desktop-screen-control-$runTs"

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class P14CodexDirectUser32 {
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
  [DllImport("user32.dll")] public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);
  [DllImport("user32.dll")] public static extern bool SetCursorPos(int X, int Y);
  [DllImport("user32.dll")] public static extern void mouse_event(uint dwFlags, uint dx, uint dy, int dwData, UIntPtr dwExtraInfo);
  [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
}
"@

function Get-CodexWindow {
  Get-Process |
    Where-Object { $_.MainWindowHandle -ne 0 -and ($_.ProcessName -match 'Codex' -or $_.MainWindowTitle -match 'Codex') } |
    Sort-Object StartTime -Descending |
    Select-Object -First 1
}

function Get-Rect([System.Diagnostics.Process]$p) {
  $rect = New-Object P14CodexDirectUser32+RECT
  [P14CodexDirectUser32]::GetWindowRect([IntPtr]$p.MainWindowHandle, [ref]$rect) | Out-Null
  return $rect
}

function Click-At([int]$x, [int]$y) {
  [P14CodexDirectUser32]::SetCursorPos($x, $y) | Out-Null
  Start-Sleep -Milliseconds 150
  [P14CodexDirectUser32]::mouse_event(0x0002, 0, 0, 0, [UIntPtr]::Zero)
  Start-Sleep -Milliseconds 80
  [P14CodexDirectUser32]::mouse_event(0x0004, 0, 0, 0, [UIntPtr]::Zero)
  Start-Sleep -Milliseconds 250
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

$result = [ordered]@{ run_timestamp_utc = $runTs }
try {
  $codex = Get-CodexWindow
  if ($null -eq $codex) { throw 'Codex window not found' }
  [P14CodexDirectUser32]::ShowWindowAsync([IntPtr]$codex.MainWindowHandle, 9) | Out-Null
  Start-Sleep -Milliseconds 400
  [P14CodexDirectUser32]::SetForegroundWindow([IntPtr]$codex.MainWindowHandle) | Out-Null
  Start-Sleep -Milliseconds 1000
  $r = Get-Rect $codex
  $clicks = @(
    @{ x = [int]($r.Left + 735); y = [int]($r.Top + 291) },
    @{ x = [int]($r.Left + 880); y = [int]($r.Top + 291) },
    @{ x = [int]($r.Left + 700); y = [int]($r.Top + 291) }
  )
  foreach ($c in $clicks) { Click-At $c.x $c.y }
  [System.Windows.Forms.SendKeys]::SendWait('P14-REAL-CODEX-20260624T041937Z')
  Start-Sleep -Seconds 1
  $path = "$root\raw\codex\window-codex-real-marker-plugin-search-direct-keys.png"
  $rect = Capture $codex $path
  $result.ok = $true
  $result.clicks = $clicks
  $result.screenshot_path = $path
  $result.screenshot_rect = $rect
} catch {
  $result.ok = $false
  $result.error = $_.Exception.Message
}

$result | ConvertTo-Json -Depth 8 | Set-Content -Path "$root\logs\recapture-codex-plugin-search-direct-keys.json" -Encoding UTF8
Write-Host "CODEX_PLUGIN_DIRECT_ROOT=$root"
