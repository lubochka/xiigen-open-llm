# Layer 0-B — Router Pairs (042)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-042-001 — "get feedback on" + DOCUMENT routing

A: "Get feedback on the documentation structure and then write the full API (Application Programming Interface) reference."
Router decision: Predicates = ["DOCUMENT"] → DocumentationFlow | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "get feedback on structure" = gap warning; "write full API reference" = requester's DOCUMENT action → DOCUMENT routes to DocumentationFlow.

---

## B34-router-042-002 — "sanity check" + TEST routing

A: "Sanity check the test matrix with the QA (Quality Assurance) lead and write the missing edge-case tests."
Router decision: Predicates = ["TEST"] → UnitTestingFlow51 | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "sanity check with QA lead" = gap warning; "write edge-case tests" = requester's TEST action → TEST routes to UnitTestingFlow51.

---

## B34-router-042-003 — "second opinion" + API_CONTRACT routing

A: "Get a second opinion on the API (Application Programming Interface) versioning approach and integrate the v2 endpoints with the gateway."
Router decision: Predicates = ["API_CONTRACT"] → APIContractFlow33 | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "second opinion on API versioning" = gap warning; "integrate v2 endpoints with gateway" = requester's direct integrate action → API_CONTRACT routes to APIContractFlow33.

---

## B34-router-042-004 — multiple advisory verbs + single IMPLEMENT routing

A: "Consult with the team, get a second opinion from the architect, and implement the selected approach."
Router decision: Predicates = ["IMPLEMENT"] → ImplementationFlow41 | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: consult + second opinion = gap warnings (multiple, consolidated); "implement the selected approach" = requester's IMPLEMENT action → single IMPLEMENT flow.

---

## B34-router-042-005 — GapHandler suppression rule: any code predicate suppresses GapHandler

Router rule: GapHandler is suppressed whenever predicate set ≠ [].
Guidance vocabulary present alongside code predicates → CapabilityGapWarning emitted (not GapHandler invoked).
GapHandler invoked ONLY when predicate set = [] after full scan of request.
SCHEDULE_CAPABILITY_LEARNING NOT emitted when GapHandler suppressed.
