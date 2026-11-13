# Plan Review Cycle 09 - Documentation, Debug and API/Dashboard Docs

## Человекочитаемый пакет

Стадия: цикл 9 из 10 перед любым продолжением runtime/code/training/host.

Reviewer/sub-agent id: `PLAN-REVIEW-CYCLE-09-DOCUMENTATION-DEBUG-API-DASHBOARD-DOCS`.

Model/reasoning evidence: `model=gpt-5.5`, `reasoning_effort=xhigh`, `downgrade_warning_or_NONE=NONE`.

Текущая authority: Luba назначила bounded execution-plan review/fix sub-agent, cycle 9 of 10, с фокусом на documentation, human-readable project docs, debug setting docs, API/dashboard verification docs. Агентов не спавнил, ветку не менял, commit/push не делал, runtime/source code не редактировал.

## Что проверено

- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md` sections 4.1.2, 4.6, 5.1, 6, 7, 8, 9, 9.1, 9.2, 11.1 and 11.2.
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json` top-level cycle gate and active completion gate.
- `plan-review-cycle-01..08.md/json` artifacts.
- Existing QSG/runtime/token/quality/local-learning state entries only as supporting context, not as runtime execution authority.

## Находки

1. `PASS_WITH_PLAN_FIX`: план уже имел QA evidence scope `documentation` и финальную строку `Documentation`, но не требовал конкретную матрицу документов. Это могло позволить закрыть DOC-01 одной общей страницей без объяснения `llm_mvp_core`, Claude/Cursor/Codex extensions, debug, dashboard/API, lifecycle and evidence scopes.
2. `PASS_WITH_DEBUG_DOCS_FIX`: debug trace был проверяемым в runtime/QA gates, но документация не требовала явно показать xiigen method, redacted input, redacted output, error path, correlation id and debug on/off behavior.
3. `PASS_WITH_API_DASHBOARD_DOCS_FIX`: dashboard/API QA был честно scoped, но не было отдельного how-to, который учит пользователя тестировать API через dashboard и не путать это с live Claude/Cursor/Codex runtime.
4. `PASS_WITH_LOCAL_MODEL_LIFECYCLE_DOCS_FIX`: backup/export/import/restore/load-without-retrain/continue-training gates были в learnability части плана, но docs gate не требовал человекочитаемого lifecycle guide.
5. `PASS_WITH_UML_NO_CODE_CODE_QUOTES_FIX`: в плане уже есть UML, no-code algorithms и короткие code quotes, но future docs не были обязаны переносить или ссылаться на эти элементы там, где документация проекта требует человеческое объяснение.
6. `PASS_WITH_RUSSIAN_STATUS_FIX`: план требовал Russian visible packet after cycle 10, но не требовал отдельный reusable Russian status/log template для будущих implementation/status docs.
7. `PASS_WITH_STATE_FIX`: execution state переведен на `PLAN-REVIEW-CYCLE-09_OF_10`, записывает cycle 09 artifacts, сохраняет `implementation_authorized=false` и ставит next action на cycle 10.

## Исправления в плане

- Добавлена Section `4.7. Human-readable documentation, debug and dashboard/API docs matrix`.
- Усилен phase `DOC-01`: теперь он требует конкретные docs для `llm_mvp_core`, Claude, Cursor, Codex, debug xiigen method/input/output, API/dashboard QA, evidence scopes, local lifecycle, UML/no-code/code quotes and Russian status docs.
- В Section 7 добавлены `documentation-matrix.md/json`, `documentation-qa-report.md/json` and `documentation-cross-reference-ledger.md/json`.
- В Section 8 pass 10 уточнены требования к documentation and verification.
- В Sections 9, 9.1 and 9.2 documentation acceptance теперь требует docs matrix, docs QA and no-overclaim.
- В Section 11.1 добавлена evidence row `Documentation QA report`, которая доказывает только docs coverage and cannot prove runtime/token/quality/learnability/final readiness.

## Документационная матрица, добавленная в план

План теперь требует эти типы документов:

| Doc type | Что обязан объяснить |
|---|---|
| Core project human doc | Как `llm_mvp_core` владеет core API, prompt-to-task, orchestration, skill selection, token, quality, DPO, local model lifecycle, rollback and no-overclaim. |
| Claude extension doc | Как Claude thin adapter загружается, вызывает `XiigenExtensionApiService`, показывает answer/debug/error и остается thin. |
| Cursor extension doc | Как Cursor/VS Code Extension Host path работает и почему local helper не равен live Extension Host proof. |
| Codex extension doc | Как Codex plugin/app runtime вызывает core API и почему package smoke не равен Codex runtime readiness. |
| Debug display guide | Как читать `xiigen {debugTrace.Method} :`, `input:`, `output:`, errors, correlation id and redaction for all methods. |
| API/dashboard QA guide | Как тестировать API через dashboard/API explorer and label this evidence as dashboard/API only. |
| Local model lifecycle guide | Как делать backup, export, import, restore, load-without-retrain, continue-training, fresh-load, held-out, ablation and rollback. |
| Evidence scope glossary | Что означают dashboard/API, handler-level, smoke, live host runtime, token-quality pair, local lifecycle and final readiness. |
| Russian status/log template | Как писать current stage, opened/closed claims, next bounded work order and remaining gaps по-русски. |

## Recheck evidence

Completed bounded verification for this cycle:

- PASS: execution state JSON parse.
- PASS: cycle 09 JSON parse.
- PASS: `git diff --check`.
- PASS: targeted `rg` for documentation, debug, dashboard, API, UML, no-code, code quotes, backup, export, import and continue-training.

`dotnet test`, runtime probes, host probes, training, checkpoint mutation, commit and push were not run because they are not authorized for this plan-review cycle.

## Verdict

PASS for cycle 9. Этот цикл засчитывается в requested 10 real review/fix cycles.

cycle_counts_toward_10: true.

cycles_completed_after_this_packet: 9.

remaining_cycles_before_execution: 1.

proves_three_full_extensions_in_this_dimension: true as a plan/documentation gate for future Claude/Cursor/Codex human-readable docs.

product_readiness_proven_now: false.

Runtime/code/training/host execution, QSG-20, production locator update, checkpoint mutation, commit and push remain forbidden in this sub-agent slice.
