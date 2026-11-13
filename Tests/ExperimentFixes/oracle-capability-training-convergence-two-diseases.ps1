#requires -version 5
# Oracle for the eleventh repair work: the SECOND live consumer of the DPO
# convergence monitor, in the capability-training project.
#
# It proves BOTH boundaries BY EXECUTION, not by argument:
#   boundary 1 - unreachability: the convergence check can never shorten a run
#                when the step budget does not exceed three observation
#                intervals;
#   boundary 2 - zero-threshold deadness: at threshold 0 the comparison
#                "loss change below threshold" is false for every pair of
#                observations, including two identical losses, so stopping is
#                impossible and removing the monitor is observationally
#                neutral.
#
# Every mechanism below is a verbatim port of live product code, with the exact
# source line it was ported from. Nothing here is recalled from memory.
#
# PORTED SOURCES (read live before this script was written):
#   11.DPO/DpoConvergenceMonitor.cs:23  _previousLoss = float.MaxValue
#   11.DPO/DpoConvergenceMonitor.cs:32  _threshold   = MathF.Max(0f, threshold)
#   11.DPO/DpoConvergenceMonitor.cs:34  _windowSteps = Math.Max(1, windowSteps)
#   11.DPO/DpoConvergenceMonitor.cs:47  StepsTaken += _windowSteps
#   11.DPO/DpoConvergenceMonitor.cs:49  first call only records the loss
#   11.DPO/DpoConvergenceMonitor.cs:57  MathF.Abs(prev - cur) < _threshold
#   11.DPO/DpoConvergenceMonitor.cs:59  stable count grows
#   11.DPO/DpoConvergenceMonitor.cs:62  stable count resets
#   11.DPO/DpoConvergenceMonitor.cs:67  return _stableWindowCount >= 2
#   18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs:271-287
#                                       thin wrapper, forwards unchanged
#   18.CapabilityTraining/LlmIngredientLogicalUnits.cs:697  min budget = 1
#   18.CapabilityTraining/LlmIngredientLogicalUnits.cs:699  max budget = 5
#   18.CapabilityTraining/LlmIngredientLogicalUnits.cs:576  default budget = 1
#   18.CapabilityTraining/LlmIngredientLogicalUnits.cs:577  default threshold
#   18.CapabilityTraining/LlmIngredientLogicalUnits.cs:578  default interval 25
#   18.CapabilityTraining/LlmIngredientLogicalUnits.cs:897-899 zero admitted
#   BEFORE the repair, tip 2c3c1ca9272:
#   18.CapabilityTraining/LlmIngredientLogicalUnits.cs:1461 always create
#   18.CapabilityTraining/LlmIngredientLogicalUnits.cs:1499 double condition
#   AFTER the repair, working tree:
#   18.CapabilityTraining/LlmIngredientLogicalUnits.cs:1563 conditional create
#   18.CapabilityTraining/LlmIngredientLogicalUnits.cs:1574 one diagnostics call
#   18.CapabilityTraining/LlmIngredientLogicalUnits.cs:1620 triple condition
#   18.CapabilityTraining/LlmIngredientLogicalUnits.cs:1655 DpoStepCount local

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# Live contract constants, taken from the cited product lines.
$MIN_BUDGET        = 1      # LlmIngredientLogicalUnits.cs:697
$MAX_BUDGET        = 5      # LlmIngredientLogicalUnits.cs:699
$DEFAULT_BUDGET    = 1      # LlmIngredientLogicalUnits.cs:576
$DEFAULT_THRESHOLD = 0.002  # LlmIngredientLogicalUnits.cs:577
$DEFAULT_INTERVAL  = 25     # LlmIngredientLogicalUnits.cs:578
$MAX_PROBED_INTERVAL = 30   # probed with margin around the default of 25

# Verbatim port of DpoConvergenceMonitor plus its thin wrapper.
function New-Monitor {
    param([double]$Threshold, [int]$WindowSteps)
    # DpoConvergenceMonitor.cs:32 and :34 - both inputs are clamped.
    [pscustomobject]@{
        Threshold        = [Math]::Max(0.0, $Threshold)
        WindowSteps      = [Math]::Max(1, $WindowSteps)
        PreviousLoss     = [double]::MaxValue   # :23 "no previous call yet"
        StableWindowCount = 0
        StepsTaken       = 0
        TrueAnswers      = 0
        CallCount        = 0
    }
}

function Invoke-ShouldStop {
    param($Monitor, [double]$CurrentLoss)
    $Monitor.CallCount = $Monitor.CallCount + 1
    # :47 accounting only.
    $Monitor.StepsTaken = $Monitor.StepsTaken + $Monitor.WindowSteps
    # :49 the first observation has nothing to compare against.
    if ($Monitor.PreviousLoss -eq [double]::MaxValue) {
        $Monitor.PreviousLoss = $CurrentLoss
        return $false
    }
    # :57 one comparison against the previous observation.
    if ([Math]::Abs($Monitor.PreviousLoss - $CurrentLoss) -lt $Monitor.Threshold) {
        $Monitor.StableWindowCount = $Monitor.StableWindowCount + 1   # :59
    } else {
        $Monitor.StableWindowCount = 0                                # :62
    }
    $Monitor.PreviousLoss = $CurrentLoss                              # :65
    # :67 two consecutive stable comparisons are required.
    $answer = ($Monitor.StableWindowCount -ge 2)
    if ($answer) { $Monitor.TrueAnswers = $Monitor.TrueAnswers + 1 }
    return $answer
}

# Verbatim port of the product loop AFTER the repair (lines 1563, 1579, 1614,
# 1620). Returns executed steps, whether a monitor existed, how many questions
# were asked and how many of them answered yes.
function Invoke-LoopAfterRepair {
    param([int]$StepBudget, [int]$WindowSteps, [double]$Threshold, [double]$FlatLoss)
    # :1563 the monitor is not created at all when the threshold is zero.
    $monitor = $null
    if ($Threshold -ne 0.0) { $monitor = New-Monitor -Threshold $Threshold -WindowSteps $WindowSteps }
    $executed = 0
    for ($step = 1; $step -le $StepBudget; $step++) {
        $executed = $step                                              # :1614
        # :1620 same triple condition in the same order as the product.
        if (($step % $WindowSteps) -eq 0 -and $null -ne $monitor) {
            if (Invoke-ShouldStop -Monitor $monitor -CurrentLoss $FlatLoss) { break }
        }
    }
    [pscustomobject]@{
        Executed     = $executed
        MonitorBuilt = ($null -ne $monitor)
        Questions    = if ($null -eq $monitor) { 0 } else { $monitor.CallCount }
        TrueAnswers  = if ($null -eq $monitor) { 0 } else { $monitor.TrueAnswers }
    }
}

# Verbatim port of the product loop BEFORE the repair, at tip 2c3c1ca9272
# (lines 1461 and 1499): the monitor was always created and always asked.
function Invoke-LoopBeforeRepair {
    param([int]$StepBudget, [int]$WindowSteps, [double]$Threshold, [double]$FlatLoss)
    $monitor = New-Monitor -Threshold $Threshold -WindowSteps $WindowSteps   # :1461
    $executed = 0
    for ($step = 1; $step -le $StepBudget; $step++) {
        $executed = $step
        if (($step % $WindowSteps) -eq 0) {                                   # :1499
            if (Invoke-ShouldStop -Monitor $monitor -CurrentLoss $FlatLoss) { break }
        }
    }
    [pscustomobject]@{
        Executed    = $executed
        Questions   = $monitor.CallCount
        TrueAnswers = $monitor.TrueAnswers
    }
}

# The shipped predicates, ported verbatim from lines 801-803 and 812.
function Test-Unreachable {
    param([int]$MaxStepCount, [int]$WindowSteps, [double]$Threshold)
    return ($Threshold -ne 0.0) -and ($WindowSteps -ge 1) -and `
           ($MaxStepCount -le (3 * $WindowSteps))
}

function Test-DisabledByZero {
    param([double]$Threshold)
    return ($Threshold -eq 0.0)
}

# The shipped emitter order, ported verbatim from lines 829 and 846: at a zero
# threshold only the zero sign speaks.
function Get-Sign {
    param([int]$MaxStepCount, [int]$WindowSteps, [double]$Threshold)
    if (Test-DisabledByZero -Threshold $Threshold) { return 'ZERO_THRESHOLD' }
    if (Test-Unreachable -MaxStepCount $MaxStepCount -WindowSteps $WindowSteps -Threshold $Threshold) {
        return 'UNREACHABLE'
    }
    return 'SILENT'
}

Write-Output '================================================================'
Write-Output 'ORACLE: capability-training DPO convergence, two diseases'
Write-Output '================================================================'
Write-Output ("live contract: budget {0}..{1}; record defaults budget {2}, interval {3}, threshold {4}" -f `
    $MIN_BUDGET, $MAX_BUDGET, $DEFAULT_BUDGET, $DEFAULT_INTERVAL, $DEFAULT_THRESHOLD)
Write-Output ''

# ---------------------------------------------------------------- PANEL 1 ----
Write-Output 'PANEL 1 - BOUNDARY 1 (unreachability) over the WHOLE allowed space'
Write-Output 'flat loss 0.5, working threshold 0.002, budget 1..5, interval 1..30'
Write-Output ''
$p1Rows = New-Object System.Collections.Generic.List[object]
$shortening = New-Object System.Collections.Generic.List[object]
$p1Disagreements = 0
for ($budget = $MIN_BUDGET; $budget -le $MAX_BUDGET; $budget++) {
    for ($interval = 1; $interval -le $MAX_PROBED_INTERVAL; $interval++) {
        $run = Invoke-LoopAfterRepair -StepBudget $budget -WindowSteps $interval `
            -Threshold $DEFAULT_THRESHOLD -FlatLoss 0.5
        $neverShortened = ($run.Executed -eq $budget)
        $predicate = Test-Unreachable -MaxStepCount $budget -WindowSteps $interval -Threshold $DEFAULT_THRESHOLD
        if ($neverShortened -ne $predicate) { $p1Disagreements = $p1Disagreements + 1 }
        if (-not $neverShortened) {
            $shortening.Add([pscustomobject]@{ Budget = $budget; Interval = $interval; Executed = $run.Executed })
        }
        $p1Rows.Add([pscustomobject]@{
            Budget = $budget; Interval = $interval; Executed = $run.Executed
            Questions = $run.Questions; TrueAnswers = $run.TrueAnswers
            NeverShortened = $neverShortened; Predicate = $predicate
        })
    }
}
Write-Output ("rows probed by execution              : {0}" -f $p1Rows.Count)
Write-Output ("rows where the run was shortened      : {0}" -f $shortening.Count)
Write-Output ("predicate-vs-execution disagreements  : {0}" -f $p1Disagreements)
Write-Output 'the complete list of shortening configurations:'
foreach ($row in $shortening) {
    Write-Output ("   budget {0}, interval {1} -> executed {2} (saved {3})" -f `
        $row.Budget, $row.Interval, $row.Executed, ($row.Budget - $row.Executed))
}
$shorteningWithInterval2Plus = @($shortening | Where-Object { $_.Interval -ge 2 }).Count
Write-Output ("shortening configurations with interval >= 2: {0}" -f $shorteningWithInterval2Plus)
Write-Output 'STRONGER-THAN-THE-BRIDGE CLAIM, measured here by execution:'
Write-Output ('   the budget of this unit is capped at 5, therefore EVERY observation')
Write-Output ('   interval of 2 or more leaves the convergence check dead for EVERY')
Write-Output ('   allowed budget. Measured count of counterexamples: ' + $shorteningWithInterval2Plus)
Write-Output ''

# ---------------------------------------------------------------- PANEL 2 ----
Write-Output 'PANEL 2 - how the two live caller shapes behave'
Write-Output ''
$callerShapes = New-Object System.Collections.Generic.List[object]
$callerShapes.Add([pscustomobject]@{ Name = 'DpoTrainingExecution.cs:446 (no interval passed)'; Budget = $MAX_BUDGET; Interval = $DEFAULT_INTERVAL })
$callerShapes.Add([pscustomobject]@{ Name = 'DesignConversionDpoTrainingHandoff.cs:405 (no interval)'; Budget = $MAX_BUDGET; Interval = $DEFAULT_INTERVAL })
$callerShapes.Add([pscustomobject]@{ Name = 'DefaultLogicalCapabilityTrainer.cs:2298 (interval 1)'; Budget = $MAX_BUDGET; Interval = 1 })
$callerShapes.Add([pscustomobject]@{ Name = 'record own defaults (budget 1, interval 25)'; Budget = $DEFAULT_BUDGET; Interval = $DEFAULT_INTERVAL })
foreach ($shape in $callerShapes) {
    $run = Invoke-LoopAfterRepair -StepBudget $shape.Budget -WindowSteps $shape.Interval `
        -Threshold $DEFAULT_THRESHOLD -FlatLoss 0.5
    $sign = Get-Sign -MaxStepCount $shape.Budget -WindowSteps $shape.Interval -Threshold $DEFAULT_THRESHOLD
    $verdict = if ($run.Executed -eq $shape.Budget) { 'DEAD (full budget always)' } else { 'ALIVE (saved ' + ($shape.Budget - $run.Executed) + ')' }
    Write-Output ("   {0}" -f $shape.Name)
    Write-Output ("      budget {0}, interval {1} -> executed {2}, questions {3}, yes {4} -> {5}; sign {6}" -f `
        $shape.Budget, $shape.Interval, $run.Executed, $run.Questions, $run.TrueAnswers, $verdict, $sign)
}
Write-Output ''

# ---------------------------------------------------------------- PANEL 3 ----
Write-Output 'PANEL 3 - BOUNDARY 2 (zero threshold) proven by execution'
Write-Output ''
$zeroTotalQuestions = 0
$zeroTotalTrue = 0
$neutralityBreaks = 0
$zeroRows = 0
for ($budget = $MIN_BUDGET; $budget -le $MAX_BUDGET; $budget++) {
    for ($interval = 1; $interval -le $MAX_PROBED_INTERVAL; $interval++) {
        $before = Invoke-LoopBeforeRepair -StepBudget $budget -WindowSteps $interval -Threshold 0.0 -FlatLoss 0.5
        $after  = Invoke-LoopAfterRepair  -StepBudget $budget -WindowSteps $interval -Threshold 0.0 -FlatLoss 0.5
        $zeroTotalQuestions = $zeroTotalQuestions + $before.Questions
        $zeroTotalTrue = $zeroTotalTrue + $before.TrueAnswers
        if ($before.Executed -ne $after.Executed) { $neutralityBreaks = $neutralityBreaks + 1 }
        if ($after.Executed -ne $budget) { $neutralityBreaks = $neutralityBreaks + 1 }
        $zeroRows = $zeroRows + 1
    }
}
Write-Output ("rows probed at threshold 0                        : {0}" -f $zeroRows)
Write-Output ("questions the OLD loop asked at threshold 0       : {0}" -f $zeroTotalQuestions)
Write-Output ("of those, questions answered yes                  : {0}" -f $zeroTotalTrue)
Write-Output ("rows where OLD and NEW executed a different count : {0}" -f $neutralityBreaks)
Write-Output 'direct check of the dead comparison, 20 identical losses, interval 1:'
$deadMonitor = New-Monitor -Threshold 0.0 -WindowSteps 1
$deadYes = 0
for ($observation = 1; $observation -le 20; $observation++) {
    if (Invoke-ShouldStop -Monitor $deadMonitor -CurrentLoss 0.5) { $deadYes = $deadYes + 1 }
}
Write-Output ("   identical-loss observations: 20; yes answers: {0}" -f $deadYes)
$negZeroMonitor = New-Monitor -Threshold -0.0 -WindowSteps 1
$negZeroYes = 0
for ($observation = 1; $observation -le 20; $observation++) {
    if (Invoke-ShouldStop -Monitor $negZeroMonitor -CurrentLoss 0.5) { $negZeroYes = $negZeroYes + 1 }
}
Write-Output ("   same check with a negative zero threshold : yes answers {0}" -f $negZeroYes)
Write-Output ''

# ---------------------------------------------------------------- PANEL 4 ----
Write-Output 'PANEL 4 - the two signs never speak together, and order is enforced'
Write-Output ''
$signRows = New-Object System.Collections.Generic.List[object]
$signRows.Add([pscustomobject]@{ Budget = 1; Interval = 25; Threshold = 0.002; Expect = 'UNREACHABLE' })
$signRows.Add([pscustomobject]@{ Budget = 5; Interval = 25; Threshold = 0.002; Expect = 'UNREACHABLE' })
$signRows.Add([pscustomobject]@{ Budget = 3; Interval = 1;  Threshold = 0.002; Expect = 'UNREACHABLE' })
$signRows.Add([pscustomobject]@{ Budget = 4; Interval = 1;  Threshold = 0.002; Expect = 'SILENT' })
$signRows.Add([pscustomobject]@{ Budget = 5; Interval = 1;  Threshold = 0.002; Expect = 'SILENT' })
$signRows.Add([pscustomobject]@{ Budget = 1; Interval = 25; Threshold = 0.0;   Expect = 'ZERO_THRESHOLD' })
$signRows.Add([pscustomobject]@{ Budget = 5; Interval = 1;  Threshold = 0.0;   Expect = 'ZERO_THRESHOLD' })
$signRows.Add([pscustomobject]@{ Budget = 5; Interval = 25; Threshold = 0.0;   Expect = 'ZERO_THRESHOLD' })
$signMismatches = 0
foreach ($row in $signRows) {
    $sign = Get-Sign -MaxStepCount $row.Budget -WindowSteps $row.Interval -Threshold $row.Threshold
    $unreach = Test-Unreachable -MaxStepCount $row.Budget -WindowSteps $row.Interval -Threshold $row.Threshold
    $zero = Test-DisabledByZero -Threshold $row.Threshold
    $both = ($unreach -and $zero)
    $ok = ($sign -eq $row.Expect) -and (-not $both)
    if (-not $ok) { $signMismatches = $signMismatches + 1 }
    Write-Output ("   budget {0}, interval {1}, threshold {2} -> sign {3} (expected {4}); unreachable={5} zero={6} both={7} {8}" -f `
        $row.Budget, $row.Interval, $row.Threshold, $sign, $row.Expect, $unreach, $zero, $both, $(if ($ok) { 'OK' } else { 'MISMATCH' }))
}
Write-Output ("sign mismatches / both-signs-at-once cases: {0}" -f $signMismatches)
Write-Output ''

# ---------------------------------------------------------------- PANEL 5 ----
Write-Output 'PANEL 5 - which rows became correct, stayed incorrect, or regressed'
Write-Output ''
$classes = New-Object System.Collections.Generic.List[object]
$classes.Add([pscustomobject]@{ Class = 'working threshold, budget <= 3*interval (dead run)'; Budget = 5; Interval = 25; Threshold = 0.002 })
$classes.Add([pscustomobject]@{ Class = 'working threshold, budget > 3*interval (live run)'; Budget = 5; Interval = 1; Threshold = 0.002 })
$classes.Add([pscustomobject]@{ Class = 'zero threshold (stopping impossible)'; Budget = 5; Interval = 1; Threshold = 0.0 })
foreach ($class in $classes) {
    $before = Invoke-LoopBeforeRepair -StepBudget $class.Budget -WindowSteps $class.Interval -Threshold $class.Threshold -FlatLoss 0.5
    $after = Invoke-LoopAfterRepair -StepBudget $class.Budget -WindowSteps $class.Interval -Threshold $class.Threshold -FlatLoss 0.5
    $sign = Get-Sign -MaxStepCount $class.Budget -WindowSteps $class.Interval -Threshold $class.Threshold
    $behaviourChanged = ($before.Executed -ne $after.Executed)
    $status = if ($behaviourChanged) { 'REGRESSION-RISK: executed count changed' } `
        elseif ($sign -eq 'SILENT') { 'unchanged and correctly silent' } `
        else { 'BECAME CORRECT: same steps, now speaks (' + $sign + ')' }
    Write-Output ("   {0}" -f $class.Class)
    Write-Output ("      before: executed {0}, questions {1}; after: executed {2}, questions {3}; sign {4}" -f `
        $before.Executed, $before.Questions, $after.Executed, $after.Questions, $sign)
    Write-Output ("      verdict: {0}" -f $status)
}
Write-Output ''

# ---------------------------------------------------------------- SUMMARY ----
Write-Output '================================================================'
$allGreen = ($p1Disagreements -eq 0) -and ($neutralityBreaks -eq 0) -and `
            ($signMismatches -eq 0) -and ($zeroTotalTrue -eq 0) -and `
            ($deadYes -eq 0) -and ($negZeroYes -eq 0) -and `
            ($shorteningWithInterval2Plus -eq 0) -and ($shortening.Count -eq 2)
Write-Output ("ORACLE RESULT: {0}" -f $(if ($allGreen) { 'BOTH BOUNDARIES PROVEN BY EXECUTION' } else { 'FAILED' }))
Write-Output ("   boundary 1 predicate-vs-execution disagreements : {0}" -f $p1Disagreements)
Write-Output ("   boundary 1 shortening configurations found      : {0} (expected exactly 2)" -f $shortening.Count)
Write-Output ("   boundary 1 interval>=2 counterexamples          : {0}" -f $shorteningWithInterval2Plus)
Write-Output ("   boundary 2 yes-answers at zero threshold        : {0}" -f ($zeroTotalTrue + $deadYes + $negZeroYes))
Write-Output ("   boundary 2 neutrality breaks (old vs new loop)  : {0}" -f $neutralityBreaks)
Write-Output ("   sign independence / order mismatches            : {0}" -f $signMismatches)
Write-Output 'WHAT THIS DOES NOT PROVE: no measured training experiment exists for'
Write-Output 'this consumer. No claim of faster or better training is made here.'
Write-Output '================================================================'
