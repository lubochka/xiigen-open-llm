# Layer 0-B — Router Pairs (029)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-029-001 — "introduce" + feature routing: IMPLEMENT

A: "Introduce circuit breakers into the inter-service communication layer."
Router decision: Predicates = ["IMPLEMENT"] → ImplementationFlow41 | TransitionType: feature_addition
Evidence: "introduce [circuit breakers] into [layer]" = software feature insertion → IMPLEMENT. No guidance frame (no person object).

---

## B34-router-029-002 — "familiarize" + DOCUMENT routing

A: "Familiarize the technical writers with the codebase and write the API reference documentation."
Router decision: Predicates = ["DOCUMENT"] → DocumentationFlow | CapabilityGapWarning: gap-skills-guidance-onboard-001
Evidence: "familiarize technical writers with codebase" = gap warning (onboard-001); "write the API reference documentation" = documentation action → DOCUMENT fires.

---

## B34-router-029-003 — "run through" ARCHITECTURE routing

A: "Run through the data model and design the new schema for multi-tenancy."
Router decision: Predicates = ["ARCHITECTURE"] → ArchitectureFlow31 | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "run through the data model" = gap warning; "design the new schema for multi-tenancy" = architecture action → ARCHITECTURE fires.

---

## B34-router-029-004 — "review with" + SECURITY routing

A: "Review the authentication flow with the security lead and patch the session fixation vulnerability."
Router decision: Predicates = ["SECURITY"] → SecurityAuditFlow | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "review with the security lead" = gap warning; "patch the session fixation vulnerability" = security action → SECURITY fires.

---

## B34-router-029-005 — "discuss" + TEST + DOCUMENT routing

A: "Discuss the documentation gaps, write the missing tests, and document the untested modules."
Router decision: Predicates = ["TEST", "DOCUMENT"] → UnitTestingFlow51 ‖ DocumentationFlow | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "discuss" = gap warning; "write the missing tests" → TEST; "document the untested modules" → DOCUMENT. Both code predicates fire. GapHandler suppressed.
