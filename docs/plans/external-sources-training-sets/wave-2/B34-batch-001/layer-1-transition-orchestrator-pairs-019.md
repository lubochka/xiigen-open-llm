# Layer 1 — Transition Orchestrator Pairs (019)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-019-001 — TEST: "guide through writing tests"

Request: "Guide the new developers through writing unit tests for the cart service module."
Predicates: ["TEST"]
Orchestration: UnitTestingFlow51
TransitionType: documentation | Gap warning: guide = gap-skills-guidance-mentor-001

---

## TO-019-002 — TEST: "coach through writing E2E tests"

Request: "Coach the QA (Quality Assurance) engineers through writing end-to-end tests for the checkout pipeline."
Predicates: ["TEST"]
Orchestration: E2ETestingFlow53
TransitionType: documentation | Gap warning: coach = gap warning

---

## TO-019-003 — TEST+IMPLEMENT: "mentor through TDD cycle"

Request: "Mentor the engineers through the full TDD (Test-Driven Development) cycle: write the failing test, implement the handler, and make it pass."
Predicates: ["TEST", "IMPLEMENT"]
Orchestration: UnitTestingFlow51 → ImplementationFlow41 (TDD cycle order: test first)
TransitionType: feature_addition | Gap warning: mentor noted

---

## TO-019-004 — Gap: "coach on test coverage" (no write verb)

Request: "Coach the team on achieving and maintaining 90% unit test coverage."
Predicates: [] | GapHandler → gap-skills-guidance-mentor-001
Orchestration: GapHandler. "coach on achieving coverage" = quality goal advisory → gap; no write/create test verb. SCHEDULE_CAPABILITY_LEARNING emitted.

---

## TO-019-005 — TEST: "show how to write property-based tests"

Request: "Show the senior engineers how to write property-based tests for the pricing calculation service."
Predicates: ["TEST"]
Orchestration: UnitTestingFlow51
TransitionType: documentation | Gap warning: show = gap-skills-guidance-train-001
