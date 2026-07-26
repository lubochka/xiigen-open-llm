// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure.Logical;

namespace ExperimentsV2.LogicalFlow;

// This record carries selected security evidence into the node-03 planner unit.
public sealed record SecurityPlannerInput(
    string ScenarioId,
    SecurityDomainTranslation Translation,
    SecurityArbiterSelection Selection);

// This record carries the planned security verification step from node-03 to node-04.
public sealed record SecurityExecutionPlan(
    string ScenarioId,
    string PlanStep,
    string SourcePairId,
    string[] RequiredResultSignals,
    string[] ForbiddenResultSignals);

// This unit is the CB4 pair-backed node-03 planner for the security-verification flow.
public sealed class SecurityPlannerUnit : ILogicalUnit<SecurityPlannerInput, SecurityExecutionPlan>
{
    // This id identifies the node-03 planner in evidence and later unit registries.
    public const string Id = "unit-cb4-security-planner-v1";

    // This role names the station responsibility the unit fills.
    public const string Role = "node-03-planner";

    // This domain tag scopes the unit to the CB4 security-verification flow.
    public const string DomainTag = "security-verification-3.4";

    // This confidence remains above the replacement floor used by the security proof.
    public const double Confidence = 0.89;

    // Execute the planner step for one security-verification scenario.
    public Task<AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityExecutionPlan>>> ExecuteAsync(
        LogicalUnitRequest<SecurityPlannerInput> request,
        CancellationToken cancellationToken = default)
    {
        // Return cancellation through the shared operation envelope.
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityExecutionPlan>>.Fail("Cancelled.", "CANCELLED"));

        // Refuse empty planner input because planning requires a formal translation and selected arbiters.
        if (request.Input is null || request.Input.Translation is null || request.Input.Selection is null)
            return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityExecutionPlan>>.Fail("Security planner input is required.", "EMPTY_SECURITY_PLANNER_INPUT"));

        // Build the execution plan from the CB4 oracle scenario.
        var plan = CreateAuthoritativePlan(request.Input);

        // Build the logical-unit result around the execution plan.
        var result = new LogicalUnitResult<SecurityExecutionPlan>(
            request.UnitId,
            request.UnitRole,
            request.DomainTag,
            plan,
            Confidence,
            BuildPredicateIds(plan),
            [DomainTag],
            [plan.SourcePairId],
            []);

        // Return the execution plan through the async unit contract.
        return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityExecutionPlan>>.Ok(result));
    }

    // Expose the single authoritative planner mapping to the internal flow admission boundary.
    internal static SecurityExecutionPlan CreateAuthoritativePlan(SecurityPlannerInput input)
        => Plan(input);

    // Build the security verification plan for one oracle scenario.
    private static SecurityExecutionPlan Plan(SecurityPlannerInput input)
    {
        // S2 planning must preserve the correction path for the PKCE plain downgrade.
        if (string.Equals(input.ScenarioId, "S2", StringComparison.Ordinal))
            return new SecurityExecutionPlan(
                input.ScenarioId,
                $"Plan: {input.Translation.FormalStatement} Preserve correction path if plain is accepted while S256 is supported.",
                input.Translation.SourcePairId,
                input.Translation.RequiredResultSignals,
                input.Translation.ForbiddenResultSignals);

        // S3 planning must preserve the split between ASVS compliance and an internal standard concern.
        if (string.Equals(input.ScenarioId, "S3", StringComparison.Ordinal))
            return new SecurityExecutionPlan(
                input.ScenarioId,
                $"Plan: {input.Translation.FormalStatement} Keep the boundary flag visible through arbiter review.",
                input.Translation.SourcePairId,
                input.Translation.RequiredResultSignals,
                input.Translation.ForbiddenResultSignals);

        // S17 planning must preserve the RFC 9700 compatibility argument rejection.
        if (string.Equals(input.ScenarioId, "S17", StringComparison.Ordinal))
            return new SecurityExecutionPlan(
                input.ScenarioId,
                $"Plan: {input.Translation.FormalStatement} Confirm optional PKCE leaves non-compliant public clients unprotected.",
                input.Translation.SourcePairId,
                input.Translation.RequiredResultSignals,
                input.Translation.ForbiddenResultSignals);

        // S18 planning must confirm AS-level rejection and client S256 usage.
        if (string.Equals(input.ScenarioId, "S18", StringComparison.Ordinal))
            return new SecurityExecutionPlan(
                input.ScenarioId,
                $"Plan: {input.Translation.FormalStatement} Confirm both AS enforcement and S256 client behavior.",
                input.Translation.SourcePairId,
                input.Translation.RequiredResultSignals,
                input.Translation.ForbiddenResultSignals);

        // S19 planning must preserve the multi-party signing key distribution risk.
        if (string.Equals(input.ScenarioId, "S19", StringComparison.Ordinal))
            return new SecurityExecutionPlan(
                input.ScenarioId,
                $"Plan: {input.Translation.FormalStatement} Compare HS256 key distribution against RS256 private-key isolation.",
                input.Translation.SourcePairId,
                input.Translation.RequiredResultSignals,
                input.Translation.ForbiddenResultSignals);

        // S20 planning must confirm private-key exclusivity and JWKS delivery.
        if (string.Equals(input.ScenarioId, "S20", StringComparison.Ordinal))
            return new SecurityExecutionPlan(
                input.ScenarioId,
                $"Plan: {input.Translation.FormalStatement} Confirm resource-server compromise cannot produce a signing key.",
                input.Translation.SourcePairId,
                input.Translation.RequiredResultSignals,
                input.Translation.ForbiddenResultSignals);

        // S21 planning must keep the high-volume services argument as a design concern, not a pass.
        if (string.Equals(input.ScenarioId, "S21", StringComparison.Ordinal))
            return new SecurityExecutionPlan(
                input.ScenarioId,
                $"Plan: {input.Translation.FormalStatement} Preserve that high-volume clients can use configured limits rather than no per-client counter.",
                input.Translation.SourcePairId,
                input.Translation.RequiredResultSignals,
                input.Translation.ForbiddenResultSignals);

        // S22 planning must confirm the per-client counter defeats distributed guessing.
        if (string.Equals(input.ScenarioId, "S22", StringComparison.Ordinal))
            return new SecurityExecutionPlan(
                input.ScenarioId,
                $"Plan: {input.Translation.FormalStatement} Confirm botnet source IP count cannot bypass the client_id counter.",
                input.Translation.SourcePairId,
                input.Translation.RequiredResultSignals,
                input.Translation.ForbiddenResultSignals);

        // S23 planning must preserve the security impact of forced logout.
        if (string.Equals(input.ScenarioId, "S23", StringComparison.Ordinal))
            return new SecurityExecutionPlan(
                input.ScenarioId,
                $"Plan: {input.Translation.FormalStatement} Preserve operational disruption, DoS, and phishing precursor risk.",
                input.Translation.SourcePairId,
                input.Translation.RequiredResultSignals,
                input.Translation.ForbiddenResultSignals);

        // S24 planning must confirm the id_token_hint validation chain blocks forced logout.
        if (string.Equals(input.ScenarioId, "S24", StringComparison.Ordinal))
            return new SecurityExecutionPlan(
                input.ScenarioId,
                $"Plan: {input.Translation.FormalStatement} Confirm signature, subject, audience, expiry, and session binding are validated.",
                input.Translation.SourcePairId,
                input.Translation.RequiredResultSignals,
                input.Translation.ForbiddenResultSignals);

        // S16 planning must preserve all four domains so the comprehensive pass does not drop a control family.
        if (string.Equals(input.ScenarioId, "S16", StringComparison.Ordinal))
            return new SecurityExecutionPlan(
                input.ScenarioId,
                $"Plan: {input.Translation.FormalStatement} Confirm no ext31 control family is omitted.",
                input.Translation.SourcePairId,
                input.Translation.RequiredResultSignals,
                input.Translation.ForbiddenResultSignals);

        // S10 planning must confirm both pass controls before advancing.
        if (string.Equals(input.ScenarioId, "S10", StringComparison.Ordinal))
            return new SecurityExecutionPlan(
                input.ScenarioId,
                $"Plan: {input.Translation.FormalStatement} Confirm throttling and logout protections both meet their thresholds.",
                input.Translation.SourcePairId,
                input.Translation.RequiredResultSignals,
                input.Translation.ForbiddenResultSignals);

        // S11 planning must keep the two MUST-level failures separate through arbiter review.
        if (string.Equals(input.ScenarioId, "S11", StringComparison.Ordinal))
            return new SecurityExecutionPlan(
                input.ScenarioId,
                $"Plan: {input.Translation.FormalStatement} Reject legacy reliability as an exception to either MUST-level rule.",
                input.Translation.SourcePairId,
                input.Translation.RequiredResultSignals,
                input.Translation.ForbiddenResultSignals);

        // S12 planning must confirm both RS256 and ROPC replacement before returning PASS.
        if (string.Equals(input.ScenarioId, "S12", StringComparison.Ordinal))
            return new SecurityExecutionPlan(
                input.ScenarioId,
                $"Plan: {input.Translation.FormalStatement} Confirm no resource server can forge tokens and no application collects user credentials.",
                input.Translation.SourcePairId,
                input.Translation.RequiredResultSignals,
                input.Translation.ForbiddenResultSignals);

        // S13 planning must preserve the causal training gap rather than treating it as background context.
        if (string.Equals(input.ScenarioId, "S13", StringComparison.Ordinal))
            return new SecurityExecutionPlan(
                input.ScenarioId,
                $"Plan: {input.Translation.FormalStatement} Keep SSDF PO.1 as a proximate-cause finding, not a footnote.",
                input.Translation.SourcePairId,
                input.Translation.RequiredResultSignals,
                input.Translation.ForbiddenResultSignals);

        // S14 planning must preserve the compound risk that forged tokens would also be non-expiring.
        if (string.Equals(input.ScenarioId, "S14", StringComparison.Ordinal))
            return new SecurityExecutionPlan(
                input.ScenarioId,
                $"Plan: {input.Translation.FormalStatement} Preserve forgeability and non-expiry as compounding risks.",
                input.Translation.SourcePairId,
                input.Translation.RequiredResultSignals,
                input.Translation.ForbiddenResultSignals);

        // S15 planning must preserve the graduation rule where REPROCESS absorbs the concern.
        if (string.Equals(input.ScenarioId, "S15", StringComparison.Ordinal))
            return new SecurityExecutionPlan(
                input.ScenarioId,
                $"Plan: {input.Translation.FormalStatement} Preserve that REPROCESS overrides the IP-only throttling concern.",
                input.Translation.SourcePairId,
                input.Translation.RequiredResultSignals,
                input.Translation.ForbiddenResultSignals);

        // S7 planning must keep both MUST-level failures independent so one issue cannot hide the other.
        if (string.Equals(input.ScenarioId, "S7", StringComparison.Ordinal))
            return new SecurityExecutionPlan(
                input.ScenarioId,
                $"Plan: {input.Translation.FormalStatement} Reject gradual-adoption and operational-simplicity arguments as normative exceptions.",
                input.Translation.SourcePairId,
                input.Translation.RequiredResultSignals,
                input.Translation.ForbiddenResultSignals);

        // S8 planning must confirm both positive controls before returning a pass.
        if (string.Equals(input.ScenarioId, "S8", StringComparison.Ordinal))
            return new SecurityExecutionPlan(
                input.ScenarioId,
                $"Plan: {input.Translation.FormalStatement} Confirm both PKCE enforcement and asymmetric signing are present.",
                input.Translation.SourcePairId,
                input.Translation.RequiredResultSignals,
                input.Translation.ForbiddenResultSignals);

        // S9 planning must preserve the graduation rule that multiple concerns remain concerns.
        if (string.Equals(input.ScenarioId, "S9", StringComparison.Ordinal))
            return new SecurityExecutionPlan(
                input.ScenarioId,
                $"Plan: {input.Translation.FormalStatement} Preserve the rule that concern plus concern does not become reprocess.",
                input.Translation.SourcePairId,
                input.Translation.RequiredResultSignals,
                input.Translation.ForbiddenResultSignals);

        // S4 planning must preserve the current-MUST correction path for public-client PKCE enforcement.
        if (string.Equals(input.ScenarioId, "S4", StringComparison.Ordinal))
            return new SecurityExecutionPlan(
                input.ScenarioId,
                $"Plan: {input.Translation.FormalStatement} Reject backward-compatibility as an exception to RFC 9700 2.1.1.",
                input.Translation.SourcePairId,
                input.Translation.RequiredResultSignals,
                input.Translation.ForbiddenResultSignals);

        // S5 planning must preserve the CONCERN level for a defense-in-depth throttling gap.
        if (string.Equals(input.ScenarioId, "S5", StringComparison.Ordinal))
            return new SecurityExecutionPlan(
                input.ScenarioId,
                $"Plan: {input.Translation.FormalStatement} Determine whether the gap is a specific MUST violation or a defense-in-depth concern.",
                input.Translation.SourcePairId,
                input.Translation.RequiredResultSignals,
                input.Translation.ForbiddenResultSignals);

        // S6 planning must preserve the PASS path for complete per-client-id throttling evidence.
        if (string.Equals(input.ScenarioId, "S6", StringComparison.Ordinal))
            return new SecurityExecutionPlan(
                input.ScenarioId,
                $"Plan: {input.Translation.FormalStatement} Confirm distributed attacks cannot bypass the per-client counter.",
                input.Translation.SourcePairId,
                input.Translation.RequiredResultSignals,
                input.Translation.ForbiddenResultSignals);

        // S1 planning must preserve the complete ASVS verification path.
        return new SecurityExecutionPlan(
            input.ScenarioId,
            $"Plan: {input.Translation.FormalStatement} Use the selected ASVS arbiter families.",
            input.Translation.SourcePairId,
            input.Translation.RequiredResultSignals,
            input.Translation.ForbiddenResultSignals);
    }

    // Build predicate ids that describe the security planning step.
    private static string[] BuildPredicateIds(SecurityExecutionPlan plan)
    {
        // REPROCESS plans carry a correction branch forward.
        if (plan.RequiredResultSignals.Contains("REPROCESS", StringComparer.Ordinal))
            return ["CREATE", "FAIL", "AUTH_OP"];

        // CONCERN plans carry a governance branch forward.
        if (plan.RequiredResultSignals.Contains("CONCERN", StringComparer.Ordinal))
            return ["CREATE", "BRANCH", "AUTH_OP"];

        // PASS plans carry a clean verification path forward.
        return ["CREATE", "VALIDATE_OP", "AUTH_OP"];
    }
}
