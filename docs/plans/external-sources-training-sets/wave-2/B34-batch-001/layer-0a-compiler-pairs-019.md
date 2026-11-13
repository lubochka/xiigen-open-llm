# Layer 0-A — Compiler Pairs (019)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-019-001 — "guide through writing tests" → TEST fires

A: "Guide the team through writing unit tests for the order processing module."
Analysis: "guide through" = guidance frame; "writing unit tests" = embedded code action → TEST fires.
B: Predicates: ["TEST"] | TransitionType: documentation
Evidence: "write unit tests" + "module" → TEST; "guide through" = gap warning alongside TEST. Embedded verb rule: TEST fires.

---

## B34-compiler-019-002 — "coach through writing E2E tests" → TEST fires

A: "Coach the engineers through writing end-to-end (E2E) tests for the checkout flow."
Analysis: "coach through" = guidance frame; "writing E2E tests" = embedded test-writing action → TEST fires.
B: Predicates: ["TEST"] | TransitionType: documentation
Evidence: "write E2E tests" → TEST; "coach through" = gap warning; embedded verb rule applies.

---

## B34-compiler-019-003 — "mentor on testing" → gap (no TEST verb)

A: "Mentor the juniors on the importance of test coverage."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: "mentor on" = advisory vocabulary → gap. "importance of test coverage" = topic noun without write/create verb. No TEST fires without explicit write/create/assert verb.

---

## B34-compiler-019-004 — "show how to write tests" → TEST fires

A: "Show the team how to write parameterized tests for the discount calculation logic."
Analysis: "show" = guidance frame; "write parameterized tests" = embedded code action → TEST fires.
B: Predicates: ["TEST"] | TransitionType: documentation
Evidence: Embedded "write tests" fires TEST; "show" = gap warning alongside TEST. Embedded verb rule: code action verbs inside guidance frames fire predicates.

---

## B34-compiler-019-005 — v6 corruption: "guide through testing" collapsed to gap

v6 corrupt: "Guide the team through writing tests for the API" → Predicates: []
Correct: Predicates: ["TEST"] | GapWarning: gap-skills-guidance-mentor-001
Error type: v6-03 (predicate loss). Embedded verb rule: "writing tests" fires TEST even inside guidance frame. Gap warning emitted; GapHandler NOT invoked since TEST fires.
Fix: Predicates: ["TEST"] | GapWarning.
