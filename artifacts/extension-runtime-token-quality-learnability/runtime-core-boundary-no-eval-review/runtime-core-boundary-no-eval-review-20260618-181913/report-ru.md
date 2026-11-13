# RUNTIME-CORE-BOUNDARY-NO-EVAL-REVIEW-SLICE

Run id: `runtime-core-boundary-no-eval-review-20260618-181913`
Branch: `codex/chat-gpt-5.5`
Scope: review artifact only. No eval, no training, no private-data use, no external AI, no source edits, no readiness claims.

## Доска

Всего: 4 блока обзора: core API boundary, три thin adapters, evidence classification, follow-up work orders.
Сделано: 4/4; прочитаны AGENTS.md, CLAUDE.md, active plan, state JSON, `XiigenExtensionApiService`, contracts/tests, host adapter sources and runtime evidence packets.
Сейчас: evidence-only review packet готовится для parent review; implementation authorization remains false.
Осталось: выполнить отдельные bounded runtime-proof/normalization work orders; QUALITY-02 local-private eval remains locked until exact approval exists.
Тип текущего результата: review artifact, not code, not runtime, not eval.
Принятые доказательства: static code map, handler-level tests, local/CLI helper evidence, Cursor Extension Host command evidence, Claude plugin hook evidence, Codex installed-cache/app-server plugin evidence.
Закрытые claims и почему: token savings, quality improvement, semantic quality, learnability, final readiness and product readiness remain closed because no eval, no paired token/quality run, no training, no full final readiness gate and no exact QUALITY-02 approval were run in this slice.
Открытые разрывы: full Claude chat UI, full Cursor chat participant/UI, full Codex desktop chat UI, normalized evidence classification, and final cross-host readiness proof.
Следующее действие: `RUNTIME-EVIDENCE-CLASSIFICATION-NORMALIZATION-01`, then host-specific no-eval runtime proof work orders listed below.
Execution paused for eval/training/readiness: yes. Safe no-eval runtime/evidence work can continue by bounded work order.

## Что закрыто

- The central request/response boundary exists in `43.SmartSkillsDomain/XiigenExtensionApiContracts.cs`: `XiigenExtensionRequest`, `XiigenTargetLocator`, `XiigenExtensionResponse`, and `XiigenDebugTrace`.
- `XiigenExtensionApiService.ProcessHostExtensionRequest` is the core entrypoint used by thin adapters and the helper script.
- Core-owned logic calls `PromptToTaskMachine.Translate`, `OrchestrationReasoningMachine.Plan`, and `SkillSelectionMachine.DecideFromTrainedState` inside `llm_mvp_core`.
- Claude, Cursor, and Codex adapter sources are thin request/response shims in the inspected code: they build or forward `XiigenExtensionRequest`, call `tools/xiigen-extension-core-api.ps1`, and display response/debug output.
- Existing evidence honestly keeps `runtime_extension_claimed=false`, `semantic_quality_claimed=false`, and broader product claims closed.
- QSG-21 is useful target-bound host-surface evidence, but it still labels full live UI/chat readiness and final readiness as not proven.

## Что НЕ закрыто

- Full Claude chat UI / Claude Desktop visible conversation proof is not closed. The best current evidence is Claude Code plugin hook / SessionStart command path.
- Full Cursor chat participant UI proof is not closed. The best current evidence is Cursor Extension Host command path; safe noninteractive chat participant invocation was not found in RUNTIME-03.
- Full Codex desktop UI/chat proof is not closed. The best current evidence is installed-cache/plugin-entrypoint and QSG-21 app-server/plugin path; desktop visible conversation remains missing.
- Static maps, handler tests, CLI helpers, dashboard/API traces, component QA, plugin hook evidence, and app-server plugin evidence do not prove final product readiness by themselves.
- Token savings, quality improvement, semantic quality, learnability, DPO, training, checkpoint/locator production mutation, rollback promotion, final readiness, and product readiness are not proven by this review.

## Runtime/Core Boundary Map

| Host | Where request enters | `llm_mvp_core` API used | Debug input/output surface | Evidence that exists | Honest scope |
|---|---|---|---|---|---|
| Claude | `C:\Projects\claude-xiigen-smart-skills\src\claude-entrypoint.js`, launched by `src\claude-entrypoint.ps1` or Claude Code plugin hook | `tools/xiigen-extension-core-api.ps1` calls `SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest` | Adapter strips core raw debug display, then shows `xiigen <method> : input: <redacted> output: <redacted>` only when debug is enabled and safe | HOST-IMPL-01 source adapter report; RUNTIME-02 Claude Code plugin validate/list/details plus positive/negative SessionStart hook; QSG-21 `LIVE_PLUGIN_HOOK` | Real Claude Code plugin hook/core-boundary evidence; not full Claude chat UI readiness |
| Cursor | `C:\Projects\cursor-xiigen-smart-skills\src\extension.ts` registers `xiigen.smartSkills.select`, panel, and chat participant; `scripts/local-runtime-helper.js` is helper-only | `invokeCoreApi` spawns `tools/xiigen-extension-core-api.ps1`, which calls `XiigenExtensionApiService` | `CursorDisplayText` appends exact redacted debug shape when `xiigenSmartSkills.debug=true`; local helper has helper-only debug output | HOST-IMPL-CURSOR-01 adapter report; RUNTIME-03 Cursor Extension Host command path; QSG-21 `EXTENSION_HOST` | Cursor Extension Host command/core-boundary evidence; not full chat participant UI readiness |
| Codex | `C:\Projects\codex-xiigen-smart-skills\src\codex-entrypoint.js`; `src\codex-entrypoint.ps1` is launcher; installed cache entrypoint was used in RUNTIME-04 | `invokeXiigenCore` spawns `tools/xiigen-extension-core-api.ps1`, which calls `XiigenExtensionApiService` | `createVisibleAnswerDisplay` shows core-visible text plus `xiigen <method> : input/output <redacted>` when debug is enabled | HOST-IMPL-03 component/source-entrypoint QA; RUNTIME-04 installed-cache/plugin-entrypoint; QSG-21 `CODEX_APP_SERVER_PLUGIN` | Installed-cache/app-server/plugin evidence; not full Codex desktop UI/chat readiness |

## Evidence Gap Matrix

| Evidence type | Current examples | What it can prove | What it cannot prove |
|---|---|---|---|
| Static code map | Contracts, `43.SmartSkillsDomain/README.md`, adapter source inspection | Boundary shape and absence of obvious host-owned core logic in inspected source | Runtime behavior, installation, live UI/chat, quality, learning |
| Handler-level test | `Tests/XiigenExtensionApiContractsTests.cs` | Contract validation, debug redaction, semantic/runtime claim locks, direct service behavior | Host adapter load, app/plugin UI, real host invocation |
| CLI helper / local adapter | `tools/xiigen-extension-core-api.ps1`, Cursor local helper, Codex component QA | A helper can format a request and reach the core boundary | Live host UI/chat/app readiness |
| Dashboard/API/core trace | token/QSG core traces, handler traces | Core request/response path and target locator behavior for supplied input | Full extension readiness or live host user journey |
| Live host surface proof, narrow | RUNTIME-02 Claude plugin hook, RUNTIME-03 Cursor Extension Host command, RUNTIME-04 Codex installed-cache entrypoint, QSG-21 host probes | Named host surface invoked core and produced visible/redacted output for that scenario | Final readiness, token savings, quality, learnability, full UI/chat where explicitly missing |
| Full live UI/chat/app proof | Not fully present for Claude/Cursor/Codex in the inspected evidence | Would prove the named host-visible user journey only if same-run request/response/correlation/freshness/error evidence is present | Still would not prove token savings, quality improvement, learning, or final readiness without their gates |

## Boundary Risks

- Some older artifacts use labels such as `runtimeEvidence=true` while their own `evidenceScope` says `core_api_check_set_runtime_not_full_host`. This is a classification risk: reports must normalize those booleans into evidence classes before any readiness review.
- Claude has strong hook evidence, but full chat UI proof remains missing. A hook path should not be used as a full Claude conversation claim.
- Cursor has Extension Host command evidence, but chat participant invocation and panel evidence are incomplete in the freshest runtime packet.
- Codex has installed-cache/app-server/plugin evidence, but desktop UI/chat send-message visible conversation remains missing.
- Core contains baseline/internal and KNN/counter/scaffold mechanisms. Current code mostly labels them as `semantic_quality_claimed=false`, `BASELINE_NOT_SEMANTIC_QUALITY`, or diagnostics/support evidence only; future reports must keep that wording and not promote those mechanisms to final trainable authority.
- Adapter sources contain controlled error display and debug display logic. That is allowed host presentation logic, but it must not become semantic selection, routing, model logic, DPO, training, or target promotion logic.
- Debug traces are good but not sufficient everywhere: reports need same-run request, response, host trace, core trace, correlation id, visible output, negative/error path, and evidence class.

## Follow-Up Bounded Work Orders

### 1. RUNTIME-EVIDENCE-CLASSIFICATION-NORMALIZATION-01

Artifact type: review/report plus optional report-normalization patch only if separately authorized.
Allowed write scope: new timestamped artifact folder under `artifacts/extension-runtime-token-quality-learnability/runtime-evidence-classification-normalization/` and state JSON; no source edits unless a separate implementation work order authorizes them.
Task: classify every existing Claude/Cursor/Codex runtime artifact into static code map, handler-level test, CLI/helper, dashboard/API/core, narrow live host surface, or full live UI/chat/app proof. Flag every `runtimeEvidence=true` row whose scope is narrower than full host runtime.
Verification: JSON parse, evidence-row count, no source code changes, all readiness claims remain false.
Next after PASS: run host-specific proof work orders below.

### 2. RUNTIME-CLAUDE-FULL-CHAT-UI-NO-EVAL-PROOF-01

Artifact type: runtime proof packet.
Allowed write scope: new timestamped artifact folder under `artifacts/extension-runtime-token-quality-learnability/runtime-claude-full-chat-ui-no-eval-proof/` and state JSON.
Task: attempt the safest available non-eval, non-training, non-private-data, no-external-AI Claude visible host path. If full chat UI necessarily invokes external Claude model task-solving, do not run it; record `FULL_CHAT_UI_REMAINS_BLOCKED_BY_EXTERNAL_AI_BOUNDARY` and preserve RUNTIME-02 hook evidence as narrow proof only.
Required evidence: Claude executable/plugin version, plugin load state, same-run request/response/core trace/host trace, visible output, debug off/on if safe, negative/error path, correlation id, freshness, `external_ai_used=false`, `runtime_extension_claimed=false`, `semantic_quality_claimed=false`.
Verification: JSON parse, scope guard, no eval/training/source changes, no readiness claims.

### 3. RUNTIME-CURSOR-CHAT-PARTICIPANT-PANEL-NO-EVAL-PROOF-01

Artifact type: runtime proof packet.
Allowed write scope: new timestamped artifact folder under `artifacts/extension-runtime-token-quality-learnability/runtime-cursor-chat-participant-panel-no-eval-proof/` and state JSON.
Task: run available Cursor Extension Host command, panel, and chat participant surfaces without external AI task-solving. If chat participant cannot be safely invoked noninteractively, record the API gap and keep RUNTIME-03 command evidence narrow.
Required evidence: Extension Host load/activation, command/panel/chat rows separately classified, same-run request/response/core trace/host trace, visible markdown or panel output where available, debug off/on, negative/error path, correlation id, freshness, `external_ai_task_solving=false`.
Verification: JSON parse, no source changes unless separately authorized, no eval/training, no readiness claims.

### 4. RUNTIME-CODEX-DESKTOP-OR-APP-SERVER-UI-NO-EVAL-PROOF-01

Artifact type: runtime proof packet.
Allowed write scope: new timestamped artifact folder under `artifacts/extension-runtime-token-quality-learnability/runtime-codex-desktop-or-app-server-ui-no-eval-proof/` and state JSON.
Task: use the installed Codex plugin/cache/app-server path if available and classify it separately from desktop UI. Attempt desktop/UI proof only if a safe no-external-AI plugin invocation path exists; otherwise record `DESKTOP_CHAT_UI_REMAINS_MISSING_SAFE_INVOKE_PATH`.
Required evidence: installed plugin version/cache sha, entrypoint request/response/core trace, app-server or desktop surface label, visible output, debug off/on, negative/error path, correlation id, freshness, `external_ai_used=false`, no model-backed task-solving.
Verification: JSON parse, no source changes unless separately authorized, no eval/training, no readiness claims.

## What This Proves

- The inspected code and artifacts support a narrow conclusion: the core boundary and thin-adapter request/response pattern exist and are mostly wired through `llm_mvp_core`.
- The current no-eval review can advance extension readiness by identifying exact runtime proof gaps without touching private eval, training, source code, checkpoints, locators, manifests, or external AI.

## What This Does Not Prove

- It does not prove full extension readiness for Claude, Cursor, or Codex.
- It does not prove token savings, quality improvement, semantic quality, learnability, DPO, training, target promotion, production locator update, rollback readiness, final readiness, or product readiness.

## Files Inspected

- `AGENTS.md`
- `CLAUDE.md`
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md`
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`
- `43.SmartSkillsDomain/XiigenExtensionApiService.cs`
- `43.SmartSkillsDomain/XiigenExtensionApiContracts.cs`
- `43.SmartSkillsDomain/PromptToTaskMachine.cs`
- `43.SmartSkillsDomain/OrchestrationReasoningMachine.cs`
- `43.SmartSkillsDomain/SkillSelectionMachine.cs`
- `43.SmartSkillsDomain/SkillSelectionLogicalHierarchy.cs`
- `Tests/XiigenExtensionApiContractsTests.cs`
- `tools/xiigen-extension-core-api.ps1`
- `C:\Projects\claude-xiigen-smart-skills\src\claude-entrypoint.js`
- `C:\Projects\cursor-xiigen-smart-skills\src\extension.ts`
- `C:\Projects\cursor-xiigen-smart-skills\scripts\local-runtime-helper.js`
- `C:\Projects\codex-xiigen-smart-skills\src\codex-entrypoint.js`
- RUNTIME-01, RUNTIME-02, RUNTIME-03, RUNTIME-04, QSG-20 and QSG-21 evidence folders under `artifacts/extension-runtime-token-quality-learnability`.
