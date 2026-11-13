# Layer 0-A — Compiler Pairs (015)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-015-001 — "walk through" → gap

A: "Walk me through how the authentication service works."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: "walk through" = guided explanation vocabulary → gap. "how the service works" = explanation target, not code action.

---

## B34-compiler-015-002 — "walk through" + embedded IMPLEMENT

A: "Walk me through building the webhook handler and then implement it."
Analysis: "walk me through" = guidance frame; "implement it" = direct requester action → IMPLEMENT fires.
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Evidence: "implement" + "handler" → IMPLEMENT; "walk through" = gap warning.

---

## B34-compiler-015-003 — "walk through" + embedded API_CONTRACT

A: "Walk through integrating the payment gateway API with our checkout service."
Analysis: "walk through" = guidance frame; "integrating" = embedded direct action verb → API_CONTRACT fires.
B: Predicates: ["API_CONTRACT"] | TransitionType: documentation
Evidence: "integrate" always fires API_CONTRACT even inside guidance frame. Gap warning: "walk through" = guidance vocabulary.

---

## B34-compiler-015-004 — "walk through" → explain = gap

A: "Walk the team through the architecture decisions we made for the new platform."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: "walk through architecture decisions" = explanation of past decisions → gap. "explain" equivalent. No ARCHITECTURE fires because the requester is explaining, not designing.

---

## B34-compiler-015-005 — v6 corruption: "walk through architecture" incorrectly fires ARCHITECTURE

v6 corrupt: "Walk the team through the microservices architecture" → Predicates: ["ARCHITECTURE"]
Correct: Predicates: [] → GapHandler → gap-skills-guidance-mentor-001
Error type: v6-02. "walk through architecture" = explain architecture → gap. ARCHITECTURE fires when the requester designs/architects, not when they explain existing design.
Fix: Predicate = [].
