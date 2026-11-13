$ErrorActionPreference = "Stop"

$root = "C:\Users\lubao\Documents\xiigen\llm mvp\documentation learning\documentation-runs\dotnet-csharp-callgraph-extraction"
$methodPath = Join-Path $root "extracted-method-inventory.jsonl"
$edgePath = Join-Path $root "extracted-callgraph-edges.jsonl"
$outDir = Join-Path $root "coverage-audit"
New-Item -ItemType Directory -Force -Path $outDir | Out-Null

function JsonLine($obj) {
    return ($obj | ConvertTo-Json -Depth 40 -Compress)
}

function NormSymbol($value) {
    if ($null -eq $value) {
        return ""
    }
    return [string]$value
}

$methods = @(Get-Content -LiteralPath $methodPath | Where-Object { $_.Trim().Length -gt 0 } | ForEach-Object { $_ | ConvertFrom-Json })
$edges = @(Get-Content -LiteralPath $edgePath | Where-Object { $_.Trim().Length -gt 0 } | ForEach-Object { $_ | ConvertFrom-Json })

$methodKeys = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
foreach ($m in $methods) {
    [void]$methodKeys.Add((NormSymbol $m.symbol))
}

$coveredSymbols = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
$coveredEdges = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
$coverageFiles = @(Get-ChildItem -LiteralPath $root -Directory |
    Where-Object { $_.Name -match '^batch-\d{3}$' } |
    ForEach-Object {
        @(
            Join-Path $_.FullName "TRAIN-PAIRS.jsonl"
            Join-Path $_.FullName "HELDOUT-ROWS.jsonl"
        )
    } |
    Where-Object { Test-Path $_ })

foreach ($file in $coverageFiles) {
    foreach ($line in @(Get-Content -LiteralPath $file | Where-Object { $_.Trim().Length -gt 0 })) {
        $row = $line | ConvertFrom-Json
        $symbol = NormSymbol $row.symbol_id
        if ($symbol.Length -gt 0) {
            [void]$coveredSymbols.Add($symbol)
        }
        foreach ($call in @($row.calls)) {
            $caller = NormSymbol $call.caller_symbol_id
            $callee = NormSymbol $call.callee_symbol_id
            if ($caller.Length -gt 0 -and $callee.Length -gt 0) {
                [void]$coveredEdges.Add("$caller->$callee")
            }
        }
    }
}

$edgeKeys = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
foreach ($e in $edges) {
    [void]$edgeKeys.Add("$(NormSymbol $e.caller_symbol)->$(NormSymbol $e.callee_symbol)")
}

$edgesByCaller = @{}
foreach ($e in $edges) {
    $caller = NormSymbol $e.caller_symbol
    if (!$edgesByCaller.ContainsKey($caller)) {
        $edgesByCaller[$caller] = New-Object System.Collections.ArrayList
    }
    [void]$edgesByCaller[$caller].Add($e)
}

function ReasonForMethod($m) {
    $symbol = NormSymbol $m.symbol
    $project = [string]$m.project
    $artifact = if ($m.artifact_kind) { [string]$m.artifact_kind } else { "" }
    $body = if ($m.body) { [string]$m.body } else { "" }
    $edgeCount = if ($edgesByCaller.ContainsKey($symbol)) { @($edgesByCaller[$symbol]).Count } else { 0 }

    if ($coveredSymbols.Contains($symbol)) {
        return "covered"
    }
    if ($artifact -match "framework_lifecycle|expression_bodied|controller_action") {
        return "not_selected_yet_framework_lifecycle_pool"
    }
    if ($edgeCount -eq 0) {
        if ($body.Length -lt 80) {
            return "leaf_small_body_not_sampled_yet"
        }
        return "leaf_no_local_call_edges_not_sampled_yet"
    }
    if ($edgeCount -eq 1) {
        return "single_edge_caller_needs_positive_transit_sampling"
    }
    return "multi_edge_caller_needs_more_transit_sampling"
}

function ReasonForEdge($e) {
    $key = "$(NormSymbol $e.caller_symbol)->$(NormSymbol $e.callee_symbol)"
    if ($coveredEdges.Contains($key)) {
        return "covered"
    }
    $kind = if ($e.call_kind) { [string]$e.call_kind } else { "" }
    if ($kind -match "framework_lifecycle|aspnetcore|endpoint|controller_activation|response_serialization") {
        return "framework_lifecycle_edge_not_sampled_yet"
    }
    if ($kind -match "direct_method_name_match") {
        return "direct_call_edge_not_sampled_yet"
    }
    return "edge_not_sampled_yet"
}

$methodRows = New-Object System.Collections.ArrayList
foreach ($m in $methods) {
    $symbol = NormSymbol $m.symbol
    [void]$methodRows.Add([ordered]@{
            project      = [string]$m.project
            symbol       = $symbol
            file         = if ($m.relative) { [string]$m.relative } else { [string]$m.file }
            covered      = $coveredSymbols.Contains($symbol)
            reason       = ReasonForMethod $m
            edge_count   = if ($edgesByCaller.ContainsKey($symbol)) { @($edgesByCaller[$symbol]).Count } else { 0 }
            artifact_kind = if ($m.artifact_kind) { [string]$m.artifact_kind } else { "" }
        })
}

$edgeRows = New-Object System.Collections.ArrayList
foreach ($e in $edges) {
    $caller = NormSymbol $e.caller_symbol
    $callee = NormSymbol $e.callee_symbol
    $key = "$caller->$callee"
    [void]$edgeRows.Add([ordered]@{
            project = [string]$e.project
            caller  = $caller
            callee  = $callee
            covered = $coveredEdges.Contains($key)
            reason  = ReasonForEdge $e
            kind    = if ($e.call_kind) { [string]$e.call_kind } else { "" }
        })
}

$projectRows = New-Object System.Collections.ArrayList
foreach ($group in @($methodRows | Group-Object project)) {
    $project = $group.Name
    $projectMethods = @($group.Group)
    $projectEdges = @($edgeRows | Where-Object { $_.project -eq $project })
    $coveredMethodCount = @($projectMethods | Where-Object { $_.covered }).Count
    $coveredEdgeCount = @($projectEdges | Where-Object { $_.covered }).Count
    $methodReasons = @($projectMethods | Where-Object { -not $_.covered } | Group-Object reason | ForEach-Object {
            [ordered]@{ reason = $_.Name; count = $_.Count }
        })
    $edgeReasons = @($projectEdges | Where-Object { -not $_.covered } | Group-Object reason | ForEach-Object {
            [ordered]@{ reason = $_.Name; count = $_.Count }
        })
    [void]$projectRows.Add([ordered]@{
            project                 = $project
            methods_total           = $projectMethods.Count
            methods_covered         = $coveredMethodCount
            methods_uncovered       = $projectMethods.Count - $coveredMethodCount
            method_coverage_ratio   = if ($projectMethods.Count -eq 0) { 0 } else { [Math]::Round($coveredMethodCount / [double]$projectMethods.Count, 6) }
            edges_total             = $projectEdges.Count
            edges_covered           = $coveredEdgeCount
            edges_uncovered         = $projectEdges.Count - $coveredEdgeCount
            edge_coverage_ratio     = if ($projectEdges.Count -eq 0) { 0 } else { [Math]::Round($coveredEdgeCount / [double]$projectEdges.Count, 6) }
            uncovered_method_reasons = [object[]]$methodReasons
            uncovered_edge_reasons   = [object[]]$edgeReasons
        })
}

$summary = [ordered]@{
    generated_at            = (Get-Date).ToString("o")
    batches_scanned         = @($coverageFiles | ForEach-Object { Split-Path -Parent $_ } | Sort-Object -Unique).Count
    method_total            = $methods.Count
    method_covered          = @($methodRows | Where-Object { $_.covered }).Count
    method_uncovered        = @($methodRows | Where-Object { -not $_.covered }).Count
    method_coverage_ratio   = [Math]::Round((@($methodRows | Where-Object { $_.covered }).Count / [double]$methods.Count), 6)
    edge_total              = $edges.Count
    edge_covered            = @($edgeRows | Where-Object { $_.covered }).Count
    edge_uncovered          = @($edgeRows | Where-Object { -not $_.covered }).Count
    edge_coverage_ratio     = [Math]::Round((@($edgeRows | Where-Object { $_.covered }).Count / [double]$edges.Count), 6)
    project_coverage        = [object[]]$projectRows
    missing_resource_groups = [object[]]@(
        "more direct sampling for leaf methods with no call edges",
        "more positive transit sampling for single-edge callers",
        "more multi-callee negative/positive transit sampling",
        "more framework lifecycle examples for thin ASP.NET Core projects",
        "more external MIT/Apache framework authority for ASP.NET 4.x and WebForms lifecycle"
    )
}

Set-Content -LiteralPath (Join-Path $outDir "COVERAGE-AUDIT-SUMMARY.json") -Value ($summary | ConvertTo-Json -Depth 50) -Encoding UTF8
Set-Content -LiteralPath (Join-Path $outDir "UNCOVERED-METHODS.jsonl") -Value (@($methodRows | Where-Object { -not $_.covered } | ForEach-Object { JsonLine $_ })) -Encoding UTF8
Set-Content -LiteralPath (Join-Path $outDir "UNCOVERED-EDGES.jsonl") -Value (@($edgeRows | Where-Object { -not $_.covered } | ForEach-Object { JsonLine $_ })) -Encoding UTF8
Set-Content -LiteralPath (Join-Path $outDir "COVERAGE-AUDIT-REPORT.md") -Value @"
# Code Documentation Inventory Coverage Audit

Generated: $($summary.generated_at)

## Totals

- Methods: $($summary.method_covered) / $($summary.method_total) covered
- Uncovered methods: $($summary.method_uncovered)
- Method coverage ratio: $($summary.method_coverage_ratio)
- Edges: $($summary.edge_covered) / $($summary.edge_total) covered
- Uncovered edges: $($summary.edge_uncovered)
- Edge coverage ratio: $($summary.edge_coverage_ratio)

## Why Things Remain Uncovered

The remaining gaps are not one thing. They split into:

- leaf methods with no local call edges that have not yet been sampled;
- single-edge callers needing more positive transit sampling;
- multi-edge callers needing more complete/incomplete transit pairs;
- framework lifecycle edges that need more thin-project sampling;
- ASP.NET 4.x/WebForms lifecycle requiring more external MIT/Apache authority.

See `UNCOVERED-METHODS.jsonl` and `UNCOVERED-EDGES.jsonl` for exact symbols.
"@ -Encoding UTF8

Write-Host ("coverage audit methods={0}/{1} edges={2}/{3} report={4}" -f $summary.method_covered, $summary.method_total, $summary.edge_covered, $summary.edge_total, (Join-Path $outDir "COVERAGE-AUDIT-REPORT.md"))
