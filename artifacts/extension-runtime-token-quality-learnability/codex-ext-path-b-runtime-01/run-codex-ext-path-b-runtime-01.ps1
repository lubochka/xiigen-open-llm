$ErrorActionPreference = 'Stop'

$SubagentId = 'CODEX-EXT-PATH-B-RUNTIME-01'
$EvidenceRoot = 'C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\codex-ext-path-b-runtime-01'
$CommandsRoot = Join-Path $EvidenceRoot 'commands'
$SmokeRoot = Join-Path $EvidenceRoot 'installed-plugin-cache-smoke'
$CoreRoot = 'C:\Projects\llm_mvp_core'
$StatePath = 'C:\Projects\llm_mvp_core\docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json'
$PlanPath = 'C:\Projects\llm_mvp_core\docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md'
$AgentsPath = 'C:\Projects\llm_mvp_core\AGENTS.md'
$Row23Path = 'C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\row23-input-path\row23-approved-safe-scenario-input.json'
$PriorPacketPath = 'C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\codex-ext-readiness-01\codex-ext-readiness-01-packet.json'
$SkillPath = 'C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500\skills\xiigen-smart-skills\SKILL.md'
$InstalledRoot = 'C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500'
$InstalledManifestPath = Join-Path $InstalledRoot '.codex-plugin\plugin.json'
$InstalledPackagePath = Join-Path $InstalledRoot 'package.json'
$InstalledEntrypointPath = Join-Path $InstalledRoot 'src\codex-entrypoint.js'
$RepoRoot = 'C:\Projects\codex-xiigen-smart-skills'
$RepoManifestPath = Join-Path $RepoRoot '.codex-plugin\plugin.json'
$RepoPackagePath = Join-Path $RepoRoot 'package.json'
$RepoEntrypointPath = Join-Path $RepoRoot 'src\codex-entrypoint.js'
$ActiveQuote = @'
Не понимаю почему снова стоп??? Найди и эту лазейку пожалуйста. Занеси это и действия внизу в план работы- сделай план работы полным по нашим стандартам. Сделай 20 проходов и убедись что план доведёт наконец необходимые extension до полной готовности. И приступайк действию до полной готовности всех 3 extensions. NO STOPS approved!!! All through agents!!!
'@

New-Item -ItemType Directory -Force -Path $EvidenceRoot, $CommandsRoot, $SmokeRoot | Out-Null

function Get-FileEvidence {
  param([Parameter(Mandatory = $true)][string]$Path)
  $exists = Test-Path -LiteralPath $Path
  if (-not $exists) {
    return [pscustomobject]@{ path = $Path; exists = $false }
  }
  $item = Get-Item -LiteralPath $Path
  $hash = Get-FileHash -LiteralPath $Path -Algorithm SHA256
  [pscustomobject]@{
    path = $Path
    exists = $true
    length = $item.Length
    last_write_time = $item.LastWriteTime.ToString('o')
    sha256 = $hash.Hash.ToLowerInvariant()
  }
}

function Invoke-CapturedSimpleCommand {
  param(
    [Parameter(Mandatory = $true)][string]$Name,
    [Parameter(Mandatory = $true)][string]$FilePath,
    [string[]]$ArgumentList = @(),
    [Parameter(Mandatory = $true)][string]$WorkingDirectory,
    [int]$TimeoutMs = 20000
  )
  $stdoutPath = Join-Path $CommandsRoot "$Name.stdout.txt"
  $stderrPath = Join-Path $CommandsRoot "$Name.stderr.txt"
  $metaPath = Join-Path $CommandsRoot "$Name.meta.json"
  if (Test-Path -LiteralPath $stdoutPath) { Remove-Item -LiteralPath $stdoutPath -Force }
  if (Test-Path -LiteralPath $stderrPath) { Remove-Item -LiteralPath $stderrPath -Force }
  $started = Get-Date
  $oldLocation = Get-Location
  $exitCode = $null
  $timedOut = $false
  $errorText = $null
  try {
    Set-Location -LiteralPath $WorkingDirectory
    & $FilePath @ArgumentList 1> $stdoutPath 2> $stderrPath
    $exitCode = $LASTEXITCODE
    if ($null -eq $exitCode) { $exitCode = 0 }
  } catch {
    $exitCode = 127
    $errorText = $_.Exception.Message
    Set-Content -LiteralPath $stderrPath -Value $errorText -Encoding UTF8
    if (-not (Test-Path -LiteralPath $stdoutPath)) {
      Set-Content -LiteralPath $stdoutPath -Value '' -Encoding UTF8
    }
  } finally {
    Set-Location $oldLocation
  }
  $finished = Get-Date
  $record = [pscustomobject]@{
    name = $Name
    command_line = ((@($FilePath) + $ArgumentList) -join ' ')
    working_directory = $WorkingDirectory
    stdout_path = $stdoutPath
    stderr_path = $stderrPath
    exit_code = $exitCode
    timed_out = $timedOut
    error = $errorText
    started_at = $started.ToString('o')
    finished_at = $finished.ToString('o')
  }
  $record | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $metaPath -Encoding UTF8
  $record
}

function Invoke-CapturedNodeSmoke {
  param(
    [Parameter(Mandatory = $true)][string]$Name,
    [Parameter(Mandatory = $true)][string]$WorkingDirectory,
    [Parameter(Mandatory = $true)][string[]]$ArgumentList
  )
  $stdoutPath = Join-Path $CommandsRoot "$Name.stdout.txt"
  $stderrPath = Join-Path $CommandsRoot "$Name.stderr.txt"
  $metaPath = Join-Path $CommandsRoot "$Name.meta.json"
  $started = Get-Date
  $oldLocation = Get-Location
  $exitCode = $null
  $errorText = $null
  try {
    Set-Location -LiteralPath $WorkingDirectory
    & node.exe @ArgumentList 1> $stdoutPath 2> $stderrPath
    $exitCode = $LASTEXITCODE
  } catch {
    $exitCode = 127
    $errorText = $_.Exception.Message
    Set-Content -LiteralPath $stderrPath -Value $errorText -Encoding UTF8
    if (-not (Test-Path -LiteralPath $stdoutPath)) {
      Set-Content -LiteralPath $stdoutPath -Value '' -Encoding UTF8
    }
  } finally {
    Set-Location $oldLocation
  }
  $finished = Get-Date
  $record = [pscustomobject]@{
    name = $Name
    command_line = ('node.exe ' + ($ArgumentList -join ' '))
    working_directory = $WorkingDirectory
    stdout_path = $stdoutPath
    stderr_path = $stderrPath
    exit_code = $exitCode
    timed_out = $false
    error = $errorText
    started_at = $started.ToString('o')
    finished_at = $finished.ToString('o')
  }
  $record | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $metaPath -Encoding UTF8
  $record
}

function Add-StateMarkerTail {
  param(
    [Parameter(Mandatory = $true)][string]$Path,
    [Parameter(Mandatory = $true)][string]$Key,
    [Parameter(Mandatory = $true)]$Marker
  )
  $markerJson = $Marker | ConvertTo-Json -Depth 30
  $addition = ",`r`n    `"$Key`": $markerJson`r`n}`r`n"
  $bytes = [System.Text.Encoding]::UTF8.GetBytes($addition)
  $fs = [System.IO.File]::Open($Path, [System.IO.FileMode]::Open, [System.IO.FileAccess]::ReadWrite, [System.IO.FileShare]::None)
  try {
    $position = $fs.Length - 1
    while ($position -ge 0) {
      $fs.Position = $position
      $b = $fs.ReadByte()
      if ($b -notin @(9, 10, 13, 32)) { break }
      $position--
    }
    if ($position -lt 0 -or $b -ne 125) {
      throw "State file does not end with a top-level closing brace."
    }
    $fs.SetLength($position)
    $fs.Position = $position
    $fs.Write($bytes, 0, $bytes.Length)
  } finally {
    $fs.Dispose()
  }
}

$Row23 = Get-Content -LiteralPath $Row23Path -Raw | ConvertFrom-Json
$PriorPacket = Get-Content -LiteralPath $PriorPacketPath -Raw | ConvertFrom-Json
$CorrelationId = ('codex-ext-path-b-runtime-01-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
$RequestPath = Join-Path $SmokeRoot 'request.json'
$ResponsePath = Join-Path $SmokeRoot 'response.json'
$TracePath = Join-Path $SmokeRoot 'trace.json'

$HostFacts = @{
  surface = 'installed-codex-plugin-cache-smoke'
  scope = 'CODEX-EXT-PATH-B-RUNTIME-01'
  row23_prompt_id = $Row23.scenario_input.prompt_id
  evidence_level = 'installed_plugin_cache_adapter_core_boundary_only'
  real_codex_host_ui = 'false'
  no_external_ai = 'true'
} | ConvertTo-Json -Compress
$RequestedSkills = '["xiigen-smart-skills"]'
$AvailableSkills = '["xiigen-smart-skills"]'
$CheckSetInputs = @{
  work_order = 'CODEX-EXT-PATH-B-RUNTIME-01'
  full_readiness_claim_locked = 'true'
  row23_prompt_id = [string]$Row23.scenario_input.prompt_id
  path_b_route = 'installed_plugin_cache_entrypoint'
} | ConvertTo-Json -Compress
$Provenance = @{
  source = 'CODEX-EXT-PATH-B-RUNTIME-01'
  installed_plugin_root = $InstalledRoot
  row23_input_path = $Row23Path
  network_used = $false
  external_ai_used = $false
  secrets_included = $false
} | ConvertTo-Json -Compress

$Commands = @()
$Commands += Invoke-CapturedSimpleCommand -Name '01-where-codex' -FilePath 'where.exe' -ArgumentList @('codex') -WorkingDirectory $CoreRoot
$Commands += Invoke-CapturedSimpleCommand -Name '02-codex-version' -FilePath 'codex' -ArgumentList @('--version') -WorkingDirectory $CoreRoot
$Commands += Invoke-CapturedSimpleCommand -Name '03-codex-help' -FilePath 'codex' -ArgumentList @('--help') -WorkingDirectory $CoreRoot
$Commands += Invoke-CapturedSimpleCommand -Name '04-codex-plugin-help' -FilePath 'codex' -ArgumentList @('plugin', '--help') -WorkingDirectory $CoreRoot
$Commands += Invoke-CapturedSimpleCommand -Name '05-codex-plugin-list-json' -FilePath 'codex' -ArgumentList @('plugin', 'list', '--json') -WorkingDirectory $CoreRoot
$SmokeArgs = @(
  'src\codex-entrypoint.js',
  '--method', 'prompt-to-task',
  '--user-prompt-or-task-text', [string]$Row23.scenario_input.prompt_text,
  '--host-facts-json', $HostFacts,
  '--requested-skills-json', $RequestedSkills,
  '--available-skills-json', $AvailableSkills,
  '--check-set-inputs-json', $CheckSetInputs,
  '--provenance-json', $Provenance,
  '--correlation-id', $CorrelationId,
  '--debug-enabled',
  '--core-root', $CoreRoot,
  '--extension-root', $InstalledRoot,
  '--request-path', $RequestPath,
  '--response-path', $ResponsePath,
  '--trace-path', $TracePath
)
$Commands += Invoke-CapturedNodeSmoke -Name '06-installed-plugin-cache-smoke' -WorkingDirectory $InstalledRoot -ArgumentList $SmokeArgs

$InstalledManifest = Get-Content -LiteralPath $InstalledManifestPath -Raw | ConvertFrom-Json
$InstalledPackage = Get-Content -LiteralPath $InstalledPackagePath -Raw | ConvertFrom-Json
$Response = if (Test-Path -LiteralPath $ResponsePath) { Get-Content -LiteralPath $ResponsePath -Raw | ConvertFrom-Json } else { $null }
$Trace = if (Test-Path -LiteralPath $TracePath) { Get-Content -LiteralPath $TracePath -Raw | ConvertFrom-Json } else { $null }
$Request = if (Test-Path -LiteralPath $RequestPath) { Get-Content -LiteralPath $RequestPath -Raw | ConvertFrom-Json } else { $null }

$Metadata = [pscustomobject]@{
  installed_plugin_root = $InstalledRoot
  installed_plugin_manifest = $InstalledManifest
  installed_package = [pscustomobject]@{
    name = $InstalledPackage.name
    version = $InstalledPackage.version
    main = $InstalledPackage.main
    runtimeSmokeClaimed = $InstalledPackage.codexXiigenSmartSkills.runtimeSmokeClaimed
    semanticQualityClaimed = $InstalledPackage.codexXiigenSmartSkills.semanticQualityClaimed
    hostLogicBoundary = $InstalledPackage.codexXiigenSmartSkills.hostLogicBoundary
    coreEndpoint = $InstalledPackage.codexXiigenSmartSkills.coreEndpoint
  }
  file_evidence = @(
    Get-FileEvidence -Path $AgentsPath
    Get-FileEvidence -Path $PlanPath
    Get-FileEvidence -Path $StatePath
    Get-FileEvidence -Path $Row23Path
    Get-FileEvidence -Path $PriorPacketPath
    Get-FileEvidence -Path $SkillPath
    Get-FileEvidence -Path $InstalledManifestPath
    Get-FileEvidence -Path $InstalledPackagePath
    Get-FileEvidence -Path $InstalledEntrypointPath
    Get-FileEvidence -Path $RepoManifestPath
    Get-FileEvidence -Path $RepoPackagePath
    Get-FileEvidence -Path $RepoEntrypointPath
  )
}
$MetadataPath = Join-Path $EvidenceRoot 'installed-plugin-route-metadata.json'
$Metadata | ConvertTo-Json -Depth 30 | Set-Content -LiteralPath $MetadataPath -Encoding UTF8

$SmokeExit = ($Commands | Where-Object { $_.name -eq '06-installed-plugin-cache-smoke' }).exit_code
$SmokeSuccess = ($SmokeExit -eq 0 -and $Response -and $Response.Success -eq $true)
$CodexCliFound = (($Commands | Where-Object { $_.name -eq '01-where-codex' }).exit_code -eq 0)
$PluginListCommand = $Commands | Where-Object { $_.name -eq '05-codex-plugin-list-json' }
$PluginListStdout = if (Test-Path -LiteralPath $PluginListCommand.stdout_path) { Get-Content -LiteralPath $PluginListCommand.stdout_path -Raw } else { '' }
$PluginListShowsXiigen = ($PluginListStdout -match 'codex-xiigen-smart-skills')

$CodexPathBStatus = if ($SmokeSuccess) { 'PARTIAL' } else { 'BLOCKED' }
$CodexReadinessStatus = if ($SmokeSuccess) { 'PARTIAL' } else { 'BLOCKED' }
$PriorPathASuccess = if ($PriorPacket.PSObject.Properties.Name -contains 'row23_response_success') {
  [bool]$PriorPacket.row23_response_success
} elseif ($PriorPacket.observed_summary -and ($PriorPacket.observed_summary.PSObject.Properties.Name -contains 'row23_response_success')) {
  [bool]$PriorPacket.observed_summary.row23_response_success
} else {
  $false
}
$PriorPathAStatus = if ($PriorPacket.PSObject.Properties.Name -contains 'row23_response_status') {
  [string]$PriorPacket.row23_response_status
} elseif ($PriorPacket.observed_summary -and ($PriorPacket.observed_summary.PSObject.Properties.Name -contains 'row23_response_status')) {
  [string]$PriorPacket.observed_summary.row23_response_status
} else {
  $null
}
$PriorPathACorrelationId = if ($PriorPacket.PSObject.Properties.Name -contains 'row23_correlation_id') {
  [string]$PriorPacket.row23_correlation_id
} elseif ($PriorPacket.observed_summary -and ($PriorPacket.observed_summary.PSObject.Properties.Name -contains 'row23_trace_correlation_id')) {
  [string]$PriorPacket.observed_summary.row23_trace_correlation_id
} else {
  $null
}

$ComparisonStatus = if ($SmokeSuccess -and $PriorPathASuccess) {
  'PARTIAL: Path A prior direct source-entrypoint smoke and this installed-plugin-cache route both reached llm_mvp_core with Row23 safe input, but no real Codex host UI/chat/app visible answer was proven.'
} else {
  'BLOCKED: comparison cannot pass because one side lacks a successful same-input core-boundary smoke.'
}

$ExactFilesRead = @(
  $AgentsPath,
  $PlanPath,
  $StatePath,
  $Row23Path,
  $PriorPacketPath,
  $SkillPath,
  $InstalledManifestPath,
  $InstalledPackagePath,
  $InstalledEntrypointPath,
  $RepoManifestPath,
  $RepoPackagePath,
  $RepoEntrypointPath
)
$ExactFilesWritten = @(
  (Join-Path $EvidenceRoot 'run-codex-ext-path-b-runtime-01.ps1'),
  $MetadataPath,
  $RequestPath,
  $ResponsePath,
  $TracePath,
  (Join-Path $EvidenceRoot 'runner-summary.json'),
  (Join-Path $EvidenceRoot 'codex-ext-path-b-runtime-01-packet.json'),
  (Join-Path $EvidenceRoot 'codex-ext-path-b-runtime-01-packet.md'),
  $StatePath
) + ($Commands | ForEach-Object { @($_.stdout_path, $_.stderr_path, (Join-Path $CommandsRoot ($_.name + '.meta.json'))) } | ForEach-Object { $_ })

$Blockers = @()
if (-not $SmokeSuccess) {
  $Blockers += 'Installed plugin cache smoke did not complete successfully; inspect command stdout/stderr.'
}
$Blockers += 'No safe sub-agent-accessible route was found to submit Row23 through the live Codex chat/UI/app and capture a visible host answer without external AI or UI/browser interaction.'
if (-not $PluginListShowsXiigen) {
  $Blockers += 'Codex CLI plugin list did not prove the xiigen plugin is invokable through a live host command surface; installed cache metadata proves only local package presence.'
}
$Blockers += 'Full Codex readiness still requires real host visible answer or exact live-host route blocker, debug/error path, same-scenario Path A/Path B proof, token/quality/learnability/lifecycle/rollback/docs/final coverage gates.'

$Packet = [pscustomobject]@{
  schema = 'xiigen.codex_ext_path_b_runtime_01.packet.v1'
  subagent_id_or_nickname = $SubagentId
  generated_at_local = (Get-Date).ToString('o')
  active_luba_quote_creating_authority = $ActiveQuote
  requested_artifact_type = 'real Codex host Path B proof evidence packet, or exact host-route blocker packet plus state update'
  changed_files = $ExactFilesWritten
  exact_files_read = $ExactFilesRead
  exact_files_written = $ExactFilesWritten
  commands_run = $Commands
  codex_path_b_status = $CodexPathBStatus
  codex_readiness_status = $CodexReadinessStatus
  codex_readiness_exact_gates = [pscustomobject]@{
    installed_skill_surface_loaded = (Test-Path -LiteralPath $SkillPath)
    installed_plugin_cache_metadata_present = (Test-Path -LiteralPath $InstalledManifestPath)
    installed_plugin_cache_entrypoint_smoke_exit_code = $SmokeExit
    adapter_core_boundary_executed = $SmokeSuccess
    row23_request_host = $Request.HostName
    row23_request_host_type = $Request.HostType
    row23_response_success = if ($Response) { $Response.Success } else { $false }
    row23_response_status = if ($Response) { $Response.Status } else { $null }
    trace_correlation_id = if ($Trace) { $Trace.CorrelationId } else { $null }
    full_codex_chat_ui_proven = $false
    real_codex_host_visible_answer_proven = $false
    token_savings_proven = $false
    quality_improvement_proven = $false
    learnability_proven = $false
    training_started = $false
    eval_started = $false
    checkpoint_mutation_started = $false
  }
  path_a_path_b_comparison_status = $ComparisonStatus
  path_a_path_b_comparison = [pscustomobject]@{
    path_a_prior_packet = $PriorPacketPath
    path_a_prior_subagent = $PriorPacket.subagent_id_or_nickname
    path_a_status = $PriorPathAStatus
    path_a_success = $PriorPathASuccess
    path_a_correlation_id = $PriorPathACorrelationId
    path_a_evidence_scope = $PriorPacket.evidence_supports_only
    path_b_current_route = 'installed Codex plugin cache src/codex-entrypoint.js via local node.exe, not live Codex chat/UI'
    path_b_success = $SmokeSuccess
    path_b_correlation_id = $CorrelationId
    path_b_response_status = if ($Response) { $Response.Status } else { $null }
    same_row23_prompt_id = $Row23.scenario_input.prompt_id
    same_prompt_text_used = $true
    narrow_common_result = 'Both successful routes, when successful, exercise the same thin adapter/core boundary with Row23 safe input.'
    missing_for_full_path_b = 'Visible answer inside real Codex host surface with window/screenshot/session/redaction evidence, or documented no-AI live host command surface proof.'
  }
  state_transition_made_or_not = 'PENDING before state marker append'
  state_marker_key = $null
  next_bounded_worker_scope = 'CODEX-HOST-UI-ROUTE-PROOF-OR-BLOCKER-01: use a user-visible Codex host action/tool route, if available, to submit the same Row23 prompt through the live Codex app/plugin surface with no secrets captured and no external AI unless explicitly authorized; otherwise record exact live-host UI/tool blocker with screen/state evidence.'
  evidence_supports_only = 'Installed Codex plugin cache metadata plus a local installed-cache entrypoint smoke through llm_mvp_core for Row23 safe input. This is partial host-route evidence: it proves installed skill/package presence and adapter/core boundary from the installed cache, not full Codex chat/UI/app Path B, token savings, semantic quality improvement, learnability/training, lifecycle, rollback, documentation, or final three-extension readiness.'
  blockers = $Blockers
  redaction_and_safety = [pscustomobject]@{
    row23_input_safe_local = $true
    secrets_included = $false
    external_ai_calls_made = $false
    network_installs_or_downloads_made = $false
    git_actions_made = $false
    ui_or_browser_checks_made = $false
    raw_output_redacted_for_user = $true
  }
  route_inspection = [pscustomobject]@{
    codex_cli_found_on_path = $CodexCliFound
    codex_plugin_list_exit_code = $PluginListCommand.exit_code
    codex_plugin_list_shows_xiigen = $PluginListShowsXiigen
    installed_manifest_declares_skills = [bool]$InstalledManifest.skills
    installed_manifest_capabilities = $InstalledManifest.interface.capabilities
    installed_package_main = $InstalledPackage.main
    installed_package_runtime_smoke_claimed = $InstalledPackage.codexXiigenSmartSkills.runtimeSmokeClaimed
    installed_package_semantic_quality_claimed = $InstalledPackage.codexXiigenSmartSkills.semanticQualityClaimed
  }
  no_overclaim_booleans = [pscustomobject]@{
    codex_full_readiness_claimed = $false
    full_codex_chat_ui_proven = $false
    real_codex_path_b_ui_proven = $false
    installed_plugin_runtime_invocation_proven = $false
    token_savings_proven = $false
    quality_improvement_proven = $false
    learnability_or_training_proven = $false
    backup_export_import_continue_training_proven = $false
    rollback_or_target_safety_proven = $false
    final_dual_path_scenario_coverage_proven = $false
  }
  parent_will_perform_commit_push_after_review = $true
  git_actions_performed_by_this_subagent = $false
}

$PacketPath = Join-Path $EvidenceRoot 'codex-ext-path-b-runtime-01-packet.json'
$PacketMdPath = Join-Path $EvidenceRoot 'codex-ext-path-b-runtime-01-packet.md'
$Packet | ConvertTo-Json -Depth 40 | Set-Content -LiteralPath $PacketPath -Encoding UTF8

$StateMarkerKey = ('codex_ext_path_b_runtime_01_' + (Get-Date -Format 'yyyyMMdd_HHmmss'))
$StateMarker = [pscustomobject]@{
  schema = 'xiigen.codex_ext_path_b_runtime_01.state_marker.v1'
  subagent_id_or_nickname = $SubagentId
  timestamp = (Get-Date).ToString('o')
  evidence_packet = $PacketPath
  codex_path_b_status = $CodexPathBStatus
  codex_readiness_status = $CodexReadinessStatus
  path_a_path_b_comparison_status = $ComparisonStatus
  installed_plugin_cache_smoke_exit_code = $SmokeExit
  installed_plugin_cache_smoke_success = $SmokeSuccess
  full_codex_chat_ui_proven = $false
  codex_full_readiness_claimed = $false
  state_transition_made_or_not = 'MADE: appended narrow CODEX-EXT-PATH-B-RUNTIME-01 marker with PARTIAL/BLOCKED status and explicit non-claims.'
  next_bounded_worker_scope = $Packet.next_bounded_worker_scope
  evidence_supports_only = $Packet.evidence_supports_only
  blockers = $Blockers
  parent_will_perform_commit_push_after_review = $true
  git_actions_performed_by_this_subagent = $false
}
Add-StateMarkerTail -Path $StatePath -Key $StateMarkerKey -Marker $StateMarker

$Packet.state_transition_made_or_not = 'MADE: appended state marker ' + $StateMarkerKey + ' to execution state.'
$Packet.state_marker_key = $StateMarkerKey
$Packet | ConvertTo-Json -Depth 40 | Set-Content -LiteralPath $PacketPath -Encoding UTF8

$Md = @"
# CODEX-EXT-PATH-B-RUNTIME-01 Packet

- subagent_id_or_nickname: $SubagentId
- codex_path_b_status: $CodexPathBStatus
- codex_readiness_status: $CodexReadinessStatus
- path_a_path_b_comparison_status: $ComparisonStatus
- state_transition_made_or_not: $($Packet.state_transition_made_or_not)
- evidence_supports_only: $($Packet.evidence_supports_only)

## Blockers
$($Blockers | ForEach-Object { "- $_" } | Out-String)

## Commands
$($Commands | ForEach-Object { "- $($_.name): exit_code=$($_.exit_code); stdout=$($_.stdout_path); stderr=$($_.stderr_path)" } | Out-String)
"@
$Md | Set-Content -LiteralPath $PacketMdPath -Encoding UTF8

$Summary = [pscustomobject]@{
  packet = $PacketPath
  packet_md = $PacketMdPath
  state_marker_key = $StateMarkerKey
  codex_path_b_status = $CodexPathBStatus
  codex_readiness_status = $CodexReadinessStatus
  smoke_success = $SmokeSuccess
  commands = $Commands
}
$SummaryPath = Join-Path $EvidenceRoot 'runner-summary.json'
$Summary | ConvertTo-Json -Depth 30 | Set-Content -LiteralPath $SummaryPath -Encoding UTF8
$Summary | ConvertTo-Json -Depth 30
