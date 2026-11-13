# Layer 0-D — Flow Factory Pairs (005)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## B38-0D-005-001 — probationary_rejected: 7.1 candidate fails Gate B → fallback

FlowType: probationary_rejected
Scenario: ReleaseGateFlow71_v2 candidate fails Gate B

Gate B condition: predicate_confidence >= 0.85
Gate B evaluation: confidence = 0.62 → FAIL

Factory output:
{
  "flow_id": "ReleaseGateFlow71_v2",
  "flow_type": "probationary_rejected",
  "phase": "7.1",
  "gate_b_passed": false,
  "gate_b_score": 0.62,
  "gate_b_threshold": 0.85,
  "fallback_flow_id": "ReleaseGateFlow71",
  "fallback_flow_type": "deterministic_generic",
  "entry_state": "probationary_pending",
  "exit_state": "probationary_rejected"
}

---

## B38-0D-005-002 — What happens after probationary_rejected

1. Probationary flow is NOT executed
2. Fallback flow (deterministic_generic) is selected
3. Fallback executes in place of the probationary candidate
4. Output is produced by the deterministic_generic flow
5. Gate B failure is recorded for the probationary candidate
6. Probationary candidate remains in probationary state (not demoted further)

---

## B38-0D-005-003 — probationary_rejected vs v6-02 corruption

Correct behavior (probationary_rejected):
- Gate B checked → fails → fallback selected → deterministic_generic executes
- Flow output: produced by fallback

v6-02 corrupted behavior:
- Gate B checked → fails → probationary flow executes anyway (bypass)
- Flow output: produced by unvalidated probationary flow

Observable difference:
- Correct: flow_type in output = "deterministic_generic" (fallback); gate_b_passed = false
- Corrupt: flow_type in output = "probationary_accepted" (wrong type); gate_b_passed = false but flow ran

---

## B38-0D-005-004 — Gate B failure causes in B38

Low Gate B scores (< 0.85) typically occur when:
1. Input uses indirect QA language (e.g., "please check the release" — "check" is borderline)
2. Predicate confidence degraded by ambiguous vocabulary
3. Co-occurring gap vocabulary reduces confidence of predicate attribution
4. Short input with single weak QA signal

High Gate B scores (>= 0.85) typically occur when:
1. Multiple clear QA terms present (review + approve + sign-off)
2. Direct predicate-domain vocabulary used
3. No ambiguous borderline terms
