[CmdletBinding()]
param(
    [string]$ClaudeRepoRoot = "C:\Projects\claude-xiigen-smart-skills",
    [string]$CoreRoot = "C:\Projects\llm_mvp_core",
    [string]$Row23InputPath = "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\row23-input-path\row23-approved-safe-scenario-input.json"
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"

$EvidenceRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$utf8NoBom = [System.Text.UTF8Encoding]::new($false)

function Invoke-CapturedCommand {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name,
        [Parameter(Mandatory = $true)]
        [string]$Executable,
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments,
        [Parameter(Mandatory = $true)]
        [string]$OutputDirectory
    )

    New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
    $stdoutPath = Join-Path $OutputDirectory "$Name.stdout.txt"
    $stderrPath = Join-Path $OutputDirectory "$Name.stderr.txt"
    $metaPath = Join-Path $OutputDirectory "$Name.command.json"
    $startedAt = Get-Date -Format o
    $commandRecord = [ordered]@{
        name = $Name
        executable = $Executable
        arguments = $Arguments
        working_directory = $ClaudeRepoRoot
        stdout = $stdoutPath
        stderr = $stderrPath
        started_at = $startedAt
    }

    [System.IO.File]::WriteAllText($metaPath, (($commandRecord | ConvertTo-Json -Depth 12) + [Environment]::NewLine), $utf8NoBom)
    Push-Location $ClaudeRepoRoot
    try {
        & $Executable @Arguments 1> $stdoutPath 2> $stderrPath
        $exitCode = $LASTEXITCODE
        if ($null -eq $exitCode) {
            $exitCode = if ($?) { 0 } else { 1 }
        }
    }
    finally {
        Pop-Location
    }

    $commandRecord["ended_at"] = Get-Date -Format o
    $commandRecord["exit_code"] = [int]$exitCode
    $commandRecord["command_meta"] = $metaPath
    [System.IO.File]::WriteAllText($metaPath, (($commandRecord | ConvertTo-Json -Depth 12) + [Environment]::NewLine), $utf8NoBom)
    return [pscustomobject]$commandRecord
}

function Write-JsonFile {
    param(
        [Parameter(Mandatory = $true)]
        [object]$Value,
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $parent = Split-Path -Parent $Path
    if ($parent) {
        New-Item -ItemType Directory -Force -Path $parent | Out-Null
    }
    [System.IO.File]::WriteAllText($Path, (($Value | ConvertTo-Json -Depth 32) + [Environment]::NewLine), $utf8NoBom)
}

$installRoot = Join-Path $EvidenceRoot "install-smoke"
$row23Root = Join-Path $EvidenceRoot "row23-adapter-smoke"
New-Item -ItemType Directory -Force -Path $installRoot, $row23Root | Out-Null

$npmCommand = (Get-Command npm.cmd -ErrorAction SilentlyContinue).Source
if (-not $npmCommand) {
    $npmCommand = (Get-Command npm -ErrorAction Stop).Source
}

$commands = @()
$commands += Invoke-CapturedCommand -Name "npm-install-smoke" -Executable $npmCommand -OutputDirectory $installRoot -Arguments @(
    "run",
    "install-smoke",
    "--",
    "-CoreRoot",
    $CoreRoot,
    "-Output",
    (Join-Path $installRoot "install-smoke-report.json"),
    "-TracePath",
    (Join-Path $installRoot "install-smoke-trace.json")
)

$row23 = Get-Content -Raw -LiteralPath $Row23InputPath | ConvertFrom-Json
$row23CheckSetPath = Join-Path $row23Root "row23-claude-check-set.json"
Write-JsonFile -Path $row23CheckSetPath -Value @(
    [ordered]@{
        id = "row23-claude-core-boundary"
        kind = "core_contract_boundary"
        assertion = "Claude adapter sends Row23 safe local input to llm_mvp_core and does not make host-side semantic/training decisions."
    },
    [ordered]@{
        id = "row23-claude-non-claims"
        kind = "non_claim_locks"
        assertion = "Adapter smoke cannot claim full host readiness, token savings, quality improvement, learnability, target promotion, or rollback."
    }
)

$commands += Invoke-CapturedCommand -Name "row23-claude-adapter-smoke" -Executable "powershell.exe" -OutputDirectory $row23Root -Arguments @(
    "-NoProfile",
    "-ExecutionPolicy",
    "Bypass",
    "-File",
    (Join-Path $ClaudeRepoRoot "src\claude-entrypoint.ps1"),
    "-ConcreteTask",
    "CLAUDE-EXT-READINESS-01 Row23 safe input adapter smoke",
    "-Prompt",
    [string]$row23.scenario_input.prompt_text,
    "-Method",
    "skill-selection",
    "-CheckSetPath",
    $row23CheckSetPath,
    "-CoreRoot",
    $CoreRoot,
    "-OutputPath",
    (Join-Path $row23Root "row23-claude-response.json"),
    "-TracePath",
    (Join-Path $row23Root "row23-claude-core-trace.json"),
    "-DebugEnabled"
)

$summary = [ordered]@{
    schema = "xiigen.claude_ext_readiness_01.runner_summary.v1"
    subagent_id_or_nickname = "CLAUDE-EXT-READINESS-01"
    created_at = Get-Date -Format o
    claude_repo_root = $ClaudeRepoRoot
    core_root = $CoreRoot
    row23_input_path = $Row23InputPath
    commands = $commands
    evidence_files = @(
        (Join-Path $installRoot "install-smoke-report.json"),
        (Join-Path $installRoot "install-smoke-trace.json"),
        (Join-Path $row23Root "row23-claude-response.json"),
        (Join-Path $row23Root "row23-claude-core-trace.json"),
        (Join-Path $row23Root "xiigen-claude-core-request.json"),
        (Join-Path $row23Root "xiigen-claude-host-trace.json")
    )
}

Write-JsonFile -Path (Join-Path $EvidenceRoot "runner-summary.json") -Value $summary

$failed = @($commands | Where-Object { $_.exit_code -ne 0 })
if ($failed.Count -gt 0) {
    exit 1
}

exit 0
