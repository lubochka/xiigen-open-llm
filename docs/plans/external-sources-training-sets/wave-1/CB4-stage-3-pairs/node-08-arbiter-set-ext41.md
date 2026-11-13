# CB4 Stage 3 — Node 08 Arbiter Set ext41
# Sub-phase 3.4 · Security and Authorization Model
# Sequences CB4-3.4-1063 through CB4-3.4-1070 (8 base pairs × 2 phrasings = 16 instances)
# New domains: dpop-htu-htm-binding, par-request_uri-client-binding, oidc-hybrid-c_hash, dpop-jti-replay, scope-downgrade-response, tls-version-endpoint, oidc-hybrid-at_hash, expires_in-absent
# Running total after this file: 120 + (1070 − 60) × 2 = 2140 instances (1070 base pairs)
# Sources: RFC 9449 §4.2/§7.2, RFC 9126 §5, OIDC Core §3.3.2.11, RFC 9449 §11.1, RFC 6749 §3.3, RFC 9325 §4, OIDC Core §3.3.2.11, RFC 6749 §5.1

---

## Pair[CB4-3.4-1063-A]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** DPoP proof `htu` and `htm` claims not validated at RS — proof accepted for different URI and method
**Expected verdict:** REPROCESS

**A:**
A resource server validates DPoP (Demonstrating Proof of Possession, RFC 9449) proofs from clients. The RS verifies: DPoP proof signature (using the public key in the proof), that the `jkt` value in the token's `cnf` claim matches the DPoP public key, the `iat` freshness window, and the `jti` for replay prevention. However, the RS does not validate the `htu` (HTTP Target URI) or `htm` (HTTP Method) claims in the DPoP proof. A DPoP proof signed for `POST https://api.example.com/payments` would be accepted at `DELETE https://api.example.com/accounts/123`. The rationale: "We validate the key binding via jkt — the uri/method are redundant since we check the token's audience anyway."

**B:**
REPROCESS. RFC 9449 §7.2 is explicit: the RS MUST verify that "the htm claim matches the HTTP method of the current request" and "the htu claim matches the HTTP target URI (without query and fragment parts)." These are normative MUST requirements. The purpose of htu and htm binding is to constrain DPoP proofs to a specific endpoint and method — a proof cannot be captured from one endpoint and replayed at another. Without htu/htm validation: an attacker who intercepts a DPoP proof from a low-privilege endpoint (e.g., `GET /user/profile`) can replay it at a high-privilege endpoint (e.g., `DELETE /admin/accounts`) as long as the key binding and signature are valid. The "aud validates the RS" argument is rejected: aud proves which RS, not which endpoint or method within that RS. htu and htm are the endpoint-level binding mechanism. Both are unconditional MUST requirements.

**Reason:** RFC 9449 §7.2 MUST: RS must verify htu matches request URI and htm matches HTTP method. Omitting htu/htm allows DPoP proof replay across different endpoints and methods. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 9449 §7.2 MUST: RS must validate htu (target URI) and htm (HTTP method) against current request. Accepting a DPoP proof without URI/method binding allows captured proofs to be replayed at different endpoints — the key binding via jkt does not restrict which endpoint the proof is valid for. REPROCESS.

---

## Pair[CB4-3.4-1063-B]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** RS validates DPoP proof `htu` and `htm` against current request — endpoint and method binding enforced
**Expected verdict:** PASS

**A:**
The resource server validates DPoP proofs fully per RFC 9449. The validation pipeline checks: (1) DPoP proof signature valid with embedded public key; (2) `jkt` in token's `cnf` claim matches SHA-256 thumbprint of DPoP public key; (3) `iat` freshness (within 60-second window); (4) `jti` not in replay list (tracked per key+htu+htm for the proof lifetime); (5) `htm` exactly matches the HTTP method of the current request (case-sensitive); (6) `htu` matches the HTTP target URI (excluding query string and fragment) of the current request. Requests where any of these checks fail are rejected with 401 and `WWW-Authenticate: DPoP error="invalid_dpop_proof"`.

**B:**
PASS. RFC 9449 §7.2 MUST satisfied: htm and htu validated against current request on every DPoP-protected call. DPoP proofs are endpoint-and-method specific — a proof captured from one endpoint cannot be replayed at another. Combined with jti replay prevention and iat freshness, full DPoP binding is enforced. No violations found.

**Reason:** RFC 9449 §7.2 MUST satisfied. htm and htu validated; DPoP proof bound to specific URI and method. Replay across endpoints prevented. PASS.

**Phrasing-B:**
PASS. RS validates htm (exact HTTP method match) and htu (URI without query/fragment) per RFC 9449 §7.2 MUST. DPoP proofs are endpoint-and-method-constrained. Combined with jkt, jti, and iat checks, full DPoP binding is operational. PASS.

---

## Pair[CB4-3.4-1064-A]

**Territory:** T-rfc-oauth2 · T-asvs-token
**Domain:** PAR `request_uri` used at authorization endpoint without client_id binding verification
**Expected verdict:** REPROCESS

**A:**
An Authorization Server implements PAR (Pushed Authorization Requests, RFC 9126). At the PAR endpoint, the client pushes an authorization request and receives a `request_uri` valid for 30 seconds. At the authorization endpoint, the AS processes requests containing `request_uri` parameter values. However, the AS does not verify that the `client_id` provided at the authorization endpoint matches the `client_id` that originally pushed the request to the PAR endpoint. Any client that knows a `request_uri` value can use it. The rationale: "The request_uri is short-lived (30 seconds) and opaque — guessing it is infeasible. Client binding is an edge case for our threat model."

**B:**
REPROCESS. RFC 9126 §5 is normative: "When processing a request using a request_uri, the authorization server MUST check that the client_id provided in the authorization request matches the client_id associated with the pushed authorization request object." This is an unconditional MUST. The client binding requirement exists to prevent cross-client substitution attacks: if Client A pushes a request and obtains a `request_uri`, Client B (who obtains the `request_uri` by any means — eavesdropping, SSRF on the PAR endpoint, insider threat) should not be able to use it to initiate an authorization flow as itself. Without client binding: a malicious client could use another client's `request_uri` to complete an authorization flow with different client credentials, defeating the authorization model. The "short-lived and opaque makes guessing infeasible" argument is rejected: the MUST applies regardless of opaqueness — legitimate client binding is a normative requirement, not merely a residual-risk countermeasure.

**Reason:** RFC 9126 §5 MUST: AS must verify client_id at authorization endpoint matches client_id that pushed the PAR request. Omitting binding allows cross-client request_uri substitution. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 9126 §5 MUST: client_id at authorization endpoint must match client_id of the original PAR request. No binding check means any party with the request_uri can initiate an authorization flow for a different client. "Short-lived and opaque" does not substitute for the MUST binding requirement. REPROCESS.

---

## Pair[CB4-3.4-1064-B]

**Territory:** T-rfc-oauth2 · T-asvs-token
**Domain:** PAR `request_uri` — AS verifies client_id binding at authorization endpoint
**Expected verdict:** PASS

**A:**
The Authorization Server stores the `client_id` associated with each PAR request when the `request_uri` is issued. At the authorization endpoint, when a request includes a `request_uri`, the AS: (1) resolves the `request_uri` to the stored authorization request object; (2) compares the `client_id` query parameter in the authorization request against the `client_id` stored with the pushed request; (3) rejects with `error=invalid_request` if they do not match. The `request_uri` is single-use and expires after 30 seconds regardless of whether it is used.

**B:**
PASS. RFC 9126 §5 MUST satisfied: client_id at authorization endpoint verified against PAR-associated client_id. Cross-client substitution attack prevented. Single-use and expiry per RFC 9126 §4 satisfied. No violations found.

**Reason:** RFC 9126 §5 MUST satisfied. client_id binding verified at authorization endpoint. Cross-client request_uri substitution rejected. PASS.

**Phrasing-B:**
PASS. AS stores client_id with PAR request and verifies match at authorization endpoint per RFC 9126 §5 MUST. Mismatch → invalid_request. request_uri single-use + 30s expiry per RFC 9126 §4. PASS.

---

## Pair[CB4-3.4-1065-A]

**Territory:** T-oauth-oidc · T-rfc-jwt
**Domain:** OIDC hybrid flow — `id_token` missing `c_hash` (code hash) claim when authorization code is returned
**Expected verdict:** REPROCESS

**A:**
An OIDC (OpenID Connect) Authorization Server implements the hybrid flow (`response_type=code id_token`). The authorization endpoint returns both the authorization code and an `id_token` in the fragment. The `id_token` is signed (RS256) and includes standard claims (`iss`, `sub`, `aud`, `exp`, `iat`, `nonce`) but does not include the `c_hash` (code hash) claim. The AS developer reasoning: "c_hash is an optimization for the RP to verify the code. We bind the code to the session in our database — that's equivalent protection."

**B:**
REPROCESS. OIDC Core §3.3.2.11 is explicit: "If the ID Token is issued from the Hybrid Endpoint when the code is returned from the Authorization Endpoint... c_hash: Code hash value... If the ID Token is issued with a code in response to an Authorization Request, this Claim MUST be present in the ID Token response to the Authorization Request." This is a MUST. The `c_hash` provides cryptographic binding between the `id_token` and the authorization code: the RP can verify that the code returned in the front-channel matches the code the AS intended to issue (protecting against code substitution attacks in the fragment). Without `c_hash`, the RP has no cryptographic mechanism to verify that the authorization code was produced by the same AS operation that produced the `id_token`. The "database binding" argument is rejected: the MUST requires the RP-verifiable `c_hash` claim in the id_token; server-side state does not satisfy the RP-side binding requirement.

**Reason:** OIDC Core §3.3.2.11 MUST: c_hash claim required in id_token when code is returned from hybrid flow authorization endpoint. Absent c_hash breaks code-to-id_token cryptographic binding. REPROCESS.

**Phrasing-B:**
REPROCESS. OIDC Core §3.3.2.11 MUST: id_token from hybrid flow authorization endpoint MUST include c_hash when authorization code is also returned. c_hash cryptographically binds the code to the id_token; absent = RP cannot verify code integrity against id_token. "Database binding" does not satisfy the RP-side MUST. REPROCESS.

---

## Pair[CB4-3.4-1065-B]

**Territory:** T-oauth-oidc · T-rfc-jwt
**Domain:** OIDC hybrid flow — `id_token` includes `c_hash` binding the authorization code
**Expected verdict:** PASS

**A:**
The Authorization Server includes `c_hash` in the `id_token` when the hybrid flow returns both an authorization code and an `id_token` from the authorization endpoint. The `c_hash` is computed as: `BASE64URL(LEFT_HALF(SHA256(ASCII(code))))` where the hash algorithm matches the `alg` in the JOSE header (RS256 → SHA-256, ES256 → SHA-256, RS384 → SHA-384). The RP validates `c_hash` by recomputing the hash of the received code and comparing it against the `c_hash` claim in the `id_token`. When `at_hash` is also applicable (access token also returned from the authorization endpoint), it is included with the same hash computation pattern.

**B:**
PASS. OIDC Core §3.3.2.11 MUST satisfied: c_hash present in id_token; computed correctly using the same hash algorithm as the JOSE alg header; RP can verify code-to-id_token binding. No violations found.

**Reason:** OIDC Core §3.3.2.11 MUST satisfied. c_hash computed and present; RP can verify code integrity. PASS.

**Phrasing-B:**
PASS. id_token includes c_hash per OIDC Core §3.3.2.11 MUST: BASE64URL(LEFT_HALF(SHA256(code))). RP validates c_hash against received authorization code. Code-to-id_token cryptographic binding enforced. PASS.

---

## Pair[CB4-3.4-1066-A]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** DPoP proof `jti` not tracked server-side — same DPoP proof accepted multiple times (replay)
**Expected verdict:** REPROCESS

**A:**
A resource server validates DPoP proofs per RFC 9449. The RS checks signature, jkt-to-token binding, iat freshness (60-second window), htu, and htm. However, the RS does not maintain a server-side store of observed `jti` (JWT ID) values — DPoP proof `jti` claims are not persisted or checked for uniqueness. The same DPoP proof can be submitted multiple times within the iat freshness window and will be accepted each time. The developer rationale: "jti tracking requires distributed state. The iat freshness window already limits the replay window to 60 seconds — that's acceptable."

**B:**
REPROCESS. RFC 9449 §7.2 is normative: the RS MUST "check that the jti claim value is unique within a reasonable time window." RFC 9449 §11.1 elaborates: "To prevent DPoP proof replay... the server has to ... store used jti values in a time-bounded cache." The 60-second iat window defines the outer boundary of the replay window, but jti uniqueness enforcement is what closes it — without jti tracking, any network observer who captures a DPoP proof can replay it as many times as they wish within the 60-second window. The "distributed state overhead" argument is rejected: the MUST is unconditional; jti replay caches (e.g., a Redis set with TTL matching the freshness window) are a standard implementation. A 60-second distributed replay window for sensitive API calls is not acceptable.

**Reason:** RFC 9449 §7.2 MUST: RS must check jti uniqueness. Without jti tracking, DPoP proofs are replayable within the iat freshness window. "iat window substitutes for jti" rejected. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 9449 §7.2 MUST: RS must verify jti uniqueness to prevent DPoP proof replay. iat freshness window (60s) defines replay boundary; jti uniqueness closes it. Without jti tracking, captured DPoP proofs are replayable up to 60 times per second within the window. Distributed jti cache (Redis with TTL) is the standard solution. REPROCESS.

---

## Pair[CB4-3.4-1066-B]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** DPoP proof `jti` tracked in server-side cache — replay prevention enforced
**Expected verdict:** PASS

**A:**
The resource server maintains a per-key-thumbprint jti replay cache for DPoP proofs. When a DPoP proof is validated: (1) the `jti` + `jkt` + `htu` + `htm` composite key is looked up in the cache; (2) if found, the request is rejected with 401 (DPoP proof replay); (3) if not found, the composite key is added to the cache with TTL equal to the iat freshness tolerance window (60 seconds) plus clock skew tolerance (5 seconds). The jti cache is a shared Redis (Remote Dictionary Server) cluster shared across all RS instances behind the load balancer. jti values are not predictable — they are UUID (Universally Unique Identifier) v4 values generated by the DPoP library.

**B:**
PASS. RFC 9449 §7.2 MUST satisfied: jti uniqueness checked; replay cache with appropriate TTL prevents DPoP proof reuse. Distributed cache ensures consistency across RS instances. UUID v4 jti values are unpredictable and cannot be pre-generated by an attacker. No violations found.

**Reason:** RFC 9449 §7.2 MUST satisfied. jti tracked in distributed cache with TTL. DPoP proof replay prevented. PASS.

**Phrasing-B:**
PASS. RS maintains shared jti replay cache (Redis, TTL=freshness window) per RFC 9449 §7.2 MUST. jti+jkt+htu+htm composite key prevents proof replay across endpoints and instances. UUID v4 jti values are unpredictable. PASS.

---

## Pair[CB4-3.4-1067-A]

**Territory:** T-rfc-oauth2 · T-asvs-token
**Domain:** Scope downgrade — AS issues token with narrowed scope without including `scope` in token response
**Expected verdict:** REPROCESS

**A:**
An Authorization Server issues access tokens with a scope that is narrower than what the client requested. For example: client requests `scope=read:profile write:data`; AS issues a token with scope `read:profile` only (silently omits `write:data`). The token response body does not include the `scope` parameter — it contains only `access_token`, `token_type`, and `expires_in`. The AS rationale: "The client will discover the effective scope when it gets a 403 from the resource server. Including scope in the response is optional per RFC 6749."

**B:**
REPROCESS. RFC 6749 §3.3 is normative: "If the issued access token scope is different from the one requested by the client, the authorization server MUST include the `scope` response parameter to inform the client of the actual scope granted." This is an unconditional MUST. The client has no other reliable mechanism to discover the effective scope: the JWT access token payload may not be readable by the client, and relying on 403 responses from resource servers to discover scope gaps is operationally unreliable and introduces latency-based discovery loops. The "scope is optional" argument is rejected: RFC 6749 §3.3 makes scope MUST when the issued scope differs from the requested scope — the optionality applies only when the issued scope exactly matches the requested scope.

**Reason:** RFC 6749 §3.3 MUST: when issued scope differs from requested scope, AS must include scope in token response. Silent scope downgrade without disclosure violates MUST. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 6749 §3.3 MUST: if issued scope ≠ requested scope, AS must return scope parameter in token response. Omitting scope on downgrade leaves client unable to know what was granted; relies on 403 discovery — operationally broken. "Scope is optional" correct only when scope matches; MUST applies when different. REPROCESS.

---

## Pair[CB4-3.4-1067-B]

**Territory:** T-rfc-oauth2 · T-asvs-token
**Domain:** Scope downgrade — AS includes `scope` in token response when issued scope differs from requested scope
**Expected verdict:** PASS

**A:**
When the Authorization Server issues an access token with a scope that differs from the requested scope (whether by policy restriction, resource owner consent, or scope validation), the token response includes the `scope` parameter explicitly: e.g., `{"access_token": "...", "token_type": "Bearer", "expires_in": 3600, "scope": "read:profile"}`. The client validates the returned `scope` against what it needs and aborts or degrades gracefully if required scopes were not granted. When the issued scope exactly matches the requested scope, the `scope` parameter is omitted (per RFC 6749 §3.3 optionality for exact matches).

**B:**
PASS. RFC 6749 §3.3 MUST satisfied: scope parameter included in token response when issued scope differs from requested scope. Client can programmatically detect scope downgrade and handle it correctly before making resource server calls. No violations found.

**Reason:** RFC 6749 §3.3 MUST satisfied. Scope parameter returned when issued scope ≠ requested scope. Client-side scope validation enabled. PASS.

**Phrasing-B:**
PASS. AS returns scope in token response per RFC 6749 §3.3 MUST when issued scope ≠ requested scope. Client detects downgrade programmatically; no reliance on 403 discovery. Exact-match scope omission per RFC 6749 §3.3 optionality correctly applied. PASS.

---

## Pair[CB4-3.4-1068-A]

**Territory:** T-rfc-oauth2 · T-asvs-token
**Domain:** Token endpoint accepts TLS 1.0 and TLS 1.1 connections — deprecated protocol versions permitted
**Expected verdict:** REPROCESS

**A:**
An Authorization Server's token endpoint accepts connections using TLS (Transport Layer Security) 1.0, TLS 1.1, TLS 1.2, and TLS 1.3. The server has not explicitly disabled TLS 1.0 and TLS 1.1. The operations team rationale: "TLS 1.0/1.1 support ensures backward compatibility with legacy clients. Our monitoring shows no attacks against these protocol versions in our infrastructure. TLS 1.2 is always negotiated first — the lower versions are only fallback."

**B:**
REPROCESS. RFC 9325 §4 (BCP 195, the current TLS Best Practices RFC) states: "Implementations MUST NOT negotiate TLS 1.0 and TLS 1.1." This is an unconditional MUST NOT. TLS 1.0 is vulnerable to BEAST, POODLE, and similar attacks; TLS 1.1 is also deprecated by RFC 8996. An OAuth token endpoint is a credential-issuing endpoint: client secrets, authorization codes, and bearer tokens are transmitted over this connection. Accepting deprecated TLS versions exposes these credentials to protocol-level downgrade attacks. The "always negotiated first" fallback argument is rejected: a downgrade attack intentionally forces lower TLS versions during negotiation; servers that accept TLS 1.0/1.1 are susceptible to forced downgrade by on-path attackers. The MUST NOT is unconditional regardless of observed traffic patterns.

**Reason:** RFC 9325 §4 (BCP 195) MUST NOT: token endpoint must not accept TLS 1.0 or TLS 1.1. Deprecated protocol versions expose credential transmission to downgrade attacks. "Backward compatibility" and "no observed attacks" do not override MUST NOT. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 9325 §4 MUST NOT: TLS 1.0 and TLS 1.1 must not be negotiated. Both versions are deprecated per RFC 8996 and vulnerable to protocol-level downgrade attacks. OAuth token endpoint is a credential-issuing endpoint — accepting deprecated TLS exposes client secrets and bearer tokens. "Fallback only" does not satisfy MUST NOT. REPROCESS.

---

## Pair[CB4-3.4-1068-B]

**Territory:** T-rfc-oauth2 · T-asvs-token
**Domain:** Token endpoint requires TLS 1.2 minimum; TLS 1.0 and TLS 1.1 disabled
**Expected verdict:** PASS

**A:**
The Authorization Server's token endpoint is configured to accept TLS 1.2 and TLS 1.3 only. TLS 1.0 and TLS 1.1 are explicitly disabled in the TLS configuration (OpenSSL `MinProtocol = TLSv1.2`). The server prefers TLS 1.3 when supported by the client. Cipher suites are restricted to forward-secret (PFS (Perfect Forward Secrecy)) suites: TLS 1.3 uses `TLS_AES_256_GCM_SHA384` and `TLS_CHACHA20_POLY1305_SHA256`; TLS 1.2 accepts only `ECDHE-*-AES*-GCM-*` suites. TLS version and cipher suite configuration is reviewed quarterly and updated per current BCP 195 guidance.

**B:**
PASS. RFC 9325 §4 MUST NOT satisfied: TLS 1.0 and TLS 1.1 explicitly disabled; TLS 1.2 minimum enforced. TLS 1.3 preferred. PFS (Perfect Forward Secrecy) cipher suites only. No deprecated protocol or cipher suites accepted. No violations found.

**Reason:** RFC 9325 §4 MUST NOT satisfied. TLS 1.0 and 1.1 disabled; TLS 1.2+ required; TLS 1.3 preferred; PFS suites only. PASS.

**Phrasing-B:**
PASS. Token endpoint enforces TLS 1.2 minimum per RFC 9325 §4 MUST NOT. TLS 1.3 preferred; TLS 1.0 and 1.1 explicitly disabled. PFS cipher suites only. BCP 195 compliant. PASS.

---

## Pair[CB4-3.4-1069-A]

**Territory:** T-oauth-oidc · T-rfc-jwt
**Domain:** OIDC hybrid flow — `id_token` missing `at_hash` when access token is returned from authorization endpoint
**Expected verdict:** REPROCESS

**A:**
An OIDC Authorization Server implements the hybrid flow with `response_type=code token id_token`, returning authorization code, access token, and id_token from the authorization endpoint (all in the fragment). The `id_token` includes `c_hash` (binding to the authorization code) and standard claims, but does not include `at_hash` (access token hash). The AS rationale: "c_hash covers the code; the access token is short-lived (5 minutes); at_hash is optional per OIDC Core."

**B:**
REPROCESS. OIDC Core §3.3.2.11 is explicit: "at_hash: Access Token hash value... If the ID Token is issued with an access_token in response to an Authorization Request, this Claim MUST be present in the ID Token response to the Authorization Request." The hybrid flow with `response_type=code token id_token` returns an access token from the authorization endpoint — this triggers the MUST. The `at_hash` provides cryptographic binding between the `id_token` and the access token in the front channel: without it, an on-path attacker can substitute a different access token in the fragment without the RP detecting the substitution via the id_token. The "at_hash is optional" characterization is incorrect: it is optional only when no access token is returned from the authorization endpoint (code-only flows); when an access token is returned from the authorization endpoint, `at_hash` is MUST.

**Reason:** OIDC Core §3.3.2.11 MUST: at_hash required in id_token when access token is returned from hybrid flow authorization endpoint. Absent at_hash enables access token substitution attack in front channel. REPROCESS.

**Phrasing-B:**
REPROCESS. OIDC Core §3.3.2.11 MUST: at_hash MUST be present in id_token when access_token is issued from the hybrid authorization endpoint. Without at_hash, RP cannot verify access token authenticity against id_token — front-channel access token substitution undetectable. "Optional per OIDC Core" applies only to flows that don't return access token at auth endpoint. REPROCESS.

---

## Pair[CB4-3.4-1069-B]

**Territory:** T-oauth-oidc · T-rfc-jwt
**Domain:** OIDC hybrid flow — `at_hash` present in `id_token` binding the access token
**Expected verdict:** PASS

**A:**
When the AS hybrid flow returns an access token from the authorization endpoint, the `id_token` includes `at_hash` computed as `BASE64URL(LEFT_HALF(SHA256(ASCII(access_token))))` (matching the hash algorithm of the JOSE `alg` header). The RP validates `at_hash` after receiving the fragment, computing the expected hash of the received access token and comparing it against the `at_hash` claim in the `id_token`. When using `response_type=code id_token` (no access token at auth endpoint), `at_hash` is correctly omitted (it is optional in that flow). When `c_hash` is required (code returned), it is also present.

**B:**
PASS. OIDC Core §3.3.2.11 MUST satisfied: at_hash present and correctly computed when access token returned from authorization endpoint. RP validates at_hash for access token integrity. at_hash correctly omitted in flows where no access token is returned at the authorization endpoint. No violations found.

**Reason:** OIDC Core §3.3.2.11 MUST satisfied. at_hash present and validated; access token front-channel substitution prevented. Correct omission in code-only flows. PASS.

**Phrasing-B:**
PASS. id_token includes at_hash per OIDC Core §3.3.2.11 MUST when access token returned at auth endpoint. BASE64URL(LEFT_HALF(SHA256(access_token))) computed correctly. RP validates at_hash. Front-channel access token binding enforced. PASS.

---

## Pair[CB4-3.4-1070-A]

**Territory:** T-rfc-oauth2 · T-asvs-token
**Domain:** Token response omits `expires_in` — clients cannot determine token expiry time
**Expected verdict:** CONCERN

**A:**
An Authorization Server issues access tokens with a 30-minute lifetime. The token response contains `access_token` and `token_type` fields but omits the `expires_in` field. The AS rationale: "Clients should use the token until they get a 401, then refresh. The 30-minute TTL (Time-To-Live) is documented in our developer portal. expires_in is optional per RFC 6749 §5.1."

**B:**
CONCERN. RFC 6749 §5.1 states: "expires_in: RECOMMENDED. The lifetime in seconds of the access token. For example, the value '3600' denotes that the access token will expire in one hour from the time the response was generated. If omitted, the authorization server SHOULD provide the expiration time via other means or document the default value." RECOMMENDED (RFC 2119) is a guidance-level term: it indicates that there may be valid reasons to omit the field, but those reasons should be documented. "Optional per RFC 6749 §5.1" is technically accurate — this is not a MUST. However, the absence of `expires_in` forces clients into a reactive pattern (use until 401) rather than proactive token management (pre-fetch before expiry). Clients that follow the 401-first pattern generate additional failed requests and expose expiry timing to servers. The RFC 6749 §5.1 caveat ("SHOULD provide by other means") is partially satisfied by developer portal documentation, but programmatic discovery by clients is not possible. Guidance-level gap = CONCERN. Correct practice: include `expires_in` in all token responses.

**Reason:** RFC 6749 §5.1 RECOMMENDED: expires_in should be included; if omitted, SHOULD provide by other means. Not MUST → CONCERN. "Optional" technically correct but forces reactive client behavior. CONCERN.

**Phrasing-B:**
CONCERN. RFC 6749 §5.1 RECOMMENDED: expires_in should be present in token response. Omitting it forces clients into reactive 401-driven token refresh rather than proactive expiry management. "Documentation substitutes for expires_in" partially accepted per RFC 6749 §5.1 SHOULD; not programmatically discoverable. No MUST → CONCERN. Recommend: include expires_in in all token responses.
