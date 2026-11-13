# Node 08 — Arbiter Set Extension 43
# Sub-phase: 3.4 Security and Authorization Model
# Pair ID prefix: CB4-3.4-
# Sequences: 1115–1122 (16 instances — 8 base pairs × 2 phrasings)
# Sources: RFC 6749 §4.1.3/§3.1, RFC 9101 §10.2, RFC 9700 §4.13.2,
#          RFC 9068 §4, OIDC Core §5.3.1, RFC 7591 §2, CWE-862 / ASVS V8.1.1
# Running total after this file: 2244 instances (1122 base pairs)
# Formula check: 120 + (1122 − 60) × 2 = 120 + 2124 = 2244 ✓

---

## Pair[CB4-3.4-1115]/A

OAuth 2.0 token endpoint receives a POST request containing
`code=<auth_code>&redirect_uri=https://app.example.com/callback&client_id=app01&client_secret=<secret>`.
The `grant_type` parameter is entirely absent from the request body.
The authorization server infers that the client intends an authorization_code
grant based on the presence of the `code` parameter, processes the request
as authorization_code, and issues an access token with HTTP 200.

## Pair[CB4-3.4-1115]/B

REPROCESS. RFC 6749 §4.1.3 classifies `grant_type` as a REQUIRED parameter
in every token request. The authorization server MUST return
`error=invalid_request` (RFC 6749 §5.2) when a required parameter is absent;
it MUST NOT infer the grant type from the presence of other parameters.
Silently defaulting to authorization_code violates the REQUIRED status of the
parameter and introduces grant-type confusion when multiple grant types share
overlapping parameters.

## Pair[CB4-3.4-1115]/Reason

RFC 6749 §4.1.3 REQUIRED parameter; §5.2 `error=invalid_request` for absent
required parameter; AS MUST NOT default-process missing REQUIRED fields.

## Pair[CB4-3.4-1115]/Phrasing-B

Verdict REPROCESS. The authorization server silently inferred an
authorization_code grant from the presence of the `code` parameter, but
RFC 6749 §4.1.3 classifies `grant_type` as REQUIRED. Omission of a REQUIRED
parameter MUST trigger `error=invalid_request` under §5.2; the server's
inference behavior substitutes its own judgment for the explicit parameter
requirement and violates the specification unconditionally.

---

## Pair[CB4-3.4-1116]/A

Client sends an authorization request including a `request=` parameter
containing a JWT request object (RFC 9101 JAR — JWT-Secured Authorization
Request). The JWT carries `alg: none` in its JOSE header — it is unsigned,
with an empty signature component. The authorization server parses and accepts
the request object, treating the unsigned JWT as a valid JAR request, and
proceeds with the authorization flow using the claims from the object.

## Pair[CB4-3.4-1116]/B

REPROCESS. RFC 9101 §10.2 explicitly prohibits `alg=none` for signed request
objects: MUST NOT. Accepting an unsigned request object nullifies JAR's core
security guarantee — integrity protection of authorization parameters.
An unsigned request object provides no more protection than plain query
parameters; any on-path party can substitute the `request=` JWT with a
forged one carrying elevated scope or a different redirect_uri.

## Pair[CB4-3.4-1116]/Reason

RFC 9101 §10.2 MUST NOT accept `alg=none` for request objects; unsigned JAR
removes the integrity protection the specification was designed to provide.

## Pair[CB4-3.4-1116]/Phrasing-B

Verdict REPROCESS. RFC 9101 §10.2 provides an unconditional MUST NOT for
`alg=none` in signed request objects. The prohibition exists because JAR's
purpose is to provide integrity protection for authorization parameters beyond
what plain query parameters offer; an unsigned request object delivers neither
integrity nor authenticity guarantees and degrades to the same threat model as
an unprotected request. The authorization server MUST reject the request object
and return `request_not_supported` or `invalid_request_object`.

---

## Pair[CB4-3.4-1117]/A

Authorization server issues refresh tokens for authorization_code grant flows
but stores only the refresh token hash, associated user ID, and authorized
scope — no `client_id` binding is recorded with the refresh token. When any
authenticated confidential client presents the refresh token at the token
endpoint, the AS validates the token's hash and the client's credentials
independently and issues a new access token — regardless of which client
originally received the refresh token.

## Pair[CB4-3.4-1117]/B

CONCERN. RFC 6749 §10.4 and RFC 9700 §4.13.2 both use SHOULD language for
binding refresh tokens to the issuing client — neither imposes an
unconditional MUST. The gap is meaningful: if Client A is compromised,
its stolen refresh tokens can be exercised by Client B. However, the
standard does not provide a MUST-level anchor for this binding, making
this a guidance-level gap. CONCERN not REPROCESS.

## Pair[CB4-3.4-1117]/Reason

RFC 6749 §10.4 / RFC 9700 §4.13.2 SHOULD bind refresh token to issuing
client; SHOULD-level guidance gap — no unconditional MUST anchor → CONCERN.

## Pair[CB4-3.4-1117]/Phrasing-B

Verdict CONCERN. RFC 6749 §10.4 states the authorization server SHOULD bind
refresh tokens to the client to which they were issued; RFC 9700 §4.13.2
reiterates this as SHOULD. The cross-client usability gap is a real security
concern — particularly in multi-client deployments where a compromised client's
refresh tokens become usable system-wide — but the SHOULD-level normative
weight means this is guidance-level, not a MUST violation. CONCERN not
REPROCESS.

---

## Pair[CB4-3.4-1118]/A

Resource server receives a JWT access token containing valid `iss`, `sub`,
`aud`, `exp`, `scope`, and `jti` claims but no `iat` claim. The RS validates
`exp` (not yet expired), `iss` (trusted issuer), and `aud` (matches RS
identifier), then authorizes the request. No token issuance recency check is
performed because `iat` is absent.

## Pair[CB4-3.4-1118]/B

CONCERN. RFC 9068 §4 uses SHOULD language for the `iat` claim and for RS
recency validation — neither is a MUST. The RS has satisfied all MUST-level
validations in RFC 9068 §4 (`typ`, `iss`, `exp`, `aud`). The missing `iat`
removes a defense-in-depth control for clock-skew anomaly detection and
long-lived token replay analysis but does not constitute a MUST violation.
CONCERN not REPROCESS.

## Pair[CB4-3.4-1118]/Reason

RFC 9068 §4 SHOULD include `iat`; RS SHOULD validate recency; SHOULD-level
guidance gap only — all MUST validations satisfied → CONCERN.

## Pair[CB4-3.4-1118]/Phrasing-B

Verdict CONCERN. RFC 9068 §4 recommends that access tokens include an `iat`
claim and that the RS use it for recency checking, but both are expressed as
SHOULD. The RS fulfills every MUST-level validation from RFC 9068 §4: `typ`,
`iss`, `aud`, `exp`, and `nbf` (if present). The absence of `iat` deprives the
RS of a useful signal for detecting anomalous token age, but the gap is
guidance-level. CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1119]/A

OIDC provider's UserInfo endpoint (`/userinfo`) accepts HTTP GET requests
without an `Authorization: Bearer` header. Any caller can supply a `sub`
query parameter (e.g., `GET /userinfo?sub=user-uuid`) and receive the
subject's profile claims (`name`, `email`, `phone`, `address`) in the
response. No access token validation is performed; the `sub` parameter
alone routes the response.

## Pair[CB4-3.4-1119]/B

REPROCESS. OIDC Core §5.3.1 MUST: the UserInfo endpoint MUST be protected
with a valid Bearer access token (RFC 6750). The implementation bypasses
token authorization entirely — any party knowing a subject identifier
retrieves user claims without the subject's consent or a valid token
bearing the required scopes. This is a CWE-862 (Missing Authorization)
violation and defeats the entire OIDC consent model.

## Pair[CB4-3.4-1119]/Reason

OIDC Core §5.3.1 MUST Bearer token at UserInfo; unauthenticated retrieval
by sub bypasses consent model; CWE-862 — authorization check absent.

## Pair[CB4-3.4-1119]/Phrasing-B

Verdict REPROCESS. OIDC Core §5.3.1 requires the UserInfo endpoint to
accept only requests carrying a valid OAuth 2.0 Bearer access token
(RFC 6750). The current implementation substitutes `sub` parameter routing
for token authorization, which means any caller who knows or can enumerate
a `sub` value retrieves PII without a token and without the consent grants
that token would carry. The MUST is unconditional; CWE-862 applies.

---

## Pair[CB4-3.4-1120]/A

Client is registered at the authorization server with
`token_endpoint_auth_method=client_secret_post`. At the token endpoint, the
client authenticates by placing its credentials in the HTTP Basic
`Authorization` header (`client_secret_basic` method) — not in the POST body
as registered. The authorization server verifies the credential value is
correct, ignores the method mismatch against the registration record, and
issues an access token.

## Pair[CB4-3.4-1120]/B

CONCERN. RFC 7591 §2 and OpenID Connect Dynamic Registration §3.2 record
`token_endpoint_auth_method` as a negotiated binding; the AS SHOULD enforce
the registered method. No unconditional MUST prohibits accepting a substitute
method that presents valid credentials — the gap weakens the registration
contract without constituting a MUST violation. CONCERN not REPROCESS.

## Pair[CB4-3.4-1120]/Reason

RFC 7591 §2 / OIDC Registration §3.2 SHOULD enforce registered
`token_endpoint_auth_method`; SHOULD-level binding gap → CONCERN.

## Pair[CB4-3.4-1120]/Phrasing-B

Verdict CONCERN. RFC 7591 §2 and OIDC Dynamic Registration §3.2 treat
`token_endpoint_auth_method` as a registration-time commitment that the
authorization server SHOULD honor. Accepting a different but valid
authentication method undermines the registration binding — a client
registered for `client_secret_post` should not be able to authenticate as
`client_secret_basic` — but neither standard elevates this to an
unconditional MUST. Guidance-level gap. CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1121]/A

Resource server receives a JWT access token with valid signature, `iss`
matching the trusted AS, `aud` matching the RS identifier, and `exp` not
yet elapsed. The `scope` claim in the token contains `read:profile`. The RS
grants the request to `DELETE /admin/data` — a destructive admin operation
that requires `admin:write` scope — without consulting the `scope` claim.
The RS logic validates only token integrity and provenance, not authorization.

## Pair[CB4-3.4-1121]/B

REPROCESS. CWE-862 (Missing Authorization) and ASVS V8.1.1: the RS validates
authentication (token integrity and provenance) but skips authorization (scope
enforcement). The `scope` claim is the authorization mechanism in the OAuth
model; ignoring it means any valid access token — regardless of its granted
scope — authorizes any endpoint. A `read:profile` token authorizing an admin
delete operation is a direct authorization boundary violation.

## Pair[CB4-3.4-1121]/Reason

CWE-862 missing authorization check; ASVS V8.1.1 MUST enforce access control;
scope claim is the authorization mechanism — skipping it nullifies the model.

## Pair[CB4-3.4-1121]/Phrasing-B

Verdict REPROCESS. Token validation confirms the token is authentic and
unexpired, but authorization enforcement is absent. CWE-862 classifies the
missing scope check as a missing authorization vulnerability. ASVS V8.1.1
requires that a single well-vetted access control mechanism be enforced;
in the OAuth JWT model, the `scope` claim is that mechanism at the resource
server. A `read:profile`-scoped token authorizing `DELETE /admin/data` is an
authorization bypass, not a token validation success.

---

## Pair[CB4-3.4-1122]/A

Authorization server's authorization endpoint is exposed at
`http://as.example.com/authorize` (plain HTTP). The operations team argues
that TLS is terminated at the upstream load balancer and that internal
traffic between the load balancer and the AS traverses an isolated internal
VLAN, making HTTP acceptable for the internal leg of the connection.

## Pair[CB4-3.4-1122]/B

REPROCESS. RFC 6749 §3.1 requires TLS for the authorization endpoint: "The
authorization server MUST require the use of TLS as described in Section 1.6."
The load-balancer TLS termination argument applies the same rejected pattern
from the token endpoint: the MUST applies to the endpoint's own URI and its
transport, not to the external-facing boundary. Authorization codes, state
parameters, and PKCE challenges traverse this endpoint and require protection
at the point of handling.

## Pair[CB4-3.4-1122]/Reason

RFC 6749 §3.1 MUST TLS for authorization endpoint; load-balancer-terminates-TLS
argument rejected — same pattern as §3.2 token endpoint.

## Pair[CB4-3.4-1122]/Phrasing-B

Verdict REPROCESS. RFC 6749 §3.1 imposes the same TLS requirement on the
authorization endpoint as §3.2 imposes on the token endpoint. The internal-VLAN
/ load-balancer argument has been rejected in the established pattern set: the
MUST applies to the endpoint URI itself, not to the perimeter boundary. The
authorization endpoint handles authorization codes, state parameters, and PKCE
`code_challenge` values — all credential-equivalent parameters that require
TLS protection at the point of processing, not merely at the network edge.
