# N37 — E22 PredicateDatasetRouter: один новый урок невидим из-за порога `k`, пустая память ведёт себя противоположно, Train может заменять накопленное

**16.10.2026 · NEW SOURCE-VERIFIED CONDITIONAL DIFFERENCE · NO NATIVE TRAINING/PRODUCT JUDGE · DRAFT FOR INDEPENDENT REVIEW.**

**Scope:** только `AgentAlgorithms.PredicateDatasetRouter`, E22/E24 и возможный *будущий* перенос в online KNN/Unit. Это **не** generic `KnnLogicalUnit`, не Translation `ExecutorUnit` и **не** нейронная LoRA RP01. Сравнение с уже опубликованными документами выполнено адресно, а не пообъектно со всеми файлами библиотеки. Core `docs/plans/replanning-possible-diffs/**` READ_ONLY.

**Pin:** [core@793d7b755b23aac90445da3ac6661b1bff7e6672](https://github.com/lubochka/llm_mvp_core/tree/793d7b755b23aac90445da3ac6661b1bff7e6672) и [planning llm_plan3](https://github.com/lubochka/xiigen-planning/tree/llm_plan3). Все line ссылки ниже относятся к core pin.

## 1. Почему это отличается от ранее известных проблем

Ранее в каталоге [N14](14-KNN-IDENTITY-PREDICATE-SCOPE-REBUILD-GAPS.md) разобран другой `KnnLogicalUnit` с HNSW, ID collisions и absent domain scope; [N34](34-NEW-ONE-LESSON-DOUBLE-INSERT-UNDER-ALIAS-SIGNATURE-2026-10-16.md) — Translation memory top-k; [core read-only 65](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/65-e22-positive-late-run-and-label-transfer.md) установил положительный поздний E22 и разницу AST vs string labels. **Нынешняя находка другая:** hard-coded знаменатель для голосования `_k` при реальном количестве соседей `n < _k` в именно `PredicateDatasetRouter` — точная несовместимость с *incremental one-pair learning*. Дополнительные две гипотезы — replacement semantics и empty-query fallback — касаются того же класса, но не приписываются ему как доказанные regression в RP01.

### N37-A / P1 при online-adoption — majority cliff на первой паре

[PredicateDatasetRouter.cs L19–48](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/22.AgentAlgorithms/Context/PredicateDatasetRouter.cs#L19-L48):
```csharp
public void Train(IReadOnlyList<(int[] Predicates, bool[] Datasets)> examples) {
    _train.Clear();
    _train.AddRange(examples);
}
public bool[] Predict(int[] queryPredicates) {
    if (_train.Count == 0)
        return Enumerable.Repeat(true, _datasetCount).ToArray();
    var neighbours = _train
        .OrderBy(row => JaccardDistance(queryPredicates, row.Predicates))
        .Take(_k).ToArray();
    // ...
    var votes = neighbours.Count(row => index < row.Datasets.Length && row.Datasets[index]);
    result[index] = votes > _k / 2;
}
```

При `_k=3`, **одной учебной записи** и запросе с идентичным списком предикатов расстояние равно 0, `votes=1`, но `1 > 3/2` с целочисленным делением — `false`. Это истинно даже если учебная запись помечена всеми шестью целевыми dataset buckets. При **нуле** уроков код, наоборот, принудительно возвращает `true` для *всех* шести. Для двух положительных уроков `votes=2`, `2>1` — `true`. Итого число принятых уроков 0→1→2 может переводить тот же bucket **true→false→true**, не имея отношения к изменению предметного знания. При `k=5` нужен минимум 3 голоса, т.е. даже 2 позитивных примера не обеспечат ответ.

**C_SOURCE_LOGIC_CONFIRMED:** исходный код математически задаёт такой вывод. **UNKNOWN:** существуют ли реальные текущие обычные consumer вызовы этого класса в режиме `Train` на 1–2 кейсах; повлияло ли это на RP01 (прямой путь не доказан). **Нельзя** объявлять причиной RP01 `0/3` и `0/4`, потому что [RP01 recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md) измеряет *Student TextGenerator*, а этот router обучает дискретный набор меток.

### N37-B / P1 если online caller передаёт только delta — Train заменяет всю память

В тех же [строках L25–29](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/22.AgentAlgorithms/Context/PredicateDatasetRouter.cs#L25-L29) `Train` выполняет `_train.Clear()` перед `AddRange`. В эксперименте E22 [E22DatasetRouter.cs L16–39](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/35.ExperimentsV2/Experiments/E22DatasetRouter.cs#L16-L39) это *корректная batch/rebuild* семантика: router обучается полной train-частью один раз. **Условный consumer contract gap:** если будущая `ApplySingleLesson` ошибочно вызывает `Train([newPair])`, а не `Train(allAcceptedPairs)`, все старые label-neighbors исчезают и возникает N37-A. **Реальный caller, который делает именно это, пока не найден.**

### N37-C / P2 при потере предикатов — query без evidence выбирает уроки по порядку корпуса

[PredicateDatasetRouter.cs L84–92](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/22.AgentAlgorithms/Context/PredicateDatasetRouter.cs#L84-L92) вычисляет Jaccard. Для `queryPredicates=[]` и любого непустого training-set `intersection=0`, `union>0`, значит все distances `1.0`. [L35–38](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/22.AgentAlgorithms/Context/PredicateDatasetRouter.cs#L35-L38) берёт первые `k` по устойчивому source order. Класс не возвращает `NO_EVIDENCE` и не отказывается от классификации. **Контрпример:** 3 семейства C# BugFix и 3 семейства Tests; empty-query будет голосовать за то семейство, записи которого стоят первыми, при одинаковой нулевой информации о query. Аналогия с [N36 seedless GraphRAG](36-NEW-GRAPHRAG-SEEDLESS-PAGERANK-FALSE-EVIDENCE-2026-10-16.md) концептуальная, но **другой код и другой входной контекст**. Empty+empty имеет distance 0, поэтому отдельно проверить оба случая.

## 2. Сопоставление с успешным историческим обучением: не стирать позитив E22

В primary [E22 saved run 2026-05-07-002](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/E22-DATASET-ROUTER/results/run-2026-05-07-002.json) **macro-F1 0.4709 → 0.5646**, 450 примеров, train360/val90, `passed=true`. Это **положительное историческое измерение KNN-router** на автоматически размеченных данных, хотя его не следует переносить в RP01 neural или настоящий semantic user quality. В архиве старые результаты неоднородны — [read-only исследование 65](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/65-e22-positive-late-run-and-label-transfer.md) уже обнаружило, почему общий ярлык «E22 failed» неверен, и пояснило смену label function при переносе.

Ключевая причина, почему исторический PASS не опровергает N37-A: E22 обучает **360 объектов**, а `n>=k=3`; при cold one-pair online-state количество соседей `n=1`. Это **разные режимы одного алгоритма**. Механизм E22 — классификация dataset buckets по Jaccard предикатам, **не** эксперимент с матричным скорером, который потом перенесли в KNN; конкретный matrix→KNN provenance отдельно в [12](12-MATRIX-KNN-TRANSFER-PROVENANCE-AND-EVALUATION.md). Не заявлять E22 тем самым переносом.

Также исторический E22 после [DatasetLoader.cs](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/35.ExperimentsV2/Infrastructure/DatasetLoader.cs) читает `.cs`; TypeScript/Tests `F1=0` в archive не равен independent evidence этих доменов. Старые AST и новые string-based BugFix labels содержат разный oracle, как уже установлено в core read-only 65. В [E24AttributedRouter.cs L32–41, L57–92](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/35.ExperimentsV2/Experiments/E24AttributedRouter.cs#L32-L92) объект `router.Train` создаётся, но измеряется *другой* `AttributedSegmentRouter`, и `router.Predict` не вызывается. E24 не доказывает маршрутизацию E22 в обычном продукте.

## 3. Где это применимо и где нет

| Контекст | Уместность | Что проверять |
|---|---|---|
| C# / AST BugFix / code docs | P1 **если** E22-classifier передан в actual learning Unit | Истинная метка по AST/method scope; near-miss `namespace A.B`, комментарии; one-pair и retention |
| Orchestration PLAN/Flow, multi-label | P1 условно | Correct multi-label routing; не терять параллельные законные dataset buckets |
| Angular/TypeScript | P2/UNKNOWN | Нужны независимые TS-кейсы: исторический E22 читал C#, и TS F1=0 |
| Figma→HTML/CSS | UNPROVEN/не прямой механизм | Нет оснований переносить предикаты E17 на layout без отдельной domain representation |
| EN↔RU, POS, morphology, negation, sentence order | **NOT DIRECT** | Это другой KNN/Translation Unit; нельзя применять router-majority patch к переводу без нового data/model contract |
| RP01 neural Student+LoRA | **NOT DIRECT** | Не влияет на веса, градиент или словарь; только при доказанном интегрирующем маршруте |

## 4. Быстрая проверка

См. [37-MICRO-E22-ONE-LESSON-KNN-CAUSAL-PILOT-2026-10-16.md](37-MICRO-E22-ONE-LESSON-KNN-CAUSAL-PILOT-2026-10-16.md) и source-only [fixture](37-MICRO-E22-ONE-LESSON-KNN-FIXTURE.json), [proxy](37-MICRO-E22-ONE-LESSON-VOTE-PROXY.mjs). Сначала 0/1/2/3 accepted lessons и frozen exact queries; затем только условно независимые 12–24 domain heldout и обычный consumer. `effectiveK = min(_k, actualNeighbourCount)` — **кандидат на проверку**, не автоматическая правка кода. Его влияние на calibration/fp на иных классах нужно измерить. При `n=0` не считать `all=true` успешным обучением; у empty-query не подменять проблему «нет доказательства» уверенным label.

## 5. Governance, роли, оговорки

- Human selection и окончательный quality verdict делает **CODEX с Astra 6 UltraCode**. При реальном DPO обе финальные `chosen` и `rejected` порождает **разрешённая внешняя открытая китайская модель**; CODEX/Astra 6 UltraCode независимо проверяет exact pair и отдельно quality learned outputs. Настоящий `E22` micro требует **labelled examples, не нового DPO teacher call**, до отдельного разрешённого перехода к one-pair DPO.
- Никакого native .NET, независимого CODEX judgement, внешнего teacher или продуктивного online consumer-experiment в этой работе не запускалось. **SOURCE_CONFIRMED predicate-majority behaviour ≠ causal proof user-quality gain.**
- Применены процедуры `docs/skills/{architectural-decision-testing,phase-planning,plan-review,code-examination,experiment-design,learning-path-audit,human-readable-architecture-audit}-SKILL.md`. Отдельный независимый review требуется до `ACCEPT`.
