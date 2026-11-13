# CB4 Stage 3 — Node 08 Arbiter Set ext36
# Sub-phase 3.4 · Security and Authorization Model
# Sequences CB4-3.4-933 through CB4-3.4-940 (8 base pairs × 2 phrasings = 16 instances)
# New domains: PAR enforcement bypass, offline_access scope issuance, JWT typ claim, DPoP-bound token as Bearer
# Running total after this file: 120 + (940 − 60) × 2 = 1880 instances (940 base pairs)
# Sources: RFC 9126 §4, OIDC Core §11, RFC 9068 §2.1, RFC 9449 §7.2

---

## Pair[CB4-3.4-933-A]

**Territory:** T-rfc-oauth2 · T-oauth-oidc
**Domain:** PAR `require_pushed_authorization_requests` set true; authorization endpoint still accepts non-PAR requests
**Expected verdict:** REPROCESS

**A:**
An authorization server's discovery metadata includes `"require_pushed_authorization_requests": true`. The authorization endpoint implementation, however, contains a fallback branch: if an incoming authorization request does not include a `request_uri` parameter (the PAR reference), the endpoint falls through to its legacy code path and processes the request as a plain direct authorization request. The developer rationale: "We set the metadata flag to signal PAR preference to new clients; legacy clients that don't support PAR need a migration path."

**B:**
REPROCESS. RFC 9126 §4 states: "If the server is configured to require pushed authorization requests, the server MUST reject the authorization request if it does not contain a valid `request_uri` parameter." The `require_pushed_authorization_requests=true` metadata value is not an advisory preference — it is a server-side enforcement declaration. Accepting direct authorization requests after advertising PAR-required status nullifies every PAR security property: parameter injection protection (PAR binds parameters server-side before the authorization endpoint is invoked); parameter confidentiality (PAR keeps scope, redirect_uri, and state off the user-agent); request integrity (PAR-issued request_uri is short-lived and client-bound). The "legacy migration" argument is rejected: a migration path must be implemented by temporarily relaxing the metadata flag, not by maintaining a permanently open bypass. Clients observing `require_pushed_authorization_requests=true` are entitled to rely on the guarantee it represents. The bypass means an attacker who skips PAR entirely can inject authorization parameters that the PAR mechanism was designed to prevent.

**Reason:** RFC 9126 §4 MUST: server that advertises require_pushed_authorization_requests=true MUST reject non-PAR authorization requests. Enforcement and metadata must be consistent; a metadata-advertised security posture that is not enforced is a deceptive capability claim and leaves all PAR-targetted attack vectors open.

**Phrasing-B:**
REPROCESS. RFC 9126 §4: when require_pushed_authorization_requests=true is advertised, the authorization endpoint MUST reject any request lacking a valid request_uri; this is not preference signaling — it is a binding enforcement commitment. The legacy fallback branch bypasses PAR entirely, defeating: parameter injection protection, parameter confidentiality, and request integrity guarantees. "Migration path" framing is rejected — the metadata flag must be set to false during a migration window; maintaining an open bypass while advertising PAR-required is inconsistent and removes all PAR protections. REPROCESS.

---

## Pair[CB4-3.4-933-B]

**Territory:** T-rfc-oauth2 · T-oauth-oidc
**Domain:** PAR enforcement — authorization endpoint correctly rejects non-PAR requests
**Expected verdict:** PASS

**A:**
The authorization server advertises `"require_pushed_authorization_requests": true`. The authorization endpoint implementation validates the `request_uri` parameter on every incoming request: if `request_uri` is absent, the endpoint returns `error=invalid_request` with `error_description="Request must be submitted via PAR endpoint"` and HTTP 400. For the PAR endpoint, the server validates: client authentication (client_secret_basic), binds the request_uri to the client_id, sets a 60-second expiry on the reference, and enforces one-time-use. Legacy client migration is handled by maintaining a separate instance with the flag set to false, explicitly documented as transitional.

**B:**
PASS. RFC 9126 §4 MUST satisfied: authorization endpoint enforces PAR-required status unconditionally; no fallback path exists. PAR endpoint satisfies RFC 9126 §2.2: client authentication required; request_uri bound to client_id (prevents cross-client use); 60-second expiry (well within the RECOMMENDED short-lived constraint); one-time-use enforced (prevents replay). Migration strategy is correctly isolated to a separate instance with separate metadata rather than maintaining a bypass in the production instance. All PAR security properties — parameter injection protection, parameter confidentiality, request integrity — are fully operational.

**Reason:** RFC 9126 §4 MUST satisfied unconditionally. PAR endpoint meets §2.2 requirements. The migration approach correctly separates concerns rather than creating a production bypass. PASS on all checked requirements.

**Phrasing-B:**
PASS. Authorization endpoint rejects non-PAR requests with error=invalid_request, satisfying RFC 9126 §4 MUST unconditionally. PAR endpoint enforces: client authentication, client_id binding of request_uri, 60-second TTL, one-time-use. Legacy migration isolated to separate instance — no production bypass. All PAR protections active.

---

## Pair[CB4-3.4-934-A]

**Territory:** T-rfc-oidc · T-oauth-oidc
**Domain:** AS issues refresh token without `offline_access` scope in the authorization request
**Expected verdict:** REPROCESS

**A:**
An OIDC authorization server issues refresh tokens with every successful authorization code flow response, regardless of whether the requesting client included `offline_access` in the `scope` parameter. The implementation comment: "Refresh tokens improve user experience by reducing re-authentication prompts. Since users have authenticated, granting refresh access is a reasonable default." A representative authorization request carries `scope=openid profile email` with no `offline_access` value.

**B:**
REPROCESS. OIDC Core §11 provides an unambiguous constraint: "A Refresh Token MUST NOT be returned unless the `offline_access` Scope was granted in the Authorization Request and the OP supports providing refresh tokens." Issuing a refresh token when `offline_access` was not requested violates this MUST NOT and constitutes a scope-enlargement on the server side — the user consented to a session-scoped token exchange; a refresh token grants the application continued access to the user's resources beyond the active session without the user's explicit consent to that capability. The "improved UX" reasoning is rejected: user experience preferences do not override MUST NOT normative text; an application that wants refresh token capability must include `offline_access` in its scope request, giving the user (and the OIDC prompt) the opportunity to consent to or deny that specific capability.

**Reason:** OIDC Core §11 MUST NOT: refresh token must not be issued unless offline_access was explicitly in the authorization request scope. Issuing without explicit scope request bypasses user consent for offline/long-term access. MUST NOT = REPROCESS.

**Phrasing-B:**
REPROCESS. OIDC Core §11 MUST NOT issue a Refresh Token unless offline_access was included in the scope of the Authorization Request. The AS issuing refresh tokens on every flow — regardless of scope — violates this unconditionally. The user authenticated for an online session; a refresh token extends the application's access beyond that session without the user's explicit consent to offline_access. "UX default" is not an exception to a MUST NOT clause. REPROCESS.

---

## Pair[CB4-3.4-934-B]

**Territory:** T-rfc-oidc · T-oauth-oidc
**Domain:** Refresh token only issued when `offline_access` scope explicitly granted
**Expected verdict:** PASS

**A:**
The authorization server's token issuance logic gates refresh token generation on explicit `offline_access` scope presence: `if (!requestedScopes.Contains("offline_access")) { return tokenResponse without refresh_token; }`. When `offline_access` is absent, the response contains `access_token`, `id_token`, and `expires_in` only. When `offline_access` is present, the authorization endpoint's consent screen explicitly names the capability ("Allow this application to access your account when you are not active?"), and after consent the token endpoint issues both access token and refresh token. The `offline_access` scope is listed in the AS's `scopes_supported` metadata.

**B:**
PASS. OIDC Core §11 MUST NOT satisfied: refresh token only issued when `offline_access` scope explicitly appears in the authorization request. User consent for offline access is obtained through the authorization endpoint's consent screen — the user is informed of and approves the capability. The AS metadata correctly advertises `offline_access` in `scopes_supported`, allowing clients to request it explicitly. Token responses correctly omit refresh token when offline access was not requested. The implementation fully honours the consent-based model for refresh token issuance.

**Reason:** OIDC Core §11 constraint satisfied: refresh token conditional on offline_access scope + user consent. No MUST NOT violated. PASS.

**Phrasing-B:**
PASS. Refresh token issuance is gated on offline_access scope presence in the authorization request, satisfying OIDC Core §11 MUST NOT. Consent screen names the offline access capability. Token responses omit refresh_token when offline_access is absent. PASS.

---

## Pair[CB4-3.4-935-A]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** JWT access token `typ` header absent (RFC 9068 §2.1 MUST `at+JWT`)
**Expected verdict:** REPROCESS

**A:**
An authorization server issues JWT access tokens. Inspection of issued tokens reveals that the JOSE header contains only `alg` and `kid` fields: `{"alg":"RS256","kid":"key-2024-01"}`. The `typ` header field is absent. The resource server validates the JWT signature, checks `iss`, `aud`, `exp`, and `scope` claims but performs no `typ` check — it accepts any RS256-signed JWT from the known issuer as a valid access token. A developer comment explains: "The `typ` field is metadata. Our tokens are structurally valid JWTs and that is sufficient."

**B:**
REPROCESS. RFC 9068 §2.1 MUST: "The JWT MUST include a `typ` claim set to the value `at+JWT`." This requirement is not metadata convenience — `typ: at+JWT` is a structural discriminator that prevents token confusion attacks. Without `typ: at+JWT`, there is no programmatic way to distinguish a JWT access token from an ID token, a JWT client assertion, or any other JWT type produced by the same issuer with the same signing key. A concrete attack: an attacker who obtains an ID token (issued with `{"alg":"RS256","kid":"key-2024-01"}` and the same issuer) can present it to the resource server as an access token. Without a `typ` check, the RS accepts it — the ID token has valid signature, correct `iss`, valid `exp`, and the aud claim in an ID token is the client_id (not the RS audience). The `typ: at+JWT` requirement exists precisely to foreclose this cross-token confusion channel. The "structurally valid JWT" argument is rejected: RFC 9068 is a profile-specific MUST that imposes requirements beyond the JWT base specification.

**Reason:** RFC 9068 §2.1 MUST `typ: at+JWT` in JWT access token header. Absent typ prevents RS from discriminating access tokens from ID tokens and other JWT types. Token confusion attack vector without this check. MUST violation = REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 9068 §2.1 MUST: JWT access tokens must carry `typ: at+JWT` in the JOSE header. The absent typ field means the resource server cannot programmatically distinguish access tokens from ID tokens or JWT client assertions produced by the same issuer. An ID token from the same issuer — with the same alg and kid — would pass the RS's validation checks; the RS has no typ-based rejection path. This token confusion attack vector is the precise motivation for the RFC 9068 typ requirement. REPROCESS.

---

## Pair[CB4-3.4-935-B]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** JWT access token with `typ: at+JWT`; RS validates typ before accepting
**Expected verdict:** PASS

**A:**
The authorization server issues JWT access tokens with JOSE headers containing `{"typ":"at+JWT","alg":"RS256","kid":"key-2024-01"}`. The resource server's token validation pipeline checks in the following order: (1) parse JWT header; (2) assert `typ == "at+JWT"` — reject with 401 if not present or wrong value; (3) look up signing key by `kid` against HTTPS JWKS cache; (4) verify RS256 signature; (5) validate `iss`, `aud` (RS's own identifier), `exp`, `iat`; (6) check `scope` claim contains required permission. Requests presenting ID tokens (`typ: JWT` or no `typ`) are rejected at step 2 before signature verification.

**B:**
PASS. RFC 9068 §2.1 MUST satisfied: `typ: at+JWT` present in JOSE header. RS validation pipeline correctly enforces `typ` check at the earliest step (before key lookup and signature verification), which prevents ID token and other JWT-type cross-presentation. Ordering the `typ` check before signature verification is correct practice — it avoids wasted key lookups on structurally wrong tokens and prevents the RS from accepting tokens whose structural discriminator is incorrect. All further validation steps (sig, iss, aud, exp, scope) are also correct. No violations found.

**Reason:** RFC 9068 §2.1 MUST satisfied. RS typ enforcement is correctly positioned in the validation pipeline. Token confusion attack vector eliminated by typ check. PASS.

**Phrasing-B:**
PASS. JWT access tokens carry `typ: at+JWT` as required by RFC 9068 §2.1 MUST. Resource server validates typ as the first check, rejecting ID tokens and other JWT types before signature verification. Full validation chain (typ → kid → sig → iss/aud/exp → scope) is correct. Token confusion attack vector eliminated. PASS.

---

## Pair[CB4-3.4-936-A]

**Territory:** T-rfc-oauth2 · T-rfc-jwt
**Domain:** DPoP-bound access token (`cnf/jkt`) accepted at RS without requiring DPoP proof
**Expected verdict:** REPROCESS

**A:**
The authorization server issues DPoP-bound access tokens that include a `cnf` claim with a `jkt` value (JWK Thumbprint of the client's DPoP public key). The resource server, however, accepts these tokens using standard Bearer token validation: it checks the `Authorization: Bearer <token>` header, validates the JWT signature, checks `iss`, `aud`, `exp`, and `scope`, but makes no check for a `DPoP` header or `cnf/jkt` claim. The RS developer reasoning: "We validate the signature and claims; the token is legitimate regardless of whether DPoP metadata is present in the header."

**B:**
REPROCESS. RFC 9449 §7.2 is explicit: "If the resource server determines that the access token was issued with a DPoP-bound key (i.e., the access token contains a `cnf/jkt` claim), the resource server MUST check for the existence of the DPoP HTTP request header field and MUST process it according to the rules in Section 4.3." Accepting a DPoP-bound token without requiring the DPoP proof defeats the entire sender-constraint property: a DPoP-bound token is issued with the explicit expectation that only the holder of the corresponding private key can use it at a resource server. If the RS accepts it as plain Bearer, an attacker who steals the access token — without possessing the DPoP private key — can use it successfully. The "valid signature and claims" argument is rejected: a DPoP-bound token is valid for bearer use only if the cnf/jkt constraint is enforced; ignoring cnf/jkt is the same as removing the constraint. The resource server's ignoring of the `cnf/jkt` claim transforms a sender-constrained token into an unconstrained Bearer token, undermining the security property the token was issued with.

**Reason:** RFC 9449 §7.2 MUST: RS must validate DPoP proof when access token contains cnf/jkt claim. Ignoring cnf/jkt converts sender-constrained token to unconstrained Bearer — stolen token usable without DPoP key. MUST violation = REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 9449 §7.2: RS MUST check for and validate the DPoP proof when the access token carries a cnf/jkt claim. The RS treating a DPoP-bound token as a plain Bearer token eliminates the sender-constraint property: any attacker who intercepts the access token can present it to the RS without the DPoP private key and receive a successful response. The RS's standard Bearer validation path is correct for unconstrained tokens but is insufficient when cnf/jkt is present. The cnf/jkt claim is a binding mandate on the RS, not optional metadata. REPROCESS.

---

## Pair[CB4-3.4-936-B]

**Territory:** T-rfc-oauth2 · T-rfc-jwt
**Domain:** RS correctly validates DPoP proof for DPoP-bound access tokens
**Expected verdict:** PASS

**A:**
The resource server's token validation pipeline detects the presence of `cnf/jkt` in the access token claims. When cnf/jkt is present, the RS requires a `DPoP` header in the HTTP request. It validates the DPoP proof per RFC 9449 §4.3: (1) verify DPoP JWT signature using the public key in the `jwk` header; (2) confirm the public key's JWK Thumbprint matches the `cnf/jkt` value in the access token; (3) verify `htm` matches the HTTP method of the current request; (4) verify `htu` matches the request URI; (5) verify `ath` equals `BASE64URL(SHA256(ASCII(access_token)))`; (6) verify `iat` is within an acceptable clock window (±60 seconds); (7) verify `jti` has not been seen before (DPoP nonce store with TTL matching the access token lifetime).

**B:**
PASS. RFC 9449 §7.2 MUST satisfied: RS checks for DPoP proof when cnf/jkt is present in the access token. The validation chain covers all RFC 9449 §4.3 requirements: DPoP key match via cnf/jkt (sender binding), htm/htu (request binding), ath (access token binding), iat (freshness), jti (replay prevention). The `ath` claim cryptographically binds this specific DPoP proof to this specific access token, preventing a stolen access token from being used with a different DPoP key pair. Replay prevention via jti ensures each DPoP proof is single-use. The full sender-constraint property is operational.

**Reason:** RFC 9449 §7.2 MUST satisfied. Full DPoP proof validation chain (key match, method/URI binding, access token hash, freshness, jti replay check) implemented correctly. Sender-constraint property fully enforced. PASS.

**Phrasing-B:**
PASS. RS detects cnf/jkt in access token and requires DPoP proof per RFC 9449 §7.2. Validation chain: DPoP public key JWK thumbprint matches cnf/jkt; htm/htu match current request; ath = BASE64URL(SHA256(access_token)); iat within clock window; jti not previously seen. Sender-constraint property fully enforced — stolen access token unusable without the corresponding DPoP private key. PASS.
