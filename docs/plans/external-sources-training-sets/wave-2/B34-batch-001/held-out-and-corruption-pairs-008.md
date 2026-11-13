# Held-Out and Corruption Pairs (008)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## HC-008-A — Held-out: "teach to document" → gap

A: "Teach junior developers to document their code properly."
B: Predicates: [] | GapHandler → gap-skills-guidance-mentor-001
Reason: requester's action = "teach" = gap. "document their code" is what juniors will learn; not what requester does.
Phrasing-B: Gap — "teach to X" is always gap regardless of what X is.

---

## HC-008-B — Held-out: "document the coding standards" (requester does it) → DOCUMENT

A: "Document the coding standards for the new engineering team."
B: Predicates: ["DOCUMENT"] | DocumentationFlow | TransitionType: documentation
Reason: requester's action = "document" = DOCUMENT fires (requester is creating documentation).
Phrasing-B: DOCUMENT fires — requester directly creates documentation artifact.

---

## HC-008-C — Corruption v6-02: "teach to document" fires DOCUMENT

v6 corrupt: "Teach the team to document the API" → Predicates: ["DOCUMENT"]
Correct: Predicates: [] → GapHandler → gap-skills-guidance-mentor-001
Error type: v6-02. "teach to document" = teaching vocabulary → gap. Requester's action = "teach" = gap.

---

## HC-008-D — Held-out: "run code reviews" (mentoring context) → gap

A: "Run code reviews for junior developers to help them improve."
B: Predicates: [] | GapHandler → gap-skills-guidance-mentor-001
Reason: "run code reviews" in mentoring context = guidance activity → gap.
Note: "run" is never a predicate-firing verb. "run" + any activity → gap.
