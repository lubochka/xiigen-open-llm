param(
  [string]$EvidenceRoot = "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\cursor-path-b-route-proof-20260628-020041",
  [string]$CursorExtensionRoot = "C:\Projects\cursor-xiigen-smart-skills",
  [string]$CommandTitle = "XIIGen: Open Smart Skills Panel"
)

$ErrorActionPreference = "Stop"
$screenshotsDir = Join-Path $EvidenceRoot "screenshots"
$logsDir = Join-Path $EvidenceRoot "logs"
$manifestsDir = Join-Path $EvidenceRoot "manifests"
New-Item -ItemType Directory -Force -Path $EvidenceRoot, $screenshotsDir, $logsDir, $manifestsDir | Out-Null

$events = [System.Collections.Generic.List[object]]::new()
function Add-Event([string]$name, [hashtable]$data = @{}) {
  $events.Add([pscustomobject]@{
    time = (Get-Date).ToString("o")
    name = $name
    data = $data
  }) | Out-Null
}

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class Win32RouteProof {
  public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
  [DllImport("user32.dll")] public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
  [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr hWnd);
  [DllImport("user32.dll")] public static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);
  [DllImport("user32.dll")] public static extern int GetWindowTextLength(IntPtr hWnd);
  [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
  [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);
  [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
}
"@

function Get-WindowTitle([IntPtr]$handle) {
  $length = [Win32RouteProof]::GetWindowTextLength($handle)
  if ($length -le 0) { return "" }
  $builder = [System.Text.StringBuilder]::new($length + 1)
  [void][Win32RouteProof]::GetWindowText($handle, $builder, $builder.Capacity)
  return $builder.ToString()
}

function Get-TopLevelWindows {
  $windows = [System.Collections.Generic.List[object]]::new()
  $callback = [Win32RouteProof+EnumWindowsProc]{
    param([IntPtr]$hWnd, [IntPtr]$lParam)
    if ([Win32RouteProof]::IsWindowVisible($hWnd)) {
      $title = Get-WindowTitle $hWnd
      if ($title) {
        [uint32]$windowPid = 0
        [void][Win32RouteProof]::GetWindowThreadProcessId($hWnd, [ref]$windowPid)
        $process = Get-Process -Id $windowPid -ErrorAction SilentlyContinue
        $windows.Add([pscustomobject]@{
          handle = ("0x{0:X}" -f $hWnd.ToInt64())
          processId = [int]$windowPid
          processName = if ($process) { $process.ProcessName } else { $null }
          title = $title
        }) | Out-Null
      }
    }
    return $true
  }
  [void][Win32RouteProof]::EnumWindows($callback, [IntPtr]::Zero)
  return @($windows)
}

function Save-WindowInventory([string]$path, [string]$stage) {
  $windows = Get-TopLevelWindows
  $cursorWindows = @($windows | Where-Object { $_.processName -eq "Cursor" -or $_.title -like "*Cursor*" })
  $payload = [pscustomobject]@{
    schemaVersion = 1
    stage = $stage
    capturedAt = (Get-Date).ToString("o")
    cursorWindows = $cursorWindows
    allWindowCount = @($windows).Count
  }
  $payload | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $path -Encoding UTF8
  return $payload
}

function Get-TargetCursorWindow {
  $windows = Get-TopLevelWindows
  $target = @($windows | Where-Object {
    $_.processName -eq "Cursor" -and
    $_.title -like "*[Extension Development Host]*" -and
    $_.title -like "*cursor-xiigen-smart-skills*"
  } | Select-Object -First 1)
  if ($target.Count -gt 0) { return $target[0] }
  return $null
}

function Start-CursorHostIfMissing {
  $target = Get-TargetCursorWindow
  if ($target) { return $target }
  $cursorExe = "C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe"
  Add-Event "target_window_missing_launching_cursor" @{ cursorExe = $cursorExe; extensionRoot = $CursorExtensionRoot }
  Start-Process -FilePath $cursorExe -ArgumentList @(
    "--extensionDevelopmentPath", $CursorExtensionRoot,
    "--new-window",
    "--disable-workspace-trust",
    "--skip-welcome",
    "--skip-release-notes",
    $CursorExtensionRoot
  ) -WindowStyle Hidden | Out-Null
  $deadline = (Get-Date).AddSeconds(45)
  do {
    Start-Sleep -Milliseconds 750
    $target = Get-TargetCursorWindow
    if ($target) { return $target }
  } while ((Get-Date) -lt $deadline)
  return $null
}

function Focus-Window([string]$handleHex) {
  $handle = [IntPtr]([Convert]::ToInt64($handleHex.Replace("0x", ""), 16))
  [void][Win32RouteProof]::ShowWindow($handle, 9)
  Start-Sleep -Milliseconds 300
  [void][Win32RouteProof]::SetForegroundWindow($handle)
  Start-Sleep -Milliseconds 600
  return $handle
}

function Capture-Window([IntPtr]$handle, [string]$path) {
  $rect = New-Object Win32RouteProof+RECT
  if (-not [Win32RouteProof]::GetWindowRect($handle, [ref]$rect)) {
    throw "GetWindowRect failed for $handle"
  }
  $width = [Math]::Max(1, $rect.Right - $rect.Left)
  $height = [Math]::Max(1, $rect.Bottom - $rect.Top)
  $bitmap = [System.Drawing.Bitmap]::new($width, $height)
  $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
  $graphics.CopyFromScreen($rect.Left, $rect.Top, 0, 0, [System.Drawing.Size]::new($width, $height))
  $bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
  $graphics.Dispose()
  $bitmap.Dispose()
  return [pscustomobject]@{ path = $path; width = $width; height = $height; left = $rect.Left; top = $rect.Top }
}

function Send-KeyChord([string]$keys, [int]$delayMs = 700) {
  [System.Windows.Forms.SendKeys]::SendWait($keys)
  Start-Sleep -Milliseconds $delayMs
}

$packagePath = Join-Path $CursorExtensionRoot "package.json"
$package = Get-Content -LiteralPath $packagePath -Raw | ConvertFrom-Json
$commandCandidates = @(
  $package.contributes.commands | ForEach-Object {
    [pscustomobject]@{
      sourceFile = $packagePath
      command = $_.command
      title = $_.title
      category = $_.category
      selectedForRouteProof = ($_.title -eq $CommandTitle)
    }
  }
)
$chatCandidates = @(
  $package.contributes.chatParticipants | ForEach-Object {
    [pscustomobject]@{
      sourceFile = $packagePath
      id = $_.id
      name = $_.name
      selectedForRouteProof = $false
    }
  }
)
@{ commands = $commandCandidates; chatParticipants = $chatCandidates } |
  ConvertTo-Json -Depth 8 |
  Set-Content -LiteralPath (Join-Path $manifestsDir "command-candidates.json") -Encoding UTF8
Add-Event "command_candidates_recorded" @{ selectedCommandTitle = $CommandTitle; packagePath = $packagePath }

$initialInventory = Save-WindowInventory -path (Join-Path $EvidenceRoot "window-inventory.json") -stage "before_focus"
$target = Start-CursorHostIfMissing
if (-not $target) {
  Add-Event "blocked_target_window_not_found" @{}
  Save-WindowInventory -path (Join-Path $logsDir "window-inventory-final.json") -stage "target_missing"
  $events | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $logsDir "runner-events.json") -Encoding UTF8
  throw "TARGET_WINDOW_NOT_FOUND"
}

$handle = Focus-Window -handleHex $target.handle
Add-Event "target_window_focused" @{ handle = $target.handle; title = $target.title }
$shot1 = Capture-Window -handle $handle -path (Join-Path $screenshotsDir "01-focused-before-tab-close.png")
Add-Event "screenshot" @{ path = $shot1.path; stage = "focused_before_tab_close" }

if ($target.title -like "*Browser Tab*") {
  Add-Event "browser_tab_visible_closing_first" @{ title = $target.title }
  Send-KeyChord "^w" 1200
  $target = Get-TargetCursorWindow
  if ($target) {
    $handle = Focus-Window -handleHex $target.handle
    Add-Event "after_browser_tab_close" @{ title = $target.title }
  } else {
    Add-Event "after_browser_tab_close_target_missing" @{}
  }
}

$target = Get-TargetCursorWindow
if (-not $target) {
  Save-WindowInventory -path (Join-Path $logsDir "window-inventory-after-tab-close.json") -stage "after_tab_close_target_missing"
  $events | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $logsDir "runner-events.json") -Encoding UTF8
  throw "TARGET_WINDOW_NOT_FOUND_AFTER_TAB_CLOSE"
}

$handle = Focus-Window -handleHex $target.handle
Save-WindowInventory -path (Join-Path $logsDir "window-inventory-after-tab-close.json") -stage "after_tab_close" | Out-Null
$shot2 = Capture-Window -handle $handle -path (Join-Path $screenshotsDir "02-after-tab-close.png")
Add-Event "screenshot" @{ path = $shot2.path; stage = "after_tab_close" }

Send-KeyChord "^+p" 900
[System.Windows.Forms.Clipboard]::SetText($CommandTitle)
Add-Event "clipboard_set_for_command_palette" @{ text = $CommandTitle; scenarioPrompt = $false }
Send-KeyChord "^v" 900
$shot3 = Capture-Window -handle $handle -path (Join-Path $screenshotsDir "03-command-palette-candidate-pasted.png")
Add-Event "screenshot" @{ path = $shot3.path; stage = "command_palette_candidate_pasted" }

Send-KeyChord "{ENTER}" 4000
$target = Get-TargetCursorWindow
if ($target) {
  $handle = Focus-Window -handleHex $target.handle
}
Start-Sleep -Seconds 6
$shot4 = Capture-Window -handle $handle -path (Join-Path $screenshotsDir "04-route-result.png")
Add-Event "screenshot" @{ path = $shot4.path; stage = "route_result" }

Save-WindowInventory -path (Join-Path $logsDir "window-inventory-final.json") -stage "final" | Out-Null
$events | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $logsDir "runner-events.json") -Encoding UTF8

[pscustomobject]@{
  status = "RUNNER_COMPLETED"
  targetWindowInitialTitle = $target.title
  commandTitle = $CommandTitle
  scenarioPromptTyped = $false
  scenarioPromptSubmitted = $false
  screenshots = @($shot1.path, $shot2.path, $shot3.path, $shot4.path)
} | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $logsDir "runner-summary.json") -Encoding UTF8

Write-Output "RUNNER_COMPLETED"
