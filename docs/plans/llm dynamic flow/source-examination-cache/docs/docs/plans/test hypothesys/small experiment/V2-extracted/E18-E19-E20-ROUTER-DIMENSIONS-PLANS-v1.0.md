# E18 · E19 · E20 — Три измерения маршрутизатора
## Подробный план для Claude Code v1.0

**Проект:** `35.ExperimentsV2`
**Зависимости:** SemanticNormalizer (E15), FamilyCollapser (E16), LogicExtractor (E17)
**Этот документ не касается XIIGen.**

---

## Зачем три измерения

Маршрутизатор с одним измерением теряет информацию:
- "Fix null check in TypeScript service" — домен BUG_FIX, стек TS, один шаг
- "Plan the auth flow then implement it in C#" — домен PLAN+CODE_CHANGE, стек C#, два шага

Без E18 (домен) — непонятно какой датасет паттернов нужен.
Без E19 (сложность) — непонятно нужна ли декомпозиция задачи.
Без E20 (стек) — непонятно какой FamilyCollapser словарь применять и какие примеры показывать.

Все три работают **до** E21-E24: сначала определяем домен, сложность и стек,
потом разбиваем на сегменты и атрибутируем.

---

# E18 — DomainClassifier: к какому домену относится запрос

## Гипотеза

Предикатный профиль промпта (E17) содержит достаточно информации
чтобы классифицировать домен без ключевых слов.

"Why does getUserById return null when user exists" →
предикаты `[NULL_GUARD, GET_CALL, CONDITION]` → профиль BUG_FIX
(не потому что есть слово "why", а потому что NULL_GUARD + GET_CALL = типичный баг).

**Пять доменов:**

| Домен | Типичные предикаты | Пример |
|---|---|---|
| `CODE_QUESTION` | `GET_CALL, CONDITION` | "What does ValidateToken return when null" |
| `BUG_FIX` | `NULL_GUARD, FAIL, CONDITION` | "Fix NullReference in UserService" |
| `CODE_CHANGE` | `CREATE_OBJ, ASSIGN, VALIDATE_OP` | "Add null check to GetUserById" |
| `ORCHESTRATION` | `ITERATE, ASSIGN, SUCCESS` | "Plan phase A then implement flow-01" |
| `ARCHITECTURE` | `VALIDATE_OP, CREATE_OBJ` | "Design the auth module structure" |

**Урок из E22:** baseline должен быть честным.
Baseline = keyword classifier (не "всегда предсказывать всё").
Тогда Enhanced может показать настоящее улучшение.

## Обучающие данные: авто-разметка из ключевых слов

300 синтетических промптов — разметка детерминирована:

```
BUG_FIX:      слова "fix"/"error"/"exception"/"null"/"fail"/"why does"
CODE_CHANGE:  слова "add"/"create"/"update"/"refactor"/"remove"/"implement"
CODE_QUESTION: слова "what"/"how"/"explain"/"show"/"find"/"list"
ORCHESTRATION: слова "flow"/"phase"/"plan"/"step"/"sequence"/"after"/"then"
ARCHITECTURE:  слова "design"/"pattern"/"structure"/"best way"/"should"/"architect"
```

Такая разметка честна: keyword baseline использует те же ключевые слова → можно
показать что предикаты дают больше точности на неоднозначных случаях.

## Шаг 1 — Создать `DomainClassifier.cs`

**Путь:** `35.ExperimentsV2/Infrastructure/DomainClassifier.cs`

```csharp
namespace ExperimentsV2;

// DomainClassifier классифицирует домен запроса по предикатному профилю.
// Пять доменов: CODE_QUESTION, BUG_FIX, CODE_CHANGE, ORCHESTRATION, ARCHITECTURE.
public sealed class DomainClassifier
{
    public const int CODE_QUESTION = 0;
    public const int BUG_FIX       = 1;
    public const int CODE_CHANGE   = 2;
    public const int ORCHESTRATION = 3;
    public const int ARCHITECTURE  = 4;

    public static readonly string[] DomainNames =
        ["CODE_QUESTION", "BUG_FIX", "CODE_CHANGE", "ORCHESTRATION", "ARCHITECTURE"];

    // Ключевые слова для baseline
    private static readonly Dictionary<int, string[]> Keywords = new()
    {
        [BUG_FIX]       = ["fix","error","exception","null","fail","wrong","broken","why","debug"],
        [CODE_CHANGE]   = ["add","create","update","refactor","remove","implement","write","change"],
        [CODE_QUESTION] = ["what","how","explain","show","find","list","where","which"],
        [ORCHESTRATION] = ["flow","phase","plan","step","sequence","orchestrat","workflow"],
        [ARCHITECTURE]  = ["design","pattern","structure","architect","best","should","approach"],
    };

    private readonly List<(int[] Predicates, int Domain)> _train = new();
    private readonly int _k;

    public DomainClassifier(int k = 5) => _k = k;

    public void Train(IReadOnlyList<(int[] Predicates, int Domain)> examples)
    {
        _train.Clear();
        _train.AddRange(examples);
    }

    // Enhanced: K-NN на предикатах
    public int Predict(int[] predicates)
    {
        if (_train.Count == 0) return CODE_QUESTION;
        return _train
            .OrderBy(t => JaccardDist(predicates, t.Predicates))
            .Take(_k)
            .GroupBy(t => t.Domain)
            .OrderByDescending(g => g.Count())
            .First().Key;
    }

    // Baseline: keyword matching на нормализованном тексте
    public static int PredictByKeywords(string normalizedText)
    {
        var lower = normalizedText.ToLowerInvariant();
        // Приоритет: BUG_FIX > CODE_CHANGE > ORCHESTRATION > ARCHITECTURE > CODE_QUESTION
        foreach (var domain in new[] { BUG_FIX, CODE_CHANGE, ORCHESTRATION, ARCHITECTURE })
            if (Keywords[domain].Any(kw => lower.Contains(kw)))
                return domain;
        return CODE_QUESTION; // fallback
    }

    // Авто-разметка промпта для генерации датасета
    public static int AutoLabel(string prompt)
        => PredictByKeywords(prompt);

    private static double JaccardDist(int[] a, int[] b)
    {
        var sa = new HashSet<int>(a); var sb = new HashSet<int>(b);
        int i = sa.Count(x => sb.Contains(x));
        int u = sa.Count + sb.Count - i;
        return u == 0 ? 0 : 1.0 - (double)i / u;
    }
}
```

## Шаг 2 — Создать `E18DomainClassifier.cs`

**Путь:** `35.ExperimentsV2/Experiments/E18DomainClassifier.cs`

```csharp
namespace ExperimentsV2;

// E18: классификация домена запроса.
// Baseline:  keyword matching (детерминированный, честный)
// Enhanced:  K-NN на предикатах E17
// Обучение:  300 синтетических промптов с авто-разметкой
// Метрика:   top1_accuracy (5 классов, random = 20%)
public sealed class E18DomainClassifier
{
    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var examples = GenerateExamples(seed: 42, count: tier * 100);
        int split    = (int)(examples.Count * 0.8);
        var train    = examples.Take(split).ToList();
        var val      = examples.Skip(split).ToList();

        // Обучить K-NN на предикатах
        var classifier = new DomainClassifier(k: 5);
        classifier.Train(train.Select(e =>
        {
            var preds = TextToPredicates(e.Prompt);
            return (preds, e.Domain);
        }).ToList());

        // ── Baseline: keyword matching ────────────────────────────────────────
        var t0 = Stopwatch();
        int baseCorrect = val.Count(e =>
            DomainClassifier.PredictByKeywords(e.Prompt) == e.Domain);
        double baselineAcc = baseCorrect / (double)val.Count;
        double baselineMs  = ElapsedMs(t0);

        // ── Enhanced: K-NN на предикатах ─────────────────────────────────────
        var t1 = Stopwatch();
        int enhCorrect = val.Count(e =>
            classifier.Predict(TextToPredicates(e.Prompt)) == e.Domain);
        double enhancedAcc = enhCorrect / (double)val.Count;
        double enhancedMs  = ElapsedMs(t1);

        // Per-domain breakdown
        string perDomain = string.Join(", ",
            Enumerable.Range(0, DomainClassifier.DomainNames.Length).Select(d =>
            {
                var dVal   = val.Where(e => e.Domain == d).ToList();
                if (dVal.Count == 0) return $"{DomainClassifier.DomainNames[d]}=N/A";
                int correct = dVal.Count(e =>
                    classifier.Predict(TextToPredicates(e.Prompt)) == d);
                return $"{DomainClassifier.DomainNames[d]}={correct}/{dVal.Count}";
            }));

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E18-DOMAIN-CLASSIFIER",
            Tier              = tier,
            BaselinePrimary   = baselineAcc,
            EnhancedPrimary   = enhancedAcc,
            PrimaryMetricName = "top1_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = (double)examples.Count(e => IsAmbiguous(e.Prompt)) / examples.Count,
            ScalingSignalNote = "ambiguity_ratio (промпты без явных ключевых слов)",
            ShouldScaleUp     = val.Count < 30,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"examples={examples.Count}, val={val.Count}, " +
                    $"baseAcc={baselineAcc:P1}, enhAcc={enhancedAcc:P1}, " +
                    $"per_domain=[{perDomain}]"
        });
    }

    private static int[] TextToPredicates(string text)
    {
        var families = FamilyCollapser.CollapseString(text);
        return families.Select(f => f switch
        {
            "VALIDATE_OP" => LogicExtractor.VALIDATE_OP,
            "GET_OP"      => LogicExtractor.GET_CALL,
            "CREATE_OP"   => LogicExtractor.CREATE_OBJ,
            "ERROR"       => LogicExtractor.FAIL,
            "NULL"        => LogicExtractor.NULL_GUARD,
            "SUCCESS"     => LogicExtractor.SUCCESS,
            "SET_OP"      => LogicExtractor.ASSIGN,
            _             => LogicExtractor.UNKNOWN
        }).Where(p => p != LogicExtractor.UNKNOWN).ToArray();
    }

    private static bool IsAmbiguous(string p) =>
        !new[] {"fix","add","what","flow","design"}.Any(
            kw => p.ToLower().Contains(kw));

    private static List<(string Prompt, int Domain)> GenerateExamples(int seed, int count)
    {
        var rng = new Random(seed);
        var templates = new (string Template, int Domain)[]
        {
            // BUG_FIX
            ("Fix NullReferenceException in {svc}", DomainClassifier.BUG_FIX),
            ("Why does {method} return null when {obj} exists", DomainClassifier.BUG_FIX),
            ("Debug the {method} logic — it fails for {obj}", DomainClassifier.BUG_FIX),
            ("Error: {obj} is null after calling {method}", DomainClassifier.BUG_FIX),
            ("The {svc} throws exception when {obj} is missing", DomainClassifier.BUG_FIX),
            // CODE_CHANGE
            ("Add null check to {method} before accessing {prop}", DomainClassifier.CODE_CHANGE),
            ("Refactor {svc} to use the new {method}", DomainClassifier.CODE_CHANGE),
            ("Update {method} to return OperationResult", DomainClassifier.CODE_CHANGE),
            ("Create {method} in {svc} that validates {obj}", DomainClassifier.CODE_CHANGE),
            ("Remove the duplicate {method} from {svc}", DomainClassifier.CODE_CHANGE),
            // CODE_QUESTION
            ("What does {method} return when {obj} is null", DomainClassifier.CODE_QUESTION),
            ("How does {svc} handle {obj} validation", DomainClassifier.CODE_QUESTION),
            ("Explain the {method} logic in {svc}", DomainClassifier.CODE_QUESTION),
            ("Find all methods that call {method}", DomainClassifier.CODE_QUESTION),
            ("Show the flow of {obj} through {svc}", DomainClassifier.CODE_QUESTION),
            // ORCHESTRATION
            ("Plan phase A of flow-01 for {svc}", DomainClassifier.ORCHESTRATION),
            ("Create the step sequence for {method} workflow", DomainClassifier.ORCHESTRATION),
            ("Design the flow for {obj} processing pipeline", DomainClassifier.ORCHESTRATION),
            ("Outline the phases for implementing {svc}", DomainClassifier.ORCHESTRATION),
            ("What steps are needed to deploy {svc}", DomainClassifier.ORCHESTRATION),
            // ARCHITECTURE
            ("Design the architecture for {svc}", DomainClassifier.ARCHITECTURE),
            ("What is the best pattern for {obj} validation", DomainClassifier.ARCHITECTURE),
            ("How should {svc} be structured for multi-tenant", DomainClassifier.ARCHITECTURE),
            ("Architect the {method} module for scalability", DomainClassifier.ARCHITECTURE),
            ("What approach should {svc} use for {obj}", DomainClassifier.ARCHITECTURE),
        };

        var svcs    = new[] {"UserService","AuthService","TokenService","PaymentService","OrderService"};
        var methods = new[] {"GetUser","ValidateToken","ProcessPayment","BuildSearchFilter","CreateSession"};
        var objs    = new[] {"user","token","payment","order","session","tenant","config"};
        var props   = new[] {"Id","Name","Email","Status","CreatedAt","TenantId"};

        var result = new List<(string, int)>();
        while (result.Count < count)
        {
            var (tmpl, domain) = templates[rng.Next(templates.Length)];
            string prompt = tmpl
                .Replace("{svc}",    svcs[rng.Next(svcs.Length)])
                .Replace("{method}", methods[rng.Next(methods.Length)])
                .Replace("{obj}",    objs[rng.Next(objs.Length)])
                .Replace("{prop}",   props[rng.Next(props.Length)]);
            result.Add((prompt, domain));
        }
        return result;
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
```

## Добавить в Dispatcher/Runner/ResultWriter

```csharp
// Dispatcher: RunE18(int tier = 1) => new E18DomainClassifier().Run(tier);
// Runner: "E18" => _d.RunE18(tier),  (не нужен ds — датасет генерируется внутри)
// ResultWriter: "E18-DOMAIN-CLASSIFIER" => "E18-DOMAIN-CLASSIFIER",
```

## Ожидаемый вывод

```
OK top1_accuracy (up) baseline=0.730 enhanced=0.840 delta=+15.1% | PASSED
notes: examples=100, val=20, baseAcc=73.0%, enhAcc=84.0%,
       per_domain=[CODE_QUESTION=4/5, BUG_FIX=5/5, CODE_CHANGE=4/4, ORCH=3/4, ARCH=1/2]
```

Keyword baseline уже хорошо работает (~73%) потому что промпты сгенерированы с явными ключевыми словами. Улучшение Enhanced (+11%) будет видно на двусмысленных промптах без явных ключевых слов — именно для них K-NN на предикатах работает.

---

---

# E19 — ComplexityRouter: один шаг или план из шагов

## Гипотеза

Сложность запроса определяет как с ним работать:
- `SINGLE_STEP` — один VERB-NOUN, один ответ, один датасет
- `MULTI_STEP` — несколько VERB-NOUN через CONJ, нужна декомпозиция

**Проверяемое утверждение:** количество переходов между разными
FamilyCollapser-тегами предсказывает MULTI_STEP точнее чем просто
наличие союзов.

Почему союзы плохо работают (урок из E21):
- "Find null **and** log them" — союз есть, но это SINGLE_STEP
- "Add auth **and** write tests" — союз есть, и это MULTI_STEP

Разница: в первом случае оба глагола относятся к одной операции
(тег GET_OP + VALIDATE_OP в одной цепочке). Во втором случае цепочки
разные (CREATE_OP/AUTH_OP vs CREATE_OBJ/VALIDATE_OP).

## Метрика и честный baseline

Baseline: подсчитать число союзов ≥ 1 → MULTI_STEP.
Это работает в ~65-70% случаев и честно ошибается на ложных CONJ.
Enhanced: число **переходов между разными FamilyCollapser-тегами** > порог.

## Шаг 1 — Создать `ComplexityRouter.cs`

**Путь:** `35.ExperimentsV2/Infrastructure/ComplexityRouter.cs`

```csharp
namespace ExperimentsV2;

// ComplexityRouter определяет сложность запроса: SINGLE_STEP или MULTI_STEP.
// SINGLE_STEP — один ответ, одна операция.
// MULTI_STEP  — нужна декомпозиция на подзадачи.
public sealed class ComplexityRouter
{
    public const int SINGLE_STEP = 0;
    public const int MULTI_STEP  = 1;

    // Количество переходов между разными тегами FamilyCollapser выше которого = MULTI_STEP
    private readonly int _transitionThreshold;

    public ComplexityRouter(int transitionThreshold = 3)
        => _transitionThreshold = transitionThreshold;

    // Enhanced: считать переходы между разными семейными тегами
    public int Predict(string prompt)
    {
        var families = FamilyCollapser.CollapseString(prompt);
        if (families.Count < 2) return SINGLE_STEP;

        int transitions = 0;
        for (int i = 1; i < families.Count; i++)
            if (families[i] != families[i - 1])
                transitions++;

        return transitions >= _transitionThreshold ? MULTI_STEP : SINGLE_STEP;
    }

    // Baseline: наличие союзного слова
    public static int PredictByConj(string prompt)
    {
        var lower   = prompt.ToLowerInvariant();
        var conjWords = new[] {" and ", " then ", " after that ", " also ", " plus "};
        return conjWords.Any(c => lower.Contains(c)) ? MULTI_STEP : SINGLE_STEP;
    }

    // Подобрать оптимальный порог на обучающих данных
    public static int TuneThreshold(List<(string Prompt, int Complexity)> train)
    {
        int best = 3; int bestScore = 0;
        for (int t = 1; t <= 8; t++)
        {
            var r    = new ComplexityRouter(t);
            int score = train.Count(e => r.Predict(e.Prompt) == e.Complexity);
            if (score > bestScore) { bestScore = score; best = t; }
        }
        return best;
    }
}
```

## Шаг 2 — Создать `E19ComplexityRouter.cs`

**Путь:** `35.ExperimentsV2/Experiments/E19ComplexityRouter.cs`

```csharp
namespace ExperimentsV2;

// E19: классификация сложности запроса.
// Baseline:  наличие союзного слова (≥1 and/then/also → MULTI_STEP)
// Enhanced:  число переходов между FamilyCollapser-тегами > threshold
// Обучение:  200 синтетических примеров (100 single, 100 multi, + ложные CONJ)
// Метрика:   binary accuracy + false_positive_rate (ложные CONJ)
public sealed class E19ComplexityRouter
{
    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var examples = GenerateExamples(seed: 42, count: tier * 80);
        int split    = (int)(examples.Count * 0.8);
        var train    = examples.Take(split).ToList();
        var val      = examples.Skip(split).ToList();

        // Подобрать threshold на train
        int bestThreshold = ComplexityRouter.TuneThreshold(train);
        var router = new ComplexityRouter(bestThreshold);

        // ── Baseline: союзное слово ───────────────────────────────────────────
        var t0 = Stopwatch();
        int baseCorrect = val.Count(e =>
            ComplexityRouter.PredictByConj(e.Prompt) == e.Complexity);
        // False positives: ложные CONJ (обозначены как SINGLE_STEP)
        var falseSingles = val.Where(e => e.Complexity == ComplexityRouter.SINGLE_STEP).ToList();
        int baseFP = falseSingles.Count(e =>
            ComplexityRouter.PredictByConj(e.Prompt) == ComplexityRouter.MULTI_STEP);
        double baselineAcc = baseCorrect / (double)val.Count;
        double baseFPR     = falseSingles.Count > 0 ? baseFP / (double)falseSingles.Count : 0;
        double baselineMs  = ElapsedMs(t0);

        // ── Enhanced: переходы тегов ──────────────────────────────────────────
        var t1 = Stopwatch();
        int enhCorrect = val.Count(e => router.Predict(e.Prompt) == e.Complexity);
        int enhFP = falseSingles.Count(e =>
            router.Predict(e.Prompt) == ComplexityRouter.MULTI_STEP);
        double enhancedAcc = enhCorrect / (double)val.Count;
        double enhFPR      = falseSingles.Count > 0 ? enhFP / (double)falseSingles.Count : 0;
        double enhancedMs  = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E19-COMPLEXITY-ROUTER",
            Tier              = tier,
            BaselinePrimary   = baselineAcc,
            EnhancedPrimary   = enhancedAcc,
            PrimaryMetricName = "binary_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = baseFPR - enhFPR,
            ScalingSignalNote = "fpr_reduction (насколько меньше ложных MULTI_STEP)",
            ShouldScaleUp     = val.Count < 20,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"examples={examples.Count}, threshold={bestThreshold}, " +
                    $"baseAcc={baselineAcc:P1} FPR={baseFPR:P1}, " +
                    $"enhAcc={enhancedAcc:P1} FPR={enhFPR:P1}, " +
                    $"fpr_delta={baseFPR - enhFPR:+.1%}"
        });
    }

    private static List<(string Prompt, int Complexity)> GenerateExamples(int seed, int count)
    {
        var rng = new Random(seed);
        var singleTemplates = new[]
        {
            "Add null check to {method} before accessing {prop}",
            "Fix NullReference in {svc} when {obj} is missing",
            "What does {method} return when {obj} is null",
            "Refactor {svc} to use the new {method}",
            "Explain how {svc} handles {obj} validation",
            // Ложные CONJ — один шаг с союзом
            "Find methods that return null and log them",
            "Check null and empty before accessing {prop}",
            "Validate {obj} and return result",
            "Read {obj} and write output",
            "Get {obj} and process it",
        };
        var multiTemplates = new[]
        {
            "Add null check to {method} and write unit tests",
            "Fix {svc} then refactor the validation logic",
            "Create {method} in {svc} and update the tests",
            "Refactor {svc} and then plan the new flow structure",
            "Add {method} to {svc} and write integration tests for edge cases",
        };
        var svcs    = new[] {"UserService","AuthService","TokenService"};
        var methods = new[] {"GetUser","ValidateToken","BuildSearchFilter"};
        var objs    = new[] {"user","token","order","session"};
        var props   = new[] {"Id","Name","TenantId"};

        string Fill(string tmpl) => tmpl
            .Replace("{svc}",    svcs[rng.Next(svcs.Length)])
            .Replace("{method}", methods[rng.Next(methods.Length)])
            .Replace("{obj}",    objs[rng.Next(objs.Length)])
            .Replace("{prop}",   props[rng.Next(props.Length)]);

        var result = new List<(string, int)>();
        while (result.Count < count)
        {
            bool isMulti = rng.Next(2) == 0;
            var templates = isMulti ? multiTemplates : singleTemplates;
            result.Add((Fill(templates[rng.Next(templates.Length)]),
                isMulti ? ComplexityRouter.MULTI_STEP : ComplexityRouter.SINGLE_STEP));
        }
        return result;
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
```

## Добавить в Dispatcher/Runner/ResultWriter

```csharp
// Dispatcher: RunE19(int tier = 1) => new E19ComplexityRouter().Run(tier);
// Runner: "E19" => _d.RunE19(tier),
// ResultWriter: "E19-COMPLEXITY-ROUTER" => "E19-COMPLEXITY-ROUTER",
```

## Ожидаемый вывод

```
OK binary_accuracy (up) baseline=0.700 enhanced=0.833 delta=+19.0% | PASSED
notes: examples=80, threshold=3, baseAcc=70.0% FPR=40.0%, enhAcc=83.3% FPR=15.0%,
       fpr_delta=+25.0%
```

Ключевой сигнал — `fpr_reduction`: Enhanced делает меньше ложных "это MULTI_STEP"
на ложных CONJ ("Find null **and** log them").

---

---

# E20 — StackDetector: определить стек технологий

## Гипотеза

После E15-нормализации стек-специфичные слова **остаются**:
- C#: `task`, `async`, `await`, `ienumerable`, `cancellation` — не удаляются E15
- TypeScript: `promise`, `injectable`, `interface`, `export`, `readonly`
- Python: `def`, `none`, `yield`, `comprehension`, `tuple`

Эти слова не входят в Decorators-словарь E15 и не трансформируются
FamilyCollapser E16 — они доходят до E20 как есть.

K-NN по bag-of-words этих стек-специфичных токенов предсказывает стек
точнее чем случайный классификатор (25%).

**Важный вывод после E20:**
- Если PASSED → стек определяется из нормализованного текста → E15-нормализация
  применяется после детекции стека → каждый стек получает свой FamilyCollapser-словарь
- Если FAILED → стек-маркеры не выживают после E15 → детекцию стека надо делать
  до нормализации (на сырых токенах)

## Синтетические данные

Четыре стека, коды генерируются программно — не нужны реальные файлы TS/Python:

```
C#/.NET:    namespace X { class Y { async Task<T> Method() { ... } } }
TypeScript: @Injectable() export class Y { async method(): Promise<T> { ... } }
Python:     def method(self) -> Optional[T]: return None if not x else x
Generic:    class Y { method(x) { if x == null: return null; } }
```

## Шаг 1 — Создать `StackDetector.cs`

**Путь:** `35.ExperimentsV2/Infrastructure/StackDetector.cs`

```csharp
namespace ExperimentsV2;

// StackDetector определяет технологический стек по стек-специфичным токенам
// которые выживают после E15-нормализации.
public sealed class StackDetector
{
    public const int CSHARP     = 0;
    public const int TYPESCRIPT = 1;
    public const int PYTHON     = 2;
    public const int GENERIC    = 3;

    public static readonly string[] StackNames =
        ["CSharp", "TypeScript", "Python", "Generic"];

    // Стек-специфичные маркеры которые E15 НЕ удаляет
    private static readonly Dictionary<int, HashSet<string>> Markers = new()
    {
        [CSHARP] = new(StringComparer.OrdinalIgnoreCase)
        {
            "task","ienumerable","cancellation","namespace","csharp","dotnet",
            "nullable","linq","delegate","struct","sealed","override",
            "partial","extern","unsafe","checked","stackalloc"
        },
        [TYPESCRIPT] = new(StringComparer.OrdinalIgnoreCase)
        {
            "promise","injectable","interface","export","readonly","enum",
            "typescript","nestjs","angular","rxjs","observable","decorator",
            "generics","union","intersection","keyof","typeof"
        },
        [PYTHON] = new(StringComparer.OrdinalIgnoreCase)
        {
            "def","none","yield","tuple","dict","list","optional","typing",
            "python","django","flask","fastapi","pydantic","dataclass",
            "comprehension","generator","lambda","unpacking"
        },
    };

    private readonly List<(float[] Features, int Stack)> _train = new();
    private readonly int _k;

    public StackDetector(int k = 3) => _k = k;

    public void Train(IReadOnlyList<(string NormalizedText, int Stack)> examples)
    {
        _train.Clear();
        foreach (var (text, stack) in examples)
            _train.Add((BuildFeatureVector(text), stack));
    }

    // Enhanced: K-NN на feature vector (частоты маркеров)
    public int Predict(string normalizedText)
    {
        if (_train.Count == 0) return GENERIC;
        var q = BuildFeatureVector(normalizedText);
        return _train
            .OrderBy(t => CosineDistance(q, t.Features))
            .Take(_k)
            .GroupBy(t => t.Stack)
            .OrderByDescending(g => g.Count())
            .First().Key;
    }

    // Baseline: считать маркеры по стекам, взять максимум
    public static int PredictByMarkers(string normalizedText)
    {
        var words = normalizedText.ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet();

        var scores = new int[4];
        for (int s = 0; s < 3; s++)  // не считаем GENERIC
            scores[s] = Markers[s].Count(m => words.Contains(m));

        int maxScore = scores[..3].Max();
        if (maxScore == 0) return GENERIC;
        return Array.IndexOf(scores[..3], maxScore);
    }

    // Вектор: нормализованные частоты маркеров каждого стека
    private static float[] BuildFeatureVector(string normalizedText)
    {
        var words  = normalizedText.ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet();
        var result = new float[3];  // CSharp, TypeScript, Python
        for (int s = 0; s < 3; s++)
            result[s] = Markers[s].Count(m => words.Contains(m)) / (float)(Markers[s].Count);
        return result;
    }

    private static double CosineDistance(float[] a, float[] b)
    {
        double dot = 0, na = 0, nb = 0;
        for (int i = 0; i < Math.Min(a.Length, b.Length); i++)
        {
            dot += a[i] * b[i]; na += a[i] * a[i]; nb += b[i] * b[i];
        }
        double d = Math.Sqrt(na) * Math.Sqrt(nb);
        return d < 1e-9 ? 1.0 : 1.0 - dot / d;
    }

    // Автоматически сгенерировать код для каждого стека
    public static List<(string Code, int Stack)> GenerateSyntheticExamples(int seed, int perStack)
    {
        var rng     = new Random(seed);
        var result  = new List<(string, int)>();
        var methods = new[] {"GetUser","ValidateToken","ProcessPayment","BuildFilter"};
        var types   = new[] {"User","Token","Payment","Result"};

        // C# шаблоны
        for (int i = 0; i < perStack; i++)
        {
            string m = methods[rng.Next(methods.Length)], t = types[rng.Next(types.Length)];
            result.Add(($"namespace App {{ sealed class {t}Service {{ async Task<{t}?> {m}(IEnumerable<int> ids, CancellationToken ct) {{ if (ids == null) return null; return await _repo.GetAsync(ids, ct); }} }} }}", CSHARP));
        }
        // TypeScript шаблоны
        for (int i = 0; i < perStack; i++)
        {
            string m = methods[rng.Next(methods.Length)], t = types[rng.Next(types.Length)];
            result.Add(($"@Injectable() export class {t}Service {{ async {m}(ids: readonly string[]): Promise<{t} | undefined> {{ if (!ids) return undefined; return await this.repo.findOne(ids); }} }}", TYPESCRIPT));
        }
        // Python шаблоны
        for (int i = 0; i < perStack; i++)
        {
            string m = methods[rng.Next(methods.Length)], t = types[rng.Next(types.Length)];
            result.Add(($"from typing import Optional, List\nclass {t}Service:\n    async def {m}(self, ids: List[int]) -> Optional[{t}]:\n        if not ids:\n            return None\n        return await self.repo.get(ids)", PYTHON));
        }
        // Generic шаблоны (без стек-маркеров)
        for (int i = 0; i < perStack; i++)
        {
            string m = methods[rng.Next(methods.Length)], t = types[rng.Next(types.Length)];
            result.Add(($"class {t}Service {{ method {m}(ids) {{ if ids == null return null result = repo.get(ids) return result }} }}", GENERIC));
        }
        return result;
    }
}
```

## Шаг 2 — Создать `E20StackDetector.cs`

**Путь:** `35.ExperimentsV2/Experiments/E20StackDetector.cs`

```csharp
namespace ExperimentsV2;

// E20: определение технологического стека.
// Baseline:  подсчёт стек-специфичных маркеров (marker voting)
// Enhanced:  K-NN на нормализованных частотах маркеров
// Обучение:  синтетические примеры кода × 4 стека (генерируются программно)
// Метрика:   top1_accuracy (4 класса, random = 25%)
//
// Ключевой вопрос: выживают ли стек-маркеры после E15-нормализации?
// Scaling signal: marker_survival_rate — % маркеров которые E15 не удаляет
public sealed class E20StackDetector
{
    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        int perStack = tier * 15;
        var rawExamples = StackDetector.GenerateSyntheticExamples(seed: 42, perStack);

        // Прогнать через E15 нормализацию
        var examples = rawExamples
            .Select(e => (
                Normalized: SemanticNormalizer.NormalizeToString(e.Code),
                Stack:       e.Stack))
            .ToList();

        // Измерить выживаемость маркеров после нормализации
        int totalMarkers = 0; int survivedMarkers = 0;
        foreach (var (norm, stack) in examples.Where(e => e.Stack < 3))
        {
            var words = norm.ToLowerInvariant().Split(' ').ToHashSet();
            // Получить маркеры через reflection-like подход
            // (упрощённо: просто проверим несколько известных маркеров)
            var knownMarkers = new[] { "task","promise","def","injectable","ienumerable","yield" };
            totalMarkers += knownMarkers.Length;
            survivedMarkers += knownMarkers.Count(m => words.Contains(m));
        }
        double survivalRate = totalMarkers > 0 ? survivedMarkers / (double)totalMarkers : 0;

        int split = (int)(examples.Count * 0.8);
        var train = examples.Take(split).ToList();
        var val   = examples.Skip(split).ToList();

        var detector = new StackDetector(k: 3);
        detector.Train(train);

        // ── Baseline: маркерное голосование ───────────────────────────────────
        var t0 = Stopwatch();
        int baseCorrect = val.Count(e =>
            StackDetector.PredictByMarkers(e.Normalized) == e.Stack);
        double baselineAcc = baseCorrect / (double)val.Count;
        double baselineMs  = ElapsedMs(t0);

        // ── Enhanced: K-NN ────────────────────────────────────────────────────
        var t1 = Stopwatch();
        int enhCorrect = val.Count(e =>
            detector.Predict(e.Normalized) == e.Stack);
        double enhancedAcc = enhCorrect / (double)val.Count;
        double enhancedMs  = ElapsedMs(t1);

        // Per-stack breakdown
        string perStack2 = string.Join(", ", Enumerable.Range(0, 4).Select(s =>
        {
            var sv = val.Where(e => e.Stack == s).ToList();
            if (sv.Count == 0) return $"{StackDetector.StackNames[s]}=N/A";
            int c  = sv.Count(e => detector.Predict(e.Normalized) == s);
            return $"{StackDetector.StackNames[s]}={c}/{sv.Count}";
        }));

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E20-STACK-DETECTOR",
            Tier              = tier,
            BaselinePrimary   = baselineAcc,
            EnhancedPrimary   = enhancedAcc,
            PrimaryMetricName = "top1_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = survivalRate,
            ScalingSignalNote = "marker_survival_rate (доля стек-маркеров после E15)",
            // Если выживаемость низкая — E15 убивает маркеры, нужно детектировать до нормализации
            ShouldScaleUp     = survivalRate < 0.3 || val.Count < 20,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"perStack={perStack}, survival={survivalRate:P1}, " +
                    $"baseAcc={baselineAcc:P1}, enhAcc={enhancedAcc:P1}, " +
                    $"per_stack=[{perStack2}]"
        });
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
```

## Добавить в Dispatcher/Runner/ResultWriter

```csharp
// Dispatcher: RunE20(int tier = 1) => new E20StackDetector().Run(tier);
// Runner: "E20" => _d.RunE20(tier),
// ResultWriter: "E20-STACK-DETECTOR" => "E20-STACK-DETECTOR",
```

## Ожидаемый вывод — два возможных сценария

**Сценарий A (стек-маркеры выживают — survival > 0.5):**
```
OK top1_accuracy (up) baseline=0.750 enhanced=0.917 delta=+22.2% | PASSED
notes: perStack=15, survival=62.0%, baseAcc=75.0%, enhAcc=91.7%,
       per_stack=[CSharp=12/15, TypeScript=13/15, Python=14/15, Generic=10/15]
```
→ Вывод: E15-нормализацию применять ПОСЛЕ детекции стека.
  Каждый стек получает свой FamilyCollapser-словарь.

**Сценарий B (стек-маркеры не выживают — survival < 0.3):**
```
FAILED top1_accuracy (up) baseline=0.500 enhanced=0.500 delta=0.0%
notes: perStack=15, survival=18.0%, ...
```
→ Вывод: детектировать стек до нормализации на raw-токенах.
  E20 надо запускать до E15 в пайплайне.

---

## Полный маршрутизатор: все три измерения вместе

```
Промпт
  ├─ E20 StackDetector  → CSharp | TypeScript | Python | Generic
  │        │
  │        └── выбрать FamilyCollapser-словарь для этого стека
  │
  ├─ E18 DomainClassifier → BUG_FIX | CODE_CHANGE | CODE_QUESTION | ORCH | ARCH
  │        │
  │        └── выбрать поддатасет паттернов для этого домена
  │
  ├─ E19 ComplexityRouter → SINGLE_STEP | MULTI_STEP
  │        │
  │        └── если MULTI_STEP → E21 PromptSplitter → E22/E24 атрибуция
  │
  └─ Модель получает:
       - подпромпт для своего датасета (E24)
       - через правильный FamilyCollapser для своего стека (E20)
       - с правильным поддатасетом для своего домена (E18)
```

---

## Сводная таблица E18–E20

| Exp | Гипотеза | Baseline | Enhanced | Метрика | Ключевой вопрос |
|---|---|---|---|---|---|
| E18 | Предикаты определяют домен точнее ключевых слов | Keyword matching (~73%) | K-NN на предикатах | top1_accuracy (5 кл.) | Улучшение на неоднозначных промптах |
| E19 | Переходы тегов точнее CONJ для MULTI_STEP | CONJ count (~70%) | Tag transition count | binary_accuracy + FPR | Снижение ложных MULTI_STEP |
| E20 | Стек-маркеры выживают после E15 | Marker voting (~75%) | K-NN на vectors | top1_accuracy (4 кл.) | Выживаемость маркеров (порог 30%) |

---

*E18/E19/E20 план v1.0 — три измерения маршрутизатора · не касается XIIGen*
