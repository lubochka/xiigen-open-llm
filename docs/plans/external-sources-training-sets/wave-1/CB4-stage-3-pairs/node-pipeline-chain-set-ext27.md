# CB4 Stage 3 — Action 3 Pipeline Chain Set
# Node: Multi-node (full 9-node traces) — ext27
# Sequences: CB4-3.4-863 through CB4-3.4-870
# Instances: 16 (8 base pairs × 2 phrasings each)
# Stratum: C (full pipeline traces for ext33 patterns)
# Running total after this file: 1740 instances (870 base pairs)

---

## Pair[CB4-3.4-863]

### A
Full pipeline trace — JWT iss claim validated with substring match (RFC 7519 §4.1.1):

A resource server validates JWT access tokens by checking whether the iss claim contains the string "as.example.com" rather than performing an exact string comparison. Tokens from "https://evil-as.example.com/" pass the substring check.

### B
Node 1 — Domain Splitter: Input domain: T-rfc-jwt / T-rfc-oauth2. Sub-domain: JWT iss (issuer) claim validation semantics — substring match vs exact string comparison. Security implication: issuer confusion via substring collision. Pipeline route: Node 8 RFC arbiter with RFC 7519 §4.1.1 analysis.

Node 2 — Domain Translator: RFC 7519 §4.1.1: "The issuer claim value is a case-sensitive string." RFC 9068 §4: RS MUST verify the iss claim identifies the expected AS. Comparison semantics derived from RFC 7519 §4.1.1: the iss value is compared as a complete, exact string — not as a substring, prefix, suffix, or pattern. RFC 8414 §2: AS identifier is an exact URL string. No RFC provides a "contains" matching mode for iss.

Node 3 — Scheme Planner: Verify comparison type against RFC 7519 §4.1.1 requirement. Model attack path: attacker controls "evil-as.example.com"; substring "as.example.com" present → pass. Evaluate team's multi-environment argument. Expected: REPROCESS if exact comparison requirement violated.

Node 4 — Domain Processor: Substring match "as.example.com": test with "https://evil-as.example.com/" — contains "as.example.com" → passes. RFC 7519 §4.1.1 requires exact comparison — not substring. Attack path: attacker registers "https://evil-as.example.com/", creates AS metadata at that URL, issues tokens with iss = "https://evil-as.example.com/". In JWKS discovery mode, RS fetches keys from the attacker's discovery URL, obtains the attacker's public key, and uses it to validate tokens the attacker signed. Both iss check and signature check pass. Complete issuer confusion attack. Multi-environment argument: rejected — correct approach is explicit allowlist of exact issuer strings per environment.

Node 5 — Applier: RFC 7519 §4.1.1 exact string comparison violated. Issuer confusion attack feasible. REPROCESS.

Node 6 — Result Translator: REPROCESS. JWT iss substring match allows attacker-controlled issuers to bypass iss validation. In JWKS-discovery mode, this enables complete issuer confusion with forged token acceptance.

Node 7 — Arbiter Selector: Route to Node 8 RFC arbiter (T-rfc-jwt). MUST-level comparison semantics violated. No PO.1 escalation needed (already REPROCESS).

Node 8 — Arbiter Units: REPROCESS. RFC 7519 §4.1.1 case-sensitive exact string comparison violated. Substring match enables issuer confusion. "Evil-as.example.com" contains "as.example.com" → passes check → attacker-issued tokens accepted. Multi-environment: use allowlist of exact issuer strings per environment. REPROCESS.

Node 9 — Formatter: REPROCESS. RFC 7519 §4.1.1 iss validation must use exact case-sensitive string comparison. Substring/contains matching allows any issuer whose URL contains the target string — including adversarial subdomains. In JWKS-discovery environments: RS fetches attacker's JWKS, attacker's signatures validate, attack complete. Remediation: configure RS with allowlist of exact issuer URLs; compare byte-for-byte; reject any token whose iss is not in the allowlist.

### Reason
9-node trace correctly identifies RFC 7519 §4.1.1 exact-string requirement, models the substring-collision attack path (including JWKS-discovery chain), and rejects the multi-environment argument. REPROCESS.

### Phrasing-B
REPROCESS — 9-node trace: JWT iss substring match, RFC 7519 §4.1.1 exact comparison violated, issuer confusion attack viable.

N1: T-rfc-jwt / iss validation semantics; substring vs exact.
N2: RFC 7519 §4.1.1 case-sensitive string; RFC 9068 §4 exact AS identification required.
N3: Model contains("as.example.com") vs exact; test attack path; evaluate multi-environment argument.
N4: evil-as.example.com contains target → passes; JWKS-discovery chain → attacker keys fetched → forged tokens accepted.
N5: Exact comparison violated; issuer confusion feasible. REPROCESS.
N6: REPROCESS — substring match enables issuer confusion.
N7: RFC arbiter T-rfc-jwt; MUST violation.
N8: REPROCESS — RFC 7519 §4.1.1 violated; use exact allowlist per environment.
N9: REPROCESS. Exact allowlist; byte-for-byte comparison; no pattern matching for iss.

---

## Pair[CB4-3.4-864]

### A
Full pipeline trace — JWT iss claim validated with exact case-sensitive allowlist:

A resource server validates JWT access tokens by comparing the iss claim against a per-environment allowlist of exact issuer identifiers. The comparison is case-sensitive byte-for-byte. Tokens whose iss is not in the allowlist are rejected with 401.

### B
Node 1 — Domain Splitter: Input domain: T-rfc-jwt / T-rfc-oauth2. Sub-domain: JWT iss validation with exact allowlist. Pipeline route: Node 8 RFC arbiter with positive compliance confirmation.

Node 2 — Domain Translator: RFC 7519 §4.1.1 case-sensitive exact string comparison. Exact allowlist per environment. No substring, prefix, or pattern matching. RFC 9068 §4: RS validates iss identifies the specific expected AS.

Node 3 — Scheme Planner: Verify comparison is exact case-sensitive. Verify allowlist is per-environment. Verify no substring or prefix matching. Expected: PASS if exact comparison confirmed.

Node 4 — Domain Processor: Allowlist: ["https://as.example.com/"] exact strings per environment. Comparison: case-sensitive byte-for-byte. "https://evil-as.example.com/" — not in allowlist → rejected. "https://as.example.com/" — in allowlist → passes. "https://As.Example.Com/" (case variant) — not in allowlist → rejected. Correct. Per-environment isolation: separate allowlists for staging and production. No shared substring matching across environments.

Node 5 — Applier: RFC 7519 §4.1.1 exact comparison confirmed. No issuer confusion surface. PASS.

Node 6 — Result Translator: PASS. Exact case-sensitive allowlist satisfies RFC 7519 §4.1.1. Issuer confusion attacks blocked.

Node 7 — Arbiter Selector: Route to Node 8 RFC arbiter (T-rfc-jwt). Positive compliance.

Node 8 — Arbiter Units: PASS. RFC 7519 §4.1.1 satisfied. Exact case-sensitive allowlist per environment. No substring matching. Issuer confusion attacks blocked. No findings.

Node 9 — Formatter: PASS. JWT iss validated with exact case-sensitive allowlist per RFC 7519 §4.1.1. Per-environment isolation. Issuer confusion attacks — including adversarial subdomain and case-variant attacks — blocked at iss validation layer.

### Reason
9-node trace confirms exact case-sensitive allowlist satisfies RFC 7519 §4.1.1, verifies per-environment isolation, and confirms issuer confusion attacks are blocked. PASS.

### Phrasing-B
PASS — 9-node trace: JWT iss exact allowlist, RFC 7519 §4.1.1 satisfied, per-environment isolation confirmed.

N1: T-rfc-jwt / iss validation; exact allowlist pattern.
N2: RFC 7519 §4.1.1 case-sensitive string; RFC 9068 §4 RS validates exact AS identity.
N3: Verify exact comparison; per-environment allowlist; no pattern matching.
N4: Exact allowlist confirmed; evil-as.example.com rejected; case-variant rejected; per-environment isolation confirmed.
N5: Exact comparison confirmed. PASS.
N6: PASS — RFC 7519 §4.1.1 satisfied.
N7: RFC arbiter T-rfc-jwt; positive.
N8: PASS — exact allowlist, no issuer confusion surface.
N9: PASS. Exact case-sensitive allowlist per environment; issuer confusion blocked.

---

## Pair[CB4-3.4-865]

### A
Full pipeline trace — DPoP proof presented at RS without ath (access token hash) claim:

A resource server accepts DPoP-bound access tokens. DPoP proofs contain htm, htu, iat, and jti but no ath claim. The RS validates the DPoP proof signature, method, URI, and freshness but cannot verify that the proof was generated for the specific access token presented.

### B
Node 1 — Domain Splitter: Input domain: T-rfc-oauth2 / T-oauth-oidc. Sub-domain: DPoP (Demonstration of Proof-of-Possession) proof validation — ath claim requirement at RS. The scenario involves a MUST-level claim absence in the DPoP proof. Pipeline route: Node 8 RFC arbiter with RFC 9449 §4.2 analysis.

Node 2 — Domain Translator: RFC 9449 §4.2: "The DPoP proof JWT MUST contain ... an ath (access token hash) claim" when presented at a resource server. MUST-level requirement. ath = BASE64URL(SHA256(ASCII(access_token))). The ath claim is the cryptographic binding between the DPoP proof and the specific access token — without it, the sender-constrained property of DPoP is undermined.

Node 3 — Scheme Planner: Verify ath claim presence in DPoP proof at RS. Model attack path: stolen access token + attacker's DPoP proof (no ath required by RS). Evaluate "RS can correlate without ath" argument. Expected: REPROCESS if RFC 9449 §4.2 MUST violated.

Node 4 — Domain Processor: ath absent from DPoP proof. RFC 9449 §4.2 MUST: violated. Attack path: attacker intercepts access token. Attacker generates DPoP key pair and creates a DPoP proof with htm/htu/iat/jti for the target resource. No ath check required by RS → proof is valid even though it was not generated for the presented access token. RS validates: DPoP proof signature (valid, attacker's key), htm/htu match (valid), iat fresh (valid), jti fresh (valid). Access granted to attacker. "RS can correlate" argument: visual co-presence of token and proof in the same HTTP request does not prevent an attacker who controls their own DPoP key from generating a new proof after stealing the token. ath is the cryptographic requirement that binds proof to token; without it, DPoP does not constrain who can use the access token.

Node 5 — Applier: RFC 9449 §4.2 MUST ath violated. DPoP sender-constrained property undermined. Token theft exploitable with attacker's own DPoP key. REPROCESS.

Node 6 — Result Translator: REPROCESS. RFC 9449 §4.2 MUST ath absent. Token theft attack viable: stolen access token + attacker's DPoP proof (any key pair) = successful RS request. DPoP's core sender-binding guarantee broken by ath absence.

Node 7 — Arbiter Selector: Route to Node 8 RFC arbiter (T-rfc-oauth2). MUST-level violation. No PO.1 escalation needed (already REPROCESS).

Node 8 — Arbiter Units: REPROCESS. RFC 9449 §4.2 MUST ath: violated. Without ath, DPoP proof not cryptographically bound to the specific access token. Token theft: intercept AT → generate new DPoP proof with attacker's key → no ath cross-check fails → access granted. "RS can correlate" rejected. REPROCESS.

Node 9 — Formatter: REPROCESS. RFC 9449 §4.2 MUST ath claim absent from DPoP proof at RS. ath = BASE64URL(SHA256(ASCII(access_token))) — the cryptographic link between proof and token. Without ath, stolen access token usable with any DPoP key pair (attacker generates fresh proof). DPoP sender-constrained property reduced to sender-constrained-per-request-method-and-URI only, not per-token. Remediation: require ath in all DPoP proofs presented at RS; RS cross-checks ath against received access token value.

### Reason
9-node trace correctly identifies RFC 9449 §4.2 MUST ath, models the stolen-token-plus-new-proof attack, rejects the "RS can correlate" argument, and produces REPROCESS. The ath claim is the sender-to-token binding; its absence breaks the DPoP guarantee.

### Phrasing-B
REPROCESS — 9-node trace: DPoP proof without ath, RFC 9449 §4.2 MUST violated, token theft exploitable.

N1: T-rfc-oauth2 / DPoP ath claim at RS; sender-constrained property analysis.
N2: RFC 9449 §4.2 MUST ath = BASE64URL(SHA256(ASCII(AT))); binds proof to specific token.
N3: ath absent → verify MUST; model stolen token + new DPoP proof attack; evaluate correlation argument.
N4: ath absent; attack: intercept AT → new proof with attacker's key → no ath check → access granted.
N5: MUST ath violated; DPoP binding broken. REPROCESS.
N6: REPROCESS — RFC 9449 §4.2 MUST; token theft exploitable with any DPoP key.
N7: RFC arbiter T-rfc-oauth2; MUST violation.
N8: REPROCESS — ath absent; stolen token + attacker DPoP = valid; "correlation" rejected.
N9: REPROCESS. Require ath; RS cross-checks ath against received AT value.

---

## Pair[CB4-3.4-866]

### A
Full pipeline trace — DPoP proof with all required claims including ath, RS validates completely:

Resource server accepts DPoP-bound access tokens. DPoP proofs contain ath (access token hash), htm, htu, iat, jti. RS validates: DPoP proof signature, ath cross-checked against received access token, htm/htu match request, iat within window, jti not seen before. cnf/jkt in access token matches DPoP proof header public key thumbprint.

### B
Node 1 — Domain Splitter: Input domain: T-rfc-oauth2 / T-oauth-oidc. Sub-domain: DPoP proof complete validation with ath cross-check. Pipeline route: Node 8 RFC arbiter with positive compliance confirmation.

Node 2 — Domain Translator: RFC 9449 §4.2: all required claims present — ath (MUST at RS), htm (MUST), htu (MUST), iat (MUST), jti (MUST). cnf/jkt cross-check: RFC 9449 §7.1 — public key in DPoP proof header MUST match cnf/jkt in access token. RS validates all fields. Full sender-constrained DPoP properly implemented.

Node 3 — Scheme Planner: Verify all §4.2 required claims present. Verify ath cross-check correct. Verify htm/htu match. Verify iat freshness. Verify jti replay prevention. Verify cnf/jkt cross-check. Expected: PASS if all controls in place.

Node 4 — Domain Processor: ath: BASE64URL(SHA256(ASCII(AT))) — RS recomputes from received AT, verifies match against ath in proof. Correct. htm/htu: match the current request method and URI. Correct. iat: within freshness window. jti: UUID v4, tracked by RS for replay prevention. cnf/jkt: RS verifies public key in DPoP proof header thumbprint matches cnf/jkt in AT — confirms consistent key pair usage. Token theft: intercepted AT cannot be used with attacker's DPoP key because ath binds proof to specific AT value, and cnf/jkt binds AT to specific public key. Attack requires attacker to also possess the DPoP private key.

Node 5 — Applier: All RFC 9449 §4.2 requirements satisfied. cnf/jkt cross-check confirms key consistency. Token is fully sender-constrained. PASS.

Node 6 — Result Translator: PASS. All DPoP requirements met. Token sender-constrained: theft without private key is non-exploitable. RFC 9449 §4.2 satisfied.

Node 7 — Arbiter Selector: Route to Node 8 RFC arbiter (T-rfc-oauth2). Positive compliance.

Node 8 — Arbiter Units: PASS. RFC 9449 §4.2 all required claims present and validated. ath cross-check confirms token binding. cnf/jkt cross-check confirms key consistency. Token sender-constrained. No findings.

Node 9 — Formatter: PASS. DPoP proof complete: ath (token binding verified), htm/htu (request binding), iat (freshness), jti (replay prevention), cnf/jkt (key consistency). RFC 9449 §4.2 fully satisfied. Stolen access token non-exploitable without corresponding DPoP private key.

### Reason
9-node trace confirms all RFC 9449 §4.2 DPoP proof requirements satisfied including ath cross-check and cnf/jkt key consistency verification. Full sender-constrained token. PASS.

### Phrasing-B
PASS — 9-node trace: DPoP proof complete with ath, RS validates all fields including cnf/jkt cross-check.

N1: T-rfc-oauth2 / DPoP full compliance; ath + cnf/jkt cross-check.
N2: RFC 9449 §4.2 all MUST claims; §7.1 cnf/jkt MUST match.
N3: Verify ath cross-check; htm/htu match; iat freshness; jti replay; cnf/jkt consistency.
N4: All controls confirmed; token theft non-exploitable without private key.
N5: All requirements met. PASS.
N6: PASS — RFC 9449 §4.2 fully satisfied; sender-constrained.
N7: RFC arbiter T-rfc-oauth2; positive.
N8: PASS — all DPoP requirements met; no findings.
N9: PASS. ath verified; cnf/jkt verified; token sender-constrained.

---

## Pair[CB4-3.4-867]

### A
Full pipeline trace — PKCE code_verifier generated from Unix timestamp (RFC 7636 §4.1):

A native mobile application generates the PKCE code_verifier from the current Unix timestamp in milliseconds, producing a 13-character string. The code_challenge is BASE64URL(SHA256(timestamp_string)). Team: "Timestamps are unique per request."

### B
Node 1 — Domain Splitter: Input domain: T-rfc-oauth2 / T-asvs-authentication. Sub-domain: PKCE code_verifier generation quality — entropy source and length. The scenario presents timestamp-based generation as a substitute for cryptographic randomness. Pipeline route: Node 8 RFC arbiter with RFC 7636 §4.1 analysis.

Node 2 — Domain Translator: RFC 7636 §4.1: "The client MUST generate a fresh code_verifier per authorization code transaction using a high-entropy cryptographic random number generator." Two requirements: (1) high-entropy CSPRNG source; (2) minimum 43 characters (43*128unreserved-char). Both are MUST-level. Timestamp satisfies neither: it is not a CSPRNG, and 13 characters is below the 43-character minimum.

Node 3 — Scheme Planner: Verify entropy source (CSPRNG vs timestamp). Model brute-force attack: timestamp ≈ 10 bits/second entropy; ±5s window → enumerable. Verify length (13 chars < 43 char minimum). Evaluate "unique" argument. Expected: REPROCESS if MUST violated on entropy and/or length.

Node 4 — Domain Processor: Entropy source: Unix timestamp in milliseconds. Entropy per millisecond: ~10 bits. An attacker who can estimate the authorization request time within ±5 seconds can enumerate 10,000 candidate verifiers (5000ms × 2). SHA256(candidate_timestamp) computed offline. Against a 15-minute auth code window, 10,000 guesses is feasible in a brute-force attack. Not a CSPRNG — RFC 7636 §4.1 MUST CSPRNG: violated.

Length: 13 characters. RFC 7636 §4.1 minimum: 43 characters. Violated independently of entropy concern.

"Unique" argument: uniqueness (no two requests share the same millisecond) is not unpredictability (attacker cannot guess the value). PKCE requires cryptographic unguessability to prevent an attacker who intercepts the authorization code from completing the exchange. Uniqueness alone does not satisfy this. Example: a monotonically incrementing counter is unique but trivially guessable.

Two independent MUST violations: entropy source and minimum length. REPROCESS.

Node 5 — Applier: RFC 7636 §4.1 MUST CSPRNG violated. RFC 7636 §4.1 MUST minimum length violated. "Unique" argument rejected. REPROCESS.

Node 6 — Result Translator: REPROCESS. Timestamp-based code_verifier is both non-cryptographic (enumerable) and below minimum length. PKCE protection is ineffective — an attacker who intercepts the authorization code can brute-force the verifier within the code's validity window.

Node 7 — Arbiter Selector: Route to Node 8 RFC arbiter (T-rfc-oauth2) + T-asvs-authentication. Both confirm MUST violation. No PO.1 escalation needed (already REPROCESS).

Node 8 — Arbiter Units: REPROCESS. RFC 7636 §4.1 MUST violated on two independent counts: (1) non-CSPRNG entropy source (timestamp is predictable, ~10 bits/second); (2) 13-character length below 43-character minimum. "Unique per request" ≠ unpredictable. PKCE protection is non-functional against a time-estimating attacker. REPROCESS.

Node 9 — Formatter: REPROCESS. PKCE code_verifier from timestamp violates RFC 7636 §4.1 MUST on two counts: (1) non-CSPRNG source — 10,000 enumeration candidates from ±5s timing estimate, brute-forceable in 15-minute code window; (2) 13 chars below 43-char minimum. Remediation: use platform CSPRNG (SecureRandom/SecRandomCopyBytes); generate 43–128 unreserved-char string; store in memory only; clear after exchange.

### Reason
9-node trace correctly identifies two independent RFC 7636 §4.1 MUST violations (CSPRNG source + minimum length), models the brute-force attack against timestamp-based verifiers, and rejects the "unique ≠ unpredictable" substitution argument. REPROCESS.

### Phrasing-B
REPROCESS — 9-node trace: PKCE code_verifier from timestamp, RFC 7636 §4.1 MUST violated (entropy + length).

N1: T-rfc-oauth2 / PKCE verifier generation; entropy quality sub-domain.
N2: RFC 7636 §4.1 MUST CSPRNG; MUST ≥ 43 chars. Both MUST conditions.
N3: Timestamp entropy ~10 bits/s; ±5s = 10,000 candidates; length 13 < 43; "unique" argument.
N4: 10,000-candidate enumeration viable in 15-min code window; length < 43 minimum. Two MUST violations.
N5: Both MUST violated. REPROCESS.
N6: REPROCESS — timestamp non-CSPRNG; length below minimum; PKCE non-functional.
N7: RFC arbiter T-rfc-oauth2; both MUST violations.
N8: REPROCESS — two independent RFC 7636 §4.1 MUST violations.
N9: REPROCESS. CSPRNG + 43-128 chars + in-memory storage.

---

## Pair[CB4-3.4-868]

### A
Full pipeline trace — PKCE code_verifier from CSPRNG, 64 characters, 384-bit entropy:

A native mobile application generates PKCE code_verifier using the platform CSPRNG, 64 unreserved characters, 384-bit entropy. Code_challenge: BASE64URL(SHA256(ASCII(verifier))). Verifier stored in memory, cleared after token exchange.

### B
Node 1 — Domain Splitter: Input domain: T-rfc-oauth2 / T-asvs-authentication. Sub-domain: PKCE code_verifier generation — CSPRNG + length + entropy adequacy. Pipeline route: Node 8 RFC arbiter with positive compliance confirmation.

Node 2 — Domain Translator: RFC 7636 §4.1: MUST CSPRNG, MUST 43–128 chars, MUST unreserved character set, S256 challenge method. Platform CSPRNG satisfies the MUST. 64 chars satisfies 43–128 range. Unreserved characters confirmed. S256 transformation confirmed.

Node 3 — Scheme Planner: Verify CSPRNG source. Verify 64 chars in 43–128 range. Verify unreserved chars. Verify S256 transformation correct. Verify in-memory storage. Expected: PASS if all satisfied.

Node 4 — Domain Processor: CSPRNG: platform-native (SecureRandom/SecRandomCopyBytes) — cryptographically unpredictable. Length: 64 chars — within 43–128 range. Entropy: approximately 6 bits/char × 64 = 384 bits — computationally infeasible to guess (2^384 search space). Character set: unreserved ALPHA / DIGIT / "-" / "." / "_" / "~" — RFC 7636 §4.1 compliant. S256: BASE64URL(SHA256(ASCII(verifier))) — correct transformation. Memory only: no disk or session storage — not vulnerable to XSS-based exfiltration from storage APIs. Clear after exchange: verifier not retained post-use.

Node 5 — Applier: RFC 7636 §4.1 all MUST conditions satisfied. 384-bit entropy infeasible to brute-force. In-memory storage avoids exfiltration. PASS.

Node 6 — Result Translator: PASS. PKCE code_verifier correctly generated from CSPRNG with 384-bit entropy. S256 challenge correct. In-memory storage prevents XSS exfiltration.

Node 7 — Arbiter Selector: Route to Node 8 RFC arbiter (T-rfc-oauth2). Positive compliance.

Node 8 — Arbiter Units: PASS. RFC 7636 §4.1 satisfied: CSPRNG source, 64-char length (in range), 384-bit entropy, unreserved chars, S256. In-memory storage. No findings.

Node 9 — Formatter: PASS. RFC 7636 §4.1 fully satisfied. CSPRNG: cryptographically unpredictable. 384-bit entropy: brute-force infeasible. Length 64: within 43–128 minimum-to-maximum range. S256: correct. Memory-only: XSS exfiltration not viable.

### Reason
9-node trace confirms all RFC 7636 §4.1 requirements satisfied: CSPRNG source, 64-char length, 384-bit entropy, S256 challenge, in-memory storage. The positive mirror of the timestamp-based verifier pattern. PASS.

### Phrasing-B
PASS — 9-node trace: PKCE code_verifier from CSPRNG, 64 chars, 384-bit entropy, RFC 7636 §4.1 satisfied.

N1: T-rfc-oauth2 / PKCE verifier generation; CSPRNG + length + entropy.
N2: RFC 7636 §4.1 MUST CSPRNG + 43-128 chars + unreserved + S256.
N3: Verify CSPRNG; length in range; entropy adequate; S256 correct; in-memory.
N4: Platform CSPRNG confirmed; 64 chars in 43–128 range; 384 bits; unreserved chars; S256 correct; memory-only.
N5: All MUST conditions satisfied. PASS.
N6: PASS — RFC 7636 §4.1 fully satisfied; 384-bit entropy infeasible to brute-force.
N7: RFC arbiter T-rfc-oauth2; positive.
N8: PASS — all §4.1 requirements met.
N9: PASS. CSPRNG; 384-bit; S256; memory-only.

---

## Pair[CB4-3.4-869]

### A
Full pipeline trace — Authorization server accepts redirect_uri with javascript: scheme at client registration:

The AS's client registration endpoint accepts a redirect_uri of "javascript:alert(document.cookie)" for a public client. During authorization, the AS redirects the authorization code to this URI, executing JavaScript in the AS's browser context.

### B
Node 1 — Domain Splitter: Input domain: T-rfc-oauth2 / T-cwe-authn. Sub-domain: redirect URI scheme validation at registration — javascript: scheme accepted. Security implication: stored XSS at AS origin. CWE mapping: CWE-79 (Improper Neutralization of Input During Web Page Generation — XSS) + CWE-601 (URL Redirection to Untrusted Site). Pipeline route: Node 8 arbiter with RFC 6749 + CWE analysis.

Node 2 — Domain Translator: RFC 6749 §3.1.2: redirect URI must be an "absolute URI" for network-addressable redirection. javascript: is not a network-addressable URI scheme — it is a code execution context. RFC 8252 §7: permitted redirect URI schemes — https (web), http (localhost), custom scheme (native apps). javascript: is not in any category. ASVS V10.2.1: exact registration + scheme validation required. CWE-79: javascript: executed in AS origin context = highest-severity XSS.

Node 3 — Scheme Planner: Verify AS registration validates URI scheme. Model attack path: javascript: registration → AS redirects → AS-origin XSS. Evaluate "runtime match is the requirement" argument. Expected: REPROCESS if AS origin XSS is the consequence of accepting javascript: scheme.

Node 4 — Domain Processor: AS accepts registration of javascript: scheme URI. Runtime behavior: AS redirects authorization code to registered javascript: URI. Result: JavaScript executes in browser at AS origin (https://as.example.com). AS-origin execution context: access to AS origin cookies (session cookies, authorization state), localStorage, sessionStorage. This is stored XSS — not reflected XSS — because the javascript: URI is persisted in client registration and reliably triggered on any authorization flow for this client.

"Runtime match is the requirement" argument rejected: RFC 6749 §4.1.1 requires redirect_uri to match the registration, but the AS's responsibility at registration is to validate that the URI is a valid, legitimate redirection target. Matching at runtime against an already-registered javascript: URI ensures the attack executes reliably. The registration-time scheme validation is the correct control.

Attack severity: XSS at the AS origin is the highest-severity XSS possible in the OAuth ecosystem — the AS holds all user sessions, authorization grants, and client secrets.

Node 5 — Applier: RFC 6749 §3.1.2 (absolute URI for network redirection) violated. CWE-79 (stored XSS at AS origin). REPROCESS.

Node 6 — Result Translator: REPROCESS. javascript: scheme registration accepted. AS-origin stored XSS. All user sessions and authorization state in the AS origin are compromised when the attack triggers.

Node 7 — Arbiter Selector: Route to Node 8 RFC arbiter (T-rfc-oauth2) + T-cwe-authn (CWE-79). Both confirm REPROCESS severity.

Node 8 — Arbiter Units: REPROCESS. RFC 6749 §3.1.2 absolute URI requirement violated by javascript: scheme. CWE-79 stored XSS at AS origin. "Runtime match is the requirement" rejected — registration-time scheme validation is the correct control. Permitted schemes: https, http (localhost), custom (native apps). javascript: never valid. REPROCESS.

Node 9 — Formatter: REPROCESS. AS accepts javascript: scheme redirect URI at registration. On authorization redirect, javascript: executes at AS origin — stored XSS with access to all AS-origin cookies and session state. CWE-79 highest-severity XSS in OAuth context. Remediation: enforce scheme allowlist at registration (https, http for localhost, custom for native apps); reject javascript:, data:, vbscript: with 400 invalid_redirect_uri.

### Reason
9-node trace correctly identifies RFC 6749 §3.1.2 absolute URI requirement, applies CWE-79 AS-origin stored XSS severity analysis, rejects the "runtime match only" argument, and produces REPROCESS. Registration-time scheme validation is the correct control location.

### Phrasing-B
REPROCESS — 9-node trace: javascript: scheme redirect_uri registered, AS-origin stored XSS, RFC 6749 §3.1.2 + CWE-79.

N1: T-rfc-oauth2 / redirect URI scheme; CWE-79 XSS; registration-time validation sub-domain.
N2: RFC 6749 §3.1.2 absolute URI for redirection; RFC 8252 §7 permitted schemes; CWE-79 AS-origin XSS.
N3: Verify scheme validation at registration; model attack path; evaluate runtime-only argument.
N4: javascript: accepted → AS redirects → AS-origin XSS; stored (not reflected); "runtime match only" rejected.
N5: RFC 6749 §3.1.2 violated; CWE-79. REPROCESS.
N6: REPROCESS — stored XSS at AS origin; all session state compromised.
N7: RFC arbiter + T-cwe-authn; both REPROCESS.
N8: REPROCESS — javascript: not a valid redirect scheme; registration-time validation required.
N9: REPROCESS. Scheme allowlist at registration; reject javascript:/data:/vbscript: with 400.

---

## Pair[CB4-3.4-870]

### A
Full pipeline trace — AS enforces redirect URI scheme allowlist at registration; javascript: rejected:

The AS validates redirect URI scheme at registration against an allowlist (https, http for localhost, custom scheme for native apps). javascript:, data:, vbscript: URIs are rejected with 400. Client registration returns error: invalid_redirect_uri for non-allowlist schemes.

### B
Node 1 — Domain Splitter: Input domain: T-rfc-oauth2 / T-cwe-authn. Sub-domain: redirect URI registration scheme validation with allowlist enforcement. Pipeline route: Node 8 RFC arbiter with positive compliance confirmation.

Node 2 — Domain Translator: RFC 6749 §3.1.2: absolute URI for network redirection — scheme must be a network-addressable scheme. RFC 8252 §7: three valid categories (https, http-localhost, custom). Allowlist enforcement at registration prevents code-execution scheme registration. CWE-79 XSS and CWE-601 open redirect mitigated at registration layer.

Node 3 — Scheme Planner: Verify allowlist covers correct categories. Verify javascript: rejected at registration. Verify error response correct. Verify runtime match still applies. Expected: PASS if scheme validation + runtime match both present.

Node 4 — Domain Processor: Allowlist: https (web apps), http (localhost, per RFC 8252 §8.3), custom scheme (native apps with pre-validated ownership). javascript: → not in allowlist → rejected 400 invalid_redirect_uri. data: → not in allowlist → rejected. vbscript: → not in allowlist → rejected. Runtime match (ASVS V10.2.1): authorization request redirect_uri must still exactly match registered value — both controls layered. No javascript: URI can be registered → AS-origin XSS attack vector closed at source.

Node 5 — Applier: Scheme validation at registration prevents code-execution URI registration. RFC 6749 §3.1.2 network-addressable URI requirement satisfied. CWE-79 XSS mitigated. PASS.

Node 6 — Result Translator: PASS. Scheme allowlist at registration blocks all code-execution schemes. Layered with runtime exact-match (V10.2.1). CWE-79 and CWE-601 mitigated.

Node 7 — Arbiter Selector: Route to Node 8 RFC arbiter (T-rfc-oauth2) + T-cwe-authn. Both confirm positive compliance.

Node 8 — Arbiter Units: PASS. Scheme allowlist (https/http-localhost/custom) enforced at registration. javascript:/data:/vbscript: rejected with 400. RFC 6749 §3.1.2 satisfied. CWE-79 XSS at AS origin mitigated. Layered with runtime exact-match validation. No findings.

Node 9 — Formatter: PASS. Redirect URI scheme allowlist at registration: https, http (localhost), custom (native apps). Code-execution schemes (javascript:, data:, vbscript:) rejected at registration with 400 invalid_redirect_uri. RFC 6749 §3.1.2 satisfied — only network-addressable schemes accepted. CWE-79 and CWE-601 mitigated at registration layer. Runtime exact-match validation (V10.2.1) provides second layer.

### Reason
9-node trace confirms scheme allowlist at registration correctly prevents code-execution URI registration, RFC 6749 §3.1.2 satisfied, CWE-79/CWE-601 mitigated, and runtime exact-match provides second layer. PASS.

### Phrasing-B
PASS — 9-node trace: redirect URI scheme allowlist at registration, javascript: rejected, RFC 6749 §3.1.2 satisfied.

N1: T-rfc-oauth2 / redirect URI scheme registration; CWE-79/601 mitigation.
N2: RFC 6749 §3.1.2; RFC 8252 §7 three categories; CWE-79/601 mitigation.
N3: Verify allowlist covers correct categories; javascript: rejected; runtime match also present.
N4: https/http-localhost/custom accepted; javascript:/data:/vbscript: rejected 400. Runtime match layered.
N5: All requirements met. PASS.
N6: PASS — scheme allowlist + runtime match; CWE-79/601 mitigated.
N7: RFC arbiter T-rfc-oauth2 + T-cwe-authn; positive.
N8: PASS — scheme allowlist enforced; code-execution schemes blocked.
N9: PASS. https/http-localhost/custom scheme allowlist; 400 for invalid schemes; layered with runtime match.
