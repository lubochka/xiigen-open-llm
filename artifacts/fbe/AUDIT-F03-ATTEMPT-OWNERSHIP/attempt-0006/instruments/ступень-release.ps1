#requires -Version 7
# Ступень в конфигурации Release: занятость -> сборка с ЯВНЫМ ключом -> обнаружение -> прогон.
# Отличия от прошлой ступени, каждое по названному дефекту:
#   1) ключ конфигурации стоит явно во всех трёх командах, а не берётся по умолчанию;
#   2) каталог результатов свежий и уникальный, прежние результаты не перезаписываются;
#   3) отпечаток и размер САМОЙ БИБЛИОТЕКИ снимаются до прогона и после него, потому что
#      привязка прогона к библиотеке по пути и времени — слабое звено, а отпечаток её закрывает.

#   4) конфигурация — ЕДИНСТВЕННЫЙ владелец величины: она объявлена один раз параметром, путь
#      библиотеки выводится из неё, и она же подставляется во все три команды. В прежнем приборе
#      было два независимых источника дефекта: сборка без ключа (бралась конфигурация по
#      умолчанию) и путь библиотеки, повторённый литералом с отладочным каталогом. Добавить ключ
#      только в команду сборки было бы половинной починкой: прибор продолжил бы смотреть на
#      отладочную библиотеку. Это тот же класс, что имя файла-подсказки в этом же слайсе:
#      величина не должна жить в двух местах.

param([string]$Configuration = 'Release')

$ErrorActionPreference = 'Stop'
$cfg  = $Configuration
$root = 'C:\Projects\llm_mvp_core'
$base = Join-Path $root 'artifacts\fbe\AUDIT-F03-ATTEMPT-OWNERSHIP\attempt-0006'
$out  = Join-Path $base ('release-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
$proj = Join-Path $root 'Tests\LlmFromScratch.Tests.csproj'
$lib  = Join-Path $root "Tests\bin\$cfg\net9.0\LlmFromScratch.Tests.dll"
New-Item -ItemType Directory -Path $out -Force | Out-Null
$log = Join-Path $out 'журнал-ступени.log'

function Say([string]$t) { $t | Tee-Object -FilePath $log -Append }
function Stamp([string]$when) {
    # Отпечаток библиотеки: объект — байты файла сборки проверок, прибор — SHA-256.
    if (Test-Path $lib) {
        $h = (Get-FileHash $lib -Algorithm SHA256).Hash
        Say ("библиотека {0}: sha256={1} размер={2}" -f $when, $h, (Get-Item $lib).Length)
    } else { Say ("библиотека {0}: отсутствует" -f $when) }
}

Say ("конфигурация: {0}; каталог результатов: {1}" -f $cfg, $out)

# Отпечатки исходника слайса снимает сам прибор, а не отдельный ручной шаг: связка
# «команда -> файл результатов -> имена и счёт -> код возврата -> библиотека и исходник» обязана
# читаться без оговорок, а ручной шаг рядом с прогоном — это второй владелец величины.
$slice = @(
    'Tests\ExperimentFixes\ExperimentEvidenceSink.cs',
    'Tests\ExperimentFixes\ExperimentAttemptScope.cs',
    'Tests\ExperimentFixes\ExperimentAttemptOwnershipTests.cs'
)
function StampSlice([string]$when) {
    foreach ($rel in $slice) {
        $p = Join-Path $root $rel
        $h = (Get-FileHash $p -Algorithm SHA256).Hash
        $n = (Get-Content -LiteralPath $p | Measure-Object -Line).Lines
        Say ("исходник {0} {1}: sha256={2} строк={3}" -f $when, $rel, $h, $n)
    }
}

# Занятость общего каталога условием: без двух подряд нулей сборка не начинается вовсе.
function Occupancy {
    $mine = $PID
    @(Get-CimInstance Win32_Process -Filter "Name='dotnet.exe' OR Name='vstest.console.exe' OR Name='testhost.exe'" |
        Where-Object {
            $_.CommandLine -match 'LlmFromScratch\.Tests' -and
            $_.CommandLine -notmatch '--artifacts-path' -and
            $_.ProcessId -ne $mine
        }).Count
}
$deadline = (Get-Date).AddMinutes(10)
$zeros = 0
while ($zeros -lt 2) {
    if ((Get-Date) -gt $deadline) { Say 'ОЧЕРЕДЬ: предел десяти минут истёк, сборка не начата'; exit 3 }
    $n = Occupancy
    Say ("очередь: занятость={0} нулей_подряд={1} {2}" -f $n, $zeros, (Get-Date -Format 'HH:mm:ss'))
    if ($n -eq 0) { $zeros++ } else { $zeros = 0 }
    if ($zeros -lt 2) { Start-Sleep -Seconds 30 }
}

Stamp 'ДО сборки'
StampSlice 'ДО сборки'
dotnet build $proj -c $cfg -v minimal 2>&1 | Tee-Object -FilePath (Join-Path $out 'сборка.log')
$code = $LASTEXITCODE
Say ("КОД СБОРКИ: {0}" -f $code)
if ($code -ne 0) { exit 4 }
Stamp 'ПОСЛЕ сборки, ДО прогона'

$filter = 'FullyQualifiedName~LlmFromScratch.Tests.ExperimentFixes.ExperimentAttemptOwnershipTests|FullyQualifiedName~LlmFromScratch.Tests.ExperimentFixes.ExperimentEvidenceSinkTests'

dotnet test $proj -c $cfg --no-build --list-tests --filter $filter 2>&1 |
    Tee-Object -FilePath (Join-Path $out 'обнаружение.log')
Say ("КОД ОБНАРУЖЕНИЯ: {0}" -f $LASTEXITCODE)

dotnet test $proj -c $cfg --no-build --filter $filter `
    --logger "trx;LogFileName=прогон-release.trx" --results-directory $out 2>&1 |
    Tee-Object -FilePath (Join-Path $out 'прогон.log')
Say ("КОД ПРОГОНА: {0}" -f $LASTEXITCODE)

Stamp 'ПОСЛЕ прогона'
StampSlice 'ПОСЛЕ прогона'

# Конфигурация прогона читается из ФАЙЛА РЕЗУЛЬТАТОВ, а не из текста прибора: путь библиотеки в
# нём — независимый признак. Прибор может говорить одно, а прогон идти по другому пути; именно так
# отладочный прогон и был найден. Поэтому признак проверяется здесь же, а не берётся на слово.
$trx = Join-Path $out 'прогон-release.trx'
if (Test-Path $trx) {
    $m = Select-String -Path $trx -Pattern 'LlmFromScratch\.Tests\.dll' -AllMatches |
         Select-Object -First 1
    $line = if ($m) { $m.Line.Trim() } else { '(путь библиотеки в файле результатов не найден)' }
    Say ("признак конфигурации из файла результатов: {0}" -f $line)
    Say ("путь ведёт в каталог {0}: {1}" -f $cfg, ($line -match [regex]::Escape("\bin\$cfg\")))
}
Say 'СТУПЕНЬ ЗАВЕРШЕНА ПОЛНОСТЬЮ'
