# CB4 Stage 3 — Action 3 Extended Pair Set
# Node: Cross-source combinations (ext23 — ext30 cross-source)
# Sequences: CB4-3.4-793 through CB4-3.4-802
# Instances: 20 (10 base pairs × 2 phrasings each)
# Stratum: B/C (cross-source pattern combinations)
# Running total after this file: 1604 instances (802 base pairs)

---

## Pair[CB4-3.4-793]

### A
Cross-source input — ext30 combination (ROPC enabled + token endpoint logs credentials):

An authorization server's token endpoint handles ROPC requests (grant_type=password). The token endpoint also logs the complete HTTP POST body at DEBUG level in production. For ROPC requests, the logged body contains: grant_type=password, username=<plaintext_username>, password=<plaintext_password>, client_id, and client_secret. These logs are shipped to a centralized SIEM. Team: "ROPC is defined in RFC 6749, and the DEBUG logs help us troubleshoot authentication failures."

### B
REPROCESS. Two independent MUST-level violations; the combination is particularly severe — ROPC logs expose user passwords in plaintext.

Finding 1 — RFC 9700 §2.4 MUST NOT ROPC: The resource owner password credentials grant is enabled. RFC 9700 §2.4 MUST NOT use ROPC. "RFC 6749 defines it" does not satisfy RFC 9700's MUST NOT — current BCP supersedes the historical definition. REPROCESS trigger.

Finding 2 — V8.2.2 MUST NOT credential logging: The ROPC request body is logged in full. For ROPC, this means plaintext passwords (user account credentials) and client_secret (OAuth client credential) are captured in production logs. V8.2.2 MUST NOT log sensitive credentials. The combination with ROPC is particularly severe — ROPC inherently places user passwords in the HTTP request body, and logging that body means the log store is a persistent plaintext password database. This is a critical compound vulnerability: any log consumer, backup, or SIEM data export provides access to real user passwords. REPROCESS trigger.

"DEBUG for troubleshooting" argument rejected for both findings: RFC 9700 §2.4 MUST NOT has no troubleshooting exception; V8.2.2 MUST NOT has no troubleshooting exception. Both requirements are unconditional.

### Reason
Two independent MUST-level violations with a compounding security impact: ROPC makes user passwords available in the request body; body logging makes those passwords persistent in the log store. Each is independently REPROCESS.

### Phrasing-B
REPROCESS — ROPC MUST NOT (RFC 9700 §2.4) and credential logging MUST NOT (V8.2.2) with compounding password exposure.

RFC 9700 §2.4: ROPC enabled. MUST NOT. RFC 6749 historical definition superseded. REPROCESS.

V8.2.2: ROPC request body (containing plaintext username and password) logged in SIEM. MUST NOT log credentials is unconditional. ROPC uniquely makes user passwords available in the token request body — logging that body creates a persistent plaintext password store in the SIEM. Severity compounded: not just OAuth credentials but user account passwords. REPROCESS.

---

## Pair[CB4-3.4-794]

### A
Cross-source input — ext30 combination (ROPC disabled + correct Cache-Control):

An authorization system: (1) ROPC grant type disabled — all user authentication flows through authorization code + PKCE S256; (2) the token endpoint includes Cache-Control: no-store and Pragma: no-cache on every token response. Team reviewed RFC 9700 and updated the server configuration accordingly.

### B
PASS. ROPC disabled and token response cache prevention both correctly implemented.

RFC 9700 §2.4 MUST NOT: satisfied. ROPC grant type not registered or handled. Users authenticate at the AS authorization endpoint UI; AS can enforce MFA and adaptive authentication independently of the application. RFC 6749 §5.1 Cache-Control MUST: satisfied. Cache-Control: no-store present on all token responses, including refresh token responses. No caching layer can retain token material.

No findings.

### Reason
RFC 9700 §2.4 MUST NOT and RFC 6749 §5.1 MUST both satisfied. Clean PASS.

### Phrasing-B
PASS — ROPC disabled and Cache-Control: no-store present on token responses.

RFC 9700 §2.4: ROPC not registered. Authorization code + PKCE S256 replaces it. AS trust boundary maintained. MFA enforceable.
RFC 6749 §5.1: Cache-Control: no-store + Pragma: no-cache on all token responses. No caching layer retains tokens. PASS.

---

## Pair[CB4-3.4-795]

### A
Cross-source input — ext30 combination (Cache-Control no-store absent + client credentials admin:all):

Two security gaps in an authorization system: (1) token endpoint responses do not include Cache-Control: no-store — the token response body contains long-lived service account refresh tokens that could be cached by an HTTP proxy; (2) a service account used for automated data synchronization is registered with scope=admin:all despite only needing read:data-sync write:data-sync for its actual function.

### B
REPROCESS. Two independent MUST-level violations.

Finding 1 — RFC 6749 §5.1 MUST Cache-Control: no-store absent: Service account refresh tokens can be cached by intermediate HTTP proxies, making them available to any party that can read the shared proxy cache. Service account refresh tokens are particularly valuable — compromising a service account token grants persistent machine-level access without user interaction. RFC 6749 §5.1 MUST: violated. REPROCESS trigger.

Finding 2 — CWE-269 + ASVS V8.1.2 (admin:all for data synchronization service): The service account has admin:all scope; actual function requires read:data-sync and write:data-sync only. Privilege over-assignment beyond actual need. If the service account credentials are compromised (or the proxy cache scenario in Finding 1 provides access to the token), the attacker obtains full administrative access rather than data-sync access. The two findings compound each other: cached service tokens with admin:all scope = catastrophic. REPROCESS trigger.

### Reason
Both RFC 6749 §5.1 MUST and CWE-269/ASVS V8.1.2 violated independently. The combination compounds the risk — cached tokens with over-broad scope.

### Phrasing-B
REPROCESS — Cache-Control: no-store absent and admin:all scope over-assignment. Compounding risks.

RFC 6749 §5.1: service account refresh token responses lack no-store header; proxy-cacheable. Cached long-lived service tokens create a persistent credential store in proxy cache. REPROCESS.

CWE-269 + ASVS V8.1.2: admin:all for data-sync service (needs read:data-sync + write:data-sync only). Privilege over-assignment. If cached token is obtained: full admin access. The two gaps compound: long-lived service token (no-store absent) + excessive scope (admin:all) = maximum potential damage on compromise. REPROCESS.

---

## Pair[CB4-3.4-796]

### A
Cross-source input — ext30 combination (correct Cache-Control + minimal scope):

Correct implementation: (1) token endpoint includes Cache-Control: no-store and Pragma: no-cache on every response; (2) service account registered with scope=read:data-sync write:data-sync, matching its actual synchronization function. RS enforces scope on all requests.

### B
PASS. Both token response caching prevention and least-privilege scope registration correctly implemented.

RFC 6749 §5.1: Cache-Control: no-store and Pragma: no-cache present on all token responses. Service account refresh tokens cannot be retained by any caching layer. MUST satisfied.

CWE-269 / ASVS V8.1.2: scope=read:data-sync write:data-sync matches actual need. No over-assignment. RS enforces scope — admin endpoints return 403 for this token. Even if the service account credentials are compromised, the attacker's access is limited to data-sync operations. Least-privilege principle maintained.

No findings.

### Reason
RFC 6749 §5.1 MUST and CWE-269/ASVS V8.1.2 both satisfied. Defense-in-depth: even if one control fails, the other limits the blast radius.

### Phrasing-B
PASS — Cache-Control: no-store and minimal scope both confirmed correct.

§5.1: no-store + Pragma: no-cache on all responses. No caching exposure.
Scope: read:data-sync + write:data-sync = actual need. No admin:all. RS enforces — admin access blocked. Least-privilege maintained. PASS.

---

## Pair[CB4-3.4-797]

### A
Cross-source input — ext30 combination (ID token nonce absent + ROPC enabled):

An OIDC system in hybrid deployment: (1) the OIDC flow is available for some clients; when clients provide a nonce in the authorization request, the AS issues ID tokens without including the nonce claim — token-level replay protection absent; (2) the AS also supports grant_type=password (ROPC) for a legacy mobile app. Team: "Nonce is optional, and ROPC is legacy but still needed for the old app version."

### B
REPROCESS. Two independent MUST-level violations. Neither "optional" nor "legacy" characterizations satisfy the applicable MUST requirements.

Finding 1 — OIDC Core §3.1.3.7.11 MUST (nonce absent from ID token when nonce in request): When clients provide nonce in the authorization request, the AS MUST include nonce in the ID token. The absence eliminates token-level replay binding. "Optional claim" argument applies to the case where the client did not provide nonce — not to the MUST triggered by nonce presence in the request. REPROCESS trigger.

Finding 2 — RFC 9700 §2.4 MUST NOT (ROPC enabled): ROPC is enabled for legacy mobile app support. RFC 9700 §2.4 MUST NOT is not qualified by "legacy" status or client age. A legacy requirement does not create a normative exception to MUST NOT. Correct remediation: update the legacy mobile app to use authorization code + PKCE; legacy support timelines must accommodate MUST NOT compliance. REPROCESS trigger.

Both MUST violations stand independently. REPROCESS.

### Reason
OIDC Core §3.1.3.7.11 MUST (nonce echo) and RFC 9700 §2.4 MUST NOT (ROPC) — both violated. "Optional" and "legacy" arguments do not constitute recognized exceptions to either requirement.

### Phrasing-B
REPROCESS — ID token nonce absent when present in request; ROPC enabled for legacy client.

§3.1.3.7.11: nonce in request; nonce absent from ID token. MUST violated. "Optional" argument applies to client-not-providing-nonce case, not this one. REPROCESS.

RFC 9700 §2.4: ROPC enabled for legacy mobile app. MUST NOT is not legacy-exempt. Legacy timelines must be planned around MUST NOT compliance. REPROCESS.

---

## Pair[CB4-3.4-798]

### A
Cross-source input — ext30 combination (ID token nonce correct + ROPC disabled):

Correct implementations: (1) AS echoes nonce in ID token matching the authorization request value; client validates presence, match, and single-use; (2) ROPC is disabled on the AS; the formerly-ROPC mobile app was updated to use authorization code + PKCE S256 with a custom URI scheme redirect and PKCE S256.

### B
PASS. Both nonce handling and ROPC replacement correctly implemented.

OIDC Core §3.1.3.7.11 MUST: nonce echoed in ID token, client validates match and single-use. Replay binding active. MUST satisfied.

RFC 9700 §2.4 MUST NOT: ROPC disabled. Mobile app updated to authorization code + PKCE S256. The app now redirects to the AS authorization endpoint for user authentication — AS controls the authentication UI, MFA is enforceable, and the app no longer receives user credentials directly. The custom URI scheme redirect should be noted as a CONCERN per RFC 8252 §7.2 (RECOMMENDS claimed HTTPS redirect), but no PKCE or other MUST violation is present; the PKCE S256 requirement of RFC 8252 §8.1 is satisfied.

Overall verdict: PASS (noting the custom URI scheme redirect as a defense-in-depth recommendation only, not a MUST violation that changes the PASS).

### Reason
Both §3.1.3.7.11 MUST and RFC 9700 §2.4 MUST NOT satisfied. The PKCE S256 and nonce controls both active. PASS.

### Phrasing-B
PASS — nonce correct in ID token; ROPC replaced by authorization code + PKCE S256.

§3.1.3.7.11: nonce echoed; client validates match + single-use. MUST satisfied. Replay binding active.
RFC 9700 §2.4: ROPC disabled. Mobile app uses authorization code + PKCE S256. AS controls auth UI. MUST NOT satisfied.
Note: custom URI scheme redirect — RFC 8252 §7.2 RECOMMENDS claimed HTTPS redirect; PKCE S256 mitigates risk. This observation (CONCERN level) does not change the PASS verdict for the primary evaluation targets. PASS.

---

## Pair[CB4-3.4-799]

### A
Cross-source input — ext30 + PO.1 escalation (ROPC traceable to training gap on RFC 9700):

An engineering team has ROPC enabled in a product that was architected three years ago. During a recent security audit: (1) ROPC is active in production (RFC 9700 §2.4 MUST NOT); (2) SSDF PO.1 audit finds: the team has no formal process for tracking OAuth RFC updates or BCP publications; the lead architect states they were unaware that RFC 9700 was published in 2023 and superseded the ROPC permission in RFC 6749; (3) the architect confirms: "If I had known RFC 9700 said MUST NOT, we would not have kept ROPC." The training gap is directly traceable to the MUST NOT violation remaining undetected and unresolved.

### B
REPROCESS. RFC 9700 §2.4 MUST NOT violated in production, with the violation directly traceable to a SSDF PO.1 process gap.

Primary finding — RFC 9700 §2.4 MUST NOT ROPC: ROPC remains in active production use. The architect's unawareness of RFC 9700 does not create an exception; MUST NOT applies from the date of RFC 9700's publication regardless of implementation team awareness. REPROCESS.

PO.1 escalation: SSDF PO.1 requires the organization to establish processes for security training and staying current with security guidance. OAuth-based products have an obligation to track BCP updates — RFC 9700 superseding RFC 6749's ROPC permission is a significant, documented BCP change. The absence of a process for tracking BCP publications meant this MUST NOT went undetected for the period since RFC 9700's publication. The architect's own statement establishes the causal chain: no tracking process → unaware of RFC 9700 → ROPC MUST NOT persisted in production. PO.1 process absence + traceable production MUST NOT violation = escalation from CONCERN to REPROCESS.

REPROCESS on two independent grounds.

### Reason
RFC 9700 §2.4 MUST NOT is independently REPROCESS. PO.1 escalation: the absence of a BCP-tracking process is the proximate cause of the MUST NOT violation going unresolved. Both grounds confirmed.

### Phrasing-B
REPROCESS — RFC 9700 §2.4 MUST NOT ROPC; PO.1 BCP-tracking gap confirmed proximate cause.

RFC 9700 §2.4: ROPC active in production. MUST NOT applies from publication date regardless of team awareness. REPROCESS.

PO.1 escalation: no process for tracking OAuth BCP updates. RFC 9700 published 2023; team unaware. Architect confirmed: if aware, ROPC would have been removed. BCP-tracking process absence → MUST NOT undetected → production violation. SSDF PO.1 requires processes that include staying current with applicable security standards for the technologies in use. PO.1 gap + traceable production MUST NOT = REPROCESS (escalation from CONCERN). REPROCESS.

---

## Pair[CB4-3.4-800]

### A
Cross-source input — ext30 + ext29 combination (Cache-Control: no-store absent + access tokens without exp):

Two compounding token lifecycle gaps: (1) the token endpoint does not set Cache-Control: no-store on token responses — tokens and refresh tokens can be retained by HTTP caching layers; (2) access tokens issued have no exp claim — tokens are non-expiring at the resource server. Team: "Tokens are managed through our revocation API — if a token needs to be invalidated, we call the revocation endpoint. We don't need Cache-Control or exp because we control revocation actively."

### B
REPROCESS. Two independent MUST-level violations. The "active revocation management" argument addresses a third control layer that does not substitute for the violated MUST requirements.

Finding 1 — RFC 6749 §5.1 MUST Cache-Control: no-store absent: Cached token responses expose refresh tokens to any shared proxy or caching layer. The revocation API cannot revoke tokens that have been exfiltrated from a cache before revocation is triggered. Active revocation management assumes the organization knows a token has been compromised — cached tokens may be used by attackers before any compromise detection. RFC 6749 §5.1 MUST: violated. REPROCESS trigger.

Finding 2 — RFC 9068 §2.2 MUST exp claim absent: Non-expiring access tokens at the RS. The revocation API invocation does not expire existing access tokens — it records a revocation event that must be checked by the RS. Without exp, the RS has no time boundary to enforce; without an active blocklist check (not mentioned by the team), revoked tokens remain valid at the RS indefinitely. Even with a blocklist, tokens without exp require indefinite blocklist entry maintenance — no TTL-based cleanup possible. RFC 9068 §2.2 MUST: violated. REPROCESS trigger.

Active revocation management is a valuable control layer but is not a substitute for either MUST requirement. Both MUSTs establish baseline properties that revocation management augments, not replaces.

### Reason
RFC 6749 §5.1 MUST and RFC 9068 §2.2 MUST are independent baseline requirements. Active revocation is a complementary control, not a replacement. Both MUSTs violated.

### Phrasing-B
REPROCESS — Cache-Control: no-store absent and access token exp absent. Active revocation does not substitute for either MUST.

RFC 6749 §5.1: no no-store header. Tokens cacheable by intermediaries. Revocation API cannot revoke tokens that have been exfiltrated from a cache before detection. MUST violated. REPROCESS.

RFC 9068 §2.2: no exp claim. RS cannot enforce time boundary independently. Revocation API requires RS-side blocklist check per request — not mentioned and not guaranteed. Without exp, blocklist entries must be maintained indefinitely (no natural TTL anchor). MUST violated. REPROCESS.

---

## Pair[CB4-3.4-801]

### A
Cross-source input — ext30 + ext29 combination (admin:all service scope + scope elevation at refresh):

Two independent privilege escalation vulnerabilities: (1) a background analytics service is registered with scope=admin:all despite needing only read:analytics; (2) the authorization server's refresh token endpoint honors scope widening — user clients originally granted read:reports can request admin:reports in a refresh exchange and receive a token with that scope. Team: "The analytics service has broad access for flexibility, and scope expansion at refresh is a feature — it supports gradual permission acquisition."

### B
REPROCESS. Two independent privilege escalation MUST-level violations.

Finding 1 — CWE-269 + ASVS V8.1.2 (service account admin:all): The analytics service has admin:all scope; actual need is read:analytics. Privilege over-assignment — the service can perform operations completely unrelated to its function. "Flexibility for the service" does not satisfy the minimum-access requirement; least-privilege requires scoping to actual need, not to potential future needs. REPROCESS trigger.

Finding 2 — RFC 6749 §6 scope ceiling violation (refresh scope elevation): The AS honors admin:reports in a refresh exchange for a user originally granted only read:reports. RFC 6749 §6 bounds refresh scope to the original grant — the AS must not expand scope at refresh. "Gradual permission acquisition" is a legitimate UX goal but requires a new authorization request with user consent for each new scope; it cannot be implemented through the refresh grant where no user consent mechanism exists. REPROCESS trigger.

The "features" framing of both violations is rejected — "flexibility" and "gradual permission acquisition" describe security properties that can be achieved through compliant means (minimal scope re-registration; new authorization requests) but not through the non-compliant patterns described.

### Reason
Two independent privilege escalation REPROCESS triggers: CWE-269 service account over-scoping and RFC 6749 §6 refresh scope ceiling violation. Both described as "features" but neither satisfies the applicable MUST/standard.

### Phrasing-B
REPROCESS — two privilege escalation violations. Service account admin:all and refresh scope elevation both REPROCESS.

CWE-269 / ASVS V8.1.2: analytics service with admin:all; needs read:analytics. Flexibility argument rejected — least-privilege requires actual-need scoping, not future-flexibility scoping. REPROCESS.

RFC 6749 §6: refresh exchange honors admin:reports for a read:reports-only grant. Scope ceiling violated. "Gradual permission acquisition" requires new authorization requests with user consent, not refresh exchanges. REPROCESS.

---

## Pair[CB4-3.4-802]

### A
Cross-source input — ext30 comprehensive PASS (all patterns correctly implemented):

A complete authorization system review covers all ext30 domains: (1) all token responses include Cache-Control: no-store and Pragma: no-cache (RFC 6749 §5.1); (2) service accounts are registered with minimum necessary scope — analytics service: read:analytics; sync service: read:data-sync write:data-sync; no service registered with admin:all (CWE-269 / ASVS V8.1.2); (3) AS echoes nonce in ID token matching each client's authorization request value; clients validate nonce presence, match, and single-use (OIDC Core §3.1.3.7.11); (4) ROPC is disabled; all user auth uses authorization code + PKCE S256 (RFC 9700 §2.4); (5) refresh token exchanges are bounded to original grant scope — intersection enforced; widening returns 400 invalid_scope (RFC 6749 §6).

### B
PASS. All five ext30 security control domains correctly implemented. No findings.

RFC 6749 §5.1 Cache-Control: Cache-Control: no-store and Pragma: no-cache present on all token responses. No caching layer retains token material. MUST satisfied.

CWE-269 / ASVS V8.1.2 minimal scope: all service accounts registered with actual-need scope only. No over-assigned admin capabilities. RS enforces scope — any scope-unauthorized request returns 403. Least-privilege maintained.

OIDC Core §3.1.3.7.11 nonce: AS echoes nonce in ID token; clients perform complete nonce validation (presence, value match, single-use). Token-level replay binding active. MUST satisfied.

RFC 9700 §2.4 ROPC disabled: ROPC not registered or handled. All user authentication via authorization code + PKCE S256. AS controls authentication UI; MFA enforceable. MUST NOT satisfied.

RFC 6749 §6 refresh scope ceiling: intersection-based scope enforcement on all refresh exchanges. Widening → 400 invalid_scope. No unapproved scope expansion via refresh. MUST constraint satisfied.

No REPROCESS triggers. No CONCERN findings.

### Reason
All five ext30 domains satisfied: RFC 6749 §5.1 MUST, CWE-269/ASVS V8.1.2, OIDC Core §3.1.3.7.11 MUST, RFC 9700 §2.4 MUST NOT, RFC 6749 §6. Comprehensive PASS.

### Phrasing-B
PASS — all ext30 domains correctly implemented across caching, scope, nonce, ROPC, and refresh lifecycle.

(1) RFC 6749 §5.1: Cache-Control: no-store + Pragma: no-cache on all token responses. MUST satisfied.
(2) CWE-269 / ASVS V8.1.2: minimal scope per service; no admin:all over-assignment. RS scope enforcement active.
(3) OIDC Core §3.1.3.7.11: nonce echoed in ID token; client validates match + single-use. MUST satisfied.
(4) RFC 9700 §2.4 MUST NOT: ROPC disabled; authorization code + PKCE S256 replaces it.
(5) RFC 6749 §6: refresh scope = intersection(requested, original); widening → 400 invalid_scope.

All requirements met. PASS.
