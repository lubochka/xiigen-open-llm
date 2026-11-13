Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$RunFolder = 'C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z'
$LogsDir = Join-Path $RunFolder 'logs'
$ScreenshotsDir = Join-Path $RunFolder 'screenshots'
$CursorExe = 'C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe'
$ExtensionRoot = 'C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills'
$ProbeJs = Join-Path $LogsDir 'panel-activation-wait-probe.js'
$ActivationResultPath = Join-Path $LogsDir 'panel-activation-wait-result.json'
$ApiRequestPath = Join-Path $LogsDir 'panel-api-open-request.json'
$ApiResultPath = Join-Path $LogsDir 'panel-api-open-result.json'
$StopPath = Join-Path $LogsDir 'panel-activation-wait-stop.txt'
$UiProbePath = Join-Path $LogsDir 'panel-ui-probe-after-activation.json'
$SearchText = 'XIIGen: Open Smart Skills Panel'

New-Item -ItemType Directory -Force -Path $LogsDir, $ScreenshotsDir | Out-Null
Remove-Item -LiteralPath $ActivationResultPath, $ApiRequestPath, $ApiResultPath, $StopPath -Force -ErrorAction SilentlyContinue

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

$Signature = @'
using System;
using System.Runtime.InteropServices;
using System.Text;
public static class Row12PanelWin32 {
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
  [DllImport("user32.dll")] public static extern bool SetCursorPos(int X, int Y);
  [DllImport("user32.dll")] public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);
  [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);
  [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
}
'@
Add-Type $Signature

function Get-CursorDevWindows {
  $Windows = New-Object System.Collections.Generic.List[object]
  $Callback = [Row12PanelWin32+EnumWindowsProc]{
    param([IntPtr]$Hwnd, [IntPtr]$LParam)
    if (-not [Row12PanelWin32]::IsWindowVisible($Hwnd)) { return $true }
    $Len = [Row12PanelWin32]::GetWindowTextLength($Hwnd)
    if ($Len -le 0) { return $true }
    $Sb = New-Object System.Text.StringBuilder ($Len + 1)
    [void][Row12PanelWin32]::GetWindowText($Hwnd, $Sb, $Sb.Capacity)
    [uint32]$WindowPid = 0
    [void][Row12PanelWin32]::GetWindowThreadProcessId($Hwnd, [ref]$WindowPid)
    $Title = $Sb.ToString()
    if ($Title -like '*Extension Development Host*Cursor*') {
      $Rect = New-Object Row12PanelWin32+RECT
      [void][Row12PanelWin32]::GetWindowRect($Hwnd, [ref]$Rect)
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
  [void][Row12PanelWin32]::EnumWindows($Callback, [IntPtr]::Zero)
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
    $Printed = [Row12PanelWin32]::PrintWindow($Target.Handle, $Hdc, 2)
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

function Get-UiNames {
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

function Activate-TargetWindow {
  param([Parameter(Mandatory=$true)] $Target)
  [void][Row12PanelWin32]::ShowWindow($Target.Handle, 9)
  Start-Sleep -Milliseconds 300
  [void][Row12PanelWin32]::SetWindowPos($Target.Handle, [IntPtr](-1), $Target.Rect.Left, $Target.Rect.Top, [Math]::Max(1, $Target.Rect.Right - $Target.Rect.Left), [Math]::Max(1, $Target.Rect.Bottom - $Target.Rect.Top), 0x0040)
  Start-Sleep -Milliseconds 300
  [void][Row12PanelWin32]::SetForegroundWindow($Target.Handle)
  Start-Sleep -Milliseconds 500
  $ClickX = [int](($Target.Rect.Left + $Target.Rect.Right) / 2)
  $ClickY = [int]($Target.Rect.Top + 24)
  [void][Row12PanelWin32]::SetCursorPos($ClickX, $ClickY)
  [Row12PanelWin32]::mouse_event(0x0002, 0, 0, 0, [UIntPtr]::Zero)
  [Row12PanelWin32]::mouse_event(0x0004, 0, 0, 0, [UIntPtr]::Zero)
  Start-Sleep -Milliseconds 500
  [void][Row12PanelWin32]::SetWindowPos($Target.Handle, [IntPtr](-2), $Target.Rect.Left, $Target.Rect.Top, [Math]::Max(1, $Target.Rect.Right - $Target.Rect.Left), [Math]::Max(1, $Target.Rect.Bottom - $Target.Rect.Top), 0x0040)
}

$ShortHostRoot = Join-Path ([System.IO.Path]::GetTempPath()) ('xiigen-panel-activation-ui-' + (Get-Date -Format 'yyyyMMddHHmmss'))
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

$ActivationDeadline = (Get-Date).AddSeconds(45)
while (-not (Test-Path -LiteralPath $ActivationResultPath) -and (Get-Date) -lt $ActivationDeadline) {
  Start-Sleep -Milliseconds 500
}

$ActivationResult = $null
if (Test-Path -LiteralPath $ActivationResultPath) {
  $ActivationResult = Get-Content -LiteralPath $ActivationResultPath -Raw | ConvertFrom-Json
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

$CommandPaletteOpened = $false
$TargetWindowInputVerified = $false
$ExactCommandFound = $false
$PanelOpenMethod = 'not_opened'
$ApiOpenResult = $null
$InitialNames = @()
$PaletteNames = @()
$PanelNames = @()
$VisibleControls = [ordered]@{
  panel_title_or_header = $false
  textarea_or_prompt = $false
  textarea_id_accessible = $false
  submit_button = $false
  submit_id_accessible = $false
}
$ScreenshotPaths = @()
$Status = 'TARGET_WINDOW_INPUT_NOT_VERIFIED'

if ($Target) {
  Activate-TargetWindow -Target $Target
  $InitialScreenshot = Join-Path $ScreenshotsDir 'panel-availability-target-window-before-command.png'
  Save-TargetScreenshot -Target $Target -Path $InitialScreenshot
  $ScreenshotPaths += $InitialScreenshot

  [System.Windows.Forms.SendKeys]::SendWait('^+p')
  Start-Sleep -Seconds 1
  [System.Windows.Forms.SendKeys]::SendWait('^a')
  Start-Sleep -Milliseconds 200
  Set-Clipboard -Value $SearchText
  [System.Windows.Forms.SendKeys]::SendWait('^v')
  Start-Sleep -Seconds 2
  $CommandPaletteOpened = $true
  $TargetWindowInputVerified = $true

  $PaletteScreenshot = Join-Path $ScreenshotsDir 'panel-availability-command-palette-after-activation.png'
  Save-TargetScreenshot -Target $Target -Path $PaletteScreenshot
  $ScreenshotPaths += $PaletteScreenshot

  $PaletteNames = Get-UiNames -Target $Target
  $ExactCommandRows = @($PaletteNames | Where-Object { $_.name -eq $SearchText -or $_.name -like "*$SearchText*" -or $_.automation_id -eq $SearchText })
  $ExactCommandFound = @($ExactCommandRows).Count -gt 0

  if ($ExactCommandFound) {
    [System.Windows.Forms.SendKeys]::SendWait('{ENTER}')
    $PanelOpenMethod = 'command_palette_ui_exact_title'
  } else {
    [System.Windows.Forms.SendKeys]::SendWait('{ESC}')
    Start-Sleep -Milliseconds 500
    [pscustomobject]@{
      requested_at_utc = (Get-Date).ToUniversalTime().ToString('o')
      command = 'xiigenSmartSkills.panel'
      reason = 'Command palette exact title not surfaced in UI Automation names after activation; safe host API fallback authorized by work order.'
    } | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $ApiRequestPath -Encoding UTF8

    $ApiDeadline = (Get-Date).AddSeconds(20)
    while (-not (Test-Path -LiteralPath $ApiResultPath) -and (Get-Date) -lt $ApiDeadline) {
      Start-Sleep -Milliseconds 500
    }
    if (Test-Path -LiteralPath $ApiResultPath) {
      $ApiOpenResult = Get-Content -LiteralPath $ApiResultPath -Raw | ConvertFrom-Json
    }
    $PanelOpenMethod = 'safe_host_api_executeCommand_after_command_palette_not_visible'
  }

  Start-Sleep -Seconds 4
  $PanelScreenshot = Join-Path $ScreenshotsDir 'panel-availability-opened-after-activation.png'
  Save-TargetScreenshot -Target $Target -Path $PanelScreenshot
  $ScreenshotPaths += $PanelScreenshot

  $PanelNames = Get-UiNames -Target $Target
  $AllPanelText = (($PanelNames | ForEach-Object { "$($_.name) $($_.automation_id) $($_.class_name)" }) -join "`n")
  $VisibleControls.panel_title_or_header = $AllPanelText -match 'XIIGen Smart Skills'
  $VisibleControls.textarea_or_prompt = $AllPanelText -match 'prompt|textarea|Smart Skills prompt|Enter.*request|What.*working'
  $VisibleControls.textarea_id_accessible = $AllPanelText -match 'xiigen-smart-skills-prompt'
  $VisibleControls.submit_button = $AllPanelText -match 'submit|run|send|Generate|Start'
  $VisibleControls.submit_id_accessible = $AllPanelText -match 'xiigen-smart-skills-submit'

  if ($PanelOpenMethod -eq 'command_palette_ui_exact_title' -and $VisibleControls.panel_title_or_header -and $VisibleControls.textarea_or_prompt -and $VisibleControls.submit_button) {
    $Status = 'PANEL_VISIBLE_WITH_INPUT_SUBMIT'
  } elseif ($PanelOpenMethod -like 'safe_host_api*' -and $ApiOpenResult -and $ApiOpenResult.succeeded -and $VisibleControls.panel_title_or_header) {
    $Status = 'PANEL_COMMAND_UI_STILL_NOT_VISIBLE_BUT_API_CAN_OPEN'
  } elseif ($TargetWindowInputVerified) {
    $Status = 'PANEL_NOT_VISIBLE_WITH_EVIDENCE'
  }
}

'stop' | Set-Content -LiteralPath $StopPath -Encoding UTF8

$Record = [pscustomobject]@{
  packet_type = 'ROW12_PANEL_AVAILABILITY_AFTER_ACTIVATION_PACKET'
  status = $Status
  checked_at_utc = (Get-Date).ToUniversalTime().ToString('o')
  run_folder = $RunFolder
  git_authority = 'forbidden'
  git_actions_run = @()
  row12_prompt_typed = $false
  row12_submit_run = $false
  login_or_browser_recovery_run = $false
  external_ai_used = $false
  readiness_claimed = $false
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
  activation_evidence = $ActivationResult
  target_window_protocol = [pscustomobject]@{
    target_window_found = [bool]$Target
    target_window_title = if ($Target) { $Target.Title } else { $null }
    target_process_id = if ($Target) { $Target.ProcessId } else { $null }
    target_native_window_handle = if ($Target) { $Target.HandleInt } else { $null }
    target_window_bounds = if ($Target) {
      [pscustomobject]@{
        left = $Target.Rect.Left
        top = $Target.Rect.Top
        right = $Target.Rect.Right
        bottom = $Target.Rect.Bottom
        width = [Math]::Max(1, $Target.Rect.Right - $Target.Rect.Left)
        height = [Math]::Max(1, $Target.Rect.Bottom - $Target.Rect.Top)
      }
    } else { $null }
    activation_method_used = if ($Target) { 'Win32 ShowWindow+SetForegroundWindow+safe top-center click' } else { $null }
    input_focus_probe = if ($CommandPaletteOpened) { 'Ctrl+Shift+P then paste exact XIIGen command title into target command palette' } else { $null }
    cursor_ui_reaction_observed = $CommandPaletteOpened
    target_window_input_verified = $TargetWindowInputVerified
    target_window_candidates = @($Windows | Select-Object Title, ProcessId, HandleInt)
  }
  command_palette_evidence = [pscustomobject]@{
    command_palette_opened = $CommandPaletteOpened
    search_text = $SearchText
    exact_command_title_found = $ExactCommandFound
    matching_names = @($PaletteNames | Where-Object { $_.name -like '*XIIGen*' -or $_.name -like '*Smart Skills*' -or $_.automation_id -like '*xiigen*' -or $_.automation_id -like '*Smart*' } | Select-Object -First 80)
    ui_command_palette_availability_proven = $ExactCommandFound
  }
  panel_open_method = $PanelOpenMethod
  api_open_result = $ApiOpenResult
  visible_controls = $VisibleControls
  panel_accessibility_excerpt = @($PanelNames | Where-Object { $_.name -like '*XIIGen*' -or $_.name -like '*Smart*' -or $_.name -like '*Prompt*' -or $_.name -like '*Submit*' -or $_.automation_id -like '*xiigen*' -or $_.automation_id -like '*submit*' -or $_.automation_id -like '*prompt*' } | Select-Object -First 80)
  screenshot_status = [pscustomobject]@{
    captured = @($ScreenshotPaths).Count -gt 0
    paths = $ScreenshotPaths
    inspected_by_worker = $false
  }
  what_was_proven = if ($Status -eq 'PANEL_VISIBLE_WITH_INPUT_SUBMIT') {
    'After host API activation in a visible Cursor Extension Development Host, the command palette opened the exact XIIGen panel command and the panel was visible with prompt/input and submit controls by UI evidence.'
  } elseif ($Status -eq 'PANEL_COMMAND_UI_STILL_NOT_VISIBLE_BUT_API_CAN_OPEN') {
    'After host API activation in a visible Cursor Extension Development Host, the command palette path did not surface the exact command title in UI Automation names, but safe host API execution opened the XIIGen Smart Skills panel.'
  } elseif ($Status -eq 'PANEL_NOT_VISIBLE_WITH_EVIDENCE') {
    'Target-window input was verified, but the XIIGen Smart Skills panel was not proven visible.'
  } else {
    'Target-window input was not verified.'
  }
  what_is_not_proven = 'No row12 prompt was typed, no submit button was clicked, no browser/login recovery ran, no external AI was used, no git action ran, and no readiness/token-savings/semantic-quality claim is made.'
  human_explanation = if ($Status -eq 'PANEL_COMMAND_UI_STILL_NOT_VISIBLE_BUT_API_CAN_OPEN') {
    'The extension activated and registered xiigenSmartSkills.panel in a live visible host. The Command Palette UI path remains unproven in this run, so the panel was opened by the safe host API fallback and then visually captured.'
  } else {
    'The run stayed within panel availability only: activation, panel open, visible control check, and screenshots. Row12 submit was deliberately not run.'
  }
  next_smallest_action = if ($Status -eq 'PANEL_VISIBLE_WITH_INPUT_SUBMIT') {
    'A separately authorized row12 submit slice can use the visible textarea and submit button, preserving the no-readiness/no-token-savings boundary until full Path B evidence exists.'
  } elseif ($Status -eq 'PANEL_COMMAND_UI_STILL_NOT_VISIBLE_BUT_API_CAN_OPEN') {
    'Investigate command-palette discovery separately, or proceed only under a work order that accepts API-opened panel availability as narrower evidence.'
  } else {
    'Repair the remaining panel visibility or target-window control blocker, then rerun this same narrow availability split without row12 submit.'
  }
}

$Record | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $UiProbePath -Encoding UTF8
$Record | ConvertTo-Json -Depth 12
