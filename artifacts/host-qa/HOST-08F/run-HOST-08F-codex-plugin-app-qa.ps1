param(
    [string]$CodexRepo = "C:\Projects\codex-xiigen-smart-skills",
    [string]$CoreRoot = "C:\Projects\llm_mvp_core",
    [string]$RunId = ""
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RunId)) {
    $RunId = Get-Date -Format "yyyyMMdd-HHmmss"
}

$OutputRoot = Join-Path $CoreRoot "artifacts\host-qa\HOST-08F\$RunId"
$HostReport = Join-Path $CodexRepo "reports\HOST-08F-codex-plugin-app-qa.json"
$StdoutPath = Join-Path $OutputRoot "codex-component-qa.stdout.log"
$StderrPath = Join-Path $OutputRoot "codex-component-qa.stderr.log"
$SummaryPath = Join-Path $OutputRoot "codex-component-qa-command-summary.json"
$RunnerPath = Join-Path $CodexRepo "test\host-qa\HOST-08F-codexComponentQA.js"

New-Item -ItemType Directory -Force -Path $OutputRoot | Out-Null

$Start = Get-Date
$Process = Start-Process -FilePath "node" -ArgumentList @(
    $RunnerPath,
    "--output-root", $OutputRoot,
    "--host-report", $HostReport,
    "--core-root", $CoreRoot
) -WorkingDirectory $CodexRepo -NoNewWindow -Wait -PassThru -RedirectStandardOutput $StdoutPath -RedirectStandardError $StderrPath
$End = Get-Date

$Summary = [ordered]@{
    work_order = "HOST-08F-CODEX-PLUGIN-APP-QA-PATH"
    claim_scope = "Codex active JS source-entrypoint component QA only; not full Codex plugin/app UI runtime evidence."
    command = "node $RunnerPath --output-root $OutputRoot --host-report $HostReport --core-root $CoreRoot"
    working_directory = $CodexRepo
    exit_code = $Process.ExitCode
    elapsed_ms = [int](($End - $Start).TotalMilliseconds)
    stdout_ref = $StdoutPath
    stderr_ref = $StderrPath
    output_root = $OutputRoot
    host_report_ref = $HostReport
    codex_plugin_runtime_qa = $false
    codex_exec_model_backed_qa = $false
    handler_or_component_level_qa = $true
    full_plugin_ui_evidence = $false
}

$Summary | ConvertTo-Json -Depth 8 | Set-Content -Path $SummaryPath -Encoding UTF8
if ($Process.ExitCode -ne 0) {
    throw "HOST-08F Codex component QA failed with exit code $($Process.ExitCode). See $SummaryPath"
}

Write-Output "HOST-08F Codex component QA passed. Summary: $SummaryPath"
