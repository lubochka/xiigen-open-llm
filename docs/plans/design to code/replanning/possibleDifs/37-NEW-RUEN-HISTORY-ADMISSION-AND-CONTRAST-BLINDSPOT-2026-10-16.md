# N37 — Meaning-changing RU→EN contrasts filtered as repeated lessons: an overlooked training-quality seam

**16.10.2026 · source-confirmed semantics, source-equivalent Python proxy executed; native .NET NOT_RUN; Student gain NOT_MEASURED; independent CODEX/Astra NOT_RUN.** Source SHA: [core 793d7b755b23aac90445da3ac6661b1ff7e6672](https://github.com/lubochka/llm_mvp_core/tree/793d7b755b23aac90445da3ac6661b1ff7e6672). Target: planning llm_plan3/replanning/possibleDifs; core docs/plans/replanning-possible-diffs protected READ_ONLY.

## Для человека: смысловая поправка ошибочно названа повтором

Система обучалась переводить русский в английский. В корпусе уже есть «Этот важный файл уже доступен». Новый урок «Этот важный файл уже **не** доступен» нужен, чтобы не утрачивать отрицание. Однако текущий фильтр истории смотрит только на множество слов, не проверяет различие смысла и удаляет кандидат с Jaccard 5/6. Перестановка действий даёт те же 100% токенов и также удаляется. Количество «три→четыре» в другом фиксированном контексте — Jaccard 6/8 — тоже удаляется.

Не путать с известной потерей отрицания в KNN retrieval: **здесь полезный новый учебный сигнал может быть удалён ДО обучения Unit**, а затем на уровне стандартного RU_EN Batch029 **и минимальный heldout-контраст могут быть заблокированы как leakage**. Эти два gating path нельзя лечить снятием защиты blind exam. Не утверждается, что они использовались в двух конкретных нейронных RP01 кампаниях.

## 1. Source→contract→consumer evidence

| ID | Живой исходник, закреплённый SHA | Подтверждается | Не подтверждается |
| --- | --- | --- | --- |
| H37-01 | [TranslationStructuredBatchBuilder L201–255](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1ff7e6672/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L201-L255) | Кандидаты с Jaccard по lowercase alphanumeric-set >=0.70 к ЛЮБОМУ history A-side исключены. Множество не сохраняет порядок и кратность, error class/rejected смысл не используется. | Частота ошибочного отсева на реальном содержимом набора. |
| H37-02 | [TranslationBatch029Runner L86–124](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1ff7e6672/27.TranslationDomain/TranslationBatch029Runner.cs#L86-L124) | Реальный caller передаёт history из corpusPairs, очищает новые train rows, затем проверяет heldout против allTrainPairs. | Достижимость этого caller в RP01 neural text route. |
| H37-03 | [FindSimilarityContamination L387–445](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1ff7e6672/27.TranslationDomain/TranslationBatch029Runner.cs#L387-L445) | Heldout с лексическим overlap >0.70 отвергается, также без порядка/полярности. Training cutoff >=0.70, heldout >0.70 — разные boundary. | Что независимый по источнику heldout вообще следует отвергать. |
| H37-04 | [Batch029Runner L126–160, L326–339](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1ff7e6672/27.TranslationDomain/TranslationBatch029Runner.cs#L126-L160) | Корпус превращается в ExactPair, текущая партия и supplemental в Structural. Исторический фильтр видит corpus A-side, но не добавленные supplemental и не дубли внутри самой batch; MergeTrainingPairs смотрит на ID. | Что приоритет supplemental является сознательным семантическим законом. |
| H37-05 | [TranslationMachine.TrainStructuredBatch L89–147](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1ff7e6672/27.TranslationDomain/TranslationMachine.cs#L89-L147), [Executor.TrainOnExample L404–422](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1ff7e6672/27.TranslationDomain/TranslationTrainedUnits.cs#L404-L422) | Sequential learned Unit mutation and last-write replacement of exact source key are in source. | Что last-write всегда ухудшает: поздний chosen может быть корректным исправлением. |
| H37-06 | [Executor.Execute L264–285](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1ff7e6672/27.TranslationDomain/TranslationTrainedUnits.cs#L264-L285) | ExactSourceMap берёт приоритет перед phrase/word/KNN, поэтому перезаписанный ответ может скрыть остальные learned mechanisms на конкретном входе. | Реальный процент скрытых попаданий и true English correctness. |

## 2. Контрпримеры и выполненная иллюстрация

Исполнен короткий независимый [Python source-equivalent proxy](37-ruen-history-gate-source-proxy.py) с [receipt](37-ruen-history-gate-source-proxy-result.json). Он воспроизводит только Jaccard и условия двух guards; не исполняет .NET, не меняет веса и не оценивает перевод.

| A-side history → new | Jaccard | Train admission | Стандартный heldout guard | Gold relation |
| --- | ---: | --- | --- | --- |
| «Этот важный файл уже доступен» → «Этот важный файл уже не доступен» | 0.833333 | DROP | REJECT | Opposite polarity |
| «Сначала проверь доступ затем опубликуй событие» → «Сначала опубликуй событие затем проверь доступ» | 1.0 | DROP | REJECT | Different temporal sequence |
| «Клиент получил три важных письма вчера вечером» → «Клиент получил четыре важных письма вчера вечером» | 0.75 | DROP | REJECT | Different quantity |
| Та же фраза с другой пунктуацией и регистром | 1.0 | DROP | REJECT | Valid duplicate to exclude |
| Фраза о совсем иной теме | 0.0 | KEEP | ALLOW | Independent lexical content |

Это сильный контрпример **к идентичности семантики и lexical-Jaccard**, но не позитивный эксперимент по обучению. Для математического утверждения «все подобные пары будут удалены» нужны exact prerequisites (данная пара кандидат, соответствующий history corpus, route Batch029 и корректная русско-английская метка).

## 3. Три самостоятельные проверяемые гипотезы

**H37-A — loss of contrastive lesson (P1 if RU_EN route reached).** При наличии схожей записи history новая корректная law-changing train pair удаляется. Предсказание: переход от current A к semantic-delta-safe admission B меняет accepted pair IDs, downstream Unit state и final answer accuracy на хотя бы некоторых новых значимых family-disjoint cases. Альтернатива: новая пара противоречива/плохая, consumer её не использует или базовая модель уже знает различие. Нельзя выводить полезность из одного роста accepted count.

**H37-B — examiner-selection blindspot (P1 if Batch029 used for proof).** Стандартный validation error в Batch029 не позволяет использовать *именно минимально изменённые примеры* даже там, где нужен научный контраст. Нельзя удалять heldout contamination guard: нужен отдельный heldout по source family, заранее замороженный и не использованный для выбора training. Изменение rejected heldout count не равно улучшению модели.

**H37-C — asymmetric provenance and overwrite (P1 conditional).** Нынешний Filter применяется только к истории corpus, а batch/supplemental не нормализуются друг относительно друга по смысловому ключу; при повторном normalized A-side ExactSourceMap использует last writer. Predicted: разная очередность допустимых supplemental repairs может менять итоговый ответ при одинаковом множестве уроков. Обязательно проверить фактический invocation, выбранный source ID и истинный gold; если финальный перевод лучше, last-writer не является багом сам по себе. Вынести в независимый one-factor experiment.

## 4. Почему это не повторение чужих гипотез

- [N27 polarity KNN](27-NEW-SEQUENCE-SPLIT-CONFOUNDING-AND-NEGATION-KNN-GAPS-2026-10-16.md) исследует потерю отрицания при **сравнении источника с соседом после обучения**, не факт отсева пары до Training.
- [N29 rejected key](29-NEW-REJECTED-ANNOTATION-IN-TRAIN-ONLY-KNN-KEY-2026-10-16.md) исследует **конструирование KNN signature из rejected annotation**, не lexical-history admission.
- [N34 pretrain](34-NEW-PRETRAIN-SOURCE-AND-BATCHING-GAPS-2026-10-16.md) исследует другие **pretrain EOF/shard**-границы, не специализированный RU_EN Batch029/Jaccard.
- [Общий master N35](35-MASTER-RP01-DIVERGENCE-HISTORICAL-SUCCESSES-AND-CAUSAL-ORDER-2026-10-16.md) собрал neural/KNN/Unit риски; N37 — адресная добавленная producer→gate→trainer→consumer цепь. Полная новизна относительно всех 292+ постоянно изменяющихся файлов не удостоверена: обнаруженный прежний идентичный finding должен получить приоритет и ссылку, а N37 понизить до RECONFIRMATION.

## 5. Доменность и ограничения

**RU→EN:** прямой Batch029 consumer. Negation, actor, temporal order, quantity, modality и entities должны оставаться наблюдаемыми изменениями; не достаточно «похожие слова». Роль word order не одинакова у разных языков.

**EN→RU:** применима аналогичная постановка, однако не доказано использование здесь *того же* RU_EN Filter. Перед исправлением найти свой конкретный owner. Нейронный RP01 ENRU500 не читает RU_EN Batch029 по показанной цепи.

**C# / SQL / Design-to-Code / Orchestration:** принцип сравнения семантики важен (AST operators, NULL/UNKNOWN, ON-vs-WHERE, порядок операций), но нужно отдельно найти реальные gates; данную функцию нельзя приписывать им по аналогии.

**HE / RU morphological KNN:** отдельные root and morphology experiments (соседняя language_translate) не покрываются RUEN lexical admission.

## 6. Одно короткое разрешённое продолжение

[37-MICRO: native controlled A/B](37-MICRO-RUEN-CONTRASTIVE-REPAIR-CAUSAL-EXPERIMENT-2026-10-16.md): M0 воспроизвести Filter на настоящем .NET; M1 измерить кто реально получил новые уроки; M2 показать independent blind semantic accuracy на обычном RU_EN Flow с одинаковым состоянием и изолированным одним фактором. Начать с M0, остановить и понизить scope, если caller не достигнут. Обычный test PASS, matched source, changed Unit count и checkpoint save не считаются learned user skill. Не нужен массовый Windows Actions прогон для узкой source-local гипотезы.

**Вердикт текущей записи:** mechanism source-confirmed; pretraining contrastive loss **CONDITIONAL**; quality harm **UNKNOWN**; source-equivalent Python proxy **RUN**; .NET and CODEX/Astra **NOT_RUN**.
