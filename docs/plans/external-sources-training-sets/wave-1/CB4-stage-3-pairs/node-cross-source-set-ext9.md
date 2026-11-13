# CB4 Stage 3 — Node Cross-Source Set Extension 9
# Sequences CB4-3.4-429 through CB4-3.4-438
# Stratum: B/C/D — multi-source combinations, cross-territory interactions
# Instances: 20 (10 base pairs × 2 phrasings)
# Running total after this file: 876 instances

---

## Pair[CB4-3.4-429]

**A:** Session fixation and credential management combined. Application: session ID regenerated on login (old invalidated). Session cookie: HttpOnly, Secure, SameSite=Strict. Database password stored in an environment variable in `.env` file. The `.env` file is in the project root. `.env` is in `.gitignore`. The team: "It's in .gitignore so it's not in the repo."

**B:**
```
Arbiter: ASVS-V7-Session + SSDF-PW

T-asvs-session:
  V7.2.3 (session fixation): new session ID on login, old invalidated ✓
  Session cookie: HttpOnly ✓; Secure ✓; SameSite=Strict ✓
  T-asvs-session verdict: PASS

T-ssdf-process:
  PW.1 (.env file with database password):
    ".gitignore prevents commit" argument evaluation:
    (1) .gitignore prevents FUTURE accidental commits of the .env file.
    (2) It does not prevent: (a) the file being present on the server or
        developer machine where it can be read by local processes;
        (b) the file being accidentally committed before .gitignore was set up;
        (c) the file being shared via other channels (Slack, email, etc.);
        (d) the plaintext value being readable by anyone with access to the
        deployment filesystem.
    (3) .gitignore is source control hygiene, not credential management.
    (4) A .env file with plaintext credentials is not a protected credential store —
        it is a plaintext file on the filesystem.
    Finding: CONCERN — .env with plaintext credential is not PW.1-compliant
      protected storage; .gitignore reduces source control exposure but does
      not protect the credential at rest.
  T-ssdf-process verdict: CONCERN

Overall verdict: CONCERN (session PASS; credential management CONCERN)
```

**Reason:** Session fixation prevention is correctly implemented (PASS). `.env` file with plaintext credentials is CONCERN: `.gitignore` prevents future source control commits but does not protect the credential at rest on the filesystem. A protected credential store (Vault, CI/CD secrets) is required. CONCERN because `.gitignore` provides partial protection (source control).

**Phrasing-B:** Session ID regenerated on login with HttpOnly/Secure/SameSite cookie. DB password in .env file, .env in .gitignore. Session fixation is handled — does the .gitignore on .env satisfy PW.1?

---

## Pair[CB4-3.4-430]

**A:** Token TLS and algorithm combined. Token endpoint: HTTPS directly (RFC 6749 §3.2 satisfied). JWT algorithm: RS256. RSA key: 1024-bit. Key stored in HashiCorp Vault. Code review: Security Champion + OWASP checklist. All other OAuth controls (exact URIs, PKCE S256, state) correct.

**B:**
```
Arbiter: RFC-OAuth2 + RFC-JWT + ASVS-V9-Token + SSDF-PW + ASVS-V10-OAuth

T-rfc-oauth2:
  RFC 6749 §3.2: HTTPS directly ✓ — PASS

T-asvs-token / T-rfc-jwt:
  V9.1.1 RS256 allowlist ✓
  RFC 7518 §3.3: 1024-bit RSA key ✗
    MUST ≥ 2048 bits for RS256 — 1024-bit violates this MUST
  T-asvs-token verdict: REPROCESS

T-ssdf-process:
  PW.1: Vault for signing key ✓ — PASS
  PW.3.2: Security Champion + checklist ✓ — PASS

T-oauth-oidc:
  V10.2.1 ✓; V10.4.6 ✓; V10.2.6 ✓ → PASS

Overall verdict: REPROCESS
(RFC 7518 §3.3 1024-bit key REPROCESS overrides all PASSes)
```

**Reason:** Token endpoint TLS correct (PASS). Vault and Security Champion correct (PASS). OAuth controls correct (PASS). The 1024-bit RSA key violates RFC 7518 §3.3 MUST ≥ 2048 bits — regardless of algorithm allowlist, key size is a separate MUST. One REPROCESS overrides all other correct controls.

**Phrasing-B:** Token endpoint direct HTTPS, RS256 allowlist, but 1024-bit RSA key (in Vault). Security Champion + OWASP checklist. All other OAuth controls correct. Does the correct TLS and process controls compensate for the 1024-bit key, or is it a REPROCESS?

---

## Pair[CB4-3.4-431]

**A:** Scope validation and authorization combined. OAuth: scope response returned when narrowed (RFC 6749 §3.3 satisfied). Authorization: /me/permissions endpoint returns full permission set. Deny-by-default with explicit grants. However: access tokens include a `scope` claim reflecting the authorized OAuth scopes. The resource server checks the token signature and `aud` but does NOT enforce scope — every authenticated request succeeds regardless of what scopes are in the token. Team: "Scope is for the client to know what it can do — we use role-based checks at the endpoint level."

**B:**
```
Arbiter: ASVS-V10-OAuth + ASVS-V8-AuthZ + RFC-OAuth2

T-rfc-oauth2:
  RFC 6749 §3.3: scope response returned when narrowed ✓ — PASS

T-oauth-oidc:
  Token scope claims present but not enforced at resource server:
  RFC 6749 and OAuth 2.0 authorization model: access tokens convey authorization
  scope — the resource server should verify that the token's scope covers the
  requested operation.
  Not enforcing scope means: a token with scope=read can call write endpoints;
  a token issued to one client scope can access all resource endpoints.
  V10 / resource server scope enforcement: REPROCESS
  While RBAC at endpoint level adds authorization, scope enforcement is a
  separate OAuth authorization mechanism.

T-asvs-authorization:
  V8.1.2 (deny-by-default): RBAC with deny-by-default ✓
  V8.2.1 /me/permissions ✓
  However: scope not enforced = broader access than the token was authorized for.
  This is a CONCERN at the authorization level (scope bypass via role checks).

Team argument ("role-based checks at endpoint level"):
  RBAC and scope enforcement are both required in OAuth resource servers.
  RBAC answers "does this user have the role for this action?"
  Scope answers "was this token issued with authorization for this scope?"
  A client may have a role but receive a narrow-scope token intentionally —
  not enforcing scope undermines the token's authorization constraint.

T-oauth-oidc verdict: REPROCESS (scope not enforced at resource server)
T-asvs-authorization: CONCERN (RBAC exists; scope bypass is a separate gap)
Overall verdict: REPROCESS
```

**Reason:** RFC 6749 §3.3 scope response is correctly returned (PASS). However, not enforcing the token's scope at the resource server undermines the OAuth authorization model — a narrow-scope token grants access to all operations if RBAC alone is checked. REPROCESS for scope non-enforcement. RBAC and scope are independent, complementary authorization mechanisms.

**Phrasing-B:** Scope correctly returned in responses, /me/permissions and deny-by-default RBAC correct. But resource server doesn't enforce token scope — we trust RBAC at endpoint level. Does RBAC substitute for resource server scope enforcement?

---

## Pair[CB4-3.4-432]

**A:** Combined review: stateless JWT sessions, token endpoint TLS, and authorization code lifetime. Application: stateless JWT (no server-side sessions). Token endpoint: HTTPS directly on service. Authorization codes: 3-minute expiry, single-use. PKCE S256. State validated. Exact redirect URIs. RS256 with 2048-bit key rotated annually. Vault for secrets. Argon2id.

**B:**
```
Arbiter: ASVS-V7-Session + RFC-OAuth2 + ASVS-V10-OAuth + ASVS-V9-Token

T-asvs-session:
  V7.2.3 (session fixation — stateless JWT): PASS
    No pre-auth session; JWT issued post-authentication only ✓
    Fixation attack surface absent by architecture ✓

T-rfc-oauth2:
  RFC 6749 §3.2: HTTPS directly ✓ — PASS
  RFC 6749 §4.1.2: 3-min single-use codes ✓ (within 10-min RECOMMENDED max) — PASS

T-oauth-oidc:
  V10.2.1 ✓; V10.4.6 ✓; V10.2.6 ✓ → PASS

T-asvs-token:
  V9.1.1 RS256 ✓; RFC 7518 §3.3 2048-bit ✓; V9.2 annual rotation ✓ → PASS

T-asvs-authentication:
  V6.2.5 Argon2id ✓ → PASS

T-ssdf-process: Vault ✓ → PASS

All territories: PASS
Overall verdict: PASS
```

**Reason:** Stateless JWT inherently satisfies V7.2.3 (no pre-auth session). Token endpoint direct TLS and 3-minute codes satisfy RFC 6749. RS256 with 2048-bit key and annual rotation correct. Comprehensive PASS.

**Phrasing-B:** Stateless JWT sessions, token endpoint direct HTTPS, 3-min single-use codes, PKCE S256, RS256 2048-bit rotated annually, Vault, Argon2id. Does this combination pass all territories?

---

## Pair[CB4-3.4-433]

**A:** Three-way combined: session fixation + token endpoint TLS + RSA key size. Application: session-based with cookie. Session ID NOT regenerated on login (same ID used pre/post-auth). Token endpoint: HTTPS directly. JWT signing: RS256 with 4096-bit key rotated quarterly. Exact URIs, PKCE S256, state, Argon2id, Vault, Security Champion.

**B:**
```
Arbiter: ASVS-V7-Session + RFC-OAuth2 + ASVS-V9-Token + Multiple

T-asvs-session:
  V7.2.3: session ID NOT regenerated on login ✗ → REPROCESS
    Same session ID pre/post-login = classic session fixation vulnerability.
    Remediation: regenerate session ID on successful authentication.

T-rfc-oauth2:
  RFC 6749 §3.2: HTTPS directly ✓ — PASS

T-asvs-token:
  V9.1.1 RS256 ✓; RFC 7518 §3.3 4096-bit ✓; V9.2 quarterly ✓ → PASS

All other controls: PASS

Overall verdict: REPROCESS
(V7.2.3 session fixation REPROCESS overrides all PASSes)
```

**Reason:** Token endpoint TLS is correct; 4096-bit RS256 with quarterly rotation is correct; all OAuth and credential controls are correct. The single gap is session fixation: the session ID is not regenerated on login. One REPROCESS overrides everything.

**Phrasing-B:** Token endpoint direct HTTPS, 4096-bit RS256 rotated quarterly, PKCE, Vault, Security Champion — all correct. Session ID not regenerated on login. Is the session fixation gap a REPROCESS despite everything else being correct?

---

## Pair[CB4-3.4-434]

**A:** OAuth scope combined with PKCE and state. Authorization server: PKCE S256 enforced for all clients; state validated. Scope: a client registered with `scope=read` requests `scope=admin` at authorization time. The authorization server: (a) detects the requested scope exceeds the client's registered scopes; (b) rejects the authorization request with `error=invalid_scope`; (c) does not issue a token. Team: "We reject rather than silently narrow — is error rejection compliant?"

**B:**
```
Arbiter: RFC-OAuth2 + ASVS-V10-OAuth

T-rfc-oauth2:
  RFC 6749 §4.1.2 / §5.2 (error responses):
    Authorization server rejects request with error=invalid_scope ✓
    RFC 6749 §4.1.2.1 defines error responses for authorization requests;
    §4.2.2.1 for implicit (not applicable here).
    Rejecting with invalid_scope when requested scope exceeds registered scope
    is a valid and compliant response per RFC 6749. ✓
  RFC 6749 §3.3: narrowing with scope response is one option; rejection is another.
    Both are RFC 6749-conformant behaviors.

T-oauth-oidc:
  V10.4.6 PKCE ✓; V10.2.6 state ✓
  Scope enforcement: rejection of unauthorized scope requests ✓ — strict enforcement

Both rejection and silent narrowing (with scope response) are RFC-conformant.
Rejection is a stricter enforcement choice that may be preferable for
clients requesting privileges they should not have.

T-rfc-oauth2 verdict: PASS
T-oauth-oidc verdict: PASS
Overall verdict: PASS
```

**Reason:** RFC 6749 allows both silent scope narrowing (with scope returned in response) and rejection with `error=invalid_scope` when requested scope exceeds registered scope. Rejection is a strict enforcement option and is RFC-compliant. The choice between narrowing and rejection is an implementation decision; both are conformant.

**Phrasing-B:** Client registered for read scope requests admin scope. We reject with error=invalid_scope rather than silently narrowing. Is rejection of an out-of-scope request compliant with RFC 6749, or does the spec require silent narrowing?

---

## Pair[CB4-3.4-435]

**A:** JWT claim validation review. JWT validation library configured to validate: `exp` (expiry), `iat` (issued-at), `aud` (audience), `iss` (issuer), `nbf` (not-before). The `sub` (subject) claim is present in all issued tokens but is not validated by the resource server — the server uses `sub` to identify the user after trust is established but does not validate it against any expected value. Team: "We validate iss, aud, exp, iat, nbf — all the security-relevant claims. sub is just a user identifier, not a security claim."

**B:**
```
Arbiter: RFC-JWT + ASVS-V9-Token

RFC 7519 / V9 claim validation:
  exp: validated ✓ — PASS
  iat: validated ✓
  aud: validated ✓ — PASS (critical)
  iss: validated ✓ — PASS (critical)
  nbf: validated ✓ — PASS

sub validation: RFC 7519 does not require the resource server to validate
  sub against a specific expected value — sub is the subject identifier
  issued by the identity provider. The resource server uses sub to identify
  the user, not to verify it against a pre-known value.
  Unlike iss and aud (which have specific expected values the server should
  validate against), sub identifies WHO the token was issued for — validation
  is implicit in the signature and iss/aud checks.

Team argument ("sub is not a security claim"): partially correct.
  sub is used for authorization decisions (which user is making the request)
  but the resource server does not pre-know expected sub values —
  any sub from a trusted iss/aud-validated token is accepted by design.
  This is the correct behavior.

Assessment: all security-critical claims (iss, aud, exp, iat, nbf) are validated.
sub is correctly used as a user identifier without requiring pre-validation
against an expected value.

Overall verdict: PASS
```

**Reason:** `iss`, `aud`, `exp`, `iat`, and `nbf` are all validated — these are the security-critical claims. `sub` identifies the authenticated user from a trusted token (issuer and audience validated) and is not expected to be pre-validated against a known value by the resource server. The team's characterization is imprecise but the implementation is correct. PASS.

**Phrasing-B:** JWT library validates exp, iat, aud, iss, nbf. The sub claim is used to identify the user but not pre-validated against an expected value. Are all required claims validated, or does the unvalidated sub claim create a finding?

---

## Pair[CB4-3.4-436]

**A:** Comprehensive review exposing a scope enforcement gap alongside correct controls. Application: HTTPS on token endpoint; session ID regenerated on login; RS256 2048-bit annually rotated; PKCE S256 universal; state validated; exact redirect URIs; introspection with Bearer auth; auth codes 5-minute single-use; Argon2id; Vault; Security Champion + checklist; aud validated; iss validated. Resource server: validates signature, exp, aud, iss — but does NOT check the `scope` claim on incoming tokens. Any RS256-signed token with valid exp/aud/iss can call any endpoint regardless of scope.

**B:**
```
Arbiter: Multiple

All prior controls: PASS (verified per prior assessments)

Resource server scope enforcement:
  V10 / OAuth resource server: scope claim not checked → REPROCESS
    Access tokens with scope=read can call write and admin endpoints.
    An OAuth client that received a narrow-scope token (e.g., scope=read:profile)
    can call any resource endpoint — the scope constraint on the token is ignored.
    This is a direct authorization bypass via scope non-enforcement.
    The authorization server correctly issues scoped tokens; the resource server
    nullifies the scope constraint by not enforcing it.
  T-oauth-oidc verdict: REPROCESS

Overall verdict: REPROCESS
(Scope non-enforcement REPROCESS overrides all correct controls)
```

**Reason:** All infrastructure controls correct. The resource server not validating the `scope` claim means scoped tokens provide no authorization constraint — any valid token can call any endpoint regardless of issued scope. This is a direct authorization bypass. One REPROCESS overrides the comprehensive PASSes elsewhere.

**Phrasing-B:** All token, session, authentication, credential, and process controls correct. But resource server doesn't check the scope claim on incoming tokens — any valid token can call any endpoint. Does scope non-enforcement at the resource server create a REPROCESS despite everything else passing?

---

## Pair[CB4-3.4-437]

**A:** .env file pattern review — graduated assessment. Three scenarios:
(A) DB password in `.env` file; `.env` in `.gitignore`; server runs directly from `.env`.
(B) DB password in `.env` file; `.env` in `.gitignore`; CI/CD injects the value from GitHub Actions secrets at deploy time; `.env` is only used locally.
(C) `.env` file used locally; production deployment reads `DB_PASSWORD` from environment variable injected by the platform (not from `.env` file at all); `.env` is never deployed.

**B:**
```
Arbiter: SSDF-PW

Graduated assessment of .env file patterns:

Scenario A — .env in .gitignore; server reads from .env directly:
  PW.1: CONCERN
    Credential is in a plaintext file on the server filesystem.
    .gitignore prevents source control exposure but .env is on the production
    server, readable by any process with filesystem access.
    Not a purpose-built protected credential store.
    Finding: CONCERN — partial protection (source control); production filesystem exposure.

Scenario B — .env in .gitignore; CI/CD injects from GitHub Actions secrets; .env for local only:
  PW.1: PASS for production deployment
    Production credential comes from GitHub Actions secrets (encrypted store) ✓
    Injected at deploy time as environment variable ✓
    .env file is a local development convenience — the production credential
    is never in the .env file.
    Finding: PASS (production credential path is GitHub Secrets)
    Note: local .env with real credentials would be a developer machine concern,
    not a production PW.1 finding.

Scenario C — .env local only; production reads from platform-injected env var:
  PW.1: PASS
    Production deployment does not use .env at all ✓
    Credential comes from platform environment injection ✓
    .env is a local development pattern with no production credential impact.
    Finding: PASS

Summary:
  A: CONCERN (server reads plaintext .env in production)
  B: PASS (production uses CI/CD secrets injection; .env is local-only)
  C: PASS (production uses platform env injection; .env never deployed)
```

**Reason:** The `.env` pattern verdict depends entirely on whether the file is read in production. Scenario A (production server reads .env) is CONCERN. Scenarios B and C (production uses secrets manager injection; .env is local development only) are PASS. The `.gitignore` is irrelevant to the PW.1 assessment — what matters is whether the production credential comes from a protected store.

**Phrasing-B:** Three .env patterns: (A) server reads .env directly in production; (B) .env for local dev, CI/CD secrets inject production credentials; (C) .env never deployed, platform injects at runtime. Are these all CONCERN, or does the production credential source determine the verdict?

---

## Pair[CB4-3.4-438]

**A:** Post-remediation comprehensive — all gaps from ext16 and ext9 cross-source patterns resolved. Prior findings addressed: (1) session fixation: session ID now regenerated on login; (2) token endpoint: direct TLS on service; (3) RSA key: 2048-bit (upgraded from 1024-bit); (4) scope enforcement: resource server now validates scope claim on all requests; (5) .env removed from production: Vault injection instead; (6) introspection: Bearer auth required. All six remediations verified.

**B:**
```
Arbiter: Multiple

(1) V7.2.3 session fixation: new session ID on login ✓ — PASS
(2) RFC 6749 §3.2 token endpoint: direct HTTPS ✓ — PASS
(3) RFC 7518 §3.3 key size: 2048-bit (upgraded) ✓ — PASS
    Key rotation: performed at upgrade ✓
(4) Scope enforcement: resource server validates scope claim ✓ — PASS
    Scope-checked before processing each request ✓
(5) PW.1 .env removed: Vault injection in production ✓ — PASS
(6) RFC 7662 §2.1 introspection: Bearer auth required ✓ — PASS

All six remediations: PASS
Note: prior REPROCESS and CONCERN findings resolved.
Current state assessed on its own merits.

Overall verdict: PASS
```

**Reason:** All six prior findings remediated. Note for key size upgrade: key rotation should be performed when upgrading key size — the old 1024-bit key must be retired and new tokens issued with the new key. Comprehensive post-remediation PASS.

**Phrasing-B:** Six prior findings resolved: session fixation fixed, token endpoint direct HTTPS, key upgraded to 2048-bit with rotation, scope enforcement added, .env replaced with Vault injection, introspection now requires Bearer auth. Does post-remediation yield an overall PASS?

---

## File Summary

| Pair ID | Description | Stratum | Arbiters | Verdict |
|---------|-------------|---------|---------|---------|
| CB4-3.4-429 | Session fixation PASS + .env with .gitignore → CONCERN (source control ≠ credential store) | B/C | V7-Session + SSDF-PW | CONCERN |
| CB4-3.4-430 | Token TLS PASS + Vault PASS + 1024-bit key REPROCESS → REPROCESS overall | C | RFC-OAuth2 + V9-Token + SSDF-PW | REPROCESS |
| CB4-3.4-431 | Scope RFC §3.3 PASS + resource server no scope enforcement → REPROCESS | C/D | RFC-OAuth2 + V10-OAuth + V8-AuthZ | REPROCESS |
| CB4-3.4-432 | Stateless JWT (no fixation surface) + direct TLS + 3-min codes + 2048-bit → PASS | C | Multiple | PASS |
| CB4-3.4-433 | Session fixation REPROCESS + token TLS PASS + 4096-bit quarterly PASS → REPROCESS | C | V7-Session + Multiple | REPROCESS |
| CB4-3.4-434 | Scope rejection (error=invalid_scope) is RFC 6749 compliant → PASS | B/D | RFC-OAuth2 + V10-OAuth | PASS |
| CB4-3.4-435 | JWT claims: iss/aud/exp/iat/nbf validated; sub used not pre-validated → PASS | B/D | RFC-JWT + V9-Token | PASS |
| CB4-3.4-436 | All infrastructure controls PASS + scope non-enforcement → REPROCESS overall | C | Multiple | REPROCESS |
| CB4-3.4-437 | .env graduated: server reads .env in prod=CONCERN; CI/CD secrets injection=PASS; platform env=PASS | D | SSDF-PW | Mixed |
| CB4-3.4-438 | All six prior findings remediated → post-remediation PASS | B | Multiple | PASS |
