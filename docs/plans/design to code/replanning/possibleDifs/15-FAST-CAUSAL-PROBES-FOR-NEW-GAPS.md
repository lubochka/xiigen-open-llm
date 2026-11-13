# 15 — Минимальные причинные эксперименты: как быстро доказать влияние найденных отличий на качество обучения

Дата: 16.10.2026. **Статус: READY_TO_IMPLEMENT / NOT_RUN.** Independent CODEX/Astra review и Student training NOT_PERFORMED.
Смежные материалы: [44 исходные DIF + micro-checks](12-MINIPROBES-DIF01-DIF44.md), [B0 cold start](12-RP01-MINI01-COLD-START-CAUSAL-EXPERIMENT.md), [KNN order](11A-MICRO-KNN-ORDER-CAUSAL-PILOT.md), [KNN top1](13-MICRO-KNN-CAUSAL-LEARNING-EXPERIMENT.md), [морфология соседней ветки](13-LANGUAGE-TRANSLATE-BRANCH-MORPHOLOGY-GAPS.md), [новые прямые дефекты KNN](14-KNN-IDENTITY-PREDICATE-SCOPE-REBUILD-GAPS.md).

## Ключевой критерий: не подгонять тест под систему

- `L0 mechanism` — отличие реально существует, вход дошёл до нужного кода.
- `L1 quality effect` — **одна переменная** изменила фактический ответ на **той же заранее определённой предметной панели**. Учитываем wrong/abstain/cost, а не только PASS, confidence или DPO-margin.
- `L2 learning generalization` — обучаемый state действительно изменился и появился прирост на **новых семействах/композициях**, которых не было в Train. Если меняли только тип запроса, декодер или checkpoint route, можно доказать влияние на **использование уже выученного**, но нельзя выдавать это за доказанное улучшение оптимизатора DPO/LoRA.

Не изменять frozen C#500/ENRU500 RP01, старые checkpoints и принятые оценки. Не запускать долгий GitHub Windows CI ради этой проверки. Два контраста и два отрицательных контроля лучше сотни очередных passing технических тестов.

## Первый опыт: MICRO-15.01 — потеря семантического предиката при типизации KNN metadata

**Гипотеза DIF-14.02:** при обучении KNN различает закон по `PredicateIds`, но на входе inference тот же закон теряется, когда тип объекта в `Metadata["predicate_ids"]` не `string[]`. Поэтому выученные различия могут не влиять на ответ.

**Доказательство механизма из кода:** [KnnLogicalUnit.Train](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L103-L163) обучает BPE и добавляет PredicateIds в вектор; [ExecuteAsync](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L216-L253) при `value is string[]` сохраняет, при `List<string>` — заменяет на пустой набор. [LogicalUnitRequest.Metadata](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/00.Core/LogicalContracts.cs#L61-L77) объявлена как словарь объектов без строгой схемы. Сам баг условный: он влияет на product, **только если** реальный caller передаёт другой тип.

**Train, один раз:** `KnnLogicalUnit<string,string>`, `vectorDimensions=64`, `k=2`. Два примера: `(row-A, alpha, LAW_A, output=A)` и `(row-B, beta, LAW_B, output=B)`. Тексты/законы фиксируются до опыта. Проверка должна показать, что обученные выходы действительно используются.

**Holdout, 4 случая — две комбинации до этого не встречались как пары**:

| Input + predicate | Gold output | Роль |
|---|---|---|
| `alpha + LAW_B` | `B` | cross-law, unseen combination |
| `beta + LAW_A` | `A` | cross-law, unseen combination |
| `alpha + LAW_A` | `A` | positive control |
| `beta + LAW_B` | `B` | positive control |

**A/B/C — та же обученная Unit и тот же индекс, меняется только форма объекта metadata:**
- A: `new[] {"LAW_B"}` — правильный `string[]`.
- B: `new List<string> {"LAW_B"}` — проверяемая потеря.
- C: входной List, преобразованный через `ToArray()` на ingress до `ExecuteAsync` — минимальный тестовый repair, без изменения Trainer/Index/Vectorize.

**Компактный исполнимый C# harness** (новый отдельный diagnostic console, с project reference на `21.AgentInfrastructure`, без модификаций core):

~~~csharp
using AgentInfrastructure.Logical;

var rows = new[]
{
    new LogicalKnnTrainingExample<string,string>(
        "row-A", "alpha", "A", ["LAW_A"], ["probe"], ["train-A"]),
    new LogicalKnnTrainingExample<string,string>(
        "row-B", "beta", "B", ["LAW_B"], ["probe"], ["train-B"])
};
var trained = KnnLogicalUnit<string,string>.Train(
    "micro-law-01", "law-classifier", "probe", rows,
    static x => x, vectorDimensions: 64, k: 2, confidenceFloor: 0.0);
if (!trained.IsSuccess) throw new Exception(trained.ErrorMessage);
var unit = trained.Value!;

async Task<(string label, double confidence)> Predict(string input, object predicates)
{
    var req = new LogicalUnitRequest<string>(
        "micro-run", "micro-law-01", "law-classifier", "probe",
        input, new LogicalExecutionFlags(),
        new Dictionary<string, object> { ["predicate_ids"] = predicates });
    var r = await unit.ExecuteAsync(req);
    if (!r.IsSuccess) throw new Exception(r.ErrorMessage);
    return (r.Value!.Output, r.Value.Confidence);
}
var cases = new[]
{
    (text:"alpha", law:"LAW_B", gold:"B", unseen:true),
    (text:"beta",  law:"LAW_A", gold:"A", unseen:true),
    (text:"alpha", law:"LAW_A", gold:"A", unseen:false),
    (text:"beta",  law:"LAW_B", gold:"B", unseen:false)
};
foreach (var q in cases)
{
    var a = await Predict(q.text, new[] { q.law });
    var b = await Predict(q.text, new List<string> { q.law });
    var c = await Predict(q.text, new List<string> { q.law }.ToArray());
    Console.WriteLine(
        $"{q.text}/{q.law} gold={q.gold} unseen={q.unseen} " +
        $"A={a.label}({a.confidence:F3}) " +
        $"B={b.label}({b.confidence:F3}) " +
        $"C={c.label}({c.confidence:F3})");
}
~~~

**Ожидаемое, но ещё НЕ измеренное:** canonical array и repaired ingress дадут одинаковые корректные answers; при List порядок закономерности поменяется на лексический nearest. В коде предикаты имеют вес +2, токены +1; для `LAW_A` и `LAW_B` вычисляются разные hash slots (13 и 14 в predicate half при dimensions=64). Проверить output и top-hit evidence; не ссылаться на прогноз как результат.

**Локальное принятие:** заранее выбранные cross queries: A=C=gold; B != gold как минимум на одном, при безошибочных двух positive controls. Это доказывает **влияние type-boundary на использование обученного Unit**. Если все arms равны: `NO_EFFECT_OBSERVED`; если неправильно и A — `INVALID_FIXTURE` / проверить collision и HNSW, **не подгонять** cases после просмотра. Затем отдельно 8–12 family-disjoint примеров для L2, а также label-swap train как negative control: перестановка обученных меток должна переставлять выходы. **Root cause именно RP01** разрешено писать только после caller-trace с фактической ошибочной формой metadata.

## 4 соседних мини-эксперимента (дополняют существующие 44, не дублируют их)

| Гипотеза | Единственное изменение A/B, минимум материалов | Что подтвердит влияние на *quality*, а не только код |
|---|---|---|
| **15.02 / DIF-14.01 — alias ExampleId** | Обучить из 2 примеров с ID `"case"` / `" case "`, затем из идентичных примеров с **разными** IDs. 2 exact sanity и 4 unseen текстовых варианта, общий gold. | A возвращает label *другого* вектора вследствие перезаписи dictionary, а B сохраняет корреспонденцию vector→label. Exact sanity = mechanism; new variants = локальная utility. |
| **15.03 / DIF-14.03 — domain scope** | Смешанный обучающий pool с двумя доменами, одинаковый вопрос, разные lawful outputs. A = один KNN с `UseDomainScopedRetrieval=true`, B = индекс/пул, ограниченный нужным domain **до** topK. 6 unseen queries. | Ошибочный чужой domain в A, правильный permitted result B при неизменных остальных features. Не заявлять tenant leak без проверки upstream. |
| **15.04 / DIF-14.04 — RNG HNSW** | Пять–десять rebuild одного и того же train set (60–100 близких exemplars, порядок зафиксирован), один heldout из 12 queries; каждый сравнить с `BruteForceIndex` exact reference. | Разброс recall@k или correctness across rebuild при стабильном gold. Отсутствие разброса в этой панели = NOT_OBSERVED, не универсальное опровержение. |
| **15.05 / DIF-13.01–03 — morphology branch** | Frozen embedding/input: A без дополнительных координат, B с historical `MorphDimensionAppender`, C с отдельным root-identity feature. 8 families для train и 12 unseen RU/HE/EN форм с ложными совпадениями. | B выигрывает semantic gold на unseen словоформах без роста false positives; C показывает предел семейного размера. До trace исторического caller это не доказательство потерянной RP01 функции. |

**Приоритет:** сперва 15.01 (без нейросетевых градиентов), 15.02; далее scoping, morphology, RNG. Для исходного DPO-проблемного места не дублировать [MINI-01 B0 vs DPO](12-RP01-MINI01-COLD-START-CAUSAL-EXPERIMENT.md). KNN-проба **не** может доказать correctness или defect LoRA.

## Обязательный файл результата и исполнитель

~~~json
{
  "probe_id": "MICRO-15.01",
  "status": "NOT_RUN",
  "source_sha": "83590a462b2f8aedc9d3ab4cb775193eb9c25ce5",
  "train_sha256": null,
  "holdout_sha256": null,
  "one_state_A_B_C": true,
  "intervention": "predicate_ids type only",
  "rows": [],
  "quality_correct_A_B_C": null,
  "ordinary_caller_verified": false,
  "learning_generalization_proved": false,
  "causal_verdict": "NOT_MEASURED",
  "independent_review": "NOT_PERFORMED"
}
~~~

**DPO authority:** в тех случаях, где действительно готовятся новые DPO, **CODEX на GPT-6 Astra 6 UltraCode выбирает учебные задачи от имени человека и независимо оценивает окончательные пары**, а **внешняя разрешённая открытая китайская модель создаёт оба chosen и rejected**. Здесь DPO-generation **не вызывается**; synthetic KNN probe и программный oracle не следует именовать китайским автором или CODEX/Astra-оценкой. Все фактические исходы требуют отдельной публикации evidence, review и readback. 
