# Layer 0-B — Router Pairs (028)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-028-001 — "show how to" + TEST routing

A: "Show the team how to write property-based tests for the pricing engine."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-train-001
Evidence: "show how to write tests" = "teach to X" parallel → gap. Infinitive "to write" = learning content for others.

---

## B34-router-028-002 — "show" + "write tests" compound routing

A: "Show the team the test patterns and write the property-based tests for the pricing engine."
Router decision: Predicates = ["TEST"] → UnitTestingFlow51 | CapabilityGapWarning: gap-skills-guidance-train-001
Evidence: "show the test patterns" = gap warning; "write the property-based tests" = requester's test-writing action → TEST fires. GapHandler suppressed.

---

## B34-router-028-003 — "demonstrate" + SECURITY routing

A: "Demonstrate the penetration testing process and harden the admin API against SQL injection."
Router decision: Predicates = ["SECURITY"] → SecurityAuditFlow | CapabilityGapWarning: gap-skills-guidance-train-001
Evidence: "demonstrate penetration testing process" = gap warning; "harden the admin API against SQL injection" = security action → SECURITY fires.

---

## B34-router-028-004 — "brief" + SECURITY routing

A: "Brief the developers on secure coding practices and audit the existing codebase for OWASP (Open Web Application Security Project) vulnerabilities."
Router decision: Predicates = ["SECURITY"] → SecurityAuditFlow | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "brief on secure coding practices" = gap warning; "audit the existing codebase for OWASP vulnerabilities" = security audit action → SECURITY fires.

---

## B34-router-028-005 — all-guidance compound routing

A: "Brief, discuss, and walk the team through the new observability strategy."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: brief + discuss + walk through = all guidance vocabulary. Single GapHandler invocation. Single SCHEDULE_CAPABILITY_LEARNING. Gap ID: gap-skills-guidance-mentor-001.
