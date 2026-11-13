#requires -Version 7
# Ступень: занятость общего каталога -> сборка -> обнаружение перечислением -> прогон двух классов.
# Занятость проверяется УСЛОВИЕМ внутри скрипта: без двух подряд нулей через 30 секунд сборка
# не начинается вовсе, а не просто печатается предупреждение.

$ErrorActionPreference = 'Stop'
$root = 'C:\Projects\llm_mvp_core'
$out  = Join-Path $root 'artifacts\fbe\AUDIT-F03-ATTEMPT-OWNERSHIP\attempt-0006'
$proj = Join-Path $root 'Tests\LlmFromScratch.Tests.csproj'
$lib  = Join-Path $root 'Tests\bin\Debug\net9.0\LlmFromScratch.Tests.dll'

function Occupancy {
    # Чужие сборки И прогоны держат общий каталог одинаково, поэтому считаются вместе.
    $mine = $PID
    @(Get-CimInstance Win32_Process -Filter "Name='dotnet.exe' OR Name='vstest.console.exe' OR Name='testhost.exe'" |
        Where-Object {
            $_.CommandLine -match 'LlmFromScratch\.Tests' -and
            $_.CommandLine -notmatch '--artifacts-path' -and
            $_.ProcessId -ne $mine
        }).Count
}

# Очередь: до двух подряд нулей с паузой 30 секунд, предел десять минут.
$deadline = (Get-Date).AddMinutes(10)
$zeros = 0
while ($zeros -lt 2) {
    if ((Get-Date) -gt $deadline) { "ОЧЕРЕДЬ: предел десяти минут истёк, сборка не начата" | Tee-Object -FilePath "$out\журнал-ступени.log" -Append; exit 3 }
    $n = Occupancy
    "очередь: занятость=$n нулей_подряд=$zeros $(Get-Date -Format 'HH:mm:ss')" | Tee-Object -FilePath "$out\журнал-ступени.log" -Append
    if ($n -eq 0) { $zeros++ } else { $zeros = 0 }
    if ($zeros -lt 2) { Start-Sleep -Seconds 30 }
}

# Отпечаток библиотеки ДО сборки: без него нельзя доказать, чью сборку мы прогоняем.
if (Test-Path $lib) {
    $h = (Get-FileHash $lib -Algorithm SHA256).Hash.Substring(0,16)
    $s = (Get-Item $lib).Length
    "библиотека ДО сборки: sha256=$h размер=$s" | Tee-Object -FilePath "$out\журнал-ступени.log" -Append
} else {
    "библиотека ДО сборки: отсутствует" | Tee-Object -FilePath "$out\журнал-ступени.log" -Append
}

# Сборка проекта проверок: он тянет затронутые проекты по ссылкам, поэтому собираются и они.
dotnet build $proj -v minimal 2>&1 | Tee-Object -FilePath "$out\сборка.log"
$buildCode = $LASTEXITCODE
"КОД СБОРКИ: $buildCode" | Tee-Object -FilePath "$out\журнал-ступени.log" -Append
if ($buildCode -ne 0) { exit 4 }

$h2 = (Get-FileHash $lib -Algorithm SHA256).Hash.Substring(0,16)
$s2 = (Get-Item $lib).Length
"библиотека ПОСЛЕ сборки: sha256=$h2 размер=$s2" | Tee-Object -FilePath "$out\журнал-ступени.log" -Append

# Обнаружение: полные имена перечисляются, счёт берётся по строкам, а не из итогового числа.
$filter = 'FullyQualifiedName~LlmFromScratch.Tests.ExperimentFixes.ExperimentAttemptOwnershipTests|FullyQualifiedName~LlmFromScratch.Tests.ExperimentFixes.ExperimentEvidenceSinkTests'
dotnet test $proj --no-build --list-tests --filter $filter 2>&1 | Tee-Object -FilePath "$out\обнаружение.log"
"КОД ОБНАРУЖЕНИЯ: $LASTEXITCODE" | Tee-Object -FilePath "$out\журнал-ступени.log" -Append

# Прогон ровно тех же двух классов; полный прогон проекта запрещён и не выполняется.
dotnet test $proj --no-build --filter $filter `
    --logger "trx;LogFileName=прогон.trx" --results-directory $out 2>&1 |
    Tee-Object -FilePath "$out\прогон.log"
"КОД ПРОГОНА: $LASTEXITCODE" | Tee-Object -FilePath "$out\журнал-ступени.log" -Append

$h3 = (Get-FileHash $lib -Algorithm SHA256).Hash.Substring(0,16)
"библиотека ПОСЛЕ прогона: sha256=$h3" | Tee-Object -FilePath "$out\журнал-ступени.log" -Append
"СТУПЕНЬ ЗАВЕРШЕНА ПОЛНОСТЬЮ" | Tee-Object -FilePath "$out\журнал-ступени.log" -Append
