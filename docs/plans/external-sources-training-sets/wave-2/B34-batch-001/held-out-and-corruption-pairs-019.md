# Held-Out and Corruption Pairs (019)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## HC-019-A — Held-out: "guide through writing tests" → TEST

A: "Guide the junior developers through writing unit tests for the authentication module."
B: Predicates: ["TEST"] | UnitTestingFlow51 | TransitionType: documentation
Reason: "guide through" = gap warning; "writing unit tests" → TEST fires. Embedded verb rule.

---

## HC-019-B — Held-out: "mentor on test coverage" → gap (no write verb)

A: "Mentor the team on achieving high test coverage across the core services."
B: Predicates: [] | GapHandler → gap-skills-guidance-mentor-001
Reason: "mentor on test coverage" = advisory vocabulary → gap. Quality goal without write/create verb → no TEST.

---

## HC-019-C — Corruption v6-03: "guide through writing tests" collapsed to gap

v6 corrupt: "Guide the team through writing integration tests for the API" → Predicates: []
Correct: Predicates: ["TEST"] | UnitTestingFlow51 | GapWarning: gap-skills-guidance-mentor-001
Error type: v6-03 (predicate loss). Embedded verb rule: "writing integration tests" fires TEST inside guidance frame. Gap warning alongside TEST; GapHandler NOT invoked.

---

## HC-019-D — Corruption v6-02: "coach on testing" fires TEST

v6 corrupt: "Coach the team on unit testing best practices" → Predicates: ["TEST"]
Correct: Predicates: [] → GapHandler → gap-skills-guidance-mentor-001
Error type: v6-02. "coach on testing best practices" = advisory vocabulary → gap. No write/create/assert verb → no TEST. TEST fires only on explicit test-writing actions.
