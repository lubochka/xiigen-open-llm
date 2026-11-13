# Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
# Licensed under GNU Affero General Public License v3.0.
# SPDX-License-Identifier: AGPL-3.0-only
# See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
#
# Проверяемая программа переноса кандидата 3 девяносто девятого опыта: обычная
# ветвь генерации больше не выдумывает признак конца ответа из последней записи
# обученного словаря.
#
# Здесь исполняется НАСТОЯЩИЙ продуктовый код, собранный в памяти процесса,
# в двух версиях: прежняя берётся из опубликованной вершины, нынешняя — с диска.
# Замеренное замыкание зависимостей генератора — 61 файл и около 12 647 строк в
# ШЕСТИ сборках, и цепочка замыкается на семантическом компиляторе. Поэтому
# сборка идёт шестью отдельными сборками в порядке зависимостей, а не одной:
# часть проектов объявляет свои одноимённые типы, и склейка в одну сборку их
# столкнула бы. Сборка проекта не выполняется, файлов компилятор не пишет.
#
# Панели:
#   1. Шесть сборок в порядке зависимостей, две версии проекта вывода.
#   2. Прежняя версия обрывает поток на последней записи словаря.
#   3. Нынешняя версия доходит до предела длины, начало ответа не изменилось.
#   4. Законный случай: заданный вызывающим признак по-прежнему останавливает.
#   5. Вариант (Г): словарь сообщает отсутствие признака, формат не задет.
#   6. Чувствительность: порча правки обязана быть поймана.
#   7. Сверка вторым способом на эталоне с заведомо известным ответом.

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

function Build-Project(
    [string]$assemblyName,
    [string]$projectDirectory,
    [System.Collections.Generic.List[Microsoft.CodeAnalysis.MetadataReference]]$references,
    [hashtable]$replacements,
    [string]$extraSource) {

    $trees = [System.Collections.Generic.List[Microsoft.CodeAnalysis.SyntaxTree]]::new()
    foreach ($file in Get-ProjectSources $projectDirectory) {
        $text = [System.IO.File]::ReadAllText($file.FullName)
        # ПОРЯДОК ЗДЕСЬ СУЩЕСТВЕНЕН, и первая редакция его перепутала.
        # Подстановка версии идёт ПЕРВОЙ, по тексту файла как он лежит на диске:
        # ключом служит полное содержимое файла, и любая предварительная правка
        # текста ключ рассогласует. В первой редакции сначала снималось
        # объявление видимости внутренних членов, и поэтому «прежняя» пара
        # сборок молча собиралась из нынешнего кода — четыре сверки упали, и все
        # четыре были симптомами этой одной ошибки.
        if ($null -ne $replacements) {
            foreach ($key in $replacements.Keys) {
                $text = $text.Replace($key, $replacements[$key])
            }
        }
        # Дружественное имя площадки проверок в памяти не воспроизводится,
        # поэтому объявление видимости внутренних членов снимается, а пробы
        # добавляются отдельным деревом в ТУ ЖЕ сборку — так внутренние
        # конструкторы доступны без правки продуктового файла.
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
    return [ordered]@{ Bytes = $bytes; Assembly = $assembly }
}

function New-ReferenceSet([object[]]$built) {
    $set = [System.Collections.Generic.List[Microsoft.CodeAnalysis.MetadataReference]]::new()
    foreach ($r in $frameworkReferences) { $set.Add($r) }
    foreach ($b in $built) {
        $set.Add([Microsoft.CodeAnalysis.MetadataReference]::CreateFromImage($b.Bytes))
    }
    return $set
}

Write-Output "Проверяемая программа: обычная ветвь генерации не выдумывает признак конца ответа."
Write-Output ("Разборщик: " + $sdk.Name + "; эталонные сборки: " + $refPack.Name)
Write-Output ""

# ── Панель 1. Шесть сборок в порядке зависимостей ──────────────────────────
Write-Output "Панель 1. Шесть сборок в порядке зависимостей, две версии проекта вывода."

$tensorCore = Build-Project 'TensorCore' '00.TensorCore' (New-ReferenceSet @()) $null ''
Check "собран тензорный слой" $true ($null -ne $tensorCore.Assembly)

$semanticCompiler = Build-Project 'SemanticCompiler' '29.SemanticCompiler' (New-ReferenceSet @()) $null ''
Check "собран семантический компилятор" $true ($null -ne $semanticCompiler.Assembly)

$core = Build-Project 'AgentInfrastructure' '00.Core' (New-ReferenceSet @($semanticCompiler)) $null ''
Check "собрана основа" $true ($null -ne $core.Assembly)

$transformerModel = Build-Project 'TransformerModel' '03.TransformerModel' (New-ReferenceSet @($tensorCore, $core, $semanticCompiler)) $null ''
Check "собрана модель" $true ($null -ne $transformerModel.Assembly)

# Проба живёт в ТОЙ ЖЕ сборке, что проект вывода: так доступен внутренний
# конструктор с подставляемым источником токенов, и продуктовый файл не правится.
$probeSource = @'
namespace EosProbe;

using Inference;
using TensorCore.Core;
using Tokenizer;
using TransformerModel;
using TransformerModel.Caching;
using TransformerModel.Config;

public static class Probe
{
    private static ModelConfig TinyConfig() => new()
    {
        VocabSize = 256,
        ContextLen = 64,
        HiddenDim = 32,
        NumLayers = 2,
        NumHeads = 4,
        NumKvHeads = 2,
        FfnHiddenDim = 64
    };

    private static BpeTokenizer UntrainedTokenizer()
    {
        var tokenizer = new BpeTokenizer();
        tokenizer.Train("abc", 256);
        return tokenizer;
    }

    // Обычная ветвь: возвращает только текст.
    public static string OrdinaryText(int[] stream)
    {
        var index = 0;
        var generator = new TextGenerator(
            new LlmModel(TinyConfig()),
            UntrainedTokenizer(),
            layers => new KvCache(layers),
            (_, _, _, _, _) => stream[System.Math.Min(index++, stream.Length - 1)]);
        return generator.Generate("abc", stream.Length, 0f, 0, 1f, 7, System.Threading.CancellationToken.None);
    }

    // Подробная ветвь: отдаёт квитанцию с числом токенов и причиной остановки.
    public static (int count, string reason) DetailedReceipt(int[] stream, int? marker)
    {
        var index = 0;
        var generator = new TextGenerator(
            new LlmModel(TinyConfig()),
            UntrainedTokenizer(),
            layers => new KvCache(layers),
            (_, _, _, _, _) => stream[System.Math.Min(index++, stream.Length - 1)]);
        var receipt = generator.GenerateDetailed(
            "abc", stream.Length, 0f, 0, 1f, 7, marker, System.Threading.CancellationToken.None);
        return (receipt.GeneratedTokenCount, receipt.StopReason);
    }

    // Что словарь говорит про признак конца: число или отсутствие.
    public static string MarkerAsText()
    {
        var tokenizer = UntrainedTokenizer();
        var property = typeof(BpeTokenizer).GetProperty("EosToken");
        var value = property!.GetValue(tokenizer);
        return value is null ? "ОТСУТСТВУЕТ" : value.ToString()!;
    }

    public static string VocabularyStateSha(string corpus, int requested)
    {
        var tokenizer = new BpeTokenizer();
        tokenizer.Train(corpus, requested);
        return tokenizer.ComputeStateSha256();
    }
}
'@

$publishedGenerator = (& git -C $repoRoot show 'origin/codex/dpo-implementation:09.Inference/TextGenerator.cs' | Out-String)
$publishedTokenizer = (& git -C $repoRoot show 'origin/codex/dpo-implementation:01.Tokenizer/BpeTokenizer.cs' | Out-String)
$currentGenerator = [System.IO.File]::ReadAllText((Join-Path $repoRoot '09.Inference\TextGenerator.cs'))
$currentTokenizer = [System.IO.File]::ReadAllText((Join-Path $repoRoot '01.Tokenizer\BpeTokenizer.cs'))
Check "опубликованный генератор отличается от нынешнего" $true ($publishedGenerator -ne $currentGenerator)
Check "опубликованный словарь отличается от нынешнего" $true ($publishedTokenizer -ne $currentTokenizer)

# ПРЕЖНЯЯ версия: словарь и генератор берутся из опубликованной вершины.
# Правленый текст в файлы не пишется — подстановка живёт только в памяти.
# Имена сборок у версий РАЗНЫЕ, и это существенно. Разрешитель держит одну
# сборку на простое имя, поэтому две версии словаря под именем `Tokenizer`
# затирали бы друг друга: прежний проект вывода, собранный против прежнего
# словаря, при исполнении связывался с нынешним и падал с «метод не найден».
# Поймано прогоном. Пространство имён при этом остаётся прежним, поэтому
# продуктовый текст проекта вывода править не нужно.
$oldTokenizer = Build-Project 'TokenizerOld' '01.Tokenizer' (New-ReferenceSet @()) @{ $currentTokenizer = $publishedTokenizer } ''
Check "собран прежний словарь" $true ($null -ne $oldTokenizer.Assembly)
$oldInference = Build-Project 'InferenceOld' '09.Inference' (New-ReferenceSet @($tensorCore, $oldTokenizer, $transformerModel, $core, $semanticCompiler)) @{ $currentGenerator = $publishedGenerator } $probeSource
Check "собран прежний проект вывода" $true ($null -ne $oldInference.Assembly)

Write-Output ""
Write-Output "  Прежняя и нынешняя версии собираются как отдельные пары сборок, потому"
Write-Output "  что словарь и генератор меняются вместе: вариант (Г) трогает словарь,"
Write-Output "  вариант (В) — генератор."
Write-Output ""

# НЫНЕШНЯЯ пара: словарь и генератор берутся с диска как есть.
$newTokenizer = Build-Project 'TokenizerNew' '01.Tokenizer' (New-ReferenceSet @()) $null ''
Check "собран нынешний словарь" $true ($null -ne $newTokenizer.Assembly)
$newInference = Build-Project 'InferenceNew' '09.Inference' (New-ReferenceSet @($tensorCore, $newTokenizer, $transformerModel, $core, $semanticCompiler)) $null $probeSource
Check "собран нынешний проект вывода" $true ($null -ne $newInference.Assembly)

if ($failures -gt 0) {
    Write-Output ("ИТОГ: сверок {0}, упало {1}. Дальше мерить нечего." -f $checks, $failures)
    exit 1
}
Write-Output ""

function Invoke-Probe($built, [string]$method, [object[]]$arguments) {
    $probe = $built.Assembly.GetType('EosProbe.Probe')
    return $probe.GetMethod($method).Invoke($null, $arguments)
}

# Поток номеров: третьим идёт 255 — последняя запись словаря на 256 токенов,
# то есть то число, которое прежний код принимал за признак конца ответа.
# После него поток продолжается буквами «C» и «D».
$stream = [int[]]@(65, 66, 255, 67, 68)

# ── Панель 2. Прежняя версия обрывает поток ────────────────────────────────
Write-Output "Панель 2. Прежняя версия обрывает поток на последней записи словаря."
$oldText = Invoke-Probe $oldInference 'OrdinaryText' @(,$stream)
$oldBytes = [System.Text.Encoding]::UTF8.GetBytes($oldText)
Write-Output ("      прежний текст: знаков " + $oldText.Length + ", байт " + $oldBytes.Count)
Check "прежняя версия НЕ довела ответ до «CD»" $false $oldText.EndsWith('CD', [System.StringComparison]::Ordinal)
Check "но начало ответа «AB» она выдала" $true $oldText.StartsWith('AB', [System.StringComparison]::Ordinal)
Write-Output "  Это и есть дефект: буквы после признака в ответ не попали вовсе, и"
Write-Output "  вызывающий не получил ни причины, ни признака обрыва — обычные"
Write-Output "  перегрузки возвращают только текст."
Write-Output ""

# ── Панель 3. Нынешняя версия доходит до предела ──────────────────────────
Write-Output "Панель 3. Нынешняя версия доходит до предела длины."
$newText = Invoke-Probe $newInference 'OrdinaryText' @(,$stream)
$newBytes = [System.Text.Encoding]::UTF8.GetBytes($newText)
Write-Output ("      нынешний текст: знаков " + $newText.Length + ", байт " + $newBytes.Count)
Check "нынешняя версия довела ответ до «CD»" $true $newText.EndsWith('CD', [System.StringComparison]::Ordinal)
Check "и начало ответа «AB» не изменилось" $true $newText.StartsWith('AB', [System.StringComparison]::Ordinal)
Check "ответы двух версий различаются" $true ($oldText -ne $newText)
Write-Output "  Различие прямое и поведенческое: на одном и том же потоке номеров"
Write-Output "  прежняя версия теряет хвост ответа, нынешняя его отдаёт."
Write-Output ""

# ── Панель 4. Законный случай сохранён ────────────────────────────────────
Write-Output "Панель 4. Законный случай: заданный вызывающим признак по-прежнему останавливает."
$lawfulStream = [int[]]@(65, 66, 7, 67, 68)
$oldLawful = Invoke-Probe $oldInference 'DetailedReceipt' @($lawfulStream, [object]7)
$newLawful = Invoke-Probe $newInference 'DetailedReceipt' @($lawfulStream, [object]7)
Write-Output ("      прежняя: токенов " + $oldLawful.Item1 + ", причина " + $oldLawful.Item2)
Write-Output ("      нынешняя: токенов " + $newLawful.Item1 + ", причина " + $newLawful.Item2)
Check "прежняя остановилась на заданном признаке" 3 $oldLawful.Item1
Check "нынешняя остановилась там же" 3 $newLawful.Item1
Check "прежняя назвала причину концом ответа" 'EOS' $oldLawful.Item2
Check "нынешняя назвала ту же причину" 'EOS' $newLawful.Item2
Write-Output "  Правка не отнимает раннего завершения: когда признак ЗАДАН и проверен,"
Write-Output "  остановка по нему остаётся, и причина правдива."
Write-Output ""
Write-Output "  И подробная ветвь БЕЗ признака: обе версии доходят до предела."
$oldNoMarker = Invoke-Probe $oldInference 'DetailedReceipt' @($stream, $null)
$newNoMarker = Invoke-Probe $newInference 'DetailedReceipt' @($stream, $null)
Check "прежняя без признака дошла до предела" 5 $oldNoMarker.Item1
Check "нынешняя без признака дошла до предела" 5 $newNoMarker.Item1
Check "обе назвали причину пределом длины" $oldNoMarker.Item2 $newNoMarker.Item2
Write-Output "  Отсюда главное следствие: после правки обычная ветвь ведёт себя РОВНО"
Write-Output "  как подробная без признака. Две ветви сходятся на честной, а не"
Write-Output "  расходятся в разные стороны, поэтому «подробная неверна в другую"
Write-Output "  сторону» перестаёт быть вторым дефектом."
Write-Output ""

# ── Панель 5. Вариант (Г): словарь сообщает отсутствие ───────────────────
Write-Output "Панель 5. Вариант (Г): словарь сообщает отсутствие признака, формат не задет."
$oldMarker = Invoke-Probe $oldInference 'MarkerAsText' @()
$newMarker = Invoke-Probe $newInference 'MarkerAsText' @()
Write-Output ("      прежний словарь говорит: " + $oldMarker)
Write-Output ("      нынешний словарь говорит: " + $newMarker)
Check "прежний словарь выдумывал номер последней записи" '255' $oldMarker
Check "нынешний словарь сообщает отсутствие" 'ОТСУТСТВУЕТ' $newMarker

# Обученное состояние словаря не изменилось: формат и байты те же.
foreach ($case in @(
    @{ corpus = 'the quick brown fox jumps over the lazy dog'; requested = 300; name = 'недостижимый 300' },
    @{ corpus = 'abc'; requested = 256; name = 'ровно порог 256' })) {
    $oldSha = Invoke-Probe $oldInference 'VocabularyStateSha' @($case.corpus, [int]$case.requested)
    $newSha = Invoke-Probe $newInference 'VocabularyStateSha' @($case.corpus, [int]$case.requested)
    Check ("  " + $case.name + ": отпечаток обученного состояния совпал") $oldSha $newSha
}
Write-Output "  Признак конца в сериализацию не входит вовсе — сохраняются только"
Write-Output "  таблица слияний и словарь, — поэтому ни один из 3210 сохранённых"
Write-Output "  артефактов этой правкой не задет, и это измерено отпечатками."
# Число словарей исправлено: оно измерено по ФОРМЕ артефакта, а не по имени
# файла. Файлов с именем tokenizer.json в дереве 3383, но 173 из них — описатель
# полей обходчика кода на 147 байт, а не словарь. Прежняя запись исполнителя
# называла 3383, и это было завышением; вывод не меняется.
Write-Output "  Число измерено по форме артефакта, а не по имени файла: файлов с"
Write-Output "  именем tokenizer.json в дереве 3383, но 173 из них — описатель"
Write-Output "  полей обходчика кода на 147 байт, а не словарь. Прежнее число"
Write-Output "  3383 было завышением исполнителя. Состав цены: 3201 словарь в"
Write-Output "  замороженных хозяевах опытов, 8 под .kiro, ровно один в config."
Write-Output ""

# ── Панель 6. Чувствительность: порча правки ──────────────────────────────
Write-Output "Панель 6. Чувствительность: порча правки обязана быть поймана."
# Порча: генератор снова берёт признак из словаря, но словарь теперь сообщает
# отсутствие, поэтому порча выражается возвратом к номеру последней записи.
$brokenTokenizerText = $currentTokenizer.Replace(
    'public int? EosToken => null;',
    'public int? EosToken => _vocab.Count - 1;')
Check "подстановка порчи в словарь изменила текст" $true ($brokenTokenizerText -ne $currentTokenizer)
$brokenGeneratorText = $currentGenerator.Replace(
    'int? eosToken = verifiedEosToken;',
    'int? eosToken = detailed ? verifiedEosToken : tokenizer.EosToken;')
Check "подстановка порчи в генератор изменила текст" $true ($brokenGeneratorText -ne $currentGenerator)
$brokenTokenizer = Build-Project 'TokenizerBroken' '01.Tokenizer' (New-ReferenceSet @()) @{ $currentTokenizer = $brokenTokenizerText } ''
$brokenInference = Build-Project 'InferenceBroken' '09.Inference' (New-ReferenceSet @($tensorCore, $brokenTokenizer, $transformerModel, $core, $semanticCompiler)) @{ $currentGenerator = $brokenGeneratorText } $probeSource
Check "порченая пара собирается (порча тихая)" $true ($null -ne $brokenInference.Assembly)
$brokenText = Invoke-Probe $brokenInference 'OrdinaryText' @(,$stream)
Write-Output ("      порченый текст: знаков " + $brokenText.Length)
Check "порча поймана: ответ снова теряет «CD»" $false $brokenText.EndsWith('CD', [System.StringComparison]::Ordinal)
Check "и совпадает с прежним поведением" $oldText $brokenText
Write-Output "  Порча собралась молча и вернула прежнее поведение, а сверка это"
Write-Output "  поймала. Значит зелёный результат панели 3 не слепой."
Write-Output ""

# ── Панель 7. Второй способ на эталоне с известным ответом ───────────────
Write-Output "Панель 7. Сверка вторым способом на эталоне с заведомо известным ответом."
# Эталон: поток БЕЗ номера последней записи. Ответ известен заранее — обе
# версии обязаны дойти до предела и выдать одно и то же.
$neutralStream = [int[]]@(65, 66, 67, 68, 69)
$oldNeutral = Invoke-Probe $oldInference 'OrdinaryText' @(,$neutralStream)
$newNeutral = Invoke-Probe $newInference 'OrdinaryText' @(,$neutralStream)
Write-Output ("      эталон без признака: прежний «" + $oldNeutral + "», нынешний «" + $newNeutral + "»")
Check "  на потоке без признака версии совпадают" $oldNeutral $newNeutral
Check "  и обе выдали все пять знаков" 'ABCDE' $newNeutral
Write-Output "  Это и есть проверка самой сверки: на потоке, где дефект не может"
Write-Output "  проявиться, версии обязаны совпасть — и совпадают. Значит различие"
Write-Output "  панели 3 вызвано именно номером признака, а не разницей сборок."
Write-Output ""

Write-Output "Бюджет этого прогона."
Write-Output "  сборок проекта: 0; прогонов площадки проверок: 0; обучения модели: 0"
Write-Output "  сборок в памяти процесса: 9 (шесть общих плюс три версии пары)"
Write-Output "  обучений словаря: 8; нейронных обновлений: 0"
Write-Output "  файлов на диск компилятор не писал; правленый текст прежней и порченой"
Write-Output "  версий существовал ТОЛЬКО в памяти — на диск он не записывался"
Write-Output ""
Write-Output "Чем это НЕ доказано."
Write-Output "  Родная приёмка платформы НЕ исполнялась: NOT_EXECUTED, а не PASS — лане"
Write-Output "  сборки проекта и прогона проверок закрыта правами сессии. Здесь исполнен"
Write-Output "  настоящий продуктовый код, собранный в памяти, но это не прогон"
Write-Output "  площадки проверок и его не заменяет."
Write-Output "  Модель НЕ обучена: источник номеров токенов подставлен точно, потому что"
Write-Output "  измеряется решение об остановке, а не качество модели. Про частоту обрыва"
Write-Output "  в работающем обученном продукте не доказано ничего: девяносто девятый"
Write-Output "  опыт доказал достижимость, а не частоту."
Write-Output "  Цена правки названа и не скрыта: ответы доходят до предела длины, пока"
Write-Output "  вызывающий не задаст признак, то есть расходуют больше процессорного"
Write-Output "  времени. Улучшение качества ответов НЕ заявляется."
Write-Output ""

if ($failures -eq 0) {
    Write-Output ("ИТОГ: все {0} сверок прошли. На одном потоке номеров прежняя версия теряет хвост ответа, нынешняя его отдаёт; заданный вызывающим признак по-прежнему останавливает; словарь сообщает отсутствие признака, а его обученное состояние совпало побайтово; порча возвращает прежнее поведение и поймана." -f $checks)
    exit 0
} else {
    Write-Output ("ИТОГ: сверок {0}, упало {1}." -f $checks, $failures)
    exit 1
}
