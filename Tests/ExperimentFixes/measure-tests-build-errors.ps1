# Измерение числа ошибок компиляции проекта тестов с разбором по владельцу файла.
#
# Зачем отдельный скрипт: нужно сравнимое «было / стало» вокруг одной правки в
# чужом файле, и нужно, чтобы доказательство осталось на диске.
#
# Запуск только так:
#   pwsh -NoProfile -File Tests/ExperimentFixes/measure-tests-build-errors.ps1 -Label before
#   pwsh -NoProfile -File Tests/ExperimentFixes/measure-tests-build-errors.ps1 -Label after
#
# Без Start-Process, без циклов ожидания, без фонового запуска.

param(
    # Метка замера: попадает в имя файла доказательства и в его заголовок.
    [Parameter(Mandatory = $true)]
    [ValidateSet('before', 'after')]
    [string]$Label
)

# Код выхода каждой команды проверяется вручную сразу после неё.
$ErrorActionPreference = 'Continue'

# Иначе русский текст в окне запуска превращается в знаки вопроса.
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$scriptDir = $PSScriptRoot
$repoRoot = (Resolve-Path (Join-Path $scriptDir '..\..')).Path
$outPath = Join-Path $scriptDir ("measure-tests-build-errors-" + $Label + ".txt")

# Шесть файлов переноса конечных показателей — это мой кусок работы.
$metricsSliceFiles = @(
    'ProjectWikiDpoPublicationMetricGate.cs',
    'ProjectWikiDomainModels.cs',
    'ProjectWikiDpoMetricRunner.cs',
    'CrossStackBusinessFlowGraphManifestPublisher.cs',
    'CrossStackBusinessFlowGraphTrainingPipeline.cs',
    'ProjectWikiDpoFinalCheckpointMetricGateTests.cs'
)
# Файл отдельной находки: обращается к типу, пространство имён которого не
# подключает. Его я считаю отдельно, чтобы числа не смешивались с моим куском.
$separateDefectFile = 'LowConfidenceLessonSignalContractTests.cs'

Set-Location $repoRoot

# Сборка проекта тестов вместе с зависимостями.
$buildOutput = & dotnet build 'Tests\LlmFromScratch.Tests.csproj' -c Debug --nologo 2>&1
$buildExitCode = $LASTEXITCODE

# Все строки ошибок как есть, затем те же строки без повторов: один и тот же
# дефект печатается дважды, если цель сборки проходит по проекту два раза.
$allErrorLines = @(
    $buildOutput |
        Select-String -Pattern 'error [A-Z]+[0-9]+' |
        ForEach-Object { $_.Line.Trim() }
)
# Ключ повтора — «файл(строка,столбец): error КОД», без текста сообщения.
$uniqueErrorLines = @(
    $allErrorLines |
        ForEach-Object {
            if ($_ -match '^(?<file>.+?)\((?<pos>\d+,\d+)\): (?<kind>error [A-Z]+[0-9]+)') {
                '{0}({1}): {2}' -f $Matches['file'], $Matches['pos'], $Matches['kind']
            }
            else { $_ }
        } |
        Sort-Object -Unique
)

function Get-Bucket([string]$errorLine) {
    foreach ($name in $metricsSliceFiles) {
        if ($errorLine -like ('*' + $name + '*')) { return 'metrics_slice' }
    }
    if ($errorLine -like ('*' + $separateDefectFile + '*')) { return 'separate_defect' }
    return 'other_untouched'
}

$metricsSliceErrors = @($uniqueErrorLines | Where-Object { (Get-Bucket $_) -eq 'metrics_slice' })
$separateDefectErrors = @($uniqueErrorLines | Where-Object { (Get-Bucket $_) -eq 'separate_defect' })
$otherErrors = @($uniqueErrorLines | Where-Object { (Get-Bucket $_) -eq 'other_untouched' })

$lines = New-Object System.Collections.Generic.List[string]
$lines.Add('=== Замер ошибок компиляции проекта тестов: ' + $Label + ' ===')
$lines.Add('Время (всемирное): ' + (Get-Date).ToUniversalTime().ToString('o'))
$lines.Add('Корень дерева: ' + $repoRoot)
$lines.Add('Код выхода сборки: ' + $buildExitCode)
$lines.Add('')
$lines.Add('Всего строк ошибок (с повторами): ' + $allErrorLines.Count)
$lines.Add('Уникальных ошибок: ' + $uniqueErrorLines.Count)
$lines.Add('Из них в моём куске переноса конечных показателей: ' + $metricsSliceErrors.Count)
$lines.Add('Из них в файле отдельной находки: ' + $separateDefectErrors.Count)
$lines.Add('Из них в прочих файлах, которых я не касался: ' + $otherErrors.Count)
$lines.Add('')
$lines.Add('--- Уникальные ошибки в моём куске переноса конечных показателей ---')
if ($metricsSliceErrors.Count -eq 0) { $lines.Add('(нет)') }
$metricsSliceErrors | ForEach-Object { $lines.Add($_) }
$lines.Add('')
$lines.Add('--- Уникальные ошибки в файле отдельной находки ---')
if ($separateDefectErrors.Count -eq 0) { $lines.Add('(нет)') }
$separateDefectErrors | ForEach-Object { $lines.Add($_) }
$lines.Add('')
$lines.Add('--- Уникальные ошибки в прочих файлах, которых я не касался ---')
if ($otherErrors.Count -eq 0) { $lines.Add('(нет)') }
$otherErrors | ForEach-Object { $lines.Add($_) }
$lines.Add('')
$lines.Add('--- Полный вывод сборки ---')
$lines.Add((($buildOutput | Out-String).TrimEnd()))

# Формат доказательств проекта: UTF-8 без метки порядка байтов, переводы строк
# LF, без пробелов в конце строк.
$joined = ($lines -join "`n") -replace "`r`n", "`n"
$trimmed = ($joined -split "`n" | ForEach-Object { $_.TrimEnd() }) -join "`n"
[System.IO.File]::WriteAllText(
    $outPath,
    ($trimmed + "`n"),
    (New-Object System.Text.UTF8Encoding $false))

Write-Output ('label=' + $Label)
Write-Output ('build_exit=' + $buildExitCode)
Write-Output ('errors_total_with_duplicates=' + $allErrorLines.Count)
Write-Output ('errors_unique=' + $uniqueErrorLines.Count)
Write-Output ('errors_unique_metrics_slice=' + $metricsSliceErrors.Count)
Write-Output ('errors_unique_separate_defect=' + $separateDefectErrors.Count)
Write-Output ('errors_unique_other_untouched=' + $otherErrors.Count)
Write-Output ('evidence_file=' + $outPath)
Write-Output '--- ошибки в моём куске ---'
if ($metricsSliceErrors.Count -eq 0) { Write-Output '(нет)' }
$metricsSliceErrors | ForEach-Object { Write-Output $_ }
Write-Output '--- ошибки в прочих файлах, которых я не касался ---'
if ($otherErrors.Count -eq 0) { Write-Output '(нет)' }
$otherErrors | ForEach-Object { Write-Output $_ }

exit $buildExitCode
