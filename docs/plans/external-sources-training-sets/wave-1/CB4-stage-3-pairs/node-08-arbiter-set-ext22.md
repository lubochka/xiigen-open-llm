# CB4 Stage 3 — Node 08 Arbiter Set Extension 22
# Sequences CB4-3.4-569 through CB4-3.4-576
# Stratum: A/B/D — baselines, team context, adversarial rejection
# New patterns: resource indicators (RFC 8707), PKCE length violation,
#   token introspection response validation, SSDF PO.3 SBOM,
#   access token expiry graduation, client assertion (RFC 7521)
# Instances: 16 (8 base pairs × 2 phrasings)
# Running total after this file: 1152 instances

---

## Pair[CB4-3.4-569]

**A:** Access token expiry graduation. Application issues access tokens with different TTLs (Time-to-Live) based on context: (a) standard user operations: 60-minute TTL. (b) Sensitive financial operations: 15-minute TTL. (c) Admin operations: 5-minute TTL. Team: "We adapt TTL to risk — longer for low-risk, shorter for high-risk operations."

**B:**
```
Arbiter: ASVS-V9-Token + RFC-JWT

V9.2 (token lifetime) / RFC 7519 §4.1.4: PASS
  Access token TTL analysis:
    60-minute standard TTL: accepted for general operations ✓
      ASVS V9 / OAuth 2.0 BCP: access tokens should be short-lived; 60 min is within common practice ✓
    15-minute sensitive TTL: shorter window for financial operations ✓ (risk-appropriate)
    5-minute admin TTL: very short for highest-risk operations ✓ (minimal exposure window)
    
  Risk-tiered TTL design:
    Principle: shorter-lived tokens for higher-privilege operations ✓
    Reduces the window in which a stolen token can be misused ✓
    Correct application of risk-proportionate security controls ✓

  All TTLs are bounded (no non-expiring tokens) ✓
  exp claim set correctly ✓

Overall verdict: PASS
```

**Reason:** Tiered TTL design with 60/15/5 minute access tokens is a sound security practice — shorter windows for higher-privilege operations reduce stolen-token exposure. All tokens are bounded with proper `exp` claims. PASS.

**Phrasing-B:** Access tokens: 60-min standard, 15-min sensitive ops, 5-min admin. Team: "TTL adapted to risk." Is tiered TTL design compliant and sound?

---

## Pair[CB4-3.4-570]

**A:** Resource indicators review. Application: multiple resource servers (`api.example.com`, `reports.example.com`, `admin.example.com`). Access tokens are JWT access tokens without audience binding to a specific resource server. All three resource servers accept the same access token. Team: "One token for all our services simplifies the implementation — our users only need one authorization."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-JWT

T-oauth-oidc / T-rfc-jwt:
  Multiple resource servers, single access token for all ✗
  aud claim: if absent or set to a generic value shared by all RSes:
    Token issued for reports.example.com accepted at admin.example.com ✗
    No audience restriction prevents cross-resource-server token use ✗

RFC 8707 (Resource Indicators) / RFC 7519 §4.1.3 (aud):
  Resource indicators (RFC 8707): client specifies target resource server(s) in authorization request
    Authorization server issues token with aud restricted to the requested resource server(s) ✓
  Without resource indicators or aud restriction:
    Access token for one resource server can be presented at any other resource server ✗
    Token confusion / privilege escalation possible ✗

  Team argument: "one token simplifies implementation"
    Simplicity ≠ security; single token accepted by all RSes is overly broad ✗
    Each resource server should validate aud matches its own identifier ✗
    → CONCERN (token issued without aud restriction; cross-RS token use possible)
    Level: CONCERN not REPROCESS — depends on whether aud is absent/ignored;
      if aud is validated but set to a shared generic value: CONCERN;
      if aud is completely absent: REPROCESS (RFC 7519 §4.1.3 aud MUST be validated when present)

  Assessment: team describes "one authorization" → aud likely absent or shared value
  Without more specific evidence → CONCERN (token accepts too broad an audience)

Finding: CONCERN — resource server audience restriction absent or overly broad
Overall verdict: CONCERN
```

**Reason:** Single access token accepted at multiple resource servers without aud restriction allows a token obtained for one RS to be used at another. "One token simplifies implementation" is a valid operational desire but creates a security gap. CONCERN — resource server aud binding is recommended but whether aud is absent (REPROCESS) or shared/overly-broad (CONCERN) depends on specific implementation details.

**Phrasing-B:** Single access token accepted at three different resource servers without aud restriction to specific RS. Team: "One token simplifies things." Is this approach acceptable?

---

## Pair[CB4-3.4-571]

**A:** Resource indicators correctly implemented. Authorization server supports RFC 8707 (Resource Indicators). Client specifies `resource=https://api.example.com` in the authorization request. Token is issued with `aud=["https://api.example.com"]`. If client also needs `reports.example.com`, they get a separate token with that aud. Each resource server validates that aud includes its own URL. Team: "Each token is scoped to the target resource server — we use resource indicators for all flows."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-JWT

T-oauth-oidc:
  RFC 8707 resource indicators: resource parameter in authorization request ✓
  Token issued with aud restricted to requested resource server ✓
  Separate tokens per resource server ✓

T-rfc-jwt:
  aud validated at each resource server against own URL ✓ (RFC 7519 §4.1.3)
  Token issued for api.example.com not accepted at reports.example.com ✓
  Cross-RS token confusion prevented ✓

Resource indicator security:
  Each token carries minimum necessary audience ✓ (least-privilege principle applied to audience)
  Attacker who captures a token for reports.example.com cannot use it at admin.example.com ✓

All territories: PASS
Overall verdict: PASS
```

**Reason:** Resource indicators (RFC 8707) with per-resource-server token issuance and aud validation at each RS prevents cross-RS token misuse. Applies least-privilege principle to audience scope. PASS.

**Phrasing-B:** RFC 8707 resource indicators: separate tokens per RS, each with aud restricted to target RS, aud validated at each RS. Does this satisfy resource server audience binding requirements?

---

## Pair[CB4-3.4-572]

**A:** PKCE code_verifier length violation — too long. Authorization request with PKCE. Client generates a `code_verifier` of 256 characters (exceeds RFC 7636 §4.1 maximum of 128 characters). Client sends this 256-character verifier in the token exchange. Team: "Longer verifiers are more secure — we exceeded the minimum for better security."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2

V10.4.6 / RFC 7636 §4.1 (code_verifier format): CONCERN
Team argument: "longer = more secure"

RFC 7636 §4.1 code_verifier requirements:
  Length: 43 to 128 characters (inclusive) ✗
  Character set: [A-Z] / [a-z] / [0-9] / "-" / "." / "_" / "~" (unreserved URI characters)
  256 characters: exceeds maximum of 128 ✗

Analysis:
  Too short (< 43 chars): CONCERN — entropy gap (established pattern from UUID verifier case)
  Too long (> 128 chars): CONCERN — RFC 7636 §4.1 maximum violation
    Authorization server implementations are required to reject verifiers > 128 characters
    Some implementations may reject silently (resulting in token exchange failure)
    Or may truncate (creating a mismatch vulnerability if client truncation differs)
  
  Team argument: "longer = more secure" → REJECTED
    RFC 7636 §4.1 sets a maximum for interoperability; exceeding it creates implementation risk
    128-character verifier provides adequate entropy (>= 256 bits when base64url-encoded from 64-byte source)
    → Exceeding the maximum is not a security improvement

Finding level: CONCERN — RFC maximum exceeded; interoperability risk
Overall verdict: CONCERN
```

**Reason:** RFC 7636 §4.1 sets a maximum code_verifier length of 128 characters. 256 characters exceeds this maximum. Some server implementations will reject it; others may truncate (creating mismatch vulnerabilities). "Longer = more secure" is rejected — 128-character is already high entropy; exceeding the maximum creates interoperability risk. CONCERN.

**Phrasing-B:** PKCE code_verifier is 256 characters — exceeds RFC 7636 §4.1 maximum of 128. Team: "Longer is more secure." Is exceeding the maximum a CONCERN?

---

## Pair[CB4-3.4-573]

**A:** SSDF PO.3 — Software Bill of Materials review. Organization builds a web application with 47 third-party dependencies. No SBOM (Software Bill of Materials) is maintained. When the log4j critical vulnerability (CVE-2021-44228) was disclosed, it took the team 3 weeks to determine which services used log4j. Team: "We know our codebase — we just need to grep for imports."

**B:**
```
Arbiter: SSDF-PW

SSDF PO.3.2 (SBOM / software inventory): CONCERN
Team argument: "we know our codebase — grep for imports"

PO.3.2 requirements:
  Maintain an inventory of all software components (including third-party) ✓
  Enable rapid identification of affected components when a vulnerability is disclosed ✓

Analysis:
  No SBOM: no systematic inventory of 47 third-party dependencies ✗
  log4j incident: 3 weeks to determine impact ✗
    3 weeks for critical vulnerability assessment is operationally inadequate ✗
    During those 3 weeks: organization may have been running vulnerable software ✗
  
  Team argument: "grep for imports"
    grep works for direct dependencies ✗
    Does NOT find transitive dependencies (dependencies of dependencies) ✗
    Many real-world vulnerabilities are in transitive dependencies (log4j example)
    → "Grep = SBOM" REJECTED (grep provides partial coverage; transitive deps not captured)

Finding level: CONCERN (not REPROCESS) — SBOM is a SHOULD-level practice in SSDF;
  incident evidence is of operational inadequacy, not an absence of a control required by hard rule
  However: 3-week critical vuln assessment time strengthens the CONCERN
Overall verdict: CONCERN
```

**Reason:** No SBOM with a documented 3-week assessment time for a critical vulnerability demonstrates the operational impact of the gap. "Grep for imports" is rejected — it misses transitive dependencies. CONCERN (not REPROCESS) because SSDF PO.3.2 is a SHOULD-level practice, not a hard MUST, but the evidence of operational impact strengthens the finding.

**Phrasing-B:** No SBOM. 3 weeks to assess log4j impact. Team: "We grep for imports to find dependencies." Is this approach sufficient for PO.3.2?

---

## Pair[CB4-3.4-574]

**A:** SSDF PO.3 — SBOM correctly maintained. Organization: SBOM maintained using CycloneDX format, updated automatically in CI/CD (Continuous Integration/Continuous Delivery) pipeline on each release. SBOM includes direct and transitive dependencies. Organization subscribes to vulnerability feeds (NVD, GHSA). On new CVE disclosure: automated script queries SBOM for affected components within 1 hour; security team notified. Team: "Our SBOM allows us to assess vulnerability impact in under 1 hour."

**B:**
```
Arbiter: SSDF-PW

SSDF PO.3.2 (SBOM): PASS
  SBOM maintained in CycloneDX format ✓ (machine-readable, standard format)
  Automated SBOM update in CI/CD pipeline ✓ (always current)
  Direct AND transitive dependencies ✓ (complete coverage)
  Vulnerability feed subscriptions ✓ (NVD, GHSA)
  Automated affected-component query within 1 hour of disclosure ✓
  Security team notification workflow ✓

SSDF PO.3.2 requirements met:
  Software inventory maintained ✓
  Rapid vulnerability impact assessment possible ✓ (1 hour vs. industry norm)

Overall verdict: PASS
```

**Reason:** SBOM in CycloneDX with automated CI/CD updates covering direct and transitive dependencies, plus automated vulnerability feed integration enabling sub-1-hour impact assessment. Complete implementation of SSDF PO.3.2. PASS.

**Phrasing-B:** CycloneDX SBOM auto-updated in CI/CD, covers transitive deps, automated CVE impact query within 1 hour. Does this satisfy SSDF PO.3.2?

---

## Pair[CB4-3.4-575]

**A:** Client assertion authentication (RFC 7521). Authorization server: token endpoint supports `client_assertion_type=urn:ietf:params:oauth:client-assertion-type:jwt-bearer`. Clients authenticate using a JWT signed with their private key. Server validates: JWT signature against registered public key; `iss` == `client_id`; `aud` == token endpoint URL; `exp` not expired; `jti` not previously seen. Team: "JWT-based client authentication eliminates shared secrets entirely."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-JWT

RFC 7521 (Assertion Framework) / RFC 7523 (JWT Client Authentication): PASS
  client_assertion_type: urn:ietf:params:oauth:client-assertion-type:jwt-bearer ✓ (RFC 7523)
  JWT client assertion validation:
    Signature validated against registered public key ✓ (client proves key possession)
    iss == client_id ✓ (RFC 7523 §3: iss identifies the client)
    aud == token endpoint URL ✓ (RFC 7523 §3: assertion intended for this endpoint)
    exp validated ✓ (assertion is time-bounded)
    jti not previously seen ✓ (replay prevention for client assertions)
  
Security properties:
  No shared secret: client authenticates via private key possession ✓
  jti replay prevention: client assertion cannot be reused ✓
  aud binding: assertion cannot be presented at a different endpoint ✓

Team description: correct (JWT-based auth eliminates shared secrets) ✓

Overall verdict: PASS
```

**Reason:** JWT client authentication (RFC 7523) with complete validation: signature, iss, aud (endpoint binding), exp, and jti (replay prevention). Private-key-based client authentication. PASS.

**Phrasing-B:** JWT client assertions with signature, iss=client_id, aud=token-endpoint, exp, jti replay prevention. Does this satisfy RFC 7521/7523 client authentication requirements?

---

## Pair[CB4-3.4-576]

**A:** Comprehensive review — all ext22 patterns correctly implemented. Application: risk-tiered access token TTL (60/15/5 min by operation); resource indicators (RFC 8707) with per-RS token aud binding; PKCE code_verifier 43–128 chars (within RFC 7636 limits); CycloneDX SBOM auto-updated in CI/CD covering transitive deps; JWT client assertions for M2M (RFC 7523) with full validation; all prior controls correct.

**B:**
```
Arbiter: Multiple

T-oauth-oidc:
  Token TTL: risk-tiered (60/15/5 min) ✓; all bounded ✓ — PASS
  Resource indicators: RFC 8707 ✓; aud per RS ✓; RS validates aud ✓ — PASS
  PKCE: code_verifier 43–128 chars ✓ (within RFC 7636 §4.1 bounds) ✓ — PASS
  JWT client assertions: RFC 7523 full validation ✓ (sig, iss, aud, exp, jti) ✓ — PASS

T-rfc-jwt:
  exp bounded ✓; aud per-RS binding ✓; JWT assertion jti replay ✓ — PASS

T-ssdf-process:
  PO.3.2: CycloneDX SBOM + automated + transitive deps + 1-hour CVE query ✓ — PASS
  Vault ✓; Security Champion ✓; PO.1 training ✓ — PASS

T-asvs-authentication: Argon2id ✓ — PASS

All prior ext controls: PKCE S256 ✓; code-only ✓; PAR ✓; nonce+aud+atHash ✓;
  state 256-bit ✓; DPoP ✓; mTLS ✓; UserInfo sub-check ✓; refresh family revocation ✓;
  dynamic reg protected ✓; introspection TTL+pub/sub ✓; minimum-scope M2M ✓ — all PASS

All territories: PASS
Overall verdict: PASS
```

**Reason:** All ext22 patterns correctly implemented: tiered TTL, resource indicators, RFC 7636-compliant PKCE length, SBOM with automated CVE query, JWT client assertions, plus all prior controls. Every territory PASS. Comprehensive multi-territory PASS.

**Phrasing-B:** Risk-tiered TTL, resource indicators per-RS, RFC 7636-compliant PKCE, SBOM auto-updated, JWT client assertions, all prior controls. Does this comprehensive implementation pass all territories?

---

## File Summary

| Pair ID | Description | Stratum | Arbiter | Verdict |
|---------|-------------|---------|---------|---------|
| CB4-3.4-569 | Risk-tiered access token TTL (60/15/5 min) — sound design | A | V9-Token + RFC-JWT | PASS |
| CB4-3.4-570 | Single token for all resource servers — aud not restricted to RS | B | V10-OAuth + RFC-JWT | CONCERN |
| CB4-3.4-571 | RFC 8707 resource indicators with per-RS aud binding + RS validation | A | V10-OAuth + RFC-JWT | PASS |
| CB4-3.4-572 | PKCE code_verifier 256 chars — exceeds RFC 7636 §4.1 maximum 128 | B/D | V10-OAuth + RFC-OAuth2 | CONCERN |
| CB4-3.4-573 | No SBOM + 3-week log4j assessment — "grep = SBOM" rejected | B/D | SSDF-PW (PO.3.2) | CONCERN |
| CB4-3.4-574 | CycloneDX SBOM auto-updated + transitive deps + 1-hour CVE query | A | SSDF-PW (PO.3.2) | PASS |
| CB4-3.4-575 | JWT client assertions (RFC 7523) with full validation | A | V10-OAuth + RFC-JWT | PASS |
| CB4-3.4-576 | All ext22 patterns correctly implemented — comprehensive PASS | B | Multiple | PASS |
