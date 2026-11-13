# Layer 0-B — Router Pairs (022)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-022-001 — [QA] alone: staged rollout gate approval

PredicateSet: ["QA"]
Scenario: StagedRolloutGate
Input: "Approve the staged rollout gate after validating each environment is healthy."

Routing decision:
1. QA → Phase 7.1 (ReleaseGateFlow71)

DecisionCount: 1
TransitionType: no_contribution
CapabilityGapWarnings: ["rollout"] — warning only, no routing decision
OutputFiles: [staged_rollout_gate_report.md, environment_health_approval.md]

---

## B38-0B-022-002 — [QA, IMPLEMENT]: staged rollout with service

PredicateSet: ["QA", "IMPLEMENT"]
Scenario: StagedRolloutServiceGate
Input: "Verify the recommendation service is stable in each staged rollout phase and sign off."

Routing decisions:
1. IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
2. QA → Phase 7.1 (ReleaseGateFlow71) — QA last

DecisionCount: 2
TransitionType: feature_addition
CapabilityGapWarnings: ["rollout"]

---

## B38-0B-022-003 — [QA, DOCUMENT]: staged rollout success criteria

PredicateSet: ["QA", "DOCUMENT"]
Scenario: StagedRolloutCriteriaGate
Input: "Validate the staged rollout success criteria before approving the next environment tier."

Routing decisions:
1. DOCUMENT → Phase 2.1 (DocumentationFlow)
2. QA → Phase 7.1 (ReleaseGateFlow71) — QA last

DecisionCount: 2
TransitionType: no_contribution
CapabilityGapWarnings: ["rollout"]

---

## B38-0B-022-004 — [QA, ARCHITECTURE]: staged rollout architecture review

PredicateSet: ["QA", "ARCHITECTURE"]
Scenario: StagedRolloutArchitectureGate
Input: "Review the staged rollout architecture design and approve the phased deployment approach."

Routing decisions:
1. ARCHITECTURE → Phase 5.x (ArchitectureFlow)
2. QA → Phase 7.1 (ReleaseGateFlow71) — QA last

DecisionCount: 2
TransitionType: no_contribution (ARCHITECTURE does not produce feature_addition; only IMPLEMENT does)
CapabilityGapWarnings: ["rollout", "deployment"]

Note: ARCHITECTURE routing is always no_contribution. TransitionType = feature_addition requires IMPLEMENT in the predicate set.

---

## B38-0B-022-005 — Knowledge_edge: staged rollout launch gap

PredicateSet: []
Scenario: StagedRolloutLaunchGap
Input: "Launch the staged rollout on Friday and monitor each environment tier for issues."

Routing: knowledge_edge
CapabilityGapWarnings: ["launch", "rollout", "monitor"]
DecisionCount: 1 (knowledge_edge sentinel)
TransitionType: N/A
