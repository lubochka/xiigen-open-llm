# Layer 0-A — Compiler Pairs (018)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-018-001 — "facilitate" → gap

A: "Facilitate the knowledge transfer session between the outgoing and incoming backend teams."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: "facilitate" = facilitation vocabulary → gap. "knowledge transfer" = learning/handoff vocabulary → gap.

---

## B34-compiler-018-002 — "facilitate" + embedded TEST verb

A: "Facilitate a session where the team writes integration tests for the payment service."
Analysis: "facilitate" = gap frame; "writes integration tests" = embedded direct action by the team → but team's action ≠ requester's action. Requester's action = "facilitate" = gap.
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: Team's action of writing tests does not fire predicates. Requester's action = facilitate = gap. Embedded verb rule applies only to requester's own embedded actions.

---

## B34-compiler-018-003 — "facilitate" + embedded requester code action

A: "Facilitate the session and then personally implement the feature the team designs."
Analysis: "facilitate" = gap frame; "personally implement the feature" = direct requester action → IMPLEMENT fires.
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Evidence: "implement" + "feature" → IMPLEMENT; "facilitate" = gap warning. Requester explicitly implements.

---

## B34-compiler-018-004 — "facilitate integration" → API_CONTRACT fires

A: "Facilitate the integration of the third-party analytics SDK (Software Development Kit) with our mobile app."
Analysis: "facilitate" = gap frame; "integration of the third-party analytics SDK" = embedded API action. "integrate" always fires API_CONTRACT.
B: Predicates: ["API_CONTRACT"] | TransitionType: documentation
Evidence: "integrate" always fires API_CONTRACT. "facilitate integration" = requester directing/enabling integration → API_CONTRACT. Gap warning for "facilitate."

---

## B34-compiler-018-005 — v6 corruption: "facilitate" incorrectly maps to no predicate for embedded integrate

v6 corrupt: "Facilitate integrating the SSO provider with our platform" → Predicates: []
Correct: Predicates: ["API_CONTRACT"] | APIContractFlow33
Error type: v6-03 (predicate loss). "integrate" always fires API_CONTRACT even inside facilitation frame. The integration action is real and must be routed.
Fix: Predicates: ["API_CONTRACT"].
