# Held-Out and Corruption Pairs (015)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## HC-015-A — Held-out: "walk through explanation" → gap

A: "Walk the new backend engineers through our microservices architecture and how the services communicate."
B: Predicates: [] | GapHandler → gap-skills-guidance-onboard-001
Reason: "walk through architecture" = explain existing design → gap. Onboarding context.

---

## HC-015-B — Held-out: "walk through integrating" → API_CONTRACT

A: "Walk through integrating the Okta identity provider with the developer platform."
B: Predicates: ["API_CONTRACT"] | APIContractFlow33 | TransitionType: documentation
Reason: "walk through" = gap warning; "integrating" → API_CONTRACT fires (integrate always fires).

---

## HC-015-C — Corruption v6-02: "walk through architecture" fires ARCHITECTURE

v6 corrupt: "Walk the team through the microservices architecture we designed" → Predicates: ["ARCHITECTURE"]
Correct: Predicates: [] → GapHandler → gap-skills-guidance-mentor-001
Error type: v6-02. "walk through architecture" = explain architecture → gap. ARCHITECTURE fires on design/architect verbs by requester, not on explaining existing architecture.

---

## HC-015-D — Corruption v6-03: "walk through integrating" collapsed to gap

v6 corrupt: "Walk through integrating the payment API with our checkout" → Predicates: []
Correct: Predicates: ["API_CONTRACT"] | APIContractFlow33
Error type: v6-03 (predicate loss). "integrate" always fires API_CONTRACT even inside walk-through frame. Gap warning alongside API_CONTRACT; GapHandler suppressed.
