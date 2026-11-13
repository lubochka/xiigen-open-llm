# CB4 Stage 3 — Node Cross-Source Set Extension 14
# Sequences CB4-3.4-559 through CB4-3.4-568
# Stratum: B/C/D — multi-source combinations for ext21 patterns
# Instances: 20 (10 base pairs × 2 phrasings)
# Running total after this file: 1136 instances

---

## Pair[CB4-3.4-559]

**A:** mTLS correct plus JWT in production logs. Application: mTLS client authentication with cnf/x5t#S256 certificate binding. Separately: access tokens (JWTs) logged at DEBUG level in production, including the full token string in the `Authorization` header. Team: "Certificate binding makes our tokens sender-constrained — they can't be used by anyone without the certificate."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-JWT

T-oauth-oidc:
  mTLS + cnf/x5t#S256: correctly implemented ✓ — PASS

T-rfc-jwt:
  JWT at DEBUG level in production with full Authorization header ✗
  Production logs contain live, valid JWT strings ✗
    Log access = token theft vector ✗
    RFC 7519 / ASVS V9: tokens must not appear in logs ✗
    → REPROCESS (active security violation)

Team argument: "certificate binding = tokens can't be misused"
  mTLS certificates bind tokens to the client ✓ — correct for normal API calls
  However: JWT content exposed in logs is a separate issue:
    (1) The JWT can be read from logs (PII in payload if present; claim enumeration)
    (2) For certificate-bound tokens: attacker needs both the token AND the certificate
        Certificate is not in the log — so direct replay from log alone may be limited
    But: JWT logging REPROCESS applies regardless of binding strength:
    (a) Log contents may contain PII claims ✗
    (b) Token rotation / revocation can be bypassed if old valid tokens are in logs ✗
    → "Certificate binding compensates for JWT in logs" REJECTED — different attack surfaces

T-rfc-jwt verdict: REPROCESS
Overall verdict: REPROCESS
```

**Reason:** mTLS is correct (PASS). JWT in production logs is REPROCESS — token exposure in logs is an independent finding. Certificate binding mitigates direct replay (attacker needs the cert too) but does not protect log contents from PII exposure or from log-based analysis. REPROCESS overrides PASS.

**Phrasing-B:** mTLS correct (PASS). JWTs logged at DEBUG in production (REPROCESS). Team: "Certificate binding prevents misuse." Does mTLS compensate for JWT production logging?

---

## Pair[CB4-3.4-560]

**A:** Refresh rotation without family revocation plus open introspection. Token server: single-use refresh rotation, no family revocation. Token introspection endpoint: no authentication required (any caller can introspect any token). Revocation endpoint: client authentication required. Team: "Revocation is authenticated, and rotation means old tokens are invalid."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2

T-oauth-oidc:
  Refresh rotation: single-use ✓; family revocation absent ✗ → CONCERN

T-rfc-oauth2:
  Introspection endpoint: no authentication ✗
    RFC 7662 §2.1: introspection MUST require authentication ✗
    Any party can query active status, scope, subject of any token ✗
    → REPROCESS

  Revocation: client auth required ✓ — PASS (mechanism)
  Team argument: "rotation = old tokens invalid" → correct for rotation side; does not address introspection auth gap
  Team argument: "revocation is authenticated" → correct; separate from introspection finding

Multi-source analysis:
  Refresh family CONCERN + open introspection REPROCESS + revocation PASS
  Graduation: REPROCESS present → Overall REPROCESS

T-oauth-oidc verdict: CONCERN
T-rfc-oauth2 verdict: REPROCESS (introspection) / PASS (revocation mechanism)
Overall verdict: REPROCESS
```

**Reason:** Open introspection (REPROCESS) + refresh rotation without family revocation (CONCERN) + correct revocation mechanism (PASS). REPROCESS overrides CONCERN and PASS.

**Phrasing-B:** Refresh rotation no family revocation (CONCERN). Open introspection (REPROCESS). Revocation authenticated (PASS). What is the verdict?

---

## Pair[CB4-3.4-561]

**A:** UserInfo sub cross-check plus PKCE and all auth controls correct. OIDC: code-only flow, PKCE S256, nonce (value match + single-use), aud validated, PAR mandatory, UserInfo sub cross-check implemented, OIDC Core §5.3.4 satisfied. No PII in JWT payload. All other controls correct.

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-JWT + ASVS-V6-Auth

T-oauth-oidc:
  OIDC complete: code-only ✓; PKCE S256 ✓; nonce ✓; aud ✓; PAR ✓; UserInfo sub cross-check ✓
  → PASS

T-rfc-jwt:
  No PII in JWT ✓; JWT signing RS256 ✓; aud validated ✓ — PASS

All territories: PASS
Overall verdict: PASS
```

**Reason:** All OIDC controls correct including UserInfo sub cross-check, plus all prior controls. Multi-source PASS.

**Phrasing-B:** Complete OIDC: code-only, PKCE S256, nonce, aud, PAR, UserInfo sub cross-check. No PII in JWT. Does this comprehensive OIDC implementation produce a PASS?

---

## Pair[CB4-3.4-562]

**A:** No training plus CWE-269 privilege escalation. Organization has no formal security training. In production: privilege escalation vulnerability where a regular user can grant themselves the "system_admin" role by calling `PUT /users/{id}/roles` without authorization verification on the server side. Developer who wrote the endpoint was not aware that client-side role checks are insufficient. Team: "We have senior developers — they know security."

**B:**
```
Arbiter: SSDF-PW (PO.1) + CWE-Authz

T-ssdf-process:
  PO.1 training: absent ✗
  Incident linkage: CWE-269 privilege escalation in production traceable to knowledge gap
    (developer unaware that client-side checks are insufficient) ✗
  → REPROCESS (same escalation as ext15 pipeline: absent training + production vulnerability
    traceable to knowledge gap)

T-cwe-authz:
  CWE-269 (improper privilege management): PUT /users/{id}/roles without server-side auth ✗
  Regular user can grant themselves admin role ✗
  → REPROCESS

Multi-source analysis:
  Training REPROCESS + CWE-269 REPROCESS → REPROCESS

T-ssdf-process verdict: REPROCESS
T-cwe-authz verdict: REPROCESS
Overall verdict: REPROCESS
```

**Reason:** No training escalated to REPROCESS (production CWE-269 traceable to knowledge gap). CWE-269 is independently REPROCESS. Two REPROCESSes. Team argument rejected — "senior developers know security" is contradicted by the production privilege escalation.

**Phrasing-B:** No formal security training + CWE-269 privilege escalation in production traceable to developer knowledge gap. What is the multi-source verdict?

---

## Pair[CB4-3.4-563]

**A:** Refresh family revocation plus Vault correct. Single-use refresh rotation with family revocation on replay detection. All secrets (refresh token keys, signing keys, DB) stored in Vault with audit logging. No other gaps. Team: "Token security and credential management are both correct."

**B:**
```
Arbiter: ASVS-V10-OAuth + SSDF-PW

T-oauth-oidc:
  Refresh rotation: single-use ✓; family revocation on replay ✓ (BCP §4.14.1) — PASS

T-ssdf-process:
  Vault: all secrets ✓; audit logging ✓ — PASS

All territories: PASS
Team characterization: correct (token security + credential management both correct)
Overall verdict: PASS
```

**Reason:** Refresh rotation with family revocation (PASS) plus Vault with audit logging (PASS). Multi-source PASS.

**Phrasing-B:** Refresh rotation with family revocation. Vault for all secrets. Does this produce a multi-source PASS?

---

## Pair[CB4-3.4-564]

**A:** mTLS optional (not required) for M2M. Authorization server: token endpoint supports both mTLS and `client_secret_post` authentication. M2M services use `client_secret_post` — client secrets are 12-character alphanumeric strings (~71 bits entropy). mTLS is available but not required. Team: "mTLS is available for services that want stronger security; 71-bit secrets are adequate."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2

T-oauth-oidc:
  client_secret_post with 12-char alphanumeric: 71 bits entropy
    Below 128-bit recommended for machine secrets ✗
    → CONCERN (not negligible; below machine-secret threshold)
  mTLS available but not required: ✓ (availability alone is PASS for mTLS)
    mTLS not required → clients default to weaker authentication method ✗
    Weaker method with sub-threshold entropy → CONCERN

T-rfc-oauth2:
  RFC 6749 §2.3.1: client credentials valid method ✓
  Entropy gap: CONCERN (same analysis as 8-char case in ext15, but 71-bit is better)

Multi-source analysis:
  Secret entropy CONCERN + mTLS availability PASS
  Graduation: 0 REPROCESS, 1 CONCERN → CONCERN

T-oauth-oidc verdict: CONCERN
Overall verdict: CONCERN
```

**Reason:** Client secrets at 71 bits (12-char alphanumeric) — below the 128-bit recommended threshold for machine credentials. mTLS available but not required, so clients use weaker authentication. CONCERN overall. "71-bit is adequate" argument is contestable against 128-bit recommendation.

**Phrasing-B:** M2M client secrets 12-char alphanumeric (71 bits). mTLS available but not required. Team: "71 bits is adequate." Is this acceptable?

---

## Pair[CB4-3.4-565]

**A:** UserInfo sub cross-check absent plus token endpoint HTTP. OIDC login: UserInfo response used without sub cross-check. Token endpoint: HTTP accepted (no TLS required by the endpoint itself — TLS is handled at the load balancer). Team: "The load balancer handles TLS — traffic from load balancer to token endpoint is on internal network."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2

T-oauth-oidc:
  UserInfo sub cross-check absent ✗ → CONCERN

T-rfc-oauth2:
  Token endpoint HTTP: load balancer handles TLS
    RFC 6749 §3.2: token endpoint MUST use TLS ✗
    "Load balancer handles TLS" = perimeter TLS argument → REJECTED
    (Same pattern as ext16 pair CB4-3.4-415: API gateway TLS ≠ endpoint TLS)
    RFC 6749 §3.2 MUST applies to the endpoint; load balancer offloading is not compliant
    → REPROCESS

Multi-source analysis:
  UserInfo sub-check CONCERN + token endpoint HTTP REPROCESS
  Team argument for token endpoint: REJECTED (perimeter TLS pattern)
  Graduation: REPROCESS present → Overall REPROCESS

T-oauth-oidc verdict: CONCERN (UserInfo) / REPROCESS (token endpoint contributes)
T-rfc-oauth2 verdict: REPROCESS
Overall verdict: REPROCESS
```

**Reason:** UserInfo sub cross-check absent (CONCERN) + token endpoint HTTP with load balancer TLS (REPROCESS — perimeter TLS argument rejected, same as prior ext16 pattern). REPROCESS overrides CONCERN.

**Phrasing-B:** UserInfo sub cross-check absent (CONCERN). Token endpoint uses HTTP — TLS at load balancer (REPROCESS). What is the verdict?

---

## Pair[CB4-3.4-566]

**A:** Complete ext21 multi-source: mTLS, UserInfo, training, family revocation, 128-bit client secrets, plus 2048-bit signing key and quarterly rotation. No gaps identified.

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-JWT + RFC-OAuth2 + SSDF-PW + ASVS-V6-Auth

T-oauth-oidc:
  mTLS: tls_client_auth + cnf/x5t#S256 ✓ — PASS
  UserInfo: sub cross-check + JWT sig ✓ — PASS
  Refresh: single-use + family revocation ✓ — PASS
  client_credentials: 128-bit CSPRNG secrets ✓ — PASS

T-rfc-jwt:
  RS256 2048-bit signing key ✓; quarterly rotation ✓ — PASS
  aud validated ✓; all claims ✓ — PASS

T-ssdf-process:
  PO.1 training: annual role-based ✓ — PASS; Vault ✓ — PASS

T-asvs-authentication: Argon2id ✓ — PASS

All prior ext controls: PKCE S256 ✓; code-only ✓; PAR ✓; nonce ✓; state 256-bit ✓;
  no-PII JWT ✓; httpOnly ✓; DPoP ✓; protected dynamic reg ✓;
  TTL+pub/sub introspection ✓; minimum-scope M2M ✓ — all PASS

All territories: PASS
Overall verdict: PASS
```

**Reason:** Complete ext21 patterns plus 2048-bit signing with quarterly rotation, all prior controls. Every territory PASS. Comprehensive multi-source PASS.

**Phrasing-B:** All ext21 patterns correct plus 2048-bit RS256 quarterly rotation, all prior controls. Does this comprehensive multi-source configuration produce a PASS?

---

## Pair[CB4-3.4-567]

**A:** Refresh rotation family revocation REPROCESS escalation scenario. Application: refresh token rotation implemented. On replay detection: server logs the event. Team deployed monitoring to alert on replay events. No family revocation. Additionally: access tokens never expire (`exp` claim set to year 2099). Team: "We monitor replays and long-lived access tokens reduce round trips."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-JWT

T-oauth-oidc:
  Refresh rotation: single-use ✓; family revocation absent ✗ → CONCERN
  Monitoring replays: good operational practice ✓ (does not satisfy BCP §4.14.1 revocation)

T-rfc-jwt:
  Access tokens exp=2099 (effectively non-expiring) ✗
    RFC 7519 §4.1.4: exp claim establishes token validity; non-expiring tokens ✗
    ASVS V9: access tokens must have bounded lifetime ✗
    If access token is stolen: valid until 2099 — no natural expiry protection ✗
    → REPROCESS (effectively non-expiring access tokens)

Team argument: "long-lived tokens reduce round trips"
  Performance optimization ≠ security requirement
  Short-lived tokens are required; refresh tokens handle re-issuance without re-authentication
  → REJECTED

Multi-source analysis:
  Refresh family revocation CONCERN + non-expiring access tokens REPROCESS
  Graduation: REPROCESS present → Overall REPROCESS

T-oauth-oidc verdict: CONCERN (refresh)
T-rfc-jwt verdict: REPROCESS (exp=2099)
Overall verdict: REPROCESS
```

**Reason:** Refresh rotation without family revocation (CONCERN) + effectively non-expiring access tokens (REPROCESS — `exp=2099` violates bounded lifetime requirement). REPROCESS overrides CONCERN. "Long-lived tokens reduce round trips" is rejected — performance cannot override the bounded-lifetime requirement; refresh tokens exist to handle re-issuance.

**Phrasing-B:** Refresh rotation no family revocation (CONCERN). Access tokens with exp=2099 (non-expiring). Team: "Long-lived tokens reduce round trips." What is the verdict?

---

## Pair[CB4-3.4-568]

**A:** Post-remediation ext21 — all gaps closed. Previous audit found: (1) UserInfo sub cross-check absent → CONCERN [remediated]. (2) Refresh rotation without family revocation → CONCERN [remediated]. (3) No formal security training → CONCERN [remediated: annual OWASP-based training program launched]. (4) Client secrets at 8-char alphanumeric → CONCERN [remediated: rotated to 128-bit CSPRNG secrets]. All prior controls already correct.

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-JWT + SSDF-PW

T-oauth-oidc:
  UserInfo sub cross-check ✓ (CONCERN remediated) — PASS
  Refresh rotation: single-use ✓; family revocation ✓ (CONCERN remediated) — PASS

T-ssdf-process:
  PO.1 annual OWASP-based training ✓ (CONCERN remediated) — PASS

T-oauth-oidc:
  client_credentials: 128-bit CSPRNG secrets ✓ (CONCERN remediated) — PASS

All prior controls: PKCE S256 ✓; code-only ✓; PAR ✓; nonce+aud ✓; state 256-bit ✓;
  DPoP ✓; mTLS available ✓; dynamic reg protected ✓; introspection TTL+pub/sub ✓;
  minimum-scope M2M ✓; Argon2id ✓; Vault ✓ — all PASS

All territories: PASS
Overall verdict: PASS
```

**Reason:** All four CONCERNs (UserInfo sub check, family revocation, training, client secret entropy) remediated with correct implementations. Post-remediation: all territories PASS. Comprehensive PASS confirms all ext21 remediation complete and correct.

**Phrasing-B:** Previous audit found 4 CONCERNs — all remediated (UserInfo sub check, family revocation, training, 128-bit secrets). Does post-remediation produce a comprehensive PASS?

---

## File Summary

| Pair ID | Description | Stratum | Sources | Verdict |
|---------|-------------|---------|---------|---------|
| CB4-3.4-559 | mTLS correct PASS + JWT prod logs REPROCESS — certificate binding doesn't cover logs | B | V10-OAuth + RFC-JWT | REPROCESS |
| CB4-3.4-560 | Refresh no family revocation CONCERN + open introspection REPROCESS | B | V10-OAuth + RFC-OAuth2 | REPROCESS |
| CB4-3.4-561 | Complete OIDC with UserInfo sub cross-check + all controls | A | V10-OAuth + RFC-JWT | PASS |
| CB4-3.4-562 | No training REPROCESS + CWE-269 REPROCESS — both escalated | B | SSDF-PW + CWE-Authz | REPROCESS |
| CB4-3.4-563 | Refresh family revocation PASS + Vault PASS | A | V10-OAuth + SSDF-PW | PASS |
| CB4-3.4-564 | mTLS optional + 71-bit M2M secrets — CONCERN | B | V10-OAuth + RFC-OAuth2 | CONCERN |
| CB4-3.4-565 | UserInfo sub-check CONCERN + token endpoint HTTP REPROCESS | B | V10-OAuth + RFC-OAuth2 | REPROCESS |
| CB4-3.4-566 | Complete ext21 + 2048-bit RS256 quarterly rotation + all prior — comprehensive PASS | A | Multiple | PASS |
| CB4-3.4-567 | Refresh no family revocation CONCERN + access tokens exp=2099 REPROCESS | B/D | V10-OAuth + RFC-JWT | REPROCESS |
| CB4-3.4-568 | Post-remediation: 4 CONCERNs all closed → comprehensive PASS | A | V10-OAuth + SSDF-PW | PASS |
