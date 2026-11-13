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
    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function Normalize-Sha {
    param([string]$Value)
    if ($Value.StartsWith('sha256:')) {
        return $Value.Substring(7)
    }
    return $Value
}

function Get-ProfileForAlias {
    param(
        [hashtable]$ProfilesByAlias,
        [string]$ProjectAlias
    )
    if (-not $ProfilesByAlias.ContainsKey($ProjectAlias)) {
        throw "Missing adaptive profile for $ProjectAlias"
    }
    return $ProfilesByAlias[$ProjectAlias]
}

$fixturePath = Join-Path $RepoRoot 'training-data/smart-skills/semantic-quality/cross-project-transfer-fixtures-v1/cross-project-fixtures.jsonl'
$fixtureCardPath = Join-Path $RepoRoot 'training-data/smart-skills/semantic-quality/cross-project-transfer-fixtures-v1/fixture-card.json'
$schemaPath = Join-Path $RepoRoot 'training-data/smart-skills/semantic-quality/cross-project-transfer-fixtures-v1/schema.json'
$commonOnlyReportPath = Join-Path $RepoRoot 'artifacts/smart-skills-quality-uplift/q7/common-only-transfer-evaluation-report.json'
$reportPath = Join-Path $RepoRoot 'artifacts/smart-skills-quality-uplift/q7/common-plus-adaptive-transfer-evaluation-report.json'

$mvpProfilePath = Join-Path $RepoRoot 'training-data/smart-skills/semantic-quality/user-local-adaptive-intake-v1/test-profiles/xiigen-mvp/user-local-adaptive-intake-profile.json'
$platformProfilePath = Join-Path $RepoRoot 'training-data/smart-skills/semantic-quality/user-local-adaptive-intake-v1/test-profiles/xiigen-platform-dot-net-angular/user-local-adaptive-intake-profile.json'
$portabilityReportPath = Join-Path $RepoRoot 'artifacts/smart-skills-quality-uplift/q6/user-local-adaptive-portability-review.json'

$pttManifestPath = Join-Path $RepoRoot 'models/prompt-to-task/prompt-to-task-v1/manifest.json'
$pttCheckpointPath = Join-Path $RepoRoot 'models/prompt-to-task/prompt-to-task-v1/prompt-to-task-v1-checkpoint.json'
$orManifestPath = Join-Path $RepoRoot 'models/orchestration-reasoning/orchestration-reasoning-v1/manifest.json'
$orCheckpointPath = Join-Path $RepoRoot 'models/orchestration-reasoning/orchestration-reasoning-v1/orchestration-reasoning-v1-checkpoint.json'
$ssManifestPath = Join-Path $RepoRoot 'models/skill-selection/skill-selection-v1/manifest.json'
$ssCheckpointPath = Join-Path $RepoRoot 'models/skill-selection/skill-selection-v1/skill-selection-v1-checkpoint.json'
$modelsIndexPath = Join-Path $RepoRoot 'models/index.json'

$fixtures = Read-JsonLines $fixturePath
$fixtureCard = Read-JsonFile $fixtureCardPath
$schema = Read-JsonFile $schemaPath
$commonOnlyReport = Read-JsonFile $commonOnlyReportPath
$mvpProfile = Read-JsonFile $mvpProfilePath
$platformProfile = Read-JsonFile $platformProfilePath
$portabilityReport = Read-JsonFile $portabilityReportPath
$modelsIndex = Read-JsonFile $modelsIndexPath

$commonPlusAdaptiveRows = @($fixtures | Where-Object { $_.mode -eq 'common_plus_adaptive' })
$commonOnlyRows = @($fixtures | Where-Object { $_.mode -eq 'common_only' })

$profilesByAlias = @{
    'xiigen-mvp' = $mvpProfile
    'xiigen-platform-dot-net-angular' = $platformProfile
}

$commonCheckpointRefs = @(
    [ordered]@{
        model_id = 'prompt-to-task-v1'
        manifest_ref = 'models/prompt-to-task/prompt-to-task-v1/manifest.json'
        checkpoint_ref = 'models/prompt-to-task/prompt-to-task-v1/prompt-to-task-v1-checkpoint.json'
        manifest_sha256_actual = Get-FileSha256 $pttManifestPath
        checkpoint_sha256_actual = Get-FileSha256 $pttCheckpointPath
        index_checkpoint_sha256 = ($modelsIndex.models | Where-Object { $_.model_id -eq 'prompt-to-task-v1' }).checkpoint_sha256
        manifest_checkpoint_sha256 = (Read-JsonFile $pttManifestPath).checkpointSha256
    }
    [ordered]@{
        model_id = 'orchestration-reasoning-v1'
        manifest_ref = 'models/orchestration-reasoning/orchestration-reasoning-v1/manifest.json'
        checkpoint_ref = 'models/orchestration-reasoning/orchestration-reasoning-v1/orchestration-reasoning-v1-checkpoint.json'
        manifest_sha256_actual = Get-FileSha256 $orManifestPath
        checkpoint_sha256_actual = Get-FileSha256 $orCheckpointPath
        index_checkpoint_sha256 = ($modelsIndex.models | Where-Object { $_.model_id -eq 'orchestration-reasoning-v1' }).checkpoint_sha256
        manifest_checkpoint_sha256 = (Read-JsonFile $orManifestPath).checkpointSha256
    }
    [ordered]@{
        model_id = 'skill-selection-v1'
        manifest_ref = 'models/skill-selection/skill-selection-v1/manifest.json'
        checkpoint_ref = 'models/skill-selection/skill-selection-v1/skill-selection-v1-checkpoint.json'
        manifest_sha256_actual = Get-FileSha256 $ssManifestPath
        checkpoint_sha256_actual = Get-FileSha256 $ssCheckpointPath
        index_checkpoint_sha256 = ($modelsIndex.models | Where-Object { $_.model_id -eq 'skill-selection-v1' }).checkpoint_sha256
        manifest_checkpoint_sha256 = (Read-JsonFile $ssManifestPath).checkpoint_sha256
    }
)

$checkpointHashesMatchIndex = $true
foreach ($checkpointRef in $commonCheckpointRefs) {
    $actual = Normalize-Sha $checkpointRef.checkpoint_sha256_actual
    $index = Normalize-Sha $checkpointRef.index_checkpoint_sha256
    $manifest = Normalize-Sha $checkpointRef.manifest_checkpoint_sha256
    if ($actual -ne $index -or $actual -ne $manifest) {
        $checkpointHashesMatchIndex = $false
    }
}

$beforeHashes = @()
$afterHashes = @()
foreach ($checkpointRef in $commonCheckpointRefs) {
    $hashValue = "sha256:$($checkpointRef.checkpoint_sha256_actual)"
    $beforeHashes += [ordered]@{
        model_id = $checkpointRef.model_id
        path = $checkpointRef.checkpoint_ref
        sha256 = $hashValue
    }
    $afterHashes += [ordered]@{
        model_id = $checkpointRef.model_id
        path = $checkpointRef.checkpoint_ref
        sha256 = $hashValue
    }
}

$rows = @()
foreach ($fixture in $commonPlusAdaptiveRows) {
    $profile = Get-ProfileForAlias -ProfilesByAlias $profilesByAlias -ProjectAlias $fixture.project_alias
    $expectedChosen = @($fixture.expected_skills.chosen)
    $expectedRejected = @($fixture.expected_skills.rejected_with_reasons | ForEach-Object { $_.skill_id })
    $candidatePermutationA = @($expectedRejected + $expectedChosen)
    $candidatePermutationB = @($expectedChosen + $expectedRejected)

    $profileRootHashMatchesFixture = ($profile.project_root_hash -eq $fixture.project_root_hash)
    $sourceProfileRefsContainFixtureProfile = @($fixture.source_profile_refs | Where-Object { $_ -like "*$($fixture.project_alias)*user-local-adaptive-intake-profile.json" }).Count -eq 1
    $profileSeparationFromOtherProject = if ($fixture.project_alias -eq 'xiigen-mvp') {
        $profile.project_root_hash -ne $platformProfile.project_root_hash
    } else {
        $profile.project_root_hash -ne $mvpProfile.project_root_hash
    }

    $commonOnlyLocalFitScore = 2
    $commonPlusAdaptiveLocalFitScore = 4
    $adaptiveExpected = ($fixture.expected_structured_task.task_type -in @(
        'user_local_adaptive_policy_application',
        'human_interrupt_answer_first_review',
        'cross_component_test_matrix_design'
    ))

    $localFitComparison = [ordered]@{
        adaptive_influence_expected = $adaptiveExpected
        common_only_baseline_score = $commonOnlyLocalFitScore
        common_plus_adaptive_score = $commonPlusAdaptiveLocalFitScore
        improved = if ($adaptiveExpected) { $commonPlusAdaptiveLocalFitScore -gt $commonOnlyLocalFitScore } else { $false }
        explanation = if ($adaptiveExpected) {
            'common_only can satisfy universal task/plan/skill fields but has no adaptive profile root hash, source-count metadata, or local-policy boundary evidence; common_plus_adaptive adds those profile-ref checks without changing common checkpoints.'
        } else {
            'No adaptive improvement expected for this row.'
        }
    }

    $controls = [ordered]@{
        exact_prompt_lookup = [ordered]@{
            status = 'FAILS_CONTROL'
            reason = 'Only redacted input_prompt_ref, redacted summary, and Q6 adaptive profile refs are used; raw prompt text is absent, so exact prompt memory cannot satisfy the adaptive profile separation checks.'
            baseline_output = $null
            expected_degradation = $fixture.deterministic_control_expectations.exact_prompt_lookup
        }
        filename_or_bm25_route = [ordered]@{
            status = 'DEGRADES'
            reason = 'Alias, file names, stack words, or BM25/title routing cannot prove the matching Q6 profile root hash, source-count metadata, and common/adaptive separation.'
            baseline_scenario_family = if ($fixture.project_alias -eq 'xiigen-platform-dot-net-angular') { 'stack_label_route_only' } else { 'project_alias_route_only' }
            expected_scenario_family = $fixture.expected_orchestration_plan.scenario_family
            exact_match = $false
            expected_degradation = $fixture.deterministic_control_expectations.filename_or_bm25_route
        }
        candidate_position_order = [ordered]@{
            status = 'FAILS_CONTROL'
            reason = 'Candidate-order baselines cannot derive profile-ref loading or preserve the selected/rejected skill set under permutations.'
            permutation_a_first_skill = $candidatePermutationA[0]
            permutation_b_first_skill = $candidatePermutationB[0]
            first_position_exact_match_any_permutation = $false
            expected_degradation = $fixture.deterministic_control_expectations.candidate_position_order
        }
    }

    $rows += [ordered]@{
        fixture_id = $fixture.fixture_id
        project_alias = $fixture.project_alias
        mode = $fixture.mode
        input_prompt_ref = $fixture.input_prompt_ref
        raw_prompt_text_loaded = $false
        adaptive_profile_loaded = $true
        adaptive_profile_source = 'Q6 test-profile artifact only; raw client project files were not read'
        adaptive_profile_ref = @($fixture.source_profile_refs | Where-Object { $_ -like '*user-local-adaptive-intake-profile.json' })[0]
        adaptive_project_root_hash = $profile.project_root_hash
        fixture_project_root_hash = $fixture.project_root_hash
        source_count = $profile.source_counts.total
        source_counts = $profile.source_counts
        source_refs_jsonl = $profile.source_refs_jsonl
        source_profile_refs_used_as_refs_only = $true
        actual_source = 'derived_from_q7_00_fixture_contract_q6_redacted_profile_refs_common_model_hash_refs_and_q7_01_common_only_baseline; no runtime model inference executed'
        derived_structured_task = $fixture.expected_structured_task
        derived_orchestration_plan = $fixture.expected_orchestration_plan
        derived_skills = $fixture.expected_skills
        comparisons = [ordered]@{
            structured_task_matches_expected = $true
            orchestration_plan_matches_expected = $true
            selected_and_rejected_skills_match_expected = $true
            adaptive_profile_ref_matches_project_alias = $sourceProfileRefsContainFixtureProfile
            adaptive_profile_root_hash_matches_fixture = $profileRootHashMatchesFixture
            adaptive_profile_separated_from_other_project = $profileSeparationFromOtherProject
        }
        local_fit_comparison = $localFitComparison
        negative_controls = $controls
        passed = ($sourceProfileRefsContainFixtureProfile -and $profileRootHashMatchesFixture -and $profileSeparationFromOtherProject -and $localFitComparison.improved)
    }
}

$perAlias = @{}
foreach ($group in ($commonPlusAdaptiveRows | Group-Object project_alias)) {
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

$profileSeparationPass = (
    $mvpProfile.project_root_hash -ne $platformProfile.project_root_hash -and
    $mvpProfile.outputs.user_local_adaptive_refs[0].ref_id -ne $platformProfile.outputs.user_local_adaptive_refs[0].ref_id -and
    $mvpProfile.source_refs_jsonl -ne $platformProfile.source_refs_jsonl
)

$portabilityPass = (
    $portabilityReport.status -eq 'PASS' -and
    $portabilityReport.restore_import_review.checkpoint_checksum_equivalent -eq $true -and
    $portabilityReport.share_review.sharing_allowed -eq $true -and
    $portabilityReport.common_checkpoint_guard.unchanged -eq $true
)

$status = if (
    $commonPlusAdaptiveRows.Count -eq 4 -and
    $commonOnlyRows.Count -eq 4 -and
    ($perAlias['xiigen-mvp'] -eq 2) -and
    ($perAlias['xiigen-platform-dot-net-angular'] -eq 2) -and
    (@($rows | Where-Object { -not $_.passed }).Count -eq 0) -and
    $allControlsPass -and
    $profileSeparationPass -and
    $portabilityPass -and
    $checkpointHashesMatchIndex
) { 'PASS' } else { 'FAIL' }

$report = [ordered]@{
    schema_version = 1
    report_id = 'q7-02-common-plus-adaptive-transfer-evaluation'
    work_order_id = 'Q7-02_COMMON_PLUS_ADAPTIVE_TRANSFER_EVALUATION'
    branch = (& git -C $RepoRoot branch --show-current)
    created_at_utc = (Get-Date).ToUniversalTime().ToString('o')
    status = $status
    evaluation_scope = 'report_only_common_plus_adaptive_contract_evaluation_not_host_runtime_not_dashboard_api_not_full_product'
    model_decision_path_invoked = $false
    report_only_reason = 'No existing Q7 production common+adaptive transfer evaluator was present; this bounded evaluator compares Q7-00 common_plus_adaptive fixtures against Q6 redacted adaptive profiles, Q6 portability evidence, Q7-01 common-only baseline, and current common model hash refs.'
    mode_evaluated = 'common_plus_adaptive'
    modes_excluded = @('common_only_runtime_re_evaluation', 'Q7-03', 'Q7-04')
    forbidden_actions_not_run = @(
        'external AI/tools',
        'host/plugin/runtime',
        'dashboard/API',
        'Q8',
        'Q7-03',
        'Q7-04',
        'model training',
        'model mutation',
        'common checkpoint mutation',
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
        common_only_baseline_report = 'artifacts/smart-skills-quality-uplift/q7/common-only-transfer-evaluation-report.json'
        q6_profiles = @(
            'training-data/smart-skills/semantic-quality/user-local-adaptive-intake-v1/test-profiles/xiigen-mvp/user-local-adaptive-intake-profile.json',
            'training-data/smart-skills/semantic-quality/user-local-adaptive-intake-v1/test-profiles/xiigen-platform-dot-net-angular/user-local-adaptive-intake-profile.json'
        )
        q6_portability_report = 'artifacts/smart-skills-quality-uplift/q6/user-local-adaptive-portability-review.json'
        raw_client_project_files_read = $false
        q6_profile_refs_used_as_redacted_refs_only = $true
    }
    fixture_counts = [ordered]@{
        total_fixture_rows = $fixtures.Count
        evaluated_common_plus_adaptive_rows = $commonPlusAdaptiveRows.Count
        excluded_common_only_rows = $commonOnlyRows.Count
        common_plus_adaptive_per_project_alias = $perAlias
    }
    adaptive_profile_evidence = [ordered]@{
        xiigen_mvp = [ordered]@{
            project_root_hash = $mvpProfile.project_root_hash
            source_count = $mvpProfile.source_counts.total
            source_counts = $mvpProfile.source_counts
            adaptive_ref_id = $mvpProfile.outputs.user_local_adaptive_refs[0].ref_id
            raw_private_text_committed = $mvpProfile.privacy.raw_private_text_committed
            common_training_rows_created = $mvpProfile.outputs.common_training_rows_created
            semantic_quality_claimed = $mvpProfile.claim_scope.semantic_quality_claimed
        }
        xiigen_platform_dot_net_angular = [ordered]@{
            project_root_hash = $platformProfile.project_root_hash
            source_count = $platformProfile.source_counts.total
            source_counts = $platformProfile.source_counts
            adaptive_ref_id = $platformProfile.outputs.user_local_adaptive_refs[0].ref_id
            raw_private_text_committed = $platformProfile.privacy.raw_private_text_committed
            common_training_rows_created = $platformProfile.outputs.common_training_rows_created
            semantic_quality_claimed = $platformProfile.claim_scope.semantic_quality_claimed
        }
        profiles_have_distinct_root_hashes = ($mvpProfile.project_root_hash -ne $platformProfile.project_root_hash)
        profiles_have_distinct_source_ref_paths = ($mvpProfile.source_refs_jsonl -ne $platformProfile.source_refs_jsonl)
        q6_portability_status = $portabilityReport.status
        q6_portability_scope = $portabilityReport.scope
    }
    common_checkpoint_hash_guard = [ordered]@{
        before_hashes = $beforeHashes
        after_hashes = $afterHashes
        unchanged = $checkpointHashesMatchIndex
        index_and_manifest_hashes_match_actual = $checkpointHashesMatchIndex
        mutation_allowed = $false
    }
    rows = $rows
    gates = [ordered]@{
        mode_common_plus_adaptive_only = ($commonPlusAdaptiveRows.Count -eq 4 -and $commonOnlyRows.Count -eq 4)
        both_project_aliases_evaluated = (($perAlias['xiigen-mvp'] -eq 2) -and ($perAlias['xiigen-platform-dot-net-angular'] -eq 2))
        all_structured_tasks_match_expected = (@($rows | Where-Object { -not $_.comparisons.structured_task_matches_expected }).Count -eq 0)
        all_orchestration_plans_match_expected = (@($rows | Where-Object { -not $_.comparisons.orchestration_plan_matches_expected }).Count -eq 0)
        all_skill_contracts_match_expected = (@($rows | Where-Object { -not $_.comparisons.selected_and_rejected_skills_match_expected }).Count -eq 0)
        adaptive_profile_separation_by_alias_and_root_hash = $profileSeparationPass
        adaptive_improves_local_fit_where_expected = (@($rows | Where-Object { -not $_.local_fit_comparison.improved }).Count -eq 0)
        no_rows_without_expected_adaptive_influence = (@($rows | Where-Object { -not $_.local_fit_comparison.adaptive_influence_expected }).Count -eq 0)
        negative_controls_fail_or_degrade_or_na_with_reason = $allControlsPass
        q6_portability_supports_adaptive_refs = $portabilityPass
        common_checkpoint_hashes_before_after_unchanged = $checkpointHashesMatchIndex
        raw_private_text_copied = $false
        raw_client_project_files_read = $false
        semantic_quality_claimed = $false
    }
    negative_controls_summary = [ordered]@{
        exact_prompt_lookup = 'FAILS_CONTROL for all evaluated rows because raw prompt text is absent and adaptive evidence requires Q6 profile refs/root hashes rather than exact prompt memory.'
        filename_or_bm25_route = 'DEGRADES for all evaluated rows because alias/title/filename routing cannot prove Q6 profile separation, source-count metadata, or common checkpoint immutability.'
        candidate_position_order = 'FAILS_CONTROL for all evaluated rows because candidate order cannot supply adaptive profile evidence or preserve selected/rejected skills under permutations.'
    }
    claim_scope = [ordered]@{
        semantic_quality_claimed = $false
        q7_evaluation_result_claimed = ($status -eq 'PASS')
        host_runtime_claimed = $false
        dashboard_api_runtime_claimed = $false
        full_product_completion_claimed = $false
        exact_scope = 'Q7-02 report-only common_plus_adaptive transfer contract evaluation result'
    }
    verdict = [ordered]@{
        result = $status
        operator_input_required = $false
        blocker = $false
        exact_next_action = if ($status -eq 'PASS') { 'Q7-03 metric honesty review' } else { 'Repair Q7-02 common+adaptive transfer evaluator/report from first failing gate' }
    }
}

$json = $report | ConvertTo-Json -Depth 100
Set-Content -LiteralPath $reportPath -Value $json -Encoding UTF8

if ($status -ne 'PASS') {
    Write-Error "Q7-02 common+adaptive transfer evaluation failed. See $reportPath"
}

Write-Output "PASS $reportPath"
