# N28-H1 — Новый риск: обученный пример присутствует в HNSW, но недостижим из entry point

**Дата:** 2026-10-16. **Статус:** SOURCE_VERIFIED_MECHANISM / PYTHON_LEVEL0_SHADOW_COUNTEREXAMPLE / NATIVE_DOTNET_NOT_RUN / REAL_STUDENT_GAIN_NOT_MEASURED / INDEPENDENT_ASTRA_REVIEW_NOT_RUN.  
**Запрещённый для записи источник:** `llm_mvp_core/docs/plans/replanning-possible-diffs` — только чтение.  
**Снимок core:** `codex/dpo-implementation@d5c2409b24292d361593b8262b0c54997b15f539`; исторические `language_translate@f6032aff81f1f83de7c09cf9b9efe6b058dd12b9`, `DesignToCode@8f20ceb8a9bfbadab2ad309226c47dd76e714548`.

## Сначала — точная причинная граница

**RP01 DPO Student** был отдельным neural learner: случайная B0, DPO q/v LoRA; C#500 и ENRU500 изменили LoRA, но опубликованные свободные ответы остались 0/3→0/3 и 0/4→0/4. Нынешняя находка **НЕ объясняет эти 0/3 или 0/4**, пока не найден consumer, который вызывает соответствующий KNN при том же экзамене. Не смешивать весовой learning и example-memory learning.

**Отличие:** обычная программа, которая добавила `corrected` example в KNN, может иметь `Count=N+1`, успешно сохранить state и назвать Unit обученной, однако ANN не сможет извлечь этот пример. Проблема не в отсутствии примера, низком quality DPO, неверном LoRA gradient или дрейфе BPE: обратные связи графа удаляются в самом построении индекса.

## Первичный source-trace — доказано чтением C#

1. [HnswIndex.Add L49–95](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/06.VectorSearch/HnswIndex.cs#L49-L95): после добавления `newId` в `_vectors` и создания `_graph[newId]`, для каждого найденного `nb` выполняются `_graph[newId][l].Add(nb)` и `_graph[nb][l].Add(newId)`. Если степень `nb` превысила `maxM`, его список **заменяется** `SelectNeighbours(_vectors[nb], _graph[nb][l], maxM)`; обратного ремонта `newId -> nb`/поиска другого inbound нет.
2. [HnswIndex.Search L99–111, GreedySearch L166–185, SearchLayer L188–219](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/06.VectorSearch/HnswIndex.cs#L99-L219): обычный query стартует от `_entryPoint` и посещает только соседей по ориентированному содержимому `_graph[visited]`. Хранение в `_vectors` само по себе не делает точку посещаемой.
3. [HnswIndex.Save/Load L115–149](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/06.VectorSearch/HnswIndex.cs#L115-L149): сохраняется и восстанавливается построенная adjacency; простая перезагрузка не ремонтирует одностороннюю связность.
4. [KnnLogicalUnit.Train L136–153](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L136-L153): generic learned Unit действительно строит `HnswIndex(m:8,efConstruction:64,efSearch:32)`.
5. [TransformerEmbeddingKnnLogicalUnit.Train L114–136; Execute L181–207](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L114-L207): другой learned Unit использует `HnswIndex(m:8,efConstruction:32,efSearch:16)`, извлекает кандидатов и затем применяет свой reranker. Даже запрос `topK=_examples.Count` не находит недостижимый узел.
6. [BruteForceIndex.Search L20–30](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/06.VectorSearch/BruteForceIndex.cs#L20-L30) — исходный **точный** baseline для проверки ANN recall; семантическую правильность выбранного output он сам не оценивает.

**Ветки:** код `_graph[nb][l] = SelectNeighbours(...)` найден **и** в `language_translate`, **и** в `DesignToCode`. Это потенциальная **наследуемая масштабная слабость**, не обнаруженный RP01-introduced regression. Наличие исторического успешного KNN совместимо с ошибкой: она может быть редкой на ранних индексах/разных `m` и невидимой в исторических validation pools.

## Численный shadow-контрпример: не native, но логически различающий

Выполнена локальная **Python-реализация только level=0** того же порядка `SearchLayer → SelectNeighbours → add inbound → trim`, c `m=2, efConstruction=64, efSearch=16`, фиксированные первые 11 трехмерных Gaussian-векторов `random.Random(0)`. Это **не C# runtime** и **не производственный m=8**; фиксирование уровня 0 изолирует алгоритм `prune`, а не оценивает распределение случайных HNSW levels.

| Срез | Наблюдение в shadow |
|---|---|
| После вставки первых 10 точек | Все индексы 0..9 достижимы из entry 0 |
| Вставка `newId=10` с вектором `[-1.981533, 0.288244, -0.119123]` | Исходящие соседи `[8,4,5,3]`, **0 входящих** после trim |
| Тот же запрос = exact vector нового урока | Exhaustive nearest id **10** (cosine=1.0); графовый search id **8** (cosine≈0.3989) |
| Реальная метка/качество перевода/кода | **NOT_RUN**, не утверждается |

Это **существование контрпримера для level-0 механизма при m=2**, не доказательство частоты для `m=8`. Чтобы проверить, является ли механизм причиной ухудшения, нужны native parity + независимый предметный quality exam.

## Почему это не дубликат ранее опубликованных документов

- [N21-BPE drift](21-INDEPENDENT-BPE-RETRAIN-VOCAB-DRIFT-2026-10-16.md) — изменение embedding от переобучения словаря; здесь vectors **заморожены**.
- [N23 shape mismatch](23-MICRO-SHAPE-AND-SAMPLER-CAUSAL-PILOT.md) — разная размерность query/index; здесь все vectors одинаковой размерности.
- [N20 candidate set nonlocal ranking](20-LESSON-ADDITION-RERANKER-NONLOCAL-QUALITY-GAPS-2026-10-16.md) — изменение `ScoreCandidates` после ANN; здесь **correct candidate отсутствует до reranking**.
- [N16 index/model identity](16-LIVE-ENCODER-INDEX-VERSION-DRIFT.md) — смена энкодера; здесь **model и tokenizer SHA неизменны**.
- [N27 replay aperture](27-NEW-REPLAY-APERTURE-DPO-KNN-DIVERGENCE-2026-10-16.md) — отбор manifest до обучения; здесь урок **уже индексирован**.

По заголовкам и просмотренным материалам `possibleDifs` этот отдельный алгоритмический seam reverse-prune/reachability ранее не был выделен. Это утверждение об ограниченной **проверенной выборке документов**, не о полном текстовом отсутствии во всей библиотеке.

## Доменная применимость

| Домен/route | Статус риска | Нужная проверка |
|---|---|---|
| C#/SQL/AST/predicate и rule→KNN | **Высокий условный**, если consumer использует HNSW и кода структурных векторов хватает для nearest task | Held-out пары с отрицанием, target, rule family; exact vs ANN nearest |
| EN↔RU / HE / языковая морфология | **Условный**, только learned KNN (не lexical/exact dictionary) | Subject/verb agreement, direction, negation; не смешивать с deterministic translator |
| Design→Code | **Условный**, только когда реально вызывается learned HNSW unit; не доказано, что текущий DTC consumer это делает | Render-QA на одинаковых макетах при изоляции learned KNN |
| RP01 standalone DPO/LoRA Student | **NOT_APPLICABLE_TO_EXAM_AS_TRACED** | Сначала доказать вызов KNN в policy-generation; иначе не чинить ANN ради 0/3 |

## Короткий native фальсификатор

См. [N28-MICRO-HNSW](28-MICRO-HNSW-LESSON-REACHABILITY-EXPERIMENT-2026-10-16.md). Сначала gate **native reachability/recall**, далее gate **качество ordinary consumer**; при отсутствии m=8 расхождений на целевых корпусах статус `NO_EVIDENCE_FOR_THIS_CORPUS`, а не “успешно улучшили RP01”. Не подменять `Count`, наличие output в памяти, cosine и тестовый PASS настоящим улучшением.

**DPO-ответственность неизменна:** **CODEX с GPT-6 Astra 6 UltraCode выбирает учебный случай в роли человека**; **разрешённая внешняя открытая китайская модель создаёт ОБЕ стороны DPO (chosen и rejected)**; **CODEX с GPT-6 Astra 6 UltraCode независимо оценивает качество пары и ответа Student**. Ни эти вызовы, ни native Student eval в рамках данного документарного аудита **не совершались**.
