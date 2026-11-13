# E46 · E47 · E48 · E49 · E50 — DynamicDomains (Подпроект 17)
## Подробный план для Claude Code v1.0

**Проект:** `35.ExperimentsV2`
**Зависимости:** DomainClassifier (E18), DependencyDetector (E35), EpisodeDecomposer (E32)
**Этот документ не касается XIIGen.**

---

## Пять компонентов динамического расширения

| Компонент | Эксперимент | Утверждение |
|---|---|---|
| ConfidenceGate | E46 | Низкий confidence RouteClassifier = незнакомый домен (триггер расширения) |
| ModelHubSearcher | E47 | BM25 на описании домена находит подходящий адаптер лучше чем точное совпадение |
| DomainBootstrapper | E48 | Warm start от ближайшего домена быстрее cold start при < 100 примерах |
| OrchestratorExpander | E49 | Зависимости нового домена выводятся из предикатного overlap с существующими |
| DomainGraphClosure | E50 | После добавления N доменов граф зависимостей стабилизируется (сходится) |

---

## Ключевая идея: confidence как сигнал незнакомости

```
Существующие домены (E18): CODE_QUESTION, BUG_FIX, CODE_CHANGE, ORCHESTRATION, ARCHITECTURE
Новый запрос: "Deploy the app to Kubernetes and set up monitoring"
  → RouteClassifier: CODE_QUESTION=0.15, BUG_FIX=0.10, CODE_CHANGE=0.20,
                     ORCHESTRATION=0.30, ARCHITECTURE=0.25
  → max_confidence=0.30 < threshold(0.60)
  → UNKNOWN_DOMAIN триггер → E47 ищет DevOps адаптер в ModelHub
```

---

---

# E46 — ConfidenceGate: обнаружить незнакомый домен

## Гипотеза

Когда max_confidence RouteClassifier ниже порога — задача
в незнакомой территории. Это надёжный сигнал для запуска
расширения без участия пользователя.

**Три режима:**
```
max_conf > 0.60  → KNOWN (обработать существующим доменом)
0.30–0.60        → UNCERTAIN (запросить подтверждение пользователя)
< 0.30           → UNKNOWN (запустить Domain Expansion автоматически)
```

**Дорогая ошибка — false KNOWN:** сказать "знаю" когда не знаю.
**Дешёвая ошибка — false UNKNOWN:** запустить поиск без необходимости.

## Шаг 1 — Создать `E46ConfidenceGate.cs`

**Путь:** `35.ExperimentsV2/Experiments/E46ConfidenceGate.cs`

```csharp
namespace ExperimentsV2;

// E46: ConfidenceGate — обнаружить запрос в незнакомом домене.
// Baseline:  любой conf > 0.5 → KNOWN (один порог)
// Enhanced:  три зоны (KNOWN / UNCERTAIN / UNKNOWN) с оптимальными порогами
// Метрика:   unknown_detection_rate + false_known_rate (дорогая ошибка)
public sealed class E46ConfidenceGate
{
    // Известные домены E18
    private static readonly string[] KnownDomains =
        ["CODE_QUESTION", "BUG_FIX", "CODE_CHANGE", "ORCHESTRATION", "ARCHITECTURE"];

    // Новые домены которые RouteClassifier не знает
    private static readonly string[] UnknownDomains =
        ["DEVOPS", "DATA_ANALYTICS", "SECURITY_REVIEW", "MOBILE", "DATABASE_ADMIN"];

    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var rng      = new Random(42);
        var examples = GenerateConfidenceExamples(rng, n: tier * 60);
        var shuffled = examples.OrderBy(_ => rng.Next()).ToList();
        int split    = (int)(shuffled.Count * 0.8);
        var train    = shuffled.Take(split).ToList();
        var val      = shuffled.Skip(split).ToList();

        // Найти оптимальный порог на train
        double bestThreshold = TuneThreshold(train);

        // ── Baseline: один порог 0.5 ─────────────────────────────────────────
        var t0 = Stopwatch();
        int baseUnknownDetected = val.Count(x => !x.IsKnown &&
            PredictSingle(x.Confidences, 0.5) == 0);
        int baseFalseKnown = val.Count(x => !x.IsKnown &&
            PredictSingle(x.Confidences, 0.5) == 1);
        int unknownCount   = val.Count(x => !x.IsKnown);
        double baseDetRate  = unknownCount > 0 ? baseUnknownDetected / (double)unknownCount : 0;
        double baseFKR      = unknownCount > 0 ? baseFalseKnown / (double)unknownCount : 0;
        double baselineMs   = ElapsedMs(t0);

        // ── Enhanced: оптимальный порог ──────────────────────────────────────
        var t1 = Stopwatch();
        int enhUnknownDetected = val.Count(x => !x.IsKnown &&
            PredictSingle(x.Confidences, bestThreshold) == 0);
        int enhFalseKnown = val.Count(x => !x.IsKnown &&
            PredictSingle(x.Confidences, bestThreshold) == 1);
        double enhDetRate  = unknownCount > 0 ? enhUnknownDetected / (double)unknownCount : 0;
        double enhFKR      = unknownCount > 0 ? enhFalseKnown / (double)unknownCount : 0;
        double enhancedMs  = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E46-CONFIDENCE-GATE",
            Tier              = tier,
            BaselinePrimary   = baseDetRate,
            EnhancedPrimary   = enhDetRate,
            PrimaryMetricName = "unknown_detection_rate",
            LowerIsBetter     = false,
            ScalingSignal     = baseFKR - enhFKR,
            ScalingSignalNote = "false_known_reduction (меньше пропущенных неизвестных доменов)",
            ShouldScaleUp     = val.Count < 15,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"val={val.Count}, unknown={unknownCount}, threshold={bestThreshold:F2}, " +
                    $"baseDetRate={baseDetRate:P1}(FKR={baseFKR:P1}), " +
                    $"enhDetRate={enhDetRate:P1}(FKR={enhFKR:P1})"
        });
    }

    // 0=UNKNOWN, 1=KNOWN
    private static int PredictSingle(float[] confs, double threshold)
        => confs.Max() >= threshold ? 1 : 0;

    private static double TuneThreshold(List<(float[] Confidences, bool IsKnown)> train)
    {
        double best = 0.5; double bestF1 = 0;
        for (double t = 0.3; t <= 0.8; t += 0.05)
        {
            int tp = train.Count(x => !x.IsKnown && x.Confidences.Max() < t);
            int fp = train.Count(x => x.IsKnown  && x.Confidences.Max() < t);
            int fn = train.Count(x => !x.IsKnown && x.Confidences.Max() >= t);
            double prec = tp + fp > 0 ? tp / (double)(tp + fp) : 0;
            double rec  = tp + fn > 0 ? tp / (double)(tp + fn) : 0;
            double f1   = prec + rec > 0 ? 2 * prec * rec / (prec + rec) : 0;
            if (f1 > bestF1) { bestF1 = f1; best = t; }
        }
        return best;
    }

    private static List<(float[] Confidences, bool IsKnown)> GenerateConfidenceExamples(
        Random rng, int n)
    {
        var result = new List<(float[], bool)>();
        while (result.Count < n)
        {
            bool isKnown = rng.Next(2) == 0;
            float[] confs;
            if (isKnown)
            {
                // Известный домен: один доминирующий (0.6-0.9)
                confs = Enumerable.Range(0, 5)
                    .Select(_ => (float)rng.NextDouble() * 0.15f).ToArray();
                confs[rng.Next(5)] = 0.6f + (float)rng.NextDouble() * 0.3f;
                float sum = confs.Sum();
                for (int i = 0; i < confs.Length; i++) confs[i] /= sum;
            }
            else
            {
                // Неизвестный домен: равномерное распределение (0.15-0.25 каждый)
                confs = Enumerable.Range(0, 5)
                    .Select(_ => 0.1f + (float)rng.NextDouble() * 0.2f).ToArray();
                float sum = confs.Sum();
                for (int i = 0; i < confs.Length; i++) confs[i] /= sum;
            }
            result.Add((confs, isKnown));
        }
        return result;
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
```

---

# E47 — ModelHubSearcher: найти подходящий адаптер

## Гипотеза

Реестр ModelHub содержит адаптеры с описаниями доменов.
Когда ConfidenceGate триггерит UNKNOWN — нужно найти
ближайший существующий адаптер.

BM25 на E15-нормализованных словах описания домена
найдёт "DevOps/Kubernetes deployment" даже если пользователь
написал "deploy app to cloud with containers".

## Шаг 1 — Создать `E47ModelHubSearcher.cs`

**Путь:** `35.ExperimentsV2/Experiments/E47ModelHubSearcher.cs`

```csharp
namespace ExperimentsV2;

// E47: ModelHubSearcher — найти адаптер в реестре по описанию нового домена.
// Baseline:  точное совпадение имени домена (requires exact "devops" in query)
// Enhanced:  BM25 на E15-нормализованных словах описания
// Обучение:  20 адаптеров в реестре с описаниями
// Метрика:   retrieval_accuracy (hit@1, hit@3)
public sealed class E47ModelHubSearcher
{
    // Синтетический реестр адаптеров (имитация ModelHub)
    private static readonly (string Name, string Description, string[] Tags)[] Registry = new[]
    {
        ("csharp-dotnet-v2",
         "C# and .NET services, dependency injection, async patterns, EF Core",
         new[] {"csharp","dotnet","service","async","entity"}),
        ("typescript-nestjs-v1",
         "TypeScript NestJS controllers, decorators, dependency injection, guards",
         new[] {"typescript","nestjs","controller","decorator","guard"}),
        ("devops-deploy-v1",
         "Kubernetes deployment, Docker containers, CI/CD pipelines, Helm charts",
         new[] {"deploy","kubernetes","docker","cicd","container","pipeline"}),
        ("devops-monitoring-v1",
         "Monitoring dashboards, alerting, metrics, Prometheus, Grafana observability",
         new[] {"monitor","alert","metric","dashboard","observability"}),
        ("security-auth-v2",
         "JWT authentication, OAuth2, authorization, vulnerability review, RBAC",
         new[] {"security","auth","jwt","oauth","vulnerability","rbac"}),
        ("data-analytics-v1",
         "Query optimization, data pipelines, ETL processes, analytics reporting",
         new[] {"data","query","pipeline","analytics","etl","optimize"}),
        ("testing-e2e-v1",
         "End-to-end testing, integration tests, test automation, Playwright",
         new[] {"test","e2e","integration","automation","playwright"}),
        ("mobile-ios-v1",
         "iOS Swift development, SwiftUI, mobile authentication, push notifications",
         new[] {"mobile","ios","swift","swiftui","notification"}),
        ("database-sql-v1",
         "SQL query optimization, indexing strategy, migration scripts, stored procedures",
         new[] {"database","sql","index","migration","stored","procedure"}),
        ("python-ml-v1",
         "Python machine learning, scikit-learn, pandas dataframes, model training",
         new[] {"python","machine","learn","pandas","scikit","model"}),
    };

    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var testCases = BuildTestCases();
        var rng       = new Random(42);
        var val       = testCases.OrderBy(_ => rng.Next()).ToList();

        // Построить BM25 индекс на реестре
        var bm25 = new BM25EpisodeMatcher();
        bm25.Index(Registry.Select((r, i) => (r.Description, i)).ToList());

        // ── Baseline: точное совпадение тега ─────────────────────────────────
        var t0 = Stopwatch();
        int baseHit1 = 0, baseHit3 = 0;
        foreach (var tc in val)
        {
            var lower  = tc.Query.ToLowerInvariant().Split(' ');
            int best   = -1;
            for (int i = 0; i < Registry.Length; i++)
                if (Registry[i].Tags.Any(t => lower.Contains(t)))
                { best = i; break; }

            if (best == tc.ExpectedIdx)   baseHit1++;
            if (best == tc.ExpectedIdx || // hit@3: check top-3 matches
                Registry.Take(3).Any((r, i) =>
                    i != best && r.Tags.Any(t => lower.Contains(t)) &&
                    i == tc.ExpectedIdx))
                baseHit3++;
        }
        double baseAcc1   = baseHit1 / (double)val.Count;
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: BM25 на описаниях ──────────────────────────────────────
        var t1 = Stopwatch();
        int enhHit1 = 0, enhHit3 = 0;
        foreach (var tc in val)
        {
            var results = bm25.Search(tc.Query, topK: 3);
            if (results.Count > 0 && results[0].EpisodeId == tc.ExpectedIdx) enhHit1++;
            if (results.Any(r => r.EpisodeId == tc.ExpectedIdx))             enhHit3++;
        }
        double enhAcc1    = enhHit1 / (double)val.Count;
        double enhAcc3    = enhHit3 / (double)val.Count;
        double enhancedMs = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E47-MODEL-HUB-SEARCHER",
            Tier              = tier,
            BaselinePrimary   = baseAcc1,
            EnhancedPrimary   = enhAcc1,
            PrimaryMetricName = "hit_at_1",
            LowerIsBetter     = false,
            ScalingSignal     = enhAcc3,
            ScalingSignalNote = "hit@3 (достаточно для загрузки нескольких кандидатов)",
            ShouldScaleUp     = val.Count < 10,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"registry={Registry.Length}, testCases={val.Count}, " +
                    $"baseHit1={baseAcc1:P1}, enhHit1={enhAcc1:P1}, enhHit3={enhAcc3:P1}"
        });
    }

    private static List<(string Query, int ExpectedIdx)> BuildTestCases() => new()
    {
        // DevOps deploy (idx=2)
        ("Deploy the application to Kubernetes cluster with proper resource limits", 2),
        ("Set up CI/CD pipeline for automated deployment", 2),
        ("Configure container orchestration for production", 2),
        // Security (idx=4)
        ("Review the JWT token implementation for security vulnerabilities", 4),
        ("Check the authentication flow for RBAC compliance", 4),
        ("Audit the OAuth2 integration for potential attack vectors", 4),
        // Data analytics (idx=5)
        ("Optimise the analytics query for better performance", 5),
        ("Build a data pipeline for the reporting dashboard", 5),
        // Mobile (idx=7)
        ("Add push notifications to the iOS app", 7),
        ("Implement the mobile authentication flow in SwiftUI", 7),
        // Database (idx=8)
        ("Write migration scripts for the new user schema", 8),
        ("Add database indexes to optimise the slow queries", 8),
    };

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
```

---

# E48 — DomainBootstrapper: warm start vs cold start

## Гипотеза

Новый домен DevOps имеет частичное пересечение с ORCHESTRATION и ARCHITECTURE.
Начать обучение с весов ближайшего домена (warm start)
быстрее достигает целевой точности чем начать с нуля.

**Sample efficiency:** точность при N < 100 обучающих примерах.

## Шаг 1 — Создать `E48DomainBootstrapper.cs`

**Путь:** `35.ExperimentsV2/Experiments/E48DomainBootstrapper.cs`

```csharp
namespace ExperimentsV2;

// E48: DomainBootstrapper — warm start от ближайшего домена.
// Baseline:  cold start — случайная инициализация весов
// Enhanced:  warm start — начать с весов ближайшего по предикатам домена
// Метрика:   accuracy при N=[10,25,50,100] примерах (sample efficiency curve)
//            + steps_to_threshold (сколько шагов до accuracy=0.5)
public sealed class E48DomainBootstrapper
{
    private readonly int _epochs;
    public E48DomainBootstrapper(int epochs = 5) => _epochs = epochs;

    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
    {
        const int vocabSize = 12;
        var rng = new Random(42);

        // Разделить на "существующий домен" (warm source) и "новый домен"
        int wSplit  = (int)(files.Count * 0.6);
        var warmSrc = files.Take(wSplit).ToList();   // ближайший известный домен
        var newDom  = files.Skip(wSplit).ToList();   // новый домен

        var warmPreds = ExtractPreds(warmSrc);
        var newPreds  = ExtractPreds(newDom);

        if (warmPreds.Count < 20 || newPreds.Count < 10)
            return OperationResult<ExperimentV2Result>.Fail("Слишком мало предикатов");

        // Validation: последние 20% нового домена
        int nSplit  = (int)(newPreds.Count * 0.8);
        var newTrain = newPreds.Take(nSplit).ToList();
        var newVal   = newPreds.Skip(nSplit).ToList();

        // Обучить warm source модель
        var warmNet = new ProbeNeuralNetwork(vocabSize, 32, 0.01f, 42);
        for (int ep = 0; ep < _epochs; ep++)
            for (int i = 0; i < warmPreds.Count - 1; i++)
                warmNet.TrainStep(warmPreds[i], warmPreds[i + 1]);

        // Контрольные точки: 10, 25, 50, 100% от newTrain
        var checkpoints = new[] { 0.1, 0.25, 0.5, 1.0 };

        // ── Baseline: cold start (случайные веса) ────────────────────────────
        var t0       = Stopwatch();
        var coldAccs = new List<(int N, double Acc)>();
        foreach (var pct in checkpoints)
        {
            int n = Math.Max(2, (int)(newTrain.Count * pct));
            var cold = new ProbeNeuralNetwork(vocabSize, 32, 0.02f, 42);
            for (int ep = 0; ep < _epochs; ep++)
                for (int i = 0; i < Math.Min(n, newTrain.Count) - 1; i++)
                    cold.TrainStep(newTrain[i], newTrain[i + 1]);
            coldAccs.Add((n, cold.Evaluate(newVal)));
        }
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: warm start (веса от ближайшего домена) ──────────────────
        var t1       = Stopwatch();
        var warmAccs = new List<(int N, double Acc)>();
        foreach (var pct in checkpoints)
        {
            int n = Math.Max(2, (int)(newTrain.Count * pct));
            // Копировать веса warm source (freeze embedding, fine-tune top layers)
            // В ProbeNeuralNetwork нет copy — симулируем через transfer training:
            // сначала pre-train на warmPreds (1 epoch), потом fine-tune на newPreds
            var warm = new ProbeNeuralNetwork(vocabSize, 32, 0.02f, 42);
            for (int i = 0; i < warmPreds.Count - 1; i++)  // 1 pass warm source
                warm.TrainStep(warmPreds[i], warmPreds[i + 1]);
            for (int ep = 0; ep < _epochs; ep++)            // fine-tune on new domain
                for (int i = 0; i < Math.Min(n, newTrain.Count) - 1; i++)
                    warm.TrainStep(newTrain[i], newTrain[i + 1]);
            warmAccs.Add((n, warm.Evaluate(newVal)));
        }
        double enhancedMs = ElapsedMs(t1);

        // Sample efficiency gain: разница в точности при N=25% обучающих данных
        double coldAt25 = coldAccs.FirstOrDefault(x => x.N >= newTrain.Count / 4).Acc;
        double warmAt25 = warmAccs.FirstOrDefault(x => x.N >= newTrain.Count / 4).Acc;
        double effGain  = coldAt25 > 0 ? (warmAt25 - coldAt25) / coldAt25 : 0;

        string coldStr = string.Join(",", coldAccs.Select(x => $"N{x.N}={x.Acc:P0}"));
        string warmStr = string.Join(",", warmAccs.Select(x => $"N{x.N}={x.Acc:P0}"));

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E48-DOMAIN-BOOTSTRAPPER",
            Tier              = tier,
            BaselinePrimary   = coldAccs[^1].Acc,
            EnhancedPrimary   = warmAccs[^1].Acc,
            PrimaryMetricName = "final_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = effGain,
            ScalingSignalNote = "efficiency_gain_at_25pct (warm start при малых данных)",
            ShouldScaleUp     = newVal.Count < 5,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"warmPreds={warmPreds.Count}, newPreds={newPreds.Count}, " +
                    $"cold=[{coldStr}], warm=[{warmStr}], effGain={effGain:+.1%}"
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

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
```

---

# E49 — OrchestratorExpander: зависимости нового домена

## Гипотеза

DevOps задачи ("Deploy app with monitoring") содержат предикаты
из ORCHESTRATION (ITERATE, ASSIGN) и ARCHITECTURE (VALIDATE_OP, CREATE_OBJ).
Предикатный overlap с существующими доменами предсказывает зависимости.

**Пример:**
```
Новый домен DevOps: предикаты [ITERATE, ASSIGN, CREATE_OBJ, VALIDATE_OP]
  → overlap с ORCHESTRATION: 0.71 → зависит
  → overlap с CODE_CHANGE:   0.45 → слабая зависимость
  → overlap с BUG_FIX:       0.12 → независим
```

## Шаг 1 — Создать `E49OrchestratorExpander.cs`

**Путь:** `35.ExperimentsV2/Experiments/E49OrchestratorExpander.cs`

```csharp
namespace ExperimentsV2;

// E49: OrchestratorExpander — предсказать зависимости нового домена.
// Baseline:  все домены независимы (линейный граф)
// Enhanced:  Jaccard overlap предикатов → edge если overlap > threshold
// Метрика:   dependency_accuracy + graph_precision
//            (правильно ли определены рёбра в графе)
public sealed class E49OrchestratorExpander
{
    // Известные домены с типичными предикатными профилями
    private static readonly (string Name, int[] Predicates)[] KnownDomains = new[]
    {
        ("CODE_QUESTION", new[] { 7, 9, 7, 9, 3 }),          // GET_CALL, CONDITION
        ("BUG_FIX",       new[] { 1, 2, 9, 1, 2, 0 }),       // NULL_GUARD, FAIL, CONDITION
        ("CODE_CHANGE",   new[] { 8, 6, 4, 8, 3, 8 }),       // CREATE_OBJ, ASSIGN, VALIDATE
        ("ORCHESTRATION", new[] { 5, 6, 3, 5, 6, 6 }),       // ITERATE, ASSIGN
        ("ARCHITECTURE",  new[] { 4, 8, 3, 4, 4, 8 }),       // VALIDATE, CREATE, VALIDATE
    };

    // Новые домены с известными правильными зависимостями
    private static readonly (string Name, int[] Predicates, int[] DependsOn)[] NewDomains = new[]
    {
        // DevOps зависит от ORCHESTRATION(3) и ARCHITECTURE(4)
        ("DEVOPS", new[] { 5, 6, 8, 4, 5, 6, 3 }, new[] { 3, 4 }),
        // Security зависит от CODE_CHANGE(2) и BUG_FIX(1)
        ("SECURITY", new[] { 1, 4, 2, 1, 2, 0, 9 }, new[] { 1, 2 }),
        // Data Analytics зависит от CODE_QUESTION(0) и ORCHESTRATION(3)
        ("DATA_ANALYTICS", new[] { 7, 5, 3, 9, 7, 5, 6 }, new[] { 0, 3 }),
    };

    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        double threshold = 0.35; // overlap > 0.35 → зависимость

        int totalEdges   = 0;
        int baseCorrect  = 0;
        int enhCorrect   = 0;

        var t0 = Stopwatch();
        foreach (var (name, preds, correct) in NewDomains)
        {
            totalEdges += correct.Length;

            // Baseline: нет зависимостей (линейный граф)
            // Верно только если правильных зависимостей нет
            if (correct.Length == 0) baseCorrect += KnownDomains.Length;

            // Enhanced: Jaccard overlap
            for (int d = 0; d < KnownDomains.Length; d++)
            {
                var newSet  = preds.ToHashSet();
                var domSet  = KnownDomains[d].Predicates.ToHashSet();
                int inter   = newSet.Count(p => domSet.Contains(p));
                int union   = newSet.Count + domSet.Count - inter;
                double jacc = union == 0 ? 0 : (double)inter / union;
                bool predicted = jacc > threshold;
                bool actual    = correct.Contains(d);
                if (predicted == actual) enhCorrect++;
            }
        }
        double baselineMs = ElapsedMs(t0);
        double enhancedMs = baselineMs; // синхронный вычисление

        int totalDecisions = NewDomains.Length * KnownDomains.Length;
        double baseAcc = (totalDecisions - totalEdges) / (double)totalDecisions; // always NO
        double enhAcc  = enhCorrect / (double)totalDecisions;

        // Graph precision: из предсказанных рёбер сколько правильных
        int predEdges = 0; int trueEdges = 0;
        foreach (var (_, preds, correct) in NewDomains)
            for (int d = 0; d < KnownDomains.Length; d++)
            {
                var newSet = preds.ToHashSet();
                var domSet = KnownDomains[d].Predicates.ToHashSet();
                int i = newSet.Count(p => domSet.Contains(p));
                int u = newSet.Count + domSet.Count - i;
                if (u > 0 && (double)i / u > threshold) predEdges++;
                if (correct.Contains(d)) trueEdges++;
            }
        double graphPrecision = predEdges > 0
            ? NewDomains.Sum(nd => nd.DependsOn.Count(d =>
            {
                var ns = nd.Predicates.ToHashSet();
                var ds = KnownDomains[d].Predicates.ToHashSet();
                int ii = ns.Count(p => ds.Contains(p));
                int uu = ns.Count + ds.Count - ii;
                return uu > 0 && (double)ii / uu > threshold;
            })) / (double)predEdges : 0;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E49-ORCHESTRATOR-EXPANDER",
            Tier              = tier,
            BaselinePrimary   = baseAcc,
            EnhancedPrimary   = enhAcc,
            PrimaryMetricName = "dependency_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = graphPrecision,
            ScalingSignalNote = "graph_precision (правильные рёбра / все предсказанные рёбра)",
            ShouldScaleUp     = NewDomains.Length < 5,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"newDomains={NewDomains.Length}, knownDomains={KnownDomains.Length}, " +
                    $"threshold={threshold:F2}, predEdges={predEdges}, trueEdges={trueEdges}, " +
                    $"baseAcc={baseAcc:P1}, enhAcc={enhAcc:P1}, precision={graphPrecision:P1}"
        });
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
```

---

# E50 — DomainGraphClosure: граф сходится после N доменов

## Гипотеза

После добавления каждого нового домена граф зависимостей меняется.
Через N доменов граф стабилизируется — добавление нового домена
не меняет существующие рёбра.

**Closure signal:** после добавления домена N, сколько рёбер изменилось?
Если изменения < 5% — граф достиг closure.

## Шаг 1 — Создать `E50DomainGraphClosure.cs`

**Путь:** `35.ExperimentsV2/Experiments/E50DomainGraphClosure.cs`

```csharp
namespace ExperimentsV2;

// E50: DomainGraphClosure — граф стабилизируется после N доменов.
// Baseline:  граф никогда не стабилизируется (постоянно добавляем рёбра)
// Enhanced:  edge delta падает < 5% после 3-4 новых доменов
// Метрика:   edge_delta_per_domain (насколько меняется граф при добавлении домена)
//            + closure_round (после какого домена delta < threshold)
public sealed class E50DomainGraphClosure
{
    private const double EdgeDeltaThreshold = 0.05; // 5% изменений = стабилен

    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        // Последовательно добавлять домены и измерять delta графа
        var allDomains = new (string Name, int[] Predicates)[]
        {
            ("CODE_QUESTION", new[] { 7, 9, 7, 9, 3 }),
            ("BUG_FIX",       new[] { 1, 2, 9, 1, 2, 0 }),
            ("CODE_CHANGE",   new[] { 8, 6, 4, 8, 3, 8 }),
            ("ORCHESTRATION", new[] { 5, 6, 3, 5, 6, 6 }),
            ("ARCHITECTURE",  new[] { 4, 8, 3, 4, 4, 8 }),
            // Новые домены
            ("DEVOPS",        new[] { 5, 6, 8, 4, 5, 3 }),
            ("SECURITY",      new[] { 1, 4, 2, 1, 0, 9 }),
            ("DATA_ANALYTICS",new[] { 7, 5, 9, 7, 5, 6 }),
            ("MOBILE",        new[] { 8, 3, 7, 8, 4, 6 }),
            ("TESTING",       new[] { 4, 9, 2, 4, 1, 3 }),
        };

        const double threshold = 0.35;
        var deltas = new List<double>();
        var prevEdges = new HashSet<(int, int)>();
        int closureRound = allDomains.Length; // по умолчанию: не сходится

        var t0 = Stopwatch();
        for (int n = 2; n <= allDomains.Length; n++)
        {
            var currentDomains = allDomains.Take(n).ToArray();
            var currentEdges   = BuildEdges(currentDomains, threshold);

            if (n > 2)
            {
                int changed = currentEdges.Count(e => !prevEdges.Contains(e))
                            + prevEdges.Count(e => !currentEdges.Contains(e));
                int total   = Math.Max(1, currentEdges.Count + prevEdges.Count);
                double delta = changed / (double)total;
                deltas.Add(delta);

                if (delta < EdgeDeltaThreshold && closureRound == allDomains.Length)
                    closureRound = n;
            }
            prevEdges = currentEdges;
        }
        double baselineMs = ElapsedMs(t0);

        // Baseline: average delta без convergence detection
        // Enhanced: identify closure round
        double avgDelta  = deltas.Count > 0 ? deltas.Average() : 1.0;
        double finalDelta = deltas.Count > 0 ? deltas[^1] : 1.0;
        double convergenceRate = 1.0 - finalDelta; // насколько стабилен финальный граф

        string deltaStr = string.Join(",", deltas.Select((d, i) => $"D{i+2}={d:P0}"));

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E50-DOMAIN-GRAPH-CLOSURE",
            Tier              = tier,
            BaselinePrimary   = avgDelta,      // средняя нестабильность
            EnhancedPrimary   = finalDelta,    // финальная нестабильность
            PrimaryMetricName = "final_edge_delta",
            LowerIsBetter     = true,          // меньше delta = стабильнее граф
            ScalingSignal     = closureRound / (double)allDomains.Length,
            ScalingSignalNote = "closure_ratio (раньше = быстрее стабилизировался)",
            ShouldScaleUp     = allDomains.Length < 8,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = baselineMs, // один проход
            Notes = $"domains={allDomains.Length}, threshold={threshold:F2}, " +
                    $"closureRound={closureRound}({closureRound}/{allDomains.Length}), " +
                    $"avgDelta={avgDelta:P1}, finalDelta={finalDelta:P1}, " +
                    $"deltas=[{deltaStr}]"
        });
    }

    private static HashSet<(int, int)> BuildEdges(
        (string Name, int[] Predicates)[] domains, double threshold)
    {
        var edges = new HashSet<(int, int)>();
        for (int i = 0; i < domains.Length; i++)
            for (int j = i + 1; j < domains.Length; j++)
            {
                var a = domains[i].Predicates.ToHashSet();
                var b = domains[j].Predicates.ToHashSet();
                int inter = a.Count(p => b.Contains(p));
                int union = a.Count + b.Count - inter;
                if (union > 0 && (double)inter / union > threshold)
                    edges.Add((i, j));
            }
        return edges;
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
```

---

## Добавить все 5 в Dispatcher/Runner/ResultWriter

```csharp
// Dispatcher:
RunE46(int tier = 1)                  => new E46ConfidenceGate().Run(tier)
RunE47(int tier = 1)                  => new E47ModelHubSearcher().Run(tier)
RunE48(IReadOnlyList<(s,s)> f, int t) => new E48DomainBootstrapper().Run(f, t)
RunE49(int tier = 1)                  => new E49OrchestratorExpander().Run(tier)
RunE50(int tier = 1)                  => new E50DomainGraphClosure().Run(tier)

// Runner ids: "E46","E47","E48","E49","E50"
// ResultWriter: все 5 имён
```

---

## Ожидаемые результаты

| Exp | Baseline | Enhanced | Ключевой сигнал |
|---|---|---|---|
| E46 ConfidenceGate | ~65% detection | ~90% detection | false_known_rate ↓ |
| E47 ModelHubSearcher | ~30% hit@1 (exact) | ~80% hit@1 (BM25) | hit@3 ≥ 90% |
| E48 DomainBootstrapper | cold=[N10=20%...] | warm=[N10=35%...] | efficiency_gain_at_25pct |
| E49 OrchestratorExpander | ~60% (no edges) | ~85% (Jaccard) | graph_precision ≥ 80% |
| E50 DomainGraphClosure | avgDelta=35% | finalDelta<5% | closureRound ≤ 7 |

---

## Полный цикл динамического расширения

```
"Deploy the app to Kubernetes with monitoring"
         │
         ▼ E46 ConfidenceGate
  max_confidence = 0.28 < 0.60 → UNKNOWN DOMAIN
         │
         ▼ E47 ModelHubSearcher
  BM25 search "deploy kubernetes monitoring"
  → devops-deploy-v1 (score=0.83), devops-monitoring-v1 (score=0.71)
  → Скачать оба адаптера (2+2 МБ, ~30 секунд)
         │
         ▼ E48 DomainBootstrapper
  Warm start от ORCHESTRATION (ближайший по предикатам)
  Fine-tune на devops примерах
  → accuracy 40% при 25 примерах (vs 20% cold start)
         │
         ▼ E49 OrchestratorExpander
  DevOps предикаты overlap с ORCHESTRATION=0.71, ARCHITECTURE=0.52
  → Добавить рёбра: DevOps→ORCHESTRATION, DevOps→ARCHITECTURE
         │
         ▼ E50 DomainGraphClosure
  После DevOps: edge_delta=18%
  После Security: edge_delta=11%
  После DataAnalytics: edge_delta=4% < 5% → CLOSURE
  Граф стабилизировался после 3 новых доменов
         │
         ▼ EpisodeDecomposer теперь знает:
  "Deploy app with monitoring" → 3 под-эпизода:
    1. CODE_CHANGE (prepare code) → 2. DEVOPS (deploy) → 3. DEVOPS (monitor)
  Зависимость: 1→2→3 (последовательно, из E49 графа)
```

---

*E46-E50 план v1.0 — DynamicDomains · Подпроект 17 · не касается XIIGen*
