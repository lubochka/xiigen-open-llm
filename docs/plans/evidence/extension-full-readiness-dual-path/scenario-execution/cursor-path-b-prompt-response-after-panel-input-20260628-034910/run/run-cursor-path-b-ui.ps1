$ErrorActionPreference = 'Stop'

$ScenarioPath = 'C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-prep\cursor-full-extension-runtime-after-unlock-20260627-090159\scenario.json'
$EvidenceFolder = Split-Path -Parent (Split-Path -Parent $PSCommandPath)
$ScreenshotsFolder = Join-Path $EvidenceFolder 'screenshots'
$RunFolder = Join-Path $EvidenceFolder 'run'

[void][System.IO.Directory]::CreateDirectory($ScreenshotsFolder)
[void][System.IO.Directory]::CreateDirectory($RunFolder)

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type @'
using System;
using System.Runtime.InteropServices;
public class NativeMethods {
  [StructLayout(LayoutKind.Sequential)]
  public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
  [DllImport("user32.dll")] public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);
  [DllImport("user32.dll")] public static extern bool SetCursorPos(int X, int Y);
  [DllImport("user32.dll")] public static extern void mouse_event(int flags, int dx, int dy, int data, int extraInfo);
}
'@

$Utf8NoBom = [System.Text.UTF8Encoding]::new($false)
$LogLines = [System.Collections.Generic.List[string]]::new()
$ScreenshotPaths = [System.Collections.Generic.List[string]]::new()
$ClaimsNotMade = @(
  'token savings',
  'semantic quality improvement',
  'learnability/training',
  'checkpoint promotion',
  'all-host readiness'
)

function Save-Text {
  param([string]$Path, [string]$Content)
  $normalized = $Content -replace "`r`n", "`n"
  [System.IO.File]::WriteAllText($Path, $normalized, $Utf8NoBom)
}

function Add-Log {
  param([string]$Message)
  $line = "$(Get-Date -Format o) $Message"
  $LogLines.Add($line)
  Write-Host $line
}

function New-RectObject {
  $rect = New-Object NativeMethods+RECT
  return $rect
}

function Get-WindowRectangle {
  param([IntPtr]$Handle)
  $rect = New-RectObject
  [void][NativeMethods]::GetWindowRect($Handle, [ref]$rect)
  return $rect
}

function Save-WindowScreenshot {
  param([IntPtr]$Handle, [string]$FileName)
  $rect = Get-WindowRectangle -Handle $Handle
  $width = [Math]::Max(1, $rect.Right - $rect.Left)
  $height = [Math]::Max(1, $rect.Bottom - $rect.Top)
  $path = Join-Path $ScreenshotsFolder $FileName
  $bitmap = [System.Drawing.Bitmap]::new($width, $height)
  $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
  try {
    $graphics.CopyFromScreen($rect.Left, $rect.Top, 0, 0, [System.Drawing.Size]::new($width, $height))
    $bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
  } finally {
    $graphics.Dispose()
    $bitmap.Dispose()
  }
  $ScreenshotPaths.Add($path)
  Add-Log "Saved screenshot: $path"
  return $path
}

function Get-ElementBoundsObject {
  param($Element)
  $r = $Element.Current.BoundingRectangle
  return [pscustomobject]@{
    left = [Math]::Round($r.Left, 2)
    top = [Math]::Round($r.Top, 2)
    right = [Math]::Round($r.Right, 2)
    bottom = [Math]::Round($r.Bottom, 2)
    width = [Math]::Round($r.Width, 2)
    height = [Math]::Round($r.Height, 2)
  }
}

function Click-ElementCenter {
  param($Element)
  $r = $Element.Current.BoundingRectangle
  if ($r.IsEmpty -or $r.Width -le 0 -or $r.Height -le 0) {
    return $false
  }
  $x = [int](($r.Left + $r.Right) / 2)
  $y = [int](($r.Top + $r.Bottom) / 2)
  [void][NativeMethods]::SetCursorPos($x, $y)
  Start-Sleep -Milliseconds 100
  [NativeMethods]::mouse_event(0x0002, 0, 0, 0, 0)
  Start-Sleep -Milliseconds 80
  [NativeMethods]::mouse_event(0x0004, 0, 0, 0, 0)
  Start-Sleep -Milliseconds 250
  return $true
}

function Get-UiaSnapshot {
  param($Root, [string]$Name)
  $items = [System.Collections.Generic.List[object]]::new()
  $all = $Root.FindAll(
    [System.Windows.Automation.TreeScope]::Descendants,
    [System.Windows.Automation.Condition]::TrueCondition
  )
  for ($i = 0; $i -lt $all.Count; $i++) {
    $el = $all.Item($i)
    $current = $el.Current
    $typeName = $current.ControlType.ProgrammaticName -replace '^ControlType\.', ''
    $text = [string]$current.Name
    $automationId = [string]$current.AutomationId
    $shouldKeep = -not [string]::IsNullOrWhiteSpace($text)
    $shouldKeep = $shouldKeep -or ($typeName -in @('Edit', 'Button', 'Text', 'Document'))
    if ($shouldKeep) {
      $items.Add([pscustomobject]@{
        index = $i
        type = $typeName
        name = $text
        automation_id = $automationId
        enabled = $current.IsEnabled
        offscreen = $current.IsOffscreen
        bounds = Get-ElementBoundsObject -Element $el
      })
    }
  }
  $path = Join-Path $RunFolder "$Name-uia-snapshot.json"
  Save-Text -Path $path -Content (($items | ConvertTo-Json -Depth 7) + "`n")
  Add-Log "Saved UIA snapshot: $path"
  return $items
}

function Get-VisibleNames {
  param([object[]]$Snapshot)
  return @($Snapshot |
    Where-Object { -not $_.offscreen -and -not [string]::IsNullOrWhiteSpace($_.name) } |
    ForEach-Object { $_.name.Trim() } |
    Where-Object { $_.Length -gt 0 } |
    Select-Object -Unique)
}

function Find-SubmitButton {
  param($Root)
  $condition = [System.Windows.Automation.PropertyCondition]::new(
    [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
    [System.Windows.Automation.ControlType]::Button
  )
  $buttons = $Root.FindAll([System.Windows.Automation.TreeScope]::Descendants, $condition)
  $candidates = [System.Collections.Generic.List[object]]::new()
  for ($i = 0; $i -lt $buttons.Count; $i++) {
    $button = $buttons.Item($i)
    $name = [string]$button.Current.Name
    if (-not $button.Current.IsOffscreen -and $button.Current.IsEnabled) {
      if ($name -match '^(Submit|Send)$|Submit|Send') {
        $candidates.Add([pscustomobject]@{
          element = $button
          name = $name
          bounds = Get-ElementBoundsObject -Element $button
        })
      }
    }
  }
  if ($candidates.Count -eq 0) {
    return $null
  }
  $path = Join-Path $RunFolder 'submit-button-candidates.json'
  Save-Text -Path $path -Content (($candidates | Select-Object name,bounds | ConvertTo-Json -Depth 5) + "`n")
  Add-Log "Saved submit button candidates: $path"
  return $candidates[0].element
}

function Find-PromptEdit {
  param($Root, $SubmitButton)
  $condition = [System.Windows.Automation.PropertyCondition]::new(
    [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
    [System.Windows.Automation.ControlType]::Edit
  )
  $edits = $Root.FindAll([System.Windows.Automation.TreeScope]::Descendants, $condition)
  $candidates = [System.Collections.Generic.List[object]]::new()
  $buttonBounds = $null
  if ($SubmitButton) {
    $buttonBounds = $SubmitButton.Current.BoundingRectangle
  }
  for ($i = 0; $i -lt $edits.Count; $i++) {
    $edit = $edits.Item($i)
    $current = $edit.Current
    $r = $current.BoundingRectangle
    if ($current.IsOffscreen -or $r.IsEmpty -or $r.Width -lt 80 -or $r.Height -lt 20) {
      continue
    }
    $score = 0
    if ($current.Name -match 'prompt|message|ask|input|request') {
      $score += 500
    }
    if ($buttonBounds) {
      $overlap = ($r.Left -lt $buttonBounds.Right) -and ($r.Right -gt $buttonBounds.Left)
      if ($overlap) { $score += 500 }
      if ($r.Bottom -le ($buttonBounds.Top + 80)) {
        $score += [Math]::Max(0, 400 - [Math]::Abs($buttonBounds.Top - $r.Bottom))
      }
    }
    $score += [Math]::Min(300, [int]($r.Width / 2))
    $score += [Math]::Min(200, [int]($r.Height * 2))
    $candidates.Add([pscustomobject]@{
      element = $edit
      score = $score
      name = [string]$current.Name
      bounds = Get-ElementBoundsObject -Element $edit
    })
  }
  if ($candidates.Count -eq 0) {
    return $null
  }
  $ranked = @($candidates | Sort-Object -Property score -Descending)
  $path = Join-Path $RunFolder 'prompt-edit-candidates.json'
  Save-Text -Path $path -Content (($ranked | Select-Object score,name,bounds | ConvertTo-Json -Depth 5) + "`n")
  Add-Log "Saved prompt edit candidates: $path"
  return $ranked[0].element
}

function Try-GetValuePatternText {
  param($Element)
  if (-not $Element) { return $null }
  $pattern = $null
  if ($Element.TryGetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern, [ref]$pattern)) {
    return [string]$pattern.Current.Value
  }
  return $null
}

function Write-RequiredFiles {
  param(
    [object]$Result,
    [string]$StatusLine,
    [string]$BlockerLine,
    [string]$SummaryLine
  )
  $resultPath = Join-Path $EvidenceFolder 'result.json'
  Save-Text -Path $resultPath -Content (($Result | ConvertTo-Json -Depth 8) + "`n")

  $actionLog = @(
    '# Action log',
    '',
    $LogLines
  ) -join "`n"
  Save-Text -Path (Join-Path $EvidenceFolder 'action-log.md') -Content ($actionLog + "`n")

  $readme = @"
# Cursor Path B prompt-response UI scenario

Checked: local Cursor Extension Development Host, XIIGen Smart Skills panel, exact prompt from scenario.json, and Submit button path.

Prompt source: `exact_prompt`.

Steps: found and focused the Cursor window, captured the window before typing, pasted the exact prompt into the textarea, captured the before-submit state, clicked Submit, waited up to 60 seconds for visible response/status, and captured the after-submit state.

Result: $StatusLine

Proven: $SummaryLine

Not proven: token savings, semantic quality improvement, learnability/training, checkpoint promotion, or readiness of all hosts.

Blocker: $BlockerLine

Screenshots are in the `screenshots` folder.
"@
  Save-Text -Path (Join-Path $EvidenceFolder 'README.md') -Content ($readme + "`n")

  $redaction = @"
# Redaction note

No redaction was applied. The `exact_prompt` content contains no secrets, passwords, PIN, 2FA, or tokens.
"@
  Save-Text -Path (Join-Path $EvidenceFolder 'redaction-note.md') -Content ($redaction + "`n")
}

$status = 'BLOCKED_NOT_STARTED'
$blocker = $null
$promptTyped = $false
$submitClicked = $false
$visibleResponseSeen = $false
$responseSummary = ''
$scenario = $null
$promptField = 'exact_prompt'
$hostLabel = 'Cursor Extension Development Host'
$pathLabel = 'Path B real local Cursor UI'

try {
  Add-Log 'Loading scenario JSON.'
  $scenario = Get-Content -Raw -Encoding UTF8 -Path $ScenarioPath | ConvertFrom-Json
  $prompt = [string]$scenario.exact_prompt
  if ([string]::IsNullOrWhiteSpace($prompt)) {
    throw 'scenario.json field exact_prompt is missing or empty.'
  }
  Add-Log "Loaded prompt from field $promptField. Character length: $($prompt.Length)."

  $processes = @(Get-Process Cursor -ErrorAction SilentlyContinue |
    Where-Object { $_.MainWindowHandle -ne 0 -and $_.MainWindowTitle -match 'Extension Development Host|XIIGen|cursor-xiigen-smart-skills' } |
    Sort-Object @{ Expression = { if ($_.MainWindowTitle -match 'Extension Development Host') { 0 } else { 1 } } }, Id)
  Save-Text -Path (Join-Path $RunFolder 'window-inventory.json') -Content (($processes | Select-Object Id,ProcessName,MainWindowTitle,MainWindowHandle | ConvertTo-Json -Depth 4) + "`n")
  if ($processes.Count -eq 0) {
    $status = 'BLOCKED_TARGET_WINDOW_NOT_FOUND'
    $blocker = 'No Cursor Extension Development Host window appeared in process inventory.'
    throw $blocker
  }

  $process = $processes[0]
  $handle = [IntPtr]$process.MainWindowHandle
  Add-Log "Selected window pid=$($process.Id), handle=$($process.MainWindowHandle), title=$($process.MainWindowTitle)."
  [void][NativeMethods]::ShowWindowAsync($handle, 9)
  Start-Sleep -Milliseconds 300
  [void][NativeMethods]::SetForegroundWindow($handle)
  Start-Sleep -Seconds 1

  Save-WindowScreenshot -Handle $handle -FileName '01-window-before-typing.png' | Out-Null
  $root = [System.Windows.Automation.AutomationElement]::FromHandle($handle)
  $snapshotBefore = Get-UiaSnapshot -Root $root -Name '01-before-typing'
  $visibleBefore = Get-VisibleNames -Snapshot $snapshotBefore
  if (($visibleBefore -join "`n") -notmatch 'XIIGen|Smart Skills|Submit|Send') {
    Add-Log 'Panel markers are not strongly visible in UIA text; continuing because screenshots may still show WebView content.'
  }

  $submitButton = $null
  $promptEdit = $null
  $controlDeadline = (Get-Date).AddSeconds(12)
  do {
    $root = [System.Windows.Automation.AutomationElement]::FromHandle($handle)
    $submitButton = Find-SubmitButton -Root $root
    if ($submitButton) {
      $promptEdit = Find-PromptEdit -Root $root -SubmitButton $submitButton
    }
    if ($submitButton -and $promptEdit) {
      break
    }
    Start-Sleep -Milliseconds 750
  } while ((Get-Date) -lt $controlDeadline)
  if (-not $submitButton) {
    $status = 'BLOCKED_SUBMIT_BUTTON_NOT_FOUND'
    $blocker = 'Could not find an enabled Submit/Send button in the Cursor window UIA tree.'
    throw $blocker
  }
  Add-Log "Found submit button name='$($submitButton.Current.Name)' bounds=$(($submitButton.Current.BoundingRectangle).ToString())."
  if (-not $promptEdit) {
    $status = 'BLOCKED_PROMPT_TEXTAREA_NOT_FOUND'
    $blocker = 'Could not find a visible prompt textarea/edit control in the Cursor window UIA tree.'
    throw $blocker
  }
  Add-Log "Found prompt edit name='$($promptEdit.Current.Name)' bounds=$(($promptEdit.Current.BoundingRectangle).ToString())."

  [void][NativeMethods]::SetForegroundWindow($handle)
  Start-Sleep -Milliseconds 300
  $valuePattern = $null
  if ($promptEdit.TryGetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern, [ref]$valuePattern)) {
    $valuePattern.SetValue($prompt)
    Start-Sleep -Seconds 1
    Add-Log 'Set exact prompt with UI Automation ValuePattern.'
  }
  $editValueAfterSet = Try-GetValuePatternText -Element $promptEdit
  if (($editValueAfterSet -eq $null) -or (-not $editValueAfterSet.Contains($prompt))) {
    [void](Click-ElementCenter -Element $promptEdit)
    [System.Windows.Forms.Clipboard]::SetText($prompt)
    Start-Sleep -Milliseconds 300
    [System.Windows.Forms.SendKeys]::SendWait('^a')
    Start-Sleep -Milliseconds 200
    [System.Windows.Forms.SendKeys]::SendWait('^v')
    Start-Sleep -Seconds 1
    Add-Log 'Pasted exact prompt into selected edit control using the clipboard fallback; no secrets were typed.'
  }

  $root = [System.Windows.Automation.AutomationElement]::FromHandle($handle)
  $snapshotEntered = Get-UiaSnapshot -Root $root -Name '02-before-submit'
  $visibleEntered = Get-VisibleNames -Snapshot $snapshotEntered
  $editValue = Try-GetValuePatternText -Element $promptEdit
  $promptTyped = (($visibleEntered -join "`n").Contains($prompt)) -or (($editValue -ne $null) -and $editValue.Contains($prompt))
  Save-WindowScreenshot -Handle $handle -FileName '02-before-submit-prompt-entered.png' | Out-Null

  $submitButton = Find-SubmitButton -Root $root
  if (-not $submitButton) {
    $status = 'BLOCKED_SUBMIT_BUTTON_LOST_AFTER_PROMPT'
    $blocker = 'Submit button was found before typing but not after entering the prompt.'
    throw $blocker
  }
  $invokePattern = $null
  if ($submitButton.TryGetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern, [ref]$invokePattern)) {
    $invokePattern.Invoke()
  } else {
    [void](Click-ElementCenter -Element $submitButton)
  }
  $submitClicked = $true
  Add-Log 'Clicked/invoked Submit.'

  $beforeSet = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
  foreach ($name in $visibleEntered) {
    [void]$beforeSet.Add($name)
  }

  $newVisible = @()
  $start = Get-Date
  do {
    Start-Sleep -Seconds 2
    $root = [System.Windows.Automation.AutomationElement]::FromHandle($handle)
    $snapshotAfterLoop = Get-UiaSnapshot -Root $root -Name ('03-after-submit-{0:00}s' -f [int]((Get-Date) - $start).TotalSeconds)
    $visibleNow = Get-VisibleNames -Snapshot $snapshotAfterLoop
    $newVisible = @($visibleNow | Where-Object { -not $beforeSet.Contains($_) -and $_ -ne $prompt })
    $markerText = ($newVisible -join "`n")
    if ($markerText -match '(?i)response|status|error|warning|selected|skills|orchestration|task|structured|plan|requested|available|core|processing|loading') {
      $visibleResponseSeen = $true
      break
    }
  } while (((Get-Date) - $start).TotalSeconds -lt 60)

  Save-WindowScreenshot -Handle $handle -FileName '03-after-submit-response.png' | Out-Null
  if ($visibleResponseSeen) {
    $status = 'ACCEPTED_UI_PROMPT_RESPONSE'
    $responseSummary = (($newVisible | Select-Object -First 8) -join ' | ')
    if ([string]::IsNullOrWhiteSpace($responseSummary)) {
      $responseSummary = 'Visible UIA response/status marker appeared after submit.'
    }
  } else {
    $status = 'BLOCKED_RESPONSE_NOT_VISIBLE'
    $blocker = 'Prompt was submitted, but no new visible UIA response/status marker appeared within 60 seconds.'
    $responseSummary = 'No new visible response/status marker detected by UIA within timeout; inspect after-submit screenshot.'
  }
} catch {
  if ($status -eq 'BLOCKED_NOT_STARTED') {
    $status = 'FAILED_RUNNER_EXCEPTION'
  }
  if ([string]::IsNullOrWhiteSpace($blocker)) {
    $blocker = $_.Exception.Message
  }
  Add-Log "Exception or blocker: $blocker"
  try {
    if ($handle) {
      Save-WindowScreenshot -Handle $handle -FileName '99-blocker-state.png' | Out-Null
    }
  } catch {
    Add-Log "Could not capture blocker screenshot: $($_.Exception.Message)"
  }
} finally {
  $summaryLine = if ($status -eq 'ACCEPTED_UI_PROMPT_RESPONSE') {
    'Prompt typed, Submit clicked, and a new visible response/status marker appeared after this submit.'
  } elseif ($submitClicked) {
    'Prompt submit action was attempted; visible response/status was not proven by automation.'
  } else {
    'UI prompt-response scenario was not completed.'
  }
  $blockerLine = if ([string]::IsNullOrWhiteSpace($blocker)) { 'none' } else { $blocker }
  $result = [ordered]@{
    scenario_id = if ($scenario) { [string]$scenario.scenario_id } else { 'unknown' }
    host = $hostLabel
    path = $pathLabel
    status = $status
    prompt_source_field = $promptField
    prompt_typed = [bool]$promptTyped
    submit_clicked = [bool]$submitClicked
    visible_response_seen = [bool]$visibleResponseSeen
    response_or_status_summary = $responseSummary
    screenshots = @($ScreenshotPaths)
    blocker_if_any = if ([string]::IsNullOrWhiteSpace($blocker)) { $null } else { $blocker }
    deprecated_product_readiness_used = $false
    claims_not_made = $ClaimsNotMade
  }
  Write-RequiredFiles -Result $result -StatusLine $status -BlockerLine $blockerLine -SummaryLine $summaryLine
  Add-Log "Final status: $status"
  Save-Text -Path (Join-Path $EvidenceFolder 'action-log.md') -Content (("# Action log", "", $LogLines) -join "`n" + "`n")
  if ($status -like 'FAILED_*') {
    exit 2
  }
  if ($status -like 'BLOCKED_*') {
    exit 1
  }
  exit 0
}
