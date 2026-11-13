# Layer 0-B — Router Pairs (034)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-034-001 — Complete routing reference for build cadence scenarios

This file is a complete routing reference for all build cadence scenarios (nightly, weekly, monthly, quarterly) in the 025-036 extension.

### All build cadence routing outcomes
| Scenario | PredicateSet | Phases routed | DecisionCount | TransitionType |
|---|---|---|---|---|
| NightlyBuildPipelineValidation | ["QA","IMPLEMENT"] | 4.1, 7.1 | 2 | feature_addition |
| NightlyBuildScheduleKnowledgeEdge | [] | none | 0 | N/A |
| NightlyBuildCriteriaDocumentation | ["DOCUMENT"] | 2.1 | 1 | no_contribution |
| WeeklyReleaseBuildApproval | ["QA","IMPLEMENT"] | 4.1, 7.1 | 2 | feature_addition |
| WeeklyReleaseProcessDocumentation | ["DOCUMENT"] | 2.1 | 1 | no_contribution |
| WeeklyReleaseServiceApprovalWithRolloutGap | ["QA","IMPLEMENT"] | 4.1, 7.1 | 2 | feature_addition |
| WeeklyReleaseServiceAndCriteria | ["QA","IMPLEMENT","DOCUMENT"] | 4.1, 2.1, 7.1 | 3 | feature_addition |
| MonthlyReleaseGateWithDeploymentGap | ["QA","DOCUMENT"] | 2.1, 7.1 | 2 | no_contribution |
| MonthlyReleaseServiceApproval | ["QA","IMPLEMENT"] | 4.1, 7.1 | 2 | feature_addition |
| MonthlyReleaseArchitectureReview | ["QA","ARCHITECTURE"] | 5.x, 7.1 | 2 | no_contribution |
| QuarterlyReleaseFourPredicate | ["QA","IMPLEMENT","DOCUMENT","SECURITY"] | 4.1, 2.1, 3.4, 7.1 | 4 | feature_addition |
| QuarterlyReleasePlanningKnowledgeEdge | [] | none | 0 | N/A |
| QuarterlyReleaseSignOffCollection | ["QA"] | 7.1 | 1 | no_contribution |

---

## B38-0B-034-002 — Gap warnings in build cadence routing

Gap vocabulary observed in build cadence scenarios:
- "rollout" (gap-project-delivery-rollout-golive-001): appears in weekly release sentences
- "deployment" (rule 4): appears in monthly release sentences
- "cutover" (gap-project-delivery-cutover-001): appears in quarterly release sentences
- "go-live" (gap-project-delivery-go-live-001): may appear in monthly/quarterly sentences

All gap warnings: metadata only, never routing decisions, never affect DecisionCount.

---

## B38-0B-034-003 — DecisionCount invariant for cadence scenarios

| PredicateSet | Expected DecisionCount | Common in cadence scenario |
|---|---|---|
| [] | 0 (knowledge_edge) | Schedule-only inputs |
| ["QA"] | 1 | Sign-off collection only |
| ["DOCUMENT"] | 1 | Criteria documentation only |
| ["SECURITY"] | 1 | Security audit only |
| ["QA","IMPLEMENT"] | 2 | Build + approve |
| ["QA","DOCUMENT"] | 2 | Document + approve |
| ["QA","SECURITY"] | 2 | Audit + approve |
| ["QA","ARCHITECTURE"] | 2 | Architecture review + approve |
| ["QA","IMPLEMENT","DOCUMENT"] | 3 | Build + document criteria + approve |
| ["QA","IMPLEMENT","SECURITY"] | 3 | Build + security audit + approve |
| ["QA","IMPLEMENT","DOCUMENT","SECURITY"] | 4 | Full cadence gate |
