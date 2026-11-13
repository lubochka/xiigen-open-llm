# CB4 Stage 3 — Node 08 Arbiter Set Extension 16
# Sequences CB4-3.4-413 through CB4-3.4-420
# Stratum: A/B/D — baselines, team context, adversarial rejection
# Instances: 16 (8 base pairs × 2 phrasings)
# Running total after this file: 840 instances

---

## Pair[CB4-3.4-413]

**A:** RSA key size review. JWT signing with RS256. RSA key pair is 2048 bits. The team is evaluating whether to upgrade to 4096-bit keys. They ask: "Is 2048-bit RSA sufficient for RS256, or is a larger key size required?"

**B:**
```
Arbiter: RFC-JWT + ASVS-V9-Token

RFC 7518 §3.3 (RSA key size for RS256): PASS
RFC 7518 §3.3 states: "A key of size 2048 bits or larger MUST be used with
these algorithms." 2048-bit RSA meets the minimum MUST threshold exactly.

V9.1 (token signing): PASS — 2048-bit RS256 is compliant.

Architectural note: 2048-bit RSA is currently considered adequate by NIST
(SP 800-131A) through at least 2030. 4096-bit keys provide a stronger security
margin and are recommended for new deployments or long-key-lifetime scenarios,
but are not required by RFC 7518 for RS256.

The team's consideration of upgrading to 4096-bit keys is good security planning
and proactive hygiene — not remediation of a current gap.

Overall verdict: PASS
```

**Reason:** RFC 7518 §3.3 MUST requires ≥ 2048 bits for RS256. 2048-bit exactly meets this threshold (PASS). 4096-bit provides a stronger margin and is recommended for new deployments but is not mandated. Planning an upgrade is positive hygiene, not remediation of a current gap. This mirrors the bcrypt→Argon2id migration pattern: current state is compliant; upgrade is improvement.

**Phrasing-B:** RS256 with a 2048-bit RSA key. We're considering upgrading to 4096-bit. Does 2048-bit meet RFC 7518 §3.3 requirements, or does planning an upgrade imply the current key size is non-compliant?

---

## Pair[CB4-3.4-414]

**A:** Token endpoint security review. OAuth token endpoint (`POST /oauth/token`) is accessible over HTTP (not HTTPS) on the internal network. The team states: "The token endpoint is only on our internal network — no public internet exposure. TLS is terminated at the load balancer at the network perimeter. Traffic inside our network is plaintext between services."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2

V10.x (token endpoint transport security): REPROCESS
RFC 6749 §3.2 requires that the token endpoint use TLS.
Team argument rejected: "internal network + perimeter TLS = equivalent."

RFC 6749 §3.2: "The endpoint URI MUST use TLS."
This is an unconditional MUST — it applies regardless of network location:
(1) TLS termination at the load balancer means the token endpoint itself is
    not protected by TLS — the plaintext segment is between the load balancer
    and the service.
(2) Any internal network principal (other services, compromised hosts, internal
    attackers) can intercept token endpoint traffic if it is plaintext inside
    the network.
(3) OAuth tokens (authorization codes, access tokens, refresh tokens) transmitted
    in plaintext over the internal network are interceptable.
(4) "Internal network" is not TLS — the requirement is that the endpoint itself
    use TLS, not that the network perimeter does.

Correct implementation: terminate TLS at the service itself (or use mutual TLS
for internal service-to-service communication). Token endpoints must not transmit
credentials over plaintext channels regardless of network location.

Evidence gap: token endpoint serves HTTP internally; TLS not terminated at service.
Overall verdict: REPROCESS
```

**Reason:** RFC 6749 §3.2 requires TLS on the token endpoint — unconditionally. TLS termination at the load balancer leaves the token endpoint serving HTTP internally, which is not TLS. Authorization codes and tokens exchanged in plaintext on the internal network are interceptable by any internal network principal. The "internal network" argument mirrors the introspection endpoint pattern: network location does not substitute for transport security.

**Phrasing-B:** Token endpoint is HTTP-only on the internal network. TLS terminates at the load balancer. Internal traffic is plaintext. Does perimeter TLS satisfy RFC 6749 §3.2's requirement for TLS on the token endpoint?

---

## Pair[CB4-3.4-415]

**A:** Session fixation review. Application login flow: (1) user accesses `/login`; (2) application issues a pre-authentication session ID stored in a cookie; (3) user submits credentials; (4) credentials validated; (5) application continues to use the same session ID after authentication — no new session is created on successful login. The team: "We set the session cookie at the start of the login flow. It persists through authentication. The same session ID is used pre- and post-login."

**B:**
```
Arbiter: ASVS-V7-Session

V7.2.3 (session fixation prevention): REPROCESS
Session fixation attack pattern:
(1) Attacker obtains a pre-authentication session ID (e.g., by visiting /login
    and capturing the set-cookie response).
(2) Attacker sends the victim a link with the known session ID embedded.
(3) Victim authenticates with the attacker's known session ID.
(4) Post-authentication, the server continues using the same session ID.
(5) Attacker, who knows the session ID, now has an authenticated session.

V7.2.3 requires that the session ID be regenerated after successful authentication.
The current implementation: same session ID pre- and post-login — V7.2.3 violated.

Evidence gap: session ID not regenerated on successful authentication.
Remediation: on successful credential validation, invalidate the pre-authentication
session and issue a new session ID for the authenticated session.
Overall verdict: REPROCESS
```

**Reason:** V7.2.3 (CWE-384, Session Fixation) requires session ID regeneration on authentication. Using the same session ID before and after login allows an attacker who obtained the pre-auth session ID to hijack the authenticated session. Regenerating the session ID on login is the mitigation.

**Phrasing-B:** Same session ID used before and after login — no regeneration on authentication. Is this a V7.2.3 session fixation finding?

---

## Pair[CB4-3.4-416]

**A:** Session fixation — correctly handled. Application login flow: (1) pre-authentication state stored in server-side storage with a temporary ID; (2) user submits credentials; (3) on successful authentication, a new session is generated with a new random session ID; (4) pre-authentication temporary ID is invalidated; (5) authenticated session cookie set with the new session ID. HttpOnly and Secure flags on the session cookie.

**B:**
```
Arbiter: ASVS-V7-Session

V7.2.3 (session fixation prevention): PASS
New session ID generated on successful authentication ✓
Pre-authentication session invalidated on login ✓
Session fixation attack mitigated: an attacker who obtained the pre-auth
session ID cannot use it post-login — it is invalidated at the moment of
authentication.

V7.1 (session cookie security):
HttpOnly flag ✓ — prevents JavaScript access to session cookie
Secure flag ✓ — cookie transmitted only over HTTPS

Overall verdict: PASS
```

**Reason:** Correct session fixation prevention: new session ID generated and old one invalidated on authentication. HttpOnly + Secure flags on the session cookie are additional positive controls. PASS.

**Phrasing-B:** New session ID generated on login, pre-auth session invalidated, HttpOnly and Secure flags set. Is the session fixation prevention correctly implemented?

---

## Pair[CB4-3.4-417]

**A:** OAuth state parameter review. The state parameter is generated as a random UUID, included in the authorization request, stored server-side, and verified on callback — if the callback state does not match the stored state, the request is rejected. One team member argues: "The state parameter protects against CSRF. But we already have CSRF tokens on all our HTML forms. Our SPA (single-page application) doesn't use HTML forms for the OAuth callback — there's no CSRF risk on the callback endpoint because SPAs don't use traditional form submissions."

**B:**
```
Arbiter: ASVS-V10-OAuth

V10.2.6 (state parameter): PASS — state is already implemented and validated.
The team member's argument is evaluated but does not affect the current verdict.

V10.2.6 requires the state parameter on OAuth flows. The application correctly:
  - Generates a random UUID state ✓
  - Includes it in the authorization request ✓
  - Stores it server-side ✓
  - Validates on callback ✓
  - Rejects mismatched state ✓

Regarding the team member's argument:
The state parameter serves multiple purposes beyond CSRF prevention:
(1) It protects against authorization code injection attacks.
(2) It allows applications to maintain state across the redirect (e.g., preserve
    the page the user was on before authentication).
(3) It prevents replay attacks on the authorization response.
Existing CSRF tokens on forms are separate from the OAuth state requirement.
V10.2.6 requires state specifically for OAuth flows regardless of other CSRF
protection mechanisms.

The argument is noted but irrelevant — the state is already correctly implemented.
Overall verdict: PASS
```

**Reason:** State is correctly implemented — the verdict is PASS regardless of the team member's argument. The argument is evaluated and rejected: state protects against OAuth-specific attacks beyond form-based CSRF (authorization code injection, replay). Existing form CSRF tokens are independent of V10.2.6. The correct answer is PASS because the implementation is correct, not because the argument has merit.

**Phrasing-B:** State parameter: random UUID, stored server-side, validated on callback. Team member argues that SPAs don't have CSRF risk, so state is unnecessary since we have other CSRF protection. Is the state correctly implemented, and does the SPA argument affect the verdict?

---

## Pair[CB4-3.4-418]

**A:** Authorization code expiry review. Authorization codes issued by the authorization server have a 10-minute expiry. Codes are single-use — once exchanged for a token, the code is invalidated. If the code is not used within 10 minutes, it is expired server-side. The team asks: "Is a 10-minute authorization code expiry compliant? RFC 6749 says codes SHOULD be short-lived."

**B:**
```
Arbiter: RFC-OAuth2 + ASVS-V10-OAuth

RFC 6749 §4.1.2 (authorization code lifetime): PASS with note.
RFC 6749 §4.1.2 states: "The authorization code MUST expire shortly after it
is issued to mitigate the risk of leaks. A maximum authorization code lifetime
of 10 minutes is RECOMMENDED."

Assessment:
  - 10-minute expiry: meets the RFC 6749 RECOMMENDED maximum exactly ✓
  - Single-use enforcement: correctly implemented ✓ (code invalidated after exchange)
  - Server-side expiry: correctly enforced ✓

Note on the team's question ("SHOULD be short-lived"):
  RFC 6749 §4.1.2 actually uses "RECOMMENDED" (not just SHOULD) for the specific
  10-minute maximum. 10 minutes is the boundary of the recommended range.
  Single-use enforcement is the more important security control — even a long-lived
  code is mitigated by single-use invalidation.

Recommendation: shorter expiry (1-5 minutes) is best practice; 10 minutes is
at the outer boundary of the recommendation but compliant.
Overall verdict: PASS
```

**Reason:** 10-minute authorization code expiry exactly meets RFC 6749 §4.1.2's RECOMMENDED maximum. Single-use enforcement is also correctly implemented. PASS — at the boundary of the recommendation, but compliant. Shorter expiry (1-5 minutes) is better practice.

**Phrasing-B:** Authorization codes expire in 10 minutes and are single-use. RFC 6749 recommends 10-minute maximum. Does a 10-minute code expiry with single-use enforcement satisfy RFC 6749 §4.1.2?

---

## Pair[CB4-3.4-419]

**A:** Comprehensive new-domain review covering introspection, key size, token endpoint TLS, session fixation, and state parameter. Application: token endpoint uses TLS (HTTPS); authorization code 5-minute expiry, single-use; state UUID generated, stored, validated; session ID regenerated on login; RS256 with 2048-bit key, rotated annually; introspection endpoint requires client_id + client_secret; PKCE S256 universal; aud validated; Vault for all secrets; Argon2id; /me/permissions; Security Champion + OWASP checklist.

**B:**
```
Arbiter: RFC-OAuth2 + ASVS-V10-OAuth + ASVS-V7-Session + ASVS-V9-Token + SSDF-PW + ASVS-V6-Auth

T-rfc-oauth2:
  RFC 6749 §3.2 (token endpoint TLS): PASS ✓ — HTTPS on token endpoint
  RFC 6749 §4.1.2 (auth code expiry): 5 min, single-use → PASS ✓
  RFC 7662 §2.1 (introspection auth): client_id+client_secret → PASS ✓

T-oauth-oidc:
  V10.2.6 (state): UUID, stored, validated → PASS ✓
  V10.4.6 (PKCE S256 universal): PASS ✓
  V10.5.1 (aud): PASS ✓

T-asvs-session:
  V7.2.3 (session fixation): new session ID on login → PASS ✓
  V7.3.1: (assumed — not detailed; context indicates correct implementation)

T-asvs-token:
  V9.1.1 RS256 allowlist ✓
  RFC 7518 §3.3: 2048-bit key ✓ (meets MUST minimum)
  V9.2: annual rotation ✓

T-ssdf-process:
  PW.1 Vault ✓; PW.3.2 Security Champion + checklist ✓

T-asvs-authentication:
  V6.2.5 Argon2id ✓

T-asvs-authorization:
  V8.2.1 /me/permissions ✓

All territories: PASS
Overall verdict: PASS
```

**Reason:** All new-domain patterns correctly implemented: TLS on token endpoint; 5-minute code expiry; client_id+secret on introspection; session ID regenerated on login; 2048-bit RS256; universal PKCE; aud validated. Comprehensive PASS covering all patterns introduced in ext15-ext16.

**Phrasing-B:** Token endpoint HTTPS, 5-min single-use auth codes, introspection with client auth, session ID regenerated on login, 2048-bit RS256 rotated annually, universal PKCE, aud validated, Vault, Argon2id, /me/permissions, Security Champion. Does this implementation pass all applicable territories?

---

## Pair[CB4-3.4-420]

**A:** Two adversarial arguments submitted together. (1) Token endpoint argument: "Our token endpoint uses HTTP internally. OAuth flows go through our API gateway which handles TLS. The gateway forwards requests to the token endpoint over HTTP on the internal network. RFC 6749 §3.2 TLS is satisfied at the gateway." (2) Session argument: "We don't regenerate the session ID on login because our session IDs are very long (256 bytes of random data) and are HTTP-only. A long random session ID is unguessable, so session fixation is not a realistic attack."

**B:**
```
Arbiter: RFC-OAuth2 + ASVS-V7-Session

Argument 1 — Gateway TLS satisfies RFC 6749 §3.2: REJECTED
V10.x / RFC 6749 §3.2: REPROCESS
  RFC 6749 §3.2 requires TLS at the token endpoint URI.
  TLS termination at an API gateway followed by HTTP forwarding to the
  token endpoint means:
  (1) The token endpoint itself is not TLS-protected.
  (2) Authorization codes, client credentials, and access tokens transmitted
      on the HTTP segment (gateway → service) are in plaintext.
  (3) This is identical to "load balancer TLS termination" — the same pattern
      rejected in this sub-domain.
  RFC 6749 §3.2 MUST is violated. Gateway TLS does not satisfy the token
  endpoint TLS requirement.

Argument 2 — Long session ID substitutes for V7.2.3 regeneration: REJECTED
V7.2.3: REPROCESS
  Session fixation does not require guessing the session ID:
  (1) In a session fixation attack, the attacker obtains the pre-authentication
      session ID legitimately (by visiting /login and capturing the cookie).
  (2) The attacker then coerces the victim into using that known session ID.
  (3) After authentication, if the session ID is not regenerated, the attacker's
      known ID is now authenticated.
  (4) Session ID length/entropy is irrelevant to session fixation — the attacker
      does not guess the ID, they obtain it directly.
  Session ID regeneration on authentication is the required mitigation.
  Long session IDs protect against session ID brute-force/guessing, not fixation.

Both arguments rejected.
Overall verdict: REPROCESS
```

**Reason:** Two adversarial arguments rejected in one pair. (1) Gateway TLS is the same pattern as load-balancer TLS termination — RFC 6749 §3.2 requires TLS at the endpoint itself. (2) Session ID length does not protect against fixation — fixation does not involve guessing; the attacker obtains the ID directly. Both confuse a different protection mechanism with the required one.

**Phrasing-B:** Two arguments: (1) Our API gateway handles TLS; the token endpoint is HTTP behind the gateway. Does gateway TLS satisfy RFC 6749 §3.2? (2) Our session IDs are 256 bytes of random data — too long to guess. Does long session ID entropy substitute for session ID regeneration on login?

---

## File Summary

| Pair ID | Description | Stratum | Arbiter | Verdict |
|---------|-------------|---------|---------|---------|
| CB4-3.4-413 | RFC 7518 §3.3: 2048-bit RS256 PASS; planning 4096-bit upgrade ≠ non-compliant | B/D | RFC-JWT + V9-Token | PASS |
| CB4-3.4-414 | RFC 6749 §3.2: token endpoint HTTP (internal); perimeter TLS argument → REPROCESS | A | RFC-OAuth2 + V10-OAuth | REPROCESS |
| CB4-3.4-415 | V7.2.3 session fixation: same session ID pre/post-login → REPROCESS | A | ASVS-V7-Session | REPROCESS |
| CB4-3.4-416 | V7.2.3 session fixation: new ID on login, old invalidated → PASS | A | ASVS-V7-Session | PASS |
| CB4-3.4-417 | V10.2.6 state correctly implemented; SPA CSRF argument irrelevant → PASS | B/D | ASVS-V10-OAuth | PASS |
| CB4-3.4-418 | RFC 6749 §4.1.2 auth code 10-min single-use → PASS (boundary of recommended) | A/B | RFC-OAuth2 + V10-OAuth | PASS |
| CB4-3.4-419 | All ext15+16 patterns correctly implemented → comprehensive PASS | B | Multiple | PASS |
| CB4-3.4-420 | Two adversarial rejections: gateway TLS ≠ RFC 6749 §3.2; long session ID ≠ fixation fix | D | RFC-OAuth2 + V7-Session | REPROCESS |
```

