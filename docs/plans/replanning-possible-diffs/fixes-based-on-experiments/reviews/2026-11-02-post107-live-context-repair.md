# Восстановление действующего контекста после POST107 — 2 ноября 2026

Работник: `/root/post107_card_context`. Фаза: чтение источников и согласованные правки Markdown. Этот работник не изменял код, runners, datasets, weights и исторические raw; не запускал эксперименты, тесты, сборки, inference или обучение и не выполнял Git mutations. Это утверждение о собственных действиях, не обо всех параллельных действиях в общем workspace.

Исходный снимок: `3cd2f26ced57da3acc4cd8a860c716a6bdac8a1e`; опубликованный аудит: `f3f9ae7b379d6e5f5f7707695083c769ae5647fe`. В первом снимке Git index не содержит unmerged entries, однако 179 Markdown содержали 801 строку конфликтных маркеров. Это 267 открывающих маркеров, включая вложенные; число не является количеством независимых смысловых проверок.

Перед записью внешний участник удалил шесть маркеров из `experimets/cards/P104-law-eligibility.md`. Эта правка прочитана через git diff, сохранена и не засчитывается работнику. Остальные файлы сверены с собственным снимком непосредственно перед записью; при drift запись пропускается для отдельного разбора.

## Что восстановлено

Сохранены дополнительные требования EXECUTOR-CONTEXT120/OWNER-CONTEXT120 и адресные POST88–POST107 блоки обеих сторон. Смысловые расхождения разрешены отдельно: прямое требование полного свежего чтения, последняя фактическая запись Q102–Q107 с ограничениями аудита123, N/A против NOT_MEASURED, исторический статус старых ремонтов, действительный владелец задачи и проверенные ссылки на исходники. Одинаковый заголовок RF02a не дублируется. История исходных опытов сохранена.

При начальном восстановлении байты вне конфликтных регионов оставлены без изменений. Последующее адресное усиление перечисляется отдельно; оно не выдаётся за механическое восстановление.

## Границы чтения

Свежо полностью прочитаны документы POST107 41/123/124/125. AGENTS/CLAUDE и большие skills прочитаны частично, поэтому полная загрузка основного стека НЕ заявляется. Процессное чтение файла программой и обнаружение разделов не являются полным смысловым чтением. Прямое требование полного чтения в контрактах сохранено; в этой сессии полный запрошенный reload не выполнен. Нативного PASS документная работа не подтверждает.

## Точный перечень восстановленных файлов работника

В таблице указаны внешние конфликтные регионы; вложенные разделители входят в соответствующий регион. Перечень фиксирует документные правки, а не семантические аудиты всех файлов.

| Файл | Внешних регионов | Строк маркеров до правки |
|---|---:|---:|
| [experimets/00-README.md](../../experimets/00-README.md) | 1 | 3 |
| [experimets/31-tool-discovery-and-wait.md](../../experimets/31-tool-discovery-and-wait.md) | 1 | 3 |
| [experimets/33-shared-learned-starts.md](../../experimets/33-shared-learned-starts.md) | 1 | 3 |
| [experimets/34-P-experiment-execution-contract.md](../../experimets/34-P-experiment-execution-contract.md) | 1 | 6 |
| [experimets/35-P-fixture-packs.md](../../experimets/35-P-fixture-packs.md) | 1 | 3 |
| [experimets/36-diagnostic-learning-programmes.md](../../experimets/36-diagnostic-learning-programmes.md) | 1 | 3 |
| [experimets/37-full-catalogue-learning-coverage.md](../../experimets/37-full-catalogue-learning-coverage.md) | 1 | 3 |
| [experimets/39-transfer-evidence-contract-2026-10-30.md](../../experimets/39-transfer-evidence-contract-2026-10-30.md) | 2 | 9 |
| [experimets/40-effective-executor-context-2026-11-02.md](../../experimets/40-effective-executor-context-2026-11-02.md) | 2 | 9 |
| [experimets/cards/00-execution-index.md](../../experimets/cards/00-execution-index.md) | 3 | 12 |
| [experimets/cards/HELDOUT-ADMISSION-ATOMICITY-01.md](../../experimets/cards/HELDOUT-ADMISSION-ATOMICITY-01.md) | 3 | 12 |
| [experimets/cards/KNN-ORDER-PERMUTATION-01.md](../../experimets/cards/KNN-ORDER-PERMUTATION-01.md) | 2 | 9 |
| [experimets/cards/METHOD-KNN-SCOPED-EXPRESSION-TARGET-01.md](../../experimets/cards/METHOD-KNN-SCOPED-EXPRESSION-TARGET-01.md) | 1 | 6 |
| [experimets/cards/P01-trainable-recipient.md](../../experimets/cards/P01-trainable-recipient.md) | 1 | 3 |
| [experimets/cards/P02-warm-base.md](../../experimets/cards/P02-warm-base.md) | 1 | 3 |
| [experimets/cards/P03-historical-score-lineage.md](../../experimets/cards/P03-historical-score-lineage.md) | 1 | 3 |
| [experimets/cards/P04-preference-contrast.md](../../experimets/cards/P04-preference-contrast.md) | 1 | 3 |
| [experimets/cards/P05-response-length-objective.md](../../experimets/cards/P05-response-length-objective.md) | 1 | 3 |
| [experimets/cards/P06-teaching-vector-adamw.md](../../experimets/cards/P06-teaching-vector-adamw.md) | 1 | 3 |
| [experimets/cards/P07-materialized-token-contrast.md](../../experimets/cards/P07-materialized-token-contrast.md) | 1 | 3 |
| [experimets/cards/P08-answer-boundary.md](../../experimets/cards/P08-answer-boundary.md) | 1 | 3 |
| [experimets/cards/P09-cached-forward.md](../../experimets/cards/P09-cached-forward.md) | 1 | 3 |
| [experimets/cards/P10-composed-gradient.md](../../experimets/cards/P10-composed-gradient.md) | 1 | 3 |
| [experimets/cards/P100-topk-ties.md](../../experimets/cards/P100-topk-ties.md) | 1 | 3 |
| [experimets/cards/P101-learning-budget.md](../../experimets/cards/P101-learning-budget.md) | 1 | 3 |
| [experimets/cards/P102-teacher-forced-decoding.md](../../experimets/cards/P102-teacher-forced-decoding.md) | 1 | 6 |
| [experimets/cards/P103-stack-before-normalization.md](../../experimets/cards/P103-stack-before-normalization.md) | 1 | 6 |
| [experimets/cards/P105-frozen-reference.md](../../experimets/cards/P105-frozen-reference.md) | 1 | 6 |
| [experimets/cards/P106-corrected-semantic-target.md](../../experimets/cards/P106-corrected-semantic-target.md) | 1 | 6 |
| [experimets/cards/P107-lawful-neighbor-aggregation.md](../../experimets/cards/P107-lawful-neighbor-aggregation.md) | 1 | 6 |
| [experimets/cards/P108-metadata-carrier.md](../../experimets/cards/P108-metadata-carrier.md) | 1 | 6 |
| [experimets/cards/P109-gold-validity.md](../../experimets/cards/P109-gold-validity.md) | 1 | 6 |
| [experimets/cards/P11-autograd-reachability.md](../../experimets/cards/P11-autograd-reachability.md) | 1 | 3 |
| [experimets/cards/P110-numeric-matrix-cost.md](../../experimets/cards/P110-numeric-matrix-cost.md) | 1 | 6 |
| [experimets/cards/P111-reward-credit.md](../../experimets/cards/P111-reward-credit.md) | 1 | 6 |
| [experimets/cards/P112-family-disjoint-evaluation.md](../../experimets/cards/P112-family-disjoint-evaluation.md) | 1 | 6 |
| [experimets/cards/P113-feature-energy.md](../../experimets/cards/P113-feature-energy.md) | 1 | 6 |
| [experimets/cards/P114-zero-weight-pair.md](../../experimets/cards/P114-zero-weight-pair.md) | 1 | 6 |
| [experimets/cards/P115-bm25-statistics-cache.md](../../experimets/cards/P115-bm25-statistics-cache.md) | 1 | 6 |
| [experimets/cards/P116-e43-file-boundary.md](../../experimets/cards/P116-e43-file-boundary.md) | 1 | 6 |
| [experimets/cards/P117-translation-direction-memory.md](../../experimets/cards/P117-translation-direction-memory.md) | 1 | 6 |
| [experimets/cards/P118-projectwiki-content.md](../../experimets/cards/P118-projectwiki-content.md) | 1 | 6 |
| [experimets/cards/P119-learning-dynamics.md](../../experimets/cards/P119-learning-dynamics.md) | 1 | 6 |
| [experimets/cards/P12-optimizer-boundaries.md](../../experimets/cards/P12-optimizer-boundaries.md) | 1 | 3 |
| [experimets/cards/P120-semantic-compression-laws.md](../../experimets/cards/P120-semantic-compression-laws.md) | 1 | 6 |
| [experimets/cards/P121-project-adapter-selection.md](../../experimets/cards/P121-project-adapter-selection.md) | 1 | 6 |
| [experimets/cards/P122-root-channel-unicode-digits-operators.md](../../experimets/cards/P122-root-channel-unicode-digits-operators.md) | 1 | 6 |
| [experimets/cards/P123-design-conversion-critic-evidence.md](../../experimets/cards/P123-design-conversion-critic-evidence.md) | 1 | 6 |
| [experimets/cards/P124-p3-fixed-discriminator-contract.md](../../experimets/cards/P124-p3-fixed-discriminator-contract.md) | 1 | 6 |
| [experimets/cards/P125-executor-memory-learning.md](../../experimets/cards/P125-executor-memory-learning.md) | 1 | 3 |
| [experimets/cards/P13-finite-ingress.md](../../experimets/cards/P13-finite-ingress.md) | 1 | 3 |
| [experimets/cards/P14-teacher-provenance.md](../../experimets/cards/P14-teacher-provenance.md) | 1 | 3 |
| [experimets/cards/P15-retention-replay.md](../../experimets/cards/P15-retention-replay.md) | 1 | 3 |
| [experimets/cards/P16-semantic-order-roles.md](../../experimets/cards/P16-semantic-order-roles.md) | 1 | 3 |
| [experimets/cards/P17-tokenizer-refit.md](../../experimets/cards/P17-tokenizer-refit.md) | 1 | 3 |
| [experimets/cards/P18-state-identity.md](../../experimets/cards/P18-state-identity.md) | 1 | 3 |
| [experimets/cards/P19-mutable-output.md](../../experimets/cards/P19-mutable-output.md) | 1 | 3 |
| [experimets/cards/P20-hnsw-reachability.md](../../experimets/cards/P20-hnsw-reachability.md) | 1 | 3 |
| [experimets/cards/P21-hnsw-ingress-atomicity.md](../../experimets/cards/P21-hnsw-ingress-atomicity.md) | 1 | 3 |
| [experimets/cards/P22-hnsw-rng-continuation.md](../../experimets/cards/P22-hnsw-rng-continuation.md) | 1 | 3 |
| [experimets/cards/P23-retrieval-fusion.md](../../experimets/cards/P23-retrieval-fusion.md) | 1 | 3 |
| [experimets/cards/P24-replay-selection.md](../../experimets/cards/P24-replay-selection.md) | 1 | 3 |
| [experimets/cards/P25-cold-restore.md](../../experimets/cards/P25-cold-restore.md) | 1 | 6 |
| [experimets/cards/P26-structural-features.md](../../experimets/cards/P26-structural-features.md) | 1 | 3 |
| [experimets/cards/P27-structural-codec.md](../../experimets/cards/P27-structural-codec.md) | 1 | 3 |
| [experimets/cards/P28-morphology-family.md](../../experimets/cards/P28-morphology-family.md) | 1 | 3 |
| [experimets/cards/P29-formula-memory.md](../../experimets/cards/P29-formula-memory.md) | 1 | 3 |
| [experimets/cards/P30-learned-fusion-alpha.md](../../experimets/cards/P30-learned-fusion-alpha.md) | 1 | 3 |
| [experimets/cards/P31-source-trust.md](../../experimets/cards/P31-source-trust.md) | 1 | 3 |
| [experimets/cards/P32-skill-selection.md](../../experimets/cards/P32-skill-selection.md) | 1 | 3 |
| [experimets/cards/P33-domain-classifier.md](../../experimets/cards/P33-domain-classifier.md) | 1 | 3 |
| [experimets/cards/P34-router-labels.md](../../experimets/cards/P34-router-labels.md) | 1 | 3 |
| [experimets/cards/P35-actual-neighbor-votes.md](../../experimets/cards/P35-actual-neighbor-votes.md) | 1 | 3 |
| [experimets/cards/P36-incremental-router.md](../../experimets/cards/P36-incremental-router.md) | 1 | 3 |
| [experimets/cards/P37-empty-router-query.md](../../experimets/cards/P37-empty-router-query.md) | 1 | 3 |
| [experimets/cards/P38-learned-segmentation.md](../../experimets/cards/P38-learned-segmentation.md) | 1 | 3 |
| [experimets/cards/P39-oracle-output-separation.md](../../experimets/cards/P39-oracle-output-separation.md) | 1 | 3 |
| [experimets/cards/P40-router-feedback.md](../../experimets/cards/P40-router-feedback.md) | 1 | 3 |
| [experimets/cards/P41-stop-and-split.md](../../experimets/cards/P41-stop-and-split.md) | 1 | 3 |
| [experimets/cards/P42-checker-arbiter.md](../../experimets/cards/P42-checker-arbiter.md) | 1 | 3 |
| [experimets/cards/P43-contextual-pos.md](../../experimets/cards/P43-contextual-pos.md) | 1 | 3 |
| [experimets/cards/P44-template-projection.md](../../experimets/cards/P44-template-projection.md) | 1 | 3 |
| [experimets/cards/P45-learned-reordering.md](../../experimets/cards/P45-learned-reordering.md) | 1 | 3 |
| [experimets/cards/P46-pair-value.md](../../experimets/cards/P46-pair-value.md) | 1 | 3 |
| [experimets/cards/P47-file-vector-context.md](../../experimets/cards/P47-file-vector-context.md) | 1 | 3 |
| [experimets/cards/P48-weighting-curriculum.md](../../experimets/cards/P48-weighting-curriculum.md) | 1 | 3 |
| [experimets/cards/P49-probe-warm-start.md](../../experimets/cards/P49-probe-warm-start.md) | 1 | 3 |
| [experimets/cards/P50-graph-dependencies.md](../../experimets/cards/P50-graph-dependencies.md) | 1 | 3 |
| [experimets/cards/P51-translation-context.md](../../experimets/cards/P51-translation-context.md) | 1 | 3 |
| [experimets/cards/P52-semantic-annotation.md](../../experimets/cards/P52-semantic-annotation.md) | 1 | 3 |
| [experimets/cards/P53-batch-atomicity.md](../../experimets/cards/P53-batch-atomicity.md) | 1 | 3 |
| [experimets/cards/P54-phrase-composition.md](../../experimets/cards/P54-phrase-composition.md) | 1 | 3 |
| [experimets/cards/P55-translation-laws.md](../../experimets/cards/P55-translation-laws.md) | 1 | 3 |
| [experimets/cards/P56-verifier-context.md](../../experimets/cards/P56-verifier-context.md) | 1 | 3 |
| [experimets/cards/P57-semantic-measurement.md](../../experimets/cards/P57-semantic-measurement.md) | 1 | 3 |
| [experimets/cards/P58-feedback-learning.md](../../experimets/cards/P58-feedback-learning.md) | 1 | 3 |
| [experimets/cards/P59-correction-visibility.md](../../experimets/cards/P59-correction-visibility.md) | 1 | 3 |
| [experimets/cards/P60-signature-duplicate.md](../../experimets/cards/P60-signature-duplicate.md) | 1 | 3 |
| [experimets/cards/P61-unknown-tail.md](../../experimets/cards/P61-unknown-tail.md) | 1 | 3 |
| [experimets/cards/P62-signature-monotonicity.md](../../experimets/cards/P62-signature-monotonicity.md) | 1 | 3 |
| [experimets/cards/P63-source-containment.md](../../experimets/cards/P63-source-containment.md) | 1 | 3 |
| [experimets/cards/P64-tier-postfilter.md](../../experimets/cards/P64-tier-postfilter.md) | 1 | 3 |
| [experimets/cards/P65-history-admission.md](../../experimets/cards/P65-history-admission.md) | 1 | 3 |
| [experimets/cards/P66-heldout-contrast.md](../../experimets/cards/P66-heldout-contrast.md) | 1 | 3 |
| [experimets/cards/P67-correction-identity.md](../../experimets/cards/P67-correction-identity.md) | 1 | 3 |
| [experimets/cards/P68-pretrain-pipeline.md](../../experimets/cards/P68-pretrain-pipeline.md) | 1 | 3 |
| [experimets/cards/P69-graphrag-context.md](../../experimets/cards/P69-graphrag-context.md) | 1 | 3 |
| [experimets/cards/P70-wikitrace-context.md](../../experimets/cards/P70-wikitrace-context.md) | 1 | 3 |
| [experimets/cards/P71-perplexity-alphabet.md](../../experimets/cards/P71-perplexity-alphabet.md) | 1 | 3 |
| [experimets/cards/P72-project-conditioning.md](../../experimets/cards/P72-project-conditioning.md) | 1 | 3 |
| [experimets/cards/P73-domain-conditioning.md](../../experimets/cards/P73-domain-conditioning.md) | 1 | 3 |
| [experimets/cards/P74-memory-eviction.md](../../experimets/cards/P74-memory-eviction.md) | 1 | 3 |
| [experimets/cards/P75-sampler-residual.md](../../experimets/cards/P75-sampler-residual.md) | 1 | 3 |
| [experimets/cards/P76-utf8-boundary.md](../../experimets/cards/P76-utf8-boundary.md) | 1 | 3 |
| [experimets/cards/P77-rare-class-support.md](../../experimets/cards/P77-rare-class-support.md) | 1 | 3 |
| [experimets/cards/P78-loss-plateau.md](../../experimets/cards/P78-loss-plateau.md) | 1 | 3 |
| [experimets/cards/P79-grammar-leakage.md](../../experimets/cards/P79-grammar-leakage.md) | 1 | 3 |
| [experimets/cards/P80-marker-denominator.md](../../experimets/cards/P80-marker-denominator.md) | 1 | 3 |
| [experimets/cards/P81-input-conditioning.md](../../experimets/cards/P81-input-conditioning.md) | 1 | 3 |
| [experimets/cards/P82-file-split.md](../../experimets/cards/P82-file-split.md) | 1 | 3 |
| [experimets/cards/P83-expression-body.md](../../experimets/cards/P83-expression-body.md) | 1 | 3 |
| [experimets/cards/P84-lambda-family.md](../../experimets/cards/P84-lambda-family.md) | 1 | 3 |
| [experimets/cards/P85-composite-obligations.md](../../experimets/cards/P85-composite-obligations.md) | 1 | 3 |
| [experimets/cards/P86-fractional-response-mask.md](../../experimets/cards/P86-fractional-response-mask.md) | 1 | 3 |
| [experimets/cards/P87-mixed-objectives.md](../../experimets/cards/P87-mixed-objectives.md) | 1 | 3 |
| [experimets/cards/P88-unit-context-consumption.md](../../experimets/cards/P88-unit-context-consumption.md) | 1 | 3 |
| [experimets/cards/P89-density-stop.md](../../experimets/cards/P89-density-stop.md) | 1 | 3 |
| [experimets/cards/P90-tagged-search.md](../../experimets/cards/P90-tagged-search.md) | 1 | 3 |
| [experimets/cards/P91-method-pair-split.md](../../experimets/cards/P91-method-pair-split.md) | 1 | 3 |
| [experimets/cards/P92-dpo-memory-flow-bridge.md](../../experimets/cards/P92-dpo-memory-flow-bridge.md) | 1 | 3 |
| [experimets/cards/P93-confidence-abstention.md](../../experimets/cards/P93-confidence-abstention.md) | 1 | 6 |
| [experimets/cards/P94-typed-many-results.md](../../experimets/cards/P94-typed-many-results.md) | 1 | 3 |
| [experimets/cards/P95-learned-flow-edge.md](../../experimets/cards/P95-learned-flow-edge.md) | 1 | 3 |
| [experimets/cards/P96-async-orchestration.md](../../experimets/cards/P96-async-orchestration.md) | 1 | 3 |
| [experimets/cards/P97-learned-graph-order.md](../../experimets/cards/P97-learned-graph-order.md) | 1 | 3 |
| [experimets/cards/P98-tokenizer-capacity.md](../../experimets/cards/P98-tokenizer-capacity.md) | 1 | 3 |
| [experimets/cards/P99-unknown-token-id.md](../../experimets/cards/P99-unknown-token-id.md) | 1 | 3 |
| [experimets/cards/PHRASE-COVERAGE-CAP-01.md](../../experimets/cards/PHRASE-COVERAGE-CAP-01.md) | 2 | 9 |
| [experimets/cards/SAVED-STATE-PANEL-0407-01.md](../../experimets/cards/SAVED-STATE-PANEL-0407-01.md) | 1 | 6 |
| [experimets/prompts/P-SERIES-CONTINUE-executor.md](../../experimets/prompts/P-SERIES-CONTINUE-executor.md) | 3 | 9 |
| [fixes-based-on-experiments/01-registry.md](../../fixes-based-on-experiments/01-registry.md) | 1 | 6 |
| [fixes-based-on-experiments/02-learning-state-and-fixtures.md](../../fixes-based-on-experiments/02-learning-state-and-fixtures.md) | 1 | 6 |
| [fixes-based-on-experiments/03-test-and-execution-contract.md](../../fixes-based-on-experiments/03-test-and-execution-contract.md) | 1 | 6 |
| [fixes-based-on-experiments/04-evidence-and-novelty.md](../../fixes-based-on-experiments/04-evidence-and-novelty.md) | 1 | 6 |
| [fixes-based-on-experiments/101-persistent-state-bundle-contract-2026-10-30.md](../../fixes-based-on-experiments/101-persistent-state-bundle-contract-2026-10-30.md) | 2 | 9 |
| [fixes-based-on-experiments/102-late-transfer-repair-plan-2026-10-29.md](../../fixes-based-on-experiments/102-late-transfer-repair-plan-2026-10-29.md) | 1 | 3 |
| [fixes-based-on-experiments/103-transfer-queue-commit-mapping-2026-10-30.md](../../fixes-based-on-experiments/103-transfer-queue-commit-mapping-2026-10-30.md) | 2 | 6 |
| [fixes-based-on-experiments/105-addressed-repair-plan-2026-10-30.md](../../fixes-based-on-experiments/105-addressed-repair-plan-2026-10-30.md) | 12 | 36 |
| [fixes-based-on-experiments/108-addressed-repair-plan-2026-11-01.md](../../fixes-based-on-experiments/108-addressed-repair-plan-2026-11-01.md) | 12 | 36 |
| [fixes-based-on-experiments/109-card-hardening-and-coverage-2026-11-01.md](../../fixes-based-on-experiments/109-card-hardening-and-coverage-2026-11-01.md) | 3 | 9 |
| [fixes-based-on-experiments/11-translation-verifier-admission.md](../../fixes-based-on-experiments/11-translation-verifier-admission.md) | 1 | 6 |
| [fixes-based-on-experiments/111-post-p55-transfer-plan-2026-11-01.md](../../fixes-based-on-experiments/111-post-p55-transfer-plan-2026-11-01.md) | 1 | 3 |
| [fixes-based-on-experiments/112-convergence-two-diseases-second-consumer-2026-11-01.md](../../fixes-based-on-experiments/112-convergence-two-diseases-second-consumer-2026-11-01.md) | 1 | 3 |
| [fixes-based-on-experiments/113-post88-transfer-and-experiment-audit-2026-11-01.md](../../fixes-based-on-experiments/113-post88-transfer-and-experiment-audit-2026-11-01.md) | 1 | 3 |
| [fixes-based-on-experiments/114-post88-addressed-repair-plan-2026-11-01.md](../../fixes-based-on-experiments/114-post88-addressed-repair-plan-2026-11-01.md) | 9 | 27 |
| [fixes-based-on-experiments/115-post88-card-hardening-and-coverage-2026-11-01.md](../../fixes-based-on-experiments/115-post88-card-hardening-and-coverage-2026-11-01.md) | 1 | 3 |
| [fixes-based-on-experiments/13-dpo-reference-cache.md](../../fixes-based-on-experiments/13-dpo-reference-cache.md) | 1 | 3 |
| [fixes-based-on-experiments/15-prompt-flow-retention.md](../../fixes-based-on-experiments/15-prompt-flow-retention.md) | 1 | 3 |
| [fixes-based-on-experiments/16-replay-domain-retention.md](../../fixes-based-on-experiments/16-replay-domain-retention.md) | 1 | 3 |
| [fixes-based-on-experiments/19-translation-memory-coverage.md](../../fixes-based-on-experiments/19-translation-memory-coverage.md) | 1 | 6 |
| [fixes-based-on-experiments/20-prompt-domain-e2e.md](../../fixes-based-on-experiments/20-prompt-domain-e2e.md) | 1 | 3 |
| [fixes-based-on-experiments/21-sft-optimizer-continuity.md](../../fixes-based-on-experiments/21-sft-optimizer-continuity.md) | 1 | 3 |
| [fixes-based-on-experiments/26-method-knn-training-admission.md](../../fixes-based-on-experiments/26-method-knn-training-admission.md) | 1 | 3 |
| [fixes-based-on-experiments/27-predicate-dataset-router-learning-coverage.md](../../fixes-based-on-experiments/27-predicate-dataset-router-learning-coverage.md) | 1 | 3 |
| [fixes-based-on-experiments/28-dpo-tokenizer-snapshot.md](../../fixes-based-on-experiments/28-dpo-tokenizer-snapshot.md) | 1 | 3 |
| [fixes-based-on-experiments/33-wiki-training-feature-reuse.md](../../fixes-based-on-experiments/33-wiki-training-feature-reuse.md) | 1 | 3 |
| [fixes-based-on-experiments/35-method-knn-target-scope.md](../../fixes-based-on-experiments/35-method-knn-target-scope.md) | 1 | 3 |
| [fixes-based-on-experiments/36-dpo-scored-row-head.md](../../fixes-based-on-experiments/36-dpo-scored-row-head.md) | 1 | 3 |
| [fixes-based-on-experiments/40-bpe-realized-merge-objective.md](../../fixes-based-on-experiments/40-bpe-realized-merge-objective.md) | 1 | 3 |
| [fixes-based-on-experiments/48-learned-route-body-identity.md](../../fixes-based-on-experiments/48-learned-route-body-identity.md) | 1 | 3 |
| [fixes-based-on-experiments/49-router-learned-key-normalization.md](../../fixes-based-on-experiments/49-router-learned-key-normalization.md) | 1 | 3 |
| [fixes-based-on-experiments/52-translation-training-identity-index.md](../../fixes-based-on-experiments/52-translation-training-identity-index.md) | 1 | 3 |
| [fixes-based-on-experiments/54-frozen-base-check-scope.md](../../fixes-based-on-experiments/54-frozen-base-check-scope.md) | 1 | 6 |
| [fixes-based-on-experiments/58-optimizer-step-metadata-read.md](../../fixes-based-on-experiments/58-optimizer-step-metadata-read.md) | 1 | 3 |
| [fixes-based-on-experiments/74-knn-domain-constructor-transfer.md](../../fixes-based-on-experiments/74-knn-domain-constructor-transfer.md) | 1 | 3 |
| [fixes-based-on-experiments/83-learned-bpe-boundary-transfer-addendum.md](../../fixes-based-on-experiments/83-learned-bpe-boundary-transfer-addendum.md) | 1 | 3 |
| [fixes-based-on-experiments/90-review-and-publication.md](../../fixes-based-on-experiments/90-review-and-publication.md) | 1 | 6 |
| [fixes-based-on-experiments/README.md](../../fixes-based-on-experiments/README.md) | 1 | 6 |

Собственные изменения при первоначальном восстановлении: 178 файлов; удалено 795 строк маркеров; внешних регионов 222. Drift-пропуски: нет.

## Адресное усиление после восстановления

Содержательно усилены 8 существующих карточек/договоров, без нового общего слоя одинаковых вставок. P108/P119/P123/P124 уточнены внутри уже действующего POST107-блока; P56/P57/33/101 — внутри существующего входа120. Это профилактические договоры будущей приёмки, не новые найденные runtime-дефекты этих восьми владельцев. Общие 40/41/120 задают физическую целостность входа, слепую калибровку с достижением нужной стадии, неизменность original и обязательное составление технического итога.

| Файл | Конкретный риск и контроль |
|---|---|
| [P108](../../experimets/cards/P108-metadata-carrier.md) | Подмена одного consumed ID при том же count; три carriers и два invalid; missing outcome не исчезает из required set |
| [P119](../../experimets/cards/P119-learning-dynamics.md) | Абсолютный logP(gold) и odds имеют разные направления; first-step и final checkpoint не заменяются trajectory/step count |
| [P123](../../experimets/cards/P123-design-conversion-critic-evidence.md) | Learned verdict отделён от supplied QA; исправный геометрический scorer и полнота A/B/C обязательны |
| [P124](../../experimets/cards/P124-p3-fixed-discriminator-contract.md) | Сохранены required/actual identity и полный verdict; PASS_WITH_GAPS не становится полным enforcement |
| [P56](../../experimets/cards/P56-verifier-context.md) | ready→other при той же длине; not ready сохраняет исходный узкий gold, полярность не выдумывается |
| [P57](../../experimets/cards/P57-semantic-measurement.md) | Уже назначенные polarity/quantity controls и перестановка ID при прежнем агрегате; valid no-gain не failure |
| [33](../33-wiki-training-feature-reuse.md) | Cache feature values, terminal provenance, подготовка/epoch loop и ordinary publisher; S03/S06 не переоткрывают R09 |
| [101](../101-persistent-state-bundle-contract-2026-10-30.md) | Все применимые части, новый owner/load, полный ordinary payload и отсутствие partial publication; ранний checksum не заменяет semantic evidence |

Отдельно согласован ровно один устаревший первый sentence «Использовать пять точных навыков» в активных cards. Остальной текст, исторические skill hashes и 18 старых probe-документов не переписывались. P104 получил только эту нашу фразу поверх чужого удаления шести маркеров; его исходное concurrent разрешение не присвоено работнику.

Уточнение активного skill-entry: 126 файлов. Это не 126 независимых содержательных аудитов. Точный список:

- [experimets/cards/N61-dpo-reference-reuse.md](../../experimets/cards/N61-dpo-reference-reuse.md)
- [experimets/cards/P01-trainable-recipient.md](../../experimets/cards/P01-trainable-recipient.md)
- [experimets/cards/P02-warm-base.md](../../experimets/cards/P02-warm-base.md)
- [experimets/cards/P03-historical-score-lineage.md](../../experimets/cards/P03-historical-score-lineage.md)
- [experimets/cards/P04-preference-contrast.md](../../experimets/cards/P04-preference-contrast.md)
- [experimets/cards/P05-response-length-objective.md](../../experimets/cards/P05-response-length-objective.md)
- [experimets/cards/P06-teaching-vector-adamw.md](../../experimets/cards/P06-teaching-vector-adamw.md)
- [experimets/cards/P07-materialized-token-contrast.md](../../experimets/cards/P07-materialized-token-contrast.md)
- [experimets/cards/P08-answer-boundary.md](../../experimets/cards/P08-answer-boundary.md)
- [experimets/cards/P09-cached-forward.md](../../experimets/cards/P09-cached-forward.md)
- [experimets/cards/P10-composed-gradient.md](../../experimets/cards/P10-composed-gradient.md)
- [experimets/cards/P100-topk-ties.md](../../experimets/cards/P100-topk-ties.md)
- [experimets/cards/P101-learning-budget.md](../../experimets/cards/P101-learning-budget.md)
- [experimets/cards/P102-teacher-forced-decoding.md](../../experimets/cards/P102-teacher-forced-decoding.md)
- [experimets/cards/P103-stack-before-normalization.md](../../experimets/cards/P103-stack-before-normalization.md)
- [experimets/cards/P104-law-eligibility.md](../../experimets/cards/P104-law-eligibility.md)
- [experimets/cards/P105-frozen-reference.md](../../experimets/cards/P105-frozen-reference.md)
- [experimets/cards/P106-corrected-semantic-target.md](../../experimets/cards/P106-corrected-semantic-target.md)
- [experimets/cards/P107-lawful-neighbor-aggregation.md](../../experimets/cards/P107-lawful-neighbor-aggregation.md)
- [experimets/cards/P108-metadata-carrier.md](../../experimets/cards/P108-metadata-carrier.md)
- [experimets/cards/P109-gold-validity.md](../../experimets/cards/P109-gold-validity.md)
- [experimets/cards/P11-autograd-reachability.md](../../experimets/cards/P11-autograd-reachability.md)
- [experimets/cards/P110-numeric-matrix-cost.md](../../experimets/cards/P110-numeric-matrix-cost.md)
- [experimets/cards/P111-reward-credit.md](../../experimets/cards/P111-reward-credit.md)
- [experimets/cards/P112-family-disjoint-evaluation.md](../../experimets/cards/P112-family-disjoint-evaluation.md)
- [experimets/cards/P113-feature-energy.md](../../experimets/cards/P113-feature-energy.md)
- [experimets/cards/P114-zero-weight-pair.md](../../experimets/cards/P114-zero-weight-pair.md)
- [experimets/cards/P115-bm25-statistics-cache.md](../../experimets/cards/P115-bm25-statistics-cache.md)
- [experimets/cards/P116-e43-file-boundary.md](../../experimets/cards/P116-e43-file-boundary.md)
- [experimets/cards/P117-translation-direction-memory.md](../../experimets/cards/P117-translation-direction-memory.md)
- [experimets/cards/P118-projectwiki-content.md](../../experimets/cards/P118-projectwiki-content.md)
- [experimets/cards/P119-learning-dynamics.md](../../experimets/cards/P119-learning-dynamics.md)
- [experimets/cards/P12-optimizer-boundaries.md](../../experimets/cards/P12-optimizer-boundaries.md)
- [experimets/cards/P120-semantic-compression-laws.md](../../experimets/cards/P120-semantic-compression-laws.md)
- [experimets/cards/P121-project-adapter-selection.md](../../experimets/cards/P121-project-adapter-selection.md)
- [experimets/cards/P122-root-channel-unicode-digits-operators.md](../../experimets/cards/P122-root-channel-unicode-digits-operators.md)
- [experimets/cards/P123-design-conversion-critic-evidence.md](../../experimets/cards/P123-design-conversion-critic-evidence.md)
- [experimets/cards/P124-p3-fixed-discriminator-contract.md](../../experimets/cards/P124-p3-fixed-discriminator-contract.md)
- [experimets/cards/P125-executor-memory-learning.md](../../experimets/cards/P125-executor-memory-learning.md)
- [experimets/cards/P13-finite-ingress.md](../../experimets/cards/P13-finite-ingress.md)
- [experimets/cards/P14-teacher-provenance.md](../../experimets/cards/P14-teacher-provenance.md)
- [experimets/cards/P15-retention-replay.md](../../experimets/cards/P15-retention-replay.md)
- [experimets/cards/P16-semantic-order-roles.md](../../experimets/cards/P16-semantic-order-roles.md)
- [experimets/cards/P17-tokenizer-refit.md](../../experimets/cards/P17-tokenizer-refit.md)
- [experimets/cards/P18-state-identity.md](../../experimets/cards/P18-state-identity.md)
- [experimets/cards/P19-mutable-output.md](../../experimets/cards/P19-mutable-output.md)
- [experimets/cards/P20-hnsw-reachability.md](../../experimets/cards/P20-hnsw-reachability.md)
- [experimets/cards/P21-hnsw-ingress-atomicity.md](../../experimets/cards/P21-hnsw-ingress-atomicity.md)
- [experimets/cards/P22-hnsw-rng-continuation.md](../../experimets/cards/P22-hnsw-rng-continuation.md)
- [experimets/cards/P23-retrieval-fusion.md](../../experimets/cards/P23-retrieval-fusion.md)
- [experimets/cards/P24-replay-selection.md](../../experimets/cards/P24-replay-selection.md)
- [experimets/cards/P25-cold-restore.md](../../experimets/cards/P25-cold-restore.md)
- [experimets/cards/P26-structural-features.md](../../experimets/cards/P26-structural-features.md)
- [experimets/cards/P27-structural-codec.md](../../experimets/cards/P27-structural-codec.md)
- [experimets/cards/P28-morphology-family.md](../../experimets/cards/P28-morphology-family.md)
- [experimets/cards/P29-formula-memory.md](../../experimets/cards/P29-formula-memory.md)
- [experimets/cards/P30-learned-fusion-alpha.md](../../experimets/cards/P30-learned-fusion-alpha.md)
- [experimets/cards/P31-source-trust.md](../../experimets/cards/P31-source-trust.md)
- [experimets/cards/P32-skill-selection.md](../../experimets/cards/P32-skill-selection.md)
- [experimets/cards/P33-domain-classifier.md](../../experimets/cards/P33-domain-classifier.md)
- [experimets/cards/P34-router-labels.md](../../experimets/cards/P34-router-labels.md)
- [experimets/cards/P35-actual-neighbor-votes.md](../../experimets/cards/P35-actual-neighbor-votes.md)
- [experimets/cards/P36-incremental-router.md](../../experimets/cards/P36-incremental-router.md)
- [experimets/cards/P37-empty-router-query.md](../../experimets/cards/P37-empty-router-query.md)
- [experimets/cards/P38-learned-segmentation.md](../../experimets/cards/P38-learned-segmentation.md)
- [experimets/cards/P39-oracle-output-separation.md](../../experimets/cards/P39-oracle-output-separation.md)
- [experimets/cards/P40-router-feedback.md](../../experimets/cards/P40-router-feedback.md)
- [experimets/cards/P41-stop-and-split.md](../../experimets/cards/P41-stop-and-split.md)
- [experimets/cards/P42-checker-arbiter.md](../../experimets/cards/P42-checker-arbiter.md)
- [experimets/cards/P43-contextual-pos.md](../../experimets/cards/P43-contextual-pos.md)
- [experimets/cards/P44-template-projection.md](../../experimets/cards/P44-template-projection.md)
- [experimets/cards/P45-learned-reordering.md](../../experimets/cards/P45-learned-reordering.md)
- [experimets/cards/P46-pair-value.md](../../experimets/cards/P46-pair-value.md)
- [experimets/cards/P47-file-vector-context.md](../../experimets/cards/P47-file-vector-context.md)
- [experimets/cards/P48-weighting-curriculum.md](../../experimets/cards/P48-weighting-curriculum.md)
- [experimets/cards/P49-probe-warm-start.md](../../experimets/cards/P49-probe-warm-start.md)
- [experimets/cards/P50-graph-dependencies.md](../../experimets/cards/P50-graph-dependencies.md)
- [experimets/cards/P51-translation-context.md](../../experimets/cards/P51-translation-context.md)
- [experimets/cards/P52-semantic-annotation.md](../../experimets/cards/P52-semantic-annotation.md)
- [experimets/cards/P53-batch-atomicity.md](../../experimets/cards/P53-batch-atomicity.md)
- [experimets/cards/P54-phrase-composition.md](../../experimets/cards/P54-phrase-composition.md)
- [experimets/cards/P55-translation-laws.md](../../experimets/cards/P55-translation-laws.md)
- [experimets/cards/P56-verifier-context.md](../../experimets/cards/P56-verifier-context.md)
- [experimets/cards/P57-semantic-measurement.md](../../experimets/cards/P57-semantic-measurement.md)
- [experimets/cards/P58-feedback-learning.md](../../experimets/cards/P58-feedback-learning.md)
- [experimets/cards/P59-correction-visibility.md](../../experimets/cards/P59-correction-visibility.md)
- [experimets/cards/P60-signature-duplicate.md](../../experimets/cards/P60-signature-duplicate.md)
- [experimets/cards/P61-unknown-tail.md](../../experimets/cards/P61-unknown-tail.md)
- [experimets/cards/P62-signature-monotonicity.md](../../experimets/cards/P62-signature-monotonicity.md)
- [experimets/cards/P63-source-containment.md](../../experimets/cards/P63-source-containment.md)
- [experimets/cards/P64-tier-postfilter.md](../../experimets/cards/P64-tier-postfilter.md)
- [experimets/cards/P65-history-admission.md](../../experimets/cards/P65-history-admission.md)
- [experimets/cards/P66-heldout-contrast.md](../../experimets/cards/P66-heldout-contrast.md)
- [experimets/cards/P67-correction-identity.md](../../experimets/cards/P67-correction-identity.md)
- [experimets/cards/P68-pretrain-pipeline.md](../../experimets/cards/P68-pretrain-pipeline.md)
- [experimets/cards/P69-graphrag-context.md](../../experimets/cards/P69-graphrag-context.md)
- [experimets/cards/P70-wikitrace-context.md](../../experimets/cards/P70-wikitrace-context.md)
- [experimets/cards/P71-perplexity-alphabet.md](../../experimets/cards/P71-perplexity-alphabet.md)
- [experimets/cards/P72-project-conditioning.md](../../experimets/cards/P72-project-conditioning.md)
- [experimets/cards/P73-domain-conditioning.md](../../experimets/cards/P73-domain-conditioning.md)
- [experimets/cards/P74-memory-eviction.md](../../experimets/cards/P74-memory-eviction.md)
- [experimets/cards/P75-sampler-residual.md](../../experimets/cards/P75-sampler-residual.md)
- [experimets/cards/P76-utf8-boundary.md](../../experimets/cards/P76-utf8-boundary.md)
- [experimets/cards/P77-rare-class-support.md](../../experimets/cards/P77-rare-class-support.md)
- [experimets/cards/P78-loss-plateau.md](../../experimets/cards/P78-loss-plateau.md)
- [experimets/cards/P79-grammar-leakage.md](../../experimets/cards/P79-grammar-leakage.md)
- [experimets/cards/P80-marker-denominator.md](../../experimets/cards/P80-marker-denominator.md)
- [experimets/cards/P81-input-conditioning.md](../../experimets/cards/P81-input-conditioning.md)
- [experimets/cards/P82-file-split.md](../../experimets/cards/P82-file-split.md)
- [experimets/cards/P83-expression-body.md](../../experimets/cards/P83-expression-body.md)
- [experimets/cards/P84-lambda-family.md](../../experimets/cards/P84-lambda-family.md)
- [experimets/cards/P85-composite-obligations.md](../../experimets/cards/P85-composite-obligations.md)
- [experimets/cards/P86-fractional-response-mask.md](../../experimets/cards/P86-fractional-response-mask.md)
- [experimets/cards/P87-mixed-objectives.md](../../experimets/cards/P87-mixed-objectives.md)
- [experimets/cards/P88-unit-context-consumption.md](../../experimets/cards/P88-unit-context-consumption.md)
- [experimets/cards/P89-density-stop.md](../../experimets/cards/P89-density-stop.md)
- [experimets/cards/P90-tagged-search.md](../../experimets/cards/P90-tagged-search.md)
- [experimets/cards/P91-method-pair-split.md](../../experimets/cards/P91-method-pair-split.md)
- [experimets/cards/P92-dpo-memory-flow-bridge.md](../../experimets/cards/P92-dpo-memory-flow-bridge.md)
- [experimets/cards/P93-confidence-abstention.md](../../experimets/cards/P93-confidence-abstention.md)
- [experimets/cards/P94-typed-many-results.md](../../experimets/cards/P94-typed-many-results.md)
- [experimets/cards/P95-learned-flow-edge.md](../../experimets/cards/P95-learned-flow-edge.md)
- [experimets/cards/P96-async-orchestration.md](../../experimets/cards/P96-async-orchestration.md)
- [experimets/cards/P97-learned-graph-order.md](../../experimets/cards/P97-learned-graph-order.md)
- [experimets/cards/P98-tokenizer-capacity.md](../../experimets/cards/P98-tokenizer-capacity.md)
- [experimets/cards/P99-unknown-token-id.md](../../experimets/cards/P99-unknown-token-id.md)

Drift-пропуски адресного усиления: нет. Все writes этой стадии прочитаны обратно побайтово. Полная native/semantic аттестация каталога не выполнялась.

## Итоговое документное чтение и ограничения

Собственные операции затронули **183 уникальных Markdown-пути**: 178 файлов разрешения конфликтов, 126 файлов согласования одной инструкции чтения, восемь адресных карточек/договоров, общие входы 40/41/120 и этот review. Эти множества пересекаются; их размеры не суммируются. Список 178 и список 126 выше плюс общие входы и review дают 183 уникальных пути. В P104 удаление шести конфликтных маркеров выполнено внешним участником; работник добавил только согласование skill-entry поверх сохранённой чужой правки.

Документное чтение после правок:

- В пределах `docs/plans/replanning-possible-diffs` больше не найдено строк конфликтных маркеров; `rg` вернул код 1 без совпадений. Это проверка текста, не аттестация всех смысловых требований.
- Точной старой действующей фразы «Навыки и роли. Использовать пять точных навыков» в активных cards нет. 18 исторических probe-документов и прежние skill hashes не менялись.
- Q-таблица содержит 130 строк: 107 RESULT_RECORDED, 23 DESIGNED_NOT_RUN; первая незаписанная — Q108. Recorded по-прежнему не означает научный или технический PASS.
- Проверены 457 уникальных локальных ссылок из добавленных строк и этого review; отсутствующих файлов 0. Это разрешение путей, не проверка всех anchors, содержательной правильности ссылок или доступности внешних URL.
- Новые восемь адресных блоков прочитаны обратно; P56 разделяет архивный пересчёт и будущую native Verify-калибровку с отдельно назначаемыми calls. В 41 отрицательный raw_actual=false отделён от check_passed; технический PASS требует известного terminal exit 0. Метаконтроли finalizer не включают собственный PASS в свой required set.
- U+FFFD в P76 не новая порча кодировки: 16 в baseline и 16 после правок; это сохранённые UTF-8-примеры. Эти исторические символы не исправлялись автоматически.

Основной агент `/root` прочитал конфликтные решения и восемь адресных блоков. Его замечания о raw false/check_passed, process terminal, P56 Verify-cost и рекурсивном self-PASS исправлены и прочитаны обратно. Это родительская проверка, не дополнительный независимый субагент и не серия runtime-проходов. Работник вложенных агентов не создавал.

**Параллельное изменение источника.** Во время заключительного чтения внешний участник создал HEAD `735fa919d953e3eae636ac3c40ac5ff7c0c8d9da` поверх исходного `3cd2f26c`. Название коммита относится к P104; ранее наблюдались чужие изменения его Program.cs/Experiment.csproj, затем появился untracked `experiments-v2/RP01-MICRO/P10801/Program.cs`. Работник не выполнял эти операции и не проверял их runtime или remote push. Родитель `/root` проверил состав локального внешнего коммита командой `git show --format= --name-only 735fa919d953e3eae636ac3c40ac5ff7c0c8d9da` (exit 0): 192 пути, из них 183 Markdown-пути в `docs/plans/replanning-possible-diffs`, включая этот review, и 9 внешних P104/audit-файлов. Таким образом, включение 183 документных путей в этот локальный коммит подтверждено; remote push нами не проверен, Git mutations мы не выполняли. Поздние правки семи документов остаются в рабочем дереве. Авторство операций и набор сделанного здесь определяются указанными путями/дельтами, а не текущим числом git diff. Родитель также подтвердил живую Q-таблицу `experimets/cards/00-execution-index.md`: 130 строк, 107 RESULT_RECORDED, 23 DESIGNED_NOT_RUN, следующий Q108. Аудит новых продуктовых изменений 735fa919 не входит в этот пакет; исходные выводы привязаны к 3cd/f3, а не автоматически повышены до нового HEAD.

Открытая граница: полный свежий reload всех огромных governance/skills, прямо запрошенный пользователем, в этой сессии не выполнен. Прочитанные применимые фрагменты, индексы и обработка байтов не заменяют его. Требование сохранено в живых входах и не ослаблялось. Эта работа даёт применённый документный результат с перечисленными ограничениями, не новый runtime PASS, не разрешение на запуск и не утверждение о полной повторной научной сертификации каталога.

**Заключительное чтение родителем, полученный packet `/root`.** На HEAD `735fa919d953e3eae636ac3c40ac5ff7c0c8d9da` основной агент сообщил статическую проверку 726 Markdown в subtree: конфликтных маркеров 0, старой five-skill фразы в active cards 0; `git diff --check -- docs/plans/replanning-possible-diffs` завершился с exit 0. P56 и finalizer41 после замечаний прочитаны и подтверждены родителем. Это полученное свидетельство его документной проверки, не исполнение worker и не runtime-проверка. На том чтении оставались семь modified docs, а внешние untracked P10801 Program.cs/Run-Experiment.ps1/results не входят в наши действия; состояние общего workspace может продолжать меняться.
