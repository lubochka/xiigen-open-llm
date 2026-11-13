# Layer 0-D — Flow Factory Pairs (015)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## FF-015-001 — "Walk through" vocabulary: factory map

| Request | Predicates | Factory | TransitionType |
|---------|-----------|---------|----------------|
| "Walk through how the API works" | [] | GapHandler | none |
| "Walk through integrating the payment API" | [API_CONTRACT] | APIContractFlow33 | documentation |
| "Walk through writing the E2E tests" | [TEST] | E2ETestingFlow53 | documentation |
| "Walk through implementing the service + implement it" | [IMPLEMENT] | ImplementationFlow41 | feature_addition |
| "Walk through architecting + design the diagram" | [ARCHITECTURE] | ArchitectureFlow31 | documentation |
| "Walk through integrating + implement the flow" | [API_CONTRACT, IMPLEMENT] | APIContractFlow33 → ImplementationFlow41 | feature_addition |

---

## FF-015-002 — "Walk through" + explain equivalent → GapHandler

"Walk through the codebase" → GapHandler (explain existing artifact)
"Walk the team through the architecture decisions" → GapHandler (explain past decisions)
"Walk through the deployment process" → GapHandler (explain process; deploy ≠ IMPLEMENT trigger alone)
"Walk through the API documentation" → GapHandler (explain documentation; no documentation creation verb)

---

## FF-015-003 — "Walk through" gap warning ID assignment

"Walk through" in new-hire/onboarding context → gap-skills-guidance-onboard-001
"Walk through" in mentoring/explanation context → gap-skills-guidance-mentor-001
"Walk through the integration guide for new developers" → gap-skills-guidance-onboard-001 (onboarding context)
"Walk through the architecture for the senior engineers" → gap-skills-guidance-mentor-001 (mentoring context)
