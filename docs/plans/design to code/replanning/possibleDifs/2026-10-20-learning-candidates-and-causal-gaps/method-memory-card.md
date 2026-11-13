# KH-EXT-01 — сохранность обученной MethodKnn памяти при следующем sparse batch

**Статус: EXTENSION_OF_KNOWN_MECHANISM / SOURCE_CONFIRMED / NOT_RUN.** Сейчас только документирование. Карточка не назначает новый запуск, не утверждает измеренного выигрыша и не разрешает внедрение в production.

## 1. Что известно и что проверит опыт

**Домен:** программирование на C#. **Задача:** получить сохранённый прогноз типа следующего метода после последовательного обучения. **Получатель:** `MethodKnnPredictor._pairs`. **Граница результата:** поле `CompilationResult.PredictedNextMethodType`; свободный текстовый ответ и RP01 Student не измеряются.

[LT-N02 L48](https://github.com/lubochka/xiigen-planning/blob/de270758087502a350c4fcf460acdf1638bdd318/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/47-LANGUAGE-TRANSLATE-INTERNALS-AND-NEW-HYPOTHESES-2026-10-19/03-NEW-KNN-HYPOTHESES.md#L48) уже описал несовпадение методов и пар: TRAIN допускает ≥4 метода, reader требует ≥4 пары. Это prior art. Уточнение этой карточки — **T1 выучил непустую память → T2 из четырёх одно-методных файлов проходит старый допуск → Clear стирает T1 → reader перестаёт отвечать**. Новая политика B отклоняет T2 до Clear, когда пригодных пар меньше прежнего reader minimum4; обучение T1 остаётся настоящим.

[Facade TRAIN L178–190](https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/12.SemanticCompiler/SemanticCompilerFacade.cs#L178-L190) вызывает Train по числу methods. [Train L120–130](https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/12.SemanticCompiler/Prediction/MethodKnn.cs#L120-L130) сначала очищает `_pairs`, затем отбирает пары по FileId и соседним IndexInFile. HasTrainingData, exception или дополнительного admission внутри Train нет. [Reader L313–323](https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/12.SemanticCompiler/SemanticCompilerFacade.cs#L313-L323) возвращает null при TrainingSize<4. [TrainingSize L187](https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/12.SemanticCompiler/Prediction/MethodKnn.cs#L187-L189) считает пары. [Compile L158–172](https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/12.SemanticCompiler/SemanticCompilerFacade.cs#L155-L172) помещает результат reader в публичное поле.

`UseMethodKnn=true` по [default Options L17–18](https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/12.SemanticCompiler/SemanticCompilerOptions.cs#L17-L18). Это не доказывает, что RP01 обучает этот экземпляр. Установлены публичный training entry и обычный consumer; отдельный production caller, последовательно вызывающий T1 и T2 на одном facade, в прочитанном подмножестве исходников не установлен. Поэтому это условная проблема retention, не доказанная причина RP01 и не универсальный P0. Если владелец API намеренно трактует sparse batch как очистку, требуется иное product-решение. B здесь проверяет явно заданный контракт «недостаточный replacement не уничтожает доступную компетенцию».

## 2. Единственное вмешательство и все места

A использует текущий admission `allMethods.Count >= 4`. B перед Train считает **то же** условие пригодной пары, что native Train, и допускает replacement лишь при `eligiblePairs >= 4`. Прежний reader minimum4, k5, native extractor, Jaccard, vote, order, source strings и canonical labels неизменны. Это не append/replay, не исправление While/Do canonical label и не class-boundary эксперимент.

Рабочий root будущего исполнителя: `C:/Projects/llm_mvp_core`. Новые файлы только:

- `35.ExperimentsV2/Experiments/Rp01Micro/KhExt01/Source/MethodKnn.cs` — точная копия pinned native файла с исходным namespace;
- там же `Core12.cs`, `Core29.cs` — точные копии двух простых dependency files с исходными namespaces;
- `experiments-v2/RP01-MICRO/KH-EXT-01/KhExt01Runner.csproj`, `Program.cs`, `Prepare.ps1`, `Run.ps1`, `provenance.json`;
- там же `results/A.json`, `results/B.json`, `results/report.md`, `results/console.txt`, собственные bin/obj.

`Program.cs` содержит единственный лабораторный класс `KhExt01MethodEntry`, который портирует ровно train-entry и reader из facade. Оригинальный большой Compile не вызывается, поскольку он обновляет другие memories. В карточке явно указан этот узкий scope; обычный final-generator эффект не приписывается.

| Только читаемый оригинал | Pinned Git blob |
|---|---|
| `12.SemanticCompiler/Prediction/MethodKnn.cs` | `e0ecebb6d513259a0930e8706737254f926197cd` |
| `12.SemanticCompiler/Core/Core.cs` | `7e68b2ce1526402be24d3a286a7edf31b395d920` |
| `29.SemanticCompiler/Core/Core.cs` | `0dc09344f15816c11f23e3166bfe8e24ff9fada9` |

Все три из core commit `9622b575a6aabb79939b12cc9f5d96313f1bf370`. Сохраняется copyright. Не меняются original, solution, production csproj,35dispatcher, Tests, CI, protected `docs/plans/replanning-possible-diffs`, старые эксперименты или результаты.

## 3. Fixtures, последовательность и бюджет

T1 — один файл с5 методами. Все4 next labels — Loop; native extractor/Train сами создают пары. T2 —4 отдельных файла, по1 методу:9 методов за два batch на плечо, но4 затем0 eligible pairs. Полные строки приведены в Program ниже и являются единственным источником fixtures.

Четыре frozen query sources не входят в TRAIN. Для этой синтетической панели задан контракт следующей стадии Loop. Он не утверждает, что любой реальный C# метод должен сопровождаться циклом. Основная метрика — сохранность полезного доступного ответа T1 после T2, не обобщение выбора среди классов. При k5 и4 пары одного класса T1 прогноз тривиален по метке; эта простота намеренно изолирует потерю памяти.

На каждом плече ровно последовательность **Q0 → T1 → Q1 → T2 → Q2**. Один экземпляр learner живёт на протяжении своего плеча; A и B — разные fresh процессы. Каждая Q —4 query. До T1 guard закрыт. Ни query, ни её gold не поступают в Train. В B T2 отвергнут до native Train; это измеряемый исход admission, а не симулированное обучение.

| Операция | A | B | Всего |
|---|---:|---:|---:|
| Training entry attempts | 2 | 2 | 4 |
| Source presentations T1+T2 | 5 | 5 | 10 |
| Извлечённые методы T1+T2, ожидаемо | 9 | 9 | 18 |
| Native Train, ожидаемо | 2 | 1 | 3 |
| Logical queries | 12 | 12 | 24 |
| Native Predict, ожидаемо | 4 | 8 | 12 |
| Guard-null, ожидаемо | 8 | 4 | 12 |
| Optimizer steps / generation / Compile | 0 | 0 | 0 |

Это cap будущего опыта, а не полученные результаты. Если native extraction не даёт указанные counts, сохранить расхождение и не добавлять уроки ради обхода gate. Нет дополнительных warmup, reverse-order, parity-training, full-suite или перезапуска уже исполненного плеча. Рутинные исправления собственного host и повторная сборка до runtime допустимы при неизменных данных, факторе и scoring; отдельное разрешение на них не требуется.

## 4. Подготовка без выбора мест и конфликтов

Сохранить кодовые блоки csproj/Program/Run ниже в названные файлы. `Prepare.ps1` должен:

1. Проверить три original файла против указанных Git blobs в pinned commit; прочитать именно эти blobs через `git cat-file blob <sha>` с UTF8. В рабочем original допустимы только отличия CRLF/LF. При ином содержимом записать `SOURCE_CHANGED` и остановить runtime этап без выбора другого SHA.
2. Создать три копии из pinned blobs в заданном Source. Namespace и тела не менять. Existing identical files можно использовать как собственную незавершённую подготовку; differing files или чужой provenance — `PATH_CONFLICT`, не overwriting и не поиск другого ID.
3. До первого Train записать `provenance.json`: core SHA, три Git blobs, SHA256 исходных bytes и копий, Program/csproj/Run/card hashes, `dotnet --info`, UTC, ROOT; card должен быть сохранён рядом со своим planning provenance, его SHA256 включить из конкретного установленного документа. Сам вызов `dotnet --info` не обучает модель.
4. Если A.json/B.json уже существует, runtime не запускать и результат не перезаписывать. При partial run сохранить partial receipt/counters, не увеличивать cap.

Блок ниже точно создаёт копии. Он не запускает parser или learner. В `Prepare.ps1` пути жёстко заданы; этот код не должен быть заменён поиском «подходящего экспериментального проекта».

```powershell
$ErrorActionPreference='Stop'
$khRoot='C:/Projects/llm_mvp_core'
$khTarget="$khRoot/35.ExperimentsV2/Experiments/Rp01Micro/KhExt01/Source"
$khUtf8=[System.Text.UTF8Encoding]::new($false)
$khInputs=@(
 @{source='12.SemanticCompiler/Prediction/MethodKnn.cs'; name='MethodKnn.cs'; blob='e0ecebb6d513259a0930e8706737254f926197cd'},
 @{source='12.SemanticCompiler/Core/Core.cs'; name='Core12.cs'; blob='7e68b2ce1526402be24d3a286a7edf31b395d920'},
 @{source='29.SemanticCompiler/Core/Core.cs'; name='Core29.cs'; blob='0dc09344f15816c11f23e3166bfe8e24ff9fada9'}
)
[IO.Directory]::CreateDirectory($khTarget) | Out-Null
foreach($khItem in $khInputs) {
 $khStart=[Diagnostics.ProcessStartInfo]::new('git')
 $khStart.UseShellExecute=$false
 $khStart.RedirectStandardOutput=$true
 $khStart.RedirectStandardError=$true
 $khStart.StandardOutputEncoding=$khUtf8
 foreach($khArg in @('-C',$khRoot,'cat-file','blob',$khItem.blob)) { $khStart.ArgumentList.Add($khArg) }
 $khProcess=[Diagnostics.Process]::Start($khStart)
 $khText=$khProcess.StandardOutput.ReadToEnd()
 $khError=$khProcess.StandardError.ReadToEnd()
 $khProcess.WaitForExit()
 if($khProcess.ExitCode -ne 0) { throw "PINNED_BLOB_UNAVAILABLE: $khError" }
 $khWorking=[IO.File]::ReadAllText("$khRoot/$($khItem.source)")
 if($khWorking.Replace("`r`n","`n") -cne $khText.Replace("`r`n","`n")) { throw "SOURCE_CHANGED: $($khItem.source)" }
 $khDest="$khTarget/$($khItem.name)"
 if(Test-Path $khDest) {
  if([IO.File]::ReadAllText($khDest) -cne $khText) { throw "PATH_CONFLICT: $khDest" }
 } else { [IO.File]::WriteAllText($khDest,$khText,$khUtf8) }
}
```

Если pinned blobs отсутствуют локально, получить ровно названный commit read-only `git fetch origin 9622b575a6aabb79939b12cc9f5d96313f1bf370`; если origin недоступен — зафиксировать проблему доступа, не менять пути/данные и не симулировать source.

## 5. Полный маленький проект

`experiments-v2/RP01-MICRO/KH-EXT-01/KhExt01Runner.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
 <PropertyGroup>
  <OutputType>Exe</OutputType><TargetFramework>net9.0</TargetFramework>
  <ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable>
  <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
  <AssemblyName>KhExt01Runner</AssemblyName>
 </PropertyGroup>
 <ItemGroup>
  <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.12.0" />
  <Compile Include="Program.cs" />
  <Compile Include="../../../35.ExperimentsV2/Experiments/Rp01Micro/KhExt01/Source/MethodKnn.cs" />
  <Compile Include="../../../35.ExperimentsV2/Experiments/Rp01Micro/KhExt01/Source/Core12.cs" />
  <Compile Include="../../../35.ExperimentsV2/Experiments/Rp01Micro/KhExt01/Source/Core29.cs" />
 </ItemGroup>
</Project>
```

ProjectReference отсутствуют; Core12/Core29 нужны только для настоящих типов OperationResult/StackLabel и их compile dependencies. Они не создают learner, не меняются и не запускают дополнительные функции.

## 6. Полный Program.cs

Ни этот текст, ни native parser/learner в данном аудите не запускались.

```csharp
using System.Diagnostics;
using System.Text.Json;
using SemanticCompiler;
using SemanticCompiler.Prediction;

if(args.Length!=2 || (args[0]!="A" && args[0]!="B"))
    throw new ArgumentException("Usage: KhExt01Runner A|B <new-result-json>");
var arm=args[0];
var output=Path.GetFullPath(args[1]);
if(File.Exists(output)) throw new IOException("Result exists: "+output);
Directory.CreateDirectory(Path.GetDirectoryName(output)!);
string[] t1=[
 "class Seed { int Start(){return 1;} void Stage1(){for(int i=0;i<2;i++) {}} void Stage2(){for(int j=0;j<3;j++) {}} void Stage3(){for(int k=0;k<4;k++) {}} void Stage4(){for(int n=0;n<5;n++) {}} }"
];
string[] t2=[
 "class NewA { int OnlyA(){return 10;} }",
 "class NewB { int OnlyB(){return 20;} }",
 "class NewC { int OnlyC(){return 30;} }",
 "class NewD { int OnlyD(){return 40;} }"
];
string[] exam=[
 "class QueryA { int BeforeStage(){return 7;} }",
 "class QueryB { void BeforeStage(){for(int x=0;x<7;x++) {}} }",
 "class QueryC { void BeforeStage(){for(int x=0;x<8;x++) {int y=x+1;}} }",
 "class QueryD { void BeforeStage(){foreach(int x in new int[]{1,2}) {}} }"
];
var entry=new KhExt01MethodEntry(arm=="B");
Snapshot Read(string phase) {
 var answers=exam.Select(source=>entry.Query(source,StackLabel.CSharp)).ToArray();
 return new(phase,entry.Size,answers,answers.Count(x=>x is not null),answers.Count(x=>x=="Loop"));
}
var q0=Read("beforeT1");
var r1=entry.Train(t1);
var q1=Read("afterT1");
var r2=entry.Train(t2);
var q2=Read("afterT2");
var report=new {
 experiment="KH-EXT-01",arm,runtime=Environment.Version.ToString(),t1,t2,exam,
 gold=Enumerable.Repeat("Loop",exam.Length).ToArray(),
 snapshots=new[]{q0,q1,q2},training=new[]{r1,r2},
 retainedNonNullAnswers=q2.Answers.Where((answer,i)=>answer is not null && answer==q1.Answers[i]).Count(),
 retainedCorrectAnswers=q2.Answers.Where((answer,i)=>answer=="Loop" && q1.Answers[i]=="Loop").Count(),
 retentionDenominator=q1.CorrectLoopAnswers,
 entry.TrainAttempts,entry.NativeTrainCalls,entry.Queries,entry.NativePredicts,entry.GuardNulls,
 scope="Native MethodExtractor->facade admission port->native Train->native memory->facade reader port->native Predict; no Compile"
};
File.WriteAllText(output,JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));

sealed record Snapshot(string Phase,int PairCount,string?[] Answers,int NonNullAnswers,int CorrectLoopAnswers);
sealed record UpdateReceipt(int Sources,int Methods,int EligiblePairs,int BeforePairs,int AfterPairs,
 bool Accepted,string Status,double TrainEntryMs,double CpuMs,long AllocatedBytes);
sealed class KhExt01MethodEntry(bool validatePairs) {
 readonly MethodKnnPredictor memory=new(k:5);
 public int Size=>memory.TrainingSize;
 public int TrainAttempts{get;private set;}
 public int NativeTrainCalls{get;private set;}
 public int Queries{get;private set;}
 public int NativePredicts{get;private set;}
 public int GuardNulls{get;private set;}
 public UpdateReceipt Train(IReadOnlyList<string> sources) {
  TrainAttempts++;
  int before=memory.TrainingSize;
  using var process=Process.GetCurrentProcess();
  var cpu=process.TotalProcessorTime;
  var alloc=GC.GetAllocatedBytesForCurrentThread();
  var begin=Stopwatch.GetTimestamp();
  var methods=new List<PackedMethod>();
  for(short fileId=0;fileId<Math.Min(sources.Count,short.MaxValue);fileId++) {
   var extracted=MethodExtractor.Extract(sources[fileId],fileId);
   if(extracted.IsSuccess) methods.AddRange(extracted.Value!);
  }
  // Both arms record the same diagnostic count. Only B uses it for admission.
  int eligible=0;
  for(int i=0;i<methods.Count-1;i++)
   if(methods[i].FileId==methods[i+1].FileId && methods[i+1].IndexInFile==methods[i].IndexInFile+1) eligible++;
  bool accepted=validatePairs ? eligible>=4 : methods.Count>=4;
  if(accepted) { memory.Train(methods); NativeTrainCalls++; }
  var elapsed=Stopwatch.GetElapsedTime(begin).TotalMilliseconds;
  var bytes=GC.GetAllocatedBytesForCurrentThread()-alloc;
  var cpuMs=(process.TotalProcessorTime-cpu).TotalMilliseconds;
  return new(sources.Count,methods.Count,eligible,before,memory.TrainingSize,accepted,
   accepted?"AcceptedReplacement":validatePairs?"RejectedInsufficientPairs":"RejectedInsufficientMethods",elapsed,cpuMs,bytes);
 }
 public string? Query(string source,StackLabel stack) {
  Queries++;
  // UseMethodKnn fixed true in both arms; original minimum4 is unchanged.
  if(stack!=StackLabel.CSharp || memory.TrainingSize<4) { GuardNulls++; return null; }
  var extracted=MethodExtractor.Extract(source);
  if(!extracted.IsSuccess || extracted.Value!.Count==0) return null;
  NativePredicts++;
  return memory.Predict(extracted.Value[^1]);
 }
}
```

Диагностический eligible-count считается в обоих плечах, чтобы log не был привилегией B; только admission использует его в B. Поэтому timing здесь включает одинаковый диагностический проход и не является чистым product cost benchmark. Основная проверка retention сохраняет один фактор.

## 7. Команда и обработка ошибок

После сохранения файлов и provenance выполнить только:

```powershell
$khRoot='C:/Projects/llm_mvp_core'
$khHost="$khRoot/experiments-v2/RP01-MICRO/KH-EXT-01"
pwsh -NoProfile -File "$khHost/Prepare.ps1"
if($LASTEXITCODE -ne 0) { throw 'Prepare failed; preserve evidence and resolve only the named conflict' }
dotnet build "$khHost/KhExt01Runner.csproj" -c Release --nologo -p:ImportDirectoryBuildProps=false -p:ImportDirectoryBuildTargets=false
if($LASTEXITCODE -ne 0) { throw 'Fix only this experimental host without changing factor/data/scoring; rebuild before Train' }
$khDll="$khHost/bin/Release/net9.0/KhExt01Runner.dll"
dotnet $khDll A "$khHost/results/A.json"
if($LASTEXITCODE -ne 0) { throw 'A failed; preserve partial run, do not repeat counted training' }
dotnet $khDll B "$khHost/results/B.json"
if($LASTEXITCODE -ne 0) { throw 'B failed; preserve partial run, do not repeat counted training' }
```

Сохранить stdout/stderr/exit codes в собственный console log. При отсутствующем .NET9/Roslyn4.12 записать `ENVIRONMENT_UNAVAILABLE`. Не искать другой host, не чинить production и не прогонять общий test suite. Ошибка компиляции своего standalone кода исправляется в этом же host; это не основание для нового approval или изменения учебного бюджета. Если ошибочный runtime успел сделать часть действий, не скрывать и не повторять их вне cap.

## 8. Отчёт и статус результата

Читать полученные JSON без нового вызова native функций. Таблица обязана отдельно показать Q0/Q1/Q2 каждого плеча: pair-count,4 собственных answers, non-null coverage, correct contract answers; T1 acquisition и T2 retention, accepted/rejected update; время/CPU/allocations каждого учебного входа. Показать абсолютные counts и знаменатели. Отдельно отметить, что B сохраняет прежнее состояние, а не усваивает T2: новый sparse материал в этом вмешательстве намеренно не становится памятью. Это возможная цена выбранной политики.

**Любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост accuracy для этого не обязателен.** Сохранение non-null полезного learned ответа после T2 может быть успехом retention/coverage даже без роста Q1 accuracy. Все ухудшения, дополнительная стоимость B, отклонение обновления и ограничения рядом. Не требовать общей метки PASS и не стирать локальный успех из-за другой метрики. Если фактическая разница отсутствует, записать её; не объявлять предсказанные0/4 или4/4 измеренными числами.

Если опыт удачен — добавить отчёт и кандидат исправления original `SemanticCompilerFacade.TrainMethodKnn` перед строкой189: eligible-pair admission before Train, прежний reader minimum4. Не менять `MethodKnnPredictor` на append, не менять threshold и не внедрять патч без выбора пользователя. Внутренний void API product может потребовать отдельного решения об отчёте отклонённого batch; лабораторный `UpdateReceipt` не является готовым product API.

## 9. Промпт исполнителю

> Выполни только выбранный KH-EXT-01 по этой сохранённой карточке. Это уточнение prior art LT-N02 о sequential retention, не новая общая причина RP01. Используй ровно заданные source copies, KhExt01MethodEntry, standalone project, fixtures, Q0→T1→Q1→T2→Q2 и commands; не ищи новое место или новый проект. Единственный фактор — допуск по eligible pairs до destructive Clear. Флаг true, k5, minimum4, extractor/distance/voting не меняются. Нужны настоящее обучение T1, настоящее T2 admission/Train в A и frozen native queries, не один арифметический подсчёт пар. Любое измеренное улучшение полезного показателя является успехом этой метрики и домена; рост accuracy не обязателен, ухудшения показывай рядом. Можно исправить только собственный host и повторить сборку без изменения опыта. Cap:4 training attempts,10 source presentations,3 native Train,24 logical queries; не добавляй warmup/parity/BA/full-suite. Не редактируй production, protected папку или старые эксперименты. Сохрани raw результаты, ограничения, ясный отчёт и кандидат исправления; окончательное внедрение выбирает пользователь.
