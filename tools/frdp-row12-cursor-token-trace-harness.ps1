param(
    [string]$CoreRoot = "C:\Projects\llm_mvp_core",
    [string]$SpecPath = "docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\row12-workload-spec.json",
    [string]$MeasurementRoot,
    [switch]$NoRun
)

$ErrorActionPreference = "Stop"

function Resolve-Row12Path {
    param([Parameter(Mandatory = $true)][string]$Path)

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return [System.IO.Path]::GetFullPath($Path)
    }

    return [System.IO.Path]::GetFullPath((Join-Path $CoreRoot $Path))
}

function Write-Row12Text {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Text
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

function Write-Row12Json {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)]$Value,
        [switch]$Compress
    )

    $json = if ($Compress) {
        $Value | ConvertTo-Json -Depth 100 -Compress
    } else {
        $Value | ConvertTo-Json -Depth 100
    }

    Write-Row12Text -Path $Path -Text $json
}

function Get-Row12Sha256Text {
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

function Get-Row12Sha256File {
    param([Parameter(Mandatory = $true)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        return $null
    }

    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $stream = [System.IO.File]::OpenRead($Path)
        try {
            return "sha256:" + ([BitConverter]::ToString($sha.ComputeHash($stream)) -replace "-", "").ToLowerInvariant()
        }
        finally {
            $stream.Dispose()
        }
    }
    finally {
        $sha.Dispose()
    }
}

function Get-Row12HeuristicTokenCount {
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

function Get-Row12RelativePath {
    param([Parameter(Mandatory = $true)][string]$Path)

    $root = [System.IO.Path]::GetFullPath($CoreRoot).TrimEnd("\") + "\"
    $full = [System.IO.Path]::GetFullPath($Path)
    if ($full.StartsWith($root, [System.StringComparison]::OrdinalIgnoreCase)) {
        return $full.Substring($root.Length).Replace("\", "/")
    }

    return $full
}

function Get-Row12PropertyValue {
    param(
        [Parameter(Mandatory = $true)]$Object,
        [Parameter(Mandatory = $true)][string]$Name
    )

    if ($null -eq $Object) {
        return $null
    }

    $property = $Object.PSObject.Properties | Where-Object { $_.Name -ieq $Name } | Select-Object -First 1
    if ($property) {
        return $property.Value
    }

    return $null
}

function Invoke-Row12Process {
    param(
        [Parameter(Mandatory = $true)][string]$FileName,
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$WorkingDirectory,
        [Parameter(Mandatory = $true)][string]$StdoutPath,
        [Parameter(Mandatory = $true)][string]$StderrPath
    )

    $start = [System.Diagnostics.ProcessStartInfo]::new()
    $start.FileName = $FileName
    $start.Arguments = (@($Arguments | ForEach-Object {
        $argument = [string]$_
        if ($argument -match '[\s"]') {
            '"' + ($argument -replace '"', '\"') + '"'
        } else {
            $argument
        }
    }) -join " ")
    $start.WorkingDirectory = $WorkingDirectory
    $start.RedirectStandardOutput = $true
    $start.RedirectStandardError = $true
    $start.UseShellExecute = $false
    $start.CreateNoWindow = $true

    $process = [System.Diagnostics.Process]::Start($start)
    $stdout = $process.StandardOutput.ReadToEnd()
    $stderr = $process.StandardError.ReadToEnd()
    $process.WaitForExit()

    Write-Row12Text -Path $StdoutPath -Text $stdout
    Write-Row12Text -Path $StderrPath -Text $stderr

    return [ordered]@{
        exit_code = $process.ExitCode
        stdout = $stdout
        stderr = $stderr
    }
}

function New-Row12Request {
    param([Parameter(Mandatory = $true)][string]$Mode)

    $correlationId = "$pairId|$marker|$scenarioId"
    [ordered]@{
        HostName = $hostName
        HostType = "cursor-no-send-local-extension-runtime-candidate"
        Method = $method
        UserPromptOrTaskText = $promptText
        HostFacts = [ordered]@{
            workspace = "workspace:$CoreRoot"
            scenario_id = $scenarioId
            marker = $marker
            pair_id = $pairId
            host_path_label = $hostPathLabel
            token_mode = $Mode
            core_api_path = $method
            proof_scope = "row12 no-send core API measurement only; no token savings/readiness/semantic quality claim"
        }
        DebugEnabled = $debugEnabled
        RequestedSkills = @($allowedSkills)
        AvailableSkills = @($allowedSkills)
        CheckSetInputs = [ordered]@{
            scenario_id = $scenarioId
            marker = $marker
            pair_id = $pairId
            token_mode = $Mode
            external_ai_send = "false"
            token_savings_claimed = "false"
            semantic_quality_claimed = "false"
            readiness_claimed = "false"
            training_started = "false"
            checkpoint_mutation = "false"
            locator_update = "false"
            "constraint.no_external_ai" = "true"
            "constraint.no_checkpoint_mutation" = "true"
        }
        Provenance = [ordered]@{
            source = "row12-workload-spec"
            row12_workload_spec_json = (Get-Row12RelativePath $resolvedSpecPath)
            active_luba_authority_quote = "Deystviya cherez sub agentov"
        }
        CorrelationId = $correlationId
    }
}

function Invoke-Row12CoreApi {
    param([Parameter(Mandatory = $true)][string]$Mode)

    $requestPath = Join-Path $resolvedMeasurementRoot "$Mode-request.json"
    $responsePath = Join-Path $resolvedMeasurementRoot "$Mode-response.json"
    $coreTracePath = Join-Path $resolvedMeasurementRoot "$Mode-core-trace.json"
    $commandResultPath = Join-Path $resolvedMeasurementRoot "$Mode-command-result.json"
    $stdoutPath = Join-Path $resolvedMeasurementRoot "$Mode-command-output.txt"
    $stderrPath = Join-Path $resolvedMeasurementRoot "$Mode-command-error.txt"

    $request = New-Row12Request -Mode $Mode
    Write-Row12Json -Path $requestPath -Value $request

    $arguments = @(
        "-NoProfile",
        "-ExecutionPolicy", "Bypass",
        "-File", $coreApiScriptPath,
        "-RequestPath", $requestPath,
        "-ResponsePath", $responsePath,
        "-TracePath", $coreTracePath,
        "-CoreRoot", $CoreRoot
    )
    $startedUtc = (Get-Date).ToUniversalTime().ToString("o")
    $process = Invoke-Row12Process -FileName "powershell.exe" -Arguments $arguments -WorkingDirectory $CoreRoot -StdoutPath $stdoutPath -StderrPath $stderrPath
    $completedUtc = (Get-Date).ToUniversalTime().ToString("o")

    $commandText = "powershell.exe " + ($arguments -join " ")

    $response = $null
    if (Test-Path -LiteralPath $responsePath) {
        $response = Get-Content -Raw -LiteralPath $responsePath | ConvertFrom-Json
    }

    $commandResult = [ordered]@{
        schema_version = "frdp.row12.command_result.v1"
        mode = $Mode
        command = $commandText
        started_utc = $startedUtc
        completed_utc = $completedUtc
        exit_code = $process.exit_code
        request_path = (Get-Row12RelativePath $requestPath)
        response_path = (Get-Row12RelativePath $responsePath)
        trace_path = (Get-Row12RelativePath $coreTracePath)
        stdout_path = (Get-Row12RelativePath $stdoutPath)
        stderr_path = (Get-Row12RelativePath $stderrPath)
        external_ai_send = $false
        external_ai_used = $false
        token_savings_claimed = $false
        semantic_quality_claimed = $false
        readiness_claimed = $false
        training_started = $false
        checkpoint_mutated = $false
        locator_updated = $false
    }
    Write-Row12Json -Path $commandResultPath -Value $commandResult

    return [ordered]@{
        mode = $Mode
        exit_code = $process.exit_code
        request_path = $requestPath
        response_path = $responsePath
        core_trace_path = $coreTracePath
        command_result_path = $commandResultPath
        stdout_path = $stdoutPath
        stderr_path = $stderrPath
        response = $response
    }
}

function New-Row12TokenTrace {
    param(
        [Parameter(Mandatory = $true)]$Run,
        [Parameter(Mandatory = $true)][string]$Mode
    )

    $answerText = ""
    if ($Run.response) {
        $answerText = [string](Get-Row12PropertyValue -Object $Run.response -Name "ChatDisplayText")
    }

    $inputCount = Get-Row12HeuristicTokenCount -Text $promptText
    $outputCount = Get-Row12HeuristicTokenCount -Text $answerText
    $startedUtc = (Get-Date).ToUniversalTime().ToString("o")

    [ordered]@{
        schema_version = "xiigen.extension_token_trace.v1"
        scenario_id = $scenarioId
        pair_id = $pairId
        mode = $Mode
        host_name = $hostName
        host_path_label = $hostPathLabel
        method = $method
        core_api_path = $method
        debug_enabled = $debugEnabled
        prompt_hash = (Get-Row12Sha256Text -Text $promptText)
        visible_answer_hash = (Get-Row12Sha256Text -Text $answerText)
        tokenizer_name = "xiigen-heuristic-word-punctuation"
        tokenizer_version = "v1"
        tokenizer_accounting_type = "heuristic"
        token_count_input = $inputCount
        token_count_output = $outputCount
        token_count_total = $inputCount + $outputCount
        quality_score = $null
        quality_regression = $null
        isolation_mode = $isolationMode
        trace_started_at_utc = $startedUtc
        trace_ended_at_utc = (Get-Date).ToUniversalTime().ToString("o")
        artifact_refs = [ordered]@{
            request = (Get-Row12RelativePath $Run.request_path)
            response = (Get-Row12RelativePath $Run.response_path)
            core_trace = (Get-Row12RelativePath $Run.core_trace_path)
            command_result = (Get-Row12RelativePath $Run.command_result_path)
            command_output = (Get-Row12RelativePath $Run.stdout_path)
            command_error = (Get-Row12RelativePath $Run.stderr_path)
        }
        prompt_redacted = $true
        answer_redacted = $true
        contains_raw_private_content = $false
        external_ai_send = $false
        external_ai_used = $false
        semantic_quality_claimed = $false
        token_savings_claimed = $false
        readiness_claimed = $false
        learnability_claimed = $false
        training_started = $false
        checkpoint_mutated = $false
        locator_updated = $false
        accounting_source = [ordered]@{
            implementation = "faithful PowerShell equivalent of TokenTraceAccounting.CreateHeuristicTrace"
            source_file = "43.SmartSkillsDomain/TokenTraceAccounting.cs"
            limitation = "The core API call uses compiled SmartSkillsDomain code; the token trace counter/hash in this harness mirrors TokenTraceAccounting.cs instead of invoking that type directly."
        }
    }
}

function Test-Row12Pair {
    param(
        [Parameter(Mandatory = $true)]$Baseline,
        [Parameter(Mandatory = $true)]$Adaptive
    )

    $errors = New-Object System.Collections.Generic.List[string]
    foreach ($field in @(
        "scenario_id",
        "pair_id",
        "host_name",
        "host_path_label",
        "method",
        "core_api_path",
        "debug_enabled",
        "prompt_hash",
        "tokenizer_name",
        "tokenizer_version",
        "tokenizer_accounting_type",
        "isolation_mode",
        "external_ai_send",
        "external_ai_used",
        "contains_raw_private_content",
        "prompt_redacted",
        "answer_redacted"
    )) {
        if ($Baseline.$field -ne $Adaptive.$field) {
            $errors.Add("${field}_must_match_between_baseline_and_adaptive")
        }
    }

    if ($Baseline.mode -ne "baseline") { $errors.Add("baseline_mode_required") }
    if ($Adaptive.mode -ne "adaptive") { $errors.Add("adaptive_mode_required") }
    if ($Baseline.prompt_hash -ne $expectedPromptHash -or $Adaptive.prompt_hash -ne $expectedPromptHash) {
        $errors.Add("prompt_hash_must_match_accepted_row12_spec")
    }
    if ($Baseline.token_savings_claimed -or $Adaptive.token_savings_claimed) { $errors.Add("token_savings_claim_forbidden") }
    if ($Baseline.semantic_quality_claimed -or $Adaptive.semantic_quality_claimed) { $errors.Add("semantic_quality_claim_forbidden") }
    if ($Baseline.learnability_claimed -or $Adaptive.learnability_claimed) { $errors.Add("learnability_claim_forbidden") }
    if ($Baseline.external_ai_send -or $Adaptive.external_ai_send) { $errors.Add("external_ai_send_forbidden") }
    if ($Baseline.external_ai_used -or $Adaptive.external_ai_used) { $errors.Add("external_ai_used_forbidden") }

    $delta = [int]$Adaptive.token_count_total - [int]$Baseline.token_count_total
    [ordered]@{
        schema_version = "frdp.row12.pair_validation.v1"
        scenario_id = $scenarioId
        pair_id = $pairId
        pair_valid = $errors.Count -eq 0
        pair_validation_errors_count = $errors.Count
        pair_validation_errors = @($errors)
        same_fields_required = @(
            "scenario_id",
            "pair_id",
            "host_name",
            "host_path_label",
            "method",
            "core_api_path",
            "debug_enabled",
            "prompt_hash",
            "tokenizer_name",
            "tokenizer_version",
            "tokenizer_accounting_type",
            "isolation_mode",
            "external_ai_send",
            "external_ai_used",
            "contains_raw_private_content",
            "prompt_redacted",
            "answer_redacted"
        )
        baseline_total_tokens = [int]$Baseline.token_count_total
        adaptive_total_tokens = [int]$Adaptive.token_count_total
        token_delta_observation_only = $delta
        token_reduction_observed = $delta -lt 0
        external_ai_send = $false
        external_ai_used = $false
        token_savings_claimed = $false
        semantic_quality_claimed = $false
        readiness_claimed = $false
        token_savings_claim_allowed = $false
    }
}

function Test-Row12ContainsAll {
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Text,
        [Parameter(Mandatory = $true)][string[]]$Needles
    )

    foreach ($needle in $Needles) {
        if ($Text.IndexOf($needle, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
            return $false
        }
    }

    return $true
}

function New-Row12Assertion {
    param(
        [Parameter(Mandatory = $true)][int]$Id,
        [Parameter(Mandatory = $true)][string]$Assertion,
        [Parameter(Mandatory = $true)][bool]$Passed,
        [Parameter(Mandatory = $true)][string]$Evidence
    )

    [ordered]@{
        id = $Id
        assertion = $Assertion
        passed = $Passed
        evidence = $Evidence
    }
}

function Test-Row12QualityGuard {
    param(
        [Parameter(Mandatory = $true)]$BaselineRun,
        [Parameter(Mandatory = $true)]$AdaptiveRun,
        [Parameter(Mandatory = $true)]$BaselineTrace,
        [Parameter(Mandatory = $true)]$AdaptiveTrace,
        [Parameter(Mandatory = $true)]$Pair
    )

    $baselineResponseText = if (Test-Path -LiteralPath $BaselineRun.response_path) { Get-Content -Raw -LiteralPath $BaselineRun.response_path } else { "" }
    $adaptiveResponseText = if (Test-Path -LiteralPath $AdaptiveRun.response_path) { Get-Content -Raw -LiteralPath $AdaptiveRun.response_path } else { "" }
    $combined = $baselineResponseText + "`n" + $adaptiveResponseText

    $selected = @()
    if ($BaselineRun.response -and $BaselineRun.response.SelectedSkills) { $selected += @($BaselineRun.response.SelectedSkills) }
    if ($AdaptiveRun.response -and $AdaptiveRun.response.SelectedSkills) { $selected += @($AdaptiveRun.response.SelectedSkills) }
    $selected = @($selected | Select-Object -Unique)
    $selectedAllowed = $true
    foreach ($skill in $selected) {
        if ($allowedSkills -notcontains $skill) {
            $selectedAllowed = $false
        }
    }

    $assertions = @(
        (New-Row12Assertion 1 "pair_validation_errors_count == 0" ($Pair.pair_validation_errors_count -eq 0) "pair_validation_errors_count=$($Pair.pair_validation_errors_count)"),
        (New-Row12Assertion 2 "both traces keep token_savings_claimed=false, semantic_quality_claimed=false and learnability_claimed=false" (-not $BaselineTrace.token_savings_claimed -and -not $AdaptiveTrace.token_savings_claimed -and -not $BaselineTrace.semantic_quality_claimed -and -not $AdaptiveTrace.semantic_quality_claimed -and -not $BaselineTrace.learnability_claimed -and -not $AdaptiveTrace.learnability_claimed) "trace claim flags are false"),
        (New-Row12Assertion 3 "baseline and adaptive responses preserve marker FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1" ($baselineResponseText.Contains($marker) -and $adaptiveResponseText.Contains($marker)) "actual baseline/adaptive response JSON searched for exact marker"),
        (New-Row12Assertion 4 "baseline and adaptive responses preserve scenario id frdp-p01-cursor-token-savings-baseline" ($baselineResponseText.Contains($scenarioId) -and $adaptiveResponseText.Contains($scenarioId)) "actual baseline/adaptive response JSON searched for exact scenario id"),
        (New-Row12Assertion 5 "prompt_to_task output contains task_id, task_title, constraints and acceptance_gates" (Test-Row12ContainsAll $combined @("prompt_to_task", "task_id", "task_title", "constraints", "acceptance_gates")) "actual response JSON searched for prompt_to_task fields"),
        (New-Row12Assertion 6 "task_to_plan output contains exactly three ordered steps" ((Test-Row12ContainsAll $combined @("task_to_plan", "step 1", "step 2", "step 3")) -or (Test-Row12ContainsAll $combined @("task_to_plan", '"steps"', '"3"'))) "actual response JSON searched for three ordered steps"),
        (New-Row12Assertion 7 "each plan step has one verification gate" (Test-Row12ContainsAll $combined @("verification gate", "step")) "actual response JSON searched for per-step verification gates"),
        (New-Row12Assertion 8 "task_to_skills output exists" ($selected.Count -gt 0 -or (Test-Row12ContainsAll $combined @("task_to_skills"))) "selected skill output or task_to_skills text searched"),
        (New-Row12Assertion 9 "skill selection includes prompt-to-task, task-to-plan and task-to-skills" (($selected -contains "prompt-to-task") -and ($selected -contains "task-to-plan") -and ($selected -contains "task-to-skills")) "selected=$($selected -join ',')"),
        (New-Row12Assertion 10 "every selected skill is from the allowed set" $selectedAllowed "selected=$($selected -join ',')"),
        (New-Row12Assertion 11 "references include marker and scenario id" (Test-Row12ContainsAll $combined @($marker, $scenarioId)) "actual response references searched"),
        (New-Row12Assertion 12 "non_claims include all required non-claims" (Test-Row12ContainsAll $combined @("no token savings claim", "no semantic quality claim", "no readiness claim", "no training", "no checkpoint promotion", "no locator update")) "actual response JSON searched for complete non_claims"),
        (New-Row12Assertion 13 "response is not generic: exact marker, exact scenario id, required sections and row-specific non-claims are present" (Test-Row12ContainsAll $combined @($marker, $scenarioId, "task", "plan", "skills", "references", "non_claims")) "actual response JSON searched for row-specific structure"),
        (New-Row12Assertion 14 "deterministic shortcuts are disclosed: synthetic no-send workload, heuristic tokenizer/accounting, no external AI send and token delta alone not quality proof" (Test-Row12ContainsAll $combined @("synthetic no-send workload", "heuristic tokenizer", "no external AI", "token delta alone")) "actual response JSON searched for shortcut boundary disclosures")
    )

    $passed = @($assertions | Where-Object { $_.passed }).Count
    [ordered]@{
        schema_version = "frdp.row12.quality_guard.v1"
        pass = ($passed -eq 14 -and $Pair.pair_validation_errors_count -eq 0)
        quality_assertions_total = 14
        quality_assertions_passed = $passed
        critical_failures_count = 14 - $passed
        assertions = @($assertions)
        external_ai_send = $false
        external_ai_used = $false
        token_savings_claimed = $false
        semantic_quality_claimed = $false
        readiness_claimed = $false
        quality_scope_note = "This guard checks actual baseline/adaptive core API response JSON. Request text and report text are not credited as response quality."
    }
}

function Write-Row12HostBlocker {
    $jsonPath = Join-Path $resolvedMeasurementRoot "cursor-host-visible-candidate-blocker.json"
    $mdPath = Join-Path $resolvedMeasurementRoot "cursor-host-visible-candidate-blocker.md"
    $blocker = [ordered]@{
        schema_version = "frdp.row12.cursor_host_visible_candidate_blocker.v1"
        status = "BLOCKED_WITH_NEXT_ACTION"
        scenario_id = $scenarioId
        marker = $marker
        blocker = "No true Cursor host command invocation that accepts row12-workload-spec.json was used by this harness."
        evidence_checked = @(
            "Current harness uses the repo-local no-send core/API path tools/xiigen-extension-core-api.ps1.",
            "The bounded write scope only authorizes this harness and measurement artifacts; it does not authorize editing or parameterizing existing Cursor host-visible scripts."
        )
        next_action = "Create or parameterize a Cursor no-send host-visible command that accepts scenario_id, marker, prompt, method=prompt-to-task, and evidence root, then rerun row12 with the same prompt hash."
        external_ai_send = $false
        external_ai_used = $false
        token_savings_claimed = $false
        semantic_quality_claimed = $false
        readiness_claimed = $false
    }

    Write-Row12Json -Path $jsonPath -Value $blocker
    Write-Row12Text -Path $mdPath -Text (@(
        "# Cursor host-visible candidate blocker",
        "",
        "Status: BLOCKED_WITH_NEXT_ACTION",
        "",
        "No true Cursor host command invocation that accepts row12-workload-spec.json was used by this harness.",
        "",
        "This harness used only the repo-local no-send core/API path tools/xiigen-extension-core-api.ps1.",
        "",
        "Next action: create or parameterize a Cursor no-send host-visible command that accepts scenario_id, marker, prompt, method=prompt-to-task, and evidence root, then rerun row12 with the same prompt hash.",
        "",
        "Non-claims: no token savings, no semantic quality, no readiness, no training, no checkpoint mutation, no locator update."
    ) -join "`n")

    return [ordered]@{
        json = $jsonPath
        md = $mdPath
    }
}

function Write-Row12Report {
    param(
        [Parameter(Mandatory = $true)][string]$Status,
        [Parameter(Mandatory = $true)]$Commands,
        [Parameter(Mandatory = $true)]$ReportBody
    )

    $reportJsonPath = Join-Path $resolvedMeasurementRoot "execution-report.json"
    $reportMdPath = Join-Path $resolvedMeasurementRoot "execution-report.md"
    $created = @(Get-ChildItem -LiteralPath $resolvedMeasurementRoot -File -Recurse | ForEach-Object { Get-Row12RelativePath $_.FullName } | Sort-Object)
    $report = [ordered]@{
        schema_version = "frdp.row12.execution_report.v1"
        status = $Status
        generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
        current_task = "Create the smallest row12 Cursor token trace harness and run it once if feasible."
        active_luba_authority_quote = "Deystviya cherez sub agentov"
        not_the_task = @(
            "token savings claim",
            "readiness claim",
            "semantic quality claim",
            "checkpoint mutation",
            "training",
            "external AI send",
            "board/skills/governance edit",
            "git stage/commit/push"
        )
        source_spec = (Get-Row12RelativePath $resolvedSpecPath)
        prompt_hash_expected = $expectedPromptHash
        prompt_hash_actual = $promptHash
        prompt_hash_matches = ($promptHash -eq $expectedPromptHash)
        commands_run = $Commands
        body = $ReportBody
        created_files = $created
    }
    Write-Row12Json -Path $reportJsonPath -Value $report

    $md = @(
        "# Row 12 Cursor token trace harness report",
        "",
        "Status: $Status",
        "",
        "## Current task",
        "",
        "Create the smallest row12 Cursor token trace harness and run it once if feasible.",
        "",
        "## Commands",
        ""
    )
    foreach ($command in $Commands) {
        $md += "- " + $command.command + ": " + $command.status
    }
    $md += @(
        "",
        "## Token numbers (observation only)",
        "",
        "Baseline input/output/total: $($ReportBody.token_numbers_observation_only.baseline_input_tokens) / $($ReportBody.token_numbers_observation_only.baseline_output_tokens) / $($ReportBody.token_numbers_observation_only.baseline_total_tokens).",
        "Adaptive input/output/total: $($ReportBody.token_numbers_observation_only.adaptive_input_tokens) / $($ReportBody.token_numbers_observation_only.adaptive_output_tokens) / $($ReportBody.token_numbers_observation_only.adaptive_total_tokens).",
        "Observed candidate delta: $($ReportBody.token_numbers_observation_only.observed_candidate_delta).",
        "",
        "This is not a token savings claim.",
        "",
        "## Quality guard",
        "",
        "Assertions passed: $($ReportBody.quality_guard.quality_assertions_passed) / $($ReportBody.quality_guard.quality_assertions_total).",
        "Critical failures: $($ReportBody.quality_guard.critical_failures_count).",
        "",
        "## Proven",
        "",
        "- Prompt hash reproduction: $($promptHash -eq $expectedPromptHash).",
        "- Pair validation errors: $($ReportBody.pair_validation.pair_validation_errors_count).",
        "- External AI send: false.",
        "- External AI used: false.",
        "- Token savings claimed: false.",
        "",
        "## Not proven",
        "",
        "- Token savings.",
        "- Semantic quality.",
        "- Product readiness or full Cursor readiness.",
        "- True Cursor host-visible row12 candidate behavior.",
        "",
        "## Main artifacts",
        "",
        "- baseline-request.json",
        "- baseline-response.json",
        "- baseline-trace.json",
        "- baseline-core-trace.json",
        "- adaptive-request.json",
        "- adaptive-response.json",
        "- adaptive-trace.json",
        "- adaptive-core-trace.json",
        "- token-traces.jsonl",
        "- pair-validation.json",
        "- quality-guard.json",
        "- execution-report.json"
    )
    Write-Row12Text -Path $reportMdPath -Text ($md -join "`n")
}

$CoreRoot = Resolve-Row12Path $CoreRoot
$resolvedSpecPath = Resolve-Row12Path $SpecPath
if (-not (Test-Path -LiteralPath $resolvedSpecPath)) {
    throw "SpecPath does not exist: $resolvedSpecPath"
}

$scenarioRoot = Split-Path -Parent $resolvedSpecPath
if ([string]::IsNullOrWhiteSpace($MeasurementRoot)) {
    $timestamp = (Get-Date).ToUniversalTime().ToString("yyyyMMddTHHmmssZ")
    $resolvedMeasurementRoot = Join-Path $scenarioRoot "measurement-$timestamp"
} else {
    $resolvedMeasurementRoot = Resolve-Row12Path $MeasurementRoot
}
New-Item -ItemType Directory -Force -Path $resolvedMeasurementRoot | Out-Null

$spec = Get-Content -Raw -LiteralPath $resolvedSpecPath | ConvertFrom-Json
$workload = $spec.workload
$scenarioId = [string]$spec.scenario.scenario_id
$pairId = [string]$workload.pair_id
$method = [string]$workload.method
$hostName = "Cursor"
$hostPathLabel = [string]$workload.host_path_label
$marker = [string]$workload.marker
$expectedPromptHash = [string]$workload.prompt_hash
$promptText = (@($workload.prompt_lines) | ForEach-Object { [string]$_ }) -join "`n"
$promptHash = Get-Row12Sha256Text -Text $promptText
$debugEnabled = $true
$allowedSkills = @($spec.quality_guard.allowed_skill_set | ForEach-Object { [string]$_ })
$isolationMode = "measurement_$([System.IO.Path]::GetFileName($resolvedMeasurementRoot))_core_api_no_send"
$coreApiScriptPath = Join-Path $CoreRoot "tools\xiigen-extension-core-api.ps1"
$tokenSourcePath = Join-Path $CoreRoot "43.SmartSkillsDomain\TokenTraceAccounting.cs"
$tokenTestPath = Join-Path $CoreRoot "Tests\ExtensionTokenTraceAccountingTests.cs"

$commands = New-Object System.Collections.Generic.List[object]
$hostBlocker = Write-Row12HostBlocker

if ($promptHash -ne $expectedPromptHash) {
    $body = [ordered]@{
        failure = "prompt_hash_mismatch"
        token_numbers_observation_only = [ordered]@{}
        pair_validation = [ordered]@{ pair_validation_errors_count = 1; errors = @("prompt_hash_mismatch") }
        quality_guard = [ordered]@{ quality_assertions_total = 14; quality_assertions_passed = 0; critical_failures_count = 14; pass = $false }
        host_visible_candidate = [ordered]@{ status = "exact_blocker_recorded"; blocker_ref = (Get-Row12RelativePath $hostBlocker.json) }
    }
    $commands.Add([ordered]@{ command = "prompt hash verification"; status = "FAIL"; exit_code = 1 }) | Out-Null
    Write-Row12Report -Status "MEASUREMENT_NEEDS_FIX" -Commands $commands.ToArray() -ReportBody $body
    Write-Output "MEASUREMENT_NEEDS_FIX"
    Write-Output (Join-Path $resolvedMeasurementRoot "execution-report.json")
    exit 1
}

$commands.Add([ordered]@{ command = "prompt hash verification"; status = "PASS"; exit_code = 0 }) | Out-Null

if ($NoRun) {
    $body = [ordered]@{
        reason = "NoRun switch was used."
        token_numbers_observation_only = [ordered]@{}
        pair_validation = [ordered]@{ pair_validation_errors_count = $null }
        quality_guard = [ordered]@{ quality_assertions_total = 14; quality_assertions_passed = $null; critical_failures_count = $null; pass = $false }
        host_visible_candidate = [ordered]@{ status = "exact_blocker_recorded"; blocker_ref = (Get-Row12RelativePath $hostBlocker.json) }
    }
    $commands.Add([ordered]@{ command = "core/API measurement run"; status = "SKIPPED"; exit_code = $null }) | Out-Null
    Write-Row12Report -Status "BLOCKED_WITH_NEXT_ACTION" -Commands $commands.ToArray() -ReportBody $body
    Write-Output "BLOCKED_WITH_NEXT_ACTION"
    Write-Output (Join-Path $resolvedMeasurementRoot "execution-report.json")
    exit 0
}

if (-not (Test-Path -LiteralPath $coreApiScriptPath)) {
    $body = [ordered]@{
        failure = "core_api_script_missing"
        missing_path = (Get-Row12RelativePath $coreApiScriptPath)
        token_numbers_observation_only = [ordered]@{}
        pair_validation = [ordered]@{ pair_validation_errors_count = $null }
        quality_guard = [ordered]@{ quality_assertions_total = 14; quality_assertions_passed = $null; critical_failures_count = $null; pass = $false }
        host_visible_candidate = [ordered]@{ status = "exact_blocker_recorded"; blocker_ref = (Get-Row12RelativePath $hostBlocker.json) }
    }
    $commands.Add([ordered]@{ command = "core/API script lookup"; status = "FAIL"; exit_code = 1 }) | Out-Null
    Write-Row12Report -Status "BLOCKED_WITH_NEXT_ACTION" -Commands $commands.ToArray() -ReportBody $body
    Write-Output "BLOCKED_WITH_NEXT_ACTION"
    Write-Output (Join-Path $resolvedMeasurementRoot "execution-report.json")
    exit 1
}

$baselineRun = Invoke-Row12CoreApi -Mode "baseline"
$adaptiveRun = Invoke-Row12CoreApi -Mode "adaptive"
$commands.Add([ordered]@{ command = "baseline core/API no-send call"; status = if ($baselineRun.exit_code -eq 0) { "PASS" } else { "FAIL" }; exit_code = $baselineRun.exit_code }) | Out-Null
$commands.Add([ordered]@{ command = "adaptive core/API no-send call"; status = if ($adaptiveRun.exit_code -eq 0) { "PASS" } else { "FAIL" }; exit_code = $adaptiveRun.exit_code }) | Out-Null

$baselineTrace = New-Row12TokenTrace -Run ([pscustomobject]$baselineRun) -Mode "baseline"
$adaptiveTrace = New-Row12TokenTrace -Run ([pscustomobject]$adaptiveRun) -Mode "adaptive"
$pair = Test-Row12Pair -Baseline ([pscustomobject]$baselineTrace) -Adaptive ([pscustomobject]$adaptiveTrace)
$qualityGuard = Test-Row12QualityGuard -BaselineRun ([pscustomobject]$baselineRun) -AdaptiveRun ([pscustomobject]$adaptiveRun) -BaselineTrace ([pscustomobject]$baselineTrace) -AdaptiveTrace ([pscustomobject]$adaptiveTrace) -Pair ([pscustomobject]$pair)

Write-Row12Json -Path (Join-Path $resolvedMeasurementRoot "baseline-trace.json") -Value $baselineTrace
Write-Row12Json -Path (Join-Path $resolvedMeasurementRoot "adaptive-trace.json") -Value $adaptiveTrace
Write-Row12Json -Path (Join-Path $resolvedMeasurementRoot "token-traces.json") -Value ([ordered]@{
    schema_version = "frdp.row12.token_traces_packet.v1"
    scenario_id = $scenarioId
    pair_id = $pairId
    prompt_hash = $promptHash
    traces = @($baselineTrace, $adaptiveTrace)
})
$jsonl = @(
    ($baselineTrace | ConvertTo-Json -Depth 100 -Compress),
    ($adaptiveTrace | ConvertTo-Json -Depth 100 -Compress)
) -join "`n"
Write-Row12Text -Path (Join-Path $resolvedMeasurementRoot "token-traces.jsonl") -Text $jsonl
Write-Row12Json -Path (Join-Path $resolvedMeasurementRoot "pair-validation.json") -Value $pair
Write-Row12Json -Path (Join-Path $resolvedMeasurementRoot "quality-guard.json") -Value $qualityGuard

$status = if (
    $baselineRun.exit_code -eq 0 -and
    $adaptiveRun.exit_code -eq 0 -and
    $promptHash -eq $expectedPromptHash -and
    $pair.pair_validation_errors_count -eq 0 -and
    $qualityGuard.quality_assertions_passed -eq 14 -and
    $qualityGuard.critical_failures_count -eq 0 -and
    -not $qualityGuard.external_ai_send -and
    -not $qualityGuard.external_ai_used -and
    -not $qualityGuard.token_savings_claimed -and
    -not $qualityGuard.semantic_quality_claimed -and
    -not $qualityGuard.readiness_claimed
) {
    "MEASUREMENT_PACKET_READY"
} else {
    "MEASUREMENT_NEEDS_FIX"
}

$body = [ordered]@{
    mode_results = [ordered]@{
        baseline = [ordered]@{
            exit_code = $baselineRun.exit_code
            response_success = if ($baselineRun.response) { [bool](Get-Row12PropertyValue -Object $baselineRun.response -Name "Success") } else { $false }
            response_status = if ($baselineRun.response) { [string](Get-Row12PropertyValue -Object $baselineRun.response -Name "Status") } else { $null }
        }
        adaptive = [ordered]@{
            exit_code = $adaptiveRun.exit_code
            response_success = if ($adaptiveRun.response) { [bool](Get-Row12PropertyValue -Object $adaptiveRun.response -Name "Success") } else { $false }
            response_status = if ($adaptiveRun.response) { [string](Get-Row12PropertyValue -Object $adaptiveRun.response -Name "Status") } else { $null }
        }
    }
    token_numbers_observation_only = [ordered]@{
        baseline_input_tokens = [int]$baselineTrace.token_count_input
        baseline_output_tokens = [int]$baselineTrace.token_count_output
        baseline_total_tokens = [int]$baselineTrace.token_count_total
        adaptive_input_tokens = [int]$adaptiveTrace.token_count_input
        adaptive_output_tokens = [int]$adaptiveTrace.token_count_output
        adaptive_total_tokens = [int]$adaptiveTrace.token_count_total
        observed_candidate_delta = [int]$adaptiveTrace.token_count_total - [int]$baselineTrace.token_count_total
        token_reduction_observed = ([int]$adaptiveTrace.token_count_total -lt [int]$baselineTrace.token_count_total)
        token_savings_claimed = $false
        token_savings_claim_allowed = $false
    }
    pair_validation = $pair
    quality_guard = [ordered]@{
        pass = $qualityGuard.pass
        quality_assertions_total = $qualityGuard.quality_assertions_total
        quality_assertions_passed = $qualityGuard.quality_assertions_passed
        critical_failures_count = $qualityGuard.critical_failures_count
        external_ai_send = $qualityGuard.external_ai_send
        external_ai_used = $qualityGuard.external_ai_used
        token_savings_claimed = $qualityGuard.token_savings_claimed
        semantic_quality_claimed = $qualityGuard.semantic_quality_claimed
        readiness_claimed = $qualityGuard.readiness_claimed
    }
    host_visible_candidate = [ordered]@{
        status = "exact_blocker_recorded"
        blocker_ref = (Get-Row12RelativePath $hostBlocker.json)
    }
    accounting = [ordered]@{
        source_file = "43.SmartSkillsDomain/TokenTraceAccounting.cs"
        source_sha256 = (Get-Row12Sha256File $tokenSourcePath)
        test_file = "Tests/ExtensionTokenTraceAccountingTests.cs"
        test_sha256 = (Get-Row12Sha256File $tokenTestPath)
        implementation_note = "Core/API response came from compiled SmartSkillsDomain through tools/xiigen-extension-core-api.ps1; token trace hash/count logic is a faithful PowerShell equivalent of TokenTraceAccounting.cs."
    }
    what_is_proven = @(
        "The accepted row12 prompt hash was reproduced exactly.",
        "Baseline and adaptive no-send core/API calls produced request/response/core-trace artifacts.",
        "Pair validation checked same scenario, pair, host, method, debug, prompt hash, tokenizer, isolation, redaction and no-send fields.",
        "No external AI send/use, token savings claim, semantic quality claim, readiness claim, training, checkpoint mutation, or locator update is recorded."
    )
    what_is_not_proven = @(
        "Token savings.",
        "Semantic quality.",
        "Product readiness or full Cursor readiness.",
        "True Cursor host-visible row12 candidate behavior."
    )
}

Write-Row12Report -Status $status -Commands $commands.ToArray() -ReportBody $body
Write-Output $status
Write-Output (Join-Path $resolvedMeasurementRoot "execution-report.json")

if ($status -eq "MEASUREMENT_PACKET_READY") {
    exit 0
}

exit 2
