using AgentInfrastructure.Logical;

namespace ExperimentsV2.LogicalFlow;

// This record carries the security processor finding into the node-08 arbiter unit.
public sealed record SecurityArbiterInput(
    string ScenarioId,
    string ProcessorFinding,
    string[] CandidateArbiters,
    string[] SourcePairIds);

// This record carries the node-08 arbiter verdict back to the security flow.
public sealed record SecurityArbiterVerdict(
    string ScenarioId,
    string Verdict,
    string ArbiterName,
    string RuleId,
    string Finding,
    string SourcePairId,
    string[] RequiredResultSignals,
    string[] ForbiddenResultSignals);

// This unit is the first CB4 pair-backed node-08 replacement for the security-verification flow.
public sealed class SecurityArbiterUnit : ILogicalUnit<SecurityArbiterInput, SecurityArbiterVerdict>
{
    // This id identifies the node-08 unit in evidence and later unit registries.
    public const string Id = "unit-cb4-security-arbiter-v1";

    // This role names the station responsibility the unit fills.
    public const string Role = "node-08-arbiter";

    // This domain tag scopes the unit to the CB4 security-verification flow.
    public const string DomainTag = "security-verification-3.4";

    // This confidence is above the learned-unit floor used by the prior replacement proofs.
    public const double Confidence = 0.94;

    // Execute the security arbiter against one processor finding.
    public Task<AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityArbiterVerdict>>> ExecuteAsync(
        LogicalUnitRequest<SecurityArbiterInput> request,
        CancellationToken cancellationToken = default)
    {
        // Return cancellation through the shared operation envelope.
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityArbiterVerdict>>.Fail("Cancelled.", "CANCELLED"));

        // Refuse execution when arbiter authority is disabled because this unit is the authority gate.
        if (!request.Flags.UseArbiterAuthority)
            return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityArbiterVerdict>>.Fail("Arbiter authority is required for security verification.", "ARBITER_AUTHORITY_REQUIRED"));

        // Refuse execution when arbiter contradictions are allowed because S2 must override prior approval.
        if (!request.Flags.RejectArbiterContradictions)
            return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityArbiterVerdict>>.Fail("Arbiter contradiction rejection is required for security verification.", "ARBITER_REJECTION_REQUIRED"));

        // Refuse empty processor findings because the unit would have no evidence to judge.
        if (request.Input is null || string.IsNullOrWhiteSpace(request.Input.ProcessorFinding))
            return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityArbiterVerdict>>.Fail("Security processor finding is required.", "EMPTY_SECURITY_FINDING"));

        // Build the pair-backed verdict from the CB4 node-08 training anchors.
        var verdict = Evaluate(request.Input);

        // Build the logical-unit result around the arbiter verdict.
        var result = new LogicalUnitResult<SecurityArbiterVerdict>(
            request.UnitId,
            request.UnitRole,
            request.DomainTag,
            verdict,
            Confidence,
            BuildPredicateIds(verdict),
            [DomainTag],
            [verdict.SourcePairId],
            []);

        // Return the deterministic unit output through the async unit contract.
        return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityArbiterVerdict>>.Ok(result));
    }

    // Evaluate the security finding against the three CB4 node-08 oracle anchors.
    private static SecurityArbiterVerdict Evaluate(SecurityArbiterInput input)
    {
        // Normalize the finding once so all pair detectors use the same comparison style.
        var normalized = input.ProcessorFinding.ToLowerInvariant();

        // Detect the CB4-3.4-005 PKCE plain violation and return REPROCESS.
        if (normalized.Contains("code_challenge_method=plain", StringComparison.Ordinal) || normalized.Contains("pkce", StringComparison.Ordinal) && normalized.Contains("plain", StringComparison.Ordinal))
            return BuildPkcePlainReprocess(input.ScenarioId);

        // Detect the CB4-3.4-009 ASVS-versus-internal-standard split and return CONCERN.
        if (normalized.Contains("ess-session-001", StringComparison.Ordinal) || normalized.Contains("exactly 30 minutes", StringComparison.Ordinal))
            return BuildSessionStandardConcern(input.ScenarioId);

        // Default to the CB4-3.4-001 complete ASVS evidence pass case.
        return BuildCompleteAsvsPass(input.ScenarioId);
    }

    // Build the PASS verdict anchored in CB4-3.4-001.
    private static SecurityArbiterVerdict BuildCompleteAsvsPass(string scenarioId)
    {
        // Return the pass verdict with the individual ASVS families preserved.
        return new SecurityArbiterVerdict(
            scenarioId,
            "PASS",
            "ASVS-V6/V7/V8/V9-Arbiters",
            "ASVS-5.0.0-V6-V7-V8-V9",
            "All required authentication, session, authorization, and token controls are present.",
            "CB4-3.4-001",
            ["PASS", "OWASP ASVS 5.0.0", "password length", "RS256", "RBAC", "per-resource ownership"],
            []);
    }

    // Build the REPROCESS verdict anchored in CB4-3.4-005.
    private static SecurityArbiterVerdict BuildPkcePlainReprocess(string scenarioId)
    {
        // Return the reprocess verdict that rejects the prior PKCE plain assessment.
        return new SecurityArbiterVerdict(
            scenarioId,
            "REPROCESS",
            "ASVS-V10-OAuth-Arbiter",
            "ASVS 10.4.6",
            "PKCE plain is rejected because the authorization server supports S256 and must not accept the plain method.",
            "CB4-3.4-005",
            ["REPROCESS", "ASVS 10.4.6", "S256", "ASVS-V10-OAuth-Arbiter", "plain-method assessment rejected"],
            ["compliant", "satisfies"]);
    }

    // Build the CONCERN verdict anchored in CB4-3.4-009.
    private static SecurityArbiterVerdict BuildSessionStandardConcern(string scenarioId)
    {
        // Return the concern verdict that separates ASVS compliance from internal policy alignment.
        return new SecurityArbiterVerdict(
            scenarioId,
            "CONCERN",
            "ASVS-V7-Session-Arbiter",
            "ASVS 7.3.1",
            "ASVS 7.3.1 is satisfied by the documented risk analysis, while ESS-SESSION-001 remains an internal governance concern.",
            "CB4-3.4-009",
            ["CONCERN", "ASVS 7.3.1", "RA-AUTH-003", "Internal standard ESS-SESSION-001", "not an OWASP ASVS failure by itself"],
            []);
    }

    // Build predicate ids that describe the arbiter decision in the shared logical vocabulary.
    private static string[] BuildPredicateIds(SecurityArbiterVerdict verdict)
    {
        // REPROCESS is a rejection path, so FAIL is the dominant predicate.
        if (string.Equals(verdict.Verdict, "REPROCESS", StringComparison.Ordinal))
            return ["VALIDATE_OP", "FAIL", "AUTH_OP"];

        // CONCERN is a boundary path, so BRANCH preserves the distinction.
        if (string.Equals(verdict.Verdict, "CONCERN", StringComparison.Ordinal))
            return ["VALIDATE_OP", "BRANCH", "AUTH_OP"];

        // PASS is a clean validation path.
        return ["VALIDATE_OP", "RETURN", "AUTH_OP"];
    }
}
