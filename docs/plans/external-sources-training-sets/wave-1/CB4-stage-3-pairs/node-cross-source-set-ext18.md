# CB4 Stage 3 — Cross-Source Combination Set ext18
# Sequences CB4-3.4-663 through CB4-3.4-672
# Stratum B/C/D — ext25 multi-source combinations
# Running total after this file: 1344 instances (672 base pairs)
# Format: Pair[ID] / A / B / Reason / Phrasing-B

---

## Pair[CB4-3.4-663]

**A**
```
Security audit findings:

Finding 1 — PAR: request_uri not tracked as one-time-use. Server issues request_uri
  (expires_in=60), but accepts the same URI at /authorize multiple times within
  the 60-second window.
Finding 2 — JWKS: endpoint served at http://auth.internal/jwks (plain HTTP).
  Internal-only claim from operations team.
Finding 3 — Password hashing: Argon2id, m=65536, t=3, p=4, 16-byte CSPRNG salt.
  Stored in separate credential store, not co-located with profile data.
```

**B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 (RFC 9126) / T-rfc-jwt + T-rfc-oauth2 (RFC 8414) / T-asvs-authentication (V6.2.5)
Node 2 — Translation:
  F1: PAR one-time-use enforcement
  F2: JWKS transport security
  F3: password hashing compliance
Node 3 — Plan: F1=§4.1-one-time-use; F2=RFC-8414-HTTPS; F3=V6.2.5-Argon2id
Node 4 — Processing:
  F1: request_uri reuse accepted → RFC 9126 §4.1 MUST one-time-use violated → REPROCESS
  F2: HTTP JWKS → RFC 8414 §2 MUST HTTPS violated → REPROCESS
  F3: Argon2id m=65536/t=3/p=4 + CSPRNG salt + separate store → V6.2.5 compliant → PASS
Node 5 — Application: REPROCESS × 2 + PASS × 1
Node 6 — Translation: two REPROCESS findings; overall REPROCESS
Node 7 — Arbiter: F1 and F2 co-primary REPROCESS; F3 PASS
Node 8 — Arbiter verdict:
  REPROCESS — Two simultaneous REPROCESS findings:
  F1: PAR request_uri reuse violates RFC 9126 §4.1 MUST one-time-use. Implement consumed-URI
    cache with TTL = expires_in.
  F2: JWKS served over HTTP violates RFC 8414 §2 MUST HTTPS. HTTP JWKS allows public-key
    substitution on internal network → JWT verification bypass. Serve over HTTPS.
  F3: Password hashing (Argon2id) is correctly implemented.
Node 9 — Format: REPROCESS; F1 + F2 co-primary; F3 PASS noted
```

**Reason**
Two independent REPROCESS violations (PAR one-time-use + JWKS HTTP) alongside a correct implementation (Argon2id). Demonstrates selective compliance: good password hashing does not offset REPROCESS violations in other areas.

**Phrasing-B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 / T-rfc-jwt + T-rfc-oauth2 / T-asvs-authentication
Node 4:
  F1: request_uri reuse → §4.1 MUST → REPROCESS
  F2: HTTP JWKS → RFC 8414 §2 MUST → REPROCESS
  F3: Argon2id correct → PASS
Node 8 — Arbiter: REPROCESS — F1 (§4.1) + F2 (RFC 8414 §2) co-primary. F3 correct.
  Implement consumed-URI cache; serve JWKS over HTTPS.
Node 9 — Format: REPROCESS; two violations; F3 PASS
```

---

## Pair[CB4-3.4-664]

**A**
```
Post-remediation review of prior findings:

All three findings from the prior audit remediated:
1. PAR: Redis consumed flag added. TTL = expires_in = 60. client_id bound at /par;
   validated at /authorize. Second use → 400 invalid_request.
2. JWKS: moved to https://auth.example.com/.well-known/jwks.json (TLS 1.3, Let's Encrypt).
   Internal services updated to use the new HTTPS endpoint.
3. Password hashing: unchanged (Argon2id was already correct).
```

**B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 / T-rfc-jwt + T-rfc-oauth2 / T-asvs-authentication
Node 2 — Translation: post-remediation for all three findings
Node 3 — Plan: verify each remediation satisfies original finding criterion
Node 4 — Processing:
  F1: Redis consumed flag + TTL=60 + client_id binding → RFC 9126 §4.1 PASS
  F2: HTTPS JWKS + TLS 1.3 + Let's Encrypt → RFC 8414 §2 PASS
  F3: Argon2id unchanged, was already correct → PASS
Node 5 — Application: PASS × 3
Node 6 — Translation: all remediations verified
Node 7 — Arbiter: post-remediation PASS
Node 8 — Arbiter verdict:
  PASS — All three prior findings remediated. PAR single-use enforced (Redis consumed flag,
  client_id binding). JWKS served over HTTPS with TLS 1.3. Password hashing unchanged
  and correct. No findings.
Node 9 — Format: PASS; three items confirmed resolved
```

**Reason**
Post-remediation PASS for the CB4-3.4-663 multi-finding scenario. Pairs with 663 to demonstrate before/after verification.

**Phrasing-B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 / T-rfc-jwt + T-rfc-oauth2 / T-asvs-authentication
Node 4: F1 Redis consumed flag → PASS; F2 HTTPS JWKS → PASS; F3 unchanged → PASS
Node 8 — Arbiter: PASS — All three findings resolved. No remaining issues.
Node 9 — Format: PASS; three findings confirmed resolved
```

---

## Pair[CB4-3.4-665]

**A**
```
Architecture review combining PKCE storage and mTLS findings:

Component A — SPA: PKCE code_verifier stored in sessionStorage. 43-char URL-safe
  base64, crypto.getRandomValues(). Developer: "sessionStorage > localStorage."
Component B — Backend service: mTLS client authentication. Authorization server
  accepts self-signed certificates without CA validation or pre-registration.
  Developer: "TLS handshake proves key possession."
```

**B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 (RFC 7636) / T-rfc-oauth2 (RFC 8705) + T-asvs-authentication
Node 2 — Translation:
  A: PKCE code_verifier storage method
  B: mTLS certificate validation policy
Node 3 — Plan: A=sessionStorage-concern; B=CA-validation-check
Node 4 — Processing:
  A: sessionStorage XSS-accessible → CONCERN (not MUST violation; RFC 7636 does not mandate storage)
  B: any self-signed accepted without CA/pre-registration → CWE-295 → RFC 8705 §2 → REPROCESS
  "Handshake proves key possession" → true; proves nothing about identity without binding
Node 5 — Application: CONCERN (A) + REPROCESS (B)
Node 6 — Translation: REPROCESS present → overall REPROCESS
Node 7 — Arbiter: B (mTLS CWE-295) is controlling REPROCESS finding; A secondary CONCERN
Node 8 — Arbiter verdict:
  REPROCESS — Component B (mTLS) is the controlling violation. Accepting any self-signed
  certificate without CA validation or pre-registration provides no authentication (CWE-295,
  RFC 8705 §2). Any party generates a self-signed cert with any identity. Remediate:
  configure private CA trust store or implement fingerprint pre-registration.
  Component A (PKCE sessionStorage) is a secondary CONCERN: sessionStorage is XSS-accessible;
  use a module-level variable instead.
Node 9 — Format: REPROCESS; B primary (CWE-295); A secondary CONCERN
```

**Reason**
Component B (mTLS without CA validation) is REPROCESS; Component A (sessionStorage) is CONCERN. REPROCESS governs. This is a cross-pattern combination demonstrating that a CONCERN in one component does not lower the overall verdict when another component is REPROCESS.

**Phrasing-B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 / T-rfc-oauth2 + T-asvs-authentication
Node 4:
  A: sessionStorage XSS-accessible → CONCERN
  B: any self-signed without CA/pre-reg → CWE-295 → REPROCESS
Node 8 — Arbiter: REPROCESS — mTLS (B) controls. CWE-295: any self-signed cert accepted.
  A (sessionStorage) is secondary CONCERN. Fix mTLS first; then improve PKCE storage.
Node 9 — Format: REPROCESS; B primary; A CONCERN
```

---

## Pair[CB4-3.4-666]

**A**
```
Three-CONCERN cross-source review:

Finding 1 — PKCE: code_verifier in sessionStorage (43-char CSPRNG, format correct).
Finding 2 — JWKS caching: max-age=7200 (2-hour cache) with no re-fetch on JWT
  verification failure. Key rollover window: 1 hour. If keys are rotated, resource
  servers will reject valid tokens for up to 2 hours after rotation.
Finding 3 — PAR: request_uri expires_in=600 (10 minutes). One-time-use tracked
  (Redis consumed flag). But: client_id binding NOT enforced at /authorize —
  only the request_uri value is checked, not whether it was issued to the requesting client.
```

**B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 (RFC 7636) / T-rfc-jwt (RFC 7517) / T-rfc-oauth2 (RFC 9126)
Node 2 — Translation:
  F1: PKCE storage
  F2: JWKS caching vs rollover window mismatch
  F3: PAR client_id binding at /authorize
Node 3 — Plan: F1=storage-concern; F2=cache-TTL-rollover-gap; F3=client-binding-check
Node 4 — Processing:
  F1: sessionStorage → XSS-accessible → CONCERN (not MUST violation)
  F2: 2-hour cache with no re-fetch on failure + 1-hour rollover window
      After rotation: resource servers cache old key for up to 2 hours; rollover window = 1 hour
      Tokens issued with new key will fail validation for up to 2 hours
      Combined: JWKS cache too long relative to rollover window → operational CONCERN
      Not a REPROCESS (no security violation; valid tokens temporarily rejected, not attackable)
  F3: RFC 9126 §4.1: "MUST be bound to the client that pushed the authorization request"
      client_id binding NOT enforced at /authorize → MUST violated → REPROCESS
      An attacker who obtains a valid request_uri (issued to another client) can use it
      with a different client_id → authorization code issued to wrong client → token theft
      One-time-use tracking alone is not sufficient; binding must also be checked
Node 5 — Application: CONCERN (F1) + CONCERN (F2) + REPROCESS (F3)
Node 6 — Translation: REPROCESS present → overall REPROCESS
Node 7 — Arbiter: F3 (RFC 9126 §4.1 client binding absent) is controlling REPROCESS
Node 8 — Arbiter verdict:
  REPROCESS — F3 (PAR client_id binding at /authorize absent) is the controlling violation.
  RFC 9126 §4.1 MUST bind request_uri to the issuing client. An attacker who obtains a
  request_uri can present it with a different client_id to receive an authorization code
  issued to the attacker's session. One-time-use alone does not satisfy §4.1.
  F1 (PKCE sessionStorage): secondary CONCERN.
  F2 (JWKS cache vs rollover): secondary CONCERN — cache exceeds rollover window by 2×,
    causing valid tokens to fail verification for up to 2 hours post-rotation.
  Remediate: validate client_id at /authorize against the client_id recorded at PAR push.
Node 9 — Format: REPROCESS; F3 primary (RFC 9126 §4.1 binding); F1/F2 secondary CONCERNs
```

**Reason**
F3 is REPROCESS: RFC 9126 §4.1 says BOTH one-time-use AND client binding. One without the other is a MUST violation. F1 is CONCERN (sessionStorage). F2 is CONCERN (JWKS cache > rollover window = operational disruption, not security violation). REPROCESS governs due to F3. This pattern teaches the model that RFC 9126 §4.1 has two separate MUST conditions.

**Phrasing-B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 (RFC 7636) / T-rfc-jwt / T-rfc-oauth2 (RFC 9126)
Node 4:
  F1: sessionStorage XSS-accessible → CONCERN
  F2: 2-hour cache + 1-hour rollover → tokens rejected for ~2h post-rotation → CONCERN
  F3: client_id not bound at /authorize → §4.1 MUST violated → REPROCESS
      Attacker with stolen request_uri uses own client_id → code issued to attacker
Node 8 — Arbiter: REPROCESS — F3 controls (RFC 9126 §4.1 client binding absent).
  F1 + F2 are secondary CONCERNs. Validate client_id at /authorize against PAR record.
Node 9 — Format: REPROCESS; F3 primary; F1/F2 secondary
```

---

## Pair[CB4-3.4-667]

**A**
```
Developer argument about JWKS public-key exposure:

"Our JWKS endpoint exposes both public and private keys. The private key is included
in the JWKS as an RSA JWK with 'd', 'p', 'q', 'dp', 'dq', 'qi' parameters.
We do this so resource servers can also sign tokens when needed — it's more flexible.
Developer: it's still JWT — the format is the same whether you include private key params or not."
```

**B**
```
REPROCESS

Node 1 — Domain split: T-rfc-jwt (RFC 7517) + T-asvs-token
Node 2 — Translation: JWKS content — private key exposure in public endpoint
Node 3 — Plan: RFC 7517 public vs private key distinction; assess exposure
Node 4 — Processing:
  RFC 7517 §4: JWK Set for public distribution contains public key parameters ONLY
    For RSA: "n" (modulus) and "e" (exponent) are public key params
    "d", "p", "q", "dp", "dq", "qi" are private key params — MUST NOT be distributed publicly
  Including RSA private key params in a publicly accessible JWKS endpoint:
    Any caller with HTTPS access to the JWKS endpoint obtains the private key
    With the private key: anyone can sign arbitrary JWTs that will pass signature verification
    → complete compromise of all JWT-based authentication and authorization
  Developer claim: "resource servers need to sign tokens"
    Resource servers are VERIFIERS, not signers — they verify tokens issued by the AS
    The authorization server signs tokens using its private key (kept secret)
    Resource servers use the PUBLIC key (from JWKS) to verify signatures
    Resource servers should NEVER hold the AS private signing key
  Developer claim: "JWT format is the same" — TRUE at the wire level
    Irrelevant: the JWK 'd' parameter IS the private key, regardless of format consistency
  Complete authentication bypass: private key in JWKS = any party can forge tokens
Node 5 — Application: REPROCESS (private key in public JWKS = complete auth bypass)
Node 6 — Translation: private key exposure via JWKS; RFC 7517 fundamental violation
Node 7 — Arbiter: T-rfc-jwt + T-asvs-token — private key in public JWKS
Node 8 — Arbiter verdict:
  REPROCESS — Private RSA key parameters (d, p, q, dp, dq, qi) MUST NOT be included in
  a publicly accessible JWKS endpoint. Any caller obtains the signing private key and can
  forge arbitrary JWTs that pass all signature verification. This is a complete compromise
  of JWT-based authentication. Resource servers are verifiers, not signers — they use
  only the public key (n, e) from JWKS. Remove private key parameters from the JWKS
  endpoint immediately; rotate all affected signing keys; invalidate all outstanding tokens.
Node 9 — Format: REPROCESS; private key in public JWKS; complete auth bypass
```

**Reason**
Private key parameters in a public JWKS endpoint is a catastrophic REPROCESS — complete forgery capability. The developer arguments ("resource servers need to sign" and "JWT format is the same") are both independently wrong and rejected. This introduces a new high-severity pattern: private key exposure via JWKS.

**Phrasing-B**
```
REPROCESS

Node 1 — Domain split: T-rfc-jwt / T-asvs-token
Node 4:
  RFC 7517: JWKS public distribution contains public key params only
  'd', 'p', 'q', etc. are private key params → MUST NOT in public JWKS
  Private key in JWKS = any caller can forge arbitrary JWTs → complete auth bypass
  "Resource servers sign tokens" → REJECTED (RS = verifiers; AS = signer)
  "JWT format same" → irrelevant; 'd' param is the private key regardless
Node 8 — Arbiter: REPROCESS — Private key in public JWKS enables arbitrary JWT forgery.
  Complete authentication compromise. Remove private params; rotate keys; invalidate tokens.
Node 9 — Format: REPROCESS; complete auth bypass; both developer arguments rejected
```

---

## Pair[CB4-3.4-668]

**A**
```
mTLS deployment review:

Scenario A: API service authenticates to authorization server using mTLS.
  Client certificate: issued by a private CA operated by the organization.
  Authorization server: CA certificate in trust store. Validates certificate chain
  to the CA. Does NOT additionally pre-register certificate fingerprints.

Scenario B: Another service uses a certificate from a public CA (DigiCert).
  Authorization server accepts certificates from this public CA in addition
  to the private CA. All DigiCert-issued certificates are trusted.
```

**B**
```
CONCERN

Node 1 — Domain split: T-rfc-oauth2 (RFC 8705) + T-asvs-authentication
Node 2 — Translation: mTLS certificate validation — two scenarios
Node 3 — Plan: A=private-CA-only; B=public-CA-scope
Node 4 — Processing:
  Scenario A: private CA + chain validation → RFC 8705 §2 one condition satisfied (CA chain)
    No fingerprint pre-registration: RFC 8705 does not require both; CA chain alone satisfies §2
    However: without fingerprint pre-registration, ANY certificate issued by the org's private CA
    can authenticate — if the CA issues certificates to multiple parties or is compromised,
    any CA-issued cert can authenticate as any client
    This is the minimum compliant approach; not a REPROCESS, but defense-in-depth gap → CONCERN
  Scenario B: all DigiCert certificates trusted
    DigiCert issues certificates to thousands of organizations worldwide
    Trusting ALL DigiCert-issued certs = any organization with a DigiCert certificate
    can authenticate to the authorization server → the trust boundary is extremely broad
    This is not the intended use of mTLS client authentication with a public CA
    RFC 8705 §2 intent: certificates are bound to specific registered clients
    Trusting a public CA without additional client-specific restriction is a CONCERN:
    the trust boundary is much wider than intended
  Neither scenario is a REPROCESS: chain validation is present in both
  But both have scope-of-trust concerns → combined CONCERN verdict
Node 5 — Application: CONCERN × 2
Node 6 — Translation: CONCERN — scope of trust too broad in both scenarios
Node 7 — Arbiter: T-rfc-oauth2 — mTLS trust scope
Node 8 — Arbiter verdict:
  CONCERN — Two trust-scope issues. Scenario A: CA chain validation is RFC 8705 §2 compliant,
  but without fingerprint pre-registration, any certificate from the org's private CA can
  authenticate. If the private CA issues broadly or is compromised, the protection collapses.
  Add fingerprint pre-registration as defense in depth. Scenario B: trusting all DigiCert-
  issued certificates creates an extremely broad trust boundary — thousands of external
  organizations can authenticate. Restrict public CA acceptance to specific pre-registered
  fingerprints or use a private CA for internal services.
Node 9 — Format: CONCERN; two scenarios; defense-in-depth gap and broad public CA trust
```

**Reason**
Both scenarios have CA chain validation (RFC 8705 §2 minimum), making neither a REPROCESS. But both have trust-scope CONCERNs: Scenario A lacks fingerprint pre-registration (any CA-issued cert authenticates), Scenario B trusts all DigiCert certs (extremely broad). This is the Stratum B nuanced trust-scope analysis for mTLS.

**Phrasing-B**
```
CONCERN

Node 1 — Domain split: T-rfc-oauth2 (RFC 8705)
Node 4:
  A: private CA chain → RFC 8705 §2 compliant; no fingerprint pre-reg → any CA-issued cert authenticates → CONCERN
  B: all DigiCert trusted → thousands of external orgs can authenticate → trust boundary too broad → CONCERN
  Both have CA chain validation → not REPROCESS; trust-scope gaps → CONCERN
Node 8 — Arbiter: CONCERN — A: add fingerprint pre-registration (defense in depth).
  B: restrict public CA trust to specific pre-registered fingerprints.
Node 9 — Format: CONCERN; two trust-scope issues
```

---

## Pair[CB4-3.4-669]

**A**
```
Security review — three-finding mixed verdict:

Finding 1 — PAR: correctly implemented. Request_uri consumed flag (Redis, TTL=60).
  client_id binding at /par and /authorize. Authenticated PAR endpoint.
Finding 2 — JWKS: https://auth.example.com/.well-known/jwks.json, TLS 1.3, trusted CA.
  Public keys only. max-age=3600, re-fetch on verification failure.
Finding 3 — SSDF PO.3 SBOM: dependency manifest auto-generated in CI/CD (CycloneDX).
  CVE scanning: daily automated scan with 24-hour alert SLA. Transitive dependencies
  included. Last SBOM generated: 6 hours ago.
```

**B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 (RFC 9126) / T-rfc-jwt + T-rfc-oauth2 / T-ssdf-process (PO.3)
Node 2 — Translation:
  F1: PAR implementation
  F2: JWKS configuration
  F3: SBOM process
Node 3 — Plan: verify each finding
Node 4 — Processing:
  F1: Redis consumed flag + client_id binding at push+use + authenticated PAR endpoint
      → RFC 9126 §4.1 PASS (all three controls)
  F2: HTTPS + TLS 1.3 + trusted CA + public-only + rotation-aware cache → RFC 8414 + RFC 7517 PASS
  F3: CycloneDX auto-generated in CI/CD + daily CVE scan + 24h SLA + transitive deps → PASS
      Compare to manual/monthly SBOM (CONCERN): automated daily with transitive coverage
      and a defined SLA is the correct pattern (established in ext22)
Node 5 — Application: PASS × 3
Node 6 — Translation: all three findings correct
Node 7 — Arbiter: comprehensive PASS
Node 8 — Arbiter verdict:
  PASS — All three findings are correctly implemented. PAR: RFC 9126 §4.1 fully satisfied
  (single-use + client binding + authenticated endpoint). JWKS: HTTPS, TLS 1.3, trusted CA,
  public-only, rotation-aware caching. SBOM: automated CycloneDX with daily CVE scanning
  and 24-hour SLA. No findings.
Node 9 — Format: PASS; three findings confirmed
```

**Reason**
Multi-source PASS: PAR (RFC 9126), JWKS (RFC 8414/7517), SBOM (SSDF PO.3). All three are correct. The SBOM is explicitly confirmed against the ext22 automated baseline (CycloneDX + transitive + automated daily scan = PASS; manual/monthly = CONCERN).

**Phrasing-B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 / T-rfc-jwt / T-ssdf-process
Node 4:
  F1: Redis consumed flag + client_id binding → §4.1 PASS
  F2: HTTPS + TLS 1.3 + trusted CA + rotation-aware → RFC 8414 PASS
  F3: CycloneDX + daily CVE scan + 24h SLA + transitive → PO.3 PASS
Node 8 — Arbiter: PASS — PAR, JWKS, SBOM all correct. No findings.
Node 9 — Format: PASS; three findings confirmed
```

---

## Pair[CB4-3.4-670]

**A**
```
Security argument about PKCE verifier and authorization code binding:

Developer: "In our implementation, the PKCE code_verifier is stored in the database
associated with the session. When the token exchange happens, we retrieve the
stored code_verifier and compare it against the SHA256 hash. The code_verifier is
stored server-side, not in the browser. This means XSS cannot exfiltrate it."
```

**B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 (RFC 7636)
Node 2 — Translation: PKCE code_verifier storage — server-side database
Node 3 — Plan: assess server-side storage vs browser-side; XSS exfiltration analysis
Node 4 — Processing:
  Server-side database storage of code_verifier:
    Not accessible from browser JavaScript → XSS cannot read it → XSS exfiltration eliminated
    code_verifier stored in session table, associated with the session/state parameter
    Token exchange: server retrieves code_verifier from DB, computes SHA256, compares to code_challenge
    This is a valid server-side PKCE implementation pattern
  Comparison: browser-side (sessionStorage/localStorage → XSS-accessible → CONCERN)
    vs server-side (database → not browser-accessible → PASS)
  code_verifier format: not explicitly stated in input; assuming RFC 7636 §4.1 compliance
    (43-128 char, CSPRNG, URL-safe base64) — developer describes "compare against SHA256 hash" correctly
  No XSS exfiltration pathway for server-side storage → addresses the sessionStorage CONCERN
Node 5 — Application: PASS (server-side storage eliminates browser-side XSS risk)
Node 6 — Translation: correct PKCE implementation with server-side verifier storage
Node 7 — Arbiter: T-rfc-oauth2 — PKCE server-side verifier storage
Node 8 — Arbiter verdict:
  PASS — Server-side database storage for the PKCE code_verifier is a correct
  implementation pattern. The verifier is not accessible from browser JavaScript,
  eliminating the XSS exfiltration risk present with sessionStorage/localStorage.
  Server-side SHA256 comparison on token exchange is the correct verification procedure.
  No findings.
Node 9 — Format: PASS; server-side storage pattern confirmed; XSS risk eliminated
```

**Reason**
Server-side PKCE verifier storage is PASS — it moves the verifier out of JavaScript-accessible storage entirely. This completes the PKCE storage spectrum: sessionStorage = CONCERN, module variable = PASS (with caveats), server-side DB = PASS (cleanest). The model must recognize server-side as the correct alternative.

**Phrasing-B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 (RFC 7636)
Node 4:
  Server-side DB storage → not browser-accessible → XSS cannot read it
  Token exchange retrieves from DB → SHA256 comparison → correct PKCE procedure
  Eliminates the sessionStorage XSS-exfiltration CONCERN entirely
Node 8 — Arbiter: PASS — Server-side code_verifier storage eliminates XSS exfiltration risk.
  Correct PKCE implementation. No findings.
Node 9 — Format: PASS; server-side pattern confirmed
```

---

## Pair[CB4-3.4-671]

**A**
```
Multi-source review: PAR + mTLS + error response:

Finding 1 — PAR: request_uri expires_in=30. No one-time-use tracking.
  Developer: "30-second window is impractical to exploit."
Finding 2 — mTLS: self-signed certificates accepted if the client pre-registers the
  certificate fingerprint (SHA-256) at registration time. No CA validation required.
  Authorization server validates: fingerprint must match pre-registered value.
Finding 3 — Error response: {"error": "unauthorized_client", "error_description":
  "Your client is not authorized for this grant type."}
```

**B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 (RFC 9126) / T-rfc-oauth2 (RFC 8705) / T-rfc-oauth2 (RFC 6749)
Node 2 — Translation: PAR one-time-use / mTLS certificate binding / error response disclosure
Node 3 — Plan: F1=§4.1-one-time-use; F2=fingerprint-pre-registration; F3=error-content
Node 4 — Processing:
  F1: no one-time-use tracking; "30-second impractical" → REJECTED
      RFC 9126 §4.1 MUST one-time-use; no risk-based exception
      Pre-positioned attacker with active IdP session completes in ~2-5 seconds → REPROCESS
  F2: self-signed + fingerprint pre-registration → RFC 8705 §2 second condition satisfied
      RFC 8705 allows self-signed certificates IF they are explicitly pre-registered
      Fingerprint (SHA-256) binding = the certificate is cryptographically bound to the registered client
      This is the correct self-signed mTLS pattern → PASS
  F3: "unauthorized_client" + "not authorized for this grant type" → no enumeration,
      no registry data, no IP; guidance-only error_description → PASS
Node 5 — Application: REPROCESS (F1) + PASS (F2) + PASS (F3)
Node 6 — Translation: REPROCESS present → overall REPROCESS
Node 7 — Arbiter: F1 (RFC 9126 §4.1) is controlling finding; F2/F3 PASS
Node 8 — Arbiter verdict:
  REPROCESS — PAR request_uri lacks one-time-use tracking, violating RFC 9126 §4.1 MUST.
  The 30-second argument is rejected: (1) MUST is unconditional; (2) pre-positioned attackers
  can complete authorization in seconds. mTLS via self-signed certificate with fingerprint
  pre-registration is correctly implemented (RFC 8705 §2). Error response is appropriately
  scoped with no disclosure. Primary remediation: add consumed-URI cache (TTL = expires_in).
Node 9 — Format: REPROCESS; F1 primary; F2/F3 PASS noted
```

**Reason**
F1 is REPROCESS (PAR without one-time-use). F2 is PASS: the self-signed + fingerprint pre-registration pattern is the correct RFC 8705 alternative to CA chain — the key distinction from CB4-3.4-652 (which had NO fingerprint pre-registration). F3 is PASS (guidance-only error description). This teaches the distinction between "self-signed without pre-registration = REPROCESS" and "self-signed with fingerprint pre-registration = PASS."

**Phrasing-B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 × 3
Node 4:
  F1: no one-time-use; "30-second" rejected → §4.1 MUST → REPROCESS
  F2: self-signed + fingerprint pre-registration → RFC 8705 §2 second condition → PASS
  F3: grant type denial message, no enumeration → PASS
Node 8 — Arbiter: REPROCESS — F1 controls (§4.1 MUST). F2 self-signed fingerprint
  pre-registration is correct; F3 error response acceptable. Fix PAR one-time-use.
Node 9 — Format: REPROCESS; F1 primary; F2/F3 PASS
```

---

## Pair[CB4-3.4-672]

**A**
```
Comprehensive post-remediation PASS — all ext25 patterns:

1. PAR: Redis consumed flag, TTL=expires_in, client_id binding at /par and /authorize.
   Authenticated PAR endpoint. expires_in=90 seconds.
2. JWKS: https endpoint, TLS 1.3, trusted CA, public keys only, 1-hour cache + re-fetch
   on verification failure.
3. PKCE: code_verifier generated server-side, stored in database with session association,
   retrieved and compared during token exchange. 43-char CSPRNG URL-safe base64.
4. mTLS: private CA trust store + fingerprint pre-registration (defense in depth).
   OR: self-signed + SHA-256 fingerprint whitelist (pre-registered at client registration).
5. Error responses: error code only, or error code + guidance-only description.
   No enumeration of valid identifiers.
```

**B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 / T-rfc-jwt / T-rfc-oauth2 / T-rfc-oauth2 / T-rfc-oauth2
Node 2 — Translation: all five ext25 domain patterns
Node 3 — Plan: verify each component against applicable RFC
Node 4 — Processing:
  PAR: §4.1 three controls (consumed flag + client_id binding + authenticated endpoint) → PASS
  JWKS: RFC 8414 §2 HTTPS + TLS 1.3 + trusted CA + public-only + rotation-aware → PASS
  PKCE: server-side DB storage + 43-char CSPRNG → XSS-safe; RFC 7636 §4.1 → PASS
  mTLS: private CA + fingerprint (defense in depth) OR self-signed + fingerprint whitelist
    → either satisfies RFC 8705 §2; both patterns here satisfy → PASS
  Error responses: code only or guidance-only description → no enumeration → RFC 6749 §5.2 → PASS
Node 5 — Application: PASS × 5
Node 6 — Translation: all ext25 patterns correct
Node 7 — Arbiter: comprehensive ext25 + RFC 6749 PASS
Node 8 — Arbiter verdict:
  PASS — All five patterns correctly implemented. PAR: RFC 9126 §4.1 single-use +
  client binding. JWKS: RFC 8414 HTTPS + trusted CA + public-only + rotation-aware.
  PKCE: server-side verifier storage (XSS-safe) + RFC 7636-compliant format. mTLS:
  CA chain and/or fingerprint pre-registration per RFC 8705 §2. Error responses:
  minimum disclosure per RFC 6749 §5.2. No findings.
Node 9 — Format: PASS; five patterns confirmed; no findings
```

**Reason**
Comprehensive post-remediation PASS covering all ext25 patterns plus the error response baseline from ext24. The mTLS entry lists both acceptable patterns (CA + fingerprint, and self-signed + fingerprint whitelist) to confirm both are PASS. This closes the ext25 cross-source set.

**Phrasing-B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 × 5
Node 4:
  PAR: §4.1 three controls → PASS
  JWKS: RFC 8414 + RFC 7517 → PASS
  PKCE: server-side DB → XSS-safe → PASS
  mTLS: CA chain + fingerprint OR self-signed + fingerprint → RFC 8705 §2 → PASS
  Errors: code only or guidance-only → RFC 6749 §5.2 → PASS
Node 8 — Arbiter: PASS — All five ext25 patterns correct. No findings.
Node 9 — Format: PASS; five patterns confirmed
```
