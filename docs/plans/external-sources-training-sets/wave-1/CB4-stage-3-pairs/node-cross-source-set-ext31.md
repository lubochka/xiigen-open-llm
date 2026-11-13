# CB4 Stage 3 — Cross-Source Pair Set ext31
# Sub-phase 3.4 · Security and Authorization Model
# Sequences CB4-3.4-1001 through CB4-3.4-1010 (10 base pairs × 2 phrasings = 20 instances)
# Source domains: ext38 patterns (introspection-active-expired, scope-enlargement-code-exchange,
#   iss-absent-RS, access-token-TTL-long) combined with each other and with earlier patterns
# Running total after this file: 120 + (1010 − 60) × 2 = 2020 instances (1010 base pairs)
# Graduation rule: REPROCESS overrides CONCERN; N CONCERNs alone ≠ REPROCESS
# PO.1 escalation: PO.1 absence + traceable production MUST violation = REPROCESS

---

## Pair[CB4-3.4-1001-A]

**Territory:** T-rfc-oauth2 · T-asvs-token
**Domains combined:** introspection-active-expired (ext38) + scope-enlargement-code-exchange (ext38)
**Expected verdict:** REPROCESS

**A:**
An OAuth deployment has two gaps. First: the token introspection endpoint returns `active: true` for expired tokens (RFC 7662 §2.2 violated). Second: the token endpoint grants scope at the authorization code exchange that exceeds the original authorization grant — a client authorized for `scope=read:profile` receives an access token with `scope=read:profile write:profile` (RFC 6749 §4.1.3 MUST NOT violated).

**B:**
REPROCESS. Two independent MUST-level violations are present. First: RFC 7662 §2.2 — introspection active field must accurately reflect token status including expiry; returning active=true for expired tokens makes the status oracle unreliable. Second: RFC 6749 §4.1.3 MUST NOT — scope at token exchange must not exceed the original authorization grant; granting write:profile when only read:profile was consented violates the resource owner's consent boundary unconditionally. The interaction compounds risk: even if an attacker's access token expires, the introspection endpoint may still report it as active; and the scope enlargement means the token carried broader permissions than the user approved. Both violations independently mandate REPROCESS.

**Reason:** RFC 7662 §2.2 (active=true for expired) + RFC 6749 §4.1.3 MUST NOT (scope enlargement). Two independent MUST violations. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 7662 §2.2: introspection must report expired tokens as active=false — accurate status oracle is required. RFC 6749 §4.1.3 MUST NOT: scope granted must not exceed authorization grant — consent boundary violated. Two independent MUSTs. REPROCESS.

---

## Pair[CB4-3.4-1002-A]

**Territory:** T-rfc-jwt · T-asvs-token
**Domains combined:** iss-absent-RS (ext38, REPROCESS) + access-token-TTL-24h (ext38, CONCERN)
**Expected verdict:** REPROCESS (graduation: REPROCESS overrides CONCERN)

**A:**
A resource server does not validate the `iss` claim in JWT access tokens — any issuer's token is accepted if signature is valid (RFC 9068 §4 MUST violated = REPROCESS). The access tokens have a 24-hour TTL, substantially exceeding ASVS V9.3.1 guidance (CONCERN).

**B:**
REPROCESS. Graduation rule applies: REPROCESS overrides CONCERN. The iss absence violates RFC 9068 §4 MUST unconditionally — iss validation is a required step in JWT access token validation at the RS; omitting it allows tokens from unauthorized issuers to be accepted. This mandates REPROCESS. The 24-hour TTL is ASVS V9.3.1 guidance-level (CONCERN: "usually 15 min to 1 hour" — "usually" is not MUST). Graduation rule: REPROCESS controls. The combination is particularly concerning: an unauthorized-issuer token (iss not checked) with a 24-hour validity window (long TTL) could be used to access the RS for an extended period — but the verdict is REPROCESS on the iss violation alone, regardless of the TTL concern.

**Reason:** RFC 9068 §4 MUST (iss absent = REPROCESS) + ASVS V9.3.1 guidance (24h TTL = CONCERN). Graduation: REPROCESS overrides CONCERN. REPROCESS.

**Phrasing-B:**
REPROCESS. Graduation: REPROCESS + CONCERN = REPROCESS. RFC 9068 §4 iss absent = REPROCESS. ASVS V9.3.1 24h TTL = CONCERN. REPROCESS controls. Fix both: add iss allowlist validation; reduce TTL to 15 minutes.

---

## Pair[CB4-3.4-1003-A]

**Territory:** T-rfc-oauth2 · T-rfc-oidc
**Domains combined:** scope-enlargement-code-exchange (ext38) + offline_access-no-scope (ext36)
**Expected verdict:** REPROCESS

**A:**
A token endpoint exhibits two consent-boundary violations. First: authorization code exchange grants scope beyond the original authorization grant (RFC 6749 §4.1.3 MUST NOT). Second: refresh tokens are issued unconditionally, including for flows without `offline_access` scope (OIDC Core §11 MUST NOT).

**B:**
REPROCESS. Two independent MUST NOT violations related to consent-boundary management. First: RFC 6749 §4.1.3 MUST NOT — the token endpoint cannot enlarge scope beyond what the resource owner consented to at the authorization phase; granting additional scopes at exchange bypasses informed consent. Second: OIDC Core §11 MUST NOT — refresh tokens must not be issued without explicit `offline_access` scope consent; issuing them unconditionally extends application access beyond the user's online session without consent. Both violations concern the token endpoint exceeding the boundaries of what the resource owner authorized. Each independently mandates REPROCESS.

**Reason:** RFC 6749 §4.1.3 MUST NOT (scope enlargement at exchange) + OIDC Core §11 MUST NOT (refresh without offline_access). Two independent MUST NOT violations at the token endpoint. REPROCESS.

**Phrasing-B:**
REPROCESS. Two MUST NOT violations at the token endpoint: RFC 6749 §4.1.3 MUST NOT (scope enlargement beyond authorization grant) + OIDC Core §11 MUST NOT (refresh token without offline_access). Both exceed the resource owner's consent boundary. REPROCESS.

---

## Pair[CB4-3.4-1004-A]

**Territory:** T-ssdf-process · T-rfc-jwt
**Domains combined:** PO.1-absent + iss-absent-RS (ext38) — PO.1 escalation rule
**Expected verdict:** REPROCESS (PO.1 escalation: PO.1 gap + traceable production MUST violation)

**A:**
The development team has no formal SSDF PO.1 security training program. The resource server's JWT validation implementation is found to skip `iss` claim validation — any JWT with a valid signature is accepted regardless of issuer. When asked, the developer explains: "I implemented signature verification, aud check, and exp check. I didn't know RFC 9068 required iss validation too. I thought signature + aud was sufficient for JWT validation at a resource server."

**B:**
REPROCESS. PO.1 escalation rule applies: PO.1 absence alone is CONCERN; PO.1 absence combined with a traceable production MUST violation is REPROCESS. The developer's statement directly traces the RFC 9068 §4 MUST violation (iss not validated) to a knowledge gap that a PO.1-compliant training program would address — understanding that RFC 9068 §4 requires a specific validation sequence including iss, not just signature + aud. The MUST violation itself (iss absent from RS validation) is independently REPROCESS-eligible. PO.1 absence is the systemic root cause; the §4 violation is the production evidence with a clear causal link: "I didn't know RFC 9068 required iss validation too." Together: REPROCESS per the escalation rule.

**Reason:** PO.1 escalation: no formal security training + RFC 9068 §4 MUST violated (iss absent) + direct causal link (developer explicitly traces the §4 miss to not receiving training on RFC 9068 validation requirements). REPROCESS.

**Phrasing-B:**
REPROCESS. PO.1 escalation: no formal training (PO.1 absent — CONCERN) + RFC 9068 §4 MUST violated (iss not validated at RS) + direct causal link (developer states "I didn't know RFC 9068 required iss validation"). PO.1 + traceable production MUST violation = REPROCESS.

---

## Pair[CB4-3.4-1005-A]

**Territory:** T-rfc-oauth2 · T-rfc-jwt
**Domains combined:** introspection-active-expired (ext38) + JWT-typ-absent (ext36)
**Expected verdict:** REPROCESS

**A:**
A deployment has two RS-side validation failures. First: the token introspection endpoint returns active=true for expired tokens (RFC 7662 §2.2 violated). Second: JWT access tokens lack `typ: at+JWT` in the JOSE header, and the RS does not check typ (RFC 9068 §2.1 MUST violated). RSes rely entirely on introspection for token status — a token that is expired but reports active=true, with no typ check to prevent ID token substitution, creates a doubly unsafe validation posture.

**B:**
REPROCESS. Two independent MUST-level violations are present. First: RFC 7662 §2.2 — introspection must report expired tokens as active=false; the unreliable status oracle allows expired tokens to be used at RSes that trust introspection. Second: RFC 9068 §2.1 MUST — JWT access tokens must carry `typ: at+JWT`; absent typ enables token type confusion (ID token presented as access token). Both violations independently mandate REPROCESS. The combined impact: an attacker can present an ID token (typ not checked) that the introspection endpoint reports as active=true even after its exp (expiry not checked in introspection). Two distinct attack vectors are simultaneously open.

**Reason:** RFC 7662 §2.2 (active=true for expired = REPROCESS) + RFC 9068 §2.1 MUST (typ absent = REPROCESS). Two independent MUST violations affecting token validation at RS. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 7662 §2.2: introspection active=true for expired tokens undermines the status oracle. RFC 9068 §2.1 MUST: typ: at+JWT absent — token confusion attack vector open. Two independent MUSTs. Both attack vectors simultaneously active. REPROCESS.

---

## Pair[CB4-3.4-1006-A]

**Territory:** T-asvs-token · T-rfc-jwt
**Domains combined:** access-token-TTL-24h (ext38, CONCERN) + wildcard-aud (ext37, CONCERN)
**Expected verdict:** CONCERN (graduation: two CONCERNs do not escalate to REPROCESS)

**A:**
JWT access tokens have a 24-hour TTL (ASVS V9.3.1 guidance not met — CONCERN) and carry a wildcard audience value `aud="*.example.com"` instead of per-RS specific identifiers (RFC 8725 §3.9 SHOULD — CONCERN).

**B:**
CONCERN. Two SHOULD-level gaps are present. First: ASVS V9.3.1 guidance — 24-hour TTL substantially exceeds the "usually 15 minutes to 1 hour" guidance range; "usually" is guidance language, not MUST. CONCERN. Second: RFC 8725 §3.9 — wildcard aud defeats per-RS audience isolation; SHOULD-level guidance toward specific identifiers. CONCERN. Graduation rule: two CONCERNs do not escalate to REPROCESS. No MUST violations are present. The combined security posture is meaningful: long-lived tokens (24h) with no RS audience restriction (wildcard aud) means a single exfiltrated token is valid at any RS on the domain for a full day. However, neither gap is a MUST violation; the verdict is CONCERN with heightened urgency. Both should be remediated: reduce TTL to 15 minutes; adopt per-RS aud via RFC 8707 resource indicators.

**Reason:** ASVS V9.3.1 guidance (24h TTL = CONCERN) + RFC 8725 §3.9 SHOULD (wildcard aud = CONCERN). Both guidance-level. Graduation: N CONCERNs = CONCERN. CONCERN.

**Phrasing-B:**
CONCERN. ASVS V9.3.1 24h TTL = CONCERN. RFC 8725 §3.9 wildcard aud = CONCERN. Graduation: two CONCERNs = CONCERN; they do not escalate to REPROCESS. Combined: long-lived, audience-unrestricted tokens — but both gaps are SHOULD-level. Verdict: CONCERN.

---

## Pair[CB4-3.4-1007-A]

**Territory:** T-rfc-oauth2 · T-cwe-authz
**Domains combined:** scope-enlargement-code-exchange (ext38) + state-not-validated (ext34)
**Expected verdict:** REPROCESS

**A:**
The authorization code flow has two gaps: (1) the callback handler logs but does not validate the `state` parameter — CSRF protection absent (RFC 6749 §10.12 MUST); (2) the token endpoint grants scope beyond the authorization grant — scope enlargement at code exchange (RFC 6749 §4.1.3 MUST NOT).

**B:**
REPROCESS. Two independent RFC 6749 MUST violations. First: RFC 6749 §10.12 MUST — CSRF protection requires state validation; logging without comparison provides zero CSRF protection; attackers can inject their authorization code into a victim's session. Second: RFC 6749 §4.1.3 MUST NOT — scope at token exchange must not exceed the authorization grant; scope enlargement violates resource owner consent. The combination of these two gaps enables a particularly damaging attack: an attacker who CSRF-injects their authorization code into a victim's callback session (state not validated), then the token endpoint grants broader scope than the user originally authorized (scope enlargement) — the victim is bound to the attacker's grant with expanded permissions. Both MUSTs are independently violated; each alone mandates REPROCESS.

**Reason:** RFC 6749 §10.12 MUST (state not validated = REPROCESS) + RFC 6749 §4.1.3 MUST NOT (scope enlargement = REPROCESS). Two RFC 6749 violations, both from the same specification, affecting different phases. REPROCESS.

**Phrasing-B:**
REPROCESS. Two RFC 6749 violations: §10.12 MUST (CSRF protection requires state validation — logging ≠ validation) + §4.1.3 MUST NOT (token exchange scope enlargement beyond authorization grant). The attack path: CSRF code injection (state bypass) → token exchange with expanded scope (§4.1.3 bypass) → victim receives attacker's grant with unauthorized permissions. Each violation is independently REPROCESS; combined attack is especially harmful. REPROCESS.

---

## Pair[CB4-3.4-1008-A]

**Territory:** T-rfc-oauth2 · T-rfc-jwt
**Domains combined:** iss-absent-RS (ext38) + PAR-enforcement-bypass (ext36)
**Expected verdict:** REPROCESS

**A:**
The deployment has two OAuth server-side enforcement failures. First: the authorization server advertises `require_pushed_authorization_requests=true` but accepts direct authorization requests as a fallback (RFC 9126 §4 MUST violated). Second: the resource server does not validate the `iss` claim in JWT access tokens — any issuer's JWT is accepted if signature is valid (RFC 9068 §4 MUST violated).

**B:**
REPROCESS. Two independent MUST-level violations are present. First: RFC 9126 §4 MUST — authorization endpoint must reject non-PAR requests when require_pushed_authorization_requests=true is advertised; the fallback path nullifies all PAR security properties (parameter injection protection, parameter confidentiality, request integrity). Second: RFC 9068 §4 MUST — RS must validate iss against the expected AS identifier; omitting iss allows tokens from unauthorized issuers to be accepted if their signing keys are valid. Both violations independently mandate REPROCESS. The combined attack surface: an attacker can bypass PAR (parameter injection via direct authorization request) to obtain an authorization code, then present a forged token from an unauthorized issuer at the RS (iss not checked) — two independent attack paths.

**Reason:** RFC 9126 §4 MUST (PAR bypass) + RFC 9068 §4 MUST (iss absent at RS). Two independent MUST violations affecting AS-side and RS-side respectively. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 9126 §4 MUST: PAR enforcement absent despite metadata advertisement — AS-side MUST violation. RFC 9068 §4 MUST: iss not validated at RS — RS-side MUST violation. Two independent MUSTs affecting both ends of the authorization flow. REPROCESS.

---

## Pair[CB4-3.4-1009-A]

**Territory:** T-asvs-token · T-rfc-oidc
**Domains combined:** access-token-TTL-24h (ext38, CONCERN) + acr-not-validated (ext34, CONCERN)
**Expected verdict:** CONCERN (graduation: two CONCERNs do not escalate to REPROCESS)

**A:**
Access tokens have a 24-hour TTL (ASVS V9.3.1 guidance not met — CONCERN). The client also fails to validate the `acr` claim in returned ID tokens despite requesting `acr_values=2` (OIDC Core §3.1.3.7.9 SHOULD — CONCERN).

**B:**
CONCERN. Two SHOULD-level gaps are present. First: ASVS V9.3.1 guidance — 24-hour TTL substantially exceeds the guidance range; guidance-level gap = CONCERN. Second: OIDC Core §3.1.3.7.9 — acr claim should be validated against requested level; silent acr downgrade (level 2 → level 1) undetected; SHOULD-level = CONCERN. Graduation rule: two CONCERNs do not escalate to REPROCESS. No MUST violations are present. The combined risk is notable: an attacker who bypasses MFA via acr downgrade (acr not validated) receives a long-lived access token (24-hour TTL) that persists for an extended exploitation window. However, both gaps are SHOULD-level; the verdict is CONCERN. Both should be remediated: reduce access token TTL to 15 minutes; validate acr ≥ requested level at ID token receipt.

**Reason:** ASVS V9.3.1 guidance (24h TTL = CONCERN) + OIDC Core §3.1.3.7.9 SHOULD (acr not validated = CONCERN). Both SHOULD-level. Graduation rule: N CONCERNs = CONCERN. CONCERN.

**Phrasing-B:**
CONCERN. ASVS V9.3.1 24h TTL = CONCERN. OIDC Core §3.1.3.7.9 SHOULD acr not validated = CONCERN. Graduation: two CONCERNs = CONCERN. Combined risk: MFA-downgrade + long-lived token exploitation window — both SHOULD gaps, not MUST violations. Verdict: CONCERN.

---

## Pair[CB4-3.4-1010-A]

**Territory:** T-rfc-oauth2 · T-rfc-jwt · T-asvs-token
**Domains combined:** comprehensive PASS — introspection correct + scope bounded + iss validated + 15-min TTL
**Expected verdict:** PASS

**A:**
A deployment satisfies all four ext38 PASS conditions simultaneously. (1) Token introspection evaluates expiry before responding: expired tokens receive `{"active": false}`; valid tokens receive full active responses with current claims. (2) Token exchange scope is validated against the authorization grant: excess scope rejected with error=invalid_scope; downscoping accepted; resource owner consent boundary upheld. (3) The resource server validates the `iss` claim as an exact match against a configured allowlist `["https://auth.example.com"]` before key lookup; tokens from other issuers receive 401. (4) Access tokens have a 15-minute TTL (`expires_in=900`), within ASVS V9.3.1's guidance range.

**B:**
PASS. No arbiter violations found. RFC 7662 §2.2 satisfied: introspection active field accurately reflects token status including expiry. RFC 6749 §4.1.3 MUST NOT satisfied: scope at token exchange validated against authorization grant; no consent boundary violation. RFC 9068 §4 MUST satisfied: iss validated against exact allowlist before key lookup; unauthorized-issuer tokens rejected. ASVS V9.3.1 guidance satisfied: 15-minute TTL is within the "usually 15 minutes to 1 hour" guidance range. All four ext38 PASS conditions simultaneously satisfied.

**Reason:** RFC 7662 §2.2, RFC 6749 §4.1.3, RFC 9068 §4, ASVS V9.3.1 — all satisfied. No MUST NOT violations, no MUST violations, no guidance gaps. Correct integrated deployment. PASS.

**Phrasing-B:**
PASS. Four simultaneous PASS conditions: (1) introspection active field accurate per RFC 7662 §2.2; (2) token exchange scope ceiling enforced per RFC 6749 §4.1.3 MUST NOT; (3) RS iss validation exact allowlist per RFC 9068 §4 MUST; (4) 15-minute access token TTL per ASVS V9.3.1 guidance. No violations at any layer. PASS.
