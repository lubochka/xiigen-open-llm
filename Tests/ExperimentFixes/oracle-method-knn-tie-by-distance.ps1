# Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
# Licensed under GNU Affero General Public License v3.0.
# SPDX-License-Identifier: AGPL-3.0-only
# See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
#
# Проверяемая программа переноса: ровный голос методного предсказателя
# разрешается расстоянием, а не написанием метки.
#
# Здесь исполняется НАСТОЯЩИЙ продуктовый код проекта 12, собранный в памяти
# процесса, в ТРЁХ версиях: нынешняя берётся с диска; прежняя получается
# обратной подстановкой — из нынешнего текста убирается ровно одна поведенческая
# строка правки; порченая получается заменой «ближайший» на «дальний». Правленый
# и порченый текст существуют ТОЛЬКО В ПАМЯТИ, на диск не записываются. Команды
# гита не вызываются ни одной — поэтому прежняя версия строится подстановкой, а
# не выниманием из опубликованной вершины.
#
# Оснастка НЕ придумана: четыре обучающих текста и четыре запроса взяты дословно
# из замороженного хозяина восемьдесят пятого опыта
# (`experiments-v2/RP01-MICRO/P8501/Program.cs`, строки 61–71 и 458–464), а
# ожидаемые ответы — из его сырых показаний
# (`.../results/attempt-0001/panel-production.json`). Хозяин опыта только
# читается.
#
# Панели:
#   1. Две сборки, три версии проекта 12.
#   2. Измеренная боевая панель: прежняя версия отдаёт одну метку на все запросы.
#   3. Нынешняя версия отдаёт метки ближайших пар — как при одном соседе.
#   4. Эталон вторым способом: один сосед даёт тот же ответ в ОБЕИХ версиях.
#   5. Сторона «не ломает законное»: двадцать четыре пары, обе версии 24 из 24.
#   6. Инвариант «написание метки не решает»: ближайшая метка последняя по алфавиту.
#   7. Чувствительность: порча правки обязана быть поймана.

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8
if ($null -ne $PSStyle) { $PSStyle.OutputRendering = 'PlainText' }

$checks = 0
$failures = 0

function Check([string]$what, $expected, $actual) {
    $script:checks++
    if ("$expected" -eq "$actual") {
        Write-Output ("  ок     {0} — ожидалось {1}, получено {2}" -f $what, $expected, $actual)
    } else {
        $script:failures++
        Write-Output ("  УПАЛО  {0} — ожидалось {1}, получено {2}" -f $what, $expected, $actual)
    }
}

$sdk = Get-ChildItem 'C:\Program Files\dotnet\sdk' -Directory |
    Where-Object { $_.Name -like '9.*' } | Sort-Object Name -Descending | Select-Object -First 1
$bin = Join-Path $sdk.FullName 'Roslyn\bincore'
Add-Type -Path (Join-Path $bin 'Microsoft.CodeAnalysis.dll')
Add-Type -Path (Join-Path $bin 'Microsoft.CodeAnalysis.CSharp.dll')

$refPack = Get-ChildItem 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref' -Directory |
    Where-Object { $_.Name -like '9.*' } | Sort-Object Name -Descending | Select-Object -First 1
$refDirectory = Join-Path $refPack.FullName 'ref\net9.0'
$frameworkReferences = [System.Collections.Generic.List[Microsoft.CodeAnalysis.MetadataReference]]::new()
foreach ($dll in Get-ChildItem $refDirectory -Filter '*.dll') {
    $frameworkReferences.Add([Microsoft.CodeAnalysis.MetadataReference]::CreateFromFile($dll.FullName))
}

# Проект 12 ссылается на разборщик кода пакетом 4.12.0. Для СБОРКИ нужна его
# метаданная ссылка; в ПРОЦЕССЕ уже загружен разборщик из набора средств
# разработки, и разрешитель по простому имени отдаёт именно его — различие версий
# здесь безразлично, потому что используется только разбор текста в дерево.
$nuget = Join-Path $env:USERPROFILE '.nuget\packages'
$roslynReferences = @(
    (Join-Path $nuget 'microsoft.codeanalysis.csharp\4.12.0\lib\net8.0\Microsoft.CodeAnalysis.CSharp.dll'),
    (Join-Path $nuget 'microsoft.codeanalysis.common\4.12.0\lib\net8.0\Microsoft.CodeAnalysis.dll')
)
foreach ($p in $roslynReferences) {
    if (-not (Test-Path -LiteralPath $p)) { throw ('нет библиотеки разборщика: ' + $p) }
}

# Сборки живут только в памяти, файлов на диске у них нет, поэтому разрешитель
# обязан отдавать их по имени.
$script:LoadedByName = @{}
[System.AppDomain]::CurrentDomain.add_AssemblyResolve({
    param($sender, $eventArgs)
    $simpleName = ($eventArgs.Name -split ',')[0]
    if ($script:LoadedByName.ContainsKey($simpleName)) { return $script:LoadedByName[$simpleName] }
    foreach ($loaded in [System.AppDomain]::CurrentDomain.GetAssemblies()) {
        if ($loaded.GetName().Name -eq $simpleName) { return $loaded }
    }
    return $null
})

$implicitUsings = @'
global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;
'@

$repoRoot = 'C:\Projects\llm_mvp_core'
$parseOptions = [Microsoft.CodeAnalysis.CSharp.CSharpParseOptions]::Default.WithLanguageVersion(
    [Microsoft.CodeAnalysis.CSharp.LanguageVersion]::Latest)

function Get-ProjectSources([string]$projectDirectory) {
    return @(Get-ChildItem (Join-Path $repoRoot $projectDirectory) -Recurse -Filter '*.cs' -File |
        Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' } |
        Sort-Object FullName)
}

function Build-Project(
    [string]$assemblyName,
    [string]$projectDirectory,
    [System.Collections.Generic.List[Microsoft.CodeAnalysis.MetadataReference]]$references,
    [hashtable]$replacements,
    [string]$extraSource) {

    $trees = [System.Collections.Generic.List[Microsoft.CodeAnalysis.SyntaxTree]]::new()
    $replacedCount = 0
    foreach ($file in Get-ProjectSources $projectDirectory) {
        $text = [System.IO.File]::ReadAllText($file.FullName)
        # Подстановка идёт по тексту файла как он лежит на диске: ключом служит
        # содержимое, и любая предварительная правка текста ключ рассогласует.
        if ($null -ne $replacements) {
            foreach ($key in $replacements.Keys) {
                if (-not $text.Contains($key)) { continue }
                $text = $text.Replace($key, $replacements[$key])
                $replacedCount++
            }
        }
        $trees.Add([Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($text, $parseOptions))
    }
    $trees.Add([Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($implicitUsings, $parseOptions))
    if (-not [string]::IsNullOrEmpty($extraSource)) {
        $trees.Add([Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($extraSource, $parseOptions))
    }

    $options = [Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions]::new(
        [Microsoft.CodeAnalysis.OutputKind]::DynamicallyLinkedLibrary)
    $options = $options.WithNullableContextOptions([Microsoft.CodeAnalysis.NullableContextOptions]::Enable)
    $compilation = [Microsoft.CodeAnalysis.CSharp.CSharpCompilation]::Create(
        $assemblyName, $trees, $references, $options)
    $stream = New-Object System.IO.MemoryStream
    $emit = $compilation.Emit($stream)
    if (-not $emit.Success) {
        [Console]::Out.WriteLine("      ошибки сборки (" + $assemblyName + "):")
        foreach ($d in @($emit.Diagnostics |
            Where-Object { $_.Severity -eq [Microsoft.CodeAnalysis.DiagnosticSeverity]::Error } |
            Select-Object -First 8)) {
            [Console]::Out.WriteLine('        ' + $d.ToString())
        }
        throw ('сборка в памяти не удалась: ' + $assemblyName)
    }
    $bytes = $stream.ToArray()
    $assembly = [System.Reflection.Assembly]::Load($bytes)
    $script:LoadedByName[$assembly.GetName().Name] = $assembly
    return [ordered]@{ Bytes = $bytes; Assembly = $assembly; Replaced = $replacedCount }
}

function New-ReferenceSet([object[]]$built, [string[]]$extraPaths) {
    $set = [System.Collections.Generic.List[Microsoft.CodeAnalysis.MetadataReference]]::new()
    foreach ($r in $frameworkReferences) { $set.Add($r) }
    foreach ($b in $built) { $set.Add([Microsoft.CodeAnalysis.MetadataReference]::CreateFromImage($b.Bytes)) }
    foreach ($p in $extraPaths) { $set.Add([Microsoft.CodeAnalysis.MetadataReference]::CreateFromFile($p)) }
    # Запятая обязательна: иначе оболочка развернёт список на выходе из функции.
    return ,$set
}

Write-Output "Проверяемая программа: ровный голос разрешается расстоянием, а не написанием метки."
Write-Output ("Разборщик: " + $sdk.Name + "; эталонные сборки: " + $refPack.Name)
Write-Output ""

# ── Панель 1. Две сборки, три версии проекта 12 ────────────────────────────
Write-Output "Панель 1. Две сборки, три версии проекта 12 (нынешняя, прежняя, порченая)."

# Единственная ПОВЕДЕНЧЕСКАЯ строка правки. Её удаление возвращает прежнее
# поведение целиком: перенос расстояний рядом с парой поведения не меняет.
$distanceTieLine = '            .ThenBy(group => group.Min(item => item.Distance))' + "`r`n"
$distanceTieLineLf = '            .ThenBy(group => group.Min(item => item.Distance))' + "`n"
$farthestTieLine = '            .ThenByDescending(group => group.Min(item => item.Distance))' + "`n"

$probeSource = @'
namespace SemanticCompiler.Prediction;

// Проба живёт в ТОЙ ЖЕ сборке, что проект 12: так доступны и предсказатель, и
// извлечение признаков, без правки продуктовых файлов.
public static class MethodKnnTieProbe
{
    // Разобрать исходник и вернуть его методы, как это делает хозяин опыта.
    public static PackedMethod[] Pack(string source, short fileId)
    {
        var extracted = MethodExtractor.Extract(source, fileId);
        if (!extracted.IsSuccess)
            throw new InvalidOperationException("извлечение не удалось: " + extracted.ErrorMessage);
        return extracted.Value!.ToArray();
    }

    // Вид метода по тому же продуктовому правилу, которым пользуется голос.
    public static string Label(PackedMethod method) => MethodKnnPredictor.CanonicalType(method.KindBag);

    // Обучить предсказатель и задать ему один запрос.
    public static string PredictWith(int k, PackedMethod[] training, PackedMethod query)
    {
        var predictor = new MethodKnnPredictor(k);
        predictor.Train(training);
        return predictor.Predict(query);
    }

    // Сколько пар получилось из обучающего набора.
    public static int TrainedPairs(int k, PackedMethod[] training)
    {
        var predictor = new MethodKnnPredictor(k);
        predictor.Train(training);
        return predictor.TrainingSize;
    }

    // Расстояние тем же продуктовым измерителем.
    public static double Distance(PackedMethod a, PackedMethod b)
        => MethodKnnPredictor.JaccardDistance(a.KindBag, b.KindBag);

    // Сколько выученных пар отвечают своим же продолжением: это инвариант
    // существующей постоянной проверки площадки, и правка обязана его держать.
    public static int PairsAnsweringTheirOwnTarget(int k, PackedMethod[] methods)
    {
        var predictor = new MethodKnnPredictor(k);
        predictor.Train(methods);
        var correct = 0;
        for (var index = 0; index < methods.Length - 1; index++)
        {
            if (methods[index].FileId != methods[index + 1].FileId ||
                methods[index + 1].IndexInFile != methods[index].IndexInFile + 1)
                continue;
            var expected = MethodKnnPredictor.CanonicalType(methods[index + 1].KindBag);
            if (predictor.Predict(methods[index]) == expected) correct++;
        }
        return correct;
    }
}
'@

$semantic29 = Build-Project 'SemanticCompiler29' '29.SemanticCompiler' (New-ReferenceSet @() @()) $null ''
Check "собран проект 29" $true ($null -ne $semantic29.Assembly)

$referencesFor12 = New-ReferenceSet @($semantic29) $roslynReferences

$current = Build-Project 'SemanticCompilerCurrent' '12.SemanticCompiler' $referencesFor12 $null $probeSource
Check "собрана нынешняя версия проекта 12" $true ($null -ne $current.Assembly)

$before = Build-Project 'SemanticCompilerBefore' '12.SemanticCompiler' (New-ReferenceSet @($semantic29) $roslynReferences) @{ $distanceTieLineLf = '' } $probeSource
Check "собрана прежняя версия проекта 12" $true ($null -ne $before.Assembly)
Check "  прежняя версия получена ровно одной подстановкой" 1 $before.Replaced

$broken = Build-Project 'SemanticCompilerBroken' '12.SemanticCompiler' (New-ReferenceSet @($semantic29) $roslynReferences) @{ $distanceTieLineLf = $farthestTieLine } $probeSource
Check "собрана порченая версия проекта 12" $true ($null -ne $broken.Assembly)
Check "  порченая версия получена ровно одной подстановкой" 1 $broken.Replaced

$probeCurrent = $current.Assembly.GetType('SemanticCompiler.Prediction.MethodKnnTieProbe')
$probeBefore  = $before.Assembly.GetType('SemanticCompiler.Prediction.MethodKnnTieProbe')
$probeBroken  = $broken.Assembly.GetType('SemanticCompiler.Prediction.MethodKnnTieProbe')
Check "пробы найдены во всех трёх версиях" $true (($null -ne $probeCurrent) -and ($null -ne $probeBefore) -and ($null -ne $probeBroken))
Write-Output ""

# ── Оснастка: дословно из замороженного хозяина восемьдесят пятого опыта ───
$trainGuardAsync = 'class GA{void A(int x){if(x<0)return;} async System.Threading.Tasks.Task B(int x){if(x<0)return;await System.Threading.Tasks.Task.Delay(1);}}'
$trainLoop = 'class LP{void A(){for(int i=0;i<2;i++){}} void B(){for(int i=0;i<3;i++){}}}'
$trainErrorHandling = 'class EH{void A(int x){try{}catch{}} void B(int x){try{}catch{}}}'
$trainSimple = 'class SP{void A(){int a=1;} void B(){int b=2;}}'

$examSources = @(
    @{ Name = 'guard-0'; Source = 'class Q{void Q1(int n){if(n<0)return;}}'; Gold = 'Guard' },
    @{ Name = 'guard-1'; Source = 'class Q{void Q2(int n){if(n<1)return;}}'; Gold = 'Guard' },
    @{ Name = 'loop-0';  Source = 'class Q{void Q3(){for(int j=0;j<4;j++){}}}'; Gold = 'Loop' },
    @{ Name = 'loop-1';  Source = 'class Q{void Q4(){for(int j=1;j<5;j++){}}}'; Gold = 'Loop' }
)

function Invoke-Probe($probeType, [string]$method, [object[]]$values) {
    $arguments = New-Object 'object[]' $values.Length
    for ($i = 0; $i -lt $values.Length; $i++) { $arguments[$i] = $values[$i] }
    return $probeType.GetMethod($method).Invoke($null, $arguments)
}

function Build-ProductionTraining($probeType) {
    $all = New-Object System.Collections.ArrayList
    foreach ($pair in @(@($trainGuardAsync, 0), @($trainLoop, 1), @($trainErrorHandling, 2), @($trainSimple, 3))) {
        $packed = Invoke-Probe $probeType 'Pack' @($pair[0], [int16]$pair[1])
        foreach ($m in $packed) { [void]$all.Add($m) }
    }
    $elementType = $probeType.Assembly.GetType('SemanticCompiler.Prediction.PackedMethod')
    $array = [System.Array]::CreateInstance($elementType, $all.Count)
    for ($i = 0; $i -lt $all.Count; $i++) { $array[$i] = $all[$i] }
    return ,$array
}

# ── Панель 2. Измеренная боевая панель: прежняя версия ─────────────────────
Write-Output "Панель 2. Измеренная боевая панель: прежняя версия отдаёт одну метку на все запросы."
$trainingBefore = Build-ProductionTraining $probeBefore
Check "  обученных пар" 4 (Invoke-Probe $probeBefore 'TrainedPairs' @(5, $trainingBefore))
$beforeCorrect = 0
foreach ($exam in $examSources) {
    $query = (Invoke-Probe $probeBefore 'Pack' @($exam.Source, [int16]99))[0]
    $answer = Invoke-Probe $probeBefore 'PredictWith' @(5, $trainingBefore, $query)
    if ($answer -eq $exam.Gold) { $beforeCorrect++ }
    Check ("  запрос {0}: прежний ответ" -f $exam.Name) 'ErrorHandling' $answer
}
Check "  прежняя точность метки, верных из 4" 0 $beforeCorrect
Write-Output ""

# ── Панель 3. Нынешняя версия ──────────────────────────────────────────────
Write-Output "Панель 3. Нынешняя версия отдаёт метки ближайших пар."
$trainingCurrent = Build-ProductionTraining $probeCurrent
Check "  обученных пар" 4 (Invoke-Probe $probeCurrent 'TrainedPairs' @(5, $trainingCurrent))
$currentCorrect = 0
foreach ($exam in $examSources) {
    $query = (Invoke-Probe $probeCurrent 'Pack' @($exam.Source, [int16]99))[0]
    $answer = Invoke-Probe $probeCurrent 'PredictWith' @(5, $trainingCurrent, $query)
    if ($answer -eq $exam.Gold) { $currentCorrect++ }
    Check ("  запрос {0}: нынешний ответ" -f $exam.Name) $exam.Gold $answer
}
Check "  нынешняя точность метки, верных из 4" 4 $currentCorrect
Write-Output ""

# ── Панель 4. Эталон вторым способом: один сосед ───────────────────────────
Write-Output "Панель 4. Эталон вторым способом: при ОДНОМ соседе ровного голоса нет вовсе,"
Write-Output "  и обе версии обязаны дать один и тот же ответ — измеренный опытом."
foreach ($exam in $examSources) {
    $queryBefore = (Invoke-Probe $probeBefore 'Pack' @($exam.Source, [int16]99))[0]
    $queryCurrent = (Invoke-Probe $probeCurrent 'Pack' @($exam.Source, [int16]99))[0]
    $answerBefore = Invoke-Probe $probeBefore 'PredictWith' @(1, $trainingBefore, $queryBefore)
    $answerCurrent = Invoke-Probe $probeCurrent 'PredictWith' @(1, $trainingCurrent, $queryCurrent)
    Check ("  запрос {0}: один сосед, прежняя версия" -f $exam.Name) $exam.Gold $answerBefore
    Check ("  запрос {0}: один сосед, нынешняя версия" -f $exam.Name) $exam.Gold $answerCurrent
}
Write-Output ""

# ── Панель 5. Сторона «не ломает законное» ─────────────────────────────────
Write-Output "Панель 5. Сторона «не ломает законное»: инвариант существующей постоянной проверки"
Write-Output "  площадки — двадцать четыре пары при пяти соседях отвечают своим же продолжением."
function Build-VolumeTraining($probeType) {
    $all = New-Object System.Collections.ArrayList
    $fileId = 0
    for ($copy = 1; $copy -le 6; $copy++) {
        $forms = @(
            ('class A{0}{{async Task<int> M()=>await R();async Task<int> N(){{return await R();}}}}' -f $copy),
            ('class B{0}{{async Task<int> M(){{return await R();}}async Task<int> N(){{return await R();}}}}' -f $copy),
            ('class C{0}{{int M()=>Calc();int N(){{return Calc();}}}}' -f $copy),
            ('class D{0}{{int M(){{return Calc();}}int N(){{return Calc();}}}}' -f $copy)
        )
        foreach ($form in $forms) {
            $packed = Invoke-Probe $probeType 'Pack' @($form, [int16]$fileId)
            $fileId++
            foreach ($m in $packed) { [void]$all.Add($m) }
        }
    }
    $elementType = $probeType.Assembly.GetType('SemanticCompiler.Prediction.PackedMethod')
    $array = [System.Array]::CreateInstance($elementType, $all.Count)
    for ($i = 0; $i -lt $all.Count; $i++) { $array[$i] = $all[$i] }
    return ,$array
}
$volumeBefore = Build-VolumeTraining $probeBefore
$volumeCurrent = Build-VolumeTraining $probeCurrent
Check "  обученных пар объёмного набора" 24 (Invoke-Probe $probeCurrent 'TrainedPairs' @(5, $volumeCurrent))
Check "  прежняя версия: пар, отвечающих своим продолжением" 24 (Invoke-Probe $probeBefore 'PairsAnsweringTheirOwnTarget' @(5, $volumeBefore))
Check "  нынешняя версия: пар, отвечающих своим продолжением" 24 (Invoke-Probe $probeCurrent 'PairsAnsweringTheirOwnTarget' @(5, $volumeCurrent))
Write-Output "  Значит там, где голос НЕ ровный, правка не меняет ничего."
Write-Output ""

# ── Панель 6. Написание метки не решает ────────────────────────────────────
Write-Output "Панель 6. Инвариант «написание метки не решает»: запрос, чья ближайшая пара несёт"
Write-Output "  метку ПОСЛЕДНЮЮ по алфавиту. Прежняя версия обязана отдать первую по алфавиту."
$lastAlphabetQuery = 'class Q{void Q5(){int z=9;}}'
$queryBefore = (Invoke-Probe $probeBefore 'Pack' @($lastAlphabetQuery, [int16]99))[0]
$queryCurrent = (Invoke-Probe $probeCurrent 'Pack' @($lastAlphabetQuery, [int16]99))[0]
$nearestLabel = Invoke-Probe $probeCurrent 'PredictWith' @(1, $trainingCurrent, $queryCurrent)
Write-Output ("  метка ближайшей пары (один сосед): " + $nearestLabel)
Check "  ближайшая метка последняя по алфавиту из четырёх" 'Mixed' $nearestLabel
Check "  прежняя версия при пяти соседях отдала первую по алфавиту" 'ErrorHandling' (Invoke-Probe $probeBefore 'PredictWith' @(5, $trainingBefore, $queryBefore))
Check "  нынешняя версия отдала метку ближайшей пары" $nearestLabel (Invoke-Probe $probeCurrent 'PredictWith' @(5, $trainingCurrent, $queryCurrent))
Write-Output ""

# ── Панель 7. Чувствительность: порча правки ───────────────────────────────
Write-Output "Панель 7. Чувствительность: порча правки обязана быть поймана."
Write-Output "  Порча первая: вместо ближайшего соседа предпочитается ДАЛЬНИЙ."
$trainingBroken = Build-ProductionTraining $probeBroken
$brokenCorrect = 0
$brokenDiffers = 0
foreach ($exam in $examSources) {
    $query = (Invoke-Probe $probeBroken 'Pack' @($exam.Source, [int16]99))[0]
    $answer = Invoke-Probe $probeBroken 'PredictWith' @(5, $trainingBroken, $query)
    if ($answer -eq $exam.Gold) { $brokenCorrect++ }
    if ($answer -ne $exam.Gold) { $brokenDiffers++ }
    Write-Output ("    запрос {0}: порченый ответ {1}, эталон {2}" -f $exam.Name, $answer, $exam.Gold)
}
Check "  порча «дальний вместо ближайшего»: верных из 4" 0 $brokenCorrect
Check "  порча отличается от эталона на всех четырёх строках" 4 $brokenDiffers
Write-Output "  Порча вторая — это и есть прежняя версия: удаление одной строки правки"
Write-Output "  возвращает ноль из четырёх, и панель 2 это уже показала."
Write-Output ""

# ── Итог ───────────────────────────────────────────────────────────────────
Write-Output "Чего эта программа НЕ доказывает:"
Write-Output "  Она не измеряет качество на отложенной выборке: четыре запроса и четыре"
Write-Output "  обучающие пары — это панель опыта, а не отложенный набор. Обученной"
Write-Output "  способности здесь не заявляется: ни контрольной точки, ни абляции, ни"
Write-Output "  свежей загрузки, ни продолжения обучения у этой единицы нет."
Write-Output "  Она не вводит вес голоса по расстоянию: это отдельная непроверенная"
Write-Output "  гипотеза, записанная в реестре предложением карточки опыта."
Write-Output "  Она не касается двух соседних предсказателей с тем же дефектом разрешения:"
Write-Output "  их боевая достижимость не измерена, и правка неизмеренного запрещена."
Write-Output "  Родная приёмка платформы НЕ исполнялась: NOT_EXECUTED, а не PASS — лане"
Write-Output "  сборки проекта и прогона проверок закрыта правами сессии."
Write-Output ""

if ($failures -eq 0) {
    Write-Output ("ИТОГ: все " + $checks + " сверок прошли. Прежняя версия отдаёт одну метку на все четыре запроса, нынешняя отдаёт метки ближайших пар; на объёмном наборе обе версии держат 24 из 24; порча «дальний вместо ближайшего» поймана на всех четырёх строках.")
    exit 0
} else {
    Write-Output ("ИТОГ: сверок " + $checks + ", упало " + $failures + ". Это и есть результат: смотри строки «УПАЛО» выше.")
    exit 1
}
