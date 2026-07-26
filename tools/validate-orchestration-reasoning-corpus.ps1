param(
    [string]$CorpusPath = "training-data\orchestration-reasoning\orchestration-reasoning-common-corpus.jsonl",
    [string]$ManifestPath = "training-data\orchestration-reasoning\orchestration-reasoning-common-corpus-manifest.json",
    [string]$ReportPath = "artifacts\orchestration-reasoning-corpus\OR-02V-validator-report.json"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Add-Check {
    param(
        [System.Collections.ArrayList]$Checks,
        [string]$Id,
        [bool]$Passed,
        [string]$Message
    )

    [void]$Checks.Add([ordered]@{
        id = $Id
        passed = $Passed
        message = $Message
    })
}

function Write-ValidationReport {
    param(
        [string]$ResolvedReportPath,
        [object]$Payload
    )

    $reportDirectory = Split-Path -Parent $ResolvedReportPath
    if (-not (Test-Path -LiteralPath $reportDirectory -PathType Container)) {
        New-Item -ItemType Directory -Path $reportDirectory | Out-Null
    }

    [System.IO.File]::WriteAllText(
        $ResolvedReportPath,
        (($Payload | ConvertTo-Json -Depth 10) + [Environment]::NewLine),
        [System.Text.UTF8Encoding]::new($false))
}

function New-ValidationPayload {
    param(
        [System.Collections.ArrayList]$Checks,
        [string]$CorpusPath,
        [string]$ManifestPath,
        [string]$CorpusSha256,
        [int]$RowCount,
        [int]$ManifestRowCount,
        [int]$ExcludedAdaptiveRowCount,
        [string[]]$RequiredRowFields
    )

    $failedChecks = @($Checks | Where-Object { $_.passed -ne $true })

    return [ordered]@{
        schema = "xiigen-orchestration-reasoning-corpus-validation-v1"
        schema_version = "1.1"
        work_order = "OR-02V"
        generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
        validation_only = $true
        training_started = $false
        dpo_finalized = $false
        dpo_train_ready = $false
        train_ready = $false
        model_or_checkpoint_claimed = $false
        host_runtime_claimed = $false
        corpus_path = $CorpusPath
        manifest_path = $ManifestPath
        corpus_sha256 = $CorpusSha256
        row_count = $RowCount
        manifest_row_count = $ManifestRowCount
        excluded_adaptive_row_count = $ExcludedAdaptiveRowCount
        required_row_fields = $RequiredRowFields
        checks = @($Checks)
        failed_checks = @($failedChecks)
        failures = @($failedChecks | ForEach-Object { "$($_.id): $($_.message)" })
        check_count = $Checks.Count
        failed_check_count = $failedChecks.Count
        passed = ($failedChecks.Count -eq 0)
        status = $(if ($failedChecks.Count -eq 0) { "PASS" } else { "FAIL" })
        claim_scope = "validator-only OrchestrationReasoning corpus gate; no inference, no training, no checkpoint, no host runtime"
        next_action = $(if ($failedChecks.Count -eq 0) { "parent REVIEW_INTERNAL reviews OR-02V evidence; if accepted, issue WO-COMMIT-OR-02V-01" } else { "repair first failed OR-02V validator check before OR-03" })
    }
}

function Resolve-InputPath {
    param(
        [string]$RepoRoot,
        [string]$Path
    )

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return $Path
    }

    return (Join-Path $RepoRoot $Path)
}

function Get-PropertyValue {
    param(
        [object]$Object,
        [string]$Name
    )

    $property = $Object.PSObject.Properties[$Name]
    if ($null -eq $property) {
        return $null
    }

    return $property.Value
}

function Test-HasProperty {
    param(
        [object]$Object,
        [string]$Name
    )

    return ($null -ne $Object.PSObject.Properties[$Name])
}

function Test-FalseProperty {
    param(
        [object]$Object,
        [string]$Name
    )

    if (-not (Test-HasProperty -Object $Object -Name $Name)) {
        return $false
    }

    return ((Get-PropertyValue -Object $Object -Name $Name) -eq $false)
}

function Test-Sha256Hex {
    param(
        [string]$Value
    )

    return ($Value -match '^[0-9a-fA-F]{64}$')
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$resolvedCorpusPath = Resolve-InputPath -RepoRoot $repoRoot -Path $CorpusPath
$resolvedManifestPath = Resolve-InputPath -RepoRoot $repoRoot -Path $ManifestPath
$resolvedReportPath = Resolve-InputPath -RepoRoot $repoRoot -Path $ReportPath
$checks = [System.Collections.ArrayList]::new()
$requiredRowFields = @(
    "corpus_row_id",
    "source_candidate_row_id",
    "source_ref",
    "source_sha256",
    "target_machine",
    "model_leg",
    "abstraction_layer",
    "universal_reasoning_pattern",
    "chosen_universal_ref",
    "rejected_near_miss_ref",
    "error_code",
    "error_taxonomy_ref",
    "project_adapter_refs",
    "excluded_technical_execution_refs",
    "raw_private_text_copied",
    "technical_execution_leakage",
    "dpo_finalized",
    "dpo_train_ready",
    "training_started"
)
$falseFlagFields = @(
    "raw_private_text_copied",
    "technical_execution_leakage",
    "dpo_finalized",
    "dpo_train_ready",
    "training_started"
)
$privatePathPattern = 'C:\\Users\\|C:/Users/'

Add-Check -Checks $checks -Id "corpus_file_exists" -Passed (Test-Path -LiteralPath $resolvedCorpusPath -PathType Leaf) -Message $resolvedCorpusPath
Add-Check -Checks $checks -Id "manifest_file_exists" -Passed (Test-Path -LiteralPath $resolvedManifestPath -PathType Leaf) -Message $resolvedManifestPath

if (-not (Test-Path -LiteralPath $resolvedCorpusPath -PathType Leaf) -or -not (Test-Path -LiteralPath $resolvedManifestPath -PathType Leaf)) {
    $payload = New-ValidationPayload -Checks $checks -CorpusPath $CorpusPath -ManifestPath $ManifestPath -CorpusSha256 "" -RowCount 0 -ManifestRowCount 0 -ExcludedAdaptiveRowCount 0 -RequiredRowFields $requiredRowFields
    Write-ValidationReport -ResolvedReportPath $resolvedReportPath -Payload $payload
    Write-Output ($payload | ConvertTo-Json -Depth 10)
    exit 1
}

$corpusLines = @()
try {
    $corpusLines = @(Get-Content -LiteralPath $resolvedCorpusPath | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
}
catch {
    Add-Check -Checks $checks -Id "corpus_file_readable" -Passed $false -Message $_.Exception.Message
    $payload = New-ValidationPayload -Checks $checks -CorpusPath $CorpusPath -ManifestPath $ManifestPath -CorpusSha256 "" -RowCount 0 -ManifestRowCount 0 -ExcludedAdaptiveRowCount 0 -RequiredRowFields $requiredRowFields
    Write-ValidationReport -ResolvedReportPath $resolvedReportPath -Payload $payload
    Write-Output ($payload | ConvertTo-Json -Depth 10)
    exit 1
}
$rows = @()
$jsonlFailures = @()
for ($index = 0; $index -lt $corpusLines.Count; $index++) {
    try {
        $parsedRow = $corpusLines[$index] | ConvertFrom-Json
        $rows += $parsedRow
    }
    catch {
        $jsonlFailures += "line=$($index + 1):$($_.Exception.Message)"
    }
}

$manifest = $null
$manifestParseMessage = "Manifest parsed."
try {
    $manifest = Get-Content -Raw -LiteralPath $resolvedManifestPath | ConvertFrom-Json
}
catch {
    $manifestParseMessage = $_.Exception.Message
}

Add-Check -Checks $checks -Id "corpus_jsonl_parses" -Passed ($jsonlFailures.Count -eq 0) -Message (($jsonlFailures -join "; "))
Add-Check -Checks $checks -Id "manifest_json_parses" -Passed ($null -ne $manifest) -Message $manifestParseMessage

$corpusHash = ""
if ($jsonlFailures.Count -eq 0) {
    $corpusHash = (Get-FileHash -LiteralPath $resolvedCorpusPath -Algorithm SHA256).Hash.ToLowerInvariant()
}

if ($jsonlFailures.Count -gt 0 -or $null -eq $manifest) {
    $payload = New-ValidationPayload -Checks $checks -CorpusPath $CorpusPath -ManifestPath $ManifestPath -CorpusSha256 $corpusHash -RowCount $rows.Count -ManifestRowCount 0 -ExcludedAdaptiveRowCount 0 -RequiredRowFields $requiredRowFields
    Write-ValidationReport -ResolvedReportPath $resolvedReportPath -Payload $payload
    Write-Output ($payload | ConvertTo-Json -Depth 10)
    exit 1
}

Add-Check -Checks $checks -Id "manifest_corpus_sha256_matches" -Passed ($manifest.corpus_sha256 -eq $corpusHash) -Message "manifest=$($manifest.corpus_sha256); actual=$corpusHash"

$candidateRef = $manifest.depends_on.candidate_ref
$candidateHashPassed = $false
$candidateHashMessage = "No dependency candidate_ref found."
if (-not [string]::IsNullOrWhiteSpace($candidateRef)) {
    $resolvedCandidatePath = Resolve-InputPath -RepoRoot $repoRoot -Path $candidateRef
    if (Test-Path -LiteralPath $resolvedCandidatePath -PathType Leaf) {
        $candidateHash = (Get-FileHash -LiteralPath $resolvedCandidatePath -Algorithm SHA256).Hash.ToLowerInvariant()
        $candidateHashPassed = ($manifest.depends_on.candidate_sha256 -eq $candidateHash)
        $candidateHashMessage = "manifest=$($manifest.depends_on.candidate_sha256); actual=$candidateHash"
    }
    else {
        $candidateHashMessage = "Candidate file not found: $candidateRef"
    }
}
Add-Check -Checks $checks -Id "manifest_candidate_sha256_matches" -Passed $candidateHashPassed -Message $candidateHashMessage
Add-Check -Checks $checks -Id "manifest_row_count_matches_corpus" -Passed ($rows.Count -eq [int]$manifest.row_count) -Message "rows=$($rows.Count); manifest.row_count=$($manifest.row_count)"

$rowFieldFailures = @()
$rowSemanticFailures = @()
foreach ($row in $rows) {
    foreach ($field in $requiredRowFields) {
        if (-not (Test-HasProperty -Object $row -Name $field)) {
            $rowFieldFailures += "$($row.corpus_row_id).$field"
        }
    }

    if ($row.model_leg -ne "common") {
        $rowSemanticFailures += "$($row.corpus_row_id).model_leg=$($row.model_leg)"
    }

    if ($row.target_machine -ne "OrchestrationReasoningMachine") {
        $rowSemanticFailures += "$($row.corpus_row_id).target_machine=$($row.target_machine)"
    }

    if ($row.error_code -notmatch '^ORCH-') {
        $rowSemanticFailures += "$($row.corpus_row_id).error_code=$($row.error_code)"
    }

    if ([string]::IsNullOrWhiteSpace($row.source_ref)) {
        $rowSemanticFailures += "$($row.corpus_row_id).source_ref_missing"
    }

    if (-not (Test-Sha256Hex -Value ([string]$row.source_sha256))) {
        $rowSemanticFailures += "$($row.corpus_row_id).source_sha256_invalid"
    }

    foreach ($field in $falseFlagFields) {
        if (-not (Test-FalseProperty -Object $row -Name $field)) {
            $rowSemanticFailures += "$($row.corpus_row_id).$field"
        }
    }

    $adapterRefs = Get-PropertyValue -Object $row -Name "project_adapter_refs"
    if ($null -eq $adapterRefs -or @($adapterRefs).Count -eq 0) {
        $rowSemanticFailures += "$($row.corpus_row_id).project_adapter_refs_empty"
    }

    $executionRefs = Get-PropertyValue -Object $row -Name "excluded_technical_execution_refs"
    if ($null -eq $executionRefs -or @($executionRefs).Count -eq 0) {
        $rowSemanticFailures += "$($row.corpus_row_id).excluded_technical_execution_refs_empty"
    }
}
Add-Check -Checks $checks -Id "required_trainable_target_fields_present" -Passed ($rowFieldFailures.Count -eq 0) -Message (($rowFieldFailures -join "; "))
Add-Check -Checks $checks -Id "row_semantics_and_false_flags" -Passed ($rowSemanticFailures.Count -eq 0) -Message (($rowSemanticFailures -join "; "))

$manifestFalseFailures = @($falseFlagFields | Where-Object { -not (Test-FalseProperty -Object $manifest -Name $_) })
Add-Check -Checks $checks -Id "manifest_false_flags" -Passed ($manifestFalseFailures.Count -eq 0) -Message (($manifestFalseFailures -join "; "))
Add-Check -Checks $checks -Id "manifest_excluded_adaptive_count_present" -Passed (Test-HasProperty -Object $manifest -Name "excluded_adaptive_row_count") -Message "excluded_adaptive_row_count=$($manifest.excluded_adaptive_row_count)"

$adaptiveRowsInCorpus = @($rows | Where-Object { $_.model_leg -eq "adaptive" })
Add-Check -Checks $checks -Id "adaptive_rows_not_in_common_corpus" -Passed ($adaptiveRowsInCorpus.Count -eq 0) -Message "adaptive_rows_in_corpus=$($adaptiveRowsInCorpus.Count)"

$rawText = ((Get-Content -Raw -LiteralPath $resolvedCorpusPath) + [Environment]::NewLine + (Get-Content -Raw -LiteralPath $resolvedManifestPath))
Add-Check -Checks $checks -Id "no_private_absolute_paths" -Passed ($rawText -notmatch $privatePathPattern) -Message "Checked corpus and manifest text."

$payload = New-ValidationPayload -Checks $checks -CorpusPath $CorpusPath -ManifestPath $ManifestPath -CorpusSha256 $corpusHash -RowCount $rows.Count -ManifestRowCount ([int]$manifest.row_count) -ExcludedAdaptiveRowCount ([int]$manifest.excluded_adaptive_row_count) -RequiredRowFields $requiredRowFields
Write-ValidationReport -ResolvedReportPath $resolvedReportPath -Payload $payload

Write-Output ($payload | ConvertTo-Json -Depth 10)
if ($payload.passed) {
    exit 0
}

exit 1
