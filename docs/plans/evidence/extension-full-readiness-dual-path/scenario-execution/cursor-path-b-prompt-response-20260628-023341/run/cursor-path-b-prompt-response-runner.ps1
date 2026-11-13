param(
  [ValidateSet("PrepareTyped", "ActivateInspect", "CaptureOnly")]
  [string]$Mode,
  [string]$EvidenceRoot = "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\cursor-path-b-prompt-response-20260628-023341",
  [string]$ScenarioPath = "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-prep\cursor-full-extension-runtime-after-unlock-20260627-090159\scenario.json",
  [string]$SearchText = "Open Smart Skills Panel"
)

$ErrorActionPreference = "Stop"
$utf8NoBom = [System.Text.UTF8Encoding]::new($false)
$screenshotsDir = Join-Path $EvidenceRoot "screenshots"
$logsDir = Join-Path $EvidenceRoot "logs"
$manifestsDir = Join-Path $EvidenceRoot "manifests"
New-Item -ItemType Directory -Force -Path $screenshotsDir, $logsDir, $manifestsDir | Out-Null

function Write-Utf8NoBom([string]$Path, [string]$Text) {
  [System.IO.File]::WriteAllText($Path, $Text, $utf8NoBom)
}

function Save-Json([string]$Path, [object]$Value) {
  Write-Utf8NoBom -Path $Path -Text (($Value | ConvertTo-Json -Depth 12) + "`n")
}

function Get-Sha256Hex([string]$Text) {
  $sha = [System.Security.Cryptography.SHA256]::Create()
  try {
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Text)
    return (($sha.ComputeHash($bytes) | ForEach-Object { $_.ToString("x2") }) -join "")
  } finally {
    $sha.Dispose()
  }
}

$eventsPath = Join-Path $logsDir "runner-events.json"
$events = [System.Collections.Generic.List[object]]::new()
if (Test-Path -LiteralPath $eventsPath) {
  $previous = Get-Content -LiteralPath $eventsPath -Raw -Encoding UTF8 | ConvertFrom-Json
  foreach ($event in @($previous)) { $events.Add($event) | Out-Null }
}

function Add-Event([string]$Name, [hashtable]$Data = @{}) {
  $events.Add([pscustomobject]@{
    time = (Get-Date).ToString("o")
    name = $Name
    data = $Data
  }) | Out-Null
}

$scenarioText = [System.IO.File]::ReadAllText($ScenarioPath, [System.Text.Encoding]::UTF8)
$scenario = $scenarioText | ConvertFrom-Json
$exactPrompt = [string]$scenario.exact_prompt
$promptSha = Get-Sha256Hex $exactPrompt
$previewLength = [Math]::Min(120, $exactPrompt.Length)
$promptPreview = $exactPrompt.Substring(0, $previewLength)
if ($exactPrompt.Length -gt $previewLength) { $promptPreview = "$promptPreview..." }
Save-Json -Path (Join-Path $manifestsDir "prompt-meta.json") -Value ([pscustomobject]@{
  source = $ScenarioPath
  scenario_id = $scenario.scenario_id
  scenario_prompt_sha = $promptSha
  prompt_length_chars = $exactPrompt.Length
  redacted_preview = $promptPreview
  full_prompt_stored = $false
})

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class Win32PathBPromptResponse {
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

function Get-WindowTitle([IntPtr]$Handle) {
  $length = [Win32PathBPromptResponse]::GetWindowTextLength($Handle)
  if ($length -le 0) { return "" }
  $builder = [System.Text.StringBuilder]::new($length + 1)
  [void][Win32PathBPromptResponse]::GetWindowText($Handle, $builder, $builder.Capacity)
  return $builder.ToString()
}

function Get-TopLevelWindows {
  $windows = [System.Collections.Generic.List[object]]::new()
  $callback = [Win32PathBPromptResponse+EnumWindowsProc]{
    param([IntPtr]$hWnd, [IntPtr]$lParam)
    if ([Win32PathBPromptResponse]::IsWindowVisible($hWnd)) {
      $title = Get-WindowTitle $hWnd
      if ($title) {
        [uint32]$windowPid = 0
        [void][Win32PathBPromptResponse]::GetWindowThreadProcessId($hWnd, [ref]$windowPid)
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
  [void][Win32PathBPromptResponse]::EnumWindows($callback, [IntPtr]::Zero)
  return @($windows)
}

function Get-TargetCursorWindow {
  $target = @(Get-TopLevelWindows | Where-Object {
    $_.processName -eq "Cursor" -and
    $_.title -like "*[Extension Development Host]*" -and
    $_.title -like "*cursor-xiigen-smart-skills*"
  } | Select-Object -First 1)
  if ($target.Count -gt 0) { return $target[0] }
  return $null
}

function Focus-Window([string]$HandleHex) {
  $handle = [IntPtr]([Convert]::ToInt64($HandleHex.Replace("0x", ""), 16))
  [void][Win32PathBPromptResponse]::ShowWindow($handle, 9)
  Start-Sleep -Milliseconds 300
  [void][Win32PathBPromptResponse]::SetForegroundWindow($handle)
  Start-Sleep -Milliseconds 700
  return $handle
}

function Get-WindowRectObject([IntPtr]$Handle) {
  $rect = New-Object Win32PathBPromptResponse+RECT
  if (-not [Win32PathBPromptResponse]::GetWindowRect($Handle, [ref]$rect)) {
    throw "GetWindowRect failed for $Handle"
  }
  return $rect
}

function Capture-Window([IntPtr]$Handle, [string]$Path) {
  $rect = Get-WindowRectObject $Handle
  $width = [Math]::Max(1, $rect.Right - $rect.Left)
  $height = [Math]::Max(1, $rect.Bottom - $rect.Top)
  $bitmap = [System.Drawing.Bitmap]::new($width, $height)
  $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
  $graphics.CopyFromScreen($rect.Left, $rect.Top, 0, 0, [System.Drawing.Size]::new($width, $height))
  $bitmap.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png)
  $graphics.Dispose()
  $bitmap.Dispose()
  return [pscustomobject]@{ path = $Path; width = $width; height = $height; left = $rect.Left; top = $rect.Top }
}

function Capture-PanelCrop([IntPtr]$Handle, [string]$Path) {
  $rect = Get-WindowRectObject $Handle
  $windowWidth = [Math]::Max(1, $rect.Right - $rect.Left)
  $windowHeight = [Math]::Max(1, $rect.Bottom - $rect.Top)
  $cropX = $rect.Left + [Math]::Min(310, [int]($windowWidth * 0.25))
  $cropY = $rect.Top + 70
  $cropW = [Math]::Min([int]($windowWidth * 0.43), $windowWidth - ($cropX - $rect.Left) - 20)
  $cropH = [Math]::Max(1, $windowHeight - 115)
  $bitmap = [System.Drawing.Bitmap]::new($cropW, $cropH)
  $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
  $graphics.CopyFromScreen($cropX, $cropY, 0, 0, [System.Drawing.Size]::new($cropW, $cropH))
  $bitmap.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png)
  $graphics.Dispose()
  $bitmap.Dispose()
  return [pscustomobject]@{ path = $Path; width = $cropW; height = $cropH; left = $cropX; top = $cropY }
}

function Save-WindowInventory([string]$Path, [string]$Stage) {
  Save-Json -Path $Path -Value ([pscustomobject]@{
    schemaVersion = 1
    stage = $Stage
    capturedAt = (Get-Date).ToString("o")
    cursorWindows = @(Get-TopLevelWindows | Where-Object { $_.processName -eq "Cursor" -or $_.title -like "*Cursor*" })
  })
}

function Send-KeyChord([string]$Keys, [int]$DelayMs = 700) {
  [System.Windows.Forms.SendKeys]::SendWait($Keys)
  Start-Sleep -Milliseconds $DelayMs
}

$target = Get-TargetCursorWindow
if (-not $target) { throw "TARGET_WINDOW_NOT_FOUND" }
$handle = Focus-Window -HandleHex $target.handle
Add-Event "target_window_focused" @{ mode = $Mode; title = $target.title; handle = $target.handle }

if ($Mode -eq "PrepareTyped") {
  Save-WindowInventory -Path (Join-Path $EvidenceRoot "window-inventory.json") -Stage "prepare_before"
  Send-KeyChord "{ESC}" 300
  Send-KeyChord "{ESC}" 300
  Send-KeyChord "{ESC}" 500
  $shot1 = Capture-Window -Handle $handle -Path (Join-Path $screenshotsDir "01-before-command-palette.png")
  Add-Event "screenshot" @{ stage = "before_command_palette"; path = $shot1.path }
  Send-KeyChord "{F1}" 900
  Send-KeyChord "^a" 250
  [System.Windows.Forms.SendKeys]::SendWait($SearchText)
  Start-Sleep -Milliseconds 1400
  Add-Event "typed_panel_route_search" @{ text = $SearchText; scenarioPrompt = $false }
  $shot2 = Capture-Window -Handle $handle -Path (Join-Path $screenshotsDir "02-panel-command-before-activation.png")
  Add-Event "screenshot" @{ stage = "panel_command_before_activation"; path = $shot2.path }
  Save-Json -Path (Join-Path $logsDir "prepare-typed-summary.json") -Value ([pscustomobject]@{
    status = "PREPARE_TYPED_COMPLETED"
    searchText = $SearchText
    activationPerformed = $false
    scenarioPromptTyped = $false
    scenarioPromptSubmitted = $false
    screenshots = @($shot1.path, $shot2.path)
  })
  Save-Json -Path $eventsPath -Value @($events)
  Write-Output "PREPARE_TYPED_COMPLETED"
  exit 0
}

if ($Mode -eq "ActivateInspect") {
  $shot3 = Capture-Window -Handle $handle -Path (Join-Path $screenshotsDir "03-immediate-before-enter.png")
  Add-Event "screenshot" @{ stage = "immediate_before_enter"; path = $shot3.path }
  Send-KeyChord "{ENTER}" 3500
  Start-Sleep -Seconds 4
  $target = Get-TargetCursorWindow
  if ($target) {
    $handle = Focus-Window -HandleHex $target.handle
    Add-Event "target_window_refocused_after_enter" @{ title = $target.title; handle = $target.handle }
  }
  $shot4 = Capture-Window -Handle $handle -Path (Join-Path $screenshotsDir "04-panel-route-before-prompt.png")
  Add-Event "screenshot" @{ stage = "panel_route_before_prompt"; path = $shot4.path }
  $shot5 = Capture-PanelCrop -Handle $handle -Path (Join-Path $screenshotsDir "05-panel-area-inspection.png")
  Add-Event "screenshot" @{ stage = "panel_area_inspection"; path = $shot5.path }
  Save-WindowInventory -Path (Join-Path $logsDir "window-inventory-after-route.json") -Stage "after_route"
  Save-Json -Path (Join-Path $logsDir "activate-inspect-summary.json") -Value ([pscustomobject]@{
    status = "ACTIVATE_INSPECT_COMPLETED"
    activationPerformed = $true
    panelRouteExpected = "XIIGen Smart Skills"
    scenarioPromptTyped = $false
    scenarioPromptSubmitted = $false
    screenshots = @($shot3.path, $shot4.path, $shot5.path)
  })
  Save-Json -Path $eventsPath -Value @($events)
  Write-Output "ACTIVATE_INSPECT_COMPLETED"
  exit 0
}

if ($Mode -eq "CaptureOnly") {
  Start-Sleep -Seconds 2
  $target = Get-TargetCursorWindow
  if (-not $target) { throw "TARGET_WINDOW_NOT_FOUND_FOR_CAPTURE_ONLY" }
  $handle = Focus-Window -HandleHex $target.handle
  $shot = Capture-Window -Handle $handle -Path (Join-Path $screenshotsDir "06-final-blocker-state.png")
  Add-Event "screenshot" @{ stage = "final_blocker_state"; path = $shot.path }
  Save-WindowInventory -Path (Join-Path $logsDir "window-inventory-final.json") -Stage "final_blocker_state"
  Save-Json -Path (Join-Path $logsDir "capture-only-summary.json") -Value ([pscustomobject]@{
    status = "CAPTURE_ONLY_COMPLETED"
    scenarioPromptTyped = $false
    scenarioPromptSubmitted = $false
    screenshots = @($shot.path)
  })
  Save-Json -Path $eventsPath -Value @($events)
  Write-Output "CAPTURE_ONLY_COMPLETED"
  exit 0
}
