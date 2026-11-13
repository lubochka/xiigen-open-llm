# MANIFEST — B26-batch-001
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06
# Total files: 201

---

## File Count by Layer

| Layer | Files |
|-------|-------|
| Layer 0-A (compiler pairs) | 24 (001–024) |
| Layer 0-B (router pairs) | 24 (001–024) |
| Layer 0-C (knowledge edge pairs) | 24 (001–024) |
| Layer 0-D (flow factory pairs) | 24 (001–024) |
| Layer 1 (transition orchestrator pairs) | 24 (001–024) |
| Layer 2 (nine-node 6.3 pairs) | 24 (001–024) |
| Layer 3 (teaching unit pairs) | 24 (001–024) |
| Held-out and corruption pairs | 24 (001–024) |
| Cross-layer synthesis | 8 (001–008) |
| MANIFEST | 1 |
| **Total** | **201** |

---

## SecurityAuditFlow63 Nine-Node Flows (Layer 2)

| File | Service | Finding | Result |
|------|---------|---------|--------|
| 001 | AdminService | SQL injection in searchAdminUsers | FAIL (1 CRITICAL) |
| 002 | UserService | SQL injection via string interpolation | FAIL (1 CRITICAL) |
| 003 | AuthService | JWT algorithm pinning | PASS |
| 004 | AdminService | Privilege escalation via wildcard grants | FAIL (2 CRITICAL) |
| 005 | PaymentService | PAN in debug logs | PASS_WITH_WARNINGS (1 HIGH) |
| 006 | ProductSearchService | Stored XSS + CSP misconfiguration | FAIL (1 CRITICAL, 1 MEDIUM) |
| 007 | WebhookService | SSRF: internal IP + cloud metadata | FAIL (2 CRITICAL) |
| 008 | SessionService | Stateless token reuse post-logout | PASS_WITH_WARNINGS (1 HIGH) |
| 009 | ConfigService | Hardcoded SMTP password + Stripe key | FAIL (1 HIGH, 1 MEDIUM) |
| 010 | FileUploadService | URL-encoded path traversal bypass | FAIL (1 CRITICAL) |
| 011 | CheckoutService | CSRF on checkout, missing token | FAIL (1 CRITICAL, 2 HIGH) |
| 012 | Dependency scan | 5 CVEs including 1 CRITICAL Newtonsoft.Json | FAIL (1 CRITICAL, 2 HIGH, 2 MEDIUM) |
| 013 | ApiSecurityAudit | Authentication bypass + default-allow posture | FAIL (2 CRITICAL, 2 HIGH) |
| 014 | GdprComplianceReview | Plaintext PII in UserProfile + key in app config | FAIL (1 CRITICAL, 2 HIGH) |
| 015 | PciDssAudit | PAN in debug logs + sequential tokens | FAIL (1 CRITICAL, 2 HIGH) |
| 016 | ThreatModelReview | Elevation of Privilege (STRIDE) — role claim | FAIL (1 CRITICAL, 2 HIGH) |
| 017 | VulnerabilityAssessment | SSRF payment redirect + path traversal | FAIL (2 CRITICAL, 1 HIGH) |
| 018 | SecretRotationAudit | JWT secret + DB credentials over-deadline | FAIL (2 CRITICAL, 3 HIGH) |
| 019 | NetworkSecurityReview | SSH open to internet + Redis externally exposed | FAIL (2 CRITICAL, 3 HIGH) |
| 020 | DependencyVulnerabilityAudit | Newtonsoft.Json CVE-2024-21907 (CRITICAL 9.8) | FAIL (1 CRITICAL, 2 HIGH) |
| 021 | AccessControlAudit | Admin wildcard + IDOR in /admin/users/{id} | FAIL (2 CRITICAL, 2 HIGH) |
| 022 | DataEncryptionReview | Encryption keys in source control + CBC static IV | FAIL (1 CRITICAL, 2 HIGH) |
| 023 | ProbationaryRejected | Write-tests context routed to 6.3 — rejected | REJECTED (probationary_rejected) |
| 024 | Nine-node rule summary | Complete invariants for all nodes | Reference |

---

## Key Teaching Points

1. **"audit" (verb) → SECURITY; "audit" (noun event) → gap** — morphological disambiguation required
2. **SECURITY sub-phase duality**: audit/review/scan context → 6.3; implement/enforce/write-tests context → 5.2
3. **"QA" abbreviation → QA predicate; "quality assurance" spelled out → gap** — strict abbreviation gate
4. **Compliance frameworks are gap**: GDPR, SOC2, ISO 27001, PCI-DSS do not fire SECURITY
5. **"secure" as adjective is gap**: "make it more secure", "ensure it's secure" → []
6. **Attack-vector nouns fire SECURITY**: XSS, SQL injection, CSRF, SSRF, IDOR, XXE, MITM, replay attack, timing attack, brute-force, mass assignment, path traversal, data exfiltration
7. **Privilege escalation and IDOR fire BOTH SECURITY and AUTHORIZATION**
8. **"code review" without security co-occurrence → gap**
9. **Cryptographic algorithms fire SECURITY**: AES, RSA, bcrypt, SHA, HMAC, key rotation, certificate pinning
10. **Risk vocabulary is gap**: risk assessment, risk management, risk register, security risk → [] unless paired with audit/enforce/implement
11. **Authorization header fires API_CONTRACT not AUTHORIZATION** — context disambiguation
12. **FAIL is a valid nine-node output** — security audits commonly produce FAIL; model must not suppress negative results
13. **"QA alone → no contribution TransitionType" in 6.3 context** — SECURITY drives TransitionType
14. **IMPLEMENT overrides DOCUMENT for TransitionType** — IMPLEMENT present → feature_addition
15. **GDPR/PCI-DSS are always gap** — only the technical action vocabulary fires SECURITY
16. **Dual-predicate triggers**: "privilege escalation", "IDOR", "wildcard permission" fire SECURITY+AUTHORIZATION
17. **probationary_rejected**: test-writing context routed to 6.3 is rejected by ContextArbiter
18. **PASS_WITH_WARNINGS is a valid outcome** — not all security audits produce FAIL

---

## Cross-Layer Synthesis Traces

| File | Scenario | Predicate Set |
|------|----------|---------------|
| 001 | SQL injection audit + QA sign-off | SECURITY+QA |
| 002 | Implement rate limiting + audit brute-force exposure | IMPLEMENT+SECURITY |
| 003 | Gap: "Make application more secure overall" | [] → GapHandler |
| 004 | Four-predicate: audit + document + test + QA | SECURITY+DOCUMENT+TEST+QA |
| 005 | API security audit: SECURITY+QA independent routing | SECURITY+QA |
| 006 | QA alone → no contribution TransitionType in 6.3 | QA (alone in 6.3 context) |
| 007 | SECURITY+AUTHORIZATION+QA+DOCUMENT four-predicate trace | SECURITY+AUTHORIZATION+QA+DOCUMENT |
| 008 | Full six-predicate: ARCHITECTURE+IMPLEMENT+SECURITY+AUTHORIZATION+QA+DOCUMENT | All six |
