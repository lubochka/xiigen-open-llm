# Layer 0-B — Router Pairs (030) — OWASPTop10Review Routing
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: router

---

## R030-001 — OWASP Top 10 review → SECURITY → 6.3

R030-001 test: "Review the application against the OWASP Top 10 list."
  SECURITY → 6.3 (OWASP Top 10 is strong enough co-occurrence to push "review" past gap threshold)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## R030-002 — OWASP A01 + AUTHORIZATION → dual routing 6.3

R030-002 test: "Audit OWASP A01 (Broken Access Control) — check IDOR and privilege escalation."
  SECURITY → 6.3 | AUTHORIZATION → 6.3 (IDOR + privilege escalation = dual-trigger)
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"] | SubPhases: [6.3, 6.3]

---

## R030-003 — OWASP A02 cryptographic failures → SECURITY → 6.3

R030-003 test: "Audit cipher suites and certificate validation for OWASP A02."
  SECURITY → 6.3 (audit + cipher + certificate) | CompiledPredicateIds: ["SECURITY"]

---

## R030-004 — OWASP A03 injection scan → SECURITY → 6.3

R030-004 test: "Scan for SQL injection and XSS vulnerabilities per OWASP A03."
  SECURITY → 6.3 (scan + injection + XSS + vulnerabilities) | CompiledPredicateIds: ["SECURITY"]

---

## R030-005 — "make OWASP compliant" gap

R030-005 test: "Make the application OWASP Top 10 compliant."
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING
  Teaching: "make compliant" → gap regardless of framework.
