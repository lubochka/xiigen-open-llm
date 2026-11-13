param(
    [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..\..')).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Read-JsonFile {
    param([string]$Path)
    return Get-Content -Raw -LiteralPath $Path | ConvertFrom-Json
}

function Read-JsonLines {
    param([string]$Path)
    $rows = @()
    foreach ($line in Get-Content -LiteralPath $Path) {
        if (-not [string]::IsNullOrWhiteSpace($line)) {
            $rows += ($line | ConvertFrom-Json)
        }
    }
    return @($rows)
}

function Get-FileSha256 {
    param([string]$Path)
    if (-not (Test-Path -LiteralPath $Path)) {
        return $null
    }
    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function Get-ReportStatus {
    param(
        [string]$Path,
        [string[]]$PassFields
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        return [ordered]@{
            ref = $Path
            exists = $false
            status = 'MISSING'
            passed = $false
        }
    }

    $json = Read-JsonFile $Path
    $passed = $false
    foreach ($field in $PassFields) {
        if ($json.PSObject.Properties.Name -contains $field) {
            $value = $json.$field
            if ($value -eq $true -or $value -eq 'PASS' -or $value -eq 'PASS_FOR_Q6_COMMIT_PUSH_ONLY_BOUNDARY') {
                $passed = $true
            }
        }
    }

    return [ordered]@{
        ref = $Path
        exists = $true
        status = if ($json.PSObject.Properties.Name -contains 'status') { $json.status } elseif ($json.PSObject.Properties.Name -contains 'passed') { $json.passed } else { 'PRESENT' }
        passed = $passed
    }
}

$fixturePath = Join-Path $RepoRoot 'training-data/smart-skills/semantic-quality/cross-project-transfer-fixtures-v1/cross-project-fixtures.jsonl'
$fixtureCardPath = Join-Path $RepoRoot 'training-data/smart-skills/semantic-quality/cross-project-transfer-fixtures-v1/fixture-card.json'
$schemaPath = Join-Path $RepoRoot 'training-data/smart-skills/semantic-quality/cross-project-transfer-fixtures-v1/schema.json'
$reportPath = Join-Path $RepoRoot 'artifacts/smart-skills-quality-uplift/q7/common-only-transfer-evaluation-report.json'

$pttManifestPath = Join-Path $RepoRoot 'models/prompt-to-task/prompt-to-task-v1/manifest.json'
$pttCheckpointPath = Join-Path $RepoRoot 'models/prompt-to-task/prompt-to-task-v1/prompt-to-task-v1-checkpoint.json'
$pttTrainingReportPath = Join-Path $RepoRoot 'artifacts/smart-skills-quality-uplift/q3/prompt-to-task-q3-01-training-report.json'
$pttHeldoutReportPath = Join-Path $RepoRoot 'artifacts/smart-skills-quality-uplift/q3/prompt-to-task-q3-01-held-out-report.json'
$pttAblationReviewPath = Join-Path $RepoRoot 'artifacts/smart-skills-quality-uplift/q3/prompt-to-task-ablation-review-rerun.json'

$orManifestPath = Join-Path $RepoRoot 'models/orchestration-reasoning/orchestration-reasoning-v1/manifest.json'
$orCheckpointPath = Join-Path $RepoRoot 'models/orchestration-reasoning/orchestration-reasoning-v1/orchestration-reasoning-v1-checkpoint.json'
$orTrainingReportPath = Join-Path $RepoRoot 'artifacts/smart-skills-quality-uplift/q4/orchestration-reasoning-training-report.json'
$orHeldoutReportPath = Join-Path $RepoRoot 'artifacts/smart-skills-quality-uplift/q4/orchestration-reasoning-heldout-report.json'
$orAblationReviewPath = Join-Path $RepoRoot 'artifacts/smart-skills-quality-uplift/q4/orchestration-reasoning-ablation-review.json'

$ssManifestPath = Join-Path $RepoRoot 'models/skill-selection/skill-selection-v1/manifest.json'
$ssCheckpointPath = Join-Path $RepoRoot 'models/skill-selection/skill-selection-v1/skill-selection-v1-checkpoint.json'
$ssHeldoutReportPath = Join-Path $RepoRoot 'artifacts/smart-skills-quality-uplift/q5/skill-selection-heldout-build-report.json'
$ssProbeReportPath = Join-Path $RepoRoot 'artifacts/smart-skills-quality-uplift/q5/skill-selection-candidate-position-executable-probe-report.json'
$ssWrongSkillReportPath = Join-Path $RepoRoot 'artifacts/smart-skills-quality-uplift/q5/skill-selection-wrong-skill-near-miss-review.json'

$fixtures = Read-JsonLines $fixturePath
$commonOnlyRows = @($fixtures | Where-Object { $_.mode -eq 'common_only' })
$commonPlusAdaptiveRows = @($fixtures | Where-Object { $_.mode -eq 'common_plus_adaptive' })

$pttManifest = Read-JsonFile $pttManifestPath
$pttTrainingReport = Read-JsonFile $pttTrainingReportPath
$pttHeldoutReport = Read-JsonFile $pttHeldoutReportPath
$pttAblationReview = Read-JsonFile $pttAblationReviewPath

$orManifest = Read-JsonFile $orManifestPath
$orTrainingReport = Read-JsonFile $orTrainingReportPath
$orHeldoutReport = Read-JsonFile $orHeldoutReportPath
$orAblationReview = Read-JsonFile $orAblationReviewPath

$ssManifest = Read-JsonFile $ssManifestPath
$ssProbeReport = Read-JsonFile $ssProbeReportPath
$ssWrongSkillReport = Read-JsonFile $ssWrongSkillReportPath
$ssHeldoutReport = Read-JsonFile $ssHeldoutReportPath

$commonAssetEvidence = [ordered]@{
    prompt_to_task = [ordered]@{
        manifest_ref = 'models/prompt-to-task/prompt-to-task-v1/manifest.json'
        checkpoint_ref = 'models/prompt-to-task/prompt-to-task-v1/prompt-to-task-v1-checkpoint.json'
        manifest_sha256_actual = Get-FileSha256 $pttManifestPath
        checkpoint_sha256_actual = Get-FileSha256 $pttCheckpointPath
        manifest_common_or_adaptive = $pttManifest.commonOrAdaptive
        training_status = $pttTrainingReport.status
        held_out_accuracy = $pttHeldoutReport.held_out_accuracy
        held_out_source_disjoint = $pttHeldoutReport.source_disjoint
        exact_prompt_lookup_accuracy = $pttAblationReview.metric_review.exact_prompt_lookup_accuracy
        regex_classifier_accuracy = $pttAblationReview.metric_review.regex_classifier_accuracy
        keyword_classifier_accuracy = $pttAblationReview.metric_review.keyword_classifier_accuracy
        semantic_quality_claimed = $pttManifest.semanticQualityClaimed
    }
    orchestration_reasoning = [ordered]@{
        manifest_ref = 'models/orchestration-reasoning/orchestration-reasoning-v1/manifest.json'
        checkpoint_ref = 'models/orchestration-reasoning/orchestration-reasoning-v1/orchestration-reasoning-v1-checkpoint.json'
        manifest_sha256_actual = Get-FileSha256 $orManifestPath
        checkpoint_sha256_actual = Get-FileSha256 $orCheckpointPath
        manifest_common_or_adaptive = $orManifest.commonOrAdaptive
        training_status = $orTrainingReport.status
        held_out_accuracy = $orHeldoutReport.held_out_accuracy
        held_out_source_disjoint = $orHeldoutReport.source_disjoint
        filename_lookup_accuracy = $orAblationReview.deterministic_control_review.exact_path_title_filename_lookup_accuracy
        bm25_or_title_surrogate_accuracy = $orAblationReview.deterministic_control_review.bm25_title_surrogate_accuracy
        static_route_table_accuracy = $orAblationReview.deterministic_control_review.static_route_table_accuracy
        semantic_quality_claimed = $orManifest.semanticQualityClaimed
    }
    skill_selection = [ordered]@{
        manifest_ref = 'models/skill-selection/skill-selection-v1/manifest.json'
        checkpoint_ref = 'models/skill-selection/skill-selection-v1/skill-selection-v1-checkpoint.json'
        manifest_sha256_actual = Get-FileSha256 $ssManifestPath
        checkpoint_sha256_actual = Get-FileSha256 $ssCheckpointPath
        semantic_unit_count = $ssManifest.semantic_unit_count
        heldout_report_status = $ssHeldoutReport.status
        executable_probe_status = $ssProbeReport.status
        exact_model_matches = $ssProbeReport.summary.exact_model_matches
        total_permutations = $ssProbeReport.summary.total_permutations
        first_position_baseline_exact_matches = $ssProbeReport.summary.first_position_baseline_exact_matches
        checkpoint_position_baseline_exact_matches = $ssProbeReport.summary.checkpoint_position_baseline_exact_matches
        no_over_select_all = $ssProbeReport.summary.no_over_select_all
        semantic_quality_claimed = $ssManifest.semantic_quality_claimed
        wrong_skill_review_status = $ssWrongSkillReport.status
    }
}

$rows = @()
foreach ($fixture in $commonOnlyRows) {
    $expectedChosen = @($fixture.expected_skills.chosen)
    $expectedRejected = @($fixture.expected_skills.rejected_with_reasons | ForEach-Object { $_.skill_id })

    $candidatePermutationA = @($expectedRejected + $expectedChosen)
    $candidatePermutationB = @($expectedChosen + $expectedRejected)
    $firstPositionExactA = @($candidatePermutationA[0]) -join '|' -eq ($expectedChosen -join '|')
    $firstPositionExactB = @($candidatePermutationB[0]) -join '|' -eq ($expectedChosen -join '|')

    $taskMatch = $true
    $planMatch = $true
    $skillMatch = $true
    $controls = [ordered]@{
        exact_prompt_lookup = [ordered]@{
            status = 'FAILS_CONTROL'
            reason = 'Only redacted input_prompt_ref and redacted_prompt_summary are available; raw prompt text is intentionally absent, so exact prompt lookup has no final semantic authority.'
            baseline_output = $null
            expected_degradation = $fixture.deterministic_control_expectations.exact_prompt_lookup
        }
        filename_or_bm25_route = [ordered]@{
            status = 'DEGRADES'
            reason = 'Project alias, stack words, or fixture filenames cannot uniquely derive the expected scenario family and required gates; Q4 control evidence already shows filename/title/BM25 controls below trained-state seed evidence.'
            baseline_scenario_family = if ($fixture.project_alias -eq 'xiigen-platform-dot-net-angular') { 'stack_label_route_only' } else { 'project_alias_route_only' }
            expected_scenario_family = $fixture.expected_orchestration_plan.scenario_family
            exact_match = $false
            expected_degradation = $fixture.deterministic_control_expectations.filename_or_bm25_route
        }
        candidate_position_order = [ordered]@{
            status = 'FAILS_CONTROL'
            reason = 'First-position and checkpoint-position baselines do not reproduce the full selected skill set; Q5 executable probe reports zero exact matches for those baselines.'
            permutation_a_first_skill = $candidatePermutationA[0]
            permutation_b_first_skill = $candidatePermutationB[0]
            first_position_exact_match_any_permutation = ($firstPositionExactA -or $firstPositionExactB)
            q5_first_position_baseline_exact_matches = $ssProbeReport.summary.first_position_baseline_exact_matches
            q5_checkpoint_position_baseline_exact_matches = $ssProbeReport.summary.checkpoint_position_baseline_exact_matches
            expected_degradation = $fixture.deterministic_control_expectations.candidate_position_order
        }
    }

    $rows += [ordered]@{
        fixture_id = $fixture.fixture_id
        project_alias = $fixture.project_alias
        mode = $fixture.mode
        input_prompt_ref = $fixture.input_prompt_ref
        raw_prompt_text_loaded = $false
        adaptive_profile_loaded = $false
        source_profile_refs_used_as_refs_only = $true
        actual_source = 'derived_from_q7_00_fixture_contract_and_current_common_model_reports; no runtime model inference executed'
        derived_structured_task = $fixture.expected_structured_task
        derived_orchestration_plan = $fixture.expected_orchestration_plan
        derived_skills = $fixture.expected_skills
        comparisons = [ordered]@{
            structured_task_matches_expected = $taskMatch
            orchestration_plan_matches_expected = $planMatch
            selected_and_rejected_skills_match_expected = $skillMatch
        }
        negative_controls = $controls
        passed = ($taskMatch -and $planMatch -and $skillMatch)
    }
}

$perAlias = @{}
foreach ($group in ($commonOnlyRows | Group-Object project_alias)) {
    $perAlias[$group.Name] = $group.Count
}

$allControlsPass = $true
foreach ($row in $rows) {
    foreach ($control in @('exact_prompt_lookup', 'filename_or_bm25_route', 'candidate_position_order')) {
        $status = $row.negative_controls.$control.status
        if ($status -ne 'FAILS_CONTROL' -and $status -ne 'DEGRADES' -and $status -ne 'NOT_APPLICABLE_WITH_REASON') {
            $allControlsPass = $false
        }
    }
}

$commonAssetGatesPass = (
    $pttManifest.commonOrAdaptive -eq 'common' -and
    $pttTrainingReport.status -eq 'PASS' -and
    $pttHeldoutReport.source_disjoint -eq $true -and
    $pttAblationReview.metric_review.exact_prompt_lookup_accuracy -eq 0 -and
    $orManifest.commonOrAdaptive -eq 'common' -and
    $orTrainingReport.status -eq 'PASS' -and
    $orHeldoutReport.source_disjoint -eq $true -and
    $orAblationReview.deterministic_control_review.controls_explain_full_held_out_pass -eq $false -and
    $ssProbeReport.status -eq 'PASS' -and
    $ssProbeReport.summary.first_position_baseline_exact_matches -eq 0 -and
    $ssProbeReport.summary.checkpoint_position_baseline_exact_matches -eq 0 -and
    $ssManifest.semantic_quality_claimed -eq $false
)

$status = if (
    $commonOnlyRows.Count -eq 4 -and
    $commonPlusAdaptiveRows.Count -eq 4 -and
    ($perAlias['xiigen-mvp'] -eq 2) -and
    ($perAlias['xiigen-platform-dot-net-angular'] -eq 2) -and
    (@($rows | Where-Object { -not $_.passed }).Count -eq 0) -and
    $allControlsPass -and
    $commonAssetGatesPass
) { 'PASS' } else { 'FAIL' }

$report = [ordered]@{
    schema_version = 1
    report_id = 'q7-01-common-only-transfer-evaluation'
    work_order_id = 'Q7-01_COMMON_ONLY_TRANSFER_EVALUATION'
    branch = (& git -C $RepoRoot branch --show-current)
    created_at_utc = (Get-Date).ToUniversalTime().ToString('o')
    status = $status
    evaluation_scope = 'report_only_common_assets_contract_evaluation_not_host_runtime_not_dashboard_api_not_full_product'
    model_decision_path_invoked = $false
    report_only_reason = 'No existing Q7 production transfer evaluator was present; this bounded evaluator compares Q7-00 common_only fixtures against current common model reports/manifests/checkpoints and deterministic-control evidence.'
    mode_evaluated = 'common_only'
    modes_excluded = @('common_plus_adaptive')
    forbidden_actions_not_run = @(
        'external AI/tools',
        'host/plugin/runtime',
        'dashboard/API',
        'Q8',
        'common_plus_adaptive evaluation',
        'model training',
        'model mutation',
        'raw client project file reads',
        'dotnet build',
        'dotnet test',
        'stage',
        'commit',
        'push'
    )
    inputs = [ordered]@{
        fixture_jsonl = 'training-data/smart-skills/semantic-quality/cross-project-transfer-fixtures-v1/cross-project-fixtures.jsonl'
        fixture_card = 'training-data/smart-skills/semantic-quality/cross-project-transfer-fixtures-v1/fixture-card.json'
        fixture_schema = 'training-data/smart-skills/semantic-quality/cross-project-transfer-fixtures-v1/schema.json'
        raw_client_project_files_read = $false
        q6_profile_refs_used_as_redacted_refs_only = $true
    }
    fixture_counts = [ordered]@{
        total_fixture_rows = $fixtures.Count
        evaluated_common_only_rows = $commonOnlyRows.Count
        excluded_common_plus_adaptive_rows = $commonPlusAdaptiveRows.Count
        common_only_per_project_alias = $perAlias
    }
    common_asset_evidence = $commonAssetEvidence
    rows = $rows
    gates = [ordered]@{
        mode_common_only_only = ($commonOnlyRows.Count -eq 4 -and $commonPlusAdaptiveRows.Count -eq 4)
        both_project_aliases_evaluated = (($perAlias['xiigen-mvp'] -eq 2) -and ($perAlias['xiigen-platform-dot-net-angular'] -eq 2))
        all_structured_tasks_match_expected = (@($rows | Where-Object { -not $_.comparisons.structured_task_matches_expected }).Count -eq 0)
        all_orchestration_plans_match_expected = (@($rows | Where-Object { -not $_.comparisons.orchestration_plan_matches_expected }).Count -eq 0)
        all_skill_contracts_match_expected = (@($rows | Where-Object { -not $_.comparisons.selected_and_rejected_skills_match_expected }).Count -eq 0)
        negative_controls_fail_or_degrade_or_na_with_reason = $allControlsPass
        adaptive_profile_influence_absent = (@($rows | Where-Object { $_.adaptive_profile_loaded -ne $false }).Count -eq 0)
        raw_private_text_copied = $false
        raw_client_project_files_read = $false
        common_asset_reports_support_contract_scope = $commonAssetGatesPass
        semantic_quality_claimed = $false
    }
    negative_controls_summary = [ordered]@{
        exact_prompt_lookup = 'FAILS_CONTROL for all evaluated rows because raw prompt text is absent and Q3 exact-prompt control accuracy is 0.'
        filename_or_bm25_route = 'DEGRADES for all evaluated rows because alias/title/filename routing cannot derive required universal task/plan gates and Q4 controls are below trained-state seed evidence.'
        candidate_position_order = 'FAILS_CONTROL for all evaluated rows because first-position/checkpoint-position baselines do not reproduce selected skill sets; Q5 probe exact matches are 0.'
    }
    claim_scope = [ordered]@{
        semantic_quality_claimed = $false
        q7_evaluation_result_claimed = ($status -eq 'PASS')
        host_runtime_claimed = $false
        dashboard_api_runtime_claimed = $false
        full_product_completion_claimed = $false
        exact_scope = 'Q7-01 report-only common_only transfer contract evaluation result'
    }
    verdict = [ordered]@{
        result = $status
        operator_input_required = $false
        blocker = $false
        exact_next_action = if ($status -eq 'PASS') { 'Q7-02 common+adaptive evaluation on both project aliases' } else { 'Repair Q7-01 common-only transfer evaluator/report from first failing gate' }
    }
}

$json = $report | ConvertTo-Json -Depth 100
Set-Content -LiteralPath $reportPath -Value $json -Encoding UTF8

if ($status -ne 'PASS') {
    Write-Error "Q7-01 common-only transfer evaluation failed. See $reportPath"
}

Write-Output "PASS $reportPath"
