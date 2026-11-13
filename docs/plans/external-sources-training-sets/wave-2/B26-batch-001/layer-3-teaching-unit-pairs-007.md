# Layer 3 — Teaching Unit Pairs (007)
# Batch: B26-batch-001 | Scenario: B26 — Security audit
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU007-001 — Teach: "scan" fires SECURITY in vulnerability context only
  "Scan for SQL injection vulnerabilities." → ["SECURITY"]
  "Scan the logs for performance issues." → [] (gap — no security vocabulary co-occurrence)
  Rule: "scan" fires SECURITY only with security/vulnerability co-occurrence.

## TU007-002 — Teach: CVE, OWASP, NVD abbreviations fire SECURITY
  "Check for CVE advisories." → ["SECURITY"] | "Apply OWASP mitigations." → ["SECURITY", "IMPLEMENT"]
  Rule: Security standard abbreviations (CVE, NVD, OWASP, CWE) fire SECURITY.

## TU007-003 — Teach: "plaintext" fires SECURITY in credential context
  "Confirm passwords are not stored in plaintext." → ["SECURITY"]
  "Confirm error messages don't contain plaintext." → ["SECURITY"] (information disclosure)
  Rule: "plaintext" in any credential/data exposure context fires SECURITY.

## TU007-004 — Teach: "certificate" fires SECURITY in TLS/PKI context
  "Verify TLS certificate validation is enforced." → ["TEST", "SECURITY"]
  "Implement certificate pinning." → ["SECURITY", "IMPLEMENT"]
  Rule: Certificate vocabulary (TLS cert, certificate pinning, certificate validation) fires SECURITY.

## TU007-005 — Teach: "sensitive data" fires SECURITY in audit/exposure context
  "Audit for sensitive data exposure in error responses." → ["SECURITY"]
  "Mask sensitive data in the export." → ["SECURITY", "IMPLEMENT"]
  Rule: "sensitive data" + audit/enforcement action → SECURITY.
