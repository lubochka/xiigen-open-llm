# CB4 Stage 3 — Node 08 Arbiter Set Extension 18
# Sequences CB4-3.4-465 through CB4-3.4-472
# Stratum: A/B/D — baselines, team context, adversarial rejection
# Instances: 16 (8 base pairs × 2 phrasings)
# Running total after this file: 944 instances

---

## Pair[CB4-3.4-465]

**A:** redirect_uri at token exchange review. Application registers exact redirect URIs. At token exchange, the server checks that the submitted redirect_uri matches the one stored with the authorization code. However: the comparison is case-insensitive (`redirect_uri.lower() == stored_uri.lower()`). Team: "URIs are case-insensitive for the scheme and host components. Our case-insensitive comparison handles both uppercase and lowercase URIs correctly."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2

V10.2.1 / RFC 6749 §4.1.3 (redirect_uri comparison): CONCERN
Team argument is partially correct but the implementation is overly permissive.

RFC 3986 (URI syntax) normalization rules:
  - Scheme (https) and host (example.com) are case-insensitive ✓
  - Path component (/oauth/callback) is case-SENSITIVE ✓

Case-insensitive comparison of the ENTIRE URI:
  - Correctly handles scheme/host case variants ✓
  - Incorrectly treats path variants as equal:
    /oauth/callback == /OAuth/Callback == /OAUTH/CALLBACK ✗
  - An attacker who registers /OAuth/CALLBACK and submits
    /oauth/callback would pass the comparison (or vice versa)
  - This could allow redirects to paths that differ only in case

Finding: CONCERN
The comparison is overly broad — case-insensitivity applied to the full URI
rather than per-component. RFC 6749 §4.1.3 requires exact match. While the
security impact is limited (attacker would need a registered case variant),
the comparison does not satisfy the exact-match principle.

Remediation: normalize scheme+host to lowercase (per RFC 3986); treat path
as case-sensitive; compare path components exactly.
Overall verdict: CONCERN
```

**Reason:** RFC 3986 makes only the scheme and host case-insensitive; the path is case-sensitive. Applying case-insensitive comparison to the entire URI violates the exact-match principle for path components. CONCERN rather than REPROCESS because the practical exploitation window is narrow (requires a registered case-variant path), but the comparison logic is incorrect.

**Phrasing-B:** Token exchange redirect_uri verified with case-insensitive comparison. RFC 3986 says scheme and host are case-insensitive. Is full-URI case-insensitive comparison correct for redirect_uri verification?

---

## Pair[CB4-3.4-466]

**A:** OAuth scope naming review. Authorization server: scopes are named `read`, `write`, `admin`. No hierarchical scope structure. Client with scope=write can call GET and POST endpoints. Client with scope=read can only call GET endpoints. Client with scope=admin can call all endpoints plus management APIs. The resource server enforces scope per-endpoint. A separate internal service calls the resource server with a machine-to-machine token using scope=admin for all operations. Team: "Broad admin scope for internal services is simpler — they need everything."

**B:**
```
Arbiter: ASVS-V8-AuthZ + ASVS-V10-OAuth

V8.1.2 (least privilege / deny-by-default): CONCERN
An internal service with scope=admin "for everything it might need":
(1) Violates the principle of least privilege — the token carries more
    privilege than any individual operation requires.
(2) If the internal service token is compromised (stolen from memory,
    intercepted, exfiltrated from logs), the attacker has admin-scope
    access to all endpoints.
(3) Scope should be minimized to what each operation actually requires —
    machine-to-machine tokens benefit from narrow scopes as much as
    human-facing tokens.

Finding level: CONCERN (not REPROCESS). Scope enforcement is correctly
implemented at the resource server — the gap is the scope design for
internal services, not an absence of enforcement.

Recommendation: issue different tokens per operation type; or use
narrower scopes (scope=read:data,write:data instead of scope=admin).
Overall verdict: CONCERN
```

**Reason:** Scope enforcement is correctly implemented (PASS for the mechanism). The gap is that internal services use an `admin` scope for all operations rather than minimum required scopes. This violates least privilege. CONCERN because enforcement works — the design of what scope is requested is the gap, not the enforcement itself.

**Phrasing-B:** Scope enforcement is correct at resource server. Internal machine-to-machine service uses scope=admin for all its calls because it needs different operations at different times. Is broad scope for internal services a concern, or is scope enforcement at the server sufficient?

---

## Pair[CB4-3.4-467]

**A:** Token caching review. API gateway caches access token introspection results for 5 minutes. If the same access token is presented within 5 minutes of a previous introspection, the gateway returns the cached `active: true` result without re-checking the authorization server. A token that is revoked (e.g., via the revocation endpoint) will still be accepted for up to 5 minutes after revocation due to the cache.

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2

V10.3 (token revocation) / RFC 7009: CONCERN
The revocation endpoint is implemented — tokens can be revoked.
However, a 5-minute introspection cache creates a window where revoked
tokens remain effective:
(1) User logs out → token revoked at authorization server ✓
(2) Revoked token presented at API gateway within 5 minutes of last introspection
(3) Gateway serves cached `active: true` → token accepted ✓ (to attacker)
(4) Effective token revocation is delayed by up to 5 minutes

Finding level: CONCERN
The revocation mechanism exists (V10.3 partially satisfied). The cache creates
a bounded post-revocation window. CONCERN because:
- 5-minute window is bounded and short
- The revocation infrastructure exists (not an absence of revocation)
- Introspection caching is a common operational optimization
- RFC 7009 / V10.3 do not explicitly prohibit bounded cache windows

However: for high-security operations (financial, healthcare), even 5-minute
windows may be unacceptable. The cache window should be documented as a
known limitation.

Overall verdict: CONCERN
```

**Reason:** Token revocation is implemented but a 5-minute introspection cache creates a post-revocation window. CONCERN because the revocation mechanism exists, the window is bounded (5 minutes), and caching is a common operational pattern. REPROCESS would apply if there were no revocation mechanism at all. The cache window should be documented and reviewed for the application's risk tolerance.

**Phrasing-B:** Token revocation is implemented. API gateway caches introspection results for 5 minutes — revoked tokens remain effective for up to 5 minutes after revocation. Is the 5-minute revocation delay a CONCERN or a REPROCESS?

---

## Pair[CB4-3.4-468]

**A:** Introspection caching review — correctly managed. API gateway: introspection results cached with TTL equal to the token's remaining lifetime (`cache_ttl = token_exp - now`). When a token is revoked, the gateway calls `DELETE /cache/{token_hash}` to invalidate the cached entry immediately. Revocation events propagate to the gateway via a Redis pub/sub channel within 100ms. Revoked tokens are not served from cache after the invalidation propagates.

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2

V10.3 (token revocation effectiveness): PASS
The caching architecture correctly manages revocation:
(1) TTL-based cache (exp - now): cache expires naturally with token ✓
(2) Active cache invalidation on revocation via Redis pub/sub ✓
(3) 100ms propagation delay: negligible for practical security purposes ✓
(4) Revoked tokens are not served after invalidation ✓

This satisfies the intent of V10.3: revoked tokens cease to be effective
within the propagation delay (100ms). The revocation mechanism is complete
and the cache does not create a meaningful bypass window.

Overall verdict: PASS
```

**Reason:** Cache TTL tied to token expiry plus active invalidation via pub/sub on revocation satisfies V10.3. The 100ms propagation delay is negligible. This is the correct way to implement introspection caching without creating a post-revocation window. PASS.

**Phrasing-B:** Introspection cache TTL = token's remaining lifetime. On revocation, gateway cache invalidated via Redis pub/sub within 100ms. Does this caching architecture satisfy V10.3 token revocation effectiveness?

---

## Pair[CB4-3.4-469]

**A:** Dynamic client registration review. Authorization server supports RFC 7591 (OAuth Dynamic Client Registration). Any party can call `POST /oauth/register` to register a new OAuth client without authentication. Registered clients receive a `client_id` and optionally a `client_secret`. Team: "RFC 7591 supports open dynamic registration — new clients can self-register. This is the spec behavior."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2

Dynamic client registration (RFC 7591): REPROCESS
Team argument: "RFC 7591 supports open dynamic registration."

RFC 7591 §3.1 states that the registration endpoint MAY be protected with
an initial access token. RFC 7591 explicitly supports both open and protected
registration, but:
(1) Open (unauthenticated) dynamic registration creates significant risks:
    - Any party can register clients with arbitrary redirect_uris
    - Attackers can register clients to create phishing infrastructure
      (malicious redirect URIs that receive authorization codes)
    - Open registration undermines the entire redirect_uri registration
      security model (V10.2.1 exact registration) by allowing anyone to
      register new URIs at will
(2) ASVS V10 requires that client registration be controlled — open dynamic
    registration without any authentication or vetting contradicts the
    principle of exact redirect URI registration.
(3) "The spec supports it" refers to RFC 7591's flexibility — it does not
    mean open registration is a recommended or secure deployment choice.
    RFC 7591 §2 explicitly anticipates that servers may require initial
    access tokens for registration.

Evidence gap: unauthenticated dynamic client registration enabled.
Overall verdict: REPROCESS
```

**Reason:** RFC 7591 allows open registration as an option but does not recommend it. Unauthenticated dynamic registration allows anyone to register clients with arbitrary redirect URIs, undermining the security model of exact redirect URI registration. "The spec supports it" is rejected — the spec supports both open and protected registration; open registration is the insecure option.

**Phrasing-B:** Dynamic client registration open to anyone without authentication — per RFC 7591. Is open dynamic registration compliant, or does allowing unauthenticated client registration create a REPROCESS?

---

## Pair[CB4-3.4-470]

**A:** Dynamic client registration — correctly protected. Authorization server: RFC 7591 dynamic registration endpoint requires an initial access token. Initial access tokens are issued only to vetted parties after a manual approval process. Registered clients receive a `client_id`; confidential clients receive a `client_secret`. Registered redirect_uris are stored exactly and matched exactly at authorization time.

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2

Dynamic client registration (RFC 7591 with initial access token): PASS
(1) Initial access token required ✓ — unauthenticated registration prevented
(2) Initial access tokens issued after manual vetting ✓ — controlled onboarding
(3) Registered redirect_uris stored exactly ✓ (V10.2.1 satisfied)
(4) Exact match at authorization time ✓

RFC 7591 §3.1: initial access token is the recommended mechanism for
protected dynamic registration. The implementation correctly uses RFC 7591
in its secure configuration.

Overall verdict: PASS
```

**Reason:** Dynamic client registration with an initial access token (issued after vetting) correctly implements the protected form of RFC 7591. Registered redirect URIs are stored and matched exactly (V10.2.1). This is the secure deployment of dynamic registration. PASS.

**Phrasing-B:** Dynamic registration requires an initial access token issued after manual vetting. Registered redirect URIs stored and matched exactly. Is initial-access-token-protected dynamic registration compliant with RFC 7591 and V10.2.1?

---

## Pair[CB4-3.4-471]

**A:** Combined revocation caching and scope review. Token revocation endpoint implemented with client authentication. Introspection cache: 15-minute TTL with no active cache invalidation on revocation (cache entries expire naturally). Scope enforcement at resource server: all endpoints check scope claim. Internal admin service uses scope=admin for all calls.

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2 + ASVS-V8-AuthZ

T-oauth-oidc / T-rfc-oauth2:
  Revocation endpoint (RFC 7009 §2.1): client auth required ✓ — PASS (mechanism)
  Introspection cache 15-min TTL, no active invalidation:
    15 minutes post-revocation window — same analysis as 5-minute case
    but with longer window.
    15 minutes: CONCERN (bounded window; revocation infrastructure exists;
    but 15 minutes is a longer exposure than 5 minutes)
    Level: CONCERN — same type as 5-min cache but larger window

T-asvs-authorization:
  Scope enforcement at resource server ✓ — PASS
  Internal service scope=admin: CONCERN (least-privilege violation; same
    analysis as scope naming pair)

T-rfc-oauth2 verdict: CONCERN
T-asvs-authorization verdict: CONCERN
Overall verdict: CONCERN (two CONCERNs; no REPROCESS)
```

**Reason:** Revocation with client auth is correct (PASS). 15-minute cache without active invalidation is CONCERN (larger window than 5-minute but same category). Scope enforcement correct; admin-scope-for-everything is CONCERN (least-privilege). Two CONCERNs, no REPROCESS → overall CONCERN.

**Phrasing-B:** Revocation with client auth, 15-minute introspection cache no active invalidation, scope enforced correctly, internal service uses scope=admin for all calls. Two CONCERNs — does that make the overall verdict CONCERN or REPROCESS?

---

## Pair[CB4-3.4-472]

**A:** Comprehensive review — all ext18 patterns correctly implemented. Application: redirect_uri comparison: scheme+host case-normalized, path compared exactly; minimum-scope tokens for all internal services (separate token per operation scope); introspection cache with TTL=token-remaining + active pub/sub invalidation on revocation; dynamic client registration protected by initial access token (vetting required); 5-minute auth codes single-use; redirect_uri verified at exchange; all prior controls correct (PKCE S256 no-plain, code-only, CORS restricted, Argon2id, Vault, Security Champion, etc.).

**B:**
```
Arbiter: Multiple

T-oauth-oidc:
  V10.2.1: exact redirect URIs; RFC 3986-correct comparison (scheme/host
    normalized, path exact) ✓ — PASS
  RFC 6749 §4.1.3: redirect_uri at exchange ✓ — PASS
  response_type=code only ✓; V10.4.6 S256 no-plain ✓ — PASS
  Dynamic registration: initial access token required ✓ — PASS

T-rfc-oauth2:
  RFC 7009 §2.1: revocation client auth ✓ — PASS
  RFC 7662 §2.1: introspection Bearer auth ✓ — PASS
  Introspection cache: TTL-based + pub/sub invalidation (100ms) ✓ — PASS
  V10.3: revocation effective; cache does not create bypass window ✓ — PASS

T-asvs-authorization:
  V8.1.2: minimum-scope tokens per operation ✓ (least privilege) — PASS

T-asvs-authentication: Argon2id ✓ — PASS
T-ssdf-process: Vault ✓; Security Champion ✓ — PASS

All territories: PASS
Overall verdict: PASS
```

**Reason:** All ext18 patterns correctly implemented: RFC 3986-correct URI comparison, minimum-scope internal tokens, cache with active invalidation, initial-access-token-protected dynamic registration, redirect_uri at exchange, PKCE no-plain. Comprehensive multi-territory PASS.

**Phrasing-B:** Correct URI comparison per RFC 3986 (scheme/host normalized, path exact), minimum-scope tokens for internal services, cache with pub/sub invalidation, protected dynamic registration, redirect_uri at exchange, all prior controls correct. Does this comprehensive implementation pass all territories?

---

## File Summary

| Pair ID | Description | Stratum | Arbiter | Verdict |
|---------|-------------|---------|---------|---------|
| CB4-3.4-465 | redirect_uri full-URI case-insensitive comparison → CONCERN (path is case-sensitive per RFC 3986) | B/D | V10-OAuth + RFC-OAuth2 | CONCERN |
| CB4-3.4-466 | Scope enforcement PASS + internal service scope=admin → CONCERN (least privilege) | B | V8-AuthZ + V10-OAuth | CONCERN |
| CB4-3.4-467 | Revocation implemented + 5-min introspection cache → CONCERN (bounded post-revocation window) | B | V10-OAuth + RFC-OAuth2 | CONCERN |
| CB4-3.4-468 | Cache TTL=token-remaining + pub/sub invalidation on revocation → PASS | A | V10-OAuth + RFC-OAuth2 | PASS |
| CB4-3.4-469 | RFC 7591 open dynamic registration "spec supports it" → REPROCESS | A | V10-OAuth + RFC-OAuth2 | REPROCESS |
| CB4-3.4-470 | RFC 7591 dynamic registration with initial access token + vetting → PASS | A | V10-OAuth + RFC-OAuth2 | PASS |
| CB4-3.4-471 | Revocation PASS + 15-min cache no-invalidation CONCERN + scope=admin CONCERN → CONCERN | B | V10-OAuth + V8-AuthZ | CONCERN |
| CB4-3.4-472 | All ext18 patterns correctly implemented → comprehensive PASS | B | Multiple | PASS |
