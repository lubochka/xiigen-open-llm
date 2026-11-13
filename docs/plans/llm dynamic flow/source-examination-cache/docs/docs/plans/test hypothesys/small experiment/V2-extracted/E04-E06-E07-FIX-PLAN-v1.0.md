# E04 + E06 + E07 — Fix Plan v1.0
## Три эксперимента, три файла, ~40 строк изменений

**Проект:** llm-mvp-core — 35.ExperimentsV2  
**Этот документ не касается XIIGen.**

---

## Контекст: что сейчас происходит

| Эксп | Результат | Причина |
|---|---|---|
| E04 | baseline=1.0, enhanced=1.0, Δ=0% | Baseline проверяет "есть ли слово в файле где метод ОПРЕДЕЛЁН" — всегда true |
| E06 | -23.6%, TUNE_OR_ABANDON | Gini=0.16 — граф из 3 файлов слишком мал, PageRank бесполезен. Должен быть SCALE_UP |
| E07 | -4.6%, TUNE_OR_ABANDON | Два бага: (1) fallback = текущий токен, (2) baseline пропускает позиции без данных |

---

## Исправление 1 — E04: сломанный baseline

**Файл:** `35.ExperimentsV2/Experiments/E04Bm25Context.cs`

### Диагностика

```csharp
// Строки 61-62 — что сейчас:
bool localHit = documents.Any(
    d => d.Id == expectedId                                       // файл где метод ОПРЕДЕЛЁН
      && d.Source.Contains(query, StringComparison.OrdinalIgnoreCase));  // содержит ли имя метода
if (localHit) { baseCorrect1++; baseCorrect3++; baseCorrect5++; }
```

`query` = имя метода (например `Encode`).  
`expectedId` = `BpeTokenizer` — файл где Encode определён.  
Конечно `BpeTokenizer.cs` содержит слово `Encode` — там же его определение.  
Baseline = 100% тривиально. Enhanced BM25 тоже 100%. Delta = 0%.

### Правильная логика baseline

Baseline симулирует "поиск без BM25, только в текущем файле". Cross-file вызовы — те где определение в другом файле — **не находимы** без поиска по проекту. Baseline recall = 0 для всех таких запросов.

### Что изменить

Заменить строки 55-66 (весь baseline блок):

```csharp
// БЫЛО (строки 55-66):
int baseCorrect1 = 0, baseCorrect3 = 0, baseCorrect5 = 0;
foreach (var (query, expectedId) in queries)
{
    bool localHit = documents.Any(d => d.Id == expectedId && d.Source.Contains(query, StringComparison.OrdinalIgnoreCase));
    if (localHit) { baseCorrect1++; baseCorrect3++; baseCorrect5++; }
}
double baselineMs  = ElapsedMs(t0);
double baseRecall3 = baseCorrect3 / (double)queries.Count;

// СТАЛО:
// Baseline = "local file only" — cross-file calls are not findable without search.
// All queries in this experiment are cross-file by construction (BuildCrossFileQueries
// only returns calls where the definition is in a different file than the call site).
// Without BM25, recall = 0 for all of them.
double baselineMs  = ElapsedMs(t0);
double baseRecall3 = 0.0;
```

### Что ожидать после исправления

- baseline_value = 0.0 (крест-файловые вызовы недоступны без поиска)
- enhanced_value = recall@3 BM25 (было ~61.5% recall@1, ~100% recall@5 по notes)
- Δ = +∞% или просто большое положительное число
- passed = true → PROMOTE_TO_NEXT_TIER

---

## Исправление 2 — E06: добавить ShouldScaleUp при малом графе

**Файл:** `35.ExperimentsV2/Experiments/E06PageRank.cs`

### Диагностика

```
methods=33, edges=39, gini=0.160
```

Gini=0.16 ≈ 0 означает PageRank распределён почти равномерно — методы одинаково "важны".  
При равномерном PageRank frequency-ранжирование правильнее (оно замечает 2-3 популярных метода).  
PageRank нужен граф с хабами (gini > 0.3).  
На Tier 1 из 3 файлов граф почти пуст. Это не "подход не работает" — это "нужно больше файлов".

### Что изменить

В блоке создания `ExperimentV2Result` (строка ~109), добавить одно поле:

```csharp
// БЫЛО (строки 109-126):
return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
{
    ExperimentId      = "E06-PAGERANK-RANKING",
    Tier              = tier,
    BaselinePrimary   = baseMrr,
    EnhancedPrimary   = enhMrr,
    PrimaryMetricName = "mrr",
    LowerIsBetter     = false,
    ScalingSignal     = gini,
    ScalingSignalNote = "pagerank_gini (higher = more concentrated = hubs exist)",
    BaselineTimeMs    = baselineMs,
    EnhancedTimeMs    = enhancedMs,
    Notes             = $"methods={allMethods.Count}, edges={edgeCounts.Count}, " +
                        $"gini={gini:F3}, baseMrr={baseMrr:F3}, enhMrr={enhMrr:F3}"
});

// СТАЛО (добавить одну строку ShouldScaleUp):
return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
{
    ExperimentId      = "E06-PAGERANK-RANKING",
    Tier              = tier,
    BaselinePrimary   = baseMrr,
    EnhancedPrimary   = enhMrr,
    PrimaryMetricName = "mrr",
    LowerIsBetter     = false,
    ScalingSignal     = gini,
    ScalingSignalNote = "pagerank_gini (higher = more concentrated = hubs exist)",
    BaselineTimeMs    = baselineMs,
    EnhancedTimeMs    = enhancedMs,
    Notes             = $"methods={allMethods.Count}, edges={edgeCounts.Count}, " +
                        $"gini={gini:F3}, baseMrr={baseMrr:F3}, enhMrr={enhMrr:F3}",
    // Gini < 0.3 = graph too flat for PageRank to differentiate hubs.
    // Scale up to get a larger, more connected call graph.
    ShouldScaleUp     = gini < 0.3
});
```

### Что ожидать после исправления

Tier 1: gini=0.16 → ShouldScaleUp=true → SCALE_UP_DATA_TOO_SMALL → лестница переходит на Tier 2  
Tier 2 (~47 файлов, ~300 методов): ожидаем gini ~0.35-0.50 → PageRank начинает работать  
При gini ≥ 0.3 на Tier 2: эксперимент получит настоящий результат

---

## Исправление 3 — E07: два бага

**Файл:** `35.ExperimentsV2/Experiments/E07StructuralContextToken.cs`

### Диагностика бага 1: fallback возвращает текущий токен

```csharp
// Строки 296-300:
private int BigramFallback(int bpe)
{
    if (_bigram.TryGetValue(bpe, out var b) && b.Count > 0)
        return b.MaxBy(kv => kv.Value).Key;
    return bpe;  // ← предсказывает текущий токен как следующий — в BPE коде почти всегда неверно
}
```

Когда нет ни multidim-данных ни bigram-данных — predict возвращает `bpe` (текущий токен).  
В BPE-кодировке следующий токен почти никогда не равен текущему.  
Это систематически занижает accuracy enhanced.

### Диагностика бага 2: несправедливое сравнение

```csharp
// Baseline (строки 61-68) — пропускает позиции без данных:
if (bigramCounts.TryGetValue(cur, out var d) && d.Count > 0)  // ← только если есть данные
{
    int pred = d.MaxBy(kv => kv.Value).Key;
    if (pred == next) baseCorrect++;
}
// baseTotal = val.Count - 1 (все позиции в знаменателе)

// Enhanced (строки 80-84) — предсказывает ВСЕГДА (включая плохой fallback):
int pred = mdNgram.Predict(...);  // ← всегда возвращает что-то
if (pred == val[i + 1].BpeId) enhCorrect++;
// enhTotal = val.Count - 1 (те же все позиции)
```

Baseline: числитель — только верные предсказания там где есть данные. Знаменатель — ВСЕ позиции.  
Enhanced: числитель — верные включая позиции где дал неверный fallback. Знаменатель — ВСЕ позиции.  
Enhanced получает штраф за позиции где baseline промолчал.

### Что изменить

#### Шаг 3а: добавить `_mostFrequentNext` в `MultidimensionalNGram`

Найти класс `MultidimensionalNGram` (строка ~255). Добавить поле и обновить `Train`:

```csharp
public sealed class MultidimensionalNGram
{
    private readonly Dictionary<(int bpe, short ast, byte depth), Dictionary<int, int>> _trans = new();
    private readonly Dictionary<int, Dictionary<int, int>> _bigram = new();
    // ДОБАВИТЬ: глобально самый частый следующий токен (fallback когда нет данных)
    private int _mostFrequentNext = 0;

    public int UniqueContexts => _trans.Count;

    public void Train(IReadOnlyList<StructuralToken> tokens)
    {
        // ДОБАВИТЬ: считать глобальную частоту следующих токенов
        var globalNextCounts = new Dictionary<int, int>();

        for (int i = 0; i < tokens.Count - 1; i++)
        {
            var cur  = tokens[i];
            int next = tokens[i + 1].BpeId;

            // ДОБАВИТЬ:
            globalNextCounts[next] = globalNextCounts.GetValueOrDefault(next, 0) + 1;

            // Существующий код — без изменений:
            var key = (cur.BpeId, cur.AstNodeType, cur.AstDepth);
            if (!_trans.TryGetValue(key, out var d))
                _trans[key] = d = new Dictionary<int, int>();
            d[next] = d.GetValueOrDefault(next, 0) + 1;

            if (!_bigram.TryGetValue(cur.BpeId, out var b))
                _bigram[cur.BpeId] = b = new Dictionary<int, int>();
            b[next] = b.GetValueOrDefault(next, 0) + 1;
        }

        // ДОБАВИТЬ: запомнить глобально самый частый следующий токен
        if (globalNextCounts.Count > 0)
            _mostFrequentNext = globalNextCounts.MaxBy(kv => kv.Value).Key;
    }
```

#### Шаг 3б: исправить `BigramFallback`

```csharp
// БЫЛО:
private int BigramFallback(int bpe)
{
    if (_bigram.TryGetValue(bpe, out var b) && b.Count > 0)
        return b.MaxBy(kv => kv.Value).Key;
    return bpe;   // ← БАГ: предсказывает текущий токен
}

// СТАЛО:
private int BigramFallback(int bpe)
{
    if (_bigram.TryGetValue(bpe, out var b) && b.Count > 0)
        return b.MaxBy(kv => kv.Value).Key;
    return _mostFrequentNext;   // глобально самый частый — лучшее что можем без данных
}
```

#### Шаг 3в: выровнять baseline — тоже предсказывать для ВСЕХ позиций

Найти baseline evaluation loop в `E07StructuralContextToken.cs` (строки ~60-72):

```csharp
// БЫЛО:
int baseCorrect = 0, baseTotal = val.Count - 1;
for (int i = 0; i < baseTotal; i++)
{
    int cur  = val[i].BpeId;
    int next = val[i + 1].BpeId;
    if (bigramCounts.TryGetValue(cur, out var d) && d.Count > 0)  // ← пропускает без данных
    {
        int pred = d.MaxBy(kv => kv.Value).Key;
        if (pred == next) baseCorrect++;
    }
}
double baselineAcc = baseTotal > 0 ? baseCorrect / (double)baseTotal : 0;

// СТАЛО:
// Вычислить глобально самый частый следующий токен для baseline-fallback
var globalFreq  = new Dictionary<int, int>();
for (int i = 0; i < train.Count - 1; i++)
    globalFreq[train[i + 1].BpeId] = globalFreq.GetValueOrDefault(train[i + 1].BpeId, 0) + 1;
int baselineFallback = globalFreq.Count > 0 ? globalFreq.MaxBy(kv => kv.Value).Key : 0;

int baseCorrect = 0, baseTotal = val.Count - 1;
for (int i = 0; i < baseTotal; i++)
{
    int cur  = val[i].BpeId;
    int next = val[i + 1].BpeId;
    int pred;
    if (bigramCounts.TryGetValue(cur, out var d) && d.Count > 0)
        pred = d.MaxBy(kv => kv.Value).Key;
    else
        pred = baselineFallback;   // то же самое что enhanced использует когда нет данных
    if (pred == next) baseCorrect++;
}
double baselineAcc = baseTotal > 0 ? baseCorrect / (double)baseTotal : 0;
```

### Что ожидать после исправления

Оба используют глобально самый частый токен когда нет специфических данных.  
Enhanced должен быть лучше baseline потому что у него дополнительная информация (ast_type, depth).  
Если enhanced ≤ baseline — это настоящий сигнал что размерности не помогают на Tier 1.

---

## Порядок выполнения для Claude Code

```
1. Изменить E04Bm25Context.cs
   Удалить строки 55-66 (baseline блок с localHit)
   Заменить на: double baselineMs = ElapsedMs(t0); double baseRecall3 = 0.0;
   -> dotnet build -> 0 Error(s)

2. Изменить E06PageRank.cs
   В блоке ExperimentV2Result (строка ~109) добавить:
   ShouldScaleUp = gini < 0.3
   -> dotnet build -> 0 Error(s)

3. Изменить E07StructuralContextToken.cs — три подшага:
   3а. В MultidimensionalNGram: добавить _mostFrequentNext поле + его вычисление в Train()
   3б. В BigramFallback: return bpe -> return _mostFrequentNext
   3в. В baseline evaluation loop: добавить baselineFallback и убрать if-skip
   -> dotnet build -> 0 Error(s)

4. Запустить тесты:
   dotnet test
   Ожидаем: 0 failures (существующие тесты не должны упасть)

5. Запустить все эксперименты:
   runner.RunAll()

6. Проверить результаты по каждому:

   E04: baseline_value должна стать 0.0
        enhanced_value должна быть > 0 (BM25 находит файлы)
        improvement_pct > 0, passed = true
        Если enhanced тоже близко к 0 — проверить что ForE04 возвращает queries

   E06: Tier 1 должен показать SCALE_UP_DATA_TOO_SMALL (gini=0.16 < 0.3)
        Лестница должна автоматически перейти на Tier 2
        На Tier 2 ожидаем gini > 0.3 и реальный результат

   E07: baseline и enhanced теперь оба предсказывают для ВСЕХ позиций
        Если enhanced > baseline — структурный контекст помогает на Tier 1
        Если enhanced ≈ baseline — нужен Tier 2 (больше данных для многомерной таблицы)
        Если ShouldScaleUp нужен для E07 — добавить: ShouldScaleUp = contextRatio > 0.8
        (contextRatio=0.588 означает 58% контекстов уникальны — почти все контексты видели один раз)
```

---

## Что НЕ трогать

- E01, E02, E03, E05 — все прошли, без изменений
- ExperimentRunner.cs, ResultTypes.cs — без изменений
- Core/NgramModel.cs — без изменений
- Infrastructure/* — без изменений

---

## Ожидаемый прогресс после исправления

```
SUMMARY -- auto tier
ID       Metric          Tier  Baseline  Enhanced  Delta%  Status
E01      perplexity_3g      1  7057795    2787    +100.0  PASSED
E02      top1_accuracy      3     0.54      0.66   +22.6  PASSED
E03      perplexity         3   244.95    171.14   +30.1  PASSED
E04      recall_at_3        2     0.00      0.6X   +∞     PASSED  <- было 0%
E05      top1_accuracy      1     0.51      0.66   +29.9  PASSED
E06      mrr                2        ?         ?      ?   ?       <- было FAILED, теперь Tier 2
E07      top1_accuracy      1        ?         ?      ?   ?       <- было -4.6% с багом
```

E04 и E07 должны дать реальные результаты.  
E06 дойдёт до Tier 2 где граф плотнее.

---

*Fix Plan v1.0 — E04 baseline + E06 scale_up + E07 fallback · не касается XIIGen*
