param(
    [string]$Root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

$ErrorActionPreference = "Stop"

$registryPath = Join-Path $Root "registry\PROJECT-WIKI-TRAINING-ASSET-REGISTRY.json"
$schemaPath = Join-Path $Root "registry\PROJECT-WIKI-TRAINING-ASSET-REGISTRY.schema.json"
$auditPath = Join-Path $Root "registry\ARTIFACT-INTEGRITY-AUDIT.json"
$migrationPath = Join-Path $Root "reports\PROJECT-WIKI-TRAINING-ASSET-MIGRATION-REPORT.json"
$blockPath = Join-Path $Root "registry\TRAINING-CONTINUATION-BLOCK.json"
$nextActionPath = Join-Path $Root "reports\PROJECT-WIKI-NEXT-TRAINING-ACTION.json"

$errors = New-Object System.Collections.Generic.List[string]

foreach ($path in @($registryPath, $schemaPath, $auditPath, $migrationPath, $blockPath, $nextActionPath)) {
    if (-not (Test-Path -LiteralPath $path)) {
        $errors.Add("missing required file: $path")
    }
}

if ($errors.Count -eq 0) {
    $registry = Get-Content -LiteralPath $registryPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $audit = Get-Content -LiteralPath $auditPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $migration = Get-Content -LiteralPath $migrationPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $block = Get-Content -LiteralPath $blockPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $nextAction = Get-Content -LiteralPath $nextActionPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $entries = @($registry.entries)
    $allowedStatuses = @("accepted", "rejected", "experimental_no_authority", "blocked")

    if ([bool]$registry.training_continuation_blocked_until_registry_validated) {
        $errors.Add("registry training continuation flag is still blocked")
    }

    if ([bool]$block.training_continuation_blocked_until_registry_validated) {
        $errors.Add("block file training continuation flag is still blocked")
    }

    if ($entries.Count -eq 0) {
        $errors.Add("registry contains no entries")
    }

    $invalidStatuses = @($entries | Where-Object { $_.status -notin $allowedStatuses })
    if ($invalidStatuses.Count -gt 0) {
        $errors.Add("registry contains invalid statuses: $($invalidStatuses.Count)")
    }

    $unknownStatuses = @($entries | Where-Object { [string]::IsNullOrWhiteSpace($_.status) -or $_.status -eq "UNKNOWN" })
    if ($unknownStatuses.Count -gt 0) {
        $errors.Add("registry contains UNKNOWN batch statuses: $($unknownStatuses.Count)")
    }

    $acceptedMissingReport = @($entries | Where-Object { $_.status -eq "accepted" -and [string]::IsNullOrWhiteSpace($_.runner_report_path) })
    if ($acceptedMissingReport.Count -gt 0) {
        $errors.Add("accepted entries without runner report: $($acceptedMissingReport.Count)")
    }

    $acceptedUnknownLocator = @($entries | Where-Object { $_.status -eq "accepted" -and $_.locator_status -eq "UNKNOWN" })
    if ($acceptedUnknownLocator.Count -gt 0) {
        $errors.Add("accepted entries with UNKNOWN locator status: $($acceptedUnknownLocator.Count)")
    }

    $acceptedPublishableMissingManifest = @($entries | Where-Object { $_.status -eq "accepted" -and $_.import_export_status -ne "not_publishable_baseline_only" -and [string]::IsNullOrWhiteSpace($_.manifest_ref) })
    if ($acceptedPublishableMissingManifest.Count -gt 0) {
        $errors.Add("accepted publishable entries without manifest_ref: $($acceptedPublishableMissingManifest.Count)")
    }

    $acceptedPublishableMissingLocator = @($entries | Where-Object { $_.status -eq "accepted" -and $_.import_export_status -ne "not_publishable_baseline_only" -and [string]::IsNullOrWhiteSpace($_.locator_key) })
    if ($acceptedPublishableMissingLocator.Count -gt 0) {
        $errors.Add("accepted publishable entries without locator_key: $($acceptedPublishableMissingLocator.Count)")
    }

    $acceptedPublishableMissingRemoteUnit = @($entries | Where-Object { $_.status -eq "accepted" -and $_.import_export_status -ne "not_publishable_baseline_only" -and [string]::IsNullOrWhiteSpace($_.remote_unit_id) })
    if ($acceptedPublishableMissingRemoteUnit.Count -gt 0) {
        $errors.Add("accepted publishable entries without remote_unit_id: $($acceptedPublishableMissingRemoteUnit.Count)")
    }

    $allowedImportExportStatuses = @("manifest_present", "manifest_derivable_but_not_registry_linked", "checkpoint_reference_only", "not_publishable_baseline_only")
    $invalidImportExportStatus = @($entries | Where-Object { $_.import_export_status -notin $allowedImportExportStatuses })
    if ($invalidImportExportStatus.Count -gt 0) {
        $errors.Add("entries with invalid import_export_status: $($invalidImportExportStatus.Count)")
    }

    $rejectedContinuation = @($entries | Where-Object { $_.status -eq "rejected" -and [bool]$_.checkpoint_allowed_as_continuation })
    if ($rejectedContinuation.Count -gt 0) {
        $errors.Add("rejected continuation sources: $($rejectedContinuation.Count)")
    }

    if ([int]$audit.accepted_missing_artifacts -ne 0) {
        $errors.Add("audit accepted_missing_artifacts is not zero")
    }

    if ([int]$audit.rejected_continuation_sources -ne 0) {
        $errors.Add("audit rejected_continuation_sources is not zero")
    }

    if (-not [bool]$audit.registry_validated) {
        $errors.Add("audit registry_validated is false")
    }

    if ([bool]$migration.training_started) {
        $errors.Add("migration report says training_started=true")
    }

    if ([int]$migration.new_batches_created -ne 0) {
        $errors.Add("migration report says new_batches_created is not zero")
    }

    if ([bool]$nextAction.training_started) {
        $errors.Add("next training action report says training_started=true")
    }

    if ([int]$nextAction.new_batches_created -ne 0) {
        $errors.Add("next training action report says new_batches_created is not zero")
    }

    if ([string]::IsNullOrWhiteSpace($nextAction.target_unit_name) -or $nextAction.target_unit_name -in @("UNKNOWN", "NONE")) {
        $errors.Add("next training action target unit is not actionable")
    }

    if ([string]::IsNullOrWhiteSpace($nextAction.recommended_batch_strategy) -or $nextAction.recommended_batch_strategy -eq "manual_architect_review_required") {
        $errors.Add("next training action has no concrete batch strategy")
    }

    if ([int]$nextAction.recommended_batch_size -ne 50) {
        $errors.Add("next training action recommended batch size is not 50")
    }

    if (@($nextAction.evidence_batch_ids).Count -eq 0) {
        $errors.Add("next training action has no evidence batch ids")
    }

    if (-not [bool]$nextAction.new_training_batch_allowed) {
        $errors.Add("next training action does not allow the next batch after registry validation")
    }

    if ([bool]$nextAction.stop_blind_training) {
        $errors.Add("next training action still requests stop_blind_training")
    }

    $statusCounts = @{}
    foreach ($status in $allowedStatuses) {
        $statusCounts[$status] = @($entries | Where-Object { $_.status -eq $status }).Count
    }

    foreach ($status in $allowedStatuses) {
        if ([int]$registry.status_counts.$status -ne [int]$statusCounts[$status]) {
            $errors.Add("status count mismatch for $status")
        }
    }
}

if ($errors.Count -gt 0) {
    $errors | ForEach-Object { Write-Error $_ }
    exit 1
}

Write-Output "PROJECT_WIKI_TRAINING_ASSET_REGISTRY_GATE_B_PASSED"
