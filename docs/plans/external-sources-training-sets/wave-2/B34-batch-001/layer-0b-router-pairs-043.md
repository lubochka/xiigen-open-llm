# Layer 0-B — Router Pairs (043)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-043-001 — "show how to integrate" routing = GapHandler (not APIContractFlow33)

A: "Show the team how to integrate third-party logging providers."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-train-001
Evidence: "show how to integrate" = learning target infinitive → GapHandler. Integrate exception does NOT apply here.

---

## B34-router-043-002 — "demonstrate how to integrate" routing = GapHandler

A: "Demonstrate how to integrate the payment SDK (Software Development Kit) with the checkout flow."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-train-001
Evidence: "demonstrate how to integrate" = learning target → GapHandler. Not API_CONTRACT.

---

## B34-router-043-003 — direct "integrate" routing = APIContractFlow33

A: "Integrate the payment SDK (Software Development Kit) with the checkout flow."
Router decision: Predicates = ["API_CONTRACT"] → APIContractFlow33 | TransitionType: documentation
Evidence: "integrate" as requester's direct action = API_CONTRACT always → APIContractFlow33.

---

## B34-router-043-004 — "teach to integrate" routing = GapHandler (established rule)

A: "Teach the backend team to integrate external data providers."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-train-001
Evidence: "teach to integrate" = "teach to X" pattern → GapHandler. Integrate exception blocked by teach-to pattern.

---

## B34-router-043-005 — "walk through integrating" routing = APIContractFlow33 (embedded verb rule)

A: "Walk through integrating the metrics exporter with the Prometheus (monitoring system) endpoint."
Router decision: Predicates = ["API_CONTRACT"] → APIContractFlow33 | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: walk-through = gap warning; "integrating" (participial) in guidance frame = integrate exception fires → API_CONTRACT routes to APIContractFlow33.
