param([Parameter(Mandatory=$true)][string]$EvidenceRoot)
$ErrorActionPreference = "Stop"
$Screens = Join-Path $EvidenceRoot "screenshots"
$LogPath = Join-Path $EvidenceRoot "action-log.md"
Add-Type -AssemblyName System.Drawing
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class UiMenuNative {
  [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
  [DllImport("user32.dll")] public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
  [DllImport("user32.dll")] public static extern bool SetCursorPos(int X, int Y);
  [DllImport("user32.dll")] public static extern void mouse_event(uint flags, uint dx, uint dy, uint data, UIntPtr extraInfo);
  public const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
  public const uint MOUSEEVENTF_LEFTUP = 0x0004;
}
"@
function Add-Log([string]$Text) { $line = "- $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') - $Text"; Add-Content -Path $LogPath -Value $line -Encoding UTF8; Write-Output $line }
function ClickAt([int]$x,[int]$y){ [UiMenuNative]::SetCursorPos($x,$y)|Out-Null; Start-Sleep -Milliseconds 100; [UiMenuNative]::mouse_event([UiMenuNative]::MOUSEEVENTF_LEFTDOWN,0,0,0,[UIntPtr]::Zero); Start-Sleep -Milliseconds 80; [UiMenuNative]::mouse_event([UiMenuNative]::MOUSEEVENTF_LEFTUP,0,0,0,[UIntPtr]::Zero) }
function Capture-Window([System.IntPtr]$Hwnd,[string]$Name){ $r=New-Object UiMenuNative+RECT; [UiMenuNative]::GetWindowRect($Hwnd,[ref]$r)|Out-Null; $w=[Math]::Max(1,$r.Right-$r.Left); $h=[Math]::Max(1,$r.Bottom-$r.Top); $bmp=New-Object System.Drawing.Bitmap $w,$h; $g=[System.Drawing.Graphics]::FromImage($bmp); try { $g.CopyFromScreen($r.Left,$r.Top,0,0,(New-Object System.Drawing.Size $w,$h)); $p=Join-Path $Screens $Name; $bmp.Save($p,[System.Drawing.Imaging.ImageFormat]::Png); Add-Log "Screenshot saved: screenshots/$Name" } finally { $g.Dispose(); $bmp.Dispose() } }
$target = Get-Process Cursor | Where-Object { $_.MainWindowTitle -like "*[Extension Development Host]*" -and $_.MainWindowTitle -like "*cursor-xiigen-smart-skills*" } | Sort-Object Id | Select-Object -First 1
if (-not $target) { throw "Target Cursor Extension Development Host not found" }
$r=New-Object UiMenuNative+RECT
[UiMenuNative]::GetWindowRect($target.MainWindowHandle,[ref]$r)|Out-Null
[UiMenuNative]::ShowWindowAsync($target.MainWindowHandle,9)|Out-Null
[UiMenuNative]::SetForegroundWindow($target.MainWindowHandle)|Out-Null
Start-Sleep -Milliseconds 700
Add-Log "Mouse-menu attempt: clicking Cursor View menu in the Extension Development Host."
ClickAt ($r.Left + 196) ($r.Top + 18)
Start-Sleep -Milliseconds 700
Capture-Window $target.MainWindowHandle "01e-view-menu-after-click.png"
Add-Log "Mouse-menu attempt captured; no further click issued unless visual review confirms the menu."
