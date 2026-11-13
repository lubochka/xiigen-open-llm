param(
    [string]$CoreRoot = "C:\Projects\llm_mvp_core"
)

$ErrorActionPreference = "Stop"

$scenarioId = "frdp-p01-cursor-token-savings-baseline"
$pairId = "$scenarioId|Cursor|orchestration-plan|debug-false"
$method = "orchestration-plan"
$hostName = "Cursor"
$hostType = "cursor-local-adapter-harness"
$tokenizerName = "xiigen-heuristic-word-punctuation"
$tokenizerVersion = "v1"
$tokenizerAccountingType = "heuristic"
$scenarioDir = $PSScriptRoot
$marker = "FRDP-P01-$scenarioId"
$promptText = "For the Cursor token scenario, turn this product-readiness request into a bounded orchestration plan. Use the same workload for baseline and adaptive modes, keep evidence explicit, and do not claim token savings, semantic quality, training, checkpoint mutation, locator update, or product readiness."

function Write-Utf8NoBom {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Text
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

function ConvertTo-PrettyJson {
    param([Parameter(Mandatory = $true)]$Value)
    return $Value | ConvertTo-Json -Depth 100
}

function ConvertTo-CompactJson {
    param([Parameter(Mandatory = $true)]$Value)
    return $Value | ConvertTo-Json -Depth 100 -Compress
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

function New-Request {
    param(
        [Parameter(Mandatory = $true)][string]$Mode,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][string[]]$RequestedSkills,
        [Parameter(Mandatory = $true)][string[]]$AvailableSkills
    )

    [ordered]@{
        HostName = $hostName
        HostType = $hostType
        Method = $method
        UserPromptOrTaskText = $promptText
        HostFacts = [ordered]@{
            workspace = "workspace:$CoreRoot"
            host_client = "thin"
            scenario_id = $scenarioId
            marker = $marker
            pair_id = $pairId
            token_mode = $Mode
            proof_scope = "test-only adapter/core token proof; no real Cursor user interface claim"
        }
        DebugEnabled = $false
        RequestedSkills = @($RequestedSkills)
        AvailableSkills = @($AvailableSkills)
        CheckSetInputs = [ordered]@{
            scenario_id = $scenarioId
            marker = $marker
            token_mode = $Mode
            host_runtime_claimed = "false"
            semantic_quality_claimed = "false"
            token_savings_claimed = "false"
            external_ai = "forbidden"
            training_started = "false"
            checkpoint_mutation = "false"
            locator_update = "false"
            scope = "bounded-scenario-execution-token-proof"
        }
        Provenance = [ordered]@{
            source = "current Luba product-readiness execution packet"
            scenario_board_artifact = "docs/plans/evidence/extension-full-readiness-dual-path/product-readiness-scenario-board.md"
            scenario_execution_folder = "docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/$scenarioId"
            work_order_goal = "advance Cursor token scenario with safe local evidence"
        }
        CorrelationId = "frdp-board-cursor-token-$Mode-20260624"
    }
}

function Invoke-CoreApi {
    param(
        [Parameter(Mandatory = $true)][string]$Mode,
        [Parameter(Mandatory = $true)]$Request
    )

    $requestPath = Join-Path $scenarioDir "$Mode-request.json"
    $responsePath = Join-Path $scenarioDir "$Mode-response.json"
    $tracePath = Join-Path $scenarioDir "$Mode-core-trace.json"
    $stdoutPath = Join-Path $scenarioDir "$Mode-command-output.txt"
    $stderrPath = Join-Path $scenarioDir "$Mode-command-error.txt"
    $commandResultPath = Join-Path $scenarioDir "$Mode-command-result.json"

    Write-Utf8NoBom -Path $requestPath -Text (ConvertTo-PrettyJson $Request)

    $scriptPath = Join-Path $CoreRoot "tools\xiigen-extension-core-api.ps1"
    $startedUtc = (Get-Date).ToUniversalTime().ToString("o")
    $argumentList = @(
        "-NoProfile",
        "-ExecutionPolicy", "Bypass",
        "-File", $scriptPath,
        "-RequestPath", $requestPath,
        "-ResponsePath", $responsePath,
        "-TracePath", $tracePath,
        "-CoreRoot", $CoreRoot
    )

    $commandOutput = & powershell.exe @argumentList 2>&1
    $exitCode = $LASTEXITCODE
    $completedUtc = (Get-Date).ToUniversalTime().ToString("o")
    $stdoutText = ($commandOutput | Out-String)
    $stderrText = ""
    Write-Utf8NoBom -Path $stdoutPath -Text $stdoutText
    Write-Utf8NoBom -Path $stderrPath -Text $stderrText

    $quotedArgs = @($argumentList | ForEach-Object {
        if ($_ -match "\s") { '"' + $_ + '"' } else { $_ }
    })
    $commandText = "powershell.exe " + ($quotedArgs -join " ")
    $commandResult = [ordered]@{
        schema_version = "frdp.scenario.command_result.v1"
        mode = $Mode
        command = $commandText
        started_utc = $startedUtc
        completed_utc = $completedUtc
        exit_code = $exitCode
        request_path = Get-RelativePath $requestPath
        response_path = Get-RelativePath $responsePath
        trace_path = Get-RelativePath $tracePath
        stdout_path = Get-RelativePath $stdoutPath
        stderr_path = Get-RelativePath $stderrPath
        external_ai_used = $false
        training_started = $false
        checkpoint_mutated = $false
    }
    Write-Utf8NoBom -Path $commandResultPath -Text (ConvertTo-PrettyJson $commandResult)

    $response = $null
    if (Test-Path -LiteralPath $responsePath) {
        $response = Get-Content -Raw -LiteralPath $responsePath | ConvertFrom-Json
    }

    [ordered]@{
        mode = $Mode
        exit_code = $exitCode
        request_path = $requestPath
        response_path = $responsePath
        trace_path = $tracePath
        stdout_path = $stdoutPath
        stderr_path = $stderrPath
        command_result_path = $commandResultPath
        response = $response
    }
}

function New-TokenTrace {
    param(
        [Parameter(Mandatory = $true)]$Run,
        [Parameter(Mandatory = $true)][string]$Mode
    )

    $answer = if ($Run.response) { [string]$Run.response.ChatDisplayText } else { "" }
    $startedUtc = (Get-Date).ToUniversalTime().ToString("o")
    [ordered]@{
        SchemaVersion = "xiigen.extension_token_trace.v1"
        ScenarioId = $scenarioId
        PairId = $pairId
        Mode = $Mode
        HostName = $hostName
        Method = $method
        DebugEnabled = $false
        PromptHash = Get-Sha256 $promptText
        VisibleAnswerHash = Get-Sha256 $answer
        TokenizerName = $tokenizerName
        TokenizerVersion = $tokenizerVersion
        TokenizerAccountingType = $tokenizerAccountingType
        TokenCountInput = Get-HeuristicTokenCount $promptText
        TokenCountOutput = Get-HeuristicTokenCount $answer
        TokenCountTotal = (Get-HeuristicTokenCount $promptText) + (Get-HeuristicTokenCount $answer)
        QualityScore = $null
        QualityRegression = $null
        IsolationMode = "test_only_adapter_core_no_live_cursor_user_interface"
        TraceStartedAtUtc = $startedUtc
        TraceEndedAtUtc = (Get-Date).ToUniversalTime().ToString("o")
        ArtifactRefs = [ordered]@{
            request = Get-RelativePath $Run.request_path
            response = Get-RelativePath $Run.response_path
            core_trace = Get-RelativePath $Run.trace_path
            command_result = Get-RelativePath $Run.command_result_path
        }
        PromptRedacted = $true
        AnswerRedacted = $true
        ContainsRawPrivateContent = $false
        SemanticQualityClaimed = $false
        TokenSavingsClaimed = $false
        LearnabilityClaimed = $false
    }
}

function Test-Pair {
    param(
        [Parameter(Mandatory = $true)]$Baseline,
        [Parameter(Mandatory = $true)]$Adaptive
    )

    $errors = New-Object System.Collections.Generic.List[string]
    foreach ($field in @("ScenarioId", "PairId", "HostName", "Method", "DebugEnabled", "PromptHash", "TokenizerName", "TokenizerVersion", "TokenizerAccountingType")) {
        if ($Baseline.$field -ne $Adaptive.$field) {
            $errors.Add("$field mismatch")
        }
    }
    if ($Baseline.Mode -ne "baseline") { $errors.Add("baseline_mode_required") }
    if ($Adaptive.Mode -ne "adaptive") { $errors.Add("adaptive_mode_required") }
    if ($Baseline.TokenSavingsClaimed -or $Adaptive.TokenSavingsClaimed) { $errors.Add("token_savings_claim_forbidden_in_trace_schema_phase") }
    if ($Baseline.SemanticQualityClaimed -or $Adaptive.SemanticQualityClaimed) { $errors.Add("semantic_quality_claim_forbidden_in_trace_schema_phase") }
    if ($Baseline.LearnabilityClaimed -or $Adaptive.LearnabilityClaimed) { $errors.Add("learnability_claim_forbidden_in_trace_schema_phase") }

    $qualityGateReasons = New-Object System.Collections.Generic.List[string]
    if ($errors.Count -gt 0) {
        foreach ($error in $errors) { $qualityGateReasons.Add($error) }
    }
    if ($null -eq $Baseline.QualityScore -or $null -eq $Adaptive.QualityScore) {
        $qualityGateReasons.Add("token_delta_alone_cannot_pass_quality")
    }
    if ($null -eq $Baseline.QualityRegression -or $null -eq $Adaptive.QualityRegression) {
        $qualityGateReasons.Add("quality_regression_requires_approved_quality_rubric")
    }
    $qualityGateReasons.Add("live_cursor_candidate_behavior_not_proven")

    $tokenDelta = [int]$Adaptive.TokenCountTotal - [int]$Baseline.TokenCountTotal
    [ordered]@{
        schema_version = "frdp.cursor_token_pair_validation.v1"
        scenario_id = $scenarioId
        pair_id = $pairId
        pair_valid = $errors.Count -eq 0
        pair_validation_errors = @($errors)
        baseline_total_tokens = [int]$Baseline.TokenCountTotal
        adaptive_total_tokens = [int]$Adaptive.TokenCountTotal
        token_delta = $tokenDelta
        token_reduction_observed = $tokenDelta -lt 0
        quality_gate_can_pass = $false
        quality_gate_reasons = @($qualityGateReasons)
        token_savings_claim_allowed = $false
        token_savings_claimed = $false
        semantic_quality_claimed = $false
        host_runtime_claimed = $false
        external_ai_used = $false
        training_started = $false
        checkpoint_mutated = $false
        locator_updated = $false
    }
}

$baselineRequest = New-Request -Mode "baseline" -RequestedSkills @() -AvailableSkills @(
    "authority-governance",
    "plan-review",
    "runtime-QA",
    "test-integrity",
    "token-measurement"
)
$adaptiveRequest = New-Request -Mode "adaptive" -RequestedSkills @(
    "plan-review",
    "runtime-QA",
    "test-integrity",
    "token-measurement"
) -AvailableSkills @(
    "authority-governance",
    "extension-runtime",
    "plan-review",
    "privacy-redaction",
    "runtime-QA",
    "shortcut-audit",
    "test-integrity",
    "token-measurement"
)

$baselineRun = Invoke-CoreApi -Mode "baseline" -Request $baselineRequest
$adaptiveRun = Invoke-CoreApi -Mode "adaptive" -Request $adaptiveRequest

$baselineTrace = New-TokenTrace -Run $baselineRun -Mode "baseline"
$adaptiveTrace = New-TokenTrace -Run $adaptiveRun -Mode "adaptive"
$pair = Test-Pair -Baseline ([pscustomobject]$baselineTrace) -Adaptive ([pscustomobject]$adaptiveTrace)

$tracePath = Join-Path $scenarioDir "token-traces.json"
$traceJsonlPath = Join-Path $scenarioDir "token-traces.jsonl"
$pairPath = Join-Path $scenarioDir "pair-validation.json"
$reportJsonPath = Join-Path $scenarioDir "execution-report.json"
$reportMdPath = Join-Path $scenarioDir "execution-report.md"

Write-Utf8NoBom -Path $tracePath -Text (ConvertTo-PrettyJson ([ordered]@{
    schema_version = "frdp.cursor_token_traces_packet.v1"
    scenario_id = $scenarioId
    traces = @($baselineTrace, $adaptiveTrace)
}))
Write-Utf8NoBom -Path $traceJsonlPath -Text ((@($baselineTrace, $adaptiveTrace) | ForEach-Object { ConvertTo-CompactJson $_ }) -join "`n")
Write-Utf8NoBom -Path $pairPath -Text (ConvertTo-PrettyJson $pair)

$report = [ordered]@{
    schema_version = "frdp.scenario_execution_report.v1"
    selected_scenario = $scenarioId
    host = $hostName
    selected_reason = "Non-moved Cursor row with existing automated token/accounting guard path and feasible local adapter/core token trace action."
    command_exit_codes = [ordered]@{
        baseline = $baselineRun.exit_code
        adaptive = $adaptiveRun.exit_code
    }
    response_success = [ordered]@{
        baseline = [bool]$baselineRun.response.Success
        adaptive = [bool]$adaptiveRun.response.Success
    }
    token_counts = [ordered]@{
        baseline_input = $baselineTrace.TokenCountInput
        baseline_output = $baselineTrace.TokenCountOutput
        baseline_total = $baselineTrace.TokenCountTotal
        adaptive_input = $adaptiveTrace.TokenCountInput
        adaptive_output = $adaptiveTrace.TokenCountOutput
        adaptive_total = $adaptiveTrace.TokenCountTotal
        token_delta = $pair.token_delta
        token_reduction_observed = $pair.token_reduction_observed
    }
    pair_valid = $pair.pair_valid
    quality_gate_can_pass = $pair.quality_gate_can_pass
    quality_gate_reasons = $pair.quality_gate_reasons
    material_evidence_increase = ($baselineRun.exit_code -eq 0 -and $adaptiveRun.exit_code -eq 0 -and $pair.pair_valid)
    formal_joined_pass = $false
    no_overclaim = [ordered]@{
        product_readiness_claimed = $false
        full_extension_readiness_claimed = $false
        token_savings_claimed = $false
        token_savings_claim_allowed = $false
        semantic_quality_claimed = $false
        host_runtime_claimed = $false
        external_ai_used = $false
        training_started = $false
        checkpoint_mutated = $false
        locator_updated = $false
    }
    evidence_files = @(
        (Get-RelativePath (Join-Path $scenarioDir "baseline-request.json")),
        (Get-RelativePath (Join-Path $scenarioDir "baseline-response.json")),
        (Get-RelativePath (Join-Path $scenarioDir "baseline-core-trace.json")),
        (Get-RelativePath (Join-Path $scenarioDir "baseline-command-result.json")),
        (Get-RelativePath (Join-Path $scenarioDir "adaptive-request.json")),
        (Get-RelativePath (Join-Path $scenarioDir "adaptive-response.json")),
        (Get-RelativePath (Join-Path $scenarioDir "adaptive-core-trace.json")),
        (Get-RelativePath (Join-Path $scenarioDir "adaptive-command-result.json")),
        (Get-RelativePath $tracePath),
        (Get-RelativePath $traceJsonlPath),
        (Get-RelativePath $pairPath),
        (Get-RelativePath $reportJsonPath),
        (Get-RelativePath $reportMdPath)
    )
    what_this_proves = @(
        "A Cursor-shaped baseline and adaptive request both reached SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest through the local Application Programming Interface (API) harness.",
        "The same prompt hash, host, method, debug flag, tokenizer name, tokenizer version, and tokenizer accounting type were used for the baseline/adaptive token pair.",
        "Heuristic token counts were recorded from the actual core responses and the token-savings gate stayed closed."
    )
    what_this_does_not_prove = @(
        "No real Cursor user interface (UI), extension command activation, prompt intake, or host-visible candidate behavior was proven.",
        "No token savings, semantic quality improvement, product readiness, full extension readiness, learnability, training, checkpoint mutation, or locator update was proven.",
        "The quality gate cannot pass because token delta alone is not quality evidence and no approved quality regression rubric was attached."
    )
    next_concrete_action = "Run a safe Cursor extension-host or no-send local route for the same token scenario, or attach approved quality non-regression evidence to the same baseline/adaptive token pair before any token-savings claim is reconsidered."
}
Write-Utf8NoBom -Path $reportJsonPath -Text (ConvertTo-PrettyJson $report)

$md = @(
    "# Scenario execution report - Cursor token savings baseline",
    "",
    "CURRENT TASK: advance the next concrete product-readiness scenario with local evidence, not a status-only artifact.",
    "NOT THE TASK: product readiness claim, full Cursor readiness claim, real Cursor user interface (UI) claim, token savings claim, semantic quality improvement claim, training, checkpoint mutation, locator update, commit, push, or stage.",
    "EVIDENCE-ONLY: product readiness scenario board, Phase 03 token/accounting guard evidence, and this scenario-specific adapter/core token trace packet.",
    "",
    "## Scenario",
    "",
    "| Field | Value |",
    "|---|---|",
    "| Scenario number | 12 of 28 |",
    "| Scenario identifier | ``$scenarioId`` |",
    "| Host | Cursor |",
    "| Claim family | token savings baseline for Cursor |",
    "| Marker | ``$marker`` |",
    "| Selected because | Row 12 was not moved and had a feasible local automated/core/Application Programming Interface (API) path without external AI or unsafe prompt sending. |",
    "",
    "## Commands actually run",
    "",
    "| Mode | Exit code | Command result | Output log | Error log |",
    "|---|---:|---|---|---|",
    "| baseline | $($baselineRun.exit_code) | [baseline-command-result.json](baseline-command-result.json) | [baseline-command-output.txt](baseline-command-output.txt) | [baseline-command-error.txt](baseline-command-error.txt) |",
    "| adaptive | $($adaptiveRun.exit_code) | [adaptive-command-result.json](adaptive-command-result.json) | [adaptive-command-output.txt](adaptive-command-output.txt) | [adaptive-command-error.txt](adaptive-command-error.txt) |",
    "",
    "## Concrete result numbers",
    "",
    "| Check | Result |",
    "|---|---:|",
    "| Baseline response success | $([bool]$baselineRun.response.Success) |",
    "| Adaptive response success | $([bool]$adaptiveRun.response.Success) |",
    "| Pair valid | $($pair.pair_valid) |",
    "| Baseline input tokens | $($baselineTrace.TokenCountInput) |",
    "| Baseline output tokens | $($baselineTrace.TokenCountOutput) |",
    "| Baseline total tokens | $($baselineTrace.TokenCountTotal) |",
    "| Adaptive input tokens | $($adaptiveTrace.TokenCountInput) |",
    "| Adaptive output tokens | $($adaptiveTrace.TokenCountOutput) |",
    "| Adaptive total tokens | $($adaptiveTrace.TokenCountTotal) |",
    "| Token delta | $($pair.token_delta) |",
    "| Token reduction observed | $($pair.token_reduction_observed) |",
    "| Quality gate can pass | $($pair.quality_gate_can_pass) |",
    "| Token savings claim allowed | $($pair.token_savings_claim_allowed) |",
    "| Token savings claimed | $($pair.token_savings_claimed) |",
    "| Semantic quality claimed | $($pair.semantic_quality_claimed) |",
    "| Host runtime claimed | $($pair.host_runtime_claimed) |",
    "| External AI used | $($pair.external_ai_used) |",
    "| Training started | $($pair.training_started) |",
    "| Checkpoint mutated | $($pair.checkpoint_mutated) |",
    "| Locator updated | $($pair.locator_updated) |",
    "",
    "## What moved",
    "",
    "This scenario moved from no scenario-specific execution packet to a partial local evidence packet. The fresh run proves that a Cursor-shaped baseline/adaptive pair can reach `SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest`, emit actual core responses, and produce paired heuristic token traces with the same prompt hash, host, method, debug flag, tokenizer name, tokenizer version, and tokenizer accounting type.",
    "",
    "The evidence does not prove token savings. The quality gate is closed because token delta alone is not quality evidence, no approved quality regression rubric is attached, and live Cursor candidate behavior is still not proven.",
    "",
    "## Evidence files",
    "",
    "- [baseline-request.json](baseline-request.json)",
    "- [baseline-response.json](baseline-response.json)",
    "- [baseline-core-trace.json](baseline-core-trace.json)",
    "- [adaptive-request.json](adaptive-request.json)",
    "- [adaptive-response.json](adaptive-response.json)",
    "- [adaptive-core-trace.json](adaptive-core-trace.json)",
    "- [token-traces.json](token-traces.json)",
    "- [token-traces.jsonl](token-traces.jsonl)",
    "- [pair-validation.json](pair-validation.json)",
    "- [execution-report.json](execution-report.json)",
    "",
    "## Next concrete action",
    "",
    $report.next_concrete_action
) -join "`n"
Write-Utf8NoBom -Path $reportMdPath -Text $md

if ($baselineRun.exit_code -ne 0 -or $adaptiveRun.exit_code -ne 0 -or -not $pair.pair_valid) {
    exit 1
}

exit 0
