# Semantic quality eval для Cursor Row12

Статус: `PARTIAL_NARROW_ROW12_SEMANTIC_EVAL`.

Проверен один узкий вопрос: существующий Path A core-response для Cursor Row12 превращает синтетический запрос в universal task, universal orchestration plan и допустимый набор skills. Это не проверка полной semantic quality продукта.

## Что проверено

- Prep rubric: `docs/plans/evidence/extension-full-readiness-dual-path/quality-semantic-row12-cursor-prep-20260628T145342Z/prep.json`.
- Path A request/core-response/core-trace/runner из `docs/plans/evidence/extension-full-readiness-dual-path/row12-cursor-timeout-fix-final-20260628T134935Z/`.
- Path B UI report: `docs/plans/evidence/extension-full-readiness-dual-path/row12-cursor-path-b-real-ui-20260628T140709Z/path-b-ui-report.json`, только как same-scenario UI correlation.
- Joined report: `docs/plans/evidence/extension-full-readiness-dual-path/row12-cursor-joined-path-a-b-20260628T142044Z/join-report.json`, только как mapping/boundary evidence.
- Token review: `docs/plans/evidence/extension-full-readiness-dual-path/token-row12-cursor-review-20260628T143616Z/review-report.json`, только как boundary evidence.
- Status board: `docs/plans/evidence/extension-full-readiness-dual-path/status-after-row12-token-review-20260628-174621/status-board.json`, только как group-4 boundary.

## Итог простыми словами

Прошло:
- `prompt_to_task` найден, парсится и содержит universal task с `task_id`, `task_title`, `task_type`, constraints, deliverables, acceptance gates, prompt redaction/hash и non-claims.
- `task_to_plan` найден, парсится и содержит ровно 3 ordered steps; у каждого шага есть `uses` и `verification_gate`.
- `task_to_skills` найден, парсится и выбирает только разрешенные skills: `prompt-to-task`, `task-to-plan`, `task-to-skills`, `evidence-ledger`.
- Candidate order / candidate position не используются как final semantic authority; они помечены как diagnostics/scaffold.
- Path B совпадает по Row12 scenario/pair/marker/hash, но это только UI route evidence; `submitted_to_core=false`.

Не прошло полностью:
- Reference preservation только `PARTIAL`: Row12 marker/scenario/pair сохранены через request correlation, runner mapping, `task_id`, prompt hash, joined report, token review и Path B report. Но внутри самого core-response поле `CheckSetResults.references` показывает `marker`, `scenario_id`, `pair_id` как `not-provided`.
- Evidence sufficiency только `PARTIAL`: этого достаточно для узкой structural eval, но не для full semantic quality. Нет held-out/source-disjoint набора, before/after quality comparison, ablation, no-memorization proof и regression guard.

Остается:
- Следующий bounded worker должен отдельно разобрать reference preservation gap.
- Следующий bounded worker должен подготовить held-out/source-disjoint semantic quality eval с ablation/no-leakage/no-memorization controls, если нужен claim шире этого узкого Row12 результата.

## Honesty block

claim_scope: один bounded eval packet для Cursor Row12 semantic-output structure и evidence boundaries
evidence_scope: использованы 9 JSON evidence records: prep rubric, Path A request, Path A core response, Path A core trace, Path A runner report, Path B UI report, joined report, token review, status board
count_claimed: 1 eval packet; 11 verdict rows; 0 claims о product readiness, token savings, training, checkpoint, locator или full semantic quality
independent_evidence_count: 9
subagent_id_or_NONE: QUALITY-SEMANTIC-ROW12-CURSOR-EVAL-01
downgrade_needed: да, evidence поддерживает только narrow Row12 structural semantic-output eval с partial evidence sufficiency

## Verdicts

| Layer | Verdict | Причина | Evidence |
| --- | --- | --- | --- |
| nested_output_presence | PASS | Все три nested outputs найдены и парсятся. | `request-core-response-2026-06-28T13-50-15-212Z.json` |
| prompt_to_task | PASS | Запрос стал universal task с нужными полями, constraints, deliverables, acceptance gates и non-claims. | `request-core-response-2026-06-28T13-50-15-212Z.json` |
| task_type_fit | PASS | `task_type=plan_only_or_approval_gated_task` подходит синтетическому request на bounded Smart Skills execution plan без external AI. | `request-request-2026-06-28T13-50-15-212Z.json`, `request-core-response-2026-06-28T13-50-15-212Z.json` |
| task_to_plan | PASS | Task стал universal orchestration plan: 3 ordered steps, у каждого `uses` и `verification_gate`. | `request-core-response-2026-06-28T13-50-15-212Z.json` |
| task_to_skills | PASS | Выбраны ровно разрешенные route skills; unapproved skills не найдены. | `request-core-response-2026-06-28T13-50-15-212Z.json` |
| leakage_guard | PASS | Candidate-order/position evidence помечены как diagnostics/scaffold only, selection authority blocked. | `request-core-response-2026-06-28T13-50-15-212Z.json` |
| non_claims | PASS | Forbidden claims остаются false. | `request-core-response-2026-06-28T13-50-15-212Z.json`, `row12-host-visible-runner-report.json`, `path-b-ui-report.json`, `review-report.json` |
| path_b_same_scenario_ui_correlation | PASS | Path B совпадает по Row12 mapping и показывает UI route, но не core semantic output. | `path-b-ui-report.json` |
| reference_preservation | PARTIAL | Mapping есть через request/runner/task_id/hash/join/token/Path B; в `CheckSetResults.references` marker/scenario/pair равны `not-provided`. | `request-request-2026-06-28T13-50-15-212Z.json`, `request-core-response-2026-06-28T13-50-15-212Z.json`, `row12-host-visible-runner-report.json` |
| evidence_sufficiency | PARTIAL | Узкая structural eval поддержана, full semantic quality не доказана. | `prep.json`, `status-board.json`, `review-report.json` |
| no_masquerade | PASS | Token counts, screenshots, status rows и joined report не использованы как semantic quality proof. | `join-report.json`, `path-b-ui-report.json`, `review-report.json`, `status-board.json` |

## Evidence boundaries

Эта eval поддерживает только: narrow Cursor Row12 semantic-output structure eval.

Не заявлено:
- product_readiness_claimed=false
- semantic_quality_claimed=false
- token_savings_claimed=false
- training_or_learnability_claimed=false
- checkpoint_mutation_or_promotion_claimed=false
- locator_update_claimed=false
- full_live_host_ui_submit_claimed=false
- external_ai_used=false

Path B cross-check: `path-b-ui-report.json` подтверждает same-scenario UI route correlation и `submitted_to_core=false`. Скриншоты/marker сами по себе не считаются semantic quality.

Token cross-check: `review-report.json` показывает `6209 -> 6209`, delta `0`, `token_savings_proven=false`. Это не quality proof.

## Artifact paths changed

- `docs/plans/evidence/extension-full-readiness-dual-path/quality-semantic-row12-cursor-eval-20260628T151416Z/eval.ru.md`
- `docs/plans/evidence/extension-full-readiness-dual-path/quality-semantic-row12-cursor-eval-20260628T151416Z/eval.json`

## Command/check summary

Выполнено до записи packet:
- `Get-Location` в `C:/Projects/llm_mvp_core_codex_sync_20260628`.
- `rg --files` по prep, Path A, Path B, joined и token review folders.
- `Get-Content -Raw` для prep/core/request/runner/Path B/join/token/status JSON records.
- `ConvertFrom-Json` для Path A core response и nested `prompt_to_task`, `task_to_plan`, `task_to_skills`.
- `ConvertFrom-Json` для same-scenario summary across request, runner, join, Path B, token review и status board.

Проверки после записи packet:
- parse `eval.json`: `PASS`.
- scan `eval.ru.md` на mojibake marker и forbidden true-claim markers для product readiness, semantic quality и token savings: `PASS`.
- `git status --short -- <new eval folder>`: `PASS_UNTRACKED_NEW_PACKET`.

## Next bounded work orders

1. `QUALITY-SEMANTIC-ROW12-REFERENCE-PRESERVATION-TRIAGE-01`

   Purpose: проверить, нужно ли явно заполнять `marker`, `scenario_id`, `pair_id` внутри `CheckSetResults.references` для Row12-like packets, и подготовить отдельный bounded implementation/documentation order при необходимости.

2. `QUALITY-SEMANTIC-ROW12-HELDOUT-ABLATION-01`

   Purpose: подготовить held-out/source-disjoint semantic quality eval с no-leakage/no-memorization controls, before/after comparison, ablation и regression guard для той же scenario family.
