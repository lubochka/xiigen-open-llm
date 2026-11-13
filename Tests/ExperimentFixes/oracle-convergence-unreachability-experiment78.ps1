# Oracle v2 for the tenth repair work: convergence-detection reachability.
# v1 conflated two observables (the monitor FIRES vs the run is SHORTENED) and
# had an array-construction bug in panel 3. Both are fixed here.
# Product source: C:\Projects\llm_mvp_core (branch codex/dpo-implementation).

$ErrorActionPreference = "Stop"

# ---- Port of 11.DPO/DpoConvergenceMonitor.cs:29-72 ------------------------
class Monitor {
    [float]$Threshold
    [int]$WindowSteps
    [float]$PreviousLoss
    [int]$StableWindowCount
    [int]$StepsTaken

    Monitor([float]$threshold, [int]$windowSteps) {
        $this.Threshold = [Math]::Max([float]0, $threshold)   # :32
        $this.WindowSteps = [Math]::Max(1, $windowSteps)      # :34
        $this.PreviousLoss = [float]::MaxValue                # :23
        $this.StableWindowCount = 0                           # :26
        $this.StepsTaken = 0                                  # :72
    }

    [bool] ShouldStop([float]$currentLoss) {
        $this.StepsTaken += $this.WindowSteps                 # :47
        if ($this.PreviousLoss -eq [float]::MaxValue) {        # :49
            $this.PreviousLoss = $currentLoss                 # :52
            return $false                                     # :53
        }
        if ([Math]::Abs($this.PreviousLoss - $currentLoss) -lt $this.Threshold) {   # :57
            $this.StableWindowCount++                         # :59
        } else {
            $this.StableWindowCount = 0                       # :62
        }
        $this.PreviousLoss = $currentLoss                     # :65
        return $this.StableWindowCount -ge 2                  # :67
    }
}

# ---- Two candidate boundaries ---------------------------------------------
# (A) the boundary v1 shipped: "the monitor can never answer yes"
function Test-UnreachableStrict {
    param([int]$DpoStepCount, [int]$WindowSteps, [float]$Threshold)
    return ($Threshold -ne [float]0) -and ($WindowSteps -ge 1) -and ([long]$DpoStepCount -lt (3 * [long]$WindowSteps))
}
# (B) the boundary now in the product: "the yes can never shorten the run"
# Port of 44.DesignConversionDomain/..CheckpointBridge.cs:6144-6152
function Test-Unreachable {
    param([int]$DpoStepCount, [int]$WindowSteps, [float]$Threshold)
    return ($Threshold -ne [float]0) -and ($WindowSteps -ge 1) -and ([long]$DpoStepCount -le (3 * [long]$WindowSteps))
}

# ---- Port of 44.DesignConversionDomain/..CheckpointBridge.cs:6191-6225 ----
# Instrumented to report BOTH observables separately.
function Invoke-BridgeLoop {
    param([int]$StepBudget, [int]$WindowSteps, [float]$Threshold, [float[]]$LossSeries, [bool]$WithNewSign)
    $monitor = $null
    if ($Threshold -ne [float]0) {                            # :6192
        $monitor = [Monitor]::new($Threshold, $WindowSteps)   # :6194-6196
    }
    $signCount = 0
    if ($WithNewSign) {                                       # :6201-6204
        if (Test-Unreachable -DpoStepCount $StepBudget -WindowSteps $WindowSteps -Threshold $Threshold) {
            $signCount = 1
        }
    }
    $actual = 0                                               # :6205
    $fired = $false
    $firedAtStep = 0
    for ($step = 1; $step -le $StepBudget; $step++) {          # :6206
        $loss = $LossSeries[[Math]::Min($step - 1, $LossSeries.Length - 1)]
        $actual = $step                                       # :6216
        if ($null -ne $monitor -and ($step % $WindowSteps) -eq 0 -and $monitor.ShouldStop($loss)) {   # :6219-6221
            $fired = $true
            $firedAtStep = $step
            break                                             # :6223
        }
    }
    return [pscustomobject]@{
        Executed    = $actual
        Fired       = $fired
        FiredAtStep = $firedAtStep
        Shortened   = ($actual -lt $StepBudget)
        Signs       = $signCount
    }
}

# ---- Port of 18.CapabilityTraining/LlmIngredientLogicalUnits.cs:1460-1500 -
function Invoke-TrainerLoop {
    param([int]$MaxSteps, [int]$WindowSteps, [float]$Threshold, [float]$FlatLoss)
    $ws = [Math]::Max(1, $WindowSteps)                        # :1460
    $monitor = [Monitor]::new($Threshold, $ws)                # :1461 (no null branch there)
    $steps = 0
    $fired = $false
    for ($step = 1; $step -le $MaxSteps; $step++) {            # :1463
        $steps = $step                                        # :1498
        if (($step % $ws) -eq 0 -and $monitor.ShouldStop($FlatLoss)) {   # :1499
            $fired = $true
            break                                             # :1500
        }
    }
    return [pscustomobject]@{ Executed = $steps; Fired = $fired; Shortened = ($steps -lt $MaxSteps); Signs = 0 }
}

$flat = [float[]]@(0.5)

Write-Output "PANEL 1: bridge loop, flat loss 0.5 (the best possible case for stopping)"
Write-Output "budget interval obs execOld execNew fired firedAt shortened predA predB agreeA agreeB"
$rows = @((1,1), (2,1), (3,1), (4,1), (24,25), (25,25), (49,25), (50,25), (74,25), (75,25), (76,25), (100,25), (6,7), (13,7), (14,7), (20,7), (21,7), (22,7))
$mismatchA = 0
$mismatchB = 0
$drift = 0
foreach ($r in $rows) {
    $b = [int]$r[0]
    $w = [int]$r[1]
    $old = Invoke-BridgeLoop -StepBudget $b -WindowSteps $w -Threshold 0.01 -LossSeries $flat -WithNewSign $false
    $new = Invoke-BridgeLoop -StepBudget $b -WindowSteps $w -Threshold 0.01 -LossSeries $flat -WithNewSign $true
    $obs = [Math]::Floor($b / $w)
    $pa = Test-UnreachableStrict -DpoStepCount $b -WindowSteps $w -Threshold 0.01
    $pb = Test-Unreachable -DpoStepCount $b -WindowSteps $w -Threshold 0.01
    $agreeA = ($pa -eq (-not $new.Fired))
    $agreeB = ($pb -eq (-not $new.Shortened))
    if (-not $agreeA) { $mismatchA++ }
    if (-not $agreeB) { $mismatchB++ }
    if ($old.Executed -ne $new.Executed) { $drift++ }
    Write-Output ("{0,6} {1,8} {2,3} {3,7} {4,7} {5,5} {6,7} {7,9} {8,5} {9,5} {10,6} {11,6}" -f $b, $w, $obs, $old.Executed, $new.Executed, $new.Fired, $new.FiredAtStep, $new.Shortened, $pa, $pb, $agreeA, $agreeB)
}
Write-Output ("PANEL 1 boundary A vs observable FIRED, mismatches: {0}" -f $mismatchA)
Write-Output ("PANEL 1 boundary B vs observable SHORTENED, mismatches: {0}" -f $mismatchB)
Write-Output ("PANEL 1 executed-step drift old-vs-new (must be 0): {0}" -f $drift)
Write-Output ""

Write-Output "PANEL 2: first budget where the monitor fires, and first budget where the run is shortened"
Write-Output "interval firstBudgetFired threeIntervals firstBudgetShortened threeIntervalsPlusOne stepAtWhichItFires"
foreach ($w in 1, 2, 7, 25, 331) {
    $firstFired = -1
    $firstShort = -1
    for ($b = 1; $b -le (4 * $w + 2); $b++) {
        $res = Invoke-BridgeLoop -StepBudget $b -WindowSteps $w -Threshold 0.01 -LossSeries $flat -WithNewSign $true
        if ($firstFired -lt 0 -and $res.Fired) { $firstFired = $b }
        if ($firstShort -lt 0 -and $res.Shortened) { $firstShort = $b; break }
    }
    $big = Invoke-BridgeLoop -StepBudget (4 * $w + 2) -WindowSteps $w -Threshold 0.01 -LossSeries $flat -WithNewSign $true
    Write-Output ("{0,8} {1,16} {2,14} {3,20} {4,21} {5,18}" -f $w, $firstFired, (3 * $w), $firstShort, (3 * $w + 1), $big.FiredAtStep)
}
Write-Output ""

Write-Output "PANEL 3: interval == budget and interval == budget/2 (the experiment-78 shapes)"
Write-Output "budget interval shape                exec fired shortened signNew"
$earlyStops = 0
$silent = 0
$cases = New-Object System.Collections.Generic.List[object]
foreach ($b in 1, 2, 10, 25, 50, 100, 1000) {
    $cases.Add([pscustomobject]@{ Budget = $b; Interval = $b; Shape = "interval==budget" })
    if (($b % 2) -eq 0) {
        $cases.Add([pscustomobject]@{ Budget = $b; Interval = [int]($b / 2); Shape = "interval==budget/2" })
    }
}
foreach ($c in $cases) {
    $res = Invoke-BridgeLoop -StepBudget $c.Budget -WindowSteps $c.Interval -Threshold 0.01 -LossSeries $flat -WithNewSign $true
    if ($res.Shortened) { $earlyStops++ }
    if ($res.Signs -eq 0) { $silent++ }
    Write-Output ("{0,6} {1,8} {2,-20} {3,5} {4,5} {5,9} {6,7}" -f $c.Budget, $c.Interval, $c.Shape, $res.Executed, $res.Fired, $res.Shortened, $res.Signs)
}
Write-Output ("PANEL 3 rows {0}; runs shortened (must be 0): {1}; rows left silent (must be 0): {2}" -f $cases.Count, $earlyStops, $silent)
Write-Output ""

Write-Output "PANEL 4: the sign fires exactly on rows the check cannot shorten, and stays silent elsewhere"
$falseAlarm = 0
$missed = 0
$checked = 0
foreach ($w in 1, 2, 3, 7, 25, 100) {
    for ($b = 1; $b -le (3 * $w + 5); $b++) {
        $res = Invoke-BridgeLoop -StepBudget $b -WindowSteps $w -Threshold 0.01 -LossSeries $flat -WithNewSign $true
        $checked++
        if ($res.Signs -eq 1 -and $res.Shortened) { $falseAlarm++ }
        if ($res.Signs -eq 0 -and -not $res.Shortened) { $missed++ }
    }
}
Write-Output ("rows checked {0}; sign on a row the check does shorten {1}; silence on a row it cannot shorten {2}" -f $checked, $falseAlarm, $missed)
$zero = Invoke-BridgeLoop -StepBudget 1 -WindowSteps 25 -Threshold 0 -LossSeries $flat -WithNewSign $true
Write-Output ("threshold 0 (intentional disable): signs = {0}; executed = {1}; fired = {2}" -f $zero.Signs, $zero.Executed, $zero.Fired)
$huge = Test-Unreachable -DpoStepCount ([int]::MaxValue) -WindowSteps ([int]::MaxValue) -Threshold 0.002
Write-Output ("very large interval: Test-Unreachable(int.MaxValue, int.MaxValue) = {0}" -f $huge)
Write-Output ""

Write-Output "PANEL 5: a declining loss series must behave identically with and without the sign"
$declining = [float[]]@(0.90, 0.70, 0.50, 0.40, 0.35, 0.34, 0.339, 0.3385, 0.3384, 0.3384)
foreach ($w in 1, 2) {
    foreach ($b in 4, 6, 10) {
        $old = Invoke-BridgeLoop -StepBudget $b -WindowSteps $w -Threshold 0.01 -LossSeries $declining -WithNewSign $false
        $new = Invoke-BridgeLoop -StepBudget $b -WindowSteps $w -Threshold 0.01 -LossSeries $declining -WithNewSign $true
        Write-Output ("budget {0,4} interval {1,3} execOld {2,4} execNew {3,4} identical {4,5} fired {5,5} signNew {6}" -f $b, $w, $old.Executed, $new.Executed, ($old.Executed -eq $new.Executed), $new.Fired, $new.Signs)
    }
}
Write-Output ""

Write-Output "PANEL 6: the SECOND live caller (trainer wrapper), NOT fixed by this work"
Write-Output "maxSteps interval threshold exec fired shortened signAtThisCaller"
$trainerRows = @((25,25,0.01), (50,25,0.01), (74,25,0.01), (75,25,0.01), (76,25,0.01), (100,25,0.01), (1000,1,0.0))
foreach ($c in $trainerRows) {
    $res = Invoke-TrainerLoop -MaxSteps ([int]$c[0]) -WindowSteps ([int]$c[1]) -Threshold ([float]$c[2]) -FlatLoss 0.5
    Write-Output ("{0,8} {1,8} {2,9} {3,5} {4,5} {5,9} {6,16}" -f [int]$c[0], [int]$c[1], [float]$c[2], $res.Executed, $res.Fired, $res.Shortened, $res.Signs)
}
Write-Output ""

Write-Output "PANEL 7: accounting field StepsTaken after three observations at interval 25"
$acc = [Monitor]::new(0.01, 25)
$null = $acc.ShouldStop(0.5)
$null = $acc.ShouldStop(0.5)
$r3 = $acc.ShouldStop(0.5)
Write-Output ("third call returned {0}; StepsTaken = {1}" -f $r3, $acc.StepsTaken)
Write-Output ""

Write-Output "PANEL 8: row-level verdict, old product behaviour vs new product behaviour"
Write-Output "budget interval executedBefore executedAfter stepsChanged operatorToldBefore operatorToldAfter verdict"
$verdictRows = @((25,25), (50,25), (74,25), (75,25), (76,25), (100,25), (1000,331), (7,9991))
foreach ($r in $verdictRows) {
    $b = [int]$r[0]
    $w = [int]$r[1]
    $old = Invoke-BridgeLoop -StepBudget $b -WindowSteps $w -Threshold 0.01 -LossSeries $flat -WithNewSign $false
    $new = Invoke-BridgeLoop -StepBudget $b -WindowSteps $w -Threshold 0.01 -LossSeries $flat -WithNewSign $true
    $verdict = "unchanged-and-correct"
    if ($new.Signs -eq 1) { $verdict = "became-visible" }
    if ($old.Executed -ne $new.Executed) { $verdict = "REGRESSED-step-count" }
    if ($new.Signs -eq 1 -and $new.Shortened) { $verdict = "REGRESSED-false-alarm" }
    Write-Output ("{0,6} {1,8} {2,14} {3,13} {4,12} {5,17} {6,16} {7}" -f $b, $w, $old.Executed, $new.Executed, ($old.Executed -ne $new.Executed), "no", ($new.Signs -eq 1 ? "yes" : "no"), $verdict)
}
