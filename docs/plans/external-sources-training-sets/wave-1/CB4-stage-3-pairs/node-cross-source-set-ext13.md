# CB4 Stage 3 — Node Cross-Source Set Extension 13
# Sequences CB4-3.4-533 through CB4-3.4-542
# Stratum: B/C/D — multi-source combinations for ext20 patterns
# Instances: 20 (10 base pairs × 2 phrasings)
# Running total after this file: 1084 instances

---

## Pair[CB4-3.4-533]

**A:** Hybrid flow plus K8s Secret without etcd encryption. Application: OIDC hybrid flow (`response_type=code id_token`). Kubernetes deployment: JWT signing key stored in a Kubernetes Secret, but etcd encryption at rest is not enabled. Team: "K8s Secrets are the right resource type for credentials — we're using the platform correctly."

**B:**
```
Arbiter: ASVS-V10-OAuth + SSDF-PW

T-oauth-oidc:
  response_type=code id_token: ID token in front-channel URL fragment ✗
  OAuth 2.0 BCP restricts front-channel token delivery ✗
  → REPROCESS

T-ssdf-process:
  Kubernetes Secret without etcd encryption at rest:
    K8s Secret is the correct resource type ✓ (better than ConfigMap)
    Without etcd encryption: secrets stored as base64 in etcd — any etcd reader sees value ✗
    → CONCERN (correct resource type; encryption-at-rest gap — same analysis as ext14 K8s pattern)
  Team argument: "correct resource type" → correct; but encoding ≠ encryption → CONCERN not dismissed

Multi-source analysis:
  Hybrid flow REPROCESS + K8s Secret no-etcd-encryption CONCERN
  Graduation: REPROCESS present → Overall REPROCESS

T-oauth-oidc verdict: REPROCESS
T-ssdf-process verdict: CONCERN
Overall verdict: REPROCESS
```

**Reason:** Hybrid flow (REPROCESS) + K8s Secret without etcd encryption (CONCERN). REPROCESS overrides CONCERN. K8s Secret is the correct resource type (CONCERN, not REPROCESS — unlike ConfigMap), but the signing key is still accessible without etcd encryption.

**Phrasing-B:** OIDC hybrid flow (REPROCESS). JWT signing key in K8s Secret, no etcd encryption (CONCERN). What is the multi-source verdict?

---

## Pair[CB4-3.4-534]

**A:** State entropy gap plus token endpoint HTTP. Authorization server: OAuth state parameter is a 10-character alphanumeric string (~59 bits). Token endpoint configured to accept HTTP in addition to HTTPS (for "development compatibility"). Team: "59 bits of state is more than sufficient; HTTP is only for dev environments."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2

T-oauth-oidc:
  State 10-char alphanumeric: 62^10 ≈ 8.4 × 10^17 combinations = ~59 bits entropy
    Below 128-bit minimum ✗ → CONCERN

T-rfc-oauth2:
  Token endpoint accepts HTTP:
    RFC 6749 §3.2: token endpoint MUST use TLS ✗
    "Development compatibility" argument: HTTP in production violates unconditional MUST
    "Only for dev" argument: if the endpoint is reachable via HTTP, the MUST is violated
    regardless of intent → REPROCESS

Multi-source analysis:
  State CONCERN + token endpoint HTTP REPROCESS
  Team arguments: "59 bits sufficient" → below 128-bit min, CONCERN;
    "HTTP only for dev" → unconditional MUST violation regardless of intent
  Graduation: REPROCESS present → Overall REPROCESS

T-oauth-oidc verdict: CONCERN
T-rfc-oauth2 verdict: REPROCESS
Overall verdict: REPROCESS
```

**Reason:** Token endpoint HTTP is REPROCESS (RFC 6749 §3.2 unconditional MUST — no dev-compatibility exception). State entropy at 59 bits is CONCERN (below 128-bit minimum). REPROCESS overrides CONCERN.

**Phrasing-B:** State 10-char alphanumeric (CONCERN). Token endpoint accepts HTTP for dev compatibility (REPROCESS). What is the overall verdict?

---

## Pair[CB4-3.4-535]

**A:** JWT PII in payload plus Argon2id correct. Application: JWT access tokens contain `email` and `name` claims. JWTs stored in sessionStorage (JavaScript-accessible but cleared on tab close). Password hashing: Argon2id with recommended parameters. Team: "We use Argon2id which is best-in-class password security."

**B:**
```
Arbiter: ASVS-V9-Token + ASVS-V6-Auth

T-asvs-token:
  JWT payload: email, name — PII ✗
  sessionStorage: JavaScript-accessible (same as localStorage for XSS purposes) ✗
    XSS reads sessionStorage just as easily as localStorage ✗
    sessionStorage clears on tab close ✓ — marginally better persistence than localStorage
    But XSS within session window: PII exposed ✗
  JWT not encrypted (JWE absent) ✗
  → REPROCESS (PII in unencrypted JWT in JS-accessible storage)

T-asvs-authentication:
  Argon2id with recommended parameters ✓ — PASS

Multi-source analysis:
  JWT PII REPROCESS + Argon2id PASS
  Argon2id correctness addresses password storage; does not protect JWT PII
  Graduation: REPROCESS present → Overall REPROCESS

T-asvs-token verdict: REPROCESS
T-asvs-authentication verdict: PASS
Overall verdict: REPROCESS
```

**Reason:** PII in unencrypted JWT in sessionStorage (XSS-accessible) is REPROCESS. Argon2id is correct (PASS) but addresses password storage — independent finding that doesn't protect JWT content from XSS. REPROCESS overrides PASS.

**Phrasing-B:** JWT with email and name in payload, stored in sessionStorage. Argon2id correct. Does Argon2id correctness compensate for PII in JWT?

---

## Pair[CB4-3.4-536]

**A:** Code-only OIDC plus DPoP plus aud — fully correct. OIDC: response_type=code, PAR mandatory. DPoP: enforced on all API requests, full proof validation. aud: validated at resource server against server URL. JWT: no PII, signed RS256 2048-bit, httpOnly Secure cookie. OIDC nonce: value match + single-use. All prior controls correct.

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-JWT + RFC-OAuth2 + ASVS-V9-Token

T-oauth-oidc:
  response_type=code ✓; PAR mandatory ✓; DPoP full proof ✓; OIDC nonce ✓ — PASS

T-rfc-jwt:
  aud validated ✓; RS256 2048-bit ✓; no PII in payload ✓ — PASS

T-asvs-token:
  JWT no PII ✓; httpOnly Secure SameSite ✓; JWE not required ✓ — PASS

T-rfc-oauth2:
  DPoP (RFC 9449) full ✓; prior OAuth controls ✓ — PASS

All territories: PASS
Overall verdict: PASS
```

**Reason:** All ext20 patterns correct plus prior controls: code-only OIDC, PAR mandatory, DPoP full proof, aud validated, no-PII JWT in httpOnly cookie, correct nonce. Multi-source comprehensive PASS.

**Phrasing-B:** Code-only OIDC, PAR mandatory, DPoP full, aud validated, no-PII JWT in httpOnly cookie, nonce correct. Multi-source PASS?

---

## Pair[CB4-3.4-537]

**A:** State low entropy plus implicit flow. Application: state parameter is a 5-digit integer (0–99999, ~17 bits). Additionally: authorization server still supports `response_type=token` (implicit flow) for legacy client compatibility. Team: "State prevents CSRF, and implicit flow is only for legacy clients that can't be updated."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2

T-oauth-oidc:
  response_type=token (implicit flow): access token in front-channel URL fragment ✗
    OAuth 2.0 BCP §2.1.2: implicit flow deprecated → REPROCESS
    "Legacy clients that can't be updated" does not provide ASVS exemption ✗
    Planning migration: if implicit flow is currently active → REPROCESS regardless of migration plans

T-rfc-oauth2:
  State parameter 5-digit integer:
    Range: 0–99999 = 100,000 values ≈ 17 bits entropy ✗
    Sequential/predictable integers: attacker may guess current value ✗
    "State prevents CSRF" → presence is correct; entropy is critically low → CONCERN

Multi-source analysis:
  Implicit flow REPROCESS + low-entropy state CONCERN
  Team arguments: "legacy clients can't be updated" → no ASVS exemption;
    "state prevents CSRF" → mechanism present; entropy gap is CONCERN
  Graduation: REPROCESS + CONCERN → REPROCESS

T-oauth-oidc verdict: REPROCESS
T-rfc-oauth2 verdict: CONCERN
Overall verdict: REPROCESS
```

**Reason:** Implicit flow still supported (REPROCESS — no legacy exception) + low-entropy state (CONCERN). Graduation: REPROCESS overrides CONCERN. "Legacy clients can't be updated" argument rejected — ASVS requires deprecation of implicit flow regardless of migration timeline.

**Phrasing-B:** response_type=token active for legacy clients. State is a 5-digit integer. REPROCESS + CONCERN — what is the verdict?

---

## Pair[CB4-3.4-538]

**A:** DPoP absent from protected resource plus correct OIDC. OIDC login: code-only flow, PKCE S256, PAR mandatory, nonce+aud validated. Protected resource API: no DPoP enforcement — any valid Bearer token accepted without proof of possession. HTTPS enforced. Token TTL: 5 minutes. Team: "Short-lived tokens with HTTPS make DPoP redundant — any stolen token expires in 5 minutes."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2

T-oauth-oidc:
  OIDC: code-only ✓; PKCE S256 ✓; PAR ✓; nonce+aud ✓ — PASS

T-rfc-oauth2:
  DPoP enforcement absent on protected resource:
    RFC 9449: DPoP is OPTIONAL, not MUST ✓
    5-minute TTL + HTTPS: mitigation for token theft ✓
    Short-lived tokens: stolen token usable for ≤5 minutes ✓
    DPoP PASS analysis: same pattern as ext17 pair CB4-3.4-444
      (DPoP absent with 15-min tokens + HTTPS → PASS; 5-minute window is even shorter)
    → PASS (DPoP optional; short TTL + HTTPS provides acceptable mitigation)

Team argument: "short TTL + HTTPS makes DPoP redundant"
  This is a valid architectural decision for risk-acceptable contexts ✓
  DPoP adds defense-in-depth but is not required for ASVS compliance

T-oauth-oidc verdict: PASS
T-rfc-oauth2 verdict: PASS
Overall verdict: PASS
```

**Reason:** DPoP is optional (RFC 9449) — not a hard MUST. 5-minute token TTL combined with HTTPS provides acceptable mitigation for token theft. OIDC login flow correct. Team argument is valid. PASS. Note: DPoP would add defense-in-depth but its absence with short-lived tokens and HTTPS is not a CONCERN.

**Phrasing-B:** OIDC correct (code-only, PAR, nonce, aud). API does not enforce DPoP — 5-minute tokens + HTTPS. Team: "Short TTL makes DPoP redundant." Is DPoP required?

---

## Pair[CB4-3.4-539]

**A:** JWT with PII plus no session invalidation on logout. Application: JWT access tokens contain `email` and `phone_number`. JWTs stored in httpOnly cookie. On logout: cookie cleared client-side. Token revocation endpoint NOT implemented — JWTs accepted until expiry (30-minute TTL). Team: "Clearing the cookie on logout ends the session — the user cannot use the token after logout."

**B:**
```
Arbiter: ASVS-V9-Token + ASVS-V10-OAuth

T-asvs-token:
  JWT payload: email, phone_number — PII ✗
  Storage: httpOnly cookie ✓ (not JS-accessible — better than localStorage)
  JWE absent: PII in unencrypted token value ✗
    Even in httpOnly cookie: if token is captured via MITM (TLS weakness), network interception,
      or server-side log of token value → PII exposed in plain token
    httpOnly prevents XSS access but does not encrypt the token payload
  → CONCERN (httpOnly mitigates XSS; PII unencrypted in token; not REPROCESS because
    httpOnly eliminates the primary attack vector of sessionStorage/localStorage)

T-oauth-oidc:
  Logout: cookie cleared client-side ✓ (client-side session terminated)
  Token revocation: absent ✗
    30-minute TTL: cookie cleared but token is still valid for up to 30 minutes
    If token was captured before logout → remains valid for residual TTL ✗
    V10.3 / RFC 7009: revocation endpoint required for effective logout ✗
    → CONCERN (revocation infrastructure absent; bounded 30-min window — same CONCERN pattern
      as introspection cache analysis but here it's absence of revocation entirely being
      bounded by short TTL)
  Note: absence of revocation with short TTL → CONCERN; with long TTL → would escalate

Multi-source analysis:
  PII in httpOnly JWT → CONCERN; revocation absent with short TTL → CONCERN
  Two CONCERNs → CONCERN (graduation rule)

T-asvs-token verdict: CONCERN
T-oauth-oidc verdict: CONCERN
Overall verdict: CONCERN
```

**Reason:** PII in JWT without JWE stored in httpOnly cookie — CONCERN (httpOnly blocks XSS, reducing severity to CONCERN from REPROCESS; but payload still unencrypted if token is captured). No revocation with 30-minute TTL — CONCERN (bounded window; same pattern as introspection cache). Two CONCERNs = CONCERN overall.

**Phrasing-B:** JWT with email and phone in payload, httpOnly cookie, no revocation endpoint, 30-minute TTL. Two CONCERNs — is the verdict CONCERN or REPROCESS?

---

## Pair[CB4-3.4-540]

**A:** OIDC hybrid flow plus Vault correct. Application: OIDC hybrid flow response_type=code id_token. All secrets (OIDC client secret, signing keys, DB credentials) stored in HashiCorp Vault with access control and audit logging. Team: "Our secret management is excellent — Vault protects all our credentials, including OIDC secrets."

**B:**
```
Arbiter: ASVS-V10-OAuth + SSDF-PW

T-oauth-oidc:
  OIDC hybrid flow: ID token in authorization URL ✗ → REPROCESS

T-ssdf-process:
  Vault: all secrets in Vault ✓; access control ✓; audit logging ✓ — PASS

Multi-source analysis:
  OIDC hybrid flow REPROCESS + Vault PASS
  Vault correctness protects credentials at rest; does not protect ID token exposed in front-channel URL
  Team argument: "excellent secret management" → Vault addresses credential storage, not response_type policy
  Graduation: REPROCESS → Overall REPROCESS

T-oauth-oidc verdict: REPROCESS
T-ssdf-process verdict: PASS
Overall verdict: REPROCESS
```

**Reason:** Vault is correct (PASS) but addresses credential storage — it does not compensate for the hybrid flow response_type finding (REPROCESS). These are independent security requirements. REPROCESS overrides PASS.

**Phrasing-B:** OIDC hybrid flow (REPROCESS). All secrets in Vault with access control (PASS). Does Vault excellence compensate for hybrid flow?

---

## Pair[CB4-3.4-541]

**A:** Three-CONCERN scenario — state, DPoP absent, rate limiting absent. Application: (1) OAuth state: 12-character alphanumeric (~71 bits — below 128-bit minimum). (2) DPoP not enforced (Bearer tokens accepted). Tokens are 15-minute TTL. (3) Token endpoint: IP-only rate limiting (no per-client-id). All other controls correct. Team: "We are close to compliant — these are minor gaps."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2 + ASVS-V8-AuthZ

T-oauth-oidc:
  State 12-char alphanumeric: 62^12 ≈ 3.2 × 10^21 = ~71 bits entropy ✗
    Below 128-bit minimum → CONCERN (significant improvement over 4-char; still below threshold)

T-rfc-oauth2:
  DPoP absent with 15-min tokens + HTTPS: → PASS (DPoP optional; same analysis as CB4-3.4-444)
  Actually: per the DPoP PASS pattern, 15-min + HTTPS = PASS for DPoP absence
  So DPoP absent is NOT a finding here — it is PASS per established pattern

T-asvs-authentication:
  IP-only rate limiting: partial defense ✓; per-client-id gap ✗ → CONCERN (same as CB4-3.4-503 analysis)

Re-analysis:
  State CONCERN + DPoP PASS + rate limiting CONCERN
  One PASS, two CONCERNs
  Two CONCERNs do NOT escalate to REPROCESS
  Graduation: 0 REPROCESS, 2 CONCERN → CONCERN

T-oauth-oidc verdict: CONCERN (state entropy)
T-rfc-oauth2 verdict: PASS (DPoP absent with short TTL + HTTPS)
T-asvs-authentication verdict: CONCERN (IP-only rate limiting)
Overall verdict: CONCERN
```

**Reason:** State entropy (71 bits, below 128) → CONCERN. DPoP absent with 15-minute tokens + HTTPS → PASS (DPoP optional, short TTL is acceptable mitigation). IP-only rate limiting → CONCERN (per-client-id gap). Two CONCERNs + one PASS = CONCERN overall. "Minor gaps" team characterization understates the state entropy issue but the graduation is still CONCERN.

**Phrasing-B:** State 12-char alphanumeric (CONCERN), DPoP absent with 15-min TTL + HTTPS (PASS by established pattern), IP-only rate limiting (CONCERN). What is the overall verdict?

---

## Pair[CB4-3.4-542]

**A:** All ext20 patterns correct plus cross-source: CWE-79 review. OIDC: code-only, PAR mandatory, nonce+aud validated. DPoP enforced. JWT: standard claims only, httpOnly Secure cookie. State: 256-bit CSPRNG. Cross-source security review: XSS (CWE-79) prevention — all output encoded; CSP (Content Security Policy) deployed restricting script-src to self + nonce; no inline scripts. Team: "XSS prevention is complete — our CSP restricts all untrusted script execution."

**B:**
```
Arbiter: ASVS-V10-OAuth + CWE-Authz (CWE-79 note)

T-oauth-oidc:
  All ext20 patterns ✓ (code-only, PAR, nonce, aud, DPoP, state 256-bit, standard-claim JWT) — PASS

T-cwe-authn:
  CWE-79 (XSS) prevention:
    Output encoding ✓ — all user-controlled output encoded before rendering ✓
    CSP: script-src 'self' + nonce-based allowed scripts ✓
      Nonce-based CSP correctly restricts untrusted inline scripts ✓
      No unsafe-inline or unsafe-eval ✓
    → PASS (XSS mitigations in place)
  Note: standard-claim JWT in httpOnly cookie benefits from XSS prevention:
    Even if XSS succeeded, httpOnly cookie not accessible to injected script ✓
    Defense-in-depth: XSS prevention + httpOnly both in place ✓

All territories: PASS
Overall verdict: PASS
```

**Reason:** All ext20 patterns correct plus XSS prevention (output encoding + nonce-based CSP). Defense-in-depth: httpOnly cookie + XSS prevention. Multi-source comprehensive PASS.

**Phrasing-B:** All ext20 controls correct. XSS prevention via output encoding + nonce CSP. Does the multi-source combination including CWE-79 prevention produce a PASS?

---

## File Summary

| Pair ID | Description | Stratum | Sources | Verdict |
|---------|-------------|---------|---------|---------|
| CB4-3.4-533 | Hybrid flow REPROCESS + K8s Secret no-etcd CONCERN | B | V10-OAuth + SSDF-PW | REPROCESS |
| CB4-3.4-534 | State 59-bit CONCERN + token endpoint HTTP REPROCESS | B | V10-OAuth + RFC-OAuth2 | REPROCESS |
| CB4-3.4-535 | JWT PII in sessionStorage REPROCESS + Argon2id PASS | B | V9-Token + V6-Auth | REPROCESS |
| CB4-3.4-536 | Code-only OIDC + DPoP full + aud + prior controls — all PASS | A | Multiple | PASS |
| CB4-3.4-537 | Implicit flow REPROCESS + 5-digit state CONCERN → REPROCESS | B/D | V10-OAuth + RFC-OAuth2 | REPROCESS |
| CB4-3.4-538 | Correct OIDC + DPoP absent 5-min TTL HTTPS → PASS (DPoP optional) | B | V10-OAuth + RFC-OAuth2 | PASS |
| CB4-3.4-539 | PII JWT httpOnly CONCERN + no revocation 30-min CONCERN → two CONCERNs | B | V9-Token + V10-OAuth | CONCERN |
| CB4-3.4-540 | Hybrid flow REPROCESS + Vault PASS — Vault doesn't compensate | B | V10-OAuth + SSDF-PW | REPROCESS |
| CB4-3.4-541 | State 71-bit CONCERN + DPoP absent PASS + IP-only rate limit CONCERN | B | V10-OAuth + V8-AuthZ | CONCERN |
| CB4-3.4-542 | All ext20 correct + CWE-79 XSS prevention — multi-source PASS | A | V10-OAuth + CWE | PASS |
