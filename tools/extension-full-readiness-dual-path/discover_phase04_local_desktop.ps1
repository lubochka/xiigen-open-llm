param(
    [string]$EvidenceDir = "docs/plans/evidence/extension-full-readiness-dual-path/phase-04-local-desktop-evidence"
)

$ErrorActionPreference = "Continue"
$root = (Get-Location).Path
$evidencePath = Join-Path $root $EvidenceDir
New-Item -ItemType Directory -Force -Path $evidencePath | Out-Null

function Save-Json {
    param(
        [string]$Name,
        [object]$Value
    )
    $path = Join-Path $evidencePath $Name
    $Value | ConvertTo-Json -Depth 8 | Set-Content -Encoding UTF8 -Path $path
    return $path
}

function Save-Text {
    param(
        [string]$Name,
        [string]$Value
    )
    $path = Join-Path $evidencePath $Name
    $Value | Set-Content -Encoding UTF8 -Path $path
    return $path
}

function Test-KnownPath {
    param(
        [string]$HostName,
        [string]$Path
    )
    [pscustomobject]@{
        host = $HostName
        path = $Path
        exists = [bool](Test-Path -LiteralPath $Path)
        item_type = if (Test-Path -LiteralPath $Path) { (Get-Item -LiteralPath $Path -ErrorAction SilentlyContinue).GetType().Name } else { "NOT_FOUND" }
    }
}

function Invoke-SafeProbe {
    param(
        [string]$Command,
        [string]$Arguments,
        [int]$TimeoutMs = 7000
    )

    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = "cmd.exe"
    $psi.Arguments = "/d /s /c `"`"$Command`" $Arguments`""
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError = $true
    $psi.UseShellExecute = $false
    $psi.CreateNoWindow = $true

    $process = New-Object System.Diagnostics.Process
    $process.StartInfo = $psi
    $started = $false
    try {
        $started = $process.Start()
        if (-not $started) {
            return [pscustomobject]@{
                command = $Command
                arguments = $Arguments
                exit_code = $null
                timed_out = $false
                stdout = ""
                stderr = "Process did not start."
            }
        }
        $finished = $process.WaitForExit($TimeoutMs)
        if (-not $finished) {
            try { $process.Kill() } catch {}
            return [pscustomobject]@{
                command = $Command
                arguments = $Arguments
                exit_code = $null
                timed_out = $true
                stdout = $process.StandardOutput.ReadToEnd()
                stderr = $process.StandardError.ReadToEnd()
            }
        }
        return [pscustomobject]@{
            command = $Command
            arguments = $Arguments
            exit_code = $process.ExitCode
            timed_out = $false
            stdout = $process.StandardOutput.ReadToEnd()
            stderr = $process.StandardError.ReadToEnd()
        }
    } catch {
        return [pscustomobject]@{
            command = $Command
            arguments = $Arguments
            exit_code = $null
            timed_out = $false
            stdout = ""
            stderr = $_.Exception.Message
        }
    } finally {
        if ($null -ne $process) {
            $process.Dispose()
        }
    }
}

$timestampUtc = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
$candidateCommands = @(
    @{ host = "Claude"; command = "claude" },
    @{ host = "Claude"; command = "claude-code" },
    @{ host = "Cursor"; command = "cursor" },
    @{ host = "Codex"; command = "codex" },
    @{ host = "Codex"; command = "codex-cli" },
    @{ host = "Codex"; command = "openai" }
)

$commandRows = foreach ($candidate in $candidateCommands) {
    $matches = @(Get-Command $candidate.command -All -ErrorAction SilentlyContinue)
    if ($matches.Count -eq 0) {
        [pscustomobject]@{
            host = $candidate.host
            command = $candidate.command
            found = $false
            command_type = "NOT_FOUND"
            source = "NOT_FOUND"
            path = "NOT_FOUND"
        }
    } else {
        foreach ($match in $matches) {
            [pscustomobject]@{
                host = $candidate.host
                command = $candidate.command
                found = $true
                command_type = [string]$match.CommandType
                source = [string]$match.Source
                path = [string]$match.Path
            }
        }
    }
}

$programFilesX86 = ${env:ProgramFiles(x86)}
$knownPaths = @(
    @{ host = "Claude"; path = Join-Path $env:LOCALAPPDATA "Programs\Claude\Claude.exe" },
    @{ host = "Claude"; path = Join-Path $env:LOCALAPPDATA "AnthropicClaude\Claude.exe" },
    @{ host = "Claude"; path = Join-Path $env:LOCALAPPDATA "AnthropicClaude\claude.exe" },
    @{ host = "Claude"; path = Join-Path $env:ProgramFiles "Claude\Claude.exe" },
    @{ host = "Claude"; path = if ($programFilesX86) { Join-Path $programFilesX86 "Claude\Claude.exe" } else { "" } },
    @{ host = "Cursor"; path = Join-Path $env:LOCALAPPDATA "Programs\Cursor\Cursor.exe" },
    @{ host = "Cursor"; path = Join-Path $env:LOCALAPPDATA "Programs\cursor\Cursor.exe" },
    @{ host = "Cursor"; path = Join-Path $env:ProgramFiles "Cursor\Cursor.exe" },
    @{ host = "Cursor"; path = if ($programFilesX86) { Join-Path $programFilesX86 "Cursor\Cursor.exe" } else { "" } },
    @{ host = "Codex"; path = Join-Path $env:LOCALAPPDATA "Programs\Codex\Codex.exe" },
    @{ host = "Codex"; path = Join-Path $env:LOCALAPPDATA "Programs\OpenAI Codex\Codex.exe" },
    @{ host = "Codex"; path = Join-Path $env:LOCALAPPDATA "Programs\OpenAI\Codex.exe" },
    @{ host = "Codex"; path = Join-Path $env:ProgramFiles "OpenAI\Codex\Codex.exe" },
    @{ host = "Codex"; path = Join-Path $env:APPDATA "npm\codex.cmd" }
) | Where-Object { -not [string]::IsNullOrWhiteSpace($_.path) }

$knownPathRows = foreach ($candidatePath in $knownPaths) {
    Test-KnownPath -HostName $candidatePath.host -Path $candidatePath.path
}

$uninstallRoots = @(
    "HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*",
    "HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*",
    "HKLM:\Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*"
)
$installedRows = foreach ($rootKey in $uninstallRoots) {
    Get-ItemProperty $rootKey -ErrorAction SilentlyContinue |
        Where-Object { $_.DisplayName -match "Claude|Cursor|Codex|OpenAI|Anthropic" } |
        ForEach-Object {
            [pscustomobject]@{
                registry_root = $rootKey
                display_name = [string]$_.DisplayName
                display_version = [string]$_.DisplayVersion
                publisher = [string]$_.Publisher
                install_location = [string]$_.InstallLocation
                display_icon = [string]$_.DisplayIcon
                uninstall_string_present = [bool]$_.UninstallString
            }
        }
}

$appxRows = Get-AppxPackage -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -match "Claude|Cursor|Codex|OpenAI|Anthropic" -or $_.PackageFullName -match "Claude|Cursor|Codex|OpenAI|Anthropic" } |
    ForEach-Object {
        [pscustomobject]@{
            name = [string]$_.Name
            package_full_name = [string]$_.PackageFullName
            version = [string]$_.Version
            publisher = [string]$_.Publisher
            install_location = [string]$_.InstallLocation
            is_framework = [bool]$_.IsFramework
        }
    }

$shortcutRoots = @(
    (Join-Path $env:APPDATA "Microsoft\Windows\Start Menu\Programs"),
    (Join-Path $env:ProgramData "Microsoft\Windows\Start Menu\Programs"),
    [Environment]::GetFolderPath("Desktop")
)
$shell = New-Object -ComObject WScript.Shell
$shortcutRows = foreach ($shortcutRoot in $shortcutRoots) {
    if (Test-Path -LiteralPath $shortcutRoot) {
        Get-ChildItem -LiteralPath $shortcutRoot -Recurse -Filter *.lnk -ErrorAction SilentlyContinue |
            Where-Object { $_.Name -match "Claude|Cursor|Codex|OpenAI|Anthropic" } |
            ForEach-Object {
                $target = ""
                $arguments = ""
                try {
                    $shortcut = $shell.CreateShortcut($_.FullName)
                    $target = [string]$shortcut.TargetPath
                    $arguments = [string]$shortcut.Arguments
                } catch {
                    $target = "UNRESOLVED"
                    $arguments = $_.Exception.Message
                }
                [pscustomobject]@{
                    name = $_.Name
                    full_name = $_.FullName
                    target = $target
                    arguments = $arguments
                }
            }
    }
}

$processRows = Get-Process -ErrorAction SilentlyContinue |
    Where-Object { $_.ProcessName -match "Claude|Cursor|Codex|Code|OpenAI" } |
    ForEach-Object {
        $path = "UNAVAILABLE"
        try { $path = [string]$_.Path } catch {}
        [pscustomobject]@{
            process_name = $_.ProcessName
            id = $_.Id
            main_window_title_present = -not [string]::IsNullOrWhiteSpace($_.MainWindowTitle)
            path = $path
            start_time_available = [bool]($_.StartTime)
        }
    }

$probeTargets = @(foreach ($row in $commandRows | Where-Object { $_.found -eq $true -and $_.command -in @("claude", "cursor", "codex") }) {
    [pscustomobject]@{
        host = $row.host
        command = $row.command
        invocation = "path_lookup_name"
        target = $row.command
        source = $row.source
    }
    if ($row.path -and $row.path -ne "NOT_FOUND" -and (Test-Path -LiteralPath $row.path)) {
        [pscustomobject]@{
            host = $row.host
            command = $row.command
            invocation = "exact_command_path"
            target = $row.path
            source = $row.source
        }
    }
}) | Sort-Object host, command, invocation, target -Unique

$probeRows = foreach ($row in $probeTargets) {
    foreach ($args in @("--version", "--help")) {
        $probe = Invoke-SafeProbe -Command $row.target -Arguments $args
        [pscustomobject]@{
            host = $row.host
            command = $row.command
            invocation = $row.invocation
            target = $row.target
            source = $row.source
            arguments = $args
            exit_code = $probe.exit_code
            timed_out = $probe.timed_out
            stdout_first_2000 = if ($probe.stdout.Length -gt 2000) { $probe.stdout.Substring(0, 2000) } else { $probe.stdout }
            stderr_first_2000 = if ($probe.stderr.Length -gt 2000) { $probe.stderr.Substring(0, 2000) } else { $probe.stderr }
        }
    }
}

$markerRows = @(
    [pscustomobject]@{
        host = "Claude"
        marker = "FRDP-P04-DISCOVERY-Claude"
        safe_marker_smoke_status = "NOT_RUN"
        reason = "A Claude GUI or prompt smoke may expose account/chat history or contact an external AI service; discovery records only local paths/processes and safe CLI version/help probes."
    },
    [pscustomobject]@{
        host = "Cursor"
        marker = "FRDP-P04-DISCOVERY-Cursor"
        safe_marker_smoke_status = "NOT_RUN"
        reason = "Launching a Cursor GUI marker file could reveal workspace/recent-file/account UI; discovery records local command/app evidence only."
    },
    [pscustomobject]@{
        host = "Codex"
        marker = "FRDP-P04-DISCOVERY-Codex"
        safe_marker_smoke_status = "NOT_RUN"
        reason = "The current Codex chat UI may reveal this conversation or workspace details; discovery records local CLI/app/process evidence only."
    }
)

$summary = [pscustomobject]@{
    generated_at_utc = $timestampUtc
    evidence_dir = $evidencePath
    command_candidates = $candidateCommands
    found_command_count = @($commandRows | Where-Object { $_.found }).Count
    known_path_found_count = @($knownPathRows | Where-Object { $_.exists }).Count
    installed_app_match_count = @($installedRows).Count
    appx_package_match_count = @($appxRows).Count
    shortcut_match_count = @($shortcutRows).Count
    process_match_count = @($processRows).Count
    safe_marker_policy = "No screenshot or host prompt marker smoke was run unless the surface could be proven marker-only and private-data-free; none met that threshold in this automated slice."
}

$paths = [ordered]@{}
$paths["01-get-command-discovery.json"] = Save-Json -Name "01-get-command-discovery.json" -Value $commandRows
$paths["02-known-path-discovery.json"] = Save-Json -Name "02-known-path-discovery.json" -Value $knownPathRows
$paths["03-installed-apps-discovery.json"] = Save-Json -Name "03-installed-apps-discovery.json" -Value $installedRows
$paths["04-appx-package-discovery.json"] = Save-Json -Name "04-appx-package-discovery.json" -Value $appxRows
$paths["05-shortcuts-discovery.json"] = Save-Json -Name "05-shortcuts-discovery.json" -Value $shortcutRows
$paths["06-process-discovery.json"] = Save-Json -Name "06-process-discovery.json" -Value $processRows
$paths["07-cli-version-help-probes.json"] = Save-Json -Name "07-cli-version-help-probes.json" -Value $probeRows
$paths["08-marker-smoke-policy.json"] = Save-Json -Name "08-marker-smoke-policy.json" -Value $markerRows
$paths["00-discovery-summary.json"] = Save-Json -Name "00-discovery-summary.json" -Value $summary
$paths["00-discovery-summary.txt"] = Save-Text -Name "00-discovery-summary.txt" -Value ($summary | Format-List | Out-String)

[pscustomobject]@{
    generated_at_utc = $timestampUtc
    evidence_dir = $evidencePath
    output_paths = $paths
    summary = $summary
} | ConvertTo-Json -Depth 8
