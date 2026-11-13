$ErrorActionPreference = 'Stop'
$root = 'C:\Projects\llm_mvp_core'
$statePath = Join-Path $root 'docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json'
$reviewPath = Join-Path $root 'docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1-REVIEW.jsonl'
$artifactRoot = Join-Path $root 'artifacts\extension-runtime-token-quality-learnability'
$bytes = [System.IO.File]::ReadAllBytes($statePath)
$bom = ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF)
$stateText = [System.Text.Encoding]::UTF8.GetString($bytes)
$state = $stateText | ConvertFrom-Json
$markers = $state.recovered_top_level_state_markers
$reviewLines = Get-Content -LiteralPath $reviewPath
$reviewIds = @()
foreach ($line in $reviewLines) {
  if ($line.Trim()) {
    $o = $line | ConvertFrom-Json
    if ($o.current_pass_id) { $reviewIds += [string]$o.current_pass_id }
    elseif ($o.pass) { $reviewIds += ('pass-' + $o.pass) }
  }
}
$dirs = Get-ChildItem -LiteralPath $artifactRoot -Directory |
  Where-Object { $_.Name -match 'claude|cursor|codex|row23|plan-review|current-blocker|04c|path-b' } |
  Sort-Object LastWriteTime -Descending |
  Select-Object -First 40 Name, LastWriteTime, FullName
[pscustomobject]@{
  state_utf8_bom = $bom
  state_top_keys = @($state.PSObject.Properties.Name)
  marker_names = @($markers.PSObject.Properties.Name)
  claude_marker = $markers.claude_live_host_route_proof_or_blocker_01_20260628
  cursor_marker = $markers.cursor_ext_readiness_01_20260628
  row20_codex_marker = $markers.row20_status_board_sync_20260626
  row21_codex_marker = $markers.row21_status_board_sync_20260626
  row22_codex_marker = $markers.row22_status_board_sync_20260626
  row23_marker = $markers.row23_status_board_sync_20260628
  plan_repair_marker = $markers.plan_repair_stop_loophole_20260628
  review_nonblank_lines = @($reviewLines | Where-Object { $_.Trim() }).Count
  review_parsed_ids = $reviewIds
  relevant_artifact_dirs = $dirs
} | ConvertTo-Json -Depth 12
