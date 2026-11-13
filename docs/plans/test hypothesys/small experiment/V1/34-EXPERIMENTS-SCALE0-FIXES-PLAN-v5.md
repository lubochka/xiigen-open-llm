# Experiments — План: два технических бага + Scale 2
## Исправления ResultWriter + запуск Scale 2 для E01 E02 E03 E06

**Проект:** llm-mvp-core — подпроект 34.Experiments  
**Этот документ не касается XIIGen.**

---

## Состояние: что работает, что не работает

**Работает:**
- JSON файлы — корректные, полные данные
- E01 Scale 0 и 1: +33% / +41.5% ✅
- E02 Scale 0 и 1: +11% / +13.4% ✅
- E03 Scale 0 и 1: +14% / +15.4% ✅
- E06 Scale 0 и 1: +30% / +60.5% ✅

**Не работает:**
- MD файлы все пустые (0 байт)
- progress.json содержит только последний запущенный эксперимент

---

## Баг 1 — Пустые MD файлы

**Файл:** `Infrastructure/ResultWriter.cs`, метод `BuildMarkdown()`

**Причина:** метод использует C# raw string literal (`$"""..."""`).  
Этот синтаксис появился в C# 11. В некоторых конфигурациях сборки на .NET 9  
raw string literal с интерполяцией внутри условного выражения  
`{(r.Passed ? "..." : "...")}` может возвращать пустую строку вместо содержимого.

**Диагностика:** `File.WriteAllText(path, BuildMarkdown(...))` записывает файл,  
JSON записывается правильно тем же методом — значит проблема именно в `BuildMarkdown`.  
Файлы созданы (не отсутствуют), но 0 байт — значит метод вернул пустую строку.

**Исправление:** переписать `BuildMarkdown` через `StringBuilder` вместо raw string literal.  
Это гарантированно работает на всех версиях C# и не зависит от конфигурации проекта.

```csharp
// Было (raw string literal — может дать пустую строку в некоторых сборках):
private static string BuildMarkdown(ExperimentResult r, DatasetContents d)
{
    return $"""
# {r.ExperimentId} — Scale {r.Scale}: {d.TierName}
...
""";
}

// Нужно (StringBuilder — надёжно всегда):
private static string BuildMarkdown(ExperimentResult r, DatasetContents d)
{
    var sb = new System.Text.StringBuilder();
    
    string verdict = r.Passed
        ? "✅ PASSED — PROMOTE_TO_NEXT_SCALE"
        : "❌ FAILED — ABANDON or TUNE";
    
    double baseAcc  = Math.Round(r.Baseline.AccuracyFraction * 100, 1);
    double enhAcc   = Math.Round(r.Enhanced.AccuracyFraction * 100, 1);
    double accDelta = Math.Round(r.AccuracyDeltaPct, 1);
    double timeSave = TimeSaved(r);
    double memSave  = MemorySaved(r);

    string accMark  = accDelta >= 5 ? "✅" : accDelta >= 0 ? "⚠️" : "❌";
    string timeMark = timeSave >= 5 ? "✅" : timeSave >= 0 ? "⚠️" : "❌";
    string memMark  = memSave  >= 5 ? "✅" : memSave  >= 0 ? "⚠️" : "❌";

    sb.AppendLine($"# {r.ExperimentId} — Scale {r.Scale}: {d.TierName}");
    sb.AppendLine($"**Run:** {DateTime.UtcNow:dd MMM yyyy HH:mm} UTC");
    sb.AppendLine();
    sb.AppendLine($"## Result: {verdict}");
    sb.AppendLine();
    sb.AppendLine("| Metric | Baseline | Enhanced | Δ |");
    sb.AppendLine("|---|---|---|---|");
    sb.AppendLine($"| Accuracy | {baseAcc}% | {enhAcc}% | {FormatDelta(accDelta)} {accMark} |");
    sb.AppendLine($"| Time (ms) | {Math.Round(r.Baseline.TimeMs, 0)} ms | {Math.Round(r.Enhanced.TimeMs, 0)} ms | {FormatSave(timeSave)} {timeMark} |");
    sb.AppendLine($"| Memory (MB) | {Math.Round(r.Baseline.MemoryMb, 1)} MB | {Math.Round(r.Enhanced.MemoryMb, 1)} MB | {FormatSave(memSave)} {memMark} |");
    sb.AppendLine();
    sb.AppendLine("## Dataset");
    sb.AppendLine($"- Tier: {d.TierName}");
    sb.AppendLine($"- Files: {d.Files.Count}");
    sb.AppendLine($"- Characters: {d.TotalChars:N0}");
    sb.AppendLine($"- Approx tokens: {d.ApproxTokens:N0}");
    sb.AppendLine();
    sb.AppendLine("## Notes");
    sb.AppendLine(r.Notes);
    sb.AppendLine();
    sb.AppendLine("## Next step");
    if (r.Passed)
        sb.AppendLine($"Promote {r.ExperimentId} to Scale {r.Scale + 1}");
    else
        sb.AppendLine($"Improvement {accDelta:+0.0;-0.0}% is below +5% threshold. Abandon or tune.");

    return sb.ToString();
}
```

**Gate B:** после исправления `RESULT-E01-scale0.md` содержит заголовок `# E01 — Scale 0`.

---

## Баг 2 — progress.json содержит только один эксперимент

**Файл:** `Infrastructure/ResultWriter.cs`, метод `UpdateProgress()`  
и `Infrastructure/ExperimentRunner.cs`, поле `_allResults`

**Причина:** `_allResults` — поле экземпляра `ExperimentRunner`.  
Если `Program.cs` создаёт новый `ExperimentRunner` для каждого эксперимента  
(или несколько последовательных запусков программы),  
каждый экземпляр начинает с пустым `_allResults`.  
`UpdateProgress(_allResults)` каждый раз перезаписывает `progress.json`  
со списком из одного элемента.

**Исправление:** `UpdateProgress` должна читать существующий `progress.json`,  
объединять с новыми результатами, затем записывать.  
Это работает независимо от числа экземпляров ExperimentRunner.

```csharp
// В ResultWriter.cs — новый метод MergeAndUpdateProgress:
public void UpdateProgress(IReadOnlyList<ExperimentResult> newResults)
{
    string progressPath = Path.Combine(_outputDir, "progress.json");

    // Шаг 1: прочитать существующий progress.json если он есть
    var existing = new List<(string Id, int Scale, double Delta, bool Passed, string NextAction)>();
    if (File.Exists(progressPath))
    {
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(File.ReadAllText(progressPath));
            if (doc.RootElement.TryGetProperty("experiments", out var exps))
            {
                foreach (var e in exps.EnumerateArray())
                {
                    existing.Add((
                        Id:         e.GetProperty("id").GetString()!,
                        Scale:      e.GetProperty("scale").GetInt32(),
                        Delta:      e.GetProperty("accuracy_delta_pct").GetDouble(),
                        Passed:     e.GetProperty("passed").GetBoolean(),
                        NextAction: e.GetProperty("next_action").GetString()!
                    ));
                }
            }
        }
        catch { /* если файл повреждён — начинаем с чистого листа */ }
    }

    // Шаг 2: убрать из существующих те что обновляются сейчас
    var newKeys = newResults.Select(r => (r.ExperimentId, r.Scale)).ToHashSet();
    existing.RemoveAll(e => newKeys.Contains((e.Id, e.Scale)));

    // Шаг 3: объединить и записать
    var allEntries = existing
        .Select(e => new { id = e.Id, scale = e.Scale, accuracy_delta_pct = e.Delta,
                           passed = e.Passed, next_action = e.NextAction })
        .Concat(newResults.Select(r => new
        {
            id                 = r.ExperimentId,
            scale              = r.Scale,
            accuracy_delta_pct = Math.Round(r.AccuracyDeltaPct, 2),
            passed             = r.Passed,
            next_action        = r.NextAction
        }))
        .OrderBy(e => e.id).ThenBy(e => e.scale)
        .ToList();

    var summary = new { updated_at = DateTime.UtcNow.ToString("o"), experiments = allEntries };

    File.WriteAllText(progressPath,
        System.Text.Json.JsonSerializer.Serialize(summary,
            new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

    // Аналогично для progress.md
    BuildProgressMarkdownFile(newResults, progressPath.Replace(".json", ".md"), existing);
}
```

**Gate B:** запустить E01 Scale 1 затем E03 Scale 1 двумя отдельными вызовами `runner.Run()`.  
В `progress.json` должны быть обе записи.

---

## Часть 2 — Scale 2 для E01, E02, E03, E06

Все четыре прошли Scale 0 и Scale 1. Теперь Scale 2.

**Scale 2 = подпроекты 00–08 (~30 543 BPE токена, 9 подпроектов):**
```
00.TensorCore      ~12 968 токенов
01.Tokenizer        ~1 566 токенов
02.DatasetPipeline  ~1 535 токенов
03.TransformerModel ~4 346 токенов
04.TrainingLoop     ~2 370 токенов
05.Embeddings       ~1 015 токенов
06.VectorSearch     ~2 699 токенов
07.GraphEngine      ~1 937 токенов
08.GraphRag         ~2 107 токенов
ИТОГО:             ~30 543 токена
```

**Точные команды для Claude Code:**
```csharp
var runner = new ExperimentRunner(
    projectRoot: @"путь\к\llm_mvp_core",
    outputDir:   @"experiments\results");

runner.Run("E01", scale: 2);
runner.Run("E02", scale: 2);
runner.Run("E03", scale: 2);
runner.Run("E06", scale: 2);
```

**Ожидаемые результаты Scale 2:**

| Эксп. | Scale 1 | Ожидание Scale 2 | Обоснование |
|---|---|---|---|
| E01 | +41.5% | +35–45% | K-Means на ~450 BPE токенах — кластеры стабильнее |
| E02 | +13.4% | +14–18% | Энтропия улучшается монотонно с данными |
| E03 | +15.4% | +15–20% | Тригамма стабильна, корпус плотнее |
| E06 | +60.5% | +55–65% | AST грамматика C# — результат должен быть стабильным |

**Критерий перехода:** accuracy_delta ≥ 5% для каждого → продвигаем на Scale 3.

---

## Порядок выполнения

```
Шаг 1: Исправить BuildMarkdown (StringBuilder) в ResultWriter.cs
        Gate B: RESULT-E01-scale0.md содержит заголовок

Шаг 2: Исправить UpdateProgress (чтение + слияние) в ResultWriter.cs
        Gate B: два запуска → оба в progress.json

Шаг 3: Запустить Scale 2 для E01 E02 E03 E06
        Gate B: все четыре RESULT-*-scale2.json существуют и корректны
```

---

## Сводная таблица изменений

| Файл | Что меняем | Строк |
|---|---|---|
| `Infrastructure/ResultWriter.cs` | `BuildMarkdown`: raw string → StringBuilder | ~25 |
| `Infrastructure/ResultWriter.cs` | `UpdateProgress`: читать + объединять существующий progress.json | ~35 |

Итого: 1 файл, ~60 строк.

---

## Оценка времени

| Задача | Дней |
|---|---|
| Баг 1 — BuildMarkdown | 0.5 |
| Баг 2 — UpdateProgress | 0.5 |
| Запуск Scale 2 | 0.5 |
| **Итого** | **1.5 дня** |

---

⛔ STOP — жду одобрения.

*Experiments Fix + Scale 2 Plan v1.0 — не касается XIIGen*
