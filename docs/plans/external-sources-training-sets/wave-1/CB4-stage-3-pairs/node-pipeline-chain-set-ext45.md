# Node Pipeline Chain Set Extension 45
# Sub-phase: 3.4 Security and Authorization Model
# Pair ID prefix: CB4-3.4-
# Sequences: 1331–1338 (16 instances — 8 base pairs × 2 phrasings)
# Sources: RFC 6750 §7, RFC 9101 §4,
#          RFC 7521 §4.8, RFC 6749 §5.1,
#          RFC 9126 §2.3, RFC 7662,
#          OIDC Core §5.5.1, RFC 8414 §2
# Running total after this file: 2676 instances (1338 base pairs)
# Formula check: 120 + (1338 − 60) × 2 = 120 + 2556 = 2676 ✓

---

## Pair[CB4-3.4-1331]/A

A resource server receives a request that includes the same Bearer token in both
the `Authorization: Bearer abc123` header and the request body as
`token=abc123`. The RS team verified the tokens are identical and accepted the
request, using the header value for access control decisions. Evaluate whether
the RS behavior is conformant.

## Pair[CB4-3.4-1331]/B

Node 1 — Domain Splitter: Bearer token transmission; multiple token methods in
single request; resource server acceptance logic. Routes to RFC Resolver with
tags: Bearer, token-transmission, multiple-methods, RFC-6750, request-binding.

Node 2 — RFC Resolver: Primary citation — RFC 6750 §7 MUST: resource servers MUST
reject requests with multiple Bearer token transmission methods and return an
`invalid_request` error. No exception exists when token values match across methods.
Resolver notes: the prohibition is about method multiplicity, not value multiplicity.

Node 3 — Obligation Classifier: RFC 6750 §7 → MUST reject. The RS accepted the
request instead of rejecting. MUST is unconditional: the obligation does not
depend on token identity. Classifier routes as MUST-level, unmet.

Node 4 — Scenario Instantiator: Concrete violation — Bearer token in Authorization
header AND in request body form parameter; tokens identical; RS accepted using
header value. The identical-value argument is incorrect as a justification — the
MUST applies to method multiplicity. An attacker can add a second token parameter
to a captured valid request to probe RS behavior.

Node 5 — Evidence Collector: Evidence of violation: (a) Authorization header
contains Bearer token; (b) body contains token= parameter; (c) RS accepted request;
(d) RFC 6750 §7 MUST not met. Counter-argument logged: "identical tokens, benign"
— rejected; MUST is method-count-based, not value-based.

Node 6 — Gap Analyzer: RS should detect the presence of both transmission methods
before applying any access control decision. The correct behavior is to reject
with invalid_request before examining the token value.

Node 7 — Verdict Engine: MUST violated. RFC 6750 §7: multiple transmission methods
must be rejected. Identical-value argument rejected. Verdict: REPROCESS.

Node 8 — Arbiter: Confirms. Compatibility argument rejected. RFC 6750 §7 MUST is
unconditional. REPROCESS.

Node 9 — Formatter: REPROCESS. RFC 6750 §7 MUST: when a Bearer token appears via
multiple transmission methods in the same request, the RS MUST reject with
invalid_request. Token identity across methods provides no exemption — the rejection
must occur before the token value is examined. REPROCESS.

## Pair[CB4-3.4-1331]/Reason

RFC 6750 §7 MUST: RS must reject requests with multiple Bearer token transmission
methods; both header and body present; tokens identical; identical-token argument
rejected → REPROCESS.

## Pair[CB4-3.4-1331]/Phrasing-B

Verdict REPROCESS. RFC 6750 §7 MUST: multiple Bearer token methods in a single
request require rejection with invalid_request. Token value identity is irrelevant
to this MUST. REPROCESS.

---

## Pair[CB4-3.4-1332]/A

A client sends a JAR request object to the authorization endpoint. The request
JWT payload contains `aud`, `exp`, `iat`, `response_type`, `client_id`,
`redirect_uri`, `scope`, and `state`, but does not include the `iss` claim.
The AS verifies the JWT signature against the client's registered public key
and processes the authorization request normally. The team argues that the
signature verification already proves the request object came from the key holder,
making the `iss` claim redundant.

## Pair[CB4-3.4-1332]/B

Node 1 — Domain Splitter: JAR (JWT-Secured Authorization Request); request object
required claims; iss claim absent; AS acceptance. Routes to RFC Resolver with
tags: JAR, request-object, iss-claim, RFC-9101, client-identity.

Node 2 — RFC Resolver: Primary citation — RFC 9101 §4 MUST: request objects MUST
contain the `iss` claim set to the client's `client_id`. The `iss` establishes
the client's identity in the request object independently of the signing key.
Resolver notes: signature verification proves key possession; `iss` proves client
identity within the token structure; they serve distinct security functions.

Node 3 — Obligation Classifier: RFC 9101 §4 → MUST include iss. The AS processed
the request without iss present. MUST unmet. Classifier routes as MUST-level, unmet.

Node 4 — Scenario Instantiator: Concrete violation — request JWT has no iss;
AS validated signature and accepted the request. The signature-is-sufficient
argument conflates key-holder proof with client identity assertion. A request
object without iss can be repurposed for a different client_id if an attacker
controls a different client's key.

Node 5 — Evidence Collector: Evidence of violation: (a) request JWT payload lacks
iss claim; (b) AS did not reject; (c) RFC 9101 §4 MUST unmet. Counter-argument
logged: "signature proves sender" — rejected; iss and signature serve distinct
functions.

Node 6 — Gap Analyzer: The AS must validate that iss is present and equals
client_id before processing a JAR request object. Signature validity is necessary
but not sufficient — the iss binding ties the request object to the specific client
whose registration is being invoked.

Node 7 — Verdict Engine: MUST violated. RFC 9101 §4: request objects must contain
iss. AS accepted request without iss. Verdict: REPROCESS.

Node 8 — Arbiter: Confirms. Signature-is-sufficient argument rejected. iss and
signature serve distinct functions in JAR security. REPROCESS.

Node 9 — Formatter: REPROCESS. RFC 9101 §4 MUST: JAR request objects must include
the `iss` claim equal to client_id. Signature verification proves key possession
but does not substitute for the `iss` identity assertion within the request object
structure. AS must reject request objects without iss. REPROCESS.

## Pair[CB4-3.4-1332]/Reason

RFC 9101 §4 MUST: JAR request object must include iss=client_id; iss absent from
request JWT; signature-is-sufficient argument rejected → REPROCESS.

## Pair[CB4-3.4-1332]/Phrasing-B

Verdict REPROCESS. RFC 9101 §4 MUST: iss must be present in request objects and
equal client_id. Signature proof and iss claim serve distinct security functions.
Absent iss violates the MUST. REPROCESS.

---

## Pair[CB4-3.4-1333]/A

A confidential client uses `private_key_jwt` to authenticate at the token endpoint.
The AS validates the JWT signature, checks `iss`, `sub`, `aud`, and `exp`, and
issues tokens. The AS never records the `jti` values from client assertions.
A penetration tester captures a valid client assertion and replays it six times
within its 60-second validity window, receiving six sets of tokens. The AS team
argues that 60-second assertions expire quickly enough to limit risk.

## Pair[CB4-3.4-1333]/B

Node 1 — Domain Splitter: Client authentication via private_key_jwt; jti replay
prevention; AS obligation to track assertion jti values. Routes to RFC Resolver
with tags: private_key_jwt, client-assertion, jti, replay-prevention, RFC-7521.

Node 2 — RFC Resolver: Primary citation — RFC 7521 §4.8 MUST: each JSON Web Token
MUST contain a unique jti; the authorization server MUST ensure that the same jti
is not reused within the assertion's validity window. Secondary citation — RFC 7521
§4.5: the jti claim enables the AS to detect and reject replayed assertions.
Resolver notes: exp limits the replay window in time but cannot prevent replay
within it.

Node 3 — Obligation Classifier: RFC 7521 §4.8 → MUST track jti uniqueness. AS
does not record jti; tester replayed assertion 6× within validity window; all six
succeeded. Classifier routes as MUST-level, unmet.

Node 4 — Scenario Instantiator: Concrete violation — 60-second assertion jti not
tracked; replayed 6 times within validity window; all 6 token issuances succeeded.
Attack surface: any intercepted client assertion can be replayed for up to 60
seconds to authenticate as the client multiple times.

Node 5 — Evidence Collector: Evidence of violation: (a) AS has no jti recording
mechanism; (b) same assertion submitted 6 times; (c) 6 sets of tokens issued;
(d) RFC 7521 §4.8 MUST not met. Counter-argument logged: "60-second expiry limits
risk" — rejected; exp limits the window but does not prevent replay within it.

Node 6 — Gap Analyzer: jti tracking is a mandatory replay-prevention control for
client assertions. Short-lived assertions reduce the replay window but are still
replayable within that window without jti tracking. The fix is an in-memory or
persistent jti store keyed to the assertion's expiry time.

Node 7 — Verdict Engine: MUST violated. RFC 7521 §4.8: AS must check jti
uniqueness. exp-sufficient argument rejected. Verdict: REPROCESS.

Node 8 — Arbiter: Confirms. Short expiry does not substitute for jti tracking.
Six-token replay demonstrates the attack is practical. REPROCESS.

Node 9 — Formatter: REPROCESS. RFC 7521 §4.8 MUST: the AS must record jti values
from client assertions and reject reuse within the assertion's validity window.
Short expiry limits the window of exploitation but does not prevent replay within
it. Not tracking jti allows unlimited replay for the assertion's lifetime. REPROCESS.

## Pair[CB4-3.4-1333]/Reason

RFC 7521 §4.8 MUST: AS must check jti uniqueness for client assertions; jti not
tracked; assertion replayed 6 times within 60-second window; exp-sufficient
argument rejected → REPROCESS.

## Pair[CB4-3.4-1333]/Phrasing-B

Verdict REPROCESS. RFC 7521 §4.8 MUST: jti must be unique and the AS must reject
reused assertion jtis within the validity window. Short expiry does not substitute
for jti tracking. Six-replay attack confirmed. REPROCESS.

---

## Pair[CB4-3.4-1334]/A

A client requests `scope=read write admin` from the authorization server. The AS
determines the client is only authorized for `read write` and issues a token for
those two scopes. The token response body contains `{"access_token":"...",
"token_type":"Bearer","expires_in":3600}` with no `scope` field. The client
assumes the full `read write admin` scope was granted and attempts admin operations
that fail silently at the RS. The team argues that scope information is redundant
since the token encodes the granted scope internally.

## Pair[CB4-3.4-1334]/B

Node 1 — Domain Splitter: Token response content; scope reduction notification;
RFC 6749 §5.1 MUST; client awareness of granted scope. Routes to RFC Resolver
with tags: token-response, scope, reduction, RFC-6749, client-notification.

Node 2 — RFC Resolver: Primary citation — RFC 6749 §5.1 MUST: if the scope of
the issued access token is different from the scope requested by the client, the
AS MUST include the `scope` response parameter to inform the client of the actual
scope granted. Resolver notes: the obligation runs to the client, not to the RS;
the client cannot reliably inspect the token's internal scope (especially with
opaque tokens).

Node 3 — Obligation Classifier: RFC 6749 §5.1 → MUST include scope when reduced.
Scope was reduced from `read write admin` to `read write`; `scope` absent from
response. MUST unmet. Classifier routes as MUST-level, unmet.

Node 4 — Scenario Instantiator: Concrete violation — client requested 3 scopes;
2 granted; response omits scope field; client assumes 3 scopes granted; attempts
admin operations that fail. The token-encodes-scope argument assumes clients
decode JWTs and inspect scope claims — opaque tokens provide no such access.

Node 5 — Evidence Collector: Evidence of violation: (a) requested scope was
`read write admin`; (b) issued scope was `read write`; (c) response body contains
no `scope` field; (d) RFC 6749 §5.1 MUST not met. Counter-argument logged:
"token encodes scope internally" — rejected; obligation is to the client in the
response, not to the RS in the token.

Node 6 — Gap Analyzer: The scope parameter in the token response enables clients
to adapt their behavior to the actual scope granted — abort if required scopes
were not granted, limit requests to the granted scope. Without it, clients may
silently operate with incorrect scope assumptions.

Node 7 — Verdict Engine: MUST violated. RFC 6749 §5.1: scope must be in response
when different from requested. Verdict: REPROCESS.

Node 8 — Arbiter: Confirms. Token-encodes-scope argument rejected. Client must
be notified via response body scope parameter. REPROCESS.

Node 9 — Formatter: REPROCESS. RFC 6749 §5.1 MUST: when the AS issues a token
with a different scope than requested, the `scope` parameter must be included in
the token response. The client depends on this notification to understand what was
actually granted. Internal token scope encoding does not satisfy this response-
level MUST. REPROCESS.

## Pair[CB4-3.4-1334]/Reason

RFC 6749 §5.1 MUST: scope must be included in token response when issued scope
differs from requested scope; scope absent from response; token-encodes-scope
argument rejected → REPROCESS.

## Pair[CB4-3.4-1334]/Phrasing-B

Verdict REPROCESS. RFC 6749 §5.1 MUST: scope must appear in the token response
when the granted scope differs from the requested scope. Client notification via
the response body is a distinct obligation from token-internal scope encoding.
REPROCESS.

---

## Pair[CB4-3.4-1335]/A

An authorization server supports Pushed Authorization Requests (PAR). A client
pushes an authorization request and receives a `request_uri` with `expires_in=60`.
The client attempts to use the `request_uri` at the authorization endpoint 95
seconds later. The AS queries its PAR record store, finds the record (not yet
garbage-collected), and processes the authorization normally. The team argues that
a 35-second grace period beyond the stated expiry prevents flow failures from
clock skew.

## Pair[CB4-3.4-1335]/B

Node 1 — Domain Splitter: PAR (Pushed Authorization Request); request_uri expiry
enforcement; AS obligation to reject expired request_uri values. Routes to RFC
Resolver with tags: PAR, request_uri, expiry, RFC-9126, invalid_request_uri.

Node 2 — RFC Resolver: Primary citation — RFC 9126 §2.3 MUST: the authorization
server MUST reject authorization requests that use a `request_uri` beyond its
expiry time and return `error=invalid_request_uri`. Resolver notes: the expiry
is a normative security control; database record presence does not constitute
validity; no grace-period provision exists in RFC 9126.

Node 3 — Obligation Classifier: RFC 9126 §2.3 → MUST reject expired request_uri.
request_uri used 95 seconds after 60-second expiry; AS accepted. MUST unmet.
Classifier routes as MUST-level, unmet.

Node 4 — Scenario Instantiator: Concrete violation — request_uri with expires_in=60
used 95 seconds after issuance; database record present; AS processed normally.
The grace-period argument introduces an undefined extension to the expiry contract.
Clock skew is typically handled by a few seconds, not 35 seconds.

Node 5 — Evidence Collector: Evidence of violation: (a) request_uri expires_in=60;
(b) used at t+95s; (c) AS located database record; (d) AS processed request; (e)
RFC 9126 §2.3 MUST not met. Counter-argument logged: "35-second grace period for
clock skew" — rejected; no grace-period provision; clock skew mitigation is
measured in single-digit seconds, not 35.

Node 6 — Gap Analyzer: The AS must check the expiry timestamp stored with the PAR
record and compare it to the current time at the moment the request_uri is used.
The check must happen regardless of whether the record is still in the database.
Grace periods for clock skew are not normatively defined in RFC 9126.

Node 7 — Verdict Engine: MUST violated. RFC 9126 §2.3: expired request_uri must
be rejected. Grace-period argument rejected. Verdict: REPROCESS.

Node 8 — Arbiter: Confirms. Database record presence does not override expiry.
Grace-period argument not grounded in RFC 9126. REPROCESS.

Node 9 — Formatter: REPROCESS. RFC 9126 §2.3 MUST: the AS must reject a
request_uri used beyond its expiry time with invalid_request_uri. Database record
retention is not a substitute for expiry validation. No grace-period provision
exists in RFC 9126. REPROCESS.

## Pair[CB4-3.4-1335]/Reason

RFC 9126 §2.3 MUST: AS must reject request_uri used after expiry; request_uri
used at t+95s despite expires_in=60; grace-period argument rejected → REPROCESS.

## Pair[CB4-3.4-1335]/Phrasing-B

Verdict REPROCESS. RFC 9126 §2.3 MUST: expired request_uri must be rejected.
Record presence in the AS database does not extend validity. 35-second grace period
is not an RFC-defined provision. REPROCESS.

---

## Pair[CB4-3.4-1336]/A

A resource server validates JWT access tokens via token introspection. To reduce
latency at high throughput, the RS caches each introspection response for 24 hours.
A token is issued with a 15-minute lifetime and revoked by the AS after 10 minutes.
The RS continues accepting the revoked token for 23 hours and 50 minutes based on
the cached introspection response. The RS team argues that no RFC specifies an
introspection cache TTL maximum.

## Pair[CB4-3.4-1336]/B

Node 1 — Domain Splitter: Token introspection caching; revocation window; cache
TTL vs. token exp; RS operational trade-off. Routes to RFC Resolver with tags:
introspection, cache, RFC-7662, revocation, exp, guidance.

Node 2 — RFC Resolver: Primary citation — RFC 7662 — no unconditional MUST
specifying a maximum cache duration for introspection responses. The `exp` field
in introspection responses conveys token expiry; guidance-level recommendations
suggest respecting exp when caching. Secondary citation — no ASVS MUST for
introspection cache duration. Resolver notes: the RS team is correct that no RFC
specifies a maximum TTL.

Node 3 — Obligation Classifier: No MUST for introspection cache TTL. Guidance-
level recommendation to align cache TTL with token exp. Classifier routes as
guidance-level, partially unmet. Gap is real: 24-hour cache creates 23h50m
revocation window for a 15-minute token.

Node 4 — Scenario Instantiator: Guidance gap — 24-hour cache; 15-minute token;
revoked at 10 minutes; RS accepts for 23h50m post-revocation. Operational risk
is high; no RFC MUST is violated.

Node 5 — Evidence Collector: Evidence of gap: (a) introspection responses cached
24 hours; (b) token revoked at t+10m; (c) RS accepts through t+23h50m; (d) no
RFC MUST for cache duration. RS team's regulatory argument is normatively correct.

Node 6 — Gap Analyzer: The guidance gap is significant. Best practice is to cache
introspection responses for at most the token's remaining lifetime (exp − now).
The 24-hour cache dramatically extends the effective revocation window beyond any
reasonable security posture.

Node 7 — Verdict Engine: No MUST violated. RFC 7662 guidance-level recommendation
for cache alignment with exp. Verdict: CONCERN.

Node 8 — Arbiter: Confirms. RS team is normatively correct — no MUST for cache
TTL maximum. Revocation window is a real security gap at guidance level. CONCERN
not REPROCESS.

Node 9 — Formatter: CONCERN. RFC 7662 contains no unconditional MUST specifying
a maximum introspection response cache duration. Caching beyond the token's `exp`
creates a post-revocation acceptance window — a guidance-level security gap with
measurable operational impact. The recommended practice is to limit cache TTL to
the token's remaining lifetime. No MUST violated. CONCERN not REPROCESS.

## Pair[CB4-3.4-1336]/Reason

RFC 7662 no unconditional MUST for introspection cache duration; 24-hour cache
creates revocation window; guidance-level gap only → CONCERN.

## Pair[CB4-3.4-1336]/Phrasing-B

Verdict CONCERN. RFC 7662 contains no MUST for introspection cache TTL. Caching
beyond token exp is a guidance gap that creates a prolonged revocation window.
No MUST violated. CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1337]/A

An OIDC relying party sends an authorization request with the `claims` parameter
specifying `{"userinfo":{"email":{"essential":true},"phone_number":{"essential":true}}}`.
The OIDC provider's authorization server ignores the `claims` parameter. The
UserInfo endpoint returns `{"sub":"...","email":"user@example.com"}` — `email`
is present but `phone_number` is absent. The RP's phone verification step fails
because `phone_number` is unavailable. The RP asks whether the AS's behavior
is normatively compliant.

## Pair[CB4-3.4-1337]/B

Node 1 — Domain Splitter: OIDC claims request parameter; essential claims; AS
obligation to honor claims requests; UserInfo response completeness. Routes to
RFC Resolver with tags: claims-parameter, essential, OIDC-Core, UserInfo,
phone_number.

Node 2 — RFC Resolver: Primary citation — OIDC Core §5.5.1: the `claims` parameter
requests specific Claims; the AS SHOULD return only Claims requested. Essential
claims indicate that the RP requires them; if the AS cannot return an essential
claim, it SHOULD return an error. These obligations are SHOULD-level. Resolver
notes: no unconditional MUST forces the AS to fulfill every claims request.

Node 3 — Obligation Classifier: OIDC Core §5.5.1 → SHOULD. The AS ignoring the
claims parameter entirely is a guidance gap, not a MUST violation. Classifier
routes as guidance-level, unmet.

Node 4 — Scenario Instantiator: Guidance gap — essential phone_number requested;
AS returns default claims; phone_number absent; RP workflow fails. Real-world
impact: RP cannot complete phone verification. SHOULD-level obligation not met.

Node 5 — Evidence Collector: Evidence of gap: (a) claims parameter in request
specifying essential phone_number; (b) AS ignores claims parameter; (c) UserInfo
response omits phone_number; (d) no MUST violation; OIDC Core §5.5.1 SHOULD not
fully met.

Node 6 — Gap Analyzer: The gap degrades the RP's ability to use the OIDC claims
request mechanism for essential claims. OIDC Core §5.5.1 acknowledges that AS
implementations may not support the claims parameter, which is why the obligation
is SHOULD-level rather than MUST.

Node 7 — Verdict Engine: No MUST violated. OIDC Core §5.5.1 SHOULD; guidance-level
gap. Verdict: CONCERN.

Node 8 — Arbiter: Confirms. AS's behavior is technically compliant at the normative
MUST level. SHOULD-level claims parameter support is not universally mandated.
CONCERN not REPROCESS.

Node 9 — Formatter: CONCERN. OIDC Core §5.5.1 SHOULD: the claims parameter is a
guidance-level mechanism. The AS is not unconditionally required to fulfill all
claims requests. Ignoring the claims parameter is a guidance gap that degrades RP
functionality but does not violate a normative MUST. CONCERN not REPROCESS.

## Pair[CB4-3.4-1337]/Reason

OIDC Core §5.5.1 SHOULD: AS should honor claims request parameter; no unconditional
MUST; AS ignoring claims parameter is guidance-level gap → CONCERN.

## Pair[CB4-3.4-1337]/Phrasing-B

Verdict CONCERN. OIDC Core §5.5.1 SHOULD: honoring the claims request parameter
is guidance-level. The AS is not unconditionally required to return all essential
claims. Guidance gap — CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1338]/A

An OIDC authorization server publishes discovery metadata at its well-known URL.
The metadata document includes all REQUIRED fields (issuer, authorization_endpoint,
token_endpoint, jwks_uri, response_types_supported, subject_types_supported,
id_token_signing_alg_values_supported) plus userinfo_endpoint. The document
does not include the `grant_types_supported` field. A client developer integrating
with the AS asks whether the missing field is a specification violation.

## Pair[CB4-3.4-1338]/B

Node 1 — Domain Splitter: AS discovery metadata; grant_types_supported field;
RFC 8414 §2 RECOMMENDED; optional metadata fields. Routes to RFC Resolver with
tags: discovery, metadata, grant_types_supported, RFC-8414, RECOMMENDED.

Node 2 — RFC Resolver: Primary citation — RFC 8414 §2: grant_types_supported is
RECOMMENDED (SHOULD) in AS metadata; when absent, clients assume the AS supports
only authorization_code and implicit. The obligation is SHOULD-level, not MUST.
Resolver notes: RFC 8414 explicitly allows the field to be omitted with defined
fallback behavior.

Node 3 — Obligation Classifier: RFC 8414 §2 → SHOULD / RECOMMENDED. The AS omits
grant_types_supported; RFC 8414 provides a defined fallback assumption for this
case. Classifier routes as guidance-level, unmet.

Node 4 — Scenario Instantiator: Guidance gap — grant_types_supported absent; client
cannot discover which grant types are supported without probing. The RFC provides
a default interpretation (only authorization_code and implicit supported) when
the field is absent.

Node 5 — Evidence Collector: Evidence of gap: (a) discovery document published;
(b) grant_types_supported absent; (c) RFC 8414 §2 SHOULD not met; (d) defined
fallback behavior exists; (e) no MUST violated.

Node 6 — Gap Analyzer: The gap is minor. RFC 8414 §2 provides a clear default
interpretation when grant_types_supported is absent. Clients can work around the
absence by attempting the grant type and handling an error response.

Node 7 — Verdict Engine: No MUST violated. RFC 8414 §2 SHOULD / RECOMMENDED;
guidance-level gap. Verdict: CONCERN.

Node 8 — Arbiter: Confirms. Developer's question answered: no MUST is violated.
SHOULD gap with defined fallback behavior. CONCERN not REPROCESS.

Node 9 — Formatter: CONCERN. RFC 8414 §2 RECOMMENDED: grant_types_supported is
a SHOULD-level field in AS discovery metadata. Its absence is a guidance gap —
RFC 8414 provides a defined fallback (only authorization_code/implicit assumed).
No MUST violated. CONCERN not REPROCESS.

## Pair[CB4-3.4-1338]/Reason

RFC 8414 §2 SHOULD / RECOMMENDED: grant_types_supported recommended in AS metadata;
no unconditional MUST; absent field is guidance-level gap with defined fallback →
CONCERN.

## Pair[CB4-3.4-1338]/Phrasing-B

Verdict CONCERN. RFC 8414 §2 RECOMMENDED: grant_types_supported is a SHOULD-level
field. Absence is a guidance gap; RFC 8414 provides a defined default interpretation.
No MUST violated. CONCERN not REPROCESS.
