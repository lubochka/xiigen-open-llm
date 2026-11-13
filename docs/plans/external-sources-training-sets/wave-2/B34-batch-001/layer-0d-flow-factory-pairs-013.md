# Layer 0-D — Flow Factory Pairs (013)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## FF-013-001 — Shadow vocabulary: factory map

| Request | Predicates | Factory | TransitionType |
|---------|-----------|---------|----------------|
| "Shadow the engineer on deployment" | [] | GapHandler | none |
| "Shadow and build the same caching layer" | [IMPLEMENT] | ImplementationFlow41 | feature_addition |
| "Shadow and write the runbook" | [DOCUMENT] | DocumentationFlow | documentation |
| "Shadow and write regression tests" | [TEST] | UnitTestingFlow51 | documentation |

---

## FF-013-002 — Gap warning factory behavior

When guidance verb + code predicate fires:
- Factory: selected normally based on predicate
- TransitionType: based on predicate, not guidance frame
- Gap warning: appended to factory output as CapabilityGapWarning
- GapHandler: NOT invoked separately

Example:
"Shadow and build the auth module" → ImplementationFlow41 selected; CapabilityGapWarning: "shadow = guidance vocabulary, gap-skills-guidance-mentor-001"; TransitionType: feature_addition.

---

## FF-013-003 — Multiple embedded actions: factory chaining

"Shadow, then build the service and write tests for it."
Predicates: ["IMPLEMENT", "TEST"]
Factories: ImplementationFlow41 → UnitTestingFlow51
TransitionType: feature_addition
Gap warning: "shadow" guidance vocabulary noted.
