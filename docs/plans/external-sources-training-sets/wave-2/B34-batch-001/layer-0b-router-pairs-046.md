# Layer 0-B — Router Pairs (046)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-046-001 — "discuss" + IMPLEMENT + TEST routing (multi-predicate)

A: "Discuss the feature scope with the product team, implement the filtering engine, and write unit tests."
Router decision: Predicates = ["IMPLEMENT", "TEST"] → ImplementationFlow41 ‖ UnitTestingFlow51 | CapabilityGapWarning: gap-skills-guidance-mentor-001
TransitionType: feature_addition

---

## B34-router-046-002 — "present" + ARCHITECTURE + DOCUMENT routing

A: "Present the architecture proposal to stakeholders, architect the service mesh, and document the topology."
Router decision: Predicates = ["ARCHITECTURE", "DOCUMENT"] → ArchitectureFlow31 ‖ DocumentationFlow | CapabilityGapWarning: gap-skills-guidance-train-001
TransitionType: documentation

---

## B34-router-046-003 — "consult on" + IMPLEMENT + SECURITY routing

A: "Consult on the security approach and implement the JWT (JSON Web Token) authentication with rate limiting."
Router decision: Predicates = ["IMPLEMENT", "SECURITY"] → ImplementationFlow41 ‖ SecurityAuditFlow | CapabilityGapWarning: gap-skills-guidance-mentor-001
TransitionType: feature_addition

---

## B34-router-046-004 — all advisory + all code predicates routing

A: "Get feedback, sanity check the approach, implement the feature, write tests, and document the API (Application Programming Interface)."
Router decision: Predicates = ["IMPLEMENT", "TEST", "DOCUMENT"] → Three flows | CapabilityGapWarning: gap-skills-guidance-mentor-001
Advisory vocabulary (get feedback + sanity check) = gap warnings only. All three code predicates route to their respective flows.
TransitionType: feature_addition

---

## B34-router-046-005 — routing priority rule for B34

When IMPLEMENT present: TransitionType = feature_addition (overrides documentation).
When ARCHITECTURE present (no IMPLEMENT): ArchitectureFlow31 first.
When API_CONTRACT present (no IMPLEMENT, no ARCHITECTURE): APIContractFlow33 first.
Guidance vocabulary never changes routing priority or TransitionType.
