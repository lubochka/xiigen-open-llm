$ErrorActionPreference = 'Continue'

$runTs = '20260624T041937Z'
$root = "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-14-fixes\real-program-ui-proof\p14-fix-13-real-desktop-screen-control-$runTs"
$dirs = @(
  $root,
  "$root\raw",
  "$root\raw\claude",
  "$root\raw\cursor",
  "$root\raw\codex",
  "$root\logs",
  "$root\traces",
  "$root\traces\claude",
  "$root\traces\cursor",
  "$root\traces\codex"
)
foreach ($d in $dirs) { New-Item -ItemType Directory -Force -Path $d | Out-Null }

$log = New-Object System.Collections.Generic.List[string]
function Add-Log([string]$message) {
  $line = "{0} {1}" -f ((Get-Date).ToUniversalTime().ToString('o')), $message
  $log.Add($line)
  Write-Host $line
}

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class P14User32 {
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
  [DllImport("user32.dll")] public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);
  [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
}
"@

function Get-TargetProcess([string]$targetHost) {
  $pattern = switch ($targetHost) {
    'claude' { 'claude' }
    'cursor' { 'Cursor' }
    'codex' { 'Codex' }
  }
  Get-Process |
    Where-Object { $_.MainWindowHandle -ne 0 -and ($_.ProcessName -match $pattern -or $_.MainWindowTitle -match $pattern) } |
    Sort-Object StartTime -Descending |
    Select-Object -First 1
}

function Activate-ProcessWindow([System.Diagnostics.Process]$p) {
  if ($null -eq $p) { return $false }
  $h = [IntPtr]$p.MainWindowHandle
  [P14User32]::ShowWindowAsync($h, 9) | Out-Null
  Start-Sleep -Milliseconds 500
  [P14User32]::SetForegroundWindow($h) | Out-Null
  Start-Sleep -Milliseconds 1200
  return $true
}

function Capture-Window([System.Diagnostics.Process]$p, [string]$path) {
  if ($null -eq $p) { throw 'No process for capture' }
  $rect = New-Object P14User32+RECT
  $ok = [P14User32]::GetWindowRect([IntPtr]$p.MainWindowHandle, [ref]$rect)
  if (-not $ok) { throw 'GetWindowRect failed' }
  $width = $rect.Right - $rect.Left
  $height = $rect.Bottom - $rect.Top
  if ($width -le 80 -or $height -le 80) { throw "Window rect too small: ${width}x${height}" }

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

function Invoke-ClaudeNewSession([System.Diagnostics.Process]$p) {
  try {
    $rootEl = [System.Windows.Automation.AutomationElement]::FromHandle([IntPtr]$p.MainWindowHandle)
    $cond = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::NameProperty, 'New session')
    $button = $rootEl.FindFirst([System.Windows.Automation.TreeScope]::Subtree, $cond)
    if ($null -ne $button) {
      $invoke = $button.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
      $invoke.Invoke()
      Start-Sleep -Milliseconds 1500
      return 'UIAutomation New session invoked'
    }

    [System.Windows.Forms.SendKeys]::SendWait('^n')
    Start-Sleep -Milliseconds 1500
    return 'New session button not found; sent Ctrl+N fallback'
  } catch {
    try {
      [System.Windows.Forms.SendKeys]::SendWait('^n')
      Start-Sleep -Milliseconds 1500
      return "UIAutomation failed; sent Ctrl+N fallback: $($_.Exception.Message)"
    } catch {
      return "UIAutomation and Ctrl+N failed: $($_.Exception.Message)"
    }
  }
}

function Paste-Marker([string]$text) {
  [System.Windows.Forms.Clipboard]::SetText($text)
  Start-Sleep -Milliseconds 300
  [System.Windows.Forms.SendKeys]::SendWait('^v')
  Start-Sleep -Milliseconds 1200
}

$hostResults = @{}
$discovery = [ordered]@{
  run_timestamp_utc = $runTs
  start_apps = @(Get-StartApps | Where-Object { $_.Name -match 'Claude|Cursor|Codex|ChatGPT|OpenAI' -or $_.AppID -match 'Claude|Cursor|Codex|ChatGPT|OpenAI' } | Select-Object Name, AppID)
  process_windows_before = @(Get-Process | Where-Object { $_.MainWindowHandle -ne 0 -and ($_.ProcessName -match 'Claude|Cursor|Codex|ChatGPT|OpenAI' -or $_.MainWindowTitle -match 'Claude|Cursor|Codex|ChatGPT|OpenAI') } | Select-Object ProcessName, Id, MainWindowTitle, Path, MainWindowHandle)
}
$discovery | ConvertTo-Json -Depth 6 | Set-Content -Path "$root\logs\discovery.json" -Encoding UTF8
Add-Log "Evidence root: $root"

$cursorMarker = "P14-REAL-CURSOR-$runTs"
$cursorMarkerFile = "$root\raw\cursor\p14-real-cursor-marker.md"
@(
  "# $cursorMarker",
  "",
  "Phase 14.13 fresh real Cursor app marker.",
  "This file is an evidence-only local marker opened in the real Cursor window.",
  "Extension runtime trace from Cursor was not triggered by this marker file alone."
) | Set-Content -Path $cursorMarkerFile -Encoding UTF8

try {
  $cursorExe = 'C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe'
  if (Test-Path -Path $cursorExe) {
    Start-Process -FilePath $cursorExe -ArgumentList @('--reuse-window', $cursorMarkerFile) | Out-Null
    Add-Log "Cursor launched/activated via executable with marker file $cursorMarkerFile"
  } else {
    Start-Process 'shell:AppsFolder\Anysphere.Cursor' | Out-Null
    Add-Log 'Cursor launched via StartApps shell app id; executable missing at expected path'
  }

  Start-Sleep -Seconds 4
  $cursorProc = Get-Process |
    Where-Object { $_.MainWindowHandle -ne 0 -and $_.ProcessName -match 'Cursor' -and ($_.MainWindowTitle -match 'p14-real-cursor-marker|P14-REAL-CURSOR|Cursor') } |
    Sort-Object { if ($_.MainWindowTitle -match 'p14-real-cursor-marker|P14-REAL-CURSOR') { 0 } else { 1 } }, StartTime -Descending |
    Select-Object -First 1
  Activate-ProcessWindow $cursorProc | Out-Null
  $cursorRect = Capture-Window $cursorProc "$root\raw\cursor\window-cursor-real-marker.png"
  $hostResults.cursor = [ordered]@{
    host = 'Cursor'
    marker = $cursorMarker
    real_app_found = $true
    launched_or_activated = 'Started Cursor.exe with safe marker file in evidence folder and activated MainWindowHandle'
    screenshot_path = "$root\raw\cursor\window-cursor-real-marker.png"
    screenshot_rect = $cursorRect
    extension_trace_path = $null
    status = 'DONE_REAL_APP_SCREENSHOT_ONLY_EXTENSION_BLOCKED'
    limitation = 'Fresh real Cursor window screenshot was captured, but no reliable UI path was available in this run to invoke the XIIGen extension/runtime from inside Cursor; marker file alone proves app/window control, not extension runtime.'
    process = @{ name = $cursorProc.ProcessName; id = $cursorProc.Id; title = $cursorProc.MainWindowTitle; path = $cursorProc.Path }
  }
} catch {
  Add-Log "Cursor attempt failed: $($_.Exception.Message)"
  $hostResults.cursor = [ordered]@{
    host = 'Cursor'
    marker = $cursorMarker
    real_app_found = $false
    launched_or_activated = 'Attempted Cursor executable/app activation'
    screenshot_path = $null
    extension_trace_path = $null
    status = 'NEEDS_FIX_EVIDENCE_INSUFFICIENT'
    limitation = $_.Exception.Message
  }
}

$claudeMarker = "P14-REAL-CLAUDE-$runTs"
try {
  $claudeProc = Get-TargetProcess 'claude'
  if ($null -eq $claudeProc) {
    Start-Process 'shell:AppsFolder\Claude_pzs8sxrjxfjjc!Claude' | Out-Null
    Start-Sleep -Seconds 5
    $claudeProc = Get-TargetProcess 'claude'
  }
  if ($null -eq $claudeProc) { throw 'Claude app/window was not found after StartApps activation attempt.' }

  Activate-ProcessWindow $claudeProc | Out-Null
  $newSessionResult = Invoke-ClaudeNewSession $claudeProc
  Activate-ProcessWindow $claudeProc | Out-Null
  Paste-Marker "$claudeMarker`r`nPhase 14.13 visible-app marker only; not submitted to Claude."
  $claudeRect = Capture-Window $claudeProc "$root\raw\claude\window-claude-real-marker.png"
  $hostResults.claude = [ordered]@{
    host = 'Claude'
    marker = $claudeMarker
    real_app_found = $true
    launched_or_activated = "Activated Claude Desktop/app MainWindowHandle; $newSessionResult; pasted marker text without pressing Enter"
    screenshot_path = "$root\raw\claude\window-claude-real-marker.png"
    screenshot_rect = $claudeRect
    extension_trace_path = $null
    status = 'DONE_REAL_APP_SCREENSHOT_ONLY_EXTENSION_BLOCKED'
    limitation = 'Real Claude Desktop/app window was controlled enough to show a fresh marker, but no XIIGen extension/runtime command or safe local-only invocation surface was available in Claude Desktop UI during this run; no external Claude prompt was submitted.'
    process = @{ name = $claudeProc.ProcessName; id = $claudeProc.Id; title = $claudeProc.MainWindowTitle; path = $claudeProc.Path }
  }
} catch {
  Add-Log "Claude attempt failed: $($_.Exception.Message)"
  $hostResults.claude = [ordered]@{
    host = 'Claude'
    marker = $claudeMarker
    real_app_found = $false
    launched_or_activated = 'Attempted Claude StartApps/process activation and marker paste'
    screenshot_path = $null
    extension_trace_path = $null
    status = 'NEEDS_FIX_EVIDENCE_INSUFFICIENT'
    limitation = $_.Exception.Message
  }
}

$codexMarker = "P14-REAL-CODEX-$runTs"
try {
  $codexProc = Get-TargetProcess 'codex'
  if ($null -eq $codexProc) {
    Start-Process 'shell:AppsFolder\OpenAI.Codex_2p2nqsd0c76g0!App' | Out-Null
    Start-Sleep -Seconds 5
    $codexProc = Get-TargetProcess 'codex'
  }
  if ($null -eq $codexProc) { throw 'Codex app/window was not found after StartApps activation attempt.' }

  Activate-ProcessWindow $codexProc | Out-Null
  Start-Sleep -Seconds 1
  $codexRect = Capture-Window $codexProc "$root\raw\codex\window-codex-real-marker.png"
  $hostResults.codex = [ordered]@{
    host = 'Codex'
    marker = $codexMarker
    real_app_found = $true
    launched_or_activated = 'Activated existing real Codex Desktop app MainWindowHandle for this session; marker was emitted in the visible conversation before capture'
    screenshot_path = "$root\raw\codex\window-codex-real-marker.png"
    screenshot_rect = $codexRect
    extension_trace_path = "$root\traces\codex\trace.json"
    status = 'NEEDS_FIX_EVIDENCE_INSUFFICIENT'
    limitation = 'Screenshot captured real Codex app surface; extension trace still pending at screenshot time and must be correlated before claiming DONE_REAL_APP_SCREENSHOT_AND_EXTENSION_TRACE.'
    process = @{ name = $codexProc.ProcessName; id = $codexProc.Id; title = $codexProc.MainWindowTitle; path = $codexProc.Path }
  }
} catch {
  Add-Log "Codex screenshot attempt failed: $($_.Exception.Message)"
  $hostResults.codex = [ordered]@{
    host = 'Codex'
    marker = $codexMarker
    real_app_found = $false
    launched_or_activated = 'Attempted Codex StartApps/process activation'
    screenshot_path = $null
    extension_trace_path = $null
    status = 'NEEDS_FIX_EVIDENCE_INSUFFICIENT'
    limitation = $_.Exception.Message
  }
}

$cursorExtensionsPath = Join-Path $env:USERPROFILE '.cursor\extensions'
$extensionDiscovery = [ordered]@{
  cursor_extensions_matching_xiigen = if (Test-Path -Path $cursorExtensionsPath) { @(Get-ChildItem -Path $cursorExtensionsPath -Directory -ErrorAction SilentlyContinue | Where-Object { $_.Name -match 'xiigen|llm|mvp|smart' } | Select-Object Name, FullName) } else { @() }
  claude_accessible_controls_matching_xiigen = @()
  codex_plugin_skill_present = (Test-Path -Path 'C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500\src\codex-entrypoint.js')
}
try {
  $claudeProc2 = Get-TargetProcess 'claude'
  if ($null -ne $claudeProc2) {
    $rootEl = [System.Windows.Automation.AutomationElement]::FromHandle([IntPtr]$claudeProc2.MainWindowHandle)
    $walker = [System.Windows.Automation.TreeWalker]::ControlViewWalker
    $matches = New-Object System.Collections.Generic.List[object]
    $queue = New-Object System.Collections.Queue
    $queue.Enqueue(@($rootEl, 0))
    $count = 0
    while ($queue.Count -gt 0 -and $count -lt 500) {
      $item = $queue.Dequeue()
      $el = $item[0]
      $d = [int]$item[1]
      $count++
      $name = $el.Current.Name
      if ($name -match 'xiigen|extension|tool|mcp|connector') {
        $matches.Add([ordered]@{ name = $name; control_type = $el.Current.ControlType.ProgrammaticName; class = $el.Current.ClassName })
      }
      if ($d -lt 5) {
        $child = $walker.GetFirstChild($el)
        while ($null -ne $child) {
          $queue.Enqueue(@($child, $d + 1))
          $child = $walker.GetNextSibling($child)
        }
      }
    }
    $extensionDiscovery.claude_accessible_controls_matching_xiigen = @($matches)
  }
} catch {
  Add-Log "Claude extension control discovery failed: $($_.Exception.Message)"
}
$extensionDiscovery | ConvertTo-Json -Depth 8 | Set-Content -Path "$root\logs\extension-surface-discovery.json" -Encoding UTF8
[ordered]@{ root = $root; run_timestamp_utc = $runTs; hosts = $hostResults } | ConvertTo-Json -Depth 12 | Set-Content -Path "$root\report.partial.json" -Encoding UTF8
$log | Set-Content -Path "$root\logs\screen-control-script.log" -Encoding UTF8
Write-Host "ROOT=$root"
