# CB4 Stage 3 — Pipeline Chain Set Extension 5
# Sequences CB4-3.4-301 through CB4-3.4-308
# Stratum: C — full 9-node pipeline traces, positive and negative cases for new sub-domains
# Instances: 16 (8 base pairs × 2 phrasings)
# Running total after this file: 616 instances

---

## Pair[CB4-3.4-301]

**A:** OAuth server audit. Redirect URIs validated via prefix match — `provided_uri.startsWith(registered_prefix)`. S256 PKCE present and correct. State parameter present on all flows.

**B:**
```
Node 01 — Domain Splitter:
Input signals: OAuth flow, prefix-matched redirect URI validation (not exact),
PKCE S256, state parameter present.
Territories: [T-oauth-oidc]
All signals resolve to the same territory. Prefix match is a V10.2.1 gap.

Node 02 — Domain Translator:
T-oauth-oidc → oauth-oidc-normative

Node 03 — Scheme Planner:
Linear (single territory). Prefix-match argument passed through for Node 08.

Node 04 — Domain Processor:
V10.2.1: exact registration required. Evidence: startsWith prefix match.
  Prefix match accepts any path under registered prefix, not just the
  registered URI. GAP.
V10.2.6 (state): SATISFIED — state present on all flows.
V10.4.6 (PKCE S256): SATISFIED — correct implementation.

Node 05 — Applier:
Context: production OAuth server. Prefix matching allows path-confusion
redirect exploitation. REPROCESS confirmed for V10.2.1 regardless of
correct PKCE and state.

Node 06 — Result Translator:
Tags: "V10.2.1", "exact registration", "prefix matching", "startsWith",
"redirect URI", "path confusion", "V10.4.6 PASS", "V10.2.6 PASS".

Node 07 — Arbiter Selector:
oauth-oidc → ASVS-V10-OAuth.

Node 08 — Arbiter Unit (ASVS-V10-OAuth):
V10.2.6 (state): PASS.
V10.4.6 (PKCE S256): PASS.
V10.2.1 (redirect URI): REPROCESS — startsWith prefix match is not exact
  registration. Any path under the prefix is accepted. Exact character
  comparison against the registered set is required.
Territory verdict: REPROCESS (one REPROCESS overrides two PASSes).

Node 09 — Formatter:
{
  "flow": "security-verification-3.4",
  "territories": ["oauth-oidc"],
  "findings": [
    {
      "territory": "oauth-oidc",
      "arbiter": "ASVS-V10-OAuth",
      "sub_findings": [
        {"requirement": "V10.2.6", "verdict": "PASS"},
        {"requirement": "V10.4.6", "verdict": "PASS"},
        {"requirement": "V10.2.1", "verdict": "REPROCESS",
         "gap": "prefix match (startsWith) not exact registration; accepts any path under registered prefix"}
      ],
      "territory_verdict": "REPROCESS"
    }
  ],
  "overall_verdict": "REPROCESS",
  "source_attribution": "OWASP ASVS 5.0.0 — CC BY-SA 4.0"
}
```

**Reason:** Three sub-requirements within the same oauth-oidc territory. Two PASS (V10.2.6 state, V10.4.6 PKCE) and one REPROCESS (V10.2.1 prefix match). Node 04 correctly maps all three. Node 08 evaluates all three and produces REPROCESS for V10.2.1. Territory verdict is REPROCESS — one REPROCESS in a territory overrides the PASSes.

**Phrasing-B:** OAuth with prefix-matched redirect URIs, correct S256 PKCE, and state parameter on all flows. Full trace showing how three V10 sub-requirements are evaluated and how one REPROCESS affects the territory verdict.

---

## Pair[CB4-3.4-302]

**A:** Session security assessment. JWT sessions with 5-minute expiry. Logout clears the cookie; no server-side revocation. Team argues 5-minute window is negligible.

**B:**
```
Node 01 — Domain Splitter:
Input signals: JWT session token, 5-minute expiry, logout without server-side
revocation, "5-minute window" mitigation argument.
Territories: [T-asvs-session]
Session management territory. Short expiry is a context factor, not a separate
territory.

Node 02 — Domain Translator:
T-asvs-session → asvs-session-normative

Node 03 — Scheme Planner:
Linear. Mitigation argument passed to Node 08.

Node 04 — Domain Processor:
V7.3.1: server-side invalidation on logout required. Evidence: cookie deleted
client-side only, no server-side revocation. GAP with mitigating factor.
Mitigation noted: 5-minute expiry significantly reduces post-logout window.
Both gap and mitigation passed to Node 08.

Node 05 — Applier:
Context: application with short-lived sessions. 5-minute expiry is a genuine
and significant mitigating factor. Severity is reduced compared to long-lived
tokens. Context supports CONCERN classification.

Node 06 — Result Translator:
Tags: "V7.3.1", "server-side invalidation", "logout", "5-minute expiry",
"CONCERN", "mitigating factor", "stateless JWT".

Node 07 — Arbiter Selector:
asvs-session → ASVS-V7-Session.

Node 08 — Arbiter Unit (ASVS-V7-Session):
V7.3.1: CONCERN.
Argument partially accepted: 5-minute expiry substantially reduces
exploitation window. This is a genuine mitigating factor.
Argument rejected: "5 minutes is negligible / no meaningful exploit."
Automated tools operate in milliseconds; 5 minutes is exploitable.
Finding: CONCERN (not REPROCESS). Short expiry is a meaningful compensating
control; V7.3.1 server-side invalidation is not fully satisfied but severity
is significantly reduced. Recommend server-side revocation as remediation.
Territory verdict: CONCERN.

Node 09 — Formatter:
{
  "flow": "security-verification-3.4",
  "territories": ["asvs-session"],
  "findings": [
    {
      "territory": "asvs-session",
      "arbiter": "ASVS-V7-Session",
      "sub_findings": [
        {"requirement": "V7.3.1", "verdict": "CONCERN",
         "note": "5-minute expiry is a significant mitigating factor; 'negligible window' argument rejected but expiry reduces severity to CONCERN",
         "gap": "server-side session revocation absent; short expiry is mitigating not resolving"}
      ],
      "territory_verdict": "CONCERN"
    }
  ],
  "overall_verdict": "CONCERN",
  "source_attribution": "OWASP ASVS 5.0.0 — CC BY-SA 4.0"
}
```

**Reason:** The CONCERN classification requires explicit tracing through the pipeline — Node 04 flags both the gap and the mitigating factor; Node 05 confirms the severity reduction in context; Node 08 adjudicates the argument, partially accepting the mitigation while rejecting the "negligible" characterization. The formatter records both the CONCERN verdict and the reasoning (mitigating factor accepted, "negligible" claim rejected).

**Phrasing-B:** JWT sessions, 5-minute expiry, logout clears cookie only. Team says 5 minutes is effectively safe from exploitation. Full pipeline trace showing where the mitigation argument is processed and why this is CONCERN rather than REPROCESS.

---

## Pair[CB4-3.4-303]

**A:** Credential management review — correct implementation case. Application retrieves all secrets at startup from HashiCorp Vault using AppRole authentication. No credentials in source code or configuration files committed to version control. Vault TTLs enforce rotation.

**B:**
```
Node 01 — Domain Splitter:
Input signals: credential management, Vault for secrets, no hardcoded
credentials, AppRole authentication pattern.
Territories: [T-ssdf-process]
Process evidence for PW.1. No authentication weakness signals (credentials
properly managed, not exposed). Positive evidence.

Node 02 — Domain Translator:
T-ssdf-process → ssdf-process-normative

Node 03 — Scheme Planner:
Linear. Positive evidence evaluation.

Node 04 — Domain Processor:
PW.1: credentials MUST NOT be hardcoded. Evidence: all secrets from Vault,
no credentials in source code, no credentials in version-controlled configs.
SATISFIED.

Node 05 — Applier:
Context: application using purpose-built secrets management. Vault with TTL
rotation is a best-practice implementation. PW.1 PASS confirmed.

Node 06 — Result Translator:
Tags: "PW.1", "Vault", "AppRole", "no hardcoded credentials", "TTL rotation",
"secrets management".

Node 07 — Arbiter Selector:
ssdf-process → SSDF-PW.

Node 08 — Arbiter Unit (SSDF-PW):
PW.1: PASS — credentials retrieved from HashiCorp Vault at runtime via
AppRole authentication. No credentials in source code. TTL-enforced rotation.
This is the recommended implementation pattern for PW.1.
Territory verdict: PASS.

Node 09 — Formatter:
{
  "flow": "security-verification-3.4",
  "territories": ["ssdf-process"],
  "findings": [
    {
      "territory": "ssdf-process",
      "arbiter": "SSDF-PW",
      "sub_findings": [
        {"practice": "PW.1", "verdict": "PASS"}
      ],
      "territory_verdict": "PASS"
    }
  ],
  "overall_verdict": "PASS",
  "source_attribution": "NIST SP 800-218 SSDF v1.1 — public domain"
}
```

**Reason:** The pipeline processes positive evidence the same way it processes gaps — each node performs its function on the available evidence. Node 04 maps the evidence to PW.1 and marks it as SATISFIED. Node 08 confirms PASS. The formatter outputs PASS. Positive cases must flow through the entire 9-node pipeline correctly; truncating the pipeline for PASS cases is incorrect behavior.

**Phrasing-B:** HashiCorp Vault with AppRole, all secrets at runtime, no credentials in source or committed config, TTL rotation enforced. Full 9-node trace for a correct PW.1 implementation showing the PASS path through the pipeline.

---

## Pair[CB4-3.4-304]

**A:** PKCE implementation review. S256 code_challenge included in all authorization requests. At token exchange, the server accepts any value for code_verifier without validation — the code can be exchanged by any party with the authorization code. Team: "We include the challenge to be standards-compliant. Verifier validation adds complexity."

**B:**
```
Node 01 — Domain Splitter:
Input signals: PKCE S256 challenge present, no code_verifier validation at
exchange, authorization code exchangeable without verifier, "complexity"
argument.
Territories: [T-oauth-oidc]
Single territory. Challenge without verifier validation is a V10.4.6 gap.

Node 02 — Domain Translator:
T-oauth-oidc → oauth-oidc-normative

Node 03 — Scheme Planner:
Linear. Argument passed to Node 08.

Node 04 — Domain Processor:
V10.4.6: challenge present (S256). Verifier validation at exchange: absent.
PKCE requires both. GAP: verifier validation absent.

Node 05 — Applier:
Context: production authorization server. Without verifier validation, any
party holding the authorization code can exchange it — PKCE provides zero
protection in this configuration. REPROCESS confirmed.

Node 06 — Result Translator:
Tags: "V10.4.6", "PKCE", "code_challenge", "S256", "code_verifier", "exchange",
"validation absent", "REPROCESS".

Node 07 — Arbiter Selector:
oauth-oidc → ASVS-V10-OAuth.

Node 08 — Arbiter Unit (ASVS-V10-OAuth):
V10.4.6: REPROCESS.
Argument rejected: "we include the challenge to be standards-compliant;
verifier validation adds complexity."
Including a challenge without enforcing the verifier is not standards-compliant —
it is performing the appearance of PKCE while providing none of its security
properties. PKCE's protection is entirely in the verifier validation at exchange;
the challenge alone provides no security. The "complexity" of verifier validation
is the security measure itself.
Evidence gap: code_verifier not validated at token exchange.
Territory verdict: REPROCESS.

Node 09 — Formatter:
{
  "flow": "security-verification-3.4",
  "territories": ["oauth-oidc"],
  "findings": [
    {
      "territory": "oauth-oidc",
      "arbiter": "ASVS-V10-OAuth",
      "sub_findings": [
        {"requirement": "V10.4.6", "verdict": "REPROCESS",
         "argument_rejected": "challenge without verifier validation provides zero PKCE security; 'complexity' argument rejected",
         "gap": "code_verifier not validated at token exchange"}
      ],
      "territory_verdict": "REPROCESS"
    }
  ],
  "overall_verdict": "REPROCESS",
  "source_attribution": "OWASP ASVS 5.0.0 — CC BY-SA 4.0"
}
```

**Reason:** The "we include the challenge for compliance" argument is a form of security theater — performing the appearance of a security control without its substance. Node 08 explicitly rejects this argument: a challenge without verifier validation is not PKCE and provides zero protection. The "complexity" argument is also rejected since verifier validation is exactly the security value of PKCE.

**Phrasing-B:** We send S256 code_challenges on all authorization requests. At exchange we don't validate the code_verifier because it adds complexity. The challenge is there to show we support PKCE. Full trace showing why challenge-without-verifier-validation is still REPROCESS.

---

## Pair[CB4-3.4-305]

**A:** Password hashing security review — correct implementation. Application uses Argon2id with m=65536 (64 MB), t=3, p=4. Migration from bcrypt (cost 10) was completed 6 months ago — old bcrypt hashes are re-hashed to Argon2id on next successful login. Old bcrypt hashes from before migration are still in the database for inactive users.

**B:**
```
Node 01 — Domain Splitter:
Input signals: Argon2id m=65536/t=3/p=4 (current), bcrypt cost 10 (legacy,
being migrated on login), partial migration in progress.
Territories: [T-asvs-authentication]

Node 02 — Domain Translator:
T-asvs-authentication → asvs-authentication-normative

Node 03 — Scheme Planner:
Linear. Migration state context passed through.

Node 04 — Domain Processor:
V6.2.5: current hashing algorithm Argon2id m=65536, t=3, p=4. SATISFIED.
Migration context: bcrypt cost 10 (old hashes). bcrypt cost 10 is an
appropriate cost factor — this is not a vulnerability in the bcrypt tier.
The migration to Argon2id is in progress (lazy migration on login). Inactive
users retain bcrypt cost 10 hashes which are acceptable.

Node 05 — Applier:
Context: production with phased migration. Active users are on Argon2id.
Inactive users with bcrypt cost 10 are in a secure intermediate state.
No V6.2.5 gap exists: both the current algorithm and the legacy algorithm
are appropriate. PASS.

Node 06 — Result Translator:
Tags: "V6.2.5", "Argon2id", "bcrypt cost 10", "migration", "PASS",
"memory-hard", "appropriate configuration".

Node 07 — Arbiter Selector:
asvs-authentication → ASVS-V6-Auth.

Node 08 — Arbiter Unit (ASVS-V6-Auth):
V6.2.5: PASS.
Argon2id m=65536, t=3, p=4: appropriate memory-hard algorithm with
appropriate parameters.
bcrypt cost 10 (legacy, migration in progress): bcrypt cost 10 is an
appropriate configuration — it is not a V6.2.5 violation. The lazy
migration pattern is a standard approach for credential hash upgrades.
Both algorithm tiers satisfy V6.2.5. Migration to a stronger algorithm
is best practice, not a compliance correction.
Territory verdict: PASS.

Node 09 — Formatter:
{
  "flow": "security-verification-3.4",
  "territories": ["asvs-authentication"],
  "findings": [
    {
      "territory": "asvs-authentication",
      "arbiter": "ASVS-V6-Auth",
      "sub_findings": [
        {"requirement": "V6.2.5", "verdict": "PASS",
         "note": "Argon2id active; bcrypt cost 10 legacy hashes in migration — both algorithms are V6.2.5 compliant"}
      ],
      "territory_verdict": "PASS"
    }
  ],
  "overall_verdict": "PASS",
  "source_attribution": "OWASP ASVS 5.0.0 — CC BY-SA 4.0"
}
```

**Reason:** Both Argon2id and bcrypt cost 10 are V6.2.5-compliant algorithms. The partial migration is a positive pattern, not a gap. Node 08 correctly recognizes that bcrypt cost 10 (appropriate cost factor) is compliant — the earlier CONCERN was for bcrypt cost 4, which is insufficient. Migrating from one compliant algorithm to another (even better) algorithm is best practice, not remediation.

**Phrasing-B:** Current passwords: Argon2id m=65536/t=3/p=4. Legacy inactive users: bcrypt cost 10, being migrated on next login. Is a partial migration state a V6.2.5 concern, or is bcrypt cost 10 still compliant?

---

## Pair[CB4-3.4-306]

**A:** Multi-finding OAuth review. Three separate gaps: (1) Redirect URI validated via startsWith prefix match. (2) PKCE uses code_challenge_method=plain (not S256). (3) Threat model last updated 20 months ago.

**B:**
```
Node 01 — Domain Splitter:
Territories: [T-oauth-oidc, T-rfc-oauth2, T-ssdf-process]
Prefix match and plain PKCE are V10.2.1 and V10.4.6 gaps (oauth-oidc).
Plain PKCE method also triggers RFC 7636 evaluation (rfc-oauth2).
Stale threat model is a PW.2 gap (ssdf-process).

Node 02 — Domain Translator:
T-oauth-oidc → oauth-oidc-normative
T-rfc-oauth2 → rfc-oauth2-normative
T-ssdf-process → ssdf-process-normative

Node 03 — Scheme Planner:
Parallel (three territories). RFC 7636 and ASVS V10 are independent
evaluations of the PKCE method; they run in parallel.

Node 04 — Domain Processor (parallel):
oauth-oidc: V10.2.1 GAP (prefix match). V10.4.6 GAP (plain method).
rfc-oauth2: RFC 7636 §4.2 — S256 MUST be used when available. Plain method
  on a standard platform. GAP.
ssdf-process: PW.2 — 20-month threat model. GAP.

Node 05 — Applier:
Context: three independent violations. No mitigating context changes any verdict.

Node 06 — Result Translator:
oauth-oidc tags: "V10.2.1", "prefix match", "V10.4.6", "plain PKCE".
rfc-oauth2 tags: "RFC 7636", "S256 MUST", "plain method".
ssdf-process tags: "PW.2", "threat model", "20 months".

Node 07 — Arbiter Selector:
oauth-oidc → ASVS-V10-OAuth
rfc-oauth2 → RFC-OAuth2
ssdf-process → SSDF-PW

Node 08 — Arbiter Units (parallel):
ASVS-V10-OAuth: V10.2.1 REPROCESS (prefix ≠ exact). V10.4.6 REPROCESS (plain).
RFC-OAuth2: RFC 7636 §4.2 REPROCESS (S256 MUST when capable; plain is fallback only).
SSDF-PW: PW.2 REPROCESS (20 months > threshold; no current threat model).

Node 09 — Formatter:
{
  "flow": "security-verification-3.4",
  "territories": ["oauth-oidc", "rfc-oauth2", "ssdf-process"],
  "findings": [
    {
      "territory": "oauth-oidc",
      "arbiter": "ASVS-V10-OAuth",
      "sub_findings": [
        {"requirement": "V10.2.1", "verdict": "REPROCESS",
         "gap": "prefix match not exact registration"},
        {"requirement": "V10.4.6", "verdict": "REPROCESS",
         "gap": "code_challenge_method=plain; S256 required"}
      ],
      "territory_verdict": "REPROCESS"
    },
    {
      "territory": "rfc-oauth2",
      "arbiter": "RFC-OAuth2",
      "sub_findings": [
        {"standard": "RFC 7636 §4.2", "verdict": "REPROCESS",
         "gap": "plain method used; S256 MUST be used when capable"}
      ],
      "territory_verdict": "REPROCESS"
    },
    {
      "territory": "ssdf-process",
      "arbiter": "SSDF-PW",
      "sub_findings": [
        {"practice": "PW.2", "verdict": "REPROCESS",
         "gap": "threat model 20 months old; current system not modeled"}
      ],
      "territory_verdict": "REPROCESS"
    }
  ],
  "overall_verdict": "REPROCESS",
  "source_attribution": [
    "OWASP ASVS 5.0.0 — CC BY-SA 4.0",
    "IETF RFC 7636 — IETF Trust provisions",
    "NIST SP 800-218 SSDF v1.1 — public domain"
  ]
}
```

**Reason:** Three-territory trace with three independent REPROCESS findings. The PKCE plain method creates two territory entries (oauth-oidc for V10.4.6 and rfc-oauth2 for RFC 7636) because two standards independently govern the method selection. Node 01 correctly splits these into separate territories for independent evaluation.

**Phrasing-B:** OAuth server: prefix-matched redirect URIs, plain PKCE method, 20-month threat model. Full 3-territory trace showing V10 + RFC 7636 + SSDF combined.

---

## Pair[CB4-3.4-307]

**A:** Security review combining a correct practice and a gap. Application credentials retrieved via `os.environ.get('DB_PASSWORD')` injected from Vault at startup (PW.1 correct). JWT tokens transmitted to a legacy reporting service via URL query parameter (V9.3.1 gap — migration scheduled Q3).

**B:**
```
Node 01 — Domain Splitter:
Input signals: env var credential from Vault (PW.1 satisfied), JWT tokens
in URL parameters (V9.3.1 gap).
Territories: [T-ssdf-process, T-asvs-token]

Node 02 — Domain Translator:
T-ssdf-process → ssdf-process-normative
T-asvs-token → asvs-token-normative

Node 03 — Scheme Planner:
Parallel. Independent territories.

Node 04 — Domain Processor (parallel):
ssdf-process: PW.1 — env var from Vault. SATISFIED.
asvs-token: V9.3.1 — tokens MUST NOT be transmitted via URL parameters.
  Evidence: JWT in query param on reporting service endpoint. GAP.
  Migration timeline noted; passed to arbiter.

Node 05 — Applier:
PW.1 context: correct credential management. Token transport context:
URL parameters regardless of migration timeline create log exposure risk.

Node 06 — Result Translator:
ssdf-process tags: "PW.1", "Vault", "environment variable", "PASS".
asvs-token tags: "V9.3.1", "URL query parameter", "JWT", "log exposure", "REPROCESS".

Node 07 — Arbiter Selector:
ssdf-process → SSDF-PW
asvs-token → ASVS-V9-Token

Node 08 — Arbiter Units:
SSDF-PW: PW.1 PASS. Vault + env var is correct pattern.
ASVS-V9-Token: V9.3.1 REPROCESS. URL query parameter token delivery.
  Migration timeline does not suspend the MUST requirement.

Node 09 — Formatter:
{
  "flow": "security-verification-3.4",
  "territories": ["ssdf-process", "asvs-token"],
  "findings": [
    {
      "territory": "ssdf-process",
      "arbiter": "SSDF-PW",
      "sub_findings": [
        {"practice": "PW.1", "verdict": "PASS"}
      ],
      "territory_verdict": "PASS"
    },
    {
      "territory": "asvs-token",
      "arbiter": "ASVS-V9-Token",
      "sub_findings": [
        {"requirement": "V9.3.1", "verdict": "REPROCESS",
         "gap": "JWT tokens transmitted via URL query parameter to reporting service"}
      ],
      "territory_verdict": "REPROCESS"
    }
  ],
  "overall_verdict": "REPROCESS",
  "source_attribution": [
    "NIST SP 800-218 SSDF v1.1 — public domain",
    "OWASP ASVS 5.0.0 — CC BY-SA 4.0"
  ]
}
```

**Reason:** Two territories: one PASS (PW.1, Vault credentials), one REPROCESS (V9.3.1, URL token delivery). REPROCESS overrides PASS in the overall verdict. The trace shows that correct credential management doesn't offset a separate token transport violation — they are independent controls in separate territories.

**Phrasing-B:** Credentials from Vault via env vars (PW.1 correct) but JWT tokens going to a legacy reporting service in URL query params (V9.3.1 gap). Mixed PASS/REPROCESS across two territories. Full trace.

---

## Pair[CB4-3.4-308]

**A:** Complete positive security review covering all new sub-domains plus full pipeline trace. All controls verified: exact OAuth redirect URIs, S256 PKCE with verifier validation, state parameter, server-side session revocation, Argon2id m=65536/t=3/p=4, permission introspection endpoint, Vault credentials, Security Champion review, SAST, current threat model (5 months), deny-by-default server-side.

**B:**
```
Node 01 — Domain Splitter:
All positive signals across all security domains.
Territories: [T-oauth-oidc, T-asvs-session, T-asvs-authentication,
              T-asvs-authorization, T-ssdf-process]

Node 02 — Domain Translator:
All five territories to respective normative labels.

Node 03 — Scheme Planner:
Parallel (five territories, all positive evidence).

Node 04 — Domain Processor (parallel — all SATISFIED):
oauth-oidc: V10.2.1 exact URIs ✓. V10.2.6 state ✓. V10.4.6 S256 + verifier ✓.
asvs-session: V7.3.1 server-side revocation ✓.
asvs-authentication: V6.2.5 Argon2id m=65536/t=3/p=4 ✓.
asvs-authorization: V8.2.1 /me/permissions ✓. V8.1.2 deny-by-default ✓.
ssdf-process: PW.1 Vault ✓. PW.3.2 Security Champion + checklist ✓.
  PW.4 SAST ✓. PW.2 threat model 5 months ✓.

Node 05 — Applier:
Context: production application. All controls verified. No context factor
changes any assessment.

Node 06 — Result Translator:
All territories: positive compliance vocabulary.

Node 07 — Arbiter Selector:
Five arbiters selected one per territory.

Node 08 — Arbiter Units (all PASS):
ASVS-V10-OAuth: V10.2.1 PASS. V10.2.6 PASS. V10.4.6 PASS. PASS.
ASVS-V7-Session: V7.3.1 PASS. PASS.
ASVS-V6-Auth: V6.2.5 PASS. PASS.
ASVS-V8-AuthZ: V8.2.1 PASS. V8.1.2 PASS. PASS.
SSDF-PW: PW.1 PASS. PW.3.2 PASS. PW.4 PASS. PW.2 PASS. PASS.

Node 09 — Formatter:
{
  "flow": "security-verification-3.4",
  "territories": ["oauth-oidc", "asvs-session", "asvs-authentication",
                  "asvs-authorization", "ssdf-process"],
  "findings": [
    {"territory": "oauth-oidc", "arbiter": "ASVS-V10-OAuth",
     "territory_verdict": "PASS"},
    {"territory": "asvs-session", "arbiter": "ASVS-V7-Session",
     "territory_verdict": "PASS"},
    {"territory": "asvs-authentication", "arbiter": "ASVS-V6-Auth",
     "territory_verdict": "PASS"},
    {"territory": "asvs-authorization", "arbiter": "ASVS-V8-AuthZ",
     "territory_verdict": "PASS"},
    {"territory": "ssdf-process", "arbiter": "SSDF-PW",
     "territory_verdict": "PASS"}
  ],
  "overall_verdict": "PASS",
  "source_attribution": [
    "OWASP ASVS 5.0.0 — CC BY-SA 4.0",
    "NIST SP 800-218 SSDF v1.1 — public domain"
  ]
}
```

**Reason:** Comprehensive all-PASS pipeline trace for all new sub-domains introduced across ext10-ext11. The trace demonstrates that positive evidence flows correctly through all nine nodes, each node performing its function on satisfying evidence, and the formatter outputs PASS at each territory and overall. This is the reference end-state for a fully compliant deployment.

**Phrasing-B:** Comprehensive all-controls-satisfied review. Full 9-node trace for the ideal case — exact OAuth URIs, complete PKCE, state, session revocation, Argon2id, permission endpoint, Vault, Security Champion, SAST, current threat model, deny-by-default.

---

## File Summary

| Pair ID | Description | Stratum | Territories | Verdict |
|---------|-------------|---------|-------------|---------|
| CB4-3.4-301 | V10.2.1 prefix match + V10.2.6 PASS + V10.4.6 PASS — one REPROCESS | C | oauth-oidc | REPROCESS |
| CB4-3.4-302 | V7.3.1 5-min expiry CONCERN — mitigation argument processing trace | C | asvs-session | CONCERN |
| CB4-3.4-303 | PW.1 Vault correct implementation — full PASS pipeline trace | C | ssdf-process | PASS |
| CB4-3.4-304 | V10.4.6 challenge without verifier "complexity" argument rejected | C | oauth-oidc | REPROCESS |
| CB4-3.4-305 | V6.2.5 Argon2id + bcrypt cost 10 legacy migration — full PASS | C | asvs-authentication | PASS |
| CB4-3.4-306 | V10.2.1 prefix + V10.4.6 plain + RFC 7636 + PW.2 stale — 3-territory chain | C | oauth-oidc, rfc-oauth2, ssdf-process | REPROCESS |
| CB4-3.4-307 | PW.1 Vault PASS + V9.3.1 URL token REPROCESS — mixed territory trace | C | ssdf-process, asvs-token | REPROCESS |
| CB4-3.4-308 | All new sub-domain 5-territory comprehensive PASS — final reference baseline | C | oauth-oidc, asvs-session, asvs-authentication, asvs-authorization, ssdf-process | PASS |
