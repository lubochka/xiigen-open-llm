# Layer 0-D — Flow Factory Pairs (034)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## FF-034-001 — factory selection when IMPLEMENT + API_CONTRACT present

A: "Show the team how the integration works, implement the adapter layer, and integrate the legacy CRM (Customer Relationship Management) system."
Factory: ImplementationFlow41 ‖ APIContractFlow33 | TransitionType: feature_addition | CapabilityGapWarning: gap-skills-guidance-train-001
Evidence: "show how integration works" = gap warning; implement → IMPLEMENT; integrate → API_CONTRACT. API_CONTRACT flows before IMPLEMENT in ordering.

---

## FF-034-002 — factory selection when ARCHITECTURE + IMPLEMENT present

A: "Discuss the event-driven design, architect the event bus topology, and implement the event router service."
Factory: ArchitectureFlow31 ‖ ImplementationFlow41 | TransitionType: feature_addition | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: discuss = gap warning; architect → ARCHITECTURE (ArchitectureFlow31 first); implement → IMPLEMENT (ImplementationFlow41 second). TransitionType: feature_addition (IMPLEMENT present).

---

## FF-034-003 — factory selection when all four main predicates present

A: "Consult on design, architect the service, implement the core logic, integrate the external dependency, and write tests."
Factory: ArchitectureFlow31 ‖ APIContractFlow33 ‖ ImplementationFlow41 ‖ UnitTestingFlow51 | TransitionType: feature_addition | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: consult = gap warning; architect → ARCHITECTURE; integrate → API_CONTRACT; implement → IMPLEMENT; write tests → TEST.

---

## FF-034-004 — factory selection: guidance with QA at end

A: "Brief the team, implement the feature, test it, and QA the release."
Factory: ImplementationFlow41 ‖ UnitTestingFlow51 ‖ QAGatingFlow73 | TransitionType: feature_addition | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: brief = gap warning; implement → IMPLEMENT; test → TEST; QA → QA (QAGatingFlow73 last).

---

## FF-034-005 — factory: gap warning emission rule (one warning regardless of guidance verb count)

When guidance vocabulary present alongside code predicates:
- One CapabilityGapWarning emitted total (regardless of how many guidance verbs appear)
- Gap ID = most specific gap ID from all guidance vocabulary present
- CapabilityGapWarning payload: {gap_id, domainId: software-development}
- SCHEDULE_CAPABILITY_LEARNING: NOT emitted
