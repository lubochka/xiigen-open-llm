# CB4 Stage 3 — Node Pipeline Chain Set Extension 15
# Sequences CB4-3.4-551 through CB4-3.4-558
# Stratum: C — full 9-node pipeline traces for ext21 patterns
# Instances: 16 (8 base pairs × 2 phrasings)
# Running total after this file: 1116 instances

---

## Pair[CB4-3.4-551]

**A:** Full pipeline trace — mTLS client authentication. Authorization server: token endpoint requires mTLS for client authentication (`tls_client_auth`). Clients authenticate by presenting a TLS client certificate. Authorization server validates the certificate against a trusted CA (Certificate Authority). Issued access tokens include `cnf/x5t#S256` (SHA-256 thumbprint of the client certificate). Resource server validates `cnf/x5t#S256` on every request.

**B:**
```
N1 · Domain Splitter
  Input: mTLS client authentication, certificate-bound tokens, cnf/x5t#S256 validation
  Domain signals: client authentication strength, token binding, OAuth 2.0 mutual TLS
  Territories opened: T-oauth-oidc (primary), T-rfc-oauth2 (RFC 8705)
  Split: [mtls-client-auth | certificate-bound-token | resource-server-validation]

N2 · Domain Translator
  mtls-client-auth → RFC 8705 §2.1: tls_client_auth method
  certificate-bound-token → RFC 8705 §3: cnf/x5t#S256 claim in access token
  resource-server-validation → token binding check: certificate thumbprint match per request

N3 · Scheme Planner
  Primary scheme: RFC8705-mTLS-client-auth
  Secondary scheme: RFC8705-certificate-bound-token
  Conflict check: none — all components present
  Plan: verify CA validation; verify token binding claim; verify RS validation

N4 · Domain Processor
  mTLS token endpoint:
    Client presents TLS certificate ✓
    Authorization server validates against trusted CA ✓
    tls_client_auth method: proven-origin client authentication ✓
  Certificate-bound token:
    cnf/x5t#S256: SHA-256 of client certificate public key (SPKI fingerprint) ✓
    Included in access token at issuance ✓
  Resource server validation:
    Extracts cnf/x5t#S256 from token ✓
    Computes SHA-256 of certificate presented on mTLS connection ✓
    Compares: if mismatch → token rejected ✓
    Token cannot be used by party without the corresponding client certificate ✓
  No gaps found

N5 · Applier
  Apply RFC8705-mTLS-client-auth: PASS
  Apply RFC8705-certificate-bound-token: PASS
  Verdict: PASS

N6 · Result Translator → PASS

N7 · Arbiter Selector
  Primary: ASVS-V10-OAuth
  Secondary: RFC-OAuth2 (RFC 8705)

N8 · Arbiter Units
  ASVS-V10-OAuth: mTLS ✓; certificate binding ✓ — PASS
  RFC-OAuth2 (RFC 8705): tls_client_auth + cnf/x5t#S256 ✓ — PASS
  T-oauth-oidc verdict: PASS
  T-rfc-oauth2 verdict: PASS

N9 · Formatter → Overall PASS
Overall verdict: PASS
```

**Reason:** mTLS client authentication with CA validation plus certificate-bound tokens with resource server thumbprint validation correctly implements RFC 8705. Token cannot be used by anyone without the client certificate. PASS.

**Phrasing-B:** Pipeline trace for mTLS client auth + cnf/x5t#S256 certificate-bound tokens validated at resource server. Walk through all 9 nodes to confirm PASS.

---

## Pair[CB4-3.4-552]

**A:** Full pipeline trace — UserInfo sub cross-check absent. OIDC login: code-only flow, PKCE S256, nonce validated. After token exchange, application calls UserInfo endpoint. Response is plain JSON with `sub`, `email`, `name`. Application stores these claims in the user session without comparing `sub` to the ID token's `sub`. Team: "The UserInfo call is authenticated — it can only return data for the authenticated user."

**B:**
```
N1 · Domain Splitter
  Input: UserInfo JSON claims accepted without sub cross-check, "authenticated call = safe" argument
  Domain signals: UserInfo response validation, OIDC Core §5.3.4, confused-deputy protection
  Territories opened: T-oauth-oidc (primary), T-rfc-jwt
  Split: [userinfo-call-authentication | sub-cross-check-requirement | adversarial-argument]

N2 · Domain Translator
  userinfo-call-authentication → UserInfo call uses access token ✓ (authenticated)
  sub-cross-check-requirement → OIDC Core §5.3.4: sub in UserInfo MUST equal sub in ID token
  adversarial-argument → "authenticated = can only return own data" → flag for N5

N3 · Scheme Planner
  Primary scheme: OIDC-UserInfo-validation
  Conflict check: call is authenticated; sub cross-check is an independent correctness guarantee
  Plan: verify sub cross-check; reject "authenticated = validated" argument

N4 · Domain Processor
  UserInfo call: uses access token ✓ — authentication confirms call identity
  sub cross-check: sub in UserInfo compared to ID token sub ✗ — ABSENT
  OIDC Core §5.3.4 requirement:
    "The sub Claim in the UserInfo Response MUST be verified to exactly match the sub Claim
     in the ID Token"
  Team argument: "authenticated call can only return own data"
    → Authenticated call: returns data for the user the access token represents ✓
    → Cross-check ensures the user in UserInfo is the SAME user in the ID token
    → If somehow different (edge case, misconfiguration, MITM on internal call): cross-check detects
    → "Authenticated = can only return own data" assumes perfect internal routing ✗
    → REJECTED: cross-check is the verification step; assumption is not verification

N5 · Applier
  Adversarial rejection: "authenticated call = safe" → REJECTED (OIDC Core requires explicit verification)
  Apply OIDC-UserInfo-sub-cross-check: CONCERN — absent; HTTPS mitigates primary threat
  Verdict: CONCERN (not REPROCESS — HTTPS mitigates primary network threat; cross-check is defensive)

N6 · Result Translator
  Finding: UserInfo sub cross-check absent; team argument rejected
  Severity: CONCERN

N7 · Arbiter Selector: ASVS-V10-OAuth, RFC-JWT

N8 · Arbiter Units
  ASVS-V10-OAuth: OIDC Core §5.3.4 sub cross-check absent ✗ — CONCERN
  RFC-JWT: sub binding validation absent ✗ — CONCERN
  T-oauth-oidc verdict: CONCERN
  T-rfc-jwt verdict: CONCERN

N9 · Formatter
  Territory results: CONCERN
  Graduation: 0 REPROCESS, 1+ CONCERN → Overall CONCERN
  Remediation: after UserInfo response, assert UserInfo.sub == ID_token.sub; reject if different
Overall verdict: CONCERN
```

**Reason:** UserInfo authenticated call does not substitute for sub cross-check. "Authenticated = can only return own data" assumes perfect routing — sub cross-check is the explicit verification. CONCERN because HTTPS mitigates the primary MITM threat; the cross-check is a defensive completeness requirement. Team argument rejected.

**Phrasing-B:** Pipeline trace for OIDC UserInfo response accepted without sub cross-check. Team: "Authenticated UserInfo call can only return the user's own data." Walk through all 9 nodes.

---

## Pair[CB4-3.4-553]

**A:** Full pipeline trace — no formal security training. Organization: no security training program. Security knowledge is acquired on-the-job. Recent incidents: two XSS vulnerabilities found in production (by external pen-test) traceable to developers unfamiliar with output encoding requirements. Team: "We learn from security incidents — they make the team stronger."

**B:**
```
N1 · Domain Splitter
  Input: no formal training, XSS incidents traceable to knowledge gap, "learn from incidents" argument
  Domain signals: organizational security process, SSDF PO.1, developer knowledge gap linkage
  Territories opened: T-ssdf-process (primary)
  Split: [training-program-absence | incident-linkage | adversarial-argument]

N2 · Domain Translator
  training-program-absence → SSDF PO.1.1: organizational security training required ✗
  incident-linkage → CWE-79 XSS incidents directly linked to absent training → escalation signal
  adversarial-argument → "learn from incidents" → reactive vs proactive distinction

N3 · Scheme Planner
  Primary scheme: SSDF-PO1-training
  Escalation check: incidents traceable to knowledge gap may escalate CONCERN to REPROCESS
  Plan: assess PO.1 finding; evaluate whether incident linkage changes severity

N4 · Domain Processor
  Training program: absent ✗ (PO.1.1 gap)
  Incident linkage:
    Two production XSS vulnerabilities found by external pen-test ✗
    Root cause: developers unfamiliar with output encoding ✗
    Training absence → knowledge gap → production vulnerability → external discovery
    This is the failure mode that PO.1 training is designed to prevent ✗
  Team argument: "learn from incidents"
    Reactive learning (post-incident) ≠ SSDF-required proactive training ✗
    "Made the team stronger" does not close the training program gap ✗
    → REJECTED

  Severity escalation:
    Training absence alone: CONCERN
    Training absence WITH production vulnerabilities traceable to knowledge gap:
      REPROCESS — the gap directly caused verifiable security failures ✗
    Active vulnerability evidence changes severity from CONCERN to REPROCESS

N5 · Applier
  Adversarial rejection: "learn from incidents" → REJECTED (reactive ≠ proactive per SSDF)
  Apply SSDF-PO1: REPROCESS (training absence caused active vulnerabilities)
  Verdict: REPROCESS

N6 · Result Translator
  Finding: no training program; XSS incidents directly traceable to training gap
  Severity: REPROCESS — absence caused verifiable production vulnerabilities

N7 · Arbiter Selector: SSDF-PW (PO.1)

N8 · Arbiter Units
  SSDF-PW:
    PO.1.1: training program absent ✗
    Escalation: incidents traceable to knowledge gap ✗ → REPROCESS
  T-ssdf-process verdict: REPROCESS

N9 · Formatter
  Territory results: T-ssdf-process=REPROCESS
  Graduation: REPROCESS → Overall REPROCESS
  Remediation: establish role-based security training; cover OWASP Top 10 including XSS/output encoding
Overall verdict: REPROCESS
```

**Reason:** No formal security training (normally CONCERN) escalates to REPROCESS because production XSS vulnerabilities are directly traceable to the knowledge gap the training program would have closed. "Learn from incidents" is reactive and does not satisfy proactive PO.1 requirements. When training absence has caused verifiable production vulnerabilities, REPROCESS applies.

**Phrasing-B:** Pipeline trace for no security training plus two production XSS incidents traceable to developer knowledge gaps. Team: "We learn from incidents." Walk through all 9 nodes.

---

## Pair[CB4-3.4-554]

**A:** Full pipeline trace — refresh rotation without family revocation. Authorization server: single-use refresh token rotation. On receiving a previously-used (rotated-away) refresh token: server logs the replay event and returns `invalid_grant` error. The current (active) refresh token for the family is NOT revoked. Team: "The old token is rejected — the attacker cannot use it. Our rotation is correct."

**B:**
```
N1 · Domain Splitter
  Input: single-use rotation, replay → error but no family revocation, team argument
  Domain signals: refresh token security, stolen token race condition, BCP §4.14.1
  Territories opened: T-oauth-oidc (primary), T-rfc-oauth2
  Split: [rotation-mechanism | replay-response | attacker-first-scenario]

N2 · Domain Translator
  rotation-mechanism → RFC 6749 refresh rotation ✓; single-use ✓
  replay-response → replay detected → error returned; current token NOT revoked ✗
  attacker-first-scenario → if attacker exchanges first, attacker holds current token ✗

N3 · Scheme Planner
  Primary scheme: V10-3-refresh-token-security
  Conflict check: rotation correct; family revocation absent = race condition unhandled
  Plan: construct attacker-first scenario; show that rotation alone is insufficient; verdict

N4 · Domain Processor
  Rotation: single-use ✓
  Replay detected: error returned to replayer ✓
  Family revocation: NOT performed ✗

  Race condition analysis:
    Token theft: attacker obtains refresh token (e.g., from log, memory, network)
    Scenario A (attacker exchanges AFTER legitimate client):
      Legitimate client exchanges → new token issued → old invalidated ✓
      Attacker presents old token → replay detected → error returned ✓
      Attacker cannot proceed ✓ — rotation handles this case correctly
    Scenario B (attacker exchanges BEFORE legitimate client):
      Attacker exchanges stolen token → new token-B issued to attacker ✓
      Attacker holds token-B ✓
      Legitimate client now presents their copy of the token → replay detected ✓
      Server returns error to legitimate client ✓
      Family NOT revoked ✗ → token-B (attacker's current token) remains valid ✗
      Attacker continues to have access; legitimate client is locked out ✗
      Theft is silent — user is not forced to re-authenticate to detect the breach ✗

  Family revocation closes Scenario B:
    On replay detection: revoke all family tokens ✓
    Both parties forced to re-authenticate ✓
    Theft surfaces → user/system can act ✓

  Team argument: "old token rejected = attacker blocked"
    → Only correct for Scenario A (attacker exchanges after)
    → Fails for Scenario B (attacker exchanges first) ✗
    → REJECTED (partial analysis)

N5 · Applier
  Adversarial rejection: "old token rejected = attacker blocked" → REJECTED (Scenario B unhandled)
  Apply V10-3-refresh-family: CONCERN — rotation correct; family revocation absent
  Verdict: CONCERN

N6 · Result Translator
  Finding: rotation correct; Scenario B (attacker-first) unhandled without family revocation
  Severity: CONCERN

N7 · Arbiter Selector: ASVS-V10-OAuth, RFC-OAuth2

N8 · Arbiter Units
  ASVS-V10-OAuth: rotation ✓; family revocation absent ✗ — CONCERN
  RFC-OAuth2 (BCP §4.14.1): SHOULD revoke family on replay — absent ✗ — CONCERN
  T-oauth-oidc verdict: CONCERN
  T-rfc-oauth2 verdict: CONCERN

N9 · Formatter
  Graduation: 0 REPROCESS, 1+ CONCERN → Overall CONCERN
  Remediation: on replay detection, revoke all tokens in the family; force re-authentication
Overall verdict: CONCERN
```

**Reason:** Single-use rotation handles the case where the attacker uses the token AFTER the legitimate client. It does NOT handle the attacker-first case (Scenario B) — if the attacker exchanges first, they hold the current token, and the legitimate client is the one triggering replay detection. Without family revocation, the attacker's current token remains valid. "Old token rejected = attacker blocked" is rejected as a partial analysis. CONCERN.

**Phrasing-B:** Pipeline trace for refresh rotation with replay detection but no family revocation. Team: "Old token is rejected — attacker blocked." Walk through the attacker-first scenario in all 9 nodes.

---

## Pair[CB4-3.4-555]

**A:** Full pipeline trace — refresh rotation with family revocation. Same as CB4-3.4-554 but with family revocation: on replay detection, server revokes all tokens in the family (including the current active token). Both the legitimate client and any attacker are forced to re-authenticate.

**B:**
```
N1 · Domain Splitter
  Input: single-use rotation + family revocation on replay detection
  Domain signals: stolen token detection, forced re-authentication, BCP §4.14.1
  Territories opened: T-oauth-oidc, T-rfc-oauth2
  Split: [rotation-mechanism | family-revocation | attacker-first-scenario]

N2 · Domain Translator
  rotation-mechanism → single-use ✓; replay-detection ✓
  family-revocation → on replay: all tokens in family revoked ✓
  attacker-first-scenario → Scenario B: attacker exchanges first; replay detected from legitimate
    client → family revoked ✓ → attacker's current token also revoked ✓

N3 · Scheme Planner → all PASS expected

N4 · Domain Processor
  Scenario B (attacker-first) with family revocation:
    Attacker exchanges stolen token → token-B issued ✓
    Legitimate client presents their (now-stale) copy → replay detected ✓
    Family revocation: token-B revoked ✓; all family tokens revoked ✓
    Attacker's token-B: immediately invalid after family revocation ✓
    Both parties forced to re-authenticate → theft surfaces ✓
  BCP §4.14.1: "If replay is detected, all refresh tokens of the token series SHOULD be revoked"
    Implementation matches SHOULD (followed) ✓

N5 · Applier → PASS

N6 · Result Translator → PASS

N7 · Arbiter Selector: ASVS-V10-OAuth, RFC-OAuth2

N8 · Arbiter Units
  ASVS-V10-OAuth: rotation + family revocation ✓ — PASS
  RFC-OAuth2 BCP §4.14.1: SHOULD met ✓ — PASS
  All territories: PASS

N9 · Formatter → Overall PASS
Overall verdict: PASS
```

**Reason:** Refresh token rotation with family revocation correctly handles the attacker-first scenario — when the legitimate client triggers replay detection, family revocation invalidates the attacker's current token and forces re-authentication for both parties. Matches OAuth 2.0 BCP §4.14.1. PASS.

**Phrasing-B:** Pipeline trace for refresh rotation where replay detection triggers family revocation. Walk through all 9 nodes including the attacker-first scenario.

---

## Pair[CB4-3.4-556]

**A:** Full pipeline trace — weak client_credentials secret. Authorization server: client_credentials grant for M2M service. Client secret: 8-character alphanumeric string (~47 bits entropy). Client secret stored hashed. Team: "8 characters is our standard password policy for all credentials — it's sufficient."

**B:**
```
N1 · Domain Splitter
  Input: 8-char alphanumeric client secret, "standard password policy" argument
  Domain signals: client credential strength, M2M authentication security
  Territories opened: T-oauth-oidc (primary), T-rfc-oauth2
  Split: [client-secret-entropy | policy-applicability | comparison-to-requirements]

N2 · Domain Translator
  client-secret-entropy → 62^8 ≈ 218 trillion combinations ≈ 47 bits entropy
  policy-applicability → human password policy ≠ machine secret requirements
  comparison-to-requirements → ASVS V10 / RFC 6749: client secrets should be high-entropy

N3 · Scheme Planner
  Primary scheme: V10-client-credential-strength
  Plan: assess 47-bit entropy against requirements; evaluate policy-substitution argument

N4 · Domain Processor
  8-character alphanumeric entropy: 47 bits
    Adequate for human passwords (entropy vs guessing rate with bcrypt)
    Not adequate for machine secrets: attackers may have offline attack capability;
    high-speed credential stuffing against token endpoint without rate limiting
    Machine credentials should be 128-bit minimum (ASVS V9.1 / general NIST guidance)
  "Standard password policy applies": human password policy ≠ machine secret requirements
    Humans have rate limits (login page, lockout) built around human interaction pace
    Machine-to-machine: token endpoint may be faster; secrets should be higher entropy
    → CONCERN (not REPROCESS — 47 bits is not negligible; below recommended machine threshold)
  Storage (hashed) ✓ — correct

N5 · Applier
  Team argument: "standard password policy sufficient" → CONCERN (below 128-bit machine threshold)
  Verdict: CONCERN

N6 · Result Translator
  Finding: 8-char alphanumeric client secret below recommended 128-bit entropy for machine secrets
  Severity: CONCERN (not REPROCESS — entropy is not negligible; below recommended threshold)

N7 · Arbiter Selector: ASVS-V10-OAuth

N8 · Arbiter Units
  ASVS-V10-OAuth: client secret entropy below machine-secret recommendation ✗ — CONCERN
  T-oauth-oidc verdict: CONCERN

N9 · Formatter
  Graduation: 0 REPROCESS, CONCERN → Overall CONCERN
  Remediation: generate client secrets using CSPRNG with ≥128-bit entropy
Overall verdict: CONCERN
```

**Reason:** 8-character alphanumeric client secret yields ~47 bits entropy — below the 128-bit recommended threshold for machine credentials. "Standard password policy applies" is incorrect reasoning: human password policies account for human-scale interaction rates; machine secrets should be higher entropy. CONCERN because entropy is not negligible, but below the machine-secret recommended minimum.

**Phrasing-B:** Pipeline trace for client_credentials with 8-char alphanumeric secret. Team: "Our standard password policy is sufficient." Walk through all 9 nodes.

---

## Pair[CB4-3.4-557]

**A:** Full pipeline trace — combined ext21 CONCERNs. Application: (1) UserInfo sub cross-check absent. (2) Refresh rotation without family revocation. (3) No formal security training. All other OAuth controls correct (code-only, PKCE S256, PAR, nonce+aud, correct dynamic registration, etc.).

**B:**
```
N1 · Domain Splitter
  Input: three CONCERN-level gaps; all core OAuth controls correct
  Territories opened: T-oauth-oidc, T-ssdf-process, T-rfc-oauth2
  Split: [userinfo-sub-check | refresh-family-revocation | training-program | correct-controls]

N2 · Domain Translator
  userinfo-sub-check → OIDC Core §5.3.4 → CONCERN
  refresh-family-revocation → BCP §4.14.1 → CONCERN (rotation present; family revocation absent)
  training-program → SSDF PO.1 → CONCERN (no incidents cited this time)
  correct-controls → PKCE, PAR, nonce, aud, etc. → PASS

N3 · Scheme Planner
  Three CONCERN candidates + multiple PASS
  Graduation preview: 0 REPROCESS + N CONCERN → CONCERN (graduation rule)
  Plan: verify each independently; confirm graduation

N4 · Domain Processor
  UserInfo sub cross-check: absent ✗ → CONCERN (HTTPS mitigates primary risk)
  Refresh rotation family revocation: absent ✗ → CONCERN (rotation handles Scenario A; Scenario B unhandled)
  Security training: absent ✗; no incidents cited → CONCERN (not REPROCESS; no incident linkage)
  Core controls: all correct ✓ → PASS (code-only, PKCE S256, PAR, nonce, aud, etc.)

N5 · Applier
  Three CONCERN + multiple PASS
  Graduation: 0 REPROCESS, 3 CONCERN → CONCERN

N6 · Result Translator → CONCERN (three gaps; no REPROCESS)

N7 · Arbiter Selector: ASVS-V10-OAuth, T-ssdf-process, RFC-OAuth2

N8 · Arbiter Units
  T-oauth-oidc: UserInfo ✗ → CONCERN; core controls ✓ → PASS
  T-rfc-oauth2: family revocation ✗ → CONCERN; rotation ✓
  T-ssdf-process: PO.1 ✗ → CONCERN (no incident linkage → stays CONCERN)

N9 · Formatter
  Territory results: multiple CONCERN, no REPROCESS
  Graduation: 0 REPROCESS, N CONCERN → Overall CONCERN
Overall verdict: CONCERN
```

**Reason:** Three independent CONCERNs: UserInfo sub cross-check absent, refresh rotation without family revocation, no formal security training (no incident linkage → stays CONCERN). All core OAuth controls correct (PASS). Three CONCERNs do not escalate to REPROCESS. Overall CONCERN.

**Phrasing-B:** Pipeline trace for three CONCERN-level gaps (UserInfo sub check, refresh family revocation, training) with all core OAuth controls correct. Does three CONCERNs become REPROCESS?

---

## Pair[CB4-3.4-558]

**A:** Full pipeline trace — all ext21 patterns correct. mTLS available for high-security M2M; UserInfo sub cross-check implemented; OIDC Core §5.3.4 satisfied; formal annual security training (SSDF PO.1); refresh rotation with family revocation (BCP §4.14.1); client_credentials secrets at 128-bit entropy. All prior controls correct.

**B:**
```
N1 · Domain Splitter
  Input: comprehensive ext21 implementation — all patterns correct
  Territories opened: T-oauth-oidc, T-rfc-oauth2, T-rfc-jwt, T-ssdf-process, T-asvs-authentication
  Split: [mtls-userinfo | refresh-rotation | training | client-credentials | prior-controls]

N2 · Domain Translator
  All sub-domains: correct implementations → expected PASS per sub-scheme

N3 · Scheme Planner → all PASS expected; no conflicts

N4 · Domain Processor
  mTLS: tls_client_auth + cnf/x5t#S256 ✓
  UserInfo: sub cross-check ✓; JWT sig validation ✓
  Training: annual role-based ✓ (SSDF PO.1 satisfied)
  Refresh rotation: single-use ✓; family revocation on replay ✓ (BCP §4.14.1)
  client_credentials: 128-bit CSPRNG secrets ✓; bcrypt stored ✓
  Prior controls: all correct ✓
  No gaps

N5 · Applier → all PASS

N6 · Result Translator → PASS

N7 · Arbiter Selector: all territories

N8 · Arbiter Units: all PASS

N9 · Formatter → Overall PASS
Overall verdict: PASS
```

**Reason:** All ext21 patterns correctly implemented: mTLS with certificate binding, UserInfo sub cross-check, annual PO.1 training, refresh rotation with family revocation, 128-bit client secrets, plus all prior controls. Comprehensive multi-territory PASS.

**Phrasing-B:** Pipeline trace for all ext21 patterns correct: mTLS, UserInfo sub cross-check, PO.1 training, refresh family revocation, 128-bit secrets, prior controls. Walk through all 9 nodes.

---

## File Summary

| Pair ID | Description | Stratum | Pipeline Focus | Verdict |
|---------|-------------|---------|----------------|---------|
| CB4-3.4-551 | mTLS client auth + cnf/x5t#S256 certificate-bound tokens pipeline trace | C | T-oauth-oidc + T-rfc-oauth2 | PASS |
| CB4-3.4-552 | UserInfo sub cross-check absent pipeline trace — "authenticated = safe" rejected | C/D | T-oauth-oidc + T-rfc-jwt | CONCERN |
| CB4-3.4-553 | No training + production XSS incidents traceable to gap — escalated | C/D | T-ssdf-process | REPROCESS |
| CB4-3.4-554 | Refresh rotation without family revocation — attacker-first scenario | C | T-oauth-oidc + T-rfc-oauth2 | CONCERN |
| CB4-3.4-555 | Refresh rotation with family revocation — Scenario B handled | C | T-oauth-oidc + T-rfc-oauth2 | PASS |
| CB4-3.4-556 | client_credentials 8-char secret below 128-bit threshold | C | T-oauth-oidc | CONCERN |
| CB4-3.4-557 | Three CONCERNs (UserInfo, family revocation, training) → CONCERN graduation | C | Multiple | CONCERN |
| CB4-3.4-558 | All ext21 patterns correct — comprehensive pipeline PASS | C | Multiple | PASS |
