# Plan Review Cycle 04 - Token/Quality/Live-Runtime Pairing Integrity

## Человеческий пакет

Стадия: цикл 4 из 10 перед любым продолжением runtime/code/training/host.

Фокус: целостность pairing для token savings, quality improvement и live-runtime comparisons. Проверка закрывает риск, что dashboard/API/helper/handler/test-only traces будут сравнены с live UI/chat traces как будто это один и тот же класс доказательства.

Reviewer/sub-agent id: `PLAN-REVIEW-CYCLE-04-TOKEN-QUALITY-LIVE-RUNTIME-PAIRING-INTEGRITY`.

Model/reasoning evidence: `model=gpt-5.5`, `reasoning_effort=xhigh`, `downgrade_warning_or_NONE=NONE`.

Загруженные обязательные gates:

- 10 Review Cycle Gate.
- Full Extension Definition Gate.
- test-integrity full-extension evidence.
- output readability.
- no-overclaim gates.

## Что проверено

- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md` sections 0, 4.1.1, 4.2, 4.2.1, 4.3, 8, 8.1, 9, 9.1, 9.2, 11, 11.1, 11.2.
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json` top-level cycle gate, active completion gate, QSG-11..19 entries and final claim booleans.
- `plan-review-cycle-01..03.md/json` and `visible-plan-summary-ru.md`.
- QSG-11..19 reports as evidence trail.

## Находки и исправления

1. PASS_WITH_PLAN_FIX: Section 4.2.1 уже требовал same scenario, host/runtime, method, tokenizer, isolation, repeated-run handling, redacted traces и quality non-regression. Но там не было одной конкретной матрицы, которая связывает token, quality и live-runtime claims с тем же evidence class и тем же target locator mode. Добавлена Section 4.2.2.
2. PASS_WITH_PAIRING_MATRIX: Section 4.2.2 теперь требует `pairing_integrity_status=PASS` до принятия token savings, quality improvement или live-runtime improvement.
3. PASS_WITH_EVIDENCE_CLASS_FIREWALL: dashboard/API, CLI helper, direct handler, package smoke, static report и test-only adapter traces нельзя сравнивать с live UI/chat/CLI/app traces как эквивалентные. Такие строки остаются scoped evidence своего класса.
4. PASS_WITH_TARGET_LOCATOR_GATE: baseline/adaptive/common rows обязаны использовать тот же `target_locator_mode`, locator ref, target checkpoint id, rollback checkpoint id и locator activation scope. Единственная допустимая плановая разница - явные `model_mode` / `adaptive_enabled` baseline-vs-adaptive flags.
5. PASS_WITH_TRACE_AND_RUBRIC_GATE: обе стороны пары должны иметь raw-private и redacted trace refs, core request/response/debug refs, same-run correlation id, явные baseline/adaptive/common flags и тот же scoring rubric id/version/adjudication method.
6. PASS_WITH_QSG_CONTEXT: QSG-14 остается bounded token/quality evidence только для своего paired trace set. QSG-18 имеет zero true live target-bound invocation passes. QSG-19 является handler/test-only locator plumbing. План теперь запрещает объединять их в один live target-bound token/quality/runtime PASS.
7. PASS_WITH_STATE_FIX: execution state переведен на `PLAN-REVIEW-CYCLE-04_OF_10`, записывает cycle 04 artifacts, сохраняет `implementation_authorized=false` и ставит next action на cycle 5.

## Результат pairing acceptance

`pairing_integrity_status=PASS` requires all of these to match for the same `pair_id`:

- тот же host и runtime surface;
- тот же prompt/task/scenario/method;
- тот же evidence class;
- тот же target locator mode;
- явные baseline/adaptive/common flags;
- сохраненные raw-private и redacted trace refs;
- тот же scoring rubric и source-disjoint/eval split.

Если любое поле отсутствует или смешано с другим классом доказательства, отчет обязан использовать `INVALID_PAIR`, `INVALID_TARGET_LOCATOR_PAIR`, `INCONCLUSIVE_QUALITY_PAIRING` или `SCOPED_NON_LIVE_QUALITY_EVIDENCE_ONLY`, а не PASS.

## Full extension dimension

cycle_counts_toward_10: true.

proves_three_full_extensions_in_this_dimension: true. The plan now requires Claude, Cursor and Codex token/quality/live-runtime claims to be paired per host and per evidence class, so all three future extensions must prove the same prompt/task/locator/rubric chain through `llm_mvp_core`.

product_readiness_proven_now: false. Этот цикл ремонтирует gate в плане; он не запускает live hosts, code, training, checkpoint mutation, QSG-20, commit или push.

## Recheck evidence

Post-patch verification status: PASS.

Выполненные проверки:

- PASS: JSON parse for execution state.
- PASS: JSON parse for cycle 04 JSON.
- PASS: `git diff --check`.
- PASS: targeted `rg` for pairing terms: `pairing_integrity_status`, `target_locator_mode`, `evidence_classification`, `baseline/adaptive/common`, `redacted trace`, `rubric`, `INVALID_TARGET_LOCATOR_PAIR`, `SCOPED_NON_LIVE_QUALITY_EVIDENCE_ONLY`.

## Verdict

PASS for cycle 4. Этот цикл засчитывается в requested 10 real review/fix cycles.

remaining_cycles_before_execution: 6.

Runtime/code/training/host execution, QSG-20, production locator update, checkpoint mutation, commit и push остаются запрещены в этом sub-agent slice.
