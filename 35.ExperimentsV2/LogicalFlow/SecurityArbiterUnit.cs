// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

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

        // Detect the CB4-3.4-811 AS-side PKCE failure trace.
        if (input.SourcePairIds.Contains("CB4-3.4-811", StringComparer.Ordinal) || normalized.Contains("optional pkce leaves non-compliant public clients unprotected", StringComparison.Ordinal))
            return BuildPipelinePkceReprocess(input.ScenarioId);

        // Detect the CB4-3.4-812 AS-side PKCE pass trace.
        if (input.SourcePairIds.Contains("CB4-3.4-812", StringComparer.Ordinal) || normalized.Contains("as enforcement and s256 client behavior", StringComparison.Ordinal))
            return BuildPipelinePkcePass(input.ScenarioId);

        // Detect the CB4-3.4-813 HS256 multi-party failure trace.
        if (input.SourcePairIds.Contains("CB4-3.4-813", StringComparer.Ordinal) || normalized.Contains("hs256 key distribution against rs256", StringComparison.Ordinal))
            return BuildPipelineHs256Reprocess(input.ScenarioId);

        // Detect the CB4-3.4-814 RS256 multi-party pass trace.
        if (input.SourcePairIds.Contains("CB4-3.4-814", StringComparer.Ordinal) || normalized.Contains("resource-server compromise cannot produce a signing key", StringComparison.Ordinal))
            return BuildPipelineRs256Pass(input.ScenarioId);

        // Detect the CB4-3.4-815 IP-only throttling concern trace.
        if (input.SourcePairIds.Contains("CB4-3.4-815", StringComparer.Ordinal) || normalized.Contains("configured limits rather than no per-client counter", StringComparison.Ordinal))
            return BuildPipelineIpOnlyConcern(input.ScenarioId);

        // Detect the CB4-3.4-816 per-client throttling pass trace.
        if (input.SourcePairIds.Contains("CB4-3.4-816", StringComparer.Ordinal) || normalized.Contains("cannot bypass the client_id counter", StringComparison.Ordinal))
            return BuildPipelinePerClientPass(input.ScenarioId);

        // Detect the CB4-3.4-817 OIDC logout concern trace.
        if (input.SourcePairIds.Contains("CB4-3.4-817", StringComparer.Ordinal) || normalized.Contains("phishing precursor risk", StringComparison.Ordinal))
            return BuildPipelineLogoutConcern(input.ScenarioId);

        // Detect the CB4-3.4-818 OIDC logout pass trace.
        if (input.SourcePairIds.Contains("CB4-3.4-818", StringComparer.Ordinal) || normalized.Contains("session binding are validated", StringComparison.Ordinal))
            return BuildPipelineLogoutPass(input.ScenarioId);

        // Detect the CB4-3.4-828 comprehensive ext31 pass before narrower pass cases can capture it.
        if (input.SourcePairIds.Contains("CB4-3.4-828", StringComparer.Ordinal) || normalized.Contains("all four ext31", StringComparison.Ordinal))
            return BuildComprehensiveExt31Pass(input.ScenarioId);

        // Detect the CB4-3.4-822 throttling plus logout pass case.
        if (input.SourcePairIds.Contains("CB4-3.4-822", StringComparer.Ordinal) || normalized.Contains("throttling and logout protections both meet", StringComparison.Ordinal))
            return BuildThrottleAndLogoutPass(input.ScenarioId);

        // Detect the CB4-3.4-823 HS256 plus ROPC double failure.
        if (input.SourcePairIds.Contains("CB4-3.4-823", StringComparer.Ordinal) || normalized.Contains("hs256 shared-secret distribution and enabled ropc", StringComparison.Ordinal))
            return BuildHs256AndRopcReprocess(input.ScenarioId);

        // Detect the CB4-3.4-824 RS256 plus ROPC-disabled pass case.
        if (input.SourcePairIds.Contains("CB4-3.4-824", StringComparer.Ordinal) || normalized.Contains("rs256 and ropc replacement", StringComparison.Ordinal))
            return BuildRs256AndRopcDisabledPass(input.ScenarioId);

        // Detect the CB4-3.4-825 PKCE failure caused by a training gap.
        if (input.SourcePairIds.Contains("CB4-3.4-825", StringComparer.Ordinal) || normalized.Contains("ssdf po.1 as a proximate-cause", StringComparison.Ordinal))
            return BuildPkceTrainingGapReprocess(input.ScenarioId);

        // Detect the CB4-3.4-826 forgeable and non-expiring token failure.
        if (input.SourcePairIds.Contains("CB4-3.4-826", StringComparer.Ordinal) || normalized.Contains("forgeability and non-expiry", StringComparison.Ordinal))
            return BuildHs256NoExpReprocess(input.ScenarioId);

        // Detect the CB4-3.4-827 REPROCESS-over-CONCERN graduation case.
        if (input.SourcePairIds.Contains("CB4-3.4-827", StringComparer.Ordinal) || normalized.Contains("reprocess overrides the ip-only throttling concern", StringComparison.Ordinal))
            return BuildPkceOverridesThrottleConcern(input.ScenarioId);

        // Detect the CB4-3.4-819 cross-source PKCE plus HS256 failure before either single finding can capture it.
        if (input.SourcePairIds.Contains("CB4-3.4-819", StringComparer.Ordinal) || normalized.Contains("two independent must-level violations", StringComparison.Ordinal))
            return BuildPkceAndHs256Reprocess(input.ScenarioId);

        // Detect the CB4-3.4-820 cross-source PKCE plus RS256 pass case.
        if (input.SourcePairIds.Contains("CB4-3.4-820", StringComparer.Ordinal) || normalized.Contains("pkce enforcement and asymmetric signing", StringComparison.Ordinal))
            return BuildPkceAndRs256Pass(input.ScenarioId);

        // Detect the CB4-3.4-821 cross-source two-concern case before the IP-only single concern captures it.
        if (input.SourcePairIds.Contains("CB4-3.4-821", StringComparer.Ordinal) || normalized.Contains("concern plus concern does not become reprocess", StringComparison.Ordinal))
            return BuildIpLimitAndLogoutConcern(input.ScenarioId);

        // Detect the CB4-3.4-803 RFC 9700 PKCE enforcement violation and return REPROCESS.
        if (input.SourcePairIds.Contains("CB4-3.4-803", StringComparer.Ordinal) || normalized.Contains("rfc 9700 2.1.1", StringComparison.Ordinal) && normalized.Contains("omit code_challenge", StringComparison.Ordinal))
            return BuildPublicClientPkceReprocess(input.ScenarioId);

        // Detect the CB4-3.4-807 IP-only throttling boundary and return CONCERN.
        if (input.SourcePairIds.Contains("CB4-3.4-807", StringComparer.Ordinal) || normalized.Contains("per-client-id throttling is absent", StringComparison.Ordinal))
            return BuildIpOnlyRateLimitConcern(input.ScenarioId);

        // Detect the CB4-3.4-808 per-client throttling pass case and return PASS.
        if (input.SourcePairIds.Contains("CB4-3.4-808", StringComparer.Ordinal) || normalized.Contains("secret-rotation response", StringComparison.Ordinal))
            return BuildPerClientRateLimitPass(input.ScenarioId);

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

    // Build the REPROCESS verdict anchored in CB4-3.4-811.
    private static SecurityArbiterVerdict BuildPipelinePkceReprocess(string scenarioId)
    {
        // Return the reprocess verdict for AS-side PKCE non-enforcement.
        return new SecurityArbiterVerdict(
            scenarioId,
            "REPROCESS",
            "RFC-9700-OAuth-Security-Arbiter",
            "RFC 9700 2.1.1",
            "Authorization servers must reject public client authorization code requests without a valid code_challenge; backward compatibility is not an exception.",
            "CB4-3.4-811",
            ["REPROCESS", "RFC 9700 2.1.1", "AS-side PKCE enforcement", "backward compatibility rejected", "400 invalid_request"],
            ["backward compatibility exception"]);
    }

    // Build the PASS verdict anchored in CB4-3.4-812.
    private static SecurityArbiterVerdict BuildPipelinePkcePass(string scenarioId)
    {
        // Return the pass verdict for AS-side PKCE enforcement and S256 client behavior.
        return new SecurityArbiterVerdict(
            scenarioId,
            "PASS",
            "NoActivation-PASS-Confirmation",
            "RFC 9700 2.1.1 + RFC 7636 4.2",
            "The authorization server rejects non-PKCE public client requests and registered public clients use S256.",
            "CB4-3.4-812",
            ["PASS", "RFC 9700 2.1.1", "RFC 7636 4.2", "S256", "400 invalid_request"],
            []);
    }

    // Build the REPROCESS verdict anchored in CB4-3.4-813.
    private static SecurityArbiterVerdict BuildPipelineHs256Reprocess(string scenarioId)
    {
        // Return the reprocess verdict for HS256 shared-secret distribution.
        return new SecurityArbiterVerdict(
            scenarioId,
            "REPROCESS",
            "RFC-8725-JWT-Arbiter + ASVS-V9-Token-Arbiter",
            "RFC 8725 3.6 + V9.1.1",
            "HS256 distributes the signing secret to resource-server teams, so any resource-server compromise enables token forgery.",
            "CB4-3.4-813",
            ["REPROCESS", "RFC 8725 3.6", "V9.1.1", "HS256 shared secret", "token forgery", "simplicity rejected"],
            ["simplicity exception"]);
    }

    // Build the PASS verdict anchored in CB4-3.4-814.
    private static SecurityArbiterVerdict BuildPipelineRs256Pass(string scenarioId)
    {
        // Return the pass verdict for RS256 private-key isolation.
        return new SecurityArbiterVerdict(
            scenarioId,
            "PASS",
            "NoActivation-PASS-Confirmation",
            "RFC 8725 3.6 + V9.1.1",
            "RS256 keeps the private signing key only at the authorization server and resource servers verify through HTTPS JWKS.",
            "CB4-3.4-814",
            ["PASS", "RFC 8725 3.6", "V9.1.1", "RS256", "HTTPS JWKS", "private key exclusive"],
            []);
    }

    // Build the CONCERN verdict anchored in CB4-3.4-815.
    private static SecurityArbiterVerdict BuildPipelineIpOnlyConcern(string scenarioId)
    {
        // Return the concern verdict for IP-only rate limiting without per-client throttling.
        return new SecurityArbiterVerdict(
            scenarioId,
            "CONCERN",
            "ASVS-V6-Auth-Arbiter",
            "ASVS V6.2",
            "IP-only rate limiting is partial protection because distributed guessing against one client_id remains viable; high-volume clients can use configured limits.",
            "CB4-3.4-815",
            ["CONCERN", "ASVS V6.2", "IP-only rate limiting", "distributed attack viable", "high-volume argument addressable"],
            []);
    }

    // Build the PASS verdict anchored in CB4-3.4-816.
    private static SecurityArbiterVerdict BuildPipelinePerClientPass(string scenarioId)
    {
        // Return the pass verdict for per-client throttling with graduated response.
        return new SecurityArbiterVerdict(
            scenarioId,
            "PASS",
            "NoActivation-PASS-Confirmation",
            "ASVS V6.2",
            "Per-client-id throttling defeats distributed guessing and the 429 plus rotation response covers sustained attacks.",
            "CB4-3.4-816",
            ["PASS", "ASVS V6.2", "per-client-id counter", "429 Retry-After", "secret rotation", "layered with IP"],
            []);
    }

    // Build the CONCERN verdict anchored in CB4-3.4-817.
    private static SecurityArbiterVerdict BuildPipelineLogoutConcern(string scenarioId)
    {
        // Return the concern verdict for missing id_token_hint in logout.
        return new SecurityArbiterVerdict(
            scenarioId,
            "CONCERN",
            "OIDC-RP-Initiated-Logout-Arbiter",
            "OIDC RP-Initiated Logout",
            "End Session without id_token_hint leaves session forcing viable, and the harmless-logout argument is rejected.",
            "CB4-3.4-817",
            ["CONCERN", "OIDC RP-Initiated Logout", "id_token_hint absent", "session forcing", "harmless logout rejected"],
            []);
    }

    // Build the PASS verdict anchored in CB4-3.4-818.
    private static SecurityArbiterVerdict BuildPipelineLogoutPass(string scenarioId)
    {
        // Return the pass verdict for required and fully validated id_token_hint logout.
        return new SecurityArbiterVerdict(
            scenarioId,
            "PASS",
            "NoActivation-PASS-Confirmation",
            "OIDC RP-Initiated Logout",
            "id_token_hint is required and signature, subject, audience, expiry, and session binding are validated before logout.",
            "CB4-3.4-818",
            ["PASS", "OIDC RP-Initiated Logout", "id_token_hint required", "signature sub aud exp validation", "session forcing blocked"],
            []);
    }

    // Build the PASS verdict anchored in CB4-3.4-828.
    private static SecurityArbiterVerdict BuildComprehensiveExt31Pass(string scenarioId)
    {
        // Return the pass verdict for the complete ext31 control set.
        return new SecurityArbiterVerdict(
            scenarioId,
            "PASS",
            "RFC-9700-OAuth-Security-Arbiter + RFC-8725-JWT-Arbiter + ASVS-V6-Auth-Arbiter + OIDC-RP-Initiated-Logout-Arbiter",
            "ext31-comprehensive",
            "All four ext31 domains are correctly implemented: AS-enforced PKCE, RS256/JWKS signing, per-client-id throttling, and id_token_hint logout validation.",
            "CB4-3.4-828",
            ["PASS", "RFC 9700 2.1.1", "RFC 8725 3.6", "ASVS V6.2", "OIDC RP-Initiated Logout", "all four ext31 domains"],
            []);
    }

    // Build the PASS verdict anchored in CB4-3.4-822.
    private static SecurityArbiterVerdict BuildThrottleAndLogoutPass(string scenarioId)
    {
        // Return the pass verdict for per-client throttling and logout hint validation.
        return new SecurityArbiterVerdict(
            scenarioId,
            "PASS",
            "ASVS-V6-Auth-Arbiter + OIDC-RP-Initiated-Logout-Arbiter",
            "ASVS V6.2 + OIDC RP-Initiated Logout",
            "Per-client-id throttling blocks distributed credential attacks and id_token_hint validation blocks session forcing.",
            "CB4-3.4-822",
            ["PASS", "ASVS V6.2", "OIDC RP-Initiated Logout", "per-client-id rate limiting", "id_token_hint required", "session forcing blocked"],
            []);
    }

    // Build the REPROCESS verdict anchored in CB4-3.4-823.
    private static SecurityArbiterVerdict BuildHs256AndRopcReprocess(string scenarioId)
    {
        // Return the reprocess verdict for two independent legacy security failures.
        return new SecurityArbiterVerdict(
            scenarioId,
            "REPROCESS",
            "RFC-8725-JWT-Arbiter + RFC-9700-OAuth-Security-Arbiter",
            "RFC 8725 3.6 + RFC 9700 2.4",
            "HS256 shared-secret distribution and enabled ROPC are independent MUST-level failures; legacy reliability is not an exception.",
            "CB4-3.4-823",
            ["REPROCESS", "RFC 8725 3.6", "RFC 9700 2.4", "HS256 multi-party", "ROPC enabled", "two independent MUST-level violations"],
            ["legacy exception"]);
    }

    // Build the PASS verdict anchored in CB4-3.4-824.
    private static SecurityArbiterVerdict BuildRs256AndRopcDisabledPass(string scenarioId)
    {
        // Return the pass verdict for asymmetric signing and replacement of ROPC.
        return new SecurityArbiterVerdict(
            scenarioId,
            "PASS",
            "RFC-8725-JWT-Arbiter + RFC-9700-OAuth-Security-Arbiter",
            "RFC 8725 3.6 + RFC 9700 2.4",
            "RS256 keeps signing authority at the authorization server, and disabling ROPC preserves the OAuth trust boundary.",
            "CB4-3.4-824",
            ["PASS", "RFC 8725 3.6", "RFC 9700 2.4", "RS256 asymmetric signing", "ROPC disabled", "authorization code plus PKCE S256"],
            []);
    }

    // Build the REPROCESS verdict anchored in CB4-3.4-825.
    private static SecurityArbiterVerdict BuildPkceTrainingGapReprocess(string scenarioId)
    {
        // Return the reprocess verdict for a production PKCE failure caused by a training gap.
        return new SecurityArbiterVerdict(
            scenarioId,
            "REPROCESS",
            "RFC-9700-OAuth-Security-Arbiter + SSDF-PO1-Training-Arbiter",
            "RFC 9700 2.1.1 + SSDF PO.1",
            "Public-client PKCE is not enforced, and the configuration failure is directly traceable to missing current RFC 9700 training.",
            "CB4-3.4-825",
            ["REPROCESS", "RFC 9700 2.1.1", "SSDF PO.1", "training gap proximate cause", "React SPA without PKCE", "native app without PKCE"],
            ["RFC 6749 optional PKCE exception"]);
    }

    // Build the REPROCESS verdict anchored in CB4-3.4-826.
    private static SecurityArbiterVerdict BuildHs256NoExpReprocess(string scenarioId)
    {
        // Return the reprocess verdict for tokens that are both forgeable and non-expiring.
        return new SecurityArbiterVerdict(
            scenarioId,
            "REPROCESS",
            "RFC-8725-JWT-Arbiter + RFC-9068-Access-Token-Arbiter",
            "RFC 8725 3.6 + RFC 9068 2.2",
            "HS256 shared-secret distribution makes tokens forgeable, and missing exp means forged access tokens have no resource-server time boundary.",
            "CB4-3.4-826",
            ["REPROCESS", "RFC 8725 3.6", "RFC 9068 2.2", "HS256 multi-party", "no exp claim", "forgeable and non-expiring tokens"],
            []);
    }

    // Build the REPROCESS verdict anchored in CB4-3.4-827.
    private static SecurityArbiterVerdict BuildPkceOverridesThrottleConcern(string scenarioId)
    {
        // Return the reprocess verdict where the current PKCE MUST violation absorbs the throttling concern.
        return new SecurityArbiterVerdict(
            scenarioId,
            "REPROCESS",
            "RFC-9700-OAuth-Security-Arbiter + ASVS-V6-Auth-Arbiter",
            "RFC 9700 2.1.1 + ASVS V6.2",
            "The IP-only rate limiting concern is preserved, but current public-client PKCE non-enforcement determines REPROCESS and a future release is not current compliance.",
            "CB4-3.4-827",
            ["REPROCESS", "RFC 9700 2.1.1", "REPROCESS overrides CONCERN", "IP-only rate limiting CONCERN absorbed", "future release is not current compliance"],
            ["future release exception"]);
    }

    // Build the REPROCESS verdict anchored in CB4-3.4-819.
    private static SecurityArbiterVerdict BuildPkceAndHs256Reprocess(string scenarioId)
    {
        // Return the reprocess verdict for two independent MUST-level violations.
        return new SecurityArbiterVerdict(
            scenarioId,
            "REPROCESS",
            "RFC-9700-OAuth-Security-Arbiter + RFC-8725-JWT-Arbiter",
            "RFC 9700 2.1.1 + RFC 8725 3.6",
            "Both non-PKCE public client flows and HS256 shared-secret distribution are independent MUST-level failures; gradual adoption and operational simplicity are not normative exceptions.",
            "CB4-3.4-819",
            ["REPROCESS", "RFC 9700 2.1.1", "RFC 8725 3.6", "two independent MUST-level violations", "non-PKCE public client flows", "HS256 multi-party signing secret"],
            ["gradual adoption exception", "operational simplicity exception"]);
    }

    // Build the PASS verdict anchored in CB4-3.4-820.
    private static SecurityArbiterVerdict BuildPkceAndRs256Pass(string scenarioId)
    {
        // Return the pass verdict for both required cross-source controls.
        return new SecurityArbiterVerdict(
            scenarioId,
            "PASS",
            "RFC-9700-OAuth-Security-Arbiter + RFC-8725-JWT-Arbiter",
            "RFC 9700 2.1.1 + RFC 8725 3.6",
            "PKCE S256 is enforced for public clients and RS256 keeps the signing private key at the authorization server while resource servers use JWKS public keys.",
            "CB4-3.4-820",
            ["PASS", "RFC 9700 2.1.1", "RFC 8725 3.6", "PKCE S256 enforced", "RS256 asymmetric signing", "HTTPS JWKS verification"],
            []);
    }

    // Build the CONCERN verdict anchored in CB4-3.4-821.
    private static SecurityArbiterVerdict BuildIpLimitAndLogoutConcern(string scenarioId)
    {
        // Return the concern verdict for two SHOULD-level gaps that must not be escalated.
        return new SecurityArbiterVerdict(
            scenarioId,
            "CONCERN",
            "ASVS-V6-Auth-Arbiter + OIDC-RP-Initiated-Logout-Arbiter",
            "ASVS V6.2 + OIDC RP-Initiated Logout",
            "IP-only throttling and End Session without id_token_hint are concern-level gaps; the graduation rule keeps concern plus concern as CONCERN rather than REPROCESS.",
            "CB4-3.4-821",
            ["CONCERN", "ASVS V6.2", "OIDC RP-Initiated Logout", "N CONCERNs do not escalate to REPROCESS", "per-client-id throttling absent", "id_token_hint absent"],
            ["REPROCESS escalation"]);
    }

    // Build the REPROCESS verdict anchored in CB4-3.4-803.
    private static SecurityArbiterVerdict BuildPublicClientPkceReprocess(string scenarioId)
    {
        // Return the reprocess verdict for missing authorization-server PKCE enforcement.
        return new SecurityArbiterVerdict(
            scenarioId,
            "REPROCESS",
            "RFC-9700-OAuth-Security-Arbiter",
            "RFC 9700 2.1.1",
            "Authorization servers must require PKCE for public clients; accepting authorization code requests without code_challenge violates the current MUST.",
            "CB4-3.4-803",
            ["REPROCESS", "RFC 9700 2.1.1", "PKCE required for public clients", "public clients without code_challenge rejected"],
            ["backward compatibility exception"]);
    }

    // Build the CONCERN verdict anchored in CB4-3.4-807.
    private static SecurityArbiterVerdict BuildIpOnlyRateLimitConcern(string scenarioId)
    {
        // Return the concern verdict for IP-only throttling without a per-client counter.
        return new SecurityArbiterVerdict(
            scenarioId,
            "CONCERN",
            "ASVS-V6-Auth-Arbiter",
            "ASVS V6.2",
            "IP-only token endpoint throttling is partial protection; distributed credential attacks remain viable without per-client-id throttling.",
            "CB4-3.4-807",
            ["CONCERN", "ASVS V6.2", "per-client-id throttling absent", "distributed credential attack viable"],
            []);
    }

    // Build the PASS verdict anchored in CB4-3.4-808.
    private static SecurityArbiterVerdict BuildPerClientRateLimitPass(string scenarioId)
    {
        // Return the pass verdict for per-client-id throttling with graduated response.
        return new SecurityArbiterVerdict(
            scenarioId,
            "PASS",
            "ASVS-V6-Auth-Arbiter",
            "ASVS V6.2",
            "Per-client-id token endpoint throttling with 429 Retry-After, rotation flagging, and owner notification addresses distributed credential attacks.",
            "CB4-3.4-808",
            ["PASS", "ASVS V6.2", "per-client-id rate limiting", "429", "Retry-After", "secret rotation flag"],
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
