# Layer 1 — Transition Orchestrator Pairs (020)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-020-001 — IMPLEMENT: "coach through building"

Request: "Coach the backend engineers through building the async message consumer service."
Predicates: ["IMPLEMENT"]
Orchestration: ImplementationFlow41
TransitionType: feature_addition | Gap warning: coach = gap-skills-guidance-mentor-001

---

## TO-020-002 — IMPLEMENT: "mentor through implementing"

Request: "Mentor the intern through implementing the user profile update endpoint."
Predicates: ["IMPLEMENT"]
Orchestration: ImplementationFlow41
TransitionType: feature_addition | Gap warning: mentor = gap warning

---

## TO-020-003 — Gap: "coach on how to build" (teach-to pattern)

Request: "Coach the developers on how to build event-sourced systems with proper aggregate boundaries."
Predicates: [] | GapHandler → gap-skills-guidance-mentor-001
Orchestration: GapHandler. "coach on how to build" = advisory on methodology → gap. SCHEDULE_CAPABILITY_LEARNING emitted.

---

## TO-020-004 — IMPLEMENT (refactoring): "guide through refactoring"

Request: "Guide the senior engineers through refactoring the monolithic order service into bounded contexts."
Predicates: ["IMPLEMENT"]
Orchestration: ImplementationFlow41
TransitionType: refactoring | Gap warning: guide = gap warning

---

## TO-020-005 — IMPLEMENT+DOCUMENT: "coach through building and documenting"

Request: "Coach the team through building the API gateway layer and documenting the routing configuration."
Predicates: ["IMPLEMENT", "DOCUMENT"]
Orchestration: ImplementationFlow41 ‖ DocumentationFlow
TransitionType: feature_addition | Gap warning: coach noted
