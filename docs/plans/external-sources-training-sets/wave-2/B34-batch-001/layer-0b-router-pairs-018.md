# Layer 0-B — Router Pairs (018)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-018-001 — Gap route: "facilitate knowledge transfer" → gap

A: "Facilitate the knowledge transfer from the outgoing team lead to the new engineers."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001

---

## B34-router-018-002 — API_CONTRACT route: "facilitate integration" → API_CONTRACT fires

A: "Facilitate the integration of the HR (Human Resources) system with the developer provisioning tool."
Router decision: Predicates = ["API_CONTRACT"] → APIContractFlow33
Evidence: "integrate" always fires API_CONTRACT even in facilitation context; "facilitate" = gap warning.

---

## B34-router-018-003 — Gap route: "facilitate" + third-party code action

A: "Facilitate a session where the team implements the new caching strategy."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: Team's "implements" = third-party action. Requester's action = facilitate = gap. Embedded verb rule: only requester's actions fire predicates.

---

## B34-router-018-004 — IMPLEMENT route: "facilitate" + requester implements directly

A: "Facilitate the discussion and then implement the agreed-upon caching strategy myself."
Router decision: Predicates = ["IMPLEMENT"] → ImplementationFlow41
Evidence: "implement … myself" = requester's direct action → IMPLEMENT; "facilitate the discussion" = gap warning.

---

## B34-router-018-005 — v6 corruption: "facilitate implementation" incorrectly fires IMPLEMENT

v6 corrupt: "Facilitate the team's implementation of the new microservice" → Predicates: ["IMPLEMENT"]
Correct: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Error type: v6-02. "facilitate the team's implementation" = facilitation vocabulary + third-party action → gap. Requester's action = facilitate = gap. Team's implementation is theirs, not the requester's.
