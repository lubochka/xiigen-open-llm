# Прогон исправленного доказательства восстановления: отвечает сама загруженная
# машина (пункт M03c адресного плана; человеческое имя — «исправить доказательство
# восстановления»).
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
#   pwsh -NoProfile -File Tests/ExperimentFixes/run-restored-consumer-gate.ps1 -Label restored-consumer
#   pwsh -NoProfile -File Tests/ExperimentFixes/run-restored-consumer-gate.ps1 -Label broken-reader
#   pwsh -NoProfile -File Tests/ExperimentFixes/run-restored-consumer-gate.ps1 -Label translation-domain-neighbours
#
# Без Start-Process, без циклов ожидания, без фонового запуска, без полного прогона дерева.

param(
    # Метка прогона: попадает в имя файла доказательства и выбирает, что делать.
    # restored-consumer — продуктовый код как есть. Проверка хранения и поведенческая
    #   проверка загруженной машины обязаны пройти.
    # broken-reader — ОТРИЦАТЕЛЬНЫЙ КОНТРОЛЬ. Применение выбранной загруженной карты
    #   отключается, а её повторный экспорт сохраняется: поведенческая проверка обязана
    #   УПАСТЬ, проверка хранения обязана остаться зелёной. Исходник правится на время
    #   прогона в памяти скрипта и возвращается обратно безусловно.
    # translation-domain-neighbours — соседние проверки домена перевода: их поведение
    #   меняться не должно.
    [Parameter(Mandatory = $true)]
    [ValidateSet('restored-consumer', 'broken-reader', 'translation-domain-neighbours')]
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
$outPath = Join-Path $scriptDir ("run-restored-consumer-gate-" + $Label + ".txt")
$blocks = New-Object System.Collections.Generic.List[string]

# Узкая область прогона.
if ($Label -eq 'translation-domain-neighbours') {
    $testFilter = 'FullyQualifiedName~Translation&FullyQualifiedName!~TranslationPairTailSignature'
}
else {
    $testFilter = 'FullyQualifiedName~TranslationPairTailSignatureLearningTests'
}

# Файл и точная строка выбранной загруженной карты: быстрая память точных переводов
# исполнителя, восстановленная загрузкой. Её ЧИТАТЕЛЬ отключается в отрицательном
# контроле; её ЭКСПОРТ при этом не трогается вовсе.
$unitPath = Join-Path $repoRoot '27.TranslationDomain\TranslationTrainedUnits.cs'
$readerOriginal = '        if (_exactTopicSourceMap.TryGetValue(exactKey, out var exactTranslation))'
$readerBroken = '        if (_exactTopicSourceMap.TryGetValue(exactKey, out var exactTranslation) && false)'

# Исходное содержимое целевого файла держится в памяти скрипта: временных файлов и
# временных рабочих копий в дереве проекта не создаётся.
$originalText = $null

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

# Возвращает исходное содержимое целевого файла, если оно правилось.
function Restore-Source {
    if ($null -ne $script:originalText) {
        [System.IO.File]::WriteAllText($unitPath, $script:originalText)
        $script:originalText = $null
    }
}

Add-Block '=== Прогон доказательства: отвечает сама загруженная машина ==='
Add-Block ('Метка прогона: ' + $Label)
Add-Block ('Начало (всемирное время): ' + (Get-Date).ToUniversalTime().ToString('o'))
Add-Block ('Корень дерева: ' + $repoRoot)
Add-Block ('Фильтр проверок: ' + $testFilter)
Add-Block ''

# Все команды сборки и прогона выполняются из корня дерева.
Set-Location $repoRoot

try {
    Add-Block '--- Шаг 1 из 5: версия набора средств разработки ---'
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

    Add-Block '--- Шаг 2 из 5: состояние читателя и экспорта выбранной загруженной карты ---'
    # Этот шаг печатает в доказательство сам код читателя и экспорта. Без него нельзя
    # привязать «упало» и «прошло» к состоянию кода в этом конкретном прогоне.
    if ($Label -eq 'broken-reader') {
        $originalText = [System.IO.File]::ReadAllText($unitPath)
        if (-not $originalText.Contains($readerOriginal)) {
            Add-Block 'ОСТАНОВ: строка читателя выбранной карты не найдена, порча не применялась.'
            Add-Block ('Ожидалась строка: ' + $readerOriginal)
            Save-Output
            Write-Output 'reader_line_not_found=1'
            exit 2
        }
        # Применяется ровно одна порча: условие читателя никогда не выполняется.
        # Сама карта, её загрузка и её экспорт не меняются.
        $brokenText = $originalText.Replace($readerOriginal, $readerBroken)
        [System.IO.File]::WriteAllText($unitPath, $brokenText)
        Add-Block 'ПОРЧА ПРИМЕНЕНА: применение выбранной загруженной карты отключено.'
        Add-Block ('Было:  ' + $readerOriginal.Trim())
        Add-Block ('Стало: ' + $readerBroken.Trim())
        Add-Block 'Экспорт этой карты не правился: повторный экспорт обязан остаться прежним.'
    }
    else {
        Add-Block 'Порча не применялась: продуктовый код как есть.'
    }
    $readerHits = Select-String -LiteralPath $unitPath -Pattern '_exactTopicSourceMap'
    Add-Block (($readerHits | ForEach-Object { $_.LineNumber.ToString() + ': ' + $_.Line.Trim() } | Out-String).TrimEnd())
    Add-Block ''

    Add-Block '--- Шаг 3 из 5: сборка изменённого домена перевода ---'
    $domainBuildOutput = & dotnet build '27.TranslationDomain\27.TranslationDomain.csproj' -c Debug --nologo 2>&1
    $domainBuildExitCode = $LASTEXITCODE
    Add-Block (($domainBuildOutput | Out-String).TrimEnd())
    Add-Block ('Код выхода сборки домена: ' + $domainBuildExitCode)
    if ($domainBuildExitCode -ne 0) {
        Add-Block 'ОСТАНОВ: домен не компилируется, дальше идти нельзя.'
        Save-Output
        $domainErrors = $domainBuildOutput |
            Select-String -Pattern 'error [A-Z]+[0-9]+' |
            ForEach-Object { $_.Line.Trim() } |
            Sort-Object -Unique |
            Select-Object -First 25
        Write-Output ('domain_build_exit=' + $domainBuildExitCode)
        Write-Output '--- ошибки компиляции домена ---'
        $domainErrors | ForEach-Object { Write-Output $_ }
        exit $domainBuildExitCode
    }
    Add-Block ''

    Add-Block '--- Шаг 4 из 5: сборка проекта проверок вместе с зависимостями ---'
    $buildOutput = & dotnet build 'Tests\LlmFromScratch.Tests.csproj' -c Debug --nologo 2>&1
    $buildExitCode = $LASTEXITCODE
    Add-Block (($buildOutput | Out-String).TrimEnd())
    Add-Block ('Код выхода сборки проекта проверок: ' + $buildExitCode)
    if ($buildExitCode -ne 0) {
        Add-Block 'ОСТАНОВ: проект проверок не собрался, проверки не запускались.'
        Save-Output
        $errorLines = $buildOutput |
            Select-String -Pattern 'error [A-Z]+[0-9]+' |
            ForEach-Object { $_.Line.Trim() } |
            Sort-Object -Unique
        $mineFiles = 'TranslationPairTailSignatureLearningTests|TranslationTrainedUnits'
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

    Add-Block '--- Шаг 5 из 5: прогон проверок узким фильтром ---'
    $testOutput = & dotnet test 'Tests\LlmFromScratch.Tests.csproj' -c Debug --no-build --nologo --filter $testFilter 2>&1
    $testExitCode = $LASTEXITCODE
    Add-Block (($testOutput | Out-String).TrimEnd())
    Add-Block ('Код выхода прогона проверок: ' + $testExitCode)
    Add-Block ''
    Add-Block ('Конец (всемирное время): ' + (Get-Date).ToUniversalTime().ToString('o'))

    # Короткая сводка в окно запуска. Для метки broken-reader ненулевой код выхода —
    # это ожидаемый результат отрицательного контроля, а не авария скрипта.
    $testSummary = $testOutput |
        Select-String -Pattern 'Passed!|Failed!|Skipped!|Passed:|Failed:|Total:|\[FAIL\]|error [A-Z]+[0-9]+' |
        ForEach-Object { $_.Line.Trim() } |
        Select-Object -First 80
    Write-Output ('label=' + $Label)
    Write-Output ('domain_build_exit=' + $domainBuildExitCode)
    Write-Output ('tests_build_exit=' + $buildExitCode)
    Write-Output ('test_exit=' + $testExitCode)
    Write-Output ('evidence_file=' + $outPath)
    Write-Output '--- сводка прогона ---'
    $testSummary | ForEach-Object { Write-Output $_ }
}
finally {
    # Исходник возвращается безусловно: порча живёт только внутри этого прогона.
    Restore-Source
    # Доказательство пишется и при обычном окончании, и при любом обрыве.
    $restoredText = [System.IO.File]::ReadAllText($unitPath)
    $readerIsBack = $restoredText.Contains($readerOriginal)
    $breakIsGone = -not $restoredText.Contains($readerBroken)
    Add-Block ''
    Add-Block '--- Возврат исходника после прогона ---'
    Add-Block ('Строка читателя на месте: ' + $readerIsBack)
    Add-Block ('Порчи в файле нет: ' + $breakIsGone)
    Save-Output
    Write-Output ('reader_restored=' + $readerIsBack)
    Write-Output ('break_removed=' + $breakIsGone)
}

exit $testExitCode
