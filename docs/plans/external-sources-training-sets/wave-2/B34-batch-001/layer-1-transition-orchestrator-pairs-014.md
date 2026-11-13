# Layer 1 — Transition Orchestrator Pairs (014)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-014-001 — Gap: "pair with" no embedded code action

Request: "Pair with the senior backend engineers to learn their development workflow."
Predicates: [] | GapHandler → gap-skills-guidance-mentor-001
Orchestration: GapHandler. SCHEDULE_CAPABILITY_LEARNING emitted.

---

## TO-014-002 — TEST: "pair with to write tests"

Request: "Pair with me to write integration tests for the payment processing module."
Predicates: ["TEST"]
Orchestration: UnitTestingFlow51
TransitionType: documentation | Gap warning: pair-with = gap-skills-guidance-mentor-001

---

## TO-014-003 — API_CONTRACT: "pair with to integrate"

Request: "Pair with the platform team to integrate our internal notification service with the external email provider."
Predicates: ["API_CONTRACT"]
Orchestration: APIContractFlow33
TransitionType: documentation | Gap warning: pair-with = gap warning

---

## TO-014-004 — IMPLEMENT+TEST: "pair with to implement and write tests"

Request: "Pair with the team to implement the feature flag system and write unit tests for the flag evaluation logic."
Predicates: ["IMPLEMENT", "TEST"]
Orchestration: ImplementationFlow41 → UnitTestingFlow51
TransitionType: feature_addition | Gap warning: pair-with noted

---

## TO-014-005 — Gap: "pair with to learn"

Request: "Pair with the infrastructure team to understand how they manage Kubernetes deployments."
Predicates: [] | GapHandler → gap-skills-guidance-onboard-001
Orchestration: GapHandler. "understand how they manage" = learning intent → gap. SCHEDULE_CAPABILITY_LEARNING emitted.
