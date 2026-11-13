# DOCS-QA evidence-scope index for Claude, Cursor and Codex

Дата: 2026-06-21T17:05:10+03:00
Work order: DOCS-QA-EXTENSION-EVIDENCE-SCOPE-INDEX-NO-RUNTIME-NO-SCORING-01
Sub-agent: codex-bounded-sub-agent-docs-qa-extension-evidence-scope-index-no-runtime-no-scoring-01

## Короткий статус

Это безопасный docs-QA индекс, а не runtime proof и не scoring. В этой фазе не запускались Claude, Cursor или Codex hosts, не запускались proof runner, proof producer, eval runner или quality scorer, не было external AI/web/model calls, raw-private request/response/trace contents не читались, training/DPO/checkpoint/model/locator/target не менялись, code files не менялись, git commands не выполнялись.

Проектный статус сохранен: quality refs total 63, PASS 9, MISSING 45, DEFERRED 9, approvals 0/9, runner-ready 0/9, quality-pass 0/9. Эти PASS 9 остаются pointer-only/support evidence и не открывают runner-ready, quality-pass, token savings, quality improvement, learnability, product readiness или final readiness.

## Общая граница evidence

Сейчас самое сильное доказательство по hosts остается узким: named adapter, hook, Extension Host command или installed plugin cache surface reaches `llm_mvp_core` core boundary. Это полезно для product evidence map, но не равно full extension readiness.

Dashboard/API/docs/helper/static evidence доказывает только свой слой: schema, wording, support report, direct handler/API behavior, helper command или source-level boundary. Оно не доказывает live host runtime.

Direct core evidence доказывает, что `XiigenExtensionApiService.ProcessHostExtensionRequest` получил запрос и вернул response/error для названного сценария. Оно не доказывает host UI/chat/app journey.

Installed plugin surface evidence доказывает только installed/cache/plugin/extension surface, указанную в конкретном packet. Оно не доказывает, что пользовательский chat/app turn в полном host UI был пройден.

## Claude

### Что evidence уже показывает

- `runtime-02-claude-live-runtime` показал Claude Code 2.1.177, `--plugin-dir` validation/list/details, SessionStart hook inventory, positive hook path into `llm_mvp_core`, target locator binding, debug shape, and controlled missing TargetLocator error.
- `runtime-claude-live-evidence-20260619-035103` показал direct local Claude source entrypoint via `node.exe` for positive debug-off, positive debug-on and negative missing-locator. It did not run Claude Desktop, `claude.exe` prompt/chat path, or Claude model-facing command.
- `host-impl-claude-01` showed session-only install/load metadata and local hook wiring; it did not prove AppData install or full Claude chat UI.

### Что это доказывает

Это доказывает narrow Claude adapter/hook/core-boundary support evidence: request formation, target-locator path, debug-on/debug-off shape, controlled error path and core boundary reachability for named scenarios.

### Что это не доказывает

Full Claude Desktop/chat UI journey is not proven. Claude model task-solving is intentionally not invoked. Token savings, quality improvement, semantic quality, learnability, DPO/training lifecycle, target promotion, locator update, rollback, product readiness and final readiness remain closed.

### Следующий безопасный шаг

Если runtime remains unavailable under no-external-AI boundary, keep Claude full UI/chat proof deferred and prepare a non-runtime work-order bundle that names the exact allowed future proof-or-defer gate. Do not relabel hook/core-boundary evidence as full runtime.

## Cursor

### Что evidence уже показывает

- `host-impl-cursor-01` showed Cursor extension dev-load through VS Code Extension Host, command path, panel path, chat participant registration observation, handler-level chat adapter, debug pass-through, skills arrays and TargetLocator pass-through to `llm_mvp_core`.
- `runtime-03-cursor-live-runtime` showed Cursor Extension Host command/dev-load path with target locator and controlled missing-core error, but not full Cursor chat UI.
- `runtime-cursor-full-chat-ui-no-eval-proof` captured partial command debug off/on artifacts and partial handler-level chat artifacts, then classified the packet as blocked because Cursor emitted built-in backend/model metadata attempts.
- `cursor-safe-launch-preflight-or-defer` and `cursor-safe-launch-network-process-isolation-repair` prepared/deferred a safe-launch contract. Cursor was not relaunched there.

### Что это доказывает

Это доказывает narrow Cursor Extension Host command/helper evidence and a prepared no-overclaim isolation contract. It supports future safe proof planning but does not close full UI/chat.

### Что это не доказывает

Clean no-external-AI/model-call Cursor host proof is not established. Full Cursor chat participant UI, full panel UI journey, visual WebView proof and negative/error path in a clean live run remain missing. Token savings, quality improvement, semantic quality, learnability, target promotion, locator update, product readiness and final readiness remain closed.

### Следующий безопасный шаг

Do not launch Cursor until a bounded live-preflight work order can satisfy the network/process isolation contract. If the contract cannot pass, keep Cursor full UI proof deferred and continue safe non-runtime product/evidence work.

## Codex

### Что evidence уже показывает

- `host-impl-codex-01` showed installed cache entrypoint smoke, TargetLocator pass-through, RequestedSkills/AvailableSkills pass-through, core boundary execution and controlled missing TargetLocator error.
- `runtime-04-codex-live-runtime` showed installed Codex plugin cache Node entrypoint and matching repo/cache entrypoint hash, but did not use Codex CLI/app-server/chat send-message because direct `codex.exe` access was denied from PowerShell.
- `runtime-codex-live-evidence-20260619-050129` showed installed Codex plugin cache adapter scenarios positive debug-off, positive debug-on and negative missing TargetLocator. It did not invoke Codex desktop/app/chat/model surface.

### Что это доказывает

Это доказывает installed plugin cache adapter/core-boundary support evidence for Codex and installed skill/plugin surface loading in the narrow adapter sense.

### Что это не доказывает

Full Codex desktop/app/chat visible conversation is not proven. Installed plugin runtime invocation inside a real desktop chat turn is not proven. Codex external/model-backed task-solving is not invoked. Token savings, quality improvement, semantic quality, learnability, target promotion, locator update, rollback, product readiness and final readiness remain closed.

### Следующий безопасный шаг

If real Codex app/chat proof remains unavailable, keep Codex full UI/app proof deferred and use a non-runtime work-order bundle to define exact future proof-or-defer criteria.

## Cross-host full-extension requirements still open

All three hosts still need full-extension proof rows before any readiness claim can open:

- real host entrypoint in the target host UI/chat/app or explicit scoped defer;
- settings/debug surface in that host path;
- target-locator or core API path into `llm_mvp_core`;
- prompt-to-task path through the host;
- task-to-orchestration path through the host;
- task/plan-to-skills path through the host;
- local adaptive learning path and approval-safe DPO route;
- export/import/backup/share/continue-training lifecycle evidence;
- QA with live host evidence or explicitly scoped handler evidence.

The existing evidence can support planning and review, but it cannot substitute dashboard/API/helper/static/direct-core/plugin-cache evidence for full live host UI/chat/app proof.

## Следующее bounded work order

Selected follow-up: `DOCS-QA-FULL-EXTENSION-GAP-WORK-ORDER-BUNDLE-NO-RUNTIME-NO-SCORING-02`.

This should create a launch-ready, non-runtime work-order bundle from this matrix: one exact proof-or-defer work order per host and one cross-host token/quality/learnability/lifecycle/doc readiness map. It must not start hosts, run scoring, train, mutate checkpoints/locators/targets, read raw-private contents, or claim readiness.
