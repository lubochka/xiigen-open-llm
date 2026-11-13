# Held-Out and Corruption Pairs (023)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## HC-023-A — Held-out: multiple guidance verbs → gap

A: "Mentor, coach, shadow, and advise the new team leads on engineering best practices."
B: Predicates: [] | GapHandler → gap-skills-guidance-mentor-001
Reason: Four guidance verbs, no embedded code actions. Predicate set = []. GapHandler + SCHEDULE_CAPABILITY_LEARNING.

---

## HC-023-B — Held-out: mixed guidance + multiple code predicates

A: "Coach through building the auth service, walk through integrating it with the SSO (Single Sign-On) provider, and guide through writing the contract tests."
B: Predicates: ["IMPLEMENT", "API_CONTRACT", "TEST"] | ImplementationFlow41 + APIContractFlow33 + UnitTestingFlow51
Orchestration: APIContractFlow33 → ImplementationFlow41 → UnitTestingFlow51 | TransitionType: feature_addition
GapWarning: coach/walk-through/guide = gap-skills-guidance-mentor-001

---

## HC-023-C — Corruption v6-03: multiple guidance+code collapsed to all-gap

v6 corrupt: "Coach through building, walk through integrating, guide through writing tests" → Predicates: []
Correct: Predicates: ["IMPLEMENT", "API_CONTRACT", "TEST"] | Three flows execute
Error type: v6-03. Embedded verb rule requires all three code predicates to fire. Guidance frames become gap warnings, not gap logs. GapHandler suppressed.

---

## HC-023-D — Corruption v6-01: invented predicates for guidance vocabulary

v6 corrupt: "Mentor the team on architecture and coach the juniors on testing" → Predicates: ["MENTORING", "TRAINING"]
Correct: Predicates: [] → GapHandler → gap-skills-guidance-mentor-001
Error type: v6-01 (invented predicates). MENTORING and TRAINING are not registered predicates. Guidance vocabulary has no predicate; it maps to gap IDs, not predicates.
