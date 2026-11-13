# Прогон приёмки «партия обучения перевода применяется целиком, и новая версия
# состояния публикуется целиком».
#
# Зачем файл-скрипт: прямые команды сборки и прогона проверок запрещены
# настройками владельца, а правило построения команд требует вести длинную
# работу файлом рядом с доказательствами, запускать её по пути файла и
# сохранять обычный вывод, поток ошибок и код выхода на диск.
#
# Запуск только так:
#   pwsh -NoProfile -File Tests/BatchAtomicPublicationGate/run-batch-atomic-publication-gate.ps1 -Label domain-build
#   pwsh -NoProfile -File Tests/BatchAtomicPublicationGate/run-batch-atomic-publication-gate.ps1 -Label after-fix
#   pwsh -NoProfile -File Tests/BatchAtomicPublicationGate/run-batch-atomic-publication-gate.ps1 -Label before-fix
#   pwsh -NoProfile -File Tests/BatchAtomicPublicationGate/run-batch-atomic-publication-gate.ps1 -Label naive-swap
#   pwsh -NoProfile -File Tests/BatchAtomicPublicationGate/run-batch-atomic-publication-gate.ps1 -Label neighbours
#
# Без Start-Process, без циклов ожидания, без фонового пуска.

param(
    # Метка прогона: попадает в имя файла доказательства и выбирает область.
    # domain-build — только сборка изменённого домена перевода: быстрый круг правки.
    # after-fix — новые проверки на ИСПРАВЛЕННОМ продуктовом коде: обязаны пройти.
    # before-fix — те же новые проверки на ПРЕЖНЕЙ логике продукта: обязаны упасть,
    #   иначе они ничего не доказывают.
    # naive-swap — те же проверки на наивной правке, где новая версия подменяется
    #   без решателя версии у дирижёра: условие «прежний дирижёр видит новую
    #   версию» обязано упасть именно на ней.
    # neighbours — соседние уже принятые проверки домена перевода и чужая приёмка
    #   отката партии: их поведение меняться не должно.
    [Parameter(Mandatory = $true)]
    [ValidateSet('domain-build', 'after-fix', 'before-fix', 'naive-swap', 'neighbours')]
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
$outPath = Join-Path $scriptDir ("run-batch-atomic-publication-gate-" + $Label + ".txt")
$blocks = New-Object System.Collections.Generic.List[string]

# Узкая область прогона. Полный прогон дерева проверок здесь не заказан и не нужен.
if ($Label -eq 'neighbours') {
    $testFilter = 'FullyQualifiedName~TranslationBatchTypeMismatchRollbackTests|FullyQualifiedName~TranslationBatchJsonLearningTests|FullyQualifiedName~TranslationDomainTests|FullyQualifiedName~SimulationEnRuContinuationTests'
}
else {
    $testFilter = 'FullyQualifiedName~TranslationBatchAtomicVersionPublicationTests'
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

Add-Block '=== Прогон приёмки атомарной партии и согласованной публикации версии ==='
Add-Block ('Метка прогона: ' + $Label)
Add-Block ('Начало (всемирное время): ' + (Get-Date).ToUniversalTime().ToString('o'))
Add-Block ('Корень дерева: ' + $repoRoot)
Add-Block ('Фильтр проверок: ' + $testFilter)
Add-Block ''

# Все команды сборки и прогона выполняются из корня дерева.
Set-Location $repoRoot

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

Add-Block '--- Шаг 2 из 5: состояние правила в машине перевода на этом прогоне ---'
# Этот шаг печатает сами ключевые строки продукта в файл доказательства, чтобы
# по сохранённому выводу было видно, какая логика стояла в прогоне: прежняя
# правка по живому состоянию или публикация целой новой версии.
$rulePath = Join-Path $repoRoot '27.TranslationDomain\TranslationMachine.cs'
$ruleLine = Select-String -LiteralPath $rulePath -Pattern 'BATCH_ROW_UNREADABLE|BATCH_ROW_TRAINING_FAILED|BATCH_STAGING_FAILED|StageRuntimeCopy|PublishVersion|_publishedVersion|StateVersionMetricKey|InlineTranslationOrchestrator\('
Add-Block (($ruleLine | ForEach-Object { $_.LineNumber.ToString() + ': ' + $_.Line.Trim() } | Out-String).TrimEnd())
Add-Block ''

Add-Block '--- Шаг 3 из 5: сборка изменённого домена перевода ---'
# Этот шаг проверяет ровно правку продукта: если он зелёный, изменённая машина
# перевода компилируется независимо от состояния проекта проверок.
$domainBuildOutput = & dotnet build '27.TranslationDomain\27.TranslationDomain.csproj' -c Debug --nologo 2>&1
$domainBuildExitCode = $LASTEXITCODE
Add-Block (($domainBuildOutput | Out-String).TrimEnd())
Add-Block ('Код выхода сборки домена: ' + $domainBuildExitCode)
if ($domainBuildExitCode -ne 0) {
    # Это обычный первый круг правки, а не блокер прав: правим и прогоняем снова.
    Add-Block 'ОСТАНОВ: изменённый домен не компилируется, дальше идти нельзя.'
    Save-Output
    $domainErrors = $domainBuildOutput |
        Select-String -Pattern 'error [A-Z]+[0-9]+' |
        ForEach-Object { $_.Line.Trim() } |
        Sort-Object -Unique |
        Select-Object -First 25
    Write-Output ('domain_build_exit=' + $domainBuildExitCode)
    Write-Output '--- ошибки компиляции изменённого домена ---'
    $domainErrors | ForEach-Object { Write-Output $_ }
    exit $domainBuildExitCode
}
Add-Block ''

# Метка быстрого круга правки заканчивается здесь: проект проверок не собирается.
if ($Label -eq 'domain-build') {
    Add-Block '--- Шаги 4 и 5 пропущены: метка прогона просит только сборку домена ---'
    Add-Block ('Конец (всемирное время): ' + (Get-Date).ToUniversalTime().ToString('o'))
    Save-Output
    Write-Output ('label=' + $Label)
    Write-Output ('domain_build_exit=' + $domainBuildExitCode)
    Write-Output ('evidence_file=' + $outPath)
    exit 0
}

Add-Block '--- Шаг 4 из 5: сборка проекта проверок вместе с зависимостями ---'
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
    $mineFiles = 'TranslationBatchAtomicVersionPublication|TranslationMachine'
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
Save-Output

# Короткая сводка в окно запуска: сами числа прогона и имена упавших проверок.
# Для меток before-fix и naive-swap ненулевой код выхода — ОЖИДАЕМЫЙ результат,
# а не авария скрипта: новые проверки обязаны падать на той логике.
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

exit $testExitCode
