# Held-Out and Corruption Pairs (018)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## B38-HO-018-001 — HELD-OUT: production handoff sign-off

Input: "Collect sign-off from the operations team for the production handoff."

Correct result:
- "sign-off" → QA (sign-off fires QA)
- "operations team" → context
- "production handoff" → no predicate ("handoff" is operational transfer, not code authoring)
- PredicateSet: ["QA"]
- TransitionType: no_contribution
- DecisionCount: 1

Gate B assertion: PredicateSet == ["QA"] AND "handoff" NOT in PredicateSet triggers

---

## B38-HO-018-002 — CORRUPTION: "handoff" wrongly fires QA

Corrupt input processing:
- "handoff" → QA (WRONG — "handoff" is not "sign-off"; it does not fire QA)

Why this is wrong:
"handoff" is an operational transfer noun. It has no approval semantics.
"sign-off" is an approval action noun. It explicitly fires QA.
Confusing these two terms is a predicate map error.

Correct: QA fires only from "sign-off". "handoff" produces no predicate.

---

## B38-HO-018-003 — HELD-OUT: handoff runbook is knowledge_edge

Input: "Execute the handoff runbook and monitor the production environment after cutover."

Correct result:
- PredicateSet: []
- CapabilityGapWarnings: ["execute","runbook","monitor","cutover"]
- ResultState: knowledge_edge

Gate B assertion: PredicateSet == [] AND len(CapabilityGapWarnings) == 4
