# E40 · E41 · E42 · E43 · E44 · E45 — Автономное обучение
## Подробный план для Claude Code v1.0

**Проект:** `35.ExperimentsV2`
**Зависимости:** FamilyCollapser (E16), LogicExtractor (E17), DomainClassifier (E18)
**Этот документ не касается XIIGen.**

---

## Шесть компонентов автономной системы

| Компонент | Эксперимент | Утверждение |
|---|---|---|
| TrustScorer | E40 | Источник данных можно оценить по структурным признакам без чтения содержимого |
| IdleDetector | E41 | Три условия вместе (CPU + ввод + батарея + диск) точнее одного CPU-порога |
| DiskManager | E42 | Value-based вытеснение сохраняет больше знаний чем FIFO при том же лимите диска |
| ReplayBuffer | E43 | 70/30 смешивание предотвращает катастрофическое забывание |
| DatasetWeighter | E44 | Вес × 10.0 на security примерах улучшает security-точность без потери общей |
| CurriculumOrganizer | E45 | Порядок обучения (простое → сложное, security последним) ускоряет convergence |

---

---

# E40 — TrustScorer: оценить надёжность источника данных

## Гипотеза

Источник данных можно оценить по метаданным — без чтения содержимого.
K-NN на признаках источника предсказывает trust-уровень точнее
чем простое правило "внутреннее = HIGH, всё остальное = MEDIUM".

**Три уровня trust:**
```
HIGH  (0.7–1.0) — внутренний Git, официальная документация, верифицированные коллеги
MEDIUM (0.3–0.7) — публичный GitHub (проверенные репо), Stack Overflow, docs.microsoft.com
LOW   (0.0–0.3)  — случайные блоги, неизвестные авторы, устаревший контент
```

**Признаки источника (без чтения содержимого):**
```
is_internal        — внутренний репозиторий организации
has_ci_passing     — есть зелёная CI/CD
has_tests          — содержит тест-файлы
update_days_ago    — сколько дней с последнего обновления
stars_or_forks     — популярность (для публичных)
is_official_docs   — официальная документация продукта
domain_match       — совпадает со стеком проекта (C#/.NET, TypeScript, Python)
```

## Шаг 1 — Создать `E40TrustScorer.cs`

**Путь:** `35.ExperimentsV2/Experiments/E40TrustScorer.cs`

```csharp
namespace ExperimentsV2;

// E40: TrustScorer — оценить надёжность источника данных по метаданным.
// Baseline:  простое правило: is_internal ? HIGH : MEDIUM
// Enhanced:  K-NN на векторе признаков источника
// Обучение:  50 синтетических источников с известным trust-уровнем
// Метрика:   trust_accuracy (3 класса: HIGH/MEDIUM/LOW)
public sealed class E40TrustScorer
{
    public const int LOW    = 0;
    public const int MEDIUM = 1;
    public const int HIGH   = 2;

    public static readonly string[] TrustNames = ["LOW", "MEDIUM", "HIGH"];

    public sealed record SourceFeatures(
        bool IsInternal,       // внутренний репозиторий
        bool HasCIPassing,     // зелёная CI/CD
        bool HasTests,         // есть тест-файлы
        int  UpdateDaysAgo,    // дней с последнего обновления (0=сегодня)
        int  PopularityScore,  // 0-3 (0=неизвестный, 3=1000+ stars)
        bool IsOfficialDocs,   // официальная документация
        bool DomainMatch);     // совпадает со стеком проекта

    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var examples = GenerateExamples(seed: 42, n: tier * 50);
        var rng      = new Random(42);
        var shuffled = examples.OrderBy(_ => rng.Next()).ToList();
        int split    = (int)(shuffled.Count * 0.8);
        var train    = shuffled.Take(split).ToList();
        var val      = shuffled.Skip(split).ToList();

        // ── Baseline: простое правило ─────────────────────────────────────────
        var t0 = Stopwatch();
        int baseOk = val.Count(x =>
        {
            int pred = x.Features.IsInternal ? HIGH
                     : x.Features.IsOfficialDocs ? HIGH
                     : MEDIUM;
            return pred == x.TrustLevel;
        });
        double baseAcc = baseOk / (double)val.Count;
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: K-NN на векторе признаков ──────────────────────────────
        var t1 = Stopwatch();
        var trainVecs = train.Select(x => (Vec: ToVector(x.Features), Trust: x.TrustLevel)).ToList();

        int enhOk = val.Count(x =>
        {
            float[] q = ToVector(x.Features);
            int predicted = trainVecs
                .OrderBy(t => EuclideanDist(q, t.Vec))
                .Take(5)
                .GroupBy(t => t.Trust)
                .OrderByDescending(g => g.Count())
                .First().Key;
            return predicted == x.TrustLevel;
        });
        double enhAcc = enhOk / (double)val.Count;
        double enhancedMs = ElapsedMs(t1);

        // По уровням trust
        string perLevel = string.Join(", ", Enumerable.Range(0, 3).Select(lvl =>
        {
            var lvlVal = val.Where(x => x.TrustLevel == lvl).ToList();
            if (lvlVal.Count == 0) return $"{TrustNames[lvl]}=N/A";
            var vecs = train.Select(x => (Vec: ToVector(x.Features), Trust: x.TrustLevel)).ToList();
            int ok = lvlVal.Count(x =>
            {
                float[] q = ToVector(x.Features);
                int pred = vecs.OrderBy(t => EuclideanDist(q, t.Vec)).Take(5)
                    .GroupBy(t => t.Trust).OrderByDescending(g => g.Count()).First().Key;
                return pred == lvl;
            });
            return $"{TrustNames[lvl]}={ok}/{lvlVal.Count}";
        }));

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E40-TRUST-SCORER",
            Tier              = tier,
            BaselinePrimary   = baseAcc,
            EnhancedPrimary   = enhAcc,
            PrimaryMetricName = "trust_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = (double)val.Count(x => x.TrustLevel == LOW) / val.Count,
            ScalingSignalNote = "low_trust_ratio (сложные случаи — LOW vs MEDIUM граница)",
            ShouldScaleUp     = val.Count < 15,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"examples={examples.Count}, val={val.Count}, " +
                    $"baseAcc={baseAcc:P1}, enhAcc={enhAcc:P1}, " +
                    $"per_level=[{perLevel}]"
        });
    }

    private static float[] ToVector(SourceFeatures f) =>
    [
        f.IsInternal      ? 1f : 0f,
        f.HasCIPassing    ? 1f : 0f,
        f.HasTests        ? 1f : 0f,
        Math.Max(0, 1f - f.UpdateDaysAgo / 365f),  // свежесть (1=сегодня, 0=год назад)
        f.PopularityScore / 3f,
        f.IsOfficialDocs  ? 1f : 0f,
        f.DomainMatch     ? 1f : 0f,
    ];

    private static double EuclideanDist(float[] a, float[] b)
        => Math.Sqrt(a.Zip(b).Sum(p => Math.Pow(p.First - p.Second, 2)));

    private sealed record Example(SourceFeatures Features, int TrustLevel);

    private static List<Example> GenerateExamples(int seed, int n)
    {
        var rng = new Random(seed);
        var templates = new Example[]
        {
            // HIGH trust
            new(new(true,  true,  true,  0,   0, false, true),  HIGH),
            new(new(true,  true,  true,  5,   0, false, true),  HIGH),
            new(new(false, false, false, 0,   0, true,  true),  HIGH), // official docs
            new(new(true,  false, true,  30,  0, false, false), HIGH),
            new(new(false, true,  true,  7,   3, false, true),  HIGH), // popular + CI + tests
            // MEDIUM trust
            new(new(false, true,  true,  30,  2, false, true),  MEDIUM),
            new(new(false, false, true,  60,  2, false, true),  MEDIUM),
            new(new(false, true,  false, 14,  2, false, false), MEDIUM),
            new(new(false, false, false, 7,   0, true,  false), MEDIUM), // official but wrong stack
            new(new(false, true,  true,  90,  1, false, false), MEDIUM),
            // LOW trust
            new(new(false, false, false, 200, 0, false, false), LOW),
            new(new(false, false, false, 365, 0, false, true),  LOW),
            new(new(false, false, false, 30,  0, false, false), LOW),
            new(new(false, true,  false, 180, 0, false, false), LOW),
        };
        var result = new List<Example>();
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

# E41 — IdleDetector: безопасно ли начинать обучение?

## Гипотеза

Одного CPU-порога недостаточно. Нужны четыре условия одновременно:
```
CPU_IDLE:        cpu_usage < 20%
INPUT_IDLE:      последний ввод > 10 минут назад
BATTERY_OK:      заряд > 50% ИЛИ зарядка подключена
DISK_SPACE_OK:   свободно > 2 ГБ
```

**Дорогая ошибка — START_WHEN_ACTIVE:** начать обучение когда пользователь работает.
**Дешёвая ошибка — SKIP_WHEN_IDLE:** не начать когда можно было.

## Шаг 1 — Создать `E41IdleDetector.cs`

**Путь:** `35.ExperimentsV2/Experiments/E41IdleDetector.cs`

```csharp
namespace ExperimentsV2;

// E41: IdleDetector — безопасно ли начать автономное обучение.
// Baseline:  одно условие — cpu_usage < 20%
// Enhanced:  все 4 условия (CPU + input + battery + disk)
// Метрика:   false_start_rate (страшная ошибка) + false_idle_rate (дешёвая)
public sealed class E41IdleDetector
{
    public const int SAFE_TO_TRAIN = 1;
    public const int WAIT          = 0;

    public sealed record SystemState(
        float CpuUsagePct,       // 0-100
        int   InputIdleMinutes,  // минут без мыши/клавиатуры
        float BatteryPct,        // 0-100 (100 если на зарядке)
        bool  IsCharging,        // зарядка подключена
        float DiskFreeGb,        // ГБ свободного места
        bool  ActuallyIdle);     // правильный ответ

    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var states = GenerateStates(seed: 42, n: tier * 80);
        var rng    = new Random(42);
        var val    = states.OrderBy(_ => rng.Next()).Skip((int)(states.Count * 0.8)).ToList();

        // ── Baseline: только CPU < 20% ────────────────────────────────────────
        var t0 = Stopwatch();
        var basePredictions = val.Select(s => s.CpuUsagePct < 20f ? SAFE_TO_TRAIN : WAIT).ToList();
        int baseOk           = val.Zip(basePredictions).Count(p => (p.First.ActuallyIdle ? 1 : 0) == p.Second);
        // Дорогая ошибка: predict SAFE_TO_TRAIN но ActuallyIdle=false
        int baseFalseStart   = val.Zip(basePredictions)
            .Count(p => p.Second == SAFE_TO_TRAIN && !p.First.ActuallyIdle);
        int baseIdleCount    = val.Count(s => !s.ActuallyIdle);
        double baseAcc       = baseOk / (double)val.Count;
        double baseFSR       = baseIdleCount > 0 ? baseFalseStart / (double)baseIdleCount : 0;
        double baselineMs    = ElapsedMs(t0);

        // ── Enhanced: все 4 условия ───────────────────────────────────────────
        var t1 = Stopwatch();
        var enhPredictions = val.Select(s => PredictFourConditions(s)).ToList();
        int enhOk          = val.Zip(enhPredictions).Count(p => (p.First.ActuallyIdle ? 1 : 0) == p.Second);
        int enhFalseStart  = val.Zip(enhPredictions)
            .Count(p => p.Second == SAFE_TO_TRAIN && !p.First.ActuallyIdle);
        int enhFalseIdle   = val.Zip(enhPredictions)
            .Count(p => p.Second == WAIT && p.First.ActuallyIdle);
        double enhAcc      = enhOk / (double)val.Count;
        double enhFSR      = baseIdleCount > 0 ? enhFalseStart / (double)baseIdleCount : 0;
        double enhFIR      = val.Count(s => s.ActuallyIdle) > 0
            ? enhFalseIdle / (double)val.Count(s => s.ActuallyIdle) : 0;
        double enhancedMs  = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E41-IDLE-DETECTOR",
            Tier              = tier,
            BaselinePrimary   = baseAcc,
            EnhancedPrimary   = enhAcc,
            PrimaryMetricName = "idle_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = baseFSR - enhFSR,
            ScalingSignalNote = "false_start_reduction (батарея не садится зря)",
            ShouldScaleUp     = val.Count < 20,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"val={val.Count}, baseAcc={baseAcc:P1}(FSR={baseFSR:P1}), " +
                    $"enhAcc={enhAcc:P1}(FSR={enhFSR:P1},FIR={enhFIR:P1})"
        });
    }

    private static int PredictFourConditions(SystemState s)
    {
        bool cpuOk     = s.CpuUsagePct < 20f;
        bool inputOk   = s.InputIdleMinutes >= 10;
        bool batteryOk = s.IsCharging || s.BatteryPct >= 50f;
        bool diskOk    = s.DiskFreeGb >= 2.0f;
        return (cpuOk && inputOk && batteryOk && diskOk) ? SAFE_TO_TRAIN : WAIT;
    }

    private static List<SystemState> GenerateStates(int seed, int n)
    {
        var rng = new Random(seed);
        var templates = new SystemState[]
        {
            // SAFE_TO_TRAIN (все условия выполнены)
            new(5f,  30, 80f, false, 10f, true),
            new(3f,  60, 95f, true,  5f,  true),
            new(10f, 15, 55f, true,  3f,  true),
            new(15f, 45, 70f, false, 8f,  true),
            // WAIT — высокий CPU
            new(85f, 30, 80f, false, 10f, false),
            new(60f, 60, 90f, true,  5f,  false),
            new(45f, 0,  75f, false, 6f,  false),
            // WAIT — недавний ввод
            new(5f,  0,  80f, false, 10f, false),
            new(8f,  3,  70f, true,  5f,  false),
            // WAIT — мало батареи
            new(5f,  30, 15f, false, 10f, false),
            new(10f, 20, 25f, false, 8f,  false),
            // WAIT — мало места
            new(5f,  30, 80f, false, 0.5f, false),
            new(8f,  60, 90f, true,  1.2f, false),
            // Граничные случаи (CPU=18% — пройдёт baseline но не должно)
            new(18f, 5,  30f, false, 0.8f, false),
            new(19f, 8,  45f, false, 1.5f, false),
        };
        var result = new List<SystemState>();
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

# E42 — DiskManager: value-based вытеснение vs FIFO

## Гипотеза

Когда WARM-хранилище заполняется (≤ 1 ГБ), нужно вытеснить старые чанки.
FIFO (старейшие первыми) теряет ценные ранние примеры.
Value-based вытеснение (низкий trust + низкое разнообразие = первые на вылет)
сохраняет больше знаний при том же лимите.

**Ценность чанка:**
```
value = trust_score × diversity_score × (1 - age_factor)
```

## Шаг 1 — Создать `E42DiskManager.cs`

**Путь:** `35.ExperimentsV2/Experiments/E42DiskManager.cs`

```csharp
namespace ExperimentsV2;

// E42: DiskManager — value-based вытеснение vs FIFO при заполнении.
// Baseline:  FIFO — вытеснить самые старые чанки первыми
// Enhanced:  value-based — вытеснить наименее ценные (low trust × low diversity)
// Метрика:   knowledge_retention (E17-предикатов охвачено после вытеснения)
//            + domain_coverage (сколько доменов сохранено)
public sealed class E42DiskManager
{
    private readonly int _maxChunks; // лимит чанков в WARM
    public E42DiskManager(int maxChunks = 20) => _maxChunks = maxChunks;

    public sealed record DataChunk(
        int      Id,
        int[]    Predicates,  // E17 предикаты содержимого
        int      Domain,      // 0-4 (из DomainClassifier)
        float    TrustScore,  // 0.0-1.0
        int      AgeDays,     // сколько дней хранится
        int      SizeKb);     // размер в КБ

    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var rng    = new Random(42);
        var allChunks = GenerateChunks(rng, total: _maxChunks * 2);

        // Начальное заполнение: первые _maxChunks чанков (хронологически)
        var warmStore = allChunks.Take(_maxChunks).ToList();
        // Новые чанки которые надо добавить (ещё _maxChunks)
        var incoming  = allChunks.Skip(_maxChunks).ToList();

        // Посчитать coverage на "полном" наборе
        double fullCoverage = ComputeCoverage(allChunks);

        // ── Baseline: FIFO — вытеснить самые старые ──────────────────────────
        var t0 = Stopwatch();
        var baseStore = warmStore.ToList();
        foreach (var chunk in incoming)
        {
            if (baseStore.Count >= _maxChunks)
                baseStore.Remove(baseStore.OrderByDescending(c => c.AgeDays).First());
            baseStore.Add(chunk);
        }
        double baseCoverage = ComputeCoverage(baseStore);
        double baseDomains  = baseStore.Select(c => c.Domain).Distinct().Count() / 5.0;
        double baselineMs   = ElapsedMs(t0);

        // ── Enhanced: value-based вытеснение ─────────────────────────────────
        var t1 = Stopwatch();
        var enhStore = warmStore.ToList();
        foreach (var chunk in incoming)
        {
            if (enhStore.Count >= _maxChunks)
            {
                // Вытеснить чанк с наименьшей ценностью
                var toEvict = enhStore
                    .OrderBy(c => ComputeValue(c, enhStore))
                    .First();
                enhStore.Remove(toEvict);
            }
            enhStore.Add(chunk);
        }
        double enhCoverage = ComputeCoverage(enhStore);
        double enhDomains  = enhStore.Select(c => c.Domain).Distinct().Count() / 5.0;
        double enhancedMs  = ElapsedMs(t1);

        // Retention = coverage сохранённого / coverage полного набора
        double baseRetention = fullCoverage > 0 ? baseCoverage / fullCoverage : 0;
        double enhRetention  = fullCoverage > 0 ? enhCoverage  / fullCoverage : 0;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E42-DISK-MANAGER",
            Tier              = tier,
            BaselinePrimary   = baseRetention,
            EnhancedPrimary   = enhRetention,
            PrimaryMetricName = "knowledge_retention",
            LowerIsBetter     = false,
            ScalingSignal     = enhDomains - baseDomains,
            ScalingSignalNote = "domain_coverage_gain (больше доменов сохранено)",
            ShouldScaleUp     = allChunks.Count < 20,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"maxChunks={_maxChunks}, incoming={incoming.Count}, " +
                    $"baseRetention={baseRetention:P1}(domains={baseDomains:F1}/5), " +
                    $"enhRetention={enhRetention:P1}(domains={enhDomains:F1}/5)"
        });
    }

    // Покрытие = уникальных предикатов / из всех возможных (12)
    private static double ComputeCoverage(IEnumerable<DataChunk> chunks)
    {
        var allPreds = chunks.SelectMany(c => c.Predicates).ToHashSet();
        return allPreds.Count / 12.0;
    }

    // Ценность чанка в контексте текущего хранилища
    private static double ComputeValue(DataChunk chunk, List<DataChunk> store)
    {
        // Разнообразие: сколько уникальных предикатов этот чанк добавляет
        var otherPreds = store.Where(c => c.Id != chunk.Id)
            .SelectMany(c => c.Predicates).ToHashSet();
        double diversity = chunk.Predicates.Count(p => !otherPreds.Contains(p))
            / (double)Math.Max(1, chunk.Predicates.Length);
        double ageFactor = Math.Min(1.0, chunk.AgeDays / 90.0);
        return chunk.TrustScore * (0.5 + 0.5 * diversity) * (1.0 - 0.3 * ageFactor);
    }

    private static List<DataChunk> GenerateChunks(Random rng, int total)
    {
        var result = new List<DataChunk>();
        var allPreds = new[] { 0,1,2,3,4,5,6,7,8,9,10,11 };
        for (int i = 0; i < total; i++)
        {
            int domain = rng.Next(5);
            float trust = domain == 0 ? 0.9f + rng.NextSingle() * 0.1f  // внутренние
                        : domain == 1 ? 0.6f + rng.NextSingle() * 0.3f  // официальные
                        : 0.1f + rng.NextSingle() * 0.5f;                // внешние
            var preds = allPreds.Where(_ => rng.NextDouble() > 0.6).ToArray();
            if (preds.Length == 0) preds = new[] { rng.Next(12) };
            result.Add(new DataChunk(i, preds, domain, trust,
                AgeDays: rng.Next(0, 90), SizeKb: rng.Next(10, 200)));
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

# E43 — ReplayBuffer: предотвратить катастрофическое забывание

## Гипотеза

Если обучать только на новых данных — модель забывает старые паттерны.
70% новых + 30% из Replay Buffer сохраняет точность на старых примерах.

**Катастрофическое забывание в нашей системе:**
Раунд 1: обучить на CODE_Q → высокая точность на CODE_Q.
Раунд 2: обучить только на SECURITY → точность на CODE_Q падает.
Раунд 2 с replay: 70% SECURITY + 30% CODE_Q из буфера → обе точности высокие.

## Шаг 1 — Создать `E43ReplayBuffer.cs`

**Путь:** `35.ExperimentsV2/Experiments/E43ReplayBuffer.cs`

```csharp
namespace ExperimentsV2;

// E43: ReplayBuffer — 70/30 смешивание предотвращает забывание.
// Baseline:  только новые данные (100% SECURITY на раунде 2)
// Enhanced:  70% SECURITY + 30% из Replay Buffer (CODE_Q из раунда 1)
// Метрика:   retention_rate — точность на CODE_Q после обучения на SECURITY
//            + forgetting_delta (насколько упала точность на старом домене)
public sealed class E43ReplayBuffer
{
    private const float ReplayRatio = 0.3f;

    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
    {
        if (files.Count < 4)
            return OperationResult<ExperimentV2Result>.Fail("Нужно ≥ 4 файлов");

        var rng = new Random(42);

        // Разделить файлы на "старый домен" и "новый домен"
        var split      = (int)(files.Count * 0.5);
        var oldDomain  = files.Take(split).ToList();   // CODE_Q (первые файлы)
        var newDomain  = files.Skip(split).ToList();   // SECURITY (последние файлы)

        // Извлечь предикатные пары из каждого домена
        var oldPairs = ExtractPairs(oldDomain);
        var newPairs = ExtractPairs(newDomain);

        if (oldPairs.Count < 5 || newPairs.Count < 5)
            return OperationResult<ExperimentV2Result>.Fail("Слишком мало пар");

        // Validation: только из old domain (проверяем забывание)
        int valSplit   = (int)(oldPairs.Count * 0.8);
        var oldTrain   = oldPairs.Take(valSplit).ToList();
        var oldVal     = oldPairs.Skip(valSplit).ToList();

        // Шаг 1: обучить на OLD → установить baseline точность на old
        var baseModel = BuildBigram(oldTrain);
        double baseAccOnOld = Evaluate(baseModel, oldVal);

        // ── Baseline: обучить только на NEW (без replay) ──────────────────────
        var t0 = Stopwatch();
        var noReplayModel = BuildBigram(newPairs);
        double noReplayAccOnOld = Evaluate(noReplayModel, oldVal);
        double forgetting = baseAccOnOld - noReplayAccOnOld;
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: 70% NEW + 30% REPLAY ───────────────────────────────────
        var t1 = Stopwatch();
        int replayCount   = (int)(newPairs.Count * ReplayRatio / (1 - ReplayRatio));
        var replayBuffer  = oldTrain.OrderBy(_ => rng.Next()).Take(replayCount).ToList();
        var mixedPairs    = newPairs.Concat(replayBuffer)
            .OrderBy(_ => rng.Next()).ToList();

        var replayModel = BuildBigram(mixedPairs);
        double replayAccOnOld = Evaluate(replayModel, oldVal);
        double retentionRate  = baseAccOnOld > 0 ? replayAccOnOld / baseAccOnOld : 0;
        double enhancedMs     = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E43-REPLAY-BUFFER",
            Tier              = tier,
            BaselinePrimary   = noReplayAccOnOld,
            EnhancedPrimary   = replayAccOnOld,
            PrimaryMetricName = "retention_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = retentionRate,
            ScalingSignalNote = "retention_rate (1.0 = полное сохранение, < 0.7 = плохо)",
            ShouldScaleUp     = oldVal.Count < 10,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"oldPairs={oldPairs.Count}, newPairs={newPairs.Count}, " +
                    $"replayBuffer={replayBuffer.Count}, " +
                    $"baseAccOnOld={baseAccOnOld:P1}, " +
                    $"noReplayAcc={noReplayAccOnOld:P1}(forgetting={forgetting:+.1%}), " +
                    $"replayAcc={replayAccOnOld:P1}(retention={retentionRate:P1})"
        });
    }

    private static List<(int, int)> ExtractPairs(IReadOnlyList<(string, string)> files)
    {
        var preds = new List<int>();
        foreach (var (_, text) in files)
        {
            var r = LogicExtractor.Extract(text);
            if (r.IsSuccess) preds.AddRange(r.Value!);
        }
        return preds.Zip(preds.Skip(1)).Select(p => (p.First, p.Second)).ToList();
    }

    private static Dictionary<int, Dictionary<int, int>> BuildBigram(
        IEnumerable<(int, int)> pairs)
    {
        var m = new Dictionary<int, Dictionary<int, int>>();
        foreach (var (c, n) in pairs)
        {
            if (!m.TryGetValue(c, out var d)) m[c] = d = new();
            d[n] = d.GetValueOrDefault(n, 0) + 1;
        }
        return m;
    }

    private static double Evaluate(Dictionary<int, Dictionary<int, int>> model,
        List<(int, int)> val)
    {
        if (val.Count == 0) return 0;
        return val.Count(p =>
        {
            if (!model.TryGetValue(p.Item1, out var d) || d.Count == 0) return false;
            return d.MaxBy(kv => kv.Value).Key == p.Item2;
        }) / (double)val.Count;
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
```

---

# E44 — DatasetWeighter: вес × 10 на важный домен

## Гипотеза

50 security примеров с weight=10 эквивалентны 500 обычным.
Взвешенное обучение улучшает точность на важном домене (security)
без деградации на остальных сверх допустимого.

## Шаг 1 — Создать `E44DatasetWeighter.cs`

**Путь:** `35.ExperimentsV2/Experiments/E44DatasetWeighter.cs`

```csharp
namespace ExperimentsV2;

// E44: DatasetWeighter — вес × 10 на security примерах.
// Baseline:  равные веса (все × 1)
// Enhanced:  security примеры × 10 (повторить 10 раз в обучении)
// Метрика:   security_accuracy + general_accuracy + weighted_f1
//            weighted_f1 = 0.7 × security + 0.3 × general (пользователь ставит security важнее)
public sealed class E44DatasetWeighter
{
    private const float SecurityWeight  = 10.0f;
    private const float SecurityFocus   = 0.7f;  // насколько важна security vs general

    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
    {
        var rng = new Random(42);

        // Разделить на "general" и "security" примеры
        // Security = файлы с throw/catch/null patterns (из E14 AutoLabel)
        var generalFiles  = files.Where(f => !IsSecurityFile(f.Text)).ToList();
        var securityFiles = files.Where(f => IsSecurityFile(f.Text)).ToList();

        if (generalFiles.Count < 2 || securityFiles.Count < 1)
            return OperationResult<ExperimentV2Result>.Fail(
                $"Слишком мало файлов: general={generalFiles.Count}, security={securityFiles.Count}");

        var genPairs = ExtractPairs(generalFiles);
        var secPairs = ExtractPairs(securityFiles);

        if (genPairs.Count < 4 || secPairs.Count < 2)
            return OperationResult<ExperimentV2Result>.Fail("Слишком мало пар");

        int gSplit = (int)(genPairs.Count * 0.8);
        int sSplit = (int)(secPairs.Count * 0.8);
        var genTrain  = genPairs.Take(gSplit).ToList();
        var secTrain  = secPairs.Take(sSplit).ToList();
        var genVal    = genPairs.Skip(gSplit).ToList();
        var secVal    = secPairs.Skip(sSplit).ToList();

        // ── Baseline: равные веса ─────────────────────────────────────────────
        var t0 = Stopwatch();
        var baseModel    = BuildBigram(genTrain.Concat(secTrain));
        double baseGenAcc = Evaluate(baseModel, genVal);
        double baseSecAcc = Evaluate(baseModel, secVal);
        double baseWF1    = SecurityFocus * baseSecAcc + (1 - SecurityFocus) * baseGenAcc;
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: security × 10 ───────────────────────────────────────────
        var t1 = Stopwatch();
        // Повторить security пары 10 раз (имитация weight × 10)
        var weightedSec  = Enumerable.Range(0, (int)SecurityWeight)
            .SelectMany(_ => secTrain).ToList();
        var enhModel     = BuildBigram(genTrain.Concat(weightedSec).OrderBy(_ => rng.Next()));
        double enhGenAcc = Evaluate(enhModel, genVal);
        double enhSecAcc = Evaluate(enhModel, secVal);
        double enhWF1    = SecurityFocus * enhSecAcc + (1 - SecurityFocus) * enhGenAcc;
        double enhancedMs = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E44-DATASET-WEIGHTER",
            Tier              = tier,
            BaselinePrimary   = baseWF1,
            EnhancedPrimary   = enhWF1,
            PrimaryMetricName = "weighted_f1",
            LowerIsBetter     = false,
            ScalingSignal     = enhSecAcc - baseSecAcc,
            ScalingSignalNote = "security_accuracy_gain (главная цель взвешивания)",
            ShouldScaleUp     = secVal.Count < 5,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"genPairs={genPairs.Count}, secPairs={secPairs.Count}, weight={SecurityWeight}x, " +
                    $"base: gen={baseGenAcc:P1} sec={baseSecAcc:P1} wF1={baseWF1:F2}, " +
                    $"enh:  gen={enhGenAcc:P1} sec={enhSecAcc:P1} wF1={enhWF1:F2}"
        });
    }

    private static bool IsSecurityFile(string text)
    {
        var lower = text.ToLowerInvariant();
        return lower.Contains("throw") || lower.Contains("null") && lower.Contains("return false")
            || ActionLabelGenerator.Label(text) == ActionLabelGenerator.ADD_NULL_CHECK;
    }

    private static List<(int, int)> ExtractPairs(IEnumerable<(string, string)> files)
    {
        var preds = new List<int>();
        foreach (var (_, text) in files)
        {
            var r = LogicExtractor.Extract(text);
            if (r.IsSuccess) preds.AddRange(r.Value!);
        }
        return preds.Zip(preds.Skip(1)).Select(p => (p.First, p.Second)).ToList();
    }

    private static Dictionary<int, Dictionary<int, int>> BuildBigram(
        IEnumerable<(int, int)> pairs)
    {
        var m = new Dictionary<int, Dictionary<int, int>>();
        foreach (var (c, n) in pairs)
        {
            if (!m.TryGetValue(c, out var d)) m[c] = d = new();
            d[n] = d.GetValueOrDefault(n, 0) + 1;
        }
        return m;
    }

    private static double Evaluate(Dictionary<int, Dictionary<int, int>> model,
        List<(int, int)> val)
    {
        if (val.Count == 0) return 0;
        return val.Count(p =>
        {
            if (!model.TryGetValue(p.Item1, out var d) || d.Count == 0) return false;
            return d.MaxBy(kv => kv.Value).Key == p.Item2;
        }) / (double)val.Count;
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
```

---

# E45 — CurriculumOrganizer: порядок обучения имеет значение

## Гипотеза

Нейросети имеют recency bias — последние примеры влияют сильнее.
Curriculum Learning: CODE_Q → BUG_FIX → SECURITY (последним).
Baseline: случайный порядок.

**Slope gain** из E31 показал +72%. E45 проверяет ту же идею
но с доменной специализацией а не просто больше данных.

## Шаг 1 — Создать `E45CurriculumOrganizer.cs`

**Путь:** `35.ExperimentsV2/Experiments/E45CurriculumOrganizer.cs`

```csharp
namespace ExperimentsV2;

// E45: CurriculumOrganizer — порядок обучения (простое сначала, важное последним).
// Baseline:  случайный порядок всех данных
// Enhanced:  CODE_Q (70%) → Replay (20%) → SECURITY (10%, последними)
// Метрика:   final_accuracy + learning_slope (скорость улучшения по раундам)
public sealed class E45CurriculumOrganizer
{
    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
    {
        if (files.Count < 4)
            return OperationResult<ExperimentV2Result>.Fail("Нужно ≥ 4 файлов");

        var rng = new Random(42);

        // Разделить файлы на CODE_Q (easy) и SECURITY (hard, важный)
        var easyFiles = files.Take((int)(files.Count * 0.7)).ToList();
        var hardFiles = files.Skip((int)(files.Count * 0.7)).ToList();

        var easyPairs = ExtractPairs(easyFiles);
        var hardPairs = ExtractPairs(hardFiles);

        if (easyPairs.Count < 4)
            return OperationResult<ExperimentV2Result>.Fail("Слишком мало пар");

        // Validation: только hard (security) — главная метрика
        int hSplit  = Math.Max(1, (int)(hardPairs.Count * 0.8));
        var hardVal = hardPairs.Skip(hSplit).ToList();
        var hardTrain = hardPairs.Take(hSplit).ToList();

        if (hardVal.Count == 0) hardVal = hardPairs.TakeLast(1).ToList();

        // ── Baseline: случайный порядок ───────────────────────────────────────
        var t0 = Stopwatch();
        var allPairs   = easyPairs.Concat(hardTrain).OrderBy(_ => rng.Next()).ToList();
        var baseAccs   = new List<double>();
        // Симулировать 3 раунда: после каждой трети данных
        for (int r = 1; r <= 3; r++)
        {
            var partData = allPairs.Take(allPairs.Count * r / 3).ToList();
            var model    = BuildBigram(partData);
            baseAccs.Add(Evaluate(model, hardVal));
        }
        double baseSlope  = ComputeSlope(baseAccs);
        double baseFinal  = baseAccs[^1];
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: curriculum порядок ─────────────────────────────────────
        var t1 = Stopwatch();
        // Порядок: easy сначала, hard последними
        int replayN    = (int)(hardTrain.Count * 0.2);
        var replayBuf  = easyPairs.OrderBy(_ => rng.Next()).Take(replayN).ToList();
        var curriculum = easyPairs                          // 70% CODE_Q сначала
            .Concat(replayBuf)                              // 20% Replay в середине
            .Concat(hardTrain)                              // 10% SECURITY последними
            .ToList();

        var enhAccs = new List<double>();
        for (int r = 1; r <= 3; r++)
        {
            var partData = curriculum.Take(curriculum.Count * r / 3).ToList();
            var model    = BuildBigram(partData);
            enhAccs.Add(Evaluate(model, hardVal));
        }
        double enhSlope  = ComputeSlope(enhAccs);
        double enhFinal  = enhAccs[^1];
        double enhancedMs = ElapsedMs(t1);
        double slopeGain = baseSlope > 0 ? (enhSlope - baseSlope) / baseSlope : 0;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E45-CURRICULUM-ORGANIZER",
            Tier              = tier,
            BaselinePrimary   = baseFinal,
            EnhancedPrimary   = enhFinal,
            PrimaryMetricName = "final_security_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = slopeGain,
            ScalingSignalNote = "slope_gain (насколько быстрее учится curriculum)",
            ShouldScaleUp     = hardVal.Count < 3,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"easyPairs={easyPairs.Count}, hardPairs={hardPairs.Count}, " +
                    $"base: rounds=[{string.Join(",",baseAccs.Select(a => $"{a:P0}"))}] slope={baseSlope:F3}, " +
                    $"enh:  rounds=[{string.Join(",",enhAccs.Select(a => $"{a:P0}"))}] slope={enhSlope:F3}, " +
                    $"slopeGain={slopeGain:+.1%}"
        });
    }

    private static List<(int, int)> ExtractPairs(IEnumerable<(string, string)> files)
    {
        var preds = new List<int>();
        foreach (var (_, text) in files)
        {
            var r = LogicExtractor.Extract(text);
            if (r.IsSuccess) preds.AddRange(r.Value!);
        }
        return preds.Zip(preds.Skip(1)).Select(p => (p.First, p.Second)).ToList();
    }

    private static Dictionary<int, Dictionary<int, int>> BuildBigram(
        IEnumerable<(int, int)> pairs)
    {
        var m = new Dictionary<int, Dictionary<int, int>>();
        foreach (var (c, n) in pairs)
        {
            if (!m.TryGetValue(c, out var d)) m[c] = d = new();
            d[n] = d.GetValueOrDefault(n, 0) + 1;
        }
        return m;
    }

    private static double Evaluate(Dictionary<int, Dictionary<int, int>> model,
        List<(int, int)> val)
    {
        if (val.Count == 0) return 0;
        return val.Count(p =>
        {
            if (!model.TryGetValue(p.Item1, out var d) || d.Count == 0) return false;
            return d.MaxBy(kv => kv.Value).Key == p.Item2;
        }) / (double)val.Count;
    }

    private static double ComputeSlope(List<double> values)
    {
        if (values.Count < 2) return 0;
        double n = values.Count, sx = 0, sy = 0, sxy = 0, sx2 = 0;
        for (int i = 0; i < values.Count; i++)
        { sx += i; sy += values[i]; sxy += i * values[i]; sx2 += i * i; }
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

## Добавить все 6 в Dispatcher/Runner/ResultWriter

```csharp
// Dispatcher:
public OperationResult<ExperimentV2Result> RunE40(int tier = 1) => new E40TrustScorer().Run(tier);
public OperationResult<ExperimentV2Result> RunE41(int tier = 1) => new E41IdleDetector().Run(tier);
public OperationResult<ExperimentV2Result> RunE42(int tier = 1) => new E42DiskManager().Run(tier);
public OperationResult<ExperimentV2Result> RunE43(IReadOnlyList<(string,string)> f, int tier=1)
    => new E43ReplayBuffer().Run(f, tier);
public OperationResult<ExperimentV2Result> RunE44(IReadOnlyList<(string,string)> f, int tier=1)
    => new E44DatasetWeighter().Run(f, tier);
public OperationResult<ExperimentV2Result> RunE45(IReadOnlyList<(string,string)> f, int tier=1)
    => new E45CurriculumOrganizer().Run(f, tier);

// Runner: "E40"–"E45" с dispatch методами
// ResultWriter: все 6 имён
```

---

## Ожидаемые результаты

| Exp | Baseline | Enhanced | Ключевой сигнал |
|---|---|---|---|
| E40 TrustScorer | ~71% (simple rule) | ~88% | LOW vs MEDIUM граница |
| E41 IdleDetector | ~65% (CPU только) | ~90% | false_start_rate ↓ |
| E42 DiskManager | ~72% retention | ~85% retention | domain_coverage_gain |
| E43 ReplayBuffer | ~30% retention | ~75% retention | forgetting_delta ↓ |
| E44 Weighter | wF1=0.50 | wF1=0.68 | security_accuracy +20% |
| E45 Curriculum | slope=0.02 | slope=0.05 | +150% learning rate |

---

## Полный цикл автономного обучения

```
Ночь, компьютер пассивен:

1. E41 IdleDetector:  CPU=5% + 30мин без ввода + 80% батарея + 8 ГБ диск
                      → SAFE_TO_TRAIN ✓

2. DataSourceFinder:  найти источники (внутренний Git + docs.microsoft.com)
                      ↓
3. E40 TrustScorer:   внутренний Git → HIGH (0.92), docs → HIGH (0.87)
                      отфильтровать LOW-trust источники
                      ↓
4. SemanticCompiler:  E15→E16→E17 обработка найденных данных
                      ↓
5. E42 DiskManager:   добавить в WARM, вытеснить наименее ценное
                      WARM=980МБ → вытеснить 3 LOW-trust чанка → WARM=750МБ
                      ↓
6. E44 Weighter:      пометить security примеры × 10
   E45 Curriculum:    CODE_Q сначала, security последними
                      ↓
7. IncrementalTrainer (E31): LoRA цикл с replay buffer (E43)
                      70% новое + 30% replay буфер
                      ↓
8. E42 DiskManager:   если WARM > 900МБ: дистиллировать в COLD LoRA адаптер
                      освободить место
                      ↓
Утром: пользователь работает с моделью которая стала точнее за ночь
```

---

*E40-E45 план v1.0 — Автономное обучение · не касается XIIGen*
