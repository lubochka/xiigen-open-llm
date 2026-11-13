# Node Pipeline Chain Set Extension 47
# Sub-phase: 3.4 Security and Authorization Model
# Pair ID prefix: CB4-3.4-
# Sequences: 1383–1390 (16 instances — 8 base pairs × 2 phrasings)
# Sources: RFC 8693 §2.1, RFC 9101 §4 / RFC 9126,
#          OIDC Back-Channel Logout §2.6, RFC 6749 §4.1.1,
#          OIDC Core §3.1.3.7.6, RFC 7591 §2,
#          RFC 8628 §3.5, OIDC Core §3.1.3.7.13
# Running total after this file: 2780 instances (1390 base pairs)
# Formula check: 120 + (1390 − 60) × 2 = 120 + 2660 = 2780 ✓

---

## Pair[CB4-3.4-1383]/A

A service submits an RFC 8693 token exchange request with `grant_type`,
`subject_token`, and `requested_token_type` but omits `subject_token_type`.
The AS parses the subject token as a JWT and infers it is an access token, then
completes the exchange. Evaluate conformance.

## Pair[CB4-3.4-1383]/B

Node 1 — Domain Splitter: OAuth 2.0 Token Exchange; required request parameters;
subject_token_type omission; AS inference of token type. Routes to RFC Resolver
with tags: token-exchange, RFC-8693, subject_token_type, REQUIRED, invalid_request.

Node 2 — RFC Resolver: Primary citation — RFC 8693 §2.1 REQUIRED: subject_token_type
is a REQUIRED parameter identifying the type of the subject security token. Its
absence requires the AS to return error=invalid_request. No format-inference
exception exists. Resolver notes: different token types share the same JWT format;
type cannot be reliably inferred from structure.

Node 3 — Obligation Classifier: RFC 8693 §2.1 → REQUIRED (MUST include). AS
accepted the request without subject_token_type. MUST unmet. Classifier routes as
MUST-level, unmet.

Node 4 — Scenario Instantiator: Concrete violation — subject_token_type absent;
AS infers type from JWT format; exchange completes. Format inference is unreliable:
both ID tokens and access tokens are JWTs; an ID token could be submitted and
accepted as an access token.

Node 5 — Evidence Collector: Evidence of violation: (a) subject_token_type absent
from request; (b) AS did not return error=invalid_request; (c) RFC 8693 §2.1
REQUIRED not met. Counter-argument logged: "type inferred from JWT format" —
rejected; RFC 8693 §2.1 is unconditional.

Node 6 — Gap Analyzer: Without subject_token_type, the AS cannot determine the
semantics of the subject token. The REQUIRED designation exists precisely because
format is insufficient — the AS must know the token type explicitly.

Node 7 — Verdict Engine: MUST violated. RFC 8693 §2.1: subject_token_type is
REQUIRED. Inference argument rejected. Verdict: REPROCESS.

Node 8 — Arbiter: Confirms. REQUIRED parameters cannot be substituted by
AS-side inference. REPROCESS.

Node 9 — Formatter: REPROCESS. RFC 8693 §2.1 REQUIRED: subject_token_type must
be present in token exchange requests. Its absence requires error=invalid_request.
Format-based type inference does not satisfy the REQUIRED obligation. REPROCESS.

## Pair[CB4-3.4-1383]/Reason

RFC 8693 §2.1 REQUIRED: subject_token_type must be included in token exchange
request; absent; AS inferred type from format; inference argument rejected →
REPROCESS.

## Pair[CB4-3.4-1383]/Phrasing-B

Verdict REPROCESS. RFC 8693 §2.1 REQUIRED: subject_token_type is mandatory.
AS must return invalid_request when absent. Token format inference rejected.
REPROCESS.

---

## Pair[CB4-3.4-1384]/A

A client pushes a JAR to a PAR endpoint. The client authenticates at the PAR
endpoint using client_secret_basic. The pushed request body contains a signed
JWT in the `request` parameter. The AS extracts the authorization parameters
from the JWT payload without verifying the JWT's signature, relying on the
transport-level client authentication as sufficient proof of request integrity.

## Pair[CB4-3.4-1384]/B

Node 1 — Domain Splitter: JAR (JWT-Secured Authorization Request) pushed via PAR;
request object signature verification obligation; transport vs. payload authentication.
Routes to RFC Resolver with tags: JAR, PAR, RFC-9101, signature-verification,
request-object, transport-authentication.

Node 2 — RFC Resolver: Primary citation — RFC 9101 §4 MUST: the authorization
server must validate request objects including verifying their signatures. Secondary
citation — RFC 9126 §2: PAR accepts the request object at the push endpoint;
the validation obligations of RFC 9101 still apply. Transport authentication proves
the identity of the submitter; it does not validate the integrity of the JWT payload.

Node 3 — Obligation Classifier: RFC 9101 §4 → MUST verify JAR signature. AS did
not verify the signature. MUST unmet. Classifier routes as MUST-level, unmet.

Node 4 — Scenario Instantiator: Concrete violation — JAR pushed to PAR; client
authenticated via client_secret_basic; AS skips signature verification; authorization
parameters extracted from unverified JWT payload. An attacker who can intercept
and modify the JWT payload (without changing the signature) would have their
modified parameters processed by the AS.

Node 5 — Evidence Collector: Evidence of violation: (a) JAR in `request` parameter;
(b) AS does not verify signature; (c) RFC 9101 §4 MUST not met. Counter-argument:
"PAR client auth is sufficient" — rejected; transport auth and JWT integrity are
orthogonal.

Node 6 — Gap Analyzer: Transport authentication (client_secret_basic) proves the
client connected and submitted a request — it does not prove the JWT payload was
not tampered with after signing. RFC 9101 §4 MUST applies to the JWT as a data
structure regardless of transport.

Node 7 — Verdict Engine: MUST violated. RFC 9101 §4: JAR signature must be verified.
Transport-auth argument rejected. Verdict: REPROCESS.

Node 8 — Arbiter: Confirms. PAR client authentication does not substitute for JAR
signature verification. REPROCESS.

Node 9 — Formatter: REPROCESS. RFC 9101 §4 MUST: request object signatures must
be verified by the AS. PAR endpoint client authentication is a transport-level
control that does not validate JWT payload integrity. The AS must verify the JAR
signature independently. REPROCESS.

## Pair[CB4-3.4-1384]/Reason

RFC 9101 §4 MUST: AS must verify JAR signature; signature not verified when JAR
pushed via PAR; PAR client authentication argument rejected → REPROCESS.

## Pair[CB4-3.4-1384]/Phrasing-B

Verdict REPROCESS. RFC 9101 §4 MUST: JAR signature verification is mandatory
regardless of PAR transport-level authentication. Transport auth and JWT integrity
are orthogonal. REPROCESS.

---

## Pair[CB4-3.4-1385]/A

An OIDC provider issues back-channel logout tokens that include the `events` claim,
`iss`, `sub`, `iat`, `jti`, `aud`, and `exp`. To improve logout reliability, the
provider also includes the `nonce` value from the original authentication session
in the logout token payload. Evaluate whether the logout token structure is
conformant.

## Pair[CB4-3.4-1385]/B

Node 1 — Domain Splitter: OIDC Back-Channel Logout; logout token structure; nonce
claim prohibition; token type confusion prevention. Routes to RFC Resolver with
tags: back-channel-logout, logout-token, nonce, MUST-NOT, token-type-confusion.

Node 2 — RFC Resolver: Primary citation — OIDC Back-Channel Logout §2.6 MUST NOT:
logout tokens must not contain a nonce claim. The prohibition prevents logout
tokens from being mistaken for ID tokens (which use nonce for replay prevention)
and ID tokens from being replayed as logout tokens. Resolver notes: jti in the
logout token already provides replay prevention; nonce is redundant and prohibited.

Node 3 — Obligation Classifier: OIDC Back-Channel Logout §2.6 → MUST NOT include
nonce. Logout token contains nonce claim. MUST NOT violated. Classifier routes
as MUST NOT level, unmet.

Node 4 — Scenario Instantiator: Concrete violation — logout token payload includes
nonce claim; OIDC BCL §2.6 MUST NOT not met. Provider's reliability argument
does not constitute a valid exception.

Node 5 — Evidence Collector: Evidence of violation: (a) logout token payload
contains nonce; (b) OIDC BCL §2.6 MUST NOT not met; (c) jti already provides
replay prevention. Counter-argument logged: "nonce improves logout reliability"
— rejected; jti handles replay; nonce is prohibited.

Node 6 — Gap Analyzer: The nonce prohibition is a token-type discriminator. An RP
that accepts both ID tokens and logout tokens could be confused by a logout token
that looks like an ID token (has nonce). The prohibition is protective.

Node 7 — Verdict Engine: MUST NOT violated. OIDC BCL §2.6: nonce must not appear
in logout tokens. Verdict: REPROCESS.

Node 8 — Arbiter: Confirms. Reliability argument rejected. jti provides replay
prevention. REPROCESS.

Node 9 — Formatter: REPROCESS. OIDC Back-Channel Logout §2.6 MUST NOT: logout
tokens must not contain a nonce claim. The nonce prohibition prevents token type
confusion between logout tokens and ID tokens. jti already provides replay
prevention in logout tokens. REPROCESS.

## Pair[CB4-3.4-1385]/Reason

OIDC Back-Channel Logout §2.6 MUST NOT: logout tokens must not include nonce claim;
nonce present in logout token; reliability argument rejected; jti provides replay
protection → REPROCESS.

## Pair[CB4-3.4-1385]/Phrasing-B

Verdict REPROCESS. OIDC BCL §2.6 MUST NOT: nonce is prohibited in logout tokens
to prevent token type confusion. jti handles replay prevention. REPROCESS.

---

## Pair[CB4-3.4-1386]/A

A browser-based SPA submits an authorization request with `response_type=code`,
`redirect_uri=https://app.example.com/callback`, `scope=openid`, `state=xyz`, and
`code_challenge=...`. The `client_id` parameter is not present. The AS queries its
registry for clients whose `redirect_uri` matches and finds exactly one registered
client. The AS proceeds with the authorization flow for that client. Assess the
AS behavior.

## Pair[CB4-3.4-1386]/B

Node 1 — Domain Splitter: Authorization request required parameters; client_id
absent; AS redirect-URI lookup as substitute identification. Routes to RFC Resolver
with tags: authorization-request, client_id, REQUIRED, RFC-6749, invalid_request.

Node 2 — RFC Resolver: Primary citation — RFC 6749 §4.1.1 REQUIRED: the client_id
parameter is REQUIRED in authorization requests. The AS must return
error=invalid_request when client_id is absent. Resolver notes: redirect_uri lookup
is unreliable in multi-client deployments; client_id is the explicit required
identifier; RFC 6749 provides no redirect-URI-as-client-id provision.

Node 3 — Obligation Classifier: RFC 6749 §4.1.1 → REQUIRED (MUST include
client_id). AS accepted request without client_id. MUST unmet. Classifier routes
as MUST-level, unmet.

Node 4 — Scenario Instantiator: Concrete violation — client_id absent; AS found
one matching redirect_uri; AS inferred client_id; flow proceeded. Even with one
matching URI, the identification is AS-inferred rather than client-asserted,
violating the REQUIRED specification.

Node 5 — Evidence Collector: Evidence of violation: (a) client_id absent from
authorization request; (b) AS did not return error=invalid_request; (c) RFC 6749
§4.1.1 REQUIRED not met. Counter-argument logged: "redirect_uri uniquely identifies
client" — rejected; REQUIRED designation is unconditional.

Node 6 — Gap Analyzer: The REQUIRED constraint exists because client_id is the
authoritative client identifier — the AS should not infer it. In deployments where
redirect URIs are shared or change over time, the inference would fail silently.

Node 7 — Verdict Engine: MUST violated. RFC 6749 §4.1.1: client_id is REQUIRED.
Redirect-URI inference rejected. Verdict: REPROCESS.

Node 8 — Arbiter: Confirms. REQUIRED parameters cannot be substituted by AS
inference regardless of uniqueness. REPROCESS.

Node 9 — Formatter: REPROCESS. RFC 6749 §4.1.1 REQUIRED: client_id must be
present in authorization requests. Its absence requires error=invalid_request.
Redirect URI lookup does not substitute for explicit client identification.
REPROCESS.

## Pair[CB4-3.4-1386]/Reason

RFC 6749 §4.1.1 REQUIRED: client_id must be in authorization request; absent;
AS inferred client from redirect_uri; redirect-URI substitution rejected →
REPROCESS.

## Pair[CB4-3.4-1386]/Phrasing-B

Verdict REPROCESS. RFC 6749 §4.1.1 REQUIRED: client_id is mandatory in
authorization requests. AS must reject with invalid_request when absent. Redirect
URI inference is not an acceptable substitute. REPROCESS.

---

## Pair[CB4-3.4-1387]/A

An OIDC relying party receives an ID token with `iat` set to 90 minutes ago.
The token is not yet expired (exp is still in the future), the signature is valid,
iss and aud match, and the nonce is correct. The RP accepts the token without
performing any check on the time elapsed since issuance. The RP team argues that
exp is the normative time boundary and that iat checking introduces fragility
from clock differences.

## Pair[CB4-3.4-1387]/B

Node 1 — Domain Splitter: OIDC ID token iat freshness; RP validation pipeline;
optional staleness check; exp as normative boundary. Routes to RFC Resolver with
tags: ID-token, iat, freshness, OIDC-Core, optional-check, exp.

Node 2 — RFC Resolver: Primary citation — OIDC Core §3.1.3.7.6 provides that the
iat claim can be used to reject tokens issued too far away from the current time,
with the acceptable range being client-specific. This is descriptive guidance —
no MUST or SHOULD specifies a required maximum iat age. The normative time
boundary is exp per §3.1.3.7.5. Resolver notes: RP team is normatively correct.

Node 3 — Obligation Classifier: OIDC Core §3.1.3.7.6 → guidance / descriptive.
No MUST for iat staleness check. Classifier routes as guidance-level, absent.

Node 4 — Scenario Instantiator: Guidance gap — iat 90 minutes old; exp still valid;
RP omits iat staleness check; all other validation passes. No MUST violated.
RP's exp-as-boundary argument is normatively correct.

Node 5 — Evidence Collector: Evidence of gap: (a) iat is 90 minutes old; (b) RP
does not check iat staleness; (c) OIDC Core §3.1.3.7.6 is descriptive guidance
only; (d) no MUST for maximum iat age. RP team's argument is normatively accurate.

Node 6 — Gap Analyzer: The gap is minor in a deployment where exp is set
appropriately. iat staleness checking is a defense-in-depth measure that can
catch tokens from sessions that should have re-authenticated. However, no normative
obligation mandates it.

Node 7 — Verdict Engine: No MUST violated. OIDC Core §3.1.3.7.6 guidance;
iat staleness check is optional. Verdict: CONCERN.

Node 8 — Arbiter: Confirms. RP team's argument is normatively correct. CONCERN
not REPROCESS.

Node 9 — Formatter: CONCERN. OIDC Core §3.1.3.7.6 describes iat-staleness checking
as optional client-specific defense-in-depth. No MUST mandates a maximum iat age;
exp is the normative time boundary. Omitting iat staleness validation is a guidance-
level gap. CONCERN not REPROCESS.

## Pair[CB4-3.4-1387]/Reason

OIDC Core §3.1.3.7.6 guidance: iat staleness check is descriptive, not MUST; exp
is normative boundary; iat staleness omission is guidance-level → CONCERN.

## Pair[CB4-3.4-1387]/Phrasing-B

Verdict CONCERN. OIDC Core §3.1.3.7.6 guidance: iat-staleness check is optional.
exp is the normative time bound. No MUST violated. CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1388]/A

An OIDC provider's dynamic client registration endpoint returns a response
containing `client_id`, `client_secret`, `client_id_issued_at`, and
`client_secret_expires_at`. The `token_endpoint_auth_method` field is absent.
The registered client begins using `client_secret_basic` at the token endpoint
without being told this is the correct method. The developer asks whether the
absent field in the registration response constitutes a violation.

## Pair[CB4-3.4-1388]/B

Node 1 — Domain Splitter: Dynamic client registration response; token_endpoint_auth_method
field; RFC 7591 §2 SHOULD; default behavior when field is absent. Routes to RFC
Resolver with tags: dynamic-registration, token_endpoint_auth_method, RFC-7591,
optional-fields, default-behavior.

Node 2 — RFC Resolver: Primary citation — RFC 7591 §2 SHOULD: registration
responses should include all registered client metadata including
token_endpoint_auth_method. When absent, the default value is `client_secret_basic`.
No unconditional MUST mandates this specific field's inclusion. Resolver notes:
defined default behavior exists; clients can infer the method.

Node 3 — Obligation Classifier: RFC 7591 §2 → SHOULD. No MUST for explicit field
inclusion. Defined default applies. Classifier routes as guidance-level, absent.

Node 4 — Scenario Instantiator: Guidance gap — token_endpoint_auth_method absent
from registration response; client uses client_secret_basic (which is the default);
flow works. The defined default means the client receives implicit guidance.

Node 5 — Evidence Collector: Evidence of gap: (a) registration response lacks
token_endpoint_auth_method; (b) RFC 7591 §2 SHOULD not met; (c) default
(client_secret_basic) applies; (d) no MUST violated.

Node 6 — Gap Analyzer: The gap is minor due to the defined default. The SHOULD
reflects that explicit inclusion is preferred for clarity, not that its absence
is a MUST violation.

Node 7 — Verdict Engine: No MUST violated. RFC 7591 §2 SHOULD; defined default
applies. Verdict: CONCERN.

Node 8 — Arbiter: Confirms. No MUST for this field. Default behavior is normatively
defined. CONCERN not REPROCESS.

Node 9 — Formatter: CONCERN. RFC 7591 §2 SHOULD: registration responses should
include token_endpoint_auth_method. No MUST mandates it. When absent, the default
(client_secret_basic) applies. Guidance-level gap with defined fallback. CONCERN
not REPROCESS.

## Pair[CB4-3.4-1388]/Reason

RFC 7591 §2 SHOULD: token_endpoint_auth_method should be in registration response;
no MUST; defined default client_secret_basic applies; guidance-level gap → CONCERN.

## Pair[CB4-3.4-1388]/Phrasing-B

Verdict CONCERN. RFC 7591 §2 SHOULD: registration response should include
token_endpoint_auth_method. No MUST mandates it. Defined default applies.
CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1389]/A

An authorization server implements the Device Authorization Grant. The device
authorization response it returns includes `device_code`, `user_code`,
`verification_uri`, and `expires_in`, but omits the `interval` field. A client
device polls the token endpoint every 2 seconds. The AS team states that
`interval` is optional and that clients are expected to implement exponential
backoff. Assess the conformance of the AS response.

## Pair[CB4-3.4-1389]/B

Node 1 — Domain Splitter: Device Authorization Grant; device authorization
response fields; interval field; polling behavior; RFC 8628 §3.5. Routes to
RFC Resolver with tags: device-flow, interval, RFC-8628, optional-field, polling.

Node 2 — RFC Resolver: Primary citation — RFC 8628 §3.5: `interval` is OPTIONAL
in the device authorization response. When absent, the client SHOULD use a
minimum default of 5 seconds between polling attempts. No unconditional MUST
requires the AS to include `interval`. Resolver notes: AS team is normatively
correct.

Node 3 — Obligation Classifier: RFC 8628 §3.5 → OPTIONAL (no MUST for field).
Classifier routes as guidance-level, absent.

Node 4 — Scenario Instantiator: Guidance gap — interval absent; client polls at
2s instead of 5s default; AS does not rate-limit. No MUST violated. The 2-second
polling is a client-side gap (client should default to 5s per RFC 8628 §3.5).

Node 5 — Evidence Collector: Evidence of gap: (a) interval absent from device
authorization response; (b) RFC 8628 §3.5 OPTIONAL — AS team is correct; (c)
no MUST for interval inclusion; (d) client polling rate is a client-side concern.

Node 6 — Gap Analyzer: The gap is at the AS level: including interval would guide
clients to use the recommended polling rate. Without it, clients that don't
implement the 5-second default may over-poll. However, this is guidance-level.

Node 7 — Verdict Engine: No MUST violated. RFC 8628 §3.5 OPTIONAL; interval
absence is guidance-level. Verdict: CONCERN.

Node 8 — Arbiter: Confirms. AS team is normatively correct. CONCERN not REPROCESS.

Node 9 — Formatter: CONCERN. RFC 8628 §3.5 OPTIONAL: the interval field is not
required in the device authorization response. When absent, clients should default
to 5-second polling. No MUST violated. Guidance-level gap. CONCERN not REPROCESS.

## Pair[CB4-3.4-1389]/Reason

RFC 8628 §3.5 OPTIONAL: interval field is optional in device authorization response;
no MUST; absent field is guidance-level gap → CONCERN.

## Pair[CB4-3.4-1389]/Phrasing-B

Verdict CONCERN. RFC 8628 §3.5 OPTIONAL: interval is not required. When absent,
5-second default applies. No MUST violated. CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1390]/A

An OIDC relying party sends an authorization request with
`acr_values=urn:mace:incommon:iap:gold`. The returned ID token contains
`acr=urn:mace:incommon:iap:silver`. The RP's validation pipeline does not compare
the returned acr against the requested acr_values and proceeds with the session
as if gold-level authentication was achieved. The RP team argues that the presence
of any acr claim satisfies the acr_values request.

## Pair[CB4-3.4-1390]/B

Node 1 — Domain Splitter: OIDC acr_values request; returned acr validation;
RP-side obligation to check acr against requested levels. Routes to RFC Resolver
with tags: acr, acr_values, OIDC-Core, authentication-context, SHOULD.

Node 2 — RFC Resolver: Primary citation — OIDC Core §3.1.3.7.13 SHOULD: if
acr_values were requested, the Client SHOULD verify that the claimed acr value
is in the requested list and at the appropriate assurance level. The obligation
is SHOULD-level. Resolver notes: any-acr-satisfies-acr_values argument is
incorrect as best practice, but no MUST elevates the check to mandatory.

Node 3 — Obligation Classifier: OIDC Core §3.1.3.7.13 → SHOULD. No MUST for
acr verification. Classifier routes as guidance-level, unmet.

Node 4 — Scenario Instantiator: Guidance gap — gold requested; silver returned;
RP accepts without acr comparison. Security impact: high-assurance operations
proceed under lower-assurance authentication. No MUST violated.

Node 5 — Evidence Collector: Evidence of gap: (a) acr_values=gold requested;
(b) ID token returns acr=silver; (c) RP does not compare; (d) OIDC Core §3.1.3.7.13
SHOULD not met; (e) no MUST violated. Counter-argument logged: "any acr satisfies"
— rejected as incorrect best practice but not a MUST anchor.

Node 6 — Gap Analyzer: The gap has significant real-world impact in high-security
deployments. However, the normative level is SHOULD. The access control implications
are a real risk that SHOULD be addressed.

Node 7 — Verdict Engine: No MUST violated. OIDC Core §3.1.3.7.13 SHOULD;
guidance-level gap. Verdict: CONCERN.

Node 8 — Arbiter: Confirms. SHOULD-level gap with real security implications.
CONCERN not REPROCESS.

Node 9 — Formatter: CONCERN. OIDC Core §3.1.3.7.13 SHOULD: the RP should verify
that the returned acr satisfies the requested acr_values. No unconditional MUST
mandates this check. Accepting a lower-assurance acr without detection is a
guidance-level gap with access control implications. CONCERN not REPROCESS.

## Pair[CB4-3.4-1390]/Reason

OIDC Core §3.1.3.7.13 SHOULD: RP should check returned acr against requested
acr_values; no unconditional MUST; lower-assurance acr accepted without check →
CONCERN.

## Pair[CB4-3.4-1390]/Phrasing-B

Verdict CONCERN. OIDC Core §3.1.3.7.13 SHOULD: acr validation against acr_values
is recommended. No MUST mandates it. Lower-assurance acr accepted without detection
is a guidance-level security gap. CONCERN not REPROCESS.
