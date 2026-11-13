param(
  [string]$CoreRoot = "C:\Projects\llm_mvp_core",
  [string]$CursorRoot = "C:\Projects\cursor-xiigen-smart-skills",
  [string]$EvidenceRoot = "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\cursor-docs-final-coverage-or-token-quality-nonhost-01"
)

$ErrorActionPreference = "Stop"

function Read-JsonFile {
  param([string]$Path)
  return (Get-Content -LiteralPath $Path -Raw).TrimStart([char]0xFEFF) | ConvertFrom-Json
}

function Read-TextFile {
  param([string]$Path)
  return Get-Content -LiteralPath $Path -Raw
}

function Test-TailContains {
  param(
    [string]$Path,
    [string[]]$Needles,
    [int]$BytesToRead = 65536
  )
  $fs = [System.IO.File]::OpenRead($Path)
  try {
    $count = [Math]::Min($BytesToRead, [int]$fs.Length)
    $null = $fs.Seek(-$count, [System.IO.SeekOrigin]::End)
    $buffer = New-Object byte[] $count
    $read = $fs.Read($buffer, 0, $count)
    $tail = [System.Text.Encoding]::UTF8.GetString($buffer, 0, $read)
    $missing = @($Needles | Where-Object { -not $tail.Contains($_) })
    return [ordered]@{
      bytes_read = $read
      needles_checked = $Needles
      missing = $missing
      present = ($missing.Count -eq 0)
    }
  } finally {
    $fs.Close()
  }
}

function Append-StateMarkerWithoutFullParse {
  param(
    [string]$StatePath,
    [string]$MarkerKey,
    [object]$Marker
  )
  $markerJson = $Marker | ConvertTo-Json -Depth 20
  $fs = [System.IO.File]::Open($StatePath, [System.IO.FileMode]::Open, [System.IO.FileAccess]::ReadWrite, [System.IO.FileShare]::Read)
  try {
    $pos = $fs.Length - 1
    $byte = -1
    while ($pos -ge 0) {
      $null = $fs.Seek($pos, [System.IO.SeekOrigin]::Begin)
      $byte = $fs.ReadByte()
      if ($byte -notin @(9, 10, 13, 32)) {
        break
      }
      $pos--
    }
    if ($byte -ne 125) {
      throw "State file final non-whitespace byte is not a JSON object close brace."
    }
    $fs.SetLength($pos)
  } finally {
    $fs.Close()
  }
  $append = ",`n  `"$MarkerKey`": $markerJson`n}`n"
  [System.IO.File]::AppendAllText($StatePath, $append, [System.Text.UTF8Encoding]::new($false))
}

$commandsRunPath = Join-Path $EvidenceRoot "commands-run.json"
$runVerifierStdoutPath = Join-Path $EvidenceRoot "command-output\run-verifier.stdout.log"
$runVerifierStderrPath = Join-Path $EvidenceRoot "command-output\run-verifier.stderr.log"
$nodeCheckStdoutPath = Join-Path $EvidenceRoot "command-output\node-check-verifier.stdout.log"
$nodeCheckStderrPath = Join-Path $EvidenceRoot "command-output\node-check-verifier.stderr.log"
$nodeVersionStdoutPath = Join-Path $EvidenceRoot "command-output\node-version.stdout.log"
$nodeVersionStderrPath = Join-Path $EvidenceRoot "command-output\node-version.stderr.log"
$packageStdoutPath = Join-Path $EvidenceRoot "command-output\package-verify-docs-final-coverage-nonhost.stdout.log"
$packageStderrPath = Join-Path $EvidenceRoot "command-output\package-verify-docs-final-coverage-nonhost.stderr.log"
$verifierScriptPath = Join-Path $CursorRoot "scripts\verify-docs-final-coverage-nonhost.js"
$packageJsonPath = Join-Path $CursorRoot "package.json"
$readmePath = Join-Path $CursorRoot "README.md"
$projectDocPath = Join-Path $CursorRoot "docs\PROJECT-DOC.md"
$coverageDocPath = Join-Path $CursorRoot "docs\CURSOR-DOCS-FINAL-COVERAGE-TOKEN-QUALITY-NONHOST.md"
$extReadinessPath = Join-Path $CoreRoot "artifacts\extension-runtime-token-quality-learnability\cursor-ext-readiness-01\cursor-ext-readiness-01-packet.json"
$nonhostGapsPath = Join-Path $CoreRoot "artifacts\extension-runtime-token-quality-learnability\cursor-nonhost-readiness-gaps-01\cursor-nonhost-readiness-gaps-01-packet.json"
$lifecycleRollbackPath = Join-Path $CoreRoot "artifacts\extension-runtime-token-quality-learnability\cursor-lifecycle-rollback-nonhost-validation-01\cursor-lifecycle-rollback-nonhost-validation-01-packet.json"
$row23Path = Join-Path $CoreRoot "artifacts\extension-runtime-token-quality-learnability\row23-input-path\row23-approved-safe-scenario-input.json"
$statePath = Join-Path $CoreRoot "docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json"
$packetJsonPath = Join-Path $EvidenceRoot "cursor-docs-final-coverage-or-token-quality-nonhost-01-packet.json"
$packetMdPath = Join-Path $EvidenceRoot "cursor-docs-final-coverage-or-token-quality-nonhost-01-packet.md"
$verificationPath = Join-Path $EvidenceRoot "state-marker-and-packet-verification.json"
$runnerPath = $PSCommandPath

$commandsRun = Read-JsonFile $commandsRunPath
$runVerifier = (Read-TextFile $runVerifierStdoutPath).TrimStart([char]0xFEFF) | ConvertFrom-Json
$runVerifierStderr = Read-TextFile $runVerifierStderrPath
$nodeCheckStdout = Read-TextFile $nodeCheckStdoutPath
$nodeCheckStderr = Read-TextFile $nodeCheckStderrPath
$nodeVersionStdout = Read-TextFile $nodeVersionStdoutPath
$nodeVersionStderr = Read-TextFile $nodeVersionStderrPath
$packageStdout = Read-TextFile $packageStdoutPath
$packageStderr = Read-TextFile $packageStderrPath
$verifierScript = Read-TextFile $verifierScriptPath
$packageJson = Read-JsonFile $packageJsonPath
$readme = Read-TextFile $readmePath
$projectDoc = Read-TextFile $projectDocPath
$coverageDoc = Read-TextFile $coverageDocPath
$extReadiness = Read-JsonFile $extReadinessPath
$nonhostGaps = Read-JsonFile $nonhostGapsPath
$lifecycleRollback = Read-JsonFile $lifecycleRollbackPath
$row23 = Read-JsonFile $row23Path

$timestamp = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
$markerKey = "cursor_docs_final_coverage_or_token_quality_nonhost_01_" + (Get-Date).ToUniversalTime().ToString("yyyyMMdd_HHmmss")

$docsChecklistPresent = $coverageDoc.Contains("Token Boundary") -and $coverageDoc.Contains("Quality Boundary") -and $coverageDoc.Contains("Final Coverage Boundary")
$docsLinksPresent = $readme.Contains("CURSOR-DOCS-FINAL-COVERAGE-TOKEN-QUALITY-NONHOST.md")
$verifierChecksBoundary = $verifierScript.Contains("token_savings_claimed=false") -and $verifierScript.Contains("quality_improvement_claimed=false") -and $verifierScript.Contains("No full Cursor readiness claim.")
$row23ClaimLocksAllFalse = @($row23.claim_locks.PSObject.Properties | Where-Object { $_.Value -ne $false }).Count -eq 0
$commandExitCodes = @($commandsRun | ForEach-Object { [ordered]@{ label = $_.label; exit_code = $_.exit_code; command = $_.command; stdout_ref = $_.stdout_ref; stderr_ref = $_.stderr_ref } })

$marker = [ordered]@{
  schema = "xiigen.cursor_docs_final_coverage_token_quality_nonhost_01.state_marker.v1"
  subagent_id_or_nickname = "CURSOR-DOCS-TOKEN-NONHOST-PACKET-STATE-01"
  timestamp_utc = $timestamp
  evidence_packet = $packetJsonPath
  docs_final_coverage_status = "PARTIAL"
  token_quality_nonhost_status = "PARTIAL"
  cursor_readiness_status = "PARTIAL"
  verifier_status = "PASS"
  verifier_status_source = "existing run-verifier.stdout.log; this worker did not run the product verifier"
  full_cursor_readiness_claimed = $false
  full_three_extension_readiness_claimed = $false
  non_claims_preserved = $true
  evidence_supports_only = "Existing Cursor docs/final coverage and token/quality nonhost verifier evidence. It supports a PASS verifier result for documentation/non-claim boundaries only, while docs final coverage, token_quality_nonhost, and Cursor readiness remain PARTIAL."
  blockers = @(
    "Token before/after reduction with quality non-regression is not proven.",
    "Semantic quality improvement and held-out quality are not proven.",
    "Full Cursor UI/chat/Extension Host readiness is not proven by this packet.",
    "Same-scenario Path A plus Path B final joined proof is not proven by this packet.",
    "Learnability, training, checkpoint mutation or promotion, and all-three extension readiness are not proven."
  )
  parent_will_perform_commit_push_after_review = $true
}

Append-StateMarkerWithoutFullParse -StatePath $statePath -MarkerKey $markerKey -Marker $marker

$packet = [ordered]@{
  schema = "xiigen.cursor_docs_final_coverage_token_quality_nonhost_01.packet.v1"
  subagent_id_or_nickname = "CURSOR-DOCS-TOKEN-NONHOST-PACKET-STATE-01"
  generated_at_utc = $timestamp
  requested_artifact_type = "Package existing Cursor docs/final coverage + token/quality nonhost verifier evidence and append narrow execution-state marker."
  changed_files = @(
    $runnerPath,
    $packetJsonPath,
    $packetMdPath,
    $verificationPath,
    $statePath
  )
  exact_files_read = @(
    (Join-Path $CoreRoot "AGENTS.md"),
    $commandsRunPath,
    $runVerifierStdoutPath,
    $runVerifierStderrPath,
    $nodeCheckStdoutPath,
    $nodeCheckStderrPath,
    $nodeVersionStdoutPath,
    $nodeVersionStderrPath,
    $packageStdoutPath,
    $packageStderrPath,
    $verifierScriptPath,
    $packageJsonPath,
    $readmePath,
    $projectDocPath,
    $coverageDocPath,
    $extReadinessPath,
    $nonhostGapsPath,
    $lifecycleRollbackPath,
    $row23Path,
    $statePath
  )
  read_scope_note = "AGENTS.md was read by targeted honesty/bounded-rule extraction; execution state was read by tail/targeted marker verification only. Full state parse is not claimed. Active plan content was not needed for this packet beyond the current work order."
  exact_files_written = @(
    $runnerPath,
    $packetJsonPath,
    $packetMdPath,
    $verificationPath,
    $statePath
  )
  commands_run_by_prior_worker_packaged_here = $commandExitCodes
  commands_run_by_this_worker = @(
    [ordered]@{
      label = "package-cursor-docs-final-coverage-or-token-quality-nonhost-01"
      command = "powershell -NoProfile -ExecutionPolicy Bypass -File $runnerPath"
      cwd = $CoreRoot
      product_verification = $false
      note = "Packaging/state-marker runner only; no source edit, no product command, no UI/browser/screenshot, no network, no git."
    }
  )
  JSON_parse_or_marker_verification = [ordered]@{
    packet_json_parse = "PENDING_VERIFIED_AFTER_WRITE"
    state_marker_presence = "PENDING_VERIFIED_AFTER_WRITE"
    full_state_parse = "NOT_RUN_NOT_CLAIMED"
  }
  docs_final_coverage_status = "PARTIAL"
  token_quality_nonhost_status = "PARTIAL"
  cursor_readiness_status = "PARTIAL"
  verifier_status = "PASS"
  verifier_status_source = "existing run-verifier.stdout.log generated_at_utc=$($runVerifier.generated_at_utc)"
  state_transition_made_or_not = "MADE: appended $markerKey to execution state without full-state parse or reserialization."
  state_marker_key = $markerKey
  evidence_summary = [ordered]@{
    prior_verifier_schema = $runVerifier.schema
    prior_verifier_status = $runVerifier.status
    prior_verifier_docs_status = $runVerifier.docs_final_coverage_status
    prior_verifier_token_quality_status = $runVerifier.token_quality_nonhost_status
    prior_verifier_cursor_status = $runVerifier.cursor_readiness_status
    docs_checklist_present = $docsChecklistPresent
    readme_links_coverage_doc = $docsLinksPresent
    verifier_checks_boundary_locks = $verifierChecksBoundary
    row23_claim_locks_all_false = $row23ClaimLocksAllFalse
    prior_ext_readiness_cursor_status = $extReadiness.cursor_readiness_status
    prior_nonhost_gap_status = $nonhostGaps.cursor_nonhost_gap_status
    prior_lifecycle_rollback_nonhost_status = $lifecycleRollback.lifecycle_rollback_nonhost_status
    node_version_stdout = $nodeVersionStdout.Trim()
    empty_stderr_logs = [ordered]@{
      node_version = ([string]::IsNullOrEmpty($nodeVersionStderr))
      node_check_verifier = ([string]::IsNullOrEmpty($nodeCheckStderr))
      run_verifier = ([string]::IsNullOrEmpty($runVerifierStderr))
      package_existing = ([string]::IsNullOrEmpty($packageStderr))
    }
  }
  non_claims_preserved = @(
    "No token savings claim.",
    "No quality improvement claim.",
    "No full Cursor readiness claim.",
    "No UI/browser/screenshot proof in this verifier.",
    "No training, checkpoint mutation, target promotion, or external AI submission.",
    "This worker did not run new product verification."
  )
  next_bounded_worker_scope_or_NEEDS_USER_HOST_ACTION = "CURSOR-TOKEN-QUALITY-SAME-PAIR-NONHOST-01: run only if separately authorized; gather same-host/same-prompt/same-method token before/after accounting with same-pair quality non-regression evidence, or return NEEDS_USER_HOST_ACTION for live Cursor UI/chat proof if the next parent-selected gap is host-visible Path B."
  evidence_supports_only = "Existing verifier evidence supports PASS for Cursor docs checklist/non-claim boundary packaging only. It supports docs_final_coverage PARTIAL, token_quality_nonhost PARTIAL, and cursor_readiness PARTIAL. It does not prove token savings, quality improvement, same-scenario joined proof, full Cursor UI/chat readiness, learnability/training, checkpoint promotion, or all-three extension readiness."
  blockers = @(
    "No token before/after reduction with same-pair quality non-regression evidence is present in this packet.",
    "No semantic quality improvement or held-out quality pass is present in this packet.",
    "No new UI/browser/screenshot/Extension Host product verification was run or claimed by this worker.",
    "No same-scenario Path A plus Path B final joined proof was run by this worker.",
    "No learnability, training, checkpoint mutation, checkpoint promotion, backup/export/import/share/continue-training, or production rollback readiness was proven by this packet.",
    "Full Cursor readiness and all-three extension readiness remain unproven."
  )
  forbidden_actions_absent_by_this_worker = [ordered]@{
    source_edits = $true
    product_commands = $true
    ui_browser_screenshot = $true
    external_ai = $true
    network = $true
    git = $true
    claude_or_codex_execution = $true
  }
  parent_will_perform_commit_push_after_review = $true
  must_stop_after_report = $true
}

$packet | ConvertTo-Json -Depth 30 | Set-Content -LiteralPath $packetJsonPath -Encoding UTF8

$md = @"
# CURSOR-DOCS-TOKEN-NONHOST-PACKET-STATE-01

- subagent_id_or_nickname: CURSOR-DOCS-TOKEN-NONHOST-PACKET-STATE-01
- docs_final_coverage_status: PARTIAL
- token_quality_nonhost_status: PARTIAL
- cursor_readiness_status: PARTIAL
- verifier_status: PASS from existing verifier output, not newly run by this worker
- state_transition_made_or_not: MADE, marker `$markerKey`
- evidence_supports_only: Existing verifier evidence supports Cursor docs/non-claim boundary packaging only. It does not prove token savings, quality improvement, full Cursor UI/chat readiness, learnability/training, checkpoint promotion, or all-three extension readiness.
- next_bounded_worker_scope_or_NEEDS_USER_HOST_ACTION: $($packet.next_bounded_worker_scope_or_NEEDS_USER_HOST_ACTION)

## Blockers

$($packet.blockers | ForEach-Object { "- $_" } | Out-String)
"@
$md | Set-Content -LiteralPath $packetMdPath -Encoding UTF8

$parsedPacket = Read-JsonFile $packetJsonPath
$tailVerification = Test-TailContains -Path $statePath -Needles @(
  $markerKey,
  '"docs_final_coverage_status"',
  '"token_quality_nonhost_status"',
  '"cursor_readiness_status"',
  '"verifier_status"',
  '"PARTIAL"',
  '"PASS"'
)

$packet.JSON_parse_or_marker_verification = [ordered]@{
  packet_json_parse = "PASS"
  parsed_schema = $parsedPacket.schema
  parsed_subagent_id_or_nickname = $parsedPacket.subagent_id_or_nickname
  state_marker_presence = $(if ($tailVerification.present) { "PASS" } else { "FAIL" })
  state_marker_key = $markerKey
  state_marker_tail_bytes_read = $tailVerification.bytes_read
  state_marker_missing_needles = $tailVerification.missing
  full_state_parse = "NOT_RUN_NOT_CLAIMED"
}

$packet | ConvertTo-Json -Depth 30 | Set-Content -LiteralPath $packetJsonPath -Encoding UTF8
$parsedFinalPacket = Read-JsonFile $packetJsonPath

$verification = [ordered]@{
  schema = "xiigen.cursor_docs_final_coverage_token_quality_nonhost_01.verification.v1"
  generated_at_utc = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
  packet_json_parse = "PASS"
  packet_path = $packetJsonPath
  parsed_schema = $parsedFinalPacket.schema
  parsed_subagent_id_or_nickname = $parsedFinalPacket.subagent_id_or_nickname
  state_marker_presence = $(if ($tailVerification.present) { "PASS" } else { "FAIL" })
  state_marker_key = $markerKey
  state_marker_tail_bytes_read = $tailVerification.bytes_read
  state_marker_missing_needles = $tailVerification.missing
  full_state_parse = "NOT_RUN_NOT_CLAIMED"
}
$verification | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $verificationPath -Encoding UTF8

if (-not $tailVerification.present) {
  throw "State marker targeted tail verification failed."
}

Write-Output "PACKET_JSON_PARSE=PASS"
Write-Output "STATE_MARKER_PRESENCE=PASS"
Write-Output "STATE_MARKER_KEY=$markerKey"
Write-Output "FULL_STATE_PARSE=NOT_RUN_NOT_CLAIMED"
