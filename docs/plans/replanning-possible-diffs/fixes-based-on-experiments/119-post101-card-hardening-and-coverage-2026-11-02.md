# Адресное усиление карточек после P101 — 2 ноября 2026

<!-- POST102-AUDIT-2026-11-02:START -->
## Граница этого исторического среза

Этот документ сохраняет результаты аудита после P101. Позднее опубликованы P102, MethodKnn distance tie fix и Q127–Q130; их новая проверка и действующие статусы — [120](120-post102-transfer-and-executor-audit-2026-11-02.md), уточнённый остаток — [121](121-post102-addressed-repair-plan-2026-11-02.md). Здесь не пересчитываются задним числом прежние101/25/126. Текущая очередь103recorded/27designed из130, NEXTQ104; ниже сохранены исходные доказательства и план своей даты.
<!-- POST102-AUDIT-2026-11-02:END -->


## Фактически изменённая область

В этом пакете изменены **42 экспериментальные карточки**, включая девять P93–P101 и **33 других карточки** с применимой аналогичной границей. Каждый файл содержит один новый блок `POST101-AUDIT-2026-11-02`, конкретный check ID, WRONG/CORRECT и negative/positive приёмку. Старое тело карточки сохраняется побайтно. Это число изменённых документов, не число независимых прогонов, не аттестация всех271 карточки и не количество выполненных продуктовых ремонтов.

Также уточнены четыре общих договора переноса02/03/04/90, общий экспериментальный договор39, девять строк индекса и три входа продолжения. Актуальные 117/118 связаны с README, registry, required-changes, прежним планом114 и planning-сводкой. Исправлены два текущих словесных расчёта40%→20%; исторический oracle-output сохранён.

## Почему требования распространяются именно сюда

| Доказанный случай | Семейство аналогичных ошибок | Изменённые P-карточки |
|---|---|---|
| P101:2/4 скрывает gained/lost; copied/native parity различается | Предметная метрика, source of update, restored state, независимый gold | P01/02/03/04/12/15/18/57/66/101/102/105/106/109 |
| P89 mixed-scale; Wiki Final=mean | Одинаковые шкалы, временная точка, producer→actual gate | P03/06/57/89/101/105 |
| P99 Decode и P101 ASCII-кандидат | IDs, stop reason, byte-prefix/terminal, ordinary vs detailed | P08/09/75/76/99/100/101/102 |
| RF01 поздний typed failure; HNSW admission | Публикация состояния, late failure и continuation | P18/20/21/53/83 |
| RF06 eligibility послеcap; RF05a exact minority | Полный admission, logical identity, реальный caller, tie/permutation | P30/35/37/90/91/92/100/104/107 |
| P94–P97 intermediate answer ≠ executed output | Typed множество, edge, sink, actual async, simple baseline | P57/66/92/94/95/96/97/103/106/108 |
| P93 all-correct/no PAV; P98 одинаковая фактическая ёмкость | Проверка возможности сравнения и честный producer outcome | P90/93/98 |

Классы не являются разбиением: одна карточка может иметь несколько применимых рисков. Номера и старые budgets не менялись. Общий typed ledger распространяет lesson из вспомогательного BPETrain P94–P97 без вставки одинакового длинного текста во все271 карточки.

## Полная карта фактических правок

| Карточка | Изменённый владелец | Названная проверка |
|---|---|---|
| P01 | [P01-trainable-recipient.md](../experimets/cards/P01-trainable-recipient.md) | `P01-NATIVE-RECIPIENT-AND-PARITY` |
| P02 | [P02-warm-base.md](../experimets/cards/P02-warm-base.md) | `P02-ACTUAL-WARM-STATE` |
| P03 | [P03-historical-score-lineage.md](../experimets/cards/P03-historical-score-lineage.md) | `P03-METRIC-TIME-AND-SCALE` |
| P04 | [P04-preference-contrast.md](../experimets/cards/P04-preference-contrast.md) | `P04-GAIN-LOSS-BY-ROW` |
| P06 | [P06-teaching-vector-adamw.md](../experimets/cards/P06-teaching-vector-adamw.md) | `P06-ROWWISE-UNSCALED-LOSS` |
| P08 | [P08-answer-boundary.md](../experimets/cards/P08-answer-boundary.md) | `P08-STOP-ID-DECODE-BOUNDARY` |
| P09 | [P09-cached-forward.md](../experimets/cards/P09-cached-forward.md) | `P09-ORDINARY-AND-CACHED-ID-TRACE` |
| P12 | [P12-optimizer-boundaries.md](../experimets/cards/P12-optimizer-boundaries.md) | `P12-ENDPOINT-NOT-STEP-CAUSALITY` |
| P15 | [P15-retention-replay.md](../experimets/cards/P15-retention-replay.md) | `P15-RETENTION-BEFORE-RESTORE` |
| P18 | [P18-state-identity.md](../experimets/cards/P18-state-identity.md) | `P18-PAYLOAD-AND-CONSUMED-STATE` |
| P20 | [P20-hnsw-reachability.md](../experimets/cards/P20-hnsw-reachability.md) | `P20-HNSW-ADMISSION-SEPARATE-FROM-ANSWERS` |
| P21 | [P21-hnsw-ingress-atomicity.md](../experimets/cards/P21-hnsw-ingress-atomicity.md) | `P21-HNSW-NO-PARTIAL-PUBLICATION` |
| P30 | [P30-learned-fusion-alpha.md](../experimets/cards/P30-learned-fusion-alpha.md) | `P30-TIE-NOT-INFORMATIVE-FACTOR` |
| P35 | [P35-actual-neighbor-votes.md](../experimets/cards/P35-actual-neighbor-votes.md) | `P35-FULL-ADMISSION-BEFORE-CAP` |
| P37 | [P37-empty-router-query.md](../experimets/cards/P37-empty-router-query.md) | `P37-EXACT-ROUTE-AND-UNKNOWN-ROUTE` |
| P53 | [P53-batch-atomicity.md](../experimets/cards/P53-batch-atomicity.md) | `P53-TYPED-LATE-FAILURE-ATOMICITY` |
| P57 | [P57-semantic-measurement.md](../experimets/cards/P57-semantic-measurement.md) | `P57-SEMANTIC-METRIC-CONSUMER` |
| P66 | [P66-heldout-contrast.md](../experimets/cards/P66-heldout-contrast.md) | `P66-HELDOUT-NOVELTY-AND-BASELINE` |
| P75 | [P75-sampler-residual.md](../experimets/cards/P75-sampler-residual.md) | `P75-SAMPLER-SUPPORT-AND-PROBABILITY` |
| P76 | [P76-utf8-boundary.md](../experimets/cards/P76-utf8-boundary.md) | `P76-UTF8-PREFIX-AND-TERMINAL` |
| P83 | [P83-expression-body.md](../experimets/cards/P83-expression-body.md) | `P83-MAIN-EXTRACTOR-VS-SCOPED-TARGET` |
| P89 | [P89-density-stop.md](../experimets/cards/P89-density-stop.md) | `P89-IMPLEMENTED-PRODUCER-AND-TWO-READERS` |
| P90 | [P90-tagged-search.md](../experimets/cards/P90-tagged-search.md) | `P90-CONFIDENCE-WARNING-AND-ACTUAL-UNIT` |
| P91 | [P91-method-pair-split.md](../experimets/cards/P91-method-pair-split.md) | `P91-PAIR-IDENTITY-AND-ACTUAL-SPLIT` |
| P92 | [P92-dpo-memory-flow-bridge.md](../experimets/cards/P92-dpo-memory-flow-bridge.md) | `P92-WRITER-READER-DOMAIN-IDENTITY` |
| P93 | [P93-confidence-abstention.md](../experimets/cards/P93-confidence-abstention.md) | `P93-EMPIRICAL-RATE-VS-CALIBRATION` |
| P94 | [P94-typed-many-results.md](../experimets/cards/P94-typed-many-results.md) | `P94-TYPED-SET-TO-EXECUTED-NODES` |
| P95 | [P95-learned-flow-edge.md](../experimets/cards/P95-learned-flow-edge.md) | `P95-FLOW-CORRECTION-TO-EXECUTION` |
| P96 | [P96-async-orchestration.md](../experimets/cards/P96-async-orchestration.md) | `P96-ACTUAL-ASYNC-AND-COST-BOUNDARY` |
| P97 | [P97-learned-graph-order.md](../experimets/cards/P97-learned-graph-order.md) | `P97-GRAPH-ORDER-TO-SINK` |
| P98 | [P98-tokenizer-capacity.md](../experimets/cards/P98-tokenizer-capacity.md) | `P98-ACHIEVED-CAPACITY-AND-CONSUMER` |
| P99 | [P99-unknown-token-id.md](../experimets/cards/P99-unknown-token-id.md) | `P99-REVISED-SCOPE-AND-LOSSLESS-DECODE` |
| P100 | [P100-topk-ties.md](../experimets/cards/P100-topk-ties.md) | `P100-TIE-SUPPORT-VS-LEARNED-PANEL` |
| P101 | [P101-learning-budget.md](../experimets/cards/P101-learning-budget.md) | `P101-ROW-REGRESSION-PARITY-AND-UTF8` |
| P102 | [P102-teacher-forced-decoding.md](../experimets/cards/P102-teacher-forced-decoding.md) | `P102-TEACHER-FORCING-VS-FREE-GENERATION` |
| P103 | [P103-stack-before-normalization.md](../experimets/cards/P103-stack-before-normalization.md) | `P103-ACTUAL-STACK-BEFORE-LOSSY-NORMALIZATION` |
| P104 | [P104-law-eligibility.md](../experimets/cards/P104-law-eligibility.md) | `P104-LAW-ELIGIBILITY-BEFORE-TOPK` |
| P105 | [P105-frozen-reference.md](../experimets/cards/P105-frozen-reference.md) | `P105-FROZEN-REFERENCE-AND-SAME-OBJECTIVE` |
| P106 | [P106-corrected-semantic-target.md](../experimets/cards/P106-corrected-semantic-target.md) | `P106-CORRECTION-AUTHORITY-AND-RETENTION` |
| P107 | [P107-lawful-neighbor-aggregation.md](../experimets/cards/P107-lawful-neighbor-aggregation.md) | `P107-UNIQUE-LAWFUL-NEIGHBORS` |
| P108 | [P108-metadata-carrier.md](../experimets/cards/P108-metadata-carrier.md) | `P108-METADATA-CARRIER-TO-ORDINARY-DECISION` |
| P109 | [P109-gold-validity.md](../experimets/cards/P109-gold-validity.md) | `P109-INDEPENDENT-GOLD-AND-PER-ROW-VERDICT` |

## Общие договоры и предотвращение повторения

- **02 state/fixtures:** наличие remote payload, фактический restore и consumed state имеют разные статусы; отсутствие локальной копии не означает потерю модели; разные owners имеют разные обязательные state components.
- **03 execution:** immutable RowId/gold, TRAIN/EXAM gained/lost, typed nested ledger, актуальные source/assembly/caller; fixed order/timing не доказывает причинность JIT.
- **04 evidence:** mean/last/final, measured/derived/unknown, scope replacement, objective/gradient/update parity; нельзя смешивать шкалы и расширять однотокенный control на весь Unicode.
- **90 review/publication:** reviewer проверяет actual ordinary consumer и область кандидата, документирует оставшиеся native gaps, читает итоговые bytes после публикации на expected branch head.
- **39 experiment transfer:** словарь и graph не заменяют предметную тему обучения; simple baseline и class/key novelty объявляются явно; raw-correction bound к исходному digest.

## Как проверять этот пакет

[experiments-v2/_audits/2026-11-02-post101-transfer-audit/validate_audit.py](../../../../experiments-v2/_audits/2026-11-02-post101-transfer-audit/validate_audit.py) проверяет ровно перечисленные изменения, один новый блок в каждой карточке, целостность сохранённого тела, Q-статусы и ссылки новых блоков. Source-manifest связывает численные выводы с исходными blob/SHA256; recheck_experiments.py пересчитывает P93–P101. Эти проверки не запускают модель и не доказывают runtime-приёмку production fixes.

Независимая сверка фактического пакета записывается в audit evidence отдельно от parent/self validation. Ошибка, найденная при review, должна получить конкретную правку и повторную проверку именно затронутой границы, а не вымышленный счёт циклов.
