$ErrorActionPreference = 'Stop'

$repo = 'C:\Projects\llm_mvp_core'
$evidenceDir = Join-Path $repo 'artifacts\extension-runtime-token-quality-learnability\row23-input-path'
$statePath = Join-Path $repo 'docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json'
$inputPath = Join-Path $evidenceDir 'row23-approved-safe-scenario-input.json'
$packetPath = Join-Path $evidenceDir 'row23-input-path-evidence-packet.json'
$approvalQuoteBase64 = '0J3QtSDQv9C+0L3QuNC80LDRjiDQv9C+0YfQtdC80YMg0YHQvdC+0LLQsCDRgdGC0L7Qvz8/PyDQndCw0LnQtNC4INC4INGN0YLRgyDQu9Cw0LfQtdC50LrRgyDQv9C+0LbQsNC70YPQudGB0YLQsC4g0JfQsNC90LXRgdC4INGN0YLQviDQuCDQtNC10LnRgdGC0LLQuNGPINCy0L3QuNC30YMg0LIg0L/Qu9Cw0L0g0YDQsNCx0L7RgtGLLSDRgdC00LXQu9Cw0Lkg0L/Qu9Cw0L0g0YDQsNCx0L7RgtGLINC/0L7Qu9C90YvQvCDQv9C+INC90LDRiNC40Lwg0YHRgtCw0L3QtNCw0YDRgtCw0LwuINCh0LTQtdC70LDQuSAyMCDQv9GA0L7RhdC+0LTQvtCyINC4INGD0LHQtdC00LjRgdGMINGH0YLQviDQv9C70LDQvSDQtNC+0LLQtdC00ZHRgiDQvdCw0LrQvtC90LXRhiDQvdC10L7QsdGF0L7QtNC40LzRi9C1IGV4dGVuc2lvbiDQtNC+INC/0L7Qu9C90L7QuSDQs9C+0YLQvtCy0L3QvtGB0YLQuC4g0Jgg0L/RgNC40YHRgtGD0L/QsNC50Log0LTQtdC50YHRgtCy0LjRjiDQtNC+INC/0L7Qu9C90L7QuSDQs9C+0YLQvtCy0L3QvtGB0YLQuCDQstGB0LXRhSAzIGV4dGVuc2lvbnMuIE5PIFNUT1BTIGFwcHJvdmVkISEhIEFsbCB0aHJvdWdoIGFnZW50cyEhIQ=='
$approvalQuote = [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($approvalQuoteBase64))
$createdAt = (Get-Date).ToString('o')

New-Item -ItemType Directory -Force -Path $evidenceDir | Out-Null

$inputArtifact = [ordered]@{
  schema = 'xiigen.extension_readiness.row23_safe_local_input.v1'
  created_at_local = $createdAt
  subagent_id_or_NONE = 'ROW23-INPUT-PATH-RESOLVER'
  row_id = 'Row23'
  scenario_id = 'frdp-p01-codex-semantic-quality-heldout'
  host = 'Codex'
  category = 'safe_local_redacted_scenario_input'
  approval_quote_used = $approvalQuote
  provenance = [ordered]@{
    created_by = 'ROW23-INPUT-PATH-RESOLVER bounded sub-agent'
    source_basis = @(
      'Current bounded work order explicitly authorizes resolving Row23 by selecting or creating exactly one approved safe local input/path packet.',
      'Active plan Section 12.5 requires exactly one Row23 input/path with provenance, approval quote, category, evidence refs, forbidden actions, and claim locks.',
      'Execution state Row23 record previously accepted BLOCKED_NO_APPROVED_INPUT_PATH with approved_input_paths_found=0.'
    )
    external_ai_output_used = $false
    network_used = $false
    secret_material_included = $false
    redaction_required = $false
  }
  scenario_input = [ordered]@{
    prompt_id = 'row23-codex-safe-local-semantic-quality-input-001'
    prompt_text = 'Route this local project request through the Codex XIIGen smart-skills adapter: identify the task, list the most relevant skills, name the evidence that would be needed before claiming readiness, and keep all product/runtime/quality claims locked unless verified.'
    expected_safe_response_shape = @(
      'task classification',
      'candidate skills',
      'evidence needed before readiness claims',
      'non-claims and boundary locks'
    )
    forbidden_content = @(
      'secrets',
      'credentials',
      'private user data',
      'external AI output',
      'network-derived content'
    )
  }
  claim_locks = [ordered]@{
    semantic_quality_improvement_claim = $false
    heldout_pass_claim = $false
    codex_host_visible_answer_claim = $false
    host_readiness_claim = $false
    product_runtime_ui_readiness_claim = $false
    product_readiness_claim = $false
    formal_joined_pass = $false
    token_savings_claim = $false
    training_or_learnability_claim = $false
    checkpoint_promotion_claim = $false
    locator_update_claim = $false
    full_extension_readiness_claim = $false
  }
  allowed_use = 'May be used only as the single approved safe local Row23 input/path for the next bounded worker. It is not semantic-quality evidence by itself.'
}

$inputArtifact | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $inputPath -Encoding utf8

$selectedInput = Resolve-Path -LiteralPath $inputPath
$extensionTargets = [ordered]@{
  Claude = 'C:\Projects\claude-xiigen-smart-skills\src\claude-entrypoint.js'
  Cursor = 'C:\Projects\cursor-xiigen-smart-skills\src\extension.ts'
  Codex = 'C:\Projects\codex-xiigen-smart-skills\src\codex-entrypoint.js'
}
$targetExistence = [ordered]@{}
foreach ($entry in $extensionTargets.GetEnumerator()) {
  $targetExistence[$entry.Key] = [ordered]@{
    path = $entry.Value
    exists = [bool](Test-Path -LiteralPath $entry.Value -PathType Leaf)
  }
}

$packet = [ordered]@{
  schema = 'xiigen.extension_readiness.row23_input_path_evidence_packet.v1'
  created_at_local = $createdAt
  subagent_id_or_nickname = 'ROW23-INPUT-PATH-RESOLVER'
  claim_scope = 'Row23 safe local input/path resolution only.'
  evidence_scope = @(
    'AGENTS.md read from exact path.',
    'Active plan read from exact path.',
    'Execution state read and Row23 state queried.',
    'Row23 intake/design/blocker evidence packets read from exact paths.',
    'Selected input artifact created under allowed Row23 evidence folder.',
    'Three extension target paths checked for existence only.'
  )
  count_claimed = [ordered]@{
    approved_row23_input_paths_selected = 1
    row23_evidence_packets_created = 2
    state_files_updated = 1
    product_runtime_probes = 0
    tests_or_builds = 0
    ui_or_browser_checks = 0
    network_calls = 0
    external_ai_calls = 0
    git_actions = 0
  }
  independent_evidence_count = 1
  downgrade_needed = $true
  row_id = 'Row23'
  scenario_id = 'frdp-p01-codex-semantic-quality-heldout'
  selected_row23_input_path = $selectedInput.Path
  selected_row23_input_path_exists = $true
  approval_quote_used = $approvalQuote
  category = 'safe_local_redacted_scenario_input'
  provenance = $inputArtifact.provenance
  evidence_refs = @(
    'C:\Projects\llm_mvp_core\AGENTS.md',
    'C:\Projects\llm_mvp_core\docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md',
    'C:\Projects\llm_mvp_core\docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json',
    'C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-semantic-quality-heldout\row23-authority-input-intake-20260626-231026\row23-authority-input-intake-decision-packet.json',
    'C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-semantic-quality-heldout\row23-evidence-path-design-20260626-213801\row23-evidence-path-design-packet.json',
    'C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-semantic-quality-heldout\row23-blocker-packaging-status-20260626-232000\row23-blocker-packaging-status.json',
    'C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-semantic-quality-heldout\row23-heldout-same-scenario-evidence-20260626-221532\row23-heldout-same-scenario-evidence-packet.md'
  )
  forbidden_actions = @(
    'No product runtime probes',
    'No tests/builds',
    'No UI/browser checks',
    'No network calls',
    'No external AI calls',
    'No eval/heldout execution',
    'No Row24 advance',
    'No product extension code edits',
    'No git stage/commit/push'
  )
  claim_locks = $inputArtifact.claim_locks
  non_claims = @(
    'I did not verify semantic quality improvement.',
    'I did not run Row23 heldout evaluation.',
    'I did not run product runtime, UI, tests, builds, network, or external AI calls.',
    'I did not edit Claude, Cursor, Codex, or llm_mvp_core product code.',
    'I did not claim host readiness, product readiness, token savings, learnability, checkpoint promotion, locator update, full extension readiness, or formal joined pass.'
  )
  extension_targets_checked_for_existence_only = $targetExistence
  state_transition = [ordered]@{
    from = 'BLOCKED_NO_APPROVED_INPUT_PATH'
    to = 'ROW23_APPROVED_SAFE_INPUT_PATH_SELECTED_NEXT_BOUNDED_EXTENSION_READINESS'
    semantic_quality_status = 'blocked_not_proven'
    product_readiness_claim = $false
    formal_joined_pass = $false
    scenario_counts_preserved = [ordered]@{
      total_scenarios = 28
      scoped_accepted = 12
      product_readiness_full_pass = 0
      full_joined_pass = 0
      remaining = 16
    }
  }
  next_bounded_worker_scope = 'CLAUDE-EXT-READINESS-01: bounded sub-agent should inspect Claude extension host path and same-scenario evidence requirements, with product/runtime/UI/tests only if separately authorized in that next work order; preserve all non-claims until packet-backed proof exists.'
  evidence_supports_only = 'Exactly one safe local Row23 input/path was created and selected, with provenance and claim locks, and the state was moved to the next bounded action. This does not prove semantic quality, runtime/UI behavior, token savings, learnability, product readiness, or full extension readiness.'
  blockers = @()
  parent_will_perform_commit_push_after_review = $true
}

$packet | ConvertTo-Json -Depth 30 | Set-Content -LiteralPath $packetPath -Encoding utf8

$state = Get-Content -Raw -LiteralPath $statePath | ConvertFrom-Json
$state.next_action = 'Row23 approved safe local input/path has been selected by ROW23-INPUT-PATH-RESOLVER. Next bounded action: CLAUDE-EXT-READINESS-01 for Claude extension readiness evidence, followed by Cursor and Codex readiness workers. Product runtime/UI/tests/eval/heldout execution and Row24 advance remain forbidden unless a later bounded work order explicitly authorizes them.'
$state.human_readable_status_review_current = 'Row23 input/path blocker resolved narrowly: exactly one safe local Row23 input/path exists under artifacts/extension-runtime-token-quality-learnability/row23-input-path/. Counts remain honest: 28 scenarios total, 12 scoped accepted, 0 product-readiness full pass, 0 full joined pass, 16 remaining. This proves only input/path availability, not semantic quality, host runtime, token savings, learnability, or readiness. Next bounded action is CLAUDE-EXT-READINESS-01.'

if (-not $state.PSObject.Properties['row23_input_path_resolution_20260628']) {
  $state | Add-Member -NotePropertyName 'row23_input_path_resolution_20260628' -NotePropertyValue $null
}
$state.row23_input_path_resolution_20260628 = [pscustomobject]$packet

if ($state.PSObject.Properties['row23_status_board_sync_20260626']) {
  $state.row23_status_board_sync_20260626.status = 'ROW23_APPROVED_SAFE_INPUT_PATH_SELECTED_NEXT_BOUNDED_EXTENSION_READINESS / semantic_quality_blocked_not_proven'
  $state.row23_status_board_sync_20260626.semantic_quality_status = 'blocked_not_proven'
  $state.row23_status_board_sync_20260626.product_readiness_claim = $false
  $state.row23_status_board_sync_20260626.formal_joined_pass = $false
  $state.row23_status_board_sync_20260626.next_action = 'Row23 input/path blocker is resolved narrowly by row23_input_path_resolution_20260628. Continue with bounded CLAUDE-EXT-READINESS-01, then Cursor and Codex readiness workers. No semantic-quality pass, product/runtime/UI readiness, tests, heldout execution, Row24 advance, or final readiness is claimed.'
  if (-not $state.row23_status_board_sync_20260626.PSObject.Properties['approved_input_paths_found']) {
    $state.row23_status_board_sync_20260626 | Add-Member -NotePropertyName 'approved_input_paths_found' -NotePropertyValue 1
  } else {
    $state.row23_status_board_sync_20260626.approved_input_paths_found = 1
  }
  if (-not $state.row23_status_board_sync_20260626.PSObject.Properties['selected_row23_input_path']) {
    $state.row23_status_board_sync_20260626 | Add-Member -NotePropertyName 'selected_row23_input_path' -NotePropertyValue $selectedInput.Path
  } else {
    $state.row23_status_board_sync_20260626.selected_row23_input_path = $selectedInput.Path
  }
}

$state | ConvertTo-Json -Depth 100 | Set-Content -LiteralPath $statePath -Encoding utf8

[pscustomobject]@{
  selected_row23_input_path = $selectedInput.Path
  packet_path = (Resolve-Path -LiteralPath $packetPath).Path
  state_path = $statePath
  state_transition = $packet.state_transition
} | ConvertTo-Json -Depth 20
