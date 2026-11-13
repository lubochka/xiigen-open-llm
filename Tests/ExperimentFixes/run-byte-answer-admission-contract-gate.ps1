# Прогон исправленного договора приёмки «в ответ не попадает то, что символом не
# является» (пункт M02a адресного плана; человеческое имя — «сначала исправить
# приёмку, потом генератор»).
#
# Зачем файл-скрипт: прямые команды сборки и прогона проверок запрещены настройками
# владельца, а правило построения команд требует вести длинную работу файлом рядом с
# доказательствами, запускать по пути файла и сохранять обычный вывод, поток ошибок и
# код выхода на диск.
#
# Соседние уже опубликованные скрипты не правятся: это отдельный скрипт по тому же
# образцу.
#
# Запуск только так:
#   pwsh -NoProfile -File Tests/ExperimentFixes/run-byte-answer-admission-contract-gate.ps1 -Label contract
#   pwsh -NoProfile -File Tests/ExperimentFixes/run-byte-answer-admission-contract-gate.ps1 -Label neighbour-generation-tests
#
# Без Start-Process, без циклов ожидания, без фонового запуска, без полного прогона дерева.

param(
    # Метка прогона: попадает в имя файла доказательства и выбирает фильтр.
    # contract — исправленный договор приёмки. Положительные случаи и встречные
    #   контроли обязаны пройти; отрицательные случаи генератора обязаны УПАСТЬ,
    #   потому что состояния UTF-8 у генератора ещё нет (пункт M02b).
    # neighbour-generation-tests — соседние уже существующие проверки чтения ответа:
    #   их поведение меняться не должно.
    [Parameter(Mandatory = $true)]
    [ValidateSet('contract', 'neighbour-generation-tests')]
    [string]$Label
)

# Ошибки не должны молча прерывать скрипт: код выхода каждой команды проверяется
# вручную сразу после неё.
$ErrorActionPreference = 'Continue'

# Русский текст в окне запуска иначе превращается в знаки вопроса: кодовая страница
# консоли по умолчанию не совпадает с UTF-8.
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# Папка самого скрипта и корень дерева проекта на два уровня выше.
$scriptDir = $PSScriptRoot
$repoRoot = (Resolve-Path (Join-Path $scriptDir '..\..')).Path

# Доказательство остаётся на диске рядом со скриптом, а не только в окне агента.
$outPath = Join-Path $scriptDir ("run-byte-answer-admission-contract-gate-" + $Label + ".txt")
$blocks = New-Object System.Collections.Generic.List[string]

# Узкая область прогона: ровно один класс приёмки либо соседние проверки чтения
# ответа. Полный прогон дерева проверок здесь не заказан и не нужен.
if ($Label -eq 'neighbour-generation-tests') {
    $testFilter = 'FullyQualifiedName~OrdinaryGenerationDoesNotInventAnEndOfAnswerMarkerTests'
}
else {
    $testFilter = 'FullyQualifiedName~AnswerReadingRejectsIncompleteCharacterIdentifiersTests'
}

# Добавляет блок текста в будущий файл доказательства.
function Add-Block([string]$text) {
    $blocks.Add([string]$text)
}

# Пишет файл доказательства в кодировке UTF-8 без метки порядка байтов, с переводами
# строк в стиле LF и без пробелов в конце строк: таков формат доказательств проекта.
function Save-Output {
    $joined = ($blocks -join "`n")
    $normalized = $joined -replace "`r`n", "`n"
    $trimmed = ($normalized -split "`n" | ForEach-Object { $_.TrimEnd() }) -join "`n"
    $utf8NoBom = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::WriteAllText($outPath, ($trimmed + "`n"), $utf8NoBom)
}

Add-Block '=== Прогон договора: ответ читается целой последовательностью, а не по одному номеру ==='
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

Add-Block '--- Шаг 2 из 4: состояние допуска символа в генераторе и выборщике ---'
# Этот шаг печатает в доказательство сам код тех мест, где обязан появиться допуск
# по состоянию последовательности. Пока их нет — отрицательные случаи генератора
# красные по этой причине, и по сохранённому выводу это видно, а не домыслено.
$generatorPath = Join-Path $repoRoot '09.Inference\TextGenerator.cs'
$samplerPath = Join-Path $repoRoot '09.Inference\Sampler.cs'
$admissionHits = Select-String -LiteralPath $generatorPath, $samplerPath -Pattern 'Utf8Admission|AppendTokenBytes|CandidateStatus|VALID_COMPLETE|VALID_PREFIX'
if ($null -eq $admissionHits -or @($admissionHits).Count -eq 0) {
    Add-Block 'Состояния UTF-8 у генератора и выборщика НЕТ: совпадений по именам допуска ноль.'
    Add-Block 'Значит отрицательные случаи генератора обязаны быть красными (ожидает пункта M02b).'
}
else {
    Add-Block (($admissionHits | ForEach-Object { $_.Filename + ':' + $_.LineNumber + ': ' + $_.Line.Trim() } | Out-String).TrimEnd())
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
    # Разделение чужой поломки и своей: ошибки группируются по файлу, чтобы сразу было
    # видно, относятся они к этой правке или нет.
    $errorLines = $buildOutput |
        Select-String -Pattern 'error [A-Z]+[0-9]+' |
        ForEach-Object { $_.Line.Trim() } |
        Sort-Object -Unique
    $mineFiles = 'AnswerReadingRejectsIncompleteCharacterIdentifiersTests'
    $mine = $errorLines | Where-Object { $_ -match $mineFiles }
    $foreign = $errorLines | Where-Object { $_ -notmatch $mineFiles }
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

# Короткая сводка в окно запуска: числа прогона и имена упавших проверок. Для метки
# contract ненулевой код выхода ожидаем: отрицательные случаи генератора красные.
$testSummary = $testOutput |
    Select-String -Pattern 'Passed!|Failed!|Skipped!|Passed:|Failed:|Total:|\[FAIL\]|error [A-Z]+[0-9]+' |
    ForEach-Object { $_.Line.Trim() } |
    Select-Object -First 80
Write-Output ('label=' + $Label)
Write-Output ('tests_build_exit=' + $buildExitCode)
Write-Output ('test_exit=' + $testExitCode)
Write-Output ('evidence_file=' + $outPath)
Write-Output '--- сводка прогона ---'
$testSummary | ForEach-Object { Write-Output $_ }

exit $testExitCode
