# CB4 Stage 3 — Action 3 Pipeline Chain Set
# Node: Multi-node (full 9-node traces) — ext28
# Sequences: CB4-3.4-889 through CB4-3.4-896
# Instances: 16 (8 base pairs × 2 phrasings each)
# Stratum: C (full pipeline traces for ext34 patterns)
# Running total after this file: 1792 instances (896 base pairs)

---

## Pair[CB4-3.4-889]

### A
Full pipeline trace — OAuth state parameter sent but not validated at callback (RFC 6749 §10.12):

A web application sends a 256-bit state in authorization requests but does not validate the state at the callback. The callback handler extracts and uses the authorization code without checking whether the returned state matches what was sent.

### B
Node 1 — Domain Splitter: Input domain: T-rfc-oauth2 / T-cwe-authn. Sub-domain: OAuth state parameter CSRF protection — state sent but validation absent at callback. CWE mapping: CWE-352 (Cross-Site Request Forgery). Pipeline route: Node 8 RFC arbiter with RFC 6749 §10.12 MUST analysis.

Node 2 — Domain Translator: RFC 6749 §10.12: "The client MUST implement CSRF protection for its redirect URI." The state parameter is the standard mechanism for this protection. State validation at the callback is the act that implements the CSRF protection — without it, the state is a non-functional artifact. CWE-352: CSRF against OAuth callback enables token injection attacks.

Node 3 — Scheme Planner: Verify state validation present at callback. Model CSRF attack path: attacker's code injected into victim's callback. Evaluate "sending for logging" argument. Expected: REPROCESS if §10.12 CSRF protection non-functional.

Node 4 — Domain Processor: State sent: 256-bit random — present in authorization request. State validated at callback: NO — callback extracts code without checking state. CSRF protection: non-functional. Attack path: (1) attacker initiates their own authorization flow, obtains an authorization code. (2) Attacker crafts a URL to victim's callback endpoint using attacker's code. (3) Victim's browser navigates to the crafted URL. (4) Callback processes attacker's code; session bound to attacker's identity. State mismatch would have been detected (victim's state ≠ attacker's callback state) — but check is absent. "Sending for logging" argument: zero CSRF protection from transmitted-but-unvalidated state. RFC 6749 §10.12 MUST: violated — CSRF protection non-functional.

Node 5 — Applier: RFC 6749 §10.12 MUST CSRF protection non-functional. CWE-352 CSRF attack against OAuth callback enabled. REPROCESS.

Node 6 — Result Translator: REPROCESS. State sent but not validated = zero CSRF protection. Attacker's authorization code accepted at victim's callback. Victim's session bound to attacker's tokens.

Node 7 — Arbiter Selector: Route to Node 8 RFC arbiter (T-rfc-oauth2) + T-cwe-authn (CWE-352). Both confirm REPROCESS severity.

Node 8 — Arbiter Units: REPROCESS. RFC 6749 §10.12 MUST CSRF protection violated. State transmitted without validation provides no protection. CWE-352 CSRF against OAuth callback active. "Logging purposes" argument rejected. REPROCESS.

Node 9 — Formatter: REPROCESS. RFC 6749 §10.12 MUST CSRF protection for redirect URI violated. State parameter sent but not validated at callback — CSRF protection non-functional. CWE-352 CSRF attack: attacker's authorization code injected into victim's callback via CSRF, binding victim's session to attacker's tokens. Remediation: validate state at callback with exact-match comparison against session-stored value; single-use enforcement; reject absent state.

### Reason
9-node trace correctly identifies RFC 6749 §10.12 MUST CSRF protection violation, models the state-sent-but-unvalidated gap, demonstrates the CSRF attack path, and rejects the logging argument. REPROCESS.

### Phrasing-B
REPROCESS — 9-node trace: state sent but not validated, RFC 6749 §10.12 MUST CSRF protection non-functional, CWE-352.

N1: T-rfc-oauth2 + CWE-352 / state parameter CSRF protection; validation absent.
N2: RFC 6749 §10.12 MUST; state validation at callback = CSRF protection implementation.
N3: Verify validation present; model CSRF attack; evaluate "logging" argument.
N4: State sent; validation absent; attack: attacker's code → victim's callback → victim bound to attacker token.
N5: CSRF protection non-functional. REPROCESS.
N6: REPROCESS — state without validation = zero protection.
N7: RFC arbiter + T-cwe-authn CWE-352; REPROCESS confirmed.
N8: REPROCESS — RFC 6749 §10.12 MUST violated.
N9: REPROCESS. Validate state: exact-match + single-use + reject-absent.

---

## Pair[CB4-3.4-890]

### A
Full pipeline trace — OAuth state parameter correctly generated and validated:

A web application generates 256-bit CSPRNG state per authorization request, stores it in the server-side session, and validates the callback state with exact-match comparison. Absent or mismatched state causes callback rejection.

### B
Node 1 — Domain Splitter: Input domain: T-rfc-oauth2 / T-cwe-authn. Sub-domain: OAuth state CSRF protection — all validation steps present. Pipeline route: Node 8 RFC arbiter with positive compliance confirmation.

Node 2 — Domain Translator: RFC 6749 §10.12 MUST CSRF protection. State = 256-bit CSPRNG (unpredictable), stored in session, validated at callback. CWE-352 mitigation: attacker cannot know victim's state value.

Node 3 — Scheme Planner: Verify 256-bit CSPRNG generation. Verify session storage. Verify exact-match comparison. Verify single-use. Verify absent-rejected. Expected: PASS if all steps confirmed.

Node 4 — Domain Processor: Generation: 256-bit CSPRNG — unpredictable; brute-force infeasible. Session storage: server-side. Callback validation: exact match against session value — case-sensitive. Single-use: state invalidated on use. Absent-rejected: callback without state rejected. CSRF attack: attacker cannot predict victim's 256-bit state; CSRF-injected callback fails validation. All controls confirmed.

Node 5 — Applier: RFC 6749 §10.12 CSRF protection fully implemented. CWE-352 mitigated. PASS.

Node 6 — Result Translator: PASS. 256-bit CSPRNG state with exact-match validation, single-use, absent-rejection. CSRF attack blocked.

Node 7 — Arbiter Selector: Route to Node 8 RFC arbiter (T-rfc-oauth2). Positive compliance.

Node 8 — Arbiter Units: PASS. RFC 6749 §10.12 MUST satisfied. 256-bit CSPRNG, exact-match, single-use, absent-rejected. CWE-352 mitigated. No findings.

Node 9 — Formatter: PASS. RFC 6749 §10.12 CSRF protection satisfied. 256-bit CSPRNG state: unpredictable; exact-match validated at callback; single-use; absent-rejected. CWE-352 CSRF against OAuth callback blocked.

### Reason
9-node trace confirms all state validation requirements: CSPRNG generation, session storage, exact-match, single-use, absent-rejected. RFC 6749 §10.12 CSRF protection satisfied. PASS.

### Phrasing-B
PASS — 9-node trace: OAuth state 256-bit CSPRNG + exact-match + single-use + absent-rejected. RFC 6749 §10.12 satisfied.

N1: T-rfc-oauth2 + CWE-352 / state validation; all steps present.
N2: RFC 6749 §10.12 MUST; 256-bit CSPRNG; session storage; exact-match validation.
N3: Verify CSPRNG; session; exact-match; single-use; absent-rejected.
N4: All controls confirmed; CSRF attack blocked (attacker cannot predict 256-bit state).
N5: All requirements met. PASS.
N6: PASS — CSRF protection complete.
N7: RFC arbiter T-rfc-oauth2; positive.
N8: PASS — RFC 6749 §10.12 satisfied; CWE-352 mitigated.
N9: PASS. All state validation controls confirmed.

---

## Pair[CB4-3.4-891]

### A
Full pipeline trace — OAuth access tokens stored in browser localStorage:

A SPA stores access tokens in localStorage after token exchange. Team argues 15-minute TTL limits theft impact and API validation on every request provides protection.

### B
Node 1 — Domain Splitter: Input domain: T-asvs-token / T-oauth-oidc. Sub-domain: client-side token storage location — localStorage vs memory. Security concern: XSS-based token exfiltration from JavaScript-accessible storage. Pipeline route: Node 8 ASVS arbiter with V9.3.1 analysis.

Node 2 — Domain Translator: ASVS V9.3.1: guidance on token transport and storage security. localStorage is a JavaScript-accessible browser storage API — any JavaScript running in the page's origin can read it via window.localStorage.getItem(). An XSS attack exfiltrates all stored tokens. RFC 6750 §2.3 (query parameters) and the general principle of minimum-surface token storage inform the analysis. No RFC mandates a specific client storage mechanism with a MUST; ASVS V9.3.1 is guidance-level.

Node 3 — Scheme Planner: Assess localStorage's JavaScript accessibility. Evaluate TTL argument: does 15-min TTL eliminate the XSS exfiltration risk? Evaluate "API validation" argument: is RS-side validation a client-storage substitute? Determine normative level. Expected: CONCERN if SHOULD-level; REPROCESS only if MUST violation identified.

Node 4 — Domain Processor: localStorage: accessible to any JavaScript in the page origin — full XSS exfiltration surface. 15-minute TTL argument: XSS can re-exfiltrate continuously (every 10 minutes) — attacker holds current valid tokens indefinitely as long as XSS persists. TTL only limits the impact of a one-time exfiltration; persistent XSS defeats TTL. "API validation" argument: RS-side token validation (sig/iss/aud/exp) is orthogonal to client storage — it verifies token legitimacy, not the security of how the client stored it. No MUST violation: ASVS V9.3.1 is SHOULD-level guidance; no RFC specifies a MUST excluding localStorage. SHOULD-level gap.

Node 5 — Applier: ASVS V9.3.1 SHOULD guidance violated. No MUST identified. XSS exfiltration risk present. CONCERN.

Node 6 — Result Translator: CONCERN. localStorage creates XSS-based token exfiltration surface. 15-minute TTL partially mitigates single-point exfiltration but not persistent XSS. No MUST violation. CONCERN.

Node 7 — Arbiter Selector: Route to Node 8 ASVS arbiter (T-asvs-token). SHOULD-level gap. No SSDF PO.1 escalation triggered (no MUST violation traceable).

Node 8 — Arbiter Units: CONCERN. ASVS V9.3.1 storage guidance: localStorage creates persistent XSS exfiltration surface. TTL argument partially accepted (limits single-theft window) but does not address persistent XSS re-exfiltration. RS validation argument: orthogonal — does not address client-side storage. No MUST violation. CONCERN.

Node 9 — Formatter: CONCERN. Access tokens in localStorage: accessible to any JavaScript in page origin; XSS continuously re-exfiltrates tokens every TTL interval. ASVS V9.3.1 guidance: use in-memory storage (module-level variable) for access tokens; store refresh token in HttpOnly Secure SameSite=Strict cookie instead. No MUST violation — CONCERN not REPROCESS.

### Reason
9-node trace correctly identifies localStorage as a SHOULD-level storage concern (ASVS V9.3.1), evaluates TTL as partial-only mitigation (persistent XSS defeats it), rejects the RS-validation argument as orthogonal, and produces CONCERN. No MUST identified.

### Phrasing-B
CONCERN — 9-node trace: access tokens in localStorage, ASVS V9.3.1 SHOULD, XSS re-exfiltration risk.

N1: T-asvs-token / client-side token storage; localStorage XSS surface.
N2: ASVS V9.3.1 SHOULD guidance; no RFC MUST excluding localStorage; XSS exfiltration surface.
N3: Assess localStorage JS-accessibility; evaluate TTL + RS-validation arguments; determine normative level.
N4: localStorage = full XSS surface; persistent XSS defeats TTL; RS validation orthogonal; no MUST violation.
N5: SHOULD guidance violated; no MUST. CONCERN.
N6: CONCERN — XSS exfiltration surface; TTL partially mitigates; RS validation not a storage substitute.
N7: ASVS arbiter T-asvs-token; SHOULD-level; no PO.1 escalation.
N8: CONCERN — ASVS V9.3.1 guidance; localStorage; no MUST.
N9: CONCERN. Use module-level memory + HttpOnly cookie for refresh token.

---

## Pair[CB4-3.4-892]

### A
Full pipeline trace — OAuth access tokens in module-level memory; refresh token in HttpOnly Secure cookie:

A SPA stores access tokens in a JavaScript module-level variable (cleared after use/TTL). Refresh tokens are in an HttpOnly Secure SameSite=Strict cookie (JavaScript-inaccessible).

### B
Node 1 — Domain Splitter: Input domain: T-asvs-token / T-oauth-oidc. Sub-domain: client-side token storage — module memory + HttpOnly cookie pattern. Pipeline route: Node 8 ASVS arbiter with positive compliance confirmation.

Node 2 — Domain Translator: ASVS V9.3.1: in-memory storage minimizes JavaScript-accessible exfiltration surface. HttpOnly cookie: inaccessible to JavaScript (document.cookie does not expose HttpOnly cookies). Secure flag: cookie only transmitted over HTTPS. SameSite=Strict: prevents cross-site requests from including the cookie (CSRF protection for refresh token use).

Node 3 — Scheme Planner: Verify module-level storage (not localStorage/sessionStorage). Verify HttpOnly + Secure + SameSite=Strict on refresh cookie. Verify access token cleared after use. Expected: PASS if all controls confirmed.

Node 4 — Domain Processor: Module-level variable: not in localStorage or sessionStorage — no storage-API exfiltration. XSS can still access the variable during the active window, but cannot silently accumulate tokens from persistent storage. Cleared after use/TTL: no long-term accumulation. HttpOnly: JavaScript cannot read the cookie — no XSS exfiltration of refresh token. Secure: HTTPS only. SameSite=Strict: CSRF using the refresh cookie blocked. All controls confirmed.

Node 5 — Applier: ASVS V9.3.1 guidance satisfied. Correct SPA token storage pattern. PASS.

Node 6 — Result Translator: PASS. Module-level memory for access tokens; HttpOnly Secure SameSite=Strict cookie for refresh tokens. Correct pattern.

Node 7 — Arbiter Selector: Route to Node 8 ASVS arbiter (T-asvs-token). Positive compliance.

Node 8 — Arbiter Units: PASS. ASVS V9.3.1 guidance satisfied. Access tokens: module-level memory, cleared post-use. Refresh token: HttpOnly (JavaScript-inaccessible), Secure, SameSite=Strict. No findings.

Node 9 — Formatter: PASS. Correct SPA OAuth token storage pattern. Access token in module-level variable — minimal XSS surface; cleared post-use. Refresh token in HttpOnly Secure SameSite=Strict cookie — JavaScript-inaccessible; CSRF-protected. ASVS V9.3.1 guidance satisfied.

### Reason
9-node trace confirms module-level memory for access tokens and HttpOnly Secure SameSite=Strict cookie for refresh tokens. ASVS V9.3.1 satisfied. Correct SPA pattern. PASS.

### Phrasing-B
PASS — 9-node trace: access tokens in module-level memory; refresh token in HttpOnly Secure SameSite=Strict cookie. ASVS V9.3.1 satisfied.

N1: T-asvs-token / correct SPA storage pattern.
N2: ASVS V9.3.1; module memory not in storage APIs; HttpOnly = JS-inaccessible; Secure; SameSite=Strict.
N3: Verify module-level (not localStorage); verify HttpOnly+Secure+SameSite; verify cleared post-use.
N4: All controls confirmed; no storage-API exfiltration surface; refresh token JS-inaccessible.
N5: ASVS V9.3.1 satisfied. PASS.
N6: PASS — correct SPA token storage pattern.
N7: ASVS arbiter T-asvs-token; positive.
N8: PASS — all controls confirmed; no findings.
N9: PASS. Module memory + HttpOnly cookie = correct SPA OAuth pattern.

---

## Pair[CB4-3.4-893]

### A
Full pipeline trace — OIDC acr claim not validated despite acr_values request:

An OIDC RP sends acr_values=2 in the authorization request. The AS returns an ID token with acr=1 (lower than requested). The RP validates signature, iss, aud, exp, and nonce but does not check the acr claim value, proceeding as if acr=2 was satisfied.

### B
Node 1 — Domain Splitter: Input domain: T-rfc-oidc / T-asvs-authentication. Sub-domain: OIDC acr (authentication context class reference) claim validation when acr_values was requested. Pipeline route: Node 8 OIDC arbiter with OIDC Core §3.1.3.7.9 analysis.

Node 2 — Domain Translator: OIDC Core §3.1.3.7.9: "If the acr Claim was requested, the Client SHOULD verify that the assurance level used corresponds to the level requested." SHOULD-level. acr_values=2 in authorization request is a RP security requirement signaling that MFA is required. Without acr validation, the AS can silently downgrade authentication assurance without RP detection.

Node 3 — Scheme Planner: Verify acr_values was requested. Verify acr in ID token is below requested level. Verify RP did not check acr. Determine normative level. Expected: CONCERN if SHOULD-level; REPROCESS only if MUST identified.

Node 4 — Domain Processor: acr_values=2 requested by RP. ID token acr=1 (password-only returned when MFA required). RP validates: sig (✓), iss (✓), aud (✓), exp (✓), nonce (✓). RP validates acr: NOT CHECKED. RP proceeds with acr=2 assumption. Impact: user authenticated at acr=1 gains access to acr=2-gated resource. MFA requirement is non-functional. Normative level: OIDC Core §3.1.3.7.9 SHOULD — not MUST. No unconditional MUST requiring acr validation at RP. SHOULD-level gap.

Node 5 — Applier: OIDC Core §3.1.3.7.9 SHOULD gap. MFA requirement silently bypassed. CONCERN.

Node 6 — Result Translator: CONCERN. acr_values request non-functional without acr validation. AS assurance downgrade undetected. SHOULD-level: CONCERN.

Node 7 — Arbiter Selector: Route to Node 8 OIDC arbiter (T-rfc-oidc). SHOULD-level gap. No PO.1 escalation (no MUST traceable).

Node 8 — Arbiter Units: CONCERN. OIDC Core §3.1.3.7.9 SHOULD not implemented. acr_values=2 request is non-functional — AS returns acr=1; RP does not detect. MFA assurance level silently bypassed. No MUST violation identified. CONCERN.

Node 9 — Formatter: CONCERN. OIDC acr claim not validated despite acr_values=2 request. OIDC Core §3.1.3.7.9 SHOULD: RP should verify acr meets or exceeds requested level. AS assurance downgrade (MFA → password-only) undetected by RP. Remediation: validate acr claim in ID token; require acr ≥ requested level; reject and force re-auth if insufficient.

### Reason
9-node trace correctly identifies OIDC Core §3.1.3.7.9 SHOULD level, documents the assurance downgrade scenario, and produces CONCERN. Not MUST — CONCERN not REPROCESS.

### Phrasing-B
CONCERN — 9-node trace: OIDC acr not validated, OIDC Core §3.1.3.7.9 SHOULD, MFA downgrade undetected.

N1: T-rfc-oidc / acr claim validation; acr_values requested but acr not checked.
N2: OIDC Core §3.1.3.7.9 SHOULD; acr_values=2 = RP MFA security requirement.
N3: Verify acr_values requested; verify acr=1 < requested; verify RP did not check; normative level.
N4: acr_values=2 sent; acr=1 returned; RP does not validate acr; MFA bypass undetected; SHOULD-level.
N5: SHOULD gap; MFA silently bypassed. CONCERN.
N6: CONCERN — acr_values non-functional; §3.1.3.7.9 SHOULD.
N7: OIDC arbiter T-rfc-oidc; SHOULD-level; no PO.1.
N8: CONCERN — §3.1.3.7.9 SHOULD; no MUST.
N9: CONCERN. Validate acr ≥ requested level; reject + re-auth on insufficient assurance.

---

## Pair[CB4-3.4-894]

### A
Full pipeline trace — OIDC acr claim validated; insufficient assurance rejected and re-auth forced:

An OIDC RP sends acr_values=2. On receiving the ID token, the RP validates that the acr claim is present and meets or exceeds level 2. If acr is absent or acr < 2, the RP rejects the authentication and forces a new authorization request with prompt=login.

### B
Node 1 — Domain Splitter: Input domain: T-rfc-oidc / T-asvs-authentication. Sub-domain: OIDC acr validation with correct reject-and-re-auth behavior. Pipeline route: Node 8 OIDC arbiter with positive compliance confirmation.

Node 2 — Domain Translator: OIDC Core §3.1.3.7.9 SHOULD. RP validates acr ≥ requested level. Insufficient acr triggers re-auth with prompt=login — ensures the AS authenticates the user at the required assurance level rather than silently accepting a downgrade.

Node 3 — Scheme Planner: Verify acr claim presence check. Verify level comparison (acr ≥ 2). Verify reject behavior. Verify re-auth with prompt=login. Expected: PASS.

Node 4 — Domain Processor: acr present: checked. acr ≥ 2: validated. If acr < 2 or absent: authentication rejected. Re-auth: new authorization request with prompt=login forces the AS to re-authenticate the user at the required assurance level. MFA assurance requirement is a functional security control. acr_values=2 request + acr validation = end-to-end MFA enforcement. All controls confirmed.

Node 5 — Applier: OIDC Core §3.1.3.7.9 SHOULD satisfied above-SHOULD level. PASS.

Node 6 — Result Translator: PASS. acr validation makes acr_values request a functional security control. Insufficient assurance rejected and re-auth forced. SHOULD guidance implemented correctly.

Node 7 — Arbiter Selector: Route to Node 8 OIDC arbiter (T-rfc-oidc). Positive compliance.

Node 8 — Arbiter Units: PASS. OIDC Core §3.1.3.7.9 SHOULD satisfied. acr claim validated: presence + level ≥ 2. Insufficient acr: rejected + prompt=login re-auth. MFA assurance requirement functional. No findings.

Node 9 — Formatter: PASS. OIDC acr claim validated against acr_values=2 request. acr ≥ 2 enforced; absent or insufficient acr triggers prompt=login re-auth. MFA assurance requirement is functional end-to-end. OIDC Core §3.1.3.7.9 SHOULD implemented above the SHOULD floor.

### Reason
9-node trace confirms acr validation with level check and reject-plus-re-auth behavior. OIDC Core §3.1.3.7.9 SHOULD satisfied. acr_values request is a functional security control. PASS.

### Phrasing-B
PASS — 9-node trace: OIDC acr validated with level check + reject-re-auth. §3.1.3.7.9 SHOULD satisfied.

N1: T-rfc-oidc / acr validation; re-auth enforcement pattern.
N2: §3.1.3.7.9 SHOULD; acr ≥ requested level; prompt=login re-auth on failure.
N3: Verify acr present; level ≥ 2; reject behavior; re-auth mechanism.
N4: All controls confirmed; MFA assurance is functional end-to-end.
N5: SHOULD satisfied. PASS.
N6: PASS — acr_values functional; assurance downgrade rejected.
N7: OIDC arbiter; positive.
N8: PASS — §3.1.3.7.9 satisfied above SHOULD floor.
N9: PASS. acr ≥ requested; absent/insufficient → prompt=login re-auth.

---

## Pair[CB4-3.4-895]

### A
Full pipeline trace — UserInfo endpoint returns all registered user claims regardless of access token scope:

An OIDC AS's UserInfo endpoint returns sub, name, email, phone_number, and address to any valid access token — regardless of whether the presented scope includes profile, email, phone, or address. A token with scope=openid receives the same full response as scope=openid profile email phone address.

### B
Node 1 — Domain Splitter: Input domain: T-rfc-oidc / T-asvs-authorization. Sub-domain: UserInfo endpoint scope-based claim filtering — scope ignored during claim response construction. Pipeline route: Node 8 OIDC arbiter with OIDC Core §5.3.3 MUST analysis.

Node 2 — Domain Translator: OIDC Core §5.3.3: "The Claims returned from the UserInfo Endpoint MUST be those associated with the presented scopes." MUST-level. Scope-to-claim mapping defined in OIDC Core §5.4: openid → sub; profile → name/family_name/given_name/nickname/preferred_username/profile/picture/website/gender/birthdate/zoneinfo/locale/updated_at; email → email/email_verified; phone → phone_number/phone_verified; address → address. Returning claims beyond authorized scopes violates MUST and the user's consent (user consented to specific scopes during authorization).

Node 3 — Scheme Planner: Verify AS parses scope from access token. Verify claim filtering applied. Verify user without scope=profile cannot receive name/birthdate. Evaluate "authenticated = entitled" argument. Expected: REPROCESS if MUST §5.3.3 violated.

Node 4 — Domain Processor: Scope in access token: scope=openid. Claims returned: sub + name + email + phone_number + address. Expected for scope=openid per §5.4: sub only. Actual returns claims from profile (name), email (email), phone (phone_number), address (address) — all four unauthorized scopes disclosed. OIDC Core §5.3.3 MUST: violated.

"Authenticated = entitled" argument: authentication proves token legitimacy (signature, iss, aud, exp). It does not prove claim entitlement. Scope is the authorization dimension — user explicitly authorized disclosure of specific claim categories when granting specific scopes. Returning claims for scopes the user never granted violates their consent and bypasses the scope authorization model.

Privacy impact: user authorized scope=openid (sub disclosure only). The AS disclosed name, email, phone, and home address without the user's consent. This is a GDPR-relevant privacy violation in addition to an OIDC specification violation.

Node 5 — Applier: OIDC Core §5.3.3 MUST scope-filtered claims violated. User consent violated. Privacy impact. REPROCESS.

Node 6 — Result Translator: REPROCESS. UserInfo returns all claims regardless of scope. OIDC Core §5.3.3 MUST violated. Scope=openid client receives full PII profile without user consent for those claims.

Node 7 — Arbiter Selector: Route to Node 8 OIDC arbiter (T-rfc-oidc) + T-asvs-authorization. Both confirm REPROCESS. MUST violation + privacy impact.

Node 8 — Arbiter Units: REPROCESS. OIDC Core §5.3.3 MUST: UserInfo MUST return only scope-authorized claims. scope=openid entitles only sub; returning name/email/phone/address is unauthorized per §5.4 mapping. "Authenticated = entitled" rejected: authentication ≠ claim entitlement. User consent violation. REPROCESS.

Node 9 — Formatter: REPROCESS. UserInfo returns all claims regardless of scope. OIDC Core §5.3.3 MUST: violated. scope=openid → sub only per §5.4. Returning name (profile scope), email (email scope), phone_number (phone scope), address (address scope) without those scopes in the token = unauthorized claim disclosure. User privacy consent violated. Remediation: parse access token scope claim; apply §5.4 scope-to-claim mapping; return only authorized claims.

### Reason
9-node trace correctly identifies OIDC Core §5.3.3 MUST scope-filtered claims, documents the scope=openid receiving full profile violation, rejects the "authenticated = entitled" argument, and documents privacy consent impact. REPROCESS.

### Phrasing-B
REPROCESS — 9-node trace: UserInfo returns all claims regardless of scope, OIDC Core §5.3.3 MUST violated.

N1: T-rfc-oidc / UserInfo scope filtering; all claims returned regardless of scope.
N2: §5.3.3 MUST scope-filtered; §5.4 scope-to-claim mapping: openid → sub only.
N3: Verify scope parsing; verify §5.4 filtering; evaluate "authenticated = entitled" argument.
N4: scope=openid returns sub + name + email + phone + address; §5.3.3 MUST violated; consent violated.
N5: MUST violated; privacy impact. REPROCESS.
N6: REPROCESS — §5.3.3 MUST; scope=openid discloses PII beyond consent.
N7: OIDC + T-asvs-authorization; both REPROCESS.
N8: REPROCESS — §5.3.3 MUST violated; "authenticated = entitled" rejected.
N9: REPROCESS. Parse scope; apply §5.4 mapping; return only authorized claims.

---

## Pair[CB4-3.4-896]

### A
Full pipeline trace — UserInfo endpoint returns only scope-authorized claims per §5.4 mapping:

The OIDC UserInfo endpoint parses the access token's scope claim and returns only claims authorized by the presented scopes per OIDC Core §5.4 mapping. scope=openid returns sub only; scope=openid profile email returns sub + name + email + email_verified.

### B
Node 1 — Domain Splitter: Input domain: T-rfc-oidc / T-asvs-authorization. Sub-domain: UserInfo endpoint correct scope-filtered claim response. Pipeline route: Node 8 OIDC arbiter with positive compliance confirmation.

Node 2 — Domain Translator: OIDC Core §5.3.3 MUST scope-filtered claims. §5.4 scope-to-claim mapping: openid → sub; profile → name/family_name/given_name/etc.; email → email/email_verified; phone → phone_number/phone_verified; address → address. UserInfo must correctly parse scope, apply mapping, and return intersection with registered user claims.

Node 3 — Scheme Planner: Verify scope is parsed from access token. Verify §5.4 mapping is applied. Verify scope=openid → sub only. Verify scope=openid profile email → sub + profile claims + email claims. Expected: PASS if all controls confirmed.

Node 4 — Domain Processor: Scope parsing: space-delimited scope from access token parsed correctly. §5.4 mapping applied: openid → sub; profile → name/family_name/etc.; email → email/email_verified. Scope=openid: returns sub only — email, phone, address, name not returned (those scopes not granted). Scope=openid profile email: returns sub + name (and other profile claims) + email/email_verified. No over-disclosure. User consent enforced at claim delivery layer. All controls confirmed.

Node 5 — Applier: OIDC Core §5.3.3 MUST satisfied. Correct §5.4 scope-to-claim mapping applied. PASS.

Node 6 — Result Translator: PASS. UserInfo returns only scope-authorized claims. §5.3.3 MUST satisfied. User consent enforced. Privacy maintained.

Node 7 — Arbiter Selector: Route to Node 8 OIDC arbiter (T-rfc-oidc). Positive compliance.

Node 8 — Arbiter Units: PASS. OIDC Core §5.3.3 MUST satisfied. §5.4 scope-to-claim mapping correctly applied. Scope=openid → sub only. No over-disclosure. User consent enforced at UserInfo layer. No findings.

Node 9 — Formatter: PASS. UserInfo correctly returns scope-filtered claims per OIDC Core §5.3.3 MUST and §5.4 mapping. scope=openid → sub only. scope=openid profile email → sub + profile claims + email/email_verified. Consent enforced: users receive only claims they authorized through scope selection.

### Reason
9-node trace confirms scope parsing, §5.4 mapping application, scope=openid → sub only verification, and correct multi-scope response construction. OIDC Core §5.3.3 MUST satisfied. PASS.

### Phrasing-B
PASS — 9-node trace: UserInfo returns scope-filtered claims, §5.3.3 MUST satisfied, §5.4 mapping applied.

N1: T-rfc-oidc / UserInfo correct scope filtering; compliance confirmation.
N2: §5.3.3 MUST; §5.4 scope-to-claim mapping per scope category.
N3: Verify scope parsing; §5.4 mapping; openid → sub only; multi-scope correct.
N4: All confirmed: scope parsed; §5.4 applied; openid → sub; no over-disclosure.
N5: §5.3.3 MUST satisfied. PASS.
N6: PASS — §5.3.3 MUST; consent enforced.
N7: OIDC arbiter; positive.
N8: PASS — §5.3.3 MUST satisfied; no findings.
N9: PASS. Scope-filtered per §5.4; consent enforced at delivery layer.
