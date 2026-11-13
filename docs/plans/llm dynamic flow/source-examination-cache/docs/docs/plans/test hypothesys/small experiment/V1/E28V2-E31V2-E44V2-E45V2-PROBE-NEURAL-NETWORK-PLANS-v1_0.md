# E28v2 · E29v2 · E30v2 · E31v2 · E44v2 · E45v2 — ProbeNeuralNetwork
## Подробный план для Claude Code v1.0

**Проект:** `35.ExperimentsV2`
**Зависимости:** PatternClassifier (E18), LogicExtractor (E17), FamilyCollapser (E16)
**Причина ревизии:** биграмная модель = счётчик частот без backpropagation.
Взвешивание, DPO, curriculum, кеш — всё это концепции градиентного обучения.
Нужен `ProbeNeuralNetwork` — маленький 1-слойный классификатор с loss и backprop.
**Этот документ не касается XIIGen.**

---

## Шесть гипотез — одна инфраструктура

| Эксперимент | Гипотеза | Метрика |
|---|---|---|
| E28v2 ProjectAdaptor | Fine-tuning на проекте улучшает точность для этого проекта | accuracy delta |
| E29v2 DpoPairValue | DPO chosen/rejected даёт больший градиентный сигнал чем просто chosen | improvement per example |
| E30v2 PredicateCache | Семантический кеш снижает inference время без потери качества | cache_hit_rate + accuracy |
| E31v2 ContinuousLoop | Обратная связь по домену улучшает accuracy за несколько раундов | learning_slope |
| E44v2 DatasetWeighter | loss × 10 на security увеличивает security-accuracy без регрессии | security_accuracy + overall |
| E45v2 CurriculumOrganizer | Security примеры последними увеличивают security-accuracy (recency bias) | final_security_accuracy |

---

---

# ProbeNeuralNetwork — общая инфраструктура

## Зачем

PatternClassifier (E18) — это уже обученная нейросеть.
Но для экспериментов нужна маленькая переобучаемая сеть с открытым доступом к loss.

`ProbeNeuralNetwork` = 1-слойный линейный классификатор поверх E17-предикатов:
- Вход: предикатный вектор (12 измерений, по одному на каждый предикат)
- Один скрытый слой: 32 нейрона, ReLU
- Выход: N классов (domain / predicate / stop-signal)
- Loss: CrossEntropy
- Оптимизатор: SGD (momentum=0.9)
- Реализация: чистый C# без TorchSharp (матричные операции из 33.MatrixAlgorithms)

```csharp
// ProbeNeuralNetwork.cs — переиспользуется всеми E28v2-E45v2
// Путь: 35.ExperimentsV2/Infrastructure/ProbeNeuralNetwork.cs
namespace ExperimentsV2;

// Маленькая нейросеть для проверки градиентных гипотез.
// Вход: вектор предикатов из E17 (12 измерений).
// Архитектура: Input(12) → Linear(32) → ReLU → Linear(N) → Softmax
// Обучение: SGD с momentum, поддержка весов примеров и DPO.
public sealed class ProbeNeuralNetwork
{
    private readonly int _inputSize;
    private readonly int _hiddenSize;
    private readonly int _outputSize;

    // Веса слоя 1: [hiddenSize × inputSize]
    private double[][] _w1;
    // Веса слоя 2: [outputSize × hiddenSize]
    private double[][] _w2;
    // Скорость обучения
    private readonly double _lr;
    // Momentum буферы
    private double[][] _m1, _m2;

    public ProbeNeuralNetwork(int inputSize, int outputSize,
        int hiddenSize = 32, double lr = 0.01)
    {
        _inputSize  = inputSize;
        _hiddenSize = hiddenSize;
        _outputSize = outputSize;
        _lr         = lr;

        // Xavier инициализация
        double scale1 = Math.Sqrt(2.0 / inputSize);
        double scale2 = Math.Sqrt(2.0 / hiddenSize);
        _w1 = InitWeights(hiddenSize, inputSize, scale1);
        _w2 = InitWeights(outputSize, hiddenSize, scale2);
        _m1 = ZeroMatrix(hiddenSize, inputSize);
        _m2 = ZeroMatrix(outputSize, hiddenSize);
    }

    // Predict: вернуть класс с максимальной вероятностью.
    public int Predict(double[] input)
    {
        var (_, probs) = Forward(input);
        return Array.IndexOf(probs, probs.Max());
    }

    // Train один пример с заданным весом (1.0 = обычный, 10.0 = критичный).
    public void TrainStep(double[] input, int label, double sampleWeight = 1.0)
    {
        var (hidden, probs) = Forward(input);
        Backward(input, hidden, probs, label, sampleWeight);
    }

    // TrainDpo: обучить на DPO паре (chosen лучше rejected).
    // loss = -log(P(chosen)) + log(P(rejected)) × beta
    public void TrainDpoStep(double[] chosenInput, double[] rejectedInput,
        int chosenLabel, int rejectedLabel, double beta = 0.1)
    {
        // Шаг на chosen с весом 1.0
        TrainStep(chosenInput, chosenLabel, sampleWeight: 1.0);
        // Шаг на rejected с отрицательным градиентом
        TrainStep(rejectedInput, rejectedLabel, sampleWeight: -beta);
    }

    // Forward pass: вернуть скрытый слой и softmax вероятности.
    private (double[] Hidden, double[] Probs) Forward(double[] input)
    {
        // Слой 1: hidden = ReLU(W1 × input)
        double[] hidden = new double[_hiddenSize];
        for (int h = 0; h < _hiddenSize; h++)
        {
            double sum = 0;
            for (int i = 0; i < _inputSize; i++)
                sum += _w1[h][i] * input[i];
            hidden[h] = Math.Max(0, sum); // ReLU
        }

        // Слой 2: logits = W2 × hidden
        double[] logits = new double[_outputSize];
        for (int o = 0; o < _outputSize; o++)
        {
            double sum = 0;
            for (int h = 0; h < _hiddenSize; h++)
                sum += _w2[o][h] * hidden[h];
            logits[o] = sum;
        }

        // Softmax
        double maxLogit = logits.Max();
        double[] probs  = logits.Select(l => Math.Exp(l - maxLogit)).ToArray();
        double   sumExp = probs.Sum();
        for (int o = 0; o < _outputSize; o++) probs[o] /= sumExp;

        return (hidden, probs);
    }

    // Backward pass: обновить веса через SGD с momentum.
    private void Backward(double[] input, double[] hidden,
        double[] probs, int label, double sampleWeight)
    {
        // Градиент выхода: dL/dlogits = probs - one_hot(label)
        double[] dLogits = probs.ToArray();
        dLogits[label] -= 1.0;
        for (int o = 0; o < _outputSize; o++)
            dLogits[o] *= sampleWeight; // применить вес примера

        // Обновить W2, накопить градиент для hidden
        double[] dHidden = new double[_hiddenSize];
        for (int o = 0; o < _outputSize; o++)
            for (int h = 0; h < _hiddenSize; h++)
            {
                double grad = dLogits[o] * hidden[h];
                _m2[o][h]   = 0.9 * _m2[o][h] + _lr * grad;
                _w2[o][h]  -= _m2[o][h];
                dHidden[h] += dLogits[o] * _w2[o][h];
            }

        // ReLU backward
        for (int h = 0; h < _hiddenSize; h++)
            dHidden[h] *= hidden[h] > 0 ? 1.0 : 0.0;

        // Обновить W1
        for (int h = 0; h < _hiddenSize; h++)
            for (int i = 0; i < _inputSize; i++)
            {
                double grad = dHidden[h] * input[i];
                _m1[h][i]   = 0.9 * _m1[h][i] + _lr * grad;
                _w1[h][i]  -= _m1[h][i];
            }
    }

    private static double[][] InitWeights(int rows, int cols, double scale)
    {
        var rng = new Random(42);
        return Enumerable.Range(0, rows)
            .Select(_ => Enumerable.Range(0, cols)
                .Select(__ => (rng.NextDouble() * 2 - 1) * scale).ToArray())
            .ToArray();
    }

    private static double[][] ZeroMatrix(int rows, int cols) =>
        Enumerable.Range(0, rows)
            .Select(_ => new double[cols]).ToArray();
}
```

---

# E28v2 — ProjectAdaptor: специализация через градиентное обучение

## Гипотеза

Модель дообученная только на файлах конкретного проекта точнее
предсказывает следующий предикат для файлов этого проекта.

Ожидаемый delta: +5-15% (специализированные паттерны доминируют).

```csharp
// E28v2ProjectAdaptor.cs
// Путь: 35.ExperimentsV2/Experiments/E28v2ProjectAdaptor.cs
//
// Baseline:  ProbeNeuralNetwork обученная на ВСЕХ файлах (generic)
// Enhanced:  ProbeNeuralNetwork дообученная на файлах одного проекта (adapted)
// Метрика:   top1_accuracy на тестовых файлах того же проекта
public sealed class E28v2ProjectAdaptor
{
    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> allFiles, int tier = 1)
    {
        var predPairs = ExtractPredicatePairs(allFiles); // из E17

        // Разделить: проект A = первые 60%, тест = последние 40%
        int split      = (int)(predPairs.Count * 0.6);
        var projectA   = predPairs.Take(split).ToList();
        var testPairs  = predPairs.Skip(split).ToList();

        // ── Baseline: Generic (все файлы) ───────────────────────────────
        var generic = new ProbeNeuralNetwork(inputSize: 12, outputSize: 12);
        foreach (var (ctx, next) in predPairs)
            generic.TrainStep(ctx, next);

        int baseCorrect = testPairs.Count(p => generic.Predict(p.Context) == p.Next);
        double baseline = baseCorrect / (double)testPairs.Count;

        // ── Enhanced: Adapted (только projectA, fine-tune поверх generic) ─
        var adapted = new ProbeNeuralNetwork(inputSize: 12, outputSize: 12);
        foreach (var (ctx, next) in predPairs)   // сначала общее
            adapted.TrainStep(ctx, next);
        foreach (var (ctx, next) in projectA)    // потом fine-tune
            adapted.TrainStep(ctx, next, sampleWeight: 2.0); // двойной вес

        int enhCorrect  = testPairs.Count(p => adapted.Predict(p.Context) == p.Next);
        double enhanced = enhCorrect / (double)testPairs.Count;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result(
            ExperimentId: "E28v2", Tier: tier,
            BaselineValue: baseline, EnhancedValue: enhanced,
            MetricName: "top1_accuracy",
            Notes: $"project_a={projectA.Count}, test={testPairs.Count}"));
    }
}
```

**Gate B:** `enhanced > baseline` — специализация помогает, не мешает.

---

# E29v2 — DpoPairValue: DPO сигнал > обычный пример

## Гипотеза

Пара (chosen, rejected) даёт в 5-10x больше информации оптимизатору
чем просто один positive пример. DPO loss = -log P(chosen) + β·log P(rejected).

```csharp
// E29v2DpoPairValue.cs
// Baseline:  обучить ProbeNN только на chosen примерах
// Enhanced:  обучить ProbeNN на DPO парах (chosen + rejected)
// Метрика:   accuracy на тесте после равного числа шагов
public sealed class E29v2DpoPairValue
{
    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> allFiles, int tier = 1)
    {
        // DPO пары: chosen = файл с правильным следующим предикатом,
        //           rejected = файл с НЕПРАВИЛЬНЫМ следующим предикатом
        var (dpoPairs, testPairs) = GenerateDpoPairs(allFiles);

        // ── Baseline: только chosen (обычное обучение) ──────────────────
        var baseNet = new ProbeNeuralNetwork(inputSize: 12, outputSize: 12);
        foreach (var (chosen, _, label, _) in dpoPairs)
            baseNet.TrainStep(chosen, label);

        int baseCorrect = testPairs.Count(p => baseNet.Predict(p.Context) == p.Next);
        double baseline = baseCorrect / (double)testPairs.Count;

        // ── Enhanced: DPO пары ──────────────────────────────────────────
        var dpoNet = new ProbeNeuralNetwork(inputSize: 12, outputSize: 12);
        foreach (var (chosen, rejected, chosenLabel, rejectedLabel) in dpoPairs)
            dpoNet.TrainDpoStep(chosen, rejected, chosenLabel, rejectedLabel);

        int enhCorrect  = testPairs.Count(p => dpoNet.Predict(p.Context) == p.Next);
        double enhanced = enhCorrect / (double)testPairs.Count;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result(
            ExperimentId: "E29v2", Tier: tier,
            BaselineValue: baseline, EnhancedValue: enhanced,
            MetricName: "improvement_per_example",
            Notes: $"dpo_pairs={dpoPairs.Count}, test={testPairs.Count}"));
    }
}
```

**Gate B:** `enhanced > baseline` при том же количестве шагов обучения.

---

# E30v2 — PredicateCache: семантический кеш снижает inference время

## Гипотеза

Если два промпта имеют cosine_similarity > 0.95 в E17 пространстве,
они получат одинаковый предикт. Кеш можно использовать безопасно.

```csharp
// E30v2PredicateCache.cs
// Baseline:  ProbeNN без кеша (каждый запрос = forward pass)
// Enhanced:  ProbeNN с семантическим кешем (cosine > 0.95 = вернуть кешированный)
// Метрика:   cache_hit_rate при accuracy не хуже baseline
public sealed class E30v2PredicateCache
{
    private readonly double _similarityThreshold;

    public E30v2PredicateCache(double threshold = 0.95)
        => _similarityThreshold = threshold;

    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> allFiles, int tier = 1)
    {
        var (trainPairs, testPairs) = SplitPairs(ExtractPredicatePairs(allFiles));

        var net = new ProbeNeuralNetwork(inputSize: 12, outputSize: 12);
        foreach (var (ctx, next) in trainPairs)
            net.TrainStep(ctx, next);

        // ── Baseline: без кеша ──────────────────────────────────────────
        int baseCorrect = testPairs.Count(p => net.Predict(p.Context) == p.Next);
        double baseline = baseCorrect / (double)testPairs.Count;

        // ── Enhanced: с семантическим кешем ────────────────────────────
        var cache       = new Dictionary<int, int>(); // hash(roundedVector) → prediction
        int hits        = 0;
        int enhCorrect  = 0;

        foreach (var p in testPairs)
        {
            int cacheKey = ComputeCacheKey(p.Context, _similarityThreshold);
            int prediction;

            if (cache.TryGetValue(cacheKey, out int cached))
            {
                prediction = cached;
                hits++;
            }
            else
            {
                prediction = net.Predict(p.Context);
                cache[cacheKey] = prediction;
            }

            if (prediction == p.Next) enhCorrect++;
        }

        double enhanced  = enhCorrect / (double)testPairs.Count;
        double hitRate   = hits / (double)testPairs.Count;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result(
            ExperimentId: "E30v2", Tier: tier,
            BaselineValue: baseline, EnhancedValue: enhanced,
            MetricName: "cache_hit_rate",
            Notes: $"hit_rate={hitRate:P1}, " +
                   $"accuracy_delta={enhanced - baseline:+0.3;-0.3}"));
    }

    // Округлить вектор до заданной точности = ключ кеша.
    private static int ComputeCacheKey(double[] vec, double threshold)
    {
        double bucketSize = 1.0 - threshold; // 0.05 для threshold=0.95
        var rounded = vec.Select(v => (int)(v / bucketSize));
        return rounded.Aggregate(HashCode.Combine);
    }
}
```

**Gate B:** `hit_rate > 15%` при `accuracy_delta >= -0.01` (кеш не портит качество).

---

# E31v2 — ContinuousLoop: обратная связь улучшает модель

## Гипотеза

Маршрутизация обратной связи по домену даёт измеримый learning_slope
(accuracy растёт с каждым раундом обратной связи).

```csharp
// E31v2ContinuousLoop.cs
// Baseline:  accuracy без feedback (статичная модель)
// Enhanced:  accuracy после 5 раундов domain-routed feedback
// Метрика:   learning_slope (delta accuracy per round)
public sealed class E31v2ContinuousLoop
{
    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> allFiles, int tier = 1)
    {
        var allPairs = ExtractPredicatePairs(allFiles);
        var (initTrain, rounds) = SplitIntoRounds(allPairs, numRounds: 5);

        // ── Baseline: обучить один раз, не обновлять ─────────────────────
        var staticNet = new ProbeNeuralNetwork(inputSize: 12, outputSize: 12);
        foreach (var (ctx, next) in initTrain)
            staticNet.TrainStep(ctx, next);

        double staticAccuracy = Evaluate(staticNet, rounds.Last());

        // ── Enhanced: feedback loops ─────────────────────────────────────
        var adaptNet = new ProbeNeuralNetwork(inputSize: 12, outputSize: 12);
        foreach (var (ctx, next) in initTrain)
            adaptNet.TrainStep(ctx, next);

        var roundAccuracies = new List<double>();
        foreach (var round in rounds)
        {
            // Симулировать feedback: примеры которые модель предсказала неверно
            // получают дополнительный training step (повышенный вес)
            foreach (var (ctx, next) in round)
            {
                if (adaptNet.Predict(ctx) != next)
                    adaptNet.TrainStep(ctx, next, sampleWeight: 3.0); // корректировать
            }
            roundAccuracies.Add(Evaluate(adaptNet, round));
        }

        // learning_slope = средний прирост за раунд
        double slope = ComputeSlope(roundAccuracies);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result(
            ExperimentId: "E31v2", Tier: tier,
            BaselineValue: staticAccuracy,
            EnhancedValue: roundAccuracies.Last(),
            MetricName: "learning_slope",
            Notes: $"slope={slope:+0.4;-0.4}_per_round, " +
                   $"rounds={string.Join(",", roundAccuracies.Select(a => a.ToString("P1")))}"));
    }

    private static double ComputeSlope(IReadOnlyList<double> accuracies)
    {
        if (accuracies.Count < 2) return 0;
        return (accuracies.Last() - accuracies.First()) / (accuracies.Count - 1);
    }
}
```

**Gate B:** `learning_slope > 0` — каждый раунд обратной связи помогает.

---

# E44v2 — DatasetWeighter: weight × 10 на security

## Гипотеза

`loss × 10` на security примерах увеличивает security-accuracy
без потери общей accuracy (recency bias компенсирует дисбаланс данных).

```csharp
// E44v2DatasetWeighter.cs
// Baseline:  обучить ProbeNN, все примеры weight=1.0
// Enhanced:  обучить ProbeNN, security примеры weight=10.0
// Метрика:   weighted_f1 (security recall × 2, overall accuracy × 1)
public sealed class E44v2DatasetWeighter
{
    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> allFiles, int tier = 1)
    {
        var (trainPairs, testPairs) = SplitWeighted(allFiles);

        // ── Baseline: равные веса ───────────────────────────────────────
        var baseNet = new ProbeNeuralNetwork(inputSize: 12, outputSize: 12);
        foreach (var (ctx, next, _) in trainPairs)
            baseNet.TrainStep(ctx, next, sampleWeight: 1.0);

        double baseSecurityF1 = EvaluateSecurityF1(baseNet, testPairs);
        double baseOverall    = EvaluateOverall(baseNet, testPairs);

        // ── Enhanced: security weight × 10 ─────────────────────────────
        var wgtNet = new ProbeNeuralNetwork(inputSize: 12, outputSize: 12);
        foreach (var (ctx, next, isSecurity) in trainPairs)
            wgtNet.TrainStep(ctx, next, sampleWeight: isSecurity ? 10.0 : 1.0);

        double enhSecurityF1 = EvaluateSecurityF1(wgtNet, testPairs);
        double enhOverall    = EvaluateOverall(wgtNet, testPairs);

        // weighted_f1 = security_f1 × 2 + overall × 1 (security важнее)
        double baseWeighted = baseSecurityF1 * 2 + baseOverall;
        double enhWeighted  = enhSecurityF1  * 2 + enhOverall;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result(
            ExperimentId: "E44v2", Tier: tier,
            BaselineValue: baseWeighted, EnhancedValue: enhWeighted,
            MetricName: "weighted_f1",
            Notes: $"security: {baseSecurityF1:P1}→{enhSecurityF1:P1}, " +
                   $"overall: {baseOverall:P1}→{enhOverall:P1}"));
    }
}
```

**Gate B:** `security_f1 enhanced > security_f1 baseline` при `overall_accuracy delta >= -0.02`.

---

# E45v2 — CurriculumOrganizer: security последними

## Гипотеза

Нейросети имеют recency bias: последние примеры влияют сильнее.
Security примеры последними = усиление без изменения весов.

```csharp
// E45v2CurriculumOrganizer.cs
// Baseline:  случайный порядок (security примеры равномерно распределены)
// Enhanced:  curriculum: сначала CODE_Q/BUG/PLAN, потом security ПОСЛЕДНИМИ
// Метрика:   final_security_accuracy (точность на security тестах)
public sealed class E45v2CurriculumOrganizer
{
    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> allFiles, int tier = 1)
    {
        var (allTrain, testPairs) = SplitCurriculum(allFiles);

        // ── Baseline: случайный порядок ─────────────────────────────────
        var shuffled = allTrain.OrderBy(_ => HashCode.Combine(_.Context)).ToList();
        var baseNet  = new ProbeNeuralNetwork(inputSize: 12, outputSize: 12);
        foreach (var (ctx, next, _) in shuffled)
            baseNet.TrainStep(ctx, next);

        double baseSecAcc = EvaluateSecurityAccuracy(baseNet, testPairs);

        // ── Enhanced: curriculum (security последними) ──────────────────
        var nonSecurity = allTrain.Where(p => !p.IsSecurity).ToList();
        var security    = allTrain.Where(p => p.IsSecurity).ToList();

        var curriculum = nonSecurity.Concat(security).ToList(); // security ПОСЛЕДНИЕ

        var currNet = new ProbeNeuralNetwork(inputSize: 12, outputSize: 12);
        foreach (var (ctx, next, _) in curriculum)
            currNet.TrainStep(ctx, next);

        double enhSecAcc = EvaluateSecurityAccuracy(currNet, testPairs);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result(
            ExperimentId: "E45v2", Tier: tier,
            BaselineValue: baseSecAcc, EnhancedValue: enhSecAcc,
            MetricName: "final_security_accuracy",
            Notes: $"security_train={security.Count}, " +
                   $"non_security_train={nonSecurity.Count}"));
    }
}
```

**Gate B:** `enhanced > baseline` — curriculum порядок помогает для редкого домена.

---

## Добавить все v2 в Dispatcher

```csharp
// ExperimentDispatcher: добавить ProbeNeuralNetwork эксперименты
new E28v2ProjectAdaptor().Run(allFiles, tier),
new E29v2DpoPairValue().Run(allFiles, tier),
new E30v2PredicateCache().Run(allFiles, tier),
new E31v2ContinuousLoop().Run(allFiles, tier),
new E44v2DatasetWeighter().Run(allFiles, tier),
new E45v2CurriculumOrganizer().Run(allFiles, tier),
```

---

## Ожидаемые результаты

| Эксперимент | Ожидаемый delta | Почему |
|---|---|---|
| E28v2 | +5-15% | специализированные паттерны проекта доминируют |
| E29v2 | +10-20% | DPO даёт информацию о том что НЕ предсказывать |
| E30v2 | hit_rate > 15%, accuracy delta ≥ -1% | разработчики повторяют похожие запросы |
| E31v2 | slope > 0 | каждый раунд обратной связи сдвигает веса в нужную сторону |
| E44v2 | security_f1 +10-30%, overall delta ≥ -2% | 50 × 10 = 500 эффективных примеров |
| E45v2 | security_acc +5-15% | recency bias подтверждён нейросетями |

**Время:** 3-4 дня (инфраструктура ProbeNeuralNetwork + 6 экспериментов).
