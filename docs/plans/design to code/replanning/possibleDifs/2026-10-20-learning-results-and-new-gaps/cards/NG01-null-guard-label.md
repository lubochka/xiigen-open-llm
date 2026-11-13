# NG01 — числовой pattern не должен скрывать учебную потребность в null guard

**Статус: DESIGNED_NOT_RUN.** Сейчас создан только документ. Host, source copies, build и Train/Predict не выполнялись. Это узкий проверяемый подслучай известного N48-01; самостоятельная новая root-cause family не заявляется. Промпт — [NG01-executor.md](../prompts/NG01-executor.md).

**Фаза и очередь.** Будущий runtime выполняется по поручению выполнить NG01 либо выполнять/продолжать программу по реестру пакета, когда очередь дошла до NG01. Уже данное поручение на программу не требует повторного допуска каждого ID. Текущая документная сессия runtime не разрешает. Защищённые № 01–03 завершены; следующий действующий пункт — № 04 N62-R1. NG01 не вставляется перед ним. Следующий опыт определяется реестром/прямым назначением, не поиском похожего ID. Production, protected docs и старые результаты не меняются.

**Место в дополнительном [реестре пакета](../08-execution-registry.md): № 32; следующий № 33 KN-ROUTER-LEARNED-KEY-01.** Это новый порядок будущего порученного исполнения; защищённый исходный индекс не редактируется. Прямо назначенный ID имеет приоритет. При одиночном поручении NG01 следующий опыт самостоятельно не начинать.

## Обязательные правила карточки и любого её промпта

**Модель:** модель текущей сессии по умолчанию, без override; фактическое имя в отчёте. Ученик — native FileActionKnnPredictor, k=3. Внешняя LLM, LoRA, neural optimizer и GPU не участвуют.

**Успех:** любое фактически измеренное полезное улучшение считается успехом по соответствующему показателю и домену; рост accuracy не обязателен. Все изменения остальных показателей, включая ухудшения, показать рядом. Нулевой эффект допустим. Одиночное фиксированное A→B время/CPU — наблюдение с неустановленной причинностью.

**Послабления:** в назначенной экспериментальной копии допустимы throw, try/catch, void, простые DTO и минимальный host. Product OperationResult, полный CI, комментарии к каждой строке и требования выпуска не навязываются. Ошибку сохранить с ненулевым exit code. Native learner не подменять proxy.

**Только отчёт и кандидат:** после обычного завершения — русский report.md, raw JSON и candidate.md с source fix, полезным результатом, регрессиями и границей доказательства. Перенос в production, product tests, commit/push и правка protected required-changes.md не выполняются. Решение о переносе принимает пользователь. Самопроверка не называется независимым review.

**STOP/NEXT:** по «заканчивай и переходи к следующему» немедленно перейти к уже назначенному следующему заданию в разрешённой фазе. Запрещено продолжать прежний отчёт, реестр, паспорт, hashes, review, retry, исследование Git или закрывающие проверки. Нельзя объявлять завершение и снова возвращаться к опыту. По «остановить всё» немедленно прекратить всю работу; допустима лишь необходимая остановка собственного живого процесса. Незавершённые файлы и raw сохранить. Эти команды имеют приоритет над оформлением. Историческая цитата об остановке не является текущим приказом остановить документный аудит.

## 1. Учебный вопрос, домен и источник

**Домен:** программирование C#, распознавание необходимой защиты nullable receiver. Контракт панели: nullable параметр может быть null; операция должна обработать это до dereference. Это заданный контракт, не утверждение, что любой API обязан иметь guard.

[ActionLabelGenerator.Label32–38](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/12.SemanticCompiler/Prediction/FileActionKnn.cs#L32-L38) считает любой IsPatternExpression внутри if признаком null condition:

```csharp
int F(string? x, int n) { if (n is > 0) return x.Length; return 0; }
```

Условие относится к non-nullable int n и не защищает x. По коду Label подавляет ADD_NULL_CHECK. Это source inference, не измеренный native результат. Удалять все relational patterns нельзя: `int F(int? x){if(x is >0)return x.Value;return 0;}` законно исключает null перед x.Value; такой контроль обязателен.

**Реальный учебный путь:** [Facade.TrainFileActionKnn194–203](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/12.SemanticCompiler/SemanticCompilerFacade.cs#L194-L203) → FeatureVector(source) + Label(source) → [native Train122–126](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/12.SemanticCompiler/Prediction/FileActionKnn.cs#L122-L126) → таблица _train → [Predict129–141](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/12.SemanticCompiler/Prediction/FileActionKnn.cs#L129-L141).

[Reader facade327–333](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/12.SemanticCompiler/SemanticCompilerFacade.cs#L327-L333) требует включённый механизм, C# и минимум 4 записи. [UseFileActionKnn default false](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/12.SemanticCompiler/SemanticCompilerOptions.cs#L20-L21). Host фиксирует C# и включение одинаково, портирует только guard минимум 4. Полный Compile и downstream RP01 не вызываются.

**Новизна ограничена:** [N48-01:17–95](https://github.com/lubochka/xiigen-planning/blob/4b950599a04388941f56b41e634233ebb7dc17c2/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/48-LANGUAGE-TRANSLATE-AUDIT-2026-10-19/02-NEW-HYPOTHESES.md#L17-L95) уже описывает false suppression из-за чужого/nested guard и отсутствие receiver/scope/dominance. Здесь отдельный ablation — не-nullable int relational pattern. N-KU проверял DoStatement, METHOD — Next scope; они не повторяются.

Дополнительный prior — [core65:45](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/65-e22-positive-late-run-and-label-transfer.md#L45) и [N53 dedup:109](https://github.com/lubochka/xiigen-planning/blob/4b950599a04388941f56b41e634233ebb7dc17c2/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/53-EXTERNAL-DOMAINS-RECOVERY-AND-SPARSE-KNN-REFINEMENTS-2026-10-19/03-MECHANISM-COVERAGE-AND-DEDUP.md#L109): общий тезис «любой IsPatternExpression не обязательно защищает нужный объект» уже известен. Точный int-parameter factor с nullable-int/type/null controls данным prior не задаётся. Этот рецепт уточняет известную проблему, не присваивает ей новую историю происхождения.

## 2. Единственный фактор

A — весь исходный FileActionKnn.cs, только namespace Ng01.SourceA. B — тот же файл с namespace Ng01.SourceB; в hasNullCondition исключён только direct relational pattern на параметре, явно записанном как int. Точная замена и helper заданы в Prepare.ps1 ниже.

Nullable int, type patterns, null literal и остальные patterns сохраняются. Это узкий фильтр заведомо нерелевантного null evidence, не общий null analyzer. Он не решает receiver binding, dominance, called local functions, compound/parenthesized patterns, aliases System.Int32 и другие типы. Не расширять его в ходе опыта.

FeatureVector/TargetKinds, 30 координат, k=3, CosineDistance, сортировка, label priorities, данные и порядок одинаковы. Label и FeatureVector по-прежнему отдельно парсят source: KH-COST оптимизация здесь была бы вторым фактором.

## 3. Точное место и состав будущего host

Короткий корень **C:/xg/NG01**, project **C:/xg/NG01/Runner.csproj**, results **C:/xg/NG01/r/attempt-0001**. Длинный protected путь Windows не используется для build/results. Если корень занят другой работой или attempt существует — сохранить его, не удалять/не перезаписывать и не выбирать новый attempt самовольно.

Read-only source checkout: **C:/Projects/llm_mvp_core**; pin **0b76ab950d82534f5904005dda096545505ae307**; путь **12.SemanticCompiler/Prediction/FileActionKnn.cs**; Git blob **ef7a88a3c4c2540569b9ce6f204d8e4750875501**. Git нужен только для чтения этого объекта, не истории/публикации.

Создать только четыре файла ниже через файловый инструмент. Prepare.ps1 затем создаёт SourceA.cs/SourceB.cs. Никакого ProjectReference, Core stubs, reflection, общего experiment dispatcher или whole-repo globs. net9.0, Roslyn4.12.0 и такой source-copy состав взяты из существующего [N-KU паспорта 198–245](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/experimets/cards/N-KU-01-do-loop-label.md#L198-L245). Это назначение нового host, не утверждение, что NG01 уже собран.

### Runner.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
    <AssemblyName>Ng01Runner</AssemblyName>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.12.0" />
    <Compile Include="Program.cs" />
    <Compile Include="SourceA.cs" />
    <Compile Include="SourceB.cs" />
  </ItemGroup>
</Project>
```

### Prepare.ps1

```powershell
$ErrorActionPreference = 'Stop'
$Root = 'C:/xg/NG01'
$Repo = 'C:/Projects/llm_mvp_core'
$Commit = '0b76ab950d82534f5904005dda096545505ae307'
$NativePath = '12.SemanticCompiler/Prediction/FileActionKnn.cs'
if (!(Test-Path -LiteralPath $Root -PathType Container)) {
    throw 'NG01 root is missing; materialize only the four files from the card.'
}
foreach ($Name in @('SourceA.cs', 'SourceB.cs', 'source-origin.json')) {
    if (Test-Path -LiteralPath (Join-Path $Root $Name)) {
        throw "Existing NG01 artifact must be preserved: $Name"
    }
}
$Lines = & git -C $Repo show ($Commit + ':' + $NativePath)
if ($LASTEXITCODE -ne 0) {
    throw 'Pinned Git object unavailable; do not substitute HEAD or another branch.'
}
$Source = ($Lines -join [string][char]10) + [string][char]10
function Replace-ExactlyOnce([string]$Text, [string]$Old, [string]$New) {
    if ([regex]::Matches($Text, [regex]::Escape($Old)).Count -ne 1) {
        throw 'Pinned source anchor does not occur exactly once.'
    }
    return $Text.Replace($Old, $New)
}
$A = Replace-ExactlyOnce $Source 'namespace SemanticCompiler.Prediction;' 'namespace Ng01.SourceA;'
$B = Replace-ExactlyOnce $Source 'namespace SemanticCompiler.Prediction;' 'namespace Ng01.SourceB;'
$OldCondition = '.Any(node => node.Kind() == SyntaxKind.NullLiteralExpression || node.Kind() == SyntaxKind.IsPatternExpression));'
$NewCondition = @'
.Any(node => node.Kind() == SyntaxKind.NullLiteralExpression ||
                        (node is IsPatternExpressionSyntax pattern &&
                         !IsNonNullableIntRelationalPattern(method, pattern))));
'@
$B = Replace-ExactlyOnce $B $OldCondition ($NewCondition.Trim())
$Anchor = '    // Build the normalized SyntaxKind frequency vector used by FileActionKnnPredictor.'
$Helper = @'
    private static bool IsNonNullableIntRelationalPattern(
        MethodDeclarationSyntax method,
        IsPatternExpressionSyntax pattern)
    {
        if (pattern.Pattern is not RelationalPatternSyntax ||
            pattern.Expression is not IdentifierNameSyntax identifier)
            return false;
        return method.ParameterList.Parameters.Any(parameter =>
            parameter.Identifier.ValueText == identifier.Identifier.ValueText &&
            parameter.Type is PredefinedTypeSyntax predefined &&
            predefined.Keyword.IsKind(SyntaxKind.IntKeyword));
    }

'@
$B = Replace-ExactlyOnce $B $Anchor ($Helper + [Environment]::NewLine + $Anchor)
$Utf8 = [System.Text.UTF8Encoding]::new($false)
[System.IO.File]::WriteAllText((Join-Path $Root 'SourceA.cs'), $A, $Utf8)
[System.IO.File]::WriteAllText((Join-Path $Root 'SourceB.cs'), $B, $Utf8)
$Origin = @{
    repository = 'lubochka/llm_mvp_core'
    commit = $Commit
    path = $NativePath
    blob = 'ef7a88a3c4c2540569b9ce6f204d8e4750875501'
    note = 'git show pinned object; LF normalization; namespaces isolated'
} | ConvertTo-Json
[System.IO.File]::WriteAllText((Join-Path $Root 'source-origin.json'), $Origin, $Utf8)
```

### Program.cs — literal TRAIN/EXAM и bounded native lifecycle

TRAIN — 12 source: по три разных идентификатора/числа в четырёх структурных семействах. EXAM — четыре новых текста тех же семейств. Это влияние teacher на выученные ответы; histogram повторяются, **обобщение на независимые семейства не заявляется**. Gold не передаётся Label, FeatureVector, Train или Predict.

```csharp
using System.Diagnostics;
using System.Text.Json;
using ALabel = Ng01.SourceA.ActionLabelGenerator;
using BLabel = Ng01.SourceB.ActionLabelGenerator;

internal sealed record Sample(string Id, string Source, string Expected, string Group);
internal sealed record Answer(string Id, string Expected, string? Actual, bool Correct);

internal static class Program
{
    private static readonly string[] Names =
        ["ADD_NULL_CHECK", "ADD_LOOP", "REFACTOR_EXTRACT", "ADD_METHOD"];
    private static readonly Sample[] Lessons =
    [
        new("T01", "class L1{int F(string? x,int n){if(n is >0)return x.Length;return 0;}}", "ADD_NULL_CHECK", "unrelated-int"),
        new("T02", "class L2{int G(string? a,int count){if(count is >1)return a.Length;return 1;}}", "ADD_NULL_CHECK", "unrelated-int"),
        new("T03", "class L3{int H(string? text,int size){if(size is >2)return text.Length;return 2;}}", "ADD_NULL_CHECK", "unrelated-int"),
        new("T04", "class L4{int F(string? x){if(x==null)return 0;return x.Length;}}", "ADD_METHOD", "null-literal"),
        new("T05", "class L5{int G(string? a){if(a==null)return 1;return a.Length;}}", "ADD_METHOD", "null-literal"),
        new("T06", "class L6{int H(string? text){if(text==null)return 2;return text.Length;}}", "ADD_METHOD", "null-literal"),
        new("T07", "class L7{int F(object? x){if(x is string s)return s.Length;return 0;}}", "ADD_METHOD", "type-pattern"),
        new("T08", "class L8{int G(object? a){if(a is string b)return b.Length;return 1;}}", "ADD_METHOD", "type-pattern"),
        new("T09", "class L9{int H(object? value){if(value is string text)return text.Length;return 2;}}", "ADD_METHOD", "type-pattern"),
        new("T10", "class L10{int F(int? x){if(x is >0)return x.Value;return 0;}}", "ADD_METHOD", "nullable-int"),
        new("T11", "class L11{int G(int? a){if(a is >1)return a.Value;return 1;}}", "ADD_METHOD", "nullable-int"),
        new("T12", "class L12{int H(int? value){if(value is >2)return value.Value;return 2;}}", "ADD_METHOD", "nullable-int")
    ];
    private static readonly Sample[] Queries =
    [
        new("Q1", "class Q1{int Read(string? item,int limit){if(limit is >3)return item.Length;return 3;}}", "ADD_NULL_CHECK", "unrelated-int"),
        new("Q2", "class Q2{int Read(string? item){if(item==null)return 3;return item.Length;}}", "ADD_METHOD", "null-literal"),
        new("Q3", "class Q3{int Read(object? item){if(item is string content)return content.Length;return 3;}}", "ADD_METHOD", "type-pattern"),
        new("Q4", "class Q4{int Read(int? item){if(item is >3)return item.Value;return 3;}}", "ADD_METHOD", "nullable-int")
    ];

    private static int Main(string[] args)
    {
        if (args.Length != 2 || (args[0] != "A" && args[0] != "B"))
        {
            Console.Error.WriteLine("Usage: Ng01Runner A|B C:/xg/NG01/r/attempt-0001");
            return 2;
        }
        var arm = args[0];
        var output = Path.GetFullPath(args[1]);
        if (!Directory.Exists(output))
        {
            Console.Error.WriteLine("The assigned result directory must already exist.");
            return 2;
        }
        var rawPath = Path.Combine(output, "arm-" + arm + ".json");
        var failurePath = Path.Combine(output, "failure-" + arm + ".json");
        if (File.Exists(rawPath) || File.Exists(failurePath))
        {
            Console.Error.WriteLine("Existing arm artifacts must not be overwritten.");
            return 2;
        }

        var stage = "initialize";
        var featureIssued = 0; var featureCompleted = 0;
        var labelIssued = 0; var labelCompleted = 0;
        var trainIssued = 0; var trainCompleted = 0;
        var predictIssued = 0; var predictCompleted = 0;
        var logicalQueries = 0; var guardBlocked = 0;
        try
        {
            var a = arm == "A" ? new Ng01.SourceA.FileActionKnnPredictor(3) : null;
            var b = arm == "B" ? new Ng01.SourceB.FileActionKnnPredictor(3) : null;
            Func<string, float[]> feature = arm == "A" ? ALabel.FeatureVector : BLabel.FeatureVector;
            Func<string, int> label = arm == "A" ? ALabel.Label : BLabel.Label;
            Func<int> count = arm == "A" ? () => a!.TrainingSize : () => b!.TrainingSize;
            Action<IReadOnlyList<(float[] Features, int Label)>> train =
                arm == "A" ? a!.Train : b!.Train;
            Func<float[], int> predict = arm == "A" ? a!.Predict : b!.Predict;

            float[] Features(string source)
            {
                featureIssued++;
                var value = feature(source);
                featureCompleted++;
                return value;
            }
            string? Read(float[] vector)
            {
                logicalQueries++;
                if (count() < 4) { guardBlocked++; return null; }
                predictIssued++;
                var value = predict(vector);
                predictCompleted++;
                return Names[value];
            }

            stage = "exam-features";
            var queryVectors = Queries.Select(query => Features(query.Source)).ToArray();
            stage = "before";
            var rowsBefore = count();
            var before = new List<Answer>();
            for (var i = 0; i < Queries.Length; i++)
            {
                var actual = Read(queryVectors[i]);
                before.Add(new(Queries[i].Id, Queries[i].Expected, actual,
                    actual == Queries[i].Expected));
            }

            stage = "training";
            using var process = Process.GetCurrentProcess();
            var stopwatch = new Stopwatch();
            process.Refresh();
            var cpuBefore = process.TotalProcessorTime.TotalMilliseconds;
            var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            var collectionsBefore = GC.CollectionCount(0);
            stopwatch.Start();
            var rows = new (float[] Features, int Label)[Lessons.Length];
            for (var i = 0; i < Lessons.Length; i++)
            {
                var vector = Features(Lessons[i].Source);
                labelIssued++;
                var target = label(Lessons[i].Source);
                labelCompleted++;
                rows[i] = (vector, target);
            }
            trainIssued++;
            train(rows);
            trainCompleted++;
            stopwatch.Stop();
            var allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
            var gen0Collections = GC.CollectionCount(0) - collectionsBefore;
            process.Refresh();
            var cpuMilliseconds = process.TotalProcessorTime.TotalMilliseconds - cpuBefore;
            var rowsAfter = count();

            stage = "after";
            var after = new List<Answer>();
            for (var i = 0; i < Queries.Length; i++)
            {
                var actual = Read(queryVectors[i]);
                after.Add(new(Queries[i].Id, Queries[i].Expected, actual,
                    actual == Queries[i].Expected));
            }

            stage = "record";
            var result = new
            {
                experiment = "NG01", arm,
                sourceCommit = "0b76ab950d82534f5904005dda096545505ae307",
                domain = "C# programming; nullable receiver guard",
                learner = "native source-copy FileActionKnnPredictor",
                model = new { k = 3, expectedFeatureDimensions = 30,
                    neuralLayers = 0, trainableNeuralLayers = 0, optimizerSteps = 0 },
                rowsBefore, rowsAfter,
                training = Lessons.Select((sample, i) => new
                {
                    sample.Id, sample.Source, sample.Group, sample.Expected,
                    actualLabel = Names[rows[i].Label],
                    labelCorrect = Names[rows[i].Label] == sample.Expected,
                    features = rows[i].Features
                }).ToArray(),
                queryData = Queries, queryVectors, before, after,
                correctBefore = before.Count(value => value.Correct),
                correctAfter = after.Count(value => value.Correct),
                cost = new
                {
                    measuredRegion = "12 native FeatureVector + 12 native Label + 1 native Train",
                    wallMilliseconds = stopwatch.Elapsed.TotalMilliseconds,
                    cpuMilliseconds, allocatedBytes, gen0Collections,
                    storedFeatureAndLabelPayloadBytes =
                        rows.Sum(row => (long)row.Features.Length * sizeof(float) + sizeof(int)),
                    retainedHeapBytes = (long?)null,
                    processPeakBytes = (long?)null,
                    vramBytes = (long?)null
                },
                calls = new
                {
                    featureIssued, featureCompleted, labelIssued, labelCompleted,
                    trainIssued, trainCompleted, predictIssued, predictCompleted,
                    logicalQueries, guardBlocked, generateCalls = 0,
                    compileFacadeCalls = 0, warmupCalls = 0, retryCalls = 0
                },
                scope = "family recall, not unseen-family generalization; full facade not run"
            };
            WriteNew(rawPath, result);
            return 0;
        }
        catch (Exception error)
        {
            WriteNew(failurePath, new
            {
                experiment = "NG01", arm, stage, error = error.ToString(),
                featureIssued, featureCompleted, labelIssued, labelCompleted,
                trainIssued, trainCompleted, predictIssued, predictCompleted,
                logicalQueries, guardBlocked
            });
            Console.Error.WriteLine(error);
            return 1;
        }
    }

    private static void WriteNew(string path, object value)
    {
        using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(stream, value, new JsonSerializerOptions { WriteIndented = true });
    }
}
```

### Run.ps1 — одна сборка, один процесс A, один процесс B

```powershell
$ErrorActionPreference = 'Stop'
$Root = 'C:/xg/NG01'
$Result = 'C:/xg/NG01/r/attempt-0001'
if (Test-Path -LiteralPath $Result) {
    throw 'Attempt already exists; preserve all partial results and do not retry.'
}
[System.IO.Directory]::CreateDirectory($Result) | Out-Null
$Utf8 = [System.Text.UTF8Encoding]::new($false)
function Log-Stage([string]$Stage, [string]$State, [int]$Code) {
    $Event = @{ stage = $Stage; state = $State; exitCode = $Code; utc = [DateTime]::UtcNow.ToString('O') } | ConvertTo-Json -Compress
    [System.IO.File]::AppendAllText((Join-Path $Result 'events.jsonl'), $Event + [Environment]::NewLine, $Utf8)
}
function Finish([string]$Stage, [int]$Code) {
    $Value = @{ experiment = 'NG01'; stage = $Stage; exitCode = $Code } | ConvertTo-Json
    [System.IO.File]::WriteAllText((Join-Path $Result 'status.json'), $Value, $Utf8)
}
Log-Stage 'build' 'issued' -1
& dotnet build (Join-Path $Root 'Runner.csproj') -c Release -v minimal -m:1 -p:ImportDirectoryBuildProps=false -p:ImportDirectoryBuildTargets=false 1> (Join-Path $Result 'build.stdout') 2> (Join-Path $Result 'build.stderr')
$Code = $LASTEXITCODE
Log-Stage 'build' 'completed' $Code
if ($Code -ne 0) { Finish 'build-failed' $Code; exit $Code }
$Dll = Join-Path $Root 'bin/Release/net9.0/Ng01Runner.dll'
Log-Stage 'A' 'issued' -1
& dotnet $Dll A $Result 1> (Join-Path $Result 'A.stdout') 2> (Join-Path $Result 'A.stderr')
$Code = $LASTEXITCODE
Log-Stage 'A' 'completed' $Code
if ($Code -ne 0) { Finish 'A-failed' $Code; exit $Code }
Log-Stage 'B' 'issued' -1
& dotnet $Dll B $Result 1> (Join-Path $Result 'B.stdout') 2> (Join-Path $Result 'B.stderr')
$Code = $LASTEXITCODE
Log-Stage 'B' 'completed' $Code
if ($Code -ne 0) { Finish 'B-failed' $Code; exit $Code }
Finish 'A-and-B-completed' 0
```

Если shell завершился до status, stdout/stderr и последняя issued/completed запись остаются evidence; не повторять процесс для «полного» журнала. Сбой build/restore не разрешает чинить production или искать другой проект.

### Единственные команды будущего исполнения

```powershell
pwsh -NoProfile -File C:/xg/NG01/Prepare.ps1
pwsh -NoProfile -File C:/xg/NG01/Run.ps1
```

Команды последовательны. После неуспешного Prepare Run не выполнять. Никаких dotnet test, полного 35 build, warmup, повторов, B→A, «equivalence run» продукта или нового экзамена reviewer.

## 4. Бюджет и метрики

| Операция | На плечо | Всего A+B |
|---|---:|---:|
| Учебных source | 12 | 24 |
| Native Label | 12 | 24 |
| FeatureVector на TRAIN | 12 | 24 |
| FeatureVector на EXAM, переиспользуются до/после | 4 | 8 |
| Native Train | 1 | 2 |
| Logical queries до/после | 4+4 | 16 |
| Закрыто guard до Train | 4 | 8 |
| Native Predict после Train | 4 | 8 |
| Optimizer / Generate / Compile / warmup / retry | 0 | 0 |

C# snippets парсятся, но не компилируются и не исполняются как программы. Один build host учитывается отдельно от runtime cost. Effort подготовки/build не прибавляется к Train скрыто. Лимит **120 секунд на build и 30 секунд на плечо** наблюдает исполнитель; при превышении останавливает только собственный процесс и сохраняет partial. Незавершённый этап не является accuracy 0. Второй параллельный процесс не запускать.

Основные оси: правильность фактически использованных TRAIN labels; ответы Q1–Q4 до/после; false negative ADD_NULL_CHECK на Q1; false positive на Q2–Q4; сохранность трёх контрольных guards. ADD_METHOD на controls означает оставшуюся категорию действующей policy и отсутствие ложного ADD_NULL_CHECK, не доказанную полезность добавления метода.

Allocated bytes — managed выделения в указанном training region. Feature/label payload — нижняя оценка полезного содержимого, не retained heap или peak RSS. Полный peak, retained heap, VRAM и end-to-end RP01 quality не измеряются. Один локальный learner и guard adapter; полный Unit/Flow не пройден. Нейронных слоёв и optimizer updates нет.

Ожидание по source — изменятся три labels unrelated-int, controls останутся прежними. Это не результат. При одинаковых ответах A/B записать отсутствие query gain, даже если teacher labels исправлены. Полезный measured label-quality gain относится к реально использованной supervision, не автоматически к качеству output.

## 5. Отчёт и кандидат без поиска формата

**Русский Markdown:** до 350 слов пояснений плюс таблицы фактов и ссылки на raw. Файлы **C:/xg/NG01/r/attempt-0001/report.md** и **candidate.md**. Raw arm-A.json/arm-B.json, failure, logs, events и status не редактируются. Для отчёта не пересчитывать Label/FeatureVector/Predict, hashes и не повторять опыт.

Навыки из **C:/Projects/llm_mvp_core/docs/skills**:

| Навык | Что применяется |
|---|---|
| architect-behavior-classifier-SKILL.md | Teacher → learned state → reader |
| planning-session-startup-SKILL.md | Назначенная фаза/пути/бюджет |
| plan-review-SKILL.md | Один фактор и controls |
| generated-code-review-SKILL.md | Один просмотр source-copy diff до исполнения |
| prompt-patch-authoring-SKILL.md | Не менять другой bottleneck/данные |
| output-readability-gate-SKILL.md | Понятный русский отчёт |
| human-communication-GLOSSARY.md | Домен C#, механизм kNN |

Если нужные разделы уже прочитаны и не менялись, повторный полный обход навыков не нужен.

```markdown
# NG01: влияние числового pattern на обучение null-guard совета
Статус: <завершён / частичный / техническая ошибка>.
Домен: программирование C#. Исполнитель: <фактическая default модель>.
Проверка: <отдельный reviewer или самопроверка; без новых запусков>.
Источник: commit 0b76ab9..., FileActionKnn.cs::ActionLabelGenerator.Label.
Raw: arm-A.json, arm-B.json, status.json; отсутствующее — явно.

## Что сравнивали
A — исходный label. B — исключён direct relational pattern параметра int.
Один Train на 12 source; те же 30 features/k3 и четыре family-recall запроса.

## Фактические ответы
| Query | A до | A после | B до | B после | Gold |
|---|---|---|---|---|---|
| Q1, null risk | | | | | ADD_NULL_CHECK |
| Q2, null literal | | | | | ADD_METHOD по текущей policy |
| Q3, type pattern | | | | | ADD_METHOD по текущей policy |
| Q4, nullable int | | | | | ADD_METHOD по текущей policy |

## Полезный эффект и ухудшения
<Correct TRAIN labels, actual output, allocated bytes, wall/CPU наблюдения.
Любое полезное измеренное улучшение — успех своей метрики/домена;
рост accuracy не обязателен. Все ухудшения рядом.
Неизмеренное — «не измерено», не ноль.>

## Граница и кандидат
<Не полный null analyzer, не unseen-family generalization, не доказательство RP01.
Точный helper/condition; production не менялся.
Calls issued/completed и лишние операции отдельно; no automatic transfer.>
```

candidate.md — до 120 слов: source method, один diff из раздела 2/Prepare, ссылка на report/raw, фактический useful gain, регрессии и статус «кандидат; решение о переносе за пользователем». Без результата это непроверенная гипотеза, успех не присваивается. Один раз прочитать отчёт и исправить только фактическую ошибку/ссылку/кодировку; не продолжать оформление по кругу.

## 6. Проверка документа и пределы

Source-copy API и mutation просмотрены статически. Конкретная версия NG01 не собиралась; parse/runtime, equality controls и качество не проверены. Текущий документный аудит запрещает такой запуск. Будущий исполнитель не меняет рецептуру при неожиданном результате: сохраняет его и завершает назначенный бюджет.

Этот паспорт сохраняет точный source, один фактор, настоящее обучение памяти, ответы до/после, short paths, immutable results и немедленный STOP/NEXT. Полная production цепочка не требуется для проверки гипотезы.

