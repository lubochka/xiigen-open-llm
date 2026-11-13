[CmdletBinding()]
param(
    [string]$ClaudeRepoRoot = "C:\Projects\claude-xiigen-smart-skills",
    [string]$CoreRoot = "C:\Projects\llm_mvp_core",
    [string]$Row23InputPath = "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\row23-input-path\row23-approved-safe-scenario-input.json"
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"

$evidenceRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$runRoot = Join-Path $evidenceRoot "row23-adapter-smoke-with-skills"
$utf8NoBom = [System.Text.UTF8Encoding]::new($false)
New-Item -ItemType Directory -Force -Path $runRoot | Out-Null

$row23 = Get-Content -Raw -LiteralPath $Row23InputPath | ConvertFrom-Json
$checkSetPath = Join-Path $runRoot "row23-claude-check-set.json"
$checkSet = @(
    [ordered]@{
        id = "row23-claude-core-boundary"
        kind = "core_contract_boundary"
        assertion = "Claude adapter sends Row23 safe local input to llm_mvp_core and host does not own semantic/training logic."
    },
    [ordered]@{
        id = "row23-claude-task-to-skills"
        kind = "task_to_skills"
        assertion = "Host supplies a bounded available-skill surface so core can select from it."
    },
    [ordered]@{
        id = "row23-claude-non-claims"
        kind = "non_claim_locks"
        assertion = "This smoke cannot claim full host readiness, token savings, quality improvement, learnability, target promotion, or rollback."
    }
)
[System.IO.File]::WriteAllText($checkSetPath, (($checkSet | ConvertTo-Json -Depth 12) + [Environment]::NewLine), $utf8NoBom)

$stdoutPath = Join-Path $runRoot "row23-claude-adapter-smoke-with-skills.stdout.txt"
$stderrPath = Join-Path $runRoot "row23-claude-adapter-smoke-with-skills.stderr.txt"
$metaPath = Join-Path $runRoot "row23-claude-adapter-smoke-with-skills.command.json"
$argsList = @(
    "-NoProfile",
    "-ExecutionPolicy",
    "Bypass",
    "-File",
    (Join-Path $ClaudeRepoRoot "src\claude-entrypoint.ps1"),
    "-ConcreteTask",
    "CLAUDE-EXT-READINESS-01 Row23 safe input adapter smoke with available skills",
    "-Prompt",
    [string]$row23.scenario_input.prompt_text,
    "-Method",
    "skill-selection",
    "-CheckSetPath",
    $checkSetPath,
    "-CoreRoot",
    $CoreRoot,
    "-OutputPath",
    (Join-Path $runRoot "row23-claude-response.json"),
    "-TracePath",
    (Join-Path $runRoot "row23-claude-core-trace.json"),
    "-AvailableSkills",
    "codex-xiigen-smart-skills:xiigen-smart-skills,openai-docs,github:github,github:gh-address-comments,skill-creator,browser:control-in-app-browser",
    "-DebugEnabled"
)

$record = [ordered]@{
    name = "row23-claude-adapter-smoke-with-skills"
    executable = "powershell.exe"
    arguments = $argsList
    working_directory = $ClaudeRepoRoot
    stdout = $stdoutPath
    stderr = $stderrPath
    started_at = Get-Date -Format o
}

[System.IO.File]::WriteAllText($metaPath, (($record | ConvertTo-Json -Depth 12) + [Environment]::NewLine), $utf8NoBom)
Push-Location $ClaudeRepoRoot
try {
    & powershell.exe @argsList 1> $stdoutPath 2> $stderrPath
    $exitCode = $LASTEXITCODE
    if ($null -eq $exitCode) {
        $exitCode = if ($?) { 0 } else { 1 }
    }
}
finally {
    Pop-Location
}

$record["ended_at"] = Get-Date -Format o
$record["exit_code"] = [int]$exitCode
$record["command_meta"] = $metaPath
[System.IO.File]::WriteAllText($metaPath, (($record | ConvertTo-Json -Depth 12) + [Environment]::NewLine), $utf8NoBom)
exit $exitCode
