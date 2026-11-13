$ErrorActionPreference = "Stop"

$root = "C:\Users\lubao\Documents\xiigen\llm mvp\documentation learning\documentation-runs\dotnet-csharp-callgraph-extraction"
$outDir = Join-Path $root "coverage-audit"
$methodDepthPath = Join-Path $outDir "METHOD-DEPTH-COVERAGE.jsonl"
$edgeDepthPath = Join-Path $outDir "EDGE-DEPTH-COVERAGE.jsonl"
$methodInventoryPath = Join-Path $root "extracted-method-inventory.jsonl"
$edgeInventoryPath = Join-Path $root "extracted-callgraph-edges.jsonl"

function JsonLine($obj) {
    return ($obj | ConvertTo-Json -Depth 60 -Compress)
}

function Read-Jsonl([string]$path) {
    if (!(Test-Path -LiteralPath $path)) {
        return @()
    }
    return @(Get-Content -LiteralPath $path |
            Where-Object { $_.Trim().Length -gt 0 } |
            ForEach-Object { $_ | ConvertFrom-Json })
}

function Group-Counts($rows, [string]$field) {
    return [object[]]@($rows |
        Group-Object $field |
        Sort-Object Count -Descending |
        ForEach-Object {
            [ordered]@{
                name  = $_.Name
                count = $_.Count
            }
        })
}

$methodDepth = Read-Jsonl $methodDepthPath
$edgeDepth = Read-Jsonl $edgeDepthPath
$methodInventory = Read-Jsonl $methodInventoryPath
$edgeInventory = Read-Jsonl $edgeInventoryPath

$methodBySymbol = @{}
foreach ($method in $methodInventory) {
    $methodBySymbol[[string]$method.symbol] = $method
}

$callerSet = @{}
$calleesByCaller = @{}
foreach ($edge in $edgeInventory) {
    $caller = [string]$edge.caller_symbol
    $callee = [string]$edge.callee_symbol
    $callerSet[$caller] = $true
    if (!$calleesByCaller.ContainsKey($caller)) {
        $calleesByCaller[$caller] = New-Object System.Collections.ArrayList
    }
    [void]$calleesByCaller[$caller].Add($callee)
}

$missingPositive = @($methodDepth | Where-Object { $_.gap_reason -eq "needs_positive_ready_documentation_examples" })
$missingD4 = @($methodDepth | Where-Object { $_.gap_reason -eq "needs_caller_or_workflow_level_examples" })
$missingEdgeNegative = @($edgeDepth | Where-Object { $_.gap_reason -eq "needs_missing_callee_negative_examples" })

$calleePropagation = @{}
$batchDirs = @(Get-ChildItem -LiteralPath $root -Directory |
        Where-Object { $_.Name -match "^batch-\d{3}$" } |
        Sort-Object Name)
foreach ($batchDir in $batchDirs) {
    foreach ($fileName in @("TRAIN-PAIRS.jsonl", "HELDOUT-ROWS.jsonl")) {
        $path = Join-Path $batchDir.FullName $fileName
        if (!(Test-Path -LiteralPath $path)) {
            continue
        }
        foreach ($row in Read-Jsonl $path) {
            if ([string]$row.documentation_level -ne "D4") {
                continue
            }
            foreach ($callee in @($row.known_callees)) {
                $symbol = [string]$callee.symbol_id
                if ([string]::IsNullOrWhiteSpace($symbol)) {
                    continue
                }
                if (!$calleePropagation.ContainsKey($symbol)) {
                    $calleePropagation[$symbol] = 0
                }
                $calleePropagation[$symbol] = [int]$calleePropagation[$symbol] + 1
            }
        }
    }
}

$missingPositiveWithBody = @($missingPositive | Where-Object {
        $methodBySymbol.ContainsKey([string]$_.symbol) -and
        -not [string]::IsNullOrWhiteSpace([string]$methodBySymbol[[string]$_.symbol].body)
    })
$missingPositiveWithoutBody = @($missingPositive | Where-Object {
        !$methodBySymbol.ContainsKey([string]$_.symbol) -or
        [string]::IsNullOrWhiteSpace([string]$methodBySymbol[[string]$_.symbol].body)
    })

$missingD4HasOutgoing = @($missingD4 | Where-Object { $callerSet.ContainsKey([string]$_.symbol) })
$missingD4NoOutgoing = @($missingD4 | Where-Object { !$callerSet.ContainsKey([string]$_.symbol) })
$missingD4AlreadyPropagatedAsCallee = @($missingD4 | Where-Object { $calleePropagation.ContainsKey([string]$_.symbol) })
$missingD4NoCalleePropagation = @($missingD4 | Where-Object { !$calleePropagation.ContainsKey([string]$_.symbol) })

$edgeNegativeRows = New-Object System.Collections.ArrayList
foreach ($row in $missingEdgeNegative) {
    $parts = ([string]$row.edge).Split("->", [System.StringSplitOptions]::None)
    $caller = if ($parts.Count -gt 0) { $parts[0] } else { "" }
    $distinctCallees = 0
    if ($calleesByCaller.ContainsKey($caller)) {
        $distinctCallees = @($calleesByCaller[$caller] | Sort-Object -Unique).Count
    }
    [void]$edgeNegativeRows.Add([ordered]@{
            project                  = [string]$row.project
            edge                     = [string]$row.edge
            caller                   = $caller
            distinct_callee_count    = $distinctCallees
            total_appearances        = [int]$row.total_appearances
            positive_ready_rows      = [int]$row.positive_ready_rows
            reason_not_closed        = if ($distinctCallees -le 1) {
                "single-callee caller needs explicit empty-transit negative; partial-transit generator cannot create a partial omission from one callee"
            }
            else {
                "multi-callee caller was not targeted by partial/empty transit-negative selection"
            }
        })
}

$diagnosis = [ordered]@{
    generated_at = (Get-Date).ToString("o")
    batches_scanned = $batchDirs.Count
    headline = "No depth improvement means the current generator is no longer selecting the exact uncovered symbols/edges."
    missing_positive_ready = [ordered]@{
        total = $missingPositive.Count
        with_inventory_body = $missingPositiveWithBody.Count
        without_inventory_body = $missingPositiveWithoutBody.Count
        by_project = Group-Counts $missingPositive "project"
        cause = "These methods have source bodies, but every generated row for them has been source/version blocked. This is a selection-policy gap, not a missing-code-resource gap."
        missing_resource = "targeted positive D2 leaf rows for these exact symbols"
        top_symbols = [object[]]@($missingPositive |
            Sort-Object total_appearances -Descending |
            Select-Object -First 25 project, symbol, total_appearances, source_negative_rows, version_negative_rows)
    }
    d4_or_workflow_level = [ordered]@{
        row_symbol_missing_d4_total = $missingD4.Count
        has_outgoing_calls = $missingD4HasOutgoing.Count
        no_outgoing_calls = $missingD4NoOutgoing.Count
        already_propagated_as_known_callee_in_d4 = $missingD4AlreadyPropagatedAsCallee.Count
        no_known_callee_propagation_yet = $missingD4NoCalleePropagation.Count
        by_project = Group-Counts $missingD4 "project"
        cause = "The current audit counts D4 only when the method is the row symbol. For leaf/callee methods this overstates the gap because bottom-up coverage should also count known_callees inside caller D4 rows."
        missing_resource = "audit metric needs callee-propagation coverage; remaining true gaps need incoming caller/workflow rows or richer static call extraction"
        top_true_missing = [object[]]@($missingD4NoCalleePropagation |
            Sort-Object total_appearances -Descending |
            Select-Object -First 25 project, symbol, total_appearances, positive_ready_rows)
    }
    missing_callee_negative_edges = [ordered]@{
        total = $missingEdgeNegative.Count
        single_callee_callers = @($edgeNegativeRows | Where-Object { [int]$_["distinct_callee_count"] -le 1 }).Count
        multi_callee_callers = @($edgeNegativeRows | Where-Object { [int]$_["distinct_callee_count"] -gt 1 }).Count
        by_project = Group-Counts $missingEdgeNegative "project"
        cause = "The generator creates transit negatives by rotating modes, not by reading uncovered edges. Single-callee callers especially require explicit empty-transit negative rows."
        missing_resource = "targeted empty-transit negatives for single-callee callers and targeted partial-transit negatives for multi-callee callers"
        top_edges = [object[]]@($edgeNegativeRows |
            Sort-Object total_appearances -Descending |
            Select-Object -First 25)
    }
    required_protocol_change = [object[]]@(
        "Before each 10-batch interval, read METHOD-DEPTH-COVERAGE.jsonl and EDGE-DEPTH-COVERAGE.jsonl.",
        "Reserve part of each 50-row batch for exact uncovered symbols/edges instead of relying on modulo rotation.",
        "Add targeted positive rows for methods with positive_ready_rows == 0.",
        "Count leaf/callee propagation through known_callees in D4 caller rows; do not require leaf methods to become row-symbol D4.",
        "Add targeted empty-transit negatives for single-callee edge gaps.",
        "Add targeted partial-transit negatives for multi-callee edge gaps.",
        "If incoming caller/workflow context is missing, improve static call extraction or add workflow/lifecycle rows from allowed MIT/Apache sources."
    )
}

Set-Content -LiteralPath (Join-Path $outDir "DEPTH-GAP-DIAGNOSIS.json") -Value ($diagnosis | ConvertTo-Json -Depth 80) -Encoding UTF8
Set-Content -LiteralPath (Join-Path $outDir "DEPTH-GAP-DIAGNOSIS.md") -Value @"
# Code Documentation Depth Gap Diagnosis

Generated: $($diagnosis.generated_at)

## Headline

$($diagnosis.headline)

## Missing Positive-Ready Method Rows

- Total: $($diagnosis.missing_positive_ready.total)
- With inventory body: $($diagnosis.missing_positive_ready.with_inventory_body)
- Without inventory body: $($diagnosis.missing_positive_ready.without_inventory_body)

Cause: $($diagnosis.missing_positive_ready.cause)

Missing resource/change: $($diagnosis.missing_positive_ready.missing_resource)

## D4 / Workflow-Level Method Rows

- Row-symbol D4 gaps: $($diagnosis.d4_or_workflow_level.row_symbol_missing_d4_total)
- Have outgoing calls: $($diagnosis.d4_or_workflow_level.has_outgoing_calls)
- No outgoing calls: $($diagnosis.d4_or_workflow_level.no_outgoing_calls)
- Already propagated as known callee in D4 rows: $($diagnosis.d4_or_workflow_level.already_propagated_as_known_callee_in_d4)
- No known callee propagation yet: $($diagnosis.d4_or_workflow_level.no_known_callee_propagation_yet)

Cause: $($diagnosis.d4_or_workflow_level.cause)

Missing resource/change: $($diagnosis.d4_or_workflow_level.missing_resource)

## Missing-Callee Negative Edge Rows

- Total: $($diagnosis.missing_callee_negative_edges.total)
- Single-callee callers: $($diagnosis.missing_callee_negative_edges.single_callee_callers)
- Multi-callee callers: $($diagnosis.missing_callee_negative_edges.multi_callee_callers)

Cause: $($diagnosis.missing_callee_negative_edges.cause)

Missing resource/change: $($diagnosis.missing_callee_negative_edges.missing_resource)

## Required Protocol Change

$(@($diagnosis.required_protocol_change) | ForEach-Object { "- $_" } | Out-String)
"@ -Encoding UTF8

Write-Host ("gap diagnosis missing_positive={0} d4_row_symbol={1} d4_true_no_callee_propagation={2} edge_negative={3}" -f $diagnosis.missing_positive_ready.total, $diagnosis.d4_or_workflow_level.row_symbol_missing_d4_total, $diagnosis.d4_or_workflow_level.no_known_callee_propagation_yet, $diagnosis.missing_callee_negative_edges.total)
