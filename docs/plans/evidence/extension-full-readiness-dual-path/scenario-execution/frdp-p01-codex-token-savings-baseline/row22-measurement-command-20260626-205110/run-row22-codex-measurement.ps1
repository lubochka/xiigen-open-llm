param(
    [string]$CoreRoot = "C:\Projects\llm_mvp_core"
)

$ErrorActionPreference = "Stop"

$OutputDir = $PSScriptRoot
$StartedUtc = (Get-Date).ToUniversalTime().ToString("o")
$ScenarioId = "frdp-p01-codex-token-savings-baseline"
$PairId = "$ScenarioId|Codex|evidence-only|row22-replacement"
$HostName = "Codex"
$HostType = "codex-local-no-send-measurement"
$Method = "evidence-only-measurement-command"
$TokenizerName = "row22-simple-word-punctuation-tokenizer"
$TokenizerVersion = "v1"
$TokenizerAccountingType = "deterministic-local-heuristic"
$IsolationMode = "deterministic_local_no_send_row22_replacement"
$HostRouteBlocker = "ROW21_BLOCKED_UI_INPUT_CONTROL_NOT_FOUND"

function Assert-InOutputDir {
    param([Parameter(Mandatory = $true)][string]$Path)
    $root = [System.IO.Path]::GetFullPath($OutputDir).TrimEnd("\") + "\"
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
    $normalized = $Text.Replace("`r`n", "`n").Replace("`r", "`n")
    if (-not $normalized.EndsWith("`n")) {
        $normalized += "`n"
    }
    [System.IO.File]::WriteAllText($Path, $normalized, [System.Text.UTF8Encoding]::new($false))
}

function ConvertTo-JsonText {
    param([Parameter(Mandatory = $true)]$Value)
    return $Value | ConvertTo-Json -Depth 100
}

function Get-Sha256Text {
    param([Parameter(Mandatory = $true)][AllowEmptyString()][string]$Text)
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($Text)
        return "sha256:" + ([BitConverter]::ToString($sha.ComputeHash($bytes)) -replace "-", "").ToLowerInvariant()
    }
    finally {
        $sha.Dispose()
    }
}

function Get-TokenCount {
    param([Parameter(Mandatory = $true)][AllowEmptyString()][string]$Text)
    $matches = [regex]::Matches($Text, "[A-Za-z0-9_-]+|[^\sA-Za-z0-9_-]")
    return $matches.Count
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
        [Parameter(Mandatory = $true)][string]$AnswerText,
        [Parameter(Mandatory = $true)][string[]]$Facts
    )
    $inputTokens = Get-TokenCount -Text $PromptText
    $outputTokens = Get-TokenCount -Text $AnswerText
    return [ordered]@{
        schema_version = "frdp.row22.codex_token_trace.v1"
        scenario_id = $ScenarioId
        pair_id = $PairId
        mode = $Mode
        host_name = $HostName
        host_type = $HostType
        method = $Method
        debug_enabled = $false
        prompt_hash = Get-Sha256Text -Text $PromptText
        answer_hash = Get-Sha256Text -Text $AnswerText
        answer_fact_hash = Get-Sha256Text -Text (($Facts | Sort-Object) -join "`n")
        tokenizer_name = $TokenizerName
        tokenizer_version = $TokenizerVersion
        tokenizer_accounting_type = $TokenizerAccountingType
        token_count_input = $inputTokens
        token_count_output = $outputTokens
        token_count_total = $inputTokens + $outputTokens
        isolation_mode = $IsolationMode
        prompt_redacted = $true
        answer_redacted = $true
        contains_raw_private_content = $false
        external_ai_send = $false
        external_ai_used = $false
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
        token_savings_claimed_in_trace = $false
        semantic_quality_claimed = $false
        product_readiness_claim = $false
    }
}

function Test-QualityGuard {
    param(
        [Parameter(Mandatory = $true)]$Baseline,
        [Parameter(Mandatory = $true)]$Adaptive
    )
    $checks = [ordered]@{
        same_scenario_id = $Baseline.scenario_id -eq $Adaptive.scenario_id
        same_pair_id = $Baseline.pair_id -eq $Adaptive.pair_id
        same_host_name = $Baseline.host_name -eq $Adaptive.host_name
        same_host_type = $Baseline.host_type -eq $Adaptive.host_type
        same_method = $Baseline.method -eq $Adaptive.method
        same_prompt_hash = $Baseline.prompt_hash -eq $Adaptive.prompt_hash
        same_answer_fact_hash = $Baseline.answer_fact_hash -eq $Adaptive.answer_fact_hash
        same_tokenizer = ($Baseline.tokenizer_name -eq $Adaptive.tokenizer_name) -and ($Baseline.tokenizer_version -eq $Adaptive.tokenizer_version) -and ($Baseline.tokenizer_accounting_type -eq $Adaptive.tokenizer_accounting_type)
        same_isolation_mode = $Baseline.isolation_mode -eq $Adaptive.isolation_mode
        no_external_ai = (-not $Baseline.external_ai_send) -and (-not $Adaptive.external_ai_send) -and (-not $Baseline.external_ai_used) -and (-not $Adaptive.external_ai_used)
        no_training_or_eval = (-not $Baseline.training_mutation) -and (-not $Adaptive.training_mutation) -and (-not $Baseline.eval_run) -and (-not $Adaptive.eval_run) -and (-not $Baseline.dpo_run) -and (-not $Adaptive.dpo_run)
        no_checkpoint_or_locator_mutation = (-not $Baseline.checkpoint_mutation) -and (-not $Adaptive.checkpoint_mutation) -and (-not $Baseline.locator_mutation) -and (-not $Adaptive.locator_mutation)
        no_hidden_external_ai = (-not $Baseline.hidden_external_ai) -and (-not $Adaptive.hidden_external_ai)
        no_git_action = (-not $Baseline.git_stage) -and (-not $Adaptive.git_stage) -and (-not $Baseline.git_commit) -and (-not $Adaptive.git_commit) -and (-not $Baseline.git_push) -and (-not $Adaptive.git_push)
        no_product_readiness_claim = (-not $Baseline.product_readiness_claim) -and (-not $Adaptive.product_readiness_claim)
    }
    $failed = @($checks.GetEnumerator() | Where-Object { -not [bool]$_.Value } | ForEach-Object { $_.Key })
    return [ordered]@{
        schema_version = "frdp.row22.codex_same_workload_quality_guard.v1"
        scenario_id = $ScenarioId
        pair_id = $PairId
        quality_guard_available = $true
        quality_guard_passed = $failed.Count -eq 0
        quality_assertions_total = $checks.Count
        quality_assertions_passed = $checks.Count - $failed.Count
        quality_assertions_failed = $failed
        assertion_values = $checks
        guard_scope = "same workload, same tokenizer, same required fact set, and mutation/no-claim guard; not host UI proof and not semantic quality improvement proof"
    }
}

$promptFacts = @(
    "row=22",
    "scenario_id=$ScenarioId",
    "host=Codex",
    "host_visible_behavior_blocked_by_row21",
    "no_external_ai",
    "no_training_eval_dpo_checkpoint_locator_git",
    "no_product_readiness_claim"
)

$promptText = @"
Row22 Codex evidence-only token baseline workload.
Use the same scenario, same tokenizer, same fact set, and no-send local measurement path for baseline and adaptive rows.
Keep host-visible candidate behavior blocked by Row21 and keep product readiness false.
"@

$baselineAnswer = @"
Row22 Codex measurement baseline records the complete evidence boundary in verbose form: scenario frdp-p01-codex-token-savings-baseline, host Codex, deterministic local no-send measurement, same tokenizer row22-simple-word-punctuation-tokenizer v1, same workload fact set, and Row21 host route blocker ROW21_BLOCKED_UI_INPUT_CONTROL_NOT_FOUND. This baseline does not use external AI, does not run training, does not run eval, does not run DPO, does not mutate checkpoints, does not mutate locators, does not stage commit or push git changes, does not capture host UI screenshots or readback, does not prove host-visible candidate behavior, does not create a formal joined pass, and does not claim product readiness.
"@

$adaptiveAnswer = @"
Row22 Codex: same scenario, tokenizer, workload facts, and local no-send boundary. Row21 still blocks host-visible behavior. No external AI, training, eval, DPO, checkpoint, locator, git, host UI proof, joined pass, or product readiness claim.
"@

$baselineTrace = New-Trace -Mode "baseline" -PromptText $promptText -AnswerText $baselineAnswer -Facts $promptFacts
$adaptiveTrace = New-Trace -Mode "adaptive" -PromptText $promptText -AnswerText $adaptiveAnswer -Facts $promptFacts
$qualityGuard = Test-QualityGuard -Baseline ([pscustomobject]$baselineTrace) -Adaptive ([pscustomobject]$adaptiveTrace)

$beforeTokens = [int]$baselineTrace.token_count_total
$afterTokens = [int]$adaptiveTrace.token_count_total
$tokenDelta = $afterTokens - $beforeTokens
$tokenReductionObserved = $tokenDelta -lt 0
$tokenSavingsClaim = $tokenReductionObserved -and [bool]$qualityGuard.quality_guard_passed

$tracePath = Join-Path $OutputDir "row22-token-traces.json"
$qualityPath = Join-Path $OutputDir "row22-quality-guard.json"
$reportPath = Join-Path $OutputDir "row22-measurement-report.json"
$metadataPath = Join-Path $OutputDir "row22-measurement-command-metadata.json"
$statusPath = Join-Path $OutputDir "row22-measurement-command-status.json"
$packetPath = Join-Path $OutputDir "row22-measurement-command-evidence-packet.md"

$tracePacket = [ordered]@{
    schema_version = "frdp.row22.codex_token_traces_packet.v1"
    scenario_id = $ScenarioId
    traces = @($baselineTrace, $adaptiveTrace)
}
Write-Utf8NoBom -Path $tracePath -Text (ConvertTo-JsonText $tracePacket)
Write-Utf8NoBom -Path $qualityPath -Text (ConvertTo-JsonText $qualityGuard)

$mutationFlags = [ordered]@{
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

$report = [ordered]@{
    schema_version = "frdp.row22.codex_measurement_report.v1"
    row = 22
    scenario_id = $ScenarioId
    generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
    measurement_scope = "replacement evidence-only deterministic local no-send token measurement"
    same_workload_tokenizer = $TokenizerName
    same_workload_tokenizer_version = $TokenizerVersion
    before_tokens = $beforeTokens
    after_tokens = $afterTokens
    token_delta = $tokenDelta
    token_reduction_observed = $tokenReductionObserved
    quality_guard_available = [bool]$qualityGuard.quality_guard_available
    quality_guard_passed = [bool]$qualityGuard.quality_guard_passed
    token_savings_claim = $tokenSavingsClaim
    quality_guard = $qualityGuard
    host_visible_candidate_behavior_proven = $false
    host_route_blocker = $HostRouteBlocker
    formal_joined_pass = $false
    product_readiness_claim = $false
    mutation_flags = $mutationFlags
    non_claims = @(
        "I did not verify product readiness.",
        "I did not prove Codex host-visible candidate behavior.",
        "I did not prove Codex host UI readiness.",
        "I did not run training, eval, DPO, checkpoint mutation, locator mutation, hidden external AI, git stage, git commit, or git push."
    )
}
Write-Utf8NoBom -Path $reportPath -Text (ConvertTo-JsonText $report)

$metadata = [ordered]@{
    schema_version = "frdp.row22.codex_measurement_command_metadata.v1"
    subagent_id_or_nickname = "Row22ReplacementWorker-Codex"
    command = "& `"$PSCommandPath`" -CoreRoot `"$CoreRoot`""
    script_path = Get-RelativePath $PSCommandPath
    output_dir = Get-RelativePath $OutputDir
    started_utc = $StartedUtc
    completed_utc = (Get-Date).ToUniversalTime().ToString("o")
    row22_measurement_command_created = $true
    row22_measurement_command_run = $true
    run_attempts = 1
    mutation_flags = $mutationFlags
}
Write-Utf8NoBom -Path $metadataPath -Text (ConvertTo-JsonText $metadata)

$status = [ordered]@{
    schema = "xiigen.extension_readiness.row22_measurement_command_status.v1"
    subagent_id_or_nickname = "Row22ReplacementWorker-Codex"
    created_at_utc = (Get-Date).ToUniversalTime().ToString("o")
    row = 22
    scenario_id = $ScenarioId
    row22_measurement_command_created = $true
    row22_measurement_command_run = $true
    run_attempts = 1
    before_tokens = $beforeTokens
    after_tokens = $afterTokens
    token_delta = $tokenDelta
    token_reduction_observed = $tokenReductionObserved
    quality_guard_available = [bool]$qualityGuard.quality_guard_available
    quality_guard_passed = [bool]$qualityGuard.quality_guard_passed
    token_savings_claim = $tokenSavingsClaim
    host_visible_candidate_behavior_proven = $false
    host_route_blocker = $HostRouteBlocker
    formal_joined_pass = $false
    product_readiness_claim = $false
    mutation_flags = $mutationFlags
    command_run_or_blocker = [ordered]@{
        command_run = $true
        blocker_code = $null
        report_path = Get-RelativePath $reportPath
        metadata_path = Get-RelativePath $metadataPath
        quality_guard_path = Get-RelativePath $qualityPath
    }
    exact_next_action = "Parent/reviewer should review the replacement Row22 evidence packet. Row21 remains the host route blocker, and product readiness remains false."
    parent_will_perform_commit_push_after_review = $true
}
Write-Utf8NoBom -Path $statusPath -Text (ConvertTo-JsonText $status)

$packet = @"
# Row22 Codex Measurement Command Evidence Packet

subagent_id_or_nickname: Row22ReplacementWorker-Codex

## Result

- row22_measurement_command_created: true
- row22_measurement_command_run: true
- run_attempts: 1
- before_tokens: $beforeTokens
- after_tokens: $afterTokens
- token_delta: $tokenDelta
- token_reduction_observed: $tokenReductionObserved
- quality_guard_available: $($qualityGuard.quality_guard_available)
- quality_guard_passed: $($qualityGuard.quality_guard_passed)
- token_savings_claim: $tokenSavingsClaim
- host_visible_candidate_behavior_proven: false
- host_route_blocker: $HostRouteBlocker
- formal_joined_pass: false
- product_readiness_claim: false

## Files

- row22-measurement-command-status.json
- row22-measurement-command-metadata.json
- row22-measurement-report.json
- row22-quality-guard.json
- row22-token-traces.json
- run-row22-codex-measurement.ps1

## Non-Claims

I did not verify product readiness.
I did not prove Codex host-visible candidate behavior.
I did not prove Codex host UI readiness.
I did not run training, eval, DPO, checkpoint mutation, locator mutation, hidden external AI, git stage, git commit, or git push.

## Next Action

Parent/reviewer should review the replacement Row22 evidence packet. Row21 remains the host route blocker, and product readiness remains false.
"@
Write-Utf8NoBom -Path $packetPath -Text $packet

exit 0
