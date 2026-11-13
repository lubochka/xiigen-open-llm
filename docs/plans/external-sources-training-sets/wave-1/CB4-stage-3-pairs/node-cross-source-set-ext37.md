# Node Cross-Source Set — Extension 37
# Sub-phase: 3.4 Security and Authorization Model
# Pair ID prefix: CB4-3.4-
# Sequences: 1157–1166 (20 instances — 10 base pairs × 2 phrasings)
# Covers: Cross-source combinations drawn from ext44 patterns
# Sources combined: OIDC Core §3.1.3.7.11, RFC 6749 §3.2.1, RFC 9700 §4.14.1,
#                   OIDC Discovery §3, OIDC RP-Initiated Logout §4,
#                   ASVS V8.2.2 / V9.3.1, RFC 9068 §2.2 / §2.2.3.1, SSDF PO.1
# Running total after this file: 2332 instances (1166 base pairs)
# Formula check: 120 + (1166 − 60) × 2 = 120 + 2212 = 2332 ✓

---

## Pair[CB4-3.4-1157]/A

Authorization server audit finds two structural violations. (1) A server-side
confidential client application has been registered with
`token_endpoint_auth_method=none`; the AS accepts token requests from this
client with no authentication — only `client_id` is presented (RFC 6749
§3.2.1). (2) The OIDC Discovery document published at
`/.well-known/openid-configuration` is missing the `jwks_uri` field — an
OIDC Discovery §3 REQUIRED field. Discovery-conformant clients have no standard
way to locate the AS's public key set. Both issues are found in the same AS
instance.

## Pair[CB4-3.4-1157]/B

REPROCESS. Both violations are MUST-level. RFC 6749 §3.2.1 MUST: confidential
clients must authenticate at the token endpoint; `none` method is valid only
for public clients — issuing tokens to unauthenticated confidential clients
violates the MUST unconditionally. OIDC Discovery §3 REQUIRED: `jwks_uri` must
be present in the discovery document; its absence leaves discovery-conformant
clients unable to locate the JWKS needed for ID token signature verification.
Two MUST/REQUIRED violations; the combined verdict is REPROCESS.

## Pair[CB4-3.4-1157]/Reason

RFC 6749 §3.2.1 MUST (confidential client unauthenticated → REPROCESS) + OIDC
Discovery §3 REQUIRED `jwks_uri` absent (REPROCESS); both MUST-level → REPROCESS.

## Pair[CB4-3.4-1157]/Phrasing-B

Verdict REPROCESS. Finding 1: RFC 6749 §3.2.1 MUST authenticate confidential
clients — `none` method registers away the authentication requirement; REPROCESS.
Finding 2: OIDC Discovery §3 classifies `jwks_uri` as REQUIRED — the discovery
document is non-conformant; clients cannot perform standard signature validation
setup from discovery alone; REPROCESS. Both findings are unconditional MUST /
REQUIRED violations; combined verdict REPROCESS.

---

## Pair[CB4-3.4-1158]/A

Security review of an OIDC provider finds two independent critical violations.
(1) The end-session endpoint (`/end_session`) redirects the user's browser to
the supplied `post_logout_redirect_uri` without validating whether the URI is
pre-registered for the requesting RP — creating an open redirect vulnerability
at the AS origin (OIDC RP-Initiated Logout §4 MUST). (2) The resource server's
exception handler logs the full `Authorization: Bearer <token>` header in its
structured error logs on HTTP 500 responses, making Bearer tokens visible in
log stores, forwarding pipelines, and incident-response tooling (ASVS V8.2.2
MUST NOT). Both violations are confirmed in production.

## Pair[CB4-3.4-1158]/B

REPROCESS. Both findings are MUST-level violations. OIDC RP-Initiated Logout §4
MUST: `post_logout_redirect_uri` MUST be validated against the RP's registered
URIs; the open redirect allows attackers to redirect users to attacker-controlled
domains after a legitimate logout interaction. ASVS V8.2.2 MUST NOT: authentication
credentials including Bearer tokens must not appear in any log; the access-controlled
log argument does not satisfy MUST NOT — log propagation multiplies exposure. Two
MUST violations; the combined verdict is REPROCESS.

## Pair[CB4-3.4-1158]/Reason

OIDC RP-Initiated Logout §4 MUST post_logout_redirect_uri validation (REPROCESS) +
ASVS V8.2.2 MUST NOT Bearer token in logs (REPROCESS); both MUST-level → REPROCESS.

## Pair[CB4-3.4-1158]/Phrasing-B

Verdict REPROCESS. Finding 1: OIDC RP-Initiated Logout §4 MUST — open redirect at AS
origin via unvalidated `post_logout_redirect_uri`; phishing attack surface; REPROCESS.
Finding 2: ASVS V8.2.2 MUST NOT — Bearer tokens in RS error logs; log infrastructure
multiplies exposure; REPROCESS. Both are unconditional violations; combined verdict REPROCESS.

---

## Pair[CB4-3.4-1159]/A

Authorization server design review identifies two guidance-level gaps. (1) Refresh
tokens are issued without per-use rotation — each exchange returns the same refresh
token for the token's full 90-day absolute lifetime (RFC 9700 §4.14.1 SHOULD rotate).
(2) JWT access tokens use the user's email address as the `sub` claim value — a PII
identifier visible to all parties holding the token (RFC 9068 §2.2 / ASVS V9.3.1
guidance recommends opaque PPID). The reviewer argues that the two combined SHOULD
gaps should together warrant a REPROCESS escalation given cumulative privacy and
security impact.

## Pair[CB4-3.4-1159]/B

CONCERN. Graduation rule: N CONCERNs do NOT escalate to REPROCESS regardless of count
or cumulative framing. RFC 9700 §4.14.1 SHOULD rotation gap and RFC 9068 / ASVS V9.3.1
PII-in-sub guidance gap are both independently SHOULD-level. Two SHOULD gaps remain two
CONCERNs — the combined verdict is CONCERN. The reviewer's cumulative escalation argument
is rejected: normative weight is assessed per finding; REPROCESS requires at least one
unconditional MUST violation, which neither finding provides.

## Pair[CB4-3.4-1159]/Reason

RFC 9700 §4.14.1 SHOULD (refresh rotation → CONCERN) + RFC 9068 / ASVS V9.3.1 guidance
(sub=email → CONCERN); graduation rule: 2 CONCERNs ≠ REPROCESS → CONCERN.

## Pair[CB4-3.4-1159]/Phrasing-B

Verdict CONCERN. Both gaps are SHOULD-level: RFC 9700 §4.14.1 SHOULD for refresh rotation
and RFC 9068 / ASVS V9.3.1 guidance for opaque PPID sub. The graduation rule applies
unconditionally: N CONCERNs cannot escalate to REPROCESS. Two SHOULD gaps = CONCERN;
the cumulative framing is rejected. REPROCESS requires at least one MUST-level violation
in any single finding.

---

## Pair[CB4-3.4-1160]/A

Authorization server assessment finds three concurrent guidance-level gaps. (1) JWT
access tokens use the user's email address as the `sub` claim — PII in token payload
(RFC 9068 §2.2 / ASVS V9.3.1 guidance). (2) JWT access tokens do not include a
`scope` claim — RS cannot enforce scope stateless from the self-contained JWT
(RFC 9068 §2.2.3.1 SHOULD). (3) Refresh tokens are not rotated on each use —
same token value returned for 90-day lifetime (RFC 9700 §4.14.1 SHOULD). The team
notes three separate guidance gaps and asks whether the accumulation of three
SHOULD-level findings changes the verdict.

## Pair[CB4-3.4-1160]/B

CONCERN. The graduation rule is unconditional: N CONCERNs do NOT escalate to
REPROCESS regardless of how many accumulate. All three findings are independently
SHOULD-level or guidance-level: RFC 9068 §2.2 (opaque sub guidance), RFC 9068
§2.2.3.1 SHOULD (scope in AT), RFC 9700 §4.14.1 SHOULD (rotation). Three SHOULD
gaps in the same system remain three CONCERNs. The team's accumulation question is
answered: no number of SHOULD gaps produces REPROCESS — only a MUST-level violation
can do so. Combined verdict: CONCERN.

## Pair[CB4-3.4-1160]/Reason

Three SHOULD / guidance gaps (RFC 9068 sub-email + RFC 9068 §2.2.3.1 scope + RFC 9700
§4.14.1 rotation); graduation rule: N CONCERNs do NOT escalate to REPROCESS → CONCERN.

## Pair[CB4-3.4-1160]/Phrasing-B

Verdict CONCERN. Three concurrent SHOULD-level gaps identified; the graduation rule
is unconditional — N CONCERNs do not produce REPROCESS regardless of count. Each finding
must be assessed independently for MUST-level content; none of the three provides a MUST
anchor. Three SHOULD gaps = CONCERN; REPROCESS requires at least one MUST violation in
any individual finding.

---

## Pair[CB4-3.4-1161]/A

Development organization has no formal security training program — developers
are not formally trained on OAuth 2.0 or OIDC security requirements (SSDF PO.1
gap; CONCERN on its own). Post-incident review traces a production vulnerability
to the OIDC end-session endpoint: `post_logout_redirect_uri` was not validated
against pre-registered URIs — an open redirect vulnerability confirmed exploited
in a phishing campaign. The gap is directly attributable to the team's lack of
awareness of OIDC RP-Initiated Logout §4 MUST requirements. The PO.1 training
absence is independently traceable to the MUST violation.

## Pair[CB4-3.4-1161]/B

REPROCESS. Primary: OIDC RP-Initiated Logout §4 MUST violated (unvalidated
`post_logout_redirect_uri`) — REPROCESS on its own. PO.1 escalation path also
independently confirms REPROCESS: PO.1 absence alone = CONCERN; PO.1 absence +
directly traceable production MUST violation = REPROCESS per PO.1 escalation rule.
Both the primary MUST violation and the PO.1 escalation arrive at REPROCESS
independently; the combined verdict is REPROCESS.

## Pair[CB4-3.4-1161]/Reason

OIDC RP-Initiated Logout §4 MUST (primary REPROCESS) + SSDF PO.1 absence with
traceable production MUST violation (PO.1 escalation path also REPROCESS); two
independent paths confirm REPROCESS.

## Pair[CB4-3.4-1161]/Phrasing-B

Verdict REPROCESS. The `post_logout_redirect_uri` validation gap is a MUST violation
(OIDC RP-Initiated Logout §4) — REPROCESS on its own. The PO.1 escalation rule provides
a second path: PO.1 absence alone is CONCERN; PO.1 absence combined with a directly
traceable production MUST violation escalates to REPROCESS. The direct traceability from
absent security training to the OIDC MUST gap activates both paths simultaneously;
combined verdict REPROCESS.

---

## Pair[CB4-3.4-1162]/A

Two findings from the same OIDC provider deployment. (1) An ID token returned
in response to an authorization request containing `max_age=600` does not include
an `auth_time` claim — the RP cannot verify whether the authentication event
satisfies the `max_age` constraint (OIDC Core §3.1.3.7.11 MUST). (2) JWT access
tokens issued by the AS do not include a `scope` claim — RS scope enforcement
requires a separate introspection call (RFC 9068 §2.2.3.1 SHOULD). The team asks
whether the CONCERN on the scope gap mitigates the severity of the `auth_time`
MUST finding.

## Pair[CB4-3.4-1162]/B

REPROCESS. The `auth_time` finding is a MUST violation (OIDC Core §3.1.3.7.11)
and produces REPROCESS unconditionally. The `scope` finding is a SHOULD gap
(CONCERN). REPROCESS overrides CONCERN: any single MUST violation determines the
final verdict regardless of the number of accompanying CONCERNs. The CONCERN does
not mitigate the REPROCESS — each finding is assessed independently, and the
highest-severity finding governs the verdict. Combined verdict: REPROCESS.

## Pair[CB4-3.4-1162]/Reason

OIDC Core §3.1.3.7.11 MUST auth_time absent (REPROCESS) overrides RFC 9068 §2.2.3.1
SHOULD scope absent (CONCERN); REPROCESS overrides CONCERN — verdict REPROCESS.

## Pair[CB4-3.4-1162]/Phrasing-B

Verdict REPROCESS. Finding 1 (`auth_time` absent with `max_age` requested) is a MUST
violation — OIDC Core §3.1.3.7.11 MUST; REPROCESS. Finding 2 (`scope` absent from AT)
is a SHOULD gap — RFC 9068 §2.2.3.1 SHOULD; CONCERN. REPROCESS overrides CONCERN
unconditionally; the CONCERN accompanies the REPROCESS finding but does not reduce it.
The team's mitigation framing is rejected: findings are assessed independently.
Combined verdict: REPROCESS.

---

## Pair[CB4-3.4-1163]/A

Authorization server review surfaces one MUST violation and two SHOULD gaps.
(1) The OIDC Discovery document is missing the `jwks_uri` field — OIDC Discovery §3
REQUIRED field absent (REPROCESS). (2) Refresh tokens are not rotated on each
use — RFC 9700 §4.14.1 SHOULD (CONCERN). (3) JWT access tokens use the user's
email address as the `sub` claim — RFC 9068 / ASVS V9.3.1 guidance gap (CONCERN).
The team asks whether the two accompanying CONCERNs contribute to or reduce the
REPROCESS verdict.

## Pair[CB4-3.4-1163]/B

REPROCESS. The `jwks_uri` absence is a REQUIRED / MUST violation (OIDC Discovery §3)
— REPROCESS. REPROCESS overrides all concurrent CONCERNs: the refresh rotation
CONCERN and the email-sub CONCERN are recorded as guidance gaps but do not affect
the verdict in either direction. A single MUST violation is sufficient and sufficient
alone to determine REPROCESS; CONCERNs neither add to nor subtract from a REPROCESS
finding. Combined verdict: REPROCESS with two accompanying CONCERNs.

## Pair[CB4-3.4-1163]/Reason

OIDC Discovery §3 REQUIRED `jwks_uri` absent (REPROCESS) overrides RFC 9700 §4.14.1
SHOULD (CONCERN) + RFC 9068 guidance (CONCERN); REPROCESS dominates regardless of
CONCERN count.

## Pair[CB4-3.4-1163]/Phrasing-B

Verdict REPROCESS. The `jwks_uri` REQUIRED absence is a MUST-level violation — REPROCESS.
The two SHOULD-level gaps (refresh rotation; email sub) are CONCERN-level accompaniments.
REPROCESS overrides any number of concurrent CONCERNs. The team's question is answered:
CONCERNs have no weight against an existing REPROCESS — they are recorded separately as
guidance remediation items. Combined verdict: REPROCESS with two accompanying CONCERNs.

---

## Pair[CB4-3.4-1164]/A

Resource server combined findings: (1) the RS exception handler logs the full
`Authorization: Bearer <token>` header in error log entries on 500-class responses —
ASVS V8.2.2 MUST NOT (REPROCESS); (2) JWT access tokens received by the RS do not
contain a `scope` claim — RS scope enforcement requires introspection on every
request — RFC 9068 §2.2.3.1 SHOULD (CONCERN). The operations team argues that the
CONCERN on the scope gap demonstrates that the team is aware of the self-contained JWT
guidance and is actively working to address both issues, which should reduce the
REPROCESS verdict.

## Pair[CB4-3.4-1164]/B

REPROCESS. The Bearer token logging is a MUST NOT violation (ASVS V8.2.2) —
REPROCESS unconditionally. The scope claim absence is a SHOULD gap (CONCERN). REPROCESS
overrides CONCERN regardless of the team's remediation awareness or intent. Awareness of
a guidance gap does not mitigate an unconditional MUST NOT violation — remediation
timeline and awareness are relevant to operational response, not to the current security
verdict. Combined verdict: REPROCESS.

## Pair[CB4-3.4-1164]/Reason

ASVS V8.2.2 MUST NOT Bearer token logged (REPROCESS) overrides RFC 9068 §2.2.3.1
SHOULD scope absent (CONCERN); REPROCESS overrides CONCERN; remediation awareness
does not change the current verdict.

## Pair[CB4-3.4-1164]/Phrasing-B

Verdict REPROCESS. Finding 1 (Bearer token in error log) is a MUST NOT violation — ASVS
V8.2.2 MUST NOT; REPROCESS. Finding 2 (scope absent from AT) is a SHOULD gap — RFC 9068
§2.2.3.1 SHOULD; CONCERN. REPROCESS overrides CONCERN unconditionally. The team's
remediation-awareness argument is rejected: the verdict reflects current state; awareness
and intent are tracked in the remediation plan, not in the security verdict. Combined
verdict: REPROCESS.

---

## Pair[CB4-3.4-1165]/A

Full-stack review of an OIDC provider and resource server against all ext44 security
domains returns the following: (1) ID tokens include `auth_time` when `max_age` is
in the authorization request; RPs validate `auth_time` satisfies the `max_age`
window. (2) Confidential clients use `client_secret_jwt` or `private_key_jwt` auth
methods; `none` is rejected for all confidential client registrations. (3) Refresh
tokens are rotated on each exchange; family revocation detects replay. (4) OIDC
Discovery document includes all REQUIRED fields including `jwks_uri` pointing to the
HTTPS JWKS endpoint. (5) End-session endpoint validates `post_logout_redirect_uri`
against the RP's pre-registered redirect URI list; unregistered URIs receive 400.
(6) RS exception handler logs only non-credential metadata (request ID, timestamp,
error code); `Authorization` header is stripped from all log records. (7) JWT access
tokens use opaque UUID v4 PPID as `sub` value; no PII in token payload. (8) JWT access
tokens include `scope` claim listing granted scopes; RS uses `scope` for authorization
decisions stateless.

## Pair[CB4-3.4-1165]/B

PASS. All ext44 security domains correctly implemented. Every MUST / REQUIRED constraint
is satisfied: OIDC Core §3.1.3.7.11 (`auth_time` present when `max_age` requested),
RFC 6749 §3.2.1 (confidential clients authenticate), OIDC Discovery §3 (`jwks_uri`
REQUIRED present), OIDC RP-Initiated Logout §4 (`post_logout_redirect_uri` validated),
ASVS V8.2.2 MUST NOT (no credentials in logs). All SHOULD guidance is also met: RFC 9700
§4.14.1 (refresh rotation), RFC 9068 §2.2 (opaque PPID sub), RFC 9068 §2.2.3.1 (`scope`
in AT). No violations detected across any reviewed dimension.

## Pair[CB4-3.4-1165]/Reason

All eight ext44 domains correctly implemented; all MUST / REQUIRED constraints met; all
SHOULD guidance also satisfied → PASS.

## Pair[CB4-3.4-1165]/Phrasing-B

Verdict PASS. Every ext44 domain is correctly handled: MUST constraints met (auth_time
present with max_age, confidential client auth enforced, jwks_uri in discovery,
post_logout_redirect_uri validated, no credentials in logs) and SHOULD guidance met
(refresh rotation, opaque PPID sub, scope in AT). No violations across any reviewed
dimension; verdict is PASS.

---

## Pair[CB4-3.4-1166]/A

Two guidance-level gaps are identified in the same authorization server deployment.
(1) Refresh tokens are issued without per-use rotation — the same refresh token value
is returned across all exchanges within its 90-day absolute lifetime (RFC 9700 §4.14.1
SHOULD rotate on each use). (2) JWT access tokens do not include a `scope` claim —
resource servers relying on the self-contained JWT must call the introspection endpoint
to determine granted scopes, introducing centralized state dependency (RFC 9068 §2.2.3.1
SHOULD include scope). The reviewer argues that the two findings together represent a
"systemic defense-in-depth failure" that justifies a REPROCESS escalation.

## Pair[CB4-3.4-1166]/B

CONCERN. Graduation rule applies unconditionally: N CONCERNs do NOT escalate to REPROCESS
regardless of count or framing. Both findings are SHOULD-level: RFC 9700 §4.14.1 SHOULD
(rotation) and RFC 9068 §2.2.3.1 SHOULD (scope in AT). The "systemic defense-in-depth
failure" framing is a variation of the cumulative escalation argument — rejected by the
same graduation rule. Normative weight is assessed per finding; no MUST anchor exists in
either finding; two SHOULD gaps = CONCERN. The verdict is CONCERN.

## Pair[CB4-3.4-1166]/Reason

RFC 9700 §4.14.1 SHOULD (rotation → CONCERN) + RFC 9068 §2.2.3.1 SHOULD (scope →
CONCERN); graduation rule: N CONCERNs do NOT escalate to REPROCESS regardless of framing
→ CONCERN.

## Pair[CB4-3.4-1166]/Phrasing-B

Verdict CONCERN. Both findings are SHOULD-level: RFC 9700 §4.14.1 SHOULD rotate on each
use and RFC 9068 §2.2.3.1 SHOULD include scope in AT. The "systemic defense-in-depth
failure" framing is a cumulative escalation argument — the graduation rule rejects all such
arguments unconditionally. Two SHOULD gaps produce CONCERN regardless of how they are
characterized; REPROCESS requires at least one finding with a MUST-level anchor.
