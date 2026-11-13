# CB4 Stage 3 — Node Pipeline Chain Set ext30
# Sub-phase 3.4 · Security and Authorization Model
# Sequences CB4-3.4-941 through CB4-3.4-948 (8 base pairs × 2 phrasings = 16 instances)
# Full 9-node pipeline traces for all 8 ext36 arbiter patterns
# Running total after this file: 120 + (948 − 60) × 2 = 1896 instances (948 base pairs)
# Domains: PAR enforcement bypass, offline_access scope, JWT typ claim, DPoP-bound token as Bearer

---

## Pair[CB4-3.4-941-A]

**Territory:** T-rfc-oauth2 · T-oauth-oidc
**Domain:** PAR `require_pushed_authorization_requests` advertised true; authorization endpoint accepts non-PAR requests
**Pipeline verdict:** REPROCESS

**A:**
[Node 1 — Domain Splitter] Input classified: OAuth 2.0 authorization server configuration + authorization endpoint enforcement behaviour. Security sub-domain: Pushed Authorization Request (PAR) enforcement, RFC 9126. Input routed to PAR enforcement branch.

[Node 2 — Domain Translator] Standard terminology: `require_pushed_authorization_requests` is a server-side enforcement metadata field (RFC 9126 §4). When set to `true`, the authorization server is required to reject any authorization request that does not carry a valid `request_uri` parameter obtained from the PAR endpoint. The input describes a mismatch between the advertised metadata value (`true`) and the actual enforcement behaviour (fallback to direct authorization code processing when `request_uri` is absent).

[Node 3 — Scheme Planner] Validation plan: (1) Identify the normative requirement governing the `require_pushed_authorization_requests` metadata value. (2) Assess whether a fallback branch that bypasses PAR is consistent with that requirement. (3) Evaluate the "legacy migration" justification. (4) Determine verdict severity.

[Node 4 — Domain Processor] RFC 9126 §4: "If the server is configured to require pushed authorization requests, the server MUST reject the authorization request if it does not contain a valid `request_uri` parameter." The operative word is MUST. The `require_pushed_authorization_requests` field is not a preference signal — it is a commitment that the server enforces PAR-required behaviour. The metadata declaration is consumed by clients and software libraries that make security decisions based on it. A server that advertises `true` but silently accepts direct requests makes a false security guarantee.

[Node 5 — Applier] Application to input: The server advertises `require_pushed_authorization_requests=true`. The authorization endpoint processes direct authorization requests when `request_uri` is absent. This is a direct contradiction of RFC 9126 §4 MUST. The "legacy migration" argument is evaluated: RFC 9126 provides no migration exception to MUST. A compliant migration path requires setting the metadata flag to `false` during the transition period, not maintaining a production bypass while advertising enforcement. PAR security properties defeated by bypass: parameter injection protection, parameter confidentiality, request integrity — all PAR-targetted properties are nullified by the open fallback path.

[Node 6 — Result Translator] Finding: RFC 9126 §4 MUST violated. Enforcement inconsistent with advertised metadata. Legacy migration argument rejected — not an exception clause in RFC 9126 §4. PAR security properties fully nullified by bypass path. Verdict severity: MUST violation = REPROCESS.

[Node 7 — Arbiter Selector] IETF-RFCS arbiter selected. Primary citation: RFC 9126 §4. Normative keyword: MUST. Verdict: REPROCESS.

[Node 8 — Arbiter] REPROCESS. RFC 9126 §4 MUST requires rejection of all non-PAR authorization requests when `require_pushed_authorization_requests=true` is advertised. The fallback branch violates this unconditionally. "Legacy migration" is not a valid exception; migration must be handled by toggling the metadata flag, not by maintaining a bypass. All PAR security properties — parameter injection protection, parameter confidentiality, request integrity — are defeated by the open fallback path.

[Node 9 — Formatter] Verdict: **REPROCESS** | Standard: RFC 9126 §4 | Normative level: MUST | Gap: PAR enforcement absent despite advertised require_pushed_authorization_requests=true | Remediation: Remove fallback branch; authorization endpoint must return error=invalid_request for all non-request_uri authorization requests; set metadata flag to false during any migration window.

**B:**
REPROCESS. RFC 9126 §4 MUST: when `require_pushed_authorization_requests=true` is advertised, the authorization endpoint MUST reject authorization requests lacking a valid `request_uri`. The fallback path that processes direct requests violates this unconditionally — metadata declarations are binding commitments, not preferences. The "legacy migration" argument is rejected: the RFC provides no migration exception; compliance requires setting the flag to `false` during transition, not maintaining a bypass while advertising enforcement. All PAR security properties are nullified by the open fallback. Remediation: remove fallback; return 400 error=invalid_request for all non-PAR requests.

**Reason:** RFC 9126 §4 MUST violated. Metadata-advertised security posture inconsistent with actual enforcement. No migration exception exists in the RFC. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 9126 §4 MUST is categorical: require_pushed_authorization_requests=true binds the server to reject non-PAR authorization requests; no fallback is permitted. The advertised `true` value creates a security commitment that clients rely upon; a silent bypass defeats that commitment and leaves all PAR-targeted attack vectors (parameter injection, parameter disclosure) open. Migration must be managed by changing the metadata, not by maintaining a production bypass. REPROCESS — remove fallback path.

---

## Pair[CB4-3.4-942-A]

**Territory:** T-rfc-oauth2 · T-oauth-oidc
**Domain:** PAR enforcement correct — authorization endpoint rejects non-PAR requests
**Pipeline verdict:** PASS

**A:**
[Node 1 — Domain Splitter] Input classified: OAuth 2.0 authorization server with PAR-required configuration. Security sub-domain: PAR enforcement and request_uri validation, RFC 9126. Input routed to PAR enforcement branch.

[Node 2 — Domain Translator] Standard terminology: `require_pushed_authorization_requests=true` advertised in server metadata. Authorization endpoint is configured to require a valid `request_uri` parameter on every authorization request. PAR endpoint enforces client authentication, client_id binding, 60-second expiry, and one-time-use. Legacy migration isolated to a separate instance.

[Node 3 — Scheme Planner] Validation plan: (1) Verify authorization endpoint rejects non-request_uri requests. (2) Verify PAR endpoint meets RFC 9126 §2.2 requirements. (3) Verify migration strategy does not create production bypass. (4) Render verdict.

[Node 4 — Domain Processor] RFC 9126 §4: authorization endpoint MUST reject non-PAR requests when require_pushed_authorization_requests=true. RFC 9126 §2.2: PAR endpoint requires authentication of the client, binds request_uri to client_id, sets short-lived expiry, enforces one-time-use. Input satisfies each requirement: 400 error=invalid_request for absent request_uri; client_secret_basic authentication; 60-second TTL; one-time-use; migration on separate instance with flag=false.

[Node 5 — Applier] All PAR requirements satisfied. No MUST violations or SHOULD gaps identified. Authorization endpoint enforcement consistent with advertised metadata. PAR endpoint meets RFC 9126 §2.2. Migration strategy correctly separates production enforcement from transitional clients.

[Node 6 — Result Translator] No violations found. All checked requirements satisfied. Verdict: PASS.

[Node 7 — Arbiter Selector] IETF-RFCS arbiter selected. Primary citations: RFC 9126 §4, §2.2. No violations.

[Node 8 — Arbiter] PASS. RFC 9126 §4 and §2.2 satisfied. Authorization endpoint enforces PAR-required status unconditionally; PAR endpoint implements full security requirements; migration strategy does not compromise production enforcement.

[Node 9 — Formatter] Verdict: **PASS** | Standards: RFC 9126 §4, §2.2 | All PAR enforcement requirements met | No violations found.

**B:**
PASS. RFC 9126 §4 satisfied: authorization endpoint rejects non-request_uri requests unconditionally (400 error=invalid_request). RFC 9126 §2.2 satisfied: PAR endpoint requires client authentication, binds request_uri to client_id, enforces 60-second expiry and one-time-use. Legacy migration isolated to separate instance — no production bypass. All PAR security properties operational.

**Reason:** RFC 9126 §4 and §2.2 fully satisfied. No MUST violations. Correct PAR enforcement pattern with compliant migration approach. PASS.

**Phrasing-B:**
PASS. Authorization endpoint correctly enforces require_pushed_authorization_requests=true by rejecting all non-PAR requests with error=invalid_request. PAR endpoint satisfies RFC 9126 §2.2: client authentication, request_uri client-binding, short expiry, single-use. Production enforcement is not compromised by legacy migration (separate instance, separate metadata). PASS.

---

## Pair[CB4-3.4-943-A]

**Territory:** T-rfc-oidc · T-oauth-oidc
**Domain:** AS issues refresh token without `offline_access` scope in authorization request
**Pipeline verdict:** REPROCESS

**A:**
[Node 1 — Domain Splitter] Input classified: OIDC authorization server token issuance policy. Security sub-domain: refresh token issuance conditions, OIDC Core §11. Input routed to refresh token scope enforcement branch.

[Node 2 — Domain Translator] Standard terminology: `offline_access` is an OIDC-defined scope value that specifically authorizes issuance of a refresh token (RFC 6749 §1.5 extension; OIDC Core §11 normalization). When `offline_access` is absent from the authorization request, the OIDC Core specification prohibits issuance of a refresh token.

[Node 3 — Scheme Planner] Validation plan: (1) Identify OIDC Core §11 constraint on refresh token issuance. (2) Assess the "UX default" justification. (3) Evaluate consent implications. (4) Determine verdict severity.

[Node 4 — Domain Processor] OIDC Core §11: "A Refresh Token MUST NOT be returned unless the `offline_access` Scope was granted in the Authorization Request and the OP supports providing refresh tokens." This is a MUST NOT — the strongest normative prohibition. The rationale is consent-based: a refresh token grants the application the ability to access the user's resources during periods when the user is not actively interacting with the application; this is a distinct capability from online session-based access, and users must consent to it explicitly via the `offline_access` scope request.

[Node 5 — Applier] Application to input: The AS issues refresh tokens on all authorization code flows regardless of scope. Representative request carries `scope=openid profile email` — no `offline_access`. The AS violates OIDC Core §11 MUST NOT. The "UX default" reasoning is evaluated: user experience considerations do not provide an exception to a MUST NOT clause. The user consented to an online session; a refresh token extends access beyond that session without explicit consent.

[Node 6 — Result Translator] Finding: OIDC Core §11 MUST NOT violated. Refresh tokens issued beyond the scope of user consent. "UX default" argument rejected. Verdict severity: MUST NOT violation = REPROCESS.

[Node 7 — Arbiter Selector] IETF-RFCS arbiter selected (OIDC Core is an IETF-registered protocol). Primary citation: OIDC Core §11. Normative keyword: MUST NOT. Verdict: REPROCESS.

[Node 8 — Arbiter] REPROCESS. OIDC Core §11 MUST NOT: refresh tokens must not be issued unless `offline_access` was in the authorization request scope. Issuing refresh tokens by default violates user consent scope — the user approved an online session, not ongoing offline access. "UX default" is rejected; no exception clause exists in OIDC Core §11.

[Node 9 — Formatter] Verdict: **REPROCESS** | Standard: OIDC Core §11 | Normative level: MUST NOT | Gap: Refresh tokens issued without offline_access scope consent | Remediation: Gate refresh token issuance on offline_access scope presence; add consent screen that names offline access capability when offline_access is requested.

**B:**
REPROCESS. OIDC Core §11 MUST NOT: a refresh token must not be returned unless `offline_access` was included in the authorization request scope. The AS issuing refresh tokens as a default violates both this MUST NOT and the user's consent boundary — the user consented to an online session, not to long-term offline access. No UX justification overrides a MUST NOT constraint. Remediation: gate refresh token issuance on offline_access scope; add consent screen for offline access capability.

**Reason:** OIDC Core §11 MUST NOT violated. Refresh token issued beyond user's consented scope. REPROCESS.

**Phrasing-B:**
REPROCESS. OIDC Core §11 explicitly prohibits issuing refresh tokens unless offline_access was in the authorization request scope. The AS's default-refresh-token policy violates this MUST NOT unconditionally — every flow without offline_access scope that receives a refresh token is a violation. The user's explicit consent to offline access (distinct from session-based access) is a deliberate design feature of OIDC §11 that cannot be bypassed by operational convenience preferences. REPROCESS.

---

## Pair[CB4-3.4-944-A]

**Territory:** T-rfc-oidc · T-oauth-oidc
**Domain:** Refresh token only issued when `offline_access` scope explicitly granted
**Pipeline verdict:** PASS

**A:**
[Node 1 — Domain Splitter] Input classified: OIDC authorization server refresh token gating logic. Security sub-domain: offline_access scope enforcement, OIDC Core §11.

[Node 2 — Domain Translator] Standard terminology: AS evaluates scope parameter before issuing refresh token; `offline_access` presence is a prerequisite. Consent screen at authorization endpoint explicitly names the offline access capability.

[Node 3 — Scheme Planner] Validation plan: (1) Verify refresh token conditional on offline_access scope. (2) Verify user consent mechanism for offline access. (3) Render verdict.

[Node 4 — Domain Processor] OIDC Core §11: MUST NOT issue refresh token unless offline_access in scope + OP supports it. Input: `if (!requestedScopes.Contains("offline_access")) { return response without refresh_token; }`. Consent screen explicitly names offline access. offline_access listed in scopes_supported.

[Node 5 — Applier] All conditions satisfied. Refresh token issuance conditional on offline_access. User consent obtained. No MUST NOT violation.

[Node 6 — Result Translator] No violations. PASS.

[Node 7 — Arbiter Selector] IETF-RFCS arbiter selected. OIDC Core §11. No violations.

[Node 8 — Arbiter] PASS. OIDC Core §11 MUST NOT satisfied: refresh token only issued when offline_access explicitly in scope + user consent obtained at authorization endpoint.

[Node 9 — Formatter] Verdict: **PASS** | Standard: OIDC Core §11 | Refresh token issuance correctly gated on offline_access scope + consent.

**B:**
PASS. OIDC Core §11 MUST NOT satisfied: refresh token issuance is gated on explicit `offline_access` scope presence in the authorization request. Consent screen names the offline access capability, ensuring user awareness. Token responses correctly omit refresh_token when offline_access is absent. No violations found.

**Reason:** OIDC Core §11 MUST NOT satisfied. Correct consent-based model for refresh token issuance implemented. PASS.

**Phrasing-B:**
PASS. AS correctly gates refresh token issuance on offline_access scope per OIDC Core §11. User consent screen explicitly identifies the offline access capability. Responses without offline_access scope do not contain refresh_token. PASS.

---

## Pair[CB4-3.4-945-A]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** JWT access token `typ` header absent — token confusion attack vector
**Pipeline verdict:** REPROCESS

**A:**
[Node 1 — Domain Splitter] Input classified: JWT access token structure + resource server validation logic. Security sub-domain: JWT token type header, RFC 9068 §2.1. Input routed to JWT access token profile validation branch.

[Node 2 — Domain Translator] Standard terminology: JWT JOSE header `typ` claim is a type discriminator for JWT tokens. RFC 9068 §2.1 defines the `at+JWT` value specifically for JWT access tokens. The resource server validates signature, iss, aud, exp, scope — but not typ. Input describes JOSE headers lacking the typ field.

[Node 3 — Scheme Planner] Validation plan: (1) Identify RFC 9068 §2.1 requirement for typ. (2) Assess the token confusion attack vector. (3) Evaluate "structurally valid JWT" argument. (4) Determine verdict severity.

[Node 4 — Domain Processor] RFC 9068 §2.1: "The JWT MUST include a `typ` claim set to the value `at+JWT`." This is an unconditional MUST. The purpose: `typ: at+JWT` discriminates access tokens from ID tokens, JWT client assertions, and other JWT types that may be issued by the same AS with the same signing key. Without typ: an ID token issued with `{"alg":"RS256","kid":"key-2024-01"}` would pass the RS's signature, iss, and exp checks — the RS has no mechanism to reject it based on token type alone. The aud claim in an ID token is the client_id (not the RS), but the RS is currently not checking aud correctly either in this input (aud validation not mentioned as absent, but typ is the primary gap here).

[Node 5 — Applier] Application to input: JOSE headers contain only alg and kid. typ is absent. RS performs no typ check. This violates RFC 9068 §2.1 MUST. "Structurally valid JWT" argument evaluated: RFC 9068 is a profile-specific MUST that adds requirements beyond the base JWT specification; "structurally valid JWT" satisfies RFC 7519 but not RFC 9068 §2.1. Token confusion attack is enabled: same issuer, same key, different token type → RS cannot distinguish.

[Node 6 — Result Translator] Finding: RFC 9068 §2.1 MUST violated. typ: at+JWT absent from access token header. Token confusion attack vector present. "Structurally valid" argument rejected. Verdict: REPROCESS.

[Node 7 — Arbiter Selector] IETF-RFCS arbiter selected. Primary citation: RFC 9068 §2.1. Normative keyword: MUST. Verdict: REPROCESS.

[Node 8 — Arbiter] REPROCESS. RFC 9068 §2.1 MUST: JWT access tokens must carry `typ: at+JWT`. Absent typ field enables token confusion: an ID token from the same issuer would pass the RS's validation checks without a typ-based rejection path. The RFC 9068 typ requirement exists precisely to prevent this cross-token confusion vector.

[Node 9 — Formatter] Verdict: **REPROCESS** | Standard: RFC 9068 §2.1 | Normative level: MUST | Gap: typ: at+JWT absent from JWT access token header | Remediation: Set typ=at+JWT in JOSE header at AS; add typ allowlist check at RS as first validation step before signature verification.

**B:**
REPROCESS. RFC 9068 §2.1 MUST: JWT access tokens must carry `typ: at+JWT` in the JOSE header. The absent typ field leaves the resource server unable to programmatically distinguish access tokens from ID tokens or other JWT types issued by the same AS. Token confusion attack: ID token (same issuer, same signing key) would pass the RS's signature/iss/exp checks without a typ rejection path. "Structurally valid JWT" does not satisfy RFC 9068 profile-specific MUST. Remediation: AS sets `typ: at+JWT`; RS validates typ before signature check.

**Reason:** RFC 9068 §2.1 MUST violated. Absent typ enables token confusion. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 9068 §2.1 MUST requires `typ: at+JWT` in every JWT access token JOSE header. This discriminator prevents token confusion attacks — without it, an ID token bearing the same issuer, algorithm, and key identifier would pass the resource server's validation pipeline. The "metadata only" characterisation of typ is incorrect; it is a functional security control required by a profile-specific MUST. REPROCESS — set typ at AS; check typ at RS.

---

## Pair[CB4-3.4-946-A]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** JWT access token `typ: at+JWT` present; RS validates before signature check
**Pipeline verdict:** PASS

**A:**
[Node 1 — Domain Splitter] Input classified: JWT access token structure + RS validation order. Security sub-domain: RFC 9068 §2.1 typ claim enforcement.

[Node 2 — Domain Translator] Standard terminology: AS issues JWT access tokens with `{"typ":"at+JWT","alg":"RS256","kid":"key-2024-01"}`. RS validates typ == "at+JWT" as the first step, rejecting tokens with wrong or absent typ before any key lookup or signature verification.

[Node 3 — Scheme Planner] Validation plan: (1) Verify typ: at+JWT present at AS. (2) Verify RS typ check is correctly positioned. (3) Render verdict.

[Node 4 — Domain Processor] RFC 9068 §2.1 MUST: typ=at+JWT present. RS validation order: (1) parse header; (2) assert typ == at+JWT; (3) key lookup; (4) signature verification; (5) claim validation. Step 2 rejects ID tokens and other JWT types before incurring key lookup cost. Correct — typ check before signature prevents confusion attack; also prevents needless JWKS cache misses for wrong-type tokens.

[Node 5 — Applier] All requirements satisfied. typ present. RS validation ordered correctly. No violations found.

[Node 6 — Result Translator] No violations. PASS.

[Node 7 — Arbiter Selector] IETF-RFCS arbiter. RFC 9068 §2.1. No violations.

[Node 8 — Arbiter] PASS. RFC 9068 §2.1 MUST satisfied: typ=at+JWT in JOSE header. RS validates typ before signature check — correct security-first validation order. Token confusion attack vector eliminated.

[Node 9 — Formatter] Verdict: **PASS** | Standard: RFC 9068 §2.1 | typ: at+JWT present; RS validates typ first | No violations found.

**B:**
PASS. RFC 9068 §2.1 MUST satisfied: JWT access tokens carry `typ: at+JWT` in the JOSE header. Resource server validates typ as the first check before key lookup and signature verification, correctly rejecting ID tokens and other JWT types. Full validation chain (typ → kid → sig → iss/aud/exp → scope) is correct. Token confusion attack vector eliminated. PASS.

**Reason:** RFC 9068 §2.1 MUST satisfied. Correct typ enforcement order at RS. No violations. PASS.

**Phrasing-B:**
PASS. AS issues access tokens with `typ: at+JWT` per RFC 9068 §2.1 MUST. RS validation pipeline checks typ first (step 1), rejecting non-access-token JWTs before wasting resources on key lookup. Token confusion eliminated. Full claim validation also correct. PASS.

---

## Pair[CB4-3.4-947-A]

**Territory:** T-rfc-oauth2 · T-rfc-jwt
**Domain:** DPoP-bound access token (`cnf/jkt`) accepted at RS without DPoP proof
**Pipeline verdict:** REPROCESS

**A:**
[Node 1 — Domain Splitter] Input classified: Resource server access token validation for DPoP-bound tokens. Security sub-domain: DPoP proof-of-possession enforcement at RS, RFC 9449 §7.2. Input routed to DPoP enforcement branch.

[Node 2 — Domain Translator] Standard terminology: DPoP (Demonstration of Proof-of-Possession) binds an access token to a specific key pair; the `cnf/jkt` claim in the access token carries the JWK Thumbprint of the client's DPoP public key. RFC 9449 §7.2 requires the RS to validate the DPoP proof header when cnf/jkt is present. The input describes an RS that validates signature, iss, aud, exp, scope but ignores the cnf/jkt claim and does not check for a DPoP proof.

[Node 3 — Scheme Planner] Validation plan: (1) Identify RFC 9449 §7.2 MUST requirement. (2) Assess the practical impact of ignoring cnf/jkt. (3) Evaluate "valid signature and claims" argument. (4) Determine verdict severity.

[Node 4 — Domain Processor] RFC 9449 §7.2: "If the resource server determines that the access token was issued with a DPoP-bound key (i.e., the access token contains a `cnf/jkt` claim), the resource server MUST check for the existence of the DPoP HTTP request header field and MUST process it according to the rules in Section 4.3." Two MUST requirements. The cnf/jkt claim is the RS's signal to enforce DPoP proof validation. Without enforcement: a stolen access token (obtained via network intercept, RS log, or server compromise) can be used by any attacker directly as a Bearer token, without possessing the DPoP private key.

[Node 5 — Applier] Application to input: cnf/jkt is present in tokens. RS ignores it. No DPoP proof is required. This directly violates RFC 9449 §7.2's two MUST requirements (check for DPoP header + process per §4.3). "Valid signature and claims" argument evaluated: the sender-constraint property is independent of and in addition to signature/claim validation; ignoring cnf/jkt effectively removes the constraint the AS embedded in the token. The RS is converting a sender-constrained token into an unconstrained Bearer token by policy.

[Node 6 — Result Translator] Finding: RFC 9449 §7.2 MUST violated (two counts). Sender-constraint property nullified. Stolen token usable without DPoP key. "Valid claims" argument rejected. Verdict: REPROCESS.

[Node 7 — Arbiter Selector] IETF-RFCS arbiter selected. Primary citation: RFC 9449 §7.2. Normative keyword: MUST (two instances). Verdict: REPROCESS.

[Node 8 — Arbiter] REPROCESS. RFC 9449 §7.2 MUST (twice): when cnf/jkt is present, RS MUST check for DPoP header AND process per §4.3. Ignoring cnf/jkt converts sender-constrained tokens to unconstrained Bearer tokens — stolen access tokens become freely usable without the DPoP private key. The "valid signature" argument is rejected: cnf/jkt is a binding security mandate in the token, not optional metadata.

[Node 9 — Formatter] Verdict: **REPROCESS** | Standard: RFC 9449 §7.2 | Normative level: MUST (×2) | Gap: DPoP proof not validated despite cnf/jkt present in access token | Remediation: RS must detect cnf/jkt claim and require DPoP proof per RFC 9449 §4.3 (key match via cnf/jkt, htm, htu, ath, iat, jti).

**B:**
REPROCESS. RFC 9449 §7.2 MUST (twice): RS must (1) check for DPoP header and (2) validate DPoP proof per §4.3 when access token contains cnf/jkt claim. The RS ignoring cnf/jkt strips the sender-constraint property and allows stolen access tokens to be used as plain Bearer tokens without the DPoP private key. "Signature and claims validation is sufficient" is rejected — cnf/jkt is a binding mandate that the RS must enforce, not optional metadata.

**Reason:** RFC 9449 §7.2 MUST violated (two counts). Sender-constraint property nullified by RS ignoring cnf/jkt. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 9449 §7.2 imposes two MUST requirements on the RS when cnf/jkt is present: check for DPoP header + validate per §4.3. The RS treating DPoP-bound tokens as plain Bearer tokens defeats the entire sender-constraint model — the token was issued with the explicit expectation that only the holder of the DPoP private key can use it; the RS's ignoring of cnf/jkt grants that access to any token bearer regardless of key possession. REPROCESS.

---

## Pair[CB4-3.4-948-A]

**Territory:** T-rfc-oauth2 · T-rfc-jwt
**Domain:** RS validates DPoP proof for DPoP-bound access tokens — all §4.3 checks satisfied
**Pipeline verdict:** PASS

**A:**
[Node 1 — Domain Splitter] Input classified: Resource server DPoP proof validation implementation. Security sub-domain: RFC 9449 §7.2 + §4.3 DPoP enforcement at RS.

[Node 2 — Domain Translator] Standard terminology: RS detects cnf/jkt in access token. Validates DPoP proof covering: public key JWK thumbprint match against cnf/jkt; htm (HTTP method); htu (request URI); ath = BASE64URL(SHA256(access_token)); iat within clock window; jti not previously seen (nonce store with matching TTL).

[Node 3 — Scheme Planner] Validation plan: (1) Verify RS detects cnf/jkt and requires DPoP proof. (2) Verify all §4.3 validation steps present. (3) Render verdict.

[Node 4 — Domain Processor] RFC 9449 §7.2: RS MUST check DPoP header when cnf/jkt present. RFC 9449 §4.3: DPoP proof must be validated for: key match (public key in jwk header thumbprint = cnf/jkt); htm; htu; ath; iat freshness; jti uniqueness. Input covers all six checks. cnf/jkt detection is the trigger. ath claim binds the DPoP proof cryptographically to the specific access token. jti tracking prevents replay within the access token's validity window.

[Node 5 — Applier] All RFC 9449 §7.2 and §4.3 requirements satisfied. Sender-constraint property fully operational. No MUST violations. No SHOULD gaps identified.

[Node 6 — Result Translator] No violations. PASS.

[Node 7 — Arbiter Selector] IETF-RFCS arbiter. RFC 9449 §7.2, §4.3. No violations.

[Node 8 — Arbiter] PASS. RFC 9449 §7.2 MUST satisfied: RS detects cnf/jkt and requires DPoP proof. §4.3 validation chain complete: key match, htm, htu, ath, iat, jti replay prevention. Sender-constraint property fully enforced.

[Node 9 — Formatter] Verdict: **PASS** | Standards: RFC 9449 §7.2, §4.3 | All DPoP enforcement requirements met | Sender-constraint property operational | No violations found.

**B:**
PASS. RFC 9449 §7.2 MUST satisfied: RS detects cnf/jkt in access token and requires DPoP proof. §4.3 validation chain complete: public key JWK thumbprint matches cnf/jkt (sender binding); htm and htu match current request (request binding); ath = BASE64URL(SHA256(access_token)) (access token binding); iat within clock window (freshness); jti not previously seen (replay prevention). Sender-constraint property fully operational — stolen access token unusable without DPoP private key. PASS.

**Reason:** RFC 9449 §7.2 and §4.3 fully satisfied. All DPoP proof validation checks implemented. Sender-constraint property enforced. PASS.

**Phrasing-B:**
PASS. RS correctly enforces DPoP per RFC 9449 §7.2: cnf/jkt detected → DPoP header required → full §4.3 validation (key-match/htm/htu/ath/iat/jti). The ath claim cryptographically binds each DPoP proof to the specific access token instance, preventing proof reuse with different tokens. jti tracking prevents DPoP proof replay within the token lifetime. Sender-constraint fully operational. PASS.
