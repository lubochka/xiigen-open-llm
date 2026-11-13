param(
    [string]$Root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

$ErrorActionPreference = "Stop"

function ConvertTo-RelativePath {
    param(
        [string]$BasePath,
        [string]$FullPath
    )

    $baseUri = [Uri]((Resolve-Path $BasePath).Path.TrimEnd('\') + '\')
    $fullUri = [Uri](Resolve-Path $FullPath).Path
    return [Uri]::UnescapeDataString($baseUri.MakeRelativeUri($fullUri).ToString()).Replace('/', '\')
}

function Count-JsonlRows {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        return "UNKNOWN"
    }

    return (Get-Content -LiteralPath $Path -Encoding UTF8 | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }).Count
}

function Read-JsonFile {
    param([string]$Path)

    try {
        return Get-Content -LiteralPath $Path -Raw -Encoding UTF8 | ConvertFrom-Json
    }
    catch {
        return $null
    }
}

function Get-JsonNumber {
    param(
        [object]$Object,
        [string[]]$Names
    )

    if ($null -eq $Object) {
        return "UNKNOWN"
    }

    foreach ($name in $Names) {
        $current = $Object
        $found = $true
        foreach ($part in ($name -split "\.")) {
            if ($null -eq $current) {
                $found = $false
                break
            }

            $property = $current.PSObject.Properties[$part]
            if ($null -eq $property) {
                $found = $false
                break
            }

            $current = $property.Value
        }

        if ($found -and $null -ne $current) {
            return $current
        }
    }

    return "UNKNOWN"
}

function Infer-BatchGroupId {
    param(
        [string]$ArtifactRelativePath,
        [string]$Directory,
        [string]$FileName
    )

    $segments = @($ArtifactRelativePath -split "\\")
    if ($segments.Count -ge 2 -and $segments[0] -in @("runner-metrics", "base-wiki-runs", "phase-metrics")) {
        return "$($segments[0])/$($segments[1])".ToLowerInvariant()
    }

    return Infer-BatchId -Directory $Directory -FileName $FileName
}

function Infer-UnitName {
    param([string]$Text)

    $normalized = $Text.ToLowerInvariant()

    if ($normalized -match "request-router|request-routing") { return "ProjectWikiRequestRouterUnit" }
    if ($normalized -match "stack-version|stack-and-version") { return "StackAndVersionClassifierUnit" }
    if ($normalized -match "cross-stack-business-flow-graph|cross_stack_business_flow_graph|business-flow-graph") { return "CrossStackBusinessFlowGraphUnit" }
    if ($normalized -match "system-boundary|system_boundary|boundary-mapper") { return "SystemBoundaryMapperUnit" }
    if ($normalized -match "code-trace|code_trace") { return "CodeTraceBuilderUnit" }
    if ($normalized -match "entity-impact|entity_impact") { return "EntityImpactMapperUnit" }
    if ($normalized -match "documentation-generator|docgen") { return "DocumentationGeneratorUnit" }
    if ($normalized -match "query-planning|query_planning") { return "ProjectWikiQueryPlanningFlow" }
    if ($normalized -match "context-routing|context_routing") { return "WikiContextRoutingUnit" }
    if ($normalized -match "answer-composer|answer_composer") { return "ProjectWikiAnswerComposer" }
    if ($normalized -match "answer-verifier|answer_verifier") { return "ProjectAnswerVerifier" }
    if ($normalized -match "trace-aggregator|trace_aggregator") { return "WikiTraceAggregator" }
    if ($normalized -match "negative-dpo-generator|negative_dpo_generator") { return "NegativeDpoGenerator" }
    if ($normalized -match "negative-dpo|negative_dpo") { return "NegativeDpoGenerator" }
    if ($normalized -match "base-wiki") { return "ProjectWikiAnswerComposer" }

    return "UNKNOWN"
}

function ConvertTo-LocatorKey {
    param([string]$UnitName)

    switch ($UnitName) {
        "ProjectWikiRequestRouterUnit" { return "project-wiki.request-router" }
        "StackAndVersionClassifierUnit" { return "project-wiki.stack-and-version" }
        "SystemBoundaryMapperUnit" { return "project-wiki.system-boundary-mapper" }
        "CodeTraceBuilderUnit" { return "project-wiki.code-trace-builder" }
        "EntityImpactMapperUnit" { return "project-wiki.entity-impact-mapper" }
        "DocumentationGeneratorUnit" { return "project-wiki.documentation-generator" }
        "NegativeDpoGenerator" { return "project-wiki.negative-dpo-generator" }
        "ProjectWikiQueryPlanningFlow" { return "project-wiki.query-planning" }
        "WikiContextRoutingUnit" { return "project-wiki.context-routing" }
        "ProjectWikiAnswerComposer" { return "project-wiki.answer-composer" }
        "ProjectAnswerVerifier" { return "project-wiki.answer-verifier" }
        "WikiTraceAggregator" { return "project-wiki.trace-aggregator" }
        "CrossStackBusinessFlowGraphUnit" { return "project-wiki.cross-stack-business-flow-graph" }
        default { return "" }
    }
}

function ConvertTo-RemoteUnitId {
    param([string]$UnitName)

    $locatorKey = ConvertTo-LocatorKey -UnitName $UnitName
    if ([string]::IsNullOrWhiteSpace($locatorKey)) {
        return ""
    }

    return "remote-unit://$locatorKey/v1"
}

function Requires-DpoMetrics {
    param([string]$UnitName)

    return $UnitName -in @(
        "ProjectWikiRequestRouterUnit",
        "ProjectWikiQueryPlanningFlow",
        "ProjectWikiAnswerComposer",
        "WikiContextRoutingUnit",
        "ProjectAnswerVerifier",
        "WikiTraceAggregator",
        "CrossStackBusinessFlowGraphUnit",
        "CodeTraceBuilderUnit",
        "EntityImpactMapperUnit",
        "DocumentationGeneratorUnit",
        "NegativeDpoGenerator"
    )
}

function Infer-BatchId {
    param(
        [string]$Directory,
        [string]$FileName
    )

    $text = "$Directory\$FileName"
    $patterns = @(
        "batch-\d+[a-z0-9\-]*",
        "session-\d+-batch-\d+",
        "promotion-\d+",
        "stored-procedure-[a-z0-9\-]+-\d+"
    )

    foreach ($pattern in $patterns) {
        $match = [regex]::Match($text, $pattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
        if ($match.Success) {
            return $match.Value.ToLowerInvariant()
        }
    }

    return (Split-Path -Leaf $Directory).ToLowerInvariant()
}

function Get-StatusFromReport {
    param(
        [object]$Report,
        [string]$Path,
        [bool]$HasTrain,
        [bool]$HasHeldout,
        [bool]$HasCheckpoint
    )

    $text = ""
    try {
        $text = Get-Content -LiteralPath $Path -Raw -Encoding UTF8
    }
    catch {
        $text = ""
    }

    $lower = $text.ToLowerInvariant()

    $overallVerdict = Get-JsonNumber -Object $Report -Names @("overallVerdict", "value.overallVerdict")
    $summaryPassed = Get-JsonNumber -Object $Report -Names @("summary.passed", "value.summary.passed")
    $trainingStatus = Get-JsonNumber -Object $Report -Names @("training_status", "value.metrics.training_status")
    $isAuthoritativeRunnerPass =
        ($overallVerdict -ne "UNKNOWN") -and
        ($overallVerdict.ToString().ToLowerInvariant() -match "gate_passed|passed|promoted|accepted") -and
        ($summaryPassed.ToString().ToLowerInvariant() -eq "true") -and
        ($trainingStatus.ToString().ToLowerInvariant() -eq "published")

    if ($HasTrain -and $HasHeldout -and $isAuthoritativeRunnerPass) {
        return @("accepted", "runner emitted published gate pass with train and held-out evidence", "may_use_as_baseline", $HasCheckpoint)
    }

    if ($lower -match "rejected|fail|failed|regress|negative margin|not accepted") {
        return @("rejected", "report contains rejection or failure marker", "preserve_as_negative_history", $false)
    }

    if ($HasTrain -and $HasHeldout -and ($lower -match "accepted|passed|pass|promoted|promotion")) {
        return @("accepted", "report contains pass/promotion marker and train/held-out files exist", "may_use_as_baseline", $HasCheckpoint)
    }

    if (-not $HasTrain -or -not $HasHeldout) {
        return @("experimental_no_authority", "missing train or held-out rows", "preserve_trace_only", $false)
    }

    return @("experimental_no_authority", "no authoritative pass/reject marker found", "requires_adjudication_before_use", $false)
}

$rootPath = (Resolve-Path $Root).Path
$artifactRoot = Join-Path $rootPath "artifacts"
$registryRoot = Join-Path $rootPath "registry"
$reportsRoot = Join-Path $rootPath "reports"

New-Item -ItemType Directory -Force -Path $registryRoot | Out-Null
New-Item -ItemType Directory -Force -Path $reportsRoot | Out-Null

$allFiles = @(Get-ChildItem -LiteralPath $artifactRoot -Recurse -File -ErrorAction SilentlyContinue)
$jsonFiles = @($allFiles | Where-Object { $_.Extension -eq ".json" })
$jsonlFiles = @($allFiles | Where-Object { $_.Extension -eq ".jsonl" })
$reportFiles = @($allFiles | Where-Object { $_.Name -match "report|result|summary|metrics|evaluation" -and $_.Extension -in @(".json", ".md", ".txt") })
$checkpointFiles = @($allFiles | Where-Object { $_.Name -match "checkpoint|policy|reference" -or $_.Extension -eq ".bin" })
$manifestFiles = @($allFiles | Where-Object { $_.Name -match "manifest" -and $_.Extension -eq ".json" })

$groups = @{}

foreach ($file in $allFiles) {
    $relative = ConvertTo-RelativePath -BasePath $rootPath -FullPath $file.FullName
    $artifactRelative = ConvertTo-RelativePath -BasePath $artifactRoot -FullPath $file.FullName
    $batchId = Infer-BatchGroupId -ArtifactRelativePath $artifactRelative -Directory $file.DirectoryName -FileName $file.Name
    if (-not $groups.ContainsKey($batchId)) {
        $groups[$batchId] = [ordered]@{
            batch_id = $batchId
            files = @()
            train = @()
            heldout = @()
            reports = @()
            checkpoints = @()
            manifests = @()
        }
    }

    $groups[$batchId].files += $relative

    if ($file.Extension -eq ".jsonl" -and $file.Name -match "train|pairs|dpo-pairs") {
        $groups[$batchId].train += $file.FullName
    }
    if ($file.Extension -eq ".jsonl" -and $file.Name -match "heldout|held-out|held_out") {
        $groups[$batchId].heldout += $file.FullName
    }
    if ($file.Name -match "report|result|summary|metrics|evaluation") {
        $groups[$batchId].reports += $file.FullName
    }
    if ($file.Name -match "checkpoint|policy|reference" -or $file.Extension -eq ".bin") {
        $groups[$batchId].checkpoints += $file.FullName
    }
    if ($file.Name -match "manifest" -and $file.Extension -eq ".json") {
        $groups[$batchId].manifests += $file.FullName
    }
}

$entries = @()
$unknownFiles = @()

foreach ($group in $groups.Values) {
    $firstFile = if ($group.files.Count -gt 0) { $group.files[0] } else { "" }
    $context = "$($group.batch_id) $firstFile"
    $unitName = Infer-UnitName -Text $context
    $trainPath = if ($group.train.Count -gt 0) { $group.train[0] } else { "" }
    $heldoutPath = if ($group.heldout.Count -gt 0) { $group.heldout[0] } else { "" }
    $reportPath = if ($group.reports.Count -gt 0) { $group.reports[0] } else { "" }
    $checkpointPath = if ($group.checkpoints.Count -gt 0) { $group.checkpoints[0] } else { "" }
    $manifestPath = if ($group.manifests.Count -gt 0) { $group.manifests[0] } else { "" }
    $manifestRef = if ($manifestPath) { ConvertTo-RelativePath -BasePath $rootPath -FullPath $manifestPath } else { "" }
    $locatorKey = ConvertTo-LocatorKey -UnitName $unitName
    $remoteUnitId = ConvertTo-RemoteUnitId -UnitName $unitName
    $reportObject = if ($reportPath) { Read-JsonFile -Path $reportPath } else { $null }
    $reportedTrainRows = Get-JsonNumber -Object $reportObject -Names @("trainingRowCount", "training_row_count", "rows_trained_this_run", "value.metrics.training_row_count")
    $reportedHeldoutRows = Get-JsonNumber -Object $reportObject -Names @("heldOutRowCount", "heldoutRowCount", "held_out_row_count", "heldout_rows", "value.metrics.held_out_row_count")
    $reportedTargetUnitId = Get-JsonNumber -Object $reportObject -Names @("target_unit_id", "unit_name", "value.metrics.target_unit_id")
    $reportedTargetUnitName = if ($reportedTargetUnitId -ne "UNKNOWN") { Infer-UnitName -Text $reportedTargetUnitId } else { "UNKNOWN" }
    $hasTrainEvidence = ([bool]$trainPath) -or ($reportedTrainRows -ne "UNKNOWN")
    $hasHeldoutEvidence = ([bool]$heldoutPath) -or ($reportedHeldoutRows -ne "UNKNOWN")
    $statusParts = Get-StatusFromReport -Report $reportObject -Path $reportPath -HasTrain $hasTrainEvidence -HasHeldout $hasHeldoutEvidence -HasCheckpoint ([bool]$checkpointPath)
    $status = $statusParts[0]
    $statusReason = $statusParts[1]
    $allowedAction = $statusParts[2]
    $continuationAllowed = [bool]$statusParts[3]

    if ($unitName -eq "UNKNOWN" -or $group.files.Count -eq 0) {
        $status = "blocked"
        $statusReason = "unit name or files could not be classified"
        $allowedAction = "repair_metadata_before_use"
        $continuationAllowed = $false
        $unknownFiles += $group.files
    }

    $accuracyMetric = Get-JsonNumber -Object $reportObject -Names @("accuracy", "weighted_accuracy", "final_accuracy", "heldout_accuracy", "ExactEvidenceAccuracy", "exact_evidence_accuracy", "value.metrics.primary_accuracy", "value.metrics.dpo_accuracy", "dpo_run.final_accuracy", "dpo.final_accuracy")
    $rewardMarginMetric = Get-JsonNumber -Object $reportObject -Names @("reward_margin", "final_reward_margin", "heldout_reward_margin", "value.metrics.dpo_reward_margin", "dpo_run.final_reward_margin", "dpo.final_reward_margin")
    $lossMetric = Get-JsonNumber -Object $reportObject -Names @("loss", "final_loss", "heldout_loss", "value.metrics.dpo_loss", "dpo_run.final_loss", "dpo.final_loss")
    $hasRealMetric = ($accuracyMetric -ne "UNKNOWN") -or ($rewardMarginMetric -ne "UNKNOWN") -or ($lossMetric -ne "UNKNOWN")

    if ($status -eq "accepted" -and -not $hasRealMetric) {
        $status = "experimental_no_authority"
        $statusReason = "report has pass marker but no runner-emitted metric was found"
        $allowedAction = "requires_adjudication_before_use"
        $continuationAllowed = $false
    }

    $dpoAccuracyMetric = Get-JsonNumber -Object $reportObject -Names @("dpo_accuracy", "value.metrics.dpo_accuracy")
    $dpoRewardMarginMetric = Get-JsonNumber -Object $reportObject -Names @("dpo_reward_margin", "value.metrics.dpo_reward_margin")
    $dpoLossMetric = Get-JsonNumber -Object $reportObject -Names @("dpo_loss", "value.metrics.dpo_loss")
    if ($status -eq "accepted" -and (Requires-DpoMetrics -UnitName $unitName) -and
        ($dpoAccuracyMetric -eq "UNKNOWN" -or $dpoRewardMarginMetric -eq "UNKNOWN" -or $dpoLossMetric -eq "UNKNOWN")) {
        $status = "experimental_no_authority"
        $statusReason = "DPO-trainable unit has pass marker but missing runner-emitted DPO metrics"
        $allowedAction = "requires_adjudication_before_use"
        $continuationAllowed = $false
    }

    if ($status -eq "accepted" -and $reportedTargetUnitName -ne "UNKNOWN" -and $unitName -ne "UNKNOWN" -and $reportedTargetUnitName -ne $unitName) {
        $status = "rejected"
        $statusReason = "runner target unit does not match the batch unit inferred from artifact path"
        $allowedAction = "preserve_as_negative_history"
        $continuationAllowed = $false
    }

    $heldoutAccuracyMetric = Get-JsonNumber -Object $reportObject -Names @("heldout_accuracy", "heldOutAccuracy", "value.metrics.dpo_heldout_accuracy")
    if ($status -eq "accepted" -and -not $manifestPath -and $checkpointPath) {
        $rewardFailed = ($rewardMarginMetric -ne "UNKNOWN") -and ([double]$rewardMarginMetric -lt 0)
        $heldoutFailed = ($heldoutAccuracyMetric -ne "UNKNOWN") -and ([double]$heldoutAccuracyMetric -lt 0.75)
        if ($rewardFailed -or $heldoutFailed) {
            $status = "rejected"
            $statusReason = "training run produced checkpoint but publication was skipped because DPO reward margin or held-out accuracy failed"
            $allowedAction = "preserve_as_negative_history"
            $continuationAllowed = $false
        }
        else {
            $status = "experimental_no_authority"
            $statusReason = "training run produced checkpoint but no published manifest was found"
            $allowedAction = "requires_adjudication_before_use"
            $continuationAllowed = $false
        }
    }

    $checksum = "UNKNOWN"
    if ($checkpointPath) {
        try {
            $checksum = "sha256:" + (Get-FileHash -LiteralPath $checkpointPath -Algorithm SHA256).Hash.ToLowerInvariant()
        }
        catch {
            $checksum = "UNKNOWN"
        }
    }

    $entry = [ordered]@{
        batch_id = $group.batch_id
        unit_name = $unitName
        unit_version = "v1"
        status = $status
        status_reason = $statusReason
        train_rows = if ($reportedTrainRows -ne "UNKNOWN") { $reportedTrainRows } elseif ($trainPath) { Count-JsonlRows -Path $trainPath } else { "UNKNOWN" }
        heldout_rows = if ($reportedHeldoutRows -ne "UNKNOWN") { $reportedHeldoutRows } elseif ($heldoutPath) { Count-JsonlRows -Path $heldoutPath } else { "UNKNOWN" }
        train_path = if ($trainPath) { ConvertTo-RelativePath -BasePath $rootPath -FullPath $trainPath } else { "" }
        heldout_path = if ($heldoutPath) { ConvertTo-RelativePath -BasePath $rootPath -FullPath $heldoutPath } else { "" }
        runner_report_path = if ($reportPath) { ConvertTo-RelativePath -BasePath $rootPath -FullPath $reportPath } else { "" }
        metrics = [ordered]@{
            accuracy = $accuracyMetric
            reward_margin = $rewardMarginMetric
            loss = $lossMetric
            cpu_ms = Get-JsonNumber -Object $reportObject -Names @("cpu_ms", "CpuMilliseconds", "cpu_time_ms", "TotalCpuTimeMs", "value.metrics.cpu_ms", "dpo_run.total_cpu_time_ms")
            ram_mb = Get-JsonNumber -Object $reportObject -Names @("ram_mb", "PeakWorkingSetMb", "peak_working_set_mb", "value.metrics.peak_ram_mb", "dpo_run.max_working_set_delta_mb")
            elapsed_ms = Get-JsonNumber -Object $reportObject -Names @("elapsed_ms", "ElapsedMilliseconds", "total_elapsed_ms", "value.metrics.elapsed_ms", "dpo_run.total_elapsed_ms")
        }
        checkpoint_ref = if ($checkpointPath) { ConvertTo-RelativePath -BasePath $rootPath -FullPath $checkpointPath } else { "" }
        checkpoint_allowed_as_continuation = $continuationAllowed
        allowed_next_action = $allowedAction
        source_authority = if ($reportPath) { "runner_report" } else { "artifact_inventory" }
        manifest_ref = $manifestRef
        locator_key = $locatorKey
        remote_unit_id = $remoteUnitId
        upgrade_policy = if ($unitName -ne "UNKNOWN") { "minor_upgrade_requires_same_unit_contract_runner_metrics_compatibility_report_and_rollback_ref" } else { "" }
        compatibility_report_ref = if ($reportPath) { ConvertTo-RelativePath -BasePath $rootPath -FullPath $reportPath } else { "" }
        rollback_ref = if ($checkpointPath) { ConvertTo-RelativePath -BasePath $rootPath -FullPath $checkpointPath } else { "" }
        import_export_status = if ($manifestPath) { "manifest_present" } elseif ($checkpointPath -and -not [string]::IsNullOrWhiteSpace($locatorKey)) { "manifest_derivable_but_not_registry_linked" } elseif ($checkpointPath) { "checkpoint_reference_only" } elseif ($status -eq "accepted") { "not_publishable_baseline_only" } else { "checkpoint_reference_only" }
        locator_status = if ($manifestPath -and $checkpointPath -and -not [string]::IsNullOrWhiteSpace($locatorKey)) { "remote_manifest_derivable" } elseif ($status -eq "accepted" -and -not $checkpointPath) { "not_publishable_baseline_only" } elseif (-not [string]::IsNullOrWhiteSpace($locatorKey)) { "locator_key_present_manifest_not_linked" } else { "not_registered" }
        artifact_integrity = if (($status -eq "accepted") -and (-not $reportPath -or -not $hasTrainEvidence -or -not $hasHeldoutEvidence)) { "missing_required_artifact" } else { "inventory_checked" }
        artifact_checksum = $checksum
        lfs_oid = "UNKNOWN"
        source_run_id = Split-Path -Leaf (Split-Path -Parent (Join-Path $rootPath $firstFile))
        discovered_from = $firstFile
        review_status = "machine_classified_pending_human_review"
        warnings = @()
    }

    if (-not $manifestPath -and -not ($status -eq "accepted" -and -not $checkpointPath)) { $entry.warnings += "manifest_missing" }
    if ([string]::IsNullOrWhiteSpace($locatorKey) -and $unitName -ne "UNKNOWN") { $entry.warnings += "locator_key_missing" }
    if ([string]::IsNullOrWhiteSpace($remoteUnitId) -and $unitName -ne "UNKNOWN") { $entry.warnings += "remote_unit_id_missing" }
    if (-not $reportPath) { $entry.warnings += "runner_report_missing" }
    if (-not $hasTrainEvidence) { $entry.warnings += "train_rows_missing" }
    if (-not $hasHeldoutEvidence) { $entry.warnings += "heldout_rows_missing" }
    if ($status -eq "rejected" -and $continuationAllowed) { $entry.warnings += "invalid_rejected_continuation" }

    $entries += [pscustomobject]$entry
}

$statusGroups = $entries | Group-Object status
$statusCounts = [ordered]@{}
foreach ($status in @("accepted", "rejected", "experimental_no_authority", "blocked")) {
    $match = $statusGroups | Where-Object { $_.Name -eq $status }
    $statusCounts[$status] = if ($match) { $match.Count } else { 0 }
}

$acceptedMissingArtifacts = @($entries | Where-Object { $_.status -eq "accepted" -and ($_.warnings | Where-Object { $_ -in @("runner_report_missing", "train_rows_missing", "heldout_rows_missing") }).Count -gt 0 }).Count
$rejectedContinuationSources = @($entries | Where-Object { $_.status -eq "rejected" -and $_.checkpoint_allowed_as_continuation }).Count
$invalidStatuses = @($entries | Where-Object { $_.status -notin @("accepted", "rejected", "experimental_no_authority", "blocked") }).Count
$acceptedUnknownLocator = @($entries | Where-Object { $_.status -eq "accepted" -and $_.locator_status -eq "UNKNOWN" }).Count
$registryValidated = ($acceptedMissingArtifacts -eq 0) -and ($rejectedContinuationSources -eq 0) -and ($invalidStatuses -eq 0) -and ($acceptedUnknownLocator -eq 0)

$registry = [ordered]@{
    schema_version = "1.0"
    generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
    source_root = $rootPath
    training_continuation_blocked_until_registry_validated = (-not $registryValidated)
    status_counts = $statusCounts
    entries = $entries
}

$audit = [ordered]@{
    generated_at_utc = $registry.generated_at_utc
    source_root = $rootPath
    total_files = $allFiles.Count
    json_files = $jsonFiles.Count
    jsonl_files = $jsonlFiles.Count
    runner_reports = $reportFiles.Count
    checkpoints = $checkpointFiles.Count
    manifests = $manifestFiles.Count
    classified_batches = $entries.Count
    unknown_file_count = $unknownFiles.Count
    accepted_missing_artifacts = $acceptedMissingArtifacts
    rejected_continuation_sources = $rejectedContinuationSources
    invalid_statuses = $invalidStatuses
    accepted_unknown_locator = $acceptedUnknownLocator
    registry_validated = $registryValidated
    unknown_files = @($unknownFiles | Select-Object -First 200)
}

$migration = [ordered]@{
    scope = "cleanup_migration"
    training_started = $false
    new_batches_created = 0
    accuracy = "UNKNOWN unless read from runner report"
    reward_margin = "UNKNOWN unless read from runner report"
    loss = "UNKNOWN unless read from runner report"
    cpu_ms = "UNKNOWN"
    ram_mb = "UNKNOWN"
    elapsed_ms = "UNKNOWN"
    accepted_count = $statusCounts.accepted
    rejected_count = $statusCounts.rejected
    experimental_no_authority_count = $statusCounts.experimental_no_authority
    blocked_count = $statusCounts.blocked
    total_entries = $entries.Count
    next_allowed_action = if ($registryValidated) { "continue_with_registry_guard" } else { "validate_registry_or_fix_blocked_entries" }
}

$blockState = [ordered]@{
    training_continuation_blocked_until_registry_validated = (-not $registryValidated)
    reason = if ($registryValidated) { "registry validation passed" } else { "registry validation failed" }
    accepted_missing_artifacts = $acceptedMissingArtifacts
    rejected_continuation_sources = $rejectedContinuationSources
    invalid_statuses = $invalidStatuses
    accepted_unknown_locator = $acceptedUnknownLocator
}

$publishableAcceptedBatchByUnit = @{}
foreach ($entry in @($entries | Where-Object {
    $_.status -eq "accepted" -and
    -not [string]::IsNullOrWhiteSpace($_.manifest_ref) -and
    -not [string]::IsNullOrWhiteSpace($_.locator_key) -and
    -not [string]::IsNullOrWhiteSpace($_.remote_unit_id)
})) {
    if (-not $publishableAcceptedBatchByUnit.ContainsKey($entry.unit_name)) {
        $publishableAcceptedBatchByUnit[$entry.unit_name] = @()
    }

    $publishableAcceptedBatchByUnit[$entry.unit_name] += $entry.batch_id
}

$weakSignalEntries = @($entries | Where-Object {
    $_.status -in @("rejected", "blocked", "experimental_no_authority") -and
    -not [string]::IsNullOrWhiteSpace($_.unit_name) -and
    $_.unit_name -ne "UNKNOWN" -and
    -not $publishableAcceptedBatchByUnit.ContainsKey($_.unit_name)
})
$unitScores = @{}
foreach ($entry in $weakSignalEntries) {
    if (-not $unitScores.ContainsKey($entry.unit_name)) {
        $unitScores[$entry.unit_name] = 0
    }

    $unitScores[$entry.unit_name] += switch ($entry.status) {
        "rejected" { 3 }
        "blocked" { 2 }
        "experimental_no_authority" { 1 }
        default { 0 }
    }
}

$publishableAcceptedUnits = @($entries |
    Where-Object {
        $_.status -eq "accepted" -and
        -not [string]::IsNullOrWhiteSpace($_.manifest_ref) -and
        -not [string]::IsNullOrWhiteSpace($_.locator_key) -and
        -not [string]::IsNullOrWhiteSpace($_.remote_unit_id)
    } |
    Select-Object -ExpandProperty unit_name -Unique)
$requiredReadinessUnits = @(
    "ProjectWikiRequestRouterUnit",
    "StackAndVersionClassifierUnit",
    "SystemBoundaryMapperUnit",
    "CodeTraceBuilderUnit",
    "EntityImpactMapperUnit",
    "DocumentationGeneratorUnit",
    "NegativeDpoGenerator",
    "CrossStackBusinessFlowGraphUnit",
    "ProjectWikiQueryPlanningFlow",
    "WikiContextRoutingUnit",
    "ProjectWikiAnswerComposer",
    "ProjectAnswerVerifier",
    "WikiTraceAggregator"
)
$nextUnit = "NONE"
$nextScore = 0
$missingReadinessUnits = @($requiredReadinessUnits | Where-Object { $_ -notin $publishableAcceptedUnits })
foreach ($name in $missingReadinessUnits) {
    if ($unitScores.ContainsKey($name) -and $unitScores[$name] -gt 0) {
        $nextUnit = $name
        $nextScore = $unitScores[$name]
        break
    }
}

if ($nextUnit -eq "NONE" -and $missingReadinessUnits.Count -gt 0) {
    $nextUnit = $missingReadinessUnits[0]
    $nextScore = 0
}

if ($nextUnit -eq "NONE") {
    foreach ($name in $unitScores.Keys) {
        if ($unitScores[$name] -gt $nextScore) {
            $nextUnit = $name
            $nextScore = $unitScores[$name]
        }
    }
}

$evidenceBatchIds = @($entries |
    Where-Object { $_.unit_name -eq $nextUnit -and $_.status -in @("rejected", "blocked", "experimental_no_authority") } |
    Select-Object -First 10 -ExpandProperty batch_id)
if ($evidenceBatchIds.Count -eq 0 -and $nextUnit -notin @("NONE", "UNKNOWN")) {
    $evidenceBatchIds = @("validation-gap/$nextUnit")
}

$recommendedBatchStrategy = switch ($nextUnit) {
    "ProjectWikiRequestRouterUnit" { "build_50_project_wiki_request_routing_rows" }
    "StackAndVersionClassifierUnit" { "build_50_stack_and_version_classifier_rows" }
    "SystemBoundaryMapperUnit" { "build_50_system_boundary_mapper_rows" }
    "CodeTraceBuilderUnit" { "build_50_code_trace_builder_rows" }
    "EntityImpactMapperUnit" { "build_50_entity_impact_mapper_rows" }
    "DocumentationGeneratorUnit" { "build_50_documentation_generator_rows" }
    "NegativeDpoGenerator" { "build_50_negative_dpo_generator_rows" }
    "ProjectWikiQueryPlanningFlow" { "build_50_project_wiki_query_planning_rows" }
    "ProjectWikiAnswerComposer" { "build_50_project_wiki_answer_composer_rows" }
    "WikiContextRoutingUnit" { "build_50_wiki_context_routing_rows" }
    "ProjectAnswerVerifier" { "build_50_project_answer_verifier_rows" }
    "WikiTraceAggregator" { "build_50_wiki_trace_aggregator_rows" }
    "CrossStackBusinessFlowGraphUnit" { "build_50_cross_stack_business_flow_graph_rows" }
    default { "manual_architect_review_required" }
}

$nextTrainingAction = [ordered]@{
    report_scope = "next_training_action"
    source_of_truth = "PROJECT-WIKI-TRAINING-ASSET-REGISTRY.json"
    training_started = $false
    new_batches_created = 0
    target_unit_name = $nextUnit
    reason = if ($nextUnit -eq "NONE") { "No weak unit could be selected from registry statuses." } elseif ($nextUnit -in $missingReadinessUnits) { "Selected because the unit is required by readiness gates and has no publishable accepted manifest yet." } else { "Selected by registry weak-signal score from rejected, blocked, and experimental entries." }
    weak_signal_score = $nextScore
    evidence_batch_ids = $evidenceBatchIds
    recommended_batch_strategy = $recommendedBatchStrategy
    recommended_batch_size = 50
    new_training_batch_allowed = ($registryValidated -and $nextUnit -ne "NONE" -and $nextUnit -ne "UNKNOWN")
    stop_blind_training = (-not $registryValidated)
    registry_entry_count = $entries.Count
    status_counts = $statusCounts
}

$registryPath = Join-Path $registryRoot "PROJECT-WIKI-TRAINING-ASSET-REGISTRY.json"
$auditPath = Join-Path $registryRoot "ARTIFACT-INTEGRITY-AUDIT.json"
$migrationPath = Join-Path $reportsRoot "PROJECT-WIKI-TRAINING-ASSET-MIGRATION-REPORT.json"
$migrationMdPath = Join-Path $reportsRoot "PROJECT-WIKI-TRAINING-ASSET-MIGRATION-REPORT.md"
$blockPath = Join-Path $registryRoot "TRAINING-CONTINUATION-BLOCK.json"
$nextActionPath = Join-Path $reportsRoot "PROJECT-WIKI-NEXT-TRAINING-ACTION.json"
$nextActionMdPath = Join-Path $reportsRoot "PROJECT-WIKI-NEXT-TRAINING-ACTION.md"

$registry | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $registryPath -Encoding UTF8
$audit | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $auditPath -Encoding UTF8
$migration | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $migrationPath -Encoding UTF8
$blockState | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $blockPath -Encoding UTF8
$nextTrainingAction | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $nextActionPath -Encoding UTF8

$md = @(
    "# Project Wiki Training Asset Migration Report",
    "",
    "scope: cleanup_migration",
    "training_started: false",
    "new_batches_created: 0",
    "accepted_count: $($statusCounts.accepted)",
    "rejected_count: $($statusCounts.rejected)",
    "experimental_no_authority_count: $($statusCounts.experimental_no_authority)",
    "blocked_count: $($statusCounts.blocked)",
    "total_entries: $($entries.Count)",
    "registry_validated: $registryValidated",
    "training_continuation_blocked_until_registry_validated: $(-not $registryValidated)",
    "accuracy: UNKNOWN unless read from runner report",
    "reward_margin: UNKNOWN unless read from runner report",
    "loss: UNKNOWN unless read from runner report",
    "cpu_ms: UNKNOWN",
    "ram_mb: UNKNOWN",
    "elapsed_ms: UNKNOWN",
    "",
    "No training batch was started by this migration."
)
$md | Set-Content -LiteralPath $migrationMdPath -Encoding UTF8

$nextActionMd = @(
    "# Project Wiki Next Training Action",
    "",
    "report_scope: next_training_action",
    "training_started: false",
    "new_batches_created: 0",
    "target_unit_name: $nextUnit",
    "weak_signal_score: $nextScore",
    "recommended_batch_strategy: $recommendedBatchStrategy",
    "recommended_batch_size: 50",
    "new_training_batch_allowed: $($nextTrainingAction.new_training_batch_allowed)",
    "stop_blind_training: $($nextTrainingAction.stop_blind_training)",
    "reason: $($nextTrainingAction.reason)",
    "evidence_batch_ids: $($evidenceBatchIds -join ', ')"
)
$nextActionMd | Set-Content -LiteralPath $nextActionMdPath -Encoding UTF8

Write-Output "registry=$registryPath"
Write-Output "audit=$auditPath"
Write-Output "migration=$migrationPath"
Write-Output "next_training_action=$nextActionPath"
Write-Output "entries=$($entries.Count)"
Write-Output "accepted=$($statusCounts.accepted)"
Write-Output "rejected=$($statusCounts.rejected)"
Write-Output "experimental_no_authority=$($statusCounts.experimental_no_authority)"
Write-Output "blocked=$($statusCounts.blocked)"
