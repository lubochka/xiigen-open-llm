# Node 1 — Domain Splitter Pairs
# Batch CB4 — Sub-phase 3.4 (Security and Authorization Model)
# Wave 1 — Stage 3 training pairs
# Sequence range: CB4-3.4-035 through CB4-3.4-040
# Base pairs: 6 × 2 phrasings = 12 pair instances
# Node role: receives raw security input; classifies into domain territories
# Production date: 2026-05-05

---

## Coverage map

| Pair | Input type | Territories produced | Source | Stratum |
|------|-----------|---------------------|--------|---------|
| 035 | OAuth/PKCE single-domain | T-oauth-oidc | OWASP-ASVS | B |
| 036 | Session management single-domain | T-session | OWASP-ASVS | B |
| 037 | Comprehensive auth multi-domain | T-auth, T-session, T-authz, T-token | OWASP-ASVS | A |
| 038 | SSDF process artifacts | T-process-evidence | NIST-SSDF | B |
| 039 | CWE access control analysis | T-cwe-authz | CWE-TOP25 | A |
| 040 | RFC JWT + OAuth protocol | T-rfc-jwt | IETF-RFCS | A |

---

## Pairs

---

### Pair CB4-3.4-035 — OAuth/PKCE input split to single T-oauth-oidc territory (SPLIT)
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: B

A: Security verification submission received. Content: "Our single-page application uses
OAuth 2.0 authorization code flow. We implemented PKCE to protect against authorization
code interception. The authorization request includes a code_challenge parameter.
code_challenge_method is set to 'plain'. Our authorization server accepts both S256 and
plain methods. We believe this satisfies ASVS 10.4.6." No session management claims. No
authentication controls claims. No token validation claims beyond what OAuth provides.
Split input into domain territories.

B: TERRITORIES: [T-oauth-oidc].
Territory T-oauth-oidc:
  Vocabulary signals: [OAuth-2.0, authorization-code-flow, PKCE, code_challenge,
    code_challenge_method=plain, ASVS-10.4.6-cited]
  Scope: OAuth and OIDC (ASVS Chapter V10)
  Claim type: team assertion ("we believe this satisfies")
  Sub-phase: 3.4 (Security and Authorization Model)
Territories not produced:
  T-auth: no [password, MFA, credential-storage] vocabulary
  T-session: no [idle-timeout, session-invalidation] vocabulary
  T-authz: no [RBAC, resource-ownership] vocabulary
  T-token: no [standalone-JWT-algorithm, token-claims-validation] vocabulary

Reason: PKCE and authorization-code-flow vocabulary produce exactly one territory:
T-oauth-oidc. The absence of authentication, session, and authorization vocabulary means
no additional territories are created. The team assertion ("we believe this satisfies") is
preserved as claim metadata — Node 1 records it but does not evaluate it.

---

### Pair CB4-3.4-035 — Phrasing B
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: B

A: Input: SPA uses OAuth 2.0 authorization code flow with PKCE. code_challenge present,
code_challenge_method='plain', server accepts both methods. Team claims ASVS 10.4.6
satisfied. No other security domains mentioned. Split into territories.

B: TERRITORIES: [T-oauth-oidc].
Territory T-oauth-oidc:
  Vocabulary signals: [OAuth-2.0, authorization-code-flow, PKCE, code_challenge,
    code_challenge_method=plain, ASVS-10.4.6-cited]
  Scope: OAuth and OIDC (ASVS Chapter V10)
  Claim type: team assertion ("we believe this satisfies")
  Sub-phase: 3.4 (Security and Authorization Model)
Territories not produced:
  T-auth: no [password, MFA, credential-storage] vocabulary
  T-session: no [idle-timeout, session-invalidation] vocabulary
  T-authz: no [RBAC, resource-ownership] vocabulary
  T-token: no [standalone-JWT-algorithm, token-claims-validation] vocabulary

---

### Pair CB4-3.4-036 — Session management input split to single T-session territory (SPLIT)
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: B

A: Security verification submission received. Content: "Our session management
implementation uses a 20-minute idle timeout with a documented risk analysis (RA-AUTH-003)
justifying this value against ASVS 7.3.1. Sessions are invalidated on logout. Our internal
standard requires 15 minutes but we have risk-accepted the deviation." No MFA or password
controls. No resource authorization controls. No JWT or token algorithm vocabulary.
Split input into domain territories.

B: TERRITORIES: [T-session].
Territory T-session:
  Vocabulary signals: [idle-timeout=20min, risk-analysis=RA-AUTH-003, ASVS-7.3.1-cited,
    session-invalidation-on-logout, internal-standard-deviation=15min]
  Scope: Session Management (ASVS Chapter V7)
  Claim type: mixed — ASVS requirement cited with supporting document; deviation from
    internal standard acknowledged
  Sub-phase: 3.4 (Security and Authorization Model)
Territories not produced:
  T-auth: no [password, MFA, credential-storage] vocabulary
  T-authz: no [RBAC, resource-ownership] vocabulary
  T-token: no [JWT, signing-algorithm, claims] vocabulary
  T-oauth-oidc: no [PKCE, authorization-code-flow] vocabulary

Reason: Idle timeout, session invalidation, and risk analysis vocabulary produce T-session.
The internal standard deviation is captured as metadata within T-session — it does not
create a separate territory. The session identifier reference does not produce T-token.

---

### Pair CB4-3.4-036 — Phrasing B
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: B

A: Input: session management uses 20-minute idle timeout. Risk analysis RA-AUTH-003
justifies deviation from 15-minute internal standard. ASVS 7.3.1 cited. Sessions
invalidated on logout. No other domains in scope. Split to territories.

B: TERRITORIES: [T-session].
Territory T-session:
  Vocabulary signals: [idle-timeout=20min, risk-analysis=RA-AUTH-003, ASVS-7.3.1-cited,
    session-invalidation-on-logout, internal-standard-deviation=15min]
  Scope: Session Management (ASVS Chapter V7)
  Claim type: mixed — ASVS requirement cited with supporting document; deviation from
    internal standard acknowledged
  Sub-phase: 3.4 (Security and Authorization Model)
Territories not produced:
  T-auth: no [password, MFA, credential-storage] vocabulary
  T-authz: no [RBAC, resource-ownership] vocabulary
  T-token: no [JWT, signing-algorithm, claims] vocabulary
  T-oauth-oidc: no [PKCE, authorization-code-flow] vocabulary

---

### Pair CB4-3.4-037 — Comprehensive auth input split to four territories (SPLIT)
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: A

A: Security verification submission received. Content: "Our backend authentication
service has the following verified controls in production: Password minimum length: 15
characters, enforced at write time. MFA required for all admin operations. Credentials
stored using bcrypt with cost factor 12. Account lockout after 10 failed attempts.
Session idle timeout: 20 minutes. Absolute session timeout: 8 hours. Sessions invalidated
on logout. Access control: RBAC enforced at API gateway. Per-endpoint resource ownership
check. JWT tokens: RS256 signed, signature verified server-side, exp and iss/aud validated.
ASVS V6, V7, V8, V9 all claimed satisfied." Split input into domain territories.

B: TERRITORIES: [T-auth, T-session, T-authz, T-token].
Territory T-auth:
  Vocabulary signals: [password=15chars, bcrypt=cost12, MFA-admin, lockout=10,
    ASVS-V6-claimed]
  Scope: Authentication (ASVS Chapter V6)
Territory T-session:
  Vocabulary signals: [idle-timeout=20min, absolute-timeout=8h,
    session-invalidation-on-logout, ASVS-V7-claimed]
  Scope: Session Management (ASVS Chapter V7)
Territory T-authz:
  Vocabulary signals: [RBAC-API-gateway, resource-ownership-check, ASVS-V8-claimed]
  Scope: Authorization (ASVS Chapter V8)
Territory T-token:
  Vocabulary signals: [JWT, RS256, signature-verification, exp-enforced, iss-aud-validated,
    ASVS-V9-claimed]
  Scope: Self-Contained Tokens (ASVS Chapter V9)
Territories not produced:
  T-oauth-oidc: no [PKCE, authorization-code-flow] vocabulary

Reason: Each ASVS chapter domain produces one territory. The four distinct vocabulary
clusters (password/MFA, session timeout, RBAC/ownership, JWT/RS256) map to four separate
territories. T-oauth-oidc is not produced because no PKCE or OAuth authorization code
flow vocabulary is present — JWT for API auth maps to T-token, not T-oauth-oidc.

---

### Pair CB4-3.4-037 — Phrasing B
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: A

A: Comprehensive auth service submission. Controls claimed: bcrypt+MFA+password(V6),
20-min idle + 8h absolute + logout invalidation(V7), RBAC+ownership(V8), JWT RS256 +
sig/exp/iss/aud(V9). No PKCE. ASVS V6-V9 all claimed satisfied. Split to territories.

B: TERRITORIES: [T-auth, T-session, T-authz, T-token].
Territory T-auth:
  Vocabulary signals: [password=15chars, bcrypt=cost12, MFA-admin, lockout=10,
    ASVS-V6-claimed]
  Scope: Authentication (ASVS Chapter V6)
Territory T-session:
  Vocabulary signals: [idle-timeout=20min, absolute-timeout=8h,
    session-invalidation-on-logout, ASVS-V7-claimed]
  Scope: Session Management (ASVS Chapter V7)
Territory T-authz:
  Vocabulary signals: [RBAC-API-gateway, resource-ownership-check, ASVS-V8-claimed]
  Scope: Authorization (ASVS Chapter V8)
Territory T-token:
  Vocabulary signals: [JWT, RS256, signature-verification, exp-enforced, iss-aud-validated,
    ASVS-V9-claimed]
  Scope: Self-Contained Tokens (ASVS Chapter V9)
Territories not produced:
  T-oauth-oidc: no [PKCE, authorization-code-flow] vocabulary

---

### Pair CB4-3.4-038 — SSDF process artifacts split to T-process-evidence territory (SPLIT)
Source: NIST SP 800-218 SSDF v1.1
License: Public domain (NIST)
Stratum: B

A: Security verification submission received. Content: "We are submitting evidence of
our secure development process for the authorization module. Artifacts: (1) Security
requirements document TS-AUTHZ-001, produced before design began. (2) Threat model
TM-AUTHZ-2026, produced before implementation. (3) Design review record DR-AUTHZ-2026-03,
conducted pre-implementation. (4) Security test results TR-AUTHZ-2026-05 including DAST
scan and ASVS checklist. We are claiming SSDF PW.1, PW.2, and PW.8 compliance."
Split input into domain territories.

B: TERRITORIES: [T-process-evidence].
Territory T-process-evidence:
  Vocabulary signals: [security-requirements=TS-AUTHZ-001, threat-model=TM-AUTHZ-2026,
    design-review=DR-AUTHZ-2026-03, test-results=TR-AUTHZ-2026-05, SSDF-PW.1-claimed,
    SSDF-PW.2-claimed, SSDF-PW.8-claimed]
  Scope: SSDF PW practice family (Produce Well-Secured Software)
  Claim type: process artifact submission with SSDF practice references
  Sub-phase: 3.4 (Security and Authorization Model)
Territories not produced:
  T-auth, T-session, T-authz, T-token, T-oauth-oidc: no ASVS vocabulary
  T-rfc-jwt: no RFC normative clause vocabulary

Reason: SSDF process artifact vocabulary (threat model document IDs, design review records,
practice family references) produces a single T-process-evidence territory. The DAST scan
and ASVS checklist within TR-AUTHZ-2026-05 do not produce additional ASVS territories —
those test results are SSDF evidence artifacts evaluated by SSDF-PW-Arbiter, not
standalone ASVS compliance claims.

---

### Pair CB4-3.4-038 — Phrasing B
Source: NIST SP 800-218 SSDF v1.1
License: Public domain (NIST)
Stratum: B

A: SSDF secure development evidence submission. Artifacts: TS-AUTHZ-001 (pre-design
requirements), TM-AUTHZ-2026 (pre-impl threat model), DR-AUTHZ-2026-03 (pre-impl design
review), TR-AUTHZ-2026-05 (DAST + ASVS checklist). Claims: PW.1, PW.2, PW.8 satisfied.
No ASVS chapter requirements cited independently. Split to territories.

B: TERRITORIES: [T-process-evidence].
Territory T-process-evidence:
  Vocabulary signals: [security-requirements=TS-AUTHZ-001, threat-model=TM-AUTHZ-2026,
    design-review=DR-AUTHZ-2026-03, test-results=TR-AUTHZ-2026-05, SSDF-PW.1-claimed,
    SSDF-PW.2-claimed, SSDF-PW.8-claimed]
  Scope: SSDF PW practice family (Produce Well-Secured Software)
  Claim type: process artifact submission with SSDF practice references
  Sub-phase: 3.4 (Security and Authorization Model)
Territories not produced:
  T-auth, T-session, T-authz, T-token, T-oauth-oidc: no ASVS vocabulary
  T-rfc-jwt: no RFC normative clause vocabulary

---

### Pair CB4-3.4-039 — CWE access control submission split to T-cwe-authz territory (SPLIT)
Source: CWE Top 25 2023
License: Public domain (MITRE)
Stratum: A

A: Security verification submission received. Content: "API endpoint
GET /api/v1/users/{userId}/profile. JWT bearer token required for all requests.
We verify the JWT signature and expiry. The userId in the path is used to look up the
profile. Any authenticated user can call this endpoint. We believe our authentication
requirement satisfies the access control requirement." No RBAC policy described. No
sub-to-userId comparison described. Split into domain territories.

B: TERRITORIES: [T-cwe-authz].
Territory T-cwe-authz:
  Vocabulary signals: [JWT-required, userId-path-parameter, authenticated-user-unrestricted,
    access-control-claim, no-ownership-check-described]
  Scope: CWE authorization weakness analysis (CWE-862, CWE-269 evaluation)
  Claim type: team assertion conflating authentication with authorization
    ("authentication requirement satisfies access control requirement")
  CWE-862 candidate: flagged — no resource ownership check described in submission
  Sub-phase: 3.4 (Security and Authorization Model)
Territories not produced:
  T-auth: authentication mechanism is mentioned but as a proxy claim for authz, not
    as a standalone authentication controls submission
  T-oauth-oidc: no PKCE or authorization code flow vocabulary

Reason: "Any authenticated user can call this endpoint" without describing a per-user
ownership check is a CWE-862 signal. The team assertion that authentication satisfies the
access control requirement is the claim-type metadata — it does not produce a T-auth
territory because the submission is about an authorization endpoint, not an authentication
control. T-cwe-authz is the appropriate territory.

---

### Pair CB4-3.4-039 — Phrasing B
Source: CWE Top 25 2023
License: Public domain (MITRE)
Stratum: A

A: GET /api/v1/users/{userId}/profile. JWT required, signature and expiry verified.
Any authenticated user can call the endpoint with any userId. Team states authentication
satisfies authorization requirement. No ownership check described. Split to territories.

B: TERRITORIES: [T-cwe-authz].
Territory T-cwe-authz:
  Vocabulary signals: [JWT-required, userId-path-parameter, authenticated-user-unrestricted,
    access-control-claim, no-ownership-check-described]
  Scope: CWE authorization weakness analysis (CWE-862, CWE-269 evaluation)
  Claim type: team assertion conflating authentication with authorization
    ("authentication requirement satisfies access control requirement")
  CWE-862 candidate: flagged — no resource ownership check described in submission
  Sub-phase: 3.4 (Security and Authorization Model)
Territories not produced:
  T-auth: authentication mechanism is mentioned but as a proxy claim for authz, not
    as a standalone authentication controls submission
  T-oauth-oidc: no PKCE or authorization code flow vocabulary

---

### Pair CB4-3.4-040 — RFC JWT + OAuth submission split to T-rfc-jwt territory (SPLIT)
Source: IETF RFCs (RFC 7519, RFC 6749)
License: IETF Trust (pre-5378 works)
Stratum: A

A: Security verification submission received. Content: "Our authorization service uses
JWT access tokens signed with HS256. The same signing key is shared among all 6 resource
servers. Tokens are issued by our OAuth 2.0 authorization server. We validate signature,
exp, iss, and aud on each resource server. We believe this conforms to RFC 7519."
Split into domain territories.

B: TERRITORIES: [T-rfc-jwt].
Territory T-rfc-jwt:
  Vocabulary signals: [JWT, HS256, shared-key=6-resource-servers, OAuth-2.0-issuer,
    signature-validation, exp-iss-aud-validated, RFC-7519-cited]
  Scope: IETF RFC protocol conformance (RFC 7519, RFC 6749)
  Claim type: team assertion ("we believe this conforms to RFC 7519")
  RFC 7519 §8 candidate: flagged — HS256 shared key across multiple resource servers
    creates cross-service token forgery capability; trust boundary analysis required
  Sub-phase: 3.4 (Security and Authorization Model)
Territories not produced:
  T-token: RFC conformance analysis is in scope, not ASVS V9 standalone token validation
  T-oauth-oidc: no PKCE vocabulary; OAuth vocabulary present but subsumed in T-rfc-jwt
    for this submission's primary conformance claim

Reason: RFC 7519 citation and HS256 shared key vocabulary produce T-rfc-jwt for RFC
conformance analysis. The shared key across 6 resource servers is flagged as a §8 trust
boundary candidate at domain splitting — Node 1 does not evaluate the claim, only records
the signal. The OAuth 2.0 issuer mention is subsumed in T-rfc-jwt here because the primary
submission claim is RFC 7519 JWT conformance.

---

### Pair CB4-3.4-040 — Phrasing B
Source: IETF RFCs (RFC 7519, RFC 6749)
License: IETF Trust (pre-5378 works)
Stratum: A

A: JWT access tokens, HS256, shared key across 6 resource servers. OAuth 2.0
authorization server issues tokens. exp/iss/aud validated on each resource server.
Team claims RFC 7519 conformance. Split to territories.

B: TERRITORIES: [T-rfc-jwt].
Territory T-rfc-jwt:
  Vocabulary signals: [JWT, HS256, shared-key=6-resource-servers, OAuth-2.0-issuer,
    signature-validation, exp-iss-aud-validated, RFC-7519-cited]
  Scope: IETF RFC protocol conformance (RFC 7519, RFC 6749)
  Claim type: team assertion ("we believe this conforms to RFC 7519")
  RFC 7519 §8 candidate: flagged — HS256 shared key across multiple resource servers
    creates cross-service token forgery capability; trust boundary analysis required
  Sub-phase: 3.4 (Security and Authorization Model)
Territories not produced:
  T-token: RFC conformance analysis is in scope, not ASVS V9 standalone token validation
  T-oauth-oidc: no PKCE vocabulary; OAuth vocabulary present but subsumed in T-rfc-jwt
    for this submission's primary conformance claim

---

## Node 1 summary

Pairs produced: CB4-3.4-035 through CB4-3.4-040 (6 base × 2 phrasings = 12 instances)

Splitting rules trained:
1. PKCE/OAuth-only input → [T-oauth-oidc] only
2. Session management only → [T-session] only
3. Comprehensive multi-domain input → one territory per vocabulary cluster
4. SSDF process artifact input → [T-process-evidence]; test results inside do not split further
5. IDOR/access-control pattern → [T-cwe-authz]; JWT auth claim does not produce T-auth
6. RFC citation + shared-key → [T-rfc-jwt]; RFC conformance takes precedence over ASVS V9

Running sequence total after this file: CB4-3.4-001 through CB4-3.4-040 (80 instances)
