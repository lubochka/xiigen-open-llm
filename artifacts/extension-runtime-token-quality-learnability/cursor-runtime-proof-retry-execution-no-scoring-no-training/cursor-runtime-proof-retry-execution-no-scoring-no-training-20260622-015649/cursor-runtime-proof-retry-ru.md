# Cursor runtime proof retry - PASS

Work order: DOCS-QA-CURSOR-RUNTIME-PROOF-RETRY-EXECUTION-NO-SCORING-NO-TRAINING-01

## Запущенные команды и пути

- Первый запуск через `C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe` вернул управление без runner result; это не засчитано как proof.
- Засчитанный запуск: `C:\Users\lubao\AppData\Local\Programs\cursor\resources\app\bin\cursor.cmd` с `--extensionDevelopmentPath`, `--extensionTestsPath`, isolated `--user-data-dir`, isolated `--extensions-dir`, `--headless`, `--new-window` и workspace `C:\Projects\llm_mvp_core`.
- Extension development path: `C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills`.
- Test runner: `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\cursor-runtime-proof-retry-execution-no-scoring-no-training\cursor-runtime-proof-retry-execution-no-scoring-no-training-20260622-015649\extension-host-tests\cursor-runtime-proof-retry-runner.js`.
- Runtime evidence dir: `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\cursor-runtime-proof-retry-execution-no-scoring-no-training\cursor-runtime-proof-retry-execution-no-scoring-no-training-20260622-015649\runtime-evidence`.
- Effective core endpoint: `C:\Projects\llm_mvp_core\tools\xiigen-extension-core-api.ps1`.
- Contract reference path also records: `C:\Projects\llm_mvp_core\tools\phase40-extension-core-api.ps1`.

## Почему это Cursor Extension Host command path

Cursor был запущен с `--extensionDevelopmentPath` и `--extensionTestsPath`; runner внутри Extension Host активировал extension id `xiigen.cursor-xiigen-smart-skills` и вызвал `vscode.commands.executeCommand("xiigenSmartSkills.runtimeProof.invoke", ...)`. Это не dashboard/API-only shortcut: core endpoint был вызван через runtime handler расширения, а не отдельным standalone script без Cursor Extension Host.

## Positive scenario

- Correlation id: `cursor-runtime-proof-retry-20260622-015649-positive`.
- Redacted input: `[REDACTED Cursor smart-skills routing request]`.
- Core status: `core_boundary_partial_no_semantic_quality_claim`.
- Core trace endpoint/call target: `xiigen-extension-core-api` -> `SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest`.
- Output summary: request passed the llm_mvp_core boundary; semantic quality and runtime extension claims stayed false.

## Negative scenario

- Correlation id: `cursor-runtime-proof-retry-20260622-015649-negative`.
- Redacted input: `[REDACTED missing or invalid Cursor smart-skills request]`.
- Controlled negative condition: empty prompt plus `XIIGEN_TARGET_LOCATOR_REQUIRED=true` for this invocation only.
- Core status/errors: `invalid_request` / `target_locator_required`.
- Output summary: negative error path was observed with redacted output and without fallback scoring/training/model mutation.

## Same-run correlation

- Same run id: `cursor-runtime-proof-retry-20260622-015649`.
- Positive request/core trace/host trace/redacted trace share `cursor-runtime-proof-retry-20260622-015649-positive`.
- Negative request/core trace/host trace/redacted trace share `cursor-runtime-proof-retry-20260622-015649-negative`.

## Fresh artifacts

- Positive request/core-response/core-trace/host-trace/entrypoint-response/redacted-trace were written under `runtime-evidence`.
- Negative request/core-response/core-trace/host-trace/entrypoint-response/redacted-trace were written under `runtime-evidence`.
- Summary artifacts are `cursor-runtime-proof-retry.json`, `positive-request-trace.json`, `negative-request-trace.json`, `fresh-artifacts-manifest.json`, `side-effect-audit.json`, `selected-follow-up-work-order.*`, and `state-entry.json`.

## Sanitized artifact boundary

Сгенерированные Cursor user-data (`cursor-user-data/`) и `cursor-extensions/` были удалены из proof packet после acceptance review, потому что это profile/cache/storage/trust-token/sentry material, а не proof payload. Доказательства proof остаются в `runtime-evidence/*.json`, top-level summaries, curated logs (пустые stdout/stderr logs), launch command/exit metadata и Extension Host runner script. `cursor-extensions/extensions.json` был пустым массивом `[]`; вместо папки оставлена sanitized запись в `sanitized-artifact-boundary.json`.

## RequestedSkills/AvailableSkills binding

The command input passed `RequestedSkills=[prompt-to-task, skill-selection]` and `AvailableSkills=[prompt-to-task, orchestration-plan, skill-selection]`. The generated core requests contain those arrays, and both host traces record `requestedSkillsPassedThrough=true`, `availableSkillsPassedThrough=true`, `hostSelectorOrScorerAdded=false`, and `host_skill_selection_binding=pass_through_only`. This proves pass-through binding, not a host-side selector/scorer.

## No-side-effect statement

No git, scoring/eval/token-savings measurement, training, DPO, checkpoint/model/locator/target mutation, Claude runtime proof, Codex runtime proof, external AI/web/model calls, broad private corpus reads, or repo-wide tests were run.

## Что не доказано

Этот proof не доказывает token savings; не доказывает quality improvement; не доказывает learnability; не доказывает semantic quality; не доказывает model readiness; не доказывает Claude/Codex runtime. Также он не доказывает full Cursor chat UI proof или product readiness.
