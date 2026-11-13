# Node Pipeline Chain Set Extension 48
# Sub-phase: 3.4 Security and Authorization Model
# Pair ID prefix: CB4-3.4-
# Sequences: 1409–1416 (16 instances — 8 base pairs × 2 phrasings)
# Sources: RFC 8414 §3, RFC 6749 §5.1,
#          RFC 9126 §2.2, OIDC Core §5.3.2,
#          OIDC Discovery §3, RFC 9101 §10.3,
#          OIDC Core §3.1.3.7.4, RFC 9700 §4.3.1
# Running total after this file: 2832 instances (1416 base pairs)
# Formula check: 120 + (1416 − 60) × 2 = 120 + 2712 = 2832 ✓

---

## Pair[CB4-3.4-1409]/A

An OIDC provider's discovery document at
`https://as.example.com/.well-known/openid-configuration` contains
`"issuer":"https://auth.otherdomain.com"`. Relying parties that fetch the
discovery document and pin the issuer against the expected domain reject the
configuration. The AS team says the issuer reflects their canonical brand domain.
Evaluate pipeline assessment.

## Pair[CB4-3.4-1409]/B

Node 1 — Domain Splitter: OIDC/OAuth discovery metadata; issuer field validation;
RFC 8414 §3 issuer-URL alignment. Routes to RFC Resolver with tags: discovery,
metadata, issuer, RFC-8414, well-known, mismatch.

Node 2 — RFC Resolver: Primary citation — RFC 8414 §3 MUST: the issuer value
returned in authorization server metadata must be identical to the well-known
URI prefix used to retrieve the metadata. The discovery document at
`https://as.example.com/.well-known/openid-configuration` must have
`"issuer":"https://as.example.com"`. Any other value violates the binding.
Resolver notes: brand-domain argument is rejected; RFC 8414 §3 is unconditional.

Node 3 — Obligation Classifier: RFC 8414 §3 → MUST: issuer must match well-known
prefix. Issuer is `https://auth.otherdomain.com`; well-known prefix is
`https://as.example.com`. Mismatch confirmed. Classifier routes as MUST-level,
unmet.

Node 4 — Scenario Instantiator: Concrete violation — discovery document issuer
points to different domain than serving domain. Attacker exploit: an attacker
serving a discovery document from a compromised server can claim to be a trusted
issuer from a different domain. RPs that follow RFC 8414 §3 would detect the
mismatch; non-validating RPs are vulnerable.

Node 5 — Evidence Collector: Evidence of violation: (a) issuer="https://auth.otherdomain.com"
in discovery document served from `https://as.example.com`; (b) RFC 8414 §3 MUST
not met. Counter-argument logged: "canonical brand domain" — rejected; RFC 8414
§3 is unconditional.

Node 6 — Gap Analyzer: The issuer-URL binding is the trust anchor for OIDC
deployments. Mismatches destroy the trust chain: RPs cannot verify that the entity
serving the discovery document is the same entity that issued the tokens.

Node 7 — Verdict Engine: MUST violated. RFC 8414 §3: issuer must match well-known
URL prefix. Verdict: REPROCESS.

Node 8 — Arbiter: Confirms. Brand-domain argument rejected. RFC 8414 §3 MUST is
unconditional. REPROCESS.

Node 9 — Formatter: REPROCESS. RFC 8414 §3 MUST: the issuer value in discovery
metadata must exactly match the well-known URL prefix. A mismatch breaks the
trust binding between the discovery document and the issuer identity.
Brand-domain arguments do not override this MUST. REPROCESS.

## Pair[CB4-3.4-1409]/Reason

RFC 8414 §3 MUST: issuer must match well-known URL prefix; issuer points to
different domain; brand-domain argument rejected → REPROCESS.

## Pair[CB4-3.4-1409]/Phrasing-B

Verdict REPROCESS. RFC 8414 §3 MUST: issuer must equal the well-known URL prefix.
Discovery-serving domain and issuer domain mismatch breaks the trust anchor.
REPROCESS.

---

## Pair[CB4-3.4-1410]/A

An authorization server returns access token responses with `access_token`,
`expires_in`, and `scope` fields, but no `token_type` field. A client that
strictly follows RFC 6749 and RFC 6750 fails to process the response because it
cannot determine the token transmission scheme. The AS team argues that all
tokens are Bearer type and clients should simply assume Bearer.

## Pair[CB4-3.4-1410]/B

Node 1 — Domain Splitter: Token endpoint response; token_type field; REQUIRED
parameter; RFC 6749 §5.1; client token handling. Routes to RFC Resolver with
tags: token-response, token_type, REQUIRED, RFC-6749, Bearer, invalid-response.

Node 2 — RFC Resolver: Primary citation — RFC 6749 §5.1 REQUIRED: `token_type`
is a REQUIRED element of the access token response. Resolver notes: the token_type
value communicates how the token should be presented (Bearer per RFC 6750, DPoP
per RFC 9449, etc.); its absence prevents clients from determining the correct
transmission scheme.

Node 3 — Obligation Classifier: RFC 6749 §5.1 → REQUIRED (MUST include). token_type
absent from response. MUST unmet. Classifier routes as MUST-level, unmet.

Node 4 — Scenario Instantiator: Concrete violation — access token response without
token_type; RFC 6749-compliant clients fail; AS argues implicit Bearer assumption.
The implicit assumption argument creates interoperability failures for any client
that correctly follows the specification.

Node 5 — Evidence Collector: Evidence of violation: (a) token_type absent from
response; (b) RFC 6749 §5.1 REQUIRED not met; (c) RFC 6749-compliant clients
cannot process the response. Counter-argument logged: "all tokens are Bearer" —
rejected; REQUIRED is unconditional.

Node 6 — Gap Analyzer: The REQUIRED designation for token_type ensures explicit
protocol contract between AS and client. Implicit type assumptions lead to
interoperability failures and make it impossible for clients to support multiple
token types from the same AS.

Node 7 — Verdict Engine: MUST violated. RFC 6749 §5.1: token_type REQUIRED.
Implicit-type argument rejected. Verdict: REPROCESS.

Node 8 — Arbiter: Confirms. REQUIRED parameters cannot be omitted based on assumed
values. REPROCESS.

Node 9 — Formatter: REPROCESS. RFC 6749 §5.1 REQUIRED: token_type must be present
in access token responses. Implicit Bearer assumption is not an acceptable
substitute. Clients following the specification correctly cannot process token_type-
absent responses. REPROCESS.

## Pair[CB4-3.4-1410]/Reason

RFC 6749 §5.1 REQUIRED: token_type must be in access token response; absent;
implicit-Bearer argument rejected → REPROCESS.

## Pair[CB4-3.4-1410]/Phrasing-B

Verdict REPROCESS. RFC 6749 §5.1 REQUIRED: token_type is mandatory. No implicit
assumption substitutes for the required field. REPROCESS.

---

## Pair[CB4-3.4-1411]/A

A client successfully pushes an authorization request to the PAR endpoint.
The AS returns HTTP 201 with `{"expires_in":60}`. The client cannot find a
`request_uri` in the response and cannot initiate the authorization flow.
The AS team argues that the `expires_in` tells the client how long it has to
perform the next step and that the request_uri can be constructed from the
client_id and server timestamp.

## Pair[CB4-3.4-1411]/B

Node 1 — Domain Splitter: PAR response required elements; request_uri absence;
RFC 9126 §2.2 REQUIRED fields; client authorization flow dependency. Routes to
RFC Resolver with tags: PAR, request_uri, REQUIRED, RFC-9126, expires_in.

Node 2 — RFC Resolver: Primary citation — RFC 9126 §2.2 REQUIRED: the PAR
response must include `request_uri` (a server-assigned reference to the pushed
authorization request) and `expires_in`. Both are REQUIRED. The `request_uri`
is the reference the client MUST use in the subsequent authorization request.
Without it, PAR cannot function. Resolver notes: constructing request_uri from
client_id+timestamp has no basis in RFC 9126.

Node 3 — Obligation Classifier: RFC 9126 §2.2 → REQUIRED: request_uri must be
in PAR response. request_uri absent. MUST unmet. Classifier routes as MUST-level,
unmet.

Node 4 — Scenario Instantiator: Concrete violation — PAR response missing
request_uri; client cannot proceed with authorization; AS proposes client
construct the reference from client_id and timestamp. The construct-from-
client_id argument is not defined in RFC 9126 and would produce non-unique,
predictable references.

Node 5 — Evidence Collector: Evidence of violation: (a) PAR response lacks
request_uri; (b) RFC 9126 §2.2 REQUIRED not met; (c) AS's construction proposal
has no RFC basis. No valid justification for request_uri absence.

Node 6 — Gap Analyzer: The request_uri is the core deliverable of the PAR
interaction — it is what the client submits to the authorization endpoint.
Its absence renders PAR non-functional.

Node 7 — Verdict Engine: MUST violated. RFC 9126 §2.2: request_uri REQUIRED.
Construction argument rejected. Verdict: REPROCESS.

Node 8 — Arbiter: Confirms. PAR without request_uri is non-functional. Construction
argument has no RFC basis. REPROCESS.

Node 9 — Formatter: REPROCESS. RFC 9126 §2.2 REQUIRED: PAR responses must include
request_uri. Without it, the client cannot invoke the authorization flow. Client-
side construction of request_uri is not defined in RFC 9126. REPROCESS.

## Pair[CB4-3.4-1411]/Reason

RFC 9126 §2.2 REQUIRED: request_uri must be in PAR response; absent; construction
argument has no RFC basis → REPROCESS.

## Pair[CB4-3.4-1411]/Phrasing-B

Verdict REPROCESS. RFC 9126 §2.2 REQUIRED: request_uri is mandatory in PAR
responses. Its absence breaks the PAR flow. REPROCESS.

---

## Pair[CB4-3.4-1412]/A

An OIDC UserInfo endpoint returns `{"sub":"...","email":"user@example.com","name":"User Name"}`
in its HTTP response body. The response `Content-Type` header is `text/plain`.
Most client libraries parse the response successfully despite the incorrect
Content-Type. The AS team argues that behavior is what matters, not the header,
and that Content-Type is a best-effort hint.

## Pair[CB4-3.4-1412]/B

Node 1 — Domain Splitter: OIDC UserInfo endpoint; Content-Type header; MUST for
application/json; client parsing behavior. Routes to RFC Resolver with tags:
UserInfo, Content-Type, OIDC-Core, MUST, application-json, text-plain.

Node 2 — RFC Resolver: Primary citation — OIDC Core §5.3.2 MUST: the UserInfo
endpoint MUST return `Content-Type: application/json`. Secondary citation —
HTTP specification: Content-Type communicates the media type of the response
for client processing decisions. Resolver notes: parsing success is irrelevant
to the Content-Type MUST obligation.

Node 3 — Obligation Classifier: OIDC Core §5.3.2 → MUST: Content-Type must be
application/json. Response uses text/plain. MUST unmet. Classifier routes as
MUST-level, unmet.

Node 4 — Scenario Instantiator: Concrete violation — Content-Type: text/plain
on a UserInfo JSON response; OIDC Core §5.3.2 MUST not met; strict clients reject.
The best-effort-hint argument mischaracterizes Content-Type as optional — it is
normatively required by OIDC Core.

Node 5 — Evidence Collector: Evidence of violation: (a) Content-Type: text/plain;
(b) OIDC Core §5.3.2 MUST not met; (c) strict RFC-compliant clients reject the
response. Counter-argument logged: "Content-Type is best-effort" — rejected;
§5.3.2 MUST is unconditional.

Node 6 — Gap Analyzer: Correct Content-Type ensures that all HTTP clients
(not just lenient ones) can process the response. Strict media-type validation
is common in enterprise HTTP stacks.

Node 7 — Verdict Engine: MUST violated. OIDC Core §5.3.2: Content-Type must be
application/json. Verdict: REPROCESS.

Node 8 — Arbiter: Confirms. Behavior-over-headers argument rejected. Content-Type
MUST is structural and unconditional. REPROCESS.

Node 9 — Formatter: REPROCESS. OIDC Core §5.3.2 MUST: UserInfo responses must
have Content-Type: application/json. text/plain is non-conformant. Client parsing
success does not satisfy the Content-Type MUST. REPROCESS.

## Pair[CB4-3.4-1412]/Reason

OIDC Core §5.3.2 MUST: UserInfo response Content-Type must be application/json;
text/plain returned; best-effort argument rejected → REPROCESS.

## Pair[CB4-3.4-1412]/Phrasing-B

Verdict REPROCESS. OIDC Core §5.3.2 MUST: Content-Type application/json is
required for UserInfo responses. text/plain violates this MUST. REPROCESS.

---

## Pair[CB4-3.4-1413]/A

An OIDC provider's discovery document contains all required fields but omits the
`userinfo_endpoint` URL. The AS does operate a UserInfo endpoint, but its URL
is not published in the discovery document. Client integrations that dynamically
discover the UserInfo endpoint URL via the discovery document cannot locate it
and fall back to hardcoded URLs. The AS team argues that the UserInfo endpoint
URL is stable and clients can hard-code it.

## Pair[CB4-3.4-1413]/B

Node 1 — Domain Splitter: OIDC discovery document; userinfo_endpoint field;
OIDC Discovery §3 RECOMMENDED; static URL hardcoding. Routes to RFC Resolver
with tags: discovery, userinfo_endpoint, RECOMMENDED, OIDC-Discovery, hardcode.

Node 2 — RFC Resolver: Primary citation — OIDC Discovery §3: userinfo_endpoint
is a RECOMMENDED element in the discovery document when the AS provides a UserInfo
endpoint. No unconditional MUST requires it. Resolver notes: when the AS provides
a UserInfo endpoint, including it in discovery is recommended so clients can
dynamically discover the URL.

Node 3 — Obligation Classifier: OIDC Discovery §3 → RECOMMENDED (SHOULD). No
MUST. Hardcode argument is an accepted workaround. Classifier routes as guidance-
level, absent.

Node 4 — Scenario Instantiator: Guidance gap — userinfo_endpoint operating but
not published in discovery; clients fall back to hardcoded URL. No MUST violated.
Hardcode is a valid workaround with tight coupling trade-off.

Node 5 — Evidence Collector: Evidence of gap: (a) userinfo_endpoint present but
not in discovery; (b) OIDC Discovery §3 RECOMMENDED not met; (c) AS team's
hardcode argument is normatively valid — no MUST requires publication.

Node 6 — Gap Analyzer: The gap creates tight coupling between clients and the
AS infrastructure. Discovery document updates cannot communicate URL changes.
However, no MUST is violated.

Node 7 — Verdict Engine: No MUST violated. OIDC Discovery §3 RECOMMENDED;
guidance-level gap. Verdict: CONCERN.

Node 8 — Arbiter: Confirms. AS team's argument is normatively valid. CONCERN not
REPROCESS.

Node 9 — Formatter: CONCERN. OIDC Discovery §3 RECOMMENDED: userinfo_endpoint
should be published in the discovery document when provided. No MUST mandates it.
Its absence creates tight client coupling. Guidance-level gap. CONCERN not REPROCESS.

## Pair[CB4-3.4-1413]/Reason

OIDC Discovery §3 RECOMMENDED: userinfo_endpoint should be in discovery document;
no MUST; hardcode argument normatively valid; guidance-level gap → CONCERN.

## Pair[CB4-3.4-1413]/Phrasing-B

Verdict CONCERN. OIDC Discovery §3 RECOMMENDED: userinfo_endpoint inclusion is
recommended. No MUST mandates it. Its absence is a guidance-level gap with tight-
coupling implications. CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1414]/A

A client sends a JAR request object with `iss`, `aud`, `exp`, and `iat` claims.
The request object does not include a `nbf` (not-before) claim. The AS processes
the request successfully. A security reviewer asks whether the missing `nbf` is
a specification violation.

## Pair[CB4-3.4-1414]/B

Node 1 — Domain Splitter: JAR request object claims; nbf claim presence; RFC 9101
§10.3 guidance; request object lifetime limits. Routes to RFC Resolver with tags:
JAR, nbf, not-before, RFC-9101, guidance, optional.

Node 2 — RFC Resolver: Primary citation — RFC 9101 §10.3 guidance: request objects
SHOULD have tightly bounded lifetimes using `exp` and `nbf` to limit the window
in which a captured request object can be replayed. No section of RFC 9101
establishes an unconditional MUST for the `nbf` claim in request objects.
Resolver notes: `exp` provides the forward time boundary; `nbf` provides the
backward boundary; both are recommended but only exp is typically checked.

Node 3 — Obligation Classifier: RFC 9101 §10.3 → SHOULD (guidance). No MUST for
nbf. Classifier routes as guidance-level, absent.

Node 4 — Scenario Instantiator: Guidance gap — nbf absent from JAR; exp present;
AS processes successfully. Without nbf, the request object is valid from the
moment of issuance (no backward boundary). Minor security gap at guidance level.

Node 5 — Evidence Collector: Evidence of gap: (a) nbf absent from request JWT;
(b) RFC 9101 §10.3 SHOULD not fully met; (c) no unconditional MUST for nbf.

Node 6 — Gap Analyzer: nbf provides defense-in-depth for request objects. Without
it, there is no "not before" window to detect replayed or early-issued requests.
However, the guidance level means this is a recommendation, not a mandate.

Node 7 — Verdict Engine: No MUST violated. RFC 9101 §10.3 SHOULD; guidance-level
gap. Verdict: CONCERN.

Node 8 — Arbiter: Confirms. nbf is guidance-level for JAR request objects.
CONCERN not REPROCESS.

Node 9 — Formatter: CONCERN. RFC 9101 §10.3 guidance: nbf is recommended for
request objects to tightly bound their validity window. No MUST mandates its
presence. Absence is a guidance-level gap. CONCERN not REPROCESS.

## Pair[CB4-3.4-1414]/Reason

RFC 9101 §10.3 guidance: nbf recommended for JAR request objects; no unconditional
MUST; absent nbf is guidance-level → CONCERN.

## Pair[CB4-3.4-1414]/Phrasing-B

Verdict CONCERN. RFC 9101 §10.3 guidance: nbf is recommended. No MUST mandates
it in request objects. Its absence is a guidance-level gap. CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1415]/A

An OIDC relying party processes an ID token with `aud=["rp-client","third-party"]`
and `azp="rp-client"`. The RP's validation pipeline verifies: signature, iss,
aud membership, exp, nonce. It does not check whether `azp` equals its own
`client_id`. The azp is "rp-client" which would pass the check if performed.
The RP team argues that aud membership already guarantees the token was issued
for the RP, making azp redundant.

## Pair[CB4-3.4-1415]/B

Node 1 — Domain Splitter: OIDC ID token validation; azp claim check for multi-
audience tokens; OIDC Core §3.1.3.7.4 SHOULD; RP validation pipeline. Routes to
RFC Resolver with tags: azp, authorized-party, multi-audience, OIDC-Core, SHOULD.

Node 2 — RFC Resolver: Primary citation — OIDC Core §3.1.3.7.4 SHOULD: when the
ID token has multiple audience values, the RP SHOULD verify that its client_id is
the value of the azp Claim. This is a SHOULD-level obligation. Resolver notes:
azp provides an additional binding confirming the authorized party beyond aud
membership.

Node 3 — Obligation Classifier: OIDC Core §3.1.3.7.4 → SHOULD. No MUST. The RP
omits the azp check. Classifier routes as guidance-level, absent.

Node 4 — Scenario Instantiator: Guidance gap — RP skips azp validation when aud
is multi-valued; azp is correct in this case. Without the check, a multi-audience
token where azp points to a different client would be accepted.

Node 5 — Evidence Collector: Evidence of gap: (a) multi-valued aud; (b) azp
present; (c) RP skips azp validation; (d) OIDC Core §3.1.3.7.4 SHOULD not met;
(e) no MUST violated. RP team's aud-sufficiency argument is normatively correct
at the MUST level.

Node 6 — Gap Analyzer: The azp check is defense-in-depth for multi-audience
scenarios. Without it, a multi-audience token with an incorrect azp would be
accepted. SHOULD-level gap.

Node 7 — Verdict Engine: No MUST violated. OIDC Core §3.1.3.7.4 SHOULD; guidance-
level gap. Verdict: CONCERN.

Node 8 — Arbiter: Confirms. RP team's aud-sufficiency argument is normatively
correct at the MUST level. azp validation is SHOULD. CONCERN not REPROCESS.

Node 9 — Formatter: CONCERN. OIDC Core §3.1.3.7.4 SHOULD: when aud is multi-
valued, the RP should verify azp equals its client_id. No MUST mandates this check.
Omitting it is a guidance-level gap. CONCERN not REPROCESS.

## Pair[CB4-3.4-1415]/Reason

OIDC Core §3.1.3.7.4 SHOULD: RP should validate azp when aud is multi-valued;
no MUST; azp check omitted; aud-sufficiency argument normatively correct at MUST
level → CONCERN.

## Pair[CB4-3.4-1415]/Phrasing-B

Verdict CONCERN. OIDC Core §3.1.3.7.4 SHOULD: azp validation is recommended for
multi-audience tokens. No MUST mandates it. Omission is guidance-level. CONCERN
not REPROCESS.

---

## Pair[CB4-3.4-1416]/A

An authorization server issues authorization codes for public clients (SPAs, native
apps) without requiring PKCE. The AS accepts token requests from public clients
that present only the authorization code, with no `code_verifier`. The AS relies
on exact redirect_uri matching and short code lifetimes as its primary security
controls for public clients. The team asks whether this configuration is a MUST
violation.

## Pair[CB4-3.4-1416]/B

Node 1 — Domain Splitter: PKCE requirement for public clients; authorization code
protection; RFC 9700 §4.3.2; redirect URI matching as compensating control.
Routes to RFC Resolver with tags: PKCE, public-clients, RFC-9700, code-injection,
SHOULD.

Node 2 — RFC Resolver: Primary citation — RFC 9700 §4.3.2 SHOULD: authorization
servers should require PKCE for all public clients. This is a SHOULD-level
recommendation — no unconditional RFC MUST mandates PKCE for public clients.
Redirect URI exact-match and short code lifetime are partial compensating controls
acknowledged in security guidance. Resolver notes: no MUST anchor for PKCE
requirement.

Node 3 — Obligation Classifier: RFC 9700 §4.3.2 → SHOULD. No MUST. Classifier
routes as guidance-level, unmet.

Node 4 — Scenario Instantiator: Guidance gap — public clients complete
authorization code flows without PKCE; redirect URI matching and short TTL are
present as compensating controls. No MUST violated. Real security risk exists:
code injection attacks are possible without PKCE.

Node 5 — Evidence Collector: Evidence of gap: (a) public clients can exchange
codes without code_verifier; (b) RFC 9700 §4.3.2 SHOULD not met; (c) compensating
controls present; (d) no MUST violated.

Node 6 — Gap Analyzer: Without PKCE, public client authorization codes can be
injected by an attacker who can intercept or guess a code. The redirect URI
matching reduces the attack surface but does not prevent code injection by an
authorized-but-malicious app that shares the redirect URI.

Node 7 — Verdict Engine: No MUST violated. RFC 9700 §4.3.2 SHOULD; guidance-level
gap. Verdict: CONCERN.

Node 8 — Arbiter: Confirms. Team's question answered: no MUST violation. Guidance-
level gap with real injection risk. CONCERN not REPROCESS.

Node 9 — Formatter: CONCERN. RFC 9700 §4.3.2 SHOULD: PKCE for public clients is
recommended. No unconditional MUST mandates it. Not requiring PKCE is a guidance-
level gap with meaningful code injection risk. CONCERN not REPROCESS.

## Pair[CB4-3.4-1416]/Reason

RFC 9700 §4.3.2 SHOULD: PKCE should be required for public clients; AS doesn't
enforce PKCE for public clients; no unconditional MUST → CONCERN.

## Pair[CB4-3.4-1416]/Phrasing-B

Verdict CONCERN. RFC 9700 §4.3.2 SHOULD: PKCE for public clients is recommended.
No MUST mandates it. Absence of PKCE requirement is a guidance-level gap. CONCERN
not REPROCESS.
