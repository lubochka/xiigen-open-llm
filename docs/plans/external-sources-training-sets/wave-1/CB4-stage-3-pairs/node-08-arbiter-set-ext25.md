# CB4 Stage 3 — Node 8 Arbiter Baseline Set ext25
# Sequences CB4-3.4-647 through CB4-3.4-654
# Stratum A/B/D — PAR-request_uri / JWKS-HTTP / PKCE-verifier-storage / mTLS-CA-validation
# Running total after this file: 1308 instances (654 base pairs)
# Format: Pair[ID] / A / B / Reason / Phrasing-B

---

## Pair[CB4-3.4-647]

**A**
```
Pushed Authorization Request (PAR) implementation review:

The authorization server implements PAR (RFC 9126). When a client pushes a request:
  POST /par
  → returns: request_uri=urn:ietf:params:oauth:request_uri:abc123, expires_in=60

The client receives the request_uri and uses it in an authorization redirect:
  GET /authorize?client_id=my-client&request_uri=urn:...:abc123

Observation: the server does not track whether a given request_uri has been used.
If the same request_uri is presented to /authorize a second time (e.g., browser back
button, network retry, or attacker replay), the server accepts it and generates
a new authorization code.
```

**B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 (RFC 9126 PAR)
Node 2 — Translation: PAR request_uri single-use enforcement — sub-phase 3.4 in scope
Node 3 — Plan: check RFC 9126 single-use requirement; check replay attack surface
Node 4 — Processing:
  RFC 9126 §4.1: "The request_uri value MUST be bound to the client that pushed
    the authorization request and MUST be one-time use."
  One-time use = server MUST track whether request_uri has been used and reject reuse
  Current implementation: request_uri reuse accepted → direct RFC 9126 §4.1 MUST violation
  Attack surface: attacker intercepts or observes the request_uri (e.g., referrer header,
    proxy log, browser history) and replays it → generates a second authorization code
    for the same pushed request → potential token issuance for attacker session
  Developer scenario "browser back button" → legitimate concern; but RFC 9126 requires
    the client to push a new PAR request rather than reuse an existing request_uri
  The one-time-use MUST requirement is unambiguous; no exception for retries
Node 5 — Application: REPROCESS (RFC 9126 §4.1 MUST one-time-use not enforced)
Node 6 — Translation: RFC 9126 §4.1 MUST violation — request_uri reuse accepted
Node 7 — Arbiter: T-rfc-oauth2 — PAR request_uri single-use enforcement
Node 8 — Arbiter verdict:
  REPROCESS — PAR request_uri is not enforced as one-time-use, violating RFC 9126 §4.1
  MUST. An attacker who observes or intercepts a request_uri can replay it to generate
  additional authorization codes. The authorization server must track used request_uri
  values (e.g., in a short-lived cache keyed by the URI, valid for expires_in seconds)
  and reject any second use. Clients requiring a retry must push a new PAR request.
Node 9 — Format: REPROCESS; RFC 9126 §4.1 MUST cited; replay attack surface explained
```

**Reason**
RFC 9126 §4.1 explicitly MUST one-time-use for request_uri. Not tracking used values is a direct MUST violation. The server must maintain a used-request_uri cache sized to the expires_in window. Browser back button and retry scenarios do not create exceptions — clients must push new PAR requests.

**Phrasing-B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 (RFC 9126)
Node 4 — Processing:
  RFC 9126 §4.1: request_uri MUST be one-time-use
  Server accepts reuse → MUST violation
  Back button / retry → not an exception; client must push new PAR request
Node 8 — Arbiter verdict:
  REPROCESS — request_uri reuse violates RFC 9126 §4.1 MUST one-time-use.
  Track used URIs in a cache for the expires_in window; reject duplicates.
Node 9 — Format: REPROCESS; §4.1 cited
```

---

## Pair[CB4-3.4-648]

**A**
```
PAR implementation review — correct:

Authorization server implements PAR per RFC 9126:
  - POST /par returns request_uri + expires_in=90
  - request_uri stored in Redis with TTL=90 seconds and used=false flag
  - On first use at /authorize: used flag set to true; subsequent uses → 400 invalid_request
  - request_uri bound to client_id at PAR time; /authorize validates client_id matches
  - expires_in on request_uri set to 90 seconds (shorter than auth code lifetime of 10 min)
  - PAR endpoint authenticated: client_id + client_secret required to push
```

**B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 (RFC 9126)
Node 2 — Translation: PAR implementation completeness check — sub-phase 3.4 in scope
Node 3 — Plan: verify single-use, client binding, expiry, PAR endpoint auth
Node 4 — Processing:
  Single-use: Redis used flag; second use → 400 → PASS (RFC 9126 §4.1 one-time-use)
  Client binding: request_uri bound to client_id at push time; /authorize validates → PASS
    RFC 9126 §4.1: "MUST be bound to the client that pushed the authorization request"
  Expiry: 90 seconds; short-lived → reduces replay window → PASS
  PAR endpoint authenticated (client credentials required) → prevents unauthenticated push → PASS
  All RFC 9126 §4.1 requirements verified
Node 5 — Application: PASS (RFC 9126 fully satisfied)
Node 6 — Translation: PAR implementation correct and complete
Node 7 — Arbiter: T-rfc-oauth2 — PAR PASS baseline
Node 8 — Arbiter verdict:
  PASS — PAR implementation satisfies RFC 9126 §4.1. request_uri is single-use (Redis
  used flag with 90-second TTL). Client binding enforced at both /par and /authorize.
  90-second expiry minimizes replay window. PAR endpoint requires authentication.
  No findings.
Node 9 — Format: PASS; RFC 9126 §4.1 all controls confirmed
```

**Reason**
Correct PAR implementation: single-use tracking (Redis used flag), client_id binding at both push and use, short expiry, and authenticated PAR endpoint. All RFC 9126 §4.1 MUST requirements met. This is the PASS baseline pairing with CB4-3.4-647.

**Phrasing-B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 (RFC 9126)
Node 4 — Processing:
  Single-use: Redis flag → second use rejected → PASS
  Client binding: request_uri bound to client_id; validated at /authorize → PASS
  90-second expiry → short replay window → PASS
  PAR endpoint authenticated → PASS
Node 8 — Arbiter verdict:
  PASS — All RFC 9126 §4.1 requirements met: single-use, client binding, short expiry,
  authenticated endpoint. No findings.
Node 9 — Format: PASS; §4.1 confirmed
```

---

## Pair[CB4-3.4-649]

**A**
```
Authorization server configuration review:

JWKS endpoint configuration:
  jwks_uri: http://auth.internal/jwks (plain HTTP, not HTTPS)
  Developer comment: "The JWKS endpoint is internal-only. External callers cannot
  reach it. Internal services use it to fetch public keys for JWT verification.
  HTTPS is unnecessary overhead on an internal network."

Authorization server metadata (RFC 8414):
  issuer: https://auth.example.com (HTTPS)
  jwks_uri: http://auth.internal/jwks (HTTP)
```

**B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 (RFC 8414 AS metadata) + T-rfc-jwt (RFC 7517 JWKS)
Node 2 — Translation: JWKS endpoint transport security — sub-phase 3.4 in scope
Node 3 — Plan: check transport security requirement for JWKS endpoint; check internal-only argument
Node 4 — Processing:
  RFC 7517 §5 and RFC 8414 §2: jwks_uri MUST use HTTPS
    RFC 8414 §2 specifies that jwks_uri in AS metadata MUST be an HTTPS URL
    RFC 7517 §5: JWK Sets used for key distribution SHOULD use HTTPS for integrity
  JWKS integrity: if JWKS is served over HTTP, an on-path attacker on the internal
    network can substitute keys (MitM attack) → all JWT signature verifications using
    the substituted public key would accept tokens signed by the attacker's private key
  Developer claim: "internal-only; HTTPS unnecessary overhead"
    Internal network ≠ trusted network: insider threat, misconfigured routing, lateral movement
    HTTP on internal network = any host on the LAN can inject malicious JWKS response
    "Internal = safe" argument rejected (same pattern as "internal network = no auth")
  RFC 8414 §2 requirement is normative: jwks_uri MUST be an HTTPS URL
  issuer is HTTPS but jwks_uri is HTTP → inconsistency; RFC 8414 §1 requires issuer HTTPS
    and §2 requires jwks_uri to be in the same trust domain as the issuer
Node 5 — Application: REPROCESS (RFC 8414 §2 MUST + RFC 7517 §5 HTTPS for JWKS)
Node 6 — Translation: RFC 8414 and RFC 7517 require HTTPS for jwks_uri
Node 7 — Arbiter: T-rfc-jwt + T-rfc-oauth2 — JWKS transport security
Node 8 — Arbiter verdict:
  REPROCESS — JWKS endpoint served over HTTP violates RFC 8414 §2 (jwks_uri MUST be
  HTTPS) and undermines JWT signature verification integrity. An on-path attacker on
  the internal network can substitute public keys, causing all resource servers to
  accept attacker-crafted tokens. The "internal network" argument is rejected: internal
  networks are not inherently trusted, and HTTP allows any host on the path to inject
  a malicious JWKS response. Serve the JWKS endpoint over HTTPS.
Node 9 — Format: REPROCESS; RFC 8414 §2 and RFC 7517 cited; internal-only argument rejected
```

**Reason**
RFC 8414 §2 requires jwks_uri to be HTTPS. HTTP JWKS allows key substitution by any on-path internal host — a complete bypass of JWT signature verification integrity. The "internal network = safe" argument follows the same rejected pattern as "internal network = no auth needed." This is a new pattern: JWKS transport (not just JWKS rotation window).

**Phrasing-B**
```
REPROCESS

Node 1 — Domain split: T-rfc-jwt (RFC 7517) + T-rfc-oauth2 (RFC 8414)
Node 4 — Processing:
  RFC 8414 §2: jwks_uri MUST be HTTPS
  HTTP JWKS → on-path MitM can substitute public keys → JWT signature verification bypass
  "Internal network = safe" → rejected (insider threat, lateral movement)
Node 8 — Arbiter verdict:
  REPROCESS — HTTP jwks_uri violates RFC 8414 §2 MUST. Key substitution via on-path
  attack on internal network bypasses all JWT signature verification. Use HTTPS.
Node 9 — Format: REPROCESS; RFC 8414 §2 cited; internal argument rejected
```

---

## Pair[CB4-3.4-650]

**A**
```
Authorization server JWKS configuration review — correct:

jwks_uri: https://auth.example.com/.well-known/jwks.json (HTTPS enforced)
TLS: TLS 1.3 with certificate from a trusted CA (Let's Encrypt, publicly trusted)
JWKS caching at resource servers: Cache-Control: max-age=3600 (1-hour cache)
  + resource servers re-fetch on JWT signature verification failure (key rotation support)
JWKS content: {"keys": [{"kty":"RSA","n":"...","e":"...","kid":"2026-q2","use":"sig"}]}
  Only the public key is served; private key never exposed via JWKS endpoint.
AS metadata (RFC 8414):
  issuer: https://auth.example.com
  jwks_uri: https://auth.example.com/.well-known/jwks.json
```

**B**
```
PASS

Node 1 — Domain split: T-rfc-jwt (RFC 7517) + T-rfc-oauth2 (RFC 8414)
Node 2 — Translation: JWKS endpoint configuration — sub-phase 3.4 in scope
Node 3 — Plan: verify HTTPS, TLS quality, caching, content (public key only), AS metadata
Node 4 — Processing:
  HTTPS enforced → RFC 8414 §2 MUST satisfied → PASS
  TLS 1.3 + trusted CA certificate → RFC 8446 compliant; certificate validates identity → PASS
  JWKS caching: 1-hour max-age with re-fetch on verification failure → supports key rotation
    without invalidating cached keys prematurely → PASS
  JWKS content: only public key (n, e, kid, use=sig) exposed → private key not in endpoint → PASS
  AS metadata: issuer and jwks_uri both HTTPS, same domain → RFC 8414 §1 + §2 satisfied → PASS
Node 5 — Application: PASS (all JWKS and AS metadata requirements met)
Node 6 — Translation: JWKS correctly configured; AS metadata complete
Node 7 — Arbiter: T-rfc-jwt + T-rfc-oauth2 — JWKS PASS baseline
Node 8 — Arbiter verdict:
  PASS — JWKS endpoint correctly configured. HTTPS enforced with TLS 1.3 and trusted
  CA certificate. Resource servers cache with 1-hour max-age and re-fetch on verification
  failure (supports key rotation). JWKS exposes only the public key. AS metadata
  correctly aligns issuer and jwks_uri (both HTTPS, same domain). No findings.
Node 9 — Format: PASS; RFC 8414 + RFC 7517 both confirmed
```

**Reason**
Correct JWKS configuration: HTTPS (RFC 8414 §2), TLS 1.3 + trusted CA (RFC 8446), appropriate caching with rotation-aware re-fetch, public key only, and AS metadata aligned. Pairs with CB4-3.4-649 as the PASS baseline for JWKS transport security.

**Phrasing-B**
```
PASS

Node 1 — Domain split: T-rfc-jwt / T-rfc-oauth2
Node 4 — Processing:
  HTTPS + TLS 1.3 + trusted CA → RFC 8414 §2 + RFC 8446 → PASS
  1-hour cache + re-fetch on failure → rotation-aware → PASS
  JWKS public key only → no private key exposure → PASS
  AS metadata aligned → PASS
Node 8 — Arbiter verdict:
  PASS — JWKS endpoint fully compliant. HTTPS, trusted CA, rotation-aware caching,
  public-only JWKS content, aligned AS metadata. No findings.
Node 9 — Format: PASS; RFC 8414 + RFC 7517 confirmed
```

---

## Pair[CB4-3.4-651]

**A**
```
SPA (Single-Page Application) OAuth 2.0 implementation review:

PKCE flow: authorization code flow with PKCE S256.
code_verifier storage: generated on each authorization request; stored in
  window.sessionStorage under key "pkce_verifier".
code_verifier format: 43-character URL-safe base64 string, generated by
  crypto.getRandomValues() (CSPRNG equivalent in browser).
Exchange: on callback, code_verifier retrieved from sessionStorage and sent
  to /token with the authorization code.

Developer: "sessionStorage is cleared on tab close, which is more secure than
localStorage. And PKCE means the code alone can't be used without the verifier."
```

**B**
```
CONCERN

Node 1 — Domain split: T-rfc-oauth2 (RFC 7636 PKCE) + T-asvs-token
Node 2 — Translation: PKCE code_verifier storage method — sub-phase 3.4 in scope
Node 3 — Plan: assess sessionStorage security; compare to alternatives; check verifier format
Node 4 — Processing:
  code_verifier format: 43-char URL-safe base64, crypto.getRandomValues() → RFC 7636 §4.1 → PASS
  sessionStorage vs localStorage security comparison:
    localStorage: persists across tabs and browser restarts → broader exposure
    sessionStorage: per-tab, cleared on tab close → less persistent than localStorage
    Both: accessible to JavaScript in the same origin → XSS vulnerability
    XSS on the same origin can read sessionStorage contents → code_verifier exfiltrated
    "PKCE means code alone can't be used without verifier" — TRUE: XSS gets BOTH
    If an attacker has XSS on the SPA, they can read the code_verifier from sessionStorage
    AND observe the authorization code from the URL → complete code+verifier pair
    PKCE provides CSRF protection and mitigates authorization code interception;
    it does not protect against XSS exfiltration of the verifier itself
  Developer claim "sessionStorage more secure than localStorage" → TRUE (persistence)
    But: both are equally accessible to XSS on same origin → primary risk unaddressed
  Alternatives: in-memory variable (not storage API) → XSS would need to execute during
    the exact short window between code_verifier generation and token exchange
    More secure than sessionStorage for XSS resilience
  Not a REPROCESS: no MUST violation for code_verifier storage location in RFC 7636
    RFC 7636 specifies the verifier format and entropy; storage method is implementation choice
    CONCERN: sessionStorage is JavaScript-accessible; XSS is a real risk vector
Node 5 — Application: CONCERN (sessionStorage JavaScript-accessible; XSS risk)
Node 6 — Translation: CONCERN — acceptable but better alternatives exist
Node 7 — Arbiter: T-rfc-oauth2 / T-asvs-token — PKCE verifier storage method
Node 8 — Arbiter verdict:
  CONCERN — PKCE code_verifier generated correctly (43-char, CSPRNG, RFC 7636 §4.1).
  The storage gap: sessionStorage is JavaScript-accessible and can be exfiltrated by
  XSS on the same origin. An XSS attack can read the code_verifier from sessionStorage
  and extract the authorization code from the URL, producing a complete code+verifier
  pair. "PKCE protects without the verifier" is correct but irrelevant when XSS yields
  both. sessionStorage is better than localStorage for persistence, but both share the
  same XSS accessibility. Consider storing code_verifier in memory (module-level
  variable) for the short window between generation and exchange.
Node 9 — Format: CONCERN; verifier format PASS; storage method CONCERN; XSS vector explained
```

**Reason**
code_verifier format is correct (RFC 7636 §4.1). The CONCERN is sessionStorage accessibility to XSS — same as localStorage for XSS, just less persistent. The developer's "PKCE means code alone can't be used" argument is correct but irrelevant when XSS yields both the code and the verifier. Not REPROCESS because RFC 7636 does not specify storage method. In-memory storage is the better practice.

**Phrasing-B**
```
CONCERN

Node 1 — Domain split: T-rfc-oauth2 (RFC 7636) / T-asvs-token
Node 4 — Processing:
  code_verifier format: 43-char CSPRNG → RFC 7636 §4.1 → PASS
  sessionStorage: JavaScript-accessible → XSS can read code_verifier
  "PKCE protects without verifier" → irrelevant when XSS yields both code and verifier
  "sessionStorage > localStorage for persistence" → true; both XSS-accessible equally
  Not REPROCESS: RFC 7636 does not mandate storage method → CONCERN
Node 8 — Arbiter verdict:
  CONCERN — code_verifier format correct; sessionStorage is XSS-accessible.
  Use in-memory variable (not storage API) for the verifier window to reduce XSS risk.
Node 9 — Format: CONCERN; verifier format PASS; storage method CONCERN
```

---

## Pair[CB4-3.4-652]

**A**
```
mTLS (Mutual TLS) implementation for OAuth client authentication review:

Configuration: RFC 8705 mTLS client authentication implemented.
Client certificates: self-signed certificates accepted without CA validation.
  The authorization server accepts any client certificate as long as the TLS
  handshake completes successfully.
Developer argument: "The client presents a certificate; the handshake proves they
have the private key. Whether it's CA-signed or self-signed doesn't matter —
the mTLS handshake is the authentication."
```

**B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 (RFC 8705) + T-asvs-authentication
Node 2 — Translation: mTLS client certificate validation — sub-phase 3.4 in scope
Node 3 — Plan: evaluate CA validation requirement; assess developer argument
Node 4 — Processing:
  mTLS authentication purpose: the client proves possession of the private key
    corresponding to a certificate that was pre-registered or is issued by a trusted CA
  Self-signed with no CA validation: any party can generate a self-signed certificate
    with any subject name → no identity binding → no authentication
  TLS handshake proves: the presenter has the private key matching the presented cert
    It does NOT prove: the cert identity is trustworthy or pre-registered
  Developer argument: "handshake proves private key possession"
    TRUE: TLS handshake proves private key possession for the presented certificate
    FALSE inference: "certificate = identity" without CA validation or pre-registration
    Without CA validation: any attacker can generate cert(cn=legitimate-service) self-signed
    and successfully complete the mTLS handshake → the server accepts it as authentication
    The handshake proves "I have a private key" not "I am who I claim to be"
  RFC 8705 §2: mTLS client authentication requires either:
    (a) PKI with CA validation (certificate chain to trusted root), or
    (b) Self-signed certificates that were explicitly pre-registered at the AS
    In either case, there is a binding between the certificate and the registered client
    "Accept any self-signed cert" satisfies neither condition → REPROCESS
  CWE-295: Improper Certificate Validation
Node 5 — Application: REPROCESS (CWE-295 + RFC 8705 — no identity binding)
Node 6 — Translation: self-signed certs without CA validation or pre-registration = no authentication
Node 7 — Arbiter: T-rfc-oauth2 + T-asvs-authentication — mTLS certificate validation
Node 8 — Arbiter verdict:
  REPROCESS — Accepting any self-signed certificate without CA validation or pre-registration
  provides no authentication. The mTLS handshake proves private key possession for the
  presented certificate — but without identity binding (CA chain or explicit registration),
  any attacker can generate a self-signed certificate with any subject name and authenticate
  successfully. CWE-295 (Improper Certificate Validation). RFC 8705 requires either CA-validated
  certificates or explicitly pre-registered self-signed certificates. Remediate: configure
  the AS to validate certificate chains against a trusted CA, or implement explicit
  certificate fingerprint pre-registration for self-signed clients.
Node 9 — Format: REPROCESS; CWE-295 + RFC 8705; developer argument rebutted
```

**Reason**
Accepting any self-signed certificate without CA validation or pre-registration is CWE-295 — the mTLS handshake proves private key possession but not identity. The developer argument "handshake proves key possession = authentication" is a partial-truth: key possession is necessary but not sufficient without identity binding. RFC 8705 requires either CA chain or explicit pre-registration.

**Phrasing-B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 (RFC 8705) / T-asvs-authentication
Node 4 — Processing:
  Self-signed + no CA validation + no pre-registration → no identity binding
  Handshake proves private key possession; does NOT prove identity without binding
  Any attacker generates self-signed cert with any subject → passes handshake → authenticated
  RFC 8705 §2: requires CA validation or explicit pre-registration
  CWE-295: Improper Certificate Validation
Node 8 — Arbiter verdict:
  REPROCESS — Any-self-signed-cert acceptance is CWE-295. Handshake proves key possession,
  not identity. Use CA-validated certs or pre-register specific certificate fingerprints.
Node 9 — Format: REPROCESS; CWE-295 + RFC 8705; argument rebutted
```

---

## Pair[CB4-3.4-653]

**A**
```
PAR request_uri lifetime argument:

Developer: "Our request_uri expires in 30 seconds (expires_in=30). We don't
enforce one-time-use because the window is so short that replay attacks are
impractical. In 30 seconds, any attacker would need to observe the request_uri,
set up a browser session, and complete the authorization — practically impossible."
```

**B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 (RFC 9126)
Node 2 — Translation: PAR request_uri one-time-use — sub-phase 3.4 in scope
Node 3 — Plan: evaluate short-expiry argument against RFC 9126 §4.1 MUST; assess practicality
Node 4 — Processing:
  RFC 9126 §4.1: "MUST be one-time use" — unqualified; no exception for short expiry
  Developer argument: "30-second window = impractical to replay"
    This is a risk-reduction argument for a MUST-level requirement
    MUST requirements do not have a "low risk = acceptable" exception
    Compare: "we don't use PKCE because our redirect URIs are exact" → rejected
    MUST means MUST; attacker risk assessment does not override normative requirements
  Practical attack analysis (additional):
    An attacker does NOT need to complete human authorization in 30 seconds
    If the attacker pre-positions (has existing IdP session), they can:
      Intercept request_uri, immediately redirect browser to /authorize?request_uri=...
      Complete authorization silently if they have an active session → within seconds
    "Set up a browser session" misunderstands the attack: attacker uses THEIR session
      to complete authorization for the request_uri that was pushed by the victim
    30 seconds is not "impractical" for a pre-positioned attacker with an active session
  Even if risk were low: RFC 9126 §4.1 MUST is not conditional on risk assessment
Node 5 — Application: REPROCESS (RFC 9126 §4.1 MUST one-time-use; risk argument rejected)
Node 6 — Translation: REPROCESS — MUST requirement; no risk-based exception
Node 7 — Arbiter: T-rfc-oauth2 — PAR one-time-use enforcement
Node 8 — Arbiter verdict:
  REPROCESS — RFC 9126 §4.1 MUST requires one-time-use for request_uri regardless of
  expiry duration. Short expiry reduces but does not eliminate replay risk, and MUST
  requirements do not have risk-based exceptions. Additionally, 30 seconds is not
  "impractical" for a pre-positioned attacker with an active IdP session who can
  complete authorization without human interaction. Implement one-time-use tracking
  (a cache of used request_uris for the expires_in window).
Node 9 — Format: REPROCESS; §4.1 MUST; risk-based argument rejected; attack scenario explained
```

**Reason**
"Short expiry substitutes for one-time-use" is a risk-reduction argument against a MUST requirement — the same rejected pattern as "low likelihood of attack = no need for control." RFC 9126 §4.1 is unconditional. Additionally, the "30 seconds is impractical" claim underestimates a pre-positioned attacker with an active IdP session.

**Phrasing-B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 (RFC 9126)
Node 4 — Processing:
  RFC 9126 §4.1: one-time-use MUST — no risk-based exception
  "30-second expiry = impractical" → REJECTED:
    (1) MUST requirements not conditional on risk; (2) pre-positioned attacker completes
    authorization in seconds with an active IdP session
Node 8 — Arbiter verdict:
  REPROCESS — Short expiry does not substitute for RFC 9126 §4.1 MUST one-time-use.
  Risk-based argument rejected. Implement used-request_uri cache.
Node 9 — Format: REPROCESS; §4.1 MUST; risk argument rejected
```

---

## Pair[CB4-3.4-654]

**A**
```
Comprehensive review — all ext25 patterns correct:

1. PAR: POST /par returns request_uri + expires_in=60. Redis tracks request_uri
   with TTL=60 and consumed flag. Second use → 400. client_id binding enforced.
2. JWKS: https://auth.example.com/.well-known/jwks.json (HTTPS, TLS 1.3, trusted CA).
   Public keys only. 1-hour cache + re-fetch on verification failure.
3. PKCE code_verifier: 43-char S256 verifier, generated by crypto.getRandomValues().
   Stored in module-level JavaScript variable (not sessionStorage/localStorage);
   variable cleared after successful token exchange.
4. mTLS: client certificates validated against a private CA. AS has trust store with
   the CA certificate. Pre-registered client certificates only (fingerprint whitelist
   in addition to CA validation).
```

**B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 / T-rfc-jwt / T-rfc-oauth2 / T-rfc-oauth2
Node 2 — Translation: all four ext25 patterns in sub-phase 3.4 scope
Node 3 — Plan: verify each component against RFC 9126 / RFC 8414 / RFC 7636 / RFC 8705
Node 4 — Processing:
  F1: PAR — Redis consumed flag + TTL + client_id binding → RFC 9126 §4.1 satisfied → PASS
  F2: JWKS — HTTPS + TLS 1.3 + trusted CA + public-only content + 1-hour cache + re-fetch → PASS
  F3: PKCE — 43-char CSPRNG verifier + module variable (not storage API) + cleared after use → PASS
      Module variable: XSS can still access JavaScript heap, but verifier is not
      persisted and is cleared after exchange — better than sessionStorage
  F4: mTLS — private CA trust store + fingerprint whitelist → RFC 8705 §2 satisfied → PASS
      Defense in depth: CA chain AND fingerprint pre-registration
Node 5 — Application: PASS × 4
Node 6 — Translation: all four ext25 patterns correctly implemented
Node 7 — Arbiter: comprehensive ext25 multi-domain PASS
Node 8 — Arbiter verdict:
  PASS — All four ext25 domain patterns are correctly implemented. PAR satisfies RFC 9126
  §4.1 (single-use + client binding). JWKS uses HTTPS with TLS 1.3, trusted CA, public
  keys only, and rotation-aware caching. PKCE code_verifier uses module-level variable
  (not storage API) and is cleared after exchange. mTLS uses both CA validation and
  fingerprint pre-registration. No findings.
Node 9 — Format: PASS; four patterns confirmed conformant
```

**Reason**
All four ext25 baselines implemented correctly. The F3 note on module-level variable vs storage API adds a nuanced acknowledgment that module variables are still accessible to XSS (JavaScript heap) but are better than storage APIs because they are not persisted and are cleared post-exchange. F4 shows defense-in-depth (CA + fingerprint whitelist) which is more than the minimum.

**Phrasing-B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 / T-rfc-jwt / T-rfc-oauth2 / T-rfc-oauth2
Node 4 — Processing:
  F1: PAR Redis single-use + binding → RFC 9126 §4.1 → PASS
  F2: HTTPS JWKS + TLS 1.3 + trusted CA → RFC 8414 + RFC 7517 → PASS
  F3: 43-char CSPRNG + module variable + cleared after exchange → PASS
  F4: private CA + fingerprint whitelist → RFC 8705 §2 → PASS
Node 8 — Arbiter verdict:
  PASS — All four ext25 patterns correctly implemented. PAR, JWKS, PKCE storage, and
  mTLS all satisfy applicable requirements. No findings.
Node 9 — Format: PASS; four patterns confirmed
```
