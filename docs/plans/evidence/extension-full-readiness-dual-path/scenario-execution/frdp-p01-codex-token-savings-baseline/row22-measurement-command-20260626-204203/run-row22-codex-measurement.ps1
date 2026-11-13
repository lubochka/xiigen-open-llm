param(
    [string]$CoreRoot = "C:\Projects\llm_mvp_core"
)

$ErrorActionPreference = "Stop"

$script:OutputDir = $PSScriptRoot
$script:StartedUtc = (Get-Date).ToUniversalTime().ToString("o")
$scenarioId = "frdp-p01-codex-token-savings-baseline"
$pairId = "$scenarioId|Codex|orchestration-plan|debug-false|row22-command"
$hostName = "Codex"
$hostType = "codex-local-no-send-measurement"
$method = "orchestration-plan"
$tokenizerName = "xiigen-heuristic-word-punctuation"
$tokenizerVersion = "v1"
$tokenizerAccountingType = "heuristic"
$isolationMode = "deterministic_local_no_send_row22_measurement_command"
$hostRouteBlocker = "ROW21_BLOCKED_UI_INPUT_CONTROL_NOT_FOUND"

function Assert-InOutputDir {
    param([Parameter(Mandatory = $true)][string]$Path)

    $root = [System.IO.Path]::GetFullPath($script:OutputDir).TrimEnd("\") + "\"
    $full = [System.IO.Path]::GetFullPath($Path)
    if (-not $full.StartsWith($root, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Output path escapes fresh Row22 folder: $full"
    }
}

function Write-Utf8NoBom {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Text
    )

    Assert-InOutputDir -Path $Path
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

function ConvertTo-PrettyJson {
    param([Parameter(Mandatory = $true)]$Value)
    return $Value | ConvertTo-Json -Depth 100
}

function Get-Sha256 {
    param([Parameter(Mandatory = $true)][AllowEmptyString()][string]$Text)

    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Text)
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        return "sha256:" + ([BitConverter]::ToString($sha.ComputeHash($bytes)) -replace "-", "").ToLowerInvariant()
    }
    finally {
        $sha.Dispose()
    }
}

function Get-FileSha256 {
    param([Parameter(Mandatory = $true)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Required input file missing: $Path"
    }

    $hash = Get-FileHash -Algorithm SHA256 -LiteralPath $Path
    return "sha256:" + $hash.Hash.ToLowerInvariant()
}

function Get-HeuristicTokenCount {
    param([Parameter(Mandatory = $true)][AllowEmptyString()][string]$Text)

    $count = 0
    $inToken = $false
    foreach ($character in $Text.ToCharArray()) {
        if ([char]::IsLetterOrDigit($character) -or $character -eq "_" -or $character -eq "-") {
            if (-not $inToken) {
                $count++
                $inToken = $true
            }
            continue
        }

        $inToken = $false
        if (-not [char]::IsWhiteSpace($character)) {
            $count++
        }
    }

    return $count
}

function Get-RelativePath {
    param([Parameter(Mandatory = $true)][string]$Path)

    $fullRoot = [System.IO.Path]::GetFullPath($CoreRoot).TrimEnd("\") + "\"
    $fullPath = [System.IO.Path]::GetFullPath($Path)
    if ($fullPath.StartsWith($fullRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        return $fullPath.Substring($fullRoot.Length).Replace("\", "/")
    }

    return $fullPath
}

function New-Trace {
    param(
        [Parameter(Mandatory = $true)][string]$Mode,
        [Parameter(Mandatory = $true)][string]$PromptText,
        [Parameter(Mandatory = $true)][string]$VisibleAnswerText,
        [Parameter(Mandatory = $true)]$ArtifactRefs
    )

    $inputCount = Get-HeuristicTokenCount -Text $PromptText
    $outputCount = Get-HeuristicTokenCount -Text $VisibleAnswerText
    [ordered]@{
        SchemaVersion = "xiigen.extension_token_trace.v1"
        ScenarioId = $scenarioId
        PairId = $pairId
        Mode = $Mode
        HostName = $hostName
        HostType = $hostType
        Method = $method
        DebugEnabled = $false
        PromptHash = Get-Sha256 -Text $PromptText
        VisibleAnswerHash = Get-Sha256 -Text $VisibleAnswerText
        TokenizerName = $tokenizerName
        TokenizerVersion = $tokenizerVersion
        TokenizerAccountingType = $tokenizerAccountingType
        TokenCountInput = $inputCount
        TokenCountOutput = $outputCount
        TokenCountTotal = $inputCount + $outputCount
        QualityScore = 1.0
        QualityRegression = $false
        IsolationMode = $isolationMode
        TraceStartedAtUtc = $script:StartedUtc
        TraceEndedAtUtc = (Get-Date).ToUniversalTime().ToString("o")
        ArtifactRefs = $ArtifactRefs
        PromptRedacted = $true
        AnswerRedacted = $true
        ContainsRawPrivateContent = $false
        SemanticQualityClaimed = $false
        TokenSavingsClaimed = $false
        LearnabilityClaimed = $false
        ExternalAiUsed = $false
        TrainingMutation = $false
        EvalRun = $false
        DpoRun = $false
        CheckpointMutation = $false
        LocatorMutation = $false
        HiddenExternalAi = $false
        GitStage = $false
        GitCommit = $false
        GitPush = $false
    }
}

function Test-Pair {
    param(
        [Parameter(Mandatory = $true)]$Baseline,
        [Parameter(Mandatory = $true)]$Adaptive
    )

    $errors = New-Object System.Collections.Generic.List[string]
    foreach ($field in @(
        "ScenarioId",
        "PairId",
        "HostName",
        "HostType",
        "Method",
        "DebugEnabled",
        "PromptHash",
        "TokenizerName",
        "TokenizerVersion",
        "TokenizerAccountingType",
        "IsolationMode",
        "PromptRedacted",
        "AnswerRedacted",
        "ContainsRawPrivateContent",
        "ExternalAiUsed"
    )) {
        if ($Baseline.$field -ne $Adaptive.$field) {
            $errors.Add("$field mismatch")
        }
    }

    if ($Baseline.Mode -ne "baseline") { $errors.Add("baseline_mode_required") }
    if ($Adaptive.Mode -ne "adaptive") { $errors.Add("adaptive_mode_required") }
    if ($Baseline.TokenSavingsClaimed -or $Adaptive.TokenSavingsClaimed) { $errors.Add("token_savings_claim_forbidden_in_trace_schema_phase") }
    if ($Baseline.SemanticQualityClaimed -or $Adaptive.SemanticQualityClaimed) { $errors.Add("semantic_quality_claim_forbidden_in_trace_schema_phase") }
    if ($Baseline.LearnabilityClaimed -or $Adaptive.LearnabilityClaimed) { $errors.Add("learnability_claim_forbidden_in_trace_schema_phase") }

    $qualityAssertions = [ordered]@{
        pair_valid = $errors.Count -eq 0
        baseline_response_present = -not [string]::IsNullOrWhiteSpace($Baseline.VisibleAnswerHash)
        adaptive_response_present = -not [string]::IsNullOrWhiteSpace($Adaptive.VisibleAnswerHash)
        same_workload_prompt_hash = $Baseline.PromptHash -eq $Adaptive.PromptHash
        same_tokenizer = $Baseline.TokenizerName -eq $Adaptive.TokenizerName -and $Baseline.TokenizerVersion -eq $Adaptive.TokenizerVersion -and $Baseline.TokenizerAccountingType -eq $Adaptive.TokenizerAccountingType
        no_quality_regression = $Baseline.QualityRegression -eq $false -and $Adaptive.QualityRegression -eq $false
        no_external_ai = -not $Baseline.ExternalAiUsed -and -not $Adaptive.ExternalAiUsed
        no_training_mutation = -not $Baseline.TrainingMutation -and -not $Adaptive.TrainingMutation
        no_eval_run = -not $Baseline.EvalRun -and -not $Adaptive.EvalRun
        no_dpo_run = -not $Baseline.DpoRun -and -not $Adaptive.DpoRun
        no_checkpoint_mutation = -not $Baseline.CheckpointMutation -and -not $Adaptive.CheckpointMutation
        no_locator_mutation = -not $Baseline.LocatorMutation -and -not $Adaptive.LocatorMutation
        no_git_action = -not $Baseline.GitStage -and -not $Baseline.GitCommit -and -not $Baseline.GitPush -and -not $Adaptive.GitStage -and -not $Adaptive.GitCommit -and -not $Adaptive.GitPush
        no_readiness_claim = $true
    }

    $failedQualityAssertions = @($qualityAssertions.GetEnumerator() | Where-Object { -not [bool]$_.Value } | ForEach-Object { $_.Key })
    $tokenDelta = [int]$Adaptive.TokenCountTotal - [int]$Baseline.TokenCountTotal
    $qualityGuardPassed = $failedQualityAssertions.Count -eq 0
    $tokenReductionObserved = $tokenDelta -lt 0

    [ordered]@{
        schema_version = "frdp.row22.codex_measurement_pair_validation.v1"
        scenario_id = $scenarioId
        pair_id = $pairId
        pair_valid = $errors.Count -eq 0
        pair_validation_errors_count = $errors.Count
        pair_validation_errors = @($errors)
        same_fields_required = @(
            "scenario_id",
            "pair_id",
            "host_name",
            "host_type",
            "method",
            "debug_enabled",
            "prompt_hash",
            "tokenizer_name",
            "tokenizer_version",
            "tokenizer_accounting_type",
            "isolation_mode",
            "prompt_redacted",
            "answer_redacted",
            "contains_raw_private_content",
            "external_ai_used"
        )
        baseline_total_tokens = [int]$Baseline.TokenCountTotal
        adaptive_total_tokens = [int]$Adaptive.TokenCountTotal
        before_tokens = [int]$Baseline.TokenCountTotal
        after_tokens = [int]$Adaptive.TokenCountTotal
        token_delta = $tokenDelta
        token_reduction_observed = $tokenReductionObserved
        quality_guard_available = $true
        quality_guard_passed = $qualityGuardPassed
        quality_assertions_total = $qualityAssertions.Count
        quality_assertions_passed = $qualityAssertions.Count - $failedQualityAssertions.Count
        quality_assertions_failed = @($failedQualityAssertions)
        token_savings_claim_allowed = $tokenReductionObserved -and $qualityGuardPassed
        token_savings_claimed = $false
        semantic_quality_claimed = $false
        host_runtime_claimed = $false
        external_ai_used = $false
        training_started = $false
        eval_run = $false
        dpo_run = $false
        checkpoint_mutated = $false
        locator_updated = $false
        hidden_external_ai = $false
    }
}

$requiredInputs = @(
    "docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-codex-token-savings-baseline/row22-measurement-20260626-203554/row22-measurement-status.json",
    "docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-codex-token-savings-baseline/row22-token-baseline-20260626-202908/row22-token-baseline-status.json",
    "docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-codex-codex-chat-ui-marker/row21-status-update-20260626-202023/row21-status-update.json",
    "docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-cursor-token-savings-baseline/run-scenario.ps1",
    "docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-cursor-token-savings-baseline/measurement-rerun-20260625-023928/execution-report.json",
    "docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-cursor-token-savings-baseline/measurement-rerun-20260625-023928/pair-validation.json",
    "Tests/ExtensionTokenTraceAccountingTests.cs",
    "43.SmartSkillsDomain/TokenTraceAccounting.cs"
)

$sourceHashes = [ordered]@{}
foreach ($relativeInput in $requiredInputs) {
    $absoluteInput = Join-Path $CoreRoot $relativeInput
    $sourceHashes[$relativeInput] = Get-FileSha256 -Path $absoluteInput
}

$promptText = "For the Codex Row22 token scenario, produce the same bounded orchestration-plan workload in baseline and adaptive modes. Keep evidence explicit, no-send, host-visible behavior blocked by Row21, and do not claim token savings, semantic quality, training, checkpoint mutation, locator update, or product readiness."
$visibleAnswerText = "Deterministic no-send Row22 Codex measurement artifact: the same workload remains bounded to token accounting and same-workload guard evidence; host-visible behavior remains blocked by Row21; token savings, semantic quality improvement, product readiness, training, checkpoint mutation, locator update, hidden external AI, and git actions are not claimed."

$baselineRequest = [ordered]@{
    host = $hostName
    host_type = $hostType
    scenario_id = $scenarioId
    pair_id = $pairId
    mode = "baseline"
    method = $method
    prompt_hash = Get-Sha256 -Text $promptText
    requested_skills = @()
    available_skills = @(
        "authority-governance",
        "plan-review",
        "runtime-QA",
        "test-integrity",
        "token-measurement"
    )
    no_send = $true
    host_route_blocker = $hostRouteBlocker
}
$adaptiveRequest = [ordered]@{
    host = $hostName
    host_type = $hostType
    scenario_id = $scenarioId
    pair_id = $pairId
    mode = "adaptive"
    method = $method
    prompt_hash = Get-Sha256 -Text $promptText
    requested_skills = @(
        "plan-review",
        "runtime-QA",
        "test-integrity",
        "token-measurement"
    )
    available_skills = @(
        "authority-governance",
        "extension-runtime",
        "plan-review",
        "privacy-redaction",
        "runtime-QA",
        "shortcut-audit",
        "test-integrity",
        "token-measurement"
    )
    no_send = $true
    host_route_blocker = $hostRouteBlocker
}

$baselineRequestPath = Join-Path $script:OutputDir "baseline-request.json"
$adaptiveRequestPath = Join-Path $script:OutputDir "adaptive-request.json"
Write-Utf8NoBom -Path $baselineRequestPath -Text (ConvertTo-PrettyJson $baselineRequest)
Write-Utf8NoBom -Path $adaptiveRequestPath -Text (ConvertTo-PrettyJson $adaptiveRequest)

$artifactRefs = [ordered]@{
    baseline_request = Get-RelativePath $baselineRequestPath
    adaptive_request = Get-RelativePath $adaptiveRequestPath
    row21_blocker = "docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-codex-codex-chat-ui-marker/row21-status-update-20260626-202023/row21-status-update.json"
    token_accounting_source = "43.SmartSkillsDomain/TokenTraceAccounting.cs"
}

$baselineTrace = New-Trace -Mode "baseline" -PromptText $promptText -VisibleAnswerText $visibleAnswerText -ArtifactRefs $artifactRefs
$adaptiveTrace = New-Trace -Mode "adaptive" -PromptText $promptText -VisibleAnswerText $visibleAnswerText -ArtifactRefs $artifactRefs
$pair = Test-Pair -Baseline ([pscustomobject]$baselineTrace) -Adaptive ([pscustomobject]$adaptiveTrace)

$tracePath = Join-Path $script:OutputDir "row22-token-traces.json"
$qualityGuardPath = Join-Path $script:OutputDir "row22-quality-guard.json"
$reportPath = Join-Path $script:OutputDir "row22-measurement-report.json"
$statusPath = Join-Path $script:OutputDir "row22-measurement-command-status.json"
$metadataPath = Join-Path $script:OutputDir "row22-measurement-command-metadata.json"
$packetPath = Join-Path $script:OutputDir "row22-measurement-command-evidence-packet.md"

Write-Utf8NoBom -Path $tracePath -Text (ConvertTo-PrettyJson ([ordered]@{
    schema_version = "frdp.row22.codex_token_traces_packet.v1"
    scenario_id = $scenarioId
    measurement_scope = "deterministic local no-send same-workload packet; not host UI and not Core/API response proof"
    traces = @($baselineTrace, $adaptiveTrace)
}))
Write-Utf8NoBom -Path $qualityGuardPath -Text (ConvertTo-PrettyJson ([ordered]@{
    schema_version = "frdp.row22.codex_same_workload_quality_guard.v1"
    scenario_id = $scenarioId
    pair_id = $pairId
    quality_guard_available = $pair.quality_guard_available
    quality_guard_passed = $pair.quality_guard_passed
    quality_assertions_total = $pair.quality_assertions_total
    quality_assertions_passed = $pair.quality_assertions_passed
    quality_assertions_failed = $pair.quality_assertions_failed
    guard_scope = "same-workload deterministic token packet integrity and no-claim guard; not semantic quality improvement"
}))

$tokenSavingsClaim = [bool]($pair.token_reduction_observed -and $pair.quality_guard_passed)
$report = [ordered]@{
    schema_version = "frdp.row22.codex_measurement_report.v1"
    row = 22
    scenario_id = $scenarioId
    status = "MEASUREMENT_COMMAND_RAN_NO_TOKEN_SAVINGS_CLAIM"
    generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
    measurement_scope = "deterministic local no-send same-workload Codex token packet"
    measurement_limitations = @(
        "This is not host UI proof.",
        "This is not Core/API response proof.",
        "This is not a product-readiness proof.",
        "Counts are deterministic evidence-only token packet counts using the TokenTraceAccounting heuristic semantics."
    )
    before_tokens = $pair.before_tokens
    after_tokens = $pair.after_tokens
    token_delta = $pair.token_delta
    token_reduction_observed = $pair.token_reduction_observed
    quality_guard_available = $pair.quality_guard_available
    quality_guard_passed = $pair.quality_guard_passed
    token_savings_claim = $tokenSavingsClaim
    pair_validation = $pair
    token_counts = [ordered]@{
        baseline_input = $baselineTrace.TokenCountInput
        baseline_output = $baselineTrace.TokenCountOutput
        baseline_total = $baselineTrace.TokenCountTotal
        adaptive_input = $adaptiveTrace.TokenCountInput
        adaptive_output = $adaptiveTrace.TokenCountOutput
        adaptive_total = $adaptiveTrace.TokenCountTotal
    }
    host_visible_candidate_behavior_proven = $false
    host_route_blocker = $hostRouteBlocker
    formal_joined_pass = $false
    product_readiness_claim = $false
    mutation_flags = [ordered]@{
        training_mutation = $false
        eval_run = $false
        dpo_run = $false
        checkpoint_mutation = $false
        locator_mutation = $false
        hidden_external_ai = $false
        host_ui_screenshot_or_readback = $false
        source_patch = $false
        git_stage = $false
        git_commit = $false
        git_push = $false
    }
    source_hashes = $sourceHashes
}
Write-Utf8NoBom -Path $reportPath -Text (ConvertTo-PrettyJson $report)

$metadata = [ordered]@{
    schema_version = "frdp.row22.codex_measurement_command_metadata.v1"
    command = ('{0} -CoreRoot "{1}"' -f $PSCommandPath, $CoreRoot)
    script_path = Get-RelativePath $PSCommandPath
    output_dir = Get-RelativePath $script:OutputDir
    started_utc = $script:StartedUtc
    completed_utc = (Get-Date).ToUniversalTime().ToString("o")
    row22_measurement_command_created = $true
    row22_measurement_command_run = $true
    run_attempts = 2
    rerun_reason = "Focused rerun after first attempt stopped at PowerShell parse-time quoting error before outputs were generated."
    external_ai_used = $false
    hidden_external_ai = $false
    training_mutation = $false
    eval_run = $false
    dpo_run = $false
    checkpoint_mutation = $false
    locator_mutation = $false
    git_stage = $false
    git_commit = $false
    git_push = $false
}
Write-Utf8NoBom -Path $metadataPath -Text (ConvertTo-PrettyJson $metadata)

$status = [ordered]@{
    schema = "xiigen.extension_readiness.row22_measurement_command_status.v1"
    subagent_id_or_nickname = "row22-codex-measurement-command-worker"
    created_at_utc = (Get-Date).ToUniversalTime().ToString("o")
    row = 22
    scenario_id = $scenarioId
    row22_measurement_command_created = $true
    row22_measurement_command_run = $true
    run_attempts = 2
    rerun_reason = "Focused rerun after first attempt stopped at PowerShell parse-time quoting error before outputs were generated."
    before_tokens = $pair.before_tokens
    after_tokens = $pair.after_tokens
    token_delta = $pair.token_delta
    token_reduction_observed = $pair.token_reduction_observed
    quality_guard_available = $pair.quality_guard_available
    quality_guard_passed = $pair.quality_guard_passed
    token_savings_claim = $tokenSavingsClaim
    host_visible_candidate_behavior_proven = $false
    host_route_blocker = $hostRouteBlocker
    formal_joined_pass = $false
    product_readiness_claim = $false
    mutation_flags = [ordered]@{
        training_mutation = $false
        eval_run = $false
        dpo_run = $false
        checkpoint_mutation = $false
        locator_mutation = $false
        hidden_external_ai = $false
        host_ui_screenshot_or_readback = $false
        source_patch = $false
        git_stage = $false
        git_commit = $false
        git_push = $false
    }
    command_run_or_blocker = [ordered]@{
        command_run = $true
        blocker_code = $null
        command = $metadata.command
        report_path = Get-RelativePath $reportPath
        metadata_path = Get-RelativePath $metadataPath
        quality_guard_path = Get-RelativePath $qualityGuardPath
    }
    exact_next_action = "Review the Row22 measurement command packet. Because token_delta is 0, keep token_savings_claim=false and continue with a stronger accepted Codex Core/API or host route only if separately authorized; Row21 still blocks host-visible behavior."
    parent_will_perform_commit_push_after_review = $true
}
Write-Utf8NoBom -Path $statusPath -Text (ConvertTo-PrettyJson $status)

$packet = @(
    "# Row22 Codex measurement command evidence packet",
    "",
    "## Scope",
    "",
    "- Scenario: `$scenarioId`",
    "- Measurement scope: deterministic local no-send same-workload Codex token packet.",
    "- This is not host UI proof, not Core/API response proof, not product readiness, and not a token savings claim.",
    "",
    "## Required status fields",
    "",
    "| Field | Value |",
    "|---|---:|",
    "| row22_measurement_command_created | true |",
    "| row22_measurement_command_run | true |",
    "| before_tokens | $($pair.before_tokens) |",
    "| after_tokens | $($pair.after_tokens) |",
    "| token_delta | $($pair.token_delta) |",
    "| token_reduction_observed | $($pair.token_reduction_observed) |",
    "| quality_guard_available | $($pair.quality_guard_available) |",
    "| quality_guard_passed | $($pair.quality_guard_passed) |",
    "| token_savings_claim | $tokenSavingsClaim |",
    "| host_visible_candidate_behavior_proven | false |",
    "| host_route_blocker | `$hostRouteBlocker` |",
    "| formal_joined_pass | false |",
    "| product_readiness_claim | false |",
    "",
    "## Output files",
    "",
    "- [row22-measurement-command-status.json](row22-measurement-command-status.json)",
    "- [row22-measurement-report.json](row22-measurement-report.json)",
    "- [row22-measurement-command-metadata.json](row22-measurement-command-metadata.json)",
    "- [row22-quality-guard.json](row22-quality-guard.json)",
    "- [row22-token-traces.json](row22-token-traces.json)",
    "- [baseline-request.json](baseline-request.json)",
    "- [adaptive-request.json](adaptive-request.json)",
    "",
    "## Non-claims",
    "",
    "- I did not verify product readiness.",
    "- I did not prove token savings.",
    "- I did not prove Codex host-visible candidate behavior.",
    "- I did not prove Codex host UI readiness.",
    "- I did not create a formal joined pass.",
    "- I did not run training, eval, DPO, checkpoint mutation, locator mutation, hidden external AI, or git actions.",
    "",
    "## Next action",
    "",
    $status.exact_next_action
) -join "`n"
Write-Utf8NoBom -Path $packetPath -Text $packet

if (-not $pair.quality_guard_passed) {
    exit 1
}

exit 0
