# 86. Применимость новых выводов ко всему реестру переносов

27 октября 2026. Проверены ссылки текущего реестра: 62 ID, 56 разных файлов-владельцев. Во все56 владельцев добавлен адресный блок по категории ошибки; внешние владельцы из fixes-based-on-experimentsperiments также включены. Это полный охват применимости требований документации, **не62 независимых source/runtime audits**. Первичный source/raw аудит этой работы ограничен P02/P08/P09 и названными в84 существующими потребителями/тестами. Новые тесты и обучение не выполнялись.

Основания — [84](84-reporting-sampling-cache-transfer-audit-2026-10-27.md), исполнение — [85](85-reporting-sampling-cache-repair-plan-2026-10-27.md). Общий договор03 обновлён. Кодовые дефекты установлены адресно в84; аналогия не переносит статус дефекта/ремонта на все строки этой таблицы.

## Что изменено в каждой категории

* **M:** Для метрики DPO указывать единицу pair/step/panel, веса и фазу. Контрпример одного шага с margins [+1,-3]: доля положительных пар 0,5, признак положительного среднего 0; совпадение этих счётов на исходном опыте не делает их одной метрикой. Проверка вызывает полный writer с metric_definitions, а не только внутренний builder. Неизмеренное время SFT — null, не0; исторические receipts не переписывать.
* **F:** Разделить точные bytes prompt/response, позиции readout и фактическую функцию loss. Сохранённый навык может диагностироваться тем же reader, но response-position scoring не означает response-only SFT. Контрпример P08/P09: native full-sequence TrainSupervisedNextTokenStep выполнялся при ошибочной фразе exact response masks. Не менять исходную учебную цель ради согласования подписи; сохранять обучение, состояние и отдельный исправляющий receipt.
* **E:** Доказывать вызов реального получателя после выбора, а не наличие созданного constructor/bridge. Контрпример P08: A/B TextGenerator создан, но sampled ответы получены вне него. Для этой карточки записать выбранную модель/профиль, реально вызванный метод потребителя, данные ответа и получателя нового урока; альтернативный маршрут и restore проверяются отдельно. Подмена ответа ближайшей строкой/таблицей gold не проходит E2E даже при правильном выборе домена.
* **C:** Стоимость относится к точно названному окну. Контрпример P09: разность allocations четырёх arm windows, делённая на4, включает создание входов/cache/receipts и не является постоянной стоимостью одного Forward. Для этой оптимизации указать размер работы, warmup, что внутри/снаружи счётчиков и одинаковую обвязку A/B; не закреплять точные bytes по одному такому среднему. Проверять существующие tests и дополнять их learned fixture, не объявлять отсутствующими по одному отчёту.
* **T:** Экономия обучения токенизатора проверяется вместе с результатом выученных merges/IDs и исходным учебным объёмом. Не смешивать число представленных повторов с числом уникальных фрагментов или token positions; у каждого показателя свой знаменатель. Контроль использует выученный tokenizer и тот же реальный downstream reader, а не только constructor. Ресурсное окно включает заранее названные стадии, без приписывания writer allocations обучающему алгоритму.
* **Q:** Сохранять построчные исходы и независимый gold выбранного домена вместе с итоговой долей. Одинаковая общая точность может скрыть потерю прежнего урока и случайный выигрыш другой строки; изменение scorer/support само по себе не доказывает обучение. Для данной карточки подтверждать назначенный Train и чтение его результата реальным потребителем; существующую проверку усиливать сохранённым learned state вместо дублирования искусственного теста.

Ни одному ID не назначены дополнительные модельные вызовы этой таблицей. Требования ресурса/единицы измерения применяются к фактически заявленному показателю; отсутствующий показатель NOT_MEASURED. Старые эксперименты и их лимиты сохранены. Содержание полного training fixture и конкретный owner остаются в исходной карточке.

| ID | Действующий владелец | Добавленный контроль |
|---|---|---|
| FIX-KDP | [74-knn-domain-constructor-transfer.md](74-knn-domain-constructor-transfer.md) | E |
| E2E-KDR | [74-knn-domain-constructor-transfer.md](74-knn-domain-constructor-transfer.md) | E |
| FIX-MC | [21-sft-optimizer-continuity.md](21-sft-optimizer-continuity.md) | M |
| FIX-IL | [55-supervised-ingress-length.md](../fixes-based-on-experimentsperiments/55-supervised-ingress-length.md) | F |
| FIX-SC | [41-neural-learning-resource-deltas.md](../fixes-based-on-experimentsperiments/41-neural-learning-resource-deltas.md) | C |
| FIX-SEP | [62-supervised-framing-and-learning-readout.md](62-supervised-framing-and-learning-readout.md) | F |
| FIX-KHE | [26-method-knn-training-admission.md](26-method-knn-training-admission.md) | Q |
| FIX-KS | [35-method-knn-target-scope.md](35-method-knn-target-scope.md) | Q |
| FIX-FSR | [51-file-label-and-pos-reader.md](../fixes-based-on-experimentsperiments/51-file-label-and-pos-reader.md) | E |
| FIX-IM | [48-learned-route-body-identity.md](48-learned-route-body-identity.md) | E |
| FIX-KR | [49-router-learned-key-normalization.md](49-router-learned-key-normalization.md) | E |
| FIX-HCL | [53-hierarchical-local-history.md](53-hierarchical-local-history.md) | Q |
| FIX-SVD | [53-hierarchical-history-and-directed-svd.md](../fixes-based-on-experimentsperiments/53-hierarchical-history-and-directed-svd.md) | Q |
| FIX-PD | [22-dpo-valid-length.md](22-dpo-valid-length.md) | M |
| FIX-LC | [13-dpo-reference-cache.md](13-dpo-reference-cache.md) | M |
| FIX-TS | [28-dpo-tokenizer-snapshot.md](28-dpo-tokenizer-snapshot.md) | F |
| FIX-FBC | [54-frozen-base-check-scope.md](54-frozen-base-check-scope.md) | C |
| FIX-OSR | [58-optimizer-step-metadata-read.md](58-optimizer-step-metadata-read.md) | C |
| FIX-BWS | [60-backward-softmax-scratch.md](60-backward-softmax-scratch.md) | C |
| FIX-IGC | [61-initial-gradient-owned-copy.md](61-initial-gradient-owned-copy.md) | C |
| FIX-LFT | [55-frozen-linear-transpose-cache.md](55-frozen-linear-transpose-cache.md) | C |
| FIX-AGC | [41-grad-accum-copy.md](41-grad-accum-copy.md) | C |
| FIX-FBM | [42-frozen-broadcast-map.md](42-frozen-broadcast-map.md) | C |
| FIX-CS | [41-neural-learning-resource-deltas.md](../fixes-based-on-experimentsperiments/41-neural-learning-resource-deltas.md) | C |
| FIX-AC | [41-neural-learning-resource-deltas.md](../fixes-based-on-experimentsperiments/41-neural-learning-resource-deltas.md) | C |
| FIX-RRD | [44-rms-row-reduce.md](44-rms-row-reduce.md) | C |
| FIX-MB | [41-neural-learning-resource-deltas.md](../fixes-based-on-experimentsperiments/41-neural-learning-resource-deltas.md) | C |
| FIX-ATS | [45-attention-transpose-strides.md](45-attention-transpose-strides.md) | C |
| FIX-RPE | [41-neural-learning-resource-deltas.md](../fixes-based-on-experimentsperiments/41-neural-learning-resource-deltas.md) | C |
| FIX-SSR | [47-silu-sigmoid-reuse.md](47-silu-sigmoid-reuse.md) | C |
| FIX-SEXP | [41-neural-learning-resource-deltas.md](../fixes-based-on-experimentsperiments/41-neural-learning-resource-deltas.md) | C |
| FIX-GB | [29-matmul-right-gradient-locality.md](29-matmul-right-gradient-locality.md) | C |
| FIX-NH | [36-dpo-scored-row-head.md](36-dpo-scored-row-head.md) | M |
| FIX-RMS | [30-broadcast-coordinate-allocation.md](30-broadcast-coordinate-allocation.md) | C |
| FIX-BPE | [12-bpe-no-match-copy.md](12-bpe-no-match-copy.md) | T |
| FIX-BW | [31-bpe-weighted-chunks.md](31-bpe-weighted-chunks.md) | T |
| FIX-BR | [40-bpe-realized-merge-objective.md](40-bpe-realized-merge-objective.md) | T |
| FIX-KHC | [24-file-action-shared-parse.md](24-file-action-shared-parse.md) | C |
| FIX-AST | [38-structural-knn-sibling-cache.md](38-structural-knn-sibling-cache.md) | C |
| FIX-HNQ | [50-hnsw-training-query-norm.md](50-hnsw-training-query-norm.md) | C |
| FIX-PQ | [51-pos-query-bigram-reuse.md](51-pos-query-bigram-reuse.md) | C |
| FIX-JO | [46-training-json-options-reuse.md](46-training-json-options-reuse.md) | C |
| FIX-IDX | [52-translation-training-identity-index.md](52-translation-training-identity-index.md) | C |
| FIX-N60 | [11-translation-verifier-admission.md](11-translation-verifier-admission.md) | Q |
| FIX-VA | [43-verifier-apostrophe-learning.md](43-verifier-apostrophe-learning.md) | Q |
| FIX-NKU | [10-nku-do-while-label.md](10-nku-do-while-label.md) | Q |
| FIX-NG | [39-null-guard-training-label.md](39-null-guard-training-label.md) | Q |
| FIX-RET | [15-prompt-flow-retention.md](15-prompt-flow-retention.md) | Q |
| FIX-WC | [33-wiki-training-feature-reuse.md](33-wiki-training-feature-reuse.md) | C |
| FIX-WK | [32-wiki-feature-key-isolation.md](32-wiki-feature-key-isolation.md) | E |
| FIX-WT | [14-wiki-routing-taxonomy.md](14-wiki-routing-taxonomy.md) | E |
| FIX-E2E | [20-prompt-domain-e2e.md](20-prompt-domain-e2e.md) | E |
| FIX-N63C | [17-dpo-sft-correction-collision.md](17-dpo-sft-correction-collision.md) | M |
| TEST-COLLISION-REPAIR | [59-collision-repair-transfer.md](59-collision-repair-transfer.md) | M |
| FIX-N63R | [16-replay-domain-retention.md](16-replay-domain-retention.md) | M |
| FIX-MS | [23-sft-response-mask.md](23-sft-response-mask.md) | F |
| TEST-POS | [34-pos-bigram-learning-coverage.md](34-pos-bigram-learning-coverage.md) | Q |
| TEST-SK | [37-structural-knn-learning-coverage.md](37-structural-knn-learning-coverage.md) | Q |
| TEST-ORDER | [25-dpo-sft-order-learning-coverage.md](25-dpo-sft-order-learning-coverage.md) | M |
| TEST-TCTX | [18-contextual-dictionary-learning-coverage.md](18-contextual-dictionary-learning-coverage.md) | Q |
| TEST-TMEM | [19-translation-memory-coverage.md](19-translation-memory-coverage.md) | Q |
| TEST-E22 | [27-predicate-dataset-router-learning-coverage.md](27-predicate-dataset-router-learning-coverage.md) | E |

## Связанные рецепты и общий исполнитель

Исправлены учебные формулировки всех шести найденных P-карточек, одновременно ссылающихся на H64-RD и называющих его SFT exact response masks: P08, P09, P75, P81, P99, P100. Для H64-RD loss full-sequence; response positions относятся к readout. Это не blanket замена masked DPO или отдельного FIX-MS. Остальные совпадения фразы относятся к другим программам и этим аудитом не переопределяются.

P08/P09 получили актуальный результат и предупреждение против повторного запуска; P02 — незакрытые writer/aggregation пункты. P75/P100 получили требование явно различать прямой sampler и вызванный TextGenerator, P81 — отличать conditional margins от фактического ответа, P99 — отличать injected ID от естественного output. Программа36, общий договор34 и промпт продолжения несут то же правило producer/consumer и обучения. Старый текст постановки помечен историческим, не удалён.

Проверка покрытия — по множеству62 ID и фактическим файлам, а не по количеству одинаковых шаблонов. Адресные новые исходные выводы: P02 writer/агрегации, P08 sampler/consumer, P09 существующие tests/окно ресурсов. Данные остальных переносов заново не измерялись; их допуск не повышен.

## Проверка записанных изменений

Работник `/root/p08_transfer_audit` независимо прочитал все56 новых owner blocks, сопоставил62 ID и ссылки с реестром и картой: пропусков не нашёл. Затем проверил P08-разделы85 в документной границе: реальные constructor/delegate, native candidate B, восстановление S1 и бюджеты согласованы. Это адресное документное ревью, не запуск будущих тестов.

Родитель прочитал записанные файлы и разрешил467 локальных ссылок в новых документах/добавленных блоках: отсутствующих путей0. Git diff --check для выбранных отслеживаемых документов завершился exit0; это проверка whitespace, не сборка. Последнее замечание работника о P75/P100 снято явным direct prefill без Sample/Generate: проектный20Forward и24Sample согласованы и с карточками, и с общей таблицей36. Прежний проектный лимит12Forward не покрывал реальные генераторы; новые запуски этим не выполнены и не разрешены.

