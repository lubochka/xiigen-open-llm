# Row12 core API probe report

Status: CORE_API_PROBE_READY
Pass: true

## Scope
Smallest row12 core API probe only: one local `tools\xiigen-extension-core-api.ps1` call with the accepted row12 no-send prompt.

## Extracted spec values
- marker: `FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1`
- scenario_id: `frdp-p01-cursor-token-savings-baseline`
- pair_id: `frdp-p01-row12-cursor-token-baseline-v1`
- method: `prompt-to-task`
- host: `Cursor`
- prompt_hash_from_spec: `sha256:7140fd94d9e6df18b4abef4f13317146ef41a5db7ba1953e8579ffcb783cb374`
- prompt_hash_computed: `sha256:7140fd94d9e6df18b4abef4f13317146ef41a5db7ba1953e8579ffcb783cb374`
- prompt_hash_matches_spec: `true`

## Command run
```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "C:\Projects\llm_mvp_core\tools\xiigen-extension-core-api.ps1" -RequestPath "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\core-api-probe-20260625T011410\core-api-probe-request.json" -ResponsePath "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\core-api-probe-20260625T011410\core-api-probe-response.json" -TracePath "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\core-api-probe-20260625T011410\core-api-probe-trace.json" -CoreRoot "C:\Projects\llm_mvp_core"
```
- exit_code: `0`
- stdout: `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\core-api-probe-20260625T011410\core-api-probe-command-stdout.txt`
- stderr: `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\core-api-probe-20260625T011410\core-api-probe-command-stderr.txt`

## Response
- response_returned: `true`
- response_success: `true`
- response_status: `core_boundary_partial_no_semantic_quality_claim`
- debug_trace_returned: `true`
- trace_artifact_returned: `true`
- trace_external_ai_used: `unknown`
- trace_host_runtime_claimed: `unknown`

## What this proves
- The local core/API script accepted the row12 no-send request JSON and returned a response artifact for `prompt-to-task`.
- This is core/API boundary evidence only.

## What this does not prove
- No token savings claim.
- No product readiness claim.
- No full Cursor host/runtime readiness claim.
- No semantic quality improvement claim.
- No training, checkpoint promotion, or locator update claim.

## Created files
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\core-api-probe-20260625T011410\core-api-probe-request.json
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\core-api-probe-20260625T011410\core-api-probe-response.json
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\core-api-probe-20260625T011410\core-api-probe-trace.json
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\core-api-probe-20260625T011410\core-api-probe-command-stdout.txt
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\core-api-probe-20260625T011410\core-api-probe-command-stderr.txt
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\core-api-probe-20260625T011410\core-api-probe-command-result.json
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\core-api-probe-20260625T011410\core-api-probe-report.json
- C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\core-api-probe-20260625T011410\core-api-probe-report.md
