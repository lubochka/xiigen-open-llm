# Оракул переноса FIX-P83: читать выражение-тело метода в наборе признаков.
#
# Зачем он нужен. Сборка проекта и прогон площадочных тестов закрыты правами
# сессии (точные команды и ошибки — в отчёте переноса), поэтому поведение
# доказывается здесь: логика продукта перенесена в эту программу ДОСЛОВНО, с
# указанием файла и строк источника, и прогоняется на фактических учебных
# данных восемьдесят третьего опыта.
#
# Что сравнивается. Три варианта чтения признаков метода:
#   «прежний»    — ранний выход с пустым набором при отсутствии блочного тела
#                  (как было в 12.SemanticCompiler/Prediction/MethodKnn.cs
#                  до переноса, строки 139-141);
#   «новый»      — перенесённый вариант: при отсутствии блочного тела читается
#                  выражение-тело ВМЕСТЕ с самим выражением;
#   «испорченный» — проверка на чувствительность: две строки чтения узлов
#                  выровнены между собой, то есть у выражения-тела читается
#                  поддерево БЕЗ самого выражения. Карточка переноса прямо
#                  предупреждает, что так правка теряет смысл целиком.
#
# Остальные части механизма — сокращение имён видов узлов, упаковка подписи,
# расстояние по наборам, вид метода, обучение и ответ — одни и те же для всех
# трёх вариантов и перенесены без изменений.

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# Та же библиотека разбора C# версии 4.12.0, на которую ссылается проект
# 12.SemanticCompiler (PackageReference Microsoft.CodeAnalysis.CSharp 4.12.0).
$packages = Join-Path $env:USERPROFILE '.nuget\packages'
$roslynCommon = Join-Path $packages 'microsoft.codeanalysis.common\4.12.0\lib\net8.0\Microsoft.CodeAnalysis.dll'
$roslynCSharp = Join-Path $packages 'microsoft.codeanalysis.csharp\4.12.0\lib\net8.0\Microsoft.CodeAnalysis.CSharp.dll'

foreach ($dll in @($roslynCommon, $roslynCSharp)) {
    if (-not (Test-Path -LiteralPath $dll)) {
        throw "Не найдена библиотека разбора: $dll"
    }
}

Write-Host 'Библиотека разбора C#:'
Write-Host "  $roslynCommon"
Write-Host "  $roslynCSharp"
Write-Host ''

$csharp = @'
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OracleP83
{
    // Дословный перенос записи метода из 12.SemanticCompiler/Prediction/MethodKnn.cs
    // (строки 12-16): упакованная подпись, набор видов узлов, номер файла и номер
    // метода внутри файла.
    public sealed class Packed
    {
        public string PackedString;
        public HashSet<string> KindBag;
        public short FileId;
        public int IndexInFile;
    }

    public static class Mechanism
    {
        // Какой вариант чтения признаков включён.
        public const int Legacy = 0;      // прежний: ранний выход
        public const int Transferred = 1; // новый: выражение вместе с собой
        public const int Mutated = 2;     // испорченный: выражение без себя

        // ДОСЛОВНО: AbbreviateKind, MethodKnn.cs строки 154-180 после переноса
        // (строки 148-174 до переноса — текст функции переносом не менялся).
        public static string AbbreviateKind(string kind)
        {
            switch (kind)
            {
                case "IfStatement": return "If";
                case "ElseClause": return "Else";
                case "ReturnStatement": return "Ret";
                case "ForStatement": return "For";
                case "ForEachStatement": return "ForEach";
                case "WhileStatement": return "While";
                case "InvocationExpression": return "Call";
                case "AssignmentExpression": return "Assign";
                case "SimpleAssignmentExpression": return "Assign";
                case "NullLiteralExpression": return "NullCheck";
                case "ThrowStatement": return "Throw";
                case "TryStatement": return "Try";
                case "CatchClause": return "Catch";
                case "LocalDeclarationStatement": return "Decl";
                case "BinaryExpression": return "BinOp";
                case "MemberAccessExpression": return "Member";
                case "ObjectCreationExpression": return "New";
                case "AwaitExpression": return "Await";
                case "LambdaExpression": return "Lambda";
                case "ConditionalExpression": return "Ternary";
            }

            if (kind.Length <= 6) return kind;
            if (kind.EndsWith("Expression", StringComparison.Ordinal)) return kind.Substring(0, kind.Length - 10);
            if (kind.EndsWith("Statement", StringComparison.Ordinal)) return kind.Substring(0, kind.Length - 9);
            return "?";
        }

        // ДОСЛОВНО: PackMethod, MethodKnn.cs строки 122-134. Переносом НЕ менялась:
        // карточка измерила, что эта половина не двигает ни одного ответа.
        public static string PackMethod(MethodDeclarationSyntax method)
        {
            var parameters = string.Join(",", method.ParameterList.Parameters.Select(p => p.Type == null ? "?" : p.Type.ToString()));
            var bodyKinds = method.Body == null
                ? new string[0]
                : method.Body.DescendantNodes()
                    .Select(node => AbbreviateKind(node.Kind().ToString()))
                    .Where(k => k != "?")
                    .Distinct()
                    .Take(12)
                    .ToArray();
            var kinds = bodyKinds.Length == 0 ? "Empty" : string.Join(" ", bodyKinds);
            return "fn " + method.ReturnType + " " + method.Identifier + "(" + parameters + ") [" + kinds + "]";
        }

        // Три варианта чтения признаков.
        //
        // ПРЕЖНИЙ — дословно MethodKnn.cs строки 139-151 до переноса:
        //     var bag = new HashSet<string>(StringComparer.Ordinal);
        //     if (method.Body is null)
        //         return bag;
        //     foreach (var node in method.Body.DescendantNodes())
        //
        // НОВЫЙ — дословно MethodKnn.cs после переноса: источник узлов выбирается
        // блоком if/else if/else, у выражения-тела берётся DescendantNodesAndSelf.
        //
        // ИСПОРЧЕННЫЙ — тот же новый код с одной подменой: у выражения-тела
        // DescendantNodes вместо DescendantNodesAndSelf, то есть две строки
        // чтения узлов выровнены между собой.
        public static HashSet<string> ExtractKindBag(MethodDeclarationSyntax method, int variant)
        {
            var bag = new HashSet<string>(StringComparer.Ordinal);

            IEnumerable<SyntaxNode> nodes;
            if (method.Body != null)
            {
                nodes = method.Body.DescendantNodes();
            }
            else if (variant != Legacy && method.ExpressionBody != null)
            {
                nodes = variant == Transferred
                    ? method.ExpressionBody.Expression.DescendantNodesAndSelf()
                    : method.ExpressionBody.Expression.DescendantNodes();
            }
            else
            {
                return bag;
            }

            foreach (var node in nodes)
            {
                var abbreviation = AbbreviateKind(node.Kind().ToString());
                if (abbreviation != "?")
                    bag.Add(abbreviation);
            }

            return bag;
        }

        // ДОСЛОВНО: MethodExtractor.Extract, MethodKnn.cs строки 37-59.
        public static List<Packed> Extract(string source, short fileId, int variant)
        {
            var root = CSharpSyntaxTree.ParseText(source).GetRoot();
            var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();
            var packed = new List<Packed>();
            for (var index = 0; index < methods.Count; index++)
            {
                var method = methods[index];
                packed.Add(new Packed
                {
                    PackedString = PackMethod(method),
                    KindBag = ExtractKindBag(method, variant),
                    FileId = fileId,
                    IndexInFile = index
                });
            }

            return packed;
        }

        // ДОСЛОВНО: JaccardDistance, MethodKnn.cs строки 276-283 до переноса и
        // 306-313 после. Текст переносом не менялся.
        public static double JaccardDistance(HashSet<string> a, HashSet<string> b)
        {
            if (a.Count == 0 && b.Count == 0)
                return 0;

            var intersection = a.Count(b.Contains);
            var union = a.Count + b.Count - intersection;
            return union == 0 ? 0 : 1.0 - intersection / (double)union;
        }

        // ДОСЛОВНО: CanonicalType, MethodKnn.cs строки 297-310 до переноса.
        public static string CanonicalType(HashSet<string> kinds)
        {
            if (kinds.Contains("Try") || kinds.Contains("Catch"))
                return "ErrorHandling";
            if (kinds.Contains("If") && kinds.Contains("Ret"))
                return "Guard";
            if (kinds.Contains("ForEach") || kinds.Contains("For"))
                return "Loop";
            if (kinds.Contains("Await"))
                return "Async";
            if (kinds.Count == 0)
                return "Simple";
            return "Mixed";
        }

        // ДОСЛОВНО: MethodKnnPredictor.Train, MethodKnn.cs строки 195-206.
        public static List<Packed[]> Train(List<Packed> methods)
        {
            var pairs = new List<Packed[]>();
            for (var index = 0; index < methods.Count - 1; index++)
            {
                if (methods[index].FileId == methods[index + 1].FileId &&
                    methods[index + 1].IndexInFile == methods[index].IndexInFile + 1)
                {
                    pairs.Add(new[] { methods[index], methods[index + 1] });
                }
            }

            return pairs;
        }

        // ДОСЛОВНО: MethodKnnPredictor.Predict, MethodKnn.cs строки 259-272.
        public static string Predict(List<Packed[]> pairs, Packed query, int k)
        {
            if (pairs.Count == 0)
                return "Mixed";

            return pairs
                .OrderBy(pair => JaccardDistance(query.KindBag, pair[0].KindBag))
                .Take(k)
                .GroupBy(pair => CanonicalType(pair[1].KindBag))
                .OrderByDescending(group => group.Count())
                .ThenBy(group => group.Key, StringComparer.Ordinal)
                .First()
                .Key;
        }
    }
}
'@

# Явный перечень ссылок заменяет набор по умолчанию, а не дополняет его, поэтому
# к двум библиотекам разбора добавляется набор ссылочных сборок платформы для
# net9.0 — той же целевой платформы, что объявлена у проекта 12.SemanticCompiler.
# Берутся именно ссылочные сборки, а не файлы времени исполнения: в последних
# типы вроде HashSet перенаправлены в закрытую сборку ядра.
$refPackRoot = Join-Path $env:ProgramFiles 'dotnet\packs\Microsoft.NETCore.App.Ref'
$refDir = Get-ChildItem -LiteralPath $refPackRoot -Directory -ErrorAction SilentlyContinue |
    ForEach-Object { Join-Path $_.FullName 'ref\net9.0' } |
    Where-Object { Test-Path -LiteralPath $_ } |
    Sort-Object -Descending | Select-Object -First 1

if (-not $refDir) {
    throw "Не найден набор ссылочных сборок net9.0 под $refPackRoot"
}

Write-Host "Ссылочные сборки платформы: $refDir"
Write-Host ''
$frameworkRefs = Get-ChildItem -LiteralPath $refDir -Filter '*.dll' | ForEach-Object { $_.FullName }

# -IgnoreWarnings гасит предупреждение о разных версиях System.Runtime у
# библиотеки разбора (собрана под net8.0) и у ссылочных сборок net9.0. На
# поведение переносимой логики это не влияет: сравниваются виды узлов дерева
# разбора, а не версии сборок.
Add-Type -TypeDefinition $csharp `
    -ReferencedAssemblies (@($roslynCommon, $roslynCSharp) + $frameworkRefs) `
    -IgnoreWarnings `
    -ErrorAction Stop

# ── Фактические учебные данные восемьдесят третьего опыта ────────────────────
# Источник: experiments-v2/RP01-MICRO/P8301/Program.cs строки 72-79 (учебные
# файлы), 86-89 (экзамен), 93 (правильные ответы), 557-562 (края),
# 631-639 (объём шесть копий каждой формы). Вызовы R() и Calc() нигде не
# определены: разборщик строит дерево синтаксиса, и это извлечению не мешает.

$trainFiles = @(
    @{ Name = 'F1 асинхронный через стрелку + блочный двойник'; Source = 'class A{async Task<int> M()=>await R();async Task<int> N(){return await R();}}' }
    @{ Name = 'F2 оба асинхронных блочных';                     Source = 'class B{async Task<int> M(){return await R();}async Task<int> N(){return await R();}}' }
    @{ Name = 'F3 обычный через стрелку + блочный двойник';      Source = 'class C{int M()=>Calc();int N(){return Calc();}}' }
    @{ Name = 'F4 оба обычных блочных';                         Source = 'class D{int M(){return Calc();}int N(){return Calc();}}' }
)

$examFiles = @(
    @{ Name = 'Q1 асинхронный через стрелку'; Source = 'class Q1{async Task<int> Query()=>await R();}'; Gold = 'Async' }
    @{ Name = 'Q2 асинхронный блочный';       Source = 'class Q2{async Task<int> Query(){return await R();}}'; Gold = 'Async' }
    @{ Name = 'Q3 обычный через стрелку';     Source = 'class Q3{int Query()=>Calc();}'; Gold = 'Mixed' }
    @{ Name = 'Q4 обычный блочный';           Source = 'class Q4{int Query(){return Calc();}}'; Gold = 'Mixed' }
)

$edgeFiles = @(
    @{ Name = 'асинхронный через стрелку';         Source = 'class E1{async Task<int> M()=>await R();}' }
    @{ Name = 'его блочный смысловой двойник';     Source = 'class E2{async Task<int> M(){return await R();}}' }
    @{ Name = 'обычный через стрелку';             Source = 'class E3{int M()=>Calc();}' }
    @{ Name = 'абстрактный метод без тела';        Source = 'abstract class E4{abstract int M();}' }
    @{ Name = 'объявление метода в интерфейсе';    Source = 'interface E5{int M();}' }
    @{ Name = 'внешний метод без тела';            Source = 'class E6{extern int M();}' }
)

# Объём, превышающий число соседей: шесть копий каждой из четырёх форм.
$volumeFiles = [System.Collections.Generic.List[hashtable]]::new()
foreach ($copy in 1..6) {
    $volumeFiles.Add(@{ Name = "F1 копия $copy"; Source = "class A$copy{async Task<int> M()=>await R();async Task<int> N(){return await R();}}" })
    $volumeFiles.Add(@{ Name = "F2 копия $copy"; Source = "class B$copy{async Task<int> M(){return await R();}async Task<int> N(){return await R();}}" })
    $volumeFiles.Add(@{ Name = "F3 копия $copy"; Source = "class C$copy{int M()=>Calc();int N(){return Calc();}}" })
    $volumeFiles.Add(@{ Name = "F4 копия $copy"; Source = "class D$copy{int M(){return Calc();}int N(){return Calc();}}" })
}

$variants = [ordered]@{ 'прежний' = 0; 'новый' = 1; 'испорченный' = 2 }

function Collect($files, [int]$variant) {
    $all = [System.Collections.Generic.List[OracleP83.Packed]]::new()
    for ($i = 0; $i -lt $files.Count; $i++) {
        foreach ($m in [OracleP83.Mechanism]::Extract($files[$i].Source, [short]$i, $variant)) { $all.Add($m) }
    }
    return $all
}

$failures = [System.Collections.Generic.List[string]]::new()
function Check([string]$what, $expected, $actual) {
    $ok = ("$expected" -eq "$actual")
    $mark = if ($ok) { 'ок  ' } else { 'ПЛОХО' }
    Write-Host ("  {0} {1}: ожидалось {2}, получено {3}" -f $mark, $what, $expected, $actual)
    if (-not $ok) { $failures.Add("$what : ожидалось $expected, получено $actual") }
}

# ── Панель 1: признаки и вид метода на краях ─────────────────────────────────
Write-Host 'Панель 1. Признаки и вид метода на краях, по вариантам чтения.'
$edgeBags = @{}
foreach ($v in $variants.Keys) {
    $edgeBags[$v] = @{}
    foreach ($f in $edgeFiles) {
        $m = [OracleP83.Mechanism]::Extract($f.Source, [short]0, $variants[$v])[0]
        $edgeBags[$v][$f.Name] = $m
    }
}
foreach ($f in $edgeFiles) {
    $line = '  {0,-32}' -f $f.Name
    foreach ($v in $variants.Keys) {
        $m = $edgeBags[$v][$f.Name]
        $set = if ($m.KindBag.Count -eq 0) { 'пусто' } else { ($m.KindBag | Sort-Object) -join ',' }
        $kind = [OracleP83.Mechanism]::CanonicalType($m.KindBag)
        $line += ' | {0}: {1} ({2})' -f $v, $set, $kind
    }
    Write-Host $line
}
Write-Host ''
Write-Host 'Панель 1, сверка с показаниями опыта (panel-5-edges-and-two-sided-distance.json):'
Check 'прежний, асинхронный через стрелку — признаков' 0 $edgeBags['прежний']['асинхронный через стрелку'].KindBag.Count
Check 'прежний, асинхронный через стрелку — вид' 'Simple' ([OracleP83.Mechanism]::CanonicalType($edgeBags['прежний']['асинхронный через стрелку'].KindBag))
Check 'новый, асинхронный через стрелку — признаки' 'Await,Call' (($edgeBags['новый']['асинхронный через стрелку'].KindBag | Sort-Object) -join ',')
Check 'новый, асинхронный через стрелку — вид' 'Async' ([OracleP83.Mechanism]::CanonicalType($edgeBags['новый']['асинхронный через стрелку'].KindBag))
Check 'новый, обычный через стрелку — признаки' 'Call' (($edgeBags['новый']['обычный через стрелку'].KindBag | Sort-Object) -join ',')
Check 'новый, блочный двойник — признаки' 'Await,Call,Ret' (($edgeBags['новый']['его блочный смысловой двойник'].KindBag | Sort-Object) -join ',')
Check 'новый, абстрактный без тела — признаков' 0 $edgeBags['новый']['абстрактный метод без тела'].KindBag.Count
Check 'новый, объявление в интерфейсе — признаков' 0 $edgeBags['новый']['объявление метода в интерфейсе'].KindBag.Count
Check 'новый, внешний без тела — признаков' 0 $edgeBags['новый']['внешний метод без тела'].KindBag.Count
Write-Host ''

# ── Панель 2: двусторонний дефект расстояния ─────────────────────────────────
Write-Host 'Панель 2. Расстояние от асинхронного однострочника до соседей.'
$pairsToMeasure = @(
    'его блочный смысловой двойник'
    'абстрактный метод без тела'
    'объявление метода в интерфейсе'
    'внешний метод без тела'
    'обычный через стрелку'
)
foreach ($right in $pairsToMeasure) {
    $line = '  до {0,-32}' -f $right
    foreach ($v in $variants.Keys) {
        $d = [OracleP83.Mechanism]::JaccardDistance(
            $edgeBags[$v]['асинхронный через стрелку'].KindBag,
            $edgeBags[$v][$right].KindBag)
        $line += ' | {0}: {1}' -f $v, $d.ToString('0.000', [Globalization.CultureInfo]::InvariantCulture)
    }
    Write-Host $line
}
Write-Host ''
Write-Host 'Панель 2, сверка с показаниями опыта:'
function Dist($v, $right) {
    return [OracleP83.Mechanism]::JaccardDistance(
        $edgeBags[$v]['асинхронный через стрелку'].KindBag,
        $edgeBags[$v][$right].KindBag).ToString('0.000', [Globalization.CultureInfo]::InvariantCulture)
}
Check 'прежний: однострочник до своего блочного двойника' '1.000' (Dist 'прежний' 'его блочный смысловой двойник')
Check 'прежний: однострочник до абстрактного без тела' '0.000' (Dist 'прежний' 'абстрактный метод без тела')
Check 'прежний: однострочник до объявления в интерфейсе' '0.000' (Dist 'прежний' 'объявление метода в интерфейсе')
Check 'прежний: однострочник до внешнего без тела' '0.000' (Dist 'прежний' 'внешний метод без тела')
Check 'прежний: однострочник до обычного однострочника' '0.000' (Dist 'прежний' 'обычный через стрелку')
Check 'новый: однострочник до своего блочного двойника' '0.333' (Dist 'новый' 'его блочный смысловой двойник')
Check 'новый: однострочник до абстрактного без тела' '1.000' (Dist 'новый' 'абстрактный метод без тела')
Check 'новый: однострочник до объявления в интерфейсе' '1.000' (Dist 'новый' 'объявление метода в интерфейсе')
Check 'новый: однострочник до внешнего без тела' '1.000' (Dist 'новый' 'внешний метод без тела')
Check 'новый: однострочник до обычного однострочника' '0.500' (Dist 'новый' 'обычный через стрелку')
Write-Host ''

# ── Панель 3: экзамен при одном соседе на объёме карточки ────────────────────
Write-Host 'Панель 3. Экзамен при одном соседе, четыре учебные пары.'
$examResults = @{}
foreach ($v in $variants.Keys) {
    $methods = Collect $trainFiles $variants[$v]
    $pairs = [OracleP83.Mechanism]::Train($methods)
    $correct = 0
    foreach ($q in $examFiles) {
        $query = [OracleP83.Mechanism]::Extract($q.Source, [short]99, $variants[$v])[0]
        $answer = [OracleP83.Mechanism]::Predict($pairs, $query, 1)
        if ($answer -eq $q.Gold) { $correct++ }
    }
    $examResults[$v] = $correct
    Write-Host ('  {0,-12}: пар {1}, верных ответов {2} из {3}' -f $v, $pairs.Count, $correct, $examFiles.Count)
}
Write-Host ''
Write-Host 'Панель 3, сверка с показаниями опыта (отчёт, строки 182-184):'
Check 'прежний: экзамен при одном соседе' 3 $examResults['прежний']
Check 'новый: экзамен при одном соседе' 4 $examResults['новый']
Write-Host ''

# ── Панель 4: продуктовая настройка — пять соседей, объём 24 пары ────────────
Write-Host 'Панель 4. Продуктовая настройка: пять соседей, объём двадцать четыре пары.'
$volumeExam = @{}
$volumeControl = @{}
$emptyBags = @{}
foreach ($v in $variants.Keys) {
    $methods = Collect $volumeFiles $variants[$v]
    $pairs = [OracleP83.Mechanism]::Train($methods)

    $correctExam = 0
    foreach ($q in $examFiles) {
        $query = [OracleP83.Mechanism]::Extract($q.Source, [short]99, $variants[$v])[0]
        if ([OracleP83.Mechanism]::Predict($pairs, $query, 5) -eq $q.Gold) { $correctExam++ }
    }

    $correctControl = 0
    foreach ($pair in $pairs) {
        $expected = [OracleP83.Mechanism]::CanonicalType($pair[1].KindBag)
        if ([OracleP83.Mechanism]::Predict($pairs, $pair[0], 5) -eq $expected) { $correctControl++ }
    }

    # Пустые наборы признаков в наборе карточки: восемь учебных методов плюс
    # четыре экзаменационных, всего двенадцать.
    $cardMethods = Collect $trainFiles $variants[$v]
    $empty = 0
    foreach ($m in $cardMethods) { if ($m.KindBag.Count -eq 0) { $empty++ } }
    foreach ($q in $examFiles) {
        if ([OracleP83.Mechanism]::Extract($q.Source, [short]99, $variants[$v])[0].KindBag.Count -eq 0) { $empty++ }
    }

    $volumeExam[$v] = $correctExam
    $volumeControl[$v] = $correctControl
    $emptyBags[$v] = $empty
    Write-Host ('  {0,-12}: пар {1}, экзамен {2} из 4, контроль на учебных {3} из {4}, пустых наборов {5} из 12' -f `
        $v, $pairs.Count, $correctExam, $correctControl, $pairs.Count, $empty)
}
Write-Host ''
Write-Host 'Панель 4, сверка с показаниями опыта (panel-6-production-setting-with-volume.json):'
Check 'прежний: экзамен при пяти соседях' 3 $volumeExam['прежний']
Check 'новый: экзамен при пяти соседях' 4 $volumeExam['новый']
Check 'прежний: контроль на учебных вопросах' 18 $volumeControl['прежний']
Check 'новый: контроль на учебных вопросах' 24 $volumeControl['новый']
Check 'прежний: пустых наборов признаков' 4 $emptyBags['прежний']
Check 'новый: пустых наборов признаков' 0 $emptyBags['новый']
Write-Host ''

# ── Панель 5: потерянных случаев нет ────────────────────────────────────────
Write-Host 'Панель 5. Потерянные случаи: где прежний отвечает верно, а новый неверно.'
$lost = 0
foreach ($k in @(1, 5)) {
    foreach ($setName in @('карточка', 'объём')) {
        $files = if ($setName -eq 'карточка') { $trainFiles } else { $volumeFiles }
        $pairsOld = [OracleP83.Mechanism]::Train((Collect $files $variants['прежний']))
        $pairsNew = [OracleP83.Mechanism]::Train((Collect $files $variants['новый']))
        foreach ($q in $examFiles) {
            $qOld = [OracleP83.Mechanism]::Extract($q.Source, [short]99, $variants['прежний'])[0]
            $qNew = [OracleP83.Mechanism]::Extract($q.Source, [short]99, $variants['новый'])[0]
            $aOld = [OracleP83.Mechanism]::Predict($pairsOld, $qOld, $k)
            $aNew = [OracleP83.Mechanism]::Predict($pairsNew, $qNew, $k)
            if (($aOld -eq $q.Gold) -and ($aNew -ne $q.Gold)) {
                $lost++
                Write-Host ('  ПОТЕРЯ: {0}, соседей {1}, набор {2}: прежний {3}, новый {4}, правильно {5}' -f `
                    $q.Name, $k, $setName, $aOld, $aNew, $q.Gold)
            }
        }
    }
}
Write-Host ('  потерянных случаев: {0}' -f $lost)
Check 'потерянных случаев во всех панелях' 0 $lost
Write-Host ''

# ── Панель 6: чувствительность — испорченная логика обязана ронять проверку ──
Write-Host 'Панель 6. Чувствительность: две строки чтения узлов выровнены между собой,'
Write-Host '          то есть у выражения-тела читается поддерево БЕЗ самого выражения.'
Write-Host '          Если бы такая логика стояла в продукте, перечисленные ниже сверки'
Write-Host '          панелей 1, 2 и 4 упали бы. Это и есть проверка на чувствительность.'
Write-Host ''

# Те же ожидания, что уже проверены для нового варианта, но предъявленные
# испорченному. Каждое несовпадение здесь — доказательство, что оракул
# отличает правильную логику от выровненной, а не проходит на любой.
$mutationChecks = @(
    @{ What = 'набор признаков асинхронного однострочника'
       Expected = 'Await,Call'
       Actual = (($edgeBags['испорченный']['асинхронный через стрелку'].KindBag | Sort-Object) -join ',') }
    @{ What = 'набор признаков обычного однострочника'
       Expected = 'Call'
       Actual = $(if ($edgeBags['испорченный']['обычный через стрелку'].KindBag.Count -eq 0) { 'пусто' }
                  else { ($edgeBags['испорченный']['обычный через стрелку'].KindBag | Sort-Object) -join ',' }) }
    @{ What = 'вид асинхронного однострочника'
       Expected = 'Async'
       Actual = [OracleP83.Mechanism]::CanonicalType($edgeBags['испорченный']['асинхронный через стрелку'].KindBag) }
    @{ What = 'расстояние до своего блочного двойника'
       Expected = '0.333'
       Actual = (Dist 'испорченный' 'его блочный смысловой двойник') }
    @{ What = 'расстояние до обычного однострочника'
       Expected = '0.500'
       Actual = (Dist 'испорченный' 'обычный через стрелку') }
    @{ What = 'пустых наборов признаков из двенадцати'
       Expected = '0'
       Actual = "$($emptyBags['испорченный'])" }
)

$caught = 0
foreach ($c in $mutationChecks) {
    $differs = ("$($c.Expected)" -ne "$($c.Actual)")
    if ($differs) { $caught++ }
    $mark = if ($differs) { 'упало ' } else { 'прошло' }
    Write-Host ('  {0} {1}: у нового {2}, у испорченного {3}' -f $mark, $c.What, $c.Expected, $c.Actual)
}
Write-Host ''
Check 'испорченная логика роняет сверки нового варианта (должна ронять хотя бы одну)' $true ($caught -ge 1)
Write-Host ('  испорченная логика уронила сверок: {0} из {1}' -f $caught, $mutationChecks.Count)
Write-Host ''

# Честная граница самой проверки на чувствительность, измеренная, а не выведенная.
# Карточка переноса предсказывала схлопывание наборов признаков из устройства
# кода и прямо отмечала, что это вывод, а не отдельное измерение. Схлопывание
# подтвердилось полностью. Но на точности ЭТИХ панелей испорченная логика НЕ
# отличается от правильной: голос всё равно попадает в верный вид, потому что
# в наборе карточки у каждой формы есть своя копия, и запрос с пустым набором
# оказывается ближе всего к такому же пустому набору своей формы.
Write-Host 'Панель 6, честная граница: на точности этих панелей испорченная логика НЕ ловится.'
Write-Host ('  экзамен при одном соседе:   новый {0} из 4, испорченный {1} из 4' -f $examResults['новый'], $examResults['испорченный'])
Write-Host ('  экзамен при пяти соседях:   новый {0} из 4, испорченный {1} из 4' -f $volumeExam['новый'], $volumeExam['испорченный'])
Write-Host ('  контроль на учебных:        новый {0} из 24, испорченный {1} из 24' -f $volumeControl['новый'], $volumeControl['испорченный'])
Write-Host '  Значит ловить выравнивание двух строк обязаны сверки наборов признаков и'
Write-Host '  расстояний, а не числа точности. Это измеренный факт, а не предположение.'
Write-Host ''

# ── Итог ────────────────────────────────────────────────────────────────────
if ($failures.Count -eq 0) {
    Write-Host 'ИТОГ: все сверки прошли. Перенос воспроизводит показания опыта, испорченная логика отличается.'
    exit 0
}

Write-Host ('ИТОГ: несовпадений {0}:' -f $failures.Count)
foreach ($f in $failures) { Write-Host "  - $f" }
exit 1
