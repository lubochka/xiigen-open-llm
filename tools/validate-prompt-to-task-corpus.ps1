param(
    [string]$CorpusPath = "artifacts\prompt-to-task-corpus.jsonl",
    [string]$SummaryPath = "artifacts\prompt-to-task-corpus-summary.json",
    [string]$UniversalizationPath = "artifacts\dpo-universalization\PTT-02U.json",
    [string]$ReportPath = "artifacts\prompt-to-task-corpus-validation\PTT-02V.json"
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

$repoRoot = Split-Path -Parent $PSScriptRoot
$resolvedCorpusPath = Resolve-InputPath -RepoRoot $repoRoot -Path $CorpusPath
$resolvedSummaryPath = Resolve-InputPath -RepoRoot $repoRoot -Path $SummaryPath
$resolvedUniversalizationPath = Resolve-InputPath -RepoRoot $repoRoot -Path $UniversalizationPath
$resolvedReportPath = Resolve-InputPath -RepoRoot $repoRoot -Path $ReportPath
$checks = [System.Collections.ArrayList]::new()
$requiredRowFields = @(
    "row_id",
    "source_path_hash_sha256",
    "source_path_redacted_ref",
    "content_hash",
    "corpus_bundle_id",
    "source_root_id",
    "split_candidate"
)
$forbiddenPayloadFields = @(
    "raw_content",
    "raw_text",
    "raw_prompt_text",
    "full_text",
    "prompt_text",
    "chosen_text",
    "rejected_text",
    "input_text",
    "output_text"
)
$falseFlagFields = @(
    "copied_raw_private_content",
    "dpo_finalized",
    "dpo_train_ready",
    "training_started"
)

foreach ($path in @($resolvedCorpusPath, $resolvedSummaryPath, $resolvedUniversalizationPath)) {
    Add-Check -Checks $checks -Id "file_exists:$([System.IO.Path]::GetFileName($path))" -Passed (Test-Path -LiteralPath $path -PathType Leaf) -Message $path
}

$corpusLines = @(Get-Content -LiteralPath $resolvedCorpusPath | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
$rows = @()
foreach ($line in $corpusLines) {
    $rows += ($line | ConvertFrom-Json)
}
$summary = Get-Content -Raw -LiteralPath $resolvedSummaryPath | ConvertFrom-Json
$universalization = Get-Content -Raw -LiteralPath $resolvedUniversalizationPath | ConvertFrom-Json

Add-Check -Checks $checks -Id "corpus_jsonl_parses" -Passed ($rows.Count -eq $corpusLines.Count) -Message "Parsed $($rows.Count) JSONL rows."
Add-Check -Checks $checks -Id "row_count_matches_summary" -Passed ($rows.Count -eq [int]$summary.row_count) -Message "rows=$($rows.Count); summary.row_count=$($summary.row_count)"

$rowFieldFailures = @()
for ($index = 0; $index -lt $rows.Count; $index++) {
    foreach ($field in $requiredRowFields) {
        if (-not (Test-HasProperty -Object $rows[$index] -Name $field)) {
            $rowFieldFailures += "row[$index].$field"
        }
    }
}
Add-Check -Checks $checks -Id "required_row_fields_present" -Passed ($rowFieldFailures.Count -eq 0) -Message (($rowFieldFailures -join "; "))

$summaryFieldsPresent = (
    (Test-HasProperty -Object $summary -Name "schema") -and
    (Test-HasProperty -Object $summary -Name "source_root_id") -and
    (Test-HasProperty -Object $summary -Name "source_root_path_hash_sha256") -and
    (Test-HasProperty -Object $summary -Name "row_count") -and
    (Test-HasProperty -Object $summary -Name "local_source_required")
)
Add-Check -Checks $checks -Id "summary_required_fields_present" -Passed $summaryFieldsPresent -Message "schema/source_root/hash/row_count/local_source_required checked."
$summaryFlagFailures = @($falseFlagFields | Where-Object { -not (Test-FalseProperty -Object $summary -Name $_) })
Add-Check -Checks $checks -Id "summary_false_flags" -Passed ($summaryFlagFailures.Count -eq 0) -Message (($summaryFlagFailures -join "; "))

$rowFlagFailures = @()
foreach ($row in $rows) {
    foreach ($field in $falseFlagFields) {
        if (-not (Test-FalseProperty -Object $row -Name $field)) {
            $rowFlagFailures += "$($row.row_id).$field"
        }
    }
}
Add-Check -Checks $checks -Id "row_false_flags" -Passed ($rowFlagFailures.Count -eq 0) -Message (($rowFlagFailures -join "; "))

$payloadFieldHits = @()
foreach ($artifact in @($rows + @($summary, $universalization))) {
    foreach ($field in $forbiddenPayloadFields) {
        if (Test-HasProperty -Object $artifact -Name $field) {
            $payloadFieldHits += $field
        }
    }
}
Add-Check -Checks $checks -Id "no_raw_payload_fields" -Passed ($payloadFieldHits.Count -eq 0) -Message (($payloadFieldHits | Sort-Object -Unique) -join "; ")

$legBoundary = Get-PropertyValue -Object $universalization -Name "leg_boundary"
$forbiddenFlags = Get-PropertyValue -Object $universalization -Name "forbidden_flags"
$universalizationPassed = (
    ($universalization.status -eq "PASS") -and
    ($legBoundary.common_leg_ok -eq $true) -and
    ($legBoundary.adaptive_leg_ok -eq $true) -and
    ($universalization.no_hidden_deterministic_shortcut -eq $true) -and
    ($forbiddenFlags.raw_private_text_copied -eq $false) -and
    ($forbiddenFlags.dpo_finalized -eq $false) -and
    ($forbiddenFlags.dpo_train_ready -eq $false) -and
    ($forbiddenFlags.training_started -eq $false)
)
Add-Check -Checks $checks -Id "ptt_02u_universalization_preconditions" -Passed $universalizationPassed -Message "PTT-02U common/adaptive/no-hidden-determinism/false forbidden flags checked."

$failedChecks = @($checks | Where-Object { $_.passed -ne $true })
$payload = [ordered]@{
    schema = "xiigen-prompt-to-task-corpus-validation-v1"
    schema_version = "1.0"
    work_order = "PTT-02V"
    generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
    validation_only = $true
    training_started = $false
    dpo_finalized = $false
    dpo_train_ready = $false
    train_ready = $false
    corpus_path = $CorpusPath
    summary_path = $SummaryPath
    universalization_path = $UniversalizationPath
    row_count = $rows.Count
    summary_row_count = [int]$summary.row_count
    required_row_fields = $requiredRowFields
    forbidden_payload_fields = $forbiddenPayloadFields
    checks = @($checks)
    failed_check_count = $failedChecks.Count
    passed = ($failedChecks.Count -eq 0)
    status = $(if ($failedChecks.Count -eq 0) { "PASS" } else { "FAIL" })
    claim_scope = "validator-only pre-training PromptToTask corpus gate; no inference, no training, no checkpoint, no host runtime"
    next_action = $(if ($failedChecks.Count -eq 0) { "parent REVIEW_INTERNAL reviews PTT-02V evidence; if accepted, issue WO-COMMIT-PTT-02V-01" } else { "repair first failed validator check before any PTT-03 work" })
}

$reportDirectory = Split-Path -Parent $resolvedReportPath
if (-not (Test-Path -LiteralPath $reportDirectory -PathType Container)) {
    New-Item -ItemType Directory -Path $reportDirectory | Out-Null
}

[System.IO.File]::WriteAllText(
    $resolvedReportPath,
    (($payload | ConvertTo-Json -Depth 10) + [Environment]::NewLine),
    [System.Text.UTF8Encoding]::new($false))

Write-Output ($payload | ConvertTo-Json -Depth 10)
if ($payload.passed) {
    exit 0
}

exit 1
