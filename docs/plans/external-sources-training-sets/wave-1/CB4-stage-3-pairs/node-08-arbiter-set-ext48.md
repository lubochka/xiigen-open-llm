# Node 08 — Arbiter Set Extension 48
# Sub-phase: 3.4 Security and Authorization Model
# Pair ID prefix: CB4-3.4-
# Sequences: 1245–1252 (16 instances — 8 base pairs × 2 phrasings)
# Sources: RFC 6749 §4.1.3, OIDC Dynamic Registration §3.2,
#          CWE-601 / ASVS V13.4.1, RFC 7009 §1,
#          OIDC Core §5.3.4, RFC 6749 §4.1.2,
#          ASVS V6.3.2, RFC 9101 §10.3
# Running total after this file: 2504 instances (1252 base pairs)
# Formula check: 120 + (1252 − 60) × 2 = 120 + 2384 = 2504 ✓

---

## Pair[CB4-3.4-1245]/A

A client includes `redirect_uri=https://app.example.com/callback` in its
authorization request. At the token endpoint, the same client submits the
authorization code but omits the `redirect_uri` parameter entirely. The
authorization server locates the code, validates the client credentials, and
issues an access token without checking the redirect_uri. The AS team argues
that since only one redirect_uri is registered for this client, the check is
redundant — there is no other URI the code could have been issued for.

## Pair[CB4-3.4-1245]/B

REPROCESS. RFC 6749 §4.1.3 MUST: when the redirect_uri was included in the
authorization request, it MUST be provided by the client in the token request,
and the AS MUST verify that it is identical to the value used in the authorization
request. This MUST applies procedurally and does not contain a single-registered-URI
exception. The AS team's redundancy argument is rejected: the comparison requirement
exists regardless of how many redirect URIs are registered — the procedural check
is what provides the normative guarantee; skipping it because the answer "should be
obvious" substitutes AS assumption for the required verification step. MUST violated;
REPROCESS.

## Pair[CB4-3.4-1245]/Reason

RFC 6749 §4.1.3 MUST: redirect_uri REQUIRED at token endpoint when present in auth
request; AS MUST verify identical value; single-registration redundancy argument
rejected → REPROCESS.

## Pair[CB4-3.4-1245]/Phrasing-B

Verdict REPROCESS. RFC 6749 §4.1.3: if redirect_uri was in the authorization
request, the client MUST supply it at the token endpoint and the AS MUST verify
identity. No single-registration exception exists; the check is procedural regardless
of registration count. MUST violated. REPROCESS.

---

## Pair[CB4-3.4-1246]/A

An OIDC provider's dynamic client registration endpoint receives a registration
request for a confidential client with `"token_endpoint_auth_method": "client_secret_basic"`.
The AS registers the client and returns a registration response containing `client_id`,
`client_secret_expires_at`, and the echoed `token_endpoint_auth_method` — but the
`client_secret` field is absent from the response. The development team argues the
client can request its secret separately via a client read endpoint.

## Pair[CB4-3.4-1246]/B

REPROCESS. OIDC Dynamic Client Registration §3.2 MUST: when `token_endpoint_auth_method`
requires a client secret (such as `client_secret_basic`, `client_secret_post`, or
`client_secret_jwt`), the AS MUST include `client_secret` in the registration response.
This is the only point at which the secret is transmitted; it cannot be retrieved
after the initial registration response because the secret must be stored hashed
after issuance. The separate-read-endpoint argument is rejected: any client read
endpoint that returns the plain client secret would itself violate ASVS V9.2.2
(which requires hashed storage — a stored hash cannot be reversed to the original
secret for transmission). Without the secret in the registration response, the
confidential client cannot authenticate at the token endpoint. MUST violated;
REPROCESS.

## Pair[CB4-3.4-1246]/Reason

OIDC Dynamic Registration §3.2 MUST: client_secret required in registration
response when auth method requires it; secret not separately retrievable after
issuance (stored hashed); separate-read-endpoint argument rejected → REPROCESS.

## Pair[CB4-3.4-1246]/Phrasing-B

Verdict REPROCESS. OIDC Dynamic Client Registration §3.2 requires `client_secret`
in the registration response when the auth method requires a secret. Post-issuance
retrieval is architecturally impossible under ASVS V9.2.2 hashed storage. The
separate-endpoint argument is rejected. MUST violated. REPROCESS.

---

## Pair[CB4-3.4-1247]/A

An OIDC authorization server's login page accepts a `return_url` query parameter
that specifies where the user should be redirected after successful authentication.
The AS echoes the `return_url` value directly into an HTTP redirect response after
the user authenticates, without validating it against any registered or allowlisted
URI. An attacker crafts a malicious authorization URL with
`return_url=https://attacker.example.com/steal` — after the user authenticates,
the AS redirects the victim to the attacker-controlled domain. The team argues
that the feature is internal and not security-critical because it only operates
after successful authentication.

## Pair[CB4-3.4-1247]/B

REPROCESS. CWE-601 (URL Redirection to Untrusted Site) and ASVS V13.4.1: the
authorization server MUST validate all redirect targets against a configured
allowlist before issuing any redirect. An AS is a high-trust origin — users are
in an authenticated session and active OAuth authorization flows are in progress;
redirects from AS origin carry the implicit trust of the AS. An unvalidated
`return_url` redirect enables phishing (victim sent to attacker domain while
trusting the AS URL they navigated to) and can expose authorization codes if
the attacker URL appears after an OAuth callback. The "post-authentication only"
argument is rejected: open redirects are a security vulnerability regardless of
authentication state; CWE-601 applies unconditionally to untrusted redirect
targets. MUST violated; REPROCESS.

## Pair[CB4-3.4-1247]/Reason

CWE-601 / ASVS V13.4.1 MUST validate redirect targets against allowlist; AS open
redirect via unvalidated return_url after authentication; post-auth-only argument
rejected → REPROCESS.

## Pair[CB4-3.4-1247]/Phrasing-B

Verdict REPROCESS. ASVS V13.4.1 and CWE-601 require redirect target validation
against a configured allowlist. An AS open redirect post-authentication exposes
users to phishing in the context of an active authorization flow. The post-auth
scope argument is rejected. REPROCESS.

---

## Pair[CB4-3.4-1248]/A

An authorization server's revocation endpoint (`/revoke`) accepts and processes
`token_type_hint=refresh_token` requests, revoking refresh tokens and triggering
family revocation. However, the endpoint returns `error=unsupported_token_type`
when presented with `token_type_hint=access_token`. The AS team argues that access
tokens have a 15-minute TTL, making access token revocation unnecessary — clients
should simply wait for expiry.

## Pair[CB4-3.4-1248]/B

CONCERN. RFC 7009 §1 states that implementations "SHOULD support revocation of
access tokens" — a SHOULD-level obligation. The absence of access token revocation
is therefore a guidance-level gap, not a MUST violation. The AS team's TTL argument
is partially accepted as a compensating control: a 15-minute access token TTL limits
the post-compromise exposure window. However, the guidance gap remains: there are
threat scenarios where 15 minutes is a significant exploitation window (credential
rotation after account compromise, insider threat, emergency account lockout), and
RFC 7009 explicitly recommends access token revocation support. No MUST is violated;
CONCERN not REPROCESS.

## Pair[CB4-3.4-1248]/Reason

RFC 7009 §1 SHOULD support access token revocation; absent support = guidance-level
gap; 15-min TTL is a partial compensating control but does not eliminate SHOULD gap
→ CONCERN.

## Pair[CB4-3.4-1248]/Phrasing-B

Verdict CONCERN. RFC 7009 §1 designates access token revocation support as SHOULD —
guidance-level. The 15-minute TTL is a meaningful but incomplete compensating
control; post-revocation exposure windows matter in incident response scenarios.
No MUST violated. CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1249]/A

An OIDC provider issues ID tokens where the `sub` claim is set to a UUID-based
pseudonymous identifier (e.g., `sub: "a3f2b1d0-..."`). The same provider's
UserInfo endpoint, however, returns a `sub` field set to the user's email address
(e.g., `sub: "user@example.com"`) for the same authenticated user. An RP performing
the OIDC Core §5.3.4 sub cross-check rejects the response because the values do
not match. The AS team argues the mismatch is a legacy issue and that the RP
should use the `sub` from whichever source it prefers.

## Pair[CB4-3.4-1249]/B

REPROCESS. OIDC Core §5.3.4 MUST: the RP MUST verify that the `sub` claim in the
UserInfo response is identical to the `sub` claim in the ID token. This is not an
RP-choice comparison — it is a security requirement to ensure the UserInfo response
belongs to the same subject as the ID token, preventing UserInfo injection attacks.
Mismatched `sub` values across ID token and UserInfo response mean either the AS
has an identity consistency bug or the UserInfo response was substituted by an
intermediary. The AS team's RP-preference argument is rejected: the sub cross-check
is a MUST in the OIDC Core standard; the RP's correct rejection proves the AS is
non-conformant. The AS must ensure the same `sub` identifier is used consistently
across both the ID token and the UserInfo response. MUST violated; REPROCESS.

## Pair[CB4-3.4-1249]/Reason

OIDC Core §5.3.4 MUST: sub in UserInfo response must be identical to sub in ID
token; mismatch = non-conformance; RP preference argument rejected → REPROCESS.

## Pair[CB4-3.4-1249]/Phrasing-B

Verdict REPROCESS. OIDC Core §5.3.4 MUST: UserInfo sub must be identical to ID
token sub. A mismatch either indicates an AS identity consistency bug or a UserInfo
injection; the sub cross-check is a normative security requirement, not an RP
preference. MUST violated. REPROCESS.

---

## Pair[CB4-3.4-1250]/A

An authorization server's authorization code exchange endpoint checks single-use
status with a simple `SELECT ... WHERE code=? AND used=false` followed by
`UPDATE ... SET used=true` — two separate database operations. Under load, two
simultaneous token exchange requests for the same authorization code arrive within
a 2-millisecond window. Due to race conditions in the non-atomic check-then-set
operation, both requests read `used=false` before either writes `used=true`, and
both receive valid access tokens. The team argues this is an infrastructure
scalability issue rather than a security violation.

## Pair[CB4-3.4-1250]/B

REPROCESS. RFC 6749 §4.1.2 MUST: authorization codes MUST be single-use. The
mechanism by which single-use is enforced is an implementation concern, but the
single-use property is a MUST-level requirement. A race condition in the non-atomic
check-and-set operation causes two access tokens to be issued for the same
authorization code — the MUST is violated regardless of the root cause being a
database concurrency issue. The team's infrastructure framing is rejected: the
security requirement is that each code may be exchanged exactly once; any
implementation that allows two successful exchanges violates this MUST. The
remediation is an atomic compare-and-swap or database-level unique constraint,
not a classification as an infrastructure concern. MUST violated; REPROCESS.

## Pair[CB4-3.4-1250]/Reason

RFC 6749 §4.1.2 MUST single-use authorization codes; non-atomic check-and-set
race condition causes two successful exchanges; MUST violated regardless of
concurrency root cause; infrastructure framing rejected → REPROCESS.

## Pair[CB4-3.4-1250]/Phrasing-B

Verdict REPROCESS. RFC 6749 §4.1.2 MUST: authorization codes MUST be single-use.
A race condition in the non-atomic check-then-set operation results in two valid
access tokens for the same code — the MUST is violated. Root cause classification
as infrastructure does not change the security verdict. REPROCESS.

---

## Pair[CB4-3.4-1251]/A

An OIDC provider's MFA (Multi-Factor Authentication) step allows users to enter
TOTP codes without any server-side rate limiting on failed attempts. An attacker
who has compromised a user's password can enumerate all possible TOTP values by
making 30 POST requests per second to the MFA verification endpoint, covering
the 6-digit code space (1,000,000 values) within approximately 9 hours for a
30-second TOTP window. The AS team argues that the attacker would need the
password first, and that TOTP with no rate limiting is still stronger than
password-only authentication.

## Pair[CB4-3.4-1251]/B

CONCERN. ASVS V6.3.2 states that implementations SHOULD rate-limit authenticator
code entry attempts and lock out accounts after repeated failures. SHOULD is
guidance-level — there is no unconditional RFC MUST that mandates a specific
lockout or rate-limiting mechanism for TOTP verification. The AS team's
"password first" argument is partially relevant: the attacker must have the
password, limiting the scope of who can attempt this attack. However, the guidance
gap is meaningful: without server-side rate limiting, a stolen password plus
automated TOTP enumeration can result in full account compromise within the TOTP
validity window. The ASVS guidance recommends rate limiting as defense-in-depth.
No MUST violated; CONCERN not REPROCESS.

## Pair[CB4-3.4-1251]/Reason

ASVS V6.3.2 SHOULD rate-limit TOTP attempts; no unconditional MUST for specific
mechanism; password-requirement partially accepted as scope limiter; guidance gap
→ CONCERN.

## Pair[CB4-3.4-1251]/Phrasing-B

Verdict CONCERN. ASVS V6.3.2 SHOULD rate-limit MFA code entry — guidance-level.
No MUST mandates a specific mechanism. The "password first" argument is partially
accepted as a scope limiter; the guidance gap on rate limiting remains meaningful.
CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1252]/A

An OIDC provider supports OIDC JAR (RFC 9101) request objects. A client submits
a signed request object with an `exp` claim set to 30 days in the future from
the time of submission — the AS validates the signature and processes the
authorization request. The AS team argues that setting a long expiry is a client
choice and the spec does not define a maximum.

## Pair[CB4-3.4-1252]/B

CONCERN. RFC 9101 §10.3 states that the expiry time MUST NOT be too far in the
future to limit their validity period — however, this MUST NOT is qualified by
the phrase "too far in the future" without providing a numeric definition, and
is accompanied by SHOULD-level guidance to set short expiry windows. The AS team's
argument that the spec does not define a maximum is partially correct — there is
no numeric MUST ceiling for JAR `exp`. A 30-day JAR expiry creates a meaningful
replay window: if the signed request object is intercepted, it remains usable for
30 days. However, the absence of a numeric MUST maximum means this is a guidance-
level gap. No unconditional MUST with a numeric threshold is violated. CONCERN
not REPROCESS.

## Pair[CB4-3.4-1252]/Reason

RFC 9101 §10.3 qualitative MUST NOT "too far in the future" + SHOULD-level
guidance for short JAR expiry; no numeric MUST maximum; 30-day exp creates replay
window; guidance-level gap → CONCERN.

## Pair[CB4-3.4-1252]/Phrasing-B

Verdict CONCERN. RFC 9101 §10.3 prohibits JAR exp "too far in the future" but
does not specify a numeric maximum — a qualitative guidance boundary. The AS team
is partially correct that no MUST ceiling is numerically defined. A 30-day replay
window is a guidance-level concern. CONCERN not REPROCESS.
