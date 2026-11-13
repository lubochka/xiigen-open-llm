# CB4 Stage 3 — Action 3 Extended Pair Set
# Node: Multi-node pipeline chains (ext23 — ext29 pipeline traces)
# Sequences: CB4-3.4-759 through CB4-3.4-766
# Instances: 16 (8 base pairs × 2 phrasings each)
# Stratum: C (full 9-node pipeline traces)
# Running total after this file: 1532 instances (766 base pairs)

---

## Pair[CB4-3.4-759]

### A
Full pipeline trace — RFC 9207 iss parameter absent from authorization response:

Input: AS does not include iss parameter in authorization code redirects. Single-AS deployment. Team: "RFC 9207 uses SHOULD language — it's optional."

### B
Node 1 — Domain Splitter: Domain T-rfc-oauth2 (RFC 9207 authorization server identification). Sub-domain: iss parameter in authorization response. Defense against AS mix-up attacks.

Node 2 — Domain Translator: Subject: iss parameter presence in authorization code response. Reference: RFC 9207 §2 SHOULD. Single-AS context noted.

Node 3 — Scheme Planner: Plan: (1) RFC 9207 §2 compliance check; (2) assess SHOULD vs MUST level; (3) evaluate single-AS risk reduction argument; (4) determine verdict.

Node 4 — Domain Processor:
  iss parameter: absent from authorization response.
  RFC 9207 §2: AS SHOULD include iss. SHOULD-level, not MUST.
  Team's characterization of SHOULD language: accurate.
  Single-AS argument: reduces mix-up risk but does not satisfy SHOULD recommendation. Defense-in-depth gap: without iss, clients cannot detect AS substitution in misconfiguration or future multi-AS expansion scenarios.
  Verdict level: SHOULD not satisfied. One CONCERN-level gap.

Node 5 — Applier: RFC 9207 §2 SHOULD not satisfied. CONCERN-level gap. No REPROCESS trigger.

Node 6 — Result Translator: CONCERN.

Node 7 — Arbiter Selector: T-rfc-oauth2 arbiter for RFC 9207 SHOULD-level guidance.

Node 8 — Arbiter Units: CONCERN. RFC 9207 §2 SHOULD iss parameter absent. Single-AS deployment reduces risk without eliminating the defense-in-depth gap. SHOULD-level: CONCERN.

Node 9 — Formatter: Verdict: CONCERN. RFC 9207 §2. Include iss parameter in authorization responses; clients should validate against configured issuer.

### Reason
SHOULD-level RFC 9207 gap. Team correctly identifies SHOULD language. Single-AS risk reduction acknowledged. CONCERN.

### Phrasing-B
Node 1: T-rfc-oauth2. RFC 9207 iss response parameter.
Node 2: iss absent. RFC 9207 §2 SHOULD.
Node 3: §2 compliance; SHOULD level; single-AS argument.
Node 4: iss absent. SHOULD not satisfied. Single-AS reduces but doesn't eliminate defense gap. CONCERN level.
Node 5: CONCERN gap. No MUST violated.
Node 6: CONCERN.
Node 7: T-rfc-oauth2.
Node 8: CONCERN. RFC 9207 §2 SHOULD iss absent.
Node 9: CONCERN. RFC 9207 §2. Include iss in authorization responses.

---

## Pair[CB4-3.4-760]

### A
Full pipeline trace — iss present in authorization response; client validates it:

Input: AS includes iss="https://as.example.com" in every authorization code response. Client validates iss against configured issuer; rejects mismatched responses.

### B
Node 1 — Domain Splitter: T-rfc-oauth2. RFC 9207 iss parameter.

Node 2 — Domain Translator: iss="https://as.example.com" in response. Client validates. RFC 9207 §2 SHOULD.

Node 3 — Scheme Planner: (1) iss present and matching; (2) client validation behavior.

Node 4 — Domain Processor: iss present in authorization response. Client validates match to configured issuer; rejects mismatch. RFC 9207 §2 SHOULD satisfied.

Node 5 — Applier: No violations.

Node 6 — Result Translator: PASS.

Node 7 — Arbiter Selector: No activation.

Node 8 — Arbiter Units: PASS. RFC 9207 §2 SHOULD satisfied. Mix-up attack vector blocked.

Node 9 — Formatter: Verdict: PASS. RFC 9207 §2 satisfied.

### Reason
iss present and validated. SHOULD satisfied. PASS.

### Phrasing-B
Node 1: T-rfc-oauth2. RFC 9207.
Node 2: iss="https://as.example.com". RFC 9207 §2 SHOULD.
Node 3: iss present; client validates.
Node 4: Match confirmed. §2 SHOULD satisfied.
Node 5: No violations.
Node 6: PASS.
Node 7: No activation.
Node 8: PASS.
Node 9: PASS.

---

## Pair[CB4-3.4-761]

### A
Full pipeline trace — Scope elevation at refresh token exchange honored by AS:

Input: Original grant scope=read:profile read:data. Client refresh request includes scope=read:profile read:data write:data admin:content. AS issues token with all four scopes. Team: "Incremental permission acquisition — user authenticated."

### B
Node 1 — Domain Splitter: Domain T-rfc-oauth2 (RFC 6749 §6 refresh token scope constraint). Sub-domain: scope boundary enforcement at token endpoint refresh exchange.

Node 2 — Domain Translator: Subject: scope field in refresh token exchange. Original grant: read:profile read:data. Requested in refresh: + write:data + admin:content (two unapproved scopes). Reference: RFC 6749 §6 — scope within originally granted scope. Argument: incremental permission / user authentication.

Node 3 — Scheme Planner: Plan: (1) compare requested scope to original grant; (2) evaluate §6 ceiling constraint; (3) assess incremental-permission argument; (4) determine verdict.

Node 4 — Domain Processor:
  Original grant: {read:profile, read:data}.
  Requested in refresh: {read:profile, read:data, write:data, admin:content}.
  Difference: write:data and admin:content were never granted by the resource owner.
  RFC 6749 §6: "The authorization server MAY issue a new access token with a different scope, provided that the scope is within the scope originally granted by the resource owner." AS issued write:data and admin:content — outside the original grant. §6 ceiling violated.
  Incremental-permission argument: obtaining new scopes requires a new authorization request at the authorization endpoint where the resource owner consents. The refresh exchange is server-to-server; no consent mechanism is present. Argument rejected.
  User authentication at original login: covers read:profile and read:data only. It does not constitute consent for write:data or admin:content.

Node 5 — Applier: RFC 6749 §6 scope ceiling violated. Argument rejected. REPROCESS trigger: 1.

Node 6 — Result Translator: REPROCESS.

Node 7 — Arbiter Selector: T-rfc-oauth2 arbiter for RFC 6749 §6 scope enforcement.

Node 8 — Arbiter Units: REPROCESS. RFC 6749 §6 violated — AS issued tokens with scopes (write:data, admin:content) beyond the original resource owner grant. Refresh exchange cannot confer new scopes; new scopes require a new authorization request with user consent. Incremental-permission argument rejected.

Node 9 — Formatter: Verdict: REPROCESS. RFC 6749 §6. AS must reject scope values not within the original grant (return 400 invalid_scope) or issue token with only the intersection of requested and original grant.

### Reason
RFC 6749 §6 scope ceiling is a MUST-level constraint. The AS issued unapproved scopes. Incremental permission requires user consent via authorization endpoint, not a refresh exchange.

### Phrasing-B
Node 1: T-rfc-oauth2. RFC 6749 §6 refresh scope.
Node 2: Original: read:profile + read:data. Requested: + write:data + admin:content. RFC 6749 §6 ceiling. Incremental-permission argument.
Node 3: (1) scope comparison; (2) §6 ceiling; (3) argument evaluation.
Node 4: write:data and admin:content not in original grant. AS issued them. §6 ceiling violated. Incremental permission requires authorization endpoint consent, not refresh request. Authentication does not substitute for scope consent.
Node 5: §6 ceiling violated. REPROCESS trigger.
Node 6: REPROCESS.
Node 7: T-rfc-oauth2.
Node 8: REPROCESS. RFC 6749 §6 — unapproved scopes in refresh response. Reject or narrow to original grant.
Node 9: REPROCESS. RFC 6749 §6. Reject scope beyond original grant or issue intersection.

---

## Pair[CB4-3.4-762]

### A
Full pipeline trace — Refresh token scope correctly bounded; downscoping honored:

Input: Original grant scope=read:profile read:data. Same-scope refresh request issued and honored (read:profile read:data). Downscoped refresh (read:profile only) also honored correctly.

### B
Node 1 — Domain Splitter: T-rfc-oauth2. RFC 6749 §6 scope constraint.

Node 2 — Domain Translator: Original grant = {read:profile, read:data}. Same-scope refresh: within grant. Downscoped refresh: subset. RFC 6749 §6.

Node 3 — Scheme Planner: (1) Same-scope: within ceiling; (2) Downscoped: subset of ceiling.

Node 4 — Domain Processor: Same-scope request = {read:profile, read:data} ⊆ original grant. Honored. RFC 6749 §6 ceiling: satisfied. Downscoped request = {read:profile} ⊂ original grant. AS issued narrower scope. RFC 6749 §6 permits downscoping. No violations.

Node 5 — Applier: No violations.

Node 6 — Result Translator: PASS.

Node 7 — Arbiter Selector: No activation.

Node 8 — Arbiter Units: PASS. RFC 6749 §6 scope ceiling satisfied in both cases. Downscoping correctly permitted.

Node 9 — Formatter: PASS. RFC 6749 §6 satisfied.

### Reason
Both same-scope and downscope refresh correctly enforce the §6 ceiling. PASS.

### Phrasing-B
Node 1: T-rfc-oauth2. §6 scope.
Node 2: Same-scope and downscoped refresh. RFC 6749 §6.
Node 3: Ceiling check both cases.
Node 4: Both within original grant. No violations.
Node 5: No violations.
Node 6: PASS.
Node 7: No activation.
Node 8: PASS. §6 satisfied.
Node 9: PASS.

---

## Pair[CB4-3.4-763]

### A
Full pipeline trace — Token endpoint logs client_secret in production DEBUG logging:

Input: Token endpoint POST body logged at DEBUG level in production. Body includes client_secret. Logs in ELK accessible to multiple teams. Team: "Logs are access-controlled."

### B
Node 1 — Domain Splitter: Domain T-asvs-authentication (V8.2.2 credential logging). Sub-domain: token endpoint request body logging containing credentials.

Node 2 — Domain Translator: Subject: client_secret captured in production logs. Reference: V8.2.2 MUST NOT log sensitive credentials. Argument: access-controlled log store.

Node 3 — Scheme Planner: Plan: (1) V8.2.2 MUST NOT compliance; (2) assess access-control argument; (3) identify exposure surface; (4) determine verdict.

Node 4 — Domain Processor:
  client_secret in production ELK logs: confirmed.
  V8.2.2 MUST NOT log sensitive credentials: violated unconditionally.
  Access-control argument: V8.2.2 is not conditioned on whether the log store is access-controlled. The MUST NOT is a prohibition on the logging event itself, not on log access permissions. ELK stores accessible to multiple teams multiply the credential exposure surface — operations, development, support, SIEM integrations, backups, exports.
  Additional: authorization codes, refresh tokens, or access tokens in request/response bodies (if also logged) would constitute additional V9.3.1 MUST NOT violations.
  Argument rejected. REPROCESS trigger: 1.

Node 5 — Applier: V8.2.2 MUST NOT violated. Argument rejected. REPROCESS trigger.

Node 6 — Result Translator: REPROCESS.

Node 7 — Arbiter Selector: T-asvs-authentication arbiter for V8.2.2 credential logging.

Node 8 — Arbiter Units: REPROCESS. V8.2.2 MUST NOT violated — client_secret captured in production logs. Access-controlled log store does not satisfy MUST NOT; it limits post-exposure access, not the credential capture event. REPROCESS.

Node 9 — Formatter: Verdict: REPROCESS. V8.2.2 MUST NOT. Argument rejected: access-controlled store. Remediation: log non-credential metadata only; apply framework-level redaction for credential fields.

### Reason
V8.2.2 MUST NOT is unconditional. Access control on the log store is irrelevant to whether the credential was logged. REPROCESS.

### Phrasing-B
Node 1: T-asvs-authentication. V8.2.2 credential logging.
Node 2: client_secret in production ELK logs. V8.2.2 MUST NOT. Access-controlled argument.
Node 3: (1) V8.2.2 compliance; (2) argument evaluation; (3) exposure surface.
Node 4: client_secret in logs confirmed. MUST NOT violated unconditionally. Access control limits who sees the credential; does not prevent the logging event. ELK multiplies exposure. Argument rejected.
Node 5: MUST NOT violated. REPROCESS trigger.
Node 6: REPROCESS.
Node 7: T-asvs-authentication.
Node 8: REPROCESS. V8.2.2 MUST NOT. client_secret in logs. Access control ≠ MUST NOT compliance.
Node 9: REPROCESS. V8.2.2. Framework-level credential redaction required.

---

## Pair[CB4-3.4-764]

### A
Full pipeline trace — Token endpoint logging correctly excludes credentials via framework redaction:

Input: Token endpoint logs timestamp, client_id, grant_type, scope, HTTP status. Framework-level redaction list covers client_secret, code, refresh_token, access_token — all output as "[REDACTED]" regardless of log level.

### B
Node 1 — Domain Splitter: T-asvs-authentication. V8.2.2 credential logging.

Node 2 — Domain Translator: Logged: non-credential metadata. Redacted: all credential fields, framework-level. V8.2.2 MUST NOT.

Node 3 — Scheme Planner: (1) Logged fields check — do any contain credentials? (2) Redaction coverage check.

Node 4 — Domain Processor: Logged fields: timestamp, client_id, grant_type, scope, HTTP status — all non-sensitive metadata. Credential fields (client_secret, code, refresh_token, access_token): all in framework-level redaction list, output as "[REDACTED]". Level-independent: changing log level does not expose credentials. V8.2.2 MUST NOT: satisfied.

Node 5 — Applier: No violations.

Node 6 — Result Translator: PASS.

Node 7 — Arbiter Selector: No activation.

Node 8 — Arbiter Units: PASS. V8.2.2 MUST NOT satisfied. Framework-level redaction prevents credential capture regardless of log level.

Node 9 — Formatter: PASS. V8.2.2 satisfied.

### Reason
Framework-level redaction covers all credential fields unconditionally. Non-credential metadata sufficient for diagnostics. PASS.

### Phrasing-B
Node 1: T-asvs-authentication. V8.2.2.
Node 2: Non-credential metadata logged. Credential fields redacted at framework level. V8.2.2 MUST NOT.
Node 3: (1) Logged field check; (2) redaction coverage.
Node 4: No credentials in logged output. Redaction level-independent. V8.2.2 satisfied.
Node 5: No violations.
Node 6: PASS.
Node 7: No activation.
Node 8: PASS. V8.2.2 satisfied.
Node 9: PASS.

---

## Pair[CB4-3.4-765]

### A
Full pipeline trace — PKCE code_challenge_method absent; AS defaults to plain for S256-capable client:

Input: SPA (S256-capable) sends authorization request with code_challenge but no code_challenge_method. AS defaults to plain per RFC 7636 §4.3. Token exchange uses plain verification. Team: "Following the RFC 7636 default behavior."

### B
Node 1 — Domain Splitter: Domain T-rfc-oauth2 (RFC 7636 PKCE). Sub-domain: code_challenge_method specification and S256 obligation for capable clients. Distinct from explicit plain-method downgrade — this is an omission-triggered downgrade.

Node 2 — Domain Translator: Subject: code_challenge_method absent in request from S256-capable SPA. Server applies §4.3 plain default. Reference: RFC 7636 §4.2 MUST use S256 if capable; §4.3 plain default when method absent. Argument: RFC default behavior.

Node 3 — Scheme Planner: Plan: (1) RFC 7636 §4.2 MUST for S256-capable client; (2) evaluate whether §4.3 default satisfies §4.2 MUST; (3) assess "following the default" argument; (4) determine verdict.

Node 4 — Domain Processor:
  Client capability: S256-capable SPA.
  Method in request: absent.
  AS action: defaults to plain per RFC 7636 §4.3.
  Result: plain PKCE verification applied to S256-capable client exchange.
  RFC 7636 §4.2 MUST: "If the client is capable of using S256, it MUST use S256." Client failed to include code_challenge_method=S256. §4.2 MUST violated by omission.
  §4.3 default argument: §4.3 defines the server-side handling of absent method (default = plain). This is technically accurate. However, §4.2 creates a client-side MUST that precedes §4.3: an S256-capable client must include code_challenge_method=S256 in its request. By omitting the parameter, the client created a situation where the server's §4.3 behavior (correct per that section) produced an outcome that violates §4.2. The "following the default" argument is accurate about the server's §4.3 compliance; it does not address the client's §4.2 non-compliance.
  Security consequence: plain method transmits code_verifier directly; equivalent to explicit plain-method request from an S256-capable client — previously established as REPROCESS.

Node 5 — Applier: RFC 7636 §4.2 MUST violated (omission-downgrade). Argument partially accepted (§4.3 correct) but does not address §4.2 MUST. REPROCESS trigger.

Node 6 — Result Translator: REPROCESS.

Node 7 — Arbiter Selector: T-rfc-oauth2 arbiter for RFC 7636 PKCE §4.2 enforcement.

Node 8 — Arbiter Units: REPROCESS. RFC 7636 §4.2 MUST violated — S256-capable SPA omitted code_challenge_method, allowing AS §4.3 default (plain) to apply. The §4.3 default behavior is procedurally correct; the §4.2 MUST creates an obligation on S256-capable clients to prevent this scenario by always specifying code_challenge_method=S256. Omission-downgrade: same risk as explicit plain request from S256-capable client. REPROCESS.

Node 9 — Formatter: Verdict: REPROCESS. RFC 7636 §4.2. S256-capable clients must include code_challenge_method=S256 in every authorization request; absence is not permissible when client is S256-capable.

### Reason
§4.2 MUST creates a client-side obligation that §4.3 plain default does not satisfy for S256-capable clients. Omission-downgrade risk = explicit downgrade risk. REPROCESS.

### Phrasing-B
Node 1: T-rfc-oauth2. RFC 7636 §4.2 / §4.3 PKCE method obligation.
Node 2: SPA (S256-capable). method absent. §4.3 plain default applied. RFC 7636 §4.2 MUST use S256. "Following default" argument.
Node 3: (1) §4.2 MUST for S256-capable; (2) §4.3 default evaluation; (3) combined analysis.
Node 4: §4.2 MUST violated — client omitted method. §4.3 default: server-side behavior correct but §4.2 MUST creates client obligation to prevent this outcome. Plain applied to S256-capable SPA. Same risk as explicit plain.
Node 5: §4.2 MUST violated by omission. REPROCESS trigger.
Node 6: REPROCESS.
Node 7: T-rfc-oauth2.
Node 8: REPROCESS. RFC 7636 §4.2 — S256-capable client must specify method=S256; §4.3 default does not satisfy §4.2 MUST.
Node 9: REPROCESS. §4.2. S256-capable clients must always include code_challenge_method=S256.

---

## Pair[CB4-3.4-766]

### A
Full pipeline trace — JWT access tokens issued without exp claim:

Input: JWT access tokens have no exp claim. Resource servers validate signature, iss, aud, scope — not exp (nothing to validate). Team: "RFC 7519 lists exp as OPTIONAL. Short-lived refresh tokens + application sessions manage token lifecycle."

### B
Node 1 — Domain Splitter: Domain T-rfc-jwt (JWT claim requirements for OAuth 2.0 access tokens). Sub-domain: exp claim absence — non-expiring access token. Separate from the application session management lifecycle.

Node 2 — Domain Translator: Subject: exp claim absent from JWT access tokens. Applicable references: RFC 9068 §2.2 (JWT Profile for OAuth 2.0 Access Tokens — exp MUST be present); RFC 7519 §4.1.4 (exp claim SHOULD, general JWT). Arguments: (1) RFC 7519 optional; (2) refresh token lifecycle; (3) application session.

Node 3 — Scheme Planner: Plan: (1) RFC 9068 §2.2 MUST check for access tokens; (2) reconcile with RFC 7519 SHOULD; (3) evaluate three arguments; (4) assess RS enforcement gap; (5) determine verdict.

Node 4 — Domain Processor:
  exp claim: absent from all issued access tokens.
  RFC 9068 §2.2: "The JWT access token MUST contain an exp claim as defined in Section 4.1.4 of [RFC7519]." Profile-specific MUST overrides base-spec claim optionality for access tokens conforming to this profile.
  RFC 7519 §4.1.4: exp is a SHOULD at base spec level — correct for general JWTs. OAuth 2.0 access tokens in this deployment are JWT access tokens; RFC 9068 profile applies.
  Argument 1 — "RFC 7519 exp optional": correctly states the base spec. RFC 9068 §2.2 MUST is the applicable norm for access tokens; profile-specific MUST is the controlling requirement.
  Argument 2 — "Refresh token lifecycle manages expiry": the refresh token controls re-issuance of access tokens; it does not expire the access token already in flight at the RS. The RS validates access tokens independently; it has no visibility into the refresh token state. A non-expiring access token is valid at the RS until it is revoked (via blocklist). Without exp, the RS cannot enforce time-bounded validity — it can only validate revocation, which requires a blocklist that must be maintained indefinitely (no natural expiry anchor).
  Argument 3 — "Application session": the application session controls client-side UX; it does not prevent direct API calls with the access token independent of the session state.

Node 5 — Applier: RFC 9068 §2.2 MUST exp: violated. All three arguments rejected. REPROCESS trigger.

Node 6 — Result Translator: REPROCESS.

Node 7 — Arbiter Selector: T-rfc-jwt arbiter for RFC 9068 §2.2 access token profile requirements.

Node 8 — Arbiter Units: REPROCESS. RFC 9068 §2.2 MUST exp claim absent from JWT access tokens. Tokens are non-expiring from the RS's perspective — the RS cannot enforce a time boundary without exp. All three arguments (RFC 7519 optional; refresh token; application session) address controls orthogonal to the access token's own exp enforcement at the RS. RFC 9068 profile-specific MUST is the controlling requirement for access tokens.

Node 9 — Formatter: Verdict: REPROCESS. RFC 9068 §2.2. Access tokens must include exp claim. Recommended: short TTL (15 minutes for standard access; shorter for high-privilege operations). Absence of exp means non-expiring tokens that cannot be naturally bounded without indefinite revocation blocklist maintenance.

### Reason
RFC 9068 §2.2 MUST exp is the controlling requirement for JWT access tokens. RFC 7519 OPTIONAL is the base-spec default, not the profile requirement. All three team arguments address different lifecycle controls, not the RS-enforced access token expiry. REPROCESS.

### Phrasing-B
Node 1: T-rfc-jwt. RFC 9068 §2.2 exp claim requirement.
Node 2: exp absent from JWT access tokens. RFC 9068 §2.2 MUST vs RFC 7519 §4.1.4 SHOULD. Three arguments.
Node 3: (1) RFC 9068 §2.2 MUST check; (2) profile vs base-spec resolution; (3) three-argument evaluation.
Node 4: exp absent confirmed. RFC 9068 §2.2 MUST: violated — profile-specific MUST controls over RFC 7519 SHOULD for access tokens. Refresh token: orthogonal (controls re-issuance, not in-flight access token expiry at RS). Application session: orthogonal (UX control, not API-layer enforcement). Non-expiring token requires indefinite blocklist — no natural TTL anchor.
Node 5: RFC 9068 §2.2 MUST violated. Three arguments rejected. REPROCESS.
Node 6: REPROCESS.
Node 7: T-rfc-jwt.
Node 8: REPROCESS. RFC 9068 §2.2 — exp absent. Access tokens non-expiring at RS. Profile MUST controls. REPROCESS.
Node 9: REPROCESS. RFC 9068 §2.2. Include exp with appropriate TTL in all issued access tokens.
