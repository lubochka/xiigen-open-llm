# RUNTIME-02 Claude Runtime QA Report

Generated: 2026-06-16T11:56:56.4629669+03:00
Phase: RUNTIME-02
Host: Claude
Verdict: PASS_RUNTIME_PROOF_CANDIDATE

## Runtime Path

- Claude executable: C:\Users\lubao\AppData\Roaming\Claude\claude-code\2.1.170\claude.exe
- Runtime mode: Claude Code session-only --plugin-dir inline plugin
- Hook event: SessionStart
- Adapter entrypoint: C:\Projects\claude-xiigen-smart-skills\src\claude-entrypoint.js
- Core endpoint: C:\Projects\llm_mvp_core\tools\xiigen-extension-core-api.ps1

## Proof Booleans

| Boolean | Value |
|---|---:|
| realClaudeRuntimeInvoked | True |
| freshHookArtifacts | True |
| userPromptReachedCore | True |
| visibleClaudeAnswerObserved | True |
| debugTraceObserved | True |
| controlledErrorPathObserved | True |
| runtimeProofAcceptedCandidate | True |

## Correlation IDs

- Positive path: claude-prompt-to-task-mqgem4k7-9rtev0ycte
- Controlled error path: claude-prompt-to-task-mqgeohr8-612lm9xj1l

## Evidence

Positive path:
- Stream: C:\Projects\claude-xiigen-smart-skills\reports\RUNTIME-02-claude\claude-cli\claude-stream-attempt2.jsonl
- Debug log: C:\Projects\claude-xiigen-smart-skills\reports\RUNTIME-02-claude\claude-cli\claude-debug-attempt2.log
- Core request: C:\Projects\claude-xiigen-smart-skills\reports\RUNTIME-02-claude\hook-artifacts\xiigen-claude-core-request.json
- Host trace: C:\Projects\claude-xiigen-smart-skills\reports\RUNTIME-02-claude\hook-artifacts\xiigen-claude-host-trace.json
- Core trace: C:\Projects\claude-xiigen-smart-skills\reports\RUNTIME-02-claude\hook-artifacts\xiigen-core-trace.json
- Response: C:\Projects\claude-xiigen-smart-skills\reports\RUNTIME-02-claude\hook-artifacts\xiigen-response.json

Controlled error path:
- Stream: C:\Projects\claude-xiigen-smart-skills\reports\RUNTIME-02-claude\claude-cli\claude-stream-error.jsonl
- Debug log: C:\Projects\claude-xiigen-smart-skills\reports\RUNTIME-02-claude\claude-cli\claude-debug-error.log
- Core request: C:\Projects\claude-xiigen-smart-skills\reports\RUNTIME-02-claude\error-artifacts\xiigen-claude-core-request.json
- Host trace: C:\Projects\claude-xiigen-smart-skills\reports\RUNTIME-02-claude\error-artifacts\xiigen-claude-host-trace.json
- Response: C:\Projects\claude-xiigen-smart-skills\reports\RUNTIME-02-claude\error-artifacts\xiigen-response.json

## Commands Run

1. Get-Content -LiteralPath C:\Projects\llm_mvp_core\.git\HEAD -Raw
2. C:\Users\lubao\AppData\Roaming\Claude\claude-code\2.1.170\claude.exe --help
3. C:\Users\lubao\AppData\Roaming\Claude\claude-code\2.1.170\claude.exe plugin validate C:\Projects\claude-xiigen-smart-skills\reports\RUNTIME-02-claude\runtime-plugin-probe
4. C:\Users\lubao\AppData\Roaming\Claude\claude-code\2.1.170\claude.exe --plugin-dir C:\Projects\claude-xiigen-smart-skills\reports\RUNTIME-02-claude\runtime-plugin-probe --include-hook-events --output-format stream-json --debug hooks --debug-file C:\Projects\claude-xiigen-smart-skills\reports\RUNTIME-02-claude\claude-cli\claude-debug.log --permission-mode dontAsk --tools  --no-session-persistence --max-budget-usd 0.05 -p RUNTIME-02 redacted Claude visible-output smoke. Reply with exactly XIIGEN_RUNTIME_02_VISIBLE.
5. C:\Users\lubao\AppData\Roaming\Claude\claude-code\2.1.170\claude.exe --plugin-dir C:\Projects\claude-xiigen-smart-skills\reports\RUNTIME-02-claude\runtime-plugin-probe --include-hook-events --output-format stream-json --verbose --debug hooks --debug-file C:\Projects\claude-xiigen-smart-skills\reports\RUNTIME-02-claude\claude-cli\claude-debug-attempt2.log --permission-mode dontAsk --tools  --no-session-persistence --max-budget-usd 0.05 -p RUNTIME-02 redacted Claude visible-output smoke. Reply with exactly XIIGEN_RUNTIME_02_VISIBLE.
6. C:\Users\lubao\AppData\Roaming\Claude\claude-code\2.1.170\claude.exe plugin validate C:\Projects\claude-xiigen-smart-skills\reports\RUNTIME-02-claude\runtime-plugin-probe-error
7. C:\Users\lubao\AppData\Roaming\Claude\claude-code\2.1.170\claude.exe --plugin-dir C:\Projects\claude-xiigen-smart-skills\reports\RUNTIME-02-claude\runtime-plugin-probe-error --include-hook-events --output-format stream-json --verbose --debug hooks --debug-file C:\Projects\claude-xiigen-smart-skills\reports\RUNTIME-02-claude\claude-cli\claude-debug-error.log --permission-mode dontAsk --tools  --no-session-persistence --max-budget-usd 0.05 -p RUNTIME-02 redacted Claude controlled-error visible smoke. Reply with exactly XIIGEN_RUNTIME_02_ERROR_VISIBLE.

## What This Proves

- A real Claude Code 2.1.170 process loaded the phase-scoped XIIGen plugin via --plugin-dir and invoked a SessionStart command hook.
- The hook invoked the existing thin Claude adapter, which sent a fresh redacted prompt to XiigenExtensionApiService and produced a matching response/debug trace with correlation id claude-prompt-to-task-mqgem4k7-9rtev0ycte.
- A controlled missing-core error path was represented through the same Claude hook mechanism with correlation id claude-prompt-to-task-mqgeohr8-612lm9xj1l.

## What This Does Not Prove

- No token savings or quality improvement is claimed.
- No semantic model quality, DPO, training, checkpoint mutation, fresh-load, held-out, ablation, rollback, Cursor runtime, Codex runtime, or full extension readiness is claimed.
- Static plugin validation and the initial malformed stream-json attempt are supporting evidence only, not runtime proof by themselves.
- This proves the named Claude Code session-only --plugin-dir hook path, not persistent marketplace installation or global Claude user settings installation.

## Evidence Separation

This proves the named host runtime path for this scenario only. It does not prove model quality or learning.

Plugin validation is static evidence only. It does not prove runtime behavior by itself.

No dashboard/API/helper/package smoke evidence is used as runtime proof in this report.

## Limitations

- The positive Claude process exited 1 after emitting the visible answer because --max-budget-usd 0.05 was exceeded; hook events, the visible answer, and fresh hook artifacts were captured before that budget result.
- Claude startup attempted configured claude.ai MCP connectors; auth/certificate warnings in debug logs are unrelated to the XIIGen hook proof.

## Next Independent Track

RUNTIME-03 Cursor runtime QA can continue independently after parent review.
