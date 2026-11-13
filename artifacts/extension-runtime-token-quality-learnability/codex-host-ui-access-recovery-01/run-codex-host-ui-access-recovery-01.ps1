param(
  [string]$EvidenceRoot = "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\codex-host-ui-access-recovery-01"
)

$ErrorActionPreference = "Continue"
$commandsDir = Join-Path $EvidenceRoot "commands"
New-Item -ItemType Directory -Force -Path $EvidenceRoot, $commandsDir | Out-Null

function Invoke-EvidenceCommand {
  param(
    [string]$Name,
    [scriptblock]$ScriptBlock
  )

  $stdoutPath = Join-Path $commandsDir "$Name.stdout.txt"
  $stderrPath = Join-Path $commandsDir "$Name.stderr.txt"
  $metaPath = Join-Path $commandsDir "$Name.meta.json"
  $stdout = $null
  $stderrText = ""
  $exitCode = 0
  $started = Get-Date
  try {
    $stdout = & $ScriptBlock 2>&1
    $nativeExit = $global:LASTEXITCODE
    if ($null -ne $nativeExit) {
      $exitCode = [int]$nativeExit
    }
    $errorRecords = @($stdout | Where-Object { $_ -is [System.Management.Automation.ErrorRecord] })
    if ($errorRecords.Count -gt 0 -and $exitCode -eq 0) {
      $exitCode = 127
    }
    $stderrText = ($errorRecords | ForEach-Object { $_.ToString() }) -join [Environment]::NewLine
    $stdout = @($stdout | Where-Object { $_ -isnot [System.Management.Automation.ErrorRecord] })
  } catch {
    $exitCode = 127
    $stderrText = $_.Exception.Message
    $stdout = @()
  }
  $finished = Get-Date
  $stdout | Out-File -LiteralPath $stdoutPath -Encoding utf8
  $stderrText | Out-File -LiteralPath $stderrPath -Encoding utf8
  $meta = [ordered]@{
    name = $Name
    started_local = $started.ToString("o")
    finished_local = $finished.ToString("o")
    exit_code = $exitCode
    stdout_path = $stdoutPath
    stderr_path = $stderrPath
    stderr_preview = if ($stderrText.Length -gt 500) { $stderrText.Substring(0, 500) } else { $stderrText }
  }
  $meta | ConvertTo-Json -Depth 8 | Out-File -LiteralPath $metaPath -Encoding utf8
  return $meta
}

$commandResults = @()
$commandResults += Invoke-EvidenceCommand "01-get-command-codex" { Get-Command codex -All | Select-Object CommandType, Name, Source, Version }
$commandResults += Invoke-EvidenceCommand "02-codex-version" { codex --version }
$commandResults += Invoke-EvidenceCommand "03-codex-help" { codex --help }
$commandResults += Invoke-EvidenceCommand "04-codex-plugin-list-json" { codex plugin list --json }
$commandResults += Invoke-EvidenceCommand "05-codex-plugin-help" { codex plugin --help }

$pluginRoot = "C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500"
$knownCodexPaths = @(
  "C:\Users\lubao\.codex",
  "C:\Users\lubao\AppData\Roaming\Codex",
  "C:\Users\lubao\AppData\Local\Codex",
  "C:\Users\lubao\AppData\Local\Programs\Codex",
  "C:\Users\lubao\AppData\Roaming\com.openai.chat",
  "C:\Users\lubao\AppData\Local\com.openai.chat"
)

$pathInventory = foreach ($p in $knownCodexPaths) {
  $exists = Test-Path -LiteralPath $p
  $children = @()
  if ($exists) {
    $children = @(Get-ChildItem -LiteralPath $p -Force -ErrorAction SilentlyContinue |
      Select-Object -First 40 Name, Mode, Length, LastWriteTime)
  }
  [ordered]@{
    path = $p
    exists = $exists
    child_count_reported_max_40 = $children.Count
    children = $children
  }
}

$pluginInventory = [ordered]@{
  plugin_root = $pluginRoot
  root_exists = Test-Path -LiteralPath $pluginRoot
  manifest_exists = Test-Path -LiteralPath (Join-Path $pluginRoot ".codex-plugin\plugin.json")
  package_json_exists = Test-Path -LiteralPath (Join-Path $pluginRoot "package.json")
  entrypoint_exists = Test-Path -LiteralPath (Join-Path $pluginRoot "src\codex-entrypoint.js")
  skill_exists = Test-Path -LiteralPath (Join-Path $pluginRoot "skills\xiigen-smart-skills\SKILL.md")
}

$processInventory = Get-Process -ErrorAction SilentlyContinue |
  Where-Object { $_.ProcessName -match "Codex|Code|Cursor|Claude" } |
  Select-Object ProcessName, Id, Path, StartTime

$summary = [ordered]@{
  schema = "xiigen.codex_host_ui_access_recovery_01.route_discovery.v1"
  generated_at_local = (Get-Date).ToString("o")
  no_secrets_captured = $true
  window_titles_captured = $false
  command_results = $commandResults
  plugin_inventory = $pluginInventory
  known_codex_path_inventory = $pathInventory
  process_inventory_without_window_titles = $processInventory
}

$summaryPath = Join-Path $EvidenceRoot "route-discovery-summary.json"
$summary | ConvertTo-Json -Depth 12 | Out-File -LiteralPath $summaryPath -Encoding utf8
Write-Output $summaryPath
