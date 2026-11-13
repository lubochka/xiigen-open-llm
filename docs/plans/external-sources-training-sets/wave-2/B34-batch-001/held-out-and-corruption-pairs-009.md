# Held-Out and Corruption Pairs (009)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## HC-009-A — Held-out: "guide through writing tests" → TEST fires

A: "Guide the junior team through writing integration tests for the order service."
B: Predicates: ["TEST"] | E2ETestingFlow53 (integration) | TransitionType: documentation
Reason: "guide through writing integration tests" → embedded TEST action fires (write tests embedded inside guidance frame).
Phrasing-B: TEST fires — "writing integration tests" is the embedded direct action; "guide through" = guidance wrapper.

---

## HC-009-B — Held-out: "guide on testing concepts" → gap

A: "Guide the team on testing concepts and best practices."
B: Predicates: [] | GapHandler → gap-skills-guidance-mentor-001
Reason: "guide on testing concepts" = guidance vocabulary; "testing concepts" = topic, not test artifact.
Phrasing-B: Gap — "guide on X" with concept noun → gap. No test-writing action embedded.

---

## HC-009-C — Held-out: "facilitate retrospective" → gap

A: "Facilitate a retrospective to help the team improve their development process."
B: Predicates: [] | GapHandler → gap-skills-guidance-mentor-001
Reason: "facilitate retrospective" = process improvement facilitation → gap.

---

## HC-009-D — Corruption v6-03: guidance + code collapsed to gap only

v6 corrupt: "Mentor the team and build the skill tracker" → Predicates: [] → GapHandler
Correct: Predicates: ["IMPLEMENT"] | ImplementationFlow41 | GapHandler NOT invoked
Error type: v6-03 (predicate loss). "build the skill tracker" = IMPLEMENT; mentor → gap warning. The code verb predicate must not be lost.
