# Held-Out and Corruption Pairs (018)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## HC-018-A — Held-out: "facilitate integration" → API_CONTRACT (special rule)

A: "Facilitate the integration of the identity management platform with our internal developer tools."
B: Predicates: ["API_CONTRACT"] | APIContractFlow33 | TransitionType: documentation
Reason: "integrate" always fires API_CONTRACT even inside facilitate wrapper. Gap warning: facilitate = gap-skills-guidance-mentor-001.

---

## HC-018-B — Held-out: "facilitate session where team writes tests" → gap (third-party)

A: "Facilitate a coding session where the junior engineers write unit tests for the new API endpoints."
B: Predicates: [] | GapHandler → gap-skills-guidance-mentor-001
Reason: "team writes tests" = third-party action. Requester's action = facilitate = gap. Embedded verb rule: only requester's direct actions fire predicates.

---

## HC-018-C — Corruption v6-03: "facilitate integration" collapsed to gap

v6 corrupt: "Facilitate the integration of the CRM (Customer Relationship Management) with our platform" → Predicates: []
Correct: Predicates: ["API_CONTRACT"] | APIContractFlow33
Error type: v6-03 (predicate loss). "integrate" always fires API_CONTRACT. Even inside facilitation frame, the integration action is real and must be routed.

---

## HC-018-D — Corruption v6-02: "facilitate session where team implements" fires IMPLEMENT

v6 corrupt: "Facilitate a session where the team implements the new feature" → Predicates: ["IMPLEMENT"]
Correct: Predicates: [] → GapHandler → gap-skills-guidance-mentor-001
Error type: v6-02. "team implements" = third-party action. Requester = facilitate = gap. IMPLEMENT fires only when requester implements. Exception does NOT extend to implement (only to integrate).
