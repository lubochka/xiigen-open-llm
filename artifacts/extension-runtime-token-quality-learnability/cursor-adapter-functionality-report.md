# Cursor Adapter Functionality Report - HOST-IMPL-02

Date: 2026-06-16.

Verdict: `PASS_ADAPTER_FUNCTIONALITY_ONLY_AFTER_REPAIR`.

This report covers Cursor thin adapter functionality only. It does not claim full Cursor Extension Host/UI runtime proof, model quality, token improvement, training, checkpoint mutation, or end-to-end extension completion.

## Inspected Files

- `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\runtime-map.md`
- `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\runtime-map.json`
- `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\parent-review-HOST-IMPL-01.md`
- `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\parent-review-HOST-IMPL-01.json`
- `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\XiigenExtensionApiContracts.cs`
- `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\XiigenExtensionApiService.cs`
- `C:\Projects\llm_mvp_core\tools\xiigen-extension-core-api.ps1`
- `C:\Projects\cursor-xiigen-smart-skills\package.json`
- `C:\Projects\cursor-xiigen-smart-skills\src\extension.ts`
- `C:\Projects\cursor-xiigen-smart-skills\dist\extension.js`
- `C:\Projects\cursor-xiigen-smart-skills\scripts\install-smoke.ps1`
- `C:\Projects\cursor-xiigen-smart-skills\scripts\debug-trace-helper.js`
- `C:\Projects\cursor-xiigen-smart-skills\scripts\local-runtime-helper.js`
- `C:\Projects\cursor-xiigen-smart-skills\README.md`
- `C:\Projects\cursor-xiigen-smart-skills\docs\PROJECT-DOC.md`
- `C:\Projects\cursor-xiigen-smart-skills\xiigen-extension-contract.json`
- `C:\Projects\cursor-xiigen-smart-skills\prompts\cursor-smoke-prompts.jsonl`

## Changed Files

- `C:\Projects\cursor-xiigen-smart-skills\src\extension.ts`
- `C:\Projects\cursor-xiigen-smart-skills\dist\extension.js`
- `C:\Projects\cursor-xiigen-smart-skills\scripts\install-smoke.ps1`
- `C:\Projects\cursor-xiigen-smart-skills\scripts\debug-trace-helper.js`
- `C:\Projects\cursor-xiigen-smart-skills\scripts\local-runtime-helper.js`
- `C:\Projects\cursor-xiigen-smart-skills\package.json`
- `C:\Projects\cursor-xiigen-smart-skills\README.md`
- `C:\Projects\cursor-xiigen-smart-skills\docs\PROJECT-DOC.md`
- `C:\Projects\cursor-xiigen-smart-skills\prompts\cursor-smoke-prompts.jsonl`
- `C:\Projects\cursor-xiigen-smart-skills\reports\HOST-IMPL-02-probe\install-smoke\install-smoke-report.json`
- `C:\Projects\cursor-xiigen-smart-skills\reports\HOST-IMPL-02-probe\install-smoke\install-smoke-trace.json`
- `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\cursor-adapter-functionality-report.md`
- `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\cursor-adapter-functionality-report.json`
- `C:\Projects\llm_mvp_core\docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`

## Entrypoint And Manifest

- Manifest: `C:\Projects\cursor-xiigen-smart-skills\package.json`
- Runtime main: `C:\Projects\cursor-xiigen-smart-skills\dist\extension.js`
- Source entrypoint: `C:\Projects\cursor-xiigen-smart-skills\src\extension.ts`
- User-visible adapter paths declared by manifest: command `xiigen.smartSkills.select`, panel command `xiigenSmartSkills.panel`, chat participant `xiigen-smart-skills`
- Core endpoint: `C:\Projects\llm_mvp_core\tools\xiigen-extension-core-api.ps1`
- Core boundary: `XiigenExtensionApiService.ProcessHostExtensionRequest`

## Request/Response Evidence

The focused local probe built `XiigenExtensionRequest` objects for Cursor and invoked the core API script. Both debug-off and debug-on cases returned `core_boundary_partial_no_semantic_quality_claim` with `coreExitCode=0`.

All runtime proof fields are false for this phase:

- `runtimeEvidence=false`
- `trueHostRuntimeEvidence=false`
- `true_host_runtime_evidence=false`
- `coreApiRuntimeEvidence=false`
- `fullCursorExtensionHostUiProof=false`

Evidence refs:

- Debug off request: `C:\Projects\cursor-xiigen-smart-skills\reports\HOST-IMPL-02-probe\install-smoke\debug-off\xiigen-cursor-request.json`
- Debug off response: `C:\Projects\cursor-xiigen-smart-skills\reports\HOST-IMPL-02-probe\install-smoke\debug-off\xiigen-cursor-core-response.json`
- Debug off host trace: `C:\Projects\cursor-xiigen-smart-skills\reports\HOST-IMPL-02-probe\install-smoke\debug-off\xiigen-cursor-host-trace.json`
- Debug on request: `C:\Projects\cursor-xiigen-smart-skills\reports\HOST-IMPL-02-probe\install-smoke\debug-on\xiigen-cursor-request.json`
- Debug on response: `C:\Projects\cursor-xiigen-smart-skills\reports\HOST-IMPL-02-probe\install-smoke\debug-on\xiigen-cursor-core-response.json`
- Debug on host trace: `C:\Projects\cursor-xiigen-smart-skills\reports\HOST-IMPL-02-probe\install-smoke\debug-on\xiigen-cursor-host-trace.json`

## Debug And Privacy Evidence

Debug-on evidence contains exactly the required visible display shape:

```text
xiigen prompt-to-task :
input: <redacted>
output: <redacted>
```

The raw sentinel `PRIVATE_SENTINEL_DO_NOT_DISPLAY` appears only in the request payload files, where `UserPromptOrTaskText` belongs by contract. It was absent from display text, response, core trace, host trace, and entrypoint response. JSON probe outputs parsed strictly and had `bom=false`.

Repair note: the adapter no longer renders JSON after `input:` or `output:` in visible debug text. Detailed request/response refs remain in trace files.

## Error State Evidence

The Cursor adapter source now keeps explicit core-missing and core-failed paths:

- `core_endpoint_missing`
- `cursor_extension_host_entrypoint_core_api_missing`
- `cursor_extension_host_entrypoint_core_api_failed`

These paths return response-shaped failures with no host semantic decision, no training, no external AI, and no runtime proof claim.

## Boundary Review

PASS for HOST-IMPL-02 adapter functionality:

- Cursor builds `XiigenExtensionRequest`.
- Cursor calls the core API script and receives `XiigenExtensionResponse`.
- Cursor displays user/chat/panel text through `CursorDisplayText`, markdown stream, information message, or panel HTML path depending on entrypoint.
- Cursor can show redacted debug input/output in the exact method/input/output shape.
- Prompt-to-task, orchestration, skill selection, model logic, DPO, training, and checkpoint mutation remain outside the adapter.
- No external AI or model-backed command was used.
- No full Cursor Extension Host/UI runtime proof was claimed.

## Probes Run

- `node --check C:\Projects\cursor-xiigen-smart-skills\dist\extension.js` - PASS
- `node --check C:\Projects\cursor-xiigen-smart-skills\scripts\debug-trace-helper.js` - PASS
- `node --check C:\Projects\cursor-xiigen-smart-skills\scripts\local-runtime-helper.js` - PASS
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:\Projects\cursor-xiigen-smart-skills\scripts\install-smoke.ps1` - PASS
- `npm run install-smoke` - PASS
- runtime overclaim scan over current source/dist/scripts/HOST-IMPL-02 outputs - PASS, no positive host-runtime evidence flags or runtime-scope success wording found
- exact debug display scan - PASS, no JSON debug display found
- stale Phase40 endpoint/metadata scan over current source/scripts/docs/package/prompts - PASS, no active stale endpoint or metadata references
- sentinel scan over HOST-IMPL-02 probe outputs - PASS, sentinel only in request payload files
- strict JSON/BOM check for HOST-IMPL-02 probe outputs - PASS

## What This Proves

- Cursor adapter functionality is wired as a thin local adapter for the current XIIGen core API.
- The debug display shows exactly `xiigen <method> :\ninput: <redacted>\noutput: <redacted>`.
- The focused local adapter probe reaches `XiigenExtensionApiService` through `xiigen-extension-core-api.ps1`.

## What This Does Not Prove

- This does not prove full Cursor Extension Host/UI runtime behavior.
- This does not prove Cursor chat participant UI invocation.
- This does not prove semantic quality, token savings, learnability, training, checkpoint readiness, or target/rollback safety.

## Remaining Gaps Before RUNTIME-03

- Run a fresh Cursor Extension Host/UI proof packet from the current `C:\Projects\cursor-xiigen-smart-skills` repo.
- Use a fresh scenario id and correlation id.
- Capture command/chat/panel path actually used.
- Capture visible UI/chat answer, debug on/off display evidence, and negative/error path evidence.
- Parent review must verify handler-level/test-only evidence is not substituted for full Cursor runtime proof.

## Process Boundary

- `git_authority_seen`: `forbidden`
- `git_commands_run`: `[]`
- `git_write_or_ref_commands_run`: `[]`
- `git_directives_emitted`: `[]`
