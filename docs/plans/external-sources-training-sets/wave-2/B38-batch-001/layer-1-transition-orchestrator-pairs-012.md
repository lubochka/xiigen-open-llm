# Layer 1 — Transition Orchestrator Pairs (012)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B38-L1-012-001 — v6 ordering violations for B38

v6-03 (ordering variant): QA placed first instead of last

Input: "Review the payment service and validate the release criteria."
Correct predicate set: ["QA", "IMPLEMENT", "DOCUMENT"]
Correct order: IMPLEMENT (1) → DOCUMENT (2) → QA (3)

v6-03 ordering violation:
QA (1) → IMPLEMENT (2) → DOCUMENT (3)

Effect:
- ReleaseGateFlow71 runs before BackendServiceFlow41
- Gate decision made without implementation review evidence
- Gate output does not reference service_review.md
- Result may be superficially complete but lacks implementation evidence

---

## B38-L1-012-002 — v6-01 ordering side effect

v6-01 (QA suppression) also affects ordering:

When v6-01 suppresses QA:
- Predicate set becomes ["IMPLEMENT", "DOCUMENT"]
- No QA flow to order
- Orchestrator runs IMPLEMENT → DOCUMENT only
- Release gate (7.1) never executes
- No gate decision produced

The ordering violation here is: gate decision is missing entirely, not just missequenced.

---

## B38-L1-012-003 — v6-02 ordering side effect

v6-02 (probationary gate bypass):
Ordering is not directly corrupted, but the flow that executes is unvalidated.

Correct result: deterministic_generic ReleaseGateFlow71 runs at last position
v6-02 result: probationary ReleaseGateFlow71_v2 runs at last position (wrong flow, correct position)

The ordering position is preserved but the flow identity is wrong. The gate decision is produced by an unvalidated flow.

---

## B38-L1-012-004 — Orchestrator assertion summary for B38

Assertions the orchestrator must satisfy before returning N09 result:

1. len(flows_executed) == len(predicate_set) (or 0 if knowledge_edge)
2. If "QA" in predicate_set: ReleaseGateFlow71 must have the highest execution_order
3. If predicate_set == []: result_state must be knowledge_edge
4. If predicate_set != []: result_state must not be knowledge_edge
5. If IMPLEMENT in predicate_set: transition_type must be feature_addition
6. If IMPLEMENT not in predicate_set and predicate_set != []: transition_type must be no_contribution
7. All probationary flows must have gate_b_passed = true before being included in flows_executed
8. No predicate's flow may be omitted (Rule 15: two predicates = two decisions)
