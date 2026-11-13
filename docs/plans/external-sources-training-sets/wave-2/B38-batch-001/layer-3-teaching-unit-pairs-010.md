# Layer 3 — Teaching Unit Pairs (010)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B38-TU-010-001 — Teaching: v6 corruption patterns for B38

LESSON: Three v6 corruption patterns appear in B38. Each has a distinct symptom.

v6-01: QA Suppression
Symptom: PredicateSet = [] when QA terms are present in input
Example: "Review the rollout plan" → v6-01 produces [] (wrong); correct is ["QA"]
Cause: compiler incorrectly suppresses QA due to co-present gap vocabulary ("rollout")
Fix: gap vocabulary → CapabilityGapWarning only; does not suppress predicates

v6-02: Probationary Gate Bypass
Symptom: Probationary flow executes with gate_b_passed = false
Example: Gate B score = 0.63; probationary flow runs anyway (wrong); should fallback to deterministic
Cause: orchestrator skips Gate B check before executing probationary flow
Fix: Gate B check is mandatory; failed Gate B → fallback to deterministic_generic

v6-03: Multi-Predicate Collapse
Symptom: DecisionCount < expected; IMPLEMENT routing missing from output
Example: Input has "service" → ["QA","IMPLEMENT"] should produce 2 decisions; v6-03 produces 1
Cause: router collapses multi-predicate set to single highest-priority predicate
Fix: one routing decision per predicate (Rule 15); no collapse logic

---

## B38-TU-010-002 — Teaching: how to detect v6 corruptions in B38 output

Detection checklist:

v6-01 detection:
□ Input contains "review", "approve", "sign-off", or other QA terms
□ Output shows PredicateSet = []
→ If both true: v6-01 likely

v6-02 detection:
□ Output shows probationary flow type in flows_executed
□ gate_b_passed = false in output
→ If both true: v6-02 likely

v6-03 detection:
□ Input contains "service", "build", or "pipeline"
□ Output shows DecisionCount = 1 (only QA)
□ IMPLEMENT not in phase_routing
→ If all three true: v6-03 likely

---

## B38-TU-010-003 — Teaching: v6 corruptions are observable from N09 output

All three v6 corruptions produce observable anomalies in N09:

v6-01: result_state = knowledge_edge when it should be 7.1 routing
v6-02: probationary flow in flows_executed + gate_b_passed = false
v6-03: DecisionCount under-counted + IMPLEMENT phase missing from phase_routing

The N09 output is the primary diagnostic surface for v6 detection in B38.
