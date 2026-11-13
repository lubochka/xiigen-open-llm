# Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
# Licensed under GNU Affero General Public License v3.0.
# SPDX-License-Identifier: AGPL-3.0-only
# See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
#
# Проверяемая программа переноса кандидата девяносто восьмого опыта: обучение
# словаря кусочков слов сообщает, был ли достигнут запрошенный размер.
#
# Чем эта программа отличается от предыдущих. Здесь НЕ перенос логики, а
# исполнение настоящего продуктового кода: три файла проекта словаря
# (`01.Tokenizer`) собираются в памяти процесса разборщиком Roslyn — и ПРЕЖНЯЯ
# версия из опубликованной вершины, и НЫНЕШНЯЯ с правкой, — после чего обе
# прогоняются на одних и тех же корпусах. Сборка проекта не выполняется, файлов
# на диск компилятор не пишет: сборка идёт в поток в памяти и загружается
# напрямую.
#
# Панели:
#   1. Прежняя и нынешняя версии учат ПОБАЙТОВО одно и то же состояние.
#   2. Исторические отпечатки, снятые до прежнего переноса, по-прежнему совпадают.
#   3. Недостижимый запрос: прежняя версия молчит, нынешняя сообщает причину.
#   4. Достижимый запрос: нынешняя версия сообщает достижение без недобора.
#   5. Чувствительность: порча причины выхода обязана быть поймана.
#   6. Проверка аргумента ниже порога байт не тронута.
#   7. Сверка вторым способом: отпечаток состояния против прямого чтения таблиц.
#
# Бюджет: сборок проекта 0; прогонов площадки проверок 0; обучения словаря —
# считаются ниже отдельно и названы, потому что обучение словаря и есть
# измеряемое действие.

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8
if ($null -ne $PSStyle) { $PSStyle.OutputRendering = 'PlainText' }

$checks = 0
$failures = 0
$trainingRuns = 0

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

$shared = Get-ChildItem 'C:\Program Files\dotnet\shared\Microsoft.NETCore.App' -Directory |
    Where-Object { $_.Name -like '9.*' } | Sort-Object Name -Descending | Select-Object -First 1

# Ссылки берутся из пакета ЭТАЛОННЫХ сборок, а не из каталога среды исполнения.
# Причина измерена прогоном: каталог среды содержит РОДНЫЕ библиотеки
# (`coreclr.dll`, `clrjit.dll`, `msquic.dll` и другие), которые не являются
# управляемыми сборками, и компилятор отвергает их с отказом «образ не содержит
# управляемых метаданных». Попытка отфильтровать их try/catch при создании
# ссылки не помогает: ссылка создаётся отложенно, и отказ всплывает только на
# сборке. Пакет эталонных сборок содержит ровно управляемый набор.
$refPack = Get-ChildItem 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref' -Directory |
    Where-Object { $_.Name -like '9.*' } | Sort-Object Name -Descending | Select-Object -First 1
$refDirectory = Join-Path $refPack.FullName 'ref\net9.0'
$references = [System.Collections.Generic.List[Microsoft.CodeAnalysis.MetadataReference]]::new()
foreach ($dll in Get-ChildItem $refDirectory -Filter '*.dll') {
    $references.Add([Microsoft.CodeAnalysis.MetadataReference]::CreateFromFile($dll.FullName))
}


$repoRoot = 'C:\Projects\llm_mvp_core'

function Build-Tokenizer([string]$tokenizerSource, [string]$label) {
    # Три файла проекта словаря: правленый токенизатор плюс неизменные
    # разделитель и запись словаря. Атрибут видимости внутренних членов из
    # исходника убирается: сборка в памяти не имеет того дружественного имени.
    $cleaned = $tokenizerSource -replace '\[assembly: System\.Runtime\.CompilerServices\.InternalsVisibleTo\("LlmFromScratch\.Tests"\)\]', ''
    $trees = [System.Collections.Generic.List[Microsoft.CodeAnalysis.SyntaxTree]]::new()
    $parseOptions = [Microsoft.CodeAnalysis.CSharp.CSharpParseOptions]::Default.WithLanguageVersion(
        [Microsoft.CodeAnalysis.CSharp.LanguageVersion]::Latest)
    $trees.Add([Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($cleaned, $parseOptions))
    foreach ($name in @('RegexSplitter.cs', 'VocabEntry.cs')) {
        $text = [System.IO.File]::ReadAllText((Join-Path $repoRoot ('01.Tokenizer\' + $name)))
        $trees.Add([Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($text, $parseOptions))
    }
    # Файл проекта словаря включает неявные подключения пространств имён
    # (`ImplicitUsings`), и сборщик проекта добавляет их сам отдельным файлом.
    # Компилятор как библиотека этого не делает, поэтому тот же набор для
    # библиотеки на .NET 9 добавляется здесь дословно. Поймано прогоном: без
    # него отвергались `List<>`, `IEnumerable<>`, `Stream`, `CancellationToken`.
    $implicitUsings = @'
global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;
'@
    $trees.Add([Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($implicitUsings, $parseOptions))
    $options = [Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions]::new(
        [Microsoft.CodeAnalysis.OutputKind]::DynamicallyLinkedLibrary)
    $options = $options.WithNullableContextOptions([Microsoft.CodeAnalysis.NullableContextOptions]::Enable)
    $compilation = [Microsoft.CodeAnalysis.CSharp.CSharpCompilation]::Create(
        ('tok' + [Guid]::NewGuid().ToString('N')), $trees, $references, $options)
    $stream = New-Object System.IO.MemoryStream
    $emit = $compilation.Emit($stream)
    if (-not $emit.Success) {
        # Диагностика печатается НАПРЯМУЮ в поток вывода, а не через конвейер
        # функции: возвращаемое значение функции при исключении теряется вместе
        # со всем, что в него попало. Поймано прогоном: первая редакция этой
        # программы упала, не показав ни одной ошибки сборки.
        [Console]::Out.WriteLine("  СБОРКА В ПАМЯТИ НЕ УДАЛАСЬ (" + $label + "):")
        foreach ($d in @($emit.Diagnostics | Where-Object { $_.Severity -eq [Microsoft.CodeAnalysis.DiagnosticSeverity]::Error } | Select-Object -First 10)) {
            [Console]::Out.WriteLine('      ' + $d.ToString())
        }
        throw ('сборка в памяти не удалась: ' + $label)
    }
    return [System.Reflection.Assembly]::Load($stream.ToArray())
}

# Прежняя версия берётся из опубликованной вершины, а не пересочиняется.
$publishedSource = (& git -C $repoRoot show 'origin/codex/dpo-implementation:01.Tokenizer/BpeTokenizer.cs' | Out-String)
$currentSource = [System.IO.File]::ReadAllText((Join-Path $repoRoot '01.Tokenizer\BpeTokenizer.cs'))

Write-Output "Проверяемая программа: обучение словаря сообщает достижимость запрошенного размера."
Write-Output ("Разборщик: " + $sdk.Name + "; среда исполнения: " + $shared.Name)
Write-Output ""
Write-Output "Сборка настоящего продуктового кода в памяти процесса, две версии."
$oldAssembly = Build-Tokenizer $publishedSource 'прежняя (опубликованная вершина)'
$newAssembly = Build-Tokenizer $currentSource 'нынешняя (с правкой)'
$oldType = $oldAssembly.GetType('Tokenizer.BpeTokenizer')
$newType = $newAssembly.GetType('Tokenizer.BpeTokenizer')
Check "прежняя версия собрана" $true ($null -ne $oldType)
Check "нынешняя версия собрана" $true ($null -ne $newType)
$oldTrain = $oldType.GetMethod('Train')
$newTrain = $newType.GetMethod('Train')
Check "в прежней версии обучение возвращало void" 'Void' $oldTrain.ReturnType.Name
Check "в нынешней версии обучение возвращает итог" 'BpeTrainingOutcome' $newTrain.ReturnType.Name
Write-Output ""

function Invoke-Training($type, [string]$corpus, [int]$vocabSize) {
    $script:trainingRuns++
    $instance = [System.Activator]::CreateInstance($type)
    $outcome = $type.GetMethod('Train').Invoke($instance, @($corpus, $vocabSize))
    return [ordered]@{
        Instance = $instance
        Outcome  = $outcome
        VocabSize = $type.GetProperty('VocabSize').GetValue($instance)
        StateSha  = $type.GetMethod('ComputeStateSha256').Invoke($instance, @())
    }
}

$reachableCorpus =
    "the quick brown fox jumps over the lazy dog the quick brown fox " +
    "a tokenizer learns merges from repeated pieces of ordinary text " +
    "the tokenizer learns the pieces the tokenizer repeats the pieces"
$unreachableCorpus = "alpha beta gamma delta epsilon zeta eta theta"

# ── Панель 1. Прежняя и нынешняя версии учат одно и то же ───────────────────
Write-Output "Панель 1. Прежняя и нынешняя версии учат ПОБАЙТОВО одно и то же состояние."
foreach ($case in @(
    @{ corpus = $reachableCorpus;   request = 260; name = 'достижимый 260' },
    @{ corpus = $reachableCorpus;   request = 288; name = 'достижимый 288' },
    @{ corpus = $reachableCorpus;   request = 320; name = 'достижимый 320' },
    @{ corpus = $unreachableCorpus; request = 300; name = 'НЕдостижимый 300' },
    @{ corpus = $unreachableCorpus; request = 2048; name = 'НЕдостижимый 2048' },
    @{ corpus = $unreachableCorpus; request = 256; name = 'ровно порог 256' })) {
    $old = Invoke-Training $oldType $case.corpus $case.request
    $new = Invoke-Training $newType $case.corpus $case.request
    Write-Output ("      " + $case.name + ": прежняя размер " + $old.VocabSize + ", нынешняя " + $new.VocabSize)
    Check ("  " + $case.name + ": размер словаря совпал") $old.VocabSize $new.VocabSize
    Check ("  " + $case.name + ": отпечаток состояния совпал") $old.StateSha $new.StateSha
}
Write-Output "  Вывод: правка добавляет только сообщение об итоге. Выученный словарь"
Write-Output "  не изменился ни на байт ни в одном из шести случаев, включая оба"
Write-Output "  недостижимых и ровно пороговый."
Write-Output ""

# ── Панель 2. Исторические отпечатки до прежнего переноса ───────────────────
Write-Output "Панель 2. Отпечатки, снятые с токенизатора ДО прежнего переноса, по-прежнему совпадают."
$historical = @(
    @{ vocab = 260; sha = 'a5e6c4f48f0b4f6b2dec33d2b07af4d574a96010f5aef3ff8dbd31a28bd53cd2' },
    @{ vocab = 288; sha = '41448a7971f75b41df5e3e79b03d70864bb53a125f45116f520bf11e7b28601b' },
    @{ vocab = 320; sha = '3278531fbd374e077f8c84aa4ee006d9dbf6303f7b79e2a79490d76302479296' })
foreach ($row in $historical) {
    $run = Invoke-Training $newType $reachableCorpus $row.vocab
    Check ("  запрос " + $row.vocab + ": размер") $row.vocab $run.VocabSize
    Check ("  запрос " + $row.vocab + ": исторический отпечаток состояния") $row.sha $run.StateSha
}
Write-Output "  Это запись прошлого, а не вывод нынешнего кода: отпечатки сняты с"
Write-Output "  токенизатора в состоянии до прежнего переноса и закреплены"
Write-Output "  существующей проверкой. Их совпадение — доказательство, что"
Write-Output "  законный путь не сломан."
Write-Output ""

# ── Панель 3. Недостижимый запрос: было молчание, стало сообщение ──────────
Write-Output "Панель 3. Недостижимый запрос: прежняя версия молчала, нынешняя сообщает причину."
$unreachableNew = Invoke-Training $newType $unreachableCorpus 300
$outcome = $unreachableNew.Outcome
$outcomeType = $outcome.GetType()
$requested = $outcomeType.GetProperty('RequestedVocabSize').GetValue($outcome)
$achieved = $outcomeType.GetProperty('AchievedVocabSize').GetValue($outcome)
$merges = $outcomeType.GetProperty('LearnedMergeCount').GetValue($outcome)
$reason = $outcomeType.GetProperty('ExitReason').GetValue($outcome).ToString()
$reached = $outcomeType.GetProperty('RequestedVocabSizeReached').GetValue($outcome)
$shortfall = $outcomeType.GetProperty('VocabSizeShortfall').GetValue($outcome)
Write-Output ("      запрошено " + $requested + ", получено " + $achieved + ", слияний " + $merges + ", причина " + $reason)
Check "запрос не достигнут" $false $reached
Check "причина выхода — исчерпание соседних пар" 'AdjacentPairsExhausted' $reason
Check "достигнутый размер равен закреплённому заранее" 286 $achieved
Check "недобор равен разности" 14 $shortfall
Check "достигнутый размер согласован со словарём" $unreachableNew.VocabSize $achieved
Check "достигнутый размер согласован с числом слияний" $achieved (256 + $merges)
Write-Output "  Число 286 закреплено существующей проверкой ЗАРАНЕЕ, с объяснением"
Write-Output "  «обучение останавливается раньше, когда склеивать больше нечего»."
Write-Output "  То есть ответ был известен до этой правки и не выведен из неё."
Write-Output ""
Write-Output "  И тот же корпус с запросом 2048 — повторение измерения опыта:"
$huge = Invoke-Training $newType $unreachableCorpus 2048
$hugeOutcome = $huge.Outcome
$hugeAchieved = $hugeOutcome.GetType().GetProperty('AchievedVocabSize').GetValue($hugeOutcome)
$hugeShortfall = $hugeOutcome.GetType().GetProperty('VocabSizeShortfall').GetValue($hugeOutcome)
Write-Output ("      запрошено 2048, получено " + $hugeAchieved + ", недобор " + $hugeShortfall)
Check "запрос 2048 даёт тот же достигнутый размер, что и 300" 286 $hugeAchieved
Check "и тот же отпечаток состояния" $unreachableNew.StateSha $huge.StateSha
Write-Output "  Это и есть находка опыта, воспроизведённая здесь на другом корпусе:"
Write-Output "  два разных запроса дают побайтово одно состояние, потому что ёмкость"
Write-Output "  ограничена составом текста. Прежде об этом не сообщалось ничего;"
Write-Output "  теперь сообщается недобор 1762 и причина."
Write-Output ""

# ── Панель 4. Достижимый запрос ────────────────────────────────────────────
Write-Output "Панель 4. Достижимый запрос: сообщается достижение без недобора."
$reachedRun = Invoke-Training $newType $reachableCorpus 288
$reachedOutcome = $reachedRun.Outcome
$rt = $reachedOutcome.GetType()
Check "запрос достигнут" $true $rt.GetProperty('RequestedVocabSizeReached').GetValue($reachedOutcome)
Check "причина выхода — достижение размера" 'RequestedSizeReached' $rt.GetProperty('ExitReason').GetValue($reachedOutcome).ToString()
Check "недобора нет" 0 $rt.GetProperty('VocabSizeShortfall').GetValue($reachedOutcome)
Check "достигнутый размер равен запрошенному" 288 $rt.GetProperty('AchievedVocabSize').GetValue($reachedOutcome)
Write-Output ""

# ── Панель 5. Чувствительность: порча причины выхода ──────────────────────
Write-Output "Панель 5. Чувствительность: порча обязана быть поймана."
# Порча: причина выхода всегда «размер достигнут», даже когда пары исчерпаны.
$brokenSource = $currentSource.Replace(
    'exitReason = BpeTrainingExitReason.AdjacentPairsExhausted;',
    'exitReason = BpeTrainingExitReason.RequestedSizeReached;')
Check "подстановка порчи изменила текст" $true ($brokenSource -ne $currentSource)
$brokenAssembly = Build-Tokenizer $brokenSource 'порченая: причина всегда «достигнут»'
$brokenType = $brokenAssembly.GetType('Tokenizer.BpeTokenizer')
$brokenRun = Invoke-Training $brokenType $unreachableCorpus 300
$brokenOutcome = $brokenRun.Outcome
$bt = $brokenOutcome.GetType()
$brokenReason = $bt.GetProperty('ExitReason').GetValue($brokenOutcome).ToString()
$brokenReached = $bt.GetProperty('RequestedVocabSizeReached').GetValue($brokenOutcome)
Write-Output ("      порченая версия на недостижимом корпусе: причина " + $brokenReason + ", достигнут " + $brokenReached)
Check "порча ловится: причина неверна" $true ($brokenReason -ne 'AdjacentPairsExhausted')
Check "порча ловится: признак достижения неверен" $true ($brokenReached -eq $true)
Check "но выученное состояние у порчи то же" $unreachableNew.StateSha $brokenRun.StateSha
Write-Output "  Порча поймана утверждениями приёмки, причём выученный словарь у неё"
Write-Output "  тот же — значит приёмка проверяет именно СООБЩЕНИЕ об итоге, а не"
Write-Output "  качество обучения, и не прошла бы на неверном сообщении."
Write-Output ""

# ── Панель 6. Проверка аргумента не тронута ───────────────────────────────
Write-Output "Панель 6. Проверка аргумента ниже порога байт оставлена исключением."
$threwOld = $false
$threwNew = $false
try { Invoke-Training $oldType $reachableCorpus 255 | Out-Null } catch { $threwOld = $true }
try { Invoke-Training $newType $reachableCorpus 255 | Out-Null } catch { $threwNew = $true }
Check "прежняя версия отвергала запрос 255" $true $threwOld
Check "нынешняя версия отвергает его так же" $true $threwNew
Write-Output "  Это намеренно: размер меньше 256 — ошибка программиста, а не"
Write-Output "  свойство данных. Замена исключения отказом была бы отдельным"
Write-Output "  фактором, и перенос её не делает."
Write-Output ""

# ── Панель 7. Второй способ на эталоне с известным ответом ────────────────
Write-Output "Панель 7. Сверка вторым способом: отпечаток состояния против прямого чтения."
# Первый способ — отпечаток канонической сериализации. Второй способ её не
# использует вовсе: он читает размер словаря и последовательность токенов.
foreach ($case in @(
    @{ corpus = $reachableCorpus;   request = 288; expectedSame = $true;  name = 'один корпус, один запрос' },
    @{ corpus = $unreachableCorpus; request = 288; expectedSame = $false; name = 'разные корпусы' })) {
    $a = Invoke-Training $newType $reachableCorpus 288
    $b = Invoke-Training $newType $case.corpus $case.request
    $byHash = ($a.StateSha -eq $b.StateSha)
    $tokensA = $newType.GetMethod('Encode').Invoke($a.Instance, @($reachableCorpus))
    $tokensB = $newType.GetMethod('Encode').Invoke($b.Instance, @($reachableCorpus))
    $byBehaviour = ((($tokensA | ForEach-Object { $_ }) -join ',') -eq (($tokensB | ForEach-Object { $_ }) -join ','))
    Write-Output ("      " + $case.name + ": по отпечатку совпали=" + $byHash + " | по поведению совпали=" + $byBehaviour)
    Check ("  " + $case.name + " — по отпечатку") $case.expectedSame $byHash
    Check ("  " + $case.name + " — по поведению") $case.expectedSame $byBehaviour
    Check ("  " + $case.name + " — два способа согласны") $byHash $byBehaviour
}
Write-Output "  Измеритель умеет и подтверждать совпадение, и обнаруживать различие."
Write-Output "  Значит «состояния совпали» в панели 1 — не слепая зелёная галочка."
Write-Output ""

Write-Output "Бюджет этого прогона."
Write-Output ("  сборок проекта: 0; прогонов площадки проверок: 0; обучений словаря: " + $trainingRuns)
Write-Output "  сборки в памяти процесса: 3 (прежняя, нынешняя, порченая); файлов на"
Write-Output "  диск компилятор не писал — поток в памяти и загрузка напрямую"
Write-Output "  нейронных обновлений: 0 — у словаря кусочков слов слоёв модели нет"
Write-Output ""
Write-Output "Чем это НЕ доказано."
Write-Output "  Родная приёмка платформы НЕ исполнялась: NOT_EXECUTED, а не PASS —"
Write-Output "  лане сборки проекта и прогона проверок закрыта правами сессии."
Write-Output "  Здесь исполнен настоящий продуктовый код, собранный в памяти, но это"
Write-Output "  не прогон площадки проверок и не заменяет его."
Write-Output "  Ничего не доказано о качестве разбора на незнакомых словах: девяносто"
Write-Output "  восьмой опыт прямо назвал это главным ограничителем своего вывода."
Write-Output "  Правка НЕ меняет ни один выученный словарь и НЕ заявляет улучшения"
Write-Output "  обучения: заявляется ровно одно — недостижимость запрошенного размера"
Write-Output "  больше не молчит."
Write-Output ""

if ($failures -eq 0) {
    Write-Output ("ИТОГ: все {0} сверок прошли. Выученное состояние прежней и нынешней версии совпало побайтово во всех шести случаях; исторические отпечатки до прежнего переноса совпали; недостижимость теперь сообщается с причиной и недобором; порча причины поймана." -f $checks)
    exit 0
} else {
    Write-Output ("ИТОГ: сверок {0}, упало {1}." -f $checks, $failures)
    exit 1
}
