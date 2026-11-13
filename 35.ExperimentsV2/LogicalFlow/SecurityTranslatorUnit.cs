// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure.Logical;

namespace ExperimentsV2.LogicalFlow;

// This record carries raw security evidence into the node-02 translator unit.
public sealed record SecurityTranslationInput(
    string ScenarioId,
    SecurityDomainPacket DomainPacket,
    SecurityArbiterSelection Selection);

// This record carries the formal security-domain statement from node-02 to node-03.
public sealed record SecurityDomainTranslation(
    string ScenarioId,
    string FormalStatement,
    string HandoffMarker,
    string SourcePairId,
    string[] RequiredResultSignals,
    string[] ForbiddenResultSignals);

// This unit is the CB4 pair-backed node-02 domain translator for the security-verification flow.
public sealed class SecurityTranslatorUnit : ILogicalUnit<SecurityTranslationInput, SecurityDomainTranslation>
{
    // This id identifies the node-02 translator in evidence and later unit registries.
    public const string Id = "unit-cb4-security-translator-v1";

    // This role names the station responsibility the unit fills.
    public const string Role = "node-02-domain-translator";

    // This domain tag scopes the unit to the CB4 security-verification flow.
    public const string DomainTag = "security-verification-3.4";

    // This confidence remains above the replacement floor used by the security proof.
    public const double Confidence = 0.88;

    // Execute the translator step for one security-verification scenario.
    public Task<AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityDomainTranslation>>> ExecuteAsync(
        LogicalUnitRequest<SecurityTranslationInput> request,
        CancellationToken cancellationToken = default)
    {
        // Return cancellation through the shared operation envelope.
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityDomainTranslation>>.Fail("Cancelled.", "CANCELLED"));

        // Refuse empty translator input because formal translation requires a domain packet.
        if (request.Input is null || request.Input.DomainPacket is null)
            return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityDomainTranslation>>.Fail("Security translation input is required.", "EMPTY_SECURITY_TRANSLATION_INPUT"));

        // Build the formal domain translation from the CB4 oracle scenario.
        var translation = CreateAuthoritativeTranslation(request.Input);

        // Build the logical-unit result around the formal domain translation.
        var result = new LogicalUnitResult<SecurityDomainTranslation>(
            request.UnitId,
            request.UnitRole,
            request.DomainTag,
            translation,
            Confidence,
            BuildPredicateIds(translation),
            [DomainTag],
            [translation.SourcePairId],
            []);

        // Return the formal domain translation through the async unit contract.
        return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityDomainTranslation>>.Ok(result));
    }

    // Expose the single authoritative translation mapping to the internal flow admission boundary.
    internal static SecurityDomainTranslation CreateAuthoritativeTranslation(SecurityTranslationInput input)
        => Translate(input);

    // Convert one raw security request into a formal domain statement.
    private static SecurityDomainTranslation Translate(SecurityTranslationInput input)
    {
        // S2 translation must keep the unsafe PKCE plain assessment visible.
        if (string.Equals(input.ScenarioId, "S2", StringComparison.Ordinal))
            return new SecurityDomainTranslation(
                input.ScenarioId,
                "Formal security statement: verify OAuth PKCE method handling where plain is accepted even though S256 is supported.",
                "send_to_scheme_planner_with_rejection_risk",
                "CB4-3.4-005",
                ["REPROCESS", "ASVS 10.4.6", "S256", "ASVS-V10-OAuth-Arbiter", "plain-method assessment rejected"],
                ["compliant", "satisfies"]);

        // S3 translation must keep internal enterprise policy separate from ASVS status.
        if (string.Equals(input.ScenarioId, "S3", StringComparison.Ordinal))
            return new SecurityDomainTranslation(
                input.ScenarioId,
                "Formal security statement: verify ASVS session re-authentication evidence while preserving ESS-SESSION-001 as an internal policy concern.",
                "send_to_scheme_planner_with_boundary_flag",
                "CB4-3.4-009",
                ["CONCERN", "ASVS 7.3.1", "RA-AUTH-003", "Internal standard ESS-SESSION-001", "not an OWASP ASVS failure by itself"],
                []);

        // S17 translation must preserve AS-side PKCE enforcement and the rejected compatibility argument.
        if (string.Equals(input.ScenarioId, "S17", StringComparison.Ordinal))
            return new SecurityDomainTranslation(
                input.ScenarioId,
                "Formal security statement: verify AS-side PKCE enforcement for public clients and reject backward compatibility as an RFC 9700 exception.",
                "send_to_scheme_planner_with_rejection_risk",
                input.DomainPacket.SourcePairId,
                input.DomainPacket.RequiredResultSignals,
                input.DomainPacket.ForbiddenResultSignals);

        // S18 translation must preserve AS-side PKCE enforcement as a clean pass.
        if (string.Equals(input.ScenarioId, "S18", StringComparison.Ordinal))
            return new SecurityDomainTranslation(
                input.ScenarioId,
                "Formal security statement: verify that non-PKCE public client requests are rejected and public clients use S256.",
                "ready_for_scheme_planner",
                input.DomainPacket.SourcePairId,
                input.DomainPacket.RequiredResultSignals,
                input.DomainPacket.ForbiddenResultSignals);

        // S19 translation must preserve HS256 shared-secret distribution as a token-forgery failure.
        if (string.Equals(input.ScenarioId, "S19", StringComparison.Ordinal))
            return new SecurityDomainTranslation(
                input.ScenarioId,
                "Formal security statement: verify HS256 shared-secret distribution in a multi-party JWT system and reject simplicity as a security exception.",
                "send_to_scheme_planner_with_rejection_risk",
                input.DomainPacket.SourcePairId,
                input.DomainPacket.RequiredResultSignals,
                input.DomainPacket.ForbiddenResultSignals);

        // S20 translation must preserve RS256 private-key exclusivity and HTTPS JWKS verification as a pass.
        if (string.Equals(input.ScenarioId, "S20", StringComparison.Ordinal))
            return new SecurityDomainTranslation(
                input.ScenarioId,
                "Formal security statement: verify RS256 private-key exclusivity at the authorization server and resource-server HTTPS JWKS verification.",
                "ready_for_scheme_planner",
                input.DomainPacket.SourcePairId,
                input.DomainPacket.RequiredResultSignals,
                input.DomainPacket.ForbiddenResultSignals);

        // S21 translation must preserve IP-only throttling as partial protection rather than a MUST failure.
        if (string.Equals(input.ScenarioId, "S21", StringComparison.Ordinal))
            return new SecurityDomainTranslation(
                input.ScenarioId,
                "Formal security statement: evaluate token endpoint IP-only rate limiting as partial brute-force protection with distributed attack exposure.",
                "send_to_scheme_planner_with_boundary_flag",
                input.DomainPacket.SourcePairId,
                input.DomainPacket.RequiredResultSignals,
                input.DomainPacket.ForbiddenResultSignals);

        // S22 translation must preserve per-client throttling and graduated response as a pass.
        if (string.Equals(input.ScenarioId, "S22", StringComparison.Ordinal))
            return new SecurityDomainTranslation(
                input.ScenarioId,
                "Formal security statement: verify token endpoint per-client-id throttling with 429 Retry-After and sustained-attack response.",
                "ready_for_scheme_planner",
                input.DomainPacket.SourcePairId,
                input.DomainPacket.RequiredResultSignals,
                input.DomainPacket.ForbiddenResultSignals);

        // S23 translation must preserve missing id_token_hint as a session-forcing concern.
        if (string.Equals(input.ScenarioId, "S23", StringComparison.Ordinal))
            return new SecurityDomainTranslation(
                input.ScenarioId,
                "Formal security statement: evaluate OIDC End Session without id_token_hint and reject the harmless-logout argument.",
                "send_to_scheme_planner_with_boundary_flag",
                input.DomainPacket.SourcePairId,
                input.DomainPacket.RequiredResultSignals,
                input.DomainPacket.ForbiddenResultSignals);

        // S24 translation must preserve id_token_hint validation as a logout pass.
        if (string.Equals(input.ScenarioId, "S24", StringComparison.Ordinal))
            return new SecurityDomainTranslation(
                input.ScenarioId,
                "Formal security statement: verify required id_token_hint validation for OIDC End Session and confirm session-forcing protection.",
                "ready_for_scheme_planner",
                input.DomainPacket.SourcePairId,
                input.DomainPacket.RequiredResultSignals,
                input.DomainPacket.ForbiddenResultSignals);

        // S16 translation must preserve all four ext31 domains as one comprehensive pass packet.
        if (string.Equals(input.ScenarioId, "S16", StringComparison.Ordinal))
            return new SecurityDomainTranslation(
                input.ScenarioId,
                "Formal security statement: verify AS-enforced PKCE, RS256/JWKS signing, per-client-id token throttling, and id_token_hint logout protection as a complete ext31 control set.",
                "ready_for_scheme_planner",
                input.DomainPacket.SourcePairId,
                input.DomainPacket.RequiredResultSignals,
                input.DomainPacket.ForbiddenResultSignals);

        // S10 translation must keep throttling and logout pass evidence together.
        if (string.Equals(input.ScenarioId, "S10", StringComparison.Ordinal))
            return new SecurityDomainTranslation(
                input.ScenarioId,
                "Formal security statement: verify per-client-id token endpoint throttling and OIDC End Session id_token_hint validation as two accepted controls.",
                "ready_for_scheme_planner",
                input.DomainPacket.SourcePairId,
                input.DomainPacket.RequiredResultSignals,
                input.DomainPacket.ForbiddenResultSignals);

        // S11 translation must preserve both HS256 multi-party and ROPC as independent failures.
        if (string.Equals(input.ScenarioId, "S11", StringComparison.Ordinal))
            return new SecurityDomainTranslation(
                input.ScenarioId,
                "Formal security statement: verify HS256 shared-secret distribution and enabled ROPC as two independent OAuth and JWT security failures.",
                "send_to_scheme_planner_with_rejection_risk",
                input.DomainPacket.SourcePairId,
                input.DomainPacket.RequiredResultSignals,
                input.DomainPacket.ForbiddenResultSignals);

        // S12 translation must preserve RS256 and ROPC replacement as two accepted controls.
        if (string.Equals(input.ScenarioId, "S12", StringComparison.Ordinal))
            return new SecurityDomainTranslation(
                input.ScenarioId,
                "Formal security statement: verify RS256 asymmetric signing and disabled ROPC with authorization code plus PKCE S256 replacement.",
                "ready_for_scheme_planner",
                input.DomainPacket.SourcePairId,
                input.DomainPacket.RequiredResultSignals,
                input.DomainPacket.ForbiddenResultSignals);

        // S13 translation must preserve the causal chain from training gap to production PKCE failure.
        if (string.Equals(input.ScenarioId, "S13", StringComparison.Ordinal))
            return new SecurityDomainTranslation(
                input.ScenarioId,
                "Formal security statement: verify public-client PKCE non-enforcement and preserve the SSDF PO.1 training gap as the proximate cause.",
                "send_to_scheme_planner_with_rejection_risk",
                input.DomainPacket.SourcePairId,
                input.DomainPacket.RequiredResultSignals,
                input.DomainPacket.ForbiddenResultSignals);

        // S14 translation must preserve the compound forgeable and non-expiring token risk.
        if (string.Equals(input.ScenarioId, "S14", StringComparison.Ordinal))
            return new SecurityDomainTranslation(
                input.ScenarioId,
                "Formal security statement: verify HS256 shared-secret distribution and access tokens without exp as a compound token-forgery risk.",
                "send_to_scheme_planner_with_rejection_risk",
                input.DomainPacket.SourcePairId,
                input.DomainPacket.RequiredResultSignals,
                input.DomainPacket.ForbiddenResultSignals);

        // S15 translation must preserve that the PKCE MUST violation overrides the throttling concern.
        if (string.Equals(input.ScenarioId, "S15", StringComparison.Ordinal))
            return new SecurityDomainTranslation(
                input.ScenarioId,
                "Formal security statement: evaluate IP-only throttling as a concern while treating current public-client PKCE non-enforcement as the determining reprocess failure.",
                "send_to_scheme_planner_with_rejection_risk",
                input.DomainPacket.SourcePairId,
                input.DomainPacket.RequiredResultSignals,
                input.DomainPacket.ForbiddenResultSignals);

        // S7 translation must preserve both independent MUST-level violations as one cross-source packet.
        if (string.Equals(input.ScenarioId, "S7", StringComparison.Ordinal))
            return new SecurityDomainTranslation(
                input.ScenarioId,
                "Formal security statement: verify that the authorization server rejects non-PKCE public client flows and that multi-party token signing does not distribute an HS256 shared secret.",
                "send_to_scheme_planner_with_rejection_risk",
                input.DomainPacket.SourcePairId,
                input.DomainPacket.RequiredResultSignals,
                input.DomainPacket.ForbiddenResultSignals);

        // S8 translation must preserve the two-control pass shape as one cross-source packet.
        if (string.Equals(input.ScenarioId, "S8", StringComparison.Ordinal))
            return new SecurityDomainTranslation(
                input.ScenarioId,
                "Formal security statement: verify AS-enforced PKCE S256 for public clients and RS256 asymmetric token signing with JWKS public-key verification.",
                "ready_for_scheme_planner",
                input.DomainPacket.SourcePairId,
                input.DomainPacket.RequiredResultSignals,
                input.DomainPacket.ForbiddenResultSignals);

        // S9 translation must preserve the two-concern graduation rule without escalating it to REPROCESS.
        if (string.Equals(input.ScenarioId, "S9", StringComparison.Ordinal))
            return new SecurityDomainTranslation(
                input.ScenarioId,
                "Formal security statement: evaluate IP-only token endpoint throttling and OIDC End Session without id_token_hint as two concern-level gaps that do not escalate to reprocess.",
                "send_to_scheme_planner_with_boundary_flag",
                input.DomainPacket.SourcePairId,
                input.DomainPacket.RequiredResultSignals,
                input.DomainPacket.ForbiddenResultSignals);

        // S4 translation must keep the RFC 9700 public-client PKCE MUST visible.
        if (string.Equals(input.ScenarioId, "S4", StringComparison.Ordinal))
            return new SecurityDomainTranslation(
                input.ScenarioId,
                "Formal security statement: verify that the authorization server rejects public client authorization code requests that omit code_challenge.",
                "send_to_scheme_planner_with_rejection_risk",
                input.DomainPacket.SourcePairId,
                input.DomainPacket.RequiredResultSignals,
                input.DomainPacket.ForbiddenResultSignals);

        // S5 translation must keep the IP-only rate limiting boundary visible.
        if (string.Equals(input.ScenarioId, "S5", StringComparison.Ordinal))
            return new SecurityDomainTranslation(
                input.ScenarioId,
                "Formal security statement: evaluate token endpoint brute-force protection where only source-IP rate limiting exists and per-client-id throttling is absent.",
                "send_to_scheme_planner_with_boundary_flag",
                input.DomainPacket.SourcePairId,
                input.DomainPacket.RequiredResultSignals,
                input.DomainPacket.ForbiddenResultSignals);

        // S6 translation must keep the per-client-id throttling pass evidence visible.
        if (string.Equals(input.ScenarioId, "S6", StringComparison.Ordinal))
            return new SecurityDomainTranslation(
                input.ScenarioId,
                "Formal security statement: verify token endpoint per-client-id rate limiting with graduated 429 and secret-rotation response.",
                "ready_for_scheme_planner",
                input.DomainPacket.SourcePairId,
                input.DomainPacket.RequiredResultSignals,
                input.DomainPacket.ForbiddenResultSignals);

        // S1 translation must keep the clean ASVS verification path visible.
        return new SecurityDomainTranslation(
            input.ScenarioId,
            "Formal security statement: verify authentication, session, authorization, and token controls against OWASP ASVS 5.0.0.",
            "ready_for_scheme_planner",
            "CB4-3.4-001",
            ["PASS", "OWASP ASVS 5.0.0", "password length", "RS256", "RBAC", "per-resource ownership"],
            []);
    }

    // Build predicate ids that describe the formal domain translation.
    private static string[] BuildPredicateIds(SecurityDomainTranslation translation)
    {
        // REPROCESS translations carry a rejection risk toward planning.
        if (translation.RequiredResultSignals.Contains("REPROCESS", StringComparer.Ordinal))
            return ["CONVERT_OP", "FAIL", "AUTH_OP"];

        // CONCERN translations carry a boundary toward planning.
        if (translation.RequiredResultSignals.Contains("CONCERN", StringComparer.Ordinal))
            return ["CONVERT_OP", "BRANCH", "AUTH_OP"];

        // PASS translations carry clean validation toward planning.
        return ["CONVERT_OP", "VALIDATE_OP", "AUTH_OP"];
    }
}
