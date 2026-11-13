# CB4 Stage 3 — Pipeline Chain Set Extension 6
# Sequences CB4-3.4-317 through CB4-3.4-324
# Stratum: C — full 9-node traces covering correct scoping decisions, mixed verdicts,
#           and team-argument processing chains
# Instances: 16 (8 base pairs × 2 phrasings)
# Running total after this file: 648 instances

---

## Pair[CB4-3.4-317]

**A:** OAuth PKCE audit. S256 PKCE on all authorization code flows. Device Authorization Grant (RFC 8628) supported for TV clients — no PKCE on device flow. State parameter present on all authorization code flows.

**B:**
```
Node 01 — Domain Splitter:
Input signals: authorization code flow (PKCE S256, state), device authorization
grant (no PKCE — different grant type). All signals: oauth-oidc territory.
Territories: [T-oauth-oidc]
Single territory. Device flow PKCE scoping is an arbiter-level evaluation.

Node 02 — Domain Translator:
T-oauth-oidc → oauth-oidc-normative

Node 03 — Scheme Planner:
Linear. Device flow grant type distinction passed to Node 08.

Node 04 — Domain Processor:
V10.4.6 (PKCE): required for authorization code flows. Evidence: S256 on all
  code flows. SATISFIED for authorization code flows.
  Device flow context: device authorization grant is a separate grant type.
  Scoping question passed to arbiter.
V10.2.6 (state): SATISFIED — state present on all code flows.

Node 05 — Applier:
Context: multi-grant-type server. Code flow compliance confirmed. Device flow
  PKCE scoping requires grant-type-aware evaluation.

Node 06 — Result Translator:
Tags: "V10.4.6", "PKCE S256", "authorization code flow", "device flow",
"RFC 8628", "grant type scoping", "V10.2.6 PASS".

Node 07 — Arbiter Selector:
oauth-oidc → ASVS-V10-OAuth.

Node 08 — Arbiter Unit (ASVS-V10-OAuth):
V10.2.6: PASS — state present on all authorization code flows.
V10.4.6 (code flows): PASS — S256 PKCE enforced.
V10.4.6 (device flow scoping): PASS — Device Authorization Grant (RFC 8628)
  uses device_code/user_code via verification URI polling, not redirect-based
  code delivery. The interception attack surface that PKCE addresses is not
  present in device flow. V10.4.6 does not require PKCE on grant types where
  the PKCE mechanism is not applicable.
Territory verdict: PASS.

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
        {"requirement": "V10.4.6", "verdict": "PASS",
         "note": "S256 on code flows; device flow excluded from PKCE requirement — correct scoping"}
      ],
      "territory_verdict": "PASS"
    }
  ],
  "overall_verdict": "PASS",
  "source_attribution": "OWASP ASVS 5.0.0 — CC BY-SA 4.0"
}
```

**Reason:** Correct grant-type scoping at Node 08 — V10.4.6 PKCE applies to authorization code flows, not to device authorization grant. The arbiter must evaluate the requirement in the context of the applicable grant type. Absence of PKCE on device flow is not a V10.4.6 violation. Full PASS trace showing correct scoping decision flowing through the pipeline.

**Phrasing-B:** S256 PKCE and state on all authorization code flows; RFC 8628 device flow without PKCE for TV clients. Full trace showing how the device flow scoping decision is made at Node 08.

---

## Pair[CB4-3.4-318]

**A:** Pen test result interaction with V10.2.1 requirement. Wildcard redirect URIs (*. example.com/callback) registered. Penetration test completed last month with no critical findings on OAuth flows. Team requests PASS based on pen test clearance.

**B:**
```
Node 01 — Domain Splitter:
Input signals: OAuth authorization code flow, wildcard redirect URI, pen test
clearance argument. T-oauth-oidc.

Node 02 — Domain Translator:
T-oauth-oidc → oauth-oidc-normative

Node 03 — Scheme Planner:
Linear. Pen test argument passed to Node 08.

Node 04 — Domain Processor:
V10.2.1: exact registration required. Evidence: wildcard pattern. GAP.
Pen test context noted: "no critical findings on OAuth flows." Passed to Node 08.

Node 05 — Applier:
Context: pen test recently completed. Wildcard redirect creates subdomain-
takeover and path-confusion vectors. Pen test clearance does not change
configuration evidence.

Node 06 — Result Translator:
Tags: "V10.2.1", "wildcard", "exact registration", "pen test",
"configuration requirement".

Node 07 — Arbiter Selector:
oauth-oidc → ASVS-V10-OAuth.

Node 08 — Arbiter Unit (ASVS-V10-OAuth):
V10.2.1: REPROCESS.
Argument rejected: "pen test no critical findings = PASS."
V10.2.1 is a configuration requirement, not an exploitability threshold.
A penetration test assesses exploitability within the scope and timeframe of
the engagement; it does not certify configuration compliance. "No critical
findings" means the tester did not demonstrate a specific exploit — it does
not mean the wildcard configuration satisfies exact-registration requirements.
The requirement is resolved by changing the configuration to exact registration,
not by demonstrating that no exploit was found.
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
        {"requirement": "V10.2.1", "verdict": "REPROCESS",
         "argument_rejected": "pen test clearance does not satisfy configuration requirement; no critical findings ≠ exact registration",
         "gap": "wildcard redirect URI; exact registration required"}
      ],
      "territory_verdict": "REPROCESS"
    }
  ],
  "overall_verdict": "REPROCESS",
  "source_attribution": "OWASP ASVS 5.0.0 — CC BY-SA 4.0"
}
```

**Reason:** The pen test argument is a common team escalation when a configuration gap is identified after a recent security assessment. Node 08 explicitly rejects it and documents the rejection in the formatter output. The pipeline shows how a team argument that sounds persuasive (recent pen test, no critical findings) is evaluated and rejected through the correct reasoning chain.

**Phrasing-B:** Wildcard redirect URIs, pen test last month with no critical OAuth findings. The pen test should give us credit for the wildcard configuration being safe. Full trace showing the pen test argument path.

---

## Pair[CB4-3.4-319]

**A:** JWT algorithm allowlist review. Production: RS256 only. Development: RS256 and HS256, different keys, isolated infrastructure. SAST and security review confirm production deployment only uses RS256. No cross-environment token acceptance possible.

**B:**
```
Node 01 — Domain Splitter:
Input signals: JWT token configuration, RS256 production allowlist, HS256 dev
environment, infrastructure isolation confirmed.
Territories: [T-asvs-token]
Single territory. Environment isolation detail is context for arbiter evaluation.

Node 02 — Domain Translator:
T-asvs-token → asvs-token-normative

Node 03 — Scheme Planner:
Linear. Environment isolation context passed through.

Node 04 — Domain Processor:
V9.1.1 (algorithm allowlist): production allowlist RS256 only. SATISFIED for
  production. Dev environment HS256 addition: separate deployment, isolated
  infrastructure. Divergence context noted.

Node 05 — Applier:
Context: isolated dev environment, confirmed no cross-environment token
acceptance. Production compliance is the primary assessment scope.
Infrastructure isolation reduces configuration drift risk.

Node 06 — Result Translator:
Tags: "V9.1.1", "RS256", "allowlist", "production", "dev environment",
"environment isolation", "CONCERN".

Node 07 — Arbiter Selector:
asvs-token → ASVS-V9-Token.

Node 08 — Arbiter Unit (ASVS-V9-Token):
V9.1.1 (production): PASS — RS256 only allowlist in production.
Dev environment divergence: CONCERN — HS256 in dev with different keys and
  isolated infrastructure. Production is correctly configured. Dev environment
  permissiveness creates risk of: configuration drift, implicit code assumptions
  about HS256 acceptance propagating to production. SAST and review reduce
  this risk but do not eliminate the concern about environment parity.
Territory verdict: CONCERN (production PASS, environment divergence CONCERN —
  overall territory assessment elevated to CONCERN).

Node 09 — Formatter:
{
  "flow": "security-verification-3.4",
  "territories": ["asvs-token"],
  "findings": [
    {
      "territory": "asvs-token",
      "arbiter": "ASVS-V9-Token",
      "sub_findings": [
        {"requirement": "V9.1.1", "verdict": "CONCERN",
         "note": "production RS256-only PASS; dev environment HS256 divergence is a configuration management concern",
         "gap": "dev environment more permissive than production — environment parity recommended"}
      ],
      "territory_verdict": "CONCERN"
    }
  ],
  "overall_verdict": "CONCERN",
  "source_attribution": "OWASP ASVS 5.0.0 — CC BY-SA 4.0"
}
```

**Reason:** Production is correctly configured (V9.1.1 PASS). The CONCERN is for environment divergence — not a production violation but an architectural risk. The full trace shows how environment context is passed through from Node 04 to Node 08, and how Node 08 produces a nuanced verdict (production PASS, overall CONCERN for divergence) rather than a binary PASS/REPROCESS.

**Phrasing-B:** Production JWT service: RS256 only allowlist. Dev: RS256 and HS256, isolated infrastructure, different keys, SAST confirms no cross-environment exposure. Full trace for the production PASS + environment divergence CONCERN finding.

---

## Pair[CB4-3.4-320]

**A:** Session management review: JWT sessions with 5-minute expiry, cookie cleared on logout, no server-side revocation. Separate OAuth refresh tokens with 7-day expiry, revoked at logout via revocation endpoint call. State parameter and PKCE S256 on all authorization code flows.

**B:**
```
Node 01 — Domain Splitter:
Input signals: JWT session (5-min expiry, no server revocation), OAuth refresh
tokens (7-day, revoked at logout), OAuth code flow (PKCE S256, state).
Territories: [T-asvs-session, T-oauth-oidc]
Session territory for V7, oauth-oidc territory for V10 (refresh revocation +
PKCE + state).

Node 02 — Domain Translator:
T-asvs-session → asvs-session-normative
T-oauth-oidc → oauth-oidc-normative

Node 03 — Scheme Planner:
Parallel (two territories).

Node 04 — Domain Processor (parallel):
asvs-session: V7.3.1 — logout without server revocation. 5-min expiry mitigates.
  GAP with significant mitigation.
oauth-oidc: V10.3 — refresh tokens revoked at logout. SATISFIED.
  V10.4.6 — S256 PKCE. SATISFIED. V10.2.6 — state present. SATISFIED.

Node 05 — Applier:
5-min session: significant mitigation, CONCERN classification appropriate.
OAuth: all controls satisfied, PASS.

Node 06 — Result Translator:
asvs-session tags: "V7.3.1", "5-minute expiry", "CONCERN", "mitigation".
oauth-oidc tags: "V10.3 PASS", "V10.4.6 PASS", "V10.2.6 PASS".

Node 07 — Arbiter Selector:
asvs-session → ASVS-V7-Session
oauth-oidc → ASVS-V10-OAuth

Node 08 — Arbiter Units:
ASVS-V7-Session: V7.3.1 CONCERN — 5-min expiry is significant mitigation;
  server-side revocation absent but risk substantially reduced.
ASVS-V10-OAuth: V10.3 PASS. V10.4.6 PASS. V10.2.6 PASS.
  Territory verdict: PASS.

Node 09 — Formatter:
{
  "flow": "security-verification-3.4",
  "territories": ["asvs-session", "oauth-oidc"],
  "findings": [
    {
      "territory": "asvs-session",
      "arbiter": "ASVS-V7-Session",
      "sub_findings": [
        {"requirement": "V7.3.1", "verdict": "CONCERN",
         "note": "5-minute expiry is significant mitigation; server-side revocation absent"}
      ],
      "territory_verdict": "CONCERN"
    },
    {
      "territory": "oauth-oidc",
      "arbiter": "ASVS-V10-OAuth",
      "sub_findings": [
        {"requirement": "V10.3", "verdict": "PASS"},
        {"requirement": "V10.4.6", "verdict": "PASS"},
        {"requirement": "V10.2.6", "verdict": "PASS"}
      ],
      "territory_verdict": "PASS"
    }
  ],
  "overall_verdict": "CONCERN",
  "source_attribution": "OWASP ASVS 5.0.0 — CC BY-SA 4.0"
}
```

**Reason:** Two territories: asvs-session CONCERN (5-min expiry mitigation) and oauth-oidc PASS (refresh revocation, PKCE, state all correct). Overall CONCERN because CONCERN overrides PASS in the overall verdict. This trace shows that partial correctness produces CONCERN, not PASS — the model must not report overall PASS when any territory has CONCERN or higher.

**Phrasing-B:** 5-minute JWT sessions (cookie cleared, no server revocation), 7-day OAuth refresh tokens revoked at logout, S256 PKCE and state on code flows. Full trace showing CONCERN+PASS combining to overall CONCERN.

---

## Pair[CB4-3.4-321]

**A:** Comprehensive security review where multiple controls are correctly implemented. Password hashing: bcrypt cost 12. Code review: Security Champion with OWASP checklist, plus SAST on every PR. GitHub Actions secrets for credentials. JWT role claims in access tokens. All authorization code OAuth flows use PKCE S256 with state. Device flow without PKCE for constrained devices.

**B:**
```
Node 01 — Domain Splitter:
All positive signals across multiple security domains.
Territories: [T-asvs-authentication, T-ssdf-process, T-asvs-authorization,
              T-oauth-oidc]

Node 02 — Domain Translator:
Four territories → respective normative labels.

Node 03 — Scheme Planner:
Parallel (four territories, all positive evidence).

Node 04 — Domain Processor (parallel — all SATISFIED):
asvs-authentication: V6.2.5 bcrypt cost 12 ✓.
ssdf-process: PW.1 GitHub Actions secrets ✓. PW.3.2 Security Champion + OWASP checklist ✓. PW.4 SAST ✓.
asvs-authorization: V8.2.1 JWT role claims as authorization metadata ✓.
oauth-oidc: V10.4.6 S256 PKCE on code flows ✓. V10.2.6 state ✓.
  Device flow: correctly excluded from PKCE ✓.

Node 05 — Applier:
Context: well-secured implementation across all assessed domains.

Node 06 — Result Translator:
All territories: positive vocabulary.

Node 07 — Arbiter Selector:
Four arbiters selected.

Node 08 — Arbiter Units:
ASVS-V6-Auth: V6.2.5 bcrypt cost 12 PASS.
SSDF-PW: PW.1 PASS. PW.3.2 PASS. PW.4 PASS.
ASVS-V8-AuthZ: V8.2.1 PASS (JWT role claims accessible).
ASVS-V10-OAuth: V10.4.6 PASS (code flows). V10.2.6 PASS. Device flow scoping PASS.

Node 09 — Formatter:
{
  "flow": "security-verification-3.4",
  "territories": ["asvs-authentication", "ssdf-process",
                  "asvs-authorization", "oauth-oidc"],
  "findings": [
    {"territory": "asvs-authentication", "arbiter": "ASVS-V6-Auth",
     "territory_verdict": "PASS"},
    {"territory": "ssdf-process", "arbiter": "SSDF-PW",
     "territory_verdict": "PASS"},
    {"territory": "asvs-authorization", "arbiter": "ASVS-V8-AuthZ",
     "territory_verdict": "PASS"},
    {"territory": "oauth-oidc", "arbiter": "ASVS-V10-OAuth",
     "territory_verdict": "PASS"}
  ],
  "overall_verdict": "PASS",
  "source_attribution": [
    "OWASP ASVS 5.0.0 — CC BY-SA 4.0",
    "NIST SP 800-218 SSDF v1.1 — public domain"
  ]
}
```

**Reason:** Four-territory all-PASS trace covering the new accepted patterns from ext11-ext12: bcrypt cost 12, GitHub Actions secrets, Security Champion with OWASP checklist, JWT role claims for V8.2.1, correct device flow PKCE scoping. Demonstrates that each of these patterns, when correctly implemented, flows through the pipeline and produces PASS.

**Phrasing-B:** bcrypt cost 12, GitHub Actions secrets, Security Champion + OWASP checklist, SAST, JWT role claims for authorization transparency, S256 PKCE on code flows (device flow correctly excluded). Full 4-territory PASS trace.

---

## Pair[CB4-3.4-322]

**A:** Security review with nightly batch session revocation as the primary session management control. JWT sessions with 12-hour expiry. Nightly batch at 02:00 UTC revokes sessions older than 12 hours. On explicit logout, cookie is deleted client-side only — no server-side revocation triggered by logout event. Threat model is current (4 months). SAST running. Argon2id m=65536/t=3/p=4.

**B:**
```
Node 01 — Domain Splitter:
Input signals: session management (nightly batch revocation, no logout-triggered
server revocation, 12-hour expiry), process evidence (threat model 4 months,
SAST), authentication (Argon2id).
Territories: [T-asvs-session, T-ssdf-process, T-asvs-authentication]

Node 02 — Domain Translator:
Three territories → respective normative labels.

Node 03 — Scheme Planner:
Parallel.

Node 04 — Domain Processor (parallel):
asvs-session: V7.3.1 — nightly batch, no logout-triggered revocation, 12-hour
  expiry. GAP: server-side invalidation on logout absent.
ssdf-process: PW.2 threat model 4 months SATISFIED. PW.4 SAST SATISFIED.
asvs-authentication: V6.2.5 Argon2id SATISFIED.

Node 05 — Applier:
12-hour expiry is a moderate mitigation (between 5-min CONCERN and 7-day REPROCESS
scenarios). Post-logout window: up to 12 hours. Nightly batch does not reduce
this window on logout day. Severity: REPROCESS (12-hour post-logout window is
not a sufficient mitigation to downgrade to CONCERN).

Node 06 — Result Translator:
asvs-session tags: "V7.3.1", "nightly batch", "12-hour expiry", "REPROCESS".
ssdf-process tags: "PW.2 PASS", "PW.4 PASS".
asvs-authentication tags: "V6.2.5 PASS".

Node 07 — Arbiter Selector:
asvs-session → ASVS-V7-Session
ssdf-process → SSDF-PW
asvs-authentication → ASVS-V6-Auth

Node 08 — Arbiter Units:
ASVS-V7-Session: V7.3.1 REPROCESS.
  Nightly batch is not logout-triggered revocation. 12-hour post-logout window
  (until next batch run) is significant. Unlike the 5-minute scenario (CONCERN),
  12 hours provides a meaningful exploitation window. REPROCESS.
SSDF-PW: PW.2 PASS (4 months). PW.4 PASS (SAST). Territory PASS.
ASVS-V6-Auth: V6.2.5 PASS (Argon2id). Territory PASS.

Node 09 — Formatter:
{
  "flow": "security-verification-3.4",
  "territories": ["asvs-session", "ssdf-process", "asvs-authentication"],
  "findings": [
    {
      "territory": "asvs-session",
      "arbiter": "ASVS-V7-Session",
      "sub_findings": [
        {"requirement": "V7.3.1", "verdict": "REPROCESS",
         "gap": "nightly batch ≠ logout-triggered revocation; 12-hour post-logout window"}
      ],
      "territory_verdict": "REPROCESS"
    },
    {"territory": "ssdf-process", "arbiter": "SSDF-PW",
     "territory_verdict": "PASS"},
    {"territory": "asvs-authentication", "arbiter": "ASVS-V6-Auth",
     "territory_verdict": "PASS"}
  ],
  "overall_verdict": "REPROCESS",
  "source_attribution": "OWASP ASVS 5.0.0 — CC BY-SA 4.0"
}
```

**Reason:** The 12-hour expiry with nightly batch revocation is REPROCESS (unlike 5-minute which is CONCERN). The 12-hour post-logout exploitation window is significant. The batch revocation does not run on logout events. Node 05's context assessment correctly distinguishes the severity: 5 minutes → CONCERN; 12 hours → REPROCESS. Two PASS territories don't offset the REPROCESS.

**Phrasing-B:** 12-hour JWT sessions with nightly 02:00 AM batch revocation. Logout clears cookie only. Threat model current, SAST running, Argon2id for passwords. How does 12-hour expiry with nightly batch compare to 5-minute expiry in terms of V7.3.1 verdict?

---

## Pair[CB4-3.4-323]

**A:** Mixed credential review: database credentials from Vault (PW.1 PASS), session invalidation on logout via Redis (V7.3.1 PASS), but S256 PKCE implemented without code_verifier validation at token exchange (V10.4.6 gap).

**B:**
```
Node 01 — Domain Splitter:
Input signals: Vault credentials (PW.1 satisfied), Redis session revocation
(V7.3.1 satisfied), PKCE challenge present but verifier not validated (V10.4.6 gap).
Territories: [T-ssdf-process, T-asvs-session, T-oauth-oidc]

Node 02 — Domain Translator:
Three territories.

Node 03 — Scheme Planner:
Parallel.

Node 04 — Domain Processor (parallel):
ssdf-process: PW.1 Vault — SATISFIED.
asvs-session: V7.3.1 Redis revocation on logout — SATISFIED.
oauth-oidc: V10.4.6 — challenge present, verifier NOT validated at exchange. GAP.

Node 05 — Applier:
Two controls correctly implemented (PASS × 2). One gap: PKCE without verifier
validation provides zero PKCE protection. REPROCESS for V10.4.6 confirmed.

Node 07 — Arbiter Selector / Node 08 — Arbiter Units:
SSDF-PW: PW.1 PASS.
ASVS-V7-Session: V7.3.1 PASS (Redis server-side revocation).
ASVS-V10-OAuth: V10.4.6 REPROCESS — challenge without verifier validation is
not PKCE; authorization code exchangeable by any code holder.

Node 09 — Formatter:
{
  "flow": "security-verification-3.4",
  "territories": ["ssdf-process", "asvs-session", "oauth-oidc"],
  "findings": [
    {"territory": "ssdf-process", "arbiter": "SSDF-PW",
     "territory_verdict": "PASS"},
    {"territory": "asvs-session", "arbiter": "ASVS-V7-Session",
     "territory_verdict": "PASS"},
    {
      "territory": "oauth-oidc",
      "arbiter": "ASVS-V10-OAuth",
      "sub_findings": [
        {"requirement": "V10.4.6", "verdict": "REPROCESS",
         "gap": "code_verifier not validated at token exchange; PKCE provides no protection"}
      ],
      "territory_verdict": "REPROCESS"
    }
  ],
  "overall_verdict": "REPROCESS",
  "source_attribution": [
    "OWASP ASVS 5.0.0 — CC BY-SA 4.0",
    "NIST SP 800-218 SSDF v1.1 — public domain"
  ]
}
```

**Reason:** Two PASS territories and one REPROCESS territory. REPROCESS overrides all PASSes in the overall verdict. The trace demonstrates that getting two out of three controls right does not produce an overall PASS. Each territory is independently evaluated; the overall verdict reflects the worst finding across all territories.

**Phrasing-B:** Vault for credentials (PW.1 correct), Redis session revocation on logout (V7.3.1 correct), but PKCE code_verifier not validated at exchange. Two controls right, one gap. How does the overall verdict resolve?

---

## Pair[CB4-3.4-324]

**A:** End-to-end compliant security review. All session sub-domains correctly implemented: exact OAuth redirect URIs, S256 PKCE with verifier validated, state parameter, refresh tokens revoked at logout, server-side session revocation on logout via Redis, bcrypt cost 12 passwords, GitHub Actions secrets, Security Champion review, SAST, threat model 3 months. JWT access tokens include role claims accessible to users.

**B:**
```
Node 01 — Domain Splitter:
All positive signals.
Territories: [T-oauth-oidc, T-asvs-session, T-asvs-authentication,
              T-ssdf-process, T-asvs-authorization]

Node 02–03: Five territories, parallel plan.

Node 04 — Domain Processor (all SATISFIED):
oauth-oidc: V10.2.1 exact URIs ✓. V10.4.6 S256+verifier ✓. V10.2.6 state ✓.
  V10.3 refresh revocation ✓.
asvs-session: V7.3.1 Redis revocation ✓.
asvs-authentication: V6.2.5 bcrypt cost 12 ✓.
ssdf-process: PW.1 GitHub Actions secrets ✓. PW.3.2 Security Champion + OWASP ✓.
  PW.4 SAST ✓. PW.2 threat model 3 months ✓.
asvs-authorization: V8.2.1 JWT role claims ✓.

Node 08 — All PASS.

Node 09 — Formatter:
{
  "flow": "security-verification-3.4",
  "territories": ["oauth-oidc", "asvs-session", "asvs-authentication",
                  "ssdf-process", "asvs-authorization"],
  "findings": [
    {"territory": "oauth-oidc", "arbiter": "ASVS-V10-OAuth",
     "territory_verdict": "PASS"},
    {"territory": "asvs-session", "arbiter": "ASVS-V7-Session",
     "territory_verdict": "PASS"},
    {"territory": "asvs-authentication", "arbiter": "ASVS-V6-Auth",
     "territory_verdict": "PASS"},
    {"territory": "ssdf-process", "arbiter": "SSDF-PW",
     "territory_verdict": "PASS"},
    {"territory": "asvs-authorization", "arbiter": "ASVS-V8-AuthZ",
     "territory_verdict": "PASS"}
  ],
  "overall_verdict": "PASS",
  "source_attribution": [
    "OWASP ASVS 5.0.0 — CC BY-SA 4.0",
    "NIST SP 800-218 SSDF v1.1 — public domain"
  ]
}
```

**Reason:** Comprehensive all-PASS trace covering all controls from ext12. bcrypt cost 12 (compliant), GitHub Actions secrets (compliant), JWT role claims (V8.2.1 satisfied), Security Champion with checklist (PW.3.2 satisfied). All session management, OAuth, authentication, process, and authorization controls verified.

**Phrasing-B:** All ext12 patterns implemented correctly: bcrypt 12, GitHub secrets, Security Champion, SAST, Redis session revocation, full V10 OAuth compliance, JWT role claims, threat model 3 months. Complete 5-territory PASS trace.

---

## File Summary

| Pair ID | Description | Stratum | Territories | Verdict |
|---------|-------------|---------|-------------|---------|
| CB4-3.4-317 | Device flow PKCE scoping decision — V10.4.6 correct exclusion PASS | C | oauth-oidc | PASS |
| CB4-3.4-318 | V10.2.1 wildcard + pen test argument — REPROCESS with rejection trace | C | oauth-oidc | REPROCESS |
| CB4-3.4-319 | V9.1.1 RS256 prod PASS + dev divergence CONCERN — full trace | C | asvs-token | CONCERN |
| CB4-3.4-320 | V7.3.1 5-min CONCERN + oauth-oidc all PASS → overall CONCERN | C | asvs-session, oauth-oidc | CONCERN |
| CB4-3.4-321 | 4-territory PASS: bcrypt12+GitHub secrets+Security Champion+device flow | C | asvs-authentication, ssdf-process, asvs-authorization, oauth-oidc | PASS |
| CB4-3.4-322 | V7.3.1 nightly batch 12h REPROCESS + PW.2+V6.2.5 PASS — 3-territory | C | asvs-session, ssdf-process, asvs-authentication | REPROCESS |
| CB4-3.4-323 | PW.1 PASS + V7.3.1 PASS + V10.4.6 REPROCESS — two PASSes don't offset | C | ssdf-process, asvs-session, oauth-oidc | REPROCESS |
| CB4-3.4-324 | All-ext12-pattern 5-territory comprehensive PASS | C | oauth-oidc, asvs-session, asvs-authentication, ssdf-process, asvs-authorization | PASS |
