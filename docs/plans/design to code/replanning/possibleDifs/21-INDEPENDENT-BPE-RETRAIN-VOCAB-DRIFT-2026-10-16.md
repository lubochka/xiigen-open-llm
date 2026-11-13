# 21 — Новый причинный механизм: переобучение BPE меняет память KNN без изменения старых меток

**16.10.2026 | Авторский независимый source-аудит | DRAFT / SOURCE-CONFIRMED + FIRST-MERGE PROXY / NATIVE_KNN_NOT_RUN / REAL_QUALITY_NOT_MEASURED.**

Core audit baseline: [llm_mvp_core@83590a462](https://github.com/lubochka/llm_mvp_core/tree/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5). При чтении актуального HEAD `0eccbdc3` проверено, что новый коммит добавил инструмент микроисследования, но не переписал исследуемые `BpeTokenizer.cs` и `KnnLogicalUnit.cs`. Исследование касается **`KnnLogicalUnit<TInput,TOutput>` из проекта 21**, НЕ `TransformerEmbeddingKnnLogicalUnit` (проект 18) и НЕ отдельного DPO/LoRA Student из RP01.

## Объяснение человеку

Когда систему учат ещё одному примеру, кажется, что прежние уроки должны остаться прежними. Однако нынешняя команда `KnnLogicalUnit.Train(...)` заново обучает BPE словарь **на полном списке входов**, а затем по номерам полученных BPE-токенов строит числовые признаки и HNSW. Даже перестановка строк прежней учебной выборки может поменять словарь. Поэтому модель поиска может «забыть» правильный старый ответ, хотя **ни один старый текст и ни одна его метка не изменились**. Это гипотеза о вероятном изменении результата, а не выполненное измерение пользовательской точности.

## N21-01 · P0 для generic KNN · Зависимость словаря от порядка уроков

**Кодовые факты (C):**
1. [`KnnLogicalUnit.Train`, строки 116–153](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L116-L153) конкатенирует входные примеры в порядке перечисления через `Environment.NewLine`, затем вызывает `tokenizer.Train(trainingText, vocabSize: 320)` и индексирует все examples.
2. [`BpeTokenizer.Train`, строки 42–68](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/01.Tokenizer/BpeTokenizer.cs#L42-L68) начинает с байтов 0..255 и присваивает последовательные IDs начиная с 256 парам, выбираемым по `CountPairs(chunks).MaxBy(kv => kv.Value)` без явного tie-break по паре.
3. [`CountPairs`, строки 526–538](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/01.Tokenizer/BpeTokenizer.cs#L526-L538) вставляет пары в словарь по порядку обхода чанков; [`ApplyMerges`, 568–573](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/01.Tokenizer/BpeTokenizer.cs#L568-L573) использует порядок выученных merge.
4. [`Vectorize`, строки 264–299](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L264-L299) кладёт count BPE token ID в slot `tokenId % (dimensions/2)`, а не в координату независимой от версии словаря семантической сущности.

**Демонстрация механизма, выполненная как внешняя арифметическая proxy, НЕ native C#:** два предтокенизированных ASCII слова `abab` и `cdcd` имеют две вхождения пары `ab` и две вхождения `cd`. При порядке `[abab, cdcd]` первый merge назначает token 256 паре байтов `(97,98)`. При порядке `[cdcd, abab]` token 256 получает `(99,100)`. Оба подсчёта проверены исполненным Node.js script, воспроизводящим только первую итерацию `CountPairs + MaxBy`. **Не проверялись** C# `vocabSize=320`, полные train-векторы, HNSW, ответы или качество. Для базового поведения Dictionary при tie требуется native readback, а не полагаться только на proxy.

**Потенциальный путь дефекта:** идентичные входы → другой BPE-ID → другой modulo-slot в двухслойном KNN-векторе → иной cosine/neighbour rank → другой stored output. Наличие пути C; наличие последнего изменения ответа/accuracy H.

## N21-02 · P0 conditional · Добавление постороннего урока переопределяет признаки старых уроков

`Train` не принимает ранее сохранённый tokenizer/index, а пересоздаёт их по расширенной выборке. Поэтому даже один новый законный пример способен изменить частоты merge, не затрагивая старые записи. В отличие от [N20 о dynamic BM25 max lexical](20-NEW-GAP-NONLOCAL-FUSION-AND-BM25-SCALING-2026-10-16.md), здесь меняется **сам encoder**, ещё ДО cosine/HNSW/fusion. В отличие от [N16 encoder/index version drift](16-LIVE-ENCODER-INDEX-VERSION-DRIFT.md), encoder и индекс могут быть заново построены согласованно, но прежнее представление знания всё равно перестаёт быть неизменным.

**Негативный контроль:** добавляем тот же distractor, но сохраняем старый frozen BPE, векторизуем новый пример только этим словарём, сравниваем exact cosine при одинаковых train outputs. Отдельно run с повторно обученным BPE; никакой дообученной LoRA в этом опыте нет.

## N21-03 · P2 conditional · Один учебный список на Windows/Linux может давать разные vocab

`Environment.NewLine` создаёт CRLF на Windows и LF на Linux, то есть `trainingText` не является каноническим байтовым корпусом на уровне `KnnLogicalUnit.Train`. `RegexSplitter` видит whitespace чанки (см. [RegexSplitter.cs](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/01.Tokenizer/RegexSplitter.cs#L10-L19)), и CRLF содержит дополнительный байт. Возможность отличающихся counts/merges подтверждается конструкцией; **реальное отличие 320-vocab на исходных корпусах и утрата качества не измерены**. Проверять в native BPE с явно заданными `\n` и `\r\n`; не называть CI/Windows корневой причиной без такого измерения.

## Что уже известно другим агентам и что здесь НЕ дублируется

- [11A — MICRO-KNN-ORDER](11A-MICRO-KNN-ORDER-CAUSAL-PILOT.md) уже верно предупреждает, что два запуска `Train` с различными `textSelector` меняют одновременно tokenizer и граф ANN. **Новое здесь**: при *тех же inputs/labels* перестановка rows либо добавление независимого примера способна изменить старые координаты — гипотеза о continual learning regression; отдельный контролируемый опыт.
- [14 — HNSW rebuild topology](14-KNN-IDENTITY-PREDICATE-SCOPE-REBUILD-GAPS.md) фиксирует random index. Мы первым тестом исключаем HNSW и сравниваем точный cosine.
- [16 — encoder/index-version drift](16-LIVE-ENCODER-INDEX-VERSION-DRIFT.md) рассматривает обновление LoRA между train/index и query. Здесь токенизатор переобучается в обоих плечах, даже при замороженных Transformer/LoRA (их нет).
- [20 — BM25 candidate normalization](20-NEW-GAP-NONLOCAL-FUSION-AND-BM25-SCALING-2026-10-16.md) относится к другому Unit, а не generic KNN.

## MICRO-BPE-RETRAIN-01 — короткая причинная проверка

**ExperimentId:** MICRO-BPE-RETRAIN-01. **Tier:** 1 (8–16 уникальных examples); затем Tier 2 только при сигнале. **Baseline A:** одинаковые train texts и labels, исходный порядок, tokenizer `Train(vocab320)`, exact cosine top1; **Treatment B:** идентичный набор с фиксированной перестановкой; **Treatment C:** исходные 8 + 1 предметно посторонний пример, полный `Train(vocab320)`; **Контроль D:** тот же 8+1, но tokenizer заморожен из A и полный exact-index перестроен в одном старом token-пространстве. Не менять семантические метки, `k`, vector dimensions, scoring/law, seed или экзамен.

**Входы:** 8–16 заранее отобранных CODE_Q/C# или безопасных action-order train examples и 8–16 **source-disjoint** heldout с перестановкой порядка действий, отрицанием, идентификатором субъекта и дистракторами. Слепой gold утверждает CODEX/Astra 6 UltraCode; без него результаты `DEV_ONLY`. Стратифицировать на семейства и не использовать heldout текст для tokenizer training. Label-shuffled и no-training контроли нужны, чтобы отличить полезность усвоенных меток от deterministic fallback.

**L0 — machine-mechanism (без обучения KNN):** зафиксировать artifact SHA, BPE merges/id table, `Encode(old_train_and_holdout)`, векторные координаты, `cosine@all` по A/B/C/D. Подтвердить либо опровергнуть `BPE_ORDER_CHANGED`. Использовать именно production `BpeTokenizer`, а не только proxy. Записать список равных частот/выбранный tie-break.

**L1 — measurable quality:** на одинаковых heldout сравнить A/B/C/D по `correct/16`, `false_SAFE`, paired top1 flip, law violations, confusion и p50 latency. Вначале **exact cosine** по уже готовым векторам; потом shadow реального `KnnLogicalUnit.Train → ExecuteAsync`; для HNSW сохранить и сопоставить actual hit IDs/ranks, не объяснять изменения BPE разным случайным графом.

**L2 — mechanism restore:** холодно восстановить тот же tokenizer/index из frozen A (если выбран API); сравнить ID, vector, hit и output; отдельно запустить перестроение B/C. Нельзя объявлять восстановление полным при одном совпадении количества записей.

**Критерий:** `MECHANISM_VERIFIED` при различиях token IDs/vectors в native коде с одинаковыми старыми текстами; `QUALITY_IMPACT` только при заранее измеренном падении `correct/16` либо росте критических ошибок B/C относительно A, **устранимом D при тех же старых labels**. Если токены меняются, а accuracy нет, записать `REPRESENTATION_DRIFT_ONLY`. Если результаты зависят только от ANN RNG, записать `ANN_CONFOUNDED`, а не победу гипотезы.

**Ресурсы:** один маленький native .NET console harness, четыре train/readback состояния и один heldout. Никаких 500 DPO, Windows GitHub Actions, длинного CI или внешней teacher-модели. Точные затраты и время записать только по реальному запуску.

## Роли и evidence boundary

**CODEX с Astra 6 UltraCode исполняет человеческий выбор учебной задачи/примера и отдельно оценивает качество DPO-пары.** Когда нужны новые DPO, **обе стороны (chosen и rejected) генерирует разрешённая внешняя открытая китайская модель**. Этот MICRO — контролируемое KNN-обучение на выбранных labels, а не создание новых DPO; поэтому Chinese teacher не требуется. Отдельно CODEX/Astra проверяет heldout rubric. Здесь не запускались CODEX/Astra, native .NET, модель, KNN или независимая рецензия; не переносить успех proxy на RP01 Student 0/4 без следа actual consumer.

**Follow-up:** после causal probe привязать `unit_id → learning_state_sha → serving_route → actual_execute → final_user_output`. Если RP01 обращается напрямую к `TextGenerator`, положительный эффект generic KNN в другом маршруте не объясняет его оценку.
