# Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
# Licensed under GNU Affero General Public License v3.0.
# SPDX-License-Identifier: AGPL-3.0-only
# See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
#
# Проверяемая программа: выразим ли устаревший псевдоним свойства договора
# признаков так, чтобы двадцать замороженных хозяев опытов продолжали
# собираться, а честное имя стало главным.
#
# Проверяемый путь (Г): свойство переименовывается, а старое имя остаётся
# помеченным устаревшим псевдонимом, пересылающим в настоящее поле.
#
# Что здесь измеряется, а что нет. Устройство договора воспроизводится
# ДОСЛОВНО по форме: закрытая запись, свойство объявлено в теле с значением по
# умолчанию и установкой только при создании, псевдоним помечен устаревшим и
# пересылает в настоящее. Это **замена по форме**, а не сам продуктовый файл:
# настоящий файл договора тянет за собой весь граф зависимостей проекта, и
# собрать его в памяти так же дешёво нельзя. Поэтому семантика проверяется на
# замене, а сам правленый файл — разбором на отсутствие ошибок. Граница названа
# прямо и за полную сборку не выдаётся.
#
# Панели:
#   1. Выразимость: собирается ли такое устройство вообще.
#   2. Жёсткость связи: задал любое имя — читается одна и та же величина.
#   3. Выражение изменения (`with`) работает через оба имени.
#   4. Равенство записи псевдонимом не ломается и не удваивается.
#   5. Предупреждение об устаревании действительно выдаётся потребителю.
#   6. Счёт имён в открытом списке не меняется, если псевдоним в него не вносить.
#   7. Чувствительность: порча пересылки обязана быть поймана.

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
$references = [System.Collections.Generic.List[Microsoft.CodeAnalysis.MetadataReference]]::new()
foreach ($dll in Get-ChildItem $refDirectory -Filter '*.dll') {
    $references.Add([Microsoft.CodeAnalysis.MetadataReference]::CreateFromFile($dll.FullName))
}

$implicitUsings = @'
global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Threading;
global using global::System.Threading.Tasks;
'@

# Сборки живут только в памяти, файлов на диске у них нет, поэтому сборка-
# потребитель не может найти договор по имени обычным путём. Поймано прогоном:
# первая редакция упала с «не удалось загрузить файл или сборку». Поэтому
# загруженные в память сборки запоминаются по имени и выдаются разрешителю.
$script:LoadedByName = @{}
[System.AppDomain]::CurrentDomain.add_AssemblyResolve({
    param($sender, $eventArgs)
    $simpleName = ($eventArgs.Name -split ',')[0]
    if ($script:LoadedByName.ContainsKey($simpleName)) { return $script:LoadedByName[$simpleName] }
    return $null
})
function Register-Loaded($assembly) {
    $script:LoadedByName[$assembly.GetName().Name] = $assembly
    return $assembly
}

function Build-Assembly([string]$code, [string]$label, [bool]$expectSuccess) {
    $parseOptions = [Microsoft.CodeAnalysis.CSharp.CSharpParseOptions]::Default.WithLanguageVersion(
        [Microsoft.CodeAnalysis.CSharp.LanguageVersion]::Latest)
    $trees = [System.Collections.Generic.List[Microsoft.CodeAnalysis.SyntaxTree]]::new()
    $trees.Add([Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($code, $parseOptions))
    $trees.Add([Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($implicitUsings, $parseOptions))
    $options = [Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions]::new(
        [Microsoft.CodeAnalysis.OutputKind]::DynamicallyLinkedLibrary)
    $options = $options.WithNullableContextOptions([Microsoft.CodeAnalysis.NullableContextOptions]::Enable)
    $compilation = [Microsoft.CodeAnalysis.CSharp.CSharpCompilation]::Create(
        ('flags' + [Guid]::NewGuid().ToString('N')), $trees, $references, $options)
    $stream = New-Object System.IO.MemoryStream
    $emit = $compilation.Emit($stream)
    $errors = @($emit.Diagnostics | Where-Object { $_.Severity -eq [Microsoft.CodeAnalysis.DiagnosticSeverity]::Error })
    $warnings = @($emit.Diagnostics | Where-Object { $_.Severity -eq [Microsoft.CodeAnalysis.DiagnosticSeverity]::Warning })
    if (-not $emit.Success) {
        [Console]::Out.WriteLine("      ошибки сборки (" + $label + "):")
        foreach ($d in @($errors | Select-Object -First 6)) { [Console]::Out.WriteLine('        ' + $d.ToString()) }
    }
    return [ordered]@{
        Success  = $emit.Success
        Errors   = $errors
        Warnings = $warnings
        Assembly = if ($emit.Success) { Register-Loaded ([System.Reflection.Assembly]::Load($stream.ToArray())) } else { $null }
    }
}

# Дословная по форме замена устройства договора: закрытая запись, свойство в
# теле со значением по умолчанию и установкой только при создании.
$contract = @'
namespace Probe;

// Настоящее имя: говорит, что ниже порога ЗАКАЗЫВАЕТСЯ УРОК, а не отвергается ответ.
public sealed record Flags
{
    public static IReadOnlyList<string> OptimizationFlagNames { get; } = Array.AsReadOnly([
        nameof(UseCompiledPredicateRepresentation),
        nameof(UseLowConfidenceLessonSignal),
        nameof(UseTraceEvidence)
    ]);

    public bool UseCompiledPredicateRepresentation { get; init; } = true;
    public bool UseLowConfidenceLessonSignal { get; init; } = true;
    public bool UseTraceEvidence { get; init; } = true;

    // Устаревший псевдоним: пересылает в настоящее поле. Своего хранилища НЕТ,
    // поэтому разойтись с настоящим именем он не может по устройству.
    [Obsolete("Старое имя обманывало: это не минимум уверенности, а заказ урока. Используйте UseLowConfidenceLessonSignal.")]
    public bool UseConfidenceFloor
    {
        get => UseLowConfidenceLessonSignal;
        init => UseLowConfidenceLessonSignal = value;
    }
}
'@

Write-Output "Проверяемая программа: устаревший псевдоним свойства договора признаков."
Write-Output ("Разборщик: " + $sdk.Name + "; эталонные сборки: " + $refPack.Name)
Write-Output ""

# ── Панель 1. Выразимость ──────────────────────────────────────────────────
Write-Output "Панель 1. Выразимо ли такое устройство вообще."
$built = Build-Assembly $contract 'договор с псевдонимом' $true
Check "договор с устаревшим псевдонимом собирается" $true $built.Success
if (-not $built.Success) {
    Write-Output ""
    Write-Output "ИТОГ: путь (Г) НЕ выразим на этом устройстве договора; дальше мерить нечего."
    Write-Output ("сверок {0}, упало {1}." -f $checks, ($failures + 1))
    exit 1
}
$flagsType = $built.Assembly.GetType('Probe.Flags')
Check "тип договора получен" $true ($null -ne $flagsType)
Write-Output ""

# ── Панель 2. Жёсткость связи ──────────────────────────────────────────────
Write-Output "Панель 2. Жёсткость связи: задал любое имя — читается одна величина."
$newProp = $flagsType.GetProperty('UseLowConfidenceLessonSignal')
$oldProp = $flagsType.GetProperty('UseConfidenceFloor')
Check "настоящее имя есть" $true ($null -ne $newProp)
Check "псевдоним есть" $true ($null -ne $oldProp)
# У псевдонима не должно быть собственного хранилища.
$backingFields = @($flagsType.GetFields([System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::NonPublic) |
    Where-Object { $_.Name -like '*UseConfidenceFloor*' })
Check "у псевдонима нет собственного хранилища" 0 @($backingFields).Count

# Значение по умолчанию читается обоими именами одинаково.
$default = [System.Activator]::CreateInstance($flagsType)
Check "по умолчанию настоящее имя истинно" $true $newProp.GetValue($default)
Check "по умолчанию псевдоним читает то же" $true $oldProp.GetValue($default)
Write-Output ""

# ── Панель 3. Выражение изменения через оба имени ──────────────────────────
Write-Output "Панель 3. Установка при создании через каждое из двух имён."
# Проверяется исполнением кода-потребителя, собранного против этого договора.
$consumer = @'
namespace Consumer;
using Probe;
public static class Probe2
{
    public static (bool viaNewReadNew, bool viaNewReadOld, bool viaOldReadNew, bool viaOldReadOld) Run()
    {
#pragma warning disable CS0618
        var viaNew = new Flags { UseLowConfidenceLessonSignal = false };
        var viaOld = new Flags { UseConfidenceFloor = false };
        return (viaNew.UseLowConfidenceLessonSignal, viaNew.UseConfidenceFloor,
                viaOld.UseLowConfidenceLessonSignal, viaOld.UseConfidenceFloor);
#pragma warning restore CS0618
    }
    public static (bool a, bool b) RunWith()
    {
#pragma warning disable CS0618
        var baseline = new Flags();
        var changedNew = baseline with { UseLowConfidenceLessonSignal = false };
        var changedOld = baseline with { UseConfidenceFloor = false };
        return (changedNew.UseConfidenceFloor, changedOld.UseLowConfidenceLessonSignal);
#pragma warning restore CS0618
    }
    public static bool EqualityHolds()
    {
#pragma warning disable CS0618
        var viaNew = new Flags { UseLowConfidenceLessonSignal = false };
        var viaOld = new Flags { UseConfidenceFloor = false };
        return viaNew == viaOld && viaNew.GetHashCode() == viaOld.GetHashCode();
#pragma warning restore CS0618
    }
}
'@
$consumerRefs = [System.Collections.Generic.List[Microsoft.CodeAnalysis.MetadataReference]]::new()
foreach ($r in $references) { $consumerRefs.Add($r) }
# Ссылка на уже собранный договор, как хозяин опыта ссылается на продукт.
$contractBytes = $null
$parseOptions2 = [Microsoft.CodeAnalysis.CSharp.CSharpParseOptions]::Default.WithLanguageVersion(
    [Microsoft.CodeAnalysis.CSharp.LanguageVersion]::Latest)
$contractTrees = [System.Collections.Generic.List[Microsoft.CodeAnalysis.SyntaxTree]]::new()
$contractTrees.Add([Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($contract, $parseOptions2))
$contractTrees.Add([Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($implicitUsings, $parseOptions2))
$contractOptions = [Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions]::new(
    [Microsoft.CodeAnalysis.OutputKind]::DynamicallyLinkedLibrary)
$contractCompilation = [Microsoft.CodeAnalysis.CSharp.CSharpCompilation]::Create(
    ('contract' + [Guid]::NewGuid().ToString('N')), $contractTrees, $references, $contractOptions)
$contractStream = New-Object System.IO.MemoryStream
$contractEmit = $contractCompilation.Emit($contractStream)
Check "договор собран отдельной сборкой" $true $contractEmit.Success
$contractBytes = $contractStream.ToArray()
$consumerRefs.Add([Microsoft.CodeAnalysis.MetadataReference]::CreateFromImage($contractBytes))

$consumerTrees = [System.Collections.Generic.List[Microsoft.CodeAnalysis.SyntaxTree]]::new()
$consumerTrees.Add([Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($consumer, $parseOptions2))
$consumerTrees.Add([Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($implicitUsings, $parseOptions2))
$consumerCompilation = [Microsoft.CodeAnalysis.CSharp.CSharpCompilation]::Create(
    ('consumer' + [Guid]::NewGuid().ToString('N')), $consumerTrees, $consumerRefs, $contractOptions)
$consumerStream = New-Object System.IO.MemoryStream
$consumerEmit = $consumerCompilation.Emit($consumerStream)
if (-not $consumerEmit.Success) {
    [Console]::Out.WriteLine("      ошибки сборки потребителя:")
    foreach ($d in @($consumerEmit.Diagnostics | Where-Object { $_.Severity -eq [Microsoft.CodeAnalysis.DiagnosticSeverity]::Error } | Select-Object -First 6)) {
        [Console]::Out.WriteLine('        ' + $d.ToString())
    }
}
Check "потребитель, использующий СТАРОЕ имя, собирается" $true $consumerEmit.Success
if ($consumerEmit.Success) {
    Register-Loaded ([System.Reflection.Assembly]::Load($contractBytes)) | Out-Null
    $consumerAssembly = Register-Loaded ([System.Reflection.Assembly]::Load($consumerStream.ToArray()))
    $probe2 = $consumerAssembly.GetType('Consumer.Probe2')
    $run = $probe2.GetMethod('Run').Invoke($null, @())
    $viaNewReadNew = $run.Item1; $viaNewReadOld = $run.Item2
    $viaOldReadNew = $run.Item3; $viaOldReadOld = $run.Item4
    Write-Output ("      задал новое -> новое=" + $viaNewReadNew + ", старое=" + $viaNewReadOld)
    Write-Output ("      задал старое -> новое=" + $viaOldReadNew + ", старое=" + $viaOldReadOld)
    Check "задал новое — старое читает то же" $viaNewReadNew $viaNewReadOld
    Check "задал старое — новое читает то же" $viaOldReadNew $viaOldReadOld
    Check "задал новое ложь — прочиталась ложь" $false $viaNewReadNew
    Check "задал старое ложь — прочиталась ложь" $false $viaOldReadNew
    Write-Output ""
    Write-Output "Панель 4. Выражение изменения и равенство записи."
    $runWith = $probe2.GetMethod('RunWith').Invoke($null, @())
    Check "изменение через новое имя видно старым" $false $runWith.Item1
    Check "изменение через старое имя видно новым" $false $runWith.Item2
    Check "равенство записи не различает путь установки" $true $probe2.GetMethod('EqualityHolds').Invoke($null, @())
    Write-Output "  Равенство опирается на хранилище, а у псевдонима его нет, поэтому"
    Write-Output "  удвоения в равенстве и свёртке не возникает."
    Write-Output ""
}

# ── Панель 5. Предупреждение об устаревании доходит до потребителя ────────
Write-Output "Панель 5. Потребитель старого имени получает ПРЕДУПРЕЖДЕНИЕ, а не ошибку."
$noSuppress = $consumer -replace '#pragma warning disable CS0618', '' -replace '#pragma warning restore CS0618', ''
$nsTrees = [System.Collections.Generic.List[Microsoft.CodeAnalysis.SyntaxTree]]::new()
$nsTrees.Add([Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($noSuppress, $parseOptions2))
$nsTrees.Add([Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($implicitUsings, $parseOptions2))
$nsCompilation = [Microsoft.CodeAnalysis.CSharp.CSharpCompilation]::Create(
    ('nosup' + [Guid]::NewGuid().ToString('N')), $nsTrees, $consumerRefs, $contractOptions)
$nsStream = New-Object System.IO.MemoryStream
$nsEmit = $nsCompilation.Emit($nsStream)
$obsoleteWarnings = @($nsEmit.Diagnostics | Where-Object { $_.Id -eq 'CS0618' })
Check "сборка потребителя старого имени УДАЁТСЯ" $true $nsEmit.Success
Check "и выдаёт предупреждение об устаревании" $true (@($obsoleteWarnings).Count -gt 0)
Write-Output ("      предупреждений об устаревании: " + @($obsoleteWarnings).Count)
if (@($obsoleteWarnings).Count -gt 0) {
    Write-Output ("      текст: " + @($obsoleteWarnings)[0].GetMessage())
}
Write-Output "  Это и есть ответ на главный вопрос пути (Г): двадцать замороженных"
Write-Output "  хозяев опытов продолжают СОБИРАТЬСЯ, получая предупреждение вместо"
Write-Output "  ошибки, и доказательная база не теряется."
Write-Output ""

# ── Панель 6. Счёт имён в открытом списке ─────────────────────────────────
Write-Output "Панель 6. Счёт имён в открытом списке не меняется."
$names = $flagsType.GetProperty('OptimizationFlagNames').GetValue($null)
$nameList = @($names)
Check "имён в списке столько же, сколько настоящих признаков" 3 $nameList.Count
Check "в списке стоит НОВОЕ имя" $true ($nameList -contains 'UseLowConfidenceLessonSignal')
Check "псевдонима в списке НЕТ" $false ($nameList -contains 'UseConfidenceFloor')
Write-Output "  Список написан вручную, поэтому псевдоним в него просто не вносится:"
Write-Output "  счёт остаётся прежним, и проверки, закрепляющие счёт, не падают."
Write-Output ""

# ── Панель 7. Чувствительность: порча пересылки ───────────────────────────
Write-Output "Панель 7. Чувствительность: порча пересылки обязана быть поймана."
# Порча: у псевдонима появляется СВОЁ хранилище, и имена расходятся.
$broken = $contract.Replace(
    @'
    public bool UseConfidenceFloor
    {
        get => UseLowConfidenceLessonSignal;
        init => UseLowConfidenceLessonSignal = value;
    }
'@,
    '    public bool UseConfidenceFloor { get; init; } = true;')
Check "подстановка порчи изменила текст" $true ($broken -ne $contract)
# Типизированный список: оболочка не приводит массив объектов к обобщённому
# перечислению деревьев разбора. Это второй случай одной и той же ошибки в моих
# программах, и он снова поймался прогоном, а не чтением.
$brokenTrees = [System.Collections.Generic.List[Microsoft.CodeAnalysis.SyntaxTree]]::new()
$brokenTrees.Add([Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($broken, $parseOptions2))
$brokenTrees.Add([Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($implicitUsings, $parseOptions2))
$brokenContract = [Microsoft.CodeAnalysis.CSharp.CSharpCompilation]::Create(
    ('broken' + [Guid]::NewGuid().ToString('N')),
    $brokenTrees,
    $references, $contractOptions)
$brokenStream = New-Object System.IO.MemoryStream
$brokenEmit = $brokenContract.Emit($brokenStream)
Check "порченый договор собирается (порча тихая)" $true $brokenEmit.Success
if ($brokenEmit.Success) {
    $brokenBytes = $brokenStream.ToArray()
    $brokenRefs = [System.Collections.Generic.List[Microsoft.CodeAnalysis.MetadataReference]]::new()
    foreach ($r in $references) { $brokenRefs.Add($r) }
    $brokenRefs.Add([Microsoft.CodeAnalysis.MetadataReference]::CreateFromImage($brokenBytes))
    $bTrees = [System.Collections.Generic.List[Microsoft.CodeAnalysis.SyntaxTree]]::new()
    $bTrees.Add([Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($consumer, $parseOptions2))
    $bTrees.Add([Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($implicitUsings, $parseOptions2))
    $bCompilation = [Microsoft.CodeAnalysis.CSharp.CSharpCompilation]::Create(
        ('bcons' + [Guid]::NewGuid().ToString('N')), $bTrees, $brokenRefs, $contractOptions)
    $bStream = New-Object System.IO.MemoryStream
    $bEmit = $bCompilation.Emit($bStream)
    if ($bEmit.Success) {
        Register-Loaded ([System.Reflection.Assembly]::Load($brokenBytes)) | Out-Null
        $bAssembly = Register-Loaded ([System.Reflection.Assembly]::Load($bStream.ToArray()))
        $bProbe = $bAssembly.GetType('Consumer.Probe2')
        $bRun = $bProbe.GetMethod('Run').Invoke($null, @())
        Write-Output ("      порченый: задал новое -> новое=" + $bRun.Item1 + ", старое=" + $bRun.Item2)
        Write-Output ("      порченый: задал старое -> новое=" + $bRun.Item3 + ", старое=" + $bRun.Item4)
        Check "порча поймана: задал новое, старое РАСХОДИТСЯ" $true ($bRun.Item1 -ne $bRun.Item2)
        Check "порча поймана: задал старое, новое РАСХОДИТСЯ" $true ($bRun.Item3 -ne $bRun.Item4)
    } else {
        Check "порченый потребитель собрался" $true $false
    }
}
Write-Output "  Порча — это ровно та болезнь двух живых имён, которую мы искали:"
Write-Output "  обе записи имеют своё хранилище, и вызывающие расходятся молча."
Write-Output "  Сверка её ловит, значит зелёный результат панели 2 что-то значит."
Write-Output ""

Write-Output "Бюджет этого прогона."
Write-Output "  сборок проекта: 0; прогонов площадки проверок: 0; обучения: 0"
Write-Output "  сборок в памяти процесса: 6; файлов на диск компилятор не писал"
Write-Output ""
Write-Output "Чем это НЕ доказано."
Write-Output "  Устройство договора воспроизведено ДОСЛОВНО ПО ФОРМЕ, но это замена,"
Write-Output "  а не продуктовый файл: настоящий договор тянет весь граф зависимостей"
Write-Output "  проекта. Семантика измерена на замене; правленый продуктовый файл"
Write-Output "  проверяется разбором на отсутствие ошибок, и за полную сборку это не"
Write-Output "  выдаётся. Родная приёмка платформы не исполнялась: NOT_EXECUTED."
Write-Output "  Про двадцать замороженных хозяев опытов доказано, что потребитель"
Write-Output "  старого имени СОБИРАЕТСЯ с предупреждением; что каждый из двадцати"
Write-Output "  собирается, здесь не проверялось — лане сборки проекта закрыта."
Write-Output ""

if ($failures -eq 0) {
    Write-Output ("ИТОГ: все {0} сверок прошли. Путь (Г) ВЫРАЗИМ: псевдоним пересылает в настоящее поле, своего хранилища не имеет, расходиться не может по устройству; потребитель старого имени собирается с предупреждением; счёт имён в списке не меняется; порча пересылки поймана." -f $checks)
    exit 0
} else {
    Write-Output ("ИТОГ: сверок {0}, упало {1}." -f $checks, $failures)
    exit 1
}
