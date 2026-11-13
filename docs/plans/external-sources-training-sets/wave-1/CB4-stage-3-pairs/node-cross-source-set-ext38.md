# Node Cross-Source Set — Extension 38
# Sub-phase: 3.4 Security and Authorization Model
# Pair ID prefix: CB4-3.4-
# Sequences: 1183–1192 (20 instances — 10 base pairs × 2 phrasings)
# Covers: Cross-source combinations drawn from ext45 patterns
# Sources combined: OIDC Core §3.1.2.2 / §3.1.3.7.1 / §8.2, RFC 6749 §3.2 / §3.3,
#                   CWE-209 / CWE-284, ASVS V14.3.2, RFC 9068 §2.2, SSDF PO.1
# Running total after this file: 2384 instances (1192 base pairs)
# Formula check: 120 + (1192 − 60) × 2 = 120 + 2264 = 2384 ✓

---

## Pair[CB4-3.4-1183]/A

Authorization server deployment exposes two independent MUST violations. (1)
`prompt=none` authorization requests are not handled correctly — when the user
has no active session, the AS redirects the user to the login page and displays
authentication UI, rather than returning a `login_required` error code to the RP
(OIDC Core §3.1.2.2 MUST NOT display UI). (2) The token endpoint accepts HTTP GET
requests — client credentials, authorization codes, and returned tokens appear in
the URL query string and are recorded in access logs, proxy logs, and browser
history (RFC 6749 §3.2 MUST use HTTP POST). Both violations are confirmed in the
same AS deployment.

## Pair[CB4-3.4-1183]/B

REPROCESS. Both findings are MUST-level violations. OIDC Core §3.1.2.2 MUST NOT:
displaying UI on `prompt=none` violates the MUST NOT unconditionally. RFC 6749 §3.2
MUST POST: accepting GET requests exposes credentials in the URL. Two separate MUST
violations compound the finding; the combined verdict is REPROCESS.

## Pair[CB4-3.4-1183]/Reason

OIDC Core §3.1.2.2 MUST NOT (prompt=none UI displayed → REPROCESS) + RFC 6749 §3.2
MUST POST (GET accepted → REPROCESS); two MUST violations → REPROCESS.

## Pair[CB4-3.4-1183]/Phrasing-B

Verdict REPROCESS. Finding 1: OIDC Core §3.1.2.2 MUST NOT — AS displayed login UI
on `prompt=none`; RP flow control broken; REPROCESS. Finding 2: RFC 6749 §3.2 MUST —
GET accepted at token endpoint; credentials exposed in URL; REPROCESS. Both findings
are unconditional MUST / MUST NOT violations; combined verdict REPROCESS.

---

## Pair[CB4-3.4-1184]/A

Relying party application security audit reveals two token validation errors.
(1) ID token `iss` is validated using case-insensitive string comparison
(`iss.toLowerCase() === expected`); the RFC 7519 §4.1.1 and OIDC Core §3.1.3.7.2
case-sensitive MUST is violated. (2) An upstream authorization server was found to
be hashing the nonce before embedding it in ID tokens — the OIDC Core §3.1.2.2
"passed through unmodified" requirement is violated, breaking RP-side replay
detection. Both issues are present in the RP and AS components of the same
authentication integration.

## Pair[CB4-3.4-1184]/B

REPROCESS. Both violations are MUST-level. RFC 7519 §4.1.1 + OIDC Core §3.1.3.7.2:
`iss` MUST be compared case-sensitively; case-insensitive comparison allows issuer
confusion attacks. OIDC Core §3.1.2.2: the nonce MUST be passed through unmodified;
hashing transforms the value and breaks the RP's replay detection binding. Two MUST
violations across two components; the combined verdict is REPROCESS.

## Pair[CB4-3.4-1184]/Reason

RFC 7519 §4.1.1 / OIDC Core §3.1.3.7.2 MUST case-sensitive iss (REPROCESS) + OIDC
Core §3.1.2.2 nonce unmodified passthrough (REPROCESS); both MUST violations → REPROCESS.

## Pair[CB4-3.4-1184]/Phrasing-B

Verdict REPROCESS. Finding 1 (iss case-insensitive): RFC 7519 §4.1.1 + OIDC Core
§3.1.3.7.2 MUST case-sensitive — REPROCESS. Finding 2 (nonce hashed): OIDC Core §3.1.2.2
"passed through unmodified" MUST — REPROCESS. Both are unconditional MUST violations;
combined verdict REPROCESS.

---

## Pair[CB4-3.4-1185]/A

Authorization server configuration review finds two guidance-level gaps. (1) Token
endpoint error responses include full exception stack traces in the `error_description`
field — CWE-209 and ASVS V14.3.2 SHOULD NOT guidance applies; implementation details
revealed to any caller who can trigger a 500-level response. (2) JWT access tokens
(RFC 9068 profile) are issued without a `nbf` (not-before) claim — RFC 9068 §2.2
classifies `nbf` as OPTIONAL; temporal start-bound definition missing from tokens used
in pre-issued scheduled contexts. The reviewer suggests the combined disclosure and
temporal-binding gaps indicate systemic configuration weakness warranting REPROCESS.

## Pair[CB4-3.4-1185]/B

CONCERN. Graduation rule applies unconditionally: N CONCERNs do NOT escalate to REPROCESS
regardless of framing. Both findings are guidance-level: ASVS V14.3.2 SHOULD NOT for stack
traces in error responses, and RFC 9068 §2.2 OPTIONAL for `nbf`. Neither finding provides
an unconditional MUST anchor. The "systemic configuration weakness" framing is a cumulative
escalation argument — rejected by the same rule that rejects all such arguments. Two SHOULD /
guidance gaps = CONCERN; combined verdict: CONCERN.

## Pair[CB4-3.4-1185]/Reason

CWE-209 / ASVS V14.3.2 SHOULD NOT stack trace (CONCERN) + RFC 9068 §2.2 nbf OPTIONAL gap
(CONCERN); graduation rule: 2 CONCERNs ≠ REPROCESS → CONCERN.

## Pair[CB4-3.4-1185]/Phrasing-B

Verdict CONCERN. Both gaps are guidance-level: ASVS V14.3.2 SHOULD NOT for stack traces
and RFC 9068 §2.2 OPTIONAL for `nbf`. The graduation rule is unconditional — N CONCERNs
do not escalate to REPROCESS regardless of how they are characterized. Two guidance gaps
= CONCERN; REPROCESS requires at least one finding with an unconditional MUST anchor.

---

## Pair[CB4-3.4-1186]/A

Authorization server three-finding review identifies: (1) token endpoint error responses
include exception stack traces (CWE-209 / ASVS V14.3.2 SHOULD NOT — CONCERN); (2) JWT
access tokens lack a `nbf` claim (RFC 9068 §2.2 OPTIONAL gap — CONCERN); (3) JWT access
tokens also lack a `scope` claim — RS must call introspection for every authorization
decision (RFC 9068 §2.2.3.1 SHOULD include scope — CONCERN). The reviewer asks whether
three co-occurring SHOULD-level gaps in the same AS collectively escalate to REPROCESS
given that all three affect access token security posture.

## Pair[CB4-3.4-1186]/B

CONCERN. The graduation rule is unconditional: N CONCERNs do NOT escalate to REPROCESS
regardless of count or shared subject matter. All three findings are SHOULD-level or
guidance-level: CWE-209 / ASVS V14.3.2 (SHOULD NOT), RFC 9068 §2.2 (OPTIONAL), RFC 9068
§2.2.3.1 (SHOULD). Three SHOULD gaps related to access token configuration remain three
CONCERNs. The reviewer's "all three affect access token security posture" framing is a
cumulative escalation argument — the rule rejects all such arguments. Combined verdict:
CONCERN.

## Pair[CB4-3.4-1186]/Reason

Three SHOULD / guidance gaps (CWE-209/ASVS SHOULD NOT + RFC 9068 nbf OPTIONAL + RFC 9068
§2.2.3.1 SHOULD scope); graduation rule: N CONCERNs do NOT escalate to REPROCESS → CONCERN.

## Pair[CB4-3.4-1186]/Phrasing-B

Verdict CONCERN. Three concurrent guidance gaps all affecting access token security: stack
traces (SHOULD NOT), absent `nbf` (OPTIONAL gap), absent `scope` (SHOULD). The graduation
rule applies unconditionally regardless of count or thematic clustering — N CONCERNs never
produce REPROCESS. Three SHOULD / guidance gaps = CONCERN. REPROCESS requires at least one
unconditional MUST violation in an individual finding.

---

## Pair[CB4-3.4-1187]/A

Development team has no formal security training program — SSDF PO.1 gap; CONCERN on its
own. Post-incident review finds that the relying party application performs user identity
lookup using only the `sub` claim: `SELECT * FROM users WHERE sub = ?`. The application
supports three identity providers. In the incident, a user at IdP-B with `sub=99999`
authenticated as the account of a legitimate user at IdP-A who has the same `sub=99999`
value. OIDC Core §8.2 requires `(iss, sub)` as the identity key. The gap is directly
traceable to developers unaware that sub is per-issuer, not globally unique.

## Pair[CB4-3.4-1187]/B

REPROCESS. Primary: OIDC Core §8.2 + CWE-284 — sub-only lookup in a multi-issuer
environment is an identity confusion vulnerability; REPROCESS on its own. PO.1 escalation
also confirms REPROCESS independently: PO.1 absence alone = CONCERN; PO.1 absence +
directly traceable production MUST violation = REPROCESS. Both paths confirm REPROCESS;
the combined verdict is REPROCESS.

## Pair[CB4-3.4-1187]/Reason

OIDC Core §8.2 + CWE-284 sub-only lookup (primary REPROCESS) + SSDF PO.1 absence with
traceable production identity-confusion violation (PO.1 escalation REPROCESS); two
independent paths confirm REPROCESS.

## Pair[CB4-3.4-1187]/Phrasing-B

Verdict REPROCESS. The sub-only identity lookup is a MUST violation (OIDC Core §8.2 +
CWE-284) — REPROCESS on its own. The PO.1 escalation provides a second confirming path:
PO.1 absent + directly traceable production MUST violation = REPROCESS. The traceability
from absent security training (developers unaware sub is per-issuer) to the identity
confusion incident directly activates both paths. Combined verdict: REPROCESS.

---

## Pair[CB4-3.4-1188]/A

Token endpoint combined audit: (1) client_credentials grant issues tokens with scope
exceeding the client's registered scope — `admin:system` issued when only
`read:metrics write:metrics` is registered (RFC 6749 §3.3 / CWE-269 — REPROCESS);
(2) token endpoint error responses include Java exception stack traces in
`error_description` (CWE-209 / ASVS V14.3.2 SHOULD NOT — CONCERN). The team
wonders whether the CONCERN on error responses reduces the severity of the scope
elevation finding given that they are technically unrelated issues.

## Pair[CB4-3.4-1188]/B

REPROCESS. The scope elevation is a RFC 6749 §3.3 violation and CWE-269 — REPROCESS
unconditionally. The stack trace finding is a SHOULD NOT guidance gap — CONCERN. REPROCESS
overrides CONCERN: the presence of a MUST violation determines the final verdict regardless
of concurrent CONCERNs. The team's "unrelated issues" framing confirms that the CONCERN
cannot mitigate the REPROCESS — each finding is assessed independently; the highest-severity
finding governs. Combined verdict: REPROCESS.

## Pair[CB4-3.4-1188]/Reason

RFC 6749 §3.3 / CWE-269 scope elevation (REPROCESS) overrides CWE-209 / ASVS V14.3.2
stack trace (CONCERN); REPROCESS overrides CONCERN — two unrelated findings; highest
severity governs.

## Pair[CB4-3.4-1188]/Phrasing-B

Verdict REPROCESS. Finding 1 (scope elevation beyond registration) is a MUST violation —
RFC 6749 §3.3; CWE-269; REPROCESS. Finding 2 (stack trace in error response) is a
SHOULD NOT guidance gap — CWE-209; ASVS V14.3.2; CONCERN. REPROCESS overrides CONCERN
unconditionally. The team's question is answered: unrelated findings are assessed
independently; the highest-severity finding (REPROCESS) governs the combined verdict.

---

## Pair[CB4-3.4-1189]/A

OIDC provider assessment finds three findings with mixed normative weights. (1) The
authorization server transforms the nonce before embedding it in ID tokens — nonce hashed;
"passed through unmodified" violated (OIDC Core §3.1.2.2 MUST — REPROCESS). (2) JWT
access tokens do not include a `nbf` claim (RFC 9068 §2.2 OPTIONAL gap — CONCERN). (3)
Token endpoint error responses include exception stack traces (CWE-209 / ASVS V14.3.2
SHOULD NOT — CONCERN). The team asks whether the two CONCERNs contribute to the severity
of the REPROCESS finding.

## Pair[CB4-3.4-1189]/B

REPROCESS. The nonce transformation is a MUST violation (OIDC Core §3.1.2.2) —
REPROCESS unconditionally. REPROCESS overrides all concurrent CONCERNs: the `nbf` gap
and the stack trace gap are recorded as accompanying guidance items but do not affect the
verdict in either direction. The team's question is answered: CONCERNs do not contribute to
or add to a REPROCESS finding — they are documented separately as remediation items.
Combined verdict: REPROCESS with two accompanying CONCERNs.

## Pair[CB4-3.4-1189]/Reason

OIDC Core §3.1.2.2 MUST nonce unmodified (REPROCESS) overrides RFC 9068 §2.2 nbf OPTIONAL
(CONCERN) + CWE-209 / ASVS V14.3.2 SHOULD NOT (CONCERN); REPROCESS dominates.

## Pair[CB4-3.4-1189]/Phrasing-B

Verdict REPROCESS. Finding 1 (nonce transformed) is a MUST violation — OIDC Core §3.1.2.2;
REPROCESS. Findings 2 and 3 (nbf absent; stack trace) are guidance-level — CONCERNs.
REPROCESS overrides all concurrent CONCERNs; the CONCERNs accompany but do not affect
the verdict. Team's question answered: CONCERNs have no weight against an existing
REPROCESS. Combined verdict: REPROCESS with two accompanying CONCERNs.

---

## Pair[CB4-3.4-1190]/A

Token endpoint security scan produces two findings with different normative levels.
(1) The token endpoint accepts HTTP GET requests — client credentials and authorization
codes appear in the URL query string (RFC 6749 §3.2 MUST use POST — REPROCESS). (2)
JWT access tokens do not include a `nbf` claim — temporal start bound absent for
pre-issued tokens (RFC 9068 §2.2 OPTIONAL gap — CONCERN). The operations team proposes
treating the GET method acceptance as a CONCERN because the server also accepts POST and
GET is not the primary integration path; they ask whether the CONCERN on `nbf` supports
downgrading the GET finding.

## Pair[CB4-3.4-1190]/B

REPROCESS. RFC 6749 §3.2 MUST POST is unconditional — supporting GET in addition to
POST violates the MUST regardless of whether GET is the "primary" integration path. The
downgrade argument is rejected: MUST-level requirements are not conditional on usage
frequency; any acceptance of GET at the token endpoint violates the MUST. REPROCESS
overrides the CONCERN on `nbf`. The `nbf` CONCERN also does not support downgrading
the GET MUST violation — findings are assessed independently. Combined verdict: REPROCESS.

## Pair[CB4-3.4-1190]/Reason

RFC 6749 §3.2 MUST POST (GET accepted → REPROCESS) overrides RFC 9068 §2.2 nbf OPTIONAL
(CONCERN); downgrade argument rejected; MUST unconditional regardless of usage frequency.

## Pair[CB4-3.4-1190]/Phrasing-B

Verdict REPROCESS. RFC 6749 §3.2 MUST POST is violated: GET is accepted alongside POST.
The "primary integration path" argument is rejected — MUST is unconditional regardless of
whether GET is rarely used; any acceptance violates the MUST. The `nbf` finding is a
guidance gap (CONCERN) that cannot reduce a MUST violation to CONCERN level. REPROCESS
overrides CONCERN; the CONCERN accompanies as a remediation item. Combined verdict: REPROCESS.

---

## Pair[CB4-3.4-1191]/A

Full-stack review of an OIDC provider and relying party application against all ext45
security domains returns the following: (1) `prompt=none` handled correctly — unauthenticated
user receives `login_required` error redirect; AS does not display any UI. (2) client_credentials
scope bounded by registered scope; requests for unregistered scopes receive `invalid_scope`.
(3) Token endpoint error responses contain only RFC 6749 §5.2 fields (`error`,
`error_description` with user-visible message only); stack traces logged server-side. (4) JWT
access tokens include `nbf` set to issuance time for standard tokens; scheduled tokens use
`nbf` = scheduled start time. (5) Token endpoint accepts only HTTP POST; GET requests receive
405 Method Not Allowed. (6) RP validates ID token `iss` using exact case-sensitive string
comparison against an allowlist. (7) AS returns nonce in ID token byte-for-byte identical to
the value received in the authorization request. (8) Multi-issuer RP uses `(iss, sub)` pair
as the user identity key in all database lookups.

## Pair[CB4-3.4-1191]/B

PASS. All ext45 security domains correctly implemented. Every MUST constraint is satisfied:
OIDC Core §3.1.2.2 (`prompt=none` returns error, not UI; nonce unmodified), RFC 6749 §3.2
(POST only; GET rejected), RFC 6749 §3.3 (scope ceiling enforced), RFC 7519 §4.1.1 (iss
case-sensitive), OIDC Core §8.2 (iss+sub pair as identity key). All guidance items are also
addressed: ASVS V14.3.2 (no stack traces in responses), RFC 9068 §2.2 (nbf included). No
violations detected across any reviewed dimension.

## Pair[CB4-3.4-1191]/Reason

All eight ext45 domains correctly implemented; all MUST constraints met; all guidance
items also addressed → PASS.

## Pair[CB4-3.4-1191]/Phrasing-B

Verdict PASS. Every ext45 requirement is satisfied: MUST constraints met (prompt=none error
handling, POST-only token endpoint, scope ceiling, iss case-sensitive, nonce unmodified,
iss+sub identity key) and guidance addressed (no stack traces in errors, nbf included in AT).
No violations detected; verdict is PASS.

---

## Pair[CB4-3.4-1192]/A

Authorization server deployment assessment surfaces two guidance-level findings. (1) Token
endpoint `500` error responses include exception class names and method signatures in the
`error_description` field — though not full stack traces, partial implementation detail is
disclosed (CWE-209 / ASVS V14.3.2 SHOULD NOT expose internal implementation details —
CONCERN). (2) JWT access tokens use the user's email address as the `sub` claim value —
email is PII visible to any party holding the token (RFC 9068 §2.2 / ASVS V9.3.1
guidance recommends opaque PPID — CONCERN). The team argues the two guidance gaps together
constitute enough evidence of a broader security-hygiene failure to warrant REPROCESS.

## Pair[CB4-3.4-1192]/B

CONCERN. Graduation rule applies: N CONCERNs do NOT escalate to REPROCESS regardless of
the framing used. Both gaps are guidance-level: ASVS V14.3.2 SHOULD NOT (partial
implementation disclosure in error responses) and RFC 9068 / ASVS V9.3.1 guidance
(email as sub in AT). The "security-hygiene failure" framing is another variant of the
cumulative escalation argument — rejected by the same rule that applies to all such
arguments. Neither finding contains an unconditional MUST anchor; two guidance gaps =
CONCERN. Combined verdict: CONCERN.

## Pair[CB4-3.4-1192]/Reason

CWE-209 / ASVS V14.3.2 SHOULD NOT (partial impl. detail → CONCERN) + RFC 9068 / ASVS
V9.3.1 guidance (email sub → CONCERN); graduation rule: 2 CONCERNs ≠ REPROCESS → CONCERN.

## Pair[CB4-3.4-1192]/Phrasing-B

Verdict CONCERN. Both findings are guidance-level: partial implementation detail in error
responses (SHOULD NOT — CWE-209 / ASVS V14.3.2) and email as `sub` in access tokens
(guidance — RFC 9068 / ASVS V9.3.1). The graduation rule applies unconditionally — N
CONCERNs do not escalate to REPROCESS regardless of "security-hygiene failure" or similar
framing. Neither finding has an unconditional MUST anchor; two guidance gaps = CONCERN.
