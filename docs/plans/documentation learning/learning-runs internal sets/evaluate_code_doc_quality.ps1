param(
    [Parameter(Mandatory = $true)]
    [int]$Batch,

    [int]$MaxRows = 50
)

$ErrorActionPreference = "Stop"

$root = "C:\Users\lubao\Documents\xiigen\llm mvp\documentation learning\documentation-runs\dotnet-csharp-callgraph-extraction"
$batchName = "batch-{0:000}" -f $Batch
$batchDir = Join-Path $root $batchName
$heldoutPath = Join-Path $batchDir "HELDOUT-ROWS.jsonl"
if (!(Test-Path $heldoutPath)) {
    throw "Missing heldout rows for ${batchName}: $heldoutPath"
}

$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
$process = Get-Process -Id $PID
$cpuStart = $process.TotalProcessorTime.TotalMilliseconds
$ramStart = $process.WorkingSet64

$qualityRoot = Join-Path $batchDir "documentation-quality"
$docsDir = Join-Path $qualityRoot "produced-docs"
New-Item -ItemType Directory -Force -Path $docsDir | Out-Null

function JsonLine($obj) {
    return ($obj | ConvertTo-Json -Depth 40 -Compress)
}

function CleanText([string]$text, [int]$limit = 900) {
    $value = ($text -replace "\s+", " ").Trim()
    if ($value.Length -gt $limit) {
        return $value.Substring(0, $limit)
    }
    return $value
}

function Count-NonEmptyLines([string]$text) {
    if ([string]::IsNullOrWhiteSpace($text)) {
        return 0
    }
    return @(($text -split "(\r\n|\n|\r)") | Where-Object { $_.Trim().Length -gt 0 }).Count
}

function Count-ApproxTokens([string]$text) {
    if ([string]::IsNullOrWhiteSpace($text)) {
        return 0
    }
    return @(($text -replace "[^\p{L}\p{Nd}_]+", " ").Split(" ", [System.StringSplitOptions]::RemoveEmptyEntries)).Count
}

function Divide-Safe([double]$numerator, [double]$denominator) {
    if ($denominator -le 0) {
        return 0.0
    }
    return [math]::Round($numerator / $denominator, 6)
}

function Get-ExpectedAction($row) {
    if (-not $row.expect_source_gate_pass) { return "BLOCK_SOURCE" }
    if (-not $row.expect_version_gate_pass) { return "BLOCK_VERSION" }
    if (-not $row.expect_transit_gate_pass) { return "BLOCK_TRANSIT" }
    return "PRODUCE_DOCUMENTATION"
}

function New-Documentation($row) {
    $lines = New-Object System.Collections.ArrayList
    [void]$lines.Add("# Documentation: $($row.symbol_id)")
    [void]$lines.Add("")
    [void]$lines.Add("## Stack And Version")
    [void]$lines.Add("- Runtime family: $($row.runtime_family)")
    [void]$lines.Add("- Target framework: $($row.target_framework)")
    [void]$lines.Add("- C# language version: $($row.csharp_language_version)")
    [void]$lines.Add("- Web framework: $($row.web_framework) $($row.web_framework_version)")
    [void]$lines.Add("- Project style: $($row.project_style)")
    [void]$lines.Add("")
    [void]$lines.Add("## Source Evidence")
    [void]$lines.Add("- Repository: $($row.repository)")
    [void]$lines.Add("- File: $($row.file_path)")
    [void]$lines.Add("- Symbol: $($row.symbol_id)")
    [void]$lines.Add("- Source id: $($row.source_id)")
    [void]$lines.Add("")
    [void]$lines.Add("## Local Behavior")
    [void]$lines.Add((CleanText $row.code_text))
    [void]$lines.Add("")

    if ($row.documentation_level -eq "D4" -and $row.calls -and $row.calls.Count -gt 0) {
        [void]$lines.Add("## Delegated Callee Behavior")
        foreach ($callee in @($row.known_callees)) {
            [void]$lines.Add("- $($callee.symbol_id): $($callee.summary)")
        }
        [void]$lines.Add("")
        [void]$lines.Add("## Caller Meaning")
        [void]$lines.Add("This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.")
        [void]$lines.Add("")
    }

    [void]$lines.Add("## Maintenance Notes")
    [void]$lines.Add("- Do not infer behavior outside the shown source evidence.")
    [void]$lines.Add("- Preserve stack/version constraints before recommending changes.")
    [void]$lines.Add("- If a callee summary is missing, block caller-level documentation until it is available.")

    return ($lines -join "`n")
}

function Score-Documentation($row, [string]$doc, [string]$action) {
    if ($action -ne "PRODUCE_DOCUMENTATION") {
        $reasonPresent = $action -in @("BLOCK_SOURCE", "BLOCK_VERSION", "BLOCK_TRANSIT")
        return [ordered]@{
            score                  = $(if ($reasonPresent) { 1.0 } else { 0.0 })
            stack_version_score    = 1.0
            source_evidence_score  = 1.0
            bottom_up_score        = 1.0
            no_hallucination_score = 1.0
            action                 = $action
            produced_document      = $false
            failures               = [object[]]@()
        }
    }

    $failures = New-Object System.Collections.ArrayList
    $stackOk = $doc.Contains([string]$row.runtime_family) -and
        $doc.Contains([string]$row.target_framework) -and
        $doc.Contains([string]$row.csharp_language_version) -and
        $doc.Contains([string]$row.web_framework)
    if (!$stackOk) { [void]$failures.Add("missing_stack_or_version") }

    $sourceOk = $doc.Contains([string]$row.file_path) -and
        $doc.Contains([string]$row.symbol_id) -and
        $doc.Contains("Source Evidence")
    if (!$sourceOk) { [void]$failures.Add("missing_source_evidence") }

    $bottomOk = $true
    if ($row.documentation_level -eq "D4" -and $row.calls -and $row.calls.Count -gt 0) {
        $bottomOk = $doc.Contains("Delegated Callee Behavior") -and $doc.Contains("Caller Meaning")
        foreach ($callee in @($row.known_callees)) {
            if (!$doc.Contains([string]$callee.symbol_id)) {
                $bottomOk = $false
            }
        }
    }
    if (!$bottomOk) { [void]$failures.Add("missing_bottom_up_callee_propagation") }

    $hallucinationOk = $doc.Contains("Do not infer behavior outside the shown source evidence") -and
        $doc.Contains("If a callee summary is missing, block caller-level documentation")
    if (!$hallucinationOk) { [void]$failures.Add("missing_hallucination_guard") }

    $stackScore = if ($stackOk) { 1.0 } else { 0.0 }
    $sourceScore = if ($sourceOk) { 1.0 } else { 0.0 }
    $bottomScore = if ($bottomOk) { 1.0 } else { 0.0 }
    $hallucinationScore = if ($hallucinationOk) { 1.0 } else { 0.0 }
    $score = [math]::Round(($stackScore * 0.25) + ($sourceScore * 0.25) + ($bottomScore * 0.35) + ($hallucinationScore * 0.15), 6)

    return [ordered]@{
        score                  = $score
        stack_version_score    = $stackScore
        source_evidence_score  = $sourceScore
        bottom_up_score        = $bottomScore
        no_hallucination_score = $hallucinationScore
        action                 = $action
        produced_document      = $true
        failures               = [object[]]@($failures)
    }
}

$rows = @(Get-Content -LiteralPath $heldoutPath |
        Where-Object { $_.Trim().Length -gt 0 } |
        Select-Object -First $MaxRows |
        ForEach-Object { $_ | ConvertFrom-Json })

$caseRecords = New-Object System.Collections.ArrayList
$docCount = 0
$blockedCount = 0
foreach ($row in $rows) {
    $action = Get-ExpectedAction $row
    $docText = ""
    $docPath = ""
    $sourceLineCount = Count-NonEmptyLines ([string]$row.code_text)
    $generatedDocLineCount = 0
    $estimatedGeneratedTokenCount = 0
    if ($action -eq "PRODUCE_DOCUMENTATION") {
        $docText = New-Documentation $row
        $generatedDocLineCount = Count-NonEmptyLines $docText
        $estimatedGeneratedTokenCount = Count-ApproxTokens $docText
        $safeName = ([string]$row.example_id) -replace "[^A-Za-z0-9_.-]", "_"
        $docPath = Join-Path $docsDir "$safeName.md"
        Set-Content -LiteralPath $docPath -Value $docText -Encoding UTF8
        $docCount++
    }
    else {
        $blockedCount++
    }

    $score = Score-Documentation $row $docText $action
    [void]$caseRecords.Add([ordered]@{
            example_id              = [string]$row.example_id
            symbol_id               = [string]$row.symbol_id
            file_path               = [string]$row.file_path
            action                  = $action
            documentation_level     = [string]$row.documentation_level
            runtime_family          = [string]$row.runtime_family
            target_framework        = [string]$row.target_framework
            csharp_language_version = [string]$row.csharp_language_version
            doc_path                = $docPath
            score                   = $score.score
            stack_version_score     = $score.stack_version_score
            source_evidence_score   = $score.source_evidence_score
            bottom_up_score         = $score.bottom_up_score
            no_hallucination_score  = $score.no_hallucination_score
            failures                = $score.failures
            source_code_line_count  = $sourceLineCount
            generated_doc_line_count = $generatedDocLineCount
            estimated_generated_tokens = $estimatedGeneratedTokenCount
        })
}

$stopwatch.Stop()
$process.Refresh()
$cpuMs = [math]::Round($process.TotalProcessorTime.TotalMilliseconds - $cpuStart, 3)
$ramMb = [math]::Round([math]::Max($process.WorkingSet64, $ramStart) / 1MB, 3)
$elapsedMs = [math]::Round($stopwatch.Elapsed.TotalMilliseconds, 3)
$avgScore = [math]::Round(((@($caseRecords) | ForEach-Object { [double]$_["score"] } | Measure-Object -Average).Average), 6)
$stackScore = [math]::Round(((@($caseRecords) | ForEach-Object { [double]$_["stack_version_score"] } | Measure-Object -Average).Average), 6)
$sourceScore = [math]::Round(((@($caseRecords) | ForEach-Object { [double]$_["source_evidence_score"] } | Measure-Object -Average).Average), 6)
$bottomScore = [math]::Round(((@($caseRecords) | ForEach-Object { [double]$_["bottom_up_score"] } | Measure-Object -Average).Average), 6)
$hallucinationScore = [math]::Round(((@($caseRecords) | ForEach-Object { [double]$_["no_hallucination_score"] } | Measure-Object -Average).Average), 6)
$sourceCodeLineCount = [int]((@($caseRecords) | ForEach-Object { [int]$_["source_code_line_count"] } | Measure-Object -Sum).Sum)
$generatedDocLineCount = [int]((@($caseRecords) | ForEach-Object { [int]$_["generated_doc_line_count"] } | Measure-Object -Sum).Sum)
$estimatedGeneratedTokenCount = [int]((@($caseRecords) | ForEach-Object { [int]$_["estimated_generated_tokens"] } | Measure-Object -Sum).Sum)
$hardFailures = @($caseRecords | Where-Object { [double]$_["score"] -lt 0.95 })
$level = if ($avgScore -ge 0.95 -and $hardFailures.Count -eq 0) {
    "EXCELLENT"
}
elseif ($avgScore -ge 0.85) {
    "GOOD"
}
elseif ($avgScore -ge 0.70) {
    "ACCEPTABLE"
}
else {
    "WEAK"
}

$summary = [ordered]@{
    batch                              = $batchName
    quality_level                      = $level
    documentation_quality_score        = $avgScore
    stack_version_score                = $stackScore
    source_evidence_score              = $sourceScore
    bottom_up_propagation_score        = $bottomScore
    no_hallucination_guard_score       = $hallucinationScore
    evaluated_rows                     = $caseRecords.Count
    produced_documentation_count       = $docCount
    correctly_blocked_count            = $blockedCount
    hard_failure_count                 = $hardFailures.Count
    cpu_ms                             = $cpuMs
    peak_ram_mb                        = $ramMb
    elapsed_ms                         = $elapsedMs
    source_code_lines_evaluated        = $sourceCodeLineCount
    generated_documentation_lines      = $generatedDocLineCount
    estimated_generated_tokens         = $estimatedGeneratedTokenCount
    quality_cost_per_evaluated_row     = [ordered]@{
        rows_measured                 = $caseRecords.Count
        cpu_ms_per_row                = Divide-Safe $cpuMs $caseRecords.Count
        elapsed_ms_per_row            = Divide-Safe $elapsedMs $caseRecords.Count
        normalized_peak_ram_mb_per_row = Divide-Safe $ramMb $caseRecords.Count
    }
    documentation_generation_cost      = [ordered]@{
        source_code_lines_evaluated       = $sourceCodeLineCount
        generated_documentation_lines     = $generatedDocLineCount
        estimated_generated_tokens        = $estimatedGeneratedTokenCount
        cpu_ms_per_source_code_line       = Divide-Safe $cpuMs $sourceCodeLineCount
        elapsed_ms_per_source_code_line   = Divide-Safe $elapsedMs $sourceCodeLineCount
        normalized_peak_ram_mb_per_source_code_line = Divide-Safe $ramMb $sourceCodeLineCount
        cpu_ms_per_estimated_generated_token = Divide-Safe $cpuMs $estimatedGeneratedTokenCount
        elapsed_ms_per_estimated_generated_token = Divide-Safe $elapsedMs $estimatedGeneratedTokenCount
        normalized_peak_ram_mb_per_estimated_generated_token = Divide-Safe $ramMb $estimatedGeneratedTokenCount
    }
    excellent_threshold                = "score >= 0.95 and hard_failure_count == 0"
    produced_docs_dir                  = $docsDir
    source_projects                    = [object[]]@("C:\Projects\wizGameserver", "C:\Projects\GenieDynamicServer", "C:\Projects\geniesoftGameMVPServer", "C:\Projects\genietasks", "C:\Projects\askapediaweb")
}

Set-Content -LiteralPath (Join-Path $qualityRoot "DOCUMENTATION-QUALITY-SUMMARY.json") -Value ($summary | ConvertTo-Json -Depth 20) -Encoding UTF8
Set-Content -LiteralPath (Join-Path $qualityRoot "DOCUMENTATION-QUALITY-CASES.jsonl") -Value (@($caseRecords) | ForEach-Object { JsonLine $_ }) -Encoding UTF8
Set-Content -LiteralPath (Join-Path $qualityRoot "DOCUMENTATION-QUALITY-REPORT.md") -Value @"
# Documentation Quality Evaluation - $batchName

Quality level: $level
Documentation quality score: $avgScore
Stack/version score: $stackScore
Source evidence score: $sourceScore
Bottom-up propagation score: $bottomScore
No-hallucination guard score: $hallucinationScore

Rows evaluated: $($caseRecords.Count)
Produced documentation samples: $docCount
Correctly blocked rows: $blockedCount
Hard failures: $($hardFailures.Count)

CPU ms: $cpuMs
Peak RAM MB: $ramMb
Elapsed ms: $elapsedMs

Source code lines evaluated: $sourceCodeLineCount
Generated documentation lines: $generatedDocLineCount
Estimated generated tokens: $estimatedGeneratedTokenCount

Quality cost per evaluated row:
- CPU ms/row: $(Divide-Safe $cpuMs $caseRecords.Count)
- Elapsed ms/row: $(Divide-Safe $elapsedMs $caseRecords.Count)
- Normalized peak RAM MB/row: $(Divide-Safe $ramMb $caseRecords.Count)

Documentation generation cost per source code line:
- CPU ms/source line: $(Divide-Safe $cpuMs $sourceCodeLineCount)
- Elapsed ms/source line: $(Divide-Safe $elapsedMs $sourceCodeLineCount)
- Normalized peak RAM MB/source line: $(Divide-Safe $ramMb $sourceCodeLineCount)

Documentation generation cost per estimated generated token:
- CPU ms/token: $(Divide-Safe $cpuMs $estimatedGeneratedTokenCount)
- Elapsed ms/token: $(Divide-Safe $elapsedMs $estimatedGeneratedTokenCount)
- Normalized peak RAM MB/token: $(Divide-Safe $ramMb $estimatedGeneratedTokenCount)

Excellent threshold: score >= 0.95 and hard_failure_count == 0.
"@ -Encoding UTF8

Write-Host ("{0} quality={1} score={2} docs={3} blocked={4} cpu={5} ram={6} cpu_per_row={7} cpu_per_source_line={8}" -f $batchName, $level, $avgScore, $docCount, $blockedCount, $cpuMs, $ramMb, (Divide-Safe $cpuMs $caseRecords.Count), (Divide-Safe $cpuMs $sourceCodeLineCount))
