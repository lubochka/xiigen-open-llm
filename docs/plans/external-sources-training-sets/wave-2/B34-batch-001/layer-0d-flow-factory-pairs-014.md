# Layer 0-D — Flow Factory Pairs (014)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## FF-014-001 — "Pair with" vocabulary: factory map

| Request | Predicates | Factory | TransitionType |
|---------|-----------|---------|----------------|
| "Pair with the team on API work" | [] | GapHandler | none |
| "Pair with to write unit tests" | [TEST] | UnitTestingFlow51 | documentation |
| "Pair with to integrate the SSO" | [API_CONTRACT] | APIContractFlow33 | documentation |
| "Pair with to implement the service" | [IMPLEMENT] | ImplementationFlow41 | feature_addition |
| "Pair with to implement + write tests" | [IMPLEMENT, TEST] | ImplementationFlow41 → UnitTestingFlow51 | feature_addition |

---

## FF-014-002 — "Pair with to learn" → GapHandler (learning intent pattern)

"Pair with the team to learn the codebase" → GapHandler
"Pair with to understand the architecture" → GapHandler
"Pair with to gain familiarity with the API" → GapHandler

"to learn/understand/gain familiarity" = learning-intent infinitives → gap. No code verb fires.
Contrast: "pair with to implement" → IMPLEMENT fires (implement = code verb, not learning verb).

---

## FF-014-003 — "Pair with" + collaborative write → TEST fires

"Pair with me to write the contract tests together" → TEST fires
"Pair on writing E2E tests" → TEST fires (writing = embedded action)
"Pair with to run tests" → gap (run ≠ write; no TEST fires)

Rule: The word "pair with" enables collaborative framing but does not change the embedded verb analysis. Write/create + test noun → TEST regardless of collaboration framing.
