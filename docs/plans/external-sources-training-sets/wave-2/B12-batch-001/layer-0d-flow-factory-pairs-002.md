# Layer 0-D — Flow Factory Pairs (002)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B12-0D-002-001 — deterministic_generic: 3.3 API contract documentation

```
source_id: xiigen-b12-0d-002-001
flow_type: deterministic_generic
transition_type: documentation
phase: 3
sub_phase: 3.3
pair_kind: factory_selection
```

A:
  run_id: "B12-0D-002-001"
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  sub_phase_hint: "3.3"
  flow_registry_query: "3.3"

B:
  FlowType: "deterministic_generic"
  SelectedFlow: "APIContractFlow33"
  SubPhase: "3.3"
  TransitionType: "documentation"
  FactoryReason: "API_CONTRACT predicate routes to 3.3 sub-phase. No learned flow registered
  for this predicate combination. deterministic_generic selected as reliable fallback."
  SecondaryFlow: "DocumentationFlow21"
  FlowOrder: ["APIContractFlow33", "DocumentationFlow21"]

Factory logic:
  1. CompiledPredicateIds contains API_CONTRACT → query flow registry for 3.3
  2. No trusted_learned or probationary_accepted flow registered for [API_CONTRACT, DOCUMENT] → fallback
  3. deterministic_generic selected: uses sub_phase_hint "3.3" to route to APIContractFlow33
  4. DOCUMENT predicate → DocumentationFlow21 appended as secondary flow
  5. FlowOrder: APIContractFlow33 first (primary), DocumentationFlow21 second

---

## B12-0D-002-002 — deterministic_generic: factory selection for single-predicate case

```
source_id: xiigen-b12-0d-002-002
flow_type: deterministic_generic
transition_type: documentation
phase: 3
sub_phase: 3.3
pair_kind: factory_selection
```

A:
  CompiledPredicateIds: ["API_CONTRACT"]
  sub_phase_hint: "3.3"

B:
  FlowType: "deterministic_generic"
  SelectedFlow: "APIContractFlow33"
  SubPhase: "3.3"
  TransitionType: "documentation"
  FactoryReason: "Single API_CONTRACT predicate. deterministic_generic maps directly to APIContractFlow33."
  SecondaryFlow: null
  FlowOrder: ["APIContractFlow33"]

---

## B12-0D-002-003 — deterministic_generic: DOCUMENT alone → DocumentationFlow

```
source_id: xiigen-b12-0d-002-003
flow_type: deterministic_generic
transition_type: documentation
phase: 2
sub_phase: 2.1
pair_kind: factory_selection
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  sub_phase_hint: "2.1"
  (request: "Define the database schema for the inventory table" — no API_CONTRACT)

B:
  FlowType: "deterministic_generic"
  SelectedFlow: "DocumentationFlow21"
  SubPhase: "2.1"
  TransitionType: "documentation"
  FactoryReason: "Single DOCUMENT predicate. No API_CONTRACT. deterministic_generic routes
  to Phase 2 documentation sub-phase."
  SecondaryFlow: null
  FlowOrder: ["DocumentationFlow21"]

---

## B12-0D-002-004 — Factory comparison: deterministic_generic vs probationary vs trusted_learned

```
source_id: xiigen-b12-0d-002-004
pair_kind: factory_reference
```

Factory type hierarchy for flow selection:

1. trusted_learned (highest priority)
   Used when: a learned flow has been promoted to trusted status for this predicate pattern
   Confidence: highest (Gate B passed + usage confirmed)
   Gate B: passed in probationary phase

2. probationary_accepted (second priority)
   Used when: a candidate flow has been provisionally accepted (Gate B not yet fully confirmed)
   Confidence: medium-high
   Gate B: provisional pass

3. deterministic_generic (fallback)
   Used when: no learned flow registered for this predicate pattern
   Confidence: reliable baseline (rule-based, always works)
   Gate B: N/A (rules-based, no gate)

4. probationary_rejected (error state — should not be selected)
   Status: rejected; fallback to deterministic_generic applied

For new B12 predicates without registered learned flows → deterministic_generic is selected.
As the system learns from B12 training data, probationary_accepted flows will be registered
and eventually promoted to trusted_learned.
