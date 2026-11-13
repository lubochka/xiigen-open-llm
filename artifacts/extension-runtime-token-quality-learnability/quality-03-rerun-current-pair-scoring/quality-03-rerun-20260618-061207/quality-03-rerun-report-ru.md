# QUALITY-03 rerun current pair scoring

Run: `quality-03-rerun-20260618-061207`
Папка evidence: `artifacts/extension-runtime-token-quality-learnability/quality-03-rerun-current-pair-scoring/quality-03-rerun-20260618-061207`

## Что изменилось после initial QUALITY-03

Initial QUALITY-03 `quality-03-20260618-051740` честно остановился на inconclusive: пары TOKEN-03/TOKEN-04 совпали, но свежих QUALITY-01/02 rubric/scorer outputs для текущих 54 pair_id еще не было.
После этого появился refresh QUALITY-01/02 `quality-01-02-20260618-053620`, закоммиченный и запушенный как `93c86c293`: он дал human-readable rubric, source-disjoint explanation, scorer contract, 54 scorer input rows и 54 scorer output rows для тех же TOKEN-03/TOKEN-04 handler-level пар.
Этот rerun использует именно эти свежие scorer outputs. Он не запускает external AI, training, live host eval, checkpoint/locator mutation или full semantic readiness проверки.

## Как были оценены 54 пары

Для каждого `pair_id` rerun связал baseline row из TOKEN-03, adaptive row из TOKEN-04, scorer input row и scorer output row из QUALITY-01/02.
Проверялись одинаковые `pair_id`, host, method, scenario_id, prompt_hash, tokenizer/accounting version и redaction policy. Затем переносились прозрачные QUALITY-02 scores: baseline/adaptive layer scores, average delta, `limited_observed_non_regression`, strict `quality_non_regression_passed`, source-disjoint status и ablation/no-leakage/no-memorization status.
Важно: `limited_observed_non_regression=true` не превращается в strict quality PASS, потому что source-disjoint/no-leakage/no-memorization/ablation controls остаются inconclusive.

## Counts

- pair rows: 54
- pair integrity: PASS 54, FAIL 0
- strict quality non-regression: PASS 0, FAIL 0, INCONCLUSIVE 54
- limited scorer regression status: limited non-regression observed 54, limited regression detected 0
- token rows: reductions 0, equal 42, increases 12; global delta adaptive-minus-baseline 48
- token-savings gate: CLOSED_GLOBALLY_AND_PER_ROW; allowed rows 0/54

## Ограничения

- Handler-level only: это direct `XiigenExtensionApiService.ProcessHostExtensionRequest` evidence, не live Claude/Cursor/Codex UI/chat send-message capture.
- QSG28 target остается staged/test-only; production locator/checkpoint promotion не выполнялись и не доказывались.
- `OrchestrationReasoning` в adaptive rows использовал repo/common fallback; user/staged asset first-load не доказан для этого слоя.
- Source-disjoint/no-leakage/no-memorization/ablation controls остаются inconclusive: текущий scenario pack не дает достаточных `source_ref`, `doc_id`, license и train-vs-held-out partition evidence.
- Full semantic quality, learnability, live-host savings, production readiness и final readiness не заявлены.

## What this proves

- 54 текущие TOKEN-03/TOKEN-04 handler-level пары совпадают по pair integrity и были пересчитаны свежими QUALITY-01/02 scorer outputs.
- В узком deterministic scorer смысле регрессия не обнаружена: 54/54 rows имеют `limited_observed_non_regression=true`.

## What this does not prove

- Это не доказывает live host token savings, full semantic quality, learnability, target promotion, rollback safety, production locator readiness или final product readiness.
- Это не открывает token savings claim, потому что strict quality non-regression не passed, controls inconclusive, evidence handler-level only, QSG28 target staged/test-only, а total token delta не показывает глобального снижения.

## Next independent track

- Следующая требуемая фаза: `QUALITY-CONTROLS-SOURCE-DISJOINT-ABLATION-THEN-QUALITY-03-RERUN` — добавить/починить `source_ref`, `doc_id`, license, train-vs-held-out partition, no-leakage/no-memorization/ablation controls для текущего scenario pack, затем снова rerun QUALITY-03. Для live-host savings отдельно нужны same-scope live Claude/Cursor/Codex TOKEN pairs.
