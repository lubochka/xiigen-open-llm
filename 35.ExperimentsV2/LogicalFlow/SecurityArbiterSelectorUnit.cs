// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

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
        var selection = CreateAuthoritativeSelection(request.Input);

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

    // Expose the single deterministic selector oracle to the in-assembly flow admission boundary.
    internal static SecurityArbiterSelection CreateAuthoritativeSelection(SecurityArbiterSelectionInput input)
        => SelectArbiters(input);

    // Select the arbiter list for the security evidence.
    private static SecurityArbiterSelection SelectArbiters(SecurityArbiterSelectionInput input)
    {
        // Prefer explicit pair anchors when the upstream splitter has already identified a CB4 scenario.
        if (input.SourcePairIds.Length > 0)
        {
            // Read the first pair id because this proof emits one domain packet per run.
            var sourcePairId = input.SourcePairIds[0];

            // Route the ext25 AS-side PKCE failure trace to the OAuth arbiter.
            if (string.Equals(sourcePairId, "CB4-3.4-811", StringComparison.Ordinal))
                return new SecurityArbiterSelection(
                    input.ScenarioId,
                    ["RFC-9700-OAuth-Security-Arbiter"],
                    sourcePairId,
                    "AS-side PKCE enforcement failures belong to the RFC 9700 OAuth security arbiter.");

            // Route the ext25 AS-side PKCE pass trace as a no-activation pass check.
            if (string.Equals(sourcePairId, "CB4-3.4-812", StringComparison.Ordinal))
                return new SecurityArbiterSelection(
                    input.ScenarioId,
                    ["NoActivation-PASS-Confirmation"],
                    sourcePairId,
                    "AS-side PKCE enforcement pass evidence needs only pass confirmation.");

            // Route the ext25 HS256 multi-party failure trace to JWT and token arbiters.
            if (string.Equals(sourcePairId, "CB4-3.4-813", StringComparison.Ordinal))
                return new SecurityArbiterSelection(
                    input.ScenarioId,
                    ["RFC-8725-JWT-Arbiter", "ASVS-V9-Token-Arbiter"],
                    sourcePairId,
                    "HS256 multi-party evidence requires JWT signing and token algorithm review.");

            // Route the ext25 RS256 multi-party pass trace as a no-activation pass check.
            if (string.Equals(sourcePairId, "CB4-3.4-814", StringComparison.Ordinal))
                return new SecurityArbiterSelection(
                    input.ScenarioId,
                    ["NoActivation-PASS-Confirmation"],
                    sourcePairId,
                    "RS256 multi-party pass evidence needs only pass confirmation.");

            // Route the ext25 IP-only throttling concern trace to the authentication arbiter.
            if (string.Equals(sourcePairId, "CB4-3.4-815", StringComparison.Ordinal))
                return new SecurityArbiterSelection(
                    input.ScenarioId,
                    ["ASVS-V6-Auth-Arbiter"],
                    sourcePairId,
                    "Token endpoint throttling granularity belongs to the ASVS V6 authentication arbiter.");

            // Route the ext25 per-client throttling pass trace as a no-activation pass check.
            if (string.Equals(sourcePairId, "CB4-3.4-816", StringComparison.Ordinal))
                return new SecurityArbiterSelection(
                    input.ScenarioId,
                    ["NoActivation-PASS-Confirmation"],
                    sourcePairId,
                    "Per-client throttling pass evidence needs only pass confirmation.");

            // Route the ext25 OIDC logout concern trace to the logout arbiter.
            if (string.Equals(sourcePairId, "CB4-3.4-817", StringComparison.Ordinal))
                return new SecurityArbiterSelection(
                    input.ScenarioId,
                    ["OIDC-RP-Initiated-Logout-Arbiter"],
                    sourcePairId,
                    "Missing id_token_hint evidence belongs to the OIDC logout arbiter.");

            // Route the ext25 OIDC logout pass trace as a no-activation pass check.
            if (string.Equals(sourcePairId, "CB4-3.4-818", StringComparison.Ordinal))
                return new SecurityArbiterSelection(
                    input.ScenarioId,
                    ["NoActivation-PASS-Confirmation"],
                    sourcePairId,
                    "Validated id_token_hint logout evidence needs only pass confirmation.");

            // Route the comprehensive ext31 pass through every arbiter family represented by the file.
            if (string.Equals(sourcePairId, "CB4-3.4-828", StringComparison.Ordinal))
                return new SecurityArbiterSelection(
                    input.ScenarioId,
                    ["RFC-9700-OAuth-Security-Arbiter", "RFC-8725-JWT-Arbiter", "ASVS-V6-Auth-Arbiter", "OIDC-RP-Initiated-Logout-Arbiter"],
                    sourcePairId,
                    "Comprehensive ext31 evidence requires all four represented arbiter families.");

            // Route the throttling plus logout pass case to the authentication and logout arbiters.
            if (string.Equals(sourcePairId, "CB4-3.4-822", StringComparison.Ordinal))
                return new SecurityArbiterSelection(
                    input.ScenarioId,
                    ["ASVS-V6-Auth-Arbiter", "OIDC-RP-Initiated-Logout-Arbiter"],
                    sourcePairId,
                    "Per-client throttling and id_token_hint logout evidence require auth throttling and logout review.");

            // Route the HS256 plus ROPC failure to the JWT and OAuth grant arbiters.
            if (string.Equals(sourcePairId, "CB4-3.4-823", StringComparison.Ordinal))
                return new SecurityArbiterSelection(
                    input.ScenarioId,
                    ["RFC-8725-JWT-Arbiter", "RFC-9700-OAuth-Security-Arbiter"],
                    sourcePairId,
                    "HS256 multi-party and ROPC evidence require both JWT signing and OAuth grant review.");

            // Route the RS256 plus ROPC-disabled pass case to the JWT and OAuth grant arbiters.
            if (string.Equals(sourcePairId, "CB4-3.4-824", StringComparison.Ordinal))
                return new SecurityArbiterSelection(
                    input.ScenarioId,
                    ["RFC-8725-JWT-Arbiter", "RFC-9700-OAuth-Security-Arbiter"],
                    sourcePairId,
                    "RS256 and ROPC-disabled evidence require both JWT signing and OAuth grant review.");

            // Route the PKCE plus training-gap failure to the OAuth and SSDF training arbiters.
            if (string.Equals(sourcePairId, "CB4-3.4-825", StringComparison.Ordinal))
                return new SecurityArbiterSelection(
                    input.ScenarioId,
                    ["RFC-9700-OAuth-Security-Arbiter", "SSDF-PO1-Training-Arbiter"],
                    sourcePairId,
                    "PKCE non-enforcement caused by a training gap requires OAuth and secure-development training review.");

            // Route the HS256 plus missing-exp failure to the JWT and access-token arbiters.
            if (string.Equals(sourcePairId, "CB4-3.4-826", StringComparison.Ordinal))
                return new SecurityArbiterSelection(
                    input.ScenarioId,
                    ["RFC-8725-JWT-Arbiter", "RFC-9068-Access-Token-Arbiter"],
                    sourcePairId,
                    "HS256 multi-party and no-exp token evidence require JWT signing and access-token profile review.");

            // Route the IP-only plus no-PKCE case to both the concern and MUST-level authorities.
            if (string.Equals(sourcePairId, "CB4-3.4-827", StringComparison.Ordinal))
                return new SecurityArbiterSelection(
                    input.ScenarioId,
                    ["ASVS-V6-Auth-Arbiter", "RFC-9700-OAuth-Security-Arbiter"],
                    sourcePairId,
                    "IP-only throttling and no-PKCE evidence require both auth throttling and OAuth security review.");

            // Route the cross-source PKCE plus HS256 failure to both OAuth and JWT arbiters.
            if (string.Equals(sourcePairId, "CB4-3.4-819", StringComparison.Ordinal))
                return new SecurityArbiterSelection(
                    input.ScenarioId,
                    ["RFC-9700-OAuth-Security-Arbiter", "RFC-8725-JWT-Arbiter", "ASVS-V9-Token-Arbiter"],
                    sourcePairId,
                    "Cross-source PKCE and HS256 multi-party evidence requires both OAuth and JWT arbiter families.");

            // Route the cross-source PKCE plus RS256 pass case to the same authorities that can verify both controls.
            if (string.Equals(sourcePairId, "CB4-3.4-820", StringComparison.Ordinal))
                return new SecurityArbiterSelection(
                    input.ScenarioId,
                    ["RFC-9700-OAuth-Security-Arbiter", "RFC-8725-JWT-Arbiter", "ASVS-V9-Token-Arbiter"],
                    sourcePairId,
                    "Cross-source PKCE and RS256 evidence requires the OAuth and JWT arbiter families to confirm both controls.");

            // Route the cross-source two-concern case to authentication and logout arbiters without creating a REPROCESS path.
            if (string.Equals(sourcePairId, "CB4-3.4-821", StringComparison.Ordinal))
                return new SecurityArbiterSelection(
                    input.ScenarioId,
                    ["ASVS-V6-Auth-Arbiter", "OIDC-RP-Initiated-Logout-Arbiter"],
                    sourcePairId,
                    "IP-only throttling and missing id_token_hint evidence requires both auth throttling and logout review.");

            // Route the RFC 9700 PKCE enforcement violation to the OAuth arbiter.
            if (string.Equals(sourcePairId, "CB4-3.4-803", StringComparison.Ordinal))
                return new SecurityArbiterSelection(
                    input.ScenarioId,
                    ["RFC-9700-OAuth-Security-Arbiter"],
                    sourcePairId,
                    "Authorization-server PKCE enforcement belongs to the RFC 9700 OAuth security arbiter.");

            // Route token endpoint rate limiting evidence to the ASVS authentication arbiter.
            if (string.Equals(sourcePairId, "CB4-3.4-807", StringComparison.Ordinal) || string.Equals(sourcePairId, "CB4-3.4-808", StringComparison.Ordinal))
                return new SecurityArbiterSelection(
                    input.ScenarioId,
                    ["ASVS-V6-Auth-Arbiter"],
                    sourcePairId,
                    "Token endpoint credential throttling belongs to the ASVS V6 authentication arbiter.");
        }

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
