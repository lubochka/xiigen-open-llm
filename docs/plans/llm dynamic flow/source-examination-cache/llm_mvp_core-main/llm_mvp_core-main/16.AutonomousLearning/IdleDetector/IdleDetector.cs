namespace AutonomousLearning;

using AgentInfrastructure;

// IdleDetectorResult — outcome of the E41 four-condition idle check.
// SafeToTrain is returned only when ALL four conditions pass simultaneously.
public enum IdleDetectorResult
{
    SafeToTrain,     // All four E41 conditions met — proceed with autonomous training
    WaitHighCpu,     // CPU ≥ 20% — machine is actively used; defer to avoid slowdown
    WaitRecentInput, // User input within last 10 minutes — user is present at the machine
    WaitLowBattery,  // Battery < 50% and not charging — protect device battery life
    WaitLowDisk      // Free disk < 2 GB — insufficient space for dataset + LoRA artefacts
}

// IdleDetector — E41 algorithm: four-condition idle check before autonomous training.
// Experiment E41 PASSED with +100% safe-detection rate vs. single-condition baseline.
// Conditions checked in evaluation order: CPU, input, battery, disk.
// All four must pass; the first failing condition is returned immediately.
public sealed class IdleDetector : BaseService
{
    // E41 threshold: CPU utilisation must stay below 20% to avoid impacting foreground work.
    private const double CpuThreshold = 20.0;

    // E41 threshold: 10 minutes of input silence confirms the user has stepped away.
    private const double InputIdleThresholdMinutes = 10.0;

    // E41 threshold: 50% battery minimum preserves device mobility and protects the cell.
    private const double BatteryThreshold = 50.0;

    // E41 threshold: 2 GB free space required for downloaded dataset files + LoRA adapter output.
    private const double DiskFreeGbThreshold = 2.0;

    // Constructor: receives ISessionStorage from dependency injection, forwarded to BaseService.
    public IdleDetector(ISessionStorage storage) : base(storage) { }

    // Check — evaluate all four E41 conditions against the supplied machine snapshot.
    // Returns OperationResult<IdleDetectorResult>.Ok(...) for every outcome —
    // Fail is only used when status itself is invalid (future extension point).
    // Condition order: CPU → input → battery → disk (cheapest checks first).
    public OperationResult<IdleDetectorResult> Check(SystemStatus status)
    {
        // Condition 1: CPU must be below 20%; any higher means the machine is under active load.
        if (status.CpuPercent >= CpuThreshold)
            return OperationResult<IdleDetectorResult>.Ok(IdleDetectorResult.WaitHighCpu);

        // Condition 2: Last input event must be more than 10 minutes ago; less means user present.
        if (status.InputIdleMinutes < InputIdleThresholdMinutes)
            return OperationResult<IdleDetectorResult>.Ok(IdleDetectorResult.WaitRecentInput);

        // Condition 3: Either AC is connected (IsCharging) or battery level is at least 50%.
        // Both conditions failing together means training could drain the battery dangerously.
        if (!status.IsCharging && status.BatteryPercent < BatteryThreshold)
            return OperationResult<IdleDetectorResult>.Ok(IdleDetectorResult.WaitLowBattery);

        // Condition 4: Free disk must be at least 2 GB to store dataset samples and LoRA output.
        if (status.DiskFreeGb < DiskFreeGbThreshold)
            return OperationResult<IdleDetectorResult>.Ok(IdleDetectorResult.WaitLowDisk);

        // All four E41 conditions passed — the machine is idle and resources are sufficient.
        return OperationResult<IdleDetectorResult>.Ok(IdleDetectorResult.SafeToTrain);
    }
}
