namespace OrchestratorModel;

using AgentInfrastructure;

// Four stop signals for decomposition (E33).
// E33 note: algorithm is correct — Tier 2 synthetic data was trivially solvable (E33 FAILED status).
// Real signal requires genuine multi-step episodes.
public sealed class StopSignalDetector : BaseService, IStopSignalDetector
{
    // FC-12: ISessionStorage first parameter → base(storage).
    public StopSignalDetector(ISessionStorage storage) : base(storage) { }

    // Returns true when the sub-task should NOT be decomposed further.
    public OperationResult<bool> ShouldStop(string subTaskDescription)
    {
        if (string.IsNullOrWhiteSpace(subTaskDescription))
            return OperationResult<bool>.Fail("Description cannot be empty.", "EMPTY_TASK");

        var lower = subTaskDescription.ToLowerInvariant();

        // Signal 1 — PHASES: ordered steps that are not independent objectives.
        bool isPhaseSequence =
            new[] { "then ", "next ", "after that", "first step", "step " }
            .Any(w => lower.Contains(w)) &&
            !lower.Contains("goal") && !lower.Contains("objective");
        if (isPhaseSequence) return OperationResult<bool>.Ok(true);

        // Signal 2 — TECH: sub-task requires technical stack context to resolve.
        bool requiresTechContext =
            (lower.Contains("configure") && lower.Contains("service")) ||
            (lower.Contains("register") && lower.Contains("dependency")) ||
            (lower.Contains("inject")    && lower.Contains("interface"));
        if (requiresTechContext) return OperationResult<bool>.Ok(true);

        // Signal 3 — SINGLE_DOC: describes one atomic document or file.
        bool isSingleDoc =
            lower.Contains("single ") ||
            lower.Contains("one document") ||
            lower.Contains("one file") ||
            lower.Contains("one-time");
        if (isSingleDoc) return OperationResult<bool>.Ok(true);

        // Signal 4 — NO_LOOP: short task with no iterative quality signal.
        bool hasNoLoop =
            !lower.Contains("review")    && !lower.Contains("iterate") &&
            !lower.Contains("revise")    && !lower.Contains("feedback") &&
            !lower.Contains("correction");
        // ≤ 4 words: short atomic leaf operation. 5+ words = substantive goal, keep it.
        if (hasNoLoop && subTaskDescription.Split(' ').Length <= 4)
            return OperationResult<bool>.Ok(true);

        return OperationResult<bool>.Ok(false);
    }
}
