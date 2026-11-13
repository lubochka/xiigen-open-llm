# Node 2 — Domain Translator Pairs
# Batch CB4 — Sub-phase 3.4 (Security and Authorization Model)
# Wave 1 — Stage 3 training pairs
# Sequence range: CB4-3.4-021 through CB4-3.4-028
# Base pairs: 8 × 2 phrasings = 16 pair instances
# Node role: receives territory classifications from Node 1; produces domain label array
# Production date: 2026-05-05

---

## Coverage map

| Pair | Territory | Domain labels produced | Source | Stratum |
|------|-----------|----------------------|--------|---------|
| 021 | T-auth (authentication controls) | G-security-verification, authentication-controls | OWASP-ASVS | A |
| 022 | T-session (session management) | G-security-verification, session-management | OWASP-ASVS | A |
| 023 | T-authz (access control) | G-security-verification, authorization-controls | OWASP-ASVS | A |
| 024 | T-token (self-contained tokens) | G-security-verification, token-validation | OWASP-ASVS | A |
| 025 | T-oauth-oidc (OAuth and OIDC) | G-security-verification, oauth-oidc | OWASP-ASVS | A |
| 026 | T-process-evidence (SSDF artifacts) | G-security-verification, ssdf-process-evidence | NIST-SSDF | B |
| 027 | T-cwe-authz (CWE weakness analysis) | G-security-verification, cwe-security-weakness | CWE-TOP25 | A |
| 028 | T-rfc-jwt (RFC protocol conformance) | G-security-verification, rfc-protocol-conformance | IETF-RFCS | A |

---

## Rule: G-security-verification global label

G-security-verification is emitted once per submission (not once per territory).
It signals that the pipeline is operating in security verification mode (sub-phase 3.4),
distinct from code analysis, performance, or documentation processing modes.
Failure mode FM-2 (WRONG_SUB_PHASE_ASSIGNMENT): assigning a 3.3 domain label
(e.g., G-architecture-review) to a security verification input.

---

## Pairs

---

### Pair CB4-3.4-021 — T-auth territory → authentication-controls domain label (TRANSLATE)
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: A

A: Node 1 territory classification output. Territory: T-auth. Signals identified:
password minimum length enforced at write time, Multi-Factor Authentication (MFA)
required for privileged operations, credential storage using bcrypt with cost factor 12,
account lockout after 10 consecutive failures. Input type: security verification submission.
Sub-phase: 3.4 (Security and Authorization Model). Translate territory to domain label(s).

B: DOMAIN_LABELS: [G-security-verification, authentication-controls].
Label definitions:
  G-security-verification: global label — input is a security controls verification
    request under sub-phase 3.4, not a general query or architecture review.
  authentication-controls: territory T-auth vocabulary (password, MFA, bcrypt, lockout)
    maps to ASVS Chapter V6 (Authentication) domain. Routes to ASVS-V6-Auth-Arbiter in
    Node 7.

Reason: G-security-verification is assigned when the submission type is security
verification (sub-phase 3.4). The authentication-controls label is assigned to territory
T-auth because password policy, MFA, and credential storage vocabulary map to the
authentication domain — not the session management domain (which requires timeout and
session lifecycle vocabulary) and not the authorization domain (which requires
resource-ownership or RBAC vocabulary).

---

### Pair CB4-3.4-021 — Phrasing B
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: A

A: Territory T-auth classified by Node 1. Authentication-related signals present: bcrypt
credential hashing, MFA enforcement policy, password length rule, account lockout
threshold. Security verification input. Domain translation required.

B: DOMAIN_LABELS: [G-security-verification, authentication-controls].
Label definitions:
  G-security-verification: global label — input is a security controls verification
    request under sub-phase 3.4, not a general query or architecture review.
  authentication-controls: territory T-auth vocabulary (password, MFA, bcrypt, lockout)
    maps to ASVS Chapter V6 (Authentication) domain. Routes to ASVS-V6-Auth-Arbiter in
    Node 7.

---

### Pair CB4-3.4-022 — T-session territory → session-management domain label (TRANSLATE)
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: A

A: Node 1 territory classification output. Territory: T-session. Signals identified:
idle timeout 20 minutes, absolute session timeout 8 hours, session token invalidated on
logout, concurrent session limit enforced, documented risk analysis RA-AUTH-003 justifying
timeout value. Input type: security verification submission. Sub-phase: 3.4. Translate
territory to domain label(s).

B: DOMAIN_LABELS: [G-security-verification, session-management].
Label definitions:
  G-security-verification: global label — security verification request, sub-phase 3.4.
  session-management: territory T-session vocabulary (idle-timeout, absolute-timeout,
    session-invalidation, risk-analysis) maps to ASVS Chapter V7 (Session Management)
    domain. Routes to ASVS-V7-Session-Arbiter in Node 7.
Note: session-token-reference in T-session does not produce token-validation label —
session tokens are managed by V7, not V9 (self-contained tokens). V9 vocabulary requires
JWT claims structure.

Reason: Idle timeout and session lifecycle vocabulary are the distinguishing signals for
session-management. The token-validation label is not assigned here because V9 requires
structured token format (JWT, iss/sub/exp/aud) — a session identifier is not a
self-contained token in the V9 sense.

---

### Pair CB4-3.4-022 — Phrasing B
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: A

A: Territory T-session output from Node 1. Vocabulary: idle timeout enforced at 20 minutes,
logout invalidates session token, concurrent sessions limited, risk analysis document
referenced (RA-AUTH-003). Session lifecycle signals present. No JWT or token claims
vocabulary. Security verification input, sub-phase 3.4. Produce domain labels.

B: DOMAIN_LABELS: [G-security-verification, session-management].
Label definitions:
  G-security-verification: global label — security verification request, sub-phase 3.4.
  session-management: territory T-session vocabulary (idle-timeout, absolute-timeout,
    session-invalidation, risk-analysis) maps to ASVS Chapter V7 (Session Management)
    domain. Routes to ASVS-V7-Session-Arbiter in Node 7.
Note: session-token-reference in T-session does not produce token-validation label —
session tokens are managed by V7, not V9 (self-contained tokens). V9 vocabulary requires
JWT claims structure.

---

### Pair CB4-3.4-023 — T-authz territory → authorization-controls domain label (TRANSLATE)
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: A

A: Node 1 territory classification output. Territory: T-authz. Signals identified:
Role-Based Access Control (RBAC) enforced at API gateway, resource ownership verified per
endpoint, principle of least privilege documented, ASVS V8.2.1 and V8.2.2 cited by team.
Input type: security verification submission. Sub-phase: 3.4. Translate territory to
domain label(s).

B: DOMAIN_LABELS: [G-security-verification, authorization-controls].
Label definitions:
  G-security-verification: global label — security verification request, sub-phase 3.4.
  authorization-controls: territory T-authz vocabulary (RBAC, resource-ownership,
    least-privilege, V8.2.1, V8.2.2) maps to ASVS Chapter V8 (Authorization) domain.
    Routes to ASVS-V8-AuthZ-Arbiter in Node 7.
Note: authorization-controls is distinct from authentication-controls — RBAC and ownership
checks operate after authentication succeeds. The V6 (Authentication) label is not
assigned here.

Reason: RBAC and resource ownership vocabulary unambiguously maps to authorization-controls
(ASVS V8). Assigning authentication-controls to this territory would be incorrect — V6
covers credential verification at login, not post-authentication access decisions.

---

### Pair CB4-3.4-023 — Phrasing B
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: A

A: Territory T-authz from Node 1. Signals: RBAC policy enforced, per-endpoint ownership
check, ASVS 8.2.1 and 8.2.2 cited, least privilege principle documented. Access control
vocabulary present. No credential or authentication lifecycle vocabulary. Security
verification input, sub-phase 3.4. Produce domain labels.

B: DOMAIN_LABELS: [G-security-verification, authorization-controls].
Label definitions:
  G-security-verification: global label — security verification request, sub-phase 3.4.
  authorization-controls: territory T-authz vocabulary (RBAC, resource-ownership,
    least-privilege, V8.2.1, V8.2.2) maps to ASVS Chapter V8 (Authorization) domain.
    Routes to ASVS-V8-AuthZ-Arbiter in Node 7.
Note: authorization-controls is distinct from authentication-controls — RBAC and ownership
checks operate after authentication succeeds. The V6 (Authentication) label is not
assigned here.

---

### Pair CB4-3.4-024 — T-token territory → token-validation domain label (TRANSLATE)
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: A

A: Node 1 territory classification output. Territory: T-token. Signals identified: JSON
Web Token (JWT) used as API access token, algorithm RS256 (asymmetric), server-side
signature verification enforced, expiry (exp claim) enforced, iss and sub claims validated,
ASVS V9.1.1 and V9.2.1 cited. No OAuth authorization code flow. No PKCE. Input type:
security verification submission. Sub-phase: 3.4. Translate territory to domain label(s).

B: DOMAIN_LABELS: [G-security-verification, token-validation].
Label definitions:
  G-security-verification: global label — security verification request, sub-phase 3.4.
  token-validation: territory T-token vocabulary (JWT, RS256, signature-verification,
    exp/iss/sub claims, V9.1.1, V9.2.1) maps to ASVS Chapter V9 (Self-Contained Tokens)
    domain. Routes to ASVS-V9-Token-Arbiter in Node 7.
Note: RS256 JWT without OAuth flow vocabulary maps to token-validation (V9), not oauth-oidc
(V10). The oauth-oidc label requires authorization-code-flow or PKCE vocabulary.

Reason: JWT with RS256 signature vocabulary maps to ASVS V9 (self-contained tokens)
when OAuth authorization flow is absent. V9 governs how tokens are structured and validated;
V10 governs how they are obtained via OAuth/OIDC flows. The distinction prevents
ASVS-V9-Token-Arbiter and ASVS-V10-OAuth-Arbiter from being conflated.

---

### Pair CB4-3.4-024 — Phrasing B
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: A

A: Territory T-token classified by Node 1. JWT access tokens, RS256, server-side signature
check, exp and sub claims validated, ASVS 9.1.1 and 9.2.1 referenced. No OAuth
authorization code flow. No PKCE. Security verification input, sub-phase 3.4. Domain
labels required.

B: DOMAIN_LABELS: [G-security-verification, token-validation].
Label definitions:
  G-security-verification: global label — security verification request, sub-phase 3.4.
  token-validation: territory T-token vocabulary (JWT, RS256, signature-verification,
    exp/iss/sub claims, V9.1.1, V9.2.1) maps to ASVS Chapter V9 (Self-Contained Tokens)
    domain. Routes to ASVS-V9-Token-Arbiter in Node 7.
Note: RS256 JWT without OAuth flow vocabulary maps to token-validation (V9), not oauth-oidc
(V10). The oauth-oidc label requires authorization-code-flow or PKCE vocabulary.

---

### Pair CB4-3.4-025 — T-oauth-oidc territory → oauth-oidc domain label (TRANSLATE)
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: A

A: Node 1 territory classification output. Territory: T-oauth-oidc. Signals identified:
OAuth 2.0 authorization code flow implemented, Proof Key for Code Exchange (PKCE) active,
code_challenge parameter present in authorization requests, code_challenge_method set to
'plain', authorization server issues JWT access tokens after code exchange. ASVS V10.4.6
cited. Input type: security verification submission. Sub-phase: 3.4. Translate territory
to domain label(s).

B: DOMAIN_LABELS: [G-security-verification, oauth-oidc].
Label definitions:
  G-security-verification: global label — security verification request, sub-phase 3.4.
  oauth-oidc: territory T-oauth-oidc vocabulary (authorization-code-flow, PKCE,
    code_challenge, code_challenge_method, V10.4.6) maps to ASVS Chapter V10
    (OAuth and OIDC) domain. Routes to ASVS-V10-OAuth-Arbiter in Node 7.
Note: the JWT issued after code exchange does not additionally produce token-validation
label — in an OAuth flow, JWT token validation is governed by V10, not independently
by V9.

Reason: PKCE and authorization-code-flow vocabulary are the discriminating signals for the
oauth-oidc label. The resulting JWT token does not trigger a second V9 label because the
token's validity within an OAuth flow is V10's concern. Dual-labeling (V9 + V10) for
OAuth-issued JWTs would route the same territory to two arbiters and create redundant
evaluation.

---

### Pair CB4-3.4-025 — Phrasing B
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: A

A: Territory T-oauth-oidc from Node 1. OAuth 2.0 authorization code flow with PKCE.
code_challenge and code_challenge_method='plain' present. Authorization server issues JWT
after successful code exchange. ASVS V10.4.6 referenced. Security verification input,
sub-phase 3.4. Produce domain labels.

B: DOMAIN_LABELS: [G-security-verification, oauth-oidc].
Label definitions:
  G-security-verification: global label — security verification request, sub-phase 3.4.
  oauth-oidc: territory T-oauth-oidc vocabulary (authorization-code-flow, PKCE,
    code_challenge, code_challenge_method, V10.4.6) maps to ASVS Chapter V10
    (OAuth and OIDC) domain. Routes to ASVS-V10-OAuth-Arbiter in Node 7.
Note: the JWT issued after code exchange does not additionally produce token-validation
label — in an OAuth flow, JWT token validation is governed by V10, not independently
by V9.

---

### Pair CB4-3.4-026 — T-process-evidence territory → ssdf-process-evidence domain label (TRANSLATE)
Source: NIST SP 800-218 SSDF v1.1
License: Public domain (NIST)
Stratum: B

A: Node 1 territory classification output. Territory: T-process-evidence. Signals
identified: threat model artifact TM-AUTHZ-2026, design review record DR-AUTHZ-2026-03,
security test results TR-AUTHZ-2026-05, all produced pre-implementation or during
development cycle. NIST Secure Software Development Framework (SSDF) PW practice family
referenced. No runtime operational metrics. Input type: security verification submission.
Sub-phase: 3.4. Translate territory to domain label(s).

B: DOMAIN_LABELS: [G-security-verification, ssdf-process-evidence].
Label definitions:
  G-security-verification: global label — security verification request, sub-phase 3.4.
  ssdf-process-evidence: territory T-process-evidence vocabulary (threat-model,
    design-review, security-requirements, PW-practice-family, pre-implementation-artifact)
    maps to NIST SSDF process evidence domain. Routes to SSDF-PW-Arbiter in Node 7.
Note: test results in T-process-evidence produce ssdf-process-evidence label, not a
separate ASVS label — SSDF-PW-Arbiter (PW.8) evaluates testing evidence; the ASVS
arbiters evaluate runtime compliance claims.

Reason: Process artifact vocabulary (threat model, design review records with document IDs
and dates) maps to the SSDF domain. The SSDF-PW-Arbiter governs all PW-family practices
including testing (PW.8); assigning an additional ASVS-compatible label from test-result
presence would cause evidence-type conflation (failure mode FM-SSDF-1).

---

### Pair CB4-3.4-026 — Phrasing B
Source: NIST SP 800-218 SSDF v1.1
License: Public domain (NIST)
Stratum: B

A: Territory T-process-evidence from Node 1. Signals: threat model TM-AUTHZ-2026,
design review DR-AUTHZ-2026-03, test results TR-AUTHZ-2026-05 (ZAP + ASVS checklist).
SSDF PW practice family signals. Development-lifecycle artifacts, not runtime metrics.
Security verification input, sub-phase 3.4. Produce domain labels.

B: DOMAIN_LABELS: [G-security-verification, ssdf-process-evidence].
Label definitions:
  G-security-verification: global label — security verification request, sub-phase 3.4.
  ssdf-process-evidence: territory T-process-evidence vocabulary (threat-model,
    design-review, security-requirements, PW-practice-family, pre-implementation-artifact)
    maps to NIST SSDF process evidence domain. Routes to SSDF-PW-Arbiter in Node 7.
Note: test results in T-process-evidence produce ssdf-process-evidence label, not a
separate ASVS label — SSDF-PW-Arbiter (PW.8) evaluates testing evidence; the ASVS
arbiters evaluate runtime compliance claims.

---

### Pair CB4-3.4-027 — T-cwe-authz territory → cwe-security-weakness domain label (TRANSLATE)
Source: CWE Top 25 2023
License: Public domain (MITRE)
Stratum: A

A: Node 1 territory classification output. Territory: T-cwe-authz. Signals identified:
Insecure Direct Object Reference (IDOR) pattern evaluation, authorization check presence
analysis, resource ownership verification at API endpoint, CWE-862 (Missing Authorization)
candidate flagged, CWE-269 (Improper Privilege Management) candidate flagged. Common
Weakness Enumeration (CWE) Top 25 analysis mode. Input type: security verification
submission. Sub-phase: 3.4. Translate territory to domain label(s).

B: DOMAIN_LABELS: [G-security-verification, cwe-security-weakness].
Label definitions:
  G-security-verification: global label — security verification request, sub-phase 3.4.
  cwe-security-weakness: territory T-cwe-authz vocabulary (IDOR, authorization-check,
    resource-ownership, CWE-862-candidate, CWE-269-candidate) maps to CWE weakness
    analysis domain. Routes to CWE-Authz-Arbiter in Node 7.
Note: cwe-security-weakness and authorization-controls are not combined — the CWE arbiter
and ASVS-V8 arbiter operate in distinct evaluation frameworks. Dual-labeling would route
to both arbiters without cross-framework deduplication logic in Node 8.

Reason: CWE candidate vocabulary maps to the cwe-security-weakness domain, not to
authorization-controls (ASVS V8). While both concern authorization, CWE evaluation
classifies by weakness type (862 vs. 269) while ASVS evaluation checks specific
requirement satisfaction. Mixing labels routes to two arbiters that may produce
contradictory verdicts for the same evidence.

---

### Pair CB4-3.4-027 — Phrasing B
Source: CWE Top 25 2023
License: Public domain (MITRE)
Stratum: A

A: Territory T-cwe-authz classified by Node 1. CWE-862 and CWE-269 candidates identified
during domain splitting. IDOR pattern present in API path parameter handling. Authorization
check evaluated. CWE Top 25 weakness analysis in scope. Security verification input,
sub-phase 3.4. Domain labels required.

B: DOMAIN_LABELS: [G-security-verification, cwe-security-weakness].
Label definitions:
  G-security-verification: global label — security verification request, sub-phase 3.4.
  cwe-security-weakness: territory T-cwe-authz vocabulary (IDOR, authorization-check,
    resource-ownership, CWE-862-candidate, CWE-269-candidate) maps to CWE weakness
    analysis domain. Routes to CWE-Authz-Arbiter in Node 7.
Note: cwe-security-weakness and authorization-controls are not combined — the CWE arbiter
and ASVS-V8 arbiter operate in distinct evaluation frameworks. Dual-labeling would route
to both arbiters without cross-framework deduplication logic in Node 8.

---

### Pair CB4-3.4-028 — T-rfc-jwt territory → rfc-protocol-conformance domain label (TRANSLATE)
Source: IETF RFCs (RFC 7519, RFC 6749)
License: IETF Trust (pre-5378 works)
Stratum: A

A: Node 1 territory classification output. Territory: T-rfc-jwt. Signals identified:
JWT access tokens with algorithm HS256, single shared key distributed to 6 resource
servers, RFC 7519 §8 trust boundary analysis flagged by domain splitter, OAuth 2.0
authorization server vocabulary also present. IETF RFC conformance analysis mode. Input
type: security verification submission. Sub-phase: 3.4. Translate territory to
domain label(s).

B: DOMAIN_LABELS: [G-security-verification, rfc-protocol-conformance].
Label definitions:
  G-security-verification: global label — security verification request, sub-phase 3.4.
  rfc-protocol-conformance: territory T-rfc-jwt vocabulary (HS256, shared-key, RFC-7519,
    trust-boundary, RFC-6749, authorization-server) maps to IETF RFC protocol conformance
    domain. Routes to RFC-JWT-Arbiter and RFC-OAuth2-Arbiter selection in Node 7.
Note: rfc-protocol-conformance covers both RFC 7519 (JWT) and RFC 6749 (OAuth 2.0) when
both vocabularies are present in the same territory — Node 7 resolves which RFC arbiters
to invoke from the rfc-protocol-conformance label plus trigger vocabulary signals.

Reason: RFC normative vocabulary (MUST/MAY clause references, RFC section citations, trust
boundary analysis under a specific RFC) maps to rfc-protocol-conformance. This is distinct
from token-validation (ASVS V9 framework) and oauth-oidc (ASVS V10 framework) — the
evaluation axis is RFC conformance, not ASVS chapter coverage.

---

### Pair CB4-3.4-028 — Phrasing B
Source: IETF RFCs (RFC 7519, RFC 6749)
License: IETF Trust (pre-5378 works)
Stratum: A

A: Territory T-rfc-jwt from Node 1. HS256 JWT with shared key across 6 resource servers.
RFC 7519 §8 flagged. OAuth 2.0 authorization server vocabulary also in territory.
IETF RFC conformance analysis. Security verification input, sub-phase 3.4. Produce domain
labels.

B: DOMAIN_LABELS: [G-security-verification, rfc-protocol-conformance].
Label definitions:
  G-security-verification: global label — security verification request, sub-phase 3.4.
  rfc-protocol-conformance: territory T-rfc-jwt vocabulary (HS256, shared-key, RFC-7519,
    trust-boundary, RFC-6749, authorization-server) maps to IETF RFC protocol conformance
    domain. Routes to RFC-JWT-Arbiter and RFC-OAuth2-Arbiter selection in Node 7.
Note: rfc-protocol-conformance covers both RFC 7519 (JWT) and RFC 6749 (OAuth 2.0) when
both vocabularies are present in the same territory — Node 7 resolves which RFC arbiters
to invoke from the rfc-protocol-conformance label plus trigger vocabulary signals.

---

## Node 2 summary

Pairs produced: CB4-3.4-021 through CB4-3.4-028 (8 base × 2 phrasings = 16 instances)

Domain labels trained:
1. G-security-verification — emitted once per security verification submission
2. authentication-controls — T-auth territory, ASVS V6 domain (password, MFA, bcrypt)
3. session-management — T-session territory, ASVS V7 domain (timeout, invalidation)
4. authorization-controls — T-authz territory, ASVS V8 domain (RBAC, ownership)
5. token-validation — T-token territory, ASVS V9 domain (JWT RS256, claims)
6. oauth-oidc — T-oauth-oidc territory, ASVS V10 domain (PKCE, authorization-code-flow)
7. ssdf-process-evidence — T-process-evidence territory, SSDF PW domain
8. cwe-security-weakness — T-cwe-authz territory, CWE weakness analysis domain
9. rfc-protocol-conformance — T-rfc-jwt territory, IETF RFC conformance domain

Key boundaries trained:
- session-token ≠ JWT (session vocabulary does not produce token-validation)
- RS256 JWT without OAuth flow → V9, not V10
- OAuth JWT → V10 only, not V9 + V10
- process-evidence test results → ssdf-process-evidence, not ASVS label
- CWE candidates → cwe-security-weakness, not authorization-controls

Running sequence total after this file: CB4-3.4-001 through CB4-3.4-028 (56 instances)
