# CB4 Stage 3 — Node Cross-Source Set Extension 12
# Sequences CB4-3.4-507 through CB4-3.4-516
# Stratum: B/C/D — multi-source combinations for ext19 patterns
# Instances: 20 (10 base pairs × 2 phrasings)
# Running total after this file: 1032 instances

---

## Pair[CB4-3.4-507]

**A:** OIDC nonce not validated plus scrypt N=1024. Application: OIDC ID token accepted after presence-only nonce check (value not compared). Password hashing: scrypt with N=1024 (32× below the N=32768 minimum). Team: "Our OIDC integration works correctly in testing and our password storage is an industry-standard algorithm."

**B:**
```
Arbiter: ASVS-V10-OAuth + ASVS-V6-Auth

T-oauth-oidc:
  OIDC nonce: present in token ✓; value not compared to sent nonce ✗
  Team: "works correctly in testing" → functionality ≠ security; replay not tested by functional tests
  → REPROCESS (required validation step absent)

T-asvs-authentication:
  scrypt N=1024: 32× below N=32768 minimum ✗
  "Industry-standard algorithm" → algorithm identity correct; parameter configuration wrong
  → REPROCESS (V6.2.5: memory-hard parameters inadequate — same severity as ext13 finding)

Multi-source analysis:
  Two independent REPROCESS findings
  Graduation: REPROCESS + REPROCESS → REPROCESS (count does not change severity)
  Both team arguments rejected: functional testing ≠ security testing; algorithm ≠ correct config

T-oauth-oidc verdict: REPROCESS
T-asvs-authentication verdict: REPROCESS
Overall verdict: REPROCESS
```

**Reason:** Two independent REPROCESS findings: OIDC nonce validation absent (token replay possible) and scrypt N=1024 (inadequate parameters). Two REPROCESSes do not change the graduation — the verdict remains REPROCESS. Both team arguments rejected.

**Phrasing-B:** OIDC nonce presence-only check. scrypt N=1024. Team: "Works in testing; industry-standard algorithm." Two independent REPROCESS findings — what is the verdict?

---

## Pair[CB4-3.4-508]

**A:** OIDC aud validation correct plus K8s ConfigMap credentials. OIDC ID token validation: signature, iss, exp, nbf, aud (against client_id), nonce (value match + single-use). Kubernetes deployment: database connection string (including password) stored in a ConfigMap. Team: "The OIDC validation is thorough and our deployment credentials are in a dedicated config resource."

**B:**
```
Arbiter: ASVS-V10-OAuth + SSDF-PW

T-oauth-oidc:
  OIDC token validation: sig ✓; iss ✓; exp ✓; nbf ✓; aud ✓ (vs client_id); nonce ✓ (value+single-use)
  → PASS (complete validation)

T-ssdf-process:
  Kubernetes ConfigMap: stores plain-text data in etcd ✗
    ConfigMap is not a credential store — it is a plain-text configuration object
    Database password in ConfigMap: accessible to any pod in the namespace with read access ✗
    SSDF PW.1 / V9.3.1: secrets must be stored in dedicated credential stores (Vault, K8s Secret
      with encryption-at-rest) — not ConfigMap
  Team: "dedicated config resource" → ConfigMap ≠ credential resource; lacks encryption or access control
  → REPROCESS (plaintext credential in non-credential store; same analysis as ConfigMap ext13)

Multi-source analysis:
  OIDC PASS + ConfigMap REPROCESS → REPROCESS overrides
  OIDC validation correctness does not protect against credential exposure through K8s ConfigMap

T-oauth-oidc verdict: PASS
T-ssdf-process verdict: REPROCESS
Overall verdict: REPROCESS
```

**Reason:** Complete OIDC aud validation is correct (PASS). Kubernetes ConfigMap with database password is REPROCESS — ConfigMap is a plain-text resource, not a credential store. REPROCESS overrides PASS. "Dedicated config resource" argument rejected: ConfigMap is not a dedicated credential store.

**Phrasing-B:** OIDC validation complete including aud and nonce. Database password in K8s ConfigMap. OIDC PASS + K8s ConfigMap REPROCESS — what is the overall verdict?

---

## Pair[CB4-3.4-509]

**A:** PAR optional plus no session fixation concern. Authorization server: PAR available but not required. Application: stateless JWT-based sessions (no server-side session). User login: JWT issued on successful authentication; new JWT per login. No traditional session cookies. Team: "PAR enhances request security; JWT sessions are stateless and immune to fixation."

**B:**
```
Arbiter: ASVS-V10-OAuth + ASVS-V7-Session

T-oauth-oidc:
  PAR available but not required for public clients ✗
  Authorization parameters (PKCE challenge, state, scope) exposed in URL for public clients ✗
  → CONCERN (PAR improvement available; not applied)

T-asvs-session:
  V7.2.3 session fixation: stateless JWT architecture
    No server-side session ID exists to be fixed ✓
    JWT issued per-login → new token per authentication ✓
    Fixation attack surface absent by architecture ✓
  → PASS (stateless JWT: fixation surface absent; same analysis as ext16 pattern)

Multi-source analysis:
  PAR CONCERN + JWT session PASS
  Graduation: 0 REPROCESS, 1 CONCERN → CONCERN

T-oauth-oidc verdict: CONCERN
T-asvs-session verdict: PASS
Overall verdict: CONCERN
```

**Reason:** PAR optional for public clients is CONCERN (authorization parameters exposed in URL). Stateless JWT sessions have no session fixation surface by architecture (PASS). CONCERN from PAR gap; JWT correctness doesn't compensate for PAR but also doesn't worsen it. Overall CONCERN.

**Phrasing-B:** PAR available but not required (CONCERN). JWT stateless sessions — no fixation surface (PASS). What is the multi-source verdict?

---

## Pair[CB4-3.4-510]

**A:** OIDC complete validation plus token endpoint rate limiting. OIDC: all claims validated including nonce (value match + single-use), aud, at_hash. Token endpoint: rate limited at 20 requests per client_id per minute, exponential back-off after 5 failures. PKCE S256, code-only, Argon2id. No other gaps.

**B:**
```
Arbiter: ASVS-V10-OAuth + ASVS-V8-AuthZ + ASVS-V6-Auth

T-oauth-oidc:
  OIDC token validation: all claims ✓ (nonce value match + single-use, aud, at_hash, sig, iss, exp, nbf)
  PKCE S256 ✓; code-only ✓ — PASS

T-asvs-authentication:
  Token endpoint rate limiting: 20 req/client_id/min ✓; exponential back-off ✓
  Defense-in-depth against rapid exchange, credential stuffing, resource exhaustion ✓
  Argon2id ✓ — PASS

All territories: PASS → Overall PASS
```

**Reason:** Complete OIDC validation (PASS), token endpoint rate limiting with exponential back-off (PASS), PKCE S256, Argon2id. All controls correct. Multi-source PASS.

**Phrasing-B:** Complete OIDC validation, token endpoint rate-limited with back-off, PKCE S256, Argon2id. Does this multi-source configuration produce a PASS?

---

## Pair[CB4-3.4-511]

**A:** OIDC nonce absent from ID token — server-side gap. Application validates OIDC ID tokens. Client always sends `nonce` in authorization request. Upon receiving the ID token, the client checks for the `nonce` claim — but the authorization server does NOT include `nonce` in the issued ID token. Client check fails silently (no nonce in token = validation passes as "nonce not applicable"). Team: "The server might not support nonce — we handle absence gracefully."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-JWT

T-oauth-oidc:
  Client sends nonce in authorization request ✓
  Server includes nonce in ID token: NO ✗
  Client handles absent nonce as "not applicable" ✓ (silent pass through)

OIDC Core §3.1.3.7 nonce validation rule:
  If nonce was sent in the authorization request, nonce MUST be present in the ID token
  Client sending nonce creates an obligation on the server to include it
  Silent acceptance of missing nonce when nonce was explicitly requested: REPROCESS
  (1) Authorization server not including nonce when requested → REPROCESS (server non-compliance)
  (2) Client silently accepting absent nonce when it sent one → REPROCESS (client non-enforcement)

Team argument: "server might not support nonce — handle absence gracefully"
  If nonce was requested, absent nonce is a validation failure, not a graceful degradation path
  Graceful handling is appropriate when nonce was NOT sent; here it was sent
  → REJECTED: absent nonce when explicitly requested = validation failure

Finding: double gap — server not including nonce + client not rejecting token when nonce absent
Overall verdict: REPROCESS
```

**Reason:** Client sends nonce, server ignores it (absent in ID token), client silently accepts — double failure. OIDC Core requires nonce presence when nonce was requested. "Handle absence gracefully" is rejected: graceful handling applies to nonces that were never requested; if the client sent a nonce, absence is a validation failure. REPROCESS.

**Phrasing-B:** Client sends nonce in auth request. Server doesn't include nonce in ID token. Client silently passes the absent nonce as "not applicable." Team: "Handle absence gracefully." Is this acceptable?

---

## Pair[CB4-3.4-512]

**A:** PAR required plus CWE-306 auth bypass review. PAR required for all clients — authorization endpoint accepts only request_uri. Additionally: security audit finds that the `/admin/metrics` endpoint returns system metrics without any authentication. Team: "Metrics are read-only — they don't change system state, so authentication is not required."

**B:**
```
Arbiter: ASVS-V10-OAuth + CWE-Authn

T-oauth-oidc:
  PAR mandatory: authorization endpoint accepts only request_uri ✓
  Authorization parameters delivered via back-channel ✓ → PASS

T-cwe-authn:
  CWE-306 (Missing Authentication for Critical Function):
    /admin/metrics: no authentication required ✗
    System metrics: server resource usage, request rates, error rates, user activity patterns
    Information disclosure: internal topology, performance characteristics, anomaly signals
    Read-only ≠ safe without auth: information disclosure is a distinct attack surface
  Team argument: "read-only = no authentication required" → REJECTED
    CWE-306 applies regardless of whether the operation is write vs. read
    Sensitive information exposure through unauthenticated read endpoints is a finding
    → REPROCESS (authentication absent on an endpoint exposing sensitive operational data)

Multi-source analysis:
  PAR PASS + CWE-306 REPROCESS → REPROCESS overrides

T-oauth-oidc verdict: PASS
T-cwe-authn verdict: REPROCESS
Overall verdict: REPROCESS
```

**Reason:** PAR mandatory is correct (PASS). `/admin/metrics` with no authentication exposes operational data — CWE-306 regardless of read-only status. "Read-only = no auth needed" is rejected: information disclosure through unauthenticated read endpoints is a CWE-306 finding. REPROCESS overrides PASS.

**Phrasing-B:** PAR mandatory (PASS). Admin metrics endpoint with no authentication — team says "read-only doesn't need auth." What is the multi-source verdict?

---

## Pair[CB4-3.4-513]

**A:** Three-CONCERN ext19 scenario. Application: (1) PAR optional for public clients. (2) Token endpoint rate limiting: present but applies only to IP address (not per-client-id, not per-authorization-code). (3) OIDC nonce: value match validation present, single-use NOT enforced (nonce can be reused). All other controls correct. Team: "We have rate limiting, we have nonce validation — the security controls are in place."

**B:**
```
Arbiter: ASVS-V10-OAuth + ASVS-V8-AuthZ

T-oauth-oidc:
  PAR optional for public clients ✗ → CONCERN (improvement available; not required)
  OIDC nonce: value match ✓; single-use NOT enforced ✗
    Nonce replay: attacker captures ID token + extracts nonce; later replays token with same nonce
    Application accepts: nonce matches stored value ✓ — but the same nonce is reusable
    → CONCERN (partial nonce validation; single-use property absent; replay possible for captured tokens
      within validity window)

T-asvs-authentication:
  Rate limiting IP-only: per-IP limits applied ✓
    Per-client-id: not applied → cloud/shared-IP scenario: multiple legitimate users share IP ✓
    Stolen tokens from different clients sharing an IP → may exhaust shared-IP limit instead of
      targeting the specific client
    Gap: per-client-id not rate-limited → stolen refresh token for one client can be exchanged
      rapidly without triggering limit
    → CONCERN (IP rate limiting provides partial coverage; per-client-id gap)

Multi-source analysis:
  Three CONCERNs: PAR optional, nonce single-use absent, IP-only rate limiting
  Three CONCERNs do NOT escalate to REPROCESS — graduation rule
  Team argument: "controls are in place" → partial controls noted; deficiencies are CONCERN level

T-oauth-oidc verdict: CONCERN
T-asvs-authentication verdict: CONCERN
Overall verdict: CONCERN (three CONCERNs; graduation rule)
```

**Reason:** Three independent CONCERNs: (1) PAR optional for public clients, (2) nonce single-use not enforced (partial replay protection), (3) IP-only rate limiting (per-client-id gap). Three CONCERNs do not escalate to REPROCESS. Team argument that controls exist is noted but each partial control creates a CONCERN.

**Phrasing-B:** PAR optional (CONCERN), nonce value-match only no single-use (CONCERN), IP-only rate limiting (CONCERN). Three CONCERNs — does that become REPROCESS?

---

## Pair[CB4-3.4-514]

**A:** OIDC aud validated plus 1024-bit RSA key. OIDC integration: ID token validated including aud against client_id, nonce (value + single-use), sig, iss, exp, nbf. JWT signing key: RSA 1024-bit. Team: "We validate all claims including audience — our token security is thorough."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-JWT

T-oauth-oidc:
  OIDC token validation: aud ✓; nonce value+single-use ✓; sig ✓; iss ✓; exp ✓; nbf ✓
  All claims validated → PASS (validation completeness)

T-rfc-jwt:
  RSA 1024-bit signing key:
    RFC 7518 §3.3: RSA key size MUST be ≥ 2048 bits ✗
    1024-bit RSA: factorizable with current compute ✗
    → REPROCESS (same analysis as ext16 pattern; MUST violation)

Multi-source analysis:
  OIDC claim validation PASS + RSA 1024-bit REPROCESS
  Claim validation thoroughness does not protect against weak signing key
  A weak key allows forgery — correctly validating claims on a forged token provides no security
  Graduation: REPROCESS present → Overall REPROCESS

T-oauth-oidc verdict: PASS (validation) — masked by T-rfc-jwt
T-rfc-jwt verdict: REPROCESS
Overall verdict: REPROCESS
```

**Reason:** Complete OIDC claim validation (PASS) cannot compensate for a 1024-bit RSA signing key (REPROCESS). If the key can be broken, tokens can be forged, and all claim validation is performed against forged tokens. REPROCESS overrides PASS.

**Phrasing-B:** OIDC all claims validated including aud and nonce. JWT signed with 1024-bit RSA. Thorough claim validation — does it compensate for the weak signing key?

---

## Pair[CB4-3.4-515]

**A:** Post-remediation ext19 — all gaps closed. Previous audit found: (1) OIDC nonce presence-only → REPROCESS [remediated: value match + single-use + clearance added]. (2) OIDC aud not validated → REPROCESS [remediated: aud validated against client_id]. (3) PAR optional for SPAs → CONCERN [remediated: PAR mandatory for all clients]. (4) Token endpoint no rate limiting → CONCERN [remediated: per-client-id rate limiting with back-off added]. All prior controls already correct.

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-JWT + ASVS-V8-AuthZ

T-oauth-oidc:
  OIDC nonce: value match + single-use + clearance ✓ (REPROCESS remediated) — PASS
  OIDC aud: validated against client_id ✓ (REPROCESS remediated) — PASS
  PAR: mandatory for all clients; request_uri-only ✓ (CONCERN remediated) — PASS

T-rfc-jwt:
  All ID token claims validated ✓ — PASS
  nonce, aud, at_hash, sig, iss, exp, nbf all checked ✓ — PASS

T-asvs-authentication:
  Token endpoint rate limiting: per-client-id + exponential back-off ✓
    (CONCERN remediated) — PASS

All prior controls: PKCE S256 ✓; code-only ✓; Argon2id ✓; Vault ✓;
  dynamic registration protected ✓; introspection cache TTL+pub/sub ✓;
  minimum-scope M2M tokens ✓ — all PASS

All territories: PASS
Overall verdict: PASS
```

**Reason:** All four previously identified gaps (2 REPROCESS + 2 CONCERN) remediated with correct implementations. Post-remediation state: every territory returns PASS. Comprehensive PASS confirms remediation is complete and correct.

**Phrasing-B:** Previous audit: 2 REPROCESS (nonce, aud) and 2 CONCERN (PAR, rate limiting). All remediated. Does the post-remediation state produce a comprehensive PASS?

---

## Pair[CB4-3.4-516]

**A:** OIDC aud and nonce both correct plus Security Champion with structured checklist. OIDC: aud validated against client_id; nonce validated (value + single-use). Security Champion designated; reviews security-sensitive changes using an OWASP-based review checklist. Champion trained in secure development annually. No other gaps. Team: "We have thorough OIDC validation and structured security review."

**B:**
```
Arbiter: ASVS-V10-OAuth + SSDF-PW

T-oauth-oidc:
  aud ✓ (validated against client_id); nonce ✓ (value match + single-use) — PASS

T-ssdf-process:
  Security Champion: designated ✓ — PASS (role exists)
  OWASP-based review checklist ✓ — PASS (structured process)
  Annual training ✓ — PASS (PO.1 organizational training)
  → PASS

Multi-source analysis:
  OIDC PASS + SSDF PASS → all territories PASS
  Team argument supported by evidence: OIDC validation correct; structured champion model correct

T-oauth-oidc verdict: PASS
T-ssdf-process verdict: PASS
Overall verdict: PASS
```

**Reason:** Complete OIDC aud and nonce validation (PASS) combined with Security Champion using structured OWASP-based checklist and annual training (PASS). Both territories return PASS. Multi-source PASS.

**Phrasing-B:** OIDC aud and nonce both correctly validated. Security Champion with OWASP checklist and annual training. Does this multi-source configuration produce a PASS?

---

## File Summary

| Pair ID | Description | Stratum | Sources | Verdict |
|---------|-------------|---------|---------|---------|
| CB4-3.4-507 | OIDC nonce presence-only REPROCESS + scrypt N=1024 REPROCESS | B | V10-OAuth + V6-Auth | REPROCESS |
| CB4-3.4-508 | OIDC aud validation PASS + K8s ConfigMap credentials REPROCESS | B | V10-OAuth + SSDF-PW | REPROCESS |
| CB4-3.4-509 | PAR optional CONCERN + stateless JWT session PASS | B | V10-OAuth + V7-Session | CONCERN |
| CB4-3.4-510 | Complete OIDC validation + token endpoint rate limiting — all PASS | A | V10-OAuth + V8-AuthZ + V6-Auth | PASS |
| CB4-3.4-511 | OIDC nonce sent but server omits it; client silently passes — double gap | B/D | V10-OAuth + RFC-JWT | REPROCESS |
| CB4-3.4-512 | PAR mandatory PASS + CWE-306 admin metrics no auth REPROCESS | B | V10-OAuth + CWE-Authn | REPROCESS |
| CB4-3.4-513 | Three CONCERNs: PAR optional + nonce no single-use + IP-only rate limit | B | V10-OAuth + V8-AuthZ | CONCERN |
| CB4-3.4-514 | OIDC all claims validated PASS + 1024-bit RSA REPROCESS | B | V10-OAuth + RFC-JWT | REPROCESS |
| CB4-3.4-515 | Post-remediation: 2 REPROCESS + 2 CONCERN all closed → comprehensive PASS | A | V10-OAuth + RFC-JWT + V8-AuthZ | PASS |
| CB4-3.4-516 | OIDC aud + nonce PASS + Security Champion with checklist PASS | A | V10-OAuth + SSDF-PW | PASS |
