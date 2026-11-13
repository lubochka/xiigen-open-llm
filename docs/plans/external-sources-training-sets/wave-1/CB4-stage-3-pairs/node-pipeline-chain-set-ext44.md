# Node Pipeline Chain Set Extension 44
# Sub-phase: 3.4 Security and Authorization Model
# Pair ID prefix: CB4-3.4-
# Sequences: 1305–1312 (16 instances — 8 base pairs × 2 phrasings)
# Sources: OIDC Core §3.1.3.7.3, CWE-347 / ASVS V9.1.1,
#          RFC 6749 §3.1.2.3, RFC 6749 §3.1.2,
#          OIDC Core §3.1.3.7 / CWE-347, OIDC Core §5.5,
#          RFC 6750 §3.1, RFC 6749 / ASVS V6.4
# Running total after this file: 2624 instances (1312 base pairs)
# Formula check: 120 + (1312 − 60) × 2 = 120 + 2504 = 2624 ✓

---

## Pair[CB4-3.4-1305]/A

An OIDC relying party receives an ID token from a trusted issuer. Its validation
pipeline checks that `iss` matches the trusted issuer, the signature verifies
against the JWKS, `exp` has not elapsed, and `nonce` matches. It also checks that
the `aud` claim is present and non-empty. The RP accepts the token. The token's
`aud` array is `["other-client-1", "other-client-2"]` — neither entry is the RP's
own `client_id`. The RP team argues that checking the issuer and the presence of
an audience field is sufficient.

## Pair[CB4-3.4-1305]/B

Node 1 — Domain Splitter: OIDC ID token validation; audience membership check;
relying-party-side obligation. Routes to RFC Resolver with tags: OIDC, aud,
audience-membership, client_id, ID-token-validation.

Node 2 — RFC Resolver: Primary citation — OIDC Core §3.1.3.7.3: the Client MUST
verify that the aud (audience) Claim contains its client_id value registered at
the Issuer. Secondary citation — RFC 7519 §4.1.3: aud identifies the recipients
that the JWT is intended for; the principal MUST identify itself as one of the
intended audiences. Resolver notes: MUST membership check, not MUST presence check.

Node 3 — Obligation Classifier: OIDC Core §3.1.3.7.3 → MUST (membership). Present-
and-non-empty check satisfies a weaker condition than membership. The gap is
mandatory: checking truthy existence does not satisfy MUST membership verification.
Classifier routes as MUST-level obligation, unmet.

Node 4 — Scenario Instantiator: Concrete violation — aud = ["other-client-1",
"other-client-2"]; RP's client_id not present; RP accepted the token. This is the
canonical cross-client token confusion attack: a token issued for client-1 is
accepted by a different RP at the same issuer.

Node 5 — Evidence Collector: Evidence of violation: (a) aud array does not contain
RP's client_id; (b) RP's validation code checks `aud.length > 0`, not
`aud.contains(client_id)`; (c) token was issued for different clients; (d) RP
accepted token. Counter-argument logged: "issuer and audience field verified" —
does not satisfy MUST membership check.

Node 6 — Gap Analyzer: Truthy-audience check is a logical gap — it prevents null-
aud tokens from passing but does not prevent cross-client token reuse. Gap is
normative: OIDC Core §3.1.3.7.3 assigns a membership MUST to the RP independently.
Gap enables token confusion attack by design.

Node 7 — Verdict Engine: MUST violated. OIDC Core §3.1.3.7.3 RP MUST verify own
client_id is in aud array. RP accepted a token whose aud does not contain its
client_id. Verdict: REPROCESS.

Node 8 — Arbiter: Confirms. Truthy-existence argument rejected. Membership check
is the normative obligation. REPROCESS.

Node 9 — Formatter: REPROCESS. OIDC Core §3.1.3.7.3 MUST: RP must verify own
client_id is a member of the aud array. Presence of a non-empty aud array is not
sufficient; the RP's client_id must appear as a value. Token with aud containing
only other clients' identifiers is a token issued for a different RP — cross-client
token confusion. REPROCESS.

## Pair[CB4-3.4-1305]/Reason

OIDC Core §3.1.3.7.3 MUST: RP must verify own client_id in aud array; truthy-
existence check does not satisfy membership MUST; cross-client token accepted →
REPROCESS.

## Pair[CB4-3.4-1305]/Phrasing-B

Verdict REPROCESS. OIDC Core §3.1.3.7.3 MUST: the RP must confirm its client_id
is present in the aud array — a non-empty aud does not satisfy this. Token issued
for other clients was accepted, enabling cross-client token confusion. REPROCESS.

---

## Pair[CB4-3.4-1306]/A

A resource server validates JWT access tokens by Base64URL-decoding the payload,
parsing the JSON, and extracting the `sub` and `scope` claims for rate limiting
and access-control decisions. The RS does not perform cryptographic signature
verification — it does not fetch the AS's JWKS or verify the signature segment.
The RS team argues that the AS is trusted and HTTPS transport between callers and
the RS prevents payload tampering.

## Pair[CB4-3.4-1306]/B

Node 1 — Domain Splitter: JWT access token validation at resource server; signature
verification omission; claim-based authorization without cryptographic assurance.
Routes to RFC Resolver with tags: JWT, signature-verification, CWE-347, ASVS,
resource-server, access-control.

Node 2 — RFC Resolver: Primary citation — CWE-347: improper verification of
cryptographic signature; leads to unauthorized trust of unverified data. Secondary
citation — ASVS V9.1.1: all JWTs used for authorization decisions MUST have their
signatures cryptographically verified before claims are used. Resolver notes:
HTTPS transport is orthogonal to JWT authenticity.

Node 3 — Obligation Classifier: CWE-347 + ASVS V9.1.1 → MUST-level: signature
verification is a prerequisite for trusting JWT claims. The obligation is on the
resource server independently of the transport layer. Classifier routes as MUST-
level, unmet.

Node 4 — Scenario Instantiator: Concrete violation — RS decodes payload and uses
claims without verifying the signature segment. Any caller who can construct a
valid-looking JSON payload, Base64URL-encode it, and append any signature segment
can obtain arbitrary `sub` and `scope` values in the RS's authorization pipeline.
The HTTPS argument protects in-transit bytes, not the authenticity of the payload
structure.

Node 5 — Evidence Collector: Evidence of violation: (a) RS extracts claims from
JWT payload without calling a verify function; (b) JWKS is not fetched; (c) signature
segment is ignored; (d) authorization decisions made on unverified claims. Counter-
argument logged: "HTTPS protects against tampering" — rejected; HTTPS protects
transport, not token authenticity.

Node 6 — Gap Analyzer: The gap is fundamental: an unverified JWT is a plain JSON
payload with a cosmetic structure. The signature segment has no protective function
if it is not checked. Any forged JWT passes through the RS's authorization pipeline
unchallenged.

Node 7 — Verdict Engine: MUST violated. CWE-347 / ASVS V9.1.1 MUST: signature
verification mandatory before trusting claims. HTTPS-transport argument rejected.
Verdict: REPROCESS.

Node 8 — Arbiter: Confirms. HTTPS transport does not substitute for JWT signature
verification. RS grants arbitrary authorization to forged tokens. REPROCESS.

Node 9 — Formatter: REPROCESS. CWE-347 / ASVS V9.1.1 MUST: JWT signatures must
be cryptographically verified before any claim is used for authorization. HTTPS
transport protects bytes in transit — it does not validate that the payload was
issued and signed by the AS. An unverified JWT is an unsigned assertion; any caller
can forge one. REPROCESS.

## Pair[CB4-3.4-1306]/Reason

CWE-347 / ASVS V9.1.1 MUST: RS must verify JWT signature before using claims;
signature segment ignored; HTTPS-transport argument rejected → REPROCESS.

## Pair[CB4-3.4-1306]/Phrasing-B

Verdict REPROCESS. CWE-347 / ASVS V9.1.1 MUST: JWT signature verification is
mandatory before claim-based authorization. HTTPS is orthogonal — it does not
prevent payload forgery. Unverified JWT claims are unauthenticated input. REPROCESS.

---

## Pair[CB4-3.4-1307]/A

A client is registered at an authorization server with three redirect URIs:
`https://app.example.com/callback`, `https://app.example.com/callback-dev`, and
`https://app.example.com/callback-mobile`. The client's authorization request
includes `client_id` and all other required parameters, but omits `redirect_uri`.
The AS has no way to determine which of the three registered URIs the client
intends. The AS selects the first registered URI and completes the flow. The
team argues this is a developer-friendly default that reduces boilerplate.

## Pair[CB4-3.4-1307]/B

Node 1 — Domain Splitter: Authorization code flow; redirect_uri parameter;
multiple registered URIs; AS-side validation of redirect_uri selection. Routes to
RFC Resolver with tags: redirect_uri, multiple-registrations, RFC-6749,
authorization-request, parameter-omission.

Node 2 — RFC Resolver: Primary citation — RFC 6749 §3.1.2.3: if the client has
registered multiple redirect URIs, the client MUST include the redirect_uri
parameter in the authorization request; the AS must fail the request with
error=invalid_request if the parameter is absent and multiple URIs are registered.
Resolver notes: no default-selection provision in RFC 6749.

Node 3 — Obligation Classifier: RFC 6749 §3.1.2.3 → MUST include redirect_uri
when multiple registered; AS MUST fail the request if absent. Both obligations
are unmet: client omitted the parameter; AS did not fail the request. Classifier
routes as MUST-level (both sides), unmet.

Node 4 — Scenario Instantiator: Concrete violation — three registered URIs; client
omits redirect_uri; AS selects first URI; flow completes. The authorization code
is delivered to a URI chosen by the AS, not explicitly requested by the client.
This ambiguity can be exploited to redirect codes to unintended destinations.

Node 5 — Evidence Collector: Evidence of violation: (a) client has three registered
redirect URIs; (b) redirect_uri absent from authorization request; (c) AS did not
return error=invalid_request; (d) AS defaulted to first registered URI; (e) flow
completed with AS-chosen redirect target. Counter-argument logged: "developer-
friendly default" — rejected; defaulting substitutes AS judgment for an explicit
required specification.

Node 6 — Gap Analyzer: The default-selection behavior violates RFC 6749 §3.1.2.3
in two directions: (1) the client failed to include a required parameter; (2) the
AS failed to reject the request. The default-to-first approach introduces redirect
ambiguity that could be weaponized to deliver authorization codes to unintended URIs.

Node 7 — Verdict Engine: MUST violated. RFC 6749 §3.1.2.3 requires redirect_uri
from the client and requires the AS to reject when absent and multiple URIs are
registered. Neither side complied. Verdict: REPROCESS.

Node 8 — Arbiter: Confirms. Developer-convenience argument rejected. RFC 6749
§3.1.2.3 is unambiguous when multiple URIs are registered. REPROCESS.

Node 9 — Formatter: REPROCESS. RFC 6749 §3.1.2.3 MUST: when multiple redirect
URIs are registered, the client MUST include redirect_uri; the AS MUST reject the
request with error=invalid_request if the parameter is absent. Defaulting to the
first registered URI substitutes AS judgment for an explicitly required parameter
and introduces redirect ambiguity. REPROCESS.

## Pair[CB4-3.4-1307]/Reason

RFC 6749 §3.1.2.3 MUST: client must include redirect_uri when multiple registered;
AS must reject if absent; AS defaulted to first registered URI instead of rejecting
→ REPROCESS.

## Pair[CB4-3.4-1307]/Phrasing-B

Verdict REPROCESS. RFC 6749 §3.1.2.3: with multiple registered URIs, redirect_uri
is mandatory in the authorization request; the AS MUST reject if absent. Selecting
a default URI bypasses this MUST. REPROCESS.

---

## Pair[CB4-3.4-1308]/A

During dynamic client registration, a client attempts to register a redirect URI
of `https://app.example.com/callback#auth-section`. The URI contains a fragment
component (`#auth-section`). The AS's registration endpoint accepts the registration
without returning an error. The team argues that fragment components are never sent
to the server by browsers — they are client-side-only, making the registration
harmless.

## Pair[CB4-3.4-1308]/B

Node 1 — Domain Splitter: Redirect URI registration; fragment component in URI;
AS-side validation at registration time. Routes to RFC Resolver with tags:
redirect_uri, fragment-component, URI-validation, RFC-6749, dynamic-registration.

Node 2 — RFC Resolver: Primary citation — RFC 6749 §3.1.2 MUST NOT: endpoint URIs
included in redirect URIs MUST NOT include a fragment component. This prohibition
applies at the point of registration. Secondary citation — RFC 3986 §3.5: fragment
identifier is client-side and not transmitted to the server, but its presence in
a stored redirect URI creates processing ambiguity when the AS constructs the
authorization redirect URL.

Node 3 — Obligation Classifier: RFC 6749 §3.1.2 MUST NOT → unconditional prohibition.
The MUST NOT applies at registration time; the runtime-harmless argument does not
provide an exception. Classifier routes as MUST NOT level, unmet.

Node 4 — Scenario Instantiator: Concrete violation — redirect URI
`https://app.example.com/callback#auth-section` accepted at registration. When
the AS later constructs the redirect URL by appending `?code=ABC`, the resulting
URL is `https://app.example.com/callback#auth-section?code=ABC` — the query string
appears after the fragment delimiter, placing the code in the fragment-relative
portion where browser processing may silently drop or mishandle it.

Node 5 — Evidence Collector: Evidence of violation: (a) redirect URI contains
`#auth-section`; (b) AS accepted registration without error; (c) RFC 6749 §3.1.2
MUST NOT is unmet; (d) AS failed to return invalid_redirect_uri error. Counter-
argument logged: "fragments are client-side only" — rejected; MUST NOT applies
at registration, not conditioned on runtime behavior.

Node 6 — Gap Analyzer: The MUST NOT is registration-time. Accepting a fragment-
containing URI stores a malformed endpoint in the client registry. Runtime impact
(code placed after fragment delimiter) makes this worse than cosmetic; but even
if runtime impact were zero, the MUST NOT still applies unconditionally.

Node 7 — Verdict Engine: MUST NOT violated. RFC 6749 §3.1.2: registration of
fragment-containing redirect URI is prohibited. AS accepted without error.
Verdict: REPROCESS.

Node 8 — Arbiter: Confirms. Harmless-at-runtime argument does not override the
registration-time MUST NOT. REPROCESS.

Node 9 — Formatter: REPROCESS. RFC 6749 §3.1.2 MUST NOT: redirect URI endpoint
MUST NOT include a fragment component. The AS must reject registration of fragment-
containing URIs with invalid_redirect_uri. The runtime-harmless argument is rejected
— the MUST NOT is unconditional at registration time. REPROCESS.

## Pair[CB4-3.4-1308]/Reason

RFC 6749 §3.1.2 MUST NOT: redirect URI must not include fragment component; AS
accepted fragment-containing URI at registration; harmless-at-runtime argument
rejected → REPROCESS.

## Pair[CB4-3.4-1308]/Phrasing-B

Verdict REPROCESS. RFC 6749 §3.1.2 MUST NOT: fragment components are prohibited
in redirect URIs. Registration-time MUST NOT is unconditional — runtime behaviour
of fragments does not provide an exception. REPROCESS.

---

## Pair[CB4-3.4-1309]/A

An OIDC relying party has a local JWKS cache. An incoming ID token's JOSE header
contains `kid="2025-key-01"`, but the cached JWKS contains only `kid="2024-key-01"`.
The RP has no matching key. Rather than rejecting the token or re-fetching the
JWKS, the RP logs a warning and proceeds to accept the token without signature
verification — a "soft failure" mode designed to prevent service outages during
key rotation. The team argues that key rotation periods require graceful degradation.

## Pair[CB4-3.4-1309]/B

Node 1 — Domain Splitter: OIDC ID token signature verification; JWKS cache miss;
kid not found; soft-failure handling; service-continuity vs. security. Routes to
RFC Resolver with tags: JWKS, kid, signature-verification, CWE-347, OIDC,
soft-failure, key-rotation.

Node 2 — RFC Resolver: Primary citation — OIDC Core §3.1.3.7: the Client MUST
validate the signature of the ID token. Secondary citation — CWE-347: improper
verification of cryptographic signature when a signing key cannot be located.
Resolver notes: the correct behavior on kid-not-found is to attempt a JWKS
re-fetch before rejecting; accepting without verification is not an acceptable
fallback under any RFC provision.

Node 3 — Obligation Classifier: OIDC Core §3.1.3.7 MUST → signature verification
is unconditional. No exception exists for key-rotation periods, JWKS cache misses,
or service-continuity goals. Classifier routes as MUST-level, unmet.

Node 4 — Scenario Instantiator: Concrete violation — kid="2025-key-01" not in
cache; RP does not re-fetch JWKS; RP accepts token without signature check. An
attacker who observes that the RP has a soft-failure mode can present any token
with a novel kid and bypass all signature verification, including for forged
identities.

Node 5 — Evidence Collector: Evidence of violation: (a) kid in token not in cached
JWKS; (b) RP does not attempt JWKS re-fetch; (c) RP logs warning and proceeds;
(d) token accepted without signature verification; (e) OIDC Core §3.1.3.7 MUST
unmet. Counter-argument logged: "graceful degradation during key rotation" —
rejected; correct behavior is JWKS re-fetch then rejection if key still absent.

Node 6 — Gap Analyzer: The soft-failure path is a complete bypass of OIDC's
cryptographic authentication guarantee. The attacker's path: submit any token with
kid="novel-key"; RP misses cache; RP accepts without verifying. Service continuity
is a legitimate goal, but it must be achieved through JWKS re-fetch, not through
signature-check bypass.

Node 7 — Verdict Engine: MUST violated. OIDC Core §3.1.3.7 / CWE-347: ID token
signature must be verified; soft-failure accepts unsigned token on kid-not-found.
Service-continuity argument rejected. Verdict: REPROCESS.

Node 8 — Arbiter: Confirms. Soft-failure path eliminates authentication guarantee.
Correct remediation: re-fetch JWKS; if key still not found, reject token. REPROCESS.

Node 9 — Formatter: REPROCESS. OIDC Core §3.1.3.7 MUST / CWE-347: ID token
signature verification is unconditional. On kid-not-found, the RP must attempt
a JWKS cache refresh; if the key is still absent after re-fetch, the token must
be rejected. Accepting tokens without signature verification to maintain service
availability is a critical authentication bypass. REPROCESS.

## Pair[CB4-3.4-1309]/Reason

OIDC Core §3.1.3.7 / CWE-347 MUST: ID token signature must be verified; kid not
in cache; RP accepts without verification instead of re-fetching JWKS; service-
continuity argument rejected → REPROCESS.

## Pair[CB4-3.4-1309]/Phrasing-B

Verdict REPROCESS. OIDC Core §3.1.3.7 MUST: signature verification is mandatory.
Correct kid-not-found behavior: re-fetch JWKS; reject if still absent. Accepting
tokens without verification on kid-not-found is authentication bypass.
Service-continuity rationale rejected. REPROCESS.

---

## Pair[CB4-3.4-1310]/A

An OIDC relying party sends `claims_locales=fr-CA en` in its authorization requests.
The OIDC provider's authorization server processes the request but ignores the
`claims_locales` parameter. UserInfo responses always return English claim values
regardless of the locale preference. French-speaking users receive English strings
for localized display fields. The RP's engineering team formally asks the AS team
whether this constitutes a normative violation requiring immediate remediation.

## Pair[CB4-3.4-1310]/B

Node 1 — Domain Splitter: OIDC UserInfo claim localization; claims_locales parameter;
AS behavior when locale preference is supplied. Routes to RFC Resolver with tags:
claims_locales, OIDC, UserInfo, internationalization, locale-preference.

Node 2 — RFC Resolver: Primary citation — OIDC Core §5.5: when claims_locales is
received, the AS SHOULD use preferred languages for returned claim values. SHOULD
is a guidance-level recommendation, not an unconditional MUST. Resolver notes: no
RFC or OIDC Core provision upgrades the locale preference obligation to MUST.

Node 3 — Obligation Classifier: OIDC Core §5.5 → SHOULD (guidance). The AS's
failure to honor claims_locales is a guidance gap, not a normative MUST violation.
Classifier routes as guidance-level, partially unmet.

Node 4 — Scenario Instantiator: Concrete gap — claims_locales=fr-CA en supplied;
AS ignores parameter; all claim values returned in English. Impact: RP cannot
provide localized user experience for French-speaking users. Normative level: SHOULD.

Node 5 — Evidence Collector: Evidence of gap: (a) claims_locales parameter present
in authorization request; (b) AS does not implement locale selection logic; (c)
UserInfo returns English regardless. SHOULD guidance not fully met. No MUST is
violated.

Node 6 — Gap Analyzer: The gap is real and affects international deployments. OIDC
Core §5.5 SHOULD reflects the expectation that capable servers support locale
preferences. The AS team's assertion that localization is optional infrastructure
is technically correct at the normative level.

Node 7 — Verdict Engine: No MUST violated. OIDC Core §5.5 SHOULD; guidance-level
gap. Verdict: CONCERN.

Node 8 — Arbiter: Confirms. AS team's position is normatively correct — SHOULD
does not mandate implementation. Localization gap is real but does not escalate to
REPROCESS. CONCERN not REPROCESS.

Node 9 — Formatter: CONCERN. OIDC Core §5.5 SHOULD: AS should use preferred
languages when claims_locales is provided. No unconditional MUST mandates locale
support. Ignoring claims_locales is a guidance-level gap — it degrades localized
user experience but does not violate a normative MUST. CONCERN not REPROCESS.

## Pair[CB4-3.4-1310]/Reason

OIDC Core §5.5 SHOULD use preferred locale; no unconditional MUST; AS ignoring
claims_locales is guidance-level gap → CONCERN.

## Pair[CB4-3.4-1310]/Phrasing-B

Verdict CONCERN. OIDC Core §5.5 SHOULD: locale preference support is recommended,
not mandated. Ignoring claims_locales is a guidance gap with real UX impact. No
MUST is violated. CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1311]/A

A resource server returns `WWW-Authenticate: Bearer error="invalid_token",
error_description="Token signature verification failed."` for requests with
invalid tokens. The challenge does not include a `realm` parameter. A client
developer reviewing the RS documentation flags the missing `realm` attribute and
asks whether this omission constitutes a specification violation.

## Pair[CB4-3.4-1311]/B

Node 1 — Domain Splitter: Bearer token challenge format; WWW-Authenticate header;
realm attribute presence; RFC 6750 compliance at the resource server. Routes to
RFC Resolver with tags: WWW-Authenticate, Bearer, realm, RFC-6750, error-response.

Node 2 — RFC Resolver: Primary citation — RFC 6750 §3.1: resource servers SHOULD
include a realm attribute in the WWW-Authenticate header for Bearer token challenges.
SHOULD is a guidance-level recommendation. Resolver notes: `error` and
`error_description` are sufficient for client processing; `realm` provides
additional context about the protection space but is not unconditionally required.

Node 3 — Obligation Classifier: RFC 6750 §3.1 → SHOULD (guidance). No unconditional
MUST mandates the realm attribute. Classifier routes as guidance-level, partially
unmet.

Node 4 — Scenario Instantiator: Concrete gap — WWW-Authenticate contains
error="invalid_token" and error_description but no realm. The challenge is
syntactically valid and processable by OAuth clients. The realm omission means
clients cannot identify the protection space by name. No client behavior is blocked.

Node 5 — Evidence Collector: Evidence of gap: (a) WWW-Authenticate header present;
(b) error and error_description present; (c) realm absent; (d) RFC 6750 §3.1 SHOULD
not fully met. No MUST violated.

Node 6 — Gap Analyzer: The gap is minor. OAuth clients can process the challenge
correctly without realm. Realm is informational for humans and automated tooling
that needs to identify the protection space. SHOULD gap does not affect spec
conformance at the MUST level.

Node 7 — Verdict Engine: No MUST violated. RFC 6750 §3.1 SHOULD; guidance-level
gap. Verdict: CONCERN.

Node 8 — Arbiter: Confirms. Missing realm is a guidance-level omission. Bearer
challenges remain parseable. CONCERN not REPROCESS.

Node 9 — Formatter: CONCERN. RFC 6750 §3.1 SHOULD: the realm attribute is recommended
in Bearer token challenges. No unconditional MUST mandates its presence. A Bearer
challenge without realm is still parseable and processable by conformant OAuth
clients. The absence is a guidance-level gap. CONCERN not REPROCESS.

## Pair[CB4-3.4-1311]/Reason

RFC 6750 §3.1 SHOULD: realm recommended in WWW-Authenticate Bearer challenge; no
unconditional MUST; absent realm is guidance-level gap → CONCERN.

## Pair[CB4-3.4-1311]/Phrasing-B

Verdict CONCERN. RFC 6750 §3.1 SHOULD: realm is a recommended attribute in Bearer
challenges. Its absence is a guidance gap — not a MUST violation. Challenge remains
processable. CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1312]/A

An OIDC authorization server's authorization endpoint accepts requests from any
source without applying rate limiting, IP-based throttling, or anomaly detection.
An attacker submits 5,000 authorization requests per minute for a public client,
generating 5,000 unused authorization codes per minute that expire in 60 seconds.
The flood consumes database write capacity and logging resources. The team argues
that because authorization requests are unauthenticated by nature, rate limiting
would require distinguishing legitimate automation from attack traffic, which is
operationally complex.

## Pair[CB4-3.4-1312]/B

Node 1 — Domain Splitter: Authorization endpoint protection; rate limiting; resource
exhaustion from authorization code flood; defense-in-depth. Routes to RFC Resolver
with tags: authorization-endpoint, rate-limiting, ASVS, anomaly-detection,
resource-exhaustion.

Node 2 — RFC Resolver: Primary citation — ASVS V6.4: guidance recommends rate
limiting on authentication and authorization initiation endpoints to prevent abuse.
No RFC contains an unconditional MUST for rate limiting the authorization endpoint.
Resolver notes: RFC 6749 does not address rate limiting; ASVS V6.4 is guidance-
level (SHOULD).

Node 3 — Obligation Classifier: ASVS V6.4 → SHOULD (guidance). No unconditional
RFC MUST mandates rate limiting on the authorization endpoint. The team's complexity
argument is partially valid as an implementation trade-off. Classifier routes as
guidance-level, unmet.

Node 4 — Scenario Instantiator: Concrete gap — 5,000 requests per minute; 5,000
authorization codes per minute created and written to the database; 60-second
expiry; database write capacity consumed; logging volume elevated. No MUST is
violated, but operational harm is measurable.

Node 5 — Evidence Collector: Evidence of gap: (a) no rate limiting on authorization
endpoint; (b) flood generates thousands of DB writes per minute; (c) no anomaly
detection triggers; (d) ASVS V6.4 SHOULD not met. No RFC MUST violated.

Node 6 — Gap Analyzer: The gap is real and operationally significant. Unbounded
authorization request flooding is a known denial-of-service vector for OAuth
deployments. The team's implementation-complexity argument is a valid trade-off
framing; however, the guidance gap remains. No MUST is violated.

Node 7 — Verdict Engine: No MUST violated. ASVS V6.4 guidance-level SHOULD;
resource exhaustion risk present. Verdict: CONCERN.

Node 8 — Arbiter: Confirms. No RFC MUST mandates authorization endpoint rate
limiting. Guidance gap is real but does not escalate to REPROCESS. CONCERN not
REPROCESS.

Node 9 — Formatter: CONCERN. No RFC contains an unconditional MUST for authorization
endpoint rate limiting. ASVS V6.4 and general defense-in-depth guidance SHOULD
rate-limit authorization initiation endpoints. Unbounded request flooding creates
measurable operational risk — database saturation, code-table bloat, logging
overload. The complexity argument is accepted as a trade-off framing. No MUST
violated. CONCERN not REPROCESS.

## Pair[CB4-3.4-1312]/Reason

No RFC MUST for authorization endpoint rate limiting; ASVS V6.4 SHOULD guidance;
unbounded flood creates operational risk; no MUST → CONCERN.

## Pair[CB4-3.4-1312]/Phrasing-B

Verdict CONCERN. No unconditional RFC MUST mandates rate limiting on the
authorization endpoint. ASVS V6.4 guidance SHOULD. Authorization code flooding
creates operational risk at guidance level. CONCERN not REPROCESS.
