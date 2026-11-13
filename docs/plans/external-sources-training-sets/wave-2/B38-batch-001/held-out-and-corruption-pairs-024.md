# Held-Out and Corruption Pairs (024)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## B38-HO-024-001 — HELD-OUT: five-predicate scenario

Input: "Review the payment service release criteria, audit the security controls, document the handoff, and approve the go-live deployment."

Correct result:
- "Review" → QA
- "payment service" → IMPLEMENT (rule 2)
- "release criteria" → DOCUMENT (criteria — rule 3)
- "audit" → QA (consolidates) + SECURITY co-fires (rule 19: security+audit)
- "security controls" → SECURITY (security fires)
- "document" → DOCUMENT (consolidates)
- "handoff" → no predicate
- "approve" → QA (consolidates)
- "go-live" → GAP
- "deployment" → GAP (rule 4)
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT","SECURITY"]
- CapabilityGapWarnings: ["go-live","deployment"]
- TransitionType: feature_addition
- DecisionCount: 4

Gate B assertion: PredicateSet == ["QA","IMPLEMENT","DOCUMENT","SECURITY"] AND DecisionCount == 4 AND TransitionType == "feature_addition"

---

## B38-HO-024-002 — CORRUPTION (v6-03): four predicates collapsed to one decision

Corrupt routing:
- DecisionCount: 1 (WRONG — only 7.1 returned for ["QA","IMPLEMENT","DOCUMENT","SECURITY"])

Why this is wrong:
Four predicates must produce four routing decisions.
v6-03 corruption collapses multi-predicate results to a single decision.
This loses three routing decisions: 4.1 (IMPLEMENT), 2.1 (DOCUMENT), 3.4 (SECURITY).

Correct: DecisionCount: 4 — all four phases return routing decisions.
IMPLEMENT → 4.1; DOCUMENT → 2.1; SECURITY → 3.4; QA → 7.1 (last)

---

## B38-HO-024-003 — HELD-OUT: plan the go-live checklist is knowledge_edge

Input: "Plan the go-live checklist for the production release."

Correct result:
- "Plan" → not in any predicate lexicon
- "go-live" → GAP
- "checklist" → GAP
- "production release" → context
- PredicateSet: []
- CapabilityGapWarnings: ["go-live","checklist"]
- ResultState: knowledge_edge

Gate B assertion: PredicateSet == [] AND "go-live" in CapabilityGapWarnings AND "checklist" in CapabilityGapWarnings

Note: This is the canonical "plan the go-live checklist → knowledge_edge" held-out case referenced in cross-layer synthesis 006.
