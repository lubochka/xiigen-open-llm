# Node Cross-Source Set — Extension 36
# Sub-phase: 3.4 Security and Authorization Model
# Pair ID prefix: CB4-3.4-
# Sequences: 1131–1140 (20 instances — 10 base pairs × 2 phrasings)
# Covers: Cross-source combinations drawn from ext43 patterns
# Sources combined: IETF RFC 6749/RFC 9101/RFC 9700/RFC 9068/OIDC Core,
#                   CWE-862, ASVS V8.1.1, SSDF PO.1, RFC 7591
# Running total after this file: 2280 instances (1140 base pairs)
# Formula check: 120 + (1140 − 60) × 2 = 120 + 2160 = 2280 ✓

---

## Pair[CB4-3.4-1131]/A

Authorization server audit reveals two structural security gaps in the same
deployment. (1) The authorization endpoint is served at
`http://as.example.com/authorize` — plain HTTP with TLS terminated upstream at
the load balancer (RFC 6749 §3.1). (2) The UserInfo endpoint at `/userinfo`
returns user profile claims (`name`, `email`, `phone`) to any caller who
supplies a `sub` query parameter without a Bearer access token — no OAuth 2.0
token validation is performed (OIDC Core §5.3.1). Both findings are identified
in the same security review cycle.

## Pair[CB4-3.4-1131]/B

REPROCESS. Both findings are REPROCESS-level violations. RFC 6749 §3.1 MUST
requires TLS at the authorization endpoint; the load-balancer argument is
rejected — the MUST applies to the endpoint URI, not the network boundary.
OIDC Core §5.3.1 MUST requires a valid Bearer access token at the UserInfo
endpoint; sub-parameter routing without token validation bypasses the MUST
unconditionally. Two REPROCESS violations compound the urgency; the verdict
is REPROCESS.

## Pair[CB4-3.4-1131]/Reason

RFC 6749 §3.1 MUST TLS (authorization endpoint) + OIDC Core §5.3.1 MUST
Bearer token (UserInfo); both are REPROCESS-level; REPROCESS verdict.

## Pair[CB4-3.4-1131]/Phrasing-B

Verdict REPROCESS. Finding 1: RFC 6749 §3.1 MUST TLS at authorization endpoint
— plain HTTP violates the requirement; load-balancer perimeter argument rejected.
Finding 2: OIDC Core §5.3.1 MUST Bearer token at UserInfo — unauthenticated
access by `sub` query parameter bypasses the MUST and exposes PII without consent.
Both findings are unconditional MUST violations; the combined verdict is REPROCESS.

---

## Pair[CB4-3.4-1132]/A

Security assessment of an OAuth 2.0 / OIDC deployment finds two independent
MUST-level violations. (1) The authorization server accepts JAR request objects
(RFC 9101) carrying `alg: none` in the JOSE header — unsigned request objects
are processed without rejection (RFC 9101 §10.2 MUST NOT). (2) The resource
server validates JWT access token signature, `iss`, `aud`, and `exp` but does
not check the `scope` claim before authorizing requests to scope-protected
endpoints — any valid token, regardless of scope, gains access (CWE-862 /
ASVS V8.1.1). The findings affect different components of the same system.

## Pair[CB4-3.4-1132]/B

REPROCESS. Both violations are REPROCESS-level. RFC 9101 §10.2 MUST NOT:
accepting unsigned JAR request objects nullifies the integrity protection JAR
is designed to provide; authorization parameters can be substituted by an
on-path attacker. CWE-862 / ASVS V8.1.1: the RS performs authentication
(token integrity) but skips authorization (scope enforcement) — the
authorization check is absent. Two REPROCESS violations; verdict is REPROCESS.

## Pair[CB4-3.4-1132]/Reason

RFC 9101 §10.2 MUST NOT alg:none (REPROCESS) + CWE-862 / ASVS V8.1.1 RS
scope enforcement absent (REPROCESS); both REPROCESS-level → REPROCESS.

## Pair[CB4-3.4-1132]/Phrasing-B

Verdict REPROCESS. Finding 1: RFC 9101 §10.2 explicitly prohibits `alg=none`
for signed request objects — unsigned JAR removes integrity protection; AS
accepted what it MUST NOT. Finding 2: CWE-862 — RS validates token authenticity
but performs no authorization check; scope claim is never consulted; any valid
token grants any endpoint. Both findings constitute unconditional MUST / MUST NOT
violations; combined verdict is REPROCESS.

---

## Pair[CB4-3.4-1133]/A

Authorization server design review identifies two guidance-level gaps. (1) Refresh
tokens are issued without recording a `client_id` binding — any authenticated
confidential client can exchange the refresh token (RFC 6749 §10.4 /
RFC 9700 §4.13.2 SHOULD). (2) JWT access tokens do not include an `iat` claim;
the resource server performs no issuance recency check (RFC 9068 §4 SHOULD).
Both gaps are SHOULD-level; the team argues that because there are two findings,
the combined verdict should escalate to REPROCESS.

## Pair[CB4-3.4-1133]/B

CONCERN. Graduation rule: N CONCERNs do NOT escalate to REPROCESS, regardless
of count. Each finding is independently SHOULD-level: RFC 6749 §10.4 / RFC 9700
§4.13.2 SHOULD (refresh binding); RFC 9068 §4 SHOULD (iat recency check). Two
SHOULD-level gaps together remain a CONCERN. No MUST-level violation is present
in either finding; the verdict is CONCERN, not REPROCESS.

## Pair[CB4-3.4-1133]/Reason

RFC 6749 §10.4 / RFC 9700 §4.13.2 SHOULD (refresh binding → CONCERN) + RFC 9068
§4 SHOULD (iat recency → CONCERN); graduation rule: 2 CONCERNs ≠ REPROCESS → CONCERN.

## Pair[CB4-3.4-1133]/Phrasing-B

Verdict CONCERN. Both findings are SHOULD-level guidance gaps: (1) RFC 9700
§4.13.2 SHOULD bind refresh token to client — no unconditional MUST; (2) RFC 9068
§4 SHOULD include `iat` and validate recency — no unconditional MUST. The team's
argument that two CONCERNs should escalate is rejected by the graduation rule: N
CONCERNs do NOT escalate to REPROCESS regardless of count. Both gaps should be
remediated; the verdict is CONCERN.

---

## Pair[CB4-3.4-1134]/A

Security review of an authorization server flags three separate guidance-level
gaps found in the same system: (1) `token_endpoint_auth_method` is not enforced
— clients can authenticate using a method different from their registered method
(RFC 7591 §2 SHOULD); (2) refresh tokens are issued without `client_id` binding
— cross-client use is possible for authenticated clients (RFC 9700 §4.13.2
SHOULD); (3) JWT access tokens lack `iat` claim; RS performs no issuance recency
check (RFC 9068 §4 SHOULD). The reviewer argues that three concurrent SHOULD gaps
in the same system must collectively constitute a REPROCESS-level finding.

## Pair[CB4-3.4-1134]/B

CONCERN. Graduation rule applies unconditionally: N CONCERNs do NOT escalate to
REPROCESS regardless of count. All three findings are SHOULD-level: RFC 7591 §2
SHOULD, RFC 9700 §4.13.2 SHOULD, RFC 9068 §4 SHOULD. Three SHOULD gaps in the
same system remain three CONCERNs — the combined verdict is CONCERN, not
REPROCESS. The reviewer's escalation argument is rejected; only a MUST-level
violation can produce REPROCESS.

## Pair[CB4-3.4-1134]/Reason

Three SHOULD gaps (RFC 7591 §2 + RFC 9700 §4.13.2 + RFC 9068 §4); graduation
rule: N CONCERNs do NOT escalate to REPROCESS regardless of count → CONCERN.

## Pair[CB4-3.4-1134]/Phrasing-B

Verdict CONCERN. Three independent SHOULD-level gaps are identified, but the
graduation rule is unconditional: N CONCERNs cannot escalate to REPROCESS
regardless of how many CONCERNs accumulate. RFC 7591 §2, RFC 9700 §4.13.2,
and RFC 9068 §4 all use SHOULD; none provides an unconditional MUST. The
combined verdict for any number of SHOULD gaps is CONCERN; REPROCESS requires
at least one MUST-level violation.

---

## Pair[CB4-3.4-1135]/A

Development team has no formal security training program — the organization
has not established the security-awareness curriculum required by SSDF PO.1
(CONCERN on its own). Post-incident review traces a production incident to a
resource server that grants full access to scope-protected endpoints without
checking the `scope` claim in JWT access tokens. The developers responsible for
the RS were unaware that scope enforcement is a mandatory authorization step
in the OAuth JWT model (CWE-862 / ASVS V8.1.1). The scope-validation gap is
directly traceable to the PO.1 training absence.

## Pair[CB4-3.4-1135]/B

REPROCESS. Primary: CWE-862 / ASVS V8.1.1 — RS scope validation absent is a
MUST-level violation; REPROCESS regardless of PO.1 status. PO.1 escalation path
also confirms REPROCESS independently: PO.1 absence alone = CONCERN; PO.1
absence + traceable production MUST violation = REPROCESS. Both paths arrive at
REPROCESS. The combined verdict is REPROCESS.

## Pair[CB4-3.4-1135]/Reason

CWE-862 / ASVS V8.1.1 MUST scope enforcement (primary REPROCESS) + SSDF PO.1
absence with traceable production MUST violation (PO.1 escalation path also
REPROCESS); two independent paths to REPROCESS.

## Pair[CB4-3.4-1135]/Phrasing-B

Verdict REPROCESS. The RS scope-validation gap is a CWE-862 / ASVS V8.1.1
MUST violation — REPROCESS on its own. The PO.1 escalation rule provides a
second independent path: PO.1 absence alone is CONCERN; PO.1 absence combined
with a traceable production MUST violation escalates to REPROCESS. The direct
traceability from training absence to the scope-enforcement gap activates the
escalation. Both paths confirm REPROCESS.

---

## Pair[CB4-3.4-1136]/A

OIDC provider deployment exhibits two findings with different normative weights.
(1) The UserInfo endpoint `/userinfo` accepts requests without an
`Authorization: Bearer` header — any caller with a `sub` value retrieves user
claims without token authorization (OIDC Core §5.3.1 MUST). (2) JWT access
tokens do not include an `iat` claim; the RS does not check issuance recency
(RFC 9068 §4 SHOULD). The operations team asks whether the CONCERN on the `iat`
gap mitigates the severity of the UserInfo finding.

## Pair[CB4-3.4-1136]/B

REPROCESS. The UserInfo finding is a MUST violation (OIDC Core §5.3.1) and
produces REPROCESS unconditionally. The `iat` finding is a SHOULD gap (CONCERN).
REPROCESS overrides CONCERN: the presence of a REPROCESS-level violation
determines the final verdict regardless of accompanying CONCERN-level findings.
The CONCERN does not mitigate the REPROCESS; the verdict is REPROCESS.

## Pair[CB4-3.4-1136]/Reason

OIDC Core §5.3.1 MUST Bearer token at UserInfo (REPROCESS) + RFC 9068 §4 SHOULD
iat recency (CONCERN); REPROCESS overrides CONCERN — verdict is REPROCESS.

## Pair[CB4-3.4-1136]/Phrasing-B

Verdict REPROCESS. Finding 1 (UserInfo without Bearer token) is a MUST violation
— OIDC Core §5.3.1 MUST is unconditional; REPROCESS. Finding 2 (`iat` absent)
is a SHOULD gap — RFC 9068 §4 SHOULD; CONCERN. The CONCERN accompanying a
REPROCESS-level finding does not reduce the overall verdict; REPROCESS is the
override verdict whenever any finding is MUST-level. The operations team's
framing is rejected: CONCERNs do not mitigate REPROCESS findings.

---

## Pair[CB4-3.4-1137]/A

Token endpoint audit across three dimensions: (1) AS accepts token requests
without `grant_type` parameter, defaulting to authorization_code based on the
presence of `code` (RFC 6749 §4.1.3 REQUIRED / §5.2). (2) AS does not enforce
`token_endpoint_auth_method` binding for confidential clients — any valid auth
method accepted regardless of registration (RFC 7591 §2 SHOULD). (3) Refresh
tokens are issued without `client_id` binding — cross-client use permitted for
authenticated clients (RFC 9700 §4.13.2 SHOULD). The team notes that one finding
is REPROCESS and two are CONCERN, and asks whether CONCERNs carry any weight
toward the verdict when a REPROCESS is also present.

## Pair[CB4-3.4-1137]/B

REPROCESS. The `grant_type` finding is a MUST violation (RFC 6749 §4.1.3
REQUIRED; §5.2 error=invalid_request required) and produces REPROCESS
unconditionally. REPROCESS overrides any number of concurrent CONCERNs: the
auth-method CONCERN and refresh-binding CONCERN do not contribute to or reduce
the REPROCESS verdict. The final verdict is determined by the highest-severity
finding. The team's question is answered: CONCERNs have no weight against an
existing REPROCESS — they are recorded as accompanying guidance gaps only.

## Pair[CB4-3.4-1137]/Reason

RFC 6749 §4.1.3 REQUIRED / §5.2 (grant_type absent → REPROCESS) overrides
RFC 7591 §2 SHOULD (CONCERN) + RFC 9700 §4.13.2 SHOULD (CONCERN); REPROCESS
dominates regardless of CONCERN count.

## Pair[CB4-3.4-1137]/Phrasing-B

Verdict REPROCESS. The `grant_type` absence is a MUST-level violation:
RFC 6749 §4.1.3 classifies the parameter as REQUIRED; §5.2 requires
`error=invalid_request`; the AS's inference behavior violates this
unconditionally. The two SHOULD-level gaps (auth-method binding and refresh
client binding) are recorded as CONCERN-level accompaniments but do not affect
the verdict. REPROCESS overrides all concurrent CONCERNs; the final verdict is
REPROCESS with two accompanying CONCERNs.

---

## Pair[CB4-3.4-1138]/A

Authorization server security scan returns two findings with different normative
weights. (1) The authorization endpoint is served over plain HTTP —
`http://as.example.com/authorize` — with TLS terminated at the upstream load
balancer (RFC 6749 §3.1 MUST TLS). (2) `token_endpoint_auth_method` is not
enforced for confidential clients — a client registered as `client_secret_post`
can authenticate using `client_secret_basic` and still receive a token
(RFC 7591 §2 SHOULD). The team argues that the CONCERN on the auth-method gap
offsets the HTTP authorization endpoint by showing that authentication itself
is still verified.

## Pair[CB4-3.4-1138]/B

REPROCESS. The HTTP authorization endpoint finding is a MUST violation
(RFC 6749 §3.1) — REPROCESS. The auth-method finding is a SHOULD gap (CONCERN).
REPROCESS overrides CONCERN unconditionally; the auth-method CONCERN cannot
offset or reduce a MUST violation. The argument that authentication being present
reduces the transport-security MUST is rejected: the transport requirement is
independent of authentication correctness — both must be satisfied simultaneously.
Verdict: REPROCESS.

## Pair[CB4-3.4-1138]/Reason

RFC 6749 §3.1 MUST TLS at authorization endpoint (REPROCESS) overrides RFC 7591
§2 SHOULD auth-method enforcement (CONCERN); REPROCESS verdict; offset argument
rejected.

## Pair[CB4-3.4-1138]/Phrasing-B

Verdict REPROCESS. RFC 6749 §3.1 MUST TLS at the authorization endpoint is
unconditional — the load-balancer termination argument is rejected; the endpoint
URI itself must use TLS. The auth-method enforcement gap is SHOULD-level (CONCERN)
and cannot offset a MUST violation. The team's offset framing is rejected: transport
security (§3.1) and authentication method enforcement (RFC 7591 §2) are independent
requirements; compliance with one does not mitigate non-compliance with the other.
REPROCESS overrides the CONCERN.

---

## Pair[CB4-3.4-1139]/A

Full-stack OAuth 2.0 / OIDC implementation review across all ext43 security domains
returns the following: (1) Token endpoint validates presence of `grant_type` on every
request; requests missing the parameter receive `error=invalid_request` per RFC 6749
§4.1.3 and §5.2. (2) Authorization server enforces JAR request object signing —
`alg: none` and all non-allowlisted algorithms are rejected per RFC 9101 §10.2.
(3) Refresh tokens are stored with `client_id` binding; exchanges from mismatched
clients receive `invalid_grant`. (4) JWT access tokens include `iat`; RS validates
`iat` for recency within a 30-second clock-skew window. (5) UserInfo endpoint requires
`Authorization: Bearer` header; unauthenticated requests receive HTTP 401. (6) AS
enforces `token_endpoint_auth_method` binding — mismatched method returns
`invalid_client`. (7) RS checks `scope` claim before authorizing every endpoint; 403
for scope mismatch. (8) Authorization endpoint served exclusively over HTTPS (TLS 1.3).

## Pair[CB4-3.4-1139]/B

PASS. All ext43 security domains are correctly implemented. Every MUST-level
requirement is satisfied: RFC 6749 §4.1.3 (grant_type REQUIRED enforced),
RFC 9101 §10.2 (alg:none rejected), OIDC Core §5.3.1 (UserInfo Bearer required),
RFC 6749 §3.1 (authorization endpoint HTTPS), CWE-862 / ASVS V8.1.1 (scope
enforcement present). All SHOULD-level guidance items are also met: RFC 9700
§4.13.2 (refresh token client binding), RFC 9068 §4 (iat recency check),
RFC 7591 §2 (auth-method enforcement). No violations across any reviewed domain.

## Pair[CB4-3.4-1139]/Reason

All eight ext43 domains correctly implemented; all MUST-level violations absent;
all SHOULD-level guidance items also met → PASS.

## Pair[CB4-3.4-1139]/Phrasing-B

Verdict PASS. The implementation satisfies every requirement across all ext43 domains:
MUST-level — `grant_type` validated, JAR alg:none rejected, UserInfo Bearer enforced,
authorization endpoint on HTTPS, scope enforced at RS. SHOULD-level — refresh token
client binding present, `iat` recency check active, `token_endpoint_auth_method`
enforced. No violations detected in any reviewed dimension; the verdict is PASS.

---

## Pair[CB4-3.4-1140]/A

Authorization server review identifies two SHOULD-level gaps: (1) The AS does not
enforce `token_endpoint_auth_method` binding for confidential clients — any valid
authentication method accepted regardless of the method recorded at registration
(RFC 7591 §2 SHOULD). (2) JWT access tokens issued by the AS do not include an
`iat` claim; the RS performs no issuance recency validation (RFC 9068 §4 SHOULD).
The reviewer notes the combination of two SHOULD gaps and asks whether, taken
together, they should be escalated to REPROCESS given the cumulative weakening
of the token security posture.

## Pair[CB4-3.4-1140]/B

CONCERN. Graduation rule applies: N CONCERNs do NOT escalate to REPROCESS
regardless of how many accumulate or how they are combined. Each finding is
independently SHOULD-level: RFC 7591 §2 SHOULD (auth-method enforcement);
RFC 9068 §4 SHOULD (iat recency check). The reviewer's cumulative-weakening
argument is rejected — normative weight is determined per-finding, not
aggregated across findings. Two SHOULD gaps remain two CONCERNs; the combined
verdict is CONCERN, not REPROCESS.

## Pair[CB4-3.4-1140]/Reason

RFC 7591 §2 SHOULD (auth-method enforcement → CONCERN) + RFC 9068 §4 SHOULD
(iat recency → CONCERN); graduation rule: N CONCERNs do NOT escalate to
REPROCESS → CONCERN; cumulative-weakening argument rejected.

## Pair[CB4-3.4-1140]/Phrasing-B

Verdict CONCERN. Both gaps are SHOULD-level: RFC 7591 §2 SHOULD for
`token_endpoint_auth_method` enforcement and RFC 9068 §4 SHOULD for `iat`
recency validation. The graduation rule is unconditional — N CONCERNs do not
escalate to REPROCESS regardless of the count or the framing of cumulative
impact. Normative weight is assessed per finding; REPROCESS requires at least
one finding with a MUST-level anchor. Two SHOULD gaps = CONCERN.
