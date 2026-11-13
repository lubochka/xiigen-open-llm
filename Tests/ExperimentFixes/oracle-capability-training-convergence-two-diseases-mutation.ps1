#requires -version 5
# Mutation-sensitivity companion to
# oracle-capability-training-convergence-two-diseases.ps1.
#
# A sign that cannot fail is not a check. This script replaces exactly one
# decision at a time and measures how many rows each replacement breaks. The
# shipped version must break nothing; every mutation must break something.
#
# Mutations (one decision each):
#   M0 SHIPPED  - as published in 18.CapabilityTraining/LlmIngredientLogicalUnits.cs
#                 lines 801-803 (unreachability), 812 (zero), 829/846 (order),
#                 1563 (no monitor at a zero threshold).
#   M1 "<"      - unreachability uses budget < 3*interval instead of <=.
#                 This is the exact off-by-one found and fixed for the bridge.
#   M2 NO-ZERO-CLAUSE - unreachability drops the "threshold != 0" clause, so a
#                 zero threshold is also reported as unreachability. Breaks the
#                 requirement that the two signs stay independent.
#   M3 ORDER    - the emitter checks unreachability BEFORE the zero sign, so a
#                 dead-by-zero run is mislabelled as a short-budget run.
#   M4 ZERO-AS-NEGATIVE - the zero predicate uses threshold < 0 instead of
#                 threshold == 0, so the zero disease goes silent again.
#   M5 ALWAYS-CREATE - the loop reverts to creating the monitor at a zero
#                 threshold, which is the state before this repair.
#   M6 NO-ZERO-CLAUSE + ORDER - M2 and M3 together.
#
# Expectation recorded BEFORE the run, so the table can refute it:
#   M1, M2, M4, M6 must be detected; M5 must change the dead-question count;
#   M3 is expected to be REDUNDANT on its own, because the shipped
#   unreachability predicate already carries the "threshold != 0" clause
#   (line 801), so the emitter order (line 829) is a second, independent guard
#   rather than the only one. M6 proves that guard is still load-bearing: once
#   the clause is removed, the order alone decides. A mutation that is expected
#   to be redundant is declared here in advance and is not counted as a pass.

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$MIN_BUDGET = 1
$MAX_BUDGET = 5
$MAX_PROBED_INTERVAL = 30
$WORKING_THRESHOLD = 0.002

function New-Monitor {
    param([double]$Threshold, [int]$WindowSteps)
    [pscustomobject]@{
        Threshold = [Math]::Max(0.0, $Threshold)
        WindowSteps = [Math]::Max(1, $WindowSteps)
        PreviousLoss = [double]::MaxValue
        StableWindowCount = 0
        CallCount = 0
    }
}

function Invoke-ShouldStop {
    param($Monitor, [double]$CurrentLoss)
    $Monitor.CallCount = $Monitor.CallCount + 1
    if ($Monitor.PreviousLoss -eq [double]::MaxValue) {
        $Monitor.PreviousLoss = $CurrentLoss
        return $false
    }
    if ([Math]::Abs($Monitor.PreviousLoss - $CurrentLoss) -lt $Monitor.Threshold) {
        $Monitor.StableWindowCount = $Monitor.StableWindowCount + 1
    } else {
        $Monitor.StableWindowCount = 0
    }
    $Monitor.PreviousLoss = $CurrentLoss
    return ($Monitor.StableWindowCount -ge 2)
}

# The product loop, with the "always create the monitor" switch for M5.
function Invoke-Loop {
    param([int]$StepBudget, [int]$WindowSteps, [double]$Threshold, [bool]$AlwaysCreate)
    $monitor = $null
    if ($AlwaysCreate -or $Threshold -ne 0.0) {
        $monitor = New-Monitor -Threshold $Threshold -WindowSteps $WindowSteps
    }
    $executed = 0
    for ($step = 1; $step -le $StepBudget; $step++) {
        $executed = $step
        if (($step % $WindowSteps) -eq 0 -and $null -ne $monitor) {
            if (Invoke-ShouldStop -Monitor $monitor -CurrentLoss 0.5) { break }
        }
    }
    [pscustomobject]@{
        Executed = $executed
        Questions = if ($null -eq $monitor) { 0 } else { $monitor.CallCount }
    }
}

function Test-Unreachable {
    param([string]$Variant, [int]$MaxStepCount, [int]$WindowSteps, [double]$Threshold)
    switch ($Variant) {
        'M1' { return ($Threshold -ne 0.0) -and ($WindowSteps -ge 1) -and ($MaxStepCount -lt (3 * $WindowSteps)) }
        'M2' { return ($WindowSteps -ge 1) -and ($MaxStepCount -le (3 * $WindowSteps)) }
        'M6' { return ($WindowSteps -ge 1) -and ($MaxStepCount -le (3 * $WindowSteps)) }
        default { return ($Threshold -ne 0.0) -and ($WindowSteps -ge 1) -and ($MaxStepCount -le (3 * $WindowSteps)) }
    }
}

function Test-DisabledByZero {
    param([string]$Variant, [double]$Threshold)
    switch ($Variant) {
        'M4' { return ($Threshold -lt 0.0) }
        default { return ($Threshold -eq 0.0) }
    }
}

function Get-Sign {
    param([string]$Variant, [int]$MaxStepCount, [int]$WindowSteps, [double]$Threshold)
    if ($Variant -eq 'M3' -or $Variant -eq 'M6') {
        if (Test-Unreachable -Variant $Variant -MaxStepCount $MaxStepCount -WindowSteps $WindowSteps -Threshold $Threshold) { return 'UNREACHABLE' }
        if (Test-DisabledByZero -Variant $Variant -Threshold $Threshold) { return 'ZERO_THRESHOLD' }
        return 'SILENT'
    }
    if (Test-DisabledByZero -Variant $Variant -Threshold $Threshold) { return 'ZERO_THRESHOLD' }
    if (Test-Unreachable -Variant $Variant -MaxStepCount $MaxStepCount -WindowSteps $WindowSteps -Threshold $Threshold) { return 'UNREACHABLE' }
    return 'SILENT'
}

Write-Output '================================================================'
Write-Output 'MUTATION PANEL: one decision replaced at a time'
Write-Output '================================================================'
Write-Output ''

$variants = @('M0', 'M1', 'M2', 'M3', 'M4', 'M5', 'M6')
# Declared before the run so the measurement can refute it.
$expectedDetected = @('M1', 'M2', 'M4', 'M6')
$expectedRedundant = @('M3')
$expectedQuestionShift = @('M5')
$report = New-Object System.Collections.Generic.List[object]
foreach ($variant in $variants) {
    $alwaysCreate = ($variant -eq 'M5')
    $predicateDisagreements = 0   # sign says "unreachable" but the run was shortened, or the reverse
    $bothSignsAtOnce = 0          # the two signs are no longer independent
    $deadRunsLeftSilent = 0       # a run that cannot stop and gets no sign at all
    $wrongSignOnZero = 0          # a zero-threshold run labelled as a short budget
    $deadQuestionsAsked = 0       # questions asked at a zero threshold, all answered no
    for ($budget = $MIN_BUDGET; $budget -le $MAX_BUDGET; $budget++) {
        for ($interval = 1; $interval -le $MAX_PROBED_INTERVAL; $interval++) {
            # --- working threshold: the unreachability boundary ---------------
            $run = Invoke-Loop -StepBudget $budget -WindowSteps $interval -Threshold $WORKING_THRESHOLD -AlwaysCreate $alwaysCreate
            $neverShortened = ($run.Executed -eq $budget)
            $unreach = Test-Unreachable -Variant $variant -MaxStepCount $budget -WindowSteps $interval -Threshold $WORKING_THRESHOLD
            if ($neverShortened -ne $unreach) { $predicateDisagreements = $predicateDisagreements + 1 }
            $sign = Get-Sign -Variant $variant -MaxStepCount $budget -WindowSteps $interval -Threshold $WORKING_THRESHOLD
            if ($neverShortened -and $sign -eq 'SILENT') { $deadRunsLeftSilent = $deadRunsLeftSilent + 1 }

            # --- zero threshold: the deadness boundary ------------------------
            $zeroRun = Invoke-Loop -StepBudget $budget -WindowSteps $interval -Threshold 0.0 -AlwaysCreate $alwaysCreate
            $deadQuestionsAsked = $deadQuestionsAsked + $zeroRun.Questions
            $zeroUnreach = Test-Unreachable -Variant $variant -MaxStepCount $budget -WindowSteps $interval -Threshold 0.0
            $zeroFlag = Test-DisabledByZero -Variant $variant -Threshold 0.0
            if ($zeroUnreach -and $zeroFlag) { $bothSignsAtOnce = $bothSignsAtOnce + 1 }
            $zeroSign = Get-Sign -Variant $variant -MaxStepCount $budget -WindowSteps $interval -Threshold 0.0
            if ($zeroSign -ne 'ZERO_THRESHOLD') { $wrongSignOnZero = $wrongSignOnZero + 1 }
        }
    }
    $broken = $predicateDisagreements + $bothSignsAtOnce + $deadRunsLeftSilent + $wrongSignOnZero
    $report.Add([pscustomobject]@{
        Variant = $variant
        PredicateDisagreements = $predicateDisagreements
        BothSignsAtOnce = $bothSignsAtOnce
        DeadRunsLeftSilent = $deadRunsLeftSilent
        WrongSignOnZero = $wrongSignOnZero
        DeadQuestionsAsked = $deadQuestionsAsked
        BrokenRows = $broken
    })
}

Write-Output 'variant | predicate | both-signs | dead-silent | wrong-sign-on-zero | dead-questions | broken'
foreach ($row in $report) {
    Write-Output ("{0,-7} | {1,9} | {2,10} | {3,11} | {4,18} | {5,14} | {6,6}" -f `
        $row.Variant, $row.PredicateDisagreements, $row.BothSignsAtOnce, `
        $row.DeadRunsLeftSilent, $row.WrongSignOnZero, $row.DeadQuestionsAsked, $row.BrokenRows)
}
Write-Output ''
$shipped = $report | Where-Object { $_.Variant -eq 'M0' }
$undetectedButExpected = New-Object System.Collections.Generic.List[string]
foreach ($name in $expectedDetected) {
    $row = $report | Where-Object { $_.Variant -eq $name }
    if ($row.BrokenRows -eq 0) { $undetectedButExpected.Add($name) }
}
$redundantButDetected = New-Object System.Collections.Generic.List[string]
foreach ($name in $expectedRedundant) {
    $row = $report | Where-Object { $_.Variant -eq $name }
    if ($row.BrokenRows -ne 0) { $redundantButDetected.Add($name) }
}
$questionShiftMissing = New-Object System.Collections.Generic.List[string]
foreach ($name in $expectedQuestionShift) {
    $row = $report | Where-Object { $_.Variant -eq $name }
    if ($row.DeadQuestionsAsked -eq $shipped.DeadQuestionsAsked) { $questionShiftMissing.Add($name) }
}
Write-Output ("shipped version broken rows                    : {0}" -f $shipped.BrokenRows)
Write-Output ("shipped version dead questions at zero         : {0}" -f $shipped.DeadQuestionsAsked)
Write-Output ("mutations expected to be detected but were not : {0}" -f `
    $(if ($undetectedButExpected.Count -eq 0) { 'none' } else { [string]::Join(',', $undetectedButExpected) }))
Write-Output ("mutations declared redundant that did fire     : {0}" -f `
    $(if ($redundantButDetected.Count -eq 0) { 'none' } else { [string]::Join(',', $redundantButDetected) }))
Write-Output ("always-create mutation without a question shift: {0}" -f `
    $(if ($questionShiftMissing.Count -eq 0) { 'none' } else { [string]::Join(',', $questionShiftMissing) }))
Write-Output ''
Write-Output 'READING OF THE TABLE, measured and not assumed:'
Write-Output '  M1 breaks the row where the budget equals three intervals exactly:'
Write-Output '     the run is not shortened there, so calling it reachable is wrong.'
Write-Output '     It also leaves that dead run with no sign at all.'
Write-Output '  M2 makes the two signs fire for the same setting, which is exactly'
Write-Output '     the dependency the work order forbids.'
Write-Output '  M3 on its own breaks NOTHING, and that is a measured fact, not an'
Write-Output '     excuse: the shipped unreachability predicate already refuses a'
Write-Output '     zero threshold in its own first clause, so the emitter order is'
Write-Output '     a SECOND independent guard rather than the only one. Requirement'
Write-Output '     "at a zero threshold only the zero sign speaks" therefore holds'
Write-Output '     twice over.'
Write-Output '  M6 proves that second guard is still load-bearing: remove the clause'
Write-Output '     and reverse the order together, and every dead-by-zero run with a'
Write-Output '     short budget is mislabelled as a short-budget run.'
Write-Output '  M4 restores the original disease: the zero threshold goes silent.'
Write-Output '  M5 restores the dead monitor calls that this repair removes. The'
Write-Output '     executed step counts stay identical, which is precisely why the'
Write-Output '     repair is observationally neutral; only the dead questions differ.'
$ok = ($shipped.BrokenRows -eq 0) -and ($undetectedButExpected.Count -eq 0) -and `
      ($redundantButDetected.Count -eq 0) -and ($questionShiftMissing.Count -eq 0)
Write-Output ''
Write-Output ("MUTATION RESULT: {0}" -f $(if ($ok) { 'SHIPPED VERSION BREAKS NOTHING AND EVERY DECLARED-DETECTABLE MUTATION IS DETECTED' } else { 'FAILED' }))
Write-Output '================================================================'
