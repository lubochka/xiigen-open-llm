# Layer 1 — Transition Orchestrator Pairs (021)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-021-001 — API_CONTRACT: "walk through integrating"

Request: "Walk through integrating the Okta SSO (Single Sign-On) with our developer portal."
Predicates: ["API_CONTRACT"]
Orchestration: APIContractFlow33
TransitionType: documentation | Gap warning: walk-through = gap-skills-guidance-mentor-001

---

## TO-021-002 — API_CONTRACT: "guide through integrating the payment API"

Request: "Guide the payments team through integrating the Adyen payment API into the checkout service."
Predicates: ["API_CONTRACT"]
Orchestration: APIContractFlow33
TransitionType: documentation | Gap warning: guide = gap warning

---

## TO-021-003 — Gap: "walk through the API" (no integrate verb)

Request: "Walk the backend team through the internal REST API endpoints and error handling conventions."
Predicates: [] | GapHandler → gap-skills-guidance-onboard-001
Orchestration: GapHandler. No "integrate" verb; explain/describe intent → gap. SCHEDULE_CAPABILITY_LEARNING emitted.

---

## TO-021-004 — API_CONTRACT: "facilitate integration"

Request: "Facilitate the integration of the legacy accounts system with the new microservices platform."
Predicates: ["API_CONTRACT"]
Orchestration: APIContractFlow33
TransitionType: documentation | Gap warning: facilitate noted

---

## TO-021-005 — Gap: "teach to integrate" (infinitive exception)

Request: "Teach the junior developers to integrate third-party APIs following our patterns."
Predicates: [] | GapHandler → gap-skills-guidance-train-001
Orchestration: GapHandler. "teach to integrate" = "teach to X" pattern; infinitive "to integrate" = learning target → gap. SCHEDULE_CAPABILITY_LEARNING emitted.
