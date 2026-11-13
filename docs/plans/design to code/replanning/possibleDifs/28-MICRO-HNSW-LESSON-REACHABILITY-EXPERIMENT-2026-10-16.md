# MICRO-N28-H1 — за один короткий цикл различить «урок не выучен» и «урок нельзя извлечь из HNSW»

**Статус на 16.10.2026:** EXECUTION_RECIPE_ONLY / LOCAL_PYTHON_LEVEL0_SHADOW_OBSERVED / NATIVE_CSHARP_NOT_RUN / ASTRA_INDEPENDENT_JUDGE_NOT_RUN.  
**Авторская гипотеза и доказательства:** [N28-H1](28-NEW-HNSW-REVERSE-PRUNE-AND-LOST-LESSON-REACHABILITY-2026-10-16.md).

## Цель и ограничение причинности

Сравнить **одни и те же сохранённые обучающие примеры с теми же векторами**, но два варианта извлечения: `HnswIndex` против точного `BruteForceIndex`. Если unit содержит законный исправляющий пример, но HNSW пропускает его и выбирает ошибочный, значит обнаружен **retrieval visibility defect**, а не недостаточный DPO/LoRA update. Вывод распространяется на тот HNSW consumer, который реально вызван; не на standalone RP01 free generation.

## Gate A — крошечная native проверка семантики графа, ~11 точек

**Место:** одно маленькое временное C# console/test harness, который напрямую вызывает `VectorSearch.HnswIndex`, `VectorSearch.BruteForceIndex`. Не менять production код, RP01 checkpoint, frozen B0, DPO500 и опубликованные показатели.

### Детерминированная изоляция level-0 (только временный test hook)

Чтобы исключить случайный уровень из `RandomLevel()`, предусмотреть **test-only** инъекцию `RandomLevel=0` через friend/internal seam или isolated копию чистого метода построения. Не объявлять «native bug reproduced», если сравнили только JS/Python-копию. Прошлый пример использует `m=2, efConstruction=64, efSearch=16`.

```json
{
  "fixtureId": "N28-HNSW-L0-11",
  "vectors": [
    [0.941715,-1.396578,-0.679714],
    [0.370504,-1.016349,-0.072120],
    [0.179196,-0.831099,-1.309037],
    [0.193888,0.993250,-0.646982],
    [-0.333668,1.645672,-0.558890],
    [-0.514157,2.404119,-1.531083],
    [0.796466,-2.003649,-0.596963],
    [1.503681,1.221436,-0.901120],
    [-0.453699,0.080233,-1.258103],
    [0.552220,2.227577,-1.355241],
    [-1.981533,0.288244,-0.119123]
  ],
  "entryPoint": 0,
  "newLessonId": 10,
  "query": [-1.981533,0.288244,-0.119123],
  "shadowOnlyExpected": {
    "lastNodeOutgoing": [8,4,5,3],
    "lastNodeIncoming": [],
    "hnswTop1": 8,
    "bruteForceTop1": 10
  }
}
```

Источник чисел — заранее исполненная Python level-0 реимплементация актуального порядка `SearchLayer/SelectNeighbours/Add`; это **не** native receipt. Поскольку `RandomLevel=0` — модифицированный экспериментальный режим, исходные цифры следует считать **ожидаемыми для проверки**, а не неизбежными результатами производственного HNSW.

**Снять в native:** `graphLevel0InDegree[10]`, `graphLevel0OutDegree[10]`, `reachableFromEntry[10]` BFS по исходящим, `nativeANNTop1Id,score`, `bruteTop1Id,score`, `vectorsSHA`, `m/efConstruction/efSearch`, snapshot до/после `Save+Load`. Проверять достижимость отдельным внутренним diagnostic reader, не использовать сам `Search` для определения graph connectivity. Если native результат отличается от shadow — **обновить гипотезу по native факту**, не подгонять harness.

### Контроль C1 — сохранение и точная модель

Та же точка `10` должна остаться в `Count` и serialized vectors. `BruteForceIndex.Search(query,1)` должен вернуть `10` с cosine~1.0. Если это не так, провален сам fixture или encoder, а не гипотеза reachability. После `HnswIndex.Save/Load` графовый ответ должен соответствовать до-restore, либо оформить отдельно restore defect.

### Контроль C2 — порядок и M

С тем же фиксированным набором выполнить **A:** `m=2, L0` (минимальный контрпример), **B:** `m=8` и настоящие RandomLevel/seed-capture без изменения векторов. Отдельно A-B по 3 разным insertion orders при стабильном model/tokenizer. Не считать улучшением A→B просто увеличение ресурса; измерить ANN recall и вернуть этим gate область применимости. При `m=8` отрицательное доказательство на этом маленьком fixture **не закрывает** проблему для larger memory.

## Gate B — небольшой настоящий learned Unit pilot, только при наличии consumer

- **Данные:** 12 TRAIN, 4 DEV, 16 blinded HELDOUT; не менее 2 domain laws × 2 роли/отрицания; кейсы source-disjoint и template-disjoint, отдельно 8 retention / 8 transfer. До старта записать hashes, domain owner, source provenance и grade rubric. Все learned labels и исправления находятся **только** в TRAIN.
- **Фиксация:** один learned Unit owner (`KnnLogicalUnit` или `TransformerEmbeddingKnnLogicalUnit` — не смешивать), один encoder SHA, один tokenizer SHA, те же 12 raw feature vectors, graph size, one frozen candidate memory и source-independent examiner. Не переобучать DPO/LoRA между arms.
- **Два arms:** A = обычный `HnswIndex`, B = точный `BruteForceIndex` поверх **тех же** frozen vectors/ids/outputs. Оба передают найденные candidates в **одинаковый** candidate-scorer, с одинаковой смесью weights и режимом `topK`, затем в одинаковый ordinary Unit consumer.
- **Два этапа:** (1) `retrieval recall@1/@k`, missing-positive lesson count, `entryReachable`, `incomingDegree`; (2) **пользовательский ответ** по domain law, `heldout correct/16`, `law violation`, `old-correct→new-wrong`, `abstain`, `wrong high-confidence`. Косинус и DPO margin — диагностики, НЕ quality KPI.
- **Роли:** CODEX **Astra 6 UltraCode** выбирает cases как human-role и проводит независимый blind quality-review ответов. При необходимости DPO-пар обе финальные стороны **создаёт разрешённая внешняя открытая китайская модель**, затем CODEX Astra отдельно оценивает. В этом pilot **создавать DPO не требуется**: не выдумывать Chinese provider/calls. 16 blind labels не передавать trainer.
- **Предварительная граница:** если хотя бы 2 из 16 source-disjoint heldout поменяли ошибочный ANN answer на правильный exact answer **из-за** ранее невидимого корректирующего lesson и без новых regressions на тех же 16, вердикт `PROVISIONAL_RETRIEVAL_CAUSAL_SIGNAL`, а не generalized learning proved. Если gap только в synthetic m2, но нет в native m8/ordinary consumer, `SHADOW_ONLY_NOT_RELEVANT_YET`. Если у wrong answers correct lesson отсутствует **даже в exact pool**, это quality-of-teaching/coverage, а не ANN bug.
- **Второй малый confidence run** с другим dataset group и ≥16 heldout только после provisional signal. Не засчитывать training repeats как независимые примеры.

## Почему одно изменение в каждом сравнении

A0→A1 проверяет **добавление одного правого lesson** без смены learned model (качество до/после обучения памяти); A1→B1 меняет **только способ retrieval на frozen memory**. Нельзя одновременно менять `m`, tokenizer, LoRA, reranker formula, k, labels или prompt; `m2` и `m8` — **отдельный diagnostic gate**, не эффект предложенной фиксации.

**Acceptance/passports:** `source_commit`, `consumer_call_chain`, `train/heldout_sha`, `encoder/tokenizer/index identity`, `insertion order`, `graph edges`, `result ids`, `latency`, `judge version`, `case-level verdict`, `counterexamples`, `reproduction command`, `native-run receipt`, `limitations`. Только native receiver + semantic quality может поднять статус до `QUALITY_VERIFIED_FOR_SCOPED_DOMAIN`.

## Где убирать проблему после подтверждения

Не делать немедленный «global HNSW rewrite». Экспериментальный кандидат: локально обеспечивать inbound reachability для нововставленного узла либо эффективный fallback/periodic exact-scan с threshold, затем проверить degree cap, качество recall, стоимость, Save/Load, deletion/restore, different insertion orders и graph integrity. Изменение `m` или `efSearch` может скрывать симптом, но не восстанавливает недостижимый из entry graph узел.

**Self-review checklist:** source refs проверены; прежние ветки подтверждают наследственность; ветки/learner не смешаны; shadow ≠ native; Test-only override не выдан за production; metrics показывают real consumer gain; CODEX/Chinese роли не подменены; `llm_mvp_core/docs/plans/replanning-possible-diffs` нетронут.
