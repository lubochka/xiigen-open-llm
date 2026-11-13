# CB4 Stage 3 — Multi-Node Pipeline Chain Set ext19
# Sequences CB4-3.4-655 through CB4-3.4-662
# Stratum C — full 9-node pipeline traces for ext25 patterns
# Running total after this file: 1324 instances (662 base pairs)
# Format: Pair[ID] / A / B / Reason / Phrasing-B

---

## Pair[CB4-3.4-655]

**A**
```
Incoming pipeline input:
"PAR authorization flow: request_uri is not tracked after issuance. If the same
request_uri appears at /authorize more than once (browser back, network retry,
attacker replay), the server processes it again. expires_in=60 seconds."
```

**B**
```
Pipeline trace — CB4-3.4-655

Node 1 — Domain Splitter:
  Input: PAR request_uri lifecycle management
  Domain: T-rfc-oauth2 (RFC 9126 Pushed Authorization Requests)
  Sub-phase: 3.4

Node 2 — Domain Translator:
  RFC 9126 §4.1: request_uri MUST be bound to the client and MUST be one-time use
  One-time use: authorization server must track whether URI has been consumed

Node 3 — Scheme Planner:
  Check 1: Is request_uri tracked post-issuance? → NO
  Check 2: Can the same URI be submitted more than once? → YES
  Check 3: Is one-time-use a MUST or SHOULD? → MUST (§4.1 unambiguous)
  Plan: RFC 9126 §4.1 MUST violation assessment

Node 4 — Domain Processor:
  No tracking → second /authorize request with same request_uri accepted → MUST violation
  Attack vector: attacker intercepts or observes request_uri → replays it at /authorize
    → server generates new authorization code for attacker session
  "expires_in=60 limits window" → risk-reduction; does not satisfy MUST requirement
  RFC 9126 §4.1 is unconditional: one-time-use is mandatory regardless of expiry duration

Node 5 — Applier:
  REPROCESS (RFC 9126 §4.1 MUST one-time-use not enforced)

Node 6 — Result Translator:
  Controlling violation: RFC 9126 §4.1 one-time-use not tracked

Node 7 — Arbiter Selector:
  T-rfc-oauth2 — PAR request_uri enforcement

Node 8 — Arbiter:
  REPROCESS — No request_uri one-time-use tracking violates RFC 9126 §4.1 MUST.
  The same URI can be replayed to generate additional authorization codes. Remediate:
  track consumed request_uris in a cache with TTL = expires_in. Reject second use.

Node 9 — Formatter:
  REPROCESS
  RFC 9126 §4.1 MUST one-time-use — not enforced
  Remediation: consumed-URI cache with TTL = expires_in
```

**Reason**
Full 9-node trace for PAR one-time-use violation. Node 4 evaluates the "expires_in limits window" mitigation claim and rejects it as not satisfying a MUST requirement. The attack vector is traced through all nodes to Node 8's verdict.

**Phrasing-B**
```
Pipeline trace — CB4-3.4-655 (phrasing B)

Node 1: T-rfc-oauth2 (RFC 9126)
Node 2: §4.1 MUST one-time-use — unconditional
Node 4: No tracking → second use accepted → MUST violation
  "expires_in=60 limits risk" → risk-reduction argument; does not satisfy MUST
Node 8 — Arbiter: REPROCESS — §4.1 MUST one-time-use absent.
  Add consumed-URI cache (TTL = expires_in). Reject second use.
Node 9: REPROCESS; §4.1 cited
```

---

## Pair[CB4-3.4-656]

**A**
```
Incoming pipeline input:
"Correct PAR implementation: POST /par returns request_uri (urn:...:abc123)
with expires_in=90. Server stores request_uri in Redis: TTL=90s, consumed=false.
At /authorize: consumed flag set to true on first use; second use → 400 error.
client_id binding enforced at both /par and /authorize endpoints."
```

**B**
```
Pipeline trace — CB4-3.4-656

Node 1 — Domain Splitter:
  Input: PAR implementation review
  Domain: T-rfc-oauth2 (RFC 9126)
  Sub-phase: 3.4

Node 2 — Domain Translator:
  RFC 9126 §4.1: one-time-use + client binding both MUST

Node 3 — Scheme Planner:
  Check 1: One-time-use tracked? → YES (Redis consumed flag)
  Check 2: Client binding enforced? → YES (at /par and /authorize)
  Check 3: Expiry enforced? → YES (Redis TTL = 90s)
  Plan: all §4.1 controls present; verify PASS

Node 4 — Domain Processor:
  Redis consumed flag + second use → 400 → one-time-use PASS
  client_id binding at /par + validation at /authorize → PASS
  TTL 90s = expires_in → URI expires correctly → PASS
  All three RFC 9126 §4.1 controls verified

Node 5 — Applier:
  PASS (all RFC 9126 §4.1 requirements satisfied)

Node 6 — Result Translator:
  PAR implementation correct and complete

Node 7 — Arbiter Selector:
  T-rfc-oauth2 — PAR PASS

Node 8 — Arbiter:
  PASS — PAR implementation satisfies RFC 9126 §4.1: single-use enforced via Redis
  consumed flag with TTL matching expires_in; client_id binding enforced at both
  push and authorization endpoints. No findings.

Node 9 — Formatter:
  PASS
  RFC 9126 §4.1: single-use + client binding + expiry all confirmed
```

**Reason**
Full pipeline trace confirming correct PAR: all three §4.1 controls present (single-use, client binding, expiry). Node 3 plans all three checks before Node 4 verifies each one. This is the Stratum C pipeline pair for the CB4-3.4-647/648 arbiter pair.

**Phrasing-B**
```
Pipeline trace — CB4-3.4-656 (phrasing B)

Node 1: T-rfc-oauth2 (RFC 9126)
Node 3: three checks — single-use, client binding, expiry
Node 4: Redis consumed flag → PASS; client_id binding → PASS; TTL=90s → PASS
Node 8 — Arbiter: PASS — All RFC 9126 §4.1 controls satisfied. No findings.
Node 9: PASS; §4.1 confirmed
```

---

## Pair[CB4-3.4-657]

**A**
```
Incoming pipeline input:
"JWKS endpoint served over HTTP at http://auth.internal/jwks. Reasoning:
it's an internal endpoint, only reachable from within the private network.
HTTPS adds operational overhead for an internal-only service."
```

**B**
```
Pipeline trace — CB4-3.4-657

Node 1 — Domain Splitter:
  Input: JWKS endpoint transport security
  Domains: T-rfc-jwt (RFC 7517) + T-rfc-oauth2 (RFC 8414)
  Sub-phase: 3.4

Node 2 — Domain Translator:
  RFC 8414 §2: jwks_uri in AS metadata MUST be an HTTPS URL
  RFC 7517 §5: JWK Set distribution for key verification SHOULD use HTTPS for integrity
  HTTPS for JWKS: prevents on-path key substitution attack

Node 3 — Scheme Planner:
  Check 1: Is jwks_uri scheme HTTP? → YES
  Check 2: Does RFC 8414 §2 require HTTPS? → YES (MUST)
  Check 3: Does "internal network" exception exist? → NO
  Plan: RFC 8414 §2 MUST violation; internal argument evaluation

Node 4 — Domain Processor:
  HTTP JWKS: on-path attacker on internal network can substitute public keys in response
    → all resource servers accept tokens signed by attacker's key (signature verification bypass)
  "Internal only" argument: same pattern as "internal network = no authentication needed"
    → rejected; internal networks have insider threats, misconfigured routes, lateral movement
  RFC 8414 §2 MUST: no exception for internal networks
  "HTTPS overhead" → operational concern; does not override MUST requirement

Node 5 — Applier:
  REPROCESS (RFC 8414 §2 MUST HTTPS violated)

Node 6 — Result Translator:
  Primary: RFC 8414 §2 — jwks_uri MUST be HTTPS
  Secondary: JWT verification bypass via key substitution

Node 7 — Arbiter Selector:
  T-rfc-jwt + T-rfc-oauth2 — JWKS transport

Node 8 — Arbiter:
  REPROCESS — HTTP jwks_uri violates RFC 8414 §2 MUST. HTTP JWKS allows key substitution
  by any on-path host on the internal network, bypassing all JWT signature verification.
  "Internal network" is not a recognized exception. Serve JWKS over HTTPS.

Node 9 — Formatter:
  REPROCESS
  RFC 8414 §2 MUST — jwks_uri must be HTTPS
  Internal-network argument rejected
  Remediation: serve JWKS over HTTPS; configure TLS certificate for auth.internal
```

**Reason**
Full pipeline trace for HTTP JWKS. Node 4 traces the attack (on-path key substitution → JWT verification bypass) and rejects the internal-network argument using the established pattern. RFC 8414 §2 MUST is the controlling reference.

**Phrasing-B**
```
Pipeline trace — CB4-3.4-657 (phrasing B)

Node 1: T-rfc-jwt + T-rfc-oauth2
Node 2: RFC 8414 §2 MUST HTTPS for jwks_uri
Node 4: HTTP → on-path key substitution → JWT verification bypass
  "Internal network" → rejected (same pattern: LAN ≠ trusted)
Node 8 — Arbiter: REPROCESS — jwks_uri MUST be HTTPS. HTTP allows key substitution.
  Use HTTPS for auth.internal JWKS.
Node 9: REPROCESS; RFC 8414 §2 primary
```

---

## Pair[CB4-3.4-658]

**A**
```
Incoming pipeline input:
"JWKS: https://auth.example.com/.well-known/jwks.json (HTTPS, TLS 1.3, Let's Encrypt CA).
Public RSA keys only. Resource servers cache with Cache-Control: max-age=3600 and
re-fetch on JWT signature verification failure to support key rotation."
```

**B**
```
Pipeline trace — CB4-3.4-658

Node 1 — Domain Splitter:
  Input: JWKS endpoint configuration review
  Domains: T-rfc-jwt + T-rfc-oauth2
  Sub-phase: 3.4

Node 2 — Domain Translator:
  RFC 8414 §2: jwks_uri MUST be HTTPS
  RFC 7517 §5: public keys served over HTTPS for integrity
  Caching: rotation-aware (re-fetch on failure) is best practice

Node 3 — Scheme Planner:
  Check 1: HTTPS? → YES
  Check 2: Trusted CA? → YES (Let's Encrypt, publicly trusted)
  Check 3: Public key only? → YES
  Check 4: Cache rotation-aware? → YES (re-fetch on failure)
  Plan: all controls present; PASS expected

Node 4 — Domain Processor:
  HTTPS + TLS 1.3 + Let's Encrypt → RFC 8414 §2 + RFC 8446 → PASS
  Public keys only exposed → no private key in JWKS → PASS
  1-hour cache + re-fetch on signature failure → supports key rotation without disruption → PASS
  All requirements verified

Node 5 — Applier:
  PASS (RFC 8414 §2 + RFC 7517 all satisfied)

Node 6 — Result Translator:
  JWKS correctly configured; no violations

Node 7 — Arbiter Selector:
  T-rfc-jwt + T-rfc-oauth2 — JWKS PASS

Node 8 — Arbiter:
  PASS — JWKS endpoint correctly configured: HTTPS with TLS 1.3 and publicly trusted CA.
  Public keys only. Rotation-aware caching (1-hour + re-fetch on failure). No findings.

Node 9 — Formatter:
  PASS
  RFC 8414 §2 + RFC 7517: HTTPS confirmed
  Public-only content confirmed
  Rotation-aware caching confirmed
```

**Reason**
Full pipeline trace for correct JWKS configuration. Node 3 lists four checks before Node 4 verifies each. The rotation-aware caching pattern (re-fetch on failure) is explicitly evaluated as supporting key rotation without disruption.

**Phrasing-B**
```
Pipeline trace — CB4-3.4-658 (phrasing B)

Node 1: T-rfc-jwt + T-rfc-oauth2
Node 3: four checks — HTTPS, CA, public-only, rotation-aware cache
Node 4: all four PASS
Node 8 — Arbiter: PASS — HTTPS JWKS with TLS 1.3, trusted CA, public-only content,
  rotation-aware caching. No findings.
Node 9: PASS; RFC 8414 + RFC 7517 confirmed
```

---

## Pair[CB4-3.4-659]

**A**
```
Incoming pipeline input:
"SPA stores PKCE code_verifier in sessionStorage. Verifier: 43-char URL-safe base64,
crypto.getRandomValues(). Developer: sessionStorage cleared on tab close — more
secure than localStorage. PKCE protects against code theft; code alone is useless."
```

**B**
```
Pipeline trace — CB4-3.4-659

Node 1 — Domain Splitter:
  Input: PKCE code_verifier storage method in SPA
  Domains: T-rfc-oauth2 (RFC 7636) + T-asvs-token
  Sub-phase: 3.4

Node 2 — Domain Translator:
  RFC 7636 §4.1: specifies verifier format and entropy; does not specify storage
  V9.3.1 / ASVS: token-equivalent data should not be in JavaScript-accessible storage

Node 3 — Scheme Planner:
  Check 1: verifier format → 43-char CSPRNG → RFC 7636 §4.1 PASS
  Check 2: storage method → sessionStorage → JavaScript-accessible → XSS risk
  Check 3: developer arguments → evaluate
  Plan: format PASS; storage CONCERN assessment

Node 4 — Domain Processor:
  Verifier format: 43-char, CSPRNG → RFC 7636 §4.1 satisfied → PASS
  sessionStorage:
    JavaScript-accessible in same origin → XSS can read verifier
    XSS attack yields: verifier from sessionStorage + auth code from URL/callback
    → complete code+verifier pair → attacker can exchange at /token
    "PKCE means code alone useless" → correct; XSS yields both → argument does not neutralize
    "sessionStorage cleared on tab close" → true; but XSS executes during the tab session
    when verifier is present → clearing after does not help against in-session XSS
  Alternative: module-level JS variable → not persisted; shorter window
  Not a REPROCESS: RFC 7636 does not mandate storage location; no MUST violated → CONCERN

Node 5 — Applier:
  CONCERN (sessionStorage XSS risk; verifier format PASS)

Node 6 — Result Translator:
  CONCERN — verifier format correct; storage method creates XSS exfiltration pathway

Node 7 — Arbiter Selector:
  T-rfc-oauth2 — PKCE verifier storage

Node 8 — Arbiter:
  CONCERN — PKCE code_verifier format is correct (RFC 7636 §4.1). Storage in sessionStorage
  is JavaScript-accessible: XSS on the same origin can read the verifier and combine it
  with the authorization code from the callback URL. "PKCE protects against code theft"
  is true but irrelevant when XSS yields both code and verifier simultaneously.
  sessionStorage clearing on tab close does not protect against in-session XSS.
  Use a module-level variable (not storage API) and clear it after token exchange.

Node 9 — Formatter:
  CONCERN
  Verifier format: PASS (RFC 7636 §4.1)
  Storage method: CONCERN (sessionStorage XSS-accessible)
  Developer arguments evaluated and rejected as incomplete
  Recommendation: module-level variable, cleared post-exchange
```

**Reason**
Full pipeline trace for PKCE verifier in sessionStorage. Node 4 traces the XSS attack path and evaluates both developer arguments. The key point: XSS executes during the session when the verifier is present, so "cleared on tab close" does not help. CONCERN not REPROCESS because RFC 7636 does not mandate storage location.

**Phrasing-B**
```
Pipeline trace — CB4-3.4-659 (phrasing B)

Node 1: T-rfc-oauth2 (RFC 7636) / T-asvs-token
Node 3: format PASS; storage → XSS risk
Node 4: sessionStorage XSS-accessible; XSS yields code+verifier pair
  "sessionStorage cleared on close" → tab-close timing doesn't protect in-session XSS
  No MUST violated → CONCERN
Node 8 — Arbiter: CONCERN — Verifier format correct; sessionStorage XSS-accessible.
  Use module-level variable cleared after exchange.
Node 9: CONCERN; format PASS; storage CONCERN
```

---

## Pair[CB4-3.4-660]

**A**
```
Incoming pipeline input:
"mTLS client authentication: authorization server accepts any client certificate
that completes the TLS handshake, including self-signed. No CA validation performed.
Developer: the TLS handshake proves the client has the private key — that's the authentication."
```

**B**
```
Pipeline trace — CB4-3.4-660

Node 1 — Domain Splitter:
  Input: mTLS client certificate validation policy
  Domains: T-rfc-oauth2 (RFC 8705) + T-asvs-authentication
  Sub-phase: 3.4

Node 2 — Domain Translator:
  RFC 8705 §2: mTLS client auth requires certificate identity binding:
    (a) PKI with CA chain validation, OR (b) self-signed pre-registered fingerprints
  CWE-295: Improper Certificate Validation

Node 3 — Scheme Planner:
  Check 1: CA validation performed? → NO
  Check 2: Self-signed pre-registration? → NO (any self-signed accepted)
  Check 3: Does handshake alone satisfy RFC 8705? → NO
  Plan: RFC 8705 §2 violation; developer argument assessment

Node 4 — Domain Processor:
  TLS handshake proves: presenter holds private key for the presented certificate
    It does NOT prove: the certificate identity is trustworthy or pre-authorized
  Any client can generate self-signed cert with cn=legitimate-service → handshake succeeds
    → server accepts it as authenticated → identity spoofing trivially achieved
  Developer argument: "handshake = authentication" → HALF TRUE
    Handshake proves key possession for the presented cert
    Without identity binding, "possession" is meaningless: anyone can generate a cert
  RFC 8705 §2: requires CA chain OR explicit pre-registration; neither present → REPROCESS
  CWE-295: certificate validation bypassed

Node 5 — Applier:
  REPROCESS (CWE-295 + RFC 8705 §2 — no identity binding)

Node 6 — Result Translator:
  mTLS without CA validation or pre-registration = unauthenticated

Node 7 — Arbiter Selector:
  T-rfc-oauth2 + T-asvs-authentication — mTLS certificate validation

Node 8 — Arbiter:
  REPROCESS — Accepting any self-signed certificate without CA validation or pre-registration
  provides no authentication. Any party generates a self-signed cert with any identity and
  completes the TLS handshake. CWE-295. RFC 8705 §2 requires CA chain or pre-registered
  fingerprints. Remediate: configure AS with CA trust store or implement fingerprint whitelist.

Node 9 — Formatter:
  REPROCESS
  CWE-295 — Improper Certificate Validation
  RFC 8705 §2 — neither CA chain nor pre-registration present
  Developer argument ("handshake = auth") rebutted
  Remediation: CA trust store or fingerprint pre-registration
```

**Reason**
Full pipeline trace for mTLS without CA validation. Node 4 provides the identity-spoofing attack vector (anyone generates self-signed cert with any cn). The developer argument is traced through the pipeline and rejected in Node 4. Node 8 delivers the REPROCESS with remediation options.

**Phrasing-B**
```
Pipeline trace — CB4-3.4-660 (phrasing B)

Node 1: T-rfc-oauth2 (RFC 8705) / T-asvs-authentication
Node 2: RFC 8705 §2 — CA chain OR pre-registered fingerprints required; CWE-295
Node 4: No CA + no pre-registration → identity spoofing trivial (self-signed any cn)
  "Handshake = auth" → proves key possession only; no identity binding without CA/pre-reg
Node 8 — Arbiter: REPROCESS — CWE-295. Any self-signed cert accepted = no authentication.
  Implement CA trust store or certificate fingerprint whitelist.
Node 9: REPROCESS; CWE-295 + RFC 8705 §2; argument rebutted
```

---

## Pair[CB4-3.4-661]

**A**
```
Incoming pipeline input:
"PAR short-expiry argument: request_uri expires in 30 seconds. We don't track
one-time-use. Developer claims: a 30-second window makes replay attacks impractical
because the attacker would need to act before the URI expires."
```

**B**
```
Pipeline trace — CB4-3.4-661

Node 1 — Domain Splitter:
  Input: PAR request_uri one-time-use with short-expiry argument
  Domain: T-rfc-oauth2 (RFC 9126)
  Sub-phase: 3.4

Node 2 — Domain Translator:
  RFC 9126 §4.1: MUST be one-time-use — no conditional language

Node 3 — Scheme Planner:
  Check 1: One-time-use tracked? → NO
  Check 2: Does short expiry (30s) satisfy MUST? → MUST not conditional on risk
  Check 3: Is 30-second replay "impractical"? → evaluate
  Plan: MUST violation; developer risk argument evaluation

Node 4 — Domain Processor:
  RFC 9126 §4.1 MUST one-time-use: no exception clause for short expiry
  Risk-based argument: "30 seconds = impractical replay" →
    (1) MUST requirements not conditional on developer risk assessment
    (2) Attacker with pre-positioned active IdP session can complete authorization in seconds:
        observe request_uri → redirect to /authorize?request_uri=... → silent auth via
        existing IdP session → authorization code issued within ~2-5 seconds
    (3) "30 seconds" is not "impractical" for a scripted pre-positioned attacker
  Both the normative and practical analysis lead to REPROCESS

Node 5 — Applier:
  REPROCESS (RFC 9126 §4.1 MUST; risk argument rejected on both normative and practical grounds)

Node 6 — Result Translator:
  Normative: MUST not conditional on risk
  Practical: 30 seconds is adequate for pre-positioned attacker

Node 7 — Arbiter Selector:
  T-rfc-oauth2 — PAR one-time-use

Node 8 — Arbiter:
  REPROCESS — RFC 9126 §4.1 MUST one-time-use has no risk-based exception. Additionally,
  30 seconds is not impractical for a pre-positioned attacker with an active IdP session —
  silent authorization can complete in 2-5 seconds. Implement request_uri consumption
  tracking (cache with TTL = expires_in).

Node 9 — Formatter:
  REPROCESS
  RFC 9126 §4.1 MUST — no risk exception
  Short-expiry argument rejected: normative (MUST) + practical (pre-positioned attacker)
  Remediation: consumed-URI cache; TTL = expires_in
```

**Reason**
Full pipeline trace for the short-expiry-as-substitute-for-one-time-use argument. Node 4 evaluates the argument on two grounds: (1) MUST requirements are unconditional and (2) 30 seconds is not impractical for a pre-positioned attacker. Both grounds independently support REPROCESS.

**Phrasing-B**
```
Pipeline trace — CB4-3.4-661 (phrasing B)

Node 1: T-rfc-oauth2 (RFC 9126)
Node 2: §4.1 MUST one-time-use — unconditional
Node 4: Two grounds:
  (1) MUST not conditional on risk assessment
  (2) Pre-positioned attacker with active IdP session completes in ~2-5 seconds
Node 8 — Arbiter: REPROCESS — §4.1 MUST + 30s is not impractical for scripted attacker.
  Add consumed-URI cache.
Node 9: REPROCESS; both grounds stated
```

---

## Pair[CB4-3.4-662]

**A**
```
Incoming pipeline input — full ext25 PASS scenario:
"PAR: Redis consumed flag, TTL=90, client_id binding at /par and /authorize.
JWKS: https with TLS 1.3, trusted CA, public keys only, rotation-aware caching.
PKCE: module-level JS variable, 43-char CSPRNG, cleared post-exchange.
mTLS: private CA trust store + fingerprint pre-registration per client."
```

**B**
```
Pipeline trace — CB4-3.4-662

Node 1 — Domain Splitter:
  Input: comprehensive ext25 pattern review
  Domains: T-rfc-oauth2 / T-rfc-jwt / T-rfc-oauth2 / T-rfc-oauth2
  Sub-phase: 3.4

Node 2 — Domain Translator:
  PAR → RFC 9126 §4.1
  JWKS → RFC 8414 §2 + RFC 7517
  PKCE → RFC 7636 §4.1 + storage practice
  mTLS → RFC 8705 §2

Node 3 — Scheme Planner:
  Four patterns; four independent PASS checks expected
  Plan: verify each control satisfies its normative requirement

Node 4 — Domain Processor:
  PAR: Redis consumed flag + client_id binding at push+use + TTL=90s → §4.1 PASS
  JWKS: HTTPS + TLS 1.3 + trusted CA + public-only + rotation-aware → RFC 8414 + RFC 7517 PASS
  PKCE: module variable (not storage API) + 43-char CSPRNG + cleared post-exchange
    → format RFC 7636 §4.1 PASS; storage better than sessionStorage/localStorage PASS
  mTLS: private CA trust store + fingerprint pre-registration → RFC 8705 §2 both conditions → PASS

Node 5 — Applier:
  PASS × 4

Node 6 — Result Translator:
  All four ext25 patterns correct

Node 7 — Arbiter Selector:
  Comprehensive multi-domain PASS

Node 8 — Arbiter:
  PASS — All four ext25 domain patterns correctly implemented. PAR: RFC 9126 §4.1
  single-use + client binding. JWKS: RFC 8414 §2 HTTPS + TLS 1.3 + public-only +
  rotation-aware caching. PKCE: module-level variable (not storage API) + CSPRNG +
  cleared post-exchange. mTLS: private CA + fingerprint pre-registration (defense in
  depth beyond RFC 8705 §2 minimum). No findings.

Node 9 — Formatter:
  PASS
  PAR: RFC 9126 §4.1 confirmed ✓
  JWKS: RFC 8414 §2 + RFC 7517 confirmed ✓
  PKCE: RFC 7636 §4.1 + storage practice confirmed ✓
  mTLS: RFC 8705 §2 confirmed (CA + pre-registration) ✓
```

**Reason**
Comprehensive PASS pipeline trace for all four ext25 patterns. Node 4 evaluates each against its applicable RFC. The mTLS note acknowledges that CA + fingerprint pre-registration is defense-in-depth beyond the RFC 8705 §2 minimum (which requires one or the other). This closes the ext25 Stratum C traces.

**Phrasing-B**
```
Pipeline trace — CB4-3.4-662 (phrasing B)

Node 1: T-rfc-oauth2 / T-rfc-jwt / T-rfc-oauth2 / T-rfc-oauth2
Node 4:
  PAR: single-use + binding + TTL → §4.1 → PASS
  JWKS: HTTPS + trusted CA + public-only + rotation-aware → RFC 8414 + RFC 7517 → PASS
  PKCE: module variable + CSPRNG + cleared → RFC 7636 + storage practice → PASS
  mTLS: CA + fingerprint pre-reg → RFC 8705 §2 → PASS
Node 8 — Arbiter: PASS — All four ext25 patterns correct. No findings.
Node 9: PASS; four patterns confirmed
```
