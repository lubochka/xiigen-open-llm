# CB4 Stage 3 — Node Cross-Source Set ext34
# Sub-phase 3.4 · Security and Authorization Model
# Sequences CB4-3.4-1079 through CB4-3.4-1088 (10 pairs × 2 phrasings = 20 instances)
# Covers: cross-source combinations of ext41 patterns with each other and with prior sub-phase patterns
# Running total after this file: 120 + (1088 − 60) × 2 = 2176 instances (1088 base pairs)
# Graduation rule: N CONCERNs do NOT escalate to REPROCESS (demonstrated: 1084)
# PO.1 escalation: PO.1 absence + traceable production MUST violation = REPROCESS (demonstrated: 1082)
# REPROCESS overrides CONCERN when both apply to same scenario (demonstrated: 1085)

---

## Pair[CB4-3.4-1079]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** DPoP proof htu/htm not validated (REPROCESS) AND DPoP proof jti not tracked (REPROCESS) — dual DPoP MUST violations
**Expected verdict:** REPROCESS

**A:**
A resource server validates DPoP proofs for signature, jkt-to-token binding, and iat freshness. However: (1) the RS does not validate `htu` (HTTP Target URI) or `htm` (HTTP Method) — a DPoP proof for `POST /payments` is accepted at `DELETE /accounts/123`; (2) the RS does not maintain a server-side jti replay cache — the same DPoP proof can be submitted multiple times within the 60-second freshness window. Both failures are confirmed in a security review.

**B:**
REPROCESS. Two independent RFC 9449 §7.2 MUST violations: (1) RFC 9449 §7.2 MUST: RS must verify htu and htm match the current request — absent validation allows captured DPoP proofs to be replayed at different endpoints and with different methods; (2) RFC 9449 §7.2 MUST: RS must check jti uniqueness within a reasonable time window — absent tracking allows the same proof to be replayed N times within the freshness window. Both are unconditional MUSTs from the same RFC section. Combined verdict: REPROCESS. The two violations compound each other: without htu/htm, a captured proof can be replayed at any endpoint; without jti tracking, it can be replayed multiple times. Both require independent remediation.

**Reason:** RFC 9449 §7.2 MUST (htu/htm validation) + RFC 9449 §7.2 MUST (jti uniqueness). Two DPoP binding MUST violations; combined verdict REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 9449 §7.2 MUST: htu/htm not validated → endpoint-and-method binding nullified. RFC 9449 §7.2 MUST: jti not tracked → proof replay within freshness window. Two independent MUST violations; combined REPROCESS. Remediate: add htu/htm comparison; add Redis jti cache with TTL=freshness window.

---

## Pair[CB4-3.4-1080]

**Territory:** T-oauth-oidc · T-rfc-jwt
**Domain:** OIDC hybrid flow — id_token missing both `c_hash` and `at_hash` when code and access token are returned
**Expected verdict:** REPROCESS

**A:**
An OIDC Authorization Server implements `response_type=code token id_token` (hybrid flow returning code, access token, and id_token from the authorization endpoint). The id_token contains standard claims (`iss`, `sub`, `aud`, `exp`, `iat`, `nonce`) but neither `c_hash` (code hash) nor `at_hash` (access token hash). The rationale: "c_hash and at_hash are optional claims. Server-side session binding covers the integrity requirement."

**B:**
REPROCESS. Two independent OIDC Core §3.3.2.11 MUST violations: (1) c_hash MUST: when authorization code is returned from the authorization endpoint in hybrid flow, c_hash must be present — absent c_hash means the RP cannot verify that the returned code matches what the AS intended; (2) at_hash MUST: when access token is returned from the authorization endpoint in hybrid flow, at_hash must be present — absent at_hash means the RP cannot verify access token integrity. Both are MUST claims whose conditions are met (code and AT both returned at auth endpoint). "Optional" correctly describes flows where code/AT are not returned at the auth endpoint; when they are returned, MUST applies. Server-side session binding is not RP-verifiable. Combined verdict: REPROCESS.

**Reason:** OIDC Core §3.3.2.11 MUST: c_hash required (code returned) + at_hash required (AT returned). Both absent. Two MUST violations; combined verdict REPROCESS.

**Phrasing-B:**
REPROCESS. OIDC Core §3.3.2.11 MUST: c_hash MUST when code returned at auth endpoint — absent. OIDC Core §3.3.2.11 MUST: at_hash MUST when AT returned at auth endpoint — absent. Both hash claims required for response_type=code token id_token. "Optional claims" defense rejected — conditional MUST applies when code/AT returned. Combined REPROCESS.

---

## Pair[CB4-3.4-1081]

**Territory:** T-rfc-oauth2 · T-asvs-token
**Domain:** Token endpoint TLS 1.0/1.1 accepted (REPROCESS) AND token response Cache-Control header absent (REPROCESS) — dual transport security MUST violations
**Expected verdict:** REPROCESS

**A:**
An Authorization Server's token endpoint has two confirmed security failures: (1) TLS 1.0 and TLS 1.1 are accepted alongside TLS 1.2/1.3 — deprecated protocol versions are not disabled; (2) Token responses do not include `Cache-Control: no-store` or `Pragma: no-cache` headers — access tokens and refresh tokens returned in token responses may be cached by intermediate proxies or client-side caches. Both failures affect the same credential-issuing endpoint.

**B:**
REPROCESS. Two independent MUST violations affecting token endpoint transport and response security: (1) RFC 9325 §4 MUST NOT: implementations must not negotiate TLS 1.0 and TLS 1.1 — both deprecated versions accepted; TLS downgrade attacks can expose credential transmission; (2) RFC 6749 §5.1 MUST: "The authorization server MUST include the HTTP Cache-Control response header field in any response containing tokens, credentials, or other sensitive information with the 'no-store' option." Token responses without no-store may be cached by proxies or browsers, persisting credentials beyond the intended session. Both are unconditional MUSTs. Combined verdict: REPROCESS. The combination is particularly concerning: TLS downgrade exposes tokens in transit; cache without no-store persists them at rest.

**Reason:** RFC 9325 §4 MUST NOT (TLS 1.0/1.1 accepted) + RFC 6749 §5.1 MUST (Cache-Control: no-store absent). Both MUSTs violated at token endpoint; combined REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 9325 §4 MUST NOT: TLS 1.0/1.1 accepted → credential transmission downgrade risk. RFC 6749 §5.1 MUST: Cache-Control: no-store absent from token response → tokens cacheable by proxies. Two transport+response MUSTs; combined REPROCESS. Fix: disable TLS 1.0/1.1; add Cache-Control: no-store to all token responses.

---

## Pair[CB4-3.4-1082]

**Territory:** T-rfc-oauth2 · T-asvs-token
**Domain:** PO.1 (no formal security policy) AND scope downgrade without scope in response — PO.1 escalation to REPROCESS
**Expected verdict:** REPROCESS

**A:**
An Authorization Server issues tokens with narrowed scopes when client requests exceed policy — but does not include the `scope` parameter in token responses to notify clients of the downgrade. The AS also has no formal security policy document (PO.1 gap): there is no documented requirement for token response completeness, scope disclosure obligations, or client notification practices. When the issue is identified, the developer team responds: "We weren't aware scope had to be returned in the response when downgraded. There's no policy document requiring it."

**B:**
REPROCESS. PO.1 (absence of formal security policy): no documented requirements for token response scope disclosure — guidance-level gap (CONCERN in isolation). The PO.1 escalation rule applies: PO.1 absence + traceable production MUST violation = REPROCESS. The traceable production MUST violation: RFC 6749 §3.3 MUST: "If the issued access token scope is different from the one requested by the client, the authorization server MUST include the `scope` response parameter." This MUST is being violated in production — scope downgrade occurs without notification. The PO.1 gap explains why the violation was undetected (no policy required it) but does not mitigate it. Combined: PO.1 (CONCERN escalation trigger) + RFC 6749 §3.3 MUST violation (traceable production fault) = REPROCESS.

**Reason:** PO.1 absence (CONCERN) + RFC 6749 §3.3 MUST violation (scope absent on downgrade in production) = REPROCESS per PO.1 escalation rule.

**Phrasing-B:**
REPROCESS. RFC 6749 §3.3 MUST: scope must be in token response when issued scope ≠ requested scope — actively violated. PO.1 gap: no formal policy documenting token response completeness requirements. PO.1 alone = CONCERN; PO.1 + traceable production MUST violation = REPROCESS. Fix: include scope in responses; document token response requirements in security policy.

---

## Pair[CB4-3.4-1083]

**Territory:** T-rfc-oauth2 · T-asvs-token
**Domain:** PAR request_uri client_id binding absent (REPROCESS) AND state parameter omitted from authorization response (REPROCESS) — dual AS authorization endpoint MUST violations
**Expected verdict:** REPROCESS

**A:**
An Authorization Server's authorization endpoint has two security failures: (1) When processing `request_uri` values from PAR (Pushed Authorization Requests), the AS does not verify that the `client_id` in the current authorization request matches the client_id that pushed the request — cross-client substitution is possible; (2) When clients include a `state` parameter in their authorization requests, the AS does not echo it back in the authorization response — CSRF verification at the client is impossible. Both failures are at the authorization endpoint.

**B:**
REPROCESS. Two independent MUST violations at the authorization endpoint: (1) RFC 9126 §5 MUST: AS must check that client_id at authorization endpoint matches client_id of the pushed PAR request — absent check allows cross-client request_uri substitution; (2) RFC 6749 §4.1.2 REQUIRED: if state is present in the authorization request, it MUST be returned in the authorization response — absent echo breaks client CSRF verification. Both are unconditional normative requirements. Combined verdict: REPROCESS. The combination is significant: PAR binding failure allows cross-client request injection; missing state prevents CSRF detection at the client.

**Reason:** RFC 9126 §5 MUST (client_id binding absent) + RFC 6749 §4.1.2 REQUIRED (state not echoed). Two authorization endpoint MUST violations; combined REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 9126 §5 MUST: client_id not verified against PAR-associated client_id → cross-client substitution. RFC 6749 §4.1.2 REQUIRED: state not echoed → CSRF verification impossible. Two authorization endpoint MUST violations; combined REPROCESS. Fix: add client_id binding check for PAR; echo state unconditionally.

---

## Pair[CB4-3.4-1084]

**Territory:** T-rfc-oauth2 · T-asvs-token
**Domain:** `expires_in` absent from token response (CONCERN) AND authorization code lifetime 10 minutes (CONCERN) — graduation rule: N CONCERNs ≠ REPROCESS
**Expected verdict:** CONCERN

**A:**
An Authorization Server has two guidance gaps simultaneously: (1) Token responses do not include `expires_in` — clients must use reactive 401-driven token refresh; the TTL is documented in the developer portal but not programmatically discoverable; (2) Authorization codes have a 10-minute expiry — exceeding the accepted 1–2 minute practice per ASVS V3.5.2 and RFC 6749 §10.5 "short-lived" guidance. Both are acknowledged by the team but are not considered high priority.

**B:**
CONCERN. Two guidance gaps: (1) expires_in absent: RFC 6749 §5.1 RECOMMENDED (not MUST); developer portal documents TTL, partially satisfying the RFC 6749 §5.1 SHOULD for "other means"; no RFC MUST violated; CONCERN; (2) 10-minute authorization codes: RFC 6749 §10.5 short-lived requirement not numerically defined; ASVS V3.5.2 "shortest possible time" guidance not met; no RFC numeric MUST for maximum duration; CONCERN. The graduation rule applies: N CONCERNs do NOT escalate to REPROCESS regardless of count. Two guidance gaps remain CONCERN. Combined verdict: CONCERN. Recommendations: (1) include expires_in in all token responses; (2) reduce authorization code lifetime to ≤2 minutes.

**Reason:** RFC 6749 §5.1 RECOMMENDED gap (expires_in absent) = CONCERN + ASVS V3.5.2 guidance gap (10-min codes) = CONCERN. Graduation rule: 2 CONCERNs ≠ REPROCESS. Combined verdict = CONCERN.

**Phrasing-B:**
CONCERN. expires_in absent: RFC 6749 §5.1 RECOMMENDED not met — CONCERN. 10-minute auth codes: ASVS V3.5.2 "shortest possible time" guidance not met — CONCERN. Graduation rule: 2 CONCERNs = CONCERN, not REPROCESS. No RFC MUST violated in either case. Recommendations: add expires_in; reduce code lifetime to ≤2 minutes.

---

## Pair[CB4-3.4-1085]

**Territory:** T-rfc-jwt · T-rfc-oauth2 · T-asvs-token
**Domain:** DPoP htu/htm not validated (REPROCESS) combined with `expires_in` absent from token response (CONCERN) — REPROCESS overrides CONCERN
**Expected verdict:** REPROCESS

**A:**
A deployment has two findings: (1) The resource server validates DPoP proofs but does not check `htu` or `htm` — DPoP proofs are not endpoint-or-method-bound, enabling cross-endpoint replay attacks (RFC 9449 §7.2 MUST violation); (2) The Authorization Server omits `expires_in` from token responses — clients cannot proactively manage token expiry (RFC 6749 §5.1 RECOMMENDED gap, CONCERN). Both are confirmed in the same security review cycle.

**B:**
REPROCESS. Two findings at different severity levels: (1) RFC 9449 §7.2 MUST: htu and htm must be validated against current request — not done; DPoP proof replay across endpoints viable; this is REPROCESS; (2) RFC 6749 §5.1 RECOMMENDED: expires_in should be present — absent; clients use reactive 401-driven refresh; no RFC MUST violated; this is CONCERN. The graduation rule: REPROCESS overrides CONCERN when both apply to the same scenario. Combined verdict: REPROCESS (driven by RFC 9449 §7.2 MUST). The expires_in gap remains a CONCERN that should be addressed, but it does not change the verdict.

**Reason:** RFC 9449 §7.2 MUST (htu/htm not validated = REPROCESS) + RFC 6749 §5.1 RECOMMENDED gap (expires_in absent = CONCERN). REPROCESS overrides CONCERN. Combined verdict = REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 9449 §7.2 MUST: htu/htm not validated → cross-endpoint DPoP proof replay → REPROCESS. RFC 6749 §5.1 RECOMMENDED: expires_in absent → reactive refresh pattern → CONCERN. REPROCESS overrides CONCERN per graduation rule. Combined: REPROCESS. Remediate DPoP binding (critical); also add expires_in (improvement).

---

## Pair[CB4-3.4-1086]

**Territory:** T-rfc-oauth2 · T-rfc-jwt · T-asvs-token
**Domain:** Scope downgrade without scope in response (REPROCESS) AND RS does not validate exp claim (REPROCESS) — dual MUST violations from different protocol layers
**Expected verdict:** REPROCESS

**A:**
A deployment has two independently confirmed MUST violations at different layers: (1) The Authorization Server issues narrowed-scope tokens without returning the `scope` parameter in the token response — clients are silently issued fewer permissions than requested with no programmatic notification (RFC 6749 §3.3 MUST violation); (2) The Resource Server does not validate the `exp` claim on JWT access tokens — expired tokens are accepted indefinitely as long as signature, iss, aud, and scope checks pass (RFC 9068 §4 MUST violation). Both are at different components (AS and RS) of the same OAuth 2.0 deployment.

**B:**
REPROCESS. Two independent MUST violations at different layers: (1) RFC 6749 §3.3 MUST: scope must be returned in token response when issued scope ≠ requested scope — silent scope downgrade breaks client token management; (2) RFC 9068 §4 MUST: RS must reject JWT when exp < current Unix time — expired tokens accepted indefinitely; stolen tokens never expire at the RS. Both are unconditional MUST requirements. Combined verdict: REPROCESS. Each violation independently drives REPROCESS; combined effect is more severe. Both components must be remediated independently.

**Reason:** RFC 6749 §3.3 MUST (scope absent on downgrade) + RFC 9068 §4 MUST (exp not validated at RS). Two MUST violations at different protocol layers; combined REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 6749 §3.3 MUST: scope not returned on downgrade — client unaware of effective permissions. RFC 9068 §4 MUST: exp not validated at RS — expired tokens valid indefinitely. Independent MUSTs at AS and RS layers; combined REPROCESS. Fix independently: AS returns scope; RS validates exp.

---

## Pair[CB4-3.4-1087]

**Territory:** T-oauth-oidc · T-rfc-jwt · T-asvs-token
**Domain:** OIDC hybrid flow c_hash absent (REPROCESS) AND OIDC prompt=login ignored (REPROCESS) — dual OIDC Core MUST violations
**Expected verdict:** REPROCESS

**A:**
An OIDC deployment has two distinct OIDC Core MUST violations: (1) The hybrid flow (`response_type=code id_token`) returns authorization codes without the `c_hash` claim in the id_token — front-channel code integrity cannot be verified by the RP (OIDC Core §3.3.2.11 MUST violation); (2) When the RP sends `prompt=login`, the AS returns the existing session's id_token without forcing re-authentication — step-up authentication is silently bypassed (OIDC Core §3.1.2.1 MUST violation). Both are confirmed in independent OIDC compliance testing.

**B:**
REPROCESS. Two independent OIDC Core MUST violations: (1) OIDC Core §3.3.2.11 MUST: c_hash must be present in id_token when code is returned from hybrid authorization endpoint — absent; front-channel code substitution attacks undetectable; (2) OIDC Core §3.1.2.1 MUST: AS must force re-authentication when prompt=login — not done; stale session returned; step-up authentication security guarantee eliminated. Both are normative OIDC Core MUST requirements. Combined verdict: REPROCESS. Two MUST violations at different OIDC flow phases (authorization response and authentication parameters); both require independent remediation.

**Reason:** OIDC Core §3.3.2.11 MUST (c_hash absent in hybrid flow) + OIDC Core §3.1.2.1 MUST (prompt=login ignored). Two independent OIDC Core MUST violations; combined REPROCESS.

**Phrasing-B:**
REPROCESS. OIDC Core §3.3.2.11 MUST: c_hash absent from hybrid flow id_token → code substitution undetectable. OIDC Core §3.1.2.1 MUST: prompt=login ignored → step-up auth bypassed. Two OIDC Core MUSTs violated at different flow stages. Combined REPROCESS. Fix independently: add c_hash computation; implement prompt=login re-authentication.

---

## Pair[CB4-3.4-1088]

**Territory:** T-rfc-jwt · T-rfc-oauth2 · T-oauth-oidc · T-asvs-token
**Domain:** Comprehensive compliant implementation — DPoP full binding, PAR client binding, OIDC hybrid hashes, scope disclosure, TLS, expires_in — all correct
**Expected verdict:** PASS

**A:**
A production OAuth 2.0 / OIDC deployment is reviewed against the ext41 domain set. Findings: (1) DPoP proof validation: RS validates signature, jkt binding (cnf/jkt), iat freshness, htu (exact URI without query/fragment match), htm (exact HTTP method match), and jti uniqueness (Redis cache with TTL = freshness window + clock skew); (2) PAR request_uri handling: AS stores client_id with pushed request; at authorization endpoint, client_id verified against stored value; mismatch rejected with invalid_request; request_uri single-use + 60-second expiry; (3) OIDC hybrid c_hash: included in id_token when code returned at authorization endpoint; computed as BASE64URL(LEFT_HALF(SHA256(ASCII(code)))) matching JOSE alg; (4) OIDC hybrid at_hash: included in id_token when access token returned at authorization endpoint; same hash computation pattern; (5) DPoP jti cache: Redis cluster shared across RS instances; TTL = iat freshness window; UUID v4 jti values; (6) Scope disclosure: when issued scope ≠ requested scope, scope parameter returned in token response; exact-match omission correctly applied; (7) TLS: token endpoint requires TLS 1.2 minimum; TLS 1.3 preferred; TLS 1.0 and 1.1 explicitly disabled; PFS (Perfect Forward Secrecy) cipher suites only; (8) expires_in: included in all token responses (seconds until expiry); clients use proactive pre-expiry refresh.

**B:**
PASS. All ext41 domain requirements satisfied: (1) RFC 9449 §7.2 MUST: htu and htm validated; (2) RFC 9449 §7.2 MUST: jti uniqueness checked via distributed cache; (3) RFC 9126 §5 MUST: client_id binding verified for PAR request_uri; (4) OIDC Core §3.3.2.11 MUST: c_hash present when code returned at authorization endpoint; (5) OIDC Core §3.3.2.11 MUST: at_hash present when access token returned at authorization endpoint; (6) RFC 6749 §3.3 MUST: scope returned when issued scope differs from requested; (7) RFC 9325 §4 MUST NOT: TLS 1.0 and 1.1 not negotiated; TLS 1.2+ enforced; (8) RFC 6749 §5.1 RECOMMENDED: expires_in present in all token responses. No violations found.

**Reason:** All RFC 9449 §7.2, RFC 9126 §5, OIDC Core §3.3.2.11, RFC 6749 §3.3, RFC 9325 §4, and RFC 6749 §5.1 requirements satisfied across all ext41 domains. PASS.

**Phrasing-B:**
PASS. RFC 9449 §7.2 MUST: htu/htm/jti validated. RFC 9126 §5 MUST: client_id binding for PAR. OIDC Core §3.3.2.11 MUST: c_hash and at_hash in hybrid flow. RFC 6749 §3.3 MUST: scope on downgrade. RFC 9325 §4 MUST NOT: TLS 1.0/1.1 disabled. RFC 6749 §5.1 RECOMMENDED: expires_in present. All ext41 requirements met. PASS.
