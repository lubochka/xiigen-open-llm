# Layer 0-B — Router Pairs (011)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: router

---

## R011-001 — SECURITY → 6.3 via "insecure deserialization" audit
Request: "Scan the ImportService for insecure deserialization vulnerabilities."
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

## R011-002 — SECURITY + TEST → [5.2, 5.1] (security regression tests)
Request: "Write regression tests asserting previously fixed XSS vulnerabilities don't re-emerge."
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT", "TEST"] | SubPhases: [5.2, 4.1, 5.1]
  Routing: SECURITY in test-writing context → 5.2 | IMPLEMENT → 4.1 | TEST → 5.1

## R011-003 — SECURITY + QA + AUTHORIZATION → [6.3, 7.3, 6.3]
Request: "QA the AdminPanel authorization controls and audit for wildcard permission grants."
  CompiledPredicateIds: ["QA", "SECURITY", "AUTHORIZATION"] | SubPhases: [7.3, 6.3, 6.3]

## R011-004 — SECURITY + DOCUMENT → [6.3, 6.2] (security advisory)
Request: "Document the security advisory for the SQL injection vulnerability found in the OrderService."
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"] | SubPhases: [6.3, 6.2]

## R011-005 — SECURITY + IMPLEMENT + ARCHITECTURE → [5.2, 4.1, 3.1]
Request: "Design a WAF (Web Application Firewall) integration for the public-facing APIs and implement the firewall rules."
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT", "ARCHITECTURE"] | SubPhases: [5.2, 4.1, 3.1]
  Routing: SECURITY in design/implement context → 5.2 | IMPLEMENT → 4.1 | ARCHITECTURE (design) → 3.1
