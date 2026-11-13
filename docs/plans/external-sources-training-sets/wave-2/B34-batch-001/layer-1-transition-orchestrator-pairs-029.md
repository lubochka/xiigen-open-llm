# Layer 1 — Transition Orchestrator Pairs (029)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-029-001 — "get feedback on" + DOCUMENT orchestration

A: "Get feedback on the documentation format from the tech writers and write the API (Application Programming Interface) user guide."
Orchestration: CapabilityGapWarning: gap-skills-guidance-mentor-001 → DocumentationFlow
TransitionType: documentation. SCHEDULE_CAPABILITY_LEARNING: NOT emitted.

---

## TO-029-002 — "sanity check" + TEST orchestration

A: "Sanity check the mutation testing approach with the team and write the mutation tests for the business logic."
Orchestration: CapabilityGapWarning: gap-skills-guidance-mentor-001 → UnitTestingFlow51
TransitionType: documentation. SCHEDULE_CAPABILITY_LEARNING: NOT emitted.

---

## TO-029-003 — "second opinion" + API_CONTRACT orchestration

A: "Get a second opinion on the streaming API (Application Programming Interface) design and integrate the Kafka (Apache Kafka) consumer endpoints."
Orchestration: CapabilityGapWarning: gap-skills-guidance-mentor-001 → APIContractFlow33
TransitionType: documentation. SCHEDULE_CAPABILITY_LEARNING: NOT emitted.

---

## TO-029-004 — multiple advisory + IMPLEMENT + TEST + DOCUMENT orchestration

A: "Consult with the team, get feedback on approach, implement the audit logging service, write tests, and document it."
Orchestration: CapabilityGapWarning: gap-skills-guidance-mentor-001 → ImplementationFlow41 ‖ UnitTestingFlow51 ‖ DocumentationFlow
TransitionType: feature_addition. Execution order: IMPLEMENT → TEST ‖ DOCUMENT.

---

## TO-029-005 — orchestration: GapWarning consolidation rule

When multiple guidance verbs are present alongside code predicates:
- All guidance vocabulary tokens become a SINGLE CapabilityGapWarning.
- Gap ID = most specific gap ID from all guidance vocabulary.
- Warning emitted ONCE, appended to first code flow result.
- SCHEDULE_CAPABILITY_LEARNING: NOT emitted regardless of guidance verb count.
