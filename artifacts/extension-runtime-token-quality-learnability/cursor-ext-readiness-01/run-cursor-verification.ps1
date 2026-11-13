param(
    [string] $RepoRoot = "C:\Projects\cursor-xiigen-smart-skills",
    [string] $CoreRoot = "C:\Projects\llm_mvp_core",
    [string] $Row23InputPath = "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\row23-input-path\row23-approved-safe-scenario-input.json"
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"

$EvidenceRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$commandsRoot = Join-Path $EvidenceRoot "command-output"
$installRoot = Join-Path $EvidenceRoot "install-smoke"
$localRuntimeRoot = Join-Path $EvidenceRoot "local-runtime-helper"
New-Item -ItemType Directory -Force -Path $commandsRoot, $installRoot, $localRuntimeRoot | Out-Null

function Write-JsonFile {
    param(
        [Parameter(Mandatory = $true)] [string] $PathValue,
        [Parameter(Mandatory = $true)] [object] $Value
    )
    $directory = Split-Path -Parent $PathValue
    if (-not [string]::IsNullOrWhiteSpace($directory)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }
    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($PathValue, (($Value | ConvertTo-Json -Depth 64) + "`n"), $utf8NoBom)
}

function Invoke-CapturedCommand {
    param(
        [Parameter(Mandatory = $true)] [string] $Name,
        [Parameter(Mandatory = $true)] [string] $FilePath,
        [Parameter(Mandatory = $true)] [string[]] $ArgumentList,
        [Parameter(Mandatory = $true)] [string] $WorkingDirectory
    )
    $safeName = $Name -replace "[^A-Za-z0-9_.-]", "_"
    $stdoutPath = Join-Path $commandsRoot "$safeName.stdout.log"
    $stderrPath = Join-Path $commandsRoot "$safeName.stderr.log"
    $started = (Get-Date).ToString("o")
    $process = Start-Process -FilePath $FilePath `
        -ArgumentList $ArgumentList `
        -WorkingDirectory $WorkingDirectory `
        -RedirectStandardOutput $stdoutPath `
        -RedirectStandardError $stderrPath `
        -WindowStyle Hidden `
        -Wait `
        -PassThru
    $ended = (Get-Date).ToString("o")
    return [ordered]@{
        name = $Name
        file = $FilePath
        args = $ArgumentList
        cwd = $WorkingDirectory
        exit_code = $process.ExitCode
        started_at = $started
        ended_at = $ended
        stdout_path = $stdoutPath
        stderr_path = $stderrPath
    }
}

$row23 = Get-Content -Raw -LiteralPath $Row23InputPath | ConvertFrom-Json
$row23Prompt = [string]$row23.scenario_input.prompt_text
$promptSuitePath = Join-Path $EvidenceRoot "row23-cursor-prompt-suite.jsonl"
$promptRow = [ordered]@{
    id = "row23-cursor-safe-local-input-001"
    entrypoint_name = "xiigen.smartSkills.select"
    entrypoint_type = "command"
    concrete_task = "Use approved Row23 safe local input for Cursor adapter/core boundary smoke."
    prompt = $row23Prompt
}
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($promptSuitePath, (($promptRow | ConvertTo-Json -Compress -Depth 16) + "`n"), $utf8NoBom)

$commands = @()
$commands += Invoke-CapturedCommand -Name "node-version" -FilePath "node.exe" -ArgumentList @("--version") -WorkingDirectory $RepoRoot
$commands += Invoke-CapturedCommand -Name "node-check-local-runtime-helper" -FilePath "node.exe" -ArgumentList @("--check", "scripts\local-runtime-helper.js") -WorkingDirectory $RepoRoot
$commands += Invoke-CapturedCommand -Name "node-check-dist-extension" -FilePath "node.exe" -ArgumentList @("--check", "dist\extension.js") -WorkingDirectory $RepoRoot
$commands += Invoke-CapturedCommand -Name "install-smoke-row23" -FilePath "powershell.exe" -ArgumentList @(
    "-NoProfile",
    "-ExecutionPolicy",
    "Bypass",
    "-File",
    "scripts\install-smoke.ps1",
    "-CoreRoot",
    $CoreRoot,
    "-PromptSuite",
    $promptSuitePath,
    "-Output",
    (Join-Path $installRoot "install-smoke-report.json"),
    "-TracePath",
    (Join-Path $installRoot "install-smoke-trace.json"),
    "-EntryPointPath",
    "src\extension.ts"
) -WorkingDirectory $RepoRoot
$commands += Invoke-CapturedCommand -Name "local-runtime-helper-prompt-to-task" -FilePath "node.exe" -ArgumentList @(
    "scripts\local-runtime-helper.js",
    "--method",
    "prompt-to-task",
    "--core-root",
    $CoreRoot,
    "--core-api-script",
    (Join-Path $CoreRoot "tools\xiigen-extension-core-api.ps1"),
    "--output",
    (Join-Path $localRuntimeRoot "local-runtime-output.json"),
    "--request",
    (Join-Path $localRuntimeRoot "local-runtime-request.json"),
    "--response",
    (Join-Path $localRuntimeRoot "local-runtime-core-response.json"),
    "--trace",
    (Join-Path $localRuntimeRoot "local-runtime-core-trace.json")
) -WorkingDirectory $RepoRoot

$allPassed = -not ($commands | Where-Object { $_.exit_code -ne 0 })
$summary = [ordered]@{
    schema = "xiigen.cursor_ext_readiness_01.command_run_summary.v1"
    subagent_id_or_nickname = "CURSOR-EXT-READINESS-01"
    generated_at = (Get-Date).ToString("o")
    repo_root = $RepoRoot
    core_root = $CoreRoot
    row23_input_path = $Row23InputPath
    prompt_suite_path = $promptSuitePath
    all_commands_exit_zero = $allPassed
    commands = $commands
    forbidden_actions = [ordered]@{
        external_ai_calls = $false
        network_installs_or_downloads = $false
        claude_or_codex_execution = $false
        git_actions = $false
        ui_browser_checks = $false
        training_or_checkpoint_mutation = $false
    }
}
Write-JsonFile -PathValue (Join-Path $EvidenceRoot "commands-run.json") -Value $summary

if (-not $allPassed) {
    exit 1
}
exit 0
