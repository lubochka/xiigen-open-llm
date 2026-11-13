# Lifecycle for the six P108–P113 callers. This is not the generic run-binding
# source-closure/consumed-input attestation. QA substitutes commands, not guards.
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Write-LauncherJson($Path, $Value) {
    $text = ($Value | ConvertTo-Json -Depth 40) -replace "`r`n", "`n"
    [IO.File]::WriteAllText($Path, $text.TrimEnd() + "`n", [Text.UTF8Encoding]::new($false))
}
function Read-LauncherFile($Path, [switch]$Json) {
    $bytes = [IO.File]::ReadAllBytes($Path)
    $identity = [ordered]@{ path = [IO.Path]::GetFullPath($Path); bytes = $bytes.Length
        sha256 = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant() }
    $value = $null
    if ($Json) {
        $value = [Text.UTF8Encoding]::new($false, $true).GetString($bytes) | ConvertFrom-Json -AsHashtable -ErrorAction Stop
        if ($value -isnot [Collections.IDictionary]) { throw 'JSON_OBJECT_REQUIRED' }
    }
    return @{ identity = $identity; value = $value }
}
function Assert-Field($Object, $Name, $Type) {
    if ($Object -isnot [Collections.IDictionary] -or -not $Object.Contains($Name)) { throw "MISSING_REQUIRED:$Name" }
    $v = $Object[$Name]
    $ok = switch ($Type) {
        'string' { $v -is [string] -and -not [string]::IsNullOrWhiteSpace($v) }
        'bool' { $v -is [bool] }
        'int' { $v -is [int] -or $v -is [long] }
        'number' { ($v -is [ValueType]) -and $v -isnot [bool] -and [double]::IsFinite([double]$v) }
        'object' { $v -is [Collections.IDictionary] }
        default { $false }
    }
    if (-not $ok) { throw "INVALID_TYPE:${Name}:$Type" }
    return $v
}
function Assert-OneOf($Value, $Allowed, $Name) {
    if ($Value -notin $Allowed) { throw "UNKNOWN_VALUE:$Name=$Value" }
}
function Get-ProcessIdentity([int]$ProcessId) {
    $p = Get-Process -Id $ProcessId -ErrorAction Stop
    return [ordered]@{ pid = $p.Id; start_time_utc = $p.StartTime.ToUniversalTime().ToString('o') }
}
function Test-ProcessIdentity($Identity) {
    try {
        $now = Get-ProcessIdentity $Identity.pid
        return $now.start_time_utc -eq $Identity.start_time_utc
    } catch {
        if (($_.FullyQualifiedErrorId -split ',')[0] -eq 'NoProcessFoundForGivenId') { return $false }
        throw "PROCESS_IDENTITY_NOT_OBSERVED:$($_.Exception.Message)"
    }
}
function Get-ProcessTree($RootIdentity) {
    $all = @(Get-CimInstance Win32_Process -ErrorAction Stop)
    $found = [Collections.Generic.List[object]]::new()
    $found.Add($RootIdentity)
    for ($i = 0; $i -lt $found.Count; $i++) {
        foreach ($p in $all | Where-Object ParentProcessId -eq $found[$i].pid) {
            if ($p.ProcessId -notin @($found | ForEach-Object pid)) {
                # A disappeared child cannot become an observed successful kill.
                $found.Add((Get-ProcessIdentity ([int]$p.ProcessId)))
            }
        }
    }
    return @($found.ToArray())
}
function Stop-LauncherTree($Process, $Observed, [bool]$RootOnly) {
    $errorText = $null
    try { $Process.Kill(-not $RootOnly); $null = $Process.WaitForExit(3000) } catch { $errorText = $_.Exception.Message }
    $checks = @()
    foreach ($identity in $Observed) {
        try { $alive = Test-ProcessIdentity $identity; $known = $true }
        catch { $alive = $null; $known = $false; $errorText = $_.Exception.Message }
        $checks += @{ identity = $identity; observed = $known; alive = $alive }
    }
    return @{ checks = $checks; error = $errorText
        stopped = ($checks.Count -gt 0 -and @($checks | Where-Object { -not $_.observed -or $_.alive }).Count -eq 0) }
}
function Invoke-LauncherStage($Context, $Stage, $Command, [double]$Cap) {
    $watch = [Diagnostics.Stopwatch]::StartNew()
    $remaining = if ($Context.total_cap -gt 0) { $Context.total_cap - $Context.clock.Elapsed.TotalSeconds } else { [double]::PositiveInfinity }
    $budget = [Math]::Min($Cap, $remaining)
    if ($budget -le 0) { return @{ stage = $Stage; outcome = 'TIMEOUT_TOTAL'; exit_code = $null; child = $null; seconds = 0; cleanup = $null } }
    $out = Join-Path $Context.invocation "$Stage.stdout.log"
    $err = Join-Path $Context.invocation "$Stage.stderr.log"
    $quoted = @($Command.arguments | ForEach-Object { '"' + ([string]$_).Replace('"','\"') + '"' })
    $process = Start-Process -FilePath $Command.file -ArgumentList $quoted -WorkingDirectory $Context.host_dir `
        -WindowStyle Hidden -PassThru -RedirectStandardOutput $out -RedirectStandardError $err
    $identity = Get-ProcessIdentity $process.Id
    $record = [ordered]@{ stage = $Stage; command = $Command; child = $identity; effective_cap_seconds = $budget
        stage_cap_seconds = $Cap; total_cap_seconds = $Context.total_cap; stdout = $out; stderr = $err }
    Write-LauncherJson (Join-Path $Context.invocation "$Stage.started.json") $record
    # Startup, identity capture and receipt IO consume both clocks too.
    $stageRemaining = $Cap - $watch.Elapsed.TotalSeconds
    $totalRemaining = if ($Context.total_cap -gt 0) { $Context.total_cap - $Context.clock.Elapsed.TotalSeconds } else { [double]::PositiveInfinity }
    $waitBudget = [Math]::Min($stageRemaining,$totalRemaining)
    $record.wait_budget_seconds = [Math]::Max(0,$waitBudget)
    if ($waitBudget -le 0 -or -not $process.WaitForExit([int][Math]::Floor($waitBudget * 1000))) {
        $record.outcome = if ($totalRemaining -le $stageRemaining) { 'TIMEOUT_TOTAL' } else { 'TIMEOUT_STAGE' }
        $record.exit_code = $null
        $record.seconds = $watch.Elapsed.TotalSeconds
        $record.partial_saved_before_stop = $true
        try { $tree = @(Get-ProcessTree $identity); $record.observed_tree = $tree }
        catch { $tree = @($identity); $record.tree_observation_error = $_.Exception.Message }
        Write-LauncherJson (Join-Path $Context.invocation "$Stage.partial.json") $record
        $record.cleanup = Stop-LauncherTree $process $tree $Context.root_only
        if ($record.Contains('tree_observation_error')) { $record.cleanup.stopped = $false }
    } else {
        $process.WaitForExit()
        $record.exit_code = $process.ExitCode
        $record.outcome = if ($process.ExitCode -eq 0) { 'CHILD_SUCCESS' } else { 'CHILD_FAILURE' }
        $record.seconds = $watch.Elapsed.TotalSeconds
        $record.cleanup = $null
    }
    Write-LauncherJson (Join-Path $Context.invocation "$Stage.receipt.json") $record
    return $record
}
function Assert-Terminal($Record, $Experiment, $Attempt, $Stage) {
    if ((Assert-Field $Record schema string) -ne 'experiment-terminal/v1') { throw 'TERMINAL_SCHEMA' }
    if ((Assert-Field $Record experiment string) -ne $Experiment) { throw 'FOREIGN_EXPERIMENT' }
    if ((Assert-Field $Record attempt string) -ne $Attempt) { throw 'FOREIGN_ATTEMPT' }
    if ((Assert-Field $Record stage string) -ne $Stage) { throw 'FOREIGN_STAGE' }
    if (-not (Assert-Field $Record terminal bool)) { throw 'NOT_TERMINAL' }
    $code = Assert-Field $Record exit_code int
    $outcome = Assert-Field $Record outcome string
    Assert-OneOf $outcome @('COMPLETED_SUCCESS','FAILED_TERMINAL','OUTCOME_UNKNOWN','TIMEOUT_STAGE','TIMEOUT_TOTAL','PROCESS_TREE_NOT_STOPPED') outcome
    $o = Assert-Field $Record outcomes object
    Assert-OneOf (Assert-Field $o process string) @('COMPLETED','FAILED','TIMEOUT','NOT_OBSERVED') process
    Assert-OneOf (Assert-Field $o measurement string) @('PASS','FAIL','NOT_OBSERVED') measurement
    Assert-OneOf (Assert-Field $o budget string) @('PASS','FAIL','NOT_OBSERVED') budget
    Assert-OneOf (Assert-Field $o learning string) @('VALID_FOR_NAMED_UNIT','UNKNOWN','NOT_OBSERVED','NOT_APPLICABLE') learning
    $null = Assert-Field $o scientific string
    $null = Assert-Field $Record reason string
    $null = Assert-Field $Record finished_utc string
    $owner = Assert-Field $Record owner object
    $null = Assert-Field $owner pid int
    $null = Assert-Field $owner start_time_utc string
    if (-not $Record.Contains('child_exit_code')) { throw 'MISSING_REQUIRED:child_exit_code' }
    if ($null -ne $Record.child_exit_code) { $null = Assert-Field $Record child_exit_code int }
    if ($code -eq 0 -and ($outcome -ne 'COMPLETED_SUCCESS' -or $Record.child_exit_code -ne 0 -or $o.process -ne 'COMPLETED' -or $o.measurement -ne 'PASS' -or $o.budget -ne 'PASS')) { throw 'CONTRADICTORY_SUCCESS' }
    if ($outcome -eq 'COMPLETED_SUCCESS' -and $code -ne 0) { throw 'CONTRADICTORY_FAILURE' }
    if ($code -lt 0 -or $code -gt 255) { throw 'EXIT_RANGE' }
    return $Record
}
function Get-StatusOutcomes($Status, $Experiment, $Attempt) {
    $o = [ordered]@{ process = 'NOT_OBSERVED'; measurement = 'NOT_OBSERVED'; budget = 'NOT_OBSERVED'; learning = 'NOT_OBSERVED'; scientific = 'NOT_OBSERVED' }
    $id = [int]$Experiment.Substring(1,3)
    if ($id -in @(108,109)) {
        if ((Assert-Field $Status 'опыт' string) -ne $Experiment) { throw 'STATUS_FOREIGN_EXPERIMENT' }
        $p = Assert-Field $Status execution_status string
    } elseif ($id -eq 111) {
        if ((Assert-Field $Status run_id string) -ne "$Experiment/$Attempt") { throw 'STATUS_FOREIGN_ATTEMPT' }
        $p = Assert-Field $Status process_status string
    } else {
        if ((Assert-Field $Status experiment string) -ne $Experiment) { throw 'STATUS_FOREIGN_EXPERIMENT' }
        $p = if ($id -eq 110) { Assert-Field $Status process_status string } else { Assert-Field $Status execution_status string }
    }
    Assert-OneOf $p @('COMPLETED','PARTIAL','FAILED') process
    $o.process = if ($p -eq 'COMPLETED') { 'COMPLETED' } else { 'FAILED' }
    if ($id -in @(108,110,111) -or ($id -eq 109 -and $Status.Contains('measurement_status') -and $Status.Contains('budget_status'))) {
        $m = Assert-Field $Status measurement_status string
        $b = Assert-Field $Status budget_status string
        # Prose in the old P108 receipt has no machine-verifiable enum semantics.
        $o.measurement = switch ($m) { { $_ -in @('PASS','VALID') } { 'PASS' } { $_ -in @('FAIL','FAILED','INVALID') } { 'FAIL' } default { 'NOT_OBSERVED' } }
        $o.budget = switch ($b) { { $_ -in @('PASS','WITHIN_ASSIGNMENT') } { 'PASS' } { $_ -in @('FAIL','FAILED','EXCEEDED') } { 'FAIL' } default { 'NOT_OBSERVED' } }
    } elseif ($id -in @(112,113)) {
        $o.measurement = if (Assert-Field $Status controls_accepted bool) { 'PASS' } else { 'FAIL' }
        $o.budget = if (Assert-Field $Status budget_exact bool) { 'PASS' } else { 'FAIL' }
        if ($id -eq 113 -and -not (Assert-Field $Status reducer_agrees_with_runtime bool)) { $o.measurement = 'FAIL' }
        $null = Assert-Field $Status exit_code int
    }
    if ($Status.Contains('learning_validity')) {
        $l = Assert-Field $Status learning_validity string
        Assert-OneOf $l @('VALID_FOR_NAMED_UNIT','UNKNOWN','NOT_OBSERVED','NOT_APPLICABLE') learning
        $o.learning = $l
    }
    if ($Status.Contains('hypothesis_result')) { $o.scientific = Assert-Field $Status hypothesis_result string }
    elseif ($Status.Contains('hypothesis_status')) { $o.scientific = Assert-Field $Status hypothesis_status string }
    return $o
}
function Get-FinalDecision($Outcomes, $ChildCode) {
    if ($ChildCode -ne 0) { return @{ outcome = 'FAILED_TERMINAL'; code = if ($ChildCode -gt 0 -and $ChildCode -le 255) { $ChildCode } else { 10 }; reason = 'CHILD_NONZERO' } }
    if ($Outcomes.measurement -eq 'FAIL') { return @{ outcome = 'FAILED_TERMINAL'; code = 21; reason = 'MEASUREMENT_FAILED' } }
    if ($Outcomes.budget -eq 'FAIL') { return @{ outcome = 'FAILED_TERMINAL'; code = 22; reason = 'BUDGET_FAILED' } }
    if ($Outcomes.process -ne 'COMPLETED' -or $Outcomes.measurement -ne 'PASS' -or $Outcomes.budget -ne 'PASS') { return @{ outcome = 'OUTCOME_UNKNOWN'; code = 11; reason = 'REQUIRED_OUTCOME_NOT_OBSERVED' } }
    return @{ outcome = 'COMPLETED_SUCCESS'; code = 0; reason = 'PROCESS_AND_REQUIRED_OUTCOMES_ACCEPTED' }
}
function Assert-ConsumedInputs($Declared, $Receipt) {
    if ((Assert-Field $Receipt schema string) -ne 'consumed-inputs/v1') { throw 'CONSUMER_SCHEMA' }
    if (-not $Receipt.Contains('inputs') -or $Receipt.inputs -isnot [array] -or $Receipt.inputs.Count -ne $Declared.Count) { throw 'CONSUMER_REQUIRED_SET' }
    $seen = @{}
    foreach ($inputIdentity in $Receipt.inputs) {
        $path = Assert-Field $inputIdentity path string
        if ($seen.ContainsKey($path)) { throw 'CONSUMER_DUPLICATE_INPUT' }
        $seen[$path] = $true
        $trusted = @($Declared | Where-Object path -eq $path)
        if ($trusted.Count -ne 1) { throw 'CONSUMER_FOREIGN_INPUT' }
        if (-not $inputIdentity.Contains('sha256')) { throw 'CONSUMER_DIGEST_MISSING' }
        $sha = Assert-Field $inputIdentity sha256 string
        $bytes = Assert-Field $inputIdentity bytes int
        if ($sha -cne $trusted[0].sha256 -or $bytes -ne $trusted[0].bytes) { throw 'CONSUMED_BYTES_CHANGED' }
    }
}

function Read-LegacyResult($Context) {
    $e = Read-LauncherFile (Join-Path $Context.output 'exit.json') -Json
    $r = Read-LauncherFile (Join-Path $Context.output 'run-1.receipt.json') -Json
    $s = Read-LauncherFile (Join-Path $Context.output 'status.json') -Json
    $receipt = $r.value; $exitRecord = $e.value
    if ((Assert-Field $receipt stage string) -ne 'прогон опыта') { throw 'LEGACY_STAGE' }
    $expectedCommand = "dotnet exec $($Context.host_dir)/bin/Release/net9.0/$($Context.host_name)Experiment.dll"
    if ((Assert-Field $receipt command string).Replace('\','/') -ne $expectedCommand.Replace('\','/')) { throw 'LEGACY_COMMAND_IDENTITY' }
    $rc = Assert-Field $receipt exit_code int
    $null = Assert-Field $receipt seconds number
    $id = [int]$Context.experiment.Substring(1,3)
    if ($id -in @(108,109)) {
        $ec = Assert-Field $exitRecord 'код_выхода' int
        $null = Assert-Field $exitRecord 'причина' string
        if ($id -eq 108) { $null = Assert-Field $exitRecord 'физическая_попытка_прогона' int }
        else { $null = Assert-Field $exitRecord 'стадия' string }
    } else {
        if ((Assert-Field $exitRecord experiment string) -ne $Context.experiment) { throw 'LEGACY_FOREIGN_EXPERIMENT' }
        $ec = Assert-Field $exitRecord exit_code int
        if ($id -eq 110) {
            if ((Assert-Field $receipt attempt string) -ne "$($Context.host_name)-attempt-0001") { throw 'LEGACY_RECEIPT_ATTEMPT' }
            $null = Assert-Field $exitRecord process_status string
            $null = Assert-Field $exitRecord stage_reached string
        } else {
            if ((Assert-Field $exitRecord subcase string) -ne "P$id.1") { throw 'LEGACY_SUBCASE' }
            $null = Assert-Field $exitRecord terminal_proof string
        }
    }
    if ($ec -ne $rc) { throw 'LEGACY_EXIT_RECEIPT_DISAGREE' }
    if ($s.value.Contains('exit_code') -and (Assert-Field $s.value exit_code int) -ne $ec) { throw 'LEGACY_STATUS_EXIT_DISAGREE' }
    # Old P111 embeds the original attempt ID. A copied legacy bundle retains it.
    $o = Get-StatusOutcomes $s.value $Context.experiment 'attempt-0001'
    $decision = Get-FinalDecision $o $ec
    return @{ outcome = $decision.outcome; exit_code = $decision.code; child_exit_code = $ec
        reason = $decision.reason; outcomes = $o; scope = 'LEGACY_RECORDED_OUTCOMES_NOT_FRESH_ACCEPTANCE'
        read_identities = @($e.identity,$r.identity,$s.identity); raw_status = $s.value }
}
function New-LauncherTerminal($Context, $Stage, $Decision, $Outcomes, $ChildCode, $Inputs) {
    $terminal = [ordered]@{ schema = 'experiment-terminal/v1'; experiment = $Context.experiment
        attempt = $Context.attempt; stage = $Stage; terminal = $true; outcome = $Decision.outcome
        exit_code = $Decision.code; child_exit_code = $ChildCode; reason = $Decision.reason
        outcomes = $Outcomes; owner = $Context.owner; finished_utc = [DateTime]::UtcNow.ToString('o')
        source_identities = $Context.sources; read_identities = @($Inputs); invocation = $Context.invocation }
    $null = Assert-Terminal $terminal $Context.experiment $Context.attempt $Stage
    Write-LauncherJson $Context.terminal $terminal
    return $terminal
}
function Invoke-ExperimentLauncher {
    [CmdletBinding()]
    param([string]$Experiment, [string]$HostDirectory, [string]$EntryPath, [string]$Mode,
        [double]$BuildDeadlineSeconds, [double]$RunDeadlineSeconds, [double]$TotalDeadlineSeconds = 0,
        [string]$QaStage = 'run', [string]$QaOutputRoot, [string]$QaAttemptId, [string]$QaFixture)
    $clock = [Diagnostics.Stopwatch]::StartNew()
    $qa = $Mode -eq 'qa'
    $root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
    $fixture = $null; $fixtureIdentity = $null
    try {
        Assert-OneOf $Mode @('all','build','run','verify-saved','selftest-launcher','qa') mode
        if ($Mode -in @('verify-saved','selftest-launcher') -and $Experiment -ne 'P108-PRIMARY-01') { throw 'MODE_NOT_SUPPORTED_BY_OWNER' }
        if ($Mode -eq 'selftest-launcher') { throw 'SELFTEST_REQUIRES_EXPLICIT_QA_ENTRY: scripts/experiment-launcher/qa/Invoke-LauncherAcceptance.ps1 -OutputRoot <new directory>' }
        if ($qa) {
            Assert-OneOf $QaStage @('all','build','run') qa_stage
            if ([string]::IsNullOrWhiteSpace($QaAttemptId) -or -not $QaAttemptId.StartsWith('Q03-')) { throw 'QA_ATTEMPT_REQUIRED' }
            $output = [IO.Path]::GetFullPath($QaOutputRoot)
            $allowed = [IO.Path]::GetFullPath((Join-Path $root 'qa-results/q03-execution')) + [IO.Path]::DirectorySeparatorChar
            if (-not $output.StartsWith($allowed,[StringComparison]::OrdinalIgnoreCase)) { throw 'QA_OUTPUT_OUTSIDE_ASSIGNED_ROOT' }
            $f = Read-LauncherFile $QaFixture -Json
            $fixture = $f.value; $fixtureIdentity = $f.identity
            if ((Assert-Field $fixture schema string) -ne 'q03-fixture/v1') { throw 'QA_FIXTURE_SCHEMA' }
            $effectiveMode = $QaStage; $attempt = $QaAttemptId
        } else {
            $output = Join-Path $HostDirectory $(if ($Mode -eq 'verify-saved') { 'results/m06-repair-0001' } else { 'results/attempt-0001' })
            $attempt = if ($Mode -eq 'verify-saved') { 'm06-repair-0001-verify-saved' } else { 'attempt-0001' }
            $effectiveMode = $Mode
        }
        foreach ($cap in @($BuildDeadlineSeconds,$RunDeadlineSeconds)) { if ($cap -le 0 -or -not [double]::IsFinite($cap)) { throw 'INVALID_DEADLINE' } }
        if ($TotalDeadlineSeconds -lt 0 -or -not [double]::IsFinite($TotalDeadlineSeconds)) { throw 'INVALID_TOTAL_DEADLINE' }
        $null = [IO.Directory]::CreateDirectory($output)
    } catch { Write-Host "OUTCOME_UNKNOWN: $($_.Exception.Message)"; return 11 }
    # Same lock and same terminal validator in ordinary and QA calls.
    try { $claim = [IO.File]::Open((Join-Path $output 'entry.claim'),[IO.FileMode]::OpenOrCreate,[IO.FileAccess]::ReadWrite,[IO.FileShare]::None) }
    catch { Write-Host 'CONCURRENT_ENTRY_REFUSED'; return 12 }
    $invocation = Join-Path $output ('invocations/' + [Guid]::NewGuid().ToString('N'))
    $null = [IO.Directory]::CreateDirectory($invocation)
    $stage = if ($Mode -eq 'verify-saved') { 'verify-saved' } else { 'run' }
    $terminalName = if ($stage -eq 'verify-saved') { 'terminal-verify-saved.json' } else { 'terminal.json' }
    $context = @{ experiment = $Experiment; attempt = $attempt; host_dir = [IO.Path]::GetFullPath($HostDirectory).Replace('\','/')
        host_name = Split-Path $HostDirectory -Leaf; output = $output; invocation = $invocation
        clock = $clock; total_cap = $TotalDeadlineSeconds; root_only = $false; owner = Get-ProcessIdentity $PID
        terminal = Join-Path $output $terminalName; declared_inputs = @(); sources = @((Read-LauncherFile $EntryPath).identity,(Read-LauncherFile $PSCommandPath).identity) }
    $result = $null
    try {
        if ($qa) {
            $context.sources += $fixtureIdentity
            $context.declared_inputs = @($fixtureIdentity)
            $context.root_only = $fixture.Contains('root_only_stop') -and (Assert-Field $fixture root_only_stop bool)
        }
        $claimBytes = [Text.UTF8Encoding]::new($false).GetBytes(($context.owner | ConvertTo-Json -Compress))
        $claim.SetLength(0); $claim.Write($claimBytes,0,$claimBytes.Length); $claim.Flush($true)
        Write-LauncherJson (Join-Path $invocation 'claim-acquired.json') @{ owner = $context.owner; experiment = $Experiment; attempt = $attempt }
        if ($qa -and $fixture.Contains('hold_claim_ms')) { Start-Sleep -Milliseconds (Assert-Field $fixture hold_claim_ms int) }
        if ($effectiveMode -ne 'build') {
            if (Test-Path -LiteralPath $context.terminal) {
                $previous = Read-LauncherFile $context.terminal -Json
                $t = Assert-Terminal $previous.value $Experiment $attempt $stage
                $result = @{ outcome = $t.outcome; exit_code = $t.exit_code; child_exit_code = $t.child_exit_code
                    reason = 'EXISTING_TERMINAL_NO_CALLBACK'; read_identities = @($previous.identity); outcomes = $t.outcomes }
                return [int]$result.exit_code
            }
            if ($stage -eq 'run' -and (Test-Path -LiteralPath (Join-Path $output 'exit.json'))) {
                $result = Read-LegacyResult $context
                return [int]$result.exit_code
            }
            $markers = if ($stage -eq 'run') { @('runtime.stdout.log','runtime.stdout.raw','run-1.receipt.json','run.started.json') } else { @('verify-saved.stdout.log','verify-saved.receipt.json','verify-saved.started.json') }
            foreach ($marker in $markers) { if (Test-Path -LiteralPath (Join-Path $output $marker)) { throw 'PRIOR_ACTIVITY_WITHOUT_VALID_TERMINAL' } }
        }
        $shell = (Get-Process -Id $PID).Path
        $dummy = Join-Path $PSScriptRoot 'qa/DummyProgram.ps1'
        $artifact = if ($qa) { Join-Path $output 'dummy-artifact.bin' } else { Join-Path $HostDirectory "bin/Release/net9.0/$($context.host_name)Experiment.dll" }
        if ($effectiveMode -in @('all','build','verify-saved')) {
            $inputs = @($context.sources)
            if ($qa) { $inputs += (Read-LauncherFile $dummy).identity }
            else {
                # Explicit own-source scope; binlog is the build's full input evidence.
                $own = @(Get-ChildItem $HostDirectory -File | Where-Object Extension -in @('.cs','.csproj'))
                if (Test-Path (Join-Path $HostDirectory 'src')) { $own += @(Get-ChildItem (Join-Path $HostDirectory 'src') -Recurse -File) }
                $inputs += @($own | ForEach-Object { (Read-LauncherFile $_.FullName).identity })
            }
            $command = if ($qa) { @{ file = $shell; arguments = @('-NoProfile','-File',$dummy,'-Stage','build','-Fixture',$QaFixture,'-OutputRoot',$output,'-Experiment',$Experiment,'-Attempt',$attempt) } }
                else { @{ file = 'dotnet'; arguments = @('build',(Join-Path $HostDirectory 'Experiment.csproj'),'-c','Release','--nologo','-v','minimal',"-bl:$invocation/build.binlog") } }
            $build = Invoke-LauncherStage $context 'build' $command $BuildDeadlineSeconds
            $built = $build.outcome -eq 'CHILD_SUCCESS' -and (Test-Path -LiteralPath $artifact)
            $buildReceipt = @{ schema = 'experiment-build/v1'; experiment = $Experiment; attempt = $attempt; stage = 'build'
                outcome = if ($built) { 'BUILT_ONLY' } else { $build.outcome }; process = $build; inputs = $inputs
                input_scope = if ($qa) { 'EXACT_DUMMY_COMMAND' } else { 'OWN_SOURCE_PLUS_BUILD_BINLOG_NOT_GENERIC_CLOSURE' }
                artifact = if ($built) { (Read-LauncherFile $artifact).identity } else { $null } }
            Write-LauncherJson (Join-Path $invocation 'build-result.json') $buildReceipt
            if (-not $built) {
                $code = if ($null -ne $build.exit_code -and $build.exit_code -ne 0) { $build.exit_code } elseif ($build.outcome -eq 'TIMEOUT_TOTAL') { 125 } elseif ($build.outcome -eq 'TIMEOUT_STAGE') { 124 } else { 11 }
                $result = @{ outcome = $buildReceipt.outcome; exit_code = $code; reason = 'BUILD_NOT_EXPERIMENT_TERMINAL' }
                return [int]$code
            }
            if ($effectiveMode -eq 'build') { $result = @{ outcome = 'BUILT_ONLY'; exit_code = 0; reason = 'NO_RUN_TERMINAL_WRITTEN' }; return 0 }
        }
        if (-not $qa -and -not (Test-Path -LiteralPath $artifact)) { throw 'ENTRY_ARTIFACT_MISSING' }
        $command = if ($qa) { @{ file = $shell; arguments = @('-NoProfile','-File',$dummy,'-Stage','run','-Fixture',$QaFixture,'-OutputRoot',$output,'-Experiment',$Experiment,'-Attempt',$attempt) } }
            elseif ($stage -eq 'verify-saved') { @{ file = 'dotnet'; arguments = @('exec',$artifact,'--verify-saved',(Join-Path $HostDirectory 'results/attempt-0001'),$output) } }
            else { @{ file = 'dotnet'; arguments = @('exec',$artifact) } }
        Write-LauncherJson (Join-Path $output "$stage.started.json") @{ invocation = $invocation; owner = $context.owner; command = $command }
        $run = Invoke-LauncherStage $context $stage $command $RunDeadlineSeconds
        $o = @{ process = 'NOT_OBSERVED'; measurement = 'NOT_OBSERVED'; budget = 'NOT_OBSERVED'; learning = 'NOT_OBSERVED'; scientific = 'NOT_OBSERVED' }
        $inputs = @()
        if ($run.outcome -in @('TIMEOUT_STAGE','TIMEOUT_TOTAL')) {
            $o.process = 'TIMEOUT'; $o.budget = 'FAIL'
            $decision = @{ outcome = $run.outcome; code = if ($run.outcome -eq 'TIMEOUT_STAGE') { 124 } else { 125 }; reason = 'DEADLINE_EXCEEDED' }
            if ($null -ne $run.cleanup -and -not $run.cleanup.stopped) { $decision = @{ outcome = 'PROCESS_TREE_NOT_STOPPED'; code = 126; reason = 'DESCENDANT_ALIVE_OR_NOT_OBSERVED' } }
        } else {
            try {
                if ($context.declared_inputs.Count -gt 0) {
                    $consumed = Read-LauncherFile (Join-Path $output 'consumed-inputs.json') -Json
                    $inputs += $consumed.identity
                    Assert-ConsumedInputs $context.declared_inputs $consumed.value
                }
                if ($stage -eq 'verify-saved') {
                    # Native verifier exit is process evidence only; it is not PRIMARY learning.
                    $o.process = if ($run.exit_code -eq 0) { 'COMPLETED' } else { 'FAILED' }
                    $o.measurement = if ($run.exit_code -eq 0) { 'PASS' } else { 'FAIL' }
                    $o.budget = 'PASS'; $o.learning = 'NOT_APPLICABLE'; $o.scientific = 'NOT_APPLICABLE_SAVED_VERIFIER'
                } else {
                    $status = Read-LauncherFile (Join-Path $output 'status.json') -Json
                    $inputs += $status.identity
                    $o = Get-StatusOutcomes $status.value $Experiment $attempt
                    if ($status.value.Contains('exit_code') -and (Assert-Field $status.value exit_code int) -ne $run.exit_code) { throw 'STATUS_CHILD_EXIT_DISAGREE' }
                }
                $decision = Get-FinalDecision $o $run.exit_code
            } catch {
                $o.process = if ($run.exit_code -eq 0) { 'COMPLETED' } else { 'FAILED' }
                $decision = if ($run.exit_code -ne 0) { Get-FinalDecision $o $run.exit_code } else { @{ outcome = 'OUTCOME_UNKNOWN'; code = 11; reason = $_.Exception.Message } }
            }
        }
        $result = New-LauncherTerminal $context $stage $decision $o $run.exit_code $inputs
        return [int]$result.exit_code
    } catch {
        $result = @{ outcome = 'OUTCOME_UNKNOWN'; exit_code = 11; reason = $_.Exception.Message }
        Write-Host "OUTCOME_UNKNOWN: $($_.Exception.Message)"
        return 11
    } finally {
        if ($null -ne $result) { Write-LauncherJson (Join-Path $invocation 'entry-result.json') $result }
        $claim.Dispose()
    }
}
Export-ModuleMember -Function Invoke-ExperimentLauncher, Read-LauncherFile, Write-LauncherJson, Assert-Terminal, Get-ProcessIdentity, Test-ProcessIdentity
