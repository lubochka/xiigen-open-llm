# Layer 0-C — Knowledge Edge Pairs (029)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-029-001 — guidance + UI: all scenarios tabulated

| Pattern | Predicate | Gap |
|---------|-----------|-----|
| "show team how to use UI components" | [] | train-001 |
| "demonstrate UI patterns" | [] | train-001 |
| "show and implement UI components" | [IMPLEMENT] | train-001 warning |
| "walk through design system and build components" | [IMPLEMENT] | mentor-001 warning |
| "present UI to stakeholders" | [] | train-001 |
| "brief on design system" | [] | mentor-001 |
| "explain UI framework" | [] | train-001 |

---

## KE-029-002 — guidance + ARCHITECTURE: all scenarios tabulated

| Pattern | Predicate | Gap |
|---------|-----------|-----|
| "explain the architecture" | [] | train-001 |
| "brief on architecture" | [] | mentor-001 |
| "discuss the architecture" | [] | mentor-001 |
| "present the architecture" | [] | train-001 |
| "discuss and design architecture" | [ARCHITECTURE] | mentor-001 warning |
| "brief and architect the system" | [ARCHITECTURE] | mentor-001 warning |
| "show the architecture and refactor" | [IMPLEMENT] | train-001 warning |

---

## KE-029-003 — "review with" + various predicates edge cases

"review security with team" → gap (knowledge-sharing sense).
"review security [code] for vulnerabilities" → SECURITY.
"review security with team and fix vulnerabilities" → SECURITY fires; "review with" = gap warning.
"review tests with team" → gap.
"review tests for completeness and write missing tests" → TEST fires; "review" = gap warning.
"review docs with team and update them" → DOCUMENT fires; "review with" = gap warning.

---

## KE-029-004 — "run through" boundary: guidance vs run-as-execute

"run through [topic/plan/design]" + person audience → gap.
"run through [test suite]" when context = test execution → TEST fires (execute = code action).
"run through the QA checklist and QA the release" → QA fires; "run through" = gap warning.

Tie-breaker: if "run [noun]" and noun = test suite/QA checklist with execution intent → code predicate. If "run through [noun]" with explanation intent → gap.

---

## KE-029-005 — multiple guidance verbs: single gap, single event

When request contains only guidance verbs (show, demonstrate, present, brief, discuss, run through, review with, familiarize, introduce [person]):
- Predicate set = []
- GapHandler invoked once
- SCHEDULE_CAPABILITY_LEARNING emitted once
- Gap ID = most specific match using priority: mentor-001 > onboard-001 > train-001
- No stacking of gap IDs within one request
