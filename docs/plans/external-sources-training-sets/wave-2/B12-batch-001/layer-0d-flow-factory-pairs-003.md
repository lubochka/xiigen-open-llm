# Layer 0-D — Flow Factory Pairs (003)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B12-0D-003-001 — probationary_accepted: 3.3 candidate passes Gate B

```
source_id: xiigen-b12-0d-003-001
flow_type: probationary_accepted
transition_type: documentation
phase: 3
sub_phase: 3.3
pair_kind: factory_selection
```

A:
  run_id: "B12-0D-003-001"
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  sub_phase_hint: "3.3"
  candidate_flow_id: "APIContractFlow33-learned-v1"
  gate_b_result: "PASS"

B:
  FlowType: "probationary_accepted"
  SelectedFlow: "APIContractFlow33-learned-v1"
  SubPhase: "3.3"
  TransitionType: "documentation"
  GateBStatus: "PASS"
  GateBCondition: "Learned flow produces correct CompiledPredicateIds AND correct routing
  decisions for minimum 5 API_CONTRACT test cases in the probationary evaluation set."
  GateBResult: "PASS — 6 of 6 test cases correct"
  FactoryReason: "Candidate APIContractFlow33-learned-v1 passed Gate B evaluation. Selected
  over deterministic_generic as probationary_accepted flow."
  FlowOrder: ["APIContractFlow33-learned-v1", "DocumentationFlow21"]

Gate B evaluation (probationary acceptance):
  Test case 1: [webhook contract] → API_CONTRACT ✓
  Test case 2: [data contracts] → API_CONTRACT ✓
  Test case 3: [gRPC service contract] → API_CONTRACT ✓
  Test case 4: [OpenAPI spec documentation] → API_CONTRACT ✓
  Test case 5: [GraphQL schema with API context] → API_CONTRACT ✓
  Test case 6: [REST endpoint specification] → API_CONTRACT ✓
  Pass rate: 6/6 (100%) → PASS

---

## B12-0D-003-002 — probationary_accepted: gate b pass details

```
source_id: xiigen-b12-0d-003-002
flow_type: probationary_accepted
pair_kind: factory_selection
```

Gate B conditions for probationary_accepted status:

Minimum requirements:
  1. Correct CompiledPredicateIds for all test cases (zero false negatives on API_CONTRACT)
  2. Correct routing decisions (3.3 present when API_CONTRACT fires)
  3. Correct TransitionType (documentation when IMPLEMENT absent, feature_addition when present)
  4. Minimum 5 test cases evaluated

Pass threshold: all N test cases correct (100% required for probationary acceptance)

Probationary period: the flow is marked probationary_accepted and used in production.
After 10 additional production uses without error → promoted to trusted_learned.

---

## B12-0D-003-003 — probationary_accepted flow selection priority

```
source_id: xiigen-b12-0d-003-003
flow_type: probationary_accepted
pair_kind: factory_selection
```

When both probationary_accepted and deterministic_generic are available:

  Priority: probationary_accepted selected over deterministic_generic
  Reason: probationary_accepted has empirical evidence of correctness (Gate B pass)
  Risk: slightly higher risk than trusted_learned (probationary = not yet fully proven)

  Factory selection order:
    1. trusted_learned (if available for this predicate pattern)
    2. probationary_accepted (if available and Gate B passed)
    3. deterministic_generic (reliable fallback)
    4. [probationary_rejected should not be selected — fallback to deterministic_generic]

---

## B12-0D-003-004 — Transition from probationary_accepted to trusted_learned

```
source_id: xiigen-b12-0d-003-004
flow_type: probationary_accepted
pair_kind: factory_selection
```

Promotion process: probationary_accepted → trusted_learned

Conditions for promotion:
  1. Gate B: passed (required for probationary_accepted)
  2. Production uses: 10 additional production uses without classification error
  3. No regression: new B12 test cases continue to pass after promotion
  4. Manual review: optional but recommended for complex predicate patterns

After promotion:
  FlowType changes from "probationary_accepted" to "trusted_learned"
  Flow is used with highest priority for its predicate pattern
  Fallback to deterministic_generic retained for cases outside learned pattern

For B12 [API_CONTRACT, DOCUMENT]:
  Current state: probationary_accepted (Gate B passed in initial evaluation)
  Next milestone: 10 production uses → promotion to trusted_learned
  Expected outcome: trusted_learned flow for API contract documentation scenarios
