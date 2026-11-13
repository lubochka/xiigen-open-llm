# Node Pipeline Chain Set Extension 41
# Sub-phase: 3.4 Security and Authorization Model
# Pair ID prefix: CB4-3.4-
# Sequences: 1227–1234 (16 instances — 8 base pairs × 2 phrasings)
# Sources: RFC 6749 §4.1.1, OIDC Core §3.1.3.7.3, RFC 9700 §4.1.3,
#          ASVS V9.2.2, RFC 6749 §3.3, OIDC Core §2,
#          RFC 6750 §2.3, RFC 8414 §2
# Running total after this file: 2468 instances (1234 base pairs)
# Formula check: 120 + (1234 − 60) × 2 = 120 + 2348 = 2468 ✓

---

## Pair[CB4-3.4-1227]/A

Full pipeline trace for the scenario in Pair[CB4-3.4-1219]/A:
Authorization server receives an authorization request without the `response_type`
parameter and infers `response_type=code` from the presence of `code_challenge`,
issuing an authorization code rather than returning error=invalid_request.

## Pair[CB4-3.4-1227]/B

Node 1 (Domain Splitter): Input domain = OAuth 2.0 Authorization Server,
sub-domain = authorization request parameter validation. Security-verification-3.4
flow selected.
Node 2 (Domain Translator): Relevant standard = RFC 6749 §4.1.1 (authorization
request REQUIRED parameters) and §4.1.2.1 (error response for missing REQUIRED
parameters). No OIDC-specific extension applies.
Node 3 (Scheme Planner): Evaluation plan — verify whether the omitted parameter
is classified REQUIRED; verify the RFC-mandated response to a missing REQUIRED
parameter; evaluate whether inference-based handling satisfies or violates MUST.
Node 4 (Domain Processor): RFC 6749 §4.1.1: `response_type` is explicitly listed
as REQUIRED. RFC 6749 §4.1.2.1: if the request is missing a REQUIRED parameter
the AS MUST return an error response with `error=invalid_request`. The AS issued a
code rather than an error: direct violation of the REQUIRED parameter rule.
Node 5 (Applier): Developer-experience argument applied — inference-based handling
tested against the MUST rule: REJECTED. REQUIRED parameters cannot be inferred;
their explicit presence provides the machine-readable signal that the AS must not
fabricate. MUST from §4.1.2.1 applies unconditionally.
Node 6 (Result Translator): MUST violated. RFC 6749 §4.1.1 + §4.1.2.1. No
mitigating exception in any successor RFC.
Node 7 (Arbiter Selector): RFC 6749 §4.1.1 / §4.1.2.1 arbiter selected; MUST
applies; REPROCESS threshold met.
Node 8 (Arbiter): REPROCESS. RFC 6749 §4.1.1 REQUIRED + §4.1.2.1 MUST
error=invalid_request when REQUIRED parameter absent; inference substitutes AS
judgment for explicit required input; MUST violated.
Node 9 (Formatter): Verdict=REPROCESS. Standard=RFC 6749 §4.1.1 + §4.1.2.1.
Remediation: return error=invalid_request when response_type absent; no
inference permitted.

## Pair[CB4-3.4-1227]/Reason

Nine-node trace: response_type absent → RFC 6749 §4.1.1 REQUIRED + §4.1.2.1 MUST
error=invalid_request; inference rejected; REPROCESS.

## Pair[CB4-3.4-1227]/Phrasing-B

Pipeline verdict REPROCESS. Node 4 identifies response_type as REQUIRED per RFC
6749 §4.1.1 and §4.1.2.1 mandates error=invalid_request when a REQUIRED parameter
is absent. Node 5 rejects the inference argument. Nodes 7–8 confirm MUST violation.
REPROCESS.

---

## Pair[CB4-3.4-1228]/A

Full pipeline trace for the scenario in Pair[CB4-3.4-1220]/A:
OIDC provider issues ID tokens containing `iss`, `sub`, `exp`, `iat`, and `nonce`
but omitting the `aud` claim. AS team argues delivery via TLS token endpoint makes
the intended recipient self-evident.

## Pair[CB4-3.4-1228]/B

Node 1 (Domain Splitter): Input domain = OIDC Authorization Server, sub-domain =
ID token mandatory claims. Security-verification-3.4 flow selected.
Node 2 (Domain Translator): Relevant standard = OIDC Core §3.1.3.7.3 (aud MUST
contain client_id). RFC 7519 §4.1.3 (aud claim semantics) provides supporting
context.
Node 3 (Scheme Planner): Verify whether aud is MUST-level in the OIDC ID token
profile; evaluate delivery-channel argument against token-level audience binding
requirement.
Node 4 (Domain Processor): OIDC Core §3.1.3.7.3: the `aud` claim MUST contain
the OAuth 2.0 client_id of the relying party as an audience value. The delivery-
channel argument addresses transport, not token content; aud is required in the
token itself for RP-side validation. Without aud, RP cannot verify intended
recipient; cross-client token confusion attack becomes viable.
Node 5 (Applier): Delivery-channel argument tested: TLS delivery is a transport
control; aud binding is a token-level claim; these are independent and complementary.
REJECTED. MUST is unconditional.
Node 6 (Result Translator): MUST violated. OIDC Core §3.1.3.7.3. Token confusion
attack surface opened.
Node 7 (Arbiter Selector): OIDC Core §3.1.3.7.3 arbiter selected; MUST present;
REPROCESS threshold met.
Node 8 (Arbiter): REPROCESS. OIDC Core §3.1.3.7.3 MUST: aud claim must contain
requesting client's client_id in every ID token; absent aud violates MUST;
delivery-channel argument rejected.
Node 9 (Formatter): Verdict=REPROCESS. Standard=OIDC Core §3.1.3.7.3. Remediation:
include aud=[client_id] in every issued ID token; RP validates aud contains its
client_id.

## Pair[CB4-3.4-1228]/Reason

Nine-node trace: ID token aud absent → OIDC Core §3.1.3.7.3 MUST; delivery-channel
argument rejected; REPROCESS.

## Pair[CB4-3.4-1228]/Phrasing-B

Pipeline verdict REPROCESS. Node 2 identifies OIDC Core §3.1.3.7.3 as the
governing standard. Node 4 establishes aud as MUST-level. Node 5 rejects the TLS
delivery argument. Nodes 7–8 confirm MUST violation. REPROCESS.

---

## Pair[CB4-3.4-1229]/A

Full pipeline trace for the scenario in Pair[CB4-3.4-1221]/A:
Confidential web application client registered with redirect_uri=http://app.example.com/callback.
AS team argues load balancer terminates TLS and the user-facing URL is HTTPS.

## Pair[CB4-3.4-1229]/B

Node 1 (Domain Splitter): Input domain = OAuth 2.0 Authorization Server, sub-domain =
redirect URI scheme validation for non-native clients. Security-verification-3.4
flow selected.
Node 2 (Domain Translator): Relevant standard = RFC 9700 §4.1.3 (redirect URIs for
non-native clients MUST use HTTPS). RFC 6749 §3.1.2 (redirect URI requirements)
provides base context.
Node 3 (Scheme Planner): Evaluate client type (web app, not native app); determine
whether §4.1.3 MUST applies; evaluate load-balancer argument against the scope of
the MUST.
Node 4 (Domain Processor): RFC 9700 §4.1.3: redirect URIs for clients other than
native applications MUST use the HTTPS URI scheme. This client is a confidential
web application — not a native app. The registered URI uses http://. The MUST
applies unconditionally to the registered URI itself.
Node 5 (Applier): Load-balancer argument tested against the same pattern as token
endpoint TLS (ext16/ext43): MUST applies to the URI, not to what terminates the
connection at runtime; http:// registered URI allows code interception before the
load balancer. REJECTED.
Node 6 (Result Translator): MUST violated. RFC 9700 §4.1.3.
Node 7 (Arbiter Selector): RFC 9700 §4.1.3 arbiter selected; MUST present;
REPROCESS threshold met.
Node 8 (Arbiter): REPROCESS. RFC 9700 §4.1.3 MUST HTTPS for non-native-app
redirect URIs; http:// registered redirect_uri violates this; load-balancer
argument rejected.
Node 9 (Formatter): Verdict=REPROCESS. Standard=RFC 9700 §4.1.3. Remediation:
register redirect_uri as https://app.example.com/callback; AS must reject
non-HTTPS registrations for non-native clients.

## Pair[CB4-3.4-1229]/Reason

Nine-node trace: web client http:// redirect_uri → RFC 9700 §4.1.3 MUST HTTPS;
load-balancer argument rejected; REPROCESS.

## Pair[CB4-3.4-1229]/Phrasing-B

Pipeline verdict REPROCESS. Node 2 identifies RFC 9700 §4.1.3. Node 4 confirms
web app is a non-native client subject to MUST HTTPS for redirect URIs. Node 5
rejects the load-balancer argument using the same pattern as token endpoint TLS.
REPROCESS.

---

## Pair[CB4-3.4-1230]/A

Full pipeline trace for the scenario in Pair[CB4-3.4-1222]/A:
Authorization server stores client_secret values as plaintext strings in the client
registry database. AS team argues that database access control through a dedicated
service account with restricted permissions provides adequate protection.

## Pair[CB4-3.4-1230]/B

Node 1 (Domain Splitter): Input domain = Authorization Server credential storage,
sub-domain = client secret at-rest protection. Security-verification-3.4 flow
selected.
Node 2 (Domain Translator): Relevant standard = ASVS V9.2.2 (client secrets MUST
be stored using approved hashing algorithms). CWE-256 (plaintext storage of
credentials) provides supporting context.
Node 3 (Scheme Planner): Verify whether ASVS V9.2.2 applies to client secrets;
evaluate whether access-control argument satisfies or can substitute for the MUST
hashing requirement.
Node 4 (Domain Processor): ASVS V9.2.2 MUST: client credentials (client secrets)
MUST be stored hashed using an approved algorithm (Argon2id, scrypt, bcrypt).
Plaintext storage means any read access to the client table yields all secrets
directly. Hashing ensures that even full database read yields computationally
infeasible mass recovery.
Node 5 (Applier): Access-control argument tested: service-account restriction
reduces probability of breach but does not satisfy the MUST for hashed storage.
Access controls and hashing are complementary and independent requirements.
REJECTED. MUST is unconditional.
Node 6 (Result Translator): MUST violated. ASVS V9.2.2. Mass credential disclosure
on database breach, backup exposure, or insider access.
Node 7 (Arbiter Selector): ASVS V9.2.2 arbiter selected; MUST present; REPROCESS
threshold met.
Node 8 (Arbiter): REPROCESS. ASVS V9.2.2 MUST: client secrets MUST be hashed at
rest; plaintext storage violates this unconditionally; access-control argument
rejected.
Node 9 (Formatter): Verdict=REPROCESS. Standard=ASVS V9.2.2. Remediation:
migrate all client secrets to Argon2id hashes; store hash + salt only; verify
using constant-time comparison on authentication.

## Pair[CB4-3.4-1230]/Reason

Nine-node trace: client secret plaintext storage → ASVS V9.2.2 MUST hash; access-
control argument rejected; REPROCESS.

## Pair[CB4-3.4-1230]/Phrasing-B

Pipeline verdict REPROCESS. Node 2 identifies ASVS V9.2.2. Node 4 establishes
plaintext storage violates the MUST hashing requirement. Node 5 rejects the access-
control argument. Nodes 7–8 confirm MUST violation. REPROCESS.

---

## Pair[CB4-3.4-1231]/A

Full pipeline trace for the scenario in Pair[CB4-3.4-1223]/A:
Service uses client_credentials grant without a scope parameter in the request;
AS issues access token with all five registered scopes. Security team asks whether
this constitutes a violation.

## Pair[CB4-3.4-1231]/B

Node 1 (Domain Splitter): Input domain = OAuth 2.0 Authorization Server, sub-domain =
scope handling in client_credentials grant with absent scope parameter.
Security-verification-3.4 flow selected.
Node 2 (Domain Translator): Relevant standard = RFC 6749 §3.3 (scope definition;
optional in request; AS may use pre-defined default). RFC 9700 §4.2 provides
least-privilege guidance context.
Node 3 (Scheme Planner): Verify whether scope is REQUIRED or OPTIONAL in
client_credentials request; determine whether issuing full registered scope when
scope omitted is permitted; classify the operational gap.
Node 4 (Domain Processor): RFC 6749 §3.3: scope is OPTIONAL in the authorization
request. When omitted: "the authorization server MUST either process the request
using a pre-defined default value or fail the request." Defaulting to the full
registered scope set is an explicitly permitted implementation choice. No MUST
requires minimal scope issuance. The guidance gap: RFC 9700 least-privilege
recommendations and SSDF guidance advise explicit minimal scope specification.
Node 5 (Applier): Least-privilege guidance applied — issuing maximum registered
scope without explicit request is not prohibited; it is a guidance-level gap.
No MUST violation found.
Node 6 (Result Translator): No MUST violation. RFC 6749 §3.3 permits default scope
behavior. Guidance gap on least-privilege practice → CONCERN.
Node 7 (Arbiter Selector): RFC 6749 §3.3 / guidance-level arbiter; REPROCESS
threshold NOT met; CONCERN threshold met.
Node 8 (Arbiter): CONCERN. RFC 6749 §3.3 permits default scope issuance when scope
absent; issuing full registered scope is allowed; least-privilege guidance gap
present; no MUST violated. CONCERN not REPROCESS.
Node 9 (Formatter): Verdict=CONCERN. Standard=RFC 6749 §3.3 / RFC 9700 guidance.
Remediation: client should explicitly request minimum required scope per token
request; AS should document default scope behavior.

## Pair[CB4-3.4-1231]/Reason

Nine-node trace: client_credentials scope absent → RFC 6749 §3.3 explicitly
permits default scope issuance; no MUST violated; least-privilege guidance gap →
CONCERN.

## Pair[CB4-3.4-1231]/Phrasing-B

Pipeline verdict CONCERN. Node 4 confirms RFC 6749 §3.3 explicitly permits AS to
default to pre-established scope when scope is omitted. Node 5 finds no MUST
violation. Node 8 classifies the least-privilege guidance gap as CONCERN. CONCERN
not REPROCESS.

---

## Pair[CB4-3.4-1232]/A

Full pipeline trace for the scenario in Pair[CB4-3.4-1224]/A:
OIDC ID tokens issued with multi-valued aud array omit the `azp` (authorized party)
claim. RP cannot determine the specifically authorized party for this issuance.

## Pair[CB4-3.4-1232]/B

Node 1 (Domain Splitter): Input domain = OIDC Authorization Server, sub-domain =
ID token authorized party identification with multi-audience token. Security-
verification-3.4 flow selected.
Node 2 (Domain Translator): Relevant standard = OIDC Core §2 (azp SHOULD be
present when aud is multi-valued or authorized party differs from sole audience).
Node 3 (Scheme Planner): Determine whether azp is MUST-level or SHOULD-level when
aud is multi-valued; classify the gap appropriately.
Node 4 (Domain Processor): OIDC Core §2: azp SHOULD be present when the aud
claim contains multiple values or when the authorized party differs from the sole
audience member. SHOULD is guidance-level; no unconditional MUST mandates azp
presence. Without azp, RPs in multi-audience contexts cannot determine the
specifically authorized party from the token alone.
Node 5 (Applier): SHOULD obligation applied — guidance-level recommendation, not
MUST. RP ambiguity is a real guidance gap; no unconditional MUST violated.
Node 6 (Result Translator): No MUST violation. OIDC Core §2 SHOULD not met in the
multi-audience case → CONCERN.
Node 7 (Arbiter Selector): OIDC Core §2 SHOULD arbiter; REPROCESS threshold NOT
met; CONCERN threshold met.
Node 8 (Arbiter): CONCERN. OIDC Core §2: azp SHOULD be present when aud is
multi-valued; SHOULD-level; authorized-party ambiguity is a guidance-level gap;
no MUST violated. CONCERN not REPROCESS.
Node 9 (Formatter): Verdict=CONCERN. Standard=OIDC Core §2. Remediation: include
azp claim identifying the authorized client_id when aud contains multiple values.

## Pair[CB4-3.4-1232]/Reason

Nine-node trace: azp absent with multi-valued aud → OIDC Core §2 SHOULD; no MUST;
guidance-level authorized-party ambiguity → CONCERN.

## Pair[CB4-3.4-1232]/Phrasing-B

Pipeline verdict CONCERN. Node 4 identifies OIDC Core §2 azp as SHOULD-level.
Node 5 finds no MUST violation. Node 8 classifies the multi-audience authorized-
party ambiguity as a guidance gap. CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1233]/A

Full pipeline trace for the scenario in Pair[CB4-3.4-1225]/A:
Client submits access token to OIDC UserInfo endpoint via URL query parameter.
Access token appears in full in server-side request log entries.

## Pair[CB4-3.4-1233]/B

Node 1 (Domain Splitter): Input domain = OIDC UserInfo endpoint, sub-domain =
access token transmission method. Security-verification-3.4 flow selected.
Node 2 (Domain Translator): Relevant standard = RFC 6750 §2.3 (query-parameter
Bearer token method: SHOULD NOT, last resort). OIDC Core §5.3.1 (Authorization
header as primary method). ASVS V8.2.2 provides logging risk context.
Node 3 (Scheme Planner): Determine whether RFC 6750 §2.3 SHOULD NOT rises to
MUST NOT; evaluate logging exposure risk; classify verdict.
Node 4 (Domain Processor): RFC 6750 §2.3 defines the query-parameter submission
method and explicitly states it SHOULD NOT be used except as a last resort when
the Authorization header is unavailable. OIDC Core §5.3.1 designates the
Authorization header as the required method. Query-parameter submission causes
the token to appear in server logs, proxy logs, and browser history. RFC 6750
§2.3 does not use MUST NOT — it is an explicit SHOULD NOT.
Node 5 (Applier): SHOULD NOT applied — logs contain full access token, elevating
the concern; no MUST NOT prohibition found; SHOULD NOT = guidance-level.
Node 6 (Result Translator): No MUST violated. RFC 6750 §2.3 SHOULD NOT +
OIDC Core §5.3.1 primary method guidance → CONCERN.
Node 7 (Arbiter Selector): RFC 6750 §2.3 SHOULD NOT arbiter; REPROCESS threshold
NOT met; CONCERN threshold met.
Node 8 (Arbiter): CONCERN. RFC 6750 §2.3 SHOULD NOT (last resort); OIDC Core
§5.3.1 Authorization header is primary; token in logs is a meaningful risk;
no MUST NOT → CONCERN not REPROCESS.
Node 9 (Formatter): Verdict=CONCERN. Standard=RFC 6750 §2.3 + OIDC Core §5.3.1.
Remediation: submit access token exclusively via Authorization: Bearer header;
configure server-side log redaction for Authorization headers.

## Pair[CB4-3.4-1233]/Reason

Nine-node trace: UserInfo query-parameter token submission → RFC 6750 §2.3 SHOULD
NOT; OIDC Core §5.3.1 header primary; token in logs; no MUST NOT → CONCERN.

## Pair[CB4-3.4-1233]/Phrasing-B

Pipeline verdict CONCERN. Node 4 identifies RFC 6750 §2.3 SHOULD NOT. Node 2 adds
OIDC Core §5.3.1 header requirement. Token appearing in logs is a meaningful risk
amplifier. No MUST NOT prohibition found. CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1234]/A

Full pipeline trace for the scenario in Pair[CB4-3.4-1226]/A:
AS metadata document at /.well-known/oauth-authorization-server omits the `issuer`
field. AS team argues the issuer is obvious from the document URL.

## Pair[CB4-3.4-1234]/B

Node 1 (Domain Splitter): Input domain = OAuth 2.0 Authorization Server metadata,
sub-domain = REQUIRED field presence in discovery document. Security-verification-
3.4 flow selected.
Node 2 (Domain Translator): Relevant standard = RFC 8414 §2 (issuer REQUIRED in
AS metadata document) and §3 (issuer MUST match well-known URL prefix). OIDC Core
Discovery §3 provides OIDC-specific parallel.
Node 3 (Scheme Planner): Verify whether issuer is REQUIRED vs. OPTIONAL in RFC
8414 metadata; evaluate inference argument; classify verdict.
Node 4 (Domain Processor): RFC 8414 §2: `issuer` is explicitly listed as a REQUIRED
field in the AS metadata document. RFC 8414 §3 further requires that the issuer
value MUST exactly match the well-known URL prefix used to derive the metadata
endpoint. REQUIRED = MUST present. Absent issuer = non-conformant document.
Without issuer, RP cannot validate that the metadata corresponds to the expected
issuer, enabling metadata injection attacks.
Node 5 (Applier): Inference argument tested — RFC 8414 §3 explicitly defines
that the issuer value must be present and exactly match the prefix, reinforcing
that inference is not a valid substitute. REJECTED. MUST violated.
Node 6 (Result Translator): MUST violated. RFC 8414 §2 REQUIRED + §3 MUST exact
match. Non-conformant metadata enables metadata injection.
Node 7 (Arbiter Selector): RFC 8414 §2 REQUIRED arbiter selected; REPROCESS
threshold met.
Node 8 (Arbiter): REPROCESS. RFC 8414 §2: issuer is REQUIRED in AS metadata
document; absent REQUIRED field = non-conformant; §3 MUST exact match; inference
argument rejected.
Node 9 (Formatter): Verdict=REPROCESS. Standard=RFC 8414 §2 + §3. Remediation:
add issuer field to metadata document with exact value matching the well-known
URL prefix; validate conformance programmatically on each metadata update.

## Pair[CB4-3.4-1234]/Reason

Nine-node trace: AS metadata issuer absent → RFC 8414 §2 REQUIRED + §3 MUST;
inference argument rejected; REPROCESS.

## Pair[CB4-3.4-1234]/Phrasing-B

Pipeline verdict REPROCESS. Node 4 confirms RFC 8414 §2 designates issuer as
REQUIRED. Node 5 rejects the inference argument citing §3's explicit exact-match
requirement. Nodes 7–8 confirm MUST violation. REPROCESS.
