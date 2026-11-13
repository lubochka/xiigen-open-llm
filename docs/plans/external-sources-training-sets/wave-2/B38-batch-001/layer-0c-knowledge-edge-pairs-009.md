# Layer 0-C — Knowledge-Edge Pairs (009)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B38-0C-009-001 — v6 corruption types overview for B38

Three v6 corruption patterns appear in B38 test cases.

v6-01: QA Suppression
- The compiler incorrectly treats gap vocabulary as a context that invalidates QA firing.
- A sentence with "checklist", "rollout", or "go-live" should produce CapabilityGapWarnings.
- v6-01 instead suppresses any QA that would have fired from co-present terms like "review" or "approve".
- Correct behavior: gap vocabulary → CapabilityGapWarning. Predicates that fire from other terms are unaffected.

v6-02: Probationary Gate Bypass
- The orchestrator selects a probationary flow candidate and executes it without verifying Gate B.
- Probationary flows require Gate B (confidence threshold) to be passed before execution.
- v6-02 skips the Gate B check and executes the probationary flow regardless.
- Correct behavior: Gate B fails → fallback to deterministic_generic (probationary_rejected state).

v6-03: Multi-Predicate Collapse
- The router receives multiple predicates but collapses them to a single routing decision.
- ["QA", "IMPLEMENT"] → router collapses → only QA routed to 7.1; IMPLEMENT 4.1 skipped.
- DecisionCount under-counted; TransitionType wrong (no_contribution instead of feature_addition).
- Correct behavior: each predicate → one independent routing decision; Rule 15.

---

## B38-0C-009-002 — v6-01 detection and correction

Detection:
- Input contains QA-firing terms (review, approve, sign-off, etc.)
- Input also contains gap vocabulary (checklist, rollout, go-live, etc.)
- Output shows PredicateSet = [] (WRONG)
- Expected PredicateSet = ["QA"]

Correction:
- The QA predicate fires from "review"/"approve"/etc., not from "checklist"/"rollout"
- Each word is evaluated independently
- Gap vocabulary does not suppress predicates; it only adds CapabilityGapWarnings

---

## B38-0C-009-003 — v6-02 detection and correction

Detection:
- Flow state shows probationary flow selected
- No Gate B pass record in output
- Result may look correct (flow completed) but was not validated

Correction:
- Check Gate B evaluation record for the flow selection
- If Gate B score < threshold, result state should be probationary_rejected
- Fallback flow (deterministic_generic) should have been selected
- Rerun with deterministic_generic; mark original probationary flow as failed

---

## B38-0C-009-004 — v6-03 detection and correction

Detection:
- Input contains IMPLEMENT-firing terms (service, build, pipeline)
- Output DecisionCount = 1 (only QA routed)
- IMPLEMENT not in routing table
- TransitionType = no_contribution (should be feature_addition when IMPLEMENT present)

Correction:
- Router must receive full predicate set from compiler
- One routing decision per predicate (Rule 15)
- For ["QA", "IMPLEMENT"]: two entries in routing table (4.1 and 7.1)
- DecisionCount = 2
- TransitionType = feature_addition
