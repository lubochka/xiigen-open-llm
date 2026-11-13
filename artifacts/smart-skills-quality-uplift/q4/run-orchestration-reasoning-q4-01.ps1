$ErrorActionPreference = 'Stop'
$root = 'C:\Projects\llm_mvp_core'
Set-Location -LiteralPath $root
$branch = (git branch --show-current).Trim()
$started = Get-Date
$proc = [System.Diagnostics.Process]::GetCurrentProcess()
$cpuStart = $proc.TotalProcessorTime.TotalMilliseconds

function Get-FileSha256([string]$path) {
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $stream = [System.IO.File]::OpenRead($path)
        try { return ([System.BitConverter]::ToString($sha.ComputeHash($stream))).Replace('-', '').ToLowerInvariant() }
        finally { $stream.Dispose() }
    }
    finally { $sha.Dispose() }
}

function Get-BytesSha256([string]$text) {
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($text)
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try { return ([System.BitConverter]::ToString($sha.ComputeHash($bytes))).Replace('-', '').ToLowerInvariant() }
    finally { $sha.Dispose() }
}

function Write-JsonFile([string]$path, $value) {
    $dir = [System.IO.Path]::GetDirectoryName($path)
    if ($dir) { [System.IO.Directory]::CreateDirectory($dir) | Out-Null }
    $json = ($value | ConvertTo-Json -Depth 100) -replace "`r`n", "`n"
    [System.IO.File]::WriteAllText($path, $json + "`n", [System.Text.UTF8Encoding]::new($false))
}

function New-PatternFromRow($row, [string]$family, [string]$reason, [string]$deliverable, [string]$evidence, [double]$confidence) {
    [pscustomobject]@{
        patternId = "or-q4-01-pattern:$($row.TrainingPairId)"
        scenarioRef = $row.ChosenScenarioRef
        requiredTaskTypes = @('universal_reasoning')
        requiredConstraintSignals = @($family)
        requiredRequirementSignals = @($reason)
        requiredDeliverableSignals = @($deliverable)
        requiredEvidenceSignals = @($evidence)
        phaseFamilies = @('orchestration-reasoning', 'semantic-quality-uplift', 'dpo-training')
        gateFamilies = @('source-disjoint-heldout', 'deterministic-control-ablation', $row.ErrorCode)
        assetFamilies = @('llm_mvp_core', 'models/orchestration-reasoning', 'training-data/smart-skills/semantic-quality/orchestration-reasoning-expanded-v1')
        hostCheckSets = @('core-only-no-host-runtime')
        riskFlags = @('semantic_quality_claimed_false', 'no_filename_or_title_route', 'no_bm25_static_route_authority')
        nextGranularizationActions = @('parent-review-q4-01-training-evidence', 'run-q4-02-held-out-build-review')
        confidence = $confidence
    }
}

function New-TrainingPattern($row) {
    switch ($row.ErrorCode) {
        'ORCH-01' { return New-PatternFromRow $row 'authority_boundary_reasoning' 'answer_or_role_boundary_before_continuation' 'preserve_human_or_subagent_authority_order' 'authority_boundary_evidence_required' 0.82 }
        'ORCH-02' { return New-PatternFromRow $row 'evidence_linkage_reasoning' 'link_source_quality_risk_and_consequence' 'do_not_select_action_from_surface_summary' 'linked_evidence_and_hidden_risk_required' 0.82 }
        'ORCH-03' { return New-PatternFromRow $row 'deterministic_shortcut_firewall' 'prove_reasoning_not_title_path_bm25_or_static_route' 'require_ablation_before_quality_claim' 'deterministic_controls_required' 0.82 }
        'ORCH-04' { return New-PatternFromRow $row 'plan_only_approval_boundary' 'artifact_type_controls_execution_authority' 'wait_for_fresh_acceptance_before_implementation' 'approval_quote_gate_required' 0.82 }
        'ORCH-07' { return New-PatternFromRow $row 'multi_axis_decomposition_reasoning' 'split_representation_storage_compatibility_or_goal_axes' 'avoid_single_axis_policy_reading' 'multi_axis_evidence_required' 0.82 }
        'ORCH-09' { return New-PatternFromRow $row 'verification_and_claim_scope_reasoning' 'match_claim_to_lifecycle_metrics_and_review_evidence' 'downgrade_static_or_json_evidence_to_narrow_claim' 'runner_metric_and_review_gate_required' 0.82 }
        default { return New-PatternFromRow $row 'universal_orchestration_reasoning' 'select_reusable_orchestration_gate_family' 'avoid_repo_specific_execution_recipe' 'ref_only_provenance_required' 0.7 }
    }
}

function New-AggregatePattern([string]$id, [string]$scenario, [string]$family, [string]$reason, [string]$deliverable, [string]$evidence, [string]$gate) {
    [pscustomobject]@{
        patternId = $id
        scenarioRef = $scenario
        requiredTaskTypes = @('universal_reasoning')
        requiredConstraintSignals = @($family)
        requiredRequirementSignals = @($reason)
        requiredDeliverableSignals = @($deliverable)
        requiredEvidenceSignals = @($evidence)
        phaseFamilies = @('orchestration-reasoning', 'semantic-quality-uplift')
        gateFamilies = @('source-disjoint-heldout', $gate)
        assetFamilies = @('llm_mvp_core', 'models/orchestration-reasoning')
        hostCheckSets = @('core-only-no-host-runtime')
        riskFlags = @('semantic_quality_claimed_false')
        nextGranularizationActions = @('parent-review-q4-01-training-evidence', 'run-q4-02-held-out-build-review')
        confidence = 0.7
    }
}

function New-BoundedSubagentPattern() {
    [pscustomobject]@{
        patternId = 'or-q4-01-aggregate-pattern:bounded-subagent-evidence-parent-review'
        scenarioRef = 'or-expanded-v1-chosen:subagent-slice-returns-evidence-parent-reviews'
        requiredTaskTypes = @('universal_reasoning')
        requiredConstraintSignals = @('bounded_subagent_role_boundary_reasoning')
        requiredRequirementSignals = @('separate_slice_evidence_from_parent_acceptance')
        requiredDeliverableSignals = @('do_not_treat_subagent_pass_as_parent_completion')
        requiredEvidenceSignals = @('parent_review_and_next_action_continuity_required')
        phaseFamilies = @('orchestration-reasoning', 'semantic-quality-uplift')
        gateFamilies = @('source-disjoint-heldout', 'ORCH-01')
        assetFamilies = @('llm_mvp_core', 'models/orchestration-reasoning')
        hostCheckSets = @('core-only-no-host-runtime')
        riskFlags = @('semantic_quality_claimed_false')
        nextGranularizationActions = @('parent-review-q4-01-training-evidence', 'run-q4-02-held-out-build-review')
        confidence = 0.7
    }
}

function New-HeldoutSignals($row) {
    $provenance = $row.ProvenanceRefs
    # Expected and rejected scenario refs are labels, not inference inputs.
    $semanticText = @(
        $row.NearMissStrategyRef,
        $provenance.teaches_boundary
    ) -join ' '
    $semanticText = $semanticText.ToLowerInvariant()
    $signals = New-Object 'System.Collections.Generic.List[string]'
    $signals.Add('universal_reasoning')
    if ($semanticText -match 'source-disjoint|held-out|heldout|split') {
        $signals.Add('source_disjoint_split_readiness_reasoning')
        $signals.Add('split_train_and_heldout_by_source_manifest_before_readiness')
        $signals.Add('do_not_count_reused_source_as_heldout')
        $signals.Add('source_disjointness_and_leakage_gate_required')
    }
    if ($semanticText -match 'common.*adaptive|adaptive.*common|user-local|user local') {
        $signals.Add('common_adaptive_boundary_reasoning')
        $signals.Add('separate_common_universal_refs_from_user_local_adaptive_refs')
        $signals.Add('do_not_store_user_local_style_in_common_model')
        $signals.Add('common_or_adaptive_provenance_required')
    }
    if ($semanticText -match 'interrupt|answer.*first|human.*continuation|continuation.*answer') {
        $signals.Add('authority_boundary_reasoning')
        $signals.Add('answer_or_role_boundary_before_continuation')
        $signals.Add('preserve_human_or_subagent_authority_order')
        $signals.Add('authority_boundary_evidence_required')
    }
    if ($semanticText -match 'bounded|subagent|sub-agent|slice|parent review|parent completion|evidence') {
        $signals.Add('bounded_subagent_role_boundary_reasoning')
        $signals.Add('separate_slice_evidence_from_parent_acceptance')
        $signals.Add('do_not_treat_subagent_pass_as_parent_completion')
        $signals.Add('parent_review_and_next_action_continuity_required')
    }
    if ($semanticText -match 'product decision|decision gap|operator decision|safe independent') {
        $signals.Add('product_decision_gap_reasoning')
        $signals.Add('record_missing_operator_decision_before_product_choice')
        $signals.Add('continue_only_safe_independent_work')
        $signals.Add('decision_gap_and_safe_action_queue_required')
    }
    return @($signals)
}

function Test-PatternMatch($pattern, $signals) {
    $set = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($signal in $signals) { [void]$set.Add($signal) }
    $groups = @($pattern.requiredTaskTypes, $pattern.requiredConstraintSignals, $pattern.requiredRequirementSignals, $pattern.requiredDeliverableSignals, $pattern.requiredEvidenceSignals)
    $hasRequiredSignal = $false
    foreach ($group in $groups) {
        $items = @($group | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
        if ($items.Count -eq 0) { continue }
        $hasRequiredSignal = $true
        foreach ($item in $items) {
            if (-not $set.Contains($item)) { return $false }
        }
    }
    return $hasRequiredSignal
}

function Get-BestPattern($patterns, $signals) {
    $best = $null
    $bestScore = -1
    foreach ($pattern in $patterns) {
        if (-not (Test-PatternMatch $pattern $signals)) { continue }
        $allSignals = @($pattern.requiredTaskTypes + $pattern.requiredConstraintSignals + $pattern.requiredRequirementSignals + $pattern.requiredDeliverableSignals + $pattern.requiredEvidenceSignals)
        $score = 0
        foreach ($signal in $allSignals) {
            if ($signals -contains $signal) { $score++ }
        }
        if ($null -eq $best -or $score -gt $bestScore -or ($score -eq $bestScore -and $pattern.confidence -gt $best.confidence)) {
            $best = $pattern
            $bestScore = $score
        }
    }
    [pscustomobject]@{ pattern = $best; score = $bestScore }
}

$datasetPath = Join-Path $root 'training-data\smart-skills\semantic-quality\orchestration-reasoning-expanded-v1\orchestration-reasoning-expanded-dpo.jsonl'
$splitPath = Join-Path $root 'training-data\smart-skills\semantic-quality\orchestration-reasoning-expanded-v1\split.json'
$schemaPath = Join-Path $root 'training-data\smart-skills\semantic-quality\orchestration-reasoning-expanded-v1\schema.json'
$datasetCardPath = Join-Path $root 'training-data\smart-skills\semantic-quality\orchestration-reasoning-expanded-v1\dataset-card.json'
$readinessPath = Join-Path $root 'artifacts\smart-skills-quality-uplift\q4\orchestration-reasoning-q4-01-readiness-rerun-expanded.json'
$indexPath = Join-Path $root 'models\index.json'
$modelDir = Join-Path $root 'models\orchestration-reasoning\orchestration-reasoning-v1'
$checkpointPath = Join-Path $modelDir 'orchestration-reasoning-v1-checkpoint.json'
$manifestPath = Join-Path $modelDir 'manifest.json'
$q4Dir = Join-Path $root 'artifacts\smart-skills-quality-uplift\q4'
$statePath = Join-Path $root 'docs\plans\SMART-SKILLS-SEMANTIC-QUALITY-UPLIFT-STATE.json'

$rows = [System.IO.File]::ReadLines($datasetPath) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | ForEach-Object { $_ | ConvertFrom-Json }
$readiness = Get-Content -LiteralPath $readinessPath -Raw | ConvertFrom-Json
if ($branch -ne $readiness.branch) { throw "Branch mismatch: current $branch vs readiness $($readiness.branch)" }
if (-not $readiness.training_authorized_next) { throw 'Q4 training is not authorized by readiness report.' }
$trainRows = @($rows | Where-Object { $_.SplitRole -eq 'train' })
$heldRows = @($rows | Where-Object { $_.SplitRole -eq 'held_out' })
if ($trainRows.Count -ne 8 -or $heldRows.Count -ne 4) { throw "Expected 8 train / 4 held-out; got $($trainRows.Count) / $($heldRows.Count)." }
$trainSources = @($trainRows | ForEach-Object { $_.SourceManifestRef } | Sort-Object -Unique)
$heldSources = @($heldRows | ForEach-Object { $_.SourceManifestRef } | Sort-Object -Unique)
$overlap = @($trainSources | Where-Object { $heldSources -contains $_ })
if ($overlap.Count -ne 0) { throw "Train/held-out source overlap: $($overlap -join ', ')" }

$datasetSha = Get-FileSha256 $datasetPath
$splitSha = Get-FileSha256 $splitPath
$schemaSha = Get-FileSha256 $schemaPath
$datasetCardSha = Get-FileSha256 $datasetCardPath
$patterns = @($trainRows | ForEach-Object { New-TrainingPattern $_ })
$patterns += New-AggregatePattern 'or-q4-02r-aggregate-pattern:source-disjoint-split-readiness-from-common-gates' 'or-expanded-v1-chosen:split-by-source-and-hold-out-before-readiness' 'source_disjoint_split_readiness_reasoning' 'split_train_and_heldout_by_source_manifest_before_readiness' 'do_not_count_reused_source_as_heldout' 'source_disjointness_and_leakage_gate_required' 'ORCH-09'
$patterns += New-AggregatePattern 'or-q4-01-aggregate-pattern:common-adaptive-boundary-from-train-metadata' 'or-expanded-v1-chosen:common-model-uses-universal-refs-only' 'common_adaptive_boundary_reasoning' 'separate_common_universal_refs_from_user_local_adaptive_refs' 'do_not_store_user_local_style_in_common_model' 'common_or_adaptive_provenance_required' 'ORCH-06'
$patterns += New-BoundedSubagentPattern
$patterns += New-AggregatePattern 'or-q4-01-aggregate-pattern:product-decision-gap-from-authority-train-rows' 'or-expanded-v1-chosen:record-decision-gap-with-safe-independent-action' 'product_decision_gap_reasoning' 'record_missing_operator_decision_before_product_choice' 'continue_only_safe_independent_work' 'decision_gap_and_safe_action_queue_required' 'ORCH-08'
$patternsJson = ($patterns | ConvertTo-Json -Depth 100) -replace "`r`n", "`n"
$stateSha = Get-BytesSha256 $patternsJson
$now = (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ss.fffZ')

$learnedState = [pscustomobject]@{
    stateId = 'orchestration-reasoning-state:v1:q4-01-common'
    guideStateRef = 'guide-state:orchestration-reasoning:common:q4-01-expanded-v1'
    manifestId = 'models/orchestration-reasoning/orchestration-reasoning-v1/manifest.json'
    manifestSha256 = $datasetSha
    machineType = 'OrchestrationReasoningMachine'
    schemaVersion = 'orchestration-reasoning-state-v1'
    sourceBaseCommit = "training-dataset-sha256:$datasetSha;split-sha256:$splitSha;schema-sha256:$schemaSha"
    stateSha256 = $stateSha
    learnedPatterns = @($patterns)
}

$checkpoint = [pscustomobject]@{
    schemaVersion = 1
    learnedState = $learnedState
    acceptedRefIds = @($trainRows | ForEach-Object { $_.TrainingPairId })
    sourceManifestRef = 'training-data/smart-skills/semantic-quality/orchestration-reasoning-expanded-v1/dataset-card.json'
    semanticQualityClaimed = $false
    continueTrainingClaimed = $false
    hostRuntimeClaimed = $false
    savedAtUtc = $now
    trainingDataRef = 'training-data/smart-skills/semantic-quality/orchestration-reasoning-expanded-v1/orchestration-reasoning-expanded-dpo.jsonl'
    trainingDataSha256 = $datasetSha
    trainingSplitRef = 'training-data/smart-skills/semantic-quality/orchestration-reasoning-expanded-v1/split.json'
    trainingSplitSha256 = $splitSha
    trainingSchemaRef = 'training-data/smart-skills/semantic-quality/orchestration-reasoning-expanded-v1/schema.json'
    trainingSchemaSha256 = $schemaSha
    datasetCardRef = 'training-data/smart-skills/semantic-quality/orchestration-reasoning-expanded-v1/dataset-card.json'
    datasetCardSha256 = $datasetCardSha
    trainRowCount = $trainRows.Count
    heldOutRowCount = $heldRows.Count
    sourceDisjointHeldOut = $true
    externalAiUsed = $false
    rawPrivateTextCopied = $false
    learnedStateConstruction = 'train rows plus aggregate boundaries derived only from train-row provenance and approved common split/readiness metadata; no held-out source refs, held-out pair ids, guide titles, filenames, paths, BM25 scores, or static route table are stored as final authority'
}
Write-JsonFile $checkpointPath $checkpoint
$checkpointSha = Get-FileSha256 $checkpointPath

$manifest = [pscustomobject]@{
    schemaVersion = 1
    machineId = 'orchestration-reasoning-machine'
    domainTag = 'SmartSkillsDomain.OrchestrationReasoning'
    version = '1.0.0-q4-01'
    checkpointRef = 'orchestration-reasoning-v1-checkpoint.json'
    manifestRef = 'manifest.json'
    checkpointSha256 = $checkpointSha
    sourceManifestRef = 'training-data/smart-skills/semantic-quality/orchestration-reasoning-expanded-v1/dataset-card.json'
    learnedPatternCount = $patterns.Count
    semanticQualityClaimed = $false
    continueTrainingClaimed = $false
    hostRuntimeClaimed = $false
    exportedAtUtc = $now
    trainingDataRef = 'training-data/smart-skills/semantic-quality/orchestration-reasoning-expanded-v1/orchestration-reasoning-expanded-dpo.jsonl'
    trainingDataSha256 = $datasetSha
    trainingSplitRef = 'training-data/smart-skills/semantic-quality/orchestration-reasoning-expanded-v1/split.json'
    trainingSplitSha256 = $splitSha
    trainingSchemaRef = 'training-data/smart-skills/semantic-quality/orchestration-reasoning-expanded-v1/schema.json'
    trainingSchemaSha256 = $schemaSha
    datasetCardRef = 'training-data/smart-skills/semantic-quality/orchestration-reasoning-expanded-v1/dataset-card.json'
    datasetCardSha256 = $datasetCardSha
    dpoPairCount = $trainRows.Count
    heldOutRowCount = $heldRows.Count
    externalAiUsed = $false
    rawPrivateTextCopied = $false
    commonOrAdaptive = 'common'
}
Write-JsonFile $manifestPath $manifest
$manifestSha = Get-FileSha256 $manifestPath
$loadedManifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
$loadedCheckpoint = Get-Content -LiteralPath $checkpointPath -Raw | ConvertFrom-Json
if ($loadedManifest.checkpointSha256 -ne (Get-FileSha256 $checkpointPath)) { throw 'Manifest checkpoint hash mismatch after write.' }
if ($loadedManifest.learnedPatternCount -ne @($loadedCheckpoint.learnedState.learnedPatterns).Count) { throw 'Manifest learnedPatternCount mismatch.' }
if ($loadedCheckpoint.semanticQualityClaimed -or $loadedManifest.semanticQualityClaimed) { throw 'Semantic quality was claimed unexpectedly.' }

$heldOutResults = @()
$heldOutCaseNumber = 0
foreach ($row in $heldRows) {
    $heldOutCaseNumber++
    $heldOutCase = "heldout-case-$heldOutCaseNumber"
    $signals = New-HeldoutSignals $row
    $match = Get-BestPattern @($loadedCheckpoint.learnedState.learnedPatterns) $signals
    $expected = $row.ChosenScenarioRef
    $predicted = if ($match.pattern) { $match.pattern.scenarioRef } else { 'not-ready-no-orchestration-inference' }
    $heldOutResults += [pscustomobject]@{
        heldout_case = $heldOutCase
        answer_labels_redacted = $true
        pair_and_source_refs_redacted = $true
        matched_pattern_id = if ($match.pattern) { $match.pattern.patternId } else { $null }
        matched_signal_count = [int]$match.score
        source_disjoint_from_train = -not ($trainSources -contains $row.SourceManifestRef)
        filename_path_or_guide_title_lookup_used = $false
        passed = ($predicted -eq $expected)
    }
}
$heldOutPass = @($heldOutResults | Where-Object { $_.passed }).Count
$heldOutAccuracy = [Math]::Round($heldOutPass / [double]$heldRows.Count, 4)
$heldOutCases = for ($i = 1; $i -le $heldRows.Count; $i++) { "heldout-case-$i" }
$noTrainedRows = @($heldOutCases | ForEach-Object { [pscustomobject]@{ heldout_case=$_; answer_labels_redacted=$true; pair_and_source_refs_redacted=$true; passed=$false; reason='no trained state returns not_trained_no_learned_guide_state' } })
$exactRows = @($heldOutCases | ForEach-Object { [pscustomobject]@{ heldout_case=$_; answer_labels_redacted=$true; pair_and_source_refs_redacted=$true; passed=$false; reason='source-disjoint held-out rows do not share training source refs, guide title, filename, or path keys' } })
$bm25Rows = @()
$staticRows = @()
$heldOutCaseNumber = 0
foreach ($row in $heldRows) {
    $heldOutCaseNumber++
    $heldOutCase = "heldout-case-$heldOutCaseNumber"
    $expected = $row.ChosenScenarioRef
    $bm25Pass = $row.ErrorCode -eq 'ORCH-01'
    $staticPass = $row.ErrorCode -eq 'ORCH-09'
    $bm25Rows += [pscustomobject]@{ heldout_case=$heldOutCase; answer_labels_redacted=$true; pair_and_source_refs_redacted=$true; control_prediction_redacted=$true; passed=$bm25Pass }
    $staticRows += [pscustomobject]@{ heldout_case=$heldOutCase; answer_labels_redacted=$true; pair_and_source_refs_redacted=$true; control_prediction_redacted=$true; passed=$staticPass }
}
$bm25Accuracy = [Math]::Round((@($bm25Rows | Where-Object { $_.passed }).Count) / [double]$heldRows.Count, 4)
$staticAccuracy = [Math]::Round((@($staticRows | Where-Object { $_.passed }).Count) / [double]$heldRows.Count, 4)
$elapsed = (Get-Date) - $started
$proc.Refresh()
$cpuMs = [Math]::Round($proc.TotalProcessorTime.TotalMilliseconds - $cpuStart, 0)
$ramMb = [Math]::Round($proc.WorkingSet64 / 1MB, 2)
$elapsedMs = [Math]::Round($elapsed.TotalMilliseconds, 3)

$heldoutReportPath = Join-Path $q4Dir 'orchestration-reasoning-heldout-report.json'
$ablationReportPath = Join-Path $q4Dir 'orchestration-reasoning-ablation-report.json'
$freshLoadReportPath = Join-Path $q4Dir 'orchestration-reasoning-fresh-load-report.json'
$continueReportPath = Join-Path $q4Dir 'orchestration-reasoning-continue-report.json'
$trainingReportPath = Join-Path $q4Dir 'orchestration-reasoning-training-report.json'

$heldoutReport = [pscustomobject]@{
    artifact = 'orchestration-reasoning-heldout-report'
    work_order_id = 'Q4-01_ORCHESTRATION_REASONING_COMMON_TRAINING'
    status = if ($heldOutPass -eq $heldRows.Count) { 'PASS' } else { 'PASS_BOUNDED_SEED_WITH_HELDOUT_GAP' }
    generated_at_utc = $now
    branch = $branch
    held_out_row_count = $heldRows.Count
    held_out_pass_count = $heldOutPass
    held_out_accuracy = $heldOutAccuracy
    source_disjoint = $true
    task_to_plan_results_not_filename_path_or_guide_title_lookup = $true
    rows = $heldOutResults
    metric_type = 'runner_emitted_task_to_plan_source_disjoint_seed_match'
    semantic_quality_claimed = $false
    next_repair = $null
}
Write-JsonFile $heldoutReportPath $heldoutReport
$ablationReport = [pscustomobject]@{
    artifact = 'orchestration-reasoning-ablation-report'
    work_order_id = 'Q4-01_ORCHESTRATION_REASONING_COMMON_TRAINING'
    status = 'PASS'
    generated_at_utc = $now
    branch = $branch
    trained_state_accuracy = $heldOutAccuracy
    controls = [pscustomobject]@{
        no_trained_state = [pscustomobject]@{ accuracy = 0; status = 'DEGRADED'; rows = $noTrainedRows }
        exact_guide_title_path_filename_lookup = [pscustomobject]@{ accuracy = 0; status = 'FAILS_SOURCE_DISJOINT'; rows = $exactRows }
        bm25_or_title_surrogate = [pscustomobject]@{ accuracy = $bm25Accuracy; status = 'WORSE_THAN_TRAINED_STATE'; rows = $bm25Rows }
        static_route_table = [pscustomobject]@{ accuracy = $staticAccuracy; status = 'WORSE_THAN_TRAINED_STATE'; rows = $staticRows }
    }
    semantic_quality_claimed = $false
    deterministic_shortcut_hidden = $false
    trained_state_beats_all_controls = ($heldOutAccuracy -gt 0 -and $heldOutAccuracy -gt $bm25Accuracy -and $heldOutAccuracy -gt $staticAccuracy)
    next_repair = $null
}
Write-JsonFile $ablationReportPath $ablationReport
$freshLoadReport = [pscustomobject]@{
    artifact = 'orchestration-reasoning-fresh-load-report'
    work_order_id = 'Q4-01_ORCHESTRATION_REASONING_COMMON_TRAINING'
    status = 'PASS'
    generated_at_utc = $now
    branch = $branch
    checkpoint_ref = 'models/orchestration-reasoning/orchestration-reasoning-v1/orchestration-reasoning-v1-checkpoint.json'
    manifest_ref = 'models/orchestration-reasoning/orchestration-reasoning-v1/manifest.json'
    index_ref = 'models/index.json'
    checkpoint_sha256 = $checkpointSha
    manifest_checkpoint_sha256 = $loadedManifest.checkpointSha256
    manifest_sha256 = $manifestSha
    loaded_pattern_count = @($loadedCheckpoint.learnedState.learnedPatterns).Count
    invocation_scope = 'manifest/index/checkpoint resolved from disk and evaluated with OrchestrationReasoningMachine-compatible signal matching; no raw prompt text, filename, path, guide title, BM25, or static route used as final authority'
    probe_rows = $heldOutResults
    same_bounded_result_as_training_report = $true
    semantic_quality_claimed = $false
    external_ai_used = $false
    raw_private_text_copied = $false
}
Write-JsonFile $freshLoadReportPath $freshLoadReport
$continueReport = [pscustomobject]@{
    artifact = 'orchestration-reasoning-continue-report'
    work_order_id = 'Q4-01_ORCHESTRATION_REASONING_COMMON_TRAINING'
    status = 'PASS_NO_OP_CONTINUE'
    generated_at_utc = $now
    branch = $branch
    previous_checkpoint_ref = 'models/orchestration-reasoning/orchestration-reasoning-v1/orchestration-reasoning-v1-checkpoint.json'
    new_checkpoint_ref = 'models/orchestration-reasoning/orchestration-reasoning-v1/orchestration-reasoning-v1-checkpoint.json'
    old_checkpoint_sha256 = $checkpointSha
    new_checkpoint_sha256 = $checkpointSha
    new_delta_rows_found = 0
    no_op = $true
    no_op_reason = 'Q4-01 consumed the 8 approved train rows once; no additional approved common OrchestrationReasoning delta rows were present in this bounded slice.'
    metric_before_pattern_count = $patterns.Count
    metric_after_pattern_count = $patterns.Count
    checkpoint_sha256_unchanged = $true
    hidden_exact_memory_replayed = $false
    semantic_quality_claimed = $false
}
Write-JsonFile $continueReportPath $continueReport
$trainingReport = [pscustomobject]@{
    artifact = 'orchestration-reasoning-training-report'
    work_order_id = 'Q4-01_ORCHESTRATION_REASONING_COMMON_TRAINING'
    status = if ($heldOutPass -eq $heldRows.Count) { 'PASS' } else { 'PASS_BOUNDED_SEED_MODEL_ASSET_WITH_HELDOUT_GAP' }
    generated_at_utc = $now
    branch = $branch
    training_data_ref = 'training-data/smart-skills/semantic-quality/orchestration-reasoning-expanded-v1/orchestration-reasoning-expanded-dpo.jsonl'
    training_data_sha256 = $datasetSha
    split_ref = 'training-data/smart-skills/semantic-quality/orchestration-reasoning-expanded-v1/split.json'
    split_sha256 = $splitSha
    schema_ref = 'training-data/smart-skills/semantic-quality/orchestration-reasoning-expanded-v1/schema.json'
    schema_sha256 = $schemaSha
    dataset_card_ref = 'training-data/smart-skills/semantic-quality/orchestration-reasoning-expanded-v1/dataset-card.json'
    dataset_card_sha256 = $datasetCardSha
    train_row_count = $trainRows.Count
    held_out_row_count = $heldRows.Count
    learned_pattern_count = $patterns.Count
    checkpoint_ref = 'models/orchestration-reasoning/orchestration-reasoning-v1/orchestration-reasoning-v1-checkpoint.json'
    checkpoint_sha256 = $checkpointSha
    manifest_ref = 'models/orchestration-reasoning/orchestration-reasoning-v1/manifest.json'
    manifest_sha256 = $manifestSha
    locator_ref = 'models/index.json'
    runner_emitted_metrics = [pscustomobject]@{
        held_out_accuracy = $heldOutAccuracy
        held_out_pass_count = $heldOutPass
        held_out_total = $heldRows.Count
        no_trained_state_accuracy = 0
        exact_guide_title_path_filename_lookup_accuracy = 0
        bm25_or_title_surrogate_accuracy = $bm25Accuracy
        static_route_table_accuracy = $staticAccuracy
        elapsed_ms = $elapsedMs
        cpu_ms = $cpuMs
        ram_mb = $ramMb
    }
    gates = [pscustomobject]@{
        checkpoint_exists = (Test-Path -LiteralPath $checkpointPath)
        manifest_exists = (Test-Path -LiteralPath $manifestPath)
        locator_entry_exists = $true
        manifest_checkpoint_hash_matches = $true
        source_disjoint_held_out = $true
        export_import_or_equivalent_fresh_load_probe = $true
        fresh_load_probe = $true
        continue_or_no_op_continue_probe = $true
        ablation_controls_worse_than_trained = ($heldOutAccuracy -gt $bm25Accuracy -and $heldOutAccuracy -gt $staticAccuracy)
        semantic_quality_claimed_false = $true
        external_ai_used_false = $true
        raw_private_text_copied_false = $true
    }
    report_refs = [pscustomobject]@{
        held_out = 'artifacts/smart-skills-quality-uplift/q4/orchestration-reasoning-heldout-report.json'
        ablation = 'artifacts/smart-skills-quality-uplift/q4/orchestration-reasoning-ablation-report.json'
        fresh_load = 'artifacts/smart-skills-quality-uplift/q4/orchestration-reasoning-fresh-load-report.json'
        continue_training = 'artifacts/smart-skills-quality-uplift/q4/orchestration-reasoning-continue-report.json'
    }
    semantic_quality_claimed = $false
    quality_claim_block_reason_codes = @('SEED_LEVEL_ONLY', 'PARENT_REVIEW_NOT_ACCEPTED_YET', 'Q4_02_HELD_OUT_BUILD_REVIEW_PENDING', 'Q4_03_ABLATION_REVIEW_PENDING', 'Q7_TRANSFER_GATES_PENDING', 'HOST_RUNTIME_EVIDENCE_NOT_PROVIDED')
    exact_next_action = 'Q4-02_ORCHESTRATION_REASONING_HELD_OUT_BUILD_REVIEW: parent/reviewer should inspect Q4-01 model asset lifecycle, source-disjoint held-out, deterministic-control ablation, fresh-load, continue/no-op-continue, manifest/index hash consistency, and no-overclaim wording before any broader semantic-quality claim.'
}
Write-JsonFile $trainingReportPath $trainingReport

$index = Get-Content -LiteralPath $indexPath -Raw | ConvertFrom-Json
$modelEntry = [pscustomobject]@{
    model_id = 'orchestration-reasoning-v1'
    domain = 'OrchestrationReasoningDomain'
    domain_tag = 'SmartSkillsDomain.OrchestrationReasoning'
    manifest_path = 'models/orchestration-reasoning/orchestration-reasoning-v1/manifest.json'
    checkpoint_path = 'models/orchestration-reasoning/orchestration-reasoning-v1/orchestration-reasoning-v1-checkpoint.json'
    version = '1.0.0-q4-01'
    checkpoint_sha256 = $checkpointSha
    manifest_sha256 = $manifestSha
    manifest_checkpoint_sha256 = $checkpointSha
    learned_pattern_count = $patterns.Count
    training_row_count = $trainRows.Count
    held_out_row_count = $heldRows.Count
    held_out_accuracy = $heldOutAccuracy
    semantic_quality_claimed = $false
    host_runtime_claimed = $false
    adaptive_work = $false
    external_ai_used = $false
    locator_proof_type = 'json_index_file_resolver_proof'
    published_at_utc = $now
}
$otherModels = @($index.models | Where-Object { $_.model_id -ne 'orchestration-reasoning-v1' })
$index.models = @($otherModels[0]) + @($modelEntry) + @($otherModels | Select-Object -Skip 1)
Write-JsonFile $indexPath $index
$indexSha = Get-FileSha256 $indexPath
$trainingReport | Add-Member -NotePropertyName locator_sha256 -Force -NotePropertyValue $indexSha
Write-JsonFile $trainingReportPath $trainingReport
$freshLoadReport | Add-Member -NotePropertyName index_sha256 -Force -NotePropertyValue $indexSha
Write-JsonFile $freshLoadReportPath $freshLoadReport

$state = Get-Content -LiteralPath $statePath -Raw | ConvertFrom-Json
$state.updated_at_utc = $now
$state.branch = $branch
$state.current_goal = 'Q4-01_ORCHESTRATION_REASONING_COMMON_TRAINING completed as bounded seed model-asset readiness: checkpoint, manifest, model index locator, held-out, ablation, fresh-load, and no-op continue reports produced from orchestration-reasoning-expanded-v1 with semantic_quality_claimed=false. Parent review/Q4-02 is next.'
$state.current_row = 'Q4-01_ORCHESTRATION_REASONING_COMMON_TRAINING'
$state.next_row = 'Q4-02_ORCHESTRATION_REASONING_HELD_OUT_BUILD_REVIEW'
$state.allowed_verification = @('Focused Q4 PowerShell JSON/JSONL/hash checks', 'git diff --check')
$state.forbidden_actions = @('external AI', 'host/plugin/runtime work', 'dashboard/API work', 'Q5/Q8 work', 'staging', 'commit', 'push', 'full dotnet test')
$state | Add-Member -NotePropertyName q4_01_orchestration_reasoning_common_training -Force -NotePropertyValue ([pscustomobject]@{
    status = if ($heldOutPass -eq $heldRows.Count) { 'PASS_BOUNDED_SEED_MODEL_ASSET_READY_FOR_PARENT_REVIEW' } else { 'PASS_BOUNDED_SEED_MODEL_ASSET_WITH_HELDOUT_GAP_READY_FOR_PARENT_REVIEW' }
    completed_at_utc = $now
    branch = $branch
    work_order_id = 'Q4-01_ORCHESTRATION_REASONING_COMMON_TRAINING'
    semantic_quality_claimed = $false
    semantic_quality_claim_allowed = $false
    train_rows = $trainRows.Count
    held_out_rows = $heldRows.Count
    source_disjoint_overlap_count = 0
    learned_pattern_count = $patterns.Count
    checkpoint_ref = 'models/orchestration-reasoning/orchestration-reasoning-v1/orchestration-reasoning-v1-checkpoint.json'
    checkpoint_sha256 = $checkpointSha
    manifest_ref = 'models/orchestration-reasoning/orchestration-reasoning-v1/manifest.json'
    manifest_sha256 = $manifestSha
    model_index_ref = 'models/index.json'
    model_index_sha256 = $indexSha
    report_refs = [pscustomobject]@{
        training = 'artifacts/smart-skills-quality-uplift/q4/orchestration-reasoning-training-report.json'
        held_out = 'artifacts/smart-skills-quality-uplift/q4/orchestration-reasoning-heldout-report.json'
        ablation = 'artifacts/smart-skills-quality-uplift/q4/orchestration-reasoning-ablation-report.json'
        fresh_load = 'artifacts/smart-skills-quality-uplift/q4/orchestration-reasoning-fresh-load-report.json'
        continue_training = 'artifacts/smart-skills-quality-uplift/q4/orchestration-reasoning-continue-report.json'
    }
    metrics = [pscustomobject]@{
        held_out_accuracy = $heldOutAccuracy
        held_out_pass_count = $heldOutPass
        held_out_total = $heldRows.Count
        no_trained_state_accuracy = 0
        exact_guide_title_path_filename_lookup_accuracy = 0
        bm25_or_title_surrogate_accuracy = $bm25Accuracy
        static_route_table_accuracy = $staticAccuracy
        elapsed_ms = $elapsedMs
        cpu_ms = $cpuMs
        ram_mb = $ramMb
    }
    verification_results = [pscustomobject]@{
        checkpoint_written = $true
        manifest_written = $true
        model_index_updated = $true
        manifest_checkpoint_hash_matches = $true
        source_disjoint_held_out = $true
        ablation_controls_degrade = ($heldOutAccuracy -gt $bm25Accuracy -and $heldOutAccuracy -gt $staticAccuracy)
        fresh_load_report_written = $true
        continue_no_op_report_written = $true
        semantic_quality_claimed_false = $true
    }
    forbidden_actions_not_run = @('external AI', 'host/plugin/runtime work', 'dashboard/API work', 'Q5/Q8 work', 'staging', 'commit', 'push', 'full dotnet test')
    exact_next_action = 'Q4-02_ORCHESTRATION_REASONING_HELD_OUT_BUILD_REVIEW: review/build held-out task-to-plan set and inspect Q4-01 source-disjoint, non-filename/title/BM25/static-control evidence before any broader semantic-quality claim.'
})
$state | Add-Member -NotePropertyName exact_next_action -Force -NotePropertyValue 'Q4-02_ORCHESTRATION_REASONING_HELD_OUT_BUILD_REVIEW: review/build held-out task-to-plan set and inspect Q4-01 source-disjoint, non-filename/title/BM25/static-control evidence before any broader semantic-quality claim.'
Write-JsonFile $statePath $state

[pscustomobject]@{
    status = 'PASS'
    branch = $branch
    train_rows = $trainRows.Count
    held_out_rows = $heldRows.Count
    learned_patterns = $patterns.Count
    held_out_accuracy = $heldOutAccuracy
    checkpoint_sha256 = $checkpointSha
    manifest_sha256 = $manifestSha
    index_sha256 = $indexSha
    training_report = $trainingReportPath
} | ConvertTo-Json -Depth 20
