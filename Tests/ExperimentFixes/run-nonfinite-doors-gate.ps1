# Прогон проверки правки «дверь приёмки отказывает на неизмеренном числе»
# в модуле страниц знаний о проекте.
#
# Зачем файл-скрипт: прямые команды сборки и прогона тестов запрещены
# настройками проекта, а правило построения команд требует вести длинную работу
# файлом под папкой доказательств, запускать по пути файла и сохранять обычный
# вывод, поток ошибок и код выхода на диск.
#
# Запуск только так:
#   pwsh -NoProfile -File Tests/ExperimentFixes/run-nonfinite-doors-gate.ps1 -Label full-rule
#   pwsh -NoProfile -File Tests/ExperimentFixes/run-nonfinite-doors-gate.ps1 -Label weakened-rule
#   pwsh -NoProfile -File Tests/ExperimentFixes/run-nonfinite-doors-gate.ps1 -Label restored-rule
#   pwsh -NoProfile -File Tests/ExperimentFixes/run-nonfinite-doors-gate.ps1 -Label publication-gate-tests
#
# Без Start-Process, без циклов ожидания, без фонового запуска.

param(
    # Метка прогона: попадает в имя файла доказательства и выбирает фильтр.
    # full-rule — целевое правило измеренности на месте, проверки обязаны пройти.
    # weakened-rule — правило нарочно ослаблено (убрана проверка бесконечности),
    #   проверки обязаны упасть: это оракул порчи, доказательство чувствительности.
    # restored-rule — правило восстановлено, проверки снова обязаны пройти.
    # after-comment-fix — те же проверки после правки правдивости комментариев:
    #   текст менялся, поведение нет, поэтому проверки обязаны остаться зелёными.
    # publication-gate-tests — существующие проверки принятой двери публикации.
    [Parameter(Mandatory = $true)]
    [ValidateSet('full-rule', 'weakened-rule', 'restored-rule', 'after-comment-fix', 'publication-gate-tests')]
    [string]$Label
)

# Ошибки не должны молча прерывать скрипт: код выхода каждой команды
# проверяется вручную сразу после неё.
$ErrorActionPreference = 'Continue'

# Русский текст в окне запуска иначе превращается в знаки вопроса: кодовая
# страница консоли по умолчанию не совпадает с UTF-8.
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# Папка самого скрипта и корень дерева проекта на два уровня выше.
$scriptDir = $PSScriptRoot
$repoRoot = (Resolve-Path (Join-Path $scriptDir '..\..')).Path

# Доказательство остаётся на диске рядом со скриптом, а не только в окне агента.
$outPath = Join-Path $scriptDir ("run-nonfinite-doors-gate-" + $Label + ".txt")
$blocks = New-Object System.Collections.Generic.List[string]

# Узкая область прогона. Три первых метки проверяют ровно новый класс проверок
# общего правила измеренности; четвёртая — уже принятые проверки двери
# публикации, которые обязаны остаться зелёными. Полный прогон дерева здесь не
# заказан и не нужен.
if ($Label -eq 'publication-gate-tests') {
    $testFilter = 'FullyQualifiedName~ProjectWikiDpoFinalCheckpointMetricGateTests'
}
else {
    $testFilter = 'FullyQualifiedName~PublicationGateMeasuredRuleRejectsNonFiniteMetrics'
}

# Добавляет блок текста в будущий файл доказательства.
function Add-Block([string]$text) {
    $blocks.Add([string]$text)
}

# Пишет файл доказательства в кодировке UTF-8 без метки порядка байтов,
# с переводами строк в стиле LF и без пробелов в конце строк: таков формат
# доказательств, принятый в проекте.
function Save-Output {
    $joined = ($blocks -join "`n")
    $normalized = $joined -replace "`r`n", "`n"
    $trimmed = ($normalized -split "`n" | ForEach-Object { $_.TrimEnd() }) -join "`n"
    $utf8NoBom = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::WriteAllText($outPath, ($trimmed + "`n"), $utf8NoBom)
}

Add-Block '=== Прогон проверки отказа двери приёмки на неизмеренном числе ==='
Add-Block ('Метка прогона: ' + $Label)
Add-Block ('Начало (всемирное время): ' + (Get-Date).ToUniversalTime().ToString('o'))
Add-Block ('Корень дерева: ' + $repoRoot)
Add-Block ('Фильтр проверок: ' + $testFilter)
Add-Block ''

# Все команды сборки и прогона выполняются из корня дерева.
Set-Location $repoRoot

Add-Block '--- Шаг 1 из 4: версия набора средств разработки ---'
$sdkOutput = & dotnet --version 2>&1
$sdkExitCode = $LASTEXITCODE
Add-Block (($sdkOutput | Out-String).TrimEnd())
Add-Block ('Код выхода: ' + $sdkExitCode)
if ($sdkExitCode -ne 0) {
    Add-Block 'ОСТАНОВ: набор средств разработки недоступен, дальше идти нельзя.'
    Save-Output
    Write-Output ('sdk_exit=' + $sdkExitCode)
    exit $sdkExitCode
}
Add-Block ''

Add-Block '--- Шаг 2 из 4: сборка изменённого модуля страниц знаний о проекте ---'
# Этот шаг проверяет ровно правку продукта: если он зелёный, изменённые двери
# компилируются независимо от состояния проекта проверок.
$domainBuildOutput = & dotnet build '42.ProjectWikiDomain\42.ProjectWikiDomain.csproj' -c Debug --nologo 2>&1
$domainBuildExitCode = $LASTEXITCODE
Add-Block (($domainBuildOutput | Out-String).TrimEnd())
Add-Block ('Код выхода сборки модуля: ' + $domainBuildExitCode)
if ($domainBuildExitCode -ne 0) {
    # Это обычный первый круг правки, а не блокер прав: правим и прогоняем снова.
    Add-Block 'ОСТАНОВ: изменённый модуль не компилируется, дальше идти нельзя.'
    Save-Output
    $domainErrors = $domainBuildOutput |
        Select-String -Pattern 'error [A-Z]+[0-9]+' |
        ForEach-Object { $_.Line.Trim() } |
        Sort-Object -Unique |
        Select-Object -First 25
    Write-Output ('domain_build_exit=' + $domainBuildExitCode)
    Write-Output '--- ошибки компиляции изменённого модуля ---'
    $domainErrors | ForEach-Object { Write-Output $_ }
    exit $domainBuildExitCode
}
Add-Block ''

Add-Block '--- Шаг 3 из 4: сборка проекта проверок вместе с зависимостями ---'
$buildOutput = & dotnet build 'Tests\LlmFromScratch.Tests.csproj' -c Debug --nologo 2>&1
$buildExitCode = $LASTEXITCODE
Add-Block (($buildOutput | Out-String).TrimEnd())
Add-Block ('Код выхода сборки проекта проверок: ' + $buildExitCode)
if ($buildExitCode -ne 0) {
    Add-Block 'ОСТАНОВ: проект проверок не собрался, проверки не запускались.'
    Save-Output
    # Разделение чужой поломки и своей: ошибки группируются по файлу, чтобы
    # сразу было видно, относятся они к этой правке или нет.
    $errorLines = $buildOutput |
        Select-String -Pattern 'error [A-Z]+[0-9]+' |
        ForEach-Object { $_.Line.Trim() } |
        Sort-Object -Unique
    $mineFiles = 'PublicationGateMeasuredRuleRejectsNonFiniteMetricsTests|ProjectWikiDpoPublicationMetricGate|CodeTraceBuilderTrainingPipeline|NegativeDpoGeneratorTrainingPipeline|ProjectWikiQueryPlanningTrainingPipeline'
    $mine = $errorLines | Where-Object { $_ -match $mineFiles }
    $foreign = $errorLines | Where-Object { $_ -notmatch $mineFiles }
    Write-Output ('domain_build_exit=' + $domainBuildExitCode)
    Write-Output ('tests_build_exit=' + $buildExitCode)
    Write-Output ('errors_in_my_files=' + @($mine).Count)
    Write-Output ('errors_in_other_files=' + @($foreign).Count)
    Write-Output '--- ошибки в моих файлах ---'
    if (@($mine).Count -eq 0) { Write-Output '(нет)' }
    $mine | Select-Object -First 25 | ForEach-Object { Write-Output $_ }
    Write-Output '--- ошибки в чужих файлах (первые 5 уникальных) ---'
    $foreign | Select-Object -First 5 | ForEach-Object { Write-Output $_ }
    exit $buildExitCode
}
Add-Block ''

Add-Block '--- Шаг 4 из 4: прогон проверок узким фильтром ---'
$testOutput = & dotnet test 'Tests\LlmFromScratch.Tests.csproj' -c Debug --no-build --nologo --filter $testFilter 2>&1
$testExitCode = $LASTEXITCODE
Add-Block (($testOutput | Out-String).TrimEnd())
Add-Block ('Код выхода прогона проверок: ' + $testExitCode)
Add-Block ''
Add-Block ('Конец (всемирное время): ' + (Get-Date).ToUniversalTime().ToString('o'))
Save-Output

# Короткая сводка в окно запуска: сами числа прогона и имена упавших проверок.
$testSummary = $testOutput |
    Select-String -Pattern 'Passed!|Failed!|Skipped!|Passed:|Failed:|Total:|\[FAIL\]|error [A-Z]+[0-9]+' |
    ForEach-Object { $_.Line.Trim() } |
    Select-Object -First 60
Write-Output ('label=' + $Label)
Write-Output ('domain_build_exit=' + $domainBuildExitCode)
Write-Output ('tests_build_exit=' + $buildExitCode)
Write-Output ('test_exit=' + $testExitCode)
Write-Output ('evidence_file=' + $outPath)
Write-Output '--- сводка прогона ---'
$testSummary | ForEach-Object { Write-Output $_ }

exit $testExitCode
