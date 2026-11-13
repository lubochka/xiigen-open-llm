# Layer 0-A — Compiler Pairs (033)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-033-001 — "discuss" + ARCHITECTURE topic → gap

A: "Discuss the proposed event-driven architecture with the team leads before we commit to the design."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: "discuss with [people]" = collaborative dialogue → gap. "before we commit to the design" = decision-stage context; no code action in scope.

---

## B34-compiler-033-002 — "discuss and design" → ARCHITECTURE fires

A: "Discuss the options and design the event-sourcing architecture for the order management system."
B: Predicates: ["ARCHITECTURE"] | TransitionType: documentation
Evidence: "discuss the options" = gap warning (gap-skills-guidance-mentor-001); "design the event-sourcing architecture" = requester's direct architecture action → ARCHITECTURE fires.

---

## B34-compiler-033-003 — "review with" + DOCUMENT action embedded

A: "Review the API documentation with the team and update the outdated endpoint descriptions."
B: Predicates: ["DOCUMENT"] | TransitionType: documentation
Evidence: "review with the team" = gap warning (gap-skills-guidance-mentor-001); "update the outdated endpoint descriptions" = requester's direct documentation action → DOCUMENT fires.

---

## B34-compiler-033-004 — guidance + UI + ARCHITECTURE both embedded

A: "Walk the frontend team through the design system and implement the responsive grid components."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Evidence: "walk the frontend team through the design system" = gap warning (gap-skills-guidance-mentor-001); "implement the responsive grid components" = requester's direct code action → IMPLEMENT fires.

---

## B34-compiler-033-005 — complete guidance boundary: "run through" disambiguation

"run through [topic] with [team]" → gap (explanation → gap-skills-guidance-mentor-001).
"run through [checklist] and [code verb]" → gap warning + code predicate fires.
"run through [sequence of steps]" + deploy/build actions → IMPLEMENT fires; run-through = gap warning.
"run through [architecture/design]" = "explain" parallel → gap.
