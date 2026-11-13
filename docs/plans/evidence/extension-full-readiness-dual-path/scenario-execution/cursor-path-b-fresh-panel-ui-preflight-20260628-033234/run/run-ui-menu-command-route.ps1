param([Parameter(Mandatory=$true)][string]$EvidenceRoot)
$ErrorActionPreference = "Stop"
$Screens = Join-Path $EvidenceRoot "screenshots"
$LogPath = Join-Path $EvidenceRoot "action-log.md"
Add-Type -AssemblyName System.Drawing
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class UiRouteNative {
  [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
  [DllImport("user32.dll")] public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
  [DllImport("user32.dll")] public static extern bool SetCursorPos(int X, int Y);
  [DllImport("user32.dll")] public static extern void mouse_event(uint flags, uint dx, uint dy, uint data, UIntPtr extraInfo);
  [DllImport("user32.dll", SetLastError=true)] public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
  [StructLayout(LayoutKind.Sequential)] public struct INPUT { public uint type; public InputUnion U; }
  [StructLayout(LayoutKind.Explicit)] public struct InputUnion { [FieldOffset(0)] public KEYBDINPUT ki; }
  [StructLayout(LayoutKind.Sequential)] public struct KEYBDINPUT { public ushort wVk; public ushort wScan; public uint dwFlags; public uint time; public IntPtr dwExtraInfo; }
  public const uint INPUT_KEYBOARD = 1;
  public const uint KEYEVENTF_KEYUP = 0x0002;
  public const uint KEYEVENTF_UNICODE = 0x0004;
  public const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
  public const uint MOUSEEVENTF_LEFTUP = 0x0004;
  public static void KeyDown(ushort vk) { INPUT[] i = new INPUT[1]; i[0].type = INPUT_KEYBOARD; i[0].U.ki.wVk = vk; SendInput(1, i, Marshal.SizeOf(typeof(INPUT))); }
  public static void KeyUp(ushort vk) { INPUT[] i = new INPUT[1]; i[0].type = INPUT_KEYBOARD; i[0].U.ki.wVk = vk; i[0].U.ki.dwFlags = KEYEVENTF_KEYUP; SendInput(1, i, Marshal.SizeOf(typeof(INPUT))); }
  public static void KeyPress(ushort vk) { KeyDown(vk); KeyUp(vk); }
  public static void UnicodeText(string text) { foreach (char c in text) { INPUT[] d = new INPUT[1]; d[0].type = INPUT_KEYBOARD; d[0].U.ki.wScan = c; d[0].U.ki.dwFlags = KEYEVENTF_UNICODE; SendInput(1, d, Marshal.SizeOf(typeof(INPUT))); INPUT[] u = new INPUT[1]; u[0].type = INPUT_KEYBOARD; u[0].U.ki.wScan = c; u[0].U.ki.dwFlags = KEYEVENTF_UNICODE | KEYEVENTF_KEYUP; SendInput(1, u, Marshal.SizeOf(typeof(INPUT))); } }
}
"@
function Add-Log([string]$Text) { $line = "- $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') - $Text"; Add-Content -Path $LogPath -Value $line -Encoding UTF8; Write-Output $line }
function ClickAt([int]$x,[int]$y){ [UiRouteNative]::SetCursorPos($x,$y)|Out-Null; Start-Sleep -Milliseconds 120; [UiRouteNative]::mouse_event([UiRouteNative]::MOUSEEVENTF_LEFTDOWN,0,0,0,[UIntPtr]::Zero); Start-Sleep -Milliseconds 80; [UiRouteNative]::mouse_event([UiRouteNative]::MOUSEEVENTF_LEFTUP,0,0,0,[UIntPtr]::Zero) }
function Capture-Window([System.IntPtr]$Hwnd,[string]$Name){ $r=New-Object UiRouteNative+RECT; [UiRouteNative]::GetWindowRect($Hwnd,[ref]$r)|Out-Null; $w=[Math]::Max(1,$r.Right-$r.Left); $h=[Math]::Max(1,$r.Bottom-$r.Top); $bmp=New-Object System.Drawing.Bitmap $w,$h; $g=[System.Drawing.Graphics]::FromImage($bmp); try { $g.CopyFromScreen($r.Left,$r.Top,0,0,(New-Object System.Drawing.Size $w,$h)); $p=Join-Path $Screens $Name; $bmp.Save($p,[System.Drawing.Imaging.ImageFormat]::Png); Add-Log "Screenshot saved: screenshots/$Name" } finally { $g.Dispose(); $bmp.Dispose() } }
$target = Get-Process Cursor | Where-Object { $_.MainWindowTitle -like "*[Extension Development Host]*" -and $_.MainWindowTitle -like "*cursor-xiigen-smart-skills*" } | Sort-Object Id | Select-Object -First 1
if (-not $target) { throw "Target Cursor Extension Development Host not found" }
$r=New-Object UiRouteNative+RECT
[UiRouteNative]::GetWindowRect($target.MainWindowHandle,[ref]$r)|Out-Null
[UiRouteNative]::ShowWindowAsync($target.MainWindowHandle,9)|Out-Null
[UiRouteNative]::SetForegroundWindow($target.MainWindowHandle)|Out-Null
Start-Sleep -Milliseconds 500
Add-Log "Menu route: clicking View menu, then Command Palette menu item."
ClickAt ($r.Left + 196) ($r.Top + 18)
Start-Sleep -Milliseconds 350
ClickAt ($r.Left + 268) ($r.Top + 47)
Start-Sleep -Milliseconds 1000
Capture-Window $target.MainWindowHandle "01f-command-palette-opened-from-view-menu.png"
Add-Log "Menu route: typing exact command label filter XIIGen: Open Smart Skills Panel."
[UiRouteNative]::UnicodeText("XIIGen: Open Smart Skills Panel")
Start-Sleep -Milliseconds 1200
Capture-Window $target.MainWindowHandle "01g-command-palette-exact-open-panel-choice.png"
Add-Log "Menu route: pressing Enter to choose the filtered command."
[UiRouteNative]::KeyPress(0x0D)
Start-Sleep -Milliseconds 3500
Capture-Window $target.MainWindowHandle "05-panel-after-menu-command-route.png"
