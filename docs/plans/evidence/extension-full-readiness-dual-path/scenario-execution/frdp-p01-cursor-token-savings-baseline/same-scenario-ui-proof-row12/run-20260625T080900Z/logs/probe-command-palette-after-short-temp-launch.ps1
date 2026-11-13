Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$runFolder = 'C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z'
$logsDir = Join-Path $runFolder 'logs'
$screenshotsDir = Join-Path $runFolder 'screenshots'
New-Item -ItemType Directory -Force -Path $logsDir, $screenshotsDir | Out-Null

$launchRecordPath = Join-Path $logsDir 'corrected-launch-record.json'
$launchRecord = Get-Content -LiteralPath $launchRecordPath -Raw | ConvertFrom-Json
$targetUserDataDir = [string]$launchRecord.user_data_dir
$targetPid = [int]$launchRecord.matching_processes_with_full_path[0].ProcessId
$searchText = 'XIIGen: Open Smart Skills Panel'

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

$signature = @'
using System;
using System.Runtime.InteropServices;
using System.Text;
public static class Win32 {
  public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
  [DllImport("user32.dll")] public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
  [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr hWnd);
  [DllImport("user32.dll", SetLastError=true)] public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
  [DllImport("user32.dll", SetLastError=true)] public static extern int GetWindowTextLength(IntPtr hWnd);
  [DllImport("user32.dll", SetLastError=true)] public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
  [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
  [DllImport("user32.dll")] public static extern bool SetCursorPos(int X, int Y);
  [DllImport("user32.dll")] public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);
  [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
}
'@
Add-Type $signature

$windows = New-Object System.Collections.Generic.List[object]
$callback = [Win32+EnumWindowsProc]{
  param([IntPtr]$hWnd, [IntPtr]$lParam)
  if (-not [Win32]::IsWindowVisible($hWnd)) { return $true }
  $len = [Win32]::GetWindowTextLength($hWnd)
  if ($len -le 0) { return $true }
  $sb = New-Object System.Text.StringBuilder ($len + 1)
  [void][Win32]::GetWindowText($hWnd, $sb, $sb.Capacity)
  [uint32]$windowPid = 0
  [void][Win32]::GetWindowThreadProcessId($hWnd, [ref]$windowPid)
  $title = $sb.ToString()
  if ($title -like '*Extension Development Host*Cursor*') {
    $rect = New-Object Win32+RECT
    [void][Win32]::GetWindowRect($hWnd, [ref]$rect)
    $windows.Add([pscustomobject]@{
      Handle = $hWnd
      HandleInt = $hWnd.ToInt64()
      ProcessId = [int]$windowPid
      Title = $title
      Rect = $rect
    })
  }
  return $true
}
[void][Win32]::EnumWindows($callback, [IntPtr]::Zero)

$target = $windows | Where-Object { $_.ProcessId -eq $targetPid } | Select-Object -First 1
if (-not $target) {
  $target = $windows | Sort-Object ProcessId -Descending | Select-Object -First 1
}
if (-not $target) { throw 'No visible Extension Development Host Cursor window found.' }

[void][Win32]::ShowWindow($target.Handle, 9)
Start-Sleep -Milliseconds 300
[void][Win32]::SetForegroundWindow($target.Handle)
Start-Sleep -Milliseconds 500
$clickX = [int](($target.Rect.Left + $target.Rect.Right) / 2)
$clickY = [int]($target.Rect.Top + 24)
[void][Win32]::SetCursorPos($clickX, $clickY)
[Win32]::mouse_event(0x0002, 0, 0, 0, [UIntPtr]::Zero)
[Win32]::mouse_event(0x0004, 0, 0, 0, [UIntPtr]::Zero)
Start-Sleep -Milliseconds 500

[System.Windows.Forms.SendKeys]::SendWait('{F1}')
Start-Sleep -Milliseconds 1000
[System.Windows.Forms.SendKeys]::SendWait('^a')
Start-Sleep -Milliseconds 200
Set-Clipboard -Value $searchText
[System.Windows.Forms.SendKeys]::SendWait('^v')
Start-Sleep -Seconds 2

$root = [System.Windows.Automation.AutomationElement]::FromHandle($target.Handle)
$condition = [System.Windows.Automation.Condition]::TrueCondition
$all = @($root.FindAll([System.Windows.Automation.TreeScope]::Subtree, $condition))
$names = foreach ($el in $all) {
  try {
    $name = $el.Current.Name
    if ($name) { $name }
  } catch {}
}
$matchingNames = @($names | Where-Object { $_ -like "*$searchText*" } | Select-Object -Unique)
$xiigenNames = @($names | Where-Object { $_ -like '*XIIGen*' -or $_ -like '*Smart Skills*' } | Select-Object -Unique)
$noMatchSignals = @($names | Where-Object { $_ -like '*No matching*' -or $_ -like '*No results*' -or $_ -like '*not found*' } | Select-Object -Unique)

$rectWidth = [Math]::Max(1, $target.Rect.Right - $target.Rect.Left)
$rectHeight = [Math]::Max(1, $target.Rect.Bottom - $target.Rect.Top)
$screenshotPath = Join-Path $screenshotsDir 'command-palette-xiigen-open-smart-skills-panel-after-short-temp-launch.png'
$bitmap = New-Object System.Drawing.Bitmap $rectWidth, $rectHeight
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.CopyFromScreen($target.Rect.Left, $target.Rect.Top, 0, 0, $bitmap.Size)
$bitmap.Save($screenshotPath, [System.Drawing.Imaging.ImageFormat]::Png)
$graphics.Dispose()
$bitmap.Dispose()

$logRoots = @()
if (Test-Path -LiteralPath $targetUserDataDir) { $logRoots += $targetUserDataDir }
$logMatches = @()
foreach ($rootPath in $logRoots) {
  $files = @(Get-ChildItem -LiteralPath $rootPath -Recurse -File -ErrorAction SilentlyContinue | Where-Object { $_.Extension -in '.log','.txt','.json' -or $_.Name -like '*log*' })
  foreach ($file in $files) {
    try {
      $hits = Select-String -LiteralPath $file.FullName -Pattern 'SQLITE_CANTOPEN','extension-host timeout','extension host timeout','Extension host did not start' -SimpleMatch -ErrorAction SilentlyContinue
      foreach ($hit in $hits) {
        $logMatches += [pscustomobject]@{
          path = $file.FullName
          line = $hit.LineNumber
          text = $hit.Line.Trim()
        }
      }
    } catch {}
  }
}

$record = [pscustomobject]@{
  packet_type = 'ROW12_SHORT_TEMP_COMMAND_PALETTE_PROBE'
  probed_at_utc = (Get-Date).ToUniversalTime().ToString('o')
  launch_record_path = $launchRecordPath
  launch_record_utc = $launchRecord.launched_at_utc
  short_host_root = $launchRecord.short_host_root
  user_data_dir = $targetUserDataDir
  extensions_dir = $launchRecord.extensions_dir
  target_window_title = $target.Title
  target_process_id = $target.ProcessId
  target_native_window_handle = $target.HandleInt
  target_window_rect = [pscustomobject]@{
    left = $target.Rect.Left
    top = $target.Rect.Top
    right = $target.Rect.Right
    bottom = $target.Rect.Bottom
    width = $rectWidth
    height = $rectHeight
  }
  target_window_candidates = @($windows | Select-Object Title,ProcessId,HandleInt)
  command_palette_opened_by = 'F1 after Win32 ShowWindow+SetForegroundWindow+safe top-center click; exact search pasted into palette input'
  search_text = $searchText
  exact_command_title_found_in_accessibility_names = @($matchingNames).Count -gt 0
  matching_accessibility_names = $matchingNames
  xiigen_or_smart_skills_accessibility_names = $xiigenNames
  no_matching_commands_signals = $noMatchSignals
  screenshot_path = $screenshotPath
  log_scan_roots = $logRoots
  sqlite_cantopen_or_extension_host_timeout_hits = $logMatches
  launch_blocker_seen_in_available_logs = @($logMatches).Count -gt 0
  row12_submit_run = $false
  panel_opened = $false
  external_ai_used = $false
  readiness_claimed = $false
}
$probePath = Join-Path $logsDir 'command-palette-probe-after-short-temp-launch.json'
$record | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $probePath -Encoding UTF8
$record | ConvertTo-Json -Depth 8
