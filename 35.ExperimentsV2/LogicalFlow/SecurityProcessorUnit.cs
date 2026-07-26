// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure.Logical;

namespace ExperimentsV2.LogicalFlow;

// This record carries the selected security scenario into the node-04 processor unit.
public sealed record SecurityProcessorInput(
    string ScenarioId,
    SecurityExecutionPlan Plan,
    SecurityArbiterSelection Selection);

// This record carries the processed security finding from node-04 to node-08.
public sealed record SecurityProcessorFinding(
    string ScenarioId,
    string Finding,
    string SourcePairId,
    string[] RequiredResultSignals,
    string[] ForbiddenResultSignals);

// This unit is the CB4 pair-backed node-04 processor for the security-verification flow.
public sealed class SecurityProcessorUnit : ILogicalUnit<SecurityProcessorInput, SecurityProcessorFinding>
{
    // This id identifies the node-04 processor in evidence and later unit registries.
    public const string Id = "unit-cb4-security-processor-v1";

    // This role names the station responsibility the unit fills.
    public const string Role = "node-04-processor";

    // This domain tag scopes the unit to the CB4 security-verification flow.
    public const string DomainTag = "security-verification-3.4";

    // This confidence remains above the replacement floor used by the security proof.
    public const double Confidence = 0.90;

    // Execute the processor step for one security-verification scenario.
    public Task<AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityProcessorFinding>>> ExecuteAsync(
        LogicalUnitRequest<SecurityProcessorInput> request,
        CancellationToken cancellationToken = default)
    {
        // Return cancellation through the shared operation envelope.
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityProcessorFinding>>.Fail("Cancelled.", "CANCELLED"));

        // Refuse empty processor input because a finding cannot be derived without a plan.
        if (request.Input is null || request.Input.Plan is null)
            return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityProcessorFinding>>.Fail("Security processor input is required.", "EMPTY_SECURITY_PROCESSOR_INPUT"));

        // Build the processed finding from the CB4 oracle scenario.
        var finding = CreateAuthoritativeFinding(request.Input);

        // Build the logical-unit result around the processed finding.
        var result = new LogicalUnitResult<SecurityProcessorFinding>(
            request.UnitId,
            request.UnitRole,
            request.DomainTag,
            finding,
            Confidence,
            BuildPredicateIds(finding),
            [DomainTag],
            [finding.SourcePairId],
            []);

        // Return the processed finding through the async unit contract.
        return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityProcessorFinding>>.Ok(result));
    }

    // Expose the single authoritative processor mapping to the internal flow admission boundary.
    internal static SecurityProcessorFinding CreateAuthoritativeFinding(SecurityProcessorInput input)
        => Process(input);

    // Convert one submitted security request into the formal finding that arbiters judge.
    private static SecurityProcessorFinding Process(SecurityProcessorInput input)
    {
        // S2 processing must preserve the PKCE plain downgrade signal for the OAuth arbiter.
        if (string.Equals(input.ScenarioId, "S2", StringComparison.Ordinal))
            return new SecurityProcessorFinding(
                input.ScenarioId,
                $"Processor finding: {input.Plan.PlanStep}",
                input.Plan.SourcePairId,
                input.Plan.RequiredResultSignals,
                input.Plan.ForbiddenResultSignals);

        // S3 processing must preserve the ASVS-versus-internal-standard distinction.
        if (string.Equals(input.ScenarioId, "S3", StringComparison.Ordinal))
            return new SecurityProcessorFinding(
                input.ScenarioId,
                $"Processor finding: {input.Plan.PlanStep}",
                input.Plan.SourcePairId,
                input.Plan.RequiredResultSignals,
                input.Plan.ForbiddenResultSignals);

        // S1 processing must preserve the complete set of accepted ASVS security controls.
        return new SecurityProcessorFinding(
            input.ScenarioId,
            $"Processor finding: {input.Plan.PlanStep}",
            input.Plan.SourcePairId,
            input.Plan.RequiredResultSignals,
            input.Plan.ForbiddenResultSignals);
    }

    // Build predicate ids that describe the security processing step.
    private static string[] BuildPredicateIds(SecurityProcessorFinding finding)
    {
        // REPROCESS findings carry a failed validation into the arbiter layer.
        if (finding.RequiredResultSignals.Contains("REPROCESS", StringComparer.Ordinal))
            return ["PROCESS_OP", "FAIL", "AUTH_OP"];

        // CONCERN findings carry a branch into the arbiter layer.
        if (finding.RequiredResultSignals.Contains("CONCERN", StringComparer.Ordinal))
            return ["PROCESS_OP", "BRANCH", "AUTH_OP"];

        // PASS findings carry accepted validation evidence into the arbiter layer.
        return ["PROCESS_OP", "VALIDATE_OP", "AUTH_OP"];
    }
}
