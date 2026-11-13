# CB4 Stage 3 — Action 3 Extended Pair Set
# Node: Cross-source combinations (ext22 — ext29 cross-source)
# Sequences: CB4-3.4-767 through CB4-3.4-776
# Instances: 20 (10 base pairs × 2 phrasings each)
# Stratum: B/C (cross-source pattern combinations)
# Running total after this file: 1552 instances (776 base pairs)

---

## Pair[CB4-3.4-767]

### A
Cross-source input — ext29 combination (scope elevation at refresh + access tokens without exp):

An authorization system has two simultaneous gaps: (1) the authorization server honors scope expansion at the refresh token endpoint — a client originally granted read:orders write:orders can present those scopes plus admin:settings in a refresh request and receive a token with all three scopes; user consent was never obtained for admin:settings; (2) the access tokens issued have no exp claim; resource servers validate signature, iss, aud, and scope, but there is no expiry field to validate. Team: "The scope expansion makes the app more capable over time, and tokens without exp give us flexibility to manage revocation through our backend revocation API."

### B
REPROCESS. Two independent MUST-level violations identified.

Finding 1 — RFC 6749 §6 (scope ceiling violated at refresh): The AS issued admin:settings in response to a refresh request, despite admin:settings never appearing in the original resource owner grant. RFC 6749 §6: scope must be within the scope originally granted. "App becoming more capable over time" through refresh exchanges bypasses the user consent requirement — new scope capabilities require a new authorization request where the user can consent. The revocation API does not address scope ceiling violations at issuance time. REPROCESS trigger.

Finding 2 — RFC 9068 §2.2 MUST exp (access tokens without expiry): Tokens with no exp claim are non-expiring from the resource server's perspective. The backend revocation API addresses post-issuance revocation but requires active intervention for every revocation event; without exp, there is no natural time boundary and the revocation API must maintain entries indefinitely with no TTL-based cleanup. RFC 9068 §2.2 MUST exp: violated. REPROCESS trigger.

Both are MUST-level violations. REPROCESS.

### Reason
RFC 6749 §6 scope ceiling and RFC 9068 §2.2 exp claim are independent MUST requirements violated by distinct implementation gaps. The "flexibility" arguments address adjacent controls without resolving the MUST violations.

### Phrasing-B
REPROCESS — two MUST violations. Scope expansion beyond original grant; non-expiring access tokens.

RFC 6749 §6: admin:settings issued in refresh response; original grant contained only read:orders + write:orders. Scope ceiling violated. Incremental capabilities require user consent via authorization endpoint, not refresh exchange. REPROCESS.

RFC 9068 §2.2: no exp claim. RS cannot enforce time boundary. Revocation API manages revocation events but does not substitute for exp — it must run indefinitely without a natural TTL anchor. MUST exp: violated. REPROCESS.

---

## Pair[CB4-3.4-768]

### A
Cross-source input — ext29 combination (correct refresh scope + access tokens with exp):

An authorization system: (1) refresh token exchanges are bounded to the original grant scope; the AS issues tokens with scope = intersection of (requested scope, original grant scope); widening requests return 400 invalid_scope; (2) every JWT access token includes exp = iat + 900 (15-minute TTL); resource servers validate exp on every request and reject expired tokens with 401.

### B
PASS. Both scope ceiling enforcement and access token expiry correctly implemented.

Scope ceiling: refresh requests bounded to original grant. Widening requests rejected with 400 invalid_scope — the client receives an explicit signal that the request was invalid, preventing silent scope expansion. Downscoping correctly permitted. RFC 6749 §6 satisfied.

Access token expiry: exp present in every token at iat + 900 seconds. 15-minute TTL provides a natural revocation window — even without active revocation, compromised tokens expire within 15 minutes. RS validates exp before processing requests. RFC 9068 §2.2 MUST exp satisfied. RFC 7519 §4.1.4 SHOULD satisfied.

No findings.

### Reason
RFC 6749 §6 scope enforcement and RFC 9068 §2.2 exp both correctly implemented. 15-minute TTL appropriate for access tokens. PASS.

### Phrasing-B
PASS — refresh scope bounded to original grant; 15-minute exp claim in all access tokens.

Scope: AS issues intersection of (requested, original grant); widening → 400 invalid_scope. RFC 6749 §6 satisfied.
Expiry: exp = iat + 900 in every JWT. RS validates exp. 15-minute natural revocation window. RFC 9068 §2.2 satisfied. PASS.

---

## Pair[CB4-3.4-769]

### A
Cross-source input — ext29 combination (token endpoint credential logging + PKCE method omission):

Two security failures identified in a web application's authorization implementation: (1) the token endpoint logs the complete POST body including client_secret at INFO level in production; the log is shipped to a Splunk SIEM instance accessible to the security operations team and the DevOps team; (2) the React SPA sends authorization requests with code_challenge values but no code_challenge_method parameter; the SPA is S256-capable; the AS defaults to plain per RFC 7636 §4.3; the SPA presents the raw code_verifier at token exchange. Team: "Both are acceptable operational practices — logs help incident response, and the RFC defines the plain default."

### B
REPROCESS. Two independent MUST-level violations.

Finding 1 — V8.2.2 MUST NOT (credential logging): client_secret is a credential. INFO-level production logging of client_secret values violates V8.2.2 MUST NOT unconditionally. The SIEM accessibility argument is rejected — V8.2.2 prohibits the logging event, not just unauthorized access to logs. The SIEM and DevOps access extends the credential exposure surface beyond the authorization team. REPROCESS trigger.

Finding 2 — RFC 7636 §4.2 MUST (S256-capable client must use S256): The SPA is S256-capable. Omitting code_challenge_method allows the §4.3 plain default to apply. RFC 7636 §4.2 requires S256-capable clients to use S256 — the §4.3 default behavior for absent method does not satisfy §4.2 MUST for capable clients. Omission-downgrade carries the same security risk as an explicit plain-method request. "RFC defines the plain default" is accurate about §4.3; it does not address §4.2 MUST. REPROCESS trigger.

Two REPROCESS triggers. REPROCESS.

### Reason
V8.2.2 MUST NOT and RFC 7636 §4.2 MUST are both violated. Neither "operational practice" framing satisfies a MUST or MUST NOT.

### Phrasing-B
REPROCESS — V8.2.2 credential logging MUST NOT and RFC 7636 §4.2 PKCE MUST violated.

V8.2.2: client_secret in INFO-level production logs. MUST NOT logging of credentials is unconditional. SIEM access scope is irrelevant to whether the logging event occurred. REPROCESS.

RFC 7636 §4.2: S256-capable SPA omitted code_challenge_method. §4.3 plain default applied. §4.2 MUST requires S256 for capable clients — §4.3 default does not satisfy this obligation. Plain verification applies to an S256-capable SPA. REPROCESS.

---

## Pair[CB4-3.4-770]

### A
Cross-source input — ext29 combination (correct credential logging + explicit PKCE S256):

Two controls confirmed correct: (1) token endpoint logs timestamp, client_id, grant_type, scope, HTTP status — all non-credential fields; framework-level redaction list prevents client_secret, code, and token values from appearing in any log output at any level; (2) the React SPA explicitly includes code_challenge_method=S256 in every authorization request; the AS validates that the method is S256; the SPA computes SHA256(code_verifier) and base64url-encodes the result as code_challenge; the AS verifies SHA256(presented_verifier) == stored_challenge at token exchange.

### B
PASS. Both credential logging controls and PKCE S256 enforcement are correctly implemented.

Logging: framework-level redaction ensures credential fields never enter the log pipeline regardless of log level. Non-credential metadata sufficient for diagnostics. V8.2.2 MUST NOT satisfied.

PKCE S256: SPA explicitly specifies code_challenge_method=S256; no platform-default ambiguity. The AS validates the method is S256 — an S256-capable server could also reject plain-method requests from clients known to support S256, providing an additional defense layer. SHA256-based verification means the code_verifier cannot be derived from the code_challenge even if the challenge is observed in the authorization endpoint logs. RFC 7636 §4.2 MUST use S256 if capable: satisfied.

No findings.

### Reason
V8.2.2 and RFC 7636 §4.2 both satisfied. Framework redaction is level-independent. Explicit S256 eliminates default-plain vulnerability.

### Phrasing-B
PASS — credential logging redaction and explicit PKCE S256 both confirmed.

V8.2.2: framework-level redaction on all credential fields; non-credential metadata only in logs. MUST NOT satisfied.
RFC 7636 §4.2: code_challenge_method=S256 explicitly present in every SPA authorization request. SHA256-based challenge/verify. No default-plain scenario possible. §4.2 MUST satisfied. PASS.

---

## Pair[CB4-3.4-771]

### A
Cross-source input — ext29 combination (RFC 9207 iss absent + scope elevation at refresh):

Two issues in an authorization system: (1) authorization code responses contain code and state but no iss parameter; single-AS deployment; (2) the refresh token endpoint accepts scope widening — a client with read:reports can include analytics:export in a refresh request and receive a token with that scope, which was never granted by the user. Team: "The iss is optional, and the scope expansion enables business-required functionality."

### B
REPROCESS. The REPROCESS determination is driven by Finding 2 (MUST violation). Finding 1 produces a CONCERN that is absorbed.

Finding 1 — RFC 9207 §2 SHOULD (iss absent): Single-AS deployment reduces mix-up risk. SHOULD not MUST. CONCERN.

Finding 2 — RFC 6749 §6 MUST (scope elevation at refresh): analytics:export was not in the original resource owner grant. The AS issued it in response to a refresh request. The "business-required functionality" argument does not modify RFC 6749 §6 — a business requirement for additional scope must be satisfied through a new authorization request with user consent, not through a refresh exchange. User consent for analytics:export was never obtained. REPROCESS trigger.

Verdict: REPROCESS (from Finding 2). The CONCERN from Finding 1 is absorbed by the REPROCESS determination. REPROCESS overrides CONCERN per graduation rule.

### Reason
RFC 6749 §6 MUST violation (scope ceiling at refresh) determines the verdict. RFC 9207 SHOULD violation (iss absent) produces CONCERN but is absorbed. "Business requirement" is not a normative exception to RFC 6749 §6.

### Phrasing-B
REPROCESS — RFC 6749 §6 scope ceiling violated. RFC 9207 CONCERN absorbed into REPROCESS.

RFC 9207 §2 SHOULD: iss absent, CONCERN. Single-AS reduces risk. Absorbed by REPROCESS.

RFC 6749 §6: analytics:export issued in refresh response; not in original grant. Business requirement for new capability must go through authorization endpoint with user consent — refresh exchange does not involve the user and has no consent mechanism for new scopes. REPROCESS.

Graduation rule: REPROCESS overrides CONCERN. Verdict: REPROCESS.

---

## Pair[CB4-3.4-772]

### A
Cross-source input — ext29 combination (iss present + bounded refresh + exp claim):

Three correct controls confirmed: (1) authorization code responses include iss="https://as.example.com"; client validates iss on every response; (2) refresh exchanges are bounded to original grant — the AS uses intersection(requested, original) as the issued scope; (3) access tokens carry exp = iat + 600 (10-minute TTL); RS validates exp on every request.

### B
PASS. All three ext29 control areas correctly implemented.

RFC 9207 §2 SHOULD: iss parameter present and client-validated. Mix-up attack defense active.

RFC 6749 §6 scope ceiling: issued scope = intersection(requested, original). No expansion beyond original grant. Any request for scope not in the original grant is silently narrowed (or rejected with 400 if full intersection is empty). Ceiling enforced.

RFC 9068 §2.2 exp: 10-minute TTL. exp present in every token. RS validates exp before processing. Natural revocation window: even without blocklist, tokens expire within 10 minutes of issuance.

No findings across all three domains.

### Reason
RFC 9207 SHOULD, RFC 6749 §6, and RFC 9068 §2.2 all correctly implemented. Clean multi-domain PASS.

### Phrasing-B
PASS — iss present + scope bounded + exp claim all confirmed correct.

iss: authorization response includes iss="https://as.example.com"; client validates. RFC 9207 §2 SHOULD satisfied.
Scope: intersection-based refresh scope enforcement. No expansion beyond original grant. RFC 6749 §6 satisfied.
Expiry: exp = iat + 600 in every token. 10-minute TTL. RS validates exp. RFC 9068 §2.2 satisfied. PASS.

---

## Pair[CB4-3.4-773]

### A
Cross-source input — ext29 + ext28 combination (access token without exp + RS aud validation absent):

A resource server environment review finds: (1) access tokens carry no exp claim — the RS validates signature, iss, scope, but has nothing to validate for expiry; (2) the same RS does not validate the aud claim — any valid AS-issued token is accepted regardless of its intended audience. The RS serves sensitive financial data. Team: "Token freshness and audience scoping are managed at the application layer, not the token layer."

### B
REPROCESS. Two independent MUST-level violations at the resource server layer.

Finding 1 — RFC 9068 §2.2 MUST exp (absent from access tokens): Non-expiring tokens at the RS — once issued, a token is valid until the RS's revocation list rejects it, with no natural expiry. The application layer cannot substitute for the RS-level exp validation — the RS processes API requests, and without exp it cannot independently enforce a time boundary. REPROCESS trigger.

Finding 2 — RFC 9068 §4 + RFC 8725 §3.9 (aud validation absent): This RS serves sensitive financial data. Any valid AS-issued token is accepted regardless of aud value — a token issued for a low-privilege RS (reporting, user profile) passes validation at this RS. Audience confusion attack: low-privilege tokens gain access to sensitive financial endpoints. The "application layer" argument does not address the RS-layer authorization boundary that aud provides. REPROCESS trigger.

"Token freshness and audience scoping managed at application layer" — rejected for both findings. Token expiry and audience constraints are RS-layer validation steps; the application layer is above the RS-layer token validation and cannot substitute for it.

### Reason
Two independent REPROCESS triggers: RFC 9068 §2.2 MUST exp and RFC 9068 §4 aud validation. Both are RS-layer MUST requirements. Application-layer management argument rejected for both.

### Phrasing-B
REPROCESS — access token exp absent and RS aud validation absent. Two RS-layer MUST violations.

RFC 9068 §2.2: no exp claim. RS cannot enforce time boundary. Application-layer management does not address RS-layer validation gap. REPROCESS.

RFC 9068 §4: aud not validated at RS. Tokens for other RSs accepted at this sensitive-financial-data RS. Audience confusion viable. Application-layer argument does not address authorization boundary gap. REPROCESS.

---

## Pair[CB4-3.4-774]

### A
Cross-source input — ext29 + PO.1 escalation (scope elevation traceable to training gap):

A post-incident review: an authorization server was discovered to be honoring scope widening at refresh token exchange. Tokens with admin:billing were issued to clients who were originally granted only read:billing. Internal review finds: (1) the engineer who implemented the refresh endpoint was not trained on RFC 6749 §6 — they were unaware of the scope ceiling concept; they implemented "honor whatever scope the client requests" because "the client knows what it needs"; (2) SSDF PO.1 audit confirms: no formal OAuth security training program exists; engineers are expected to read RFC documentation independently; (3) the admin:billing scope grants access to billing modification endpoints, and two clients have already used it.

### B
REPROCESS. RFC 6749 §6 scope ceiling violated in production with confirmed exploitation, and SSDF PO.1 training absence confirmed as proximate cause.

Primary finding — RFC 6749 §6 MUST (scope ceiling): The AS issued admin:billing in refresh responses for clients whose original grant contained only read:billing. Admin:billing was never authorized by any resource owner through the consent flow. The "client knows what it needs" argument is rejected — the client's request is not self-authorizing; authorization requires resource owner consent. Production exploitation confirmed: two clients have already received admin:billing tokens through refresh requests. REPROCESS.

PO.1 escalation: SSDF PO.1 requires formal security training for personnel with security-critical responsibilities. The engineer implementing the refresh endpoint — a security-critical OAuth flow — was unaware of the scope ceiling concept. This is a demonstrable knowledge gap that would have been covered by OAuth security training. The engineer's own statement confirms the causal chain: no knowledge of §6 → "honor whatever scope requested" → production MUST violation with exploitation. PO.1 training absence is the proximate cause of the MUST violation. Escalation rule: PO.1 CONCERN + traceable production MUST violation = REPROCESS (independent ground for REPROCESS).

Two independent REPROCESS grounds: (1) RFC 6749 §6 direct violation; (2) PO.1 traceable escalation.

### Reason
RFC 6749 §6 production violation with exploitation is already REPROCESS. PO.1 escalation provides a second independent ground. The engineer's statement establishes the causal chain without ambiguity.

### Phrasing-B
REPROCESS — RFC 6749 §6 scope ceiling violated with exploitation; PO.1 training absence confirmed proximate cause.

RFC 6749 §6: admin:billing issued via refresh to clients with original grant read:billing only. No consent for admin:billing. Production exploitation by two clients confirmed. "Client knows what it needs" is not a consent mechanism. REPROCESS.

PO.1 escalation: engineer unaware of RFC 6749 §6 scope ceiling — explicitly stated. No formal OAuth training program. Security-critical implementation (refresh endpoint) done without security-critical knowledge. PO.1 training absence + traceable production MUST violation = REPROCESS (independent ground). REPROCESS.

---

## Pair[CB4-3.4-775]

### A
Cross-source input — ext29 + ext28 combination (token endpoint credential logging + JWKS over HTTP):

Two infrastructure security gaps: (1) the token endpoint logs full POST bodies in production, including client_secret values — logs are retained in S3 for 90 days with broad access; (2) the authorization server's JWKS endpoint is served over plain HTTP at http://as.example.com/.well-known/jwks.json. Team: "Both are infrastructure decisions outside the application security team's scope."

### B
REPROCESS. Two independent MUST-level violations. Organizational scope assignment does not determine compliance status — MUST requirements apply regardless of which team owns the infrastructure.

Finding 1 — V8.2.2 MUST NOT (credential logging): client_secret values in production logs retained 90 days in S3. V8.2.2 MUST NOT log credentials is unconditional. 90-day S3 retention with broad access creates a persistent credential store from log data. Infrastructure team ownership does not transfer or defer the MUST NOT obligation. REPROCESS trigger.

Finding 2 — RFC 7517 + RFC 8414 §3 (JWKS over HTTP): JWKS endpoint over HTTP allows on-path adversaries to substitute public key material in the JWKS response. Resource servers fetching keys over HTTP may receive attacker-supplied public keys — JWT signature verification then validates attacker-forged tokens as legitimate. The integrity of all JWT validation across all resource servers depends on the integrity of JWKS delivery. HTTP JWKS is equivalent in risk to the HTTP discovery case (RFC 8414 §3 MUST HTTPS). REPROCESS trigger.

Two REPROCESS triggers. REPROCESS.

### Reason
V8.2.2 MUST NOT and RFC 7517 / RFC 8414 §3 HTTPS requirement both violated. Infrastructure team ownership is not a normative exception.

### Phrasing-B
REPROCESS — V8.2.2 credential logging MUST NOT and JWKS HTTP integrity violation.

V8.2.2: client_secret in 90-day S3 production logs. MUST NOT is unconditional; team ownership irrelevant. REPROCESS.

JWKS HTTP: on-path attacker can substitute public key material in JWKS response; RS fetches attacker-supplied key; attacker-forged JWTs pass signature verification. RFC 7517 + RFC 8414 §3 HTTPS integrity requirement. REPROCESS.

---

## Pair[CB4-3.4-776]

### A
Cross-source input — ext29 comprehensive PASS (all patterns correctly implemented):

A complete review of ext29 security domains: (1) authorization code responses include iss="https://as.example.com"; client validates iss against configured issuer on every response (RFC 9207 §2 SHOULD); (2) refresh token exchanges are bounded to original grant scope — intersection of (requested, original) used; widening requests return 400 invalid_scope (RFC 6749 §6); (3) token endpoint logs non-credential metadata only — framework-level redaction covers client_secret, code, tokens (V8.2.2 MUST NOT); (4) React SPA sends code_challenge_method=S256 explicitly in every authorization request; AS validates method is S256 (RFC 7636 §4.2); (5) access tokens carry exp = iat + 900; RS validates exp on every request (RFC 9068 §2.2).

### B
PASS. All five ext29 security control domains correctly implemented. No findings.

RFC 9207 §2 SHOULD: iss present in authorization response; client validates. Mix-up defense active.

RFC 6749 §6 scope ceiling: refresh scope = intersection(requested, original). Widening → 400 invalid_scope. No unapproved scope expansion possible. Ceiling enforced.

V8.2.2 MUST NOT credential logging: framework-level redaction prevents all credential fields from entering the log pipeline at any log level. Non-credential diagnostics maintained.

RFC 7636 §4.2 PKCE S256: S256 explicit in every authorization request. No method omission possible — AS validates method is S256; plain-method or method-absent requests would be rejected. §4.2 MUST satisfied.

RFC 9068 §2.2 exp: exp = iat + 900 in all access tokens. 15-minute natural revocation window. RS validates exp. MUST satisfied.

No REPROCESS triggers. No CONCERN findings. All controls meet or exceed normative requirements.

### Reason
All five ext29 domains satisfied: RFC 9207 SHOULD, RFC 6749 §6 MUST, V8.2.2 MUST NOT, RFC 7636 §4.2 MUST, RFC 9068 §2.2 MUST. Comprehensive PASS.

### Phrasing-B
PASS — all ext29 domains correctly implemented.

(1) RFC 9207 §2 SHOULD: iss in every authorization response; client validates. Mix-up defense active.
(2) RFC 6749 §6: scope ceiling enforced via intersection; widening returns 400 invalid_scope. No consent bypass.
(3) V8.2.2 MUST NOT: framework-level credential redaction; no credential fields in any log output.
(4) RFC 7636 §4.2 MUST: code_challenge_method=S256 explicit; AS validates; no plain fallback.
(5) RFC 9068 §2.2 MUST: exp = iat + 900 in all tokens; RS validates before processing.

All requirements met. PASS.
