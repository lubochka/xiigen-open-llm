# E40v2 · E42v2 · E32-check · E33-check — DATA fixes
## Подробный план для Claude Code v1.0

**Проект:** `35.ExperimentsV2`
**Зависимости:** LogicExtractor (E17), DomainClassifier (E18), EpisodeCorpusLoader
**Причина ревизии:** seed=42 создал аномальное разбиение — все LOW-trust / all-perfect-retention примеры попали в val-set. Тот же риск в E32/E33.
**Этот документ не касается XIIGen.**

---

## Четыре задачи

| Задача | Что делаем | Метрика |
|---|---|---|
| E40v2 TrustScorer | stratified split по классам LOW/MEDIUM/HIGH | trust_accuracy |
| E42v2 DiskManager | stratified split по retention_tier | knowledge_retention |
| E32-check | проверить distribution классов в val-set, перезапустить если нужно | count_accuracy |
| E33-check | проверить distribution StopSignal-классов в val-set | binary_accuracy |

---

---

# E40v2 — TrustScorer: stratified split

## Проблема

`GenerateExamples(seed: 42)` положил все LOW-trust примеры в validation.
Модель обучилась только на HIGH/MEDIUM → точность на LOW = 0% → общая точность 0%.

## Исправление: StratifiedSplit

```csharp
// E40v2: TrustScorer с корректным stratified split.
// Baseline:  то же правило is_internal ? HIGH : MEDIUM
// Enhanced:  K-NN на признаках источника
// Изменение: StratifiedSplit вместо RandomSplit(seed:42)
public sealed class E40v2TrustScorer
{
    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var examples = GenerateExamples(tier);

        // ── Исправление: stratified split ───────────────────────────────
        // Каждый класс (LOW=0, MEDIUM=1, HIGH=2) делится 70/30 независимо.
        // Гарантирует что все три класса представлены в train и val.
        var (train, val) = StratifiedSplit(examples, trainFraction: 0.7);

        // Проверка: каждый класс должен быть в val хотя бы 1 раз
        var valClasses = val.Select(e => e.Label).Distinct().OrderBy(x => x).ToList();
        if (valClasses.Count < 3)
            return OperationResult<ExperimentV2Result>.Fail(
                $"Val-set содержит только классы: {string.Join(",", valClasses)}. " +
                "Увеличьте размер датасета или проверьте GenerateExamples.");

        // ── Baseline ────────────────────────────────────────────────────
        int baseCorrect = val.Count(e =>
            BaselinePredict(e.Features) == e.Label);
        double baselineAcc = baseCorrect / (double)val.Count;

        // ── Enhanced: K-NN ──────────────────────────────────────────────
        var knn = new StructuralKnnPredictor(k: 5);
        knn.Train(train.Select(e => (ToVector(e.Features), e.Label)).ToList());

        int enhCorrect = val.Count(e =>
            knn.Predict(ToVector(e.Features)) == e.Label);
        double enhancedAcc = enhCorrect / (double)val.Count;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result(
            ExperimentId:  "E40v2",
            Tier:          tier,
            BaselineValue: baselineAcc,
            EnhancedValue: enhancedAcc,
            MetricName:    "trust_accuracy",
            Notes: $"val_size={val.Count}, " +
                   $"val_classes={string.Join(",", valClasses.Select(c => TrustNames[c]))}"));
    }

    // Stratified split: разбить каждый класс независимо.
    private static (List<TrustExample> Train, List<TrustExample> Val) StratifiedSplit(
        IReadOnlyList<TrustExample> examples, double trainFraction)
    {
        var train = new List<TrustExample>();
        var val   = new List<TrustExample>();

        // Группировать по классу, каждую группу разбить отдельно
        foreach (var group in examples.GroupBy(e => e.Label))
        {
            var items = group.ToList();
            // Перемешать детерминированно но без class-bias
            items = items.OrderBy(e => HashCode.Combine(e.Label, e.Features.IsInternal,
                                                         e.Features.UpdateDaysAgo)).ToList();
            int trainCount = (int)(items.Count * trainFraction);
            train.AddRange(items.Take(trainCount));
            val.AddRange(items.Skip(trainCount));
        }

        return (train, val);
    }

    private static int BaselinePredict(SourceFeatures f) =>
        f.IsInternal ? HIGH : f.IsOfficialDocs ? HIGH : MEDIUM;

    private static double[] ToVector(SourceFeatures f) => new[]
    {
        f.IsInternal ? 1.0 : 0.0,
        f.HasCIPassing ? 1.0 : 0.0,
        f.HasTests ? 1.0 : 0.0,
        Math.Max(0, 1.0 - f.UpdateDaysAgo / 365.0),
        f.PopularityScore / 3.0,
        f.IsOfficialDocs ? 1.0 : 0.0,
        f.DomainMatch ? 1.0 : 0.0,
    };
}
```

**Gate B:** `trust_accuracy enhanced > baseline`, все три класса в val-set.

---

# E42v2 — DiskManager: stratified split по retention_tier

## Проблема

Все perfect-retention примеры попали в val → ceiling эффект.
Enhanced не мог показать улучшение — baseline уже 100% на этом подмножестве.

## Исправление

```csharp
// E42v2: DiskManager с корректным stratified split.
// Три retention_tier: CRITICAL (всегда сохранять), NORMAL, STALE (кандидаты на удаление).
// StratifiedSplit гарантирует все три tier в train и val.
public sealed class E42v2DiskManager
{
    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var examples = GenerateChunks(tier);

        // ── Stratified split по retention_tier ──────────────────────────
        var (train, val) = StratifiedSplitByTier(examples, trainFraction: 0.7);

        var valTiers = val.Select(e => e.RetentionTier).Distinct().OrderBy(x => x).ToList();
        if (valTiers.Count < 3)
            return OperationResult<ExperimentV2Result>.Fail(
                $"Val-set содержит только tier: {string.Join(",", valTiers)}.");

        // ── Baseline: FIFO (удалять самые старые) ───────────────────────
        int baseRetained = SimulateEviction(val, strategy: EvictionStrategy.Fifo);
        double baselineRetention = baseRetained / (double)val.Count;

        // ── Enhanced: Value-based (удалять по весу × давности) ──────────
        int enhRetained = SimulateEviction(val, strategy: EvictionStrategy.ValueBased);
        double enhancedRetention = enhRetained / (double)val.Count;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result(
            ExperimentId:  "E42v2",
            Tier:          tier,
            BaselineValue: baselineRetention,
            EnhancedValue: enhancedRetention,
            MetricName:    "knowledge_retention",
            Notes: $"val_tiers={string.Join(",", valTiers)}, " +
                   $"critical_count={val.Count(e => e.RetentionTier == CRITICAL)}"));
    }
}
```

**Gate B:** CRITICAL чанки никогда не вытесняются при ValueBased стратегии.

---

# E32-check — проверить E32 EpisodeDecomposer

## Что проверяем

E32 использует `GenerateExamples` с фиксированным порядком.
Проверить что в val-set представлены все типы эпизодов: RESEARCH / BUILD / FIX / MIGRATE / DESIGN.

```csharp
// E32Check: запустить диагностику распределения val-set.
// Добавить в ExperimentDispatcher как диагностический шаг перед E32.
public static void CheckE32Distribution(IReadOnlyList<EpisodeExample> examples)
{
    // Разбить как делает E32 (первые 70% train, остальные val)
    int trainCount = (int)(examples.Count * 0.7);
    var val = examples.Skip(trainCount).ToList();

    var typeGroups = val.GroupBy(e => e.EpisodeType)
                        .ToDictionary(g => g.Key, g => g.Count());

    Console.WriteLine($"E32 val-set distribution (total={val.Count}):");
    foreach (var kv in typeGroups.OrderBy(kv => kv.Key))
        Console.WriteLine($"  {kv.Key}: {kv.Value}");

    // Если какой-то тип отсутствует в val → применить StratifiedSplit
    var missingTypes = new[] { "RESEARCH","BUILD","FIX","MIGRATE","DESIGN" }
        .Where(t => !typeGroups.ContainsKey(t)).ToList();

    if (missingTypes.Any())
        Console.WriteLine($"  ⚠ MISSING in val: {string.Join(",", missingTypes)}");
    else
        Console.WriteLine("  ✓ All types present in val-set");
}
```

Если нашли пропущенные типы → применить тот же `StratifiedSplit` что в E40v2, перезапустить E32.

---

# E33-check — проверить E33 StopSignalDetector

## Что проверяем

E33 — бинарный классификатор (STOP / CONTINUE).
Проверить что val-set содержит оба класса в разумной пропорции (не 90/10).

```csharp
public static void CheckE33Distribution(IReadOnlyList<StopExample> examples)
{
    int trainCount = (int)(examples.Count * 0.7);
    var val = examples.Skip(trainCount).ToList();

    int stopCount     = val.Count(e => e.ShouldStop);
    int continueCount = val.Count(e => !e.ShouldStop);
    double ratio      = stopCount / (double)val.Count;

    Console.WriteLine($"E33 val-set: STOP={stopCount}, CONTINUE={continueCount}, ratio={ratio:P1}");

    if (ratio < 0.15 || ratio > 0.85)
        Console.WriteLine("  ⚠ Сильный дисбаланс классов — применить StratifiedSplit");
    else
        Console.WriteLine("  ✓ Распределение приемлемо");
}
```

**Действие:** если ratio < 0.15 или > 0.85 → применить StratifiedSplit, перезапустить E33.

---

## Добавить check-функции и v2-эксперименты в Dispatcher

```csharp
// В ExperimentDispatcher.RunAll():

// 1. Диагностика E32/E33 перед их запуском
CheckE32Distribution(EpisodeCorpusLoader.LoadAll());
CheckE33Distribution(StopSignalDataset.LoadAll());

// 2. Запустить v2 версии вместо v1 для E40/E42
new E40v2TrustScorer().Run(tier),
new E42v2DiskManager().Run(tier),
```

---

## Ожидаемые результаты

| Эксперимент | Было | Ожидаем |
|---|---|---|
| E40v2 | 0.0% (floor) | enhanced > baseline, все 3 класса в val |
| E42v2 | 0.0% (ceiling) | CRITICAL retention > FIFO retention |
| E32-check | PASSED 11.1% | подтвердить или перезапустить с stratified |
| E33-check | PASSED 25.0% | подтвердить или перезапустить с stratified |

**Время:** 1-2 дня.
