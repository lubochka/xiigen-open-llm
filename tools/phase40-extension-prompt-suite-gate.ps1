param(
    [Parameter(Mandatory = $true)]
    [string]$ClaudeReport,

    [Parameter(Mandatory = $true)]
    [string]$CodexReport,

    [Parameter(Mandatory = $true)]
    [string]$CursorReport,

    [switch]$RequireUserFacingEntrypoint,

    [string]$OutputPath
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"

$script:Gaps = New-Object System.Collections.Generic.List[object]
$script:HostSummaries = New-Object System.Collections.Generic.List[object]

function Add-Gap {
    param(
        [string]$HostName,
        [string]$Code,
        [string]$Message
    )

    $script:Gaps.Add([pscustomobject]@{
        host = $HostName
        code = $Code
        message = $Message
    }) | Out-Null
}

function Get-ReportValue {
    param(
        [object]$Source,
        [string[]]$Names
    )

    if ($null -eq $Source) {
        return $null
    }

    foreach ($name in $Names) {
        $property = $Source.PSObject.Properties | Where-Object { $_.Name -ieq $name } | Select-Object -First 1
        if ($null -ne $property) {
            return $property.Value
        }
    }

    return $null
}

function Convert-ToGateBool {
    param([object]$Value)

    if ($null -eq $Value) {
        return $null
    }

    if ($Value -is [bool]) {
        return $Value
    }

    $text = [string]$Value
    if ($text -ieq "true") {
        return $true
    }

    if ($text -ieq "false") {
        return $false
    }

    return $null
}

function Test-RequiredText {
    param(
        [string]$HostName,
        [object]$Source,
        [string[]]$Names,
        [string]$Code,
        [string]$Message
    )

    $value = Get-ReportValue -Source $Source -Names $Names
    if ([string]::IsNullOrWhiteSpace([string]$value)) {
        Add-Gap -HostName $HostName -Code $Code -Message $Message
        return $null
    }

    return [string]$value
}

function Test-RequiredTrue {
    param(
        [string]$HostName,
        [object]$Source,
        [string[]]$Names,
        [string]$Code,
        [string]$Message
    )

    $value = Convert-ToGateBool -Value (Get-ReportValue -Source $Source -Names $Names)
    if ($true -ne $value) {
        Add-Gap -HostName $HostName -Code $Code -Message $Message
    }
}

function Test-RequiredFalse {
    param(
        [string]$HostName,
        [object]$Source,
        [string[]]$Names,
        [string]$Code,
        [string]$Message
    )

    $value = Convert-ToGateBool -Value (Get-ReportValue -Source $Source -Names $Names)
    if ($false -ne $value) {
        Add-Gap -HostName $HostName -Code $Code -Message $Message
    }
}

function Test-RequiredCollection {
    param(
        [string]$HostName,
        [object]$Source,
        [string[]]$Names,
        [string]$Code,
        [string]$Message
    )

    $value = Get-ReportValue -Source $Source -Names $Names
    if ($null -eq $value) {
        Add-Gap -HostName $HostName -Code $Code -Message $Message
        return @()
    }

    if ($value -is [string]) {
        if ([string]::IsNullOrWhiteSpace($value)) {
            Add-Gap -HostName $HostName -Code $Code -Message $Message
            return @()
        }

        return @($value)
    }

    $items = @($value)
    if ($items.Count -eq 0) {
        Add-Gap -HostName $HostName -Code $Code -Message $Message
    }

    return $items
}

function Read-Phase40Report {
    param(
        [string]$HostName,
        [string]$Path
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        Add-Gap -HostName $HostName -Code "report_missing" -Message "Report path does not exist: $Path"
        return $null
    }

    $raw = Get-Content -Raw -LiteralPath $Path
    if ([string]::IsNullOrWhiteSpace($raw)) {
        Add-Gap -HostName $HostName -Code "report_empty" -Message "Report is empty: $Path"
        return $null
    }

    try {
        $json = $raw | ConvertFrom-Json
    }
    catch {
        Add-Gap -HostName $HostName -Code "report_not_json" -Message "Report is not valid JSON: $Path"
        return $null
    }

    return [pscustomobject]@{
        Json = $json
        Raw = $raw
        Path = $Path
    }
}

function Test-EntrypointEvidence {
    param(
        [string]$HostName,
        [object]$Report,
        [string]$RawText
    )

    $entrypoint = Get-ReportValue -Source $Report -Names @("user_facing_entrypoint", "userFacingEntrypoint", "entrypoint", "entrypoint_evidence", "entrypointEvidence")
    if ($null -eq $entrypoint) {
        if ($RequireUserFacingEntrypoint.IsPresent) {
            Add-Gap -HostName $HostName -Code "entrypoint_missing" -Message "User-facing entrypoint evidence is required."
        }

        return
    }

    if ($entrypoint -is [string]) {
        if ([string]::IsNullOrWhiteSpace($entrypoint)) {
            Add-Gap -HostName $HostName -Code "entrypoint_empty" -Message "User-facing entrypoint evidence is blank."
        }
        else {
            Add-Gap -HostName $HostName -Code "entrypoint_string_not_structured" -Message "User-facing entrypoint evidence must be structured and include host-facing type, command/hook/chat/panel/adapter name, and invocation evidence."
        }
    }
    else {
        Test-RequiredText -HostName $HostName -Source $entrypoint -Names @("type", "entrypoint_type", "entrypointType") -Code "entrypoint_type_missing" -Message "Entrypoint evidence must name the host-facing type." | Out-Null
        Test-RequiredText -HostName $HostName -Source $entrypoint -Names @("name", "command", "hook", "chat", "panel", "adapter", "entrypoint_name", "entrypointName") -Code "entrypoint_name_missing" -Message "Entrypoint evidence must name the command, hook, chat, panel, or adapter." | Out-Null
        Test-RequiredText -HostName $HostName -Source $entrypoint -Names @("invocation_evidence", "invocationEvidence", "host_invocation_evidence", "hostInvocationEvidence") -Code "entrypoint_invocation_missing" -Message "Entrypoint evidence must prove invocation through the user-facing host path." | Out-Null
    }

    if ($RawText -match '(?i)"(internal_helper_only|internalHelperOnly)"\s*:\s*true') {
        Add-Gap -HostName $HostName -Code "internal_helper_only" -Message "Internal-helper-only invocation cannot satisfy Phase40 user-facing extension evidence."
    }
}

function Test-ContractBoundary {
    param(
        [string]$HostName,
        [object]$Report
    )

    $contract = Get-ReportValue -Source $Report -Names @("contract_status", "contractStatus", "status")
    if ($null -eq $contract) {
        Add-Gap -HostName $HostName -Code "contract_status_missing" -Message "Contract status with evidence scope and downgrade fields is required."
        return
    }

    $scope = Get-ReportValue -Source $contract -Names @("evidence_scope", "evidenceScope", "scope")
    if ([string]::IsNullOrWhiteSpace([string]$scope)) {
        $scope = Get-ReportValue -Source $Report -Names @("evidence_scope", "evidenceScope", "scope")
    }

    if ([string]::IsNullOrWhiteSpace([string]$scope)) {
        Add-Gap -HostName $HostName -Code "evidence_scope_missing" -Message "Evidence scope is required."
    }

    $full = Convert-ToGateBool -Value (Get-ReportValue -Source $contract -Names @("full_phase40_implementation_proven", "fullPhase40ImplementationProven", "full_implementation_proven", "fullImplementationProven"))
    if ($null -eq $full) {
        $full = Convert-ToGateBool -Value (Get-ReportValue -Source $Report -Names @("full_phase40_implementation_proven", "fullPhase40ImplementationProven", "full_implementation_proven", "fullImplementationProven", "full_host_implementation_evidence"))
    }

    $runtimeUnavailable = Convert-ToGateBool -Value (Get-ReportValue -Source $contract -Names @("host_runtime_unavailable", "hostRuntimeUnavailable"))
    if ($null -eq $runtimeUnavailable) {
        $runtimeUnavailable = Convert-ToGateBool -Value (Get-ReportValue -Source $Report -Names @("host_runtime_unavailable", "hostRuntimeUnavailable", "product_decision_required", "productDecisionRequired"))
    }

    $contractOnly = Convert-ToGateBool -Value (Get-ReportValue -Source $contract -Names @("contract_only_downgrade_required", "contractOnlyDowngradeRequired"))
    if ($null -eq $contractOnly) {
        $contractOnly = Convert-ToGateBool -Value (Get-ReportValue -Source $Report -Names @("contract_only_downgrade_required", "contractOnlyDowngradeRequired"))
    }

    $staticScope = ($scope -match '(?i)package_static|adapter_contract|contract_only|static|PackageStaticOrAdapterContractOnly')

    if ($true -eq $full -and ($staticScope -or $true -eq $runtimeUnavailable -or $true -eq $contractOnly)) {
        Add-Gap -HostName $HostName -Code "full_claim_from_contract_only" -Message "Full host implementation is claimed from static, contract-only, unavailable-runtime, or downgraded evidence."
    }

    if ($true -eq $runtimeUnavailable) {
        Test-RequiredText -HostName $HostName -Source $contract -Names @("next_required_action", "nextRequiredAction", "next_action", "nextAction") -Code "runtime_unavailable_next_action_missing" -Message "Runtime-unavailable evidence must name the next host-runtime work order." | Out-Null
    }
}

function Test-ExternalToolEvidence {
    param(
        [string]$HostName,
        [object]$Report,
        [string]$RawText
    )

    $evidence = Get-ReportValue -Source $Report -Names @("external_tool_usage", "externalToolUsage", "external_tool_usage_evidence", "externalToolUsageEvidence")
    if ($null -eq $evidence) {
        Add-Gap -HostName $HostName -Code "external_tool_evidence_missing" -Message "External-tool and AI prohibition evidence is required."
        return
    }

    Test-RequiredText -HostName $HostName -Source $evidence -Names @("prohibition_evidence", "prohibitionEvidence", "external_tool_prohibition_evidence", "externalToolProhibitionEvidence") -Code "external_tool_prohibition_missing" -Message "Report must include evidence that external AI, image generation, and unrelated tool use were prohibited." | Out-Null

    $externalUsed = Convert-ToGateBool -Value (Get-ReportValue -Source $evidence -Names @("external_ai_or_unrelated_tool_used", "externalAiOrUnrelatedToolUsed", "external_ai_used", "externalAiUsed", "unrelated_tool_used", "unrelatedToolUsed", "image_generation_used", "imageGenerationUsed"))
    $approval = Get-ReportValue -Source $evidence -Names @("explicit_approval_reference", "explicitApprovalReference", "approval_reference", "approvalReference")
    $strongToolMention = ($RawText -match '(?i)OpenAI|Anthropic|stable diffusion|image-generation-run|Invoke-RestMethod|Invoke-WebRequest|external_ai_or_unrelated_tool_used"\s*:\s*true')

    if (($true -eq $externalUsed -or $strongToolMention) -and [string]::IsNullOrWhiteSpace([string]$approval)) {
        Add-Gap -HostName $HostName -Code "external_tool_without_approval" -Message "External AI, image generation, or unrelated tool use appears without explicit Luba approval reference."
    }
}

function Test-AssetSeparation {
    param(
        [string]$HostName,
        [object]$Report
    )

    $assets = Get-ReportValue -Source $Report -Names @("asset_references", "assetReferences", "common_adaptive_assets", "commonAdaptiveAssets")
    if ($null -eq $assets) {
        Add-Gap -HostName $HostName -Code "asset_references_missing" -Message "Common/adaptive asset reference separation is required."
        return
    }

    Test-RequiredText -HostName $HostName -Source $assets -Names @("common_model_manifest_ref", "commonModelManifestRef", "common_model_manifest", "commonModelManifest") -Code "common_model_manifest_missing" -Message "Common model manifest reference is required." | Out-Null
    Test-RequiredText -HostName $HostName -Source $assets -Names @("common_training_data_manifest_ref", "commonTrainingDataManifestRef", "common_training_manifest", "commonTrainingManifest") -Code "common_training_manifest_missing" -Message "Common training-data manifest or hashed pointer reference is required." | Out-Null
    Test-RequiredText -HostName $HostName -Source $assets -Names @("adaptive_state_root_ref", "adaptiveStateRootRef", "project_local_state_ref", "projectLocalStateRef") -Code "adaptive_state_ref_missing" -Message "Adaptive user/project state reference is required and must stay separate from host packages." | Out-Null
    Test-RequiredTrue -HostName $HostName -Source $assets -Names @("common_assets_repo_owned", "commonAssetsRepoOwned") -Code "common_assets_not_repo_owned" -Message "Common trained assets must be repo-owned."
    Test-RequiredTrue -HostName $HostName -Source $assets -Names @("adaptive_state_separated_from_host_package", "adaptiveStateSeparatedFromHostPackage") -Code "adaptive_state_not_separated" -Message "Adaptive state must be separated from the host package."
}

function Test-DecisionEvidence {
    param(
        [string]$HostName,
        [object]$Source,
        [string]$CodePrefix
    )

    $decision = Get-ReportValue -Source $Source -Names @("decision_evidence", "decisionEvidence", "core_decision_evidence", "coreDecisionEvidence")
    if ($null -eq $decision) {
        Add-Gap -HostName $HostName -Code "$CodePrefix`_decision_evidence_missing" -Message "Core decision evidence is required and must separate trainable/core decisions from static evidence."
        return
    }

    $sourceKind = Test-RequiredText -HostName $HostName -Source $decision -Names @("decision_source_kind", "decisionSourceKind", "source_kind", "sourceKind") -Code "$CodePrefix`_decision_source_missing" -Message "Decision source kind is required."
    Test-RequiredText -HostName $HostName -Source $decision -Names @("core_decision_reference", "coreDecisionReference", "core_request_response_ref", "coreRequestResponseRef") -Code "$CodePrefix`_core_decision_ref_missing" -Message "Core decision reference is required." | Out-Null
    Test-RequiredFalse -HostName $HostName -Source $decision -Names @("deterministic_evidence_used_as_learning", "deterministicEvidenceUsedAsLearning") -Code "$CodePrefix`_deterministic_as_learning" -Message "Deterministic or exact/static evidence must not be reported as learning."
    Test-RequiredFalse -HostName $HostName -Source $decision -Names @("host_provided_semantic_decision", "hostProvidedSemanticDecision") -Code "$CodePrefix`_host_semantic_decision" -Message "Host package must not provide semantic decision authority."
    Test-RequiredTrue -HostName $HostName -Source $decision -Names @("masquerade_prohibited", "masqueradeProhibited") -Code "$CodePrefix`_masquerade_not_prohibited" -Message "Report must explicitly prohibit deterministic masquerade."

    if ($sourceKind -match '(?i)ExactOrStaticEvidenceOnly|HostProvidedEvidenceOnly|exact|static|host') {
        Add-Gap -HostName $HostName -Code "$CodePrefix`_non_core_decision_source" -Message "Decision source is exact/static or host-provided instead of core trainable/check-set evidence."
    }
}

function Test-PromptRows {
    param(
        [string]$HostName,
        [object]$Report
    )

    $rows = Test-RequiredCollection -HostName $HostName -Source $Report -Names @("prompt_rows", "promptRows", "prompts", "check_set_results", "checkSetResults", "results") -Code "prompt_rows_missing" -Message "Prompt-suite/check-set rows are required."
    $index = 0

    foreach ($row in $rows) {
        $index += 1
        if ($row -is [string]) {
            if ([string]::IsNullOrWhiteSpace($row)) {
                Add-Gap -HostName $HostName -Code "prompt_$index`_blank" -Message "Prompt row $index is blank."
            }
            else {
                Add-Gap -HostName $HostName -Code "prompt_$index`_string_row_not_structured" -Message "Prompt row $index must be structured and include concrete task, concrete answer/check result, core request/response refs, trace/log refs, and decision evidence."
            }

            continue
        }

        Test-RequiredText -HostName $HostName -Source $row -Names @("concrete_task", "concreteTask", "task") -Code "prompt_$index`_task_missing" -Message "Prompt row $index must include the concrete task." | Out-Null
        Test-RequiredText -HostName $HostName -Source $row -Names @("concrete_answer", "concreteAnswer", "answer", "check_result", "checkResult") -Code "prompt_$index`_answer_missing" -Message "Prompt row $index must include the concrete answer or check result." | Out-Null
        Test-RequiredText -HostName $HostName -Source $row -Names @("core_request_ref", "coreRequestRef", "core_request_payload_ref", "coreRequestPayloadRef", "coreRequestPayloadReference") -Code "prompt_$index`_core_request_ref_missing" -Message "Prompt row $index must include the core request payload reference." | Out-Null
        Test-RequiredText -HostName $HostName -Source $row -Names @("core_response_ref", "coreResponseRef", "core_response_payload_ref", "coreResponsePayloadRef", "coreResponsePayloadReference") -Code "prompt_$index`_core_response_ref_missing" -Message "Prompt row $index must include the core response payload reference." | Out-Null
        Test-RequiredCollection -HostName $HostName -Source $row -Names @("trace_log_references", "traceLogReferences", "trace_refs", "traceRefs", "logs", "traces") -Code "prompt_$index`_trace_refs_missing" -Message "Prompt row $index must include trace/log references." | Out-Null
        Test-DecisionEvidence -HostName $HostName -Source $row -CodePrefix "prompt_$index"

        $answer = Get-ReportValue -Source $row -Names @("concrete_answer", "concreteAnswer", "answer", "check_result", "checkResult")
        $answerText = $answer | ConvertTo-Json -Depth 8 -Compress
        if ($answerText -match '(?i)internal_kitchen|training_state|private_corpus|TrainBatch|DpoOptimizer|checkpoint_state|model_weights') {
            Add-Gap -HostName $HostName -Code "prompt_$index`_kitchen_answer_surface" -Message "Prompt row $index exposes internal kitchen/training state instead of concrete answer/check-set refs."
        }
    }
}

function Test-HostReport {
    param(
        [string]$HostName,
        [string]$Path
    )

    $loaded = Read-Phase40Report -HostName $HostName -Path $Path
    if ($null -eq $loaded) {
        return
    }

    $report = $loaded.Json
    $raw = $loaded.Raw

    Test-RequiredText -HostName $HostName -Source $report -Names @("host_kind", "hostKind", "host") -Code "host_kind_missing" -Message "Host kind is required." | Out-Null
    Test-RequiredText -HostName $HostName -Source $report -Names @("host_repository", "hostRepository", "host_repository_root", "hostRepositoryRoot") -Code "host_repository_missing" -Message "Host repository/root reference is required." | Out-Null
    Test-RequiredText -HostName $HostName -Source $report -Names @("package_version", "packageVersion", "version") -Code "package_version_missing" -Message "Package version or manifest version is required." | Out-Null
    Test-RequiredText -HostName $HostName -Source $report -Names @("core_request_ref", "coreRequestRef", "core_request_payload_ref", "coreRequestPayloadRef") -Code "core_request_ref_missing" -Message "Report-level core request reference is required." | Out-Null
    Test-RequiredText -HostName $HostName -Source $report -Names @("core_response_ref", "coreResponseRef", "core_response_payload_ref", "coreResponsePayloadRef") -Code "core_response_ref_missing" -Message "Report-level core response reference is required." | Out-Null
    Test-RequiredCollection -HostName $HostName -Source $report -Names @("trace_log_references", "traceLogReferences", "trace_refs", "traceRefs", "logs", "traces") -Code "trace_refs_missing" -Message "Report-level logs/traces are required." | Out-Null

    Test-EntrypointEvidence -HostName $HostName -Report $report -RawText $raw
    Test-ContractBoundary -HostName $HostName -Report $report
    Test-ExternalToolEvidence -HostName $HostName -Report $report -RawText $raw
    Test-AssetSeparation -HostName $HostName -Report $report
    Test-DecisionEvidence -HostName $HostName -Source $report -CodePrefix "report"
    Test-PromptRows -HostName $HostName -Report $report

    if ($raw -match '(?i)"contains_internal_kitchen_state"\s*:\s*true|"containsInternalKitchenState"\s*:\s*true') {
        Add-Gap -HostName $HostName -Code "trace_contains_internal_kitchen_state" -Message "Trace evidence says it contains internal kitchen state."
    }

    $contract = Get-ReportValue -Source $report -Names @("contract_status", "contractStatus", "status")
    $scope = Get-ReportValue -Source $contract -Names @("evidence_scope", "evidenceScope", "scope")
    if ([string]::IsNullOrWhiteSpace([string]$scope)) {
        $scope = Get-ReportValue -Source $report -Names @("evidence_scope", "evidenceScope", "scope")
    }

    $runtimeEvidence = Convert-ToGateBool -Value (Get-ReportValue -Source $report -Names @("runtimeEvidence", "runtime_evidence", "runtime_evidence_proven", "runtimeEvidenceProven"))
    if ($null -eq $runtimeEvidence) {
        $runtimeEvidence = Convert-ToGateBool -Value (Get-ReportValue -Source $contract -Names @("runtimeEvidence", "runtime_evidence", "runtime_evidence_proven", "runtimeEvidenceProven"))
    }

    $fullImplementation = Convert-ToGateBool -Value (Get-ReportValue -Source $report -Names @("full_phase40_implementation_proven", "fullPhase40ImplementationProven", "full_implementation_proven", "fullImplementationProven"))
    if ($null -eq $fullImplementation) {
        $fullImplementation = Convert-ToGateBool -Value (Get-ReportValue -Source $contract -Names @("full_phase40_implementation_proven", "fullPhase40ImplementationProven", "full_implementation_proven", "fullImplementationProven"))
    }

    $fullHostEvidence = Convert-ToGateBool -Value (Get-ReportValue -Source $report -Names @("full_host_implementation_evidence", "fullHostImplementationEvidence"))
    $phase40Complete = Convert-ToGateBool -Value (Get-ReportValue -Source $report -Names @("phase40_complete", "phase40Complete"))
    $contractOnly = Convert-ToGateBool -Value (Get-ReportValue -Source $contract -Names @("contract_only_downgrade_required", "contractOnlyDowngradeRequired"))
    if ($null -eq $contractOnly) {
        $contractOnly = Convert-ToGateBool -Value (Get-ReportValue -Source $report -Names @("contract_only_downgrade_required", "contractOnlyDowngradeRequired"))
    }

    $runtimeRejected = ($scope -match '(?i)package_static|adapter_contract|contract_only|static|runtime_unknown|host_runtime_unknown|PackageStaticOrAdapterContractOnly') -or ($true -eq $contractOnly) -or ($false -eq $runtimeEvidence) -or ($false -eq $fullImplementation) -or ($false -eq $fullHostEvidence) -or ($false -eq $phase40Complete)

    $script:HostSummaries.Add([pscustomobject]@{
        host = $HostName
        report_path = $Path
        inspected = $true
        evidence_scope = [string]$scope
        runtimeEvidence = $runtimeEvidence
        fullPhase40ImplementationProven = $fullImplementation
        full_host_implementation_evidence = $fullHostEvidence
        phase40_complete = $phase40Complete
        contractOnlyDowngradeRequired = $contractOnly
        runtime_completion_rejected = $runtimeRejected
    }) | Out-Null
}

Test-HostReport -HostName "Claude" -Path $ClaudeReport
Test-HostReport -HostName "Codex" -Path $CodexReport
Test-HostReport -HostName "Cursor" -Path $CursorReport

$result = [pscustomobject]@{
    passed = ($script:Gaps.Count -eq 0)
    validation_only = $true
    metric_type = "static_contract_phase40_extension_prompt_suite_report_gate"
    gate_scope = "cross_host_prompt_suite_report_field_validation"
    script_invoked_host_sdks = $false
    external_ai_invoked = $false
    require_user_facing_entrypoint = [bool]$RequireUserFacingEntrypoint
    hosts_covered = @("Claude", "Codex", "Cursor")
    runtime_completion_rejected = (@($script:HostSummaries.ToArray() | Where-Object { $_.runtime_completion_rejected }).Count -gt 0)
    contract_only_evidence_present = (@($script:HostSummaries.ToArray() | Where-Object { $_.runtime_completion_rejected -and $_.evidence_scope -match '(?i)contract_only|static|runtime_unknown|host_runtime_unknown' }).Count -gt 0)
    internal_helper_only_evidence_rejected = (@($script:Gaps.ToArray() | Where-Object { $_.code -eq "internal_helper_only" }).Count -gt 0)
    full_host_runtime = $false
    phase40_complete = $false
    host_reports = $script:HostSummaries.ToArray()
    gaps = $script:Gaps.ToArray()
}

$jsonOutput = $result | ConvertTo-Json -Depth 12
if (-not [string]::IsNullOrWhiteSpace($OutputPath)) {
    $outputDirectory = Split-Path -Parent $OutputPath
    if (-not [string]::IsNullOrWhiteSpace($outputDirectory) -and -not (Test-Path -LiteralPath $outputDirectory)) {
        New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
    }

    Set-Content -LiteralPath $OutputPath -Value $jsonOutput -Encoding UTF8
}

Write-Output $jsonOutput

if ($script:Gaps.Count -gt 0) {
    exit 1
}

exit 0

