param(
    [int]$ExternalStart,
    [int]$ExternalEnd,
    [int]$LocalStart,
    [int]$LocalEnd,
    [string]$Tag = "lifecycle_support"
)

$ErrorActionPreference = "Stop"

$scriptRoot = "C:\Users\lubao\Documents\xiigen\llm mvp\documentation learning\learning-runs internal sets"
$localRoot = "C:\Users\lubao\Documents\xiigen\llm mvp\documentation learning\documentation-runs\dotnet-csharp-callgraph-extraction"
$externalRoot = "C:\Users\lubao\Documents\xiigen\llm mvp\documentation learning\documentation-runs\external-dotnet-framework-lifecycle-source-proof"

if ($ExternalEnd -lt $ExternalStart) {
    throw "ExternalEnd must be >= ExternalStart."
}
if ($LocalEnd -lt $LocalStart) {
    throw "LocalEnd must be >= LocalStart."
}
if (($ExternalEnd - $ExternalStart + 1) -ne 10) {
    throw "Checkpoint protocol requires exactly 10 external batches."
}
if (($LocalEnd - $LocalStart + 1) -ne 10) {
    throw "Checkpoint protocol requires exactly 10 local batches."
}

function Get-BatchNumber([string]$batchName) {
    $match = [regex]::Match($batchName, "\d+")
    if (!$match.Success) {
        return -1
    }
    return [int]$match.Value
}

function Divide-Safe([double]$numerator, [double]$denominator) {
    if ($denominator -le 0) {
        return 0.0
    }
    return [math]::Round($numerator / $denominator, 6)
}

function Get-LedgerRows([string]$path, [int]$startBatch, [int]$endBatch) {
    if (!(Test-Path -LiteralPath $path)) {
        return @()
    }
    return @(Get-Content -LiteralPath $path |
            Where-Object { $_.Trim().Length -gt 0 } |
            ForEach-Object { $_ | ConvertFrom-Json } |
            Where-Object {
                $number = Get-BatchNumber ([string]$_.batch)
                $number -ge $startBatch -and $number -le $endBatch
            })
}

function Measure-RunnerCostPerRow($rows) {
    $items = @($rows)
    $rowCount = [int]((@($items) | ForEach-Object { [int]$_.rows_trained + [int]$_.rows_heldout } | Measure-Object -Sum).Sum)
    $cpuTotal = [double]((@($items) | ForEach-Object { [double]$_.cpu_ms } | Measure-Object -Sum).Sum)
    $elapsedTotal = [double]((@($items) | ForEach-Object { [double]$_.elapsed_ms } | Measure-Object -Sum).Sum)
    $averagePeakRam = [double]((@($items) | ForEach-Object { [double]$_.peak_ram_mb } | Measure-Object -Average).Average)
    $rowsPerBatch = Divide-Safe $rowCount $items.Count

    return [ordered]@{
        batch_count                         = $items.Count
        rows_measured                       = $rowCount
        cpu_ms_total                        = [math]::Round($cpuTotal, 6)
        elapsed_ms_total                    = [math]::Round($elapsedTotal, 6)
        average_peak_ram_mb_per_batch       = [math]::Round($averagePeakRam, 6)
        cpu_ms_per_row                      = Divide-Safe $cpuTotal $rowCount
        elapsed_ms_per_row                  = Divide-Safe $elapsedTotal $rowCount
        normalized_peak_ram_mb_per_row      = Divide-Safe $averagePeakRam $rowsPerBatch
    }
}

function Get-PreviousCheckpoint([string]$checkpointRoot, [int]$currentLocalEnd) {
    if (!(Test-Path -LiteralPath $checkpointRoot)) {
        return $null
    }

    $previous = @(Get-ChildItem -LiteralPath $checkpointRoot -Filter "CHECKPOINT-batch-*.json" |
            ForEach-Object {
                $batchNumber = Get-BatchNumber $_.BaseName
                [pscustomobject]@{
                    BatchNumber = $batchNumber
                    Path = $_.FullName
                }
            } |
            Where-Object { $_.BatchNumber -gt 0 -and $_.BatchNumber -lt $currentLocalEnd } |
            Sort-Object BatchNumber -Descending |
            Select-Object -First 1)

    if ($previous.Count -eq 0) {
        return $null
    }

    return Get-Content -LiteralPath $previous[0].Path -Raw | ConvertFrom-Json
}

function Delta([double]$current, [double]$previous) {
    return [math]::Round($current - $previous, 6)
}

function ImprovementForGap([double]$current, [double]$previous) {
    return [math]::Round($previous - $current, 6)
}

for ($batch = $ExternalStart; $batch -le $ExternalEnd; $batch++) {
    & (Join-Path $scriptRoot "build_external_code_doc_lifecycle_batch.ps1") -Batch $batch -Tag "external_framework_lifecycle"
}

& (Join-Path $scriptRoot "generate_code_doc_callgraph_batches.ps1") -StartBatch $LocalStart -EndBatch $LocalEnd -Tag $Tag
& (Join-Path $scriptRoot "evaluate_code_doc_quality.ps1") -Batch $LocalEnd -MaxRows 50
& (Join-Path $scriptRoot "audit_code_doc_inventory_coverage.ps1")
& (Join-Path $scriptRoot "audit_code_doc_depth_coverage.ps1")
& (Join-Path $scriptRoot "analyze_code_doc_depth_gaps.ps1")

$externalState = Get-Content -LiteralPath (Join-Path $externalRoot "STREAM-STATE.json") -Raw | ConvertFrom-Json
$localState = Get-Content -LiteralPath (Join-Path $localRoot "STREAM-STATE.json") -Raw | ConvertFrom-Json
$quality = Get-Content -LiteralPath (Join-Path $localRoot ("batch-{0:000}\documentation-quality\DOCUMENTATION-QUALITY-SUMMARY.json" -f $LocalEnd)) -Raw | ConvertFrom-Json
$depth = Get-Content -LiteralPath (Join-Path $localRoot "coverage-audit\DEPTH-COVERAGE-SUMMARY.json") -Raw | ConvertFrom-Json
$inventory = Get-Content -LiteralPath (Join-Path $localRoot "coverage-audit\COVERAGE-AUDIT-SUMMARY.json") -Raw | ConvertFrom-Json
$gapDiagnosis = Get-Content -LiteralPath (Join-Path $localRoot "coverage-audit\DEPTH-GAP-DIAGNOSIS.json") -Raw | ConvertFrom-Json
$externalLedgerRows = Get-LedgerRows (Join-Path $externalRoot "KPI-LEDGER.jsonl") $ExternalStart $ExternalEnd
$localLedgerRows = Get-LedgerRows (Join-Path $localRoot "KPI-LEDGER.jsonl") $LocalStart $LocalEnd
$externalCost = Measure-RunnerCostPerRow $externalLedgerRows
$localCost = Measure-RunnerCostPerRow $localLedgerRows
$checkpointRoot = Join-Path $localRoot "ten-batch-checkpoints"
$previousCheckpoint = Get-PreviousCheckpoint $checkpointRoot $LocalEnd

$coverageDelta = [ordered]@{
    compared_to = $null
    inventory = [ordered]@{
        methods_covered_delta = 0
        edges_covered_delta = 0
        improved = $false
    }
    depth = [ordered]@{
        methods_missing_positive_ready_improvement = 0
        methods_missing_negative_gate_improvement = 0
        methods_missing_d4_improvement = 0
        edges_missing_positive_ready_improvement = 0
        edges_missing_transit_negative_improvement = 0
        improved = $false
    }
}

if ($null -ne $previousCheckpoint) {
    $coverageDelta.compared_to = $previousCheckpoint.local_latest
    $coverageDelta.inventory.methods_covered_delta = Delta ([double]$inventory.method_covered) ([double]$previousCheckpoint.inventory_coverage.methods_covered)
    $coverageDelta.inventory.edges_covered_delta = Delta ([double]$inventory.edge_covered) ([double]$previousCheckpoint.inventory_coverage.edges_covered)
    $coverageDelta.inventory.improved = ($coverageDelta.inventory.methods_covered_delta -gt 0 -or $coverageDelta.inventory.edges_covered_delta -gt 0)

    $coverageDelta.depth.methods_missing_positive_ready_improvement = ImprovementForGap ([double]$depth.methods_missing_positive_ready) ([double]$previousCheckpoint.depth_coverage.methods_missing_positive_ready)
    $coverageDelta.depth.methods_missing_negative_gate_improvement = ImprovementForGap ([double]$depth.methods_missing_negative_gate) ([double]$previousCheckpoint.depth_coverage.methods_missing_negative_gate)
    $coverageDelta.depth.methods_missing_d4_improvement = ImprovementForGap ([double]$depth.methods_missing_d4) ([double]$previousCheckpoint.depth_coverage.methods_missing_d4)
    $coverageDelta.depth.edges_missing_positive_ready_improvement = ImprovementForGap ([double]$depth.edges_missing_positive_ready) ([double]$previousCheckpoint.depth_coverage.edges_missing_positive_ready)
    $coverageDelta.depth.edges_missing_transit_negative_improvement = ImprovementForGap ([double]$depth.edges_missing_transit_negative) ([double]$previousCheckpoint.depth_coverage.edges_missing_transit_negative)
    $coverageDelta.depth.improved = (
        $coverageDelta.depth.methods_missing_positive_ready_improvement -gt 0 -or
        $coverageDelta.depth.methods_missing_negative_gate_improvement -gt 0 -or
        $coverageDelta.depth.methods_missing_d4_improvement -gt 0 -or
        $coverageDelta.depth.edges_missing_positive_ready_improvement -gt 0 -or
        $coverageDelta.depth.edges_missing_transit_negative_improvement -gt 0
    )
}

$checkpoint = [ordered]@{
    completed_at = (Get-Date).ToString("o")
    external_batches = "batch-{0:000}..batch-{1:000}" -f $ExternalStart, $ExternalEnd
    local_batches = "batch-{0:000}..batch-{1:000}" -f $LocalStart, $LocalEnd
    external_latest = $externalState.latest_accepted_batch
    local_latest = $localState.latest_accepted_batch
    external_train_rows_total = $externalState.train_rows_total
    external_heldout_rows_total = $externalState.heldout_rows_total
    local_train_rows_total = $localState.train_rows_total
    local_heldout_rows_total = $localState.heldout_rows_total
    latest_external_kpi = $externalState.metrics
    latest_local_kpi = $localState.metrics
    runner_cost_per_row = [ordered]@{
        external_10_batch_average = $externalCost
        local_10_batch_average = $localCost
        note = "Rows measured = train rows + held-out rows. Peak RAM is normalized from average batch peak, not cumulative memory."
    }
    documentation_quality = $quality
    inventory_coverage = [ordered]@{
        methods_covered = $inventory.method_covered
        methods_total = $inventory.method_total
        edges_covered = $inventory.edge_covered
        edges_total = $inventory.edge_total
    }
    depth_coverage = [ordered]@{
        methods_missing_positive_ready = $depth.methods_missing_positive_ready
        methods_missing_negative_gate = $depth.methods_missing_negative_gate
        methods_missing_d4 = $depth.methods_missing_d4
        edges_missing_positive_ready = $depth.edges_missing_positive_ready
        edges_missing_transit_negative = $depth.edges_missing_transit_negative
    }
    coverage_delta_from_previous_checkpoint = $coverageDelta
    depth_gap_diagnosis = $gapDiagnosis
}

New-Item -ItemType Directory -Force -Path $checkpointRoot | Out-Null
$checkpointPath = Join-Path $checkpointRoot ("CHECKPOINT-batch-{0:000}.json" -f $LocalEnd)
Set-Content -LiteralPath $checkpointPath -Value ($checkpoint | ConvertTo-Json -Depth 60) -Encoding UTF8

Write-Host ("checkpoint local=batch-{0:000} external=batch-{1:000} quality={2} score={3} local_cpu_per_row={4} external_cpu_per_row={5} doc_cpu_per_row={6} doc_cpu_per_source_line={7} method_missing_positive={8} method_missing_d4={9} edge_missing_negative={10} coverage_improved={11} depth_improved={12}" -f $LocalEnd, $ExternalEnd, $quality.quality_level, $quality.documentation_quality_score, $localCost.cpu_ms_per_row, $externalCost.cpu_ms_per_row, $quality.quality_cost_per_evaluated_row.cpu_ms_per_row, $quality.documentation_generation_cost.cpu_ms_per_source_code_line, $depth.methods_missing_positive_ready, $depth.methods_missing_d4, $depth.edges_missing_transit_negative, $coverageDelta.inventory.improved, $coverageDelta.depth.improved)
