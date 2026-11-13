# Layer 0-D — Flow Factory Pairs (002)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## FF-002-001 — Gap: GapHandler factory selection

Predicates: [] | Request: "Mentor the junior developers on clean code practices."
Factory selection: GapHandler (no predicate-flow factory invoked)
Event emitted: SCHEDULE_CAPABILITY_LEARNING
Gap ID: gap-skills-guidance-mentor-001
TransitionType: none

---

## FF-002-002 — DOCUMENT: DocumentationFlow factory

Predicates: ["DOCUMENT"] | Request: "Write the developer onboarding guide."
Factory selection: DocumentationFlow
TransitionType: documentation

---

## FF-002-003 — IMPLEMENT: ImplementationFlow41 factory

Predicates: ["IMPLEMENT"] | Request: "Build the skill assessment platform."
Factory selection: ImplementationFlow41 (sub-phase 4.1)
TransitionType: feature_addition

---

## FF-002-004 — IMPLEMENT+DOCUMENT: two-flow factory

Predicates: ["IMPLEMENT", "DOCUMENT"] | Request: "Build the LMS portal and document the curriculum structure."
Factory selection: ImplementationFlow41 + DocumentationFlow
TransitionType: feature_addition (IMPLEMENT present)
Orchestration: parallel-eligible (no data dependency between flows)

---

## FF-002-005 — API_CONTRACT+IMPLEMENT: two-flow factory

Predicates: ["API_CONTRACT", "IMPLEMENT"] | Request: "Integrate the training platform with Slack and build the notification service."
Factory selection: APIContractFlow33 + ImplementationFlow41
TransitionType: feature_addition
Ordering: APIContractFlow33 before ImplementationFlow41 (contract informs implementation)
