# Layer 0-A — Compiler Pairs (011)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: compiler

---

## B38-0A-011-001 — v6-01 corruption: QA suppressed for "checklist" context

Corruption type: v6-01
Description: Corrupted compiler incorrectly suppresses QA when the sentence mentions a "checklist", treating checklist context as evidence that no quality review is happening.

Input:
"Review the deployment checklist and approve the release."

Correct result:
- "Review" → QA
- "checklist" → GAP (CapabilityGapWarning only — checklist is gap vocabulary, does not suppress predicates)
- "approve" → QA
- Predicates fired: ["QA"]
- CapabilityGapWarnings: ["deployment", "checklist"]

v6-01 corrupted result:
- Compiler sees "checklist" and applies suppression rule
- QA suppressed — "this is checklist work, not a review"
- Predicates fired: [] (WRONG)
- ResultState: knowledge_edge (WRONG — should be 7.1 routing)

Why this is wrong:
Rule 5 states checklist is B38 gap vocabulary, meaning it produces a CapabilityGapWarning. It does NOT suppress predicates that have already fired from other terms. "Review" and "approve" are independent QA triggers. The presence of gap vocabulary never cancels predicate fires from non-gap terms. v6-01 incorrectly treats gap vocabulary as a context invalidator.

---

## B38-0A-011-002 — v6-02 corruption: probationary flow accepted despite Gate B failure

Corruption type: v6-02
Description: Corrupted orchestrator selects probationary flow for 7.1 and proceeds to execution even though the Gate B condition has not been satisfied.

Scenario:
- Flow candidate: ReleaseGateFlow71 (probationary)
- Gate B condition: "predicate confidence >= 0.85 AND at least one QA term directly present in input"
- Input: "Sign off on the service configuration."
- Predicate fired: ["QA", "IMPLEMENT"]
- Gate B evaluation: QA confidence = 0.72 (below threshold)

Correct result:
- Gate B fails (0.72 < 0.85)
- Flow falls back to deterministic_generic
- No probationary flow executed

v6-02 corrupted result:
- Gate B failure detected but orchestrator proceeds anyway
- Probationary flow selected and executed
- Result: ReleaseGateFlow71 runs on low-confidence input (WRONG)
- TransitionType incorrectly recorded as feature_addition from unverified flow

Why this is wrong:
Gate B is a hard gate per Rule 4 (zero failures). A probationary flow that fails Gate B must fall back to deterministic_generic. The probationary_rejected path exists precisely for this scenario. v6-02 bypasses the gate check, allowing unverified flows to execute.

---

## B38-0A-011-003 — v6-03 corruption: QA+IMPLEMENT routing collapsed to single 7.1 decision

Corruption type: v6-03
Description: Corrupted router collapses a multi-predicate result to a single routing decision, losing the IMPLEMENT routing to Phase 4.

Input:
"Review the order service implementation and approve the release."

Correct result:
- Predicates fired: ["QA", "IMPLEMENT"]
- Routing: 7.1 (QA) + 4.1 (IMPLEMENT)
- DecisionCount: 2
- TransitionType: feature_addition

v6-03 corrupted result:
- Router collapses ["QA", "IMPLEMENT"] to ["QA"] only
- Routing: 7.1 only
- DecisionCount: 1 (WRONG — should be 2)
- TransitionType: no_contribution (WRONG — should be feature_addition)
- IMPLEMENT routing to 4.1 lost

Why this is wrong:
Rule 15 states: two predicates → two independent routing decisions. DecisionCount must equal the number of predicates fired. Collapsing multi-predicate results is the v6-03 error pattern. Each predicate routes independently; no predicate absorbs another. QA routing to 7.1 does not eliminate IMPLEMENT routing to 4.1.

---

## B38-0A-011-004 — Summary: three v6 corruption patterns side by side

v6-01 (QA Suppression):
- Trigger condition: gap vocabulary present (checklist, rollout, on-call, etc.)
- Corrupted behavior: QA predicate suppressed as if gap context invalidates review actions
- Correct behavior: gap vocabulary → CapabilityGapWarning only; does not suppress fired predicates

v6-02 (Probationary Gate Bypass):
- Trigger condition: probationary flow candidate available; Gate B score below threshold
- Corrupted behavior: flow executes despite Gate B failure
- Correct behavior: Gate B failure → fallback to deterministic_generic; probationary_rejected state

v6-03 (Multi-Predicate Collapse):
- Trigger condition: more than one predicate fires
- Corrupted behavior: router collapses to single highest-confidence predicate
- Correct behavior: each predicate routes independently; DecisionCount = predicates fired

All three v6 corruptions are observable from the outside:
- v6-01: DecisionCount = 0 when it should be ≥ 1
- v6-02: probationary flow in output without Gate B pass record
- v6-03: DecisionCount < expected based on input vocabulary count
