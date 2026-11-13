# Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
# Licensed under GNU Affero General Public License v3.0.
# SPDX-License-Identifier: AGPL-3.0-only
# See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
#
# Проверяемая программа: является ли приведение номера шага состояния
# случайности к нулю подменой измерения
# (`14.ContinuousLearning/Training/DpoTrainingExecution.cs`, строки 342 и 663).
#
# Проверяемое подозрение: ноль правдоподобен как настоящий номер шага, поэтому
# отсутствие от нуля неотличимо, и разведение необходимо.
#
# Требование к этой программе то же, что и к предыдущим: она обязана быть
# способна опровергнуть собственный вывод. Поэтому в ней есть панели
# чувствительности, которые портят канонический код отсутствия и требуют, чтобы
# читатель порчу ПОЙМАЛ, и панель, которая проверяет, что было бы с читателем
# после предлагаемой правки.
#
# Панели:
#   1. Перечень охранников: отвергается ли шаг ноль как настоящий.
#   2. Канонический код отсутствия: что именно требует читатель артефакта.
#   3. Пять приведений на месте 342 дают ровно этот код, и это проверяется.
#   4. Что стало бы с читателем, если сделать номер шага необязательным.
#   5. Чувствительность: порча канонического кода обязана быть поймана.
#   6. Связь двух объектов на первом месте построения: охранник её держит.
#   7. Сверка вторым способом на эталонах с заведомо известным ответом.
#
# Бюджет: сборок проекта нет, прогонов площадки проверок нет, обучения нет.
# Только разбор текста исходников и дословный перенос условий читателя.

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
function Read-Source([string]$relative) {
    return [System.IO.File]::ReadAllText((Join-Path $repoRoot ($relative -replace '/', '\')))
}

$execution = Read-Source '14.ContinuousLearning/Training/DpoTrainingExecution.cs'
$serverExecutor = Read-Source '14.ContinuousLearning/Training/CodeDocumentationServerOwnedDpoTrainingExecutor.cs'
$manifest = Read-Source '14.ContinuousLearning/Training/CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifest.cs'
$randomFile = Read-Source '14.ContinuousLearning/Training/RepositoryOwnedDpoRandomStateFile.cs'
$capability = Read-Source '18.CapabilityTraining/LlmIngredientLogicalUnits.cs'
$coordinator = Read-Source '26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.cs'
$model = Read-Source '03.TransformerModel/LlmModel.cs'

Write-Output "Проверяемая программа: ноль как номер шага состояния случайности — подмена или код отсутствия."
Write-Output ""

# ── Панель 1. Отвергается ли шаг ноль как настоящий ────────────────────────
Write-Output "Панель 1. Может ли шаг ноль быть НАСТОЯЩИМ: перечень охранников."
$rejectNonPositive = @(
    @{ where = 'файл состояния случайности, проверка ожидаемого шага'; source = $randomFile; pattern = 'expectedRandomStateStep <= 0' },
    @{ where = 'файл состояния случайности, чтение шага из потока';    source = $randomFile; pattern = 'step <= 0 \|\| step != expectedRandomStateStep' },
    @{ where = 'файл состояния случайности, проверка снимка';          source = $randomFile; pattern = 'snapshot\.RandomStateStep <= 0' },
    @{ where = 'единица обучения способностей';                        source = $capability; pattern = 'randomState\.RandomStateStep <= 0' },
    @{ where = 'серверный исполнитель';                                source = $serverExecutor; pattern = 'artifact\.RandomStateStep <= 0' },
    @{ where = 'координатор публикации';                               source = $coordinator; pattern = 'manifest\.RandomStateStep <= 0' }
)
$totalRejections = 0
foreach ($guard in $rejectNonPositive) {
    $count = ([regex]::Matches($guard.source, $guard.pattern)).Count
    $totalRejections += $count
    Write-Output ("      отвергает шаг не больше нуля: " + $guard.where + " — мест " + $count)
}
Check "мест, отвергающих шаг не больше нуля, найдено" 7 $totalRejections
# Единственное место, где допускается ровно ноль: восстановление в модели.
$modelAllowsZero = ([regex]::Matches($model, 'snapshot\.RandomStateStep < 0')).Count
Check "в восстановлении модели охранник слабее: отвергает только отрицательное" 1 $modelAllowsZero
Write-Output "  Вывод. Для ПРИСУТСТВУЮЩЕГО состояния случайности шаг обязан быть"
Write-Output "  строго положительным: это требуют семь мест, включая запись и чтение"
Write-Output "  файла состояния, единицу обучения, серверного исполнителя и"
Write-Output "  координатора публикации. Единственный более слабый охранник — в"
Write-Output "  восстановлении модели, он отвергает лишь отрицательное; но снимок с"
Write-Output "  нулевым шагом не может прийти с пути файла состояния, потому что тот"
Write-Output "  отвергает не больше нуля в трёх местах. Значит ноль НЕ является"
Write-Output "  правдоподобным настоящим номером шага — подозрение в этой части"
Write-Output "  неверно."
Write-Output ""

# ── Панель 2. Канонический код отсутствия: дословный перенос читателя ──────
Write-Output "Панель 2. Канонический код отсутствия: что именно требует читатель артефакта."

function Test-ServerExecutorAbsence($artifact) {
    # Дословно из серверного исполнителя: отсутствие принимается только как
    # КОНЪЮНКЦИЯ, в которой ноль шага — обязательная часть, а не единственная.
    return ($null -eq $artifact.PostStepRandomState) -and
           [string]::IsNullOrEmpty($artifact.RandomStatePath) -and
           [string]::IsNullOrEmpty($artifact.RandomStateSha256) -and
           ($artifact.RandomStateByteLength -eq 0) -and
           ($artifact.RandomStateStep -eq 0)
}
function Test-ServerExecutorPresence($artifact) {
    # Дословно из серверного исполнителя: присутствие требует шага строго больше нуля.
    return -not (($null -eq $artifact.PostStepRandomState) -or
                 ($artifact.RandomStateStep -le 0) -or
                 [string]::IsNullOrWhiteSpace($artifact.RandomStatePath) -or
                 [string]::IsNullOrWhiteSpace($artifact.RandomStateSha256) -or
                 ($artifact.RandomStateByteLength -le 0))
}

$absenceConjunction = ([regex]::Matches($serverExecutor, 'artifact\.RandomStateStep == 0')).Count
$manifestAbsence = ([regex]::Matches($manifest, 'manifest\.RandomStateStep == 0')).Count
Check "серверный исполнитель читает ноль шага как часть кода отсутствия" 1 $absenceConjunction
Check "опись состояния читает ноль шага как часть кода отсутствия" 1 $manifestAbsence
Write-Output "  Отсюда главное: ноль шага в этом артефакте — не «неизвестно вместо"
Write-Output "  измерения», а ОБЯЗАТЕЛЬНАЯ ЧАСТЬ канонического кода «состояния"
Write-Output "  случайности нет». Читатель принимает отсутствие только когда пусты"
Write-Output "  путь и свёртка, нулевые длина и шаг, и нет снимка. Любая иная"
Write-Output "  комбинация отвергается с отказом о неожидаемом состоянии."
Write-Output ""

# ── Панель 3. Пять приведений дают ровно этот код ──────────────────────────
Write-Output "Панель 3. Семь приведений на каждом месте построения дают ровно канонический код."
# Счёт по ОБРАЗЦУ здесь ненадёжен: короткое имя переменной второго места
# является подстрокой длинного имени первого, и образец считает оба. Поймано
# прогоном: ожидалось 5, получено 6. Поэтому проверка идёт по СТРОКАМ с
# точными номерами — это однозначно и локализует расхождение.
$executionLines = $execution -split "`n"
function Get-TrimmedLine([int]$number) {
    return $executionLines[$number - 1].Trim()
}
$site1 = @(
    @{ line = 339; text = 'preparedRandomState?.Path ?? "",' },
    @{ line = 340; text = 'preparedRandomState?.Sha256 ?? "",' },
    @{ line = 341; text = 'preparedRandomState?.ByteLength ?? 0,' },
    @{ line = 342; text = 'preparedRandomState?.State.RandomStateStep ?? 0,' },
    @{ line = 343; text = 'preparedRandomState is null' },
    @{ line = 347; text = 'preparedRandomState?.State.TopologySha256 ?? "",' },
    @{ line = 348; text = 'preparedRandomState?.State.LoraConfigurationSha256 ?? "",' }
)
$site2 = @(
    @{ line = 660; text = 'randomState?.Path ?? "",' },
    @{ line = 661; text = 'randomState?.Sha256 ?? "",' },
    @{ line = 662; text = 'randomState?.ByteLength ?? 0,' },
    @{ line = 663; text = 'randomState?.State.RandomStateStep ?? 0,' },
    @{ line = 664; text = 'randomState is null ? 0 : checked((int)randomState.State.FormatVersion),' },
    @{ line = 666; text = 'randomState?.State.TopologySha256 ?? "",' },
    @{ line = 667; text = 'randomState?.State.LoraConfigurationSha256 ?? "",' }
)
$site1Matched = 0
foreach ($row in $site1) { if ((Get-TrimmedLine $row.line) -eq $row.text) { $site1Matched++ } else { Write-Output ("      РАСХОЖДЕНИЕ строка " + $row.line + ": " + (Get-TrimmedLine $row.line)) } }
$site2Matched = 0
foreach ($row in $site2) { if ((Get-TrimmedLine $row.line) -eq $row.text) { $site2Matched++ } else { Write-Output ("      РАСХОЖДЕНИЕ строка " + $row.line + ": " + (Get-TrimmedLine $row.line)) } }
Check "первое место построения: семь приведённых полей на точных строках" 7 $site1Matched
Check "второе место построения: семь приведённых полей на точных строках" 7 $site2Matched
Write-Output "      первое место: строки 339-343, 347-348; второе: 660-664, 666-667"
Write-Output "      целевые строки наряда — 342 и 663 — входят в эти группы четвёртыми"
# Артефакт отсутствия, собранный ровно этими приведениями.
$absentArtifact = [pscustomobject]@{
    PostStepRandomState = $null
    RandomStatePath = ""
    RandomStateSha256 = ""
    RandomStateByteLength = 0
    RandomStateStep = 0
    RandomStateFormatVersion = 0
}
Check "собранный приведениями артефакт принимается читателем как отсутствие" $true (Test-ServerExecutorAbsence $absentArtifact)
Check "и он же НЕ принимается как присутствие" $false (Test-ServerExecutorPresence $absentArtifact)

# Опись состояния проверяет КОНЪЮНКЦИЮ ШИРЕ, чем серверный исполнитель: в неё
# входят и версия формата, и обе свёртки — ровно те два поля, которые я забыл
# при первом перечислении. Значит забытое не ослабляет вывод, а усиливает его:
# все семь приведённых полей входят в канонический код отсутствия.
function Test-ManifestAbsence($manifestCandidate) {
    # Дословно из описи состояния оптимизатора: код отсутствия.
    return (-not $manifestCandidate.RandomStateRequired) -and
           ($manifestCandidate.RandomStateStep -eq 0) -and
           [string]::IsNullOrEmpty($manifestCandidate.RandomStateRelativePath) -and
           [string]::IsNullOrEmpty($manifestCandidate.RandomStateSha256) -and
           ($manifestCandidate.RandomStateByteLength -eq 0) -and
           ($manifestCandidate.RandomStateFormatVersion -eq 0) -and
           [string]::IsNullOrEmpty($manifestCandidate.RandomStateTopologySha256) -and
           [string]::IsNullOrEmpty($manifestCandidate.LoraConfigurationSha256)
}
$manifestFieldsInAbsenceCode = @(
    'manifest\.RandomStateStep == 0',
    'string\.IsNullOrEmpty\(manifest\.RandomStateRelativePath\)',
    'string\.IsNullOrEmpty\(manifest\.RandomStateSha256\)',
    'manifest\.RandomStateByteLength == 0',
    'manifest\.RandomStateFormatVersion == 0',
    'string\.IsNullOrEmpty\(manifest\.RandomStateTopologySha256\)',
    'string\.IsNullOrEmpty\(manifest\.LoraConfigurationSha256\)'
)
$manifestCovered = 0
foreach ($pattern in $manifestFieldsInAbsenceCode) {
    if (([regex]::Matches($manifest, $pattern)).Count -ge 1) { $manifestCovered++ }
}
Check "опись проверяет в коде отсутствия все семь приведённых полей" 7 $manifestCovered
$absentManifest = [pscustomobject]@{
    RandomStateRequired = $false
    RandomStateStep = 0
    RandomStateRelativePath = ""
    RandomStateSha256 = ""
    RandomStateByteLength = 0
    RandomStateFormatVersion = 0
    RandomStateTopologySha256 = ""
    LoraConfigurationSha256 = ""
}
Check "артефакт отсутствия проходит и широкую конъюнкцию описи" $true (Test-ManifestAbsence $absentManifest)
Check "он же с ненулевым шагом конъюнкцию описи НЕ проходит" $false (Test-ManifestAbsence ($absentManifest | Select-Object * | ForEach-Object { $_.RandomStateStep = 3; $_ }))

Write-Output "  Группа приведений когерентна: путь и три свёртки пусты, длина, шаг и"
Write-Output "  версия нулевые — семь полей. Это ровно та конъюнкция, которую ждёт"
Write-Output "  опись состояния; серверный исполнитель проверяет её более узкую"
Write-Output "  часть из пяти полей. Две свёртки, забытые мной при первом"
Write-Output "  перечислении, входят в код отсутствия наравне с остальными, поэтому"
Write-Output "  обнаружение шестого и седьмого поля вывод НЕ сужает, а подтверждает"
Write-Output "  его на всех семи."
Write-Output ""

# ── Панель 4. Что стало бы с читателем после предлагаемой правки ───────────
Write-Output "Панель 4. Что стало бы, если сделать номер шага НЕОБЯЗАТЕЛЬНЫМ."
# После такой правки отсутствие пришло бы как пустое значение, а не как ноль.
$nullableArtifact = [pscustomobject]@{
    PostStepRandomState = $null
    RandomStatePath = ""
    RandomStateSha256 = ""
    RandomStateByteLength = 0
    RandomStateStep = $null
    RandomStateFormatVersion = 0
}
$absenceAfterFix = Test-ServerExecutorAbsence $nullableArtifact
Check "после правки читатель перестал бы принимать отсутствие" $false $absenceAfterFix
Check "и присутствием это тоже не стало бы" $false (Test-ServerExecutorPresence $nullableArtifact)
Write-Output "  Вывод, который переворачивает постановку задачи. Приведение к нулю"
Write-Output "  здесь не безвредно — оно НЕСУЩЕЕ. Сделай номер шага необязательным,"
Write-Output "  и конъюнкция отсутствия перестанет выполняться: артефакт без"
Write-Output "  состояния случайности будет отвергнут отказом о неожидаемом"
Write-Output "  состоянии, хотя никакого состояния в нём нет. То есть предлагаемая"
Write-Output "  правка не устранила бы ложь, а СЛОМАЛА бы правдивый путь."
Write-Output "  Поэтому строка работ закрывается как ложная тревога, а не отложенная."
Write-Output ""

# ── Панель 5. Чувствительность: порча кода отсутствия обязана быть поймана ─
Write-Output "Панель 5. Чувствительность: порча канонического кода обязана быть поймана."
$corruptions = @(
    @{ name = 'ноль шага, но путь непустой';        patch = @{ RandomStatePath = 'rng/step-000001.bin' } },
    @{ name = 'ноль шага, но длина непустая';       patch = @{ RandomStateByteLength = 128 } },
    @{ name = 'пустой путь, но шаг положительный';  patch = @{ RandomStateStep = 7 } },
    @{ name = 'ноль шага, но свёртка непустая';     patch = @{ RandomStateSha256 = 'a' * 64 } }
)
foreach ($corruption in $corruptions) {
    $candidate = [pscustomobject]@{
        PostStepRandomState = $null
        RandomStatePath = ""
        RandomStateSha256 = ""
        RandomStateByteLength = 0
        RandomStateStep = 0
        RandomStateFormatVersion = 0
    }
    foreach ($key in $corruption.patch.Keys) { $candidate.$key = $corruption.patch[$key] }
    $acceptedAsAbsent = Test-ServerExecutorAbsence $candidate
    $acceptedAsPresent = Test-ServerExecutorPresence $candidate
    Write-Output ("      " + $corruption.name + ": как отсутствие " + $acceptedAsAbsent + ", как присутствие " + $acceptedAsPresent)
    Check ("  порча «" + $corruption.name + "» не принята отсутствием") $false $acceptedAsAbsent
    Check ("  порча «" + $corruption.name + "» не принята присутствием") $false $acceptedAsPresent
}
Write-Output "  Все четыре порчи отвергнуты обеими ветвями читателя, то есть"
Write-Output "  приводят к отказу, а не к молчаливому приёму. Значит перенос"
Write-Output "  условий читателя не вырожден: он различает канонический код от"
Write-Output "  похожих на него, и зелёный результат панели 3 что-то значит."
Write-Output ""

# ── Панель 6. Связь двух объектов на первом месте построения ───────────────
Write-Output "Панель 6. Признак присутствия и приведённые поля описывают РАЗНЫЕ объекты."
$flagFromPostStep = ([regex]::Matches($execution, 'output\.PostStepRandomState is not null,')).Count
# Шесть условных обращений к подготовленному состоянию: путь, свёртка, длина,
# номер шага, свёртка топологии, свёртка настройки низкоранговых поправок.
# Седьмое приведение того же места — версия формата через `is null ? 0`.
# Прежнее ожидание «четыре» было неверным: я перечислял по памяти разговора, а
# не по файлу, и забыл две свёртки на строках 347-348.
$fieldsFromPrepared = ([regex]::Matches($execution, 'preparedRandomState\?\.')).Count
# Образец `preparedRandomState is null` совпадает ДВАЖДЫ и с разными
# конструкциями: строка 343 — приведение версии формата, строка 296 — охранник
# согласованности. Поэтому они считаются по отдельности, а не одним образцом:
# образец, совпадающий с двумя разными конструкциями, нельзя использовать для
# счёта одной из них. Это причина упавшей сверки первой редакции.
$nullTests = ([regex]::Matches($execution, 'preparedRandomState is null')).Count
$consistencyGuard = ([regex]::Matches($execution, 'output\.PostStepRandomState is not null &&\s*\r?\n\s*preparedRandomState is null')).Count
$formatVersionCoercion = if ((Get-TrimmedLine 343) -eq 'preparedRandomState is null') { 1 } else { 0 }
Check "на первом месте признак берётся от состояния ПОСЛЕ шага" 1 $flagFromPostStep
Check "условных обращений к подготовленному состоянию" 6 $fieldsFromPrepared
Check "проверок на отсутствие подготовленного состояния всего" 2 $nullTests
Check "из них охранник согласованности" 1 $consistencyGuard
Check "из них приведение версии формата на строке 343" 1 $formatVersionCoercion
Check "итого приведённых полей первого места: шесть условных плюс версия формата" 7 ($fieldsFromPrepared + $formatVersionCoercion)
$flagFromSameObject = if ((Get-TrimmedLine 665) -eq 'randomState is not null,') { 1 } else { 0 }
Check "на втором месте признак берётся от того же объекта, что поля" 1 $flagFromSameObject
Write-Output "  Это поправка к моей же прежней записи. Я писал, что «рядом стоит"
Write-Output "  отдельный признак присутствия». Точная правда иная: поле названо"
Write-Output "  «требуется», а не «присутствует», и на ПЕРВОМ месте построения оно"
Write-Output "  вычисляется от состояния ПОСЛЕ шага, тогда как все СЕМЬ приведённых"
Write-Output "  полей описывают ПОДГОТОВЛЕННОЕ состояние — другой объект. На втором"
Write-Output "  месте признак и поля берутся от одного объекта."
Write-Output "  Формулировка для строки работ: не «не проверено направление», а"
Write-Output "  «два места построения связывают признак с данными ПО-РАЗНОМУ, и на"
Write-Output "  одном из них это держится охранником»."
Write-Output "  Опасное расхождение закрыто охранником: возврат состояния после шага"
Write-Output "  без подготовленного обратного чтения отвергается отказом о неполном"
Write-Output "  состоянии случайности. Значит «требуется истинно при нулевых полях»"
Write-Output "  невозможно. Обратное направление — подготовленное есть, после шага"
Write-Output "  нет — охранником не закрыто, но и оно не молчит: читатель отвергнет"
Write-Output "  такой артефакт, потому что ни одна из двух ветвей не выполнится."
Write-Output "  Это отдельный фактор согласованности артефакта, а не приведение к"
Write-Output "  нулю, и он вынесен строкой работ, а не починен здесь."
Write-Output ""

# ── Панель 7. Второй способ на эталонах с известным ответом ────────────────
Write-Output "Панель 7. Сверка вторым способом на эталонах с заведомо известным ответом."
# Второй способ: вместо конъюнкции по полям — счёт «непустых признаков».
function Measure-NonEmptySignals($artifact) {
    $count = 0
    if ($null -ne $artifact.PostStepRandomState) { $count++ }
    if (-not [string]::IsNullOrEmpty($artifact.RandomStatePath)) { $count++ }
    if (-not [string]::IsNullOrEmpty($artifact.RandomStateSha256)) { $count++ }
    if ($artifact.RandomStateByteLength -ne 0) { $count++ }
    if ($artifact.RandomStateStep -ne 0) { $count++ }
    return $count
}
$references = @(
    @{ name = 'полное отсутствие (ответ известен: отсутствие)'; artifact = [pscustomobject]@{ PostStepRandomState=$null; RandomStatePath=""; RandomStateSha256=""; RandomStateByteLength=0; RandomStateStep=0; RandomStateFormatVersion=0 }; expectedAbsent = $true; expectedSignals = 0 },
    @{ name = 'полное присутствие (ответ известен: присутствие)'; artifact = [pscustomobject]@{ PostStepRandomState='снимок'; RandomStatePath='rng/step-000002.bin'; RandomStateSha256=('b' * 64); RandomStateByteLength=256; RandomStateStep=2; RandomStateFormatVersion=1 }; expectedAbsent = $false; expectedSignals = 5 }
)
foreach ($reference in $references) {
    $byConjunction = Test-ServerExecutorAbsence $reference.artifact
    $bySignalCount = (Measure-NonEmptySignals $reference.artifact) -eq 0
    Write-Output ("  " + $reference.name)
    Write-Output ("      по конъюнкции полей: отсутствие=" + $byConjunction + " | по счёту непустых признаков: отсутствие=" + $bySignalCount)
    Check ("  " + $reference.name + " — по конъюнкции") $reference.expectedAbsent $byConjunction
    Check ("  " + $reference.name + " — по счёту признаков") $reference.expectedAbsent $bySignalCount
    Check ("  " + $reference.name + " — два способа согласны") $byConjunction $bySignalCount
    Check ("  " + $reference.name + " — число непустых признаков") $reference.expectedSignals (Measure-NonEmptySignals $reference.artifact)
}
Write-Output ""

# ── Панель 8. Сохранённое красное: что упало в первом прогоне и почему ─────
Write-Output "Панель 8. Сохранённое красное: первый прогон этой программы дал «сверок 29, упало 2»."
Write-Output "  Оба падения были в ЭТОЙ программе, в счётах, и оба нашли БОЛЬШЕ"
Write-Output "  ожидаемого. Это требовало ответа: вывод «ноль есть часть канонического"
Write-Output "  кода отсутствия» верен лишь если известны ВСЕ приведения места."
Write-Output ""
Write-Output "  Падение 1. «все пять приведений первого места построения на месте»:"
Write-Output "  ожидалось 5, получено 6. Шестого приведения НЕ существует: сверка"
Write-Output "  складывала счёты пяти образцов, и образец «preparedRandomState is"
Write-Output "  null» совпал ДВАЖДЫ с РАЗНЫМИ конструкциями — строка 343 это"
Write-Output "  приведение версии формата, строка 296 это охранник согласованности."
Write-Output "  То есть я посчитал ПРОВЕРКУ как приведение. Правило: образец,"
Write-Output "  совпадающий с двумя разными конструкциями, нельзя использовать для"
Write-Output "  счёта одной из них."
Write-Output ""
Write-Output "  Падение 2. «приведённые поля берутся от подготовленного состояния»:"
Write-Output "  ожидалось 4, получено 6. Шесть полей это путь (339), свёртка (340),"
Write-Output "  длина (341), номер шага (342), свёртка топологии (347) и свёртка"
Write-Output "  настройки низкоранговых поправок (348). Я ожидал четыре, потому что"
Write-Output "  перечислял по памяти разговора, а не по файлу, и забыл две свёртки."
Write-Output ""
Write-Output "  Третья причина, глубже обеих: счёт по образцу на этих именах"
Write-Output "  НЕСОСТОЯТЕЛЕН в принципе, потому что короткое имя второго места"
Write-Output "  является ПОДСТРОКОЙ длинного имени первого. Любой счёт по короткому"
Write-Output "  имени молча включает вхождения длинного. Поэтому проверка переведена"
Write-Output "  на сверку по СТРОКАМ с точными номерами."
Write-Output ""
Write-Output "  Меняет ли это вывод: НЕТ, и усиливает его. Все семь приведений"
Write-Output "  одного класса — каждое отображает отсутствие в пустое или нулевое"
Write-Output "  значение своего типа, — и широкая конъюнкция описи состояния"
Write-Output "  проверяет ровно все семь. Значит упало не утверждение, а мои"
Write-Output "  ожидания, и они исправлены на измеренные числа."
Write-Output ""

Write-Output "Бюджет этого прогона."
Write-Output "  сборок проекта: 0; прогонов площадки проверок: 0; обучения: 0; проходов модели: 0"
Write-Output "  разбор текста исходников и дословный перенос двух ветвей читателя"
Write-Output ""
Write-Output "Чем это НЕ доказано."
Write-Output "  Обучение не запускалось, настоящий артефакт прогона не снимался:"
Write-Output "  доказан ДОГОВОР чтения артефакта, а не поведение обученной модели."
Write-Output "  Родная приёмка платформы не исполнялась: NOT_EXECUTED, а не PASS."
Write-Output "  Приёмочной проверки этот кусок не добавляет, и это не упущение:"
Write-Output "  правки нет, а проверка, которая проходила бы и до и после, приёмкой"
Write-Output "  не является. Эта программа НЕ проверяет направление, в котором"
Write-Output "  подготовленное состояние есть, а состояния после шага нет: оно"
Write-Output "  вынесено отдельной строкой работ. Утверждается ровно одно: ноль как"
Write-Output "  номер шага состояния случайности — не подмена измерения, а"
Write-Output "  обязательная часть канонического кода отсутствия, и правка здесь"
Write-Output "  сломала бы правдивый путь."
Write-Output ""

if ($failures -eq 0) {
    Write-Output ("ИТОГ: все {0} сверок прошли. Подозрение ОПРОВЕРГНУТО: ноль — код отсутствия, а не подмена; приведение несущее, и правка сломала бы читателя. Четыре внесённые порчи пойманы." -f $checks)
    exit 0
} else {
    Write-Output ("ИТОГ: сверок {0}, упало {1}." -f $checks, $failures)
    exit 1
}
