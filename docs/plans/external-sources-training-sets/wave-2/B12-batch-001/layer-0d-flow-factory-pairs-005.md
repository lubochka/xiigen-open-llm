# Layer 0-D — Flow Factory Pairs (005)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B12-0D-005-001 — probationary_rejected: 3.3 candidate fails Gate B

```
source_id: xiigen-b12-0d-005-001
flow_type: probationary_rejected
transition_type: documentation
phase: 3
sub_phase: 3.3
pair_kind: factory_selection
```

A:
  run_id: "B12-0D-005-001"
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  sub_phase_hint: "3.3"
  candidate_flow_id: "APIContractFlow33-candidate-v0-FAILED"
  gate_b_result: "FAIL"

B:
  FlowType: "probationary_rejected"
  RejectedFlow: "APIContractFlow33-candidate-v0-FAILED"
  SubPhase: "3.3"
  GateBStatus: "FAIL"
  GateBCondition: "Learned flow must produce correct CompiledPredicateIds for all test cases.
  Zero tolerance for false negatives on API_CONTRACT for 'contract' modifier patterns."
  GateBFailureReason: "Candidate flow failed test case 1: 'data contracts' → CompiledPredicateIds
  returned ['DOCUMENT'] instead of ['API_CONTRACT', 'DOCUMENT']. API_CONTRACT suppressed on
  'data' modifier — v6-01 corruption detected in candidate flow."
  FallbackFlow: "APIContractFlow33"  (deterministic_generic)
  FallbackFlowType: "deterministic_generic"
  FlowOrder: ["APIContractFlow33", "DocumentationFlow21"]
  FactoryReason: "Candidate rejected. Fallback to deterministic_generic for reliability."

Gate B evaluation (FAIL):
  Test case 1: "data contracts" → ["DOCUMENT"] — WRONG (expected ["API_CONTRACT", "DOCUMENT"])
  Failure: API_CONTRACT suppressed on "data" modifier — v6-01 corruption
  Decision: REJECT — fallback to deterministic_generic

---

## B12-0D-005-002 — probationary_rejected: different failure mode — wrong routing

```
source_id: xiigen-b12-0d-005-002
flow_type: probationary_rejected
pair_kind: factory_selection
```

A:
  candidate_flow_id: "APIContractFlow33-candidate-v0b-FAILED"
  gate_b_result: "FAIL"
  failure_reason: "routing"

B:
  GateBFailureReason: "Candidate flow correctly detected predicates but produced wrong routing.
  For CompiledPredicateIds: ['IMPLEMENT', 'API_CONTRACT', 'DOCUMENT', 'SECURITY'],
  candidate returned RoutingDecisions: [3.3 only] — v6-03 routing collapse detected.
  Correct routing: [4.3, 3.3, 2.1, 3.4]."
  FallbackFlow: "APIContractFlow33"  (deterministic_generic)
  FallbackFlowType: "deterministic_generic"

Gate B evaluation (FAIL):
  Test case: "Write the OpenAPI spec with authentication" → predicates correct
  But routing: [3.3 only] — WRONG (expected [4.3, 3.3, 2.1, 3.4])
  Failure: v6-03 routing collapse
  Decision: REJECT

---

## B12-0D-005-003 — probationary_rejected: SECURITY suppression failure mode

```
source_id: xiigen-b12-0d-005-003
flow_type: probationary_rejected
pair_kind: factory_selection
```

A:
  candidate_flow_id: "APIContractFlow33-candidate-v0c-FAILED"
  gate_b_result: "FAIL"

B:
  GateBFailureReason: "Candidate flow suppressed SECURITY predicate in API documentation context.
  For request 'Write the OpenAPI spec for the cart service and include authentication requirements',
  candidate returned ['IMPLEMENT', 'API_CONTRACT', 'DOCUMENT'] — SECURITY missing.
  Correct: ['IMPLEMENT', 'API_CONTRACT', 'DOCUMENT', 'SECURITY'].
  v6-02 corruption detected: authentication suppressed in API doc context."
  FallbackFlow: "APIContractFlow33"  (deterministic_generic)

Gate B evaluation (FAIL):
  Test: "OpenAPI spec with authentication requirements" → SECURITY missing
  Failure: v6-02 corruption
  Decision: REJECT

---

## B12-0D-005-004 — Rejected flow recovery: what happens after rejection

```
source_id: xiigen-b12-0d-005-004
flow_type: probationary_rejected
pair_kind: factory_selection
```

After probationary_rejected:

  1. Rejected flow is archived (not used in production)
  2. Fallback to deterministic_generic immediately
  3. Rejection reason is logged for training feedback
  4. Training team reviews failure modes:
     - v6-01 failures → add "contract regardless of modifier" training examples
     - v6-02 failures → add "authentication always fires SECURITY" training examples
     - v6-03 failures → add "each predicate routes independently" training examples
  5. New candidate flow is trained with corrected examples
  6. New candidate enters probationary evaluation
  7. Gate B re-evaluated with expanded test set (including the previously failed cases)

The B12-batch-001 training set specifically addresses these three failure modes:
  - Layer 0-A files 004, 011 → address v6-01 (contract modifier suppression)
  - Layer 0-A files 007, 011 → address v6-02 (authentication suppression)
  - Layer 0-A file 010, Layer 0-B file 010-011 → address v6-03 (routing collapse)

Purpose: future candidate flows trained on B12-batch-001 should pass Gate B for all
three corruption test cases, enabling promotion to probationary_accepted.
