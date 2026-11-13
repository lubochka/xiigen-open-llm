# Held-Out and Corruption Pairs (002)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## HC-002-A — Held-out: "coach on testing"

A: "Coach the team on writing testable code."
B: Predicates: [] | GapHandler → gap-skills-guidance-mentor-001
Reason: "coach" = guidance verb; "testable code" = quality concept, not test artifact. No TEST fires.
Phrasing-B: Gap — coaching on testing ≠ writing tests. TEST fires on write/create/assert verbs.

---

## HC-002-B — Held-out: "write tests for the service" (no guidance framing)

A: "Write unit tests for the payment processing service."
B: Predicates: ["TEST"] | UnitTestingFlow51 | TransitionType: documentation
Reason: "write unit tests" = test-writing verb + unit + module → TEST fires; routes to 5.1.
Phrasing-B: TEST fires — requester's direct action is writing tests, not coaching.

---

## HC-002-C — Corruption v6-02: "coach on testing" fires TEST

v6 corrupt: "Coach the team on writing tests" → Predicates: ["TEST"]
Correct: Predicates: [] → GapHandler → gap-skills-guidance-mentor-001
Error type: v6-02 (gap suppression). "coach on writing tests" = teaching vocabulary → gap.
Fix: The requester's action = "coach" = gap. TEST fires only when the requester writes/creates tests.
