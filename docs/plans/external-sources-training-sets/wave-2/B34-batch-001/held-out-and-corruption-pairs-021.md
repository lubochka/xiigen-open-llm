# Held-Out and Corruption Pairs (021)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## HC-021-A — Held-out: "walk through integrating" → API_CONTRACT

A: "Walk through integrating the enterprise identity platform with our developer portal and internal tools."
B: Predicates: ["API_CONTRACT"] | APIContractFlow33 | TransitionType: documentation
Reason: "walk through" = gap warning; "integrating" → API_CONTRACT fires (integrate always fires).

---

## HC-021-B — Held-out: "teach to integrate" → gap (infinitive exception)

A: "Teach the new developers to integrate third-party payment providers following our standards."
B: Predicates: [] | GapHandler → gap-skills-guidance-train-001
Reason: "teach to integrate" = "teach to X" exception. Infinitive "to integrate" = learning target. Requester's action = teach = gap.

---

## HC-021-C — Corruption v6-03: "walk through integrating" collapsed to gap

v6 corrupt: "Walk through integrating the Okta SSO with our platform" → Predicates: []
Correct: Predicates: ["API_CONTRACT"] | APIContractFlow33 | GapWarning: gap-skills-guidance-mentor-001
Error type: v6-03. "integrate" always fires API_CONTRACT. Walk-through frame does not suppress integration predicate.

---

## HC-021-D — Corruption v6-02: "teach to integrate" fires API_CONTRACT

v6 corrupt: "Teach the team to integrate third-party APIs" → Predicates: ["API_CONTRACT"]
Correct: Predicates: [] → GapHandler → gap-skills-guidance-train-001
Error type: v6-02. "teach to integrate" = "teach to X" infinitive exception. Infinitive form blocks API_CONTRACT from firing. This is the only construction where integrate does not fire API_CONTRACT.
