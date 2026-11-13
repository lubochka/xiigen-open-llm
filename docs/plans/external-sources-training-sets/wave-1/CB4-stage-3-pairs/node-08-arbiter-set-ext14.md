# CB4 Stage 3 — Node 08 Arbiter Set Extension 14
# Sequences CB4-3.4-361 through CB4-3.4-368
# Stratum: A/B/D — baselines, team context, adversarial rejection
# Instances: 16 (8 base pairs × 2 phrasings)
# Running total after this file: 736 instances

---

## Pair[CB4-3.4-361]

**A:** JWT logging review. Production application logs the full Authorization header value (which contains `Bearer <JWT>`) at DEBUG level. DEBUG logging is disabled in production (LOG_LEVEL=INFO). The team states: "DEBUG logging is disabled in production, so the JWT is never actually written to production logs. The code is there but it doesn't execute."

**B:**
```
Arbiter: ASVS-V9-Token

V9.3.1 (sensitive token logging): CONCERN
Argument partially accepted: DEBUG level is disabled in production. At LOG_LEVEL=INFO,
the JWT is not written to logs in the current production configuration.

However, a CONCERN is warranted for the following reasons:
(1) Code that logs a full JWT exists in the application. If LOG_LEVEL is changed
    to DEBUG (during incident investigation, load testing, or misconfiguration),
    the JWT will be logged in plaintext.
(2) Development and staging environments may run at DEBUG level — JWTs logged
    in those environments still represent a credential exposure risk.
(3) The correct implementation is to not log the JWT regardless of log level —
    remove the logging statement or replace it with a non-sensitive token identifier.

Finding level: CONCERN (not REPROCESS). Production is currently protected by
the LOG_LEVEL configuration. The risk is latent: a log level change triggers
immediate production JWT exposure.

Remediation: remove the full JWT from the log statement; log a non-sensitive
identifier (e.g., token fingerprint, jti claim) if logging is needed.
Overall verdict: CONCERN
```

**Reason:** The team correctly notes DEBUG is disabled in production — this is a meaningful mitigation. However, CONCERN is warranted because the JWT logging code exists and can be activated by a log level change (common during incident response). The correct practice is to never log the full JWT regardless of level. CONCERN because production is currently protected; the gap is latent risk.

**Phrasing-B:** We log the full Authorization header (including the JWT) at DEBUG level. In production, DEBUG is disabled (LOG_LEVEL=INFO). The log statement exists in code but doesn't fire in production. Is this a finding, or are we covered because DEBUG is off in production?

---

## Pair[CB4-3.4-362]

**A:** Redirect URI review. Application uses a single-page application (SPA) with dynamic redirect URIs. The registered redirect URI is `https://app.example.com/` (root only). The actual OAuth callback path is `/oauth/callback`. The team has configured the authorization server to match URIs by prefix: any URI beginning with `https://app.example.com/` is accepted. The team states: "We own the entire `app.example.com` domain. Prefix matching against our own domain is safe because we control all paths."

**B:**
```
Arbiter: ASVS-V10-OAuth

V10.2.1: REPROCESS
Argument rejected: "we own the domain, so prefix matching is safe."

V10.2.1 requires character-exact redirect URI matching. Prefix matching violates
this requirement regardless of domain ownership:
(1) V10.2.1 does not have a "domain ownership" exception. The requirement is
    exact match — the full URI, including path, must match character-for-character.
(2) Domain ownership is not the threat model that exact matching addresses.
    Exact matching prevents open redirect exploitation where an attacker crafts
    a URI that passes the prefix check but redirects to an attacker-controlled
    path (e.g., https://app.example.com/../../attacker-path).
(3) On web applications, the application itself may have open redirects,
    injected content, or paths that can be exploited even on a domain the
    team controls.
(4) Correct implementation: register the exact callback URI
    https://app.example.com/oauth/callback and configure exact matching.

Evidence gap: prefix matching configured; exact match required.
Overall verdict: REPROCESS
```

**Reason:** Domain ownership is not an exception to V10.2.1 exact matching. Prefix matching allows any path under the domain to receive authorization codes, including open redirect paths or other exploitable endpoints on the same domain. V10.2.1 requires the full URI including path to be exact.

**Phrasing-B:** We own app.example.com entirely. Our authorization server allows any URI starting with https://app.example.com/ — prefix matching on our own domain. We control all paths under that domain. Is prefix matching on a fully owned domain acceptable for V10.2.1?

---

## Pair[CB4-3.4-363]

**A:** Session management review. Application issues session tokens with a 30-minute idle timeout. After 30 minutes of inactivity, the session is expired server-side and the user must re-authenticate. The application also enforces a hard 8-hour absolute session limit regardless of activity. User logout immediately invalidates the server-side session record.

**B:**
```
Arbiter: ASVS-V7-Session

V7.2.4 (idle timeout): PASS
30-minute idle timeout is a reasonable and compliant idle timeout. The requirement
specifies that idle timeouts be implemented — 30 minutes exceeds the security
value threshold while remaining usable for interactive applications.

V7.3.1 (server-side invalidation on logout): PASS
User logout immediately invalidates the server-side session record. Logout-triggered
invalidation is correctly implemented.

Additional positive note: 8-hour absolute session limit is a defense-in-depth
control beyond the base ASVS requirements — reduces session hijacking exposure
for long-running sessions.

Overall verdict: PASS
```

**Reason:** All session management controls correctly implemented. 30-minute idle timeout satisfies V7.2.4. Immediate server-side invalidation on logout satisfies V7.3.1. The 8-hour absolute limit is defense-in-depth. This is a clean PASS with no gaps.

**Phrasing-B:** 30-minute idle timeout, 8-hour hard session limit, immediate server-side invalidation on logout. Do these session controls satisfy V7.2.4 and V7.3.1?

---

## Pair[CB4-3.4-364]

**A:** Algorithm configuration review. JWT verification library configured with an allowlist `["RS256", "RS384", "RS512"]`. All RSA-PSS and EC algorithms rejected. The team states: "We're future-proofing — RS384 and RS512 are stronger than RS256. Including multiple RSA variants in the allowlist gives us algorithm agility for future migrations."

**B:**
```
Arbiter: ASVS-V9-Token

V9.1.1 (algorithm allowlist): PASS
The allowlist ["RS256", "RS384", "RS512"] correctly implements an allowlist:
(1) Only explicitly enumerated algorithms are accepted — closed-world assumption.
(2) All three are asymmetric RSA PKCS1-v1.5 signature algorithms; none are
    known-weak algorithms.
(3) Blocklisting is not used — the configuration accepts only what is explicitly
    listed.

V9.1.1 is satisfied. An allowlist with multiple strong algorithms is a valid
configuration — V9.1.1 requires an allowlist, not a single algorithm.

Architectural note: including RS384 and RS512 alongside RS256 increases the
allowlist surface slightly (three algorithms instead of one), which can create
algorithm confusion risk if the authorization server is permissive about which
algorithm it uses. Best practice is to use the minimum set of algorithms actually
in use. However, this is an architectural quality observation, not a compliance
violation — the allowlist itself is correctly implemented.

Overall verdict: PASS
```

**Reason:** An allowlist with multiple strong RSA algorithms correctly satisfies V9.1.1. V9.1.1 requires an allowlist (closed-world), not a single-algorithm allowlist. The note about minimizing allowlist size is good practice but not a compliance requirement. PASS.

**Phrasing-B:** JWT allowlist includes RS256, RS384, RS512 — three RSA variants. We're using an allowlist (not a blocklist). Does including three RSA algorithms in the allowlist satisfy V9.1.1, or should we narrow it to one?

---

## Pair[CB4-3.4-365]

**A:** PKCE review. Mobile application (native iOS app). Authorization code flow with PKCE. The code_verifier is generated using `UUID.randomUUID().toString()` which produces values like `550e8400-e29b-41d4-a716-446655440000` (36 characters including hyphens). The code_verifier is sent in the token exchange. The authorization server validates with SHA256 comparison.

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2

V10.4.6 (PKCE with S256): CONCERN
RFC 7636 §4.1 code_verifier requirements:
  - Length: 43-128 characters
  - Character set: unreserved characters A-Z, a-z, 0-9, and "-", ".", "_", "~"
  - Entropy: "high-entropy cryptographic random string"

UUID.randomUUID().toString() analysis:
  - Generated value example: 550e8400-e29b-41d4-a716-446655440000
  - Length: 36 characters — BELOW the RFC 7636 minimum of 43 characters
  - Character set: hex digits and hyphens — hyphens are allowed ("-" is unreserved)
    but hex digit space is limited (0-9, a-f only), reducing entropy per character
  - Entropy: UUID v4 has 122 bits of randomness — entropy is adequate
  - Length violation: 36 < 43 minimum — RFC 7636 §4.1 minimum not met

Finding:
  The UUID approach has adequate entropy (122 bits) but fails the 43-character
  minimum length requirement of RFC 7636 §4.1.
  Finding level: CONCERN — entropy is adequate; the length shortfall is a
  specification compliance gap, not a practical security failure (122 bits
  of entropy exceeds security requirements), but the implementation does not
  satisfy RFC 7636 §4.1 as specified.

Remediation: generate code_verifier using a cryptographically random byte
generator producing at least 32 bytes (43+ characters when base64url-encoded).
Overall verdict: CONCERN
```

**Reason:** UUID v4 has adequate entropy (122 bits) but its string representation is 36 characters — below RFC 7636 §4.1's 43-character minimum. The practical security impact is low (entropy is sufficient), but the implementation does not conform to the specification. CONCERN rather than REPROCESS: the SHA256 verification is correctly implemented; the gap is the verifier generation length, not the verification procedure.

**Phrasing-B:** Mobile app generates PKCE code_verifier using UUID.randomUUID().toString() — gives us 36-character UUIDs with 122 bits of entropy. Authorization server does the full SHA256 comparison. Is the UUID approach compliant with RFC 7636, or is the 36-character length a problem?

---

## Pair[CB4-3.4-366]

**A:** Credential management — environment variable pattern. Application reads its database password from an environment variable: `DB_PASSWORD = os.environ['DB_PASSWORD']`. The environment variable is set by the deployment pipeline (GitHub Actions), which injects the value from GitHub Actions secrets at deployment time. The source code contains only the variable name reference `os.environ['DB_PASSWORD']` — no credential value anywhere in source. The application does not log the environment variable value.

**B:**
```
Arbiter: SSDF-PW

PW.1 (credential management): PASS
This implementation correctly satisfies PW.1:
(1) No credential value in source code — the source contains only the env var
    name, not the value.
(2) The credential value is stored in GitHub Actions secrets — a platform-managed
    encrypted secrets store.
(3) The value is injected at deployment time by the pipeline — the running
    application reads it from the process environment, not from a file in the
    repository.
(4) The credential is not logged.

This pattern (source code references env var name; value injected from CI/CD
secrets at deploy time) is a standard, accepted implementation of PW.1. The
credential lifecycle is: GitHub Actions secrets → deployment pipeline injection
→ process environment → application reads at runtime. At no point does the
credential value appear in source control.

Overall verdict: PASS
```

**Reason:** Reading a credential from an environment variable that was injected by a CI/CD secrets manager (GitHub Actions secrets) satisfies PW.1. The credential value is never in source code — only the variable name reference. This is the standard twelve-factor app pattern and is a correct implementation.

**Phrasing-B:** Database password in os.environ, injected at deploy time from GitHub Actions secrets. Source code only contains `os.environ['DB_PASSWORD']` — no credential value. Not logged. Does reading from an environment variable injected by CI/CD secrets satisfy SSDF PW.1?

---

## Pair[CB4-3.4-367]

**A:** Authorization review. Application has a hierarchical permission model: Org Admin → Team Lead → Member. An Org Admin can grant Team Lead permissions to any user in the organization. A Team Lead can grant Member permissions to users in their team. The application does not prevent a Team Lead from granting permissions that exceed their own role (privilege escalation). A Team Lead can call the API to grant Org Admin role to another user.

**B:**
```
Arbiter: ASVS-V8-AuthZ + CWE-Authz

V8.1.2 (deny-by-default access control): partially addressed — the hierarchy is
implemented but the privilege escalation path is not blocked.

CWE-269 (improper privilege management): REPROCESS
A principal should not be able to grant privileges that exceed their own
authorization level. Allowing a Team Lead to grant Org Admin role:
(1) Violates the principle that privilege grants are bounded by the grantor's
    own privilege level.
(2) Creates a privilege escalation path: any compromised Team Lead account can
    elevate itself or another user to Org Admin.
(3) This is a concrete CWE-269 (improper privilege management) finding —
    the system does not enforce that grants are bounded by the grantor's role.

V8.1.2 verdict: REPROCESS
  Evidence gap: Team Lead can grant Org Admin role; no upper-bound check on
  granted role relative to grantor's role.

CWE-Authz verdict: REPROCESS

Remediation: enforce that any role grant is bounded by the grantor's own
highest role — a Team Lead can only grant roles ≤ Team Lead; only an Org Admin
can grant Org Admin role.
Overall verdict: REPROCESS
```

**Reason:** Privilege grants must be bounded by the grantor's own privilege level — a Team Lead cannot grant a role they do not hold. This is a concrete CWE-269 (improper privilege management) finding. The absence of an upper-bound check on grants creates a privilege escalation path through any compromised Team Lead account. REPROCESS.

**Phrasing-B:** Team Lead can grant Member permissions, which is correct for their role. But Team Lead can also call the role-grant API to assign Org Admin to another user — no boundary check. Is this a privilege escalation finding?

---

## Pair[CB4-3.4-368]

**A:** Comprehensive sub-domain security review covering all new patterns introduced in ext13 and ext14. Application implements: exact redirect URIs (not prefix, not wildcard); PKCE S256 with full verifier hash comparison; state validation; SSO back-channel logout on application logout; JWT signing key in HashiCorp Vault (not Kubernetes Secret); RS256 allowlist (not blocklist); code_verifier generated with 32 cryptographic random bytes base64url-encoded (48 chars — meets 43-128 char requirement); /me/permissions endpoint; Security Champion + OWASP checklist; scrypt N=32768 (meets minimum); privilege grants bounded by grantor role.

**B:**
```
Arbiter: ASVS-V10-OAuth + ASVS-V7-Session + ASVS-V9-Token + SSDF-PW +
         ASVS-V8-AuthZ + ASVS-V6-Auth + CWE-Authz

T-oauth-oidc:
  V10.2.1: exact redirect URIs ✓ — PASS
  V10.4.6: PKCE S256 with full verifier hash comparison ✓ — PASS
  V10.2.6: state validated ✓ — PASS
  code_verifier generation: 48 chars (base64url of 32 random bytes) ✓
    — meets RFC 7636 §4.1 length (43-128) and entropy requirements — PASS

T-asvs-session:
  V7.3.1: SSO back-channel logout terminates SSO session on app logout ✓ — PASS

T-asvs-token:
  V9.1.1: RS256 allowlist (not blocklist) ✓ — PASS
  JWT signing key in Vault ✓ (supports PW.1)

T-ssdf-process:
  PW.1: JWT signing key in HashiCorp Vault ✓ — PASS
  PW.3.2: Security Champion + OWASP checklist ✓ — PASS

T-asvs-authorization:
  V8.2.1: /me/permissions endpoint ✓ — PASS
  V8.1.2: privilege grants bounded by grantor role ✓ — no privilege escalation
    — PASS

T-asvs-authentication:
  V6.2.5: scrypt N=32768, r=8, p=1 ✓ — N=32768 meets OWASP minimum — PASS

All territories: PASS
Overall verdict: PASS

Note: each control addresses a specific pattern introduced in ext13/ext14 as
a finding. The remediation of each pattern — prefix matching → exact match,
K8s Secret → Vault, blocklist → allowlist, UUID verifier → proper CSPRNG,
403-only → /me/permissions, no security structure → Security Champion,
scrypt N=1024 → N=32768, unbounded grants → bounded grants — yields PASS
on reassessment.
```

**Reason:** Comprehensive PASS covering all new sub-domain patterns. Each control directly addresses a prior finding pattern: exact URIs (not prefix), proper CSPRNG verifier length, SSO back-channel, Vault (not K8s Secret), allowlist (not blocklist), /me/permissions, Security Champion, scrypt N=32768, bounded privilege grants. All patterns resolved to PASS state.

**Phrasing-B:** We implemented all remediations from recent findings: exact redirect URIs, proper PKCE verifier generation (48-char CSPRNG), SSO back-channel logout, JWT key in Vault, RS256 allowlist, /me/permissions, Security Champion + OWASP checklist, scrypt N=32768, privilege grants bounded by grantor role. Does this comprehensive implementation pass all territories?

---

## File Summary

| Pair ID | Description | Stratum | Arbiter | Verdict |
|---------|-------------|---------|---------|---------|
| CB4-3.4-361 | JWT logged at DEBUG level; DEBUG disabled in prod — CONCERN (latent) | B/D | ASVS-V9-Token | CONCERN |
| CB4-3.4-362 | V10.2.1 prefix matching "we own the domain" — REPROCESS | A/D | ASVS-V10-OAuth | REPROCESS |
| CB4-3.4-363 | V7.2.4 30-min idle + V7.3.1 immediate logout invalidation — PASS | A | ASVS-V7-Session | PASS |
| CB4-3.4-364 | V9.1.1 allowlist ["RS256", "RS384", "RS512"] — PASS (multi-alg allowlist OK) | B | ASVS-V9-Token | PASS |
| CB4-3.4-365 | V10.4.6 UUID code_verifier (36 chars < 43 minimum) — CONCERN | D | ASVS-V10-OAuth + RFC-OAuth2 | CONCERN |
| CB4-3.4-366 | PW.1 os.environ from CI/CD secrets injection — PASS | A | SSDF-PW | PASS |
| CB4-3.4-367 | CWE-269 privilege escalation: Team Lead grants Org Admin — REPROCESS | A | ASVS-V8-AuthZ + CWE-Authz | REPROCESS |
| CB4-3.4-368 | All ext13/ext14 remediations correctly implemented — 7-territory PASS | B | Multiple | PASS |
