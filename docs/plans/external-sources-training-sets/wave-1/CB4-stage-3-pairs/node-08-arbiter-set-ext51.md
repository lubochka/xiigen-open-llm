# Node 08 — Arbiter Set Extension 51
# Sub-phase: 3.4 Security and Authorization Model
# Pair ID prefix: CB4-3.4-
# Sequences: 1323–1330 (16 instances — 8 base pairs × 2 phrasings)
# Sources: RFC 6750 §7, RFC 9101 §4,
#          RFC 7521 §4.8, RFC 6749 §5.1,
#          RFC 9126 §2.3, RFC 7662,
#          OIDC Core §5.5.1, RFC 8414 §2
# Running total after this file: 2660 instances (1330 base pairs)
# Formula check: 120 + (1330 − 60) × 2 = 120 + 2540 = 2660 ✓

---

## Pair[CB4-3.4-1323]/A

A resource server receives an HTTP request for a protected resource. The request
includes a Bearer token in both the `Authorization` header
(`Authorization: Bearer abc123`) and in the request body as a form parameter
(`token=abc123`). The RS detects both and, because the tokens are identical, uses
the header value and fulfills the request. The RS team argues that accepting both
when identical is benign and improves compatibility with misconfigured clients.

## Pair[CB4-3.4-1323]/B

REPROCESS. RFC 6750 §7 MUST: when a request contains multiple Bearer token
transmission methods, the resource server MUST reject the request using the
`invalid_request` error code. The prohibition exists because accepting multiple
transmission methods simultaneously creates ambiguity about which method is
authoritative and enables injection attacks where an adversary adds a second
token parameter to an otherwise valid request. The "identical token, same effect"
argument is rejected: the MUST applies to the presence of multiple methods in the
same request unconditionally, regardless of whether the token values match. The
RS must reject; it cannot select one method and proceed. MUST violated; REPROCESS.

## Pair[CB4-3.4-1323]/Reason

RFC 6750 §7 MUST: RS must reject requests with multiple Bearer token transmission
methods; both header and body token present; identical-token argument rejected →
REPROCESS.

## Pair[CB4-3.4-1323]/Phrasing-B

Verdict REPROCESS. RFC 6750 §7 MUST: a request containing Bearer tokens in multiple
locations MUST be rejected with invalid_request. Token identity across methods does
not exempt the request. REPROCESS.

---

## Pair[CB4-3.4-1324]/A

A client sends a JAR (JWT-Secured Authorization Request) to an OIDC provider's
authorization endpoint. The request object is a signed JWT but its payload does
not include the `iss` claim — only `aud`, `exp`, `iat`, and the standard
authorization parameters are present. The AS processes the request object,
validates the signature, and proceeds with the authorization flow. The team
argues that the `aud` claim is sufficient for request object binding, making
`iss` redundant.

## Pair[CB4-3.4-1324]/B

REPROCESS. RFC 9101 §4 MUST: request objects MUST contain the `iss` claim set
to the client's `client_id`. The `iss` claim establishes the identity of the
principal who created the request object — without it, the AS cannot confirm
that the request object was created by the client whose `client_id` appears in
the authorization parameters, enabling request object substitution attacks.
The `aud`-is-sufficient argument is rejected: `aud` identifies the intended
recipient (the AS); `iss` identifies the sender (the client). Both serve distinct
binding functions; RFC 9101 §4 requires both. MUST violated; REPROCESS.

## Pair[CB4-3.4-1324]/Reason

RFC 9101 §4 MUST: JAR request object must include iss=client_id; iss absent from
request JWT; aud-sufficient argument rejected → REPROCESS.

## Pair[CB4-3.4-1324]/Phrasing-B

Verdict REPROCESS. RFC 9101 §4 MUST: request objects must contain iss equal to
client_id. The iss claim identifies the request creator; its absence enables
substitution attacks. aud alone is insufficient. REPROCESS.

---

## Pair[CB4-3.4-1325]/A

A confidential client authenticates to the token endpoint using
`private_key_jwt` (RFC 7521). The authorization server validates the JWT
signature and checks that `iss`, `sub`, `aud`, and `exp` are correct. However,
the AS does not record or check the `jti` claim — the same client assertion
JWT can be submitted to the token endpoint multiple times within its validity
window and will succeed on each submission. The team argues that `exp` provides
sufficient replay protection since assertions expire quickly.

## Pair[CB4-3.4-1325]/B

REPROCESS. RFC 7521 §4.8 MUST: each JSON Web Token MUST contain a unique
identifier in the `jti` claim; the authorization server MUST check that the
`jti` value has not been previously used within the assertion's validity window.
The `jti` uniqueness check is the mechanism that prevents assertion replay attacks —
an attacker who intercepts a valid client assertion can replay it to authenticate
as the client for the duration of the assertion's lifetime. The `exp`-as-sufficient
argument is rejected: `exp` limits the window of exploitation but does not prevent
replay within that window; only `jti` tracking prevents replay within the validity
period. MUST violated; REPROCESS.

## Pair[CB4-3.4-1325]/Reason

RFC 7521 §4.8 MUST: AS must check jti uniqueness for client assertions; AS does
not track jti; exp-sufficient argument rejected; replay within validity window
possible → REPROCESS.

## Pair[CB4-3.4-1325]/Phrasing-B

Verdict REPROCESS. RFC 7521 §4.8 MUST: client assertions must contain unique jti;
the AS must verify jti has not been previously used within the validity window.
exp limits the replay window but does not prevent replay within it. REPROCESS.

---

## Pair[CB4-3.4-1326]/A

A client requests the scopes `read write admin` from the authorization server.
The AS determines that the client is only authorized for `read write` and issues
an access token with scope limited to `read write`. The token response does not
include a `scope` parameter — only `access_token`, `token_type`, and `expires_in`
are returned. The team argues that the narrowed scope is visible to the RS when
it validates the token, so including it in the response adds no value for the
client.

## Pair[CB4-3.4-1326]/B

REPROCESS. RFC 6749 §5.1 MUST: if the scope of the access token issued is
different from the scope requested by the client, the authorization server MUST
include the `scope` response parameter to inform the client of the actual scope
granted. This requirement exists so the client can make an informed decision
about whether the reduced scope meets its needs — the client cannot infer the
granted scope from the token itself (especially with opaque tokens or when JWTs
are not decoded by the client). The "scope visible to RS" argument is rejected:
the obligation is to the client, not the RS; the client must be notified of
scope reduction in the token response. MUST violated; REPROCESS.

## Pair[CB4-3.4-1326]/Reason

RFC 6749 §5.1 MUST: scope must be included in token response when issued scope
differs from requested scope; response omits scope parameter; RS-visibility
argument rejected → REPROCESS.

## Pair[CB4-3.4-1326]/Phrasing-B

Verdict REPROCESS. RFC 6749 §5.1 MUST: when issued scope differs from requested
scope, the token response MUST include the scope parameter. The client — not the
RS — must be informed. Omitting scope is a MUST violation. REPROCESS.

---

## Pair[CB4-3.4-1327]/A

An authorization server supports Pushed Authorization Requests (PAR) per RFC 9126.
When a client pushes an authorization request, the AS returns a `request_uri`
with a 60-second expiry. A client caches the `request_uri` value and, 90 seconds
later, uses it in a standard authorization request. The AS locates the stored PAR
record, finds the `request_uri` still in the database (not yet garbage-collected),
and proceeds with the authorization flow. The team argues that expiry is a
recommendation and that accepting slightly-expired request URIs improves resilience.

## Pair[CB4-3.4-1327]/B

REPROCESS. RFC 9126 §2.3 MUST: the authorization server MUST reject a request
that uses a `request_uri` beyond its expiry time with the error
`invalid_request_uri`. The `request_uri` expiry is a normative security control —
a short-lived, single-use reference prevents `request_uri` values from being
harvested and replayed outside the intended authorization flow. The resilience
argument is rejected: expiry enforcement is not optional; presence of the PAR
record in the database does not override the normative expiry check. The AS must
check the expiry time and reject expired references unconditionally. MUST violated;
REPROCESS.

## Pair[CB4-3.4-1327]/Reason

RFC 9126 §2.3 MUST: AS must reject request_uri used after its expiry; request_uri
used 90 seconds after 60-second expiry; resilience argument rejected; database
presence does not override expiry check → REPROCESS.

## Pair[CB4-3.4-1327]/Phrasing-B

Verdict REPROCESS. RFC 9126 §2.3 MUST: expired request_uri must be rejected with
invalid_request_uri. Database record presence does not substitute for expiry
validation. Resilience argument rejected. REPROCESS.

---

## Pair[CB4-3.4-1328]/A

A resource server validates access tokens via the token introspection endpoint
(RFC 7662). To reduce latency, the RS caches introspection responses for 24 hours
regardless of the token's `exp` field. A token revoked by the AS 10 minutes after
issuance is still accepted by the RS for 24 hours because the cached introspection
response claims it is active. The RS team argues that network round-trips per request
are too costly for high-throughput services.

## Pair[CB4-3.4-1328]/B

CONCERN. RFC 7662 does not contain an unconditional MUST specifying a maximum cache
duration for introspection responses. The `exp` field in the introspection response
indicates token expiry, and guidance-level recommendations suggest that RS
implementations respect the token's own expiry when caching. Caching beyond `exp`
creates a window where revoked or expired tokens remain valid at the RS — a real
security impact. The latency argument is acknowledged as a legitimate operational
concern. However, because no RFC MUST or ASVS MUST specifies a maximum introspection
cache duration, the gap is guidance-level. A cache TTL equal to or less than the
token's remaining lifetime is the recommended practice. No MUST is violated.
CONCERN not REPROCESS.

## Pair[CB4-3.4-1328]/Reason

RFC 7662 no unconditional MUST for introspection cache duration; caching beyond
exp creates revocation gap; latency argument acknowledged; no MUST → CONCERN.

## Pair[CB4-3.4-1328]/Phrasing-B

Verdict CONCERN. RFC 7662 contains no unconditional MUST for introspection cache
duration. Caching beyond token exp is a guidance-level gap with real revocation
window impact. No MUST violated. CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1329]/A

An OIDC relying party sends a `claims` request parameter in its authorization
request, specifying that it requires the `email` and `phone_number` claims to be
present in the UserInfo response. The OIDC provider ignores the `claims` parameter
entirely and returns the default set of claims for the requested scopes. The `email`
claim is present but `phone_number` is not returned, even though it is in the
provider's `claims_supported` list. The RP cannot complete its user verification
workflow without `phone_number`.

## Pair[CB4-3.4-1329]/B

CONCERN. OIDC Core §5.5.1 specifies that the `claims` request parameter is used
to request specific individual Claims; the AS SHOULD return only Claims requested.
The obligation to return claims specified via the `claims` parameter is SHOULD-
level — no unconditional MUST requires the AS to fulfill every claims request.
OIDC Core acknowledges that the AS may not be able to return all requested claims
in all circumstances. The gap affects the RP's ability to complete its workflow
and degrades the usefulness of the claims request mechanism. However, no MUST is
violated by the AS's decision to ignore the claims parameter. CONCERN not REPROCESS.

## Pair[CB4-3.4-1329]/Reason

OIDC Core §5.5.1 SHOULD: AS should return claims specified in claims parameter;
no unconditional MUST; AS ignoring claims parameter is guidance-level gap → CONCERN.

## Pair[CB4-3.4-1329]/Phrasing-B

Verdict CONCERN. OIDC Core §5.5.1 SHOULD: the claims request parameter is a
guidance-level obligation. The AS is not unconditionally required to fulfill all
specified claims. Ignoring the claims parameter is a guidance gap, not a MUST
violation. CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1330]/A

An OIDC authorization server publishes a discovery document at
`https://as.example.com/.well-known/openid-configuration`. The metadata document
includes `issuer`, `authorization_endpoint`, `token_endpoint`, `jwks_uri`, and
`userinfo_endpoint`. It does not include the `grant_types_supported` field. A client
developer asks whether the absence of `grant_types_supported` from the discovery
document is a violation.

## Pair[CB4-3.4-1330]/B

CONCERN. RFC 8414 §2 specifies that `grant_types_supported` is a RECOMMENDED
metadata field — servers SHOULD include it when the AS supports grant types other
than `authorization_code` and `implicit`. No unconditional MUST requires
`grant_types_supported` to be present in the discovery document. The AS team is
correct that this is an optional field; when absent, clients assume only
`authorization_code` is supported. The absence creates a discovery gap for clients
that need to know which grant types are available before initiating a flow, but
it does not violate a normative MUST. CONCERN not REPROCESS.

## Pair[CB4-3.4-1330]/Reason

RFC 8414 §2 SHOULD / RECOMMENDED: grant_types_supported recommended in AS metadata;
no unconditional MUST; absent field is guidance-level gap → CONCERN.

## Pair[CB4-3.4-1330]/Phrasing-B

Verdict CONCERN. RFC 8414 §2 RECOMMENDED: grant_types_supported is a recommended
field in AS discovery metadata. No unconditional MUST mandates its inclusion.
Its absence is a guidance-level gap. CONCERN not REPROCESS.
