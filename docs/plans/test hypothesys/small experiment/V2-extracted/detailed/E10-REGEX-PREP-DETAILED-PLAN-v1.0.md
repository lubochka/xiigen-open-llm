# E10 — Регекс как предфильтр перед Roslyn
## Подробный план для Claude Code v1.0

**Проект:** `35.ExperimentsV2`  
**Датасет:** существующий `llm_mvp_core` (ничего скачивать не нужно)  
**Этот документ не касается XIIGen.**

---

## Что делаем и зачем

Roslyn парсит каждый .cs файл полностью — включая комментарии,
строковые литералы, XML-документацию. Это лишняя работа.

Добавляем один фильтр: очищаем текст регексом ДО передачи в Roslyn.
Измеряем: ускорилась ли подготовка данных без потери качества.

---

## Шаг 1 — Создать RegexPreprocessor.cs

**Путь:** `35.ExperimentsV2/Infrastructure/RegexPreprocessor.cs`

```csharp
using System.Text.RegularExpressions;

namespace ExperimentsV2;

// RegexPreprocessor убирает шум из C# файлов до передачи в Roslyn.
// Цель: уменьшить объём текста который Roslyn должен парсить.
// Гарантия: нормализованный код синтаксически корректен (только чище).
public static class RegexPreprocessor
{
    // Порядок применения важен: сначала XML-docs, потом блочные, потом строки.
    private static readonly Regex XmlDocs = new(
        @"///[^\n]*", RegexOptions.Compiled);

    private static readonly Regex BlockComments = new(
        @"/\*.*?\*/", RegexOptions.Compiled | RegexOptions.Singleline);

    private static readonly Regex LineComments = new(
        @"//[^\n]*", RegexOptions.Compiled);

    private static readonly Regex StringLiterals = new(
        @"@""(?:[^""]|"""")*""|""(?:[^""\\]|\\.)*""",
        RegexOptions.Compiled);

    private static readonly Regex ExtraBlankLines = new(
        @"\n(\s*\n){2,}", RegexOptions.Compiled);

    // Нормализовать C# исходный код.
    // Возвращает текст без комментариев, с пустыми строковыми литералами,
    // без лишних пустых строк.
    public static string Normalize(string source)
    {
        if (string.IsNullOrWhiteSpace(source)) return source;
        string s = XmlDocs.Replace(source, "");
        s = BlockComments.Replace(s, " ");
        s = LineComments.Replace(s, "");
        s = StringLiterals.Replace(s, "\"\"");
        s = ExtraBlankLines.Replace(s, "\n\n");
        return s;
    }
}
```

**Проверить сборку:**
```
dotnet build 35.ExperimentsV2/35.ExperimentsV2.csproj
```
Ожидаем: `Build succeeded. 0 Error(s)`

---

## Шаг 2 — Создать E10RegexPrep.cs

**Путь:** `35.ExperimentsV2/Experiments/E10RegexPrep.cs`

```csharp
using Microsoft.CodeAnalysis.CSharp;

namespace ExperimentsV2;

// E10: измеряет ускорение Roslyn-разбора после регекс-нормализации.
// Baseline: Roslyn.ParseText(rawSource)
// Enhanced: Roslyn.ParseText(RegexPreprocessor.Normalize(rawSource))
// Метрика: время разбора всего датасета в миллисекундах.
public sealed class E10RegexPrep
{
    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
    {
        if (files.Count == 0)
            return OperationResult<ExperimentV2Result>.Fail("Нет файлов для E10");

        // Шаг 1: проверить что нормализация не ломает синтаксис
        int syntaxErrors = 0;
        foreach (var (path, text) in files)
        {
            string normalized = RegexPreprocessor.Normalize(text);
            var tree = CSharpSyntaxTree.ParseText(normalized);
            bool hasErrors = tree.GetDiagnostics()
                .Any(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
            if (hasErrors) syntaxErrors++;
        }

        if (syntaxErrors > 0)
            return OperationResult<ExperimentV2Result>.Fail(
                $"Нормализация сломала синтаксис в {syntaxErrors} файлах — проверь RegexPreprocessor");

        // Шаг 2: измерить baseline — Roslyn на сырых файлах
        var t0 = Stopwatch.GetTimestamp();
        int baselineNodes = 0;
        foreach (var (_, text) in files)
        {
            var tree = CSharpSyntaxTree.ParseText(text);
            baselineNodes += tree.GetRoot().DescendantNodes().Count();
        }
        double baselineMs = ElapsedMs(t0);

        // Шаг 3: измерить enhanced — регекс + Roslyn
        var t1 = Stopwatch.GetTimestamp();
        int enhancedNodes = 0;
        int totalCharsRaw = 0;
        int totalCharsNorm = 0;
        foreach (var (_, text) in files)
        {
            totalCharsRaw += text.Length;
            string normalized = RegexPreprocessor.Normalize(text);
            totalCharsNorm += normalized.Length;
            var tree = CSharpSyntaxTree.ParseText(normalized);
            enhancedNodes += tree.GetRoot().DescendantNodes().Count();
        }
        double enhancedMs = ElapsedMs(t1);

        // Scaling signal: насколько уменьшился текст после нормализации
        double charReductionPct = totalCharsRaw > 0
            ? (1.0 - (double)totalCharsNorm / totalCharsRaw) * 100.0
            : 0;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E10-REGEX-PREP",
            Tier              = tier,
            BaselinePrimary   = baselineMs,
            EnhancedPrimary   = enhancedMs,
            PrimaryMetricName = "parse_time_ms",
            LowerIsBetter     = true,
            ScalingSignal     = charReductionPct,
            ScalingSignalNote = "char_reduction_pct (сколько % символов убрал регекс)",
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes             = $"files={files.Count}, " +
                                $"charsRaw={totalCharsRaw:N0}, " +
                                $"charsNorm={totalCharsNorm:N0}, " +
                                $"reduction={charReductionPct:F1}%, " +
                                $"nodesBaseline={baselineNodes}, " +
                                $"nodesEnhanced={enhancedNodes}, " +
                                $"syntaxErrors={syntaxErrors}"
        });
    }

    private static long Stopwatch() => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t0)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t0)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
```

**Проверить сборку:**
```
dotnet build 35.ExperimentsV2/35.ExperimentsV2.csproj
```
Ожидаем: `Build succeeded. 0 Error(s)`

---

## Шаг 3 — Добавить E10 в Runner и Dispatcher

### В `ExperimentsV2Dispatcher.cs` добавить метод:

```csharp
public OperationResult<ExperimentV2Result> RunE10(
    IReadOnlyList<(string Path, string Text)> files,
    int tier = 1)
    => new E10RegexPrep().Run(files, tier);
```

### В `ExperimentRunner.cs` — два места:

**1. В массив `ids` добавить `"E10"`:**
```csharp
// БЫЛО:
string[] ids = ["E01", "E02", "E03", "E04", "E05", "E06", "E07"];

// СТАЛО:
string[] ids = ["E01", "E02", "E03", "E04", "E05", "E06", "E07", "E10"];
```

**2. В switch в `Dispatch()` добавить:**
```csharp
"E10" => DispatchE10(ds, tier),
```

**3. Добавить приватный метод после `DispatchE07`:**
```csharp
private OperationResult<ExperimentV2Result> DispatchE10(DatasetContents ds, int tier)
    => _d.RunE10(ds.Files, tier);
```

**Проверить сборку:**
```
dotnet build 35.ExperimentsV2/35.ExperimentsV2.csproj
```
Ожидаем: `Build succeeded. 0 Error(s)`

---

## Шаг 4 — Добавить E10 в ResultWriter

В `Infrastructure/ResultWriter.cs` в методе `FolderName()` добавить строку:
```csharp
"E10-REGEX-PREP" => "E10-REGEX-PREP",
```

---

## Шаг 5 — Запустить и проверить результаты

**Команда запуска:**
```csharp
var runner = new ExperimentRunner(
    projectRoot: @"C:\Projects\llm_mvp_core",   // подставить реальный путь
    outputRoot:  @"C:\Projects\llm_mvp_core\experiments-v2");

runner.Run("E10", tier: 1);
```

**Ожидаемый вывод в консоль:**
```
[E10] Tier 1 загрузка... 3 файлов, 6 468 симв.
[E10] Запуск...
OK parse_time_ms (down) baseline=45.23 enhanced=28.91 delta=-36.1% | PASSED | PROMOTE_TO_NEXT_TIER
  -> .../experiments-v2/E10-REGEX-PREP/results/run-YYYY-MM-DD-001.json
```

**Ожидаемые числа:**
- `char_reduction_pct` (scaling signal): 15–35% (регекс убирает комментарии и строки)
- `parse_time_ms` delta: от -10% до -40% (зависит от насыщенности файлов комментариями)
- `syntaxErrors`: **строго 0** — если не 0, остановиться и исправить RegexPreprocessor

**Проверить JSON файл:**
```csharp
// После запуска прочитать JSON и убедиться:
var json = JsonDocument.Parse(File.ReadAllText(".../E10-REGEX-PREP/results/run-...-001.json"));
Console.WriteLine(json.RootElement.GetProperty("notes").GetString());
// Ожидаем: "files=3, charsRaw=6468, charsNorm=XXXX, reduction=XX%, syntaxErrors=0"
```

---

## Шаг 6 — Если E10 прошёл: интегрировать в DatasetLoader

Если `improvement_pct >= 10` (ускорение ≥ 10%) — добавить нормализацию
в `Infrastructure/DatasetLoader.cs`.

Найти строку где читается текст файла:
```csharp
// БЫЛО:
files.Add((f, File.ReadAllText(f, Encoding.UTF8)));

// СТАЛО:
var raw = File.ReadAllText(f, Encoding.UTF8);
files.Add((f, RegexPreprocessor.Normalize(raw)));
```

После этого запустить все эксперименты снова:
```csharp
runner.RunAll();
```

Сравнить время в progress.md — общее время подготовки должно сократиться.

---

## Что смотреть в progress.md после запуска

```
| Experiment | Passed | Tier | Delta% | Signal | Next |
| E10-REGEX-PREP | YES | 1 | -XX.X% | XX.X | PROMOTE_TO_NEXT_TIER |
```

Знак минус у delta — это хорошо (время уменьшилось, lower_is_better=true).

---

## Тесты

После сборки добавить тест:
```csharp
// В тестовый проект:
[Fact]
public void Normalize_RemovesComments_PreservesSyntax()
{
    string source = """
        // однострочный комментарий
        /// XML docs
        /* блочный */
        public class Foo
        {
            string s = "hello world";
            void Bar() { }
        }
        """;

    string normalized = RegexPreprocessor.Normalize(source);

    // Комментарии убраны
    Assert.DoesNotContain("однострочный", normalized);
    Assert.DoesNotContain("XML docs", normalized);
    Assert.DoesNotContain("блочный", normalized);

    // Строка заменена на ""
    Assert.DoesNotContain("hello world", normalized);

    // Код синтаксически корректен
    var tree = CSharpSyntaxTree.ParseText(normalized);
    Assert.False(tree.GetDiagnostics()
        .Any(d => d.Severity == DiagnosticSeverity.Error));
}
```

```
dotnet test
```
Ожидаем: все тесты зелёные.

---

*E10 план v1.0 · не касается XIIGen*
