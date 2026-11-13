# Node 7 — Arbiter Selector Pairs
# Batch CB4 — Sub-phase 3.4 (Security and Authorization Model)
# Wave 1 — Stage 3 training pairs
# Sequence range: CB4-3.4-013 through CB4-3.4-020
# Base pairs: 8 × 2 phrasings = 16 pair instances
# Node role: receives domain-processed results; selects arbiter unit(s) for Node 8
# Production date: 2026-05-05

---

## Coverage map

| Pair | Arbiter selected | Source | Stratum | Key signal |
|------|-----------------|--------|---------|-----------|
| 013 | ASVS-V10-OAuth-Arbiter | OWASP-ASVS | B | PKCE/code_challenge_method vocabulary |
| 014 | ASVS-V6+V7+V8+V9-Arbiters | OWASP-ASVS | A | Multi-territory comprehensive input |
| 015 | ASVS-V7-Session-Arbiter | OWASP-ASVS | B | Idle-timeout + risk-analysis vocabulary |
| 016 | CWE-Authz-Arbiter | CWE-TOP25 | A | Access-control + ownership-check vocabulary |
| 017 | CWE-Authz-Arbiter (CWE-269 path) | CWE-TOP25 | D | Privilege-escalation edge case |
| 018 | RFC-JWT-Arbiter + RFC-OAuth2-Arbiter | IETF-RFCS | A | HS256 + trust-boundary vocabulary |
| 019 | SSDF-PW-Arbiter | NIST-SSDF | A | Process-evidence + PW-family vocabulary |
| 020 | ASVS-V6 + RFC-JWT + SSDF-PW | Multi-source | B | Three-cluster composite input |

---

## Pairs

---

### Pair CB4-3.4-013 — ASVS-V10-OAuth-Arbiter single selection for PKCE input (SELECT)
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: B

A: The domain processor produced results for a single-page application security submission.
OAuth 2.0 authorization code flow is in scope. PKCE is implemented: code_challenge present
in authorization requests, code_challenge_method set to 'plain'. No authentication chapter
(V6) controls submitted. No session management (V7) controls submitted. No self-contained
token validation (V9) controls beyond what the OAuth flow provides. Territory: OAuth and
OIDC. Select the arbiter unit(s) to invoke in Node 8.

B: ARBITER_SELECTED: ASVS-V10-OAuth-Arbiter.
Trigger vocabulary matched: [authorization-code-flow, PKCE, code_challenge, code_challenge_method].
Excluded arbiters:
  ASVS-V6-Auth-Arbiter: no [password-policy, MFA, credential-storage] vocabulary
  ASVS-V7-Session-Arbiter: no [idle-timeout, session-invalidation] vocabulary
  ASVS-V8-AuthZ-Arbiter: no [resource-ownership, RBAC, access-control] vocabulary
  ASVS-V9-Token-Arbiter: no [standalone-JWT-validation, signing-algorithm, claims-check] vocabulary
Single arbiter dispatched to Node 8.

Reason: PKCE vocabulary is a deterministic selection signal for ASVS-V10-OAuth-Arbiter. The
five ASVS arbiters cover five distinct chapter territories — only arbiters whose territory
vocabulary is present in the processed input are selected. Selecting all ASVS arbiters
regardless of vocabulary present produces failure mode FM-4 (ARBITER_SELECTION_GAP): Node 8
receives arbiters without evidence to evaluate, producing spurious PASS verdicts for
uninvestigated territories.

---

### Pair CB4-3.4-013 — Phrasing B
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: B

A: Domain processing complete. Input covers one security territory: OAuth 2.0 with PKCE.
Submission contains code_challenge and code_challenge_method parameters. Authorization code
flow confirmed in scope. No controls for password policy, session invalidation, resource
ownership enforcement, or JWT claim validation were submitted. Arbiter selection required
for Node 8 invocation.

B: ARBITER_SELECTED: ASVS-V10-OAuth-Arbiter.
Trigger vocabulary matched: [authorization-code-flow, PKCE, code_challenge, code_challenge_method].
Excluded arbiters:
  ASVS-V6-Auth-Arbiter: no [password-policy, MFA, credential-storage] vocabulary
  ASVS-V7-Session-Arbiter: no [idle-timeout, session-invalidation] vocabulary
  ASVS-V8-AuthZ-Arbiter: no [resource-ownership, RBAC, access-control] vocabulary
  ASVS-V9-Token-Arbiter: no [standalone-JWT-validation, signing-algorithm, claims-check] vocabulary
Single arbiter dispatched to Node 8.

---

### Pair CB4-3.4-014 — ASVS multi-arbiter selection for comprehensive authentication input (SELECT)
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: A

A: The domain processor produced results covering four security territories. Territory T-auth:
password minimum 15 characters, MFA enforcement, credential storage using bcrypt cost 12,
V6.2.1 claimed satisfied. Territory T-session: idle timeout 20 minutes, absolute timeout 8
hours, session invalidation on logout, V7.3.1 claimed satisfied. Territory T-authz: RBAC
enforced at API gateway, resource ownership verified per endpoint, V8.2.1 and V8.2.2
claimed satisfied. Territory T-token: JWT RS256 signed, signature verified server-side,
expiry enforced, V9.1.1 and V9.2.1 claimed satisfied. No PKCE or OAuth authorization code
flow vocabulary present. Select arbiter unit(s) for Node 8.

B: ARBITERS_SELECTED: [ASVS-V6-Auth-Arbiter, ASVS-V7-Session-Arbiter, ASVS-V8-AuthZ-Arbiter, ASVS-V9-Token-Arbiter].
Trigger vocabulary by arbiter:
  ASVS-V6-Auth-Arbiter: [password-policy, MFA, credential-storage, bcrypt, V6.2.1]
  ASVS-V7-Session-Arbiter: [idle-timeout, absolute-timeout, session-invalidation, V7.3.1]
  ASVS-V8-AuthZ-Arbiter: [RBAC, resource-ownership, API-gateway, V8.2.1, V8.2.2]
  ASVS-V9-Token-Arbiter: [JWT, RS256, signature-verification, expiry-enforcement, V9.1.1, V9.2.1]
Excluded: ASVS-V10-OAuth-Arbiter — no [authorization-code-flow, PKCE, code_challenge] vocabulary present.
Four arbiters dispatched to Node 8.

Reason: Each ASVS arbiter corresponds to one chapter territory. All four territories are
represented by vocabulary in the processed input — all four arbiters are selected.
ASVS-V10-OAuth-Arbiter is excluded because its trigger vocabulary (PKCE,
authorization-code-flow) is absent. The presence of JWT (RS256) alone is not sufficient to
redirect to V10 — RS256 JWT for API authentication maps to V9 (self-contained tokens), not
V10 (OAuth and OIDC).

---

### Pair CB4-3.4-014 — Phrasing B
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: A

A: Backend authentication service submission processed. Four distinct security domains
identified: authentication controls (V6 vocabulary: bcrypt, MFA, password length enforcement),
session management controls (V7 vocabulary: idle timeout, absolute timeout, logout
invalidation), authorization controls (V8 vocabulary: RBAC, ownership check per endpoint),
token validation controls (V9 vocabulary: JWT RS256, signature check, expiry). No OAuth 2.0
authorization flow, no PKCE, no code_challenge vocabulary. Select arbiters for Node 8.

B: ARBITERS_SELECTED: [ASVS-V6-Auth-Arbiter, ASVS-V7-Session-Arbiter, ASVS-V8-AuthZ-Arbiter, ASVS-V9-Token-Arbiter].
Trigger vocabulary by arbiter:
  ASVS-V6-Auth-Arbiter: [password-policy, MFA, credential-storage, bcrypt, V6.2.1]
  ASVS-V7-Session-Arbiter: [idle-timeout, absolute-timeout, session-invalidation, V7.3.1]
  ASVS-V8-AuthZ-Arbiter: [RBAC, resource-ownership, API-gateway, V8.2.1, V8.2.2]
  ASVS-V9-Token-Arbiter: [JWT, RS256, signature-verification, expiry-enforcement, V9.1.1, V9.2.1]
Excluded: ASVS-V10-OAuth-Arbiter — no [authorization-code-flow, PKCE, code_challenge] vocabulary present.
Four arbiters dispatched to Node 8.

---

### Pair CB4-3.4-015 — ASVS-V7-Session-Arbiter single selection for session management input (SELECT)
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: B

A: Domain processing complete for session management submission. Input contains: idle timeout
set to 20 minutes, absolute session timeout set to 8 hours, documented risk analysis
RA-AUTH-003 justifying the 20-minute idle value, ASVS 7.3.1 cited by submitting team. No
vocabulary from authentication chapter (no password policy, MFA, or credential storage
terms). No resource authorization vocabulary. No JWT signing algorithm or token claims
vocabulary beyond session identifier reference. Select arbiter(s) for Node 8.

B: ARBITER_SELECTED: ASVS-V7-Session-Arbiter.
Trigger vocabulary matched: [idle-timeout, absolute-timeout, risk-analysis-RA-AUTH-003, ASVS-7.3.1, session-management].
Excluded arbiters:
  ASVS-V6-Auth-Arbiter: no [password-policy, MFA, credential-storage] vocabulary
  ASVS-V8-AuthZ-Arbiter: no [RBAC, resource-ownership, authorization-check] vocabulary
  ASVS-V9-Token-Arbiter: no [JWT, signing-algorithm, claims-validation] vocabulary
  ASVS-V10-OAuth-Arbiter: no [PKCE, authorization-code-flow] vocabulary
Single arbiter dispatched to Node 8.

Reason: Session management vocabulary (idle-timeout, documented risk analysis, ASVS V7
requirement reference) unambiguously maps to ASVS-V7-Session-Arbiter. The session identifier
used to track idle time does not trigger ASVS-V9-Token-Arbiter — that arbiter requires JWT
or structured token claims vocabulary (signing algorithm, iss/sub/aud/exp claims). Selecting
V9 from session-identifier presence alone would be an arbiter selection gap.

---

### Pair CB4-3.4-015 — Phrasing B
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: B

A: Session management domain result received. Processed content: 20-minute idle timeout,
risk analysis document RA-AUTH-003 referenced as justification, ASVS 7.3.1 satisfaction
claimed. Input does not contain: password or MFA controls, access control enforcement terms,
JWT algorithm or claims terms, PKCE parameters. Single domain territory: session management.
Identify arbiter for Node 8.

B: ARBITER_SELECTED: ASVS-V7-Session-Arbiter.
Trigger vocabulary matched: [idle-timeout, absolute-timeout, risk-analysis-RA-AUTH-003, ASVS-7.3.1, session-management].
Excluded arbiters:
  ASVS-V6-Auth-Arbiter: no [password-policy, MFA, credential-storage] vocabulary
  ASVS-V8-AuthZ-Arbiter: no [RBAC, resource-ownership, authorization-check] vocabulary
  ASVS-V9-Token-Arbiter: no [JWT, signing-algorithm, claims-validation] vocabulary
  ASVS-V10-OAuth-Arbiter: no [PKCE, authorization-code-flow] vocabulary
Single arbiter dispatched to Node 8.

---

### Pair CB4-3.4-016 — CWE-Authz-Arbiter selection for access control input (SELECT)
Source: CWE Top 25 2023
License: Public domain (MITRE)
Stratum: A

A: Domain processing complete. Input: REST API endpoint GET /api/users/{userId}/profile.
JWT bearer token required. Domain processor result: resource ownership check present — JWT
sub claim compared to userId path parameter at lines 34–36 of UserController.cs. Admin
scope bypass documented and confirmed present. CWE candidate evaluation flags: CWE-862
(missing authorization check) evaluated — ownership check is present, not confirmed.
CWE-269 (improper privilege management) evaluated — scope boundaries verified, not
confirmed. No authentication failure vocabulary. Select arbiter(s) for Node 8.

B: ARBITER_SELECTED: CWE-Authz-Arbiter.
Trigger vocabulary matched: [resource-ownership-check, access-control, authorization-check, CWE-862-candidate, CWE-269-candidate].
Excluded: CWE-Authn-Arbiter — no [CWE-287, CWE-306, missing-authentication, authentication-bypass] vocabulary present.
Single arbiter dispatched to Node 8.
CWE-Authz-Arbiter evaluation paths available: CWE-862 (absent check), CWE-269 (wrong-scope
check), CWE-863 (incorrect authorization). All three paths active when authorization
vocabulary is present — Node 8 determines which path applies.

Reason: Resource ownership check vocabulary and CWE-862/CWE-269 candidate mentions are
deterministic signals for CWE-Authz-Arbiter. CWE-Authn-Arbiter is for authentication
mechanism weaknesses (CWE-287: improper authentication, CWE-306: missing authentication
for critical function) — the input contains authorization checks, not authentication
mechanism failures. CWE-Authz-Arbiter applies regardless of whether the eventual Node 8
verdict will be PASS, REPROCESS, or CONCERN.

---

### Pair CB4-3.4-016 — Phrasing B
Source: CWE Top 25 2023
License: Public domain (MITRE)
Stratum: A

A: API authorization domain processing complete. Endpoint: GET /api/users/{userId}/profile.
Authorization controls present: JWT required, sub-to-path-parameter ownership check
implemented, admin scope check implemented. Domain processor flagged CWE-862 candidate —
evaluated, ownership check confirmed present, candidate not triggered. CWE-269 candidate —
evaluated, scope enforcement confirmed present, candidate not triggered. No
missing-authentication or credential-bypass vocabulary. Select arbiter for Node 8.

B: ARBITER_SELECTED: CWE-Authz-Arbiter.
Trigger vocabulary matched: [resource-ownership-check, access-control, authorization-check, CWE-862-candidate, CWE-269-candidate].
Excluded: CWE-Authn-Arbiter — no [CWE-287, CWE-306, missing-authentication, authentication-bypass] vocabulary present.
Single arbiter dispatched to Node 8.
CWE-Authz-Arbiter evaluation paths available: CWE-862 (absent check), CWE-269 (wrong-scope
check), CWE-863 (incorrect authorization). All three paths active when authorization
vocabulary is present — Node 8 determines which path applies.

---

### Pair CB4-3.4-017 — CWE-Authz-Arbiter for privilege escalation (CWE-269 path signal) (SELECT)
Source: CWE Top 25 2023
License: Public domain (MITRE)
Stratum: D

A: Domain processing complete. Input: POST /api/workspaces/{workspaceId}/members.
Authorization gate present: editor-or-admin check executes before member addition. JWT
required. Domain processor result: privilege escalation pattern identified — editor role
can assign admin role to other members. Vocabulary signals: [privilege-escalation,
role-assignment, scope-boundary-violation]. Authorization check is present but scope is too
broad. No missing-authentication vocabulary. Select arbiter(s) for Node 8 and indicate
evaluation path.

B: ARBITER_SELECTED: CWE-Authz-Arbiter.
Trigger vocabulary matched: [privilege-escalation, role-assignment, scope-boundary-violation, authorization-gate-present].
Evaluation path signal to Node 8: CWE-269 (improper privilege management — wrong-scope
check, not absent check). CWE-862 path not primary — authorization gate executes.
Excluded: CWE-Authn-Arbiter — no [missing-authentication, CWE-287, CWE-306] vocabulary.
Note to Node 8: the authorization gate executes — this is not a missing-check case. Node 8
must evaluate whether the gate enforces correct scope boundaries (editor must not be
permitted to assign admin role).

Reason: Privilege escalation vocabulary maps to CWE-Authz-Arbiter's CWE-269 evaluation path.
The critical distinction is that an authorization check is present and executing — the
scope of that check is too broad. CWE-862 path applies when the check is absent. Node 7
passes the evaluation path signal to Node 8 to prevent failure mode FM-CWE-2
(CWE_CATEGORY_CONFUSION: applying CWE-862 reasoning to a CWE-269 case). CWE-Authn-Arbiter
is not selected: role assignment scope is an authorization boundary problem, not an
authentication mechanism problem.

---

### Pair CB4-3.4-017 — Phrasing B
Source: CWE Top 25 2023
License: Public domain (MITRE)
Stratum: D

A: Authorization domain result for workspace member management endpoint.
POST /api/workspaces/{workspaceId}/members. Finding: authorization check present and
executing. Scope of check: editor-or-admin gate passes for both editor and admin roles.
Editor can subsequently assign admin role to other members. Identified pattern: wrong-scope
authorization (privilege escalation via role assignment). CWE-862 evaluation: check is
present — not primary path. CWE-269 evaluation: check scope too broad — primary path. No
authentication bypass vocabulary. Select arbiter and evaluation path for Node 8.

B: ARBITER_SELECTED: CWE-Authz-Arbiter.
Trigger vocabulary matched: [privilege-escalation, role-assignment, scope-boundary-violation, authorization-gate-present].
Evaluation path signal to Node 8: CWE-269 (improper privilege management — wrong-scope
check, not absent check). CWE-862 path not primary — authorization gate executes.
Excluded: CWE-Authn-Arbiter — no [missing-authentication, CWE-287, CWE-306] vocabulary.
Note to Node 8: the authorization gate executes — this is not a missing-check case. Node 8
must evaluate whether the gate enforces correct scope boundaries (editor must not be
permitted to assign admin role).

---

### Pair CB4-3.4-018 — RFC-JWT-Arbiter and RFC-OAuth2-Arbiter selection for HS256 shared key input (SELECT)
Source: IETF RFCs (RFC 7519, RFC 6749)
License: IETF Trust (pre-5378 works)
Stratum: A

A: Domain processing complete. Security input: JWT access tokens used for API authorization.
Signing algorithm: HS256. Key deployment: single shared key distributed to 6 resource
servers. OAuth 2.0 authorization server in use. Domain processor flagged RFC 7519 §8 trust
boundary analysis — any resource server holding the shared HS256 key can forge tokens
accepted by all other resource servers. OAuth 2.0 authorization server vocabulary also
present (authorization endpoint, token endpoint, grant type). Select arbiter(s) for Node 8.

B: ARBITERS_SELECTED: [RFC-JWT-Arbiter, RFC-OAuth2-Arbiter].
Trigger vocabulary by arbiter:
  RFC-JWT-Arbiter: [HS256, shared-key, key-distribution, RFC-7519, trust-boundary, cross-service-forgery-capability]
  RFC-OAuth2-Arbiter: [authorization-server, OAuth-2.0, authorization-endpoint, token-endpoint]
Both arbiters selected — JWT token format vocabulary and OAuth 2.0 protocol vocabulary are
present and cover distinct RFC conformance domains.
Dispatch order: RFC-JWT-Arbiter first (trust boundary violation is the primary signal),
RFC-OAuth2-Arbiter second.

Reason: HS256 with shared-key-distribution vocabulary triggers RFC-JWT-Arbiter for trust
boundary evaluation under RFC 7519 §8. The OAuth 2.0 authorization server vocabulary
additionally triggers RFC-OAuth2-Arbiter for protocol conformance under RFC 6749. Both
arbiters are selected because the input spans two distinct RFC domains (token format vs.
authorization protocol). Selecting only RFC-JWT-Arbiter would leave OAuth 2.0 protocol
conformance unchecked; selecting only RFC-OAuth2-Arbiter would miss the HS256 trust
boundary violation.

---

### Pair CB4-3.4-018 — Phrasing B
Source: IETF RFCs (RFC 7519, RFC 6749)
License: IETF Trust (pre-5378 works)
Stratum: A

A: JWT and OAuth domain processing result. Access tokens: JWT, algorithm HS256, single key
shared across 6 resource servers. RFC 7519 §8 referenced by domain processor: HS256
deployment creates cross-service forgery capability — any resource server key holder can
produce forged tokens. Authorization server: OAuth 2.0 RFC 6749 deployment with
authorization and token endpoints. Both JWT-specific vocabulary and OAuth-protocol
vocabulary present. Select arbiter units for Node 8.

B: ARBITERS_SELECTED: [RFC-JWT-Arbiter, RFC-OAuth2-Arbiter].
Trigger vocabulary by arbiter:
  RFC-JWT-Arbiter: [HS256, shared-key, key-distribution, RFC-7519, trust-boundary, cross-service-forgery-capability]
  RFC-OAuth2-Arbiter: [authorization-server, OAuth-2.0, authorization-endpoint, token-endpoint]
Both arbiters selected — JWT token format vocabulary and OAuth 2.0 protocol vocabulary are
present and cover distinct RFC conformance domains.
Dispatch order: RFC-JWT-Arbiter first (trust boundary violation is the primary signal),
RFC-OAuth2-Arbiter second.

---

### Pair CB4-3.4-019 — SSDF-PW-Arbiter selection for process evidence input (SELECT)
Source: NIST SP 800-218 SSDF v1.1
License: Public domain (NIST)
Stratum: A

A: Domain processing complete. Input contains: security requirements document TS-AUTHZ-001
(pre-design, week 1), threat model TM-AUTHZ-2026 (pre-implementation, week 2), design
review record DR-AUTHZ-2026-03 (pre-implementation, week 4), test results TR-AUTHZ-2026-05
(ZAP Dynamic Application Security Testing (DAST) scan and ASVS checklist, week 6).
Vocabulary signals: [threat-model, design-review, process-evidence, PW-practice-family,
pre-implementation-artifact]. No PO (Prepare the Organization) practice family vocabulary
(no security training curricula, no tool acquisition records, no organizational policy
documents). Select arbiter(s) for Node 8.

B: ARBITER_SELECTED: SSDF-PW-Arbiter.
Trigger vocabulary matched: [threat-model, design-review, security-requirements, process-evidence, PW.1, PW.2, PW.8].
Practice family scope: SSDF-PW-Arbiter covers PW.1 (security requirements established
before design), PW.2 (design review for security), PW.8 (security testing).
Excluded: SSDF-PO-Arbiter — no [PO.1, PO.2, training-records, organizational-policy,
tool-acquisition] vocabulary present.
Single arbiter dispatched to Node 8.

Reason: Process evidence vocabulary (threat model, design review artifacts, test results
with SSDF PW references) selects SSDF-PW-Arbiter. SSDF-PO-Arbiter covers organizational
preparation practices (security policies, training, tooling provisioning) — none of that
vocabulary is present. The presence of test results (TR-AUTHZ-2026-05) does not re-route
to an ASVS arbiter: SSDF-PW-Arbiter evaluates testing evidence as part of the PW.8
practice, which is distinct from ASVS runtime compliance testing coverage.

---

### Pair CB4-3.4-019 — Phrasing B
Source: NIST SP 800-218 SSDF v1.1
License: Public domain (NIST)
Stratum: A

A: Process evidence domain processing result. Artifacts received: security requirements
document (pre-design), threat model (pre-implementation), design review record
(pre-implementation), test results (ZAP scan + ASVS checklist). NIST SSDF PW (Produce
Well-Secured Software) practice family referenced in domain processor output: PW.1
(requirements established), PW.2 (design reviewed), PW.8 (testing performed). No
organizational training or policy artifacts submitted. No SSDF PO (Prepare the
Organization) practice family vocabulary. Select arbiter for Node 8.

B: ARBITER_SELECTED: SSDF-PW-Arbiter.
Trigger vocabulary matched: [threat-model, design-review, security-requirements, process-evidence, PW.1, PW.2, PW.8].
Practice family scope: SSDF-PW-Arbiter covers PW.1 (security requirements established
before design), PW.2 (design review for security), PW.8 (security testing).
Excluded: SSDF-PO-Arbiter — no [PO.1, PO.2, training-records, organizational-policy,
tool-acquisition] vocabulary present.
Single arbiter dispatched to Node 8.

---

### Pair CB4-3.4-020 — Multi-source arbiter selection for composite security input (SELECT)
Source: Multi-source (OWASP ASVS 5.0.0, IETF RFC 7519, NIST SP 800-218 SSDF v1.1)
License: CC BY-SA 4.0 / IETF Trust (pre-5378 works) / Public domain (NIST)
Stratum: B

A: Domain processing complete. Input spans three vocabulary domains. Domain 1: authentication
controls — password policy enforced, Multi-Factor Authentication (MFA) required, bcrypt
storage, ASVS V6 requirement IDs cited. Domain 2: JWT access tokens — algorithm RS256, RFC
7519 claim validation, iss/sub/exp/aud verified server-side. Domain 3: process evidence —
threat model and design review artifacts submitted, SSDF PW.2 referenced explicitly.
PKCE vocabulary absent. CWE access-control or Insecure Direct Object Reference (IDOR)
vocabulary absent. OAuth authorization code flow absent. Select all arbiters required for
Node 8.

B: ARBITERS_SELECTED: [ASVS-V6-Auth-Arbiter, RFC-JWT-Arbiter, SSDF-PW-Arbiter].
Selection rationale by domain:
  Domain 1 → ASVS-V6-Auth-Arbiter: [password-policy, MFA, bcrypt, ASVS-V6, credential-storage]
  Domain 2 → RFC-JWT-Arbiter: [RS256, RFC-7519, claim-validation, iss, sub, exp, aud]
  Domain 3 → SSDF-PW-Arbiter: [threat-model, design-review, PW.2, process-evidence]
Excluded arbiters:
  ASVS-V10-OAuth-Arbiter: no [PKCE, authorization-code-flow, code_challenge] vocabulary
  CWE-Authz-Arbiter: no [access-control, IDOR, resource-ownership-check] vocabulary
  RFC-OAuth2-Arbiter: no [authorization-server, OAuth-2.0-flow, token-endpoint] vocabulary
Three arbiters from three distinct source families dispatched to Node 8.

Reason: Multi-source inputs require one arbiter per vocabulary cluster, drawn from the
appropriate source family. The selection boundary is vocabulary-driven: ASVS-V6 handles
authentication compliance, RFC-JWT handles JWT token format conformance, SSDF-PW handles
process evidence completeness. Arbiters from source families with no vocabulary
representation must be excluded. RS256 JWT does not trigger ASVS-V9-Token-Arbiter here
because V9 vocabulary requires standalone token validation claims in an authorization
context — the JWT in this input appears in a process evidence domain context alongside
SSDF artifacts, not as a standalone token verification submission.

---

### Pair CB4-3.4-020 — Phrasing B
Source: Multi-source (OWASP ASVS 5.0.0, IETF RFC 7519, NIST SP 800-218 SSDF v1.1)
License: CC BY-SA 4.0 / IETF Trust (pre-5378 works) / Public domain (NIST)
Stratum: B

A: Three-domain security verification input processed. Authentication controls vocabulary:
bcrypt, MFA, password policy, ASVS V6 requirement IDs. JWT vocabulary: RS256 algorithm,
RFC 7519 standard claim set (iss, sub, exp, aud verified). Process evidence vocabulary:
threat model TM-AUTHZ-2026, design review DR-AUTHZ-2026-03, SSDF PW.2 referenced. No
PKCE vocabulary. No OAuth authorization code flow vocabulary. No IDOR or access control
ownership-check vocabulary. Select arbiter set for Node 8 evaluation.

B: ARBITERS_SELECTED: [ASVS-V6-Auth-Arbiter, RFC-JWT-Arbiter, SSDF-PW-Arbiter].
Selection rationale by domain:
  Domain 1 → ASVS-V6-Auth-Arbiter: [password-policy, MFA, bcrypt, ASVS-V6, credential-storage]
  Domain 2 → RFC-JWT-Arbiter: [RS256, RFC-7519, claim-validation, iss, sub, exp, aud]
  Domain 3 → SSDF-PW-Arbiter: [threat-model, design-review, PW.2, process-evidence]
Excluded arbiters:
  ASVS-V10-OAuth-Arbiter: no [PKCE, authorization-code-flow, code_challenge] vocabulary
  CWE-Authz-Arbiter: no [access-control, IDOR, resource-ownership-check] vocabulary
  RFC-OAuth2-Arbiter: no [authorization-server, OAuth-2.0-flow, token-endpoint] vocabulary
Three arbiters from three distinct source families dispatched to Node 8.

---

## Node 7 summary

Pairs produced: CB4-3.4-013 through CB4-3.4-020 (8 base × 2 phrasings = 16 instances)

Arbiter selection rules trained:
1. PKCE/OAuth vocabulary → ASVS-V10-OAuth-Arbiter (not all ASVS arbiters)
2. Multi-territory comprehensive input → one arbiter per territory, ASVS V6/V7/V8/V9
3. Session-management-only vocabulary → ASVS-V7-Session-Arbiter (session identifier alone does not trigger V9)
4. Access-control/IDOR vocabulary → CWE-Authz-Arbiter (not CWE-Authn-Arbiter)
5. Privilege escalation with present gate → CWE-Authz-Arbiter with CWE-269 path signal
6. HS256 + OAuth vocabulary → RFC-JWT-Arbiter + RFC-OAuth2-Arbiter (both required)
7. Process evidence + PW vocabulary → SSDF-PW-Arbiter (not SSDF-PO-Arbiter)
8. Multi-source composite → one arbiter per source family represented

Failure modes trained against: FM-4 (ARBITER_SELECTION_GAP), FM-CWE-2 (CWE_CATEGORY_CONFUSION)

Running sequence total after this file: CB4-3.4-001 through CB4-3.4-020 (40 instances)
