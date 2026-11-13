# RUNTIME-04G Codex App-Server Adapter/Core Boundary Proof

Verdict: `PASS_APP_SERVER_CHAT_INSTALLED_SKILL_ADAPTER_CORE_BOUNDARY_PROVEN_WITH_NEGATIVE_PATH`.

## What This Proves

- `codex debug app-server send-message-v2` can start Codex app-server/chat turns that load the installed `xiigen-smart-skills` skill instructions.
- The app-server/chat turn ran the XIIGen Codex adapter and produced request/response/trace artifacts.
- Positive trace call target is `SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest` with `response_success: true`.
- Controlled negative missing-core-api path ran through a follow-up app-server turn and failed as expected with no response/trace.

## What This Does Not Prove

- Direct plugin CLI invocation remains unproven.
- Desktop Codex chat UI and desktop app UI automation remain unproven.
- Token savings, quality improvement, learnability, semantic quality, training, eval, checkpoint mutation, and full readiness remain unproven.

## Evidence

- Proof JSON: `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\codex-runtime-proof-04g.json`
- Codex summary JSON: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04G-codex\20260616-235353\runtime04g-app-server-adapter-core-boundary-summary.json`
- Positive app-server transcript: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04G-codex\20260616-235353\codex-debug-app-server-send-message-v2-runtime04g-attempt3.stdout.txt`
- Positive request/response/trace: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04G-codex\20260616-235353\positive\request.json`, `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04G-codex\20260616-235353\positive\response.json`, `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04G-codex\20260616-235353\positive\trace.json`
- Controlled negative transcript: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04G-codex\20260616-235353\codex-debug-app-server-send-message-v2-runtime04g-negative.stdout.txt`
- Controlled negative result: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04G-codex\20260616-235353\negative\case-result.json`
- Final verdict transcript: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04G-codex\20260616-235353\codex-debug-app-server-send-message-v2-runtime04g-final-verdict.stdout.txt`

## Notes

The first full positive app-server wrapper timed out after the positive artifacts were produced, so a separate read-only final-verdict app-server turn was captured. That final answer states the app-server/chat path executed the adapter/core boundary and keeps the listed readiness claims unproven.

## Exact Next Work Order

TOKEN-01-SCENARIO-PACK-AUTHOR: author the fixed paired scenario JSONL and expected quality rubric inputs for token baseline/adaptive comparison. Preserve RUNTIME-04G limitations: no direct plugin CLI claim, no desktop UI automation claim, no token savings, no quality improvement, no learnability, no semantic quality, no training, no eval, no checkpoint mutation, and no full readiness until separately proven.
