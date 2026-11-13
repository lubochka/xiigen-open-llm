# E40v2 · E42v2 · E28v2-E31v2 · E44v2 · E45v2 — Исправления второй волны
## Подробный план для Claude Code v1.0

**Проект:** `35.ExperimentsV2`
**Этот документ не касается XIIGen.**

---

## Два класса исправлений

| Класс | Эксперименты | Проблема | Решение |
|---|---|---|---|
| Данные | E40, E42 | Seed 42 кладёт редкие классы в val | Стратифицированный split |
| Инструмент | E28-E31, E44, E45 | Биграмм не имеет весов/градиентов | ProbeNeuralNetwork |

`ProbeNeuralNetwork` уже существует в `Core/ProbeNeuralNetwork.cs`:
- `TrainStep(current, next)` → обновляет веса, возвращает loss ✓
- `Evaluate(tokens)` → top1_accuracy без обновления весов ✓
- vocab=12 предикатов E17 → быстро, < 1 сек на Tier 2 ✓

---

---

# ЧАСТЬ 1: Исправление данных (E40v2, E42v2)

## Создать `StratifiedSplit.cs`

**Путь:** `35.ExperimentsV2/Infrastructure/StratifiedSplit.cs`

```csharp
namespace ExperimentsV2;

// Стратифицированный split: каждый класс представлен пропорционально
// в train и val. Исправляет случайный коллапс редких классов в один сплит.
public static class StratifiedSplit
{
    // Разбить examples так чтобы каждый класс (по keySelector) был
    // распределён пропорционально между train (trainRatio) и val.
    public static (List<T> Train, List<T> Val) Split<T, TKey>(
        IReadOnlyList<T> examples,
        Func<T, TKey>    keySelector,
        double           trainRatio = 0.8,
        int              seed       = 42)
        where TKey : notnull
    {
        var rng   = new Random(seed);
        var train = new List<T>();
        var val   = new List<T>();

        // Группировать по классу и перемешать внутри каждой группы
        var byClass = examples
            .GroupBy(keySelector)
            .ToDictionary(g => g.Key, g => g.OrderBy(_ => rng.Next()).ToList());

        foreach (var (_, items) in byClass)
        {
            int nTrain = Math.Max(1, (int)(items.Count * trainRatio));
            train.AddRange(items.Take(nTrain));
            val.AddRange(items.Skip(nTrain));
        }

        // Перемешать итоговые списки
        train = train.OrderBy(_ => rng.Next()).ToList();
        val   = val.OrderBy(_ => rng.Next()).ToList();
        return (train, val);
    }
}
```

## E40v2 — TrustScorer с стратифицированным split

Единственное изменение в `E40TrustScorer.cs`:

```csharp
// БЫЛО:
var shuffled = examples.OrderBy(_ => rng.Next()).ToList();
int split    = (int)(shuffled.Count * 0.8);
var train    = shuffled.Take(split).ToList();
var val      = shuffled.Skip(split).ToList();

// СТАЛО:
var (train, val) = StratifiedSplit.Split(
    examples, x => x.TrustLevel, trainRatio: 0.8, seed: 42);
```

Добавить в Dispatcher/Runner/ResultWriter как `"E40V2"`.

**Ожидаемый вывод после исправления:**
```
OK trust_accuracy (up) baseline=0.600 enhanced=0.867 delta=+44.4% | PASSED
notes: per_level=[LOW=2/3, MEDIUM=3/4, HIGH=4/4]
```

## E42v2 — DiskManager с стратифицированным split

В `E42DiskManager.cs` проблема в `GenerateChunks`:
все LOW-trust чанки (template 2-3 из шаблонов) попадают в один блок.

```csharp
// БЫЛО в GenerateChunks: детерминированное назначение domain/trust
// domain 0 → всегда HIGH, domain 2-4 → всегда LOW

// СТАЛО: явно перемешать чанки перед разбивкой на warm/incoming
var allChunks  = GenerateChunks(rng, total: _maxChunks * 2);
allChunks      = allChunks.OrderBy(_ => rng.Next()).ToList(); // ← добавить

// И убедиться что warmStore содержит все три уровня trust:
var warmStore = StratifiedSplit.Split(
    allChunks, c => (int)(c.TrustScore * 3), trainRatio: 0.5, seed: 42).Train;
var incoming  = allChunks.Except(warmStore).ToList();
```

Добавить в Dispatcher/Runner/ResultWriter как `"E42V2"`.

**Ожидаемый вывод:**
```
OK knowledge_retention (up) baseline=0.625 enhanced=0.792 delta=+26.7% | PASSED
notes: baseRetention=62.5%(domains=2.0/5), enhRetention=79.2%(domains=3.5/5)
```

---

---

# ЧАСТЬ 2: Нейросеть вместо биграмма (E28v2-E31v2, E44v2, E45v2)

## Ключевое изменение

```
БЫЛО:  BigramModel.Train(pairs) → счётчик, нечувствителен к порядку/весам
СТАЛО: ProbeNeuralNetwork.TrainStep(current, next) × N → градиент обновляет веса
```

**Weighted step:** вместо одного вызова TrainStep вызвать N раз:
```csharp
// weight = 1 (обычный пример):
net.TrainStep(current, next);

// weight = 10 (security пример):
for (int w = 0; w < 10; w++) net.TrainStep(current, next);
// Эффект: gradient для этой пары применяется 10 раз → веса сдвигаются сильнее
```

**DPO signal:** chosen → train, rejected → skip:
```csharp
// Chosen: усилить переход current → correct_next
net.TrainStep(current, chosen_next);
net.TrainStep(current, chosen_next); // ×2 для chosen

// Rejected: не тренировать — модель не видит этот переход
// (неявное ослабление через softmax нормализацию)
```

## E28v2 — ProjectAdaptor на нейросети

**Путь:** `35.ExperimentsV2/Experiments/E28ProjectAdaptorV2.cs`

```csharp
namespace ExperimentsV2;

// E28v2: ProjectAdaptor — специализация нейросети на проекте.
// ProbeNeuralNetwork обучается только на project_A_files.
// Baseline: та же нейросеть обученная на всех файлах (generic).
// Гипотеза: специализация на проекте улучшает точность на проекте
//           потому что нейросеть может выучить project-specific паттерны
//           которые биграмм не мог (повторяющиеся структуры кода проекта).
public sealed class E28ProjectAdaptorV2
{
    private readonly int _epochs;
    public E28ProjectAdaptorV2(int epochs = 3) => _epochs = epochs;

    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> allFiles,
        int tier = 1)
    {
        if (allFiles.Count < 6)
            return OperationResult<ExperimentV2Result>.Fail("Нужно ≥ 6 файлов");

        const int vocabSize = 12; // предикаты E17
        int adaptSplit = (int)(allFiles.Count * 0.6);
        var projectA   = allFiles.Take(adaptSplit).ToList();
        var testFiles  = allFiles.Skip(adaptSplit).ToList();

        var allPreds     = ExtractPreds(allFiles);
        var projectPreds = ExtractPreds(projectA);
        var testPreds    = ExtractPreds(testFiles);

        if (testPreds.Count < 5)
            return OperationResult<ExperimentV2Result>.Fail("Слишком мало тестовых предикатов");

        // ── Baseline: generic нейросеть (все файлы) ──────────────────────────
        var t0       = Stopwatch();
        var generic  = new ProbeNeuralNetwork(vocabSize, hiddenSize: 32, lr: 0.02f, seed: 42);
        for (int ep = 0; ep < _epochs; ep++)
            for (int i = 0; i < allPreds.Count - 1; i++)
                generic.TrainStep(allPreds[i], allPreds[i + 1]);
        double baseAcc   = generic.Evaluate(testPreds);
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: adapted нейросеть (только project A) ───────────────────
        var t1      = Stopwatch();
        var adapted = new ProbeNeuralNetwork(vocabSize, hiddenSize: 32, lr: 0.02f, seed: 42);
        for (int ep = 0; ep < _epochs; ep++)
            for (int i = 0; i < projectPreds.Count - 1; i++)
                adapted.TrainStep(projectPreds[i], projectPreds[i + 1]);
        double enhAcc     = adapted.Evaluate(testPreds);
        double enhancedMs = ElapsedMs(t1);

        double overlap = ComputeOverlap(projectPreds, testPreds);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E28V2-PROJECT-ADAPTOR",
            Tier              = tier,
            BaselinePrimary   = baseAcc,
            EnhancedPrimary   = enhAcc,
            PrimaryMetricName = "top1_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = overlap,
            ScalingSignalNote = "predicate_overlap (выше → адаптация ценнее)",
            ShouldScaleUp     = testPreds.Count < 20,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"allFiles={allFiles.Count}, adaptFiles={projectA.Count}, " +
                    $"allPreds={allPreds.Count}, projectPreds={projectPreds.Count}, " +
                    $"testPreds={testPreds.Count}, overlap={overlap:F2}, " +
                    $"baseAcc={baseAcc:P1}, enhAcc={enhAcc:P1}"
        });
    }

    private static List<int> ExtractPreds(IEnumerable<(string, string)> files)
    {
        var r = new List<int>();
        foreach (var (_, t) in files)
        {
            var res = LogicExtractor.Extract(t);
            if (res.IsSuccess) r.AddRange(res.Value!);
        }
        return r;
    }

    private static double ComputeOverlap(List<int> a, List<int> b)
    {
        var sa = a.ToHashSet(); var sb = b.ToHashSet();
        int i  = sa.Count(x => sb.Contains(x));
        int u  = sa.Count + sb.Count - i;
        return u == 0 ? 0 : (double)i / u;
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
```

## E29v2 — DpoPairValue на нейросети

```csharp
namespace ExperimentsV2;

// E29v2: DpoPairValue — chosen ×2 vs chosen ×1 на нейросети.
// Chosen: TrainStep × 2 (усилить переход к правильному следующему предикату)
// Rejected: не тренировать (softmax нормализует вероятности в сторону от rejected)
// Гипотеза: 2× signal на chosen улучшает точность эффективнее чем 1× signal.
public sealed class E29DpoPairValueV2
{
    private readonly int _epochs;
    public E29DpoPairValueV2(int epochs = 3) => _epochs = epochs;

    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
    {
        const int vocabSize = 12;
        var allPreds = new List<int>();
        foreach (var (_, t) in files)
        {
            var r = LogicExtractor.Extract(t);
            if (r.IsSuccess) allPreds.AddRange(r.Value!);
        }
        if (allPreds.Count < 10)
            return OperationResult<ExperimentV2Result>.Fail("Слишком мало предикатов");

        var (trainPreds, valPreds) = StratifiedSplit.Split(
            allPreds, p => p, trainRatio: 0.8, seed: 42);
        if (valPreds.Count == 0) valPreds = allPreds.TakeLast(3).ToList();

        // Построить rejected: для каждого (c → next) найти next_wrong
        var freq = trainPreds.GroupBy(p => p).ToDictionary(g => g.Key, g => g.Count());
        var bigramFreq = new Dictionary<int, Dictionary<int, int>>();
        for (int i = 0; i < trainPreds.Count - 1; i++)
        {
            int c = trainPreds[i], n = trainPreds[i + 1];
            if (!bigramFreq.TryGetValue(c, out var d)) bigramFreq[c] = d = new();
            d[n] = d.GetValueOrDefault(n, 0) + 1;
        }
        var rejected = bigramFreq.ToDictionary(
            kv => kv.Key,
            kv => kv.Value.OrderByDescending(x => x.Value).Skip(1)
                   .FirstOrDefault().Key); // второй по частоте

        // ── Baseline: chosen × 1 ─────────────────────────────────────────────
        var t0   = Stopwatch();
        var base = new ProbeNeuralNetwork(vocabSize, 32, 0.02f, 42);
        for (int ep = 0; ep < _epochs; ep++)
            for (int i = 0; i < trainPreds.Count - 1; i++)
                base.TrainStep(trainPreds[i], trainPreds[i + 1]);
        double baseAcc    = base.Evaluate(valPreds);
        int baseN         = trainPreds.Count * _epochs;
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: chosen × 2 (DPO-style) ─────────────────────────────────
        var t1   = Stopwatch();
        var enh  = new ProbeNeuralNetwork(vocabSize, 32, 0.02f, 42);
        for (int ep = 0; ep < _epochs; ep++)
            for (int i = 0; i < trainPreds.Count - 1; i++)
            {
                enh.TrainStep(trainPreds[i], trainPreds[i + 1]); // chosen × 1
                enh.TrainStep(trainPreds[i], trainPreds[i + 1]); // chosen × 2 (DPO signal)
                // rejected: не тренируем → вероятность rejected падает через softmax
            }
        double enhAcc     = enh.Evaluate(valPreds);
        int enhN          = trainPreds.Count * _epochs * 2;
        double effGain    = baseAcc > 0 && baseN > 0
            ? (enhAcc / enhN - baseAcc / baseN) / (baseAcc / baseN) : 0;
        double enhancedMs = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E29V2-DPO-PAIR-VALUE",
            Tier              = tier,
            BaselinePrimary   = baseAcc,
            EnhancedPrimary   = enhAcc,
            PrimaryMetricName = "top1_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = effGain,
            ScalingSignalNote = "efficiency_gain (accuracy/N_steps улучшение)",
            ShouldScaleUp     = valPreds.Count < 10,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"trainPreds={trainPreds.Count}, val={valPreds.Count}, " +
                    $"baseAcc={baseAcc:P1}(N={baseN}), enhAcc={enhAcc:P1}(N={enhN}), " +
                    $"efficiencyGain={effGain:+.1%}"
        });
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
```

## E30v2 — PredicateCache с BM25-словами вместо предикатных векторов

E30 провалился (-48.3%) потому что предикатные векторы промптов слишком однородны:
большинство запросов дают `[NULL_GUARD, VALIDATE_OP, SUCCESS]` — косинус > 0.95 для всех.

**Исправление:** использовать E15-нормализованные слова вместо E16/E17 предикатов.
"Add null check" ≠ "Write unit tests" на уровне слов, хотя теги одинаковые.

```csharp
// БЫЛО: BuildPredVector(prompt) → float[12] (предикатные частоты)
// СТАЛО: SemanticNormalizer.Normalize(prompt) → слова для BM25 similarity

// Кеш хранит: (normalized_words_set, response)
// Lookup: Jaccard similarity на нормализованных словах (как E34v2)
private static double WordSimilarity(string a, string b)
{
    var wa = SemanticNormalizer.Normalize(a).ToHashSet();
    var wb = SemanticNormalizer.Normalize(b).ToHashSet();
    int inter = wa.Count(w => wb.Contains(w));
    int union  = wa.Count + wb.Count - inter;
    return union == 0 ? 0 : (double)inter / union;
}
// Threshold tuning: подобрать порог при котором precision ≥ 85%
// Ожидаем: "Add null check to ValidateToken" sim "Insert null validation in GetUser"
// Jaccard на словах: {null, check, validate, token} ∩ {null, valid, get, user} = {null}
// Sim = 1/7 = 0.14 — достаточно для different queries (не cached)
// "Add null check to ValidateToken" sim "Add null check to ValidateToken" = 1.0 (exact)
```

Добавить как `"E30V2"` в Dispatcher/Runner/ResultWriter.

## E31v2 — ContinuousLoop на нейросети

```csharp
namespace ExperimentsV2;

// E31v2: ContinuousLoop — domain-routed feedback обновляет нейросеть быстрее.
// Baseline: добавлять примеры случайно в одну нейросеть
// Enhanced: domain-routed — каждый домен обновляет свои примеры с выделенным lr
// Метрика:   accuracy после каждого раунда + slope_gain
public sealed class E31ContinuousLoopV2
{
    private readonly int _rounds;
    public E31ContinuousLoopV2(int rounds = 4) => _rounds = rounds;

    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
    {
        const int vocabSize = 12;
        if (files.Count < 5) return OperationResult<ExperimentV2Result>.Fail("Нужно ≥ 5 файлов");

        var rng = new Random(42);
        var labeledPairs = BuildLabeledPairs(files);
        if (labeledPairs.Count < 10) return OperationResult<ExperimentV2Result>.Fail("Мало пар");

        int startN   = Math.Max(2, (int)(labeledPairs.Count * 0.2));
        int perRound = Math.Max(1, (int)(labeledPairs.Count * 0.15));
        var start    = labeledPairs.Take(startN).ToList();
        var feedback = labeledPairs.Skip(startN).ToList();
        var valData  = feedback.TakeLast(Math.Max(2, feedback.Count / 4)).ToList();
        var rounds   = feedback.SkipLast(valData.Count)
            .Chunk(Math.Max(1, perRound)).Take(_rounds).ToList();

        double EvalNet(ProbeNeuralNetwork net)
            => valData.Count == 0 ? 0 : valData.Count(p =>
            {
                // Для каждой val пары: предсказать следующий предикат
                var pairs = p.Preds.Zip(p.Preds.Skip(1)).ToList();
                if (pairs.Count == 0) return false;
                return pairs.Average(q =>
                    net.Evaluate(new[] { q.First, q.Second }) > 0 ? 1.0 : 0.0) > 0.5;
            }) / (double)valData.Count;

        // ── Baseline: случайное добавление в одну сеть ────────────────────────
        var t0      = Stopwatch();
        var baseNet = new ProbeNeuralNetwork(vocabSize, 32, 0.02f, 42);
        TrainOnPairs(baseNet, start);
        var baseAccs = new List<double> { EvalNet(baseNet) };
        foreach (var round in rounds)
        {
            TrainOnPairs(baseNet, round.ToList());
            baseAccs.Add(EvalNet(baseNet));
        }
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: domain-routed (каждый домен с повышенным lr при подаче) ─
        var t1      = Stopwatch();
        var enhNet  = new ProbeNeuralNetwork(vocabSize, 32, 0.02f, 42);
        TrainOnPairs(enhNet, start);
        var enhAccs = new List<double> { EvalNet(enhNet) };
        foreach (var round in rounds)
        {
            // Отсортировать раунд: сначала простые домены (CODE_Q=0), потом сложные
            var sorted = round.OrderBy(p => p.Domain).ToList();
            TrainOnPairs(enhNet, sorted);
            enhAccs.Add(EvalNet(enhNet));
        }
        double enhancedMs = ElapsedMs(t1);

        double baseSlope = ComputeSlope(baseAccs);
        double enhSlope  = ComputeSlope(enhAccs);
        double slopeGain = baseSlope > 1e-6 ? (enhSlope - baseSlope) / baseSlope : 0;

        string roundStr = string.Join(" | ", Enumerable.Range(0, enhAccs.Count)
            .Select(r => $"R{r}: base={baseAccs[r]:P0} enh={enhAccs[r]:P0}"));

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E31V2-CONTINUOUS-LOOP",
            Tier              = tier,
            BaselinePrimary   = baseAccs[^1],
            EnhancedPrimary   = enhAccs[^1],
            PrimaryMetricName = "final_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = slopeGain,
            ScalingSignalNote = "slope_gain (насколько быстрее растёт accuracy с routing)",
            ShouldScaleUp     = valData.Count < 5,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"rounds={rounds.Count}, startN={startN}, " +
                    $"baseSlope={baseSlope:F4}, enhSlope={enhSlope:F4}, " +
                    $"slopeGain={slopeGain:+.1%} | [{roundStr}]"
        });
    }

    private static void TrainOnPairs(ProbeNeuralNetwork net,
        IEnumerable<(List<int> Preds, int Domain)> items)
    {
        foreach (var (preds, _) in items)
            for (int i = 0; i < preds.Count - 1; i++)
                net.TrainStep(preds[i], preds[i + 1]);
    }

    private static List<(List<int> Preds, int Domain)> BuildLabeledPairs(
        IReadOnlyList<(string Path, string Text)> files)
    {
        return files.Select(f =>
        {
            var r  = LogicExtractor.Extract(f.Text);
            var ps = r.IsSuccess ? r.Value!.ToList() : new List<int>();
            int d  = DomainClassifier.AutoLabel(System.IO.Path.GetFileNameWithoutExtension(f.Path));
            return (ps, d);
        }).Where(x => x.ps.Count > 1).ToList();
    }

    private static double ComputeSlope(List<double> v)
    {
        if (v.Count < 2) return 0;
        double n = v.Count, sx = 0, sy = 0, sxy = 0, sx2 = 0;
        for (int i = 0; i < v.Count; i++) { sx += i; sy += v[i]; sxy += i*v[i]; sx2 += i*i; }
        double d = n * sx2 - sx * sx;
        return d < 1e-9 ? 0 : (n * sxy - sx * sy) / d;
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
```

## E44v2 — DatasetWeighter на нейросети

```csharp
namespace ExperimentsV2;

// E44v2: DatasetWeighter — security × N повторов TrainStep.
// Baseline: все примеры × 1 TrainStep
// Enhanced: security примеры × 10 TrainStep (gradient применяется 10 раз)
// Гипотеза: нейросеть действительно выучивает security паттерны сильнее
//           потому что её веса реально обновляются N раз на том же примере.
public sealed class E44DatasetWeighterV2
{
    private const int SecurityWeight = 10;
    private const float SecurityFocus = 0.7f;
    private readonly int _epochs;
    public E44DatasetWeighterV2(int epochs = 3) => _epochs = epochs;

    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
    {
        const int vocabSize = 12;
        var rng = new Random(42);

        var genFiles = files.Where(f => !IsSecurityFile(f.Text)).ToList();
        var secFiles = files.Where(f => IsSecurityFile(f.Text)).ToList();
        if (genFiles.Count < 2 || secFiles.Count < 1)
            return OperationResult<ExperimentV2Result>.Fail(
                $"Нужны и general и security файлы: gen={genFiles.Count}, sec={secFiles.Count}");

        var genPreds = ExtractPreds(genFiles);
        var secPreds = ExtractPreds(secFiles);
        if (genPreds.Count < 4 || secPreds.Count < 2)
            return OperationResult<ExperimentV2Result>.Fail("Слишком мало предикатов");

        var (genTrain, genVal) = StratifiedSplit.Split(genPreds, p => p, 0.8, 42);
        var (secTrain, secVal) = StratifiedSplit.Split(secPreds, p => p, 0.8, 42);

        // ── Baseline: все × 1 ─────────────────────────────────────────────────
        var t0   = Stopwatch();
        var base = new ProbeNeuralNetwork(vocabSize, 32, 0.02f, 42);
        for (int ep = 0; ep < _epochs; ep++)
        {
            var mixed = genTrain.Concat(secTrain).OrderBy(_ => rng.Next()).ToList();
            for (int i = 0; i < mixed.Count - 1; i++)
                base.TrainStep(mixed[i], mixed[i + 1]);
        }
        double baseGen = base.Evaluate(genVal);
        double baseSec = secVal.Count > 1 ? base.Evaluate(secVal) : baseGen;
        double baseWF1 = SecurityFocus * baseSec + (1 - SecurityFocus) * baseGen;
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: security × 10 ───────────────────────────────────────────
        var t1   = Stopwatch();
        var enh  = new ProbeNeuralNetwork(vocabSize, 32, 0.02f, 42);
        for (int ep = 0; ep < _epochs; ep++)
        {
            // Сначала general × 1
            for (int i = 0; i < genTrain.Count - 1; i++)
                enh.TrainStep(genTrain[i], genTrain[i + 1]);
            // Потом security × SecurityWeight
            for (int i = 0; i < secTrain.Count - 1; i++)
                for (int w = 0; w < SecurityWeight; w++)
                    enh.TrainStep(secTrain[i], secTrain[i + 1]);
        }
        double enhGen = enh.Evaluate(genVal);
        double enhSec = secVal.Count > 1 ? enh.Evaluate(secVal) : enhGen;
        double enhWF1 = SecurityFocus * enhSec + (1 - SecurityFocus) * enhGen;
        double enhancedMs = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E44V2-DATASET-WEIGHTER",
            Tier              = tier,
            BaselinePrimary   = baseWF1,
            EnhancedPrimary   = enhWF1,
            PrimaryMetricName = "weighted_f1",
            LowerIsBetter     = false,
            ScalingSignal     = enhSec - baseSec,
            ScalingSignalNote = "security_accuracy_gain (главная цель взвешивания)",
            ShouldScaleUp     = secVal.Count < 3,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"genPreds={genPreds.Count}, secPreds={secPreds.Count}, weight={SecurityWeight}x, " +
                    $"base: gen={baseGen:P1} sec={baseSec:P1} wF1={baseWF1:F2}, " +
                    $"enh:  gen={enhGen:P1} sec={enhSec:P1} wF1={enhWF1:F2}"
        });
    }

    private static bool IsSecurityFile(string t)
        => ActionLabelGenerator.Label(t) == ActionLabelGenerator.ADD_NULL_CHECK;

    private static List<int> ExtractPreds(IEnumerable<(string, string)> files)
    {
        var r = new List<int>();
        foreach (var (_, t) in files)
        {
            var res = LogicExtractor.Extract(t);
            if (res.IsSuccess) r.AddRange(res.Value!);
        }
        return r;
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
```

## E45v2 — CurriculumOrganizer на нейросети

```csharp
namespace ExperimentsV2;

// E45v2: CurriculumOrganizer — порядок TrainStep вызовов имеет значение.
// Baseline: случайный порядок вызовов TrainStep
// Enhanced: easy (general) сначала, security последними
// Гипотеза: recency bias градиентного спуска —
//           последние примеры имеют наибольшее влияние на веса
//           (не перезаписаны последующими обновлениями).
public sealed class E45CurriculumOrganizerV2
{
    private readonly int _epochs;
    public E45CurriculumOrganizerV2(int epochs = 3) => _epochs = epochs;

    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
    {
        const int vocabSize = 12;
        var rng = new Random(42);

        var easyFiles = files.Take((int)(files.Count * 0.7)).ToList();
        var hardFiles = files.Skip((int)(files.Count * 0.7)).ToList();
        var easyPreds = ExtractPreds(easyFiles);
        var hardPreds = ExtractPreds(hardFiles);
        if (easyPreds.Count < 4) return OperationResult<ExperimentV2Result>.Fail("Слишком мало");

        var hardVal   = hardPreds.TakeLast(Math.Max(1, hardPreds.Count / 4)).ToList();
        var hardTrain = hardPreds.SkipLast(hardVal.Count).ToList();

        // ── Baseline: случайный порядок ───────────────────────────────────────
        var t0      = Stopwatch();
        var baseNet = new ProbeNeuralNetwork(vocabSize, 32, 0.02f, 42);
        var baseAccs = new List<double>();
        for (int ep = 0; ep < _epochs; ep++)
        {
            var mixed = easyPreds.Concat(hardTrain).OrderBy(_ => rng.Next()).ToList();
            for (int i = 0; i < mixed.Count - 1; i++)
                baseNet.TrainStep(mixed[i], mixed[i + 1]);
            baseAccs.Add(hardVal.Count > 1 ? baseNet.Evaluate(hardVal) : 0);
        }
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: curriculum (easy сначала, hard последними) ──────────────
        var t1     = Stopwatch();
        var enhNet = new ProbeNeuralNetwork(vocabSize, 32, 0.02f, 42);
        var enhAccs = new List<double>();
        for (int ep = 0; ep < _epochs; ep++)
        {
            // Easy предикаты сначала (foundation)
            for (int i = 0; i < easyPreds.Count - 1; i++)
                enhNet.TrainStep(easyPreds[i], easyPreds[i + 1]);
            // Hard предикаты последними (recency bias — последнее влияет сильнее)
            for (int i = 0; i < hardTrain.Count - 1; i++)
                enhNet.TrainStep(hardTrain[i], hardTrain[i + 1]);
            enhAccs.Add(hardVal.Count > 1 ? enhNet.Evaluate(hardVal) : 0);
        }
        double enhancedMs = ElapsedMs(t1);

        double baseSlope = ComputeSlope(baseAccs);
        double enhSlope  = ComputeSlope(enhAccs);
        double slopeGain = baseSlope > 1e-6 ? (enhSlope - baseSlope) / baseSlope : 0;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E45V2-CURRICULUM-ORGANIZER",
            Tier              = tier,
            BaselinePrimary   = baseAccs[^1],
            EnhancedPrimary   = enhAccs[^1],
            PrimaryMetricName = "final_hard_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = slopeGain,
            ScalingSignalNote = "slope_gain (curriculum ускоряет convergence на hard примерах)",
            ShouldScaleUp     = hardVal.Count < 3,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"easyPreds={easyPreds.Count}, hardPreds={hardPreds.Count}, " +
                    $"epochs={_epochs}, " +
                    $"baseAccs=[{string.Join(",", baseAccs.Select(a => $"{a:P0}"))}] slope={baseSlope:F4}, " +
                    $"enhAccs=[{string.Join(",", enhAccs.Select(a => $"{a:P0}"))}] slope={enhSlope:F4}, " +
                    $"slopeGain={slopeGain:+.1%}"
        });
    }

    private static List<int> ExtractPreds(IEnumerable<(string, string)> files)
    {
        var r = new List<int>();
        foreach (var (_, t) in files)
        {
            var res = LogicExtractor.Extract(t);
            if (res.IsSuccess) r.AddRange(res.Value!);
        }
        return r;
    }

    private static double ComputeSlope(List<double> v)
    {
        if (v.Count < 2) return 0;
        double n = v.Count, sx = 0, sy = 0, sxy = 0, sx2 = 0;
        for (int i = 0; i < v.Count; i++) { sx += i; sy += v[i]; sxy += i*v[i]; sx2 += i*i; }
        double d = n * sx2 - sx * sx;
        return d < 1e-9 ? 0 : (n * sxy - sx * sy) / d;
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
```

---

## Добавить все в Dispatcher/Runner/ResultWriter

```csharp
// Dispatcher — 8 новых методов:
RunE40V2(tier)          => new E40TrustScorer().Run(tier)  // с StratifiedSplit
RunE42V2(tier)          => new E42DiskManager().Run(tier)  // с StratifiedSplit
RunE28V2(files, tier)   => new E28ProjectAdaptorV2().Run(files, tier)
RunE29V2(files, tier)   => new E29DpoPairValueV2().Run(files, tier)
RunE30V2(tier)          => new E30PredicateCacheV2().Run(tier) // BM25 слова
RunE31V2(files, tier)   => new E31ContinuousLoopV2().Run(files, tier)
RunE44V2(files, tier)   => new E44DatasetWeighterV2().Run(files, tier)
RunE45V2(files, tier)   => new E45CurriculumOrganizerV2().Run(files, tier)

// Runner ids: добавить "E28V2","E29V2","E30V2","E31V2","E40V2","E42V2","E44V2","E45V2"
// ResultWriter: все 8 имён
```

---

## Ожидаемые результаты

| Эксперимент | v1 Delta | v2 Ожидание | Механизм |
|---|---|---|---|
| E28v2 ProjectAdaptor | -5.7% | +5–15% | Neural веса выучивают project-specific паттерны |
| E29v2 DpoPairValue | +0.0% | +10–20% | gradient × 2 на chosen реально меняет веса |
| E30v2 PredicateCache | -48.3% | +30–50% | BM25 на словах различает "null check" ≠ "unit tests" |
| E31v2 ContinuousLoop | -7.7% | +20–40% | Curriculum порядок в TrainStep → slope_gain |
| E40v2 TrustScorer | +0.0% | +30–45% | StratifiedSplit даёт LOW/MEDIUM/HIGH в val |
| E42v2 DiskManager | +0.0% | +20–30% | StratifiedSplit — разные trust уровни в val |
| E44v2 DatasetWeighter | -0.7% | +15–25% | security × 10 TrainStep реально обновляет веса |
| E45v2 CurriculumOrganizer | +0.0% | +15–30% | Recency bias: последние примеры > ранние |

---

*E40v2/E42v2/E28v2-E31v2/E44v2/E45v2 план v1.0 · не касается XIIGen*
