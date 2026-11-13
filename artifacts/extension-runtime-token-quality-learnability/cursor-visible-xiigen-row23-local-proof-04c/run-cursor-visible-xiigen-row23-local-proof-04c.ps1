param(
  [switch]$UpdateState
)

$ErrorActionPreference = 'Stop'

$SubagentId = 'CURSOR-VISIBLE-XIIGEN-ROW23-LOCAL-PROOF-04C'
$EvidenceDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = 'C:\Projects\llm_mvp_core'
$StatePath = Join-Path $RepoRoot 'docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json'
$AgentsPath = Join-Path $RepoRoot 'AGENTS.md'
$Row23InputPath = Join-Path $RepoRoot 'artifacts\extension-runtime-token-quality-learnability\row23-input-path\row23-approved-safe-scenario-input.json'

$PriorFiles = @(
  (Join-Path $RepoRoot 'artifacts\extension-runtime-token-quality-learnability\cursor-host-route-recheck-03\cursor-host-route-recheck-03-packet.json'),
  (Join-Path $RepoRoot 'docs\plans\evidence\extension-full-readiness-dual-path\scenario-plans\cursor-path-b-route-script-20260628-015445\result.json'),
  (Join-Path $RepoRoot 'docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\cursor-path-b-route-proof-20260628-020041\result.json'),
  (Join-Path $RepoRoot 'artifacts\extension-runtime-token-quality-learnability\cursor-same-row23-visible-ui-blocker-packet-02\cursor-same-row23-visible-ui-blocker-packet-02-packet.json'),
  (Join-Path $RepoRoot 'artifacts\extension-runtime-token-quality-learnability\cursor-non-host-readiness-gaps-02\cursor-non-host-readiness-gaps-02-packet.json')
)

$PacketJsonPath = Join-Path $EvidenceDir 'cursor-visible-xiigen-row23-local-proof-04c-packet.json'
$PacketMdPath = Join-Path $EvidenceDir 'cursor-visible-xiigen-row23-local-proof-04c-packet.md'
$UiaInventoryPath = Join-Path $EvidenceDir 'uia-window-inventory.json'
$UiaPreflightPath = Join-Path $EvidenceDir 'uia-target-preflight.json'
$UiaResultPath = Join-Path $EvidenceDir 'uia-result-after-submit.json'
$StateMarkerPath = Join-Path $EvidenceDir 'state-marker-proposal.json'
$CommandLogPath = Join-Path $EvidenceDir 'commands-or-tools-used.json'

function Write-JsonFile {
  param(
    [Parameter(Mandatory=$true)] [string]$Path,
    [Parameter(Mandatory=$true)] $Value,
    [int]$Depth = 20
  )
  $json = $Value | ConvertTo-Json -Depth $Depth
  $encoding = New-Object System.Text.UTF8Encoding($false)
  [System.IO.File]::WriteAllText($Path, $json + [Environment]::NewLine, $encoding)
}

function Read-JsonFile {
  param([Parameter(Mandatory=$true)] [string]$Path)
  $text = [System.IO.File]::ReadAllText($Path, [System.Text.Encoding]::UTF8)
  if ($text.Length -gt 0 -and $text[0] -eq [char]0xFEFF) {
    $text = $text.Substring(1)
  }
  return $text | ConvertFrom-Json
}

function Get-FileRecord {
  param([Parameter(Mandatory=$true)] [string]$Path, [string]$Purpose = '')
  $item = Get-Item -LiteralPath $Path
  $hash = (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash
  [pscustomobject]@{
    path = $Path
    purpose = $Purpose
    exists = $true
    bytes = $item.Length
    sha256 = $hash
    last_write_time_utc = $item.LastWriteTimeUtc.ToString('o')
  }
}

function Sanitize-Text {
  param([AllowNull()][string]$Text)
  if ($null -eq $Text) { return $null }
  $s = $Text
  $s = [regex]::Replace($s, '[A-Za-z]:\\[^\s"\]\)]{6,}', '<local-path-redacted>')
  $s = [regex]::Replace($s, '(?i)(api[_-]?key|token|secret|password)\s*[:=]\s*[^\s,;]+', '$1=<redacted>')
  $s = [regex]::Replace($s, '[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}', '<email-redacted>')
  if ($s.Length -gt 500) { return $s.Substring(0, 500) + '...<truncated>' }
  return $s
}

function ControlTypeName {
  param($Element)
  try {
    return $Element.Current.ControlType.ProgrammaticName
  } catch {
    return $null
  }
}

function ElementRecord {
  param($Element)
  $rect = $Element.Current.BoundingRectangle
  [pscustomobject]@{
    name = Sanitize-Text $Element.Current.Name
    automation_id = $Element.Current.AutomationId
    class_name = $Element.Current.ClassName
    control_type = ControlTypeName $Element
    localized_control_type = $Element.Current.LocalizedControlType
    framework_id = $Element.Current.FrameworkId
    process_id = $Element.Current.ProcessId
    is_enabled = $Element.Current.IsEnabled
    is_keyboard_focusable = $Element.Current.IsKeyboardFocusable
    has_keyboard_focus = $Element.Current.HasKeyboardFocus
    rect = [pscustomobject]@{
      left = [math]::Round($rect.Left, 2)
      top = [math]::Round($rect.Top, 2)
      right = [math]::Round($rect.Right, 2)
      bottom = [math]::Round($rect.Bottom, 2)
      width = [math]::Round($rect.Width, 2)
      height = [math]::Round($rect.Height, 2)
    }
  }
}

function AncestorRecords {
  param($Element, $Root)
  $records = New-Object System.Collections.ArrayList
  $walker = [System.Windows.Automation.TreeWalker]::RawViewWalker
  $current = $Element
  for ($i = 0; $i -lt 30 -and $null -ne $current; $i++) {
    [void]$records.Add((ElementRecord $current))
    if ([object]::ReferenceEquals($current, $Root)) { break }
    $current = $walker.GetParent($current)
  }
  return @($records)
}

function TextMatchesRisk {
  param($Record)
  $parts = @($Record.name, $Record.automation_id, $Record.class_name, $Record.localized_control_type) -join ' '
  return ($parts -match '(?i)\b(agent|chat|composer|claude|codex|ask|prompt input|message)\b')
}

function Get-SubtreeText {
  param($RootElement)
  $items = New-Object System.Collections.ArrayList
  $all = $RootElement.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition)
  for ($i = 0; $i -lt $all.Count; $i++) {
    $e = $all.Item($i)
    $name = $e.Current.Name
    $aid = $e.Current.AutomationId
    if ([string]::IsNullOrWhiteSpace($name) -and [string]::IsNullOrWhiteSpace($aid)) { continue }
    $record = ElementRecord $e
    if (($record.name -match '(?i)xiigen|core task|semantic quality|selected|skills|readiness|prompt|submit|result|warning') -or ($record.automation_id -match '(?i)xiigen')) {
      [void]$items.Add($record)
    }
    if ($items.Count -ge 80) { break }
  }
  return @($items)
}

function Insert-StateMarker {
  param(
    [Parameter(Mandatory=$true)] [string]$Path,
    [Parameter(Mandatory=$true)] [string]$MarkerName,
    [Parameter(Mandatory=$true)] $MarkerObject
  )
  $beforeHash = (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash
  $text = [System.IO.File]::ReadAllText($Path, [System.Text.Encoding]::UTF8)
  $hasBom = ($text.Length -gt 0 -and $text[0] -eq [char]0xFEFF)
  $work = if ($hasBom) { $text.Substring(1) } else { $text }
  $trimmed = $work.TrimEnd()
  if (-not $trimmed.EndsWith('}')) {
    throw 'State file does not end with a JSON object close brace.'
  }
  $markerJson = $MarkerObject | ConvertTo-Json -Depth 20
  $withoutClose = $trimmed.Substring(0, $trimmed.Length - 1)
  $needsComma = ($withoutClose.TrimEnd().Length -gt 1 -and -not $withoutClose.TrimEnd().EndsWith('{'))
  $insert = if ($needsComma) { ",`n  `"$MarkerName`": $markerJson`n}" } else { "`n  `"$MarkerName`": $markerJson`n}" }
  $newWork = $withoutClose + $insert + [Environment]::NewLine
  $newText = if ($hasBom) { ([char]0xFEFF) + $newWork } else { $newWork }
  $tmp = Join-Path (Split-Path -Parent $Path) ([System.IO.Path]::GetFileName($Path) + '.tmp-04c')
  $encoding = New-Object System.Text.UTF8Encoding($false)
  if ($hasBom) {
    $encoding = New-Object System.Text.UTF8Encoding($true)
    [System.IO.File]::WriteAllText($tmp, $newWork, $encoding)
  } else {
    [System.IO.File]::WriteAllText($tmp, $newWork, $encoding)
  }
  try {
    $parsed = Read-JsonFile $tmp
    if ($null -eq $parsed.PSObject.Properties[$MarkerName]) {
      throw "Inserted marker $MarkerName was not parse-visible."
    }
    Move-Item -Force -Path $tmp -Destination $Path
    $afterHash = (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash
    return [pscustomobject]@{
      made = $true
      marker_name = $MarkerName
      state_before_sha256 = $beforeHash
      state_after_sha256 = $afterHash
      validation = 'temp file parsed and inserted marker was visible before replacement'
    }
  } catch {
    if (Test-Path -LiteralPath $tmp) { Remove-Item -LiteralPath $tmp -Force }
    throw
  }
}

$commands = New-Object System.Collections.ArrayList
function Add-CommandRecord {
  param([string]$Tool, [string]$Result)
  [void]$commands.Add([pscustomobject]@{
    tool_or_command = $Tool
    result = $Result
    timestamp_utc = (Get-Date).ToUniversalTime().ToString('o')
  })
}

$readFiles = New-Object System.Collections.ArrayList
$writtenFiles = New-Object System.Collections.ArrayList
$RunnerPath = $MyInvocation.MyCommand.Path
[void]$writtenFiles.Add($RunnerPath)
$blockers = New-Object System.Collections.ArrayList
$nonClaims = @(
  'No external AI prompt/chat submission.',
  'No network/install/git/source edit.',
  'No secrets captured intentionally.',
  'No token savings claim.',
  'No semantic quality improvement or heldout pass claim.',
  'No learnability/training/checkpoint mutation or promotion claim.',
  'No full Cursor readiness claim.',
  'No all-three extension readiness claim.'
)

try {
  [void]$readFiles.Add((Get-FileRecord $AgentsPath 'honesty/no-overclaim governance file required by work order'))
  $agentsText = [System.IO.File]::ReadAllText($AgentsPath, [System.Text.Encoding]::UTF8)
  $agentsRequiredSnippets = @(
    'NO DISHONEST CLAIMS / OVERCLAIM FIREWALL',
    'Parent Architect Subagent-Only Firewall',
    'If the agent did not run it, read it, receive it, verify it, or inspect it'
  )
  $agentsSnippetCheck = @{}
  foreach ($snippet in $agentsRequiredSnippets) {
    $agentsSnippetCheck[$snippet] = $agentsText.Contains($snippet)
  }
  Add-CommandRecord 'Read AGENTS.md via .NET File.ReadAllText plus hash' 'exit 0; full local file bytes read by runner and required snippets checked'
} catch {
  [void]$blockers.Add("AGENTS.md read failed: $($_.Exception.Message)")
}

$row23 = $null
try {
  [void]$readFiles.Add((Get-FileRecord $Row23InputPath 'approved Row23 safe local input packet'))
  $row23 = Read-JsonFile $Row23InputPath
  Add-CommandRecord 'Read Row23 approved safe input JSON' 'exit 0; prompt_text loaded from scenario_input.prompt_text'
} catch {
  [void]$blockers.Add("Row23 input read failed: $($_.Exception.Message)")
}

$priorSummaries = New-Object System.Collections.ArrayList
foreach ($file in $PriorFiles) {
  try {
    [void]$readFiles.Add((Get-FileRecord $file 'prior Cursor route/blocker/gap packet requested by work order'))
    $obj = Read-JsonFile $file
    [void]$priorSummaries.Add([pscustomobject]@{
      path = $file
      status = $obj.status
      same_row23_visible_ui_route_status = $obj.same_row23_visible_ui_route_status
      cursor_readiness_status = $obj.cursor_readiness_status
      path_b_route_proven = $obj.path_b_route_proven
      evidence_supports_only = Sanitize-Text ([string]$obj.evidence_supports_only)
      blockers = @($obj.blockers | ForEach-Object { Sanitize-Text ([string]$_) })
      next_bounded_worker_scope_or_NEEDS_USER_HOST_ACTION = Sanitize-Text ([string]$obj.next_bounded_worker_scope_or_NEEDS_USER_HOST_ACTION)
    })
  } catch {
    [void]$priorSummaries.Add([pscustomobject]@{
      path = $file
      read_error = $_.Exception.Message
    })
  }
}
Add-CommandRecord 'Read requested prior Cursor packets' "exit 0; attempted $($PriorFiles.Count) targeted prior packet reads"

try {
  $stateItem = Get-Item -LiteralPath $StatePath
  [void]$readFiles.Add([pscustomobject]@{
    path = $StatePath
    purpose = 'current execution state targeted/BOM-aware metadata only before optional narrow marker'
    exists = $true
    bytes = $stateItem.Length
    sha256 = (Get-FileHash -LiteralPath $StatePath -Algorithm SHA256).Hash
    first_bytes = [System.BitConverter]::ToString([System.IO.File]::ReadAllBytes($StatePath)[0..2])
    last_write_time_utc = $stateItem.LastWriteTimeUtc.ToString('o')
  })
  Add-CommandRecord 'Read execution state metadata' 'exit 0; BOM/size/hash metadata only, no full state parse at this stage'
} catch {
  [void]$blockers.Add("State metadata read failed: $($_.Exception.Message)")
}

$uia = [ordered]@{}
$preflight = [ordered]@{}
$resultSummary = [ordered]@{}
$sameRouteStatus = 'BLOCKED'
$cursorReadinessStatus = 'PARTIAL'
$stateTransition = 'NOT_MADE'
$targetSafe = $false
$submitInvoked = $false

try {
  Add-Type -AssemblyName UIAutomationClient
  Add-Type -AssemblyName UIAutomationTypes
  Add-CommandRecord 'Load UIAutomationClient/UIAutomationTypes' 'exit 0'

  $desktop = [System.Windows.Automation.AutomationElement]::RootElement
  $top = $desktop.FindAll([System.Windows.Automation.TreeScope]::Children, [System.Windows.Automation.Condition]::TrueCondition)
  $windowRecords = New-Object System.Collections.ArrayList
  $cursorCandidates = New-Object System.Collections.ArrayList

  for ($i = 0; $i -lt $top.Count; $i++) {
    $w = $top.Item($i)
    $rec = ElementRecord $w
    if (($rec.name -match '(?i)cursor|xiigen|agent|chat|claude|codex') -or ($rec.process_id -gt 0)) {
      try {
        $proc = Get-Process -Id $rec.process_id -ErrorAction SilentlyContinue
        $rec | Add-Member -NotePropertyName process_name -NotePropertyValue $proc.ProcessName -Force
      } catch {
        $rec | Add-Member -NotePropertyName process_name -NotePropertyValue $null -Force
      }
      if (($rec.process_name -eq 'Cursor') -or ($rec.name -match '(?i)cursor|xiigen')) {
        [void]$cursorCandidates.Add($w)
      }
      [void]$windowRecords.Add($rec)
    }
  }
  $uia.window_inventory = @($windowRecords)
  Write-JsonFile $UiaInventoryPath $uia 30
  [void]$writtenFiles.Add($UiaInventoryPath)
  Add-CommandRecord 'UIA top-level window inventory' "exit 0; wrote $UiaInventoryPath"

  $promptMatches = New-Object System.Collections.ArrayList
  $submitMatches = New-Object System.Collections.ArrayList
  $agentRiskMatches = New-Object System.Collections.ArrayList
  $xiigenWindows = New-Object System.Collections.ArrayList

  foreach ($window in $cursorCandidates) {
    $windowRecord = ElementRecord $window
    $desc = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition)
    $hasXiigen = $false
    for ($j = 0; $j -lt $desc.Count; $j++) {
      $e = $desc.Item($j)
      $rec = ElementRecord $e
      $hay = (@($rec.name, $rec.automation_id, $rec.class_name, $rec.localized_control_type) -join ' ')
      if ($hay -match '(?i)xiigen|smart skills') { $hasXiigen = $true }
      if ($rec.automation_id -eq 'xiigen-prompt' -and $rec.control_type -eq 'ControlType.Edit') {
        [void]$promptMatches.Add([pscustomobject]@{ element = $e; window = $window; record = $rec; window_record = $windowRecord })
      }
      if ($rec.automation_id -eq 'xiigen-submit' -and $rec.control_type -eq 'ControlType.Button') {
        [void]$submitMatches.Add([pscustomobject]@{ element = $e; window = $window; record = $rec; window_record = $windowRecord })
      }
      if ($hay -match '(?i)\b(agent|chat|composer|show chat history|new agent)\b') {
        [void]$agentRiskMatches.Add([pscustomobject]@{ record = $rec; window_record = $windowRecord })
      }
    }
    if ($hasXiigen -or $windowRecord.name -match '(?i)xiigen|smart skills') {
      [void]$xiigenWindows.Add($windowRecord)
    }
  }

  $preflight.prompt_match_count = $promptMatches.Count
  $preflight.submit_match_count = $submitMatches.Count
  $preflight.xiigen_window_count = $xiigenWindows.Count
  $preflight.xiigen_windows = @($xiigenWindows)
  $preflight.agent_chat_risk_count = $agentRiskMatches.Count
  $preflight.agent_chat_risk_samples = @($agentRiskMatches | Select-Object -First 20 | ForEach-Object { $_.record })
  $preflight.no_input_before_preflight = $true
  $preflight.screenshot_taken = $false
  $preflight.screenshot_not_taken_reason = 'Skipped to avoid capturing unrelated or secret UI content; UIA text evidence is restricted to target/control metadata and sanitized panel text.'

  if ($promptMatches.Count -ne 1 -or $submitMatches.Count -ne 1) {
    [void]$blockers.Add("UIA target not unambiguous: prompt_match_count=$($promptMatches.Count), submit_match_count=$($submitMatches.Count).")
  } else {
    $prompt = $promptMatches[0]
    $submit = $submitMatches[0]
    $promptAnc = AncestorRecords $prompt.element $prompt.window
    $submitAnc = AncestorRecords $submit.element $submit.window
    $sameWindow = [object]::ReferenceEquals($prompt.window, $submit.window)
    $promptRiskInAncestors = @($promptAnc | Where-Object { TextMatchesRisk $_ })
    $submitRiskInAncestors = @($submitAnc | Where-Object { TextMatchesRisk $_ })
    $promptXiigenAncestry = (@($promptAnc | Where-Object { ((@($_.name, $_.automation_id, $_.class_name, $_.localized_control_type) -join ' ') -match '(?i)xiigen|smart skills|rootwebarea') })).Count -gt 0
    $submitXiigenAncestry = (@($submitAnc | Where-Object { ((@($_.name, $_.automation_id, $_.class_name, $_.localized_control_type) -join ' ') -match '(?i)xiigen|smart skills|rootwebarea') })).Count -gt 0
    $promptRect = $prompt.record.rect
    $submitRect = $submit.record.rect
    $windowRect = $prompt.window_record.rect
    $rectsInsideWindow = (
      $promptRect.left -ge $windowRect.left -and $promptRect.right -le $windowRect.right -and
      $promptRect.top -ge $windowRect.top -and $promptRect.bottom -le $windowRect.bottom -and
      $submitRect.left -ge $windowRect.left -and $submitRect.right -le $windowRect.right -and
      $submitRect.top -ge $windowRect.top -and $submitRect.bottom -le $windowRect.bottom
    )

    $preflight.prompt_target = $prompt.record
    $preflight.submit_target = $submit.record
    $preflight.prompt_ancestor_chain = @($promptAnc)
    $preflight.submit_ancestor_chain = @($submitAnc)
    $preflight.same_window = $sameWindow
    $preflight.rects_inside_window = $rectsInsideWindow
    $preflight.prompt_xiigen_ancestry = $promptXiigenAncestry
    $preflight.submit_xiigen_ancestry = $submitXiigenAncestry
    $preflight.prompt_agent_chat_risk_in_ancestors = @($promptRiskInAncestors)
    $preflight.submit_agent_chat_risk_in_ancestors = @($submitRiskInAncestors)
    $preflight.other_agent_chat_controls_present_not_targeted = ($agentRiskMatches.Count -gt 0)
    $preflight.safety_decision_basis = @(
      'Exact automationId xiigen-prompt is ControlType.Edit.',
      'Exact automationId xiigen-submit is ControlType.Button.',
      'Prompt and submit controls share the same Cursor Extension Development Host window.',
      'Target ancestry contains XIIGen/Smart Skills/root web area.',
      "Prompt ancestor Agent/chat/composer risk count: $($promptRiskInAncestors.Count).",
      "Submit ancestor Agent/chat/composer risk count: $($submitRiskInAncestors.Count).",
      "Target rectangles inside window according to UIA: $rectsInsideWindow.",
      'SetValue/InvokePattern would be used only on exact UIA elements; no keyboard typing, mouse click, or Cursor Agent/chat control targeting.'
    )

    if ($sameWindow -and $rectsInsideWindow -and $promptXiigenAncestry -and $submitXiigenAncestry -and $promptRiskInAncestors.Count -eq 0 -and $submitRiskInAncestors.Count -eq 0) {
      $targetSafe = $true
      $preflight.target_control_safety = 'PASS_UNAMBIGUOUS_XIIGEN_LOCAL_PANEL_TARGET'
    } else {
      [void]$blockers.Add("UIA target ancestry did not prove a safe unambiguous XIIGen local panel target: rects_inside_window=$rectsInsideWindow, prompt_agent_chat_risk_in_ancestors=$($promptRiskInAncestors.Count), submit_agent_chat_risk_in_ancestors=$($submitRiskInAncestors.Count).")
      $preflight.target_control_safety = 'BLOCKED_NOT_UNAMBIGUOUS'
    }
  }

  Write-JsonFile $UiaPreflightPath $preflight 40
  [void]$writtenFiles.Add($UiaPreflightPath)
  Add-CommandRecord 'UIA target preflight' "exit 0; wrote $UiaPreflightPath"

  if ($targetSafe -and $null -ne $row23) {
    $promptText = [string]$row23.scenario_input.prompt_text
    if ([string]::IsNullOrWhiteSpace($promptText)) {
      throw 'Approved Row23 input did not contain scenario_input.prompt_text.'
    }
    $promptEl = $promptMatches[0].element
    $submitEl = $submitMatches[0].element
    $valuePattern = $null
    $invokePattern = $null
    if (-not $promptEl.TryGetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern, [ref]$valuePattern)) {
      throw 'xiigen-prompt does not expose ValuePattern; refusing keyboard typing.'
    }
    if (-not $submitEl.TryGetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern, [ref]$invokePattern)) {
      throw 'xiigen-submit does not expose InvokePattern; refusing mouse/keyboard submit.'
    }

    $resultSummary.prompt_set_method = 'ValuePattern.SetValue on automationId xiigen-prompt'
    $resultSummary.submit_method = 'InvokePattern.Invoke on automationId xiigen-submit'
    $resultSummary.external_ai_or_cursor_chat_targeted = $false
    $resultSummary.correlation_id_used = $false
    $resultSummary.correlation_id_limitation = 'No separate correlation-id field was exposed by UIA; the approved Row23 prompt text was not modified to add one.'
    $resultSummary.approved_prompt_sha256 = ([System.BitConverter]::ToString([System.Security.Cryptography.SHA256]::Create().ComputeHash([System.Text.Encoding]::UTF8.GetBytes($promptText))).Replace('-', ''))

    $valuePattern.SetValue($promptText)
    Start-Sleep -Milliseconds 300
    $afterSet = $null
    try {
      $afterSet = $promptEl.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).Current.Value
    } catch {
      $afterSet = $null
    }
    $resultSummary.prompt_value_matches_after_set = ($afterSet -eq $promptText)
    if (-not $resultSummary.prompt_value_matches_after_set) {
      throw 'ValuePattern.SetValue did not leave the approved Row23 prompt in xiigen-prompt; refusing submit.'
    }

    $invokePattern.Invoke()
    $submitInvoked = $true
    Add-CommandRecord 'UIA ValuePattern.SetValue + InvokePattern.Invoke' 'exit 0; approved Row23 prompt set into xiigen-prompt and xiigen-submit invoked'

    $pollRecords = New-Object System.Collections.ArrayList
    for ($poll = 0; $poll -lt 20; $poll++) {
      Start-Sleep -Milliseconds 1000
      $panelText = Get-SubtreeText $promptMatches[0].window
      [void]$pollRecords.Add([pscustomobject]@{
        poll = $poll + 1
        sampled_records = @($panelText | Select-Object -First 20)
      })
      $flat = (($panelText | ForEach-Object { @($_.name, $_.automation_id) -join ' ' }) -join ' ')
      if ($flat -match '(?i)core task|task\+plan\+skills|semantic quality|selected skills|readiness') {
        break
      }
    }
    $latestText = Get-SubtreeText $promptMatches[0].window
    $flatLatest = (($latestText | ForEach-Object { @($_.name, $_.automation_id) -join ' ' }) -join ' ')
    $resultSummary.panel_text_records_after_submit = @($latestText)
    $resultSummary.poll_record_count = $pollRecords.Count
    $resultSummary.result_text_indicators = @(
      @{ indicator = 'core task'; present = ($flatLatest -match '(?i)core task') },
      @{ indicator = 'task+plan+skills'; present = ($flatLatest -match '(?i)task\+plan\+skills') },
      @{ indicator = 'semantic quality'; present = ($flatLatest -match '(?i)semantic quality') },
      @{ indicator = 'selected skills'; present = ($flatLatest -match '(?i)selected skills') },
      @{ indicator = 'readiness'; present = ($flatLatest -match '(?i)readiness') }
    )
    $resultSummary.no_secrets_statement = 'UIA result capture was restricted to sanitized XIIGen/control metadata and selected panel text; no screenshot, clipboard dump, environment dump, network trace, or unrelated window text was captured.'

    $indicatorPass = (@($resultSummary.result_text_indicators | Where-Object { $_.present })).Count -ge 2
    if ($indicatorPass) {
      $sameRouteStatus = 'PASS'
      $cursorReadinessStatus = 'PARTIAL'
    } else {
      $sameRouteStatus = 'PARTIAL'
      $cursorReadinessStatus = 'PARTIAL'
      [void]$blockers.Add('Submit was invoked on exact XIIGen controls, but UIA result text did not expose enough fresh output indicators to claim PASS.')
    }
  } elseif (-not $targetSafe) {
    Add-CommandRecord 'UIA action phase' 'not run; preflight did not prove a safe unambiguous XIIGen local target'
  }

  Write-JsonFile $UiaResultPath $resultSummary 50
  [void]$writtenFiles.Add($UiaResultPath)
} catch {
  [void]$blockers.Add("UIA proof/action failed or aborted: $($_.Exception.Message)")
  if ($targetSafe -and -not $submitInvoked) {
    $sameRouteStatus = 'BLOCKED'
    $cursorReadinessStatus = 'PARTIAL'
  }
  Add-CommandRecord 'UIA proof/action exception' $_.Exception.Message
  try {
    Write-JsonFile $UiaResultPath $resultSummary 50
    [void]$writtenFiles.Add($UiaResultPath)
  } catch {}
}

$stateMarker = [pscustomobject]@{
  subagent_id_or_nickname = $SubagentId
  generated_at_utc = (Get-Date).ToUniversalTime().ToString('o')
  same_row23_visible_xiigen_route_status = $sameRouteStatus
  cursor_readiness_status = $cursorReadinessStatus
  evidence_packet = $PacketJsonPath
  uia_preflight = $UiaPreflightPath
  uia_result = $UiaResultPath
  evidence_supports_only = 'Narrow Cursor visible XIIGen local panel UIA proof/blocker for the approved Row23 input. No full Cursor readiness, token savings, semantic quality, learnability/training, checkpoint promotion, or all-three readiness claim.'
  non_claims = $nonClaims
}
Write-JsonFile $StateMarkerPath $stateMarker 20
[void]$writtenFiles.Add($StateMarkerPath)

if ($UpdateState) {
  try {
    $stateResult = Insert-StateMarker -Path $StatePath -MarkerName '_cursor_visible_xiigen_row23_local_proof_04c' -MarkerObject $stateMarker
    $stateTransition = "MADE: added _cursor_visible_xiigen_row23_local_proof_04c ($($stateResult.validation))"
    [void]$writtenFiles.Add($StatePath)
    Add-CommandRecord 'Execution state narrow marker update' "exit 0; $stateTransition"
  } catch {
    $stateTransition = "NOT_MADE: state marker update failed safely before replacement: $($_.Exception.Message)"
    [void]$blockers.Add($stateTransition)
    Add-CommandRecord 'Execution state narrow marker update' $stateTransition
  }
} else {
  $stateTransition = 'NOT_MADE: runner called without -UpdateState; state marker proposal written only.'
  Add-CommandRecord 'Execution state narrow marker update' $stateTransition
}

Write-JsonFile $CommandLogPath @($commands) 20
[void]$writtenFiles.Add($CommandLogPath)

$evidenceSupports = if ($sameRouteStatus -eq 'PASS') {
  'Current UIA evidence supports a narrow same-Row23 visible XIIGen local panel route: exact xiigen-prompt/xiigen-submit controls were proven in the XIIGen panel, the approved Row23 prompt was set with ValuePattern, xiigen-submit was invoked with InvokePattern, and sanitized panel UIA text exposed local result indicators. This does not prove full Cursor readiness, token savings, semantic quality improvement, learnability/training, checkpoint promotion, or all-three extension readiness.'
} elseif ($sameRouteStatus -eq 'PARTIAL') {
  'Current UIA evidence supports target/control proof and a bounded XIIGen local panel action attempt, but not enough fresh result/correlation evidence for PASS. It does not prove full Cursor readiness or any quality/token/learnability claims.'
} else {
  'Current evidence supports only a blocker or preflight failure for safe same-Row23 visible XIIGen local panel proof. No input/submission is claimed unless commands_or_tools_used says UIA ValuePattern/InvokePattern succeeded.'
}

$nextScope = if ($sameRouteStatus -eq 'PASS') {
  'Next bounded worker: parent review should validate the 04c packet and decide the next remaining Cursor readiness blocker (token savings, semantic quality, learnability/training, lifecycle, final joined coverage) without upgrading beyond this narrow UIA route proof.'
} else {
  'NEEDS_USER_HOST_ACTION: expose a local-only Cursor XIIGen panel state where xiigen-prompt/xiigen-submit are unambiguous and result output can be freshly correlated, with Cursor Agent/chat closed or unfocusable if target ancestry cannot otherwise prove safety. No external AI prompt/chat submission, no secrets, no install/network/git/source edits.'
}

$packet = [ordered]@{
  schema = 'cursor-visible-xiigen-row23-local-proof-04c.v1'
  subagent_id_or_nickname = $SubagentId
  generated_at_utc = (Get-Date).ToUniversalTime().ToString('o')
  claim_guard = [ordered]@{
    claim_scope = 'Cursor same-Row23 visible XIIGen local-route proof/blocker only.'
    evidence_scope = 'Exact files read plus live UIA/window inventory, target ancestry preflight, and only safe UIA ValuePattern/InvokePattern action if target was unambiguous.'
    count_claimed = [ordered]@{
      uia_window_inventory_runs = 1
      uia_target_preflight_runs = 1
      approved_row23_prompt_sets = if ($submitInvoked -or $sameRouteStatus -in @('PASS','PARTIAL')) { 1 } else { 0 }
      xiigen_submit_invokes = if ($submitInvoked) { 1 } else { 0 }
      screenshots = 0
      external_ai_calls = 0
      network_calls = 0
      source_edits = 0
      git_actions = 0
      tests_or_builds = 0
    }
    independent_evidence_count = 1
    subagent_id_or_NONE = $SubagentId
    downgrade_needed = ($cursorReadinessStatus -ne 'FULL')
  }
  changed_files = @($writtenFiles)
  exact_files_read = @($readFiles)
  exact_files_written = @($writtenFiles)
  controls_targeted_and_safety_preflight = [ordered]@{
    target_safe = $targetSafe
    prompt_automation_id = 'xiigen-prompt'
    submit_automation_id = 'xiigen-submit'
    preflight_ref = $UiaPreflightPath
    input_method = if ($submitInvoked) { 'UIA ValuePattern.SetValue only; no keyboard typing or mouse click' } else { 'not run' }
    submit_method = if ($submitInvoked) { 'UIA InvokePattern.Invoke only; no mouse click or keyboard submit' } else { 'not run' }
    cursor_agent_chat_controls_present_not_targeted = $preflight.other_agent_chat_controls_present_not_targeted
    safety_decision = $preflight.target_control_safety
    no_secrets_statement = 'No screenshot, clipboard dump, environment dump, network trace, or unrelated window text was captured by this runner; UIA text was sanitized and restricted to target/control metadata plus selected XIIGen panel text.'
  }
  commands_or_tools_used = @($commands)
  same_row23_visible_xiigen_route_status = $sameRouteStatus
  cursor_readiness_status = $cursorReadinessStatus
  state_transition_made_or_not = $stateTransition
  evidence_supports_only = $evidenceSupports
  blockers = @($blockers)
  non_claims = $nonClaims
  prior_packets_reconciled = @($priorSummaries)
  output_refs = [ordered]@{
    uia_window_inventory = $UiaInventoryPath
    uia_target_preflight = $UiaPreflightPath
    uia_result_after_submit = $UiaResultPath
    state_marker_proposal = $StateMarkerPath
    commands_or_tools_used = $CommandLogPath
  }
  next_bounded_worker_scope_or_NEEDS_USER_HOST_ACTION = $nextScope
  parent_will_perform_commit_push_after_review = $true
  must_stop_after_report = $true
}

Write-JsonFile $PacketJsonPath $packet 60
[void]$writtenFiles.Add($PacketJsonPath)

$md = @"
# Cursor Visible XIIGen Row23 Local Proof 04C

claim_scope: Cursor same-Row23 visible XIIGen local-route proof/blocker only.
evidence_scope: Exact file reads plus live UIA/window inventory and target ancestry preflight; UIA SetValue/Invoke only when target is unambiguous.
count_claimed: see packet JSON claim_guard.count_claimed
independent_evidence_count: 1
subagent_id_or_NONE: $SubagentId
downgrade_needed: $($cursorReadinessStatus -ne 'FULL')

## Result

- same_row23_visible_xiigen_route_status: $sameRouteStatus
- cursor_readiness_status: $cursorReadinessStatus
- state_transition_made_or_not: $stateTransition

## Evidence Supports Only

$evidenceSupports

## Safety

- Target safe: $targetSafe
- Input method: $(if ($submitInvoked) { 'UIA ValuePattern.SetValue on xiigen-prompt' } else { 'not run' })
- Submit method: $(if ($submitInvoked) { 'UIA InvokePattern.Invoke on xiigen-submit' } else { 'not run' })
- Screenshot: not captured.
- External AI/chat: not targeted.

## Blockers

$((@($blockers) | ForEach-Object { "- $_" }) -join [Environment]::NewLine)

## Next

$nextScope
"@
$encodingNoBom = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($PacketMdPath, $md + [Environment]::NewLine, $encodingNoBom)
[void]$writtenFiles.Add($PacketMdPath)

# Refresh packet exact file list after Markdown write.
$packet.exact_files_written = @($writtenFiles)
$packet.changed_files = @($writtenFiles)
Write-JsonFile $PacketJsonPath $packet 60
Write-JsonFile $CommandLogPath @($commands) 20

[pscustomobject]@{
  status = $sameRouteStatus
  cursor_readiness_status = $cursorReadinessStatus
  packet = $PacketJsonPath
  markdown = $PacketMdPath
  state_transition_made_or_not = $stateTransition
  blockers_count = $blockers.Count
} | ConvertTo-Json -Depth 10
