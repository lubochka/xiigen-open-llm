# Node 4 — Domain Processor Pairs
# Batch CB4 — Sub-phase 3.4 (Security and Authorization Model)
# Wave 1 — Stage 3 training pairs
# Sequence range: CB4-3.4-046 through CB4-3.4-050
# Base pairs: 5 × 2 phrasings = 10 pair instances
# Node role: evaluates one territory against its source requirements; produces structured finding
# Production date: 2026-05-05

---

## Coverage map

| Pair | Territory | Source | Finding | Stratum |
|------|-----------|--------|---------|---------|
| 046 | T-auth (ASVS V6) | OWASP-ASVS | All controls satisfied — PASS finding | A |
| 047 | T-oauth-oidc (ASVS V10) | OWASP-ASVS | plain method violation — REPROCESS finding | D |
| 048 | T-cwe-authz (CWE-862) | CWE-TOP25 | ownership check absent — REPROCESS finding | D |
| 049 | T-process-evidence (SSDF PW.2) | NIST-SSDF | post-implementation review — CONCERN finding | D |
| 050 | T-rfc-jwt (RFC 7519 §8) | IETF-RFCS | HS256 trust boundary — REPROCESS finding | D |

---

## Pairs

---

### Pair CB4-3.4-046 — T-auth processing: all ASVS V6 controls satisfied (PROCESS)
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: A

A: Territory T-auth received for processing. Signals: password minimum 15 characters
enforced at write time; bcrypt with cost factor 12 for credential storage; MFA required
for all admin operations; account lockout after 10 consecutive failures. Team claims
V6.2.1 and V6.5.5 satisfied. Evaluate against ASVS Chapter V6 requirements.

B: DOMAIN_PROCESSOR_RESULT: T-auth.
Requirement evaluations:
  V6.2.1 (Password minimum length ≥ 12 at write time): SATISFIED.
    Evidence: password minimum 15 characters enforced at write time.
  V6.5.5 (MFA for privileged access): SATISFIED.
    Evidence: MFA required for all admin operations.
  V6.2.3 (Credential storage: one-way adaptive hash): SATISFIED.
    Evidence: bcrypt cost 12 — adaptive, one-way, resistant to brute-force.
  V6.5.2 (Account lockout or throttling after repeated failures): SATISFIED.
    Evidence: lockout after 10 consecutive failures.
Overall T-auth finding: PASS — all evaluated V6 requirements satisfied by submitted evidence.
No gaps identified in authentication controls.

Reason: Domain processor evaluates each requirement individually against submitted evidence.
A PASS finding requires positive evidence for each requirement — the processor does not
infer satisfaction from the absence of stated failures. bcrypt cost 12 satisfies V6.2.3
because bcrypt is an adaptive one-way hash with documented cost parameter.

---

### Pair CB4-3.4-046 — Phrasing B
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: A

A: T-auth signals: password 15 chars enforced, bcrypt cost 12, MFA for admin ops,
lockout at 10 failures. V6.2.1 and V6.5.5 claimed. Evaluate V6 requirements.

B: DOMAIN_PROCESSOR_RESULT: T-auth.
Requirement evaluations:
  V6.2.1 (Password minimum length ≥ 12 at write time): SATISFIED.
    Evidence: password minimum 15 characters enforced at write time.
  V6.5.5 (MFA for privileged access): SATISFIED.
    Evidence: MFA required for all admin operations.
  V6.2.3 (Credential storage: one-way adaptive hash): SATISFIED.
    Evidence: bcrypt cost 12 — adaptive, one-way, resistant to brute-force.
  V6.5.2 (Account lockout or throttling after repeated failures): SATISFIED.
    Evidence: lockout after 10 consecutive failures.
Overall T-auth finding: PASS — all evaluated V6 requirements satisfied by submitted evidence.
No gaps identified in authentication controls.

---

### Pair CB4-3.4-047 — T-oauth-oidc processing: plain method violates ASVS 10.4.6 (PROCESS)
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: D

A: Territory T-oauth-oidc received for processing. Signals: OAuth 2.0 authorization
code flow implemented, PKCE active, code_challenge present in authorization requests,
code_challenge_method='plain', authorization server accepts both S256 and plain.
Team claims ASVS 10.4.6 satisfied. Evaluate against ASVS Chapter V10 requirements.

B: DOMAIN_PROCESSOR_RESULT: T-oauth-oidc.
Requirement evaluations:
  V10.4.6 (PKCE code_challenge_method must be S256): NOT SATISFIED.
    Evidence presented: code_challenge_method='plain'.
    Finding: The plain method transmits the unhashed PKCE code verifier as the
    code_challenge. This defeats the protection PKCE provides against authorization
    code interception — an interceptor who captures the code_challenge can directly
    derive the verifier. RFC 7636 §4.2 specifies that S256 transforms the verifier
    via SHA-256 before transmission; plain is permitted only when S256 is not technically
    feasible (RFC 7636 §4.2 SHOULD). ASVS 10.4.6 requires S256 without the plain
    exception.
    The authorization server accepting plain does not satisfy V10.4.6 — the server
    must be configured to reject plain per RFC 7636 §4.2 server enforcement.
  V10.4.1 (Authorization code flow implemented): SATISFIED.
    Evidence: authorization code flow confirmed.
Overall T-oauth-oidc finding: REPROCESS — ASVS_10_4_6_PLAIN_METHOD_REJECTED.
V10.4.6 is not satisfied; remediation required before re-evaluation.

Reason: The team's claim that plain satisfies V10.4.6 is incorrect. The domain processor
evaluates the actual method value against the requirement — it does not accept the team
assertion. The plain/S256 distinction is the entire security value of PKCE; accepting
plain renders the PKCE protection inoperative.

---

### Pair CB4-3.4-047 — Phrasing B
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: D

A: T-oauth-oidc: OAuth 2.0 + PKCE, code_challenge_method='plain', server accepts both
methods, ASVS 10.4.6 claimed. Evaluate V10 requirements.

B: DOMAIN_PROCESSOR_RESULT: T-oauth-oidc.
Requirement evaluations:
  V10.4.6 (PKCE code_challenge_method must be S256): NOT SATISFIED.
    Evidence presented: code_challenge_method='plain'.
    Finding: The plain method transmits the unhashed PKCE code verifier as the
    code_challenge. This defeats the protection PKCE provides against authorization
    code interception — an interceptor who captures the code_challenge can directly
    derive the verifier. RFC 7636 §4.2 specifies that S256 transforms the verifier
    via SHA-256 before transmission; plain is permitted only when S256 is not technically
    feasible (RFC 7636 §4.2 SHOULD). ASVS 10.4.6 requires S256 without the plain
    exception.
    The authorization server accepting plain does not satisfy V10.4.6 — the server
    must be configured to reject plain per RFC 7636 §4.2 server enforcement.
  V10.4.1 (Authorization code flow implemented): SATISFIED.
    Evidence: authorization code flow confirmed.
Overall T-oauth-oidc finding: REPROCESS — ASVS_10_4_6_PLAIN_METHOD_REJECTED.
V10.4.6 is not satisfied; remediation required before re-evaluation.

---

### Pair CB4-3.4-048 — T-cwe-authz processing: resource ownership check absent (PROCESS)
Source: CWE Top 25 2023
License: Public domain (MITRE)
Stratum: D

A: Territory T-cwe-authz received for processing. Signals: endpoint GET
/api/v1/users/{userId}/profile, JWT bearer required, any authenticated user can call
endpoint with any userId value, no ownership check described. CWE-862 candidate flagged
at Node 1. Evaluate access control for CWE weakness.

B: DOMAIN_PROCESSOR_RESULT: T-cwe-authz.
CWE evaluation:
  CWE-862 (Missing Authorization): CONFIRMED.
    Endpoint: GET /api/v1/users/{userId}/profile.
    JWT validation present: identity is established (authentication is present).
    Resource ownership check: ABSENT. The JWT sub claim is not compared to the userId
    path parameter. Any authenticated user supplying a valid JWT can retrieve the profile
    of any userId — the user's identity does not gate access to the specific resource.
    Pattern: Insecure Direct Object Reference (IDOR) — the userId parameter directly
    addresses a user record without per-caller authorization.
  CWE-269 (Improper Privilege Management): NOT APPLICABLE.
    An authorization check must be present to have wrong scope. Here the check is absent.
Overall T-cwe-authz finding: REPROCESS — CWE_862_IDOR_CONFIRMED.
The resource ownership check must be implemented before re-evaluation.

Reason: JWT authentication confirms identity; it does not authorize per-resource access.
CWE-862 applies when the authorization check is entirely absent. CWE-269 is not applicable
here because there is nothing to have wrong scope — there is no check at all.

---

### Pair CB4-3.4-048 — Phrasing B
Source: CWE Top 25 2023
License: Public domain (MITRE)
Stratum: D

A: T-cwe-authz: GET /api/v1/users/{userId}/profile. JWT required, any authenticated
user unrestricted. No ownership check described. CWE-862 candidate. Evaluate.

B: DOMAIN_PROCESSOR_RESULT: T-cwe-authz.
CWE evaluation:
  CWE-862 (Missing Authorization): CONFIRMED.
    Endpoint: GET /api/v1/users/{userId}/profile.
    JWT validation present: identity is established (authentication is present).
    Resource ownership check: ABSENT. The JWT sub claim is not compared to the userId
    path parameter. Any authenticated user supplying a valid JWT can retrieve the profile
    of any userId — the user's identity does not gate access to the specific resource.
    Pattern: Insecure Direct Object Reference (IDOR) — the userId parameter directly
    addresses a user record without per-caller authorization.
  CWE-269 (Improper Privilege Management): NOT APPLICABLE.
    An authorization check must be present to have wrong scope. Here the check is absent.
Overall T-cwe-authz finding: REPROCESS — CWE_862_IDOR_CONFIRMED.
The resource ownership check must be implemented before re-evaluation.

---

### Pair CB4-3.4-049 — T-process-evidence processing: PW.2 post-implementation timing gap (PROCESS)
Source: NIST SP 800-218 SSDF v1.1
License: Public domain (NIST)
Stratum: D

A: Territory T-process-evidence received for processing. Signals: requirements document
TS-AUTHZ-001 (week 1), threat model TM-AUTHZ-2026 (week 2), design review DR-AUTHZ-2026-04
(week 8), implementation completed (week 6), test results TR-AUTHZ-2026-05 (week 8).
Two unresolved findings in design review: (1) token expiry missing in s2s flow, (2) no
rate limiting on token endpoint. Evaluate against SSDF PW practices.

B: DOMAIN_PROCESSOR_RESULT: T-process-evidence.
SSDF PW practice evaluations:
  PW.1 (Security requirements before design): SATISFIED.
    Evidence: TS-AUTHZ-001 produced at week 1, before design (design review at week 4
    is the earliest design artifact).
  PW.2 (Design review for security before implementation): TIMING GAP.
    Design review DR-AUTHZ-2026-04 conducted at week 8.
    Implementation completed at week 6.
    Week 8 > week 6: design review is post-implementation. PW.2 pre-condition not met.
    Additional concern: two findings in DR-AUTHZ-2026-04 are unresolved —
      Finding 1: token expiry not validated in service-to-service flow.
      Finding 2: no rate limiting on token endpoint.
  PW.8 (Testing for security): SATISFIED.
    Evidence: TR-AUTHZ-2026-05 includes Dynamic Application Security Testing (DAST)
    scan and ASVS checklist.
Overall T-process-evidence finding: CONCERN — PW.2 timing gap with unresolved findings.
PW.1 and PW.8 satisfied. PW.2 remediation required.

Reason: PW.2 requires the design review to precede implementation. Week 8 > week 6
is an objective timing violation. The two unresolved findings are noted as contributing
to the CONCERN severity — even a timely design review with unresolved findings would
require a CONCERN verdict.

---

### Pair CB4-3.4-049 — Phrasing B
Source: NIST SP 800-218 SSDF v1.1
License: Public domain (NIST)
Stratum: D

A: T-process-evidence: TS-AUTHZ-001 (wk1), TM-AUTHZ-2026 (wk2), DR-AUTHZ-2026-04 (wk8,
post-impl at wk6, 2 unresolved findings), TR-AUTHZ-2026-05 (wk8). Evaluate SSDF PW.

B: DOMAIN_PROCESSOR_RESULT: T-process-evidence.
SSDF PW practice evaluations:
  PW.1 (Security requirements before design): SATISFIED.
    Evidence: TS-AUTHZ-001 produced at week 1, before design (design review at week 4
    is the earliest design artifact).
  PW.2 (Design review for security before implementation): TIMING GAP.
    Design review DR-AUTHZ-2026-04 conducted at week 8.
    Implementation completed at week 6.
    Week 8 > week 6: design review is post-implementation. PW.2 pre-condition not met.
    Additional concern: two findings in DR-AUTHZ-2026-04 are unresolved —
      Finding 1: token expiry not validated in service-to-service flow.
      Finding 2: no rate limiting on token endpoint.
  PW.8 (Testing for security): SATISFIED.
    Evidence: TR-AUTHZ-2026-05 includes Dynamic Application Security Testing (DAST)
    scan and ASVS checklist.
Overall T-process-evidence finding: CONCERN — PW.2 timing gap with unresolved findings.
PW.1 and PW.8 satisfied. PW.2 remediation required.

---

### Pair CB4-3.4-050 — T-rfc-jwt processing: HS256 shared key trust boundary violation (PROCESS)
Source: IETF RFCs (RFC 7519, RFC 6749)
License: IETF Trust (pre-5378 works)
Stratum: D

A: Territory T-rfc-jwt received for processing. Signals: JWT access tokens, algorithm
HS256, single shared signing key distributed to 6 resource servers. RFC 7519 §8 candidate
flagged. OAuth 2.0 authorization server issues tokens. Each resource server validates
signature, exp, iss, aud. Evaluate against RFC 7519 normative requirements.

B: DOMAIN_PROCESSOR_RESULT: T-rfc-jwt.
RFC normative evaluations:
  RFC 7519 §4.1.1–§4.1.4 (iss, sub, aud, exp claims): SATISFIED.
    Evidence: iss, aud, exp validated on each resource server.
  RFC 7519 §7.2 (Validating JWT): SATISFIED for signature verification.
    Evidence: each resource server verifies signature before accepting token.
  RFC 7519 §8 (Implementation Requirements — key management, trust boundary): VIOLATED.
    HS256 (HMAC SHA-256) uses a symmetric key: the signing key and verification key
    are identical. With one key shared across 6 resource servers, any resource server
    that holds the key can produce (forge) a token that will be accepted as valid by
    all other resource servers.
    Cross-service forgery capability: CONFIRMED. This is not merely "a weaker algorithm"
    — it is a structural trust boundary failure. Each resource server is a potential
    token issuer for the entire system.
    RFC 7519 §8 does not prohibit HS256 per se, but §8 requires implementers to
    consider key management and trust boundaries. This deployment violates the trust
    boundary: the authorization server's exclusive token-issuance capability is not
    preserved.
Overall T-rfc-jwt finding: REPROCESS — RFC_7519_S8_HS256_TRUST_BOUNDARY_VIOLATED.
Asymmetric algorithm (RS256, ES256) or per-resource-server key rotation required.

Reason: The cross-service forgery capability is the primary finding — not that HS256
is forbidden, but that shared-key HS256 across multiple resource servers destroys the
trust boundary that separates the authorization server from resource servers. This is
a structural violation of JWT security architecture.

---

### Pair CB4-3.4-050 — Phrasing B
Source: IETF RFCs (RFC 7519, RFC 6749)
License: IETF Trust (pre-5378 works)
Stratum: D

A: T-rfc-jwt: JWT HS256, one shared key across 6 resource servers. RFC 7519 §8 flagged.
Each resource server validates sig/exp/iss/aud. Evaluate RFC 7519.

B: DOMAIN_PROCESSOR_RESULT: T-rfc-jwt.
RFC normative evaluations:
  RFC 7519 §4.1.1–§4.1.4 (iss, sub, aud, exp claims): SATISFIED.
    Evidence: iss, aud, exp validated on each resource server.
  RFC 7519 §7.2 (Validating JWT): SATISFIED for signature verification.
    Evidence: each resource server verifies signature before accepting token.
  RFC 7519 §8 (Implementation Requirements — key management, trust boundary): VIOLATED.
    HS256 (HMAC SHA-256) uses a symmetric key: the signing key and verification key
    are identical. With one key shared across 6 resource servers, any resource server
    that holds the key can produce (forge) a token that will be accepted as valid by
    all other resource servers.
    Cross-service forgery capability: CONFIRMED. This is not merely "a weaker algorithm"
    — it is a structural trust boundary failure. Each resource server is a potential
    token issuer for the entire system.
    RFC 7519 §8 does not prohibit HS256 per se, but §8 requires implementers to
    consider key management and trust boundaries. This deployment violates the trust
    boundary: the authorization server's exclusive token-issuance capability is not
    preserved.
Overall T-rfc-jwt finding: REPROCESS — RFC_7519_S8_HS256_TRUST_BOUNDARY_VIOLATED.
Asymmetric algorithm (RS256, ES256) or per-resource-server key rotation required.

---

## Node 4 summary

Pairs produced: CB4-3.4-046 through CB4-3.4-050 (5 base × 2 phrasings = 10 instances)

Processing rules trained:
1. V6 auth — evaluates each requirement individually; PASS requires positive evidence
2. V10 OAuth — plain method ≠ S256; team assertion does not override requirement
3. CWE-862 — JWT auth establishes identity, does not authorize per-resource access
4. SSDF PW.2 — timing violation is objective (date comparison); unresolved findings add severity
5. RFC 7519 §8 — HS256 shared key = cross-service forgery capability (structural, not algorithmic)

Running sequence total after this file: CB4-3.4-001 through CB4-3.4-050 (100 instances)
