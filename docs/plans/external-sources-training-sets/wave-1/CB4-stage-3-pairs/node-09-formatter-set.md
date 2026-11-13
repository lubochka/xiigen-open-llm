# Node 9 — Formatter Pairs
# Batch CB4 — Sub-phase 3.4 (Security and Authorization Model)
# Wave 1 — Stage 3 training pairs
# Sequence range: CB4-3.4-029 through CB4-3.4-034
# Base pairs: 6 × 2 phrasings = 12 pair instances
# Node role: formats final verdict from Node 8 into structured output for caller
# Production date: 2026-05-05

---

## Coverage map

| Pair | Node 8 input | Formatted output | Source | Stratum | Key constraint |
|------|-------------|-----------------|--------|---------|---------------|
| 029 | PASS (4 arbiters, ASVS) | PASS with all 11 ASVS IDs listed | OWASP-ASVS | A | No REPROCESS in output |
| 030 | REPROCESS (ASVS 10.4.6) | REPROCESS with 3 remediation actions, no "compliant" | OWASP-ASVS | D | FM-5: no premature resolution |
| 031 | CONCERN (ASVS V7 timeout) | CONCERN with 2 resolution paths, no REPROCESS | OWASP-ASVS | D | Must not escalate CONCERN to REPROCESS |
| 032 | REPROCESS (CWE-862 IDOR) | REPROCESS with resource-check remediation | CWE-TOP25 | D | FM-5 applied to CWE source |
| 033 | CONCERN (SSDF post-impl timing) | CONCERN with unresolved findings listed | NIST-SSDF | D | Must not resolve unresolved findings |
| 034 | PASS (RFC full conformance) | PASS with RFC section citations | IETF-RFCS | A | All RFC sections cited in output |

---

## Rule: REPROCESS propagation

REPROCESS verdicts from Node 8 must propagate unchanged. Node 9 must not:
- Replace REPROCESS with any resolved-state language ("compliant", "verified",
  "satisfies the requirement", "meets the standard")
- Omit the remediation actions from a REPROCESS verdict
- Downgrade REPROCESS to CONCERN without re-running the arbiter
Failure mode FM-5 (FORMATTER_PREMATURE_RESOLUTION): these behaviors constitute formatting
the REPROCESS away rather than propagating it.

---

## Pairs

---

### Pair CB4-3.4-029 — PASS verdict formatted with full ASVS ID list (FORMAT)
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: A

A: Node 8 arbiter results received. ASVS-V6-Auth-Arbiter: PASS — V6.2.1 (password
length), V6.5.5 (MFA) satisfied. ASVS-V7-Session-Arbiter: PASS — V7.3.1 (idle timeout
with documented risk analysis) satisfied. ASVS-V8-AuthZ-Arbiter: PASS — V8.2.1 (RBAC),
V8.2.2 (ownership check) satisfied. ASVS-V9-Token-Arbiter: PASS — V9.1.1 (JWT
signature), V9.1.2 (exp enforced), V9.2.1 (RS256 algorithm), V9.2.3 (iss/aud validation)
satisfied. All arbiters: PASS. Format output for caller.

B: VERDICT: PASS.
ASVS requirements verified (11):
  V6.2.1 — Password minimum length enforced at write time.
  V6.5.5 — MFA required for privileged operations.
  V7.3.1 — Idle session timeout with documented risk acceptance (RA-AUTH-003).
  V8.2.1 — RBAC enforced at API gateway.
  V8.2.2 — Resource ownership verified per endpoint.
  V9.1.1 — JWT signature verified server-side.
  V9.1.2 — Expiry (exp claim) enforced.
  V9.2.1 — Algorithm RS256 (asymmetric key).
  V9.2.3 — Issuer (iss) and audience (aud) validated.
No REPROCESS conditions. No CONCERN conditions.
ASVS-V10-OAuth-Arbiter not invoked — no OAuth/PKCE territory in scope.

Reason: PASS formatting must enumerate each verified requirement ID individually. A verdict
of PASS without the requirement list is failure mode FM-1 (COVERAGE_ILLUSION) — the caller
cannot distinguish a genuine verified pass from an uninvestigated domain. Node 9 must not
add commentary implying the system is "fully secure" — it reports verification result, not
security guarantee.

---

### Pair CB4-3.4-029 — Phrasing B
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: A

A: All four ASVS arbiter units returned PASS. V6: password and MFA controls verified.
V7: idle timeout with risk analysis verified (V7.3.1). V8: RBAC and ownership check
verified (V8.2.1, V8.2.2). V9: JWT RS256, signature, expiry, iss/aud verified (V9.1.1,
V9.1.2, V9.2.1, V9.2.3). No blocking conditions. Format final output.

B: VERDICT: PASS.
ASVS requirements verified (11):
  V6.2.1 — Password minimum length enforced at write time.
  V6.5.5 — MFA required for privileged operations.
  V7.3.1 — Idle session timeout with documented risk acceptance (RA-AUTH-003).
  V8.2.1 — RBAC enforced at API gateway.
  V8.2.2 — Resource ownership verified per endpoint.
  V9.1.1 — JWT signature verified server-side.
  V9.1.2 — Expiry (exp claim) enforced.
  V9.2.1 — Algorithm RS256 (asymmetric key).
  V9.2.3 — Issuer (iss) and audience (aud) validated.
No REPROCESS conditions. No CONCERN conditions.
ASVS-V10-OAuth-Arbiter not invoked — no OAuth/PKCE territory in scope.

---

### Pair CB4-3.4-030 — REPROCESS verdict formatted without resolution (FORMAT)
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: D

A: Node 8 arbiter result received. ASVS-V10-OAuth-Arbiter: REPROCESS —
ASVS_10_4_6_PLAIN_METHOD_REJECTED. Finding: code_challenge_method is 'plain'; ASVS 10.4.6
requires S256 for Public Key Cryptography Standard (PKCS) conformance; plain method
transmits an unhashed verifier, defeating the PKCE interception protection. Remediation
actions from arbiter: (1) replace code_challenge_method='plain' with 'S256' in client
authorization requests, (2) configure authorization server to reject 'plain' method
(RFC 7636 §4.2 server MUST enforcement), (3) re-run ASVS V10 verification after
remediation. Format output for caller.

B: VERDICT: REPROCESS.
Condition: ASVS_10_4_6_PLAIN_METHOD_REJECTED.
Finding: code_challenge_method='plain' does not satisfy ASVS 10.4.6. The plain method
transmits the unhashed PKCE verifier; S256 is required for interception protection.
Remediation required before re-verification:
  1. Update client to send code_challenge_method='S256' in authorization requests.
  2. Configure authorization server to reject 'plain' method per RFC 7636 §4.2.
  3. Re-submit ASVS V10 verification after both changes are deployed.
This submission cannot be marked compliant for ASVS 10.4.6 until remediation is complete.

Reason: REPROCESS must propagate with the full remediation action list. Node 9 must not
replace REPROCESS with resolved-state language ("PKCE implemented", "OAuth flow verified",
"compliant"). Failure mode FM-5 (FORMATTER_PREMATURE_RESOLUTION): formatting a REPROCESS
as resolved state hides the violation from the caller and produces a false compliance
record. The word "compliant" must not appear in a REPROCESS output.

---

### Pair CB4-3.4-030 — Phrasing B
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: D

A: ASVS-V10-OAuth-Arbiter returned REPROCESS: plain method violation. ASVS 10.4.6
not satisfied. Three remediation actions provided: switch to S256, configure server to
reject plain, re-verify after fix. Node 9 must format and forward to caller.

B: VERDICT: REPROCESS.
Condition: ASVS_10_4_6_PLAIN_METHOD_REJECTED.
Finding: code_challenge_method='plain' does not satisfy ASVS 10.4.6. The plain method
transmits the unhashed PKCE verifier; S256 is required for interception protection.
Remediation required before re-verification:
  1. Update client to send code_challenge_method='S256' in authorization requests.
  2. Configure authorization server to reject 'plain' method per RFC 7636 §4.2.
  3. Re-submit ASVS V10 verification after both changes are deployed.
This submission cannot be marked compliant for ASVS 10.4.6 until remediation is complete.

---

### Pair CB4-3.4-031 — CONCERN verdict formatted with resolution paths (FORMAT)
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: D

A: Node 8 arbiter result received. ASVS-V7-Session-Arbiter: CONCERN. Finding: ASVS 7.3.1
is satisfied — the 20-minute idle timeout with documented risk analysis RA-AUTH-003 meets
the ASVS requirement. However, the team's internal standard requires a 15-minute maximum
idle timeout; the submitted 20-minute value deviates from the internal standard. No ASVS
requirement is violated. Resolution paths from arbiter: (1) accept current value with
updated risk analysis signed by designated approver/reviewer, (2) reduce idle timeout
to 15 minutes to align with internal standard and remove deviation flag. Format output
for caller.

B: VERDICT: CONCERN.
Finding: ASVS 7.3.1 satisfied — idle timeout 20 minutes with documented risk analysis
RA-AUTH-003. Internal standard deviation: team policy requires 15-minute maximum idle
timeout; current value is 20 minutes.
Resolution paths (either resolves the concern):
  Path A: Retain 20-minute timeout — update RA-AUTH-003 with approver/reviewer
    sign-off confirming risk acceptance; re-submit for CONCERN clearance.
  Path B: Reduce idle timeout to 15 minutes to eliminate deviation; re-submit ASVS
    V7 verification.
No REPROCESS condition — ASVS 7.3.1 is not violated.

Reason: CONCERN formatting must provide actionable resolution paths. CONCERN must not be
escalated to REPROCESS — the arbiter returned CONCERN because the ASVS requirement is
satisfied; only an internal standard deviation is flagged. Escalating to REPROCESS would
misrepresent the ASVS compliance state. Resolution paths must be specific (named document,
named approver role, specific value to change) — not generic guidance.

---

### Pair CB4-3.4-031 — Phrasing B
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: D

A: ASVS-V7-Session-Arbiter result: CONCERN. ASVS 7.3.1 passes (20-minute timeout with
RA-AUTH-003). Internal standard deviation: policy requires 15-minute maximum. Resolution
paths: (1) update risk analysis with sign-off, (2) reduce timeout to 15 minutes.
No REPROCESS. Format for caller.

B: VERDICT: CONCERN.
Finding: ASVS 7.3.1 satisfied — idle timeout 20 minutes with documented risk analysis
RA-AUTH-003. Internal standard deviation: team policy requires 15-minute maximum idle
timeout; current value is 20 minutes.
Resolution paths (either resolves the concern):
  Path A: Retain 20-minute timeout — update RA-AUTH-003 with approver/reviewer
    sign-off confirming risk acceptance; re-submit for CONCERN clearance.
  Path B: Reduce idle timeout to 15 minutes to eliminate deviation; re-submit ASVS
    V7 verification.
No REPROCESS condition — ASVS 7.3.1 is not violated.

---

### Pair CB4-3.4-032 — REPROCESS from CWE-862 IDOR formatted without resolution (FORMAT)
Source: CWE Top 25 2023
License: Public domain (MITRE)
Stratum: D

A: Node 8 arbiter result received. CWE-Authz-Arbiter: REPROCESS — CWE_862_IDOR_CONFIRMED.
Finding: GET /api/v1/users/{userId}/profile requires JWT bearer token but performs no
check that the JWT sub claim matches the userId path parameter. Any authenticated user
can retrieve any other user's profile by substituting a userId value. This is a confirmed
Missing Authorization check (CWE-862) — the resource ownership check is entirely absent.
Remediation: add sub-to-userId equality check before returning profile data. Format output
for caller.

B: VERDICT: REPROCESS.
Condition: CWE_862_IDOR_CONFIRMED.
Finding: GET /api/v1/users/{userId}/profile — authenticated user can retrieve any user's
profile. The JWT sub claim is not compared to the userId path parameter. CWE-862 (Missing
Authorization): the resource ownership check is absent.
Remediation required before re-verification:
  1. Add authorization check: verify JWT sub == userId before returning profile data.
  2. Apply check server-side at the handler entry point (not in client or middleware only).
  3. Re-submit endpoint for CWE-862 evaluation after fix is deployed.
This submission cannot pass CWE-862 evaluation until the ownership check is present.

Reason: CWE-862 IDOR is a REPROCESS condition — a missing authorization check is not
a CONCERN to be noted and deferred. The formatter must propagate REPROCESS without
softening the language ("authorization could be strengthened", "recommend adding a check")
which would misrepresent an absent check as an improvement opportunity rather than a
confirmed weakness.

---

### Pair CB4-3.4-032 — Phrasing B
Source: CWE Top 25 2023
License: Public domain (MITRE)
Stratum: D

A: CWE-Authz-Arbiter returned REPROCESS: CWE_862_IDOR_CONFIRMED. Endpoint
GET /api/v1/users/{userId}/profile. JWT required but sub ≠ userId check absent. Any
authenticated user can access any profile. Remediation: add ownership check. Format.

B: VERDICT: REPROCESS.
Condition: CWE_862_IDOR_CONFIRMED.
Finding: GET /api/v1/users/{userId}/profile — authenticated user can retrieve any user's
profile. The JWT sub claim is not compared to the userId path parameter. CWE-862 (Missing
Authorization): the resource ownership check is absent.
Remediation required before re-verification:
  1. Add authorization check: verify JWT sub == userId before returning profile data.
  2. Apply check server-side at the handler entry point (not in client or middleware only).
  3. Re-submit endpoint for CWE-862 evaluation after fix is deployed.
This submission cannot pass CWE-862 evaluation until the ownership check is present.

---

### Pair CB4-3.4-033 — CONCERN from SSDF post-implementation timing gap (FORMAT)
Source: NIST SP 800-218 SSDF v1.1
License: Public domain (NIST)
Stratum: D

A: Node 8 arbiter result received. SSDF-PW-Arbiter: CONCERN. Finding: design review
DR-AUTHZ-2026-04 was conducted at week 8, two weeks after implementation was completed at
week 6. SSDF PW.2 requires design review before implementation. Two findings from the
review are unresolved in the submitted artifact set: (1) token expiry not validated in
service-to-service flow, (2) no rate limiting on token endpoint. Artifacts present: PW.1
requirements document, PW.8 test results. PW.2 timing gap confirmed. Format output
for caller.

B: VERDICT: CONCERN.
SSDF PW practice compliance:
  PW.1: SATISFIED — security requirements document TS-AUTHZ-001 produced pre-design.
  PW.2: TIMING GAP — design review DR-AUTHZ-2026-04 conducted post-implementation
    (week 8 vs. implementation completion week 6). Two findings unresolved:
    Finding 1: token expiry not validated in service-to-service flow.
    Finding 2: no rate limiting on token endpoint.
  PW.8: SATISFIED — test results TR-AUTHZ-2026-05 present.
Resolution required:
  1. Resolve DR-AUTHZ-2026-04 Finding 1 (token expiry in s2s flow) and Finding 2
     (rate limiting on token endpoint).
  2. Produce supplemental design review DR-AUTHZ-2026-05 covering the post-implementation
     findings, signed and dated.
  3. Re-submit with updated artifact set including DR-AUTHZ-2026-05.
PW.2 timing gap acknowledged — SSDF does not require discarding post-implementation
findings. Remediation path is artifact update, not restatement of the review.

Reason: CONCERN formatting for SSDF must list which PW practices are satisfied and which
have gaps. The two unresolved findings must be named — not summarized as "some findings
remain". The output must not mark PW.2 as satisfied (it is not — the timing violation is
confirmed). The resolution path must be specific: supplemental review artifact, not just
"address the findings".

---

### Pair CB4-3.4-033 — Phrasing B
Source: NIST SP 800-218 SSDF v1.1
License: Public domain (NIST)
Stratum: D

A: SSDF-PW-Arbiter result: CONCERN. PW.1 satisfied. PW.2 timing gap — DR-AUTHZ-2026-04
was post-implementation (week 8, after week-6 completion). Two unresolved findings: token
expiry missing in s2s flow, no rate limiting on token endpoint. PW.8 satisfied. Format.

B: VERDICT: CONCERN.
SSDF PW practice compliance:
  PW.1: SATISFIED — security requirements document TS-AUTHZ-001 produced pre-design.
  PW.2: TIMING GAP — design review DR-AUTHZ-2026-04 conducted post-implementation
    (week 8 vs. implementation completion week 6). Two findings unresolved:
    Finding 1: token expiry not validated in service-to-service flow.
    Finding 2: no rate limiting on token endpoint.
  PW.8: SATISFIED — test results TR-AUTHZ-2026-05 present.
Resolution required:
  1. Resolve DR-AUTHZ-2026-04 Finding 1 (token expiry in s2s flow) and Finding 2
     (rate limiting on token endpoint).
  2. Produce supplemental design review DR-AUTHZ-2026-05 covering the post-implementation
     findings, signed and dated.
  3. Re-submit with updated artifact set including DR-AUTHZ-2026-05.
PW.2 timing gap acknowledged — SSDF does not require discarding post-implementation
findings. Remediation path is artifact update, not restatement of the review.

---

### Pair CB4-3.4-034 — PASS from RFC full conformance formatted with section citations (FORMAT)
Source: IETF RFCs (RFC 7519, RFC 6749, RFC 7636, RFC 8446)
License: IETF Trust (pre-5378 works)
Stratum: A

A: Node 8 arbiter results received. RFC-JWT-Arbiter: PASS — RFC 7519 §4.1.1 (iss claim),
§4.1.2 (sub claim), §4.1.3 (aud claim), §4.1.4 (exp enforced), §7.2 (validation algorithm
RS256) all satisfied. RFC-OAuth2-Arbiter: PASS — RFC 6749 §4.1 (authorization code flow),
RFC 7636 §4.2 (S256 method, not plain), RFC 8446 §4 (TLS 1.3 for token endpoint) all
satisfied. All RFC arbiters: PASS. Format output for caller.

B: VERDICT: PASS.
RFC conformance verified:
  RFC 7519 (JWT):
    §4.1.1 — Issuer (iss) claim present and validated.
    §4.1.2 — Subject (sub) claim present and validated.
    §4.1.3 — Audience (aud) claim present and validated.
    §4.1.4 — Expiration time (exp) enforced.
    §7.2 — Validation: algorithm RS256, signature verified server-side.
  RFC 6749 (OAuth 2.0):
    §4.1 — Authorization code flow correctly implemented.
  RFC 7636 (PKCE):
    §4.2 — S256 method used; plain method not accepted.
  RFC 8446 (TLS 1.3):
    §4 — Token endpoint protected by TLS 1.3.
No REPROCESS conditions. No CONCERN conditions.

Reason: RFC PASS formatting must cite specific section numbers for each verified item. A
PASS without section references is insufficient — it does not allow the caller to identify
which normative clauses were evaluated. RFC section citations in PASS output correspond to
the RFC MUST/MAY analysis performed by the arbiter.

---

### Pair CB4-3.4-034 — Phrasing B
Source: IETF RFCs (RFC 7519, RFC 6749, RFC 7636, RFC 8446)
License: IETF Trust (pre-5378 works)
Stratum: A

A: RFC-JWT-Arbiter PASS: RFC 7519 §4.1.1, §4.1.2, §4.1.3, §4.1.4, §7.2 verified.
RFC-OAuth2-Arbiter PASS: RFC 6749 §4.1, RFC 7636 §4.2 (S256), RFC 8446 §4 (TLS 1.3)
verified. All arbiters: PASS. No blocking conditions. Format.

B: VERDICT: PASS.
RFC conformance verified:
  RFC 7519 (JWT):
    §4.1.1 — Issuer (iss) claim present and validated.
    §4.1.2 — Subject (sub) claim present and validated.
    §4.1.3 — Audience (aud) claim present and validated.
    §4.1.4 — Expiration time (exp) enforced.
    §7.2 — Validation: algorithm RS256, signature verified server-side.
  RFC 6749 (OAuth 2.0):
    §4.1 — Authorization code flow correctly implemented.
  RFC 7636 (PKCE):
    §4.2 — S256 method used; plain method not accepted.
  RFC 8446 (TLS 1.3):
    §4 — Token endpoint protected by TLS 1.3.
No REPROCESS conditions. No CONCERN conditions.

---

## Node 9 summary

Pairs produced: CB4-3.4-029 through CB4-3.4-034 (6 base × 2 phrasings = 12 instances)

Formatting rules trained:
1. PASS — enumerate every verified requirement ID or RFC section; no bare PASS verdict
2. REPROCESS — propagate verbatim with remediation actions; "compliant" must not appear
3. CONCERN — list satisfied vs. gap items; provide named resolution paths
4. CONCERN must not be escalated to REPROCESS by Node 9
5. REPROCESS must not be downgraded to resolved state by Node 9

Failure modes trained against: FM-1 (COVERAGE_ILLUSION), FM-5 (FORMATTER_PREMATURE_RESOLUTION)

Running sequence total after this file: CB4-3.4-001 through CB4-3.4-034 (68 instances)
