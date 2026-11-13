# E24 — AttributedRouter: сегмент промпта → конкретный датасет
## Подробный план для Claude Code v1.0

**Проект:** `35.ExperimentsV2`
**Зависимости:** SemanticNormalizer (E15), FamilyCollapser (E16),
                 LogicExtractor (E17), PromptSplitter (E21), DatasetRouter (E22)
**Этот документ не касается XIIGen.**

---

## Гипотеза

E22 выдаёт: `bool[6]` — весь промпт идёт во все релевантные датасеты.

E24 выдаёт: `{датасет → конкретный кусок промпта}` — каждый датасет получает
только ту часть промпта которая к нему относится.

**Проверяемое утверждение:** модель которая получает целевой подпромпт вместо
полного промпта делает более точный retrieval. Это измеряется через
`segment_precision` — доля правильно атрибутированных сегментов.

**Пример:**
```
Промпт: "Add null check to ValidateToken and write tests for edge cases"

E22 выдаёт:
  CSharp=1, BugFix=1, Tests=1, Generic=1
  → Каждый датасет получает полный промпт (77 символов)

E24 выдаёт:
  CSharp  → "Add null check to ValidateToken"   (33 символа)
  BugFix  → "Add null check to ValidateToken"   (33 символа)
  Tests   → "write tests for edge cases"         (27 символа)
  Generic → оба сегмента                         (60 символов)
  → Orchestration, TypeScript отсутствуют — не задействованы
```

---

## Архитектура: два шага совместно

```
                        Полный промпт
                              │
              ┌───────────────▼───────────────┐
              │  E21 PromptSplitter            │
              │  (предикатный профиль)         │
              └───┬───────────────┬────────────┘
                  │               │
            Сегмент 1       Сегмент 2
          [NULL_GUARD,     [CREATE_OBJ,
           VALIDATE_OP,     VALIDATE_OP,
           FAIL]            SUCCESS]
                  │               │
       ┌──────────▼───┐  ┌────────▼──────────┐
       │  E22 K-NN    │  │  E22 K-NN         │
       │  датасеты 1  │  │  датасеты 2       │
       └──────────┬───┘  └────────┬──────────┘
        CSharp=1      Tests=1, Generic=1
        BugFix=1      Generic=1
        Generic=1
                  │               │
              ┌───▼───────────────▼────────────┐
              │  Attribution merge             │
              │  CSharp  → Сегмент 1           │
              │  BugFix  → Сегмент 1           │
              │  Tests   → Сегмент 2           │
              │  Generic → Сег1 + "\n" + Сег2  │
              └───────────────────────────────┘
```

---

## Шаг 1 — Создать `AttributedRoutingResult.cs`

**Путь:** `35.ExperimentsV2/Infrastructure/AttributedRoutingResult.cs`

```csharp
namespace ExperimentsV2;

// Результат атрибутированной маршрутизации.
// Каждый релевантный датасет получает строго свой кусок промпта.
public sealed record AttributedRoutingResult
{
    // Словарь: название датасета → текст подпромпта для этого датасета.
    // Отсутствие ключа = датасет не задействован совсем.
    public required IReadOnlyDictionary<string, string> DatasetToSubprompt { get; init; }

    // Общее количество задействованных датасетов
    public int ActiveDatasets => DatasetToSubprompt.Count;

    // Суммарная длина всех подпромптов (для сравнения с исходным)
    public int TotalSubpromptLength => DatasetToSubprompt.Values.Sum(v => v.Length);

    // Удобный доступ: получить подпромпт для датасета или null
    public string? ForDataset(string datasetName)
        => DatasetToSubprompt.TryGetValue(datasetName, out var v) ? v : null;
}
```

---

## Шаг 2 — Создать `AttributedRouter.cs`

**Путь:** `35.ExperimentsV2/Infrastructure/AttributedRouter.cs`

```csharp
namespace ExperimentsV2;

// AttributedRouter объединяет E21 (сплиттер) и E22 (роутер) в один компонент.
// Вход: полный промпт пользователя.
// Выход: для каждого активного датасета — только его кусок промпта.
public sealed class AttributedRouter
{
    private readonly PromptSplitter _splitter;
    private readonly DatasetRouter  _router;

    public AttributedRouter(DatasetRouter trainedRouter,
        double splitThreshold = 0.4, int windowSize = 3)
    {
        _splitter = new PromptSplitter(splitThreshold, windowSize);
        _router   = trainedRouter;
    }

    // Основной метод: промпт → атрибутированный результат
    public OperationResult<AttributedRoutingResult> Route(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            return OperationResult<AttributedRoutingResult>.Fail("Пустой промпт");

        // Шаг 1: разбить на сегменты
        var splitResult = _splitter.Split(prompt);
        if (!splitResult.IsSuccess)
            return OperationResult<AttributedRoutingResult>.Fail(
                "Сплиттер: " + splitResult.ErrorMessage);

        var segments = splitResult.Value!;

        // Шаг 2: для каждого сегмента определить датасеты
        var attribution = new Dictionary<string, List<string>>();

        foreach (var segment in segments)
        {
            // Извлечь предикаты сегмента (текстовый промпт → семьи → предикаты)
            int[] preds = TextToPredicates(segment.Text);

            // Определить релевантные датасеты для этого сегмента
            bool[] datasetVector = _router.Predict(preds);

            // Приписать сегмент к каждому активному датасету
            for (int d = 0; d < DatasetRouter.DatasetNames.Length; d++)
            {
                if (!datasetVector[d]) continue;
                string key = DatasetRouter.DatasetNames[d];
                if (!attribution.ContainsKey(key))
                    attribution[key] = new List<string>();
                attribution[key].Add(segment.Text);
            }
        }

        // Если ничего не нашли — fallback: Generic получает весь промпт
        if (attribution.Count == 0)
            attribution["Generic"] = new List<string> { prompt };

        // Объединить сегменты одного датасета через перенос строки
        var result = attribution.ToDictionary(
            kv => kv.Key,
            kv => string.Join("\n", kv.Value));

        return OperationResult<AttributedRoutingResult>.Ok(new AttributedRoutingResult
        {
            DatasetToSubprompt = result.AsReadOnly()
        });
    }

    // Конвертировать текстовый сегмент в предикаты через SemanticNormalizer + FamilyCollapser
    private static int[] TextToPredicates(string text)
    {
        var words    = SemanticNormalizer.Normalize(text);
        var families = FamilyCollapser.Collapse(words);

        // Маппинг семей → предикаты LogicExtractor
        var preds = new List<int>();
        foreach (var fam in families)
        {
            int pred = fam switch
            {
                "VALIDATE_OP" => LogicExtractor.VALIDATE_OP,
                "GET_OP"      => LogicExtractor.GET_CALL,
                "SET_OP"      => LogicExtractor.ASSIGN,
                "CREATE_OP"   => LogicExtractor.CREATE_OBJ,
                "DELETE_OP"   => LogicExtractor.FAIL,
                "AUTH_OP"     => LogicExtractor.VALIDATE_OP,
                "ERROR"       => LogicExtractor.FAIL,
                "SUCCESS"     => LogicExtractor.SUCCESS,
                "NULL"        => LogicExtractor.NULL_GUARD,
                _             => LogicExtractor.UNKNOWN
            };
            if (pred != LogicExtractor.UNKNOWN)
                preds.Add(pred);
        }

        // Fallback: если слов не нашлось — UNKNOWN как сигнал Generic
        return preds.Count > 0 ? preds.ToArray() : new[] { LogicExtractor.UNKNOWN };
    }
}
```

---

## Шаг 3 — Создать `E24AttributedRouter.cs`

**Путь:** `35.ExperimentsV2/Experiments/E24AttributedRouter.cs`

```csharp
namespace ExperimentsV2;

// E24: AttributedRouter — каждый датасет получает только свой кусок промпта.
//
// Baseline:  полный промпт → все активные датасеты (E22 поведение)
//            Метрика: среднее покрытие нерелевантного текста (шум на датасет)
//
// Enhanced:  сегмент → только его датасет
//            Метрика: то же, должно быть значительно меньше
//
// Гипотеза:  атрибуция сокращает нерелевантный текст для каждого датасета
//            в среднем на 40-60% при двух сегментах в промпте
//
// Обучение:  те же 200 авто-размеченных пар из E22 (без дополнительной разметки)
// Валидация: segment_precision — доля правильно атрибутированных сегментов
// Сравнение: baseline (полный промпт) vs enhanced (точные подпромпты)
public sealed class E24AttributedRouter
{
    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
    {
        if (files.Count < 4)
            return OperationResult<ExperimentV2Result>.Fail("Нужно ≥ 4 файлов");

        // ── Подготовить обученный DatasetRouter (из E22) ──────────────────────
        var examples = new List<(int[] Predicates, bool[] Datasets)>();
        foreach (var (_, text) in files)
        {
            var predResult = LogicExtractor.Extract(text);
            if (!predResult.IsSuccess || predResult.Value!.Count == 0) continue;
            bool[] label = DatasetRouter.AutoLabel(text);
            if (label.Any(b => b))
                examples.Add((predResult.Value!.ToArray(), label));
        }

        if (examples.Count < 4)
            return OperationResult<ExperimentV2Result>.Fail(
                $"Слишком мало размеченных примеров: {examples.Count}");

        int split = (int)(examples.Count * 0.8);
        var router = new DatasetRouter(k: 3);
        router.Train(examples.Take(split).ToList());

        var attributedRouter = new AttributedRouter(router);

        // ── Тестовые промпты с известными сегментами ─────────────────────────
        var testCases = BuildTestCases();

        // ── Baseline: полный промпт → все датасеты ───────────────────────────
        var t0 = Stopwatch();
        double baseAvgNoise = 0;    // среднее % нерелевантного текста на датасет
        int    baseActive   = 0;

        foreach (var tc in testCases)
        {
            // Baseline: весь промпт идёт во все датасеты (найденные E22)
            int totalLen = tc.FullPrompt.Length;
            foreach (var (dataset, relevantSegment) in tc.GroundTruth)
            {
                int relevantLen = relevantSegment.Length;
                double noise    = 1.0 - (double)relevantLen / totalLen;
                baseAvgNoise += noise;
                baseActive++;
            }
        }
        double baselineNoise = baseActive > 0 ? baseAvgNoise / baseActive : 1.0;
        double baselineMs    = ElapsedMs(t0);

        // ── Enhanced: атрибутированный роутер ────────────────────────────────
        var t1 = Stopwatch();
        double enhAvgNoise    = 0;
        int    enhActive      = 0;
        int    correctAttrib  = 0;   // правильно атрибутированных пар
        int    totalAttrib    = 0;   // всего ожидаемых пар

        foreach (var tc in testCases)
        {
            var routeResult = attributedRouter.Route(tc.FullPrompt);
            if (!routeResult.IsSuccess) continue;
            var routing = routeResult.Value!;

            foreach (var (expectedDataset, expectedSegment) in tc.GroundTruth)
            {
                totalAttrib++;
                string? actualSubprompt = routing.ForDataset(expectedDataset);

                if (actualSubprompt == null)
                {
                    // Датасет не нашли совсем — считаем как 100% шум
                    enhAvgNoise += 1.0;
                    enhActive++;
                    continue;
                }

                // Сколько нерелевантного текста в подпромпте?
                double noise = 1.0 - (double)expectedSegment.Length / actualSubprompt.Length;
                enhAvgNoise += Math.Max(0, noise);
                enhActive++;

                // Правильно атрибутирован если подпромпт содержит ожидаемый сегмент
                if (actualSubprompt.Contains(
                    expectedSegment.Split(' ').First(),
                    StringComparison.OrdinalIgnoreCase))
                    correctAttrib++;
            }
        }

        double enhancedNoise  = enhActive > 0 ? enhAvgNoise / enhActive : 1.0;
        double segPrecision   = totalAttrib > 0 ? correctAttrib / (double)totalAttrib : 0;
        double enhancedMs     = ElapsedMs(t1);

        // Reduction: насколько меньше шума в enhanced vs baseline
        double noiseReduction = baselineNoise > 0
            ? (baselineNoise - enhancedNoise) / baselineNoise : 0;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E24-ATTRIBUTED-ROUTER",
            Tier              = tier,
            // Baseline: средний % нерелевантного текста (ниже = лучше)
            BaselinePrimary   = baselineNoise,
            // Enhanced: тот же показатель после атрибуции
            EnhancedPrimary   = enhancedNoise,
            PrimaryMetricName = "avg_noise_ratio",
            LowerIsBetter     = true,
            ScalingSignal     = segPrecision,
            ScalingSignalNote = "segment_precision (доля правильно атрибутированных сегментов)",
            ShouldScaleUp     = testCases.Count < 10,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"testCases={testCases.Count}, " +
                    $"routerExamples={examples.Count}, " +
                    $"baseNoise={baselineNoise:P1}, enhNoise={enhancedNoise:P1}, " +
                    $"noiseReduction={noiseReduction:P1}, " +
                    $"segPrecision={segPrecision:P1}({correctAttrib}/{totalAttrib})"
        });
    }

    // Тестовые случаи с известной правильной атрибуцией
    private static List<TestCase> BuildTestCases() => new()
    {
        // Двухсегментный: CODE_CHANGE + TEST
        new(
            "Add null check to ValidateToken and write unit tests for edge cases",
            new()
            {
                ["CSharp"]  = "Add null check to ValidateToken",
                ["BugFix"]  = "Add null check to ValidateToken",
                ["Tests"]   = "write unit tests for edge cases",
                ["Generic"] = "Add null check to ValidateToken"
            }),

        // Двухсегментный: CODE_CHANGE + ORCHESTRATION
        new(
            "Refactor the auth service then create the flow plan for phase A",
            new()
            {
                ["CSharp"]         = "Refactor the auth service",
                ["Orchestration"]  = "create the flow plan for phase A",
                ["Generic"]        = "Refactor the auth service"
            }),

        // Однодоменный: BUG_FIX
        new(
            "Fix NullReferenceException in UserService when user is not found",
            new()
            {
                ["BugFix"]  = "Fix NullReferenceException in UserService when user is not found",
                ["CSharp"]  = "Fix NullReferenceException in UserService when user is not found",
                ["Generic"] = "Fix NullReferenceException in UserService when user is not found"
            }),

        // Ложный CONJ — должен остаться одним сегментом
        new(
            "Find methods that return null and log them to the output",
            new()
            {
                ["CSharp"]  = "Find methods that return null and log them to the output",
                ["Generic"] = "Find methods that return null and log them to the output"
            }),

        // Трёхдоменный
        new(
            "Add auth check to UserService and write tests then update the flow plan",
            new()
            {
                ["CSharp"]        = "Add auth check to UserService",
                ["Tests"]         = "write tests",
                ["Orchestration"] = "update the flow plan",
                ["Generic"]       = "Add auth check to UserService"
            }),
    };

    private sealed record TestCase(
        string FullPrompt,
        Dictionary<string, string> GroundTruth);

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
```

---

## Шаг 4 — Добавить в Dispatcher, Runner, ResultWriter

### В `ExperimentsV2Dispatcher.cs`:

```csharp
public OperationResult<ExperimentV2Result> RunE24(
    IReadOnlyList<(string Path, string Text)> files, int tier = 1)
    => new E24AttributedRouter().Run(files, tier);
```

### В `ExperimentRunner.cs`:

**В массив `ids` добавить `"E24"`:**
```csharp
// После "E23":
string[] ids = [..., "E21", "E22", "E23", "E24"];
```

**В switch добавить:**
```csharp
"E24" => DispatchE24(ds, tier),
```

**Новый приватный метод:**
```csharp
private OperationResult<ExperimentV2Result> DispatchE24(DatasetContents ds, int tier)
    => _d.RunE24(ds.Files, tier);
```

### В `ResultWriter.FolderName()`:
```csharp
"E24-ATTRIBUTED-ROUTER" => "E24-ATTRIBUTED-ROUTER",
```

---

## Шаг 5 — Сборка и запуск

**Сборка:**
```
dotnet build 35.ExperimentsV2/35.ExperimentsV2.csproj
```
Ожидаем: `Build succeeded. 0 Error(s)`

**Запуск:**
```csharp
runner.Run("E24", tier: 1);
```

---

## Ожидаемый вывод

```
[E24] Tier 1 загрузка... 3 файлов
[E24] Запуск...
OK avg_noise_ratio (down) baseline=0.580 enhanced=0.190 delta=+67.2% | PASSED
  -> .../E24-ATTRIBUTED-ROUTER/results/run-YYYY-MM-DD-001.json
```

**Ожидаемые числа в notes:**
```
testCases=5, routerExamples=47,
baseNoise=58.0%, enhNoise=19.0%,
noiseReduction=67.2%, segPrecision=80.0%(4/5)
```

---

## Интерпретация чисел

| Метрика | Что означает | Ожидание |
|---|---|---|
| `avg_noise_ratio` baseline | 58% текста в каждом датасете — не его задача | Для 2 сегментов: ~50% ожидаемо |
| `avg_noise_ratio` enhanced | Сколько нерелевантного осталось после атрибуции | < 20% — хорошо |
| `noiseReduction` | Насколько меньше шума | > 50% — эксперимент успешен |
| `segment_precision` | Правильно ли определили куда идёт сегмент | > 75% — норм, > 90% — отлично |

**Если `segment_precision < 60%`:** E21 сплиттер режет неправильно ИЛИ
E22 роутер присваивает неверные датасеты. Запустить E21 и E22 отдельно
и проверить их точность перед E24.

**Если `noiseReduction < 30%`:** промпты в тестовых случаях слишком короткие —
нет смысла делить 5-словный промпт на сегменты. Добавить более длинные примеры.

---

## Связь с другими экспериментами

```
E21 PromptSplitter ─┐
                    ├─→ E24 AttributedRouter  ──→ E22 DatasetRouter (обученный)
E22 DatasetRouter ──┘
(обучает K-NN)

E24 объединяет E21 + E22 в один измеримый результат.
Если E24 проходит — пайплайн готов к подключению к E23 (Beautifier).
```

---

*E24 план v1.0 — атрибутированная маршрутизация · не касается XIIGen*
