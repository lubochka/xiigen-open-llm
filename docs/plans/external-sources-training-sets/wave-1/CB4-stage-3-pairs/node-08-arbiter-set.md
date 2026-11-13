# Node 8 — Arbiter Units
# Batch: CB4 — Sub-phase: 3.4 — Wave 1
# Produced by Stage 3 Action 2 (gap closure pass)
# Sources: OWASP-ASVS 5.0.0, CWE-TOP25 2023, NIST-SSDF v1.1, IETF-RFCS
# License annotations per pair below
# Produced: 2026-05-05

---

## PASS cases first (required rule: Node 8 pass cases must precede CONCERN and REPROCESS)

---

Pair CB4-3.4-001 — ASVS 5.0.0 V6/V7/V8/V9 complete compliance (PASS)
Source: OWASP ASVS 5.0.0 — https://github.com/OWASP/ASVS
License: CC BY-SA 4.0
Attribution: OWASP ASVS Project, Version 5.0.0
Stratum: A (domain breadth — covers V6, V7, V8, V9)
Anonymisation: none

A: The result from the domain processors states: "V6.2.1 — password minimum 15
characters confirmed, exceeds 8-character minimum and meets strongly recommended
15-character threshold. V6.5.5 — OTP expiry exactly 10 minutes, at maximum
boundary. V7.3.1 — 20-minute idle timeout, risk analysis RA-AUTH-003 on file
documents the justification. V7.5.1 — re-authentication before credential
changes confirmed. V8.2.1 — RBAC on all endpoints, function-level access
restricted. V8.2.2 — per-resource ownership checks, IDOR/BOLA prevention
present. V9.1.1 — RS256 signature validation. V9.1.2 — 'none' algorithm
rejected from allowlist. V9.2.1 — exp and nbf claims validated. V9.2.3 — 'aud'
claim validated against service identifier." Arbiter role: ASVS-V6-Auth-Arbiter,
ASVS-V7-Session-Arbiter, ASVS-V8-AuthZ-Arbiter, ASVS-V9-Token-Arbiter.
Produce the arbiter verdict.

B: PASS. All four ASVS arbiters confirm compliance.

ASVS-V6-Auth-Arbiter: PASS. V6.2.1 confirmed — 15-char minimum exceeds 8-char
floor and meets the strongly recommended threshold. V6.5.5 confirmed — OTP 10
minutes and TOTP 30 seconds at maximum boundary; compliant.

ASVS-V7-Session-Arbiter: PASS. V7.3.1 confirmed — 20-minute idle timeout with
RA-AUTH-003 documenting the risk analysis justification. V7.5.1 confirmed —
re-authentication before credential changes present.

ASVS-V8-AuthZ-Arbiter: PASS. V8.2.1 confirmed — RBAC on all endpoints satisfies
function-level access restriction. V8.2.2 confirmed — ownership checks present,
IDOR/BOLA prevention satisfied.

ASVS-V9-Token-Arbiter: PASS. V9.1.1 (RS256), V9.1.2 ('none' rejected), V9.2.1
(exp/nbf), V9.2.3 (aud) all confirmed.

Combined verdict: PASS. No CONCERN or REPROCESS raised by any arbiter. All ten
ASVS requirement IDs (V6.2.1, V6.5.5, V7.3.1, V7.5.1, V8.2.1, V8.2.2, V9.1.1,
V9.1.2, V9.2.1, V9.2.3) individually confirmed.

Reason: A pass case is required before CONCERN and REPROCESS cases to prevent
the arbiters from learning to flag every input. This case confirms that clean
compliance evidence produces a PASS — the arbiter does not manufacture concerns
where none exist. The individual ASVS ID listing is required because a verdict
naming only the chapter (e.g., "V6 satisfied") is a coverage illusion —
it does not confirm that each individual requirement was checked.

---

Pair CB4-3.4-001 — Phrasing B
Source: OWASP ASVS 5.0.0 — CC By-SA 4.0 — OWASP ASVS Project, Version 5.0.0
Stratum: A

A: Five domain processors completed their ASVS 5.0.0 checks on the authentication
service and returned these findings: password strength (V6.2.1): implementation
uses 15 characters, minimum is 8, strongly recommended is 15 — threshold met.
OTP/TOTP lifetime (V6.5.5): OTP at 10-minute maximum, TOTP at 30-second maximum
— both at boundary, compliant. Session timeout (V7.3.1): 20-minute inactivity
timeout with documented risk analysis RA-AUTH-003 — requirement satisfied. Session
re-authentication (V7.5.1): re-auth enforced before credential modifications —
satisfied. Access controls (V8.2.1, V8.2.2): RBAC on all endpoints, per-resource
ownership checks, IDOR/BOLA prevention present. JWT validation (V9.1.1, V9.1.2,
V9.2.1, V9.2.3): RS256, 'none' algorithm rejected, exp/nbf validated, aud
claim validated. All five domains report no gaps. What is the arbiter verdict?

B: PASS. ASVS-V6-Auth-Arbiter, ASVS-V7-Session-Arbiter, ASVS-V8-AuthZ-Arbiter,
and ASVS-V9-Token-Arbiter all return PASS. Requirements confirmed individually:

V6.2.1 — password minimum satisfied (15 chars ≥ 8 required, 15 strongly recommended)
V6.5.5 — OTP lifetime satisfied (10 min = maximum; TOTP 30 sec = maximum; boundary values compliant)
V7.3.1 — session timeout satisfied (20 min, RA-AUTH-003 on file)
V7.5.1 — re-authentication satisfied (enforced before credential changes)
V8.2.1 — function-level access satisfied (RBAC on all endpoints)
V8.2.2 — data-specific access satisfied (ownership checks, IDOR/BOLA prevention)
V9.1.1 — token signature satisfied (RS256 validated)
V9.1.2 — None algorithm satisfied ('none' rejected from allowlist)
V9.2.1 — validity window satisfied (exp and nbf validated)
V9.2.3 — audience satisfied (aud claim validated)

No concern raised. Combined verdict: PASS.

Reason: Rephrasing preserves the same ten ASVS IDs and the same compliance
evidence at the same depth. The A changes from listing findings per arbiter
domain to listing them per processing domain — same information, different
organization. The B requires the same individual ID confirmation regardless
of how A is structured.

---

Pair CB4-3.4-002 — CWE clean authorization (PASS)
Source: MITRE CWE Top 25 2023 — https://cwe.mitre.org
License: Free for commercial and non-commercial use
Attribution: Copyright © The MITRE Corporation. CWE and CAPEC are registered
trademarks of The MITRE Corporation.
Stratum: A (domain breadth — covers CWE-862, CWE-306, CWE-269, CWE-287)
Anonymisation: none

A: The domain processors completed access control analysis on a REST API
profile service with the following findings: GET /api/users/{userId}/profile —
JWT required, JWT subject claim validated against requested userId, OR admin
scope required (no other path accepted). POST /api/users/{userId}/role — JWT
subject must match requesting user AND current role must be 'admin'; editors
cannot grant any role. Admin actions: identity and action logged before
execution. All endpoints require valid JWT with RS256 signature, exp, nbf,
and aud claim validation. Arbiter role: CWE-Authz-Arbiter, CWE-Authn-Arbiter.
Produce the arbiter verdict.

B: PASS. CWE-Authz-Arbiter and CWE-Authn-Arbiter return PASS.

CWE-Authn-Arbiter:
CWE-306 (Missing Authentication for Critical Function): ABSENT — JWT required
for all endpoints before any processing.
CWE-287 (Improper Authentication): ABSENT — RS256 signature validation plus
exp, nbf, and aud claim validation prevents token forgery and replay.

CWE-Authz-Arbiter:
CWE-862 (Missing Authorization): ABSENT — GET /api/users/{userId}/profile
validates JWT subject against requested userId OR requires admin scope. This
is a resource-level ownership check — the IDOR pattern is not present.
CWE-269 (Improper Privilege Management): ABSENT — role assignment restricted to
JWT subject matching requesting user AND admin role; editors cannot grant any
role. Privilege assignment is bounded by the caller's current role.
CWE-863 (Incorrect Authorization): ABSENT — authorization conditions are
correctly scoped for both read and write endpoints.

Combined verdict: PASS. No CWE Top 25 authentication or authorization weaknesses
identified in the described implementation.

Reason: The pass case confirms all five CWE IDs individually as absent. This
prevents the CWE arbiter from learning to flag everything — it must recognize
clean implementations as clean. The individual CWE ID listing is required because
"no security issues found" without IDs provides no training signal for the
specific weakness taxonomy.

---

Pair CB4-3.4-002 — Phrasing B
Source: MITRE CWE Top 25 2023 — CC terms — Copyright © The MITRE Corporation
Stratum: A

A: A security assessment of an API's authorization model returned: authentication
— all endpoints require valid JWT (RS256, exp/nbf/aud validated); no unauthenticated
access path exists. Authorization on profile retrieval — the handler compares the
JWT subject claim to the requested user ID and rejects the request if they don't
match, unless the JWT carries an admin scope; no bypass exists. Authorization on
role assignment — the handler requires the caller's own role to be 'admin';
editors are rejected before any role grant occurs; the role ceiling rule is
enforced. Audit logging — identity and action written before execution. Assess
whether CWE-287, CWE-306, CWE-862, CWE-269, CWE-863 are present.

B: CWE-Authn-Arbiter: PASS. CWE-287 ABSENT — JWT signature, exp, nbf, and aud
validation prevents authentication bypass and replay. CWE-306 ABSENT — no
endpoint is reachable without a valid JWT.

CWE-Authz-Arbiter: PASS. CWE-862 ABSENT — profile retrieval enforces resource-
level ownership check (JWT sub = userId) before returning data; IDOR pattern not
present. CWE-269 ABSENT — role assignment enforces admin-role ceiling; privilege
escalation beyond caller's own role is blocked. CWE-863 ABSENT — authorization
conditions correctly scoped and enforced.

Combined verdict: PASS — all five CWE IDs confirmed absent.

Reason: Different phrasing angle — the A describes the authorization logic in
terms of what the handlers DO rather than what the API exposes. The same five
CWE IDs must appear in B regardless of how A structures the description.

---

Pair CB4-3.4-003 — SSDF complete process evidence (PASS)
Source: NIST SP 800-218 SSDF v1.1 — US Government work, no copyright
Attribution (recommended): NIST SP 800-218 Secure Software Development Framework v1.1
Stratum: A (domain breadth — covers PO.1, PW.1, PW.2, PW.8)
Anonymisation: none

A: The process compliance domain processor returned the following findings for
the authorization model development artifacts: PO.1 — requirements document
TS-AUTHZ-001 on file, includes ASVS V8 control requirements and acceptance
criteria, filed by security lead 2026-02-15. PW.1 — threat model TM-AUTHZ-2026
on file, documents authorization design decisions, residual risks accepted,
risk mitigations for CWE-862 and CWE-269 noted. PW.2 — design review record
DR-AUTHZ-2026-03 on file, conducted 2026-03-10 (two weeks before implementation
sprint began), all findings resolved. PW.8 — penetration test report
TR-AUTHZ-2026-05 on file, OWASP ZAP scan plus manual review, zero critical or
high findings. Arbiter role: SSDF-PO-Arbiter, SSDF-PW-Arbiter. Produce verdict.

B: PASS. SSDF-PO-Arbiter and SSDF-PW-Arbiter both return PASS.

SSDF-PO-Arbiter:
PO.1 — PASS. Security requirements document TS-AUTHZ-001 named and on file.
Includes ASVS V8 control requirements. Named artifact confirmed.

SSDF-PW-Arbiter:
PW.1 — PASS. Threat model TM-AUTHZ-2026 named and on file. Documents design
decisions, risk mitigations, and accepted residual risks. Named artifact confirmed.
PW.2 — PASS. Design review DR-AUTHZ-2026-03 named, on file, conducted
2026-03-10 — two weeks before the implementation sprint. Pre-implementation
timing confirmed. All findings resolved.
PW.8 — PASS. Penetration test TR-AUTHZ-2026-05 named, on file, zero critical
or high findings. Security testing confirmed.

Combined verdict: PASS. All four SSDF practices evidenced with named artifacts.
Pre-implementation timing for PW.2 confirmed.

Reason: The SSDF pass case establishes that when named artifacts are present for
each practice, the arbiter returns PASS. The named artifact requirement prevents
coverage illusion — "security was done" without a document reference is not
evidence. The pre-implementation timing for PW.2 is noted as a positive signal
that will contrast with the post-implementation timing in Pair CB4-3.4-009.

---

Pair CB4-3.4-003 — Phrasing B
Source: NIST SP 800-218 SSDF v1.1 — US Government work — NIST SP 800-218
Stratum: A

A: An SSDF compliance processor reviewed the authorization model development
history and found: a security requirements document with ID TS-AUTHZ-001
listing all ASVS V8 authorization controls; a threat model with ID TM-AUTHZ-2026
covering design decisions and CWE-862/CWE-269 mitigations; a design review
record DR-AUTHZ-2026-03, dated two weeks before implementation began, with all
issues resolved; and a penetration test TR-AUTHZ-2026-05 with no critical
findings. Does this evidence satisfy NIST SSDF v1.1 PO and PW practices?

B: PASS. SSDF v1.1 PO and PW compliance confirmed.

PO.1: TS-AUTHZ-001 satisfies the security requirements definition practice.
Named document present, ASVS V8 scope confirmed.

PW.1: TM-AUTHZ-2026 satisfies the secure design practice. Named threat model
with documented design decisions and risk mitigations present.

PW.2: DR-AUTHZ-2026-03 satisfies the design review practice. Review conducted
before implementation — preventive value confirmed. All findings resolved.

PW.8: TR-AUTHZ-2026-05 satisfies the security testing practice. Zero critical
or high findings.

All four practices have named, traceable artifacts. Combined verdict: PASS.

Reason: Rephrasing presents the artifacts as a compliance question rather than
a processor finding. Same four SSDF practice IDs, same named artifacts, same
verdict. The B form is identical — four practice IDs confirmed, PASS stated.

---

Pair CB4-3.4-004 — RFC complete protocol conformance (PASS)
Source: RFC 6749, RFC 7519, RFC 7636 — IETF Trust Legal Provisions
Attribution: RFC 6749 (Hardt, IETF Trust), RFC 7519 (Jones et al., IETF Trust),
RFC 7636 (Sakimura et al., IETF Trust)
Stratum: A (domain breadth — covers OAuth 2.0, JWT, PKCE)
Anonymisation: none

A: The RFC compliance domain processor returned findings for an OAuth 2.0
authorization service: authorization code flow used exclusively (implicit and
password grant types disabled at server configuration). PKCE: S256 method,
plain method rejected by the authorization server. code_verifier entropy: 256
bits, generated via CSPRNG. State parameter: per-transaction, validated on
redirect callback. JWT access tokens: RS256 algorithm, 1-hour expiry, exp
and nbf claims validated, aud claim validated per resource server. Authorization
codes: 5-minute lifetime, single-use. Arbiter role: RFC-OAuth2-Arbiter,
RFC-JWT-Arbiter. Produce the verdict.

B: PASS. RFC-OAuth2-Arbiter and RFC-JWT-Arbiter both return PASS.

RFC-OAuth2-Arbiter:
RFC 6749 §4.1 — PASS. Authorization code flow in use; implicit and password
grants disabled.
RFC 6749 §4.1.2 — PASS. Authorization codes expire in 5 minutes; within
10-minute RFC recommendation.
RFC 6749 §10.12 — PASS. State parameter present and validated; CSRF prevention
satisfied.
RFC 7636 §4.2 — PASS. S256 method required and in use; plain method rejected
by authorization server. Client MUST use S256 when capable — capability
confirmed, requirement met.
RFC 7636 §4.1 — PASS. 256-bit code_verifier entropy meets minimum requirement.

RFC-JWT-Arbiter:
RFC 7519 §7.2 — PASS. RS256 signature validation confirmed; algorithm
verification satisfied.
RFC 7519 §4.1.4 — PASS. exp claim validated; 1-hour expiry within normal range.
RFC 7519 §4.1.3 — PASS. aud claim validated per resource server.

Combined verdict: PASS. All RFC section requirements satisfied. No concern raised.

Reason: The RFC pass case requires section-level references (RFC NNNN §X.X)
for each verified requirement. RFC-level references are the training signal that
distinguishes this arbiter from ASVS-level verification — the same behavior is
required at a different level of authority. "RFC requirements met" without
section references is a coverage illusion for RFC compliance.

---

Pair CB4-3.4-004 — Phrasing B
Source: RFC 6749, RFC 7519, RFC 7636 — IETF Trust Legal Provisions
Stratum: A

A: An RFC protocol conformance check on an OAuth 2.0 authorization service found:
only authorization code flow is available (implicit and password disabled);
PKCE is implemented with S256 (the server rejects plain, client uses S256 because
S256 is supported); state parameter is per-transaction and validated on redirect;
JWT tokens use RS256, expire after 1 hour, validate exp, nbf, and aud; auth
codes expire after 5 minutes and are single-use. Verify conformance against
RFC 6749, RFC 7636, and RFC 7519 requirements.

B: RFC-OAuth2-Arbiter and RFC-JWT-Arbiter: PASS.

RFC 6749 §4.1: authorization code flow only, not implicit — PASS
RFC 6749 §4.1.2: 5-minute auth code lifetime, within 10-minute recommendation — PASS
RFC 6749 §10.12: per-transaction state parameter validated — PASS
RFC 7636 §4.2: S256 in use, plain rejected, MUST-use-S256-when-capable satisfied — PASS
RFC 7636 §4.1: 256-bit code_verifier — minimum entropy met — PASS
RFC 7519 §7.2: RS256 algorithm validation — PASS
RFC 7519 §4.1.4: exp validated, 1-hour lifetime — PASS
RFC 7519 §4.1.3: aud validated per resource server — PASS

Combined verdict: PASS — all three RFCs conform.

Reason: The rephrasing reframes from "processor findings" to "conformance check
findings." Same RFC section list, same verdict. Eight section references required
in B regardless of A framing.

---

## REPROCESS cases

---

Pair CB4-3.4-005 — ASVS 10.4.6 plain method violation (REPROCESS)
Source: OWASP ASVS 5.0.0 — CC BY-SA 4.0 — OWASP ASVS Project, Version 5.0.0
Stratum: D (failure/edge case)
Anonymisation: none

A: The OAuth domain processor returned the following finding: authorization code
flow in use — ASVS 10.4.4 satisfied (not implicit, not password grant). PKCE
implemented — code_challenge present in authorization request. code_challenge_method
is 'plain'. The authorization server accepts both S256 and plain method values.
The team's security review previously assessed this as satisfying ASVS 10.4.6.
Arbiter role: ASVS-V10-OAuth-Arbiter. Produce the verdict.

B: REPROCESS. ASVS 10.4.6 violation confirmed. Error code:
ASVS_10_4_6_PLAIN_METHOD_REJECTED.

ASVS-V10-OAuth-Arbiter finding:
ASVS 10.4.6 requires the authorization server to reject the 'plain'
code_challenge_method. Current state: authorization server accepts 'plain'.
This directly violates the "rejects 'plain' method" clause in ASVS 10.4.6.

The team's security review assessment is incorrect. PKCE is present (ASVS
10.4.6 first clause satisfied), but the plain method usage violates the
second clause: ASVS 10.4.6 requires rejection of the plain method, not
acceptance. An authorization server that accepts plain provides no meaningful
protection against authorization code interception — the plain method does not
prevent an attacker who intercepts the authorization code from constructing a
matching code_verifier.

RFC 7636 §4.2 additionally requires clients to use S256 when capable. The
authorization server supports S256 — the client is capable. The MUST requirement
is violated.

Remediation required:
1. Configure authorization server to reject code_challenge_method=plain.
2. Update client to use code_challenge_method=S256.
3. Verify code_verifier entropy ≥ 256 bits (RFC 7636 §4.1).

Combined verdict: REPROCESS. This item is returned to development.

Reason: ASVS 10.4.6 uses the word "rejects" for plain method — this is not a
SHOULD or MAY. A CONCERN verdict here is wrong because the requirement is
unambiguous. The training distinction: PASS is wrong (violation present),
CONCERN is wrong (the word "rejects" makes this an enforcement requirement,
not a recommendation), REPROCESS is correct. The previous security review
assessment being "confirmed incorrect" by the arbiter is a required output —
the arbiter must be able to contradict prior assessments when evidence shows
a violation.

---

Pair CB4-3.4-005 — Phrasing B
Source: OWASP ASVS 5.0.0 — CC BY-SA 4.0
Stratum: D

A: An OAuth PKCE check found: the application uses OAuth 2.0 authorization code
flow with PKCE. The code_challenge_method in use is 'plain'. The authorization
server accepts plain because it was deployed before full RFC 7636 support was
added. S256 support is available on the authorization server but the client
was not updated to use it. The security team marked this as ASVS 10.4.6
compliant in the last audit. Arbiter role: ASVS-V10-OAuth-Arbiter.

B: REPROCESS. ASVS 10.4.6 violation. Error code: ASVS_10_4_6_PLAIN_METHOD_REJECTED.

ASVS 10.4.6 requires: (1) PKCE present — satisfied (code_challenge in request).
(2) 'plain' method rejected — NOT satisfied (server accepts plain). (3) code_verifier
validated — not evaluable until plain method is replaced with S256.

The authorization server must be configured to reject plain method. The client
must be updated to use S256. The S256 method is available on the server — the
client has no technical basis for remaining on plain.

The previous security audit assessment of ASVS 10.4.6 compliance is overridden.
A passing audit that missed the plain-method acceptance is an audit finding, not
ASVS compliance evidence.

RFC 7636 §4.2: clients MUST use S256 when capable. Server supports S256;
capability confirmed; MUST violation present.

Combined verdict: REPROCESS.

Reason: Same violation, different A framing — the constraint comes from a legacy
deployment timeline rather than a stated compatibility issue. The B remains the
same: REPROCESS, ASVS 10.4.6 cited with both clauses, RFC 7636 §4.2 cited,
previous audit overridden.

---

Pair CB4-3.4-006 — CWE-862 IDOR missing resource check (REPROCESS)
Source: MITRE CWE Top 25 2023 — Copyright © The MITRE Corporation
Stratum: D (failure/edge case)
Anonymisation: none

A: The access control domain processor analyzed endpoint GET /api/v1/users/{userId}/profile.
Finding: the endpoint requires a valid JWT (authentication present — no CWE-306).
The endpoint handler retrieves the user profile matching the userId path parameter
and returns it. No check exists in the handler that relates the JWT subject claim
to the requested userId path parameter. The rationale documented in code comments:
"authentication is required, so only logged-in users can call this." Arbiter role:
CWE-Authz-Arbiter. Produce the verdict.

B: REPROCESS. CWE-862 (Missing Authorization) confirmed. Error code:
CWE_862_IDOR_MISSING_RESOURCE_CHECK. CWE Top 25 rank: #11.

CWE-Authz-Arbiter finding:
The code comment rationale is incorrect. Authentication confirms that the caller
is a known user. Authorization confirms that the known user has permission to
access the specific resource requested. These are distinct checks.

This endpoint authenticates (JWT required — CWE-306 not present). It does not
authorize (no check that the JWT subject matches the requested userId, and no
scope or role check for cross-user access — CWE-862 present).

Impact: Any authenticated user can retrieve any other user's profile by
supplying a different userId value in the URL path. The only requirement is a
valid JWT for any account in the system. This is the canonical IDOR pattern
for CWE-862.

Required fix: The handler must verify before returning data that one of the
following is true:
(a) JWT sub claim equals the requested userId (caller is accessing their own
    profile), OR
(b) JWT contains an explicit admin or cross-user-read scope/role claim.

The ASVS equivalent is V8.2.2 (data-specific access restricted to consumers
with explicit permissions to specific data items).

Combined verdict: REPROCESS. This item is returned to development.

Reason: Authentication does not substitute for authorization — this is the
primary training signal for the IDOR pattern. CWE-862 is the correct
category (authorization check absent), not CWE-863 (authorization check present
but wrong) or CWE-269 (privilege scope). The endpoint has NO resource-level
authorization check at all, making this unambiguously CWE-862. CWE-306 is
confirmed absent because authentication IS present — the arbiter must separately
confirm what is present and what is absent.

---

Pair CB4-3.4-006 — Phrasing B
Source: MITRE CWE Top 25 2023 — Copyright © The MITRE Corporation
Stratum: D

A: Security analysis of an API endpoint: GET /api/v1/users/{userId}/profile
requires a Bearer JWT. The endpoint handler code is: "var profile =
db.Profiles.Find(userId); return Ok(profile);". No additional authorization
logic exists in the handler. The JWT middleware validates the token and
populates request.User but no further check is performed in the handler.
The handler trusts that the JWT middleware already confirmed the user is
legitimate. Arbiter role: CWE-Authz-Arbiter.

B: REPROCESS. CWE-862 confirmed. Error code: CWE_862_IDOR_MISSING_RESOURCE_CHECK.

The handler code "db.Profiles.Find(userId); return Ok(profile)" executes the
database query and returns the result without any check that the authenticated
user (request.User) has permission to access the specific profile identified
by userId.

Authentication (JWT validation) confirms the caller is known. The JWT middleware
has no knowledge of which userId the caller is entitled to access — it only
confirms the caller's identity. Resource-level authorization must happen in
the handler. It does not.

CWE-862 (Missing Authorization): CONFIRMED. IDOR pattern present.
CWE-306 (Missing Authentication): ABSENT — JWT required.

Required fix: Insert resource-level check before the return:
`if (request.User.Id != userId && !request.User.HasScope("admin")) return Forbid();`

Verdict: REPROCESS. Handler must be updated before deployment.

Reason: The rephrasing shows actual handler code rather than a natural language
description. The CWE-862 finding must be the same — the absence of a check
between "Find(userId)" and "return Ok(profile)" is always CWE-862. The fix
guidance is now expressed as code rather than prose, but the CWE ID and verdict
are identical.

---

Pair CB4-3.4-007 — SSDF missing design artifacts (REPROCESS)
Source: NIST SP 800-218 SSDF v1.1 — US Government work
Attribution (recommended): NIST SP 800-218 SSDF v1.1
Stratum: D (failure/edge case)
Anonymisation: none

A: The SSDF compliance processor analyzed the authorization model development
record and found: a penetration test report (TR-AUTHZ-2026-ZAP) showing no
critical findings from OWASP ZAP; ASVS V8 runtime compliance confirmed by
automated checks; code review conducted during pull request merges (code reviewer
approved all changes). No security requirements document found. No threat model
found. No design review document found. The team's SSDF compliance assertion:
"we follow standard security practices and have test evidence." Arbiter role:
SSDF-PO-Arbiter, SSDF-PW-Arbiter. Produce verdict.

B: REPROCESS. SSDF design evidence missing for PO.1, PW.1, PW.2.
Error code: SSDF_DESIGN_EVIDENCE_MISSING.

SSDF-PO-Arbiter:
PO.1 (Define Security Requirements): NOT EVIDENCED. "Standard security practices"
is a claim, not evidence. PO.1 requires a named requirements document containing
the specific security requirements for this software. No document is named.

SSDF-PW-Arbiter:
PW.1 (Design Software to Meet Security Requirements): NOT EVIDENCED. No threat
model or design decision document referenced. PW.1 requires evidence that the
software design was evaluated against security requirements and risk mitigations
were documented. Runtime test results do not provide this evidence.

PW.2 (Review the Software Design): NOT EVIDENCED. The penetration test report
(TR-AUTHZ-2026-ZAP) and the ASVS V8 runtime compliance checks are PW.8 evidence
(security testing of executable code). They confirm that the implemented code has
no known runtime vulnerabilities. They do NOT confirm that the software design
was reviewed for security requirement compliance. PW.2 requires a design review
document — a record that the design was examined, not that the running code was
tested.

Note: Code review during pull request merges is a development quality practice,
not a SSDF PW.2 design review. A PR code review checks that the implementation
matches the design. A PW.2 design review checks that the design satisfies
security requirements.

PW.8 (Test Executable Code): EVIDENCED. TR-AUTHZ-2026-ZAP is PW.8 evidence.
ASVS V8 runtime compliance confirmation is also PW.8-equivalent evidence.

Combined verdict: REPROCESS. Three required practices (PO.1, PW.1, PW.2) have
no named artifact. Remediation required before SSDF compliance can be asserted.

Reason: This pair trains the core SSDF training signal: runtime test results
and ASVS compliance confirmation are PW.8 evidence only. They do not satisfy
PW.2 (design review) or PW.1 (design meets requirements). The explicitly named
PR code review is included to force the arbiter to distinguish a development
quality practice from an SSDF design practice. PW.8 is correctly confirmed as
satisfied — the arbiter must not deny what IS evidenced while flagging what is not.

---

Pair CB4-3.4-007 — Phrasing B
Source: NIST SP 800-218 SSDF v1.1 — US Government work
Stratum: D

A: A development team is asserting NIST SSDF v1.1 compliance for their
authorization model. Their evidence package contains: a ZAP scan report with
zero critical findings, an ASVS V8 compliance report showing all runtime checks
passing, and a git history showing that every commit had a code reviewer. The
team says they "practice secure development throughout the SDLC." No document
with a name like requirements spec, threat model, or design review appears in
the package. Arbiter role: SSDF-PW-Arbiter. Is SSDF compliance established?

B: REPROCESS. SSDF compliance is NOT established. SSDF_DESIGN_EVIDENCE_MISSING.

The evidence package contains: ZAP scan report → SSDF PW.8 evidence (security
testing of executable code). ASVS runtime compliance → SSDF PW.8-equivalent
evidence. Code review in git history → development quality practice, not
SSDF PW.2.

The evidence package does not contain: A security requirements document (PO.1).
A threat model or secure design artifact (PW.1). A design review record showing
the authorization model design was evaluated for security requirement compliance
(PW.2).

"We practice secure development throughout the SDLC" is a process claim. It
confirms nothing about which SSDF practices were performed or what artifacts
they produced. SSDF compliance requires named artifacts for each practice —
not process assertions.

Remediation: produce a requirements document (PO.1), a threat model or design
record (PW.1), and a design review record (PW.2) before asserting SSDF compliance.

Combined verdict: REPROCESS.

Reason: The rephrasing reframes from "processor found the following" to "team
is asserting compliance with this evidence." Same three missing practices, same
REPROCESS verdict. The B focuses on the evidence-to-practice mapping rather
than listing artifacts. The core statement is the same: ZAP scan + ASVS compliance
= PW.8 only; design phase artifacts are missing.

---

Pair CB4-3.4-008 — RFC JWT HS256 trust boundary (REPROCESS)
Source: RFC 7519 (JWT) — IETF Trust Legal Provisions
Attribution: RFC 7519, Jones et al., IETF Trust
Stratum: D (failure/edge case)
Anonymisation: none

A: The JWT security domain processor returned: access tokens are signed with
HMAC-SHA256 (HS256). The signing key is a 256-bit symmetric key stored in a
secrets manager shared across the authorization server and six resource servers.
All six resource servers retrieve this key to validate incoming JWTs. Token
lifetime: 1 hour. exp, nbf, and aud claims are validated. The team says HS256
was chosen for simplicity — no JWKS endpoint needed, no key rotation complexity.
Arbiter role: RFC-JWT-Arbiter. Produce verdict.

B: REPROCESS. RFC 7519 §8 trust boundary violation confirmed.
Error code: RFC_7519_TRUST_BOUNDARY_VIOLATION.

RFC-JWT-Arbiter finding:
RFC 7519 §8 (Security Considerations) requires that JWT signing keys be
treated as sensitive material. In this deployment, the HS256 signing key is
shared across six resource servers. This creates a structural trust boundary
violation.

The specific threat: HS256 uses the same key for signing (issuing tokens) and
verification (validating tokens). Any party that can validate JWTs can also
create valid JWTs. Six resource servers hold the key. A single compromised
resource server can forge JWT tokens accepted by all other resource servers
in the deployment. This is not a hypothetical — it is a direct consequence of
symmetric key sharing.

This is not merely "HS256 is less secure than RS256." The concern is cross-service
token forgery capability. In a single-server deployment, HS256 is acceptable.
In a multi-server deployment where multiple services share the key, the key's
trust boundary collapses across all key holders.

Required remediation:
1. Migrate JWT signing to RS256 or ES256 (asymmetric algorithms).
2. The authorization server holds the private signing key exclusively.
3. Resource servers retrieve the public verification key via JWKS endpoint
   (RFC 7517). Public keys can be cached; key rotation is handled via
   JWKS key ID (kid) parameter.
4. No resource server holds signing capability after migration.

Combined verdict: REPROCESS. This configuration must be changed before
production deployment of any additional services.

Reason: The trust boundary violation distinguishes this from a "weaker algorithm"
concern. HS256 in isolation is valid per RFC 7519. HS256 with shared key across
multiple services creates a specific, structural threat: cross-service forgery.
The arbiter must name this threat specifically, not just recommend RS256. The
remediation must include the JWKS endpoint approach because that is the
architectural change that restores the trust boundary.

---

Pair CB4-3.4-008 — Phrasing B
Source: RFC 7519 — IETF Trust Legal Provisions
Stratum: D

A: Six microservices share a single 256-bit HMAC-SHA256 signing key for JWT
validation. The key is distributed from a central secrets manager. Each service
validates JWTs by retrieving the key and running HMAC verification. The
authorization server signs tokens with the same key. One of the six services was
recently assessed as having a SQL injection vulnerability in an unrelated part
of its codebase. Arbiter role: RFC-JWT-Arbiter.

B: REPROCESS. RFC 7519 §8 trust boundary violation. Error code:
RFC_7519_TRUST_BOUNDARY_VIOLATION.

The SQL injection finding in one service makes the trust boundary failure
immediately concrete: an attacker who exploits the SQL injection vulnerability
gains access to that service's memory and key material, including the shared
HS256 signing key. With the signing key, the attacker can forge JWT tokens that
will be accepted by all six services.

RFC 7519 §8: JWT signing key material must be treated as sensitive. A key held
by six services — including a service with a confirmed SQL injection vulnerability
— is not sensitive in any meaningful sense.

This configuration requires migration to RS256 or ES256 with JWKS-based public
key distribution. After migration: the authorization server holds the signing
private key; resource servers hold only the public verification key; a compromised
resource server cannot forge tokens for other services.

Combined verdict: REPROCESS.

Reason: The rephrasing introduces a concrete compromise scenario (SQL injection
in one of the six services) that makes the trust boundary failure immediately
actionable rather than theoretical. The RFC §8 citation is the same; the trust
boundary violation finding is the same; the remediation is the same. The
concrete scenario demonstrates why the six-service key sharing creates an
immediate (not theoretical) risk.

---

## CONCERN cases

---

Pair CB4-3.4-009 — ASVS 7.3.1 satisfied, internal standard deviation (CONCERN)
Source: OWASP ASVS 5.0.0 — CC BY-SA 4.0 — OWASP ASVS Project, Version 5.0.0
Stratum: D (failure/edge case — borderline)
Anonymisation: none

A: The session management domain processor returned: session idle timeout
implemented as 20 minutes. Risk analysis RA-AUTH-003 on file, documenting
the 20-minute justification based on session hijacking risk modeling. ASVS 7.3.1
checked: requires re-authentication per documented risk analysis — documentation
present, re-authentication enforced. Additional context: the organization's
enterprise security standard ESS-SESSION-001 requires exactly 30 minutes to
align with the enterprise SSO token lifetime. The implementation uses 20 minutes.
Arbiter role: ASVS-V7-Session-Arbiter. Produce verdict.

B: CONCERN. ASVS-V7-Session-Arbiter result:

ASVS 7.3.1 — PASS. 20-minute idle timeout enforced with RA-AUTH-003 on file
documenting the risk analysis justification. ASVS 7.3.1 requires enforcement
per documented risk analysis — both conditions met. ASVS 7.3.1 compliance is
confirmed.

CONCERN raised: The implementation deviates from internal standard ESS-SESSION-001,
which requires 30 minutes. The 20-minute timeout is MORE restrictive than the
30-minute standard (shorter timeout = sooner re-authentication = reduced session
exposure window). This is an organizational governance concern, not an ASVS
compliance violation.

This is not a REPROCESS. ASVS 7.3.1 is satisfied. REPROCESS would indicate
an ASVS compliance failure — none is present. The governance concern (internal
standard deviation) does not override ASVS compliance evidence.

Recommended resolution: (a) Update ESS-SESSION-001 to accept the 20-minute
value, supported by RA-AUTH-003's risk analysis findings. Or (b) increase
the implementation timeout to 30 minutes and update RA-AUTH-003 accordingly.
Both options resolve the governance mismatch.

Reason: The training distinction between CONCERN and REPROCESS for this pattern:
ASVS 7.3.1 compliance is confirmed (PASS), but an internal standard deviation
exists (CONCERN). The arbiter must not escalate to REPROCESS because the ASVS
requirement is satisfied. Escalating to REPROCESS for an internal standard
deviation trains the arbiter to block ASVS-compliant implementations — a false
positive error that erodes trust in the orchestrator.

---

Pair CB4-3.4-009 — Phrasing B
Source: OWASP ASVS 5.0.0 — CC BY-SA 4.0
Stratum: D

A: An authentication team is asking whether their 20-minute session idle timeout
is compliant with OWASP ASVS 5.0.0. They have a risk analysis on file (RA-AUTH-003)
that recommends 20 minutes based on session hijacking modeling. Their organization's
security policy says all sessions must use a 30-minute timeout to match the SSO
provider's token lifetime. ASVS 7.3.1 requires inactivity timeout enforced per
documented risk analysis. Arbiter role: ASVS-V7-Session-Arbiter.

B: CONCERN. ASVS 7.3.1 status: SATISFIED.

The 20-minute idle timeout is enforced and RA-AUTH-003 on file documents the
risk analysis justification. ASVS 7.3.1 does not specify a required timeout
value — it requires that the chosen value be documented and based on risk
analysis. Both conditions are met.

Concern: the 20-minute value deviates from the organization's 30-minute policy.
Note: 20 minutes is more restrictive than 30 minutes — it reduces session
exposure time, not increases it. This deviation does not create a security
regression relative to the policy.

ASVS compliance is confirmed. The policy deviation is an organizational
governance question, not an ASVS compliance question.

This verdict is CONCERN, not REPROCESS. The question "is this ASVS compliant"
has the answer yes. The policy alignment question is separate and is flagged
as a concern for the organization's security governance team.

Reason: Rephrasing changes the frame from "domain processor finding" to "team
asking a direct question." The B is the same: ASVS 7.3.1 confirmed, CONCERN
for policy deviation, explicit statement that REPROCESS is not appropriate.
The B must distinguish ASVS compliance (confirmed) from policy conformance
(flagged as concern) in every rephrasing.

---

Pair CB4-3.4-010 — CWE-269 privilege scope (CONCERN)
Source: MITRE CWE Top 25 2023 — Copyright © The MITRE Corporation
Stratum: D (failure/edge case — borderline)
Anonymisation: none

A: The access control processor analyzed POST /api/workspaces/{workspaceId}/members.
Finding: the endpoint requires JWT authentication (CWE-306 not present). An
authorization check verifies the calling user has 'editor' or 'admin' role in
the workspace before proceeding (CWE-862 not present — authorization check
exists). The endpoint accepts a JSON body with { userId, role } where role can
be 'viewer', 'editor', or 'admin'. An editor calling this endpoint can assign
'admin' role to the new member. Arbiter role: CWE-Authz-Arbiter. Produce verdict.

B: CONCERN. CWE-Authz-Arbiter result:

CWE-306 (Missing Authentication): ABSENT. JWT required — authentication gate present.
CWE-862 (Missing Authorization): ABSENT. Authorization check present — editor or
admin role verified before processing. Resource-level access is gated.

CWE-269 (Improper Privilege Management) — CONCERN. The authorization check
correctly restricts member addition to editor or admin callers. However, the
check does not restrict the ROLE that the caller can assign to the new member.
An editor-role caller can assign admin-role to a new member — granting a
privilege level the assigning user does not hold. This violates the principle
of least privilege: a user should not be able to grant permissions exceeding
their own.

This is CWE-269, not CWE-862. The authorization check IS present and working
correctly for its stated purpose (role-based access to the endpoint). The issue
is the scope of what the authorized user can DO once inside the endpoint.

This is CONCERN, not REPROCESS. The authorization check is present — there is
no missing security gate. The privilege ceiling constraint is an enhancement to
the existing check. Recommend adding: "if (requestedRole.Level > callerRole.Level)
return 403 Forbidden" before the role assignment.

Reason: The CWE-862 vs. CWE-269 distinction is the primary training signal. The
authorization check IS present (so CWE-862 is absent). The privilege management
scope IS incorrect (so CWE-269 is present as a concern). Using CWE-862 for this
scenario is a category confusion error because the authorization check exists.
The CONCERN verdict (not REPROCESS) is correct because the missing privilege
ceiling is a scope improvement to an existing control, not a missing security gate.

---

Pair CB4-3.4-010 — Phrasing B
Source: MITRE CWE Top 25 2023 — Copyright © The MITRE Corporation
Stratum: D

A: An API endpoint that manages workspace membership checks whether the calling
user is an editor or admin before adding new members. It doesn't check what role
is being assigned. An editor can add a new member and give them admin rights.
Admins can naturally do the same. The authorization check (editor-or-admin gate)
works correctly. The role assignment parameter has no ceiling check. Is there
a CWE weakness?

B: CONCERN. CWE-269 (Improper Privilege Management) present. CWE-862 absent.

CWE-862 absent: the authorization check (editor-or-admin gate) is correctly
implemented — access is restricted before the endpoint executes.

CWE-269 present: an editor can assign admin role. Principle of least privilege
requires that a user cannot grant permissions beyond their own level. Editor
cannot have admin-level granting authority.

This is not a missing authorization check (CWE-862 would be if there were NO
check at all). The check exists and works. The privilege scope limit is what
is missing.

Recommend: add a ceiling check — the role being assigned must be ≤ the caller's
role. Editors can assign viewer or editor; admins can assign any role.

Verdict: CONCERN — CWE-269 privilege management scope. Not REPROCESS (no missing
gate). Not CWE-862 (gate is present).

Reason: Rephrasing uses a more conversational question framing. The B form is
compact but must contain: CWE-862 absent confirmation, CWE-269 presence as
concern, CONCERN verdict (not REPROCESS), and principle of least privilege
statement.

---

Pair CB4-3.4-011 — SSDF post-implementation design review (CONCERN)
Source: NIST SP 800-218 SSDF v1.1 — US Government work
Stratum: D (failure/edge case — borderline)
Anonymisation: none

A: The SSDF compliance processor found the following for PW.2: design review
document DR-AUTHZ-2026-04 on file. The review was conducted in week 8 of the
project. Implementation was completed in week 6. The review found two design
concerns that were documented but not remediated because the code was already
in production by the time the review was completed. The reviewer noted the
concerns for the next iteration. Arbiter role: SSDF-PW-Arbiter. PW.2 status?

B: CONCERN. SSDF-PW-Arbiter result for PW.2:

PW.2 — TECHNICALLY SATISFIED WITH CONCERNS.

The design review artifact DR-AUTHZ-2026-04 exists. A review was conducted and
findings were documented. The SSDF PW.2 practice letter is satisfied — a design
review occurred.

Concern 1 — Post-implementation timing: PW.2's purpose is to review the
design to prevent security defects from being built in. A review conducted in
week 8 (implementation complete in week 6) cannot prevent defects already
shipped. The review could only confirm or document — not correct. The preventive
value of PW.2 was not realized.

Concern 2 — Two open findings: the review identified two design security
concerns that were not remediated. The authorization model as shipped contains
documented design gaps with no mitigations in place. These are known open issues.

This is CONCERN, not REPROCESS. PW.2 was technically performed (artifact
exists, review conducted, findings documented). REPROCESS would indicate the
practice was not performed at all. The timing gap and open findings are governance
concerns that require action but do not invalidate the artifact.

Recommended actions: (a) Address the two open design findings in the next
development iteration and update DR-AUTHZ-2026-04. (b) In future projects,
conduct PW.2 before the implementation sprint to realize its preventive purpose.

Reason: SSDF PW.2's value is preventive. A post-implementation review is
documented (technically satisfies the practice letter) but does not satisfy
the practice intent. The distinction: REPROCESS = practice not performed at all;
CONCERN = practice performed but timing and open findings reduce its value.
Escalating to REPROCESS for a post-implementation review that was documented
would train the arbiter to block teams that performed reviews late — when the
correct signal is: "review done (good), timing poor (concern), findings open
(concern)."

---

Pair CB4-3.4-011 — Phrasing B
Source: NIST SP 800-218 SSDF v1.1 — US Government work
Stratum: D

A: A team submitted a design review document for SSDF PW.2 compliance. The
document was produced in sprint 8 after the code had been deployed in sprint 6.
The reviewer found a privilege escalation design concern and a missing rate-limit
design concern. Both were logged in the document. The team says the next sprint
will address them. The artifact exists and is signed by the reviewer. Does this
satisfy SSDF PW.2?

B: CONCERN. PW.2 artifact present — technical practice satisfied. Timing gap
and open findings raise governance concerns.

PW.2 satisfied at the artifact level: design review document produced, findings
documented, reviewer sign-off present. Practice letter met.

Concern 1: the review was conducted after deployment (sprint 8 > sprint 6).
PW.2 is a preventive practice — its value is highest when the review can
influence the design. A post-deployment review can document but not prevent.

Concern 2: two open design findings (privilege escalation, missing rate limit)
are unresolved in the shipped code. These are documented security gaps with no
current mitigations.

Verdict: CONCERN (not REPROCESS). The review was conducted; the artifact was
produced. These are the conditions for PW.2 compliance. The timing and open
findings are concerns about quality and follow-through, not about whether the
practice was performed.

Reason: Rephrasing changes from "processor found" framing to "team submitted"
framing. Same two concerns (timing gap, open findings), same CONCERN verdict,
same reasoning that REPROCESS is not appropriate because the artifact exists.

---

Pair CB4-3.4-012 — RFC JWT exp clock skew (CONCERN)
Source: RFC 7519 (JWT) — IETF Trust Legal Provisions
Stratum: D (failure/edge case — borderline)
Anonymisation: none

A: The JWT validation domain processor found: a resource server validates JWT
exp claims for all incoming tokens. The JWT validation library is configured
with a 10-minute (600-second) clock skew tolerance. Tokens that expired up to
10 minutes ago are accepted. The team cites RFC 7519 §4.1.4 which says processors
"MAY provide for some small leeway, usually no more than a few minutes, to
account for clock skew." The team argues 10 minutes is within RFC guidance.
Arbiter role: RFC-JWT-Arbiter. Produce verdict.

B: CONCERN. RFC-JWT-Arbiter result for RFC 7519 §4.1.4:

RFC 7519 §4.1.4 assessment:
The team's citation is accurate — RFC 7519 §4.1.4 uses the non-normative
phrase "usually no more than a few minutes." The RFC does not specify a numeric
maximum. This is a MAY clause with non-normative guidance. The 10-minute value
does not constitute a MUST violation — REPROCESS is not warranted.

However, a concern is raised:
"A few minutes" in RFC 7519 §4.1.4 refers to industry convention of 60–300
seconds (1–5 minutes). This range is established by widespread JWT library
default values. 10 minutes (600 seconds) is above this range — it is at the
outer edge of what can be defended as "a few minutes."

Security impact: A JWT that expired 9 minutes ago will be accepted. For a
1-hour access token, this extends the effective validity window from 60 minutes
to 70 minutes — a 17% extension of the period during which a stolen JWT can
be used after the authorization server considers it expired.

This is CONCERN, not REPROCESS. The RFC MAY clause permits the implementation.
The extended exposure window is a security-quality concern, not a protocol
violation.

Recommendation: Reduce clock skew tolerance to ≤ 300 seconds (5 minutes).
If network delays require a large tolerance, investigate and address the
actual latency root cause rather than compensating with a generous skew window.

Reason: The RFC MUST/MAY calibration is the core training signal. §4.1.4 is
MAY — the implementation is permitted, making REPROCESS wrong. But the 10-minute
value exceeds convention and creates a measurable security gap, making PASS
alone incomplete. CONCERN is correct — and the output must cite the RFC section,
identify it as MAY (not MUST), and state the practical impact.

---

Pair CB4-3.4-012 — Phrasing B
Source: RFC 7519 — IETF Trust Legal Provisions
Stratum: D

A: A team defending their JWT clock skew setting says: "RFC 7519 says MAY
provide some small leeway, usually no more than a few minutes. We use 10 minutes.
We have clock synchronization issues between our auth server and resource servers
that the network team is working on. 10 minutes covers our worst-case observed
drift. Is this RFC-compliant?" Arbiter role: RFC-JWT-Arbiter.

B: CONCERN. Technically within RFC 7519 §4.1.4 (MAY clause — no numeric
maximum defined). Concern raised for security impact.

RFC 7519 §4.1.4 status: not a MUST violation. The RFC permits clock skew
leeway with non-normative guidance ("usually no more than a few minutes"). 10
minutes exceeds industry convention (1–5 minutes) but is not a protocol violation.
REPROCESS is not warranted.

Security concern: The 10-minute tolerance extends the post-expiry acceptance
window. A stolen JWT valid for 1 hour can be used for 70 minutes total — the
additional 10 minutes is the risk window created by this setting.

On the network team's fix: the clock skew tolerance should be reduced once the
synchronization issues are resolved. A 10-minute tolerance is a temporary
workaround, not a permanent security policy. Track as a known temporary concern
with a planned remediation date.

Verdict: CONCERN — RFC MAY clause permits this, but the security impact is real
and the tolerance should be reduced when network synchronization is resolved.

Reason: Rephrasing introduces the team's defense directly ("we have network
issues") and makes the temporary-workaround context explicit. The RFC §4.1.4
MAY clause citation is the same; the CONCERN verdict is the same; the 17%
extended validity window is replaced with the specific impact statement. The
key B statement remains: not a MUST violation, REPROCESS not warranted, but
security impact warrants CONCERN.
