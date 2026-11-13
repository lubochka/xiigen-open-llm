# Node 08 — Arbiter Set Extension 52
# Sub-phase: 3.4 Security and Authorization Model
# Pair ID prefix: CB4-3.4-
# Sequences: 1349–1356 (16 instances — 8 base pairs × 2 phrasings)
# Sources: OIDC Back-Channel Logout §2.4, RFC 9126 §2.3,
#          OIDC Core §2, RFC 9700 §4.3.2,
#          RFC 6749 §5.1 / OIDC Core §5.3,
#          OIDC Core §5.3 / CWE-942, RFC 9126 §2.2,
#          OIDC RP-Initiated Logout §3
# Running total after this file: 2712 instances (1356 base pairs)
# Formula check: 120 + (1356 − 60) × 2 = 120 + 2592 = 2712 ✓

---

## Pair[CB4-3.4-1349]/A

An OIDC provider implements Back-Channel Logout. When a logout event is triggered,
the provider sends logout tokens to registered back-channel logout URIs. A review
of the logout token structure reveals that the token's payload contains `iss`,
`sub`, `iat`, `jti`, and `aud` claims, but does not include the `events` claim.
The logout tokens are signed RS256. The provider team argues that the token's
purpose is unambiguous because it is delivered to the back-channel logout endpoint,
making the `events` claim redundant.

## Pair[CB4-3.4-1349]/B

REPROCESS. OIDC Back-Channel Logout §2.4 MUST: logout tokens MUST contain an
`events` claim with a JSON object whose member name is the URI
`http://schemas.openid.net/event/backchannel-logout` and whose value is an empty
JSON object. The `events` claim is the structural marker that distinguishes a
logout token from other JWTs (such as ID tokens) — without it, the token cannot
be safely identified as a logout token by the receiving party, and any OIDC JWT
could be replayed against a back-channel logout endpoint. The delivery-channel
argument is rejected: the MUST applies to the token structure regardless of how
it is delivered. MUST violated; REPROCESS.

## Pair[CB4-3.4-1349]/Reason

OIDC Back-Channel Logout §2.4 MUST: logout token must include events claim with
backchannel-logout URI as member; events claim absent; delivery-channel argument
rejected → REPROCESS.

## Pair[CB4-3.4-1349]/Phrasing-B

Verdict REPROCESS. OIDC Back-Channel Logout §2.4 MUST: the events claim is a
required structural marker in logout tokens. Its absence prevents token type
identification and enables token confusion. REPROCESS.

---

## Pair[CB4-3.4-1350]/A

An authorization server supports PAR. Client A pushes an authorization request
and receives `request_uri=urn:ietf:params:oauth:request_uri:abc123`. Client B
(a different registered client) obtains the request_uri value and uses it in
an authorization request to the authorization endpoint with Client B's own
`client_id`. The AS finds the PAR record for the request_uri, ignores the
client binding check, and proceeds with the authorization flow using Client B's
client_id. The team argues that the request_uri is a shared reference and client
binding is advisory.

## Pair[CB4-3.4-1350]/B

REPROCESS. RFC 9126 §2.3 MUST: the `request_uri` is bound to the client that
issued the push request — the authorization endpoint MUST reject the request if
the `client_id` in the authorization request does not match the `client_id` that
was used when the request was pushed. This binding prevents client substitution
attacks where one client can hijack another client's pushed authorization context
(including scope, redirect_uri, and other parameters). The shared-reference
argument is rejected: the PAR request_uri is a single-use, client-bound reference,
not a shareable URI. MUST violated; REPROCESS.

## Pair[CB4-3.4-1350]/Reason

RFC 9126 §2.3 MUST: request_uri must be bound to the client that pushed it;
different client used request_uri; client binding check ignored; shared-reference
argument rejected → REPROCESS.

## Pair[CB4-3.4-1350]/Phrasing-B

Verdict REPROCESS. RFC 9126 §2.3 MUST: the request_uri is client-bound; the
authorization endpoint must reject if client_id at the authorization endpoint
differs from the pushing client. Client substitution via request_uri is the
attack this binding prevents. REPROCESS.

---

## Pair[CB4-3.4-1351]/A

An OIDC authorization server issues ID tokens for a relying party in an
authorization flow where both the RP's `client_id` and a second audience
(`aud=["rp-client-id","analytics-service"]`) are present. The AS includes
`"azp":"analytics-service"` in the ID token — the `azp` value is set to
`analytics-service` rather than `rp-client-id` (the requesting client). The AS
team argues that `azp` was intended to identify the analytics service as a
"co-authorized" party.

## Pair[CB4-3.4-1351]/B

REPROCESS. OIDC Core §2 MUST: if the `azp` claim is present in an ID token, it
MUST contain the OAuth 2.0 Client ID of the party to which the ID token was
issued — i.e., the requesting client's `client_id`. The `azp` claim identifies
the Authorized Party: the single party authorized to use the token, which is the
relying party that initiated the authorization request. Setting `azp` to a third-
party service (`analytics-service`) misrepresents who the token was issued to
and would cause any RP that validates `azp` against its own `client_id` to reject
the token. The "co-authorized party" concept is not defined in OIDC Core §2.
MUST violated; REPROCESS.

## Pair[CB4-3.4-1351]/Reason

OIDC Core §2 MUST: when azp present, it must equal the requesting client's
client_id; azp set to analytics-service instead of rp-client-id; co-authorized
argument rejected → REPROCESS.

## Pair[CB4-3.4-1351]/Phrasing-B

Verdict REPROCESS. OIDC Core §2 MUST: azp must be the client_id of the party to
which the ID token was issued. Setting azp to a third-party service is incorrect
and violates the MUST. REPROCESS.

---

## Pair[CB4-3.4-1352]/A

An OIDC authorization server's authorization endpoint accepts
`response_type=token` in authorization requests from web application clients
(clients using an HTTPS redirect_uri that is not a native app or device). The AS
issues access tokens directly in the fragment of the authorization response
redirect, implementing the OAuth 2.0 implicit grant. The AS team argues that the
implicit flow is supported by RFC 6749 and that some older integrations require it.

## Pair[CB4-3.4-1352]/B

REPROCESS. RFC 9700 §4.3.2 MUST NOT: authorization servers MUST NOT issue access
tokens via the implicit grant (`response_type=token`) in the authorization
response. RFC 9700 supersedes the implicit flow permission in RFC 6749 and
establishes a normative prohibition on implicit access token issuance. The
security rationale is that access tokens in fragment components are exposed to
the browser history, referer headers, and JavaScript on the page. The
older-integrations argument is rejected: RFC 9700 is the current security best
practice for OAuth; continued support for the implicit flow in direct violation
of RFC 9700 §4.3.2 is a MUST NOT violation regardless of backward-compatibility
intent. MUST NOT violated; REPROCESS.

## Pair[CB4-3.4-1352]/Reason

RFC 9700 §4.3.2 MUST NOT: AS must not issue access tokens via implicit flow
(response_type=token); AS accepts response_type=token for web clients; older-
integrations argument rejected → REPROCESS.

## Pair[CB4-3.4-1352]/Phrasing-B

Verdict REPROCESS. RFC 9700 §4.3.2 MUST NOT: implicit access token issuance is
prohibited. RFC 9700 supersedes RFC 6749 on this point. Backward-compatibility
claims do not override a MUST NOT. REPROCESS.

---

## Pair[CB4-3.4-1353]/A

An authorization server issues refresh tokens in authorization code flows. Token
responses include `access_token`, `token_type`, `expires_in`, `refresh_token`,
and `scope`. The response does not include a `refresh_token_expires_in` field.
Clients have no way to determine when their refresh tokens will expire without
attempting to use them and receiving an error. The client developer team asks
whether this omission is a specification violation.

## Pair[CB4-3.4-1353]/B

CONCERN. No RFC contains an unconditional MUST requiring authorization servers to
include `refresh_token_expires_in` in the token response. RFC 6749 §5.1 specifies
the required and optional fields for token responses and does not include
`refresh_token_expires_in` as a mandatory field. Some authorization server
implementations include it as a non-standard extension for client convenience,
but the obligation to provide it is at best guidance-level. The absence creates
a usability gap — clients cannot proactively know when to re-initiate
authorization — but does not violate a normative MUST. CONCERN not REPROCESS.

## Pair[CB4-3.4-1353]/Reason

No RFC MUST for refresh_token_expires_in in token response; RFC 6749 §5.1 does
not include it as a required field; absence is guidance-level gap → CONCERN.

## Pair[CB4-3.4-1353]/Phrasing-B

Verdict CONCERN. RFC 6749 §5.1 does not require refresh_token_expires_in. No
unconditional MUST mandates this field. Its absence is a usability gap, not a
MUST violation. CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1354]/A

An OIDC provider's UserInfo endpoint does not set any CORS (Cross-Origin Resource
Sharing) headers in its responses. Browser-based single-page applications (SPAs)
that attempt to call the UserInfo endpoint directly from JavaScript receive a CORS
error and cannot retrieve user information. The AS team argues that SPAs should
use a backend-for-frontend proxy pattern instead of calling the UserInfo endpoint
directly from the browser.

## Pair[CB4-3.4-1354]/B

CONCERN. OIDC Core §5.3 and related guidance note that UserInfo endpoints accessed
by browser-based clients SHOULD support CORS to enable direct browser access.
However, no unconditional MUST in OIDC Core or an applicable RFC requires the
UserInfo endpoint to support CORS headers. The AS team's backend-for-frontend
argument is a valid architectural alternative — SPAs can access the UserInfo
endpoint through a server-side proxy, which avoids CORS requirements entirely.
The CORS gap is a guidance-level recommendation. No MUST is violated. CONCERN not
REPROCESS.

## Pair[CB4-3.4-1354]/Reason

OIDC Core §5.3 guidance SHOULD: UserInfo endpoint should support CORS for browser
clients; no unconditional MUST; BFF proxy argument valid alternative → CONCERN.

## Pair[CB4-3.4-1354]/Phrasing-B

Verdict CONCERN. OIDC Core §5.3 guidance recommends CORS support for the UserInfo
endpoint when accessed by browser clients. No unconditional MUST mandates it.
The BFF proxy is a valid architectural alternative. Guidance-level gap → CONCERN
not REPROCESS.

---

## Pair[CB4-3.4-1355]/A

An authorization server supports PAR. After accepting a pushed authorization
request, the AS returns a response with `request_uri` and `expires_in=3600` —
the request_uri is valid for one hour. A security reviewer notes that a one-hour
PAR request_uri lifetime is significantly longer than typical recommendations.
The AS team argues that some users take up to 45 minutes between selecting an
application and completing authentication.

## Pair[CB4-3.4-1355]/B

CONCERN. RFC 9126 §2.2 recommends that authorization servers keep the validity
period of PAR request_uri values short to reduce the window in which a stolen
reference could be used. However, no RFC provides an unconditional MUST with a
numeric maximum expiry for PAR request_uri values. The recommendation is
qualitative (SHOULD be short), not a specific ceiling. The AS team's argument
that some flows require longer periods is a valid operational consideration.
The one-hour lifetime creates a wider replay window for captured request_uri
values, but the absence of a numeric MUST ceiling means this is a guidance-level
gap. No MUST is violated. CONCERN not REPROCESS.

## Pair[CB4-3.4-1355]/Reason

RFC 9126 §2.2 SHOULD short request_uri lifetime; no RFC MUST with numeric ceiling;
3600-second PAR expires_in is guidance-level gap → CONCERN.

## Pair[CB4-3.4-1355]/Phrasing-B

Verdict CONCERN. RFC 9126 §2.2 recommends short request_uri lifetimes. No numeric
MUST ceiling exists. A 3600-second expiry is a guidance-level gap. CONCERN not
REPROCESS.

---

## Pair[CB4-3.4-1356]/A

An OIDC provider supports RP-Initiated Logout per the OIDC RP-Initiated Logout
specification. When the RP sends a logout request with an `id_token_hint`
parameter, the AS does not validate that the hint token was actually issued to
the currently authenticated session — it accepts any well-formed ID token from
its JWKS as an acceptable hint. The team argues that validating the hint against
the current session introduces unnecessary latency, since the user is about to
be logged out anyway.

## Pair[CB4-3.4-1356]/B

CONCERN. OIDC RP-Initiated Logout §3 specifies that authorization servers SHOULD
use the `id_token_hint` to identify the session to terminate. This is a SHOULD-
level recommendation, not an unconditional MUST. The AS accepting any valid ID
token as an acceptable hint without session binding means any party in possession
of a previously issued ID token could initiate logout for an unrelated session —
a logout CSRF risk. However, because the obligation is SHOULD-level and the
id_token_hint is defined as a hint (not a mandatory binding), the gap is guidance-
level. The AS team's latency argument does not diminish the SHOULD gap, but it
does not constitute a MUST violation. CONCERN not REPROCESS.

## Pair[CB4-3.4-1356]/Reason

OIDC RP-Initiated Logout §3 SHOULD: AS should validate id_token_hint matches
current session; no unconditional MUST; unvalidated hint accepted; guidance-level
gap → CONCERN.

## Pair[CB4-3.4-1356]/Phrasing-B

Verdict CONCERN. OIDC RP-Initiated Logout §3 SHOULD: id_token_hint should be
validated against the current session. No unconditional MUST mandates strict hint
validation. Unvalidated hints create a logout CSRF risk at guidance level. CONCERN
not REPROCESS.
