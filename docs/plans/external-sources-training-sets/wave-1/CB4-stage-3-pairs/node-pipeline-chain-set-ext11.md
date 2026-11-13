# CB4 Stage 3 — Node Pipeline Chain Set Extension 11
# Sequences CB4-3.4-447 through CB4-3.4-454
# Stratum: C — full 9-node pipeline traces with intermediate outputs
# Instances: 16 (8 base pairs × 2 phrasings)
# Running total after this file: 908 instances

---

## Pair[CB4-3.4-447]

**A:** CORS review on token endpoint. The application's OAuth token endpoint (`POST /oauth/token`) is configured with `Access-Control-Allow-Origin: *`. The security team argues: "The token endpoint requires client_secret in the body. A cross-origin attacker would need to know the client_secret — CORS doesn't matter if the credential isn't available to the attacker."

**B:**
```
Node 1 — Domain Splitter
Input: token endpoint wildcard CORS; team argues client_secret requirement negates CORS risk.
Domains detected: [oauth-oidc, rfc-oauth2]
Primary domain: rfc-oauth2
Routing: multi-domain

Node 2 — Domain Translator
Normative mapping:
  Token endpoint security: RFC 6749 §3.2 + OAuth security BCP
  CORS: ASVS V10 (token endpoint configuration)
Territories: T-rfc-oauth2, T-oauth-oidc

Node 3 — Scheme Planner
Key question: does requiring client_secret in body make wildcard CORS acceptable?

Node 4 — Domain Processor
T-rfc-oauth2 + T-oauth-oidc analysis:
  CORS wildcard on token endpoint: `Access-Control-Allow-Origin: *`
  Team argument: "client_secret required → attacker lacks credential → CORS irrelevant"
  
  Evaluation of the argument:
  (1) The argument assumes the only threat is an unauthorized client making
      token requests. However, wildcard CORS also enables:
      (a) CSRF-style token endpoint calls from malicious pages where the
          victim's browser sends credentials (e.g., cookies, existing tokens)
          automatically, and the wildcard CORS allows the response to be read
      (b) Probing/scanning of the token endpoint from any origin
  (2) For public clients (no client_secret): wildcard CORS means any origin
      can make authorization code exchange requests. The PKCE code_verifier
      requirement partially mitigates this, but the broad CORS surface is still
      a deviation from the principle of restricting token endpoint access.
  (3) Correct implementation: restrict `Access-Control-Allow-Origin` to the
      specific origins that legitimately call the token endpoint (first-party
      SPA origins).
  (4) The argument partially holds for confidential clients (client_secret
      protects against unauthorized exchanges), but does not fully address
      all CORS risks at the token endpoint.
  
  Finding: REPROCESS — wildcard CORS on token endpoint regardless of
    client_secret requirement. Client_secret partially mitigates code exchange
    abuse but does not eliminate the CORS concern.

Node 5 — Applier
Token endpoint CORS: REPROCESS
  Argument partially valid for confidential clients but insufficient
  — CORS should be restricted regardless

Node 6 — Result Translator
T-rfc-oauth2: REPROCESS
T-oauth-oidc: REPROCESS

Node 7 — Arbiter Selector
REPROCESS → RFC-OAuth2 + ASVS-V10-OAuth

Node 8 — Arbiter Unit (RFC-OAuth2)
CORS wildcard on token endpoint: REPROCESS
Argument partially acknowledged: client_secret requirement does provide
mitigation for confidential clients against unauthorized code exchange.
However:
(1) Public clients have no client_secret — wildcard CORS leaves them fully exposed.
(2) Cross-site requests from malicious pages (where the victim's session
    credentials are sent automatically) can abuse the wildcard.
(3) Principle of least privilege: the token endpoint should only be reachable
    from authorized origins.

Evidence gap: `Access-Control-Allow-Origin: *` on token endpoint.
Remediation: `Access-Control-Allow-Origin: https://app.example.com` (first-party origin).
Overall verdict: REPROCESS

Node 9 — Formatter
{
  "sequence_id": "CB4-3.4-447",
  "overall_verdict": "REPROCESS",
  "territories": [
    {
      "territory": "T-rfc-oauth2",
      "verdict": "REPROCESS",
      "arbiter": "RFC-OAuth2",
      "findings": [
        {
          "requirement": "token-endpoint-CORS",
          "verdict": "REPROCESS",
          "evidence": "Access-Control-Allow-Origin: * on token endpoint",
          "argument_rejected": "client_secret partially mitigates for confidential clients only; public clients unprotected; broad CORS surface remains"
        }
      ]
    },
    {"territory": "T-oauth-oidc", "verdict": "REPROCESS", "arbiter": "ASVS-V10-OAuth"}
  ]
}
```

**Reason:** The client_secret argument partially holds for confidential clients in the specific scenario of unauthorized code exchange, but it does not address all CORS risks (public clients, CSRF-style cross-site requests, broad attack surface). Wildcard CORS on the token endpoint is a REPROCESS. Restrict `Access-Control-Allow-Origin` to specific authorized origins.

**Phrasing-B:** Token endpoint CORS is wildcard. Our token endpoint requires client_secret in the body — attackers don't have it. Does requiring client_secret negate the wildcard CORS risk on the token endpoint?

---

## Pair[CB4-3.4-448]

**A:** Implicit flow retirement pipeline trace. Mobile iOS application currently uses `response_type=token` (implicit flow). Access tokens are returned in the URI fragment after the authorization redirect. PKCE is not used (implicit flow doesn't use authorization codes). The team plans to migrate to authorization code + PKCE in the next release quarter. Current state is assessed.

**B:**
```
Node 1 — Domain Splitter
Input: implicit flow (response_type=token) on iOS mobile app; migration planned next quarter.
Domains detected: [oauth-oidc, rfc-oauth2]
Primary domain: oauth-oidc
Routing: multi-domain

Node 2 — Domain Translator
Normative mapping:
  response_type=token: ASVS V10 (implicit flow deprecated)
  RFC 9700 / OAuth BCP: implicit flow SHOULD NOT be used
Territories: T-oauth-oidc, T-rfc-oauth2

Node 3 — Scheme Planner
Key question: implicit flow on mobile app — current state compliance?

Node 4 — Domain Processor
T-oauth-oidc analysis:
  response_type=token (implicit): access token in URI fragment ✗
    - Token in URI fragment: exposed in browser history, referrer headers,
      JavaScript on the page, other scripts
    - No PKCE protection: implicit flow does not use authorization codes,
      so PKCE (which protects codes) is not applicable
    - ASVS V10: implicit flow must not be used
    - RFC 8252 (OAuth 2.0 for Native Apps): authorization code + PKCE
      is the correct pattern for native/mobile applications
  Planned migration: does not affect current-state assessment
    - Current state: implicit flow in production = REPROCESS
    - The planned migration is noted but does not change the current verdict
    (Same principle as "bcrypt→Argon2id planning doesn't make bcrypt non-compliant"
     but in reverse: planning code→PKCE migration doesn't make implicit flow compliant)

Node 5 — Applier
V10 (implicit flow): REPROCESS
  Evidence: response_type=token in production use on iOS app
  Planned migration: noted; does not affect current assessment

Node 6 — Result Translator
T-oauth-oidc: REPROCESS
T-rfc-oauth2: REPROCESS
Overall: REPROCESS

Node 7 — Arbiter Selector
REPROCESS → ASVS-V10-OAuth + RFC-OAuth2

Node 8 — Arbiter Unit (ASVS-V10-OAuth)
V10 (implicit flow prohibition): REPROCESS
Current state: implicit flow in production on iOS app.
Planned migration to code + PKCE: noted; does not change current verdict.

Note on distinction from "bcrypt planning upgrade" PASS case:
  bcrypt cost 12 planning upgrade to Argon2id → PASS (current state compliant)
  Implicit flow planning migration to code+PKCE → REPROCESS (current state non-compliant)
The distinction: bcrypt is currently compliant; implicit flow is currently prohibited.
Planning an improvement to a compliant implementation does not create a gap.
Planning an improvement to a non-compliant implementation does not resolve the gap.

Evidence gap: response_type=token in production use.
Overall verdict: REPROCESS

Node 9 — Formatter
{
  "sequence_id": "CB4-3.4-448",
  "overall_verdict": "REPROCESS",
  "territories": [
    {
      "territory": "T-oauth-oidc",
      "verdict": "REPROCESS",
      "arbiter": "ASVS-V10-OAuth",
      "findings": [
        {
          "requirement": "V10-implicit-flow",
          "verdict": "REPROCESS",
          "evidence": "response_type=token in production use on iOS mobile app",
          "note": "Planned migration to code+PKCE does not resolve current-state finding"
        }
      ]
    },
    {"territory": "T-rfc-oauth2", "verdict": "REPROCESS", "arbiter": "RFC-OAuth2"}
  ]
}
```

**Reason:** Implicit flow (`response_type=token`) is prohibited by ASVS V10 and OAuth 2.0 BCP. Planned migration to authorization code + PKCE is positive but does not resolve the current-state finding — the current state is non-compliant. Contrast with the bcrypt→Argon2id planning case (PASS) where the current state is compliant; here the current state itself is the violation.

**Phrasing-B:** iOS app uses response_type=token (implicit) currently. We plan to migrate to authorization code + PKCE next quarter. Does planning the migration to code+PKCE resolve the current implicit flow finding, or is the current state still a REPROCESS?

---

## Pair[CB4-3.4-449]

**A:** PKCE plain fallback combined with CORS review. Authorization server: (1) accepts `code_challenge_method=plain` as a fallback when clients specify plain; (2) token endpoint CORS restricted to `https://app.example.com`. Two separate findings evaluated.

**B:**
```
Node 1 — Domain Splitter
Input: PKCE plain fallback (downgrade risk) + correct token endpoint CORS.
Domains detected: [oauth-oidc, rfc-oauth2]
Routing: multi-domain

Node 2 — Domain Translator
Normative mapping:
  PKCE method enforcement: V10.4.6, RFC 7636 §4.2
  CORS: token endpoint configuration
Territories: T-oauth-oidc, T-rfc-oauth2

Node 3 — Scheme Planner
Two controls evaluated independently:
  (a) PKCE plain fallback: downgrade attack vector
  (b) CORS: restricted to first-party origin

Node 4 — Domain Processor
PKCE plain fallback:
  Accepting plain as fallback creates a downgrade vector.
  An attacker who can modify authorization request parameters can set
  code_challenge_method=plain, making the code_challenge equal to the
  code_verifier (no hash protection).
  V10.4.6 requires S256 enforcement; plain must be rejected.
  Finding: REPROCESS

Token endpoint CORS (`https://app.example.com` only):
  Correctly restricted to the legitimate first-party SPA origin ✓
  Finding: PASS

Node 5 — Applier
V10.4.6 (PKCE plain fallback): REPROCESS
CORS: PASS

Node 6 — Result Translator
T-oauth-oidc: REPROCESS (PKCE finding dominates)
T-rfc-oauth2: REPROCESS (RFC 7636 §4.2)
Overall: REPROCESS

Node 7 — Arbiter Selector
REPROCESS → ASVS-V10-OAuth

Node 8 — Arbiter Unit (ASVS-V10-OAuth)
V10.4.6: REPROCESS — plain method accepted as fallback.
CORS: PASS — correctly restricted to https://app.example.com.
Note: correct CORS does not mitigate the PKCE downgrade finding.
Overall verdict: REPROCESS

Node 9 — Formatter
{
  "sequence_id": "CB4-3.4-449",
  "overall_verdict": "REPROCESS",
  "territories": [
    {
      "territory": "T-oauth-oidc",
      "verdict": "REPROCESS",
      "arbiter": "ASVS-V10-OAuth",
      "findings": [
        {"requirement": "V10.4.6", "verdict": "REPROCESS", "evidence": "plain method accepted as fallback"},
        {"requirement": "CORS", "verdict": "PASS", "evidence": "restricted to https://app.example.com"}
      ]
    }
  ]
}
```

**Reason:** Two independent controls: PKCE plain fallback is REPROCESS (downgrade vector); CORS restriction is PASS (correctly configured). One REPROCESS in the territory overrides the PASS. Correct CORS does not compensate for the PKCE downgrade risk.

**Phrasing-B:** Token endpoint CORS is correctly restricted to our first-party origin. But the authorization server accepts plain as a PKCE fallback. Does the correct CORS compensate for the PKCE downgrade, or is the plain fallback still a REPROCESS?

---

## Pair[CB4-3.4-450]

**A:** Revocation endpoint and introspection combined. Authorization server: (1) token revocation endpoint (`POST /oauth/revoke`) — clients must present valid client credentials (client_id + client_secret for confidential clients; client_id only for public clients); (2) introspection endpoint (`POST /oauth/introspect`) — requires a Bearer access token issued to an introspection client. Both endpoints on HTTPS.

**B:**
```
Node 1 — Domain Splitter
Input: revocation with client auth; introspection with Bearer token; both HTTPS.
Domains detected: [rfc-oauth2, oauth-oidc]
Routing: multi-domain

Node 2 — Domain Translator
Normative mapping:
  RFC 7009 §2.1: revocation client authentication
  RFC 7662 §2.1: introspection authorization
Territories: T-rfc-oauth2, T-oauth-oidc

Node 3 — Scheme Planner
Both RFC MUST requirements evaluated.

Node 4 — Domain Processor
RFC 7009 §2.1 (revocation):
  Confidential clients: client_id + client_secret ✓ — satisfies MUST
  Public clients: client_id only ✓ — correct for public clients (no secret)
  RFC 7009 §2.1: PASS

RFC 7662 §2.1 (introspection):
  Bearer access token required ✓ — satisfies "separate OAuth 2.0 access token" form
  RFC 7662 §2.1: PASS

Both HTTPS ✓

Node 5–8: Both PASS
Overall verdict: PASS

Node 9 — Formatter
{
  "sequence_id": "CB4-3.4-450",
  "overall_verdict": "PASS",
  "territories": [
    {
      "territory": "T-rfc-oauth2",
      "verdict": "PASS",
      "arbiter": "RFC-OAuth2",
      "findings": [
        {"requirement": "RFC7009-§2.1", "verdict": "PASS"},
        {"requirement": "RFC7662-§2.1", "verdict": "PASS"}
      ]
    }
  ]
}
```

**Reason:** Revocation endpoint requires client authentication (client_id + secret for confidential clients; client_id for public clients) — satisfies RFC 7009 §2.1. Introspection endpoint requires Bearer access token — satisfies RFC 7662 §2.1. Both on HTTPS. PASS.

**Phrasing-B:** Revocation endpoint: confidential clients need client_id + secret, public clients present client_id only. Introspection endpoint: Bearer token required. Both HTTPS. Do these configurations satisfy RFC 7009 §2.1 and RFC 7662 §2.1?

---

## Pair[CB4-3.4-451]

**A:** Combined CORS, implicit flow, and session review. Application: (1) uses implicit flow (response_type=token) for a browser-based SPA; (2) token endpoint CORS is restricted to `https://app.example.com`; (3) session cookie: HttpOnly, Secure, SameSite=Lax; (4) no session ID regeneration on login (session-based application). Argon2id m=65536. Vault.

**B:**
```
Node 1 — Domain Splitter
Input: implicit flow SPA + restricted CORS + no session regeneration.
Domains detected: [oauth-oidc, rfc-oauth2, asvs-session]
Routing: multi-domain

Node 2 — Domain Translator
Normative mapping:
  Implicit flow: V10 (prohibited), OAuth BCP
  CORS: token endpoint (PASS as described)
  Session fixation: V7.2.3
Territories: T-oauth-oidc, T-rfc-oauth2, T-asvs-session

Node 3 — Scheme Planner
Multiple controls; two gaps identified at intake.

Node 4 — Domain Processor
T-oauth-oidc:
  response_type=token (implicit): REPROCESS — prohibited by V10 and OAuth BCP
  CORS restricted: PASS ✓ (does not mitigate implicit flow finding)

T-asvs-session:
  V7.2.3: no session ID regeneration on login ✗ → REPROCESS
  Session cookie flags: HttpOnly ✓, Secure ✓, SameSite=Lax ✓ (partially mitigating)

T-asvs-authentication:
  V6.2.5 Argon2id ✓ → PASS

T-ssdf-process: Vault ✓ → PASS

Node 5 — Applier
Implicit flow: REPROCESS
Session fixation: REPROCESS
All other controls: PASS

Node 6–8:
Two REPROCESS findings in two separate territories.
Overall verdict: REPROCESS

Node 9 — Formatter
{
  "sequence_id": "CB4-3.4-451",
  "overall_verdict": "REPROCESS",
  "territories": [
    {
      "territory": "T-oauth-oidc",
      "verdict": "REPROCESS",
      "arbiter": "ASVS-V10-OAuth",
      "findings": [{"requirement": "V10-implicit", "verdict": "REPROCESS"}]
    },
    {
      "territory": "T-asvs-session",
      "verdict": "REPROCESS",
      "arbiter": "ASVS-V7-Session",
      "findings": [{"requirement": "V7.2.3", "verdict": "REPROCESS"}]
    }
  ]
}
```

**Reason:** Two independent REPROCESS findings: implicit flow prohibited (V10); session fixation gap (V7.2.3). Correct CORS, SameSite cookies, Argon2id, and Vault are all PASS but cannot override the two REPROCESS findings. Overall REPROCESS.

**Phrasing-B:** SPA uses implicit flow; token endpoint CORS correctly restricted; session not regenerated on login; HttpOnly/Secure/SameSite=Lax cookie; Argon2id, Vault. How many REPROCESS findings are there and what are they?

---

## Pair[CB4-3.4-452]

**A:** DPoP implementation review. Application implements DPoP (RFC 9449). Client generates an ephemeral EC key pair per request. DPoP proof JWT includes: `typ: dpop+jwt`, `htm` (HTTP method), `htu` (HTTP URI), `iat`, a `jti` (unique identifier), signed with the client's ephemeral private key. The authorization server binds the access token to the client's DPoP public key (`cnf` claim with `jkt` thumbprint). Resource server validates the DPoP proof on each request. The team asks: "Does our DPoP implementation satisfy the RFC 9449 requirements?"

**B:**
```
Node 1 — Domain Splitter
Input: DPoP implementation — ephemeral EC key, proof JWT with required claims, token binding.
Domains detected: [asvs-token, rfc-jwt]
Primary domain: asvs-token
Routing: multi-domain

Node 2 — Domain Translator
Normative mapping:
  DPoP: RFC 9449 (Demonstrating Proof-of-Possession)
  Token binding: V9 / token security
Territories: T-asvs-token, T-rfc-jwt

Node 3 — Scheme Planner
Evaluate DPoP implementation against RFC 9449 requirements.

Node 4 — Domain Processor
RFC 9449 DPoP proof requirements:
  typ: dpop+jwt ✓ (required header)
  htm (HTTP method) ✓ (required claim)
  htu (HTTP URI) ✓ (required claim)
  iat ✓ (required; freshness)
  jti ✓ (required; replay prevention per-request unique value)
  Signed with ephemeral private key ✓
  Access token bound to DPoP public key (cnf/jkt) ✓
  Resource server validates proof ✓

Missing check: RFC 9449 §4.3 also requires that the resource server check
  that the jti has not been seen before (replay detection). The submission
  states jti is present but does not mention replay detection.
  This is a potential gap; without explicit confirmation.

Finding: PASS with note — all stated components correctly implemented;
  jti replay detection not confirmed in submission. If jti replay detection
  is implemented, full PASS. If not, CONCERN (replay possible).

Node 5 — Applier
DPoP implementation: PASS (stated components)
Note: jti replay detection implementation not confirmed

Node 6–8:
T-asvs-token: PASS (all stated components)
T-rfc-jwt: PASS
Overall verdict: PASS with note

Node 9 — Formatter
{
  "sequence_id": "CB4-3.4-452",
  "overall_verdict": "PASS",
  "territories": [
    {
      "territory": "T-asvs-token",
      "verdict": "PASS",
      "arbiter": "ASVS-V9-Token",
      "note": "All stated RFC 9449 components present; confirm jti replay detection is implemented"
    },
    {"territory": "T-rfc-jwt", "verdict": "PASS", "arbiter": "RFC-JWT"}
  ]
}
```

**Reason:** All stated DPoP components correctly implemented per RFC 9449: `typ`, `htm`, `htu`, `iat`, `jti`, ephemeral key signature, `cnf/jkt` token binding, resource server validation. PASS with a note to confirm `jti` replay detection — RFC 9449 §4.3 requires the server to detect replayed DPoP proofs via `jti` tracking.

**Phrasing-B:** DPoP with ephemeral EC keys, proof JWT has typ/htm/htu/iat/jti, access token bound with cnf/jkt, resource server validates proof. Does this implementation satisfy RFC 9449?

---

## Pair[CB4-3.4-453]

**A:** Scope enforcement combined with DPoP. Resource server validates: token signature, `exp`, `aud`, `iss`. Also validates the DPoP proof (ephemeral key, htm, htu, iat, jti with replay detection). However: the `scope` claim in the access token is not checked — any DPoP-bound token with valid signature and claims can call any endpoint regardless of scope.

**B:**
```
Node 1 — Domain Splitter
Input: DPoP correctly implemented + scope not enforced at resource server.
Domains detected: [asvs-token, oauth-oidc, rfc-oauth2]
Routing: multi-domain

Node 2 — Domain Translator
Normative mapping:
  DPoP: RFC 9449 (implementation aspects)
  Scope enforcement: RFC 6749 §3.3, V10/V8
Territories: T-asvs-token, T-oauth-oidc

Node 3 — Scheme Planner
Two independent controls: DPoP (PASS) and scope enforcement (gap).

Node 4 — Domain Processor
DPoP validation:
  All RFC 9449 required components validated ✓
  jti replay detection ✓
  DPoP: PASS

Scope enforcement:
  `scope` claim not checked at resource server ✗
  DPoP-bound token with scope=read can call write/admin endpoints
  DPoP provides sender-constraining (proves possession of key pair)
  DPoP does NOT enforce scope — these are independent mechanisms
  Scope enforcement gap: REPROCESS

Node 5 — Applier
DPoP: PASS
Scope: REPROCESS (same finding as non-DPoP scope non-enforcement)

Node 6–8:
T-asvs-token: REPROCESS (scope gap overrides DPoP PASS)
Overall verdict: REPROCESS

Node 9 — Formatter
{
  "sequence_id": "CB4-3.4-453",
  "overall_verdict": "REPROCESS",
  "territories": [
    {
      "territory": "T-asvs-token",
      "verdict": "REPROCESS",
      "arbiter": "ASVS-V9-Token",
      "findings": [
        {"requirement": "DPoP-RFC9449", "verdict": "PASS"},
        {"requirement": "scope-enforcement", "verdict": "REPROCESS",
         "evidence": "scope claim not validated at resource server despite DPoP implementation"}
      ]
    }
  ]
}
```

**Reason:** DPoP is correctly implemented — it constrains which client can use a token (possession proof). Scope enforcement is a separate mechanism that constrains what operations the token can perform. DPoP PASS does not compensate for scope non-enforcement. The resource server must validate both DPoP proof AND scope claim independently. REPROCESS.

**Phrasing-B:** DPoP fully implemented with replay detection. Resource server validates signature, exp, aud, iss, and DPoP proof — but not the scope claim. Does DPoP implementation compensate for scope non-enforcement at the resource server?

---

## Pair[CB4-3.4-454]

**A:** Comprehensive eight-territory review — all ext17 patterns correctly implemented. Application: token endpoint CORS restricted to first-party SPA origin; authorization server supports response_type=code only (implicit rejected); PKCE S256 enforced with plain rejected; revocation endpoint requires client credentials; introspection endpoint requires Bearer access token; scope enforced at resource server; DPoP implemented (all RFC 9449 components including jti replay detection); session ID regenerated on login; RS256 2048-bit annually rotated; Argon2id; Vault; Security Champion + OWASP checklist; aud and iss validated; back-channel SSO logout; refresh + access tokens revoked on logout.

**B:**
```
Node 1 — Domain Splitter
Input: comprehensive multi-territory review; all ext17 patterns at correct state.
Domains detected: all territories
Routing: multi-territory

Node 2–4 — Processing all territories:
T-rfc-oauth2:
  CORS restricted ✓; response_type=code only ✓; PKCE S256 (plain rejected) ✓
  RFC 7009 §2.1 revocation auth ✓; RFC 7662 §2.1 introspection auth ✓
  Scope enforced at resource server ✓ → PASS

T-asvs-token:
  DPoP RFC 9449 (all components + jti replay) ✓
  V9.1.1 RS256 ✓; RFC 7518 §3.3 2048-bit ✓; V9.2 annual rotation ✓
  V10.5.1 aud ✓; iss ✓ → PASS

T-oauth-oidc:
  V10.2.1 ✓; V10.4.6 (S256, no plain) ✓; V10.2.6 ✓; V10.3 ✓ → PASS

T-asvs-session:
  V7.2.3 (session ID regenerated) ✓; V7.3.1 back-channel ✓ → PASS

T-asvs-authentication: V6.2.5 Argon2id ✓ → PASS
T-ssdf-process: PW.1 Vault ✓; PW.3.2 Security Champion ✓ → PASS

All territories: PASS
Overall verdict: PASS

Node 9 — Formatter
{
  "sequence_id": "CB4-3.4-454",
  "overall_verdict": "PASS",
  "territories": [
    {"territory": "T-rfc-oauth2", "verdict": "PASS"},
    {"territory": "T-asvs-token", "verdict": "PASS"},
    {"territory": "T-oauth-oidc", "verdict": "PASS"},
    {"territory": "T-asvs-session", "verdict": "PASS"},
    {"territory": "T-asvs-authentication", "verdict": "PASS"},
    {"territory": "T-ssdf-process", "verdict": "PASS"}
  ]
}
```

**Reason:** All ext17 patterns correctly implemented: CORS restricted, code-flow only, plain rejected, both RFC endpoint auth requirements met, scope enforced, DPoP with replay detection, session fixation prevented, 2048-bit RS256 rotated. Comprehensive six-territory PASS.

**Phrasing-B:** All ext17 patterns correct: CORS restricted, code-only, PKCE S256 (no plain), revocation and introspection both authenticated, scope enforced at resource server, DPoP with jti replay detection, session ID regenerated, 2048-bit RS256 annually rotated, Argon2id, Vault, Security Champion. Does this pass all territories?

---

## File Summary

| Pair ID | Description | Stratum | Arbiter | Verdict |
|---------|-------------|---------|---------|---------|
| CB4-3.4-447 | Token endpoint wildcard CORS "client_secret negates it" — REPROCESS (partial argument) | C | RFC-OAuth2 + V10-OAuth | REPROCESS |
| CB4-3.4-448 | Implicit flow planning migration "does it resolve current finding" — REPROCESS (current state non-compliant) | C | V10-OAuth + RFC-OAuth2 | REPROCESS |
| CB4-3.4-449 | PKCE plain fallback REPROCESS + correct CORS PASS → REPROCESS overall | C | V10-OAuth | REPROCESS |
| CB4-3.4-450 | Revocation client auth + introspection Bearer auth → both RFC MUSTs satisfied PASS | C | RFC-OAuth2 | PASS |
| CB4-3.4-451 | Implicit flow REPROCESS + session fixation REPROCESS + correct other controls → REPROCESS | C | V10-OAuth + V7-Session | REPROCESS |
| CB4-3.4-452 | DPoP full RFC 9449 implementation review — PASS (with jti replay note) | C | V9-Token + RFC-JWT | PASS |
| CB4-3.4-453 | DPoP PASS + scope non-enforcement → REPROCESS (DPoP ≠ scope enforcement) | C | V9-Token + V10-OAuth | REPROCESS |
| CB4-3.4-454 | All ext17 patterns correctly implemented → comprehensive 6-territory PASS | C | Multiple | PASS |
