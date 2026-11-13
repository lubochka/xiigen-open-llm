# Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
# Licensed under GNU Affero General Public License v3.0.
# SPDX-License-Identifier: AGPL-3.0-only
# See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
#
# Проверяемая программа кандидата сотого опыта, подтверждённого сто первым:
# при чтении ответа допускаются идентификаторы, не образующие целого символа.
#
# Здесь исполняется НАСТОЯЩИЙ продуктовый код, собранный в памяти процесса,
# шестью сборками в порядке зависимостей. Продуктовые файлы с диска НЕ
# правятся: правленый текст живёт только в памяти. Два файла продукта закрыты
# измеряющим опытом (`09.Inference/TextGenerator.cs`,
# `01.Tokenizer/BpeTokenizer.cs`); программа их только ЧИТАЕТ, поэтому их
# отпечатки не меняются.
#
# Оснастка логитов НЕ придумана: взяты точные измеренные значения из сырых
# данных сто первого опыта (`repair.json`, 20 строк по пяти снимкам весов).
# Прочие идентификаторы в векторе опущены ниже измеренных — это допустимо
# именно потому, что в настоящем векторе выбор продукта и выбор починки были
# ровно такими, какие здесь ожидаются.
#
# Панели:
#   1. Шесть сборок в порядке зависимостей, три версии проекта вывода.
#   2. Правило в двух формулировках на всех 256 идентификаторах байтового
#      словаря: согласие и число допущенных.
#   3. То же правило на ОБУЧЕННОМ словаре: формулировки расходятся, и это
#      решает, какая из них годится продукту.
#   4. Нынешний код на шести измеренных повреждённых строках: обломок байта.
#   5. Правленый код: выбор совпадает с измеренной починкой сто первого.
#   6. Четырнадцать законных строк: тот же идентификатор и тот же байт логита.
#   7. Жадное чтение: правка только в маске кандидатов его НЕ лечит.
#   8. Помещается ли починка внутрь одного выборщика — проверено компиляцией.
#   9. Чувствительность: порча правки обязана быть поймана.

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

# Сборки живут только в памяти, файлов на диске у них нет, поэтому разрешитель
# обязан отдавать их по имени. Поймано прогоном в предыдущей программе.
$script:LoadedByName = @{}
[System.AppDomain]::CurrentDomain.add_AssemblyResolve({
    param($sender, $eventArgs)
    $simpleName = ($eventArgs.Name -split ',')[0]
    if ($script:LoadedByName.ContainsKey($simpleName)) { return $script:LoadedByName[$simpleName] }
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

# Возвращает и диагностику, чтобы панель 8 могла измерить КОДЫ ошибок, а не
# только факт неудачи. При $expectFailure сборка не считается бедой.
function Build-Project(
    [string]$assemblyName,
    [string]$projectDirectory,
    [System.Collections.Generic.List[Microsoft.CodeAnalysis.MetadataReference]]$references,
    [hashtable]$replacements,
    [string]$extraSource,
    [bool]$expectFailure = $false) {

    $trees = [System.Collections.Generic.List[Microsoft.CodeAnalysis.SyntaxTree]]::new()
    foreach ($file in Get-ProjectSources $projectDirectory) {
        $text = [System.IO.File]::ReadAllText($file.FullName)
        # ПОРЯДОК ЗДЕСЬ СУЩЕСТВЕНЕН, и в прошлой программе первая редакция его
        # перепутала: подстановка версии идёт ПЕРВОЙ, по тексту файла как он
        # лежит на диске, потому что ключом служит содержимое файла.
        if ($null -ne $replacements) {
            foreach ($key in $replacements.Keys) {
                if (-not $text.Contains($key)) { continue }
                $text = $text.Replace($key, $replacements[$key])
            }
        }
        $text = $text -replace '\[assembly: *(System\.Runtime\.CompilerServices\.)?InternalsVisibleTo\("[^"]*"\)\]', ''
        $trees.Add([Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($text, $parseOptions))
    }
    $trees.Add([Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($implicitUsings, $parseOptions))
    if (-not [string]::IsNullOrEmpty($extraSource)) {
        $trees.Add([Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($extraSource, $parseOptions))
    }

    $options = [Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions]::new(
        [Microsoft.CodeAnalysis.OutputKind]::DynamicallyLinkedLibrary)
    $options = $options.WithNullableContextOptions([Microsoft.CodeAnalysis.NullableContextOptions]::Enable)
    $options = $options.WithAllowUnsafe($true)
    $compilation = [Microsoft.CodeAnalysis.CSharp.CSharpCompilation]::Create(
        $assemblyName, $trees, $references, $options)
    $stream = New-Object System.IO.MemoryStream
    $emit = $compilation.Emit($stream)
    $errors = @($emit.Diagnostics |
        Where-Object { $_.Severity -eq [Microsoft.CodeAnalysis.DiagnosticSeverity]::Error })
    if (-not $emit.Success) {
        if (-not $expectFailure) {
            [Console]::Out.WriteLine("      ошибки сборки (" + $assemblyName + "):")
            foreach ($d in @($errors | Select-Object -First 8)) {
                [Console]::Out.WriteLine('        ' + $d.ToString())
            }
            throw ('сборка в памяти не удалась: ' + $assemblyName)
        }
        return [ordered]@{ Bytes = $null; Assembly = $null; Errors = $errors; Success = $false }
    }
    $bytes = $stream.ToArray()
    $assembly = [System.Reflection.Assembly]::Load($bytes)
    $script:LoadedByName[$assembly.GetName().Name] = $assembly
    return [ordered]@{ Bytes = $bytes; Assembly = $assembly; Errors = $errors; Success = $true }
}

function New-ReferenceSet([object[]]$built) {
    $set = [System.Collections.Generic.List[Microsoft.CodeAnalysis.MetadataReference]]::new()
    foreach ($r in $frameworkReferences) { $set.Add($r) }
    foreach ($b in $built) {
        $set.Add([Microsoft.CodeAnalysis.MetadataReference]::CreateFromImage($b.Bytes))
    }
    return $set
}

Write-Output "Проверяемая программа: при чтении ответа не допускаются идентификаторы, не образующие целого символа."
Write-Output ("Разборщик: " + $sdk.Name + "; эталонные сборки: " + $refPack.Name)
Write-Output ""

# ── Панель 1. Шесть сборок, три версии проекта вывода ──────────────────────
Write-Output "Панель 1. Шесть сборок в порядке зависимостей, три версии проекта вывода."

$tensorCore      = Build-Project 'TensorCore'        '00.TensorCore'      (New-ReferenceSet @()) $null ''
Check "собрана тензорная основа" $true ($null -ne $tensorCore.Assembly)
$semanticCompiler = Build-Project 'SemanticCompiler' '29.SemanticCompiler' (New-ReferenceSet @()) $null ''
Check "собран семантический компилятор" $true ($null -ne $semanticCompiler.Assembly)
$core = Build-Project 'AgentInfrastructure' '00.Core' (New-ReferenceSet @($semanticCompiler)) $null ''
Check "собрана основа договоров" $true ($null -ne $core.Assembly)
$transformerModel = Build-Project 'TransformerModel' '03.TransformerModel' (New-ReferenceSet @($tensorCore, $core, $semanticCompiler)) $null ''
Check "собран преобразователь" $true ($null -ne $transformerModel.Assembly)
$tokenizer = Build-Project 'Tokenizer' '01.Tokenizer' (New-ReferenceSet @()) $null ''
Check "собран словарь кусочков слов" $true ($null -ne $tokenizer.Assembly)

# Проба живёт в ТОЙ ЖЕ сборке, что проект вывода: так внутренняя перегрузка
# выборщика доступна без правки продуктового файла и без дружественного имени.
$probeSource = @'
namespace Inference;

using TensorCore.Core;

public static class ByteFragmentProbe
{
    // Вектор логитов нужного размера: заданные идентификаторы получают точные
    // измеренные значения, остальные опускаются ниже любого из них.
    public static Tensor BuildLogits(int vocabSize, int[] ids, float[] values, float floorValue)
    {
        var data = new float[vocabSize];
        for (int i = 0; i < vocabSize; i++) data[i] = floorValue;
        for (int i = 0; i < ids.Length; i++) data[ids[i]] = values[i];
        // Форма ровно та, какую генератор отдаёт выборщику: вектор длиной в
        // размер словаря (`09.Inference/TextGenerator.cs`, извлечение последних
        // логитов). Одномерный вектор, а не пара «партия на длину».
        return new Tensor(data, new TensorShape(vocabSize));
    }

    // Нынешний продуктовый путь чтения ответа: температура ноль, то есть
    // жадная ветвь выборщика.
    public static int ChooseGreedy(Tensor logits)
        => Sampler.Sample(logits, new Random(20261102), 0f, 0, 1f);

    // Тот же путь со случайным выбором: ровно столько же обращений, фиксированное
    // зерно, чтобы доля обломка считалась воспроизводимо.
    public static int CountFragmentDraws(Tensor logits, int fragmentId, int draws, int topK, int seed)
    {
        var random = new Random(seed);
        int hits = 0;
        for (int i = 0; i < draws; i++)
            if (Sampler.Sample(logits, random, 1f, topK, 1f) == fragmentId) hits++;
        return hits;
    }

    // Байтовое представление логита выбранного идентификатора: сравнение
    // «тот же байт логита» делается без промежуточного округления.
    public static int LogitBits(Tensor logits, int id)
        => BitConverter.SingleToInt32Bits(logits.Data[id]);

    // Предикат допуска строится ОДИН раз по готовой таблице и дальше живёт
    // чистым замыканием. Иначе на каждый идентификатор каждого обращения
    // вызывался бы блок оболочки: и медленно, и вызов из скомпилированного кода
    // ненадёжен. Таблицу наполняет настоящий продуктовый словарь обратным ходом.
    public static Func<int, bool> PredicateFromTable(bool[] admissible)
        => id => id >= 0 && id < admissible.Length && admissible[id];
}
'@

# Правленая версия: допуск ответа проверяется ДО всякого отбора кандидатов,
# то есть и на жадной ветви тоже. Пятипараметрная перегрузка сохранена, потому
# что именно её связывает генератор, и она передаёт отсутствие ограничения —
# это и есть измеряемая причина, по которой правка одного выборщика продукт не
# лечит.
$currentSampleHead = @'
    internal static int Sample(Tensor logits, Random random, float temperature, int topK, float topP)
    {
        ArgumentNullException.ThrowIfNull(random);
'@
$fixedSampleHead = @'
    internal static int Sample(Tensor logits, Random random, float temperature, int topK, float topP)
        => Sample(logits, random, temperature, topK, topP, isAdmissibleAnswer: null);

    internal static int Sample(Tensor logits, Random random, float temperature, int topK, float topP,
        Func<int, bool>? isAdmissibleAnswer)
    {
        ArgumentNullException.ThrowIfNull(random);
        if (isAdmissibleAnswer is not null)
        {
            int admissibleCount = logits.Shape.TotalElements;
            var admissible = new float[admissibleCount];
            bool anyAdmissible = false;
            for (int i = 0; i < admissibleCount; i++)
            {
                if (isAdmissibleAnswer(i)) { admissible[i] = logits.Data[i]; anyAdmissible = true; }
                else { admissible[i] = float.NegativeInfinity; }
            }
            if (anyAdmissible) logits = new Tensor(admissible, logits.Shape);
        }
'@

# Третья версия: та же проверка допуска, но поставленная ТОЛЬКО в маску по числу
# кандидатов. Предикат приходит статическим полем — это устройство пробы, а не
# предложение продукту; оно нужно ровно чтобы измерить, лечит ли такая
# постановка жадное чтение.
$currentMaskHead = @'
    private static Tensor TopKMask(Tensor logits, int k)
    {
        int n = logits.Shape.TotalElements; // Сколько всего кандидатов в словаре.
'@
$maskOnlyHead = @'
    internal static Func<int, bool>? MaskOnlyProbeAdmissible;

    private static Tensor TopKMask(Tensor logits, int k)
    {
        int n = logits.Shape.TotalElements; // Сколько всего кандидатов в словаре.
        if (MaskOnlyProbeAdmissible is not null)
        {
            var allowed = new float[n];
            bool anyAllowed = false;
            for (int i = 0; i < n; i++)
            {
                if (MaskOnlyProbeAdmissible(i)) { allowed[i] = logits.Data[i]; anyAllowed = true; }
                else { allowed[i] = float.NegativeInfinity; }
            }
            if (anyAllowed) logits = new Tensor(allowed, logits.Shape);
        }
'@

$inferenceReferences = @($tensorCore, $tokenizer, $transformerModel, $core, $semanticCompiler)

$currentInference = Build-Project 'InferenceCurrent' '09.Inference' (New-ReferenceSet $inferenceReferences) $null $probeSource
Check "собран нынешний проект вывода" $true ($null -ne $currentInference.Assembly)

$fixedInference = Build-Project 'InferenceFixed' '09.Inference' (New-ReferenceSet $inferenceReferences) @{ $currentSampleHead = $fixedSampleHead } $probeSource
Check "собран правленый проект вывода" $true ($null -ne $fixedInference.Assembly)

$maskOnlyInference = Build-Project 'InferenceMaskOnly' '09.Inference' (New-ReferenceSet $inferenceReferences) @{ $currentMaskHead = $maskOnlyHead } $probeSource
Check "собран проект вывода с правкой только в маске кандидатов" $true ($null -ne $maskOnlyInference.Assembly)

$tokenizerType = $tokenizer.Assembly.GetType('Tokenizer.BpeTokenizer')
Check "тип словаря найден" $true ($null -ne $tokenizerType)
Write-Output ""

# ── Панель 2. Правило в двух формулировках на 256 идентификаторах ──────────
Write-Output "Панель 2. Правило в двух формулировках на всех 256 идентификаторах байтового словаря."
Write-Output "  Формулировка А: идентификатор меньше 0x80."
Write-Output "  Формулировка Б: обратный ход через продуктовый словарь — сбор разбора даёт тот же один идентификатор."

$byteVocabulary = [System.Activator]::CreateInstance($tokenizerType)
$tokenizerType.GetMethod('Train').Invoke($byteVocabulary, @('abc', 256)) | Out-Null
$vocabSize = $tokenizerType.GetProperty('VocabSize').GetValue($byteVocabulary)
Check "  размер байтового словаря" 256 $vocabSize

$decodeMethod = $tokenizerType.GetMethod('Decode')
$encodeMethod = $tokenizerType.GetMethod('Encode')

# Оболочка разворачивает список в отдельные элементы, когда он попадает в массив
# аргументов отражения, и вызов падает с «Int32 нельзя превратить в List». Поэтому
# массив аргументов строится запятой: @(,$list) — ровно один элемент-список.
# Массив аргументов отражения, когда среди них есть МАССИВ, собирается явно:
# через литерал @() оболочка либо разворачивает вложенный массив в элементы, либо
# оборачивает его в PSObject, и вызов падает — «Int32 нельзя превратить в List»
# и «PSObject нельзя превратить в Boolean[]». Поймано прогоном дважды.
# И ещё одна ловушка той же природы, поймана третьим прогоном: массив,
# возвращённый из функции, оболочка РАЗВОРАЧИВАЕТ на выходе, поэтому наружу
# уходил бы сам список вместо массива из одного списка. Запятая перед
# возвращаемым значением запечатывает его.
function New-SingleArgument($value) {
    $arguments = New-Object 'object[]' 1
    $arguments[0] = $value
    return ,$arguments
}

# Аргументы правленой перегрузки тоже собираются явно: `New-Object` оборачивает
# созданный генератор случайности в PSObject, и отражение падает с «PSObject
# нельзя превратить в Random». Температура ноль — это жадная ветвь, то есть
# ровно тот путь, которым читали оба опыта.
function New-SampleArguments($logits, [int]$seed, $predicate) {
    $arguments = New-Object 'object[]' 6
    $arguments[0] = $logits
    $arguments[1] = [System.Random]::new($seed)
    $arguments[2] = [single]0.0
    $arguments[3] = [int]0
    $arguments[4] = [single]1.0
    $arguments[5] = $predicate
    return ,$arguments
}

function Decode-One($vocabulary, [int]$id) {
    $list = [System.Collections.Generic.List[int]]::new()
    $list.Add($id)
    return $decodeMethod.Invoke($vocabulary, (New-SingleArgument $list))
}

function Test-RoundTrip($vocabulary, [int]$id) {
    $text = Decode-One $vocabulary $id
    $back = $encodeMethod.Invoke($vocabulary, @($text))
    return ($back.Count -eq 1) -and ($back[0] -eq $id)
}

$ruleAdmittedA = 0
$ruleAdmittedB = 0
$ruleDisagreements = 0
$disagreementExamples = [System.Collections.Generic.List[string]]::new()
for ($id = 0; $id -lt 256; $id++) {
    $a = ($id -lt 128)
    $b = Test-RoundTrip $byteVocabulary $id
    if ($a) { $ruleAdmittedA++ }
    if ($b) { $ruleAdmittedB++ }
    if ($a -ne $b) {
        $ruleDisagreements++
        if ($disagreementExamples.Count -lt 8) {
            $disagreementExamples.Add(("идентификатор {0}: А={1}, Б={2}" -f $id, $a, $b))
        }
    }
}
Write-Output ("  допущено формулировкой А: " + $ruleAdmittedA)
Write-Output ("  допущено формулировкой Б: " + $ruleAdmittedB)
Check "  расхождений формулировок на байтовом словаре" 0 $ruleDisagreements
Check "  допущенных идентификаторов" 128 $ruleAdmittedB
if ($disagreementExamples.Count -gt 0) {
    Write-Output "  расхождения, если они есть:"
    foreach ($e in $disagreementExamples) { Write-Output ("    " + $e) }
}
$fragmentAdmitted = Test-RoundTrip $byteVocabulary 182
Check "  обломок байта 182 допущен правилом" $false $fragmentAdmitted
Check "  знак «плюс» (43) допущен правилом" $true (Test-RoundTrip $byteVocabulary 43)
Check "  знак «минус» (45) допущен правилом" $true (Test-RoundTrip $byteVocabulary 45)
Check "  звёздочка (42) допущена правилом" $true (Test-RoundTrip $byteVocabulary 42)
Write-Output ""

# ── Панель 3. То же правило на ОБУЧЕННОМ словаре ───────────────────────────
Write-Output "Панель 3. То же правило на ОБУЧЕННОМ словаре: формулировки расходятся."
Write-Output "  Это решает, какая формулировка годится продукту, и мера здесь — нелатинские куски слов."

$trainedVocabulary = [System.Activator]::CreateInstance($tokenizerType)
$trainingCorpus = 'плюс минус плюс минус знак разности результата плюс минус ' +
                  'sign of difference result plus minus plus minus ' +
                  'плюс минус плюс минус знак знак знак разность разность'
$tokenizerType.GetMethod('Train').Invoke($trainedVocabulary, @($trainingCorpus, 400)) | Out-Null
$trainedSize = $tokenizerType.GetProperty('VocabSize').GetValue($trainedVocabulary)
Write-Output ("  размер обученного словаря: " + $trainedSize)

$trainedAdmittedA = 0
$trainedAdmittedB = 0
$trainedDisagreements = 0
$rejectedByRangeButWhole = 0
$examplesTrained = [System.Collections.Generic.List[string]]::new()
for ($id = 0; $id -lt $trainedSize; $id++) {
    $a = ($id -lt 128)
    $b = Test-RoundTrip $trainedVocabulary $id
    if ($a) { $trainedAdmittedA++ }
    if ($b) { $trainedAdmittedB++ }
    if ($a -ne $b) { $trainedDisagreements++ }
    if ((-not $a) -and $b) {
        $rejectedByRangeButWhole++
        if ($examplesTrained.Count -lt 6) {
            $text = Decode-One $trainedVocabulary $id
            $examplesTrained.Add(("идентификатор {0} = «{1}»" -f $id, $text))
        }
    }
}
Write-Output ("  допущено формулировкой А (диапазон): " + $trainedAdmittedA)
Write-Output ("  допущено формулировкой Б (обратный ход): " + $trainedAdmittedB)
Write-Output ("  расхождений формулировок: " + $trainedDisagreements)
Write-Output ("  целых символов, которые формулировка А отвергла бы: " + $rejectedByRangeButWhole)
Check "  на обученном словаре формулировки РАСХОДЯТСЯ" $true ($trainedDisagreements -gt 0)
Check "  формулировка А отвергает целые символы" $true ($rejectedByRangeButWhole -gt 0)
if ($examplesTrained.Count -gt 0) {
    Write-Output "  примеры целых кусков, которые диапазонное правило выбросило бы:"
    foreach ($e in $examplesTrained) { Write-Output ("    " + $e) }
}
Write-Output "  Вывод панели: продукту годится только обратный ход. Диапазонное правило"
Write-Output "  воспроизвело бы структурную недостижимость, измеренную сотым опытом:"
Write-Output "  из розыгрыша систематически выпадает всё, что не латиница."
Write-Output ""

# ── Оснастка измеренных строк сто первого опыта ────────────────────────────
# Точные значения из `repair.json`: идентификаторы 43 («плюс»), 45 («минус»),
# 182 (обломок байта), 42 («звёздочка»). Пол вектора ниже любого из них.
$measuredRows = @(
    [ordered]@{ Updates = 0;   Row = 'T1'; Plus = 5.654635;  Minus = 3.7933986; Fragment = 2.8904266; Star = $null;      ProductId = 43;  RepairId = 43 },
    [ordered]@{ Updates = 0;   Row = 'T2'; Plus = 4.5910234; Minus = 4.392597;  Fragment = 3.6110332; Star = $null;      ProductId = 43;  RepairId = 43 },
    [ordered]@{ Updates = 0;   Row = 'E1'; Plus = 3.6537466; Minus = 4.132603;  Fragment = 3.9909968; Star = $null;      ProductId = 45;  RepairId = 45 },
    [ordered]@{ Updates = 0;   Row = 'E2'; Plus = 3.8132536; Minus = 4.199436;  Fragment = 4.067336;  Star = $null;      ProductId = 45;  RepairId = 45 },
    [ordered]@{ Updates = 10;  Row = 'T1'; Plus = 5.6673927; Minus = 3.7805018; Fragment = 2.9227278; Star = $null;      ProductId = 43;  RepairId = 43 },
    [ordered]@{ Updates = 10;  Row = 'T2'; Plus = 4.5501223; Minus = 4.3783965; Fragment = 3.6431305; Star = $null;      ProductId = 43;  RepairId = 43 },
    [ordered]@{ Updates = 10;  Row = 'E1'; Plus = 3.607746;  Minus = 4.11095;   Fragment = 4.018319;  Star = $null;      ProductId = 45;  RepairId = 45 },
    [ordered]@{ Updates = 10;  Row = 'E2'; Plus = 3.767511;  Minus = 4.178944;  Fragment = 4.0906167; Star = $null;      ProductId = 45;  RepairId = 45 },
    [ordered]@{ Updates = 25;  Row = 'T1'; Plus = 5.6916633; Minus = 3.744548;  Fragment = 2.9926689; Star = $null;      ProductId = 43;  RepairId = 43 },
    [ordered]@{ Updates = 25;  Row = 'T2'; Plus = 4.451876;  Minus = 4.339107;  Fragment = 3.7093925; Star = $null;      ProductId = 43;  RepairId = 43 },
    [ordered]@{ Updates = 25;  Row = 'E1'; Plus = 3.500521;  Minus = 4.056986;  Fragment = 4.0735264; Star = $null;      ProductId = 182; RepairId = 45 },
    [ordered]@{ Updates = 25;  Row = 'E2'; Plus = 3.659883;  Minus = 4.1266756; Fragment = 4.1366158; Star = $null;      ProductId = 182; RepairId = 45 },
    [ordered]@{ Updates = 50;  Row = 'T1'; Plus = 5.7137275; Minus = 3.6665435; Fragment = 3.103953;  Star = $null;      ProductId = 43;  RepairId = 43 },
    [ordered]@{ Updates = 50;  Row = 'T2'; Plus = 4.265446;  Minus = 4.2554216; Fragment = 3.806877;  Star = $null;      ProductId = 43;  RepairId = 43 },
    [ordered]@{ Updates = 50;  Row = 'E1'; Plus = 3.3086722; Minus = 3.954266;  Fragment = 4.1517653; Star = $null;      ProductId = 182; RepairId = 45 },
    [ordered]@{ Updates = 50;  Row = 'E2'; Plus = 3.46388;   Minus = 4.0243306; Fragment = 4.1993217; Star = $null;      ProductId = 182; RepairId = 45 },
    [ordered]@{ Updates = 100; Row = 'T1'; Plus = 5.7025146; Minus = 3.455347;  Fragment = 3.250236;  Star = $null;      ProductId = 43;  RepairId = 43 },
    [ordered]@{ Updates = 100; Row = 'T2'; Plus = 3.8863244; Minus = 4.0511503; Fragment = 3.9215121; Star = $null;      ProductId = 45;  RepairId = 45 },
    [ordered]@{ Updates = 100; Row = 'E1'; Plus = 2.9433384; Minus = 3.736191;  Fragment = 4.240066;  Star = 3.8619545;  ProductId = 182; RepairId = 42 },
    [ordered]@{ Updates = 100; Row = 'E2'; Plus = 3.0801659; Minus = 3.7988818; Fragment = 4.2586083; Star = 3.8656182;  ProductId = 182; RepairId = 42 }
)

$currentProbe  = $currentInference.Assembly.GetType('Inference.ByteFragmentProbe')
$fixedProbe    = $fixedInference.Assembly.GetType('Inference.ByteFragmentProbe')
$maskOnlyProbe = $maskOnlyInference.Assembly.GetType('Inference.ByteFragmentProbe')
$maskOnlySampler = $maskOnlyInference.Assembly.GetType('Inference.Sampler')
$fixedSampler    = $fixedInference.Assembly.GetType('Inference.Sampler')

function Build-RowLogits($probeType, $row) {
    $ids = [System.Collections.Generic.List[int]]::new()
    $values = [System.Collections.Generic.List[single]]::new()
    $ids.Add(43);  $values.Add([single]$row.Plus)
    $ids.Add(45);  $values.Add([single]$row.Minus)
    $ids.Add(182); $values.Add([single]$row.Fragment)
    if ($null -ne $row.Star) { $ids.Add(42); $values.Add([single]$row.Star) }
    # Среди аргументов два массива, поэтому массив аргументов собирается явно.
    $arguments = New-Object 'object[]' 4
    $arguments[0] = 256
    $arguments[1] = [int[]]$ids.ToArray()
    $arguments[2] = [single[]]$values.ToArray()
    $arguments[3] = [single](-10.0)
    return $probeType.GetMethod('BuildLogits').Invoke($null, $arguments)
}

# Предикат допуска строится по НАСТОЯЩЕМУ продуктовому словарю обратным ходом:
# таблица наполняется один раз, дальше предикат — чистое замыкание в C#.
# Третья ловушка той же природы: `New-Object` отдаёт значение через конвейер, и
# оболочка оборачивает массив в PSObject — отражение падает с «PSObject нельзя
# превратить в Boolean[]». Поэтому массив создаётся прямым конструктором, а на
# входе отражения тип приводится явно.
$admissibleTable = [bool[]]::new(256)
for ($id = 0; $id -lt 256; $id++) {
    $admissibleTable[$id] = (Test-RoundTrip $byteVocabulary $id)
}
$admissiblePredicate = $fixedProbe.GetMethod('PredicateFromTable').Invoke($null, (New-SingleArgument ([bool[]]$admissibleTable)))
$admissiblePredicateMaskOnly = $maskOnlyProbe.GetMethod('PredicateFromTable').Invoke($null, (New-SingleArgument ([bool[]]$admissibleTable)))
$admittedByTable = @($admissibleTable | Where-Object { $_ }).Count
Check "таблица допуска: допущено идентификаторов" 128 $admittedByTable

# ── Панель 4. Нынешний код на измеренных повреждённых строках ──────────────
Write-Output "Панель 4. Нынешний продуктовый код на шести измеренных повреждённых строках."
$currentFragmentRows = 0
foreach ($row in $measuredRows) {
    if ($row.ProductId -ne 182) { continue }
    $logits = Build-RowLogits $currentProbe $row
    $chosen = $currentProbe.GetMethod('ChooseGreedy').Invoke($null, @($logits))
    if ($chosen -eq 182) { $currentFragmentRows++ }
    Check ("  обновлений {0}, строка {1}: нынешний выбор" -f $row.Updates, $row.Row) $row.ProductId $chosen
}
Check "  повреждённых строк, где нынешний код отдаёт обломок байта" 6 $currentFragmentRows
Write-Output ""

# ── Панель 5. Правленый код совпадает с измеренной починкой ────────────────
Write-Output "Панель 5. Правленый код: выбор совпадает с измеренной починкой сто первого опыта."
$fixedSampleWithFilter = $fixedSampler.GetMethods('NonPublic, Static') |
    Where-Object { $_.Name -eq 'Sample' -and $_.GetParameters().Count -eq 6 } | Select-Object -First 1
Check "  шестипараметрная перегрузка найдена" $true ($null -ne $fixedSampleWithFilter)
foreach ($row in $measuredRows) {
    if ($row.ProductId -ne 182) { continue }
    $logits = Build-RowLogits $fixedProbe $row
    $chosen = $fixedSampleWithFilter.Invoke($null, (New-SampleArguments $logits 20261102 $admissiblePredicate))
    Check ("  обновлений {0}, строка {1}: правленый выбор" -f $row.Updates, $row.Row) $row.RepairId $chosen
}
Write-Output "  Граница починки, измеренная заново: на сотой точке она отдаёт звёздочку (42),"
Write-Output "  законный символ и неверный ответ. Повреждение к сотому обновлению ушло глубже"
Write-Output "  обломка байта, и починка его НЕ лечит — ровно как записал сто первый опыт."
Write-Output ""

# ── Панель 6. Четырнадцать законных строк ──────────────────────────────────
Write-Output "Панель 6. Четырнадцать законных строк: тот же идентификатор и тот же байт логита."
$legitimateRows = 0
$legitimateIdMismatch = 0
$legitimateBitsMismatch = 0
foreach ($row in $measuredRows) {
    if ($row.ProductId -eq 182) { continue }
    $legitimateRows++
    $logitsCurrent = Build-RowLogits $currentProbe $row
    $logitsFixed   = Build-RowLogits $fixedProbe $row
    $chosenCurrent = $currentProbe.GetMethod('ChooseGreedy').Invoke($null, @($logitsCurrent))
    $chosenFixed = $fixedSampleWithFilter.Invoke($null, (New-SampleArguments $logitsFixed 20261102 $admissiblePredicate))
    if ($chosenCurrent -ne $chosenFixed) { $legitimateIdMismatch++ }
    $bitsCurrent = $currentProbe.GetMethod('LogitBits').Invoke($null, @($logitsCurrent, $chosenCurrent))
    $bitsFixed   = $fixedProbe.GetMethod('LogitBits').Invoke($null, @($logitsFixed, $chosenFixed))
    if ($bitsCurrent -ne $bitsFixed) { $legitimateBitsMismatch++ }
}
Check "  законных строк" 14 $legitimateRows
Check "  расхождений по идентификатору" 0 $legitimateIdMismatch
Check "  расхождений по байту логита" 0 $legitimateBitsMismatch
Write-Output ""

# ── Панель 7. Жадное чтение и правка только в маске кандидатов ─────────────
Write-Output "Панель 7. Жадное чтение: правка только в маске кандидатов его НЕ лечит."
Write-Output "  Причина в самом коде: при температуре ноль выборщик возвращает argmax"
Write-Output "  ДО отбора кандидатов, поэтому маска по числу кандидатов не достигается вовсе."
$maskOnlyField = $maskOnlySampler.GetField('MaskOnlyProbeAdmissible', 'NonPublic, Static')
Check "  поле предиката пробы найдено" $true ($null -ne $maskOnlyField)
$maskOnlyField.SetValue($null, $admissiblePredicateMaskOnly)
$maskOnlyGreedyFragmentRows = 0
foreach ($row in $measuredRows) {
    if ($row.ProductId -ne 182) { continue }
    $logits = Build-RowLogits $maskOnlyProbe $row
    $chosen = $maskOnlyProbe.GetMethod('ChooseGreedy').Invoke($null, @($logits))
    if ($chosen -eq 182) { $maskOnlyGreedyFragmentRows++ }
}
Check "  повреждённых строк, где правка в маске жадное чтение не вылечила" 6 $maskOnlyGreedyFragmentRows

# А при случайном выборе та же правка в маске работает — значит разница именно в
# том, какая ветвь выборщика исполняется, а не в самой правке.
$row100 = $measuredRows | Where-Object { $_.Updates -eq 100 -and $_.Row -eq 'E2' } | Select-Object -First 1
$logitsCurrent100 = Build-RowLogits $currentProbe $row100
$logitsMaskOnly100 = Build-RowLogits $maskOnlyProbe $row100
$drawsCurrent = $currentProbe.GetMethod('CountFragmentDraws').Invoke($null, @($logitsCurrent100, 182, 1000, 2, 20261102))
$drawsMaskOnly = $maskOnlyProbe.GetMethod('CountFragmentDraws').Invoke($null, @($logitsMaskOnly100, 182, 1000, 2, 20261102))
Write-Output ("  при температуре 1 и двух кандидатах нынешний код отдаёт обломок в " + $drawsCurrent + " из 1000 розыгрышей")
Check "  та же правка в маске при случайном выборе обломок не допускает" 0 $drawsMaskOnly
Write-Output "  Отсюда прямое следствие: место правки — точка выбора ответа, а не маска"
Write-Output "  кандидатов. Шесть измеренных повреждённых строк были прочитаны ЖАДНО."
Write-Output ""

# ── Панель 8. Помещается ли починка внутрь одного выборщика ────────────────
Write-Output "Панель 8. Помещается ли починка внутрь одного выборщика — проверено компиляцией."
Write-Output "  Знание о том, какие идентификаторы образуют целый символ, живёт в словаре."
Write-Output "  Выборщик словаря не видит вовсе: его вход — только тензор логитов."

# Проба А: шестой параметр ОБЯЗАТЕЛЬНЫЙ, генератор не правлен.
$requiredParameterHead = @'
    internal static int Sample(Tensor logits, Random random, float temperature, int topK, float topP,
        Func<int, bool> isAdmissibleAnswer)
    {
        ArgumentNullException.ThrowIfNull(random);
        ArgumentNullException.ThrowIfNull(isAdmissibleAnswer);
'@
$probeRequired = Build-Project 'InferenceProbeRequired' '09.Inference' (New-ReferenceSet $inferenceReferences) @{ $currentSampleHead = $requiredParameterHead } '' $true
Check "  проба А (обязательный шестой параметр): сборка" $false $probeRequired.Success
$codesRequired = @($probeRequired.Errors | ForEach-Object { $_.Id } | Sort-Object -Unique) -join ','
Write-Output ("    коды ошибок: " + $codesRequired)
Write-Output ("    ошибок всего: " + $probeRequired.Errors.Count)
foreach ($d in @($probeRequired.Errors | Select-Object -First 3)) {
    Write-Output ("    " + $d.ToString())
}

# Проба Б: шестой параметр НЕОБЯЗАТЕЛЬНЫЙ, генератор не правлен.
$optionalParameterHead = @'
    internal static int Sample(Tensor logits, Random random, float temperature, int topK, float topP,
        Func<int, bool>? isAdmissibleAnswer = null)
    {
        ArgumentNullException.ThrowIfNull(random);
'@
$probeOptional = Build-Project 'InferenceProbeOptional' '09.Inference' (New-ReferenceSet $inferenceReferences) @{ $currentSampleHead = $optionalParameterHead } '' $true
Check "  проба Б (необязательный шестой параметр): сборка" $false $probeOptional.Success
$codesOptional = @($probeOptional.Errors | ForEach-Object { $_.Id } | Sort-Object -Unique) -join ','
Write-Output ("    коды ошибок: " + $codesOptional)
foreach ($d in @($probeOptional.Errors | Select-Object -First 3)) {
    Write-Output ("    " + $d.ToString())
}
Write-Output "    Смысл: необязательный параметр не спасает — преобразование группы методов"
Write-Output "    в делегат генератора требует точного совпадения списка параметров."

# Проба В: отдельная перегрузка; собирается, но продуктовый путь не связан.
Write-Output "  проба В (отдельная перегрузка рядом с прежней): собирается, это и есть"
Write-Output "  правленая версия панели 5. Но генератор связывает ПЯТИпараметрную, и она"
Write-Output "  передаёт отсутствие ограничения."
$unwiredFragmentRows = 0
foreach ($row in $measuredRows) {
    if ($row.ProductId -ne 182) { continue }
    $logits = Build-RowLogits $fixedProbe $row
    $chosen = $fixedProbe.GetMethod('ChooseGreedy').Invoke($null, @($logits))
    if ($chosen -eq 182) { $unwiredFragmentRows++ }
}
Check "  продуктовый путь правленой версии БЕЗ связывания: обломок остаётся" 6 $unwiredFragmentRows
Write-Output "  Вывод панели: починка внутри одного выборщика либо не собирается, либо"
Write-Output "  собирается и ничего не лечит. Чтобы её связать, нужен тип делегата,"
Write-Output "  конструктор и место вызова — все три в генераторе."
Write-Output ""

# ── Панель 9. Чувствительность: порча правки ───────────────────────────────
Write-Output "Панель 9. Чувствительность: порча правки обязана быть поймана."
# Порча: допуск считается диапазоном на ОБУЧЕННОМ словаре — то самое правило,
# которое панель 3 забраковала. Здесь оно проверяется на деле.
$brokenWholeCharacterRejected = 0
for ($id = 0; $id -lt $trainedSize; $id++) {
    if ((Test-RoundTrip $trainedVocabulary $id) -and ($id -ge 128)) {
        $brokenWholeCharacterRejected++
    }
}
Check "  диапазонная порча отвергает целые символы обученного словаря" $true ($brokenWholeCharacterRejected -gt 0)
Write-Output ("    отвергнуто целых символов: " + $brokenWholeCharacterRejected)

# Вторая порча: предикат, допускающий всё. Он обязан вернуть прежнее поведение.
$allowEverythingTable = [bool[]]::new(256)
for ($id = 0; $id -lt 256; $id++) { $allowEverythingTable[$id] = $true }
$allowEverything = $fixedProbe.GetMethod('PredicateFromTable').Invoke($null, (New-SingleArgument ([bool[]]$allowEverythingTable)))
$brokenFragmentRows = 0
foreach ($row in $measuredRows) {
    if ($row.ProductId -ne 182) { continue }
    $logits = Build-RowLogits $fixedProbe $row
    $chosen = $fixedSampleWithFilter.Invoke($null, (New-SampleArguments $logits 20261102 $allowEverything))
    if ($chosen -eq 182) { $brokenFragmentRows++ }
}
Check "  порча «допускать всё» возвращает прежнее поведение" 6 $brokenFragmentRows

# Третья сверка, вторым способом на эталоне с заведомо известным ответом:
# на строке, где обломка нет вовсе, правленый и нынешний код обязаны совпасть.
$neutralRow = [ordered]@{ Updates = -1; Row = 'эталон'; Plus = 9.0; Minus = 1.0; Fragment = -5.0; Star = $null; ProductId = 43; RepairId = 43 }
$neutralCurrent = $currentProbe.GetMethod('ChooseGreedy').Invoke($null, @((Build-RowLogits $currentProbe $neutralRow)))
$neutralLogitsFixed = Build-RowLogits $fixedProbe $neutralRow
$neutralFixed = $fixedSampleWithFilter.Invoke($null, (New-SampleArguments $neutralLogitsFixed 20261102 $admissiblePredicate))
Check "  эталон без обломка: нынешний выбор" 43 $neutralCurrent
Check "  эталон без обломка: правленый выбор совпал" $neutralCurrent $neutralFixed
Write-Output ""

# ── Итог ───────────────────────────────────────────────────────────────────
Write-Output "Чего эта программа НЕ доказывает:"
Write-Output "  Она не измеряет частоту обломка в работающем обученном продукте: вектор"
Write-Output "  логитов подставлен точными измеренными значениями четырёх идентификаторов,"
Write-Output "  остальные опущены. Модель здесь не обучается вовсе: обновлений весов 0,"
Write-Output "  сборок проекта 0, прогонов площадки проверок 0."
Write-Output "  Она не доказывает, что починка даёт верный ответ: на сотой точке она"
Write-Output "  отдаёт законную звёздочку вместо верного «минуса»."
Write-Output "  Родная приёмка платформы НЕ исполнялась: NOT_EXECUTED, а не PASS — лане"
Write-Output "  сборки проекта и прогона проверок закрыта правами сессии."
Write-Output "  Правка продуктовых файлов НЕ делалась: два файла закрыты измеряющим"
Write-Output "  опытом и только читались; правленый текст выборщика жил лишь в памяти."
Write-Output ""

if ($failures -eq 0) {
    Write-Output ("ИТОГ: все " + $checks + " сверок прошли. Нынешний код отдаёт обломок байта на шести измеренных строках; правка в точке выбора ответа повторяет измеренную починку и не меняет ни одной законной строки; правка только в маске кандидатов жадное чтение не лечит; починка внутри одного выборщика либо не собирается, либо не связана.")
    exit 0
} else {
    Write-Output ("ИТОГ: сверок " + $checks + ", упало " + $failures + ". Это и есть результат: смотри строки «УПАЛО» выше.")
    exit 1
}
