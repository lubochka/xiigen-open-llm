[CmdletBinding(PositionalBinding = $false)]
param(
    [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path,
    [string]$RunRoot = '',
    [string]$BatchesFile = (Join-Path $PSScriptRoot 'batches.tsv'),
    [string]$SummaryPath = '',
    [string]$NotRunReportPath = '',
    [switch]$AllowIncomplete,
    [switch]$SelfCheck
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$Utf8NoBom = [System.Text.UTF8Encoding]::new($false)

function Write-AtomicText {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Text
    )

    $directory = [System.IO.Path]::GetDirectoryName([System.IO.Path]::GetFullPath($Path))
    if (-not [string]::IsNullOrWhiteSpace($directory)) {
        [System.IO.Directory]::CreateDirectory($directory) | Out-Null
    }

    # Русский смысл: читатель не должен увидеть половину отчёта, если процесс
    # оборвётся во время записи. Поэтому пишем временный файл и заменяем целиком.
    $temporary = $Path + ".tmp." + ([System.Guid]::NewGuid().ToString('N'))
    [System.IO.File]::WriteAllText($temporary, $Text, $Utf8NoBom)
    if (Test-Path -LiteralPath $Path -PathType Leaf) {
        Remove-Item -LiteralPath $Path -Force
    }
    Move-Item -LiteralPath $temporary -Destination $Path -Force
}

function Get-RepoRelativePath {
    param([string]$Path)

    $fullPath = [System.IO.Path]::GetFullPath($Path)
    $root = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd('\', '/')
    if ($fullPath.StartsWith($root, [System.StringComparison]::OrdinalIgnoreCase)) {
        return $fullPath.Substring($root.Length).TrimStart('\', '/').Replace('\', '/')
    }

    return $fullPath
}

function Resolve-ToolPath {
    param(
        [string]$Path,
        [string]$PrimaryBase,
        [string]$SecondaryBase
    )

    if ([string]::IsNullOrWhiteSpace($Path)) {
        return ''
    }

    $candidate = $Path.Trim()
    if ($candidate -match '^/mnt/([A-Za-z])/(.*)$') {
        $drive = $Matches[1].ToUpperInvariant() + ':'
        $tail = $Matches[2].Replace('/', '\')
        return [System.IO.Path]::GetFullPath((Join-Path ($drive + '\') $tail))
    }

    if ([System.IO.Path]::IsPathRooted($candidate)) {
        return [System.IO.Path]::GetFullPath($candidate)
    }

    if (-not [string]::IsNullOrWhiteSpace($PrimaryBase)) {
        $fromPrimary = [System.IO.Path]::GetFullPath((Join-Path $PrimaryBase $candidate))
        if (Test-Path -LiteralPath $fromPrimary -PathType Leaf) {
            return $fromPrimary
        }
    }

    if (-not [string]::IsNullOrWhiteSpace($SecondaryBase)) {
        return [System.IO.Path]::GetFullPath((Join-Path $SecondaryBase $candidate))
    }

    return [System.IO.Path]::GetFullPath($candidate)
}

function Read-TsvRows {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [switch]$SkipCommentRows
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "TSV file not found: $Path"
    }

    $rawLines = @(Get-Content -LiteralPath $Path -Encoding UTF8)
    $headerLine = ''
    $startIndex = 0
    for ($index = 0; $index -lt $rawLines.Count; $index++) {
        $line = $rawLines[$index]
        if ([string]::IsNullOrWhiteSpace($line)) {
            continue
        }

        if ($SkipCommentRows -and $line.StartsWith('#')) {
            $headerLine = $line.Substring(1).TrimStart()
            $startIndex = $index + 1
            break
        }

        $headerLine = $line
        $startIndex = $index + 1
        break
    }

    if ([string]::IsNullOrWhiteSpace($headerLine)) {
        throw "TSV header not found: $Path"
    }

    $headers = @($headerLine.TrimEnd("`r") -split "`t" | ForEach-Object {
        $name = [string]$_
        if ($name.StartsWith('#')) {
            $name = $name.Substring(1).TrimStart()
        }
        $name
    })
    $rows = New-Object System.Collections.Generic.List[object]
    for ($index = $startIndex; $index -lt $rawLines.Count; $index++) {
        $line = $rawLines[$index].TrimEnd("`r")
        if ([string]::IsNullOrWhiteSpace($line)) {
            continue
        }

        if ($SkipCommentRows -and $line.StartsWith('#')) {
            continue
        }

        $parts = @($line -split "`t", 0)
        $row = [ordered]@{
            _line_number = $index + 1
            _raw = $line
        }

        for ($column = 0; $column -lt $headers.Count; $column++) {
            $value = if ($column -lt $parts.Count) { $parts[$column] } else { '' }
            $row[$headers[$column]] = $value
        }

        $rows.Add([pscustomobject]$row)
    }

    return $rows.ToArray()
}

function Convert-NonNegativeInt {
    param(
        [string]$Value,
        [string]$FieldName,
        [string]$Where
    )

    if ($Value -notmatch '^[0-9]+$') {
        throw "Bad non-negative integer in $Where field $FieldName`: '$Value'"
    }

    return [int64]$Value
}

function Get-TsvField {
    param(
        [Parameter(Mandatory = $true)]$Row,
        [Parameter(Mandatory = $true)][string]$Name
    )

    $property = $Row.PSObject.Properties[$Name]
    if ($null -eq $property) {
        return ''
    }

    return [string]$property.Value
}

function Get-AttributeValue {
    param(
        [System.Xml.XmlElement]$Element,
        [string]$Name
    )

    if ($Element.HasAttribute($Name)) {
        return $Element.GetAttribute($Name)
    }

    return ''
}

function Get-LocalNameNodes {
    param(
        [xml]$Document,
        [string]$LocalName
    )

    return @($Document.SelectNodes("//*[local-name()='$LocalName']"))
}

function Read-TrxRows {
    param([Parameter(Mandatory = $true)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "TRX file not found: $Path"
    }

    [xml]$document = Get-Content -LiteralPath $Path -Raw -Encoding UTF8
    $definitionById = @{}
    foreach ($unitTest in Get-LocalNameNodes -Document $document -LocalName 'UnitTest') {
        $testId = Get-AttributeValue -Element $unitTest -Name 'id'
        if ([string]::IsNullOrWhiteSpace($testId)) {
            continue
        }

        $method = $unitTest.SelectSingleNode("./*[local-name()='TestMethod']")
        $definitionById[$testId] = [pscustomobject]@{
            CodeBase = if ($null -ne $method) { Get-AttributeValue -Element $method -Name 'codeBase' } else { Get-AttributeValue -Element $unitTest -Name 'storage' }
            ClassName = if ($null -ne $method) { Get-AttributeValue -Element $method -Name 'className' } else { '' }
            MethodName = if ($null -ne $method) { Get-AttributeValue -Element $method -Name 'name' } else { '' }
        }
    }

    $rows = New-Object System.Collections.Generic.List[object]
    foreach ($result in Get-LocalNameNodes -Document $document -LocalName 'UnitTestResult') {
        $outcome = Get-AttributeValue -Element $result -Name 'outcome'
        $testName = Get-AttributeValue -Element $result -Name 'testName'
        $testId = Get-AttributeValue -Element $result -Name 'testId'
        if ($outcome -notin @('Passed', 'Failed', 'NotExecuted')) {
            throw "Unsupported UnitTestResult outcome '$outcome' in $Path"
        }

        $definition = if ($definitionById.ContainsKey($testId)) { $definitionById[$testId] } else { $null }
        $rows.Add([pscustomobject]@{
            Outcome = $outcome
            TestName = $testName
            TestId = $testId
            CodeBase = if ($null -ne $definition) { [string]$definition.CodeBase } else { '' }
            ClassName = if ($null -ne $definition) { [string]$definition.ClassName } else { '' }
            MethodName = if ($null -ne $definition) { [string]$definition.MethodName } else { '' }
        })
    }

    if ($rows.Count -eq 0) {
        throw "TRX has no UnitTestResult rows: $Path"
    }

    $counters = $document.SelectSingleNode("//*[local-name()='Counters']")
    $counterNotExecuted = ''
    if ($null -ne $counters) {
        $counterNotExecuted = Get-AttributeValue -Element $counters -Name 'notExecuted'
    }

    return [pscustomobject]@{
        Rows = $rows.ToArray()
        CounterNotExecuted = $counterNotExecuted
    }
}

function New-CountObject {
    param(
        [int64]$Total = 0,
        [int64]$Passed = 0,
        [int64]$Failed = 0,
        [int64]$NotExecuted = 0,
        [string]$Provenance = 'NONE',
        [int64]$JournalOnlyTotal = 0,
        [int64]$JournalOnlyExecuted = 0
    )

    # Русский смысл: число без происхождения врёт молча. Строка результата, прочитанная из
    # файла результатов, и число, заявленное журналом о себе, — это разные доказательства, и
    # после сложения их больше не различить, если источник не ехал вместе с числом. Поэтому у
    # счётчика есть происхождение, отдельный счёт журнальных строк и отдельный счёт журнальных
    # строк, ЗАЯВЛЕННЫХ ИСПОЛНЕННЫМИ (прошло плюс упало) без единого сырого подтверждения.
    return [pscustomobject]@{
        Total = $Total
        Passed = $Passed
        Failed = $Failed
        NotExecuted = $NotExecuted
        Provenance = $Provenance
        JournalOnlyTotal = $JournalOnlyTotal
        JournalOnlyExecuted = $JournalOnlyExecuted
    }
}

function Merge-Provenance {
    param(
        [string]$Left,
        [string]$Right
    )

    # Пустая сторона происхождение не меняет: складывать нечего.
    if ($Right -eq 'NONE') { return $Left }
    if ($Left -eq 'NONE') { return $Right }

    # Одинаковые источники остаются собой; разные дают честное «смешано».
    if ($Left -eq $Right) { return $Left }
    return 'MIXED'
}

function Add-Counts {
    param(
        [object]$Left,
        [object]$Right
    )

    $Left.Total += [int64]$Right.Total
    $Left.Passed += [int64]$Right.Passed
    $Left.Failed += [int64]$Right.Failed
    $Left.NotExecuted += [int64]$Right.NotExecuted

    # Происхождение переносится вместе с числами, иначе сложение его бы стирало.
    $Left.JournalOnlyTotal += [int64]$Right.JournalOnlyTotal
    $Left.JournalOnlyExecuted += [int64]$Right.JournalOnlyExecuted
    $Left.Provenance = Merge-Provenance -Left $Left.Provenance -Right $Right.Provenance
}

function Count-TrxRows {
    param([object[]]$Rows)

    # Эти числа прочитаны из настоящих строк файла результатов, поэтому источник назван TRX.
    $counts = New-CountObject -Provenance 'TRX'
    foreach ($row in $Rows) {
        $counts.Total++
        switch ($row.Outcome) {
            'Passed' { $counts.Passed++ }
            'Failed' { $counts.Failed++ }
            'NotExecuted' { $counts.NotExecuted++ }
        }
    }

    return $counts
}

function New-NotRunRow {
    param(
        [int64]$Quantity,
        [string]$BatchId,
        [string]$HumanName,
        [string]$Source,
        [string]$TestName,
        [string]$CodeBase,
        [string]$Reason
    )

    return [pscustomobject]@{
        Quantity = $Quantity
        BatchId = $BatchId
        HumanName = $HumanName
        Source = $Source
        TestName = $TestName
        CodeBase = $CodeBase
        Reason = $Reason
    }
}

function Summarize-TrxFile {
    param(
        [string]$Path,
        [string]$BatchId,
        [string]$HumanName,
        [string]$SourceLabel,
        [System.Collections.Generic.List[object]]$Warnings
    )

    $trx = Read-TrxRows -Path $Path
    $counts = Count-TrxRows -Rows $trx.Rows
    if ($trx.CounterNotExecuted -match '^[0-9]+$') {
        $counterValue = [int64]$trx.CounterNotExecuted
        if ($counterValue -ne 0 -and $counterValue -ne $counts.NotExecuted) {
            $Warnings.Add("TRX $SourceLabel сообщает Counters.notExecuted=$counterValue, но физические строки дают $($counts.NotExecuted).")
        }
        elseif ($counterValue -eq 0 -and $counts.NotExecuted -gt 0) {
            $Warnings.Add("TRX $SourceLabel имеет ноль в Counters.notExecuted, поэтому неисполненные проверки считались по строкам UnitTestResult.")
        }
    }

    $notRunRows = New-Object System.Collections.Generic.List[object]
    foreach ($row in $trx.Rows) {
        if ($row.Outcome -eq 'NotExecuted') {
            $notRunRows.Add((New-NotRunRow `
                -Quantity 1 `
                -BatchId $BatchId `
                -HumanName $HumanName `
                -Source $SourceLabel `
                -TestName $row.TestName `
                -CodeBase $row.CodeBase `
                -Reason 'TRX outcome NotExecuted; counted from UnitTestResult row, not from header'))
        }
    }

    return [pscustomobject]@{
        Counts = $counts
        NotRunRows = $notRunRows.ToArray()
    }
}

function Summarize-LeafJournal {
    param(
        [string]$LeafJournalPath,
        [string]$BatchDirectory,
        [string]$BatchId,
        [string]$HumanName,
        [System.Collections.Generic.List[object]]$Warnings
    )

    $latestByLeaf = [ordered]@{}
    foreach ($leaf in Read-TsvRows -Path $LeafJournalPath) {
        $latestByLeaf[(Get-TsvField -Row $leaf -Name 'leaf_id')] = $leaf
    }

    $counts = New-CountObject
    $notRunRows = New-Object System.Collections.Generic.List[object]
    foreach ($leafId in $latestByLeaf.Keys) {
        $leaf = $latestByLeaf[$leafId]
        $state = Get-TsvField -Row $leaf -Name 'state'
        $resultPath = Get-TsvField -Row $leaf -Name 'result_path'
        $leafDirectory = Join-Path $BatchDirectory $leafId
        if (-not [string]::IsNullOrWhiteSpace($resultPath)) {
            $trxPath = Resolve-ToolPath -Path $resultPath -PrimaryBase $leafDirectory -SecondaryBase $BatchDirectory
            if (Test-Path -LiteralPath $trxPath -PathType Leaf) {
                $leafSummary = Summarize-TrxFile `
                    -Path $trxPath `
                    -BatchId $BatchId `
                    -HumanName $HumanName `
                    -SourceLabel (Get-RepoRelativePath -Path $trxPath) `
                    -Warnings $Warnings
                Add-Counts -Left $counts -Right $leafSummary.Counts
                foreach ($notRun in $leafSummary.NotRunRows) {
                    $notRunRows.Add($notRun)
                }
                continue
            }
        }

        # ВЕТВЬ ЛИСТА БЕЗ ЧИТАЕМОГО ФАЙЛА РЕЗУЛЬТАТОВ. Числа берутся из журнала порции, то есть
        # это заявление прогона о себе, а не прочитанная строка результата. Поэтому они едут с
        # подписью JOURNAL_ONLY, и отдельно считается, сколько из них заявлено ИСПОЛНЕННЫМИ.
        $leafTotal = Convert-NonNegativeInt -Value (Get-TsvField -Row $leaf -Name 'total') -FieldName 'total' -Where "$LeafJournalPath/$leafId"
        $leafPassed = Convert-NonNegativeInt -Value (Get-TsvField -Row $leaf -Name 'passed') -FieldName 'passed' -Where "$LeafJournalPath/$leafId"
        $leafFailed = Convert-NonNegativeInt -Value (Get-TsvField -Row $leaf -Name 'failed') -FieldName 'failed' -Where "$LeafJournalPath/$leafId"
        $leafNotExecuted = Convert-NonNegativeInt -Value (Get-TsvField -Row $leaf -Name 'not_executed') -FieldName 'not_executed' -Where "$LeafJournalPath/$leafId"
        $journalCounts = New-CountObject `
            -Total $leafTotal `
            -Passed $leafPassed `
            -Failed $leafFailed `
            -NotExecuted $leafNotExecuted `
            -Provenance 'JOURNAL_ONLY' `
            -JournalOnlyTotal $leafTotal `
            -JournalOnlyExecuted ($leafPassed + $leafFailed)
        Add-Counts -Left $counts -Right $journalCounts

        # Заявленное прохождение или падение без сырого подтверждения называется вслух.
        if (($leafPassed + $leafFailed) -gt 0) {
            $Warnings.Add("NOT_PROVEN_RAW: порция $leafId батча $BatchId заявляет исполненными $($leafPassed + $leafFailed) проверок, но читаемого файла результатов у неё нет; числа взяты из журнала порции (JOURNAL_ONLY) и подтверждённой полнотой не являются.")
        }
        if ($journalCounts.NotExecuted -gt 0) {
            $notRunRows.Add((New-NotRunRow `
                -Quantity $journalCounts.NotExecuted `
                -BatchId $BatchId `
                -HumanName $HumanName `
                -Source (Get-RepoRelativePath -Path $LeafJournalPath) `
                -TestName "$leafId" `
                -CodeBase '' `
                -Reason "leaf state $state without readable TRX; counted as explicit not executed"))
        }
    }

    return [pscustomobject]@{
        Counts = $counts
        NotRunRows = $notRunRows.ToArray()
    }
}

function Summarize-RunRoot {
    param(
        [Parameter(Mandatory = $true)][string]$RunRootPath,
        [Parameter(Mandatory = $true)][string]$ManifestPath
    )

    $runRootFull = [System.IO.Path]::GetFullPath($RunRootPath)
    $manifestRows = @(Read-TsvRows -Path $ManifestPath -SkipCommentRows)
    $journalPath = Join-Path $runRootFull 'journal.tsv'
    $journalRows = @(Read-TsvRows -Path $journalPath)
    $latestByBatch = [ordered]@{}
    foreach ($row in $journalRows) {
        $latestByBatch[(Get-TsvField -Row $row -Name 'batch_id')] = $row
    }

    $warnings = New-Object System.Collections.Generic.List[object]
    $batchSummaries = New-Object System.Collections.Generic.List[object]
    $notRunRows = New-Object System.Collections.Generic.List[object]
    $totalCounts = New-CountObject
    $openCount = 0
    $invalidCount = 0
    $platformSkipCount = 0
    $emptyCount = 0
    $readableBatchCount = 0

    foreach ($manifest in $manifestRows) {
        $batchId = Get-TsvField -Row $manifest -Name 'batch_id'
        $humanName = Get-TsvField -Row $manifest -Name 'human_name'
        if ([string]::IsNullOrWhiteSpace($batchId)) {
            $properties = @($manifest.PSObject.Properties.Name) -join ','
            throw "Manifest row has empty batch_id at line $(Get-TsvField -Row $manifest -Name '_line_number'): $(Get-TsvField -Row $manifest -Name '_raw'). Properties: $properties"
        }

        if (-not $latestByBatch.Contains($batchId)) {
            $openCount++
            $batchSummaries.Add([pscustomobject]@{
                Order = Get-TsvField -Row $manifest -Name 'order'
                BatchId = $batchId
                HumanName = $humanName
                State = 'MISSING_JOURNAL_ROW'
                Counts = New-CountObject
                Source = ''
            })
            continue
        }

        $journal = $latestByBatch[$batchId]
        $state = Get-TsvField -Row $journal -Name 'state'
        $source = Get-TsvField -Row $journal -Name 'result_path'
        $batchDirectory = Join-Path $runRootFull ("batch-{0}-{1}" -f (Get-TsvField -Row $manifest -Name 'order'), $batchId)
        $counts = $null

        if ($state -eq 'EMPTY_VERIFIED') {
            $emptyCount++

            # ПОДТВЕРЖДЁННО ПУСТАЯ ГРУППА: ЧИСЛА ОСТАЮТСЯ НУЛЯМИ, НО ЗАЯВЛЕНИЕ ЖУРНАЛА ЧИТАЕТСЯ.
            #
            # Какую беду это закрывает. Прежде эта ветвь строила счётчик вызовом БЕЗ ЕДИНОГО
            # аргумента, то есть не смотрела в журнальную строку вовсе. Поэтому строка с
            # состоянием «подтверждённо пусто», заявляющая прошедшими три, давала ноль
            # недоказанных, попадала в подтверждённые, не печатала предупреждения и возвращала
            # ноль — молча. Это тот же класс, что у платформенного пропуска: закрытое состояние
            # плюс заявление об исполненных проверках плюс отсутствие сырого файла, выданные за
            # подтверждённый результат.
            #
            # Почему исполненные счётчики остаются нулями. Состояние утверждает ПУСТОТУ: в этой
            # группе нечего было запускать. Нули здесь — смысл состояния, а не следствие лени.
            #
            # ПОЧЕМУ ОБЩИЙ СЧЁТ НЕ ЧИТАЕТСЯ. Отклонённый вариант — прочитать общий счёт, чтобы
            # вернуть в эту ветвь защиту по арифметическому несовпадению. Он отклонён: чтение
            # заявленного общего счёта втащило бы ложь прямо в итог, а ловить ложь обязан новый
            # счётчик заявленных исполненными, а не арифметика. Поэтому в итог отсюда не
            # попадает ни одного журнального числа, и источник остаётся NONE правдиво: итог этих
            # чисел не содержит.
            $emptyClaimedPassed = Convert-NonNegativeInt -Value (Get-TsvField -Row $journal -Name 'passed') -FieldName 'passed' -Where "$journalPath/$batchId"
            $emptyClaimedFailed = Convert-NonNegativeInt -Value (Get-TsvField -Row $journal -Name 'failed') -FieldName 'failed' -Where "$journalPath/$batchId"
            $counts = New-CountObject -JournalOnlyExecuted ($emptyClaimedPassed + $emptyClaimedFailed)

            # Испорченная пустая группа называется вслух: она заявляет исполнение там, где
            # состояние утверждает пустоту.
            if (($emptyClaimedPassed + $emptyClaimedFailed) -gt 0) {
                $warnings.Add("NOT_PROVEN_RAW: батч $batchId помечен подтверждённо пустым, но журнал заявляет исполненными $($emptyClaimedPassed + $emptyClaimedFailed) проверок; сырого файла результатов у пустой группы нет, поэтому это заявление не подтверждено.")
            }
        }
        elseif ($state -eq 'SKIPPED_PLATFORM') {
            $platformSkipCount++

            # ЧИСЛА ПЛАТФОРМЕННОГО ПРОПУСКА ВЗЯТЫ ИЗ ЖУРНАЛА, И ЭТО НАДО НАЗЫВАТЬ.
            #
            # Файла результатов у пропуска нет по замыслу: на этой платформе ничего не
            # запускалось. Но общий счёт и счёт неисполненных всё равно прочитаны из журнала, то
            # есть это заявление прогона о себе. Прежде источник оставался NONE, журнальный счёт
            # нулём, а слияние TRX с NONE возвращало TRX — и прогон из доказанных пакетов и одного
            # пропуска печатал «Происхождение чисел: TRX» и «заявлено журналом: 0», хотя часть
            # чисел итога пришла из журнала. Ложного зелёного там не возникало, потому что
            # заявленных исполненными у пропуска принудительно ноль; врало именно происхождение.
            #
            # Отдельное имя источника для «пропуск по замыслу» намеренно НЕ вводится: факт один и
            # тот же — числа взяты из журнала. Различие «заявлено неисполнение по замыслу» против
            # «заявлено исполнение без подтверждения» несёт отдельный счёт заявленных
            # исполненными, и именно он управляет и гейтом, и условной строкой о полноте.
            #
            # ПОЧЕМУ СЧЁТ ЗАЯВЛЕННЫХ ИСПОЛНЕННЫМИ ЧИТАЕТСЯ, А НЕ ЗАШИТ НУЛЁМ. Прошедшие и упавшие
            # обнуляются ниже намеренно и так было и прежде: на этой платформе ничего не
            # запускалось. Но если ЗАШИТЬ нулём и новый счёт заявленных исполненными, то строка
            # журнала с состоянием «пропущено на платформе», заявляющая прошедших три, пройдёт
            # арифметическую проверку (прошедшие обнулены выше) и получит ноль в новом счётчике —
            # то есть новый счётчик СКАЖЕТ О НЕЙ НЕПРАВДУ, пакет попадёт в прочитанные и не
            # увеличит счёт незакрытых. Поэтому счёт читается из тех же журнальных полей, что в
            # двух других ветвях: у честной строки пропуска он и так ноль, у испорченной станет
            # ненулевым и сам приведёт пакет к незакрытому состоянию.
            $skipTotal = Convert-NonNegativeInt -Value (Get-TsvField -Row $journal -Name 'total') -FieldName 'total' -Where "$journalPath/$batchId"
            $skipNotExecuted = Convert-NonNegativeInt -Value (Get-TsvField -Row $journal -Name 'not_executed') -FieldName 'not_executed' -Where "$journalPath/$batchId"
            $skipClaimedPassed = Convert-NonNegativeInt -Value (Get-TsvField -Row $journal -Name 'passed') -FieldName 'passed' -Where "$journalPath/$batchId"
            $skipClaimedFailed = Convert-NonNegativeInt -Value (Get-TsvField -Row $journal -Name 'failed') -FieldName 'failed' -Where "$journalPath/$batchId"
            $counts = New-CountObject `
                -Total $skipTotal `
                -Passed 0 `
                -Failed 0 `
                -NotExecuted $skipNotExecuted `
                -Provenance 'JOURNAL_ONLY' `
                -JournalOnlyTotal $skipTotal `
                -JournalOnlyExecuted ($skipClaimedPassed + $skipClaimedFailed)

            # Испорченная строка пропуска называется вслух: она заявляет исполнение там, где
            # на этой платформе ничего не запускалось.
            if (($skipClaimedPassed + $skipClaimedFailed) -gt 0) {
                $warnings.Add("NOT_PROVEN_RAW: батч $batchId помечен пропуском на этой платформе, но журнал заявляет исполненными $($skipClaimedPassed + $skipClaimedFailed) проверок; сырого файла результатов у пропуска нет, поэтому это заявление не подтверждено.")
            }
            if ($counts.NotExecuted -gt 0) {
                $notRunRows.Add((New-NotRunRow `
                    -Quantity $counts.NotExecuted `
                    -BatchId $batchId `
                    -HumanName $humanName `
                    -Source 'journal.tsv' `
                    -TestName $batchId `
                    -CodeBase (Get-TsvField -Row $manifest -Name 'dll') `
                    -Reason "batch skipped on this platform: $(Get-TsvField -Row $journal -Name 'platform_reason')"))
            }
        }
        elseif ($source -match 'leaf-journal\.tsv$') {
            $leafJournalPath = Resolve-ToolPath -Path $source -PrimaryBase $batchDirectory -SecondaryBase $runRootFull
            $leafSummary = Summarize-LeafJournal `
                -LeafJournalPath $leafJournalPath `
                -BatchDirectory $batchDirectory `
                -BatchId $batchId `
                -HumanName $humanName `
                -Warnings $warnings
            $counts = $leafSummary.Counts
            foreach ($notRun in $leafSummary.NotRunRows) {
                $notRunRows.Add($notRun)
            }
        }
        elseif (-not [string]::IsNullOrWhiteSpace($source)) {
            $trxPath = Resolve-ToolPath -Path $source -PrimaryBase $batchDirectory -SecondaryBase $runRootFull
            if (Test-Path -LiteralPath $trxPath -PathType Leaf) {
                $trxSummary = Summarize-TrxFile `
                    -Path $trxPath `
                    -BatchId $batchId `
                    -HumanName $humanName `
                    -SourceLabel (Get-RepoRelativePath -Path $trxPath) `
                    -Warnings $warnings
                $counts = $trxSummary.Counts
                foreach ($notRun in $trxSummary.NotRunRows) {
                    $notRunRows.Add($notRun)
                }
            }
            else {
                # ВЕТВЬ «ИНАЧЕ», КОТОРОЙ ЗДЕСЬ НЕ БЫЛО. Журнал объявил путь результата, а файла по
                # этому пути нет. Прежде счётчик оставался пустым и ниже молча заполнялся числами
                # из журнала, поэтому заявление прогона о себе становилось неотличимым от
                # прочитанного результата. Теперь отсутствие файла называется вслух.
                $warnings.Add("NOT_PROVEN_RAW: батч $batchId объявил файл результатов $source, но файла по этому пути нет; числа взяты из журнала прогона (JOURNAL_ONLY) и подтверждённой полнотой не являются.")
            }
        }

        if ($null -eq $counts) {
            # ЧИСЛА ИЗ ЖУРНАЛА ПРОГОНА. Это заявление прогона о себе: ни одной прочитанной строки
            # результата за ними нет, поэтому они едут с подписью JOURNAL_ONLY, и отдельно
            # считается, сколько из них заявлено ИСПОЛНЕННЫМИ — прошло плюс упало.
            $batchTotal = Convert-NonNegativeInt -Value (Get-TsvField -Row $journal -Name 'total') -FieldName 'total' -Where "$journalPath/$batchId"
            $batchPassed = Convert-NonNegativeInt -Value (Get-TsvField -Row $journal -Name 'passed') -FieldName 'passed' -Where "$journalPath/$batchId"
            $batchFailed = Convert-NonNegativeInt -Value (Get-TsvField -Row $journal -Name 'failed') -FieldName 'failed' -Where "$journalPath/$batchId"
            $batchNotExecuted = Convert-NonNegativeInt -Value (Get-TsvField -Row $journal -Name 'not_executed') -FieldName 'not_executed' -Where "$journalPath/$batchId"
            $counts = New-CountObject `
                -Total $batchTotal `
                -Passed $batchPassed `
                -Failed $batchFailed `
                -NotExecuted $batchNotExecuted `
                -Provenance 'JOURNAL_ONLY' `
                -JournalOnlyTotal $batchTotal `
                -JournalOnlyExecuted ($batchPassed + $batchFailed)
        }

        if ($counts.Total -ne ($counts.Passed + $counts.Failed + $counts.NotExecuted)) {
            throw "Arithmetic mismatch for batch $batchId"
        }

        # Заявленное исполнение без сырого подтверждения: пакет не закрыт, как бы его ни назвал журнал.
        #
        # Почему именно «прошло плюс упало», а не любое журнальное число. Платформенный пропуск и
        # подтверждённо пустая группа законно не имеют файла результатов: они заявляют НОЛЬ
        # исполненных проверок, и их честность от этого не страдает. Ложь появляется там, где
        # журнал заявляет ИСПОЛНЕННЫЕ проверки, не предъявив ни одной прочитанной строки.
        $journalOnlyExecutedWithoutRaw = [int64]$counts.JournalOnlyExecuted

        if ($state -notmatch '^COMPLETED_' -and $state -ne 'EMPTY_VERIFIED' -and $state -ne 'SKIPPED_PLATFORM') {
            $openCount++
        }
        elseif ($journalOnlyExecutedWithoutRaw -gt 0) {
            # Состояние выглядит закрытым, но сырого доказательства нет: пакет остаётся незакрытым,
            # и существующий ненулевой выход по числу незакрытых пакетов сработает сам.
            $openCount++
        }

        if ($state -in @('INVALID_RESULT', 'INVALID_TOO_FEW_ROWS', 'RETRY_REQUIRED_TIMEOUT', 'SPLIT_REQUIRED') -or $state -match '^TIMED_OUT') {
            $invalidCount++
        }

        # Прочитанным считается только пакет, чьи исполненные проверки подтверждены сырым файлом.
        #
        # Прежде этот счётчик рос по одному состоянию журнала, не спрашивая, существует ли файл
        # результатов. Поэтому пакет с состоянием «завершён» и отсутствующим файлом попадал в
        # число прочитанных, и человек читал по этой строке «прогон доказан».
        if (($state -match '^COMPLETED_' -or $state -eq 'EMPTY_VERIFIED' -or $state -eq 'SKIPPED_PLATFORM') `
            -and $journalOnlyExecutedWithoutRaw -eq 0) {
            $readableBatchCount++
        }

        Add-Counts -Left $totalCounts -Right $counts
        $batchSummaries.Add([pscustomobject]@{
            Order = Get-TsvField -Row $manifest -Name 'order'
            BatchId = $batchId
            HumanName = $humanName
            State = $state
            Counts = $counts
            Source = $source
        })
    }

    if ($openCount -gt 0 -and -not $AllowIncomplete) {
        $warnings.Add("Есть незакрытые батчи: $openCount. Для чтения неполного каталога используйте -AllowIncomplete.")
    }

    # НЕДОКАЗАННАЯ ПОЛНОТА НАЗЫВАЕТСЯ НЕЗАВИСИМО ОТ ФЛАГА ЧТЕНИЯ НЕПОЛНОГО КАТАЛОГА.
    #
    # Почему это предупреждение стоит отдельно от предупреждения о незакрытых батчах. Флаг
    # -AllowIncomplete означает «каталог честно не завершён, читаю как есть» — он никогда не
    # означал «принять числа без доказательства». Это две разные беды, и глушить вторую первой
    # нельзя: иначе при документированном запуске, где флаг печатается прямо в команде, пакет,
    # заявивший прохождение без единой прочитанной строки, прошёл бы молча.
    if ([int64]$totalCounts.JournalOnlyExecuted -gt 0) {
        $warnings.Add("NOT_PROVEN_RAW: итог содержит $($totalCounts.JournalOnlyExecuted) проверок, заявленных исполненными без сырого файла результатов. Это не подтверждённая полнота, и флаг -AllowIncomplete этого не меняет.")
    }

    return [pscustomobject]@{
        RunRoot = $runRootFull
        ManifestPath = [System.IO.Path]::GetFullPath($ManifestPath)
        ManifestBatchCount = $manifestRows.Count
        JournalRawRows = $journalRows.Count
        ReadableBatchCount = $readableBatchCount
        OpenCount = $openCount
        InvalidCount = $invalidCount
        PlatformSkipCount = $platformSkipCount
        EmptyCount = $emptyCount
        Counts = $totalCounts
        BatchSummaries = $batchSummaries.ToArray()
        NotRunRows = $notRunRows.ToArray()
        Warnings = $warnings.ToArray()
    }
}

function Format-MarkdownTableCell {
    param([string]$Value)
    return (($Value -replace '\|', '\|') -replace "`r?`n", ' ').Trim()
}

function Render-SummaryMarkdown {
    param([object]$Summary)

    $arithmeticOk = $Summary.Counts.Total -eq ($Summary.Counts.Passed + $Summary.Counts.Failed + $Summary.Counts.NotExecuted)
    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add('# Сводка результатов пакетного тестирования')
    $lines.Add('')
    $lines.Add("Папка результата: ``$($Summary.RunRoot)``")
    $lines.Add('')
    $lines.Add('Смысл: этот файл читает уже созданные результаты батчей и даёт один честный итог. Он не запускает тесты, не превращает красный результат в зелёный и считает неисполненные проверки по строкам `UnitTestResult`, а не по ошибочной шапке TRX.')
    $lines.Add('')
    $lines.Add('## Итог')
    $lines.Add('')
    $lines.Add("- Батчей в реестре: $($Summary.ManifestBatchCount)")
    $lines.Add("- Сырых строк журнала: $($Summary.JournalRawRows)")
    # Название счётчика переименовано вслед за смыслом: он требует не только читаемого состояния
    # журнала, но и того, что заявленные исполненными проверки подтверждены сырым файлом.
    $lines.Add("- Батчей с прочитанным и подтверждённым результатом: $($Summary.ReadableBatchCount)")
    $lines.Add("- Незакрытых или требующих отдельного статуса: $($Summary.OpenCount)")
    $lines.Add("- Недействительных или оборванных батчей: $($Summary.InvalidCount)")
    $lines.Add("- Платформенных пропусков: $($Summary.PlatformSkipCount)")
    $lines.Add("- Подтверждённо пустых остаточных групп: $($Summary.EmptyCount)")
    $lines.Add("- Всего строк результата: $($Summary.Counts.Total)")
    $lines.Add("- Прошло: $($Summary.Counts.Passed)")
    $lines.Add("- Упало: $($Summary.Counts.Failed)")
    $lines.Add("- Не исполнялось: $($Summary.Counts.NotExecuted)")
    # Происхождение чисел рядом с самими числами: без него число врёт молча.
    # Легенда называет ВСЕ четыре значения, включая «нет».
    #
    # Почему четвёртое значение надо расшифровывать. Прежде NONE было достижимо только на прогоне
    # вообще без чисел, и человек его почти не видел. Теперь это обычный исход случая, который
    # правка специально ловит: у испорченной подтверждённо пустой группы заявленный общий счёт в
    # итог намеренно не импортируется, поэтому в итоге действительно нет ни одного числа, а
    # заявление об исполненных проверках при этом названо отдельной строкой ниже. Без расшифровки
    # человек увидел бы значение, которого нет в легенде.
    $lines.Add("- Происхождение чисел: $($Summary.Counts.Provenance) (TRX — прочитано из файлов результатов; JOURNAL_ONLY — заявлено журналом о себе; MIXED — и то и другое; NONE — в итоге нет ни одного числа)")
    $lines.Add("- Из них заявлено журналом без сырого файла результатов: $($Summary.Counts.JournalOnlyTotal)")
    # Эта строка НЕ говорит «из них»: заявление об исполненных проверках бывает и там, где в итог
    # не попало ни одного журнального числа. Так у подтверждённо пустой группы: её заявленный
    # общий счёт в итог намеренно не импортируется, а заявление об исполненных всё равно должно
    # быть названо. Приставка «из них» в этом случае читалась бы как подмножество нуля.
    $lines.Add("- Заявлено ИСПОЛНЕННЫМИ без сырого подтверждения (NOT_PROVEN_RAW): $($Summary.Counts.JournalOnlyExecuted)")
    if ([int64]$Summary.Counts.JournalOnlyExecuted -gt 0) {
        $lines.Add("- Подтверждённой полнотой этот итог НЕ является: заявленные исполненными проверки без сырого файла результатов не доказаны (JOURNAL_ONLY / NOT_PROVEN_RAW).")
    }
    $lines.Add("- Арифметика `всего = прошло + упало + не исполнялось`: $arithmeticOk")
    $lines.Add('')
    $lines.Add('## Предупреждения')
    $lines.Add('')
    if ($Summary.Warnings.Count -eq 0) {
        $lines.Add('- Нет.')
    }
    else {
        foreach ($warning in $Summary.Warnings) {
            $lines.Add("- $warning")
        }
    }
    $lines.Add('')
    $lines.Add('## Батчи')
    $lines.Add('')
    $lines.Add('| Порядок | Батч | Человеческое название | Состояние | Всего | Прошло | Упало | Не исполнялось | Происхождение | Не доказано сырым | Источник |')
    $lines.Add('|---|---|---|---|---:|---:|---:|---:|---|---:|---|')
    foreach ($batch in $Summary.BatchSummaries) {
        $lines.Add(('| {0} | {1} | {2} | {3} | {4} | {5} | {6} | {7} | {8} | {9} | {10} |' -f `
            (Format-MarkdownTableCell $batch.Order), `
            (Format-MarkdownTableCell $batch.BatchId), `
            (Format-MarkdownTableCell $batch.HumanName), `
            (Format-MarkdownTableCell $batch.State), `
            $batch.Counts.Total, `
            $batch.Counts.Passed, `
            $batch.Counts.Failed, `
            $batch.Counts.NotExecuted, `
            (Format-MarkdownTableCell $batch.Counts.Provenance), `
            $batch.Counts.JournalOnlyExecuted, `
            (Format-MarkdownTableCell $batch.Source)))
    }
    $lines.Add('')

    return ($lines -join "`n") + "`n"
}

function Render-NotRunMarkdown {
    param([object]$Summary)

    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add('# Неисполненные проверки пакетного прогона')
    $lines.Add('')
    $lines.Add("Папка результата: ``$($Summary.RunRoot)``")
    $lines.Add('')
    $lines.Add('Смысл: здесь перечислено, что не было исполнено. Строки из TRX считаются по физическим `UnitTestResult`; таймауты и платформенные пропуски записываются отдельными строками с количеством, чтобы не притворяться зелёным прогоном.')
    $lines.Add('')
    $lines.Add('| Количество | Батч | Человеческое название | Тест или порция | Сборка | Источник | Причина |')
    $lines.Add('|---:|---|---|---|---|---|---|')
    if ($Summary.NotRunRows.Count -eq 0) {
        $lines.Add('| 0 | — | — | — | — | — | Неисполненных проверок не найдено |')
    }
    else {
        foreach ($row in $Summary.NotRunRows) {
            $lines.Add(('| {0} | {1} | {2} | {3} | {4} | {5} | {6} |' -f `
                $row.Quantity, `
                (Format-MarkdownTableCell $row.BatchId), `
                (Format-MarkdownTableCell $row.HumanName), `
                (Format-MarkdownTableCell $row.TestName), `
                (Format-MarkdownTableCell $row.CodeBase), `
                (Format-MarkdownTableCell $row.Source), `
                (Format-MarkdownTableCell $row.Reason)))
        }
    }
    $lines.Add('')

    return ($lines -join "`n") + "`n"
}

function New-SyntheticTrx {
    param(
        [string]$Path,
        [object[]]$Rows,
        [int]$HeaderNotExecuted
    )

    $total = $Rows.Count
    $passed = @($Rows | Where-Object Outcome -eq 'Passed').Count
    $failed = @($Rows | Where-Object Outcome -eq 'Failed').Count
    $notExecuted = @($Rows | Where-Object Outcome -eq 'NotExecuted').Count
    $executed = $passed + $failed
    $builder = [System.Text.StringBuilder]::new()
    [void]$builder.AppendLine('<?xml version="1.0" encoding="utf-8"?>')
    [void]$builder.AppendLine('<TestRun>')
    [void]$builder.AppendLine('  <TestDefinitions>')
    foreach ($row in $Rows) {
        [void]$builder.AppendLine(("    <UnitTest name=""{0}"" id=""{1}"" storage=""{2}""><TestMethod codeBase=""{2}"" className=""{3}"" name=""{4}"" /></UnitTest>" -f `
            [System.Security.SecurityElement]::Escape($row.Name), `
            [System.Security.SecurityElement]::Escape($row.Id), `
            [System.Security.SecurityElement]::Escape($row.CodeBase), `
            [System.Security.SecurityElement]::Escape($row.ClassName), `
            [System.Security.SecurityElement]::Escape($row.MethodName)))
    }
    [void]$builder.AppendLine('  </TestDefinitions>')
    [void]$builder.AppendLine('  <Results>')
    foreach ($row in $Rows) {
        [void]$builder.AppendLine(("    <UnitTestResult testId=""{0}"" testName=""{1}"" outcome=""{2}"" />" -f `
            [System.Security.SecurityElement]::Escape($row.Id), `
            [System.Security.SecurityElement]::Escape($row.Name), `
            [System.Security.SecurityElement]::Escape($row.Outcome)))
    }
    [void]$builder.AppendLine('  </Results>')
    [void]$builder.AppendLine(("  <ResultSummary><Counters total=""{0}"" executed=""{1}"" passed=""{2}"" failed=""{3}"" notExecuted=""{4}"" error=""0"" timeout=""0"" aborted=""0"" inconclusive=""0"" passedButRunAborted=""0"" notRunnable=""0"" disconnected=""0"" warning=""0"" completed=""0"" inProgress=""0"" pending=""0"" /></ResultSummary>" -f `
        $total, $executed, $passed, $failed, $HeaderNotExecuted))
    [void]$builder.AppendLine('</TestRun>')
    Write-AtomicText -Path $Path -Text $builder.ToString()

    return [pscustomobject]@{
        Total = $total
        Passed = $passed
        Failed = $failed
        NotExecuted = $notExecuted
    }
}

function Invoke-SelfCheck {
    $stamp = [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssZ', [Globalization.CultureInfo]::InvariantCulture)
    $selfRoot = Join-Path $RepoRoot (Join-Path 'TestResults' "summarizer-self-check-$stamp")
    try {
        [System.IO.Directory]::CreateDirectory($selfRoot) | Out-Null
        $manifest = Join-Path $selfRoot 'batches.tsv'
        Write-AtomicText -Path $manifest -Text ("# order`tbatch_id`thuman_name`tdll`tfilter`tmin_rows`tsplit_on_repeat_timeout`tplatform`tplatform_reason`n" +
            "01`talpha`tПервая синтетическая группа`tA.dll`tFullyQualifiedName~Example.Alpha`t1`tfalse`tany`tself-check`n" +
            "02`tbeta`tВторая синтетическая группа`tB.dll`tFullyQualifiedName~Example.Beta`t1`tfalse`tany`tself-check`n" +
            "03`tleaf-timeout`tСинтетическая маленькая порция с таймаутом`tC.dll`tFullyQualifiedName~Example.Leaf`t2`ttrue`tany`tself-check`n")

        $alphaDir = Join-Path $selfRoot 'batch-01-alpha'
        $betaDir = Join-Path $selfRoot 'batch-02-beta'
        $leafDir = Join-Path $selfRoot 'batch-03-leaf-timeout'
        [System.IO.Directory]::CreateDirectory($alphaDir) | Out-Null
        [System.IO.Directory]::CreateDirectory($betaDir) | Out-Null
        [System.IO.Directory]::CreateDirectory($leafDir) | Out-Null

        $alphaTrx = Join-Path $alphaDir 'result.trx'
        New-SyntheticTrx -Path $alphaTrx -HeaderNotExecuted 0 -Rows @(
            [pscustomobject]@{ Id = 'test-a-shared'; Name = 'Example.Shared.Name'; Outcome = 'Passed'; CodeBase = 'A.dll'; ClassName = 'Example.Shared'; MethodName = 'Name' },
            [pscustomobject]@{ Id = 'test-a-windows'; Name = 'Example.Windows.Only'; Outcome = 'NotExecuted'; CodeBase = 'A.dll'; ClassName = 'Example.Windows'; MethodName = 'Only' }
        ) | Out-Null

        $betaTrx = Join-Path $betaDir 'result.trx'
        New-SyntheticTrx -Path $betaTrx -HeaderNotExecuted 0 -Rows @(
            [pscustomobject]@{ Id = 'test-b-shared'; Name = 'Example.Shared.Name'; Outcome = 'Passed'; CodeBase = 'B.dll'; ClassName = 'Example.Shared'; MethodName = 'Name' }
        ) | Out-Null

        $leafJournal = Join-Path $leafDir 'leaf-journal.tsv'
        Write-AtomicText -Path $leafJournal -Text ("leaf_id`tbatch_id`tstarted_utc`tfinished_utc`tseconds`tstate`texit_code`ttotal`tpassed`tfailed`tnot_executed`tresult_path`tmethod_count`testimated_rows`tfilter`n" +
            "leaf-001`tleaf-timeout`t2026-09-01T00:00:00Z`t2026-09-01T00:00:10Z`t10`tLEAF_TIMED_OUT`t124`t2`t0`t0`t2`t`t2`t2`tFullyQualifiedName=Example.Leaf.Hangs`n")

        Write-AtomicText -Path (Join-Path $selfRoot 'journal.tsv') -Text ("order`tbatch_id`thuman_name`tstarted_utc`tfinished_utc`tseconds`tstate`texit_code`ttotal`tpassed`tfailed`tnot_executed`tresult_path`tplatform`tplatform_reason`tsplit_on_repeat_timeout`n" +
            "01`talpha`tПервая синтетическая группа`t2026-09-01T00:00:00Z`t2026-09-01T00:00:01Z`t1`tCOMPLETED_GREEN`t0`t2`t1`t0`t1`t$alphaTrx`tany`tself-check`tfalse`n" +
            "02`tbeta`tВторая синтетическая группа`t2026-09-01T00:00:02Z`t2026-09-01T00:00:03Z`t1`tCOMPLETED_GREEN`t0`t1`t1`t0`t0`t$betaTrx`tany`tself-check`tfalse`n" +
            "03`tleaf-timeout`tСинтетическая маленькая порция с таймаутом`t2026-09-01T00:00:04Z`t2026-09-01T00:00:14Z`t10`tLEAF_INCOMPLETE`tleaf-pack`t2`t0`t0`t2`t$leafJournal`tany`tself-check`ttrue`n")

        $summary = Summarize-RunRoot -RunRootPath $selfRoot -ManifestPath $manifest
        if ($summary.Counts.Total -ne 5 -or $summary.Counts.Passed -ne 2 -or $summary.Counts.Failed -ne 0 -or $summary.Counts.NotExecuted -ne 3) {
            throw "Self-check failed totals: total=$($summary.Counts.Total) passed=$($summary.Counts.Passed) failed=$($summary.Counts.Failed) notExecuted=$($summary.Counts.NotExecuted)"
        }

        $notRunQuantity = 0
        foreach ($row in $summary.NotRunRows) {
            $notRunQuantity += [int64]$row.Quantity
        }

        if ($notRunQuantity -ne 3) {
            throw "Self-check failed not-run quantity: $notRunQuantity"
        }

        if (-not (@($summary.Warnings) -join "`n").Contains('Counters.notExecuted')) {
            throw 'Self-check failed: header-notExecuted trap was not reported.'
        }

        $summaryText = Render-SummaryMarkdown -Summary $summary
        $notRunText = Render-NotRunMarkdown -Summary $summary
        if (-not $summaryText.Contains('- Всего строк результата: 5') -or -not $notRunText.Contains('Example.Windows.Only') -or -not $notRunText.Contains('leaf-001')) {
            throw 'Self-check failed: rendered reports do not contain required human evidence.'
        }

        Write-Output 'Самопроверка сводчика прошла: посчитано 5 строк результата, 2 прошли, 0 упали, 3 не исполнялись; одинаковые имена из разных сборок не склеены; неисполненный тест найден по строкам TRX несмотря на ноль в шапке; timeout маленькой порции попал в отчёт.'
    }
    finally {
        if (Test-Path -LiteralPath $selfRoot -PathType Container) {
            Remove-Item -LiteralPath $selfRoot -Recurse -Force
        }
    }
}

if ($SelfCheck) {
    Invoke-SelfCheck
    exit 0
}

if ([string]::IsNullOrWhiteSpace($RunRoot)) {
    throw 'RunRoot is required unless -SelfCheck is used.'
}

$resolvedRunRoot = [System.IO.Path]::GetFullPath($RunRoot)
$resolvedSummaryPath = if ([string]::IsNullOrWhiteSpace($SummaryPath)) { Join-Path $resolvedRunRoot 'summary.md' } else { [System.IO.Path]::GetFullPath($SummaryPath) }
$resolvedNotRunPath = if ([string]::IsNullOrWhiteSpace($NotRunReportPath)) { Join-Path $resolvedRunRoot 'notrun-report.md' } else { [System.IO.Path]::GetFullPath($NotRunReportPath) }

$result = Summarize-RunRoot -RunRootPath $resolvedRunRoot -ManifestPath $BatchesFile
Write-AtomicText -Path $resolvedSummaryPath -Text (Render-SummaryMarkdown -Summary $result)
Write-AtomicText -Path $resolvedNotRunPath -Text (Render-NotRunMarkdown -Summary $result)

Write-Output "summary=$resolvedSummaryPath"
Write-Output "notrun_report=$resolvedNotRunPath"
Write-Output "total=$($result.Counts.Total) passed=$($result.Counts.Passed) failed=$($result.Counts.Failed) notExecuted=$($result.Counts.NotExecuted) open=$($result.OpenCount)"

# Происхождение чисел видно и во внешнем выводе, а не только в записанной сводке.
Write-Output "provenance=$($result.Counts.Provenance) journalOnly=$($result.Counts.JournalOnlyTotal) notProvenRaw=$($result.Counts.JournalOnlyExecuted)"

# НЕДОКАЗАННАЯ ПОЛНОТА ИМЕЕТ СВОЙ КОД ВОЗВРАТА, И ФЛАГ ЕГО НЕ СНИМАЕТ.
#
# Почему отдельный код, а не прежний выход по незакрытым батчам. Прежний выход стоит под
# условием «флаг чтения неполного каталога не задан», а оба руководства печатают этот флаг
# прямо в образцовой команде запуска. Значит при документированном запуске пакет, заявивший
# завершение и прохождение без единой прочитанной строки результата, давал бы ноль — то есть
# родной успех из журнальных чисел, запрещённый договором об исполнении тестов.
#
# Почему не второе значение того же флага. Это добавило бы поверхность и новый способ ошибиться,
# и потребовало бы объяснять пользователю различие, которого можно не создавать. Смысл флага
# остаётся один: «каталог честно не завершён». Недоказанная полнота — другая беда, и у неё
# теперь свой выход 66, отличный от выхода 65 по незакрытым батчам.
if ([int64]$result.Counts.JournalOnlyExecuted -gt 0) {
    exit 66
}

if ($result.OpenCount -gt 0 -and -not $AllowIncomplete) {
    exit 65
}
