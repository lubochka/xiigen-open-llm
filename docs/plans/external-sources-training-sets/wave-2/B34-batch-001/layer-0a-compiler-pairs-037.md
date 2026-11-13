# Layer 0-A — Compiler Pairs (037)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-037-001 — "show how to" = gap, no exceptions

A: "Show the backend team how to structure microservices cleanly."
B: Predicates: [] | Gap: gap-skills-guidance-train-001
Rule: "show how to" = teaching frame → gap. Topic (microservices structure) is the learning content, not a predicate trigger.

---

## B34-compiler-037-002 — "show how to build" = gap ("teach to" parallel)

A: "Show the interns how to build a REST (Representational State Transfer) endpoint."
B: Predicates: [] | Gap: gap-skills-guidance-train-001
Rule: "show how to build" = infinitive after "show how to" → learning target. Requester's action = show = guidance → gap. "Build" does not fire IMPLEMENT here.

---

## B34-compiler-037-003 — "show how to" + requester code action

A: "Show the team how to test the API (Application Programming Interface), then write the regression suite yourself."
B: Predicates: ["TEST"] | GapWarning: gap-skills-guidance-train-001
Rule: "show how to test" = gap warning; "write the regression suite yourself" = requester's direct TEST action → TEST fires.

---

## B34-compiler-037-004 — "show how to integrate" = gap (teach-to-integrate parallel)

A: "Show the platform engineers how to integrate with our internal auth service."
B: Predicates: [] | Gap: gap-skills-guidance-train-001
Rule: "show how to integrate" = learning target infinitive → gap. Integrate exception does NOT apply when preceded by "show how to."

---

## B34-compiler-037-005 — "show" in non-guidance sense: demonstrate output

A: "Show the test results in the dashboard and document the coverage gaps."
B: Predicates: ["DOCUMENT"] | TransitionType: documentation
Rule: "show [results]" = display/render, not guidance vocabulary. "Document" fires DOCUMENT. No gap.
