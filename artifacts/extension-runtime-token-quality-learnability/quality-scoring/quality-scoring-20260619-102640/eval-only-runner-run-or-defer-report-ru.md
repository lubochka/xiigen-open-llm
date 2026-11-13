# QUALITY-03 eval-only runner: run/defer decision

Sub-agent: bounded-sub-agent-quality-03-eval-only-runner-run-or-defer-02
Work order: QUALITY-03-EVAL-ONLY-RUNNER-RUN-OR-DEFER-02
Run: quality-scoring-20260619-102640

## Status

Всего: 9 текущих TOKEN-PAIR rows для quality scoring.
Сделано: прочитаны state/plan, latest QUALITY-03 reviewer/adjudication inputs, 72 reviewer score rows, QUALITY-01 held-out/ablation requirements, QUALITY-02 runner contract/checklist и локальный ExtensionEvalOnlyQualityRunner/console wrapper.
Сейчас: scoring/run deferred. Все 9 rows имеют reviewer/adjudication input, но 0/9 runner-ready, потому что row-specific controls не найдены.
Осталось: создать controls для 9/9 rows и только потом готовить runner-ready eval-only JSONL.

## Что проверено

- Latest input folder: $latestRootRel.
- В каждой row controls.row_specific_controls все required refs пустые: held-out split, source-disjoint, nearest-neighbour, ablation, no-leakage, no-memorization и inter-rater/adjudication.
- Global QUALITY-01/QUALITY-02 refs существуют, но это requirements/contracts, а не результаты controls для конкретной строки.
- Локальный runner существует: 43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs и 43.SmartSkillsDomain.Tools/Quality02EvalOnlyRunner/Program.cs.
- Runner contract требует runner-ready eval-only rows; текущие rows являются reviewer/adjudication inputs и содержат explicit missing-control blockers.

## Почему scorer не запускался

Scoring сейчас был бы нечестным: он использовал бы narrow reviewer/status evidence без row-specific held-out/source-disjoint/nearest-neighbour/ablation/no-leakage/no-memorization/inter-rater controls. Это открыло бы риск overclaim. Поэтому eval runner/scorer не запускался, quality pass rows не произведены, token savings/quality improvement/semantic quality/learnability/runtime/product readiness остаются locked.

## Exact next bounded work order

QUALITY-03-ROW-SPECIFIC-EVAL-CONTROLS-AND-RUNNER-INPUTS-PRODUCE-03: создать human-readable и machine-readable controls для каждой из 9 current TOKEN-PAIR rows: held-out split, source-disjoint check, nearest-neighbour check, ablation report, no-leakage report, no-memorization report и inter-rater/adjudication ref. Затем создать Quality02EvalOnlyRunner-compatible JSONL. Не запускать scorer, пока controls и runner validation не пройдут.

## Confirmations

- External AI/model calls: не выполнялись.
- Training/DPO/checkpoint/model/locator mutation: не выполнялись.
- Eval runner/scorer: не запускался.
- Quality pass rows: 0/9.
- Claim locks: все остаются закрыты.
- Git stage/commit/push/branch: не выполнялись.
