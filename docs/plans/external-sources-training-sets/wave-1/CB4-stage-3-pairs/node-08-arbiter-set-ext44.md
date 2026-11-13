# Node 08 — Arbiter Set Extension 44
# Sub-phase: 3.4 Security and Authorization Model
# Pair ID prefix: CB4-3.4-
# Sequences: 1141–1148 (16 instances — 8 base pairs × 2 phrasings)
# Sources: OIDC Core §3.1.3.7.11 / §3.1.3.7.1, RFC 6749 §3.2.1,
#          RFC 9700 §4.14.1, OIDC Discovery §3, OIDC RP-Initiated Logout §4,
#          ASVS V8.2.2 / V9.3.1, RFC 9068 §2.2 / §2.2.3.1
# Running total after this file: 2296 instances (1148 base pairs)
# Formula check: 120 + (1148 − 60) × 2 = 120 + 2176 = 2296 ✓

---

## Pair[CB4-3.4-1141]/A

Client sends an authorization request including `max_age=300` — requiring that
the authentication event be no older than 300 seconds. The authorization server
processes the request, re-authenticates the user as required by the `max_age`
parameter, and returns an ID token in the subsequent token endpoint response.
The issued ID token does not contain an `auth_time` claim. The AS team argues
that `auth_time` is an optional claim and that the presence of `max_age` does
not mandate its inclusion.

## Pair[CB4-3.4-1141]/B

REPROCESS. OIDC Core §3.1.3.7.11 MUST: when a `max_age` request is made (or
when `auth_time` is requested as an Essential Claim), the ID token MUST contain
an `auth_time` claim. The `auth_time` claim is the mechanism by which the RP
verifies that the authentication event satisfies the `max_age` constraint. An
ID token without `auth_time` when `max_age` was requested makes RP-side
validation impossible and silently defeats the security guarantee `max_age`
is designed to enforce. The MUST is unconditional once `max_age` is in the
request.

## Pair[CB4-3.4-1141]/Reason

OIDC Core §3.1.3.7.11 MUST: `auth_time` MUST be in ID token when `max_age`
was in the authorization request; AS argument that it is optional is rejected
— optionality applies only when `max_age` is absent.

## Pair[CB4-3.4-1141]/Phrasing-B

Verdict REPROCESS. OIDC Core §3.1.3.7.11 states unconditionally: if a `max_age`
request is made, the ID token MUST contain `auth_time`. The AS's argument that
`auth_time` is optional is accurate only for authorization requests that do not
include `max_age`; once `max_age` is present, `auth_time` is required. Without
it, the RP cannot programmatically verify whether the authentication event
occurred within the requested window — the security purpose of `max_age` is
silently nullified.

---

## Pair[CB4-3.4-1142]/A

Confidential client application (a server-side web application capable of
maintaining credential confidentiality) is registered at the authorization
server with `token_endpoint_auth_method=none`. The AS accepts the registration
and subsequently processes token requests from this client with no client
authentication — the client presents only its `client_id`. The development
team argues that `none` is a valid value defined in the OIDC Dynamic
Registration specification and that the AS correctly implements it.

## Pair[CB4-3.4-1142]/B

REPROCESS. RFC 6749 §3.2.1 MUST: if the client type is confidential or the
client was issued credentials, the client MUST authenticate with the
authorization server at the token endpoint. `token_endpoint_auth_method=none`
removes all client authentication — permissible only for public clients (which
cannot maintain credential confidentiality). Registering a confidential client
with `none` and allowing unauthenticated token requests violates the MUST
unconditionally, regardless of what the registration specification defines as
syntactically valid.

## Pair[CB4-3.4-1142]/Reason

RFC 6749 §3.2.1 MUST authenticate confidential clients at token endpoint;
`token_endpoint_auth_method=none` valid only for public clients; registering
confidential client with `none` and granting unauthenticated requests → REPROCESS.

## Pair[CB4-3.4-1142]/Phrasing-B

Verdict REPROCESS. RFC 6749 §3.2.1 requires that confidential clients MUST
authenticate at the token endpoint. The `none` authentication method is defined
for public clients that are incapable of maintaining credentials — applying it
to a confidential server-side application removes the authentication MUST
entirely. The fact that `none` is a syntactically valid registration value
does not override the §3.2.1 authentication requirement; the MUST applies
to the client type, not to whether the AS chose to issue credentials at
registration.

---

## Pair[CB4-3.4-1143]/A

Authorization server issues refresh tokens for authorization_code flows with
rotation disabled — the same refresh token is valid indefinitely (up to an
absolute maximum lifetime). Each successful refresh token exchange at the
token endpoint returns a new access token but the same unchanged refresh token
value. The team argues that since the absolute lifetime is bounded (90 days)
and family revocation is in place, the absence of per-use rotation is
acceptable.

## Pair[CB4-3.4-1143]/B

CONCERN. RFC 9700 §4.14.1 SHOULD: authorization servers SHOULD rotate refresh
tokens on each use. Per-use rotation limits the exploitation window of a stolen
refresh token to the period between theft and next legitimate use — a stolen
token that has not yet been used by the legitimate user remains exploitable for
its full lifetime without rotation. The SHOULD-level guidance is not met; no
unconditional MUST applies. The absolute lifetime and family revocation are
valuable compensating controls but do not substitute for the SHOULD. CONCERN
not REPROCESS.

## Pair[CB4-3.4-1143]/Reason

RFC 9700 §4.14.1 SHOULD rotate refresh tokens on each use; absolute lifetime
and family revocation are compensating controls; no unconditional MUST for
per-use rotation → CONCERN.

## Pair[CB4-3.4-1143]/Phrasing-B

Verdict CONCERN. RFC 9700 §4.14.1 recommends per-use rotation (SHOULD) as a
defense against stolen refresh token exploitation. Without rotation, a stolen
refresh token remains exploitable for its entire lifetime even if the legitimate
user continues to use it (the attacker races to use the stolen token before the
user does). The 90-day absolute lifetime and family revocation are meaningful
controls but do not fulfill the SHOULD guidance; the gap is documented.
CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1144]/A

OIDC provider publishes an OpenID Connect Discovery document at
`https://as.example.com/.well-known/openid-configuration`. The discovery
document contains `issuer`, `authorization_endpoint`, `token_endpoint`,
`userinfo_endpoint`, `registration_endpoint`, and `scopes_supported` fields.
The `jwks_uri` field is absent from the document. The AS team argues that
clients that need the public keys can contact the AS team directly to obtain
the JWKS endpoint URL.

## Pair[CB4-3.4-1144]/B

REPROCESS. OIDC Core Discovery §3 classifies `jwks_uri` as REQUIRED: "URL of
the OP's JSON Web Key Set document — REQUIRED." Absent `jwks_uri` means
discovery-conformant clients have no standard mechanism to locate the public
keys needed to validate ID tokens and access tokens. The out-of-band
communication argument is rejected: the purpose of discovery is to provide
a machine-readable, programmatically consumable endpoint map; direct contact
with the AS team is not a substitute for a REQUIRED field.

## Pair[CB4-3.4-1144]/Reason

OIDC Discovery §3 REQUIRED: `jwks_uri` MUST be present in discovery document;
out-of-band key distribution argument rejected — discovery exists to eliminate
such manual processes.

## Pair[CB4-3.4-1144]/Phrasing-B

Verdict REPROCESS. OIDC Core Discovery §3 marks `jwks_uri` as REQUIRED in the
discovery document — not OPTIONAL or RECOMMENDED. Without `jwks_uri`, any
client relying on discovery has no standard way to fetch the JWKS needed to
verify ID token signatures. The manual out-of-band distribution argument defeats
the purpose of discovery-based dynamic configuration. The REQUIRED status is
unconditional; absence is a REPROCESS finding.

---

## Pair[CB4-3.4-1145]/A

OIDC provider supports RP-Initiated Logout (end-session endpoint at
`/end_session`). When a client sends `GET /end_session?id_token_hint=<token>
&post_logout_redirect_uri=https://attacker.example.com/landed`, the AS
processes the logout, terminates the session, and redirects the browser to
`https://attacker.example.com/landed` — without verifying whether that URI
was pre-registered by the RP. The AS team argues that validating the redirect
URI at logout is less critical than at authorization time because no tokens
are issued.

## Pair[CB4-3.4-1145]/B

REPROCESS. OIDC RP-Initiated Logout §4 MUST: if a `post_logout_redirect_uri`
is supplied, the OP MUST validate that the value is registered for the RP. The
absence-of-token argument is rejected: an unvalidated post-logout redirect URI
creates an open redirect vulnerability at the AS origin. An attacker can craft
a logout link that sends the victim's browser to an attacker-controlled page
after logout — usable for phishing (mimicking the AS or RP login page), for
session-stealing in combination with other attacks, or for social engineering.
The MUST is unconditional.

## Pair[CB4-3.4-1145]/Reason

OIDC RP-Initiated Logout §4 MUST validate `post_logout_redirect_uri` against
pre-registered URIs; open redirect vulnerability at AS; no-token argument
rejected — open redirect risk is independent of token issuance.

## Pair[CB4-3.4-1145]/Phrasing-B

Verdict REPROCESS. OIDC RP-Initiated Logout §4 requires the OP to MUST validate
`post_logout_redirect_uri` against the RP's registered redirect URIs. The AS's
argument that no tokens are involved misunderstands the open redirect threat:
the AS has established trust and its origin is used in browser security decisions.
An unvalidated post-logout redirect sends the user's browser to an arbitrary
destination after a legitimate logout interaction — a phishing and session attack
enabler. The MUST is unconditional regardless of whether tokens are issued.

---

## Pair[CB4-3.4-1146]/A

Resource server's exception handling logs full HTTP request headers in its
structured error log on 500-class errors. When a JWT access token is invalid
(expired, wrong issuer, signature failure), the RS logs the complete
`Authorization` header, including the full Bearer token value, as part of the
exception context. The logging is implemented to help diagnose validation
failures. The team argues that the error log is access-controlled and only
visible to the operations team.

## Pair[CB4-3.4-1146]/B

REPROCESS. ASVS V8.2.2 MUST NOT: authentication credentials and sensitive
tokens MUST NOT be logged. Bearer tokens are authentication credentials; logging
them in any log — regardless of access controls — violates the MUST
unconditionally. The access-controlled log argument is the same argument
rejected for client_secret logging: MUST NOT is unconditional; log aggregation
systems, backup stores, and log forwarding pipelines multiply the exposure
surface well beyond the intended audience. Also ASVS V9.3.1: tokens must be
protected throughout their lifecycle.

## Pair[CB4-3.4-1146]/Reason

ASVS V8.2.2 MUST NOT log authentication credentials; Bearer tokens are
credentials; access-controlled log argument rejected — MUST NOT is
unconditional; log propagation multiplies exposure surface.

## Pair[CB4-3.4-1146]/Phrasing-B

Verdict REPROCESS. ASVS V8.2.2 MUST NOT applies to authentication credentials
including Bearer tokens — not only to passwords and client secrets. Logging the
full Authorization header in error traces, even within an access-controlled log
system, violates the MUST NOT: log aggregators, backup retention, forwarding
pipelines, and incident-response tooling all extend the exposure surface beyond
the operations team. The MUST NOT is unconditional; the access-control argument
has been rejected for this pattern class. ASVS V9.3.1 reinforces: tokens require
protection throughout their lifecycle, including in diagnostic outputs.

---

## Pair[CB4-3.4-1147]/A

Authorization server issues JWT access tokens where the `sub` claim contains
the user's email address (`alice@example.com`). The development team argues
that email is a stable, globally unique identifier that allows resource servers
to identify the user directly without additional lookups, and that no PII
disclosure concern arises because the token is transmitted over HTTPS and
stored only server-side.

## Pair[CB4-3.4-1147]/B

CONCERN. RFC 9068 §2.2 requires `sub` to be present but does not unconditionally
mandate an opaque identifier — email is syntactically valid. ASVS V9.3.1
guidance: access tokens should not contain PII beyond what is necessary; an
email address in the JWT payload is visible to any party that possesses the
token (RS, log systems, intermediaries). The HTTPS / server-side-only argument
partially mitigates transit risk but does not address log exposure, RS-side
storage, or debug tooling. No unconditional MUST prohibits email as `sub` in AT;
CONCERN not REPROCESS.

## Pair[CB4-3.4-1147]/Reason

RFC 9068 §2.2 / ASVS V9.3.1 guidance: email as `sub` in AT contains PII;
no unconditional MUST prohibiting non-opaque sub in access tokens; guidance
gap → CONCERN.

## Pair[CB4-3.4-1147]/Phrasing-B

Verdict CONCERN. RFC 9068 §2.2 requires `sub` but does not mandate an opaque
identifier — the email address satisfies the structural MUST. However, ASVS
V9.3.1 and RFC 9068 best practices recommend opaque PPID (Pairwise Pseudonymous
Identifier) sub values precisely because the access token may appear in logs,
debug traces, and intermediary systems where email exposure is undesirable. The
HTTPS / server-side argument does not address log propagation or RS-side
processing. No unconditional MUST applies; guidance-level gap — CONCERN not
REPROCESS. Remediation: use opaque UUID-based PPID as `sub`.

---

## Pair[CB4-3.4-1148]/A

Authorization server issues JWT access tokens (RFC 9068 profile) that do not
contain a `scope` claim. The AS grants a specific scope to the access token
(e.g., `read:profile write:orders`) but does not embed it in the token body.
The scope is tracked server-side only, accessible via token introspection.
Resource servers relying solely on the self-contained JWT for authorization
decisions have no scope information and either allow all requests or implement
their own scope logic outside the OAuth model. The team argues that RFC 9068
marks scope as OPTIONAL.

## Pair[CB4-3.4-1148]/B

CONCERN. RFC 9068 §2.2.3.1 SHOULD: the authorization server SHOULD include
the `scope` claim in the access token. The team's assertion that `scope` is
OPTIONAL is technically accurate for syntactic compliance but ignores the
SHOULD recommendation. Without `scope` in the token, RS-side authorization
decisions based on self-contained JWTs are impossible; any RS that accepts
the token without introspection has no programmatic scope boundary to enforce.
This is a significant defense-in-depth gap and a guidance violation. No
unconditional MUST applies; CONCERN not REPROCESS.

## Pair[CB4-3.4-1148]/Reason

RFC 9068 §2.2.3.1 SHOULD include `scope` in access token; RS cannot enforce
scope from self-contained JWT without it; SHOULD-level guidance gap →
CONCERN.

## Pair[CB4-3.4-1148]/Phrasing-B

Verdict CONCERN. RFC 9068 §2.2.3.1 recommends including `scope` in the access
token (SHOULD). The team's citation of OPTIONAL is accurate for strict syntactic
compliance but conflates structural optionality with operational guidance. A
self-contained JWT without `scope` is unusable for RS-side authorization
enforcement without a separate introspection call — which reintroduces
centralized state for every request decision. The SHOULD guidance exists
precisely to enable stateless RS authorization. No unconditional MUST applies;
CONCERN not REPROCESS; remediation: include `scope` in all issued JWTs.
