# RunAll — Auto-escalation Fix
## Fix Plan v1.0 · 1 файл · ExperimentRunner.cs

**Проблема:** `RunAll(tier:1)` гоняет все эксперименты на фиксированном Tier 1.
E02, E03 получают `SCALE_UP`. E04 сразу падает с "requires Tier 2+".
Пользователь видит пачку NOT PASSED и должен вручную перезапускать каждый.

**Решение:** `RunAll` вызывает `RunTierLadder` на каждый эксперимент.
Лестница сама доходит до первого значимого результата.
Требует плана SCALE_UP (ShouldScaleUp в ResultTypes) — применить его первым.

---

## Изменение 1 — `RunTierLadder`: продолжать при SCALE_UP и "Tier 2+"

```csharp
// БЫЛО:
public void RunTierLadder(string id)
{
    for (int tier = 1; tier <= 3; tier++)
    {
        Console.WriteLine();
        Console.WriteLine($"=== {id} -- Tier {tier} ===================================");
        var op = Run(id, tier);

        if (!op.IsSuccess)
        {
            Console.WriteLine($"Лестница остановлена: {op.ErrorMessage}");
            return;
        }
        if (!op.Value!.Passed)
        {
            Console.WriteLine($"Лестница остановлена на Tier {tier}: улучшение < 5%.");
            return;
        }
        if (tier == 3)
            Console.WriteLine($"ALL PASSED: {id} прошёл все три тира!");
    }
}

// СТАЛО:
public OperationResult<ExperimentV2Result> RunTierLadder(string id, int startTier = 1)
{
    OperationResult<ExperimentV2Result> last =
        OperationResult<ExperimentV2Result>.Fail("не запускался");

    for (int tier = startTier; tier <= 3; tier++)
    {
        Console.WriteLine();
        Console.WriteLine($"=== {id} -- Tier {tier} ===================================");
        var op = Run(id, tier);

        // Ошибка "нужен Tier 2+" — это не сбой кода, просто пропустить тир.
        if (!op.IsSuccess)
        {
            if (op.ErrorMessage.Contains("Tier 2+") || op.ErrorMessage.Contains("Tier 2"))
            {
                Console.WriteLine($"  -> пропуск Tier {tier}: {op.ErrorMessage}");
                last = op;
                continue;
            }
            Console.WriteLine($"Лестница остановлена: {op.ErrorMessage}");
            return op;
        }

        last = op;
        var r = op.Value!;

        // SCALE_UP = данных мало, подняться выше.
        if (r.ShouldScaleUp)
        {
            Console.WriteLine($"  -> SCALE_UP: данных мало на Tier {tier}, переходим на Tier {tier + 1}");
            continue;
        }

        // PASSED = успех.
        if (r.Passed)
        {
            if (tier == 3) Console.WriteLine($"ALL PASSED: {id} прошёл все три тира!");
            return op;
        }

        // FAILED = подход не работает, остановить.
        Console.WriteLine($"  -> FAILED на Tier {tier}: улучшение {r.ImprovementPct:+0.0;-0.0}%. Остановка.");
        return op;
    }

    return last;
}
```

Изменения по сравнению с оригиналом:
- Возвращает `OperationResult<ExperimentV2Result>` (нужно для RunAll)
- Добавлен параметр `startTier` (E04 можно вызвать с `startTier:2`)
- `!op.IsSuccess` с "Tier 2+" → `continue` вместо `return`
- `ShouldScaleUp` → `continue` вместо `return`
- `!Passed && !ShouldScaleUp` → `return` (настоящий провал)

---

## Изменение 2 — `RunAll`: вызывать `RunTierLadder`, убрать фиксированный tier

```csharp
// БЫЛО:
public void RunAll(int tier)
{
    string[] ids = ["E01", "E02", "E03", "E04", "E05", "E06"];
    var results  = new List<(string Id, OperationResult<ExperimentV2Result> R)>();

    foreach (var id in ids)
    {
        Console.WriteLine();
        results.Add((id, Run(id, tier)));
    }

    // ... сводная таблица с фиксированным tier в заголовке ...
    Console.WriteLine($"SUMMARY -- all experiments at Tier {tier}");
    // ... без колонки Tier в таблице ...
    Console.WriteLine($"Passed: {passed}/{ids.Length} at Tier {tier}");
}

// СТАЛО:
public void RunAll()
{
    string[] ids = ["E01", "E02", "E03", "E04", "E05", "E06", "E07"];
    var results  = new List<(string Id, OperationResult<ExperimentV2Result> R)>();

    foreach (var id in ids)
    {
        Console.WriteLine();
        // Каждый эксперимент сам находит свой минимальный рабочий тир.
        // E04 начинает с Tier 2 потому что Tier 1 заведомо неприменим.
        int startTier = id == "E04" ? 2 : 1;
        results.Add((id, RunTierLadder(id, startTier)));
    }

    Console.WriteLine();
    Console.WriteLine(new string('=', 78));
    Console.WriteLine("SUMMARY -- auto tier (each experiment found its minimum viable tier)");
    Console.WriteLine(new string('-', 78));
    // Добавлена колонка Tier
    Console.WriteLine($"{"ID",-8} {"Metric",-22} {"Tier",4} {"Baseline",10} {"Enhanced",10} {"Delta%",7}  Status");
    Console.WriteLine(new string('-', 78));

    foreach (var (id, op) in results)
    {
        if (!op.IsSuccess)
        {
            Console.WriteLine($"{id,-8} {"--",-22} {"--",4} {"--",10} {"--",10} {"--",7}  ERROR: {op.ErrorMessage}");
            continue;
        }
        var v = op.Value!;
        string status = v.Passed ? "PASSED" : v.ShouldScaleUp ? "SCALE_UP" : "FAILED";
        Console.WriteLine($"{id,-8} {v.PrimaryMetricName,-22} {v.Tier,4} {v.BaselinePrimary,10:F3} " +
                          $"{v.EnhancedPrimary,10:F3} {v.ImprovementPct,7:+0.0;-0.0}  {status}");
    }

    Console.WriteLine(new string('-', 78));
    int passed  = results.Count(r => r.R.IsSuccess && r.R.Value!.Passed);
    int scaleUp = results.Count(r => r.R.IsSuccess && r.R.Value!.ShouldScaleUp);
    int failed  = results.Count(r => r.R.IsSuccess && !r.R.Value!.Passed && !r.R.Value!.ShouldScaleUp);
    Console.WriteLine($"PASSED: {passed}  SCALE_UP: {scaleUp}  FAILED: {failed}  из {ids.Length}");
    Console.WriteLine($"Результаты: {_outputRoot}");
    Console.WriteLine($"Сводка: {Path.Combine(_outputRoot, "progress", "progress.md")}");
    Console.WriteLine(new string('=', 78));
}
```

Изменения по сравнению с оригиналом:
- Убран параметр `int tier` — больше не нужен (каждый тир находит себя сам)
- `Run(id, tier)` → `RunTierLadder(id, startTier)`
- E04 стартует с `startTier:2`
- E07 добавлен в массив
- Таблица получила колонку `Tier` — видно на каком тире каждый прошёл
- Итоговая строка: `PASSED: N  SCALE_UP: N  FAILED: N`

---

## Ожидаемый вывод после обоих исправлений

```
=== E02 -- Tier 1 ===================================
[E02] Tier 1 загрузка... 3 файлов...
[E02] Запуск... OK top1_accuracy (up) ... delta=+0.0% | SCALE_UP
  -> SCALE_UP: данных мало на Tier 1, переходим на Tier 2

=== E02 -- Tier 2 ===================================
[E02] Tier 2 загрузка... 47 файлов...
[E02] Запуск... OK top1_accuracy (up) ... delta=+34.2% | PASSED
  -> .../E02-FREQUENCY-RANKING/results/run-2026-05-01-002.json

...

==============================================================================
SUMMARY -- auto tier (each experiment found its minimum viable tier)
------------------------------------------------------------------------------
ID       Metric                 Tier   Baseline   Enhanced  Delta%  Status
------------------------------------------------------------------------------
E01      perplexity_3gram          1 7057795.285   2787.487  +100.0  PASSED
E02      top1_accuracy             2      0.423      0.567   +34.2  PASSED
E03      perplexity                2   1742.262    412.115   +76.4  PASSED
E04      top1_accuracy             2      0.345      0.412   +19.4  PASSED
E05      top1_accuracy             1      0.508      0.508    +0.0  FAILED
E06      mrr                       1      0.268      0.205   -23.6  FAILED
E07      top1_accuracy             1      0.51X      0.5XX   +X.X%  ...
------------------------------------------------------------------------------
PASSED: 4  SCALE_UP: 0  FAILED: 2  из 7
```

Числа для E02-E04 на Tier 2 — примерные, реальные будут после запуска.

---

## Порядок для Claude Code

```
Предварительно: применить план SCALE_UP (ShouldScaleUp в ResultTypes) — обязательно.

1. ExperimentRunner.cs — изменить RunTierLadder (сигнатура + логика continue)
   -> dotnet build -> 0 Error(s)

2. ExperimentRunner.cs — изменить RunAll (убрать tier, вызвать RunTierLadder)
   -> dotnet build -> 0 Error(s)
   -> dotnet test  -> 0 failures

3. runner.RunAll()
   Проверить:
   - E02 прошёл через Tier 1 (SCALE_UP) → Tier 2
   - E03 прошёл через Tier 1 (SCALE_UP) → Tier 2
   - E04 пропустил Tier 1 → стартовал с Tier 2
   - Таблица содержит колонку Tier
   - E07 в таблице
   - Итоговая строка: PASSED / SCALE_UP / FAILED отдельно
```

---

## Что НЕ трогать

Все файлы кроме `ExperimentRunner.cs`.
ResultTypes, эксперименты E01-E07, NgramModel — без изменений.

---

*Fix Plan v1.0 — RunAll auto-escalation · не касается XIIGen*
