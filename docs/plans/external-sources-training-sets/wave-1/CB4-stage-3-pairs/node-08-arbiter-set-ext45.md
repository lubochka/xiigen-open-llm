# Node 08 — Arbiter Set Extension 45
# Sub-phase: 3.4 Security and Authorization Model
# Pair ID prefix: CB4-3.4-
# Sequences: 1167–1174 (16 instances — 8 base pairs × 2 phrasings)
# Sources: OIDC Core §3.1.2.2 / §3.1.3.7.1 / §8.2, RFC 6749 §3.2 / §3.3,
#          CWE-209, ASVS V14.3.2, RFC 9068 §2.2, RFC 7519 §4.1.1
# Running total after this file: 2348 instances (1174 base pairs)
# Formula check: 120 + (1174 − 60) × 2 = 120 + 2228 = 2348 ✓

---

## Pair[CB4-3.4-1167]/A

Client sends an authorization request with `prompt=none` — indicating that the
AS MUST NOT display any authentication or consent UI. The user does not have an
active session at the AS. Instead of returning a `login_required` error redirect
to the client, the AS redirects the user's browser to the login page and
displays the login form. The AS team argues that showing the login page is a
better user experience than an error and that the client can handle the
subsequent interaction.

## Pair[CB4-3.4-1167]/B

REPROCESS. OIDC Core §3.1.2.2 MUST NOT: when `prompt=none` is received, the
authorization server MUST NOT display any authentication or consent interface
to the end user. If the end user is not already authenticated and silent
authentication is not possible, the AS MUST return a `login_required` error
response to the client — not display UI. Redirecting to the login page
violates the MUST NOT unconditionally; the UX argument is irrelevant to
normative compliance. `prompt=none` is a contract between RP and AS; the RP
relies on the error to trigger its own flow, not on the AS displaying UI.

## Pair[CB4-3.4-1167]/Reason

OIDC Core §3.1.2.2 MUST NOT display UI when prompt=none; MUST return
login_required error when user not authenticated; UX argument rejected —
RP relies on the error code to manage its own flow.

## Pair[CB4-3.4-1167]/Phrasing-B

Verdict REPROCESS. OIDC Core §3.1.2.2 is unconditional: `prompt=none` means
the AS MUST NOT display any user interface — authentication, consent, or
otherwise. The correct behavior when the user has no active session is to
return `login_required` in the authorization response redirect. The AS's
decision to display the login page overrides the MUST NOT and breaks the RP's
ability to distinguish silent success from silent failure programmatically.

---

## Pair[CB4-3.4-1168]/A

Client application registered for `client_credentials` grant with registered
scope `read:metrics write:metrics`. At the token endpoint, the client sends
`grant_type=client_credentials&scope=read:metrics write:metrics admin:system`.
The AS issues a token with scope `read:metrics write:metrics admin:system` —
including `admin:system` which was not in the client's registered scope.

## Pair[CB4-3.4-1168]/B

REPROCESS. RFC 6749 §3.3 and CWE-269 (Improper Privilege Management): the
scope of an access token MUST NOT exceed what the authorization server has
registered for and authorized to the requesting client. Issuing `admin:system`
scope that was not registered for the client is a scope elevation — a grant of
privilege beyond what the client is entitled to receive. The client's request
does not expand its registered entitlement; the AS MUST return only scopes
within the registered scope set, or return `invalid_scope` if the requested
scope is outside that set.

## Pair[CB4-3.4-1168]/Reason

RFC 6749 §3.3 scope ceiling / CWE-269: AT scope MUST NOT exceed client's
registered scope; issuing admin:system (not in registration) is scope
elevation → REPROCESS.

## Pair[CB4-3.4-1168]/Phrasing-B

Verdict REPROCESS. RFC 6749 §3.3 establishes that the issued scope is bounded
by what the AS has authorized for the client. The client's registered scope is
`read:metrics write:metrics`; requesting and receiving `admin:system` exceeds
that boundary unconditionally. CWE-269 classifies this as improper privilege
management — the client receives capabilities beyond its registration. The AS
MUST either honor only the intersection of requested and registered scopes or
return `error=invalid_scope`.

---

## Pair[CB4-3.4-1169]/A

Authorization server token endpoint returns error responses that include full
Java exception stack traces in the error response body when an unexpected server
error occurs. Example response body:
`{"error": "server_error", "error_description": "java.lang.NullPointerException
at com.example.oauth.TokenEndpoint.processRequest(TokenEndpoint.java:142)..."}`.
The operations team argues that including stack traces helps API clients
diagnose integration issues more quickly.

## Pair[CB4-3.4-1169]/B

CONCERN. CWE-209 (Information Exposure Through an Error Message) and ASVS
V14.3.2: error responses SHOULD NOT include internal implementation details
such as stack traces, class names, or file paths. Stack traces in error
responses reveal technology stack, internal code structure, and potential attack
surface to any client receiving the error — including unauthenticated callers.
The developer-helpfulness argument is rejected for external-facing endpoints:
internal debugging details belong in server-side logs, not in API responses.
No unconditional RFC MUST prohibits informative error detail in all cases;
ASVS guidance gap → CONCERN not REPROCESS.

## Pair[CB4-3.4-1169]/Reason

CWE-209 + ASVS V14.3.2 SHOULD NOT include stack traces in error responses;
reveals technology stack and code structure; guidance gap — no unconditional
RFC MUST → CONCERN.

## Pair[CB4-3.4-1169]/Phrasing-B

Verdict CONCERN. CWE-209 identifies information disclosure through error
messages as a recognized weakness. ASVS V14.3.2 provides the corresponding
guidance: error responses should not reveal internal implementation details.
Stack traces expose class hierarchy, file names, line numbers, and
implementation logic to any caller who can trigger a 500-level response.
No unconditional RFC MUST prohibits informative error detail in all contexts;
ASVS guidance applies — CONCERN not REPROCESS. Remediation: log stack traces
server-side; return only `error=server_error` externally.

---

## Pair[CB4-3.4-1170]/A

Authorization server issues JWT access tokens (RFC 9068 profile) without
including a `nbf` (not-before) claim. The resource server performs `exp`, `iss`,
and `aud` validation but has no `nbf` value to validate temporal start bounds.
A token issued for a future-dated deployment context (e.g., scheduled batch
job) cannot be constrained to a start window. The AS team argues that `nbf` is
classified as OPTIONAL in RFC 9068 and that `exp` alone defines the validity
window adequately.

## Pair[CB4-3.4-1170]/B

CONCERN. RFC 9068 §2.2 classifies `nbf` as OPTIONAL; the AS's citation of
OPTIONAL status is correct. The gap is that `nbf` provides the temporal
start bound that complements `exp`'s end bound — together they define a closed
validity window. Without `nbf`, access tokens for scheduled or pre-issued
contexts have an unconstrained start bound; a token intended for use in
a specific window is valid from issuance rather than from the intended start.
No unconditional MUST applies for `nbf` inclusion; guidance-level temporal
binding gap → CONCERN not REPROCESS.

## Pair[CB4-3.4-1170]/Reason

RFC 9068 §2.2 `nbf` OPTIONAL; absence removes temporal start-bound definition
from AT; guidance-level gap — no unconditional MUST for `nbf` inclusion → CONCERN.

## Pair[CB4-3.4-1170]/Phrasing-B

Verdict CONCERN. RFC 9068 §2.2 marks `nbf` as OPTIONAL — the AS's citation is
accurate. The gap is operational and security-relevant: `nbf` allows the AS to
issue a token that is not yet valid, enabling time-windowed authorization
scenarios (e.g., scheduled batch processing). Without `nbf`, the only validity
bound is `exp`; the token is usable from the moment of issuance regardless of
intended start time. Guidance-level gap; no unconditional MUST → CONCERN.

---

## Pair[CB4-3.4-1171]/A

Authorization server's token endpoint accepts both HTTP `POST` and HTTP `GET`
requests. When `GET /token?grant_type=authorization_code&code=<code>
&redirect_uri=<uri>&client_id=<id>&client_secret=<secret>` is submitted, the
AS processes the request and returns an access token. The team argues that
supporting GET improves integration flexibility for clients that cannot easily
set request body content.

## Pair[CB4-3.4-1171]/B

REPROCESS. RFC 6749 §3.2 MUST: "The client MUST use the HTTP POST method when
making access token requests." GET requests to the token endpoint transmit all
parameters — including `client_secret`, `code`, and the resulting token — via
the URL query string, which is recorded in server access logs, proxy logs,
browser history, and HTTP `Referer` headers on subsequent requests. Supporting
GET at the token endpoint violates the MUST unconditionally; the
integration-flexibility argument does not override a normative MUST.

## Pair[CB4-3.4-1171]/Reason

RFC 6749 §3.2 MUST use HTTP POST for token requests; GET transmits credentials
in URL (logged in proxies, history, Referer); MUST is unconditional → REPROCESS.

## Pair[CB4-3.4-1171]/Phrasing-B

Verdict REPROCESS. RFC 6749 §3.2 requires HTTP POST for all access token
requests — MUST. GET method transmits `client_secret`, `code`, and response
tokens via the URL query string, where they are captured in server logs, proxy
logs, browser history, and `Referer` headers. The flexibility argument does not
override the MUST; clients that cannot send POST bodies are not conformant
OAuth 2.0 clients. The token endpoint MUST reject GET requests.

---

## Pair[CB4-3.4-1172]/A

Relying party (RP) application validates OIDC ID tokens by checking that the
`iss` claim matches `as.example.com` using a case-insensitive string comparison
(`iss.toLowerCase() === "as.example.com"`). The AS's actual `iss` value is
`https://AS.Example.COM` (mixed case). The RP's case-insensitive comparison
accepts the token. The RP team argues that hostnames are case-insensitive per
DNS, so comparing issuers case-insensitively is correct and standards-compliant.

## Pair[CB4-3.4-1172]/B

REPROCESS. OIDC Core §3.1.3.7.1 and RFC 7519 §4.1.1 both require case-sensitive
exact string comparison for the `iss` claim. RFC 7519 §4.1.1: "The 'iss' value
is a case-sensitive string." OIDC Core §3.1.3.7.2: the `iss` Claim Value MUST
be identical to the Issuer Identifier for the OpenID Provider URL. The DNS
hostname case-insensitivity argument is rejected: the `iss` claim is a URI
string identifier, not a hostname for DNS resolution — its comparison rule is
defined by the JWT and OIDC specifications as case-sensitive, regardless of how
DNS handles the embedded hostname component.

## Pair[CB4-3.4-1172]/Reason

OIDC Core §3.1.3.7.1 / RFC 7519 §4.1.1: `iss` MUST be compared case-sensitively;
DNS hostname case-insensitivity argument rejected — iss is a URI string
identifier, not a DNS hostname; case-sensitive per JWT spec.

## Pair[CB4-3.4-1172]/Phrasing-B

Verdict REPROCESS. RFC 7519 §4.1.1 defines `iss` as a case-sensitive string.
OIDC Core §3.1.3.7.2 requires exact (byte-for-byte) matching against the
Issuer Identifier. Case-insensitive comparison allows `https://attacker.example.com`
to match `https://AS.Example.COM` if their case-folded forms collide. The DNS
argument misidentifies `iss` as a hostname lookup; it is a URI string identifier
whose comparison semantics are defined by JWT — case-sensitive — not by DNS.
REPROCESS: `iss` validation is incorrect.

---

## Pair[CB4-3.4-1173]/A

Authorization server receives an authorization request containing a nonce
parameter (`nonce=abc123xyz`). At the token endpoint, the AS issues an ID token
containing `"nonce": "HASHED_abc123xyz"` — the AS has applied a SHA-256 hash
to the nonce before embedding it, based on a decision to avoid echo-ing raw
nonce values for privacy reasons. The RP receives the ID token and compares
`nonce` value in the token against the original `abc123xyz` — the comparison
fails; the RP rejects the token.

## Pair[CB4-3.4-1173]/B

REPROCESS. OIDC Core §3.1.2.2 requires the nonce to be "passed through
unmodified from the Authentication Request to the ID Token." The AS MUST
include the nonce exactly as received in the request; any transformation —
including hashing, encoding, or truncation — violates the unmodified passthrough
requirement. The privacy motivation is acknowledged but does not override the
MUST: the nonce's security guarantee (replay binding) depends on the RP being
able to match the token's nonce against the one it sent. Modification breaks
that match unconditionally.

## Pair[CB4-3.4-1173]/Reason

OIDC Core §3.1.2.2: nonce MUST be passed through unmodified from auth request
to ID token; AS transformation (hash) violates unmodified requirement; RP
match breaks → REPROCESS.

## Pair[CB4-3.4-1173]/Phrasing-B

Verdict REPROCESS. OIDC Core §3.1.2.2 specifies that the nonce is "passed
through unmodified from the Authentication Request to the ID Token." The AS's
decision to hash the nonce before embedding it violates the unmodified
passthrough MUST: the RP receives a token whose nonce does not match the value
it sent, making replay detection impossible. The privacy rationale does not
override the normative requirement; if the AS wishes to avoid echoing raw
values, the design must not involve transformation of the nonce claim.

---

## Pair[CB4-3.4-1174]/A

OIDC Relying Party application supports multiple identity providers (Google,
Azure AD, and an internal AS). User authentication lookup uses only the `sub`
claim from the ID token: `SELECT * FROM users WHERE sub = ?`. The RP does not
include `iss` in the lookup key. A user at Google with `sub=12345` and a user
at the internal AS with `sub=12345` would be treated as the same identity.

## Pair[CB4-3.4-1174]/B

REPROCESS. OIDC Core §8.2 states that `sub` identifiers are locally unique
within an issuer — they are NOT globally unique across issuers. The correct
identity key is the `(iss, sub)` pair, not `sub` alone. Using `sub` alone for
user lookup in a multi-issuer environment creates an identity confusion
vulnerability: a user from one IdP whose `sub` matches another user's `sub`
at a different IdP is authenticated as the wrong user. This is an authorization
boundary violation (CWE-284) traceable to incorrect identifier scoping.

## Pair[CB4-3.4-1174]/Reason

OIDC Core §8.2: `sub` is locally unique per issuer; not globally unique;
multi-issuer RP MUST use (iss, sub) pair as identity key; sub-alone lookup =
identity confusion CWE-284 → REPROCESS.

## Pair[CB4-3.4-1174]/Phrasing-B

Verdict REPROCESS. OIDC Core §8.2 is explicit: the `sub` Claim is a locally
unique identifier within the issuer — it is unique per `iss`, not globally.
A multi-issuer RP that uses `sub` alone as the user lookup key will conflate
users from different identity providers who happen to share the same sub value.
The correct identity anchor is the `(iss, sub)` pair; both fields MUST be part
of the user lookup key. Sub-alone lookup in a multi-issuer environment is a
CWE-284 (Improper Access Control) identity confusion vulnerability.
