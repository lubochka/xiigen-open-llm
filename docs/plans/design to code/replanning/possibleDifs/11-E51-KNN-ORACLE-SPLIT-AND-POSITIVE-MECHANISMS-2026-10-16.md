# 11 — Новый исходный аудит E51A/E51B: сохранённый kNN, повтор train/test и подстановка oracle

**Дата:** 2026-10-16. **Статус:** SOURCE-VERIFIED / ARCHIVED-MEASUREMENT / CAUSAL-HYPOTHESIS. Это новый адресный проход по ветке `language_translate` и текущему исходному коду, не новая тренировка, не CODEX/Astra review и не подтверждённый production-learning gain.

**Pinned sources:** core `codex/dpo-implementation@83590a462b2f8aedc9d3ab4cb775193eb9c25ce5`; исторические results `language_translate@f6032aff81f1f83de7c09cf9b9efe6b058dd12b9`. Другие агенты уже разобрали E09/E34, DPO, B0, checkpoint и generic kNN; здесь новое исследование **E51A/E51B на уровне каждой train/validation-записи и actual source output**.

## 1. Наиболее сильный новый факт: все 13 проверочных E51A используют уже обучающий CodeContext

[Первичный список T01–T27 и V01–V13, E51ACodeContextPipeline.cs](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/Experiments/E51ACodeContextPipeline.cs#L840-L903).

Для каждой записи было извлечено третье поле конструктора `new("V..", prompt, CodeContext, ...)` и найдено **буквальное совпадение имени локальной переменной CodeContext** у T-строк в **той же функции** `AllExamples()`. Всего: 27 обучающих T, 13 проверочных V, **13/13 совпадений**. Это source-level инвентаризация, а не запущенная эвристика сходства.

| Validation | Тот же CodeContext в training |
|---|---|
| V01 `nullGuard` | T01, T03 |
| V02 `factoryValidate` | T09 |
| V03 `bugFixLoop` | T13 |
| V04 `readOnlyLoop` | T04 |
| V05 `regexSplitterWithLogging` | T05 |
| V06 `nullGuard` | T01, T03 |
| V07 `architecturePluginPlan` | T11 |
| V08 `encodeCallChain` | T10 |
| V09 `orchestrationSteps` | T08, T15 |
| V10 `equatableImplementation` | T14 |
| V11 `datasetEmptyBatchBug` | T18 |
| V12 `embeddingRefreshWorkflow` | T23 |
| V13 `removeEmbeddingCacheEntry` | T25 |

**Значение:** split сделан по ID/варианту запроса, **но не по идентичности исходного кода**. Архивный [E51A Tier 2 result от 11.05.2026](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/experiments-v2/E51A-CODE-CONTEXT-PIPELINE/results/run-2026-05-11-044429.md) фиксирует baseline predicate overlap 0.205, enhanced 1.000, domain accuracy 100%, trace success 100%. Эти числа **правдивы для проверенных 13 задач**, но не свидетельствуют об освоении неизвестного C#-кода. Уверенность в train/test CodeContext duplication: **C**. Уверенность в причине деградации RP01: **H** (не измерена).

## 2. Что именно эффективно работало: domain-scoped structural kNN, а НЕ DPO LoRA

[`E51ACodeContextPipeline.RunTrace`](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/Experiments/E51ACodeContextPipeline.cs#L253-L311):

1. Semantic compilation prompt → `DomainClassifier`.
2. Кандидаты `train.Where(t => t.ExpectedDomain == predictedDomain)` (fallback на все).
3. `JaccardDistance` структурных предикатов `CodeContext`, top-k.
4. `best = neighbors[0].Example`; **`produced = best.RequiredPredicates.Distinct()`** — метки требования копируются из сохранённого учебного примера, а не генерируются LoRA/Unit.
5. `PredicateOverlap` сравнивается с валидатором.

**Полезный механизм:** локальные структурные признаки, сохранённые размеченные варианты поведения и доменное ограничение соседей перед оценкой. **Неподтверждённый тезис:** «это научило Transformer самостоятельно писать ответ». Переносить следует именно retrieval/knowledge store/ranker и проверку доменных инвариантов, а не метрику trace success как free-generation accuracy.

**Новый риск N11-A (C+H):** если перейти к DPO-LoRA без сохранения memory of example→required-predicates, полезная предметная способность исчезает даже при математически правильном DPO. Восстановление: сохранить storage/rank и затем исследовать способность переносить его в trainable Unit с отдельным proof.

## 3. E51B: нейтральные слова «Flow E2E PASS» скрывают использование oracle-ответов

Архивный [E51B run 04.05.2026](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/experiments-v2/E51B-DYNAMIC-FLOW-MINI-E2E/results/run-2026-05-04-072046.md) сообщает 100% trace success: TE01–TE04, CH01–CH02.

В действующем [`RunHeldOutTrace`](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/Experiments/E51BDynamicFlowMiniE2E.cs#L211-L250) реально получены `neighbors[0].LocalSubject` и `SelectArbiters(subject)`, но `WorkStatement`, `SchemePlan`, `AppliedResult`, `FinalResult` берутся из `ExpectedHeldOutTraces()[record.Id]`, то есть из заранее известного ответа! `passed` проверяет subject, arbiter selection и verdict, **не независимую генерацию этих четырёх результатов**.

В [`RunChallengeTrace`](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/Experiments/E51BDynamicFlowMiniE2E.cs#L250-L299) `record.Id == "CH-01"` отдельно вставляет готовые learning-context и gap-пакеты; оставшийся challenge получает фиксированный rejection и `identity_consistency`. Это проверка конкретных сценариев, **не доказательство самостоятельно выученного открытия пробела на новых CH-ID**.

**Новый риск N11-B (C):** оценщик проверяет возвращённые поля, которые были подставлены из эталона, поэтому PASS не подтверждает обучаемый Flow. **Новый риск N11-C (C):** novel-case detector пользуется ID challenge вместо содержания; `NearestKnowledgeEdge` умножает уже выбранным `top-k` score на known-root-coverage, но это само по себе **не меняет подбор соседей и не создаёт решение abstain** [lines 325–336](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/Experiments/E51BDynamicFlowMiniE2E.cs#L325-L336).

**Важно:** это не обесценивает настоящий [`Nearest` с cosine similarity](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/Experiments/E51BDynamicFlowMiniE2E.cs#L300-L326). Умение выбирать локальную специализацию, знакомый law и арбитра могло реально работать; нужно честно отделить его от статического текста.

## 4. Потерянный положительный invariant: общий словарь координат

В E51B [`BuildRootVocab(teaching)`](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/Experiments/E51BDynamicFlowMiniE2E.cs#L300-L348) строится один словарь по training и используется для векторов каждого обучающего и проверочного prompt. Это необходимое условие сравнимости positional vector coordinates.

В [`TrainingVectorBuilder.Build(prompt)`](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/23.SemanticCompilerAdapter/TrainingVectorBuilder.cs#L14-L62) в отсутствие внешнего словаря root-vocab строится из **одного текущего prompt**, а [`DpoTrainer.VectorSignal`](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/11.DPO/DpoTrainer.cs#L1159-L1166) сводит признаки к числу ненулевых slots. Не утверждать, что все вызовы `TrainingVectorBuilder` используют плохой overload; здесь доказана **опасность разного контрактного режима**, а не глобальное отсутствие shared vocabulary.

**Новый риск N11-D (C+H):** при смешивании обучающих/поисковых режимов одно и то же значение координаты `root[0]` обозначает разные слова; cosine лишается предметного смысла. Проверка: менять лексические корни между двумя запросами и сравнивать `Build(prompt)` против `Build(prompt, frozenCorpusVocab)` при неизменном kNN и fixed labels.

## 5. Минимальные честные опыты

| Контраст | Один изменяемый фактор | Критерий, который опровергает прежнюю интерпретацию |
|---|---|---|
| E51A existing V vs unseen CodeContext | Разделить исходные snippets по exact SHA, затем family-disjoint | Если «100%» резко падает — это адаптация к знакомой структуре, не generalization; хранить обе метрики |
| E51A KNN vs no-KNN | Тот же split, prompt, law, budget, только retrieval | Если выигрыша нет на unseen families, не объяснять RP01 одной потерей retrieval |
| E51B oracle fields vs generated Flow fields | Запретить чтение `ExpectedHeldOutTraces` при составлении ответа | PASS по реальному содержанию, а не только LocalSubject |
| E51B CH-ID permutation | Переименовать CH01/CH02 и добавить новый ООD challenge | ID-based policy не должна выдавать прежний «learned» verdict |
| E51B shared vocab vs per-prompt vocab | Один corpus dictionary vs заново для каждой записи | Позиции root должны иметь единый смысл, rank должен быть устойчив к перестановке записей |

**Стоп:** не переписывать исторические run JSON, пороги и oracle; вести рядом новый blind receipt. Для начала достаточно saved artifacts и локальных расчётов; не нужны Windows GitHub марафоны.

## Границы и ревью

В этой работе изучены первичные source/архивы и вручную/программно сопоставлены `T` и `V` в E51A; компилятор, реальные модели и независимый CODEX/Astra не запускались. Результаты кодового чтения **C**, значения архивного отчёта **M**, объяснение деградации RP01 **H**. Формулировка «E51B не учился вообще» **не допускается**: source подтверждает конкретное достижение kNN/routing, но не learned generation конечных текстов.


## Готовая быстрая причинная проверка этой находки

Создан отдельный [E51A-SOURCE-DISJOINT-LEARNING-01](15-MICRO-E51A-SOURCE-DISJOINT-LEARNING-GAIN.md) с [8 учебными и 8 source-disjoint heldout случаями](15-E51A-SOURCE-DISJOINT-LEARNING-FIXTURE.json). Контроль A — исходные T27, B — T27 + восемь корректных N, C — те же T27 + те же N с циклически перемешанными **только N output-предикатами**. Одни и те же H8 оцениваются по предметным правилам и forbidden-предикатам. Положительный результат измерит эффект научения kNN-памяти на **другом коде**, но не будет доказательством LoRA/Student improvement, пока не будет ordinary consumer trace. **NOT_RUN; новые outcomes не заявлены.**
