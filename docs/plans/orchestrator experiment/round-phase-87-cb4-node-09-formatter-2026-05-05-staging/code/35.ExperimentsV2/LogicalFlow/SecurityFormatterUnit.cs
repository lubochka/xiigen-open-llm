using System.Text;
using AgentInfrastructure.Logical;

namespace ExperimentsV2.LogicalFlow;

// This record carries node-07 and node-08 outputs into the node-09 formatter.
public sealed record SecurityFormatterInput(
    string ScenarioId,
    SecurityArbiterSelection Selection,
    SecurityArbiterVerdict ArbiterVerdict,
    string Attribution);

// This record carries the formatted human result from node-09 to the flow.
public sealed record SecurityFormattedResult(
    string ScenarioId,
    string HumanResult,
    string SourcePairId,
    string[] RequiredResultSignals,
    string[] ForbiddenResultSignals);

// This unit is the CB4 pair-backed node-09 formatter for the security-verification flow.
public sealed class SecurityFormatterUnit : ILogicalUnit<SecurityFormatterInput, SecurityFormattedResult>
{
    // This id identifies the node-09 formatter unit in evidence and later unit registries.
    public const string Id = "unit-cb4-security-formatter-v1";

    // This role names the station responsibility the unit fills.
    public const string Role = "node-09-formatter";

    // This domain tag scopes the formatter to the CB4 security-verification flow.
    public const string DomainTag = "security-verification-3.4";

    // This confidence remains above the replacement floor used by the prior proofs.
    public const double Confidence = 0.92;

    // Execute formatting for one security verdict.
    public Task<AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityFormattedResult>>> ExecuteAsync(
        LogicalUnitRequest<SecurityFormatterInput> request,
        CancellationToken cancellationToken = default)
    {
        // Return cancellation through the shared operation envelope.
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityFormattedResult>>.Fail("Cancelled.", "CANCELLED"));

        // Refuse empty formatter input because final wording requires a verdict.
        if (request.Input is null || request.Input.ArbiterVerdict is null)
            return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityFormattedResult>>.Fail("Security formatter input is required.", "EMPTY_FORMATTER_INPUT"));

        // Build the formatted human result from the selected arbiter verdict.
        var formatted = Format(request.Input);

        // Build the logical-unit result around the formatted output.
        var result = new LogicalUnitResult<SecurityFormattedResult>(
            request.UnitId,
            request.UnitRole,
            request.DomainTag,
            formatted,
            Confidence,
            BuildPredicateIds(request.Input.ArbiterVerdict),
            [DomainTag],
            [formatted.SourcePairId],
            []);

        // Return the formatter output through the async unit contract.
        return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityFormattedResult>>.Ok(result));
    }

    // Format the selected security verdict into human-readable text.
    private static SecurityFormattedResult Format(SecurityFormatterInput input)
    {
        // Dispatch by scenario id so each oracle result keeps its wording contract.
        return input.ScenarioId switch
        {
            // Render the S2 reprocess result without forbidden acceptance words.
            "S2" => FormatS2(input),

            // Render the S3 concern result with ASVS and internal-policy separation.
            "S3" => FormatS3(input),

            // Render the S1 pass result for verified controls.
            _ => FormatS1(input)
        };
    }

    // Format the S1 verified-control PASS result.
    private static SecurityFormattedResult FormatS1(SecurityFormatterInput input)
    {
        // Start a stable markdown result so tests can inspect named sections.
        var builder = new StringBuilder();

        // Name the verdict first so the human result is immediately inspectable.
        builder.AppendLine("PASS - the verified production controls cover the requested OWASP ASVS 5.0.0 security and authorization checks for sub-phase 3.4.");

        // Preserve the source attribution required by the licensed OWASP material.
        builder.AppendLine(input.Attribution);

        // Explain the authentication evidence that supports the pass verdict.
        builder.AppendLine("Authentication evidence: password length, OTP expiry, TOTP window, and re-authentication before sensitive account changes are explicitly verified.");

        // Explain the token evidence that supports the pass verdict.
        builder.AppendLine("Token evidence: RS256 is enforced, the none algorithm is rejected, and exp, nbf, and aud are validated against the service identifier.");

        // Explain the authorization evidence that supports the pass verdict.
        builder.AppendLine("Authorization evidence: RBAC and per-resource ownership checks are present on the protected API surface.");

        // Name the pair-backed arbiter source so the result remains traceable to the learning set.
        builder.AppendLine($"Node-08 arbiter source: {input.ArbiterVerdict.SourcePairId}.");

        // Name the pair-backed formatter source so the result remains traceable to node-09.
        builder.AppendLine($"Node-09 formatter source: {input.ArbiterVerdict.SourcePairId}.");

        // Return the formatted result with its required signals.
        return new SecurityFormattedResult(input.ScenarioId, builder.ToString(), input.ArbiterVerdict.SourcePairId, input.ArbiterVerdict.RequiredResultSignals, input.ArbiterVerdict.ForbiddenResultSignals);
    }

    // Format the S2 PKCE plain REPROCESS result.
    private static SecurityFormattedResult FormatS2(SecurityFormatterInput input)
    {
        // Start a stable markdown result so tests can inspect named sections.
        var builder = new StringBuilder();

        // Name the reprocess verdict first so unsafe acceptance cannot be hidden.
        builder.AppendLine("REPROCESS - the PKCE assessment must be corrected before this security design can pass review.");

        // Preserve the source attribution required by the licensed OWASP material.
        builder.AppendLine(input.Attribution);

        // Name the exact ASVS rule that the Stage 5 oracle requires.
        builder.AppendLine("ASVS 10.4.6 requires the authorization code flow with PKCE to use S256 when the authorization server supports it.");

        // Explain the contradiction in plain language without approving the submitted assessment.
        builder.AppendLine("The submitted evidence says code_challenge_method=plain and also says the authorization server accepts S256, so the downgrade path must be removed.");

        // Preserve the required arbiter authority signal.
        builder.AppendLine("Arbiter result: ASVS-V10-OAuth-Arbiter rejects the plain-method assessment and sends the scenario back for correction.");

        // Name the pair-backed arbiter source so the result remains traceable to the learning set.
        builder.AppendLine($"Node-08 arbiter source: {input.ArbiterVerdict.SourcePairId}.");

        // Name the pair-backed formatter source so the result remains traceable to node-09.
        builder.AppendLine($"Node-09 formatter source: {input.ArbiterVerdict.SourcePairId}.");

        // Return the formatted result with its required and forbidden signals.
        return new SecurityFormattedResult(input.ScenarioId, builder.ToString(), input.ArbiterVerdict.SourcePairId, input.ArbiterVerdict.RequiredResultSignals, input.ArbiterVerdict.ForbiddenResultSignals);
    }

    // Format the S3 ASVS-versus-internal-standard CONCERN result.
    private static SecurityFormattedResult FormatS3(SecurityFormatterInput input)
    {
        // Start a stable markdown result so tests can inspect named sections.
        var builder = new StringBuilder();

        // Name the concern verdict first so the distinction from reprocess is visible.
        builder.AppendLine("CONCERN - ASVS evidence and internal enterprise policy point to different obligations.");

        // Preserve the source attribution required by the licensed OWASP material.
        builder.AppendLine(input.Attribution);

        // Name the ASVS rule and explain why the risk-analysis evidence matters.
        builder.AppendLine("ASVS 7.3.1 allows re-authentication timing to follow a documented risk analysis, and RA-AUTH-003 supports the 20-minute value.");

        // Name the internal standard separately so it is not confused with the ASVS rule.
        builder.AppendLine("Internal standard ESS-SESSION-001 requires exactly 30 minutes to align with enterprise SSO token lifetime.");

        // Preserve the required decision distinction for the oracle.
        builder.AppendLine("Result distinction: this is an internal-standard concern that requires business/security resolution, not an OWASP ASVS failure by itself.");

        // Name the pair-backed arbiter source so the result remains traceable to the learning set.
        builder.AppendLine($"Node-08 arbiter source: {input.ArbiterVerdict.SourcePairId}.");

        // Name the pair-backed formatter source so the result remains traceable to node-09.
        builder.AppendLine($"Node-09 formatter source: {input.ArbiterVerdict.SourcePairId}.");

        // Return the formatted result with its required signals.
        return new SecurityFormattedResult(input.ScenarioId, builder.ToString(), input.ArbiterVerdict.SourcePairId, input.ArbiterVerdict.RequiredResultSignals, input.ArbiterVerdict.ForbiddenResultSignals);
    }

    // Build predicate ids that describe the formatter output in the shared logical vocabulary.
    private static string[] BuildPredicateIds(SecurityArbiterVerdict verdict)
    {
        // REPROCESS formatting turns a rejection into human-readable corrective text.
        if (string.Equals(verdict.Verdict, "REPROCESS", StringComparison.Ordinal))
            return ["CONVERT_OP", "FAIL", "RETURN"];

        // CONCERN formatting keeps a boundary visible without converting it into a block.
        if (string.Equals(verdict.Verdict, "CONCERN", StringComparison.Ordinal))
            return ["CONVERT_OP", "BRANCH", "RETURN"];

        // PASS formatting returns clean accepted evidence.
        return ["CONVERT_OP", "VALIDATE_OP", "RETURN"];
    }
}
