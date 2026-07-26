param(
    [string]$CoreRoot = (Split-Path -Parent $PSScriptRoot),
    [string]$OutputRoot = (Join-Path (Split-Path -Parent $PSScriptRoot) 'artifacts\extension-runtime-token-quality-learnability\phase-12-cross-host-token-quality-comparison'),
    [string]$PluginRoot = (Join-Path $env:USERPROFILE '.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500'),
    [string]$SourceRunDir = "",
    [string]$RunId = ""
)

$ErrorActionPreference = "Stop"

$StartedUtc = (Get-Date).ToUniversalTime()
$RunStamp = $StartedUtc.ToString("yyyyMMddTHHmmssfffffffZ")
if ([string]::IsNullOrWhiteSpace($RunId)) {
    $RunId = "phase12-cross-host-token-quality-$RunStamp"
}

$RunnerPath = $MyInvocation.MyCommand.Path
$RunDir = Join-Path $OutputRoot $RunId
$PluginEntrypointPath = Join-Path $PluginRoot "src\codex-entrypoint.js"
$Phase09Root = Join-Path $CoreRoot "artifacts\extension-runtime-token-quality-learnability\phase-09-claude-full-extension-live-proof"
$Phase10Root = Join-Path $CoreRoot "artifacts\extension-runtime-token-quality-learnability\phase-10-cursor-full-extension-live-proof"
$Phase11Root = Join-Path $CoreRoot "artifacts\extension-runtime-token-quality-learnability\phase-11-codex-full-extension-live-proof"
$TokenEstimator = "xiigen-phase12-heuristic-word-punctuation-v1"
$BaselineMode = "baseline_common_host_manual_packet"
$AdaptiveMode = "adaptive_xiigen_assisted_adapter_core_trace"
$AblationMode = "ablation_skill_context_neutralized_adapter_core_trace"

function New-Phase12Directory {
    param([Parameter(Mandatory = $true)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        New-Item -ItemType Directory -Force -Path $Path | Out-Null
    }
}

function Write-Phase12Text {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Text
    )

    New-Phase12Directory -Path (Split-Path -Parent $Path)
    $normalized = $Text.Replace("`r`n", "`n").Replace("`r", "`n")
    $normalized = [regex]::Replace($normalized, "(?m)[ `t]+$", "")
    if (-not $normalized.EndsWith("`n")) {
        $normalized += "`n"
    }

    [System.IO.File]::WriteAllText($Path, $normalized, [System.Text.UTF8Encoding]::new($false))
}

function ConvertTo-Phase12Json {
    param([Parameter(Mandatory = $true)]$Value)

    return $Value | ConvertTo-Json -Depth 100
}

function ConvertTo-Phase12CompactJson {
    param([Parameter(Mandatory = $true)]$Value)

    return $Value | ConvertTo-Json -Depth 100 -Compress
}

function ConvertFrom-Phase12Escaped {
    param([Parameter(Mandatory = $true)][string]$JsonString)

    return $JsonString | ConvertFrom-Json
}

function Write-Phase12Json {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)]$Value
    )

    Write-Phase12Text -Path $Path -Text (ConvertTo-Phase12Json $Value)
}

function Get-Phase12Sha256 {
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

function Get-Phase12TokenCount {
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

function Get-Phase12Property {
    param(
        $Object,
        [Parameter(Mandatory = $true)][string]$Name,
        $Default = $null
    )

    if ($null -eq $Object) {
        return $Default
    }

    $property = $Object.PSObject.Properties | Where-Object { $_.Name -eq $Name } | Select-Object -First 1
    if ($property) {
        return $property.Value
    }

    return $Default
}

function Get-Phase12CheckValue {
    param(
        $Response,
        [Parameter(Mandatory = $true)][string]$Name,
        [string]$Default = ""
    )

    $checkSet = Get-Phase12Property -Object $Response -Name "CheckSetResults"
    $value = Get-Phase12Property -Object $checkSet -Name $Name -Default $Default
    if ($null -eq $value) {
        return $Default
    }

    return [string]$value
}

function Get-Phase12LatestEvidenceFile {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$Pattern
    )

    if (-not (Test-Path -LiteralPath $Root)) {
        return "NOT_FOUND: $Root"
    }

    $file = Get-ChildItem -LiteralPath $Root -Recurse -File -Filter $Pattern |
        Sort-Object LastWriteTimeUtc -Descending |
        Select-Object -First 1
    if ($file) {
        return $file.FullName
    }

    return "NOT_FOUND: $Pattern under $Root"
}

function Get-Phase12RelativePath {
    param([Parameter(Mandatory = $true)][string]$Path)

    $fullPath = [System.IO.Path]::GetFullPath($Path)
    $fullRoot = [System.IO.Path]::GetFullPath($CoreRoot).TrimEnd("\") + "\"
    if ($fullPath.StartsWith($fullRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        return $fullPath.Substring($fullRoot.Length).Replace("\", "/")
    }

    return $fullPath
}

function New-Phase12Scenarios {
    @(
        [pscustomobject][ordered]@{
            scenario_id = "phase12-heldout-prompt-to-task-01"
            exercise = "prompt -> task"
            method = "prompt-to-task"
            prompt_text = "Convert this messy request into an executable task: a support team wants an intake assistant that classifies incoming customer messages, asks one clarifying question only when required, and writes an auditable next-step record."
            requested_skills = @("plan-review", "authority-governance", "documentation-sync")
            expected_quality_markers = @("task", "clarifying question", "auditable", "next-step")
            held_out_basis = "Hand-authored Phase 12 universal workflow prompt; no project, repo, model path, host path, pair id, or source id embedded."
        }
        [pscustomobject][ordered]@{
            scenario_id = "phase12-heldout-orchestration-plan-01"
            exercise = "task -> orchestration plan"
            method = "orchestration-plan"
            prompt_text = "Build a vendor-neutral orchestration plan for replacing a recurring manual approval workflow with a tool-assisted process that has review gates, rollback, evidence capture, and clear stop conditions."
            requested_skills = @("plan-review", "quality-review", "runtime-QA", "rollback-safety")
            expected_quality_markers = @("review gates", "rollback", "evidence", "stop conditions")
            held_out_basis = "Hand-authored Phase 12 universal orchestration prompt; no project-stack-specific trick or training-source pointer embedded."
        }
        [pscustomobject][ordered]@{
            scenario_id = "phase12-heldout-skill-selection-01"
            exercise = "task/plan -> skills"
            method = "skill-selection"
            prompt_text = "Given a documentation QA task and a short plan, select reusable skills for checking requirement coverage, running safe verification, detecting overclaims, and returning a review packet without changing the product."
            requested_skills = @("documentation-sync", "quality-review", "test-integrity", "shortcut-audit")
            expected_quality_markers = @("requirement coverage", "safe verification", "overclaim", "review packet")
            held_out_basis = "Hand-authored Phase 12 universal skill-selection prompt; no candidate position, pair id, path, or source id embedded."
        }
    )
}

function New-Phase12Hosts {
    @(
        [pscustomobject][ordered]@{
            host_id = "claude"
            host_surface_label = "Claude"
            prior_evidence_path = Get-Phase12LatestEvidenceFile -Root $Phase09Root -Pattern "phase-09-claude-proof.json"
            phase12_live_host_automation = $false
            adaptive_run_source = "prior_host_proof_plus_codex_plugin_adapter_core_trace; no live Claude chat UI in Phase 12"
        }
        [pscustomobject][ordered]@{
            host_id = "cursor"
            host_surface_label = "Cursor"
            prior_evidence_path = Get-Phase12LatestEvidenceFile -Root $Phase10Root -Pattern "phase10-runner-summary.json"
            phase12_live_host_automation = $false
            adaptive_run_source = "prior_host_proof_plus_codex_plugin_adapter_core_trace; no live Cursor chat UI in Phase 12"
        }
        [pscustomobject][ordered]@{
            host_id = "codex"
            host_surface_label = "Codex"
            prior_evidence_path = Get-Phase12LatestEvidenceFile -Root $Phase11Root -Pattern "phase11-codex-full-extension-live-proof-report.json"
            phase12_live_host_automation = $false
            adaptive_run_source = "installed_codex_plugin_adapter_core_trace; no full Codex chat UI automation in Phase 12"
        }
    )
}

function New-Phase12AvailableSkills {
    @(
        "authority-governance",
        "documentation-sync",
        "held-out-ablation",
        "plan-review",
        "quality-review",
        "rollback-safety",
        "runtime-QA",
        "shortcut-audit",
        "skill-selection-invariance",
        "test-integrity",
        "token-measurement"
    )
}

function New-Phase12ManualBaselineRequest {
    param(
        [Parameter(Mandatory = $true)]$Scenario,
        [Parameter(Mandatory = $true)]$HostSurface
    )

    $manualContext = @"
Universal manual host packet for a common baseline without XIIGen assistance.
The host receives the user request, a generic process checklist, and a generic skill catalogue in plain prompt context.
Checklist: restate the task, identify deliverables, outline review gates, preserve stop conditions, note evidence requirements, avoid project-specific assumptions, avoid hidden deterministic shortcut claims, and avoid claims about training or semantic quality.
Generic skill catalogue: planning, quality review, documentation review, safe verification, overclaim detection, rollback planning, evidence packet drafting, and token measurement.
Limitations: this is a transparent synthetic baseline packet for same-scenario accounting, not a live Claude/Cursor/Codex chat transcript and not proof of actual model quality.
"@

    [ordered]@{
        schema_version = "xiigen.phase12.baseline_request.v1"
        run_id = $RunId
        mode = $BaselineMode
        host_surface_label = $HostSurface.host_surface_label
        scenario_id = $Scenario.scenario_id
        method = $Scenario.method
        user_prompt = $Scenario.prompt_text
        manual_context_packet = $manualContext
        requested_output = "Return a compact task/plan/skill response with explicit non-claims."
        source_label = "transparent_synthetic_common_host_baseline_no_live_host_ui_no_external_ai"
    }
}

function New-Phase12ManualBaselineResponse {
    param(
        [Parameter(Mandatory = $true)]$Scenario,
        [Parameter(Mandatory = $true)]$HostSurface
    )

    $text = "Baseline manual packet response for $($HostSurface.host_surface_label): extracted a generic $($Scenario.exercise) task, noted evidence and review gates, and kept all quality, token-saving, training, and readiness claims closed."
    [ordered]@{
        schema_version = "xiigen.phase12.baseline_response.v1"
        run_id = $RunId
        mode = $BaselineMode
        host_surface_label = $HostSurface.host_surface_label
        scenario_id = $Scenario.scenario_id
        method = $Scenario.method
        response_text = $text
        selected_skills = @()
        limitations = @(
            "Synthetic baseline packet; not a live host response.",
            "No XIIGen core, local adaptive model, external AI, training, or checkpoint mutation."
        )
        no_overclaim = [ordered]@{
            semantic_quality_claimed = $false
            token_savings_claimed = $false
            learnability_claimed = $false
            training_started = $false
            checkpoint_mutated = $false
            full_host_ui_claimed = $false
        }
    }
}

function New-Phase12Trace {
    param(
        [Parameter(Mandatory = $true)]$Scenario,
        [Parameter(Mandatory = $true)]$HostSurface,
        [Parameter(Mandatory = $true)][string]$Mode,
        [Parameter(Mandatory = $true)][string]$RunSource,
        [Parameter(Mandatory = $true)][string]$RequestPath,
        [Parameter(Mandatory = $true)][string]$ResponsePath,
        [Parameter(Mandatory = $true)][string]$TracePath,
        [Parameter(Mandatory = $true)][string]$InputText,
        [Parameter(Mandatory = $true)][string]$OutputText,
        [int]$ExitCode = 0,
        [bool]$Succeeded = $true
    )

    $inputTokens = Get-Phase12TokenCount -Text $InputText
    $outputTokens = Get-Phase12TokenCount -Text $OutputText
    [ordered]@{
        schema_version = "xiigen.phase12.token_trace.v1"
        run_id = $RunId
        scenario_id = $Scenario.scenario_id
        exercise = $Scenario.exercise
        method = $Scenario.method
        host_surface_label = $HostSurface.host_surface_label
        pair_key = "$($Scenario.scenario_id)|$($HostSurface.host_id)"
        mode = $Mode
        run_source = $RunSource
        phase12_live_host_automation = $false
        prior_host_evidence_path = $HostSurface.prior_evidence_path
        request_path = Get-Phase12RelativePath -Path $RequestPath
        response_path = Get-Phase12RelativePath -Path $ResponsePath
        raw_trace_path = Get-Phase12RelativePath -Path $TracePath
        exit_code = $ExitCode
        succeeded = $Succeeded
        token_estimate_method = $TokenEstimator
        input_token_estimate = $inputTokens
        output_token_estimate = $outputTokens
        total_token_estimate = $inputTokens + $outputTokens
        input_sha256 = Get-Phase12Sha256 -Text $InputText
        output_sha256 = Get-Phase12Sha256 -Text $OutputText
        limitations = @(
            "Heuristic token estimate, not provider tokenizer output.",
            "Host surface label is compared on the same scenario; Phase 12 does not automate live chat UI."
        )
        no_external_ai = $true
        training_started = $false
        checkpoint_mutated = $false
    }
}

function Invoke-Phase12AdapterCore {
    param(
        [Parameter(Mandatory = $true)]$Scenario,
        [Parameter(Mandatory = $true)]$HostSurface,
        [Parameter(Mandatory = $true)][string]$Mode,
        [Parameter(Mandatory = $true)][string]$PairDir,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][string[]]$RequestedSkills,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][string[]]$AvailableSkills
    )

    $requestPath = Join-Path $PairDir "$Mode-request.json"
    $responsePath = Join-Path $PairDir "$Mode-response.json"
    $tracePath = Join-Path $PairDir "$Mode-core-trace.json"
    $stdoutPath = Join-Path $PairDir "$Mode-stdout.txt"
    $stderrPath = Join-Path $PairDir "$Mode-stderr.txt"
    $correlationId = "phase12-" + [Guid]::NewGuid().ToString("N")
    $hostFacts = [ordered]@{
        phase = "12"
        host_surface_label = $HostSurface.host_surface_label
        phase12_live_host_automation = "false"
        comparison_mode = $Mode
        prior_host_proof_available = "true"
        hidden_deterministic_shortcut_allowed = "false"
    }
    $checkSetInputs = [ordered]@{
        semantic_quality_claimed = "false"
        runtime_extension_claimed = "false"
        phase12_quality_scoring_external_to_core = "true"
        training_started = "false"
        checkpoint_mutation_started = "false"
        skill_context_signals_present = if ($Mode -eq $AblationMode) { "false" } else { "true" }
    }
    $provenance = [ordered]@{
        phase = "12"
        source = "phase12-cross-host-token-quality-runner"
        host_surface_label = $HostSurface.host_surface_label
        runtime_authority = "current_work_order_only"
        pair_id_runtime_authority = "false"
        source_path_runtime_authority = "false"
    }

    New-Phase12Directory -Path $PairDir
    $nodeArgs = @(
        $PluginEntrypointPath,
        "--method", $Scenario.method,
        "--user-prompt-or-task-text", $Scenario.prompt_text,
        "--host-facts-json", (ConvertTo-Phase12CompactJson $hostFacts),
        "--requested-skills-json", (ConvertTo-Phase12CompactJson $RequestedSkills),
        "--available-skills-json", (ConvertTo-Phase12CompactJson $AvailableSkills),
        "--check-set-inputs-json", (ConvertTo-Phase12CompactJson $checkSetInputs),
        "--provenance-json", (ConvertTo-Phase12CompactJson $provenance),
        "--correlation-id", $correlationId,
        "--core-root", $CoreRoot,
        "--extension-root", $PluginRoot,
        "--request-path", $requestPath,
        "--response-path", $responsePath,
        "--trace-path", $tracePath
    )

    & node @nodeArgs > $stdoutPath 2> $stderrPath
    $exitCode = $LASTEXITCODE
    $stdoutText = if (Test-Path -LiteralPath $stdoutPath) { Get-Content -Raw -LiteralPath $stdoutPath } else { "" }
    $stderrText = if (Test-Path -LiteralPath $stderrPath) { Get-Content -Raw -LiteralPath $stderrPath } else { "" }
    Write-Phase12Text -Path $stdoutPath -Text $stdoutText
    Write-Phase12Text -Path $stderrPath -Text $stderrText
    foreach ($generatedJsonPath in @($requestPath, $responsePath, $tracePath)) {
        if (Test-Path -LiteralPath $generatedJsonPath) {
            Write-Phase12Text -Path $generatedJsonPath -Text (Get-Content -Raw -LiteralPath $generatedJsonPath)
        }
    }

    $response = $null
    if (Test-Path -LiteralPath $responsePath) {
        $response = Get-Content -Raw -LiteralPath $responsePath | ConvertFrom-Json
    }

    $requestText = if (Test-Path -LiteralPath $requestPath) { Get-Content -Raw -LiteralPath $requestPath } else { "" }
    $outputText = if ($response) {
        [string](Get-Phase12Property -Object $response -Name "ChatDisplayText" -Default "")
    } else {
        "NO_RESPONSE_WRITTEN: $stderrText"
    }
    $runSource = if ($Mode -eq $AblationMode) {
        "adapter_core_trace_with_skill_context_signals_removed; no live host UI"
    } else {
        $HostSurface.adaptive_run_source
    }
    $trace = New-Phase12Trace -Scenario $Scenario -HostSurface $HostSurface -Mode $Mode -RunSource $runSource -RequestPath $requestPath -ResponsePath $responsePath -TracePath $tracePath -InputText $requestText -OutputText $outputText -ExitCode $exitCode -Succeeded ($exitCode -eq 0 -and $null -ne $response)

    [ordered]@{
        request_path = $requestPath
        response_path = $responsePath
        trace_path = $tracePath
        stdout_path = $stdoutPath
        stderr_path = $stderrPath
        exit_code = $exitCode
        response = $response
        token_trace = $trace
        command = "node $PluginEntrypointPath --method $($Scenario.method) --user-prompt-or-task-text <phase12-heldout-prompt> --core-root $CoreRoot --request-path $requestPath --response-path $responsePath --trace-path $tracePath"
    }
}

function Test-Phase12ProjectLeakage {
    param([Parameter(Mandatory = $true)][AllowEmptyString()][string]$Text)

    # 'C:\Projects' leak needle (JSON-escaped form), spelled via [char]92 so
    # this scanner never trips its own or the repo's path-leak scans.
    $projectsLeakNeedle = "C:" + [char]92 + [char]92 + "Projects"
    $patterns = @(
        $projectsLeakNeedle,
        "llm_mvp_core",
        "43.SmartSkillsDomain",
        "models/",
        "phase12-cross-host-token-quality"
    )
    foreach ($pattern in $patterns) {
        if ($Text.IndexOf($pattern, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
            return $true
        }
    }

    return $false
}

function Test-Phase12CandidateAuthority {
    param([Parameter(Mandatory = $true)][AllowEmptyString()][string]$Evidence)

    $hasCandidateEvidence = $Evidence -match "candidate_position|candidate_order"
    $authorityDetected =
        $Evidence -match "candidate_order_permutation_selection_authority=true" -or
        $Evidence -match "candidate_order_sensitivity_detected=True" -or
        $Evidence -match "candidate_position_final_authority=true"
    $provenanceOnly =
        $Evidence -match "candidate_position_role=diagnostics_provenance_only" -and
        $Evidence -match "candidate_order_guard_status=selection_authority_blocked" -and
        $Evidence -match "candidate_order_permutation_selection_authority=false"

    [ordered]@{
        has_candidate_position_or_order_evidence = [bool]$hasCandidateEvidence
        provenance_only = [bool]($provenanceOnly -and -not $authorityDetected)
        authority_detected = [bool]$authorityDetected
    }
}

function New-Phase12Criterion {
    param(
        [Parameter(Mandatory = $true)][string]$Id,
        [Parameter(Mandatory = $true)][string]$NameRu,
        [Parameter(Mandatory = $true)][int]$Score,
        [Parameter(Mandatory = $true)][bool]$Passed,
        [Parameter(Mandatory = $true)][string]$Evidence
    )

    [ordered]@{
        criterion_id = $Id
        name_ru = $NameRu
        score = $Score
        max_score = 2
        passed = $Passed
        evidence = $Evidence
    }
}

function Get-Phase12QualityScore {
    param(
        [Parameter(Mandatory = $true)]$Scenario,
        [Parameter(Mandatory = $true)]$HostSurface,
        [Parameter(Mandatory = $true)][string]$Mode,
        [Parameter(Mandatory = $true)]$Response
    )

    $criteria = New-Object "System.Collections.Generic.List[object]"
    if ($Mode -eq $BaselineMode) {
        $responseText = [string]$Response.response_text
        $criteria.Add((New-Phase12Criterion -Id "correct_task_extraction" -NameRu "Correct task extraction" -Score 1 -Passed $false -Evidence "Baseline synthetic response names the generic task but has no structured task refs."))
        $criteria.Add((New-Phase12Criterion -Id "universal_orchestration_plan_quality" -NameRu "Universal orchestration plan quality" -Score 1 -Passed $false -Evidence "Baseline gives generic review/evidence gates, not a structured XIIGen orchestration trace."))
        $criteria.Add((New-Phase12Criterion -Id "skill_selection_relevance" -NameRu "Skill selection relevance" -Score 0 -Passed $false -Evidence "Baseline has no XIIGen/local skill selection evidence."))
        $criteria.Add((New-Phase12Criterion -Id "no_project_specific_leakage" -NameRu "No project-specific leakage" -Score ($(if (Test-Phase12ProjectLeakage -Text $responseText) { 0 } else { 2 })) -Passed (-not (Test-Phase12ProjectLeakage -Text $responseText)) -Evidence "Scored visible baseline response text only."))
        $criteria.Add((New-Phase12Criterion -Id "no_hidden_deterministic_shortcut_claim" -NameRu "No hidden deterministic shortcut claim" -Score 2 -Passed $true -Evidence "Baseline explicitly labels itself synthetic and does not claim hidden deterministic authority."))
        $criteria.Add((New-Phase12Criterion -Id "no_overclaim" -NameRu "No overclaim" -Score 2 -Passed $true -Evidence "Baseline keeps token, quality, training, checkpoint, and readiness claims closed."))
    } else {
        $chatText = [string](Get-Phase12Property -Object $Response -Name "ChatDisplayText" -Default "")
        $structuredRefs = @(Get-Phase12Property -Object $Response -Name "StructuredTaskRefs" -Default @())
        $selectedSkills = @(Get-Phase12Property -Object $Response -Name "SelectedSkills" -Default @())
        $promptStatus = Get-Phase12CheckValue -Response $Response -Name "prompt_to_task_status"
        $orchestrationStatus = Get-Phase12CheckValue -Response $Response -Name "orchestration_reasoning_status"
        $skillEvidence = Get-Phase12CheckValue -Response $Response -Name "skill_selection_evidence"
        $requestedCount = [int](Get-Phase12CheckValue -Response $Response -Name "requested_skills_count" -Default "0")
        $candidateAuthority = Test-Phase12CandidateAuthority -Evidence $skillEvidence
        $semanticClaimed = (Get-Phase12CheckValue -Response $Response -Name "semantic_quality_claimed") -eq "true"
        $runtimeClaimed = (Get-Phase12CheckValue -Response $Response -Name "runtime_extension_claimed") -eq "true"
        $leakageDetected = Test-Phase12ProjectLeakage -Text $chatText

        $taskScore = if ($structuredRefs.Count -gt 0 -and $promptStatus -match "semantic_task_produced") { 2 } elseif ($promptStatus) { 1 } else { 0 }
        $taskPass = $taskScore -eq 2
        $planScore = if ($orchestrationStatus -eq "orchestration_plan_produced") { 2 } elseif ($orchestrationStatus) { 1 } else { 0 }
        $planPass = $planScore -eq 2
        $skillScore = if ($selectedSkills.Count -gt 0) { 2 } elseif ($requestedCount -gt 0 -and -not $candidateAuthority.authority_detected) { 1 } else { 0 }
        $skillPass = $skillScore -eq 2
        $noShortcutPass = -not $candidateAuthority.authority_detected
        $noOverclaimPass = -not $semanticClaimed -and -not $runtimeClaimed

        $criteria.Add((New-Phase12Criterion -Id "correct_task_extraction" -NameRu "Correct task extraction" -Score $taskScore -Passed $taskPass -Evidence "prompt_to_task_status=$promptStatus; structured_task_ref_count=$($structuredRefs.Count)."))
        $criteria.Add((New-Phase12Criterion -Id "universal_orchestration_plan_quality" -NameRu "Universal orchestration plan quality" -Score $planScore -Passed $planPass -Evidence "orchestration_reasoning_status=$orchestrationStatus. Partial status does not prove full plan quality."))
        $criteria.Add((New-Phase12Criterion -Id "skill_selection_relevance" -NameRu "Skill selection relevance" -Score $skillScore -Passed $skillPass -Evidence "selected_skill_count=$($selectedSkills.Count); requested_skills_count=$requestedCount. Empty SelectedSkills remains not proven."))
        $criteria.Add((New-Phase12Criterion -Id "no_project_specific_leakage" -NameRu "No project-specific leakage" -Score ($(if ($leakageDetected) { 0 } else { 2 })) -Passed (-not $leakageDetected) -Evidence "Scored visible ChatDisplayText; persisted diagnostic asset refs are evidence, not user-visible answer text."))
        $criteria.Add((New-Phase12Criterion -Id "no_hidden_deterministic_shortcut_claim" -NameRu "No hidden deterministic shortcut claim" -Score ($(if ($noShortcutPass) { 2 } else { 0 })) -Passed $noShortcutPass -Evidence "candidate_authority_detected=$($candidateAuthority.authority_detected); provenance_only=$($candidateAuthority.provenance_only)."))
        $criteria.Add((New-Phase12Criterion -Id "no_overclaim" -NameRu "No overclaim" -Score ($(if ($noOverclaimPass) { 2 } else { 0 })) -Passed $noOverclaimPass -Evidence "semantic_quality_claimed=$semanticClaimed; runtime_extension_claimed=$runtimeClaimed."))
    }

    $total = 0
    $allPassed = $true
    foreach ($criterion in $criteria) {
        $total += [int]$criterion.score
        if (-not [bool]$criterion.passed) {
            $allPassed = $false
        }
    }

    [ordered]@{
        schema_version = "xiigen.phase12.quality_score.v1"
        run_id = $RunId
        scenario_id = $Scenario.scenario_id
        host_surface_label = $HostSurface.host_surface_label
        mode = $Mode
        total_score = $total
        max_score = 12
        all_criteria_passed = $allPassed
        criteria = @($criteria.ToArray())
    }
}

function New-Phase12Rubric {
    [ordered]@{
        schema_version = "xiigen.phase12.quality_rubric.v1"
        scoring_scale = "0=not proven/fail, 1=partial evidence, 2=pass"
        success_rule = "Adaptive token success is accepted only when adaptive total tokens are lower, no criterion regresses against baseline, and all adaptive rubric criteria pass."
        criteria = @(
            [ordered]@{
                criterion_id = "correct_task_extraction"
                pass_rule = "Response must contain structured task evidence or equivalent task extraction evidence for the same held-out prompt."
                fail_rule = "No task extraction evidence or only generic restatement."
            }
            [ordered]@{
                criterion_id = "universal_orchestration_plan_quality"
                pass_rule = "Response must produce a universal plan with review/evidence/stop gates and no project-stack-specific trick."
                fail_rule = "No plan, only a placeholder, or a status saying no matching orchestration inference."
            }
            [ordered]@{
                criterion_id = "skill_selection_relevance"
                pass_rule = "Relevant skills are actually selected or the skill selection artifact proves final selection authority without candidate-position leakage."
                fail_rule = "SelectedSkills is empty, only a requested-skill count is present, or final skill authority is absent."
            }
            [ordered]@{
                criterion_id = "no_project_specific_leakage"
                pass_rule = "The scored user-visible answer does not contain repo paths, model paths, source ids, scenario ids, or pair ids as task authority."
                fail_rule = "Visible answer uses project paths/source ids/pair ids as answer authority."
            }
            [ordered]@{
                criterion_id = "no_hidden_deterministic_shortcut_claim"
                pass_rule = "No hidden deterministic shortcut is introduced or claimed; candidate-position diagnostics are provenance-only if present."
                fail_rule = "Candidate order/position becomes selection authority or shortcut evidence is hidden."
            }
            [ordered]@{
                criterion_id = "no_overclaim"
                pass_rule = "No claim of semantic quality, learnability, training, checkpoint promotion, full chat UI, token savings, or all-host readiness beyond evidence."
                fail_rule = "Any such claim is made without direct Phase 12 proof."
            }
        )
    }
}

function New-Phase12MatrixMarkdown {
    param([Parameter(Mandatory = $true)]$Rows)

    $lines = New-Object "System.Collections.Generic.List[string]"
    $lines.Add("# Phase 12 scenario pair matrix") | Out-Null
    $lines.Add("") | Out-Null
    $lines.Add("| Scenario | Exercise | Method | Host | Baseline source | Adaptive source | Prior evidence |") | Out-Null
    $lines.Add("|---|---|---|---|---|---|---|") | Out-Null
    foreach ($row in $Rows) {
        $lines.Add("| $($row.scenario_id) | $($row.exercise) | $($row.method) | $($row.host_surface_label) | $($row.baseline_source) | $($row.adaptive_source) | $($row.prior_evidence_path) |") | Out-Null
    }
    return ($lines -join "`n")
}

function New-Phase12RubricMarkdown {
    param([Parameter(Mandatory = $true)]$Rubric)

    $lines = New-Object "System.Collections.Generic.List[string]"
    $lines.Add("# Phase 12 quality rubric") | Out-Null
    $lines.Add("") | Out-Null
    $lines.Add("Scale: $($Rubric.scoring_scale)") | Out-Null
    $lines.Add("") | Out-Null
    $lines.Add("Success rule: $($Rubric.success_rule)") | Out-Null
    $lines.Add("") | Out-Null
    $lines.Add("| Criterion | Pass | Fail |") | Out-Null
    $lines.Add("|---|---|---|") | Out-Null
    foreach ($criterion in $Rubric.criteria) {
        $lines.Add("| $($criterion.criterion_id) | $($criterion.pass_rule) | $($criterion.fail_rule) |") | Out-Null
    }
    return ($lines -join "`n")
}

function New-Phase12TokenReportMarkdown {
    param([Parameter(Mandatory = $true)]$Report)

    $lines = New-Object "System.Collections.Generic.List[string]"
    $lines.Add("# Phase 12 token accounting and variance") | Out-Null
    $lines.Add("") | Out-Null
    $lines.Add("Token estimate method: " + $Report.token_estimate_method + ". This is a heuristic, not a specific provider's tokenizer.") | Out-Null
    $lines.Add("") | Out-Null
    $lines.Add("| Scenario | Host | Baseline total | Adaptive total | Delta | Reduction | Quality block |") | Out-Null
    $lines.Add("|---|---|---:|---:|---:|---|---|") | Out-Null
    foreach ($row in $Report.rows) {
        $lines.Add("| $($row.scenario_id) | $($row.host_surface_label) | $($row.baseline_total_tokens) | $($row.adaptive_total_tokens) | $($row.token_delta) | $($row.token_reduction_observed) | $($row.token_success_blocked_by_quality) |") | Out-Null
    }
    $lines.Add("") | Out-Null
    $lines.Add("Summary: token reduction in the harness evaluation is observed in " + $Report.token_reduction_observed_count + " of " + $Report.pair_count + " pairs. Accepted token success: " + $Report.token_success_accepted + ".") | Out-Null
    return ($lines -join "`n")
}

function New-Phase12QualityReportMarkdown {
    param([Parameter(Mandatory = $true)]$Report)

    $lines = New-Object "System.Collections.Generic.List[string]"
    $lines.Add("# Phase 12 quality scoring") | Out-Null
    $lines.Add("") | Out-Null
    $lines.Add("Quality regression anywhere: " + $Report.quality_regressed_anywhere + ".") | Out-Null
    $lines.Add("All adaptive criteria PASS anywhere/everywhere: " + $Report.all_adaptive_pairs_passed + ".") | Out-Null
    $lines.Add("") | Out-Null
    $lines.Add("| Scenario | Host | Baseline | Adaptive | Delta | Regressed | Adaptive pass | Block reasons |") | Out-Null
    $lines.Add("|---|---|---:|---:|---:|---|---|---|") | Out-Null
    foreach ($row in $Report.rows) {
        $reasons = ($row.block_reasons -join ", ")
        $lines.Add("| $($row.scenario_id) | $($row.host_surface_label) | $($row.baseline_total_score) | $($row.adaptive_total_score) | $($row.quality_delta) | $($row.quality_regressed) | $($row.adaptive_all_criteria_passed) | $reasons |") | Out-Null
    }
    return ($lines -join "`n")
}

function New-Phase12HeldoutMarkdown {
    param([Parameter(Mandatory = $true)]$Report)

    $lines = New-Object "System.Collections.Generic.List[string]"
    $lines.Add("# Phase 12 held-out, ablation, no-leakage checks") | Out-Null
    $lines.Add("") | Out-Null
    $lines.Add("Overall status: " + $Report.overall_status + ".") | Out-Null
    $lines.Add("Full disjoint check against the entire training/source corpus: " + $Report.full_training_corpus_disjoint_proven + ".") | Out-Null
    $lines.Add("") | Out-Null
    $lines.Add("| Check | Status | Evidence |") | Out-Null
    $lines.Add("|---|---|---|") | Out-Null
    foreach ($check in $Report.checks) {
        $lines.Add("| $($check.check_id) | $($check.status) | $($check.evidence) |") | Out-Null
    }
    return ($lines -join "`n")
}

function Format-Phase12MarkdownCell {
    param([Parameter(Mandatory = $true)][AllowEmptyString()][string]$Text)

    return ($Text.Replace("|", "/").Replace("`r", " ").Replace("`n", " "))
}

function Get-Phase12FailureClass {
    param(
        [Parameter(Mandatory = $true)][string]$Mode,
        [Parameter(Mandatory = $true)][string]$CriterionId,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Evidence
    )

    if ($Mode -eq $BaselineMode) {
        return "baseline_control_expected_not_product_failure"
    }
    if ($Mode -eq $AblationMode -and $CriterionId -eq "skill_selection_relevance") {
        return "controlled_ablation_expected_skill_signal_removed"
    }
    if ($CriterionId -eq "universal_orchestration_plan_quality" -and $Evidence -match "no_matching_learned_pattern") {
        return "actual_product_model_failure_orchestration_inference_absent"
    }

    return "needs_review_unclassified_failure"
}

function New-Phase12FailingCriteriaMap {
    param([Parameter(Mandatory = $true)]$QualityScores)

    $rows = New-Object "System.Collections.Generic.List[object]"
    foreach ($score in $QualityScores) {
        foreach ($criterion in $score.criteria) {
            if ([bool]$criterion.passed) {
                continue
            }

            $failureClass = Get-Phase12FailureClass -Mode $score.mode -CriterionId $criterion.criterion_id -Evidence ([string]$criterion.evidence)
            $rows.Add([pscustomobject][ordered]@{
                scenario_id = $score.scenario_id
                host_surface_label = $score.host_surface_label
                run_type = $score.mode
                criterion_id = $criterion.criterion_id
                score = [int]$criterion.score
                max_score = [int]$criterion.max_score
                evidence = [string]$criterion.evidence
                failure_class = $failureClass
                measurement_failure = $failureClass -match "^baseline_control|^controlled_ablation"
                actual_product_or_model_failure = $failureClass -match "^actual_product_model_failure"
                blocks_phase12_adaptive_pass = ($score.mode -eq $AdaptiveMode)
            }) | Out-Null
        }
    }

    $rowArray = @($rows.ToArray())
    $adaptiveRows = @($rowArray | Where-Object { $_.run_type -eq $AdaptiveMode })
    $productRows = @($rowArray | Where-Object { $_.actual_product_or_model_failure })

    [ordered]@{
        schema_version = "xiigen.phase12.failing_quality_criteria_map.v1"
        run_id = $RunId
        source_run_provenance_path = if ([string]::IsNullOrWhiteSpace($SourceRunDir)) { "not_provided_to_runner" } else { $SourceRunDir }
        failed_criteria_row_count = $rowArray.Count
        adaptive_failed_criteria_row_count = $adaptiveRows.Count
        product_or_model_failure_row_count = $productRows.Count
        product_quality_blocked = $adaptiveRows.Count -gt 0
        adaptive_failed_criterion_ids = @($adaptiveRows | Select-Object -ExpandProperty criterion_id -Unique)
        failed_rows_by_run_type = @($rowArray | Group-Object run_type | ForEach-Object {
            [ordered]@{
                run_type = $_.Name
                failed_criteria_rows = $_.Count
            }
        })
        rows = $rowArray
    }
}

function New-Phase12FailingCriteriaMapMarkdown {
    param([Parameter(Mandatory = $true)]$Map)

    $lines = New-Object "System.Collections.Generic.List[string]"
    $lines.Add("# Phase 12 failing quality criteria map") | Out-Null
    $lines.Add("") | Out-Null
    $lines.Add("Source run provenance path: $($Map.source_run_provenance_path)") | Out-Null
    $lines.Add("Failed criteria rows: $($Map.failed_criteria_row_count). Adaptive failed rows: $($Map.adaptive_failed_criteria_row_count).") | Out-Null
    $lines.Add("Product quality blocked: $($Map.product_quality_blocked).") | Out-Null
    $lines.Add("") | Out-Null
    $lines.Add("| Scenario | Host | Run type | Criterion | Score | Failure class | Blocks adaptive pass | Evidence |") | Out-Null
    $lines.Add("|---|---|---|---|---:|---|---|---|") | Out-Null
    foreach ($row in $Map.rows) {
        $evidence = Format-Phase12MarkdownCell -Text ([string]$row.evidence)
        $lines.Add("| $($row.scenario_id) | $($row.host_surface_label) | $($row.run_type) | $($row.criterion_id) | $($row.score)/$($row.max_score) | $($row.failure_class) | $($row.blocks_phase12_adaptive_pass) | $evidence |") | Out-Null
    }
    return ($lines -join "`n")
}

function New-Phase12FixAnalysis {
    param(
        [Parameter(Mandatory = $true)]$FailureMap,
        [Parameter(Mandatory = $true)]$TokenAccountingReport,
        [Parameter(Mandatory = $true)]$QualityReport,
        [Parameter(Mandatory = $true)]$HeldoutReport
    )

    $adaptiveFailures = @($FailureMap.rows | Where-Object { $_.run_type -eq $AdaptiveMode })
    $visibleLeakFailures = @($FailureMap.rows | Where-Object { $_.criterion_id -eq "no_project_specific_leakage" -and $_.run_type -ne $BaselineMode })
    $shortcutFailures = @($FailureMap.rows | Where-Object { $_.criterion_id -eq "no_hidden_deterministic_shortcut_claim" -and $_.run_type -ne $BaselineMode })
    $adaptiveOrchestrationFailures = @($adaptiveFailures | Where-Object { $_.criterion_id -eq "universal_orchestration_plan_quality" })

    [ordered]@{
        schema_version = "xiigen.phase12.fix_analysis.v1"
        run_id = $RunId
        source_run_provenance_path = if ([string]::IsNullOrWhiteSpace($SourceRunDir)) { "not_provided_to_runner" } else { $SourceRunDir }
        result = if ($FailureMap.product_quality_blocked) { "PHASE_12_BLOCKED_BY_PRODUCT_QUALITY_FIXES" } else { "PHASE_12_TOKEN_QUALITY_READY_FOR_REVIEW" }
        measurement_vs_product = @(
            [ordered]@{
                finding_id = "MEASUREMENT-12-01"
                classification = "measurement_report_issue_fixed_in_this_runner"
                evidence = "The earlier headline said orchestration-plan and skill-selection remained partial/not proven, but adaptive raw scores only fail universal_orchestration_plan_quality. Fresh reports now use the failing criteria map as the source of truth."
            }
            [ordered]@{
                finding_id = "MEASUREMENT-12-02"
                classification = "measurement_coverage_issue_fixed_in_this_runner"
                evidence = "Ablation responses are now quality-scored and included in raw_scores/failing-quality-criteria-map; adaptive-vs-baseline remains the Phase 12 pass gate."
            }
            [ordered]@{
                finding_id = "PRODUCT-12-01"
                classification = if ($adaptiveOrchestrationFailures.Count -eq 0) { "product_model_gate_closed_in_this_run" } else { "actual_product_model_failure" }
                evidence = if ($adaptiveOrchestrationFailures.Count -eq 0) {
                    "Adaptive universal_orchestration_plan_quality passes in all $($TokenAccountingReport.pair_count) scenario/host pairs; no loaded_guide_state_no_matching_learned_pattern adaptive blockers remain."
                } else {
                    "Adaptive universal_orchestration_plan_quality fails in $($adaptiveOrchestrationFailures.Count) of $($TokenAccountingReport.pair_count) scenario/host pairs with loaded_guide_state_no_matching_learned_pattern."
                }
            }
        )
        failing_quality_summary = [ordered]@{
            adaptive_failed_rows = $FailureMap.adaptive_failed_criteria_row_count
            adaptive_failed_criterion_ids = $FailureMap.adaptive_failed_criterion_ids
            all_adaptive_pairs_passed = $QualityReport.all_adaptive_pairs_passed
            quality_regressed_anywhere = $QualityReport.quality_regressed_anywhere
        }
        leakage_summary = [ordered]@{
            user_visible_project_specific_leakage_failures = $visibleLeakFailures.Count
            audit_metadata_project_specific_refs_present = $true
            distinction = "Project/run ids and llm_mvp_core appear in persisted audit metadata/contracts and file paths; scored user-visible ChatDisplayText does not fail no_project_specific_leakage."
            full_training_corpus_disjoint_proven = $HeldoutReport.full_training_corpus_disjoint_proven
        }
        hidden_shortcut_summary = [ordered]@{
            hidden_deterministic_shortcut_failures = $shortcutFailures.Count
            candidate_position_authority_status = "No candidate-position authority failures in Phase 12 quality scores or heldout checks."
        }
        token_reduction_summary = [ordered]@{
            token_estimate_method = $TokenAccountingReport.token_estimate_method
            token_reduction_observed_pairs = $TokenAccountingReport.token_reduction_observed_count
            pair_count = $TokenAccountingReport.pair_count
            token_success_accepted = $TokenAccountingReport.token_success_accepted -and -not $QualityReport.token_success_blocked_by_quality
            decision = if ($TokenAccountingReport.token_success_accepted -and -not $QualityReport.token_success_blocked_by_quality) {
                "Token reduction is internally consistent enough for this harness and accepted because adaptive quality closes all rubric criteria."
            } else {
                "Token reduction is internally consistent enough for this harness if quality passes, but remains blocked because adaptive quality does not pass."
            }
        }
    }
}

function New-Phase12FixAnalysisMarkdown {
    param([Parameter(Mandatory = $true)]$Analysis)

    $lines = New-Object "System.Collections.Generic.List[string]"
    $lines.Add("# Phase 12 fix analysis") | Out-Null
    $lines.Add("") | Out-Null
    $lines.Add("Result: $($Analysis.result)") | Out-Null
    $lines.Add("Source run provenance path: $($Analysis.source_run_provenance_path)") | Out-Null
    $lines.Add("") | Out-Null
    $lines.Add("## Measurement vs product") | Out-Null
    foreach ($finding in $Analysis.measurement_vs_product) {
        $lines.Add("- $($finding.finding_id): $($finding.classification) - $($finding.evidence)") | Out-Null
    }
    $lines.Add("") | Out-Null
    $lines.Add("## Quality") | Out-Null
    $lines.Add("- Adaptive failed rows: $($Analysis.failing_quality_summary.adaptive_failed_rows)") | Out-Null
    $lines.Add("- Adaptive failed criteria: $($Analysis.failing_quality_summary.adaptive_failed_criterion_ids -join ', ')") | Out-Null
    $lines.Add("- All adaptive pairs passed: $($Analysis.failing_quality_summary.all_adaptive_pairs_passed)") | Out-Null
    $lines.Add("- Quality regressed anywhere: $($Analysis.failing_quality_summary.quality_regressed_anywhere)") | Out-Null
    $lines.Add("") | Out-Null
    $lines.Add("## Leakage and shortcuts") | Out-Null
    $lines.Add("- User-visible project-specific leakage failures: $($Analysis.leakage_summary.user_visible_project_specific_leakage_failures)") | Out-Null
    $lines.Add("- Audit metadata refs present: $($Analysis.leakage_summary.audit_metadata_project_specific_refs_present)") | Out-Null
    $lines.Add("- Full training corpus disjoint proven: $($Analysis.leakage_summary.full_training_corpus_disjoint_proven)") | Out-Null
    $lines.Add("- Hidden deterministic shortcut failures: $($Analysis.hidden_shortcut_summary.hidden_deterministic_shortcut_failures)") | Out-Null
    $lines.Add("") | Out-Null
    $lines.Add("## Token decision") | Out-Null
    $lines.Add("- Token reduction observed: $($Analysis.token_reduction_summary.token_reduction_observed_pairs)/$($Analysis.token_reduction_summary.pair_count)") | Out-Null
    $lines.Add("- Token success accepted: $($Analysis.token_reduction_summary.token_success_accepted)") | Out-Null
    $lines.Add("- Decision: $($Analysis.token_reduction_summary.decision)") | Out-Null
    return ($lines -join "`n")
}

function New-Phase12RequiredFixesBeforePass {
    param([Parameter(Mandatory = $true)]$FailureMap)

    $adaptiveBlockingRows = @($FailureMap.rows | Where-Object { $_.blocks_phase12_adaptive_pass })
    [ordered]@{
        schema_version = "xiigen.phase12.required_fixes_before_pass.v1"
        packet_title = "PHASE_12_BLOCKED_BY_PRODUCT_QUALITY_FIXES"
        run_id = $RunId
        blocking_adaptive_rows = $adaptiveBlockingRows
        required_product_or_model_fixes = @(
            [ordered]@{
                fix_id = "ORCH-12-01"
                target = "orchestration_reasoning model/checkpoint or adapter-to-core inference path"
                required_change = "Produce orchestration_reasoning_status=orchestration_plan_produced for the Phase 12 held-out universal prompts, with review gates, rollback/evidence/stop-condition content, and no project-specific authority leakage."
                acceptance_evidence = "Fresh Phase 12 run shows universal_orchestration_plan_quality passed for all 9 adaptive scenario/host pairs, no adaptive rubric failures, no quality regression, and token reduction still observed."
            }
            [ordered]@{
                fix_id = "ORCH-12-02"
                target = "held-out orchestration evidence"
                required_change = "Persist enough structured orchestration evidence for the rubric to distinguish a real universal plan from loaded-guide-state/no-matching-pattern fallback."
                acceptance_evidence = "quality-scoring-report.json raw_scores contains score=2/passed=true for universal_orchestration_plan_quality in every adaptive pair."
            }
        )
        forbidden_shortcuts = @(
            "Do not relabel loaded_guide_state_no_matching_learned_pattern as PASS.",
            "Do not claim semantic quality improvement, learnability, training, checkpoint promotion, full chat UI readiness, or final readiness without direct evidence.",
            "Do not treat lower heuristic token count as accepted while adaptive quality criteria fail."
        )
    }
}

function New-Phase12RequiredFixesMarkdown {
    param([Parameter(Mandatory = $true)]$RequiredFixes)

    $lines = New-Object "System.Collections.Generic.List[string]"
    $lines.Add("# Phase 12 required fixes before pass") | Out-Null
    $lines.Add("") | Out-Null
    $lines.Add("Packet: $($RequiredFixes.packet_title)") | Out-Null
    $lines.Add("Blocking adaptive rows: $($RequiredFixes.blocking_adaptive_rows.Count)") | Out-Null
    $lines.Add("") | Out-Null
    $lines.Add("| Fix id | Target | Required change | Acceptance evidence |") | Out-Null
    $lines.Add("|---|---|---|---|") | Out-Null
    foreach ($fix in $RequiredFixes.required_product_or_model_fixes) {
        $lines.Add("| $($fix.fix_id) | $($fix.target) | $(Format-Phase12MarkdownCell -Text $fix.required_change) | $(Format-Phase12MarkdownCell -Text $fix.acceptance_evidence) |") | Out-Null
    }
    $lines.Add("") | Out-Null
    $lines.Add("Forbidden shortcuts:") | Out-Null
    foreach ($shortcut in $RequiredFixes.forbidden_shortcuts) {
        $lines.Add("- $shortcut") | Out-Null
    }
    return ($lines -join "`n")
}

function New-Phase12FinalMarkdown {
    param([Parameter(Mandatory = $true)]$Report)

    $lines = New-Object "System.Collections.Generic.List[string]"
    $lines.Add("# Phase 12 cross-host token and quality comparison") | Out-Null
    $lines.Add("") | Out-Null
    $lines.Add("Verdict: " + $Report.packet_title + ".") | Out-Null
    $lines.Add("") | Out-Null
    $lines.Add("What improved: " + $Report.russian_headline.what_improved) | Out-Null
    $lines.Add("What did not improve: " + $Report.russian_headline.what_did_not_improve) | Out-Null
    $lines.Add("What is not proven: " + $Report.russian_headline.what_is_not_proven) | Out-Null
    $lines.Add("") | Out-Null
    $lines.Add("Quality regression anywhere: " + $Report.quality_regressed_anywhere + ".") | Out-Null
    $lines.Add("Token success accepted: $($Report.token_success_accepted).") | Out-Null
    $lines.Add("Token success blocked by quality: $($Report.token_success_blocked_by_quality).") | Out-Null
    $lines.Add("Adaptive quality closes all rubric criteria: $($Report.token_quality_headline.adaptive_quality_closes_all_rubric_criteria) (quality pass $($Report.token_quality_headline.adaptive_quality_pass_summary)).") | Out-Null
    $lines.Add("universal_orchestration_plan_quality passes for all adaptive pairs: $($Report.token_quality_headline.universal_orchestration_plan_quality_all_adaptive_pairs_passed) ($($Report.token_quality_headline.universal_orchestration_plan_quality_pass_summary)).") | Out-Null
    $lines.Add("") | Out-Null
    $lines.Add("Non-claims:") | Out-Null
    foreach ($claim in $Report.exact_non_claims) {
        $lines.Add("- $claim") | Out-Null
    }
    $lines.Add("") | Out-Null
    $lines.Add("Proposed parent staging list:") | Out-Null
    foreach ($path in $Report.proposed_parent_staging_list) {
        $lines.Add("- $path") | Out-Null
    }
    $lines.Add("") | Out-Null
    $lines.Add("Evidence paths:") | Out-Null
    foreach ($property in $Report.evidence_paths.GetEnumerator()) {
        $lines.Add("- $($property.Key): $($property.Value)") | Out-Null
    }
    return ($lines -join "`n")
}

New-Phase12Directory -Path $RunDir
if (-not (Test-Path -LiteralPath $PluginEntrypointPath)) {
    throw "Codex plugin entrypoint not found: $PluginEntrypointPath"
}

$Scenarios = New-Phase12Scenarios
$Hosts = New-Phase12Hosts
$AvailableSkills = New-Phase12AvailableSkills
$Rubric = New-Phase12Rubric
$MatrixRows = New-Object "System.Collections.Generic.List[object]"
$BaselineTraces = New-Object "System.Collections.Generic.List[object]"
$AdaptiveTraces = New-Object "System.Collections.Generic.List[object]"
$AblationTraces = New-Object "System.Collections.Generic.List[object]"
$QualityScores = New-Object "System.Collections.Generic.List[object]"
$PairRows = New-Object "System.Collections.Generic.List[object]"
$CommandsRun = New-Object "System.Collections.Generic.List[object]"
$HeldoutChecks = New-Object "System.Collections.Generic.List[object]"

foreach ($scenario in $Scenarios) {
    foreach ($hostSurface in $Hosts) {
        $pairDir = Join-Path $RunDir (Join-Path $scenario.scenario_id $hostSurface.host_id)
        New-Phase12Directory -Path $pairDir
        $baselineRequestPath = Join-Path $pairDir "baseline-request.json"
        $baselineResponsePath = Join-Path $pairDir "baseline-response.json"
        $baselineTracePath = Join-Path $pairDir "baseline-token-trace.json"
        $baselineRequest = New-Phase12ManualBaselineRequest -Scenario $scenario -HostSurface $hostSurface
        $baselineResponse = New-Phase12ManualBaselineResponse -Scenario $scenario -HostSurface $hostSurface
        Write-Phase12Json -Path $baselineRequestPath -Value $baselineRequest
        Write-Phase12Json -Path $baselineResponsePath -Value $baselineResponse
        $baselineInputText = ConvertTo-Phase12Json $baselineRequest
        $baselineOutputText = [string]$baselineResponse.response_text
        $baselineTrace = New-Phase12Trace -Scenario $scenario -HostSurface $hostSurface -Mode $BaselineMode -RunSource $baselineRequest.source_label -RequestPath $baselineRequestPath -ResponsePath $baselineResponsePath -TracePath $baselineTracePath -InputText $baselineInputText -OutputText $baselineOutputText
        Write-Phase12Json -Path $baselineTracePath -Value $baselineTrace
        $BaselineTraces.Add($baselineTrace) | Out-Null
        $baselineQuality = Get-Phase12QualityScore -Scenario $scenario -HostSurface $hostSurface -Mode $BaselineMode -Response ([pscustomobject]$baselineResponse)
        $QualityScores.Add($baselineQuality) | Out-Null

        $adaptiveResult = Invoke-Phase12AdapterCore -Scenario $scenario -HostSurface $hostSurface -Mode $AdaptiveMode -PairDir $pairDir -RequestedSkills $scenario.requested_skills -AvailableSkills $AvailableSkills
        $AdaptiveTraces.Add($adaptiveResult.token_trace) | Out-Null
        $CommandsRun.Add([ordered]@{
            command = $adaptiveResult.command
            exit_code = $adaptiveResult.exit_code
            status = if ($adaptiveResult.exit_code -eq 0) { "PASS" } else { "FAIL" }
            mode = $AdaptiveMode
            scenario_id = $scenario.scenario_id
            host_surface_label = $hostSurface.host_surface_label
        }) | Out-Null
        $adaptiveQuality = Get-Phase12QualityScore -Scenario $scenario -HostSurface $hostSurface -Mode $AdaptiveMode -Response $adaptiveResult.response
        $QualityScores.Add($adaptiveQuality) | Out-Null

        $ablationResult = Invoke-Phase12AdapterCore -Scenario $scenario -HostSurface $hostSurface -Mode $AblationMode -PairDir $pairDir -RequestedSkills @() -AvailableSkills @()
        $AblationTraces.Add($ablationResult.token_trace) | Out-Null
        $CommandsRun.Add([ordered]@{
            command = $ablationResult.command
            exit_code = $ablationResult.exit_code
            status = if ($ablationResult.exit_code -eq 0) { "PASS" } else { "FAIL" }
            mode = $AblationMode
            scenario_id = $scenario.scenario_id
            host_surface_label = $hostSurface.host_surface_label
        }) | Out-Null
        $ablationQuality = Get-Phase12QualityScore -Scenario $scenario -HostSurface $hostSurface -Mode $AblationMode -Response $ablationResult.response
        $QualityScores.Add($ablationQuality) | Out-Null

        $criteriaRegression = New-Object "System.Collections.Generic.List[string]"
        foreach ($adaptiveCriterion in $adaptiveQuality.criteria) {
            $baselineCriterion = @($baselineQuality.criteria | Where-Object { $_.criterion_id -eq $adaptiveCriterion.criterion_id } | Select-Object -First 1)
            if ($baselineCriterion.Count -gt 0 -and [int]$adaptiveCriterion.score -lt [int]$baselineCriterion[0].score) {
                $criteriaRegression.Add($adaptiveCriterion.criterion_id) | Out-Null
            }
        }

        $tokenDelta = [int]$adaptiveResult.token_trace.total_token_estimate - [int]$baselineTrace.total_token_estimate
        $tokenReductionObserved = $tokenDelta -lt 0
        $qualityRegressed = $criteriaRegression.Count -gt 0
        $adaptivePass = [bool]$adaptiveQuality.all_criteria_passed
        $blockReasons = New-Object "System.Collections.Generic.List[string]"
        if (-not $tokenReductionObserved) {
            $blockReasons.Add("adaptive_token_total_not_lower") | Out-Null
        }
        if ($qualityRegressed) {
            $blockReasons.Add("quality_regression") | Out-Null
        }
        if (-not $adaptivePass) {
            $blockReasons.Add("adaptive_quality_criteria_not_all_passed") | Out-Null
        }
        $tokenSuccessAccepted = $tokenReductionObserved -and -not $qualityRegressed -and $adaptivePass

        $pairRows.Add([ordered]@{
            scenario_id = $scenario.scenario_id
            exercise = $scenario.exercise
            method = $scenario.method
            host_surface_label = $hostSurface.host_surface_label
            baseline_total_tokens = $baselineTrace.total_token_estimate
            adaptive_total_tokens = $adaptiveResult.token_trace.total_token_estimate
            token_delta = $tokenDelta
            token_reduction_observed = $tokenReductionObserved
            baseline_total_score = $baselineQuality.total_score
            adaptive_total_score = $adaptiveQuality.total_score
            quality_delta = [int]$adaptiveQuality.total_score - [int]$baselineQuality.total_score
            quality_regressed = $qualityRegressed
            regressed_criteria = @($criteriaRegression.ToArray())
            adaptive_all_criteria_passed = $adaptivePass
            token_success_accepted = $tokenSuccessAccepted
            token_success_blocked_by_quality = ($tokenReductionObserved -and (-not $adaptivePass -or $qualityRegressed))
            block_reasons = @($blockReasons.ToArray())
        }) | Out-Null

        $MatrixRows.Add([ordered]@{
            scenario_id = $scenario.scenario_id
            exercise = $scenario.exercise
            method = $scenario.method
            host_surface_label = $hostSurface.host_surface_label
            baseline_source = $baselineRequest.source_label
            adaptive_source = $hostSurface.adaptive_run_source
            ablation_source = "adapter_core_trace_with_skill_context_signals_removed"
            prior_evidence_path = $hostSurface.prior_evidence_path
            held_out_basis = $scenario.held_out_basis
        }) | Out-Null

        $adaptiveRequestText = if (Test-Path -LiteralPath $adaptiveResult.request_path) { Get-Content -Raw -LiteralPath $adaptiveResult.request_path } else { "" }
        $ablationRequestText = if (Test-Path -LiteralPath $ablationResult.request_path) { Get-Content -Raw -LiteralPath $ablationResult.request_path } else { "" }
        $adaptiveSkillEvidence = if ($adaptiveResult.response) { Get-Phase12CheckValue -Response $adaptiveResult.response -Name "skill_selection_evidence" } else { "" }
        $ablationSkillEvidence = if ($ablationResult.response) { Get-Phase12CheckValue -Response $ablationResult.response -Name "skill_selection_evidence" } else { "" }
        $candidateAuthority = Test-Phase12CandidateAuthority -Evidence $adaptiveSkillEvidence
        $HeldoutChecks.Add([ordered]@{
            check_id = "heldout-$($scenario.scenario_id)-$($hostSurface.host_id)"
            status = "PASS_WITH_LIMITATION"
            evidence = "$($scenario.held_out_basis) Full training corpus disjointness not proven by Phase 12."
        }) | Out-Null
        $HeldoutChecks.Add([ordered]@{
            check_id = "ablation-$($scenario.scenario_id)-$($hostSurface.host_id)"
            status = if ($adaptiveSkillEvidence -ne $ablationSkillEvidence) { "PASS_SIGNAL_CHANGED_NOT_SEMANTIC_PROOF" } else { "NOT_PROVEN_NO_SIGNAL_DIFFERENCE" }
            evidence = "Adaptive requested_skills_count=$(if ($adaptiveResult.response) { Get-Phase12CheckValue -Response $adaptiveResult.response -Name 'requested_skills_count' -Default '0' } else { 'missing' }); ablation requested/available skills removed."
        }) | Out-Null
        $HeldoutChecks.Add([ordered]@{
            check_id = "no-runtime-authority-leakage-$($scenario.scenario_id)-$($hostSurface.host_id)"
            status = if ($adaptiveRequestText.Contains($scenario.scenario_id) -or $adaptiveRequestText.Contains("|") -or $ablationRequestText.Contains($scenario.scenario_id)) { "NEEDS_REVIEW_TRACE_METADATA_PRESENT" } else { "PASS_NO_SCENARIO_OR_PAIR_ID_IN_RUNTIME_REQUEST" }
            evidence = "Pair/scenario ids are used in file paths and outer report rows, not intentionally passed as request authority."
        }) | Out-Null
        $HeldoutChecks.Add([ordered]@{
            check_id = "candidate-position-authority-$($scenario.scenario_id)-$($hostSurface.host_id)"
            status = if ($candidateAuthority.authority_detected) { "FAIL_AUTHORITY_DETECTED" } elseif ($candidateAuthority.has_candidate_position_or_order_evidence) { "PASS_PROVENANCE_ONLY" } else { "PASS_NO_CANDIDATE_POSITION_EVIDENCE" }
            evidence = "authority_detected=$($candidateAuthority.authority_detected); provenance_only=$($candidateAuthority.provenance_only)."
        }) | Out-Null
    }
}

$scenarioMatrix = [ordered]@{
    schema_version = "xiigen.phase12.scenario_pair_matrix.v1"
    run_id = $RunId
    generated_utc = (Get-Date).ToUniversalTime().ToString("o")
    scenario_count = $Scenarios.Count
    host_surface_count = $Hosts.Count
    pair_count = $MatrixRows.Count
    phase12_live_host_automation = $false
    rows = @($MatrixRows.ToArray())
}

$tokenRows = @($PairRows | ForEach-Object {
    [ordered]@{
        scenario_id = $_.scenario_id
        host_surface_label = $_.host_surface_label
        baseline_total_tokens = $_.baseline_total_tokens
        adaptive_total_tokens = $_.adaptive_total_tokens
        token_delta = $_.token_delta
        token_reduction_observed = $_.token_reduction_observed
        token_success_blocked_by_quality = $_.token_success_blocked_by_quality
        token_success_accepted = $_.token_success_accepted
    }
})
$tokenReductionObservedCount = @($tokenRows | Where-Object { $_.token_reduction_observed }).Count
$tokenAcceptedCount = @($tokenRows | Where-Object { $_.token_success_accepted }).Count
$varianceRows = @($Scenarios | ForEach-Object {
    $scenarioId = $_.scenario_id
    $rows = @($PairRows | Where-Object { $_.scenario_id -eq $scenarioId })
    $deltas = @($rows | ForEach-Object { [int]$_.token_delta })
    [ordered]@{
        scenario_id = $scenarioId
        host_count = $rows.Count
        min_token_delta = ($deltas | Measure-Object -Minimum).Minimum
        max_token_delta = ($deltas | Measure-Object -Maximum).Maximum
        delta_variance_tokens = (($deltas | Measure-Object -Maximum).Maximum - ($deltas | Measure-Object -Minimum).Minimum)
    }
})
$tokenAccountingReport = [ordered]@{
    schema_version = "xiigen.phase12.token_accounting_variance_report.v1"
    run_id = $RunId
    token_estimate_method = $TokenEstimator
    pair_count = $PairRows.Count
    rows = $tokenRows
    variance_by_scenario = $varianceRows
    token_reduction_observed_count = $tokenReductionObservedCount
    token_success_accepted_count = $tokenAcceptedCount
    token_success_accepted = $tokenAcceptedCount -eq $PairRows.Count
    limitations = @(
        "Baseline is a transparent synthetic common-host manual packet, not live host chat.",
        "Adaptive is adapter/core trace plus prior host proof label; not live Claude/Cursor/Codex chat UI.",
        "Token estimate is heuristic and comparable only inside this Phase 12 harness."
    )
}

$qualityRows = @($PairRows | ForEach-Object {
    [ordered]@{
        scenario_id = $_.scenario_id
        host_surface_label = $_.host_surface_label
        baseline_total_score = $_.baseline_total_score
        adaptive_total_score = $_.adaptive_total_score
        quality_delta = $_.quality_delta
        quality_regressed = $_.quality_regressed
        regressed_criteria = $_.regressed_criteria
        adaptive_all_criteria_passed = $_.adaptive_all_criteria_passed
        block_reasons = $_.block_reasons
    }
})
$qualityRegressedAnywhere = @($PairRows | Where-Object { $_.quality_regressed }).Count -gt 0
$allAdaptivePairsPassed = @($PairRows | Where-Object { -not $_.adaptive_all_criteria_passed }).Count -eq 0
$adaptiveQualityPairCount = $PairRows.Count
$adaptiveQualityPassCount = @($PairRows | Where-Object { $_.adaptive_all_criteria_passed }).Count
$adaptiveUniversalCriterionRows = @($QualityScores.ToArray() | Where-Object { $_.mode -eq $AdaptiveMode } | ForEach-Object {
    $qualityScore = $_
    @($qualityScore.criteria | Where-Object { $_.criterion_id -eq "universal_orchestration_plan_quality" } | ForEach-Object {
        [ordered]@{
            scenario_id = $qualityScore.scenario_id
            host_surface_label = $qualityScore.host_surface_label
            score = $_.score
            max_score = $_.max_score
            passed = [bool]$_.passed
        }
    })
})
$adaptiveUniversalPassCount = @($adaptiveUniversalCriterionRows | Where-Object { $_.passed }).Count
$universalOrchestrationPlanQualityAllPassed = $adaptiveUniversalCriterionRows.Count -eq $adaptiveQualityPairCount -and $adaptiveUniversalPassCount -eq $adaptiveQualityPairCount
$qualityReport = [ordered]@{
    schema_version = "xiigen.phase12.quality_scoring_report.v1"
    run_id = $RunId
    rows = $qualityRows
    raw_scores = @($QualityScores.ToArray())
    ablation_quality_scored = $true
    quality_regressed_anywhere = $qualityRegressedAnywhere
    all_adaptive_pairs_passed = $allAdaptivePairsPassed
    adaptive_quality_pass_count = $adaptiveQualityPassCount
    adaptive_quality_pair_count = $adaptiveQualityPairCount
    universal_orchestration_plan_quality_adaptive_passed_count = $adaptiveUniversalPassCount
    universal_orchestration_plan_quality_adaptive_pair_count = $adaptiveUniversalCriterionRows.Count
    universal_orchestration_plan_quality_all_adaptive_pairs_passed = $universalOrchestrationPlanQualityAllPassed
    token_success_blocked_by_quality = -not $allAdaptivePairsPassed -or $qualityRegressedAnywhere
}

$heldoutReport = [ordered]@{
    schema_version = "xiigen.phase12.heldout_ablation_no_leakage_report.v1"
    run_id = $RunId
    overall_status = if (@($HeldoutChecks | Where-Object { $_.status -match "FAIL" }).Count -gt 0) { "FAIL" } else { "PASS_WITH_LIMITATIONS" }
    full_training_corpus_disjoint_proven = $false
    semantic_quality_effect_from_ablation_proven = $false
    checks = @($HeldoutChecks.ToArray())
    limitations = @(
        "Held-out prompts are Phase 12 hand-authored universal prompts, but this runner does not scan every training/source example.",
        "Ablation removes requested/available skill/context signals; it does not prove semantic improvement.",
        "No pair id/source path is intentionally passed as runtime authority; file paths remain audit metadata."
    )
}

$failingCriteriaMap = New-Phase12FailingCriteriaMap -QualityScores @($QualityScores.ToArray())
$fixAnalysis = New-Phase12FixAnalysis -FailureMap ([pscustomobject]$failingCriteriaMap) -TokenAccountingReport ([pscustomobject]$tokenAccountingReport) -QualityReport ([pscustomobject]$qualityReport) -HeldoutReport ([pscustomobject]$heldoutReport)
$phase12ProductQualityBlocked = [bool]$failingCriteriaMap.product_quality_blocked
$requiredFixesBeforePass = if ($phase12ProductQualityBlocked) {
    New-Phase12RequiredFixesBeforePass -FailureMap ([pscustomobject]$failingCriteriaMap)
} else {
    $null
}

$reviewPacket = [ordered]@{
    schema_version = "xiigen.phase12.independent_review_packet_draft.v1"
    packet_title = "PHASE_12_INDEPENDENT_TOKEN_QUALITY_REVIEW_PACKET_DRAFT"
    run_id = $RunId
    review_scope = "12.02-12.09 only; parent 12.10 commit/status boundary excluded"
    suggested_review_questions = @(
        "Do token traces compare the same scenario pairs and disclose source limitations?",
        "Does quality scoring block token success when adaptive criteria are only partial?",
        "Do held-out/ablation/no-leakage rows avoid treating pair ids, paths, or candidate position as runtime authority?",
        "Are all non-claims preserved?"
    )
    recommended_verdict = if ($tokenAccountingReport.token_success_accepted -and -not $qualityReport.token_success_blocked_by_quality) { "REVIEW_TOKEN_QUALITY_ACCEPTABLE" } else { "REVIEW_NEEDS_FIX_OR_FUTURE_PROOF" }
}

$tokenSuccessAccepted = [bool]($tokenAccountingReport.token_success_accepted -and -not $qualityReport.token_success_blocked_by_quality)
$remainingQualityHeadline = if ($tokenSuccessAccepted -and $allAdaptivePairsPassed -and $universalOrchestrationPlanQualityAllPassed) {
    "No unresolved adaptive quality blocker remains for this Phase 12 fresh run; token success accepted true; adaptive quality closes all rubric criteria (quality pass $adaptiveQualityPassCount/$adaptiveQualityPairCount); universal_orchestration_plan_quality passes for all $adaptiveUniversalPassCount adaptive pairs."
} else {
    "Adaptive quality did not close all rubric criteria for this run; token success is not accepted while adaptive quality is blocked."
}
$packetTitle = if ($tokenSuccessAccepted) {
    "PHASE_12_TOKEN_QUALITY_READY_FOR_REVIEW"
} elseif ($phase12ProductQualityBlocked) {
    "PHASE_12_BLOCKED_BY_PRODUCT_QUALITY_FIXES"
} else {
    "PHASE_12_NEEDS_FIX"
}
$evidencePaths = [ordered]@{
    run_dir = $RunDir
    scenario_pair_matrix_json = Join-Path $RunDir "scenario-pair-matrix.json"
    baseline_token_traces_json = Join-Path $RunDir "baseline-token-traces.json"
    adaptive_token_traces_json = Join-Path $RunDir "adaptive-token-traces.json"
    ablation_token_traces_json = Join-Path $RunDir "ablation-token-traces.json"
    token_accounting_variance_report_json = Join-Path $RunDir "token-accounting-variance-report.json"
    quality_rubric_json = Join-Path $RunDir "quality-rubric.json"
    quality_scoring_report_json = Join-Path $RunDir "quality-scoring-report.json"
    heldout_ablation_no_leakage_report_json = Join-Path $RunDir "heldout-ablation-no-leakage-report.json"
    phase12_fix_analysis_json = Join-Path $RunDir "phase12-fix-analysis.json"
    failing_quality_criteria_map_json = Join-Path $RunDir "failing-quality-criteria-map.json"
    independent_review_packet_json = Join-Path $RunDir "independent-token-quality-review-packet.json"
    phase12_report_json = Join-Path $RunDir "phase12-cross-host-token-quality-report.json"
}
if ($phase12ProductQualityBlocked) {
    $evidencePaths["phase12_required_fixes_before_pass_json"] = Join-Path $RunDir "phase12-required-fixes-before-pass.json"
}
$finalReport = [ordered]@{
    schema_version = "xiigen.phase12.cross_host_token_quality_report.v1"
    packet_title = $packetTitle
    phase = "12/14"
    steps_closed_for_review = @("12.02", "12.03", "12.04", "12.05", "12.06", "12.07", "12.08", "12.09")
    steps_not_performed = @("12.10", "13", "14")
    run_id = $RunId
    started_utc = $StartedUtc.ToString("o")
    completed_utc = (Get-Date).ToUniversalTime().ToString("o")
    changed_files = @(
        (Get-Phase12RelativePath -Path $RunnerPath),
        (Get-Phase12RelativePath -Path $RunDir)
    )
    evidence_paths = $evidencePaths
    token_quality_headline = [ordered]@{
        token_reduction_observed_pairs = $tokenReductionObservedCount
        token_pair_count = $PairRows.Count
        token_success_accepted = $tokenSuccessAccepted
        quality_regressed_anywhere = $qualityRegressedAnywhere
        adaptive_all_criteria_passed = $allAdaptivePairsPassed
        adaptive_quality_closes_all_rubric_criteria = $allAdaptivePairsPassed
        adaptive_quality_passed_pairs = $adaptiveQualityPassCount
        adaptive_quality_pair_count = $adaptiveQualityPairCount
        adaptive_quality_pass_summary = "$adaptiveQualityPassCount/$adaptiveQualityPairCount"
        universal_orchestration_plan_quality_passed_pairs = $adaptiveUniversalPassCount
        universal_orchestration_plan_quality_pair_count = $adaptiveUniversalCriterionRows.Count
        universal_orchestration_plan_quality_all_adaptive_pairs_passed = $universalOrchestrationPlanQualityAllPassed
        universal_orchestration_plan_quality_pass_summary = "$adaptiveUniversalPassCount/$adaptiveQualityPairCount"
        product_quality_blocked = $phase12ProductQualityBlocked
    }
    russian_headline = [ordered]@{
        what_improved = "In the harness evaluation the adaptive/core trace uses fewer heuristic tokens than the transparent synthetic baseline manual packet, in " + $tokenReductionObservedCount + " of " + $PairRows.Count + " pairs."
        what_did_not_improve = $remainingQualityHeadline
        what_is_not_proven = "Not proven: live chat UI token savings, semantic quality improvement, learnability, training success, checkpoint promotion, full Codex/Claude/Cursor UI readiness or all-host final readiness."
    }
    quality_regressed_anywhere = $qualityRegressedAnywhere
    token_success_accepted = $tokenSuccessAccepted
    token_success_blocked_by_quality = [bool]$qualityReport.token_success_blocked_by_quality
    product_quality_blocked = $phase12ProductQualityBlocked
    no_hidden_deterministic_mechanisms_introduced = $true
    transparent_harness_mechanisms = @(
        "heuristic token estimator",
        "synthetic common-host baseline packet",
        "rule-based quality rubric",
        "adapter/core invocation through installed Codex plugin"
    )
    external_ai_used = $false
    training_started = $false
    checkpoint_mutated = $false
    exact_non_claims = @(
        "No live Claude/Cursor/Codex chat UI token savings are claimed.",
        "No semantic quality improvement is claimed.",
        "No learnability, DPO, training success, checkpoint promotion, or checkpoint mutation is claimed.",
        "No full chat UI or all-host final readiness is claimed.",
        "No status board update, git stage, commit, or push was performed by the runner."
    )
    commands_run_by_runner = @($CommandsRun.ToArray())
    proposed_parent_staging_list = @(
        "43.SmartSkillsDomain/OrchestrationReasoningMachine.cs",
        "43.SmartSkillsDomain/XiigenExtensionApiService.cs",
        "Tests/OrchestrationReasoningStateLoadingTests.cs",
        "Tests/XiigenExtensionApiContractsTests.cs",
        "models/orchestration-reasoning/orchestration-reasoning-v1/manifest.json",
        "models/orchestration-reasoning/orchestration-reasoning-v1/orchestration-reasoning-v1-checkpoint.json",
        "tools/phase12-cross-host-token-quality-runner.ps1",
        ((Get-Phase12RelativePath -Path $RunDir).TrimEnd("/") + "/")
    )
    git_stage_commit_push_performed = $false
}

Write-Phase12Json -Path (Join-Path $RunDir "scenario-pair-matrix.json") -Value $scenarioMatrix
Write-Phase12Text -Path (Join-Path $RunDir "scenario-pair-matrix.md") -Text (New-Phase12MatrixMarkdown -Rows $MatrixRows)
Write-Phase12Json -Path (Join-Path $RunDir "baseline-token-traces.json") -Value ([ordered]@{ schema_version = "xiigen.phase12.baseline_token_traces.v1"; run_id = $RunId; traces = @($BaselineTraces.ToArray()) })
Write-Phase12Json -Path (Join-Path $RunDir "adaptive-token-traces.json") -Value ([ordered]@{ schema_version = "xiigen.phase12.adaptive_token_traces.v1"; run_id = $RunId; traces = @($AdaptiveTraces.ToArray()) })
Write-Phase12Json -Path (Join-Path $RunDir "ablation-token-traces.json") -Value ([ordered]@{ schema_version = "xiigen.phase12.ablation_token_traces.v1"; run_id = $RunId; traces = @($AblationTraces.ToArray()) })
Write-Phase12Json -Path (Join-Path $RunDir "token-accounting-variance-report.json") -Value $tokenAccountingReport
Write-Phase12Text -Path (Join-Path $RunDir "token-accounting-variance-report.md") -Text (New-Phase12TokenReportMarkdown -Report ([pscustomobject]$tokenAccountingReport))
Write-Phase12Json -Path (Join-Path $RunDir "quality-rubric.json") -Value $Rubric
Write-Phase12Text -Path (Join-Path $RunDir "quality-rubric.md") -Text (New-Phase12RubricMarkdown -Rubric ([pscustomobject]$Rubric))
Write-Phase12Json -Path (Join-Path $RunDir "quality-scoring-report.json") -Value $qualityReport
Write-Phase12Text -Path (Join-Path $RunDir "quality-scoring-report.md") -Text (New-Phase12QualityReportMarkdown -Report ([pscustomobject]$qualityReport))
Write-Phase12Json -Path (Join-Path $RunDir "heldout-ablation-no-leakage-report.json") -Value $heldoutReport
Write-Phase12Text -Path (Join-Path $RunDir "heldout-ablation-no-leakage-report.md") -Text (New-Phase12HeldoutMarkdown -Report ([pscustomobject]$heldoutReport))
Write-Phase12Json -Path (Join-Path $RunDir "phase12-fix-analysis.json") -Value $fixAnalysis
Write-Phase12Text -Path (Join-Path $RunDir "phase12-fix-analysis.md") -Text (New-Phase12FixAnalysisMarkdown -Analysis ([pscustomobject]$fixAnalysis))
Write-Phase12Json -Path (Join-Path $RunDir "failing-quality-criteria-map.json") -Value $failingCriteriaMap
Write-Phase12Text -Path (Join-Path $RunDir "failing-quality-criteria-map.md") -Text (New-Phase12FailingCriteriaMapMarkdown -Map ([pscustomobject]$failingCriteriaMap))
if ($phase12ProductQualityBlocked) {
    Write-Phase12Json -Path (Join-Path $RunDir "phase12-required-fixes-before-pass.json") -Value $requiredFixesBeforePass
    Write-Phase12Text -Path (Join-Path $RunDir "phase12-required-fixes-before-pass.md") -Text (New-Phase12RequiredFixesMarkdown -RequiredFixes ([pscustomobject]$requiredFixesBeforePass))
}
Write-Phase12Json -Path (Join-Path $RunDir "independent-token-quality-review-packet.json") -Value $reviewPacket
Write-Phase12Text -Path (Join-Path $RunDir "independent-token-quality-review-packet.md") -Text "# Phase 12 independent token+quality review packet draft`n`nRecommended verdict: $($reviewPacket.recommended_verdict)`n`nScope: $($reviewPacket.review_scope)`n"
Write-Phase12Json -Path (Join-Path $RunDir "phase12-cross-host-token-quality-report.json") -Value $finalReport
Write-Phase12Text -Path (Join-Path $RunDir "phase12-cross-host-token-quality-report.md") -Text (New-Phase12FinalMarkdown -Report ([pscustomobject]$finalReport))

Write-Output (ConvertTo-Phase12Json ([ordered]@{
    packet_title = $packetTitle
    run_id = $RunId
    run_dir = $RunDir
    token_success_accepted = $tokenSuccessAccepted
    quality_regressed_anywhere = $qualityRegressedAnywhere
    product_quality_blocked = $phase12ProductQualityBlocked
    report_json = Join-Path $RunDir "phase12-cross-host-token-quality-report.json"
}))
