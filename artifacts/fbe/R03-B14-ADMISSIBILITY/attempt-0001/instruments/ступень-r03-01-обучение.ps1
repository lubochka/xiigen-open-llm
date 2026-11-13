#requires -Version 7
# Ступень R03/B14-01: сборка Release -> ОДИН настоящий прогон режима обучения ->
# проверка договора пояснительных полей по ПРОИЗВЕДЁННОМУ файлу итога.

param([string]$Configuration = 'Release')

$ErrorActionPreference = 'Continue'
$cfg  = $Configuration
$root = 'C:\Projects\llm_mvp_core'
$exp  = Join-Path $root 'experiments-v2\RP01-MICRO\P5001\attempt-0003'
$proj = Join-Path $exp 'Experiment.csproj'
$asmName = ([xml](Get-Content -LiteralPath $proj)).Project.PropertyGroup.AssemblyName |
    Where-Object { $_ } | Select-Object -First 1
$exe  = Join-Path $exp ("bin\" + $cfg + "\net9.0\" + $asmName + ".exe")
$src  = Join-Path $exp 'Program.cs'
$base = Join-Path $root 'artifacts\fbe\R03-B14-ADMISSIBILITY\attempt-0001'
$out  = Join-Path $base ('train-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
$res  = Join-Path $out 'results'
New-Item -ItemType Directory -Path $res -Force | Out-Null
$log  = Join-Path $out 'журнал-ступени.log'

function Say([string]$t) { $t | Tee-Object -FilePath $log -Append }
function Stamp([string]$when) {
    $h = (Get-FileHash $src -Algorithm SHA256).Hash
    Say ("исходник {0} Program.cs: sha256={1}" -f $when, $h)
    if (Test-Path $exe) {
        Say ("сборка {0} {1}: sha256={2} размер={3}" -f $when, "$asmName.exe",
            (Get-FileHash $exe -Algorithm SHA256).Hash, (Get-Item $exe).Length)
    } else { Say ("сборка {0}: файла нет" -f $when) }
}

Say ("конфигурация: {0}; каталог прогона: {1}" -f $cfg, $out)
Stamp 'ДО сборки'
dotnet build $proj -c $cfg -v minimal 2>&1 | Tee-Object -FilePath (Join-Path $out 'сборка.log')
Say ("КОД СБОРКИ: {0}" -f $LASTEXITCODE)
if ($LASTEXITCODE -ne 0) { exit 4 }
Stamp 'ПОСЛЕ сборки'

# ОДИН настоящий прогон режима обучения. Корень результатов — внутри каталога прогона,
# чтобы улика была самодостаточной и не перезаписывала каталоги прежних попыток.
Say 'ПРОГОН ОБУЧЕНИЯ: начат'
$watch = [Diagnostics.Stopwatch]::StartNew()
& $exe 'train' $res $root 2>&1 | Tee-Object -FilePath (Join-Path $out 'прогон-обучения.log')
$trainExit = $LASTEXITCODE
$watch.Stop()
Say ("ПРОГОН ОБУЧЕНИЯ: код выхода={0} время={1} с" -f $trainExit, [int]$watch.Elapsed.TotalSeconds)

# Произведённые файлы итога перечисляются: проверка читает итог, а не исходник.
Get-ChildItem $res -File | ForEach-Object { Say ("произведён файл итога: {0} ({1} байт)" -f $_.Name, $_.Length) }

$metrics = Join-Path $res 'metrics.json'
if (Test-Path $metrics) {
    # Третий аргумент — исходник решения: множество производимых статусов прибор выводит
    # из него, а не из списка внутри себя (находка приёмки о двух владельцах множества).
    python -I (Join-Path $base 'instruments\проверка-пояснений-итога.py') $metrics (Join-Path $out 'проверка-зелёная-0003.txt') $src
    Say ("ПРОВЕРКА ПОЯСНЕНИЙ: код выхода={0}" -f $LASTEXITCODE)
    Get-Content (Join-Path $out 'проверка-зелёная-0003.txt') | ForEach-Object { Say ("  " + $_) }
} else {
    Say 'ПРОВЕРКА ПОЯСНЕНИЙ: файла итога metrics.json нет — проверять нечего'
}
Stamp 'ПОСЛЕ прогона'
Say 'СТУПЕНЬ ЗАВЕРШЕНА ПОЛНОСТЬЮ'
