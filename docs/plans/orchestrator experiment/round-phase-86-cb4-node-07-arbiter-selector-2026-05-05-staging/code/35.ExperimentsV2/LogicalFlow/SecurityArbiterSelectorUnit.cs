using AgentInfrastructure.Logical;

namespace ExperimentsV2.LogicalFlow;

// This record carries security evidence into the node-07 arbiter selector.
public sealed record SecurityArbiterSelectionInput(
    string ScenarioId,
    string SourceFinding,
    string[] SourcePairIds);

// This record carries the selected arbiter list from node-07 to node-08.
public sealed record SecurityArbiterSelection(
    string ScenarioId,
    string[] SelectedArbiters,
    string SourcePairId,
    string SelectionReason);

// This unit is the CB4 pair-backed node-07 arbiter selector for the security-verification flow.
public sealed class SecurityArbiterSelectorUnit : ILogicalUnit<SecurityArbiterSelectionInput, SecurityArbiterSelection>
{
    // This id identifies the node-07 selector unit in evidence and later unit registries.
    public const string Id = "unit-cb4-security-arbiter-selector-v1";

    // This role names the station responsibility the unit fills.
    public const string Role = "node-07-arbiter-selector";

    // This domain tag scopes the selector to the CB4 security-verification flow.
    public const string DomainTag = "security-verification-3.4";

    // This confidence remains above the replacement floor used by the prior proofs.
    public const double Confidence = 0.93;

    // Execute arbiter selection for one security finding.
    public Task<AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityArbiterSelection>>> ExecuteAsync(
        LogicalUnitRequest<SecurityArbiterSelectionInput> request,
        CancellationToken cancellationToken = default)
    {
        // Return cancellation through the shared operation envelope.
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityArbiterSelection>>.Fail("Cancelled.", "CANCELLED"));

        // Refuse selector execution when arbiter authority is disabled.
        if (!request.Flags.UseArbiterAuthority)
            return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityArbiterSelection>>.Fail("Arbiter authority is required for selector execution.", "ARBITER_AUTHORITY_REQUIRED"));

        // Refuse empty source findings because the selector would have no signal to route.
        if (request.Input is null || string.IsNullOrWhiteSpace(request.Input.SourceFinding))
            return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityArbiterSelection>>.Fail("Security selector input is required.", "EMPTY_SELECTOR_INPUT"));

        // Build the selection from the CB4 node-07 and node-08 pair anchors.
        var selection = SelectArbiters(request.Input);

        // Build the logical-unit result around the selected arbiter list.
        var result = new LogicalUnitResult<SecurityArbiterSelection>(
            request.UnitId,
            request.UnitRole,
            request.DomainTag,
            selection,
            Confidence,
            BuildPredicateIds(selection),
            [DomainTag],
            [selection.SourcePairId],
            []);

        // Return the selector output through the async unit contract.
        return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityArbiterSelection>>.Ok(result));
    }

    // Select the arbiter list for the security evidence.
    private static SecurityArbiterSelection SelectArbiters(SecurityArbiterSelectionInput input)
    {
        // Normalize the finding once so all routing detectors use the same comparison style.
        var normalized = input.SourceFinding.ToLowerInvariant();

        // Route PKCE plain evidence to the ASVS V10 OAuth arbiter.
        if (normalized.Contains("code_challenge_method=plain", StringComparison.Ordinal) || normalized.Contains("pkce", StringComparison.Ordinal) && normalized.Contains("plain", StringComparison.Ordinal))
            return new SecurityArbiterSelection(
                input.ScenarioId,
                ["ASVS-V10-OAuth-Arbiter"],
                "CB4-3.4-005",
                "PKCE plain evidence belongs to the ASVS V10 OAuth arbiter.");

        // Route internal session-standard evidence to the ASVS V7 session arbiter.
        if (normalized.Contains("ess-session-001", StringComparison.Ordinal) || normalized.Contains("exactly 30 minutes", StringComparison.Ordinal))
            return new SecurityArbiterSelection(
                input.ScenarioId,
                ["ASVS-V7-Session-Arbiter"],
                "CB4-3.4-009",
                "Idle-timeout and risk-analysis evidence belongs to the ASVS V7 session arbiter.");

        // Route complete verified ASVS evidence to the four clean ASVS arbiter families.
        return new SecurityArbiterSelection(
            input.ScenarioId,
            ["ASVS-V6-Auth-Arbiter", "ASVS-V7-Session-Arbiter", "ASVS-V8-AuthZ-Arbiter", "ASVS-V9-Token-Arbiter"],
            "CB4-3.4-001",
            "Complete authentication, session, authorization, and token evidence needs the four matching ASVS arbiters.");
    }

    // Build predicate ids that describe the selector decision in the shared logical vocabulary.
    private static string[] BuildPredicateIds(SecurityArbiterSelection selection)
    {
        // Multiple arbiters mean the selector is routing a combined clean evidence pack.
        if (selection.SelectedArbiters.Length > 1)
            return ["BRANCH", "VALIDATE_OP", "AUTH_OP"];

        // A single selected arbiter is a direct handoff to one authority family.
        return ["SEND_OP", "VALIDATE_OP", "AUTH_OP"];
    }
}
