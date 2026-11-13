# E02/E03 — Читаемые результаты (третье состояние)
## Fix Plan v1.0 · 4 файла · ~15 строк

**Проект:** llm-mvp-core — 35.ExperimentsV2  
**Этот документ не касается XIIGen.**

---

## Проблема

Сейчас `TUNE_OR_ABANDON` — единственный исход при delta=0%.
Нельзя понять: подход сломан, или просто данных мало.

E02: `confRatio=0%` → 27 вызовов, статистики нет → **нужно больше данных**  
E03: `optimalN=1` → биграмм хуже юниграмма → **нужно больше данных**

Оба должны говорить: `SCALE_UP` — не трогай, запускай на Scale 1.

---

## Изменение 1 — `Core/ResultTypes.cs`

Добавить одно свойство в `ExperimentV2Result` и изменить `NextAction`:

```csharp
// Добавить после строки 54 (после Passed):
// True когда delta=0 не потому что подход плохой, а потому что данных мало.
// Сигнал: запускать следующий Scale, не переделывать эксперимент.
public bool ShouldScaleUp { get; init; }

// Изменить строку 57 (NextAction):
// БЫЛО:
public string NextAction => Passed ? "PROMOTE_TO_NEXT_TIER" : "TUNE_OR_ABANDON";

// СТАЛО:
public string NextAction =>
    Passed       ? "PROMOTE_TO_NEXT_TIER" :
    ShouldScaleUp? "SCALE_UP_DATA_TOO_SMALL" :
                   "TUNE_OR_ABANDON";
```

---

## Изменение 2 — `Experiments/E02FrequencyRanking.cs`

В блоке создания `ExperimentV2Result` (строка ~84), добавить одну строку:

```csharp
return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
{
    ExperimentId      = "E02-FREQUENCY-RANKING",
    Tier              = tier,
    BaselinePrimary   = baselineTop1,
    EnhancedPrimary   = enhancedTop1,
    PrimaryMetricName = "top1_accuracy",
    LowerIsBetter     = false,
    ScalingSignal     = confRatio,
    ScalingSignalNote = "confidence_ratio (pairs with >=10 obs / total pairs)",
    BaselineTimeMs    = baselineMs,
    EnhancedTimeMs    = enhancedMs,
    Notes             = $"totalCalls={total}, uniqueReceivers={freqTable.Count}, " +
                        $"confRatio={confRatio:P1}",
    // Добавить эту строку:
    // confRatio=0 означает нет ни одной пары с >=10 наблюдениями — данных мало.
    ShouldScaleUp     = confRatio == 0
});
```

---

## Изменение 3 — `Experiments/E03NgramModel.cs`

В блоке создания `ExperimentV2Result` summary (строка ~113), добавить одну строку:

```csharp
var summary = new ExperimentV2Result
{
    ExperimentId      = "E03-NGRAM-MODEL",
    Tier              = tier,
    BaselinePrimary   = baselinePerp,
    EnhancedPrimary   = enhancedPerp,
    PrimaryMetricName = "perplexity",
    LowerIsBetter     = true,
    ScalingSignal     = optimalN,
    ScalingSignalNote = "optimal_n_this_tier (higher N needs more data)",
    BaselineTimeMs    = totalMs,
    EnhancedTimeMs    = totalMs,
    Notes             = $"optimalN={optimalN}, vocab={uni.VocabSize}, " +
                        $"tokenCount={tokens.Count}",
    // Добавить эту строку:
    // optimalN=1 означает биграмм хуже юниграмма — данных не хватает даже для N=2.
    ShouldScaleUp     = optimalN == 1
};
```

---

## Изменение 4 — `ExperimentRunner.cs`

Строка 42 (статус в консоли) и строка 102 (таблица):

```csharp
// Строка 42 — БЫЛО:
string status = r.Passed ? "PASSED" : "NOT PASSED";

// СТАЛО:
string status = r.Passed        ? "PASSED" :
                r.ShouldScaleUp ? "SCALE_UP" :
                                  "FAILED";
```

```csharp
// Строка 102 (в таблице RunAll) — БЫЛО:
$"{(v.Passed ? "PASSED" : "FAILED")}"

// СТАЛО:
$"{(v.Passed ? "PASSED" : v.ShouldScaleUp ? "SCALE_UP" : "FAILED")}"
```

Строка 64 (условие остановки в `RunTierLadder`):

```csharp
// БЫЛО:
if (!op.Value!.Passed)
{
    Console.WriteLine($"Лестница остановлена на Tier {tier}: улучшение < 5%.");
    return;
}

// СТАЛО:
if (!op.Value!.Passed)
{
    string reason = op.Value.ShouldScaleUp
        ? "данных мало — запустите Scale 1 вручную"
        : "улучшение < 5%, подход требует доработки";
    Console.WriteLine($"Лестница остановлена на Tier {tier}: {reason}.");
    return;
}
```

---

## Ожидаемый вывод после исправления

```
========================================================================
SUMMARY -- all experiments at Tier 1
------------------------------------------------------------------------
ID       Metric                   Baseline   Enhanced  Delta%  Status
------------------------------------------------------------------------
E01      perplexity_3gram       7057795.28   2787.49  +100.0  PASSED
E02      top1_accuracy               0.778      0.778    +0.0  SCALE_UP
E03      perplexity               1742.262   1742.262    +0.0  SCALE_UP
E04      NOTE: E04 requires Tier 2+...
E05      top1_accuracy               0.508      0.508    +0.0  FAILED
E06      mrr                         0.268      0.205   -23.6  FAILED
E07      top1_accuracy               0.51X      0.5XX   +X.X%  ...
------------------------------------------------------------------------
Passed: 1/6 at Tier 1
```

Теперь:
- `PASSED` = работает, поднять на следующий Scale
- `SCALE_UP` = алгоритм корректен, данных мало, запустить Scale 1
- `FAILED` = подход не работает или есть баг (E05 до исправления, E06)

---

## Порядок для Claude Code

```
1. Core/ResultTypes.cs         — добавить ShouldScaleUp + изменить NextAction
   -> dotnet build -> 0 Error(s)

2. Experiments/E02FrequencyRanking.cs — добавить ShouldScaleUp = confRatio == 0
   -> dotnet build -> 0 Error(s)

3. Experiments/E03NgramModel.cs       — добавить ShouldScaleUp = optimalN == 1
   -> dotnet build -> 0 Error(s)

4. ExperimentRunner.cs                — обновить статус в консоли и таблице
   -> dotnet build -> 0 Error(s)
   -> dotnet test  -> 0 failures
   -> runner.RunAll(tier: 1)
      Проверить: E02 показывает SCALE_UP, E03 показывает SCALE_UP
```

---

*Fix Plan v1.0 — E02/E03 legibility · не касается XIIGen*
