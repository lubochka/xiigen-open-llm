param(
  [Parameter(Mandatory=$true)][string]$EvidenceRoot
)
$ErrorActionPreference = "Stop"
$Screens = Join-Path $EvidenceRoot "screenshots"
$LogPath = Join-Path $EvidenceRoot "action-log.md"
Add-Type -AssemblyName System.Drawing
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class UiRetryNative {
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
function Add-Log([string]$Text) {
  $stamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
  $line = "- $stamp - $Text"
  Add-Content -Path $LogPath -Value $line -Encoding UTF8
  Write-Output $line
}
function Capture-Window([System.IntPtr]$Hwnd, [string]$Name) {
  $rect = New-Object UiRetryNative+RECT
  if (-not [UiRetryNative]::GetWindowRect($Hwnd, [ref]$rect)) { throw "GetWindowRect failed" }
  $width = [Math]::Max(1, $rect.Right - $rect.Left)
  $height = [Math]::Max(1, $rect.Bottom - $rect.Top)
  $bmp = New-Object System.Drawing.Bitmap $width, $height
  $gfx = [System.Drawing.Graphics]::FromImage($bmp)
  try {
    $gfx.CopyFromScreen($rect.Left, $rect.Top, 0, 0, (New-Object System.Drawing.Size $width, $height))
    $path = Join-Path $Screens $Name
    $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    Add-Log "Screenshot saved: screenshots/$Name"
  } finally { $gfx.Dispose(); $bmp.Dispose() }
}
$target = Get-Process Cursor | Where-Object { $_.MainWindowTitle -like "*[Extension Development Host]*" -and $_.MainWindowTitle -like "*cursor-xiigen-smart-skills*" } | Sort-Object Id | Select-Object -First 1
if (-not $target) { throw "Target Cursor Extension Development Host not found" }
$rect = New-Object UiRetryNative+RECT
[UiRetryNative]::GetWindowRect($target.MainWindowHandle, [ref]$rect) | Out-Null
[UiRetryNative]::ShowWindowAsync($target.MainWindowHandle, 9) | Out-Null
Start-Sleep -Milliseconds 400
[UiRetryNative]::SetForegroundWindow($target.MainWindowHandle) | Out-Null
Start-Sleep -Milliseconds 400
[UiRetryNative]::SetCursorPos($rect.Left + 520, $rect.Top + 18) | Out-Null
[UiRetryNative]::mouse_event([UiRetryNative]::MOUSEEVENTF_LEFTDOWN,0,0,0,[UIntPtr]::Zero)
Start-Sleep -Milliseconds 80
[UiRetryNative]::mouse_event([UiRetryNative]::MOUSEEVENTF_LEFTUP,0,0,0,[UIntPtr]::Zero)
Start-Sleep -Milliseconds 500
Add-Log "Retry: opening command palette with F1 in target host."
[UiRetryNative]::KeyPress(0x70)
Start-Sleep -Milliseconds 900
Capture-Window $target.MainWindowHandle "01a-command-palette-open-before-filter.png"
Add-Log "Retry: typing command palette query >XIIGen: Open Smart Skills Panel."
[UiRetryNative]::UnicodeText(">XIIGen: Open Smart Skills Panel")
Start-Sleep -Milliseconds 1400
Capture-Window $target.MainWindowHandle "01b-command-palette-open-smart-skills-panel-choice.png"
Add-Log "Retry: pressing Enter to run the filtered panel command."
[UiRetryNative]::KeyPress(0x0D)
Start-Sleep -Milliseconds 3500
Capture-Window $target.MainWindowHandle "03-panel-after-command-palette-retry.png"
Write-Output "EVIDENCE_ROOT=$EvidenceRoot"
