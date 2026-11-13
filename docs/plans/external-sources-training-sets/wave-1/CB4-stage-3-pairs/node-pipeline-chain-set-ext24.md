# CB4 Stage 3 — Action 3 Extended Pair Set
# Node: Multi-node pipeline chains (ext24 — ext30 pipeline traces)
# Sequences: CB4-3.4-785 through CB4-3.4-792
# Instances: 16 (8 base pairs × 2 phrasings each)
# Stratum: C (full 9-node pipeline traces)
# Running total after this file: 1584 instances (792 base pairs)

---

## Pair[CB4-3.4-785]

### A
Full pipeline trace — Token endpoint response missing Cache-Control: no-store:

Input: AS token endpoint returns tokens in JSON response body without Cache-Control: no-store header. Team: "Short-lived tokens + API gateway handles header normalization."

### B
Node 1 — Domain Splitter: Domain T-rfc-oauth2 (RFC 6749 §5.1 token response headers). Sub-domain: Cache-Control header requirement on token endpoint responses.

Node 2 — Domain Translator: Subject: token response HTTP headers — Cache-Control absent. Reference: RFC 6749 §5.1 MUST Cache-Control: no-store. Arguments: (1) short-lived tokens; (2) API gateway.

Node 3 — Scheme Planner: Plan: (1) RFC 6749 §5.1 MUST compliance check; (2) evaluate TTL argument; (3) evaluate gateway argument; (4) identify refresh token exposure risk; (5) determine verdict.

Node 4 — Domain Processor:
  Cache-Control: no-store: absent from token endpoint response.
  RFC 6749 §5.1 MUST: "The authorization server MUST include the HTTP Cache-Control response header field with a value of no-store in any response containing tokens." Violated unconditionally.
  TTL argument: accepted as partial mitigation for access tokens only. Refresh tokens are long-lived (hours/days) and included in the same response. Cached token response exposes long-lived refresh tokens to any caching layer that stores the response. TTL argument does not satisfy MUST.
  Gateway argument: RFC 6749 §5.1 requires the AS (origin) to include the header. API gateway is not the AS; if the gateway strips or normalizes headers, origin compliance is still the AS's responsibility. Rejected.

Node 5 — Applier: RFC 6749 §5.1 MUST violated. Both arguments rejected. REPROCESS trigger: 1.

Node 6 — Result Translator: REPROCESS.

Node 7 — Arbiter Selector: T-rfc-oauth2 arbiter for RFC 6749 §5.1 token response header.

Node 8 — Arbiter Units: REPROCESS. RFC 6749 §5.1 MUST Cache-Control: no-store absent. Cached refresh tokens exposed. TTL argument partial at best; gateway argument displaces origin responsibility.

Node 9 — Formatter: Verdict: REPROCESS. RFC 6749 §5.1. Add Cache-Control: no-store and Pragma: no-cache to all token endpoint responses at the AS origin.

### Reason
RFC 6749 §5.1 MUST is unconditional. Refresh token cache exposure is the primary risk. Both arguments rejected.

### Phrasing-B
Node 1: T-rfc-oauth2. RFC 6749 §5.1 token response headers.
Node 2: Cache-Control absent. RFC 6749 §5.1 MUST. Short-TTL and gateway arguments.
Node 3: §5.1 compliance; TTL argument; gateway argument; refresh token risk.
Node 4: no-store absent. §5.1 violated. TTL inadequate for refresh tokens. Gateway is downstream of origin obligation. Rejected.
Node 5: §5.1 MUST violated. REPROCESS trigger.
Node 6: REPROCESS.
Node 7: T-rfc-oauth2.
Node 8: REPROCESS. §5.1 MUST Cache-Control: no-store absent. Refresh token caching risk.
Node 9: REPROCESS. §5.1. Add no-store + Pragma: no-cache at AS origin.

---

## Pair[CB4-3.4-786]

### A
Full pipeline trace — Token endpoint response with correct Cache-Control: no-store:

Input: AS token endpoint returns Cache-Control: no-store and Pragma: no-cache on every response containing token material. Both access token and refresh token responses include these headers.

### B
Node 1 — Domain Splitter: T-rfc-oauth2. RFC 6749 §5.1 token response headers.

Node 2 — Domain Translator: Cache-Control: no-store present. Pragma: no-cache present. RFC 6749 §5.1 MUST.

Node 3 — Scheme Planner: (1) §5.1 compliance check.

Node 4 — Domain Processor: Cache-Control: no-store present. Pragma: no-cache present for HTTP/1.0 cache compatibility. RFC 6749 §5.1 MUST: satisfied. No caching layer can retain token responses.

Node 5 — Applier: No violations.

Node 6 — Result Translator: PASS.

Node 7 — Arbiter Selector: No activation.

Node 8 — Arbiter Units: PASS. RFC 6749 §5.1 MUST Cache-Control: no-store satisfied.

Node 9 — Formatter: PASS. RFC 6749 §5.1 satisfied.

### Reason
Both no-store and Pragma: no-cache present at origin. §5.1 satisfied. PASS.

### Phrasing-B
Node 1: T-rfc-oauth2. §5.1.
Node 2: Cache-Control: no-store + Pragma: no-cache present. §5.1.
Node 3: §5.1 check.
Node 4: Both headers present. §5.1 satisfied.
Node 5: No violations.
Node 6: PASS.
Node 7: No activation.
Node 8: PASS.
Node 9: PASS.

---

## Pair[CB4-3.4-787]

### A
Full pipeline trace — Client credentials service account with over-broad scope (admin:all):

Input: Background reporting service registered with scope=admin:all. Actual need: read:user-activity only. "Internal service, trusted" argument.

### B
Node 1 — Domain Splitter: Domain T-asvs-authorization (least-privilege) + T-cwe-authz (CWE-269 privilege assignment). Sub-domain: client credentials scope registration against actual service access needs.

Node 2 — Domain Translator: Subject: service account registered with admin:all; actual use: read:user-activity. References: CWE-269 (improper privilege assignment); ASVS V8.1.2 (deny-by-default / minimum access). Argument: internal trust.

Node 3 — Scheme Planner: Plan: (1) compare registered scope to actual access need; (2) CWE-269 assessment; (3) ASVS V8.1.2 compliance; (4) evaluate internal-trust argument; (5) determine verdict.

Node 4 — Domain Processor:
  Registered scope: admin:all (full administrative access).
  Actual service access: read:user-activity (one read endpoint).
  Gap: admin:all grants billing modification, user management, system configuration — none required by a reporting service.
  CWE-269: privilege assignment not based on actual need; driven by operational convenience. Canonical CWE-269 scenario.
  ASVS V8.1.2: services must operate with minimum access required. admin:all is not minimum for a reporting service.
  Internal-trust argument: rejected. Internal services are primary targets of lateral movement attacks. Compromised service credentials with admin:all scope provide full administrative access. The trust level of the service does not determine the scope assignment — actual functional need does.

Node 5 — Applier: CWE-269 and ASVS V8.1.2 violated. Argument rejected. REPROCESS trigger.

Node 6 — Result Translator: REPROCESS.

Node 7 — Arbiter Selector: T-asvs-authorization + T-cwe-authz arbiters.

Node 8 — Arbiter Units: REPROCESS. CWE-269 privilege over-assignment — admin:all for a service needing only read:user-activity. ASVS V8.1.2 minimum access violated. Internal trust does not substitute for least-privilege scope assignment.

Node 9 — Formatter: Verdict: REPROCESS. CWE-269; ASVS V8.1.2. Re-register service with scope=read:user-activity only. Verify service operates correctly with minimal scope.

### Reason
CWE-269 and ASVS V8.1.2 both triggered by admin:all for a read-only reporting service. Internal trust rejected as a scope justification.

### Phrasing-B
Node 1: T-asvs-authorization + T-cwe-authz. Scope minimization for client credentials.
Node 2: admin:all registered; read:user-activity actual need. CWE-269; ASVS V8.1.2. Internal-trust argument.
Node 3: (1) Scope comparison; (2) CWE-269; (3) ASVS V8.1.2; (4) trust argument.
Node 4: Gap: admin:all grants billing, user mgmt, config. Reporting service needs one read endpoint. CWE-269 over-privilege. ASVS V8.1.2 minimum access violated. Internal trust rejected: lateral movement targets internal service accounts.
Node 5: CWE-269 + ASVS V8.1.2 violated. REPROCESS.
Node 6: REPROCESS.
Node 7: T-asvs-authorization + T-cwe-authz.
Node 8: REPROCESS. CWE-269 + ASVS V8.1.2. admin:all excessive. Re-register with read:user-activity.
Node 9: REPROCESS. CWE-269; ASVS V8.1.2. Minimal scope registration required.

---

## Pair[CB4-3.4-788]

### A
Full pipeline trace — Client credentials service account with minimal scope:

Input: Reporting service re-registered with scope=read:user-activity only. RS enforces scope — admin endpoints return 403 for this token. Token TTL: 60 minutes.

### B
Node 1 — Domain Splitter: T-asvs-authorization + T-cwe-authz. Scope minimization.

Node 2 — Domain Translator: scope=read:user-activity matches actual need. CWE-269; ASVS V8.1.2.

Node 3 — Scheme Planner: (1) Scope-to-need comparison; (2) RS enforcement check.

Node 4 — Domain Processor: Scope: read:user-activity = actual need. Minimal. RS enforces: admin endpoints return 403. CWE-269: eliminated. ASVS V8.1.2: satisfied. 60-min TTL appropriate for hourly batch.

Node 5 — Applier: No violations.

Node 6 — Result Translator: PASS.

Node 7 — Arbiter Selector: No activation.

Node 8 — Arbiter Units: PASS. CWE-269 eliminated. ASVS V8.1.2 satisfied. Scope matches actual need.

Node 9 — Formatter: PASS. CWE-269 and ASVS V8.1.2 satisfied.

### Reason
Minimal scope matches actual need. RS enforcement provides defense-in-depth. PASS.

### Phrasing-B
Node 1: T-asvs-authorization + T-cwe-authz.
Node 2: read:user-activity = actual need. CWE-269; ASVS V8.1.2.
Node 3: Scope comparison; RS enforcement.
Node 4: Minimal scope. RS 403 on admin endpoints. CWE-269 eliminated. ASVS V8.1.2 satisfied.
Node 5: No violations.
Node 6: PASS.
Node 7: No activation.
Node 8: PASS.
Node 9: PASS.

---

## Pair[CB4-3.4-789]

### A
Full pipeline trace — ID token issued without nonce when nonce was in authorization request:

Input: Client sends nonce=a7b3c9d2f0e1 in authorization request. AS issues ID token without nonce claim. Team: "Nonce is an optional claim."

### B
Node 1 — Domain Splitter: Domain T-rfc-oidc (OIDC Core nonce handling). Sub-domain: AS obligation to include nonce in ID token when nonce was provided in the authorization request.

Node 2 — Domain Translator: Subject: nonce present in authorization request; absent from ID token. Reference: OIDC Core §3.1.3.7.11 MUST include nonce in ID token when present in request. Argument: optional claim.

Node 3 — Scheme Planner: Plan: (1) nonce in authorization request confirmed; (2) OIDC Core §3.1.3.7.11 MUST compliance check; (3) evaluate "optional claim" argument; (4) assess security consequence; (5) determine verdict.

Node 4 — Domain Processor:
  Nonce in authorization request: confirmed (nonce=a7b3c9d2f0e1).
  ID token nonce claim: absent.
  OIDC Core §3.1.3.7.11: "If present in the Authentication Request, Authorization Servers MUST include a nonce Claim in the ID Token." Nonce was present in request; MUST includes it in ID token; AS did not include it. MUST violated.
  "Optional claim" argument: conditionally accurate — nonce is optional when the client does not provide it. When the client provides nonce in the request, §3.1.3.7.11 converts the AS's nonce inclusion to a MUST. The team's argument applies to the wrong condition.
  Security consequence: without nonce in ID token, the client cannot validate nonce match. ID tokens from a different session or from a replay attack that captures a prior ID token cannot be detected without nonce validation. Token-level replay binding is absent.

Node 5 — Applier: OIDC Core §3.1.3.7.11 MUST violated. Argument rejected. REPROCESS trigger: 1.

Node 6 — Result Translator: REPROCESS.

Node 7 — Arbiter Selector: T-rfc-oidc arbiter for OIDC Core nonce handling.

Node 8 — Arbiter Units: REPROCESS. OIDC Core §3.1.3.7.11 MUST violated — nonce present in authorization request but absent from ID token. "Optional claim" argument applies to the case where client did not provide nonce; it does not address the MUST triggered by nonce presence in the request. Replay binding absent.

Node 9 — Formatter: Verdict: REPROCESS. OIDC Core §3.1.3.7.11. Include nonce claim in ID token matching the value from the authorization request when nonce was provided.

### Reason
§3.1.3.7.11 MUST is triggered by nonce presence in the authorization request. The "optional" characterization applies to a different condition. Replay binding absent.

### Phrasing-B
Node 1: T-rfc-oidc. OIDC Core §3.1.3.7.11 nonce.
Node 2: nonce in request; absent from ID token. §3.1.3.7.11 MUST. Optional-claim argument.
Node 3: (1) nonce in request confirmed; (2) §3.1.3.7.11 MUST; (3) argument evaluation; (4) replay risk.
Node 4: nonce=a7b3c9d2f0e1 in request. ID token lacks nonce. §3.1.3.7.11 MUST violated. "Optional" applies when client does not provide nonce — not applicable here. Replay binding absent.
Node 5: §3.1.3.7.11 MUST violated. Argument rejected. REPROCESS.
Node 6: REPROCESS.
Node 7: T-rfc-oidc.
Node 8: REPROCESS. §3.1.3.7.11 — nonce absent from ID token when present in request. Replay binding absent.
Node 9: REPROCESS. §3.1.3.7.11. Echo nonce in ID token.

---

## Pair[CB4-3.4-790]

### A
Full pipeline trace — ID token nonce correct; client validates match and single-use:

Input: Client sends nonce=a7b3c9d2f0e1. AS issues ID token with nonce: "a7b3c9d2f0e1". Client validates: presence, match to session-stored value, marks as used.

### B
Node 1 — Domain Splitter: T-rfc-oidc. OIDC Core §3.1.3.7.11 nonce.

Node 2 — Domain Translator: nonce in request and ID token match. Client validates. §3.1.3.7.11 MUST.

Node 3 — Scheme Planner: (1) AS nonce echo compliance; (2) client validation completeness.

Node 4 — Domain Processor: nonce="a7b3c9d2f0e1" in ID token matches request value. §3.1.3.7.11 satisfied. Client: presence check → value match → single-use mark. Replay binding active — prior session or replayed ID tokens would have different or already-used nonce values.

Node 5 — Applier: No violations.

Node 6 — Result Translator: PASS.

Node 7 — Arbiter Selector: No activation.

Node 8 — Arbiter Units: PASS. OIDC Core §3.1.3.7.11 MUST satisfied. Nonce present, matched, single-use enforced.

Node 9 — Formatter: PASS. §3.1.3.7.11 satisfied.

### Reason
AS echoes nonce; client validates match and single-use. §3.1.3.7.11 satisfied. PASS.

### Phrasing-B
Node 1: T-rfc-oidc. §3.1.3.7.11.
Node 2: nonce match confirmed. §3.1.3.7.11 MUST.
Node 3: AS echo + client validation.
Node 4: Match confirmed. Single-use marked. §3.1.3.7.11 satisfied. Replay binding active.
Node 5: No violations.
Node 6: PASS.
Node 7: No activation.
Node 8: PASS. §3.1.3.7.11 satisfied.
Node 9: PASS.

---

## Pair[CB4-3.4-791]

### A
Full pipeline trace — ROPC grant type enabled for user authentication:

Input: AS supports grant_type=password. Users post username+password directly to /token. Application collects credentials in its own UI. Team: "RFC 6749 defines password grant. Users prefer it."

### B
Node 1 — Domain Splitter: Domain T-rfc-oauth2 (RFC 9700 ROPC prohibition). Sub-domain: resource owner password credentials grant type security status under current BCP.

Node 2 — Domain Translator: Subject: ROPC (grant_type=password) enabled. Reference: RFC 9700 §2.4 MUST NOT use ROPC. Arguments: (1) RFC 6749 defines it; (2) user preference.

Node 3 — Scheme Planner: Plan: (1) RFC 9700 §2.4 MUST NOT compliance; (2) evaluate RFC 6749 argument (historical vs current BCP); (3) evaluate user preference argument; (4) identify security rationale; (5) determine verdict.

Node 4 — Domain Processor:
  ROPC enabled: AS handles grant_type=password; users post credentials to /token via application.
  RFC 9700 §2.4: "The resource owner password credentials grant MUST NOT be used." RFC 9700 is BCP 240, the current BCP for OAuth 2.0 security. MUST NOT violated.
  RFC 6749 argument: RFC 6749 (2012) originally defined ROPC as a supported grant. RFC 9700 (current BCP) explicitly supersedes this permissive treatment based on accumulated security experience. The historical definition in RFC 6749 does not override the current MUST NOT in RFC 9700.
  User preference argument: does not modify MUST NOT. The preference for "direct credential entry in the app" is precisely the pattern OAuth 2.0 was designed to prevent — direct credential collection by applications bypasses the AS-controlled authentication UI where MFA and adaptive authentication can be enforced.
  Security rationale for MUST NOT: (a) application receives user credentials directly; (b) phishing apps indistinguishable from legitimate apps using ROPC; (c) AS cannot enforce MFA or adaptive authentication in the ROPC flow.

Node 5 — Applier: RFC 9700 §2.4 MUST NOT violated. Both arguments rejected. REPROCESS trigger: 1.

Node 6 — Result Translator: REPROCESS.

Node 7 — Arbiter Selector: T-rfc-oauth2 arbiter for RFC 9700 §2.4 ROPC prohibition.

Node 8 — Arbiter Units: REPROCESS. RFC 9700 §2.4 MUST NOT ROPC violated. RFC 6749 definition is historical — RFC 9700 BCP supersedes. User preference does not modify MUST NOT. ROPC eliminates the AS trust boundary, enables phishing, and bypasses MFA enforcement.

Node 9 — Formatter: Verdict: REPROCESS. RFC 9700 §2.4 MUST NOT. Disable ROPC; implement authorization code flow with PKCE S256 for user authentication.

### Reason
RFC 9700 §2.4 MUST NOT is current BCP superseding RFC 6749's original ROPC definition. Historical permissibility does not satisfy current MUST NOT.

### Phrasing-B
Node 1: T-rfc-oauth2. RFC 9700 §2.4 ROPC prohibition.
Node 2: ROPC enabled. RFC 9700 §2.4 MUST NOT. RFC 6749 and preference arguments.
Node 3: (1) RFC 9700 §2.4 MUST NOT; (2) RFC 6749 vs current BCP resolution; (3) preference argument; (4) security rationale.
Node 4: ROPC active. RFC 9700 §2.4 MUST NOT violated. RFC 6749 historical; RFC 9700 supersedes. Preference argument: direct credential collection = OAuth trust boundary elimination. MFA bypass; phishing indistinguishable. MUST NOT.
Node 5: RFC 9700 §2.4 violated. Both arguments rejected. REPROCESS.
Node 6: REPROCESS.
Node 7: T-rfc-oauth2.
Node 8: REPROCESS. RFC 9700 §2.4 MUST NOT ROPC. Disable; use authorization code + PKCE.
Node 9: REPROCESS. RFC 9700 §2.4. Disable ROPC; implement authorization code flow.

---

## Pair[CB4-3.4-792]

### A
Full pipeline trace — ROPC disabled; authorization code + PKCE replaces it:

Input: ROPC disabled on AS. All user auth through authorization_code + PKCE S256. AS controls authentication UI including MFA. Team reviewed RFC 9700 and disabled ROPC proactively.

### B
Node 1 — Domain Splitter: T-rfc-oauth2. RFC 9700 §2.4 ROPC and authorization code flow compliance.

Node 2 — Domain Translator: ROPC disabled. Authorization code + PKCE S256 active. RFC 9700 §2.4 MUST NOT and RFC 7636 §4.2.

Node 3 — Scheme Planner: (1) ROPC absence confirmed; (2) authorization code + PKCE S256 correctness.

Node 4 — Domain Processor: ROPC: grant_type=password not registered; not handled by token endpoint. RFC 9700 §2.4 MUST NOT: satisfied by disabling. Authorization code: user redirected to AS; AS controls authentication (MFA enforceable). PKCE S256: code_challenge_method=S256 explicit. RFC 7636 §4.2 MUST: satisfied.

Node 5 — Applier: No violations.

Node 6 — Result Translator: PASS.

Node 7 — Arbiter Selector: No activation.

Node 8 — Arbiter Units: PASS. RFC 9700 §2.4 MUST NOT satisfied (ROPC disabled). Authorization code + PKCE S256 replaces ROPC correctly. AS trust boundary maintained.

Node 9 — Formatter: PASS. RFC 9700 §2.4 and RFC 7636 §4.2 satisfied.

### Reason
ROPC disabled (MUST NOT satisfied). Authorization code + PKCE S256 is the BCP-compliant replacement. PASS.

### Phrasing-B
Node 1: T-rfc-oauth2. RFC 9700 §2.4 and RFC 7636.
Node 2: ROPC disabled. Auth code + PKCE S256. RFC 9700 §2.4; RFC 7636 §4.2.
Node 3: ROPC absence; auth code + PKCE.
Node 4: ROPC: not registered. §2.4 MUST NOT satisfied. Auth code + PKCE S256: AS controls auth; MFA enforceable; §4.2 satisfied.
Node 5: No violations.
Node 6: PASS.
Node 7: No activation.
Node 8: PASS. RFC 9700 §2.4 satisfied. Auth code + PKCE S256 BCP-compliant.
Node 9: PASS.
