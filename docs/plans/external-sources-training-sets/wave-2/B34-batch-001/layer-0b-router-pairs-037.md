# Layer 0-B — Router Pairs (037)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-037-001 — "show how to" routing = GapHandler

A: "Show the interns how to set up their local development environment."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-train-001
Evidence: "show how to" = teaching frame → GapHandler invoked. No code flow selected.

---

## B34-router-037-002 — "show how to" + requester code action routing

A: "Show the team how to configure the logging pipeline, then configure it yourself in staging."
Router decision: Predicates = ["IMPLEMENT"] → ImplementationFlow41 | CapabilityGapWarning: gap-skills-guidance-train-001
Evidence: "show how to configure" = gap warning; "configure it yourself in staging" = requester's IMPLEMENT action → IMPLEMENT routes to ImplementationFlow41.

---

## B34-router-037-003 — "demonstrate" routing = GapHandler

A: "Demonstrate the load testing process to the performance team."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-train-001
Evidence: demonstrate = guidance vocabulary → GapHandler. No code flow.

---

## B34-router-037-004 — "demonstrate" + TEST routing

A: "Demonstrate the testing framework and write the first 20 test cases."
Router decision: Predicates = ["TEST"] → UnitTestingFlow51 | CapabilityGapWarning: gap-skills-guidance-train-001
Evidence: "demonstrate testing framework" = gap warning; "write 20 test cases" = requester's TEST action → TEST routes to UnitTestingFlow51.

---

## B34-router-037-005 — "introduce [person]" routing = GapHandler

A: "Introduce the new developers to the company's coding standards."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-onboard-001
Evidence: "introduce [person] to [standards]" = onboarding vocabulary → GapHandler invoked. gap-skills-guidance-onboard-001 selected.
