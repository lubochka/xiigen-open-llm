# Layer 0-A — Compiler Pairs (013)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-013-001 — "shadow" on deployment → gap

A: "Shadow me on the next production deployment so you understand the process."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: "shadow" = observation-based learning vocabulary → gap. "production deployment" = context noun, not a predicate without deploy/build verb.

---

## B34-compiler-013-002 — "shadow" + code context → still gap

A: "Shadow the senior engineer while she refactors the payment service."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: "shadow" = guidance frame; "refactors the payment service" = embedded action by another party, not the requester. Requester's action = shadow = gap.

---

## B34-compiler-013-003 — "shadow" + embedded IMPLEMENT requested

A: "Shadow the architect and then build the same pattern in the new service."
Analysis: "shadow" = gap frame; "build the same pattern in the new service" = direct action by requester → IMPLEMENT fires.
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Evidence: "build" + "service" → IMPLEMENT; "shadow" = gap warning; gap warning only (IMPLEMENT fires).

---

## B34-compiler-013-004 — "shadow in code context" — requester writes code

A: "Shadow the deployment pipeline configuration and write the equivalent Terraform scripts."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Evidence: "write Terraform scripts" = direct code-writing action by requester → IMPLEMENT; "shadow" = gap warning alongside IMPLEMENT. Embedded verb rule: code action fires.

---

## B34-compiler-013-005 — v6 corruption: "shadow" incorrectly fires IMPLEMENT

v6 corrupt: "Shadow the engineer while she builds the CI pipeline" → Predicates: ["IMPLEMENT"]
Correct: Predicates: [] → GapHandler → gap-skills-guidance-mentor-001
Error type: v6-02 (gap suppression). The engineer builds; the requester shadows. Requester's action = "shadow" = gap.
Fix: Only the requester's direct action verbs fire predicates. Third-party embedded actions do not fire.
