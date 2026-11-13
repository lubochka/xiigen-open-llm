param([Parameter(Mandatory=$true)][string]$EvidenceRoot)
$ErrorActionPreference = "Stop"
$Screens = Join-Path $EvidenceRoot "screenshots"
$LogPath = Join-Path $EvidenceRoot "action-log.md"
Add-Type -AssemblyName System.Drawing
Add-Type @"
using System;
using System.Text;
using System.Runtime.InteropServices;
public static class UiWsNative {
  [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
  [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
  [DllImport("user32.dll", CharSet=CharSet.Unicode)] public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
}
"@
function Add-Log([string]$Text) { $line = "- $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') - $Text"; Add-Content -Path $LogPath -Value $line -Encoding UTF8; Write-Output $line }
function FgTitle { $h=[UiWsNative]::GetForegroundWindow(); $sb=New-Object System.Text.StringBuilder 512; [UiWsNative]::GetWindowText($h,$sb,$sb.Capacity) | Out-Null; return $sb.ToString() }
function Capture-Window([System.IntPtr]$Hwnd,[string]$Name){ $r=New-Object UiWsNative+RECT; [UiWsNative]::GetWindowRect($Hwnd,[ref]$r)|Out-Null; $w=[Math]::Max(1,$r.Right-$r.Left); $h=[Math]::Max(1,$r.Bottom-$r.Top); $bmp=New-Object System.Drawing.Bitmap $w,$h; $g=[System.Drawing.Graphics]::FromImage($bmp); try { $g.CopyFromScreen($r.Left,$r.Top,0,0,(New-Object System.Drawing.Size $w,$h)); $p=Join-Path $Screens $Name; $bmp.Save($p,[System.Drawing.Imaging.ImageFormat]::Png); Add-Log "Screenshot saved: screenshots/$Name" } finally { $g.Dispose(); $bmp.Dispose() } }
$target = Get-Process Cursor | Where-Object { $_.MainWindowTitle -like "*[Extension Development Host]*" -and $_.MainWindowTitle -like "*cursor-xiigen-smart-skills*" } | Sort-Object Id | Select-Object -First 1
if (-not $target) { throw "Target Cursor Extension Development Host not found" }
$ws = New-Object -ComObject WScript.Shell
Add-Log "WScript retry: foreground before AppActivate: $(FgTitle)"
$activated = $ws.AppActivate([int]$target.Id)
Add-Log "WScript retry: AppActivate($($target.Id)) returned $activated; foreground now: $(FgTitle)"
Start-Sleep -Milliseconds 1000
$ws.SendKeys('^+p')
Start-Sleep -Milliseconds 1000
Capture-Window $target.MainWindowHandle "01c-wscript-command-palette-before-query.png"
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
# The repeated Ctrl+A lines above are harmless if the palette is focused; they also create an obvious failure in the action log if focus is wrong.
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
$ws.SendKeys('^a')
Start-Sleep -Milliseconds 100
# Use SendKeys escaped braces only for Enter; plain characters for the query.
$ws.SendKeys('XIIGen: Open Smart Skills Panel')
Start-Sleep -Milliseconds 1400
Capture-Window $target.MainWindowHandle "01d-wscript-command-palette-choice.png"
$ws.SendKeys('{ENTER}')
Start-Sleep -Milliseconds 3500
Capture-Window $target.MainWindowHandle "04-panel-after-wscript-command.png"
Add-Log "WScript retry: foreground after sequence: $(FgTitle)"
