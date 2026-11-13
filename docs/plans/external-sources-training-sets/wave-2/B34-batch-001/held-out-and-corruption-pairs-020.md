# Held-Out and Corruption Pairs (020)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## HC-020-A — Held-out: "coach through building" → IMPLEMENT

A: "Coach the backend team through building the asynchronous event processor service."
B: Predicates: ["IMPLEMENT"] | ImplementationFlow41 | TransitionType: feature_addition
Reason: "coach through" = gap warning; "building the event processor service" → IMPLEMENT fires. Embedded verb rule.

---

## HC-020-B — Held-out: "coach on how to build" → gap (teach-to pattern)

A: "Coach the developers on how to build high-throughput data pipelines."
B: Predicates: [] | GapHandler → gap-skills-guidance-mentor-001
Reason: "coach on how to build" = advisory on methodology → gap. "How to build" = learning target pattern. IMPLEMENT does not fire.

---

## HC-020-C — Corruption v6-03: "coach through building" collapsed to gap

v6 corrupt: "Coach the team through building the microservice" → Predicates: []
Correct: Predicates: ["IMPLEMENT"] | ImplementationFlow41 | GapWarning: gap-skills-guidance-mentor-001
Error type: v6-03 (predicate loss). Embedded verb rule: participial "building the microservice" fires IMPLEMENT inside guidance frame. Gap warning; GapHandler suppressed.

---

## HC-020-D — Corruption v6-02: "mentor through implementing" fires with wrong TransitionType

v6 corrupt: "Mentor through implementing the auth module" → Predicates: ["IMPLEMENT"] | TransitionType: documentation
Correct: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Error type: TransitionType corruption. "implementing" with software noun → IMPLEMENT → feature_addition. Documentation TransitionType is only for non-IMPLEMENT predicates (TEST, DOCUMENT, ARCHITECTURE, etc.).
