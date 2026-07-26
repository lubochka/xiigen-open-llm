param(
    [Parameter(Mandatory = $true)]
    [string]$ScenarioPackPath,

    [string]$OutputDir = "artifacts/extension-runtime-token-quality-learnability",

    [string]$RunId = "token-runner-dry-run",

    [switch]$WriteArtifacts
)

$ErrorActionPreference = "Stop"

$BaselineMode = "baseline_common_model_only"
$AdaptiveMode = "adaptive_common_plus_user_model"
$TraceSchemaVersion = "xiigen.extension_token_runner_trace.v1"
$PairSchemaVersion = "xiigen.extension_token_runner_pair_result.v1"
$ReportSchemaVersion = "xiigen.extension_token_runner_report.v1"
$TokenizerName = "xiigen-heuristic-word-punctuation"
$TokenizerVersion = "v1"
$TokenizerAccountingType = "heuristic"
$RequiredModes = @($BaselineMode, $AdaptiveMode)
$RequiredScenarioFields = @("scenario_id", "prompt_text", "hosts", "modes", "methods_to_run")

function Resolve-TokenRunnerPath {
    param([Parameter(Mandatory = $true)][string]$Path)

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return [System.IO.Path]::GetFullPath($Path)
    }

    return [System.IO.Path]::GetFullPath((Join-Path (Get-Location).Path $Path))
}

function ConvertTo-TokenRunnerJson {
    param([Parameter(Mandatory = $true)]$Value)

    return $Value | ConvertTo-Json -Depth 80 -Compress
}

function Write-TokenRunnerText {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Text
    )

    $directory = Split-Path -Parent $Path
    if ($directory -and -not (Test-Path -LiteralPath $directory)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }

    $normalized = $Text.Replace("`r`n", "`n").Replace("`r", "`n")
    if (-not $normalized.EndsWith("`n")) {
        $normalized += "`n"
    }

    [System.IO.File]::WriteAllText($Path, $normalized, [System.Text.UTF8Encoding]::new($false))
}

function Get-TokenRunnerProperty {
    param(
        [Parameter(Mandatory = $true)]$Object,
        [Parameter(Mandatory = $true)][string]$Name
    )

    $property = $Object.PSObject.Properties | Where-Object { $_.Name -eq $Name } | Select-Object -First 1
    if ($property) {
        return $property.Value
    }

    return $null
}

function Test-TokenRunnerProperty {
    param(
        [Parameter(Mandatory = $true)]$Object,
        [Parameter(Mandatory = $true)][string]$Name
    )

    return $null -ne ($Object.PSObject.Properties | Where-Object { $_.Name -eq $Name } | Select-Object -First 1)
}

function Get-TokenRunnerStringArray {
    param($Value)

    if ($null -eq $Value) {
        return @()
    }

    if ($Value -is [string]) {
        return @($Value)
    }

    return @($Value | ForEach-Object { [string]$_ })
}

function Get-TokenRunnerBool {
    param(
        $Object,
        [string]$Name,
        [bool]$Default = $false
    )

    if ($null -eq $Object -or -not (Test-TokenRunnerProperty -Object $Object -Name $Name)) {
        return $Default
    }

    return [bool](Get-TokenRunnerProperty -Object $Object -Name $Name)
}

function Get-TokenRunnerText {
    param(
        $Object,
        [string]$Name,
        [string]$Default
    )

    if ($null -eq $Object -or -not (Test-TokenRunnerProperty -Object $Object -Name $Name)) {
        return $Default
    }

    $value = [string](Get-TokenRunnerProperty -Object $Object -Name $Name)
    if ([string]::IsNullOrWhiteSpace($value)) {
        return $Default
    }

    return $value
}

function Get-TokenRunnerSha256 {
    param([string]$Text)

    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Text)
    $sha256 = [System.Security.Cryptography.SHA256]::Create()
    try {
        $hash = $sha256.ComputeHash($bytes)
    }
    finally {
        $sha256.Dispose()
    }

    return "sha256:" + ([System.BitConverter]::ToString($hash) -replace "-", "").ToLowerInvariant()
}

function Get-TokenRunnerRelativePath {
    param(
        [Parameter(Mandatory = $true)][string]$BasePath,
        [Parameter(Mandatory = $true)][string]$Path
    )

    $baseUriText = [System.IO.Path]::GetFullPath($BasePath)
    if (-not $baseUriText.EndsWith([System.IO.Path]::DirectorySeparatorChar)) {
        $baseUriText += [System.IO.Path]::DirectorySeparatorChar
    }

    $baseUri = [Uri]$baseUriText
    $pathUri = [Uri][System.IO.Path]::GetFullPath($Path)
    return [Uri]::UnescapeDataString($baseUri.MakeRelativeUri($pathUri).ToString()).Replace("/", "\")
}

function Get-TokenRunnerHeuristicTokenCount {
    param([string]$Text)

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

function Read-TokenRunnerScenarioPack {
    param([Parameter(Mandatory = $true)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Scenario pack does not exist: $Path"
    }

    $extension = [System.IO.Path]::GetExtension($Path).ToLowerInvariant()
    if ($extension -eq ".jsonl") {
        $rows = @()
        $lineNumber = 0
        foreach ($line in [System.IO.File]::ReadLines($Path, [System.Text.Encoding]::UTF8)) {
            $lineNumber++
            if ([string]::IsNullOrWhiteSpace($line)) {
                continue
            }

            try {
                $rows += ($line | ConvertFrom-Json)
            }
            catch {
                throw "Malformed JSONL row at line $lineNumber in $Path`: $($_.Exception.Message)"
            }
        }

        return $rows
    }

    $raw = Get-Content -Raw -LiteralPath $Path
    try {
        $json = $raw | ConvertFrom-Json
    }
    catch {
        throw "Malformed JSON scenario pack $Path`: $($_.Exception.Message)"
    }

    if ($json -is [array]) {
        return @($json)
    }

    if (Test-TokenRunnerProperty -Object $json -Name "scenarios") {
        return @(Get-TokenRunnerProperty -Object $json -Name "scenarios")
    }

    if (Test-TokenRunnerProperty -Object $json -Name "scenario_id") {
        return @($json)
    }

    throw "JSON scenario pack must be an array, an object with scenarios, or a single scenario object."
}

function Test-TokenRunnerScenarioPack {
    param([Parameter(Mandatory = $true)][object[]]$Scenarios)

    $errors = New-Object System.Collections.Generic.List[string]
    $seen = @{}

    if ($Scenarios.Count -eq 0) {
        $errors.Add("scenario_pack_must_contain_at_least_one_row")
    }

    for ($index = 0; $index -lt $Scenarios.Count; $index++) {
        $scenario = $Scenarios[$index]
        $rowName = "row_$($index + 1)"

        foreach ($field in $RequiredScenarioFields) {
            if (-not (Test-TokenRunnerProperty -Object $scenario -Name $field)) {
                $errors.Add("$rowName`_missing_required_field_$field")
            }
        }

        $scenarioId = [string](Get-TokenRunnerProperty -Object $scenario -Name "scenario_id")
        if ([string]::IsNullOrWhiteSpace($scenarioId)) {
            $errors.Add("$rowName`_scenario_id_empty")
        }
        elseif ($seen.ContainsKey($scenarioId)) {
            $errors.Add("duplicate_scenario_id_$scenarioId")
        }
        else {
            $seen[$scenarioId] = $true
        }

        $hosts = Get-TokenRunnerStringArray (Get-TokenRunnerProperty -Object $scenario -Name "hosts")
        if ($hosts.Count -eq 0 -or ($hosts | Where-Object { [string]::IsNullOrWhiteSpace($_) }).Count -gt 0) {
            $errors.Add("$rowName`_missing_host")
        }

        $modes = Get-TokenRunnerStringArray (Get-TokenRunnerProperty -Object $scenario -Name "modes")
        if ($modes.Count -eq 0) {
            $errors.Add("$rowName`_missing_mode")
        }
        foreach ($mode in $RequiredModes) {
            if (-not ($modes -contains $mode)) {
                $errors.Add("$rowName`_missing_mode_$mode")
            }
        }

        $methods = Get-TokenRunnerStringArray (Get-TokenRunnerProperty -Object $scenario -Name "methods_to_run")
        if ($methods.Count -eq 0 -or ($methods | Where-Object { [string]::IsNullOrWhiteSpace($_) }).Count -gt 0) {
            $errors.Add("$rowName`_missing_method")
        }

        $prompt = [string](Get-TokenRunnerProperty -Object $scenario -Name "prompt_text")
        if ([string]::IsNullOrWhiteSpace($prompt)) {
            $errors.Add("$rowName`_prompt_text_empty")
        }
    }

    return [ordered]@{
        valid = $errors.Count -eq 0
        errors = @($errors)
    }
}

function Get-TokenRunnerQualityScores {
    param($Scenario)

    if (Test-TokenRunnerProperty -Object $Scenario -Name "quality_rubric_scores") {
        return Get-TokenRunnerProperty -Object $Scenario -Name "quality_rubric_scores"
    }

    $scores = [ordered]@{}
    $rubric = Get-TokenRunnerProperty -Object $Scenario -Name "scoring_rubric"
    if ($null -ne $rubric) {
        foreach ($property in $rubric.PSObject.Properties) {
            $scores[$property.Name] = [ordered]@{
                score = $null
                status = "PLACEHOLDER_NOT_SCORED"
                rubric_ref = [string]$property.Value
            }
        }
    }

    if ($scores.Count -eq 0) {
        $scores["unscored"] = [ordered]@{
            score = $null
            status = "PLACEHOLDER_NOT_SCORED"
            rubric_ref = "No scoring_rubric field was available."
        }
    }

    return $scores
}

function New-TokenRunnerTrace {
    param(
        [Parameter(Mandatory = $true)]$Scenario,
        [Parameter(Mandatory = $true)][string]$HostName,
        [Parameter(Mandatory = $true)][string]$Method,
        [Parameter(Mandatory = $true)][string]$Mode,
        [Parameter(Mandatory = $true)][string]$PairId,
        [Parameter(Mandatory = $true)][string]$OutputText,
        [Parameter(Mandatory = $true)][string]$ScenarioPackRef
    )

    $scenarioId = [string](Get-TokenRunnerProperty -Object $Scenario -Name "scenario_id")
    $promptText = [string](Get-TokenRunnerProperty -Object $Scenario -Name "prompt_text")
    $promptTokens = Get-TokenRunnerHeuristicTokenCount $promptText
    $outputTokens = Get-TokenRunnerHeuristicTokenCount $OutputText
    $qualityPassed = Get-TokenRunnerBool -Object $Scenario -Name "quality_non_regression_passed" -Default $false
    $shortcutPassed = Get-TokenRunnerBool -Object $Scenario -Name "no_deterministic_shortcut_passed" -Default $false

    return [ordered]@{
        schema_version = $TraceSchemaVersion
        run_id = $RunId
        scenario_id = $scenarioId
        pair_id = $PairId
        mode = $Mode
        host = $HostName
        method = $Method
        debug_enabled = $false
        prompt_sha256 = Get-TokenRunnerSha256 $promptText
        prompt_tokens = $promptTokens
        output_tokens = $outputTokens
        total_tokens = $promptTokens + $outputTokens
        tokenizer_name = $TokenizerName
        tokenizer_version = $TokenizerVersion
        tokenizer_accounting_type = $TokenizerAccountingType
        debug_trace_refs = [ordered]@{
            scenario_pack = $ScenarioPackRef
            host_runtime_trace = "NOT_RUN_TOKEN_02_STATIC_HARNESS"
            quality_trace = "PLACEHOLDER_PENDING_QUALITY_PHASE"
        }
        quality_rubric_scores = Get-TokenRunnerQualityScores -Scenario $Scenario
        quality_non_regression_passed = $qualityPassed
        token_savings_claim_allowed = $false
        no_deterministic_shortcut_gate = [ordered]@{
            required = $true
            passed = $shortcutPassed
            evidence = if ($shortcutPassed) { "explicit scenario quality fixture" } else { "not proven in TOKEN-02 dry-run/static harness" }
        }
        redaction = [ordered]@{
            prompt_redacted = $true
            output_redacted = $true
            contains_raw_private_content = $false
        }
        no_overclaim = [ordered]@{
            token_savings_claimed = $false
            quality_improvement_claimed = $false
            learnability_claimed = $false
            semantic_quality_claimed = $false
            host_runtime_claimed = $false
            training_started = $false
            checkpoint_mutated = $false
        }
    }
}

function New-TokenRunnerPairResult {
    param(
        [Parameter(Mandatory = $true)]$Scenario,
        [Parameter(Mandatory = $true)]$BaselineTrace,
        [Parameter(Mandatory = $true)]$AdaptiveTrace
    )

    $qualityPassed = [bool]$AdaptiveTrace.quality_non_regression_passed
    $shortcutPassed = [bool]$AdaptiveTrace.no_deterministic_shortcut_gate.passed
    $tokenDelta = [int]$AdaptiveTrace.total_tokens - [int]$BaselineTrace.total_tokens
    $tokenReductionObserved = $tokenDelta -lt 0
    $tokenSavingsClaimAllowed = $tokenReductionObserved -and $qualityPassed -and $shortcutPassed
    $failureReasons = New-Object System.Collections.Generic.List[string]

    if (-not $qualityPassed) {
        $failureReasons.Add("quality_non_regression_not_proven")
    }
    if (-not $shortcutPassed) {
        $failureReasons.Add("no_deterministic_shortcut_gate_not_proven")
    }
    if (-not $tokenReductionObserved) {
        $failureReasons.Add("adaptive_token_count_not_lower")
    }

    $BaselineTrace.token_savings_claim_allowed = $false
    $AdaptiveTrace.token_savings_claim_allowed = [bool]$tokenSavingsClaimAllowed

    return [ordered]@{
        schema_version = $PairSchemaVersion
        run_id = $RunId
        scenario_id = $BaselineTrace.scenario_id
        pair_id = $BaselineTrace.pair_id
        host = $BaselineTrace.host
        method = $BaselineTrace.method
        baseline_mode = $BaselineTrace.mode
        adaptive_mode = $AdaptiveTrace.mode
        baseline_total_tokens = $BaselineTrace.total_tokens
        adaptive_total_tokens = $AdaptiveTrace.total_tokens
        token_delta = $tokenDelta
        token_reduction_observed = $tokenReductionObserved
        quality_rubric_scores = $AdaptiveTrace.quality_rubric_scores
        quality_non_regression_passed = $qualityPassed
        no_deterministic_shortcut_gate = $AdaptiveTrace.no_deterministic_shortcut_gate
        token_savings_claim_allowed = [bool]$tokenSavingsClaimAllowed
        failure_reasons = @($failureReasons)
        claim_scope = "Static TOKEN-02 runner/harness evidence only; not host runtime, not semantic quality, not learnability."
    }
}

$resolvedScenarioPackPath = Resolve-TokenRunnerPath $ScenarioPackPath
$resolvedOutputDir = Resolve-TokenRunnerPath $OutputDir
$scenarioPackRef = (Get-TokenRunnerRelativePath -BasePath (Get-Location).Path -Path $resolvedScenarioPackPath).Replace("\", "/")
$scenarios = @(Read-TokenRunnerScenarioPack -Path $resolvedScenarioPackPath)
$validation = Test-TokenRunnerScenarioPack -Scenarios $scenarios

if (-not $validation.valid) {
    $invalidReport = [ordered]@{
        schema = $ReportSchemaVersion
        phase_id = "TOKEN-02"
        run_id = $RunId
        verdict = "FAIL_SCENARIO_PACK_VALIDATION"
        scenario_pack = $scenarioPackRef
        validation = $validation
        token_savings_claim_allowed = $false
        no_overclaim = [ordered]@{
            token_savings_claimed = $false
            quality_improvement_claimed = $false
            learnability_claimed = $false
            semantic_quality_claimed = $false
            host_runtime_claimed = $false
            training_started = $false
            checkpoint_mutated = $false
        }
    }

    if ($WriteArtifacts) {
        Write-TokenRunnerText -Path (Join-Path $resolvedOutputDir "token-runner-report.json") -Text (ConvertTo-TokenRunnerJson $invalidReport)
    }

    Write-Output (ConvertTo-TokenRunnerJson $invalidReport)
    exit 2
}

$traces = New-Object System.Collections.Generic.List[object]
$pairResults = New-Object System.Collections.Generic.List[object]

foreach ($scenario in $scenarios) {
    $scenarioId = [string](Get-TokenRunnerProperty -Object $scenario -Name "scenario_id")
    $hosts = Get-TokenRunnerStringArray (Get-TokenRunnerProperty -Object $scenario -Name "hosts")
    $methods = Get-TokenRunnerStringArray (Get-TokenRunnerProperty -Object $scenario -Name "methods_to_run")
    $baselineOutputText = Get-TokenRunnerText -Object $scenario -Name "baseline_output_text" -Default "Baseline TOKEN-02 placeholder output for $scenarioId. Quality is not scored."
    $adaptiveOutputText = Get-TokenRunnerText -Object $scenario -Name "adaptive_output_text" -Default "Adaptive TOKEN-02 placeholder output for $scenarioId. Quality is not scored."

    foreach ($hostName in $hosts) {
        foreach ($method in $methods) {
            $pairId = "$scenarioId|$hostName|$method|debug-false"
            $baselineTrace = New-TokenRunnerTrace -Scenario $scenario -HostName $hostName -Method $method -Mode $BaselineMode -PairId $pairId -OutputText $baselineOutputText -ScenarioPackRef $scenarioPackRef
            $adaptiveTrace = New-TokenRunnerTrace -Scenario $scenario -HostName $hostName -Method $method -Mode $AdaptiveMode -PairId $pairId -OutputText $adaptiveOutputText -ScenarioPackRef $scenarioPackRef
            $pairResult = New-TokenRunnerPairResult -Scenario $scenario -BaselineTrace $baselineTrace -AdaptiveTrace $adaptiveTrace

            $traces.Add($baselineTrace)
            $traces.Add($adaptiveTrace)
            $pairResults.Add($pairResult)
        }
    }
}

$claimAllowedCount = @($pairResults | Where-Object { $_.token_savings_claim_allowed }).Count
$report = [ordered]@{
    schema = $ReportSchemaVersion
    phase_id = "TOKEN-02"
    run_id = $RunId
    verdict = "PASS_STATIC_TOKEN_RUNNER_HARNESS_ARTIFACTS_WRITTEN_NO_REAL_SAVINGS_CLAIM"
    scenario_pack = $scenarioPackRef
    scenario_count = $scenarios.Count
    trace_count = $traces.Count
    pair_count = $pairResults.Count
    modes = $RequiredModes
    token_savings_claim_allowed_count = $claimAllowedCount
    token_savings_claim_allowed = $claimAllowedCount -gt 0
    quality_non_regression_required_for_token_savings = $true
    no_deterministic_shortcut_gate_required = $true
    validation = $validation
    artifacts = [ordered]@{
        trace_jsonl = "token-runner-traces.jsonl"
        pair_results_jsonl = "token-runner-pair-results.jsonl"
        report_json = "token-runner-report.json"
        report_md = "token-runner-report.md"
    }
    runner_command_template = "powershell -NoProfile -ExecutionPolicy Bypass -File tools/token-quality-runner.ps1 -ScenarioPackPath artifacts/extension-runtime-token-quality-learnability/scenario-pack-token-quality-v1.jsonl -OutputDir artifacts/extension-runtime-token-quality-learnability -RunId <run-id> -WriteArtifacts"
    no_overclaim = [ordered]@{
        token_savings_claimed = $false
        quality_improvement_claimed = $false
        learnability_claimed = $false
        semantic_quality_claimed = $false
        host_runtime_claimed = $false
        training_started = $false
        checkpoint_mutated = $false
        full_readiness_claimed = $false
    }
    what_this_proves = @(
        "The TOKEN-01 scenario pack can be parsed and validated by a static runner.",
        "The runner emits explicit baseline/adaptive dimensions and paired token trace rows.",
        "The token-savings claim gate stays closed unless quality non-regression and no deterministic shortcut gates pass."
    )
    what_this_does_not_prove = @(
        "No live Claude/Cursor/Codex host runtime was launched.",
        "No real token savings, quality improvement, semantic quality, learnability, DPO, training, checkpoint mutation, or full readiness is proven."
    )
    next_independent_track = "TOKEN-03-RUN-BASELINE-ADAPTIVE-TRACES: run or ingest real paired baseline/adaptive traces against the validated scenario pack, then feed them into quality scoring before any token improvement claim."
}

if ($WriteArtifacts) {
    $traceLines = ($traces | ForEach-Object { ConvertTo-TokenRunnerJson $_ }) -join "`n"
    $pairLines = ($pairResults | ForEach-Object { ConvertTo-TokenRunnerJson $_ }) -join "`n"
    $reportJson = ConvertTo-TokenRunnerJson $report
    $reportMd = @"
# TOKEN-02 runner harness report

Verdict: $($report.verdict)

What this proves:
- The TOKEN-01 scenario pack was parsed and validated.
- Baseline/adaptive dimensions were emitted as paired static trace rows.
- Token savings remain claim-blocked unless quality non-regression and no deterministic shortcut gates both pass.

What this does not prove:
- This does not prove live Claude/Cursor/Codex runtime.
- This does not prove real token savings, quality improvement, semantic quality, learnability, DPO, training, checkpoint mutation, rollback, or full readiness.

Next independent track:
- $($report.next_independent_track)

Runner command:

~~~powershell
$($report.runner_command_template)
~~~
"@

    Write-TokenRunnerText -Path (Join-Path $resolvedOutputDir "token-runner-traces.jsonl") -Text $traceLines
    Write-TokenRunnerText -Path (Join-Path $resolvedOutputDir "token-runner-pair-results.jsonl") -Text $pairLines
    Write-TokenRunnerText -Path (Join-Path $resolvedOutputDir "token-runner-report.json") -Text $reportJson
    Write-TokenRunnerText -Path (Join-Path $resolvedOutputDir "token-runner-report.md") -Text $reportMd
}

Write-Output (ConvertTo-TokenRunnerJson $report)
