# Node Pipeline Chain — Extension Set 2
# Stage 3 Action 3 — Coverage Extension (final domain pipeline traces)
# Batch: CB4 — Sub-phase 3.4 (Security and Authorization Model)
# Sequences: CB4-3.4-215 through CB4-3.4-222
# Instances: 16 (8 base pairs × 2 phrasings each)
# Running total after this file: 444 instances (sequences 001–222)

---

## Coverage targets for this file

Full 9-node pipeline traces for remaining new domain scenarios.
Chains:
  215: V6.2.4 password change (no current_password) + V8.1.2 deny-by-default violation
  216: V7.2.3 session fixation + V7.3.1 logout invalidation (both V7 gaps in one chain)
  217: SSDF PW.9 pre-merge + PO.3 SBOM (two SSDF gaps, both REPROCESS)
  218: V10.5.1 ID token aud + V10.3.3 refresh revocation (two V10 gaps, REPROCESS)
  219: CWE-352 CSRF + CWE-307 brute force (two CWE vulnerabilities)
  220: V9.3.1 URL token + V6.2.5 bcrypt (one REPROCESS, one PASS)
  221: Adversarial all-CONCERN chain (multiple CONCERN-level gaps, overall CONCERN)
  222: New-domain all-PASS extended baseline (V6.2.4+V7.2.4+V8.1.2+V9.3.1+V10.5.1 all satisfied)

---

## Pair CB4-3.4-215

**Stratum:** C (V6.2.4 + V8.1.2: two ASVS must violations from different chapters)
**Source:** OWASP ASVS 5.0.0
**Verdict:** REPROCESS

### A

```
submission:
  system: "AdminPortal v2.8"
  claims:
    - "POST /api/account/password: accepts new_password without current_password"
    - "authorization policy: default ALLOW; sensitive /admin/* on deny list"
    - "V6.2.5: bcrypt cost=12 — confirmed"
    - "V8.3.1: admin role check on /admin/* — confirmed"
```

### B (abbreviated chain)

```
node_01: [T-asvs-auth: V6.2.4+V6.2.5] + [T-asvs-authz: V8.1.2+V8.3.1]
node_04:
  V6.2.4: no current password on password change → REPROCESS
  V6.2.5: bcrypt cost=12 → PASS
  V8.1.2: default ALLOW + deny list → REPROCESS (inverted policy)
  V8.3.1: admin role check → PASS
node_08:
  ASVS-V6-Auth: REPROCESS (V6.2.4) + PASS (V6.2.5)
  ASVS-V8-AuthZ: REPROCESS (V8.1.2) + PASS (V8.3.1)
node_09:
  overall_verdict: REPROCESS
  V6: REPROCESS — password change missing current_password verification
  V8: REPROCESS — default ALLOW violates deny-by-default requirement
  note: "Two separate ASVS chapters, two REPROCESS findings. V6.2.5 and V8.3.1 partial passes noted but do not affect overall verdict."
  attribution: "OWASP ASVS 5.0.0 CC BY-SA 4.0"
```

### Reason

Two REPROCESS findings from different ASVS chapters (V6.2.4 and V8.1.2). The partial passes (V6.2.5 bcrypt and V8.3.1 admin role) are noted. The overall verdict is REPROCESS because multiple MUST violations exist. Node 09 must enumerate all findings — both REPROCESSes and both PASSes — in the formatted output.

### Phrasing B

```
AdminPortal: no current_password on password change (V6.2.4 gap). Authorization: default ALLOW + deny list (V8.1.2 gap). bcrypt and admin role check satisfied.
Full pipeline. Two chapters, two REPROCESS findings. What's the output?
```

Expected: ASVS-V6 REPROCESS (V6.2.4). ASVS-V8 REPROCESS (V8.1.2). V6.2.5 and V8.3.1 PASSes noted. Overall REPROCESS.

---

## Pair CB4-3.4-216

**Stratum:** C (V7.2.3 session fixation + V7.3.1 logout: both V7 gaps)
**Source:** OWASP ASVS 5.0.0
**Verdict:** REPROCESS

### A

```
submission:
  system: "CustomerPortal v1.6"
  claims:
    - "V7.2.1: session tokens over TLS — confirmed"
    - "V7.2.3: session ID same before and after login — no regeneration"
    - "V7.3.1: session not invalidated on logout — token remains valid"
```

### B (abbreviated chain)

```
node_01: [T-asvs-session: V7.2.1+V7.2.3+V7.3.1]
node_04:
  V7.2.1: TLS → PASS
  V7.2.3: no regeneration after login → REPROCESS (session fixation)
  V7.3.1: session not invalidated on logout → REPROCESS (session replay)
node_08:
  ASVS-V7-Session:
    verdict: REPROCESS
    satisfied: [V7.2.1]
    violated: [V7.2.3, V7.3.1]
    gap_V7_2_3: "Session ID not regenerated after login — session fixation vulnerability"
    gap_V7_3_1: "Session not server-invalidated on logout — token remains valid after logout"
    action_required:
      - "V7.2.3: Regenerate session ID immediately after successful authentication"
      - "V7.3.1: Invalidate session server-side on logout"
node_09:
  overall_verdict: REPROCESS
  V7.2.1: PASS noted
  V7.2.3+V7.3.1: REPROCESS — both session management gaps require remediation
```

### Reason

Two V7 violations in one submission. V7.2.1 PASS noted. V7.2.3 and V7.3.1 both REPROCESS. The single-arbiter (ASVS-V7-Session-Arbiter) evaluates all V7 sub-requirements and returns a REPROCESS because at least two are violated.

### Phrasing B

```
CustomerPortal: V7.2.1 TLS (OK). V7.2.3: no session regen after login. V7.3.1: no logout invalidation.
ASVS-V7-Session-Arbiter. Two V7 violations. What's the formatted output?
```

Expected: ASVS-V7 REPROCESS. V7.2.1 PASS noted. V7.2.3 + V7.3.1 both REPROCESS. Overall REPROCESS. Two separate action_required items.

---

## Pair CB4-3.4-217

**Stratum:** C (SSDF PW.9 + PO.3: two SSDF practice gaps)
**Source:** NIST SP 800-218 SSDF v1.1
**Verdict:** REPROCESS

### A

```
submission:
  system: "DevPlatform v3.0"
  claims:
    - "SSDF PW.9: Snyk runs post-merge (not pre-merge)"
    - "SSDF PO.3: no SBOM maintained; 34 third-party libraries"
    - "SSDF PW.2: threat model updated 2026-01-10 — current"
    - "SSDF PW.4: SonarQube SAST in CI — confirmed"
```

### B (abbreviated chain)

```
node_01: [T-ssdf-pw9: PW.9 timing] + [T-ssdf-po3: PO.3 SBOM] + [T-ssdf-pw2: PW.2] + [T-ssdf-pw4: PW.4]
node_04:
  PW.9: post-merge timing → REPROCESS
  PO.3: no SBOM → CONCERN
  PW.2: updated 2026-01-10 (4 months ago) → PASS
  PW.4: SonarQube SAST → PASS
node_07: SSDF-PO-Arbiter + SSDF-PW-Arbiter
node_08:
  SSDF-PW-Arbiter:
    verdict: REPROCESS
    satisfied: [PW.2.1, PW.4.1]
    violated: [PW.9]
    gap: "PW.9: Snyk post-merge violates pre-integration review requirement"
  SSDF-PO-Arbiter:
    verdict: CONCERN
    gap: "PO.3: no SBOM for 34 third-party libraries"
node_09:
  overall_verdict: REPROCESS
  PW.2+PW.4: PASS noted
  PW.9: REPROCESS
  PO.3: CONCERN (noted; does not change overall REPROCESS verdict)
```

### Reason

SSDF-PW REPROCESS (PW.9 timing) and SSDF-PO CONCERN (PO.3 SBOM). REPROCESS dominates. PW.2 and PW.4 PASSes noted. The formatter shows all four findings to give the team a complete picture.

### Phrasing B

```
DevPlatform: PW.9 post-merge (REPROCESS), PO.3 no SBOM (CONCERN), PW.2 current (PASS), PW.4 SAST (PASS).
Full chain, two SSDF arbiters. Overall verdict?
```

Expected: SSDF-PW REPROCESS (PW.9). SSDF-PO CONCERN (PO.3). Overall REPROCESS. PW.2+PW.4 PASSes noted.

---

## Pair CB4-3.4-218

**Stratum:** C (V10.5.1 ID token + V10.3.3 refresh revocation: two V10 gaps)
**Source:** OWASP ASVS 5.0.0
**Verdict:** REPROCESS

### A

```
submission:
  system: "OAuthServer v4.1"
  claims:
    - "V10.2.1: authorization code + PKCE S256 — confirmed"
    - "V10.3.1: revocation endpoint — confirmed"
    - "V10.3.2: access token revocation — confirmed"
    - "V10.3.3: refresh tokens — no revocation mechanism, 7-day validity"
    - "V10.5.1: ID token aud claim = 'all-clients'"
```

### B (abbreviated chain)

```
node_08:
  ASVS-V10-OAuth-Arbiter:
    satisfied: [V10.2.1, V10.3.1, V10.3.2]
    violated: [V10.3.3, V10.5.1]
    verdict: REPROCESS
    gap_V10_3_3: "Refresh tokens non-revocable, 7-day validity"
    gap_V10_5_1: "ID token aud='all-clients'; must contain client_id"
node_09:
  overall_verdict: REPROCESS
  V10.2.1+V10.3.1+V10.3.2: PASS noted
  V10.3.3+V10.5.1: REPROCESS — both violations require remediation before resubmission
```

### Reason

Single arbiter (ASVS-V10-OAuth-Arbiter), multiple sub-requirement violations. Two REPROCESS gaps (V10.3.3 and V10.5.1). Three satisfied (V10.2.1, V10.3.1, V10.3.2). Overall REPROCESS.

### Phrasing B

```
OAuthServer: V10.2.1+V10.3.1+V10.3.2 satisfied. V10.3.3: no refresh revocation. V10.5.1: aud='all-clients'.
Full chain. Overall verdict?
```

Expected: V10-OAuth REPROCESS. Both gaps require remediation. Satisfied sub-requirements noted. Overall REPROCESS.

---

## Pair CB4-3.4-219

**Stratum:** C (CWE-352 CSRF + CWE-307 brute force: two CWE vulnerabilities, one REPROCESS each)
**Source:** CWE Top 25 2023
**Verdict:** REPROCESS

### A

```
submission:
  system: "WebApp v5.0"
  claims:
    - "POST /api/account/email-change: no CSRF token, no SameSite"
    - "login endpoint: lockout after 100 attempts (too high threshold)"
    - "session cookie: httpOnly, secure flags set"
```

### B (abbreviated chain)

```
node_01: [T-cwe-authz: CWE-352 email-change] + [T-cwe-authn: CWE-307 lockout threshold]
node_08:
  CWE-Authz-Arbiter:
    verdict: REPROCESS
    cwe_id: CWE-352
    severity: HIGH
    gap: "No CSRF token on email-change endpoint"
  CWE-Authn-Arbiter:
    verdict: CONCERN
    cwe_id: CWE-307
    severity: MEDIUM
    gap: "Lockout at 100 attempts — threshold too high"
node_09:
  overall_verdict: REPROCESS
  CWE-352: REPROCESS (email-change, no CSRF)
  CWE-307: CONCERN (lockout exists at 100 — partial)
  note: "REPROCESS dominates. Address CWE-352 first (no protection at all); CWE-307 threshold reduction is secondary."
```

### Reason

CWE-352 REPROCESS (no CSRF protection at all) and CWE-307 CONCERN (partial lockout exists). REPROCESS dominates. The note prioritizes CWE-352 remediation first since CWE-307 is a partial control (lockout exists but at wrong threshold).

### Phrasing B

```
WebApp: email-change has no CSRF token (CWE-352). Login: lockout at 100 attempts (CWE-307).
CWE-Authz and CWE-Authn arbiters. Overall verdict?
```

Expected: CWE-Authz REPROCESS (CWE-352). CWE-Authn CONCERN (CWE-307, lockout present but high). Overall REPROCESS.

---

## Pair CB4-3.4-220

**Stratum:** C (V9.3.1 URL token REPROCESS + V6.2.5 bcrypt PASS: REPROCESS from one, PASS from another)
**Source:** OWASP ASVS 5.0.0
**Verdict:** REPROCESS

### A

```
submission:
  system: "DataAPI v2.0"
  claims:
    - "V6.2.5: bcrypt cost=12 — confirmed"
    - "V9.3.1: some endpoints accept ?token= query parameter (legacy)"
    - "newer endpoints: Authorization: Bearer header"
    - "V9.1.1: RS256 — confirmed"
    - "V9.2.1: exp=15min — confirmed"
```

### B (abbreviated chain)

```
node_01: [T-asvs-auth: V6.2.5] + [T-token-validation: V9.1.1+V9.2.1+V9.3.1]
node_08:
  ASVS-V6-Auth: PASS (V6.2.5 bcrypt)
  ASVS-V9-Token:
    satisfied: [V9.1.1, V9.2.1]
    violated: [V9.3.1]
    verdict: REPROCESS
    gap: "URL query parameter token delivery on legacy endpoints — tokens exposed in logs and browser history"
node_09:
  overall_verdict: REPROCESS
  V6.2.5: PASS
  V9.1.1+V9.2.1: PASS
  V9.3.1: REPROCESS — URL token delivery
```

### Reason

V6.2.5 PASS (bcrypt) and V9.1.1+V9.2.1 PASS (RS256, short expiry) are noted. V9.3.1 REPROCESS (URL token delivery). Overall REPROCESS because one MUST violation exists.

### Phrasing B

```
DataAPI: bcrypt V6.2.5 (OK). RS256 V9.1.1 (OK). exp=15min V9.2.1 (OK). But some legacy endpoints: ?token= URL parameter.
Full chain. Overall verdict?
```

Expected: V6+V9.1.1+V9.2.1 PASS. V9.3.1 REPROCESS. Overall REPROCESS.

---

## Pair CB4-3.4-221

**Stratum:** C (adversarial — all-CONCERN chain: multiple gaps, none REPROCESS, overall CONCERN)
**Source:** OWASP ASVS 5.0.0, NIST SP 800-218 SSDF v1.1, CWE Top 25 2023
**Verdict:** CONCERN

### A

```
submission:
  system: "ContentMgr v3.0"
  claims:
    - "V6.3.1: TOTP available but optional (no enforcement)"
    - "V7.2.4: no idle timeout"
    - "CWE-307: lockout at 20 attempts (reasonable threshold)"
    - "SSDF PW.2: threat model 10 months old"
    - "SSDF PO.3: no SBOM; 18 third-party libraries"
    - "V6.2.3+V6.2.5+V8.1.1+V8.1.2+V9.1.1+V9.2.1: all satisfied"
```

### B (abbreviated chain)

```
node_08:
  ASVS-V6: CONCERN (V6.3.1 optional MFA), PASS (V6.2.3, V6.2.5)
  ASVS-V7: CONCERN (V7.2.4 no idle timeout)
  ASVS-V8: PASS (V8.1.1, V8.1.2)
  ASVS-V9: PASS (V9.1.1, V9.2.1)
  CWE-Authn: CONCERN (CWE-307: lockout at 20 — partial control, threshold acceptable but no rate limiting)
  SSDF-PW: CONCERN (PW.2: 10 months old — approaching cycle)
  SSDF-PO: CONCERN (PO.3: no SBOM)
node_09:
  overall_verdict: CONCERN
  arbiters_run: 7
  arbiters_passed: 3
  arbiters_concern: 4
  arbiters_failed: 0
  note: "All REPROCESS findings: none. Four CONCERN findings from V6.3, V7.2.4, CWE-307, SSDF PW.2+PO.3. Multiple improvements recommended; none are blocking."
```

### Reason

Seven arbiters run; three PASS (V8, V9, V6.2.x), four CONCERN (V6.3, V7.2.4, CWE-307, SSDF). No REPROCESS findings. Overall CONCERN. This trains the pipeline on a realistic scenario where a system has a reasonable security posture with multiple non-critical gaps.

### Phrasing B

```
ContentMgr: V6.3 optional MFA, no idle timeout, lockout at 20, threat model 10 months, no SBOM.
But V6.2, V8, V9 all satisfied.
Full chain, seven arbiters. Overall verdict?
```

Expected: Overall CONCERN. Four CONCERNs (V6.3, V7.2.4, CWE-307, SSDF). Three PASSes (V8, V9, V6.2). No REPROCESS. Overall CONCERN (non-blocking improvements).

---

## Pair CB4-3.4-222

**Stratum:** C (new-domain all-PASS extended baseline: new domains all satisfied)
**Source:** OWASP ASVS 5.0.0, NIST SP 800-218 SSDF v1.1, CWE Top 25 2023, IETF RFCs
**Verdict:** PASS

### A

```
submission:
  system: "FullStack Platform v6.0"
  claims:
    - "V6.2.3+V6.2.5: TLS + bcrypt — confirmed"
    - "V6.2.4: current password required for password change — confirmed"
    - "V6.3.1+V6.3.2: TOTP available; enforced for admin users"
    - "V7.2.1+V7.2.3+V7.3.1: TLS, session regen after login, logout invalidation"
    - "V7.2.4: 30-min idle timeout with keep-alive prompt"
    - "V8.1.1+V8.1.2+V8.3.1+V8.3.2: ownership, deny-by-default, admin role, audit log"
    - "V9.1.1+V9.2.1+V9.3.1: RS256, 15-min exp, Authorization header only"
    - "V10.2.1+V10.3.1+V10.3.2+V10.3.3+V10.5.1: auth code PKCE, revocation complete, client_id in aud"
    - "CWE-307: lockout at 10 attempts + IP rate limiting"
    - "CWE-352: CSRF token on all state-changing endpoints"
    - "SSDF PO.1+PO.2+PO.3: role-specific training, SCA, SBOM in CycloneDX"
    - "SSDF PW.2+PW.4+PW.8+PW.9: threat model current, SAST, peer review, pre-merge dep review"
    - "RFC-JWT: RS256, iss URI, aud=client_id, exp=15min"
    - "RFC-OAuth2: auth code + PKCE S256"
```

### B (abbreviated chain — Node 09 output)

```
node_09:
  overall_verdict: PASS
  arbiters_run: 11
  arbiters_passed: 11
  scope_boundary: "All assessed domains: ASVS V6–V10, CWE-Authn/Authz, SSDF PO/PW, RFC-JWT, RFC-OAuth2. All requirements satisfied across all assessed chapters and practices."
  attribution: "OWASP ASVS 5.0.0 CC BY-SA 4.0"
```

### Reason

Eleven-arbiter all-PASS comprehensive baseline. All new domains from the extension sets are satisfied simultaneously. This represents the target security posture across all assessed domains. The scope_boundary enumerates what was assessed.

### Phrasing B

```
FullStack Platform: all new domains satisfied — V6.2.4 current password, V6.3 admin MFA enforced,
V7.2.3 session regen, V7.2.4 idle timeout, V8.1.2 deny-by-default, V9.3.1 header only,
V10.5.1 client_id in aud, V10.3.3 refresh revocable, CWE-307 lockout+rate, CWE-352 CSRF tokens,
SSDF PO+PW complete, RFC satisfied.
Full chain. 11 arbiters. Overall?
```

Expected: All 11 PASS. Overall PASS. Scope boundary lists all assessed domains.

---

## File summary

| Seq | Stratum | Sources | Verdict | Chain focus |
|-----|---------|---------|---------|------------|
| 215 | C | ASVS | REPROCESS | V6.2.4 + V8.1.2 violations; V6.2.5 + V8.3.1 passes noted |
| 216 | C | ASVS | REPROCESS | V7.2.3 + V7.3.1 both violated; V7.2.1 pass noted |
| 217 | C | SSDF | REPROCESS | PW.9 REPROCESS + PO.3 CONCERN; PW.2+PW.4 pass |
| 218 | C | ASVS | REPROCESS | V10.3.3 + V10.5.1 violated; V10.2.1+V10.3.1+V10.3.2 pass |
| 219 | C | CWE | REPROCESS | CWE-352 REPROCESS + CWE-307 CONCERN; REPROCESS dominates |
| 220 | C | ASVS | REPROCESS | V9.3.1 URL token REPROCESS; V6.2.5+V9.1.1+V9.2.1 pass |
| 221 | C | Multi | CONCERN | All-CONCERN chain: 4 concerns + 3 passes, no REPROCESS |
| 222 | C | Multi | PASS | 11-arbiter extended baseline: all new domains satisfied |

**Running total: 444 instances (sequences 001–222)**
Stratum breakdown: A ~29%, B ~25%, C ~31%, D ~15%
