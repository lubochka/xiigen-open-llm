# Layer 0-D — Flow Factory Pairs (035)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## FF-035-001 — AUTHORIZATION factory with guidance vocabulary

A: "Explain the authorization model to the team and implement role-based access control."
Factory: ImplementationFlow41 ‖ AuthorizationFlow | TransitionType: feature_addition | CapabilityGapWarning: gap-skills-guidance-train-001
Evidence: "explain to team" = gap warning; "implement RBAC (Role-Based Access Control)" = IMPLEMENT + AUTHORIZATION → both factories.

---

## FF-035-002 — factory for "guidance + QA only" (no other code predicates)

A: "Walk the QA (Quality Assurance) team through the testing approach and QA the build."
Factory: QAGatingFlow73 | TransitionType: no contribution | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "walk QA team through" = gap warning; "QA the build" = QA → QAGatingFlow73. TransitionType = no contribution (QA only).

---

## FF-035-003 — factory for guidance + DOCUMENT only

A: "Consult on documentation style and document the API (Application Programming Interface) reference."
Factory: DocumentationFlow | TransitionType: documentation | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "consult on style" = gap warning; "document the API reference" = DOCUMENT → DocumentationFlow. TransitionType: documentation (DOCUMENT only, no IMPLEMENT).

---

## FF-035-004 — factory for guidance + TEST only

A: "Run through the testing strategy with the team and write the unit tests for the discount calculator."
Factory: UnitTestingFlow51 | TransitionType: documentation | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "run through strategy with team" = gap warning; "write unit tests" = TEST → UnitTestingFlow51. TransitionType: documentation (TEST only, no IMPLEMENT).

---

## FF-035-005 — factory for guidance + ARCHITECTURE only

A: "Consult with the principal engineers and architect the event mesh topology."
Factory: ArchitectureFlow31 | TransitionType: documentation | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "consult with principal engineers" = gap warning; "architect event mesh topology" = ARCHITECTURE → ArchitectureFlow31. TransitionType: documentation (ARCHITECTURE only, no IMPLEMENT).
