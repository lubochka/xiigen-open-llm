# Layer 1 — Transition Orchestrator Pairs (011)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B38-L1-011-001 — QA ordering rule: examples in B38 context

Rule 12 (QA ordering rule absolute): QA always last in any multi-predicate pipeline.

Examples in B38:

Example 1: [QA, IMPLEMENT]
Correct order: IMPLEMENT (4.1) at index 0, QA (7.1) at index 1
Violation: QA at index 0, IMPLEMENT at index 1

Example 2: [QA, DOCUMENT]
Correct: DOCUMENT (2.1) first, QA (7.1) second
Violation: QA first, DOCUMENT second

Example 3: [QA, SECURITY]
Correct: SECURITY (3.4) first, QA (7.1) second
Violation: QA first, SECURITY second

Example 4: [QA, IMPLEMENT, DOCUMENT, SECURITY]
Correct: IMPLEMENT (1), DOCUMENT (2), SECURITY (3), QA (4)
Violation: any position with QA before another predicate's flow

---

## B38-L1-011-002 — QA ordering rule for single predicate [QA]

When PredicateSet = ["QA"]:
- QA is the only flow
- Ordering rule is trivially satisfied
- No calculation needed
- execution_order = 1

The ordering rule only has observable effect when there are 2+ predicates.

---

## B38-L1-011-003 — QA ordering rule does not affect input parsing

The QA ordering rule is an orchestrator rule, not a compiler rule.
The compiler fires predicates based on input vocabulary — order of terms in input is irrelevant to predicate firing.
The orchestrator then sequences the flows according to the ordering rule.

Input: "Approve the release after security audit and criteria validation"
- Predicates fired: ["QA", "SECURITY", "DOCUMENT"]
- Orchestrator order: DOCUMENT (2.1) → SECURITY (3.4) → QA (7.1)
- "Approve" appears first in input but QA executes last in orchestration

---

## B38-L1-011-004 — QA ordering rule in edge cases

Edge case: What if only non-QA predicates fire?
Example: ["IMPLEMENT"] alone (no QA terms)
→ Orchestrator runs BackendServiceFlow41
→ QA ordering rule does not apply (QA not present)
→ execution_order = 1 for IMPLEMENT

Edge case: ["IMPLEMENT", "DOCUMENT"] (no QA)
→ Orchestrator runs IMPLEMENT → DOCUMENT (conventional order; QA rule inactive)
→ QA ordering rule only activates when QA is in the predicate set
