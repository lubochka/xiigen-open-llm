# Layer 0-D — Flow Factory Pairs (031)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## FF-031-001 — "introduce [person]" + IMPLEMENT factory

A: "Introduce the junior team to the hexagonal pattern and implement the hexagonal payment service."
Factory: ImplementationFlow41 | TransitionType: feature_addition | CapabilityGapWarning: gap-skills-guidance-onboard-001
Evidence: "introduce [person] to pattern" = gap warning; "implement hexagonal payment service" = IMPLEMENT → ImplementationFlow41.

---

## FF-031-002 — "familiarize" + API_CONTRACT factory

A: "Familiarize the backend team with the OAuth2 (OAuth 2.0) spec and integrate the identity provider."
Factory: APIContractFlow33 | TransitionType: documentation | CapabilityGapWarning: gap-skills-guidance-onboard-001
Evidence: "familiarize with OAuth2 spec" = gap warning; "integrate identity provider" = API_CONTRACT → APIContractFlow33.

---

## FF-031-003 — "run through" + DOCUMENT factory

A: "Run through the technical writing standards with the team and write the service migration guide."
Factory: DocumentationFlow | TransitionType: documentation | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "run through standards with team" = gap warning; "write service migration guide" = DOCUMENT → DocumentationFlow.

---

## FF-031-004 — "review with" + TEST factory

A: "Review the test matrix with the QA (Quality Assurance) lead and write the E2E (End-to-End) test suite."
Factory: E2ETestingFlow53 | TransitionType: documentation | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "review matrix with QA lead" = gap warning; "write E2E test suite" = TEST → E2ETestingFlow53.

---

## FF-031-005 — "check with" + IMPLEMENT factory (new Round 3 vocabulary)

A: "Check with the platform team on the best approach and implement the selected solution."
Factory: ImplementationFlow41 | TransitionType: feature_addition | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "check with platform team" = gap warning (check with = advisory vocabulary); "implement selected solution" = IMPLEMENT → ImplementationFlow41.
