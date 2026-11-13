# Held-Out and Corruption Pairs (011)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## HC-011-A — Held-out: "create a tutorial" → DOCUMENT (not IMPLEMENT)

A: "Create a tutorial on setting up the development environment."
B: Predicates: ["DOCUMENT"] | DocumentationFlow | TransitionType: documentation
Reason: "create a tutorial" = create + documentation artifact (tutorial) → DOCUMENT.
Phrasing-B: DOCUMENT — "tutorial" is a documentation artifact noun; "create" → DOCUMENT in this context.

---

## HC-011-B — Held-out: "create a tutorial management system" → IMPLEMENT

A: "Create a tutorial management system for tracking developer learning progress."
B: Predicates: ["IMPLEMENT"] | ImplementationFlow41 | TransitionType: feature_addition
Reason: "create a tutorial management system" = create + software system noun → IMPLEMENT.
Phrasing-B: IMPLEMENT — "management system" is a software component noun, not a documentation artifact.

---

## HC-011-C — Held-out: pure gap — five guidance verbs

A: "Coach, mentor, train, teach, and guide the engineering team to excellence."
B: Predicates: [] | GapHandler → gap-skills-guidance-mentor-001
Reason: all five verbs = guidance vocabulary → gap. Single SCHEDULE_CAPABILITY_LEARNING.
Phrasing-B: Gap — five guidance verbs yield one gap ID (mentor priority), one event, one GapHandler invocation.

---

## HC-011-D — Corruption v6-02: "create tutorial" fires IMPLEMENT

v6 corrupt: "Create a tutorial on using the API" → Predicates: ["IMPLEMENT"]
Correct: Predicates: ["DOCUMENT"] → DocumentationFlow
Error type: v6-02 (gap suppression → wrong predicate). "tutorial" = documentation artifact → DOCUMENT, not IMPLEMENT.
