# Fixed expectations are defined here before execution, never learned from output.
[CmdletBinding()]
param([Parameter(Mandatory)][string]$OutputRoot)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot '../ExperimentLauncher.psm1') -Force
$repo = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
$OutputRoot = [IO.Path]::GetFullPath($OutputRoot)
$runRoot = Join-Path $OutputRoot ('execution-' + [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffZ'))
if (Test-Path -LiteralPath $runRoot) { throw 'OUTPUT_ALREADY_EXISTS' }
$null = [IO.Directory]::CreateDirectory($runRoot)
$shell = (Get-Process -Id $PID).Path
$watch = [Diagnostics.Stopwatch]::StartNew()
$script:entries = 0
$script:cases = [Collections.Generic.List[object]]::new()
$script:processes = [Collections.Generic.List[object]]::new()
$self = Get-ProcessIdentity $PID
$requiredCaseRead = Read-LauncherFile (Join-Path $OutputRoot 'expected-cases.json') -Json
$requiredIds = @($requiredCaseRead.value.ids)
if ($requiredIds.Count -ne 68 -or @($requiredIds | Sort-Object -Unique).Count -ne 68) { throw 'INDEPENDENT_REQUIRED_CASE_SET_INVALID' }
Write-LauncherJson (Join-Path $runRoot 'harness-start.json') @{ owner = $self; source = (Read-LauncherFile $PSCommandPath).identity; cap_seconds = 600 }

function New-Case($Name, [int]$Owner = 108, $Fixture = @{}) {
    $directory = Join-Path $runRoot $Name
    $null = [IO.Directory]::CreateDirectory($directory)
    $fixture.schema = 'q03-fixture/v1'
    $fixturePath = Join-Path $directory 'fixture.json'
    Write-LauncherJson $fixturePath $Fixture
    return @{ name = $Name; owner = $Owner; output = $directory; fixture = $fixturePath; attempt = "Q03-$Name" }
}
function Start-Entry($Case, $Stage = 'run', $Caps = @(30,30,0)) {
    if ($watch.Elapsed.TotalSeconds -ge 600) { throw 'HARNESS_TOTAL_DEADLINE' }
    $script:entries++
    $prefix = Join-Path $Case.output ("outer-{0:D3}" -f $script:entries)
    $entry = Join-Path $repo "experiments-v2/RP01-MICRO/P$($Case.owner)01/Run-Experiment.ps1"
    $arguments = @('-NoProfile','-File',$entry,'-Mode','qa','-QaStage',$Stage,'-QaOutputRoot',$Case.output,'-QaAttemptId',$Case.attempt,'-QaFixture',$Case.fixture,
        '-BuildDeadlineSeconds',[string]$Caps[0],'-RunDeadlineSeconds',[string]$Caps[1],'-TotalDeadlineSeconds',[string]$Caps[2])
    $p = Start-Process $shell -ArgumentList @($arguments | ForEach-Object { '"' + $_ + '"' }) -WindowStyle Hidden -PassThru `
        -RedirectStandardOutput "$prefix.stdout.log" -RedirectStandardError "$prefix.stderr.log"
    $record = @{ kind = 'entrypoint'; owner = Get-ProcessIdentity $p.Id; command = @{ file = $shell; arguments = $arguments }
        source = (Read-LauncherFile $entry).identity; fixture = (Read-LauncherFile $Case.fixture).identity
        started_utc = [DateTime]::UtcNow.ToString('o'); stdout = "$prefix.stdout.log"; stderr = "$prefix.stderr.log" }
    Write-LauncherJson "$prefix.started.json" $record
    return @{ process = $p; record = $record; prefix = $prefix; watch = [Diagnostics.Stopwatch]::StartNew() }
}
function Finish-Entry($Handle) {
    $remaining = [Math]::Min([double]30,600 - $watch.Elapsed.TotalSeconds)
    if ($remaining -le 0 -or -not $Handle.process.WaitForExit([int]($remaining * 1000))) {
        $Handle.process.Kill($true); $Handle.process.WaitForExit()
        $Handle.record.outer_timeout = $true
    }
    $Handle.process.WaitForExit()
    $Handle.record.exit_code = $Handle.process.ExitCode
    $Handle.record.elapsed_seconds = $Handle.watch.Elapsed.TotalSeconds
    $Handle.record.finished_utc = [DateTime]::UtcNow.ToString('o')
    Write-LauncherJson "$($Handle.prefix).receipt.json" $Handle.record
    $script:processes.Add($Handle.record)
    return $Handle.record
}
function Invoke-Entry($Case, $Stage = 'run', $Caps = @(30,30,0)) { return Finish-Entry (Start-Entry $Case $Stage $Caps) }
function Get-Callbacks($Case, $Stage = 'run') {
    $path = Join-Path $Case.output 'callbacks.ndjson'
    if (-not (Test-Path -LiteralPath $path)) { return 0 }
    return @([IO.File]::ReadAllLines($path) | Where-Object { $_ } | ForEach-Object { $_ | ConvertFrom-Json } | Where-Object stage -eq $Stage).Count
}
function Add-Case($Case, $Expected, $Actual, [bool]$Passed) {
    $r = @{ id = $Case.name; expected = $Expected; actual = $Actual; passed = $Passed; directory = $Case.output }
    $script:cases.Add($r)
    Write-LauncherJson (Join-Path $Case.output 'case-result.json') $r
    Write-Host "$($Case.name): $Passed"
}
function New-TerminalFixture($Case) {
    return @{ schema = 'experiment-terminal/v1'; experiment = "P$($Case.owner)-PRIMARY-01"; attempt = $Case.attempt
        stage = 'run'; terminal = $true; outcome = 'COMPLETED_SUCCESS'; exit_code = 0; child_exit_code = 0
        outcomes = @{ process = 'COMPLETED'; measurement = 'PASS'; budget = 'PASS'; learning = 'NOT_APPLICABLE'; scientific = 'NO_OBSERVED_GAIN' }
        reason = 'FROZEN_SYNTHETIC_TERMINAL'; owner = $self; finished_utc = '2026-11-04T00:00:00Z' }
}
function Write-Material($Directory, $Mutation) {
    $m = @{ schema = 'q03-material/v1'; arms = @(
        @{ id = 'A'; qualified = $true; completed = $true; checkpoint = 'A.checkpoint.txt'; reader_completed = $true },
        @{ id = 'B'; qualified = $true; completed = $true; checkpoint = 'B.checkpoint.txt'; reader_completed = $true }) }
    foreach ($id in @('A','B')) { [IO.File]::WriteAllText((Join-Path $Directory "$id.checkpoint.txt"),"Q03-checkpoint-$id`n",[Text.UTF8Encoding]::new($false)) }
    switch ($Mutation) {
        'qualification' { $m.arms[0].qualified = $false }
        'arm' { $m.arms[0].completed = $false }
        'checkpoint' { $m.arms[0].checkpoint = $null }
        'reader' { $m.arms[0].reader_completed = $false }
        'missing' { $m.arms = @($m.arms[0]) }
    }
    $p = Join-Path $Directory 'material.json'; Write-LauncherJson $p $m; return $p
}
function Wait-Claim($Case, $Handle) {
    $limit = [Diagnostics.Stopwatch]::StartNew()
    while ($limit.Elapsed.TotalSeconds -lt 8 -and -not $Handle.process.HasExited) {
        $claims = @(Get-ChildItem (Join-Path $Case.output 'invocations') -Filter 'claim-acquired.json' -Recurse -ErrorAction SilentlyContinue)
        if ($claims.Count -gt 0) { return $true }
        Start-Sleep -Milliseconds 50
    }
    return $false
}

try {
    foreach ($owner in 108..113) {
        $c = New-Case "P$owner-build-run-repeat" $owner
        $a = Invoke-Entry $c 'build'
        $noTerminal = -not (Test-Path (Join-Path $c.output 'terminal.json'))
        $br = @(Get-ChildItem $c.output -Recurse -Filter 'build-result.json' | ForEach-Object { (Read-LauncherFile $_.FullName -Json).value })
        $b = Invoke-Entry $c; $before = Get-Callbacks $c
        $d = Invoke-Entry $c; $after = Get-Callbacks $c
        Add-Case $c @{ exits = @(0,0,0); callbacks = 1; build = 'BUILT_ONLY'; terminal_after_build = $false } `
            @{ exits = @($a.exit_code,$b.exit_code,$d.exit_code); callbacks = $after; build = $br; no_terminal_after_build = $noTerminal } `
            ($a.exit_code -eq 0 -and $b.exit_code -eq 0 -and $d.exit_code -eq 0 -and $before -eq 1 -and $after -eq 1 -and $noTerminal -and $br.Count -eq 1 -and $br[0].outcome -eq 'BUILT_ONLY')
        $c = New-Case "P$owner-failure-build-repeat" $owner @{ child_exit = 7 }
        $a = Invoke-Entry $c
        $terminalPath = Join-Path $c.output 'terminal.json'
        $hashBefore = if (Test-Path $terminalPath) { (Read-LauncherFile $terminalPath).identity.sha256 } else { 'ABSENT' }
        $b = Invoke-Entry $c 'build'; $d = Invoke-Entry $c
        $hashAfter = if (Test-Path $terminalPath) { (Read-LauncherFile $terminalPath).identity.sha256 } else { 'ABSENT' }
        $count = Get-Callbacks $c
        Add-Case $c @{ exits = @(7,0,7); callbacks = 1; terminal_unchanged = $true } `
            @{ exits = @($a.exit_code,$b.exit_code,$d.exit_code); callbacks = $count; before = $hashBefore; after = $hashAfter } `
            ($a.exit_code -eq 7 -and $b.exit_code -eq 0 -and $d.exit_code -eq 7 -and $count -eq 1 -and $hashBefore -ne 'ABSENT' -and $hashBefore -eq $hashAfter)
    }
    foreach ($kind in @('malformed','foreign','missing','null','array','string','false-string','build-stage')) {
        $c = New-Case "terminal-$kind"; $t = New-TerminalFixture $c
        switch ($kind) {
            'foreign' { $t.attempt = 'Q03-foreign' }
            'missing' { $t.outcomes.Remove('measurement') }
            'false-string' { $t.terminal = 'false' }
            'build-stage' { $t.stage = 'build' }
        }
        $path = Join-Path $c.output 'terminal.json'
        switch ($kind) {
            'malformed' { [IO.File]::WriteAllText($path,'{') }
            'null' { [IO.File]::WriteAllText($path,'null') }
            'array' { [IO.File]::WriteAllText($path,'[]') }
            'string' { [IO.File]::WriteAllText($path,'"terminal"') }
            default { Write-LauncherJson $path $t }
        }
        $r = Invoke-Entry $c; $n = Get-Callbacks $c
        Add-Case $c @{ exit = 11; callbacks = 0 } @{ exit = $r.exit_code; callbacks = $n } ($r.exit_code -eq 11 -and $n -eq 0)
    }
    $c = New-Case 'stdout-without-terminal'; [IO.File]::WriteAllText((Join-Path $c.output 'runtime.stdout.log'),'existing paid activity')
    $r = Invoke-Entry $c; $n = Get-Callbacks $c
    Add-Case $c @{ exit = 11; callbacks = 0 } @{ exit = $r.exit_code; callbacks = $n } ($r.exit_code -eq 11 -and $n -eq 0)

    foreach ($caseSpec in @(@('scientific-zero',0),@('measurement-failure',21),@('budget-failure',22),@('missing-status',11))) {
        $f = @{}
        switch ($caseSpec[0]) { 'scientific-zero' { $f.scientific = 'NO_OBSERVED_GAIN' } 'measurement-failure' { $f.measurement = 'FAIL' } 'budget-failure' { $f.budget = 'FAIL' } 'missing-status' { $f.omit_status = $true } }
        $c = New-Case $caseSpec[0] 108 $f
        if ($caseSpec[0] -eq 'scientific-zero') { $f.verifier_input = Write-Material $c.output ''; Write-LauncherJson $c.fixture $f }
        $r = Invoke-Entry $c; $n = Get-Callbacks $c
        $chain = $true
        if ($caseSpec[0] -eq 'scientific-zero') {
            $v = (Read-LauncherFile (Join-Path $c.output 'verifier-result.json') -Json).value
            $p = (Read-LauncherFile (Join-Path $c.output 'program-result.json') -Json).value
            $chain = $v.exit_code -eq 0 -and $p.exit_code -eq 0 -and $v.owner.pid -ne $p.owner.pid
        }
        Add-Case $c @{ exit = $caseSpec[1]; callbacks = 1 } @{ exit = $r.exit_code; callbacks = $n; verifier_chain = $chain } ($r.exit_code -eq $caseSpec[1] -and $n -eq 1 -and $chain)
    }
    $reasons = @{ qualification = 'QUALIFICATION_REFUSED'; arm = 'ARM_RESULT_REFUSED'; checkpoint = 'CHECKPOINT_REQUIRED'; reader = 'READER_REFUSED'; missing = 'REQUIRED_ARM_SET' }
    foreach ($kind in @('qualification','arm','checkpoint','reader','missing')) {
        $c = New-Case "verifier-$kind"
        Write-LauncherJson $c.fixture @{ schema = 'q03-fixture/v1'; verifier_input = Write-Material $c.output $kind }
        $r = Invoke-Entry $c
        $v = (Read-LauncherFile (Join-Path $c.output 'verifier-result.json') -Json).value
        $p = (Read-LauncherFile (Join-Path $c.output 'program-result.json') -Json).value
        Add-Case $c @{ exits = @(9,9,9); reason = $reasons[$kind] } @{ exits = @($v.exit_code,$p.exit_code,$r.exit_code); reason = $v.reason } `
            ($v.exit_code -eq 9 -and $p.exit_code -eq 9 -and $r.exit_code -eq 9 -and $v.reason -eq $reasons[$kind] -and $v.owner.pid -ne $p.owner.pid)
    }
    foreach ($owner in 108..113) {
        foreach ($kind in @('honest','failure7','missing-code','string-code','disagreement')) {
            $c = New-Case "legacy-P$owner-$kind" $owner
            $historical = Join-Path $repo "experiments-v2/RP01-MICRO/P${owner}01/results/attempt-0001"
            foreach ($file in @('exit.json','run-1.receipt.json','status.json')) { [IO.File]::Copy((Join-Path $historical $file),(Join-Path $c.output $file)) }
            $e = (Read-LauncherFile (Join-Path $c.output 'exit.json') -Json).value
            $rr = (Read-LauncherFile (Join-Path $c.output 'run-1.receipt.json') -Json).value
            $s = (Read-LauncherFile (Join-Path $c.output 'status.json') -Json).value
            $field = if ($owner -lt 110) { 'код_выхода' } else { 'exit_code' }
            switch ($kind) {
                'failure7' { $e[$field] = 7; $rr.exit_code = 7; if ($s.Contains('exit_code')) { $s.exit_code = 7 } }
                'missing-code' { $e.Remove($field) }
                'string-code' { $e[$field] = '0' }
                'disagreement' { $e[$field] = 7 }
            }
            if ($kind -ne 'honest') { Write-LauncherJson (Join-Path $c.output 'exit.json') $e; Write-LauncherJson (Join-Path $c.output 'run-1.receipt.json') $rr; Write-LauncherJson (Join-Path $c.output 'status.json') $s }
            $expected = if ($kind -eq 'honest') { if ($owner -lt 110) { 11 } else { 0 } } elseif ($kind -eq 'failure7') { 7 } else { 11 }
            $r = Invoke-Entry $c; $n = Get-Callbacks $c
            Add-Case $c @{ exit = $expected; callbacks = 0; fresh_science = $false } @{ exit = $r.exit_code; callbacks = $n } ($r.exit_code -eq $expected -and $n -eq 0)
        }
    }
    foreach ($kind in @('stage','total','tree','tree-negative')) {
        $f = @{ run_delay_ms = 20000 }
        if ($kind -eq 'total') { $f.build_delay_ms = 300 }
        if ($kind -in @('tree','tree-negative')) { $f.spawn_grandchild = $true }
        if ($kind -eq 'tree-negative') { $f.root_only_stop = $true }
        $c = New-Case "deadline-$kind" $(if ($kind -eq 'total') { 110 } else { 108 }) $f
        $caps = if ($kind -eq 'total') { @(30,30,4) } elseif ($kind -in @('tree','tree-negative')) { @(30,5,0) } else { @(30,4,0) }
        $r = Invoke-Entry $c $(if ($kind -eq 'total') { 'all' } else { 'run' }) $caps
        $expected = switch ($kind) { 'total' { 125 } 'tree-negative' { 126 } default { 124 } }
        $treeGood = $true; $cleanup = $null
        if ($kind -in @('tree','tree-negative')) {
            $gi = (Read-LauncherFile (Join-Path $c.output 'grandchild-identity.json') -Json).value
            $live = Test-ProcessIdentity $gi
            $partial = @(Get-ChildItem $c.output -Recurse -Filter 'run.receipt.json' | ForEach-Object { (Read-LauncherFile $_.FullName -Json).value })[0]
            $bound = @($partial.observed_tree | Where-Object { $_.pid -eq $gi.pid -and $_.start_time_utc -eq $gi.start_time_utc }).Count -eq 1
            $treeGood = $bound -and $(if ($kind -eq 'tree-negative') { $live -and -not $partial.cleanup.stopped } else { -not $live -and $partial.cleanup.stopped })
            # First freeze the failed observation. Emergency cleanup never repairs it.
            Write-LauncherJson (Join-Path $c.output 'tree-observation-before-emergency-cleanup.json') @{ identity = $gi; alive = $live; bound = $bound; original_stop_passed = $partial.cleanup.stopped }
            if ($live) { $p = Get-Process -Id $gi.pid; if (Test-ProcessIdentity $gi) { $p.Kill($true); $null = $p.WaitForExit(3000) } }
            $cleanup = @{ identity = $gi; separate_cleanup_needed = $live; absent_after_cleanup = -not (Test-ProcessIdentity $gi) }
            Write-LauncherJson (Join-Path $c.output 'emergency-cleanup.json') $cleanup
            $treeGood = $treeGood -and $cleanup.absent_after_cleanup
        }
        Add-Case $c @{ exit = $expected; original_negative_must_fail = $kind -eq 'tree-negative' } @{ exit = $r.exit_code; tree_contract = $treeGood; cleanup = $cleanup } ($r.exit_code -eq $expected -and $treeGood)
    }
    foreach ($kind in @('changed-after-claim','missing-consumer-digest')) {
        $f = if ($kind -eq 'changed-after-claim') { @{ hold_claim_ms = 3000; payload = 'A' } } else { @{ omit_consumer_digest = $true } }
        $c = New-Case "consumed-$kind" 108 $f
        $declared = (Read-LauncherFile $c.fixture).identity
        if ($kind -eq 'changed-after-claim') {
            $h = Start-Entry $c
            $claimObserved = Wait-Claim $c $h
            $f.payload = 'B'; Write-LauncherJson $c.fixture $f
            $r = Finish-Entry $h
        } else { $claimObserved = $true; $r = Invoke-Entry $c }
        $terminal = (Read-LauncherFile (Join-Path $c.output 'terminal.json') -Json).value
        $reason = if ($kind -eq 'changed-after-claim') { 'CONSUMED_BYTES_CHANGED' } else { 'CONSUMER_DIGEST_MISSING' }
        Add-Case $c @{ exit = 11; reason = $reason } @{ exit = $r.exit_code; reason = $terminal.reason; declared = $declared; claim_observed = $claimObserved } `
            ($claimObserved -and $r.exit_code -eq 11 -and $terminal.reason -eq $reason)
    }
    foreach ($existing in @($false,$true)) {
        $c = New-Case $(if ($existing) { 'concurrent-existing-success' } else { 'concurrent-live-run' }) 108 @{ hold_claim_ms = 3000 }
        if ($existing) { Write-LauncherJson (Join-Path $c.output 'terminal.json') (New-TerminalFixture $c) }
        $first = Start-Entry $c
        $observed = Wait-Claim $c $first
        $second = Invoke-Entry $c
        $done = Finish-Entry $first; $n = Get-Callbacks $c
        $expectedCount = if ($existing) { 0 } else { 1 }
        Add-Case $c @{ exits = @(0,12); callbacks = $expectedCount } @{ exits = @($done.exit_code,$second.exit_code); callbacks = $n; claim_observed = $observed } `
            ($observed -and $done.exit_code -eq 0 -and $second.exit_code -eq 12 -and $n -eq $expectedCount)
    }
} catch {
    Write-LauncherJson (Join-Path $runRoot 'harness-failure.json') @{ error = $_.Exception.Message; stack = $_.ScriptStackTrace; elapsed_seconds = $watch.Elapsed.TotalSeconds }
    Write-Host "HARNESS_FAILURE: $($_.Exception.Message)"
} finally {
    $actualIds = @($script:cases | ForEach-Object id)
    $missing = @($requiredIds | Where-Object { $_ -notin $actualIds })
    $extra = @($actualIds | Where-Object { $_ -notin $requiredIds })
    $duplicates = @($actualIds | Group-Object | Where-Object Count -gt 1)
    $passed = $script:cases.Count -eq 68 -and $missing.Count -eq 0 -and $extra.Count -eq 0 -and $duplicates.Count -eq 0 -and @($script:cases | Where-Object { -not $_.passed }).Count -eq 0
    $observedChildren = @(Get-ChildItem $runRoot -Recurse -Filter '*-process-*.json' | ForEach-Object { (Read-LauncherFile $_.FullName -Json).value })
    $verifiers = @(Get-ChildItem $runRoot -Recurse -Filter 'verifier-process.json' | ForEach-Object { (Read-LauncherFile $_.FullName -Json).value })
    $stageReceipts = @(Get-ChildItem $runRoot -Recurse -Filter '*.receipt.json' | Where-Object Name -in @('build.receipt.json','run.receipt.json') | ForEach-Object { (Read-LauncherFile $_.FullName -Json).value })
    $counts = @{ harness = 1; entrypoints = $script:entries; build = @($observedChildren | Where-Object stage -eq 'build').Count
        run = @($observedChildren | Where-Object stage -eq 'run').Count; verifier = $verifiers.Count
        grandchild = @($observedChildren | Where-Object stage -eq 'grandchild').Count }
    $counts.total = $counts.harness + $counts.entrypoints + $counts.build + $counts.run + $counts.verifier + $counts.grandchild
    $expectedCounts = @{ harness = 1; entrypoints = 94; build = 13; run = 28; verifier = 6; grandchild = 2; total = 144 }
    $countMismatch = @($expectedCounts.Keys | Where-Object { $counts[$_] -ne $expectedCounts[$_] })
    $outerTimeouts = @($script:processes | Where-Object { $_.ContainsKey('outer_timeout') -and $_.outer_timeout })
    $unboundChildren = @($observedChildren | Where-Object stage -ne 'grandchild' | Where-Object {
        $childIdentity = $_.owner
        @($stageReceipts | Where-Object { $_.child.pid -eq $childIdentity.pid -and $_.child.start_time_utc -eq $childIdentity.start_time_utc -and $_.Contains('exit_code') }).Count -ne 1
    })
    $receiptComplete = $script:processes.Count -eq $script:entries -and $stageReceipts.Count -eq 41 -and $unboundChildren.Count -eq 0 -and
        @($verifiers | Where-Object { -not $_.Contains('exit_code') -or -not $_.owner.Contains('start_time_utc') }).Count -eq 0
    $budgetAccepted = $countMismatch.Count -eq 0 -and $outerTimeouts.Count -eq 0 -and $watch.Elapsed.TotalSeconds -le 600 -and $receiptComplete
    $passed = $passed -and $budgetAccepted
    $result = @{ schema = 'q03-acceptance/v1'; cases = @($script:cases.ToArray()); required_count = 68; observed_count = $script:cases.Count
        passed = $passed; required_case_identity = $requiredCaseRead.identity; missing = $missing; extra = $extra; duplicates = $duplicates
        entry_process_count = $script:entries; entries = @($script:processes.ToArray()); child_processes = $observedChildren; verifier_processes = $verifiers
        actual_counts = $counts; expected_counts = $expectedCounts; count_mismatch = $countMismatch; outer_timeouts = $outerTimeouts
        terminal_receipts_complete = $receiptComplete; unbound_children = $unboundChildren; budget_accepted = $budgetAccepted
        physical_process_total = 1 + $script:entries + $observedChildren.Count + $verifiers.Count; elapsed_seconds = $watch.Elapsed.TotalSeconds
        owner = $self; model_calls = 'NONE_BY_EXECUTED_ENTRY_SOURCE'; native_scientific_acceptance = 'UNRUN' }
    Write-LauncherJson (Join-Path $runRoot 'qa-result.json') $result
    Write-Host "Q03 cases=$($script:cases.Count)/68 passed=$passed processes=$($result.physical_process_total)"
}
if ($passed) { exit 0 } else { exit 1 }
