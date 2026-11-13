# 35.ExperimentsV2 — Infrastructure Plan v1.1
## Инструкция для Claude Code: написать, проверить, запустить, показать результаты

**Проект:** llm-mvp-core — подпроект 35.ExperimentsV2  
**Основа:** план v1.0 (одобрен Luba)  
**Этот документ не касается XIIGen.**

---

## Контекст: что уже есть, что сломано

### Что есть и не трогать

| Файл | Что делает |
|---|---|
| `Core/NgramModel.cs` | Утилиты N-грамм, `TrigramModelV2`, `NgramUtils` |
| `Core/ResultTypes.cs` | `OperationResult<T>`, `ExperimentV2Result`, `NgramResultEntry` |
| `Experiments/E01AstTokenizer.cs` | Алгоритм E01 |
| `Experiments/E02FrequencyRanking.cs` | Алгоритм E02 |
| `Experiments/E03NgramModel.cs` | Алгоритм E03 |
| `Experiments/E04Bm25Context.cs` | Алгоритм E04 |
| `Experiments/E05SyntaxFilter.cs` | Алгоритм E05 |
| `Experiments/E06PageRank.cs` | Алгоритм E06 |
| `ExperimentsV2Dispatcher.cs` | Диспетчер |

### Что сломано (4 проблемы)

1. **Нет загрузки файлов** — эксперименты принимают готовые данные, но никто их не готовит
2. **Разные форматы входа** — E01/E05 нужна строка, E02 — список строк, E03 — список int, E04 — пары (документ+запрос), E06 — рёбра графа
3. **Нет вывода в файлы** — `ExperimentV2Result` остаётся в памяти, ни один файл не пишется
4. **`Tier = 1` захардкодено в шести файлах** — строки E01:88, E02:87, E03:116, E04:93, E05:99, E06:111

### Что добавить

```
35.ExperimentsV2/
├── Infrastructure/
│   ├── DatasetLoader.cs     <- Фаза 1: читает .cs файлы по тирам
│   ├── InputAdapters.cs     <- Фаза 2: готовит нужный формат для каждого эксперимента
│   └── ResultWriter.cs      <- Фаза 3: пишет JSON + MD + progress
└── ExperimentRunner.cs      <- Фаза 4: связывает всё вместе
```

Плюс патч 6 строк `Tier = 1` -> `Tier = tier` и обновление Dispatcher.

---

## Фаза 1 — DatasetLoader.cs

### Написать файл `35.ExperimentsV2/Infrastructure/DatasetLoader.cs`

```csharp
using System.Text;

namespace ExperimentsV2;

// DatasetLoader читает .cs файлы из проекта llm_mvp_core по тирам.
// Tier 1: только 01.Tokenizer (~3 файла, ~2 000 токенов).
// Tier 2: шесть малых подпроектов (~47 файлов, ~9 100 токенов).
// Tier 3: все 24 подпроекта (~195 файлов, ~141 000 токенов).
public static class DatasetLoader
{
    private static readonly string[] Tier1 =
        ["01.Tokenizer"];

    private static readonly string[] Tier2 =
        ["01.Tokenizer", "02.DatasetPipeline",
         "05.Embeddings", "07.GraphEngine", "09.Inference", "10.LoRA"];

    // projectRoot — абсолютный путь к папке где лежат 01.Tokenizer, 03.TransformerModel и т.д.
    public static OperationResult<DatasetContents> Load(int tier, string projectRoot)
    {
        if (!Directory.Exists(projectRoot))
            return OperationResult<DatasetContents>.Fail(
                $"Папка не найдена: {projectRoot}");

        var folders = ResolveFolders(tier, projectRoot);
        if (folders.Count == 0)
            return OperationResult<DatasetContents>.Fail(
                $"Нет папок для тира {tier} в {projectRoot}");

        var files = new List<(string Path, string Text)>();
        foreach (var folder in folders)
        {
            if (!Directory.Exists(folder)) continue;
            foreach (var f in Directory.GetFiles(folder, "*.cs", SearchOption.AllDirectories))
            {
                if (f.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar)) continue;
                if (f.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar)) continue;
                files.Add((f, File.ReadAllText(f, Encoding.UTF8)));
            }
        }

        if (files.Count == 0)
            return OperationResult<DatasetContents>.Fail(
                $"Нет .cs файлов для тира {tier} в {projectRoot}");

        string combined = string.Join("\n\n", files.Select(f => f.Text));

        return OperationResult<DatasetContents>.Ok(new DatasetContents
        {
            Tier         = tier,
            ProjectRoot  = projectRoot,
            Files        = files.AsReadOnly(),
            CombinedText = combined
        });
    }

    private static List<string> ResolveFolders(int tier, string root) => tier switch
    {
        1 => Tier1.Select(f => Path.Combine(root, f)).ToList(),
        2 => Tier2.Select(f => Path.Combine(root, f)).ToList(),
        _ => Directory.GetDirectories(root)
                 .Where(d =>
                 {
                     string name = Path.GetFileName(d);
                     return name.Length >= 2 && char.IsDigit(name[0]) && char.IsDigit(name[1]);
                 })
                 .OrderBy(d => d)
                 .ToList()
    };
}

public sealed record DatasetContents
{
    public required int    Tier         { get; init; }
    public required string ProjectRoot  { get; init; }
    public required IReadOnlyList<(string Path, string Text)> Files { get; init; }
    public required string CombinedText { get; init; }
    public int FileCount => Files.Count;
}
```

### Gate B — Фаза 1: как проверить

**Команда:**
```
dotnet build 35.ExperimentsV2/35.ExperimentsV2.csproj
```

**Ожидаемый вывод:**
```
Build succeeded.
0 Error(s)
0 Warning(s)
```

Если ошибки — исправить до перехода к Фазе 2.

**Быстрая проверка логики:**
```csharp
// Подставить реальный путь к корню llm_mvp_core:
string root = @"/absolute/path/to/llm_mvp_core";

var t1 = DatasetLoader.Load(1, root);
Console.WriteLine($"Tier 1: {t1.IsSuccess}, файлов={t1.Value?.FileCount}");
// Ожидаем: Tier 1: True, файлов=3 (или больше)

var t2 = DatasetLoader.Load(2, root);
Console.WriteLine($"Tier 2: {t2.IsSuccess}, файлов={t2.Value?.FileCount}");
// Ожидаем: Tier 2: True, файлов=47 (или больше)

var bad = DatasetLoader.Load(1, "/not/a/path");
Console.WriteLine($"Bad path: {bad.IsSuccess}, ошибка='{bad.ErrorMessage}'");
// Ожидаем: Bad path: False, ошибка='Папка не найдена: /not/a/path'
```

---

## Фаза 2 — InputAdapters.cs

### Написать файл `35.ExperimentsV2/Infrastructure/InputAdapters.cs`

```csharp
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Tokenizer;

namespace ExperimentsV2;

// InputAdapters преобразует DatasetContents в формат который ожидает каждый эксперимент.
// Каждый метод — одна строка на стороне вызывающего:
//   var input = InputAdapters.ForE01(dataset);
public static class InputAdapters
{
    // E01 и E05 принимают весь код проекта как одну строку.
    public static string ForE01(DatasetContents d) => d.CombinedText;
    public static string ForE05(DatasetContents d) => d.CombinedText;

    // E02 принимает список строк — по одной на файл.
    public static IReadOnlyList<string> ForE02(DatasetContents d)
        => d.Files.Select(f => f.Text).ToList().AsReadOnly();

    // E03 принимает BPE-токены всего корпуса.
    public static OperationResult<IReadOnlyList<int>> ForE03(DatasetContents d)
    {
        if (string.IsNullOrWhiteSpace(d.CombinedText))
            return OperationResult<IReadOnlyList<int>>.Fail("Пустой текст для E03");

        var tok = new BpeTokenizer();
        tok.Train(d.CombinedText, vocabSize: 512);
        var tokens = tok.Encode(d.CombinedText);

        return tokens.Count < 10
            ? OperationResult<IReadOnlyList<int>>.Fail("Слишком мало токенов для E03")
            : OperationResult<IReadOnlyList<int>>.Ok(tokens.AsReadOnly());
    }

    // E04 принимает документы + кросс-файловые запросы.
    // На Tier 1 (<20 файлов) возвращает Fail с объяснением — это ожидаемое поведение.
    public static OperationResult<(
        IReadOnlyList<(string Id, string Source)> Documents,
        IReadOnlyList<(string Query, string ExpectedId)> Queries)>
        ForE04(DatasetContents d)
    {
        if (d.FileCount < 20)
            return OperationResult<(IReadOnlyList<(string, string)>,
                                    IReadOnlyList<(string, string)>)>
                .Fail($"E04 требует Tier 2+ (>= 20 файлов). Сейчас Tier {d.Tier} = {d.FileCount} файлов. " +
                      $"Запустите снова с tier: 2.");

        var documents = d.Files
            .Select(f => (Path.GetFileNameWithoutExtension(f.Path), f.Text))
            .ToList();

        var queries = BuildCrossFileQueries(d.Files);

        if (queries.Count == 0)
            return OperationResult<(IReadOnlyList<(string, string)>,
                                    IReadOnlyList<(string, string)>)>
                .Fail("Не найдено кросс-файловых вызовов. E04 нужны зависимости между файлами.");

        return OperationResult<(IReadOnlyList<(string, string)>,
                                IReadOnlyList<(string, string)>)>
            .Ok((documents.AsReadOnly(), queries.AsReadOnly()));
    }

    // E06 принимает рёбра графа вызовов (caller, callee).
    public static OperationResult<IReadOnlyList<(string Caller, string Callee)>>
        ForE06(DatasetContents d)
    {
        var pairs = new List<(string, string)>();

        foreach (var (_, source) in d.Files)
        {
            var root = CSharpSyntaxTree.ParseText(source).GetRoot();
            foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                string caller = method.Identifier.Text;
                foreach (var inv in method.DescendantNodes().OfType<InvocationExpressionSyntax>())
                {
                    string callee = inv.Expression switch
                    {
                        MemberAccessExpressionSyntax ma => ma.Name.Identifier.Text,
                        IdentifierNameSyntax id         => id.Identifier.Text,
                        _                               => string.Empty
                    };
                    if (!string.IsNullOrEmpty(callee) && callee != caller)
                        pairs.Add((caller, callee));
                }
            }
        }

        return pairs.Count == 0
            ? OperationResult<IReadOnlyList<(string, string)>>.Fail("Граф вызовов пуст")
            : OperationResult<IReadOnlyList<(string, string)>>.Ok(pairs.AsReadOnly());
    }

    private static List<(string Query, string ExpectedId)> BuildCrossFileQueries(
        IReadOnlyList<(string Path, string Text)> files)
    {
        var defs = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (path, src) in files)
        {
            string fileId = Path.GetFileNameWithoutExtension(path);
            var root = CSharpSyntaxTree.ParseText(src).GetRoot();
            foreach (var m in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
                if (m.Modifiers.Any(mod => mod.IsKind(SyntaxKind.PublicKeyword)))
                    defs.TryAdd(m.Identifier.Text, fileId);
        }

        var queries = new List<(string, string)>();
        foreach (var (path, src) in files)
        {
            string currentId = Path.GetFileNameWithoutExtension(path);
            var root = CSharpSyntaxTree.ParseText(src).GetRoot();
            foreach (var inv in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                string callee = inv.Expression switch
                {
                    MemberAccessExpressionSyntax ma => ma.Name.Identifier.Text,
                    IdentifierNameSyntax id         => id.Identifier.Text,
                    _                               => string.Empty
                };
                if (defs.TryGetValue(callee, out string? defFile) && defFile != currentId)
                {
                    var entry = (callee, defFile);
                    if (!queries.Contains(entry)) queries.Add(entry);
                }
            }
        }
        return queries;
    }
}
```

### Gate B — Фаза 2: как проверить

**Команда:**
```
dotnet build 35.ExperimentsV2/35.ExperimentsV2.csproj
```
**Ожидаемый вывод:** `Build succeeded. 0 Error(s)`

**Проверка E04 Tier 1 -> Fail (обязательно!):**
```csharp
string root = @"/absolute/path/to/llm_mvp_core";
var ds1 = DatasetLoader.Load(1, root).Value!;

var e04fail = InputAdapters.ForE04(ds1);
Console.WriteLine($"E04 Tier1: IsSuccess={e04fail.IsSuccess}");
Console.WriteLine($"Сообщение: {e04fail.ErrorMessage}");
// Ожидаем:
// E04 Tier1: IsSuccess=False
// Сообщение: E04 требует Tier 2+ (>=20 файлов). Сейчас Tier 1 = 3 файлов. Запустите снова с tier: 2.

var ds2 = DatasetLoader.Load(2, root).Value!;
var e04ok = InputAdapters.ForE04(ds2);
Console.WriteLine($"E04 Tier2: IsSuccess={e04ok.IsSuccess}, docs={e04ok.Value?.Documents.Count}");
// Ожидаем: E04 Tier2: IsSuccess=True, docs=47 (или больше)
```

---

## Фаза 3 — ResultWriter.cs

### Написать файл `35.ExperimentsV2/Infrastructure/ResultWriter.cs`

```csharp
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ExperimentsV2;

// ResultWriter записывает результат каждого прогона в два файла:
//   experiments/E01-AST-TOKENIZER/results/run-YYYY-MM-DD-001.json
//   experiments/E01-AST-TOKENIZER/results/run-YYYY-MM-DD-001.md
// И обновляет сводку в experiments/progress/progress.json + progress.md.
//
// ПРАВИЛО: JSON пишется ДО MD, MD пишется ДО progress (правило 8 — хранить до события).
public static class ResultWriter
{
    private static readonly JsonSerializerOptions Opts = new()
    {
        WriteIndented          = true,
        PropertyNamingPolicy   = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    // outputRoot — папка experiments/ (будет создана если не существует).
    // Возвращает путь к записанному JSON файлу.
    public static OperationResult<string> Write(ExperimentV2Result r, string outputRoot)
    {
        try
        {
            string dir  = Path.Combine(outputRoot, FolderName(r.ExperimentId), "results");
            Directory.CreateDirectory(dir);

            string date    = DateTime.UtcNow.ToString("yyyy-MM-dd");
            int    seq     = NextSeq(dir, date);
            string baseName = $"run-{date}-{seq:D3}";
            string jsonPath = Path.Combine(dir, baseName + ".json");
            string mdPath   = Path.Combine(dir, baseName + ".md");

            // Правило 8: сначала JSON, потом MD, потом progress.
            File.WriteAllText(jsonPath, ToJson(r),              Encoding.UTF8);
            File.WriteAllText(mdPath,  ToMarkdown(r, baseName), Encoding.UTF8);
            WriteProgress(r, outputRoot);

            return OperationResult<string>.Ok(jsonPath);
        }
        catch (Exception ex)
        {
            return OperationResult<string>.Fail($"ResultWriter: {ex.Message}");
        }
    }

    private static string ToJson(ExperimentV2Result r)
    {
        var obj = new
        {
            experiment_id       = r.ExperimentId,
            tier                = r.Tier,
            run_at              = DateTime.UtcNow.ToString("O"),
            primary_metric      = r.PrimaryMetricName,
            lower_is_better     = r.LowerIsBetter,
            baseline_value      = Math.Round(r.BaselinePrimary, 4),
            enhanced_value      = Math.Round(r.EnhancedPrimary, 4),
            improvement_pct     = Math.Round(r.ImprovementPct,  2),
            passed              = r.Passed,
            next_action         = r.NextAction,
            scaling_signal      = Math.Round(r.ScalingSignal,   4),
            scaling_signal_note = r.ScalingSignalNote,
            baseline_time_ms    = Math.Round(r.BaselineTimeMs,  1),
            enhanced_time_ms    = Math.Round(r.EnhancedTimeMs,  1),
            notes               = r.Notes
        };
        return JsonSerializer.Serialize(obj, Opts);
    }

    private static string ToMarkdown(ExperimentV2Result r, string runName)
    {
        string arrow = r.LowerIsBetter ? "down" : "up";
        string delta = r.ImprovementPct >= 5.0
            ? $"**+{r.ImprovementPct:F1}% OK**"
            : r.ImprovementPct >= 0 ? $"+{r.ImprovementPct:F1}% low"
                                    : $"{r.ImprovementPct:F1}% worse";

        return new StringBuilder()
            .AppendLine($"# {r.ExperimentId} - Tier {r.Tier} - {runName}")
            .AppendLine($"Run at: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC")
            .AppendLine()
            .AppendLine($"## Result: {(r.Passed ? "PASSED" : "NOT PASSED")}")
            .AppendLine()
            .AppendLine($"| Metric | Baseline | Enhanced | Delta | >= 5%? |")
            .AppendLine($"|--------|----------|----------|-------|--------|")
            .AppendLine($"| {r.PrimaryMetricName} ({arrow}) | {r.BaselinePrimary:F4} | {r.EnhancedPrimary:F4} | {delta} | {(r.Passed ? "YES" : "NO")} |")
            .AppendLine($"| Baseline time | {r.BaselineTimeMs:F0} ms | - | - | - |")
            .AppendLine($"| Enhanced time | - | {r.EnhancedTimeMs:F0} ms | - | - |")
            .AppendLine()
            .AppendLine($"## Scaling signal")
            .AppendLine($"Value: {r.ScalingSignal:F4} -- {r.ScalingSignalNote}")
            .AppendLine()
            .AppendLine($"## Notes")
            .AppendLine(string.IsNullOrWhiteSpace(r.Notes) ? "none" : r.Notes)
            .AppendLine()
            .AppendLine($"## Next step")
            .AppendLine(r.Passed
                ? $"Run Tier {r.Tier + 1}: runner.Run(\"{r.ExperimentId.Split('-')[0]}\", tier: {r.Tier + 1})"
                : "Improvement below 5%. Tune or skip to next experiment.")
            .ToString();
    }

    private static void WriteProgress(ExperimentV2Result r, string outputRoot)
    {
        string dir  = Path.Combine(outputRoot, "progress");
        Directory.CreateDirectory(dir);
        string jsonPath = Path.Combine(dir, "progress.json");

        Dictionary<string, object> progress;
        try
        {
            progress = File.Exists(jsonPath)
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(jsonPath)) ?? new()
                : new();
        }
        catch { progress = new(); }

        progress[r.ExperimentId] = new
        {
            experiment_id   = r.ExperimentId,
            tier            = r.Tier,
            passed          = r.Passed,
            improvement_pct = Math.Round(r.ImprovementPct, 2),
            next_action     = r.NextAction,
            scaling_signal  = Math.Round(r.ScalingSignal, 4),
            last_run        = DateTime.UtcNow.ToString("O")
        };

        File.WriteAllText(jsonPath, JsonSerializer.Serialize(progress, Opts), Encoding.UTF8);

        var sb = new StringBuilder();
        sb.AppendLine("# Experiment progress");
        sb.AppendLine($"Updated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC");
        sb.AppendLine();
        sb.AppendLine("| Experiment | Passed | Tier | Delta% | Signal | Next |");
        sb.AppendLine("|------------|--------|------|--------|--------|------|");

        foreach (var (id, _) in progress.OrderBy(kv => kv.Key))
        {
            var entry = progress[id];
            string entryJson = JsonSerializer.Serialize(entry);
            using var doc = JsonDocument.Parse(entryJson);
            var el = doc.RootElement;
            bool p   = el.TryGetProperty("passed", out var pv)          && pv.GetBoolean();
            int  t   = el.TryGetProperty("tier", out var tv)             ? tv.GetInt32()    : 0;
            double i = el.TryGetProperty("improvement_pct", out var iv)  ? iv.GetDouble()   : 0;
            double s = el.TryGetProperty("scaling_signal", out var sv)   ? sv.GetDouble()   : 0;
            string n = el.TryGetProperty("next_action", out var nv)      ? nv.GetString()!  : "";
            sb.AppendLine($"| {id} | {(p ? "YES" : "NO")} | {t} | {i:+0.0;-0.0}% | {s:F3} | {n} |");
        }

        File.WriteAllText(Path.Combine(dir, "progress.md"), sb.ToString(), Encoding.UTF8);
    }

    private static string FolderName(string id) => id.ToUpperInvariant() switch
    {
        "E01-AST-TOKENIZER"    => "E01-AST-TOKENIZER",
        "E02-FREQUENCY-RANKING"=> "E02-FREQUENCY-RANKING",
        "E03-NGRAM-MODEL"      => "E03-NGRAM-MODEL",
        "E04-BM25-CONTEXT"     => "E04-BM25-CONTEXT",
        "E05-SYNTAX-FILTER"    => "E05-SYNTAX-FILTER",
        "E06-PAGERANK-RANKING" => "E06-PAGERANK-RANKING",
        _                      => id
    };

    private static int NextSeq(string dir, string date)
    {
        int max = Directory.GetFiles(dir, $"run-{date}-*.json")
            .Select(f => Path.GetFileNameWithoutExtension(f).Split('-').LastOrDefault())
            .Select(s => int.TryParse(s, out int v) ? v : 0)
            .DefaultIfEmpty(0).Max();
        return max + 1;
    }
}
```

### Gate B — Фаза 3: как проверить

**Команда:**
```
dotnet build 35.ExperimentsV2/35.ExperimentsV2.csproj
```
**Ожидаемый вывод:** `Build succeeded. 0 Error(s)`

**Проверка что файлы создаются:**
```csharp
var fakeResult = new ExperimentV2Result
{
    ExperimentId      = "E01-AST-TOKENIZER",
    Tier              = 1,
    BaselinePrimary   = 185.23,
    EnhancedPrimary   = 42.17,
    PrimaryMetricName = "perplexity_3gram",
    LowerIsBetter     = true,
    ScalingSignal     = 0.038,
    ScalingSignalNote = "ast_saturation_ratio",
    BaselineTimeMs    = 45,
    EnhancedTimeMs    = 51,
    Notes             = "test"
};

string outputRoot = Path.Combine(Path.GetTempPath(), "expv2_gate_b");
var writeOp = ResultWriter.Write(fakeResult, outputRoot);

Console.WriteLine($"Write OK: {writeOp.IsSuccess}");       // True
Console.WriteLine($"JSON path: {writeOp.Value}");          // полный путь
Console.WriteLine($"JSON exists: {File.Exists(writeOp.Value)}"); // True

string mdPath = writeOp.Value!.Replace(".json", ".md");
Console.WriteLine($"MD exists: {File.Exists(mdPath)}");    // True

// Проверить содержимое JSON
var doc = JsonDocument.Parse(File.ReadAllText(writeOp.Value!));
Console.WriteLine($"improvement_pct: {doc.RootElement.GetProperty("improvement_pct").GetDouble()}");
// Ожидаем: improvement_pct: 77.23
Console.WriteLine($"passed: {doc.RootElement.GetProperty("passed").GetBoolean()}");
// Ожидаем: passed: True
Console.WriteLine($"next_action: {doc.RootElement.GetProperty("next_action").GetString()}");
// Ожидаем: next_action: PROMOTE_TO_NEXT_TIER

// Проверить progress файлы
string progressJson = Path.Combine(outputRoot, "progress", "progress.json");
string progressMd   = Path.Combine(outputRoot, "progress", "progress.md");
Console.WriteLine($"progress.json: {File.Exists(progressJson)}");  // True
Console.WriteLine($"progress.md: {File.Exists(progressMd)}");      // True

// Показать MD файл
Console.WriteLine(File.ReadAllText(mdPath));

// Очистить
Directory.Delete(outputRoot, recursive: true);
```

**Ожидаемое содержимое MD файла:**
```
# E01-AST-TOKENIZER - Tier 1 - run-2026-05-01-001
Run at: 2026-05-01 10:00 UTC

## Result: PASSED

| Metric | Baseline | Enhanced | Delta | >= 5%? |
|--------|----------|----------|-------|--------|
| perplexity_3gram (down) | 185.2300 | 42.1700 | **+77.2% OK** | YES |
| Baseline time | 45 ms | - | - | - |
| Enhanced time | - | 51 ms | - | - |

## Scaling signal
Value: 0.0380 -- ast_saturation_ratio

## Next step
Run Tier 2: runner.Run("E01", tier: 2)
```

---

## Фаза 4 — Патч Tier=1 + ExperimentRunner.cs

### Сначала патч: шесть строк в шести файлах

Добавить параметр `int tier = 1` последним в каждый `Run(...)` метод.
Изменить `Tier = 1` на `Tier = tier` в record-инициализаторе.

**E01 — `Experiments/E01AstTokenizer.cs`**
- Строка ~25: `public OperationResult<ExperimentV2Result> Run(string csharpSource, int bpeVocabSize = 512)` -> добавить `, int tier = 1`
- Строка 88: `Tier = 1,` -> `Tier = tier,`

**E02 — `Experiments/E02FrequencyRanking.cs`**
- Строка 23: `public OperationResult<ExperimentV2Result> Run(IReadOnlyList<string> sources)` -> добавить `, int tier = 1`
- Строка 87: `Tier = 1,` -> `Tier = tier,`

**E03 — `Experiments/E03NgramModel.cs`**
- Найти метод `Run(IReadOnlyList<int> tokens` -> добавить `, int tier = 1`
- Строка 116: `Tier = 1,` -> `Tier = tier,`

**E04 — `Experiments/E04Bm25Context.cs`**
- Строка ~27: `public OperationResult<ExperimentV2Result> Run(IReadOnlyList<(string Id, string Source)> documents, IReadOnlyList<(string Query, string ExpectedId)> queries)` -> добавить `, int tier = 1`
- Строка 93: `Tier = 1,` -> `Tier = tier,`

**E05 — `Experiments/E05SyntaxFilter.cs`**
- Строка 24: `public OperationResult<ExperimentV2Result> Run(string csharpSource)` -> добавить `, int tier = 1`
- Строка 99: `Tier = 1,` -> `Tier = tier,`

**E06 — `Experiments/E06PageRank.cs`**
- Строка ~26: `public OperationResult<ExperimentV2Result> Run(IReadOnlyList<(string Caller, string Callee)> callPairs)` -> добавить `, int tier = 1`
- Строка 111: `Tier = 1,` -> `Tier = tier,`

### Обновить Dispatcher — `ExperimentsV2Dispatcher.cs`

Добавить `int tier = 1` во все шесть методов и пробросить в вызовы:
```csharp
public OperationResult<ExperimentV2Result> RunE01(
    string csharpSource, int bpeVocabSize = 512, int tier = 1)
    => _e01.Run(csharpSource, bpeVocabSize, tier);

public OperationResult<ExperimentV2Result> RunE02(
    IReadOnlyList<string> sources, int tier = 1)
    => _e02.Run(sources, tier);

public OperationResult<(ExperimentV2Result summary, List<NgramResultEntry> perNResults)>
    RunE03(IReadOnlyList<int> tokens, int maxN = 3, int tier = 1)
    => new E03NgramModel(maxN).Run(tokens, tier);

public OperationResult<ExperimentV2Result> RunE04(
    IReadOnlyList<(string Id, string Source)> documents,
    IReadOnlyList<(string Query, string ExpectedId)> queries,
    int tier = 1)
    => _e04.Run(documents, queries, tier);

public OperationResult<ExperimentV2Result> RunE05(
    string csharpSource, int tier = 1)
    => _e05.Run(csharpSource, tier);

public OperationResult<ExperimentV2Result> RunE06(
    IReadOnlyList<(string Caller, string Callee)> callPairs, int tier = 1)
    => _e06.Run(callPairs, tier);
```

**Проверить компиляцию после патча:**
```
dotnet build 35.ExperimentsV2/35.ExperimentsV2.csproj
```
**Ожидаемый вывод:** `Build succeeded. 0 Error(s)`

---

### Написать файл `35.ExperimentsV2/ExperimentRunner.cs`

```csharp
namespace ExperimentsV2;

// ExperimentRunner -- единственная точка входа для запуска экспериментов.
// Связывает DatasetLoader -> InputAdapters -> Dispatcher -> ResultWriter.
//
// КАК ЗАПУСТИТЬ ОДИН ЭКСПЕРИМЕНТ НА ОДНОМ ТИРЕ:
//   var runner = new ExperimentRunner(
//       projectRoot: @"/path/to/llm_mvp_core",
//       outputRoot:  @"/path/to/llm_mvp_core/experiments");
//   runner.Run("E01", tier: 1);
//
// КАК УВЕЛИЧИТЬ НАГРУЗКУ (запустить лестницу Tier 1 -> 2 -> 3):
//   runner.RunTierLadder("E01");
//
// КАК ЗАПУСТИТЬ ВСЕ ШЕСТЬ ЭКСПЕРИМЕНТОВ:
//   runner.RunAll(tier: 1);
public sealed class ExperimentRunner
{
    private readonly string _projectRoot;
    private readonly string _outputRoot;
    private readonly ExperimentsV2Dispatcher _d = new();

    public ExperimentRunner(string projectRoot, string outputRoot)
    {
        _projectRoot = projectRoot;
        _outputRoot  = outputRoot;
    }

    // Запустить один эксперимент на одном тире.
    // Печатает одну строку в консоль. Записывает .json + .md в outputRoot.
    public OperationResult<ExperimentV2Result> Run(string id, int tier)
    {
        // Шаг 1: загрузить датасет
        Console.Write($"[{id}] Tier {tier} загрузка... ");
        var loadOp = DatasetLoader.Load(tier, _projectRoot);
        if (!loadOp.IsSuccess)
        {
            Console.WriteLine($"ERROR: {loadOp.ErrorMessage}");
            return OperationResult<ExperimentV2Result>.Fail(loadOp.ErrorMessage);
        }
        var ds = loadOp.Value!;
        Console.WriteLine($"{ds.FileCount} файлов, {ds.CombinedText.Length:N0} симв.");

        // Шаг 2: запустить эксперимент
        Console.Write($"[{id}] Запуск... ");
        var expOp = Dispatch(id, ds, tier);
        if (!expOp.IsSuccess)
        {
            Console.WriteLine($"ERROR: {expOp.ErrorMessage}");
            return expOp;
        }
        var r = expOp.Value!;

        // Шаг 3: записать результаты
        var writeOp = ResultWriter.Write(r, _outputRoot);
        string fileLine = writeOp.IsSuccess
            ? $"-> {writeOp.Value}"
            : $"WRITE FAILED: {writeOp.ErrorMessage}";

        // Шаг 4: напечатать однострочный итог
        string arrow  = r.LowerIsBetter ? "down" : "up";
        string status = r.Passed ? "PASSED" : "NOT PASSED";
        Console.WriteLine(
            $"OK {r.PrimaryMetricName} ({arrow}) baseline={r.BaselinePrimary:F2} " +
            $"enhanced={r.EnhancedPrimary:F2} delta={r.ImprovementPct:+0.0;-0.0}% | {status} | {r.NextAction}");
        Console.WriteLine($"  {fileLine}");

        return OperationResult<ExperimentV2Result>.Ok(r);
    }

    // Как увеличить нагрузку: запустить лестницу Tier 1 -> 2 -> 3.
    // Переходит на следующий тир если эксперимент прошёл (improvement >= 5%).
    // Останавливается при первой неудаче или достижении Tier 3.
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

    // Запустить все шесть экспериментов на одном тире.
    // Печатает сводную таблицу после всех прогонов.
    public void RunAll(int tier)
    {
        string[] ids = ["E01", "E02", "E03", "E04", "E05", "E06"];
        var results  = new List<(string Id, OperationResult<ExperimentV2Result> R)>();

        foreach (var id in ids)
        {
            Console.WriteLine();
            results.Add((id, Run(id, tier)));
        }

        // Как показать результаты: сводная таблица в консоли
        Console.WriteLine();
        Console.WriteLine(new string('=', 72));
        Console.WriteLine($"SUMMARY -- all experiments at Tier {tier}");
        Console.WriteLine(new string('-', 72));
        Console.WriteLine($"{"ID",-8} {"Metric",-24} {"Baseline",10} {"Enhanced",10} {"Delta%",7}  Status");
        Console.WriteLine(new string('-', 72));

        foreach (var (id, op) in results)
        {
            if (!op.IsSuccess)
            {
                Console.WriteLine($"{id,-8} ERROR: {op.ErrorMessage}");
                continue;
            }
            var v = op.Value!;
            Console.WriteLine($"{id,-8} {v.PrimaryMetricName,-24} {v.BaselinePrimary,10:F3} " +
                              $"{v.EnhancedPrimary,10:F3} {v.ImprovementPct,7:+0.0;-0.0}  " +
                              $"{(v.Passed ? "PASSED" : "FAILED")}");
        }

        Console.WriteLine(new string('-', 72));
        int passed = results.Count(r => r.R.IsSuccess && r.R.Value!.Passed);
        Console.WriteLine($"Passed: {passed}/{ids.Length} at Tier {tier}");
        Console.WriteLine($"Results written to: {_outputRoot}");
        Console.WriteLine($"Summary: {Path.Combine(_outputRoot, "progress", "progress.md")}");
        Console.WriteLine(new string('=', 72));
    }

    private OperationResult<ExperimentV2Result> Dispatch(
        string id, DatasetContents ds, int tier)
    {
        return id.ToUpperInvariant() switch
        {
            "E01" => _d.RunE01(InputAdapters.ForE01(ds), tier: tier),
            "E02" => _d.RunE02(InputAdapters.ForE02(ds), tier: tier),
            "E03" => DispatchE03(ds, tier),
            "E04" => DispatchE04(ds, tier),
            "E05" => _d.RunE05(InputAdapters.ForE05(ds), tier: tier),
            "E06" => DispatchE06(ds, tier),
            _     => OperationResult<ExperimentV2Result>.Fail($"Unknown experiment: {id}")
        };
    }

    private OperationResult<ExperimentV2Result> DispatchE03(DatasetContents ds, int tier)
    {
        var adapt = InputAdapters.ForE03(ds);
        if (!adapt.IsSuccess)
            return OperationResult<ExperimentV2Result>.Fail(adapt.ErrorMessage);
        var r = _d.RunE03(adapt.Value!, tier: tier);
        return r.IsSuccess
            ? OperationResult<ExperimentV2Result>.Ok(r.Value!.summary)
            : OperationResult<ExperimentV2Result>.Fail(r.ErrorMessage);
    }

    private OperationResult<ExperimentV2Result> DispatchE04(DatasetContents ds, int tier)
    {
        var adapt = InputAdapters.ForE04(ds);
        if (!adapt.IsSuccess)
        {
            // E04 на Tier 1 -- ожидаемый Fail, не ошибка кода.
            Console.WriteLine($"  NOTE: {adapt.ErrorMessage}");
            return OperationResult<ExperimentV2Result>.Fail(adapt.ErrorMessage);
        }
        return _d.RunE04(adapt.Value!.Documents, adapt.Value!.Queries, tier: tier);
    }

    private OperationResult<ExperimentV2Result> DispatchE06(DatasetContents ds, int tier)
    {
        var adapt = InputAdapters.ForE06(ds);
        return adapt.IsSuccess
            ? _d.RunE06(adapt.Value!, tier: tier)
            : OperationResult<ExperimentV2Result>.Fail(adapt.ErrorMessage);
    }
}
```

---

## Gate B — Финальная проверка всего

### Шаг 1: сборка

```
dotnet build 35.ExperimentsV2/35.ExperimentsV2.csproj
```
**Ожидаемый вывод:**
```
Build succeeded.
0 Error(s)
0 Warning(s)
```

### Шаг 2: запустить E01 на Tier 1

```csharp
string projectRoot = @"/absolute/path/to/llm_mvp_core";
string outputRoot  = Path.Combine(projectRoot, "experiments");
var runner = new ExperimentRunner(projectRoot, outputRoot);

runner.Run("E01", tier: 1);
```

**Ожидаемый вывод в консоль:**
```
[E01] Tier 1 загрузка... 3 файлов, 5 248 симв.
[E01] Запуск... OK perplexity_3gram (down) baseline=185.23 enhanced=42.17 delta=+77.2% | PASSED | PROMOTE_TO_NEXT_TIER
  -> /path/to/experiments/E01-AST-TOKENIZER/results/run-2026-05-01-001.json
```

**Проверить что файл существует:**
```csharp
string resultsDir = Path.Combine(outputRoot, "E01-AST-TOKENIZER", "results");
string[] jsonFiles = Directory.GetFiles(resultsDir, "*.json");
Console.WriteLine($"JSON files: {jsonFiles.Length}");  // Ожидаем: 1

var doc = JsonDocument.Parse(File.ReadAllText(jsonFiles[0]));
Console.WriteLine($"passed={doc.RootElement.GetProperty("passed").GetBoolean()}");          // True
Console.WriteLine($"tier={doc.RootElement.GetProperty("tier").GetInt32()}");                 // 1
Console.WriteLine($"next_action={doc.RootElement.GetProperty("next_action").GetString()}"); // PROMOTE_TO_NEXT_TIER
```

### Шаг 3: как увеличить нагрузку (лестница тиров)

```csharp
runner.RunTierLadder("E01");
```

**Ожидаемый вывод:**
```
=== E01 -- Tier 1 ===================================
[E01] Tier 1 загрузка... 3 файлов, 5 248 симв.
[E01] Запуск... OK perplexity_3gram (down) ... delta=+77.2% | PASSED | PROMOTE_TO_NEXT_TIER
  -> .../E01-AST-TOKENIZER/results/run-2026-05-01-001.json

=== E01 -- Tier 2 ===================================
[E01] Tier 2 загрузка... 47 файлов, 38 412 симв.
[E01] Запуск... OK perplexity_3gram (down) ... delta=+69.4% | PASSED | PROMOTE_TO_NEXT_TIER
  -> .../E01-AST-TOKENIZER/results/run-2026-05-01-002.json

=== E01 -- Tier 3 ===================================
[E01] Tier 3 загрузка... 195 файлов, 141 000 симв.
[E01] Запуск... OK perplexity_3gram (down) ... delta=+63.1% | PASSED | PROMOTE_TO_NEXT_TIER
  -> .../E01-AST-TOKENIZER/results/run-2026-05-01-003.json
ALL PASSED: E01 прошёл все три тира!
```

**Проверить что создано 3 файла:**
```csharp
string[] files = Directory.GetFiles(
    Path.Combine(outputRoot, "E01-AST-TOKENIZER", "results"), "*.json");
Console.WriteLine($"Files after ladder: {files.Length}");  // Ожидаем: 3
```

### Шаг 4: как показать результаты всех экспериментов

```csharp
runner.RunAll(tier: 1);
```

**Ожидаемый вывод таблицы:**
```
========================================================================
SUMMARY -- all experiments at Tier 1
------------------------------------------------------------------------
ID       Metric                   Baseline   Enhanced   Delta%  Status
------------------------------------------------------------------------
E01      perplexity_3gram          185.234     42.170    +77.2  PASSED
E02      top1_accuracy_pct           0.042      0.187   +345.2  PASSED
E03      perplexity_3gram          185.234     47.320    +74.4  PASSED
E04      NOTE: E04 requires Tier 2+ ...
E04      ERROR: E04 требует Tier 2+...
E05      syntax_correct_pct          0.710      0.990    +39.4  PASSED
E06      mrr_score                   0.110      0.130    +18.2  PASSED
------------------------------------------------------------------------
Passed: 5/6 at Tier 1
Results written to: /path/to/experiments
Summary: /path/to/experiments/progress/progress.md
========================================================================
```

**E04 на Tier 1 = ожидаемое поведение.** Запустить E04 с tier: 2:
```csharp
runner.Run("E04", tier: 2);
```

**Показать сводный MD файл (главный отчёт для человека):**
```csharp
string progressMd = Path.Combine(outputRoot, "progress", "progress.md");
Console.WriteLine(File.ReadAllText(progressMd));
```

---

## Как показать результаты: итоговая файловая структура

После всех прогонов:
```
experiments/
├── E01-AST-TOKENIZER/results/
│   ├── run-2026-05-01-001.json   <- Tier 1: passed=true, improvement_pct=77.23
│   ├── run-2026-05-01-002.json   <- Tier 2
│   └── run-2026-05-01-003.json   <- Tier 3
├── E02-FREQUENCY-RANKING/results/
│   └── run-2026-05-01-001.json
... (E03, E05, E06 аналогично)
└── progress/
    ├── progress.json   <- сводка для автоматизации
    └── progress.md    <- ГЛАВНЫЙ ОТЧЁТ для человека
```

**Чтобы показать финальный отчёт:**
```csharp
Console.WriteLine(File.ReadAllText(Path.Combine(outputRoot, "progress", "progress.md")));
```

---

## Порядок выполнения для Claude Code

```
Шаг 1: Написать Infrastructure/DatasetLoader.cs
        -> dotnet build
        -> ожидаем: Build succeeded. 0 Error(s)
        -> проверить: Load(1) возвращает 3+ файла, Load(2) возвращает 47+ файлов

Шаг 2: Написать Infrastructure/InputAdapters.cs
        -> dotnet build
        -> ожидаем: Build succeeded. 0 Error(s)
        -> проверить: ForE04(tier1Dataset) возвращает IsSuccess=False
           с сообщением содержащим "Tier 2+"

Шаг 3: Написать Infrastructure/ResultWriter.cs
        -> dotnet build
        -> ожидаем: Build succeeded. 0 Error(s)
        -> проверить: Write(fakeResult) создаёт .json и .md файлы
           JSON парсится, passed=true, next_action=PROMOTE_TO_NEXT_TIER

Шаг 4: Патч Tier=1 в шести файлах (E01:88, E02:87, E03:116, E04:93, E05:99, E06:111)
        + обновить Dispatcher (добавить int tier=1 во все RunE0X методы)
        -> dotnet build
        -> ожидаем: Build succeeded. 0 Error(s)

Шаг 5: Написать ExperimentRunner.cs
        -> dotnet build
        -> ожидаем: Build succeeded. 0 Error(s)
        -> runner.Run("E01", tier: 1)
           ПРОВЕРИТЬ: в консоли строка "PASSED | PROMOTE_TO_NEXT_TIER"
           ПРОВЕРИТЬ: файл experiments/E01-AST-TOKENIZER/results/run-*.json существует
           ПРОВЕРИТЬ: JSON содержит passed=true, tier=1
        -> runner.RunTierLadder("E01")
           ПРОВЕРИТЬ: в папке E01-AST-TOKENIZER/results/ появилось 3 файла
        -> runner.RunAll(tier: 1)
           ПРОВЕРИТЬ: таблица в консоли содержит строки E01-E06
           ПРОВЕРИТЬ: файл experiments/progress/progress.md существует
           ПОКАЗАТЬ содержимое progress.md
```

---

## Проверка DNA правил

| Правило | Реализация |
|---|---|
| Правило 1 (данные как записи) | `DatasetContents` — sealed record с примитивами |
| Правило 3 (OperationResult везде) | `Load()`, `ForE03/E04/E06()`, `Write()`, `Run()` |
| Правило 7 (идемпотентность) | Имя файла = дата+порядковый номер, повтор = новый файл |
| Правило 8 (хранить до события) | `Write()` пишет JSON до MD, MD до progress |

---

*35.ExperimentsV2 Infrastructure Plan v1.1 — не касается XIIGen*
