# QUALITY-03 same-pair quality non-regression

Run: quality-03-20260618-051740

## Что здесь означает quality non-regression

В этой фазе quality non-regression означает узкую проверку: для одного и того же `pair_id` adaptive output из TOKEN-04 не должен быть хуже baseline output из TOKEN-03 по заранее принятой QUALITY-01/QUALITY-02 методике. Это не полная семантическая оценка, не доказательство learnability и не доказательство production readiness.

## Что сравнивалось

Сравнивались 54 handler-level пары: TOKEN-03 baseline `token-03-20260618-040021` против TOKEN-04 adaptive `token-04-20260618-044550`. Pair integrity прошел для 54/54: совпали `pair_id`, host, method, scenario_id, prompt hash, tokenizer/accounting version и redaction policy.

## Результат строк

- PASS quality non-regression: 0
- FAIL quality regression: 0
- INCONCLUSIVE quality non-regression: 54
- token savings claim allowed: 0

Причина inconclusive одна и та же для всех строк: свежий QUALITY-01/QUALITY-02 пакет для именно этих TOKEN-03/TOKEN-04 outputs отсутствует или stale. Существующие QUALITY/QSG artifacts являются историческими: предыдущий QUALITY-03 имеет 96 rows и другой набор pair_ids, а QSG rows не являются scorer packet для текущих 54 handler-level pairs. Поэтому scores не подставлялись и не выдумывались.

## Ограничения claims

TOKEN-03 и TOKEN-04 являются handler-level evidence через core boundary. Это не live-host UI/chat send-message evidence, поэтому live-host token savings не доказаны.

TOKEN-04 использовал QSG28 staged/test-only target locator. Это не production locator promotion и не checkpoint mutation. Для OrchestrationReasoning user/staged asset отсутствовал, поэтому был common fallback. Из-за этого пакет не доказывает full local learnability, semantic readiness, target promotion или final readiness.

## Token savings gate

Token deltas записаны как наблюдаемые cost/trace metrics, но claim остается закрытым глобально и по каждой строке: quality non-regression не прошел, он inconclusive. Даже строки с меньшим adaptive token count не могут открыть savings claim без same-pair quality non-regression PASS, а handler-level traces не могут доказать live-host savings.

## Что это доказывает

- 54/54 текущих TOKEN-03/TOKEN-04 handler-level pairs можно сопоставить по pair_id и базовым pairing fields.
- Для этих rows создан честный QUALITY-03 guard packet без внешнего AI, training, checkpoint/locator mutation или source-code edits.

## Что это не доказывает

- Не доказаны token savings, quality improvement, full semantic quality, learnability, live-host savings, production readiness, target promotion, rollback readiness или final readiness.
- Не доказано, что adaptive behavior лучше baseline; scoring заблокирован prerequisites.

## Следующий независимый трек

Нужно refresh/создать QUALITY-01 rubric/source-disjoint eval и QUALITY-02 scorer output, привязанные к текущим TOKEN-03/TOKEN-04 54 rows, затем rerun QUALITY-03 scoring. Для live-host savings отдельно нужны same-scope live host pairs.

## Row summary

| pair_id | host | method | pair integrity | quality result | reason |
|---|---|---|---|---|---|
| TQ-001-prompt-task-documentation\|Claude\|prompt-to-task\|debug-false | Claude | prompt-to-task | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-001-prompt-task-documentation\|Codex\|prompt-to-task\|debug-false | Codex | prompt-to-task | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-001-prompt-task-documentation\|Cursor\|prompt-to-task\|debug-false | Cursor | prompt-to-task | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-002-prompt-task-interrupt\|Claude\|prompt-to-task\|debug-false | Claude | prompt-to-task | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-002-prompt-task-interrupt\|Codex\|prompt-to-task\|debug-false | Codex | prompt-to-task | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-002-prompt-task-interrupt\|Cursor\|prompt-to-task\|debug-false | Cursor | prompt-to-task | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-003-prompt-task-redaction\|Claude\|prompt-to-task\|debug-false | Claude | prompt-to-task | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-003-prompt-task-redaction\|Codex\|prompt-to-task\|debug-false | Codex | prompt-to-task | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-003-prompt-task-redaction\|Cursor\|prompt-to-task\|debug-false | Cursor | prompt-to-task | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-004-orchestration-runtime-proof\|Claude\|orchestration-plan\|debug-false | Claude | orchestration-plan | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-004-orchestration-runtime-proof\|Codex\|orchestration-plan\|debug-false | Codex | orchestration-plan | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-004-orchestration-runtime-proof\|Cursor\|orchestration-plan\|debug-false | Cursor | orchestration-plan | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-005-orchestration-token-pairing\|Claude\|orchestration-plan\|debug-false | Claude | orchestration-plan | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-005-orchestration-token-pairing\|Codex\|orchestration-plan\|debug-false | Codex | orchestration-plan | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-005-orchestration-token-pairing\|Cursor\|orchestration-plan\|debug-false | Cursor | orchestration-plan | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-006-orchestration-learning-lifecycle\|Claude\|orchestration-plan\|debug-false | Claude | orchestration-plan | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-006-orchestration-learning-lifecycle\|Codex\|orchestration-plan\|debug-false | Codex | orchestration-plan | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-006-orchestration-learning-lifecycle\|Cursor\|orchestration-plan\|debug-false | Cursor | orchestration-plan | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-007-skill-architecture-audit\|Claude\|skill-selection\|debug-false | Claude | skill-selection | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-007-skill-architecture-audit\|Codex\|skill-selection\|debug-false | Codex | skill-selection | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-007-skill-architecture-audit\|Cursor\|skill-selection\|debug-false | Cursor | skill-selection | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-008-skill-debug-runtime\|Claude\|skill-selection\|debug-false | Claude | skill-selection | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-008-skill-debug-runtime\|Codex\|skill-selection\|debug-false | Codex | skill-selection | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-008-skill-debug-runtime\|Cursor\|skill-selection\|debug-false | Cursor | skill-selection | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-009-skill-learning-safety\|Claude\|skill-selection\|debug-false | Claude | skill-selection | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-009-skill-learning-safety\|Codex\|skill-selection\|debug-false | Codex | skill-selection | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-009-skill-learning-safety\|Cursor\|skill-selection\|debug-false | Cursor | skill-selection | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-010-mixed-no-overclaim\|Claude\|orchestration-plan\|debug-false | Claude | orchestration-plan | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-010-mixed-no-overclaim\|Claude\|prompt-to-task\|debug-false | Claude | prompt-to-task | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-010-mixed-no-overclaim\|Claude\|skill-selection\|debug-false | Claude | skill-selection | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-010-mixed-no-overclaim\|Codex\|orchestration-plan\|debug-false | Codex | orchestration-plan | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-010-mixed-no-overclaim\|Codex\|prompt-to-task\|debug-false | Codex | prompt-to-task | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-010-mixed-no-overclaim\|Codex\|skill-selection\|debug-false | Codex | skill-selection | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-010-mixed-no-overclaim\|Cursor\|orchestration-plan\|debug-false | Cursor | orchestration-plan | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-010-mixed-no-overclaim\|Cursor\|prompt-to-task\|debug-false | Cursor | prompt-to-task | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-010-mixed-no-overclaim\|Cursor\|skill-selection\|debug-false | Cursor | skill-selection | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-011-mixed-shortcut-audit\|Claude\|orchestration-plan\|debug-false | Claude | orchestration-plan | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-011-mixed-shortcut-audit\|Claude\|prompt-to-task\|debug-false | Claude | prompt-to-task | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-011-mixed-shortcut-audit\|Claude\|skill-selection\|debug-false | Claude | skill-selection | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-011-mixed-shortcut-audit\|Codex\|orchestration-plan\|debug-false | Codex | orchestration-plan | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-011-mixed-shortcut-audit\|Codex\|prompt-to-task\|debug-false | Codex | prompt-to-task | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-011-mixed-shortcut-audit\|Codex\|skill-selection\|debug-false | Codex | skill-selection | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-011-mixed-shortcut-audit\|Cursor\|orchestration-plan\|debug-false | Cursor | orchestration-plan | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-011-mixed-shortcut-audit\|Cursor\|prompt-to-task\|debug-false | Cursor | prompt-to-task | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-011-mixed-shortcut-audit\|Cursor\|skill-selection\|debug-false | Cursor | skill-selection | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-012-mixed-final-readiness\|Claude\|orchestration-plan\|debug-false | Claude | orchestration-plan | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-012-mixed-final-readiness\|Claude\|prompt-to-task\|debug-false | Claude | prompt-to-task | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-012-mixed-final-readiness\|Claude\|skill-selection\|debug-false | Claude | skill-selection | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-012-mixed-final-readiness\|Codex\|orchestration-plan\|debug-false | Codex | orchestration-plan | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-012-mixed-final-readiness\|Codex\|prompt-to-task\|debug-false | Codex | prompt-to-task | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-012-mixed-final-readiness\|Codex\|skill-selection\|debug-false | Codex | skill-selection | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-012-mixed-final-readiness\|Cursor\|orchestration-plan\|debug-false | Cursor | orchestration-plan | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-012-mixed-final-readiness\|Cursor\|prompt-to-task\|debug-false | Cursor | prompt-to-task | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
| TQ-012-mixed-final-readiness\|Cursor\|skill-selection\|debug-false | Cursor | skill-selection | PASS_SAME_PAIR_HANDLER_LEVEL | INCONCLUSIVE_QUALITY_NON_REGRESSION | claim_allowed=false; QUALITY_RUBRIC_MISSING_OR_STALE, QUALITY_02_SCORER_OUTPUT_MISSING_OR_STALE, SOURCE_DISJOINT_HELD_OUT_EVAL_MISSING_OR_STALE |
