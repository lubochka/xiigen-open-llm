# CB4 Stage 3 — Node 08 Arbiter Set ext39
# Sub-phase 3.4 · Security and Authorization Model
# Sequences CB4-3.4-1011 through CB4-3.4-1018 (8 base pairs × 2 phrasings = 16 instances)
# New domains: jku-header-injection, client-enumeration-error-codes, OAuth-audit-logging, exp-not-validated-RS
# Running total after this file: 120 + (1018 − 60) × 2 = 2036 instances (1018 base pairs)
# Sources: RFC 8725 §3.3, RFC 6749 §5.2, CWE-204, ASVS V8.3.2, RFC 9068 §4

---

## Pair[CB4-3.4-1011-A]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** `jku` header parameter in JWT accepted by RS — RS fetches JWKS from attacker-supplied URL
**Expected verdict:** REPROCESS

**A:**
A resource server validates JWT access tokens by reading the JOSE header for a `jku` (JWK Set URL) parameter. If present, the RS fetches the JWKS from the URL in the `jku` header and uses those keys to verify the signature. The RS does not maintain an allowlist of trusted JWKS URLs — it fetches from any URL found in the `jku` header. The developer rationale: "jku is part of the JWT spec; honoring it is spec-compliant and allows dynamic key management without configuring JWKS endpoints in advance."

**B:**
REPROCESS. RFC 8725 §3.3 (JWT Best Current Practices) is explicit: "The `jku` and `x5u` Header Parameter values ... MUST be explicitly allowed by the application, i.e., the application MUST be configured to explicitly allow these header parameters, and their contents MUST be fetched only from locations that are explicitly trusted." An RS that fetches JWKS from an arbitrary URL found in the token header violates both MUSTs. The attack is well-understood: an attacker crafts a token with a `jku` value pointing to a URL they control, generates a key pair, signs the token with their private key, publishes the public key at their URL. When the RS fetches keys from the attacker's URL, it validates the attacker's token as legitimate — complete authentication bypass. The "spec-compliant" argument is rejected: RFC 8725 §3.3 imposes explicit restrictions on when jku may be used; passive acceptance of jku is not spec-compliant.

**Reason:** RFC 8725 §3.3 MUST (×2): jku must be explicitly allowed; JWKS fetched only from trusted locations. RS fetching from arbitrary jku header = authentication bypass via JWKS injection. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 8725 §3.3 MUST: jku header parameter must be explicitly allowed and JWKS fetched only from trusted locations. RS accepting arbitrary jku values enables complete authentication bypass: attacker signs token with own key, publishes it at attacker-controlled URL, RS validates attacker's token as legitimate. "Honoring jku is spec-compliant" is rejected — RFC 8725 §3.3 explicitly restricts jku use to pre-trusted locations. REPROCESS.

---

## Pair[CB4-3.4-1011-B]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** RS ignores `jku` header parameter — uses only pre-configured JWKS endpoint
**Expected verdict:** PASS

**A:**
The resource server ignores all unrecognized or potentially dangerous JOSE header parameters including `jku`, `jwk`, and `x5u`. It uses only a pre-configured JWKS URL (`https://auth.example.com/.well-known/jwks.json`), fetched from a deployment-time configuration value. Any JWT presenting a `jku` or `jwk` header is treated identically to one without — these parameters are not processed. The pre-configured JWKS URL is validated against a TLS trust anchor at deployment.

**B:**
PASS. RFC 8725 §3.3 satisfied: jku and x5u header parameters are not acted upon; JWKS fetched only from a pre-configured trusted location. This is the recommended implementation approach when dynamic JWKS URL resolution is not required — ignore potentially dangerous header parameters rather than process them from untrusted input. No JWKS injection attack vector exists; signature verification uses only the deployment-configured JWKS endpoint. No violations found.

**Reason:** RFC 8725 §3.3 satisfied: jku/jwk/x5u ignored; JWKS from pre-configured trusted URL only. JWKS injection attack vector eliminated. PASS.

**Phrasing-B:**
PASS. RS ignores jku/jwk/x5u JOSE header parameters per RFC 8725 §3.3; uses only deployment-configured JWKS endpoint. JWKS injection attack vector eliminated. PASS.

---

## Pair[CB4-3.4-1012-A]

**Territory:** T-rfc-oauth2 · T-cwe-authn
**Domain:** Token endpoint returns different error codes for unknown client_id vs wrong client_secret — client enumeration
**Expected verdict:** REPROCESS

**A:**
A token endpoint returns distinct error responses based on the type of authentication failure: if the `client_id` is not registered, the response is `{"error":"unknown_client","error_description":"Client not found"}`. If the `client_id` is registered but the `client_secret` is incorrect, the response is `{"error":"invalid_client","error_description":"Wrong client credentials"}`. An attacker can use these distinct responses to enumerate registered client IDs by trying arbitrary IDs and distinguishing the two error messages.

**B:**
REPROCESS. RFC 6749 §5.2 specifies that failed client authentication must return the `invalid_client` error code. The specification does not provide for a distinct `unknown_client` code — this is a custom extension that violates the spirit and intent of RFC 6749 §5.2. More critically, CWE-204 (Observable Response Discrepancy) applies: the distinct error responses allow an unauthenticated attacker to determine which client IDs are registered in the system. A registered client ID is a partially privileged identifier — knowing it enables targeted credential guessing attacks against that specific client. RFC 6749 §5.2's single `invalid_client` code for all authentication failures is intentionally designed to prevent this enumeration. The "helpful error messages" argument is rejected: informativeness to legitimate developers does not justify the enumeration attack surface against a system listing confidential client registrations.

**Reason:** RFC 6749 §5.2: authentication failures must return invalid_client without distinguishing unknown client_id vs wrong secret. CWE-204: observable response discrepancy enables client ID enumeration. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 6749 §5.2 provides one error code for authentication failures: invalid_client. Distinct responses for "not found" vs "wrong secret" enable client_id enumeration per CWE-204. Registered client IDs should not be discoverable by unauthenticated parties. RFC 6749's single error code design intentionally prevents this. REPROCESS — unify error responses.

---

## Pair[CB4-3.4-1012-B]

**Territory:** T-rfc-oauth2 · T-cwe-authn
**Domain:** Token endpoint returns uniform `invalid_client` for all client authentication failures
**Expected verdict:** PASS

**A:**
The token endpoint returns `{"error":"invalid_client"}` for all client authentication failures — whether the client_id is unregistered, the client_secret is wrong, or the authentication method does not match. The response body contains no additional description that distinguishes the failure type. HTTP status code is 401 for all authentication failures. The authentication implementation internally logs the specific failure reason (for operator investigation) without exposing it to the caller.

**B:**
PASS. RFC 6749 §5.2 satisfied: uniform `invalid_client` error for all authentication failures prevents client ID enumeration. CWE-204 mitigated: no observable response discrepancy between known and unknown client IDs. Internal logging of specific failure reasons is correct practice — operators can investigate without exposing the information externally. No violations found.

**Reason:** RFC 6749 §5.2 satisfied. Uniform error response prevents client enumeration. CWE-204 mitigated. Internal logging maintains operator visibility. PASS.

**Phrasing-B:**
PASS. RFC 6749 §5.2: uniform invalid_client error for all authentication failures. No observable discrepancy between unknown_client and wrong_secret. CWE-204 mitigated. Specific failure reason logged internally for operator investigation only. PASS.

---

## Pair[CB4-3.4-1013-A]

**Territory:** T-asvs-token · T-oauth-oidc
**Domain:** OAuth security events (failed authentication, redirect_uri mismatches) not logged
**Expected verdict:** CONCERN

**A:**
An OAuth 2.0 authorization server does not log security-relevant events: failed client authentication attempts, redirect_uri mismatch rejections, invalid authorization code submissions, CSRF state mismatches, and invalid scope requests all result in error responses to the client but generate no server-side log entries. The implementation comment: "We return the correct RFC 6749 error codes. Logging is handled at the network layer by our load balancer access logs, which capture the request URL but not the request body."

**B:**
CONCERN. ASVS V8.3.2 states: "Verify that the information security event log captures enough detail to enable forensic investigation of security incidents." OAuth security events — failed client authentication attempts, redirect_uri mismatch rejections, state parameter mismatches — are exactly the events that enable forensic investigation of authorization-server-targeted attacks (credential stuffing against clients, redirect URI manipulation attempts, CSRF attempts). Without application-level event logging, the only signal available is the load-balancer access log, which captures the URL but not the POST body parameters (client_id, state values, redirect_uri values) that are essential for forensic correlation. ASVS V8.3.2 uses "Verify" language — this is a SHOULD-equivalent obligation in the ASVS Level 1/2 framework, not an unconditional MUST in RFC terms. No RFC requires specific OAuth event logging with MUST language. The gap is guidance-level: CONCERN. Correct practice: log all security-relevant OAuth events with timestamp, client_id, IP address, event type, and outcome — excluding credentials and PII.

**Reason:** ASVS V8.3.2 guidance (security event log) not satisfied for OAuth server-side events. No RFC MUST for specific OAuth event logging. Guidance-level gap = CONCERN.

**Phrasing-B:**
CONCERN. ASVS V8.3.2: security event log must capture detail sufficient for forensic investigation. Failed client authentication, redirect_uri mismatches, state mismatches are forensically relevant OAuth events that are not being logged. Network-layer access logs lack POST body content needed for correlation. No RFC MUST-level logging requirement for OAuth specifically. ASVS guidance gap = CONCERN. Recommend: application-level event logging for all OAuth security events (excluding credentials and PII).

---

## Pair[CB4-3.4-1013-B]

**Territory:** T-asvs-token · T-oauth-oidc
**Domain:** OAuth security events logged with forensically relevant details
**Expected verdict:** PASS

**A:**
The authorization server logs security-relevant events at application level: failed client authentication (event_type, client_id, IP, timestamp, failure_reason), redirect_uri mismatch rejections (client_id, submitted_uri, registered_uri, IP), state parameter mismatches (client_id, session_id), invalid authorization code submissions (client_id, code_hash, IP), invalid scope requests (client_id, requested_scopes). Logs are written to a structured logging system (JSON). Client secrets, authorization codes in clear, and user PII are explicitly excluded from log payloads through field-level redaction.

**B:**
PASS. ASVS V8.3.2 satisfied: application-level logging captures security-relevant OAuth events with sufficient forensic detail — event type, client_id, timestamp, IP, outcome. V8.2.2 satisfied: credentials and PII excluded via field-level redaction. The logging covers the events most relevant to detecting AS-targeted attacks: authentication brute force, redirect URI manipulation, CSRF attempts. No violations found.

**Reason:** ASVS V8.3.2 guidance satisfied: security events logged with forensic detail. V8.2.2 satisfied: credentials/PII excluded from logs. PASS.

**Phrasing-B:**
PASS. Application-level structured logging for all OAuth security events: failed auth, redirect_uri mismatch, state mismatch, invalid code, invalid scope — with event_type, client_id, timestamp, IP. Credentials and PII excluded per V8.2.2. ASVS V8.3.2 guidance satisfied. PASS.

---

## Pair[CB4-3.4-1014-A]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** RS does not validate `exp` claim — expired tokens accepted indefinitely
**Expected verdict:** REPROCESS

**A:**
A resource server validates JWT access tokens for: signature (RS256, JWKS-backed), `iss` (exact allowlist), `aud` (RS identifier), `typ` (at+JWT), and `scope`. The `exp` claim is not validated — expired tokens are accepted as long as the signature, iss, aud, typ, and scope checks pass. The developer reasoning: "We issue 15-minute access tokens. In practice, clients renew before expiry. Checking exp adds latency. And if a client sends an expired token, the user experience suffers — we prefer to accept it gracefully."

**B:**
REPROCESS. RFC 9068 §4 is explicit: "The JWT MUST be rejected if it contains a claim that is NOT valid, in particular: ... if the `exp` is less than the current Unix time, the JWT is expired and MUST be rejected." This is an unconditional MUST. The `exp` claim is the sole mechanism by which the access token lifecycle is enforced at the resource server — without exp validation, access tokens do not expire from the RS's perspective, regardless of their issued TTL. An attacker who obtains an access token faces no time-bounded exploitation window; the token remains valid indefinitely at the RS. The "graceful UX" argument is rejected: the MUST is unconditional; user experience cannot override a normative expiry requirement. Clients should handle 401 responses by refreshing their token — that is the correct UX pattern. Additionally: the "15-minute TTL in practice" reasoning confirms that tokens expire after issuance — the exact scenario the exp check is designed to handle.

**Reason:** RFC 9068 §4 MUST: RS must reject JWT when exp < current time. Skipping exp validation means tokens never expire at the RS. "UX preference" argument rejected. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 9068 §4 MUST: RS must reject JWT when exp < current Unix time. No exp validation means access tokens have unlimited lifetime at the RS regardless of their issued TTL — a stolen 15-minute token is valid forever. "Graceful UX" reasoning does not override a MUST requirement; clients should refresh on 401. REPROCESS.

---

## Pair[CB4-3.4-1014-B]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** RS validates `exp` claim; expired tokens rejected with 401
**Expected verdict:** PASS

**A:**
The resource server's JWT validation pipeline includes exp check at step 4 (after typ → iss → aud, before scope): assert `exp > current_time`. A 30-second clock skew tolerance is not applied for exp (the exp boundary is treated as exact for simplicity; a generous token TTL of 15 minutes means the practical impact of clock drift is negligible). Expired tokens receive 401 with `WWW-Authenticate: Bearer error="invalid_token" error_description="Token expired"`. The client library handles 401 by attempting a token refresh before retrying.

**B:**
PASS. RFC 9068 §4 MUST satisfied: exp validated at RS; tokens with exp < current_time are rejected with 401. The choice to use exact exp comparison (no clock skew tolerance) is conservative but acceptable — it may occasionally reject tokens within the last few seconds of their TTL due to clock differences, but the security property (no expired tokens accepted) is fully enforced. For a 15-minute TTL, this is operationally appropriate. Correct client behavior (401 → refresh → retry) is implemented. No violations found.

**Reason:** RFC 9068 §4 MUST satisfied. exp validated; expired tokens rejected. Correct client 401-handling implemented. PASS.

**Phrasing-B:**
PASS. RS validates exp per RFC 9068 §4 MUST: exp < current_time → 401 invalid_token. Exact expiry boundary enforced. Client handles 401 with token refresh. Expiry enforcement fully operational. PASS.
