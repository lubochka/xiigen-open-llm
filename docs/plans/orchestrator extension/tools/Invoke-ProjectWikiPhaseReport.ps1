param(
    [string]$SolutionPath = "C:\Projects\llm_mvp_core\LlmFromScratch.sln",
    [string]$WorkDir = "C:\Projects\llm_mvp_core",
    [string]$ReportRoot = "C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics",
    [switch]$IncludeFullSolution
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function New-PhaseResult {
    param(
        [string]$Phase,
        [string]$Filter,
        [int]$ExitCode,
        [int]$Passed,
        [int]$Failed,
        [int]$Skipped,
        [int]$Total,
        [double]$ElapsedMs,
        [double]$CpuMs,
        [double]$PeakWorkingSetMb,
        [string]$StdoutTail,
        [string]$Stderr
    )

    $passRate = if ($Total -gt 0) { [Math]::Round($Passed / [double]$Total, 6) } else { 0.0 }
    $falsePositiveRisk = if ($ExitCode -eq 0 -and $Total -gt 0 -and $Failed -eq 0) { "none" } else { "partial_gate" }

    [pscustomobject]@{
        report_scope = "test_result"
        phase = $Phase
        source_of_truth = "dotnet test process output plus process sampling"
        filter = $Filter
        exit_code = $ExitCode
        passed = $Passed
        failed = $Failed
        skipped = $Skipped
        total = $Total
        pass_rate = $passRate
        elapsed_ms = [Math]::Round($ElapsedMs, 3)
        process_cpu_ms = [Math]::Round($CpuMs, 3)
        sampled_peak_working_set_mb = [Math]::Round($PeakWorkingSetMb, 3)
        section_22_applicable = $false
        runner_output_available = $false
        missing_runner_metrics = @("reward_margin", "loss", "heldout_accuracy")
        false_positive_risk = $falsePositiveRisk
        allowed_claim = "test_result_only_not_training_promotion"
        stdout_tail = $StdoutTail
        stderr = $Stderr
    }
}

function Invoke-Phase {
    param(
        [string]$Phase,
        [string]$Filter
    )

    $startedAt = Get-Date
    $stdoutPath = [System.IO.Path]::GetTempFileName()
    $stderrPath = [System.IO.Path]::GetTempFileName()
    $args = @("test", $SolutionPath, "--logger", "console;verbosity=minimal")
    if (-not [string]::IsNullOrWhiteSpace($Filter)) {
        $args += @("--filter", $Filter)
    }

    $proc = Start-Process -FilePath "dotnet" `
        -ArgumentList $args `
        -WorkingDirectory $WorkDir `
        -RedirectStandardOutput $stdoutPath `
        -RedirectStandardError $stderrPath `
        -NoNewWindow `
        -PassThru

    $peakWorkingSetMb = 0.0
    $sampledCpuMs = 0.0
    $sampleNames = @("dotnet", "testhost", "vstest.console")
    while (-not $proc.HasExited) {
        try {
            $sample = Get-Process -ErrorAction SilentlyContinue |
                Where-Object {
                    $sampleNames -contains $_.ProcessName -and
                    $_.StartTime -ge $startedAt.AddSeconds(-5)
                }
            $workingSet = ($sample | Measure-Object -Property WorkingSet64 -Sum).Sum
            if ($null -ne $workingSet) {
                $workingSetMb = $workingSet / 1MB
                if ($workingSetMb -gt $peakWorkingSetMb) {
                    $peakWorkingSetMb = $workingSetMb
                }
            }

            $cpuSeconds = ($sample |
                Where-Object { $null -ne $_.CPU } |
                Measure-Object -Property CPU -Sum).Sum
            if ($null -ne $cpuSeconds) {
                $currentCpuMs = [double]$cpuSeconds * 1000.0
                if ($currentCpuMs -gt $sampledCpuMs) {
                    $sampledCpuMs = $currentCpuMs
                }
            }
        }
        catch {
            # Process sampling is best-effort; failure must not hide test failure.
        }

        Start-Sleep -Milliseconds 250
        $proc.Refresh()
    }

    $proc.WaitForExit()
    $endedAt = Get-Date
    $stdout = Get-Content -Path $stdoutPath -Raw
    $stderr = Get-Content -Path $stderrPath -Raw
    Remove-Item -LiteralPath $stdoutPath, $stderrPath -Force -ErrorAction SilentlyContinue

    $passed = 0
    $failed = 0
    $skipped = 0
    $total = 0
    if ($stdout -match "Passed!\s+-\s+Failed:\s+(\d+),\s+Passed:\s+(\d+),\s+Skipped:\s+(\d+),\s+Total:\s+(\d+)") {
        $failed = [int]$Matches[1]
        $passed = [int]$Matches[2]
        $skipped = [int]$Matches[3]
        $total = [int]$Matches[4]
    }

    $tail = (($stdout -split "`r?`n") | Select-Object -Last 12) -join "`n"
    $elapsedTotalMs = [double]$endedAt.Subtract($startedAt).TotalMilliseconds
    $cpuTotalMs = $sampledCpuMs
    New-PhaseResult `
        -Phase $Phase `
        -Filter $Filter `
        -ExitCode $proc.ExitCode `
        -Passed $passed `
        -Failed $failed `
        -Skipped $skipped `
        -Total $total `
        -ElapsedMs $elapsedTotalMs `
        -CpuMs $cpuTotalMs `
        -PeakWorkingSetMb $peakWorkingSetMb `
        -StdoutTail $tail `
        -Stderr $stderr
}

$runId = Get-Date -Format "yyyy-MM-dd-HHmmss"
$runRoot = Join-Path $ReportRoot $runId
New-Item -ItemType Directory -Force -Path $runRoot | Out-Null

$phases = @(
    [pscustomobject]@{
        Name = "phase-01-query-planning-unit-gate"
        Filter = "FullyQualifiedName~ProjectWikiQueryPlanning"
    },
    [pscustomobject]@{
        Name = "phase-02-project-wiki-domain-integration"
        Filter = "FullyQualifiedName~ProjectWikiDomainTests"
    },
    [pscustomobject]@{
        Name = "phase-03-real-project-validation-five-dotnet-wikis"
        Filter = "FullyQualifiedName~ProjectWikiRealProjectValidationRunner_ValidatesExistingDotNetProjectWikisWhenPresent"
    }
)

if ($IncludeFullSolution) {
    $phases += [pscustomobject]@{
        Name = "phase-04-full-solution-regression"
        Filter = ""
    }
}

$results = @(foreach ($phase in $phases) {
    $result = Invoke-Phase -Phase $phase.Name -Filter $phase.Filter
    $phaseReportPath = Join-Path $runRoot "$($phase.Name).json"
    $result | Add-Member -NotePropertyName "report_artifact_path" -NotePropertyValue $phaseReportPath -Force
    $result | Add-Member -NotePropertyName "artifact_written_before_message" -NotePropertyValue $true -Force
    $result | ConvertTo-Json -Depth 8 | Set-Content -Path $phaseReportPath -Encoding UTF8
    $result
})

$riskCount = @($results | Where-Object { $_.false_positive_risk -ne "none" }).Count
$summaryPath = Join-Path $runRoot "PHASE-RESOURCE-REPORT.json"

$summary = [pscustomobject]@{
    report_scope = "test_result"
    source_of_truth = $runRoot
    report_artifact_path = $summaryPath
    artifact_written_before_message = $true
    section_22_applicable = $false
    false_positive_risk = if ($riskCount -eq 0) { "none" } else { "partial_gate" }
    allowed_claim = "phase_resource_report_generated_not_training_promotion"
    phases = $results
}

$summary | ConvertTo-Json -Depth 10 | Set-Content -Path $summaryPath -Encoding UTF8
$summary
