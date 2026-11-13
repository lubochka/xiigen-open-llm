# Layer 1 — Transition Orchestrator Pairs (013)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-013-001 — Gap: "shadow" → no orchestration

Request: "Shadow the lead engineer on the next sprint planning."
Predicates: [] | GapHandler → gap-skills-guidance-mentor-001
Orchestration: GapHandler only. No flow execution. SCHEDULE_CAPABILITY_LEARNING emitted.

---

## TO-013-002 — IMPLEMENT: "shadow and build"

Request: "Shadow the infrastructure team and implement the auto-scaling policy for the API cluster."
Predicates: ["IMPLEMENT"]
Orchestration: ImplementationFlow41
TransitionType: feature_addition | Gap warning: shadow = gap-skills-guidance-mentor-001

---

## TO-013-003 — TEST: "shadow and write tests"

Request: "Shadow the QA (Quality Assurance) engineer and write regression tests for the checkout flow."
Predicates: ["TEST"]
Orchestration: UnitTestingFlow51
TransitionType: documentation | Gap warning: shadow = gap-skills-guidance-mentor-001

---

## TO-013-004 — IMPLEMENT+TEST: embedded from shadow context

Request: "Shadow the senior dev, implement the new notification handler, and write smoke tests."
Predicates: ["IMPLEMENT", "TEST"]
Orchestration: ImplementationFlow41 → UnitTestingFlow51
TransitionType: feature_addition | Gap warning: shadow = gap warning

---

## TO-013-005 — Gap: "shadow + third-party build" → only requester's actions count

Request: "Shadow the team while they build and deploy the new release pipeline."
Predicates: [] | GapHandler → gap-skills-guidance-mentor-001
Orchestration: GapHandler. "they build and deploy" = third-party action → no predicates fire.
SCHEDULE_CAPABILITY_LEARNING emitted.
