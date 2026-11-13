$ErrorActionPreference = "Stop"
$EvidenceRoot = "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\codex-host-ui-access-recovery-01"
$statePath = "C:\Users\lubao\.codex\.codex-global-state.json"
$outPath = Join-Path $EvidenceRoot "codex-global-state-route-key-inspection.json"

function Get-RouteKeyShape {
  param(
    [object]$Value,
    [string]$Path,
    [int]$Depth
  )

  if ($Depth -gt 5 -or $null -eq $Value) {
    return @()
  }

  $rows = @()
  if ($Value -is [System.Collections.IEnumerable] -and $Value -isnot [string] -and $Value -isnot [pscustomobject]) {
    $count = @($Value).Count
    if ($Path -match "route|thread|tab|url|browser|window|app|session") {
      $rows += [ordered]@{ path = $Path; shape = "array"; count = $count; value_captured = $false }
    }
    foreach ($item in @($Value | Select-Object -First 5)) {
      $rows += Get-RouteKeyShape -Value $item -Path "$Path[]" -Depth ($Depth + 1)
    }
    return $rows
  }

  if ($Value -is [pscustomobject]) {
    foreach ($prop in $Value.PSObject.Properties) {
      $childPath = if ($Path) { "$Path.$($prop.Name)" } else { $prop.Name }
      $childValue = $prop.Value
      $shape = if ($null -eq $childValue) { "null" } elseif ($childValue -is [pscustomobject]) { "object" } elseif ($childValue -is [System.Collections.IEnumerable] -and $childValue -isnot [string]) { "array" } else { $childValue.GetType().Name }
      if ($childPath -match "route|thread|tab|url|browser|window|app|session") {
        $rows += [ordered]@{ path = $childPath; shape = $shape; value_captured = $false }
      }
      $rows += Get-RouteKeyShape -Value $childValue -Path $childPath -Depth ($Depth + 1)
    }
  }
  return $rows
}

$item = Get-Item -LiteralPath $statePath
$json = Get-Content -Raw -LiteralPath $statePath
$parsed = $json | ConvertFrom-Json
$topLevelKeys = @($parsed.PSObject.Properties.Name)
$matches = @(Get-RouteKeyShape -Value $parsed -Path "" -Depth 0)
$report = [ordered]@{
  schema = "xiigen.codex_host_ui_access_recovery_01.global_state_route_key_inspection.v1"
  generated_at_local = (Get-Date).ToString("o")
  inspected_file = $statePath
  inspected_file_length = $item.Length
  value_capture_policy = "Only property names, type shapes, and counts for arrays are captured; scalar values are not emitted."
  no_secrets_captured_by_script_design = $true
  top_level_key_count = $topLevelKeys.Count
  top_level_keys = $topLevelKeys
  matching_route_key_shapes = $matches
  route_value_or_usable_handle_emitted = $false
}
$report | ConvertTo-Json -Depth 12 | Out-File -LiteralPath $outPath -Encoding utf8
Write-Output $outPath
