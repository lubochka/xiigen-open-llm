# Node Pipeline Chain Set Extension 49
# Sub-phase: 3.4 Security and Authorization Model
# Pair ID prefix: CB4-3.4-
# Sequences: 1435–1442 (16 instances — 8 base pairs × 2 phrasings)
# Sources: RFC 6749 §5.2, OIDC Core §3.1.3.3,
#          RFC 9126 §2, RFC 9068 §2.2,
#          RFC 6750 §3.1, OIDC Discovery §3,
#          RFC 8414 §2, RFC 6749 §7
# Running total after this file: 2884 instances (1442 base pairs)
# Formula check: 120 + (1442 − 60) × 2 = 120 + 2764 = 2884 ✓

---

## Pair[CB4-3.4-1435]/A

Scenario input: an authorization server's token endpoint returns HTTP 400 with
body `{"error_description":"invalid_client"}` — the `error` field is absent.
Trace the scenario through all nine pipeline nodes and state the final verdict.

## Pair[CB4-3.4-1435]/B

Node 1 (Domain Splitter): classifies input as token-endpoint-error-response
domain → security-verification-3.4 flow.
Node 2 (Domain Translator): maps to RFC 6749 §5.2 error response requirements;
`error` REQUIRED parameter context established.
Node 3 (Scheme Planner): plans REQUIRED-field compliance check against token
endpoint error response structure.
Node 4 (Domain Processor): confirms `error` parameter absent from response body;
RFC 6749 §5.2 mandates its presence unconditionally.
Node 5 (Applier): applies RFC 6749 §5.2 REQUIRED standard; `error` absent =
REQUIRED violation detected.
Node 6 (Result Translator): translates to REPROCESS signal; REQUIRED designation
= MUST-level.
Node 7 (Arbiter Selector): selects RFC 6749 §5.2 arbiter rule.
Node 8 (Arbiter): RFC 6749 §5.2 REQUIRED: `error` must be present in token
endpoint error responses; absent → REPROCESS.
Node 9 (Formatter): REPROCESS. RFC 6749 §5.2 REQUIRED: `error` parameter is
mandatory in token endpoint error responses. error_description alone does not
satisfy the REQUIRED designation. REPROCESS.

## Pair[CB4-3.4-1435]/Reason

Nine-node trace: token endpoint error response omits REQUIRED `error` parameter
(RFC 6749 §5.2) → all nodes converge → REPROCESS.

## Pair[CB4-3.4-1435]/Phrasing-B

Verdict REPROCESS. Pipeline: Domain Splitter → token-endpoint-error-response →
RFC 6749 §5.2 REQUIRED `error` absent → REPROCESS at Arbiter → REPROCESS.

---

## Pair[CB4-3.4-1436]/A

Scenario input: a client sends an OIDC authentication request with a `nonce`
parameter; the AS issues an ID token without a `nonce` claim. Trace the scenario
through all nine pipeline nodes and state the final verdict.

## Pair[CB4-3.4-1436]/B

Node 1 (Domain Splitter): classifies input as OIDC ID token issuance domain →
security-verification-3.4 flow.
Node 2 (Domain Translator): maps to OIDC Core §3.1.3.3 nonce inclusion
requirements; nonce-in-ID-token context established.
Node 3 (Scheme Planner): plans MUST-field compliance check for nonce claim
presence when nonce was in authentication request.
Node 4 (Domain Processor): confirms nonce was present in authentication request;
confirms nonce claim absent from issued ID token.
Node 5 (Applier): applies OIDC Core §3.1.3.3 MUST standard; nonce absent from
ID token when request nonce present = MUST violation detected.
Node 6 (Result Translator): translates to REPROCESS signal; unconditional MUST.
Node 7 (Arbiter Selector): selects OIDC Core §3.1.3.3 nonce arbiter rule.
Node 8 (Arbiter): OIDC Core §3.1.3.3 MUST: nonce must be in ID token when
nonce was in authentication request; absent → REPROCESS.
Node 9 (Formatter): REPROCESS. OIDC Core §3.1.3.3 MUST: nonce must be echoed
from authentication request into ID token. Its absence disables replay protection.
REPROCESS.

## Pair[CB4-3.4-1436]/Reason

Nine-node trace: nonce in auth request but absent from ID token violates OIDC
Core §3.1.3.3 MUST → all nodes converge → REPROCESS.

## Pair[CB4-3.4-1436]/Phrasing-B

Verdict REPROCESS. Pipeline: Domain Splitter → OIDC nonce issuance → OIDC Core
§3.1.3.3 MUST nonce absent from ID token → REPROCESS at Arbiter → REPROCESS.

---

## Pair[CB4-3.4-1437]/A

Scenario input: a confidential client sends a PAR request to the AS's PAR
endpoint including only `client_id` in the request body — no client secret,
no Authorization header; the AS accepts it and returns a successful PAR response.
Trace the scenario through all nine pipeline nodes and state the final verdict.

## Pair[CB4-3.4-1437]/B

Node 1 (Domain Splitter): classifies input as pushed-authorization-request
authentication domain → security-verification-3.4 flow.
Node 2 (Domain Translator): maps to RFC 9126 §2 client authentication requirements
at the PAR endpoint; confidential-client-auth context established.
Node 3 (Scheme Planner): plans MUST authentication enforcement check for
confidential clients at PAR endpoint.
Node 4 (Domain Processor): confirms client is registered as confidential;
confirms no credentials were presented at PAR endpoint; confirms AS accepted
the unauthenticated request.
Node 5 (Applier): applies RFC 9126 §2 MUST standard; confidential client
accepted at PAR without credentials = MUST violation detected.
Node 6 (Result Translator): translates to REPROCESS signal; unconditional MUST.
Node 7 (Arbiter Selector): selects RFC 9126 §2 PAR authentication arbiter rule.
Node 8 (Arbiter): RFC 9126 §2 MUST: PAR endpoint must authenticate confidential
clients using same mechanism as token endpoint; unauthenticated acceptance →
REPROCESS.
Node 9 (Formatter): REPROCESS. RFC 9126 §2 MUST: PAR endpoint must authenticate
confidential clients. client_id alone is not authentication. REPROCESS.

## Pair[CB4-3.4-1437]/Reason

Nine-node trace: confidential client accepted unauthenticated at PAR endpoint
violates RFC 9126 §2 MUST → all nodes converge → REPROCESS.

## Pair[CB4-3.4-1437]/Phrasing-B

Verdict REPROCESS. Pipeline: Domain Splitter → PAR client authentication →
RFC 9126 §2 MUST unauthenticated acceptance → REPROCESS at Arbiter → REPROCESS.

---

## Pair[CB4-3.4-1438]/A

Scenario input: an AS issues JWT-formatted access tokens per RFC 9068; the JWT
payload contains `sub`, `aud`, `exp`, `iat`, `jti`, `client_id`, and scope
claims but omits the `iss` claim. Trace the scenario through all nine pipeline
nodes and state the final verdict.

## Pair[CB4-3.4-1438]/B

Node 1 (Domain Splitter): classifies input as JWT-access-token-structure domain
→ security-verification-3.4 flow.
Node 2 (Domain Translator): maps to RFC 9068 §2.2 JWT access token required
claims; `iss` REQUIRED claim context established.
Node 3 (Scheme Planner): plans REQUIRED-claim compliance check against JWT
access token payload.
Node 4 (Domain Processor): confirms JWT access token format claimed; confirms
`iss` claim absent from JWT payload.
Node 5 (Applier): applies RFC 9068 §2.2 REQUIRED standard; `iss` absent =
REQUIRED violation detected.
Node 6 (Result Translator): translates to REPROCESS signal; REQUIRED = MUST-level.
Node 7 (Arbiter Selector): selects RFC 9068 §2.2 JWT access token arbiter rule.
Node 8 (Arbiter): RFC 9068 §2.2 REQUIRED: `iss` must be present in JWT access
tokens; absent → REPROCESS.
Node 9 (Formatter): REPROCESS. RFC 9068 §2.2 REQUIRED: iss is a mandatory claim
in JWT-formatted access tokens. Its absence prevents RS from identifying the
issuing authority. REPROCESS.

## Pair[CB4-3.4-1438]/Reason

Nine-node trace: JWT access token missing REQUIRED `iss` claim (RFC 9068 §2.2)
→ all nodes converge → REPROCESS.

## Pair[CB4-3.4-1438]/Phrasing-B

Verdict REPROCESS. Pipeline: Domain Splitter → JWT access token structure →
RFC 9068 §2.2 REQUIRED `iss` absent → REPROCESS at Arbiter → REPROCESS.

---

## Pair[CB4-3.4-1439]/A

Scenario input: a resource server returns HTTP 401 in response to a request with
an expired Bearer token; the 401 response does not include a `WWW-Authenticate`
header. Trace the scenario through all nine pipeline nodes and state the final
verdict.

## Pair[CB4-3.4-1439]/B

Node 1 (Domain Splitter): classifies input as Bearer-token-error-response domain
→ security-verification-3.4 flow.
Node 2 (Domain Translator): maps to RFC 6750 §3.1 WWW-Authenticate requirements
in 401 responses; MUST-header-inclusion context established.
Node 3 (Scheme Planner): plans MUST header compliance check for 401 responses
from RS.
Node 4 (Domain Processor): confirms RS returned HTTP 401; confirms WWW-Authenticate
header absent from 401 response.
Node 5 (Applier): applies RFC 6750 §3.1 MUST standard; 401 without WWW-Authenticate
= MUST violation detected.
Node 6 (Result Translator): translates to REPROCESS signal; unconditional MUST.
Node 7 (Arbiter Selector): selects RFC 6750 §3.1 WWW-Authenticate arbiter rule.
Node 8 (Arbiter): RFC 6750 §3.1 MUST: RS must include WWW-Authenticate in 401
responses; header absent → REPROCESS.
Node 9 (Formatter): REPROCESS. RFC 6750 §3.1 MUST: resource servers must include
WWW-Authenticate in all 401 responses. A bare 401 without the header is non-
conformant. REPROCESS.

## Pair[CB4-3.4-1439]/Reason

Nine-node trace: RS returns 401 without WWW-Authenticate header violates RFC 6750
§3.1 MUST → all nodes converge → REPROCESS.

## Pair[CB4-3.4-1439]/Phrasing-B

Verdict REPROCESS. Pipeline: Domain Splitter → Bearer 401 response → RFC 6750
§3.1 MUST WWW-Authenticate absent → REPROCESS at Arbiter → REPROCESS.

---

## Pair[CB4-3.4-1440]/A

Scenario input: an OIDC provider supports RP-Initiated Logout; its discovery
document does not include the `end_session_endpoint` field; RPs must use a
hardcoded endpoint URL. Trace the scenario through all nine pipeline nodes and
state the final verdict.

## Pair[CB4-3.4-1440]/B

Node 1 (Domain Splitter): classifies input as OIDC discovery document completeness
domain → security-verification-3.4 flow.
Node 2 (Domain Translator): maps to OIDC Discovery §3 end_session_endpoint
recommendation; RECOMMENDED field context established.
Node 3 (Scheme Planner): plans RECOMMENDED field gap assessment for discovery
document.
Node 4 (Domain Processor): confirms RP-Initiated Logout is supported; confirms
end_session_endpoint absent from discovery document; confirms hardcode workaround
is in use.
Node 5 (Applier): applies OIDC Discovery §3 guidance; RECOMMENDED designation
— absence is a guidance gap, not a MUST violation.
Node 6 (Result Translator): translates to CONCERN signal; no MUST violated.
Node 7 (Arbiter Selector): selects OIDC Discovery §3 RECOMMENDED field arbiter
rule.
Node 8 (Arbiter): OIDC Discovery §3 RECOMMENDED: end_session_endpoint should
be present when logout is supported; absent; no MUST → CONCERN.
Node 9 (Formatter): CONCERN. OIDC Discovery §3 RECOMMENDED: end_session_endpoint
should be published when RP-Initiated Logout is supported. No MUST mandates its
presence. CONCERN not REPROCESS.

## Pair[CB4-3.4-1440]/Reason

Nine-node trace: end_session_endpoint absent from discovery is OIDC Discovery §3
RECOMMENDED gap only → CONCERN at Arbiter → CONCERN.

## Pair[CB4-3.4-1440]/Phrasing-B

Verdict CONCERN. Pipeline: Domain Splitter → OIDC discovery completeness →
OIDC Discovery §3 RECOMMENDED end_session_endpoint absent → CONCERN at Arbiter
→ CONCERN.

---

## Pair[CB4-3.4-1441]/A

Scenario input: an AS supports RFC 7009 token revocation; the AS's RFC 8414
discovery document does not include `revocation_endpoint`. Clients must use
out-of-band documentation to locate the revocation URL. Trace the scenario
through all nine pipeline nodes and state the final verdict.

## Pair[CB4-3.4-1441]/B

Node 1 (Domain Splitter): classifies input as AS-metadata-completeness domain
→ security-verification-3.4 flow.
Node 2 (Domain Translator): maps to RFC 8414 §2 revocation_endpoint metadata
field; OPTIONAL designation context established.
Node 3 (Scheme Planner): plans OPTIONAL-field gap assessment for AS metadata
document.
Node 4 (Domain Processor): confirms token revocation is operationally supported;
confirms revocation_endpoint absent from RFC 8414 discovery document.
Node 5 (Applier): applies RFC 8414 §2 guidance; OPTIONAL designation — absence
is a guidance gap, not a MUST violation.
Node 6 (Result Translator): translates to CONCERN signal; no MUST violated.
Node 7 (Arbiter Selector): selects RFC 8414 §2 metadata completeness arbiter
rule.
Node 8 (Arbiter): RFC 8414 §2 OPTIONAL: revocation_endpoint in discovery is
not REQUIRED; its absence when revocation is supported is a guidance-level gap
→ CONCERN.
Node 9 (Formatter): CONCERN. RFC 8414 §2 OPTIONAL: revocation_endpoint in
AS metadata is not mandated. Its absence is a guidance-level gap. CONCERN not
REPROCESS.

## Pair[CB4-3.4-1441]/Reason

Nine-node trace: revocation_endpoint absent from RFC 8414 metadata is OPTIONAL
guidance gap only → CONCERN at Arbiter → CONCERN.

## Pair[CB4-3.4-1441]/Phrasing-B

Verdict CONCERN. Pipeline: Domain Splitter → AS metadata completeness →
RFC 8414 §2 OPTIONAL revocation_endpoint absent → CONCERN at Arbiter → CONCERN.

---

## Pair[CB4-3.4-1442]/A

Scenario input: a resource server accepts access tokens for write operations
regardless of whether the token's scope includes `write`; a read-only token
with scope `read` is accepted for a write endpoint. Trace the scenario through
all nine pipeline nodes and state the final verdict.

## Pair[CB4-3.4-1442]/B

Node 1 (Domain Splitter): classifies input as resource-server-scope-enforcement
domain → security-verification-3.4 flow.
Node 2 (Domain Translator): maps to RFC 6749 §7 RS scope validation guidance;
no-unconditional-MUST context established.
Node 3 (Scheme Planner): plans scope enforcement gap assessment at RS operation
level.
Node 4 (Domain Processor): confirms RS accepts read-scoped tokens for write
operations; confirms absence of per-operation scope validation.
Node 5 (Applier): applies RFC 6749 §7 guidance; "should verify" language —
guidance-level, no unconditional MUST; real security gap acknowledged.
Node 6 (Result Translator): translates to CONCERN signal; SHOULD-level guidance
gap.
Node 7 (Arbiter Selector): selects RFC 6749 §7 RS scope enforcement arbiter rule.
Node 8 (Arbiter): RFC 6749 §7 guidance: RS should verify token scope is sufficient
for the operation; RS does not enforce scope; no MUST → CONCERN.
Node 9 (Formatter): CONCERN. RFC 6749 §7 guidance: resource servers should
enforce scope at the operation level. No unconditional MUST mandates it.
CONCERN not REPROCESS.

## Pair[CB4-3.4-1442]/Reason

Nine-node trace: RS accepts insufficient-scope tokens is RFC 6749 §7 guidance
gap only → CONCERN at Arbiter → CONCERN.

## Pair[CB4-3.4-1442]/Phrasing-B

Verdict CONCERN. Pipeline: Domain Splitter → RS scope enforcement → RFC 6749
§7 guidance SHOULD gap → CONCERN at Arbiter → CONCERN.
