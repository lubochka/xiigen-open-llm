# QUALITY-03 row-specific eval controls

Всего: 9 текущих quality rows для TOKEN-PAIR 001-009.
Сделано: для 9/9 строк создан per-row-controls packet с девятью категориями контроля: held-out, source-disjoint, nearest-neighbour/similarity-risk, ablation, no-leakage, no-memorization/no-exact-replay, inter-rater/adjudication, deterministic-boundary/no-hidden-shortcut и human-readable evidence refs.
Сейчас: runner-ready 0/9, quality-pass 0/9, runner/scorer не запускался. Причина: ключевые controls записаны как NOT_PROVEN или LIMITED, потому что в committed local artifacts нет row-specific proof refs для held-out/source-disjoint/nearest-neighbour/ablation/no-leakage/no-memorization/inter-rater.
Осталось: собрать или честно defer-нуть row-specific proof refs для 9/9 строк, затем создать runner-ready eval-only JSONL и только после этого запускать eval-only runner/scorer отдельным bounded work order.

## Что построено

- Machine-readable manifest: artifacts/extension-runtime-token-quality-learnability/quality-scoring/quality-scoring-20260619-104618/row-specific-eval-controls-manifest.json.
- Per-row controls JSONL: artifacts/extension-runtime-token-quality-learnability/quality-scoring/quality-scoring-20260619-104618/per-row-controls.jsonl.
- Runner input candidates JSONL: artifacts/extension-runtime-token-quality-learnability/quality-scoring/quality-scoring-20260619-104618/runner-input-candidates.jsonl.
- Все decisions детерминистические и локальные; внешняя AI/model оценка не вызывалась.
- No hidden shortcut claim не открыт: deterministic/helper boundaries записаны, но semantic/quality/readiness claims закрыты.

## Runner-ready статус

| row | pair | host | method | runner-ready | reason |
| --- | --- | --- | --- | --- | --- |
| QUALITY-03-REVIEW-ROW-001 | TOKEN-PAIR-CLAUDE-PROMPT-TO-TASK-001 | Claude | prompt-to-task | no | required controls not proven |
| QUALITY-03-REVIEW-ROW-002 | TOKEN-PAIR-CLAUDE-ORCHESTRATION-PLAN-001 | Claude | orchestration-plan | no | required controls not proven |
| QUALITY-03-REVIEW-ROW-003 | TOKEN-PAIR-CLAUDE-SKILL-SELECTION-001 | Claude | skill-selection | no | required controls not proven |
| QUALITY-03-REVIEW-ROW-004 | TOKEN-PAIR-CURSOR-PROMPT-TO-TASK-001 | Cursor | prompt-to-task | no | required controls not proven |
| QUALITY-03-REVIEW-ROW-005 | TOKEN-PAIR-CURSOR-ORCHESTRATION-PLAN-001 | Cursor | orchestration-plan | no | required controls not proven |
| QUALITY-03-REVIEW-ROW-006 | TOKEN-PAIR-CURSOR-SKILL-SELECTION-001 | Cursor | skill-selection | no | required controls not proven |
| QUALITY-03-REVIEW-ROW-007 | TOKEN-PAIR-CODEX-PROMPT-TO-TASK-001 | Codex | prompt-to-task | no | required controls not proven |
| QUALITY-03-REVIEW-ROW-008 | TOKEN-PAIR-CODEX-ORCHESTRATION-PLAN-001 | Codex | orchestration-plan | no | required controls not proven |
| QUALITY-03-REVIEW-ROW-009 | TOKEN-PAIR-CODEX-SKILL-SELECTION-001 | Codex | skill-selection | no | required controls not proven |

## Что это не доказывает

- Не доказаны token savings, quality improvement, full semantic quality, learnability, runtime readiness, common readiness, target promotion, locator update, final readiness или product readiness.
- Не доказано, что adaptive лучше baseline: quality runner/scorer не запускался.
- Не доказан no-memorization или source-disjoint split: для этого нужен следующий bounded proof work order.

## Следующий bounded work order

QUALITY-03-ROW-SPECIFIC-CONTROLS-PROOF-AND-RUNNER-RUN-OR-DEFER-04
