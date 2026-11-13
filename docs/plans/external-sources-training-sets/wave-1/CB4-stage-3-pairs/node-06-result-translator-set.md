# Node 6 — Result Translator Pairs
# Batch CB4 — Sub-phase 3.4 (Security and Authorization Model)
# Wave 1 — Stage 3 training pairs
# Sequence range: CB4-3.4-056 through CB4-3.4-060
# Base pairs: 5 × 2 phrasings = 10 pair instances
# Node role: translates applied result to human-readable summary; output feeds Node 7
# Production date: 2026-05-05

---

## Coverage map

| Pair | Applied result input | Translated output | Source | Stratum |
|------|---------------------|------------------|--------|---------|
| 056 | T-auth PASS (production confirmed) | Human-readable V6 compliance summary | OWASP-ASVS | A |
| 057 | T-oauth-oidc REPROCESS (SPA context) | PKCE violation summary with S256 key terms | OWASP-ASVS | D |
| 058 | T-session CONCERN (internal std deviation) | Session concern summary with ASVS state | OWASP-ASVS | D |
| 059 | T-cwe-authz REPROCESS (IDOR, 50k users) | IDOR finding summary with CWE vocabulary | CWE-TOP25 | D |
| 060 | T-process-evidence CONCERN (PW.2 gap) | SSDF process gap summary with PW vocabulary | NIST-SSDF | D |

---

## Rule: vocabulary preservation

Node 6 output must preserve the vocabulary signals that Node 7 uses for arbiter selection.
If the applied result contains PKCE/code_challenge_method, the translated output must retain
those terms. If the applied result contains CWE-862, the translated output must retain CWE
terminology. Stripping source-specific vocabulary from the translation causes arbiter
selection failures in Node 7.

---

## Pairs

---

### Pair CB4-3.4-056 — T-auth PASS translated to authentication compliance summary (TRANSLATE)
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: A

A: Applied result: T-auth, PASS, production confirmed. V6.2.1 satisfied (password 15 chars
at write time). V6.5.5 satisfied (MFA for admin). V6.2.3 satisfied (bcrypt cost 12 in
production credential store). V6.5.2 satisfied (lockout at 10 failures). Scope boundary:
backend auth service only. Confidence HIGH. Translate to human-readable summary for Node 7.

B: TRANSLATED_RESULT: T-auth.
Authentication controls evaluation — ASVS Chapter V6 (Authentication):
  Password policy: minimum 15 characters, enforced at write time. Satisfies V6.2.1.
  Credential storage: bcrypt with cost factor 12, production credential store confirmed.
    Adaptive one-way hash. Satisfies V6.2.3.
  Multi-Factor Authentication: required for all admin operations, applied at service
    entry point. Satisfies V6.5.5.
  Account lockout: triggers after 10 consecutive authentication failures, applied at
    authentication service level. Satisfies V6.5.2.
All evaluated authentication-controls vocabulary: [password-policy, bcrypt, MFA-admin,
  account-lockout, V6.2.1, V6.2.3, V6.5.5, V6.5.2, authentication-controls].
Overall: authentication controls PASS — all V6 requirements satisfied with production
evidence. Scope: backend authentication service.

Reason: The translated result preserves all ASVS V6 requirement IDs and authentication
vocabulary. Node 7 uses authentication-controls vocabulary to select ASVS-V6-Auth-Arbiter.
The translation adds readable narrative without removing the structured vocabulary that
enables downstream arbiter selection.

---

### Pair CB4-3.4-056 — Phrasing B
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: A

A: Applied result T-auth PASS: V6.2.1 (password), V6.5.5 (MFA admin), V6.2.3 (bcrypt
cost 12), V6.5.2 (lockout 10), all production-confirmed. Scope: backend auth service.
Translate for Node 7.

B: TRANSLATED_RESULT: T-auth.
Authentication controls evaluation — ASVS Chapter V6 (Authentication):
  Password policy: minimum 15 characters, enforced at write time. Satisfies V6.2.1.
  Credential storage: bcrypt with cost factor 12, production credential store confirmed.
    Adaptive one-way hash. Satisfies V6.2.3.
  Multi-Factor Authentication: required for all admin operations, applied at service
    entry point. Satisfies V6.5.5.
  Account lockout: triggers after 10 consecutive authentication failures, applied at
    authentication service level. Satisfies V6.5.2.
All evaluated authentication-controls vocabulary: [password-policy, bcrypt, MFA-admin,
  account-lockout, V6.2.1, V6.2.3, V6.5.5, V6.5.2, authentication-controls].
Overall: authentication controls PASS — all V6 requirements satisfied with production
evidence. Scope: backend authentication service.

---

### Pair CB4-3.4-057 — T-oauth-oidc REPROCESS translated to PKCE violation summary (TRANSLATE)
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: D

A: Applied result: T-oauth-oidc, REPROCESS, ASVS_10_4_6_PLAIN_METHOD_REJECTED.
code_challenge_method='plain', unhashed verifier transmitted, S256 not used, authorization
server accepts both. SPA context: plain code_challenge visible in browser redirect.
V10.4.1 satisfied (authorization code flow). Translate for Node 7.

B: TRANSLATED_RESULT: T-oauth-oidc.
OAuth and OIDC evaluation — ASVS Chapter V10 (OAuth and OIDC):
  Authorization code flow: implemented and confirmed. Satisfies V10.4.1.
  PKCE implementation: code_challenge parameter present in authorization requests.
    code_challenge_method value: 'plain'. ASVS 10.4.6 requires S256.
    Finding: plain method transmits unhashed code verifier — in SPA context, the
    authorization request URL is browser-visible; the plain code_challenge is exposed
    in browser history and HTTP logs.
    Authorization server: accepts both S256 and plain. Server must be configured to
    reject plain per RFC 7636 §4.2 server-side enforcement. V10.4.6 NOT SATISFIED.
All evaluated oauth-oidc vocabulary: [authorization-code-flow, PKCE, code_challenge,
  code_challenge_method=plain, S256-required, RFC-7636, ASVS-10.4.6, oauth-oidc,
  ASVS_10_4_6_PLAIN_METHOD_REJECTED].
Overall: OAuth REPROCESS — ASVS 10.4.6 not satisfied. code_challenge_method must be S256.

Reason: The translation preserves PKCE and code_challenge_method vocabulary for Node 7
arbiter selection (ASVS-V10-OAuth-Arbiter trigger). The finding summary includes the
specific ASVS requirement ID (10.4.6) and condition code (ASVS_10_4_6_PLAIN_METHOD_REJECTED)
so Node 8 can apply the arbiter evaluation precisely.

---

### Pair CB4-3.4-057 — Phrasing B
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: D

A: Applied result T-oauth-oidc REPROCESS: plain method, ASVS 10.4.6 not satisfied,
SPA context, V10.4.1 satisfied. Translate for Node 7.

B: TRANSLATED_RESULT: T-oauth-oidc.
OAuth and OIDC evaluation — ASVS Chapter V10 (OAuth and OIDC):
  Authorization code flow: implemented and confirmed. Satisfies V10.4.1.
  PKCE implementation: code_challenge parameter present in authorization requests.
    code_challenge_method value: 'plain'. ASVS 10.4.6 requires S256.
    Finding: plain method transmits unhashed code verifier — in SPA context, the
    authorization request URL is browser-visible; the plain code_challenge is exposed
    in browser history and HTTP logs.
    Authorization server: accepts both S256 and plain. Server must be configured to
    reject plain per RFC 7636 §4.2 server-side enforcement. V10.4.6 NOT SATISFIED.
All evaluated oauth-oidc vocabulary: [authorization-code-flow, PKCE, code_challenge,
  code_challenge_method=plain, S256-required, RFC-7636, ASVS-10.4.6, oauth-oidc,
  ASVS_10_4_6_PLAIN_METHOD_REJECTED].
Overall: OAuth REPROCESS — ASVS 10.4.6 not satisfied. code_challenge_method must be S256.

---

### Pair CB4-3.4-058 — T-session CONCERN translated to session management summary (TRANSLATE)
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: D

A: Applied result: T-session, CONCERN, ASVS V7.3.1 satisfied (20-min timeout with
RA-AUTH-003), internal standard 15-min deviation, approver sign-off pending, severity
LOW. Translate for Node 7.

B: TRANSLATED_RESULT: T-session.
Session management evaluation — ASVS Chapter V7 (Session Management):
  Idle session timeout: 20 minutes. ASVS 7.3.1 requires documented risk acceptance for
    timeout values. Risk analysis RA-AUTH-003 on file. Satisfies V7.3.1.
  Session invalidation on logout: confirmed. Satisfies V7.3.3 (session termination).
  Concern: team internal standard specifies 15-minute maximum idle timeout. Current
    value of 20 minutes is a 5-minute deviation. Risk analysis RA-AUTH-003 exists but
    does not carry approver/reviewer sign-off.
  ASVS compliance state: SATISFIED. The concern is an internal governance gap, not an
    ASVS violation.
  Concern severity: LOW — security control is present and appropriate; governance
    documentation step is incomplete.
All evaluated session-management vocabulary: [idle-timeout, session-invalidation,
  risk-analysis=RA-AUTH-003, ASVS-7.3.1, internal-standard-deviation, session-management].
Overall: session management CONCERN — ASVS compliant; internal standard sign-off pending.

---

### Pair CB4-3.4-058 — Phrasing B
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: D

A: Applied result T-session CONCERN: V7.3.1 satisfied, 20-min timeout, RA-AUTH-003,
internal standard 15-min, no sign-off, LOW severity. Translate for Node 7.

B: TRANSLATED_RESULT: T-session.
Session management evaluation — ASVS Chapter V7 (Session Management):
  Idle session timeout: 20 minutes. ASVS 7.3.1 requires documented risk acceptance for
    timeout values. Risk analysis RA-AUTH-003 on file. Satisfies V7.3.1.
  Session invalidation on logout: confirmed. Satisfies V7.3.3 (session termination).
  Concern: team internal standard specifies 15-minute maximum idle timeout. Current
    value of 20 minutes is a 5-minute deviation. Risk analysis RA-AUTH-003 exists but
    does not carry approver/reviewer sign-off.
  ASVS compliance state: SATISFIED. The concern is an internal governance gap, not an
    ASVS violation.
  Concern severity: LOW — security control is present and appropriate; governance
    documentation step is incomplete.
All evaluated session-management vocabulary: [idle-timeout, session-invalidation,
  risk-analysis=RA-AUTH-003, ASVS-7.3.1, internal-standard-deviation, session-management].
Overall: session management CONCERN — ASVS compliant; internal standard sign-off pending.

---

### Pair CB4-3.4-059 — T-cwe-authz REPROCESS translated to IDOR finding summary (TRANSLATE)
Source: CWE Top 25 2023
License: Public domain (MITRE)
Stratum: D

A: Applied result: T-cwe-authz, REPROCESS, CWE_862_IDOR_CONFIRMED. GET
/api/v1/users/{userId}/profile, JWT required (identity established), no sub-to-userId
ownership check, any of 50,000 authenticated users can access any profile, attack
complexity LOW. Translate for Node 7.

B: TRANSLATED_RESULT: T-cwe-authz.
CWE authorization weakness evaluation:
  Endpoint: GET /api/v1/users/{userId}/profile.
  Authentication present: JWT bearer token required — caller identity established.
  Resource authorization check: ABSENT. No comparison of JWT sub claim to userId path
    parameter. Any authenticated user can retrieve any user's profile by supplying an
    arbitrary userId value.
  Pattern: Insecure Direct Object Reference (IDOR) — direct addressability of user
    records without per-caller authorization.
  CWE-862 (Missing Authorization): CONFIRMED. Resource ownership check entirely absent.
    Attack complexity: LOW (valid JWT sufficient; no privilege escalation needed).
    Affected population: up to 50,000 active users.
  CWE-269 (Improper Privilege Management): NOT APPLICABLE (no check present to have
    wrong scope).
All evaluated cwe-security-weakness vocabulary: [IDOR, resource-ownership-check-absent,
  CWE-862, authorization-check, access-control, CWE_862_IDOR_CONFIRMED].
Overall: authorization REPROCESS — CWE-862 confirmed; ownership check required.

Reason: CWE vocabulary is preserved (CWE-862, IDOR, authorization-check) for Node 7
arbiter selection. The translation makes the CWE classification explicit so Node 7
routes to CWE-Authz-Arbiter and Node 8 applies the CWE-862 evaluation path.

---

### Pair CB4-3.4-059 — Phrasing B
Source: CWE Top 25 2023
License: Public domain (MITRE)
Stratum: D

A: Applied result T-cwe-authz REPROCESS: CWE_862_IDOR_CONFIRMED, profile endpoint,
no ownership check, 50k users, LOW complexity. Translate for Node 7.

B: TRANSLATED_RESULT: T-cwe-authz.
CWE authorization weakness evaluation:
  Endpoint: GET /api/v1/users/{userId}/profile.
  Authentication present: JWT bearer token required — caller identity established.
  Resource authorization check: ABSENT. No comparison of JWT sub claim to userId path
    parameter. Any authenticated user can retrieve any user's profile by supplying an
    arbitrary userId value.
  Pattern: Insecure Direct Object Reference (IDOR) — direct addressability of user
    records without per-caller authorization.
  CWE-862 (Missing Authorization): CONFIRMED. Resource ownership check entirely absent.
    Attack complexity: LOW (valid JWT sufficient; no privilege escalation needed).
    Affected population: up to 50,000 active users.
  CWE-269 (Improper Privilege Management): NOT APPLICABLE (no check present to have
    wrong scope).
All evaluated cwe-security-weakness vocabulary: [IDOR, resource-ownership-check-absent,
  CWE-862, authorization-check, access-control, CWE_862_IDOR_CONFIRMED].
Overall: authorization REPROCESS — CWE-862 confirmed; ownership check required.

---

### Pair CB4-3.4-060 — T-process-evidence CONCERN translated to SSDF gap summary (TRANSLATE)
Source: NIST SP 800-218 SSDF v1.1
License: Public domain (NIST)
Stratum: D

A: Applied result: T-process-evidence, CONCERN. PW.1 satisfied (TS-AUTHZ-001, week 1).
PW.2 timing gap (DR-AUTHZ-2026-04 at week 8, post-implementation week 6), two unresolved
findings (token expiry in s2s flow, rate limiting on token endpoint). PW.8 satisfied
(TR-AUTHZ-2026-05). Sprint 10 action items identified. Translate for Node 7.

B: TRANSLATED_RESULT: T-process-evidence.
SSDF secure software development process evaluation — NIST SP 800-218 SSDF v1.1:
  PW.1 (Establish security requirements before design): SATISFIED.
    Evidence: TS-AUTHZ-001, security requirements document, produced week 1 (pre-design).
  PW.2 (Design review for security before implementation): TIMING GAP.
    Design review DR-AUTHZ-2026-04 conducted week 8. Implementation completed week 6.
    Post-implementation review confirmed: PW.2 pre-implementation timing requirement
    not met.
    Unresolved findings in DR-AUTHZ-2026-04:
      Finding 1: token expiry validation absent in service-to-service flow.
      Finding 2: no rate limiting on token endpoint.
  PW.8 (Testing for security): SATISFIED.
    Evidence: TR-AUTHZ-2026-05 — DAST scan and ASVS checklist, week 8.
All evaluated ssdf-process-evidence vocabulary: [threat-model, design-review,
  process-evidence, PW.1, PW.2, PW.8, TIMING_GAP, unresolved-findings,
  ssdf-process-evidence, SSDF-PW-Arbiter-required].
Overall: SSDF process CONCERN — PW.2 timing gap with 2 unresolved findings; PW.1
and PW.8 satisfied.

Reason: SSDF vocabulary (PW.1, PW.2, PW.8, timing gap, design-review) is preserved for
Node 7 to select SSDF-PW-Arbiter. The translated result distinguishes satisfied and
non-satisfied practices so Node 8 can apply the PW.2-specific arbiter evaluation.

---

### Pair CB4-3.4-060 — Phrasing B
Source: NIST SP 800-218 SSDF v1.1
License: Public domain (NIST)
Stratum: D

A: Applied result T-process-evidence CONCERN: PW.1 satisfied, PW.2 timing gap
(wk8 > wk6, 2 open findings: s2s expiry, rate limiting), PW.8 satisfied. Translate for
Node 7.

B: TRANSLATED_RESULT: T-process-evidence.
SSDF secure software development process evaluation — NIST SP 800-218 SSDF v1.1:
  PW.1 (Establish security requirements before design): SATISFIED.
    Evidence: TS-AUTHZ-001, security requirements document, produced week 1 (pre-design).
  PW.2 (Design review for security before implementation): TIMING GAP.
    Design review DR-AUTHZ-2026-04 conducted week 8. Implementation completed week 6.
    Post-implementation review confirmed: PW.2 pre-implementation timing requirement
    not met.
    Unresolved findings in DR-AUTHZ-2026-04:
      Finding 1: token expiry validation absent in service-to-service flow.
      Finding 2: no rate limiting on token endpoint.
  PW.8 (Testing for security): SATISFIED.
    Evidence: TR-AUTHZ-2026-05 — DAST scan and ASVS checklist, week 8.
All evaluated ssdf-process-evidence vocabulary: [threat-model, design-review,
  process-evidence, PW.1, PW.2, PW.8, TIMING_GAP, unresolved-findings,
  ssdf-process-evidence, SSDF-PW-Arbiter-required].
Overall: SSDF process CONCERN — PW.2 timing gap with 2 unresolved findings; PW.1
and PW.8 satisfied.

---

## Node 6 summary

Pairs produced: CB4-3.4-056 through CB4-3.4-060 (5 base × 2 phrasings = 10 instances)

Translation rules trained:
1. Preserve vocabulary for downstream arbiter selection — do not strip source-specific terms
2. Distinguish ASVS compliance state from internal standard compliance explicitly
3. Preserve CWE IDs and pattern names (IDOR) — required for CWE arbiter routing
4. Preserve PW practice identifiers and timing analysis — required for SSDF arbiter routing
5. Include structured vocabulary list at end of each translated result

Running sequence total after this file: CB4-3.4-001 through CB4-3.4-060 (120 instances)

BATCH CB4 STAGE 3 PRODUCTION COMPLETE.
Total: 60 base pairs × 2 phrasings = 120 pair instances.
Sequence IDs: CB4-3.4-001 through CB4-3.4-060.
