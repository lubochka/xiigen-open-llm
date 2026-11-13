param(
  [string]$OutputDir = "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\cursor-ext-path-b-same-row23-visible-ui-route-01",
  [string]$Row23InputPath = "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\row23-input-path\row23-approved-safe-scenario-input.json"
)

$ErrorActionPreference = "Stop"
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$hostEvidenceDir = Join-Path $OutputDir "host-generated-copies"
New-Item -ItemType Directory -Force -Path $hostEvidenceDir | Out-Null

Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName UIAutomationClient
Add-Type @"
using System;
using System.Runtime.InteropServices;

public static class XiigenCursorSubmitNative {
  [StructLayout(LayoutKind.Sequential)]
  public struct RECT {
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;
  }

  [DllImport("user32.dll")]
  public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

  [DllImport("user32.dll")]
  public static extern bool IsWindowVisible(IntPtr hWnd);

  [DllImport("user32.dll")]
  public static extern bool IsIconic(IntPtr hWnd);

  [DllImport("user32.dll")]
  public static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);

  [DllImport("user32.dll")]
  public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

  [DllImport("user32.dll")]
  public static extern bool SetForegroundWindow(IntPtr hWnd);
}
"@

function Get-WindowRectObject {
  param([IntPtr]$Handle)
  $rect = New-Object XiigenCursorSubmitNative+RECT
  $ok = [XiigenCursorSubmitNative]::GetWindowRect($Handle, [ref]$rect)
  if (-not $ok) { return $null }
  [pscustomobject]@{
    left = $rect.Left
    top = $rect.Top
    right = $rect.Right
    bottom = $rect.Bottom
    width = $rect.Right - $rect.Left
    height = $rect.Bottom - $rect.Top
  }
}

function Convert-BoundingRect {
  param($Rect)
  if ($null -eq $Rect) { return $null }
  [pscustomobject]@{
    left = [Math]::Round($Rect.Left, 0)
    top = [Math]::Round($Rect.Top, 0)
    right = [Math]::Round($Rect.Right, 0)
    bottom = [Math]::Round($Rect.Bottom, 0)
    width = [Math]::Round($Rect.Width, 0)
    height = [Math]::Round($Rect.Height, 0)
  }
}

function Get-ImageStats {
  param([System.Drawing.Bitmap]$Bitmap)
  $sampleStepX = [Math]::Max(1, [Math]::Floor($Bitmap.Width / 80))
  $sampleStepY = [Math]::Max(1, [Math]::Floor($Bitmap.Height / 80))
  $count = 0
  $sum = 0.0
  $sumSq = 0.0
  $nonWhite = 0
  $nonBlack = 0
  for ($x = 0; $x -lt $Bitmap.Width; $x += $sampleStepX) {
    for ($y = 0; $y -lt $Bitmap.Height; $y += $sampleStepY) {
      $c = $Bitmap.GetPixel($x, $y)
      $brightness = ($c.R + $c.G + $c.B) / 3.0
      $sum += $brightness
      $sumSq += ($brightness * $brightness)
      if ($brightness -lt 245) { $nonWhite++ }
      if ($brightness -gt 10) { $nonBlack++ }
      $count++
    }
  }
  $mean = if ($count -gt 0) { $sum / $count } else { 0 }
  $variance = if ($count -gt 0) { ($sumSq / $count) - ($mean * $mean) } else { 0 }
  [pscustomobject]@{
    sampled_pixels = $count
    mean_brightness = [Math]::Round($mean, 2)
    brightness_variance = [Math]::Round($variance, 2)
    non_white_samples = $nonWhite
    non_black_samples = $nonBlack
  }
}

function Capture-TargetWindow {
  param(
    [object]$Target,
    [string]$Path,
    [string]$Phase
  )
  $bitmap = New-Object System.Drawing.Bitmap($Target.rect.width, $Target.rect.height)
  $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
  $hdc = $graphics.GetHdc()
  $printOk = $false
  try {
    $printOk = [XiigenCursorSubmitNative]::PrintWindow([IntPtr]$Target.handle, $hdc, 2)
  }
  finally {
    $graphics.ReleaseHdc($hdc)
    $graphics.Dispose()
  }
  $bitmap.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png)
  $stats = Get-ImageStats -Bitmap $bitmap
  $bitmap.Dispose()
  [pscustomobject]@{
    schema = "xiigen.cursor_visible_route_screenshot.v1"
    generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
    phase = $Phase
    screenshot_path = $Path
    target = $Target
    print_window_returned = $printOk
    image_stats = $stats
    blank_or_failed = (-not $printOk) -or ($stats.non_black_samples -eq 0) -or ($stats.brightness_variance -lt 1)
  }
}

function Find-DescendantByAutomationId {
  param(
    [System.Windows.Automation.AutomationElement]$Root,
    [string]$AutomationId
  )
  $condition = New-Object System.Windows.Automation.PropertyCondition(
    [System.Windows.Automation.AutomationElement]::AutomationIdProperty,
    $AutomationId
  )
  $Root.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
}

$row23 = Get-Content -Raw -LiteralPath $Row23InputPath | ConvertFrom-Json
$prompt = [string]$row23.scenario_input.prompt_text
$globalStorageDir = "C:\Users\lubao\AppData\Roaming\Cursor\User\globalStorage\xiigen.cursor-xiigen-smart-skills\xiigen-cursor-host-runtime"
$beforeUtc = (Get-Date).ToUniversalTime()
$beforeFiles = @()
if (Test-Path -LiteralPath $globalStorageDir) {
  $beforeFiles = @(Get-ChildItem -File -LiteralPath $globalStorageDir | Select-Object FullName, LastWriteTimeUtc, Length)
}

$windows = Get-Process |
  Where-Object { $_.MainWindowHandle -ne 0 -and $_.MainWindowTitle } |
  Sort-Object ProcessName, Id |
  ForEach-Object {
    $handle = [IntPtr]$_.MainWindowHandle
    [pscustomobject]@{
      id = $_.Id
      process_name = $_.ProcessName
      title = $_.MainWindowTitle
      handle = $_.MainWindowHandle
      path = $_.Path
      visible = [XiigenCursorSubmitNative]::IsWindowVisible($handle)
      minimized = [XiigenCursorSubmitNative]::IsIconic($handle)
      rect = Get-WindowRectObject -Handle $handle
    }
  }
$windows | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $OutputDir "submission-window-inventory-before.json") -Encoding UTF8

$target = $windows |
  Where-Object {
    $_.process_name -eq "Cursor" -and
    $_.title -like "*[Extension Development Host]*" -and
    $_.title -like "*XIIGen Smart Skills*"
  } |
  Select-Object -First 1

$decision = [ordered]@{
  schema = "xiigen.cursor_same_row23_visible_ui_route_decision.v1"
  generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
  row23_input_path = $Row23InputPath
  prompt_id = [string]$row23.scenario_input.prompt_id
  target_found = $null -ne $target
  target = $target
  method = "UIAutomation ValuePattern on xiigen-prompt plus InvokePattern on xiigen-submit"
  coordinate_typing_used = $false
  keyboard_sendkeys_used = $false
  clipboard_used = $false
  external_ai_chat_submission = "not performed"
  status = "BLOCKED"
  reason = ""
}

if ($null -eq $target) {
  $decision.reason = "No Cursor Extension Development Host window titled XIIGen Smart Skills was found."
  $decision | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $OutputDir "same-row23-visible-ui-route-decision.json") -Encoding UTF8
  Write-Output "BLOCKED: target Cursor Extension Development Host window not found."
  exit 20
}

if ($target.minimized -or -not $target.visible) {
  [void][XiigenCursorSubmitNative]::ShowWindow([IntPtr]$target.handle, 9)
  Start-Sleep -Milliseconds 750
  [void][XiigenCursorSubmitNative]::SetForegroundWindow([IntPtr]$target.handle)
  Start-Sleep -Milliseconds 750
}

$captureBefore = Capture-TargetWindow -Target $target -Path (Join-Path $OutputDir "cursor-window-before-row23-submit.png") -Phase "before-row23-submit"
$captureBefore | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $OutputDir "screenshot-before-row23-submit.json") -Encoding UTF8
if ($captureBefore.blank_or_failed) {
  $decision.reason = "Single-window screenshot before submit was blank or failed."
  $decision | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $OutputDir "same-row23-visible-ui-route-decision.json") -Encoding UTF8
  Write-Output "BLOCKED: before-submit screenshot failed."
  exit 21
}

$root = [System.Windows.Automation.AutomationElement]::FromHandle([IntPtr]$target.handle)
if ($null -eq $root) {
  $decision.reason = "UI Automation root was unavailable for the Cursor window."
  $decision | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $OutputDir "same-row23-visible-ui-route-decision.json") -Encoding UTF8
  Write-Output "BLOCKED: UI Automation root unavailable."
  exit 30
}

$promptElement = Find-DescendantByAutomationId -Root $root -AutomationId "xiigen-prompt"
$submitElement = Find-DescendantByAutomationId -Root $root -AutomationId "xiigen-submit"
$chatElement = $null
$chatCondition = New-Object System.Windows.Automation.PropertyCondition(
  [System.Windows.Automation.AutomationElement]::ClassNameProperty,
  "aislash-editor-input"
)
$chatElement = $root.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $chatCondition)

$promptRect = if ($null -ne $promptElement) { Convert-BoundingRect -Rect $promptElement.Current.BoundingRectangle } else { $null }
$submitRect = if ($null -ne $submitElement) { Convert-BoundingRect -Rect $submitElement.Current.BoundingRectangle } else { $null }
$chatRect = if ($null -ne $chatElement) { Convert-BoundingRect -Rect $chatElement.Current.BoundingRectangle } else { $null }

$decision.prompt_control = if ($null -ne $promptElement) {
  [ordered]@{
    name = $promptElement.Current.Name
    automation_id = $promptElement.Current.AutomationId
    control_type = $promptElement.Current.ControlType.ProgrammaticName
    is_enabled = $promptElement.Current.IsEnabled
    is_keyboard_focusable = $promptElement.Current.IsKeyboardFocusable
    bounding_rect = $promptRect
  }
} else { $null }
$decision.submit_control = if ($null -ne $submitElement) {
  [ordered]@{
    name = $submitElement.Current.Name
    automation_id = $submitElement.Current.AutomationId
    control_type = $submitElement.Current.ControlType.ProgrammaticName
    is_enabled = $submitElement.Current.IsEnabled
    is_keyboard_focusable = $submitElement.Current.IsKeyboardFocusable
    bounding_rect = $submitRect
  }
} else { $null }
$decision.external_ai_chat_control = if ($null -ne $chatElement) {
  [ordered]@{
    name = $chatElement.Current.Name
    automation_id = $chatElement.Current.AutomationId
    class_name = $chatElement.Current.ClassName
    control_type = $chatElement.Current.ControlType.ProgrammaticName
    bounding_rect = $chatRect
  }
} else { $null }

if ($null -eq $promptElement -or $null -eq $submitElement) {
  $decision.reason = "Named local panel controls xiigen-prompt and xiigen-submit were not both exposed through UI Automation."
  $decision | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $OutputDir "same-row23-visible-ui-route-decision.json") -Encoding UTF8
  Write-Output "BLOCKED: named local panel controls unavailable."
  exit 31
}

if ($null -eq $promptRect -or $null -eq $submitRect -or $promptRect.width -lt 100 -or $submitRect.width -lt 20) {
  $decision.reason = "Named local panel controls had invalid bounding rectangles."
  $decision | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $OutputDir "same-row23-visible-ui-route-decision.json") -Encoding UTF8
  Write-Output "BLOCKED: local panel control geometry invalid."
  exit 32
}

if ($null -ne $chatRect -and $promptRect.left -ge $chatRect.left) {
  $decision.reason = "Prompt control geometry overlaps or starts inside the external AI chat region."
  $decision | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $OutputDir "same-row23-visible-ui-route-decision.json") -Encoding UTF8
  Write-Output "BLOCKED: local prompt geometry not separate from external AI chat."
  exit 33
}

$valuePattern = $null
try {
  $valuePattern = $promptElement.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)
}
catch {
  $valuePattern = $null
}
if ($null -eq $valuePattern) {
  $decision.reason = "xiigen-prompt did not expose ValuePattern; keyboard or clipboard fallback is forbidden for this worker."
  $decision | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $OutputDir "same-row23-visible-ui-route-decision.json") -Encoding UTF8
  Write-Output "BLOCKED: xiigen-prompt ValuePattern unavailable."
  exit 34
}

$invokePattern = $null
try {
  $invokePattern = $submitElement.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
}
catch {
  $invokePattern = $null
}
if ($null -eq $invokePattern) {
  $decision.reason = "xiigen-submit did not expose InvokePattern; coordinate click fallback is forbidden for this worker."
  $decision | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $OutputDir "same-row23-visible-ui-route-decision.json") -Encoding UTF8
  Write-Output "BLOCKED: xiigen-submit InvokePattern unavailable."
  exit 35
}

$valuePattern.SetValue($prompt)
Start-Sleep -Milliseconds 500
$invokePattern.Invoke()
$decision.status = "SUBMITTED_LOCAL_PANEL_ROUTE"
$decision.reason = "Row23 prompt set by UIAutomation ValuePattern on xiigen-prompt and submitted by InvokePattern on xiigen-submit. No coordinate typing, SendKeys, clipboard, or external AI chat control was used."
$decision.submitted_at_utc = (Get-Date).ToUniversalTime().ToString("o")
$decision | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $OutputDir "same-row23-visible-ui-route-decision.json") -Encoding UTF8

$deadline = (Get-Date).AddSeconds(45)
$newFiles = @()
do {
  Start-Sleep -Seconds 2
  if (Test-Path -LiteralPath $globalStorageDir) {
    $newFiles = @(Get-ChildItem -File -LiteralPath $globalStorageDir |
      Where-Object { $_.LastWriteTimeUtc -ge $beforeUtc.AddSeconds(-1) } |
      Sort-Object LastWriteTimeUtc, Name |
      Select-Object FullName, LastWriteTimeUtc, Length)
  }
} while (@($newFiles).Count -lt 5 -and (Get-Date) -lt $deadline)

$newFiles | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $OutputDir "host-generated-files-after-submit.json") -Encoding UTF8

$copied = @()
foreach ($file in $newFiles) {
  $dest = Join-Path $hostEvidenceDir ([IO.Path]::GetFileName($file.FullName))
  Copy-Item -LiteralPath $file.FullName -Destination $dest -Force
  $copied += [pscustomobject]@{
    source = $file.FullName
    copy = $dest
    last_write_time_utc = $file.LastWriteTimeUtc
    length = $file.Length
  }
}
$copied | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $OutputDir "host-generated-copied-files.json") -Encoding UTF8

$captureAfter = Capture-TargetWindow -Target $target -Path (Join-Path $OutputDir "cursor-window-after-row23-submit.png") -Phase "after-row23-submit"
$captureAfter | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $OutputDir "screenshot-after-row23-submit.json") -Encoding UTF8

$requestCopy = $copied | Where-Object { $_.copy -like "*panel-request-*.json" } | Select-Object -Last 1
$panelCopy = $copied | Where-Object { $_.copy -like "*panel-panel-ui-*.json" } | Select-Object -Last 1
$hostTraceCopy = $copied | Where-Object { $_.copy -like "*panel-host-trace-*.json" } | Select-Object -Last 1
$coreResponseCopy = $copied | Where-Object { $_.copy -like "*panel-core-response-*.json" } | Select-Object -Last 1

$requestPromptMatches = $false
$requestJson = $null
if ($null -ne $requestCopy) {
  $requestJson = Get-Content -Raw -LiteralPath $requestCopy.copy | ConvertFrom-Json
  $requestPromptMatches = ([string]$requestJson.Prompt) -eq $prompt
}

$panelJson = $null
if ($null -ne $panelCopy) {
  $panelJson = Get-Content -Raw -LiteralPath $panelCopy.copy | ConvertFrom-Json
}

$hostTraceJson = $null
if ($null -ne $hostTraceCopy) {
  $hostTraceJson = Get-Content -Raw -LiteralPath $hostTraceCopy.copy | ConvertFrom-Json
}

$coreResponseJson = $null
if ($null -ne $coreResponseCopy) {
  $coreResponseJson = Get-Content -Raw -LiteralPath $coreResponseCopy.copy | ConvertFrom-Json
}

$redacted = [ordered]@{
  schema = "xiigen.cursor_same_row23_visible_ui_route_redacted_summary.v1"
  generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
  status = "PASS"
  prompt_id = [string]$row23.scenario_input.prompt_id
  request_prompt_matches_row23 = $requestPromptMatches
  prompt_sha256 = $null
  panel_ui_ref = if ($null -ne $panelCopy) { $panelCopy.copy } else { $null }
  host_trace_ref = if ($null -ne $hostTraceCopy) { $hostTraceCopy.copy } else { $null }
  core_response_ref = if ($null -ne $coreResponseCopy) { $coreResponseCopy.copy } else { $null }
  panel_submitted_prompt = if ($null -ne $panelJson) { $panelJson.submittedPrompt } else { $null }
  panel_external_ai_or_unrelated_tool_used = if ($null -ne $panelJson) { $panelJson.externalAiOrUnrelatedToolUsed } else { $null }
  panel_response_visible = if ($null -ne $panelJson) { $panelJson.responseVisibleInPanel } else { $null }
  visible_text_redacted_excerpt = if ($null -ne $panelJson -and $null -ne $panelJson.visibleText) { ([string]$panelJson.visibleText).Substring(0, [Math]::Min(500, ([string]$panelJson.visibleText).Length)) } else { $null }
  host_kind = if ($null -ne $hostTraceJson) { $hostTraceJson.hostKind } else { $null }
  entrypoint_name = if ($null -ne $hostTraceJson) { $hostTraceJson.entrypointName } else { $null }
  entrypoint_type = if ($null -ne $hostTraceJson) { $hostTraceJson.entrypointType } else { $null }
  core_success = if ($null -ne $coreResponseJson) { $coreResponseJson.Success } else { $null }
  core_status = if ($null -ne $coreResponseJson) { $coreResponseJson.Status } else { $null }
  semantic_quality_claimed = $false
  external_ai_chat_submission = "not performed"
  secrets_captured = "not observed; no OCR/forensic secret scan claimed"
  screenshot_after_ref = $captureAfter.screenshot_path
  capture_after_blank_or_failed = $captureAfter.blank_or_failed
  copied_file_count = @($copied).Count
}

$sha = [System.Security.Cryptography.SHA256]::Create()
$bytes = [System.Text.Encoding]::UTF8.GetBytes($prompt)
$redacted.prompt_sha256 = ([BitConverter]::ToString($sha.ComputeHash($bytes))).Replace("-", "").ToLowerInvariant()
$redacted | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $OutputDir "same-row23-visible-ui-route-redacted-summary.json") -Encoding UTF8

if (-not $requestPromptMatches) {
  Write-Output "PARTIAL: local panel submitted, but copied request did not prove exact Row23 prompt match."
  exit 41
}

if ($captureAfter.blank_or_failed) {
  Write-Output "PARTIAL: local panel submitted, but after-submit screenshot failed."
  exit 42
}

Write-Output "PASS: same Row23 prompt submitted through visible XIIGen local panel via UIAutomation controls; no external AI chat route used."
exit 0
