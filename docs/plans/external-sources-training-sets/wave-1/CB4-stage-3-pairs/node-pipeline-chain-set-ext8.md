# CB4 Stage 3 — Node Pipeline Chain Set Extension 8
# Sequences CB4-3.4-369 through CB4-3.4-376
# Stratum: C — full 9-node pipeline traces with intermediate outputs
# Instances: 16 (8 base pairs × 2 phrasings)
# Running total after this file: 752 instances

---

## Pair[CB4-3.4-369]

**A:** Redirect URI review. Application registers `https://app.example.com/` as its single redirect URI. The authorization server is configured to perform prefix matching: any URI beginning with `https://app.example.com/` is accepted. The actual callback endpoint is `/oauth/callback`. When a user logs in, the authorization code is delivered to the URI specified in the request — if the request URI starts with `https://app.example.com/`, it is accepted. The team: "We control the entire domain, so all paths are ours."

**B:**
```
Node 1 — Domain Splitter
Input: redirect URI registered as root; prefix matching configured; team claims domain ownership justifies it.
Domains detected: [oauth-oidc]
Primary domain: oauth-oidc
Routing: single-domain

Node 2 — Domain Translator
Domain: oauth-oidc
Normative mapping:
  Redirect URI registration: ASVS V10.2.1 (exact match required)
Territory: T-oauth-oidc

Node 3 — Scheme Planner
Territories: [T-oauth-oidc]
Arbiter: ASVS-V10-OAuth
Sub-requirements to evaluate:
  V10.2.1: prefix matching vs exact match — primary question
Plan: evaluate whether prefix matching satisfies exact registration;
  evaluate domain-ownership argument

Node 4 — Domain Processor
T-oauth-oidc analysis:
  Exact URI registered: https://app.example.com/ (root only)
  Actual callback: https://app.example.com/oauth/callback (different path)
  Authorization server matching: prefix match on https://app.example.com/
  V10.2.1 requirement: exact, character-by-character URI match
  Prefix matching analysis:
    - Prefix match accepts: https://app.example.com/oauth/callback ✓ (intended)
    - Prefix match also accepts: https://app.example.com/any-other-path
    - Prefix match also accepts: https://app.example.com/../../../attacker (path traversal)
    - Prefix match also accepts any path the application may expose
      (open redirects, file endpoints, etc.)
  Domain-ownership argument:
    - Domain ownership does not eliminate the threat surface; the attack surface
      exists within the application, not at the DNS level
    - An open redirect at https://app.example.com/redirect?url=... would allow
      the authorization code to be delivered to an attacker-controlled destination
      even if the team "owns" the domain
    - V10.2.1 does not have a domain-ownership exception
  Correct implementation:
    Register https://app.example.com/oauth/callback exactly; configure exact match

Node 5 — Applier
V10.2.1 → REPROCESS
  Evidence: prefix matching configured; root URI registered; actual callback
    at different path; team relies on domain ownership argument

Node 6 — Result Translator
T-oauth-oidc: REPROCESS
  Finding: prefix matching does not satisfy exact match requirement
  Argument rejected: domain ownership does not create a V10.2.1 exception
  Remediation: register exact callback URI; configure exact match algorithm

Node 7 — Arbiter Selector
REPROCESS → ASVS-V10-OAuth

Node 8 — Arbiter Unit (ASVS-V10-OAuth)
V10.2.1: REPROCESS
Argument rejected: "we control the entire domain."

Domain ownership does not satisfy V10.2.1's exact match requirement:
(1) V10.2.1 requires character-exact URI registration. The registered URI
    https://app.example.com/ does not match the callback
    https://app.example.com/oauth/callback — they differ in path.
(2) Prefix matching expands the valid callback surface beyond the intended
    endpoint. Any path the application exposes becomes a potential code recipient.
(3) Open redirects, path traversal, or any exploitable endpoint under the
    prefix receives authorization codes if an attacker crafts the redirect_uri.
(4) Domain ownership reduces some attack vectors (attacker cannot register a
    subdomain) but does not eliminate path-level exploitation within the domain.

Evidence gap: prefix matching configured; exact match required.
Correct implementation: register https://app.example.com/oauth/callback
as a separate exact URI; configure exact match.
Overall verdict: REPROCESS

Node 9 — Formatter
{
  "sequence_id": "CB4-3.4-369",
  "overall_verdict": "REPROCESS",
  "territories": [
    {
      "territory": "T-oauth-oidc",
      "verdict": "REPROCESS",
      "arbiter": "ASVS-V10-OAuth",
      "findings": [
        {
          "requirement": "V10.2.1",
          "verdict": "REPROCESS",
          "evidence": "Prefix matching on https://app.example.com/; exact callback URI not registered",
          "argument_rejected": "Domain ownership does not substitute for exact URI registration"
        }
      ]
    }
  ]
}
```

**Reason:** Prefix matching violates V10.2.1 even when the team controls the domain. The exact callback path must be registered and exact matching enforced. Domain ownership reduces some risks but does not eliminate path-level exploitation within the application — open redirects or other exploitable paths under the prefix would accept authorization codes.

**Phrasing-B:** Our authorization server matches any URI starting with https://app.example.com/ — prefix match on our registered root URI. We control every path on that domain. The real callback is at /oauth/callback. Does prefix matching on a domain we fully own satisfy V10.2.1?

---

## Pair[CB4-3.4-370]

**A:** PKCE code_verifier generation review. Mobile application generates the code_verifier using `UUID.randomUUID().toString()`, producing 36-character values (e.g., `550e8400-e29b-41d4-a716-446655440000`). The authorization server performs full SHA256 verification — computes BASE64URL(SHA256(code_verifier)) and compares to the stored code_challenge. The team states: "The UUID has 122 bits of entropy — that is more than sufficient for security. The length is a formality."

**B:**
```
Node 1 — Domain Splitter
Input: PKCE code_verifier generated as UUID (36 chars); full SHA256 verification; team argues length is formality.
Domains detected: [oauth-oidc, rfc-oauth2]
Primary domain: oauth-oidc
Secondary: rfc-oauth2
Routing: multi-domain

Node 2 — Domain Translator
Domains: oauth-oidc, rfc-oauth2
Normative mapping:
  PKCE: ASVS V10.4.6 (PKCE S256 enforced)
  RFC 7636 §4.1: code_verifier length 43-128 characters
Territories: T-oauth-oidc, T-rfc-oauth2

Node 3 — Scheme Planner
Territories: [T-oauth-oidc, T-rfc-oauth2]
Arbiters: ASVS-V10-OAuth, RFC-OAuth2
Sub-requirements:
  V10.4.6 + RFC 7636 §4.1: 36-char UUID — entropy adequate; length non-compliant
  Team argument: "length is a formality" — evaluate

Node 4 — Domain Processor
T-oauth-oidc + T-rfc-oauth2 analysis:
  UUID v4 properties:
    - Entropy: 122 bits (6 variant/version bits fixed; 122 random) ✓ — more than
      sufficient for PKCE security purposes
    - Length: 36 characters (32 hex + 4 hyphens)
    - RFC 7636 §4.1 minimum: 43 characters
    - Shortfall: 36 < 43 — 7 characters below minimum
  SHA256 verification: present and correct ✓
  Team argument ("length is a formality"):
    - RFC 7636 §4.1 is a normative requirement, not a guideline
    - The minimum length is specified to ensure adequate entropy across all
      generation methods; the UUID entropy happens to exceed the practical
      threshold, but the requirement is explicit
    - "Formality" is not an exception to specification compliance
  Finding level: CONCERN
    - SHA256 verification is correctly implemented (the security-critical step)
    - The gap is the verifier generation length: 36 < 43 as specified by RFC 7636
    - Practical security impact is low (122 bits adequate) but specification
      compliance is not met

Node 5 — Applier
V10.4.6: CONCERN
  SHA256 verification correct ✓; code_verifier length 36 < 43 required ✗
  Finding: specification non-conformance; practical security impact low
RFC 7636 §4.1: CONCERN
  Length non-compliant; entropy adequate

Node 6 — Result Translator
T-oauth-oidc: CONCERN
T-rfc-oauth2: CONCERN
Overall: CONCERN

Node 7 — Arbiter Selector
CONCERN → ASVS-V10-OAuth + RFC-OAuth2

Node 8 — Arbiter Unit (ASVS-V10-OAuth)
V10.4.6: CONCERN
Argument partially acknowledged: 122 bits of entropy exceeds the practical
security threshold. The SHA256 verification step is correctly implemented.

RFC 7636 §4.1 requires code_verifier length of 43-128 characters. UUID string
representation (36 characters) is below this minimum. "Length is a formality"
is not an exception to specification requirements.

Remediation: replace UUID generation with a proper CSPRNG (cryptographically
secure pseudo-random number generator) that produces ≥32 bytes, then
base64url-encode — result is ≥43 characters with equivalent or greater entropy.

Finding level: CONCERN — SHA256 verification correct; gap is verifier generation only.

Arbiter Unit (RFC-OAuth2)
RFC 7636 §4.1: CONCERN — length non-compliant; entropy adequate.
Overall verdict: CONCERN

Node 9 — Formatter
{
  "sequence_id": "CB4-3.4-370",
  "overall_verdict": "CONCERN",
  "territories": [
    {
      "territory": "T-oauth-oidc",
      "verdict": "CONCERN",
      "arbiter": "ASVS-V10-OAuth",
      "findings": [
        {
          "requirement": "V10.4.6",
          "verdict": "CONCERN",
          "evidence": "UUID code_verifier: 36 chars (< 43 minimum); 122-bit entropy adequate; SHA256 verification correct",
          "argument_rejected": "Length is normative, not a formality"
        }
      ]
    },
    {
      "territory": "T-rfc-oauth2",
      "verdict": "CONCERN",
      "arbiter": "RFC-OAuth2",
      "findings": [
        {"requirement": "RFC7636-§4.1", "verdict": "CONCERN", "evidence": "36-char UUID below 43-char minimum"}
      ]
    }
  ]
}
```

**Reason:** The SHA256 verification is correct — the security-critical step is properly implemented. The gap is the code_verifier generation: UUID produces 36 characters, 7 short of the RFC 7636 §4.1 minimum of 43. The team is correct that entropy is adequate (122 bits), but the specification requirement is not a guideline. CONCERN because the verification is correct; the gap is generation length only.

**Phrasing-B:** We generate code_verifier as UUID.randomUUID().toString() — 36 characters, 122 bits of entropy. Authorization server does the full SHA256 hash comparison correctly. The team says length is a formality when entropy is sufficient. Is 36-char UUID a concern or a REPROCESS?

---

## Pair[CB4-3.4-371]

**A:** Privilege escalation review. RBAC system. Users can be assigned roles: Member, Team Lead, Org Admin. Administrators use an API endpoint `POST /admin/users/{id}/roles` to assign roles. The endpoint checks that the calling user is authenticated and has any admin role (Member cannot call this endpoint). However, a Team Lead calling the endpoint can assign any role, including Org Admin, to any user. The system does not check whether the role being assigned is within the caller's privilege level.

**B:**
```
Node 1 — Domain Splitter
Input: RBAC role assignment; Team Lead can assign Org Admin (privilege escalation via unbounded grant).
Domains detected: [asvs-authorization, cwe-authz]
Primary domain: asvs-authorization
Secondary: cwe-authz
Routing: multi-domain

Node 2 — Domain Translator
Domains: asvs-authorization, cwe-authz
Normative mapping:
  Authorization: ASVS V8.1.2 (deny-by-default; privilege bounded by grantor)
  CWE: CWE-269 (improper privilege management)
Territories: T-asvs-authorization, T-cwe-authz

Node 3 — Scheme Planner
Territories: [T-asvs-authorization, T-cwe-authz]
Arbiters: ASVS-V8-AuthZ, CWE-Authz
Sub-requirements:
  V8.1.2: role assignment must be bounded by grantor's privilege level
  CWE-269: system allows a principal to grant privileges exceeding their own

Node 4 — Domain Processor
T-asvs-authorization analysis:
  Authentication check: calling user authenticated ✓
  Role gate: only users with admin role can call endpoint ✓
  Privilege bound check: Team Lead can assign Org Admin ✗
    - A principal should only be able to grant permissions ≤ their own level
    - Team Lead granting Org Admin: grants a role the Team Lead does not hold
    - Exploit path: compromise Team Lead account → escalate to Org Admin →
      full administrative control
  V8.1.2 verdict: REPROCESS — authorization check exists but is incomplete;
    the bound check on the granted role is absent

T-cwe-authz analysis:
  CWE-269 (improper privilege management): REPROCESS
    The system does not verify that the privilege being granted is within
    the grantor's authorization scope. This is a direct CWE-269 finding:
    a privilege management function that does not enforce proper bounds.

Node 5 — Applier
V8.1.2 → REPROCESS
  Evidence: Team Lead can call /admin/users/{id}/roles and assign Org Admin
  Finding: grant not bounded by caller's privilege level
CWE-269 → REPROCESS
  Evidence: privilege escalation path via unbounded role assignment API

Node 6 — Result Translator
T-asvs-authorization: REPROCESS
T-cwe-authz: REPROCESS
Overall: REPROCESS

Node 7 — Arbiter Selector
REPROCESS → ASVS-V8-AuthZ + CWE-Authz

Node 8 — Arbiter Unit (ASVS-V8-AuthZ)
V8.1.2 (privilege management): REPROCESS
The authentication check and admin-role gate are noted. However, V8.1.2
requires that privilege grants are bounded by the grantor's own level.

A Team Lead cannot grant Org Admin because:
(1) Privilege grants should be bounded by the grantor's highest role.
    A Team Lead granting Org Admin grants a privilege they do not possess.
(2) This creates a privilege escalation path: any compromised Team Lead
    account can gain full Org Admin privileges.
(3) Correct implementation: before processing a role assignment, verify that
    the requested role rank ≤ caller's highest role rank.

Arbiter Unit (CWE-Authz)
CWE-269 (improper privilege management): REPROCESS
The role assignment function lacks the bound check that prevents a lower-privilege
principal from granting a higher-privilege role.
Overall verdict: REPROCESS

Node 9 — Formatter
{
  "sequence_id": "CB4-3.4-371",
  "overall_verdict": "REPROCESS",
  "territories": [
    {
      "territory": "T-asvs-authorization",
      "verdict": "REPROCESS",
      "arbiter": "ASVS-V8-AuthZ",
      "findings": [
        {
          "requirement": "V8.1.2",
          "verdict": "REPROCESS",
          "evidence": "Team Lead can assign Org Admin; no privilege bound check on granted role",
          "remediation": "Add check: granted role rank must be ≤ caller's highest role rank"
        }
      ]
    },
    {
      "territory": "T-cwe-authz",
      "verdict": "REPROCESS",
      "arbiter": "CWE-Authz",
      "findings": [
        {
          "requirement": "CWE-269",
          "verdict": "REPROCESS",
          "evidence": "Privilege management function allows grant of roles exceeding caller's own privilege level"
        }
      ]
    }
  ]
}
```

**Reason:** The authentication and admin-role gate exist but are insufficient. V8.1.2 requires that privilege grants are bounded — a Team Lead cannot grant a role they do not hold. The missing bound check creates a privilege escalation path through any compromised Team Lead account. CWE-269 confirms the improper privilege management finding.

**Phrasing-B:** Role assignment API at POST /admin/users/{id}/roles. Only users with an admin role (not plain Members) can call it. But a Team Lead can assign Org Admin to anyone — no check that the role being assigned is within the caller's level. Is the admin-role gate enough, or is the missing bound check a REPROCESS?

---

## Pair[CB4-3.4-372]

**A:** Combined JWT algorithm and session review. Production JWT service uses an allowlist `["RS256", "RS384", "RS512"]`. Session management: 30-minute idle timeout; hard 8-hour absolute session limit; user logout immediately invalidates the server-side session. The application also calls the OIDC provider's back-channel logout endpoint on user logout to terminate the SSO session. Argon2id m=65536, t=3, p=4 for password hashing.

**B:**
```
Node 1 — Domain Splitter
Input: JWT allowlist (RS256/RS384/RS512); session management (idle+absolute+logout); SSO back-channel; Argon2id.
Domains detected: [asvs-token, asvs-session, asvs-authentication]
Primary domain: asvs-token
Additional: asvs-session, asvs-authentication
Routing: multi-domain

Node 2 — Domain Translator
Normative mapping:
  Algorithm allowlist: ASVS V9.1.1 → T-asvs-token
  Session (idle, absolute, logout): ASVS V7.2.4, V7.3.1 → T-asvs-session
  Password hashing: ASVS V6.2.5 → T-asvs-authentication

Node 3 — Scheme Planner
Territories: [T-asvs-token, T-asvs-session, T-asvs-authentication]
Arbiters: ASVS-V9-Token, ASVS-V7-Session, ASVS-V6-Auth

Node 4 — Domain Processor
T-asvs-token:
  V9.1.1: allowlist ["RS256","RS384","RS512"] — closed-world, all asymmetric RSA
    No blocklist logic; only listed algorithms accepted ✓
  Verdict: PASS

T-asvs-session:
  V7.2.4: 30-minute idle timeout ✓ — compliant
  Absolute limit (8 hours): defense-in-depth beyond base requirement ✓
  V7.3.1: immediate server-side invalidation on logout ✓
    + OIDC back-channel logout to terminate SSO session ✓
  Verdict: PASS

T-asvs-authentication:
  V6.2.5: Argon2id m=65536, t=3, p=4
    m=65536 (64 MB) ≥ minimum m=47104 ✓
    t=3 ≥ minimum t=1 ✓
  Verdict: PASS

Node 5 — Applier
All territories: PASS

Node 6 — Result Translator
All territories PASS → overall PASS

Node 7 — Arbiter Selector
All PASS — confirmatory only

Node 8 — Arbiter Units (confirmatory)
V9.1.1: PASS — RS256/RS384/RS512 allowlist correct
V7.2.4: PASS — 30-min idle timeout
V7.3.1: PASS — immediate invalidation + SSO back-channel logout
V6.2.5: PASS — Argon2id m=65536/t=3/p=4

Overall verdict: PASS

Node 9 — Formatter
{
  "sequence_id": "CB4-3.4-372",
  "overall_verdict": "PASS",
  "territories": [
    {"territory": "T-asvs-token", "verdict": "PASS", "arbiter": "ASVS-V9-Token"},
    {"territory": "T-asvs-session", "verdict": "PASS", "arbiter": "ASVS-V7-Session"},
    {"territory": "T-asvs-authentication", "verdict": "PASS", "arbiter": "ASVS-V6-Auth"}
  ]
}
```

**Reason:** All three territories correctly implemented: multi-algorithm RS allowlist satisfies V9.1.1; 30-minute idle + back-channel SSO logout satisfies V7.2.4 + V7.3.1; Argon2id at correct parameters satisfies V6.2.5. Three-territory PASS.

**Phrasing-B:** RS256/RS384/RS512 allowlist, 30-min idle timeout with 8-hour hard limit, immediate server-side logout + OIDC back-channel, Argon2id m=65536/t=3/p=4. Do these three territories all pass?

---

## Pair[CB4-3.4-373]

**A:** JWT logging pipeline trace. Production service logs structured JSON. Application code at the token validation layer logs: `logger.debug("Token validated", token=request.headers['Authorization'])`. The Authorization header contains `Bearer <full_JWT>`. Production log level is INFO — debug statements are suppressed. However, the application is also deployed to a shared staging environment where LOG_LEVEL=DEBUG for troubleshooting. The staging environment uses production-equivalent data (copied from production weekly).

**B:**
```
Node 1 — Domain Splitter
Input: JWT logged at DEBUG in code; prod runs INFO (suppressed); staging runs DEBUG with production data.
Domains detected: [asvs-token, ssdf-process]
Primary domain: asvs-token
Secondary: ssdf-process
Routing: multi-domain

Node 2 — Domain Translator
Normative mapping:
  Token logging: ASVS V9.3.1 (tokens not logged in sensitive contexts)
  Credential exposure in staging: SSDF PW.1 (credential management in non-production)
Territories: T-asvs-token, T-ssdf-process

Node 3 — Scheme Planner
Territories: [T-asvs-token, T-ssdf-process]
Arbiters: ASVS-V9-Token, SSDF-PW
Sub-requirements:
  V9.3.1: JWT logged at DEBUG; staging runs DEBUG with production data → elevated concern
  PW.1: production-data staging environment compounds credential exposure

Node 4 — Domain Processor
T-asvs-token analysis:
  Production: LOG_LEVEL=INFO; JWT not logged ✓ (currently protected)
  Staging: LOG_LEVEL=DEBUG; JWT logged in plaintext ✗
    - Staging uses production-equivalent data (weekly copy)
    - JWTs from the staging copy represent real user sessions (or near-real)
    - JWT logged in staging = production-grade credential exposed in a lower-trust environment
  Risk elevation vs standard DEBUG-disabled finding:
    - Standard finding: DEBUG disabled in prod → CONCERN (latent production risk)
    - This finding: DEBUG enabled in staging WITH production data → REPROCESS
      The credential exposure is happening in staging, not just latent in code
  T-asvs-token verdict: REPROCESS

T-ssdf-process analysis:
  PW.1: production data in staging means JWTs for real users are exposed in
    staging logs — this is an active credential exposure, not a latent risk
  T-ssdf-process verdict: REPROCESS

Node 5 — Applier
V9.3.1 → REPROCESS
  Production: protected by log level
  Staging with prod data: JWT actively logged; real credential exposure occurring
PW.1 → REPROCESS (production-grade credential exposed in lower-trust environment)

Node 6 — Result Translator
T-asvs-token: REPROCESS
T-ssdf-process: REPROCESS
Overall: REPROCESS

Node 7 — Arbiter Selector
REPROCESS → ASVS-V9-Token + SSDF-PW

Node 8 — Arbiter Unit (ASVS-V9-Token)
V9.3.1: REPROCESS
This case differs from a standard "DEBUG disabled in production" finding:
(1) The logging code exists and is active in a staging environment.
(2) That staging environment runs with production-equivalent data (weekly copy).
(3) JWTs logged in staging are effectively production-grade credentials —
    they may still be valid (if not expired), or they reveal real user identity
    and session structure.
(4) Staging is a lower-trust environment with broader access (developers,
    QA engineers, contractors) compared to production.
(5) The credential exposure is not latent — it is happening today in staging.

This justifies REPROCESS: the JWT credential is actively logged in an
environment with production-equivalent data.

Arbiter Unit (SSDF-PW)
PW.1: REPROCESS — production data in staging means credential exposure is
active, not latent. The credential management gap is real, not theoretical.
Overall verdict: REPROCESS

Node 9 — Formatter
{
  "sequence_id": "CB4-3.4-373",
  "overall_verdict": "REPROCESS",
  "territories": [
    {
      "territory": "T-asvs-token",
      "verdict": "REPROCESS",
      "arbiter": "ASVS-V9-Token",
      "findings": [
        {
          "requirement": "V9.3.1",
          "verdict": "REPROCESS",
          "evidence": "JWT logged at DEBUG; staging runs DEBUG with production-equivalent data; credential actively exposed",
          "note": "Distinct from CONCERN case where DEBUG disabled in prod; here exposure is active in staging"
        }
      ]
    },
    {
      "territory": "T-ssdf-process",
      "verdict": "REPROCESS",
      "arbiter": "SSDF-PW",
      "findings": [
        {
          "requirement": "PW.1",
          "verdict": "REPROCESS",
          "evidence": "Production-grade credential exposed in lower-trust staging environment via DEBUG logging"
        }
      ]
    }
  ]
}
```

**Reason:** When DEBUG is disabled in production, JWT logging is CONCERN (latent risk). Here, staging runs at DEBUG with production-equivalent data — the credential exposure is active, not latent. JWTs from a weekly production copy are real credentials exposed in a lower-trust environment with broader personnel access. This graduates from CONCERN to REPROCESS. Staging environment security matters when production data is present.

**Phrasing-B:** JWT logged at DEBUG level in code. Production runs at INFO — suppressed there. But staging runs DEBUG and uses a weekly production database copy. Is staging-with-prod-data JWT logging a CONCERN or REPROCESS? Different from just "DEBUG disabled in prod"?

---

## Pair[CB4-3.4-374]

**A:** Credential management pipeline trace. Application connects to a third-party payment API. The API key value (`sk_live_REDACTED_SECRET_REMOVED`) appears in the following places: (1) hardcoded in `PaymentService.cs` as a class constant; (2) also configured as a GitHub Actions secret `${{ secrets.PAYMENT_API_KEY }}` for CI/CD use. The team states: "We have the key in both places — in code for runtime and in GitHub Secrets for CI. We know the hardcoded one is not ideal but it's a backup."

**B:**
```
Node 1 — Domain Splitter
Input: API key hardcoded in source code AND configured as GitHub Actions secret; team calls hardcode "backup."
Domains detected: [ssdf-process, cwe-authn]
Primary domain: ssdf-process
Secondary: cwe-authn
Routing: multi-domain

Node 2 — Domain Translator
Normative mapping:
  Credential management: SSDF PW.1
  Hardcoded credential: CWE-798 (use of hard-coded credentials)
Territories: T-ssdf-process, T-cwe-authn

Node 3 — Scheme Planner
Territories: [T-ssdf-process, T-cwe-authn]
Arbiters: SSDF-PW, CWE-Authn
Key question: does the presence of a parallel GitHub Actions secret mitigate
the hardcoded credential finding?

Node 4 — Domain Processor
T-ssdf-process analysis:
  GitHub Actions secret ${{ secrets.PAYMENT_API_KEY }}: PASS path ✓
  Hardcoded sk_live_... in PaymentService.cs: REPROCESS ✗
    - The credential value is in version control
    - Anyone with repository read access can retrieve the live API key
    - The key is sk_live_ — a production payment API key (not sandbox)
    - Once committed to source, the key is in git history even if removed later
  "Backup" argument: rejected
    - A "backup" hardcoded credential is a hardcoded credential — the backup
      designation does not reduce the credential exposure
    - Having a compliant path (GitHub Secrets) alongside a non-compliant path
      (hardcoded) does not cancel the non-compliant path
  T-ssdf-process verdict: REPROCESS

T-cwe-authn analysis:
  CWE-798 (hardcoded credentials in source): REPROCESS
    sk_live_ key value in committed source code is a direct CWE-798 finding.
    Severity: CRITICAL — live payment processing API key exposed in source.

Node 5 — Applier
PW.1 → REPROCESS (hardcoded live API key in source)
CWE-798 → REPROCESS (CRITICAL — live payment key in source code)

Node 6 — Result Translator
T-ssdf-process: REPROCESS
T-cwe-authn: REPROCESS (CRITICAL)
Overall: REPROCESS

Node 7 — Arbiter Selector
REPROCESS → SSDF-PW + CWE-Authn

Node 8 — Arbiter Unit (SSDF-PW)
PW.1: REPROCESS
Argument rejected: "GitHub Secrets covers CI; hardcoded is just backup."

A hardcoded production API key in source code is a PW.1 violation regardless
of whether a parallel compliant path exists:
(1) The key value is in source code; any repository reader can retrieve it.
(2) The key is in git history; even if removed in a future commit, the value
    is recoverable from prior commits.
(3) A "backup" credential is a credential — the backup label does not change
    that the live payment key value exists in version-controlled source.

Remediation: remove the hardcoded value from source immediately; use the
GitHub Actions secret exclusively; rotate the key (the current key is compromised
by virtue of being in source history).

Arbiter Unit (CWE-Authn)
CWE-798: REPROCESS (CRITICAL)
Live payment processing API key (`sk_live_...`) hardcoded in source.
Immediate action required: key rotation; credential removal from source.
Overall verdict: REPROCESS

Node 9 — Formatter
{
  "sequence_id": "CB4-3.4-374",
  "overall_verdict": "REPROCESS",
  "territories": [
    {
      "territory": "T-ssdf-process",
      "verdict": "REPROCESS",
      "arbiter": "SSDF-PW",
      "findings": [
        {
          "requirement": "PW.1",
          "verdict": "REPROCESS",
          "evidence": "sk_live_ payment API key hardcoded in PaymentService.cs",
          "argument_rejected": "GitHub Secrets parallel path does not cancel hardcoded credential finding"
        }
      ]
    },
    {
      "territory": "T-cwe-authn",
      "verdict": "REPROCESS",
      "severity": "CRITICAL",
      "arbiter": "CWE-Authn",
      "findings": [
        {
          "requirement": "CWE-798",
          "verdict": "REPROCESS",
          "severity": "CRITICAL",
          "evidence": "Live payment API key value in committed source code"
        }
      ]
    }
  ]
}
```

**Reason:** Having both a compliant path (GitHub Secrets) and a non-compliant path (hardcoded) does not cancel the hardcoded credential finding. The live payment key is in source code — CWE-798 is clear and CRITICAL. The "backup" argument does not reduce the exposure: once in version control, the key is in history and accessible to any repository reader. Immediate remediation: remove from source, rotate the key.

**Phrasing-B:** Payment API key is both hardcoded in source as a backup and configured in GitHub Actions secrets for CI. Both paths exist simultaneously. Does having the key in GitHub Secrets alongside the hardcoded version mitigate the PW.1 / CWE-798 finding?

---

## Pair[CB4-3.4-375]

**A:** Post-remediation review. Prior assessment identified: (1) prefix matching on redirect URIs; (2) UUID code_verifier (36 chars); (3) Team Lead can grant Org Admin; (4) hardcoded API key alongside secrets manager entry. All four gaps remediated: (1) exact URI registered for each callback; (2) code_verifier now generated with CSPRNG — 32 random bytes, base64url-encoded (43 chars minimum); (3) role grant API now checks granted role ≤ caller's highest role; (4) hardcoded key removed from source; key rotated; GitHub Actions secret is the only path.

**B:**
```
Node 1 — Domain Splitter
Input: post-remediation review; four prior findings resolved.
Domains detected: [oauth-oidc, asvs-authorization, cwe-authz, ssdf-process, cwe-authn]
Routing: multi-domain

Node 2 — Domain Translator
Normative mapping:
  V10.2.1: redirect URI exact → T-oauth-oidc
  V10.4.6 / RFC 7636 §4.1: code_verifier → T-oauth-oidc, T-rfc-oauth2
  V8.1.2 / CWE-269: role grant bound → T-asvs-authorization, T-cwe-authz
  PW.1 / CWE-798: hardcoded key → T-ssdf-process, T-cwe-authn

Node 3 — Scheme Planner
All four gaps remediated; plan: verify each remediated control

Node 4 — Domain Processor
V10.2.1: exact callback URIs registered for each client ✓
RFC 7636 §4.1: CSPRNG 32-byte → base64url → 43+ chars ✓
V8.1.2: granted role rank ≤ caller's highest role rank enforced ✓
PW.1 / CWE-798: hardcoded key removed from source; key rotated; GitHub Secrets only ✓
  Note: key rotation is critical — prior key was in source history;
  rotation ensures the exposed key is no longer valid.

Node 5 — Applier
All remediations verified: PASS for each control

Node 6 — Result Translator
All territories PASS

Node 7 — Arbiter Selector
All PASS — confirmatory

Node 8 — Arbiter Units
V10.2.1: PASS — exact URI registration
RFC 7636 §4.1 + V10.4.6: PASS — CSPRNG verifier ≥43 chars + SHA256 verification
V8.1.2 + CWE-269: PASS — grant bounded by caller role
PW.1 + CWE-798: PASS — key removed and rotated; GitHub Secrets exclusively

Note: prior REPROCESS findings do not carry forward after remediation.
Current state is assessed on its own merits.
Overall verdict: PASS

Node 9 — Formatter
{
  "sequence_id": "CB4-3.4-375",
  "overall_verdict": "PASS",
  "territories": [
    {"territory": "T-oauth-oidc", "verdict": "PASS", "arbiter": "ASVS-V10-OAuth"},
    {"territory": "T-rfc-oauth2", "verdict": "PASS", "arbiter": "RFC-OAuth2"},
    {"territory": "T-asvs-authorization", "verdict": "PASS", "arbiter": "ASVS-V8-AuthZ"},
    {"territory": "T-cwe-authz", "verdict": "PASS", "arbiter": "CWE-Authz"},
    {"territory": "T-ssdf-process", "verdict": "PASS", "arbiter": "SSDF-PW"},
    {"territory": "T-cwe-authn", "verdict": "PASS", "arbiter": "CWE-Authn"}
  ]
}
```

**Reason:** All four prior findings remediated. Key point: the hardcoded key remediation requires both removal from source and key rotation (the exposed key is in git history and must be considered compromised). Post-remediation assessment yields PASS — prior findings do not carry forward.

**Phrasing-B:** We fixed all four prior findings: exact redirect URIs, CSPRNG code_verifier at 43+ chars, role grant bounded by caller's level, and hardcoded key removed plus rotated with GitHub Secrets as the only path. Does this complete remediation yield an overall PASS?

---

## Pair[CB4-3.4-376]

**A:** Comprehensive seven-territory review with a single CONCERN. Application implements: exact redirect URIs; PKCE S256 with CSPRNG verifier (48 chars); state validated; 30-min idle + back-channel SSO logout; Argon2id m=65536; /me/permissions endpoint; RS256 allowlist; HashiCorp Vault for all secrets; Security Champion + OWASP checklist; privilege grants bounded by caller role; aud claim validated. One gap: the application's JWT library, when it encounters an unrecognized algorithm in the `alg` header, logs the JWT header payload (base64-decoded) to INFO logs for debugging. This effectively logs the JWT header including subject claim to INFO in production when a malformed token is received.

**B:**
```
Node 1 — Domain Splitter
Input: comprehensive implementation with one gap — unrecognized alg triggers JWT header logging to INFO.
Domains detected: [oauth-oidc, asvs-session, asvs-authentication, asvs-authorization,
  asvs-token, rfc-jwt, ssdf-process]
Routing: multi-domain

Node 2 — Domain Translator
All controls mapped; primary gap:
  JWT header logging on bad alg: V9.3.1 (token data in logs) → T-asvs-token

Node 3 — Scheme Planner
All territories with positive evidence; one gap in T-asvs-token (header logging)

Node 4 — Domain Processor
All correct controls: PASS (each)
T-asvs-token gap:
  RS256 allowlist: PASS ✓
  aud claim validated: PASS ✓
  JWT header logged to INFO on unrecognized alg:
    - Trigger: attacker can deliberately send tokens with unrecognized alg values
    - Effect: JWT header (base64-decoded) logged to INFO → production logs
    - JWT header contains: alg, typ, kid (key ID) — potentially revealing
      key ID information or being used to cause log flooding
    - Note: header is separate from payload (claims); subject/role claims are
      in payload, not header. Header logging is less severe than full JWT logging
      but still: (a) reveals key ID information; (b) can be triggered by attacker;
      (c) logs are not the correct place for token debug data in production
    Finding: CONCERN — header logging (not full JWT); attacker-triggerable in prod
    Level: CONCERN because only the header (not the payload with user claims) is
    logged; the gap is narrower than full JWT logging

Node 5 — Applier
V9.3.1: CONCERN
  Evidence: JWT header logged at INFO in production on unrecognized algorithm;
  attacker-triggerable; header reveals key ID information

Node 6 — Result Translator
All other territories: PASS
T-asvs-token: CONCERN (header logging)
Overall: CONCERN

Node 7 — Arbiter Selector
CONCERN → ASVS-V9-Token for review

Node 8 — Arbiter Unit (ASVS-V9-Token)
V9.3.1: CONCERN
RS256 allowlist correctly rejects unrecognized algorithms — the security check
works. The issue is the logging behavior on rejection:
(1) The JWT header (base64url-decoded) is logged to INFO in production.
(2) This is attacker-triggerable: sending tokens with arbitrary alg values
    causes the header to appear in production INFO logs.
(3) JWT header data (key IDs, algorithm identifiers) should not appear in
    production logs, even if less sensitive than payload claims.
(4) Finding level: CONCERN (not REPROCESS). The full JWT payload is not logged;
    only the header. The actual algorithm allowlist enforcement is correct.

Remediation: on algorithm rejection, log only a sanitized message
("rejected token with unrecognized algorithm"); do not log decoded header data.
Overall verdict: CONCERN

Node 9 — Formatter
{
  "sequence_id": "CB4-3.4-376",
  "overall_verdict": "CONCERN",
  "territories": [
    {"territory": "T-oauth-oidc", "verdict": "PASS", "arbiter": "ASVS-V10-OAuth"},
    {"territory": "T-asvs-session", "verdict": "PASS", "arbiter": "ASVS-V7-Session"},
    {"territory": "T-asvs-authentication", "verdict": "PASS", "arbiter": "ASVS-V6-Auth"},
    {"territory": "T-asvs-authorization", "verdict": "PASS", "arbiter": "ASVS-V8-AuthZ"},
    {
      "territory": "T-asvs-token",
      "verdict": "CONCERN",
      "arbiter": "ASVS-V9-Token",
      "findings": [
        {
          "requirement": "V9.3.1",
          "verdict": "CONCERN",
          "evidence": "JWT header (base64-decoded) logged to INFO on unrecognized alg; attacker-triggerable in production",
          "note": "Header only (not payload); allowlist enforcement correct; logging behavior is the gap"
        }
      ]
    },
    {"territory": "T-ssdf-process", "verdict": "PASS", "arbiter": "SSDF-PW"}
  ]
}
```

**Reason:** All controls correctly implemented except for one specific logging behavior: on algorithm rejection, the JWT header (base64-decoded) is logged at INFO in production. The allowlist enforcement itself is correct — unrecognized algorithms are rejected. The gap is the logging of header data (key IDs) on rejection, which is attacker-triggerable. CONCERN because only the header is logged (not the payload with user claims) and the security check itself works. Overall CONCERN because one CONCERN in any territory overrides all PASSes.

**Phrasing-B:** Everything implemented correctly. One gap: when our JWT library rejects a token with an unrecognized algorithm, it logs the decoded JWT header (alg, kid) to INFO in production. The actual rejection/allowlist enforcement is correct. Is the header-only logging on rejection a CONCERN or a REPROCESS?

---

## File Summary

| Pair ID | Description | Stratum | Arbiter | Verdict |
|---------|-------------|---------|---------|---------|
| CB4-3.4-369 | V10.2.1 prefix matching + domain ownership argument — pipeline trace REPROCESS | C | ASVS-V10-OAuth | REPROCESS |
| CB4-3.4-370 | V10.4.6 UUID verifier "length is formality" — pipeline trace CONCERN | C | ASVS-V10-OAuth + RFC-OAuth2 | CONCERN |
| CB4-3.4-371 | CWE-269 Team Lead grants Org Admin — pipeline trace REPROCESS | C | ASVS-V8-AuthZ + CWE-Authz | REPROCESS |
| CB4-3.4-372 | RS allowlist + 30-min idle + SSO back-channel + Argon2id — 3-territory PASS | C | V9-Token + V7-Session + V6-Auth | PASS |
| CB4-3.4-373 | JWT logged DEBUG in staging-with-prod-data — REPROCESS (vs CONCERN for prod-only) | C | ASVS-V9-Token + SSDF-PW | REPROCESS |
| CB4-3.4-374 | CWE-798 hardcoded live API key + GitHub Secrets parallel — REPROCESS CRITICAL | C | SSDF-PW + CWE-Authn | REPROCESS |
| CB4-3.4-375 | All four ext14 remediations verified — 6-territory PASS | C | Multiple | PASS |
| CB4-3.4-376 | 7-territory nearly-all-PASS; JWT header logged on bad alg → CONCERN overall | C | Multiple | CONCERN |
