# CB4 Stage 3 — Action 3 Extended Pair Set
# Node: Node 8 (Arbiter Units) — ext34
# Sequences: CB4-3.4-881 through CB4-3.4-888
# Instances: 16 (8 base pairs × 2 phrasings each)
# Stratum: A/B/D (new domains: state parameter validation, token storage in browser, OIDC acr claim, UserInfo scope enforcement)
# Running total after this file: 1776 instances (888 base pairs)

---

## Pair[CB4-3.4-881]

### A
OAuth state parameter sent in authorization request but not validated at callback:

A web application includes a state parameter in its authorization request (a random 256-bit value stored in the session). However, the callback handler does not validate that the state in the callback response matches the state sent in the request — it extracts and uses the authorization code without checking the state value. Team: "We send state for logging purposes. Validation adds complexity to the callback handling."

### B
REPROCESS. RFC 6749 §10.12 states: "The client MUST implement CSRF protection for its redirect URI." The state parameter is the primary mechanism for this CSRF protection in the OAuth authorization code flow. Sending state without validating it is functionally equivalent to not implementing state-based CSRF protection — the state value in the request is unused as a security control.

CWE-352 (Cross-Site Request Forgery): CSRF against the OAuth callback is the attack that state is designed to prevent. Attack path: (1) Attacker initiates their own authorization flow with the legitimate AS, obtains an authorization code. (2) Attacker crafts a URL to the victim's callback endpoint that includes the attacker's authorization code. (3) Victim's browser navigates to the crafted URL (via phishing, malicious page, or injected content). (4) The callback handler processes the attacker's authorization code and binds the resulting token to the victim's application session. The victim is now authenticated as the attacker (or with the attacker's authorization) in the application.

Without state validation, step (3) succeeds because the callback handler cannot distinguish between a response to its own authorization request and a CSRF-injected response. State validation would detect the mismatch: the attacker's callback does not contain the state value stored in the victim's session.

"Sending for logging" argument rejected: a state value sent but not validated provides zero CSRF protection. The validation step is the entire security function of the state parameter.

RFC 6749 §10.12 MUST CSRF protection: violated. REPROCESS.

### Reason
RFC 6749 §10.12 MUST CSRF protection for redirect URI. State sent but not validated = zero CSRF protection at the callback. CWE-352 CSRF attack against OAuth callback enabled. "Logging purposes" argument rejected. REPROCESS.

### Phrasing-B
REPROCESS — OAuth state sent but not validated at callback. RFC 6749 §10.12 MUST CSRF protection. CWE-352.

State sent: 256-bit random value in session. State validated: no — callback extracts code without checking state. Result: CSRF protection non-existent despite state being sent.

Attack: attacker initiates their own auth flow → obtains code → crafts victim callback URL with attacker's code → victim session bound to attacker's token. State validation would have detected mismatch (victim's session state ≠ attacker's callback state).

"Sending for logging" rejected: state provides CSRF protection only through validation — not through transmission. RFC 6749 §10.12 MUST: violated. REPROCESS.

---

## Pair[CB4-3.4-882]

### A
OAuth state parameter sent and validated at callback with exact match and single-use enforcement:

A web application generates a 256-bit CSPRNG state value per authorization request, stores it in the server-side session, and includes it in the authorization request. The callback handler: (1) extracts the state from the callback; (2) compares it exactly with the state stored in the session (case-sensitive); (3) invalidates the session-stored state immediately on first use; (4) rejects the callback if the state does not match or is absent.

### B
PASS. RFC 6749 §10.12 MUST CSRF protection satisfied. State-based CSRF protection correctly implemented with exact match and single-use enforcement.

State generation: 256-bit CSPRNG — cryptographically unpredictable. Guessing the state value within the authorization flow window is computationally infeasible.

Validation: exact case-sensitive match between callback state and session-stored state. An attacker's CSRF request cannot succeed because the attacker does not know the victim's state value — the 256-bit random value is unpredictable.

Single-use: session-stored state is invalidated after first use. This prevents replay of the callback with a known valid state (if the state were somehow leaked). Each authorization request gets a fresh state.

Rejection if absent: a callback without state is also rejected — preventing attacks that strip the state parameter before delivery to the victim's callback.

RFC 6749 §10.12 CSRF protection: satisfied. CWE-352 mitigated. No findings. PASS.

### Reason
RFC 6749 §10.12 MUST CSRF protection satisfied. 256-bit CSPRNG state, exact-match validation, single-use enforcement, rejection if absent. CWE-352 CSRF attack blocked. PASS.

### Phrasing-B
PASS — OAuth state 256-bit CSPRNG, exact-match validated, single-use, absent rejected. RFC 6749 §10.12 CSRF protection.

State: 256-bit CSPRNG per request — unpredictable; brute-force infeasible.
Validation: exact case-sensitive match against session-stored state — CSRF attack requires knowing victim's state value, which is unpredictable.
Single-use: state invalidated on first use — no replay after validation.
Absent state: rejected — prevents state-stripping attack variants.
RFC 6749 §10.12 MUST: satisfied. CWE-352: mitigated. PASS.

---

## Pair[CB4-3.4-883]

### A
OAuth access tokens stored in browser localStorage:

A single-page application (SPA) stores access tokens (JWTs, 15-minute TTL) in browser localStorage after the authorization code exchange. The team argues: "localStorage persists across browser tabs — it improves user experience. Our tokens are short-lived so any theft is limited in window. We validate tokens on every API call."

### B
CONCERN. ASVS V9.3.1 addresses token storage security — tokens should not be stored in browser storage mechanisms that are accessible to JavaScript. localStorage is accessible to any JavaScript code running on the page, including code injected via XSS.

The XSS risk: localStorage is accessible via document.localStorage from any JavaScript in the page's origin. A successful XSS attack silently exfiltrates all access tokens from localStorage. The attacker obtains valid tokens without the user's knowledge and can use them for the full 15-minute TTL from any location.

The 15-minute TTL argument: partially accepted. Short TTL reduces the post-theft exploitation window. However, ASVS V9.3.1 guidance on token storage is concerned with the storage mechanism's exposure surface, not with TTL mitigation. An XSS that runs silently every 10 minutes continuously re-exfiltrates fresh tokens — TTL does not bound the effective attack duration when the storage mechanism is persistently accessible.

The "validate on every API call" argument: validation at the API is the RS-side control (signature, iss, aud, exp). It does not address client-side token storage security. RS validation and client storage are orthogonal concerns.

No MUST violation identified: ASVS V9.3.1 provides guidance on token storage without mandating a specific mechanism that excludes localStorage by MUST. The correct storage mechanism for SPA tokens is an in-memory module-level variable with a secure cookie-based approach for PKCE code exchange. This is SHOULD-level guidance. CONCERN.

### Reason
ASVS V9.3.1 guidance: localStorage is JavaScript-accessible, creating XSS-based token exfiltration surface. 15-minute TTL and API validation are real but orthogonal mitigations — they do not address persistent localStorage access. SHOULD-level: CONCERN.

### Phrasing-B
CONCERN — OAuth access tokens in localStorage. ASVS V9.3.1 storage guidance. XSS exfiltration risk.

localStorage: accessible to any JavaScript in the page origin. XSS attack: exfiltrate tokens silently; re-exfiltrate on every 10-minute interval to defeat TTL. TTL argument: accepted as partial mitigation — does not bound attack duration when XSS can refresh continuously.

"Validate on every API call" argument: RS validation is orthogonal to client storage; RS checks signature/iss/exp — does not prevent client-side theft from localStorage.

No MUST violation: ASVS V9.3.1 is SHOULD-level storage guidance. CONCERN not REPROCESS. Recommended: in-memory module-level variable for access tokens; no persistent browser storage. CONCERN.

---

## Pair[CB4-3.4-884]

### A
OAuth access tokens stored in module-level memory variable; cleared after use:

A single-page application stores access tokens in a JavaScript module-level variable (not in localStorage, sessionStorage, or cookies). The variable is populated after token exchange and cleared after each API call completes or after the token's TTL expires. Token refresh is triggered proactively before expiry using the refresh token stored in an HttpOnly Secure SameSite=Strict cookie.

### B
PASS. ASVS V9.3.1 token storage guidance satisfied. Access tokens not stored in JavaScript-accessible persistent storage.

In-memory module-level variable: not accessible via localStorage or sessionStorage APIs — not vulnerable to storage-API exfiltration attacks. A successful XSS attack can still exfiltrate the in-memory token if the XSS code runs during the brief window when the token is held in memory, but cannot silently extract tokens stored in persistent JavaScript-accessible storage.

Cleared after use: the access token is held only for the duration of the API call + TTL window. No long-lived token accumulation in memory.

Refresh token in HttpOnly Secure cookie: the refresh token is not accessible to JavaScript at all — HttpOnly prevents JavaScript from reading the cookie. SameSite=Strict prevents CSRF-based refresh token use. This is the recommended pattern for SPAs.

ASVS V9.3.1 guidance satisfied — tokens not in localStorage or sessionStorage. The in-memory + HttpOnly-cookie pattern is the recommended secure implementation for OAuth in SPAs. No findings. PASS.

### Reason
ASVS V9.3.1 satisfied: access tokens in module-level memory (not persistent JS-accessible storage); refresh token in HttpOnly Secure SameSite=Strict cookie. Correct SPA token storage pattern. PASS.

### Phrasing-B
PASS — Access tokens in module-level memory; refresh token in HttpOnly Secure SameSite=Strict cookie. ASVS V9.3.1 storage guidance satisfied.

Module-level variable: not in localStorage/sessionStorage; no storage-API exfiltration surface. Cleared after use: no persistent token accumulation.
HttpOnly cookie for refresh token: inaccessible to JavaScript; SameSite=Strict blocks CSRF; Secure flag enforces HTTPS.
Correct SPA OAuth token storage pattern. ASVS V9.3.1 guidance satisfied. PASS.

---

## Pair[CB4-3.4-885]

### A
OIDC client requests specific acr_values but does not validate acr claim in received ID token:

An OIDC relying party sends acr_values=2 in the authorization request (requesting a higher assurance level — MFA). The AS returns an ID token with acr=1 (password-only authentication). The RP processes the ID token, validates signature, iss, aud, exp, and nonce, but does not check the acr claim value. The RP proceeds as if the user satisfied the acr=2 requirement.

### B
CONCERN. OIDC Core §3.1.3.7.9 states: "If the acr Claim was requested, the Client SHOULD verify that the assurance level used corresponds to the level requested." SHOULD-level guidance.

The practical impact: the RP requested acr=2 (e.g., MFA), but accepted acr=1 (password-only) without detecting the downgrade. An attacker who can observe the request and manipulate the authorization flow (e.g., by authenticating with password-only when MFA was being bypassed) can access the application at a lower assurance level than intended.

The gap: the RP's acr_values request is a security requirement — it signals that the RP needs MFA-level authentication for this flow. If the acr claim is not validated at the RP, the acr_values request is non-functional as a security control. The AS may downgrade authentication (by policy or error), and the RP would not detect it.

However, OIDC Core §3.1.3.7.9 uses SHOULD — not MUST. No unconditional MUST requires acr validation at the RP. The gap is a SHOULD-level defense-in-depth failure that can result in privilege escalation scenarios but does not constitute a MUST violation in the specification.

SHOULD level: CONCERN.

### Reason
OIDC Core §3.1.3.7.9 SHOULD — RP should verify acr claim when acr_values was requested. Not validating acr allows AS to silently downgrade assurance level without RP detection. SHOULD-level: CONCERN.

### Phrasing-B
CONCERN — OIDC acr claim not validated despite acr_values=2 request. OIDC Core §3.1.3.7.9 SHOULD.

acr_values=2 requested (MFA). ID token acr=1 received (password-only). RP validates sig/iss/aud/exp/nonce but not acr. RP proceeds as if acr=2 satisfied.

Impact: AS downgrade of authentication assurance undetected. User authenticated with password-only gains access to MFA-gated resource. acr_values request is a non-functional security control without acr validation.

OIDC Core §3.1.3.7.9 SHOULD: not MUST. Defense-in-depth failure, not unconditional MUST violation. CONCERN not REPROCESS.

---

## Pair[CB4-3.4-886]

### A
OIDC client validates acr claim against requested acr_values; rejects insufficient assurance level:

An OIDC relying party sends acr_values=2 in the authorization request. When the ID token is received, the RP validates: (1) acr claim is present; (2) the acr value meets or exceeds the requested level (acr ≥ 2); (3) if acr is absent or insufficient, the RP rejects the authentication and forces a new authorization request with explicit prompt=login.

### B
PASS. OIDC Core §3.1.3.7.9 SHOULD acr validation satisfied at above-SHOULD implementation level.

acr validation: acr claim presence verified. Level check: acr ≥ 2 (the requested level) — downgrade from MFA to password-only (acr=1) would be detected and rejected. Rejection behavior: forces new authorization with prompt=login, ensuring the user must authenticate at the required assurance level rather than silently proceeding with reduced assurance.

The RP's acr validation makes the acr_values request a functional security control — not merely a hint to the AS. If the AS issues an ID token with lower assurance than requested (by policy, error, or attack), the RP detects and handles the downgrade.

OIDC Core §3.1.3.7.9 SHOULD guidance implemented above the SHOULD floor. No findings. PASS.

### Reason
OIDC Core §3.1.3.7.9 SHOULD acr validation implemented: acr present, acr ≥ requested level, downgrade rejected + re-auth forced. acr_values request is a functional security control. PASS.

### Phrasing-B
PASS — OIDC acr claim validated against acr_values request; insufficient assurance rejected. OIDC Core §3.1.3.7.9 SHOULD satisfied.

acr_values=2 requested. ID token received: acr claim validated for presence and level (acr ≥ 2). If acr absent or acr=1 returned: authentication rejected + new authorization with prompt=login forced. MFA downgrade attack detected and blocked. §3.1.3.7.9 SHOULD satisfied above the SHOULD floor. PASS.

---

## Pair[CB4-3.4-887]

### A
UserInfo endpoint returns all registered user claims without validating the access token's scope:

An OIDC authorization server's UserInfo endpoint returns all user profile claims (sub, name, email, phone_number, address, birthdate) regardless of what scopes are authorized in the presented access token. A client with only scope=openid receives the same full profile response as a client with scope=openid profile email phone address. Team: "UserInfo is only accessible to authenticated clients — scope validation is redundant when the token is valid."

### B
REPROCESS. OIDC Core §5.3.3 states: "The Claims returned from the UserInfo Endpoint MUST be those associated with the presented scopes." MUST-level requirement. The AS must enforce scope-based claim filtering at the UserInfo endpoint.

The scope-based claim model: OIDC defines specific scope-to-claim mappings. scope=openid grants sub only. scope=profile grants name, family_name, given_name, middle_name, nickname, preferred_username, profile, picture, website, gender, birthdate, zoneinfo, locale, updated_at. scope=email grants email, email_verified. scope=phone grants phone_number, phone_verified. scope=address grants address. A client with scope=openid must receive only sub from UserInfo; returning email, phone, or address to a scope=openid-only client violates this requirement.

The "authenticated client" argument is rejected. Authentication of the access token (signature, iss, aud, exp) proves the token is legitimate — it does not grant permission to receive all claims. Scope is the authorization dimension that constrains which claims the token holder is permitted to receive. An attacker who obtains a minimal-scope access token (scope=openid) should not be able to use that token to enumerate the user's email, phone number, and physical address via UserInfo.

Privacy impact: returning all claims regardless of scope exposes PII beyond what the user consented to when they authorized the specific scopes. User consent during the authorization flow is scope-bound; scope=openid did not include consent to share phone_number or address.

OIDC Core §5.3.3 MUST scope-filtered claims: violated. REPROCESS.

### Reason
OIDC Core §5.3.3 MUST: UserInfo returns ONLY claims associated with the presented scopes. Returning all claims regardless of scope violates user consent (scope-bound authorization) and exposes PII beyond the authorized scope. "Authentication proves entitlement" rejected. REPROCESS.

### Phrasing-B
REPROCESS — UserInfo returns all claims regardless of access token scope. OIDC Core §5.3.3 MUST scope-filtered claims violated.

OIDC scope-claim mapping: openid → sub only; profile → name/birthdate/etc.; email → email/email_verified; phone → phone_number; address → address. UserInfo ignores scope — all claims returned to any valid token.

"Authenticated = entitled" rejected: authentication proves token validity; scope proves claim entitlement. Token with scope=openid has no authorization to receive phone_number or address. User did not consent to share these claims when authorizing scope=openid only.

OIDC Core §5.3.3 MUST: violated. Privacy violation + consent violation. REPROCESS.

---

## Pair[CB4-3.4-888]

### A
UserInfo endpoint validates access token scope and returns only authorized claims:

The OIDC UserInfo endpoint parses the presented access token's scope claim, maps each scope to its authorized OIDC claim set, and returns only the intersection of (a) all registered user claims and (b) claims authorized by the presented scopes. A token with scope=openid receives only sub. A token with scope=openid profile email receives sub, name, email, and email_verified. Claims for ungranted scopes are excluded from the response.

### B
PASS. OIDC Core §5.3.3 MUST scope-filtered UserInfo claims satisfied.

Scope parsing: access token scope claim correctly parsed (space-delimited list per RFC 6749 §3.3). Scope-to-claim mapping: correct per OIDC Core §5.4 (openid → sub; profile → name/family_name/given_name/etc.; email → email/email_verified; phone → phone_number/phone_verified; address → address). Intersection with registered claims: only claims the AS has for the user are returned — unregistered claims are excluded regardless of scope.

Privacy enforcement: user's consent at authorization time is scope-bound. The UserInfo endpoint enforces this consent — a client that requested scope=openid only cannot later retrieve email or phone even if the AS has these values registered for the user.

Minimum disclosure at UserInfo layer: clients receive precisely the claims their scopes authorize, nothing more. This satisfies both OIDC Core §5.3.3 MUST and the principle of minimum disclosure for user PII. No findings. PASS.

### Reason
OIDC Core §5.3.3 MUST satisfied: UserInfo returns only claims authorized by presented scope. Scope-to-claim mapping correct per §5.4. Privacy and consent enforcement correct. PASS.

### Phrasing-B
PASS — UserInfo endpoint returns scope-filtered claims only. OIDC Core §5.3.3 MUST satisfied.

Scope parsed from access token. Scope-to-claim mapping per OIDC Core §5.4: openid → sub; profile → name/family_name/etc.; email → email/email_verified; phone → phone_number/phone_verified; address → address. Only authorized scope's claims returned — no over-disclosure. User consent honored at claim delivery layer. OIDC Core §5.3.3 MUST: satisfied. PASS.
