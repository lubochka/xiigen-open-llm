param(
    [string] $EvidenceRoot = $PSScriptRoot,
    [string] $ClaudeRepo = "C:\Projects\claude-xiigen-smart-skills",
    [string] $CoreRoot = "C:\Projects\llm_mvp_core"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

New-Item -ItemType Directory -Force -Path $EvidenceRoot | Out-Null
$commandRecords = @()
$pathsChecked = @()
$dirListings = @()

function Add-PathCheck {
    param(
        [string] $Path,
        [string] $Source,
        [string] $Kind = "file"
    )

    $exists = Test-Path -LiteralPath $Path
    $item = $null
    if ($exists) {
        $item = Get-Item -LiteralPath $Path -Force
    }

    $script:pathsChecked += [pscustomobject]@{
        path = $Path
        source = $Source
        expected_kind = $Kind
        exists = $exists
        item_type = if ($item) { if ($item.PSIsContainer) { "directory" } else { "file" } } else { $null }
        length = if ($item -and -not $item.PSIsContainer) { $item.Length } else { $null }
        last_write_time = if ($item) { $item.LastWriteTime.ToString("o") } else { $null }
    }
}

function Invoke-Captured {
    param(
        [string] $Name,
        [string] $Executable,
        [string[]] $Arguments,
        [string] $WorkingDirectory = $EvidenceRoot
    )

    $stdout = Join-Path $EvidenceRoot ($Name + ".stdout.txt")
    $stderr = Join-Path $EvidenceRoot ($Name + ".stderr.txt")
    $meta = Join-Path $EvidenceRoot ($Name + ".command.json")

    $record = [ordered]@{
        name = $Name
        executable = $Executable
        arguments = $Arguments
        working_directory = $WorkingDirectory
        stdout = $stdout
        stderr = $stderr
        command_meta = $meta
        started_at = (Get-Date).ToString("o")
        exit_code = $null
    }

    try {
        $process = Start-Process -FilePath $Executable -ArgumentList $Arguments -WorkingDirectory $WorkingDirectory -NoNewWindow -Wait -PassThru -RedirectStandardOutput $stdout -RedirectStandardError $stderr
        $record.exit_code = $process.ExitCode
    }
    catch {
        $record.exit_code = -999
        $_.Exception.Message | Set-Content -LiteralPath $stderr -Encoding UTF8
        "" | Set-Content -LiteralPath $stdout -Encoding UTF8
    }

    $record.finished_at = (Get-Date).ToString("o")
    $record | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $meta -Encoding UTF8
    $script:commandRecords += [pscustomobject]$record
    return [pscustomobject]$record
}

$candidatePaths = New-Object System.Collections.Generic.List[string]
$explicitPaths = @(
    "C:\Users\lubao\AppData\Roaming\Claude\claude-code\2.1.177\claude.exe",
    "C:\Users\lubao\AppData\Roaming\Claude\claude-code\2.1.170\claude.exe",
    "C:\Users\lubao\AppData\Roaming\Claude\claude-code\2.1.160\claude.exe",
    (Join-Path $env:APPDATA "npm\claude.cmd"),
    (Join-Path $env:APPDATA "npm\claude.exe"),
    (Join-Path $env:APPDATA "npm\claude.ps1"),
    (Join-Path $env:LOCALAPPDATA "Programs\Claude\Claude.exe"),
    (Join-Path $env:LOCALAPPDATA "AnthropicClaude\Claude.exe"),
    (Join-Path $env:LOCALAPPDATA "Claude\Claude.exe"),
    (Join-Path $env:ProgramFiles "Claude\Claude.exe"),
    (Join-Path $env:ProgramFiles "Anthropic\Claude\Claude.exe")
)

foreach ($path in $explicitPaths) {
    Add-PathCheck -Path $path -Source "explicit-likely-path"
    if (Test-Path -LiteralPath $path) {
        $candidatePaths.Add((Get-Item -LiteralPath $path).FullName)
    }
}

$claudeCodeRoot = Join-Path $env:APPDATA "Claude\claude-code"
Add-PathCheck -Path $claudeCodeRoot -Source "likely-claude-code-root" -Kind "directory"
if (Test-Path -LiteralPath $claudeCodeRoot) {
    $children = Get-ChildItem -LiteralPath $claudeCodeRoot -Force | Select-Object FullName, Name, PSIsContainer, LastWriteTime
    $dirListings += [pscustomobject]@{ path = $claudeCodeRoot; children = $children }
    foreach ($child in $children | Where-Object { $_.PSIsContainer }) {
        $exe = Join-Path $child.FullName "claude.exe"
        Add-PathCheck -Path $exe -Source "claude-code-version-child"
        if (Test-Path -LiteralPath $exe) {
            $candidatePaths.Add((Get-Item -LiteralPath $exe).FullName)
        }
    }
}

$npmRoot = Join-Path $env:APPDATA "npm"
Add-PathCheck -Path $npmRoot -Source "likely-npm-global-root" -Kind "directory"
if (Test-Path -LiteralPath $npmRoot) {
    $npmChildren = Get-ChildItem -LiteralPath $npmRoot -Force -Filter "claude*" | Select-Object FullName, Name, PSIsContainer, LastWriteTime
    $dirListings += [pscustomobject]@{ path = $npmRoot; children = $npmChildren }
    foreach ($child in $npmChildren | Where-Object { -not $_.PSIsContainer }) {
        $candidatePaths.Add((Get-Item -LiteralPath $child.FullName).FullName)
    }
}

$commands = @()
foreach ($name in @("claude", "claude.exe", "claude.cmd", "claude.ps1")) {
    $cmd = Get-Command $name -ErrorAction SilentlyContinue
    if ($cmd) {
        $commands += $cmd | Select-Object Name, Source, CommandType, Version
        if ($cmd.Source) {
            $candidatePaths.Add($cmd.Source)
        }
    }
}

Invoke-Captured -Name "where-claude" -Executable "where.exe" -Arguments @("claude")
Invoke-Captured -Name "where-claude-exe" -Executable "where.exe" -Arguments @("claude.exe")

$uniqueCandidates = $candidatePaths | Where-Object { $_ } | Select-Object -Unique
$validCliCandidates = @()
foreach ($candidate in $uniqueCandidates) {
    $leaf = Split-Path -Leaf $candidate
    $candidateKind = if ($candidate -like "*\claude-code\*" -or $leaf -match "^claude\.(exe|cmd|ps1)$") { "claude-code-cli-or-shim" } elseif ($leaf -eq "Claude.exe") { "desktop-app-candidate-not-cli-proof" } else { "unknown" }
    $validForCliProof = $candidateKind -eq "claude-code-cli-or-shim"
    $validCliCandidates += [pscustomobject]@{
        path = $candidate
        leaf = $leaf
        candidate_kind = $candidateKind
        valid_for_cli_plugin_metadata_probe = $validForCliProof
    }
}

$chosenCli = $validCliCandidates | Where-Object { $_.valid_for_cli_plugin_metadata_probe } | Select-Object -First 1
if ($chosenCli) {
    Invoke-Captured -Name "claude-version" -Executable $chosenCli.path -Arguments @("--version")
    Invoke-Captured -Name "claude-plugin-validate" -Executable $chosenCli.path -Arguments @("plugin", "validate", $ClaudeRepo)
    Invoke-Captured -Name "claude-plugin-details-session-local" -Executable $chosenCli.path -Arguments @("--plugin-dir", $ClaudeRepo, "plugin", "details", "claude-xiigen-smart-skills")
}

$skillProbeRoot = Join-Path $EvidenceRoot "selected-skills-probe"
New-Item -ItemType Directory -Force -Path $skillProbeRoot | Out-Null
$skillResponse = Join-Path $skillProbeRoot "xiigen-response.json"
$skillTrace = Join-Path $skillProbeRoot "xiigen-core-trace.json"
$skillStdout = Join-Path $skillProbeRoot "claude-entrypoint.stdout.txt"
$skillStderr = Join-Path $skillProbeRoot "claude-entrypoint.stderr.txt"
$skillCommand = Join-Path $skillProbeRoot "claude-entrypoint.command.json"
$entrypoint = Join-Path $ClaudeRepo "src\claude-entrypoint.ps1"
$skillArgs = @(
    "-NoProfile",
    "-ExecutionPolicy", "Bypass",
    "-File", $entrypoint,
    "-ConcreteTask", "Select the best Xiigen skill for a local extension runtime proof and implementation integrity review.",
    "-Prompt", "Choose from the available skills for this local Claude adapter request. Return only safe boundary evidence and keep readiness claims locked.",
    "-Method", "skill-selection",
    "-CoreRoot", $CoreRoot,
    "-OutputPath", $skillResponse,
    "-TracePath", $skillTrace,
    "-AvailableSkills", "runtime-proof", "implementation-integrity", "test-integrity", "documentation",
    "-RequestedSkills", "runtime-proof",
    "-DebugEnabled"
)

$skillStart = Get-Date
$skillProc = Start-Process -FilePath "powershell.exe" -ArgumentList $skillArgs -WorkingDirectory $ClaudeRepo -NoNewWindow -Wait -PassThru -RedirectStandardOutput $skillStdout -RedirectStandardError $skillStderr
$skillRecord = [ordered]@{
    name = "selected-skills-probe"
    executable = "powershell.exe"
    arguments = $skillArgs
    working_directory = $ClaudeRepo
    stdout = $skillStdout
    stderr = $skillStderr
    response = $skillResponse
    trace = $skillTrace
    command_meta = $skillCommand
    started_at = $skillStart.ToString("o")
    finished_at = (Get-Date).ToString("o")
    exit_code = $skillProc.ExitCode
}
$skillRecord | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $skillCommand -Encoding UTF8
$commandRecords += [pscustomobject]$skillRecord

$selectedSkills = @()
$skillStatus = "not_run_or_no_response"
if (Test-Path -LiteralPath $skillResponse) {
    $responseJson = Get-Content -LiteralPath $skillResponse -Raw | ConvertFrom-Json
    if ($responseJson.PSObject.Properties.Name -contains "SelectedSkills" -and $responseJson.SelectedSkills) {
        $selectedSkills = @($responseJson.SelectedSkills)
    }
    $skillStatus = if ($selectedSkills.Count -gt 0) { "ADDRESSED_NONEMPTY_SELECTED_SKILLS" } else { "PARTIAL_EMPTY_SELECTED_SKILLS" }
}

$summary = [ordered]@{
    schema = "xiigen.claude_host_runtime_path_repair_01.runner_summary.v1"
    created_at = (Get-Date).ToString("o")
    safe_search_scope = @(
        "PATH via Get-Command and where.exe",
        "explicit prior package/prior-evidence Claude Code paths",
        "immediate children under APPDATA\Claude\claude-code",
        "APPDATA\npm claude* shims",
        "likely local Claude desktop install paths without recursion"
    )
    forbidden_scope_not_used = @(
        "whole-drive recursive search",
        "network install or package download",
        "external AI prompt/call",
        "Cursor/Codex execution",
        "secret capture"
    )
    get_command_results = $commands
    local_runtime_paths_checked = $pathsChecked
    likely_directory_listings = $dirListings
    cli_candidates = $validCliCandidates
    chosen_cli_for_metadata_probe = $chosenCli
    commands_run = $commandRecords
    runtime_path_found_or_not = if ($chosenCli) { "FOUND_CLI_CANDIDATE" } else { "NOT_FOUND" }
    selected_skills_probe = [ordered]@{
        status = $skillStatus
        selected_skills_count = $selectedSkills.Count
        selected_skills = $selectedSkills
        response_path = $skillResponse
        trace_path = $skillTrace
    }
}

$summaryPath = Join-Path $EvidenceRoot "runtime-path-probe-summary.json"
$summary | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $summaryPath -Encoding UTF8
$summary | ConvertTo-Json -Depth 12
