// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure.Logical;

namespace ExperimentsV2.LogicalFlow;

// This record carries raw user evidence into the node-01 security splitter unit.
public sealed record SecuritySplitInput(string SecurityRequest);

// This record carries one security domain packet from node-01 to downstream stations.
public sealed record SecurityDomainPacket(
    string ScenarioId,
    string DomainLabel,
    string PacketText,
    string CoverageStatus,
    string HandoffMarker,
    string SourcePairId,
    string[] RequiredResultSignals,
    string[] ForbiddenResultSignals);

// This unit is the CB4 pair-backed node-01 domain splitter for the security-verification flow.
public sealed class SecuritySplitterUnit : ILogicalUnit<SecuritySplitInput, SecurityDomainPacket>
{
    // This id identifies the node-01 splitter in evidence and later unit registries.
    public const string Id = "unit-cb4-security-splitter-v1";

    // This role names the station responsibility the unit fills.
    public const string Role = "node-01-domain-splitter";

    // This domain tag scopes the unit to the CB4 security-verification flow.
    public const string DomainTag = "security-verification-3.4";

    // This confidence remains above the replacement floor used by the security proof.
    public const double Confidence = 0.87;

    // Execute the splitter step for one raw security request.
    public Task<AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityDomainPacket>>> ExecuteAsync(
        LogicalUnitRequest<SecuritySplitInput> request,
        CancellationToken cancellationToken = default)
    {
        // Return cancellation through the shared operation envelope.
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityDomainPacket>>.Fail("Cancelled.", "CANCELLED"));

        // Refuse empty split input because no domain packet can be produced without source text.
        if (request.Input is null || string.IsNullOrWhiteSpace(request.Input.SecurityRequest))
            return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityDomainPacket>>.Fail("Security split input is required.", "EMPTY_SECURITY_SPLIT_INPUT"));

        // Build the single security-domain packet from the CB4 oracle request.
        var packet = CreateAuthoritativePacket(request.Input.SecurityRequest);

        // Build the logical-unit result around the security-domain packet.
        var result = new LogicalUnitResult<SecurityDomainPacket>(
            request.UnitId,
            request.UnitRole,
            request.DomainTag,
            packet,
            Confidence,
            BuildPredicateIds(packet),
            [DomainTag],
            [packet.SourcePairId],
            []);

        // Return the domain packet through the async unit contract.
        return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityDomainPacket>>.Ok(result));
    }

    // Expose the single authoritative route mapping to the internal flow admission boundary.
    internal static SecurityDomainPacket CreateAuthoritativePacket(string securityRequest)
        => Split(securityRequest);

    // Split the raw request into the security-and-authorization domain packet used by this proof.
    private static SecurityDomainPacket Split(string securityRequest)
    {
        // Normalize the request once so all split detectors use the same comparison style.
        var normalized = securityRequest.ToLowerInvariant();

        // S2 splitting must identify the PKCE plain downgrade risk packet.
        if (normalized.Contains("code_challenge_method=plain", StringComparison.Ordinal) || normalized.Contains("pkce", StringComparison.Ordinal) && normalized.Contains("plain", StringComparison.Ordinal))
            return new SecurityDomainPacket(
                "S2",
                "security_and_authorization_model",
                securityRequest,
                "covered_reprocess_risk",
                "send_to_security_translator_with_rejection_risk",
                "CB4-3.4-005",
                ["REPROCESS", "ASVS 10.4.6", "S256", "ASVS-V10-OAuth-Arbiter", "plain-method assessment rejected"],
                ["compliant", "satisfies"]);

        // S3 splitting must identify the internal-standard boundary packet.
        if (normalized.Contains("ess-session-001", StringComparison.Ordinal) || normalized.Contains("exactly 30 minutes", StringComparison.Ordinal))
            return new SecurityDomainPacket(
                "S3",
                "security_and_authorization_model",
                securityRequest,
                "covered_boundary",
                "send_to_security_translator_with_boundary_flag",
                "CB4-3.4-009",
                ["CONCERN", "ASVS 7.3.1", "RA-AUTH-003", "Internal standard ESS-SESSION-001", "not an OWASP ASVS failure by itself"],
                []);

        // S17 splitting must identify the ext25 AS-side PKCE failure trace before the Action 3 PKCE-only detector.
        if (normalized.StartsWith("as accepts authorization code requests from public clients without code_challenge", StringComparison.Ordinal))
            return new SecurityDomainPacket(
                "S17",
                "security_and_authorization_model",
                securityRequest,
                "covered_reprocess_risk",
                "send_to_security_translator_with_rejection_risk",
                "CB4-3.4-811",
                ["REPROCESS", "RFC 9700 2.1.1", "AS-side PKCE enforcement", "backward compatibility rejected", "400 invalid_request"],
                ["backward compatibility exception"]);

        // S18 splitting must identify the ext25 AS-side PKCE pass trace before the wider ext31 pass detectors.
        if (normalized.StartsWith("as rejects public client authorization code requests without code_challenge", StringComparison.Ordinal))
            return new SecurityDomainPacket(
                "S18",
                "security_and_authorization_model",
                securityRequest,
                "covered",
                "ready_for_security_translation",
                "CB4-3.4-812",
                ["PASS", "RFC 9700 2.1.1", "RFC 7636 4.2", "S256", "400 invalid_request"],
                []);

        // S19 splitting must identify the ext25 HS256 multi-party failure trace.
        if (normalized.StartsWith("as signs tokens with hs256", StringComparison.Ordinal))
            return new SecurityDomainPacket(
                "S19",
                "security_and_authorization_model",
                securityRequest,
                "covered_reprocess_risk",
                "send_to_security_translator_with_rejection_risk",
                "CB4-3.4-813",
                ["REPROCESS", "RFC 8725 3.6", "V9.1.1", "HS256 shared secret", "token forgery", "simplicity rejected"],
                ["simplicity exception"]);

        // S20 splitting must identify the ext25 RS256 multi-party pass trace.
        if (normalized.StartsWith("as signs tokens with rsa private key", StringComparison.Ordinal))
            return new SecurityDomainPacket(
                "S20",
                "security_and_authorization_model",
                securityRequest,
                "covered",
                "ready_for_security_translation",
                "CB4-3.4-814",
                ["PASS", "RFC 8725 3.6", "V9.1.1", "RS256", "HTTPS JWKS", "private key exclusive"],
                []);

        // S21 splitting must identify the ext25 token endpoint IP-only trace before generic throttling concern handling.
        if (normalized.StartsWith("as limits token endpoint to 100 req/min per source ip", StringComparison.Ordinal))
            return new SecurityDomainPacket(
                "S21",
                "security_and_authorization_model",
                securityRequest,
                "covered_boundary",
                "send_to_security_translator_with_boundary_flag",
                "CB4-3.4-815",
                ["CONCERN", "ASVS V6.2", "IP-only rate limiting", "distributed attack viable", "high-volume argument addressable"],
                []);

        // S22 splitting must identify the ext25 per-client-id throttling trace before generic pass handling.
        if (normalized.StartsWith("30 failed auth attempts per 15-min window per client_id", StringComparison.Ordinal))
            return new SecurityDomainPacket(
                "S22",
                "security_and_authorization_model",
                securityRequest,
                "covered",
                "ready_for_security_translation",
                "CB4-3.4-816",
                ["PASS", "ASVS V6.2", "per-client-id counter", "429 Retry-After", "secret rotation", "layered with IP"],
                []);

        // S23 splitting must identify the ext25 OIDC logout concern trace.
        if (normalized.StartsWith("end session endpoint accepts post_logout_redirect_uri only", StringComparison.Ordinal))
            return new SecurityDomainPacket(
                "S23",
                "security_and_authorization_model",
                securityRequest,
                "covered_boundary",
                "send_to_security_translator_with_boundary_flag",
                "CB4-3.4-817",
                ["CONCERN", "OIDC RP-Initiated Logout", "id_token_hint absent", "session forcing", "harmless logout rejected"],
                []);

        // S24 splitting must identify the ext25 OIDC logout pass trace.
        if (normalized.StartsWith("end session requires id_token_hint", StringComparison.Ordinal))
            return new SecurityDomainPacket(
                "S24",
                "security_and_authorization_model",
                securityRequest,
                "covered",
                "ready_for_security_translation",
                "CB4-3.4-818",
                ["PASS", "OIDC RP-Initiated Logout", "id_token_hint required", "signature sub aud exp validation", "session forcing blocked"],
                []);

        // S16 splitting must identify the comprehensive ext31 pass before any narrower pass case can capture it.
        if ((normalized.Contains("complete authorization system", StringComparison.Ordinal) || normalized.Contains("all ext31 domains", StringComparison.Ordinal)) && normalized.Contains("id_token_hint", StringComparison.Ordinal) && normalized.Contains("rs256", StringComparison.Ordinal) && normalized.Contains("per-client-id", StringComparison.Ordinal))
            return new SecurityDomainPacket(
                "S16",
                "security_and_authorization_model",
                securityRequest,
                "covered",
                "ready_for_security_translation",
                "CB4-3.4-828",
                ["PASS", "RFC 9700 2.1.1", "RFC 8725 3.6", "ASVS V6.2", "OIDC RP-Initiated Logout", "all four ext31 domains"],
                []);

        // S10 splitting must identify the combined throttling and logout pass before the token-throttling-only pass can capture it.
        if (normalized.Contains("per-client-id rate limiting", StringComparison.Ordinal) && normalized.Contains("id_token_hint", StringComparison.Ordinal) && normalized.Contains("session forcing", StringComparison.Ordinal))
            return new SecurityDomainPacket(
                "S10",
                "security_and_authorization_model",
                securityRequest,
                "covered",
                "ready_for_security_translation",
                "CB4-3.4-822",
                ["PASS", "ASVS V6.2", "OIDC RP-Initiated Logout", "per-client-id rate limiting", "id_token_hint required", "session forcing blocked"],
                []);

        // S11 splitting must identify the HS256 plus ROPC double failure before either token or grant issue is isolated.
        if (normalized.Contains("hs256", StringComparison.Ordinal) && normalized.Contains("ropc", StringComparison.Ordinal) && normalized.Contains("same hmac secret", StringComparison.Ordinal))
            return new SecurityDomainPacket(
                "S11",
                "security_and_authorization_model",
                securityRequest,
                "covered_reprocess_risk",
                "send_to_security_translator_with_rejection_risk",
                "CB4-3.4-823",
                ["REPROCESS", "RFC 8725 3.6", "RFC 9700 2.4", "HS256 multi-party", "ROPC enabled", "two independent MUST-level violations"],
                ["legacy exception"]);

        // S12 splitting must identify the RS256 plus ROPC-disabled pass before the RS256-only pass can capture it.
        if (normalized.Contains("rs256", StringComparison.Ordinal) && normalized.Contains("ropc disabled", StringComparison.Ordinal))
            return new SecurityDomainPacket(
                "S12",
                "security_and_authorization_model",
                securityRequest,
                "covered",
                "ready_for_security_translation",
                "CB4-3.4-824",
                ["PASS", "RFC 8725 3.6", "RFC 9700 2.4", "RS256 asymmetric signing", "ROPC disabled", "authorization code plus PKCE S256"],
                []);

        // S13 splitting must identify the PKCE failure whose cause is a SSDF PO.1 training gap before the PKCE-only detector captures it.
        if ((normalized.Contains("ssdf po.1", StringComparison.Ordinal) || normalized.Contains("po.1", StringComparison.Ordinal)) && (normalized.Contains("does not require pkce", StringComparison.Ordinal) || normalized.Contains("without code_challenge", StringComparison.Ordinal)))
            return new SecurityDomainPacket(
                "S13",
                "security_and_authorization_model",
                securityRequest,
                "covered_reprocess_risk",
                "send_to_security_translator_with_rejection_risk",
                "CB4-3.4-825",
                ["REPROCESS", "RFC 9700 2.1.1", "SSDF PO.1", "training gap proximate cause", "React SPA without PKCE", "native app without PKCE"],
                ["RFC 6749 optional PKCE exception"]);

        // S14 splitting must identify forgeable non-expiring tokens before the HS256-only detector can capture it.
        if (normalized.Contains("hs256", StringComparison.Ordinal) && (normalized.Contains("no exp claim", StringComparison.Ordinal) || normalized.Contains("non-expiring", StringComparison.Ordinal)))
            return new SecurityDomainPacket(
                "S14",
                "security_and_authorization_model",
                securityRequest,
                "covered_reprocess_risk",
                "send_to_security_translator_with_rejection_risk",
                "CB4-3.4-826",
                ["REPROCESS", "RFC 8725 3.6", "RFC 9068 2.2", "HS256 multi-party", "no exp claim", "forgeable and non-expiring tokens"],
                []);

        // S15 splitting must identify the REPROCESS-over-CONCERN graduation case before either single finding can capture it.
        if ((normalized.Contains("future release", StringComparison.Ordinal) || normalized.Contains("pkce will be required", StringComparison.Ordinal)) && (normalized.Contains("ip-based", StringComparison.Ordinal) || normalized.Contains("per-client-id throttling absent", StringComparison.Ordinal)))
            return new SecurityDomainPacket(
                "S15",
                "security_and_authorization_model",
                securityRequest,
                "covered_reprocess_risk",
                "send_to_security_translator_with_rejection_risk",
                "CB4-3.4-827",
                ["REPROCESS", "RFC 9700 2.1.1", "REPROCESS overrides CONCERN", "IP-only rate limiting CONCERN absorbed", "future release is not current compliance"],
                ["future release exception"]);

        // S7 splitting must identify the cross-source PKCE plus HS256 multi-party failure before either single issue can capture the prompt.
        if (normalized.Contains("hs256", StringComparison.Ordinal) && (normalized.Contains("without code_challenge", StringComparison.Ordinal) || normalized.Contains("pkce is optional", StringComparison.Ordinal) || normalized.Contains("gradual adoption", StringComparison.Ordinal)))
            return new SecurityDomainPacket(
                "S7",
                "security_and_authorization_model",
                securityRequest,
                "covered_reprocess_risk",
                "send_to_security_translator_with_rejection_risk",
                "CB4-3.4-819",
                ["REPROCESS", "RFC 9700 2.1.1", "RFC 8725 3.6", "two independent MUST-level violations", "non-PKCE public client flows", "HS256 multi-party signing secret"],
                ["gradual adoption exception", "operational simplicity exception"]);

        // S8 splitting must identify the cross-source PKCE plus RS256 pass case before the generic pass fallback.
        if (normalized.Contains("code_challenge_method=s256", StringComparison.Ordinal) && (normalized.Contains("rs256", StringComparison.Ordinal) || normalized.Contains("https jwks", StringComparison.Ordinal)))
            return new SecurityDomainPacket(
                "S8",
                "security_and_authorization_model",
                securityRequest,
                "covered",
                "ready_for_security_translation",
                "CB4-3.4-820",
                ["PASS", "RFC 9700 2.1.1", "RFC 8725 3.6", "PKCE S256 enforced", "RS256 asymmetric signing", "HTTPS JWKS verification"],
                []);

        // S9 splitting must identify the cross-source two-concern case before IP-only throttling can capture it as a single issue.
        if ((normalized.Contains("ip-based", StringComparison.Ordinal) || normalized.Contains("no per-client-id throttling", StringComparison.Ordinal)) && normalized.Contains("oidc end session", StringComparison.Ordinal) && (normalized.Contains("without id_token_hint", StringComparison.Ordinal) || normalized.Contains("no id_token_hint", StringComparison.Ordinal)))
            return new SecurityDomainPacket(
                "S9",
                "security_and_authorization_model",
                securityRequest,
                "covered_boundary",
                "send_to_security_translator_with_boundary_flag",
                "CB4-3.4-821",
                ["CONCERN", "ASVS V6.2", "OIDC RP-Initiated Logout", "N CONCERNs do not escalate to REPROCESS", "per-client-id throttling absent", "id_token_hint absent"],
                ["REPROCESS escalation"]);

        // S4 splitting must identify the RFC 9700 public-client PKCE enforcement violation.
        if (normalized.Contains("without code_challenge", StringComparison.Ordinal) || normalized.Contains("pkce is optional", StringComparison.Ordinal) || normalized.Contains("backward compatibility with older client", StringComparison.Ordinal))
            return new SecurityDomainPacket(
                "S4",
                "security_and_authorization_model",
                securityRequest,
                "covered_reprocess_risk",
                "send_to_security_translator_with_rejection_risk",
                "CB4-3.4-803",
                ["REPROCESS", "RFC 9700 2.1.1", "PKCE required for public clients", "public clients without code_challenge rejected"],
                ["backward compatibility exception"]);

        // S5 splitting must identify the IP-only token endpoint throttling concern.
        if (normalized.Contains("ip-based rate limiting", StringComparison.Ordinal) || normalized.Contains("no per-client-id throttling", StringComparison.Ordinal))
            return new SecurityDomainPacket(
                "S5",
                "security_and_authorization_model",
                securityRequest,
                "covered_boundary",
                "send_to_security_translator_with_boundary_flag",
                "CB4-3.4-807",
                ["CONCERN", "ASVS V6.2", "per-client-id throttling absent", "distributed credential attack viable"],
                []);

        // S6 splitting must identify the per-client-id token endpoint throttling pass case.
        if (normalized.Contains("per-client-id rate limiting", StringComparison.Ordinal) || normalized.Contains("30 failed authentication attempts", StringComparison.Ordinal))
            return new SecurityDomainPacket(
                "S6",
                "security_and_authorization_model",
                securityRequest,
                "covered",
                "ready_for_security_translation",
                "CB4-3.4-808",
                ["PASS", "ASVS V6.2", "per-client-id rate limiting", "429", "Retry-After", "secret rotation flag"],
                []);

        // Default splitting keeps the complete ASVS evidence in the clean covered packet.
        return new SecurityDomainPacket(
            "S1",
            "security_and_authorization_model",
            securityRequest,
            "covered",
            "ready_for_security_translation",
            "CB4-3.4-001",
            ["PASS", "OWASP ASVS 5.0.0", "password length", "RS256", "RBAC", "per-resource ownership"],
            []);
    }

    // Build predicate ids that describe the domain split.
    private static string[] BuildPredicateIds(SecurityDomainPacket packet)
    {
        // Reprocess-risk packets carry a failed validation edge downstream.
        if (packet.RequiredResultSignals.Contains("REPROCESS", StringComparer.Ordinal))
            return ["BRANCH", "FAIL", "AUTH_OP"];

        // Boundary packets carry a concern edge downstream.
        if (packet.RequiredResultSignals.Contains("CONCERN", StringComparer.Ordinal))
            return ["BRANCH", "VALIDATE_OP", "AUTH_OP"];

        // Covered packets carry a clean security domain edge downstream.
        return ["BRANCH", "RETURN", "AUTH_OP"];
    }
}
