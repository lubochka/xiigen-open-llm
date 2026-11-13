# Layer 0-A — Compiler Pairs (020)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-020-001 — "coach through building" → IMPLEMENT fires

A: "Coach the team through building the notification service."
Analysis: "coach through" = guidance frame; "building the notification service" = embedded code action → IMPLEMENT fires.
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Evidence: "build" + "service" → IMPLEMENT; "coach through" = gap warning alongside IMPLEMENT. Embedded verb rule: IMPLEMENT fires.

---

## B34-compiler-020-002 — "mentor through implementing" → IMPLEMENT fires

A: "Mentor the junior engineers through implementing the data pipeline."
Analysis: "mentor through" = guidance frame; "implementing the data pipeline" = embedded code action → IMPLEMENT fires.
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Evidence: "implementing" + "data pipeline" → IMPLEMENT; "mentor through" = gap warning.

---

## B34-compiler-020-003 — "coach on how to build" → gap (infinitive is what others learn)

A: "Coach the backend team on how to build scalable APIs."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: "coach on how to build" = "teach to X" pattern → gap. The infinitive "to build" is what others will learn; requester's action = coach = gap. No IMPLEMENT fires.

---

## B34-compiler-020-004 — "guide through refactoring" → IMPLEMENT fires

A: "Guide the engineers through refactoring the legacy authentication module."
Analysis: "guide through" = guidance frame; "refactoring" = embedded code-action verb → IMPLEMENT fires. TransitionType: refactoring.
B: Predicates: ["IMPLEMENT"] | TransitionType: refactoring
Evidence: "refactor" + "module" → IMPLEMENT (refactoring sub-type); "guide through" = gap warning.

---

## B34-compiler-020-005 — v6 corruption: "coach through building" collapsed to gap

v6 corrupt: "Coach the team through building the microservice" → Predicates: []
Correct: Predicates: ["IMPLEMENT"] | GapWarning: gap-skills-guidance-mentor-001
Error type: v6-03. Embedded verb rule: "building the microservice" → IMPLEMENT fires even inside guidance frame. Gap warning only; GapHandler NOT invoked since IMPLEMENT fires.
Fix: Predicates: ["IMPLEMENT"] | GapWarning.
