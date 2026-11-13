# Layer 1 — Transition Orchestrator Pairs (031)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-031-001 — API_CONTRACT before IMPLEMENT orchestration with guidance

A: "Get feedback on the integration design, integrate the billing API (Application Programming Interface), and implement the billing service."
Orchestration: CapabilityGapWarning: gap-skills-guidance-mentor-001 → APIContractFlow33 → ImplementationFlow41
TransitionType: feature_addition. Execution order: API_CONTRACT first, then IMPLEMENT.

---

## TO-031-002 — full execution chain with guidance framing

A: "Brief the team, architect the system, implement the core, integrate the external service, and write tests."
Orchestration: CapabilityGapWarning: gap-skills-guidance-mentor-001 → ArchitectureFlow31 → APIContractFlow33 → ImplementationFlow41 → UnitTestingFlow51
TransitionType: feature_addition. Guidance brief = warning only at start of chain.

---

## TO-031-003 — conditional guidance + IMPLEMENT orchestration

A: "If the team needs clarification, explain the approach — then implement the caching layer."
Orchestration: CapabilityGapWarning: gap-skills-guidance-train-001 → ImplementationFlow41
TransitionType: feature_addition. Conditional "explain" = gap warning regardless of conditional framing.

---

## TO-031-004 — "knowledge transfer" purpose + three-flow orchestration

A: "As a knowledge transfer exercise, implement the reference microservice, write integration tests, and document the pattern."
Orchestration: CapabilityGapWarning: gap-skills-guidance-mentor-001 → ImplementationFlow41 ‖ UnitTestingFlow51 ‖ DocumentationFlow
TransitionType: feature_addition. Knowledge transfer = purpose clause → gap warning; three code flows execute.

---

## TO-031-005 — orchestration invariant: code flow priority over gap

The orchestration invariant for B34:
1. Scan predicate set first.
2. If predicate set ≠ [] → execute code flows in priority order → append CapabilityGapWarning.
3. If predicate set = [] → execute GapHandler only → emit SCHEDULE_CAPABILITY_LEARNING once.
4. Never execute both GapHandler and code flows simultaneously.
5. Gap warning position in output: appended to first code flow result, not injected between flows.
