param(
    [Parameter(Mandatory = $true)]
    [string]$RequestPath,

    [Parameter(Mandatory = $true)]
    [string]$ResponsePath,

    [Parameter(Mandatory = $true)]
    [string]$TracePath
)

$ErrorActionPreference = "Stop"

function Resolve-Phase40FullPath {
    param([string]$Path)

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return [System.IO.Path]::GetFullPath($Path)
    }

    return [System.IO.Path]::GetFullPath((Join-Path (Get-Location).Path $Path))
}

function Ensure-Phase40Directory {
    param([string]$Path)

    $directory = Split-Path -Parent $Path
    if ($directory -and -not (Test-Path -LiteralPath $directory)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }
}

function Write-Phase40Json {
    param(
        [Parameter(Mandatory = $true)]$Value,
        [Parameter(Mandatory = $true)][string]$Path
    )

    Ensure-Phase40Directory -Path $Path
    $Value | ConvertTo-Json -Depth 32 | Set-Content -LiteralPath $Path -Encoding UTF8
}

function Get-Phase40Property {
    param(
        $Object,
        [string[]]$Names
    )

    if ($null -eq $Object) {
        return $null
    }

    foreach ($name in $Names) {
        $property = $Object.PSObject.Properties | Where-Object { $_.Name -ieq $name } | Select-Object -First 1
        if ($property) {
            return $property.Value
        }
    }

    return $null
}

function To-Phase40Array {
    param($Value)

    if ($null -eq $Value) {
        return @()
    }

    if ($Value -is [System.Array]) {
        return @($Value)
    }

    return @($Value)
}

$resolvedRequestPath = Resolve-Phase40FullPath -Path $RequestPath
$resolvedResponsePath = Resolve-Phase40FullPath -Path $ResponsePath
$resolvedTracePath = Resolve-Phase40FullPath -Path $TracePath
$startedUtc = (Get-Date).ToUniversalTime().ToString("o")
$scriptPath = $MyInvocation.MyCommand.Path

try {
    if (-not (Test-Path -LiteralPath $resolvedRequestPath)) {
        throw "RequestPath does not exist: $resolvedRequestPath"
    }

    $request = Get-Content -Raw -LiteralPath $resolvedRequestPath | ConvertFrom-Json
    $schemaVersion = Get-Phase40Property -Object $request -Names @("schemaVersion", "schema_version")
    $hostKind = Get-Phase40Property -Object $request -Names @("hostKind", "host_kind")
    $skillSelectionRequest = Get-Phase40Property -Object $request -Names @("skillSelectionRequest", "skill_selection_request")
    $promptSuiteRequest = Get-Phase40Property -Object $request -Names @("promptSuiteRequest", "prompt_suite_request")
    $assetReferences = Get-Phase40Property -Object $request -Names @("assetReferences", "asset_references")
    $externalToolEvidence = Get-Phase40Property -Object $request -Names @("externalToolEvidence", "external_tool_evidence", "external_tool_usage_evidence")

    $concreteTask = Get-Phase40Property -Object $request -Names @("concreteTask", "concrete_task")
    if (-not $concreteTask) {
        $concreteTask = Get-Phase40Property -Object $skillSelectionRequest -Names @("concreteTask", "concrete_task")
    }

    $prompt = Get-Phase40Property -Object $skillSelectionRequest -Names @("prompt")
    $checkSetItems = To-Phase40Array (Get-Phase40Property -Object $skillSelectionRequest -Names @("checkSetItems", "check_set_items"))
    if (@($checkSetItems).Count -eq 0) {
        $checkSetItems = To-Phase40Array (Get-Phase40Property -Object $promptSuiteRequest -Names @("checkSetItems", "check_set_items", "prompts", "prompt_rows"))
    }

    $externalAiRequested = [bool](Get-Phase40Property -Object $externalToolEvidence -Names @("externalAiUsed", "external_ai_used", "external_ai_or_unrelated_tool_used"))
    $imageGenerationRequested = [bool](Get-Phase40Property -Object $externalToolEvidence -Names @("imageGenerationUsed", "image_generation_used"))
    $unrelatedToolRequested = [bool](Get-Phase40Property -Object $externalToolEvidence -Names @("unrelatedToolUsed", "unrelated_tool_used"))

    $gaps = @()
    if ($schemaVersion -ne 1) {
        $gaps += "schemaVersion must be 1"
    }
    if (-not $hostKind) {
        $gaps += "hostKind is required"
    }
    if (-not $concreteTask -and -not $prompt) {
        $gaps += "concrete task or prompt is required"
    }
    if ($externalAiRequested -or $imageGenerationRequested -or $unrelatedToolRequested) {
        $gaps += "external AI, image generation, and unrelated tool use are prohibited"
    }

    $results = @()
    $index = 0
    foreach ($item in $checkSetItems) {
        $index++
        $checkId = Get-Phase40Property -Object $item -Names @("checkId", "check_id", "id")
        if (-not $checkId) {
            $checkId = "check-$index"
        }

        $itemTask = Get-Phase40Property -Object $item -Names @("concreteTask", "concrete_task", "task")
        if (-not $itemTask) {
            $itemTask = $concreteTask
        }

        $itemPrompt = Get-Phase40Property -Object $item -Names @("prompt")
        if (-not $itemPrompt) {
            $itemPrompt = $prompt
        }

        $itemPassed = [bool]($checkId -and ($itemTask -or $itemPrompt))
        if (-not $itemPassed) {
            $gaps += "check-set item $index is missing concrete task or prompt"
        }

        $result = [ordered]@{
            checkId = $checkId
            passed = $itemPassed
            concreteTask = $itemTask
            prompt = $itemPrompt
            concreteAnswer = "Phase40 core API executed contract check for $checkId; semantic skill quality is not claimed by this script."
            selectedSkillIds = @()
            resultGapDecision = "contract_check_only"
            completionDecision = "core_api_contract_execution_only"
            contractCheckLabel = "deterministic_contract_check_not_learning"
            semanticQualityClaimed = $false
            decisionEvidence = [ordered]@{
                decisionSourceKind = "CoreCheckSetContract"
                coreDecisionReference = $resolvedResponsePath
                exactOrStaticEvidenceReference = $resolvedRequestPath
                deterministicEvidenceUsedAsLearning = $false
                hostProvidedSemanticDecision = $false
                masqueradeProhibited = $true
                reviewNote = "This row proves core API/check-set invocation only; it does not prove trained semantic quality or full host runtime."
            }
            traceLogReferences = @(
                [ordered]@{
                    traceId = "phase40-core-api-trace"
                    relativePath = $resolvedTracePath
                    description = "Core API command trace for contract/check-set execution"
                    containsPrivateAbsolutePath = $true
                    containsInternalKitchenState = $false
                }
            )
        }
        $results += [pscustomobject]$result
    }

    if (@($checkSetItems).Count -eq 0) {
        $results += [pscustomobject][ordered]@{
            checkId = "request-contract"
            passed = [bool]($concreteTask -or $prompt)
            concreteTask = $concreteTask
            prompt = $prompt
            concreteAnswer = "Phase40 core API executed request-level contract check; semantic skill quality is not claimed by this script."
            selectedSkillIds = @()
            resultGapDecision = "request_contract_check_only"
            completionDecision = "core_api_contract_execution_only"
            contractCheckLabel = "deterministic_contract_check_not_learning"
            semanticQualityClaimed = $false
            decisionEvidence = [ordered]@{
                decisionSourceKind = "CoreCheckSetContract"
                coreDecisionReference = $resolvedResponsePath
                exactOrStaticEvidenceReference = $resolvedRequestPath
                deterministicEvidenceUsedAsLearning = $false
                hostProvidedSemanticDecision = $false
                masqueradeProhibited = $true
                reviewNote = "Request-level fallback proves only schema/API invocation."
            }
            traceLogReferences = @(
                [ordered]@{
                    traceId = "phase40-core-api-trace"
                    relativePath = $resolvedTracePath
                    description = "Core API command trace for request-level contract execution"
                    containsPrivateAbsolutePath = $true
                    containsInternalKitchenState = $false
                }
            )
        }
    }

    $passed = ($gaps.Count -eq 0) -and -not (@($results) | Where-Object { -not $_.passed } | Select-Object -First 1)

    $response = [ordered]@{
        schemaVersion = 1
        contract = "Phase40ExtensionApiResponse"
        hostKind = $hostKind
        passed = [bool]$passed
        runtimeEvidence = $true
        fullHostRuntimeEvidence = $false
        full_host_runtime_evidence = $false
        fullPhase40ImplementationProven = $false
        evidenceScope = "core_api_check_set_runtime_not_full_host"
        validationOnly = $false
        semanticQualityClaimed = $false
        deterministicEvidenceUsedAsLearning = $false
        hostProvidedSemanticDecision = $false
        externalAiUsed = $false
        imageGenerationUsed = $false
        unrelatedToolUsed = $false
        gaps = @($gaps)
        checkSetItems = @($results)
        checkSetResults = @($results)
        promptSuiteResult = [ordered]@{
            promptSuiteId = Get-Phase40Property -Object $promptSuiteRequest -Names @("promptSuiteId", "prompt_suite_id")
            hostKind = $hostKind
            results = @($results)
            contractStatus = [ordered]@{
                evidenceScope = "core_api_check_set_runtime_not_full_host"
                hostRuntimeUnavailable = $false
                fullPhase40ImplementationProven = $false
                contractOnlyDowngradeRequired = $false
                downgradeReason = $null
                nextRequiredAction = "Run true user-facing host runtime evidence row; this core command alone is not full host runtime."
            }
        }
        decisionEvidence = [ordered]@{
            decisionSourceKind = "CoreCheckSetContract"
            coreDecisionReference = $resolvedResponsePath
            exactOrStaticEvidenceReference = $resolvedRequestPath
            deterministicEvidenceUsedAsLearning = $false
            hostProvidedSemanticDecision = $false
            masqueradeProhibited = $true
            reviewNote = "Core-owned command executed concrete check-set contract only; no trained semantic quality or deterministic learning claim."
        }
        externalToolUsageEvidence = [ordered]@{
            externalAiOrUnrelatedToolUsed = $false
            external_ai_or_unrelated_tool_used = $false
            externalAiUsed = $false
            imageGenerationUsed = $false
            unrelatedToolUsed = $false
            toolNames = @()
            explicitApprovalReference = $null
            prohibitionEvidence = "phase40-extension-core-api.ps1 does not call external AI, image generation, network, or unrelated tools."
        }
        assetReferences = $assetReferences
        traceLogReferences = @(
            [ordered]@{
                traceId = "phase40-core-api-trace"
                relativePath = $resolvedTracePath
                description = "Core API command trace"
                containsPrivateAbsolutePath = $true
                containsInternalKitchenState = $false
            }
        )
        phase40Complete = $false
    }

    Write-Phase40Json -Value $response -Path $resolvedResponsePath

    $trace = [ordered]@{
        schemaVersion = 1
        traceKind = "Phase40CoreApiCommandTrace"
        startedUtc = $startedUtc
        completedUtc = (Get-Date).ToUniversalTime().ToString("o")
        requestPath = $resolvedRequestPath
        responsePath = $resolvedResponsePath
        tracePath = $resolvedTracePath
        scriptPath = $scriptPath
        hostKind = $hostKind
        checkSetItemCount = @($results).Count
        runtimeEvidence = $true
        fullHostRuntimeEvidence = $false
        evidenceScope = "core_api_check_set_runtime_not_full_host"
        deterministicEvidenceUsedAsLearning = $false
        hostProvidedSemanticDecision = $false
        semanticQualityClaimed = $false
        externalAiUsed = $false
        imageGenerationUsed = $false
        unrelatedToolUsed = $false
        noOverclaimReview = "PASS: core command proves API/check-set execution only and does not claim trained semantic quality or full host runtime."
        gaps = @($gaps)
        passed = [bool]$passed
    }

    Write-Phase40Json -Value $trace -Path $resolvedTracePath
    if ($passed) {
        exit 0
    }

    exit 1
} catch {
    $message = $_.Exception.Message
    $failure = [ordered]@{
        schemaVersion = 1
        contract = "Phase40ExtensionApiResponse"
        passed = $false
        runtimeEvidence = $false
        fullHostRuntimeEvidence = $false
        evidenceScope = "core_api_check_set_runtime_failed"
        semanticQualityClaimed = $false
        deterministicEvidenceUsedAsLearning = $false
        hostProvidedSemanticDecision = $false
        externalAiUsed = $false
        imageGenerationUsed = $false
        unrelatedToolUsed = $false
        error = $message
        phase40Complete = $false
    }

    Write-Phase40Json -Value $failure -Path $resolvedResponsePath
    Write-Phase40Json -Value ([ordered]@{
        schemaVersion = 1
        traceKind = "Phase40CoreApiCommandTrace"
        startedUtc = $startedUtc
        completedUtc = (Get-Date).ToUniversalTime().ToString("o")
        requestPath = $resolvedRequestPath
        responsePath = $resolvedResponsePath
        tracePath = $resolvedTracePath
        runtimeEvidence = $false
        fullHostRuntimeEvidence = $false
        evidenceScope = "core_api_check_set_runtime_failed"
        semanticQualityClaimed = $false
        deterministicEvidenceUsedAsLearning = $false
        hostProvidedSemanticDecision = $false
        externalAiUsed = $false
        imageGenerationUsed = $false
        unrelatedToolUsed = $false
        error = $message
        passed = $false
    }) -Path $resolvedTracePath
    Write-Error $message
    exit 1
}
