# Mutation-sensitivity panel: run the exact InlineData rows of the new xUnit
# tests against three versions of the predicate and report pass/fail per row.
#   v0 = product at tip 9412c931df5  : no predicate, no sign at all
#   v1 = my first boundary           : dpoStepCount <  3 * interval
#   v2 = shipped boundary            : dpoStepCount <= 3 * interval
$ErrorActionPreference = "Stop"

class Monitor {
    [float]$Threshold
    [int]$WindowSteps
    [float]$PreviousLoss
    [int]$StableWindowCount
    [int]$StepsTaken
    Monitor([float]$threshold, [int]$windowSteps) {
        $this.Threshold = [Math]::Max([float]0, $threshold)
        $this.WindowSteps = [Math]::Max(1, $windowSteps)
        $this.PreviousLoss = [float]::MaxValue
        $this.StableWindowCount = 0
        $this.StepsTaken = 0
    }
    [bool] ShouldStop([float]$currentLoss) {
        $this.StepsTaken += $this.WindowSteps
        if ($this.PreviousLoss -eq [float]::MaxValue) {
            $this.PreviousLoss = $currentLoss
            return $false
        }
        if ([Math]::Abs($this.PreviousLoss - $currentLoss) -lt $this.Threshold) {
            $this.StableWindowCount++
        } else {
            $this.StableWindowCount = 0
        }
        $this.PreviousLoss = $currentLoss
        return $this.StableWindowCount -ge 2
    }
}

function Get-Executed {
    param([int]$StepBudget, [int]$WindowSteps, [float]$Threshold, [float]$FlatLoss)
    $monitor = $null
    if ($Threshold -ne [float]0) { $monitor = [Monitor]::new($Threshold, $WindowSteps) }
    $actual = 0
    for ($step = 1; $step -le $StepBudget; $step++) {
        $actual = $step
        if ($null -ne $monitor -and ($step % $WindowSteps) -eq 0 -and $monitor.ShouldStop($FlatLoss)) { break }
    }
    return $actual
}

function Pred { param([string]$Version, [int]$B, [int]$W, [float]$T)
    if ($Version -eq "v1") { return ($T -ne [float]0) -and ($W -ge 1) -and ([long]$B -lt (3 * [long]$W)) }
    return ($T -ne [float]0) -and ($W -ge 1) -and ([long]$B -le (3 * [long]$W))
}

Write-Output "TEST UnreachabilityPredicate_AgreesWithSimulatedLoopBehaviour (10 InlineData rows)"
Write-Output "budget interval executed expectedPredicate v1 v2 v1row v2row"
$rows = @((1,1), (2,1), (3,1), (4,1), (24,25), (25,25), (50,25), (74,25), (75,25), (100,25))
$v1Fail = 0
$v2Fail = 0
foreach ($r in $rows) {
    $b = [int]$r[0]
    $w = [int]$r[1]
    $exec = Get-Executed -StepBudget $b -WindowSteps $w -Threshold 0.01 -FlatLoss 0.5
    $expected = ($exec -eq $b)
    $p1 = Pred -Version "v1" -B $b -W $w -T 0.01
    $p2 = Pred -Version "v2" -B $b -W $w -T 0.01
    $r1 = ($p1 -eq $expected) ? "PASS" : "FAIL"
    $r2 = ($p2 -eq $expected) ? "PASS" : "FAIL"
    if ($r1 -eq "FAIL") { $v1Fail++ }
    if ($r2 -eq "FAIL") { $v2Fail++ }
    Write-Output ("{0,6} {1,8} {2,8} {3,17} {4,5} {5,5} {6,5} {7,5}" -f $b, $w, $exec, $expected, $p1, $p2, $r1, $r2)
}
Write-Output ("rows failing on v1 (my first boundary): {0}; rows failing on v2 (shipped): {1}" -f $v1Fail, $v2Fail)
Write-Output ""

Write-Output "TEST BridgeLoop_AtExactlyThreeIntervalsDetectsOnLastStepAndSavesNothing"
$exec75 = Get-Executed -StepBudget 75 -WindowSteps 25 -Threshold 0.01 -FlatLoss 0.5
$m = [Monitor]::new(0.01, 25)
$c1 = $m.ShouldStop(0.5)
$c2 = $m.ShouldStop(0.5)
$c3 = $m.ShouldStop(0.5)
$p1 = Pred -Version "v1" -B 75 -W 25 -T 0.01
$p2 = Pred -Version "v2" -B 75 -W 25 -T 0.01
Write-Output ("executed {0} (expected 75); monitor calls {1}/{2}/{3} (expected False/False/True)" -f $exec75, $c1, $c2, $c3)
Write-Output ("predicate v1 {0} -> {1}; predicate v2 {2} -> {3} (test asserts True)" -f $p1, (($p1) ? "PASS" : "FAIL"), $p2, (($p2) ? "PASS" : "FAIL"))
Write-Output ""

Write-Output "TEST v0 (product at tip): ConvergenceDetectionIsUnreachable / TraceUnreachableConvergenceDetection do not exist"
Write-Output "4 of 8 tests reference those members, so on v0 the test assembly does not compile: hard FAIL, not a green pass."
Write-Output ""

Write-Output "TEST ReachableConfiguration_EmitsNoDiagnosticWarning (1000 / 331) and UnreachableConfiguration (7 / 9991)"
foreach ($r in @((1000,331), (7,9991))) {
    $b = [int]$r[0]
    $w = [int]$r[1]
    $exec = Get-Executed -StepBudget $b -WindowSteps $w -Threshold 0.002 -FlatLoss 0.5
    $p2 = Pred -Version "v2" -B $b -W $w -T 0.002
    Write-Output ("budget {0,6} interval {1,6} executed {2,6} shortened {3,6} signExpected {4}" -f $b, $w, $exec, ($exec -lt $b), $p2)
}
