# Layer 0-D — Flow Factory Pairs (003)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## B38-0D-003-001 — probationary_accepted: 7.1 candidate passes Gate B

FlowType: probationary_accepted
Scenario: ReleaseGateFlow71 (candidate version, recently introduced)
PredicateSet: ["QA", "IMPLEMENT"]

Gate B condition: predicate_confidence >= 0.85 AND QA term directly present in input
Gate B evaluation: confidence = 0.91 → PASS

Factory output:
{
  "flow_id": "ReleaseGateFlow71_v2",
  "flow_type": "probationary_accepted",
  "phase": "7.1",
  "gate_b_passed": true,
  "gate_b_score": 0.91,
  "gate_b_threshold": 0.85,
  "entry_state": "probationary_pending",
  "exit_state": "probationary_accepted",
  "transition_type": "feature_addition"
}

---

## B38-0D-003-002 — Gate B evaluation process for probationary flows

Step 1: Compiler fires predicate set ["QA", "IMPLEMENT"]
Step 2: Router selects probationary candidate ReleaseGateFlow71_v2
Step 3: Gate B evaluator calculates confidence:
  - QA term count in input: 2 (review + approve) → confidence contribution
  - IMPLEMENT term count: 1 (service) → confidence contribution
  - Combined confidence = 0.91
Step 4: Gate B threshold = 0.85
Step 5: 0.91 >= 0.85 → PASS
Step 6: Flow executes as probationary_accepted
Step 7: Result recorded with gate_b_passed: true

---

## B38-0D-003-003 — Differences between probationary_accepted and deterministic_generic

| Property | deterministic_generic | probationary_accepted |
|---|---|---|
| Gate B required | No | Yes |
| Registry status | Validated | Candidate |
| Execution risk | Low | Medium (gate B mitigates) |
| Output difference | None visible externally | Gate B score appended |
| Promotion path | Already promoted | Can be promoted after N successes |

---

## B38-0D-003-004 — Post-acceptance promotion tracking

After a probationary flow is accepted N times without failure, it may be promoted to trusted_learned.
In B38, the promotion threshold is not specified in this batch, but the pathway is:

probationary → probationary_accepted (N times) → trusted_learned → deterministic_generic (eventual)

File 004 in this layer covers the trusted_learned state.
