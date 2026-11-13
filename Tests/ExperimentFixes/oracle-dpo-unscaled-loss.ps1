# Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
# Licensed under GNU Affero General Public License v3.0.
# SPDX-License-Identifier: AGPL-3.0-only
# See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
#
# Проверяемая программа кандидата восемьдесят девятого опыта: отдельное поле
# немасштабированной потери для сторожа остановки обучения (уточнённый договор
# ремонта 114-post88-addressed-repair-plan-2026-11-01, строка про накопление
# построчно до индивидуального масштабирования; опубликованный аудит границ
# исполнения 116-post89-execution-boundary-audit-2026-11-01, раздел про
# правильный перенос немасштабированной потери).
#
# Что проверяется по-человечески. В векторном пути обучения вклад каждой пары
# умножается на её нормированный вес. На ветви ТОЧНЫХ пар потеря дополнительно
# умножается на плотность обучающего вектора, а награда предпочтительного,
# награда отклонённого, запас и доля верных — нет. Сторож остановки сравнивает
# с неподвижным порогом именно потерю. Значит порог оказывался зависящим от
# плотности вектора — свойства данных, а не хода обучения.
#
# Жёсткое требование договора, которое определяет способ. Немасштабированную
# потерю НЕЛЬЗЯ получить делением готовой взвешенной суммы на среднюю
# плотность: при разных значениях плотности в одном пакете такое деление даёт
# другое число. Поэтому суммирование обязано идти построчно, ДО индивидуального
# масштабирования каждой строки. Контрпример аудита проверяется здесь числом.
#
# Способ доказательства. Правило накопления перенесено сюда ДОСЛОВНО из
# векторного пути тренера и прогоняется на наборах с одинаковой и с разной
# плотностью. Это модель одного места, а не весь механизм: ни обучения, ни
# прохода модели, ни сторожа остановки здесь нет.
#
# Бюджет: сборок проекта ноль, прогонов площадки ноль, обучения ноль.

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

# ── Дословный перенос правила накопления ───────────────────────────────────
function Invoke-Accumulation {
    <#
    Строки набора: вес до нормировки, плотность обучающего вектора, потеря,
    признак точной пары. Возвращает обе суммы и производные от них величины.

    Перенос дословный:
      totalWeight     = max(1e-6, sum(max(0.01, weight)))
      vectorWeight    = max(0.01, weight) / totalWeight
      scaledWeight    = vectorWeight * signal
      lossSum        += loss * (точная ? scaledWeight : vectorWeight)
      unscaledLossSum+= loss * vectorWeight
    #>
    param(
        [Parameter(Mandatory)] $Rows,
        # Испорченный вариант: немасштабированная сумма тоже ветвится по
        # точности пары и потому совпадает с обычной.
        [switch] $MutatedBranchesToo
    )

    $totalWeight = [math]::Max(1e-6, ($Rows | ForEach-Object { [math]::Max(0.01, $_.Weight) } | Measure-Object -Sum).Sum)
    $lossSum = 0.0
    $unscaledLossSum = 0.0
    $signalSum = 0.0
    foreach ($row in $Rows) {
        $vectorWeight = [math]::Max(0.01, $row.Weight) / $totalWeight
        $scaledWeight = $vectorWeight * $row.Signal
        $lossSum += $row.Loss * $(if ($row.Exact) { $scaledWeight } else { $vectorWeight })
        $rowWeightForUnscaled = if ($MutatedBranchesToo -and $row.Exact) { $scaledWeight } else { $vectorWeight }
        $unscaledLossSum += $row.Loss * $rowWeightForUnscaled
        $signalSum += $row.Signal
    }
    $meanSignal = $signalSum / @($Rows).Count
    return [ordered]@{
        weighted = $lossSum
        unscaled = $unscaledLossSum
        mean_signal = $meanSignal
        # Запрещённый договором приём: деление готовой взвешенной суммы на
        # среднюю плотность. Считается здесь только для того, чтобы показать
        # числом, что он даёт другой ответ.
        forbidden_division = $lossSum / $meanSignal
    }
}

Write-Output "Проверяемая программа: немасштабированная потеря для сторожа остановки."
Write-Output ""

# ── Панель 1. Контрпример договора, проверенный числом ─────────────────────
Write-Output "Панель 1. Контрпример договора: разная плотность в одном пакете."
Write-Output "  Веса строк по половине, плотности один и два, потери две десятых и восемь десятых."
$counterExample = @(
    [ordered]@{ Weight = 0.5; Signal = 1.0; Loss = 0.2; Exact = $true },
    [ordered]@{ Weight = 0.5; Signal = 2.0; Loss = 0.8; Exact = $true }
)
$result = Invoke-Accumulation -Rows $counterExample
Write-Output ("  взвешенная потеря          : " + [math]::Round($result.weighted, 10))
Write-Output ("  немасштабированная, построчно: " + [math]::Round($result.unscaled, 10))
Write-Output ("  средняя плотность          : " + [math]::Round($result.mean_signal, 10))
Write-Output ("  деление на среднюю плотность: " + [math]::Round($result.forbidden_division, 10))
Check "взвешенная потеря равна девяти десятым" 0.9 ([math]::Round($result.weighted, 10))
Check "построчная немасштабированная равна половине" 0.5 ([math]::Round($result.unscaled, 10))
Check "деление на среднюю плотность даёт шесть десятых" 0.6 ([math]::Round($result.forbidden_division, 10))
Check "деление НЕ равно правильному значению" $false ([math]::Round($result.forbidden_division, 10) -eq [math]::Round($result.unscaled, 10))
Write-Output ""
Write-Output "  Вывод числом: запрещённый приём ошибается на две десятых из половины,"
Write-Output "  то есть на сорок процентов правильной величины. Это и есть причина"
Write-Output "  требования накапливать построчно, а не делить готовую сумму."
Write-Output ""

# ── Панель 2. Одинаковая плотность: приём совпадает, и это граница ─────────
Write-Output "Панель 2. Одинаковая плотность во всём пакете."
$homogeneous = @(
    [ordered]@{ Weight = 0.5; Signal = 1.5; Loss = 0.2; Exact = $true },
    [ordered]@{ Weight = 0.5; Signal = 1.5; Loss = 0.8; Exact = $true }
)
$homo = Invoke-Accumulation -Rows $homogeneous
Write-Output ("  взвешенная: " + [math]::Round($homo.weighted, 10) + "; построчная: " + [math]::Round($homo.unscaled, 10) + "; деление: " + [math]::Round($homo.forbidden_division, 10))
Check "при одинаковой плотности деление случайно совпадает с правильным" $true ([math]::Abs($homo.forbidden_division - $homo.unscaled) -lt 1e-9)
Write-Output ""
Write-Output "  Честная граница: именно это совпадение и опасно. На однородном пакете"
Write-Output "  запрещённый приём даёт верный ответ, поэтому ошибку не видно вовсе —"
Write-Output "  она появляется только при разной плотности в одном пакете."
Write-Output ""

# ── Панель 3. Смешанный набор: ветвление по точности пары ──────────────────
Write-Output "Панель 3. Смешанный набор: точные пары и обычные вместе."
$mixed = @(
    [ordered]@{ Weight = 1.0; Signal = 2.0; Loss = 0.4; Exact = $true },
    [ordered]@{ Weight = 1.0; Signal = 2.0; Loss = 0.4; Exact = $false }
)
$mix = Invoke-Accumulation -Rows $mixed
Write-Output ("  взвешенная: " + [math]::Round($mix.weighted, 10) + "; построчная: " + [math]::Round($mix.unscaled, 10))
Check "взвешенная сумма на смешанном наборе больше построчной" $true ($mix.weighted -gt $mix.unscaled)
Check "построчная сумма равна простому среднему потерь" 0.4 ([math]::Round($mix.unscaled, 10))
Write-Output ""
Write-Output "  Что это показывает. В прежней сумме одна и та же потеря даёт разный"
Write-Output "  вклад в зависимости от того, точная пара или нет. В новой сумме обе"
Write-Output "  ветви считаются одинаково, поэтому две ветви становятся сравнимы"
Write-Output "  между собой, а порог перестаёт зависеть от плотности."
Write-Output ""

# ── Панель 4. Чувствительность с названной границей ───────────────────────
Write-Output "Панель 4. Чувствительность: новая сумма испорчена правдоподобно —"
Write-Output "она тоже ветвится по точности пары и потому совпадает с прежней."
$sensitivityRows = @(
    [ordered]@{ Name = 'точная пара, плотность два'; Rows = @([ordered]@{ Weight = 1.0; Signal = 2.0; Loss = 0.5; Exact = $true }) },
    [ordered]@{ Name = 'точная пара, плотность один'; Rows = @([ordered]@{ Weight = 1.0; Signal = 1.0; Loss = 0.5; Exact = $true }) },
    [ordered]@{ Name = 'обычная пара, плотность два'; Rows = @([ordered]@{ Weight = 1.0; Signal = 2.0; Loss = 0.5; Exact = $false }) },
    [ordered]@{ Name = 'смешанный набор'; Rows = $mixed }
)
$distinguishable = 0
foreach ($row in $sensitivityRows) {
    $correct = Invoke-Accumulation -Rows $row.Rows
    $mutated = Invoke-Accumulation -Rows $row.Rows -MutatedBranchesToo
    $differs = [math]::Abs($correct.unscaled - $mutated.unscaled) -gt 1e-9
    if ($differs) { $distinguishable++ }
    Write-Output ("  {0,-28} : правило {1,-14} испорченное {2,-14} отличимо: {3}" -f $row.Name, [math]::Round($correct.unscaled, 6), [math]::Round($mutated.unscaled, 6), $differs)
}
Check "испорченный вариант отличим ровно на двух строках из четырёх" 2 $distinguishable
Write-Output ""
Write-Output "  Граница отличимости, названная прямо. Испорченный вариант отличается"
Write-Output "  ТОЛЬКО когда в наборе есть точная пара с плотностью, отличной от"
Write-Output "  единицы. Плотность равна единице, когда в обучающем векторе пары нет"
Write-Output "  ни одного значимого значения: перенесённая формула плотности —"
Write-Output "  единица плюс доля значимых слотов от шестидесяти, с ограничением"
Write-Output "  сверху двойкой. Значит на наборе из обычных пар и на наборе точных"
Write-Output "  пар с пустыми обучающими векторами порча НЕОТЛИЧИМА, и доказать"
Write-Output "  правило можно только случаем с точной парой и непустым вектором."
Write-Output ""

# ── Панель 5. Пустое значение против нуля ─────────────────────────────────
Write-Output "Панель 5. Пустое значение против нуля: разные вещи."
Write-Output "  Поле объявлено необязательным и пропускается при записи, когда его нет."
Write-Output "  Пусто = не считалось (например, квитанция, записанная до этой правки)."
Write-Output "  Ноль  = посчитано и равно нулю."
$zeroRows = @([ordered]@{ Weight = 1.0; Signal = 2.0; Loss = 0.0; Exact = $true })
$zero = Invoke-Accumulation -Rows $zeroRows
Check "посчитанный ноль действительно ноль, а не отсутствие величины" 0 ([math]::Round($zero.unscaled, 10))
Write-Output "  Это тот же урок, что в правке происхождения чисел фактического"
Write-Output "  расхода: неизвестную величину нельзя записывать нулём, потому что"
Write-Output "  ноль читается как измеренный факт."
Write-Output ""

# ── Итог ──────────────────────────────────────────────────────────────────
Write-Output "Бюджет этого прогона."
Write-Output "  сборок проекта: 0; прогонов площадки: 0; обучения: 0; проходов модели: 0"
Write-Output ""
Write-Output "Чем это НЕ доказано."
Write-Output "  Выигрыш по остановке НЕ заявляется: на пороге две тысячных экономия"
Write-Output "  шагов ноль, и фактор переворачивает решение только внутри полосы от"
Write-Output "  0,0002091527 до 0,00022655725. Заявляется устранение зависимости"
Write-Output "  порога остановки от плотности обучающего вектора — и прямо назван"
Write-Output "  риск: для настроек внутри этой полосы и для пути точных пар это"
Write-Output "  ИЗМЕНЕНИЕ ПОВЕДЕНИЯ."
Write-Output "  Родная приёмка платформы не исполнялась: NOT_EXECUTED, не PASS."
Write-Output "  Перенесено только правило накопления; сторож остановки, проход модели"
Write-Output "  и обучение сюда не переносились."
Write-Output ""

if ($failures -eq 0) {
    Write-Output ("ИТОГ: все {0} сверок прошли. Построчное накопление до масштабирования даёт правильную величину там, где деление на среднюю плотность ошибается." -f $checks)
    exit 0
} else {
    Write-Output ("ИТОГ: сверок {0}, упало {1}." -f $checks, $failures)
    exit 1
}
