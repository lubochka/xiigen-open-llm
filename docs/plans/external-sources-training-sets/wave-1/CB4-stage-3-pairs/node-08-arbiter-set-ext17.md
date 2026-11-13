# CB4 Stage 3 — Node 08 Arbiter Set Extension 17
# Sequences CB4-3.4-439 through CB4-3.4-446
# Stratum: A/B/D — baselines, team context, adversarial rejection
# Instances: 16 (8 base pairs × 2 phrasings)
# Running total after this file: 892 instances

---

## Pair[CB4-3.4-439]

**A:** CORS (Cross-Origin Resource Sharing) configuration review on the OAuth authorization server. The authorization server's token endpoint responds with `Access-Control-Allow-Origin: *` (wildcard). The team: "Wildcard CORS is simpler to configure. The token endpoint uses POST and requires Authorization header or body credentials — CORS preflight will block unauthorized origins anyway."

**B:**
```
Arbiter: RFC-OAuth2 + ASVS-V10-OAuth

Token endpoint CORS: REPROCESS
Team argument rejected.

The token endpoint with `Access-Control-Allow-Origin: *` creates a cross-origin
request risk:
(1) RFC 6749 and OAuth security best practices require that the token endpoint
    restrict CORS to authorized origins. Wildcard CORS (`*`) means any origin
    can make cross-origin requests to the token endpoint.
(2) Team argument ("CORS preflight blocks unauthorized origins"):
    This is incorrect. Simple CORS requests (certain POST requests with
    Content-Type: application/x-www-form-urlencoded — the standard OAuth
    token request format) do not trigger a CORS preflight.
    A malicious page on any origin can make a direct token endpoint POST
    without a preflight check — the wildcard CORS response allows the
    browser to deliver the response to the malicious script.
(3) The token endpoint returns access tokens and refresh tokens in response.
    A cross-site script that can make token requests and read responses
    (allowed by wildcard CORS) could be used in CSRF attacks against the
    token endpoint.
(4) Correct implementation: `Access-Control-Allow-Origin` should list only
    the specific origins that legitimately need to call the token endpoint
    (typically the first-party SPA origin).

Evidence gap: wildcard CORS on token endpoint.
Overall verdict: REPROCESS
```

**Reason:** Wildcard CORS (`*`) on the token endpoint is a security gap. The team's preflight argument is factually wrong for OAuth token requests — `application/x-www-form-urlencoded` POST requests do not trigger CORS preflight. A malicious origin can make direct token requests and read the response when `*` is set. CORS should be restricted to authorized origins.

**Phrasing-B:** Token endpoint has Access-Control-Allow-Origin: *. We rely on CORS preflight to block unauthorized cross-origin requests. Does wildcard CORS on the token endpoint create a REPROCESS, and is the preflight argument correct?

---

## Pair[CB4-3.4-440]

**A:** CORS review — correctly configured. Authorization server token endpoint: `Access-Control-Allow-Origin: https://app.example.com`. Only the first-party SPA origin is allowed. Authorization endpoint: `Access-Control-Allow-Origin: *` (wildcard). The team: "The token endpoint is restricted. The authorization endpoint uses the redirect flow and doesn't return tokens in responses — it just redirects. Wildcard on the authorization endpoint is fine."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2

Token endpoint CORS (`Access-Control-Allow-Origin: https://app.example.com`): PASS
  Restricted to the legitimate first-party origin ✓
  Only https://app.example.com can make cross-origin requests to the token endpoint ✓

Authorization endpoint CORS (`Access-Control-Allow-Origin: *`): PASS with note
  The authorization endpoint typically handles browser redirects, not
  cross-origin XHR/fetch requests. The authorization code flow uses
  HTTP redirects, not JavaScript-initiated cross-origin requests.
  Wildcard CORS on the authorization endpoint has lower risk because:
  (1) The authorization endpoint responds with redirects, not token values.
  (2) Authorization code interception via CORS is not the primary attack vector
      (PKCE protects against code interception regardless of CORS).
  However: some OIDC flows (e.g., /.well-known/openid-configuration, JWKS)
    use wildcard CORS acceptably.
  Team's reasoning is sound for the authorization endpoint redirect behavior.

Finding: PASS (token endpoint correctly restricted; authorization endpoint
  wildcard acceptable given redirect-based flow and PKCE protection).
Overall verdict: PASS
```

**Reason:** Token endpoint CORS correctly restricted to the first-party SPA origin (PASS). Authorization endpoint wildcard CORS is acceptable because the authorization endpoint uses HTTP redirects (not cross-origin XHR returning tokens) and PKCE protects against authorization code interception. The team's reasoning for the authorization endpoint is correct.

**Phrasing-B:** Token endpoint: CORS restricted to https://app.example.com. Authorization endpoint: CORS wildcard. Team argues authorization endpoint redirects don't need restrictions since it returns redirects, not tokens. Is this CORS configuration correct?

---

## Pair[CB4-3.4-441]

**A:** Response type restriction review. Authorization server supports multiple `response_type` values: `code` (authorization code), `token` (implicit), `id_token` (implicit OIDC), and combinations. Clients may use any response_type. The team builds a mobile app and uses `response_type=token` (implicit flow with access token in fragment). Team: "The implicit flow is supported by RFC 6749. Our mobile app uses it for simplicity."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2

V10 / OAuth best practices (response_type=token — implicit flow): REPROCESS
Team argument rejected: "RFC 6749 supports it."

RFC 6749 does define the implicit grant (response_type=token), but:
(1) OAuth 2.0 Security Best Current Practice (RFC 9700, which obsoletes
    RFC 6749 implicit flow guidance) and OAuth 2.0 for Browser-Based Apps
    (RFC 9700 / BCP) explicitly state that the implicit flow SHOULD NOT be
    used. The access token is returned in the URI fragment, where it is:
    (a) exposed in browser history
    (b) potentially leaked via Referer headers
    (c) accessible to JavaScript on the page
(2) ASVS V10 requires that implicit flow (response_type=token) not be used.
    The authorization code flow with PKCE is the recommended replacement for
    all clients including mobile apps.
(3) For mobile apps specifically: authorization code + PKCE (using a custom
    URI scheme or claimed HTTPS redirect) is the correct pattern (RFC 8252).
(4) "RFC 6749 supports it" is true historically but OAuth 2.0 security best
    practices have deprecated the implicit flow.

Evidence gap: implicit flow (response_type=token) in use on mobile app.
Overall verdict: REPROCESS
```

**Reason:** RFC 6749 originally defined the implicit flow, but it is deprecated in OAuth 2.0 Security Best Current Practice. Access tokens in URI fragments create multiple exposure vectors. ASVS V10 requires implicit flow not be used. For mobile apps, RFC 8252 specifies authorization code + PKCE as the correct approach. "RFC 6749 supports it" is correct historically but does not justify using a deprecated pattern.

**Phrasing-B:** Mobile app uses response_type=token (implicit flow). RFC 6749 defines this flow. ASVS V10 and OAuth best practices have since deprecated implicit flow. Is using the implicit flow on a mobile app compliant, or is it a REPROCESS?

---

## Pair[CB4-3.4-442]

**A:** response_type restriction — correctly implemented. Authorization server: only `response_type=code` is supported. All other response_types (`token`, `id_token` in implicit mode, `code token`, etc.) are rejected with `error=unsupported_response_type`. Clients must use authorization code flow. PKCE S256 enforced on all code flows. The team: "We support only the code flow to eliminate implicit flow risks."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2

V10 (response_type restriction): PASS
  Only `response_type=code` supported ✓
  Implicit flow (`response_type=token`) not supported ✓
  OAuth 2.0 Security Best Current Practice compliant ✓
  PKCE S256 enforced on all code flows ✓

Restricting to `response_type=code` with PKCE is the current recommended
OAuth 2.0 pattern for all client types:
  - Eliminates access token exposure in URI fragments (implicit flow risk)
  - PKCE provides authorization code protection for public and confidential clients
  - Unsupported response_types rejected with correct error code ✓

Overall verdict: PASS
```

**Reason:** Restricting to authorization code flow only (rejecting implicit flow) is the recommended OAuth 2.0 Security Best Current Practice. Combined with PKCE, this eliminates implicit flow risks. PASS.

**Phrasing-B:** Authorization server allows only response_type=code with PKCE S256. Implicit flow and all other response_types are rejected. Is restricting to authorization code flow with PKCE the correct implementation?

---

## Pair[CB4-3.4-443]

**A:** Token binding review. Application issues JWT access tokens. The tokens are bearer tokens — any holder of a valid token can use it. The team is aware of DPoP (Demonstrating Proof-of-Possession, RFC 9449) but has not implemented it. Team: "DPoP requires clients to prove possession of a key pair. It's complex to implement on all clients. We use short-lived tokens (15-minute expiry) and HTTPS everywhere to mitigate bearer token theft."

**B:**
```
Arbiter: ASVS-V9-Token + RFC-JWT

V9 / Token binding (DPoP): PASS with note
DPoP (RFC 9449) provides sender-constraining for access tokens — the token
is bound to a client's ephemeral key pair, preventing token replay by a
different holder.

Assessment:
(1) DPoP is not a MUST requirement in ASVS V9 or OAuth 2.0 core specifications.
    It is a security enhancement defined in RFC 9449 as an optional extension.
(2) Short-lived access tokens (15 minutes) reduce the replay window for stolen
    bearer tokens ✓
(3) HTTPS everywhere prevents token interception in transit ✓
(4) The combination of short-lived tokens and HTTPS provides meaningful
    baseline mitigation for bearer token theft.

Finding: PASS — DPoP is not required by V9 or any normative MUST requirement.
The team's mitigations (short-lived tokens, HTTPS) represent an acceptable
baseline. DPoP would provide stronger security (defense-in-depth) but its
absence is not a compliance gap.

Note: for high-security environments (financial services, healthcare regulated
applications), DPoP is strongly recommended as defense-in-depth. For general
applications, the baseline mitigations are adequate.
Overall verdict: PASS
```

**Reason:** DPoP is an optional security enhancement, not a normative MUST in ASVS V9 or OAuth 2.0 core. Short-lived tokens and HTTPS provide adequate baseline mitigation for bearer token theft. The absence of DPoP is not a compliance gap. PASS with a recommendation note for high-security environments.

**Phrasing-B:** Bearer tokens, 15-minute expiry, HTTPS everywhere. No DPoP implementation. DPoP is complex for all our clients. Is the absence of DPoP a compliance gap, or do short-lived tokens + HTTPS provide sufficient baseline mitigation?

---

## Pair[CB4-3.4-444]

**A:** Token revocation endpoint review. Application provides a `POST /oauth/revoke` endpoint per RFC 7009. The endpoint accepts access tokens and refresh tokens and invalidates them immediately. The endpoint requires no authentication — any party can call it with a token string to revoke it. Team: "RFC 7009 says the revocation endpoint doesn't need the same authentication as introspection. Anyone should be able to revoke a token they have — revocation is always safe to do."

**B:**
```
Arbiter: RFC-OAuth2

RFC 7009 §2.1 (token revocation endpoint): PASS with note
Team's reasoning is substantially correct for the core RFC 7009 model:

RFC 7009 §2.1 states that token revocation requests "MUST be authenticated."
However, the authentication requirement in RFC 7009 is for client authentication
(the client presents its credentials when revoking its own tokens), not for
general anonymous access.

Assessment:
  RFC 7009 §2.1: the revocation endpoint MUST require the client to authenticate
  using the same methods used at the token endpoint (client credentials).
  This means:
  - A client revoking its own token must authenticate as that client ✓
  - An anonymous caller (no client credentials) revoking a token: this is what
    the team describes — which does NOT satisfy RFC 7009 §2.1.

Team argument ("revocation is always safe"):
  Revocation is safe from a confidentiality standpoint (you're invalidating,
  not reading). However, RFC 7009 §2.1 still requires client authentication
  to: (a) verify the revocation is from an authorized client, (b) support
  token binding to specific clients.

Finding: CONCERN — RFC 7009 §2.1 requires client authentication on revocation.
Unauthenticated revocation is technically a §2.1 gap, though the security
impact is lower than unauthenticated introspection (no data disclosed).
Team's reasoning partially valid; MUST is still present.
Overall verdict: CONCERN
```

**Reason:** RFC 7009 §2.1 requires client authentication on the revocation endpoint — not for the same reasons as introspection (preventing data disclosure), but to verify the revocation is from an authorized client. Unauthenticated revocation is technically a MUST gap even if the security impact is lower (no data disclosed). CONCERN rather than REPROCESS: the security impact of unauthenticated revocation is significantly lower than unauthenticated introspection.

**Phrasing-B:** Token revocation endpoint: no client authentication required — any holder can revoke. RFC 7009. Team says revocation is always safe since you're just invalidating, not reading. Is unauthenticated revocation compliant with RFC 7009 §2.1?

---

## Pair[CB4-3.4-445]

**A:** PKCE downgrade attack review. Authorization server enforces PKCE S256 for all public clients. However: if a request includes `code_challenge_method=plain` in the authorization request, the server falls back to plain method validation. The fallback exists for "legacy client compatibility." Clients that specify S256 get S256 validation. Clients that specify plain (or omit) get plain validation. Team: "We support both methods for compatibility — clients choose their method."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2

V10.4.6 (PKCE S256): REPROCESS
RFC 7636 §4.2 (S256 requirement): REPROCESS

Team argument rejected: "clients choose their method."

RFC 7636 §4.2: "If the client is capable of using 'S256', it MUST use 'S256'."
More critically for the authorization server:
(1) An authorization server that allows clients to downgrade to plain creates
    a downgrade attack vector. A man-in-the-middle or attacker who can influence
    the authorization request parameters can force plain method, then intercept
    the code_verifier (which equals the code_challenge in plain method).
(2) V10.4.6 requires S256 to be enforced — the server should reject plain
    method requests, not accept them as a fallback.
(3) "Legacy client compatibility" is not an exception to S256 enforcement.
    RFC 7636 §4.2 requires S256 from capable clients; if a client is using
    plain, either it is not RFC 7636 compliant or it is attempting a downgrade.
(4) The authorization server accepting plain provides no protection for clients
    that are downgraded to plain — PKCE security is nullified in those flows.

Evidence gap: plain method accepted as fallback; S256 not enforced universally.
Overall verdict: REPROCESS
```

**Reason:** An authorization server that accepts `code_challenge_method=plain` as a fallback creates a PKCE downgrade attack: an attacker who can influence the authorization request can force plain method, then intercept the code_verifier (which is identical to the code_challenge in plain method). V10.4.6 and RFC 7636 §4.2 require S256 enforcement — the server should reject plain method, not accept it for "compatibility."

**Phrasing-B:** PKCE enforced but server accepts plain method as a fallback for legacy clients. Clients specify S256 or plain; server validates accordingly. Is accepting plain as a fallback a REPROCESS for V10.4.6?

---

## Pair[CB4-3.4-446]

**A:** Comprehensive review covering all new patterns from ext17. Application: token endpoint CORS restricted to first-party origin; authorization server supports only response_type=code (no implicit); PKCE S256 enforced with plain rejected; DPoP not implemented (15-min tokens + HTTPS); revocation endpoint requires client authentication; scope enforced at resource server; RS256 2048-bit key; session ID regenerated on login; auth codes 5-minute single-use; introspection with Bearer auth; Argon2id; Vault; Security Champion + checklist.

**B:**
```
Arbiter: Multiple

T-rfc-oauth2:
  RFC 6749 §3.2 (token endpoint TLS) ✓
  RFC 7662 §2.1 (introspection auth) ✓
  RFC 7009 §2.1 (revocation client auth) ✓
  RFC 6749 §3.3 (scope enforced at resource) ✓
  response_type=code only (implicit rejected) ✓
  → PASS

T-oauth-oidc:
  V10.2.6 ✓; V10.4.6 S256 (plain rejected) ✓; V10.2.1 ✓; V10.3 ✓ → PASS
  CORS: token endpoint restricted to first-party origin ✓ → PASS

T-asvs-token:
  V9.1.1 RS256 ✓; RFC 7518 §3.3 2048-bit ✓; V9.2 rotation ✓
  DPoP absent: PASS (optional enhancement; baseline mitigations present) ✓
  → PASS

T-asvs-session: V7.2.3 session ID regenerated ✓ → PASS
T-asvs-authentication: V6.2.5 Argon2id ✓ → PASS
T-ssdf-process: PW.1 Vault ✓; PW.3.2 Security Champion + checklist ✓ → PASS

All territories: PASS
Overall verdict: PASS
```

**Reason:** All ext17 patterns correctly implemented: CORS restricted, implicit flow disabled, plain PKCE rejected, scope enforced at resource server, revocation with client auth, DPoP absent but baseline mitigations present (acceptable PASS). Comprehensive multi-territory PASS.

**Phrasing-B:** Token endpoint CORS to first-party only, code flow only (no implicit), PKCE S256 with plain rejected, scope enforced at resource server, revocation with client auth, no DPoP but 15-min tokens + HTTPS, 2048-bit RS256, session ID regenerated, Argon2id, Vault, Security Champion. Does this implementation pass all territories?

---

## File Summary

| Pair ID | Description | Stratum | Arbiter | Verdict |
|---------|-------------|---------|---------|---------|
| CB4-3.4-439 | CORS wildcard on token endpoint "preflight blocks it" — REPROCESS (preflight wrong for OAuth POST) | A/D | RFC-OAuth2 + V10-OAuth | REPROCESS |
| CB4-3.4-440 | Token endpoint CORS restricted; authorization endpoint wildcard → PASS (redirects, not tokens) | B | V10-OAuth + RFC-OAuth2 | PASS |
| CB4-3.4-441 | response_type=token (implicit) mobile app "RFC 6749 supports it" — REPROCESS (deprecated) | A | V10-OAuth + RFC-OAuth2 | REPROCESS |
| CB4-3.4-442 | response_type=code only (implicit rejected) + PKCE → PASS | A | V10-OAuth + RFC-OAuth2 | PASS |
| CB4-3.4-443 | DPoP absent; 15-min tokens + HTTPS mitigations → PASS (DPoP optional, not MUST) | B/D | V9-Token + RFC-JWT | PASS |
| CB4-3.4-444 | RFC 7009 §2.1: unauthenticated revocation "revocation is safe" → CONCERN | B/D | RFC-OAuth2 | CONCERN |
| CB4-3.4-445 | PKCE downgrade: server accepts plain as fallback → REPROCESS (downgrade attack vector) | A/D | V10-OAuth + RFC-OAuth2 | REPROCESS |
| CB4-3.4-446 | All ext17 patterns correctly implemented → comprehensive PASS | B | Multiple | PASS |
