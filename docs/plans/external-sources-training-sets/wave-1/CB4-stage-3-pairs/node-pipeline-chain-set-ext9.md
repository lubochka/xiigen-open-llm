# CB4 Stage 3 — Node Pipeline Chain Set Extension 9
# Sequences CB4-3.4-395 through CB4-3.4-402
# Stratum: C — full 9-node pipeline traces with intermediate outputs
# Instances: 16 (8 base pairs × 2 phrasings)
# Running total after this file: 804 instances

---

## Pair[CB4-3.4-395]

**A:** Token introspection endpoint review. Authorization server exposes `POST /oauth/introspect`. The endpoint checks that the HTTP request includes an Authorization header with a valid Bearer token issued to an introspection client. Callers who do not present a valid Bearer token receive a 401 response. The endpoint returns active/inactive status plus `sub`, `scope`, `exp`, `client_id` for active tokens.

**B:**
```
Node 1 — Domain Splitter
Input: token introspection endpoint with Bearer token caller authentication.
Domains detected: [rfc-oauth2, oauth-oidc]
Primary domain: rfc-oauth2
Secondary: oauth-oidc
Routing: multi-domain

Node 2 — Domain Translator
Domains: rfc-oauth2, oauth-oidc
Normative mapping:
  Introspection endpoint security: RFC 7662 §2.1 (MUST require authorization)
  ASVS mapping: V10 (OAuth endpoint security)
Territories: T-rfc-oauth2, T-oauth-oidc

Node 3 — Scheme Planner
Territories: [T-rfc-oauth2, T-oauth-oidc]
Arbiters: RFC-OAuth2, ASVS-V10-OAuth
Sub-requirements:
  RFC 7662 §2.1: "MUST require some form of authorization to access this endpoint"
  Bearer token caller auth: does this satisfy the MUST?

Node 4 — Domain Processor
T-rfc-oauth2 analysis:
  RFC 7662 §2.1 requirement: the endpoint MUST require some form of authorization.
  Acceptable forms per §2.1: "client authentication as described in OAuth 2.0
    [RFC6749] or a separate OAuth 2.0 access token"
  Implementation: Bearer token required from callers ✓
    - A valid Bearer access token issued to an introspection client is required
    - Unauthenticated callers receive 401 ✓
    - This satisfies "a separate OAuth 2.0 access token" as the authorization form
  RFC 7662 §2.1: PASS

T-oauth-oidc analysis:
  Introspection endpoint caller authentication: satisfies OAuth endpoint security ✓
  Response content (sub, scope, exp, client_id): standard introspection fields ✓

Node 5 — Applier
RFC 7662 §2.1: PASS — Bearer token caller auth satisfies MUST authorization requirement
V10 OAuth endpoint security: PASS

Node 6 — Result Translator
T-rfc-oauth2: PASS
T-oauth-oidc: PASS
Overall: PASS

Node 7 — Arbiter Selector
All PASS — confirmatory

Node 8 — Arbiter Units (confirmatory)
RFC-OAuth2: RFC 7662 §2.1 PASS — Bearer token caller authentication correctly implements
  the "separate OAuth 2.0 access token" authorization form specified in §2.1.
ASVS-V10-OAuth: PASS
Overall verdict: PASS

Node 9 — Formatter
{
  "sequence_id": "CB4-3.4-395",
  "overall_verdict": "PASS",
  "territories": [
    {"territory": "T-rfc-oauth2", "verdict": "PASS", "arbiter": "RFC-OAuth2"},
    {"territory": "T-oauth-oidc", "verdict": "PASS", "arbiter": "ASVS-V10-OAuth"}
  ]
}
```

**Reason:** RFC 7662 §2.1 requires "some form of authorization" — explicitly including "a separate OAuth 2.0 access token" as an acceptable form. Requiring a valid Bearer access token from callers satisfies this MUST. Unauthenticated callers receive 401. PASS.

**Phrasing-B:** Introspection endpoint requires a valid Bearer token from callers to access it. No valid Bearer token = 401. Is requiring a Bearer access token for introspection callers sufficient to satisfy RFC 7662 §2.1?

---

## Pair[CB4-3.4-396]

**A:** JWT key rotation pipeline trace. RS256 signing key pair. Current key was generated two years ago and has never been rotated. The `kid` claim is present in all issued JWTs and the JWKS endpoint serves the current public key. No rotation schedule exists. The team's argument: "The key hasn't been compromised. We'll rotate it if we detect a compromise. RFC 7517 doesn't mandate rotation frequency."

**B:**
```
Node 1 — Domain Splitter
Input: RS256 signing key never rotated in two years; no rotation schedule; team argues reactive approach.
Domains detected: [asvs-token, rfc-jwt]
Primary domain: asvs-token
Secondary: rfc-jwt
Routing: multi-domain

Node 2 — Domain Translator
Normative mapping:
  Signing key management: ASVS V9.2 (key lifecycle management)
  RFC 7517 / 7518: key management practices
Territories: T-asvs-token, T-rfc-jwt

Node 3 — Scheme Planner
Territories: [T-asvs-token, T-rfc-jwt]
Arbiters: ASVS-V9-Token, RFC-JWT
Sub-requirements:
  V9.2: key rotation frequency — CONCERN territory
  Reactive-only rotation argument: evaluate

Node 4 — Domain Processor
T-asvs-token analysis:
  Kid claim present ✓ — key versioning infrastructure exists
  JWKS endpoint serves current key ✓ — validators can look up key
  Rotation schedule: ABSENT ✗
  Time since last rotation: 2 years
  Reactive rotation ("if we detect compromise"):
    - Key compromise may not be detected promptly or at all
    - Two years without rotation means a compromised key provides two years of
      forgeability window for all issued tokens
    - ASVS V9 recommends proactive rotation as part of key lifecycle management
    - RFC 7517 does not mandate frequency, but ASVS best practice expects rotation
  RFC 7517: does not specify mandatory rotation frequency — team is correct
  ASVS V9.2: CONCERN — no proactive rotation; 2-year key age; reactive-only

T-rfc-jwt:
  RFC 7517 does not mandate rotation frequency ✓ — no MUST violation
  RFC-JWT: PASS (no normative requirement violated)

Node 5 — Applier
V9.2 → CONCERN
  Evidence: no rotation in 2 years; no schedule; reactive-only approach
RFC 7517: PASS (no mandatory frequency specified)

Node 6 — Result Translator
T-asvs-token: CONCERN
T-rfc-jwt: PASS
Overall: CONCERN (CONCERN overrides PASS)

Node 7 — Arbiter Selector
CONCERN → ASVS-V9-Token for review

Node 8 — Arbiter Unit (ASVS-V9-Token)
V9.2: CONCERN
Team argument acknowledged: RFC 7517 does not mandate rotation frequency.
RFC-JWT verdict: PASS — no normative RFC violation.

ASVS V9 key management CONCERN:
(1) A 2-year-old signing key with no rotation schedule creates an ever-growing
    compromise window. If compromised, all tokens from 2 years of operations
    are retroactively forgeable.
(2) Reactive rotation ("detect compromise, then rotate") may be too late —
    key compromise is often not immediately detectable.
(3) The kid infrastructure is correctly in place; this enables rotation when
    implemented. The gap is the absence of a proactive schedule.
(4) CONCERN not REPROCESS: no MUST is violated; this is a hygiene and
    risk-management gap.

Recommendation: annual or event-triggered rotation schedule; graceful transition
using kid versioning (infrastructure already present).
Overall verdict: CONCERN

Node 9 — Formatter
{
  "sequence_id": "CB4-3.4-396",
  "overall_verdict": "CONCERN",
  "territories": [
    {
      "territory": "T-asvs-token",
      "verdict": "CONCERN",
      "arbiter": "ASVS-V9-Token",
      "findings": [
        {
          "requirement": "V9.2",
          "verdict": "CONCERN",
          "evidence": "RS256 signing key not rotated in 2 years; no rotation schedule; reactive-only approach",
          "argument_acknowledged": "RFC 7517 does not mandate frequency; gap is ASVS hygiene practice"
        }
      ]
    },
    {"territory": "T-rfc-jwt", "verdict": "PASS", "arbiter": "RFC-JWT"}
  ]
}
```

**Reason:** RFC 7517 truly does not mandate rotation frequency — the team is factually correct. RFC-JWT is PASS. However, ASVS V9 key management best practice expects proactive rotation with a schedule. Two years without rotation creates a large compromise window. CONCERN because no MUST is violated; the gap is hygiene and risk management. RFC-JWT PASS + V9.2 CONCERN → overall CONCERN.

**Phrasing-B:** RS256 key unchanged for 2 years. kid in JWTs, JWKS endpoint correct. We rotate if we detect compromise. RFC 7517 has no frequency requirement. Is reactive-only rotation a CONCERN or a REPROCESS, and does RFC 7517 having no frequency mandate affect the finding?

---

## Pair[CB4-3.4-397]

**A:** Open introspection endpoint pipeline trace. Authorization server exposes `POST /oauth/introspect` with no authentication requirement. Any party can POST a token string and receive the introspection response. The team argues: "Our introspection endpoint is on an internal network — it's not exposed to the public internet. RFC 7662 §2.1 says 'some form of authorization.' Network-level restriction is a form of authorization."

**B:**
```
Node 1 — Domain Splitter
Input: open introspection endpoint; internal-network-only; team argues network restriction = authorization.
Domains detected: [rfc-oauth2, oauth-oidc]
Primary domain: rfc-oauth2
Routing: multi-domain

Node 2 — Domain Translator
Normative mapping:
  RFC 7662 §2.1: introspection endpoint MUST require authorization
  Network restriction as authorization: evaluate
Territories: T-rfc-oauth2, T-oauth-oidc

Node 3 — Scheme Planner
Territories: [T-rfc-oauth2, T-oauth-oidc]
Arbiters: RFC-OAuth2, ASVS-V10-OAuth
Key question: does network-level restriction satisfy RFC 7662 §2.1 MUST?

Node 4 — Domain Processor
T-rfc-oauth2 analysis:
  RFC 7662 §2.1: "the endpoint MUST require some form of authorization to
    access this endpoint, such as client authentication as described in OAuth 2.0
    [RFC6749] or a separate OAuth 2.0 access token."
  Team argument: internal-network restriction = "some form of authorization"
  Evaluation:
    RFC 7662 §2.1 explicitly provides two examples: (a) client authentication
    per RFC 6749 and (b) a separate OAuth 2.0 access token. Both examples
    are application-layer authentication mechanisms, not network controls.
    Network restriction (IP allow-listing, VPN access) is a perimeter control —
    it restricts who can reach the endpoint at the network layer. However:
    - Any internal network principal (all employees, contractors, internal services)
      can call the endpoint without any identity verification
    - RFC 7662 §2.1's "authorization" refers to identity-based access control
      at the application layer, not network-layer IP restriction
    - Network restriction alone does not verify which caller is requesting
      introspection — an internal attacker or compromised internal service
      can perform token scanning without any authentication
  Argument rejected: network restriction ≠ RFC 7662 §2.1 authorization

Node 5 — Applier
RFC 7662 §2.1 → REPROCESS
  Evidence: no application-layer authentication; network restriction is perimeter,
    not application-layer identity verification

Node 6 — Result Translator
T-rfc-oauth2: REPROCESS
  Argument rejected: network restriction ≠ application-layer authorization per §2.1

Node 7 — Arbiter Selector
REPROCESS → RFC-OAuth2 + ASVS-V10-OAuth

Node 8 — Arbiter Unit (RFC-OAuth2)
RFC 7662 §2.1: REPROCESS
Argument rejected: "internal network restriction is authorization."

RFC 7662 §2.1 requires authorization at the application layer:
(1) The examples given ("client authentication" per RFC 6749 or "a separate
    OAuth 2.0 access token") are both application-layer identity verification
    mechanisms.
(2) Network restriction limits who can reach the endpoint by IP/network segment
    but does not verify which specific client is calling or authenticate the
    caller's identity.
(3) Any principal on the internal network — employees, service accounts,
    potentially compromised hosts — can perform token scanning without any
    identity check.
(4) RFC 7662 §2.1 specifically warns about "token scanning attacks" as the
    motivation for the authorization requirement. Network restriction reduces
    but does not prevent scanning by internal actors.

Evidence gap: no application-layer authorization on introspection endpoint.
Network restriction does not satisfy RFC 7662 §2.1 MUST.
Overall verdict: REPROCESS

Node 9 — Formatter
{
  "sequence_id": "CB4-3.4-397",
  "overall_verdict": "REPROCESS",
  "territories": [
    {
      "territory": "T-rfc-oauth2",
      "verdict": "REPROCESS",
      "arbiter": "RFC-OAuth2",
      "findings": [
        {
          "requirement": "RFC7662-§2.1",
          "verdict": "REPROCESS",
          "evidence": "No application-layer authentication; internal-network restriction only",
          "argument_rejected": "Network perimeter restriction ≠ application-layer authorization per §2.1"
        }
      ]
    },
    {"territory": "T-oauth-oidc", "verdict": "REPROCESS", "arbiter": "ASVS-V10-OAuth"}
  ]
}
```

**Reason:** Network restriction is a perimeter control, not application-layer authorization. RFC 7662 §2.1's examples (client authentication per RFC 6749, OAuth access token) are identity-based mechanisms. Network restriction does not verify which caller is requesting introspection — any internal actor can perform token scanning. This is the same pattern as "IP whitelist ≠ application-layer deny-by-default" — perimeter controls do not substitute for application-layer identity verification.

**Phrasing-B:** Introspection endpoint is on an internal network — not reachable from the public internet. No application-layer auth. RFC 7662 §2.1 says "some form of authorization" — does internal network restriction count as authorization for the introspection endpoint?

---

## Pair[CB4-3.4-398]

**A:** MFA enforcement review. Application handles financial transactions. MFA is available via TOTP. MFA is enforced for: login from a new device; transactions over $1,000. Regular logins from known devices do not require MFA. The application is assessed at ASVS Level 2. The team states: "We enforce MFA for high-risk operations (new devices + large transactions). That meets risk-based MFA requirements."

**B:**
```
Node 1 — Domain Splitter
Input: ASVS Level 2 financial application; MFA enforced for new devices and large transactions;
  regular known-device logins no MFA.
Domains detected: [asvs-authentication]
Primary domain: asvs-authentication
Routing: single-domain

Node 2 — Domain Translator
Domain: asvs-authentication
Normative mapping:
  MFA requirements: ASVS V6.3 (Level 2 context)
Territory: T-asvs-authentication

Node 3 — Scheme Planner
Territories: [T-asvs-authentication]
Arbiter: ASVS-V6-Auth
Sub-requirements:
  V6.3 at Level 2: what MFA enforcement is required for a financial application?

Node 4 — Domain Processor
T-asvs-authentication analysis:
  Application context: financial transactions, ASVS Level 2
  MFA enforced for:
    - New device logins ✓ (risk-based trigger)
    - Transactions > $1,000 ✓ (risk-based trigger for high-value operations)
  MFA not enforced for:
    - Regular logins from known devices ✗ (authentication without second factor)
  ASVS Level 2 V6.3 assessment:
    - Level 2 requires MFA for all authentication in sensitive/financial contexts
    - Risk-based MFA (new device + high-value transaction) partially satisfies
      the intent but leaves regular known-device logins as single-factor
    - A compromised credential (password) from a known device would allow
      full account access without MFA
    - For a financial application at Level 2, single-factor authentication
      for any login path is a gap
  Finding: CONCERN (Level 2 financial) — MFA partially enforced; risk-based
    triggers miss the baseline MFA requirement for all logins
  Note: CONCERN rather than REPROCESS because MFA is enforced for the
    highest-risk operations; the gap is the known-device login exemption

Node 5 — Applier
V6.3 → CONCERN
  Evidence: MFA for new devices and large transactions, not for all logins
  Finding: known-device login exemption leaves single-factor path at Level 2

Node 6 — Result Translator
T-asvs-authentication: CONCERN
  Finding: Level 2 financial application with single-factor path for known devices

Node 7 — Arbiter Selector
CONCERN → ASVS-V6-Auth

Node 8 — Arbiter Unit (ASVS-V6-Auth)
V6.3 (MFA at Level 2): CONCERN
Risk-based MFA (new devices, large transactions) is a positive security practice.
The gap is that regular known-device logins remain single-factor.

For a financial application at ASVS Level 2:
(1) Credential theft (phishing, password breach) grants full account access
    from any "known device" without MFA, since the attacker either stole the
    cookie/session alongside the password, or registered their device as "known."
(2) Risk-based MFA reduces friction while providing some protection for
    high-risk events, but ASVS Level 2 expects MFA to be a baseline requirement
    for all authentication, not just triggered by specific risk signals.
(3) CONCERN not REPROCESS: high-risk operations are protected; the gap is
    the known-device baseline authentication path.

Remediation: enforce MFA for all logins (risk-based additional checks can be
layered on top, but MFA should not be the only exception trigger).
Overall verdict: CONCERN

Node 9 — Formatter
{
  "sequence_id": "CB4-3.4-398",
  "overall_verdict": "CONCERN",
  "territories": [
    {
      "territory": "T-asvs-authentication",
      "verdict": "CONCERN",
      "arbiter": "ASVS-V6-Auth",
      "findings": [
        {
          "requirement": "V6.3",
          "verdict": "CONCERN",
          "evidence": "Level 2 financial app; MFA for new devices + $1K+ transactions; known-device logins single-factor",
          "finding": "Single-factor path remains for known-device logins; Level 2 expects MFA baseline"
        }
      ]
    }
  ]
}
```

**Reason:** Risk-based MFA for high-risk events is better than fully optional MFA. However, for a Level 2 financial application, ASVS expects MFA at the login baseline — risk signals can add additional checks but should not be the only MFA trigger. A compromised credential from a known device bypasses MFA entirely. CONCERN because high-risk operations are protected; the gap is the known-device login exemption.

**Phrasing-B:** Financial app, ASVS Level 2. MFA on new device logins and transactions over $1K. Known-device logins are single-factor. Is risk-based MFA enforcement sufficient for Level 2, or does the known-device single-factor path create a CONCERN?

---

## Pair[CB4-3.4-399]

**A:** Refresh token security review. OAuth application. Access tokens: 15-minute expiry. Refresh tokens: 7-day expiry, single-use with rotation — each refresh operation issues a new refresh token and immediately invalidates the previous one. On logout: access and refresh tokens both revoked. If a stolen refresh token is used, the legitimate client's next refresh attempt will fail (the token has been consumed), enabling detection.

**B:**
```
Node 1 — Domain Splitter
Input: refresh token rotation (single-use, immediate revocation of prior token); 7-day expiry; revocation on logout.
Domains detected: [oauth-oidc, asvs-session]
Primary domain: oauth-oidc
Routing: multi-domain

Node 2 — Domain Translator
Normative mapping:
  Token revocation: ASVS V10.3
  Refresh token security: ASVS V10.3 + defense-in-depth
  Session: V7.3.1 (logout)
Territories: T-oauth-oidc, T-asvs-session

Node 3 — Scheme Planner
Territories: [T-oauth-oidc, T-asvs-session]
Arbiters: ASVS-V10-OAuth, ASVS-V7-Session
Plan: evaluate refresh token rotation against V10.3 and session requirements

Node 4 — Domain Processor
T-oauth-oidc analysis:
  V10.3 (token revocation on logout): access + refresh both revoked ✓ — PASS
  Refresh token rotation (single-use):
    - Each refresh: new token issued + old token immediately invalidated ✓
    - Theft detection: stolen token use invalidates it → legitimate client's
      next refresh fails → theft detectable ✓
    - Defense-in-depth: rotation is a security improvement beyond V10.3 baseline
    - 7-day expiry: reasonable window with single-use rotation
  V10.3: PASS (revocation + rotation exceeds baseline)

T-asvs-session analysis:
  V7.3.1 (logout invalidation): PASS ✓

Node 5 — Applier
All controls: PASS — rotation exceeds V10.3 baseline

Node 6 — Result Translator
T-oauth-oidc: PASS
T-asvs-session: PASS
Overall: PASS

Node 7 — Arbiter Selector
All PASS — confirmatory

Node 8 — Arbiter Units (confirmatory)
ASVS-V10-OAuth: V10.3 PASS — refresh token rotation is the recommended pattern
  for refresh token security; single-use rotation with immediate invalidation
  satisfies and exceeds V10.3 requirements.
ASVS-V7-Session: V7.3.1 PASS
Overall verdict: PASS

Node 9 — Formatter
{
  "sequence_id": "CB4-3.4-399",
  "overall_verdict": "PASS",
  "territories": [
    {"territory": "T-oauth-oidc", "verdict": "PASS", "arbiter": "ASVS-V10-OAuth"},
    {"territory": "T-asvs-session", "verdict": "PASS", "arbiter": "ASVS-V7-Session"}
  ]
}
```

**Reason:** Single-use refresh token rotation satisfies and exceeds V10.3 baseline. Logout revocation of both tokens satisfies V10.3 and V7.3.1. The rotation pattern provides theft detection capability: a stolen token's use consumes it, causing the legitimate client's next refresh to fail. PASS on both territories.

**Phrasing-B:** Single-use refresh token rotation — new token issued, old invalidated on each refresh. 7-day expiry, both tokens revoked on logout. Theft detection via rotation invalidation. Do single-use rotation + logout revocation satisfy V10.3 and V7.3.1?

---

## Pair[CB4-3.4-400]

**A:** nbf claim edge case review. JWT service issues tokens with `nbf` set 30 minutes in the past: `nbf = iat - 1800` (1800 seconds = 30 minutes). The team explains: "We have a distributed system where some services may be slow to receive configuration updates. A 30-minute skew ensures tokens are accepted by services that are slightly behind." Token expiry is 60 minutes from issuance.

**B:**
```
Node 1 — Domain Splitter
Input: nbf = iat - 30 minutes; distributed system; 60-minute token expiry.
Domains detected: [rfc-jwt, asvs-token]
Primary domain: rfc-jwt
Routing: multi-domain

Node 2 — Domain Translator
Normative mapping:
  nbf claim: RFC 7519 §4.1.5 (not-before claim)
  Token security: ASVS V9.1
Territories: T-rfc-jwt, T-asvs-token

Node 3 — Scheme Planner
Territories: [T-rfc-jwt, T-asvs-token]
Arbiters: RFC-JWT, ASVS-V9-Token
Key question: is a 30-minute backward nbf offset acceptable?

Node 4 — Domain Processor
T-rfc-jwt + T-asvs-token analysis:
  nbf = iat - 1800 (30 minutes before issuance):
    The token is valid from 30 minutes before it was issued.
    Token expiry: iat + 3600 (60 minutes after issuance).
    Effective validity window: from (iat - 30 min) to (iat + 60 min) = 90 minutes total.
  Security impact of 30-minute backward nbf:
    (1) A token issued at T is valid from T-30 minutes. If a token is issued at
        10:00 AM, it is valid starting at 9:30 AM.
    (2) This effectively extends the token's backward validity by 30 minutes.
    (3) If a token is intercepted immediately after issuance, the attacker has
        up to 60 + 30 = 90 minutes of validity (until exp).
    (4) More critically: a token replayed at T-30 would be accepted. This creates
        a meaningful backward replay window.
  Distinction from 5-second skew:
    - 5-second skew: clock synchronization tolerance; negligible security impact
    - 30-minute skew: extends the token's effective validity by 50% (60min → 90min);
      creates a meaningful replay window not explained by clock drift
  "Slow configuration updates" argument:
    - Clock skew (seconds to low minutes) is the accepted use of backward nbf
    - Slow configuration propagation is a deployment/architecture concern, not
      a clock skew scenario — a 30-minute window is not a clock synchronization need
  Finding: CONCERN — 30-minute backward nbf is disproportionate for clock skew;
    extends effective validity window meaningfully

Node 5 — Applier
RFC 7519 §4.1.5: CONCERN — 30-minute backward nbf is beyond clock skew tolerance
V9.1: CONCERN — effective token validity extended to 90 minutes without justification

Node 6 — Result Translator
T-rfc-jwt: CONCERN
T-asvs-token: CONCERN
Overall: CONCERN

Node 7 — Arbiter Selector
CONCERN → RFC-JWT + ASVS-V9-Token

Node 8 — Arbiter Units
RFC-JWT: CONCERN
  nbf = iat - 30 minutes creates a 30-minute backward validity window.
  RFC 7519 §4.1.5 does not explicitly prohibit large backward offsets, but the
  purpose of nbf is to prevent early use, not to extend backward validity.
  A 30-minute backward offset:
  (1) Extends effective token lifetime to 90 minutes (from iat - 30 to exp).
  (2) Creates a replay window: tokens are valid for 30 minutes before they
      appear to have been issued.
  (3) Is not justified by clock skew — 30 minutes is not a distributed system
      clock drift scenario; it is a deployment timing workaround.
  Finding: CONCERN — no RFC MUST violated but the offset is beyond acceptable
  clock skew practice and creates a meaningful security trade-off.

ASVS-V9-Token: CONCERN — same reasoning.
Overall verdict: CONCERN

Node 9 — Formatter
{
  "sequence_id": "CB4-3.4-400",
  "overall_verdict": "CONCERN",
  "territories": [
    {
      "territory": "T-rfc-jwt",
      "verdict": "CONCERN",
      "arbiter": "RFC-JWT",
      "findings": [
        {
          "requirement": "RFC7519-§4.1.5-nbf",
          "verdict": "CONCERN",
          "evidence": "nbf = iat - 30 minutes; effective validity extended to 90 minutes; not clock-skew justified",
          "note": "Distinguish from 5-second skew (PASS): 30 minutes creates meaningful replay window"
        }
      ]
    },
    {"territory": "T-asvs-token", "verdict": "CONCERN", "arbiter": "ASVS-V9-Token"}
  ]
}
```

**Reason:** The 5-second backward nbf offset is PASS (clock synchronization tolerance). A 30-minute backward offset is CONCERN: it extends effective token validity by 50% and creates a meaningful replay window. The "slow configuration propagation" rationale is not a clock-skew scenario — 30 minutes of clock drift is implausible; this is a deployment architecture trade-off being solved with a security control. CONCERN because no RFC MUST is violated, but the extension is disproportionate.

**Phrasing-B:** JWT nbf is set 30 minutes before issuance (iat - 1800s) because some services are slow to receive config updates. Token expiry is 60 minutes. Is a 30-minute backward nbf offset acceptable like a 5-second clock skew, or does the magnitude create a CONCERN?

---

## Pair[CB4-3.4-401]

**A:** PKCE confidential client combined trace. Authorization server enforces PKCE for all public clients. Confidential clients (web server-side apps with registered client_secret) are exempt from PKCE — no code_challenge required. The client_secret is stored securely (Vault). Exact redirect URIs. State parameter validated. The team states: "Confidential clients authenticate with client_secret at token exchange — that's equivalent to PKCE protection because the attacker can't exchange a stolen code without the secret."

**B:**
```
Node 1 — Domain Splitter
Input: PKCE for public clients; confidential clients exempt; Vault for client_secret; exact URIs; state validated.
Domains detected: [oauth-oidc, rfc-oauth2]
Primary domain: oauth-oidc
Routing: multi-domain

Node 2 — Domain Translator
Normative mapping:
  V10.2.1: exact redirect URIs → T-oauth-oidc (PASS)
  V10.4.6: PKCE for public clients; confidential client exemption
  V10.2.6: state parameter
  RFC 7636: PKCE requirements
Territories: T-oauth-oidc, T-rfc-oauth2

Node 3 — Scheme Planner
Key question: confidential client PKCE exemption — PASS or CONCERN?

Node 4 — Domain Processor
T-oauth-oidc analysis:
  V10.2.1: exact redirect URIs ✓ — PASS
  V10.2.6: state validated ✓ — PASS
  V10.4.6 — public clients: PKCE S256 enforced ✓ — PASS
  V10.4.6 — confidential clients: PKCE not enforced
    Team argument: client_secret at token exchange prevents code misuse
    Evaluation:
      - An intercepted authorization code cannot be exchanged by an attacker
        who does not know the client_secret ✓ — meaningful mitigation
      - However: (a) V10.4.6 does not explicitly exclude confidential clients
        in ASVS 5.0.0; (b) the protection is not equivalent — PKCE provides
        cryptographic proof of possession of the original random value, while
        client_secret authenticates the client identity
      - In practice, a confidential client with a securely stored client_secret
        and exact redirect URIs has meaningful protection against code interception
      - The residual risk is lower than public clients but not zero
    Finding: CONCERN — meaningful mitigation from client_secret; not equivalent
      to PKCE but substantially reduces code interception risk

T-oauth-oidc verdict: CONCERN

Node 5 — Applier
V10.2.1, V10.2.6, V10.4.6 (public): PASS
V10.4.6 (confidential client exemption): CONCERN

Node 6 — Result Translator
T-oauth-oidc: CONCERN (one CONCERN in territory)
T-rfc-oauth2: PASS (RFC 7636 primarily targets public clients)
Overall: CONCERN

Node 7 — Arbiter Selector
CONCERN → ASVS-V10-OAuth

Node 8 — Arbiter Unit (ASVS-V10-OAuth)
V10.2.1: PASS; V10.2.6: PASS; V10.4.6 (public): PASS.
V10.4.6 (confidential client exemption): CONCERN.

Client_secret authentication provides meaningful mitigation for code interception
for confidential clients — attacker needs both the code and the secret.
Finding level remains CONCERN:
(1) V10.4.6 does not explicitly exclude confidential clients.
(2) The protection is meaningful but not equivalent to cryptographic PKCE.
(3) Recommendation: implement PKCE universally; document risk acceptance
    for confidential client exemption with the Vault-stored secret as mitigation.
Overall verdict: CONCERN

Node 9 — Formatter
{
  "sequence_id": "CB4-3.4-401",
  "overall_verdict": "CONCERN",
  "territories": [
    {
      "territory": "T-oauth-oidc",
      "verdict": "CONCERN",
      "arbiter": "ASVS-V10-OAuth",
      "findings": [
        {
          "requirement": "V10.4.6",
          "verdict": "CONCERN",
          "evidence": "Confidential clients exempt from PKCE; client_secret in Vault provides partial mitigation",
          "note": "Public client PKCE: PASS; V10.2.1: PASS; V10.2.6: PASS"
        }
      ]
    },
    {"territory": "T-rfc-oauth2", "verdict": "PASS", "arbiter": "RFC-OAuth2"}
  ]
}
```

**Reason:** Public client PKCE enforcement is correct (PASS). Exact URIs and state are correct (PASS). The confidential client PKCE exemption with Vault-stored client_secret provides meaningful mitigation — attacker needs both code and secret — but doesn't fully satisfy V10.4.6 which has no explicit confidential-client carve-out. CONCERN for the exemption; all other controls PASS. Overall CONCERN.

**Phrasing-B:** PKCE enforced for public clients. Confidential clients exempt — they authenticate with client_secret (stored in Vault) at token exchange. Exact URIs, state validated. Does the Vault-backed client_secret sufficiently substitute for PKCE on confidential clients?

---

## Pair[CB4-3.4-402]

**A:** Comprehensive eight-territory review — all ext15 patterns correctly implemented. Application: exact redirect URIs; PKCE S256 with CSPRNG (public clients); PKCE also applied to confidential clients (universal enforcement); state validated; tokens revoked on logout + single-use refresh token rotation; back-channel SSO logout; Argon2id m=65536; /me/permissions; RS256 allowlist; signing key rotated quarterly with kid versioning; introspection endpoint requires Bearer access token from callers; MFA enforced for all logins (TOTP); Vault for all secrets; Security Champion + OWASP checklist; privilege grants bounded; aud claim validated; privilege grants in append-only audit log; no token value in any log at any level; nbf = iat - 5 (5-second clock skew only).

**B:**
```
Node 1 — Domain Splitter
Input: comprehensive multi-territory review; all ext15 patterns at correct implementation.
Domains detected: all territories
Routing: multi-territory

Node 2 — Domain Translator
All normative mappings:
  V10.2.1, V10.4.6 (universal), V10.2.6, V10.3 → T-oauth-oidc
  V7.2.4, V7.3.1 (back-channel) → T-asvs-session
  V6.2.5 (Argon2id), V6.3 (MFA all logins) → T-asvs-authentication
  V8.2.1, V8.1.2, V8.3.2 → T-asvs-authorization
  V9.1.1 (RS256 allowlist), V9.2 (quarterly rotation), V10.5.1 (aud), V9.3.1 (no token logging) → T-asvs-token
  RFC 7662 §2.1 (introspection Bearer auth), RFC 7519 §4.1.5 (nbf=iat-5) → T-rfc-jwt, T-rfc-oauth2
  PW.1 (Vault), PW.3.2 (Security Champion + checklist) → T-ssdf-process

Node 3 — Scheme Planner
All territories have complete evidence.

Node 4 — Domain Processor
T-oauth-oidc: V10.2.1 ✓; V10.4.6 universal ✓; V10.2.6 ✓; V10.3 + rotation ✓ → PASS
T-asvs-session: V7.2.4 ✓; V7.3.1 back-channel ✓ → PASS
T-asvs-authentication: V6.2.5 Argon2id ✓; V6.3 all-login MFA ✓ → PASS
T-asvs-authorization: V8.2.1 ✓; V8.1.2 bounded grants ✓; V8.3.2 audit log ✓ → PASS
T-asvs-token: V9.1.1 RS256 allowlist ✓; V9.2 quarterly rotation ✓; V10.5.1 aud ✓;
  V9.3.1 no token logging ✓ → PASS
T-rfc-jwt: nbf=iat-5 (5-second skew) ✓ → PASS
T-rfc-oauth2: RFC 7662 §2.1 Bearer auth on introspection ✓ → PASS
T-ssdf-process: PW.1 Vault ✓; PW.3.2 Security Champion + checklist ✓ → PASS

Node 5 — Applier
All territories: PASS

Node 6–9:
All territories PASS → overall PASS

{
  "sequence_id": "CB4-3.4-402",
  "overall_verdict": "PASS",
  "territories": [
    {"territory": "T-oauth-oidc", "verdict": "PASS"},
    {"territory": "T-asvs-session", "verdict": "PASS"},
    {"territory": "T-asvs-authentication", "verdict": "PASS"},
    {"territory": "T-asvs-authorization", "verdict": "PASS"},
    {"territory": "T-asvs-token", "verdict": "PASS"},
    {"territory": "T-rfc-jwt", "verdict": "PASS"},
    {"territory": "T-rfc-oauth2", "verdict": "PASS"},
    {"territory": "T-ssdf-process", "verdict": "PASS"}
  ]
}
```

**Reason:** All ext15 patterns at correct implementation state: universal PKCE (public + confidential clients); single-use refresh rotation; all-login MFA; quarterly key rotation with kid versioning; introspection with Bearer auth; 5-second nbf only; no token logging. Eight-territory comprehensive PASS.

**Phrasing-B:** All patterns from recent training correctly implemented: universal PKCE, single-use refresh rotation, all-login MFA, quarterly signing key rotation, introspection with Bearer auth, nbf=iat-5 only, no token logging. Does this full implementation yield an 8-territory PASS?

---

## File Summary

| Pair ID | Description | Stratum | Arbiter | Verdict |
|---------|-------------|---------|---------|---------|
| CB4-3.4-395 | RFC 7662 §2.1 Bearer token caller auth — pipeline trace PASS | C | RFC-OAuth2 + V10-OAuth | PASS |
| CB4-3.4-396 | V9.2 RS256 key never rotated + RFC 7517 no mandate — CONCERN + RFC PASS | C | V9-Token + RFC-JWT | CONCERN |
| CB4-3.4-397 | RFC 7662 §2.1 open introspection "internal network = auth" — REPROCESS | C | RFC-OAuth2 + V10-OAuth | REPROCESS |
| CB4-3.4-398 | V6.3 Level 2 financial app; MFA for high-risk only; known-device single-factor — CONCERN | C | ASVS-V6-Auth | CONCERN |
| CB4-3.4-399 | Single-use refresh token rotation + logout revocation — PASS | C | V10-OAuth + V7-Session | PASS |
| CB4-3.4-400 | RFC 7519 nbf = iat - 30min "slow config propagation" — CONCERN (vs 5-sec PASS) | C | RFC-JWT + V9-Token | CONCERN |
| CB4-3.4-401 | PKCE public PASS + confidential client exempt (Vault secret) — CONCERN overall | C | V10-OAuth + RFC-OAuth2 | CONCERN |
| CB4-3.4-402 | All ext15 patterns correctly implemented — 8-territory PASS | C | Multiple | PASS |
