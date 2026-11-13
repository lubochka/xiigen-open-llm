# Layer 0-D — Flow Factory Pairs (012)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## FF-012-001 — Complete factory map for B34

| Predicates | Factory Selection | TransitionType |
|-----------|------------------|----------------|
| [] | GapHandler | none |
| [DOCUMENT] | DocumentationFlow | documentation |
| [IMPLEMENT] | ImplementationFlow41 | feature_addition |
| [TEST] | UnitTestingFlow51 or E2ETestingFlow53 | documentation |
| [QA] | QAGatingFlow73 or ReleaseGateFlow71 | no contribution |
| [ARCHITECTURE] | ArchitectureFlow31 or 35 | documentation |
| [API_CONTRACT] | APIContractFlow33 | documentation |
| [IMPLEMENT+DOCUMENT] | ImplementationFlow41 + DocumentationFlow | feature_addition |
| [IMPLEMENT+TEST+QA] | Impl + Test + QA (QA last) | feature_addition |
| Multiple w/ IMPLEMENT | All factories + IMPLEMENT | feature_addition |
| refactor verb present | ImplementationFlow41 | refactoring |

---

## FF-012-002 — Gap warning vs full gap

Full gap (GapHandler + SCHEDULE_CAPABILITY_LEARNING): when predicate set = []
Capability gap warning (no GapHandler): when guidance vocab present alongside fired predicates
"Build tracker and mentor the team" → IMPLEMENT fires → gap warning only, not full gap log
"Mentor the team" → gap only → full gap log + SCHEDULE_CAPABILITY_LEARNING

---

## FF-012-003 — "run a [noun]" disambiguation

"run a workshop" → gap ("run" as facilitation verb + event noun)
"run unit tests" → gap ("run" is not a test-writing verb; running tests ≠ TEST predicate)
"run the CI pipeline" → gap ("run" = execution, not predicate-firing)
"write a workshop plan" → DOCUMENT (write + documentation artifact)

Rule: "run" is never a predicate-firing verb in any context. TEST fires on write/create/assert, not run/execute.
