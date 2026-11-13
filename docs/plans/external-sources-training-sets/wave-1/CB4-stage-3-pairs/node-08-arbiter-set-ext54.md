# Node 08 — Arbiter Set Extension 54
# Sub-phase: 3.4 Security and Authorization Model
# Pair ID prefix: CB4-3.4-
# Sequences: 1401–1408 (16 instances — 8 base pairs × 2 phrasings)
# Sources: RFC 8414 §3, RFC 6749 §5.1,
#          RFC 9126 §2.2, OIDC Core §5.3.2,
#          OIDC Discovery §3, RFC 9101 §10.3,
#          OIDC Core §3.1.3.7.4, RFC 9700 §4.3.1
# Running total after this file: 2816 instances (1408 base pairs)
# Formula check: 120 + (1408 − 60) × 2 = 120 + 2696 = 2816 ✓

---

## Pair[CB4-3.4-1401]/A

An OIDC authorization server publishes a discovery document at
`https://as.example.com/.well-known/openid-configuration`. The discovery
document's `issuer` field contains `https://auth.differentdomain.com` — a value
that does not match the domain from which the discovery document was served.
Relying parties retrieve the discovery document and use the `issuer` value for
validation. The AS team argues that the issuer field reflects their canonical
identity and that the serving URL is an operational alias.

## Pair[CB4-3.4-1401]/B

REPROCESS. RFC 8414 §3 MUST: the issuer value returned in the authorization
server metadata MUST be identical to the well-known URI prefix from which the
metadata was retrieved — the well-known URL for `https://as.example.com` is
`https://as.example.com/.well-known/openid-configuration`, and the `issuer` MUST
be `https://as.example.com`. A mismatch allows phishing attacks where a malicious
AS publishes a discovery document that claims to be a trusted issuer. The
operational-alias argument is rejected: RFC 8414 §3 is unconditional; the issuer
field is the normative identity anchor and must match the URL from which it was
served. MUST violated; REPROCESS.

## Pair[CB4-3.4-1401]/Reason

RFC 8414 §3 MUST: issuer in discovery metadata must match well-known URL prefix;
issuer points to different domain; operational-alias argument rejected →
REPROCESS.

## Pair[CB4-3.4-1401]/Phrasing-B

Verdict REPROCESS. RFC 8414 §3 MUST: the issuer value must exactly match the
well-known URL prefix. A mismatch enables phishing via discovery document spoofing.
Operational alias does not override the MUST. REPROCESS.

---

## Pair[CB4-3.4-1402]/A

An authorization server issues access tokens. The token endpoint response body
contains `{"access_token":"abc123","expires_in":3600,"scope":"read"}` but
does not include the `token_type` parameter. Client libraries that follow RFC 6750
check `token_type` to determine how to transmit the access token and fail when
the field is absent. The AS team argues that all tokens issued are Bearer tokens
and that `token_type` is redundant context.

## Pair[CB4-3.4-1402]/B

REPROCESS. RFC 6749 §5.1 specifies that `token_type` is a REQUIRED parameter in
the access token response. Clients use `token_type` to determine how to present
the token in subsequent requests — the value "Bearer" directs RFC 6750 processing,
while other registered types (such as "DPoP") require different handling. Clients
that encounter an absent `token_type` cannot reliably determine the token handling
scheme. The redundancy argument is rejected: the REQUIRED designation in RFC 6749
§5.1 is unconditional; even if all issued tokens are Bearer tokens, the field must
be present. MUST violated; REPROCESS.

## Pair[CB4-3.4-1402]/Reason

RFC 6749 §5.1 REQUIRED: token_type must be included in access token response;
absent from response; redundancy argument rejected → REPROCESS.

## Pair[CB4-3.4-1402]/Phrasing-B

Verdict REPROCESS. RFC 6749 §5.1 REQUIRED: token_type is a mandatory field in
access token responses. Its absence prevents clients from determining token
handling semantics. Redundancy arguments do not override a REQUIRED field. REPROCESS.

---

## Pair[CB4-3.4-1403]/A

An authorization server supports PAR. When a client pushes an authorization
request, the AS validates the request, stores it, and returns an HTTP 201 response
with body `{"expires_in":90}`. The `request_uri` field is absent from the PAR
response. The AS team argues that `expires_in` provides sufficient information
for clients to know they must quickly initiate the authorization flow.

## Pair[CB4-3.4-1403]/B

REPROCESS. RFC 9126 §2.2 specifies that the PAR response MUST include a
`request_uri` value and an `expires_in` value. The `request_uri` is the reference
that the client must use in the authorization request — without it, the client
cannot use PAR at all. The expires_in-is-sufficient argument is rejected: `expires_in`
tells the client how long the reference is valid; `request_uri` is the reference
itself. Both are REQUIRED elements of the PAR response per RFC 9126 §2.2. MUST
violated; REPROCESS.

## Pair[CB4-3.4-1403]/Reason

RFC 9126 §2.2 REQUIRED: PAR response must include request_uri and expires_in;
request_uri absent from response; expires_in-sufficient argument rejected →
REPROCESS.

## Pair[CB4-3.4-1403]/Phrasing-B

Verdict REPROCESS. RFC 9126 §2.2 REQUIRED: the PAR response must contain
request_uri. Without it, the PAR flow cannot proceed. expires_in alone is
insufficient. REPROCESS.

---

## Pair[CB4-3.4-1404]/A

An OIDC provider's UserInfo endpoint returns the user's profile information
successfully. However, the HTTP response has `Content-Type: text/plain` rather
than `application/json`. Client libraries that process the UserInfo response
parse the body as JSON successfully (the body is valid JSON), but the MIME type
mismatch causes some strict HTTP clients to reject the response. The AS team
argues that the body content is what matters and that the Content-Type is advisory.

## Pair[CB4-3.4-1404]/B

REPROCESS. OIDC Core §5.3.2 MUST: the UserInfo endpoint MUST return a
`Content-Type` of `application/json` unless the response is signed or encrypted
(in which case `application/jwt` is appropriate). The Content-Type is not merely
advisory — it is the HTTP mechanism by which clients determine how to interpret
the response body. A `text/plain` response type for JSON content is non-conformant
per OIDC Core §5.3.2, and clients that strictly follow the specification will
reject the response before attempting JSON parsing. The argument that the body is
parseable is rejected: the MUST applies to the Content-Type header independently
of whether the body happens to be valid JSON. MUST violated; REPROCESS.

## Pair[CB4-3.4-1404]/Reason

OIDC Core §5.3.2 MUST: UserInfo response must have Content-Type application/json;
response returns text/plain; body-content argument rejected → REPROCESS.

## Pair[CB4-3.4-1404]/Phrasing-B

Verdict REPROCESS. OIDC Core §5.3.2 MUST: the UserInfo endpoint must return
Content-Type: application/json. text/plain is non-conformant regardless of body
content. REPROCESS.

---

## Pair[CB4-3.4-1405]/A

An OIDC authorization server publishes a discovery document that contains all
REQUIRED fields: `issuer`, `authorization_endpoint`, `token_endpoint`,
`jwks_uri`, `response_types_supported`, `subject_types_supported`, and
`id_token_signing_alg_values_supported`. The `userinfo_endpoint` field is absent
from the discovery document. Client developers cannot determine the UserInfo
endpoint URL from the discovery document. The AS team argues that UserInfo is
optional and clients can hardcode the URL.

## Pair[CB4-3.4-1405]/B

CONCERN. OIDC Discovery §3 lists `userinfo_endpoint` as RECOMMENDED — it should
be included when the AS provides a UserInfo endpoint, but no unconditional MUST
requires its presence in the discovery document. The AS team is correct that the
UserInfo endpoint is optional in OIDC Core — not all OIDC deployments provide it.
The hardcode-URL argument is accepted as a workaround, though it tightly couples
clients to the AS's infrastructure. The absence of `userinfo_endpoint` from the
discovery document when a UserInfo endpoint exists is a guidance-level gap.
No MUST is violated. CONCERN not REPROCESS.

## Pair[CB4-3.4-1405]/Reason

OIDC Discovery §3 RECOMMENDED: userinfo_endpoint should be in discovery document
when available; no unconditional MUST; hardcode argument acknowledged; guidance-
level gap → CONCERN.

## Pair[CB4-3.4-1405]/Phrasing-B

Verdict CONCERN. OIDC Discovery §3 RECOMMENDED: userinfo_endpoint should be
present when provided. No MUST mandates it. Its absence from discovery is a
guidance-level gap. CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1406]/A

A client sends a JAR (JWT-Secured Authorization Request) to an OIDC provider.
The request JWT payload contains `iss`, `aud`, `exp`, `iat`, and the standard
authorization parameters. The payload does not include a `nbf` (not-before) claim.
The AS accepts and processes the request object. A security reviewer asks whether
the absent `nbf` claim constitutes a specification violation.

## Pair[CB4-3.4-1406]/B

CONCERN. RFC 9101 §10.3 provides guidance that limiting the lifetime of request
objects (through appropriate `exp` and `nbf` claims) reduces the window of
exploitation if a request object is captured. The `nbf` claim is not mandated as
an unconditional MUST — RFC 9101 does not specify that `nbf` must be present in
request objects. The `exp` claim establishes the forward time boundary for the
request object's validity. The absence of `nbf` means the request object can be
used from the moment it was issued without a defined "not before" window —
a minor security gap without a MUST anchor. No unconditional MUST is violated.
CONCERN not REPROCESS.

## Pair[CB4-3.4-1406]/Reason

RFC 9101 §10.3 guidance: limiting request object lifetime via exp and nbf
recommended; nbf is not unconditional MUST; absent nbf is guidance-level gap →
CONCERN.

## Pair[CB4-3.4-1406]/Phrasing-B

Verdict CONCERN. RFC 9101 §10.3 guidance: nbf in request objects is a recommended
lifetime control. No MUST mandates nbf. Its absence is a guidance-level gap.
CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1407]/A

An OIDC relying party receives an ID token with `aud=["rp-client","analytics-svc"]`
(multi-valued audience). The token also contains `azp="rp-client"`. The RP's
validation pipeline verifies that its own `client_id` is in the `aud` array, that
signature, `iss`, `exp`, and `nonce` are correct, but it does not check that the
`azp` value equals its own `client_id`. The azp value is correct. The team argues
that since aud membership is verified, azp provides no additional check value.

## Pair[CB4-3.4-1407]/B

CONCERN. OIDC Core §3.1.3.7.4 SHOULD: when the ID token has multiple audience
values, the RP SHOULD verify that its own `client_id` is the value of the `azp`
claim. This is a SHOULD-level validation — not an unconditional MUST. The RP's
argument that aud membership verification already confirms that the token was
issued to the RP is partially correct at the MUST level; however, the azp check
provides additional protection against multi-audience token misuse. The azp
validation gap is guidance-level. The fact that azp is correct in this scenario
does not change the assessment — the issue is that the RP does not check it.
No MUST is violated. CONCERN not REPROCESS.

## Pair[CB4-3.4-1407]/Reason

OIDC Core §3.1.3.7.4 SHOULD: RP should validate azp equals client_id when aud
is multi-valued; RP omits azp check; no unconditional MUST; guidance-level gap →
CONCERN.

## Pair[CB4-3.4-1407]/Phrasing-B

Verdict CONCERN. OIDC Core §3.1.3.7.4 SHOULD: azp validation is recommended when
aud is multi-valued. No MUST mandates it. Omitting the azp check is a guidance-
level gap. CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1408]/A

An authorization server issues authorization codes and access tokens for public
clients (clients without a `client_secret`, such as SPAs and native apps) without
requiring PKCE. Public clients can complete the authorization code flow by
presenting just the authorization code at the token endpoint, without providing
a `code_verifier`. The AS team argues that PKCE is optional for confidential
clients and that for public clients they rely on redirect URI exact-match
validation to prevent code interception.

## Pair[CB4-3.4-1408]/B

CONCERN. RFC 9700 §4.3.2 SHOULD: authorization servers should require PKCE for
all public clients to prevent authorization code injection and interception attacks.
No unconditional MUST in RFC 9700 mandates PKCE for public clients — the
requirement is SHOULD-level. The AS team's redirect URI exact-match argument is
acknowledged as a partial compensating control; however, redirect URI matching
alone does not prevent authorization code injection when a stolen code can be
used by any client that completes the flow before the legitimate client. The gap
is real: without PKCE, public clients are vulnerable to authorization code
injection. No MUST is violated. CONCERN not REPROCESS.

## Pair[CB4-3.4-1408]/Reason

RFC 9700 §4.3.2 SHOULD: PKCE should be required for public clients; AS doesn't
enforce PKCE for public clients; redirect-URI compensating control argument
acknowledged; no unconditional MUST → CONCERN.

## Pair[CB4-3.4-1408]/Phrasing-B

Verdict CONCERN. RFC 9700 §4.3.2 SHOULD: requiring PKCE for public clients is
recommended. No unconditional MUST mandates it. PKCE absence for public clients
is a guidance-level gap with meaningful injection risk. CONCERN not REPROCESS.
