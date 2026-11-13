# Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
# Licensed under GNU Affero General Public License v3.0.
# SPDX-License-Identifier: AGPL-3.0-only
# See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
#
# Проверяемая программа: падают ли обе приёмки переноса сигнала заказа урока на
# внесённой порче.
#
# Зачем она нужна. Поведение переносом не изменено, значит поведенческого
# различения у приёмок нет по устройству, и вся их ценность — в том, что они
# падают в тот день, когда кто-нибудь превратит сигнал в отказ или разведёт
# имена поля рецепта с ключами записанного файла. Приёмка, которая не падает ни
# на одной порче, не защищает ничего.
#
# Панели:
#   1. Инвариант «сигнал не используется как отказ»: на нынешнем коде проходит.
#   2. Три порчи превращения сигнала в гейт — каждая обязана быть поймана.
#   3. Совпадение имён поля рецепта с ключами файла: на нынешнем проходит.
#   4. Две порчи разведения имён — каждая обязана быть поймана.
#   5. Сверка вторым способом на эталонах с заведомо известным ответом.
#
# Бюджет: сборок проекта 0; прогонов площадки проверок 0; обучения 0.
# Логика обеих приёмок повторена тем же разборщиком Roslyn над фактическим
# текстом файлов и над тем же текстом с подстановленной порчей.

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

$repoRoot = 'C:\Projects\llm_mvp_core'
$signalReader = Join-Path $repoRoot '21.AgentInfrastructure\Logical\KnnLogicalUnit.cs'
$probeProgram = Join-Path $repoRoot 'tools\Rp01MechanismProbe\Program.cs'
$recordedRecipe = Join-Path $repoRoot '.kiro\xiigen-slices\work\results\MICRO-KNN-ORDER-20261016-01\recipe.json'

$signalName = 'UseLowConfidenceLessonSignal'

Write-Output "Проверяемая программа: падают ли приёмки переноса сигнала заказа урока на порче."
Write-Output ("Разборщик: " + $sdk.Name)
Write-Output ""

# ── Логика первой приёмки, повторённая дословно ─────────────────────────────
function Test-SignalIsNotARefusal([string]$source) {
    $tree = [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText(
        $source,
        [Microsoft.CodeAnalysis.CSharp.CSharpParseOptions]::Default.WithLanguageVersion(
            [Microsoft.CodeAnalysis.CSharp.LanguageVersion]::Latest))
    $errors = @($tree.GetDiagnostics() | Where-Object { $_.Severity -eq [Microsoft.CodeAnalysis.DiagnosticSeverity]::Error })
    if ($errors.Count -gt 0) { return [ordered]@{ passed = $false; detail = ('ошибок разбора ' + $errors.Count) } }

    $reads = @($tree.GetRoot().DescendantNodes() |
        Where-Object { $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax] } |
        Where-Object { $_.Name.Identifier.ValueText -eq $signalName })
    if (@($reads).Count -ne 1) { return [ordered]@{ passed = $false; detail = ('чтений признака ' + @($reads).Count) } }

    $statement = @(@($reads)[0].Ancestors() |
        Where-Object { $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax] })[0]
    if ($null -eq $statement) { return [ordered]@{ passed = $false; detail = 'оператор не найден' } }

    # Чтение обязано участвовать в объявлении переменной предупреждений.
    if (-not ($statement -is [Microsoft.CodeAnalysis.CSharp.Syntax.LocalDeclarationStatementSyntax])) {
        return [ordered]@{ passed = $false; detail = 'чтение не в объявлении, а в ' + $statement.GetType().Name }
    }
    $declarators = @($statement.Declaration.Variables)
    if (@($declarators).Count -ne 1) { return [ordered]@{ passed = $false; detail = 'объявлений не одно' } }
    if (@($declarators)[0].Identifier.ValueText -ne 'warnings') {
        return [ordered]@{ passed = $false; detail = ('назначается не предупреждениям, а ' + @($declarators)[0].Identifier.ValueText) }
    }
    $assigned = @($declarators)[0].Initializer.Value.ToFullString()
    $problems = @()
    if ($assigned.Contains('.Fail(')) { $problems += 'внутри назначения есть отказ' }
    if ($assigned.Contains('throw')) { $problems += 'внутри назначения есть исключение' }
    if ($assigned.Contains('return')) { $problems += 'внутри назначения есть выход' }
    return [ordered]@{ passed = (@($problems).Count -eq 0); detail = ($problems -join '; ') }
}

# ── Панель 1. Инвариант на нынешнем коде ───────────────────────────────────
Write-Output "Панель 1. Инвариант «сигнал не используется как отказ» на нынешнем коде."
$current = [System.IO.File]::ReadAllText($signalReader)
$currentResult = Test-SignalIsNotARefusal $current
Write-Output ("      нынешний код -> " + $(if ($currentResult.passed) { 'приёмка проходит' } else { 'приёмка падает: ' + $currentResult.detail }))
Check "на нынешнем коде инвариант держится" $true $currentResult.passed
Write-Output ""

# ── Панель 2. Три порчи превращения сигнала в гейт ─────────────────────────
Write-Output "Панель 2. Три порчи превращения сигнала в гейт."
$warningLine = '            var warnings = request.Flags.' + $signalName + ' && confidence < _lessonRequestConfidenceThreshold'
Check "строка назначения предупреждений найдена в тексте" $true $current.Contains($warningLine)

$corruptions = @(
    @{
        name = 'сигнал стал отказом'
        replace = @'
            if (request.Flags.UseLowConfidenceLessonSignal && confidence < _lessonRequestConfidenceThreshold)
                return Task.FromResult(OperationResult<LogicalUnitResult<TOutput>>.Fail("Confidence below floor.", "KNN_CONFIDENCE_BELOW_FLOOR"));
            var warnings = new string[0]
'@
    },
    @{
        name = 'сигнал стал исключением'
        replace = @'
            if (request.Flags.UseLowConfidenceLessonSignal && confidence < _lessonRequestConfidenceThreshold)
                throw new InvalidOperationException("KNN_CONFIDENCE_BELOW_FLOOR");
            var warnings = new string[0]
'@
    },
    @{
        name = 'сигнал ушёл в ранний выход'
        replace = @'
            if (request.Flags.UseLowConfidenceLessonSignal && confidence < _lessonRequestConfidenceThreshold)
            {
                return Task.FromResult(OperationResult<LogicalUnitResult<TOutput>>.Fail("below", "BELOW"));
            }
            var warnings = new string[0]
'@
    }
)
foreach ($corruption in $corruptions) {
    $broken = $current.Replace($warningLine, $corruption.replace)
    $changed = ($broken -ne $current)
    $result = Test-SignalIsNotARefusal $broken
    Write-Output ("      «" + $corruption.name + "»: текст изменён=" + $changed + " -> " + $(if ($result.passed) { 'приёмка ПРОХОДИТ (плохо)' } else { 'приёмка падает: ' + $result.detail }))
    Check ("  порча «" + $corruption.name + "» внесена") $true $changed
    Check ("  порча «" + $corruption.name + "» ПОЙМАНА") $false $result.passed
}
Write-Output "  Все три порчи поймана. Значит инвариант не вырожден: он падает именно"
Write-Output "  тогда, когда сигнал превращают в гейт, и это та поломка, которую"
Write-Output "  поведенческой проверкой не поймать, потому что поведение сегодня не"
Write-Output "  меняется вовсе."
Write-Output ""

# ── Логика второй приёмки, повторённая дословно ────────────────────────────
function Get-RecipeFieldNames([string]$source) {
    $tree = [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText(
        $source,
        [Microsoft.CodeAnalysis.CSharp.CSharpParseOptions]::Default.WithLanguageVersion(
            [Microsoft.CodeAnalysis.CSharp.LanguageVersion]::Latest))
    $records = @($tree.GetRoot().DescendantNodes() |
        Where-Object { $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.RecordDeclarationSyntax] } |
        Where-Object { $_.Identifier.ValueText -eq 'Recipe' })
    if (@($records).Count -ne 1) { return @() }
    return @(@($records)[0].ParameterList.Parameters | ForEach-Object { $_.Identifier.ValueText })
}
function Get-RecordedKeys() {
    $text = [System.IO.File]::ReadAllText($recordedRecipe)
    $document = [System.Text.Json.JsonDocument]::Parse([string]$text)
    return @($document.RootElement.EnumerateObject() | ForEach-Object { $_.Name })
}
function Test-RecipeNamesMatch([string]$probeSource) {
    $fields = Get-RecipeFieldNames $probeSource
    $keys = Get-RecordedKeys
    $problems = @()
    foreach ($guarded in @('ConfidenceFloor', 'UseConfidenceFloor')) {
        if (-not ($fields -contains $guarded)) { $problems += ('поля ' + $guarded + ' в записи нет') }
        if (-not ($keys -contains $guarded)) { $problems += ('ключа ' + $guarded + ' в файле нет') }
    }
    if (-not $probeSource.Contains('ПЕРЕИМЕНОВЫВАТЬ НЕЛЬЗЯ')) { $problems += 'запрет словами отсутствует' }
    return [ordered]@{ passed = (@($problems).Count -eq 0); detail = ($problems -join '; ') }
}

# ── Панель 3. Совпадение имён на нынешнем коде ─────────────────────────────
Write-Output "Панель 3. Совпадение имён поля рецепта с ключами записанного файла."
$probe = [System.IO.File]::ReadAllText($probeProgram)
$keys = Get-RecordedKeys
$fields = Get-RecipeFieldNames $probe
Write-Output ("      ключей в файле: " + @($keys).Count + "; полей в записи: " + @($fields).Count)
$matchResult = Test-RecipeNamesMatch $probe
Check "на нынешнем коде имена совпадают" $true $matchResult.passed
Check "оба охраняемых поля есть в записи" $true (($fields -contains 'ConfidenceFloor') -and ($fields -contains 'UseConfidenceFloor'))
Check "оба охраняемых ключа есть в файле" $true (($keys -contains 'ConfidenceFloor') -and ($keys -contains 'UseConfidenceFloor'))
Write-Output ""

# ── Панель 4. Две порчи разведения имён ────────────────────────────────────
Write-Output "Панель 4. Две порчи разведения имён поля и ключа."
$recipeDeclaration = 'internal sealed record Recipe(string Version, TrainRow[] Train, QueryRow[] Heldout, int VectorDimensions, int K, double ConfidenceFloor, bool UseConfidenceFloor, string[] RelationPredicates);'
Check "объявление записи рецепта найдено" $true $probe.Contains($recipeDeclaration)
$renameCorruptions = @(
    @{
        name = 'поле признака переименовано «для единообразия»'
        replace = $recipeDeclaration.Replace('bool UseConfidenceFloor', 'bool UseLowConfidenceLessonSignal')
    },
    @{
        name = 'поле порога переименовано «для единообразия»'
        replace = $recipeDeclaration.Replace('double ConfidenceFloor', 'double LessonRequestConfidenceThreshold')
    }
)
foreach ($corruption in $renameCorruptions) {
    $broken = $probe.Replace($recipeDeclaration, $corruption.replace)
    $changed = ($broken -ne $probe)
    $result = Test-RecipeNamesMatch $broken
    Write-Output ("      «" + $corruption.name + "»: текст изменён=" + $changed + " -> " + $(if ($result.passed) { 'приёмка ПРОХОДИТ (плохо)' } else { 'приёмка падает: ' + $result.detail }))
    Check ("  порча «" + $corruption.name + "» внесена") $true $changed
    Check ("  порча «" + $corruption.name + "» ПОЙМАНА") $false $result.passed
}
# И порча самого запрета: комментарий убран.
$withoutProhibition = $probe.Replace('ПЕРЕИМЕНОВЫВАТЬ НЕЛЬЗЯ', 'можно трогать')
$withoutResult = Test-RecipeNamesMatch $withoutProhibition
Check "  порча «запрет словами убран» ПОЙМАНА" $false $withoutResult.passed
Write-Output "  Именно эту поломку ничем другим не поймать: она молчаливая. Ключ в"
Write-Output "  записанном файле перестал бы связываться, свойство приняло бы значение"
Write-Output "  по умолчанию, и сверка файла по свёртке не заметила бы ничего, потому"
Write-Output "  что сам файл не менялся."
Write-Output ""

# ── Панель 5. Второй способ на эталонах с известным ответом ───────────────
Write-Output "Панель 5. Сверка вторым способом на эталонах с заведомо известным ответом."
# Второй способ не использует разбор вовсе: он ищет имена в тексте построчно.
function Measure-GuardedNamesByText([string]$source) {
    $lines = $source -split "`n"
    $declaration = @($lines | Where-Object { $_.Contains('internal sealed record Recipe(') })
    if (@($declaration).Count -ne 1) { return -1 }
    $count = 0
    foreach ($guarded in @('double ConfidenceFloor', 'bool UseConfidenceFloor')) {
        if (@($declaration)[0].Contains($guarded)) { $count++ }
    }
    return $count
}
$byText = Measure-GuardedNamesByText $probe
$byTree = @(@(Get-RecipeFieldNames $probe) | Where-Object { $_ -in @('ConfidenceFloor', 'UseConfidenceFloor') }).Count
Write-Output ("      эталон «нынешний код, ответ известен: два охраняемых поля»: по тексту " + $byText + ", по дереву " + $byTree)
Check "  эталон нынешнего кода: по тексту два" 2 $byText
Check "  эталон нынешнего кода: по дереву два" 2 $byTree
Check "  два способа согласны" $byText $byTree
$brokenBoth = $probe.Replace($recipeDeclaration, $recipeDeclaration.Replace('double ConfidenceFloor', 'double X').Replace('bool UseConfidenceFloor', 'bool Y'))
$byTextBroken = Measure-GuardedNamesByText $brokenBoth
$byTreeBroken = @(@(Get-RecipeFieldNames $brokenBoth) | Where-Object { $_ -in @('ConfidenceFloor', 'UseConfidenceFloor') }).Count
Write-Output ("      эталон «оба поля переименованы, ответ известен: ноль»: по тексту " + $byTextBroken + ", по дереву " + $byTreeBroken)
Check "  эталон порчи: по тексту ноль" 0 $byTextBroken
Check "  эталон порчи: по дереву ноль" 0 $byTreeBroken
Check "  два способа согласны и на порче" $byTextBroken $byTreeBroken
Write-Output "  Измеритель умеет и подтверждать совпадение, и обнаруживать расхождение,"
Write-Output "  значит зелёный результат панели 3 не слепой."
Write-Output ""

Write-Output "Бюджет этого прогона."
Write-Output "  сборок проекта: 0; прогонов площадки проверок: 0; обучения: 0; проходов модели: 0"
Write-Output "  разбор текста Roslyn, разбор записанного файла и подстановка порчи в памяти"
Write-Output ""
Write-Output "Чем это НЕ доказано."
Write-Output "  Родная приёмка платформы НЕ исполнялась: NOT_EXECUTED, а не PASS — лане"
Write-Output "  сборки проекта и прогона проверок закрыта правами сессии. Здесь"
Write-Output "  доказано, что логика обеих приёмок различает нынешний код от порченого,"
Write-Output "  а не что площадка проверок их прогнала."
Write-Output "  Поведение переносом НЕ изменено, и улучшение качества ответов не"
Write-Output "  заявляется: сколько ответов изменится в работающем продукте, не измерил"
Write-Output "  ни один из трёх опытов."
Write-Output ""

if ($failures -eq 0) {
    Write-Output ("ИТОГ: все {0} сверок прошли. Инвариант «сигнал не используется как отказ» держится на нынешнем коде и падает на всех трёх порчах превращения в гейт; совпадение имён поля рецепта с ключами файла держится и падает на всех трёх порчах разведения." -f $checks)
    exit 0
} else {
    Write-Output ("ИТОГ: сверок {0}, упало {1}." -f $checks, $failures)
    exit 1
}
