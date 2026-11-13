# Layer 1 — Transition Orchestrator Pairs (016)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-016-001 — Gap: "facilitate workshop"

Request: "Facilitate a three-day architecture workshop for the engineering leadership team."
Predicates: [] | GapHandler → gap-skills-guidance-train-001
Orchestration: GapHandler. Workshop facilitation = gap. SCHEDULE_CAPABILITY_LEARNING emitted.

---

## TO-016-002 — IMPLEMENT: "facilitate workshop + build demo"

Request: "Facilitate the workshop and build the demo microservice that participants will use as a reference."
Predicates: ["IMPLEMENT"]
Orchestration: ImplementationFlow41
TransitionType: feature_addition | Gap warning: facilitate = gap-skills-guidance-mentor-001

---

## TO-016-003 — API_CONTRACT: "facilitate integration workshop" (integrate fires)

Request: "Facilitate the integration of our new CRM (Customer Relationship Management) system with the support platform."
Predicates: ["API_CONTRACT"]
Orchestration: APIContractFlow33
TransitionType: documentation | Gap warning: facilitate = gap warning
Note: "facilitate integration" → API_CONTRACT fires because integrate always fires. Workshop/session framing does not suppress integrate.

---

## TO-016-004 — Gap: "design workshop curriculum" (curriculum noun)

Request: "Design a comprehensive workshop curriculum for the three-day DevOps onboarding program."
Predicates: [] | GapHandler → gap-skills-guidance-train-001
Orchestration: GapHandler. Curriculum noun → gap, not ARCHITECTURE. SCHEDULE_CAPABILITY_LEARNING emitted.

---

## TO-016-005 — ARCHITECTURE: "design system architecture" (structural noun)

Request: "Design the microservices system architecture for the new e-commerce platform."
Predicates: ["ARCHITECTURE"]
Orchestration: ArchitectureFlow31
TransitionType: documentation
Note: Contrast with 016-004. Structural noun → ARCHITECTURE. Curriculum noun → gap.
