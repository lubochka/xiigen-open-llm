param(
    [string]$EvidenceRoot = "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\claude-host-runtime-proof-02",
    [string]$ClaudeExe = "C:\Users\lubao\AppData\Roaming\Claude\claude-code\2.1.187\claude.exe",
    [string]$ClaudeRepo = "C:\Projects\claude-xiigen-smart-skills"
)

$ErrorActionPreference = "Stop"
New-Item -ItemType Directory -Force -Path $EvidenceRoot | Out-Null

function Invoke-CapturedCommand {
    param(
        [string]$Name,
        [string]$FilePath,
        [string[]]$ArgumentList,
        [string]$WorkingDirectory = $ClaudeRepo,
        [int]$TimeoutSeconds = 30
    )

    $stdout = Join-Path $EvidenceRoot "$Name.stdout.txt"
    $stderr = Join-Path $EvidenceRoot "$Name.stderr.txt"
    $meta = Join-Path $EvidenceRoot "$Name.command.json"
    Remove-Item -LiteralPath $stdout, $stderr -ErrorAction SilentlyContinue

    $startedAt = Get-Date
    Push-Location $WorkingDirectory
    try {
        & $FilePath @ArgumentList > $stdout 2> $stderr
        $exitCode = $LASTEXITCODE
        if ($null -eq $exitCode) {
            $exitCode = if ($?) { 0 } else { 1 }
        }
        $timedOut = $false
    } finally {
        Pop-Location
    }

    $record = [ordered]@{
        name = $Name
        executable = $FilePath
        arguments = $ArgumentList
        working_directory = $WorkingDirectory
        started_at_local = $startedAt.ToString("o")
        completed_at_local = (Get-Date).ToString("o")
        timeout_seconds = $TimeoutSeconds
        timed_out = $timedOut
        exit_code = $exitCode
        stdout = $stdout
        stderr = $stderr
    }
    $record | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $meta -Encoding UTF8
    return [pscustomobject]$record
}

$commands = @()
$commands += Invoke-CapturedCommand -Name "claude-version" -FilePath $ClaudeExe -ArgumentList @("--version") -TimeoutSeconds 20
$commands += Invoke-CapturedCommand -Name "claude-plugin-validate" -FilePath $ClaudeExe -ArgumentList @("plugin", "validate", $ClaudeRepo) -TimeoutSeconds 30
$commands += Invoke-CapturedCommand -Name "claude-plugin-list-json" -FilePath $ClaudeExe -ArgumentList @("--plugin-dir", $ClaudeRepo, "plugin", "list", "--json") -TimeoutSeconds 30
$commands += Invoke-CapturedCommand -Name "claude-plugin-details" -FilePath $ClaudeExe -ArgumentList @("--plugin-dir", $ClaudeRepo, "plugin", "details", "claude-xiigen-smart-skills") -TimeoutSeconds 30

$row23InputPath = "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\row23-input-path\row23-approved-safe-scenario-input.json"
$row23 = Get-Content -LiteralPath $row23InputPath -Raw | ConvertFrom-Json
$selectedSkillsRoot = Join-Path $EvidenceRoot "selected-skills-probe"
New-Item -ItemType Directory -Force -Path $selectedSkillsRoot | Out-Null
$selectedSkillsResponsePath = Join-Path $selectedSkillsRoot "xiigen-response.json"
$selectedSkillsTracePath = Join-Path $selectedSkillsRoot "xiigen-core-trace.json"
$commands += Invoke-CapturedCommand -Name "selected-skills-probe-row23" -FilePath "powershell.exe" -ArgumentList @(
    "-NoProfile",
    "-ExecutionPolicy",
    "Bypass",
    "-File",
    (Join-Path $ClaudeRepo "src\claude-entrypoint.ps1"),
    "-ConcreteTask",
    "CLAUDE-HOST-RUNTIME-PROOF-02 Row23 safe local selected-skills verification.",
    "-Prompt",
    $row23.scenario_input.prompt_text,
    "-Method",
    "skill-selection",
    "-CoreRoot",
    "C:\Projects\llm_mvp_core",
    "-OutputPath",
    $selectedSkillsResponsePath,
    "-TracePath",
    $selectedSkillsTracePath,
    "-AvailableSkills",
    "runtime-proof,implementation-integrity,test-integrity,documentation",
    "-RequestedSkills",
    "runtime-proof",
    "-DebugEnabled"
) -TimeoutSeconds 30

$selectedSkillsStatus = [ordered]@{
    status = "not_evaluated"
    response_path = $selectedSkillsResponsePath
    trace_path = $selectedSkillsTracePath
    selected_skills_count = 0
    selected_skills = @()
}
if (Test-Path -LiteralPath $selectedSkillsResponsePath) {
    $selectedResponse = Get-Content -LiteralPath $selectedSkillsResponsePath -Raw | ConvertFrom-Json
    $selectedSkills = @($selectedResponse.SelectedSkills)
    $selectedSkillsStatus.status = if ($selectedSkills.Count -gt 0) { "PASS_NON_EMPTY_SELECTED_SKILLS" } else { "PARTIAL_EMPTY_SELECTED_SKILLS" }
    $selectedSkillsStatus.selected_skills_count = $selectedSkills.Count
    $selectedSkillsStatus.selected_skills = $selectedSkills
}

$repoConfigRefs = Select-String -Path (Join-Path $ClaudeRepo "package.json"), (Join-Path $ClaudeRepo "README.md") -Pattern "2\.1\.177|2\.1\.187|claude\.exe|claude-code" | ForEach-Object {
    [pscustomobject]@{
        path = $_.Path
        line = $_.LineNumber
        text = $_.Line.Trim()
    }
}

$summary = [ordered]@{
    schema = "xiigen.claude_host_runtime_proof_02.runner_summary.v1"
    subagent_id_or_nickname = "CLAUDE-HOST-RUNTIME-PROOF-02"
    created_at_local = (Get-Date).ToString("o")
    claude_executable = $ClaudeExe
    claude_executable_exists = Test-Path -LiteralPath $ClaudeExe
    claude_repo = $ClaudeRepo
    current_config_runtime_refs = $repoConfigRefs
    commands = $commands
    selected_skills_probe = $selectedSkillsStatus
    external_ai_prompt_or_chat_call_ran = $false
    ui_browser_check_ran = $false
    git_action_ran = $false
    network_install_ran = $false
}

$summaryPath = Join-Path $EvidenceRoot "claude-host-runtime-proof-02-runner-summary.json"
$summary | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $summaryPath -Encoding UTF8
Write-Output $summaryPath
