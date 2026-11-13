# Node Pipeline Chain Set Extension 46
# Sub-phase: 3.4 Security and Authorization Model
# Pair ID prefix: CB4-3.4-
# Sequences: 1357–1364 (16 instances — 8 base pairs × 2 phrasings)
# Sources: OIDC Back-Channel Logout §2.4, RFC 9126 §2.3,
#          OIDC Core §2, RFC 9700 §4.3.2,
#          RFC 6749 §5.1 / OIDC Core §5.3,
#          OIDC Core §5.3 / CWE-942, RFC 9126 §2.2,
#          OIDC RP-Initiated Logout §3
# Running total after this file: 2728 instances (1364 base pairs)
# Formula check: 120 + (1364 − 60) × 2 = 120 + 2608 = 2728 ✓

---

## Pair[CB4-3.4-1357]/A

An OIDC provider sends back-channel logout tokens that contain `iss`, `sub`,
`iat`, `jti`, and `aud` but no `events` claim. The receiving RP's logout handler
processes the token and terminates the session. An auditor reviews the token
structure and asks the pipeline to assess whether the logout token is correctly
formed.

## Pair[CB4-3.4-1357]/B

Node 1 — Domain Splitter: OIDC Back-Channel Logout; logout token structure;
events claim requirement; token type identification. Routes to RFC Resolver with
tags: back-channel-logout, events-claim, logout-token, OIDC, token-type.

Node 2 — RFC Resolver: Primary citation — OIDC Back-Channel Logout §2.4 MUST:
logout tokens must contain an `events` claim whose member name is
`http://schemas.openid.net/event/backchannel-logout` with an empty JSON object
value. Resolver notes: the events claim is the normative marker distinguishing
logout tokens from other OIDC JWTs; its absence makes the token structurally non-
conformant regardless of delivery channel.

Node 3 — Obligation Classifier: OIDC Back-Channel Logout §2.4 → MUST include
events claim. Logout token lacks events claim. MUST unmet. Classifier routes as
MUST-level, unmet.

Node 4 — Scenario Instantiator: Concrete violation — logout token delivered to
back-channel endpoint; token contains standard JWT claims but no events claim;
AS team argues delivery channel makes events redundant. The MUST is structural —
it applies to the token body irrespective of delivery channel.

Node 5 — Evidence Collector: Evidence of violation: (a) logout token payload has
no events claim; (b) OIDC Back-Channel Logout §2.4 MUST not met; (c) AS argues
delivery channel provides context. Counter-argument logged: "delivery context
makes events redundant" — rejected; MUST applies to token structure.

Node 6 — Gap Analyzer: Without the events claim, any OIDC JWT can be replayed
against a back-channel logout endpoint — an ID token, an access token, or a
previously issued logout token from another session. The structural marker is
the defense against this confusion.

Node 7 — Verdict Engine: MUST violated. OIDC Back-Channel Logout §2.4: events
claim mandatory. Verdict: REPROCESS.

Node 8 — Arbiter: Confirms. Delivery-channel argument rejected. Token structure
MUST applies regardless of transport. REPROCESS.

Node 9 — Formatter: REPROCESS. OIDC Back-Channel Logout §2.4 MUST: logout tokens
must include the `events` claim with the backchannel-logout URI as a member.
The events claim is the required structural marker that identifies the token as
a logout token — without it, token type identification relies solely on delivery
context, enabling token confusion attacks. REPROCESS.

## Pair[CB4-3.4-1357]/Reason

OIDC Back-Channel Logout §2.4 MUST: events claim required in logout token; absent;
delivery-context argument rejected → REPROCESS.

## Pair[CB4-3.4-1357]/Phrasing-B

Verdict REPROCESS. OIDC Back-Channel Logout §2.4 MUST: the events claim is a
required structural element. Its absence makes the logout token non-conformant
regardless of delivery channel. REPROCESS.

---

## Pair[CB4-3.4-1358]/A

Client A pushes an authorization request via PAR and receives
`request_uri=urn:ietf:params:oauth:request_uri:xyz789`. Client B intercepts the
request_uri value and submits an authorization request to the AS authorization
endpoint using Client B's own `client_id` and the captured `request_uri`. The
AS locates the PAR record and, without checking the client_id binding, initiates
authorization for Client B using Client A's pushed parameters (scope, redirect_uri,
state). Assess the AS's behavior.

## Pair[CB4-3.4-1358]/B

Node 1 — Domain Splitter: PAR request_uri; client binding at authorization
endpoint; client substitution attack via hijacked request_uri. Routes to RFC
Resolver with tags: PAR, request_uri, client-binding, RFC-9126, client-id-mismatch.

Node 2 — RFC Resolver: Primary citation — RFC 9126 §2.3 MUST: the authorization
endpoint MUST reject the request if the `client_id` used in the authorization
request does not match the `client_id` of the client that submitted the PAR
request. Resolver notes: the request_uri is a client-bound token; sharing it
across clients violates the PAR security model.

Node 3 — Obligation Classifier: RFC 9126 §2.3 → MUST reject on client_id mismatch.
Client B used Client A's request_uri with Client B's client_id; AS did not check
binding; authorization proceeded. MUST unmet. Classifier routes as MUST-level,
unmet.

Node 4 — Scenario Instantiator: Concrete violation — Client B uses Client A's
request_uri; AS ignores binding check; Client B's authorization flow executes with
Client A's pushed parameters including scope and redirect_uri. Attack: Client B
can obtain authorization for scope it didn't register or redirect to a URI it
doesn't own.

Node 5 — Evidence Collector: Evidence of violation: (a) request_uri issued to
Client A; (b) Client B's client_id in authorization request; (c) AS did not verify
client_id binding; (d) RFC 9126 §2.3 MUST not met. No counter-argument logged by
team.

Node 6 — Gap Analyzer: The client binding check is the critical security control
in PAR — it ensures that the pushed parameters are only used by the client that
authorized them. Without it, any intercepted request_uri can be weaponized by
any registered client.

Node 7 — Verdict Engine: MUST violated. RFC 9126 §2.3: client_id binding at
authorization endpoint mandatory. Verdict: REPROCESS.

Node 8 — Arbiter: Confirms. Client substitution via request_uri is the attack
the binding check prevents. REPROCESS.

Node 9 — Formatter: REPROCESS. RFC 9126 §2.3 MUST: the authorization endpoint
must verify that the `client_id` in the authorization request matches the client
that submitted the PAR request. Accepting Client B's use of Client A's
request_uri enables client substitution — gaining authorization under another
client's parameters. REPROCESS.

## Pair[CB4-3.4-1358]/Reason

RFC 9126 §2.3 MUST: request_uri must be bound to pushing client; different
client_id used at authorization endpoint; binding check absent → REPROCESS.

## Pair[CB4-3.4-1358]/Phrasing-B

Verdict REPROCESS. RFC 9126 §2.3 MUST: the AS must verify client_id binding when
a request_uri is used at the authorization endpoint. Client substitution via a
captured request_uri is the attack this check prevents. REPROCESS.

---

## Pair[CB4-3.4-1359]/A

An OIDC authorization server issues an ID token for a relying party. The token
has `aud=["rp-client","analytics-service"]` and `azp="analytics-service"`. The
RP validates the ID token: signature passes, iss matches, aud contains RP's
client_id, exp is valid. However, when the RP checks that `azp` equals its own
`client_id`, the check fails because `azp="analytics-service"` ≠ `"rp-client"`.
The RP rejects the token. Assess whether the RP's rejection is correct.

## Pair[CB4-3.4-1359]/B

Node 1 — Domain Splitter: OIDC ID token azp claim validation; authorized party
identification; RP-side validation obligation. Routes to RFC Resolver with tags:
azp, authorized-party, client_id, OIDC-Core, ID-token-validation.

Node 2 — RFC Resolver: Primary citation — OIDC Core §2 MUST: if the `azp` claim
is present, it MUST contain the OAuth 2.0 Client ID of the party to whom the ID
token was issued (the requesting client). The AS set azp to `analytics-service`
when the token was issued to `rp-client`. This violates the MUST at issuance.
OIDC Core §3.1.3.7.4: if the RP's `client_id` is not the sole aud value, the RP
SHOULD verify that its client_id is the value of the azp claim. The RP's rejection
is the correct response.

Node 3 — Obligation Classifier: OIDC Core §2 → MUST when azp present: azp must
equal the requesting client_id. The AS issued a token where azp does not equal
rp-client. MUST violated at issuance. RP's check is SHOULD (§3.1.3.7.4) but the
underlying AS MUST is violated. Classifier routes as MUST-level violation at AS.

Node 4 — Scenario Instantiator: Concrete violation — azp="analytics-service" in
token issued to "rp-client"; RP validates azp against own client_id; check fails;
RP rejects token correctly. The AS's incorrect azp assignment is the source of
the violation.

Node 5 — Evidence Collector: Evidence of violation: (a) azp="analytics-service"
in token for rp-client; (b) OIDC Core §2 MUST: azp must equal requesting client_id;
(c) RP correctly rejects based on azp mismatch; (d) AS's "co-authorized party"
reasoning is not defined in OIDC Core. RP's behavior confirmed correct.

Node 6 — Gap Analyzer: The AS incorrectly set azp. The RP's validation is sound —
OIDC Core §3.1.3.7.4 recommends exactly this check when multi-audience tokens
are used. The fix is at the AS: azp should be set to rp-client, not analytics-
service.

Node 7 — Verdict Engine: MUST violated at AS. OIDC Core §2: azp must equal
requesting client_id when present. Verdict: REPROCESS (AS-side violation; RP
behavior is correct).

Node 8 — Arbiter: Confirms. RP's rejection is normatively correct. AS's co-
authorized framing has no basis in OIDC Core §2. REPROCESS.

Node 9 — Formatter: REPROCESS. OIDC Core §2 MUST: when azp is present, it must
equal the client_id of the party to whom the ID token was issued. The AS set
azp="analytics-service" on a token issued to "rp-client" — a MUST violation.
The RP's rejection of the token on azp mismatch is the correct behavior. REPROCESS.

## Pair[CB4-3.4-1359]/Reason

OIDC Core §2 MUST: azp when present must equal requesting client_id; AS set azp
to analytics-service for token issued to rp-client; RP correctly rejected →
REPROCESS.

## Pair[CB4-3.4-1359]/Phrasing-B

Verdict REPROCESS. OIDC Core §2 MUST: azp must equal the requesting client_id.
AS set azp to a third-party value — a MUST violation. RP rejection was correct.
REPROCESS.

---

## Pair[CB4-3.4-1360]/A

A web application authorization server's authorization endpoint accepts
`response_type=token` in requests from registered web application clients
(HTTPS redirect URIs). The AS issues access tokens in the fragment component of
the redirect URI, implementing the OAuth 2.0 implicit grant. The team argues that
RFC 6749 explicitly defines the implicit grant and that they must remain backward-
compatible with integration partners who have not migrated to the authorization
code flow.

## Pair[CB4-3.4-1360]/B

Node 1 — Domain Splitter: OAuth 2.0 implicit grant; response_type=token;
RFC 9700 prohibition; web application clients; backward compatibility. Routes
to RFC Resolver with tags: implicit-flow, response_type-token, RFC-9700,
RFC-6749, MUST-NOT, access-token-in-fragment.

Node 2 — RFC Resolver: Primary citation — RFC 9700 §4.3.2 MUST NOT: authorization
servers MUST NOT issue access tokens via the implicit grant flow
(response_type=token) in the authorization response. RFC 9700 is the OAuth 2.0
Security Best Current Practice and explicitly supersedes the permissions granted
in RFC 6749 regarding the implicit flow. Resolver notes: RFC 6749 defines the
implicit flow but RFC 9700 establishes a security-based prohibition on it.

Node 3 — Obligation Classifier: RFC 9700 §4.3.2 → MUST NOT issue AT via implicit
grant. AS continues to issue access tokens via implicit flow. MUST NOT violated.
Classifier routes as MUST NOT level, unmet.

Node 4 — Scenario Instantiator: Concrete violation — AS accepts response_type=token
from web clients; issues AT in redirect fragment; RFC 9700 §4.3.2 MUST NOT unmet.
The backward-compatibility argument does not override a MUST NOT.

Node 5 — Evidence Collector: Evidence of violation: (a) response_type=token
accepted; (b) AT returned in authorization response fragment; (c) RFC 9700 §4.3.2
MUST NOT not met. Counter-argument logged: "RFC 6749 permits; backward-compatible"
— rejected; RFC 9700 supersedes RFC 6749 on this point.

Node 6 — Gap Analyzer: The implicit flow's core problem is access token exposure
in URL fragments: browser history, referer headers, and JavaScript on the page
can read fragment components. RFC 9700 §4.3.2 exists specifically to eliminate
this attack surface from conformant deployments.

Node 7 — Verdict Engine: MUST NOT violated. RFC 9700 §4.3.2: implicit AT issuance
prohibited. Backward-compatibility argument rejected. Verdict: REPROCESS.

Node 8 — Arbiter: Confirms. RFC 9700 supersedes RFC 6749 on implicit flow.
REPROCESS.

Node 9 — Formatter: REPROCESS. RFC 9700 §4.3.2 MUST NOT: authorization servers
must not issue access tokens via the implicit grant. RFC 9700 is the current
security BCP (Best Current Practice) and overrides RFC 6749's implicit flow
definition. Backward-compatibility arguments do not override a MUST NOT. REPROCESS.

## Pair[CB4-3.4-1360]/Reason

RFC 9700 §4.3.2 MUST NOT: implicit grant (response_type=token) must not be used
for access token issuance; AS accepts response_type=token; backward-compatibility
argument rejected → REPROCESS.

## Pair[CB4-3.4-1360]/Phrasing-B

Verdict REPROCESS. RFC 9700 §4.3.2 MUST NOT: implicit access token issuance is
prohibited. RFC 9700 supersedes RFC 6749. No backward-compatibility exception
applies to a MUST NOT. REPROCESS.

---

## Pair[CB4-3.4-1361]/A

An authorization server issues refresh tokens in authorization code flows. Token
responses include `access_token`, `token_type`, `expires_in`, `refresh_token`,
and `scope`. A client developer notes that `refresh_token_expires_in` is absent
and files a support ticket asking whether clients can determine when refresh tokens
will expire. The AS team responds that there is no standard field for this and that
clients must attempt refresh token use to discover expiry.

## Pair[CB4-3.4-1361]/B

Node 1 — Domain Splitter: Token response content; refresh token lifetime
disclosure; RFC 6749 §5.1 optional fields; client awareness of refresh token expiry.
Routes to RFC Resolver with tags: token-response, refresh_token, expires, RFC-6749,
optional-fields.

Node 2 — RFC Resolver: Primary citation — RFC 6749 §5.1 specifies the fields in
a successful token response: `access_token` (REQUIRED), `token_type` (REQUIRED),
`expires_in` (RECOMMENDED), `refresh_token` (OPTIONAL), `scope` (conditional).
`refresh_token_expires_in` is not listed. No other RFC establishes an unconditional
MUST for this field. Resolver notes: some implementations provide it as a non-
standard extension.

Node 3 — Obligation Classifier: No MUST for refresh_token_expires_in. Gap is
real but guidance-level. RFC 6749 §5.1 does not include refresh_token_expires_in
as a required or recommended field. Classifier routes as guidance-level, absent.

Node 4 — Scenario Instantiator: Guidance gap — clients cannot proactively know
refresh token lifetime without attempting use; UX degradation; no RFC MUST. AS
team's statement that there is no standard field is normatively accurate.

Node 5 — Evidence Collector: Evidence of gap: (a) token response lacks
refresh_token_expires_in; (b) RFC 6749 §5.1 does not require it; (c) no other
RFC establishes a MUST for this field. AS team's response is normatively correct.

Node 6 — Gap Analyzer: Absent refresh token expiry disclosure causes clients to
receive unexpected expiry errors in production. It is a usability and integration
gap. Some implementations expose it as a vendor extension. No standard mandates it.

Node 7 — Verdict Engine: No MUST violated. RFC 6749 §5.1 does not require this
field. Verdict: CONCERN.

Node 8 — Arbiter: Confirms. AS team's response is normatively correct. CONCERN
not REPROCESS.

Node 9 — Formatter: CONCERN. RFC 6749 §5.1 does not require a
`refresh_token_expires_in` field in token responses. No RFC establishes an
unconditional MUST for this field. Its absence is a usability gap. CONCERN not
REPROCESS.

## Pair[CB4-3.4-1361]/Reason

No RFC MUST for refresh_token_expires_in; RFC 6749 §5.1 does not include it as
required; absence is guidance-level usability gap → CONCERN.

## Pair[CB4-3.4-1361]/Phrasing-B

Verdict CONCERN. RFC 6749 §5.1 does not mandate refresh_token_expires_in. No
standard establishes this as a MUST field. Its absence is a usability gap, not a
specification violation. CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1362]/A

An OIDC provider's UserInfo endpoint does not include any CORS headers in HTTP
responses. Browser-based applications (SPAs) attempting to call the UserInfo
endpoint directly from JavaScript fail due to browser same-origin policy
enforcement. The AS team argues that SPAs should route UserInfo calls through a
server-side backend rather than calling the endpoint from the browser directly.

## Pair[CB4-3.4-1362]/B

Node 1 — Domain Splitter: UserInfo endpoint CORS support; browser-based client
access; OIDC Core §5.3; architectural alternatives. Routes to RFC Resolver with
tags: UserInfo, CORS, OIDC-Core, SPA, browser-access, BFF-pattern.

Node 2 — RFC Resolver: Primary citation — OIDC Core §5.3 and associated guidance:
UserInfo endpoints SHOULD support CORS to enable direct browser-based access.
No unconditional MUST in OIDC Core or applicable RFC requires CORS support at
the UserInfo endpoint. Resolver notes: the backend-for-frontend (BFF) proxy pattern
is an architecturally valid alternative to direct browser access.

Node 3 — Obligation Classifier: OIDC Core §5.3 guidance → SHOULD support CORS.
No unconditional MUST. Classifier routes as guidance-level, unmet.

Node 4 — Scenario Instantiator: Guidance gap — SPAs cannot call UserInfo directly;
AS team offers BFF as alternative. No RFC MUST violated. The BFF pattern is a
valid architectural solution.

Node 5 — Evidence Collector: Evidence of gap: (a) UserInfo endpoint returns no
CORS headers; (b) SPAs receive browser CORS error; (c) OIDC Core §5.3 SHOULD
not met; (d) no RFC MUST for CORS at UserInfo. AS team's BFF argument is
architecturally valid.

Node 6 — Gap Analyzer: The gap affects direct browser access. CORS absence is
not a MUST violation; it is a guidance-level deployment recommendation. BFF proxy
is a widely accepted alternative that doesn't require CORS.

Node 7 — Verdict Engine: No MUST violated. OIDC Core §5.3 guidance SHOULD;
BFF pattern accepted as valid alternative. Verdict: CONCERN.

Node 8 — Arbiter: Confirms. No MUST for CORS at UserInfo. BFF architectural
argument is valid. CONCERN not REPROCESS.

Node 9 — Formatter: CONCERN. OIDC Core §5.3 guidance SHOULD: UserInfo endpoints
should support CORS for browser clients. No unconditional MUST requires CORS
support. The backend-for-frontend proxy pattern is a valid architectural alternative
to direct browser access. Guidance-level gap. CONCERN not REPROCESS.

## Pair[CB4-3.4-1362]/Reason

OIDC Core §5.3 guidance SHOULD: UserInfo endpoint should support CORS; no
unconditional MUST; BFF proxy is valid alternative → CONCERN.

## Pair[CB4-3.4-1362]/Phrasing-B

Verdict CONCERN. OIDC Core §5.3 SHOULD: CORS support for UserInfo is recommended
but not mandated. BFF proxy is a valid architectural alternative. No MUST violated.
CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1363]/A

An authorization server accepts PAR requests and returns `request_uri` values
with `expires_in=3600` — a one-hour lifetime. A security architect flags the
long expiry and asks whether it violates RFC 9126. The AS team responds that some
user flows take up to 45 minutes and that a one-hour expiry provides margin.

## Pair[CB4-3.4-1363]/B

Node 1 — Domain Splitter: PAR request_uri expiry; RFC 9126 §2.2 recommendation;
expiry duration; replay window. Routes to RFC Resolver with tags: PAR, request_uri,
expiry, expires_in, RFC-9126, SHOULD-short.

Node 2 — RFC Resolver: Primary citation — RFC 9126 §2.2: the authorization server
SHOULD keep the validity period of the `request_uri` short. No RFC provides a
numeric MUST ceiling for PAR request_uri expiry. The SHOULD is qualitative.
Resolver notes: the trade-off between security (short window) and usability (user
flow duration) is an implementation decision without a normative numeric limit.

Node 3 — Obligation Classifier: RFC 9126 §2.2 → SHOULD short. No MUST ceiling.
3600-second expiry is long relative to the recommendation. Classifier routes as
guidance-level, unmet.

Node 4 — Scenario Instantiator: Guidance gap — 3600-second PAR expiry; AS team's
usability argument acknowledged as valid operational consideration; replay window
for captured request_uri is one hour. No RFC MUST ceiling violated.

Node 5 — Evidence Collector: Evidence of gap: (a) expires_in=3600 in PAR response;
(b) RFC 9126 §2.2 SHOULD short not fully met; (c) no numeric MUST maximum exists;
(d) AS team's 45-minute user flow justification is an operational trade-off.

Node 6 — Gap Analyzer: A one-hour request_uri creates a wider window for replay
of a captured reference. Best practice suggests 60–120 second lifetimes. However,
without a MUST ceiling, the gap is guidance-level.

Node 7 — Verdict Engine: No MUST violated. RFC 9126 §2.2 SHOULD short; guidance-
level gap. Verdict: CONCERN.

Node 8 — Arbiter: Confirms. No numeric MUST for PAR expiry ceiling. CONCERN not
REPROCESS.

Node 9 — Formatter: CONCERN. RFC 9126 §2.2 SHOULD: PAR request_uri expiry should
be short. No unconditional RFC MUST specifies a maximum duration. A 3600-second
expiry is a guidance-level gap that widens the replay window. CONCERN not REPROCESS.

## Pair[CB4-3.4-1363]/Reason

RFC 9126 §2.2 SHOULD short request_uri expiry; no MUST ceiling; 3600-second expiry
is guidance-level gap → CONCERN.

## Pair[CB4-3.4-1363]/Phrasing-B

Verdict CONCERN. RFC 9126 §2.2 SHOULD short: no numeric MUST ceiling for PAR
expiry. 3600-second expiry creates a wider replay window — guidance gap, not a
MUST violation. CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1364]/A

An OIDC provider supports RP-Initiated Logout. Clients send logout requests to
the `end_session_endpoint` with an `id_token_hint` parameter. The AS validates
the hint's signature and checks that `iss` matches its own issuer, but does not
check whether the hint was issued for the currently authenticated session. A
logout request containing an ID token from a previous (already terminated) session
is accepted and the current session is terminated. The AS team argues that any
valid token from the same issuer is sufficient context for logout.

## Pair[CB4-3.4-1364]/B

Node 1 — Domain Splitter: RP-Initiated Logout; id_token_hint session binding;
OIDC RP-Initiated Logout §3; cross-session logout risk. Routes to RFC Resolver
with tags: end_session_endpoint, id_token_hint, session-binding, OIDC, logout-CSRF.

Node 2 — RFC Resolver: Primary citation — OIDC RP-Initiated Logout §3 SHOULD:
authorization servers should use the id_token_hint to identify which session to
terminate; the AS should verify the hint matches the current session context.
The obligation is SHOULD-level. Resolver notes: strict session binding would
prevent cross-session logout abuse; SHOULD not MUST.

Node 3 — Obligation Classifier: OIDC RP-Initiated Logout §3 → SHOULD session
binding check. No MUST. Classifier routes as guidance-level, unmet.

Node 4 — Scenario Instantiator: Guidance gap — previous-session hint accepted;
current session terminated; AS validates issuer but not session binding. Risk:
an attacker with a stale ID token can terminate active sessions. SHOULD-level gap.

Node 5 — Evidence Collector: Evidence of gap: (a) id_token_hint contains old
session token; (b) AS validates signature and iss but not session binding; (c)
current session terminated; (d) OIDC RP-Initiated Logout §3 SHOULD not met. No
MUST violated.

Node 6 — Gap Analyzer: The session-binding gap enables logout CSRF with stale
tokens. Best practice is to check that the hint corresponds to the active session.
However, since the obligation is SHOULD-level, the gap does not escalate to REPROCESS.

Node 7 — Verdict Engine: No MUST violated. OIDC RP-Initiated Logout §3 SHOULD;
guidance-level gap. Verdict: CONCERN.

Node 8 — Arbiter: Confirms. SHOULD-level gap. Stale-token-same-issuer argument
is rejected as best practice but no MUST is violated. CONCERN not REPROCESS.

Node 9 — Formatter: CONCERN. OIDC RP-Initiated Logout §3 SHOULD: the AS should
verify that the id_token_hint corresponds to the currently active session. No
unconditional MUST mandates strict session binding. Accepting a stale hint creates
a logout CSRF risk at guidance level. CONCERN not REPROCESS.

## Pair[CB4-3.4-1364]/Reason

OIDC RP-Initiated Logout §3 SHOULD: AS should validate id_token_hint matches
current session; no unconditional MUST; unvalidated hint accepted; guidance-level
gap → CONCERN.

## Pair[CB4-3.4-1364]/Phrasing-B

Verdict CONCERN. OIDC RP-Initiated Logout §3 SHOULD: session binding for
id_token_hint is recommended but not mandated. Accepting stale hints creates
a logout CSRF risk at guidance level. No MUST violated. CONCERN not REPROCESS.
