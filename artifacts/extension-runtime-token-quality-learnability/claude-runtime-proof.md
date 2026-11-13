# RUNTIME-02 Claude Runtime Proof

Classification: **PASS live Claude runtime** for the named Claude Code SessionStart hook smoke scenario only. Overall plan status remains `IN_PROGRESS_NEXT_STEP_KNOWN`.

## Command / Entrypoint

- Host: Claude Code `2.1.170 (Claude Code)`
- Entrypoint invoked: `C:\Users\lubao\AppData\Roaming\Claude\claude-code\2.1.170\claude.exe`
- Active hook source: `C:\Projects\claude-xiigen-smart-skills\reports\RUNTIME-02-claude\runtime-plugin-probe\.claude-plugin\plugin.json`
- Empty global hook manifest observed: `C:\Projects\claude-xiigen-smart-skills\hooks\hooks.json` contains an empty hooks object.
- Command record: `C:\Projects\claude-xiigen-smart-skills\reports\RUNTIME-02-claude\claude-cli\runtime02-positive-verbose-20260616-204952.command.json`
- Command shape: `claude.exe --plugin-dir <runtime-plugin-probe> --include-hook-events --output-format stream-json --verbose --debug hooks --debug-file <debug.log> --permission-mode dontAsk --tools "" --no-session-persistence --max-budget-usd 0.05 -p <redacted smoke prompt>`
- Timestamp: command `2026-06-16T17:49:52.1836371Z..2026-06-16T17:50:20.0756924Z`; core trace `2026-06-16T17:50:08.2562911Z..2026-06-16T17:50:16.9280685Z`.

The command exit code was `1` because Claude reported `error_max_budget_usd` after the visible answer. The stdout stream still contains the hook response and assistant visible answer.

## Scenario

- Scenario id: `RUNTIME-02-CLAUDE-SESSIONSTART-PROMPT-TO-TASK-SMOKE-20260616-204952`
- Correlation id: `claude-prompt-to-task-mqgxtdea-hzye9fhrpa`
- Method: `prompt-to-task`
- Raw private input ref: `C:\Projects\claude-xiigen-smart-skills\reports\RUNTIME-02-claude\hook-artifacts\xiigen-claude-core-request.json`
- Redacted input text/ref: `RUNTIME-02 redacted smoke prompt: verify XIIGen Claude hook path.` / `C:\Projects\claude-xiigen-smart-skills\reports\RUNTIME-02-claude\hook-artifacts\xiigen-core-trace.json`
- Raw output ref: `C:\Projects\claude-xiigen-smart-skills\reports\RUNTIME-02-claude\claude-cli\runtime02-positive-verbose-20260616-204952.stdout.jsonl`
- Redacted output text/ref: `Core xiigen boundary executed. Semantic quality is not claimed. xiigen prompt-to-task : input: <redacted> output: <redacted>` / `C:\Projects\claude-xiigen-smart-skills\reports\RUNTIME-02-claude\hook-artifacts\xiigen-claude-host-trace.json`

## Visible Answer

Visible answer: `RUNTIME_02_CLAUDE_VISIBLE_SMOKE`

Visible hook output: `Core xiigen boundary executed. Semantic quality is not claimed. xiigen prompt-to-task : input: <redacted> output: <redacted>`

Both are in `C:\Projects\claude-xiigen-smart-skills\reports\RUNTIME-02-claude\claude-cli\runtime02-positive-verbose-20260616-204952.stdout.jsonl`.

## Debug Trace

Debug trace: `xiigen prompt-to-task : input: <redacted> output: <redacted>`

Xiigen method/input/output refs:

- Method: `prompt-to-task`
- Input: `C:\Projects\claude-xiigen-smart-skills\reports\RUNTIME-02-claude\hook-artifacts\xiigen-response.json#/DebugTrace/InputPayloadText`
- Output: `C:\Projects\claude-xiigen-smart-skills\reports\RUNTIME-02-claude\hook-artifacts\xiigen-response.json#/DebugTrace/OutputPayloadText`

## XiigenExtensionApiService/Core Boundary

The proof shows `XiigenExtensionApiService/core boundary` handling:

- Core trace call target: `SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest`
- Core trace ref: `C:\Projects\claude-xiigen-smart-skills\reports\RUNTIME-02-claude\hook-artifacts\xiigen-core-trace.json`
- Core request ref: `C:\Projects\claude-xiigen-smart-skills\reports\RUNTIME-02-claude\hook-artifacts\xiigen-claude-core-request.json`
- Core response ref: `C:\Projects\claude-xiigen-smart-skills\reports\RUNTIME-02-claude\hook-artifacts\xiigen-response.json`
- Host trace ref: `C:\Projects\claude-xiigen-smart-skills\reports\RUNTIME-02-claude\hook-artifacts\xiigen-claude-host-trace.json`

The host trace records `host_semantic_ranking=false`, `host_model_logic=false`, `host_training=false`, `corpus_parsing=false`, and `external_ai_used=false` for the adapter/core path.

## Negative/Error Path

- Negative/error path: `PASS`
- Scenario id: `RUNTIME-02-CLAUDE-CONTROLLED-MISSING-CORE-ERROR-20260616-204952`
- Correlation id: `claude-prompt-to-task-mqgxu0x4-3mhvkxxtjp`
- Active hook source: `C:\Projects\claude-xiigen-smart-skills\reports\RUNTIME-02-claude\runtime-plugin-probe-error\.claude-plugin\plugin.json`
- Hook error output: `Xiigen adapter error: core API invocation failed.`
- Error status/code: `claude_adapter_core_endpoint_missing` / `core_api_endpoint_missing`
- Visible answer: `RUNTIME_02_CLAUDE_ERROR_SMOKE`
- Refs: `C:\Projects\claude-xiigen-smart-skills\reports\RUNTIME-02-claude\claude-cli\runtime02-error-verbose-20260616-204952.stdout.jsonl`, `C:\Projects\claude-xiigen-smart-skills\reports\RUNTIME-02-claude\error-artifacts\xiigen-response.json`

## What This Proves

- The named Claude Code runtime loaded a phase-scoped plugin via `--plugin-dir`.
- The plugin registered and ran a real `SessionStart` hook.
- The hook invoked the thin Claude adapter, which built a `XiigenExtensionRequest` and called `XiigenExtensionApiService`.
- The same fresh run produced a visible answer, hook output, debug trace, raw/redacted input/output refs, correlation id, freshness check, and negative/error path.

## What This Does Not Prove

- It does not prove Cursor/Codex runtime.
- It does not prove token savings, quality improvement, local learning, common model readiness, target promotion, rollback, or persistent/global Claude installation.
- Dashboard/API/helper/package smoke/test-only adapter/direct handler evidence remains supporting-only limitations.

## Next Independent Track

- RUNTIME-03 Cursor runtime QA can proceed independently.
- No RUNTIME-02 follow-up is required unless parent requires a clean zero-exit Claude CLI proof without the low `--max-budget-usd` guard.
