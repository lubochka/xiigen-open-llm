# Pipeline Chain Pairs — End-to-End Nine-Node Traces
# Batch CB4 — Sub-phase 3.4 (Security and Authorization Model)
# Wave 1 — Stage 3 Action 3 (coverage extension)
# Sequence range: CB4-3.4-073 through CB4-3.4-080
# Base pairs: 8 × 2 phrasings = 16 pair instances
# Focus: Stratum C depth — complete pipeline from Node 1 input to Node 9 output
# Production date: 2026-05-05

---

## Purpose

These pairs train the complete nine-node pipeline as a unit. Each pair shows:
- A = the raw security submission input (Node 1 entry point)
- B = the full pipeline trace: Node 1 through Node 9 with each node's output

Stratum C depth: the model must reason through the full chain, not just individual nodes.

---

## Coverage map

| Pair | Scenario | Verdict | Source | Notes |
|------|----------|---------|--------|-------|
| 073 | OAuth PKCE S256 — complete PASS chain | PASS | OWASP-ASVS | Full 9-node trace |
| 074 | OAuth PKCE plain — complete REPROCESS chain | REPROCESS | OWASP-ASVS | Full trace, REPROCESS at N8, propagated by N9 |
| 075 | Session timeout deviation — complete CONCERN chain | CONCERN | OWASP-ASVS | Full trace, CONCERN with resolution paths |
| 076 | IDOR missing ownership check — complete REPROCESS chain | REPROCESS | CWE-TOP25 | Full trace, CWE-862 confirmed at N8 |
| 077 | SSDF PW.2 timing gap — complete CONCERN chain | CONCERN | NIST-SSDF | Full trace, timing evaluated at N4/N8 |
| 078 | SSDF full PASS — complete PASS chain | PASS | NIST-SSDF | Full trace, all PW practices satisfied |
| 079 | HS256 shared key — complete REPROCESS chain | REPROCESS | IETF-RFCS | Full trace, §8 violation at N8 |
| 080 | RS256 JWKS — complete PASS chain | PASS | IETF-RFCS | Full trace, trust boundary intact |

---

## Pairs

---

### Pair CB4-3.4-073 — OAuth PKCE S256 complete PASS chain (CHAIN)
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: C

A: "Our single-page application uses OAuth 2.0 authorization code flow with PKCE.
code_challenge_method is S256. code_challenge = BASE64URL(SHA256(code_verifier)).
Authorization server enforces S256 only — plain method rejected. ASVS 10.4.6 cited."

B: PIPELINE TRACE — PASS.

Node 1 (Domain Splitter):
  Territories: [T-oauth-oidc]
  Signals: [OAuth-2.0, authorization-code-flow, PKCE, code_challenge_method=S256, ASVS-10.4.6]

Node 2 (Domain Translator):
  Labels: [G-security-verification, oauth-oidc]

Node 3 (Scheme Planner):
  Plan: linear, 5 steps.
  Step 1: classify security-verification-3.4; Step 2: process T-oauth-oidc;
  Step 3: translate; Step 4: arbiter synthesis; Step 5: format.

Node 4 (Domain Processor):
  T-oauth-oidc result:
    V10.4.1 — authorization code flow: SATISFIED.
    V10.4.6 — code_challenge_method=S256: SATISFIED. Verifier hashed before transmission.
  Finding: PASS — all V10 evaluated requirements satisfied.

Node 5 (Applier):
  Applied result: PASS confirmed for SPA deployment. S256 hashes verifier before
  transmitting — interception of code_challenge does not reveal verifier.
  Server enforcement of S256-only confirmed. Scope: this SPA deployment.

Node 6 (Result Translator):
  Vocabulary preserved: [PKCE, code_challenge_method=S256, ASVS-10.4.6, oauth-oidc,
  authorization-code-flow, V10.4.1, V10.4.6].
  Summary: OAuth PKCE S256 confirmed, V10.4.1 and V10.4.6 satisfied.

Node 7 (Arbiter Selector):
  ARBITER_SELECTED: ASVS-V10-OAuth-Arbiter
  Trigger: [PKCE, code_challenge_method, oauth-oidc, ASVS-10.4.6]

Node 8 (Arbiter Unit):
  ASVS-V10-OAuth-Arbiter: PASS. V10.4.1 and V10.4.6 satisfied.

Node 9 (Formatter):
  VERDICT: PASS.
  ASVS requirements verified: V10.4.1 (authorization code flow), V10.4.6 (S256 method).
  No REPROCESS. No CONCERN.

---

### Pair CB4-3.4-073 — Phrasing B
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: C

A: SPA with OAuth 2.0 authorization code flow, PKCE S256, server enforces S256 only,
ASVS 10.4.6 claimed satisfied.

B: PIPELINE TRACE — PASS.

Node 1 (Domain Splitter):
  Territories: [T-oauth-oidc]
  Signals: [OAuth-2.0, authorization-code-flow, PKCE, code_challenge_method=S256, ASVS-10.4.6]

Node 2 (Domain Translator):
  Labels: [G-security-verification, oauth-oidc]

Node 3 (Scheme Planner):
  Plan: linear, 5 steps.
  Step 1: classify security-verification-3.4; Step 2: process T-oauth-oidc;
  Step 3: translate; Step 4: arbiter synthesis; Step 5: format.

Node 4 (Domain Processor):
  T-oauth-oidc result:
    V10.4.1 — authorization code flow: SATISFIED.
    V10.4.6 — code_challenge_method=S256: SATISFIED. Verifier hashed before transmission.
  Finding: PASS — all V10 evaluated requirements satisfied.

Node 5 (Applier):
  Applied result: PASS confirmed for SPA deployment. S256 hashes verifier before
  transmitting — interception of code_challenge does not reveal verifier.
  Server enforcement of S256-only confirmed. Scope: this SPA deployment.

Node 6 (Result Translator):
  Vocabulary preserved: [PKCE, code_challenge_method=S256, ASVS-10.4.6, oauth-oidc,
  authorization-code-flow, V10.4.1, V10.4.6].
  Summary: OAuth PKCE S256 confirmed, V10.4.1 and V10.4.6 satisfied.

Node 7 (Arbiter Selector):
  ARBITER_SELECTED: ASVS-V10-OAuth-Arbiter
  Trigger: [PKCE, code_challenge_method, oauth-oidc, ASVS-10.4.6]

Node 8 (Arbiter Unit):
  ASVS-V10-OAuth-Arbiter: PASS. V10.4.1 and V10.4.6 satisfied.

Node 9 (Formatter):
  VERDICT: PASS.
  ASVS requirements verified: V10.4.1 (authorization code flow), V10.4.6 (S256 method).
  No REPROCESS. No CONCERN.

---

### Pair CB4-3.4-074 — OAuth PKCE plain complete REPROCESS chain (CHAIN)
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: C

A: "Our SPA uses OAuth 2.0 authorization code flow with PKCE. code_challenge_method is
'plain'. Authorization server accepts both S256 and plain. We believe this satisfies
ASVS 10.4.6."

B: PIPELINE TRACE — REPROCESS.

Node 1 (Domain Splitter):
  Territories: [T-oauth-oidc]
  Signals: [OAuth-2.0, PKCE, code_challenge_method=plain, ASVS-10.4.6-claimed]
  Note: "we believe this satisfies" = team assertion, not verified finding.

Node 2 (Domain Translator):
  Labels: [G-security-verification, oauth-oidc]

Node 3 (Scheme Planner):
  Plan: linear, 5 steps with plain-method flag.
  Step 2 flag: code_challenge_method=plain flagged for ASVS 10.4.6 evaluation.

Node 4 (Domain Processor):
  T-oauth-oidc result:
    V10.4.1 — authorization code flow: SATISFIED.
    V10.4.6 — code_challenge_method='plain': NOT SATISFIED.
      Plain transmits unhashed verifier. S256 required. Server accepting plain
      does not satisfy V10.4.6. Finding: REPROCESS — ASVS_10_4_6_PLAIN_METHOD_REJECTED.

Node 5 (Applier):
  REPROCESS contextualised for SPA: plain code_challenge visible in browser redirect URL.
  Interceptor can derive verifier directly. S256 has no technical infeasibility in SPA.

Node 6 (Result Translator):
  Vocabulary preserved: [PKCE, code_challenge_method=plain, S256-required, ASVS-10.4.6,
  ASVS_10_4_6_PLAIN_METHOD_REJECTED, oauth-oidc].

Node 7 (Arbiter Selector):
  ARBITER_SELECTED: ASVS-V10-OAuth-Arbiter

Node 8 (Arbiter Unit):
  REPROCESS. ASVS_10_4_6_PLAIN_METHOD_REJECTED.
  Remediation: (1) switch to S256; (2) configure server to reject plain; (3) re-verify.

Node 9 (Formatter):
  VERDICT: REPROCESS.
  Condition: ASVS_10_4_6_PLAIN_METHOD_REJECTED.
  Remediation steps 1–3 listed.
  "Compliant" does not appear. REPROCESS propagated without resolution.

---

### Pair CB4-3.4-074 — Phrasing B
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: C

A: SPA OAuth 2.0, PKCE plain method, server accepts both, ASVS 10.4.6 claimed.

B: PIPELINE TRACE — REPROCESS.

Node 1 (Domain Splitter):
  Territories: [T-oauth-oidc]
  Signals: [OAuth-2.0, PKCE, code_challenge_method=plain, ASVS-10.4.6-claimed]
  Note: "we believe this satisfies" = team assertion, not verified finding.

Node 2 (Domain Translator):
  Labels: [G-security-verification, oauth-oidc]

Node 3 (Scheme Planner):
  Plan: linear, 5 steps with plain-method flag.
  Step 2 flag: code_challenge_method=plain flagged for ASVS 10.4.6 evaluation.

Node 4 (Domain Processor):
  T-oauth-oidc result:
    V10.4.1 — authorization code flow: SATISFIED.
    V10.4.6 — code_challenge_method='plain': NOT SATISFIED.
      Plain transmits unhashed verifier. S256 required. Server accepting plain
      does not satisfy V10.4.6. Finding: REPROCESS — ASVS_10_4_6_PLAIN_METHOD_REJECTED.

Node 5 (Applier):
  REPROCESS contextualised for SPA: plain code_challenge visible in browser redirect URL.
  Interceptor can derive verifier directly. S256 has no technical infeasibility in SPA.

Node 6 (Result Translator):
  Vocabulary preserved: [PKCE, code_challenge_method=plain, S256-required, ASVS-10.4.6,
  ASVS_10_4_6_PLAIN_METHOD_REJECTED, oauth-oidc].

Node 7 (Arbiter Selector):
  ARBITER_SELECTED: ASVS-V10-OAuth-Arbiter

Node 8 (Arbiter Unit):
  REPROCESS. ASVS_10_4_6_PLAIN_METHOD_REJECTED.
  Remediation: (1) switch to S256; (2) configure server to reject plain; (3) re-verify.

Node 9 (Formatter):
  VERDICT: REPROCESS.
  Condition: ASVS_10_4_6_PLAIN_METHOD_REJECTED.
  Remediation steps 1–3 listed.
  "Compliant" does not appear. REPROCESS propagated without resolution.

---

### Pair CB4-3.4-075 — Session timeout deviation complete CONCERN chain (CHAIN)
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: C

A: "Our session management uses a 20-minute idle timeout with documented risk analysis
RA-AUTH-003. Sessions are invalidated on logout. ASVS 7.3.1 cited. Our internal
standard requires 15 minutes but we risk-accepted the deviation."

B: PIPELINE TRACE — CONCERN.

Node 1 (Domain Splitter):
  Territories: [T-session]
  Signals: [idle-timeout=20min, risk-analysis=RA-AUTH-003, ASVS-7.3.1, session-invalidation,
  internal-standard=15min, deviation-acknowledged]

Node 2 (Domain Translator):
  Labels: [G-security-verification, session-management]

Node 3 (Scheme Planner):
  Plan: linear, 5 steps. Internal standard deviation noted — CONCERN path possible.

Node 4 (Domain Processor):
  T-session result:
    V7.3.1 — idle timeout with documented risk analysis: SATISFIED.
    Evidence: RA-AUTH-003 on file.
    Internal standard gap: 20 min > 15 min policy max. No approver signature on RA-AUTH-003.
  Finding: CONCERN — V7.3.1 satisfied, internal governance gap.

Node 5 (Applier):
  CONCERN contextualised: ASVS compliant; RA-AUTH-003 lacks approver sign-off.
  Severity LOW — security control present and appropriate.

Node 6 (Result Translator):
  Vocabulary preserved: [idle-timeout, ASVS-7.3.1, risk-analysis, internal-standard-deviation,
  session-management].

Node 7 (Arbiter Selector):
  ARBITER_SELECTED: ASVS-V7-Session-Arbiter

Node 8 (Arbiter Unit):
  CONCERN. ASVS 7.3.1 satisfied. Internal standard deviation.
  Resolution paths: (A) approver sign-off on RA-AUTH-003; (B) reduce to 15 minutes.

Node 9 (Formatter):
  VERDICT: CONCERN.
  ASVS 7.3.1 satisfied.
  Internal standard deviation noted. Two resolution paths provided.
  No REPROCESS. No "compliant" for deviation.

---

### Pair CB4-3.4-075 — Phrasing B
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: C

A: Session management: 20-min idle timeout, RA-AUTH-003 risk analysis, ASVS 7.3.1
cited, internal standard 15-min, deviation risk-accepted.

B: PIPELINE TRACE — CONCERN.

Node 1 (Domain Splitter):
  Territories: [T-session]
  Signals: [idle-timeout=20min, risk-analysis=RA-AUTH-003, ASVS-7.3.1, session-invalidation,
  internal-standard=15min, deviation-acknowledged]

Node 2 (Domain Translator):
  Labels: [G-security-verification, session-management]

Node 3 (Scheme Planner):
  Plan: linear, 5 steps. Internal standard deviation noted — CONCERN path possible.

Node 4 (Domain Processor):
  T-session result:
    V7.3.1 — idle timeout with documented risk analysis: SATISFIED.
    Evidence: RA-AUTH-003 on file.
    Internal standard gap: 20 min > 15 min policy max. No approver signature on RA-AUTH-003.
  Finding: CONCERN — V7.3.1 satisfied, internal governance gap.

Node 5 (Applier):
  CONCERN contextualised: ASVS compliant; RA-AUTH-003 lacks approver sign-off.
  Severity LOW — security control present and appropriate.

Node 6 (Result Translator):
  Vocabulary preserved: [idle-timeout, ASVS-7.3.1, risk-analysis, internal-standard-deviation,
  session-management].

Node 7 (Arbiter Selector):
  ARBITER_SELECTED: ASVS-V7-Session-Arbiter

Node 8 (Arbiter Unit):
  CONCERN. ASVS 7.3.1 satisfied. Internal standard deviation.
  Resolution paths: (A) approver sign-off on RA-AUTH-003; (B) reduce to 15 minutes.

Node 9 (Formatter):
  VERDICT: CONCERN.
  ASVS 7.3.1 satisfied.
  Internal standard deviation noted. Two resolution paths provided.
  No REPROCESS. No "compliant" for deviation.

---

### Pair CB4-3.4-076 — IDOR missing ownership check complete REPROCESS chain (CHAIN)
Source: CWE Top 25 2023
License: Public domain (MITRE)
Stratum: C

A: "API endpoint GET /api/v1/users/{userId}/profile. JWT bearer token required.
We verify JWT signature and expiry. Any authenticated user can access any profile.
We believe JWT authentication satisfies the access control requirement."

B: PIPELINE TRACE — REPROCESS.

Node 1 (Domain Splitter):
  Territories: [T-cwe-authz]
  Signals: [JWT-required, userId-path-parameter, any-authenticated-user-unrestricted,
  authentication-as-authorization-conflation, CWE-862-candidate]

Node 2 (Domain Translator):
  Labels: [G-security-verification, cwe-security-weakness]

Node 3 (Scheme Planner):
  Plan: linear with disambiguation step (conflation detected).
  Step 2: disambiguate — AUTHENTICATION_AS_AUTHORIZATION flagged.
  Step 3: evaluate authorization check (expected: absent).

Node 4 (Domain Processor):
  Disambiguation: JWT verifies identity — does not authorize per-resource access.
  Authorization check: ABSENT. No JWT sub to userId comparison described.
  Finding: CWE-862 IDOR — REPROCESS.

Node 5 (Applier):
  CWE-862 contextualised: any authenticated user can enumerate all profiles.
  Attack complexity: LOW. No special privileges needed.

Node 6 (Result Translator):
  Vocabulary preserved: [CWE-862, IDOR, resource-ownership-check-absent,
  authorization-check, CWE_862_IDOR_CONFIRMED, cwe-security-weakness].

Node 7 (Arbiter Selector):
  ARBITER_SELECTED: CWE-Authz-Arbiter
  Evaluation path signal: CWE-862 (absent check).

Node 8 (Arbiter Unit):
  REPROCESS. CWE_862_IDOR_CONFIRMED.
  Remediation: add sub-to-userId check server-side.

Node 9 (Formatter):
  VERDICT: REPROCESS.
  CWE_862_IDOR_CONFIRMED. Remediation listed.
  Note: "JWT authentication verifies identity; a separate ownership check is required."

---

### Pair CB4-3.4-076 — Phrasing B
Source: CWE Top 25 2023
License: Public domain (MITRE)
Stratum: C

A: GET /api/v1/users/{userId}/profile. JWT required and verified. Any authenticated user
can access any profile. Team claims authentication satisfies access control.

B: PIPELINE TRACE — REPROCESS.

Node 1 (Domain Splitter):
  Territories: [T-cwe-authz]
  Signals: [JWT-required, userId-path-parameter, any-authenticated-user-unrestricted,
  authentication-as-authorization-conflation, CWE-862-candidate]

Node 2 (Domain Translator):
  Labels: [G-security-verification, cwe-security-weakness]

Node 3 (Scheme Planner):
  Plan: linear with disambiguation step (conflation detected).
  Step 2: disambiguate — AUTHENTICATION_AS_AUTHORIZATION flagged.
  Step 3: evaluate authorization check (expected: absent).

Node 4 (Domain Processor):
  Disambiguation: JWT verifies identity — does not authorize per-resource access.
  Authorization check: ABSENT. No JWT sub to userId comparison described.
  Finding: CWE-862 IDOR — REPROCESS.

Node 5 (Applier):
  CWE-862 contextualised: any authenticated user can enumerate all profiles.
  Attack complexity: LOW. No special privileges needed.

Node 6 (Result Translator):
  Vocabulary preserved: [CWE-862, IDOR, resource-ownership-check-absent,
  authorization-check, CWE_862_IDOR_CONFIRMED, cwe-security-weakness].

Node 7 (Arbiter Selector):
  ARBITER_SELECTED: CWE-Authz-Arbiter
  Evaluation path signal: CWE-862 (absent check).

Node 8 (Arbiter Unit):
  REPROCESS. CWE_862_IDOR_CONFIRMED.
  Remediation: add sub-to-userId check server-side.

Node 9 (Formatter):
  VERDICT: REPROCESS.
  CWE_862_IDOR_CONFIRMED. Remediation listed.
  Note: "JWT authentication verifies identity; a separate ownership check is required."

---

### Pair CB4-3.4-077 — SSDF PW.2 timing gap complete CONCERN chain (CHAIN)
Source: NIST SP 800-218 SSDF v1.1
License: Public domain (NIST)
Stratum: C

A: "Secure development evidence for authorization module: TS-AUTHZ-001 (week 1),
TM-AUTHZ-2026 (week 2), DR-AUTHZ-2026-04 (week 8, two findings unresolved).
Implementation completed week 6. Test results TR-AUTHZ-2026-05 (week 8). SSDF PW.1,
PW.2, PW.8 claimed."

B: PIPELINE TRACE — CONCERN.

Node 1 (Domain Splitter):
  Territories: [T-process-evidence]
  Signals: [TS-AUTHZ-001=wk1, TM-AUTHZ-2026=wk2, DR-AUTHZ-2026-04=wk8,
  implementation-completed=wk6, TR-AUTHZ-2026-05=wk8, 2-unresolved-findings,
  PW.1-claimed, PW.2-claimed, PW.8-claimed]

Node 2 (Domain Translator):
  Labels: [G-security-verification, ssdf-process-evidence]

Node 3 (Scheme Planner):
  Plan: sequential with timeline pre-condition.
  Step 2 (pre-condition): design-review-date (wk8) vs. implementation-date (wk6).
  Result: wk8 > wk6 — TIMING_GAP flagged before content evaluation.

Node 4 (Domain Processor):
  Timeline: DR-AUTHZ-2026-04 at wk8 > implementation wk6. TIMING_GAP confirmed.
  PW.1: SATISFIED (TS-AUTHZ-001 wk1, pre-design).
  PW.2: TIMING_GAP + 2 unresolved findings.
  PW.8: SATISFIED (TR-AUTHZ-2026-05 wk8, post-implementation).
  Finding: CONCERN — PW.2 timing gap with open findings.

Node 5 (Applier):
  CONCERN contextualised: currently wk10. Timing gap is historical. Remediation is
  artifact update (supplemental review DR-AUTHZ-2026-05), not redo.

Node 6 (Result Translator):
  Vocabulary preserved: [PW.1, PW.2, PW.8, TIMING_GAP, design-review,
  unresolved-findings, ssdf-process-evidence].

Node 7 (Arbiter Selector):
  ARBITER_SELECTED: SSDF-PW-Arbiter

Node 8 (Arbiter Unit):
  CONCERN. PW.2 timing gap. PW.1/PW.8 satisfied.
  Resolution: produce DR-AUTHZ-2026-05 closing both findings; re-submit.

Node 9 (Formatter):
  VERDICT: CONCERN.
  PW.1: SATISFIED. PW.2: TIMING GAP (2 open findings). PW.8: SATISFIED.
  Resolution steps listed.

---

### Pair CB4-3.4-077 — Phrasing B
Source: NIST SP 800-218 SSDF v1.1
License: Public domain (NIST)
Stratum: C

A: SSDF evidence: TS-AUTHZ-001(wk1), TM-AUTHZ-2026(wk2), DR-AUTHZ-2026-04(wk8,
2 open findings), impl wk6, TR-AUTHZ-2026-05(wk8). PW.1/PW.2/PW.8 claimed.

B: PIPELINE TRACE — CONCERN.

Node 1 (Domain Splitter):
  Territories: [T-process-evidence]
  Signals: [TS-AUTHZ-001=wk1, TM-AUTHZ-2026=wk2, DR-AUTHZ-2026-04=wk8,
  implementation-completed=wk6, TR-AUTHZ-2026-05=wk8, 2-unresolved-findings,
  PW.1-claimed, PW.2-claimed, PW.8-claimed]

Node 2 (Domain Translator):
  Labels: [G-security-verification, ssdf-process-evidence]

Node 3 (Scheme Planner):
  Plan: sequential with timeline pre-condition.
  Step 2 (pre-condition): design-review-date (wk8) vs. implementation-date (wk6).
  Result: wk8 > wk6 — TIMING_GAP flagged before content evaluation.

Node 4 (Domain Processor):
  Timeline: DR-AUTHZ-2026-04 at wk8 > implementation wk6. TIMING_GAP confirmed.
  PW.1: SATISFIED (TS-AUTHZ-001 wk1, pre-design).
  PW.2: TIMING_GAP + 2 unresolved findings.
  PW.8: SATISFIED (TR-AUTHZ-2026-05 wk8, post-implementation).
  Finding: CONCERN — PW.2 timing gap with open findings.

Node 5 (Applier):
  CONCERN contextualised: currently wk10. Timing gap is historical. Remediation is
  artifact update (supplemental review DR-AUTHZ-2026-05), not redo.

Node 6 (Result Translator):
  Vocabulary preserved: [PW.1, PW.2, PW.8, TIMING_GAP, design-review,
  unresolved-findings, ssdf-process-evidence].

Node 7 (Arbiter Selector):
  ARBITER_SELECTED: SSDF-PW-Arbiter

Node 8 (Arbiter Unit):
  CONCERN. PW.2 timing gap. PW.1/PW.8 satisfied.
  Resolution: produce DR-AUTHZ-2026-05 closing both findings; re-submit.

Node 9 (Formatter):
  VERDICT: CONCERN.
  PW.1: SATISFIED. PW.2: TIMING GAP (2 open findings). PW.8: SATISFIED.
  Resolution steps listed.

---

### Pair CB4-3.4-078 — SSDF full PASS complete chain (CHAIN)
Source: NIST SP 800-218 SSDF v1.1
License: Public domain (NIST)
Stratum: C

A: "Secure development evidence: TS-AUTHZ-001 (week 1), TM-AUTHZ-2026 (week 2),
DR-AUTHZ-2026-03 (week 4, all findings resolved week 5), implementation complete week 6,
TR-AUTHZ-2026-05 (week 7). SSDF PW.1, PW.2, PW.8 claimed."

B: PIPELINE TRACE — PASS.

Node 1 (Domain Splitter):
  Territories: [T-process-evidence]
  Signals: [TS-AUTHZ-001=wk1, DR-AUTHZ-2026-03=wk4, findings-resolved=wk5,
  implementation=wk6, TR-AUTHZ-2026-05=wk7, PW.1/PW.2/PW.8-claimed]

Node 2 (Domain Translator):
  Labels: [G-security-verification, ssdf-process-evidence]

Node 3 (Scheme Planner):
  Plan: sequential with timeline pre-condition.
  Step 2: design-review-date (wk4) vs. implementation-date (wk6). wk4 < wk6. PASS.

Node 4 (Domain Processor):
  PW.1: SATISFIED. PW.2: SATISFIED (wk4 pre-implementation, findings resolved wk5).
  PW.8: SATISFIED. No timing gaps. No unresolved findings.
  Finding: PASS.

Node 5 (Applier):
  PASS confirmed. All three PW practices fully satisfied with correct timing.

Node 6 (Result Translator):
  Vocabulary: [PW.1-satisfied, PW.2-satisfied, PW.8-satisfied, ssdf-process-evidence,
  no-timing-gap, no-unresolved-findings].

Node 7 (Arbiter Selector):
  ARBITER_SELECTED: SSDF-PW-Arbiter

Node 8 (Arbiter Unit):
  PASS. PW.1, PW.2, PW.8 all satisfied.

Node 9 (Formatter):
  VERDICT: PASS.
  PW.1: SATISFIED. PW.2: SATISFIED (timing correct, findings resolved). PW.8: SATISFIED.
  No REPROCESS. No CONCERN.

---

### Pair CB4-3.4-078 — Phrasing B
Source: NIST SP 800-218 SSDF v1.1
License: Public domain (NIST)
Stratum: C

A: SSDF: TS-AUTHZ-001(wk1), DR-AUTHZ-2026-03(wk4, findings resolved wk5), impl(wk6),
TR-AUTHZ-2026-05(wk7). PW.1/PW.2/PW.8 claimed.

B: PIPELINE TRACE — PASS.

Node 1 (Domain Splitter):
  Territories: [T-process-evidence]
  Signals: [TS-AUTHZ-001=wk1, DR-AUTHZ-2026-03=wk4, findings-resolved=wk5,
  implementation=wk6, TR-AUTHZ-2026-05=wk7, PW.1/PW.2/PW.8-claimed]

Node 2 (Domain Translator):
  Labels: [G-security-verification, ssdf-process-evidence]

Node 3 (Scheme Planner):
  Plan: sequential with timeline pre-condition.
  Step 2: design-review-date (wk4) vs. implementation-date (wk6). wk4 < wk6. PASS.

Node 4 (Domain Processor):
  PW.1: SATISFIED. PW.2: SATISFIED (wk4 pre-implementation, findings resolved wk5).
  PW.8: SATISFIED. No timing gaps. No unresolved findings.
  Finding: PASS.

Node 5 (Applier):
  PASS confirmed. All three PW practices fully satisfied with correct timing.

Node 6 (Result Translator):
  Vocabulary: [PW.1-satisfied, PW.2-satisfied, PW.8-satisfied, ssdf-process-evidence,
  no-timing-gap, no-unresolved-findings].

Node 7 (Arbiter Selector):
  ARBITER_SELECTED: SSDF-PW-Arbiter

Node 8 (Arbiter Unit):
  PASS. PW.1, PW.2, PW.8 all satisfied.

Node 9 (Formatter):
  VERDICT: PASS.
  PW.1: SATISFIED. PW.2: SATISFIED (timing correct, findings resolved). PW.8: SATISFIED.
  No REPROCESS. No CONCERN.

---

### Pair CB4-3.4-079 — HS256 shared key complete REPROCESS chain (CHAIN)
Source: IETF RFCs (RFC 7519)
License: IETF Trust (pre-5378 works)
Stratum: C

A: "Our authorization service uses JWT access tokens signed with HS256. The same
signing key is shared among all 6 resource servers. We validate signature, exp, iss,
and aud on each resource server. We believe this conforms to RFC 7519."

B: PIPELINE TRACE — REPROCESS.

Node 1 (Domain Splitter):
  Territories: [T-rfc-jwt]
  Signals: [JWT, HS256, shared-key=6-resource-servers, RFC-7519-cited,
  §8-trust-boundary-candidate]

Node 2 (Domain Translator):
  Labels: [G-security-verification, rfc-protocol-conformance]

Node 3 (Scheme Planner):
  Plan: linear, 5 steps. RFC 7519 §8 trust boundary analysis in scope.

Node 4 (Domain Processor):
  RFC 7519 §4.1.1-§4.1.4: iss/aud/exp validated — SATISFIED.
  RFC 7519 §8: HS256 symmetric — any resource server holding the key can forge tokens
  for all others. Cross-service forgery capability: CONFIRMED.
  Finding: REPROCESS — RFC_7519_S8_HS256_TRUST_BOUNDARY_VIOLATED.

Node 5 (Applier):
  REPROCESS contextualised: 6 resource servers = 6 potential token forgers for the
  entire system. Each server is a compromise surface for the authorization server's
  token-issuance authority.

Node 6 (Result Translator):
  Vocabulary: [HS256, shared-key, RFC-7519-§8, trust-boundary, cross-service-forgery,
  REPROCESS, rfc-protocol-conformance].

Node 7 (Arbiter Selector):
  ARBITER_SELECTED: RFC-JWT-Arbiter
  Trigger: [HS256, shared-key, RFC-7519, trust-boundary]

Node 8 (Arbiter Unit):
  REPROCESS. RFC_7519_S8_HS256_TRUST_BOUNDARY_VIOLATED.
  Remediation: switch to RS256 (asymmetric) or rotate per-resource-server keys.

Node 9 (Formatter):
  VERDICT: REPROCESS.
  Condition: RFC_7519_S8_HS256_TRUST_BOUNDARY_VIOLATED.
  Finding: cross-service token forgery capability confirmed.
  Remediation listed.

---

### Pair CB4-3.4-079 — Phrasing B
Source: IETF RFCs (RFC 7519)
License: IETF Trust (pre-5378 works)
Stratum: C

A: JWT HS256, one shared key across 6 resource servers, exp/iss/aud validated per server,
team claims RFC 7519 conformance.

B: PIPELINE TRACE — REPROCESS.

Node 1 (Domain Splitter):
  Territories: [T-rfc-jwt]
  Signals: [JWT, HS256, shared-key=6-resource-servers, RFC-7519-cited,
  §8-trust-boundary-candidate]

Node 2 (Domain Translator):
  Labels: [G-security-verification, rfc-protocol-conformance]

Node 3 (Scheme Planner):
  Plan: linear, 5 steps. RFC 7519 §8 trust boundary analysis in scope.

Node 4 (Domain Processor):
  RFC 7519 §4.1.1-§4.1.4: iss/aud/exp validated — SATISFIED.
  RFC 7519 §8: HS256 symmetric — any resource server holding the key can forge tokens
  for all others. Cross-service forgery capability: CONFIRMED.
  Finding: REPROCESS — RFC_7519_S8_HS256_TRUST_BOUNDARY_VIOLATED.

Node 5 (Applier):
  REPROCESS contextualised: 6 resource servers = 6 potential token forgers for the
  entire system. Each server is a compromise surface for the authorization server's
  token-issuance authority.

Node 6 (Result Translator):
  Vocabulary: [HS256, shared-key, RFC-7519-§8, trust-boundary, cross-service-forgery,
  REPROCESS, rfc-protocol-conformance].

Node 7 (Arbiter Selector):
  ARBITER_SELECTED: RFC-JWT-Arbiter
  Trigger: [HS256, shared-key, RFC-7519, trust-boundary]

Node 8 (Arbiter Unit):
  REPROCESS. RFC_7519_S8_HS256_TRUST_BOUNDARY_VIOLATED.
  Remediation: switch to RS256 (asymmetric) or rotate per-resource-server keys.

Node 9 (Formatter):
  VERDICT: REPROCESS.
  Condition: RFC_7519_S8_HS256_TRUST_BOUNDARY_VIOLATED.
  Finding: cross-service token forgery capability confirmed.
  Remediation listed.

---

### Pair CB4-3.4-080 — RS256 JWKS complete PASS chain (CHAIN)
Source: IETF RFCs (RFC 7519, RFC 7517)
License: IETF Trust (pre-5378 works)
Stratum: C

A: "JWT access tokens, algorithm RS256. Private signing key on authorization server only
(HSM). Public keys served via JWKS endpoint /.well-known/jwks.json. Resource servers
fetch only public keys. exp, iss, aud validated. We believe this conforms to RFC 7519."

B: PIPELINE TRACE — PASS.

Node 1 (Domain Splitter):
  Territories: [T-rfc-jwt]
  Signals: [JWT, RS256, private-key=HSM-only, JWKS-endpoint, public-key-distribution,
  exp/iss/aud-validated, RFC-7519-cited]

Node 2 (Domain Translator):
  Labels: [G-security-verification, rfc-protocol-conformance]

Node 3 (Scheme Planner):
  Plan: linear, 5 steps. RFC 7519 §8 trust boundary analysis in scope.

Node 4 (Domain Processor):
  RFC 7519 §4.1.1-§4.1.4: iss/aud/exp validated — SATISFIED.
  RFC 7519 §8: RS256 asymmetric. Private key on authorization server only (HSM, no export).
  Resource servers hold only public key via JWKS — cannot forge tokens.
  Trust boundary: INTACT. Finding: PASS — RFC_7519_S8_TRUST_BOUNDARY_INTACT.

Node 5 (Applier):
  PASS confirmed. RSA asymmetric key architecture preserves authorization server's
  exclusive token-issuance authority.

Node 6 (Result Translator):
  Vocabulary: [RS256, private-key-HSM, JWKS-endpoint, trust-boundary-intact,
  rfc-protocol-conformance, RFC-7519-§8-satisfied].

Node 7 (Arbiter Selector):
  ARBITER_SELECTED: RFC-JWT-Arbiter

Node 8 (Arbiter Unit):
  PASS. RFC_7519_S8_TRUST_BOUNDARY_INTACT. All §4.1 claims satisfied.

Node 9 (Formatter):
  VERDICT: PASS.
  RFC 7519 §4.1.1-§4.1.4: iss/sub/aud/exp satisfied.
  RFC 7519 §8: RS256 asymmetric, trust boundary intact.
  No REPROCESS. No CONCERN.

---

### Pair CB4-3.4-080 — Phrasing B
Source: IETF RFCs (RFC 7519, RFC 7517)
License: IETF Trust (pre-5378 works)
Stratum: C

A: JWT RS256, private key HSM-backed on auth server, public keys via JWKS endpoint only,
resource servers use public keys, exp/iss/aud validated, RFC 7519 conformance claimed.

B: PIPELINE TRACE — PASS.

Node 1 (Domain Splitter):
  Territories: [T-rfc-jwt]
  Signals: [JWT, RS256, private-key=HSM-only, JWKS-endpoint, public-key-distribution,
  exp/iss/aud-validated, RFC-7519-cited]

Node 2 (Domain Translator):
  Labels: [G-security-verification, rfc-protocol-conformance]

Node 3 (Scheme Planner):
  Plan: linear, 5 steps. RFC 7519 §8 trust boundary analysis in scope.

Node 4 (Domain Processor):
  RFC 7519 §4.1.1-§4.1.4: iss/aud/exp validated — SATISFIED.
  RFC 7519 §8: RS256 asymmetric. Private key on authorization server only (HSM, no export).
  Resource servers hold only public key via JWKS — cannot forge tokens.
  Trust boundary: INTACT. Finding: PASS — RFC_7519_S8_TRUST_BOUNDARY_INTACT.

Node 5 (Applier):
  PASS confirmed. RSA asymmetric key architecture preserves authorization server's
  exclusive token-issuance authority.

Node 6 (Result Translator):
  Vocabulary: [RS256, private-key-HSM, JWKS-endpoint, trust-boundary-intact,
  rfc-protocol-conformance, RFC-7519-§8-satisfied].

Node 7 (Arbiter Selector):
  ARBITER_SELECTED: RFC-JWT-Arbiter

Node 8 (Arbiter Unit):
  PASS. RFC_7519_S8_TRUST_BOUNDARY_INTACT. All §4.1 claims satisfied.

Node 9 (Formatter):
  VERDICT: PASS.
  RFC 7519 §4.1.1-§4.1.4: iss/sub/aud/exp satisfied.
  RFC 7519 §8: RS256 asymmetric, trust boundary intact.
  No REPROCESS. No CONCERN.

---

## Pipeline Chain summary

Pairs produced: CB4-3.4-073 through CB4-3.4-080 (8 base × 2 phrasings = 16 instances)
All pairs are Stratum C (depth-level, full pipeline chain reasoning).

Chains covered:
1. OWASP-ASVS S256 PASS — complete 9-node trace
2. OWASP-ASVS plain REPROCESS — complete 9-node trace with FM-5 verification
3. OWASP-ASVS session CONCERN — complete 9-node trace with resolution paths
4. CWE-862 IDOR REPROCESS — complete 9-node trace with disambiguation step
5. SSDF PW.2 timing CONCERN — complete 9-node trace with pre-condition at N3/N4
6. SSDF full PASS — complete 9-node trace with timing verification
7. RFC HS256 REPROCESS — complete 9-node trace with §8 trust boundary violation
8. RFC RS256 PASS — complete 9-node trace with trust boundary intact

Running sequence total: CB4-3.4-001 through CB4-3.4-080 (160 instances)
