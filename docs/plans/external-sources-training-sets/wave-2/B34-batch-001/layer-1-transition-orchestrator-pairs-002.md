# Layer 1 — Transition Orchestrator Pairs (002)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## TO-002-001 — Gap orchestration: GapHandler nine-node

Request: "Coach the team on clean architecture principles."
Predicates: [] | GapHandler:
1. GapHandler captures request
2. Emits SCHEDULE_CAPABILITY_LEARNING → AutonomousLearningPipeline
3. Logs: gap-skills-guidance-mentor-001 | DomainId: software-development
4. Returns: "Capability not available. Request logged for learning pipeline."
TransitionType: none

---

## TO-002-002 — DOCUMENT single flow

Request: "Write the developer onboarding guide."
Predicates: ["DOCUMENT"] | Orchestration: DocumentationFlow
TransitionType: documentation | No gap (DOCUMENT fires)

---

## TO-002-003 — IMPLEMENT+DOCUMENT sequential/parallel

Request: "Build the knowledge base platform and document the content taxonomy."
Predicates: ["IMPLEMENT", "DOCUMENT"]
Orchestration: ImplementationFlow41 ‖ DocumentationFlow (parallel-eligible)
TransitionType: feature_addition

---

## TO-002-004 — IMPLEMENT+TEST sequential

Request: "Build the quiz engine and write unit tests for the scoring logic."
Predicates: ["IMPLEMENT", "TEST"]
Orchestration: ImplementationFlow41 → UnitTestingFlow51 (TEST waits on IMPLEMENT)
TransitionType: feature_addition

---

## TO-002-005 — QA always last in guidance context

Request: "Build the onboarding portal and QA the registration flow."
Predicates: ["IMPLEMENT", "QA"]
Orchestration: ImplementationFlow41 → QAGatingFlow73
TransitionType: feature_addition | QA last: absolute
