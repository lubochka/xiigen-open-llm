$ErrorActionPreference = "Stop"

$root = "C:\Users\lubao\Documents\xiigen\llm mvp\documentation learning\documentation-runs\dotnet-csharp-callgraph-extraction"
$methodPath = Join-Path $root "extracted-method-inventory.jsonl"
$edgePath = Join-Path $root "extracted-callgraph-edges.jsonl"
$outDir = Join-Path $root "coverage-audit"
New-Item -ItemType Directory -Force -Path $outDir | Out-Null

function JsonLine($obj) {
    return ($obj | ConvertTo-Json -Depth 50 -Compress)
}

function AddCount($map, [string]$key) {
    if ([string]::IsNullOrWhiteSpace($key)) {
        return
    }
    if (!$map.ContainsKey($key)) {
        $map[$key] = 0
    }
    $map[$key] = [int]$map[$key] + 1
}

$methods = @(Get-Content -LiteralPath $methodPath | Where-Object { $_.Trim().Length -gt 0 } | ForEach-Object { $_ | ConvertFrom-Json })
$edges = @(Get-Content -LiteralPath $edgePath | Where-Object { $_.Trim().Length -gt 0 } | ForEach-Object { $_ | ConvertFrom-Json })

$symbolStats = @{}
foreach ($m in $methods) {
    $symbol = [string]$m.symbol
    $symbolStats[$symbol] = [ordered]@{
        project                 = [string]$m.project
        symbol                  = $symbol
        total_appearances       = 0
        positive_ready_rows     = 0
        blocked_rows            = 0
        source_negative_rows    = 0
        version_negative_rows   = 0
        transit_negative_rows   = 0
        d2_rows                 = 0
        d4_rows                 = 0
        callee_propagation_rows = 0
        framework_rows          = 0
        thin_template_rows      = 0
    }
}

$edgeStats = @{}
foreach ($e in $edges) {
    $key = "$($e.caller_symbol)->$($e.callee_symbol)"
    $edgeStats[$key] = [ordered]@{
        project                 = [string]$e.project
        edge                    = $key
        kind                    = if ($e.call_kind) { [string]$e.call_kind } else { "" }
        total_appearances       = 0
        positive_ready_rows     = 0
        transit_negative_rows   = 0
        framework_rows          = 0
    }
}

$batchDirs = @(Get-ChildItem -LiteralPath $root -Directory | Where-Object { $_.Name -match '^batch-\d{3}$' } | Sort-Object Name)
foreach ($batchDir in $batchDirs) {
    foreach ($path in @((Join-Path $batchDir.FullName "TRAIN-PAIRS.jsonl"), (Join-Path $batchDir.FullName "HELDOUT-ROWS.jsonl"))) {
        if (!(Test-Path $path)) {
            continue
        }
        foreach ($line in @(Get-Content -LiteralPath $path | Where-Object { $_.Trim().Length -gt 0 })) {
            $row = $line | ConvertFrom-Json
            $symbol = [string]$row.symbol_id
            $artifact = if ($row.artifact_kind) { [string]$row.artifact_kind } else { "" }
            $doc = if ($row.documentation_level) { [string]$row.documentation_level } else { "" }
            $sourcePass = [bool]$row.expect_source_gate_pass
            $versionPass = [bool]$row.expect_version_gate_pass
            $transitPass = [bool]$row.expect_transit_gate_pass

            if ($symbolStats.ContainsKey($symbol)) {
                $symbolStats[$symbol].total_appearances++
                if ($sourcePass -and $versionPass -and $transitPass) {
                    $symbolStats[$symbol].positive_ready_rows++
                }
                else {
                    $symbolStats[$symbol].blocked_rows++
                }
                if (!$sourcePass) {
                    $symbolStats[$symbol].source_negative_rows++
                }
                if (!$versionPass) {
                    $symbolStats[$symbol].version_negative_rows++
                }
                if (!$transitPass) {
                    $symbolStats[$symbol].transit_negative_rows++
                }
                if ($doc -eq "D2") {
                    $symbolStats[$symbol].d2_rows++
                }
                if ($doc -eq "D4") {
                    $symbolStats[$symbol].d4_rows++
                }
                if ($artifact -match "framework_lifecycle") {
                    $symbolStats[$symbol].framework_rows++
                }
                if ($artifact -match "thin_template") {
                    $symbolStats[$symbol].thin_template_rows++
                }
            }

            $knownCalleeSet = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
            foreach ($known in @($row.known_callees)) {
                $knownSymbol = [string]$known.symbol_id
                if ([string]::IsNullOrWhiteSpace($knownSymbol)) {
                    continue
                }
                [void]$knownCalleeSet.Add($knownSymbol)
                if ($doc -eq "D4" -and $sourcePass -and $versionPass -and $transitPass -and $symbolStats.ContainsKey($knownSymbol)) {
                    $symbolStats[$knownSymbol].callee_propagation_rows++
                }
            }

            foreach ($call in @($row.calls)) {
                $edge = "$($call.caller_symbol_id)->$($call.callee_symbol_id)"
                if ($edgeStats.ContainsKey($edge)) {
                    $calleeSymbol = [string]$call.callee_symbol_id
                    $edgeStats[$edge].total_appearances++
                    if ($sourcePass -and $versionPass -and $transitPass) {
                        $edgeStats[$edge].positive_ready_rows++
                    }
                    if (!$transitPass -and !$knownCalleeSet.Contains($calleeSymbol)) {
                        $edgeStats[$edge].transit_negative_rows++
                    }
                    if ($artifact -match "framework_lifecycle") {
                        $edgeStats[$edge].framework_rows++
                    }
                }
            }
        }
    }
}

$symbolRows = @($symbolStats.Values)
$edgeRows = @($edgeStats.Values)

function MethodGapReason($row) {
    if ($row.positive_ready_rows -eq 0) {
        return "needs_positive_ready_documentation_examples"
    }
    if ($row.blocked_rows -eq 0) {
        return "needs_negative_gate_examples"
    }
    if ($row.d4_rows -eq 0 -and $row.callee_propagation_rows -eq 0) {
        return "needs_caller_or_workflow_level_examples"
    }
    return "sufficient_for_current_level"
}

function EdgeGapReason($row) {
    if ($row.positive_ready_rows -eq 0) {
        return "needs_positive_edge_transit_examples"
    }
    if ($row.transit_negative_rows -eq 0) {
        return "needs_missing_callee_negative_examples"
    }
    return "sufficient_for_current_level"
}

$methodDepthRows = @($symbolRows | ForEach-Object {
        [ordered]@{
            project               = $_.project
            symbol                = $_.symbol
            total_appearances     = $_.total_appearances
            positive_ready_rows   = $_.positive_ready_rows
            blocked_rows          = $_.blocked_rows
            source_negative_rows  = $_.source_negative_rows
            version_negative_rows = $_.version_negative_rows
            transit_negative_rows = $_.transit_negative_rows
            d2_rows               = $_.d2_rows
            d4_rows               = $_.d4_rows
            callee_propagation_rows = $_.callee_propagation_rows
            framework_rows        = $_.framework_rows
            thin_template_rows    = $_.thin_template_rows
            gap_reason            = MethodGapReason $_
        }
    })

$edgeDepthRows = @($edgeRows | ForEach-Object {
        [ordered]@{
            project               = $_.project
            edge                  = $_.edge
            kind                  = $_.kind
            total_appearances     = $_.total_appearances
            positive_ready_rows   = $_.positive_ready_rows
            transit_negative_rows = $_.transit_negative_rows
            framework_rows        = $_.framework_rows
            gap_reason            = EdgeGapReason $_
        }
    })

$projectRows = New-Object System.Collections.ArrayList
foreach ($group in @($methodDepthRows | Group-Object project)) {
    $project = $group.Name
    $mRows = @($group.Group)
    $eRows = @($edgeDepthRows | Where-Object { $_.project -eq $project })
    [void]$projectRows.Add([ordered]@{
            project                           = $project
            methods_total                     = $mRows.Count
            methods_with_positive_ready       = @($mRows | Where-Object { $_.positive_ready_rows -gt 0 }).Count
            methods_missing_positive_ready    = @($mRows | Where-Object { $_.positive_ready_rows -eq 0 }).Count
            methods_with_negative_gate        = @($mRows | Where-Object { $_.blocked_rows -gt 0 }).Count
            methods_missing_negative_gate     = @($mRows | Where-Object { $_.blocked_rows -eq 0 }).Count
            methods_with_d4                   = @($mRows | Where-Object { $_.d4_rows -gt 0 }).Count
            methods_with_callee_propagation   = @($mRows | Where-Object { $_.callee_propagation_rows -gt 0 }).Count
            methods_missing_d4                = @($mRows | Where-Object { $_.d4_rows -eq 0 -and $_.callee_propagation_rows -eq 0 }).Count
            edges_total                       = $eRows.Count
            edges_with_positive_ready         = @($eRows | Where-Object { $_.positive_ready_rows -gt 0 }).Count
            edges_missing_positive_ready      = @($eRows | Where-Object { $_.positive_ready_rows -eq 0 }).Count
            edges_with_transit_negative       = @($eRows | Where-Object { $_.transit_negative_rows -gt 0 }).Count
            edges_missing_transit_negative    = @($eRows | Where-Object { $_.transit_negative_rows -eq 0 }).Count
        })
}

$summary = [ordered]@{
    generated_at                         = (Get-Date).ToString("o")
    batches_scanned                      = $batchDirs.Count
    methods_total                        = $methodDepthRows.Count
    methods_missing_positive_ready       = @($methodDepthRows | Where-Object { $_.positive_ready_rows -eq 0 }).Count
    methods_missing_negative_gate        = @($methodDepthRows | Where-Object { $_.blocked_rows -eq 0 }).Count
    methods_missing_d4                   = @($methodDepthRows | Where-Object { $_.d4_rows -eq 0 -and $_.callee_propagation_rows -eq 0 }).Count
    methods_with_callee_propagation      = @($methodDepthRows | Where-Object { $_.callee_propagation_rows -gt 0 }).Count
    edges_total                          = $edgeDepthRows.Count
    edges_missing_positive_ready         = @($edgeDepthRows | Where-Object { $_.positive_ready_rows -eq 0 }).Count
    edges_missing_transit_negative       = @($edgeDepthRows | Where-Object { $_.transit_negative_rows -eq 0 }).Count
    project_depth_coverage               = [object[]]$projectRows
    missing_resource_groups              = [object[]]@(
        "methods_missing_positive_ready need more accepted documentation rows",
        "methods_missing_negative_gate need source/version/transit rejected examples",
        "methods_missing_d4 need caller/workflow documentation examples",
        "edges_missing_positive_ready need more accepted caller-callee propagation rows",
        "edges_missing_transit_negative need missing-callee rejected examples",
        "legacy UI/lifecycle areas may need more MIT/Apache WebForms/MVC framework sources"
    )
}

Set-Content -LiteralPath (Join-Path $outDir "DEPTH-COVERAGE-SUMMARY.json") -Value ($summary | ConvertTo-Json -Depth 50) -Encoding UTF8
Set-Content -LiteralPath (Join-Path $outDir "METHOD-DEPTH-COVERAGE.jsonl") -Value (@($methodDepthRows | ForEach-Object { JsonLine $_ })) -Encoding UTF8
Set-Content -LiteralPath (Join-Path $outDir "EDGE-DEPTH-COVERAGE.jsonl") -Value (@($edgeDepthRows | ForEach-Object { JsonLine $_ })) -Encoding UTF8

Set-Content -LiteralPath (Join-Path $outDir "DEPTH-COVERAGE-REPORT.md") -Value @"
# Code Documentation Depth Coverage Audit

Generated: $($summary.generated_at)

## Method Depth

- Methods total: $($summary.methods_total)
- Methods missing positive-ready examples: $($summary.methods_missing_positive_ready)
- Methods missing negative gate examples: $($summary.methods_missing_negative_gate)
- Methods missing D4 caller/workflow examples: $($summary.methods_missing_d4)
- Methods covered as propagated callees in valid D4 rows: $($summary.methods_with_callee_propagation)

## Edge Depth

- Edges total: $($summary.edges_total)
- Edges missing positive-ready propagation examples: $($summary.edges_missing_positive_ready)
- Edges missing missing-callee negative examples: $($summary.edges_missing_transit_negative)

## Interpretation

This audit is stricter than inventory coverage. Inventory coverage only asks
whether a method or edge appeared at least once. Depth coverage asks whether it
has enough positive, negative, and caller/workflow examples to be robust.

D4 caller/workflow coverage now also counts leaf/callee methods that are
propagated upward through `known_callees[]` in a valid D4 caller row. A leaf
method does not need to become the row symbol of a D4 example just to count as
bottom-up propagated.

See `METHOD-DEPTH-COVERAGE.jsonl` and `EDGE-DEPTH-COVERAGE.jsonl` for exact
symbols and gap reasons.
"@ -Encoding UTF8

Write-Host ("depth audit method_missing_positive={0} method_missing_negative={1} method_missing_d4={2} edge_missing_positive={3} edge_missing_negative={4}" -f $summary.methods_missing_positive_ready, $summary.methods_missing_negative_gate, $summary.methods_missing_d4, $summary.edges_missing_positive_ready, $summary.edges_missing_transit_negative)
