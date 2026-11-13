# Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
# Licensed under GNU Affero General Public License v3.0.
# SPDX-License-Identifier: AGPL-3.0-only
# See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
#
# Проверяемая программа второй половины кандидата восемьдесят девятого опыта:
# сторож остановки обучения читает немасштабированную потерю, а не обычную.
#
# Что проверяется по-человечески. Сторож сравнивает величину с НЕПОДВИЖНЫМ
# порогом. Обычная потеря в векторном пути домножается на плотность обучающего
# вектора на ветви точных пар, а награды, запас и доля верных — нет. Поэтому
# порог зависел от свойства данных, а не от хода обучения. Теперь сторож читает
# отдельное поле, которое копится построчно до масштабирования и на обеих
# ветвях одинаково.
#
# Способ доказательства. Оба цикла шагов перенесены сюда ДОСЛОВНО, каждый со
# своим порядком условий — он у двух потребителей разный, и выравнивать его эта
# работа не бралась. На одной и той же последовательности квитанций
# прогоняются старый вариант (читает обычное поле) и новый (читает
# немасштабированное), и разница измеряется числом шагов.
#
# Бюджет: сборок ноль, прогонов площадки ноль, обучения ноль.

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

# ── Дословный перенос монитора сходимости ──────────────────────────────────
# Монитор помнит предыдущее наблюдение и отвечает «да» только когда ДВА
# подряд сравнения оказались меньше порога. Поэтому самый ранний возможный
# «да» — третий вопрос. Это не домысел: собственный тест продукта при
# интервале 1 и бюджете 5 утверждает ровно три шага.
function New-Monitor([double]$threshold) {
    return [ordered]@{ threshold = $threshold; previous = $null; calm = 0 }
}

function Invoke-ShouldStop($monitor, [double]$value) {
    if ($null -eq $monitor.previous) {
        $monitor.previous = $value
        return $false
    }
    $change = [math]::Abs($value - $monitor.previous)
    $monitor.previous = $value
    if ($change -lt $monitor.threshold) {
        $monitor.calm++
    } else {
        $monitor.calm = 0
    }
    return $monitor.calm -ge 2
}

# ── Дословный перенос двух циклов, каждый со своим порядком условий ────────
function Invoke-CapabilityLoop {
    # Порядок у потребителя обучения способностей: момент наблюдения, затем
    # существование монитора, затем сам вопрос.
    param(
        [Parameter(Mandatory)] $Receipts,
        [Parameter(Mandatory)][int] $WindowSteps,
        [Parameter(Mandatory)][double] $Threshold,
        [switch] $OldReadsScaledLoss
    )

    $monitor = if ($Threshold -eq 0) { $null } else { New-Monitor $Threshold }
    $skipped = 0
    $steps = 0
    for ($step = 1; $step -le @($Receipts).Count; $step++) {
        $receipt = @($Receipts)[$step - 1]
        $steps = $step
        if (($step % $WindowSteps) -eq 0 -and $null -ne $monitor) {
            if ($OldReadsScaledLoss) {
                if (Invoke-ShouldStop $monitor ([double]$receipt.loss)) { break }
            } else {
                $comparable = $receipt.unscaled
                if ($null -eq $comparable) {
                    $skipped++
                } elseif (Invoke-ShouldStop $monitor ([double]$comparable)) {
                    break
                }
            }
        }
    }
    return [ordered]@{ steps = $steps; skipped = $skipped }
}

function Invoke-BridgeLoop {
    # Порядок у моста контрольных точек обратный: сначала монитор, затем
    # момент наблюдения, затем вопрос. Перенесён как есть.
    param(
        [Parameter(Mandatory)] $Receipts,
        [Parameter(Mandatory)][int] $WindowSteps,
        [Parameter(Mandatory)][double] $Threshold,
        [switch] $OldReadsScaledLoss
    )

    $monitor = if ($Threshold -eq 0) { $null } else { New-Monitor $Threshold }
    $skipped = 0
    $steps = 0
    for ($step = 1; $step -le @($Receipts).Count; $step++) {
        $receipt = @($Receipts)[$step - 1]
        $steps = $step
        if ($null -ne $monitor -and ($step % $WindowSteps) -eq 0) {
            if ($OldReadsScaledLoss) {
                if (Invoke-ShouldStop $monitor ([double]$receipt.loss)) { break }
            } else {
                $comparable = $receipt.unscaled
                if ($null -eq $comparable) {
                    $skipped++
                } elseif (Invoke-ShouldStop $monitor ([double]$comparable)) {
                    break
                }
            }
        }
    }
    return [ordered]@{ steps = $steps; skipped = $skipped }
}

Write-Output "Проверяемая программа: сторож остановки читает немасштабированную потерю."
Write-Output ""

# ── Панель 1. Различающий набор: два поля расходятся через порог ───────────
Write-Output "Панель 1. Различающий набор квитанций: обычная потеря шумит, немасштабированная успокоилась."
Write-Output "  Порог 0,002. Немасштабированная меняется на 0,0001 за шаг (успокоилась),"
Write-Output "  а обычная — на 0,02 за шаг, потому что плотность вектора её растягивает."
$discriminating = @(
    [ordered]@{ loss = 0.50; unscaled = 0.2500 },
    [ordered]@{ loss = 0.52; unscaled = 0.2501 },
    [ordered]@{ loss = 0.54; unscaled = 0.2502 },
    [ordered]@{ loss = 0.56; unscaled = 0.2503 },
    [ordered]@{ loss = 0.58; unscaled = 0.2504 }
)
$capabilityOld = Invoke-CapabilityLoop -Receipts $discriminating -WindowSteps 1 -Threshold 0.002 -OldReadsScaledLoss
$capabilityNew = Invoke-CapabilityLoop -Receipts $discriminating -WindowSteps 1 -Threshold 0.002
Write-Output ("  обучение способностей: старый вариант шагов " + $capabilityOld.steps + "; новый " + $capabilityNew.steps)
Check "старый вариант не останавливается вовсе" 5 $capabilityOld.steps
Check "новый вариант останавливается на третьем шаге" 3 $capabilityNew.steps

$bridgeOld = Invoke-BridgeLoop -Receipts $discriminating -WindowSteps 1 -Threshold 0.002 -OldReadsScaledLoss
$bridgeNew = Invoke-BridgeLoop -Receipts $discriminating -WindowSteps 1 -Threshold 0.002
Write-Output ("  мост контрольных точек: старый вариант шагов " + $bridgeOld.steps + "; новый " + $bridgeNew.steps)
Check "мост: старый вариант не останавливается вовсе" 5 $bridgeOld.steps
Check "мост: новый вариант останавливается на третьем шаге" 3 $bridgeNew.steps
Write-Output ""
Write-Output "  Вывод: на одних и тех же квитанциях старое и новое поведение РАЗЛИЧАЮТСЯ."
Write-Output "  Значит проверка, различающая их, не прошла бы на прежнем коде — именно"
Write-Output "  это и требуется от приёмки."
Write-Output ""

# ── Панель 2. Обратный различающий набор ──────────────────────────────────
Write-Output "Панель 2. Обратный набор: обычная потеря выглядит успокоившейся, а немасштабированная нет."
$inverted = @(
    [ordered]@{ loss = 0.5000; unscaled = 0.25 },
    [ordered]@{ loss = 0.5001; unscaled = 0.30 },
    [ordered]@{ loss = 0.5002; unscaled = 0.35 },
    [ordered]@{ loss = 0.5003; unscaled = 0.40 },
    [ordered]@{ loss = 0.5004; unscaled = 0.45 }
)
$invOld = Invoke-CapabilityLoop -Receipts $inverted -WindowSteps 1 -Threshold 0.002 -OldReadsScaledLoss
$invNew = Invoke-CapabilityLoop -Receipts $inverted -WindowSteps 1 -Threshold 0.002
Write-Output ("  старый вариант шагов " + $invOld.steps + "; новый " + $invNew.steps)
Check "старый вариант останавливается рано по масштабированному числу" 3 $invOld.steps
Check "новый вариант проходит бюджет: по сравнимому числу обучение ещё идёт" 5 $invNew.steps
Write-Output ""
Write-Output "  Это и есть содержательная разница, а не косметика: решение об остановке"
Write-Output "  обучения переворачивается в обе стороны в зависимости от того, какое"
Write-Output "  поле подано сторожу."
Write-Output ""

# ── Панель 3. Пустое поле: не останавливать и сказать вслух ───────────────
Write-Output "Панель 3. Пустое поле немасштабированной потери: решение названо, а не молчит."
$absent = @(
    [ordered]@{ loss = 0.50; unscaled = $null },
    [ordered]@{ loss = 0.50; unscaled = $null },
    [ordered]@{ loss = 0.50; unscaled = $null },
    [ordered]@{ loss = 0.50; unscaled = $null },
    [ordered]@{ loss = 0.50; unscaled = $null }
)
$absentNew = Invoke-CapabilityLoop -Receipts $absent -WindowSteps 1 -Threshold 0.002
$absentOld = Invoke-CapabilityLoop -Receipts $absent -WindowSteps 1 -Threshold 0.002 -OldReadsScaledLoss
Write-Output ("  новый вариант: шагов " + $absentNew.steps + "; пропущенных вопросов " + $absentNew.skipped)
Write-Output ("  старый вариант на тех же данных: шагов " + $absentOld.steps)
Check "при пустом поле обучение проходит весь бюджет" 5 $absentNew.steps
Check "при пустом поле вопрос о сходимости не задаётся ни разу" 5 $absentNew.skipped
Check "ноль вместо пустого остановил бы обучение досрочно" 3 $absentOld.steps
Write-Output ""
Write-Output "  Почему это важно именно так. Если бы пустое поле приводилось к нулю,"
Write-Output "  ноль был бы неизменным от шага к шагу, два подряд спокойных сравнения"
Write-Output "  наступили бы сразу, и обучение остановилось бы на третьем шаге по"
Write-Output "  величине, которой никто не считал. Разница измерена: 5 шагов против 3."
Write-Output ""

# ── Панель 4. Нулевой порог: поведение не изменилось вовсе ────────────────
Write-Output "Панель 4. Нулевой порог: монитор не создаётся, и новое поле ничего не меняет."
$zeroOld = Invoke-CapabilityLoop -Receipts $discriminating -WindowSteps 1 -Threshold 0 -OldReadsScaledLoss
$zeroNew = Invoke-CapabilityLoop -Receipts $discriminating -WindowSteps 1 -Threshold 0
Check "при нулевом пороге старый и новый проходят одинаково" $zeroOld.steps $zeroNew.steps
Check "при нулевом пороге бюджет проходится целиком" 5 $zeroNew.steps
Check "при нулевом пороге вопрос не задаётся, поэтому и пропусков нет" 0 $zeroNew.skipped
Write-Output ""

# ── Панель 5. Чувствительность с названной границей ──────────────────────
Write-Output "Панель 5. Чувствительность: когда старое и новое НЕОТЛИЧИМЫ."
$equalFields = @(
    [ordered]@{ loss = 0.2500; unscaled = 0.2500 },
    [ordered]@{ loss = 0.2501; unscaled = 0.2501 },
    [ordered]@{ loss = 0.2502; unscaled = 0.2502 },
    [ordered]@{ loss = 0.2503; unscaled = 0.2503 },
    [ordered]@{ loss = 0.2504; unscaled = 0.2504 }
)
$eqOld = Invoke-CapabilityLoop -Receipts $equalFields -WindowSteps 1 -Threshold 0.002 -OldReadsScaledLoss
$eqNew = Invoke-CapabilityLoop -Receipts $equalFields -WindowSteps 1 -Threshold 0.002
Check "на равных полях старое и новое дают одно и то же" $eqOld.steps $eqNew.steps
$wideWindow = Invoke-CapabilityLoop -Receipts $discriminating -WindowSteps 2 -Threshold 0.002
$wideWindowOld = Invoke-CapabilityLoop -Receipts $discriminating -WindowSteps 2 -Threshold 0.002 -OldReadsScaledLoss
Check "при интервале 2 и бюджете 5 различия нет: вопросов слишком мало" $wideWindowOld.steps $wideWindow.steps
Write-Output ""
Write-Output "  Граница отличимости, названная прямо и измеренная. Старое и новое"
Write-Output "  поведение различимы ТОЛЬКО когда выполнены три условия сразу: порог не"
Write-Output "  ноль; два поля квитанции различаются (то есть в наборе есть точная пара"
Write-Output "  с плотностью, отличной от единицы); и бюджет шагов превышает три"
Write-Output "  интервала наблюдения, иначе вопрос о сходимости не задаётся трижды и"
Write-Output "  монитор не успевает ответить «да» ни в одном варианте. При равных полях"
Write-Output "  разница ноль — и это ожидаемо: на обычных путях без плотности поля"
Write-Output "  совпадают по построению."
Write-Output ""

# ── Итог ─────────────────────────────────────────────────────────────────
Write-Output "Бюджет этого прогона."
Write-Output "  сборок: 0; прогонов площадки: 0; обучения: 0; проходов модели: 0"
Write-Output ""
Write-Output "Чем это НЕ доказано."
Write-Output "  Перенесены монитор и два цикла; настоящее обучение, проход модели и"
Write-Output "  публикация контрольной точки сюда не переносились. Родная приёмка"
Write-Output "  платформы не исполнялась: NOT_EXECUTED, а не PASS."
Write-Output "  Заявляется устранение зависимости порога остановки от плотности"
Write-Output "  обучающего вектора. Выигрыш по остановке НЕ заявляется вовсе: на"
Write-Output "  действующем пороге две тысячных экономия шагов ноль, и фактор"
Write-Output "  переворачивает решение только внутри полосы от 0,0002091527 до"
Write-Output "  0,00022655725. Для настроек внутри этой полосы и для пути точных пар"
Write-Output "  это ИЗМЕНЕНИЕ ПОВЕДЕНИЯ, и оно названо заранее."
Write-Output ""

if ($failures -eq 0) {
    Write-Output ("ИТОГ: все {0} сверок прошли. Старое и новое поведение различаются на различающем наборе в обе стороны; пустое поле не останавливает обучение и названо вслух." -f $checks)
    exit 0
} else {
    Write-Output ("ИТОГ: сверок {0}, упало {1}." -f $checks, $failures)
    exit 1
}
