# E46 · E47 · E48 · E49 · E50 — Динамическое расширение доменов
## Подробный план для Claude Code v1.0

**Проект:** `35.ExperimentsV2`
**Зависимости:** DomainClassifier (E18), EpisodeDecomposer (E32), SkillRetriever (E38v2)
**Этот документ не касается XIIGen.**

---

## Пять компонентов динамического расширения

| Компонент | Эксперимент | Утверждение |
|---|---|---|
| ConfidenceGate | E46 | Порог confidence 0.6 надёжно отделяет известные домены от новых |
| ModelHubSearcher | E47 | Поиск адаптера в ModelHub точнее чем поиск по ключевым словам |
| DomainBootstrapper | E48 | Минимального синтетического датасета достаточно для первичного обучения нового домена |
| OrchestratorExpander | E49 | Добавление нового домена автоматически расширяет граф зависимостей |
| TransferLearner | E50 | Fine-tune от ближайшего домена сходится быстрее чем обучение с нуля |

---

---

# E46 — ConfidenceGate: когда задача в новом домене?

## Гипотеза

RouteClassifier возвращает `max_confidence` для самого вероятного домена.
Порог 0.6 надёжно разделяет:
- `max_confidence ≥ 0.6` → известный домен, ответить напрямую
- `max_confidence < 0.6` → новый домен, запустить расширение

**Проверяемое утверждение:** порог 0.6 имеет F1 > 0.85 для классификации
"известный vs новый домен" на тестовом наборе смешанных запросов.

```csharp
// E46ConfidenceGate.cs
// Путь: 35.ExperimentsV2/Experiments/E46ConfidenceGate.cs
//
// Baseline:  всегда отвечать напрямую (нет gate)
// Enhanced:  gate по max_confidence < 0.6
// Метрика:   gate_f1 (precision и recall детекции новых доменов)
public sealed class E46ConfidenceGate
{
    private readonly double _threshold;
    public E46ConfidenceGate(double threshold = 0.6) => _threshold = threshold;

    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        // Тестовый набор: 50% известные домены + 50% новые домены
        // Известные: CODE_Q, BUG, PLAN, ARCH, REFACTOR (из E18)
        // Новые: DEVOPS, SQL, MOBILE, SECURITY, TESTING (не виданные при обучении)
        var testCases = GenerateMixedDomainCases(
            knownCount: 50, newCount: 50, seed: 7);

        // Обучить RouteClassifier только на известных доменах
        var classifier = TrainRouteClassifier(knownDomainsOnly: true);

        // ── Baseline: нет gate, всегда отвечаем ─────────────────────────
        // Baseline accuracy = на известных доменах всё хорошо,
        // на новых доменах отвечает неверно (не знает домена)
        int baseCorrect = testCases.Count(tc =>
            tc.IsKnown && classifier.Classify(tc.Prompt).MaxConfidence >= 0.5);
        double baseline = baseCorrect / (double)testCases.Count;

        // ── Enhanced: gate по порогу ─────────────────────────────────────
        int truePositive  = 0; // новый домен → gate сработал (правильно)
        int falsePositive = 0; // известный домен → gate сработал (ошибка)
        int trueNegative  = 0; // известный домен → gate не сработал (правильно)
        int falseNegative = 0; // новый домен → gate не сработал (пропустил)

        foreach (var tc in testCases)
        {
            var result   = classifier.Classify(tc.Prompt);
            bool gated   = result.MaxConfidence < _threshold; // gate сработал?
            bool isNew   = !tc.IsKnown;

            if (isNew && gated)      truePositive++;   // правильно обнаружили новый
            else if (!isNew && gated) falsePositive++; // ложная тревога
            else if (!isNew && !gated) trueNegative++; // правильно пропустили
            else                      falseNegative++; // пропустили новый (плохо)
        }

        double precision = truePositive / (double)(truePositive + falsePositive + 1e-9);
        double recall    = truePositive / (double)(truePositive + falseNegative + 1e-9);
        double gateF1    = 2 * precision * recall / (precision + recall + 1e-9);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result(
            ExperimentId: "E46", Tier: tier,
            BaselineValue: baseline, EnhancedValue: gateF1,
            MetricName: "gate_f1",
            Notes: $"threshold={_threshold}, " +
                   $"precision={precision:P1}, recall={recall:P1}, " +
                   $"tp={truePositive}, fp={falsePositive}, " +
                   $"tn={trueNegative}, fn={falseNegative}"));
    }

    // Синтетические тестовые случаи для смешанных доменов
    private static IReadOnlyList<DomainTestCase> GenerateMixedDomainCases(
        int knownCount, int newCount, int seed)
    {
        var cases = new List<DomainTestCase>();

        // Известные домены (из обучения E18)
        var knownPrompts = new[]
        {
            ("Add null check to method", true),
            ("Fix NullReferenceException", true),
            ("Design auth flow", true),
            ("Should use factory pattern?", true),
            ("Extract validation logic", true),
        };

        // Новые домены (не виданные при обучении)
        var newPrompts = new[]
        {
            ("Write Dockerfile for .NET app", false),       // DevOps
            ("Optimize SQL index for query", false),         // SQL
            ("Create SwiftUI component", false),             // Mobile
            ("Add SQL injection protection", false),         // Security
            ("Write unit test for service", false),          // Testing
        };

        var rng = new Random(seed);
        for (int i = 0; i < knownCount; i++)
        {
            var (prompt, isKnown) = knownPrompts[i % knownPrompts.Length];
            cases.Add(new DomainTestCase(
                Prompt: prompt + $" (case {i})", IsKnown: isKnown));
        }
        for (int i = 0; i < newCount; i++)
        {
            var (prompt, isKnown) = newPrompts[i % newPrompts.Length];
            cases.Add(new DomainTestCase(
                Prompt: prompt + $" (case {i})", IsKnown: isKnown));
        }

        return cases.OrderBy(_ => rng.Next()).ToList();
    }

    private sealed record DomainTestCase(string Prompt, bool IsKnown);
}
```

**Gate B:** `gate_f1 > 0.80` при threshold=0.6.
Если F1 < 0.80 → протестировать threshold 0.5 и 0.7, выбрать лучший.

---

# E47 — ModelHubSearcher: найти адаптер для нового домена

## Гипотеза

Поиск адаптера в ModelHub по предикатному профилю задачи
точнее чем поиск по ключевым словам (E34v2 паттерн повторяется).

```csharp
// E47ModelHubSearcher.cs
// Baseline:  keyword search (поиск по названию адаптера)
// Enhanced:  K-NN по предикатному профилю задачи
// Метрика:   hub_hit@1 (правильный адаптер на первом месте)
public sealed class E47ModelHubSearcher
{
    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        // ModelHub = список адаптеров с их предикатными профилями
        // Каждый адаптер описывается типичными задачами которые он решает
        var hub = BuildModelHub();

        // Тестовые запросы: задача → правильный адаптер
        var queries = new[]
        {
            ("Write Dockerfile for app",      "devops-docker"),
            ("Create k8s deployment yaml",    "devops-k8s"),
            ("Optimize SQL query indexes",    "sql-performance"),
            ("Create SwiftUI list view",      "mobile-ios"),
            ("Add OWASP security headers",    "security-web"),
            ("Write Jest unit test",          "testing-js"),
            ("Create GitHub Actions workflow","devops-ci"),
            ("Fix N+1 query problem",         "sql-performance"),
            ("Add TouchID authentication",    "mobile-ios"),
            ("Scan for SQL injection",        "security-web"),
        };

        // ── Baseline: keyword search ─────────────────────────────────────
        int baseHits = queries.Count(q =>
        {
            var results = hub
                .Where(a => a.Name.Contains(q.Item1.Split()[0].ToLower()) ||
                             a.Tags.Any(t => q.Item1.ToLower().Contains(t)))
                .OrderByDescending(a => a.Downloads)
                .ToList();
            return results.FirstOrDefault()?.Id == q.Item2;
        });
        double baseline = baseHits / (double)queries.Length;

        // ── Enhanced: K-NN по предикатному профилю ───────────────────────
        int enhHits = queries.Count(q =>
        {
            // Преобразовать запрос в предикатный вектор через E17
            double[] queryVec = ExtractPredicateVector(q.Item1);
            // Найти адаптер с ближайшим профилем
            var best = hub
                .OrderByDescending(a => CosineSimilarity(queryVec, a.PredicateProfile))
                .First();
            return best.Id == q.Item2;
        });
        double enhanced = enhHits / (double)queries.Length;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result(
            ExperimentId: "E47", Tier: tier,
            BaselineValue: baseline, EnhancedValue: enhanced,
            MetricName: "hub_hit@1",
            Notes: $"hub_size={hub.Count}, queries={queries.Length}"));
    }

    private static IReadOnlyList<AdapterEntry> BuildModelHub() => new[]
    {
        new AdapterEntry("devops-docker", "Docker containerisation",
            new[]{"docker","dockerfile","container","image"},
            PredicateProfile: new[]{0.1,0.8,0.1,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0},
            Downloads: 847),
        new AdapterEntry("devops-k8s", "Kubernetes orchestration",
            new[]{"k8s","kubernetes","deployment","pod"},
            PredicateProfile: new[]{0.1,0.7,0.2,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0},
            Downloads: 412),
        new AdapterEntry("sql-performance", "SQL query optimisation",
            new[]{"sql","query","index","performance"},
            PredicateProfile: new[]{0.0,0.2,0.5,0.3,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0},
            Downloads: 623),
        new AdapterEntry("mobile-ios", "iOS SwiftUI development",
            new[]{"swift","ios","swiftui","mobile"},
            PredicateProfile: new[]{0.3,0.4,0.2,0.0,0.1,0.0,0.0,0.0,0.0,0.0,0.0,0.0},
            Downloads: 291),
        new AdapterEntry("security-web", "Web security and OWASP",
            new[]{"security","owasp","injection","xss"},
            PredicateProfile: new[]{0.0,0.1,0.6,0.3,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0},
            Downloads: 518),
        new AdapterEntry("testing-js", "JavaScript testing",
            new[]{"test","jest","mock","assert"},
            PredicateProfile: new[]{0.2,0.3,0.3,0.2,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0},
            Downloads: 334),
        new AdapterEntry("devops-ci", "CI/CD pipelines",
            new[]{"ci","pipeline","github","actions"},
            PredicateProfile: new[]{0.1,0.6,0.2,0.1,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0},
            Downloads: 389),
    };

    private sealed record AdapterEntry(
        string Id, string Name, string[] Tags,
        double[] PredicateProfile, int Downloads);
}
```

**Gate B:** `hub_hit@1 enhanced > baseline`.

---

# E48 — DomainBootstrapper: минимальный датасет для нового домена

## Гипотеза

20-50 синтетических примеров достаточно чтобы ProbeNN начала отличать
новый домен от пяти существующих (precision > 70%).

```csharp
// E48DomainBootstrapper.cs
// Проверяет: сколько примеров нужно чтобы обучить новый домен
// Baseline:  0 примеров (новый домен = всегда CODE_Q, ближайший)
// Enhanced:  N примеров нового домена (N = 10, 20, 50, 100)
// Метрика:   new_domain_precision при N примерах
public sealed class E48DomainBootstrapper
{
    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        // Новый домен: DevOps (5 типичных задач × 4 вариации = 20 примеров)
        var newDomainExamples = GenerateDevOpsExamples(count: 50);
        var existingExamples  = GenerateKnownDomainExamples(countPerDomain: 20);
        var testSet           = GenerateTestSet();

        // ── Baseline: нет примеров нового домена ────────────────────────
        var baseNet = new ProbeNeuralNetwork(inputSize: 12, outputSize: 6); // 5 + DevOps
        foreach (var (ctx, label) in existingExamples)
            baseNet.TrainStep(ctx, label);

        double basePrecision = EvaluateNewDomainPrecision(baseNet, testSet, newDomainLabel: 5);

        // ── Enhanced: добавить N примеров DevOps ─────────────────────────
        var results = new List<(int N, double Precision)>();
        foreach (int n in new[] { 10, 20, 50 })
        {
            var enhNet = new ProbeNeuralNetwork(inputSize: 12, outputSize: 6);
            foreach (var (ctx, label) in existingExamples)
                enhNet.TrainStep(ctx, label);
            foreach (var (ctx, label) in newDomainExamples.Take(n))
                enhNet.TrainStep(ctx, label: 5); // DevOps = класс 5
            results.Add((n, EvaluateNewDomainPrecision(enhNet, testSet, newDomainLabel: 5)));
        }

        double bestPrecision = results.Max(r => r.Precision);
        int    bestN         = results.First(r => r.Precision == bestPrecision).N;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result(
            ExperimentId: "E48", Tier: tier,
            BaselineValue: basePrecision, EnhancedValue: bestPrecision,
            MetricName: "new_domain_precision",
            Notes: $"best_n={bestN}, " +
                   $"results={string.Join("|", results.Select(r => $"n={r.N}:{r.Precision:P0}"))}"));
    }
}
```

**Gate B:** `precision at N=50 > 0.70` — 50 примеров достаточно для базового распознавания.

---

# E49 — OrchestratorExpander: автоматически расширить граф зависимостей

## Гипотеза

После добавления нового домена оркестратор автоматически обнаруживает
зависимости с существующими доменами (DevOps зависит от CODE_Q и ARCH).

```csharp
// E49OrchestratorExpander.cs
// Baseline:  граф зависимостей только для 5 известных доменов
// Enhanced:  граф после добавления DevOps домена
// Метрика:   dependency_discovery_accuracy (правильно ли найдены зависимости)
public sealed class E49OrchestratorExpander
{
    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        // Эталонные зависимости (из архитектурного анализа):
        // DevOps зависит от CODE_Q (нужен код), ARCH (нужна архитектура)
        // DevOps НЕ зависит от BUG, PLAN, REFACTOR
        var groundTruth = new Dictionary<string, bool>
        {
            { "DevOps→CODE_Q", true  },
            { "DevOps→ARCH",   true  },
            { "DevOps→BUG",    false },
            { "DevOps→PLAN",   false },
            { "DevOps→REFACTOR",false },
        };

        // ── Baseline: статичный граф (нет расширения) ────────────────────
        var staticGraph  = new DependencyGraph(knownDomainsOnly: true);
        int baseCorrect  = groundTruth.Count(kv =>
            staticGraph.HasEdge(kv.Key.Split('→')[0], kv.Key.Split('→')[1]) == kv.Value);
        double baseline  = baseCorrect / (double)groundTruth.Count;

        // ── Enhanced: OrchestratorExpander автоматически находит связи ───
        var expander     = new OrchestratorExpanderProbe();
        var expandedGraph = expander.ExpandWithNewDomain(
            domainId: "DevOps",
            exampleTasks: new[]
            {
                "Write Dockerfile for the .NET application",
                "Deploy the service using Kubernetes",
                "Create CI/CD pipeline for the codebase",
            });

        // Оркестратор анализирует задачи нового домена:
        // "Dockerfile for the .NET application" → содержит "application" (CODE_Q связь)
        // "Deploy the service" → содержит "service" (ARCH связь)
        int enhCorrect   = groundTruth.Count(kv =>
            expandedGraph.HasEdge(kv.Key.Split('→')[0], kv.Key.Split('→')[1]) == kv.Value);
        double enhanced  = enhCorrect / (double)groundTruth.Count;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result(
            ExperimentId: "E49", Tier: tier,
            BaselineValue: baseline, EnhancedValue: enhanced,
            MetricName: "dependency_discovery_accuracy",
            Notes: $"edges_tested={groundTruth.Count}, " +
                   $"enhanced_correct={enhCorrect}"));
    }
}

// OrchestratorExpanderProbe: маленькая реализация для эксперимента.
// Использует co-occurrence: если слова из задачи нового домена встречаются
// в примерах существующего домена → добавить зависимость.
public sealed class OrchestratorExpanderProbe
{
    public DependencyGraph ExpandWithNewDomain(
        string domainId, IReadOnlyList<string> exampleTasks)
    {
        var graph = new DependencyGraph(knownDomainsOnly: true);

        // Ключевые слова каждого известного домена (из E18 словаря)
        var domainKeywords = new Dictionary<string, string[]>
        {
            { "CODE_Q",   new[]{"code","method","class","function","application","service"} },
            { "ARCH",     new[]{"design","architecture","deploy","structure","service","pattern"} },
            { "BUG",      new[]{"fix","error","exception","null","bug"} },
            { "PLAN",     new[]{"plan","phase","design","implement","flow"} },
            { "REFACTOR", new[]{"refactor","clean","extract","rename","split"} },
        };

        // Для каждого существующего домена проверить overlap с задачами нового
        foreach (var (existingDomain, keywords) in domainKeywords)
        {
            int overlap = exampleTasks
                .Sum(task => keywords.Count(kw =>
                    task.ToLower().Contains(kw)));

            if (overlap >= 2) // порог: ≥2 совпадения → зависимость
                graph.AddEdge(domainId, existingDomain);
        }

        return graph;
    }
}
```

**Gate B:** `dependency_discovery_accuracy > 0.80` — 4 из 5 зависимостей определены верно.

---

# E50 — TransferLearner: fine-tune от ближайшего домена

## Гипотеза

ProbeNN инициализированная весами ближайшего домена сходится быстрее
чем ProbeNN инициализированная случайно (меньше эпох до того же качества).

```csharp
// E50TransferLearner.cs
// Baseline:  обучить ProbeNN с нуля на DevOps данных
// Enhanced:  fine-tune ProbeNN инициализированной весами CODE_Q (ближайший домен)
// Метрика:   epochs_to_threshold (сколько эпох до accuracy 70%)
public sealed class E50TransferLearner
{
    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var devOpsExamples = GenerateDevOpsExamples(count: 50);
        var testSet        = GenerateDevOpsTestSet(count: 20);
        const double TARGET_ACCURACY = 0.70;

        // ── Baseline: обучение с нуля ─────────────────────────────────────
        var fromScratch = new ProbeNeuralNetwork(inputSize: 12, outputSize: 6);
        int baseEpochs  = TrainUntilThreshold(fromScratch, devOpsExamples,
                              testSet, TARGET_ACCURACY, maxEpochs: 200);

        double baseAccuracy = Evaluate(fromScratch, testSet);

        // ── Enhanced: transfer learning от CODE_Q ────────────────────────
        // Предобучить на CODE_Q данных (ближайший домен)
        var codQNet = new ProbeNeuralNetwork(inputSize: 12, outputSize: 6);
        var codeQExamples = GenerateCodeQExamples(count: 100);
        foreach (var (ctx, label) in codeQExamples)
            codQNet.TrainStep(ctx, label);

        // Fine-tune: начать с весов CODE_Q, обучить на DevOps
        int enhEpochs = TrainUntilThreshold(codQNet, devOpsExamples,
                            testSet, TARGET_ACCURACY, maxEpochs: 200);
        double enhAccuracy = Evaluate(codQNet, testSet);

        // Сравнивать: сколько эпох нужно baseline vs enhanced для того же качества
        // Меньше = быстрее = transfer learning работает
        double speedup = baseEpochs / (double)Math.Max(enhEpochs, 1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result(
            ExperimentId: "E50", Tier: tier,
            BaselineValue: baseEpochs,       // эпохи до 70%
            EnhancedValue: enhEpochs,         // эпохи с transfer learning
            MetricName: "epochs_to_threshold",
            Notes: $"target={TARGET_ACCURACY:P0}, " +
                   $"speedup={speedup:F1}x, " +
                   $"base_acc={baseAccuracy:P1}, enh_acc={enhAccuracy:P1}"));
    }

    // Обучать пока accuracy < threshold или не достигнут maxEpochs.
    // Вернуть количество эпох до достижения порога.
    private static int TrainUntilThreshold(
        ProbeNeuralNetwork net,
        IReadOnlyList<(double[] Ctx, int Label)> train,
        IReadOnlyList<(double[] Ctx, int Label)> test,
        double threshold, int maxEpochs)
    {
        for (int epoch = 1; epoch <= maxEpochs; epoch++)
        {
            foreach (var (ctx, label) in train)
                net.TrainStep(ctx, label);

            double acc = Evaluate(net, test);
            if (acc >= threshold)
                return epoch;
        }
        return maxEpochs; // не достиг порога
    }
}
```

**Gate B:** `enhanced_epochs < baseline_epochs` — transfer learning сходится быстрее.

---

## Добавить E46-E50 в Dispatcher

```csharp
// ExperimentDispatcher: добавить динамические домены
new E46ConfidenceGate().Run(tier),
new E47ModelHubSearcher().Run(tier),
new E48DomainBootstrapper().Run(allFiles, tier),
new E49OrchestratorExpander().Run(tier),
new E50TransferLearner().Run(tier),
```

---

## Связь с DomainRegistry и E22

E46 ConfidenceGate — это механизм который ловит задачи вне известных доменов.
Когда E22 (RouteClassifier на TS+PY данных) будет готов:

```
E22: RouteClassifier знает C#/TS/PY
      ↓
E46: confidence < 0.6 для JAVA/RUST/GO задач
      ↓
E47: ModelHub находит java-spring адаптер
      ↓
E48: 50 примеров → первичное распознавание
      ↓
E49: граф: JAVA зависит от ARCH (паттерны)
      ↓
E50: fine-tune от CODE_Q → быстрое сходение
```

E22 можно проверить органически через этот пайплайн.

---

## Ожидаемые результаты

| Эксперимент | Ожидаемый результат | Почему |
|---|---|---|
| E46 | gate_f1 > 0.80 при threshold=0.6 | gap между известными и новыми доменами достаточно большой |
| E47 | hub_hit@1 enhanced > keyword | предикатный профиль точнее названия — паттерн E34v2 |
| E48 | precision at N=50 > 0.70 | 50 примеров создают достаточный кластер в пространстве |
| E49 | dependency_accuracy > 0.80 | ключевые слова + co-occurrence надёжно находят связи |
| E50 | transfer speedup > 1.5x | CODE_Q и DevOps делят паттерны работы с кодом |

**Время:** 3-4 дня.
**Параллельность:** можно запускать вместе с ProbeNeuralNetwork (E28v2-E45v2) —
независимые файлы и инфраструктура.
