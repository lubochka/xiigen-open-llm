# KH-COST-01 — одно синтаксическое дерево для двух частей учебного примера

**Статус: PROJECT / NOT_RUN.** Этот документ не утверждает нового измеренного выигрыша и не назначает повтор N-KU-01. Сейчас документируются результаты и кандидаты. Ниже полностью указан отдельный возможный будущий опыт; исполнять его только когда он выбран как следующий опыт.

## Смысл, домен и вмешательство

Домен: программирование на C#. Задача: выбор совета по файлу. Обучаемый получатель: память `FileActionKnnPredictor`. В `SemanticCompilerFacade.TrainFileActionKnn` исходный код каждого урока проходит `FeatureVector(source)` и `Label(source)`; оба метода заново вызывают Roslyn ParseText. Подготовленные признаки и метка затем действительно передаются в `FileActionKnnPredictor.Train`. [Caller L194–203](https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/12.SemanticCompiler/SemanticCompilerFacade.cs#L194-L203); [две parse-точки L21–25 и L75–85](https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/12.SemanticCompiler/Prediction/FileActionKnn.cs#L21-L85).

**A:** точная копия существующего file-action механизма, две parse-точки на один урок. **B:** то же вычисление Label/FeatureVector с одним общим SyntaxNode root на урок. Меняется только повторное построение AST в учебном входе. После подготовки оба плеча вызывают настоящий Train, затем одинаковый learned reader.

Ожидаемый полезный показатель — стоимость подготовки и записи учебных строк: allocated bytes/CPU/wall time. Это настоящая часть процесса обучения. Сама арифметика «8 parse-call sites→4» — source explanation, не измеренный выигрыш. Рост accuracy для успеха по стоимости не нужен. **Любое измеренное улучшение полезной метрики считать успехом по этой метрике и C# домену; одновременно показать все ухудшения.** Однократный wall-time шум отметить, не превращая его в новое условие, запрещающее документирование полезного результата.

## Точные места и запреты на смешение факторов

Рабочий root будущего Windows-исполнителя: `C:/Projects/llm_mvp_core`. Новые файлы только:

- `35.ExperimentsV2/Experiments/Rp01Micro/KhCost01/VariantA/FileActionKnn.cs`;
- `35.ExperimentsV2/Experiments/Rp01Micro/KhCost01/VariantB/FileActionKnn.cs`;
- `experiments-v2/RP01-MICRO/KH-COST-01/KhCost01Runner.csproj`, `Program.cs`, `Prepare.ps1`, `Run.ps1`, `provenance.json`, `card.md`, `results/A.json`, `results/B.json`, `results/report.md`, собственные bin/obj.

Original source — `12.SemanticCompiler/Prediction/FileActionKnn.cs`, pinned commit `9622b575a6aabb79939b12cc9f5d96313f1bf370`, Git blob `ef7a88a3c4c2540569b9ce6f204d8e4750875501`. Он только читается. Не меняются production, Tests, CI, solution, 35.csproj, dispatcher, навыки или защищённый `docs/plans/replanning-possible-diffs`. Существующий N-KU-01 и его результаты не перезаписываются.

В A/B сохраняется исходная DoStatement label-проблема: этот опыт не переносит уже успешную правку N-KU-01 и не проверяет её заново. Сохраняются30 признаков, все labels, normalization, k=3, distance, voting, word/source order. Не объединять с одно-проходным histogram — это ещё один возможный фактор, вне данной карточки.

## Данные и budget

Четыре TRAIN source в заданном порядке:

```csharp
class C { static int Sum(int[] xs,int n){int i=0,s=0;do{s+=xs[i];i++;}while(i<n);return s;} }
class C { static int CountPositive(int[] xs,int n){int i=0,c=0;do{if(xs[i]>0)c++;i++;}while(i<n);return c;} }
class C { static int Product(int[] xs,int n){int p=1;for(int i=0;i<n;i++){p*=xs[i];}return p;} }
class C { static int CountZero(int[] xs,int n){return 0;} }
```

Четыре EXAM source:

```csharp
class C { static int SumSquares(int[] xs,int n){int i=0,s=0;do{s+=xs[i]*xs[i];i++;}while(i<n);return s;} }
class C { static bool AnyNegative(int[] xs,int n){int i=0;do{if(xs[i]<0)return true;i++;}while(i<n);return false;} }
class C { static int CountOdd(int[] xs,int n){int i=0,c=0;while(i<n){if(xs[i]%2!=0)c++;i++;}return c;} }
class C { static int SumEven(int[] xs,int n){int s=0;return s;} }
```

Gold предметного контроля: для первых трёх любой непустой совет, кроме ADD_LOOP; для четвёртого ADD_LOOP. Эти fixed fixtures служат контролем неизменности уже известного механизма, а не новым обучающим корпусом для свободного обобщения. При неизменной задаче и source передача одних и тех же fixtures в отдельный cost опыт допустима; не считать это второй независимой репликацией качества N-KU-01.

| Операция | На плечо | Всего |
|---|---:|---:|
| Создание пустой KNN памяти | 1 | 2 |
| TRAIN presentations | 4 | 8 |
| Label учебного source / FeatureVector учебного source | 4 /4 | 8 /8 |
| Native Train | 1 | 2 |
| BEFORE запросы, закрытые `<4` guard | 4 | 8 |
| AFTER запросы → FeatureVector → native Predict | 4 | 8 |
| Optimizer steps / text generation / EXAM Label / Compile | 0 | 0 |

Малый exe собирается отдельно, A→B выполняется в двух свежих процессах. Допустима локальная починка собственных экспериментальных файлов и повторная сборка без изменения фактора, данных, метрик или бюджета реального обучения. Нет дополнительного прогрева, product parity-training, reverse-order повторов, accuracy rerun, brute-force distance расчётов или полного test suite. Если измерение времени шумное, записать это; не добавлять самовольно итерации. Весь receipt включает BEGIN/END времени подготовки+Train, total process CPU delta и allocations текущего синхронного потока; query затраты отдельно не смешиваются с train интервалом.

## Подготовка копий: точное преобразование

Создать A из original, заменив только namespace на `ExperimentsV2.Rp01Micro.KhCost01.VariantA`. B — та же копия с namespace `ExperimentsV2.Rp01Micro.KhCost01.VariantB` и двумя преобразованиями ниже. Остальное тело файла побуквенно сохраняется.

Исходное начало Label:

```csharp
    public static int Label(string csharpSource)
    {
        var root = CSharpSyntaxTree.ParseText(csharpSource).GetRoot();
```

Заменить в B на:

```csharp
    public static int Label(string csharpSource)
        => Label(CSharpSyntaxTree.ParseText(csharpSource).GetRoot());

    public static int Label(Microsoft.CodeAnalysis.SyntaxNode root)
    {
```

Исходное начало FeatureVector:

```csharp
    public static float[] FeatureVector(string csharpSource)
    {
        var root = CSharpSyntaxTree.ParseText(csharpSource).GetRoot();
```

Заменить в B на:

```csharp
    public static float[] FeatureVector(string csharpSource)
        => FeatureVector(CSharpSyntaxTree.ParseText(csharpSource).GetRoot());

    public static float[] FeatureVector(Microsoft.CodeAnalysis.SyntaxNode root)
    {
```

Сохранить оригинальные bytes и hashes source/card/A/B/runner в provenance до первого запуска. При изменении HEAD проверять именно названный original blob и фиксированную карточку, а не запрещать весь опыт из-за чужой документации. Если source blob изменён, не смешивать snapshot: записать конфликт `SOURCE_CHANGED` и не запускать незаявленную версию. Если назначенные A/B/runner/results уже существуют, не перезаписывать и не искать новый ID: использовать только собственную незавершённую подготовку, если её provenance совпадает; иначе сообщить точный конфликт. Исключения лаборатории не переносятся в production.

### Полный Prepare.ps1

Сохранить эту карточку также как `experiments-v2/RP01-MICRO/KH-COST-01/card.md`; это копия документа, не изменение старых карточек. Сохранить приведённые ниже Program/csproj/Run до исполнения. `Prepare.ps1` создаёт только названные копии. Проверка source не запускает Roslyn, Train или Predict.

```powershell
$ErrorActionPreference='Stop'
$khRoot='C:/Projects/llm_mvp_core'
$khBlob='ef7a88a3c4c2540569b9ce6f204d8e4750875501'
$khUtf8=[System.Text.UTF8Encoding]::new($false)
$khStart=[Diagnostics.ProcessStartInfo]::new('git')
$khStart.UseShellExecute=$false
$khStart.RedirectStandardOutput=$true
$khStart.RedirectStandardError=$true
$khStart.StandardOutputEncoding=$khUtf8
foreach($khArg in @('-C',$khRoot,'cat-file','blob',$khBlob)) { $khStart.ArgumentList.Add($khArg) }
$khProcess=[Diagnostics.Process]::Start($khStart)
$khOriginal=$khProcess.StandardOutput.ReadToEnd()
$khError=$khProcess.StandardError.ReadToEnd()
$khProcess.WaitForExit()
if($khProcess.ExitCode -ne 0) { throw "PINNED_BLOB_UNAVAILABLE: $khError" }
$khWorking=[IO.File]::ReadAllText("$khRoot/12.SemanticCompiler/Prediction/FileActionKnn.cs")
if($khWorking.Replace("`r`n","`n") -cne $khOriginal.Replace("`r`n","`n")) { throw 'SOURCE_CHANGED: FileActionKnn.cs' }
$khA=$khOriginal.Replace('namespace SemanticCompiler.Prediction;','namespace ExperimentsV2.Rp01Micro.KhCost01.VariantA;')
$khB=$khOriginal.Replace('namespace SemanticCompiler.Prediction;','namespace ExperimentsV2.Rp01Micro.KhCost01.VariantB;')
$khLabelOld="    public static int Label(string csharpSource)`n    {`n        var root = CSharpSyntaxTree.ParseText(csharpSource).GetRoot();"
$khLabelNew="    public static int Label(string csharpSource)`n        => Label(CSharpSyntaxTree.ParseText(csharpSource).GetRoot());`n`n    public static int Label(Microsoft.CodeAnalysis.SyntaxNode root)`n    {"
$khFeatureOld="    public static float[] FeatureVector(string csharpSource)`n    {`n        var root = CSharpSyntaxTree.ParseText(csharpSource).GetRoot();"
$khFeatureNew="    public static float[] FeatureVector(string csharpSource)`n        => FeatureVector(CSharpSyntaxTree.ParseText(csharpSource).GetRoot());`n`n    public static float[] FeatureVector(Microsoft.CodeAnalysis.SyntaxNode root)`n    {"
if(!$khB.Contains($khLabelOld) -or !$khB.Contains($khFeatureOld)) { throw 'SOURCE_SHAPE_CHANGED' }
$khB=$khB.Replace($khLabelOld,$khLabelNew).Replace($khFeatureOld,$khFeatureNew)
foreach($khArm in @('A','B')) {
 $khDest="$khRoot/35.ExperimentsV2/Experiments/Rp01Micro/KhCost01/Variant$khArm/FileActionKnn.cs"
 $khText=if($khArm -eq 'A') {$khA} else {$khB}
 [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($khDest)) | Out-Null
 if(Test-Path $khDest) {
  if([IO.File]::ReadAllText($khDest) -cne $khText) { throw "PATH_CONFLICT: $khDest" }
 } else { [IO.File]::WriteAllText($khDest,$khText,$khUtf8) }
}
```

Если pinned blob отсутствует локально, разрешён read-only `git fetch origin 9622b575a6aabb79939b12cc9f5d96313f1bf370`; недоступный origin фиксируется как access problem. Другой snapshot не подставлять. После успешной сборки и до A записать `provenance.json` с SHA256 `card.md`, `Prepare.ps1`, `Run.ps1`, `Program.cs`, csproj, original working bytes и A/B copies, pinned commit/blob, UTC и `dotnet --info`. При повторной собственной подготовке existing identical source copies допустимы. Existing results не перезаписываются; changing source/fixture/factor под тем же ID не разрешено.

## Полный csproj малого host

`experiments-v2/RP01-MICRO/KH-COST-01/KhCost01Runner.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
    <AssemblyName>KhCost01Runner</AssemblyName>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.12.0" />
    <Compile Include="Program.cs" />
    <Compile Include="../../../35.ExperimentsV2/Experiments/Rp01Micro/KhCost01/VariantA/FileActionKnn.cs" />
    <Compile Include="../../../35.ExperimentsV2/Experiments/Rp01Micro/KhCost01/VariantB/FileActionKnn.cs" />
  </ItemGroup>
</Project>
```

Без ProjectReference: host компилирует только названные copies и один Program; сборка всей35/12 цепочки не нужна.

## Полный Program.cs

Код ниже — текст будущей карточки; в этом аудите не компилировался и не запускался. Namespace aliases явно связывают выбранные копии. Runtime не вызывает Label на EXAM и не пересчитывает TRAIN для отчёта.

```csharp
using System.Diagnostics;
using System.Text.Json;
using Microsoft.CodeAnalysis.CSharp;
using AGenerator = ExperimentsV2.Rp01Micro.KhCost01.VariantA.ActionLabelGenerator;
using APredictor = ExperimentsV2.Rp01Micro.KhCost01.VariantA.FileActionKnnPredictor;
using BGenerator = ExperimentsV2.Rp01Micro.KhCost01.VariantB.ActionLabelGenerator;
using BPredictor = ExperimentsV2.Rp01Micro.KhCost01.VariantB.FileActionKnnPredictor;

if (args.Length != 2 || (args[0] != "A" && args[0] != "B"))
    throw new ArgumentException("Usage: KhCost01Runner A|B <new-result-json>");
var arm = args[0];
var output = Path.GetFullPath(args[1]);
if (File.Exists(output)) throw new IOException("Result exists: " + output);
Directory.CreateDirectory(Path.GetDirectoryName(output)!);

string[] train = [
 "class C { static int Sum(int[] xs,int n){int i=0,s=0;do{s+=xs[i];i++;}while(i<n);return s;} }",
 "class C { static int CountPositive(int[] xs,int n){int i=0,c=0;do{if(xs[i]>0)c++;i++;}while(i<n);return c;} }",
 "class C { static int Product(int[] xs,int n){int p=1;for(int i=0;i<n;i++){p*=xs[i];}return p;} }",
 "class C { static int CountZero(int[] xs,int n){return 0;} }"
];
string[] exam = [
 "class C { static int SumSquares(int[] xs,int n){int i=0,s=0;do{s+=xs[i]*xs[i];i++;}while(i<n);return s;} }",
 "class C { static bool AnyNegative(int[] xs,int n){int i=0;do{if(xs[i]<0)return true;i++;}while(i<n);return false;} }",
 "class C { static int CountOdd(int[] xs,int n){int i=0,c=0;while(i<n){if(xs[i]%2!=0)c++;i++;}return c;} }",
 "class C { static int SumEven(int[] xs,int n){int s=0;return s;} }"
];
Func<string,float[]> feature;
Func<float[],int> predict;
Action<IReadOnlyList<(float[] Features,int Label)>> learn;
Func<int> size;
if (arm == "A") {
    var memory = new APredictor(k:3);
    feature = AGenerator.FeatureVector; predict = memory.Predict;
    learn = memory.Train; size = () => memory.TrainingSize;
} else {
    var memory = new BPredictor(k:3);
    feature = BGenerator.FeatureVector; predict = memory.Predict;
    learn = memory.Train; size = () => memory.TrainingSize;
}
int queryCalls=0, actualPredicts=0, guardNulls=0, trainCalls=0;
string? Query(string source) {
    queryCalls++;
    if(size()<4) { guardNulls++; return null; }
    actualPredicts++;
    return AGenerator.Labels[predict(feature(source))];
}
bool Correct(int i,string? answer) => !string.IsNullOrWhiteSpace(answer)
    && (i<3 ? answer!="ADD_LOOP" : answer=="ADD_LOOP");
var before=exam.Select(Query).ToArray();
var beforeSize=size();
using var process=Process.GetCurrentProcess();
var cpuBefore=process.TotalProcessorTime;
var allocBefore=GC.GetAllocatedBytesForCurrentThread();
var begin=Stopwatch.GetTimestamp();
var rows=new List<(float[] Features,int Label)>(train.Length);
foreach(var source in train) {
    if(arm=="A") rows.Add((AGenerator.FeatureVector(source), AGenerator.Label(source)));
    else {
        var root=CSharpSyntaxTree.ParseText(source).GetRoot();
        rows.Add((BGenerator.FeatureVector(root), BGenerator.Label(root)));
    }
}
if(rows.Count>=4) { learn(rows); trainCalls++; }
var trainMs=Stopwatch.GetElapsedTime(begin).TotalMilliseconds;
var allocatedBytes=GC.GetAllocatedBytesForCurrentThread()-allocBefore;
var cpuMs=(process.TotalProcessorTime-cpuBefore).TotalMilliseconds;
var afterSize=size();
var after=exam.Select(Query).ToArray();
var report=new {
    experiment="KH-COST-01", arm, runtime=Environment.Version.ToString(),
    beforeSize, afterSize, before, after,
    beforeCorrect=before.Where((answer,i)=>Correct(i,answer)).Count(),
    afterCorrect=after.Where((answer,i)=>Correct(i,answer)).Count(),
    falseLoopAdviceAfter=after.Take(3).Count(answer=>answer=="ADD_LOOP"),
    neededLoopRecallAfter=after[3]=="ADD_LOOP" ? 1 : 0,
    trainMs, cpuMs, allocatedBytes,
    trainCalls, queryCalls, actualPredicts, guardNulls,
    trainSources=train, examSources=exam,
    preparedRows=rows.Select((row,i)=>new {id="K"+(i+1),features=row.Features,label=row.Label}).ToArray(),
    expectedParseSitesFromSourceDuringTrain=arm=="A" ? 8 : 4,
    scope="Source->Label+FeatureVector->native Train->guarded native Predict; no Compile"
};
File.WriteAllText(output, JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));
```

Короткие команды будущего `Run.ps1`:

```powershell
$khRoot='C:/Projects/llm_mvp_core'
$khHost="$khRoot/experiments-v2/RP01-MICRO/KH-COST-01"
pwsh -NoProfile -File "$khHost/Prepare.ps1"
if ($LASTEXITCODE -ne 0) { throw 'Prepare failed; resolve only the named conflict' }
dotnet build "$khHost/KhCost01Runner.csproj" -c Release --nologo -p:ImportDirectoryBuildProps=false -p:ImportDirectoryBuildTargets=false
if ($LASTEXITCODE -ne 0) { throw 'KH-COST-01 build failed; fix only this host without changing factor/data/scoring, then rebuild before any Train' }
$khDll="$khHost/bin/Release/net9.0/KhCost01Runner.dll"
dotnet $khDll A "$khHost/results/A.json"
if ($LASTEXITCODE -ne 0) { throw 'KH-COST-01 A failed; preserve outputs' }
dotnet $khDll B "$khHost/results/B.json"
if ($LASTEXITCODE -ne 0) { throw 'KH-COST-01 B failed; preserve outputs' }
```

Вывод сборки/stdout/stderr и exit codes сохранить рядом, не переиспользуя старые логи. Если отсутствует .NET9/Roslyn4.12, записать `ENVIRONMENT_UNAVAILABLE`. Ошибки собственного host исправлять локально, сохраняя фактор, fixtures, scoring и cap реальных вызовов; повторная сборка не считается новым learning run и отдельного разрешения на неё не нужно. Не чинить production/соседние проектные тесты и не подменять experiment Python-симуляцией. После частично выполненного run не повторять уже учтённые Train/query: сохранить факт и частичный receipt, остановить runtime этап без увеличения cap.

## Отчёт без скрытого дополнительного запуска

Читать A.json/B.json и source diff, без вызовов learner/parser. Проверить одинаковость trainSources, examSources, preparedRows.features/labels, before/after answers; эта проверка не новое измерение поведения. Если семантика различается, factor не изолирован: документировать расхождение, не объявлять выигрыш объяснённым reuse AST.

Таблица результата: BEFORE/AFTER useful answer metrics для A и B; `B.trainMs−A.trainMs`, `B.cpuMs−A.cpuMs`, `B.allocatedBytes−A.allocatedBytes`; все числители/знаменатели; process/runtime; transcript и provenance. `expectedParseSitesFromSourceDuringTrain` — статически ожидаемые sites, не инструментальный счётчик скрытых Roslyn операций. Счётчик allocations включает строки/tuples/списки подготовки и learner memory; он не peak resident memory. CPU ниже разрешения таймера обозначать явно. Не выдавать отсутствие роста accuracy за неуспех стоимости.

Окончание: сохранить отчёт, diff-кандидат original при положительном useful delta, ограничения и остальные дельты. Production не исправлять; следующие действия выбирает пользователь. Успешный новый cost результат не отменяет независимый уже сохранённый N-KU-01 label improvement.

## Промпт исполнителю

> Выполни только выбранный KH-COST-01 по этому сохранённому документу. Цель — измерить стоимость реальной подготовки+Train FileAction memory при одном parse вместо двух; полезный выигрыш времени/CPU/allocations считается успехом этой метрики в C#, рост accuracy не обязателен. Сохрани все остальные изменения метрик и шум однократного timing. Положительный N-KU-01 не повторяй и не редактируй. Копии, overload преобразования, host, fixtures, run commands, counts и конфликтные ситуации полностью заданы выше. Не меняй исходный продукт,35.csproj, protected папку, тесты, CI или настройки агента. Допустимы подготовка и локальная починка собственного host с повторной сборкой при неизменном факторе; два fresh процесса, максимум2Train/8TRAIN presentations/16queries; без дополнительных warmup/parity/BA runs. Проведи только source/JSON review полученного результата, добавь кандидат исправления и оставь пользователю решение о полном внедрении.
