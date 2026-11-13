# Node 7 — Arbiter Selector Pairs — Extension Set 1
# Batch CB4 — Sub-phase 3.4 (Security and Authorization Model)
# Wave 1 — Stage 3 Action 3 (coverage extension)
# Sequence range: CB4-3.4-081 through CB4-3.4-088
# Base pairs: 8 × 2 phrasings = 16 pair instances
# Focus: Stratum C disambiguation, combined source families, edge cases
# Production date: 2026-05-05

---

## Coverage map

| Pair | Input type | Selection result | Stratum | New coverage |
|------|-----------|-----------------|---------|-------------|
| 081 | JWT present in both V9 and V10 context | Disambiguate to V10 (OAuth flow primary) | C | V9 vs V10 disambiguation |
| 082 | CWE-287 + CWE-862 combined input | Both Authn + Authz arbiters | C | Two CWE arbiters |
| 083 | SSDF-PO vocabulary (org policy) | SSDF-PO-Arbiter (distinct from PW) | B | PO vs PW arbiter split |
| 084 | All four source families present | Maximum arbiter set (6 arbiters) | C | Max arbiter selection |
| 085 | RS256 JWT without OAuth — V9 only | V9 not V10 (no PKCE) | B | Reinforce V9/V10 boundary |
| 086 | SSDF-PW + ASVS-V6 combined input | SSDF-PW-Arbiter + ASVS-V6-Auth-Arbiter | C | Process evidence + controls |
| 087 | JWT used for session tracking (not bearer) | V7 not V9 (session vocabulary dominant) | D | Session token ≠ JWT token |
| 088 | ASVS V10 + RFC 6749 — both frameworks | V10-OAuth + RFC-OAuth2 combined | C | ASVS + RFC for same OAuth |

---

## Pairs

---

### Pair CB4-3.4-081 — JWT in OAuth flow context → ASVS-V10-OAuth-Arbiter (V9 excluded) (SELECT)
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: C

A: Domain processor result contains: OAuth 2.0 authorization code flow, PKCE S256,
authorization server issues JWT access tokens after code exchange, JWT validated by
resource servers (RS256, exp/iss/aud checked). ASVS 10.4.6 claimed. Team cites both
V9.1.1 (JWT signature) and V10.4.6 (PKCE S256) in their submission. Vocabulary contains
JWT, RS256, signature, PKCE, code_challenge, authorization-code-flow. Select arbiter(s).

B: ARBITER_SELECTED: ASVS-V10-OAuth-Arbiter.
Disambiguation: JWT present in two contexts — V9 (standalone JWT validation) and
  V10 (JWT obtained via OAuth flow). The primary classification signal is:
  authorization-code-flow + PKCE + code_challenge → V10 is the governing chapter.
  V9 is NOT selected because the JWT here is the token obtained at the end of an
  OAuth flow — its validation is governed by V10 (OAuth token semantics), not V9
  (standalone self-contained token semantics).
Trigger vocabulary: [authorization-code-flow, PKCE, code_challenge, S256, ASVS-10.4.6].
Excluded: ASVS-V9-Token-Arbiter — V9 applies when JWT is used outside an OAuth flow
  context (e.g., API-to-API authentication with no OAuth authorization server).
  The team's citation of V9.1.1 does not override the primary OAuth context classification.
Single arbiter dispatched to Node 8.

Reason: When JWT appears alongside OAuth flow vocabulary, the flow vocabulary takes
precedence for arbiter selection. V10 governs the entire OAuth+JWT combination. Selecting
both V9 and V10 for the same JWT would route the identical token to two arbiters, producing
redundant and potentially contradictory verdicts for the same evidence.

---

### Pair CB4-3.4-081 — Phrasing B
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: C

A: OAuth 2.0 auth code flow, PKCE S256, JWT issued after code exchange, RS256, exp/iss/aud
validated. Team cites V9.1.1 and V10.4.6. Vocabulary: [PKCE, code_challenge, OAuth-flow,
JWT, RS256]. Select arbiter.

B: ARBITER_SELECTED: ASVS-V10-OAuth-Arbiter.
Disambiguation: JWT present in two contexts — V9 (standalone JWT validation) and
  V10 (JWT obtained via OAuth flow). The primary classification signal is:
  authorization-code-flow + PKCE + code_challenge → V10 is the governing chapter.
  V9 is NOT selected because the JWT here is the token obtained at the end of an
  OAuth flow — its validation is governed by V10 (OAuth token semantics), not V9
  (standalone self-contained token semantics).
Trigger vocabulary: [authorization-code-flow, PKCE, code_challenge, S256, ASVS-10.4.6].
Excluded: ASVS-V9-Token-Arbiter — V9 applies when JWT is used outside an OAuth flow
  context (e.g., API-to-API authentication with no OAuth authorization server).
  The team's citation of V9.1.1 does not override the primary OAuth context classification.
Single arbiter dispatched to Node 8.

---

### Pair CB4-3.4-082 — CWE-287 + CWE-862 combined: both Authn and Authz arbiters selected (SELECT)
Source: CWE Top 25 2023
License: Public domain (MITRE)
Stratum: C

A: Domain processor result: API endpoint POST /api/v1/admin/actions. Two findings:
(1) Authentication mechanism uses custom MD5+static-secret token (CWE-287 candidate —
token is forgeable). (2) Admin action endpoint performs no admin-role check — any
authenticated user (with a forged token) can invoke admin actions (CWE-862 candidate).
Both authentication weakness vocabulary and authorization weakness vocabulary present.
Select arbiter(s).

B: ARBITERS_SELECTED: [CWE-Authn-Arbiter, CWE-Authz-Arbiter].
Selection rationale:
  CWE-Authn-Arbiter: triggered by [CWE-287-candidate, custom-auth-token, forgeable-token,
    MD5-static-secret]. CWE-287 = improper authentication mechanism.
  CWE-Authz-Arbiter: triggered by [CWE-862-candidate, admin-action-no-role-check,
    authorization-check-absent]. CWE-862 = missing authorization.
Both weaknesses are present and independent:
  - The authentication weakness (CWE-287) means identity is not reliably established.
  - The authorization weakness (CWE-862) means even with valid identity, no role check exists.
  Both must be evaluated. Node 8 runs both arbiter units independently.

Reason: When both authentication and authorization weaknesses are present, both CWE
arbiter families are selected. The weaknesses are independent — fixing CWE-287 (proper
auth mechanism) does not fix CWE-862 (missing admin role check), and vice versa.
Selecting only one arbiter would leave one weakness undetected.

---

### Pair CB4-3.4-082 — Phrasing B
Source: CWE Top 25 2023
License: Public domain (MITRE)
Stratum: C

A: POST /api/v1/admin/actions. Two findings: (1) CWE-287 candidate — custom MD5 token
forgeable. (2) CWE-862 candidate — no admin role check on endpoint. Both auth weakness
and authz weakness vocabulary present. Select arbiters.

B: ARBITERS_SELECTED: [CWE-Authn-Arbiter, CWE-Authz-Arbiter].
Selection rationale:
  CWE-Authn-Arbiter: triggered by [CWE-287-candidate, custom-auth-token, forgeable-token,
    MD5-static-secret]. CWE-287 = improper authentication mechanism.
  CWE-Authz-Arbiter: triggered by [CWE-862-candidate, admin-action-no-role-check,
    authorization-check-absent]. CWE-862 = missing authorization.
Both weaknesses are present and independent:
  - The authentication weakness (CWE-287) means identity is not reliably established.
  - The authorization weakness (CWE-862) means even with valid identity, no role check exists.
  Both must be evaluated. Node 8 runs both arbiter units independently.

---

### Pair CB4-3.4-083 — SSDF-PO-Arbiter selection for organizational policy vocabulary (SELECT)
Source: NIST SP 800-218 SSDF v1.1
License: Public domain (NIST)
Stratum: B

A: Domain processor result: organizational security policy documents submitted. Content
includes: security training curriculum for development team (PO.2.2 reference), tool
acquisition records for SAST and DAST tooling (PO.3.1 reference), security requirements
process definition (PO.1.1 reference). No design review records. No test result records.
No threat model. PO (Prepare the Organization) practice family vocabulary present. Select
arbiter(s).

B: ARBITER_SELECTED: SSDF-PO-Arbiter.
Trigger vocabulary: [PO.1.1, PO.2.2, PO.3.1, training-curriculum, tool-acquisition,
  security-requirements-process, organizational-policy, PO-practice-family].
Excluded: SSDF-PW-Arbiter — no [PW.1, PW.2, PW.8, design-review, threat-model,
  test-results] vocabulary present. PO and PW are distinct practice families:
  PO = organizational preparation (training, tools, processes).
  PW = producing well-secured software (requirements, design review, testing).
Single arbiter dispatched to Node 8.

Reason: PO vocabulary (training, tool acquisition, policy processes) selects SSDF-PO-Arbiter.
SSDF-PW-Arbiter is not selected because no artifact-level evidence (design reviews, test
results) is present. The PO/PW split is a key boundary in SSDF — organizational readiness
(PO) and software production practices (PW) are evaluated separately.

---

### Pair CB4-3.4-083 — Phrasing B
Source: NIST SP 800-218 SSDF v1.1
License: Public domain (NIST)
Stratum: B

A: SSDF submission: training curriculum (PO.2.2), tool acquisition records SAST/DAST
(PO.3.1), security requirements process definition (PO.1.1). No design reviews, no test
results, no threat model. PO family vocabulary only. Select arbiter.

B: ARBITER_SELECTED: SSDF-PO-Arbiter.
Trigger vocabulary: [PO.1.1, PO.2.2, PO.3.1, training-curriculum, tool-acquisition,
  security-requirements-process, organizational-policy, PO-practice-family].
Excluded: SSDF-PW-Arbiter — no [PW.1, PW.2, PW.8, design-review, threat-model,
  test-results] vocabulary present. PO and PW are distinct practice families:
  PO = organizational preparation (training, tools, processes).
  PW = producing well-secured software (requirements, design review, testing).
Single arbiter dispatched to Node 8.

---

### Pair CB4-3.4-084 — All four source families: maximum arbiter set (SELECT)
Source: Multi-source (OWASP ASVS, CWE Top 25, NIST SSDF, IETF RFCs)
License: CC BY-SA 4.0 / Public domain / IETF Trust
Stratum: C

A: Domain processor result spans all four source families. Vocabulary clusters:
Cluster 1: authentication controls (bcrypt, MFA, ASVS V6 claims) — ASVS auth territory.
Cluster 2: session management (idle timeout, ASVS V7 claims) — ASVS session territory.
Cluster 3: access control weakness (IDOR pattern, CWE-862 candidate) — CWE authz territory.
Cluster 4: SSDF process evidence (design review, threat model, PW.2 timing to check) — SSDF territory.
Cluster 5: JWT RS256 with JWKS (RFC 7519, §8 trust boundary analysis) — RFC JWT territory.
Cluster 6: OAuth 2.0 authorization server (RFC 6749 vocabulary) — RFC OAuth2 territory.
Select all required arbiters.

B: ARBITERS_SELECTED: [ASVS-V6-Auth-Arbiter, ASVS-V7-Session-Arbiter,
   CWE-Authz-Arbiter, SSDF-PW-Arbiter, RFC-JWT-Arbiter, RFC-OAuth2-Arbiter].
Selection rationale by cluster:
  Cluster 1 → ASVS-V6-Auth-Arbiter: [bcrypt, MFA, ASVS-V6]
  Cluster 2 → ASVS-V7-Session-Arbiter: [idle-timeout, session-management, ASVS-V7]
  Cluster 3 → CWE-Authz-Arbiter: [IDOR, CWE-862-candidate, authorization-check]
  Cluster 4 → SSDF-PW-Arbiter: [design-review, threat-model, PW.2, process-evidence]
  Cluster 5 → RFC-JWT-Arbiter: [RS256, RFC-7519, JWKS, trust-boundary]
  Cluster 6 → RFC-OAuth2-Arbiter: [authorization-server, RFC-6749, OAuth-2.0-flow]
Excluded: ASVS-V10-OAuth-Arbiter (no PKCE/code_challenge vocabulary); SSDF-PO-Arbiter
  (no training/tool-acquisition vocabulary); CWE-Authn-Arbiter (no custom-auth-mechanism
  vulnerability vocabulary).
Six arbiters from four source families dispatched to Node 8.

Reason: Maximum arbiter set requires one per vocabulary cluster from the appropriate
source family. Excluded arbiters are excluded because their specific trigger vocabulary
is absent — not because only one arbiter per source family is allowed.

---

### Pair CB4-3.4-084 — Phrasing B
Source: Multi-source
License: CC BY-SA 4.0 / Public domain / IETF Trust
Stratum: C

A: Six vocabulary clusters: ASVS V6 auth, ASVS V7 session, CWE-862 IDOR, SSDF PW process
evidence, RFC 7519 RS256 JWKS, RFC 6749 OAuth2. No PKCE, no custom-auth-token, no PO
vocabulary. Select all required arbiters.

B: ARBITERS_SELECTED: [ASVS-V6-Auth-Arbiter, ASVS-V7-Session-Arbiter,
   CWE-Authz-Arbiter, SSDF-PW-Arbiter, RFC-JWT-Arbiter, RFC-OAuth2-Arbiter].
Selection rationale by cluster:
  Cluster 1 → ASVS-V6-Auth-Arbiter: [bcrypt, MFA, ASVS-V6]
  Cluster 2 → ASVS-V7-Session-Arbiter: [idle-timeout, session-management, ASVS-V7]
  Cluster 3 → CWE-Authz-Arbiter: [IDOR, CWE-862-candidate, authorization-check]
  Cluster 4 → SSDF-PW-Arbiter: [design-review, threat-model, PW.2, process-evidence]
  Cluster 5 → RFC-JWT-Arbiter: [RS256, RFC-7519, JWKS, trust-boundary]
  Cluster 6 → RFC-OAuth2-Arbiter: [authorization-server, RFC-6749, OAuth-2.0-flow]
Excluded: ASVS-V10-OAuth-Arbiter (no PKCE/code_challenge vocabulary); SSDF-PO-Arbiter
  (no training/tool-acquisition vocabulary); CWE-Authn-Arbiter (no custom-auth-mechanism
  vulnerability vocabulary).
Six arbiters from four source families dispatched to Node 8.

---

### Pair CB4-3.4-085 — RS256 JWT for API auth: V9 only, not V10 (SELECT)
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: B

A: Domain processor result: REST API uses JWT bearer tokens for authentication. Algorithm
RS256. Tokens issued by a dedicated internal auth service (not an OAuth 2.0 authorization
server — no authorization_code flow, no code_challenge). Tokens validated by resource
services: RS256 signature, exp, iss, aud. ASVS V9 claims: V9.1.1, V9.2.1, V9.2.3. No
OAuth flow vocabulary. No PKCE. No code_challenge. Select arbiter(s).

B: ARBITER_SELECTED: ASVS-V9-Token-Arbiter.
Trigger vocabulary: [JWT, RS256, signature-verification, exp-enforced, iss-aud-validated,
  V9.1.1, V9.2.1, V9.2.3, self-contained-token].
Excluded: ASVS-V10-OAuth-Arbiter — no [authorization-code-flow, PKCE, code_challenge,
  OAuth-authorization-server] vocabulary present.
  Internal auth service that issues JWT directly (without OAuth flow) maps to V9, not V10.
  V10 governs tokens obtained via the OAuth 2.0 authorization code flow.
Single arbiter dispatched to Node 8.

Reason: The V9/V10 boundary is determined by whether the JWT is obtained via OAuth 2.0
flow (V10) or via a non-OAuth token issuance mechanism (V9). No OAuth vocabulary = V9.
This is the inverse of pair 081 (where OAuth flow was present) — V9 is correct here
because no authorization code flow exists.

---

### Pair CB4-3.4-085 — Phrasing B
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: B

A: JWT RS256, issued by internal auth service (no OAuth flow, no code_challenge, no
authorization_code grant), validated with exp/iss/aud, V9.1.1/V9.2.1/V9.2.3 claimed.
Select arbiter.

B: ARBITER_SELECTED: ASVS-V9-Token-Arbiter.
Trigger vocabulary: [JWT, RS256, signature-verification, exp-enforced, iss-aud-validated,
  V9.1.1, V9.2.1, V9.2.3, self-contained-token].
Excluded: ASVS-V10-OAuth-Arbiter — no [authorization-code-flow, PKCE, code_challenge,
  OAuth-authorization-server] vocabulary present.
  Internal auth service that issues JWT directly (without OAuth flow) maps to V9, not V10.
  V10 governs tokens obtained via the OAuth 2.0 authorization code flow.
Single arbiter dispatched to Node 8.

---

### Pair CB4-3.4-086 — SSDF-PW + ASVS-V6 combined: process evidence + auth controls (SELECT)
Source: Multi-source (NIST SSDF + OWASP ASVS)
License: Public domain (NIST) / CC BY-SA 4.0
Stratum: C

A: Domain processor result spans two families. Cluster 1: authentication controls
(password policy, bcrypt, MFA, ASVS V6.2.1/V6.5.5) — ASVS auth vocabulary. Cluster 2:
process evidence for authentication module (threat model TM-AUTH-2026, design review
DR-AUTH-2026-02, test results TR-AUTH-2026-04, SSDF PW.1/PW.2/PW.8 claimed) — SSDF
process vocabulary. Both clusters present. Select arbiter(s).

B: ARBITERS_SELECTED: [ASVS-V6-Auth-Arbiter, SSDF-PW-Arbiter].
Selection rationale:
  Cluster 1 → ASVS-V6-Auth-Arbiter: [password-policy, bcrypt, MFA, ASVS-V6.2.1, V6.5.5].
    Evaluates whether the authentication controls themselves satisfy ASVS requirements.
  Cluster 2 → SSDF-PW-Arbiter: [threat-model, design-review, test-results, PW.1, PW.2, PW.8].
    Evaluates whether the development process for the authentication module was secure.
Dual-arbiter rationale: these are orthogonal evaluation axes.
  ASVS-V6 asks: "Do the controls work correctly?" (runtime properties).
  SSDF-PW asks: "Was the module built with a secure process?" (process properties).
  A module can have correct ASVS controls but a flawed SSDF process, and vice versa.

Reason: ASVS and SSDF are complementary, not redundant. When a submission covers both
runtime control claims (ASVS) and process evidence (SSDF), both arbiter families are
required. Selecting only one would leave half the submission unevaluated.

---

### Pair CB4-3.4-086 — Phrasing B
Source: Multi-source (NIST SSDF + OWASP ASVS)
License: Public domain (NIST) / CC BY-SA 4.0
Stratum: C

A: Two clusters: ASVS V6 auth controls (password/bcrypt/MFA, V6.2.1/V6.5.5) + SSDF process
evidence for auth module (threat model, design review, test results, PW.1/PW.2/PW.8).
Select arbiters.

B: ARBITERS_SELECTED: [ASVS-V6-Auth-Arbiter, SSDF-PW-Arbiter].
Selection rationale:
  Cluster 1 → ASVS-V6-Auth-Arbiter: [password-policy, bcrypt, MFA, ASVS-V6.2.1, V6.5.5].
    Evaluates whether the authentication controls themselves satisfy ASVS requirements.
  Cluster 2 → SSDF-PW-Arbiter: [threat-model, design-review, test-results, PW.1, PW.2, PW.8].
    Evaluates whether the development process for the authentication module was secure.
Dual-arbiter rationale: these are orthogonal evaluation axes.
  ASVS-V6 asks: "Do the controls work correctly?" (runtime properties).
  SSDF-PW asks: "Was the module built with a secure process?" (process properties).
  A module can have correct ASVS controls but a flawed SSDF process, and vice versa.

---

### Pair CB4-3.4-087 — JWT used for session tracking: V7 governs, not V9 (SELECT)
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: D

A: Domain processor result: session management system uses short-lived JWTs as session
tokens (15-minute exp, refreshed on activity). Session vocabulary dominant: idle timeout
enforced via JWT exp, absolute session timeout via refresh_token exp, sessions invalidated
on logout via token blacklist. Team cites ASVS V7.3.1 and V7.3.3. Also mentions JWT exp
claim. No OAuth flow. No PKCE. No RS256 signing algorithm vocabulary. Select arbiter(s).

B: ARBITER_SELECTED: ASVS-V7-Session-Arbiter.
Disambiguation: JWT exp claim present but does not trigger ASVS-V9-Token-Arbiter here.
  The classification signal is the vocabulary cluster:
    Primary vocabulary: [idle-timeout, session-invalidation, refresh-token, ASVS-V7.3.1,
      ASVS-V7.3.3, session-management]. V7 session management is the governing domain.
    JWT exp: used to implement the idle timeout — exp is the mechanism, not the subject
    of evaluation. The evaluation question is "is session management correct?" (V7),
    not "is the JWT correctly formed?" (V9).
  V9-Token-Arbiter NOT selected: V9 applies when the submission's primary claim concerns
    JWT token format (signing algorithm, claims structure, signature verification as
    standalone controls). Here JWT is incidental to session management.
Single arbiter dispatched to Node 8.

Reason: JWT presence alone does not trigger V9. The arbiter family is determined by the
primary vocabulary cluster. When session management vocabulary is dominant and JWT is used
as the implementation mechanism for session tokens, V7 governs. This prevents V9 from
being triggered by every system that happens to use JWT for sessions.

---

### Pair CB4-3.4-087 — Phrasing B
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: D

A: Session management using JWT as session tokens. Idle timeout via JWT exp (15-min),
absolute timeout via refresh_token exp, logout via blacklist. ASVS V7.3.1/V7.3.3 cited.
JWT exp mentioned. No OAuth, no PKCE, no signing algorithm vocabulary. Select arbiter.

B: ARBITER_SELECTED: ASVS-V7-Session-Arbiter.
Disambiguation: JWT exp claim present but does not trigger ASVS-V9-Token-Arbiter here.
  The classification signal is the vocabulary cluster:
    Primary vocabulary: [idle-timeout, session-invalidation, refresh-token, ASVS-V7.3.1,
      ASVS-V7.3.3, session-management]. V7 session management is the governing domain.
    JWT exp: used to implement the idle timeout — exp is the mechanism, not the subject
    of evaluation. The evaluation question is "is session management correct?" (V7),
    not "is the JWT correctly formed?" (V9).
  V9-Token-Arbiter NOT selected: V9 applies when the submission's primary claim concerns
    JWT token format (signing algorithm, claims structure, signature verification as
    standalone controls). Here JWT is incidental to session management.
Single arbiter dispatched to Node 8.

---

### Pair CB4-3.4-088 — ASVS V10 + RFC 6749: both frameworks for same OAuth submission (SELECT)
Source: Multi-source (OWASP ASVS 5.0.0, IETF RFC 6749)
License: CC BY-SA 4.0 / IETF Trust (pre-5378)
Stratum: C

A: Domain processor result: OAuth 2.0 authorization code flow with PKCE S256. Both ASVS
V10.4.6 and RFC 7636 §4.2 cited by team in the same submission. Authorization server claims
conformance with both ASVS Chapter V10 requirements and RFC 6749/7636 normative requirements.
ASVS vocabulary: [V10.4.6, ASVS-V10]. RFC vocabulary: [RFC-7636-§4.2, RFC-6749-§4.1,
S256-required, authorization-code-flow]. Select arbiter(s).

B: ARBITERS_SELECTED: [ASVS-V10-OAuth-Arbiter, RFC-OAuth2-Arbiter].
Selection rationale:
  ASVS-V10-OAuth-Arbiter: triggered by [PKCE, code_challenge, S256, ASVS-V10.4.6].
    Evaluates OAuth implementation against ASVS Chapter V10 requirements.
  RFC-OAuth2-Arbiter: triggered by [RFC-7636-§4.2, RFC-6749-§4.1, S256-required,
    authorization-code-flow, RFC-normative-vocabulary].
    Evaluates OAuth implementation against RFC 6749/7636 normative requirements.
Dual-arbiter rationale: ASVS V10 and RFC 6749/7636 are complementary but distinct
  frameworks for OAuth compliance. ASVS is a security verification standard; RFC is
  the protocol specification. A submission citing both requires both to be evaluated.
  ASVS V10.4.6 and RFC 7636 §4.2 have overlapping but not identical scope.
Two arbiters dispatched to Node 8.

Reason: When a submission explicitly cites both ASVS and RFC requirements for the same
mechanism, both arbiter families apply. The dual evaluation catches cases where ASVS
and RFC requirements diverge (e.g., ASVS may be stricter than the corresponding RFC
SHOULD clause).

---

### Pair CB4-3.4-088 — Phrasing B
Source: Multi-source (OWASP ASVS 5.0.0, IETF RFC 6749)
License: CC BY-SA 4.0 / IETF Trust (pre-5378)
Stratum: C

A: OAuth 2.0 auth code flow, PKCE S256. Team cites both ASVS V10.4.6 and RFC 7636 §4.2
in same submission. Both ASVS V10 and RFC 6749/7636 vocabulary present. Select arbiters.

B: ARBITERS_SELECTED: [ASVS-V10-OAuth-Arbiter, RFC-OAuth2-Arbiter].
Selection rationale:
  ASVS-V10-OAuth-Arbiter: triggered by [PKCE, code_challenge, S256, ASVS-V10.4.6].
    Evaluates OAuth implementation against ASVS Chapter V10 requirements.
  RFC-OAuth2-Arbiter: triggered by [RFC-7636-§4.2, RFC-6749-§4.1, S256-required,
    authorization-code-flow, RFC-normative-vocabulary].
    Evaluates OAuth implementation against RFC 6749/7636 normative requirements.
Dual-arbiter rationale: ASVS V10 and RFC 6749/7636 are complementary but distinct
  frameworks for OAuth compliance. ASVS is a security verification standard; RFC is
  the protocol specification. A submission citing both requires both to be evaluated.
  ASVS V10.4.6 and RFC 7636 §4.2 have overlapping but not identical scope.
Two arbiters dispatched to Node 8.

---

## Node 7 Extension Set 1 summary

Pairs produced: CB4-3.4-081 through CB4-3.4-088 (8 base × 2 phrasings = 16 instances)

New arbiter selection rules trained:
1. JWT in OAuth flow context → V10 governs; V9 excluded (disambiguates V9/V10 overlap)
2. CWE-287 + CWE-862 combined → both Authn and Authz arbiters (independent weaknesses)
3. SSDF PO vocabulary → SSDF-PO-Arbiter (not SSDF-PW; PO ≠ PW)
4. All four source families → max 6 arbiters (one per vocabulary cluster, not one per family)
5. RS256 JWT without OAuth flow → V9 (not V10; no PKCE present)
6. SSDF-PW + ASVS-V6 combined → both arbiters (orthogonal: process vs. control)
7. JWT as session mechanism → V7 (JWT exp is mechanism; session vocab dominant)
8. ASVS V10 + RFC 6749 for same OAuth → both V10-OAuth + RFC-OAuth2 arbiters

Running sequence total: CB4-3.4-001 through CB4-3.4-088 (176 instances)
