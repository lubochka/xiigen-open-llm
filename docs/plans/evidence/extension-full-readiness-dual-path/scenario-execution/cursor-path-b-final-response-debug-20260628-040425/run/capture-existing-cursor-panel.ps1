param(
  [Parameter(Mandatory=$true)]
  [string]$EvidenceRoot
)

$ErrorActionPreference = 'Stop'

$runDir = Join-Path $EvidenceRoot 'run'
$screensDir = Join-Path $EvidenceRoot 'screenshots'
$excerptDir = Join-Path $EvidenceRoot 'log-excerpts'
New-Item -ItemType Directory -Force -Path $runDir, $screensDir, $excerptDir | Out-Null

$actionLog = New-Object System.Collections.Generic.List[string]
function Add-Log([string]$Message) {
  $actionLog.Add(("{0} {1}" -f (Get-Date).ToString('o'), $Message))
}

function Write-JsonFile([string]$Path, [object]$Value) {
  $Value | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $Path -Encoding UTF8
}

function Get-StringPreview([object]$Value, [int]$Max = 240) {
  if ($null -eq $Value) { return '' }
  $text = ([string]$Value) -replace "\r?\n", ' '
  if ($text.Length -le $Max) { return $text }
  return $text.Substring(0, $Max)
}

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class NativeMethods {
  [DllImport("user32.dll")]
  public static extern bool SetForegroundWindow(IntPtr hWnd);
  [DllImport("user32.dll")]
  public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
  [DllImport("user32.dll")]
  public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
}
public struct RECT {
  public int Left;
  public int Top;
  public int Right;
  public int Bottom;
}
"@

function Save-WindowScreenshot([IntPtr]$Handle, [string]$Path) {
  $rect = New-Object RECT
  if (-not [NativeMethods]::GetWindowRect($Handle, [ref]$rect)) {
    throw "GetWindowRect failed."
  }
  $width = [Math]::Max(1, $rect.Right - $rect.Left)
  $height = [Math]::Max(1, $rect.Bottom - $rect.Top)
  $bitmap = New-Object System.Drawing.Bitmap($width, $height)
  $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
  try {
    $graphics.CopyFromScreen($rect.Left, $rect.Top, 0, 0, [System.Drawing.Size]::new($width, $height))
    $bitmap.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png)
  } finally {
    $graphics.Dispose()
    $bitmap.Dispose()
  }
}

function Get-UiaTextNodes([System.Windows.Automation.AutomationElement]$Root) {
  $queue = New-Object System.Collections.Queue
  $queue.Enqueue($Root)
  $items = New-Object System.Collections.Generic.List[object]
  while ($queue.Count -gt 0 -and $items.Count -lt 600) {
    $element = [System.Windows.Automation.AutomationElement]$queue.Dequeue()
    $name = $element.Current.Name
    $controlType = $element.Current.ControlType.ProgrammaticName -replace '^ControlType\.', ''
    $automationId = $element.Current.AutomationId
    if (-not [string]::IsNullOrWhiteSpace($name)) {
      $items.Add([ordered]@{
        type = $controlType
        name = $name
        automation_id = $automationId
      })
    }
    $children = $element.FindAll([System.Windows.Automation.TreeScope]::Children, [System.Windows.Automation.Condition]::TrueCondition)
    foreach ($child in $children) {
      $queue.Enqueue($child)
    }
  }
  return @($items)
}

$status = 'RUNTIME_BLOCKER'
$blocker = $null
$windowTitle = $null
$screenshotPath = Join-Path $screensDir '01-existing-panel-after-final-response.png'
$runtimeRoot = Join-Path $env:APPDATA 'Cursor\User\globalStorage\xiigen.cursor-xiigen-smart-skills\xiigen-cursor-host-runtime'
$latestPath = Join-Path $runtimeRoot 'xiigen-cursor-host-runtime-latest.json'
$panelUiPath = $null
$visibleFinalResponse = $false
$visibleStillSubmitting = $false
$runtimeResultRecorded = $false
$uiaCaptureError = $null
$summary = ''
$refs = @{}

try {
  Add-Log 'Capture started. No prompt submission or typing is performed by this runner.'

  $process = Get-Process Cursor |
    Where-Object { $_.MainWindowHandle -ne 0 -and $_.MainWindowTitle -like '*XIIGen Smart Skills*' } |
    Sort-Object StartTime -Descending |
    Select-Object -First 1
  if (-not $process) {
    throw 'Cursor XIIGen Smart Skills window was not found.'
  }

  $handle = [IntPtr]$process.MainWindowHandle
  $windowTitle = $process.MainWindowTitle
  Add-Log ("Selected window pid={0}, title=[{1}]." -f $process.Id, $windowTitle)
  [void][NativeMethods]::ShowWindowAsync($handle, 9)
  Start-Sleep -Milliseconds 300
  [void][NativeMethods]::SetForegroundWindow($handle)
  Start-Sleep -Seconds 2

  Save-WindowScreenshot -Handle $handle -Path $screenshotPath
  Add-Log ("Saved screenshot: {0}" -f $screenshotPath)

  $uiaNodes = @()
  try {
    $root = [System.Windows.Automation.AutomationElement]::FromHandle($handle)
    $uiaNodes = Get-UiaTextNodes -Root $root
    $uiaPath = Join-Path $runDir '01-existing-panel-uia-text-nodes.json'
    Write-JsonFile -Path $uiaPath -Value $uiaNodes
    Add-Log ("Saved redacted UIA text nodes: {0}" -f $uiaPath)

    $visibleText = (@($uiaNodes | ForEach-Object { $_.name }) -join "`n")
    $visibleStillSubmitting = $visibleText -match 'Submitting prompt to llm_mvp_core'
    $visibleFinalResponse = $visibleText -match 'Core task\+plan\+skills ready|Core API response recorded for adapter functionality|Prompt-to-task produced task type'
  } catch {
    $uiaCaptureError = $_.Exception.Message
    $uiaErrorPath = Join-Path $runDir '01-existing-panel-uia-capture-error.txt'
    $uiaCaptureError | Set-Content -LiteralPath $uiaErrorPath -Encoding UTF8
    Add-Log ("UIA text capture failed but screenshot capture and runtime JSON checks continue: {0}" -f $uiaCaptureError)
  }

  if (Test-Path -LiteralPath $latestPath) {
    $latestItem = Get-Item -LiteralPath $latestPath
    $latest = Get-Content -LiteralPath $latestPath -Raw | ConvertFrom-Json
    $runtimeResultRecorded = ($latest.adapterFunctionalityPassed -eq $true -and $latest.passed -eq $true)
    $refs = [ordered]@{
      latest = $latestPath
      latestLastWriteTime = $latestItem.LastWriteTime.ToString('o')
      coreResponseRef = [string]$latest.coreResponseRef
      hostTraceRef = [string]$latest.hostTraceRef
      entrypointResponseRef = [string]$latest.entrypointResponseRef
    }
    Write-JsonFile -Path (Join-Path $excerptDir 'global-storage-latest-summary.json') -Value ([ordered]@{
      path = $latestPath
      lastWriteTime = $latestItem.LastWriteTime.ToString('o')
      adapterFunctionalityPassed = $latest.adapterFunctionalityPassed
      passed = $latest.passed
      displayPreview = Get-StringPreview $latest.CursorDisplayText
      coreResponseRef = [string]$latest.coreResponseRef
      hostTraceRef = [string]$latest.hostTraceRef
      entrypointResponseRef = [string]$latest.entrypointResponseRef
    })

    $panelCandidate = Get-ChildItem -LiteralPath $runtimeRoot -File -Filter '*panel-ui-2026-06-28T00-49-24-891Z.json' -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($panelCandidate) {
      $panelUiPath = $panelCandidate.FullName
      $panelUi = Get-Content -LiteralPath $panelUiPath -Raw | ConvertFrom-Json
      Write-JsonFile -Path (Join-Path $excerptDir 'panel-ui-summary.json') -Value ([ordered]@{
        path = $panelUiPath
        lastWriteTime = $panelCandidate.LastWriteTime.ToString('o')
        panelUiCreated = $panelUi.panelUiCreated
        submittedPrompt = $panelUi.submittedPrompt
        visibleTextPreview = Get-StringPreview $panelUi.visibleText
        responseRef = [string]$panelUi.responseRef
        traceRef = [string]$panelUi.traceRef
      })
      if (([string]$panelUi.visibleText) -match 'Core task\+plan\+skills ready|Core API response recorded for adapter functionality|Prompt-to-task produced task type') {
        $visibleFinalResponse = $true
      }
      Add-Log ("Read panel UI summary: {0}" -f $panelUiPath)
    }
  }

  if ($visibleFinalResponse) {
    $status = 'FINAL_RESPONSE_CAPTURED'
    $summary = 'Existing Cursor panel visibly shows the post-submit final response text.'
  } elseif ($runtimeResultRecorded) {
    $status = 'RUNTIME_BLOCKER'
    $blocker = 'Post-submit core result files exist, but current UI capture did not prove final response text visible.'
    $summary = 'Runtime completed and wrote result files; visible panel proof remains missing in this capture.'
  } else {
    $status = 'RUNTIME_BLOCKER'
    $blocker = 'No post-submit adapter-passed latest result was found in Cursor globalStorage.'
    $summary = 'Neither visible final response nor adapter-passed runtime result was proven.'
  }
} catch {
  $status = 'RUNTIME_BLOCKER'
  $blocker = $_.Exception.Message
  $summary = 'Capture runner failed before proving visible final response.'
  Add-Log ("ERROR: {0}" -f $blocker)
}

$checkedItems = @(
  'existing Cursor XIIGen Smart Skills window screenshot',
  'post-submit Cursor globalStorage result files',
  'old Path B evidence timing and status'
)
if ([string]::IsNullOrWhiteSpace($uiaCaptureError)) {
  $checkedItems += 'redacted UIA text names'
} else {
  $checkedItems += 'UIA text capture attempted but failed; see run/01-existing-panel-uia-capture-error.txt'
}

$result = [ordered]@{
  status = $status
  checked = $checkedItems
  no_new_prompt_submitted = $true
  prompt_text_captured_beyond_existing_scenario = $false
  windowTitle = $windowTitle
  visible_final_response_seen = $visibleFinalResponse
  visible_still_submitting_seen = $visibleStillSubmitting
  runtime_result_recorded_after_submit = $runtimeResultRecorded
  uia_capture_error_if_any = $uiaCaptureError
  screenshot = $screenshotPath
  runtimeRefs = $refs
  panelUiRef = $panelUiPath
  blocker_if_any = $blocker
  summary = $summary
  deprecated_product_readiness_used = $false
  claims_not_made = @(
    'token savings',
    'semantic quality improvement',
    'learnability/training',
    'checkpoint promotion',
    'all-host readiness'
  )
}

Write-JsonFile -Path (Join-Path $EvidenceRoot 'result.json') -Value $result

$uiaEvidenceLine = if ([string]::IsNullOrWhiteSpace($uiaCaptureError)) {
  '- Redacted UIA text nodes: run/01-existing-panel-uia-text-nodes.json'
} else {
  '- UIA helper error: run/01-existing-panel-uia-capture-error.txt'
}

$readme = @"
# Cursor Path B final-response debug

Status: $status

This bounded packet checks the concrete gap after the earlier Submit proof: whether the existing Cursor panel had advanced past `Submitting prompt to llm_mvp_core...` and whether the extension wrote post-submit result files.

No new prompt was submitted by this packet.

Evidence:
- Screenshot: screenshots/01-existing-panel-after-final-response.png
$uiaEvidenceLine
- Runtime latest summary: log-excerpts/global-storage-latest-summary.json
- Panel UI summary: log-excerpts/panel-ui-summary.json

Summary: $summary

Blocker: $(if ([string]::IsNullOrWhiteSpace($blocker)) { 'none' } else { $blocker })

Deprecated product-readiness gate used: false
"@
$readme | Set-Content -LiteralPath (Join-Path $EvidenceRoot 'README.md') -Encoding UTF8

$actionContent = "# Action log`n`n" + (($actionLog | ForEach-Object { "- $_" }) -join "`n") + "`n"
$actionContent | Set-Content -LiteralPath (Join-Path $EvidenceRoot 'action-log.md') -Encoding UTF8

Write-Output $status
if ($blocker) {
  Write-Output $blocker
}
