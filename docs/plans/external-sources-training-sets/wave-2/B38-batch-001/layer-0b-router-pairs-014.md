# Layer 0-B — Router Pairs (014)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-014-001 — [QA] alone: release train approval

PredicateSet: ["QA"]
Scenario: ReleaseTrainApproval
Input: "Approve the Q2 release train and confirm all feature teams have signed off."

Routing decision:
1. QA → Phase 7.1 (ReleaseGateFlow71) — only predicate; ordering trivially satisfied

DecisionCount: 1
TransitionType: no_contribution
OutputFiles: [release_train_approval.md, sign_off_register.md]
DomainId: software-development

Note: "release train" generates no routing decision. It is a delivery-planning concept with no predicate mapping. No gap warning (it is not listed B38 gap vocabulary; it simply has no predicate).

---

## B38-0B-014-002 — [QA, IMPLEMENT] for release train with service

PredicateSet: ["QA", "IMPLEMENT"]
Scenario: ReleaseTrainServiceApproval
Input: "Review all service deployments in the release train and approve each one."

Routing decisions:
1. IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
2. QA → Phase 7.1 (ReleaseGateFlow71) — QA last

DecisionCount: 2
TransitionType: feature_addition
CapabilityGapWarnings: ["deployments"] — warning only, no routing decision for "deployments"

---

## B38-0B-014-003 — [QA, DOCUMENT] for release train criteria

PredicateSet: ["QA", "DOCUMENT"]
Scenario: ReleaseTrainCriteriaValidation

Routing decisions:
1. DOCUMENT → Phase 2.1 (DocumentationFlow)
2. QA → Phase 7.1 (ReleaseGateFlow71) — QA last

DecisionCount: 2
TransitionType: no_contribution

---

## B38-0B-014-004 — Knowledge_edge: release train launch only

PredicateSet: []
Scenario: ReleaseTrainLaunch
Input: "Launch the Q2 release train on the scheduled go-live date."

Routing: knowledge_edge (no routing to any flow)
CapabilityGapWarnings: ["launch", "go-live"]
DecisionCount: 1 (knowledge_edge sentinel)
TransitionType: N/A

This is NOT GapHandler. This is NOT SCHEDULE_CAPABILITY_LEARNING.
Result is purely knowledge_edge because PredicateSet is empty.
