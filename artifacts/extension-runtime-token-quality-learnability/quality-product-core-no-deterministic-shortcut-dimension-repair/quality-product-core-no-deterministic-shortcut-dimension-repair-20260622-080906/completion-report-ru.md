# QUALITY-PRODUCT-CORE-NO-DETERMINISTIC-SHORTCUT-DIMENSION-REPAIR-01

## Что сделано

- Исправлен scorer: CANDIDATE_POSITION_WEIGHT_SCAFFOLD_ONLY больше не снижает no_deterministic_shortcut, если evidence полностью помечен как diagnostics/provenance-only и authority blocked.
- Raw authority остался запрещенным: candidate_position_suggestion=, candidate_index=, candidate_weight=, positive order authority или unlabelled candidate-position evidence дают failure.
- Regression test расширен: provenance-only строки PASS, а raw candidate-position authority, positive order-authority и unlabelled candidate-position строки FAIL.
- Выполнен свежий direct core API capture и fresh same-pair eval rerun; старые row-score файлы не использовались для success claim.

## Что найдено по authority

Runtime final authority не использует позицию кандидата для выбора skill. В SkillSelectionMachine сначала вызывается semantic path (TryDecideWithSemanticUnitsOnly), а путь с candidate-position weights возвращает пустой SelectedSkillIds, RequiresSecondPass=true и текст: candidate-position weights are diagnostics/leakage-guard evidence only. В XiigenExtensionApiService финальный fallback использует HOST_REQUESTED_SKILL_SURFACE_FROM_TASK_SEMANTICS и HOST_REQUESTED_SKILL_SURFACE_AVAILABLE_INTERSECTION, то есть task/request skill surface, не row/scenario/pair/hash/candidate position.

## Счетчики

| Метрика | Значение |
|---|---:|
| Done rows | 108 |
| Current rows | 108 |
| Remaining rows | 0 |
| Fresh rows accounted | 108 |
| Fresh pairs accounted | 54 |
| PASS rows | 108 |
| FAIL rows | 0 |
| NEEDS_REPAIR rows | 0 |
| Quality gate pairs | 54 |
| no_deterministic_shortcut below threshold | 0 |
| Candidate-position provenance-only rows | 54 |
| Candidate-position authority rows | 0 |
| Selected-skill shortcut violations | 0 |
| Token-savings-allowed pairs | 0 |

## Per-host

| Host | Rows | PASS | FAIL | NEEDS_REPAIR | Average |
|---|---:|---:|---:|---:|---:|
| Claude | 36 | 36 | 0 | 0 | 4 |
| Codex | 36 | 36 | 0 | 0 | 4 |
| Cursor | 36 | 36 | 0 | 0 | 4 |

## Per-method

| Method | Rows | PASS | FAIL | NEEDS_REPAIR | Average |
|---|---:|---:|---:|---:|---:|
| orchestration-plan | 36 | 36 | 0 | 0 | 4 |
| prompt-to-task | 36 | 36 | 0 | 0 | 4 |
| skill-selection | 36 | 36 | 0 | 0 | 4 |

## Claims

- product_ready=false
- token_savings_claim_allowed=false
- semantic_quality_claimed=false
- learnability_claimed=false
- Training/DPO/checkpoint/locator mutation: не выполнялись.
- Full Claude/Cursor/Codex extension readiness еще не доказана: нужен live host proof, source-disjoint semantic eval, ablation/leakage checks, fresh-load/continue-training probes и token reduction плюс quality gates на тех же парах.

## Риск

State file остается очень большим: примерно 91.2 MB. Это риск для GitHub push/review UX, но не первичный repair этого work order.
