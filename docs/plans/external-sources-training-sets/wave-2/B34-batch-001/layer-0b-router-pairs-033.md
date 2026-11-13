# Layer 0-B — Router Pairs (033)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-033-001 — "review with" + API_CONTRACT routing

A: "Review the integration contract with the team and integrate the external billing API (Application Programming Interface)."
Router decision: Predicates = ["API_CONTRACT"] → APIContractFlow33 | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "review with team" = gap warning; "integrate the external billing API" → API_CONTRACT fires (integrate always fires).

---

## B34-router-033-002 — "introduce [person]" + TEST routing

A: "Introduce the junior developers to TDD (Test-Driven Development) and write the first test cycle for the billing module."
Router decision: Predicates = ["TEST"] → UnitTestingFlow51 | CapabilityGapWarning: gap-skills-guidance-train-001
Evidence: "introduce to TDD" = gap warning (train-001); "write the first test cycle" = test-writing action → TEST fires.

---

## B34-router-033-003 — "demonstrate" + DOCUMENT routing

A: "Demonstrate our documentation standards and document the service interfaces."
Router decision: Predicates = ["DOCUMENT"] → DocumentationFlow | CapabilityGapWarning: gap-skills-guidance-train-001
Evidence: "demonstrate our documentation standards" = gap warning; "document the service interfaces" = documentation action → DOCUMENT fires.

---

## B34-router-033-004 — "present" + SECURITY routing

A: "Present the security findings to leadership and harden the vulnerable endpoints."
Router decision: Predicates = ["SECURITY"] → SecurityAuditFlow | CapabilityGapWarning: gap-skills-guidance-train-001
Evidence: "present the security findings" = gap warning; "harden the vulnerable endpoints" = security action → SECURITY fires.

---

## B34-router-033-005 — "discuss" + ARCHITECTURE + IMPLEMENT routing

A: "Discuss the design, architect the data layer, and build the persistence service."
Router decision: Predicates = ["ARCHITECTURE", "IMPLEMENT"] → ArchitectureFlow31 → ImplementationFlow41 | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "discuss" = gap warning; "architect the data layer" → ARCHITECTURE; "build the persistence service" → IMPLEMENT. Ordering: ARCHITECTURE before IMPLEMENT.
