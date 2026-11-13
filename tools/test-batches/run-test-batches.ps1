[CmdletBinding(PositionalBinding = $false)]
param(
    [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path,
    [string]$Slice = '',
    [string]$Only = '',
    [string]$ChangedSince = '',
    [string]$ChangedTo = 'HEAD',
    [string[]]$ChangedPath = @(),
    [string]$ChangedPathsFile = '',
    [string]$BatchesFile = '',
    [string]$RunRoot = '',
    [switch]$NoBuild,
    [switch]$PlanOnly,
    [string]$BatchTimeout = '20m',
    [string]$SessionTimeoutMs = '1200000',
    [string]$HangTimeout = '10m',
    [ValidateSet('pack', 'method-diagnostic', 'class-method-pack', 'off')]
    [string]$LeafMode = 'pack',
    [int]$MaxLeaves = 0,
    [int]$MaxElapsedMinutes = 0,
    [int]$LeafTargetRows = 38,
    [int]$LeafHardCapRows = 60,
    [string]$LeafTimeout = '8m',
    [string]$LeafSessionTimeoutMs = '480000',
    [string]$LeafHangTimeout = '5m',
    [switch]$Focused,
    [switch]$FocusedObligationsOnly,
    [switch]$SkipFocusedObligations
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Quote-Bash([string]$Value) {
    # Single quotes are safest for bash -lc: Windows paths, comma-separated
    # filters, and VSTest filters stay one argument instead of being split again.
    return "'" + ($Value -replace "'", "'\\''") + "'"
}

if ([string]::IsNullOrWhiteSpace($Slice)) {
    # With no manual slice name, use the current commit so result folders are
    # easy to connect back to the code they tested.
    $Slice = (git -C $RepoRoot rev-parse --short=10 HEAD).Trim()
}

$usesChangedSelector = -not [string]::IsNullOrWhiteSpace($ChangedSince) -or
    $ChangedPath.Count -gt 0 -or
    -not [string]::IsNullOrWhiteSpace($ChangedPathsFile)
$focusedManifests = @()
$autoFocusedStamp = [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssZ', [Globalization.CultureInfo]::InvariantCulture)
$autoCanonicalStamp = $autoFocusedStamp
$outerMaxLeavesIsLimited = $MaxLeaves -gt 0
$outerRemainingLeaves = $MaxLeaves

if ($FocusedObligationsOnly -and $SkipFocusedObligations) {
    throw 'Use either -FocusedObligationsOnly or -SkipFocusedObligations, not both.'
}

if ($usesChangedSelector) {
    if (-not [string]::IsNullOrWhiteSpace($Only)) {
        throw 'Используйте либо -Only, либо выбор по изменённым файлам, но не оба режима сразу. Так объём проверки остаётся явным.'
    }

    if ($Focused) {
        throw 'Выбор по изменённым файлам возвращает id основных групп; короткий обязательный список (focused manifest) остаётся отдельным явным режимом.'
    }

    if (-not [string]::IsNullOrWhiteSpace($BatchesFile)) {
        throw 'Выбор по изменённым файлам привязан к основному списку групп (batches.tsv) и живому C#-маршруту тестовых классов. Для произвольного списка используйте -Only.'
    }

    $selectorScript = Join-Path $PSScriptRoot 'select-impacted-batches.ps1'
    $selectorParams = @{
        RepoRoot = $RepoRoot
        To = $ChangedTo
        OutputMode = 'Json'
    }

    if (-not [string]::IsNullOrWhiteSpace($ChangedSince)) {
        $selectorParams['Since'] = $ChangedSince
    }

    if ($ChangedPath.Count -gt 0) {
        $selectorParams['ChangedPath'] = [string[]]$ChangedPath
    }

    if (-not [string]::IsNullOrWhiteSpace($ChangedPathsFile)) {
        $selectorParams['ChangedPathsFile'] = $ChangedPathsFile
    }

    # Сначала читаем структурированный выбор. Так обёртка отказывает смешанному
    # автоматическому и ручному набору вместо частичного зелёного запуска.
    $selection = ((& $selectorScript @selectorParams) -join "`n") | ConvertFrom-Json
    $manualPaths = @($selection.ManualPaths | ForEach-Object { $_.Path })
    if ($manualPaths.Count -gt 0) {
        throw "Автоматический выбор по изменённым файлам неполон. Для этих путей нужен ручной -Only или короткий обязательный список (focused manifest): $($manualPaths -join ', ')"
    }

    $Only = [string]$selection.OnlyArgument
    if ([string]::IsNullOrWhiteSpace($Only)) {
        throw 'Автоматический выбор по изменённым файлам не вернул ни одного id основной группы.'
    }

    $humanBatches = @($selection.Reasons |
        ForEach-Object { "$($_.HumanName) ($($_.BatchId))" } |
        Sort-Object -Unique)
    Write-Host 'Выбор по изменённым файлам выбрал основные группы:'
    foreach ($humanBatch in $humanBatches) {
        Write-Host "- $humanBatch"
    }
    Write-Host "Исполнимый аргумент запускалки: -Only `"$Only`""

    $focusedManifests = @($selection.FocusedManifests | Where-Object {
        -not [string]::IsNullOrWhiteSpace([string]$_)
    })
    if ($focusedManifests.Count -gt 0) {
        foreach ($manifest in $focusedManifests) {
            $windowsManifest = ([string]$manifest).Replace('/', '\')
            if ($SkipFocusedObligations) {
                Write-Host "Дополнительная короткая обязательная проверка НЕ выполнена, потому что задан -SkipFocusedObligations:"
                Write-Host "pwsh -NoProfile -File .\tools\test-batches\run-test-batches.ps1 -BatchesFile .\$windowsManifest -RunRoot TestResults\focused-inventory-guard-<date> -LeafMode pack -Focused -NoBuild"
            }
            else {
                Write-Host "Дополнительная короткая обязательная проверка будет запущена перед основными группами:"
                Write-Host ".\$windowsManifest"
            }
        }
    }
    elseif ($FocusedObligationsOnly) {
        throw 'Выбор по изменённым файлам не нашёл коротких обязательных проверок для запуска.'
    }

    if ($PlanOnly) {
        # Режим плана по изменённым файлам намеренно остаётся в PowerShell:
        # проверяющий видит выбранный объём без запуска WSL и без исполнения тестов.
        Write-Host 'План выбора по изменённым файлам завершён до WSL. Тесты не выполнялись.'
        exit 0
    }
}
elseif ($FocusedObligationsOnly) {
    throw '-FocusedObligationsOnly requires -ChangedSince, -ChangedPath, or -ChangedPathsFile.'
}

if ($LeafMode -eq 'off' -and $MaxLeaves -gt 0) {
    throw 'Нельзя сочетать -LeafMode off с положительным -MaxLeaves: этот лимит считает leaf-порции, а режим off не пишет leaf-journal.tsv для честного общего бюджета. Используйте -MaxLeaves 0 или leaf-режим pack/method-diagnostic/class-method-pack.'
}

# WSL accepts the C:/... form reliably without losing backslashes.
$wslRepo = (wsl -d Ubuntu-24.04 -- wslpath -a ($RepoRoot -replace '\\', '/')).Trim()

function New-RunnerArgs {
    param(
        [string]$OnlyArgument,
        [string]$BatchesPath,
        [string]$RunRootPath,
        [bool]$IsFocusedRun,
        [int]$MaxLeavesForInvocation
    )

    # Общие лимиты держим одинаковыми для основных и коротких запусков: так
    # маленькая обязательная проверка не получает более мягкий режим, чем
    # основной batch, и не может стать красивой, но неприменимой зеленой меткой.
    $runnerArgs = @(
        'tools/test-batches/run-test-batches.sh',
        '--repo-root', $wslRepo,
        '--slice', $Slice,
        '--batch-timeout', $BatchTimeout,
        '--session-timeout-ms', $SessionTimeoutMs,
        '--hang-timeout', $HangTimeout,
        '--leaf-mode', $LeafMode,
        '--leaf-target-rows', $LeafTargetRows.ToString([Globalization.CultureInfo]::InvariantCulture),
        '--leaf-hard-cap-rows', $LeafHardCapRows.ToString([Globalization.CultureInfo]::InvariantCulture),
        '--leaf-timeout', $LeafTimeout,
        '--leaf-session-timeout-ms', $LeafSessionTimeoutMs,
        '--leaf-hang-timeout', $LeafHangTimeout
    )

    if (-not [string]::IsNullOrWhiteSpace($OnlyArgument)) {
        $runnerArgs += @('--only', $OnlyArgument)
    }

    if (-not [string]::IsNullOrWhiteSpace($BatchesPath)) {
        $wslBatches = (wsl -d Ubuntu-24.04 -- wslpath -a ($BatchesPath -replace '\\', '/')).Trim()
        $runnerArgs += @('--batches', $wslBatches)
    }

    if (-not [string]::IsNullOrWhiteSpace($RunRootPath)) {
        # Явный run root остается resume-handle: повтор команды пропускает уже
        # закрытые batch внутри той же папки, а не выдает старый TRX за новый.
        $wslRunRoot = (wsl -d Ubuntu-24.04 -- wslpath -a ($RunRootPath -replace '\\', '/')).Trim()
        $runnerArgs += @('--run-root', $wslRunRoot)
    }

    if ($NoBuild) {
        $runnerArgs += '--no-build'
    }

    if ($PlanOnly) {
        # Плановый режим показывает фильтры без запуска тестов и без подмены
        # старого TRX свежим результатом.
        $runnerArgs += '--plan-only'
    }

    if ($IsFocusedRun) {
        # Focused mode всегда привязан к custom manifest; verifier потом видит,
        # что это быстрый обязательный сигнал, а не полный основной прогон.
        $runnerArgs += '--focused'
    }

    if ($MaxLeavesForInvocation -gt 0) {
        # Ограничение держит ручную проверку короткой: запуск закрывает несколько
        # leaf и оставляет остальные явно incomplete, а не молча зелеными.
        $runnerArgs += @('--max-leaves', $MaxLeavesForInvocation.ToString([Globalization.CultureInfo]::InvariantCulture))
    }

    if ($MaxElapsedMinutes -gt 0) {
        $runnerArgs += @('--max-elapsed-minutes', $MaxElapsedMinutes.ToString([Globalization.CultureInfo]::InvariantCulture))
    }

    return $runnerArgs
}

function Resolve-RepoRelativePath {
    param([string]$Path)

    if ([string]::IsNullOrWhiteSpace($Path)) {
        return ''
    }

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return [System.IO.Path]::GetFullPath($Path)
    }

    return [System.IO.Path]::GetFullPath((Join-Path $RepoRoot $Path))
}

function Get-FocusedRunRootPath {
    param([string]$ManifestPath)

    $manifestStem = [System.IO.Path]::GetFileNameWithoutExtension($ManifestPath)
    if (-not [string]::IsNullOrWhiteSpace($RunRoot)) {
        # Явный RunRoot является ручкой продолжения для всего запуска
        # PowerShell. Короткое обязательное доказательство кладём в стабильную
        # соседнюю папку: повтор команды продолжает тот же manifest, а сводка
        # основных групп не подхватывает чужие leaf-журналы рекурсивным обходом.
        $focusedBase = (Resolve-RepoRelativePath -Path $RunRoot) + '-focused-obligations'
        return Join-Path $focusedBase $manifestStem
    }

    return Join-Path $RepoRoot (Join-Path 'TestResults\focused-obligations' "$manifestStem-$autoFocusedStamp")
}

function Get-CanonicalRunRootPath {
    if (-not [string]::IsNullOrWhiteSpace($RunRoot)) {
        return Resolve-RepoRelativePath -Path $RunRoot
    }

    if ($outerMaxLeavesIsLimited) {
        # Без явного RunRoot bash сам выбрал бы каталог, который PowerShell не
        # знает. Для общего бюджета выбираем тот же тип default-root заранее,
        # передаём его дочернему процессу и потом считаем фактически дописанные
        # leaf-строки именно в этом наблюдаемом каталоге.
        return Join-Path $RepoRoot (Join-Path "TestResults\batches-$Slice" "run-$autoCanonicalStamp")
    }

    return ''
}

function Get-LeafJournalAttemptCount {
    param([string]$RunRootPath)

    # Это не предварительное обещание и не счётчик плановых строк. Мы читаем
    # leaf-журналы до и после завершившегося дочернего процесса: прирост строк
    # соответствует реально начатым leaf-порциям, а уже закрытая работа при
    # resume не тратит новый общий бюджет.
    if ([string]::IsNullOrWhiteSpace($RunRootPath) -or -not (Test-Path -LiteralPath $RunRootPath -PathType Container)) {
        return 0
    }

    $count = 0
    $journals = @(Get-ChildItem -LiteralPath $RunRootPath -Recurse -Filter 'leaf-journal.tsv' -File -ErrorAction SilentlyContinue)
    foreach ($journal in $journals) {
        foreach ($line in Get-Content -LiteralPath $journal.FullName -Encoding UTF8) {
            if ([string]::IsNullOrWhiteSpace($line) -or $line.StartsWith('leaf_id')) {
                continue
            }

            $count++
        }
    }

    return $count
}

function Get-StateText {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return ''
    }

    return ((Get-Content -LiteralPath $Path -Raw -Encoding UTF8) -replace "`r", '').Trim()
}

function Get-RunRootLatestJournalStates {
    param([string]$RunRootPath)

    $journal = Join-Path $RunRootPath 'journal.tsv'
    if (-not (Test-Path -LiteralPath $journal -PathType Leaf)) {
        return @()
    }

    $statesByBatch = [ordered]@{}
    foreach ($line in Get-Content -LiteralPath $journal -Encoding UTF8) {
        if ([string]::IsNullOrWhiteSpace($line) -or $line.StartsWith('order')) {
            continue
        }

        $parts = $line -split "`t"
        if ($parts.Count -lt 7) {
            return @('INVALID_JOURNAL_ROW')
        }

        $statesByBatch[[string]$parts[1]] = [string]$parts[6]
    }

    return @($statesByBatch.Values)
}

function Test-IncompleteReasonIsBounded {
    param([string]$Reason)

    return $Reason.StartsWith('max_leaves:', [StringComparison]::Ordinal) -or
        $Reason.StartsWith('max_elapsed_minutes:', [StringComparison]::Ordinal) -or
        $Reason.StartsWith('plan_only:', [StringComparison]::Ordinal)
}

function Get-FocusedRunStatus {
    param([string]$RunRootPath)

    $state = Get-StateText -Path (Join-Path $RunRootPath 'state.txt')
    if ([string]::IsNullOrWhiteSpace($state)) {
        return [pscustomobject]@{ Kind = 'Bad'; Detail = 'missing state.txt' }
    }

    if ($state -eq 'INCOMPLETE') {
        $reason = Get-StateText -Path (Join-Path $RunRootPath 'incomplete-reason.txt')
        if (Test-IncompleteReasonIsBounded -Reason $reason) {
            return [pscustomobject]@{ Kind = 'Incomplete'; Detail = $reason }
        }

        return [pscustomobject]@{ Kind = 'Bad'; Detail = "неожиданно неполное короткое доказательство: $reason" }
    }

    if ($state -ne 'FINISHED') {
        return [pscustomobject]@{ Kind = 'Bad'; Detail = "неожиданное состояние корня: $state" }
    }

    $latestStates = @(Get-RunRootLatestJournalStates -RunRootPath $RunRootPath)
    if ($latestStates.Count -eq 0) {
        return [pscustomobject]@{ Kind = 'Bad'; Detail = 'missing journal rows' }
    }

    $problemStates = @($latestStates | Where-Object {
        $_ -notin @('COMPLETED_GREEN', 'EMPTY_VERIFIED', 'SKIPPED_PLATFORM')
    })
    if ($problemStates.Count -gt 0) {
        return [pscustomobject]@{ Kind = 'Bad'; Detail = "короткое доказательство не зелёное: $($problemStates -join ',')" }
    }

    return [pscustomobject]@{ Kind = 'Complete'; Detail = 'короткое доказательство завершилось зелёным' }
}

function Invoke-Runner {
    param([string[]]$RunnerArgs)

    # PowerShell только передает запуск в WSL; сами результаты остаются в
    # игнорируемой папке TestResults и не пачкают публикуемый код.
    $bashCommand = 'cd ' + (Quote-Bash $wslRepo) + ' && chmod +x tools/test-batches/run-test-batches.sh && ' + (($RunnerArgs | ForEach-Object { Quote-Bash $_ }) -join ' ')
    wsl -d Ubuntu-24.04 -- bash -lc $bashCommand | ForEach-Object { Write-Host $_ }
    return [int]$LASTEXITCODE
}

function Invoke-RunnerWithOuterBudget {
    param(
        [string]$OnlyArgument,
        [string]$BatchesPath,
        [string]$RunRootPath,
        [bool]$IsFocusedRun
    )

    if ($outerMaxLeavesIsLimited -and $script:outerRemainingLeaves -le 0) {
        # Ноль остатка означает "не запускать следующий дочерний процесс", а не
        # "--max-leaves 0" с безлимитным дочерним запуском внутри.
        Write-Host 'Бюджет MaxLeaves исчерпан до следующего дочернего запуска; новый процесс не запускается.'
        return [pscustomobject]@{
            ExitCode = 0
            StartedLeaves = 0
            SkippedByBudget = $true
            RemainingLeaves = $script:outerRemainingLeaves
        }
    }

    $beforeLeaves = Get-LeafJournalAttemptCount -RunRootPath $RunRootPath
    $childMaxLeaves = if ($outerMaxLeavesIsLimited) { $script:outerRemainingLeaves } else { 0 }
    $exitCode = Invoke-Runner (New-RunnerArgs `
        -OnlyArgument $OnlyArgument `
        -BatchesPath $BatchesPath `
        -RunRootPath $RunRootPath `
        -IsFocusedRun $IsFocusedRun `
        -MaxLeavesForInvocation $childMaxLeaves)
    $afterLeaves = Get-LeafJournalAttemptCount -RunRootPath $RunRootPath
    $startedLeaves = [Math]::Max(0, $afterLeaves - $beforeLeaves)

    if ($outerMaxLeavesIsLimited) {
        $script:outerRemainingLeaves = [Math]::Max(0, $script:outerRemainingLeaves - $startedLeaves)
        Write-Host ("Остаток общего MaxLeaves после дочернего запуска: {0}" -f $script:outerRemainingLeaves)
    }

    return [pscustomobject]@{
        ExitCode = [int]$exitCode
        StartedLeaves = $startedLeaves
        SkippedByBudget = $false
        RemainingLeaves = $script:outerRemainingLeaves
    }
}

if ($usesChangedSelector -and $focusedManifests.Count -gt 0 -and -not $SkipFocusedObligations) {
    $stopAfterFocused = $false
    foreach ($manifest in $focusedManifests) {
        $manifestPath = if ([System.IO.Path]::IsPathRooted([string]$manifest)) {
            [string]$manifest
        }
        else {
            Join-Path $RepoRoot ([string]$manifest)
        }
        $focusedRunRoot = Get-FocusedRunRootPath -ManifestPath $manifestPath

        Write-Host "Запускаю короткую обязательную проверку перед основными группами: $manifest"
        Write-Host $focusedRunRoot
        $focusedResult = Invoke-RunnerWithOuterBudget -OnlyArgument '' -BatchesPath $manifestPath -RunRootPath $focusedRunRoot -IsFocusedRun $true
        if ($focusedResult.SkippedByBudget) {
            $stopAfterFocused = $true
            break
        }

        if ($focusedResult.ExitCode -ne 0) {
            exit $focusedResult.ExitCode
        }

        $focusedStatus = Get-FocusedRunStatus -RunRootPath $focusedRunRoot
        if ($focusedStatus.Kind -eq 'Bad') {
            Write-Host "Короткое обязательное доказательство не принято как зелёное: $($focusedStatus.Detail)"
            exit 65
        }

        if ($focusedStatus.Kind -eq 'Incomplete') {
            Write-Host "Короткое обязательное доказательство остановлено честно как неполное: $($focusedStatus.Detail)"
            $stopAfterFocused = $true
            break
        }

        if ($outerMaxLeavesIsLimited -and $script:outerRemainingLeaves -le 0) {
            $stopAfterFocused = $true
            break
        }
    }

    if ($FocusedObligationsOnly -or $stopAfterFocused) {
        exit 0
    }
}

$canonicalRunRoot = Get-CanonicalRunRootPath
if ([string]::IsNullOrWhiteSpace($RunRoot) -and $outerMaxLeavesIsLimited -and -not [string]::IsNullOrWhiteSpace($canonicalRunRoot)) {
    Write-Host "Основной каталог результатов для продолжения: $canonicalRunRoot"
}
$canonicalResult = Invoke-RunnerWithOuterBudget -OnlyArgument $Only -BatchesPath $BatchesFile -RunRootPath $canonicalRunRoot -IsFocusedRun ([bool]$Focused)
$canonicalExitCode = $canonicalResult.ExitCode
# Сохраняем код выхода WSL: ошибка списка групп или красная порция тестов
# не должна превращаться в успешный вызов PowerShell-обёртки.
exit $canonicalExitCode
