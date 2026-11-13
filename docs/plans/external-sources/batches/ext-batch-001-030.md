# External Source Batch — EXT-001 through EXT-030
## Sources: OWASP-ASVS, NIST-SSDF, CWE-TOP25, IETF-RFC-OAUTH2, IETF-RFC-JWT, MICROSOFT-LEARN-ARCH
## Date: 2026-05-04

Each record follows the a→f(b)+c form:
- a = trigger (what the system or user is trying to decide)
- f = function (reasoning applied)
- b = content (the design, code, or configuration being evaluated)
- c = context (constraints, system state, failure conditions)
- acceptedOutput = correct verdict or design
- rejectedOutput = wrong verdict or anti-pattern

---

### EXT-SEC-001 | OWASP-ASVS | JWT Validation — Algorithm Allowlist

**Source:** OWASP ASVS v4.0, requirement V3.5.1; RFC 7519 §6 "Unsecured JWTs"
**Source ID:** OWASP-ASVS, IETF-RFC-JWT
**License:** CC-BY-SA-4.0; IETF Trust
**Trust tier:** 1
**Unit type:** security standard requirement
**Domain:** E_ARBITER_QUALITY
**Family:** 5 — arbitrateResultI_with_resultsContext

**a→f(b)+c:**
- a: A JWT token arrives at an API endpoint and must be validated before the request is processed
- f: Apply the JWT validation algorithm arbiter to determine whether the token should be accepted, rejected, or flagged
- b: `{ "alg": "none", "typ": "JWT" }` (token header); signature field is empty; claims include a valid exp and sub
- c: The service is an authentication-required resource; the validator library supports multiple algorithms including "none"

**Draft row:**
```json
{
  "rowId": "EXT-SEC-001",
  "sourceIds": ["OWASP-ASVS", "IETF-RFC-JWT"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "jwt-validation",
  "architectureChallenge": "A JWT with algorithm 'none' arrives at a protected endpoint. The validator must decide whether to accept, reject, or escalate.",
  "input": {
    "a": "Validate the incoming JWT before allowing access to the protected resource",
    "b": "Token header: {\"alg\": \"none\", \"typ\": \"JWT\"}; signature component is absent; claims are structurally valid",
    "c": "Service is an authentication-required resource; validator library version supports the 'none' algorithm unless explicitly disabled"
  },
  "function": "Apply jwtValidationArbiter: check algorithm is in server-side allowlist before any other validation step",
  "acceptedOutput": {
    "decision": "REJECT. The algorithm 'none' is not in the server-side allowlist. The token must be rejected before any claim validation. Return 401 Unauthorized. The allowlist check must occur before signature verification, not after."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. The claims are structurally valid and not expired. The 'none' algorithm means no signature is needed. Proceed with the request."
  },
  "arbiters": ["jwtValidationArbiter"],
  "whyAccepted": "ASVS V3.5.1 requires that the algorithm is verified against a server-controlled allowlist. RFC 7519 §6 defines 'none' as an unsecured JWT — it is never appropriate for protected resources. Accepting it allows any attacker to forge arbitrary claims.",
  "whyRejected": "The rejected output trusts the 'alg' header from the token itself and interprets absent signature as intentional. This is the canonical JWT 'alg:none' attack vector. The library may be configured to accept it if not explicitly disabled.",
  "sourceAnchors": [
    { "sourceId": "OWASP-ASVS", "anchor": "V3.5.1: Verify that only server-side verified, signed tokens are accepted." },
    { "sourceId": "IETF-RFC-JWT", "anchor": "§6 Unsecured JWTs — defines 'none' as producing no signature; §7.2 Validating a JWT — algorithm must be verified before signature" }
  ],
  "licenseConstraint": "CC-BY-SA-4.0 (OWASP); IETF Trust (RFC)",
  "trainingStatus": "candidate"
}
```

---

### EXT-SEC-002 | IETF-RFC-OAUTH2 | OAuth Grant Type — Public Client

**Source:** RFC 6749 §4.1 Authorization Code Grant, §4.2 Implicit Grant; RFC 6749 §2.1 Client Types
**Source ID:** IETF-RFC-OAUTH2, OWASP-ASVS
**License:** IETF Trust; CC-BY-SA-4.0
**Trust tier:** 1
**Unit type:** protocol standard requirement
**Domain:** E_ARBITER_QUALITY
**Family:** 5 — arbitrateResultI_with_resultsContext

**Draft row:**
```json
{
  "rowId": "EXT-SEC-002",
  "sourceIds": ["IETF-RFC-OAUTH2", "OWASP-ASVS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "oauth-grant-selection",
  "architectureChallenge": "A single-page application needs to obtain an access token for a third-party API. The developer must choose between the implicit grant and the authorization code flow with PKCE.",
  "input": {
    "a": "Choose the OAuth 2.0 grant type for a browser-based single-page application requesting delegated access",
    "b": "Client type: public (cannot securely store a client secret); runtime: browser JavaScript; target: third-party resource server",
    "c": "The application does not control the authorization server; the implicit grant is already implemented in a legacy version of the app"
  },
  "function": "Apply oauthGrantArbiter: evaluate client type and context against the appropriate grant type requirements",
  "acceptedOutput": {
    "decision": "Use authorization code flow with PKCE. Generate a cryptographically random code_verifier, hash it to produce code_challenge, include code_challenge in the authorization request, exchange the authorization code for a token using the code_verifier. Access tokens are never exposed in the browser's address bar or referrer headers."
  },
  "rejectedOutput": {
    "decision": "Use the implicit grant. It is simpler to implement and the legacy code already uses it. Access tokens are returned directly in the redirect URI fragment. No client secret is needed."
  },
  "arbiters": ["oauthGrantArbiter"],
  "whyAccepted": "RFC 6749 §2.1 classifies SPAs as public clients. The implicit grant was designed before PKCE existed. OAuth 2.1 deprecates the implicit grant entirely because tokens in the URI fragment appear in browser history, server logs, and referrer headers. PKCE solves the authorization code interception problem without requiring a client secret.",
  "whyRejected": "The implicit grant is deprecated in OAuth 2.1 and OWASP ASVS V3.5.2. Access tokens in the fragment are visible to JavaScript on the page and in browser history. The argument 'legacy code already uses it' is not a technical justification for continuing an insecure pattern.",
  "sourceAnchors": [
    { "sourceId": "IETF-RFC-OAUTH2", "anchor": "§4.2 Implicit Grant — deprecated in OAuth 2.1; §4.1 Authorization Code Grant; §4.1.1 PKCE extension" },
    { "sourceId": "OWASP-ASVS", "anchor": "V3.5.2: Verify that OAuth 2.0 authorization codes are single-use and bound to PKCE verifier" }
  ],
  "licenseConstraint": "IETF Trust",
  "trainingStatus": "candidate"
}
```

---

### EXT-SEC-003 | OWASP-ASVS | Password Hashing Scheme

**Source:** OWASP ASVS v4.0 V2.4.1–V2.4.5 "Credential Storage"
**Source ID:** OWASP-ASVS
**License:** CC-BY-SA-4.0
**Trust tier:** 1
**Unit type:** security standard requirement
**Domain:** E_ARBITER_QUALITY
**Family:** 5 — arbitrateResultI_with_resultsContext

**Draft row:**
```json
{
  "rowId": "EXT-SEC-003",
  "sourceIds": ["OWASP-ASVS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "credential-storage",
  "architectureChallenge": "A new user registration service must choose a password hashing algorithm. The existing database uses MD5 hashes from a legacy system.",
  "input": {
    "a": "Evaluate the password storage implementation before deploying the new user registration service",
    "b": "Implementation: passwords are hashed with MD5(salt + password); the salt is stored in the same database row; the MD5 hash is stored as a 32-character hex string",
    "c": "Legacy data with MD5 hashes must remain readable during migration; new registrations must use the new scheme immediately"
  },
  "function": "Apply passwordStorageArbiter: evaluate the hashing scheme against ASVS V2.4 requirements",
  "acceptedOutput": {
    "decision": "BLOCK deployment of new registrations with MD5 hashing. Implement bcrypt with cost factor ≥ 10 (or Argon2id) for all new passwords immediately. For legacy MD5 hashes: re-hash on next successful login using the new scheme; after 90 days, force password reset for accounts with unreplaced MD5 hashes. Use constant-time comparison for all hash verification."
  },
  "rejectedOutput": {
    "decision": "MD5 with salt is acceptable for now. The salt prevents rainbow table attacks. Migrate to bcrypt in the next major version when the team has time."
  },
  "arbiters": ["passwordStorageArbiter"],
  "whyAccepted": "ASVS V2.4.1 requires a modern, slow hashing algorithm specifically designed for passwords. MD5 is a fast general-purpose hash; even with a salt, an attacker with GPU hardware can compute billions of MD5s per second. ASVS V2.4.5 requires constant-time comparison to prevent timing attacks. The 'migrate later' approach leaves the credential database at risk until migration completes.",
  "whyRejected": "The rejected output incorrectly treats salting as sufficient. A salt only prevents pre-computed dictionary attacks (rainbow tables). It does not reduce the speed of brute-force attacks against individual hashes. MD5 is designed to be fast; bcrypt, Argon2id, and scrypt are designed to be slow — this is the critical difference.",
  "sourceAnchors": [
    { "sourceId": "OWASP-ASVS", "anchor": "V2.4.1: bcrypt, scrypt, Argon2, or PBKDF2; V2.4.5: constant-time comparison; V2.4.2: iteration count ≥ 10,000 for PBKDF2" }
  ],
  "licenseConstraint": "CC-BY-SA-4.0",
  "trainingStatus": "candidate"
}
```

---

### EXT-SEC-004 | CWE-89 | SQL Injection in Query Construction

**Source:** CWE-89 "Improper Neutralization of Special Elements Used in an SQL Command"; OWASP ASVS V5.3.4
**Source ID:** CWE-TOP25, OWASP-ASVS
**License:** MITRE CWE Terms of Use; CC-BY-SA-4.0
**Trust tier:** 1
**Unit type:** weakness catalog entry
**Domain:** E_ARBITER_QUALITY
**Family:** 5 — arbitrateResultI_with_resultsContext

**Draft row:**
```json
{
  "rowId": "EXT-SEC-004",
  "sourceIds": ["CWE-TOP25", "OWASP-ASVS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "injection-prevention",
  "architectureChallenge": "A code review finds a data access method that constructs SQL queries by string interpolation of user-supplied input.",
  "input": {
    "a": "Review the data access layer for injection vulnerabilities before the release is promoted",
    "b": "Code: `query = f\"SELECT * FROM users WHERE email = '{user_input}'\"` followed by `db.execute(query)`",
    "c": "user_input comes from an HTTP request parameter with no pre-processing; the service runs against a production PostgreSQL database"
  },
  "function": "Apply sqlInjectionArbiter: determine whether the query construction is safe and what remediation is required",
  "acceptedOutput": {
    "decision": "BLOCK release. Replace string interpolation with a parameterized query: `db.execute('SELECT * FROM users WHERE email = %s', (user_input,))`. The query structure is fixed at compile time; the parameter is bound by the database driver, never interpolated into the query string. Apply this change to all query construction points in the file."
  },
  "rejectedOutput": {
    "decision": "Add input sanitization: strip quotes and semicolons from user_input before interpolating. The parameterized query change is in the backlog for next sprint."
  },
  "arbiters": ["sqlInjectionArbiter"],
  "whyAccepted": "CWE-89 is rank 3 in the CWE Top 25 for 2023. String interpolation of untrusted input into a query is the direct cause of SQL injection. Input sanitization (stripping characters) is bypassable through encoding and multi-byte character sequences. Parameterized queries fix the vulnerability structurally — the query structure cannot be modified by any input value.",
  "whyRejected": "Input sanitization is a defense-in-depth measure, not a primary defense against SQL injection. Any sanitization approach that tries to enumerate and remove dangerous characters will be incomplete. The 'backlog for next sprint' deferral leaves a critical vulnerability in production.",
  "sourceAnchors": [
    { "sourceId": "CWE-TOP25", "anchor": "CWE-89 — rank 3 (2023); description: SQL query constructed from user-controlled input" },
    { "sourceId": "OWASP-ASVS", "anchor": "V5.3.4: Verify that parameterized queries or stored procedures are used for all database access" }
  ],
  "licenseConstraint": "MITRE CWE Terms of Use",
  "trainingStatus": "candidate"
}
```

---

### EXT-SEC-005 | NIST-SSDF | Hardcoded Credentials in Source Code

**Source:** NIST SSDF v1.1 PW.6.2 "Implement only secure design principles"; CWE-798 "Use of Hard-coded Credentials"
**Source ID:** NIST-SSDF, CWE-TOP25
**License:** Public domain (NIST); MITRE CWE Terms of Use
**Trust tier:** 1
**Unit type:** framework practice requirement
**Domain:** H_GOVERNANCE
**Family:** 5 — arbitrateResultI_with_resultsContext

**Draft row:**
```json
{
  "rowId": "EXT-SEC-005",
  "sourceIds": ["NIST-SSDF", "CWE-TOP25"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "H_GOVERNANCE",
  "subdomain": "secret-management",
  "architectureChallenge": "A pre-commit scan finds a database connection string with credentials in a committed configuration file.",
  "input": {
    "a": "Evaluate the security posture of the repository before approving the pull request",
    "b": "File: `config/database.yml`, line 14: `password: Passw0rd!MySQL42`, committed to the main branch of a private GitHub repository",
    "c": "The repository has 12 contributors; it is private but will be forked for a new project in 3 months; the password is for a staging database that shares the same schema as production"
  },
  "function": "Apply secretPresenceArbiter: evaluate the credential exposure and determine the required remediation",
  "acceptedOutput": {
    "decision": "BLOCK merge. Take three actions: (1) Immediately rotate the database password — the credential is in git history permanently even if deleted from the file. (2) Remove the hardcoded value and replace with an environment variable reference: `password: <%= ENV['DB_PASSWORD'] %>`. (3) Add a pre-commit hook or CI secret scanner (e.g., truffleHog, git-secrets) to prevent future credential commits. Treat the staging database as compromised until the password is rotated."
  },
  "rejectedOutput": {
    "decision": "Remove the credential from the file in the next commit. Add a note in the README that credentials should go in .env. The repository is private so the exposure is limited."
  },
  "arbiters": ["secretPresenceArbiter"],
  "whyAccepted": "CWE-798 is in the CWE Top 25. Once a credential is committed to git history, `git log` and `git show` expose it permanently — deleting it from the current file does not remove it from history. NIST SSDF PW.6.2 requires that credentials are never present in source code. The 'private repository' argument fails because: (a) git history persists across forks; (b) private repos can be accidentally made public; (c) any contributor with repo access has the credential.",
  "whyRejected": "Removing the credential from the current file without rotating the password and scrubbing git history leaves the credential accessible via git history. A README note does not prevent future credential commits.",
  "sourceAnchors": [
    { "sourceId": "NIST-SSDF", "anchor": "PW.6.2: Implement coding practices that prevent the introduction of flaws that could lead to compromising credentials" },
    { "sourceId": "CWE-TOP25", "anchor": "CWE-798 — Hard-coded credentials; git history is a persistent exposure surface" }
  ],
  "licenseConstraint": "none (NIST public domain)",
  "trainingStatus": "candidate"
}
```

---

### EXT-SEC-006 | OWASP-ASVS | Cross-Tenant Data Access

**Source:** OWASP ASVS v4.0 V4.2.1 "Operation Level Access Control"
**Source ID:** OWASP-ASVS
**License:** CC-BY-SA-4.0
**Trust tier:** 1
**Unit type:** security standard requirement
**Domain:** E_ARBITER_QUALITY
**Family:** 5 — arbitrateResultI_with_resultsContext

**Draft row:**
```json
{
  "rowId": "EXT-SEC-006",
  "sourceIds": ["OWASP-ASVS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "tenant-isolation",
  "architectureChallenge": "A multi-tenant SaaS API endpoint accepts a resource ID in the URL but does not verify that the resource belongs to the authenticated tenant before returning it.",
  "input": {
    "a": "Evaluate the access control implementation of the resource retrieval endpoint",
    "b": "Endpoint: GET /api/reports/{reportId}; implementation: `SELECT * FROM reports WHERE id = :reportId`; the tenant_id of the authenticated user is in the JWT but is not used in the query",
    "c": "The reports table contains data for all tenants in a shared database; resource IDs are sequential integers that are guessable"
  },
  "function": "Apply tenantIsolationArbiter: determine whether the access control is sufficient and what fix is required",
  "acceptedOutput": {
    "decision": "BLOCK release. Add tenant_id as a mandatory binding parameter: `SELECT * FROM reports WHERE id = :reportId AND tenant_id = :tenantId`. The tenant_id must be extracted from the validated JWT, never from a user-supplied request parameter. If the report belongs to a different tenant, return 404 (not 403) to prevent confirmation of resource existence."
  },
  "rejectedOutput": {
    "decision": "Add a check after the query: if the returned report's tenant_id does not match the authenticated user's tenant_id, return 403. This is equivalent and avoids changing the query."
  },
  "arbiters": ["tenantIsolationArbiter"],
  "whyAccepted": "ASVS V4.2.1 requires that access control checks are performed at the data layer, not as an afterthought after retrieval. The post-query check approach has three problems: (1) it reads the data before checking authorization — a timing side-channel; (2) it assumes the query always returns exactly one row; (3) any code path that bypasses the post-query check silently leaks cross-tenant data. The WHERE clause approach is structurally correct and cannot be bypassed.",
  "whyRejected": "The post-query check is logically equivalent in the happy path but is not structurally enforced. It relies on every developer remembering to add the check to every data access method. The WHERE clause approach is the only structurally correct defense.",
  "sourceAnchors": [
    { "sourceId": "OWASP-ASVS", "anchor": "V4.2.1: Verify that sensitive data and APIs are protected against Insecure Direct Object Reference attacks" }
  ],
  "licenseConstraint": "CC-BY-SA-4.0",
  "trainingStatus": "candidate"
}
```

---

### EXT-SEC-007 | IETF-RFC-TLS13 | TLS Configuration

**Source:** RFC 8446 §4.2 Extensions; Appendix C.3 "Implementation Pitfalls"; OWASP ASVS V9.1
**Source ID:** IETF-RFC-TLS13, OWASP-ASVS
**License:** IETF Trust; CC-BY-SA-4.0
**Trust tier:** 1
**Unit type:** protocol standard requirement
**Domain:** E_ARBITER_QUALITY
**Family:** 5 — arbitrateResultI_with_resultsContext

**Draft row:**
```json
{
  "rowId": "EXT-SEC-007",
  "sourceIds": ["IETF-RFC-TLS13", "OWASP-ASVS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "tls-configuration",
  "architectureChallenge": "A security scan finds that an internal microservice-to-microservice channel accepts TLS 1.0 and TLS 1.1 in addition to TLS 1.2 and TLS 1.3, and includes the RC4 cipher suite in its accepted set.",
  "input": {
    "a": "Evaluate the TLS configuration of the internal service mesh before the next infrastructure review",
    "b": "TLS configuration: min_version=TLS1.0, cipher_suites=[TLS_RSA_WITH_RC4_128_SHA, TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256, TLS_AES_256_GCM_SHA384]",
    "c": "The service mesh is internal; all clients are company-controlled Go services running on Kubernetes; no legacy client requires TLS 1.0 or RC4"
  },
  "function": "Apply tlsConfigArbiter: evaluate the TLS version and cipher suite configuration",
  "acceptedOutput": {
    "decision": "REMEDIATE. Set min_version=TLS1.3 and remove all TLS 1.0, 1.1, and 1.2 support. Remove RC4 from the cipher suite list entirely — RC4 is cryptographically broken. For the internal mesh with all Go clients: TLS 1.3 is supported by the Go standard library since Go 1.12; there is no backward compatibility requirement. Configure cipher suites to only TLS_AES_128_GCM_SHA256, TLS_AES_256_GCM_SHA384, TLS_CHACHA20_POLY1305_SHA256 (TLS 1.3 cipher suites)."
  },
  "rejectedOutput": {
    "decision": "The channel is internal so the risk is low. Leave TLS 1.0 enabled for compatibility. RC4 is rarely chosen in practice because modern clients prefer stronger suites. Apply TLS 1.3 enforcement during the next infrastructure cycle."
  },
  "arbiters": ["tlsConfigArbiter"],
  "whyAccepted": "RFC 8446 Appendix C.3 lists implementation pitfalls; maintaining TLS 1.0/1.1 support alongside TLS 1.3 creates a downgrade attack surface. RC4 is documented as cryptographically broken (RFC 7465 prohibits RC4 in TLS). ASVS V9.1.2 requires TLS 1.2+ with PFS cipher suites. The 'internal so low risk' argument fails because internal service mesh traffic is the most valuable target for lateral movement after a perimeter breach.",
  "whyRejected": "The rejected output defers a known-broken configuration. 'Rarely chosen' is not the same as 'not chosen'; cipher suite negotiation falls back to weaker suites when an active attacker can interfere with the negotiation.",
  "sourceAnchors": [
    { "sourceId": "IETF-RFC-TLS13", "anchor": "Appendix C.3 Implementation Pitfalls — downgrade attacks; §4.2.1 supported_versions extension" },
    { "sourceId": "OWASP-ASVS", "anchor": "V9.1.2: TLS 1.2+ with forward secrecy; V9.1.3: verify cipher suites provide forward secrecy" }
  ],
  "licenseConstraint": "IETF Trust",
  "trainingStatus": "candidate"
}
```

---

### EXT-SEC-008 | NIST-SSDF | Supply Chain Provenance Gate

**Source:** NIST SSDF v1.1 PS.2.1 "Verify third-party software"; PS.3.1 "Archive and protect each release"
**Source ID:** NIST-SSDF
**License:** Public domain (NIST)
**Trust tier:** 1
**Unit type:** framework practice requirement
**Domain:** H_GOVERNANCE
**Family:** 8 — historyAndVersions_to_reprocessingDecision

**Draft row:**
```json
{
  "rowId": "EXT-SEC-008",
  "sourceIds": ["NIST-SSDF"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "H_GOVERNANCE",
  "subdomain": "supply-chain-integrity",
  "architectureChallenge": "A CI/CD pipeline is about to promote a build artifact to the production registry. A new dependency was added two hours ago whose hash cannot be verified against the package registry.",
  "input": {
    "a": "Decide whether to promote the build artifact to production or block the promotion pending investigation",
    "b": "Build artifact v2.4.1; added dependency: `fastjson-parser 2.0.1`; package registry hash: unavailable (registry returned 404 for this version); the package was resolved from a mirror cache",
    "c": "The deployment window is open for 30 minutes; delaying past this window requires waiting 48 hours for the next window; the team lead is offline"
  },
  "function": "Apply supplyChainArbiter: evaluate the provenance of the unverified dependency before promotion",
  "acceptedOutput": {
    "decision": "BLOCK promotion. The dependency hash is unverifiable — this is a supply chain integrity failure regardless of the deployment window constraint. Actions: (1) Quarantine the build artifact; (2) Resolve the dependency from its canonical source, not the mirror cache; (3) Verify the hash against the canonical registry; (4) Re-run the build; (5) Escalate to the team lead out-of-band if needed. The 48-hour delay is the correct cost of maintaining supply chain integrity."
  },
  "rejectedOutput": {
    "decision": "Proceed with promotion. The dependency is from a known mirror and the package name is legitimate. The team lead can review the dependency provenance after the deployment window. Document the exception in the incident log."
  },
  "arbiters": ["supplyChainArbiter"],
  "whyAccepted": "NIST SSDF PS.2.1 requires that third-party software is verified before use. An unverifiable hash means the artifact's integrity cannot be confirmed. Supply chain attacks (e.g., dependency confusion, malicious mirror injection) are specifically designed to look legitimate. The deployment window constraint is a scheduling consideration; it is not a valid reason to bypass an integrity check.",
  "whyRejected": "The rejected output accepts a known-unknown risk because of a schedule constraint. 'Legitimate package name' is not a hash verification. Post-deployment review of provenance is not a provenance control — it is an audit trail for a breach that may have already occurred.",
  "sourceAnchors": [
    { "sourceId": "NIST-SSDF", "anchor": "PS.2.1: Verify the integrity and origin of all acquired software before use; PS.3.1: Archive each release and its build attestation" }
  ],
  "licenseConstraint": "none (NIST public domain)",
  "trainingStatus": "candidate"
}
```

---

### EXT-ARCH-001 | MICROSOFT-LEARN-ARCH | Circuit Breaker vs. Direct Retry

**Source:** Microsoft Learn Azure Architecture Patterns — Circuit Breaker pattern §When to use this pattern; §Implementation
**Source ID:** MICROSOFT-LEARN-ARCH
**License:** CC-BY-4.0
**Trust tier:** 1
**Unit type:** architecture pattern page
**Domain:** A_TOPOLOGY
**Family:** 5 — arbitrateResultI_with_resultsContext

**Draft row:**
```json
{
  "rowId": "EXT-ARCH-001",
  "sourceIds": ["MICROSOFT-LEARN-ARCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "A_TOPOLOGY",
  "subdomain": "fault-isolation",
  "architectureChallenge": "An external payment API starts returning 503 responses; the order service must decide how to handle subsequent calls to the payment API.",
  "input": {
    "a": "Handle an external dependency that is intermittently unavailable without cascading failures to the order service",
    "b": "External dependency: payment API; failure pattern: 503 responses on 8 of the last 10 calls over 30 seconds; the order service has 200 concurrent requests waiting for payment responses",
    "c": "Each waiting request holds a thread; thread pool size is 250; payment API SLA is 99.9% (allows ~40 minutes downtime/month); average payment API recovery time from past incidents is 2–5 minutes"
  },
  "function": "Apply circuitBreakerArbiter: determine whether a circuit breaker or direct retry is appropriate",
  "acceptedOutput": {
    "decision": "Open the circuit breaker. Stop sending requests to the payment API immediately. Return a known failure response (e.g., 'payment_unavailable') to the caller so it can queue the order or offer an alternative. Probe the payment API at 30-second intervals with a single test request. When 3 consecutive probes succeed, close the circuit and resume normal operation. This protects the thread pool from exhaustion and prevents retry storms that would worsen the payment API's recovery."
  },
  "rejectedOutput": {
    "decision": "Retry the payment API with exponential backoff on each failed call. Add jitter to spread retries. The payment API will recover on its own."
  },
  "arbiters": ["circuitBreakerArbiter"],
  "whyAccepted": "When a dependency is already returning 80% failures, retrying each request sends 200 concurrent retry chains to a struggling service. The circuit breaker stops the retry storm, protects local resources (thread pool), and lets the payment API recover without additional load. The half-open probe strategy (single test request at intervals) is the correct way to detect recovery.",
  "whyRejected": "Retry with exponential backoff is correct for transient failures (single request timeout). It is wrong for systemic failures (dependency is down). During a 2–5 minute outage with 200 concurrent requests all retrying, exponential backoff produces hundreds of thousands of retry attempts. This is a thundering herd problem that delays the dependency's recovery.",
  "sourceAnchors": [
    { "sourceId": "MICROSOFT-LEARN-ARCH", "anchor": "Circuit Breaker pattern §When to use — use when repeated failure rate is high; §Implementation — open, half-open, closed states" }
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "candidate"
}
```

---

### EXT-ARCH-002 | MICROSOFT-LEARN-ARCH | Bulkhead Thread Pool Isolation

**Source:** Microsoft Learn Azure Architecture Patterns — Bulkhead pattern §Context and problem; §Solution
**Source ID:** MICROSOFT-LEARN-ARCH
**License:** CC-BY-4.0
**Trust tier:** 1
**Unit type:** architecture pattern page
**Domain:** A_TOPOLOGY
**Family:** 5 — arbitrateResultI_with_resultsContext

**Draft row:**
```json
{
  "rowId": "EXT-ARCH-002",
  "sourceIds": ["MICROSOFT-LEARN-ARCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "A_TOPOLOGY",
  "subdomain": "resource-isolation",
  "architectureChallenge": "A reporting workload and an order-processing workload share a single thread pool. A spike in report generation is causing order-processing requests to time out.",
  "input": {
    "a": "Evaluate the resource isolation architecture for the shared application server that hosts both reporting and order processing",
    "b": "Architecture: single thread pool of 100 threads; reporting requests: currently consuming 95 threads due to a scheduled batch job; order processing requests: queued, waiting for threads",
    "c": "Order processing has P99 latency SLA of 500ms; reporting has no latency SLA; both workloads are in the same service; shared thread pool is the default Spring Boot executor"
  },
  "function": "Apply bulkheadIsolationArbiter: evaluate whether the current architecture provides sufficient isolation and what remediation is required",
  "acceptedOutput": {
    "decision": "Implement bulkhead isolation. Define two separate thread pools: order_processing_pool with 60 threads (high-SLA workload) and reporting_pool with 40 threads (no-SLA workload). Order processing requests are never blocked by reporting workload regardless of reporting volume. If reporting saturates its 40 threads, reporting requests queue — order processing continues unaffected."
  },
  "rejectedOutput": {
    "decision": "Add a rate limiter to the reporting endpoint to cap the number of concurrent report generation requests. This prevents reporting from consuming all threads."
  },
  "arbiters": ["bulkheadIsolationArbiter"],
  "whyAccepted": "The bulkhead pattern assigns separate resource pools to separate workloads. A rate limiter reduces reporting throughput but does not prevent the remaining allowed reporting requests from consuming the shared pool. If reporting is rate-limited to 40 concurrent requests and each takes 2 seconds, those 40 requests still hold 40 threads. Bulkheading is the only structural solution.",
  "whyRejected": "Rate limiting reduces the problem but does not solve it. The shared pool means any permitted reporting request can still crowd out order processing. The only structural isolation is separate pools.",
  "sourceAnchors": [
    { "sourceId": "MICROSOFT-LEARN-ARCH", "anchor": "Bulkhead pattern §Context and problem — shared pool exhaustion; §Solution — separate pools per workload type" }
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "candidate"
}
```

---

### EXT-ARCH-003 | OPENTELEMETRY-SPEC | Trace Context Propagation

**Source:** OpenTelemetry Specification — Context Propagation §W3C TraceContext; Semantic Conventions §Attribute naming
**Source ID:** OPENTELEMETRY-SPEC
**License:** Apache-2.0
**Trust tier:** 1
**Unit type:** technical specification
**Domain:** H_GOVERNANCE
**Family:** 5 — arbitrateResultI_with_resultsContext

**Draft row:**
```json
{
  "rowId": "EXT-ARCH-003",
  "sourceIds": ["OPENTELEMETRY-SPEC"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "H_GOVERNANCE",
  "subdomain": "observability-contracts",
  "architectureChallenge": "A service makes outbound HTTP calls but does not propagate the incoming request's trace context. Downstream spans appear as disconnected root spans in the observability platform.",
  "input": {
    "a": "Evaluate the observability implementation of a service that sends HTTP requests to downstream services",
    "b": "Service: user-service; outbound call: HTTP POST to notification-service; trace context: the incoming traceparent header is read but not forwarded on outbound requests; Jaeger shows notification-service spans as unrelated to user-service spans",
    "c": "The team uses OpenTelemetry Go SDK; notifications account for 40% of the latency observed in user-service traces but cannot be correlated"
  },
  "function": "Apply tracePropagationArbiter: evaluate whether trace context is correctly propagated",
  "acceptedOutput": {
    "decision": "Fix propagation. Configure the OpenTelemetry HTTP client instrument to inject the W3C traceparent and tracestate headers into every outbound HTTP request. Use the SDK's built-in propagator (otel.GetTextMapPropagator()) on the outbound HTTP client — do not manually copy headers. Verify in Jaeger that notification-service spans appear as children of user-service spans sharing the same trace ID."
  },
  "rejectedOutput": {
    "decision": "Add a custom X-Trace-ID header that carries the user-service's span ID. Notification-service reads this header and logs it. Correlation can then be done by joining logs in Splunk."
  },
  "arbiters": ["tracePropagationArbiter"],
  "whyAccepted": "The OpenTelemetry specification defines W3C TraceContext (traceparent, tracestate) as the standard propagation format. Using the SDK's built-in propagator is correct because it handles edge cases: B3 multi-format compatibility, baggage propagation, and future propagation format additions. The result is native span parent-child relationships visible in any OTel-compatible backend.",
  "whyRejected": "A custom header requires every tool in the observability stack to understand it. Log correlation in Splunk is a manual, brittle workaround that does not produce the parent-child span hierarchy that makes distributed tracing useful for latency attribution.",
  "sourceAnchors": [
    { "sourceId": "OPENTELEMETRY-SPEC", "anchor": "specification/context/api-propagators.md §TextMapPropagator; specification/trace/sdk.md §Propagation" }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

### EXT-ARCH-004 | GOOGLE-AIP | API Versioning — Breaking Change

**Source:** Google AIP-191 "Versioning" §Breaking changes; AIP-4 §Resource names
**Source ID:** GOOGLE-AIP
**License:** CC-BY-4.0
**Trust tier:** 1
**Unit type:** API governance proposal
**Domain:** H_GOVERNANCE
**Family:** 5 — arbitrateResultI_with_resultsContext

**Draft row:**
```json
{
  "rowId": "EXT-ARCH-004",
  "sourceIds": ["GOOGLE-AIP"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "H_GOVERNANCE",
  "subdomain": "api-versioning",
  "architectureChallenge": "A field in an API response needs to change its type from a string timestamp to a structured Timestamp object. External clients depend on parsing the string format.",
  "input": {
    "a": "Decide how to introduce a breaking field type change in an API that has external consumers",
    "b": "Current API v1: `{\"created_at\": \"2026-01-15T10:30:00Z\"}` (string); required change: `{\"created_at\": {\"seconds\": 1736936200, \"nanos\": 0}}` (structured); 3 known external clients parse the string format",
    "c": "API v1 has been stable for 18 months; clients have no automated migration plan; the structured format is required for the new gRPC gateway"
  },
  "function": "Apply apiVersioningArbiter: determine the correct path for introducing a breaking change",
  "acceptedOutput": {
    "decision": "Introduce API v2 at a new path prefix (/v2/). API v2 uses the structured Timestamp object. API v1 continues to serve the string format. Announce a deprecation timeline: v1 remains active for 12 months. Provide a migration guide. The gRPC gateway targets v2. After 12 months, send a final deprecation notice and sunset v1."
  },
  "rejectedOutput": {
    "decision": "Add the structured field as an additional field alongside the string: `{\"created_at\": \"...\", \"created_at_v2\": {...}}`. Existing clients use the string; new clients use the structured field. Remove the string field in 6 months."
  },
  "arbiters": ["apiVersioningArbiter"],
  "whyAccepted": "AIP-191 states that breaking changes must be introduced in a new major version. A new path prefix (/v2/) is the cleanest signal to clients that the contract has changed. It allows old and new clients to be updated independently.",
  "whyRejected": "Adding a parallel field is not a versioning strategy — it doubles the schema surface and the string removal in 6 months is still a breaking change that occurs without a version signal. Clients that miss the migration window still break.",
  "sourceAnchors": [
    { "sourceId": "GOOGLE-AIP", "anchor": "AIP-191 §Breaking changes — breaking changes must be in a new major version; §Migration — provide migration guides" }
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "candidate"
}
```

---

### EXT-ARCH-005 | GOOGLE-AIP | Long-Running Operation Pattern

**Source:** Google AIP-151 "Long-running operations" §Guidance
**Source ID:** GOOGLE-AIP
**License:** CC-BY-4.0
**Trust tier:** 1
**Unit type:** API governance proposal
**Domain:** A_TOPOLOGY
**Family:** 5 — arbitrateResultI_with_resultsContext

**Draft row:**
```json
{
  "rowId": "EXT-ARCH-005",
  "sourceIds": ["GOOGLE-AIP"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "A_TOPOLOGY",
  "subdomain": "async-operation",
  "architectureChallenge": "A report generation API endpoint takes 45–120 seconds to complete. It is currently a synchronous HTTP POST that holds the connection open until the report is ready.",
  "input": {
    "a": "Redesign the report generation API endpoint to handle 45–120 second operations without blocking",
    "b": "Current design: POST /reports returns 200 with report data after 45–120 seconds; clients time out after 30 seconds in some load balancer configurations; retry storms occur when clients retry timed-out requests",
    "c": "API has 200 concurrent report requests during peak; each report is independently processable; clients are controlled (company-owned mobile apps and web app)"
  },
  "function": "Apply longRunningOpArbiter: evaluate the current design and determine the correct async pattern",
  "acceptedOutput": {
    "decision": "Redesign to the long-running operation pattern. POST /reports returns 202 Accepted with an operation resource: `{\"operationId\": \"op_abc123\", \"status\": \"RUNNING\", \"pollUrl\": \"/operations/op_abc123\"}`. The client polls GET /operations/op_abc123 at 5-second intervals. When status is DONE, the response includes the report URL. The original POST completes in milliseconds. No connection is held open for 45–120 seconds."
  },
  "rejectedOutput": {
    "decision": "Increase the load balancer and client timeout to 180 seconds. Add retry logic with exponential backoff. Document that report generation takes up to 2 minutes."
  },
  "arbiters": ["longRunningOpArbiter"],
  "whyAccepted": "AIP-151 defines the standard for operations that take more than a few seconds. Returning an operation resource immediately decouples the initiation latency from the completion latency. 200 concurrent operations no longer hold 200 connections. Retry storms disappear because a client that retries a POST creates a new operation, not a duplicate of the original.",
  "whyRejected": "Increasing timeouts to 180 seconds still holds the connection for the full duration. Under high load, 200 concurrent long-held connections consume significant server resources. Exponential backoff does not prevent duplicate report creation on retry.",
  "sourceAnchors": [
    { "sourceId": "GOOGLE-AIP", "anchor": "AIP-151 §Guidance — operations that take more than a few seconds should return an Operation resource; §Polling" }
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "candidate"
}
```

---

### EXT-ARCH-006 | GOOGLE-AIP | Idempotent Resource Creation

**Source:** Google AIP-133 "Standard methods: Create" §Duplicate requests
**Source ID:** GOOGLE-AIP
**License:** CC-BY-4.0
**Trust tier:** 1
**Unit type:** API governance proposal
**Domain:** D_EXECUTION_APPLIER
**Family:** 5 — arbitrateResultI_with_resultsContext

**Draft row:**
```json
{
  "rowId": "EXT-ARCH-006",
  "sourceIds": ["GOOGLE-AIP"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "idempotency",
  "architectureChallenge": "A mobile client retries a resource creation request because the first response was lost due to a network timeout. The server processes both requests and creates two duplicate resources.",
  "input": {
    "a": "Evaluate the resource creation endpoint for idempotency and determine whether duplicate creation is acceptable",
    "b": "Endpoint: POST /api/payments; behavior: creates a new payment record on every request; no idempotency key is accepted; the mobile client uses exponential backoff and retries on timeout",
    "c": "Payments are financial records; duplicate creation results in the user being charged twice; the client cannot distinguish a new creation from a retry"
  },
  "function": "Apply idempotentCreateArbiter: evaluate whether the endpoint is safe to retry",
  "acceptedOutput": {
    "decision": "Add idempotency key support. Accept an `Idempotency-Key` header on the POST request. Store the key with the payment record. On a duplicate request with the same key, return the original payment record with 200 OK — do not create a new record. The key must be generated by the client before the first attempt and reused on all retries. Key storage may use a cache with TTL (Time-to-Live) of 24 hours."
  },
  "rejectedOutput": {
    "decision": "Return 409 Conflict on duplicate requests detected by payment amount and timestamp within a 5-minute window. The client treats 409 as success if the payment amount matches."
  },
  "arbiters": ["idempotentCreateArbiter"],
  "whyAccepted": "AIP-133 requires that Create operations with a client-provided resource ID or idempotency key return the existing resource rather than creating a duplicate. An idempotency key is the standard mechanism for this. It handles the case where the first response is lost — the client retries with the same key and receives the same response as the first successful call.",
  "whyRejected": "Amount + timestamp matching is a heuristic, not an idempotency guarantee. Two legitimate separate payments for the same amount within 5 minutes would be incorrectly deduplicated. The window-based approach is not a substitute for a client-provided key.",
  "sourceAnchors": [
    { "sourceId": "GOOGLE-AIP", "anchor": "AIP-133 §Duplicate requests — clients that need idempotency should provide a request ID; return existing resource if request ID matches" }
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "candidate"
}
```

---

### EXT-ARCH-007 | MICROSOFT-LEARN-ARCH | Strangler Fig Migration Decision

**Source:** Microsoft Learn Azure Architecture Patterns — Strangler Fig pattern §When to use; §Issues and considerations
**Source ID:** MICROSOFT-LEARN-ARCH
**License:** CC-BY-4.0
**Trust tier:** 1
**Unit type:** architecture pattern page
**Domain:** A_TOPOLOGY
**Family:** 8 — historyAndVersions_to_reprocessingDecision

**Draft row:**
```json
{
  "rowId": "EXT-ARCH-007",
  "sourceIds": ["MICROSOFT-LEARN-ARCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "A_TOPOLOGY",
  "subdomain": "migration-strategy",
  "architectureChallenge": "A legacy monolith handles all business operations. A big-bang rewrite to microservices has stalled after 18 months with no completed features migrated. The team must decide whether to continue the rewrite or change strategy.",
  "input": {
    "a": "Evaluate the current migration strategy and decide whether to continue, change strategy, or roll back",
    "b": "History: big-bang rewrite started 18 months ago; current state: new microservices architecture is 60% designed but 0% in production; legacy monolith still handles 100% of production traffic; the new system cannot be deployed partially; all teams are blocked on shared infrastructure not yet built",
    "c": "The legacy monolith is stable and business is growing; the 18-month timeline was budgeted; no new features have been delivered in 18 months"
  },
  "function": "Apply stranglerMigrationArbiter: evaluate the migration strategy and decide whether to reprocess (change strategy) or accept (continue)",
  "acceptedOutput": {
    "decision": "REPROCESS: change strategy. Abandon the big-bang rewrite. Apply the Strangler Fig pattern: identify one specific feature or API route that can be independently extracted and deployed. Extract it to a microservice this sprint. Route traffic for that route to the new service via a facade. The legacy monolith continues handling everything else. New features go into the new service, not the legacy system. This delivers production value within weeks, not months."
  },
  "rejectedOutput": {
    "decision": "Continue the big-bang rewrite. 60% of the design is done; abandoning it now wastes 18 months of work. Complete the shared infrastructure first, then deploy all services simultaneously."
  },
  "arbiters": ["stranglerMigrationArbiter"],
  "whyAccepted": "The Strangler Fig pattern is specifically designed to avoid the big-bang migration trap. The 'sunk cost' argument (60% design done) is a cognitive bias — the correct question is which approach delivers production value sooner. The Strangler Fig approach delivers value incrementally; rollback is at the route level, not the whole-system level.",
  "whyRejected": "Completing shared infrastructure first means additional months of zero production value. A simultaneous multi-service deploy with no production track record is a high-risk event. The Strangler Fig pattern exists precisely because simultaneous cutover is the highest-risk migration strategy.",
  "sourceAnchors": [
    { "sourceId": "MICROSOFT-LEARN-ARCH", "anchor": "Strangler Fig pattern §When to use — incremental migration of a legacy system; §Issues — do not attempt to convert all functionality at once" }
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "candidate"
}
```

---

### EXT-DATA-001 | MICROSOFT-LEARN-ARCH | CQRS Fit Decision

**Source:** Microsoft Learn Azure Architecture Patterns — CQRS pattern §When to use this pattern; §When not to use
**Source ID:** MICROSOFT-LEARN-ARCH
**License:** CC-BY-4.0
**Trust tier:** 1
**Unit type:** architecture pattern page
**Domain:** F_REGISTRY_STORAGE
**Family:** 5 — arbitrateResultI_with_resultsContext

**Draft row:**
```json
{
  "rowId": "EXT-DATA-001",
  "sourceIds": ["MICROSOFT-LEARN-ARCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "F_REGISTRY_STORAGE",
  "subdomain": "cqrs-fit",
  "architectureChallenge": "A team proposes using CQRS for a user profile management service that handles CRUD operations on user records with no complex queries and approximately equal read and write loads.",
  "input": {
    "a": "Evaluate whether CQRS is the appropriate architectural pattern for this service",
    "b": "Service: user profile management; operations: create, read, update, delete user records; read/write ratio: approximately 1:1; query complexity: simple lookups by user ID or email; no materialized views or read model projections needed",
    "c": "The team proposes CQRS because they used it in a previous project and are familiar with it; the service has 2 engineers assigned"
  },
  "function": "Apply cqrsFitArbiter: evaluate whether CQRS complexity is justified by the workload characteristics",
  "acceptedOutput": {
    "decision": "CQRS is not appropriate for this service. Use a single repository pattern with one data model. CQRS adds two models (command model, query model), a synchronization mechanism, and eventual consistency that must be managed. None of these are needed when reads and writes have equal load, queries are simple, and no materialized view optimization is required. The added complexity would slow down a 2-engineer team without providing any measurable benefit."
  },
  "rejectedOutput": {
    "decision": "Use CQRS. Separation of command and query concerns is good practice regardless of load characteristics. The team's existing familiarity with the pattern reduces implementation risk."
  },
  "arbiters": ["cqrsFitArbiter"],
  "whyAccepted": "Microsoft Learn explicitly states: 'Avoid CQRS for simple domains.' CQRS is justified when read and write models require different optimization — different scalability, different consistency requirements, or complex read projections. When the workload is symmetric CRUD, CQRS adds two codepaths, a synchronization problem, and eventual consistency semantics for zero benefit. Team familiarity is not a technical justification.",
  "whyRejected": "The rejected output uses pattern familiarity and principle-based reasoning ('separation of concerns') to justify a pattern that is not warranted by the workload. CQRS is not universally better than a simple repository; it is appropriate in specific circumstances.",
  "sourceAnchors": [
    { "sourceId": "MICROSOFT-LEARN-ARCH", "anchor": "CQRS pattern §When not to use — avoid for simple domains where the added complexity of CQRS outweighs its benefits" }
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "candidate"
}
```

---

### EXT-DATA-002 | MICROSOFT-LEARN-ARCH | Transactional Outbox Pattern

**Source:** Microsoft Learn Azure Architecture Patterns — Event Sourcing; .NET Microservices Guide §Implementing event-based communication
**Source ID:** MICROSOFT-LEARN-ARCH, DOTNET-MICROSERVICES
**License:** CC-BY-4.0
**Trust tier:** 1
**Unit type:** architecture pattern page
**Domain:** F_REGISTRY_STORAGE
**Family:** 5 — arbitrateResultI_with_resultsContext

**Draft row:**
```json
{
  "rowId": "EXT-DATA-002",
  "sourceIds": ["MICROSOFT-LEARN-ARCH", "DOTNET-MICROSERVICES"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "F_REGISTRY_STORAGE",
  "subdomain": "event-publishing",
  "architectureChallenge": "An order service must update the order status in its database and publish an OrderPlaced event to a message broker. The current implementation does both operations sequentially without transaction coordination.",
  "input": {
    "a": "Evaluate the event publication mechanism in the order service for consistency guarantees",
    "b": "Implementation: (1) UPDATE orders SET status='placed' WHERE id=:id; (2) broker.publish('OrderPlaced', orderId); two separate operations with no atomic guarantee",
    "c": "The broker publish fails in approximately 0.1% of cases due to network transients; when it fails, the database update has already committed; downstream services waiting for OrderPlaced never receive it"
  },
  "function": "Apply transactionalOutboxArbiter: evaluate the event publishing consistency and determine whether the pattern is correct",
  "acceptedOutput": {
    "decision": "Implement the transactional outbox pattern. In a single database transaction: (1) UPDATE orders SET status='placed'; (2) INSERT INTO outbox (event_type, payload) VALUES ('OrderPlaced', :payload). A separate relay process polls the outbox table and publishes events to the broker; on publish success it marks the outbox row as published. The business state update and event publication are now atomic within the database transaction."
  },
  "rejectedOutput": {
    "decision": "Wrap both operations in a try-catch. If the broker publish fails, roll back the database update and return an error to the caller."
  },
  "arbiters": ["transactionalOutboxArbiter"],
  "whyAccepted": "The outbox pattern decouples the database transaction from the broker publish. The database transaction is the atomicity boundary; the relay handles the eventual broker publish with its own retry logic. This is the standard solution for the dual-write problem in event-driven systems.",
  "whyRejected": "Rolling back the database update on broker failure is not equivalent to atomicity — it adds complexity, doubles the failure surface, and means the business operation fails whenever the broker is temporarily unavailable. The outbox pattern allows the business operation to succeed even when the broker is temporarily down.",
  "sourceAnchors": [
    { "sourceId": "DOTNET-MICROSERVICES", "anchor": "§Implementing event-based communication with the outbox pattern — single transaction for database write and outbox insert" }
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "candidate"
}
```

---

### EXT-PROP-001 | RUST-RFCS | Proposal Completeness Before Review

**Source:** Rust RFC template; Rust RFC 1201 disposition process
**Source ID:** RUST-RFCS
**License:** MIT
**Trust tier:** 2
**Unit type:** governance proposals process
**Domain:** H_GOVERNANCE
**Family:** 5 — arbitrateResultI_with_resultsContext

**Draft row:**
```json
{
  "rowId": "EXT-PROP-001",
  "sourceIds": ["RUST-RFCS"],
  "sourceTrustTier": 2,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "H_GOVERNANCE",
  "subdomain": "proposal-governance",
  "architectureChallenge": "A design proposal for a new system capability has been submitted for team review. The alternatives section is empty and two unresolved questions are marked TODO.",
  "input": {
    "a": "Decide whether the design proposal is ready for team review",
    "b": "Proposal sections: Motivation — complete; Design — complete; Drawbacks — one paragraph; Alternatives — empty; Unresolved questions — two items marked TODO",
    "c": "The team has a scheduled review session in 2 days; the proposal author believes the alternatives are obvious and the unresolved questions will resolve during implementation"
  },
  "function": "Apply proposalCompletenessArbiter: evaluate whether the proposal is complete enough to enter review",
  "acceptedOutput": {
    "decision": "NOT READY for review. The alternatives section must be completed before review. 'Alternatives considered and why rejected' is not optional — it is the primary artifact for future contributors who re-open the same design space. Unresolved questions that will 'resolve during implementation' must either be answered now or explicitly scoped as 'resolved by implementation, decision delegated to implementer' with the delegation boundary stated. Reschedule review after completion."
  },
  "rejectedOutput": {
    "decision": "The proposal is ready for review. The alternatives section can be filled in after the review if the team approves the design. Unresolved questions are normal and will be addressed during implementation."
  },
  "arbiters": ["proposalCompletenessArbiter"],
  "whyAccepted": "The Rust RFC template requires alternatives because reviewers need to understand what design space was explored. A review of an incomplete proposal either results in reviewers re-exploring alternatives the author already considered (wasted effort) or in the team missing a better alternative. Unresolved questions that are deferred to implementation means the team is approving a design without knowing how the ambiguous parts resolve — these may require incompatible choices.",
  "whyRejected": "Accepting an incomplete proposal for review externalizes the completion cost onto the reviewers. The alternatives section being filled in 'after approval' is backward — the review should evaluate whether the chosen design is better than its alternatives.",
  "sourceAnchors": [
    { "sourceId": "RUST-RFCS", "anchor": "RFC template — Alternatives section: 'What other designs have been considered and what is the rationale for not choosing them?'; Unresolved questions — must be resolved before acceptance" }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

### EXT-PROP-002 | KAFKA-KIPS | Protocol Migration Incremental Strategy

**Source:** KIP-500 "Replace ZooKeeper with a Self-Managed Metadata Quorum" §Migration plan; KIP-848 §Rollout
**Source ID:** KAFKA-KIPS
**License:** Apache-2.0
**Trust tier:** 1
**Unit type:** governance proposal
**Domain:** A_TOPOLOGY
**Family:** 8 — historyAndVersions_to_reprocessingDecision

**Draft row:**
```json
{
  "rowId": "EXT-PROP-002",
  "sourceIds": ["KAFKA-KIPS"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "A_TOPOLOGY",
  "subdomain": "protocol-migration",
  "architectureChallenge": "A team is replacing a coordination service that is a mandatory dependency for all broker nodes. The proposed migration plan is a single cutover date where all brokers switch simultaneously.",
  "input": {
    "a": "Evaluate the migration plan for replacing the coordination service and decide whether the cutover strategy is appropriate",
    "b": "Plan: all broker nodes will switch from old coordination protocol to new protocol on a fixed cutover date; the new protocol is not backward compatible with the old; no intermediate mode; cutover requires a cluster restart",
    "c": "The cluster serves 3,000 producers and consumers; the cutover requires 15 minutes of downtime; a rollback requires re-enabling the old protocol and restarting all nodes again (another 15 minutes)"
  },
  "function": "Apply protocolMigrationArbiter: evaluate the cutover strategy against incremental migration principles",
  "acceptedOutput": {
    "decision": "REPROCESS: redesign the migration plan. The single cutover with no intermediate mode is the highest-risk migration strategy. Design a dual-mode operation: brokers can run in migration mode where they accept both old and new protocol clients simultaneously. Migrate clients in batches. Monitor migration progress with per-client telemetry. Remove old protocol support only when all clients are confirmed migrated. This mirrors the approach used in KIP-500: multi-version support was maintained for multiple Kafka releases."
  },
  "rejectedOutput": {
    "decision": "The cutover plan is acceptable. 15 minutes of downtime is within SLA. The rollback path is defined. Execute on the scheduled date."
  },
  "arbiters": ["protocolMigrationArbiter"],
  "whyAccepted": "KIP-500 spent four Kafka major versions in migration mode (Kafka 2.8 through 3.x) specifically to avoid forcing all operators to migrate simultaneously. The dual-mode approach allows lagging clients to continue using the old protocol while new clients adopt the new one. Cutting over simultaneously means any client that is slow to migrate becomes a blocker for the entire cluster.",
  "whyRejected": "The 15-minute downtime and defined rollback path address operational risk but not migration risk. If any of the 3,000 clients has not been updated, the cutover breaks them immediately. Rollback requires identifying which clients failed, which takes longer than 15 minutes with no per-client migration telemetry.",
  "sourceAnchors": [
    { "sourceId": "KAFKA-KIPS", "anchor": "KIP-500 §Migration — multi-version support maintained across Kafka 2.8, 3.0, 3.1, 3.2, 3.3; KIP-848 §Rollout — incremental group protocol migration" }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

### EXT-SEC-011 | NIST-SSDF | Release Verification Gate

**Source:** NIST SSDF v1.1 RV.1 "Identify and Confirm Vulnerabilities in the Software's Release"
**Source ID:** NIST-SSDF
**License:** Public domain (NIST)
**Trust tier:** 1
**Unit type:** framework practice requirement
**Domain:** H_GOVERNANCE
**Family:** 8 — historyAndVersions_to_reprocessingDecision

**Draft row:**
```json
{
  "rowId": "EXT-SEC-011",
  "sourceIds": ["NIST-SSDF"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "H_GOVERNANCE",
  "subdomain": "release-governance",
  "architectureChallenge": "A software release is staged for production. A static analysis scan completed 3 weeks ago found 2 medium-severity findings that were documented and deferred. The team asks whether the release should be promoted as-is or held pending re-scan.",
  "input": {
    "a": "Decide whether to promote the release to production or hold for re-scan",
    "b": "Release: v3.1.2; last scan: 3 weeks ago; findings: 2 medium-severity (SQL injection risk in a non-primary data path; outdated dependency with known CVE); deferral reason: 'low exploitability in current deployment context'",
    "c": "Production release window is open; 14 new commits have been merged since the scan; the outdated dependency has since received a critical CVE (CVSS 9.1) published 5 days ago"
  },
  "function": "Apply supplyChainArbiter + release gate: evaluate whether the release is ready for promotion given the 3-week-old scan and new CVE",
  "acceptedOutput": {
    "decision": "HOLD release. The outdated dependency now has a critical CVE (CVSS 9.1) that did not exist at the time of the deferral. The deferral decision was based on a medium-severity finding — the context has changed materially. Required actions: (1) Update the dependency to a version with the CVE patched; (2) Re-run the scan against all 14 new commits; (3) Resolve both findings before promotion. A 3-week-old scan is not a valid current scan when material security context has changed."
  },
  "rejectedOutput": {
    "decision": "Promote the release. The deferral decision was documented and approved. The new CVE is tracked in the vulnerability management backlog and will be addressed in v3.1.3."
  },
  "arbiters": ["supplyChainArbiter"],
  "whyAccepted": "NIST SSDF RV.1 requires that vulnerabilities are identified and confirmed before release. A deferral decision is valid only when the context that justified deferral remains unchanged. A new CVSS 9.1 CVE is a material change — the 'low exploitability' justification from 3 weeks ago no longer applies. SSDF PS.2.1 requires dependency verification before use.",
  "whyRejected": "The rejected output treats the deferral approval as permanent. Deferral decisions are conditional on the threat context at the time of approval. A new critical CVE invalidates the conditions that justified deferral. Tracking it in the backlog is appropriate for future planning but is not a substitute for fixing a known critical vulnerability before release.",
  "sourceAnchors": [
    { "sourceId": "NIST-SSDF", "anchor": "RV.1.1: Identify vulnerabilities in software before it is deployed; RV.1.3: Confirm whether identified vulnerabilities are exploitable in the deployed context" }
  ],
  "licenseConstraint": "none (NIST public domain)",
  "trainingStatus": "candidate"
}
```
