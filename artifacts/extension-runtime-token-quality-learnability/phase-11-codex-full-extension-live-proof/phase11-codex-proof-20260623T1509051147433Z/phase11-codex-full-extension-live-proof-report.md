# Phase 11 Codex full extension live proof

Пакет: PHASE_11_CODEX_PROOF_READY_FOR_REVIEW

Фаза 11/14 проверяла только Codex installed plugin proof: установленный skill surface `codex-xiigen-smart-skills` был загружен из plugin cache, `src/codex-entrypoint.js` был импортирован и запущен через `node`, а запрос дошел до `llm_mvp_core` через thin adapter boundary `tools/xiigen-extension-core-api.ps1`.

Что доказано:
- 11.02 package/entrypoint wiring: найден `.codex-plugin/plugin.json`, найден `package.json`, найден и импортирован `src/codex-entrypoint.js`, export `invokeXiigenCore` доступен, entrypoint описывает core endpoint.
- 11.03 live Codex/plugin path: positive debug-off и debug-on invocations вернули `Success=true`, а `CheckSetResults.core_boundary_executed=true`.
- 11.04 visible answer/error path: positive visible output показывает пользовательский ответ; controlled target-locator-required scenario вернул visible error `XIIGen Codex adapter error (invalid_request).` без падения harness.
- 11.05 debug off/on: debug-off не содержит `xiigen prompt-to-task :`; debug-on visible output содержит `xiigen prompt-to-task :`, `input: <redacted>`, `output: <redacted>`.
- 11.06 core boundary/correlation: request, response, trace и report связаны fresh correlation id `PHASE11-CODEX-20260623T1509051147433Z`.

Что не доказано:
- full Codex chat UI не доказан.
- token savings не доказаны.
- semantic quality improvement не доказано.
- learnability не доказана.
- training/eval/checkpoint mutation не запускались.
- all-host readiness и финальная готовность проекта не заявляются.

Как это выглядело бы пользователю:
- В success path пользователь видит обычный XIIGen ответ без raw prompt leak.
- При включенном debug виден redacted trace shape: `xiigen prompt-to-task :`, `input: <redacted>`, `output: <redacted>`.
- В error path пользователь видит понятный adapter error с `invalid_request`, а не crash harness.

Булевый отчет:
- installed_skill_surface_loaded = true
- adapter_core_boundary_executed = true
- installed_plugin_runtime_invocation_proven = true
- full_codex_chat_ui_proven = false
- token_savings_proven = false
- quality_improvement_proven = false
- learnability_proven = false
- training_started = false
- eval_started = false
- checkpoint_mutation_started = false

Счетчики после worker pass:
- Phase 11 substeps closed by this worker pass: 11.02, 11.03, 11.04, 11.05, 11.06.
- Phase 11 substeps remaining: 11.07 parent commit/status boundary.
- Whole plan counters are not advanced here because no parent review/commit/status boundary was performed.

Git boundary:
- Commit не выполнялся.
- Push не выполнялся.
- Branch не менялся.

Evidence:
- Machine report: `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-11-codex-full-extension-live-proof\phase11-codex-proof-20260623T1509051147433Z\phase11-codex-full-extension-live-proof-report.json`
- Wiring review: `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-11-codex-full-extension-live-proof\phase11-codex-proof-20260623T1509051147433Z\package-entrypoint-wiring-review.json`
- Correlation review: `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-11-codex-full-extension-live-proof\phase11-codex-proof-20260623T1509051147433Z\core-boundary-correlation-review.json`
- Run directory: `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-11-codex-full-extension-live-proof\phase11-codex-proof-20260623T1509051147433Z`

