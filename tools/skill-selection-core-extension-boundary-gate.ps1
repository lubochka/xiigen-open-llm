param(
    [Parameter(Mandatory = $true)]
    [string]$CoreRoot,

    [string]$ClaudeRepoRoot,

    [string]$CodexRepoRoot,

    [string]$CursorRepoRoot,

    [string]$CoreContractFile,

    [switch]$RequireApiCheckSetContract,

    [switch]$ForbidHostPackageCodeInCore,

    [switch]$RequireImplementationFiles,

    [switch]$ForbidHostSemanticKitchen,

    [switch]$ForbidExternalAiOrUnrelatedToolUse,

    [switch]$ForbidDeterministicMasquerade,

    [switch]$RequireCommonAdaptiveSplit
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version 2.0

trap {
    $failure = [ordered]@{
        passed = $false
        error = $_.Exception.Message
        script_stack_trace = $_.ScriptStackTrace
        position = $_.InvocationInfo.PositionMessage
    }
    $failure | ConvertTo-Json -Depth 8
    exit 1
}

function Resolve-ExistingDirectory {
    param([string]$PathValue)

    if ([string]::IsNullOrWhiteSpace($PathValue)) {
        return $null
    }

    if (-not (Test-Path -LiteralPath $PathValue -PathType Container)) {
        return $null
    }

    return (Resolve-Path -LiteralPath $PathValue).Path
}

function Normalize-PathForCompare {
    param([string]$PathValue)

    if ([string]::IsNullOrWhiteSpace($PathValue)) {
        return ""
    }

    $slash = [char]92
    return ($PathValue.TrimEnd($slash) + '\').ToLowerInvariant()
}

function Test-IsOutsideCore {
    param(
        [string]$HostRoot,
        [string]$ResolvedCoreRoot
    )

    $coreWithSlash = Normalize-PathForCompare -PathValue $ResolvedCoreRoot
    $hostWithSlash = Normalize-PathForCompare -PathValue $HostRoot
    return -not $hostWithSlash.StartsWith($coreWithSlash)
}

function Get-ScannableFiles {
    param([string]$Root)

    $excludedDirectories = @('\.git\', '\node_modules\', '\bin\', '\obj\', '\.vs\', '\dist\', '\out\', '\TestResults\')
    $excludedFileNames = @('smoke-report.json', 'codex-trace-report.json')

    return Get-ChildItem -LiteralPath $Root -Recurse -File -Force -ErrorAction SilentlyContinue |
        Where-Object {
            $fullName = $_.FullName
            $isExcludedDirectory = $false
            foreach ($excluded in $excludedDirectories) {
                if ($fullName.ToLowerInvariant().Contains($excluded.ToLowerInvariant())) {
                    $isExcludedDirectory = $true
                    break
                }
            }

            -not $isExcludedDirectory -and
            -not ($excludedFileNames -contains $_.Name.ToLowerInvariant())
        }
}

function Get-ForbiddenMatches {
    param(
        [string]$Root,
        [string[]]$ForbiddenPatterns
    )

    $foundMatches = @()
    foreach ($file in (Get-ScannableFiles -Root $Root)) {
        $text = ""
        try {
            $text = Get-Content -Raw -LiteralPath $file.FullName -ErrorAction Stop
        }
        catch {
            continue
        }

        foreach ($pattern in $ForbiddenPatterns) {
            if ($text.ToLowerInvariant().Contains($pattern.ToLowerInvariant())) {
                $foundMatches += [ordered]@{
                    file = $file.FullName
                    pattern = $pattern
                }
            }
        }
    }

    return @($foundMatches)
}

function Test-CoreContract {
    param(
        [string]$ResolvedCoreRoot,
        [string]$ContractPath
    )

    $effectivePath = $ContractPath
    if ([string]::IsNullOrWhiteSpace($effectivePath)) {
        $effectivePath = Join-Path $ResolvedCoreRoot '43.SmartSkillsDomain\Phase40ExtensionContracts.cs'
    }

    $resolvedContract = $null
    if (Test-Path -LiteralPath $effectivePath -PathType Leaf) {
        $resolvedContract = (Resolve-Path -LiteralPath $effectivePath).Path
    }

    $requiredTokens = @(
        'Phase40ExtensionHostKind',
        'Phase40EvidenceScope',
        'Phase40CoreDecisionSourceKind',
        'Phase40ExtensionApiRequest',
        'SkillSelectionResponse',
        'Phase40PromptSuiteRequest',
        'Phase40PromptSuiteResult',
        'Phase40CheckSetItem',
        'Phase40CheckSetResult',
        'Phase40TraceLogReference',
        'Phase40ContractStatus',
        'Phase40ExternalToolUsageEvidence',
        'Phase40AssetReferenceSet',
        'Phase40CoreDecisionEvidence',
        'DeterministicEvidenceUsedAsLearning',
        'HostProvidedSemanticDecision',
        'MasqueradeProhibited',
        'CommonAssetsRepoOwned',
        'AdaptiveStateSeparatedFromHostPackage'
    )

    if ($null -eq $resolvedContract) {
        return [ordered]@{
            path = $effectivePath
            exists = $false
            required_tokens = $requiredTokens
            missing_tokens = $requiredTokens
            passed = $false
        }
    }

    $text = Get-Content -Raw -LiteralPath $resolvedContract
    $missing = @()
    foreach ($token in $requiredTokens) {
        if (-not $text.Contains($token)) {
            $missing += $token
        }
    }

    return [ordered]@{
        path = $resolvedContract
        exists = $true
        required_tokens = $requiredTokens
        missing_tokens = $missing
        passed = ($missing.Count -eq 0)
    }
}

function Test-CoreHostPackageCode {
    param([string]$ResolvedCoreRoot)

    $forbiddenCorePackagePatterns = @(
        'claude-xiigen-smart-skills',
        'codex-xiigen-smart-skills',
        'cursor-xiigen-smart-skills',
        '.claude-plugin',
        'claude-entrypoint',
        'codex-entrypoint',
        'cursor-entrypoint',
        'vscode.commands.registerCommand',
        'activationEvents',
        'contributes',
        'package.nls.json'
    )

    $corePackageDirectoryNames = @(
        'claude-xiigen-smart-skills',
        'codex-xiigen-smart-skills',
        'cursor-xiigen-smart-skills',
        '.claude-plugin'
    )

    $directoryMatches = @()
    foreach ($name in $corePackageDirectoryNames) {
        $matches = @(Get-ChildItem -LiteralPath $ResolvedCoreRoot -Recurse -Directory -Force -ErrorAction SilentlyContinue | Where-Object { $_.Name -ieq $name })
        foreach ($match in $matches) {
            $directoryMatches += [ordered]@{
                directory = $match.FullName
                pattern = $name
            }
        }
    }

    $contentMatches = @(Get-ForbiddenMatches -Root $ResolvedCoreRoot -ForbiddenPatterns $forbiddenCorePackagePatterns |
        Where-Object { $_.file -notmatch '\\docs\\plans\\ORCHESTRATION-MACHINE-TRAINING-PLAN-v20\.md$' })

    return [ordered]@{
        forbidden_patterns = $forbiddenCorePackagePatterns
        directory_matches = $directoryMatches
        content_matches = $contentMatches
        passed = ($directoryMatches.Count -eq 0 -and $contentMatches.Count -eq 0)
    }
}

function Get-HostEvidence {
    param(
        [string]$Name,
        [string]$PathValue,
        [string]$ResolvedCoreRoot,
        [string[]]$SemanticKitchenPatterns,
        [string[]]$ExternalToolPatterns,
        [string[]]$DeterministicMasqueradePatterns,
        [string[]]$LegacyForbiddenPatterns
    )

    $resolved = Resolve-ExistingDirectory -PathValue $PathValue
    if ($null -eq $resolved) {
        return [ordered]@{
            host = $Name
            path = $PathValue
            exists = $false
            outside_core_root = $false
            file_count = 0
            scanned_file_count = 0
            semantic_kitchen_matches = @()
            external_tool_matches = @()
            deterministic_masquerade_matches = @()
            legacy_forbidden_matches = @()
            has_implementation_files = $false
            passed_legacy_boundary = $false
            passed_phase40_boundary = $false
        }
    }

    $files = @(Get-ScannableFiles -Root $resolved)
    $semanticMatches = @(Get-ForbiddenMatches -Root $resolved -ForbiddenPatterns $SemanticKitchenPatterns)
    $externalMatches = @(Get-ForbiddenMatches -Root $resolved -ForbiddenPatterns $ExternalToolPatterns)
    $deterministicMatches = @(Get-ForbiddenMatches -Root $resolved -ForbiddenPatterns $DeterministicMasqueradePatterns)
    $legacyMatches = @(Get-ForbiddenMatches -Root $resolved -ForbiddenPatterns $LegacyForbiddenPatterns)
    $outsideCore = Test-IsOutsideCore -HostRoot $resolved -ResolvedCoreRoot $ResolvedCoreRoot

    return [ordered]@{
        host = $Name
        path = $resolved
        exists = $true
        outside_core_root = $outsideCore
        file_count = @(Get-ChildItem -LiteralPath $resolved -Recurse -File -Force -ErrorAction SilentlyContinue).Count
        scanned_file_count = $files.Count
        semantic_kitchen_matches = $semanticMatches
        external_tool_matches = $externalMatches
        deterministic_masquerade_matches = $deterministicMatches
        legacy_forbidden_matches = $legacyMatches
        has_implementation_files = ($files.Count -gt 0)
        passed_legacy_boundary = ($outsideCore -and $legacyMatches.Count -eq 0)
        passed_phase40_boundary = ($outsideCore -and $semanticMatches.Count -eq 0 -and $externalMatches.Count -eq 0 -and $deterministicMatches.Count -eq 0)
    }
}

$resolvedCoreRoot = Resolve-ExistingDirectory -PathValue $CoreRoot
if ($null -eq $resolvedCoreRoot) {
    $report = [ordered]@{
        passed = $false
        error = "CoreRoot not found."
        core_root = $CoreRoot
    }
    $report | ConvertTo-Json -Depth 8
    exit 1
}

$phase40Mode = $RequireApiCheckSetContract.IsPresent -or
    $ForbidHostPackageCodeInCore.IsPresent -or
    $RequireImplementationFiles.IsPresent -or
    $ForbidHostSemanticKitchen.IsPresent -or
    $ForbidExternalAiOrUnrelatedToolUse.IsPresent -or
    $ForbidDeterministicMasquerade.IsPresent -or
    $RequireCommonAdaptiveSplit.IsPresent -or
    -not [string]::IsNullOrWhiteSpace($CoreContractFile)

$legacyForbiddenPatterns = @(
    'TrainBatchAsync',
    'TrainMachineBatchAsync',
    'ILearningMachine',
    'IMachineTraining',
    'DPO',
    'Direct Preference',
    'private-corpora',
    'private corpus',
    'orchestration-pilot-private',
    'skill-selection-corpus.jsonl',
    'canonical skill library',
    'skills-inventory-universal',
    'ProjectLocalSkillState',
    '.xiigen\smart-skills',
    'ServerSkillLibraryRegistry',
    'SkillSelectionMachine',
    'SmartSkillsSemanticUnitCatalog',
    'continue-skill-selection-training',
    'export-skill-selection-model',
    'import-skill-selection-model',
    'publish-skill-selection-model',
    'load-skill-selection-model',
    'models\skill-selection',
    'training-data\skill-selection'
)

$semanticKitchenPatterns = @(
    'BM25',
    'regex',
    'Jaccard',
    'AstRelevanceRanker',
    'SkillRetriever',
    'CandidateSkillRetriever',
    'TaskIntentBoundaryClassifier',
    'ResultGapInspector',
    'OutputValidator',
    'TaskCompletionGuard',
    'TrainBatch',
    'DpoOptimizer',
    'ILearningMachine',
    'IMachineTraining',
    'SkillSelectionMachine',
    'SmartSkillsSemanticUnitCatalog',
    'ProjectLocalSkillState',
    'private corpus',
    'private-corpora',
    'training-data\skill-selection',
    'models\skill-selection'
)

$externalToolPatterns = @(
    'OpenAI',
    'Anthropic',
    'Claude API',
    'external AI',
    'image generation',
    'stable diffusion',
    'Invoke-RestMethod',
    'Invoke-WebRequest',
    'curl ',
    'web-hosted model'
)

$deterministicMasqueradePatterns = @(
    'ExactOrStaticEvidenceOnly',
    'HostProvidedEvidenceOnly',
    'DeterministicEvidenceUsedAsLearning = true',
    'HostProvidedSemanticDecision = true',
    'exact prompt',
    'exact lookup',
    'static map',
    'lookup table',
    'hard-coded skill',
    'hardcoded skill'
)

$modelManifest = Join-Path $resolvedCoreRoot 'models\skill-selection\skill-selection-v1\manifest.json'
$modelCheckpoint = Join-Path $resolvedCoreRoot 'models\skill-selection\skill-selection-v1\skill-selection-v1-checkpoint.json'
$modelIndex = Join-Path $resolvedCoreRoot 'models\index.json'
$trainingManifest = Join-Path $resolvedCoreRoot 'training-data\skill-selection\skill-selection-v1\training-manifest.json'
$coreAssets = [ordered]@{
    model_manifest_exists = Test-Path -LiteralPath $modelManifest -PathType Leaf
    model_checkpoint_exists = Test-Path -LiteralPath $modelCheckpoint -PathType Leaf
    model_index_exists = Test-Path -LiteralPath $modelIndex -PathType Leaf
    training_manifest_exists = Test-Path -LiteralPath $trainingManifest -PathType Leaf
}
$coreAssetsPresent = $coreAssets.model_manifest_exists -and
    $coreAssets.model_checkpoint_exists -and
    $coreAssets.model_index_exists -and
    $coreAssets.training_manifest_exists

$hostCheckRequired = $RequireImplementationFiles.IsPresent -or
    $ForbidHostSemanticKitchen.IsPresent -or
    $ForbidExternalAiOrUnrelatedToolUse.IsPresent -or
    $ForbidDeterministicMasquerade.IsPresent -or
    -not ([string]::IsNullOrWhiteSpace($ClaudeRepoRoot) -and [string]::IsNullOrWhiteSpace($CodexRepoRoot) -and [string]::IsNullOrWhiteSpace($CursorRepoRoot))

$hosts = @()
if ($hostCheckRequired) {
    $hosts = @(
        Get-HostEvidence -Name 'claude' -PathValue $ClaudeRepoRoot -ResolvedCoreRoot $resolvedCoreRoot -SemanticKitchenPatterns $semanticKitchenPatterns -ExternalToolPatterns $externalToolPatterns -DeterministicMasqueradePatterns $deterministicMasqueradePatterns -LegacyForbiddenPatterns $legacyForbiddenPatterns
        Get-HostEvidence -Name 'codex' -PathValue $CodexRepoRoot -ResolvedCoreRoot $resolvedCoreRoot -SemanticKitchenPatterns $semanticKitchenPatterns -ExternalToolPatterns $externalToolPatterns -DeterministicMasqueradePatterns $deterministicMasqueradePatterns -LegacyForbiddenPatterns $legacyForbiddenPatterns
        Get-HostEvidence -Name 'cursor' -PathValue $CursorRepoRoot -ResolvedCoreRoot $resolvedCoreRoot -SemanticKitchenPatterns $semanticKitchenPatterns -ExternalToolPatterns $externalToolPatterns -DeterministicMasqueradePatterns $deterministicMasqueradePatterns -LegacyForbiddenPatterns $legacyForbiddenPatterns
    )
}

$resolvedHostPaths = @($hosts | Where-Object { $_.exists } | ForEach-Object { Normalize-PathForCompare -PathValue $_.path })
$hostRootsAreDistinct = ($resolvedHostPaths | Select-Object -Unique).Count -eq 3
$hostsExist = ($hosts.Count -eq 3 -and @($hosts | Where-Object { -not $_.exists }).Count -eq 0)
$hostsOutsideCore = ($hosts.Count -eq 3 -and @($hosts | Where-Object { -not $_.outside_core_root }).Count -eq 0)
$hostsHaveImplementationFiles = ($hosts.Count -eq 3 -and @($hosts | Where-Object { -not $_.has_implementation_files }).Count -eq 0)
$hostsNoSemanticKitchen = ($hosts.Count -eq 3 -and @($hosts | Where-Object { $_.semantic_kitchen_matches.Count -gt 0 }).Count -eq 0)
$hostsNoExternalTools = ($hosts.Count -eq 3 -and @($hosts | Where-Object { $_.external_tool_matches.Count -gt 0 }).Count -eq 0)
$hostsNoDeterministicMasquerade = ($hosts.Count -eq 3 -and @($hosts | Where-Object { $_.deterministic_masquerade_matches.Count -gt 0 }).Count -eq 0)
$hostsPassedLegacy = ($hosts.Count -eq 3 -and @($hosts | Where-Object { -not $_.passed_legacy_boundary }).Count -eq 0)

$contractEvidence = $null
if ($RequireApiCheckSetContract.IsPresent -or -not [string]::IsNullOrWhiteSpace($CoreContractFile)) {
    $contractEvidence = Test-CoreContract -ResolvedCoreRoot $resolvedCoreRoot -ContractPath $CoreContractFile
}

$coreHostPackageEvidence = $null
if ($ForbidHostPackageCodeInCore.IsPresent) {
    $coreHostPackageEvidence = Test-CoreHostPackageCode -ResolvedCoreRoot $resolvedCoreRoot
}

$phase40Assertions = [ordered]@{
    require_api_check_set_contract = if ($RequireApiCheckSetContract.IsPresent) { $contractEvidence.passed } else { $true }
    forbid_host_package_code_in_core = if ($ForbidHostPackageCodeInCore.IsPresent) { $coreHostPackageEvidence.passed } else { $true }
    require_implementation_files = if ($RequireImplementationFiles.IsPresent) { ($hostsExist -and $hostRootsAreDistinct -and $hostsOutsideCore -and $hostsHaveImplementationFiles) } else { $true }
    forbid_host_semantic_kitchen = if ($ForbidHostSemanticKitchen.IsPresent) { $hostsNoSemanticKitchen } else { $true }
    forbid_external_ai_or_unrelated_tool_use = if ($ForbidExternalAiOrUnrelatedToolUse.IsPresent) { $hostsNoExternalTools } else { $true }
    forbid_deterministic_masquerade = if ($ForbidDeterministicMasquerade.IsPresent) { $hostsNoDeterministicMasquerade } else { $true }
    require_common_adaptive_split = if ($RequireCommonAdaptiveSplit.IsPresent) { $coreAssetsPresent } else { $true }
}

$phase40Passed = @($phase40Assertions.Values | Where-Object { $_ -eq $false }).Count -eq 0
$legacyPassed = $coreAssetsPresent -and $hostRootsAreDistinct -and $hostsPassedLegacy
$passed = if ($phase40Mode) { $phase40Passed } else { $legacyPassed }

$report = [ordered]@{
    command = 'skill-selection-core-extension-boundary-gate'
    phase40_mode = $phase40Mode
    passed = $passed
    core_root = $resolvedCoreRoot
    core_contract = $contractEvidence
    core_host_package_code = $coreHostPackageEvidence
    core_assets = $coreAssets
    core_assets_present = $coreAssetsPresent
    host_roots_are_distinct = $hostRootsAreDistinct
    host_check_required = $hostCheckRequired
    phase40_flags = [ordered]@{
        require_api_check_set_contract = [bool]$RequireApiCheckSetContract
        forbid_host_package_code_in_core = [bool]$ForbidHostPackageCodeInCore
        require_implementation_files = [bool]$RequireImplementationFiles
        forbid_host_semantic_kitchen = [bool]$ForbidHostSemanticKitchen
        forbid_external_ai_or_unrelated_tool_use = [bool]$ForbidExternalAiOrUnrelatedToolUse
        forbid_deterministic_masquerade = [bool]$ForbidDeterministicMasquerade
        require_common_adaptive_split = [bool]$RequireCommonAdaptiveSplit
    }
    phase40_assertions = $phase40Assertions
    legacy_forbidden_host_patterns = $legacyForbiddenPatterns
    semantic_kitchen_patterns = $semanticKitchenPatterns
    external_tool_patterns = $externalToolPatterns
    deterministic_masquerade_patterns = $deterministicMasqueradePatterns
    hosts = $hosts
    host_repos_do_not_own_training_logic = if ($hostCheckRequired) { $hostsNoSemanticKitchen } else { $true }
    host_repos_outside_core = if ($hostCheckRequired) { $hostsOutsideCore } else { $true }
    host_repos_do_not_contain_private_corpus = if ($hostCheckRequired) { @($hosts | ForEach-Object { $_.semantic_kitchen_matches } | Where-Object { $_.pattern -like '*private*' -or $_.pattern -like '*corpus*' }).Count -eq 0 } else { $true }
    host_repos_do_not_contain_project_local_state = if ($hostCheckRequired) { @($hosts | ForEach-Object { $_.semantic_kitchen_matches } | Where-Object { $_.pattern -like '*ProjectLocalSkillState*' -or $_.pattern -like '*.xiigen*' }).Count -eq 0 } else { $true }
    host_repos_do_not_contain_canonical_library_source = if ($hostCheckRequired) { @($hosts | ForEach-Object { $_.legacy_forbidden_matches } | Where-Object { $_.pattern -like '*canonical*' -or $_.pattern -like '*skills-inventory*' }).Count -eq 0 } else { $true }
}

$report | ConvertTo-Json -Depth 12
if ($passed) {
    exit 0
}

exit 1
