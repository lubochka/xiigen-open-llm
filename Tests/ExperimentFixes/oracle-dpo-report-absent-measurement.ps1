# Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
# Licensed under GNU Affero General Public License v3.0.
# SPDX-License-Identifier: AGPL-3.0-only
# See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
#
# Проверяемая программа переноса отчётного читателя: отчёт обучения не
# превращает неизмеренную величину в ноль.
#
# Что здесь доказывается, по панелям:
#   1. Приёмка проходит на нынешнем коде и ПАДАЕТ на восстановленном прежнем —
#      по обоим утверждениям: договорному и структурному.
#   2. Необязательный договор САМ приведение не запрещает: выражение
#      `квитанция?.Потеря ?? 0f` при поле `float?` компилируется законно. Это
#      замеряется настоящим разбором со смыслом, а не рассуждением, и именно
#      это обосновывает, зачем к договорному утверждению нужно структурное.
#   3. Прежний договор отсутствие выразить НЕ МОЖЕТ: при обязательном `float`
#      передача `квитанция?.Потеря` — ошибка компиляции.
#   4. Достижимость отсутствия по боевому пути: измеряется границами счёта шагов.
#   5. Чувствительность: когда утверждения приёмки НЕОТЛИЧИМЫ, названо прямо.
#   6. Собственная структурная идиома сверяется ВТОРЫМ способом на двух
#      эталонах с заведомо известным ответом.
#
# Бюджет: сборок проекта нет, прогонов площадки проверок нет, обучения нет.
# Панели 2 и 3 используют разбор со смыслом в памяти процесса (Roslyn
# CSharpCompilation) над отрывком из пятнадцати строк, без файла на выходе и
# без сборки проекта.

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
$builderPath = Join-Path $repoRoot '18.CapabilityTraining\LlmIngredientLogicalUnits.cs'

Write-Output "Проверяемая программа: отчёт обучения не выдаёт неизмеренную величину за ноль."
Write-Output ("Разборщик: " + $sdk.Name)
Write-Output ""

# ── Логика утверждений приёмки, повторённая дословно ──────────────────────────

function Get-ReportCreation([string]$source) {
    $tree = [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($source)
    $creations = @($tree.GetRoot().DescendantNodes() |
        Where-Object { $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.ObjectCreationExpressionSyntax] } |
        Where-Object {
            ($_.Type -is [Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax]) -and
            ($_.Type.Identifier.ValueText -eq 'LogicalDpoTrainingResult')
        })
    if (@($creations).Count -ne 1) { return $null }
    return @($creations)[0]
}

function Get-ArgumentFor($creation, [string]$fieldName) {
    # Тот же разбор, что в приёмке: по имени, иначе по месту объявления.
    $arguments = @($creation.ArgumentList.Arguments)
    foreach ($argument in $arguments) {
        if ($null -ne $argument.NameColon -and
            $argument.NameColon.Name.Identifier.ValueText -eq $fieldName) {
            return $argument.Expression
        }
    }
    $index = if ($fieldName -eq 'LastLoss') { 3 } else { 4 }
    if ($arguments.Count -le $index) { return $null }
    return $arguments[$index].Expression
}

function Test-StructuralAssertion([string]$source, [string]$fieldName) {
    # Аргумент обязан быть условным чтением квитанции без слияния с числом.
    $creation = Get-ReportCreation $source
    if ($null -eq $creation) { return [ordered]@{ passed = $false; detail = 'места построения не ровно одно' } }
    $argument = Get-ArgumentFor $creation $fieldName
    if ($null -eq $argument) { return [ordered]@{ passed = $false; detail = 'аргумента нет' } }
    $isConditional = $argument -is [Microsoft.CodeAnalysis.CSharp.Syntax.ConditionalAccessExpressionSyntax]
    $coalesce = @($argument.DescendantNodesAndSelf() |
        Where-Object { $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.BinaryExpressionSyntax] } |
        Where-Object { $_.RawKind -eq [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::CoalesceExpression })
    $problems = @()
    if (-not $isConditional) { $problems += 'аргумент не условное чтение' }
    if (@($coalesce).Count -gt 0) { $problems += 'внутри есть слияние с числом' }
    return [ordered]@{ passed = (@($problems).Count -eq 0); detail = ($problems -join '; ') }
}

function Test-ContractAssertion([string]$source, [string]$fieldName) {
    # Поле записи обязано быть необязательной одинарной точностью.
    $tree = [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($source)
    $records = @($tree.GetRoot().DescendantNodes() |
        Where-Object { $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.RecordDeclarationSyntax] } |
        Where-Object { $_.Identifier.ValueText -eq 'LogicalDpoTrainingResult' })
    if (@($records).Count -ne 1) { return [ordered]@{ passed = $false; detail = 'записи не ровно одна' } }
    $parameters = @(@($records)[0].ParameterList.Parameters |
        Where-Object { $_.Identifier.ValueText -eq $fieldName })
    if (@($parameters).Count -ne 1) { return [ordered]@{ passed = $false; detail = 'поля нет' } }
    $declared = @($parameters)[0].Type.ToString()
    return [ordered]@{ passed = ($declared -eq 'float?'); detail = ('объявлено как ' + $declared) }
}

# ── Панель 1. Приёмка проходит на нынешнем, падает на прежнем ────────────────
Write-Output "Панель 1. Различающая сила приёмки: нынешний код против восстановленного прежнего."
$current = [System.IO.File]::ReadAllText($builderPath)

# Восстановление прежнего кода: возвращаем обязательный договор и приведение.
$legacy = $current
$legacy = $legacy.Replace("    float? LastLoss,`n    float? LastAccuracy,", "    float LastLoss,`n    float LastAccuracy,")
$legacy = $legacy.Replace("            lastMetrics?.Loss,`n            lastMetrics?.Accuracy,", "            lastMetrics?.Loss ?? 0f,`n            lastMetrics?.Accuracy ?? 0f,")
Check "подстановка прежнего кода изменила текст" $true ($legacy -ne $current)
$legacyTree = [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($legacy)
$legacyErrors = @($legacyTree.GetDiagnostics() | Where-Object { $_.Severity -eq [Microsoft.CodeAnalysis.DiagnosticSeverity]::Error })
Check "восстановленный прежний текст разбирается без ошибок" 0 @($legacyErrors).Count

foreach ($field in @('LastLoss', 'LastAccuracy')) {
    $cS = Test-StructuralAssertion $current $field
    $lS = Test-StructuralAssertion $legacy  $field
    $cC = Test-ContractAssertion   $current $field
    $lC = Test-ContractAssertion   $legacy  $field
    Write-Output ("  поле " + $field + ":")
    Write-Output ("      структурное  нынешний -> " + $(if ($cS.passed) { 'проходит' } else { 'падает: ' + $cS.detail }) + " | прежний -> " + $(if ($lS.passed) { 'ПРОХОДИТ (плохо)' } else { 'падает: ' + $lS.detail }))
    Write-Output ("      договорное   нынешний -> " + $(if ($cC.passed) { 'проходит' } else { 'падает: ' + $cC.detail }) + " | прежний -> " + $(if ($lC.passed) { 'ПРОХОДИТ (плохо)' } else { 'падает: ' + $lC.detail }))
    Check ($field + ": структурное на нынешнем проходит") $true $cS.passed
    Check ($field + ": структурное на прежнем ПАДАЕТ")   $false $lS.passed
    Check ($field + ": договорное на нынешнем проходит")  $true $cC.passed
    Check ($field + ": договорное на прежнем ПАДАЕТ")     $false $lC.passed
}
Write-Output ""

# ── Разбор со смыслом для панелей 2 и 3 ──────────────────────────────────────
$shared = Get-ChildItem 'C:\Program Files\dotnet\shared\Microsoft.NETCore.App' -Directory |
    Where-Object { $_.Name -like '9.*' } | Sort-Object Name -Descending | Select-Object -First 1
# Типизированные списки: оболочка не приводит Object[] к обобщённым
# перечислениям Roslyn сама, и попытка это сделать — отказ преобразования, а не
# молчаливый пропуск. Поймано прогоном, а не предположено.
$references = [System.Collections.Generic.List[Microsoft.CodeAnalysis.MetadataReference]]::new()
foreach ($name in @('System.Private.CoreLib.dll', 'System.Runtime.dll')) {
    $references.Add([Microsoft.CodeAnalysis.MetadataReference]::CreateFromFile(
        (Join-Path $shared.FullName $name)))
}

function Measure-SnippetErrors([string]$code) {
    $trees = [System.Collections.Generic.List[Microsoft.CodeAnalysis.SyntaxTree]]::new()
    $trees.Add([Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($code))
    $options = [Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions]::new(
        [Microsoft.CodeAnalysis.OutputKind]::DynamicallyLinkedLibrary)
    $compilation = [Microsoft.CodeAnalysis.CSharp.CSharpCompilation]::Create(
        ('probe' + [Guid]::NewGuid().ToString('N')),
        $trees,
        $references,
        $options)
    return @($compilation.GetDiagnostics() |
        Where-Object { $_.Severity -eq [Microsoft.CodeAnalysis.DiagnosticSeverity]::Error })
}

$snippetTemplate = @'
public sealed record Receipt(float Loss);
public sealed record Report(int Count, FIELDTYPE LastLoss);
public static class Probe
{
    public static Report Build(Receipt? receipt) => new Report(1, EXPRESSION);
}
'@

Write-Output "Панель 2. Необязательный договор САМ приведение не запрещает."
$withCoercion = $snippetTemplate.Replace('FIELDTYPE', 'float?').Replace('EXPRESSION', 'receipt?.Loss ?? 0f')
$errors2 = Measure-SnippetErrors $withCoercion
Check "при поле float? выражение со слиянием компилируется без ошибок" 0 @($errors2).Count
foreach ($d in @($errors2)) { Write-Output ('      ' + $d.ToString()) }
Write-Output "  Значит договорного утверждения для приёмки НЕ ДОСТАТОЧНО: приведение можно"
Write-Output "  вернуть, и оно останется законным. Именно поэтому структурное утверждение"
Write-Output "  в приёмке есть, а не добавлено для красоты."
Write-Output ""

Write-Output "Панель 3. Прежний договор отсутствие выразить НЕ МОЖЕТ."
$withoutCoercion = $snippetTemplate.Replace('FIELDTYPE', 'float').Replace('EXPRESSION', 'receipt?.Loss')
$errors3 = Measure-SnippetErrors $withoutCoercion
Check "при обязательном float передача условного чтения даёт ошибку компиляции" $true (@($errors3).Count -gt 0)
foreach ($d in @($errors3)) { Write-Output ('      ' + $d.Id + ': ' + $d.GetMessage()) }
$newContract = $snippetTemplate.Replace('FIELDTYPE', 'float?').Replace('EXPRESSION', 'receipt?.Loss')
$errors3b = Measure-SnippetErrors $newContract
Check "при поле float? то же выражение компилируется" 0 @($errors3b).Count
Write-Output "  Вот та разница, из-за которой перенос вообще нужен: на прежнем договоре"
Write-Output "  отсутствие нечем выразить, поэтому автор и был вынужден подставить ноль."
Write-Output ""

# ── Панель 4. Достижимость отсутствия по боевому пути ────────────────────────
Write-Output "Панель 4. Достижимо ли отсутствие по боевому пути: измеряется, а не предполагается."
$minimum = [regex]::Match($current, 'MinimumSupportedMaxStepCount\s*=\s*(\d+)').Groups[1].Value
$validates = [regex]::Matches($current, 'maxStepCount is < MinimumSupportedMaxStepCount').Count
$loops = [regex]::Matches($current, 'for \(var step = 1; step <= validatedMaxStepCount; step\+\+\)').Count
$assignsInLoop = [regex]::Matches($current, 'lastMetrics = trainer\.StepVector').Count
$callSites = [regex]::Matches($current, 'ExecuteWithExclusivePolicyLeaseAsync\(\s*\r?\n\s*request').Count
Check "наименьший допустимый счёт шагов равен единице" 1 $minimum
Check "проверка границ счёта шагов присутствует" 1 $validates
Check "цикл идёт от первого шага до счёта включительно" 1 $loops
Check "присваивание квитанции в теле шага одно" 1 $assignsInLoop
Check "место вызова цикла одно" 1 $callSites
Write-Output "  Вывод: счёт шагов не меньше единицы, цикл делает хотя бы один шаг, и тело"
Write-Output "  шага либо присваивает квитанцию, либо возвращает отказ и до построения"
Write-Output "  отчёта не доходит. Значит отсутствие обеих величин по боевому пути СЕГОДНЯ"
Write-Output "  недостижимо, и поведенческая проверка через прогон обучения без шагов"
Write-Output "  проверяла бы недостижимое состояние. Правка этим не обессмысливается:"
Write-Output "  устраняется ложь, которую унаследовал бы первый будущий читатель."
Write-Output ""

# ── Панель 5. Чувствительность: когда утверждения неотличимы ─────────────────
Write-Output "Панель 5. Чувствительность: когда приёмка НЕ различает старое и новое."
$onlyContract = $current.Replace("            lastMetrics?.Loss,`n            lastMetrics?.Accuracy,", "            lastMetrics?.Loss ?? 0f,`n            lastMetrics?.Accuracy ?? 0f,")
$ocC = Test-ContractAssertion   $onlyContract 'LastLoss'
$ocS = Test-StructuralAssertion $onlyContract 'LastLoss'
Check "испорченный наполовину: договорное утверждение всё ещё ПРОХОДИТ" $true $ocC.passed
Check "испорченный наполовину: структурное утверждение ПАДАЕТ"          $false $ocS.passed
$onlyStructure = $current.Replace("    float? LastLoss,`n    float? LastAccuracy,", "    float LastLoss,`n    float LastAccuracy,")
$osC = Test-ContractAssertion   $onlyStructure 'LastLoss'
$osS = Test-StructuralAssertion $onlyStructure 'LastLoss'
Check "испорченный наоборот: договорное утверждение ПАДАЕТ"              $false $osC.passed
Check "испорченный наоборот: структурное утверждение всё ещё ПРОХОДИТ"   $true $osS.passed
Write-Output "  Граница названа прямо: ни одно из двух утверждений не покрывает другое."
Write-Output "  Верни приведение, оставив необязательный договор — упадёт только"
Write-Output "  структурное. Верни обязательный договор, оставив чтение без слияния —"
Write-Output "  упадёт только договорное (и такой код вообще не собрался бы, что"
Write-Output "  измерено панелью 3). Поэтому в приёмке их два, а не одно."
Write-Output ""

# ── Панель 6. Сверка собственной идиомы вторым способом на эталонах ─────────
Write-Output "Панель 6. Собственная структурная идиома сверяется вторым способом на эталонах."
$referenceWithCoercion = @'
public sealed record LogicalDpoTrainingResult(int A, int B, int C, float? LastLoss, float? LastAccuracy, long D);
public static class Host
{
    public static object Build(Receipt? m) => new LogicalDpoTrainingResult(1, 2, 3, m?.Loss ?? 0f, m?.Accuracy ?? 0f, 4);
}
public sealed record Receipt(float Loss, float Accuracy);
'@
$referenceWithout = $referenceWithCoercion.Replace('m?.Loss ?? 0f, m?.Accuracy ?? 0f', 'm?.Loss, m?.Accuracy')

foreach ($pair in @(
    @{ name = 'эталон со слиянием (ответ заведомо «падает»)';  source = $referenceWithCoercion; expected = $false },
    @{ name = 'эталон без слияния (ответ заведомо «проходит»)'; source = $referenceWithout;     expected = $true })) {
    $byTree = (Test-StructuralAssertion $pair.source 'LastLoss').passed
    # Второй способ: независимый текстовый счёт слияний в тексте аргумента,
    # не через дерево разбора. Если два способа расходятся — доверять нельзя
    # ни одному, и об этом надо сказать, а не выбрать удобный.
    $creation = Get-ReportCreation $pair.source
    $argumentText = (Get-ArgumentFor $creation 'LastLoss').ToString()
    $byText = -not $argumentText.Contains('??')
    Write-Output ("  " + $pair.name)
    Write-Output ("      по дереву разбора: " + $byTree + " | по тексту аргумента: " + $byText)
    Check ("  " + $pair.name + " — по дереву") $pair.expected $byTree
    Check ("  " + $pair.name + " — по тексту") $pair.expected $byText
    Check ("  " + $pair.name + " — два способа согласны") $byTree $byText
}
Write-Output ""

Write-Output "Бюджет этого прогона."
Write-Output "  сборок проекта: 0; прогонов площадки проверок: 0; обучения: 0; проходов модели: 0"
Write-Output "  разбор текста и разбор со смыслом в памяти процесса над отрывком из"
Write-Output "  пятнадцати строк; файлов на выходе компилятор не создаёт"
Write-Output ""
Write-Output "Чем это НЕ доказано."
Write-Output "  Родная приёмка платформы НЕ исполнялась: NOT_EXECUTED, а не PASS — лане"
Write-Output "  сборки и прогона проверок закрыта правами сессии. Обучение не запускалось,"
Write-Output "  качество не измерялось. Ни одно сообщаемое сегодня число не изменилось: при"
Write-Output "  существующей квитанции отчёт несёт ту же величину, что и прежде, и"
Write-Output "  недостижимость отсутствия измерена панелью 4. Заявляется ровно одно:"
Write-Output "  отсутствие измерения больше не выдаётся за измеренный ноль."
Write-Output ""

if ($failures -eq 0) {
    Write-Output ("ИТОГ: все {0} сверок прошли. Приёмка падает на прежнем коде по обоим утверждениям; ни одно из них не покрывает другое." -f $checks)
    exit 0
} else {
    Write-Output ("ИТОГ: сверок {0}, упало {1}." -f $checks, $failures)
    exit 1
}
