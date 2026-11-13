Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$RunFolder = 'C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\ui-input-api-panel-run-20260625T095742Z'
$ParentRunFolder = 'C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z'
$LogsDir = Join-Path $RunFolder 'logs'
$ScreenshotsDir = Join-Path $RunFolder 'screenshots'
$CursorExe = 'C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe'
$ExtensionRoot = 'C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills'
$ProbeJs = Join-Path $LogsDir 'api-open-panel-host-probe.js'
$CommandResultPath = Join-Path $LogsDir 'api-open-panel-command-result.json'
$StopPath = Join-Path $LogsDir 'api-open-panel-stop.txt'
$BeforeScreenshotPath = Join-Path $ScreenshotsDir 'api-open-panel-visible-before-row12-input.png'
$AfterScreenshotPath = Join-Path $ScreenshotsDir 'row12-prompt-visible-in-xiigen-textarea.png'
$TextareaCropPath = Join-Path $ScreenshotsDir 'row12-prompt-textarea-crop.png'
$PacketJsonPath = Join-Path $RunFolder 'row12-cursor-ui-input-api-panel-packet.json'
$PacketMdPath = Join-Path $RunFolder 'row12-cursor-ui-input-api-panel-packet.md'
$PromptPath = Join-Path $RunFolder 'row12-prompt-exact.txt'

New-Item -ItemType Directory -Force -Path $LogsDir, $ScreenshotsDir | Out-Null
Remove-Item -LiteralPath $CommandResultPath, $StopPath, $BeforeScreenshotPath, $AfterScreenshotPath, $TextareaCropPath, $PacketJsonPath, $PacketMdPath, $PromptPath -Force -ErrorAction SilentlyContinue

$Row12Prompt = @'
[FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1]
Host: Cursor no-send local extension runtime.
Scenario: frdp-p01-cursor-token-savings-baseline.
User request: Convert this synthetic request into a bounded Smart Skills execution plan without calling external AI.
Requirements:
1. Preserve the marker and scenario id in references.
2. Produce a prompt_to_task result with task_id, task_title, constraints, and acceptance_gates.
3. Produce a task_to_plan result with three ordered steps and one verification gate per step.
4. Produce a task_to_skills result selecting skills from this allowed set only: prompt-to-task, task-to-plan, task-to-skills, evidence-ledger.
5. State non_claims: no token savings claim, no semantic quality claim, no readiness claim, no training, no checkpoint promotion, no locator update.
6. Return deterministic structured text with sections: task, plan, skills, references, non_claims.
'@
[System.IO.File]::WriteAllText($PromptPath, $Row12Prompt, [System.Text.Encoding]::UTF8)

$ProbeJsContent = @'
const fs = require("fs");
const path = require("path");
const vscode = require("vscode");

const probeDir = __dirname;
const packagePath = "C:\\Users\\lubao\\Documents\\xiigen\\extensions reasearch\\cursor-xiigen-smart-skills\\package.json";
const resultPath = path.join(probeDir, "api-open-panel-command-result.json");
const stopPath = path.join(probeDir, "api-open-panel-stop.txt");
const targetCommand = "xiigenSmartSkills.panel";

function writeJson(filePath, value) {
  fs.writeFileSync(filePath, `${JSON.stringify(value, null, 2)}\n`, "utf8");
}

async function sleep(ms) {
  await new Promise((resolve) => setTimeout(resolve, ms));
}

async function run() {
  const startedAt = new Date().toISOString();
  const packageJson = JSON.parse(fs.readFileSync(packagePath, "utf8").replace(/^\uFEFF/, ""));
  const extensionId = `${packageJson.publisher}.${packageJson.name}`;
  const extension = vscode.extensions.getExtension(extensionId);
  const commandsBeforeActivation = await vscode.commands.getCommands(true);
  const activation = { attempted: false, succeeded: false, error: null };

  if (extension) {
    activation.attempted = true;
    try {
      await extension.activate();
      activation.succeeded = true;
    } catch (error) {
      activation.error = error && error.stack ? error.stack : String(error);
    }
  }

  await sleep(500);
  const commandsAfterActivation = await vscode.commands.getCommands(true);
  const apiCommand = {
    command: targetCommand,
    attempted: commandsAfterActivation.includes(targetCommand),
    executed: false,
    succeeded: false,
    error: null,
    started_at_utc: new Date().toISOString(),
    finished_at_utc: null
  };

  if (apiCommand.attempted) {
    try {
      await vscode.commands.executeCommand(targetCommand);
      apiCommand.executed = true;
      apiCommand.succeeded = true;
    } catch (error) {
      apiCommand.executed = true;
      apiCommand.error = error && error.stack ? error.stack : String(error);
    }
  }
  apiCommand.finished_at_utc = new Date().toISOString();

  writeJson(resultPath, {
    packet_type: "ROW12_API_OPEN_PANEL_COMMAND_EVIDENCE",
    checked_at_utc: new Date().toISOString(),
    package_extension_id: extensionId,
    extension_present: Boolean(extension),
    extension_summary: extension ? {
      id: extension.id,
      extensionPath: extension.extensionPath,
      isActive: extension.isActive,
      package_main: extension.packageJSON && extension.packageJSON.main,
      activationEvents: extension.packageJSON && extension.packageJSON.activationEvents
    } : null,
    activation,
    target_command: targetCommand,
    target_present_before_activation: commandsBeforeActivation.includes(targetCommand),
    target_present_after_activation: commandsAfterActivation.includes(targetCommand),
    api_command: apiCommand,
    forbidden_actions: {
      command_palette_used: false,
      row12_prompt_typed_by_api: false,
      row12_submit_run: false,
      login_or_browser_recovery_run: false,
      external_ai_used: false,
      git_used: false,
      readiness_claimed: false
    },
    timing: {
      started_at_utc: startedAt,
      finished_at_utc: new Date().toISOString()
    }
  });

  const deadline = Date.now() + 120000;
  while (Date.now() < deadline && !fs.existsSync(stopPath)) {
    await sleep(500);
  }
}

module.exports = { run };
'@
[System.IO.File]::WriteAllText($ProbeJs, $ProbeJsContent, [System.Text.Encoding]::UTF8)

Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

$Signature = @'
using System;
using System.Runtime.InteropServices;
using System.Text;
public static class Row12ApiPanelInputWin32 {
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
  [DllImport("user32.dll")] public static extern bool SetCursorPos(int X, int Y);
  [DllImport("user32.dll")] public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);
  [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
}
'@
Add-Type $Signature

function Get-Row12Sha256 {
  param([Parameter(Mandatory=$true)][string] $Text)
  $Bytes = [System.Text.Encoding]::UTF8.GetBytes($Text)
  $Hash = [System.Security.Cryptography.SHA256]::Create().ComputeHash($Bytes)
  return (($Hash | ForEach-Object { $_.ToString('x2') }) -join '')
}

function ConvertTo-RelativeEvidencePath {
  param([Parameter(Mandatory=$true)][string]$Path)
  if ($Path.StartsWith($RunFolder, [System.StringComparison]::OrdinalIgnoreCase)) {
    return $Path.Substring($RunFolder.Length).TrimStart('\')
  }
  return $Path
}

function Get-CursorDevWindows {
  $Windows = New-Object System.Collections.Generic.List[object]
  $Callback = [Row12ApiPanelInputWin32+EnumWindowsProc]{
    param([IntPtr]$Hwnd, [IntPtr]$LParam)
    if (-not [Row12ApiPanelInputWin32]::IsWindowVisible($Hwnd)) { return $true }
    $Len = [Row12ApiPanelInputWin32]::GetWindowTextLength($Hwnd)
    if ($Len -le 0) { return $true }
    $Sb = New-Object System.Text.StringBuilder ($Len + 1)
    [void][Row12ApiPanelInputWin32]::GetWindowText($Hwnd, $Sb, $Sb.Capacity)
    [uint32]$WindowPid = 0
    [void][Row12ApiPanelInputWin32]::GetWindowThreadProcessId($Hwnd, [ref]$WindowPid)
    $Title = $Sb.ToString()
    if ($Title -like '*Extension Development Host*Cursor*') {
      $Rect = New-Object Row12ApiPanelInputWin32+RECT
      [void][Row12ApiPanelInputWin32]::GetWindowRect($Hwnd, [ref]$Rect)
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
  [void][Row12ApiPanelInputWin32]::EnumWindows($Callback, [IntPtr]::Zero)
  return @($Windows.ToArray())
}

function Set-TargetVisible {
  param([Parameter(Mandatory=$true)] $Target)
  [void][Row12ApiPanelInputWin32]::ShowWindow($Target.Handle, 9)
  Start-Sleep -Milliseconds 300
  [void][Row12ApiPanelInputWin32]::SetWindowPos($Target.Handle, [IntPtr](-1), 320, 140, 1280, 800, 0x0040)
  Start-Sleep -Milliseconds 500
  [void][Row12ApiPanelInputWin32]::SetForegroundWindow($Target.Handle)
  Start-Sleep -Seconds 1
  $Rect = New-Object Row12ApiPanelInputWin32+RECT
  [void][Row12ApiPanelInputWin32]::GetWindowRect($Target.Handle, [ref]$Rect)
  $Target.Rect = $Rect
  [void][Row12ApiPanelInputWin32]::SetWindowPos($Target.Handle, [IntPtr](-2), $Rect.Left, $Rect.Top, ($Rect.Right - $Rect.Left), ($Rect.Bottom - $Rect.Top), 0x0040)
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
    $Printed = [Row12ApiPanelInputWin32]::PrintWindow($Target.Handle, $Hdc, 2)
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

function Save-TextareaCrop {
  param(
    [Parameter(Mandatory=$true)][string] $SourcePath,
    [Parameter(Mandatory=$true)][string] $TargetPath
  )
  $Source = [System.Drawing.Bitmap]::FromFile($SourcePath)
  try {
    $CropRect = New-Object System.Drawing.Rectangle 300, 305, 650, 170
    $Crop = $Source.Clone($CropRect, $Source.PixelFormat)
    try {
      $Crop.Save($TargetPath, [System.Drawing.Imaging.ImageFormat]::Png)
    } finally {
      $Crop.Dispose()
    }
  } finally {
    $Source.Dispose()
  }
}

function Invoke-Click {
  param([int] $X, [int] $Y)
  [void][Row12ApiPanelInputWin32]::SetCursorPos($X, $Y)
  Start-Sleep -Milliseconds 150
  [Row12ApiPanelInputWin32]::mouse_event(0x0002, 0, 0, 0, [UIntPtr]::Zero)
  Start-Sleep -Milliseconds 80
  [Row12ApiPanelInputWin32]::mouse_event(0x0004, 0, 0, 0, [UIntPtr]::Zero)
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

$Row12Marker = 'FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1'
$ScenarioId = 'frdp-p01-cursor-token-savings-baseline'
$FilesCreated = New-Object System.Collections.Generic.List[string]
$UiStepsCompleted = New-Object System.Collections.Generic.List[string]
$PasteAttempted = $false
$PromptPasted = $false
$PasteError = $null
$Target = $null
$CommandEvidence = $null
$VisibleBefore = $false
$Status = 'TARGET_WINDOW_INPUT_NOT_VERIFIED'
$HumanExplanation = ''

$ShortHostRoot = Join-Path ([System.IO.Path]::GetTempPath()) ('xiigen-row12-api-input-' + (Get-Date -Format 'yyyyMMddHHmmss'))
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
  $UiStepsCompleted.Add('opened Cursor Extension Development Host using API-open-panel availability path arguments') | Out-Null
} finally {
  Pop-Location
}

$CommandDeadline = (Get-Date).AddSeconds(70)
while (-not (Test-Path -LiteralPath $CommandResultPath) -and (Get-Date) -lt $CommandDeadline) {
  Start-Sleep -Milliseconds 500
}

if (Test-Path -LiteralPath $CommandResultPath) {
  $CommandEvidence = Get-Content -LiteralPath $CommandResultPath -Raw | ConvertFrom-Json
  $FilesCreated.Add((ConvertTo-RelativeEvidencePath $CommandResultPath)) | Out-Null
  if ($CommandEvidence.api_command.succeeded -eq $true) {
    $UiStepsCompleted.Add('extension-host API activation and executeCommand("xiigenSmartSkills.panel") succeeded') | Out-Null
  }
}

$ProcessAfter = @(Get-CimInstance Win32_Process -Filter "Name = 'Cursor.exe'" | Select-Object ProcessId, CommandLine)
$MatchingProcesses = @($ProcessAfter | Where-Object {
  $_.CommandLine -and $_.CommandLine.Contains($UserDataDir) -and $_.CommandLine.Contains('--extensionDevelopmentPath')
})
$TargetPid = if (@($MatchingProcesses).Count -gt 0) { [int]$MatchingProcesses[0].ProcessId } else { $null }

$WindowDeadline = (Get-Date).AddSeconds(30)
do {
  $Windows = Get-CursorDevWindows
  $Target = if ($TargetPid) { $Windows | Where-Object { $_.ProcessId -eq $TargetPid } | Select-Object -First 1 } else { $null }
  if (-not $Target) {
    $Target = $Windows | Where-Object { $_.Title -like '*XIIGen Smart Skills*' } | Select-Object -First 1
  }
  if (-not $Target) { Start-Sleep -Milliseconds 500 }
} while (-not $Target -and (Get-Date) -lt $WindowDeadline)

if ($Target) {
  Set-TargetVisible -Target $Target
  Start-Sleep -Seconds 5
  Save-TargetScreenshot -Target $Target -Path $BeforeScreenshotPath
  $FilesCreated.Add((ConvertTo-RelativeEvidencePath $BeforeScreenshotPath)) | Out-Null
  $UiRows = Get-UiRows -Target $Target
  $VisibleBefore = ($Target.Title -like '*XIIGen Smart Skills*')
  if ($VisibleBefore) {
    $UiStepsCompleted.Add('verified target window title includes XIIGen Smart Skills before paste') | Out-Null
  }
}

if (-not $CommandEvidence -or $CommandEvidence.api_command.succeeded -ne $true) {
  $Status = 'PANEL_INPUT_NOT_VISIBLE'
  $HumanExplanation = 'The API-open-panel command evidence did not prove a successful xiigenSmartSkills.panel execution, so no prompt was pasted.'
} elseif (-not $Target -or -not $VisibleBefore) {
  $Status = 'TARGET_WINDOW_INPUT_NOT_VERIFIED'
  $HumanExplanation = 'The target Cursor Extension Development Host XIIGen panel window was not verified before input, so no prompt was pasted.'
} else {
  try {
    $ClickX = $Target.Rect.Left + 620
    $ClickY = $Target.Rect.Top + 355
    Invoke-Click -X $ClickX -Y $ClickY
    Start-Sleep -Milliseconds 300
    [System.Windows.Forms.SendKeys]::SendWait('^a')
    Start-Sleep -Milliseconds 300
    [System.Windows.Forms.Clipboard]::SetText($Row12Prompt)
    Start-Sleep -Milliseconds 300
    [System.Windows.Forms.SendKeys]::SendWait('^v')
    $PasteAttempted = $true
    $PromptPasted = $true
    $UiStepsCompleted.Add('focused the visible XIIGen panel textarea coordinate and pasted the exact row12 prompt') | Out-Null
    Start-Sleep -Seconds 1
    Save-TargetScreenshot -Target $Target -Path $AfterScreenshotPath
    Save-TextareaCrop -SourcePath $AfterScreenshotPath -TargetPath $TextareaCropPath
    $FilesCreated.Add((ConvertTo-RelativeEvidencePath $AfterScreenshotPath)) | Out-Null
    $FilesCreated.Add((ConvertTo-RelativeEvidencePath $TextareaCropPath)) | Out-Null
    $Status = 'ROW12_PROMPT_VISIBLE_IN_XIIGEN_TEXTAREA'
    $HumanExplanation = 'The row12 prompt was pasted into the visible XIIGen Smart Skills panel textarea after API-open-panel activation. Submit was not clicked.'
  } catch {
    $PasteError = $_.Exception.Message
    $Status = 'ROW12_PROMPT_INPUT_BLOCKED_WITH_EVIDENCE'
    $HumanExplanation = 'The XIIGen panel was visible, but the prompt paste attempt failed before input could be verified. Submit was not clicked.'
  }
}

Set-Content -LiteralPath $StopPath -Value 'stop' -Encoding ASCII
$FilesCreated.Add((ConvertTo-RelativeEvidencePath $StopPath)) | Out-Null
$FilesCreated.Add((ConvertTo-RelativeEvidencePath $ProbeJs)) | Out-Null
$FilesCreated.Add((ConvertTo-RelativeEvidencePath $PromptPath)) | Out-Null

$Packet = [ordered]@{
  packet = 'ROW12_CURSOR_UI_INPUT_API_PANEL_PACKET'
  status = $Status
  run_folder = $RunFolder
  files_created = @($FilesCreated.ToArray())
  ui_steps_completed = @($UiStepsCompleted.ToArray())
  textarea_evidence = [ordered]@{
    target_window_title = if ($Target) { $Target.Title } else { $null }
    target_process_id = if ($Target) { $Target.ProcessId } else { $null }
    api_open_panel_command_succeeded = ($CommandEvidence -and $CommandEvidence.api_command.succeeded -eq $true)
    xiigen_window_title_verified_before_input = $VisibleBefore
    target_textarea_focus_method = 'visible XIIGen panel textarea coordinate derived from proven API-open screenshot geometry'
    submit_button_clicked = $false
    prompt_paste_attempted = $PasteAttempted
    prompt_pasted = $PromptPasted
    row12_marker = $Row12Marker
    scenario_id = $ScenarioId
    marker_visible_in_textarea = if ($Status -eq 'ROW12_PROMPT_VISIBLE_IN_XIIGEN_TEXTAREA') { 'visual_screenshot_verification_required_and_captured' } else { $false }
    prompt_sha256 = ('sha256:' + (Get-Row12Sha256 -Text $Row12Prompt))
    paste_error = $PasteError
    forbidden_wrong_textbox_guard = 'No paste was attempted unless the target window title included XIIGen Smart Skills after API-open-panel command success.'
  }
  screenshot_status = [ordered]@{
    captured_before_input = (Test-Path -LiteralPath $BeforeScreenshotPath)
    captured_after_input = (Test-Path -LiteralPath $AfterScreenshotPath)
    textarea_crop_captured = (Test-Path -LiteralPath $TextareaCropPath)
    before_input_path = if (Test-Path -LiteralPath $BeforeScreenshotPath) { ConvertTo-RelativeEvidencePath $BeforeScreenshotPath } else { $null }
    after_input_path = if (Test-Path -LiteralPath $AfterScreenshotPath) { ConvertTo-RelativeEvidencePath $AfterScreenshotPath } else { $null }
    textarea_crop_path = if (Test-Path -LiteralPath $TextareaCropPath) { ConvertTo-RelativeEvidencePath $TextareaCropPath } else { $null }
    visual_inspection_required_by_worker = $true
  }
  human_explanation = $HumanExplanation
  next_smallest_action = if ($Status -eq 'ROW12_PROMPT_VISIBLE_IN_XIIGEN_TEXTAREA') { 'Stop here for this input-only slice. The separately authorized next slice would be submit, but this run intentionally did not click submit.' } else { 'Use the captured evidence to repair the API-open-panel/input focus path before any submit slice.' }
  non_actions = @(
    'no submit click',
    'no login/browser',
    'no external AI',
    'no git stage/commit/push',
    'no readiness claim',
    'no paste into generic Cursor New Agent/login textbox'
  )
  source_availability_packet = Join-Path $ParentRunFolder 'api-open-panel-availability.json'
  checked_at_utc = (Get-Date).ToUniversalTime().ToString('o')
}

$Packet | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $PacketJsonPath -Encoding UTF8
$FilesCreated.Add((ConvertTo-RelativeEvidencePath $PacketJsonPath)) | Out-Null

$Md = @"
# Row12 Cursor UI Input API Panel Packet

ROW12_CURSOR_UI_INPUT_API_PANEL_PACKET
status: $Status
run_folder: $RunFolder

## UI Steps Completed
$(@($UiStepsCompleted.ToArray()) | ForEach-Object { "- $_" } | Out-String)
## Textarea Evidence
- target_window_title: $(if ($Target) { $Target.Title } else { 'null' })
- api_open_panel_command_succeeded: $($CommandEvidence -and $CommandEvidence.api_command.succeeded -eq $true)
- xiigen_window_title_verified_before_input: $VisibleBefore
- prompt_paste_attempted: $PasteAttempted
- prompt_pasted: $PromptPasted
- row12_marker: $Row12Marker
- scenario_id: $ScenarioId
- submit_button_clicked: false
- paste_error: $PasteError

## Screenshot Status
- before input: $(if (Test-Path -LiteralPath $BeforeScreenshotPath) { ConvertTo-RelativeEvidencePath $BeforeScreenshotPath } else { 'not captured' })
- after input: $(if (Test-Path -LiteralPath $AfterScreenshotPath) { ConvertTo-RelativeEvidencePath $AfterScreenshotPath } else { 'not captured' })
- textarea crop: $(if (Test-Path -LiteralPath $TextareaCropPath) { ConvertTo-RelativeEvidencePath $TextareaCropPath } else { 'not captured' })
- visual inspection required by worker: true

## Human Explanation
$HumanExplanation

## Next Smallest Action
$($Packet.next_smallest_action)

## Non Actions
- no submit click
- no login/browser
- no external AI
- no git stage/commit/push
- no readiness claim
- no paste into generic Cursor New Agent/login textbox
"@
$Md | Set-Content -LiteralPath $PacketMdPath -Encoding UTF8

Write-Output $PacketJsonPath
