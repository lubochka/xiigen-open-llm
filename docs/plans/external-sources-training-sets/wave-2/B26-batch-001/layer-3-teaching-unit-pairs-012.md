# Layer 3 — Teaching Unit Pairs (012)
# Batch: B26-batch-001 | Scenario: B26 — Security audit
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU012-001 — Teach: "security regression test" fires SECURITY+TEST+IMPLEMENT
  "Write security regression tests for previously patched XSS vulnerabilities." → ["SECURITY", "IMPLEMENT", "TEST"]
  Rule: Security regression tests fire SECURITY (attack context), TEST (test noun), IMPLEMENT ("write"). Three predicates.

## TU012-002 — Teach: "hardening" fires SECURITY only with "security" co-occurrence
  "Apply security hardening to the database." → ["SECURITY", "IMPLEMENT"]
  "Apply configuration hardening." → [] (gap — "hardening" without "security" co-occurrence is gap)
  Rule: "security hardening" fires SECURITY. "hardening" alone is borderline gap without "security".

## TU012-003 — Teach: "data masking" vs "data anonymization" — both fire SECURITY
  "Implement data masking for PII in exports." → ["SECURITY", "IMPLEMENT"]
  "Audit data anonymization in the reporting module." → ["SECURITY"]
  Rule: Data protection vocabulary (masking, anonymization, pseudonymization) fires SECURITY.

## TU012-004 — Teach: "QA engineer" fires QA; "QA review" fires QA
  "The QA engineer should review the security controls." → ["QA"] (QA abbreviation fires; "review" is gap)
  "Submit a QA review before deployment." → ["QA"] ("deployment" is gap)
  Rule: "QA" in any compound form fires QA predicate.

## TU012-005 — Teach: security audit findings routing — FAIL is valid output
  A SecurityAuditFlow63 with FAIL verdict is correct behavior.
  FAIL = vulnerabilities found | PASS = no vulnerabilities | PASS_WITH_WARNINGS = minor findings
  Rule: FAIL is the expected output when the audit finds vulnerabilities. It is not a system error.
