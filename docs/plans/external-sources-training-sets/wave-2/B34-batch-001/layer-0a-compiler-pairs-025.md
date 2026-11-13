# Layer 0-A — Compiler Pairs (025)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-025-001 — "show how to" → gap

A: "Show the interns how to set up their local development environment."
B: Predicates: [] | Gap: gap-skills-guidance-train-001
Evidence: "show how to" = teaching/demonstration vocabulary → gap. "set up local dev environment" = topic context, not a code-action by requester.

---

## B34-compiler-025-002 — "show how to" + embedded code verb → code fires

A: "Show how to implement the retry logic and write the unit tests for it."
B: Predicates: ["IMPLEMENT", "TEST"] | TransitionType: feature_addition
Evidence: "show how to implement" = guidance frame; "implement the retry logic" → IMPLEMENT fires (embedded participial action); "write the unit tests" → TEST fires. Gap warnings: gap-skills-guidance-train-001.

---

## B34-compiler-025-003 — "demonstrate" → gap

A: "Demonstrate the CI/CD (Continuous Integration/Continuous Delivery) pipeline setup to the new DevOps team."
B: Predicates: [] | Gap: gap-skills-guidance-train-001
Evidence: "demonstrate" = demonstration vocabulary → gap always. Demonstration to others = teaching action.

---

## B34-compiler-025-004 — "demonstrate and build" — both parts parsed

A: "Demonstrate the concept and then build the reference implementation for the team."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Evidence: "demonstrate the concept" = gap warning (gap-skills-guidance-train-001); "build the reference implementation" = requester's direct code action → IMPLEMENT fires. Embedded verb rule: "build" fires IMPLEMENT.

---

## B34-compiler-025-005 — v6 corruption: "show how to implement" fires IMPLEMENT from "show"

v6 corrupt: "Show the team how to implement the feature flag system" → Predicates: ["IMPLEMENT"] with gap-free routing
Correct: Predicates: ["IMPLEMENT"] | GapWarning: gap-skills-guidance-train-001
Error type: v6-03 partial (gap warning suppressed). IMPLEMENT fires correctly from embedded "implement", but gap warning must still be appended.
Fix: "show how to X" = guidance frame → gap warning always appended alongside any firing predicates.
