# SKNN-SIBLING-PREP-01: неактивный альтернативный рецепт существующего AST-SIB-CACHE-01

**REFERENCE_ONLY / NO_SEPARATE_RUN.** Перед публикацией обнаружен concurrent planning `9d311be43f4e108f6fa1cda1774549d54941c3c5` с [AST-SIB-CACHE-01](https://github.com/lubochka/xiigen-planning/blob/9d311be43f4e108f6fa1cda1774549d54941c3c5/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/63-LEARNING-RESULTS-AND-EXPERIMENT-READBACK-2026-10-20/07-AST-SIB-CACHE-01.md). Фактор совпадает: scoped sibling cache при подготовке настоящего Structural kNN обучения. SKNN больше не заявляется новым и не имеет отдельного места в очереди. Canonical исполнение заранее выбрано: **AST**,4 TrainFromSource/128 Predict, его narrow/wide fixtures и source pin. Нельзя смешивать два рецепта, budgets, panels или hosts.

Ниже сохранён полный неисполняемый альтернативный design для review: его2 facade Train/32 logical query относятся только к этому варианту. Его команды **не являются назначением запуска**. Ранее написанные32a/32b/33 в сохранённом варианте — неактивные локальные обозначения до9d311; единственный текущий порядок — [05](05-execution-registry.md). [Сопоставление вариантов](reviews/concurrent-SKNN-reconciliation.md).

Любой будущий AST результат оценивается по полезным измеренным показателям, без требования роста accuracy и с соседними регрессиями. Default модель, русский report, protected read-only. STOP/NEXT немедленно прекращает прежнюю работу, включая report/hash/review/retry/commit.

---

## Сохранённый альтернативный design; НЕ ВЫПОЛНЯТЬ КАК ВТОРОЙ ОПЫТ

# SKNN-SIBLING-PREP-01 — сохранить те же обученные AST-векторы, сократив повторные обходы соседей

**Статус 20.10.2026:** SOURCE_CONFIRMED / HYPOTHESIS / NOT_RUN. Это новая постановка будущего опыта, не отчёт о полученном ускорении. Код host ниже ещё не создавался, не компилировался и не выполнялся. Окончательная относительная новизна сверяется в knn-history-audit.md. Защищённая папка docs/plans/replanning-possible-diffs не меняется.

## 1. Цель, граница и приоритет

Проверить стоимость **целого действующего метода SemanticCompilerFacade.TrainStructuralKnn**: ParseText → DescendantNodes → StructuralVectorExtractor.Extract → список обучающих пар → StructuralKnnPredictor.Train. В B единственное изменение — временная таблица sibling count / ordinal, живущая внутри одного вызова TrainStructuralKnn. Обучение настоящее: native Train очищает и заполняет собственную память; reader затем вызывает native Predict на этой памяти.

Домен — **программирование на C#**; профиль первого опыта — AST с широкими списками дочерних узлов, синтетические корректные C# массивы. Результат не переносится автоматически на POS, русский/английский перевод, SQL, DPO-LoRA, все C# репозитории или полный RP01.

Источник:

- [StructuralKnn.cs L42–60](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/12.SemanticCompiler/Prediction/StructuralKnn.cs#L42-L60): каждый Extract материализует Parent.ChildNodes().ToList() и ищет IndexOf.
- [TrainStructuralKnn L206–225](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/12.SemanticCompiler/SemanticCompilerFacade.cs#L206-L225): такой Extract вызывается для каждого учебного AST узла, кроме последнего в файле; затем native Train.
- [Train/Predict L116–158](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/12.SemanticCompiler/Prediction/StructuralKnn.cs#L116-L158) и [reader L299–311](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/12.SemanticCompiler/SemanticCompilerFacade.cs#L299-L311).

При родителе с d дочерними узлами исходник повторяет построение списка длины d приблизительно d раз. Это source-level объяснение возможной стоимости; **O(d²) не является измерением времени или экономии памяти**. B хранит дополнительный словарь: на малых деревьях или при малом reuse возможны ухудшения.

Порядок в едином реестре: **supplemental32a**, после №32 NG01-null-guard-label и перед новым supplemental32b BPE-REALIZED-MERGE-COUNT-01. Текущий NEXT остаётся **№05 N62-R2**; новый опыт его не заменяет. Следующий после SKNN — supplemental32b BPE-REALIZED-MERGE-COUNT-01 (карточки08/09 нового пакета), затем №33 KN-ROUTER (его старый protected номер32), только если его опубликованные предусловия разрешены; самостоятельного автоматического запуска нет. Числовые переходы из старых карточек не наследуются.

**Правило успеха:** любое измеренное улучшение полезного показателя — успех этого показателя и этого домена; рост accuracy не обязателен. Показывать рядом все ухудшения, время и границы. Для утверждения «та же выученная модель дешевле» дополнительно необходимы одинаковые ordered vectors/labels/state hashes и predictions. Несовпадение нельзя скрывать благоприятным временем: сохранить raw снижение как наблюдение, отметить INVALID_EQUIVALENCE и не выдавать его за подтверждение данной гипотезы.

**STOP / NEXT обязательны.** Прямая команда STOP/«закончить немедленно» немедленно прекращает работу, без дополнительного чтения, hash, отчёта, commit или cleanup. Допустимо только прекратить собственный уже работающий процесс, если иначе обучение продолжится. NEXT завершает работу над предыдущим опытом и переводит к указанному следующему ID; нельзя продолжать прежний отчёт «ещё один раз». Никаких новых запусков, удалений старых результатов или исправлений production после результата.

## 2. Исполнитель, место, модель, навыки

Модель исполнителя — та же default-модель текущей сессии; не менять model ID, reasoning, provider или конфигурацию агента. Новая нейронная модель не создаётся: здесь память StructuralKnnPredictor(k:5).

Применить репозиторные docs/skills/architect-behavior-classifier-SKILL.md, code-examination-SKILL.md, plan-review-SKILL.md, plan-self-validation-SKILL.md, human-readable-architecture-audit-SKILL.md и prompt-patch-authoring-SKILL.md по назначению. Читать только необходимые указанные файлы и исходники опыта, не искать новые навыки во всём репозитории. User task имеет приоритет над общими production gates. Итоговый report и candidate — на русском, JSON ключи сохраняются на английском.

**Обязательный будущий workspace contract, не обнаруженный в этой сессии факт:** repo root C:/Projects/llm_mvp_core. Если такого checkout нет, статус BLOCKED_MISSING_CHECKOUT; не искать другой путь и не создавать checkout самостоятельно.
Фиксированный новый project root: C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SKNN-SIBLING-PREP-01.
Короткий build root: C:/xexp/sk01; здесь только obj и bin. Не создавать вложенный checkout, не копировать длинный путь scaffolds. Если любой root занят чужой задачей или нет доступа, записать конкретный PATH_CONFLICT / ACCESS_BLOCKED без поиска иного места.

Будущие файлы:

- Sk01.csproj, Program.cs, Prepare.ps1, Run.ps1, card.md, executor.md;
- src/A/StructuralKnn.cs, src/A/Facade.cs, src/A/Options.cs;
- src/B/StructuralKnn.cs, src/B/Facade.cs, src/B/Options.cs, src/B/Cache.cs;
- source-provenance.json, build-provenance.json, fixtures.json;
- results/A/{intent.json,train-issued.json,train-completed.json,metrics.json}; B — аналогично;
- при исключении results/<arm>/failed.json;
- results/comparison.json, results/report.md, results/candidate.md.

Не создавать эти файлы во время документного аудита. При будущем исполнении сохранить дословно четыре code fences под указанными именами; эту карточку как card.md, отдельный prompt как executor.md. Затем команды из раздела 8. Source copies готовит Prepare, исполнитель не ищет и не переписывает алгоритм.

## 3. Что именно компилируется

Standalone net9.0, Microsoft.CodeAnalysis.CSharp **4.12.0** — те же framework/package, что в [native SemanticCompiler.csproj](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/12.SemanticCompiler/SemanticCompiler.csproj). Других NuGet и ProjectReference нет.

Prepare извлекает **полные тела двух реальных методов** из pinned Facade. A TrainStructuralKnn побуквенно сохраняется. Reader сохраняется, кроме private→public для вызова host. Минимальная оболочка содержит те же _structuralKnn, _options и k=5, опция UseStructuralKnn=true у обоих arms. StackLabel извлечён из родного Core.cs, Options копируется целиком. Полный StructuralKnn.cs копируется, а не переписывается.

Это измеряет весь метод обучения и native consumer. Это **не выполнение полного Compile**: тот вызывает RememberCompiledSource и иной grammar learner; его запуск добавил бы постороннее обучение. Не заявлять проверку deployment/admission/full RP01. Нельзя заменить этот host бенчмарком одного Extract.

Pinned core commit: 883496bca18113bf37e0afe683c75b05a0075048.

| Файл | Git blob |
|---|---|
| Prediction/StructuralKnn.cs | b3aa5d376be3f93423997674dc015cea7e9fbf58 |
| SemanticCompilerFacade.cs | f98fc1cc63a01022658e1daff4ac071e3766c239 |
| SemanticCompilerOptions.cs | 9329627b2b25a24c7e3280b2a4162e339338e8c8 |
| Core/Core.cs | 7e68b2ce1526402be24d3a286a7edf31b395d920 |

В B изменяются только optional cache-параметр Extract, соответствующий sibling block и вызов из TrainStructuralKnn; добавляется cache class. Predict вызывает прежнюю uncached Extract. Cache не static, не сохраняется в модели, не живёт между уроками, не используется для EXAM. Все fields, byte saturation255, NodeTypeId%200, order, labels, thresholds, k и voting неизменны.

## 4. Фиксированные данные и бюджет

Program ниже полностью задаёт fixtures до первого Train.

TRAIN: четыре отдельных C# файла с int[] initializer ширины **16,64,256,512**. EXAM: четыре initializer ширины **17,65,257,513** с другими class/method names, затем четыре literal-программы if/else, while, invocation, empty method. Это намеренный stress-fixture узкого механизма, не заявление source-family generalization.

Gold — следующий AST kind в существующей кодировке raw SyntaxKind%200. Native reader возвращает prediction для всех N узлов, включая последний без successor; его terminal prediction сохраняется для parity и учитывается в nativePredictCalls, но не оценивается. Для каждой строки denominator=N−1, answered=min(N−1,prediction.Length). Эта метрика оценивает именно нынешнюю задачу StructuralKnn, не семантическую правильность всего C# и не устранение известных collisions. Accuracy сохраняется как correct/total с abstention=0. До обучения native reader возвращает пустой список predictions=[]; coverage=0. Отдельная conditionalAccuracy=correct/answered в этом случае записывается null, потому что answered=0. Exam gold никогда не передаётся Train.

| Действие | На arm | Всего A+B |
|---|---:|---:|
| Новая пустая native KNN memory | 1 | 2 |
| TRAIN source presentations | 4 | 8 |
| Полный TrainStructuralKnn / native StructuralKnnPredictor.Train | 1 /1 | 2 /2 |
| BEFORE facade query (untrained guard) | 8 | 16 |
| AFTER facade query (native Predict для каждого узла) | 8 | 16 |
| Повторный Train / warmup / optimizer / generation / полный Compile | 0 | 0 |

Всего **2 native Train,32 facade queries**. Количество внутренних native Predict равно числу EXAM узлов после обучения и публикуется из результатов; оно не скрывается за числом8. Нет дополнительного parity-training, циклов для стабильного timing, reverse-order/B→A, retry после первого issued, внешних модели/teacher или полного тестового набора.

A и B запускаются в двух свежих процессах в порядке A→B. BEFORE Evaluate разбирает одну и ту же EXAM панель вне таймера, поэтому Roslyn parser уже частично прогрет. Это не полностью cold процесс для parser. Первый вызов Train и нового cache-кода всё ещё может включать разный JIT; однократное время остаётся шумным наблюдением. Managed allocated bytes, process CPU и elapsed записываются отдельно. Не добавлять повтор ради красивого выигрыша.

## 5. Prepare.ps1

PowerShell7. Код ниже только будущий рецепт, NOT_RUN.

~~~powershell
$ErrorActionPreference = 'Stop'
$repo = 'C:/Projects/llm_mvp_core'
$hostRoot = "$repo/experiments-v2/RP01-MICRO/SKNN-SIBLING-PREP-01"
$utf8 = [Text.UTF8Encoding]::new($false)
$lf = [string][char]10
function Normalize-Lf([string]$s) { $s.Replace(([string][char]13+[char]10),[string][char]10) }
function Read-Blob([string]$path,[string]$sha) {
    $psi=[Diagnostics.ProcessStartInfo]::new('git')
    $psi.UseShellExecute=$false
    $psi.RedirectStandardOutput=$true
    $psi.RedirectStandardError=$true
    $psi.StandardOutputEncoding=$utf8
    foreach($arg in @('-C',$repo,'cat-file','blob',$sha)) { $psi.ArgumentList.Add($arg) }
    $p=[Diagnostics.Process]::Start($psi)
    $s=$p.StandardOutput.ReadToEnd()
    $err=$p.StandardError.ReadToEnd()
    $p.WaitForExit()
    if($p.ExitCode -ne 0) { throw "PINNED_BLOB_UNAVAILABLE: $path $err" }
    $s=Normalize-Lf $s
    $working=Normalize-Lf ([IO.File]::ReadAllText("$repo/12.SemanticCompiler/$path"))
    if($working -cne $s) { throw "SOURCE_CHANGED: $path" }
    return $s
}
function Replace-Once([string]$s,[string]$old,[string]$replacement) {
    if(([regex]::Matches($s,[regex]::Escape($old))).Count -ne 1) {
        throw "SOURCE_SHAPE_CHANGED: $old"
    }
    return $s.Replace($old,$replacement)
}
function Extract-Method([string]$s,[string]$signature) {
    $start=$s.IndexOf($signature,[StringComparison]::Ordinal)
    if($start -lt 0) { throw "MISSING_METHOD: $signature" }
    $end=$s.IndexOf(($lf+'    }'),$start,[StringComparison]::Ordinal)
    if($end -lt 0) { throw "METHOD_END_MISSING: $signature" }
    return $s.Substring($start,$end+$lf.Length+5-$start)
}
function Write-IdenticalOrNew([string]$path,[string]$value) {
    [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($path)) | Out-Null
    if(Test-Path $path) {
        if([IO.File]::ReadAllText($path) -cne $value) { throw "PATH_CONFLICT: $path" }
    } else {
        $fs=[IO.File]::Open($path,[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::None)
        try { $bytes=$utf8.GetBytes($value); $fs.Write($bytes,0,$bytes.Length) } finally { $fs.Dispose() }
    }
}
if(!(Test-Path "$repo/.git")) { throw 'BLOCKED_MISSING_CHECKOUT: fixed workspace contract is not present' }
if(Test-Path "$hostRoot/results") { throw 'RESULTS_EXIST: do not rebuild or retry an issued experiment' }
$struct=Read-Blob 'Prediction/StructuralKnn.cs' 'b3aa5d376be3f93423997674dc015cea7e9fbf58'
$facade=Read-Blob 'SemanticCompilerFacade.cs' 'f98fc1cc63a01022658e1daff4ac071e3766c239'
$options=Read-Blob 'SemanticCompilerOptions.cs' '9329627b2b25a24c7e3280b2a4162e339338e8c8'
$core=Read-Blob 'Core/Core.cs' '7e68b2ce1526402be24d3a286a7edf31b395d920'
$enum=[regex]::Match($core,'public enum StackLabel \{[^\r\n]+\}').Value
if(!$enum) { throw 'STACKLABEL_SHAPE_CHANGED' }
$train=Extract-Method $facade '    public void TrainStructuralKnn(IReadOnlyList<string> csharpSources)'
$reader=Extract-Method $facade '    private IReadOnlyList<int> PredictStructuralNextTokens(string source, StackLabel stack)'
$reader=Replace-Once $reader '    private IReadOnlyList<int> PredictStructuralNextTokens' '    public IReadOnlyList<int> PredictStructuralNextTokens'
$oldSiblings=@'
        if (node.Parent is not null)
        {
            var siblings = node.Parent.ChildNodes().ToList();
            siblingCount = (byte)Math.Min(siblings.Count, 255);
            var index = siblings.IndexOf(node);
            positionInParent = (byte)Math.Min(index >= 0 ? index : 0, 255);
        }
'@
$newSiblings=@'
        if (node.Parent is not null)
        {
            if (siblingCache is not null)
                (positionInParent, siblingCount) = siblingCache.Locate(node);
            else
            {
                var siblings = node.Parent.ChildNodes().ToList();
                siblingCount = (byte)Math.Min(siblings.Count, 255);
                var index = siblings.IndexOf(node);
                positionInParent = (byte)Math.Min(index >= 0 ? index : 0, 255);
            }
        }
'@
$cache=@'
namespace Sk01B.Prediction;
using Microsoft.CodeAnalysis;

public sealed class StructuralSiblingPreparationCache
{
    private sealed record Slots(byte Count, Dictionary<SyntaxNode, byte> Positions);
    private readonly Dictionary<SyntaxNode, Slots> _parents =
        new(ReferenceEqualityComparer.Instance);

    internal (byte Position, byte Count) Locate(SyntaxNode node)
    {
        if (node.Parent is not SyntaxNode parent) return (0, 0);
        if (!_parents.TryGetValue(parent, out var slots))
        {
            var siblings = parent.ChildNodes().ToList();
            var positions = new Dictionary<SyntaxNode, byte>(ReferenceEqualityComparer.Instance);
            for (var i = 0; i < siblings.Count; i++)
                positions.TryAdd(siblings[i], (byte)Math.Min(i, 255));
            slots = new Slots((byte)Math.Min(siblings.Count, 255), positions);
            _parents.Add(parent, slots);
        }
        return (slots.Positions.TryGetValue(node, out var position) ? position : (byte)0, slots.Count);
    }
}
'@
foreach($arm in @('A','B')) {
    $ns="Sk01$arm"
    $s=Replace-Once $struct 'namespace SemanticCompiler.Prediction;' "namespace $ns.Prediction;"
    $o=Replace-Once $options 'namespace SemanticCompiler;' "namespace $ns;"
    $t=$train
    if($arm -eq 'B') {
        $s=Replace-Once $s 'public static StructuralVector Extract(SyntaxNode node, short fileId = 0)' 'public static StructuralVector Extract(SyntaxNode node, short fileId = 0, StructuralSiblingPreparationCache? siblingCache = null)'
        $s=Replace-Once $s (Normalize-Lf $oldSiblings) (Normalize-Lf $newSiblings)
        $t=Replace-Once $t '        var pairs = new List<(StructuralVector vector, int nextToken)>();' ('        var siblingCache = new StructuralSiblingPreparationCache();'+$lf+'        var pairs = new List<(StructuralVector vector, int nextToken)>();')
        $t=Replace-Once $t 'StructuralVectorExtractor.Extract(nodes[index], fileId)' 'StructuralVectorExtractor.Extract(nodes[index], fileId, siblingCache)'
    }
    $shell=@"
namespace $ns;
using Microsoft.CodeAnalysis.CSharp;
using $ns.Prediction;
$enum
public sealed class SemanticCompilerFacade
{
    private readonly SemanticCompilerOptions _options = new() { UseStructuralKnn = true };
    private readonly StructuralKnnPredictor _structuralKnn = new(k: 5);
$t
$reader
}
"@
    Write-IdenticalOrNew "$hostRoot/src/$arm/StructuralKnn.cs" (Normalize-Lf $s)
    Write-IdenticalOrNew "$hostRoot/src/$arm/Options.cs" (Normalize-Lf $o)
    Write-IdenticalOrNew "$hostRoot/src/$arm/Facade.cs" (Normalize-Lf $shell)
}
Write-IdenticalOrNew "$hostRoot/src/B/Cache.cs" (Normalize-Lf $cache)
$files=Get-ChildItem "$hostRoot/src" -Recurse -File | Sort-Object FullName
$hashes=@($files | ForEach-Object { [ordered]@{ path=$_.FullName; sha256=(Get-FileHash $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant() } })
$sourceReceipt=[ordered]@{
    experiment='SKNN-SIBLING-PREP-01'
    coreCommit='883496bca18113bf37e0afe683c75b05a0075048'
    productionChanged=$false
    copies=$hashes
    note='A complete native TrainStructuralKnn body; same native reader except public visibility; B only per-call sibling cache'
} | ConvertTo-Json -Depth 8
Write-IdenticalOrNew "$hostRoot/source-provenance.json" $sourceReceipt
'Source preparation complete. Training has not started.'
~~~

Если blobs отсутствуют, допустим один read-only git fetch origin 883496bca18113bf37e0afe683c75b05a0075048. После этого повторить Prepare до какого-либо Train. Недоступный fetch фиксируется; другой snapshot не подставляется. Source_CHANGED нельзя разрешать молчаливым переносом новых продуктовых функций. Документные изменения HEAD при тех же четырёх blobs не блокируют опыт.

## 6. Sk01.csproj

~~~xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
    <AssemblyName>Sk01</AssemblyName>
    <UseSharedCompilation>false</UseSharedCompilation>
    <ImportDirectoryBuildProps>false</ImportDirectoryBuildProps>
    <ImportDirectoryBuildTargets>false</ImportDirectoryBuildTargets>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.12.0" />
    <Compile Include="Program.cs" />
    <Compile Include="src/A/StructuralKnn.cs" />
    <Compile Include="src/A/Facade.cs" />
    <Compile Include="src/A/Options.cs" />
    <Compile Include="src/B/StructuralKnn.cs" />
    <Compile Include="src/B/Facade.cs" />
    <Compile Include="src/B/Options.cs" />
    <Compile Include="src/B/Cache.cs" />
  </ItemGroup>
</Project>
~~~

## 7. Program.cs

~~~csharp
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

internal static class Program
{
    private const string Experiment = "SKNN-SIBLING-PREP-01";
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };
    private static readonly string[] Fields =
    [
        "AstNodeType", "Parent1Type", "Parent2Type", "Parent3Type",
        "PositionInParent", "SiblingCount", "DepthInMethod", "ScopeDepth",
        "FileId", "SyntacticRole"
    ];
    private sealed record QueryResult(string Id, int[] Gold, int[] Prediction,
        int Correct, int Answered, int Total);

    private static int Main(string[] args)
    {
        if (args.Length != 2 || (args[0] != "A" && args[0] != "B"))
            throw new ArgumentException("Usage: Sk01 A|B <fixed-host-root>");
        var arm = args[0];
        var host = Path.GetFullPath(args[1]);
        var expected = Path.GetFullPath("C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SKNN-SIBLING-PREP-01");
        if (!string.Equals(host, expected, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("HOST_PATH_CONFLICT");
        var dir = Path.Combine(host, "results", arm);
        if (Directory.Exists(dir)) throw new InvalidOperationException("RESULTS_EXIST; no automatic retry");

        var equalsMethod = typeof(SyntaxNode).GetMethod(nameof(object.Equals), [typeof(object)]);
        if (equalsMethod?.DeclaringType != typeof(object) ||
            typeof(IEquatable<SyntaxNode>).IsAssignableFrom(typeof(SyntaxNode)))
            throw new InvalidOperationException("REFERENCE_EQUALITY_CONTRACT_CHANGED");

        var trainSources = new[] { 16, 64, 256, 512 }
            .Select(n => WideSource(n, "Train" + n, "Build")).ToArray();
        var examSources = new[]
        {
            WideSource(17, "Exam17", "Create"),
            WideSource(65, "Exam65", "Create"),
            WideSource(257, "Exam257", "Create"),
            WideSource(513, "Exam513", "Create"),
            "class ExamIf { int F(int x) { if (x > 0) return x; else return -x; } }",
            "class ExamWhile { int F(int n) { int s=0; while(n>0) { s+=n; n--; } return s; } }",
            "class ExamCall { int G(int x,int y) { return x+y; } int F() { return G(3,4); } }",
            "class ExamEmpty { void F() {} }"
        };
        var fixtureText = JsonSerializer.Serialize(new { trainSources, examSources }, Json);
        var fixturePath = Path.Combine(host, "fixtures.json");
        if (File.Exists(fixturePath))
        {
            if (File.ReadAllText(fixturePath) != fixtureText)
                throw new InvalidOperationException("FIXTURE_CONFLICT");
        }
        else WriteNew(fixturePath, fixtureText);
        var fixtureHash = Hash(fixtureText);

        object facade;
        Action<IReadOnlyList<string>> train;
        Func<string, IReadOnlyList<int>> query;
        if (arm == "A")
        {
            var f = new Sk01A.SemanticCompilerFacade();
            facade = f;
            train = f.TrainStructuralKnn;
            query = source => f.PredictStructuralNextTokens(source, Sk01A.StackLabel.CSharp);
        }
        else
        {
            var f = new Sk01B.SemanticCompilerFacade();
            facade = f;
            train = f.TrainStructuralKnn;
            query = source => f.PredictStructuralNextTokens(source, Sk01B.StackLabel.CSharp);
        }

        Directory.CreateDirectory(dir);
        WriteJson(Path.Combine(dir, "intent.json"), new
        {
            experiment = Experiment, arm, fixtureHash, trainSources = 4,
            nativeTrainBudget = 1, facadeQueriesBudget = 16,
            runtime = Environment.Version.ToString(),
            utc = DateTimeOffset.UtcNow
        });
        var issued = false;
        var completed = false;
        try
        {
            var before = Evaluate(query, examSources);
            var beforeState = Snapshot(facade);
            if (beforeState.Length != 0 || before.Any(r => r.Prediction.Length != 0))
                throw new InvalidOperationException("BEFORE_NOT_EMPTY");

            WriteJson(Path.Combine(dir, "train-issued.json"), new
            {
                trainCall = 1, nativeTrainExpected = 1, presentations = 4,
                fixtureHash, utc = DateTimeOffset.UtcNow
            });
            issued = true;
            using var process = Process.GetCurrentProcess();
            process.Refresh();
            var cpu0 = process.TotalProcessorTime;
            var alloc0 = GC.GetAllocatedBytesForCurrentThread();
            var time0 = Stopwatch.GetTimestamp();

            train(trainSources);

            var elapsed = Stopwatch.GetElapsedTime(time0).TotalMilliseconds;
            var allocated = GC.GetAllocatedBytesForCurrentThread() - alloc0;
            process.Refresh();
            var cpuMs = (process.TotalProcessorTime - cpu0).TotalMilliseconds;
            completed = true;
            WriteJson(Path.Combine(dir, "train-completed.json"), new
            {
                trainCall = 1, nativeTrainCompleted = 1, presentations = 4,
                elapsedMs = elapsed, cpuMs, managedAllocatedBytes = allocated,
                utc = DateTimeOffset.UtcNow
            });

            var state = Snapshot(facade);
            if (state.Length < 10) throw new InvalidOperationException("NATIVE_TRAIN_GUARD_NOT_REACHED");
            var after = Evaluate(query, examSources);
            var stateAfterQueries = Snapshot(facade);
            var stateJson = JsonSerializer.Serialize(state);
            var stateAfterJson = JsonSerializer.Serialize(stateAfterQueries);
            if (stateJson != stateAfterJson) throw new InvalidOperationException("EXAM_MUTATED_MEMORY");

            WriteJson(Path.Combine(dir, "metrics.json"), new
            {
                experiment = Experiment, arm, fixtureHash,
                trainCalls = 1, nativeTrainCalls = 1, trainPresentations = 4,
                elapsedMs = elapsed, cpuMs, managedAllocatedBytes = allocated,
                beforeStateHash = Hash(JsonSerializer.Serialize(beforeState)),
                trainedStateHash = Hash(stateJson),
                trainedState = state,
                stateColumns = Fields.Concat(new[] { "NextToken" }).ToArray(),
                afterQueryStateHash = Hash(stateAfterJson),
                before, after,
                beforeSummary = Summarize(before),
                afterSummary = Summarize(after),
                nativePredictCalls = after.Sum(r => r.Prediction.Length),
                note = "One fresh process, one full native prepare+Train; no warmup; no claim of semantic code correctness"
            });
            return 0;
        }
        catch (Exception ex)
        {
            WriteJson(Path.Combine(dir, "failed.json"), new
            {
                experiment = Experiment, arm, issued, completed,
                effects = issued && !completed ? "UNKNOWN" : completed ? "ONE_NATIVE_TRAIN_COMPLETED" : "NO_TRAIN_ISSUED",
                error = ex.ToString(), utc = DateTimeOffset.UtcNow
            });
            return 1;
        }
    }

    private static string WideSource(int n, string className, string method)
    {
        var entries = string.Join(",", Enumerable.Range(0, n)
            .Select(i => i.ToString(CultureInfo.InvariantCulture)));
        return $"class {className} {{ static int {method}() {{ var a = new int[] {{ {entries} }}; return a[{n - 1}]; }} }}";
    }

    private static QueryResult[] Evaluate(
        Func<string, IReadOnlyList<int>> query, string[] sources)
    {
        var rows = new List<QueryResult>();
        for (var i = 0; i < sources.Length; i++)
        {
            var nodes = CSharpSyntaxTree.ParseText(sources[i]).GetRoot().DescendantNodes().ToArray();
            var gold = nodes.Skip(1).Select(n => (int)n.Kind() % 200).ToArray();
            var prediction = query(sources[i]).ToArray();
            var answered = Math.Min(gold.Length, prediction.Length);
            var correct = Enumerable.Range(0, answered).Count(j => prediction[j] == gold[j]);
            rows.Add(new QueryResult($"exam-{i + 1:00}", gold, prediction, correct, answered, gold.Length));
        }
        return rows.ToArray();
    }

    private static object Summarize(QueryResult[] rows)
    {
        var total = rows.Sum(r => r.Total);
        var answered = rows.Sum(r => r.Answered);
        var correct = rows.Sum(r => r.Correct);
        return new
        {
            correct, answered, total,
            accuracy = total == 0 ? (double?)null : correct / (double)total,
            conditionalAccuracy = answered == 0 ? (double?)null : correct / (double)answered,
            coverage = total == 0 ? 0.0 : answered / (double)total
        };
    }

    private static int[][] Snapshot(object facade)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var knn = facade.GetType().GetField("_structuralKnn", flags)!.GetValue(facade)!;
        var list = (IEnumerable)knn.GetType().GetField("_training", flags)!.GetValue(knn)!;
        var rows = new List<int[]>();
        foreach (var item in list)
        {
            var type = item!.GetType();
            var vector = type.GetField("Item1")!.GetValue(item)!;
            var next = Convert.ToInt32(type.GetField("Item2")!.GetValue(item), CultureInfo.InvariantCulture);
            var values = Fields.Select(name =>
                Convert.ToInt32(vector.GetType().GetProperty(name)!.GetValue(vector), CultureInfo.InvariantCulture))
                .Concat(new[] { next }).ToArray();
            rows.Add(values);
        }
        return rows.ToArray();
    }

    private static string Hash(string text)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

    private static void WriteJson(string path, object value)
        => WriteNew(path, JsonSerializer.Serialize(value, Json));

    private static void WriteNew(string path, string text)
    {
        using var file = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        using var writer = new StreamWriter(file, new UTF8Encoding(false));
        writer.Write(text);
    }
}
~~~

Gold parsing и snapshot происходят вне таймера и не вызывают Train. Fixture hash одинаков в двух процессах. Полный ordered state сохраняется один раз после обучения; read-only повтор snapshot проверяет отсутствие изменения consumer-ом. В обоих arms до обучения память пуста; это no-update контроль. Идентичность final state и всех per-case predictions A/B — отдельный parity check.

## 8. Run.ps1 и будущие команды

~~~powershell
$ErrorActionPreference='Stop'
$repo='C:/Projects/llm_mvp_core'
$hostRoot="$repo/experiments-v2/RP01-MICRO/SKNN-SIBLING-PREP-01"
$buildRoot='C:/xexp/sk01'
$utf8=[Text.UTF8Encoding]::new($false)
function New-Json([string]$path,$data) {
    $text=$data | ConvertTo-Json -Depth 15
    $fs=[IO.File]::Open($path,[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::None)
    try { $b=$utf8.GetBytes($text); $fs.Write($b,0,$b.Length) } finally { $fs.Dispose() }
}
if(Test-Path "$hostRoot/results") { throw 'RESULTS_EXIST: do not retry, rebuild, delete or resume automatically' }
$resumeExactBuild=Test-Path "$hostRoot/build-provenance.json"
if(!(Test-Path "$hostRoot/source-provenance.json")) { throw 'RUN_PREPARE_FIRST' }
$owner="$buildRoot/owner.txt"
if(Test-Path $owner) {
    if([IO.File]::ReadAllText($owner) -cne 'SKNN-SIBLING-PREP-01') { throw 'BUILD_PATH_CONFLICT' }
} else {
    if((Test-Path $buildRoot) -and @(Get-ChildItem $buildRoot -Force).Count -gt 0) { throw 'BUILD_PATH_OCCUPIED' }
    [IO.Directory]::CreateDirectory($buildRoot) | Out-Null
    [IO.File]::WriteAllText($owner,'SKNN-SIBLING-PREP-01',$utf8)
}
$info=& dotnet --info
if($LASTEXITCODE -ne 0) { throw 'DOTNET_INFO_FAILED' }
$sdks=& dotnet --list-sdks
if($LASTEXITCODE -ne 0 -or !(@($sdks | Where-Object { $_ -match '^9\.' }).Count)) { throw 'NET9_SDK_REQUIRED' }
if(!$resumeExactBuild) {
    & dotnet build "$hostRoot/Sk01.csproj" -c Release --nologo -o "$buildRoot/bin" "-p:BaseIntermediateOutputPath=$buildRoot/obj/" "-p:ImportDirectoryBuildProps=false" "-p:ImportDirectoryBuildTargets=false"
    if($LASTEXITCODE -ne 0) { throw 'HOST_BUILD_FAILED: before Train, fix only host mechanical issues with unchanged factor/data/budget' }
    $files=Get-ChildItem $hostRoot -Recurse -File | Where-Object { $_.Extension -in @('.cs','.csproj','.ps1','.md') } | Sort-Object FullName
    $hashes=@($files | ForEach-Object { [ordered]@{ path=$_.FullName; sha256=(Get-FileHash $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant() } })
    New-Json "$hostRoot/build-provenance.json" @{
        experiment='SKNN-SIBLING-PREP-01'; utc=[DateTimeOffset]::UtcNow.ToString('O')
        runtimeInfo=$info; sources=$hashes
        assemblySha256=(Get-FileHash "$buildRoot/bin/Sk01.dll" -Algorithm SHA256).Hash.ToLowerInvariant()
    }
} else {
    # The top-level RESULTS_EXIST guard proves that neither arm was started.
    # Resume the existing DLL only; no compilation and no changes to receipts.
    $saved=Get-Content "$hostRoot/build-provenance.json" -Raw | ConvertFrom-Json
    if($saved.experiment -cne 'SKNN-SIBLING-PREP-01') { throw 'BUILD_RECEIPT_CONFLICT' }
    foreach($entry in $saved.sources) {
        $current=(Get-FileHash $entry.path -Algorithm SHA256).Hash.ToLowerInvariant()
        if($current -cne $entry.sha256) { throw "SOURCE_CHANGED_AFTER_BUILD: $($entry.path)" }
    }
    $dllHash=(Get-FileHash "$buildRoot/bin/Sk01.dll" -Algorithm SHA256).Hash.ToLowerInvariant()
    if($dllHash -cne $saved.assemblySha256) { throw 'ASSEMBLY_CHANGED_AFTER_BUILD' }
}
& dotnet "$buildRoot/bin/Sk01.dll" A $hostRoot
if($LASTEXITCODE -ne 0) { throw 'ARM_A_FAILED: preserve all outputs; no retry, no B' }
& dotnet "$buildRoot/bin/Sk01.dll" B $hostRoot
if($LASTEXITCODE -ne 0) { throw 'ARM_B_FAILED: preserve A and B; no retry' }
$a=Get-Content "$hostRoot/results/A/metrics.json" -Raw | ConvertFrom-Json
$b=Get-Content "$hostRoot/results/B/metrics.json" -Raw | ConvertFrom-Json
$predA=$a.after | ForEach-Object { ($_.Prediction -join ',') }
$predB=$b.after | ForEach-Object { ($_.Prediction -join ',') }
$equalState=$a.trainedStateHash -ceq $b.trainedStateHash
$equalPred=($predA -join '|') -ceq ($predB -join '|')
New-Json "$hostRoot/results/comparison.json" @{
    experiment='SKNN-SIBLING-PREP-01'
    fixturesEqual=($a.fixtureHash -ceq $b.fixtureHash)
    trainedStatesEqual=$equalState; predictionsEqual=$equalPred
    elapsedMsDelta=($b.elapsedMs-$a.elapsedMs)
    cpuMsDelta=($b.cpuMs-$a.cpuMs)
    managedAllocatedBytesDelta=($b.managedAllocatedBytes-$a.managedAllocatedBytes)
    accuracyDelta=($b.afterSummary.accuracy-$a.afterSummary.accuracy)
    coverageDelta=($b.afterSummary.coverage-$a.afterSummary.coverage)
    elapsedMetricImproved=($b.elapsedMs -lt $a.elapsedMs)
    cpuMetricImproved=($b.cpuMs -lt $a.cpuMs)
    allocationMetricImproved=($b.managedAllocatedBytes -lt $a.managedAllocatedBytes)
    causalStatus= $(if($equalState -and $equalPred) {'EQUIVALENT_STATE_OBSERVED'} else {'INVALID_EQUIVALENCE'})
    note='One A/B observation; every improved useful metric recorded, all regressions retained; no automatic production promotion'
}
'Complete. Write the Russian report/candidate from saved JSON only; do not run another experiment.'
~~~

После будущего materialize четырёх файлов и card/executor:

~~~powershell
pwsh -NoProfile -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SKNN-SIBLING-PREP-01/Prepare.ps1
pwsh -NoProfile -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SKNN-SIBLING-PREP-01/Run.ps1
~~~

Это **команды будущего опыта, сейчас не выполнялись**. Не добавлять отдельный dotnet test, full solution build, rerun старых опытов или git commit/push. До первого issued допустима механическая починка собственной оболочки/сборки без изменения factor/data/scorer/budget; если нужен иной фактор — записать конфликт и прекратить эту постановку. Если build-provenance уже создан, но ни один arm intent/issued не существует, разрешено только разовое продолжение **того же уже собранного DLL** повторным вызовом указанного Run.ps1: его отдельная ветка resume проверяет SHA и source hashes, не выполняет build и не перезаписывает receipt. При любом issued нельзя автоматически повторять даже если train-completed отсутствует.

## 9. Report и кандидат

results/report.md, русский, до450 слов плюс таблица/ссылки:

1. Что проверяли, exact core pin, pure cost фактор и узкий домен.
2. Статус выполнения и реальные issued/completed calls; все исключения/UNKNOWN.
3. A/B BEFORE и AFTER: correct/answered/total, accuracy, conditional accuracy, coverage; state/vector/order/label parity и per-case prediction parity.
4. Таблица elapsed ms, CPU ms, managed allocated bytes: A, B, delta, успех/ухудшение/без изменения. Не называть одно измерение устойчивым speedup.
5. Границы: synthetic wide AST; parser частично прогрет BEFORE панелью, first-Train JIT не исключён; ни full Compile, ни нейронный RP01 не запускались.
6. Решение: candidate для пользователя при полезном измеренном улучшении; все regression rows видимы.

results/candidate.md всегда фиксирует исход: успешный показатель/домен или отсутствие улучшения/невалидность сравнения. Если опыт успешен, указать точное будущее место продуктовой правки — cache-aware sibling подготовка в StructuralVectorExtractor.Extract и локальное создание cache в SemanticCompilerFacade.TrainStructuralKnn. Не менять production автоматически. Продолжительность жизни cache строго один Train; reader и restore формат не меняются. Решение о полной цепочке внедрения/тестов принимает пользователь.

После одного review JSON/ссылок для фактических ошибок работа над карточкой закончена. Нельзя полировать предыдущий отчёт после NEXT.
