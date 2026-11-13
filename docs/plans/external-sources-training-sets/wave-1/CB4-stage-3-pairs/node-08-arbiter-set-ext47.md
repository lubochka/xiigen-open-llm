# Node 08 — Arbiter Set Extension 47
# Sub-phase: 3.4 Security and Authorization Model
# Pair ID prefix: CB4-3.4-
# Sequences: 1219–1226 (16 instances — 8 base pairs × 2 phrasings)
# Sources: RFC 6749 §4.1.1, OIDC Core §3.1.3.7.3, RFC 9700 §4.1.3,
#          ASVS V9.2.2, RFC 6749 §3.3, OIDC Core §2,
#          RFC 6750 §2.3, RFC 8414 §2
# Running total after this file: 2452 instances (1226 base pairs)
# Formula check: 120 + (1226 − 60) × 2 = 120 + 2332 = 2452 ✓

---

## Pair[CB4-3.4-1219]/A

An OIDC authorization server receives an authorization request that includes
`client_id`, `redirect_uri`, `state`, and `code_challenge` parameters, but
omits the `response_type` parameter entirely. Rather than returning an error,
the AS infers that the client intends `response_type=code` based on the presence
of `code_challenge`, and proceeds to issue an authorization code. The team argues
that `response_type=code` is the only sensible interpretation and that strict
parameter enforcement would harm developer experience.

## Pair[CB4-3.4-1219]/B

REPROCESS. RFC 6749 §4.1.1 designates `response_type` as a REQUIRED parameter in
the authorization request. RFC 6749 §4.1.2.1 specifies that when a REQUIRED request
parameter is absent, the AS MUST return an error response with `error=invalid_request`.
The AS's inference-based handling — substituting its own judgment for the absent REQUIRED
parameter — violates this requirement unconditionally. The developer-experience argument
is rejected: REQUIRED parameters are not optional regardless of whether their value could
be inferred; the explicit parameter provides an unambiguous machine-readable signal that
the AS cannot fabricate from co-present parameters. MUST violated; REPROCESS.

## Pair[CB4-3.4-1219]/Reason

RFC 6749 §4.1.1: response_type REQUIRED in authorization request; absent REQUIRED
parameter MUST trigger error=invalid_request per §4.1.2.1; inference substitutes AS
judgment for required explicit parameter → REPROCESS.

## Pair[CB4-3.4-1219]/Phrasing-B

Verdict REPROCESS. RFC 6749 §4.1.1 lists `response_type` as REQUIRED. RFC 6749
§4.1.2.1 MUST: absent REQUIRED parameter → error=invalid_request. The AS must not
infer the missing value from co-present parameters; inference substitutes arbitrary
interpretation for mandatory explicit client input. Developer-experience rationale
rejected; MUST violated. REPROCESS.

---

## Pair[CB4-3.4-1220]/A

An OIDC provider issues ID tokens that contain `iss`, `sub`, `exp`, `iat`, and
`nonce` claims but omit the `aud` claim entirely. The AS team argues that the `aud`
claim is redundant because the token is delivered directly to the requesting client
via the token endpoint's TLS-protected response, making the intended recipient
self-evident from the delivery channel.

## Pair[CB4-3.4-1220]/B

REPROCESS. OIDC Core §3.1.3.7.3 MUST: the ID token's `aud` claim MUST contain the
`client_id` of the OAuth 2.0 Client as an audience value. This requirement exists
so that the RP can verify that the ID token was issued for it, not merely that it
was delivered over a protected channel. The delivery-channel argument is rejected:
TLS protects transmission but does not establish intended audience in the token
itself; without `aud`, a client cannot programmatically verify that the ID token
was issued for it rather than for a different client at the same provider. An absent
`aud` enables token confusion attacks between clients at the same issuer. MUST
violated; REPROCESS.

## Pair[CB4-3.4-1220]/Reason

OIDC Core §3.1.3.7.3 MUST: aud claim must contain the requesting client's
client_id in every ID token; absent aud prevents RP audience verification; delivery-
channel argument rejected → REPROCESS.

## Pair[CB4-3.4-1220]/Phrasing-B

Verdict REPROCESS. OIDC Core §3.1.3.7.3 MUST: every ID token MUST include an `aud`
claim containing the requesting client's `client_id`. The TLS delivery argument is
rejected — transport protection and token-level audience binding are independent
security controls serving different threat models; absent `aud` enables cross-client
token confusion at the same issuer. MUST violated. REPROCESS.

---

## Pair[CB4-3.4-1221]/A

A web application is registered as a confidential client with the OIDC provider.
Its `redirect_uri` is registered as `http://app.example.com/callback` — plain HTTP,
not HTTPS. The development team argues that the production load balancer terminates
TLS, so the underlying application receives the callback over the internal HTTP
path, and that the user-visible URL at the browser is HTTPS due to the load balancer.

## Pair[CB4-3.4-1221]/B

REPROCESS. RFC 9700 §4.1.3 MUST: redirect URIs for clients other than native
applications MUST use the HTTPS URI scheme. The redirect_uri is the destination
for the authorization code in the authorization response — the URI scheme must be
HTTPS to protect the code in transit between the AS and the application. The
load-balancer argument is rejected by the same pattern as token endpoint and
authorization endpoint TLS requirements: MUST HTTPS applies to the registered URI
itself, not to downstream transport within the application's infrastructure;
a plain-HTTP registered redirect_uri allows interception of the authorization code
before the load balancer receives it, and enables open redirect exploitation if the
AS performs non-HTTPS redirects. MUST violated; REPROCESS.

## Pair[CB4-3.4-1221]/Reason

RFC 9700 §4.1.3 MUST HTTPS for non-native-app redirect URIs; http:// registered
redirect_uri violates this unconditionally; load-balancer argument rejected (MUST
applies to registered URI, not internal path) → REPROCESS.

## Pair[CB4-3.4-1221]/Phrasing-B

Verdict REPROCESS. RFC 9700 §4.1.3: redirect URIs for non-native clients MUST use
HTTPS. The http:// registered URI enables authorization code interception at the
redirect step; the load balancer's TLS termination does not satisfy the requirement
because the MUST applies to the URI itself. Load-balancer argument rejected. REPROCESS.

---

## Pair[CB4-3.4-1222]/A

An authorization server stores client secrets (`client_secret` values) in its
database as plain text strings. A database administrator who gains read access to
the client table can immediately extract all registered client secrets. The AS team
argues that the client secrets table is behind a dedicated service account with
restricted permissions, and that the effort required to compromise the service
account provides sufficient protection.

## Pair[CB4-3.4-1222]/B

REPROCESS. ASVS V9.2.2 MUST: client secrets MUST be stored using an approved
hashing algorithm (such as Argon2id, scrypt, or bcrypt). The access-control argument
is rejected: MUST NOT store plaintext credentials is unconditional — access controls
are an additional defense layer, not a substitute for hashing. A database
compromise, SQL injection vulnerability, backup exposure, or insider threat that
reaches the client table yields all registered secrets immediately if stored in
plaintext. Hashing ensures that even a full database read results in computationally
infeasible mass secret recovery. The "restricted service account" defense reduces
the attack surface but does not satisfy the ASVS MUST. MUST violated; REPROCESS.

## Pair[CB4-3.4-1222]/Reason

ASVS V9.2.2 MUST: client secrets MUST be stored hashed using an approved algorithm;
plaintext storage violates this unconditionally; access-control argument rejected
(controls reduce surface but do not substitute for hashing) → REPROCESS.

## Pair[CB4-3.4-1222]/Phrasing-B

Verdict REPROCESS. ASVS V9.2.2 MUST: client secrets require approved-algorithm
hashing at rest. Plaintext storage means any read access to the client table yields
all secrets directly. Service-account access controls are defense-in-depth that
reduce probability of breach but do not satisfy the MUST requirement for hashed
storage. MUST violated. REPROCESS.

---

## Pair[CB4-3.4-1223]/A

A service uses the `client_credentials` OAuth 2.0 grant to obtain access tokens.
Its authorization request omits the `scope` parameter entirely — the request body
contains only `grant_type=client_credentials`. The AS issues an access token with
the full set of scopes registered for the client (all five registered scopes). The
security team asks whether issuing tokens with all registered scopes when no scope
is specified constitutes a violation.

## Pair[CB4-3.4-1223]/B

CONCERN. RFC 6749 §3.3 classifies `scope` as optional in the token request. When
scope is omitted, the AS may default to the set of scopes previously established for
the client, which may include all registered scopes. RFC 6749 §3.3 explicitly states:
"If the client omits the scope parameter when requesting authorization, the
authorization server MUST either process the request using a pre-defined default
value or fail the request." Defaulting to the full registered scope set is
therefore a permitted behavior. The CONCERN is guidance-level: RFC 9700 and SSDF
PO.1 guidance recommend that the client explicitly specify only the minimum required
scope per request to implement least-privilege, and that AS documentation should
describe the default scope behavior. No MUST is violated. CONCERN not REPROCESS.

## Pair[CB4-3.4-1223]/Reason

RFC 6749 §3.3: scope OPTIONAL in token request; AS may default to pre-established
registered scopes when scope absent — this is explicitly permitted; least-privilege
guidance recommends explicit minimal scope; no unconditional MUST violated →
CONCERN.

## Pair[CB4-3.4-1223]/Phrasing-B

Verdict CONCERN. RFC 6749 §3.3 explicitly permits AS to default to a pre-defined
scope set when the client omits the scope parameter — issuing full registered scopes
on an unscoped request is allowed behavior. The guidance-level gap is that the client
should explicitly request only the minimum scope needed per call, and the AS should
document its default behavior. No MUST is violated. CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1224]/A

An OIDC provider issues ID tokens to clients that have registered multiple audiences
— for example, a client registered to serve both `https://app1.example.com` and
`https://app2.example.com`. The issued ID token includes `"aud": ["client-abc",
"https://app1.example.com", "https://app2.example.com"]` as a multi-value array
but does not include an `azp` (authorized party) claim. An RP receiving the ID token
is unable to determine from the token itself which application is the intended
authorized party for this specific issuance.

## Pair[CB4-3.4-1224]/B

CONCERN. OIDC Core §2 specifies that the `azp` claim SHOULD be present when the
`aud` claim contains a multi-valued array, and is required when the authorized party
differs from the sole audience. The SHOULD obligation is guidance-level — OIDC Core
does not mandate `azp` with an unconditional MUST. Without `azp`, clients must
determine their authorization status from the `aud` array alone; when aud is
multi-valued, this creates ambiguity about which party is the specifically intended
recipient of this token issuance. The gap is RECOMMENDED/SHOULD-level: CONCERN
not REPROCESS.

## Pair[CB4-3.4-1224]/Reason

OIDC Core §2: azp SHOULD be present when aud is a multi-valued array; SHOULD-level
obligation; no unconditional MUST; RP authorization ambiguity without azp → CONCERN.

## Pair[CB4-3.4-1224]/Phrasing-B

Verdict CONCERN. OIDC Core §2 defines `azp` as SHOULD-present when the audience
claim contains multiple values. No unconditional MUST mandates its inclusion. The
absent `azp` creates an authorized-party ambiguity when aud is a multi-valued array.
SHOULD-level guidance gap → CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1225]/A

An API client accesses the OIDC UserInfo endpoint by appending the access token as
a URL query parameter: `GET /userinfo?access_token=eyJhbGci...`. The client argues
that the UserInfo endpoint accepts the token regardless of submission method and
that query parameters are "more convenient" for certain client architectures.
The OIDC provider's server logs contain the full UserInfo URL including the access
token in every request log entry.

## Pair[CB4-3.4-1225]/B

CONCERN. RFC 6750 §2.3 defines access token submission via query parameter as a
SHOULD NOT transmission method — it is explicitly a last resort for clients unable
to use the Authorization header. OIDC Core §5.3.1 specifies the Authorization header
with Bearer scheme as the primary required submission method for UserInfo requests.
The server-side logging consequence is significant: access tokens appear in full in
URL-format log entries across all log aggregators and proxies in the request chain.
However, RFC 6750 §2.3 does not use unconditional MUST NOT language — the method is
defined as discouraged but not prohibited. The logging risk elevates the concern but
does not convert the guidance-level gap to REPROCESS. CONCERN not REPROCESS.

## Pair[CB4-3.4-1225]/Reason

RFC 6750 §2.3: query-parameter token submission SHOULD NOT (last resort); OIDC Core
§5.3.1: Authorization: Bearer header is primary method; token appears in server logs
via query parameter; no unconditional MUST NOT → CONCERN.

## Pair[CB4-3.4-1225]/Phrasing-B

Verdict CONCERN. RFC 6750 §2.3 explicitly designates the query-parameter method as
SHOULD NOT — a last resort when the Authorization header is unavailable. OIDC Core
§5.3.1 requires the Bearer header for UserInfo. The access token appearing in logged
URLs is a meaningful risk but RFC 6750 §2.3 stops short of MUST NOT; guidance gap
and logging risk together → CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1226]/A

An authorization server publishes its metadata document at
`/.well-known/oauth-authorization-server`. The metadata document contains
`authorization_endpoint`, `token_endpoint`, `jwks_uri`, and
`response_types_supported` but omits the `issuer` field entirely. An RP
attempting programmatic discovery cannot determine the issuer identifier from
the metadata document and must hard-code it or infer it from the document URL.
The AS team argues that `issuer` is obvious from the URL used to retrieve the
document.

## Pair[CB4-3.4-1226]/B

REPROCESS. RFC 8414 §2 designates `issuer` as a REQUIRED field in the AS metadata
document. REQUIRED in this context means the field MUST be present; absent `issuer`
in the metadata document makes it non-conformant with RFC 8414. The AS team's
inference argument is rejected: RFC 8414 §3 further specifies that the `issuer`
value in the document MUST be identical to the well-known URL prefix used to derive
the metadata endpoint, reinforcing that the field must be explicitly present and
exactly correct — not inferred by RPs. An RP relying on hard-coded issuer values
cannot validate that the metadata document corresponds to the expected issuer,
enabling metadata injection attacks. MUST violated; REPROCESS.

## Pair[CB4-3.4-1226]/Reason

RFC 8414 §2: issuer is a REQUIRED field in AS metadata document; absent REQUIRED
field makes document non-conformant; §3 MUST exact match with well-known URL
prefix; inference argument rejected → REPROCESS.

## Pair[CB4-3.4-1226]/Phrasing-B

Verdict REPROCESS. RFC 8414 §2 marks `issuer` as REQUIRED in the AS metadata
document — a MUST-level presence requirement. RFC 8414 §3 further requires the
`issuer` value to exactly match the well-known URL prefix used to derive the
metadata endpoint. The inference argument is rejected: RPs must verify the field
explicitly; an absent field prevents programmatic issuer validation. MUST violated.
REPROCESS.
