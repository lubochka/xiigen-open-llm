# Codex Adapter Functionality Report - HOST-IMPL-03

Date: 2026-06-16.

Verdict: `PASS_ADAPTER_FUNCTIONALITY_ONLY_AFTER_REPAIR`.

This report covers Codex thin adapter functionality only. It does not claim full Codex plugin/app runtime proof, model quality, token improvement, training, checkpoint mutation, or end-to-end extension completion.

## Inspected Files

- `C:\Projects\llm_mvp_core\docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md`
- `C:\Projects\llm_mvp_core\docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`
- `C:\Projects\codex-xiigen-smart-skills\.codex-plugin\plugin.json`
- `C:\Projects\codex-xiigen-smart-skills\package.json`
- `C:\Projects\codex-xiigen-smart-skills\src\codex-entrypoint.js`
- `C:\Projects\codex-xiigen-smart-skills\src\codex-entrypoint.ps1`
- `C:\Projects\codex-xiigen-smart-skills\scripts\install-smoke.ps1`
- `C:\Projects\codex-xiigen-smart-skills\test\host-qa\HOST-08F-codexComponentQA.js`
- `C:\Projects\codex-xiigen-smart-skills\xiigen-extension-contract.json`
- `C:\Projects\codex-xiigen-smart-skills\README.md`
- `C:\Projects\codex-xiigen-smart-skills\docs\PROJECT-DOC.md`
- `C:\Projects\codex-xiigen-smart-skills\prompts\codex-smoke-prompts.jsonl`

## Changed Files

- `C:\Projects\codex-xiigen-smart-skills\.codex-plugin\plugin.json`
- `C:\Projects\codex-xiigen-smart-skills\package.json`
- `C:\Projects\codex-xiigen-smart-skills\README.md`
- `C:\Projects\codex-xiigen-smart-skills\docs\PROJECT-DOC.md`
- `C:\Projects\codex-xiigen-smart-skills\scripts\install-smoke.ps1`
- `C:\Projects\codex-xiigen-smart-skills\src\codex-entrypoint.js`
- `C:\Projects\codex-xiigen-smart-skills\test\host-qa\HOST-08F-codexComponentQA.js`
- `C:\Projects\codex-xiigen-smart-skills\xiigen-extension-contract.json`
- `C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-03-probe\install-smoke\install-smoke-report.json`
- `C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-03-probe\HOST-IMPL-03-codex-component-qa.json`
- `C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-03-probe\component\...`
- `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\codex-adapter-functionality-report.md`
- `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\codex-adapter-functionality-report.json`
- `C:\Projects\llm_mvp_core\docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`

## Entrypoint And Manifest

- Plugin manifest: `C:\Projects\codex-xiigen-smart-skills\.codex-plugin\plugin.json`
- Package manifest: `C:\Projects\codex-xiigen-smart-skills\package.json`
- Source entrypoint: `C:\Projects\codex-xiigen-smart-skills\src\codex-entrypoint.js`
- PowerShell launcher: `C:\Projects\codex-xiigen-smart-skills\src\codex-entrypoint.ps1`
- Declared user path: `xiigen.smartSkills.select`
- Core endpoint: `C:\Projects\llm_mvp_core\tools\xiigen-extension-core-api.ps1`
- Core boundary: `XiigenExtensionApiService.ProcessHostExtensionRequest`

## Request/Response Evidence

The focused component/source-entrypoint probe built `XiigenExtensionRequest` objects for Codex and invoked the core API script. The positive probe covered `prompt-to-task`, `orchestration-plan`, and `skill-selection`, with debug off and debug on. Every positive case returned `exit_code=0`, `response_success=true`, and `semantic_quality_claimed=false`.

All runtime proof fields are false for this phase:

- `runtimeEvidence=false`
- `trueHostRuntimeEvidence=false`
- `true_host_runtime_evidence=false`
- `coreApiRuntimeEvidence=false`
- `fullCodexPluginAppRuntimeProof=false`

Evidence refs:

- Component report: `C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-03-probe\HOST-IMPL-03-codex-component-qa.json`
- Debug off request: `C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-03-probe\component\prompt-to-task-debug-off\xiigen-codex-core-request.json`
- Debug off response: `C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-03-probe\component\prompt-to-task-debug-off\xiigen-codex-core-response.json`
- Debug on request: `C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-03-probe\component\prompt-to-task-debug-on\xiigen-codex-core-request.json`
- Debug on response: `C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-03-probe\component\prompt-to-task-debug-on\xiigen-codex-core-response.json`
- Debug on visible answer: `C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-03-probe\component\prompt-to-task-debug-on\visible-answer.txt`
- Missing-core error case: `C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-03-probe\component\prompt-to-task-missing-core-error\case-result.json`
- Missing-core visible error: `C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-03-probe\component\prompt-to-task-missing-core-error\visible-answer.txt`

## Debug And Privacy Evidence

Debug-on evidence contains exactly the required visible display shape:

```text
xiigen prompt-to-task :
input: <redacted>
output: <redacted>
```

The raw sentinel `HOST08F_CODEX_QA_SENTINEL_NO_VISIBLE_TRACE` appears only in request payload files, where `UserPromptOrTaskText` belongs by contract. It was absent from visible debug display, visible answer text, response, and trace outputs.

Repair note: the adapter no longer renders JSON after `input:` or `output:` in visible debug text. Detailed request/response refs remain in JSON artifacts.

## Error State Evidence

The Codex adapter source now includes `createVisibleAnswerDisplay`, which returns response-shaped visible text when the core succeeds and an honest `XIIGen Codex adapter error (...)` message when the core response is missing or failed. This keeps error display in the adapter while semantic decisions remain in `llm_mvp_core`.

The negative/error-state component case deliberately pointed the source entrypoint at a nonexistent core API script:

```text
C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-03-probe\component\missing-core-api-script.ps1
```

That case passed with `response_success=false`, `controlled_error_state=true`, and visible answer text beginning:

```text
XIIGen Codex adapter error (core_api_failed).
```

The same error-state visible answer retained the exact debug text and did not leak the raw prompt sentinel outside the request payload.

## Boundary Review

PASS for HOST-IMPL-03 adapter functionality:

- Codex builds `XiigenExtensionRequest`.
- Codex calls the core API script and receives `XiigenExtensionResponse`.
- Codex can produce visible answer text and exact redacted debug text.
- Codex can produce a controlled visible adapter error when the core API script is missing.
- Prompt-to-task, orchestration, skill selection, model logic, DPO, training, and checkpoint mutation remain outside the adapter.
- No external AI or model-backed `codex exec` was used.
- No full Codex plugin/app runtime proof was claimed.

## Probes Run

- `node --check "C:\Projects\codex-xiigen-smart-skills\src\codex-entrypoint.js"` - PASS
- `node --check "C:\Projects\codex-xiigen-smart-skills\test\host-qa\HOST-08F-codexComponentQA.js"` - PASS
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File "C:\Projects\codex-xiigen-smart-skills\scripts\install-smoke.ps1" -Output "reports\HOST-IMPL-03-probe\install-smoke\install-smoke-report.json"` - PASS
- `npm run install-smoke` - PASS
- `node "C:\Projects\codex-xiigen-smart-skills\test\host-qa\HOST-08F-codexComponentQA.js" --output-root "C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-03-probe\component" --host-report "C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-03-probe\HOST-IMPL-03-codex-component-qa.json" --core-root "C:\Projects\llm_mvp_core"` - PASS, including positive source-entrypoint cases and the missing-core error-state case
- PowerShell strict JSON/debug/redaction/runtime-overclaim validation over `C:\Projects\codex-xiigen-smart-skills\reports\HOST-IMPL-03-probe` - PASS

Non-evidence helper failures: three draft validation helper invocations failed due PowerShell quoting or file-list construction while I was building the final validator. They did not change files and were not used as evidence.

## JSON And Runtime Claim Validation

- 21 generated JSON files parsed strictly.
- Adapter-owned JSON outputs had no BOM after the smoke writer repair.
- Four core-generated trace files still contain BOM because they are written by `llm_mvp_core`; this phase did not edit core.
- Exact debug display: PASS.
- Visible answer contains exact debug display when debug is enabled: PASS.
- Controlled error-state visible answer for missing core script: PASS.
- Sentinel leak outside request payload: PASS, no leak files.
- Runtime overclaim fields false: PASS.

## What This Proves

- Codex adapter functionality is wired as a thin local adapter for the current XIIGen core API.
- The debug display shows exactly `xiigen <method> :\ninput: <redacted>\noutput: <redacted>`.
- The focused component/source-entrypoint probe reaches `XiigenExtensionApiService` through `xiigen-extension-core-api.ps1`.
- The focused negative case proves adapter-level error display for a missing core API script without claiming host runtime proof.

## What This Does Not Prove

- This does not prove full Codex plugin/app runtime behavior.
- This does not prove Codex app/plugin loading or real user chat invocation.
- This does not prove semantic quality, token savings, learnability, training, checkpoint readiness, or target/rollback safety.

## Next Plan Order And Remaining Gaps

- The active plan order after HOST-IMPL-03 is parent review, then `RUNTIME-02` Claude runtime proof, then `RUNTIME-03` Cursor runtime proof, then `RUNTIME-04` Codex runtime proof.
- Codex-specific runtime proof remains `RUNTIME-04`; it must run later from the current `C:\Projects\codex-xiigen-smart-skills` repo if a real safe runtime path is approved and available.
- `RUNTIME-04` still needs a fresh scenario id, correlation id, actual plugin/app path, visible Codex answer, debug on/off display evidence, and negative/error path evidence from the real Codex plugin/app runtime.
- Parent review must verify handler/source-entrypoint evidence is not substituted for full Codex runtime proof.

## Process Boundary

- `git_authority_seen`: `forbidden`
- `git_commands_run`: `[]`
- `git_write_or_ref_commands_run`: `[]`
- `git_directives_emitted`: `[]`
