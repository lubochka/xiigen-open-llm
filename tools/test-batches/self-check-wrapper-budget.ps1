[CmdletBinding(PositionalBinding = $false)]
param(
    [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path,
    [string]$EvidenceRoot = '',
    [string]$PwshPath = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Assert-True {
    param(
        [bool]$Condition,
        [string]$Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

function Write-TextFile {
    param(
        [string]$Path,
        [string]$Value
    )

    $parent = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($parent)) {
        New-Item -ItemType Directory -Force -Path $parent | Out-Null
    }

    Set-Content -LiteralPath $Path -Value $Value -Encoding UTF8
}

function Add-TextLine {
    param(
        [string]$Path,
        [string]$Value
    )

    $parent = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($parent)) {
        New-Item -ItemType Directory -Force -Path $parent | Out-Null
    }

    Add-Content -LiteralPath $Path -Value $Value -Encoding UTF8
}

function Get-SelfCheckLeafJournalAttemptCount {
    param([string]$Root)

    if ([string]::IsNullOrWhiteSpace($Root) -or -not (Test-Path -LiteralPath $Root -PathType Container)) {
        return 0
    }

    $count = 0
    foreach ($journal in @(Get-ChildItem -LiteralPath $Root -Recurse -Filter 'leaf-journal.tsv' -File -ErrorAction SilentlyContinue)) {
        foreach ($line in Get-Content -LiteralPath $journal.FullName -Encoding UTF8) {
            if ([string]::IsNullOrWhiteSpace($line) -or $line.StartsWith('leaf_id')) {
                continue
            }

            $count++
        }
    }

    return $count
}

function Get-WrapperOutputLines {
    param([object]$Case)

    $path = Join-Path $Case.Root 'wrapper-output.log'
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        return @()
    }

    return @(Get-Content -LiteralPath $path -Encoding UTF8)
}

function Get-FileSnapshot {
    param([string]$Path)

    $item = Get-Item -LiteralPath $Path
    return [pscustomobject]@{
        Sha256 = (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash
        Length = $item.Length
        LastWriteTimeUtc = $item.LastWriteTimeUtc
    }
}

function Assert-SameFileSnapshot {
    param(
        [object]$Before,
        [object]$After,
        [string]$Name
    )

    Assert-True ($Before.Sha256 -eq $After.Sha256) "$Name changed SHA-256."
    Assert-True ($Before.Length -eq $After.Length) "$Name changed length."
    Assert-True ($Before.LastWriteTimeUtc -eq $After.LastWriteTimeUtc) "$Name changed LastWriteTimeUtc."
}

function Get-FocusedRootForCase {
    param(
        [string]$StableRunRoot,
        [string]$ManifestPath
    )

    return Join-Path ($StableRunRoot + '-focused-obligations') ([System.IO.Path]::GetFileNameWithoutExtension($ManifestPath))
}

function Get-InvocationRows {
    param([string]$CaseRoot)

    $path = Join-Path $CaseRoot 'child-invocations.jsonl'
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        return @()
    }

    return @(Get-Content -LiteralPath $path -Encoding UTF8 | Where-Object {
        -not [string]::IsNullOrWhiteSpace($_)
    } | ForEach-Object {
        $_ | ConvertFrom-Json
    })
}

function Get-WslCallRows {
    param([string]$CaseRoot)

    $path = Join-Path $CaseRoot 'wsl-calls.jsonl'
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        return @()
    }

    return @(Get-Content -LiteralPath $path -Encoding UTF8 | Where-Object {
        -not [string]::IsNullOrWhiteSpace($_)
    } | ForEach-Object {
        $_ | ConvertFrom-Json
    })
}

function New-SelectorJson {
    param([string[]]$ManifestPaths)

    return ([pscustomobject]@{
        ChangedPathCount = 1
        BatchIds = @('canonical-a')
        OnlyArgument = 'canonical-a'
        FocusedManifests = @($ManifestPaths)
        Reasons = @([pscustomobject]@{
            Path = 'selfcheck-trigger.cs'
            BatchId = 'canonical-a'
            HumanName = 'Самопроверка основной группы'
            ClassFqn = ''
            Dll = ''
            Reason = 'синтетический выбор для проверки PowerShell-обёртки'
        })
        ManualPaths = @()
    } | ConvertTo-Json -Depth 6)
}

function New-CaseWorkspace {
    param(
        [string]$Name,
        [string[]]$FocusedManifestNames
    )

    $caseRoot = Join-Path $script:EvidenceRoot $Name
    $caseTools = Join-Path $caseRoot 'tools\test-batches'
    New-Item -ItemType Directory -Force -Path $caseTools | Out-Null
    $wrapperCopy = Join-Path $caseTools 'run-test-batches.ps1'
    Copy-Item -LiteralPath $script:LiveWrapper -Destination $wrapperCopy -Force

    $manifestRoot = Join-Path $caseRoot 'manifests'
    New-Item -ItemType Directory -Force -Path $manifestRoot | Out-Null
    $manifestPaths = @()
    foreach ($manifestName in $FocusedManifestNames) {
        $manifestPath = Join-Path $manifestRoot $manifestName
        Write-TextFile -Path $manifestPath -Value "01`t$([System.IO.Path]::GetFileNameWithoutExtension($manifestName))`tСамопроверка короткого manifest`tTests/bin/Release/net9.0/Fake.Tests.dll`tFullyQualifiedName=Example.Tests.$([System.IO.Path]::GetFileNameWithoutExtension($manifestName)).CanRun`t1`tfalse`tany`tself-check"
        $manifestPaths += $manifestPath
    }

    $selectorJson = New-SelectorJson -ManifestPaths $manifestPaths
    $selectorScript = @"
[CmdletBinding(PositionalBinding = `$false)]
param(
    [string]`$RepoRoot,
    [string]`$Since,
    [string]`$To,
    [string[]]`$ChangedPath,
    [string]`$ChangedPathsFile,
    [string]`$OutputMode
)

@'
$selectorJson
'@
"@
    Write-TextFile -Path (Join-Path $caseTools 'select-impacted-batches.ps1') -Value $selectorScript

    return [pscustomobject]@{
        Name = $Name
        Root = $caseRoot
        Wrapper = $wrapperCopy
        StableRunRoot = Join-Path $caseRoot 'stable-runroot'
        FocusedManifests = $manifestPaths
    }
}

function New-FakeClosedFocusedRoot {
    param(
        [string]$Root,
        [string]$BatchId
    )

    $batchDir = Join-Path $Root "batch-01-$BatchId"
    $leafDir = Join-Path $batchDir 'leaf-001'
    $attemptDir = Join-Path $leafDir 'attempt-0001'
    New-Item -ItemType Directory -Force -Path $attemptDir | Out-Null
    Write-TextFile -Path (Join-Path $Root 'state.txt') -Value 'FINISHED'
    Write-TextFile -Path (Join-Path $Root 'journal.tsv') -Value "order`tbatch_id`thuman_name`tstarted_utc`tfinished_utc`tseconds`tstate`texit_code`ttotal`tpassed`tfailed`tnot_executed`tresult_path`tplatform`tplatform_reason`tsplit_on_repeat_timeout`n01`t$BatchId`tСамопроверка закрытого leaf`t2026-08-31T00:00:00Z`t2026-08-31T00:00:01Z`t1`tCOMPLETED_GREEN`tleaf-pack`t1`t1`t0`t0`tleaf-journal.tsv`tany`tself-check`tfalse"
    Write-TextFile -Path (Join-Path $batchDir 'state.txt') -Value 'COMPLETED_GREEN'
    Write-TextFile -Path (Join-Path $batchDir 'leaf-journal.tsv') -Value "leaf_id`tbatch_id`tstarted_utc`tfinished_utc`tseconds`tstate`texit_code`ttotal`tpassed`tfailed`tnot_executed`tresult_path`tmethod_count`testimated_rows`tfilter`nleaf-001`t$BatchId`t2026-08-31T00:00:00Z`t2026-08-31T00:00:01Z`t1`tCOMPLETED_GREEN`t0`t1`t1`t0`t0`tresult.trx`t1`t1`tFullyQualifiedName=Example.Tests.Closed"
    Write-TextFile -Path (Join-Path $leafDir 'state.txt') -Value 'COMPLETED_GREEN'
    Write-TextFile -Path (Join-Path $attemptDir 'state.txt') -Value 'COMPLETED_GREEN'
    Write-TextFile -Path (Join-Path $attemptDir 'exit.txt') -Value '0'
    Write-TextFile -Path (Join-Path $leafDir 'result.trx') -Value '<TestRun><Results><UnitTestResult testName="Example.Tests.Closed" outcome="Passed" /></Results></TestRun>'
    Write-TextFile -Path (Join-Path $attemptDir 'result.trx') -Value '<TestRun><Results><UnitTestResult testName="Example.Tests.Closed" outcome="Passed" /></Results></TestRun>'
    return Join-Path $attemptDir 'result.trx'
}

function Invoke-WrapperCase {
    param(
        [object]$Case,
        [ValidateSet('Changed', 'ChangedNoRunRoot', 'ChangedPlanOnly', 'ExplicitOnly', 'ExplicitOnlyNoRunRoot')]
        [string]$Mode,
        [int]$MaxLeaves,
        [string]$Scenario,
        [ValidateSet('pack', 'method-diagnostic', 'off')]
        [string]$LeafMode = 'pack'
    )

    $childOutput = & $script:PwshPath -NoProfile -File $script:ChildHarness `
        -RepoRoot $script:RepoRoot `
        -WrapperPath $Case.Wrapper `
        -EvidenceRoot $Case.Root `
        -StableRunRoot $Case.StableRunRoot `
        -Mode $Mode `
        -MaxLeaves $MaxLeaves `
        -LeafMode $LeafMode `
        -Scenario $Scenario
    $childExitCode = [int]$LASTEXITCODE
    $outputLog = Join-Path $Case.Root 'wrapper-output.log'
    Add-TextLine -Path $outputLog -Value ("--- mode={0}; scenario={1}; maxLeaves={2}; leafMode={3}; exit={4}" -f $Mode, $Scenario, $MaxLeaves, $LeafMode, $childExitCode)
    foreach ($line in $childOutput) {
        Add-TextLine -Path $outputLog -Value ([string]$line)
        Write-Host $line
    }

    return $childExitCode
}

$RepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path
if ([string]::IsNullOrWhiteSpace($EvidenceRoot)) {
    $stamp = [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssZ', [Globalization.CultureInfo]::InvariantCulture)
    $EvidenceRoot = Join-Path $RepoRoot "TestResults\wrapper-budget-self-check-$stamp"
}
elseif (-not [System.IO.Path]::IsPathRooted($EvidenceRoot)) {
    $EvidenceRoot = [System.IO.Path]::GetFullPath((Join-Path $RepoRoot $EvidenceRoot))
}
else {
    $EvidenceRoot = [System.IO.Path]::GetFullPath($EvidenceRoot)
}

if ([string]::IsNullOrWhiteSpace($PwshPath)) {
    $currentPwshName = if ($IsWindows) { 'pwsh.exe' } else { 'pwsh' }
    $currentPwsh = Join-Path $PSHOME $currentPwshName
    if (Test-Path -LiteralPath $currentPwsh -PathType Leaf) {
        $PwshPath = $currentPwsh
    }
    else {
        $PwshPath = (Get-Command pwsh -ErrorAction Stop).Source
    }
}
elseif (-not (Test-Path -LiteralPath $PwshPath -PathType Leaf)) {
    throw "Не найден явно заданный pwsh: $PwshPath"
}

New-Item -ItemType Directory -Force -Path $EvidenceRoot | Out-Null
$script:RepoRoot = $RepoRoot
$script:EvidenceRoot = $EvidenceRoot
$script:PwshPath = $PwshPath
$script:LiveWrapper = Join-Path $RepoRoot 'tools\test-batches\run-test-batches.ps1'
$script:ChildHarness = Join-Path $EvidenceRoot 'invoke-wrapper-with-fake-wsl.ps1'
$wrapperHash = (Get-FileHash -LiteralPath $script:LiveWrapper -Algorithm SHA256).Hash

$childHarnessSource = @'
[CmdletBinding(PositionalBinding = $false)]
param(
    [Parameter(Mandatory = $true)]
    [string]$RepoRoot,
    [Parameter(Mandatory = $true)]
    [string]$WrapperPath,
    [Parameter(Mandatory = $true)]
    [string]$EvidenceRoot,
    [Parameter(Mandatory = $true)]
    [string]$StableRunRoot,
    [ValidateSet('Changed', 'ChangedNoRunRoot', 'ChangedPlanOnly', 'ExplicitOnly', 'ExplicitOnlyNoRunRoot')]
    [string]$Mode,
    [int]$MaxLeaves,
    [ValidateSet('pack', 'method-diagnostic', 'off')]
    [string]$LeafMode,
    [string]$Scenario
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Write-TextFile {
    param(
        [string]$Path,
        [string]$Value
    )

    $parent = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($parent)) {
        New-Item -ItemType Directory -Force -Path $parent | Out-Null
    }

    Set-Content -LiteralPath $Path -Value $Value -Encoding UTF8
}

function Add-TextLine {
    param(
        [string]$Path,
        [string]$Value
    )

    $parent = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($parent)) {
        New-Item -ItemType Directory -Force -Path $parent | Out-Null
    }

    Add-Content -LiteralPath $Path -Value $Value -Encoding UTF8
}

function Convert-ToFakeWslPath {
    param([string]$Path)

    $normalized = $Path -replace '\\', '/'
    if ($normalized -match '^([A-Za-z]):/(.*)$') {
        return "/mnt/$($Matches[1].ToLowerInvariant())/$($Matches[2])"
    }

    return $normalized
}

function Convert-FromFakeWslPath {
    param([string]$Path)

    if ($Path -match '^/mnt/([a-zA-Z])/(.*)$') {
        $drive = $Matches[1].ToUpperInvariant()
        return "$drive`:\$($Matches[2] -replace '/', '\')"
    }

    return $Path -replace '/', '\'
}

function Read-SingleQuotedTokens {
    param([string]$Command)

    $tokens = New-Object System.Collections.Generic.List[string]
    $matches = [regex]::Matches($Command, "'([^']*)'")
    foreach ($match in $matches) {
        [void]$tokens.Add([string]$match.Groups[1].Value)
    }

    return $tokens.ToArray()
}

function Get-RunnerValue {
    param(
        [string[]]$Tokens,
        [string]$Name
    )

    for ($index = 0; $index -lt $Tokens.Count - 1; $index++) {
        if ($Tokens[$index] -eq $Name) {
            return $Tokens[$index + 1]
        }
    }

    return ''
}

function Get-FakeLeafJournalAttemptCount {
    param([string]$Root)

    if ([string]::IsNullOrWhiteSpace($Root) -or -not (Test-Path -LiteralPath $Root -PathType Container)) {
        return 0
    }

    $count = 0
    foreach ($journal in @(Get-ChildItem -LiteralPath $Root -Recurse -Filter 'leaf-journal.tsv' -File -ErrorAction SilentlyContinue)) {
        foreach ($line in Get-Content -LiteralPath $journal.FullName -Encoding UTF8) {
            if ([string]::IsNullOrWhiteSpace($line) -or $line.StartsWith('leaf_id')) {
                continue
            }

            $count++
        }
    }

    return $count
}

function New-MinimalTrx {
    param([string]$Name)

    return "<TestRun><Results><UnitTestResult testName=`"$Name`" outcome=`"Passed`" /></Results></TestRun>"
}

function Write-FakeLeafResult {
    param(
        [string]$Root,
        [string]$BatchId,
        [string]$RootState,
        [string]$JournalState,
        [string]$IncompleteReason,
        [int]$LeafNumber = 0
    )

    if ($LeafNumber -le 0) {
        $LeafNumber = (Get-FakeLeafJournalAttemptCount -Root $Root) + 1
    }
    $leafId = ('leaf-{0:000}' -f $leafNumber)
    $batchDir = Join-Path $Root "batch-01-$BatchId"
    $leafDir = Join-Path $batchDir $leafId
    $attemptDir = Join-Path $leafDir 'attempt-0001'
    New-Item -ItemType Directory -Force -Path $attemptDir | Out-Null

    $leafJournal = Join-Path $batchDir 'leaf-journal.tsv'
    if (-not (Test-Path -LiteralPath $leafJournal -PathType Leaf)) {
        Write-TextFile -Path $leafJournal -Value "leaf_id`tbatch_id`tstarted_utc`tfinished_utc`tseconds`tstate`texit_code`ttotal`tpassed`tfailed`tnot_executed`tresult_path`tmethod_count`testimated_rows`tfilter"
    }

    $rootJournal = Join-Path $Root 'journal.tsv'
    if (-not (Test-Path -LiteralPath $rootJournal -PathType Leaf)) {
        Write-TextFile -Path $rootJournal -Value "order`tbatch_id`thuman_name`tstarted_utc`tfinished_utc`tseconds`tstate`texit_code`ttotal`tpassed`tfailed`tnot_executed`tresult_path`tplatform`tplatform_reason`tsplit_on_repeat_timeout"
    }

    $name = "Example.Tests.$BatchId.$leafId"
    Write-TextFile -Path (Join-Path $attemptDir 'state.txt') -Value 'COMPLETED_GREEN'
    Write-TextFile -Path (Join-Path $attemptDir 'exit.txt') -Value '0'
    Write-TextFile -Path (Join-Path $attemptDir 'result.trx') -Value (New-MinimalTrx -Name $name)
    Write-TextFile -Path (Join-Path $leafDir 'state.txt') -Value 'COMPLETED_GREEN'
    Write-TextFile -Path (Join-Path $leafDir 'exit.txt') -Value '0'
    Write-TextFile -Path (Join-Path $leafDir 'result.trx') -Value (New-MinimalTrx -Name $name)
    Write-TextFile -Path (Join-Path $batchDir 'state.txt') -Value $JournalState
    Add-TextLine -Path $leafJournal -Value "$leafId`t$BatchId`t2026-08-31T00:00:00Z`t2026-08-31T00:00:01Z`t1`tCOMPLETED_GREEN`t0`t1`t1`t0`t0`tresult.trx`t1`t1`tFullyQualifiedName=$name"
    Add-TextLine -Path $rootJournal -Value "01`t$BatchId`tСамопроверка запуска`t2026-08-31T00:00:00Z`t2026-08-31T00:00:01Z`t1`t$JournalState`tleaf-pack`t$leafNumber`t$leafNumber`t0`t0`tleaf-journal.tsv`tany`tself-check`tfalse"
    Write-TextFile -Path (Join-Path $Root 'verification-scope.tsv') -Value "key`tvalue`nscope`t$(if ($BatchId.StartsWith('focused', [StringComparison]::OrdinalIgnoreCase)) { 'FOCUSED' } else { 'CANONICAL' })"
    Write-TextFile -Path (Join-Path $Root 'state.txt') -Value $RootState
    $reasonPath = Join-Path $Root 'incomplete-reason.txt'
    if ($RootState -eq 'INCOMPLETE') {
        Write-TextFile -Path $reasonPath -Value $IncompleteReason
    }
    elseif (Test-Path -LiteralPath $reasonPath -PathType Leaf) {
        Remove-Item -LiteralPath $reasonPath -Force
    }
}

function Write-CorruptFocusedJournal {
    param(
        [string]$Root,
        [string]$BatchId
    )

    New-Item -ItemType Directory -Force -Path $Root | Out-Null
    Write-TextFile -Path (Join-Path $Root 'state.txt') -Value 'FINISHED'
    Write-TextFile -Path (Join-Path $Root 'journal.tsv') -Value "order`tbatch_id`n01`t$BatchId"
}

function Record-WslCall {
    param([string[]]$Arguments)

    $path = Join-Path $EvidenceRoot 'wsl-calls.jsonl'
    $record = [pscustomobject]@{
        arguments = @($Arguments)
    }
    Add-TextLine -Path $path -Value ($record | ConvertTo-Json -Compress -Depth 5)
}

function Record-ChildInvocation {
    param(
        [string[]]$RunnerTokens,
        [string]$Command
    )

    $path = Join-Path $EvidenceRoot 'child-invocations.jsonl'
    $sequence = 1
    if (Test-Path -LiteralPath $path -PathType Leaf) {
        $sequence = @(Get-Content -LiteralPath $path -Encoding UTF8 | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }).Count + 1
    }

    $runRootWsl = Get-RunnerValue -Tokens $RunnerTokens -Name '--run-root'
    $batchesWsl = Get-RunnerValue -Tokens $RunnerTokens -Name '--batches'
    $maxLeaves = Get-RunnerValue -Tokens $RunnerTokens -Name '--max-leaves'
    $only = Get-RunnerValue -Tokens $RunnerTokens -Name '--only'
    $record = [pscustomobject]@{
        sequence = $sequence
        runnerTokens = $RunnerTokens
        runRoot = Convert-FromFakeWslPath -Path $runRootWsl
        batches = Convert-FromFakeWslPath -Path $batchesWsl
        only = $only
        maxLeaves = $maxLeaves
        focused = ($RunnerTokens -contains '--focused')
        planOnly = ($RunnerTokens -contains '--plan-only')
        command = $Command
    }
    Add-TextLine -Path $path -Value ($record | ConvertTo-Json -Compress -Depth 6)
    return $record
}

function global:wsl {
    param(
        [Parameter(ValueFromRemainingArguments = $true)]
        [string[]]$Arguments
    )

    Record-WslCall -Arguments $Arguments

    if ($Arguments.Count -ge 4 -and $Arguments[0] -eq 'Ubuntu-24.04' -and $Arguments[1] -eq 'wslpath') {
        Convert-ToFakeWslPath -Path $Arguments[$Arguments.Count - 1]
        $global:LASTEXITCODE = 0
        return
    }

    if ($Arguments.Count -ge 5 -and $Arguments[0] -eq '-d' -and $Arguments[2] -eq '--' -and $Arguments[3] -eq 'wslpath') {
        Convert-ToFakeWslPath -Path $Arguments[$Arguments.Count - 1]
        $global:LASTEXITCODE = 0
        return
    }

    if ($Arguments.Count -ge 4 -and $Arguments[0] -eq 'Ubuntu-24.04' -and $Arguments[1] -eq 'bash' -and $Arguments[2] -eq '-lc') {
        $command = $Arguments[3]
    }
    elseif ($Arguments.Count -ge 6 -and $Arguments[0] -eq '-d' -and $Arguments[2] -eq '--' -and $Arguments[3] -eq 'bash' -and $Arguments[4] -eq '-lc') {
        $command = $Arguments[5]
    }
    else {
        throw "unexpected fake wsl arguments: $($Arguments -join ' ')"
    }

    $quoted = @(Read-SingleQuotedTokens -Command $command)
    if ($quoted.Count -lt 2) {
        throw "fake wsl could not parse runner command: $command"
    }

    $runnerTokens = @($quoted | Select-Object -Skip 1)
    $record = Record-ChildInvocation -RunnerTokens $runnerTokens -Command $command
    $isFocused = [bool]$record.focused
    $batchId = if ($isFocused) {
        [System.IO.Path]::GetFileNameWithoutExtension([string]$record.batches)
    }
    elseif (-not [string]::IsNullOrWhiteSpace([string]$record.only)) {
        [string]$record.only
    }
    else {
        'canonical-a'
    }
    $batchId = ($batchId -replace '[^A-Za-z0-9_.-]', '-')

    if ($Scenario -eq 'bad-missing' -and $isFocused) {
        $global:LASTEXITCODE = 0
        return
    }

    if ($Scenario -eq 'red-focused' -and $isFocused) {
        Write-FakeLeafResult -Root ([string]$record.runRoot) -BatchId $batchId -RootState 'FINISHED' -JournalState 'COMPLETED_RED' -IncompleteReason ''
        $global:LASTEXITCODE = 0
        return
    }

    if ($Scenario -eq 'corrupt-journal' -and $isFocused) {
        Write-CorruptFocusedJournal -Root ([string]$record.runRoot) -BatchId $batchId
        $global:LASTEXITCODE = 0
        return
    }

    if ($Scenario -eq 'closed-free' -and $isFocused) {
        $global:LASTEXITCODE = 0
        return
    }

    if ($Scenario -eq 'incomplete-resume' -and $isFocused) {
        $resumeMarker = Join-Path ([string]$record.runRoot) 'fake-resume-step.txt'
        if (-not (Test-Path -LiteralPath $resumeMarker -PathType Leaf)) {
            Write-FakeLeafResult -Root ([string]$record.runRoot) -BatchId $batchId -RootState 'INCOMPLETE' -JournalState 'LEAF_INCOMPLETE' -IncompleteReason 'max_leaves:1' -LeafNumber 1
            Write-TextFile -Path $resumeMarker -Value '1'
        }
        else {
            Write-FakeLeafResult -Root ([string]$record.runRoot) -BatchId $batchId -RootState 'FINISHED' -JournalState 'COMPLETED_GREEN' -IncompleteReason '' -LeafNumber 2
        }
        $global:LASTEXITCODE = 0
        return
    }

    Write-FakeLeafResult -Root ([string]$record.runRoot) -BatchId $batchId -RootState 'FINISHED' -JournalState 'COMPLETED_GREEN' -IncompleteReason ''
    if ($Scenario -eq 'stdout-noise') {
        Write-Output 'synthetic child stdout includes 0 but is not the exit code'
    }
    $global:LASTEXITCODE = 0
}

switch ($Mode) {
    'Changed' {
        & $WrapperPath -RepoRoot $RepoRoot -ChangedPath 'selfcheck-trigger.cs' -RunRoot $StableRunRoot -NoBuild -LeafMode $LeafMode -MaxLeaves $MaxLeaves
    }
    'ChangedNoRunRoot' {
        & $WrapperPath -RepoRoot $EvidenceRoot -Slice 'wrapper-budget-self-check' -ChangedPath 'selfcheck-trigger.cs' -NoBuild -LeafMode $LeafMode -MaxLeaves $MaxLeaves
    }
    'ChangedPlanOnly' {
        & $WrapperPath -RepoRoot $RepoRoot -ChangedPath 'selfcheck-trigger.cs' -RunRoot $StableRunRoot -NoBuild -LeafMode $LeafMode -MaxLeaves $MaxLeaves -PlanOnly
    }
    'ExplicitOnly' {
        & $WrapperPath -RepoRoot $RepoRoot -Only 'canonical-a' -RunRoot $StableRunRoot -NoBuild -LeafMode $LeafMode -MaxLeaves $MaxLeaves
    }
    'ExplicitOnlyNoRunRoot' {
        & $WrapperPath -RepoRoot $EvidenceRoot -Slice 'wrapper-budget-self-check' -Only 'canonical-a' -NoBuild -LeafMode $LeafMode -MaxLeaves $MaxLeaves
    }
}

exit $LASTEXITCODE
'@
Write-TextFile -Path $script:ChildHarness -Value $childHarnessSource

# Сценарий 1. Один короткий manifest и общий лимит 1: короткая проверка
# получает лимит, тратит одну новую leaf-порцию, а основные группы уже не
# стартуют. Fake-процесс специально пишет stdout с цифрой, чтобы проверить, что
# вывод дочернего процесса не смешивается с числовым кодом выхода.
$limitOne = New-CaseWorkspace -Name 'limit-one-shared-budget' -FocusedManifestNames @('focused-a.tsv')
$limitExit = Invoke-WrapperCase -Case $limitOne -Mode Changed -MaxLeaves 1 -Scenario 'stdout-noise'
Assert-True ($limitExit -eq 0) "limit-one exit code was $limitExit."
$limitRows = @(Get-InvocationRows -CaseRoot $limitOne.Root)
Assert-True ($limitRows.Count -eq 1) "limit-one should start only focused child, got $($limitRows.Count)."
Assert-True ([bool]$limitRows[0].focused) 'limit-one first child was not focused.'
Assert-True ([string]$limitRows[0].maxLeaves -eq '1') 'limit-one focused child did not receive remaining MaxLeaves 1.'
Assert-True (-not ($limitRows[0].runnerTokens -contains '--max-leaves' -and [string]$limitRows[0].maxLeaves -eq '0')) 'limit-one passed unlimited zero after budget exhaustion.'

# Сценарий 2. Уже закрытый короткий leaf не должен тратить новый общий бюджет:
# wrapper сравнивает количество leaf-журнальных строк до и после завершения
# дочернего процесса. Если строк не прибавилось, основной запуск получает тот же
# остаток лимита, а старый TRX/attempt не переписывается.
$closedFree = New-CaseWorkspace -Name 'closed-focused-is-free' -FocusedManifestNames @('focused-a.tsv')
$closedFocusedRoot = Get-FocusedRootForCase -StableRunRoot $closedFree.StableRunRoot -ManifestPath $closedFree.FocusedManifests[0]
$protectedResult = New-FakeClosedFocusedRoot -Root $closedFocusedRoot -BatchId 'focused-a'
$beforeProtected = Get-FileSnapshot -Path $protectedResult
$closedExit = Invoke-WrapperCase -Case $closedFree -Mode Changed -MaxLeaves 1 -Scenario 'closed-free'
Assert-True ($closedExit -eq 0) "closed-free exit code was $closedExit."
$closedRows = @(Get-InvocationRows -CaseRoot $closedFree.Root)
Assert-True ($closedRows.Count -eq 2) "closed-free should run focused skip and then canonical, got $($closedRows.Count)."
Assert-True ([bool]$closedRows[0].focused) 'closed-free first child should be focused.'
Assert-True (-not [bool]$closedRows[1].focused) 'closed-free second child should be canonical.'
Assert-True ([string]$closedRows[1].maxLeaves -eq '1') 'closed-free canonical child should keep MaxLeaves 1 because closed focused work is free.'
$afterProtected = Get-FileSnapshot -Path $protectedResult
Assert-SameFileSnapshot -Before $beforeProtected -After $afterProtected -Name 'pre-existing focused result'

# Сценарий 3. Два коротких manifest при лимите 1: первый тратит единственную
# порцию, второй короткий manifest и основные группы не запускаются. Так
# проверяется именно общий бюджет между несколькими дочерними процессами.
$twoFocused = New-CaseWorkspace -Name 'two-focused-manifests-budget-boundary' -FocusedManifestNames @('focused-a.tsv', 'focused-b.tsv')
$twoExit = Invoke-WrapperCase -Case $twoFocused -Mode Changed -MaxLeaves 1 -Scenario 'complete'
Assert-True ($twoExit -eq 0) "two-focused exit code was $twoExit."
$twoRows = @(Get-InvocationRows -CaseRoot $twoFocused.Root)
Assert-True ($twoRows.Count -eq 1) "two-focused should stop after first focused child, got $($twoRows.Count)."
Assert-True ([string]$twoRows[0].batches -eq [string]$twoFocused.FocusedManifests[0]) 'two-focused did not preserve manifest order or budget boundary.'

# Сценарий 4. Проверка расходования остатка, а не только ранней остановки:
# два коротких manifest и основные группы при MaxLeaves 3 должны получить
# дочерние лимиты 3, затем 2, затем 1. Каждый fake-запуск пишет ровно одну новую
# leaf-порцию, поэтому последовательность остатков полностью наблюдаема.
$remainingThree = New-CaseWorkspace -Name 'remaining-budget-decrements-through-main' -FocusedManifestNames @('focused-a.tsv', 'focused-b.tsv')
$remainingExit = Invoke-WrapperCase -Case $remainingThree -Mode Changed -MaxLeaves 3 -Scenario 'complete'
Assert-True ($remainingExit -eq 0) "remaining-three exit code was $remainingExit."
$remainingRows = @(Get-InvocationRows -CaseRoot $remainingThree.Root)
Assert-True ($remainingRows.Count -eq 3) "remaining-three should run two focused children and one canonical child, got $($remainingRows.Count)."
Assert-True ([string]$remainingRows[0].maxLeaves -eq '3') 'remaining-three first focused child should receive MaxLeaves 3.'
Assert-True ([string]$remainingRows[1].maxLeaves -eq '2') 'remaining-three second focused child should receive remaining MaxLeaves 2.'
Assert-True ([string]$remainingRows[2].maxLeaves -eq '1') 'remaining-three canonical child should receive remaining MaxLeaves 1.'
Assert-True ([bool]$remainingRows[0].focused -and [bool]$remainingRows[1].focused -and -not [bool]$remainingRows[2].focused) 'remaining-three focused/canonical order is wrong.'

# Сценарий 5. Неполная короткая проверка должна продолжаться в той же папке,
# если команда повторена с тем же явным RunRoot. Первый leaf/attempt фиксируется
# после неполного шага и затем сравнивается после второго шага: повтор не имеет
# права переписать уже полученное доказательство.
$resume = New-CaseWorkspace -Name 'incomplete-focused-resumes-same-directory' -FocusedManifestNames @('focused-a.tsv')
$resumeExit1 = Invoke-WrapperCase -Case $resume -Mode Changed -MaxLeaves 1 -Scenario 'incomplete-resume'
Assert-True ($resumeExit1 -eq 0) "resume first exit code was $resumeExit1."
$resumeFocusedRoot = Get-FocusedRootForCase -StableRunRoot $resume.StableRunRoot -ManifestPath $resume.FocusedManifests[0]
$resumeState1 = (Get-Content -LiteralPath (Join-Path $resumeFocusedRoot 'state.txt') -Raw -Encoding UTF8).Trim()
Assert-True ($resumeState1 -eq 'INCOMPLETE') "resume first focused state was $resumeState1."
$resumeFirstLeafResult = Join-Path $resumeFocusedRoot 'batch-01-focused-a\leaf-001\result.trx'
$resumeFirstAttemptResult = Join-Path $resumeFocusedRoot 'batch-01-focused-a\leaf-001\attempt-0001\result.trx'
$resumeLeafBefore = Get-FileSnapshot -Path $resumeFirstLeafResult
$resumeAttemptBefore = Get-FileSnapshot -Path $resumeFirstAttemptResult
$resumeExit2 = Invoke-WrapperCase -Case $resume -Mode Changed -MaxLeaves 1 -Scenario 'incomplete-resume'
Assert-True ($resumeExit2 -eq 0) "resume second exit code was $resumeExit2."
$resumeRows = @(Get-InvocationRows -CaseRoot $resume.Root)
Assert-True ($resumeRows.Count -eq 2) "resume should run the same focused child twice, got $($resumeRows.Count)."
Assert-True ([string]$resumeRows[0].runRoot -eq [string]$resumeRows[1].runRoot) 'resume focused RunRoot changed between identical commands.'
$resumeState2 = (Get-Content -LiteralPath (Join-Path ([string]$resumeRows[1].runRoot) 'state.txt') -Raw -Encoding UTF8).Trim()
Assert-True ($resumeState2 -eq 'FINISHED') "resume second focused state was $resumeState2."
$resumeLeafAfter = Get-FileSnapshot -Path $resumeFirstLeafResult
$resumeAttemptAfter = Get-FileSnapshot -Path $resumeFirstAttemptResult
Assert-SameFileSnapshot -Before $resumeLeafBefore -After $resumeLeafAfter -Name 'partial resume first leaf result'
Assert-SameFileSnapshot -Before $resumeAttemptBefore -After $resumeAttemptAfter -Name 'partial resume first attempt result'

# Сценарий 6. Нулевой MaxLeaves означает безлимитный режим. Wrapper не должен
# передавать дочернему процессу "--max-leaves 0", потому что для bash-запуска это
# специальное значение "без ограничения", а не израсходованный остаток.
$unlimited = New-CaseWorkspace -Name 'unlimited-zero-and-focused-plus-main' -FocusedManifestNames @('focused-a.tsv')
$unlimitedExit = Invoke-WrapperCase -Case $unlimited -Mode Changed -MaxLeaves 0 -Scenario 'complete'
Assert-True ($unlimitedExit -eq 0) "unlimited exit code was $unlimitedExit."
$unlimitedRows = @(Get-InvocationRows -CaseRoot $unlimited.Root)
Assert-True ($unlimitedRows.Count -eq 2) "unlimited should run focused and canonical, got $($unlimitedRows.Count)."
Assert-True (@($unlimitedRows | Where-Object { $_.runnerTokens -contains '--max-leaves' }).Count -eq 0) 'unlimited MaxLeaves 0 should be omitted, not forwarded as a bounded child budget.'

# Сценарий 7. PlanOnly для выбора по изменённым путям обязан остановиться в
# PowerShell после печати плана: ни wslpath, ни bash-дочерний процесс не должны
# запускаться.
$planOnly = New-CaseWorkspace -Name 'changed-plan-only-no-wsl' -FocusedManifestNames @('focused-a.tsv')
$planExit = Invoke-WrapperCase -Case $planOnly -Mode ChangedPlanOnly -MaxLeaves 1 -Scenario 'complete'
Assert-True ($planExit -eq 0) "plan-only exit code was $planExit."
Assert-True (@(Get-InvocationRows -CaseRoot $planOnly.Root).Count -eq 0) 'plan-only should not start a bash child.'
Assert-True (@(Get-WslCallRows -CaseRoot $planOnly.Root).Count -eq 0) 'plan-only should not call WSL at all.'

# Сценарий 8. Явный -Only не проходит через автоматические короткие manifest:
# это один основной дочерний запуск с переданным пользователем фильтром.
$explicitOnly = New-CaseWorkspace -Name 'explicit-only-stays-canonical' -FocusedManifestNames @('focused-a.tsv')
$explicitExit = Invoke-WrapperCase -Case $explicitOnly -Mode ExplicitOnly -MaxLeaves 1 -Scenario 'complete'
Assert-True ($explicitExit -eq 0) "explicit-only exit code was $explicitExit."
$explicitRows = @(Get-InvocationRows -CaseRoot $explicitOnly.Root)
Assert-True ($explicitRows.Count -eq 1) "explicit-only should run one canonical child, got $($explicitRows.Count)."
Assert-True (-not [bool]$explicitRows[0].focused) 'explicit-only incorrectly ran focused mode.'
Assert-True ([string]$explicitRows[0].only -eq 'canonical-a') 'explicit-only lost the requested -Only value.'
Assert-True ([string]$explicitRows[0].maxLeaves -eq '1') 'explicit-only did not pass bounded MaxLeaves to the single canonical child.'

# Сценарий 8a. Явный -Only без RunRoot при ограниченном бюджете получает
# наблюдаемый основной каталог результатов. Это закрывает дефект, где wrapper
# не знал каталог, видел прирост 0 и печатал ложный остаток после реального leaf.
$explicitNoRunRoot = New-CaseWorkspace -Name 'explicit-only-no-runroot-observed-budget' -FocusedManifestNames @('focused-a.tsv')
$explicitNoRunRootExit = Invoke-WrapperCase -Case $explicitNoRunRoot -Mode ExplicitOnlyNoRunRoot -MaxLeaves 1 -Scenario 'complete'
Assert-True ($explicitNoRunRootExit -eq 0) "explicit-only no RunRoot exit code was $explicitNoRunRootExit."
$explicitNoRunRootRows = @(Get-InvocationRows -CaseRoot $explicitNoRunRoot.Root)
Assert-True ($explicitNoRunRootRows.Count -eq 1) "explicit-only no RunRoot should run one canonical child, got $($explicitNoRunRootRows.Count)."
Assert-True (-not [bool]$explicitNoRunRootRows[0].focused) 'explicit-only no RunRoot incorrectly ran focused mode.'
Assert-True ([string]$explicitNoRunRootRows[0].only -eq 'canonical-a') 'explicit-only no RunRoot lost the requested -Only value.'
Assert-True ([string]$explicitNoRunRootRows[0].maxLeaves -eq '1') 'explicit-only no RunRoot did not pass bounded MaxLeaves 1.'
Assert-True (-not [string]::IsNullOrWhiteSpace([string]$explicitNoRunRootRows[0].runRoot)) 'explicit-only no RunRoot did not receive an observable canonical RunRoot.'
Assert-True (([string]$explicitNoRunRootRows[0].runRoot).StartsWith((Join-Path $explicitNoRunRoot.Root 'TestResults\batches-wrapper-budget-self-check\run-'), [StringComparison]::OrdinalIgnoreCase)) 'explicit-only no RunRoot chose an unexpected canonical RunRoot.'
Assert-True ((Get-SelfCheckLeafJournalAttemptCount -Root ([string]$explicitNoRunRootRows[0].runRoot)) -eq 1) 'explicit-only no RunRoot did not write one observable canonical leaf.'
$explicitNoRunRootOutput = @(Get-WrapperOutputLines -Case $explicitNoRunRoot)
Assert-True (@($explicitNoRunRootOutput | Where-Object { $_ -like 'Основной каталог результатов для продолжения:*' }).Count -eq 1) 'explicit-only no RunRoot did not print the continuation path.'
Assert-True ($explicitNoRunRootOutput -contains 'Остаток общего MaxLeaves после дочернего запуска: 0') 'explicit-only no RunRoot did not report exact remaining MaxLeaves 0.'

# Сценарий 8b. Автоматический выбор без RunRoot запускает короткую проверку и
# основную группу. Короткий leaf списывает 1 из 2, основной leaf списывает
# последний остаток, а wrapper печатает точную последовательность 1 -> 0.
$changedNoRunRoot = New-CaseWorkspace -Name 'changed-focused-main-no-runroot-observed-budget' -FocusedManifestNames @('focused-a.tsv')
$changedNoRunRootExit = Invoke-WrapperCase -Case $changedNoRunRoot -Mode ChangedNoRunRoot -MaxLeaves 2 -Scenario 'complete'
Assert-True ($changedNoRunRootExit -eq 0) "changed no RunRoot exit code was $changedNoRunRootExit."
$changedNoRunRootRows = @(Get-InvocationRows -CaseRoot $changedNoRunRoot.Root)
Assert-True ($changedNoRunRootRows.Count -eq 2) "changed no RunRoot should run focused and canonical, got $($changedNoRunRootRows.Count)."
Assert-True ([bool]$changedNoRunRootRows[0].focused) 'changed no RunRoot first child should be focused.'
Assert-True (-not [bool]$changedNoRunRootRows[1].focused) 'changed no RunRoot second child should be canonical.'
Assert-True ([string]$changedNoRunRootRows[0].maxLeaves -eq '2') 'changed no RunRoot focused child should receive MaxLeaves 2.'
Assert-True ([string]$changedNoRunRootRows[1].maxLeaves -eq '1') 'changed no RunRoot canonical child should receive remaining MaxLeaves 1.'
Assert-True (([string]$changedNoRunRootRows[1].runRoot).StartsWith((Join-Path $changedNoRunRoot.Root 'TestResults\batches-wrapper-budget-self-check\run-'), [StringComparison]::OrdinalIgnoreCase)) 'changed no RunRoot chose an unexpected canonical RunRoot.'
Assert-True ((Get-SelfCheckLeafJournalAttemptCount -Root ([string]$changedNoRunRootRows[1].runRoot)) -eq 1) 'changed no RunRoot did not write one observable canonical leaf.'
$changedNoRunRootOutput = @(Get-WrapperOutputLines -Case $changedNoRunRoot)
Assert-True ($changedNoRunRootOutput -contains 'Остаток общего MaxLeaves после дочернего запуска: 1') 'changed no RunRoot did not report remaining MaxLeaves 1 after focused.'
Assert-True ($changedNoRunRootOutput -contains 'Остаток общего MaxLeaves после дочернего запуска: 0') 'changed no RunRoot did not report exact remaining MaxLeaves 0 after canonical.'

# Сценарий 9. LeafMode off не пишет leaf-журнал, поэтому положительный
# MaxLeaves нечем честно списывать между дочерними процессами. Такой режим
# отвергается до WSL; безлимитный MaxLeaves 0 остаётся разрешённым.
$offBounded = New-CaseWorkspace -Name 'leaf-mode-off-positive-budget-rejected' -FocusedManifestNames @('focused-a.tsv')
$offBoundedExit = Invoke-WrapperCase -Case $offBounded -Mode ExplicitOnly -MaxLeaves 1 -Scenario 'complete' -LeafMode off
Assert-True ($offBoundedExit -ne 0) "leaf-mode off with positive MaxLeaves should be rejected, got exit $offBoundedExit."
Assert-True (@(Get-WslCallRows -CaseRoot $offBounded.Root).Count -eq 0) 'leaf-mode off positive MaxLeaves should be rejected before WSL.'

$offUnlimited = New-CaseWorkspace -Name 'leaf-mode-off-unlimited-zero-preserved' -FocusedManifestNames @('focused-a.tsv')
$offUnlimitedExit = Invoke-WrapperCase -Case $offUnlimited -Mode ExplicitOnly -MaxLeaves 0 -Scenario 'complete' -LeafMode off
Assert-True ($offUnlimitedExit -eq 0) "leaf-mode off with MaxLeaves 0 should remain unlimited, got exit $offUnlimitedExit."
$offUnlimitedRows = @(Get-InvocationRows -CaseRoot $offUnlimited.Root)
Assert-True ($offUnlimitedRows.Count -eq 1) "leaf-mode off unlimited should run one canonical child, got $($offUnlimitedRows.Count)."
Assert-True ($offUnlimitedRows[0].runnerTokens -contains '--leaf-mode') 'leaf-mode off unlimited did not forward the leaf-mode argument.'
Assert-True ([string]$offUnlimitedRows[0].maxLeaves -eq '') 'leaf-mode off unlimited should not forward --max-leaves 0.'

# Сценарий 10. Если короткая обязательная проверка вернула exit 0, но не
# оставила нормальное состояние, красную строку или испортила journal.tsv,
# wrapper не имеет права считать её зелёной и стартовать основные группы.
$badRetry = New-CaseWorkspace -Name 'bad-focused-result-then-retry' -FocusedManifestNames @('focused-a.tsv')
$badExit = Invoke-WrapperCase -Case $badRetry -Mode Changed -MaxLeaves 1 -Scenario 'bad-missing'
Assert-True ($badExit -eq 65) "bad focused result should exit 65, got $badExit."
$badRows1 = @(Get-InvocationRows -CaseRoot $badRetry.Root)
Assert-True ($badRows1.Count -eq 1) "bad focused result should stop before canonical, got $($badRows1.Count) child invocations."
$retryExit = Invoke-WrapperCase -Case $badRetry -Mode Changed -MaxLeaves 1 -Scenario 'complete'
Assert-True ($retryExit -eq 0) "retry after bad focused result exit code was $retryExit."
$badRows2 = @(Get-InvocationRows -CaseRoot $badRetry.Root)
Assert-True ($badRows2.Count -eq 2) "retry after bad focused result should add exactly one focused invocation, got $($badRows2.Count)."
Assert-True ([string]$badRows2[0].runRoot -eq [string]$badRows2[1].runRoot) 'retry after bad focused result changed the focused RunRoot.'
Assert-True ([bool]$badRows2[1].focused) 'после плохого короткого результата повтор должен сначала снова проверить короткий manifest.'

$redFocused = New-CaseWorkspace -Name 'red-focused-stops-main' -FocusedManifestNames @('focused-a.tsv')
$redExit = Invoke-WrapperCase -Case $redFocused -Mode Changed -MaxLeaves 1 -Scenario 'red-focused'
Assert-True ($redExit -eq 65) "red focused result should exit 65, got $redExit."
Assert-True (@(Get-InvocationRows -CaseRoot $redFocused.Root).Count -eq 1) 'red focused result should stop before canonical.'

$corruptJournal = New-CaseWorkspace -Name 'corrupt-focused-journal-stops-main' -FocusedManifestNames @('focused-a.tsv')
$corruptExit = Invoke-WrapperCase -Case $corruptJournal -Mode Changed -MaxLeaves 1 -Scenario 'corrupt-journal'
Assert-True ($corruptExit -eq 65) "corrupt focused journal should exit 65, got $corruptExit."
Assert-True (@(Get-InvocationRows -CaseRoot $corruptJournal.Root).Count -eq 1) 'corrupt focused journal should stop before canonical.'

$resultPath = Join-Path $EvidenceRoot 'SELF_CHECK_RESULT.txt'
Write-TextFile -Path $resultPath -Value "WRAPPER_BUDGET_SELF_CHECK_OK`nwrapper_sha256=$wrapperHash`nevidence_root=$EvidenceRoot`nproof_scope=Самопроверка управления PowerShell через управляемую подмену выбора групп и запуска Linux (fake selector, fake WSL); это не продуктовые .NET тесты."
Write-Host "WRAPPER_BUDGET_SELF_CHECK_OK"
Write-Host $EvidenceRoot
