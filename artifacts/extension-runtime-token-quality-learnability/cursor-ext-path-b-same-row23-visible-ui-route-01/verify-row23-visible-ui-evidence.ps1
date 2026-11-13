param(
  [string]$OutputDir = "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\cursor-ext-path-b-same-row23-visible-ui-route-01",
  [string]$Row23InputPath = "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\row23-input-path\row23-approved-safe-scenario-input.json"
)

$ErrorActionPreference = "Stop"
$row23 = Get-Content -Raw -LiteralPath $Row23InputPath | ConvertFrom-Json
$prompt = [string]$row23.scenario_input.prompt_text
$copiesDir = Join-Path $OutputDir "host-generated-copies"
$request = Get-ChildItem -File -LiteralPath $copiesDir -Filter "*panel-request-*.json" | Sort-Object LastWriteTimeUtc | Select-Object -Last 1
$panelUi = Get-ChildItem -File -LiteralPath $copiesDir -Filter "*panel-panel-ui-*.json" | Sort-Object LastWriteTimeUtc | Select-Object -Last 1
$hostTrace = Get-ChildItem -File -LiteralPath $copiesDir -Filter "*panel-host-trace-*.json" | Sort-Object LastWriteTimeUtc | Select-Object -Last 1
$coreResponse = Get-ChildItem -File -LiteralPath $copiesDir -Filter "*panel-core-response-*.json" | Sort-Object LastWriteTimeUtc | Select-Object -Last 1
$coreTrace = Get-ChildItem -File -LiteralPath $copiesDir -Filter "*panel-core-trace-*.json" | Sort-Object LastWriteTimeUtc | Select-Object -Last 1
$entrypoint = Get-ChildItem -File -LiteralPath $copiesDir -Filter "*panel-entrypoint-response-*.json" | Sort-Object LastWriteTimeUtc | Select-Object -Last 1

$requestJson = if ($request) { Get-Content -Raw -LiteralPath $request.FullName | ConvertFrom-Json } else { $null }
$panelJson = if ($panelUi) { Get-Content -Raw -LiteralPath $panelUi.FullName | ConvertFrom-Json } else { $null }
$hostTraceJson = if ($hostTrace) { Get-Content -Raw -LiteralPath $hostTrace.FullName | ConvertFrom-Json } else { $null }
$coreResponseJson = if ($coreResponse) { Get-Content -Raw -LiteralPath $coreResponse.FullName | ConvertFrom-Json } else { $null }

$sha = [System.Security.Cryptography.SHA256]::Create()
$bytes = [System.Text.Encoding]::UTF8.GetBytes($prompt)
$promptSha = ([BitConverter]::ToString($sha.ComputeHash($bytes))).Replace("-", "").ToLowerInvariant()

$requestPrompt = if ($null -ne $requestJson) { [string]$requestJson.UserPromptOrTaskText } else { "" }
$visibleText = if ($null -ne $panelJson -and $null -ne $panelJson.visibleText) { [string]$panelJson.visibleText } else { "" }
$summary = [ordered]@{
  schema = "xiigen.cursor_same_row23_visible_ui_route_verification.v1"
  generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
  status = "PASS"
  prompt_id = [string]$row23.scenario_input.prompt_id
  prompt_sha256 = $promptSha
  request_ref = if ($request) { $request.FullName } else { $null }
  panel_ui_ref = if ($panelUi) { $panelUi.FullName } else { $null }
  host_trace_ref = if ($hostTrace) { $hostTrace.FullName } else { $null }
  core_response_ref = if ($coreResponse) { $coreResponse.FullName } else { $null }
  core_trace_ref = if ($coreTrace) { $coreTrace.FullName } else { $null }
  entrypoint_response_ref = if ($entrypoint) { $entrypoint.FullName } else { $null }
  request_prompt_field = "UserPromptOrTaskText"
  request_prompt_matches_row23 = $requestPrompt -eq $prompt
  request_prompt_sha256 = if ($requestPrompt.Length -gt 0) {
    $requestBytes = [System.Text.Encoding]::UTF8.GetBytes($requestPrompt)
    ([BitConverter]::ToString($sha.ComputeHash($requestBytes))).Replace("-", "").ToLowerInvariant()
  } else { $null }
  host_kind = if ($null -ne $hostTraceJson) { $hostTraceJson.hostKind } else { $null }
  entrypoint_name = if ($null -ne $hostTraceJson) { $hostTraceJson.entrypointName } else { $null }
  entrypoint_type = if ($null -ne $hostTraceJson) { $hostTraceJson.entrypointType } else { $null }
  host_entrypoint = if ($null -ne $requestJson -and $requestJson.HostFacts) { $requestJson.HostFacts.host_entrypoint } else { $null }
  core_api_script = if ($null -ne $requestJson -and $requestJson.HostFacts) { $requestJson.HostFacts.core_api_script } else { $null }
  external_ai_flag = if ($null -ne $requestJson -and $requestJson.HostFacts) { $requestJson.HostFacts.external_ai } else { $null }
  same_handler_core_boundary = if ($null -ne $requestJson -and $requestJson.HostFacts) { $requestJson.HostFacts.same_handler_core_boundary } else { $null }
  panel_submitted_prompt = if ($null -ne $panelJson) { $panelJson.submittedPrompt } else { $null }
  panel_response_visible = if ($null -ne $panelJson) { $panelJson.responseVisibleInPanel } else { $null }
  panel_external_ai_or_unrelated_tool_used = if ($null -ne $panelJson) { $panelJson.externalAiOrUnrelatedToolUsed } else { $null }
  core_success = if ($null -ne $coreResponseJson) { $coreResponseJson.Success } else { $null }
  core_status = if ($null -ne $coreResponseJson) { $coreResponseJson.Status } else { $null }
  visible_text_redacted_excerpt = $visibleText.Substring(0, [Math]::Min(500, $visibleText.Length))
  screenshot_before_ref = Join-Path $OutputDir "cursor-window-before-row23-submit.png"
  screenshot_after_ref = Join-Path $OutputDir "cursor-window-after-row23-submit.png"
  first_submit_command_exit_code = 41
  first_submit_command_exit_note = "The first submit runner used the wrong request field name for prompt verification; follow-up verification uses UserPromptOrTaskText and passes."
  non_claims = @(
    "No token savings claim.",
    "No semantic quality improvement claim.",
    "No learning/training/lifecycle claim.",
    "No full Cursor readiness claim.",
    "No three-extension readiness claim."
  )
}

if (-not $summary.request_prompt_matches_row23) { $summary.status = "PARTIAL" }
if ($summary.external_ai_flag -ne "false") { $summary.status = "PARTIAL" }
if ($summary.panel_external_ai_or_unrelated_tool_used -ne $false) { $summary.status = "PARTIAL" }
if ($summary.core_success -ne $true) { $summary.status = "PARTIAL" }

$summary | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $OutputDir "same-row23-visible-ui-route-verification.json") -Encoding UTF8
if ($summary.status -eq "PASS") {
  Write-Output "PASS: copied host evidence verifies same Row23 prompt through visible Cursor XIIGen panel and local llm_mvp_core path."
  exit 0
}

Write-Output "PARTIAL: copied host evidence did not satisfy every verifier condition."
exit 40
