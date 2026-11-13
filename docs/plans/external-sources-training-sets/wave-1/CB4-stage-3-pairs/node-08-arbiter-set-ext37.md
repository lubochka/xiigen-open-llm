# CB4 Stage 3 — Node 08 Arbiter Set ext37
# Sub-phase 3.4 · Security and Authorization Model
# Sequences CB4-3.4-959 through CB4-3.4-966 (8 base pairs × 2 phrasings = 16 instances)
# New domains: client_credentials+refresh_token, OIDC max_age, nbf validation at RS, wildcard aud
# Running total after this file: 120 + (966 − 60) × 2 = 1932 instances (966 base pairs)
# Sources: RFC 6749 §4.4.3, OIDC Core §3.1.2.2, RFC 9068 §4, RFC 8725 §3.9

---

## Pair[CB4-3.4-959-A]

**Territory:** T-rfc-oauth2
**Domain:** Token endpoint response for `grant_type=client_credentials` includes `refresh_token` field
**Expected verdict:** REPROCESS

**A:**
A token endpoint issues a `refresh_token` in all successful token responses, including responses to `grant_type=client_credentials` requests. The developer rationale: "Long-running background services need to refresh their tokens without re-authenticating every hour. Issuing a refresh token with client credentials simplifies their token lifecycle." An example response: `{"access_token":"...","token_type":"Bearer","expires_in":3600,"refresh_token":"...","scope":"read:telemetry"}`.

**B:**
REPROCESS. RFC 6749 §4.4.3 provides a categorical prohibition: "A refresh token MUST NOT be included" in the access token response for the client credentials grant. This MUST NOT is unconditional — it reflects a deliberate design decision in the OAuth 2.0 framework: the client credentials flow authenticates the client (a confidential application) directly, not on behalf of an end user; refresh tokens in this context would create long-lived credentials beyond the intended access token TTL without providing any additional security value beyond simply re-authenticating with the client's credentials (which the client already possesses). The "simplified lifecycle" argument is rejected: a service holding valid client credentials can always obtain a new access token by re-presenting those credentials; a refresh token is an additional secret that, if compromised, grants access without the client credentials check. The MUST NOT is explicit and unconditional.

**Reason:** RFC 6749 §4.4.3 MUST NOT: refresh_token must not be included in client_credentials grant responses. Clear normative prohibition. The lifecycle simplification argument does not override MUST NOT text. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 6749 §4.4.3 MUST NOT: the access token response for client credentials MUST NOT include a refresh token. The client already holds its credentials and can re-authenticate to obtain a new access token — a refresh token adds an additional long-lived secret with no security benefit. Including a refresh_token violates this unconditional prohibition. REPROCESS.

---

## Pair[CB4-3.4-959-B]

**Territory:** T-rfc-oauth2
**Domain:** `grant_type=client_credentials` response correctly omits `refresh_token`
**Expected verdict:** PASS

**A:**
A token endpoint serving `grant_type=client_credentials` returns responses containing: `access_token`, `token_type=Bearer`, `expires_in=3600`, and `scope`. No `refresh_token` field is included. Long-running services renew tokens by re-presenting their client credentials before expiry (with a pre-expiry buffer of 60 seconds). The token management library is configured to detect `expires_in` and schedule automatic re-authentication.

**B:**
PASS. RFC 6749 §4.4.3 MUST NOT satisfied: no refresh_token present in client credentials grant response. The renewal strategy — automatic re-authentication before expiry using the client's credentials — is correct and efficient; it achieves the same lifecycle goal without creating an additional long-lived secret. RFC 6749 §5.1 MUST satisfied: access_token, token_type, and expires_in all present. No violations found.

**Reason:** RFC 6749 §4.4.3 MUST NOT satisfied (no refresh_token). RFC 6749 §5.1 satisfied. Correct client credentials renewal pattern. PASS.

**Phrasing-B:**
PASS. Client credentials grant response correctly contains only access_token, token_type, expires_in, scope — no refresh_token per RFC 6749 §4.4.3 MUST NOT. Automatic re-authentication before expiry satisfies the lifecycle requirement without the additional credential risk. PASS.

---

## Pair[CB4-3.4-960-A]

**Territory:** T-rfc-oidc · T-oauth-oidc
**Domain:** OIDC `max_age` parameter present in authorization request but ignored by AS
**Expected verdict:** REPROCESS

**A:**
A relying party (RP) sends an OIDC authorization request with `max_age=300` (5 minutes), signalling that it requires the user to have authenticated with the identity provider within the last 5 minutes. The AS has an active session for the user that was created 4 hours ago. The AS does not evaluate the `max_age` parameter — it issues the authorization code based on the existing session without triggering re-authentication. The ID token's `auth_time` claim reflects the original authentication from 4 hours ago. The AS developer: "max_age is an advisory hint; we prioritize session continuity over re-authentication prompts."

**B:**
REPROCESS. OIDC Core §3.1.2.2 is unambiguous: "If the End-User is not already authenticated, or if the max_age request parameter is provided and the elapsed time since the last authentication is greater than the max_age value, the OP MUST attempt to actively re-authenticate the End-User." The operative word is MUST. The `max_age` parameter is not a hint or preference — it is a security requirement from the RP that the AS is obligated to honour when present. The practical consequence of ignoring it: an RP relying on max_age to enforce step-up authentication for sensitive operations (e.g., payment authorization, account settings changes) has no effective control over authentication freshness. An attacker who accesses the user's browser with an existing session — 4 hours stale — can complete the sensitive operation without triggering fresh authentication. "Session continuity" is a user experience consideration that does not override a MUST requirement in OIDC Core.

**Reason:** OIDC Core §3.1.2.2 MUST: AS must re-authenticate if elapsed time since last auth > max_age. Treating max_age as advisory violates this unconditional MUST and undermines RP-controlled authentication freshness guarantees. REPROCESS.

**Phrasing-B:**
REPROCESS. OIDC Core §3.1.2.2 MUST: when max_age is present and elapsed time since last authentication exceeds it, the OP must actively re-authenticate the user. The max_age parameter is a hard security requirement from the RP, not an advisory preference. Ignoring it breaks the RP's ability to enforce authentication freshness — a critical control for step-up authentication scenarios. "Session continuity priority" does not override OIDC Core MUST text. REPROCESS.

---

## Pair[CB4-3.4-960-B]

**Territory:** T-rfc-oidc · T-oauth-oidc
**Domain:** OIDC `max_age` correctly enforced — re-authentication triggered when session exceeds max_age
**Expected verdict:** PASS

**A:**
The AS's authorization endpoint evaluates `max_age` when present in the authorization request: it computes `current_time − auth_time` from the user's session and compares it to `max_age`. If the elapsed time exceeds `max_age`, the AS initiates re-authentication via the login page before issuing the authorization code. If the user successfully re-authenticates, the session `auth_time` is updated, the ID token reflects the new `auth_time`, and the authorization code is issued. An authorization request with `max_age=300` against a session authenticated 4 hours ago triggers re-authentication.

**B:**
PASS. OIDC Core §3.1.2.2 MUST satisfied: the AS evaluates max_age, compares elapsed time against session auth_time, and triggers active re-authentication when the elapsed time exceeds max_age. The ID token's `auth_time` reflects the actual re-authentication time, giving the RP an accurate freshness signal. The RP's authentication freshness requirement is honoured unconditionally. No violations found.

**Reason:** OIDC Core §3.1.2.2 MUST satisfied. max_age evaluated, re-authentication triggered correctly, auth_time updated accurately. PASS.

**Phrasing-B:**
PASS. AS evaluates max_age on every authorization request: computes current_time − auth_time; triggers re-authentication if elapsed > max_age; updates auth_time after re-auth; reflects correct auth_time in ID token. OIDC Core §3.1.2.2 MUST fully satisfied. Authentication freshness guarantee operational for the RP. PASS.

---

## Pair[CB4-3.4-961-A]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** RS accepts JWT access token before its `nbf` (not-before) time — nbf check skipped
**Expected verdict:** REPROCESS

**A:**
A resource server validates JWT access tokens by checking: signature, `iss`, `aud`, `exp`, and `scope`. The `nbf` (not-before) claim is present in tokens but the RS's validation logic does not check it. The developer explanation: "We check `exp` which covers the validity window. The `nbf` claim is redundant — if the token hasn't expired, it should be usable." A penetration test demonstrates that a JWT with `iat: T`, `nbf: T+3600` (valid starting one hour from now), and `exp: T+7200` is accepted immediately at the RS.

**B:**
REPROCESS. RFC 9068 §4 is explicit: "If the JWT has an `nbf` (not before) claim, the JWT MUST be rejected if it contains an `nbf` claim that is greater than the current Unix time." This is an unconditional MUST for JWT access token validation at the resource server. The practical implication: if the AS issues tokens with a future `nbf` (for scheduled access, pre-issued tokens in time-locked scenarios, or by misconfiguration), the `nbf` claim is the server's only mechanism to enforce the intended start time. An RS that skips nbf allows pre-issued tokens to be used before the AS intended. The "exp covers the window" argument is rejected: `exp` defines when a token stops being valid; `nbf` defines when it starts being valid. These are complementary constraints, not redundant ones. Skipping `nbf` means the RS honours tokens before their intended validity window opens.

**Reason:** RFC 9068 §4 MUST: RS must reject JWT if nbf > current time. Skipping nbf validation violates this unconditionally. exp and nbf serve different boundary conditions; neither is redundant. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 9068 §4: RS MUST reject a JWT access token if its nbf claim is greater than the current Unix time. The penetration test confirms: a token with nbf one hour in the future is accepted. The "exp is sufficient" argument conflates the two boundary checks: exp defines token expiry; nbf defines token validity start. Both are mandatory for correct token lifecycle enforcement. Skipping nbf allows use of tokens before their intended validity window. REPROCESS.

---

## Pair[CB4-3.4-961-B]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** RS validates `nbf` claim with appropriate clock skew tolerance
**Expected verdict:** PASS

**A:**
The resource server's JWT validation pipeline checks: (1) signature; (2) `iss` allowlist; (3) `aud` contains RS identifier; (4) `exp` > current_time; (5) `nbf` ≤ current_time + 5_seconds (5-second forward clock skew tolerance); (6) `typ` == "at+JWT"; (7) `scope` contains required permission. Step (5) validates the not-before constraint: if nbf is present and greater than current_time by more than 5 seconds, the token is rejected with 401. A 5-second tolerance accommodates minor clock synchronization differences between the AS and RS.

**B:**
PASS. RFC 9068 §4 MUST satisfied: RS validates `nbf` claim and rejects tokens where nbf > current time. The 5-second clock skew tolerance is a well-established practice (referenced in RFC 7519 §4.1.5) that accommodates minor clock synchronization differences without creating a meaningful attack window. The full validation chain (sig → iss → aud → exp → nbf → typ → scope) is correct and ordered. No violations found.

**Reason:** RFC 9068 §4 MUST satisfied. nbf validation with 5-second clock skew tolerance is correct practice. Full token validation chain implemented. PASS.

**Phrasing-B:**
PASS. RS validates nbf per RFC 9068 §4 MUST: rejects if nbf > current_time + 5s clock skew tolerance. The 5-second tolerance is standard practice for distributed clock synchronization. Full validation pipeline includes sig, iss, aud, exp, nbf, typ, scope. No violations. PASS.

---

## Pair[CB4-3.4-962-A]

**Territory:** T-rfc-jwt · T-rfc-oauth2
**Domain:** JWT `aud` claim issued as a glob/wildcard pattern (`*.example.com`) rather than specific RS URI
**Expected verdict:** CONCERN

**A:**
An authorization server issues JWT access tokens with `"aud": "*.example.com"`. Multiple resource servers in the deployment — `api.example.com`, `data.example.com`, `admin.example.com` — all match this pattern and accept any token with the wildcard aud value. The developer reasoning: "This is simpler than maintaining per-RS aud values. Any RS on our domain is an authorized recipient."

**B:**
CONCERN. RFC 8725 §3.9 (JWT Best Current Practices) and RFC 9068 §4 address audience validation: the RS must verify that the aud claim contains an identifier for that specific RS; the aud mechanism is designed to restrict token use to the intended recipient. Issuing tokens with a wildcard audience pattern fundamentally compromises the token audience restriction: a token acceptable at `api.example.com` is equally acceptable at `admin.example.com`, even if the requesting client only had authorization to access `api.example.com`. This defeats the audience isolation that RFC 8707 (resource indicators) and RFC 9068 are designed to provide. However, no RFC provides an explicit MUST requiring per-RS aud values as a positive obligation — RFC 8725 §3.9 uses SHOULD language for specific audience binding; MUST NOT language is not applied to wildcard patterns specifically. The aud restriction failure is a meaningful guidance-level gap. Graduation rule: this is a CONCERN, not REPROCESS. Correct practice: issue tokens with the specific RS URI as aud, using RFC 8707 resource indicators at the authorization endpoint to select the appropriate aud value.

**Reason:** RFC 8725 §3.9 SHOULD for specific aud identifiers; no explicit MUST prohibiting wildcard aud in positive terms. Wildcard aud defeats per-RS token audience isolation. SHOULD-level gap = CONCERN, not REPROCESS.

**Phrasing-B:**
CONCERN. JWT aud="*.example.com" defeats per-RS audience isolation: any RS on the domain accepts any token regardless of the client's authorized scope of access. RFC 8725 §3.9 and RFC 9068 §4 establish aud-based audience restriction as the audience model; wildcards undermine this. However, the prohibition is SHOULD-level (RFC 8725 §3.9) rather than an unconditional MUST — CONCERN, not REPROCESS. Recommend: per-RS aud values using RFC 8707 resource indicators to bind tokens to their intended recipient.

---

## Pair[CB4-3.4-962-B]

**Territory:** T-rfc-jwt · T-rfc-oauth2
**Domain:** JWT `aud` claim carries the specific RS URI; RS validates its own identifier in aud
**Expected verdict:** PASS

**A:**
JWT access tokens are issued with `"aud": "https://api.example.com"` when the client requests access to the API RS, or `"aud": "https://data.example.com"` for the data RS. Clients use RFC 8707 `resource` parameter in the authorization request to indicate the target RS, which the AS uses to set the aud value. Each RS validates that its own URI is present in the aud claim: `api.example.com` accepts tokens where aud is or contains `"https://api.example.com"` and rejects all others. A token for the data RS is rejected at the API RS.

**B:**
PASS. RFC 9068 §4 satisfied: each RS validates that the aud claim contains its own URI identifier. RFC 8707 resource indicators correctly drive per-RS token issuance, ensuring tokens are audience-restricted at issuance time. Wildcard patterns are absent; each token is restricted to its specific intended RS. Token audience isolation is fully operational: access to one RS confers no access to another. RFC 8725 §3.9 SHOULD satisfied: specific audience identifiers used. No violations found.

**Reason:** RFC 9068 §4 and RFC 8725 §3.9 satisfied. Per-RS aud via RFC 8707 resource indicators. Audience isolation operational. PASS.

**Phrasing-B:**
PASS. Per-RS aud values issued using RFC 8707 resource indicators at authorization time. Each RS validates its own URI in aud, rejecting tokens intended for other RSes. Audience isolation fully operational — access to api.example.com does not extend to admin.example.com. RFC 9068 §4 and RFC 8725 §3.9 satisfied. PASS.
