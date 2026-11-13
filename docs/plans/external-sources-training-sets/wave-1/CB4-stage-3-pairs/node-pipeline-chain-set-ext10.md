# CB4 Stage 3 — Node Pipeline Chain Set Extension 10
# Sequences CB4-3.4-421 through CB4-3.4-428
# Stratum: C — full 9-node pipeline traces with intermediate outputs
# Instances: 16 (8 base pairs × 2 phrasings)
# Running total after this file: 856 instances

---

## Pair[CB4-3.4-421]

**A:** Token endpoint TLS review. Application uses HTTPS on all external-facing endpoints. The internal microservice architecture uses HTTP between services. The OAuth token endpoint runs as an internal microservice — clients call it via the API gateway (HTTPS). The API gateway forwards token endpoint requests to the internal service over HTTP. Team: "All client-facing traffic is HTTPS. The internal hop is trusted network."

**B:**
```
Node 1 — Domain Splitter
Input: token endpoint HTTP internally; HTTPS at API gateway; "trusted internal network" argument.
Domains detected: [oauth-oidc, rfc-oauth2]
Primary domain: rfc-oauth2
Secondary: oauth-oidc
Routing: multi-domain

Node 2 — Domain Translator
Domains: rfc-oauth2, oauth-oidc
Normative mapping:
  Token endpoint TLS: RFC 6749 §3.2 (MUST use TLS)
Territories: T-rfc-oauth2, T-oauth-oidc

Node 3 — Scheme Planner
Territories: [T-rfc-oauth2, T-oauth-oidc]
Arbiters: RFC-OAuth2, ASVS-V10-OAuth
Key question: does API gateway HTTPS satisfy RFC 6749 §3.2 at the token endpoint?

Node 4 — Domain Processor
T-rfc-oauth2 analysis:
  RFC 6749 §3.2: "The endpoint URI MUST use TLS."
  Implementation:
    External facing: HTTPS (clients → gateway) ✓
    Internal: HTTP (gateway → token service) ✗
  The token endpoint URI as seen by the token service itself is HTTP.
  What traverses the HTTP segment:
    - Authorization codes (from code grant)
    - Client credentials (client_id, client_secret)
    - Access tokens issued in response
    - Refresh tokens submitted for exchange
  All of these transit in plaintext on the internal HTTP segment.
  Team argument ("trusted internal network"):
    - "Trusted" network reduces external interception probability but does not
      eliminate internal interception (compromised hosts, internal actors,
      monitoring systems, debug proxies)
    - RFC 6749 §3.2 is unconditional — it does not carve out exceptions for
      internal network segments
    - Pattern: same as "load balancer TLS" and "API gateway TLS" arguments
      evaluated previously — both rejected on the same basis

Node 5 — Applier
RFC 6749 §3.2 → REPROCESS
  Evidence: token endpoint serves HTTP on the internal segment
  Argument rejected: gateway TLS ≠ endpoint TLS; "trusted network" does not
    satisfy unconditional MUST

Node 6 — Result Translator
T-rfc-oauth2: REPROCESS
T-oauth-oidc: REPROCESS (token endpoint security violation)

Node 7 — Arbiter Selector
REPROCESS → RFC-OAuth2 + ASVS-V10-OAuth

Node 8 — Arbiter Unit (RFC-OAuth2)
RFC 6749 §3.2: REPROCESS
Argument rejected: "API gateway handles TLS."

This is the third variant of the gateway/proxy TLS argument pattern:
- Load balancer TLS termination + internal HTTP = REPROCESS
- API gateway TLS termination + internal HTTP = REPROCESS
- This submission: API gateway + "trusted network" = REPROCESS

In all three cases:
(1) The token endpoint URI uses HTTP.
(2) OAuth credentials (codes, secrets, tokens) traverse the HTTP segment.
(3) RFC 6749 §3.2's MUST is unconditional — no internal-network exception.
(4) Remediation: configure the token endpoint service to use TLS directly,
    or use mTLS (mutual TLS) for internal service-to-service communication.

Evidence gap: token endpoint HTTP on internal segment.
Overall verdict: REPROCESS

Node 9 — Formatter
{
  "sequence_id": "CB4-3.4-421",
  "overall_verdict": "REPROCESS",
  "territories": [
    {
      "territory": "T-rfc-oauth2",
      "verdict": "REPROCESS",
      "arbiter": "RFC-OAuth2",
      "findings": [
        {
          "requirement": "RFC6749-§3.2",
          "verdict": "REPROCESS",
          "evidence": "Token endpoint HTTP internally; API gateway HTTPS only",
          "argument_rejected": "Gateway TLS ≠ endpoint TLS; no internal-network exception in RFC 6749 §3.2"
        }
      ]
    },
    {"territory": "T-oauth-oidc", "verdict": "REPROCESS", "arbiter": "ASVS-V10-OAuth"}
  ]
}
```

**Reason:** Third variant of the TLS argument pattern: load balancer, API gateway, and now "trusted internal network" — all rejected. RFC 6749 §3.2 requires TLS at the token endpoint URI itself. OAuth credentials transit the HTTP internal segment in plaintext. The MUST is unconditional.

**Phrasing-B:** API gateway handles HTTPS from clients and forwards to our token service over HTTP internally. The internal network is trusted. Does "trusted internal network" create an exception to RFC 6749 §3.2's TLS requirement?

---

## Pair[CB4-3.4-422]

**A:** Session fixation review with detailed login flow. Application uses stateless JWT sessions (no server-side session storage). On login, the server validates credentials and issues a signed JWT. The JWT is delivered in the response body. No session cookie is set before authentication. There is no pre-authentication session ID — the first server-issued token is the post-authentication JWT.

**B:**
```
Node 1 — Domain Splitter
Input: stateless JWT sessions; no pre-authentication session ID; JWT issued only post-login.
Domains detected: [asvs-session]
Primary domain: asvs-session
Routing: single-domain

Node 2 — Domain Translator
Domain: asvs-session
Normative mapping:
  Session fixation: ASVS V7.2.3
  Stateless JWT sessions: scoping question
Territory: T-asvs-session

Node 3 — Scheme Planner
Territories: [T-asvs-session]
Arbiter: ASVS-V7-Session
Key question: does V7.2.3 apply to stateless JWT architectures?

Node 4 — Domain Processor
T-asvs-session analysis:
  V7.2.3 (session fixation prevention): requires new session identifier issued
    after successful authentication.
  Stateless JWT architecture:
    - No session ID exists before authentication — there is no pre-auth session
      to "fix" to the authenticated state
    - The JWT is the first and only session credential issued; it is issued only
      after successful credential verification
    - A session fixation attack requires the attacker to pre-place a known
      session ID that the victim then authenticates with
    - Without a pre-authentication session, the fixation attack surface does
      not exist in this architecture
  V7.2.3 scoping: the requirement is satisfied by architectural design —
    JWT issued only post-authentication inherently prevents fixation.
  No pre-authentication token exists to be "fixed."

Node 5 — Applier
V7.2.3 → PASS
  Evidence: stateless JWT architecture; no pre-authentication session ID;
    JWT issued only after credential verification
  Finding: session fixation attack surface absent in this architecture

Node 6 — Result Translator
T-asvs-session: PASS
  V7.2.3 satisfied by architecture (no pre-auth session to fixate)

Node 7 — Arbiter Selector
PASS — confirmatory

Node 8 — Arbiter Unit (ASVS-V7-Session)
V7.2.3: PASS
Stateless JWT sessions that issue tokens only after authentication are inherently
protected against session fixation:
(1) Session fixation requires a pre-authentication session identifier that
    survives the authentication event.
(2) A stateless JWT architecture with no pre-authentication session has no
    fixation surface — there is no ID to "fix."
(3) V7.2.3's requirement (new session identifier after authentication) is
    satisfied by design: the JWT is the first identifier issued and it is
    issued post-authentication.

This is a correct architectural approach that satisfies V7.2.3 without explicit
regeneration logic (because there is nothing to regenerate from).
Overall verdict: PASS

Node 9 — Formatter
{
  "sequence_id": "CB4-3.4-422",
  "overall_verdict": "PASS",
  "territories": [
    {
      "territory": "T-asvs-session",
      "verdict": "PASS",
      "arbiter": "ASVS-V7-Session",
      "findings": [
        {
          "requirement": "V7.2.3",
          "verdict": "PASS",
          "evidence": "Stateless JWT; no pre-auth session; JWT issued only post-authentication",
          "note": "Fixation surface absent by architecture"
        }
      ]
    }
  ]
}
```

**Reason:** V7.2.3 session fixation prevention applies to session-based architectures with pre-authentication session identifiers. A stateless JWT architecture that issues the first token only after authentication has no pre-authentication session to fixate — the attack surface is absent by design. V7.2.3 is satisfied architecturally.

**Phrasing-B:** Stateless JWT sessions: no session cookie before login, JWT issued only after credential validation. No pre-auth session ID exists. Does V7.2.3 apply to stateless JWT architectures, or is the fixation surface absent by design?

---

## Pair[CB4-3.4-423]

**A:** Authorization code lifetime review. Authorization codes: 2-second expiry. The application is a high-frequency trading platform where the OAuth flow completes entirely within a single user interaction. Codes are single-use. Team: "We set the shortest reasonable expiry. 2-second codes maximize security by minimizing the interception window."

**B:**
```
Node 1 — Domain Splitter
Input: 2-second authorization code expiry; single-use; high-frequency trading context.
Domains detected: [oauth-oidc, rfc-oauth2]
Primary domain: oauth-oidc
Secondary: rfc-oauth2
Routing: multi-domain

Node 2 — Domain Translator
Normative mapping:
  Authorization code lifetime: RFC 6749 §4.1.2 (RECOMMENDED maximum 10 minutes)
  Single-use: RFC 6749 §4.1.2 (code revoked after use)
Territories: T-oauth-oidc, T-rfc-oauth2

Node 3 — Scheme Planner
Key question: is 2-second expiry compliant with RFC 6749 §4.1.2?

Node 4 — Domain Processor
T-rfc-oauth2 analysis:
  RFC 6749 §4.1.2: RECOMMENDED maximum 10 minutes.
  2-second expiry: well within the 10-minute maximum ✓
  Single-use: code revoked after exchange ✓
  RFC 6749 §4.1.2 sets a maximum, not a minimum — shorter expiry is more
  restrictive and provides better security.
  2-second expiry is compliant.

  Practical consideration: a 2-second window may be very tight depending on:
    - Network latency between client and authorization server
    - Processing time for the token exchange
    For high-frequency trading platforms with low-latency infrastructure,
    2 seconds may be adequate. This is an operational consideration, not a
    compliance finding.

Node 5 — Applier
RFC 6749 §4.1.2: PASS
  2-second expiry is within the RECOMMENDED maximum of 10 minutes
  Single-use correctly implemented

Node 6 — Result Translator
T-rfc-oauth2: PASS
T-oauth-oidc: PASS
Overall: PASS

Node 7 — Arbiter Selector
PASS — confirmatory

Node 8 — Arbiter Units (confirmatory)
RFC-OAuth2: RFC 6749 §4.1.2 PASS — 2-second expiry < 10-minute maximum; single-use ✓.
  The RECOMMENDED maximum sets an upper bound. Operating well below the maximum
  is compliant and represents a stricter implementation.
ASVS-V10-OAuth: PASS
Overall verdict: PASS

Node 9 — Formatter
{
  "sequence_id": "CB4-3.4-423",
  "overall_verdict": "PASS",
  "territories": [
    {"territory": "T-oauth-oidc", "verdict": "PASS", "arbiter": "ASVS-V10-OAuth"},
    {"territory": "T-rfc-oauth2", "verdict": "PASS", "arbiter": "RFC-OAuth2"}
  ]
}
```

**Reason:** RFC 6749 §4.1.2 sets a RECOMMENDED maximum of 10 minutes — it is an upper bound, not a range. A 2-second expiry is well within this maximum and more restrictive. Single-use enforcement adds additional protection. PASS. The operational concern about very tight windows is noted but is not a compliance finding.

**Phrasing-B:** Authorization codes expire in 2 seconds and are single-use. RFC 6749 §4.1.2 recommends a maximum of 10 minutes. Does a 2-second expiry violate the recommendation, or does the maximum set a ceiling that shorter values satisfy?

---

## Pair[CB4-3.4-424]

**A:** Session fixation — cookie-based application with a subtle gap. Application login flow: (1) user visits `/login`; (2) server sets a session cookie for CSRF protection during the login form; (3) user submits credentials via the login form; (4) credentials validated; (5) server calls `session.regenerate()` which creates a new session ID; (6) the new session ID is set in the session cookie; (7) the old session ID is invalidated. However: the `session.regenerate()` call happens 200 milliseconds after credential validation while other post-login operations run in parallel. The session ID is valid during this 200ms window.

**B:**
```
Node 1 — Domain Splitter
Input: session regenerated after login but with 200ms delay; session valid during window.
Domains detected: [asvs-session]
Primary domain: asvs-session
Routing: single-domain

Node 2 — Domain Translator
Domain: asvs-session
Normative mapping: ASVS V7.2.3 (session fixation)
Territory: T-asvs-session

Node 3 — Scheme Planner
Territories: [T-asvs-session]
Key question: does 200ms delay in session regeneration create a V7.2.3 finding?

Node 4 — Domain Processor
T-asvs-session analysis:
  Session regeneration is performed ✓ — new session ID generated post-authentication
  Old session invalidated ✓
  200ms window between credential validation and regeneration:
    In practice: V7.2.3 requires that the old session not continue in an
    authenticated state after successful login. The 200ms delay means:
    (a) The old session ID exists in a transitional state for 200ms
    (b) In a session fixation attack, the attacker would need to make a
        request using the known pre-auth session ID within this 200ms window
        to potentially access the authenticated session before regeneration
    This is a theoretical race condition, not a practical exploitation window
    for typical session fixation attacks (which are not timing-based).
  V7.2.3 assessment: the regeneration is performed; the delay is implementation
    detail; session fixation attacks do not typically exploit millisecond windows.
  Finding: PASS — session regeneration is implemented; the 200ms delay is a
    coding quality concern but not a meaningful session fixation vulnerability.

Node 5 — Applier
V7.2.3 → PASS
  Session regeneration performed with old session invalidated ✓
  200ms delay: code quality concern; not a practical fixation vulnerability

Node 6–8:
T-asvs-session: PASS
Overall verdict: PASS (with implementation quality note)

Node 9 — Formatter
{
  "sequence_id": "CB4-3.4-424",
  "overall_verdict": "PASS",
  "territories": [
    {
      "territory": "T-asvs-session",
      "verdict": "PASS",
      "arbiter": "ASVS-V7-Session",
      "note": "Session regeneration performed; 200ms delay is implementation detail, not a practical fixation window"
    }
  ]
}
```

**Reason:** V7.2.3 requires session ID regeneration after authentication — this is performed. The 200ms delay between validation and regeneration is an implementation quality concern but not a meaningful session fixation vulnerability: fixation attacks are not typically millisecond-precision timing attacks. The core requirement (regeneration + old session invalidation) is satisfied. PASS with a code quality note.

**Phrasing-B:** Session is regenerated after login with old session invalidated — correct fixation prevention. But there's a 200ms delay between credential validation and regeneration while other operations run. Does the 200ms window make V7.2.3 non-compliant?

---

## Pair[CB4-3.4-425]

**A:** RSA key size and rotation combined trace. JWT service RS256. Key: 4096-bit RSA generated 18 months ago, no rotation since generation. kid claim present; JWKS endpoint serves current key. No rotation schedule. Team: "We used a 4096-bit key specifically so we wouldn't need to rotate as often. Larger key = longer security lifetime."

**B:**
```
Node 1 — Domain Splitter
Input: RS256 with 4096-bit RSA key; 18 months old; no rotation; team argues larger key = longer lifetime.
Domains detected: [asvs-token, rfc-jwt]
Primary domain: asvs-token
Routing: multi-domain

Node 2 — Domain Translator
Normative mapping:
  Key size: RFC 7518 §3.3 (MUST ≥ 2048 bits)
  Key rotation: ASVS V9.2 (lifecycle management)
Territories: T-asvs-token, T-rfc-jwt

Node 3 — Scheme Planner
Key questions:
  (a) Does 4096-bit key satisfy RFC 7518 §3.3? Yes — 4096 > 2048 minimum.
  (b) Does "larger key = longer rotation cadence" argument hold?

Node 4 — Domain Processor
T-asvs-token analysis:
  RFC 7518 §3.3: 4096-bit RSA ≥ 2048-bit MUST minimum ✓ — key size compliant
  V9.2 key rotation: 18 months without rotation; no rotation schedule
    Team argument ("larger key = longer lifetime"):
      (a) Key size affects resistance to cryptanalytic attacks (factoring).
          4096-bit is stronger than 2048-bit against brute-force attacks. ✓
      (b) However, key rotation is about operational security: exposure window,
          compromise detection, and defense-in-depth — not just cryptographic strength.
      (c) A compromised 4096-bit key is just as exploitable as a compromised
          2048-bit key — strength only affects the difficulty of mathematical
          attacks, not operational compromise (theft, insider access, etc.).
      (d) Larger key size does not reduce the operational compromise risk window.
    Finding: CONCERN — 4096-bit key is cryptographically strong (PASS for size);
      no rotation schedule remains a key lifecycle management gap (CONCERN)

T-rfc-jwt:
  RFC 7518 §3.3: PASS (4096-bit exceeds minimum)
  No normative rotation frequency requirement in RFC

Node 5 — Applier
RFC 7518 §3.3: PASS (4096-bit)
V9.2 rotation: CONCERN (18 months; no schedule; argument rejected)

Node 6 — Result Translator
T-asvs-token: CONCERN
T-rfc-jwt: PASS
Overall: CONCERN

Node 7 — Arbiter Selector
CONCERN → ASVS-V9-Token

Node 8 — Arbiter Unit (ASVS-V9-Token)
RFC 7518 §3.3: PASS — 4096-bit RSA satisfies the MUST minimum.
V9.2 (key rotation): CONCERN
Argument rejected: "larger key = longer rotation cadence."

Key size and key rotation serve different security purposes:
(1) Key size: resistance to cryptanalytic attacks (RSA factoring).
    4096-bit provides stronger resistance to mathematical attacks than 2048-bit.
(2) Key rotation: operational exposure management — limits the blast radius
    of a key compromise event regardless of how the key was obtained.
A compromised 4096-bit key (e.g., stolen from Vault, insider exfiltration)
enables token forgery for the entire unrotated lifetime. Key size does not
protect against operational compromise.

Finding: CONCERN — key size PASS; rotation hygiene CONCERN.
18 months without rotation; no schedule; kid infrastructure present.
Recommendation: establish annual or event-triggered rotation schedule.
Overall verdict: CONCERN

Node 9 — Formatter
{
  "sequence_id": "CB4-3.4-425",
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
          "evidence": "4096-bit key; 18 months no rotation; no schedule",
          "argument_rejected": "Larger key size does not substitute for rotation lifecycle management"
        }
      ]
    },
    {"territory": "T-rfc-jwt", "verdict": "PASS", "arbiter": "RFC-JWT"}
  ]
}
```

**Reason:** 4096-bit RSA correctly satisfies RFC 7518 §3.3 (PASS). The argument "larger key = longer rotation cadence" is rejected because key size and key rotation serve different purposes: size addresses mathematical attacks; rotation addresses operational compromise (theft, insider access). A stolen 4096-bit key is just as dangerous as a stolen 2048-bit key for the duration of the unrotated lifetime. V9.2 CONCERN for absent rotation schedule.

**Phrasing-B:** RS256 with 4096-bit RSA key, 18 months old, no rotation, kid/JWKS infrastructure in place. Argument: bigger key means we can rotate less often because it's harder to crack. Does larger key size justify extended rotation intervals, or is the rotation gap still a CONCERN?

---

## Pair[CB4-3.4-426]

**A:** Combined session fixation and token endpoint TLS. Application: session-based (not JWT). Login flow: new session ID generated on authentication, old invalidated. Token endpoint: HTTPS directly on the service (not TLS-terminated upstream). Exact redirect URIs. PKCE S256. State validated. Argon2id m=65536. Vault for secrets. Security Champion + OWASP checklist.

**B:**
```
Node 1 — Domain Splitter
Input: correct session fixation prevention; token endpoint direct TLS; full OAuth + other controls.
Domains detected: [asvs-session, rfc-oauth2, oauth-oidc, asvs-authentication, ssdf-process]
Routing: multi-domain

Node 2 — Domain Translator
All normative mappings applied across territories.

Node 3 — Scheme Planner
Territories: [T-asvs-session, T-rfc-oauth2, T-oauth-oidc, T-asvs-authentication, T-ssdf-process]
All evidence appears positive; verify each.

Node 4 — Domain Processor
T-asvs-session:
  V7.2.3: new session ID on auth; old invalidated ✓ → PASS

T-rfc-oauth2:
  RFC 6749 §3.2: HTTPS directly on token endpoint ✓ → PASS
  (Direct TLS termination at service — not upstream proxy)

T-oauth-oidc:
  V10.2.1 ✓; V10.4.6 ✓; V10.2.6 ✓ → PASS

T-asvs-authentication:
  V6.2.5 Argon2id ✓ → PASS

T-ssdf-process:
  PW.1 Vault ✓; PW.3.2 Security Champion + checklist ✓ → PASS

All territories: PASS

Node 5–8: All PASS — confirmatory
Overall verdict: PASS

Node 9 — Formatter
{
  "sequence_id": "CB4-3.4-426",
  "overall_verdict": "PASS",
  "territories": [
    {"territory": "T-asvs-session", "verdict": "PASS"},
    {"territory": "T-rfc-oauth2", "verdict": "PASS"},
    {"territory": "T-oauth-oidc", "verdict": "PASS"},
    {"territory": "T-asvs-authentication", "verdict": "PASS"},
    {"territory": "T-ssdf-process", "verdict": "PASS"}
  ]
}
```

**Reason:** Session fixation correctly prevented (new session ID on login, old invalidated). Token endpoint uses direct TLS (not upstream proxy termination) — satisfies RFC 6749 §3.2. All other controls correct. Five-territory PASS.

**Phrasing-B:** Session ID regenerated on login, token endpoint uses direct HTTPS (not proxied), PKCE S256, Argon2id, Vault, Security Champion. Do these controls pass all applicable territories?

---

## Pair[CB4-3.4-427]

**A:** Scope validation review. OAuth authorization server. Clients request authorization with a `scope` parameter. The authorization server: (1) validates that requested scopes exist; (2) checks the client's registered allowed scopes; (3) if the requested scope is a subset of the client's allowed scopes, issues a token with the requested scopes; (4) if the requested scope includes scopes the client is not authorized for, the authorization server issues a token with only the scopes the client is authorized for, without rejecting the request. Team: "We silently narrow scope rather than rejecting — better UX."

**B:**
```
Node 1 — Domain Splitter
Input: OAuth scope validation; server silently narrows unauthorized scopes rather than rejecting.
Domains detected: [oauth-oidc, rfc-oauth2]
Primary domain: rfc-oauth2
Routing: multi-domain

Node 2 — Domain Translator
Normative mapping:
  Scope handling: RFC 6749 §3.3 (scope parameter)
  Silent narrowing: RFC 6749 §3.3 RECOMMENDED behavior
Territories: T-rfc-oauth2, T-oauth-oidc

Node 3 — Scheme Planner
Key question: does silent scope narrowing conform to RFC 6749 §3.3?

Node 4 — Domain Processor
T-rfc-oauth2 analysis:
  RFC 6749 §3.3: "If the issued access token scope is different from the one
    requested by the client, the authorization server MUST include the 'scope'
    response parameter to inform the client of the actual scope granted."
  Silent narrowing:
    - The server issues a token with fewer scopes than requested.
    - RFC 6749 §3.3 requires the `scope` response parameter when the issued
      scope differs from the requested scope — this informs the client.
    - If the server silently narrows and INCLUDES the `scope` in the response:
      RFC 6749 §3.3 is satisfied.
    - If the server silently narrows without including `scope` in response:
      RFC 6749 §3.3 MUST is violated.
  Assessment: the submission says "silently narrows" — does not specify whether
    `scope` is included in the response. CONCERN: the narrowing behavior is
    potentially compliant if scope is returned, but the "silent" description
    suggests the response scope may not be returned.
  Finding: CONCERN — silent narrowing without explicit scope response
    may violate RFC 6749 §3.3 MUST; UX argument does not override MUST

Node 5 — Applier
RFC 6749 §3.3: CONCERN
  Evidence: scope narrowing; "silently" suggests scope not returned in response
  Finding: if scope response omitted when narrowed, §3.3 MUST violated

Node 6–8:
T-rfc-oauth2: CONCERN (MUST may be violated depending on scope response inclusion)
T-oauth-oidc: CONCERN
Overall verdict: CONCERN

Node 8 — Arbiter Unit (RFC-OAuth2)
RFC 6749 §3.3: CONCERN
The authorization server may silently narrow scope — this is within RFC 6749's
scope handling framework IF the actual issued scope is returned in the response.
The "silently" description creates ambiguity about whether the `scope` response
parameter is included. RFC 6749 §3.3 MUST requires scope inclusion when the
issued scope differs from the requested scope.

Clarification needed: does the token response include `scope` when narrowed?
  - If yes and scope is returned: PASS (RFC 6749 §3.3 satisfied)
  - If no: REPROCESS (MUST violation — client unaware of narrowing)

Finding: CONCERN pending clarification. The UX argument ("better UX") does
not create an exception to RFC 6749 §3.3.
Overall verdict: CONCERN
```

**Reason:** Silent scope narrowing is within RFC 6749's framework when the actual issued scope is returned in the response (`scope` parameter in token response) — this is a MUST when the issued scope differs from the requested scope. "Silently" suggests the client may not be informed of the narrowing. CONCERN because clarification is needed: scope returned → PASS; scope not returned → REPROCESS. The UX argument does not override RFC MUSTs.

**Phrasing-B:** Our authorization server silently narrows unauthorized scopes rather than rejecting the request — better UX. Does RFC 6749 §3.3 allow silent scope narrowing, or does the client need to be informed when the issued scope differs from the requested scope?

---

## Pair[CB4-3.4-428]

**A:** Comprehensive review — all ext16 patterns correctly resolved plus scope validation. Application: token endpoint HTTPS directly on service; session ID regenerated on login; authorization codes 5-minute single-use; RS256 with 2048-bit key rotated annually; introspection endpoint with Bearer auth; scope response included when narrowed; state validated; PKCE S256 (universal); exact redirect URIs; Argon2id; Vault; Security Champion + checklist; back-channel SSO logout; refresh tokens revoked on logout.

**B:**
```
Node 1 — Domain Splitter
Input: comprehensive multi-territory review; all ext16 patterns at correct state.
Domains detected: all territories
Routing: multi-territory

Node 2–4 — Processing all territories:
T-rfc-oauth2:
  RFC 6749 §3.2: HTTPS directly ✓ — PASS
  RFC 6749 §4.1.2: 5-min single-use codes ✓ — PASS
  RFC 6749 §3.3: scope narrowing includes scope response ✓ — PASS
  RFC 7662 §2.1: Bearer auth on introspection ✓ — PASS

T-oauth-oidc:
  V10.2.1 ✓; V10.4.6 universal ✓; V10.2.6 ✓; V10.3 ✓ → PASS

T-asvs-session:
  V7.2.3 (session ID regenerated on login) ✓
  V7.3.1 (back-channel logout) ✓ → PASS

T-asvs-token:
  V9.1.1 RS256 allowlist ✓
  RFC 7518 §3.3: 2048-bit ✓
  V9.2: annual rotation ✓ → PASS

T-asvs-authentication:
  V6.2.5 Argon2id ✓ → PASS

T-ssdf-process:
  PW.1 Vault ✓; PW.3.2 Security Champion + checklist ✓ → PASS

All territories: PASS
Overall verdict: PASS

Node 9 — Formatter
{
  "sequence_id": "CB4-3.4-428",
  "overall_verdict": "PASS",
  "territories": [
    {"territory": "T-rfc-oauth2", "verdict": "PASS"},
    {"territory": "T-oauth-oidc", "verdict": "PASS"},
    {"territory": "T-asvs-session", "verdict": "PASS"},
    {"territory": "T-asvs-token", "verdict": "PASS"},
    {"territory": "T-asvs-authentication", "verdict": "PASS"},
    {"territory": "T-ssdf-process", "verdict": "PASS"}
  ]
}
```

**Reason:** All ext16 patterns correctly implemented: token endpoint direct TLS, session ID regenerated on login, short-lived single-use auth codes, scope response returned on narrowing, Bearer auth on introspection, 2048-bit RS256 with annual rotation. Six-territory comprehensive PASS.

**Phrasing-B:** Token endpoint direct HTTPS, session ID regenerated on login, 5-min single-use codes, scope returned when narrowed, introspection with Bearer auth, 2048-bit RS256 annually rotated, universal PKCE, Argon2id, Vault, Security Champion. Does this implementation pass all territories?

---

## File Summary

| Pair ID | Description | Stratum | Arbiter | Verdict |
|---------|-------------|---------|---------|---------|
| CB4-3.4-421 | RFC 6749 §3.2: API gateway TLS + internal HTTP "trusted network" — REPROCESS | C | RFC-OAuth2 + V10-OAuth | REPROCESS |
| CB4-3.4-422 | V7.2.3 stateless JWT: no pre-auth session — fixation surface absent → PASS | C | ASVS-V7-Session | PASS |
| CB4-3.4-423 | RFC 6749 §4.1.2: 2-second auth codes — PASS (maximum sets ceiling, not floor) | C | RFC-OAuth2 + V10-OAuth | PASS |
| CB4-3.4-424 | V7.2.3: session regenerated with 200ms delay — PASS (regeneration performed; delay not exploitable) | C | ASVS-V7-Session | PASS |
| CB4-3.4-425 | RFC 7518 §3.3 4096-bit PASS + "large key = no rotation" argument rejected → CONCERN | C | ASVS-V9-Token + RFC-JWT | CONCERN |
| CB4-3.4-426 | Session fixation correct + direct TLS + all other controls → 5-territory PASS | C | Multiple | PASS |
| CB4-3.4-427 | RFC 6749 §3.3: silent scope narrowing without scope response → CONCERN | C | RFC-OAuth2 + V10-OAuth | CONCERN |
| CB4-3.4-428 | All ext16 patterns correctly implemented → 6-territory PASS | C | Multiple | PASS |
