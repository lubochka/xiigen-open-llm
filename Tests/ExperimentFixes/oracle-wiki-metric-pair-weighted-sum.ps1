# Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
# Licensed under GNU Affero General Public License v3.0.
# SPDX-License-Identifier: AGPL-3.0-only
# See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
#
# Проверяемая программа: смешиваются ли масштабы в свёртке показателей по парам
# бегунка показателей вики проекта
# (`42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs`, строки 662–666).
#
# Проверяемое подозрение: «взвешенное складывается с невзвешенным, и получается
# число, которого никто не считал».
#
# Главное требование к этой программе — не подтвердить вывод, а быть способной
# его опровергнуть. Поэтому в ней есть панели чувствительности: они вносят
# настоящее смешивание и требуют, чтобы сверка его ПОЙМАЛА. Зелёная программа,
# которая не падает ни на одной порче, доказательством не является.
#
# Панели:
#   1. Дословный перенос свёртки на достижимой форме (ровно 50 строк по 5).
#   2. Та же свёртка на НЕровных партиях: взвешивание по числу пар — именно то,
#      что делает результат точным; невзвешенное среднее средних было бы неверно.
#   3. Чувствительность: вносится настоящее смешивание (делитель снят; вес входит
#      в потерю, а делитель остался простым счётом) — сверка обязана поймать.
#   4. Тождество процессорного времени: умножение на число ядер есть точная
#      обратная операция к делению внутри вычисления доли загрузки.
#   5. Достижимость: что из защитных ветвей вообще может сработать.
#   6. Сверка вторым способом на эталонах с заведомо известным ответом.
#
# Бюджет: сборок проекта нет, прогонов площадки проверок нет, обучения нет.
# Только арифметика переноса и разбор текста исходников.

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

$repoRoot = 'C:\Projects\llm_mvp_core'
$runnerPath = Join-Path $repoRoot '42.ProjectWikiDomain\ProjectWikiDpoMetricRunner.cs'
$trainerPath = Join-Path $repoRoot '11.DPO\DpoTrainer.cs'
$batchPath = Join-Path $repoRoot '11.DPO\Models\DpoBatch.cs'

Write-Output "Проверяемая программа: смешиваются ли масштабы в свёртке показателей по парам."
Write-Output ""

# ── Дословный перенос обеих половин ──────────────────────────────────────────

function Invoke-StepManyLoss([double[]]$pairLosses) {
    # Дословно из DpoTrainer.StepMany: lossSum копится по партиям, затем
    # делится на ЧИСЛО ПАРТИЙ. Одна партия пары = одна пара, потому что
    # тензоры объявлены формой [1, T].
    if ($pairLosses.Count -eq 0) { throw 'хотя бы одна партия обязательна' }
    $lossSum = 0.0
    foreach ($loss in $pairLosses) { $lossSum += $loss }
    return $lossSum / $pairLosses.Count
}

function Invoke-RunnerAggregation($summaries) {
    # Дословно из ProjectWikiDpoMetricRunner, строки 662–663.
    $pairCountSum = 0
    foreach ($summary in $summaries) { $pairCountSum += $summary.PairCount }
    $trainedPairCount = [Math]::Max(1, $pairCountSum)
    $weighted = 0.0
    foreach ($summary in $summaries) { $weighted += $summary.Loss * $summary.PairCount }
    return $weighted / $trainedPairCount
}

function Get-ExactMean([double[]]$values) {
    $sum = 0.0
    foreach ($value in $values) { $sum += $value }
    return $sum / $values.Count
}

function Build-Summaries([double[]]$allPairLosses, [int]$miniBatchSize) {
    # Дословно из бегунка: группировка по целочисленному делению номера строки
    # на размер мини-партии; сводка несёт среднее партии и число пар в ней.
    $summaries = @()
    for ($start = 0; $start -lt $allPairLosses.Count; $start += $miniBatchSize) {
        $end = [Math]::Min($start + $miniBatchSize, $allPairLosses.Count)
        $slice = @($allPairLosses[$start..($end - 1)])
        $summaries += [pscustomobject]@{
            PairCount = $slice.Count
            Loss      = Invoke-StepManyLoss $slice
        }
    }
    return $summaries
}

# Набор потерь, заведомо разных по величине, чтобы любое смешивание проявилось.
$fifty = @(0..49 | ForEach-Object { 0.1 + ($_ * 0.037) })

# ── Панель 1. Достижимая форма: ровно 50 строк по 5 ─────────────────────────
Write-Output "Панель 1. Достижимая форма: ровно 50 строк, мини-партия 5."
$summaries50 = Build-Summaries $fifty 5
$aggregated = Invoke-RunnerAggregation $summaries50
$exact = Get-ExactMean $fifty
Check "число сводок" 10 @($summaries50).Count
Check "все мини-партии ровно по 5 пар" 1 (@($summaries50 | Select-Object -ExpandProperty PairCount -Unique).Count)
Check "свёртка совпадает с точным средним по всем 50 парам" $true ([Math]::Abs($aggregated - $exact) -lt 1e-12)
Write-Output ("      свёртка " + $aggregated.ToString('F15') + " | точное среднее " + $exact.ToString('F15'))
Write-Output "  На достижимой форме все мини-партии равны по числу пар, поэтому"
Write-Output "  взвешивание по числу пар здесь даже избыточно: оно защитное, а не"
Write-Output "  несущее. Смешивать нечего — складываются величины одного масштаба"
Write-Output "  (средние по парам), с весом, равным числу пар каждой."
Write-Output ""

# ── Панель 2. Неровные партии: взвешивание и есть то, что делает точным ─────
Write-Output "Панель 2. Неровные партии: взвешивание по числу пар обязательно."
$fortySeven = @($fifty[0..46])
$summaries47 = Build-Summaries $fortySeven 5
$aggregated47 = Invoke-RunnerAggregation $summaries47
$exact47 = Get-ExactMean $fortySeven
# Невзвешенное среднее средних — вот это было бы настоящим смешиванием.
$unweighted47 = Get-ExactMean (@($summaries47 | ForEach-Object { $_.Loss }))
Check "последняя мини-партия короче остальных" 2 (@($summaries47)[-1].PairCount)
Check "свёртка бегунка точна и на неровных партиях" $true ([Math]::Abs($aggregated47 - $exact47) -lt 1e-12)
Check "невзвешенное среднее средних НЕ точно" $true ([Math]::Abs($unweighted47 - $exact47) -gt 1e-9)
Write-Output ("      свёртка " + $aggregated47.ToString('F15'))
Write-Output ("      точное среднее " + $exact47.ToString('F15'))
Write-Output ("      невзвешенное среднее средних " + $unweighted47.ToString('F15') + "  (отклонение " + [Math]::Abs($unweighted47 - $exact47).ToString('E3') + ")")
Write-Output "  Вывод: взвешивание по числу пар — не источник смешивания, а защита"
Write-Output "  ОТ него. Именно оно делает результат точным, когда последняя"
Write-Output "  мини-партия короче. Автор сделал это правильно."
Write-Output ""

# ── Панель 3. Чувствительность: вносим настоящее смешивание ────────────────
Write-Output "Панель 3. Чувствительность: сверка обязана ПОЙМАТЬ настоящее смешивание."

# Порча 1: делитель снят — сводка несёт СУММУ партии, а вес остался числом пар.
function Invoke-StepManyLossBrokenNoDivisor([double[]]$pairLosses) {
    $lossSum = 0.0
    foreach ($loss in $pairLosses) { $lossSum += $loss }
    return $lossSum
}
$brokenSummaries = @()
for ($start = 0; $start -lt $fifty.Count; $start += 5) {
    $slice = @($fifty[$start..([Math]::Min($start + 5, $fifty.Count) - 1)])
    $brokenSummaries += [pscustomobject]@{ PairCount = $slice.Count; Loss = Invoke-StepManyLossBrokenNoDivisor $slice }
}
$broken1 = Invoke-RunnerAggregation $brokenSummaries
Check "порча 1 (суммы вместо средних) поймана" $true ([Math]::Abs($broken1 - $exact) -gt 1e-9)
Write-Output ("      порченая свёртка " + $broken1.ToString('F9') + " против точного " + $exact.ToString('F9') + " — отклонение в " + ($broken1 / $exact).ToString('F3') + " раза")

# Порча 2: вес входит в потерю, а делитель остался простым счётом партий.
# Это буквально то подозрение, которое проверяется: взвешенное поделено на
# невзвешенный счёт.
#
# ВАЖНО про выбор весов. Первая редакция этой панели брала вес
# `0.5 + ((индекс % 5) * 0.25)` и НЕ ловила порчу — потому что такой вес
# периодичен с периодом 5, равным размеру мини-партии: сумма весов в КАЖДОЙ
# мини-партии выходила ровно 5, средний вес ровно 1, и обе формулы совпадали
# тождественно. Эталон был выроженным и не мог поймать порчу по построению.
# Поймано прогоном, а не рассуждением. Здесь вес намеренно НЕ периодичен
# размеру мини-партии, и это проверяется отдельной сверкой ниже.
$weights = @(0..49 | ForEach-Object { 0.3 + (($_ % 7) * 0.4) })
function Invoke-StepManyLossBrokenWeighted([double[]]$pairLosses, [double[]]$pairWeights) {
    $lossSum = 0.0
    for ($index = 0; $index -lt $pairLosses.Count; $index++) {
        $lossSum += $pairLosses[$index] * $pairWeights[$index]
    }
    return $lossSum / $pairLosses.Count
}
$broken2Summaries = @()
for ($start = 0; $start -lt $fifty.Count; $start += 5) {
    $stop = [Math]::Min($start + 5, $fifty.Count) - 1
    $slice = @($fifty[$start..$stop])
    $sliceWeights = @($weights[$start..$stop])
    $broken2Summaries += [pscustomobject]@{ PairCount = $slice.Count; Loss = Invoke-StepManyLossBrokenWeighted $slice $sliceWeights }
}
$broken2 = Invoke-RunnerAggregation $broken2Summaries
# Правильная взвешенная величина: сумма веса на потерю, делённая на сумму весов.
$properWeighted = 0.0
$weightSum = 0.0
for ($index = 0; $index -lt $fifty.Count; $index++) {
    $properWeighted += $fifty[$index] * $weights[$index]
    $weightSum += $weights[$index]
}
$properWeighted = $properWeighted / $weightSum
# Эталон не выроженный: сумма весов НЕ равна числу пар, иначе обе формулы
# совпали бы тождественно и панель была бы слепой.
Check "эталон весов не выроженный: сумма весов не равна числу пар" $true ([Math]::Abs($weightSum - $fifty.Count) -gt 1e-9)
Check "порча 2 (взвешенное делённое на невзвешенный счёт) поймана" $true ([Math]::Abs($broken2 - $properWeighted) -gt 1e-9)
Write-Output ("      порченая свёртка " + $broken2.ToString('F9') + " против правильного взвешенного " + $properWeighted.ToString('F9'))
Write-Output "  Обе порчи на этом наборе пойманы. Но зелёное здесь ничего не стоит без"
Write-Output "  названного условия: в первой редакции порча 2 была НЕОТЛИЧИМА, и"
Write-Output "  условие её неотличимости установлено ниже отдельной панелью, а не"
Write-Output "  обойдено подбором удачного набора."
Write-Output ""

# ── Панель 3b. Условие неотличимости порчи 2, установленное разделением ────
Write-Output "Панель 3b. При каком условии порча 2 НЕОТЛИЧИМА: две гипотезы разделены."
Write-Output "  Гипотеза А: виноваты равные размеры мини-партий."
Write-Output "  Гипотеза Б: виновато равенство суммы весов числу пар (средний вес ровно 1)."

function Invoke-BrokenWeightedBatch([double[]]$losses, [double[]]$pairWeights) {
    $sum = 0.0
    for ($index = 0; $index -lt $losses.Count; $index++) { $sum += $losses[$index] * $pairWeights[$index] }
    return $sum / $losses.Count
}
function Build-SummariesWithSizes([double[]]$losses, [double[]]$pairWeights, [int[]]$sizes) {
    $out = @()
    $position = 0
    foreach ($size in $sizes) {
        $sliceLosses = @($losses[$position..($position + $size - 1)])
        $sliceWeights = @($pairWeights[$position..($position + $size - 1)])
        $out += [pscustomobject]@{ PairCount = $size; Loss = (Invoke-BrokenWeightedBatch $sliceLosses $sliceWeights) }
        $position += $size
    }
    return $out
}
function Get-ProperWeightedMean([double[]]$losses, [double[]]$pairWeights) {
    $numerator = 0.0
    $denominator = 0.0
    for ($index = 0; $index -lt $losses.Count; $index++) {
        $numerator += $losses[$index] * $pairWeights[$index]
        $denominator += $pairWeights[$index]
    }
    return $numerator / $denominator
}

$twenty = @(0..19 | ForEach-Object { 0.1 + ($_ * 0.137) })

# Случай 1: размеры НЕРАВНЫЕ, средний вес ровно 1.
$unitMeanWeights = @(0..19 | ForEach-Object { if ($_ % 2 -eq 0) { 0.5 } else { 1.5 } })
$unitWeightSum = 0.0
foreach ($value in $unitMeanWeights) { $unitWeightSum += $value }
$case1Broken = Invoke-RunnerAggregation (Build-SummariesWithSizes $twenty $unitMeanWeights @(3, 7, 4, 6))
$case1Proper = Get-ProperWeightedMean $twenty $unitMeanWeights
Check "случай 1: сумма весов равна числу пар" $true ([Math]::Abs($unitWeightSum - $twenty.Count) -lt 1e-12)
Check "случай 1: размеры мини-партий НЕравные, но порча НЕОТЛИЧИМА" $true ([Math]::Abs($case1Broken - $case1Proper) -lt 1e-9)
Write-Output ("      размеры 3,7,4,6; средний вес " + ($unitWeightSum / $twenty.Count).ToString('F4') + " -> порченое " + $case1Broken.ToString('F12') + " = правильное " + $case1Proper.ToString('F12'))

# Случай 2: размеры РАВНЫЕ, средний вес НЕ единица.
$nonUnitWeights = @(0..19 | ForEach-Object { 0.3 + (($_ % 7) * 0.4) })
$nonUnitSum = 0.0
foreach ($value in $nonUnitWeights) { $nonUnitSum += $value }
$case2Broken = Invoke-RunnerAggregation (Build-SummariesWithSizes $twenty $nonUnitWeights @(5, 5, 5, 5))
$case2Proper = Get-ProperWeightedMean $twenty $nonUnitWeights
Check "случай 2: сумма весов НЕ равна числу пар" $true ([Math]::Abs($nonUnitSum - $twenty.Count) -gt 1e-9)
Check "случай 2: размеры мини-партий РАВНЫЕ, но порча ОТЛИЧИМА" $true ([Math]::Abs($case2Broken - $case2Proper) -gt 1e-9)
Write-Output ("      размеры 5,5,5,5; средний вес " + ($nonUnitSum / $twenty.Count).ToString('F4') + " -> порченое " + $case2Broken.ToString('F12') + " против правильного " + $case2Proper.ToString('F12'))

Write-Output "  Вывод: гипотеза А ОПРОВЕРГНУТА. Неравные размеры при среднем весе"
Write-Output "  единица дают неотличимость, а равные размеры при среднем весе не"
Write-Output "  единица дают отличимость. Размеры мини-партий сокращаются"
Write-Output "  ТОЖДЕСТВЕННО: порченая свёртка равна сумме произведений веса на"
Write-Output "  потерю, делённой на число пар, потому что делитель внутри партии"
Write-Output "  и вес снаружи — одно и то же число пар. Правильная же величина"
Write-Output "  делится на сумму весов. Значит единственное условие неотличимости:"
Write-Output "  сумма весов равна числу пар, то есть средний вес ровно единица."
Write-Output "  Первая редакция панели 3 брала вес, периодичный размеру мини-партии,"
Write-Output "  и потому попадала ровно в это условие."
Write-Output ""

# ── Панель 4. Тождество процессорного времени ──────────────────────────────
Write-Output "Панель 4. Тождество процессорного времени: умножение на ядра есть обратная операция, остаток измеряется."
$processorCount = 8
$batchObservations = @(
    @{ CpuDeltaMs = 37.0;  ElapsedMs = 12.0 },
    @{ CpuDeltaMs = 410.0; ElapsedMs = 95.0 },
    @{ CpuDeltaMs = 3.0;   ElapsedMs = 410.0 }
)
$cpuTotalFromPercent = 0.0
$cpuTotalDirect = 0.0
$cpuTotalWithoutProcessorFactor = 0.0
# ВАЖНО про дословность переноса. Первая редакция этой панели писала
# `[Math]::Max(0, <double>)`, как в боевом коде, и давала 447,84 вместо 450.
# Причина измерена: оболочка разрешает перегрузку иначе, чем язык продукта —
# `[Math]::Max(0, 38.5416)` возвращает 39 типа Int32, то есть выбирает
# целочисленную перегрузку и ОКРУГЛЯЕТ. В C# тот же вызов берёт
# `Max(double, double)`, потому что `int -> double` есть неявное преобразование,
# а `double -> int` нет. Отсюда правило: дословный перенос в другой язык
# дословным НЕ является, когда разрешение перегрузок различается. Поэтому ноль
# здесь явно типизирован, и расхождение двух языков закреплено сверкой ниже.
$shellOverloadRounds = ([Math]::Max(0, 38.5416666666)).GetType().Name -eq 'Int32'
foreach ($observation in $batchObservations) {
    # Дословно ComputeCpuPercent, с явно типизированным нулём.
    $percent = [Math]::Max([double]0, $observation.CpuDeltaMs / $observation.ElapsedMs / [Math]::Max(1, $processorCount) * 100.0)
    # Дословно строка 666.
    $cpuTotalFromPercent += $percent / 100.0 * $observation.ElapsedMs * [Math]::Max(1, $processorCount)
    $cpuTotalWithoutProcessorFactor += $percent / 100.0 * $observation.ElapsedMs
    $cpuTotalDirect += $observation.CpuDeltaMs
}
Check "оболочка действительно разрешает перегрузку иначе, чем язык продукта" $true $shellOverloadRounds
Check "сумма через долю загрузки равна прямой сумме процессорного времени" $true ([Math]::Abs($cpuTotalFromPercent - $cpuTotalDirect) -lt 1e-9)
Check "без множителя ядер результат был бы неверен" $true ([Math]::Abs($cpuTotalWithoutProcessorFactor - $cpuTotalDirect) -gt 1e-9)
Write-Output ("      через долю загрузки " + $cpuTotalFromPercent.ToString('F9') + " | прямая сумма " + $cpuTotalDirect.ToString('F9'))
Write-Output ("      без множителя ядер " + $cpuTotalWithoutProcessorFactor.ToString('F9') + " — ровно в " + $processorCount + " раз меньше")

# Остаток измеряется числом, а не объявляется нулём.
$residualAbsolute = [Math]::Abs($cpuTotalFromPercent - $cpuTotalDirect)
$residualRelative = $residualAbsolute / $cpuTotalDirect
Write-Output ("      остаток тождества: " + $residualAbsolute.ToString('E3') + " абсолютный, " + $residualRelative.ToString('E3') + " относительный")
Check "остаток тождества не превышает округления двойной точности" $true ($residualRelative -lt 1e-12)

# Разброс величин на три порядка: округление двойной точности не накапливается.
$spread = @(
    @{ CpuDeltaMs = 0.017; ElapsedMs = 3.0 },
    @{ CpuDeltaMs = 1234.567; ElapsedMs = 311.0 },
    @{ CpuDeltaMs = 99999.5; ElapsedMs = 20000.0 },
    @{ CpuDeltaMs = 0.0; ElapsedMs = 7.0 })
$spreadFromPercent = 0.0
$spreadDirect = 0.0
foreach ($observation in $spread) {
    $percent = [Math]::Max([double]0, $observation.CpuDeltaMs / $observation.ElapsedMs / [Math]::Max(1, $processorCount) * 100.0)
    $spreadFromPercent += $percent / 100.0 * $observation.ElapsedMs * [Math]::Max(1, $processorCount)
    $spreadDirect += $observation.CpuDeltaMs
}
$spreadResidual = [Math]::Abs($spreadFromPercent - $spreadDirect) / $spreadDirect
Write-Output ("      на разбросе величин в шесть порядков относительный остаток " + $spreadResidual.ToString('E3'))
Check "на разбросе величин остаток тоже в пределах округления" $true ($spreadResidual -lt 1e-12)

# Единственная НАСТОЯЩАЯ необратимость боевой цепочки: обрезание снизу нулём.
$negativeObservation = @{ CpuDeltaMs = -5.0; ElapsedMs = 10.0 }
$clampedPercent = [Math]::Max([double]0, $negativeObservation.CpuDeltaMs / $negativeObservation.ElapsedMs / [Math]::Max(1, $processorCount) * 100.0)
$clampedBack = $clampedPercent / 100.0 * $negativeObservation.ElapsedMs * [Math]::Max(1, $processorCount)
Check "обрезание нулём необратимо: отрицательная разница теряется" $true ([Math]::Abs($clampedBack - $negativeObservation.CpuDeltaMs) -gt 1e-9)
Write-Output ("      обрезание нулём: вход " + $negativeObservation.CpuDeltaMs + " мс -> восстановлено " + $clampedBack + " мс")

Write-Output "  Вывод, исправленный по измерению. Множитель числа ядер в строке 666"
Write-Output "  есть ОБРАТНАЯ операция к делению на число ядер внутри вычисления доли"
Write-Output "  загрузки: в вещественной арифметике тождественно, в арифметике двойной"
Write-Output "  точности — до округления, измеренного выше числом, а не объявленного"
Write-Output "  нулём. Слово «точная» из прежней редакции вывода убрано: побайтового"
Write-Output "  равенства у плавающей арифметики нет и быть не может."
Write-Output "  Остаток 2,16 (ноль целых сорок восемь сотых процента) из ПЕРВОГО"
Write-Output "  прогона этой программы к боевому коду отношения не имел: его причина"
Write-Output "  измерена и названа в панели сохранённого красного ниже — это была"
Write-Output "  моя оболочка, округлявшая долю загрузки до целого."
Write-Output "  Единственная настоящая необратимость боевой цепочки — обрезание"
Write-Output "  снизу нулём: отрицательная разница счётчика процессорного времени"
Write-Output "  теряется безвозвратно. Для суммы это безопасно в ту сторону, где"
Write-Output "  отрицательное процессорное время физически невозможно, но"
Write-Output "  восстановление суммы в таком прогоне будет занижено, и это названо"
Write-Output "  здесь, а не умолчано. Смешивания масштабов здесь нет."
Write-Output ""

# ── Панель 5. Достижимость защитных ветвей ─────────────────────────────────
Write-Output "Панель 5. Достижимость: какие защитные ветви вообще могут сработать."
$runner = [System.IO.File]::ReadAllText($runnerPath)
$trainer = [System.IO.File]::ReadAllText($trainerPath)
$batch = [System.IO.File]::ReadAllText($batchPath)

$fiftyGate = ([regex]::Matches($runner, 'rows\.Count\s*!=\s*50')).Count
$miniBatch = [regex]::Match($runner, 'MiniBatchSize\s*=\s*(\d+)').Groups[1].Value
$grouping = ([regex]::Matches($runner, 'index\s*/\s*MiniBatchSize')).Count
$maxGuard = ([regex]::Matches($runner, 'Math\.Max\(1,\s*summaries\.Sum')).Count
$emptyGuard = ([regex]::Matches($runner, 'summaries\.Count\s*==\s*0\s*\?\s*0')).Count
$singleShortcut = ([regex]::Matches($trainer, 'if \(batches\.Count == 1\)')).Count
$oneRowShape = ([regex]::Matches($runner, 'TensorShape\(1,\s*contextLen\)')).Count
$objectiveHasWeight = ([regex]::Matches($trainer, 'ObjectiveTerms\(DpoBatch batch, float margin\)\s*=>[^;]*Weight')).Count
$batchHasWeight = ([regex]::Matches($batch, '\bWeight\b')).Count

Check "проверка «ровно 50 строк» присутствует" 1 $fiftyGate
Check "размер мини-партии" 5 $miniBatch
Check "группировка по размеру мини-партии присутствует" 1 $grouping
Check "защита Math.Max(1, …) присутствует в тексте" 1 $maxGuard
Check "защита пустых сводок присутствует в тексте" 1 $emptyGuard
Check "ранний возврат на одной партии присутствует в тренере" 1 $singleShortcut
Check "форма тензоров пары объявлена как одна строка" 4 $oneRowShape
Check "вес в цель потери НЕ входит" 0 $objectiveHasWeight
Check "у партии пары поля веса нет вовсе" 0 $batchHasWeight

$batchesPerEpoch = [Math]::Ceiling(50 / [int]$miniBatch)
Check "мини-партий за эпоху" 10 $batchesPerEpoch
Check "остаток 50 по модулю размера мини-партии" 0 (50 % [int]$miniBatch)
Write-Output "  Вывод о достижимости. Обучающая партия обязана содержать ровно 50"
Write-Output "  строк, отбора при токенизации нет, размер мини-партии 5, значит"
Write-Output "  ровно 10 мини-партий по 5 пар и ни одной короткой. Отсюда: защита"
Write-Output "  Math.Max(1, …) НЕ срабатывает никогда; защита пустых сводок НЕ"
Write-Output "  срабатывает никогда; ранний возврат тренера на одной партии с этого"
Write-Output "  бегунка НЕ достигается. Все три — мёртвый защитный код, и ни один"
Write-Output "  из них не может подменить измерение числом."
Write-Output ""

# ── Панель 6. Второй способ на эталонах с известным ответом ────────────────
Write-Output "Панель 6. Сверка вторым способом на эталонах с заведомо известным ответом."

# Эталон А: все потери равны — ответ известен по построению.
$constant = @(1..50 | ForEach-Object { 0.42 })
$constantSummaries = Build-Summaries $constant 5
$byTransfer = Invoke-RunnerAggregation $constantSummaries
# Второй способ: простое накопление без группировки вовсе.
$byPlainLoop = 0.0
foreach ($value in $constant) { $byPlainLoop += $value }
$byPlainLoop = $byPlainLoop / $constant.Count
Write-Output ("  эталон А (все потери 0,42): переносом " + $byTransfer.ToString('F15') + " | накоплением " + $byPlainLoop.ToString('F15'))
Check "  эталон А: перенос даёт известный ответ" $true ([Math]::Abs($byTransfer - 0.42) -lt 1e-12)
Check "  эталон А: два способа согласны" $true ([Math]::Abs($byTransfer - $byPlainLoop) -lt 1e-12)

# Эталон Б: арифметическая прогрессия — среднее известно аналитически.
$progression = @(0..49 | ForEach-Object { 1.0 + $_ })
$expectedMean = (1.0 + 50.0) / 2.0
$progressionSummaries = Build-Summaries $progression 5
$byTransferB = Invoke-RunnerAggregation $progressionSummaries
$byPlainLoopB = 0.0
foreach ($value in $progression) { $byPlainLoopB += $value }
$byPlainLoopB = $byPlainLoopB / $progression.Count
Write-Output ("  эталон Б (1..50): переносом " + $byTransferB.ToString('F15') + " | аналитически " + $expectedMean.ToString('F15'))
Check "  эталон Б: перенос даёт аналитический ответ" $true ([Math]::Abs($byTransferB - $expectedMean) -lt 1e-12)
Check "  эталон Б: два способа согласны" $true ([Math]::Abs($byTransferB - $byPlainLoopB) -lt 1e-12)
Write-Output ""

# ── Панель 7. Сохранённое красное: что упало в первом прогоне и почему ─────
Write-Output "Панель 7. Сохранённое красное: первый прогон этой программы дал «сверок 25, упало 2»."
Write-Output "  Красное не стирается переписыванием. Оба падения были в ЭТОЙ программе,"
Write-Output "  а не в боевом коде, и причины установлены замером, а не догадкой."
Write-Output ""
Write-Output "  Падение 1. «порча 2 (взвешенное делённое на невзвешенный счёт) поймана»:"
Write-Output "  ожидалось True, получено False; порченое 1,025000000 против правильного"
Write-Output "  1,025000000 — в точности то же число. Причина: вес был взят как"
Write-Output "  0,5 плюс остаток индекса по модулю 5, умноженный на 0,25. Такой вес"
Write-Output "  периодичен с периодом 5, равным размеру мини-партии, поэтому сумма"
Write-Output "  весов в каждой мини-партии выходила ровно 5, средний вес ровно"
Write-Output "  единица, и обе формулы совпадали тождественно. Эталон был выроженным."
Write-Output "  Условие неотличимости установлено панелью 3b и опровергает гипотезу"
Write-Output "  про равные размеры мини-партий."
Write-Output ""
Write-Output "  Падение 2. «сумма через долю загрузки равна прямой сумме процессорного"
Write-Output "  времени»: ожидалось True, получено False; 447,840000000 против"
Write-Output "  450,000000000, остаток 2,16. Причина: я написал в переносе"
Write-Output "  [Math]::Max(0, <дробное>), как в боевом коде, но оболочка разрешает эту"
Write-Output "  перегрузку иначе, чем язык продукта: она выбирает целочисленную"
Write-Output "  перегрузку и ОКРУГЛЯЕТ. Проверка арифметикой ниже воспроизводит"
Write-Output "  наблюдённое число точно."
# Воспроизведение остатка первого прогона: округление доли загрузки до целого.
$firstRunReproduction = 0.0
foreach ($observation in $batchObservations) {
    $percentDouble = $observation.CpuDeltaMs / $observation.ElapsedMs / [Math]::Max(1, $processorCount) * 100.0
    # Ровно то, что делала оболочка: целочисленная перегрузка с округлением.
    $percentRounded = [Math]::Max(0, $percentDouble)
    $firstRunReproduction += $percentRounded / 100.0 * $observation.ElapsedMs * [Math]::Max(1, $processorCount)
}
Check "остаток первого прогона воспроизводится округлением доли загрузки" $true ([Math]::Abs($firstRunReproduction - 447.84) -lt 1e-9)
Write-Output ("      воспроизведено: " + $firstRunReproduction.ToString('F9') + " — ровно наблюдённые 447,840000000")
Write-Output "  Отсюда правило, которое дороже самого случая: дословный перенос в"
Write-Output "  другой язык дословным НЕ является, когда разрешение перегрузок"
Write-Output "  различается. Это пятый случай одного класса за эти дни: идиома"
Write-Output "  законна, разбор молчит, ответ неверен."
Write-Output ""
Write-Output "  Что из первого прогона НЕ отменено: вывод по сути. Панели 1 и 2"
Write-Output "  проходили и тогда, и сейчас, и именно они опровергают подозрение."
Write-Output ""

Write-Output "Бюджет этого прогона."
Write-Output "  сборок проекта: 0; прогонов площадки проверок: 0; обучения: 0; проходов модели: 0"
Write-Output "  арифметика дословного переноса и разбор текста исходников"
Write-Output ""
Write-Output "Чем это НЕ доказано."
Write-Output "  Обучение не запускалось, настоящие числа прогона не снимались:"
Write-Output "  доказана АРИФМЕТИКА свёртки, а не качество обученной модели."
Write-Output "  Родная приёмка платформы не исполнялась: NOT_EXECUTED, а не PASS."
Write-Output "  Эта программа НЕ проверяет, что поля с именем «final» означают"
Write-Output "  конечное состояние обучения: они означают среднее по всем шагам"
Write-Output "  всех эпох, и это отдельная находка, вынесенная строкой работ, а не"
Write-Output "  закрытая здесь. Утверждается ровно одно: смешивания масштабов в"
Write-Output "  свёртке показателей по парам НЕТ, и сверка, это показавшая,"
Write-Output "  чувствительна к настоящему смешиванию."
Write-Output ""

if ($failures -eq 0) {
    Write-Output ("ИТОГ: все {0} сверок прошли. Подозрение на смешивание масштабов ОПРОВЕРГНУТО. Обе внесённые порчи пойманы на невырожденном наборе; условие, при котором порча 2 НЕОТЛИЧИМА, названо и измерено (сумма весов равна числу пар). Первый прогон этой программы дал «сверок 25, упало 2»: оба падения были в ней самой, их причины воспроизведены числом в панели 7." -f $checks)
    exit 0
} else {
    Write-Output ("ИТОГ: сверок {0}, упало {1}." -f $checks, $failures)
    exit 1
}
