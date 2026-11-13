param(
    [string]$OutRoot = "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\codex-host-ui-route-proof-or-blocker-01"
)

$ErrorActionPreference = "Continue"
$commandsDir = Join-Path $OutRoot "commands"
New-Item -ItemType Directory -Force -Path $commandsDir | Out-Null

function Invoke-Captured {
    param(
        [string]$Name,
        [string]$FilePath,
        [string[]]$ArgumentList = @(),
        [string]$WorkingDirectory = "C:\Projects\llm_mvp_core"
    )

    $stdoutPath = Join-Path $commandsDir "$Name.stdout.txt"
    $stderrPath = Join-Path $commandsDir "$Name.stderr.txt"
    $metaPath = Join-Path $commandsDir "$Name.meta.json"
    $started = Get-Date
    $exitCode = $null
    $errorText = $null

    try {
        Push-Location $WorkingDirectory
        & $FilePath @ArgumentList 1> $stdoutPath 2> $stderrPath
        $exitCode = $LASTEXITCODE
    }
    catch {
        $exitCode = 127
        $errorText = $_.Exception.Message
        Set-Content -Path $stderrPath -Value $errorText -Encoding UTF8
        if (-not (Test-Path -Path $stdoutPath)) {
            Set-Content -Path $stdoutPath -Value "" -Encoding UTF8
        }
    }
    finally {
        Pop-Location
    }

    $finished = Get-Date
    $renderedCommandLine = (($FilePath) + $(if ($ArgumentList.Count -gt 0) { " " + ($ArgumentList -join " ") } else { "" }))
    $meta = [ordered]@{
        name = $Name
        command_line = $renderedCommandLine
        working_directory = $WorkingDirectory
        stdout_path = $stdoutPath
        stderr_path = $stderrPath
        exit_code = $exitCode
        error = $errorText
        started_at = $started.ToString("o")
        finished_at = $finished.ToString("o")
    }
    $meta | ConvertTo-Json -Depth 8 | Set-Content -Path $metaPath -Encoding UTF8
    return $meta
}

$getCommandPath = Join-Path $commandsDir "01-get-command-codex.stdout.txt"
$getCommandErr = Join-Path $commandsDir "01-get-command-codex.stderr.txt"
$getCommandMeta = Join-Path $commandsDir "01-get-command-codex.meta.json"
$started = Get-Date
$getCommandError = $null
try {
    Get-Command codex -All | Select-Object Source, CommandType, Version |
        ConvertTo-Json -Depth 8 | Set-Content -Path $getCommandPath -Encoding UTF8
    Set-Content -Path $getCommandErr -Value "" -Encoding UTF8
}
catch {
    $getCommandError = $_.Exception.Message
    Set-Content -Path $getCommandPath -Value "" -Encoding UTF8
    Set-Content -Path $getCommandErr -Value $getCommandError -Encoding UTF8
}
$finished = Get-Date
$getCommandMetaObj = [ordered]@{
    name = "01-get-command-codex"
    command_line = "Get-Command codex -All"
    working_directory = "C:\Projects\llm_mvp_core"
    stdout_path = $getCommandPath
    stderr_path = $getCommandErr
    exit_code = $(if ($getCommandError) { 1 } else { 0 })
    error = $getCommandError
    started_at = $started.ToString("o")
    finished_at = $finished.ToString("o")
}
$getCommandMetaObj | ConvertTo-Json -Depth 8 | Set-Content -Path $getCommandMeta -Encoding UTF8

$commandMetas = @()
$commandMetas += $getCommandMetaObj
$commandMetas += Invoke-Captured -Name "02-codex-version" -FilePath "codex" -ArgumentList @("--version")
$commandMetas += Invoke-Captured -Name "03-codex-help" -FilePath "codex" -ArgumentList @("--help")
$commandMetas += Invoke-Captured -Name "04-codex-plugin-help" -FilePath "codex" -ArgumentList @("plugin", "--help")
$commandMetas += Invoke-Captured -Name "05-codex-plugin-list-json" -FilePath "codex" -ArgumentList @("plugin", "list", "--json")

$pluginRoot = "C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500"
$pluginManifest = Join-Path $pluginRoot ".codex-plugin\plugin.json"
$pluginPackage = Join-Path $pluginRoot "package.json"
$processPath = Join-Path $commandsDir "06-codex-process-inventory.stdout.json"
$processErr = Join-Path $commandsDir "06-codex-process-inventory.stderr.txt"
$processError = $null
try {
    Get-Process | Where-Object { $_.ProcessName -match "Codex|OpenAI" } |
        Select-Object Id, ProcessName, Path, @{Name="HasMainWindowTitle";Expression={ [bool]$_.MainWindowTitle }} |
        ConvertTo-Json -Depth 8 | Set-Content -Path $processPath -Encoding UTF8
    Set-Content -Path $processErr -Value "" -Encoding UTF8
}
catch {
    $processError = $_.Exception.Message
    Set-Content -Path $processPath -Value "" -Encoding UTF8
    Set-Content -Path $processErr -Value $processError -Encoding UTF8
}

$summary = [ordered]@{
    schema = "xiigen.codex_host_ui_route_discovery_01.v1"
    subagent_id_or_nickname = "CODEX-HOST-UI-ROUTE-PROOF-OR-BLOCKER-01"
    generated_at_local = (Get-Date).ToString("o")
    no_secrets_captured = $true
    external_ai_calls_made = $false
    network_installs_or_downloads_made = $false
    git_actions_made = $false
    route_discovery_commands = $commandMetas
    plugin_cache = [ordered]@{
        root = $pluginRoot
        root_exists = (Test-Path -Path $pluginRoot)
        manifest = $pluginManifest
        manifest_exists = (Test-Path -Path $pluginManifest)
        package_json = $pluginPackage
        package_json_exists = (Test-Path -Path $pluginPackage)
        manifest_sha256 = $(if (Test-Path -Path $pluginManifest) { (Get-FileHash -Algorithm SHA256 -Path $pluginManifest).Hash } else { $null })
        package_sha256 = $(if (Test-Path -Path $pluginPackage) { (Get-FileHash -Algorithm SHA256 -Path $pluginPackage).Hash } else { $null })
    }
    process_inventory = [ordered]@{
        path = $processPath
        stderr_path = $processErr
        error = $processError
        window_titles_redacted = $true
    }
    route_findings = [ordered]@{
        codex_cli_found_on_path = $true
        codex_cli_safe_invocation_blocked = (($commandMetas | Where-Object { $_.name -eq "02-codex-version" }).exit_code -ne 0)
        codex_plugin_list_safe_invocation_blocked = (($commandMetas | Where-Object { $_.name -eq "05-codex-plugin-list-json" }).exit_code -ne 0)
        installed_plugin_cache_present = (Test-Path -Path $pluginRoot)
        installed_plugin_cache_is_not_live_host_ui = $true
        user_visible_codex_host_route_proven = $false
    }
}

$summaryPath = Join-Path $OutRoot "route-discovery-summary.json"
$summary | ConvertTo-Json -Depth 20 | Set-Content -Path $summaryPath -Encoding UTF8
Write-Output $summaryPath
