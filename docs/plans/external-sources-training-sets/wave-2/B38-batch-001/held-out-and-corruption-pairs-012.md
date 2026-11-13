# Held-Out and Corruption Pairs (012)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## B38-HO-012-001 — HELD-OUT (mixed): QA+SECURITY boundary test

Input: "Verify the security audit results and sign off on the compliance check."

Correct result:
- "Verify" → QA
- "security" → SECURITY
- "audit" → QA (release context) + SECURITY (with "security" present)
- "sign off" → QA
- "compliance" → QA
- "check" → QA (in quality-gate context)
- PredicateSet: ["QA", "SECURITY"]
- CapabilityGapWarnings: []
- TransitionType: no_contribution
- Phase routing: 3.4 + 7.1

Gate B: PredicateSet == ["QA","SECURITY"]

---

## B38-HO-012-002 — HELD-OUT (mixed): criteria + service boundary test

Input: "Review the API service implementation criteria and approve the release."

Correct result:
- "Review" → QA
- "API service" → IMPLEMENT (service fires IMPLEMENT)
- "implementation" → supports IMPLEMENT
- "criteria" → DOCUMENT
- "approve" → QA
- PredicateSet: ["QA", "IMPLEMENT", "DOCUMENT"]
- CapabilityGapWarnings: []
- TransitionType: feature_addition
- Phase routing: 4.1 + 2.1 + 7.1
- DecisionCount: 3

Gate B: DecisionCount == 3 AND TransitionType == "feature_addition"

---

## B38-HO-012-003 — HELD-OUT (mixed): multiple gap terms with QA

Input: "Approve the release before the go-live cutover and rollout."

Correct result:
- "Approve" → QA
- "go-live" → GAP
- "cutover" → GAP
- "rollout" → GAP
- PredicateSet: ["QA"]
- CapabilityGapWarnings: ["go-live", "cutover", "rollout"]
- TransitionType: no_contribution
- Phase routing: 7.1

Gate B: PredicateSet == ["QA"] AND len(CapabilityGapWarnings) == 3

Note: Three gap terms do not change the predicate set. They all produce gap warnings. One QA predicate → one routing decision. The gap count is irrelevant to routing.

---

## B38-HO-012-004 — HELD-OUT (mixed): zero predicates with multiple gap types

Input: "Initiate the system cutover, follow the go-live runbook, and monitor the rollout."

Correct result:
- "Initiate" → not a predicate
- "cutover" → GAP
- "go-live" → GAP
- "runbook" → GAP
- "monitor" → GAP (rule 7)
- "rollout" → GAP
- PredicateSet: []
- CapabilityGapWarnings: ["cutover", "go-live", "runbook", "monitor", "rollout"]
- Gap IDs: gap-project-delivery-cutover-001, gap-project-delivery-go-live-001, gap-project-delivery-runbook-001, gap-project-delivery-rollout-golive-001
- ResultState: knowledge_edge
- DecisionCount: 0

Gate B: PredicateSet == [] AND ResultState == "knowledge_edge" AND len(CapabilityGapWarnings) >= 4
