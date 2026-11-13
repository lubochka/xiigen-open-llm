# Node 08 — Arbiter Set Extension 53
# Sub-phase: 3.4 Security and Authorization Model
# Pair ID prefix: CB4-3.4-
# Sequences: 1375–1382 (16 instances — 8 base pairs × 2 phrasings)
# Sources: RFC 8693 §2.1, RFC 9101 §4 / RFC 9126,
#          OIDC Back-Channel Logout §2.6, RFC 6749 §4.1.1,
#          OIDC Core §3.1.3.7.6, RFC 7591 §2,
#          RFC 8628 §3.5, OIDC Core §3.1.3.7.13
# Running total after this file: 2764 instances (1382 base pairs)
# Formula check: 120 + (1382 − 60) × 2 = 120 + 2644 = 2764 ✓

---

## Pair[CB4-3.4-1375]/A

A client sends a Token Exchange request (RFC 8693) to an authorization server.
The request body contains `grant_type=urn:ietf:params:oauth:grant-type:token-exchange`,
`subject_token=<token_value>`, and `requested_token_type=urn:ietf:params:oauth:token-type:access_token`.
The `subject_token_type` parameter is absent. The AS inspects the subject token,
infers that it is a JWT access token based on its format, and completes the
exchange. The team argues that the token type can be inferred from the token format.

## Pair[CB4-3.4-1375]/B

REPROCESS. RFC 8693 §2.1 specifies that `subject_token_type` is a REQUIRED
parameter for token exchange requests. The AS cannot reliably determine the
semantic type of a security token from its format alone — different token types
may share the same format (e.g., both ID tokens and access tokens can be JWTs).
The token type informs the AS about the security context of the presented subject
token (e.g., whether it is an access token, ID token, SAML assertion, or other).
The format-inference argument is rejected: the REQUIRED designation in RFC 8693
§2.1 is unconditional; the AS must return `error=invalid_request` when the
parameter is absent. MUST violated; REPROCESS.

## Pair[CB4-3.4-1375]/Reason

RFC 8693 §2.1 REQUIRED: subject_token_type is required in token exchange requests;
absent from request; format-inference argument rejected → REPROCESS.

## Pair[CB4-3.4-1375]/Phrasing-B

Verdict REPROCESS. RFC 8693 §2.1 REQUIRED: subject_token_type is a mandatory
parameter. Its absence requires error=invalid_request. Token format inference does
not substitute for the required type declaration. REPROCESS.

---

## Pair[CB4-3.4-1376]/A

A client pushes a JAR (JWT-Secured Authorization Request) to the PAR endpoint.
The pushed request body contains a signed JWT in the `request` parameter. The
AS parses the JWT payload to extract the authorization parameters but does not
verify the JWT signature — it processes the request as if the signing were
advisory. The AS team argues that because the client authenticated at the PAR
endpoint, signature verification of the JAR is redundant.

## Pair[CB4-3.4-1376]/B

REPROCESS. RFC 9101 §4 MUST: when a request object (JAR) is used, the authorization
server MUST validate the request object, including verifying its signature against
the client's registered public key (or shared secret for symmetric algorithms).
PAR client authentication at the endpoint level establishes that the push came
from the authenticated client, but it does not validate the integrity and
authenticity of the JWT payload. The signature verification ensures that the
request parameters inside the JWT have not been tampered with between signing
and submission. The client-authentication-as-sufficient argument is rejected:
RFC 9101 §4 MUST applies to JAR independently of the transport-level
authentication mechanism. MUST violated; REPROCESS.

## Pair[CB4-3.4-1376]/Reason

RFC 9101 §4 MUST: AS must verify JAR request object signature; JAR pushed via PAR
but signature not verified; PAR client authentication argument rejected → REPROCESS.

## Pair[CB4-3.4-1376]/Phrasing-B

Verdict REPROCESS. RFC 9101 §4 MUST: JAR request object signatures must be
verified. PAR endpoint authentication proves client identity at the transport
level; it does not substitute for JWT signature verification. REPROCESS.

---

## Pair[CB4-3.4-1377]/A

An OIDC provider implements Back-Channel Logout. The logout tokens it issues are
well-formed, contain the `events` claim, `iss`, `sub`, `iat`, `jti`, `aud`, and
`exp`. However, the logout token payload also includes a `nonce` claim, copied
from the original authentication request. The provider team argues that including
the nonce improves replay resistance for the logout token.

## Pair[CB4-3.4-1377]/B

REPROCESS. OIDC Back-Channel Logout §2.6 MUST NOT: logout tokens MUST NOT contain
a `nonce` claim. The prohibition exists to prevent logout tokens from being
confused with ID tokens, which do use nonces. The nonce in an ID token serves
replay prevention for authentication events; its presence in a logout token would
allow an RP that carelessly accepts either token type to confuse a logout token
for an authentication token. The replay-resistance argument is rejected: `jti`
already provides replay prevention for logout tokens; adding a nonce violates
the MUST NOT prohibition. MUST NOT violated; REPROCESS.

## Pair[CB4-3.4-1377]/Reason

OIDC Back-Channel Logout §2.6 MUST NOT: logout tokens must not contain nonce claim;
nonce present in logout token; replay-resistance argument rejected; jti provides
logout replay prevention → REPROCESS.

## Pair[CB4-3.4-1377]/Phrasing-B

Verdict REPROCESS. OIDC Back-Channel Logout §2.6 MUST NOT: nonce is prohibited
in logout tokens to prevent token type confusion with ID tokens. jti provides
replay prevention; nonce is redundant and prohibited. REPROCESS.

---

## Pair[CB4-3.4-1378]/A

A public client initiates an authorization code flow. The authorization request
URL sent to the authorization endpoint contains `response_type=code`,
`redirect_uri=https://app.example.com/callback`, `scope=openid`, and `state=abc`.
The `client_id` parameter is absent. The AS accepts the request, looks up the
only registered client whose redirect_uri matches, and proceeds with the
authorization flow for that client. The team argues this is a convenience feature
that reduces URL length and simplifies client implementation.

## Pair[CB4-3.4-1378]/B

REPROCESS. RFC 6749 §4.1.1 MUST: the authorization request MUST include the
`client_id` parameter. The client_id is a required parameter because it uniquely
identifies the requesting client to the authorization server — without it, the
AS cannot reliably determine which client's registration to apply. The
redirect_uri-lookup convenience argument is rejected: in a multi-client deployment,
redirect URIs may not be unique; relying on redirect_uri lookup introduces
ambiguity and enables potential client confusion attacks. The AS must return
`error=invalid_request` when `client_id` is absent. MUST violated; REPROCESS.

## Pair[CB4-3.4-1378]/Reason

RFC 6749 §4.1.1 MUST: client_id is required in authorization request; absent from
request; redirect-uri-lookup convenience argument rejected → REPROCESS.

## Pair[CB4-3.4-1378]/Phrasing-B

Verdict REPROCESS. RFC 6749 §4.1.1 MUST: client_id is a required authorization
request parameter. Its absence requires error=invalid_request. Redirect URI lookup
is not a substitute for explicit client identification. REPROCESS.

---

## Pair[CB4-3.4-1379]/A

An OIDC relying party receives an ID token with `iat` indicating issuance 90
minutes ago. The RP's validation pipeline checks `iss`, `aud`, `exp`, `nonce`,
and signature, but does not reject the token based on the elapsed time since `iat`.
The RP team argues that `exp` is the authoritative time bound and that checking
`iat` staleness would introduce clock-synchronization complexity.

## Pair[CB4-3.4-1379]/B

CONCERN. OIDC Core §3.1.3.7.6 provides guidance that the `iat` claim can be used
to reject tokens issued too far in the past, with the acceptable range being
client-specific. This is descriptive guidance rather than an unconditional MUST —
OIDC Core does not specify a mandatory maximum `iat` staleness threshold. The RP
team is correct that `exp` is the normative time boundary for token acceptance;
`iat` staleness checking is an optional additional defense. The clock-synchronization
complexity argument is acknowledged as a valid implementation concern. No MUST is
violated by omitting iat freshness checks. CONCERN not REPROCESS.

## Pair[CB4-3.4-1379]/Reason

OIDC Core §3.1.3.7.6 guidance: iat staleness check is descriptive guidance, not
unconditional MUST; exp is normative boundary; iat staleness omission is guidance-
level gap → CONCERN.

## Pair[CB4-3.4-1379]/Phrasing-B

Verdict CONCERN. OIDC Core §3.1.3.7.6 describes iat-staleness checking as an
optional defense. No MUST mandates a maximum iat age. exp is the normative time
bound. Omitting iat staleness validation is a guidance-level gap. CONCERN not
REPROCESS.

---

## Pair[CB4-3.4-1380]/A

An OIDC provider's dynamic client registration endpoint returns a registration
response that includes `client_id`, `client_secret`, `client_id_issued_at`, and
`client_secret_expires_at`. The response does not include the
`token_endpoint_auth_method` field. The registered client subsequently uses
`client_secret_basic` authentication at the token endpoint, which happens to work.
The developer asks whether the missing `token_endpoint_auth_method` in the
response is a conformance issue.

## Pair[CB4-3.4-1380]/B

CONCERN. RFC 7591 §2 specifies that the server's registration response SHOULD
include all registered client metadata. When `token_endpoint_auth_method` is
absent from the response, the default value is `client_secret_basic` per RFC 7591
§2 — the client can infer the method from the default. No RFC contains an
unconditional MUST requiring the registration response to explicitly include
`token_endpoint_auth_method`. The field's absence is a guidance-level gap; the
defined default behavior provides sufficient information for a correctly implemented
client. CONCERN not REPROCESS.

## Pair[CB4-3.4-1380]/Reason

RFC 7591 §2 SHOULD include all client metadata in registration response; no
unconditional MUST for token_endpoint_auth_method field; defined default value
exists; guidance-level gap → CONCERN.

## Pair[CB4-3.4-1380]/Phrasing-B

Verdict CONCERN. RFC 7591 §2 SHOULD: registration responses should include all
client metadata. No MUST mandates explicit inclusion of token_endpoint_auth_method.
The defined default (client_secret_basic) applies when absent. Guidance-level gap.
CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1381]/A

An authorization server supports the OAuth 2.0 Device Authorization Grant (RFC
8628). The device authorization response returned to the device client includes
`device_code`, `user_code`, `verification_uri`, and `expires_in`, but does not
include the `interval` field. The client polls the token endpoint at a fixed rate
of 2 seconds per poll. The RFC 8628 specification notes that the default polling
interval is 5 seconds. The AS team argues that the interval field is optional.

## Pair[CB4-3.4-1381]/B

CONCERN. RFC 8628 §3.5 specifies that `interval` is an OPTIONAL field in the
device authorization response — when absent, the client SHOULD use a minimum
default of 5 seconds between polling attempts. No unconditional MUST requires
the authorization server to include the `interval` field in the response. The
AS team is correct that `interval` is optional. The gap is that clients that
do not implement the 5-second default may poll more aggressively, creating
unnecessary load on the token endpoint — a guidance-level operational concern.
No MUST is violated. CONCERN not REPROCESS.

## Pair[CB4-3.4-1381]/Reason

RFC 8628 §3.5 OPTIONAL: interval field is optional in device authorization
response; default of 5 seconds applies when absent; no unconditional MUST;
guidance-level gap → CONCERN.

## Pair[CB4-3.4-1381]/Phrasing-B

Verdict CONCERN. RFC 8628 §3.5 OPTIONAL: the interval field is not mandatory.
When absent, clients should default to 5-second polling. No MUST violated. Guidance-
level gap. CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1382]/A

An OIDC relying party requests authentication with `acr_values=urn:mace:incommon:iap:gold`.
The ID token returned by the OIDC provider contains `acr=urn:mace:incommon:iap:silver`
— a lower assurance level than requested. The RP's validation pipeline does not
check whether the returned `acr` value matches the requested `acr_values` list.
The RP proceeds with the session as if gold-level authentication was satisfied.
The team argues that the provider's inclusion of the `acr` claim means the RP has
sufficient context.

## Pair[CB4-3.4-1382]/B

CONCERN. OIDC Core §3.1.3.7.13 specifies that if `acr_values` were requested using
a specific list of required values, the Client SHOULD verify that the returned
`acr` is in the requested list and at the appropriate level. The obligation is
SHOULD-level, not an unconditional MUST. However, the impact is significant: an
RP that uses gold-level authentication context for high-security decisions and
receives silver-level authentication without detection may grant privileges that
require gold-level assurance. The team's "sufficient context" argument is partially
accepted in the normative sense (SHOULD not MUST), but represents a meaningful
security gap in deployments where acr levels have access control implications.
No unconditional MUST is violated. CONCERN not REPROCESS.

## Pair[CB4-3.4-1382]/Reason

OIDC Core §3.1.3.7.13 SHOULD: RP should verify returned acr matches requested
acr_values; no unconditional MUST; lower-assurance acr accepted without check;
guidance-level gap → CONCERN.

## Pair[CB4-3.4-1382]/Phrasing-B

Verdict CONCERN. OIDC Core §3.1.3.7.13 SHOULD: RP should check returned acr
against requested acr_values. No MUST mandates this check. Accepting a lower-
assurance acr without detection is a guidance-level gap with access control
implications. CONCERN not REPROCESS.
