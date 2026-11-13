# Cursor repair/discovery - PASS

Work order: DOCS-QA-CURSOR-RUNTIME-PROOF-REPAIR-DISCOVERY-NO-SCORING-NO-TRAINING-01

## Что сломалось в предыдущем Cursor proof

Предыдущий Cursor runtime proof запустил локальный Cursor GUI/Extension Host через `--extensionDevelopmentPath` и подтвердил, что Extension Host видел extension id `xiigen.cursor-xiigen-smart-skills`. Но он не доказал, что positive или negative redacted request был надёжно вызван через команду расширения. Поэтому не появились свежие core request/response/trace от `llm_mvp_core`, не было сквозного correlation id на request/response, не было redacted input/output, и `reached_llm_mvp_core=false`.

## Какой path/harness теперь есть

В Cursor extension добавлен deterministic command-level harness:

- command id: `xiigenSmartSkills.runtimeProof.invoke`
- source implementation: `C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\src\extension.ts`
- runtime main: `C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\dist\extension.js`
- activation/config: `C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\package.json`

Команда принимает объект с `scenario`, `correlationId`, `prompt`, `evidenceRoot`, `redactedInput`. Для positive сценария она отправляет безопасный redacted prompt. Для negative сценария она допускает пустой prompt, что заставляет core API вернуть gaps/error path без host-side fallback. В обоих случаях команда вызывает тот же `runCursorSmartSkillsRequest`, который строит core request, вызывает `C:\Projects\llm_mvp_core\tools\phase40-extension-core-api.ps1`, пишет core response/trace, host trace, entrypoint response и redacted trace с correlation id.

## Был ли изменён код

Да. Изменены только три файла Cursor extension:

- `C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\package.json`: добавлены activation/contribution entries для `xiigenSmartSkills.runtimeProof.invoke`.
- `C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\src\extension.ts`: source теперь содержит тот же test-only command contract, positive/negative input normalization, `evidenceRoot` override, correlation-aware trace naming и redacted trace output.
- `C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\dist\extension.js`: добавлен command handler, correlation-aware trace naming, `evidenceRoot` override и redacted trace writer.

Repair follow-up corrected the source boundary: `src/extension.ts` is no longer stale. The harness is not dist-only: source/dist aligned. Это значит, что source и runtime artifact описывают один и тот же command-level путь, поэтому будущая сборка/source sync не должна стереть команду `xiigenSmartSkills.runtimeProof.invoke`.

## Почему это не dashboard/API shortcut

Это не dashboard/API shortcut, потому что следующий retry должен запускать Cursor/Extension Host с `--extensionDevelopmentPath`, активировать extension id `xiigen.cursor-xiigen-smart-skills`, затем вызвать command `xiigenSmartSkills.runtimeProof.invoke` через `vscode.commands.executeCommand`. Новый handler находится внутри Cursor extension runtime и переиспользует тот же `runCursorSmartSkillsRequest`, что и реальные command/chat/panel entrypoints. Он не вызывает `phase40-extension-core-api.ps1` снаружи как standalone dashboard или install-smoke shortcut.

Marker for review: source/dist aligned covers src/extension.ts and dist/extension.js.

## Final pushed Cursor extension commit after rebase

- Cursor extension repo path: `C:/Users/lubao/Documents/xiigen/extensions reasearch/cursor-xiigen-smart-skills`
- Final pushed commit: `73769ae8f251e573f1d0236a546abf04e28c3384`
- Branch: `main`
- Remote: `origin/main`
- Rebase conflict was resolved for `src/extension.ts` and `dist/extension.js`; `package.json` auto-merged/staged as part of the final commit.
- Parent push succeeded.

## Next retry command/path

Следующий retry должен использовать:

1. Cursor executable: `C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe`
2. Extension development path: `C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills`
3. Workspace: `C:\Projects\llm_mvp_core`
4. Environment: `PHASE40_CURSOR_RUNTIME_EVIDENCE_DIR=<NEXT_RUN_ROOT>\runtime-evidence`
5. Extension Host test body: activate `xiigen.cursor-xiigen-smart-skills`, then call `vscode.commands.executeCommand("xiigenSmartSkills.runtimeProof.invoke", ...)` once for positive and once for negative scenario.

Exact command/test contract is in `invocation-path-contract.json`.

## Что остаётся не доказанным

Этот repair/discovery PASS доказывает только, что retry теперь можно запускать по deterministic Cursor command-level path. Он не запускает сам runtime proof и не доказывает full chat UI evidence.

Он не доказывает token savings; не доказывает quality improvement; не доказывает learnability; не доказывает semantic quality; не доказывает model readiness. Также не доказывает scoring/eval readiness, training readiness, DPO, checkpoint promotion, full product readiness, Claude runtime proof или Codex runtime proof.

## Verification

- `node --check "C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\dist\extension.js"`: PASS
- `Get-Content -Raw "C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills\package.json" | ConvertFrom-Json | Out-Null`: PASS
- TypeScript compile/check: not run because `Get-Command tsc` returned `TSC_NOT_FOUND`; no local TypeScript compiler was available in PATH.
- TypeScript/source-boundary verification: `src/extension.ts` contains command id `xiigenSmartSkills.runtimeProof.invoke`, `normalizeRuntimeProofCommandInput`, `writeRedactedTrace`, `ensureEvidenceDirectory`, `buildTraceReferences`, `correlationId`, `evidenceRoot`, and `negative_error_request`; source/dist aligned.

Git commands, scoring, training, model mutation, checkpoint promotion, Claude runtime proof, Codex runtime proof, external AI/web/model calls and repo-wide tests were not run.
