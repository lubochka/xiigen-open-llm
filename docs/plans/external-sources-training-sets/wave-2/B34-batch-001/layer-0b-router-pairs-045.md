# Layer 0-B — Router Pairs (045)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-045-001 — "show how to" + guidance topic = GapHandler (topic irrelevant)

A: "Show the team how to use the observability stack for debugging."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-train-001
Evidence: "show how to use" = teaching frame → GapHandler. Observability topic does not change routing.

---

## B34-router-045-002 — "brief" + IMPLEMENT routing

A: "Brief the engineers on the feature requirements and implement the user search functionality."
Router decision: Predicates = ["IMPLEMENT"] → ImplementationFlow41 | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "brief engineers" = gap warning; "implement user search functionality" = IMPLEMENT → ImplementationFlow41.

---

## B34-router-045-003 — "familiarize" + TEST routing

A: "Familiarize the QA (Quality Assurance) team with the test infrastructure and write the smoke test suite."
Router decision: Predicates = ["TEST"] → UnitTestingFlow51 | CapabilityGapWarning: gap-skills-guidance-onboard-001
Evidence: "familiarize QA team" = gap warning; "write smoke test suite" = TEST → UnitTestingFlow51.

---

## B34-router-045-004 — "run through" + DOCUMENT routing

A: "Run through the documentation standards with the team and document the service SLA (Service Level Agreement)."
Router decision: Predicates = ["DOCUMENT"] → DocumentationFlow | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "run through standards with team" = gap warning; "document service SLA" = DOCUMENT → DocumentationFlow.

---

## B34-router-045-005 — "review with" + API_CONTRACT routing

A: "Review the integration specification with the partner team and integrate the new webhook endpoint."
Router decision: Predicates = ["API_CONTRACT"] → APIContractFlow33 | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "review spec with partner team" = gap warning; "integrate new webhook endpoint" = direct integrate → API_CONTRACT → APIContractFlow33.
