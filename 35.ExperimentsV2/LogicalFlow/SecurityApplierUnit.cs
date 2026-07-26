// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure.Logical;

namespace ExperimentsV2.LogicalFlow;

// This record carries the selected arbiter verdict into the node-05 applier unit.
public sealed record SecurityApplicationInput(
    string ScenarioId,
    SecurityArbiterSelection Selection,
    SecurityArbiterVerdict ArbiterVerdict);

// This record carries the applied security result from node-05 to node-06.
public sealed record SecurityAppliedResult(
    string ScenarioId,
    string Verdict,
    string AppliedAction,
    SecurityArbiterVerdict ArbiterVerdict,
    string SourcePairId,
    string[] RequiredResultSignals,
    string[] ForbiddenResultSignals);

// This unit is the CB4 pair-backed node-05 applier for the security-verification flow.
public sealed class SecurityApplierUnit : ILogicalUnit<SecurityApplicationInput, SecurityAppliedResult>
{
    // This id identifies the node-05 applier in evidence and later unit registries.
    public const string Id = "unit-cb4-security-applier-v1";

    // This role names the station responsibility the unit fills.
    public const string Role = "node-05-applier";

    // This domain tag scopes the unit to the CB4 security-verification flow.
    public const string DomainTag = "security-verification-3.4";

    // This confidence remains above the replacement floor used by the security proof.
    public const double Confidence = 0.91;

    // Execute the application step for one arbiter verdict.
    public Task<AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityAppliedResult>>> ExecuteAsync(
        LogicalUnitRequest<SecurityApplicationInput> request,
        CancellationToken cancellationToken = default)
    {
        // Return cancellation through the shared operation envelope.
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityAppliedResult>>.Fail("Cancelled.", "CANCELLED"));

        // Refuse empty applier input because application needs a concrete arbiter verdict.
        if (request.Input is null || request.Input.ArbiterVerdict is null)
            return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityAppliedResult>>.Fail("Security application input is required.", "EMPTY_SECURITY_APPLICATION_INPUT"));

        // Build the applied result from the pair-backed verdict class.
        var applied = CreateAuthoritativeApplication(request.Input);

        // Build the logical-unit result around the applied security result.
        var result = new LogicalUnitResult<SecurityAppliedResult>(
            request.UnitId,
            request.UnitRole,
            request.DomainTag,
            applied,
            Confidence,
            BuildPredicateIds(applied),
            [DomainTag],
            [applied.SourcePairId],
            []);

        // Return the applied result through the async unit contract.
        return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityAppliedResult>>.Ok(result));
    }

    // Expose the single deterministic application oracle to the in-assembly flow admission boundary.
    internal static SecurityAppliedResult CreateAuthoritativeApplication(SecurityApplicationInput input)
        => Apply(input);

    // Apply the arbiter verdict to the security flow's formal result path.
    private static SecurityAppliedResult Apply(SecurityApplicationInput input)
    {
        // Build the applied action according to the verdict class.
        var action = input.ArbiterVerdict.Verdict switch
        {
            // REPROCESS application blocks advancement and sends the failed finding back for correction.
            "REPROCESS" => $"Apply REPROCESS: block advancement and return {input.ArbiterVerdict.RuleId} to the correction path.",

            // CONCERN application preserves the pass while keeping the internal policy issue visible.
            "CONCERN" => $"Apply CONCERN: keep {input.ArbiterVerdict.RuleId} visible as an internal-governance boundary.",

            // PASS application lets accepted evidence continue to result translation.
            _ => $"Apply PASS: accept {input.ArbiterVerdict.RuleId} evidence and continue to result translation."
        };

        // Return the applied result with the arbiter verdict attached for downstream traceability.
        return new SecurityAppliedResult(
            input.ScenarioId,
            input.ArbiterVerdict.Verdict,
            action,
            input.ArbiterVerdict,
            input.ArbiterVerdict.SourcePairId,
            input.ArbiterVerdict.RequiredResultSignals,
            input.ArbiterVerdict.ForbiddenResultSignals);
    }

    // Build predicate ids that describe the application decision in the shared logical vocabulary.
    private static string[] BuildPredicateIds(SecurityAppliedResult applied)
    {
        // REPROCESS application carries a blocking failure forward.
        if (string.Equals(applied.Verdict, "REPROCESS", StringComparison.Ordinal))
            return ["UPDATE_OP", "FAIL", "RETURN"];

        // CONCERN application carries a preserved boundary forward.
        if (string.Equals(applied.Verdict, "CONCERN", StringComparison.Ordinal))
            return ["UPDATE_OP", "BRANCH", "RETURN"];

        // PASS application carries accepted evidence forward.
        return ["UPDATE_OP", "VALIDATE_OP", "RETURN"];
    }
}
