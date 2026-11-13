# Layer 1 — Transition Orchestrator Pairs (022)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-022-001 — Gap: "explain the architecture"

Request: "Explain the hexagonal architecture pattern to the new engineering cohort."
Predicates: [] | GapHandler → gap-skills-guidance-train-001
Orchestration: GapHandler. "explain" = always gap. SCHEDULE_CAPABILITY_LEARNING emitted.

---

## TO-022-002 — DOCUMENT: "explain and document"

Request: "Explain the event-driven design and document the event schema for the platform handbook."
Predicates: ["DOCUMENT"]
Orchestration: DocumentationFlow
TransitionType: documentation | Gap warning: explain = gap-skills-guidance-train-001

---

## TO-022-003 — Gap: "explain how to write tests"

Request: "Explain how to write effective unit tests to the junior engineers."
Predicates: [] | GapHandler → gap-skills-guidance-train-001
Orchestration: GapHandler. "explain how to write" = teaching vocabulary → gap. SCHEDULE_CAPABILITY_LEARNING emitted.

---

## TO-022-004 — IMPLEMENT: "explain and build"

Request: "Explain the circuit breaker pattern and build the implementation for the order service."
Predicates: ["IMPLEMENT"]
Orchestration: ImplementationFlow41
TransitionType: feature_addition | Gap warning: explain = gap warning

---

## TO-022-005 — TEST+DOCUMENT: "explain and write tests and document"

Request: "Explain the approach, write the contract tests for the billing API, and document the test plan."
Predicates: ["TEST", "DOCUMENT"]
Orchestration: UnitTestingFlow51 ‖ DocumentationFlow
TransitionType: documentation | Gap warning: explain noted
