# Layer 0-D — Flow Factory Pairs (020)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## FF-020-001 — Guidance + IMPLEMENT embedded: factory selection examples

| Request | Predicates | Factory | TransitionType |
|---------|-----------|---------|----------------|
| "Coach through building the service" | [IMPLEMENT] | ImplementationFlow41 | feature_addition |
| "Mentor through implementing the pipeline" | [IMPLEMENT] | ImplementationFlow41 | feature_addition |
| "Guide through refactoring the module" | [IMPLEMENT] | ImplementationFlow41 | refactoring |
| "Coach on how to build" (teach-to pattern) | [] | GapHandler | none |
| "Walk through building + implement it" | [IMPLEMENT] | ImplementationFlow41 | feature_addition |
| "Coach through building + write tests" | [IMPLEMENT, TEST] | ImplementationFlow41 → UnitTestingFlow51 | feature_addition |

---

## FF-020-002 — Refactoring sub-type in guidance context

"Guide through refactoring the auth service" → [IMPLEMENT] | TransitionType: refactoring
"Coach through cleaning up the legacy code" → [IMPLEMENT] | TransitionType: refactoring (clean-up = refactoring intent)
"Mentor through extracting the domain layer" → [IMPLEMENT] | TransitionType: refactoring (extraction = structural refactoring)

Guidance frame does not change TransitionType. Refactoring intent inside guidance frame → refactoring TransitionType.

---

## FF-020-003 — "Help [person] implement" → depends on whether requester implements

"Help the junior dev implement the feature" → [IMPLEMENT] | ImplementationFlow41
"Help implement" = requester participates in implementation → IMPLEMENT fires
"Help the team understand implementation patterns" → [] | GapHandler (understanding = learning intent = gap)
"Help by implementing the example" → [IMPLEMENT] | ImplementationFlow41 (requester implements the example)
