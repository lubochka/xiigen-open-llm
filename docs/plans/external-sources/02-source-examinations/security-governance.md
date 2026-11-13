# Source Examination: Security and Governance
## Sources: OWASP-ASVS, NIST-SSDF, CWE-TOP25, IETF-RFC-OAUTH2, IETF-RFC-JWT, IETF-RFC-TLS13
## Priority: 1 (families 5 and 8 — highest gap)
## Date examined: 2026-05-04

---

## 1. Sources Examined

| Source ID | Title | Trust tier | Why included |
|-----------|-------|-----------|-------------|
| OWASP-ASVS | OWASP ASVS v4.0 | 1 | CC-BY-SA 4.0; canonical security verification standard with explicit pass/fail requirements |
| NIST-SSDF | NIST SSDF v1.1 | 1 | Public domain; structured four-group framework with testable practices |
| CWE-TOP25 | CWE Top 25 (2023) | 1 | MITRE free-use; each entry has a concrete flaw and concrete mitigation |
| IETF-RFC-OAUTH2 | RFC 6749 OAuth 2.0 | 1 | IETF Trust; protocol contract with explicit flow requirements and security considerations |
| IETF-RFC-JWT | RFC 7519 JWT | 1 | IETF Trust; token format with explicit validation algorithm |
| IETF-RFC-TLS13 | RFC 8446 TLS 1.3 | 1 | IETF Trust; protocol with explicit cipher suite and negotiation requirements |

---

## 2. Architecture Challenges Found

| Challenge ID | Challenge statement | Source anchor | XIIGEN domain |
|-------------|--------------------|--------------|--------------| 
| SEC-01 | An API endpoint receives a JWT; the service must decide whether to accept it, reject it, or flag it for re-authentication without exposing which claim failed | RFC 7519 §7.2 "Validating a JWT" | E_ARBITER_QUALITY |
| SEC-02 | A client attempts to use an authorization code more than once; the server must decide whether to reject it, revoke the associated tokens, or treat it as an error | RFC 6749 §10.5 "Authorization Code" | E_ARBITER_QUALITY |
| SEC-03 | A service stores passwords; it must choose a hashing scheme that balances security and performance under the system's authentication load | OWASP ASVS V2.4 "Credential Storage" | E_ARBITER_QUALITY |
| SEC-04 | A service receives a request over TLS; it must decide whether to accept the cipher suite offered by the client or renegotiate | RFC 8446 §4.2 "Extensions" and Appendix C | E_ARBITER_QUALITY |
| SEC-05 | A deployment pipeline must decide whether a dependency change in a build artifact is safe to promote or requires human review | NIST SSDF PS.3 "Archive and Protect Each Software Release" | H_GOVERNANCE |
| SEC-06 | A web endpoint handles file uploads; the arbiter must decide whether the upload should be accepted, rejected, or quarantined | OWASP ASVS V12.1 "File Upload Requirements" | E_ARBITER_QUALITY |
| SEC-07 | An OAuth client attempts to use an implicit grant; the system must decide whether to allow it, redirect to authorization code + PKCE (Proof Key for Code Exchange), or reject the request | RFC 6749 §4.2 and OWASP ASVS V3.5 | E_ARBITER_QUALITY |
| SEC-08 | A session token is presented to an endpoint; the arbiter must determine whether the session is still valid, expired, or potentially hijacked | OWASP ASVS V3.2 "Session Binding" | E_ARBITER_QUALITY |
| SEC-09 | A software release is about to be published; the SSDF requires a verification step; the arbiter must decide whether verification is sufficient or requires re-testing | NIST SSDF RV.1 "Identify and Confirm Vulnerabilities" | H_GOVERNANCE |
| SEC-10 | CWE-79 (Cross-Site Scripting) is found during code review; the team must decide whether to block the release, apply output encoding, or defer to a WAF (Web Application Firewall) | CWE-79 "Improper Neutralization of Input During Web Page Generation" | E_ARBITER_QUALITY |
| SEC-11 | CWE-89 (SQL Injection) is found in a data access layer; the arbiter must decide between parameterized queries, stored procedures, or an ORM (Object-Relational Mapping) as a fix | CWE-89 "Improper Neutralization of Special Elements Used in an SQL Command" | E_ARBITER_QUALITY |
| SEC-12 | A microservice needs to communicate with another service; the team must decide whether mutual TLS (mTLS), API key, or OAuth client credentials is the appropriate authentication mechanism | OWASP ASVS V9.2 "Server Communications Security" | E_ARBITER_QUALITY |
| SEC-13 | A build artifact's provenance is unverifiable; the SSDF PO group requires supply chain integrity; the arbiter must decide whether to block, warn, or allow with exception | NIST SSDF PO.1 "Define Security Requirements" and PS.2 | H_GOVERNANCE |
| SEC-14 | A service endpoint is reached without authentication; the arbiter must decide whether the endpoint is correctly public or a misconfiguration | OWASP ASVS V1.2 "Authentication Architecture" | E_ARBITER_QUALITY |
| SEC-15 | A JWT contains a "none" algorithm header; the validator must decide how to handle it | RFC 7519 §6 "Unsecured JWTs" and OWASP JWT Cheat Sheet | E_ARBITER_QUALITY |

---

## 3. Patterns Found

| Pattern ID | Pattern name | When to use | When NOT to use | Source anchor |
|-----------|-------------|------------|----------------|--------------|
| PAT-SEC-01 | JWT validation sequence | Any service accepting bearer tokens for authentication | Do not use when session cookies are the auth mechanism — use session validation instead | RFC 7519 §7.2 |
| PAT-SEC-02 | Authorization code + PKCE | Public clients (mobile apps, SPAs) requesting delegated access | Do not use with confidential clients on trusted backends; client_credentials or client secret is appropriate | RFC 6749 §4.1 + OAuth 2.0 Security Best Current Practice |
| PAT-SEC-03 | Adaptive password hashing (bcrypt/Argon2) | All services storing user passwords at rest | Do not use where timing side-channels on hash comparison are acceptable — always use constant-time comparison | OWASP ASVS V2.4.1–V2.4.5 |
| PAT-SEC-04 | Defense in depth for input validation | All public-facing endpoints receiving untrusted input | Do not use validation alone as the only defense against injection — combine with parameterized queries and output encoding | OWASP ASVS V5.1–V5.3 |
| PAT-SEC-05 | TLS 1.3 only with forward secrecy cipher suites | All service-to-service and client-to-service communication | Do not disable TLS 1.3 in favor of TLS 1.2 unless a specific legacy integration requirement is documented and time-bounded | RFC 8446 Appendix C.3 |
| PAT-SEC-06 | Signed software releases with provenance attestation | Every production artifact pushed to a registry | Do not skip provenance for internal-only artifacts — supply chain attacks target internal tooling as frequently as public ones | NIST SSDF PS.3.1 |

---

## 4. Anti-Patterns Found

| Anti-pattern ID | Bad design | Failure mode | Better design | Source anchor |
|----------------|-----------|-------------|--------------|--------------|
| ANTI-SEC-01 | Accepting JWTs with "alg: none" | An attacker crafts an unsigned token that passes validation because the library accepts the algorithm claim without verification | Reject any JWT where the algorithm header is not in a server-side allowlist; never trust the "alg" header from the token itself | RFC 7519 §6; OWASP JWT Cheat Sheet |
| ANTI-SEC-02 | Using implicit OAuth grant for new applications | Access tokens appear in browser history and referrer headers; the implicit grant has no mechanism to detect token theft | Use authorization code flow with PKCE for all new public clients; the implicit grant is deprecated in OAuth 2.1 | RFC 6749 §4.2; RFC 9700 OAuth 2.1 |
| ANTI-SEC-03 | Storing passwords with MD5 or SHA-1 | Both hashes are fast; an attacker with the hash database can crack most passwords within hours using commodity hardware | Use memory-hard hashing (bcrypt cost factor ≥ 10, Argon2id, or scrypt) so that cracking requires resources proportional to the attacker's budget | OWASP ASVS V2.4.1 |
| ANTI-SEC-04 | Building SQL queries by string concatenation | Any user-controlled input that reaches the concatenation point can change the query's structure, allowing data exfiltration or deletion | Use parameterized queries or prepared statements at every database interaction; the query structure must be fixed before parameters are bound | CWE-89; OWASP ASVS V5.3.4 |
| ANTI-SEC-05 | Hardcoding credentials in source code | Credentials committed to a repository become permanent history entries visible to anyone with repository access, including future contributors | Use environment variables or a secrets manager; never reference credentials by value in code; SSDF PW.6.2 requires this | NIST SSDF PW.6.2; CWE-798 |
| ANTI-SEC-06 | Allowing cross-tenant data access through shared database queries | A query scoped to one tenant returns results from another if the tenant isolation predicate is missing or bypassed | Enforce tenant isolation at the query construction layer with mandatory tenant ID binding; treat missing tenant scope as an error, not a fallback to global | OWASP ASVS V4.2.1 |
| ANTI-SEC-07 | Accepting file uploads without type and size validation | Uploaded files can be malicious executables, excessively large files causing denial of service, or polyglot files that pass format checks but execute as a different type | Validate content type from magic bytes (not the Content-Type header), enforce maximum file size, store uploads outside the web root, and never execute uploaded content | OWASP ASVS V12.1.1–V12.1.3; CWE-434 |

---

## 5. Arbiter Rules Extracted

| Arbiter ID | What it checks | Positive signal | Negative signal | Source anchor |
|-----------|--------------|----------------|----------------|--------------|
| jwtValidationArbiter | JWT is cryptographically valid, not expired, and claims are within expected bounds | Algorithm is in allowlist; signature verifies; exp claim is in the future; iss and aud claims match expected values | alg is "none"; signature fails; token is expired; iss does not match expected issuer | RFC 7519 §7.2 |
| oauthGrantArbiter | OAuth grant type is appropriate for the client type | Client is confidential and uses client_credentials or authorization_code with client secret; public client uses authorization_code + PKCE | Public client uses implicit grant; client_secret appears in browser-visible request; no PKCE verifier on public client | RFC 6749 §2.1; OAuth 2.1 |
| passwordStorageArbiter | Password hashing scheme is resistant to offline brute-force attacks | Memory-hard algorithm (bcrypt, Argon2id, scrypt) with adequate cost factor; constant-time comparison on verification | MD5, SHA-1, SHA-256 without salt; static cost factor that cannot be increased; non-constant-time comparison | OWASP ASVS V2.4.1–V2.4.5 |
| sqlInjectionArbiter | Database queries use parameterized binding, not string concatenation | All queries use parameterized statements or an ORM that enforces binding; no f-string, sprintf, or + operator used in query construction | Any query constructed by string interpolation of user input; dynamic table names or column names without allowlist validation | CWE-89; OWASP ASVS V5.3.4 |
| secretPresenceArbiter | Source code and configuration files contain no hardcoded credentials | All credential references resolve to environment variables or secrets manager keys; no secrets in .env files committed to version control | Any literal password, API key, or private key in source code, config files, or commit history | NIST SSDF PW.6.2; CWE-798 |
| tenantIsolationArbiter | Every multi-tenant query is scoped by tenant ID | Tenant ID is a mandatory binding parameter in every query or row filter; missing tenant ID causes query rejection, not global scope | Tenant ID is optional; absent tenant ID falls back to superadmin context; cross-tenant joins are allowed without explicit authorization | OWASP ASVS V4.2.1 |
| tlsConfigArbiter | TLS configuration allows only secure protocol versions and cipher suites | TLS 1.3 required; cipher suites provide forward secrecy (ECDHE key exchange); certificate chain validation is on | TLS 1.0 or 1.1 accepted; RC4, DES, or 3DES cipher suites configured; certificate validation disabled for internal services | RFC 8446 Appendix C.3; OWASP ASVS V9.1 |
| supplyChainArbiter | Every dependency has a verified, auditable provenance record | Dependencies are pinned to exact versions with hash verification; SBOM (Software Bill of Materials) is generated per release; artifact signatures are verified before installation | Dependencies resolved by version range (^, ~, latest); no SBOM; unsigned artifacts accepted from package registries | NIST SSDF PS.2.1, PS.3.1 |
| fileUploadArbiter | File uploads are validated at content level, not header level | Magic byte validation confirms declared MIME type; file size is bounded before reading; upload is stored outside web root | Content-Type header is trusted without magic byte check; no size limit; upload is stored in the web root or executed | OWASP ASVS V12.1.1–V12.1.3; CWE-434 |

---

## 6. Candidate Training Rows

| Row ID | Dataset family | Domain | Status |
|--------|--------------|--------|-------|
| EXT-SEC-001 | 5 — arbitrateResultI_with_resultsContext | E_ARBITER_QUALITY | draft — JWT validation arbiter |
| EXT-SEC-002 | 5 — arbitrateResultI_with_resultsContext | E_ARBITER_QUALITY | draft — OAuth grant selection |
| EXT-SEC-003 | 5 — arbitrateResultI_with_resultsContext | E_ARBITER_QUALITY | draft — password storage arbiter |
| EXT-SEC-004 | 5 — arbitrateResultI_with_resultsContext | E_ARBITER_QUALITY | draft — SQL injection detection |
| EXT-SEC-005 | 5 — arbitrateResultI_with_resultsContext | E_ARBITER_QUALITY | draft — secret presence detection |
| EXT-SEC-006 | 5 — arbitrateResultI_with_resultsContext | E_ARBITER_QUALITY | draft — tenant isolation check |
| EXT-SEC-007 | 5 — arbitrateResultI_with_resultsContext | E_ARBITER_QUALITY | draft — TLS configuration arbiter |
| EXT-SEC-008 | 5 — arbitrateResultI_with_resultsContext | H_GOVERNANCE | draft — supply chain provenance |
| EXT-SEC-009 | 5 — arbitrateResultI_with_resultsContext | E_ARBITER_QUALITY | draft — file upload validation |
| EXT-SEC-010 | 5 — arbitrateResultI_with_resultsContext | E_ARBITER_QUALITY | draft — JWT "none" algorithm rejection |
| EXT-SEC-011 | 8 — historyAndVersions_to_reprocessingDecision | H_GOVERNANCE | draft — SSDF release verification gate |
| EXT-SEC-012 | 8 — historyAndVersions_to_reprocessingDecision | H_GOVERNANCE | draft — dependency change review decision |
| EXT-SEC-013 | 1 — userPrompt_to_machineCommands | E_ARBITER_QUALITY | draft — security review scope decomposition |
| EXT-SEC-014 | 5 — arbitrateResultI_with_resultsContext | E_ARBITER_QUALITY | draft — implicit grant deprecation decision |
| EXT-SEC-015 | 5 — arbitrateResultI_with_resultsContext | E_ARBITER_QUALITY | draft — session validity arbiter |

---

## 7. Gaps

This examination does not cover:
- OWASP ASVS V7 (Error Handling and Logging) — needed for observability arbiter rows
- OWASP ASVS V13 (API and Web Service) — needed for REST-specific arbiter rows
- NIST SSDF PW group (Produce Well-Secured Software) in detail — produces code-review arbiter rows
- CWE-TOP25 entries 6–25 — covered in batch 2 (EXT-SEC-016 onward)
- IETF RFC 9700 (OAuth 2.1 draft) — supersedes some RFC 6749 sections; needed for updated grant-type rows
