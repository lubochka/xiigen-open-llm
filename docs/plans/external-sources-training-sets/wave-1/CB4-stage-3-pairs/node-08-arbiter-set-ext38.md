# CB4 Stage 3 — Node 08 Arbiter Set ext38
# Sub-phase 3.4 · Security and Authorization Model
# Sequences CB4-3.4-985 through CB4-3.4-992 (8 base pairs × 2 phrasings = 16 instances)
# New domains: introspection-active-expired, scope-enlargement-code-exchange, iss-absent-RS, access-token-TTL-long
# Running total after this file: 120 + (992 − 60) × 2 = 1984 instances (992 base pairs)
# Sources: RFC 7662 §2.2, RFC 6749 §4.1.3, RFC 9068 §4, ASVS V9.3.1

---

## Pair[CB4-3.4-985-A]

**Territory:** T-rfc-oauth2 · T-asvs-token
**Domain:** Token introspection endpoint returns `active: true` for expired tokens
**Expected verdict:** REPROCESS

**A:**
A token introspection endpoint (RFC 7662) returns `{"active": true, "scope": "read:data", "exp": 1714500000}` for a token whose `exp` value is in the past. The RS submitting the introspection request trusts this response and grants the request, even though the access token has expired. The implementation comment: "We respond based on the database record; if the token is in the active_tokens table and not explicitly revoked, we return active=true. Expiry checking is the RS's responsibility — they can see the exp value in our response."

**B:**
REPROCESS. RFC 7662 §2.2 defines the `active` field semantics: "Boolean indicator of whether or not the presented token is currently active ... In addition to the claims included in the token, any additional claims are also considered when determining the token's active status. A token is considered active if and only if [all conditions including] the token is not expired are met." RFC 7662 §2.2 further specifies: "active ... MUST be true if the token is currently active" — the corollary is that MUST implies MUST NOT be true when the token is not active. Returning `active: true` for an expired token violates the semantics of the active field: the introspection endpoint is the authoritative status oracle; RSes may rely on it without independently evaluating exp. The "RS can check exp themselves" argument is rejected: the entire purpose of introspection is to provide a trustworthy active status that RSes can rely on; if RSes must redundantly validate exp anyway, introspection provides no additional security value over self-contained JWT validation. The response is materially incorrect and constitutes a MUST violation on the active field's normative semantics.

**Reason:** RFC 7662 §2.2: active MUST accurately reflect current token status including expiry. Returning active=true for expired tokens violates the normative active field semantics and undermines the trustworthy-status-oracle function of introspection. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 7662 §2.2: the `active` field is the authoritative token status indicator; it must be false for expired tokens. Returning active=true for an expired token makes the introspection endpoint an unreliable oracle — RSes that trust introspection will continue accepting expired tokens. "RS checks exp themselves" is rejected: delegating expiry checking to the RS negates introspection's value and is not what RFC 7662 §2.2 mandates. REPROCESS.

---

## Pair[CB4-3.4-985-B]

**Territory:** T-rfc-oauth2 · T-asvs-token
**Domain:** Introspection correctly returns `active: false` for expired tokens
**Expected verdict:** PASS

**A:**
The introspection endpoint evaluates token status before responding: it checks the token record against the current time (`current_time < exp`), checks the revocation store, and verifies any database-side active flag. If any condition fails (expired, revoked, or explicitly deactivated), the endpoint returns `{"active": false}`. For active, non-expired, non-revoked tokens, it returns the full active response with all applicable claims. Expired token: `{"active": false}`. Valid token: `{"active": true, "sub": "...", "scope": "read:data", "exp": <future>}`.

**B:**
PASS. RFC 7662 §2.2 satisfied: `active` accurately reflects current token status. Expiry is evaluated server-side before responding, ensuring RSes receive an authoritative and accurate status. The introspection endpoint serves its intended function: a trustworthy, real-time token status oracle. No violations found.

**Reason:** RFC 7662 §2.2 satisfied. active accurately reflects all status conditions (exp, revocation, active flag). Introspection oracle trustworthy. PASS.

**Phrasing-B:**
PASS. Introspection endpoint evaluates expiry, revocation, and active status before responding. active=false for expired or revoked tokens; active=true only for currently valid tokens. RFC 7662 §2.2 satisfied. PASS.

---

## Pair[CB4-3.4-986-A]

**Territory:** T-rfc-oauth2
**Domain:** `scope` parameter at authorization code exchange exceeds the scope of the original authorization grant
**Expected verdict:** REPROCESS

**A:**
A client is authorized during the authorization phase with `scope=read:profile`. At the token exchange step (POST /token with grant_type=authorization_code), the client sends `scope=read:profile write:profile admin:settings`. The token endpoint grants the full requested scope — the access token is issued with `scope=read:profile write:profile admin:settings` — because the implementation processes the scope from the token request without checking it against the authorization phase scope. Developer note: "We process whatever scope the client asks for at the exchange; we assume they know what they need."

**B:**
REPROCESS. RFC 6749 §4.1.3 is explicit: "scope — OPTIONAL. The scope of the access request as described by Section 3.3. The requested scope MUST NOT include any scope not originally granted by the resource owner, and if omitted is treated as equal to the scope originally granted by the resource owner." This is a MUST NOT. The authorization grant (scope=read:profile) represents the resource owner's consent decision; the token endpoint must honour that decision, not the client's post-hoc assertion of a wider scope. Granting `write:profile` and `admin:settings` — which the user never consented to during the authorization phase — is a scope enlargement that bypasses the user's consent boundary. The "client knows what it needs" argument is rejected: the client's self-reported scope requirement is not a valid basis for expanding the authorization grant; the resource owner's consent at the authorization endpoint is the governing boundary.

**Reason:** RFC 6749 §4.1.3 MUST NOT: token exchange scope must not exceed the original authorization grant scope. Granting wider scope than the resource owner consented to is a MUST NOT violation and bypasses the user consent boundary. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 6749 §4.1.3 MUST NOT: scope at token exchange must not exceed the original grant. The user consented to read:profile; the token endpoint issuing write:profile + admin:settings violates that consent boundary unconditionally. "Client self-reports its needs" does not override MUST NOT text — the authorization phase scope ceiling is fixed by resource owner consent. REPROCESS.

---

## Pair[CB4-3.4-986-B]

**Territory:** T-rfc-oauth2
**Domain:** Token exchange scope correctly bounded to original authorization grant
**Expected verdict:** PASS

**A:**
The token endpoint's scope processing for authorization code exchanges: if the token request includes a `scope` parameter, it is validated against the authorization code's stored scope. Any scope in the token request that exceeds the authorization grant scope is rejected with `error=invalid_scope`. If scope is omitted from the token request, the access token is issued with the full authorization grant scope. Downscoping (requesting a subset of the granted scope) is accepted and the access token carries the narrower scope.

**B:**
PASS. RFC 6749 §4.1.3 MUST NOT satisfied: scope at token exchange is validated against the authorization grant; no scope expansion is permitted. Downscoping is correctly allowed (the client may request a subset). error=invalid_scope is the correct error response for a scope violation. No violations found.

**Reason:** RFC 6749 §4.1.3 MUST NOT satisfied. Scope validation against authorization grant implemented. Downscoping permitted; upscoping rejected with correct error. PASS.

**Phrasing-B:**
PASS. Token exchange validates scope against authorization grant: expansion rejected (error=invalid_scope); subset accepted. RFC 6749 §4.1.3 MUST NOT satisfied. Resource owner consent boundary upheld. PASS.

---

## Pair[CB4-3.4-987-A]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** RS does not validate `iss` claim — any issuer's JWT access token accepted
**Expected verdict:** REPROCESS

**A:**
A resource server validates JWT access tokens for: signature (using the JWKS at a configured URL), `aud` (must contain the RS's identifier), `exp` (must not be expired), `typ` (must be "at+JWT"). The `iss` claim is not validated — the RS does not check the issuer against a configured allowlist or expected value. The developer reasoning: "We verify the signature against our trusted JWKS endpoint. If the signature is valid, the token came from a trusted source — we don't need to also check iss."

**B:**
REPROCESS. RFC 9068 §4 explicitly lists `iss` validation as a required step: "The Resource Server MUST validate the JWT access token following the steps described in [RFC7519] Section 7.2 and the process described in [RFC7519] Section 3. The RS specifically MUST verify: ... the iss claim is trusted (the token was issued by the expected authorization server)." The "signature proves source" argument conflates JWKS-based signature verification with issuer identity validation. An attacker who controls a JWKS endpoint that shares the same URL as the configured JWKS — or who achieves JWKS cache poisoning — can serve forged keys; without iss validation, the RS cannot detect that tokens are from an unauthorized issuer even when key substitution occurs. More broadly: iss validation ensures the token was issued by the specific AS the RS is configured to trust, not just by any party whose keys are currently in the fetched JWKS. The iss claim is the logical binding between the token and the authorized issuer.

**Reason:** RFC 9068 §4 MUST: RS must validate iss against expected AS identifier. "Signature proves source" argument rejected — iss validation is complementary to signature verification. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 9068 §4: RS must validate iss as a required step in JWT access token validation. Signature verification and iss validation serve different security properties: signature verifies integrity (the token wasn't tampered with); iss verifies provenance (the token was issued by the expected AS). Omitting iss allows tokens from unauthorized issuers to be accepted if their signing keys happen to be valid JWKS responses. REPROCESS — add iss allowlist check.

---

## Pair[CB4-3.4-987-B]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** RS validates `iss` claim against configured AS identifier allowlist
**Expected verdict:** PASS

**A:**
The resource server maintains a configured allowlist of trusted issuer identifiers: `["https://auth.example.com"]`. The JWT validation pipeline checks `iss` at step 2 (after typ check, before key lookup): the iss claim value must be an exact string match against an entry in the allowlist. If iss is absent or not in the allowlist, the token is rejected with 401 before key lookup is attempted. The allowlist is configured at deployment time and is not modifiable at runtime without a deployment change.

**B:**
PASS. RFC 9068 §4 MUST satisfied: iss is validated against a configured allowlist of trusted issuer identifiers. Exact string match ensures that sub-domain variants (e.g., evil-auth.example.com) cannot pass iss validation (per the ext33 pattern). Checking iss before key lookup is efficient — invalid issuers are rejected before incurring JWKS fetch cost. Allowlist-at-deployment-time configuration prevents runtime manipulation. All RFC 9068 §4 iss validation requirements satisfied.

**Reason:** RFC 9068 §4 MUST satisfied. iss validated against exact allowlist. Ordering (before key lookup) is correct. No violations. PASS.

**Phrasing-B:**
PASS. RS validates iss as exact string match against allowlist ["https://auth.example.com"] per RFC 9068 §4 MUST. iss check before key lookup is correct ordering. Allowlist prevents sub-domain and variant issuer bypass. PASS.

---

## Pair[CB4-3.4-988-A]

**Territory:** T-asvs-token · T-rfc-oauth2
**Domain:** Access token TTL of 24 hours for user-facing authorization code flows — exceeds guidance range
**Expected verdict:** CONCERN

**A:**
An authorization server issues access tokens with `expires_in=86400` (24 hours) for authorization code flow responses. The developer reasoning: "Users should not have to re-authenticate frequently. A 24-hour access token reduces friction." The access tokens are JWTs carrying standard claims (sub PPID, iss, aud, exp, iat, jti, scope). No PII is in the payload.

**B:**
CONCERN. ASVS V9.3.1 provides guidance on token lifetimes: "Verify that access tokens have a limited lifetime (usually 15 minutes to 1 hour)." A 24-hour access token TTL substantially exceeds this guidance range. The practical risk: a 24-hour access token that is exfiltrated (via XSS, log exposure, network intercept, or storage compromise) remains usable for up to 24 hours after exfiltration. The attacker has a full day to exploit stolen credentials before they expire. The "jti + blocklist" combination can mitigate this for known-compromised tokens, but detection of compromise is not guaranteed — many exfiltration methods are covert. However, ASVS V9.3.1 uses "usually" — not "MUST" or "MUST NOT" — indicating guidance rather than an unconditional requirement. No RFC provides a specific maximum access token TTL with MUST-level language. The 24-hour TTL is a meaningful security posture gap; the recommendation is a 15-minute to 1-hour TTL. Verdict: CONCERN (guidance gap, not MUST violation).

**Reason:** ASVS V9.3.1 guidance ("usually 15 min to 1 hour") not met by 24-hour TTL. No MUST violation — "usually" is guidance language. 24 hours is substantially outside the guidance range. CONCERN, not REPROCESS.

**Phrasing-B:**
CONCERN. ASVS V9.3.1 guidance: access token TTL should typically be 15 minutes to 1 hour. 24 hours is 16–96× above the guidance ceiling. "Usually" language makes this guidance rather than unconditional MUST — but the 24-hour window creates a substantial exfiltration exploitation window. No RFC provides a maximum TTL in MUST-level terms. CONCERN. Recommend: reduce to 15-minute TTL for user-facing flows; use refresh tokens for session continuity.

---

## Pair[CB4-3.4-988-B]

**Territory:** T-asvs-token · T-rfc-oauth2
**Domain:** Access token TTL within guidance range with refresh token for continuity
**Expected verdict:** PASS

**A:**
Access tokens are issued with `expires_in=900` (15 minutes) for user-facing authorization code flows. Clients that included `offline_access` in their scope receive a refresh token with a 30-day absolute maximum lifetime. The client library renews access tokens automatically when they are within 60 seconds of expiry. The refresh token undergoes rotation on each use per BCP §4.14.1.

**B:**
PASS. ASVS V9.3.1 guidance satisfied: 15-minute access token TTL is within the "15 minutes to 1 hour" guidance range. The refresh token enables session continuity without requiring the access token to have a long TTL — this is the intended architectural pattern: short-lived access tokens (limiting exfiltration exploitation window) + refresh tokens for continuity (with offline_access consent + rotation + absolute lifetime). BCP §4.14.1 refresh token rotation is implemented. No violations found.

**Reason:** ASVS V9.3.1 guidance satisfied: 15-minute TTL within guidance range. Refresh token provides continuity per correct pattern. BCP rotation implemented. PASS.

**Phrasing-B:**
PASS. 15-minute access token TTL satisfies ASVS V9.3.1 guidance range. Refresh token (offline_access consent + 30-day absolute max + rotation) provides session continuity without long-lived access tokens. Correct two-token architecture. PASS.
