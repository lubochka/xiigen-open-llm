# E51A-SOURCE-DISJOINT-LEARNING-01 — короткий причинный эксперимент «обучение или повтор знакомого кода?»

**Дата:** 16.10.2026. **Статус:** `DRAFT_SPEC / FIXTURE_CREATED / NOT_RUN / REVIEW_PENDING`. **Привязано к:** [11-E51-KNN-ORACLE-SPLIT...](11-E51-KNN-ORACLE-SPLIT-AND-POSITIVE-MECHANISMS-2026-10-16.md), [12-SIBLING-BRANCH...](12-SIBLING-BRANCH-MORPHOLOGY-DPO-CONTRACT-AND-E52-2026-10-16.md), [08-CAUSAL-DIAGNOSTICS](08-CAUSAL-DIAGNOSTICS.md). Оценка короткого *обучаемого состояния kNN Unit*, а не проверка прироста весов LoRA.

## 1. Человеческий смысл: какую ошибку ищем

В исходном E51A все 13 проверочных `V01–V13` используют переменные `CodeContext`, уже использованные в `T01–T27`. На архивном тесте это дало 100% успешных трасс, но нам нужно понять, **научился ли примерный структурный kNN переносить ранее полученную связь «код/закон → необходимые предикаты» на другой исходный код**, и имеет ли смысл возвращать этот механизм в архитектуру. [Источник E51A](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/Experiments/E51ACodeContextPipeline.cs#L843-L904); [`RunTrace`](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/Experiments/E51ACodeContextPipeline.cs#L253-L308).

**H1 (в пользу механизма):** новые *правильно размеченные* примеры, добавленные в kNN-память, повышают точность определения законов на *других* исходных кодовых фрагментах. **H0:** реального прироста на таком heldout нет; старый успешный результат мог опираться на повтор известных `CodeContext`, доменные ярлыки или заранее сохранённые `RequiredPredicates`. Отдельная гипотеза (пока НЕ проверяемая этим пробником): восстановление kNN позволит **RP01 Student** лучше отвечать в ordinary consumer.

## 2. Материал уже подготовлен

[15-E51A-SOURCE-DISJOINT-LEARNING-FIXTURE.json](15-E51A-SOURCE-DISJOINT-LEARNING-FIXTURE.json) — **8 новых TRAIN `N01–N08` + 8 blind HELDOUT `H01–H08`** с конкретными `prompt`, `codeContext`, `domain`, `family`, `requiredPredicates`, `forbiddenPredicates`. В каждой N/H-паре **одна и та же категория правила, но другой код**; ни один literal `codeContext` не совпадает между обучением и heldout. Это тест **переноса внутри знакомого семейства на новый источник**, ещё не unseen-law generalization. **Метки синтетические, предварительные**: после code/AST/semantic review (CODEX/Astra 6 UltraCode) зафиксировать gold/исключить invalid cases ДО любой scored inference. Для C# snippets, не заявленных как готовые standalone-проекты, не выдавать компиляторный PASS без реального компилятора. Не смешивать fixture со старыми DPO500.

Пример из fixture:
- N01: `Read(string? x)` возвращает пустую строку, если `x == null`; учебный набор `NULL_GUARD, RETURN`.
- H01: другой метод `Get(string? source)` возвращает `-1`, если source null; **тот же тип структурного закона, но другой C# код**, не находящийся в training.
- N04/H04: две *разные* последовательности `await`, учебная связка `AWAIT, CALL`.
- Случаи `H01–H08` не передавать ни в BPE fit, ни в `DomainClassifier.Train`, ни в kNN memory, даже при включённом флаге `heldout`. Ответы и `requiredPredicates` видит только измеритель.

## 3. Три условия на одном и том же blind HELDOUT (одна учебная переменная)

| Условие | Что имеет learner | Что меняем | Зачем |
|---|---|---|---|
| **A — BEFORE** | исходные 27 E51A T-записей | нет новых `N` | естественный baseline: что система уже знает |
| **B — AFTER CORRECT** | те же 27 T + 8 N | в память добавлены правильные source→predicate labels | эффект короткого обучения на новых примерах |
| **C — AFTER WRONG (negative control)** | **те же самые** 27 T + 8 N, те же тексты/домены, тот же индекс/токенизатор и k | только у 8 N `RequiredPredicates` переставлены по заранее фиксированной циклической перестановке `N01→N02→…→N08→N01` | доказать влияние выученной метки, а не просто расширения BPE/словаря/количества примеров |

У B и C одинаковые training input texts, tokenizer corpus, labels `ExpectedDomain` и параметры; **только принадлежность `RequiredPredicates` к example различается**. Если результаты B/C одинаковы и в hits нет N-примеров, обучение новым N ещё **не влияет на решение**. Если C не ухудшается при 4+ N top1 hits, это возможный сигнал, что метки/судья не проверяют реальное содержимое, а не PASS.

Вторая (диагностическая, НЕ основная) панель — старые `V01–V13`. Она показывает разрыв со знакомым кодом. Результаты на V не считать независимым доказательством обучения.

**Фиксация:** с одного pinned commit/source и одной копии старых T собрать A/B/C. Не изменять `JaccardDistance`, `DomainClassifier`, `k`, `LogicExtractor`, prompt compiler, thresholds, model, scoring и label schema. Если нынешний E51A требует приватного доступа к `AllExamples`/`RunTrace`, добавить **только test-only диагностический overload/runner** с общим production кодовым маршрутом; НЕ переписывать поиск в тесте и не выдавать новый toy-classifier за E51A. Готовой CLI-команды для этого опыта на текущей ветке не заявлено.

## 4. Что именно быстро реализовать

1. **Предрегистрация:** открыть коды из JSON, сверить 8/8, отсутствие точных дублей, family labels и все `H` gold с фактическим C#/AST/doman-law oracle. CODEX/Astra 6 UltraCode делает независимое смысловое утверждение, и gold SHA записывается ДО прогнозов. Ошибка в label ⇒ `INVALID_FIXTURE`, не исправлять ретроспективно.
2. **Мини runner:** предложенный путь `tools/Rp01E51ACausalProbe/` (не существует до реализации); считать JSON, выдать режимы `A,B,C`, запустить **оригинальную** компиляцию prompt, `DomainClassifier`, domain-scoped nearest (Jaccard), `best.RequiredPredicates` и evidence; не менять production-конфигурации.
3. **Training:** A uses `T01..T27`; B/C use `T01..T27+N01..N08`, без `H*`. Новые N берутся только из уже проверенного fixture. Обучение E51A — это **построение classifier/index/example memory**, не optimizer-step нейросети.
4. **Inference:** один и тот же fixed `H01..H08` в A/B/C; сохранять `predictedDomain`, `top1.ID`, `top1.score`, `producedPredicates`, `warnings`, время/выделения, hash source. Материалы `ExpectedHeldOutTraces()`, `gold` и `requiredPredicates` нельзя передавать в predictor, допускаются только в независимый evaluator.
5. **На случай нестабильности:** повторить с теми же immutable rows/порядком до 3 seed-controlled rebuilds. Если route deterministic, достаточно одного и записать это. Ни Windows GitHub CI, ни сотни модельных вызовов не нужны.
6. **Проверить реальную границу:** если B выигрывает, отдельно trace ordinary RP01 path: вызывает ли он этот trained Unit? Если нет, вердикт о RP01 — `KNN_CAN_LEARN_BUT_NOT_CONNECTED_TO_RP01`, **не** «починили RP01».

## 5. Счёт качества и заранее выбранная оценка

**Главная метрика:** `exact_law_pass/8`: predicted predicates должны включать ВСЕ обязательные и не включать forbidden; отдельно оценивать точность/полноту/F1 по независимо утверждённой полной разметке. Старый `PredicateOverlap` измеряет в основном **recall по required** ([реализация](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/Experiments/E51ACodeContextPipeline.cs#L487-L494)); он сам по себе не штрафует произвольные лишние не-forbidden предикаты, поэтому не считать его достаточной метрикой правильного закона.

**Наблюдение, поддерживающее H1 (пилотный сигнал, не статистический научный итог):** B улучшает `exact_law_pass` минимум на 2 из 8 случаев относительно A, минимум на 2 из 8 относительно C, и 0 новых `forbiddenPredicate` нарушений; как минимум на **половине исправившихся H** `top1.ID` должен принадлежать новым N. Требование ≈2/8 выбрано ДО запуска, но выборка слишком мала для обобщённых процентов. Отсутствие улучшения ⇒ `NO_GAIN_OBSERVED` или `INCONCLUSIVE`, а не «все kNN бесполезны»; нарушения и невозможность проверить gold ⇒ `INVALID_EVIDENCE`.

Отдельно сохранить `count(top1 in N)`, `macro_F1`, `domain accuracy`, `avg_latency_ms`, `managed_bytes`, `B−A`, `B−C`, old V13 diagnostic. **Абстиненции** считать покрытием отдельно; не выкидывать неудобные H. Не менять пороги после просмотра результатов.

## 6. Результат исполнения, который нужен CODEX

Создать один `experiment-result.json` и короткий `report.md` рядом с run, а не переписывать архивные E51A results. Каждый H row: `input_sha256`, `gold_sha256`, `train_set_sha256`, `mode`, `predicted_domain`, `top_hit_id`, `predicates`, `exact_law_pass`, `forbidden_violations`, `latency_ms`. Сводка: `source_commit`, `runner_commit`, `A/B/C scores`, `N-hit count`, `difference`, `failure reason`, `status`, `CODEX/Astra reviewer proof or NOT_RUN`.

**Мини-поручение исполнителю:** «Добавь только один диагностический `E51A` runner, используй исходный алгоритм и [fixture](15-E51A-SOURCE-DISJOINT-LEARNING-FIXTURE.json). Сначала утверди gold без запуска; затем A/B/C на одинаковых H8, где B отличается обученными N8, а C имеет те же тексты и ошибочные N-labels. Запиши каждое предсказание, hits и source SHA, сравни exact law pass/8 и forbidden violations. Не меняй DPO500/checkpoints/CI и не объявляй learned kNN обновлением LoRA. Заверши отдельным code/plan review и точным сообщением, что было измерено».

**Роли для любого последующего DPO (не используются в этом lookup-only эксперименте):** человеческий выбор учебного случая — **CODEX с Astra 6 UltraCode**; **обе** стороны `chosen/rejected` создаёт разрешённая внешняя **открытая китайская модель**; проверяет качество DPO-пары снова **CODEX с Astra 6 UltraCode**. Оценка *обученного Student* после optimizer-update отделена от проверки пары и не подменяется этим pilot.

**Барьеры доказательства:** fixture синтетический; новый runner, actual local execution, независимое Astra-рецензирование, измеренное улучшение и ordinary consumer trace **не выполнены в сессии составления документа**. Положительный результат подтверждал бы только локальный kNN обучающий механизм/семейство, не корневую причину всех отрицательных RP01 free answers.
