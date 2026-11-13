# Layer 1 — Transition Orchestrator Pairs (030)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-030-001 — "explain and build" compound orchestration

A: "Explain the CQRS (Command Query Responsibility Segregation) pattern to the team and build the command handler."
Orchestration: CapabilityGapWarning: gap-skills-guidance-train-001 → ImplementationFlow41
TransitionType: feature_addition. SCHEDULE_CAPABILITY_LEARNING: NOT emitted.
Key rule: explain always becomes gap warning; build fires IMPLEMENT.

---

## TO-030-002 — "show how to" + code action orchestration position rule

A: "Implement the feature flag system and then show the team how it works."
Orchestration: CapabilityGapWarning: gap-skills-guidance-train-001 → ImplementationFlow41
TransitionType: feature_addition. SCHEDULE_CAPABILITY_LEARNING: NOT emitted.
Key rule: guidance vocabulary at END of request = same outcome as at start. Position does not affect orchestration.

---

## TO-030-003 — "introduce [person]" + IMPLEMENT orchestration

A: "Introduce the new developers to the event-driven patterns and implement the event handler scaffold."
Orchestration: CapabilityGapWarning: gap-skills-guidance-onboard-001 → ImplementationFlow41
TransitionType: feature_addition. SCHEDULE_CAPABILITY_LEARNING: NOT emitted.

---

## TO-030-004 — "familiarize" + API_CONTRACT orchestration

A: "Familiarize the integration team with the partner API (Application Programming Interface) and integrate the payment webhook."
Orchestration: CapabilityGapWarning: gap-skills-guidance-onboard-001 → APIContractFlow33
TransitionType: documentation. SCHEDULE_CAPABILITY_LEARNING: NOT emitted.

---

## TO-030-005 — orchestration: ARCHITECTURE before IMPLEMENT rule with guidance

A: "Consult on design, architect the service topology, and implement the core services."
Orchestration: CapabilityGapWarning: gap-skills-guidance-mentor-001 → ArchitectureFlow31 → ImplementationFlow41
TransitionType: feature_addition. Execution order: ARCHITECTURE first, then IMPLEMENT. Guidance = warning only.
