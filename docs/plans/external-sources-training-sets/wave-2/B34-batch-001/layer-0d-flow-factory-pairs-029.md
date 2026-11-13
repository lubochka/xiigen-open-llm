# Layer 0-D — Flow Factory Pairs (029)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## FF-029-001 — "consult" + ARCHITECTURE factory

A: "Consult with the principal engineer on the caching strategy and architect the distributed cache layer."
Factory: ArchitectureFlow31 | TransitionType: documentation | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "consult on strategy" = gap warning; "architect the distributed cache layer" = ARCHITECTURE → ArchitectureFlow31.

---

## FF-029-002 — "get feedback" + DOCUMENT factory

A: "Get feedback on the runbook structure from the ops team and write the deployment runbook."
Factory: DocumentationFlow | TransitionType: documentation | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "get feedback on structure" = gap warning; "write deployment runbook" = DOCUMENT → DocumentationFlow.

---

## FF-029-003 — "sanity check" + TEST factory

A: "Sanity check the test coverage approach with the QA (Quality Assurance) lead and write the missing integration tests."
Factory: UnitTestingFlow51 | TransitionType: documentation | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "sanity check with QA lead" = gap warning; "write missing integration tests" = TEST → UnitTestingFlow51.

---

## FF-029-004 — "second opinion" + API_CONTRACT factory

A: "Get a second opinion on the event schema design and integrate the new event types with the consumer services."
Factory: APIContractFlow33 | TransitionType: documentation | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "second opinion on schema design" = gap warning; "integrate event types with consumer services" = API_CONTRACT → APIContractFlow33.

---

## FF-029-005 — "rubber duck" + IMPLEMENT factory

A: "Rubber duck the rate limiter design and implement the token bucket algorithm in the API (Application Programming Interface) gateway."
Factory: ImplementationFlow41 | TransitionType: feature_addition | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "rubber duck rate limiter design" = gap warning; "implement token bucket algorithm" = IMPLEMENT → ImplementationFlow41.
