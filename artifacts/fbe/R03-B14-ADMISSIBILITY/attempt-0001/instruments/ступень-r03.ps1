#requires -Version 7
# Ступень R03/B14: сборка Release явным ключом -> пять оснасток через настоящий вход.
# Конфигурация — единственный владелец величины: объявлена параметром, путь сборки
# выводится из неё, она же подставляется в команду сборки.

param([string]$Configuration = 'Release')

$ErrorActionPreference = 'Stop'
$cfg  = $Configuration
$root = 'C:\Projects\llm_mvp_core'
$exp  = Join-Path $root 'experiments-v2\RP01-MICRO\P5001\attempt-0003'
$proj = Join-Path $exp 'Experiment.csproj'
# Имя исполняемого файла выводится из имени сборки, объявленного в проекте, а не
# повторяется литералом рядом: первая попытка угадала имя «Experiment.exe», прибор
# сборку не нашёл, и ступень встала. У величины «как зовётся сборка» один владелец — проект.
$asmName = ([xml](Get-Content -LiteralPath $proj)).Project.PropertyGroup.AssemblyName |
    Where-Object { $_ } | Select-Object -First 1
$exe  = Join-Path $exp ("bin\" + $cfg + "\net9.0\" + $asmName + ".exe")
$src  = Join-Path $exp 'Program.cs'
$base = Join-Path $root 'artifacts\fbe\R03-B14-ADMISSIBILITY\attempt-0001'
$fix  = Join-Path $base 'fixtures'
$out  = Join-Path $base ('run-' + $cfg.ToLower() + '-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $out -Force | Out-Null
$log  = Join-Path $out 'журнал-ступени.log'

function Say([string]$t) { $t | Tee-Object -FilePath $log -Append }
function Stamp([string]$when) {
    # Отпечатки снимает сам прибор внутрь каталога прогона: ручной шаг рядом с прогоном
    # был бы вторым владельцем величины.
    $h = (Get-FileHash $src -Algorithm SHA256).Hash
    $n = (Get-Content -LiteralPath $src | Measure-Object -Line).Lines
    Say ("исходник {0} Program.cs: sha256={1} непустых_строк={2}" -f $when, $h, $n)
    if (Test-Path $exe) {
        $eh = (Get-FileHash $exe -Algorithm SHA256).Hash
        # Подпись берёт имя из того же владельца, иначе журнал печатал бы чужое имя файла.
        Say ("сборка {0} {1}: sha256={2} размер={3} путь={4}" -f $when, "$asmName.exe", $eh, (Get-Item $exe).Length, $exe)
    } else { Say ("сборка {0}: исполняемого файла нет" -f $when) }
}

Say ("конфигурация: {0}; каталог прогона: {1}" -f $cfg, $out)
Stamp 'ДО сборки'

dotnet build $proj -c $cfg -v minimal 2>&1 | Tee-Object -FilePath (Join-Path $out 'сборка.log')
$code = $LASTEXITCODE
Say ("КОД СБОРКИ: {0}" -f $code)
if ($code -ne 0) { exit 4 }
Stamp 'ПОСЛЕ сборки'

# Пять оснасток. Каждая получает СВОЙ каталог результатов внутри каталога прогона,
# чтобы файлы итогов оснасток не перезаписывали друг друга.
$rows = @()
foreach ($dir in (Get-ChildItem -Directory $fix | Sort-Object Name)) {
    $case = $dir.Name
    $dst = Join-Path $out $case
    New-Item -ItemType Directory -Path $dst -Force | Out-Null
    Copy-Item (Join-Path $dir.FullName 'facts.json') (Join-Path $dst 'facts.json')

    # Настоящий исполняемый вход, существующий позиционный режим и корень результатов.
    $stdout = & $exe 'admissibility' $dst 2>&1
    $exit = $LASTEXITCODE
    $stdout | Out-File -FilePath (Join-Path $dst 'вывод.log') -Encoding utf8

    # Величины читаются из файла итога оснастки, а не из текста вывода.
    $j = Get-Content (Join-Path $dst 'admissibility.json') -Raw | ConvertFrom-Json
    $rows += [pscustomobject]@{
        case = $case
        exit_code = $exit
        procedure_admissible = $j.procedure_admissible
        train_calls = $j.train_calls
        reasons = ($j.procedure_inadmissibility_reasons -join ' | ')
        # Эхо договора поля ряда: пусто у оснасток, которые ряд не называли.
        sweep_series = ($j.sweep_series -join ',')
        sweep_monotone_rising = $j.sweep_monotone_rising
        sweep_edges_rising = $j.sweep_edges_rising
    }
    Say ("оснастка {0}: код выхода={1} пригодна={2} вызовов обучения={3} ряд=[{4}] монотонный={5} края_растут={6}" -f $case, $exit, $j.procedure_admissible, $j.train_calls, ($j.sweep_series -join ','), $j.sweep_monotone_rising, $j.sweep_edges_rising)
}

$rows | ConvertTo-Json -Depth 4 | Out-File -FilePath (Join-Path $out 'исходы-оснасток.json') -Encoding utf8
Stamp 'ПОСЛЕ прогона'
Say 'СТУПЕНЬ ЗАВЕРШЕНА ПОЛНОСТЬЮ'
