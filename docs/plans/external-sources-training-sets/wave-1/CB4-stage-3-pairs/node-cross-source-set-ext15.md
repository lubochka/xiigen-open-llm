# CB4 Stage 3 — Node Cross-Source Set Extension 15
# Sequences CB4-3.4-585 through CB4-3.4-594
# Stratum: B/C/D — multi-source combinations for ext22 patterns
# Instances: 20 (10 base pairs × 2 phrasings)
# Running total after this file: 1188 instances

---

## Pair[CB4-3.4-585]

**A:** Broad aud token plus no session invalidation on logout. Single access token accepted at all resource servers (aud covers all RSes). Application: on user logout, the access token is not revoked — only the client-side cookie is cleared. Access token TTL: 45 minutes. Team: "Clearing the cookie is sufficient for user logout — the token expires in 45 minutes."

**B:**
```
Arbiter: ASVS-V10-OAuth + ASVS-V7-Session

T-oauth-oidc:
  Broad aud (all RSes) ✗ → CONCERN (same as ext22 pattern)

T-asvs-session:
  Logout: cookie cleared ✓ (client-side termination)
  Access token: not revoked ✗; 45-minute residual TTL
    V7.3.1 / V10.3: token must be invalidated on logout ✗
    "Cookie cleared = session ended" → server-side token remains valid 45 minutes ✗
    If token is captured or retained by another tab: still valid for 45 minutes ✗
    → REPROCESS (server-side token invalidation absent on logout)

Multi-source analysis:
  Broad aud CONCERN + logout no revocation REPROCESS
  Graduation: REPROCESS present → Overall REPROCESS

T-oauth-oidc verdict: CONCERN
T-asvs-session verdict: REPROCESS
Overall verdict: REPROCESS
```

**Reason:** Broad aud is CONCERN. No token revocation on logout with 45-minute TTL is REPROCESS (server-side token invalidation required). REPROCESS overrides CONCERN.

**Phrasing-B:** Broad aud token (CONCERN). Cookie cleared on logout but access token not revoked, 45-min TTL (REPROCESS). What is the verdict?

---

## Pair[CB4-3.4-586]

**A:** JWT client assertion jti reuse plus open introspection. Token endpoint: JWT client assertions without jti uniqueness checking. Token introspection endpoint: no authentication required. Revocation: client auth required. Team: "JWT assertion signature proves authenticity; introspection on internal network is safe."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2

T-oauth-oidc:
  JWT client assertion jti not tracked ✗ → REPROCESS (RFC 7523 §3)
  "Signature proves authenticity" → rejected: signature ≠ replay prevention ✗

T-rfc-oauth2:
  Introspection endpoint: no auth ✗ → REPROCESS (RFC 7662 §2.1)
  "Internal network = safe" → rejected: perimeter ≠ application-layer auth ✗

Multi-source analysis:
  JWT assertion REPROCESS + introspection REPROCESS
  Two REPROCESSes → REPROCESS

T-oauth-oidc verdict: REPROCESS
T-rfc-oauth2 verdict: REPROCESS
Overall verdict: REPROCESS
```

**Reason:** Two independent REPROCESS findings: JWT client assertion jti not tracked (replay possible) and open introspection (RFC 7662 §2.1 violated). Both team arguments rejected. Overall REPROCESS.

**Phrasing-B:** JWT client assertion jti not tracked (REPROCESS). Introspection no auth (REPROCESS). Two independent REPROCESSes — what is the verdict?

---

## Pair[CB4-3.4-587]

**A:** Resource indicators correct plus 1024-bit signing key. RFC 8707 resource indicators used — per-RS aud binding. JWT access tokens signed with RSA 1024-bit key. Per-RS scope minimization. Team: "Per-RS audience binding is our key security measure — we're doing resource indicators correctly."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-JWT

T-oauth-oidc:
  RFC 8707 resource indicators: per-RS aud ✓; RS validates aud ✓ — PASS

T-rfc-jwt:
  RSA 1024-bit signing key ✗
  RFC 7518 §3.3: MUST ≥ 2048 bits ✗ → REPROCESS (same as ext16 pattern)
  
Multi-source analysis:
  Resource indicators PASS + 1024-bit key REPROCESS
  Correct audience binding does not protect against weak signing key — forged tokens would have
    any aud the attacker wants (including the correct RS)
  Graduation: REPROCESS → Overall REPROCESS

T-oauth-oidc verdict: PASS
T-rfc-jwt verdict: REPROCESS
Overall verdict: REPROCESS
```

**Reason:** Resource indicators correct (PASS) but 1024-bit RSA signing key (REPROCESS). A breakable signing key allows token forgery — the attacker can forge tokens with any aud value, defeating the audience binding. REPROCESS overrides PASS.

**Phrasing-B:** RFC 8707 resource indicators correct (PASS). JWT signed with RSA 1024-bit key (REPROCESS). Does correct audience binding compensate for weak signing key?

---

## Pair[CB4-3.4-588]

**A:** SBOM manual process plus Argon2id correct. Organization: no automated SBOM tooling — manually curated dependency list updated when developers remember to do so. Last update: 3 months ago. Password hashing: Argon2id with recommended parameters. Team: "Argon2id shows our commitment to security best practices."

**B:**
```
Arbiter: SSDF-PW + ASVS-V6-Auth

T-asvs-authentication:
  Argon2id recommended parameters ✓ — PASS

T-ssdf-process:
  SBOM: manually curated, last updated 3 months ago ✗
    3-month-old dependency list misses recent additions ✗
    Manual process: no enforcement guarantee ✗
    SSDF PO.3.2: requires current inventory ✗
    → CONCERN (SBOM intent present; currency and reliability inadequate)
  "Argon2id shows commitment" → password hashing and SBOM are independent requirements ✗

Multi-source analysis:
  Argon2id PASS + stale manual SBOM CONCERN
  Graduation: 0 REPROCESS, 1 CONCERN → CONCERN

T-asvs-authentication verdict: PASS
T-ssdf-process verdict: CONCERN
Overall verdict: CONCERN
```

**Reason:** Argon2id is correct (PASS). Stale manual SBOM (last updated 3 months ago) is CONCERN — SBOM intent exists but currency and reliability are inadequate for rapid vulnerability response. CONCERN overall.

**Phrasing-B:** Stale manual SBOM (3 months old). Argon2id correct. Team: "Argon2id shows our security commitment." Does password hashing correctness compensate for the SBOM gap?

---

## Pair[CB4-3.4-589]

**A:** Complete ext22 plus correct mTLS and refresh family revocation. All ext22 patterns correct: resource indicators, tiered TTL, RFC 7636-compliant PKCE, SBOM auto-updated, JWT assertions with jti. Additionally: mTLS for M2M services, refresh token rotation with family revocation, UserInfo sub cross-check. All prior controls correct.

**B:**
```
Arbiter: Multiple

T-oauth-oidc:
  Resource indicators (RFC 8707) ✓; risk-tiered TTL ✓; PKCE 43-128 ✓;
  JWT assertions with jti ✓; mTLS ✓; refresh family revocation ✓;
  UserInfo sub cross-check ✓ — PASS (all ext22 + ext21 ext20 patterns)

T-rfc-jwt: aud per RS ✓; assertions validated ✓; exp bounded ✓ — PASS

T-ssdf-process: SBOM auto-updated ✓; Vault ✓; PO.1 training ✓ — PASS

All prior controls: PKCE S256 ✓; code-only ✓; PAR ✓; nonce+aud ✓; state 256-bit ✓;
  DPoP ✓; dynamic reg protected ✓; TTL+pub/sub introspection ✓;
  minimum-scope M2M ✓; Argon2id ✓ — all PASS

All territories: PASS
Overall verdict: PASS
```

**Reason:** All ext22 patterns plus ext21 additions (mTLS, family revocation, UserInfo check) and all prior controls. Every territory PASS. Comprehensive PASS.

**Phrasing-B:** All ext22 patterns plus mTLS, family revocation, UserInfo sub check, and all prior controls. Multi-source comprehensive PASS?

---

## Pair[CB4-3.4-590]

**A:** PKCE code_verifier too long plus Vault correct. PKCE: code_verifier generated as 180-character string (exceeds RFC 7636 §4.1 maximum of 128). Secret management: all credentials in Vault with audit logging. Team: "Our key management is excellent and PKCE S256 is the strongest method."

**B:**
```
Arbiter: ASVS-V10-OAuth + SSDF-PW

T-oauth-oidc:
  PKCE S256 method ✓; code_verifier 180 chars ✗ (exceeds maximum 128) → CONCERN

T-ssdf-process:
  Vault: credentials in Vault with audit logging ✓ — PASS

Multi-source analysis:
  PKCE length CONCERN + Vault PASS
  Vault correctness does not compensate for PKCE format non-compliance
  Graduation: 0 REPROCESS, 1 CONCERN → CONCERN

T-oauth-oidc verdict: CONCERN
T-ssdf-process verdict: PASS
Overall verdict: CONCERN
```

**Reason:** PKCE S256 method is correct but verifier length 180 exceeds RFC 7636 §4.1 maximum of 128. Vault is correct (PASS). CONCERN from PKCE length gap; Vault doesn't affect this.

**Phrasing-B:** PKCE S256 method correct, verifier 180 chars (exceeds max). Vault correct. What is the verdict?

---

## Pair[CB4-3.4-591]

**A:** JWT assertion aud binding incorrect. Token endpoint: JWT client assertions. All validation steps present: signature, iss, exp, jti uniqueness. `aud` validation: client assertion `aud` claim is validated — but the check is `aud.contains("example.com")` (substring match) rather than exact equality to the token endpoint URL. Team: "We validate that our domain is in the aud — that's sufficient for endpoint binding."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-JWT

T-oauth-oidc:
  JWT client assertion: sig ✓; iss ✓; exp ✓; jti uniqueness ✓
  aud validation: substring check (contains "example.com") ✗
    RFC 7523 §3: aud MUST be the token endpoint URL (exact match)
    Substring check: any aud value containing "example.com" passes
      Attacker forges assertion with aud="https://attacker.example.com/token" ✓
        → contains "example.com" → passes the substring check ✗
      OR: assertion issued for a DIFFERENT endpoint at example.com passes ✗
    → Substring check is a bypass of the aud binding requirement
    → REPROCESS (exact match required; substring check is incorrect)

T-rfc-jwt:
  RFC 7523 §3: aud should be the specific token endpoint ✗ (substring match) → REPROCESS

Team argument: "our domain is in the aud"
  RFC 7523 requires the token endpoint URL, not a domain substring ✗ → REJECTED

Overall verdict: REPROCESS
```

**Reason:** JWT client assertion `aud` validated via substring match instead of exact equality to the token endpoint URL. Substring match allows forged assertions with `aud` values containing the domain string. RFC 7523 §3 requires exact match to the token endpoint. "Domain in aud" argument rejected. REPROCESS.

**Phrasing-B:** JWT client assertion aud validated by substring match for domain, not exact token endpoint URL. Team: "Domain in aud is sufficient." Is substring match acceptable?

---

## Pair[CB4-3.4-592]

**A:** SBOM with transitive deps plus Security Champion with structured review. SBOM: CycloneDX, automated, includes transitive dependencies, integrated with NVD feed. Security Champion: designated; uses OWASP ASVS checklist for security reviews. Annual developer training. No other gaps.

**B:**
```
Arbiter: SSDF-PW + ASVS-V10-OAuth

T-ssdf-process:
  SBOM: CycloneDX ✓; automated ✓; transitive deps ✓; NVD feed integration ✓ — PASS
  Security Champion: designated ✓; OWASP ASVS checklist ✓ — PASS (PW.3.2)
  Annual training ✓ — PASS (PO.1)

T-oauth-oidc: all controls correct ✓ (assumed prior controls) — PASS

All territories: PASS
Overall verdict: PASS
```

**Reason:** SBOM fully automated with transitive deps (PO.3.2 PASS) plus Security Champion with structured OWASP checklist (PW.3.2 PASS) plus annual training (PO.1 PASS). Multi-source PASS.

**Phrasing-B:** Automated CycloneDX SBOM with transitive deps + NVD feed. Security Champion with OWASP checklist + annual training. Multi-source PASS?

---

## Pair[CB4-3.4-593]

**A:** Three-CONCERN scenario — broad aud, SBOM manual, client secret 71-bit. Application: (1) Access tokens with aud covering all RSes. (2) SBOM maintained manually, last updated 6 weeks ago. (3) client_credentials client secrets: 12-char alphanumeric (~71 bits). All core OAuth flow controls correct. Team: "We have baseline coverage for all security areas."

**B:**
```
Arbiter: ASVS-V10-OAuth + SSDF-PW + RFC-OAuth2

T-oauth-oidc:
  Broad aud (all RSes) ✗ → CONCERN

T-ssdf-process:
  SBOM: manual, 6 weeks old ✗ → CONCERN (currency gap)

T-rfc-oauth2:
  Client secrets 71 bits ✗ (below 128-bit machine threshold) → CONCERN

Multi-source analysis:
  Three CONCERNs; no REPROCESS
  Three CONCERNs do NOT escalate to REPROCESS — graduation rule
  Team argument: "baseline coverage" → three separate gaps noted; none rise to REPROCESS

T-oauth-oidc verdict: CONCERN
T-ssdf-process verdict: CONCERN
T-rfc-oauth2 verdict: CONCERN
Overall verdict: CONCERN (three CONCERNs; graduation rule)
```

**Reason:** Three CONCERNs: broad aud, stale manual SBOM, 71-bit client secrets. All are design/configuration gaps, none are absent required controls. Three CONCERNs = CONCERN per graduation rule.

**Phrasing-B:** Broad aud CONCERN, stale manual SBOM CONCERN, 71-bit client secrets CONCERN. Three CONCERNs — does that escalate to REPROCESS?

---

## Pair[CB4-3.4-594]

**A:** Post-remediation ext22 — all gaps closed. Previous audit found: (1) Broad aud for all RSes → CONCERN [remediated: RFC 8707 resource indicators, per-RS tokens]. (2) PKCE verifier 200-char → CONCERN [remediated: 64-char CSPRNG-generated verifier]. (3) SBOM manual/stale → CONCERN [remediated: CycloneDX auto-updated in CI/CD]. (4) JWT assertion jti not tracked → REPROCESS [remediated: Redis jti tracking]. All prior controls correct.

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-JWT + SSDF-PW

T-oauth-oidc:
  Resource indicators: per-RS aud ✓ (CONCERN remediated) — PASS
  PKCE verifier: 64-char CSPRNG ✓ (in 43-128 range) (CONCERN remediated) — PASS
  JWT assertions: jti tracked in Redis ✓ (REPROCESS remediated) — PASS

T-rfc-jwt:
  JWT assertions: sig + iss + aud-exact + exp + jti-unique ✓ — PASS

T-ssdf-process:
  SBOM: CycloneDX auto-updated ✓ (CONCERN remediated) — PASS

All prior controls: all PASS
Overall verdict: PASS
```

**Reason:** All four gaps (1 REPROCESS + 3 CONCERN) remediated: resource indicators, correct PKCE length, SBOM automation, JWT assertion jti tracking. Post-remediation comprehensive PASS.

**Phrasing-B:** Previous audit: 1 REPROCESS (JWT assertion jti) and 3 CONCERNs (broad aud, PKCE length, SBOM staleness). All remediated. Does post-remediation produce a comprehensive PASS?

---

## File Summary

| Pair ID | Description | Stratum | Sources | Verdict |
|---------|-------------|---------|---------|---------|
| CB4-3.4-585 | Broad aud CONCERN + no logout revocation REPROCESS | B | V10-OAuth + V7-Session | REPROCESS |
| CB4-3.4-586 | JWT assertion jti REPROCESS + open introspection REPROCESS | B | V10-OAuth + RFC-OAuth2 | REPROCESS |
| CB4-3.4-587 | Resource indicators PASS + 1024-bit RSA REPROCESS | B | V10-OAuth + RFC-JWT | REPROCESS |
| CB4-3.4-588 | Stale manual SBOM CONCERN + Argon2id PASS | B | SSDF-PW + V6-Auth | CONCERN |
| CB4-3.4-589 | All ext22 + ext21 patterns + all prior controls — comprehensive PASS | A | Multiple | PASS |
| CB4-3.4-590 | PKCE verifier 180-char CONCERN + Vault PASS | B | V10-OAuth + SSDF-PW | CONCERN |
| CB4-3.4-591 | JWT assertion aud substring match — "domain in aud" rejected | B/D | V10-OAuth + RFC-JWT | REPROCESS |
| CB4-3.4-592 | SBOM auto with transitive deps + Security Champion with checklist PASS | A | SSDF-PW + V10-OAuth | PASS |
| CB4-3.4-593 | Three CONCERNs (broad aud, stale SBOM, 71-bit secret) → CONCERN | B | V10-OAuth + SSDF-PW + RFC-OAuth2 | CONCERN |
| CB4-3.4-594 | Post-remediation: 1 REPROCESS + 3 CONCERNs all closed → PASS | A | V10-OAuth + RFC-JWT + SSDF-PW | PASS |
