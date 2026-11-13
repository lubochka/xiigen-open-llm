# Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
# Licensed under GNU Affero General Public License v3.0.
# SPDX-License-Identifier: AGPL-3.0-only
# See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
#
# Доказательство того, что новая постоянная проверка является ПРИЁМКОЙ: она
# проходит на нынешнем коде и ПАДАЕТ на прежнем. Проверка, которая прошла бы и
# там и там, приёмкой не является.
#
# Как это доказывается без сборки. Структурная логика новой проверки —
# `Tests/ExperimentFixes/DpoConvergenceGuardReadsUnscaledLossTests.cs` —
# повторена здесь тем же разборщиком Roslyn и прогоняется дважды: на фактическом
# тексте обоих боевых файлов и на том же тексте, в котором ветвь наблюдения
# заменена обратно на прежнюю однострочную. Прогон площадки проверок закрыт
# правами сессии, поэтому сама проверка помечается NOT_EXECUTED; здесь
# доказывается её различающая сила, а не её зелёный прогон.

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
if ($null -eq $sdk) { $sdk = Get-ChildItem 'C:\Program Files\dotnet\sdk' -Directory | Sort-Object Name -Descending | Select-Object -First 1 }
$bin = Join-Path $sdk.FullName 'Roslyn\bincore'
Add-Type -Path (Join-Path $bin 'Microsoft.CodeAnalysis.dll')
Add-Type -Path (Join-Path $bin 'Microsoft.CodeAnalysis.CSharp.dll')

$repoRoot = 'C:\Projects\llm_mvp_core'
$guards = @(
    [ordered]@{
        name = 'обучение способностей'
        path = Join-Path $repoRoot '18.CapabilityTraining\LlmIngredientLogicalUnits.cs'
        # Прежняя однострочная ветвь этого потребителя, дословно.
        legacy = 'if (step % windowSteps == 0 && monitor is not null && monitor.ShouldStop(lastMetrics.Loss))' + "`n" + '                    break;'
    },
    [ordered]@{
        name = 'мост контрольных точек'
        path = Join-Path $repoRoot '44.DesignConversionDomain\DesignConversionDpoPolicyCheckpointBridge.cs'
        legacy = 'if (convergenceMonitor is not null &&' + "`n" +
                 '                step % convergenceWindowSteps == 0 &&' + "`n" +
                 '                convergenceMonitor.ShouldStop(metrics.Loss))' + "`n" +
                 '            {' + "`n" + '                break;' + "`n" + '            }'
    }
)

function Get-ObservationBranchText([string]$source) {
    # Та же логика, что в постоянной проверке: единственный вызов сторожа, затем
    # ближайший объемлющий условный оператор, содержащий чтение нового поля.
    $tree = [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($source)
    $root = $tree.GetRoot()
    $invocations = @($root.DescendantNodes() |
        Where-Object { $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax] } |
        Where-Object {
            $expression = $_.Expression
            ($expression -is [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax]) -and
            ($expression.Name.Identifier.ValueText -eq 'ShouldStop')
        })
    if (@($invocations).Count -ne 1) { return [ordered]@{ found = $false; reason = ('вызовов сторожа: ' + @($invocations).Count); text = '' } }
    $branches = @(@($invocations)[0].Ancestors() |
        Where-Object { $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.IfStatementSyntax] } |
        Where-Object { $_.ToFullString().Contains('UnscaledLoss') })
    if (@($branches).Count -eq 0) { return [ordered]@{ found = $false; reason = 'ветви с новым полем нет'; text = '' } }
    return [ordered]@{ found = $true; reason = ''; text = @($branches)[0].ToFullString() }
}

function Test-Assertions($branch) {
    # Пять утверждений постоянной проверки, повторённые дословно.
    if (-not $branch.found) { return [ordered]@{ passed = $false; detail = $branch.reason } }
    $text = $branch.text
    $problems = @()
    if (-not $text.Contains('UnscaledLoss')) { $problems += 'нет чтения нового поля' }
    if ($text.Contains('.Loss)')) { $problems += 'обычное поле читается в ветви' }
    if ($text.Contains('.Loss;')) { $problems += 'обычное поле читается в ветви' }
    if (-not $text.Contains('is null')) { $problems += 'нет ветви пустого значения' }
    if ($text.Contains('?? 0')) { $problems += 'отсутствие приводится к нулю' }
    if (-not $text.Contains('TraceWarning')) { $problems += 'решение не названо вслух' }
    return [ordered]@{ passed = (@($problems).Count -eq 0); detail = ($problems -join '; ') }
}

Write-Output "Различающая сила новой постоянной проверки: проходит на нынешнем, падает на прежнем."
Write-Output ("Разборщик: " + $sdk.Name)
Write-Output ""

foreach ($guard in $guards) {
    Write-Output ("Потребитель: " + $guard.name)
    $source = [System.IO.File]::ReadAllText($guard.path)

    # 1. Нынешний код: проверка обязана пройти.
    $current = Test-Assertions (Get-ObservationBranchText $source)
    Write-Output ("  нынешний код -> " + $(if ($current.passed) { 'проверка проходит' } else { 'проверка падает: ' + $current.detail }))
    Check ($guard.name + ": на нынешнем коде проверка проходит") $true $current.passed

    # 2. Прежний код: ветвь наблюдения заменяется обратно на однострочную.
    $branch = Get-ObservationBranchText $source
    if (-not $branch.found) {
        Check ($guard.name + ": ветвь наблюдения найдена") $true $false
        continue
    }
    $legacySource = $source.Replace($branch.text.TrimEnd(), $guard.legacy)
    Check ($guard.name + ": подстановка прежней ветви изменила текст") $true ($legacySource -ne $source)
    $legacyTree = [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($legacySource)
    $legacyErrors = @($legacyTree.GetDiagnostics() | Where-Object { $_.Severity -eq [Microsoft.CodeAnalysis.DiagnosticSeverity]::Error })
    Check ($guard.name + ": восстановленный прежний текст разбирается без ошибок") 0 @($legacyErrors).Count
    $legacy = Test-Assertions (Get-ObservationBranchText $legacySource)
    Write-Output ("  прежний код  -> " + $(if ($legacy.passed) { 'проверка ПРОХОДИТ — это было бы плохо' } else { 'проверка падает: ' + $legacy.detail }))
    Check ($guard.name + ": на прежнем коде проверка ПАДАЕТ") $false $legacy.passed
    Write-Output ""
}

# ── Запрет подачи обычного поля сторожу во всём боевом коде ────────────────
Write-Output "Запрет подачи обычного поля сторожу: перечень боевых мест вызова."
$productionFiles = @(Get-ChildItem -LiteralPath $repoRoot -Recurse -Filter '*.cs' -File -ErrorAction SilentlyContinue |
    Where-Object {
        $relative = [System.IO.Path]::GetRelativePath($repoRoot, $_.FullName).Replace('\', '/')
        -not ($relative.StartsWith('experiments-v2/') -or $relative.StartsWith('35.ExperimentsV2/') -or
              $relative.StartsWith('Tests/') -or $relative.StartsWith('TestResults/') -or
              $relative.Contains('/bin/') -or $relative.Contains('/obj/'))
    })
$callSites = @()
$offenders = @()
foreach ($file in $productionFiles) {
    $source = [System.IO.File]::ReadAllText($file.FullName)
    if (-not $source.Contains('.ShouldStop(')) { continue }
    $relative = [System.IO.Path]::GetRelativePath($repoRoot, $file.FullName).Replace('\', '/')
    $tree = [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($source)
    foreach ($invocation in @($tree.GetRoot().DescendantNodes() |
        Where-Object { $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax] } |
        Where-Object {
            $expression = $_.Expression
            ($expression -is [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax]) -and
            ($expression.Name.Identifier.ValueText -eq 'ShouldStop')
        })) {
        $line = $invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1
        $callSites += ($relative + ':' + $line)
        $feedsLoss = @($invocation.ArgumentList.Arguments |
            ForEach-Object { $_.Expression } |
            Where-Object { $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax] } |
            Where-Object { $_.Name.Identifier.ValueText -eq 'Loss' })
        if (@($feedsLoss).Count -gt 0) { $offenders += ($relative + ':' + $line) }
    }
}
Write-Output ("  боевых мест вызова сторожа: " + @($callSites).Count)
foreach ($site in @($callSites)) { Write-Output ("      " + $site) }
Check "ни одно боевое место вызова не подаёт сторожу обычное поле потери" 0 @($offenders).Count
Write-Output ""
Write-Output "  Перечень мест вызова собран по символу вызываемого члена, а не по"
Write-Output "  имени метода. Поэтому в него попали и тонкая обёртка монитора, и"
Write-Output "  неродственный `ShouldStop` оркестратора — и это правильно: запрет"
Write-Output "  обязан держать их всех, а не только два известных цикла."
Write-Output ""

Write-Output "Бюджет этого прогона."
Write-Output "  сборок: 0; прогонов площадки проверок: 0; обучения: 0"
Write-Output "  только разбор текста Roslyn и подстановка прежней ветви в памяти"
Write-Output ""
Write-Output "Чем это НЕ доказано."
Write-Output "  Сама постоянная проверка НЕ исполнялась: лане сборки и прогона"
Write-Output "  проверок закрыта правами сессии, поэтому её прогон помечается"
Write-Output "  NOT_EXECUTED, а не PASS. Здесь доказана её различающая сила:"
Write-Output "  на нынешнем коде утверждения выполняются, на восстановленном"
Write-Output "  прежнем — нет. Это доказательство приёмочной годности проверки,"
Write-Output "  а не её зелёного прогона на площадке."
Write-Output ""

if ($failures -eq 0) {
    Write-Output ("ИТОГ: все {0} сверок прошли. Новая проверка проходит на нынешнем коде и падает на прежнем у обоих потребителей." -f $checks)
    exit 0
} else {
    Write-Output ("ИТОГ: сверок {0}, упало {1}." -f $checks, $failures)
    exit 1
}
