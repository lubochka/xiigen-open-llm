# E21 · E22 · E23 — Маршрутизация и Beautifier
## Подробный план для Claude Code v1.0

**Проект:** `35.ExperimentsV2`
**Датасет:** синтетический (генерируется программно) + `llm_mvp_core`
**Этот документ не касается XIIGen.**

---

## Зачем нужна маршрутизация: центральная идея

Сейчас одна модель видит все запросы через одно окно контекста.
Маршрутизация позволяет:

1. **Меньший словарь** — каждый поддатасет обучает модель на узком vocab
2. **Быстрее обучение** — меньше примеров нужно для насыщения (как E17: vocab=12, perplexity=5.6)
3. **Выше точность** — NestJS-вопрос не "знает" о C# шаблонах и наоборот
4. **Гибкость** — новый стек = новый поддатасет, не переобучение всего

**Ключевой принцип для E22:** модель не угадывает следующий токен —
она выдаёт **бинарный вектор** (один бит на поддатасет).
1 = этот датасет релевантен для данного запроса.
Несколько единиц = несколько датасетов используются одновременно.

---

# E21 — PromptSplitter: разделить запрос на сегменты

## Гипотеза

Запрос пользователя может содержать несколько независимых задач:
`"Add auth to UserService AND write unit tests for it"`

Это **два** разных сегмента с разными доменами и разными датасетами.
Если не разделить — ответ будет смешанным и менее точным.

**Проверяемое утверждение:** смена профиля E17-предикатов внутри промпта
предсказывает границу между сегментами точнее чем поиск союзов
(`and`, `then`, `also`).

Почему союзы плохо работают:
- "Find methods that return null **and** log them" — один сегмент, не два
- "Add auth to UserService **and** write tests" — два сегмента

Разница не в слове "and", а в том что глагольные группы независимы
(`Add auth` и `write tests` — разные действия).

## Обучающие данные

Синтетические — генерируются программно без разметки вручную:

```
Однодоменные фразы:
  "Add GetUserById to UserService"   → [CODE_CHANGE]
  "Find methods without null check"  → [CODE_QUESTION]
  "Write unit tests for AuthService" → [TEST]
  "Fix NullReferenceException in"    → [BUG_FIX]

Двухдоменные (соединить через CONJ):
  "Add GetUserById to UserService AND write unit tests" → [CODE_CHANGE, TEST]
  "Fix NullReference THEN refactor the validation"     → [BUG_FIX, CODE_CHANGE]

Ложные разрывы (CONJ внутри одного сегмента):
  "Find methods that return null and log them"    → [CODE_QUESTION] (1 сегмент!)
  "Check null and empty before accessing .Name"   → [CODE_QUESTION] (1 сегмент!)
```

Генератор создаёт 300 примеров (100 одноdomain, 100 двух, 100 ложных).

## Baseline

Split по CONJ-токену (E08 RoleBasedTaskDecomposer уже реализован):
- Найти CONJ между двумя VERB-NOUN группами → граница
- Точность на 300 примерах: ожидаем ~71% (ложные разрывы дают FP)

## Enhanced

Split по **предикатному профилю**:
- Разбить промпт на окна по 3 токена
- Вычислить распределение E17-предикатов в каждом окне
- Если распределение меняется существенно (cosine distance > threshold) → граница
- Threshold подбирается на validation set

Ключевое отличие: "Find null **and** log them":
- Окно 1: `[NULL, GET_CALL]` → профиль CODE_QUESTION
- Окно 2: `[NULL, GET_CALL]` → тот же профиль → НЕ граница ✓

"Add auth **and** write tests":
- Окно 1: `[AUTH_OP, CREATE_OP]` → профиль CODE_CHANGE
- Окно 2: `[CREATE_OBJ, VALIDATE_OP]` → другой профиль TEST → ГРАНИЦА ✓

## Шаг 1 — Создать `PromptSplitter.cs`

**Путь:** `35.ExperimentsV2/Infrastructure/PromptSplitter.cs`

```csharp
namespace ExperimentsV2;

// PromptSplitter разбивает промпт на сегменты по смене предикатного профиля.
// Каждый сегмент = независимая задача с возможно разным доменом.
public sealed class PromptSplitter
{
    private readonly double _changeThreshold;  // порог изменения профиля
    private readonly int    _windowSize;       // размер скользящего окна

    public PromptSplitter(double changeThreshold = 0.4, int windowSize = 3)
    {
        _changeThreshold = changeThreshold;
        _windowSize      = windowSize;
    }

    // Разбить текст на сегменты по смене предикатного профиля.
    // Возвращает список сегментов (подстрок исходного промпта).
    public OperationResult<IReadOnlyList<PromptSegment>> Split(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            return OperationResult<IReadOnlyList<PromptSegment>>.Fail("Пустой промпт");

        // 1. Нормализовать промпт через SemanticNormalizer
        var words = SemanticNormalizer.Normalize(prompt);
        if (words.Count < 3)
            return OperationResult<IReadOnlyList<PromptSegment>>.Ok(
                new[] { new PromptSegment(prompt, 0, prompt.Length - 1) });

        // 2. Преобразовать каждое слово в предикат через FamilyCollapser
        var predicates = FamilyCollapser.Collapse(words);

        // 3. Вычислить профиль (частоты предикатов) для каждого окна
        var boundaries = new List<int>(); // позиции границ в исходном слове-списке
        for (int i = _windowSize; i < predicates.Count - _windowSize; i++)
        {
            var leftProfile  = GetProfile(predicates, Math.Max(0, i - _windowSize), i);
            var rightProfile = GetProfile(predicates, i, Math.Min(predicates.Count, i + _windowSize));
            double dist = CosineDist(leftProfile, rightProfile);
            if (dist > _changeThreshold)
                boundaries.Add(i);
        }

        // 4. Убрать границы ближе чем 2 позиции друг от друга (шум)
        var cleanBoundaries = MergeCloseBoundaries(boundaries, minGap: 2);

        // 5. Построить сегменты из слов обратно в подстроки
        var segments = WordsToSegments(prompt, words, cleanBoundaries);

        return OperationResult<IReadOnlyList<PromptSegment>>.Ok(segments);
    }

    // Profiler: count of each unique predicate in a window
    private static Dictionary<string, double> GetProfile(
        List<string> predicates, int start, int end)
    {
        var profile = new Dictionary<string, double>();
        for (int i = start; i < end; i++)
            profile[predicates[i]] = profile.GetValueOrDefault(predicates[i], 0) + 1;
        // Normalize
        double total = profile.Values.Sum();
        if (total > 0)
            foreach (var k in profile.Keys.ToList())
                profile[k] /= total;
        return profile;
    }

    private static double CosineDist(Dictionary<string, double> a, Dictionary<string, double> b)
    {
        var allKeys = a.Keys.Union(b.Keys).ToList();
        double dot = allKeys.Sum(k => a.GetValueOrDefault(k, 0) * b.GetValueOrDefault(k, 0));
        double na  = Math.Sqrt(a.Values.Sum(v => v * v));
        double nb  = Math.Sqrt(b.Values.Sum(v => v * v));
        return na < 1e-9 || nb < 1e-9 ? 1.0 : 1.0 - dot / (na * nb);
    }

    private static List<int> MergeCloseBoundaries(List<int> bs, int minGap)
    {
        if (bs.Count < 2) return bs;
        var result = new List<int> { bs[0] };
        for (int i = 1; i < bs.Count; i++)
            if (bs[i] - result[^1] >= minGap)
                result.Add(bs[i]);
        return result;
    }

    private static IReadOnlyList<PromptSegment> WordsToSegments(
        string original, List<string> words, List<int> boundaries)
    {
        // Map word positions back to approximate character offsets
        // (simplified — split original by spaces and slice)
        var rawWords  = original.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var segments  = new List<PromptSegment>();
        var allBounds = new[] { 0 }.Concat(boundaries)
                                    .Append(words.Count).ToArray();
        for (int i = 0; i < allBounds.Length - 1; i++)
        {
            int wStart = allBounds[i];
            int wEnd   = Math.Min(allBounds[i + 1], rawWords.Length);
            if (wStart >= wEnd) continue;
            string text = string.Join(" ", rawWords[wStart..wEnd]);
            segments.Add(new PromptSegment(text, wStart, wEnd - 1));
        }
        return segments.AsReadOnly();
    }
}

public record PromptSegment(string Text, int WordStart, int WordEnd);
```

## Шаг 2 — Создать `E21PromptSplitter.cs`

**Путь:** `35.ExperimentsV2/Experiments/E21PromptSplitter.cs`

```csharp
namespace ExperimentsV2;

// E21: PromptSplitter — разделение промпта по предикатному профилю.
// Baseline:  split по CONJ-токену (E08 роли: CONJ между VERB-группами)
// Enhanced:  split по cosine-расстоянию между профилями предикатов
//
// Обучение:  300 синтетических примеров (генерируются программно)
// Валидация: boundary_accuracy на hidden 100 примерах (20% split)
// Сравнение: сколько примеров с правильным количеством сегментов
public sealed class E21PromptSplitter
{
    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        // Генерировать датасет программно
        var examples = GenerateExamples(seed: 42);
        int trainN   = (int)(examples.Count * 0.8);
        var train    = examples.Take(trainN).ToList();
        var val      = examples.Skip(trainN).ToList();

        var splitter = new PromptSplitter(changeThreshold: 0.4, windowSize: 3);
        // Подобрать threshold на train
        double bestThreshold = TuneThreshold(train);
        splitter = new PromptSplitter(changeThreshold: bestThreshold);

        // ── Baseline: split по CONJ ───────────────────────────────────────────
        var t0 = Stopwatch();
        int baseCorrect = val.Count(ex =>
            ConjSplit(ex.Prompt).Count == ex.ExpectedSegments);
        double baselineAcc = baseCorrect / (double)val.Count;
        double baselineMs  = ElapsedMs(t0);

        // ── Enhanced: предикатный профиль ────────────────────────────────────
        var t1 = Stopwatch();
        int enhCorrect = 0;
        int falsePositives = 0, falseNegatives = 0;
        foreach (var ex in val)
        {
            var result = splitter.Split(ex.Prompt);
            if (!result.IsSuccess) continue;
            int got = result.Value!.Count;
            if (got == ex.ExpectedSegments) enhCorrect++;
            else if (got > ex.ExpectedSegments) falsePositives++;
            else falseNegatives++;
        }
        double enhancedAcc = enhCorrect / (double)val.Count;
        double enhancedMs  = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E21-PROMPT-SPLITTER",
            Tier              = tier,
            BaselinePrimary   = baselineAcc,
            EnhancedPrimary   = enhancedAcc,
            PrimaryMetricName = "boundary_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = bestThreshold,
            ScalingSignalNote = "tuned_threshold (чем стабильнее — тем лучше обобщается)",
            ShouldScaleUp     = val.Count < 50,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"examples={examples.Count}, val={val.Count}, " +
                    $"baseAcc={baselineAcc:P1}, enhAcc={enhancedAcc:P1}, " +
                    $"threshold={bestThreshold:F2}, " +
                    $"FP={falsePositives}(лишние разрывы), FN={falseNegatives}(пропущенные)"
        });
    }

    // Генерация примеров: 1-segment и 2-segment промпты
    private static List<(string Prompt, int ExpectedSegments)> GenerateExamples(int seed)
    {
        var rng     = new Random(seed);
        var singles = new[]
        {
            // CODE_CHANGE
            "Add GetUserById to UserService",
            "Extract validation from ContextWindowManager",
            "Update the auth token handler",
            "Refactor the search filter builder",
            // CODE_QUESTION
            "What does ValidateToken return when user is null",
            "Explain how the AST packing works",
            "Find methods without null check",
            "Show all callers of BuildSearchFilter",
            // BUG_FIX
            "Fix NullReferenceException in UserService",
            "Why does getUserById fail when id is negative",
            "Debug the token expiry logic",
            // TEST
            "Write unit tests for AuthService",
            "Add integration test for the null check flow",
        };

        var falseConj = new[]
        {
            // CONJ внутри одного сегмента — НЕ должны разбиваться
            "Find methods that return null and log them",
            "Check null and empty before accessing Name",
            "Validate token and user before returning result",
            "Read file and write output to same directory",
        };

        var result = new List<(string, int)>();

        // 1-segment примеры
        foreach (var s in singles)
            result.Add((s, 1));

        // Ложные CONJ (всё ещё 1 сегмент)
        foreach (var s in falseConj)
            result.Add((s, 1));

        // 2-segment примеры: соединяем два singles через AND/THEN
        var conjs = new[] { "and", "then", "after that" };
        for (int i = 0; i < singles.Length - 1; i += 2)
        {
            string conj = conjs[rng.Next(conjs.Length)];
            // Берём из разных доменов чтобы профиль менялся
            if (i + 1 < singles.Length && DifferentDomain(singles[i], singles[i + 1]))
                result.Add(($"{singles[i]} {conj} {singles[i + 1]}", 2));
        }

        return result;
    }

    private static bool DifferentDomain(string a, string b)
    {
        // Грубая проверка — разные ключевые слова
        var aWords = a.ToLower().Split(' ');
        var bWords = b.ToLower().Split(' ');
        return !aWords.Any(w => bWords.Contains(w));
    }

    private static List<string> ConjSplit(string prompt)
    {
        // Простой split по союзам — baseline
        var parts = System.Text.RegularExpressions.Regex
            .Split(prompt, @"\band\b|\bthen\b|\bafter that\b",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToList();
        return parts.Count > 0 ? parts : new List<string> { prompt };
    }

    private static double TuneThreshold(List<(string Prompt, int Expected)> train)
    {
        // Перебрать пороги от 0.2 до 0.8 шагом 0.05
        double best = 0.4; int bestScore = 0;
        for (double t = 0.2; t <= 0.8; t += 0.05)
        {
            var sp = new PromptSplitter(t);
            int score = train.Count(ex =>
            {
                var r = sp.Split(ex.Prompt);
                return r.IsSuccess && r.Value!.Count == ex.Expected;
            });
            if (score > bestScore) { bestScore = score; best = t; }
        }
        return best;
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
```

## Добавить в Dispatcher/Runner/ResultWriter

```csharp
// Dispatcher: public OperationResult<ExperimentV2Result> RunE21(int tier = 1)
//     => new E21PromptSplitter().Run(tier);
// Runner: "E21" => _d.RunE21(tier),
// ResultWriter: "E21-PROMPT-SPLITTER" => "E21-PROMPT-SPLITTER",
```

## Ожидаемый вывод

```
OK boundary_accuracy (up) baseline=0.71 enhanced=0.87 delta=+22.5% | PASSED
notes: examples=300, val=60, baseAcc=71.0%, enhAcc=87.0%,
       threshold=0.42, FP=4(лишние), FN=4(пропущенные)
```

---

---

# E22 — DatasetRouter: какие датасеты нужны для этого запроса

## Гипотеза

**Одна цифра определяет всё:** модель с датасетом 100% релевантных примеров
нужно в N× меньше данных чтобы выучить паттерн, чем модель на смешанном датасете.

Пример: если BUG_FIX паттерны составляют 10% общего датасета, модель видит
BUG_FIX пример каждый 10-й шаг. С routing она видит его каждый 1-й шаг — 10× эффективнее.

**Проверяемое утверждение:** K-NN на E17-предикатах предскажет правильный
бинарный вектор (какие датасеты нужны) с F1 > 0.80 на validation.

Baseline: всегда возвращать все датасеты (максимальный recall, нулевая precision).
Enhanced: K-NN на предикатах → ближайший пример → скопировать его бинарный вектор.

## Датасеты (6 штук)

| Индекс | Название | Примеры откуда |
|---|---|---|
| 0 | Generic code patterns | любой C#/TS/Python файл |
| 1 | C#/.NET specific | файлы из llm_mvp_core |
| 2 | TypeScript/NestJS specific | файлы с `interface`/`@Injectable`/`.ts` |
| 3 | BugFix patterns | методы с `throw`/`catch` + NullCheck |
| 4 | Orchestration/Planning | FLOW документы, planning sequences |
| 5 | Test patterns | файлы с `[Test]`/`Assert`/`Fact` |

## Обучающие данные: автоматическая разметка

200 примеров генерируются из существующего кода через E14 ActionLabelGenerator
и структурный анализ — **без ручной разметки**:

```csharp
// Правила для автоматической разметки:
// файл .cs → C# = 1, Generic = 1
// файл с [Fact]/[Test] → Tests = 5, Generic = 0
// метод с throw/NullCheck и без проверки → BugFix = 3
// метод с async/await → может быть NestJS или C#
// файл без namespace → Generic = 0
```

## Шаг 1 — Создать `DatasetRouter.cs`

**Путь:** `35.ExperimentsV2/Infrastructure/DatasetRouter.cs`

```csharp
namespace ExperimentsV2;

// DatasetRouter предсказывает бинарный вектор релевантности датасетов.
// Вектор: [Generic, CSharp, TypeScript, BugFix, Orchestration, Tests]
// 1 = этот датасет полезен для данного запроса.
public sealed class DatasetRouter
{
    // Обучающие примеры: (предикатный профиль) → (бинарный вектор)
    private readonly List<(int[] Predicates, bool[] Datasets)> _train = new();
    private readonly int _k;
    private readonly int _datasetCount;

    public static readonly string[] DatasetNames =
        ["Generic", "CSharp", "TypeScript", "BugFix", "Orchestration", "Tests"];

    public DatasetRouter(int k = 3, int datasetCount = 6)
    {
        _k            = k;
        _datasetCount = datasetCount;
    }

    public void Train(IReadOnlyList<(int[] Predicates, bool[] Datasets)> examples)
    {
        _train.Clear();
        _train.AddRange(examples);
    }

    // Предсказать бинарный вектор для запроса.
    // Для каждого датасета независимо: большинство из K соседей имеет 1 → предсказываем 1.
    public bool[] Predict(int[] queryPredicates)
    {
        if (_train.Count == 0)
            return Enumerable.Repeat(true, _datasetCount).ToArray();

        var neighbours = _train
            .OrderBy(t => PredicateDistance(queryPredicates, t.Predicates))
            .Take(_k)
            .ToList();

        var result = new bool[_datasetCount];
        for (int d = 0; d < _datasetCount; d++)
        {
            int votes = neighbours.Count(n => n.Datasets[d]);
            result[d] = votes > _k / 2;
        }
        return result;
    }

    // Расстояние между двумя предикатными последовательностями:
    // Jaccard на множестве уникальных предикатов
    private static double PredicateDistance(int[] a, int[] b)
    {
        var setA = new HashSet<int>(a);
        var setB = new HashSet<int>(b);
        int inter = setA.Count(x => setB.Contains(x));
        int union = setA.Count + setB.Count - inter;
        return union == 0 ? 0 : 1.0 - (double)inter / union;
    }

    // Автоматически разметить C# файл: какие датасеты он представляет
    public static bool[] AutoLabel(string csharpSource)
    {
        var label = new bool[6];
        var lower = csharpSource.ToLowerInvariant();

        // Generic: всегда 1 если есть хоть какой-то код
        label[0] = csharpSource.Length > 100;

        // CSharp: namespace, class, using
        label[1] = lower.Contains("namespace ") || lower.Contains("using system");

        // TypeScript: interface, Injectable, export
        label[2] = lower.Contains("interface ") && lower.Contains("injectable") ||
                   lower.Contains("export default") || lower.Contains(".ts\n");

        // BugFix: throw + catch, или MemberAccess без NullCheck (E14 логика)
        bool hasThrow = lower.Contains("throw ") || lower.Contains("catch (");
        bool hasNullMiss = ActionLabelGenerator.Label(csharpSource)
                           == ActionLabelGenerator.ADD_NULL_CHECK;
        label[3] = hasThrow || hasNullMiss;

        // Orchestration: flow, phase, plan, orchestrat
        label[4] = lower.Contains("flow") && lower.Contains("phase") ||
                   lower.Contains("orchestrat") || lower.Contains("plan ");

        // Tests: [Fact], [Test], Assert, Should
        label[5] = lower.Contains("[fact]") || lower.Contains("[test]") ||
                   lower.Contains("assert.") || lower.Contains(".should()");

        return label;
    }
}
```

## Шаг 2 — Создать `E22DatasetRouter.cs`

**Путь:** `35.ExperimentsV2/Experiments/E22DatasetRouter.cs`

```csharp
namespace ExperimentsV2;

// E22: DatasetRouter — K-NN предсказывает бинарный вектор датасетов.
// Baseline:  всегда возвращать все датасеты (recall=1, precision=1/6=16.7%)
// Enhanced:  K-NN на E17 предикатах → точный вектор (recall≥0.9, precision→1.0)
//
// Обучение:  200 авто-размеченных пар из llm_mvp_core (без ручной разметки!)
// Валидация: F1 по каждому датасету отдельно
// Гипотеза:  предикатный профиль файла достаточен для маршрутизации
public sealed class E22DatasetRouter
{
    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
    {
        if (files.Count < 4)
            return OperationResult<ExperimentV2Result>.Fail("Нужно ≥ 4 файлов");

        // Построить обучающие пары: предикаты → авто-метки
        var examples = new List<(int[] Predicates, bool[] Datasets)>();
        foreach (var (_, text) in files)
        {
            var predResult = LogicExtractor.Extract(text);
            if (!predResult.IsSuccess || predResult.Value!.Count == 0) continue;
            bool[] label = DatasetRouter.AutoLabel(text);
            // Пропустить если все нули (файл не попал ни в какой датасет)
            if (label.Any(b => b))
                examples.Add((predResult.Value!.ToArray(), label));
        }

        if (examples.Count < 4)
            return OperationResult<ExperimentV2Result>.Fail(
                $"Слишком мало примеров с метками: {examples.Count}");

        int split  = (int)(examples.Count * 0.8);
        var train  = examples.Take(split).ToList();
        var val    = examples.Skip(split).ToList();

        var router = new DatasetRouter(k: 3);
        router.Train(train);

        // ── Baseline: все датасеты (recall=1) ────────────────────────────────
        var t0 = Stopwatch();
        int dsCount = DatasetRouter.DatasetNames.Length;
        var baseAllOnes = Enumerable.Repeat(true, dsCount).ToArray();
        // Precision baseline = сколько реально нужных / все 6
        double baseAvgPrecision = val.Average(ex =>
            ex.Datasets.Count(b => b) / (double)dsCount);
        double baseF1 = HarmonicMean(1.0, baseAvgPrecision);
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: K-NN routing ────────────────────────────────────────────
        var t1 = Stopwatch();
        // F1 по каждому датасету
        var tp = new int[dsCount]; var fp = new int[dsCount];
        var fn = new int[dsCount]; var tn = new int[dsCount];

        foreach (var (preds, actual) in val)
        {
            bool[] predicted = router.Predict(preds);
            for (int d = 0; d < dsCount; d++)
            {
                if (predicted[d] && actual[d])  tp[d]++;
                if (predicted[d] && !actual[d]) fp[d]++;
                if (!predicted[d] && actual[d]) fn[d]++;
                if (!predicted[d] && !actual[d])tn[d]++;
            }
        }

        var f1PerDataset = Enumerable.Range(0, dsCount)
            .Select(d =>
            {
                double prec = tp[d] + fp[d] > 0 ? tp[d] / (double)(tp[d] + fp[d]) : 0;
                double rec  = tp[d] + fn[d] > 0 ? tp[d] / (double)(tp[d] + fn[d]) : 0;
                return HarmonicMean(prec, rec);
            })
            .ToArray();

        double macroF1 = f1PerDataset.Average();
        double enhancedMs = ElapsedMs(t1);

        string f1Notes = string.Join(", ", Enumerable.Range(0, dsCount)
            .Select(d => $"{DatasetRouter.DatasetNames[d]}={f1PerDataset[d]:F2}"));

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E22-DATASET-ROUTER",
            Tier              = tier,
            BaselinePrimary   = baseF1,
            EnhancedPrimary   = macroF1,
            PrimaryMetricName = "macro_f1",
            LowerIsBetter     = false,
            ScalingSignal     = val.Count(ex =>
                ex.Datasets.Count(b => b) > 1) / (double)val.Count,
            ScalingSignalNote = "multi_dataset_ratio (доля запросов нужных >1 датасет)",
            ShouldScaleUp     = val.Count < 20,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"examples={examples.Count}, train={train.Count}, val={val.Count}, " +
                    $"baseF1={baseF1:F2}, macroF1={macroF1:F2}, " +
                    $"per_dataset=[{f1Notes}]"
        });
    }

    private static double HarmonicMean(double a, double b)
        => a + b < 1e-9 ? 0 : 2 * a * b / (a + b);

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
```

## Ожидаемый вывод

```
OK macro_f1 (up) baseline=0.28 enhanced=0.74 delta=+164.3% | PASSED
notes: examples=47, train=37, val=10,
       baseF1=0.28, macroF1=0.74,
       per_dataset=[Generic=0.91, CSharp=0.83, TypeScript=0.60,
                    BugFix=0.71, Orchestration=0.55, Tests=0.80]
```

---

---

# E23 — Beautifier: предикаты → код для конкретного стека

## Гипотеза

После того как модель генерирует `NULL_GUARD → VALIDATE_OP → SUCCESS`:
- На C#:    `if (user == null) return OperationResult<T>.Fail("...");\n var result = Validate(user);`
- На NestJS: `if (!user) throw new NotFoundException();\n await this.validate(user);`
- На Python: `if user is None: raise ValueError(...)\n result = validate(user)`

**Проверяемое утверждение:** K-NN в пространстве предикатов находит правильный
шаблон кода для данной предикатной последовательности + стека с
syntaxCorrectPct ≥ 95% (как E05 который дал 99.5%).

Baseline: случайный шаблон из библиотеки стека.
Enhanced: K-NN (предикаты + стек) → ближайший шаблон.

## Обучающие данные: 50 шаблонов на стек

Шаблоны аннотируются вручную, но их мало (~50 на стек × 3 стека = 150).
Для каждого шаблона: predicate_sequence + target_stack + code_template.

## Шаг 1 — Создать `BeautifierTemplateLibrary.cs`

**Путь:** `35.ExperimentsV2/Infrastructure/BeautifierTemplateLibrary.cs`

```csharp
namespace ExperimentsV2;

// BeautifierTemplateLibrary хранит шаблоны кода с аннотациями предикатов.
// Для каждого шаблона: какие предикаты он реализует + для какого стека.
public static class BeautifierTemplateLibrary
{
    public const string CSHARP     = "CSharp";
    public const string TYPESCRIPT = "TypeScript";
    public const string GENERIC    = "Generic";

    // Один шаблон: предикатная последовательность + код
    public record Template(
        int[]  Predicates,  // из LogicExtractor (NULL_GUARD=1, FAIL=2, ...)
        string Stack,
        string Code,        // шаблон с {VAR}, {TYPE} плейсхолдерами
        string Description);

    // Библиотека шаблонов — расширять по мере необходимости
    public static readonly IReadOnlyList<Template> All = new[]
    {
        // ── C# шаблоны ──────────────────────────────────────────────────────
        new Template(
            new[] { LogicExtractor.NULL_GUARD, LogicExtractor.FAIL },
            CSHARP,
            "if ({VAR} == null)\n    return OperationResult<{TYPE}>.Fail(\"{VAR} cannot be null\");",
            "Null guard → Fail"),

        new Template(
            new[] { LogicExtractor.NULL_GUARD, LogicExtractor.FAIL, LogicExtractor.SUCCESS },
            CSHARP,
            "if ({VAR} == null)\n    return OperationResult<{TYPE}>.Fail(\"...\");\n// ...\nreturn OperationResult<{TYPE}>.Ok(result);",
            "Null guard → Fail | Success"),

        new Template(
            new[] { LogicExtractor.VALIDATE_OP, LogicExtractor.FAIL },
            CSHARP,
            "var validResult = _validator.Validate({VAR});\nif (!validResult.IsSuccess)\n    return OperationResult<{TYPE}>.Fail(validResult.ErrorMessage);",
            "Validate → Fail"),

        new Template(
            new[] { LogicExtractor.ITERATE, LogicExtractor.GET_CALL },
            CSHARP,
            "foreach (var item in {COLLECTION})\n{\n    var result = Get{ITEM}(item.Id);\n    // process result\n}",
            "Iterate → Get"),

        new Template(
            new[] { LogicExtractor.THROW_OP },
            CSHARP,
            "throw new InvalidOperationException(\"{MESSAGE}\");",
            "Throw exception"),

        // ── TypeScript шаблоны ───────────────────────────────────────────────
        new Template(
            new[] { LogicExtractor.NULL_GUARD, LogicExtractor.FAIL },
            TYPESCRIPT,
            "if (!{VAR}) {\n  throw new NotFoundException(`{VAR} not found`);\n}",
            "NestJS null guard → NotFoundException"),

        new Template(
            new[] { LogicExtractor.VALIDATE_OP, LogicExtractor.FAIL },
            TYPESCRIPT,
            "const errors = await this.validator.validate({VAR});\nif (errors.length > 0) {\n  throw new BadRequestException(errors);\n}",
            "NestJS validation → BadRequest"),

        new Template(
            new[] { LogicExtractor.ITERATE, LogicExtractor.GET_CALL },
            TYPESCRIPT,
            "const results = await Promise.all(\n  {COLLECTION}.map(item => this.get{ITEM}(item.id))\n);",
            "NestJS iterate → Promise.all"),

        // ── Generic шаблоны ─────────────────────────────────────────────────
        new Template(
            new[] { LogicExtractor.NULL_GUARD, LogicExtractor.FAIL },
            GENERIC,
            "if ({VAR} is None:\n    raise ValueError(f\"{VAR} cannot be None\")",
            "Python null guard"),

        new Template(
            new[] { LogicExtractor.ITERATE, LogicExtractor.TRANSFORM },
            GENERIC,
            "{RESULT} = [{TRANSFORM}(item) for item in {COLLECTION}]",
            "Python list comprehension"),
    }.AsReadOnly();

    // Получить шаблоны для конкретного стека
    public static IReadOnlyList<Template> ForStack(string stack)
        => All.Where(t => t.Stack == stack || t.Stack == GENERIC).ToList().AsReadOnly();
}
```

## Шаг 2 — Создать `E23Beautifier.cs`

**Путь:** `35.ExperimentsV2/Experiments/E23Beautifier.cs`

```csharp
using Microsoft.CodeAnalysis.CSharp;

namespace ExperimentsV2;

// E23: Beautifier — K-NN предикаты+стек → шаблон кода.
// Baseline:  случайный шаблон из библиотеки стека
// Enhanced:  K-NN на предикатах → ближайший шаблон → syntaxCorrectPct
//
// Метрика:   syntaxCorrectPct (Roslyn парсит сгенерированный C# без ошибок)
//            + template_match_rate (выбран правильный шаблон для профиля)
// Гипотеза:  K-NN в пространстве 12 предикатов достаточен для выбора шаблона
public sealed class E23Beautifier
{
    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
    {
        // Тестовые запросы: (предикатная последовательность, ожидаемый шаблон, стек)
        var testCases = BuildTestCases(files);
        if (testCases.Count < 3)
            return OperationResult<ExperimentV2Result>.Fail("Нужно ≥ 3 тестовых случаев");

        var templates = BeautifierTemplateLibrary.All.ToList();
        var rng       = new Random(42);

        // ── Baseline: случайный шаблон для стека ─────────────────────────────
        var t0 = Stopwatch();
        int baseSyntaxOk = 0;
        foreach (var tc in testCases)
        {
            var stackTemplates = BeautifierTemplateLibrary.ForStack(tc.Stack);
            if (stackTemplates.Count == 0) continue;
            var randTemplate = stackTemplates[rng.Next(stackTemplates.Count)];
            if (SyntaxCorrect(randTemplate.Code, tc.Stack)) baseSyntaxOk++;
        }
        double baselineAcc = baseSyntaxOk / (double)testCases.Count;
        double baselineMs  = ElapsedMs(t0);

        // ── Enhanced: K-NN на предикатах ─────────────────────────────────────
        var t1 = Stopwatch();
        int enhSyntaxOk = 0; int enhMatchOk = 0;
        foreach (var tc in testCases)
        {
            var stackTemplates = BeautifierTemplateLibrary.ForStack(tc.Stack).ToList();
            if (stackTemplates.Count == 0) continue;

            // Найти ближайший шаблон по Jaccard на предикатах
            var best = stackTemplates
                .OrderBy(t => JaccardDist(tc.Predicates, t.Predicates))
                .First();

            if (SyntaxCorrect(best.Code, tc.Stack)) enhSyntaxOk++;
            if (best.Description == tc.ExpectedDescription) enhMatchOk++;
        }
        double enhancedAcc  = enhSyntaxOk  / (double)testCases.Count;
        double matchRate     = enhMatchOk   / (double)testCases.Count;
        double enhancedMs    = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E23-BEAUTIFIER",
            Tier              = tier,
            BaselinePrimary   = baselineAcc,
            EnhancedPrimary   = enhancedAcc,
            PrimaryMetricName = "syntax_correct_pct",
            LowerIsBetter     = false,
            ScalingSignal     = matchRate,
            ScalingSignalNote = "template_match_rate (правильный шаблон выбран)",
            ShouldScaleUp     = testCases.Count < 10,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"testCases={testCases.Count}, " +
                    $"templates={BeautifierTemplateLibrary.All.Count}, " +
                    $"baseSyntax={baselineAcc:P1}, enhSyntax={enhancedAcc:P1}, " +
                    $"matchRate={matchRate:P1}"
        });
    }

    private static List<(int[] Predicates, string Stack, string ExpectedDescription)>
        BuildTestCases(IReadOnlyList<(string Path, string Text)> files)
    {
        var cases = new List<(int[], string, string)>();
        foreach (var (_, text) in files.Take(20))
        {
            var predResult = LogicExtractor.Extract(text);
            if (!predResult.IsSuccess) continue;
            var preds = predResult.Value!.ToArray();

            // Определить ожидаемый шаблон по предикатному профилю
            bool hasNullGuard  = preds.Contains(LogicExtractor.NULL_GUARD);
            bool hasFail       = preds.Contains(LogicExtractor.FAIL);
            bool hasValidate   = preds.Contains(LogicExtractor.VALIDATE_OP);

            string? expected = (hasNullGuard && hasFail) ? "Null guard → Fail"
                              : (hasValidate && hasFail) ? "Validate → Fail"
                              : null;
            if (expected == null) continue;

            cases.Add((preds, BeautifierTemplateLibrary.CSHARP, expected));
        }
        return cases;
    }

    private static bool SyntaxCorrect(string code, string stack)
    {
        if (stack != BeautifierTemplateLibrary.CSHARP) return true; // нет Roslyn для TS/Python
        // Обернуть в класс чтобы Roslyn мог парсить
        string wrapped = $"class T {{ void M() {{ {code.Replace("{VAR}","x").Replace("{TYPE}","object")} }} }}";
        var tree   = CSharpSyntaxTree.ParseText(wrapped);
        var errors = tree.GetDiagnostics()
            .Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .ToList();
        return errors.Count == 0;
    }

    private static double JaccardDist(int[] a, int[] b)
    {
        var setA = new HashSet<int>(a);
        var setB = new HashSet<int>(b);
        int inter = setA.Count(x => setB.Contains(x));
        int union = setA.Count + setB.Count - inter;
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
OK syntax_correct_pct (up) baseline=0.60 enhanced=0.95 delta=+58.3% | PASSED
notes: testCases=15, templates=10, baseSyntax=60%, enhSyntax=95%, matchRate=73%
```

---

## Сводная таблица E21–E23

| Эксперимент | Гипотеза | Baseline | Enhanced | Что считаем |
|---|---|---|---|---|
| E21 PromptSplitter | Профиль предикатов точнее CONJ | CONJ split (71%) | Predicate profile shift | boundary_accuracy |
| E22 DatasetRouter | K-NN на предикатах → бинарный вектор | Все датасеты (F1=0.28) | K-NN routing | macro_F1 per dataset |
| E23 Beautifier | K-NN предикаты → правильный шаблон | Random template (60%) | K-NN → template | syntaxCorrectPct |

---

## Полный пайплайн после E21–E23

```
"Add auth to UserService AND write tests"
         ↓ E21 PromptSplitter
   Сегмент 1: "Add auth to UserService"    → [AUTH_OP, CREATE_OP]
   Сегмент 2: "write tests"                → [CREATE_OBJ, VALIDATE_OP]
         ↓ E22 DatasetRouter (для каждого сегмента)
   Сегмент 1 → [Generic=1, CSharp=1, NestJS=0, BugFix=0, Orch=0, Tests=0]
   Сегмент 2 → [Generic=1, CSharp=1, NestJS=0, BugFix=0, Orch=0, Tests=1]
         ↓ Retrieve из выбранных датасетов + модель на предикатах
   → NULL_GUARD → VALIDATE_OP → SUCCESS | FAIL
         ↓ E23 Beautifier (стек = C# из E20)
   → if (user == null) return OperationResult<T>.Fail(...)
     var result = _validator.Validate(user);
     return OperationResult<T>.Ok(result);
```

---

*E21/E22/E23 план v1.0 — маршрутизация и Beautifier · не касается XIIGen*
