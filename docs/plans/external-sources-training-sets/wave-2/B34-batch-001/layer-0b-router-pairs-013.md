# Layer 0-B — Router Pairs (013)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-013-001 — Gap route: "shadow" → gap

A: "Shadow me on the production deployment tomorrow."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001

---

## B34-router-013-002 — IMPLEMENT route: "shadow" + build embedded

A: "Shadow the senior dev and then build the same caching layer in the new service."
Router decision: Predicates = ["IMPLEMENT"] → ImplementationFlow41
Evidence: "build" + "service" = IMPLEMENT trigger; "shadow" = gap warning; embedded verb rule routes to IMPLEMENT.

---

## B34-router-013-003 — Gap route: "shadow" + third-party action only

A: "Shadow the DevOps engineer while she deploys the Kubernetes cluster."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: "she deploys" = third-party action, not requester action. Requester's action = "shadow" = gap.

---

## B34-router-013-004 — TEST route: "shadow" + write tests embedded (requester action)

A: "Shadow the QA (Quality Assurance) lead and write the regression test suite alongside her."
Router decision: Predicates = ["TEST"] → UnitTestingFlow51
Evidence: "write the regression test suite" = direct requester action → TEST fires; "shadow" = gap warning.

---

## B34-router-013-005 — v6 corruption: "shadow" routes to IMPLEMENT incorrectly

v6 corrupt: "Shadow the engineer deploying the service" → Predicates: ["IMPLEMENT"] → ImplementationFlow41
Correct: Predicates: [] → GapHandler → gap-skills-guidance-mentor-001
Error type: v6-02. "shadow the engineer deploying" = requester observes; requester's action = shadow = gap. Deploying is third-party action.
