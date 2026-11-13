# Layer 1 — Transition Orchestrator Pairs (018)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-018-001 — Gap: "facilitate knowledge transfer"

Request: "Facilitate the knowledge transfer between the senior engineers and the new joiners."
Predicates: [] | GapHandler → gap-skills-guidance-mentor-001
Orchestration: GapHandler. Facilitation vocabulary → gap. SCHEDULE_CAPABILITY_LEARNING emitted.

---

## TO-018-002 — API_CONTRACT: "facilitate integration"

Request: "Facilitate the integration of the HR (Human Resources) provisioning system with our developer toolchain."
Predicates: ["API_CONTRACT"]
Orchestration: APIContractFlow33
TransitionType: documentation | Gap warning: facilitate = gap-skills-guidance-mentor-001

---

## TO-018-003 — Gap: "facilitate session where team writes tests" (third-party action)

Request: "Facilitate a test-writing session where the backend team writes unit tests for the new service."
Predicates: [] | GapHandler → gap-skills-guidance-mentor-001
Orchestration: GapHandler. "team writes tests" = third-party action; requester = facilitate = gap. SCHEDULE_CAPABILITY_LEARNING emitted.

---

## TO-018-004 — IMPLEMENT+DOCUMENT: "facilitate and directly deliver"

Request: "Facilitate the design session, implement the agreed-upon solution, and document the implementation decisions."
Predicates: ["IMPLEMENT", "DOCUMENT"]
Orchestration: ImplementationFlow41 ‖ DocumentationFlow
TransitionType: feature_addition | Gap warning: facilitate noted

---

## TO-018-005 — API_CONTRACT+IMPLEMENT: "facilitate integration and build connector"

Request: "Facilitate the integration of the legacy billing system with the new platform and build the data migration connector."
Predicates: ["API_CONTRACT", "IMPLEMENT"]
Orchestration: APIContractFlow33 → ImplementationFlow41
TransitionType: feature_addition | Gap warning: facilitate noted
