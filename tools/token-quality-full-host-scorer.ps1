param(
    [string]$InputPath = "artifacts/extension-runtime-token-quality-learnability/token-03-gap-repair-merged-quality-scoring-inputs.jsonl",
    [string]$PairInputPath = "artifacts/extension-runtime-token-quality-learnability/token-03-gap-repair-merged-pair-results.jsonl",
    [string]$OutputDir = "artifacts/extension-runtime-token-quality-learnability",
    [string]$StatePath = "docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json",
    [string]$PhaseId = "TOKEN-04-QUALITY-SCORING-FULL-HOSTS",
    [string]$OutputPrefix = "token-04-quality-scoring-full-hosts",
    [string]$NextBoundedWorkOrder = "QUALITY-GAP-REPAIR-01: repair the quality blockers before token savings or semantic-quality claims. Start with OrchestrationReasoning manifest/checkpoint mismatch, PromptToTask no-matching-pattern coverage, SkillSelection empty/second-pass output, and real user-local adaptive model effect; then rerun TOKEN-04 scoring.",
    [switch]$WriteState
)

$ErrorActionPreference = "Stop"

$RowScoresName = "$OutputPrefix-row-scores.jsonl"
$PairScoresName = "$OutputPrefix-pair-results.jsonl"
$ReportJsonName = "$OutputPrefix-report.json"
$ReportMdName = "$OutputPrefix-report.md"
$ValidationName = "$OutputPrefix.validation.json"

function Resolve-FullPath {
    param([Parameter(Mandatory = $true)][string]$Path)

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return [System.IO.Path]::GetFullPath($Path)
    }

    return [System.IO.Path]::GetFullPath((Join-Path (Get-Location).Path $Path))
}

function Write-Utf8NoBom {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Text
    )

    $parent = Split-Path -Parent $Path
    if ($parent -and -not (Test-Path -LiteralPath $parent)) {
        New-Item -ItemType Directory -Force -Path $parent | Out-Null
    }

    $normalized = $Text.Replace("`r`n", "`n").Replace("`r", "`n")
    if (-not $normalized.EndsWith("`n")) {
        $normalized += "`n"
    }

    [System.IO.File]::WriteAllText($Path, $normalized, [System.Text.UTF8Encoding]::new($false))
}

function ConvertTo-CompactJson {
    param([Parameter(Mandatory = $true)]$Value)
    return $Value | ConvertTo-Json -Depth 100 -Compress
}

function ConvertTo-PrettyJson {
    param([Parameter(Mandatory = $true)]$Value)
    return $Value | ConvertTo-Json -Depth 100
}

function Read-Jsonl {
    param([Parameter(Mandatory = $true)][string]$Path)

    $fullPath = Resolve-FullPath $Path
    if (-not (Test-Path -LiteralPath $fullPath)) {
        throw "JSONL input does not exist: $fullPath"
    }

    $rows = @()
    $lineNumber = 0
    foreach ($line in [System.IO.File]::ReadLines($fullPath, [System.Text.Encoding]::UTF8)) {
        $lineNumber++
        if ([string]::IsNullOrWhiteSpace($line)) {
            continue
        }

        try {
            $rows += ($line | ConvertFrom-Json)
        }
        catch {
            throw "Malformed JSONL row $lineNumber in $fullPath`: $($_.Exception.Message)"
        }
    }

    return @($rows)
}

function Get-PropertyValue {
    param($Object, [Parameter(Mandatory = $true)][string]$Name, $Default = $null)

    if ($null -eq $Object) {
        return $Default
    }

    $property = $Object.PSObject.Properties | Where-Object { $_.Name -eq $Name } | Select-Object -First 1
    if ($property) {
        return $property.Value
    }

    return $Default
}

function Get-Text {
    param($Object, [Parameter(Mandatory = $true)][string]$Name, [string]$Default = "")
    $value = Get-PropertyValue -Object $Object -Name $Name -Default $Default
    if ($null -eq $value) {
        return $Default
    }

    return [string]$value
}

function Get-BoolText {
    param($Object, [Parameter(Mandatory = $true)][string]$Name)
    return (Get-Text -Object $Object -Name $Name -Default "").ToLowerInvariant()
}

function Read-JsonFileSafe {
    param([string]$Path)

    if ([string]::IsNullOrWhiteSpace($Path)) {
        return $null
    }

    if (-not (Test-Path -LiteralPath $Path)) {
        return $null
    }

    try {
        return (Get-Content -Raw -LiteralPath $Path -Encoding UTF8 | ConvertFrom-Json)
    }
    catch {
        return $null
    }
}

function Get-ResponseEnvelope {
    param([Parameter(Mandatory = $true)]$Row)

    $responsePath = Get-Text -Object $Row -Name "raw_output_ref_private"
    if ([string]::IsNullOrWhiteSpace($responsePath)) {
        $responsePath = Get-Text -Object $Row -Name "host_runtime_response_ref"
    }

    $requestPath = Get-Text -Object $Row -Name "raw_prompt_ref_private"
    if ([string]::IsNullOrWhiteSpace($requestPath)) {
        $requestPath = Get-Text -Object $Row -Name "host_runtime_request_ref"
    }

    $request = Read-JsonFileSafe $requestPath
    $response = Read-JsonFileSafe $responsePath
    $checkSet = Get-PropertyValue -Object $response -Name "CheckSetResults" -Default ([pscustomobject]@{})
    $warnings = @(Get-PropertyValue -Object $response -Name "Warnings" -Default @())
    $errors = @(Get-PropertyValue -Object $response -Name "Errors" -Default @())
    $selectedSkills = @(Get-PropertyValue -Object $response -Name "SelectedSkills" -Default @())
    $requestedSkills = @(Get-PropertyValue -Object $request -Name "RequestedSkills" -Default @())
    $availableSkills = @(Get-PropertyValue -Object $request -Name "AvailableSkills" -Default @())
    $structuredTaskRefs = @(Get-PropertyValue -Object $response -Name "StructuredTaskRefs" -Default @())
    $scenarioRefs = @(Get-PropertyValue -Object $response -Name "ScenarioRefs" -Default @())
    $chatText = Get-Text -Object $response -Name "ChatDisplayText"

    return [pscustomobject]@{
        request_path = $requestPath
        request_loaded = $null -ne $request
        response_path = $responsePath
        response_loaded = $null -ne $response
        success = [bool](Get-PropertyValue -Object $response -Name "Success" -Default $false)
        status = Get-Text -Object $response -Name "Status"
        chat_text_present = -not [string]::IsNullOrWhiteSpace($chatText)
        chat_text_hash = if ([string]::IsNullOrWhiteSpace($chatText)) { "" } else { Get-Sha256 $chatText }
        check_set = $checkSet
        warnings = $warnings
        errors = $errors
        selected_skills = $selectedSkills
        requested_skills = $requestedSkills
        available_skills = $availableSkills
        selected_skill_count = $selectedSkills.Count
        requested_skill_count = [int](Get-PropertyValue -Object $checkSet -Name "requested_skills_count" -Default $requestedSkills.Count)
        available_skill_count = [int](Get-PropertyValue -Object $checkSet -Name "available_skills_count" -Default $availableSkills.Count)
        structured_task_ref_count = $structuredTaskRefs.Count
        scenario_ref_count = $scenarioRefs.Count
        debug_trace_present = $null -ne (Get-PropertyValue -Object $response -Name "DebugTrace" -Default $null)
    }
}

function Get-Sha256 {
    param([string]$Text)

    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Text)
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        return "sha256:" + ([BitConverter]::ToString($sha.ComputeHash($bytes)) -replace "-", "").ToLowerInvariant()
    }
    finally {
        $sha.Dispose()
    }
}

function New-LayerScore {
    param(
        [string]$LayerId,
        [string]$HumanName,
        [int]$Score,
        [string[]]$EvidenceRefs,
        [string[]]$FailureCategories,
        [string]$HumanReviewerNote
    )

    return [pscustomobject]@{
        layer_id = $LayerId
        human_name = $HumanName
        score = $Score
        evidence_refs = @($EvidenceRefs)
        failure_categories = @($FailureCategories)
        human_reviewer_note = $HumanReviewerNote
    }
}

function Get-CandidatePositionAuthorityStatus {
    param([string]$SkillEvidence)

    $hasCandidatePositionEvidence =
        $SkillEvidence -match "candidate_position" -or
        $SkillEvidence -match "CANDIDATE_POSITION" -or
        $SkillEvidence -match "candidate_index=" -or
        $SkillEvidence -match "candidate_weight="
    $hasRawAuthorityEvidence =
        $SkillEvidence -match "candidate_position_suggestion=" -or
        $SkillEvidence -match "candidate_index=" -or
        $SkillEvidence -match "candidate_weight=" -or
        $SkillEvidence -match "candidate_position_final_authority=true"
    $hasPositiveOrderAuthority =
        $SkillEvidence -match "candidate_order_permutation_selection_authority=true" -or
        $SkillEvidence -match "candidate_order_sensitivity_detected=True"
    $hasProvenanceOnlyLabel =
        $SkillEvidence -match "CANDIDATE_POSITION_WEIGHT_SCAFFOLD_ONLY" -and
        $SkillEvidence -match "candidate_position_role=diagnostics_provenance_only" -and
        $SkillEvidence -match "CANDIDATE_ORDER_LEAKAGE_GUARD_DIAGNOSTICS_ONLY" -and
        $SkillEvidence -match "candidate_order_guard_status=selection_authority_blocked" -and
        $SkillEvidence -match "candidate_order_permutation_selection_authority=false" -and
        $SkillEvidence -match "candidate_order_probe_source=semantic_units_without_candidate_position"
    $authorityDetected = $hasRawAuthorityEvidence -or $hasPositiveOrderAuthority
    $unlabelledCandidatePosition = $hasCandidatePositionEvidence -and -not $hasProvenanceOnlyLabel -and -not $authorityDetected
    $failureReason = ""
    if ($authorityDetected) {
        $failureReason = "candidate_position_used_as_runtime_authority"
    } elseif ($unlabelledCandidatePosition) {
        $failureReason = "candidate_position_used_as_unlabelled_authority"
    }

    return [pscustomobject]@{
        has_candidate_position_evidence = $hasCandidatePositionEvidence
        provenance_only = $hasProvenanceOnlyLabel -and -not $authorityDetected
        authority_detected = $authorityDetected
        unlabelled_candidate_position = $unlabelledCandidatePosition
        failure_reason = $failureReason
    }
}

function Get-RowScore {
    param([Parameter(Mandatory = $true)]$Row)

    $envelope = Get-ResponseEnvelope $Row
    $checkSet = $envelope.check_set
    $promptStatus = Get-Text -Object $checkSet -Name "prompt_to_task_status"
    $orchestrationStatus = Get-Text -Object $checkSet -Name "orchestration_reasoning_status"
    $orchestrationLoadError = Get-Text -Object $checkSet -Name "orchestration_reasoning_load_error"
    $semanticClaim = Get-BoolText -Object $checkSet -Name "semantic_quality_claimed"
    $runtimeClaim = Get-BoolText -Object $checkSet -Name "runtime_extension_claimed"
    $skillEvidence = Get-Text -Object $checkSet -Name "skill_selection_evidence"
    $skillSecondPass = Get-BoolText -Object $checkSet -Name "skill_selection_requires_second_pass"
    $skillConfidence = Get-Text -Object $checkSet -Name "skill_selection_confidence"
    $semanticQualityKeptClosed = $semanticClaim -ne "true"
    $hasStructuredTaskRef = $envelope.structured_task_ref_count -gt 0
    $hasScenarioRef = $envelope.scenario_ref_count -gt 0
    $hasSelectedSkills = $envelope.selected_skill_count -gt 0
    $skillSurfaceRequired = $envelope.requested_skill_count -gt 0 -or (Get-Text -Object $Row -Name "method") -eq "skill-selection"
    $requiredSkillSelectionAbsent = $skillSurfaceRequired -and -not $hasSelectedSkills
    $skillConfidenceNumber = 0.0
    [void][double]::TryParse(
        $skillConfidence,
        [System.Globalization.NumberStyles]::Float,
        [System.Globalization.CultureInfo]::InvariantCulture,
        [ref]$skillConfidenceNumber)

    $evidenceRefs = @(
        (Get-Text -Object $Row -Name "raw_output_ref_private"),
        (Get-Text -Object $Row -Name "trace_ref")
    ) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }

    $forbiddenFailures = New-Object System.Collections.Generic.List[string]
    if ($semanticClaim -eq "true") { $forbiddenFailures.Add("semantic_quality_claimed_by_runtime_row") }
        if ($runtimeClaim -eq "true") { $forbiddenFailures.Add("runtime_extension_claimed_inside_core_response") }
    if (-not $envelope.request_loaded) { $forbiddenFailures.Add("request_envelope_not_loaded_for_skill_count_audit") }
    $candidatePositionAuthorityStatus = Get-CandidatePositionAuthorityStatus $skillEvidence
    if (-not [string]::IsNullOrWhiteSpace($candidatePositionAuthorityStatus.failure_reason)) {
        $forbiddenFailures.Add($candidatePositionAuthorityStatus.failure_reason)
    }

    $promptScore = 2
    $promptFailures = @("no_matching_learned_pattern")
    if ($promptStatus -match "matched|semantic" -and $promptStatus -notmatch "no_matching") {
        $promptScore = 3
        $promptFailures = @("matched_state_but_no_independent_semantic_eval")
    }
    if ($promptStatus -match "semantic.*produced|produced.*semantic" -and $hasStructuredTaskRef -and $semanticQualityKeptClosed) {
        $promptScore = 4
        $promptFailures = @("runtime_quality_passed_semantic_quality_not_proven")
    }
    if (-not $envelope.response_loaded -or -not $envelope.success) {
        $promptScore = 1
        $promptFailures = @("response_missing_or_failed")
    }

    $orchestrationScore = 1
    $orchestrationFailures = @("orchestration_not_trained_or_not_loaded")
    if ([string]::IsNullOrWhiteSpace($orchestrationLoadError) -and $orchestrationStatus -notmatch "not_trained|not_loaded|failed|error") {
        $orchestrationScore = 3
        $orchestrationFailures = @("orchestration_not_independently_quality_scored")
    }
    if (
        [string]::IsNullOrWhiteSpace($orchestrationLoadError) -and
        $orchestrationStatus -match "semantic.*plan.*produced|plan.*produced|produced.*plan" -and
        $orchestrationStatus -notmatch "no_matching|not_ready|not_trained|not_loaded|failed|error" -and
        $hasScenarioRef -and
        $semanticQualityKeptClosed
    ) {
        $orchestrationScore = 4
        $orchestrationFailures = @("runtime_quality_passed_semantic_quality_not_proven")
    }

    $skillScore = 2
    $skillFailures = @("selected_skills_empty_or_second_pass_required")
    if ($hasSelectedSkills -and $skillSecondPass -ne "true" -and $skillConfidenceNumber -gt 0) {
        $skillScore = 3
        $skillFailures = @("skills_selected_but_semantic_quality_not_proven")
    }
    if (
        $hasSelectedSkills -and
        $skillSecondPass -ne "true" -and
        $skillConfidenceNumber -gt 0 -and
        $skillEvidence -match "SEMANTIC_SKILL_SELECTION_PIPELINE_BOUNDARY" -and
        $skillEvidence -match "semantic_quality_claimed=false" -and
        $forbiddenFailures.Count -eq 0
    ) {
        $skillScore = 4
        $skillFailures = @("runtime_quality_passed_semantic_quality_not_proven")
    }

    $usefulnessScore = 3
    $usefulnessFailures = @("honest_boundary_answer_but_not_scenario_specific")
    if (-not $envelope.chat_text_present -or $envelope.errors.Count -gt 0) {
        $usefulnessScore = 2
        $usefulnessFailures = @("missing_chat_text_or_errors_present")
    }
    if (
        $envelope.chat_text_present -and
        $envelope.errors.Count -eq 0 -and
        $semanticQualityKeptClosed -and
        ($hasStructuredTaskRef -or $hasScenarioRef -or $hasSelectedSkills)
    ) {
        $usefulnessScore = 4
        $usefulnessFailures = @("runtime_quality_passed_semantic_quality_not_proven")
    }

    $noShortcutScore = if ($forbiddenFailures.Count -eq 0) { 4 } else { 0 }

    $debugTraceScore = if (-not [string]::IsNullOrWhiteSpace((Get-Text -Object $Row -Name "trace_ref"))) { 4 } else { 2 }
    $tokenFieldScore = if (
        (Get-PropertyValue -Object $Row -Name "prompt_tokens" -Default $null) -ne $null -and
        (Get-PropertyValue -Object $Row -Name "output_tokens" -Default $null) -ne $null -and
        (Get-PropertyValue -Object $Row -Name "total_tokens" -Default $null) -ne $null
    ) { 5 } else { 1 }
    $dimensionScores = [ordered]@{
        task_quality = $promptScore
        plan_universality = $orchestrationScore
        skill_selection = $skillScore
        no_deterministic_shortcut = $noShortcutScore
        debug_trace_availability = $debugTraceScore
        token_measurement_fields = $tokenFieldScore
    }
    $requiredDimensionScoresBelowThreshold = @(
        foreach ($dimension in $dimensionScores.GetEnumerator()) {
            if ([int]$dimension.Value -lt 4) {
                [pscustomobject]@{
                    dimension = $dimension.Key
                    score = [int]$dimension.Value
                    pass_threshold = 4
                }
            }
        }
    )

    $layers = @(
        (New-LayerScore "prompt_to_task" "PromptToTask: request -> task" $promptScore $evidenceRefs $promptFailures "Runtime row reports prompt_to_task_status='$promptStatus'; score 4 means universal runtime task evidence only, not semantic quality."),
        (New-LayerScore "orchestration_reasoning" "OrchestrationReasoning: task -> universal plan" $orchestrationScore $evidenceRefs $orchestrationFailures "Runtime row reports orchestration_reasoning_status='$orchestrationStatus' and load_error='$orchestrationLoadError'; score 4 means runtime plan evidence only, not semantic quality."),
        (New-LayerScore "skill_selection" "SkillSelection: task/plan -> needed skills" $skillScore $evidenceRefs $skillFailures "Selected skill count=$($envelope.selected_skill_count), second_pass=$skillSecondPass, confidence='$skillConfidence'; score 4 means runtime skill-selection evidence only, not semantic quality."),
        (New-LayerScore "final_output_usefulness" "Final output: usefulness and honesty" $usefulnessScore $evidenceRefs $usefulnessFailures "Output is readable, has no runtime errors, and keeps semantic quality closed; score 4 means runtime usefulness evidence only, not semantic quality.")
    )

    $average = [Math]::Round((($layers | ForEach-Object { $_.score }) | Measure-Object -Average).Average, 2)
    $layerGatePassed = (($layers | Where-Object { $_.score -lt 4 }).Count -eq 0)
    $dimensionGatePassed = $requiredDimensionScoresBelowThreshold.Count -eq 0
    $qualityGatePassed = ($forbiddenFailures.Count -eq 0) -and $layerGatePassed -and $dimensionGatePassed
    $verdict = if ($qualityGatePassed) { "PASS" } elseif (($layers | Where-Object { $_.score -le 2 }).Count -gt 0 -or $forbiddenFailures.Count -gt 0) { "FAIL" } else { "NEEDS_REPAIR" }

    return [pscustomobject]@{
        schema = "xiigen.extension_token_04_quality_row_score.v1"
        phase_id = $PhaseId
        scenario_id = Get-Text -Object $Row -Name "scenario_id"
        pair_id = Get-Text -Object $Row -Name "pair_id"
        host = Get-Text -Object $Row -Name "host"
        method = Get-Text -Object $Row -Name "method"
        mode = Get-Text -Object $Row -Name "mode"
        debug_enabled = [bool](Get-PropertyValue -Object $Row -Name "debug_enabled" -Default $false)
        prompt_hash = Get-Text -Object $Row -Name "prompt_hash"
        output_hash = Get-Text -Object $Row -Name "output_hash"
        tokenizer_name = Get-Text -Object $Row -Name "tokenizer_name"
        tokenizer_version = Get-Text -Object $Row -Name "tokenizer_version"
        tokenizer_accounting_type = Get-Text -Object $Row -Name "tokenizer_accounting_type"
        prompt_tokens = [int](Get-PropertyValue -Object $Row -Name "prompt_tokens" -Default 0)
        output_tokens = [int](Get-PropertyValue -Object $Row -Name "output_tokens" -Default 0)
        total_tokens = [int](Get-PropertyValue -Object $Row -Name "total_tokens" -Default 0)
        adaptive_effect_classification = Get-Text -Object $Row -Name "adaptive_effect_classification"
        user_adaptive_any_asset_loaded = Get-Text -Object $Row -Name "user_adaptive_any_asset_loaded"
        user_adaptive_asset_root_used = [bool](Get-PropertyValue -Object $Row -Name "user_adaptive_asset_root_used" -Default $false)
        response_ref_private = $envelope.response_path
        trace_ref_private = Get-Text -Object $Row -Name "trace_ref"
        response_loaded = $envelope.response_loaded
        response_success = $envelope.success
        response_status = $envelope.status
        response_chat_text_hash = $envelope.chat_text_hash
        selected_skills = @($envelope.selected_skills)
        requested_skills = @($envelope.requested_skills)
        available_skills_count = $envelope.available_skill_count
        selected_skills_count = $envelope.selected_skill_count
        requested_skills_count = $envelope.requested_skill_count
        skill_selection_surface_required = $skillSurfaceRequired
        required_skill_selection_absent = $requiredSkillSelectionAbsent
        skill_selection_evidence = $skillEvidence
        candidate_position_evidence_present = $candidatePositionAuthorityStatus.has_candidate_position_evidence
        candidate_position_provenance_only = $candidatePositionAuthorityStatus.provenance_only
        candidate_position_authority_detected = $candidatePositionAuthorityStatus.authority_detected
        candidate_position_unlabelled = $candidatePositionAuthorityStatus.unlabelled_candidate_position
        dimension_scores = [pscustomobject]$dimensionScores
        required_dimension_scores_below_threshold = @($requiredDimensionScoresBelowThreshold)
        required_dimension_pass_threshold = 4
        dimension_gate_passed = $dimensionGatePassed
        layer_gate_passed = $layerGatePassed
        layer_scores = $layers
        average_score = $average
        verdict = $verdict
        quality_gate_passed = $qualityGatePassed
        quality_gate_scope = "runtime_row_only_not_semantic_not_pair_not_product"
        runtime_quality_gate_passed = $qualityGatePassed
        semantic_quality_gate_passed = $false
        pair_quality_gate_passed = $false
        product_readiness_gate_passed = $false
        forbidden_failures = @($forbiddenFailures)
        quality_result_claimed = $false
        semantic_quality_claimed = $false
        learnability_claimed = $false
        token_savings_claimed = $false
        human_reviewer_note = "TOKEN-04 scored this real host row conservatively. It diagnoses quality gaps only; it does not prove semantic quality, learnability, token savings, training, or checkpoint readiness."
    }
}

$inputFullPath = Resolve-FullPath $InputPath
$pairInputFullPath = Resolve-FullPath $PairInputPath
$outputFullDir = Resolve-FullPath $OutputDir
$stateFullPath = Resolve-FullPath $StatePath

$rows = Read-Jsonl $inputFullPath
$pairRows = Read-Jsonl $pairInputFullPath
if ($rows.Count -ne 108) {
    throw "Expected 108 quality input rows, got $($rows.Count)."
}

$rowScores = @($rows | ForEach-Object { Get-RowScore $_ })
$rowsByPair = $rowScores | Group-Object -Property pair_id
$pairInputById = @{}
foreach ($pairInput in $pairRows) {
    $pairInputById[(Get-Text -Object $pairInput -Name "pair_id")] = $pairInput
}

$pairScores = foreach ($group in $rowsByPair) {
    $baseline = @($group.Group | Where-Object { $_.mode -eq "baseline_common_model_only" }) | Select-Object -First 1
    $adaptive = @($group.Group | Where-Object { $_.mode -eq "adaptive_common_plus_user_model" }) | Select-Object -First 1
    $pairInput = $pairInputById[$group.Name]
    $hasBoth = $null -ne $baseline -and $null -ne $adaptive
    $adaptiveEffectProven = $false
    if ($hasBoth) {
        $adaptiveUserAssetLoaded = Get-Text -Object $adaptive -Name "user_adaptive_any_asset_loaded"
        $adaptiveEffectProven =
            $adaptive.adaptive_effect_classification -notmatch "unavailable|no_real_adaptive_effect" -and
            $adaptiveUserAssetLoaded -eq "true" -and
            $adaptive.output_hash -ne $baseline.output_hash
    }

    $qualityNonRegression = $hasBoth -and
        $adaptive.average_score -ge $baseline.average_score -and
        $adaptive.verdict -ne "FAIL_FORBIDDEN"
    $qualityGatePassed = $hasBoth -and $baseline.quality_gate_passed -and $adaptive.quality_gate_passed
    $tokenDelta = if ($hasBoth) { [int]$baseline.total_tokens - [int]$adaptive.total_tokens } else { 0 }
    $tokenSavingsAllowed = $qualityNonRegression -and $qualityGatePassed -and $adaptiveEffectProven -and $tokenDelta -gt 0

    [pscustomobject]@{
        schema = "xiigen.extension_token_04_quality_pair_result.v1"
        phase_id = $PhaseId
        scenario_id = if ($hasBoth) { $baseline.scenario_id } else { "" }
        pair_id = $group.Name
        host = if ($hasBoth) { $baseline.host } else { "" }
        method = if ($hasBoth) { $baseline.method } else { "" }
        debug_enabled = if ($hasBoth) { $baseline.debug_enabled } else { $false }
        baseline_score = if ($baseline) { $baseline.average_score } else { $null }
        adaptive_score = if ($adaptive) { $adaptive.average_score } else { $null }
        score_delta = if ($hasBoth) { [Math]::Round($adaptive.average_score - $baseline.average_score, 2) } else { $null }
        baseline_verdict = if ($baseline) { $baseline.verdict } else { "MISSING_BASELINE" }
        adaptive_verdict = if ($adaptive) { $adaptive.verdict } else { "MISSING_ADAPTIVE" }
        quality_non_regression_for_scored_rows = $qualityNonRegression
        quality_gate_passed = $qualityGatePassed
        quality_gate_scope = "paired_baseline_and_adaptive_runtime_rows_only_not_semantic_not_product"
        runtime_quality_gate_passed_for_both_rows = $qualityGatePassed
        semantic_quality_gate_passed = $false
        product_readiness_gate_passed = $false
        adaptive_effect_proven = $adaptiveEffectProven
        baseline_total_tokens = if ($baseline) { $baseline.total_tokens } else { $null }
        adaptive_total_tokens = if ($adaptive) { $adaptive.total_tokens } else { $null }
        token_delta = $tokenDelta
        token_reduction_observed = $tokenDelta -gt 0
        token_savings_claim_allowed = $tokenSavingsAllowed
        inherited_pairing_fields_preserved = [bool](Get-PropertyValue -Object $pairInput -Name "pairing_fields_preserved" -Default $false)
        failure_reasons = @(
            if (-not $hasBoth) { "missing_baseline_or_adaptive_row" }
            if (-not $qualityGatePassed) { "quality_gate_not_passed" }
            if (-not $adaptiveEffectProven) { "adaptive_effect_not_proven" }
            if ($tokenDelta -le 0) { "token_reduction_not_observed" }
            if (-not $tokenSavingsAllowed) { "token_savings_claim_forbidden" }
        )
        human_reviewer_note = "Baseline/adaptive comparison is diagnostic only. Equal weak scores are non-regression for scored rows, not quality improvement and not token savings."
    }
}

$hosts = @($rowScores | Select-Object -ExpandProperty host -Unique | Sort-Object)
$methods = @($rowScores | Select-Object -ExpandProperty method -Unique | Sort-Object)
$scenarioCount = @($rowScores | Select-Object -ExpandProperty scenario_id -Unique).Count
$pairCount = @($pairScores).Count
$qualityPassedRows = @($rowScores | Where-Object { $_.quality_gate_passed }).Count
$failedRows = @($rowScores | Where-Object { $_.verdict -eq "FAIL" }).Count
$needsRepairRows = @($rowScores | Where-Object { $_.verdict -eq "NEEDS_REPAIR" }).Count
$dimensionGatePassedRows = @($rowScores | Where-Object { $_.dimension_gate_passed }).Count
$requiredDimensionScoreBelowThresholdRows = @($rowScores | Where-Object { @($_.required_dimension_scores_below_threshold).Count -gt 0 }).Count
$noDeterministicShortcutBelowThresholdRows = @($rowScores | Where-Object { [int]$_.dimension_scores.no_deterministic_shortcut -lt 4 }).Count
$selectedSkillsZeroRows = @($rowScores | Where-Object { $_.selected_skills_count -eq 0 }).Count
$requestedSkillsZeroRows = @($rowScores | Where-Object { $_.requested_skills_count -eq 0 }).Count
$requiredSkillSelectionAbsentRows = @($rowScores | Where-Object { $_.required_skill_selection_absent }).Count
$baselineRows = @($rowScores | Where-Object { $_.mode -eq "baseline_common_model_only" })
$baselineSkillRowsPassedRuntime = @($baselineRows | Where-Object {
    $skillLayer = $_.layer_scores | Where-Object { $_.layer_id -eq "skill_selection" } | Select-Object -First 1
    $null -ne $skillLayer -and $skillLayer.score -ge 4
}).Count
$baselinePromptRowsStillBlocked = @($baselineRows | Where-Object {
    $promptLayer = $_.layer_scores | Where-Object { $_.layer_id -eq "prompt_to_task" } | Select-Object -First 1
    $null -ne $promptLayer -and $promptLayer.score -lt 4
}).Count
$baselineOrchestrationRowsStillBlocked = @($baselineRows | Where-Object {
    $orchestrationLayer = $_.layer_scores | Where-Object { $_.layer_id -eq "orchestration_reasoning" } | Select-Object -First 1
    $null -ne $orchestrationLayer -and $orchestrationLayer.score -lt 4
}).Count
$nonRegressionPairs = @($pairScores | Where-Object { $_.quality_non_regression_for_scored_rows }).Count
$qualityGatePairs = @($pairScores | Where-Object { $_.quality_gate_passed }).Count
$adaptiveEffectPairs = @($pairScores | Where-Object { $_.adaptive_effect_proven }).Count
$tokenSavingsAllowedPairs = @($pairScores | Where-Object { $_.token_savings_claim_allowed }).Count

$aggregateByHost = foreach ($hostName in $hosts) {
    $hostRows = @($rowScores | Where-Object { $_.host -eq $hostName })
    [pscustomobject]@{
        host = $hostName
        row_count = $hostRows.Count
        fail_count = @($hostRows | Where-Object { $_.verdict -eq "FAIL" }).Count
        needs_repair_count = @($hostRows | Where-Object { $_.verdict -eq "NEEDS_REPAIR" }).Count
        pass_count = @($hostRows | Where-Object { $_.verdict -eq "PASS" }).Count
        average_score = [Math]::Round((($hostRows | ForEach-Object { $_.average_score }) | Measure-Object -Average).Average, 2)
        selected_skills_zero_rows = @($hostRows | Where-Object { $_.selected_skills_count -eq 0 }).Count
        requested_skills_zero_rows = @($hostRows | Where-Object { $_.requested_skills_count -eq 0 }).Count
        required_skill_selection_absent_rows = @($hostRows | Where-Object { $_.required_skill_selection_absent }).Count
    }
}

$aggregateByMethod = foreach ($methodName in $methods) {
    $methodRows = @($rowScores | Where-Object { $_.method -eq $methodName })
    [pscustomobject]@{
        method = $methodName
        row_count = $methodRows.Count
        fail_count = @($methodRows | Where-Object { $_.verdict -eq "FAIL" }).Count
        needs_repair_count = @($methodRows | Where-Object { $_.verdict -eq "NEEDS_REPAIR" }).Count
        pass_count = @($methodRows | Where-Object { $_.verdict -eq "PASS" }).Count
        average_score = [Math]::Round((($methodRows | ForEach-Object { $_.average_score }) | Measure-Object -Average).Average, 2)
        selected_skills_zero_rows = @($methodRows | Where-Object { $_.selected_skills_count -eq 0 }).Count
        requested_skills_zero_rows = @($methodRows | Where-Object { $_.requested_skills_count -eq 0 }).Count
        required_skill_selection_absent_rows = @($methodRows | Where-Object { $_.required_skill_selection_absent }).Count
    }
}

$aggregateByHostMethod = foreach ($group in ($rowScores | Group-Object -Property host, method | Sort-Object Name)) {
    [pscustomobject]@{
        host_method = $group.Name
        row_count = $group.Count
        fail_count = @($group.Group | Where-Object { $_.verdict -eq "FAIL" }).Count
        needs_repair_count = @($group.Group | Where-Object { $_.verdict -eq "NEEDS_REPAIR" }).Count
        pass_count = @($group.Group | Where-Object { $_.verdict -eq "PASS" }).Count
        selected_skills_zero_rows = @($group.Group | Where-Object { $_.selected_skills_count -eq 0 }).Count
        requested_skills_zero_rows = @($group.Group | Where-Object { $_.requested_skills_count -eq 0 }).Count
        required_skill_selection_absent_rows = @($group.Group | Where-Object { $_.required_skill_selection_absent }).Count
    }
}

$aggregateByScenario = foreach ($scenarioGroup in ($rowScores | Group-Object -Property scenario_id | Sort-Object Name)) {
    [pscustomobject]@{
        scenario_id = $scenarioGroup.Name
        row_count = $scenarioGroup.Count
        fail_count = @($scenarioGroup.Group | Where-Object { $_.verdict -eq "FAIL" }).Count
        needs_repair_count = @($scenarioGroup.Group | Where-Object { $_.verdict -eq "NEEDS_REPAIR" }).Count
        pass_count = @($scenarioGroup.Group | Where-Object { $_.verdict -eq "PASS" }).Count
        average_score = [Math]::Round((($scenarioGroup.Group | ForEach-Object { $_.average_score }) | Measure-Object -Average).Average, 2)
    }
}

$adaptiveEffectGapText = if ($adaptiveEffectPairs -gt 0 -and $qualityGatePairs -gt 0) {
    "Adaptive output effect is visible in pair hashes, and $qualityGatePairs paired runtime quality gates pass; token-savings and semantic/product gates remain separate and closed."
} elseif ($adaptiveEffectPairs -gt 0 -and $qualityPassedRows -gt 0 -and $qualityGatePairs -eq 0) {
    "Adaptive output effect is visible in pair hashes and adaptive runtime rows pass, but paired/common quality gates and semantic-quality gates still do not pass."
} elseif ($adaptiveEffectPairs -gt 0) {
    "Adaptive output effect is now visible in pair hashes, but quality gates and semantic-quality gates still do not pass."
} else {
    "Adaptive rows do not prove real user-local adaptive model effect; many outputs are identical to baseline."
}

$qualityGapText = if ($failedRows -eq 0 -and $needsRepairRows -eq 0 -and $qualityGatePairs -gt 0) {
    "All runtime quality rows pass, and $qualityGatePairs paired runtime quality gates pass; semantic quality, product readiness, and token savings remain separate claims."
} elseif ($failedRows -eq 0 -and $needsRepairRows -gt 0 -and $qualityGatePairs -eq 0) {
    "Baseline/common rows no longer hard-fail runtime scoring, but $needsRepairRows rows remain NEEDS_REPAIR, so paired quality gates and token-savings gates stay closed."
} elseif ($qualityPassedRows -gt 0 -and $qualityGatePairs -eq 0) {
    "Adaptive rows can pass the runtime quality rubric, but baseline/common rows still fail, so paired quality gates and token-savings gates stay closed."
} elseif ($qualityPassedRows -gt 0) {
    "Some runtime quality rows pass; semantic-quality gates remain closed until source-disjoint eval, ablation, leakage checks, fresh-load/continue-training, and quality gates all pass."
} else {
    "Runtime quality rows still do not pass the quality gate."
}

$promptGapText = if ($baselinePromptRowsStillBlocked -gt 0) {
    "Baseline/common PromptToTask rows still have $baselinePromptRowsStillBlocked rows below runtime quality because no matching learned pattern is produced."
} else {
    "Baseline/common PromptToTask rows pass runtime-row quality, but semantic quality remains unproven."
}

$skillGapText = if ($baselineSkillRowsPassedRuntime -eq $baselineRows.Count -and $baselineRows.Count -gt 0) {
    "Baseline/common SkillSelection rows now select skills without second pass for all $baselineSkillRowsPassedRuntime baseline rows; common readiness still remains false until PromptToTask/Orchestration and semantic-proof gates pass."
} else {
    "Baseline/common SkillSelection rows still require second pass or have empty selected skills; common readiness remains false."
}

$orchestrationGapText = if ($baselineOrchestrationRowsStillBlocked -gt 0) {
    "Baseline/common OrchestrationReasoning rows still have $baselineOrchestrationRowsStillBlocked rows below runtime quality because no independently quality-scored semantic plan is proven."
} else {
    "Baseline/common OrchestrationReasoning rows pass runtime-row quality, but semantic quality remains unproven."
}

$dimensionGapText = if ($noDeterministicShortcutBelowThresholdRows -gt 0) {
    "$noDeterministicShortcutBelowThresholdRows rows have no_deterministic_shortcut below the pass threshold because candidate-position evidence is raw authority, unlabelled, or order-sensitive; those rows cannot PASS and stay claims-closed."
} elseif ($requiredDimensionScoreBelowThresholdRows -gt 0) {
    "$requiredDimensionScoreBelowThresholdRows rows have at least one required dimension below the pass threshold; those rows cannot PASS and stay claims-closed."
} else {
    "All required dimension scores meet the runtime pass threshold; semantic/product gates still remain separate."
}

$whatThisProvesAdaptiveLine = if ($adaptiveEffectPairs -gt 0) {
    "The current corpus proves user-local adaptive output effect for $adaptiveEffectPairs baseline/adaptive pairs, while keeping quality/token/semantic claims closed."
} else {
    "The current full-host corpus does not pass quality gates and does not prove adaptive effect."
}

$report = [pscustomobject]@{
    schema = "xiigen.extension_token_04_quality_scoring_full_hosts_report.v1"
    phase_id = $PhaseId
    input_ref = $inputFullPath
    pair_input_ref = $pairInputFullPath
    row_scores_ref = Join-Path $outputFullDir $RowScoresName
    pair_results_ref = Join-Path $outputFullDir $PairScoresName
    row_count = $rowScores.Count
    pair_count = $pairCount
    scenario_count = $scenarioCount
    hosts = $hosts
    methods = $methods
    quality_passed_rows = $qualityPassedRows
    failed_rows = $failedRows
    needs_repair_rows = $needsRepairRows
    dimension_gate_passed_rows = $dimensionGatePassedRows
    required_dimension_score_below_threshold_rows = $requiredDimensionScoreBelowThresholdRows
    no_deterministic_shortcut_below_threshold_rows = $noDeterministicShortcutBelowThresholdRows
    selected_skills_zero_rows = $selectedSkillsZeroRows
    requested_skills_zero_rows = $requestedSkillsZeroRows
    required_skill_selection_absent_rows = $requiredSkillSelectionAbsentRows
    quality_gate_pairs = $qualityGatePairs
    quality_non_regression_for_scored_rows_pairs = $nonRegressionPairs
    adaptive_effect_proven_pairs = $adaptiveEffectPairs
    token_savings_claim_allowed_pairs = $tokenSavingsAllowedPairs
    quality_gate_scope = "runtime_rows_and_pairs_only_not_semantic_not_product"
    runtime_quality_passed_rows = $qualityPassedRows
    semantic_quality_gate_passed_rows = 0
    product_readiness_gate_passed_rows = 0
    quality_non_regression_passed = $false
    quality_improvement_claim_allowed = $false
    token_savings_claim_allowed = $false
    semantic_quality_claimed = $false
    learnability_claimed = $false
    training_started = $false
    checkpoint_mutation_started = $false
    verdict = "QUALITY_SCORING_COMPLETE_GAPS_FOUND_NO_QUALITY_OR_TOKEN_CLAIM"
    aggregate_by_host = @($aggregateByHost)
    aggregate_by_method = @($aggregateByMethod)
    aggregate_by_host_method = @($aggregateByHostMethod)
    aggregate_by_scenario = @($aggregateByScenario)
    blocking_quality_gaps = @(
        $qualityGapText,
        $promptGapText,
        $orchestrationGapText,
        $skillGapText,
        $dimensionGapText,
        "Runtime quality scoring remains separate from semantic quality; semantic quality is not proven without source-disjoint eval, ablation, leakage checks, fresh-load/continue-training, and passing gates.",
        $adaptiveEffectGapText
    )
    next_bounded_work_order = $NextBoundedWorkOrder
    what_this_proves = @(
        "All 108 captured output rows were scored against the TOKEN-01 dimensions.",
        "All 54 baseline/adaptive pairs were compared with token savings claim kept false.",
        $whatThisProvesAdaptiveLine
    )
    what_this_does_not_prove = @(
        "There is no proof for token savings, quality improvement, semantic quality, learnability, training, checkpoint readiness, target promotion, locator update, or final extension readiness.",
        "This scoring does not mutate models and does not repair the remaining PromptToTask or OrchestrationReasoning gaps."
    )
}

$rowScoresPath = Join-Path $outputFullDir $RowScoresName
$pairScoresPath = Join-Path $outputFullDir $PairScoresName
$reportJsonPath = Join-Path $outputFullDir $ReportJsonName
$reportMdPath = Join-Path $outputFullDir $ReportMdName
$validationPath = Join-Path $outputFullDir $ValidationName

Write-Utf8NoBom $rowScoresPath (($rowScores | ForEach-Object { ConvertTo-CompactJson $_ }) -join "`n")
Write-Utf8NoBom $pairScoresPath (($pairScores | ForEach-Object { ConvertTo-CompactJson $_ }) -join "`n")
Write-Utf8NoBom $reportJsonPath (ConvertTo-PrettyJson $report)

$hostSummaryRows = ($aggregateByHost | ForEach-Object {
    "| $($_.host) | $($_.row_count) | $($_.pass_count) | $($_.fail_count) | $($_.needs_repair_count) | $($_.selected_skills_zero_rows) | $($_.requested_skills_zero_rows) | $($_.required_skill_selection_absent_rows) | $($_.average_score) |"
}) -join "`n"
$methodSummaryRows = ($aggregateByMethod | ForEach-Object {
    "| $($_.method) | $($_.row_count) | $($_.pass_count) | $($_.fail_count) | $($_.needs_repair_count) | $($_.selected_skills_zero_rows) | $($_.requested_skills_zero_rows) | $($_.required_skill_selection_absent_rows) | $($_.average_score) |"
}) -join "`n"
$md = @(
    "# $PhaseId",
    "",
    "What this proves:",
    "- All 108 captured output rows were scored against the TOKEN-01 dimensions.",
    "- All 54 baseline/adaptive pairs were compared.",
    "- Current quality is diagnostic only: the corpus does not pass quality gates. $whatThisProvesAdaptiveLine",
    "",
    "What this does not prove:",
    "- No token savings are claimed.",
    "- No quality improvement is claimed.",
    "- No full semantic quality, learnability, training, checkpoint readiness, target promotion, locator update, or final extension readiness is claimed.",
    "",
    "## Counts",
    "",
    "| Metric | Value |",
    "|---|---:|",
    "| Scored rows | $($rowScores.Count) |",
    "| Baseline/adaptive pairs | $pairCount |",
    "| Scenarios | $scenarioCount |",
    "| Quality-passing rows | $qualityPassedRows |",
    "| Failed rows | $failedRows |",
    "| Needs-repair rows | $needsRepairRows |",
    "| Rows passing all required dimension scores | $dimensionGatePassedRows |",
    "| Rows with required dimension below pass threshold | $requiredDimensionScoreBelowThresholdRows |",
    "| Rows with no_deterministic_shortcut below pass threshold | $noDeterministicShortcutBelowThresholdRows |",
    "| Rows with zero selected skills | $selectedSkillsZeroRows |",
    "| Rows with zero requested skills | $requestedSkillsZeroRows |",
    "| Rows with required skill selection absent | $requiredSkillSelectionAbsentRows |",
    "| Pairs with scored-row non-regression | $nonRegressionPairs |",
    "| Pairs with quality gate pass | $qualityGatePairs |",
    "| Pairs with real adaptive effect proven | $adaptiveEffectPairs |",
    "| Pairs allowed to claim token savings | $tokenSavingsAllowedPairs |",
    "",
    "## Host Summary",
    "",
    "| Host | Rows | PASS rows | FAIL rows | NEEDS_REPAIR rows | Zero selected | Zero requested | Required absent | Average score |",
    "|---|---:|---:|---:|---:|---:|---:|---:|---:|",
    $hostSummaryRows,
    "",
    "## Method Summary",
    "",
    "| Method | Rows | PASS rows | FAIL rows | NEEDS_REPAIR rows | Zero selected | Zero requested | Required absent | Average score |",
    "|---|---:|---:|---:|---:|---:|---:|---:|---:|",
    $methodSummaryRows,
    "",
    "## Blocking Quality Gaps",
    "",
    "- $qualityGapText",
    "- $promptGapText",
    "- $orchestrationGapText",
    "- $skillGapText",
    "- $dimensionGapText",
    "- Runtime quality scoring remains separate from semantic quality; semantic quality is not proven without source-disjoint eval, ablation, leakage checks, fresh-load/continue-training, and passing gates.",
    "- $adaptiveEffectGapText",
    "",
    "## Next Independent Track",
    "",
    $NextBoundedWorkOrder
) -join "`n"
Write-Utf8NoBom $reportMdPath $md

$validation = [pscustomobject]@{
    schema = "xiigen.extension_token_04_quality_scoring_full_hosts_validation.v1"
    phase_id = $PhaseId
    input_rows_expected = 108
    input_rows_actual = $rows.Count
    scored_rows_actual = $rowScores.Count
    pair_rows_actual = $pairScores.Count
    row_count_pass = $rows.Count -eq 108 -and $rowScores.Count -eq 108
    pair_count_pass = $pairScores.Count -eq 54
    dimension_gate_passed_rows = $dimensionGatePassedRows
    required_dimension_score_below_threshold_rows = $requiredDimensionScoreBelowThresholdRows
    no_deterministic_shortcut_below_threshold_rows = $noDeterministicShortcutBelowThresholdRows
    pass_blocked_when_required_dimensions_below_threshold = @($rowScores | Where-Object { @($_.required_dimension_scores_below_threshold).Count -gt 0 -and $_.verdict -eq "PASS" }).Count -eq 0
    token_savings_claim_allowed = $false
    quality_improvement_claim_allowed = $false
    semantic_quality_claimed = $false
    learnability_claimed = $false
    output_files = @($rowScoresPath, $pairScoresPath, $reportJsonPath, $reportMdPath)
}
Write-Utf8NoBom $validationPath (ConvertTo-PrettyJson $validation)

if ($WriteState) {
    $state = Get-Content -Raw -LiteralPath $stateFullPath -Encoding UTF8 | ConvertFrom-Json
    $state.updated_at_local = (Get-Date).ToUniversalTime().ToString("o")
    $state.current_phase = $PhaseId
    $state.phase_status = "QUALITY_SCORING_COMPLETE_GAPS_FOUND_NO_QUALITY_OR_TOKEN_CLAIM"
    $state.phase_scope = "Scored 108 captured real host output rows and 54 baseline/adaptive pairs; no token savings, quality improvement, semantic quality, learnability, training, checkpoint mutation, target promotion, locator update, or final readiness claim."
    $state.goal_reached = $false
    $state.stop_allowed = $false
    $state.continuation_required = $true
    $state.eval_allowed = $true
    $state.eval_started = $true
    $state.training_started = $false
    $state.checkpoint_mutation_started = $false
    $state.quality_non_regression_passed = $false
    $state.token_savings_claim_allowed = $false
    $state.next_action = $report.next_bounded_work_order
    $state | Add-Member -NotePropertyName "token_04_quality_scoring_full_hosts" -NotePropertyValue $report -Force
    Write-Utf8NoBom $stateFullPath (ConvertTo-PrettyJson $state)
}

Write-Output (ConvertTo-PrettyJson $validation)
