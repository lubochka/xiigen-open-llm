# Node 08 — Arbiter Set Extension 55
# Sub-phase: 3.4 Security and Authorization Model
# Pair ID prefix: CB4-3.4-
# Sequences: 1427–1434 (16 instances — 8 base pairs × 2 phrasings)
# Sources: RFC 6749 §5.2, OIDC Core §3.1.3.3,
#          RFC 9126 §2, RFC 9068 §2.2,
#          RFC 6750 §3.1, OIDC Discovery §3,
#          RFC 8414 §2, RFC 6749 §7
# Running total after this file: 2868 instances (1434 base pairs)
# Formula check: 120 + (1434 − 60) × 2 = 120 + 2748 = 2868 ✓

---

## Pair[CB4-3.4-1427]/A

An authorization server's token endpoint returns error responses when client
authentication fails. The response body is `{"error_description":"Client
authentication failed, credentials not recognized"}` with HTTP status 400.
The `error` parameter is absent from the response. Client libraries that parse
error responses per RFC 6749 cannot determine the machine-readable error code.
The AS team argues that the error_description is sufficient for debugging.

## Pair[CB4-3.4-1427]/B

REPROCESS. RFC 6749 §5.2 specifies that the `error` parameter is REQUIRED in
token endpoint error responses. The value must be one of the registered error
codes (e.g., `invalid_client`, `invalid_grant`, `unauthorized_client`). The
`error_description` parameter is OPTIONAL and supplementary — it does not
substitute for the REQUIRED `error` field. Client libraries that follow RFC
6749 parse `error` to determine the error type and take programmatic action;
an absent `error` parameter leaves clients unable to distinguish authentication
failure from grant failure from scope failure. The debugging-sufficiency
argument is rejected: `error_description` is advisory context, not the
normative error code. REQUIRED violated; REPROCESS.

## Pair[CB4-3.4-1427]/Reason

RFC 6749 §5.2 REQUIRED: `error` parameter must be present in token endpoint
error responses; `error_description` alone is insufficient; debugging argument
rejected → REPROCESS.

## Pair[CB4-3.4-1427]/Phrasing-B

Verdict REPROCESS. RFC 6749 §5.2 REQUIRED: token endpoint error responses must
include `error`. error_description alone does not satisfy the REQUIRED designation.
Client-side error parsing is not possible without `error`. REPROCESS.

---

## Pair[CB4-3.4-1428]/A

An OIDC provider issues ID tokens. A client sends an authentication request
that includes a `nonce` parameter. The AS validates the request, authenticates
the user, and issues an ID token. The ID token payload contains `iss`, `sub`,
`aud`, `exp`, `iat`, and the requested claims, but does not contain a `nonce`
claim. The AS team argues that `nonce` is a client-side replay protection
mechanism and the AS is not responsible for echoing it.

## Pair[CB4-3.4-1428]/B

REPROCESS. OIDC Core §3.1.3.3 MUST: if the nonce parameter was included in the
authentication request, the authorization server MUST include a `nonce` claim in
the ID token with the value being the exact nonce value from the authentication
request. The nonce mechanism functions only if the AS echoes the nonce into the
ID token so the RP can verify it against the stored session value — without the
nonce in the ID token, the RP cannot detect token replay or session injection
attacks. The AS-is-not-responsible argument is rejected: the MUST applies to
the AS unconditionally when nonce is present in the request. MUST violated;
REPROCESS.

## Pair[CB4-3.4-1428]/Reason

OIDC Core §3.1.3.3 MUST: nonce must be present in ID token when included in
authentication request; ID token omits nonce; AS-responsibility argument rejected
→ REPROCESS.

## Pair[CB4-3.4-1428]/Phrasing-B

Verdict REPROCESS. OIDC Core §3.1.3.3 MUST: the AS must echo the nonce from
the authentication request into the ID token. Its absence disables replay
protection. REPROCESS.

---

## Pair[CB4-3.4-1429]/A

An authorization server supports PAR. The PAR endpoint (`/as/par`) accepts
pushed authorization requests. A confidential client with registered
`client_secret_basic` authentication sends a PAR request that includes
`client_id` in the request body but does not include any client credentials
(no Authorization header, no `client_secret` parameter). The AS validates the
request parameters and returns a successful PAR response with `request_uri`
and `expires_in`. The AS team argues that client identity is established by
`client_id` and authentication is enforced at the token endpoint.

## Pair[CB4-3.4-1429]/B

REPROCESS. RFC 9126 §2 MUST: authorization servers MUST require the same client
credentials at the PAR endpoint as are required at the token endpoint; confidential
clients MUST authenticate at the PAR endpoint in the same manner as described in
RFC 6749 §3.2.1. A confidential client that presents only `client_id` without
credentials at the PAR endpoint has not authenticated — `client_id` is a public
identifier, not a credential. The token-endpoint-deferred argument is rejected:
authentication at the PAR endpoint is not deferred to the token endpoint; it must
occur at PAR request submission time. Unauthenticated PAR from a confidential
client is a MUST violation. REPROCESS.

## Pair[CB4-3.4-1429]/Reason

RFC 9126 §2 MUST: PAR endpoint must authenticate confidential clients using the
same mechanism required at the token endpoint; confidential client request accepted
without credentials; deferred-auth argument rejected → REPROCESS.

## Pair[CB4-3.4-1429]/Phrasing-B

Verdict REPROCESS. RFC 9126 §2 MUST: PAR endpoint must authenticate confidential
clients. Accepting only client_id without credentials violates the MUST. Token-
endpoint authentication is not a substitute. REPROCESS.

---

## Pair[CB4-3.4-1430]/A

An authorization server issues JWT-formatted access tokens to resource servers.
The JWT payload contains `sub`, `aud`, `exp`, `iat`, `jti`, `client_id`, and
custom claim fields describing the granted scope and resource. The `iss` (Issuer)
claim is absent from the JWT payload. Resource servers that receive the token
cannot identify the issuing entity without the `iss` claim. The AS team argues
that the `aud` claim already identifies the resource owner's context and `iss`
is redundant because the token was issued by the same AS that signed it.

## Pair[CB4-3.4-1430]/B

REPROCESS. RFC 9068 §2.2 specifies that `iss` is a REQUIRED claim in JWT-formatted
access tokens conforming to the JWT Profile for OAuth 2.0 Access Tokens. Resource
servers use `iss` to identify the authority that issued the token, retrieve the
correct JWKS for signature verification, and enforce issuer-specific validation
policy — the `aud` claim identifies the audience, not the issuer, and cannot
substitute for `iss`. The redundancy argument is rejected: a token signed by
the AS without identifying the AS via `iss` prevents resource servers from
establishing the issuer's identity independently of the signing key. REQUIRED
violated; REPROCESS.

## Pair[CB4-3.4-1430]/Reason

RFC 9068 §2.2 REQUIRED: `iss` must be present in JWT-formatted access tokens;
`iss` absent from JWT payload; aud-substitution argument rejected → REPROCESS.

## Pair[CB4-3.4-1430]/Phrasing-B

Verdict REPROCESS. RFC 9068 §2.2 REQUIRED: iss is a mandatory claim in JWT
access tokens. Its absence prevents resource servers from identifying and
validating the issuing authority. REPROCESS.

---

## Pair[CB4-3.4-1431]/A

A resource server protects API endpoints with OAuth Bearer token authentication.
A client presents a request with an invalid access token (expired). The resource
server rejects the request with HTTP 401 but does not include a `WWW-Authenticate`
response header. Client libraries that follow RFC 6750 inspect the `WWW-Authenticate`
header to determine error codes and realm information for re-authentication. The
RS team argues that the 401 status code is sufficient to signal unauthorized access.

## Pair[CB4-3.4-1431]/B

REPROCESS. RFC 6750 §3.1 MUST: if the protected resource request does not include
an access token that enables access — whether because the token is absent, expired,
insufficient in scope, or otherwise invalid — the resource server MUST include the
HTTP `WWW-Authenticate` response header field in the 401 response. The
`WWW-Authenticate` header carries the Bearer challenge, including the `error`,
`error_description`, and `scope` attributes that allow clients to distinguish
token-absent scenarios from token-expired scenarios from insufficient-scope
scenarios and take appropriate recovery action. The status-code-sufficient argument
is rejected: the MUST applies unconditionally; a 401 without `WWW-Authenticate`
is non-conformant per RFC 6750 §3.1. MUST violated; REPROCESS.

## Pair[CB4-3.4-1431]/Reason

RFC 6750 §3.1 MUST: RS must include WWW-Authenticate in 401 responses; 401
returned without WWW-Authenticate header; status-code-sufficient argument rejected
→ REPROCESS.

## Pair[CB4-3.4-1431]/Phrasing-B

Verdict REPROCESS. RFC 6750 §3.1 MUST: resource servers must include
WWW-Authenticate in 401 responses. A bare 401 without the header is non-conformant.
REPROCESS.

---

## Pair[CB4-3.4-1432]/A

An OIDC provider supports RP-Initiated Logout. Relying parties can initiate
logout by directing the end user to the OP's end-session endpoint. However,
the OP's discovery document does not include the `end_session_endpoint` field.
Relying parties must obtain the logout URL out-of-band or by hardcoding it.
The OP team argues that the logout URL is documented in their developer portal
and the discovery document is reserved for core protocol URLs.

## Pair[CB4-3.4-1432]/B

CONCERN. OIDC Discovery §3 lists `end_session_endpoint` as RECOMMENDED — it
should be included in the discovery document when the OP provides an end-session
endpoint, but no unconditional MUST requires its presence. The OP team is correct
that out-of-band documentation and hardcoding are functional workarounds; however,
these approaches tightly couple RPs to OP infrastructure and undermine the
self-describing nature of the discovery document. The absence of `end_session_endpoint`
when the capability exists is a guidance-level gap — not a MUST violation.
No MUST is violated. CONCERN not REPROCESS.

## Pair[CB4-3.4-1432]/Reason

OIDC Discovery §3 RECOMMENDED: end_session_endpoint should be in discovery when
RP-Initiated Logout is supported; no unconditional MUST; out-of-band workaround
acknowledged; guidance-level gap → CONCERN.

## Pair[CB4-3.4-1432]/Phrasing-B

Verdict CONCERN. OIDC Discovery §3 RECOMMENDED: end_session_endpoint should be
advertised when logout is supported. No MUST mandates it. Its absence is a
guidance-level gap. CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1433]/A

An authorization server supports RFC 7009 token revocation. Clients can
successfully revoke tokens by calling the revocation endpoint. However, the
AS's discovery document (RFC 8414 metadata) does not include the
`revocation_endpoint` field. Clients that dynamically discover the AS's
capabilities cannot locate the revocation endpoint from the metadata alone.
The AS team argues that revocation is an advanced feature and its endpoint
URL is provided in their API reference documentation.

## Pair[CB4-3.4-1433]/B

CONCERN. RFC 8414 §2 designates `revocation_endpoint` as OPTIONAL in the
authorization server metadata document — its inclusion is not mandated by
an unconditional MUST. The AS team's position that documentation-based endpoint
discovery is acceptable is technically valid under RFC 8414, though it prevents
clients from dynamically discovering revocation support and tightly couples
deployments to static configuration. The guidance expectation is that supported
endpoints should be published in metadata to enable self-describing deployments;
however, the OPTIONAL designation means absence is not a normative violation.
No MUST is violated. CONCERN not REPROCESS.

## Pair[CB4-3.4-1433]/Reason

RFC 8414 §2 OPTIONAL: revocation_endpoint in AS metadata is not REQUIRED;
AS supports revocation but omits endpoint from discovery; no unconditional
MUST → CONCERN.

## Pair[CB4-3.4-1433]/Phrasing-B

Verdict CONCERN. RFC 8414 §2 OPTIONAL: revocation_endpoint in discovery is not
mandated. Its absence when revocation is supported is a guidance-level gap.
CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1434]/A

A resource server hosts several API endpoints. Presented access tokens may
carry scopes `read`, `write`, and `admin`. Endpoints that perform write
operations accept tokens with any scope — including `read`-only tokens — and
execute the write operation without checking whether the presented token includes
the `write` scope. The RS team argues that scope enforcement duplicates the
authorization server's scope validation and that tokens carry valid signatures.

## Pair[CB4-3.4-1434]/B

CONCERN. RFC 6749 §7 provides guidance that resource servers should verify that
the scope of the presented access token is sufficient for the operation being
requested — scope validation at the RS layer is part of the defense-in-depth
model for OAuth 2.0 deployments. No unconditional MUST in RFC 6749 §7 mandates
that resource servers enforce scope at the operation level; the language is guidance-
level. The RS team's argument that tokens carry valid signatures is partially
correct — a valid signature does not mean the scope is appropriate for the operation.
The absence of RS-side scope enforcement is a real security gap; however, without
a MUST-level anchor, it remains guidance-level. No MUST is violated.
CONCERN not REPROCESS.

## Pair[CB4-3.4-1434]/Reason

RFC 6749 §7 guidance: RS should verify token scope is sufficient for requested
operation; RS accepts write operations with read-only tokens; no unconditional
MUST for RS scope enforcement → CONCERN.

## Pair[CB4-3.4-1434]/Phrasing-B

Verdict CONCERN. RFC 6749 §7 guidance: resource servers should enforce scope
at the operation level. No unconditional MUST mandates it. RS scope enforcement
absence is a guidance-level gap with real security impact. CONCERN not REPROCESS.
