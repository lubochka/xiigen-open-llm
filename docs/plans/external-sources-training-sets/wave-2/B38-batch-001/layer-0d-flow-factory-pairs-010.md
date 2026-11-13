# Layer 0-D — Flow Factory Pairs (010)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## B38-0D-010-001 — v6-02 factory corruption: probationary flow selected without Gate B pass

Corruption type: v6-02
Scenario: ReleaseGateFlow71_v2 is a probationary candidate

Input: "Review and approve the auth service release."
PredicateSet: ["QA", "IMPLEMENT"]
Gate B score: 0.63 (below threshold 0.85)

Correct factory behavior:
1. Probationary candidate selected (ReleaseGateFlow71_v2)
2. Gate B evaluated: 0.63 < 0.85 → FAIL
3. Fallback to ReleaseGateFlow71 (deterministic_generic)
4. probationary_rejected state recorded
5. Factory output contains deterministic_generic flow

v6-02 corrupted factory behavior:
1. Probationary candidate selected (ReleaseGateFlow71_v2)
2. Gate B evaluated: 0.63 < 0.85 → FAIL
3. (Bug): factory proceeds with probationary candidate anyway
4. ReleaseGateFlow71_v2 executes with gate_b_passed = false
5. Factory output shows probationary flow ran (WRONG)

---

## B38-0D-010-002 — Factory output comparison: correct vs v6-02

Correct output:
{
  "flow_id": "ReleaseGateFlow71",
  "flow_type": "deterministic_generic",
  "gate_b_checked": true,
  "gate_b_passed": "n/a (deterministic)",
  "result_state": "probationary_rejected_fallback_executed"
}

v6-02 corrupted output:
{
  "flow_id": "ReleaseGateFlow71_v2",
  "flow_type": "probationary_accepted",
  "gate_b_checked": true,
  "gate_b_passed": false,
  "result_state": "probationary_executed_without_gate" (WRONG)
}

The v6-02 corruption is detectable because gate_b_passed = false appears alongside a probationary flow execution record.

---

## B38-0D-010-003 — Factory invariant: probationary flow never executes when Gate B fails

Factory invariant (must hold at all times):
IF (flow_type == "probationary") AND (gate_b_score < gate_b_threshold) THEN:
  flow_executed = false
  fallback_flow_executed = true
  result_state = "probationary_rejected"

This invariant is violated by v6-02. Any factory implementation must enforce this before executing any flow.

---

## B38-0D-010-004 — Detecting v6-02 in production outputs

Audit check for v6-02:
1. Find all records where flow_type contains "probationary"
2. For each, check gate_b_passed value
3. If gate_b_passed = false AND flow was executed → v6-02 corruption detected
4. Remediation: re-run with deterministic_generic fallback; invalidate v6-02 result
