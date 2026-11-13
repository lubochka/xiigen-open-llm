# CB4 Stage 3 — Action 3 Extended Pair Set
# Node: Node 8 (Arbiter Units) — ext35
# Sequences: CB4-3.4-907 through CB4-3.4-914
# Instances: 16 (8 base pairs × 2 phrasings each)
# Stratum: A/B/D (new domains: mobile client_secret hardcoding, token_type field, JWT PII in payload, confidential client PKCE)
# Running total after this file: 1828 instances (914 base pairs)

---

## Pair[CB4-3.4-907]

### A
Mobile application with hardcoded client_secret embedded in the app binary:

A native mobile OAuth client (iOS and Android apps) is registered as a confidential client with a client_secret. The client_secret is hardcoded in the app's source code as a string constant and compiled into the distributed binary. The team applies code obfuscation to the binary and argues: "The secret is obfuscated — reverse-engineering the binary requires significant effort. We need the secret for server-side API calls made from the mobile app."

### B
REPROCESS. ASVS V9.2.1 states: "Verify that the application does not contain hardcoded credentials." CWE-798 (Use of Hardcoded Credentials) classifies this as a critical vulnerability. Obfuscation is not an acceptable substitute for credential security.

Native mobile applications are inherently public clients. When a client_secret is compiled into a distributed mobile application binary, it is effectively a public secret: (1) The same binary is distributed to all users — any user can extract the binary and reverse-engineer it. (2) Tools like jadx (Android), Hopper, and IDA Pro can reverse-engineer compiled binaries with varying effort depending on obfuscation quality. (3) Obfuscation is a deterrent, not a security control — it raises the cost of extraction but does not make extraction infeasible. Security MUST requirements cannot be satisfied by "making extraction harder."

The correct architecture: native mobile apps MUST be registered as public clients (no client_secret). RFC 8252 (OAuth 2.0 for Native Apps) establishes that native apps are public clients and must use PKCE for code exchange protection instead of a client_secret. A public client with PKCE S256 provides cryptographic proof-of-possession without distributing a secret.

ASVS V9.2.1 MUST no hardcoded credentials: violated. CWE-798: confirmed. REPROCESS.

### Reason
ASVS V9.2.1 MUST no hardcoded credentials. CWE-798. Mobile binaries are distributed to users — any hardcoded secret is extractable regardless of obfuscation. RFC 8252: native apps are public clients; use PKCE, not client_secret. REPROCESS.

### Phrasing-B
REPROCESS — hardcoded client_secret in mobile binary. ASVS V9.2.1 MUST, CWE-798. Obfuscation rejected as security control.

client_secret compiled into iOS/Android binary. Obfuscation applied. All users receive same binary → extractable by any user with reverse-engineering tools (jadx/Hopper/IDA).

"Obfuscation requires significant effort" rejected: deterrent ≠ security control. MUST cannot be satisfied by raising attacker cost — it requires credential security.

Correct architecture: RFC 8252 — native apps are public clients; register with no client_secret; use PKCE S256 for code exchange. ASVS V9.2.1 MUST: violated. CWE-798. REPROCESS.

---

## Pair[CB4-3.4-908]

### A
Mobile application registered as public client using PKCE S256; no client_secret:

A native mobile OAuth client (iOS and Android) is registered as a public client with no client_secret. For each authorization code flow, the app generates a PKCE code_verifier (CSPRNG, 64 chars) and sends code_challenge_method=S256 to the authorization server. The authorization server enforces PKCE for all public clients. No client_secret exists to be hardcoded or distributed.

### B
PASS. RFC 8252 (OAuth 2.0 for Native Apps) public client architecture satisfied. ASVS V9.2.1 hardcoded credentials risk eliminated by design.

Public client registration: no client_secret — eliminates the risk of distributing a secret in a binary. Per RFC 8252 §8.4: native apps should be registered as public clients. No credential exists to be hardcoded, leaked, or reverse-engineered from the binary.

PKCE S256: CSPRNG 64-char code_verifier provides cryptographic proof-of-possession at token exchange. RFC 7636 §4.1 and RFC 9700 §2.1.1 requirements satisfied. Code interception attacks require possession of both the code and the verifier — the verifier is generated per-request and never transmitted before the exchange.

RFC 8252 §7.2: claimed HTTPS redirect URIs recommended for iOS (Universal Links) and Android (App Links) — operating-system-level verified redirect URI handling. No javascript: or custom-scheme redirect URI vulnerability.

ASVS V9.2.1: no hardcoded credentials because no credentials exist. CWE-798: not applicable by design. No findings. PASS.

### Reason
RFC 8252 public client architecture with PKCE S256. No client_secret eliminates hardcoded credential risk entirely. PKCE provides code exchange protection. ASVS V9.2.1 satisfied by design (no credentials to hardcode). PASS.

### Phrasing-B
PASS — Mobile app as public client with PKCE S256. No client_secret. ASVS V9.2.1 satisfied by design.

Public client: no client_secret registered. RFC 8252 §8.4: native apps as public clients. Hardcoded credential risk: eliminated by design — no credential exists.
PKCE S256: CSPRNG 64-char verifier; RFC 7636 §4.1 + RFC 9700 §2.1.1. Code interception requires both code and verifier.
CWE-798: not applicable — no credentials in binary. PASS.

---

## Pair[CB4-3.4-909]

### A
Token endpoint successful response does not include token_type field:

An authorization server's token endpoint returns successful responses containing access_token and expires_in but omits the token_type field. Team: "All our tokens are Bearer tokens — clients know from our documentation what type to use. Explicitly sending token_type is redundant given our documentation."

### B
REPROCESS. RFC 6749 §5.1 states: "The authorization server MUST include the HTTP response parameters: access_token, token_type." token_type is a MUST field in the successful token response — it is not optional.

The token_type field serves a functional purpose beyond documentation: it allows clients to determine programmatically how to use the received token without requiring out-of-band knowledge of the token type. RFC 6750 §6.1.1 registers "Bearer" as the token type for OAuth 2.0 Bearer Token usage. Other token types exist (e.g., DPoP — RFC 9449; MAC — expired but referenced; N_A for client_credentials). A client that receives a token_type can programmatically construct the correct Authorization header scheme without relying on documentation.

The "clients know from our documentation" argument is rejected: documentation is an out-of-band mechanism. RFC 6749 §5.1 requires the token response to be self-describing by including token_type. A client that correctly follows RFC 6749 §5.1 would treat a response without token_type as an error or undefined behavior.

Additional impact: without token_type, clients using generic OAuth libraries may not correctly format authorization headers, may use incorrect header schemes, or may fail silently. The omission also makes the server non-conformant with RFC 6749, which affects interoperability with standards-compliant clients.

RFC 6749 §5.1 MUST token_type: violated. REPROCESS.

### Reason
RFC 6749 §5.1 MUST token_type in successful token response. Documentation cannot substitute for required response field. token_type allows clients to programmatically determine how to use the token. REPROCESS.

### Phrasing-B
REPROCESS — token endpoint response missing token_type. RFC 6749 §5.1 MUST field absent.

RFC 6749 §5.1: MUST include access_token AND token_type in successful response. token_type = "Bearer" (RFC 6750), "DPoP" (RFC 9449), or other registered type. Enables client to programmatically construct correct Authorization header.

"Documentation substitutes" rejected: RFC 6749 §5.1 requires self-describing response; out-of-band documentation is not equivalent to a required response field. Standards-conformant clients expect token_type; omission is non-conformance and potential interoperability failure. REPROCESS.

---

## Pair[CB4-3.4-910]

### A
Token endpoint response includes token_type=Bearer and all required fields:

The authorization server's token endpoint returns successful responses with: access_token, token_type=Bearer, expires_in, refresh_token (where applicable), and scope (where different from requested per RFC 6749 §3.3 SHOULD). All MUST fields from RFC 6749 §5.1 are present.

### B
PASS. RFC 6749 §5.1 MUST required fields satisfied in token endpoint responses.

access_token: present — the issued access token. token_type: "Bearer" — correct RFC 6750 registration for standard Bearer tokens. RFC 6749 §5.1 MUST: satisfied. expires_in: present — clients can proactively refresh before expiry. refresh_token: present where applicable (authorization_code and password grants — the latter excluded per ROPC prohibition). scope: present when different from requested per RFC 6749 §3.3 SHOULD — allows clients to detect scope narrowing by the AS.

Self-describing response: clients receive all necessary information from the response itself without requiring documentation. Standards-conformant OAuth clients can correctly handle the response. Interoperability maintained with generic OAuth libraries.

RFC 6749 §5.1 MUST fields: all present. No findings. PASS.

### Reason
RFC 6749 §5.1 satisfied: access_token, token_type=Bearer, expires_in, refresh_token (where applicable), scope (where narrowed). Self-describing response. PASS.

### Phrasing-B
PASS — token endpoint response includes all RFC 6749 §5.1 MUST fields. token_type=Bearer.

access_token present; token_type=Bearer (RFC 6750); expires_in present; refresh_token where applicable; scope when narrowed (§3.3 SHOULD). RFC 6749 §5.1 MUST satisfied. Self-describing response; no out-of-band documentation required for correct client behavior. PASS.

---

## Pair[CB4-3.4-911]

### A
JWT access token contains user email in a custom claim without encryption:

An authorization server issues JWT access tokens where the payload includes a custom claim "email": "user@example.com" alongside standard claims (sub, iss, aud, exp, iat, jti, scope). The JWT is signed but not encrypted (not a JWE). The resource server receives and logs these tokens. Team: "Including email in the token helps the RS perform user lookup without an extra DB query. Tokens are only transmitted over HTTPS."

### B
CONCERN. ASVS V9.3.1 and general PII-in-token guidance recommend against including PII in unencrypted tokens. The "HTTPS protects in transit" argument was established in ext20 as insufficient — tokens exist in contexts beyond transit.

JWT payloads are BASE64URL-encoded, not encrypted. Any party that holds the token (the RS, log aggregators, monitoring systems, debug interfaces, error reporting tools) can decode the payload and read the email address. Signed but unencrypted JWTs are transparent to all bearers.

Logging risk: the RS is described as logging tokens. If the JWT is logged, the user's email address enters the log storage pipeline — violating ASVS V8.2.2 (do not log PII in production logs) and creating a secondary exposure surface for the PII.

HTTPS protects transit only: the email in the JWT payload is exposed at any endpoint that stores, logs, or processes the token — not just in transit. localStorage (ext34 pattern), browser developer tools, server-side logs, and proxy logs all expose the payload.

However, there is no MUST-level requirement that specifically mandates JWT encryption for access tokens containing PII. ASVS V9.3.1 addresses token transport; JWE is recommended for PII-containing tokens (ext20 established this). This is SHOULD/guidance-level. The correct remediation is either to remove the email claim from the access token (use opaque sub and a dedicated UserInfo call) or to use JWE. CONCERN.

### Reason
ASVS V9.3.1 / PII-in-token guidance: JWT payload is transparent to all bearers; logging risk introduces V8.2.2 concern. "HTTPS protects in transit" rejected (ext20 pattern). SHOULD-level: CONCERN.

### Phrasing-B
CONCERN — JWT access token contains email in unencrypted payload. ASVS V9.3.1 PII guidance. Logging risk introduces V8.2.2 concern.

JWT: BASE64URL-encoded, not encrypted. Any token holder (RS, log aggregator, monitoring) can decode and read email. HTTPS protects transit only — payload exposed in storage, logs, debug tools.

RS logs tokens → email enters log pipeline → V8.2.2 concern (PII in logs). "HTTPS protects in transit" rejected: ext20 established that token payload exposure extends beyond transit.

No MUST explicitly mandating JWE for PII-in-access-token. SHOULD/guidance-level: CONCERN. Remediation: remove email from access token (use sub PPID + UserInfo for email); or use JWE for encrypted payload.

---

## Pair[CB4-3.4-912]

### A
JWT access tokens contain only standard non-PII claims:

JWT access tokens contain: sub (opaque UUID v4 PPID), iss, aud, exp, iat, jti (UUID v4), and scope. No username, email, phone, real name, or other PII is included in the token payload. Resource servers that need user profile information call the UserInfo endpoint with the access token.

### B
PASS. JWT access token payload contains no PII. ASVS V9.3.1 privacy guidance satisfied. V8.2.2 logging risk eliminated.

sub: opaque UUID v4 PPID — identifies the user without exposing any PII. All other claims (iss, aud, exp, iat, jti, scope) are non-PII operational claims. The token payload can be logged, stored, or processed by any party without creating a PII exposure risk.

RS lookup pattern: RSes that need user profile attributes call the UserInfo endpoint directly (OIDC Core §5.3). This separates the authorization credential (access token) from user profile data — authorization state does not carry PII; profile data is fetched only when needed, only with the appropriate scope-filtered claims.

V8.2.2 logging: logging JWT access tokens in operational logs does not create a PII exposure event when the payload contains no PII. JWE not required: payload transparency is not a privacy risk without PII content.

ASVS V9.3.1 and PII-in-token guidance satisfied. No findings. PASS.

### Reason
JWT access token payload contains only standard non-PII claims. No email, name, phone, or other PII. V9.3.1 privacy guidance satisfied. V8.2.2 logging risk eliminated. RS calls UserInfo for profile data. PASS.

### Phrasing-B
PASS — JWT access token with non-PII claims only (sub PPID, iss, aud, exp, iat, jti, scope). ASVS V9.3.1 satisfied.

sub: opaque UUID v4 PPID — no PII. All claims non-PII operational data. Token payload transparency not a privacy risk without PII content. V8.2.2: token logging creates no PII exposure event. JWE not required. RS calls UserInfo for profile claims (OIDC §5.3). PASS.

---

## Pair[CB4-3.4-913]

### A
Authorization code flow for confidential client without PKCE:

A confidential client (server-side web application) uses the authorization code flow without including a code_challenge parameter. The client authenticates at the token endpoint with client_secret_post. Team: "We're a confidential client with server-side execution — our client_secret protects the token exchange. PKCE is designed for public clients that lack a secret."

### B
CONCERN. RFC 9700 §2.1.1 states: "Authorization servers SHOULD require PKCE for confidential clients as well." SHOULD-level guidance. The team's argument has a legitimate historical basis but is overridden by current best practice.

Historical context: in RFC 7636 (PKCE), PKCE was designed primarily for public clients. RFC 6749 relied on client_secret authentication for confidential clients. However, RFC 9700 §2.1.1 extends the PKCE recommendation to confidential clients as well.

The client_secret argument: client_secret authenticates the client at the token endpoint. However, it does not protect against authorization code interception between the authorization endpoint and the redirect URI. An attacker who intercepts the authorization code (from referrer headers, browser history, server-side logs, or network interception on the redirect) can attempt to exchange it at the token endpoint using the legitimate client's client_id without the client_secret (if the AS has any weakness) or by registering a different confidential client with the same client_id pattern (in multi-tenant environments). PKCE binds the code to the legitimate client's cryptographic proof regardless of secret.

However, RFC 9700 §2.1.1 uses SHOULD for confidential clients — not MUST (which applies to public clients). The client_secret provides meaningful protection that does not exist for public clients. The gap is SHOULD-level. CONCERN.

### Reason
RFC 9700 §2.1.1 SHOULD PKCE for confidential clients as well. Client_secret authenticates at token endpoint but does not prevent authorization code interception. SHOULD-level: CONCERN not REPROCESS.

### Phrasing-B
CONCERN — Confidential client authorization code flow without PKCE. RFC 9700 §2.1.1 SHOULD.

RFC 9700 §2.1.1: "Authorization servers SHOULD require PKCE for confidential clients as well." SHOULD not MUST. Client_secret authenticates at token endpoint — partial mitigation. PKCE adds: prevents code interception attacks independent of client_secret; binds code to cryptographic proof held only by the legitimate client.

"PKCE is for public clients" partially accepted historically but overridden by RFC 9700 §2.1.1 SHOULD guidance. SHOULD-level gap: CONCERN not REPROCESS.

---

## Pair[CB4-3.4-914]

### A
Confidential client uses authorization code flow with PKCE S256:

A confidential server-side web application uses the authorization code flow with PKCE S256 (code_challenge_method=S256). The authorization server requires PKCE for both public and confidential clients. The client also authenticates at the token endpoint with client_secret_post. Both protections are layered.

### B
PASS. RFC 9700 §2.1.1 SHOULD PKCE for confidential clients satisfied. Layered protection: PKCE + client_secret at token endpoint.

PKCE S256: code_challenge_method=S256 with CSPRNG code_verifier. RFC 7636 §4.1 and RFC 9700 §2.1.1 requirements satisfied. Authorization code interception attacks require the attacker to also possess the code_verifier — which is held only by the legitimate client and not transmitted until the token exchange. Even if the authorization code is intercepted, it cannot be exchanged without the code_verifier.

client_secret at token endpoint: authenticates the client to the AS. Combined with PKCE: the token exchange requires both (a) the client's secret (proving client identity) and (b) the code_verifier (proving that the code was issued to this client's specific request). Neither alone provides both authentication and binding.

AS enforces PKCE for confidential clients: AS-side enforcement ensures no confidential client can skip PKCE by omitting the code_challenge. This is the defense-in-depth principle: enforcement at both the client and AS layers.

RFC 9700 §2.1.1 SHOULD: satisfied at above-SHOULD level (SHOULD fully implemented with AS enforcement). No findings. PASS.

### Reason
RFC 9700 §2.1.1 SHOULD PKCE for confidential clients satisfied. Layered: PKCE S256 + client_secret at token endpoint. AS enforces for both public and confidential clients. No findings. PASS.

### Phrasing-B
PASS — Confidential client with authorization code + PKCE S256 + client_secret. RFC 9700 §2.1.1 SHOULD satisfied; layered protection.

PKCE S256: CSPRNG verifier; code interception requires verifier possession. RFC 9700 §2.1.1 SHOULD above-SHOULD. client_secret: token endpoint client authentication. Layered: client_secret proves identity; PKCE proves binding. AS enforces for both client types. PASS.
