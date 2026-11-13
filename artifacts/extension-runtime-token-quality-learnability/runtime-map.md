# RUNTIME-01 — карта реальных runtime-путей Claude / Cursor / Codex

Дата проверки: 2026-06-16T10:10:25+03:00.

Роль этого файла: это не запуск host runtime и не доказательство, что extensions уже полностью работают. Это карта того, что сейчас найдено на диске, какие entrypoint-и выглядят реальными, какие старые артефакты можно использовать только как reference, и какие поля ещё надо закрыть перед настоящим runtime proof.

## Короткий вердикт

`RUNTIME-01` можно считать выполненным как reality map: source paths, installed/runtime candidates и главные gaps найдены. Но итоговый verdict для runtime readiness: `PASS_WITH_REPAIR_NEEDED`.

Почему не `PASS`: ни один из трёх host-ов в этом slice не запускался, а старые evidence-файлы по плану не могут закрыть текущий runtime proof. Для следующей фазы нужен отдельный host runtime probe packet.

## Общая граница архитектуры

Во всех трёх adapters найден правильный базовый boundary: host adapter должен быть тонким клиентом, а логика остаётся в `llm_mvp_core`.

Core boundary:

```text
C:\Projects\llm_mvp_core\tools\xiigen-extension-core-api.ps1
  -> SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest

C:\Projects\llm_mvp_core\43.SmartSkillsDomain\XiigenExtensionApiService.cs
  -> PromptToTaskMachine
  -> OrchestrationReasoningMachine
  -> SkillSelectionMachine
```

Что это значит человеческим языком: extension не должен сам решать, какую задачу понял пользователь, какой план оркестрации нужен, или какие скилы выбрать. Он должен собрать `XiigenExtensionRequest`, отправить его в core API, показать ответ и debug trace.

## Claude

### Что найдено

Source repo:

```text
C:\Projects\claude-xiigen-smart-skills
```

Source entrypoints:

```text
C:\Projects\claude-xiigen-smart-skills\src\claude-entrypoint.js
C:\Projects\claude-xiigen-smart-skills\src\claude-entrypoint.ps1
```

Plugin manifest:

```text
C:\Projects\claude-xiigen-smart-skills\.claude-plugin\plugin.json
```

Manifest hook points to the JS entrypoint:

```text
node.exe "C:\Projects\claude-xiigen-smart-skills\src\claude-entrypoint.js" ...
```

Current Claude executable candidate:

```text
C:\Users\lubao\AppData\Roaming\Claude\claude-code\2.1.170\claude.exe
```

Installed plugin data candidate:

```text
C:\Users\lubao\.claude\plugins\data\claude-xiigen-smart-skills-inline
```

Important stale-path finding: old `host-interactive-runtime-claude.json` referenced:

```text
C:\Users\lubao\AppData\Roaming\Claude\claude-code\2.1.160\claude.exe
```

That path is no longer present in this inspection. The current candidate is `2.1.170`.

### What existing artifacts prove and do not prove

Useful reference:

```text
C:\Projects\claude-xiigen-smart-skills\reports\CL-04-smoke-trace-report.json
```

That old report says Claude once loaded the plugin through `--plugin-dir`, emitted hook events, and produced core request/trace artifacts. It is useful as a reference for the next probe shape.

It does not close current `RUNTIME-01` runtime proof because this slice did not launch Claude, the executable version has changed, and the plan requires fresh evidence for the current phase.

Older core/entrypoint artifacts in the repo also do not count as final proof because they are files from previous runs, not fresh artifacts from this phase.

### Boundary risks

- Real host runtime proof path is known in principle: run Claude with `--plugin-dir C:\Projects\claude-xiigen-smart-skills` and capture fresh hook/core artifacts.
- Exact current command must be rebuilt with `2.1.170`; old `2.1.160` command is stale.
- `hooks\hooks.json` is empty, while `.claude-plugin\plugin.json` has the real SessionStart hook. Next probe must use the plugin manifest, not `hooks\hooks.json`.
- Debug path exists in source (`DebugEnabled` and redacted debug shape), but current host chat display proof is not fresh.
- No model/training logic was found in the Claude adapter; source states host model logic and host training are false.

### Missing fields before Claude runtime PASS

- Fresh command line using the current Claude executable.
- Fresh run timestamp.
- Fresh correlation id.
- Fresh hook response, host trace, core request, core response, core trace.
- Evidence that debug-off and debug-on display the expected safe shape.
- Parent review that the artifact is current and not reused from old runs.

## Cursor

### Что найдено

Source repo:

```text
C:\Projects\cursor-xiigen-smart-skills
```

Source and built extension entrypoints:

```text
C:\Projects\cursor-xiigen-smart-skills\src\extension.ts
C:\Projects\cursor-xiigen-smart-skills\dist\extension.js
```

Package manifest:

```text
C:\Projects\cursor-xiigen-smart-skills\package.json
```

Runtime binary candidate:

```text
C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe
```

User installed extension location checked:

```text
C:\Users\lubao\.cursor\extensions
```

That folder currently contains only `extensions.json`; no installed `xiigen` extension directory was found there.

### What the Cursor adapter actually exposes

The package declares:

```text
main: ./dist/extension.js
activationEvents:
  onCommand:xiigen.smartSkills.select
  onCommand:xiigenSmartSkills.panel
  onChatParticipant:xiigen-smart-skills
```

The source registers:

```text
xiigen.smartSkills.select
xiigenSmartSkills.panel
xiigen-smart-skills chat participant
xiigenSmartSkills.__test.chatHandlerLevel
```

The adapter builds `XiigenExtensionRequest`, writes trace refs, and calls:

```text
C:\Projects\llm_mvp_core\tools\xiigen-extension-core-api.ps1
```

### What existing artifacts prove and do not prove

Useful old reference:

```text
C:\Projects\cursor-xiigen-smart-skills\phase40-cursor-approved-host-runtime-rerun-20260603T115607Z\run-summary.json
```

That old run used Cursor with `--extensionDevelopmentPath` and produced runtime evidence files. It is useful as a candidate command pattern.

It does not close current runtime proof because it is old, points partly to the previous research copy under `C:\Users\lubao\Documents\xiigen\extensions reasearch\...`, and the plan requires current fresh evidence from the target source repo.

Dashboard/API tests and handler-level commands do not count as Cursor host runtime proof. They prove the handler/core boundary only.

### Boundary risks

- Source repo has real VS Code/Cursor extension code, not only JSON.
- Installed extension candidate is missing from `C:\Users\lubao\.cursor\extensions`; next runtime proof likely needs `--extensionDevelopmentPath C:\Projects\cursor-xiigen-smart-skills`.
- The test-only command `xiigenSmartSkills.__test.chatHandlerLevel` must not be reported as full chat UI evidence.
- Adapter contains no semantic/model logic; it calls core API and records boundary fields.
- Existing panel evidence may prove an Extension Host path only if rerun fresh from current repo and validated against the plan’s runtime proof packet.

### Missing fields before Cursor runtime PASS

- Fresh command invoking current `C:\Projects\cursor-xiigen-smart-skills` through Cursor Extension Host/UI.
- Fresh runtime evidence directory under current phase artifacts.
- Confirmation of command, chat participant or panel path used.
- Fresh request/response/debug trace created by the running extension.
- Negative/error path evidence.
- Parent review that no handler-only/test-only evidence was substituted for UI/Extension Host proof.

## Codex

### Что найдено

Source repo:

```text
C:\Projects\codex-xiigen-smart-skills
```

Source entrypoints:

```text
C:\Projects\codex-xiigen-smart-skills\src\codex-entrypoint.js
C:\Projects\codex-xiigen-smart-skills\src\codex-entrypoint.ps1
```

Plugin manifest:

```text
C:\Projects\codex-xiigen-smart-skills\.codex-plugin\plugin.json
```

Codex binary candidates:

```text
C:\Users\lubao\AppData\Local\OpenAI\Codex\bin\codex.exe
C:\Users\lubao\AppData\Local\OpenAI\Codex\bin\f1c7ee7a13db5fed\codex.exe
```

Installed Codex plugin cache checked:

```text
C:\Users\lubao\.codex\plugins\cache
```

No installed `codex-xiigen-smart-skills` cache entry was found in the checked cache root.

### What the Codex adapter actually exposes

The package declares:

```text
main: src/codex-entrypoint.js
pluginManifest: .codex-plugin/plugin.json
sourceEntrypointInvokesCoreEndpoint: true
coreEndpoint: C:/Projects/llm_mvp_core/tools/xiigen-extension-core-api.ps1
runtimeSmokeClaimed: false
semanticQualityClaimed: false
hostLogicBoundary: thin-request-client-and-redacted-debug-display-only
```

The source creates `XiigenExtensionRequest`, invokes the core endpoint, and can render the debug shape:

```text
xiigen <method> :
input: <redacted request sent to xiigen endpoint>
output: <redacted response received from xiigen endpoint>
```

### What existing artifacts prove and do not prove

Useful old reference:

```text
C:\Projects\codex-xiigen-smart-skills\reports\CX-04R-host-runtime-path-report.json
C:\Projects\codex-xiigen-smart-skills\reports\HOST-08F-codex-plugin-app-qa.json
```

The first report found a local Codex CLI but did not find a safe noninteractive plugin/app invocation path. The second report proves component/source-entrypoint QA only.

They do not close current Codex runtime proof because no fresh Codex plugin/app runtime was invoked in this slice, and component QA is not the same thing as Codex app/plugin chat/runtime evidence.

### Boundary risks

- Source entrypoint is real and thin, but installed plugin path is not known.
- Direct local `codex.exe` exists, but the known old report says plugin management existed while plugin/app invocation was not proven.
- `codex exec` may be model-backed execution; it needs explicit runtime/eval packet and must not be silently used as a local component proof.
- Debug trace rendering exists in source, but Codex app/plugin display evidence is not fresh.
- No model/training logic was found in the Codex adapter.

### Missing fields before Codex runtime PASS

- Exact supported Codex plugin/app invocation path.
- Evidence that the local plugin package is installed or loaded by Codex.
- Fresh visible app/plugin output or transcript.
- Fresh request/response/debug trace from the plugin/app runtime.
- Explicit separation from `codex exec` model-backed execution unless that track is approved.
- Parent review that source-entrypoint/component QA was not substituted for runtime proof.

## Why old artifacts are not final runtime proof

Old artifacts are useful for path discovery and regression design, but not completion proof for this plan. They are rejected as final proof when any of these is true:

1. The artifact is from a prior date or prior work order.
2. The artifact references a stale executable path.
3. The artifact proves only local helper, component, dashboard, handler, or direct core API behavior.
4. The artifact says `runtimeSmokeClaimed=false`, `fullHostRuntimeEvidence=false`, or equivalent.
5. The artifact did not come from the current phase’s scenario id, correlation id, and fresh run packet.

## Next recommended phase

Recommended next phase: `RUNTIME-02-host-runtime-proof-packet`.

It should prepare one fresh, bounded runtime probe packet for Claude, Cursor, and Codex. The packet must name exact commands, scenario id, correlation id, output paths, debug-on/off expectations, negative path, and proof boundaries. It must not launch training or mutate checkpoints.

Current map status:

- Claude: runtime path likely known, but stale executable path must be updated to `2.1.170` and fresh proof must be produced.
- Cursor: Extension Host path likely possible through `--extensionDevelopmentPath`, but installed extension path is not found and current-repo fresh proof is missing.
- Codex: local binary exists and source plugin package exists, but real plugin/app invocation path remains the biggest gap.
