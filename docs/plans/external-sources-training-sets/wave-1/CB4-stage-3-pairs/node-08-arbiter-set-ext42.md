# CB4 Stage 3 — Node 08 Arbiter Set ext42
# Sub-phase 3.4 · Security and Authorization Model
# Sequences CB4-3.4-1089 through CB4-3.4-1096 (8 base pairs × 2 phrasings = 16 instances)
# New domains: dpop-cnf-jkt-issuance, oidc-iss-https, jwks-rollover-window, revocation-400-error, redirect-uri-binding, jwks-use-absent, introspection-pii, user-enumeration-login-hint
# Running total after this file: 120 + (1096 − 60) × 2 = 2192 instances (1096 base pairs)
# Sources: RFC 9449 §6, OIDC Core §2, ASVS V9.2, RFC 7009 §2.2, RFC 6749 §4.1.3, RFC 7517 §4.2, RFC 7662 §2.2, ASVS V6.1/CWE-204

---

## Pair[CB4-3.4-1089-A]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** DPoP used at token endpoint but issued access token contains no `cnf/jkt` claim
**Expected verdict:** REPROCESS

**A:**
A client presents a valid DPoP (Demonstrating Proof of Possession, RFC 9449) proof when calling the token endpoint — the DPoP proof includes a correct signature, htm (`POST`), htu (token endpoint URI), iat, and jti. The AS validates the DPoP proof and issues an access token, but the issued access token is a standard JWT without a `cnf` claim — no `cnf/jkt` (JWK (JSON Web Key) thumbprint) is embedded. The RS validates the token as a plain bearer token (no DPoP binding enforced). AS rationale: "DPoP proof at the token endpoint authenticates the client; the issued access token doesn't need to carry the binding forward."

**B:**
REPROCESS. RFC 9449 §6 is explicit: "If the DPoP proof is valid for the token endpoint..., the access token issued MUST be bound to the public key in the DPoP proof, by including a `cnf` claim in the access token that has a `jkt` claim with the value being the SHA-256 hash of the public key used in the DPoP proof." This is an unconditional MUST. The purpose of DPoP is sender-constrained tokens: the RS must be able to verify, at the time of API call, that the client presenting the token holds the private key corresponding to the public key committed at issuance. If the AS does not embed `cnf/jkt` in the access token, the RS has no information needed to enforce DPoP binding — the token degrades to plain bearer semantics. A stolen token is freely usable by any party with no cryptographic constraint. The "DPoP at token endpoint authenticates the request" argument is rejected: that is authentication for the issuance request; `cnf/jkt` in the token is what enables RS-side enforcement.

**Reason:** RFC 9449 §6 MUST: AS must embed cnf/jkt in access token when DPoP proof is presented at token endpoint. Absent cnf/jkt means RS cannot enforce DPoP binding — token degrades to plain bearer. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 9449 §6 MUST: access token MUST contain cnf/jkt (SHA-256 thumbprint of DPoP public key) when DPoP proof presented at token endpoint. Without cnf/jkt in AT, RS cannot bind token to key — DPoP sender-constraint nullified; stolen AT usable as plain bearer. "DPoP at issuance = binding at use" rejected; cnf/jkt is the mechanism. REPROCESS.

---

## Pair[CB4-3.4-1089-B]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** AS embeds `cnf/jkt` in access token when DPoP proof is presented at token endpoint
**Expected verdict:** PASS

**A:**
When a client presents a valid DPoP proof at the token endpoint, the AS: (1) validates the DPoP proof (signature, htm, htu, iat, jti); (2) extracts the public key from the DPoP proof's JOSE header; (3) computes the SHA-256 thumbprint of that public key (JWK Thumbprint per RFC 7638); (4) embeds `"cnf": {"jkt": "<base64url-thumbprint>"}` in the issued JWT access token. The RS validates the `cnf/jkt` value against the DPoP proof public key on every API call per RFC 9449 §7.2. Clients that do not present a DPoP proof receive a standard bearer token without `cnf`.

**B:**
PASS. RFC 9449 §6 MUST satisfied: cnf/jkt embedded in access token when DPoP proof presented at token endpoint. RS can verify sender constraint on every API call. Plain bearer tokens (no DPoP) correctly issued without cnf. No violations found.

**Reason:** RFC 9449 §6 MUST satisfied. cnf/jkt embedded in AT; RS can verify DPoP binding. Sender-constraint fully operational. PASS.

**Phrasing-B:**
PASS. AS computes JWK thumbprint from DPoP proof public key; embeds cnf/jkt in JWT access token per RFC 9449 §6 MUST. RS validates cnf/jkt against DPoP proof on each request. Sender-constraint preserved end-to-end. PASS.

---

## Pair[CB4-3.4-1090-A]

**Territory:** T-oauth-oidc · T-rfc-jwt
**Domain:** OIDC `id_token` — `iss` claim uses HTTP URI instead of HTTPS
**Expected verdict:** REPROCESS

**A:**
An OIDC (OpenID Connect) Authorization Server issues `id_tokens` where the `iss` (issuer) claim is `http://auth.example.com` (HTTP). The AS is deployed with TLS on port 443 and is reachable via `https://auth.example.com`, but the `iss` claim in issued id_tokens uses the HTTP URL. The developer explanation: "Both URLs resolve to the same server. HTTPS is used for all actual connections. The issuer identifier in the token is just a string identifier, not a live URL — HTTP vs HTTPS in the string doesn't affect security."

**B:**
REPROCESS. OIDC Core §2 specifies: "iss: REQUIRED. Issuer Identifier for the Issuer of the response. The iss value is a case sensitive URL using the https scheme that contains scheme, host, and optionally, port number and path components and no query or fragment components." This is a normative requirement: the issuer identifier MUST use the `https` scheme. Additionally, OIDC Discovery §3: the issuer identifier in the discovery document (and therefore in issued tokens) MUST be an HTTPS URL. The downstream consequence: the RP validates the `iss` claim against a pre-configured expected issuer value — if the expected value is `https://auth.example.com` and the token contains `http://auth.example.com`, validation fails (case-sensitive exact comparison). More critically: HTTP issuer identifiers do not receive the integrity guarantees of HTTPS discovery; the OpenID Connect discovery document at `http://.../.well-known/openid-configuration` is unprotected and can be substituted by on-path attackers. The "just a string" argument is rejected: OIDC Core §2 explicitly requires the https scheme.

**Reason:** OIDC Core §2 MUST: iss claim in id_token must use https scheme. HTTP iss violates normative requirement; breaks RP issuer validation; discovery integrity not guaranteed. REPROCESS.

**Phrasing-B:**
REPROCESS. OIDC Core §2 MUST: iss value must be a case-sensitive URL using the https scheme. id_token iss=http://... violates this requirement. RP issuer comparison against configured https:// value fails; HTTP discovery document is on-path-MITM-able. "Both resolve to same server" and "just a string" rejected. REPROCESS.

---

## Pair[CB4-3.4-1090-B]

**Territory:** T-oauth-oidc · T-rfc-jwt
**Domain:** OIDC `id_token` — `iss` claim uses HTTPS URI matching discovery document
**Expected verdict:** PASS

**A:**
The OIDC AS issues `id_tokens` with `iss` set to `https://auth.example.com` (HTTPS scheme). The issuer identifier matches the `issuer` field in the OIDC Discovery document at `https://auth.example.com/.well-known/openid-configuration`. RPs are configured with the issuer `https://auth.example.com` and perform exact case-sensitive comparison against the `iss` claim in received id_tokens. The discovery document and JWKS endpoint are both served over HTTPS with valid TLS certificates.

**B:**
PASS. OIDC Core §2 MUST satisfied: iss uses HTTPS scheme. Issuer matches OIDC Discovery document per §4.3 issuer binding requirement. RP can validate iss claim via exact comparison against configured value. No violations found.

**Reason:** OIDC Core §2 MUST satisfied. iss uses HTTPS scheme; matches discovery issuer. RP validation operational. PASS.

**Phrasing-B:**
PASS. id_token iss=https://auth.example.com per OIDC Core §2 MUST. Discovery document issuer matches token iss exactly. RP exact-comparison validation operational. PASS.

---

## Pair[CB4-3.4-1091-A]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** JWKS rollover window shorter than maximum access token TTL — valid tokens rejected after rotation
**Expected verdict:** CONCERN

**A:**
An Authorization Server rotates JWKS (JSON Web Key Set) signing keys quarterly. The key rollover procedure: (1) generate new signing key pair; (2) publish new key in JWKS immediately; (3) begin signing new tokens with the new key; (4) remove old key from JWKS after 30 minutes. The maximum access token TTL is 1 hour. An access token issued 50 minutes before the rollover, signed with the old key, will be presented at the RS 10 minutes after the rollover — at which point the old key has been removed from JWKS after 30 minutes, and the RS receives a key-not-found result. The RS rejects the still-valid token. The team's rollover procedure was designed for the typical 15-minute token TTL but was not updated when the TTL was increased.

**B:**
CONCERN. There is no RFC MUST specifying a minimum JWKS key retention window. RFC 7517 does not specify key overlap duration. ASVS V9.2 guidance states: "Verify that signing keys are rotated with sufficient overlap to prevent disruption to active tokens." The gap here is a design oversight: the 30-minute overlap is shorter than the 1-hour maximum token TTL, meaning tokens that are still valid can be rejected at the RS due to key unavailability. This is not a security vulnerability in the traditional sense (the old key being retained longer does not weaken security) but an operational correctness gap that causes legitimate token rejections. The MUST-level violation would require either: (a) a specific numeric MUST in an RFC (absent); or (b) a security failure (not present here — this is an availability concern). ASVS V9.2 guidance not fully satisfied. CONCERN. Correct practice: key overlap window ≥ maximum AT TTL (1 hour minimum in this case).

**Reason:** No RFC MUST for minimum JWKS overlap duration. ASVS V9.2 guidance: "sufficient overlap" not met (30-min window < 1-hour max TTL). Operational availability concern, not security failure. CONCERN.

**Phrasing-B:**
CONCERN. ASVS V9.2 guidance: JWKS overlap window must be sufficient to cover active token lifetimes. 30-minute window < 1-hour max TTL → valid tokens rejected post-rotation. No RFC MUST for specific overlap duration — guidance gap only. CONCERN. Correct practice: overlap window ≥ max AT TTL; review rollover procedure when TTL changes.

---

## Pair[CB4-3.4-1091-B]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** JWKS rollover window exceeds maximum access token TTL — no valid token rejected during rotation
**Expected verdict:** PASS

**A:**
The Authorization Server's JWKS rollover procedure: (1) generate new key pair; (2) publish new key in JWKS alongside old key; (3) begin signing new tokens with the new key (old key kept for verification only); (4) remove old key from JWKS after 2 hours (overlap window = 2 × max AT TTL of 1 hour). During the 2-hour overlap, both old and new keys are available in the JWKS — any token signed with the old key that was validly issued within the TTL window can still be verified. The rollover procedure is documented and tested quarterly. The overlap duration is reviewed whenever the maximum AT TTL changes.

**B:**
PASS. ASVS V9.2 guidance satisfied: JWKS overlap window (2 hours) exceeds maximum AT TTL (1 hour) — no validly-issued active token will be rejected during key rotation. Dual-key JWKS during transition ensures continuity. Rollover procedure is documented and TTL-aware. No violations found.

**Reason:** ASVS V9.2 guidance satisfied. Overlap window (2h) > max AT TTL (1h). No active tokens rejected during rotation. PASS.

**Phrasing-B:**
PASS. JWKS rollover maintains old key for 2h (>1h max AT TTL) per ASVS V9.2 guidance. Dual-key transition: new tokens signed with new key; old tokens verified with old key until TTL expiry. No valid token rejected during rotation. PASS.

---

## Pair[CB4-3.4-1092-A]

**Territory:** T-rfc-oauth2 · T-asvs-token
**Domain:** Token revocation endpoint returns 400 for unrecognized or already-expired tokens
**Expected verdict:** REPROCESS

**A:**
A token revocation endpoint (RFC 7009) returns `HTTP 400 Bad Request` with `error=invalid_token` when a client presents a token that: (a) was not issued by this AS, or (b) has already expired, or (c) was already revoked. The AS rationale: "If the token is invalid, we should tell the client. Returning 200 for invalid tokens is misleading — 400 tells the client something was wrong."

**B:**
REPROCESS. RFC 7009 §2.2 is explicit: "The authorization server responds with HTTP status code 200 if the token has been revoked successfully or if the client submitted an invalid token." The specification intentionally requires 200 for invalid tokens — this is not a bug but a deliberate design decision. The reasons: (1) the revocation endpoint must not allow clients to probe whether tokens are valid (a 400 for "invalid" vs 200 for "revoked" enables token validity enumeration); (2) a client's goal is "this token should not be usable" — that goal is achieved whether the token was actively revoked or was never valid; (3) the response semantics are "the presented token will not be honored" (200), not "the token was validly created and then invalidated" (which would be a subset of 200 cases). The "200 is misleading" argument is rejected: RFC 7009 §2.2 defines 200 as the correct status for all success cases including invalid tokens.

**Reason:** RFC 7009 §2.2 MUST: AS must return 200 for token revocation whether token is valid, invalid, or already expired/revoked. 400 for invalid token violates this requirement and enables token validity enumeration. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 7009 §2.2 MUST: revocation endpoint returns 200 for both successful revocation AND invalid/unrecognized tokens. Returning 400 for invalid tokens enables clients to probe token validity (enumeration). "200 is misleading" rejected — RFC 7009 §2.2 defines this behavior intentionally. REPROCESS.

---

## Pair[CB4-3.4-1092-B]

**Territory:** T-rfc-oauth2 · T-asvs-token
**Domain:** Token revocation endpoint returns 200 for all revocation requests regardless of token validity
**Expected verdict:** PASS

**A:**
The token revocation endpoint returns `HTTP 200 OK` with an empty body for all revocation requests where client authentication succeeds — whether the presented token was valid and was actively revoked, whether the token was expired, whether the token was already revoked, or whether the token was not found in the AS's records. The response does not distinguish between these cases. Authentication failures (invalid client credentials) return 401. Authorization failures (authenticated client revoking another client's token) return 403 per RFC 7009 §2.2.

**B:**
PASS. RFC 7009 §2.2 MUST satisfied: uniform 200 response for all token revocation outcomes (active revocation, invalid token, already-expired token). Prevents token validity enumeration through revocation responses. Authentication and authorization failures correctly return 401/403. No violations found.

**Reason:** RFC 7009 §2.2 MUST satisfied. Uniform 200 for all revocation outcomes. Token validity enumeration via revocation response prevented. PASS.

**Phrasing-B:**
PASS. Revocation endpoint returns 200 for all outcomes (revoked, invalid, expired, not-found) per RFC 7009 §2.2. No distinguishable response for "token was valid" vs "token was never valid." Token validity enumeration prevented. PASS.

---

## Pair[CB4-3.4-1093-A]

**Territory:** T-rfc-oauth2 · T-asvs-token
**Domain:** Authorization code not bound to `redirect_uri` at issuance — any registered redirect_uri accepted at token exchange
**Expected verdict:** REPROCESS

**A:**
An Authorization Server processes authorization requests that include a `redirect_uri` parameter. The AS validates the redirect_uri against the client's registered URIs at authorization request time but does not store the specific redirect_uri used with the issued authorization code. At the token endpoint, the client includes a different registered redirect_uri (different from the one used in the authorization request) and the AS accepts it — no comparison is made against the redirect_uri from the original authorization request. The AS rationale: "We validate redirect_uri at authorization time. Both are registered URIs. Validating again at token time seems redundant."

**B:**
REPROCESS. RFC 6749 §4.1.3 is normative: "redirect_uri: REQUIRED, if the redirect_uri parameter was included in the authorization request as described in Section 4.1.1, and their values MUST be identical." The verification at token exchange is not redundant — it serves a distinct security purpose: ensuring that the client presenting the code at the token endpoint is the same client that initiated the authorization request (using the same redirect_uri it registered as the callback for that specific request). Without this check, an attacker who obtains an authorization code can exchange it using any of the victim client's registered redirect_uris, not just the one used in the original request. This enables code injection attacks where a stolen code from one redirect_uri flow can be exchanged using a different redirect_uri. The "both are registered" argument is rejected: registration membership is a necessary but not sufficient condition; the specific value must match the original request.

**Reason:** RFC 6749 §4.1.3 MUST: if redirect_uri was in authorization request, exact same value MUST be present and identical at token exchange. Accepting any registered URI enables code injection via redirect_uri substitution. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 6749 §4.1.3 MUST: redirect_uri at token exchange must exactly match the redirect_uri from the original authorization request. Storing only registered membership (not the specific URI used) enables code injection: stolen code exchangeable with any registered redirect_uri. "Validated at auth time" rejected — token exchange validation is a distinct MUST requirement. REPROCESS.

---

## Pair[CB4-3.4-1093-B]

**Territory:** T-rfc-oauth2 · T-asvs-token
**Domain:** Authorization code bound to `redirect_uri` at issuance — exact match enforced at token exchange
**Expected verdict:** PASS

**A:**
The Authorization Server stores the `redirect_uri` value from the authorization request with the issued authorization code. At the token endpoint, when a client presents an authorization code and a `redirect_uri` parameter: the AS retrieves the stored redirect_uri for the code and performs a case-sensitive exact-string comparison with the submitted redirect_uri. Mismatches result in `error=invalid_grant`. If the original authorization request did not include a redirect_uri parameter (where only one is registered), the token request is accepted without a redirect_uri check.

**B:**
PASS. RFC 6749 §4.1.3 MUST satisfied: redirect_uri at token exchange verified against value stored at authorization request time; exact match enforced. Redirect_uri substitution attack prevented. Correct handling when redirect_uri was absent from original request (no check required). No violations found.

**Reason:** RFC 6749 §4.1.3 MUST satisfied. redirect_uri stored at auth time; exact-match enforced at token time. Code injection via redirect_uri substitution prevented. PASS.

**Phrasing-B:**
PASS. AS stores redirect_uri with authorization code; verifies exact match at token exchange per RFC 6749 §4.1.3 MUST. Case-sensitive string comparison; mismatch → invalid_grant. Authorization code bound to specific redirect_uri. PASS.

---

## Pair[CB4-3.4-1094-A]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** JWKS keys published without `use` parameter — RS cannot distinguish signing from encryption keys
**Expected verdict:** CONCERN

**A:**
An Authorization Server publishes its JWKS with multiple key pairs (one RSA key pair for JWT signing, one RSA key pair for JWE (JSON Web Encryption) encryption) but none of the keys include the `use` (Public Key Use) parameter. A resource server fetching the JWKS cannot determine which key is intended for signature verification and which is for encryption. The RS tries all keys in the JWKS for signature verification. The AS rationale: "Both key types are in the JWKS. Our RS tries each key in order — the right key will succeed."

**B:**
CONCERN. RFC 7517 §4.2 states: "`use` SHOULD be present in a JWK for all key types to indicate intended use." SHOULD is a guidance-level recommendation. Absent `use`: (1) RSes must try all keys for signature verification, causing operational overhead and potential key-type confusion; (2) An RS that picks the encryption key for signature verification will fail signature checks with confusing error behavior; (3) When new keys are added to the JWKS, all parties must guess the intended use. No RFC MUST prohibits absent `use`, and the JWKS will technically function (the correct signing key will eventually succeed). However, the absent `use` is a key hygiene gap that increases operational complexity and reduces clarity. ASVS V9.1.1 guidance: key purpose should be explicit. No MUST → CONCERN. Correct practice: include `use=sig` for signing keys and `use=enc` for encryption keys.

**Reason:** RFC 7517 §4.2 SHOULD: use parameter should be present for all key types. Absent use causes key-type confusion and operational overhead. No MUST → CONCERN.

**Phrasing-B:**
CONCERN. RFC 7517 §4.2 SHOULD: use parameter should indicate key purpose. Absent use in JWKS: RS cannot distinguish signing (use=sig) from encryption (use=enc) keys without trial-and-error. No RFC MUST for use presence — guidance gap. CONCERN. Add use=sig to signing keys; use=enc to encryption keys.

---

## Pair[CB4-3.4-1094-B]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** JWKS keys include `use` parameter — signing and encryption keys explicitly distinguished
**Expected verdict:** PASS

**A:**
The Authorization Server's JWKS includes `use` parameter on all published keys: signing RSA keys have `"use": "sig"`; JWE encryption keys have `"use": "enc"`. Resource servers filter JWKS by `use=sig` before attempting signature verification. The JWKS also includes `alg` parameters on each key specifying the intended algorithm (e.g., `"alg": "RS256"` for the signing key, `"alg": "RSA-OAEP"` for the encryption key). Key IDs (`kid`) allow targeted key selection on a per-token basis.

**B:**
PASS. RFC 7517 §4.2 SHOULD satisfied: use parameter present on all keys; signing and encryption keys explicitly distinguished. RSes can filter by use=sig before signature verification. No key-type confusion. No violations found.

**Reason:** RFC 7517 §4.2 SHOULD satisfied. use=sig and use=enc present on respective keys. Key-type confusion eliminated. PASS.

**Phrasing-B:**
PASS. JWKS includes use=sig for signing keys; use=enc for encryption keys per RFC 7517 §4.2 SHOULD. RS filters by use before signature verification; no trial-and-error needed. alg parameters further constrain key usage. PASS.

---

## Pair[CB4-3.4-1095-A]

**Territory:** T-rfc-oauth2 · T-asvs-token
**Domain:** Token introspection response includes user PII beyond the scope of the access token
**Expected verdict:** CONCERN

**A:**
A token introspection endpoint (RFC 7662) returns detailed user attributes in the introspection response regardless of the access token's scope. A token with `scope=read:orders` receives an introspection response that includes `email`, `phone_number`, `address`, and `birthdate` fields — user PII (Personally Identifiable Information) not authorized by the token's scope. The AS rationale: "Resource servers need to know who the user is to make authorization decisions. Including user attributes in introspection is convenient — the RS doesn't have to call UserInfo separately."

**B:**
CONCERN. RFC 7662 §2.2 states: "Authorization servers SHOULD NOT include any additional information about the resource owner beyond what was granted in the access token." SHOULD NOT is a guidance-level recommendation, not MUST NOT. Additionally, ASVS V8.2.2 states: "Verify that sensitive data is not included in logs or monitoring systems." Introspection responses are consumed by resource servers and may be logged in RS access logs — PII in introspection responses is subject to wider logging and caching than intended. The combination of RFC 7662 §2.2 SHOULD NOT and ASVS V8.2.2 logging guidance creates a meaningful concern. However, no RFC MUST NOT prohibits PII in introspection responses — the severity is CONCERN not REPROCESS. Correct practice: restrict introspection response to claims authorized by the presented token's scope; use UserInfo for additional attributes when needed.

**Reason:** RFC 7662 §2.2 SHOULD NOT: introspection should not include attributes beyond token scope. ASVS V8.2.2 logging concern for PII in RS-facing responses. No MUST NOT → CONCERN.

**Phrasing-B:**
CONCERN. RFC 7662 §2.2 SHOULD NOT: introspection must not include resource owner info beyond token scope. PII (email, phone, address) in introspection response with scope=read:orders exceeds authorized disclosure. ASVS V8.2.2: PII in RS-side introspection response risks log exposure. No MUST NOT → CONCERN. Scope-filter introspection response to token's authorized claims.

---

## Pair[CB4-3.4-1095-B]

**Territory:** T-rfc-oauth2 · T-asvs-token
**Domain:** Token introspection response scope-filtered — only claims authorized by token scope returned
**Expected verdict:** PASS

**A:**
The token introspection endpoint scope-filters the response: for `scope=read:orders`, the introspection response includes `active`, `scope`, `client_id`, `exp`, `iat`, `sub` (opaque PPID (Pairwise Pseudonymous Identifier)), and order-related claims only. User PII fields (email, phone, address, birthdate) are not included in the response unless the token's scope explicitly grants access to those claims (`scope=email`, `scope=phone`, etc.). The introspection response filtering logic mirrors the UserInfo endpoint scope-claim mapping.

**B:**
PASS. RFC 7662 §2.2 SHOULD NOT satisfied: introspection response does not include resource owner attributes beyond what the token's scope authorizes. ASVS V8.2.2 concern mitigated: PII not present in RS-facing introspection responses for non-PII scopes. No violations found.

**Reason:** RFC 7662 §2.2 SHOULD NOT satisfied. Introspection scope-filtered to authorized claims. PII not exposed beyond token's authorized scope. PASS.

**Phrasing-B:**
PASS. Introspection response filtered to claims authorized by token scope per RFC 7662 §2.2 SHOULD NOT. PII claims absent for non-PII scopes. ASVS V8.2.2 logging risk mitigated. PASS.

---

## Pair[CB4-3.4-1096-A]

**Territory:** T-oauth-oidc · T-cwe-authn
**Domain:** User enumeration via `login_hint` — AS returns different responses for registered vs unregistered users
**Expected verdict:** CONCERN

**A:**
An OIDC Authorization Server supports the `login_hint` parameter to pre-populate the login form with a username or email. When `login_hint` contains an email address for a registered user, the AS returns the normal authentication challenge. When `login_hint` contains an email address for an unregistered user, the AS returns `{"error":"login_required","error_description":"User not found in directory"}`. These distinct responses allow unauthenticated parties to enumerate whether email addresses are registered in the system by observing the difference in error descriptions.

**B:**
CONCERN. CWE-204 (Observable Response Discrepancy) applies: distinct error responses for "user found" vs "user not found" allow unauthenticated enumeration of registered email addresses. ASVS V6.1.3 guidance states: "Verify that all authentication functions display generic messages... such that whether a valid account exists is not discoverable." "Verify" language in ASVS is guidance-level in our framework (CONCERN, not REPROCESS) unless backed by an RFC MUST. No RFC MUST prohibits distinguishable error responses for login_hint specifically. The AS using login_hint reveals user existence through error behavior, which is a CWE-204 gap. No unconditional RFC MUST with MUST NOT language on this specific behavior → CONCERN. Correct practice: return a uniform error (`login_required`) regardless of whether the login_hint identifies a registered user.

**Reason:** CWE-204: distinct error responses for registered vs unregistered users enable email enumeration via login_hint. ASVS V6.1.3 guidance (generic messages). No RFC MUST NOT for this specific behavior. CONCERN.

**Phrasing-B:**
CONCERN. CWE-204 (Observable Response Discrepancy): AS error behavior distinguishes registered vs unregistered users on login_hint → email enumeration by unauthenticated parties. ASVS V6.1.3 guidance: authentication functions should not reveal account existence. No RFC MUST NOT for login_hint response behavior. CONCERN. Return uniform login_required regardless of user existence.

---

## Pair[CB4-3.4-1096-B]

**Territory:** T-oauth-oidc · T-cwe-authn
**Domain:** Uniform `login_required` response regardless of user existence — no user enumeration via `login_hint`
**Expected verdict:** PASS

**A:**
The OIDC Authorization Server returns a uniform `{"error": "login_required"}` response for all authorization requests that require authentication, regardless of whether the `login_hint` value identifies a registered user, an unregistered email, a syntactically invalid value, or no user at all. Internally, the AS may use the login_hint to pre-populate the login form UI (User Interface), but no external behavioral difference is observable by unauthenticated parties. The error description does not contain user-existence information.

**B:**
PASS. CWE-204 mitigated: no observable response discrepancy between registered and unregistered users via login_hint error responses. ASVS V6.1.3 guidance satisfied: authentication functions return generic messages that do not reveal account existence. No violations found.

**Reason:** CWE-204 mitigated. Uniform error response regardless of user existence. ASVS V6.1.3 guidance satisfied. No user enumeration via login_hint. PASS.

**Phrasing-B:**
PASS. Uniform login_required error for all login_hint values (registered, unregistered, invalid). No observable discrepancy enables user enumeration. CWE-204 mitigated. ASVS V6.1.3 guidance satisfied. PASS.
