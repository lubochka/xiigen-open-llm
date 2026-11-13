# E32 · E33 · E34 · E35 · E36 · E37 — OrchestratorModel (Подпроект 15)
## Подробный план для Claude Code v1.0

**Проект:** `35.ExperimentsV2`
**Обучающие данные:** `/home/claude/orch_data/translated/` (29 файлов)
**Зависимости:** FamilyCollapser (E16), LogicExtractor (E17), DomainClassifier (E18)
**Этот документ не касается XIIGen.**

---

## Семь компонентов OrchestratorModel

| Компонент | Эксперимент | Что делает |
|---|---|---|
| EpisodeDecomposer | E32 | Задача → N под-эпизодов |
| StopSignalDetector | E33 | Когда прекратить декомпозицию (4 сигнала) |
| EpisodeQueryMatcher | E34 | Новый промпт → похожий эпизод в корпусе |
| DependencyDetector | E35 | A и B — последовательно или параллельно? |
| FeedbackCorrector | E36 | Правка пользователя → какой эпизод обновить |
| MergerSynthesizer | E37 | Несколько ответов агентов → лучший/объединённый |

**AgentSelector** (выбор модели для под-эпизода) следует после E35:
зная зависимости и домен под-эпизода → выбрать Qwen 8B vs Gemma 27B vs Qwen 4B.

---

## Обучающий корпус из документов

```
EPISODE-MAP-FULL-HUMAN-LANGUAGE.md
  10 эпизодов × 2-5 под-эпизодов = ~35 примеров декомпозиции

EPISODE-QUERY-MAPPING.md
  10 эпизодов × 8 альтернативных формулировок = 80 примеров для E34

DEPTH-CALIBRATION-GUIDANCE.md
  4 сигнала остановки с конкретными примерами — обучение E33

JOINT-REVIEW-EPISODE-MAPS.md
  Примеры правок: "three rounds of corrections" → correction signal — E36

SUB-EPISODE-MAP.md
  452 значимых эпизода — зависимости для E35, качество для E37
```

---

---

# E32 — EpisodeDecomposer: задача → N под-эпизодов

## Гипотеза

Зная предикатный профиль задачи, K-NN предсказывает
правильное количество под-эпизодов точнее чем
"всегда 3" (медиана по корпусу).

Обучающий факт из документов:
- Ep3 "49 modules too tightly coupled" → 5 под-эпизодов (большая исследовательская задача)
- Ep4 "Before anything can run, too much missing" → 2 под-эпизода (починить и проверить)
- Ep10 "Build orchestration advisor" → 2 под-эпизода (спроектировать и реализовать)

**Паттерн:** задачи с исследованием дают 4-5 под-эпизодов,
задачи с реализацией дают 2-3.

## Шаг 1 — Создать `EpisodeCorpusLoader.cs`

**Путь:** `35.ExperimentsV2/Infrastructure/EpisodeCorpusLoader.cs`

```csharp
namespace ExperimentsV2;

// Загружает обучающие данные из документов оркестратора.
// Каждый эпизод = задача + количество под-эпизодов + теги.
public static class EpisodeCorpusLoader
{
    public record EpisodeExample(
        string CanonicalPrompt,           // "Canonical" из EPISODE-QUERY-MAPPING
        string[] AlternativePhrasings,    // 8 альтернативных формулировок
        int     SubEpisodeCount,          // сколько под-эпизодов
        string  EpisodeType,              // RESEARCH / BUILD / FIX / MIGRATE / DESIGN
        bool    HasIteration);            // есть ли итеративные раунды

    // Данные из EPISODE-MAP-FULL-HUMAN-LANGUAGE.md + EPISODE-QUERY-MAPPING.md
    // Жёстко закодированы из документов — без парсинга файлов
    public static IReadOnlyList<EpisodeExample> LoadAll() => new EpisodeExample[]
    {
        new(
            "I have a business description and some diagrams. Help me turn this into a system I can actually build.",
            new[] {
                "I know what I want to build but I don't know where to start",
                "How do I go from a feature list to an actual architecture?",
                "I have a rough idea of the product — can you help me structure it?",
                "We sketched out the flows on a whiteboard, now what?",
                "I need to turn my wireframes and notes into something a developer can work with",
                "How do I get from business requirements to a technical plan?",
                "My product spec is ready — how do I figure out what to build first?",
                "I have diagrams of what users do but no idea what the actual code should look like",
            },
            SubEpisodeCount: 4,
            EpisodeType: "DESIGN",
            HasIteration: false),

        new(
            "I'm building a tool that produces output I can see, but it keeps coming out wrong. I need to fix it round by round.",
            new[] {
                "My tool generates output but it never looks right",
                "I keep fixing the same type of problem but it breaks somewhere else",
                "The output looks almost right but there's always something off",
                "How do I improve something when I can only tell it's wrong by looking at it?",
                "We've done ten rounds of fixes and it's still not working properly",
                "I fix one bug and another appears — how do I get ahead of this?",
                "The AI keeps generating the wrong layout even after I tell it what's wrong",
                "How do you handle a problem that only shows up in the final output?",
            },
            SubEpisodeCount: 5,
            EpisodeType: "BUILD",
            HasIteration: true),

        new(
            "My platform's modules are too tightly coupled. I want tenants to be able to copy, adapt, and share them independently.",
            new[] {
                "How do I make my system less tangled?",
                "I want clients to be able to customize without breaking everything",
                "My modules depend on each other too much — how do I untangle them?",
                "I need a way to let customers fork parts of the system",
                "The platform is monolithic and I need to make it modular",
                "How do I make each part independent enough to be reused?",
                "Tenants need their own version of certain modules without affecting others",
                "How do I decouple the parts of my system so they can evolve separately?",
            },
            SubEpisodeCount: 5,
            EpisodeType: "RESEARCH",
            HasIteration: false),

        new(
            "Before anything can run, too much is missing. I need to build the foundation before anything else can start.",
            new[] {
                "Nothing works because the basics aren't in place",
                "I need to fix the infrastructure before I can build features",
                "The system won't start because too many dependencies are missing",
                "I can't test anything until the foundation is working",
                "Everything is blocked waiting for the base layer to be ready",
                "How do I build the scaffolding before the real work can begin?",
                "The prerequisites aren't met — I need to clear them before moving forward",
                "I need to get the platform to a working state before adding features",
            },
            SubEpisodeCount: 2,
            EpisodeType: "FIX",
            HasIteration: false),

        new(
            "Move my entire platform from one technology to another without breaking what's working.",
            new[] {
                "I need to migrate the platform to a new stack",
                "How do I switch technologies without losing functionality?",
                "We decided to change frameworks — how do we do it safely?",
                "I need to port the whole system to a different language or platform",
                "The old technology is no longer supported — how do I move forward?",
                "How do I migrate incrementally without a big-bang rewrite?",
                "We need to move from X to Y — where do we start?",
                "I need to replace the foundation while the house is still running",
            },
            SubEpisodeCount: 3,
            EpisodeType: "MIGRATE",
            HasIteration: false),

        new(
            "Make my platform work inside a WordPress site without changing how WordPress works.",
            new[] {
                "I need to embed my system inside an existing CMS",
                "How do I integrate without changing the host platform?",
                "My tool needs to live inside someone else's platform",
                "I need a plugin that adds my system to an existing site",
                "How do I make my app work as an add-on to WordPress?",
                "The client wants this inside their WordPress — how do we do that?",
                "I need to create a bridge between my platform and WordPress",
                "How do I package my system so it can run as a plugin?",
            },
            SubEpisodeCount: 3,
            EpisodeType: "BUILD",
            HasIteration: true),

        new(
            "Expand to 14 platforms and build a marketplace where others can publish plugins.",
            new[] {
                "I want to support many more platforms at once",
                "How do I build a plugin marketplace for my tool?",
                "I need an ecosystem where third parties can contribute",
                "How do I make my platform extensible by others?",
                "I want to scale from one integration to dozens simultaneously",
                "We need a way for external developers to add to the system",
                "How do I create a distribution model for plugins and extensions?",
                "I want to build something that others can build on top of",
            },
            SubEpisodeCount: 4,
            EpisodeType: "DESIGN",
            HasIteration: false),

        new(
            "Extract what my old codebases know so any AI assistant can use that knowledge.",
            new[] {
                "How do I get knowledge out of old code into something useful?",
                "I want to turn legacy code into training data",
                "My old projects contain a lot of hard-won knowledge — how do I preserve it?",
                "How do I make past work useful for future AI assistance?",
                "I need to extract patterns from codebases that no longer run",
                "How do I teach an AI what my old team knew?",
                "I want to mine my codebase history for reusable knowledge",
                "How do I turn years of code into a structured knowledge base?",
            },
            SubEpisodeCount: 3,
            EpisodeType: "RESEARCH",
            HasIteration: false),

        new(
            "The AI assistant's own working guidelines were getting stale and inconsistent. They needed to be rebuilt from scratch.",
            new[] {
                "My AI's instructions are contradicting each other",
                "The system prompt has grown out of control — how do I fix it?",
                "My working guidelines for the AI are out of date",
                "How do I rebuild the assistant's rules without losing what works?",
                "The AI keeps making the same mistakes because the instructions are wrong",
                "I need to audit and rewrite the instructions that govern how the AI works",
                "The guidance documents are inconsistent — how do I align them?",
                "My AI assistant is following outdated rules — how do I update them systematically?",
            },
            SubEpisodeCount: 2,
            EpisodeType: "FIX",
            HasIteration: true),

        new(
            "Build an orchestration advisor that searches past solutions and returns grounded suggestions using three local models.",
            new[] {
                "I want a tool that recommends how to approach a problem based on past work",
                "How do I build a system that searches its own history for answers?",
                "I need an advisor that uses previous solutions as context",
                "How do I make an AI that learns from its own past decisions?",
                "I want to build something that can recommend approaches based on solved problems",
                "How do I create a retrieval system over my own problem-solving history?",
                "I need a multi-model system that compares different approaches",
                "How do I build a grounded advisor that doesn't hallucinate?",
            },
            SubEpisodeCount: 3,
            EpisodeType: "BUILD",
            HasIteration: false),
    }.AsReadOnly();

    // Расширить датасет через альтернативные формулировки
    public static List<(string Prompt, int SubEpisodeCount, string Type)> Expand()
    {
        var result = new List<(string, int, string)>();
        foreach (var ep in LoadAll())
        {
            result.Add((ep.CanonicalPrompt, ep.SubEpisodeCount, ep.EpisodeType));
            foreach (var alt in ep.AlternativePhrasings)
                result.Add((alt, ep.SubEpisodeCount, ep.EpisodeType));
        }
        return result;
    }
}
```

## Шаг 2 — Создать `E32EpisodeDecomposer.cs`

**Путь:** `35.ExperimentsV2/Experiments/E32EpisodeDecomposer.cs`

```csharp
namespace ExperimentsV2;

// E32: EpisodeDecomposer — предсказать сколько под-эпизодов нужно.
// Обучение:  10 эпизодов × 9 формулировок = 90 примеров
// Baseline:  всегда предсказывать медиану (3 под-эпизода)
// Enhanced:  K-NN на FamilyCollapser-тегах промпта
// Метрика:   count_accuracy (правильное кол-во) + ±1_accuracy (близко)
public sealed class E32EpisodeDecomposer
{
    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var data  = EpisodeCorpusLoader.Expand();
        // Перемешать перед разбивкой (урок из E20/E25)
        var rng   = new Random(42);
        var shuffled = data.OrderBy(_ => rng.Next()).ToList();
        int split = (int)(shuffled.Count * 0.8);
        var train = shuffled.Take(split).ToList();
        var val   = shuffled.Skip(split).ToList();

        // Медиана по train
        int median = train.Select(x => x.SubEpisodeCount)
            .OrderBy(x => x).ElementAt(train.Count / 2);

        // Обучить K-NN на семейных тегах
        var trainFeatures = train.Select(x =>
            (FamilyCollapser.CollapseString(x.Prompt).ToArray(),
             x.SubEpisodeCount)).ToList();

        // ── Baseline: всегда медиана ──────────────────────────────────────────
        var t0 = Stopwatch();
        int baseExact = val.Count(x => x.SubEpisodeCount == median);
        int basePm1   = val.Count(x => Math.Abs(x.SubEpisodeCount - median) <= 1);
        double baseAcc  = baseExact / (double)val.Count;
        double basePm1A = basePm1   / (double)val.Count;
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: K-NN на тегах ──────────────────────────────────────────
        var t1 = Stopwatch();
        int enhExact = 0, enhPm1 = 0;
        foreach (var (prompt, expected, _) in val)
        {
            var qTags = FamilyCollapser.CollapseString(prompt);
            int predicted = trainFeatures
                .OrderBy(t => TagSetDistance(qTags.ToHashSet(), new HashSet<string>(t.Item1)))
                .Take(3)
                .GroupBy(t => t.Item2)
                .OrderByDescending(g => g.Count())
                .First().Key;
            if (predicted == expected)           enhExact++;
            if (Math.Abs(predicted - expected) <= 1) enhPm1++;
        }
        double enhAcc  = enhExact / (double)val.Count;
        double enhPm1A = enhPm1   / (double)val.Count;
        double enhancedMs = ElapsedMs(t1);

        // Breakdown по типу эпизода
        var typeStats = val.GroupBy(x => x.Type)
            .Select(g =>
            {
                int ok = g.Count(x =>
                {
                    var qT = FamilyCollapser.CollapseString(x.Prompt);
                    int pred = trainFeatures
                        .OrderBy(t => TagSetDistance(qT.ToHashSet(), new HashSet<string>(t.Item1)))
                        .Take(3).GroupBy(t => t.Item2)
                        .OrderByDescending(gg => gg.Count()).First().Key;
                    return pred == x.SubEpisodeCount;
                });
                return $"{g.Key}={ok}/{g.Count()}";
            });

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E32-EPISODE-DECOMPOSER",
            Tier              = tier,
            BaselinePrimary   = baseAcc,
            EnhancedPrimary   = enhAcc,
            PrimaryMetricName = "count_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = enhPm1A,
            ScalingSignalNote = "±1_accuracy (допустимая близость к правильному числу)",
            ShouldScaleUp     = val.Count < 10,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"trainN={train.Count}, valN={val.Count}, median={median}, " +
                    $"baseExact={baseAcc:P1} ±1={basePm1A:P1}, " +
                    $"enhExact={enhAcc:P1} ±1={enhPm1A:P1}, " +
                    $"by_type=[{string.Join(",", typeStats)}]"
        });
    }

    private static double TagSetDistance(HashSet<string> a, HashSet<string> b)
    {
        if (a.Count == 0 && b.Count == 0) return 0;
        int inter = a.Count(x => b.Contains(x));
        int union = a.Count + b.Count - inter;
        return union == 0 ? 0 : 1.0 - (double)inter / union;
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
```

## Ожидаемый вывод

```
OK count_accuracy (up) baseline=0.389 enhanced=0.556 delta=+42.9% | PASSED
notes: trainN=72, valN=18, median=3, baseExact=38.9% ±1=72.2%,
       enhExact=55.6% ±1=88.9%, by_type=[RESEARCH=2/2,BUILD=3/5,FIX=2/3]
```

---

---

# E33 — StopSignalDetector: когда прекратить декомпозицию

## Четыре сигнала остановки (из документов)

```
Сигнал 1 — PHASES:      "следующий уровень = просто порядок шагов выполнения"
                         итерации — НЕ суб-суб-эпизоды, это шаги внутри одного
Сигнал 2 — TECH:        "следующий уровень требует технического контекста"
                         если нельзя объяснить без знания конкретной технологии
Сигнал 3 — SINGLE_DOC:  "следующий уровень = один документ без структуры"
                         один документ, принятый с первого раза, не нужен уровень
Сигнал 4 — NO_LOOP:     "нет истории версий, нет обратной связи пользователя"
                         сделано однократно, принято — это шаг, не под-эпизод
```

## Гипотеза

Проверить правильный сигнал остановки важнее чем точность классификации.
"Ложная глубина" (продолжить когда надо остановиться) стоит дороже
чем "ложная остановка" — оркестратор создаёт лишних агентов.

Baseline: всегда CONTINUE (никогда не останавливаться) — recall=100%, precision=0%.
Enhanced: K-NN по предикатному профилю → один из 4 сигналов STOP или CONTINUE.

## Шаг 1 — Создать `StopSignalDetector.cs`

**Путь:** `35.ExperimentsV2/Infrastructure/StopSignalDetector.cs`

```csharp
namespace ExperimentsV2;

// StopSignalDetector классифицирует единицу работы:
// STOP (это шаг внутри эпизода) или CONTINUE (это отдельный под-эпизод).
public static class StopSignalDetector
{
    public const int CONTINUE    = 0; // продолжить декомпозицию
    public const int STOP_PHASES = 1; // порядок выполнения, не под-эпизоды
    public const int STOP_TECH   = 2; // требует технического контекста
    public const int STOP_SINGLE = 3; // один документ без структуры
    public const int STOP_NOLOOP = 4; // нет итерации, нет правок пользователя

    public static readonly string[] SignalNames =
        ["CONTINUE","STOP_PHASES","STOP_TECH","STOP_SINGLE","STOP_NOLOOP"];

    // Правило-baseline: применить все 4 сигнала последовательно
    public static int PredictByRules(string taskDescription, bool hasIterations, bool hasTechTerms)
    {
        var lower = taskDescription.ToLowerInvariant();
        // Сигнал 1: "then", "next", "after", "first" без самостоятельной цели
        bool isPhaseSequence =
            new[] {"then ","next ","after that","first ","step ","finally "}
            .Any(w => lower.Contains(w)) &&
            !lower.Contains("goal") && !lower.Contains("objective");
        if (isPhaseSequence) return STOP_PHASES;

        // Сигнал 2: технические термины без человеческого объяснения
        if (hasTechTerms) return STOP_TECH;

        // Сигнал 3: "single document", "one file", "produced once"
        if (lower.Contains("single ") || lower.Contains("one document") ||
            lower.Contains("produced once")) return STOP_SINGLE;

        // Сигнал 4: нет итераций
        if (!hasIterations) return STOP_NOLOOP;

        return CONTINUE;
    }

    // Синтетические обучающие примеры из документов оркестратора
    public static List<(string Description, bool HasIter, bool HasTech, int Signal)>
        GenerateExamples(int seed, int n)
    {
        var rng = new Random(seed);
        var templates = new (string Desc, bool Iter, bool Tech, int Signal)[]
        {
            // STOP_PHASES — порядок шагов
            ("First read the requirements, then decide the architecture, then implement it",
             false, false, STOP_PHASES),
            ("Check the configuration, then run the tests, then deploy the result",
             false, false, STOP_PHASES),
            ("Open the file, parse the content, write the output to disk",
             false, false, STOP_PHASES),
            // STOP_TECH — требует технического контекста
            ("Configure the DI container to resolve the scoped service correctly",
             false, true, STOP_TECH),
            ("Fix the JWT token validation to handle the RS256 algorithm edge case",
             false, true, STOP_TECH),
            // STOP_SINGLE — один документ
            ("Produce the architecture diagram that will govern all subsequent decisions",
             false, false, STOP_SINGLE),
            ("Create the single requirements document approved first pass",
             false, false, STOP_SINGLE),
            // STOP_NOLOOP — без итерации
            ("Analyse the module dependencies to understand the coupling",
             false, false, STOP_NOLOOP),
            ("Write the summary of findings from the code review",
             false, false, STOP_NOLOOP),
            // CONTINUE — настоящий под-эпизод
            ("Implement the authentication flow with multiple rounds of review and correction",
             true, false, CONTINUE),
            ("Design the database schema, review with team, iterate based on feedback",
             true, false, CONTINUE),
            ("Build the search feature: first version, review output, three rounds of fixes",
             true, false, CONTINUE),
            ("Migrate the platform in stages, validate each stage before proceeding",
             false, false, CONTINUE),
        };

        var result = new List<(string, bool, bool, int)>();
        while (result.Count < n)
            result.Add(templates[rng.Next(templates.Length)]);
        return result;
    }
}
```

## Шаг 2 — Создать `E33StopSignalDetector.cs`

**Путь:** `35.ExperimentsV2/Experiments/E33StopSignalDetector.cs`

```csharp
namespace ExperimentsV2;

// E33: StopSignalDetector — когда прекратить декомпозицию.
// Baseline:  правила на ключевых словах (4 сигнала последовательно)
// Enhanced:  K-NN на семейных тегах + признаки hasIter/hasTech
// Метрика:   binary_accuracy (STOP vs CONTINUE)
//            + false_continue_rate (самая дорогая ошибка)
public sealed class E33StopSignalDetector
{
    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var data     = StopSignalDetector.GenerateExamples(seed: 42, n: tier * 60);
        var rng      = new Random(42);
        var shuffled = data.OrderBy(_ => rng.Next()).ToList();
        int split    = (int)(shuffled.Count * 0.8);
        var train    = shuffled.Take(split).ToList();
        var val      = shuffled.Skip(split).ToList();

        // Строим K-NN: каждый пример = (теги + hasIter + hasTech) → signal
        var trainFeatures = train.Select(x => (
            Tags:    FamilyCollapser.CollapseString(x.Description),
            HasIter: x.HasIter,
            HasTech: x.HasTech,
            Signal:  x.Signal)).ToList();

        int Predict(string desc, bool hasIter, bool hasTech)
        {
            var qTags = FamilyCollapser.CollapseString(desc);
            return trainFeatures
                .OrderBy(t =>
                {
                    double tagDist = TagDist(qTags, t.Tags);
                    double feaDist = (hasIter == t.HasIter ? 0 : 0.5) +
                                     (hasTech == t.HasTech ? 0 : 0.5);
                    return tagDist + feaDist;
                })
                .Take(3)
                .GroupBy(t => t.Signal > 0 ? 1 : 0)  // STOP vs CONTINUE
                .OrderByDescending(g => g.Count())
                .First().Key;
        }

        // ── Baseline: правила ─────────────────────────────────────────────────
        var t0 = Stopwatch();
        int baseOk = val.Count(x =>
        {
            int predicted = StopSignalDetector.PredictByRules(x.Description, x.HasIter, x.HasTech);
            return (predicted > 0) == (x.Signal > 0); // STOP vs CONTINUE
        });
        // False CONTINUE = предсказали CONTINUE но должно быть STOP (лишние агенты)
        var stops = val.Where(x => x.Signal > 0).ToList();
        int baseFalseCont = stops.Count(x =>
            StopSignalDetector.PredictByRules(x.Description, x.HasIter, x.HasTech) == 0);
        double baseAcc  = baseOk / (double)val.Count;
        double baseFCR  = stops.Count > 0 ? baseFalseCont / (double)stops.Count : 0;
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: K-NN ────────────────────────────────────────────────────
        var t1 = Stopwatch();
        int enhOk = val.Count(x =>
            Predict(x.Description, x.HasIter, x.HasTech) == (x.Signal > 0 ? 1 : 0));
        int enhFalseCont = stops.Count(x =>
            Predict(x.Description, x.HasIter, x.HasTech) == 0);
        double enhAcc = enhOk / (double)val.Count;
        double enhFCR = stops.Count > 0 ? enhFalseCont / (double)stops.Count : 0;
        double enhancedMs = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E33-STOP-SIGNAL-DETECTOR",
            Tier              = tier,
            BaselinePrimary   = baseAcc,
            EnhancedPrimary   = enhAcc,
            PrimaryMetricName = "binary_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = baseFCR - enhFCR,
            ScalingSignalNote = "false_continue_reduction (меньше лишних агентов)",
            ShouldScaleUp     = val.Count < 15,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"trainN={train.Count}, stops={stops.Count}, " +
                    $"baseAcc={baseAcc:P1} FCR={baseFCR:P1}, " +
                    $"enhAcc={enhAcc:P1} FCR={enhFCR:P1}, " +
                    $"false_continue_reduction={baseFCR-enhFCR:+.1%}"
        });
    }

    private static double TagDist(List<string> a, List<string> b)
    {
        var sa = new HashSet<string>(a); var sb = new HashSet<string>(b);
        int i = sa.Count(x => sb.Contains(x));
        int u = sa.Count + sb.Count - i;
        return u == 0 ? 0 : 1.0 - (double)i / u;
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
```

---

---

# E34 — EpisodeQueryMatcher: новый промпт → похожий эпизод

## Гипотеза

Когда пользователь пишет "My modules are too coupled and clients need their own version",
система должна найти эпизод 3 (модульное расцепление) из 10 известных.

80 размеченных примеров (10 эпизодов × 8 формулировок) —
достаточно чтобы K-NN на FamilyCollapser-тегах нашёл правильный эпизод.

## Шаг 1 — Создать `E34EpisodeQueryMatcher.cs`

**Путь:** `35.ExperimentsV2/Experiments/E34EpisodeQueryMatcher.cs`

```csharp
namespace ExperimentsV2;

// E34: EpisodeQueryMatcher — найти похожий эпизод для нового промпта.
// Обучение:  80 примеров (10 эпизодов × 8 формулировок) из EPISODE-QUERY-MAPPING
// Baseline:  keyword matching (E18 стиль: RESEARCH/BUILD/FIX → эпизод по типу)
// Enhanced:  K-NN на FamilyCollapser-тегах промпта
// Метрика:   hit@1 (10 классов, random=10%)
public sealed class E34EpisodeQueryMatcher
{
    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var data     = EpisodeCorpusLoader.Expand()
            .Select((x, i) => (x.Prompt, x.SubEpisodeCount, x.Type,
                EpisodeId: EpisodeIdFromIndex(i, EpisodeCorpusLoader.Expand().Count)))
            .ToList();

        var rng      = new Random(42);
        var shuffled = data.OrderBy(_ => rng.Next()).ToList();
        int split    = (int)(shuffled.Count * 0.8);
        var train    = shuffled.Take(split).ToList();
        var val      = shuffled.Skip(split).ToList();

        // Train: (теги, episodeId)
        var trainK = train.Select(x => (
            Tags:   FamilyCollapser.CollapseString(x.Prompt),
            EpId:   x.EpisodeId)).ToList();

        int PredictKnn(string prompt)
        {
            var qTags = FamilyCollapser.CollapseString(prompt);
            return trainK
                .OrderBy(t => TagDist(qTags, t.Tags))
                .Take(3)
                .GroupBy(t => t.EpId)
                .OrderByDescending(g => g.Count())
                .First().Key;
        }

        int PredictKeyword(string prompt)
        {
            var lower = prompt.ToLowerInvariant();
            if (lower.Contains("modules") || lower.Contains("coupled") || lower.Contains("tangled")) return 3;
            if (lower.Contains("business") || lower.Contains("architecture") || lower.Contains("diagrams")) return 1;
            if (lower.Contains("generate") || lower.Contains("output") || lower.Contains("wrong")) return 2;
            if (lower.Contains("migration") || lower.Contains("migrate") || lower.Contains("move")) return 5;
            if (lower.Contains("wordpress") || lower.Contains("plugin") || lower.Contains("embed")) return 6;
            if (lower.Contains("marketplace") || lower.Contains("14 platform")) return 7;
            if (lower.Contains("legacy") || lower.Contains("old code") || lower.Contains("knowledge")) return 8;
            if (lower.Contains("guidelines") || lower.Contains("instructions") || lower.Contains("stale")) return 9;
            if (lower.Contains("orchestration") || lower.Contains("advisor")) return 10;
            return 4; // default: foundation/missing
        }

        // ── Baseline ──────────────────────────────────────────────────────────
        var t0 = Stopwatch();
        int baseOk = val.Count(x => PredictKeyword(x.Prompt) == x.EpisodeId);
        double baseAcc = baseOk / (double)val.Count;
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced ──────────────────────────────────────────────────────────
        var t1 = Stopwatch();
        int enhOk = val.Count(x => PredictKnn(x.Prompt) == x.EpisodeId);
        double enhAcc = enhOk / (double)val.Count;
        double enhancedMs = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E34-EPISODE-QUERY-MATCHER",
            Tier              = tier,
            BaselinePrimary   = baseAcc,
            EnhancedPrimary   = enhAcc,
            PrimaryMetricName = "hit_at_1",
            LowerIsBetter     = false,
            ScalingSignal     = (double)val.Select(x => x.EpisodeId).Distinct().Count() / 10.0,
            ScalingSignalNote = "episode_coverage (доля эпизодов в val-наборе)",
            ShouldScaleUp     = val.Count < 15,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"trainN={train.Count}, valN={val.Count}, " +
                    $"baseAcc={baseAcc:P1}, enhAcc={enhAcc:P1}"
        });
    }

    private static int EpisodeIdFromIndex(int idx, int total)
        => (idx / (total / 10)) + 1;  // распределить по 10 эпизодам

    private static double TagDist(List<string> a, List<string> b)
    {
        var sa = new HashSet<string>(a); var sb = new HashSet<string>(b);
        int i = sa.Count(x => sb.Contains(x));
        int u = sa.Count + sb.Count - i;
        return u == 0 ? 0 : 1.0 - (double)i / u;
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
```

---

---

# E35 — DependencyDetector: последовательно или параллельно?

## Гипотеза

Если под-эпизод A **производит** выход который нужен под-эпизоду B — они последовательны.
Если A и B независимы — они параллельны (запускать одновременно, быстрее).

Из документов: "Design the architecture" → "Implement the features" — последовательно.
"Write documentation" || "Write tests" — могут идти параллельно.

## Шаг 1 — Создать `E35DependencyDetector.cs`

**Путь:** `35.ExperimentsV2/Experiments/E35DependencyDetector.cs`

```csharp
namespace ExperimentsV2;

// E35: DependencyDetector — A и B последовательны или параллельны?
// Baseline:  всегда SEQUENTIAL (безопасно, но медленно)
// Enhanced:  K-NN на предикатном overlap + "produces/consumes" сигнал
// Метрика:   dependency_accuracy + parallel_opportunity_rate
//            (сколько параллельных пар правильно найдено)
public sealed class E35DependencyDetector
{
    public const int SEQUENTIAL = 0;
    public const int PARALLEL   = 1;

    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var pairs = GenerateDependencyPairs(seed: 42, n: tier * 50);
        var rng   = new Random(42);
        var shuffled = pairs.OrderBy(_ => rng.Next()).ToList();
        int split = (int)(shuffled.Count * 0.8);
        var train = shuffled.Take(split).ToList();
        var val   = shuffled.Skip(split).ToList();

        // Обнаружить зависимость: A производит то что B потребляет
        bool IsSequential(string a, string b)
        {
            var aTags = FamilyCollapser.CollapseString(a).ToHashSet();
            var bTags = FamilyCollapser.CollapseString(b).ToHashSet();
            // Если A создаёт (CREATE_OP) а B получает (GET_OP) → вероятно зависимость
            bool aCreates = aTags.Contains("CREATE_OP") || aTags.Contains("SET_OP");
            bool bGets    = bTags.Contains("GET_OP");
            bool overlap  = aTags.Count(t => bTags.Contains(t)) >= 2; // 2+ общих тега
            return (aCreates && bGets) || overlap;
        }

        // ── Baseline: всегда SEQUENTIAL ───────────────────────────────────────
        var t0 = Stopwatch();
        int baseOk   = val.Count(p => p.Expected == SEQUENTIAL);
        var parallels = val.Where(p => p.Expected == PARALLEL).ToList();
        double baseAcc = baseOk / (double)val.Count;
        double basePOR = 0; // 0% параллельных возможностей найдено
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced ──────────────────────────────────────────────────────────
        var t1 = Stopwatch();
        int enhOk = val.Count(p =>
            (IsSequential(p.A, p.B) ? SEQUENTIAL : PARALLEL) == p.Expected);
        int enhParOk = parallels.Count(p =>
            !IsSequential(p.A, p.B));
        double enhAcc = enhOk / (double)val.Count;
        double enhPOR = parallels.Count > 0 ? enhParOk / (double)parallels.Count : 0;
        double enhancedMs = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E35-DEPENDENCY-DETECTOR",
            Tier              = tier,
            BaselinePrimary   = baseAcc,
            EnhancedPrimary   = enhAcc,
            PrimaryMetricName = "dependency_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = enhPOR,
            ScalingSignalNote = "parallel_opportunity_rate (найденные параллельные пары)",
            ShouldScaleUp     = val.Count < 15,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"pairs={pairs.Count}, parallels={parallels.Count}, " +
                    $"baseAcc={baseAcc:P1}(POR=0%), " +
                    $"enhAcc={enhAcc:P1}(POR={enhPOR:P1})"
        });
    }

    private sealed record DependencyPair(string A, string B, int Expected);

    private static List<DependencyPair> GenerateDependencyPairs(int seed, int n)
    {
        var rng = new Random(seed);
        var templates = new DependencyPair[]
        {
            // SEQUENTIAL (A → B)
            new("Design the database schema", "Implement the data access layer", SEQUENTIAL),
            new("Build the authentication service", "Implement the protected routes", SEQUENTIAL),
            new("Create the API contracts", "Build the client that consumes them", SEQUENTIAL),
            new("Write the architecture document", "Implement the components it defines", SEQUENTIAL),
            new("Define the data model", "Create the migration scripts for it", SEQUENTIAL),
            new("Analyse the existing codebase", "Design the refactoring plan based on findings", SEQUENTIAL),
            new("Build the base model", "Fine-tune the adapter on project data", SEQUENTIAL),
            new("Set up the test infrastructure", "Write the integration tests that use it", SEQUENTIAL),
            // PARALLEL (A || B)
            new("Write the unit tests", "Write the documentation", PARALLEL),
            new("Implement the search feature", "Implement the notification feature", PARALLEL),
            new("Fix the login bug", "Fix the export bug", PARALLEL),
            new("Write tests for module A", "Write tests for module B", PARALLEL),
            new("Migrate the user data", "Migrate the product catalogue", PARALLEL),
            new("Review the API design", "Review the database schema", PARALLEL),
        };
        var result = new List<DependencyPair>();
        while (result.Count < n)
            result.Add(templates[rng.Next(templates.Length)]);
        return result;
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
```

---

---

# E36 — FeedbackCorrector: правка пользователя → какой эпизод обновить

## Гипотеза

Из JOINT-REVIEW: "correction signal = exact words user said that caused the change."

Когда пользователь говорит "No, that's one task not three" —
это правка для EpisodeDecomposer, не для BugFix.
Правильная маршрутизация улучшает именно тот компонент который ошибся.

## Шаг 1 — Создать `E36FeedbackCorrector.cs`

**Путь:** `35.ExperimentsV2/Experiments/E36FeedbackCorrector.cs`

```csharp
namespace ExperimentsV2;

// E36: FeedbackCorrector — правка пользователя → какой компонент обновить.
// Baseline:  обновить последний использованный компонент
// Enhanced:  K-NN на тегах правки → маршрутизировать к правильному
// Метрика:   routing_accuracy (6 компонентов: E32-E37)
public sealed class E36FeedbackCorrector
{
    public const int DECOMPOSER   = 32; // "that's one task not three"
    public const int STOP_SIGNAL  = 33; // "this needs its own sub-step"
    public const int QUERY_MATCH  = 34; // "this is more like episode 5"
    public const int DEPENDENCY   = 35; // "these can run in parallel"
    public const int MERGER       = 37; // "use the second answer, not the first"

    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var examples = GenerateFeedbackExamples(seed: 42, n: tier * 40);
        var rng      = new Random(42);
        var shuffled = examples.OrderBy(_ => rng.Next()).ToList();
        int split    = (int)(shuffled.Count * 0.8);
        var train    = shuffled.Take(split).ToList();
        var val      = shuffled.Skip(split).ToList();

        var trainK = train.Select(x => (
            Tags:      FamilyCollapser.CollapseString(x.Correction),
            Component: x.TargetComponent)).ToList();

        // ── Baseline: всегда обновить DECOMPOSER (самый частый) ──────────────
        var t0 = Stopwatch();
        int mostFreq = train.GroupBy(x => x.TargetComponent)
            .OrderByDescending(g => g.Count()).First().Key;
        int baseOk = val.Count(x => x.TargetComponent == mostFreq);
        double baseAcc = baseOk / (double)val.Count;
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: K-NN ────────────────────────────────────────────────────
        var t1 = Stopwatch();
        int enhOk = val.Count(x =>
        {
            var qTags = FamilyCollapser.CollapseString(x.Correction);
            int predicted = trainK
                .OrderBy(t => TagDist(qTags, t.Tags))
                .Take(3)
                .GroupBy(t => t.Component)
                .OrderByDescending(g => g.Count())
                .First().Key;
            return predicted == x.TargetComponent;
        });
        double enhAcc = enhOk / (double)val.Count;
        double enhancedMs = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E36-FEEDBACK-CORRECTOR",
            Tier              = tier,
            BaselinePrimary   = baseAcc,
            EnhancedPrimary   = enhAcc,
            PrimaryMetricName = "routing_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = (double)val.Select(x => x.TargetComponent).Distinct().Count() / 5.0,
            ScalingSignalNote = "component_coverage (доля компонентов в val)",
            ShouldScaleUp     = val.Count < 10,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"trainN={train.Count}, valN={val.Count}, " +
                    $"baseAcc={baseAcc:P1}(freq={mostFreq}), enhAcc={enhAcc:P1}"
        });
    }

    private sealed record FeedbackExample(string Correction, int TargetComponent);

    private static List<FeedbackExample> GenerateFeedbackExamples(int seed, int n)
    {
        var rng = new Random(seed);
        var templates = new FeedbackExample[]
        {
            // → DECOMPOSER (число под-эпизодов)
            new("No, that's one task not three separate episodes", DECOMPOSER),
            new("These should be combined into a single step", DECOMPOSER),
            new("You split this into too many pieces — it's all one thing", DECOMPOSER),
            new("This needs to be broken down further into sub-steps", DECOMPOSER),
            // → STOP_SIGNAL (останавливаться или нет)
            new("This should have its own dedicated sub-episode", STOP_SIGNAL),
            new("These are just execution steps, not separate episodes", STOP_SIGNAL),
            new("This is too granular — it doesn't need to be a separate step", STOP_SIGNAL),
            // → QUERY_MATCH (какой эпизод нашли)
            new("This is more like the migration problem, not the architecture problem", QUERY_MATCH),
            new("The closest example is the WordPress integration, not the business planning one", QUERY_MATCH),
            new("You matched this to the wrong previous episode", QUERY_MATCH),
            // → DEPENDENCY (порядок выполнения)
            new("These two parts can run in parallel, you don't need to wait", DEPENDENCY),
            new("This has to finish before that one can start", DEPENDENCY),
            new("The order matters here — you can't reverse these steps", DEPENDENCY),
            // → MERGER (выбор ответа)
            new("Use the second answer, the first one missed the point", MERGER),
            new("Combine the two responses — each covers something the other missed", MERGER),
            new("The shorter answer was more accurate, ignore the longer one", MERGER),
        };
        var result = new List<FeedbackExample>();
        while (result.Count < n)
            result.Add(templates[rng.Next(templates.Length)]);
        return result;
    }

    private static double TagDist(List<string> a, List<string> b)
    {
        var sa = new HashSet<string>(a); var sb = new HashSet<string>(b);
        int i = sa.Count(x => sb.Contains(x));
        int u = sa.Count + sb.Count - i;
        return u == 0 ? 0 : 1.0 - (double)i / u;
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
```

---

---

# E37 — MergerSynthesizer: несколько ответов агентов → лучший

## Гипотеза

Из документов: три модели (Qwen3.6 8B, Gemma 27B, Qwen3 4B) дают разные ответы.
Ответ с наибольшим покрытием предикатов запроса — лучший.

"Покрытие" = сколько предикатов из запроса присутствует в ответе.
Baseline: взять самый длинный ответ.
Enhanced: взять ответ с максимальным покрытием предикатов.

## Шаг 1 — Создать `E37MergerSynthesizer.cs`

**Путь:** `35.ExperimentsV2/Experiments/E37MergerSynthesizer.cs`

```csharp
namespace ExperimentsV2;

// E37: MergerSynthesizer — выбрать лучший из нескольких ответов агентов.
// Baseline:  самый длинный ответ (больше = полнее?)
// Enhanced:  максимальное покрытие предикатов запроса
// Метрика:   quality_selection_accuracy
//            (правильно ли выбрали лучший ответ из 3)
public sealed class E37MergerSynthesizer
{
    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var testCases = GenerateTestCases(seed: 42, n: tier * 20);
        var rng       = new Random(42);
        var shuffled  = testCases.OrderBy(_ => rng.Next()).ToList();
        int split     = (int)(shuffled.Count * 0.8);
        var val       = shuffled.Skip(split).ToList();

        // ── Baseline: самый длинный ──────────────────────────────────────────
        var t0 = Stopwatch();
        int baseOk = val.Count(tc =>
        {
            int longest = tc.Responses.Select((r, i) => (Length: r.Length, Idx: i))
                .OrderByDescending(x => x.Length).First().Idx;
            return longest == tc.BestIdx;
        });
        double baseAcc = baseOk / (double)val.Count;
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: покрытие предикатов ────────────────────────────────────
        var t1 = Stopwatch();
        int enhOk = val.Count(tc =>
        {
            var qTags = FamilyCollapser.CollapseString(tc.Query).ToHashSet();
            int best  = tc.Responses
                .Select((resp, idx) =>
                {
                    var rTags = FamilyCollapser.CollapseString(resp).ToHashSet();
                    double coverage = qTags.Count > 0
                        ? qTags.Count(t => rTags.Contains(t)) / (double)qTags.Count : 0;
                    return (Coverage: coverage, Idx: idx);
                })
                .OrderByDescending(x => x.Coverage)
                .First().Idx;
            return best == tc.BestIdx;
        });
        double enhAcc     = enhOk / (double)val.Count;
        double enhancedMs = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E37-MERGER-SYNTHESIZER",
            Tier              = tier,
            BaselinePrimary   = baseAcc,
            EnhancedPrimary   = enhAcc,
            PrimaryMetricName = "quality_selection_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = val.Count(tc =>
                tc.Responses.Max(r => r.Length) != tc.Responses[tc.BestIdx].Length)
                / (double)val.Count,
            ScalingSignalNote = "length_mismatch_rate (доля случаев где лучший ≠ самый длинный)",
            ShouldScaleUp     = val.Count < 10,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"testCases={testCases.Count}, val={val.Count}, " +
                    $"baseAcc={baseAcc:P1}, enhAcc={enhAcc:P1}"
        });
    }

    private sealed record MergeTestCase(
        string   Query,
        string[] Responses,   // 3 ответа от разных агентов
        int      BestIdx);    // индекс правильного ответа (0,1,2)

    private static List<MergeTestCase> GenerateTestCases(int seed, int n)
    {
        var rng = new Random(seed);
        var templates = new MergeTestCase[]
        {
            // Query focused on null check; best answer covers null + validation
            new("Add null check to ValidateToken before accessing user properties",
                new[] {
                    "You should consider improving the error handling in this method.",  // off-topic
                    "Add if (token == null) return OperationResult.Fail before accessing token.Name.",  // correct
                    "The ValidateToken method is in UserService.cs and handles authentication." // describes but doesn't solve
                }, BestIdx: 1),

            // Query about decomposing task; best answer matches predicate profile
            new("Split the authentication feature into manageable sub-tasks",
                new[] {
                    "The authentication feature involves login, token management, and session handling. These can be separate tasks.",  // correct
                    "Authentication is a critical security component that needs careful implementation.", // no decomposition
                    "You will need to create a database table for sessions." // too specific
                }, BestIdx: 0),

            // Query about parallel work; best answer identifies parallelism
            new("Which parts of the migration can run at the same time?",
                new[] {
                    "Migration tasks are complex and should be planned carefully before starting.",
                    "The user data migration and the product catalogue migration are independent and can run in parallel.",  // correct
                    "You need to migrate the user data, then the product catalogue, then the orders."  // sequential (wrong)
                }, BestIdx: 1),

            // Shorter but more accurate
            new("What is the first step when rebuilding a system from scratch?",
                new[] {
                    "Start by understanding what the current system does and what problems it has before designing anything new.",  // correct — focused
                    "Rebuilding a system from scratch involves many phases: requirements gathering, architecture design, team planning, technology selection, prototype development, review cycles, and iterative implementation with continuous integration.",  // too broad
                    "You should use modern frameworks and ensure the new system has proper test coverage."  // generic
                }, BestIdx: 0),
        };
        var result = new List<MergeTestCase>();
        while (result.Count < n)
            result.Add(templates[rng.Next(templates.Length)]);
        return result;
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
```

---

## Добавить все 6 в Dispatcher/Runner/ResultWriter

```csharp
// Dispatcher — 6 методов:
public OperationResult<ExperimentV2Result> RunE32(int tier = 1) => new E32EpisodeDecomposer().Run(tier);
public OperationResult<ExperimentV2Result> RunE33(int tier = 1) => new E33StopSignalDetector().Run(tier);
public OperationResult<ExperimentV2Result> RunE34(int tier = 1) => new E34EpisodeQueryMatcher().Run(tier);
public OperationResult<ExperimentV2Result> RunE35(
    IReadOnlyList<(string,string)> files, int tier=1) => new E35DependencyDetector().Run(tier);
public OperationResult<ExperimentV2Result> RunE36(int tier = 1) => new E36FeedbackCorrector().Run(tier);
public OperationResult<ExperimentV2Result> RunE37(int tier = 1) => new E37MergerSynthesizer().Run(tier);

// Runner ids: добавить "E32","E33","E34","E35","E36","E37"
// Runner switch:
// "E32" => _d.RunE32(tier),  "E33" => _d.RunE33(tier),
// "E34" => _d.RunE34(tier),  "E35" => _d.RunE35(ds.Files, tier),
// "E36" => _d.RunE36(tier),  "E37" => _d.RunE37(tier),

// ResultWriter: добавить все 6 имён
```

---

## Ожидаемые результаты

| Exp | Baseline | Enhanced | Ключевой сигнал |
|---|---|---|---|
| E32 | 38.9% (медиана=3) | ~55% | ±1 accuracy ≥ 88% |
| E33 | ~70% (правила) | ~85% | false_continue_rate ↓ |
| E34 | ~45% (keywords) | ~80% | hit@1 на 10 классах |
| E35 | 65% (всегда SEQ) | ~82% | parallel_opportunity ≥ 50% |
| E36 | ~30% (самый частый) | ~70% | routing по 5 компонентам |
| E37 | ~50% (длиннейший) | ~75% | length_mismatch_rate |

---

*E32-E37 план v1.0 — OrchestratorModel · Подпроект 15 · не касается XIIGen*
