Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$RunFolder = 'C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\micro-open-panel-run-20260625T100845Z'
$LogsDir = Join-Path $RunFolder 'logs'
$ScreenshotsDir = Join-Path $RunFolder 'screenshots'
$CursorExe = 'C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe'
$ExtensionRoot = 'C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills'
$ProbeJs = Join-Path $LogsDir 'api-open-panel-host-probe.js'
$CommandResultPath = Join-Path $LogsDir 'api-open-panel-command-result.json'
$StopPath = Join-Path $LogsDir 'api-open-panel-stop.txt'
$JsonPath = Join-Path $RunFolder 'api-open-panel-availability.json'
$MdPath = Join-Path $RunFolder 'api-open-panel-availability.md'
$ScreenshotPath = Join-Path $ScreenshotsDir 'api-open-panel-visible-window.png'

New-Item -ItemType Directory -Force -Path $LogsDir, $ScreenshotsDir | Out-Null
Remove-Item -LiteralPath $CommandResultPath, $StopPath, $JsonPath, $MdPath, $ScreenshotPath -Force -ErrorAction SilentlyContinue

Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

$Signature = @'
using System;
using System.Runtime.InteropServices;
using System.Text;
public static class Row12ApiOpenPanelWin32 {
  public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
  [DllImport("user32.dll")] public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
  [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr hWnd);
  [DllImport("user32.dll", SetLastError=true)] public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
  [DllImport("user32.dll", SetLastError=true)] public static extern int GetWindowTextLength(IntPtr hWnd);
  [DllImport("user32.dll", SetLastError=true)] public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
  [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
  [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
  [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);
  [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
}
'@
Add-Type $Signature

function ConvertTo-RelativeEvidencePath {
  param([Parameter(Mandatory=$true)][string]$Path)
  if ($Path.StartsWith($RunFolder, [System.StringComparison]::OrdinalIgnoreCase)) {
    return $Path.Substring($RunFolder.Length).TrimStart('\')
  }
  return $Path
}

function Get-CursorDevWindows {
  $Windows = New-Object System.Collections.Generic.List[object]
  $Callback = [Row12ApiOpenPanelWin32+EnumWindowsProc]{
    param([IntPtr]$Hwnd, [IntPtr]$LParam)
    if (-not [Row12ApiOpenPanelWin32]::IsWindowVisible($Hwnd)) { return $true }
    $Len = [Row12ApiOpenPanelWin32]::GetWindowTextLength($Hwnd)
    if ($Len -le 0) { return $true }
    $Sb = New-Object System.Text.StringBuilder ($Len + 1)
    [void][Row12ApiOpenPanelWin32]::GetWindowText($Hwnd, $Sb, $Sb.Capacity)
    [uint32]$WindowPid = 0
    [void][Row12ApiOpenPanelWin32]::GetWindowThreadProcessId($Hwnd, [ref]$WindowPid)
    $Title = $Sb.ToString()
    if ($Title -like '*Extension Development Host*Cursor*') {
      $Rect = New-Object Row12ApiOpenPanelWin32+RECT
      [void][Row12ApiOpenPanelWin32]::GetWindowRect($Hwnd, [ref]$Rect)
      $Windows.Add([pscustomobject]@{
        Handle = $Hwnd
        HandleInt = $Hwnd.ToInt64()
        ProcessId = [int]$WindowPid
        Title = $Title
        Rect = $Rect
      })
    }
    return $true
  }
  [void][Row12ApiOpenPanelWin32]::EnumWindows($Callback, [IntPtr]::Zero)
  return @($Windows.ToArray())
}

function Save-TargetScreenshot {
  param(
    [Parameter(Mandatory=$true)] $Target,
    [Parameter(Mandatory=$true)] [string] $Path
  )
  $Width = [Math]::Max(1, $Target.Rect.Right - $Target.Rect.Left)
  $Height = [Math]::Max(1, $Target.Rect.Bottom - $Target.Rect.Top)
  $Bitmap = New-Object System.Drawing.Bitmap $Width, $Height
  $Graphics = [System.Drawing.Graphics]::FromImage($Bitmap)
  $Hdc = $Graphics.GetHdc()
  $Printed = $false
  try {
    $Printed = [Row12ApiOpenPanelWin32]::PrintWindow($Target.Handle, $Hdc, 2)
  } finally {
    $Graphics.ReleaseHdc($Hdc)
  }
  if (-not $Printed) {
    $Graphics.CopyFromScreen($Target.Rect.Left, $Target.Rect.Top, 0, 0, $Bitmap.Size)
  }
  $Bitmap.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png)
  $Graphics.Dispose()
  $Bitmap.Dispose()
}

function Get-UiRows {
  param([Parameter(Mandatory=$true)] $Target)
  $Root = [System.Windows.Automation.AutomationElement]::FromHandle($Target.Handle)
  $All = @($Root.FindAll([System.Windows.Automation.TreeScope]::Subtree, [System.Windows.Automation.Condition]::TrueCondition))
  $Rows = foreach ($El in $All) {
    try {
      [pscustomobject]@{
        name = $El.Current.Name
        automation_id = $El.Current.AutomationId
        class_name = $El.Current.ClassName
        control_type = $El.Current.ControlType.ProgrammaticName
        is_enabled = $El.Current.IsEnabled
        is_offscreen = $El.Current.IsOffscreen
      }
    } catch {}
  }
  return @($Rows | Where-Object { $_.name -or $_.automation_id -or $_.class_name })
}

function Set-TargetVisible {
  param([Parameter(Mandatory=$true)] $Target)
  [void][Row12ApiOpenPanelWin32]::ShowWindow($Target.Handle, 9)
  Start-Sleep -Milliseconds 300
  $Width = [Math]::Max(1, $Target.Rect.Right - $Target.Rect.Left)
  $Height = [Math]::Max(1, $Target.Rect.Bottom - $Target.Rect.Top)
  [void][Row12ApiOpenPanelWin32]::SetWindowPos($Target.Handle, [IntPtr](-1), $Target.Rect.Left, $Target.Rect.Top, $Width, $Height, 0x0040)
  Start-Sleep -Milliseconds 300
  [void][Row12ApiOpenPanelWin32]::SetForegroundWindow($Target.Handle)
  Start-Sleep -Seconds 1
  [void][Row12ApiOpenPanelWin32]::SetWindowPos($Target.Handle, [IntPtr](-2), $Target.Rect.Left, $Target.Rect.Top, $Width, $Height, 0x0040)
}

$ShortHostRoot = Join-Path ([System.IO.Path]::GetTempPath()) ('xiigen-api-open-panel-' + (Get-Date -Format 'yyyyMMddHHmmss'))
$UserDataDir = Join-Path $ShortHostRoot 'user-data'
$ExtensionsDir = Join-Path $ShortHostRoot 'extensions'
New-Item -ItemType Directory -Force -Path $UserDataDir, $ExtensionsDir | Out-Null

$Arguments = @(
  '--new-window',
  '--user-data-dir', $UserDataDir,
  '--extensions-dir', $ExtensionsDir,
  "--extensionDevelopmentPath=$ExtensionRoot",
  "--extensionTestsPath=$ProbeJs",
  '--disable-workspace-trust',
  '--skip-welcome',
  '--skip-release-notes',
  '--skip-onboarding',
  '--disable-telemetry',
  '--disable-updates',
  '--disable-gpu',
  $ExtensionRoot
)

$ProcessBefore = @(Get-CimInstance Win32_Process -Filter "Name = 'Cursor.exe'" | Select-Object ProcessId, CommandLine)
Push-Location -LiteralPath $ExtensionRoot
try {
  & $CursorExe @Arguments
} finally {
  Pop-Location
}

$CommandDeadline = (Get-Date).AddSeconds(60)
while (-not (Test-Path -LiteralPath $CommandResultPath) -and (Get-Date) -lt $CommandDeadline) {
  Start-Sleep -Milliseconds 500
}

$CommandEvidence = $null
if (Test-Path -LiteralPath $CommandResultPath) {
  $CommandEvidence = Get-Content -LiteralPath $CommandResultPath -Raw | ConvertFrom-Json
}

$ProcessAfter = @(Get-CimInstance Win32_Process -Filter "Name = 'Cursor.exe'" | Select-Object ProcessId, CommandLine)
$MatchingProcesses = @($ProcessAfter | Where-Object {
  $_.CommandLine -and $_.CommandLine.Contains($UserDataDir) -and $_.CommandLine.Contains('--extensionDevelopmentPath')
})
$TargetPid = if (@($MatchingProcesses).Count -gt 0) { [int]$MatchingProcesses[0].ProcessId } else { $null }

$Windows = Get-CursorDevWindows
$Target = if ($TargetPid) { $Windows | Where-Object { $_.ProcessId -eq $TargetPid } | Select-Object -First 1 } else { $null }
if (-not $Target) {
  $Target = $Windows | Sort-Object ProcessId -Descending | Select-Object -First 1
}

$UiRows = @()
$VisibleControls = [ordered]@{
  title_or_header_xiigen_smart_skills = $false
  textarea_or_input_id_xiigen_smart_skills_prompt = $false
  submit_button_id_xiigen_smart_skills_submit = $false
}
$ScreenshotCaptured = $false

if ($Target) {
  Set-TargetVisible -Target $Target
  Start-Sleep -Seconds 4
  Save-TargetScreenshot -Target $Target -Path $ScreenshotPath
  $ScreenshotCaptured = Test-Path -LiteralPath $ScreenshotPath
  $UiRows = Get-UiRows -Target $Target
  $VisibleControls.title_or_header_xiigen_smart_skills = [bool](@($UiRows | Where-Object {
    $_.name -eq 'XIIGen Smart Skills' -and $_.is_offscreen -eq $false
  }).Count)
  $VisibleControls.textarea_or_input_id_xiigen_smart_skills_prompt = [bool](@($UiRows | Where-Object {
    $_.automation_id -eq 'xiigen-smart-skills-prompt' -and $_.control_type -eq 'ControlType.Edit' -and $_.is_offscreen -eq $false
  }).Count)
  $VisibleControls.submit_button_id_xiigen_smart_skills_submit = [bool](@($UiRows | Where-Object {
    $_.automation_id -eq 'xiigen-smart-skills-submit' -and $_.control_type -eq 'ControlType.Button' -and $_.is_offscreen -eq $false
  }).Count)
}

$ApiSucceeded = $CommandEvidence -and $CommandEvidence.api_command -and $CommandEvidence.api_command.succeeded -eq $true
$Status = if (-not $ApiSucceeded) {
  'API_OPEN_PANEL_COMMAND_FAILED'
} elseif ($VisibleControls.title_or_header_xiigen_smart_skills -and $VisibleControls.textarea_or_input_id_xiigen_smart_skills_prompt -and $VisibleControls.submit_button_id_xiigen_smart_skills_submit) {
  'API_OPEN_PANEL_VISIBLE_WITH_INPUT_SUBMIT'
} elseif ($VisibleControls.title_or_header_xiigen_smart_skills) {
  'API_OPEN_PANEL_VISIBLE_CONTROLS_MISSING'
} else {
  'API_OPEN_PANEL_BLOCKED_WITH_EVIDENCE'
}

$RelevantRows = @($UiRows | Where-Object {
  $_.name -like '*XIIGen*' -or
  $_.name -like '*Smart*' -or
  $_.name -like '*prompt*' -or
  $_.name -like '*Submit*' -or
  $_.automation_id -like '*xiigen*' -or
  $_.automation_id -like '*prompt*' -or
  $_.automation_id -like '*submit*'
} | Select-Object -First 80)

$Record = [pscustomobject]@{
  packet_type = 'ROW12_API_OPEN_PANEL_AVAILABILITY_PACKET'
  status = $Status
  checked_at_utc = (Get-Date).ToUniversalTime().ToString('o')
  run_folder = $RunFolder
  files_created = @(
    (ConvertTo-RelativeEvidencePath -Path $JsonPath),
    (ConvertTo-RelativeEvidencePath -Path $MdPath),
    (ConvertTo-RelativeEvidencePath -Path $ProbeJs),
    (ConvertTo-RelativeEvidencePath -Path (Join-Path $LogsDir 'run-api-open-panel-availability.ps1')),
    (ConvertTo-RelativeEvidencePath -Path $CommandResultPath),
    (ConvertTo-RelativeEvidencePath -Path $ScreenshotPath)
  )
  authority_and_boundaries = [pscustomobject]@{
    current_task = 'Run the smallest panel availability check using extension-host API/command execution to open xiigenSmartSkills.panel after activation, then verify visible textarea and submit button.'
    git_authority = 'forbidden'
    command_palette_used = $false
    row12_prompt_typed = $false
    row12_submit_run = $false
    login_or_browser_recovery_run = $false
    external_ai_used = $false
    readiness_claimed = $false
  }
  launch_evidence = [pscustomobject]@{
    cursor_exe = $CursorExe
    extension_development_path = $ExtensionRoot
    extension_tests_path = $ProbeJs
    short_host_root = $ShortHostRoot
    user_data_dir = $UserDataDir
    extensions_dir = $ExtensionsDir
    argument_array = $Arguments
    process_before_count = @($ProcessBefore).Count
    process_after_count = @($ProcessAfter).Count
    matching_processes = $MatchingProcesses
  }
  api_command_evidence = $CommandEvidence
  target_window = [pscustomobject]@{
    found = [bool]$Target
    title = if ($Target) { $Target.Title } else { $null }
    process_id = if ($Target) { $Target.ProcessId } else { $null }
    native_window_handle = if ($Target) { $Target.HandleInt } else { $null }
    bounds = if ($Target) {
      [pscustomobject]@{
        left = $Target.Rect.Left
        top = $Target.Rect.Top
        right = $Target.Rect.Right
        bottom = $Target.Rect.Bottom
        width = [Math]::Max(1, $Target.Rect.Right - $Target.Rect.Left)
        height = [Math]::Max(1, $Target.Rect.Bottom - $Target.Rect.Top)
      }
    } else { $null }
    target_window_candidates = @($Windows | Select-Object Title, ProcessId, HandleInt)
  }
  visible_controls = $VisibleControls
  panel_accessibility_excerpt = $RelevantRows
  screenshot_status = [pscustomobject]@{
    captured = $ScreenshotCaptured
    path = if ($ScreenshotCaptured) { ConvertTo-RelativeEvidencePath -Path $ScreenshotPath } else { $null }
    visual_inspection_required_by_worker = $true
  }
  what_was_proven = if ($Status -eq 'API_OPEN_PANEL_VISIBLE_WITH_INPUT_SUBMIT') {
    'Cursor Extension Host API activation succeeded, vscode.commands.executeCommand("xiigenSmartSkills.panel") succeeded, and the visible panel exposes title/header XIIGen Smart Skills plus visible automation ids xiigen-smart-skills-prompt and xiigen-smart-skills-submit.'
  } elseif ($Status -eq 'API_OPEN_PANEL_VISIBLE_CONTROLS_MISSING') {
    'Cursor Extension Host API command execution opened a visible XIIGen Smart Skills panel, but one or both required controls were missing from visible accessibility evidence.'
  } elseif ($Status -eq 'API_OPEN_PANEL_COMMAND_FAILED') {
    'The API command execution path did not succeed.'
  } else {
    'The run produced blocking evidence before a visible panel with required controls could be proven.'
  }
  what_is_not_proven = 'This does not prove row12 prompt submit, token savings, semantic quality, training, checkpoint promotion, full Cursor readiness, browser/login state, command-palette discovery, or any external AI behavior.'
  human_explanation = if ($Status -eq 'API_OPEN_PANEL_VISIBLE_WITH_INPUT_SUBMIT') {
    'The smallest API-open panel availability path works: after activation, the extension-host API opened the panel and the visible control tree contains the expected textarea and submit button. No prompt was typed and submit was not clicked.'
  } else {
    'The run stayed inside the smallest availability slice and captured evidence for the remaining blocker. No prompt was typed and submit was not clicked.'
  }
  next_smallest_action = if ($Status -eq 'API_OPEN_PANEL_VISIBLE_WITH_INPUT_SUBMIT') {
    'Under a separate authorization, run the smallest no-command-palette row12 UI submit slice using the now-visible textarea and submit button.'
  } else {
    'Fix or rerun only the blocked API-open panel availability slice; do not proceed to row12 submit from this evidence.'
  }
}

$Record | ConvertTo-Json -Depth 14 | Set-Content -LiteralPath $JsonPath -Encoding UTF8

$Markdown = @(
  '# Row12 API Open Panel Availability',
  '',
  'ROW12_API_OPEN_PANEL_AVAILABILITY_PACKET',
  "status: $($Record.status)",
  "run_folder: $RunFolder",
  '',
  '## Files Created',
  ($Record.files_created | ForEach-Object { "- $_" }),
  '',
  '## API Command Evidence',
  "- command: $($CommandEvidence.target_command)",
  "- extension_present: $($CommandEvidence.extension_present)",
  "- activation_succeeded: $($CommandEvidence.activation.succeeded)",
  "- target_present_after_activation: $($CommandEvidence.target_present_after_activation)",
  "- api_command_executed: $($CommandEvidence.api_command.executed)",
  "- api_command_succeeded: $($CommandEvidence.api_command.succeeded)",
  "- api_command_error: $($CommandEvidence.api_command.error)",
  '',
  '## Visible Controls',
  "- XIIGen Smart Skills title/header visible: $($VisibleControls.title_or_header_xiigen_smart_skills)",
  "- xiigen-smart-skills-prompt visible edit control: $($VisibleControls.textarea_or_input_id_xiigen_smart_skills_prompt)",
  "- xiigen-smart-skills-submit visible button: $($VisibleControls.submit_button_id_xiigen_smart_skills_submit)",
  '',
  '## Screenshot',
  "- captured: $ScreenshotCaptured",
  "- path: $(if ($ScreenshotCaptured) { ConvertTo-RelativeEvidencePath -Path $ScreenshotPath } else { '' })",
  '',
  '## What Was Proven',
  $Record.what_was_proven,
  '',
  '## What Is Not Proven',
  $Record.what_is_not_proven,
  '',
  '## Human Explanation',
  $Record.human_explanation,
  '',
  '## Next Smallest Action',
  $Record.next_smallest_action,
  ''
) -join "`n"
$Markdown | Set-Content -LiteralPath $MdPath -Encoding UTF8

'stop' | Set-Content -LiteralPath $StopPath -Encoding UTF8
$Record | ConvertTo-Json -Depth 14

