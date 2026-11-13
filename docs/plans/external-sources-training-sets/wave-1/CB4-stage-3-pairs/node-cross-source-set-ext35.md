# CB4 Stage 3 — Node Cross-Source Set ext35
# Sub-phase 3.4 · Security and Authorization Model
# Sequences CB4-3.4-1105 through CB4-3.4-1114 (10 pairs × 2 phrasings = 20 instances)
# Covers: cross-source combinations of ext42 patterns with each other and with prior sub-phase patterns
# Running total after this file: 120 + (1114 − 60) × 2 = 2228 instances (1114 base pairs)
# Graduation rule: N CONCERNs do NOT escalate to REPROCESS (demonstrated: 1110, 1111)
# PO.1 escalation: PO.1 absence + traceable production MUST violation = REPROCESS (demonstrated: 1109)
# REPROCESS overrides CONCERN when both apply to same scenario (demonstrated: 1112, 1113)

---

## Pair[CB4-3.4-1105]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** DPoP cnf/jkt absent from issued AT (REPROCESS) AND DPoP htu/htm not validated at RS (REPROCESS) — dual DPoP violations at issuance and validation layers
**Expected verdict:** REPROCESS

**A:**
A deployment has two DPoP (Demonstrating Proof of Possession) failures at different layers: (1) The Authorization Server issues access tokens without `cnf/jkt` when DPoP proofs are presented at the token endpoint — the tokens have plain bearer semantics (RFC 9449 §6 MUST violation); (2) The Resource Server independently also does not validate `htu` or `htm` in DPoP proofs — even if tokens had `cnf/jkt`, endpoint binding would not be enforced (RFC 9449 §7.2 MUST violation). Both failures were identified in the same DPoP implementation audit.

**B:**
REPROCESS. Two independent RFC 9449 MUST violations at different protocol layers: (1) RFC 9449 §6 MUST: AS must embed cnf/jkt in AT when DPoP proof presented — DPoP sender-constraint cannot be enforced at RS without the key thumbprint in the token; (2) RFC 9449 §7.2 MUST: RS must validate htu and htm against current request — even if cnf/jkt were present, endpoint binding would not be enforced. The combined effect is a complete DPoP implementation failure: issuance doesn't bind keys, and RS-side validation doesn't check binding claims. Both require independent remediation. Combined verdict: REPROCESS.

**Reason:** RFC 9449 §6 MUST (cnf/jkt absent in AT) + RFC 9449 §7.2 MUST (htu/htm not validated). Dual DPoP MUST violations at issuance and validation layers. Combined REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 9449 §6 MUST: AS issues AT without cnf/jkt — DPoP binding not established. RFC 9449 §7.2 MUST: RS doesn't validate htu/htm — endpoint binding not enforced. Two-layer DPoP failure; complete sender-constraint breakdown. Combined REPROCESS. Fix both layers independently.

---

## Pair[CB4-3.4-1106]

**Territory:** T-oauth-oidc · T-rfc-jwt · T-asvs-token
**Domain:** OIDC id_token iss uses HTTP (REPROCESS) AND OIDC hybrid flow c_hash absent (REPROCESS) — dual OIDC Core MUST violations
**Expected verdict:** REPROCESS

**A:**
An OIDC AS has two normative failures: (1) id_tokens have `iss=http://auth.example.com` — HTTP scheme violates OIDC Core §2's MUST for HTTPS; (2) In hybrid flow (`response_type=code id_token`), id_tokens omit the `c_hash` claim — violates OIDC Core §3.3.2.11 MUST when authorization code is returned from the authorization endpoint. Both failures were discovered in the same OIDC conformance test run.

**B:**
REPROCESS. Two independent OIDC Core MUST violations: (1) OIDC Core §2 MUST: iss claim must use https scheme — HTTP iss breaks RP issuer validation and exposes discovery document to MITM; (2) OIDC Core §3.3.2.11 MUST: c_hash must be present when code is returned at authorization endpoint — absent c_hash enables front-channel code substitution attacks. Both are unconditional OIDC Core normative requirements. Combined verdict: REPROCESS.

**Reason:** OIDC Core §2 MUST (iss=http violates https requirement) + OIDC Core §3.3.2.11 MUST (c_hash absent in hybrid flow). Dual OIDC Core MUST violations. Combined REPROCESS.

**Phrasing-B:**
REPROCESS. OIDC Core §2 MUST: iss must use https scheme — http://... violates it. OIDC Core §3.3.2.11 MUST: c_hash required when code returned at hybrid auth endpoint — absent. Two OIDC Core MUSTs at different levels (token claim format + hybrid hash). Combined REPROCESS.

---

## Pair[CB4-3.4-1107]

**Territory:** T-rfc-oauth2 · T-rfc-jwt · T-asvs-token
**Domain:** RFC 7009 revocation returns 400 for unrecognized tokens (REPROCESS) AND DPoP jti not tracked (REPROCESS) — different RFC MUST violations
**Expected verdict:** REPROCESS

**A:**
An OAuth 2.0 deployment has two independent MUST violations: (1) The token revocation endpoint returns `400 Bad Request` for unrecognized or already-expired tokens, instead of the RFC 7009 §2.2 MUST of returning 200 — token validity enumeration via the revocation endpoint is possible; (2) The resource server does not maintain a server-side DPoP jti replay cache — DPoP proofs are replayable multiple times within the 60-second freshness window per RFC 9449 §7.2 MUST. Both failures are in separate components (revocation endpoint at AS; DPoP jti tracking at RS).

**B:**
REPROCESS. Two independent MUST violations at different components: (1) RFC 7009 §2.2 MUST: revocation endpoint must return 200 for unrecognized tokens — 400 enables token validity enumeration; (2) RFC 9449 §7.2 MUST: RS must track jti uniqueness to prevent DPoP proof replay — absent cache allows proof replay within 60 seconds. Both are unconditional MUST requirements from different RFCs. Combined verdict: REPROCESS.

**Reason:** RFC 7009 §2.2 MUST (400 for unrecognized token = enumeration) + RFC 9449 §7.2 MUST (jti not tracked = DPoP replay). Two RFC MUST violations at different components. Combined REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 7009 §2.2 MUST: return 200 for unrecognized tokens — 400 enables enumeration. RFC 9449 §7.2 MUST: jti must be tracked for replay prevention — not tracked. Independent MUSTs at AS (revocation) and RS (DPoP). Combined REPROCESS.

---

## Pair[CB4-3.4-1108]

**Territory:** T-rfc-oauth2 · T-asvs-token
**Domain:** redirect_uri not bound at authorization code issuance (REPROCESS) AND PAR request_uri client_id binding absent (REPROCESS) — dual authorization endpoint binding failures
**Expected verdict:** REPROCESS

**A:**
An Authorization Server has two related binding failures at the authorization endpoint: (1) Authorization codes are not bound to the specific redirect_uri from the authorization request — at token exchange, any registered redirect_uri is accepted (RFC 6749 §4.1.3 MUST violation); (2) PAR `request_uri` values are not verified against the client_id at the authorization endpoint — any client knowing a request_uri can use it with different credentials (RFC 9126 §5 MUST violation). Both failures indicate missing request-to-code binding mechanisms.

**B:**
REPROCESS. Two independent MUST violations at the authorization endpoint binding layer: (1) RFC 6749 §4.1.3 MUST: redirect_uri at token exchange must exactly match the authorization request redirect_uri — absent binding allows code injection via redirect_uri substitution; (2) RFC 9126 §5 MUST: client_id at authorization endpoint must match the PAR-associated client_id — absent check allows cross-client request_uri substitution. Both are unconditional normative requirements. Combined verdict: REPROCESS.

**Reason:** RFC 6749 §4.1.3 MUST (redirect_uri not bound/verified at token exchange) + RFC 9126 §5 MUST (PAR client_id binding absent). Dual authorization-layer binding MUSTs. Combined REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 6749 §4.1.3 MUST: redirect_uri not stored with code; any registered URI accepted at exchange → code injection risk. RFC 9126 §5 MUST: PAR request_uri client_id not verified → cross-client substitution. Two authorization-layer binding failures. Combined REPROCESS.

---

## Pair[CB4-3.4-1109]

**Territory:** T-rfc-oauth2 · T-asvs-token
**Domain:** PO.1 absent AND redirect_uri not bound at authorization code issuance — PO.1 escalation to REPROCESS
**Expected verdict:** REPROCESS

**A:**
An Authorization Server does not bind authorization codes to the specific redirect_uri from authorization requests — at token exchange, any of the client's registered redirect_uris is accepted (RFC 6749 §4.1.3 MUST violation). Additionally, the AS has no formal security policy document (PO.1 gap): there is no documented requirement for authorization code parameter binding, redirect_uri verification at token exchange, or code injection prevention. The developer comment when the issue is raised: "We validate redirect_uri at authorization time — that seemed enough. We have no policy specifying what needs to be checked at token exchange."

**B:**
REPROCESS. PO.1 (absence of formal security policy): no documented requirements for authorization code binding or redirect_uri verification at token exchange — guidance-level gap (CONCERN in isolation). The PO.1 escalation rule applies: PO.1 absence + traceable production MUST violation = REPROCESS. The traceable production MUST violation: RFC 6749 §4.1.3 MUST: redirect_uri at token exchange must exactly match the authorization request redirect_uri — actively violated in production. PO.1 gap explains why the validation was not implemented (no policy requirement documented) but does not mitigate the MUST violation. Combined: PO.1 (escalation trigger) + RFC 6749 §4.1.3 MUST violation (traceable production fault) = REPROCESS.

**Reason:** PO.1 absence (CONCERN) + RFC 6749 §4.1.3 MUST violation (redirect_uri not bound in production) = REPROCESS per PO.1 escalation rule.

**Phrasing-B:**
REPROCESS. RFC 6749 §4.1.3 MUST: redirect_uri at token exchange must match authorization request — not enforced in production. PO.1 gap: no formal policy documenting this requirement. PO.1 alone = CONCERN; PO.1 + traceable production MUST = REPROCESS. Fix redirect_uri binding; document code exchange requirements in security policy.

---

## Pair[CB4-3.4-1110]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** JWKS rollover window < max AT TTL (CONCERN) AND JWKS `use` parameter absent (CONCERN) — graduation rule: N CONCERNs ≠ REPROCESS
**Expected verdict:** CONCERN

**A:**
An Authorization Server's JWKS configuration has two guidance gaps: (1) The JWKS rollover procedure removes the old signing key after 30 minutes, while the maximum AT TTL is 1 hour — valid tokens may be rejected after key rotation (ASVS V9.2 guidance gap); (2) The JWKS does not include `use` parameters on any keys — RSes must try all keys for signature verification, cannot distinguish signing from encryption keys (RFC 7517 §4.2 SHOULD gap). Both gaps are in the JWKS operational configuration.

**B:**
CONCERN. Two guidance gaps: (1) JWKS overlap window too short: ASVS V9.2 guidance — "sufficient overlap" — 30-minute window < 1-hour max TTL; no RFC MUST for specific numeric overlap; valid token rejections possible; CONCERN; (2) JWKS use absent: RFC 7517 §4.2 SHOULD — use parameter should be present; no RFC MUST; key-type confusion and RS overhead; CONCERN. The graduation rule applies: N CONCERNs do NOT escalate to REPROCESS. Two ASVS/RFC SHOULD gaps remain CONCERN. Combined verdict: CONCERN. Recommendations: (1) extend overlap window to ≥ max AT TTL; (2) add use=sig and use=enc to respective keys.

**Reason:** ASVS V9.2 guidance gap (30-min overlap < 1-hour TTL) = CONCERN + RFC 7517 §4.2 SHOULD gap (use absent) = CONCERN. Two CONCERNs do NOT escalate. Combined CONCERN.

**Phrasing-B:**
CONCERN. JWKS overlap 30 min < 1-hour TTL: ASVS V9.2 guidance gap (CONCERN). JWKS use absent: RFC 7517 §4.2 SHOULD gap (CONCERN). Graduation rule: 2 CONCERNs = CONCERN not REPROCESS. No RFC MUST violated. Extend overlap; add use parameters.

---

## Pair[CB4-3.4-1111]

**Territory:** T-rfc-oauth2 · T-oauth-oidc · T-asvs-token
**Domain:** Introspection returns PII beyond scope (CONCERN) AND user enumeration via login_hint (CONCERN) — graduation rule: N CONCERNs ≠ REPROCESS
**Expected verdict:** CONCERN

**A:**
An OIDC deployment has two guidance gaps: (1) The token introspection endpoint returns user PII (email, phone, address) for all access tokens regardless of scope — RFC 7662 §2.2 SHOULD NOT and ASVS V8.2.2 concern; (2) The authorization endpoint's login_hint handling returns `error_description="User not found in directory"` for unregistered users, enabling email address enumeration — CWE-204 and ASVS V6.1.3 guidance gap. Both are acknowledged in a security review.

**B:**
CONCERN. Two guidance gaps: (1) Introspection PII beyond scope: RFC 7662 §2.2 SHOULD NOT — no RFC MUST NOT; ASVS V8.2.2 logging concern; CONCERN; (2) User enumeration via login_hint: CWE-204 observable discrepancy — no RFC MUST NOT for login_hint response behavior; ASVS V6.1.3 guidance gap; CONCERN. The graduation rule is explicit: N CONCERNs do NOT escalate to REPROCESS regardless of count. Two guidance gaps remain CONCERN. Combined verdict: CONCERN. Recommendations: (1) scope-filter introspection responses; (2) return uniform login_required regardless of user existence.

**Reason:** RFC 7662 §2.2 SHOULD NOT gap (introspection PII) = CONCERN + CWE-204/ASVS V6.1.3 gap (login_hint enumeration) = CONCERN. Two CONCERNs do NOT escalate. Combined CONCERN.

**Phrasing-B:**
CONCERN. Introspection PII beyond scope: RFC 7662 §2.2 SHOULD NOT not met (CONCERN). User enumeration via login_hint: CWE-204 + ASVS V6.1.3 guidance gap (CONCERN). Graduation rule: 2 CONCERNs = CONCERN, not REPROCESS. Address both: scope-filter introspection; uniform login_required.

---

## Pair[CB4-3.4-1112]

**Territory:** T-rfc-jwt · T-rfc-oauth2 · T-asvs-token
**Domain:** DPoP AT without cnf/jkt (REPROCESS) combined with introspection returns PII beyond scope (CONCERN) — REPROCESS overrides CONCERN
**Expected verdict:** REPROCESS

**A:**
A deployment has two findings at different severity levels: (1) The AS issues access tokens without `cnf/jkt` when DPoP proofs are presented — the tokens lack sender-constraint binding and function as plain bearer tokens (RFC 9449 §6 MUST violation); (2) The token introspection endpoint includes user PII (email, phone) in responses for tokens with `scope=read:data` — beyond what the token scope authorizes (RFC 7662 §2.2 SHOULD NOT gap). The two issues are in different components.

**B:**
REPROCESS. Two findings: (1) RFC 9449 §6 MUST: cnf/jkt must be in AT when DPoP proof presented — absent; AT has plain bearer semantics; DPoP protection nullified. This is REPROCESS; (2) RFC 7662 §2.2 SHOULD NOT: introspection should not include PII beyond token scope — guidance gap; no RFC MUST NOT. This is CONCERN. The graduation rule: REPROCESS overrides CONCERN. Combined verdict: REPROCESS (driven by RFC 9449 §6 MUST). The introspection PII gap remains a CONCERN that should be addressed, but it does not change the verdict.

**Reason:** RFC 9449 §6 MUST (cnf/jkt absent = REPROCESS) + RFC 7662 §2.2 SHOULD NOT gap (introspection PII = CONCERN). REPROCESS overrides CONCERN. Combined REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 9449 §6 MUST: cnf/jkt absent from AT → DPoP sender-constraint nullified → REPROCESS. RFC 7662 §2.2 SHOULD NOT: introspection PII beyond scope → CONCERN. REPROCESS overrides CONCERN per graduation rule. Combined: REPROCESS. Address cnf/jkt (critical); also scope-filter introspection.

---

## Pair[CB4-3.4-1113]

**Territory:** T-oauth-oidc · T-rfc-oauth2 · T-asvs-token
**Domain:** OIDC id_token iss uses HTTP (REPROCESS) combined with `expires_in` absent from token response (CONCERN) — REPROCESS overrides CONCERN
**Expected verdict:** REPROCESS

**A:**
A deployment has findings at two different severity levels: (1) OIDC id_tokens have `iss=http://auth.example.com` — HTTP scheme violates OIDC Core §2's MUST for HTTPS; RP issuer validation fails; OIDC discovery document exposed to MITM; (2) Token responses do not include `expires_in` — clients cannot proactively manage token expiry; forced into reactive 401-driven refresh pattern (RFC 6749 §5.1 RECOMMENDED gap). Both are in the same AS deployment.

**B:**
REPROCESS. Two findings: (1) OIDC Core §2 MUST: iss must use https scheme — violated; RP cannot validate iss; HTTP discovery susceptible to MITM. This is REPROCESS; (2) RFC 6749 §5.1 RECOMMENDED: expires_in should be in token response — absent; clients use reactive 401 refresh; no RFC MUST violated. This is CONCERN. REPROCESS overrides CONCERN per the graduation rule. Combined verdict: REPROCESS (driven by OIDC Core §2 MUST). The expires_in gap remains a CONCERN that should be addressed concurrently.

**Reason:** OIDC Core §2 MUST (iss=http violates https requirement = REPROCESS) + RFC 6749 §5.1 RECOMMENDED gap (expires_in absent = CONCERN). REPROCESS overrides CONCERN. Combined REPROCESS.

**Phrasing-B:**
REPROCESS. OIDC Core §2 MUST: iss=http://... violates https requirement → REPROCESS. RFC 6749 §5.1 RECOMMENDED: expires_in absent → CONCERN. REPROCESS overrides CONCERN. Combined: REPROCESS. Fix iss to HTTPS (critical); add expires_in (improvement).

---

## Pair[CB4-3.4-1114]

**Territory:** T-rfc-jwt · T-rfc-oauth2 · T-oauth-oidc · T-asvs-token
**Domain:** Comprehensive compliant implementation — DPoP cnf/jkt, OIDC iss HTTPS, JWKS overlap, revocation 200, redirect_uri binding, JWKS use, introspection filtering, login_hint uniform — all correct
**Expected verdict:** PASS

**A:**
A production OAuth 2.0 / OIDC deployment is reviewed against the ext42 domain set. Findings: (1) DPoP AT issuance: when DPoP proof presented at token endpoint, AS computes JWK thumbprint from DPoP public key (RFC 7638); embeds cnf={"jkt":"<thumbprint>"} in issued JWT AT; RS validates cnf/jkt on every API call; (2) OIDC iss claim: id_tokens issued with iss=https://auth.example.com (HTTPS scheme); matches OIDC Discovery issuer field; RP validates via exact case-sensitive comparison; (3) JWKS rollover: key overlap window 2 hours (2× max AT TTL of 1 hour); old key retained during full overlap; new tokens signed with new key; old tokens verified with old key until expiry; (4) Token revocation: endpoint returns 200 for all revocation outcomes (successfully revoked, unrecognized, expired, already-revoked); authentication failures → 401; (5) redirect_uri binding: specific redirect_uri stored with authorization code; exact match enforced at token exchange; mismatch → invalid_grant; (6) JWKS `use` parameter: use=sig on signing keys; use=enc on encryption keys; RS filters by use=sig for signature verification; (7) Introspection scope-filtering: introspection response includes only claims authorized by presented token scope; PII excluded for non-PII scopes; (8) login_hint uniform response: uniform login_required returned regardless of whether login_hint identifies a registered user; no observable discrepancy.

**B:**
PASS. All ext42 domain requirements satisfied: (1) RFC 9449 §6 MUST: cnf/jkt embedded in AT when DPoP proof presented; (2) OIDC Core §2 MUST: iss uses https scheme; (3) ASVS V9.2 guidance: overlap window ≥ max AT TTL; (4) RFC 7009 §2.2 MUST: 200 for all revocation outcomes; (5) RFC 6749 §4.1.3 MUST: redirect_uri bound at issuance, verified at exchange; (6) RFC 7517 §4.2 SHOULD: use parameter present on all JWKS keys; (7) RFC 7662 §2.2 SHOULD NOT satisfied: introspection scope-filtered; (8) CWE-204/ASVS V6.1.3 guidance: uniform login_required prevents user enumeration. No violations found.

**Reason:** All RFC 9449 §6, OIDC Core §2, ASVS V9.2, RFC 7009 §2.2, RFC 6749 §4.1.3, RFC 7517 §4.2, RFC 7662 §2.2, and ASVS V6.1 requirements satisfied across all ext42 domains. PASS.

**Phrasing-B:**
PASS. RFC 9449 §6 MUST: cnf/jkt in AT. OIDC Core §2 MUST: iss https. RFC 7009 §2.2 MUST: 200 for revocation. RFC 6749 §4.1.3 MUST: redirect_uri bound and verified. RFC 7517 §4.2 SHOULD: use present. RFC 7662 §2.2 SHOULD NOT: introspection scope-filtered. CWE-204 mitigated: uniform login_required. All ext42 requirements met. PASS.
