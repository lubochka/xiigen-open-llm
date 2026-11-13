# 106. Какие карточки усилены и что проверено — 30 октября 2026

<!-- EFFECTIVE-AUDIT-2026-11-01:START -->
## Действующая сверка — 1 ноября 2026

Core `790228c256168a336b01abae4387e28391048c0c`: **6 DONE / 2 PARTIAL / 1 OPEN / 1 CONDITIONAL** в очереди102. R2 принят; R6 не закрывает reference-only imports и legacy terminal без нового claim; R7 сохраняет constant-tie defect. Поэтому ни старый5/3/1/1, ни поздний8/0/1/1 не являются текущей приёмкой.

- [107 — доказательства, принятые переносы и точность P55–P81](107-transfer-and-experiment-audit-2026-11-01.md).
- [108 — конкретные owners, решения и закрывающие проверки](108-addressed-repair-plan-2026-11-01.md).
- [109 — адресные дополнения89 карточек и проверка сохранности](109-card-hardening-and-coverage-2026-11-01.md).

Все более ранние срезы ниже остаются историей на своих датах. Этот блок не назначает новый scientific NEXT, запуск или повтор обучения.
<!-- EFFECTIVE-AUDIT-2026-11-01:END -->

**Публикационная редакция:**80 Markdown изменений сохранены, дополнительно врепозитории размещаются маленькие воспроизводители и доказательства. Актуализированы четыре status notes P30/P31/P40/P53 и очередь102 на `aab5e5143f2516b92b42ce82606759bb6b4ef0b2`; прежний пакет остаётся историческим auditbase. Основные64card bodies ниже вставок остаются неизменными.

## Итог документного изменения

В P01–P64 добавлены **72 адресная проверка**: фактический owner/consumer, конкретные WRONG/CORRECT, положительный контроль и отрицательный контрпример. Каждый блок стоит сразу после H1 и имеет приоритет над расходящимися историческими обещаниями. Старое тело каждой карточки сохранено буквально; old design, бюджеты и raw history не переписаны. Научный NEXT и уже выданные work orders этим пакетом не меняются.

[104](104-transfer-and-experiment-audit-2026-10-30.md) содержит доказательства и текущие результаты; [105](105-addressed-repair-plan-2026-10-30.md) задаёт будущие исправления кода; [39](../experimets/39-transfer-evidence-contract-2026-10-30.md) содержит общий исполнимый контракт и prompt block. На проверенном SHA реализации C01–C10 не объявлены завершёнными. Документное усиление само по себе не исправляет production defect.

## Почему проверки распределены именно так

| Ошибка из аудита | Распространённый критерий |
|---|---|
| Явная policy работает, default всё ещё shared | P19/P39 проверяют default overload и оба направления mutable ownership |
| DatasetRouter исправлен, canonical RouteClassifier хранит чужой массив | P33/P34/P36/P53 требуют staged validation, copy и сохранение модели при отказе |
| Внутренний метод не равен public route | P01/P11/P32/P33/P38/P45/P58 проверяют actual consumer и вмешательство в state |
| Два чтения обновлённых весов в ProbeNN | P10/P11/P48/P49 отделяют корректный pre-step backward от другого исправного DPO owner |
| Общий runner обойдён отдельным script | P20/P21/P50/P52 проверяют каждый entry, output root и атомарный admission |
| Manifest/hash заменяет actual saved state | P17/P18/P24/P25/P42/P44/P50/P52/P59 проверяют typed bytes и нового reader |
| Незаметный повтор и неполный расход | P02/P15/P48/P49/P50 считают preparation, физические исполнения и probes отдельно |
| Модель не видит критерий разделения данных | P04/P14/P28/P43/P46/P48/P49 различают source split, input overlap, provenance и baselines |
| Векторная/адресная разница выдана за answer gain | P23/P30/P40/P41/P44/P46/P47/P51/P52 различают достигнутый фактор и целевую метрику |
| Новый context label недоступен runtime | P47/P51/P52/P54–P64 проверяют actual input, span/key parity, ambiguity и masking routes |

У остальных карточек критерий привязан к собственной постановке: token boundary, cache prefix/state, finite ingress, codec identity, actual k, empty-query policy и другим перечисленным ниже механизмам. Это не механическая вставка одной и той же общей фразы в64 файла.

## Полная карта изменений

| Карточка | Назначенные проверки |
|---|---|
| [P01](../experimets/cards/P01-trainable-recipient.md) | `P01-QUALIFIED-RANK-STATE` |
| [P02](../experimets/cards/P02-warm-base.md) | `P02-WARM-REFERENCE-AND-COST` |
| [P03](../experimets/cards/P03-historical-score-lineage.md) | `P03-GRADIENT-VS-OPTIMIZER-DELTA` |
| [P04](../experimets/cards/P04-preference-contrast.md) | `P04-MODEL-VISIBLE-PREFERENCE` |
| [P05](../experimets/cards/P05-response-length-objective.md) | `P05-OBJECTIVE-AND-PANEL-INTEGRITY` |
| [P06](../experimets/cards/P06-teaching-vector-adamw.md) | `P06-SIGNAL-SEMANTICS` |
| [P07](../experimets/cards/P07-materialized-token-contrast.md) | `P07-TOKEN-BOUNDARY-OWNERSHIP` |
| [P08](../experimets/cards/P08-answer-boundary.md) | `P08-SUPPORT-AND-ACTUAL-GENERATOR` |
| [P09](../experimets/cards/P09-cached-forward.md) | `P09-SAME-STATE-SAME-PREFIX` |
| [P10](../experimets/cards/P10-composed-gradient.md) | `P10-INDEPENDENT-PRESTEP-ORACLE` |
| [P11](../experimets/cards/P11-autograd-reachability.md) | `P11-REACHABILITY-AND-VERDICT` |
| [P12](../experimets/cards/P12-optimizer-boundaries.md) | `P12-FIRST-STEP-AND-ATOMIC-REFUSAL` |
| [P13](../experimets/cards/P13-finite-ingress.md) | `P13-FINITE-INGRESS-ATOMICITY` |
| [P14](../experimets/cards/P14-teacher-provenance.md) | `P14-PER-SIDE-PROVENANCE` |
| [P15](../experimets/cards/P15-retention-replay.md) | `P15-RETENTION-AND-EXPOSURE` |
| [P16](../experimets/cards/P16-semantic-order-roles.md) | `P16-ORDER-REPRESENTATION-CONTRAST` |
| [P17](../experimets/cards/P17-tokenizer-refit.md) | `P17-ENCODER-GENERATION-STATE` |
| [P18](../experimets/cards/P18-state-identity.md) | `P18-DIMENSION-AND-IDENTITY` |
| [P19](../experimets/cards/P19-mutable-output.md) | `P19-OWNERSHIP-BOTH-DIRECTIONS` |
| [P20](../experimets/cards/P20-hnsw-reachability.md) | `P20-OUTPUT-AND-REACHABILITY` |
| [P21](../experimets/cards/P21-hnsw-ingress-atomicity.md) | `P21-WRITER-AND-ADMISSION`; `P21-LOAD-TOPOLOGY-BOUNDARY` |
| [P22](../experimets/cards/P22-hnsw-rng-continuation.md) | `P22-RNG-CONTINUATION` |
| [P23](../experimets/cards/P23-retrieval-fusion.md) | `P23-FINAL-RETRIEVAL-CAUSALITY` |
| [P24](../experimets/cards/P24-replay-selection.md) | `P24-TYPED-CONTINUATION` |
| [P25](../experimets/cards/P25-cold-restore.md) | `P25-EXACT-STATE-API` |
| [P26](../experimets/cards/P26-structural-features.md) | `P26-UNICODE-COORDINATES` |
| [P27](../experimets/cards/P27-structural-codec.md) | `P27-CODEC-IDENTITY` |
| [P28](../experimets/cards/P28-morphology-family.md) | `P28-GIVEN-FAMILY-BASELINE` |
| [P29](../experimets/cards/P29-formula-memory.md) | `P29-TRAINED-RHS-READOUT` |
| [P30](../experimets/cards/P30-learned-fusion-alpha.md) | `P30-DERIVED-IDENTIFIABILITY` |
| [P31](../experimets/cards/P31-source-trust.md) | `P31-VERSIONED-EFFECTIVE-RESULT` |
| [P32](../experimets/cards/P32-skill-selection.md) | `P32-SELECTION-CONSUMER` |
| [P33](../experimets/cards/P33-domain-classifier.md) | `P33-PUBLIC-PATH`; `P33-MEMORY-OWNERSHIP` |
| [P34](../experimets/cards/P34-router-labels.md) | `P34-LABEL-SOURCE-AND-OWNER` |
| [P35](../experimets/cards/P35-actual-neighbor-votes.md) | `P35-ACTUAL-K-VOTE` |
| [P36](../experimets/cards/P36-incremental-router.md) | `P36-REPLACEMENT-CALLER` |
| [P37](../experimets/cards/P37-empty-router-query.md) | `P37-EMPTY-QUERY-ORDER` |
| [P38](../experimets/cards/P38-learned-segmentation.md) | `P38-SPAN-AND-ROUTE-EVIDENCE` |
| [P39](../experimets/cards/P39-oracle-output-separation.md) | `P39-MUTABLE-DEFAULT-REFUSAL` |
| [P40](../experimets/cards/P40-router-feedback.md) | `P40-SPLIT-DERIVED-VERDICT` |
| [P41](../experimets/cards/P41-stop-and-split.md) | `P41-THRESHOLD-IDENTIFIABILITY` |
| [P42](../experimets/cards/P42-checker-arbiter.md) | `P42-CHECKER-TO-TYPED-STATE` |
| [P43](../experimets/cards/P43-contextual-pos.md) | `P43-RULE-BASELINE-AND-POS-SCHEMA` |
| [P44](../experimets/cards/P44-template-projection.md) | `P44-ORDER-SENSITIVE-CONSUMER`; `P44-TYPED-MEMORY-BUNDLE` |
| [P45](../experimets/cards/P45-learned-reordering.md) | `P45-ACTUAL-REORDER-REACHABILITY` |
| [P46](../experimets/cards/P46-pair-value.md) | `P46-FACTOR-AND-MODEL-INPUT` |
| [P47](../experimets/cards/P47-file-vector-context.md) | `P47-CURRENT-KEY-READER` |
| [P48](../experimets/cards/P48-weighting-curriculum.md) | `P48-ONE-PRESTEP-GRADIENT`; `P48-WEIGHT-CONTRAST-AND-COST` |
| [P49](../experimets/cards/P49-probe-warm-start.md) | `P49-OVERLAP-AND-RETENTION`; `P49-WARM-COST-AND-GRADIENT-GATE` |
| [P50](../experimets/cards/P50-graph-dependencies.md) | `P50-SINGLE-USE-ENTRY`; `P50-STATE-AND-PHYSICAL-COST` |
| [P51](../experimets/cards/P51-translation-context.md) | `P51-TOPIC-KEY-END-TO-END` |
| [P52](../experimets/cards/P52-semantic-annotation.md) | `P52-ADDRESS-VS-TRANSLATION`; `P52-CHECKPOINT-BYTES`; `P52-SINGLE-USE-ENTRY` |
| [P53](../experimets/cards/P53-batch-atomicity.md) | `P53-BATCH-COMMIT-ATOMICITY` |
| [P54](../experimets/cards/P54-phrase-composition.md) | `P54-PHRASE-SPAN-COVERAGE` |
| [P55](../experimets/cards/P55-translation-laws.md) | `P55-POLARITY-SCHEMA-PARITY` |
| [P56](../experimets/cards/P56-verifier-context.md) | `P56-WHOLE-TOKEN-SCOPE` |
| [P57](../experimets/cards/P57-semantic-measurement.md) | `P57-SCORER-INDEPENDENCE` |
| [P58](../experimets/cards/P58-feedback-learning.md) | `P58-FEEDBACK-TO-REAL-LESSON` |
| [P59](../experimets/cards/P59-correction-visibility.md) | `P59-CORRECTION-AFTER-COLD-RESTORE` |
| [P60](../experimets/cards/P60-signature-duplicate.md) | `P60-ALIAS-DEDUPE-BOUNDARY` |
| [P61](../experimets/cards/P61-unknown-tail.md) | `P61-UNKNOWN-TAIL-KEY-PARITY` |
| [P62](../experimets/cards/P62-signature-monotonicity.md) | `P62-TAG-GUARD-MONOTONICITY` |
| [P63](../experimets/cards/P63-source-containment.md) | `P63-ACTUAL-TOKENS-AND-CONTAINMENT` |
| [P64](../experimets/cards/P64-tier-postfilter.md) | `P64-POSTFILTER-CASCADE` |

## Какие статусы уточнены

Для прочитанных исполненных PRIMARY notes связывают текущий результат с прежним design: P23/P24/P25/P28/P41/P42/P43/P44/P45/P46/P47/P48/P49/P50/P51/P52. Описания сообщают узкую подтверждённую способность и оставшийся вопрос. Например, P25 имеет19/24accuracy и24/24parity, P50 сохраняет2/4→4/4 при открытых state/cost/runner, P52 остаётсяKEY_SHAPE_ONLY.

При публикации дополнительно актуализированы P30/P31/P40 по новым evaluators/resolver и P53 по появившемуся report/metrics; добавлена отрицательная tie-проверка в существующийP30 check. Число named checks остаётся72.

Для P01–P19 и остальных карточек вне этой выборки существование/отсутствие нового научного результата не выводится из добавления проверки. Там current note относится к проверенному коду, историческому срезу либо явно говорит, что статус результата не устанавливался. P65–P125 не редактировались и не объявлены проверенными. Полный каталог possibleDifs не сертифицирован этим ограниченным аудитом.

## Общие документы и второй репозиторий

В core обновлены README очереди переноса, текущая таблица102, историческая граница103, typed contract101, experiment README, execution34, fixture35 и shared starts33. Добавлены104/105/106 и39. Снята ссылка102 на отсутствовавший файл `101-late-transfer-and-experiment-audit-2026-10-29.md`; действительный101 — договор другого содержания.

В xiigen-planning добавлен мост `2026-10-30-TRANSFER-EXECUTION-AND-EXPERIMENT-VALIDITY-AUDIT.md`, он указан вREADME. N29 дополнен подтверждённой KEY_SHAPE_ONLY границей P52; N33 — ограничениями причинного вывода по weighted runs и P48/P49. Исходные гипотезы и отрицательные результаты сохранены.

## Как принять сам документный пакет

Пакет содержит full overlays для80 Markdown файлов:76 вcore,4 вplanning, и отдельные unified patches для двух репозиториев. Первоначальный пакет был подготовлен как patch; публикационная редакция согласована с coreaab5e514/planning9b58737d и сохраняет новые remote changes. Commit этого файла является публикацией соответствующей редакции; parent/source SHA и проверки находятся в папке аудита. Исходные findings не расширяются автоматически на чужие новые изменения.

Внешний manifest пакета фиксирует original/new SHA256 и тип add/modify. Для карточек дополнительно сохранён признак буквальной сохранности прежнего тела и список72check IDs. Patch проверяется `git apply --check` на исходных bytes и затем побайтным сравнением результата с overlays. Это проверка применимости документации, не product test.

Устойчивые source URLs в104 указывают на exact SHA. Runtime evidence содержит source manifest и16observations; пересчёт raw содержит собственный JSON и скрипт. Приложенные probes воспроизводят наблюдения текущего дефектного/исправного кода и не предназначены как «все тесты должны оставаться зелёными после исправлений»: ожидаемые defective observations надо превратить в invariant regression assertions при реализации.
