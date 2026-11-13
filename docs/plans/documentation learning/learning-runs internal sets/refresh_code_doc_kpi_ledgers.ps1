$ErrorActionPreference = "Stop"

$streams = @(
    "C:\Users\lubao\Documents\xiigen\llm mvp\documentation learning\documentation-runs\external-dotnet-framework-lifecycle-source-proof",
    "C:\Users\lubao\Documents\xiigen\llm mvp\documentation learning\documentation-runs\dotnet-csharp-callgraph-extraction"
)

function JsonLine($obj) {
    return ($obj | ConvertTo-Json -Depth 30 -Compress)
}

foreach ($stream in $streams) {
    if (!(Test-Path $stream)) {
        continue
    }

    $rows = New-Object System.Collections.ArrayList
    foreach ($batchDir in @(Get-ChildItem -LiteralPath $stream -Directory | Where-Object { $_.Name -match '^batch-\d{3}$' } | Sort-Object Name)) {
        $metricsPath = Join-Path $batchDir.FullName "TRAINING-METRICS.json"
        if (!(Test-Path $metricsPath)) {
            continue
        }

        $metrics = Get-Content -LiteralPath $metricsPath -Raw | ConvertFrom-Json
        $batchNumber = [int]($batchDir.Name -replace '^batch-', '')
        [void]$rows.Add([ordered]@{
                batch               = $batchDir.Name
                rows_trained        = 50
                rows_heldout        = 50
                routing_accuracy    = $metrics.routing_accuracy
                source_accuracy     = $metrics.source_gate_accuracy
                version_accuracy    = $metrics.version_gate_accuracy
                transit_accuracy    = $metrics.transit_gate_accuracy
                bottom_up_accuracy  = $metrics.bottom_up_order_accuracy
                callee_accuracy     = $metrics.callee_summary_transit_accuracy
                local_vs_delegated  = $metrics.local_vs_delegated_accuracy
                cpu_ms              = $metrics.cpu_ms
                peak_ram_mb         = $metrics.peak_ram_mb
                elapsed_ms          = $metrics.elapsed_ms
                documentation_check = ($batchNumber % 10 -eq 0)
                recorded_at         = (Get-Date).ToString("o")
            })
    }

    Set-Content -LiteralPath (Join-Path $stream "KPI-LEDGER.jsonl") -Value (@($rows) | ForEach-Object { JsonLine $_ }) -Encoding UTF8
    Write-Host ("refreshed {0} rows={1}" -f $stream, $rows.Count)
}
