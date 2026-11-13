# Layer 0-B — Router Pairs (030)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-030-001 — Complete routing table for versioning scenarios

This file is a complete routing reference for all versioning scenarios introduced in the 025-036 extension.

### All versioning scenario routing outcomes
| Scenario | PredicateSet | Phases | DecisionCount | TransitionType |
|---|---|---|---|---|
| SemanticVersioningApproval | ["QA"] | 7.1 | 1 | no_contribution |
| SemanticVersioningBuildApproval | ["QA","IMPLEMENT"] | 4.1, 7.1 | 2 | feature_addition |
| CalVerPlanningKnowledgeEdge | [] | none | 0 | N/A |
| SemVerBumpDocumentation | ["DOCUMENT"] | 2.1 | 1 | no_contribution |
| PreReleaseTagVerification | ["QA"] | 7.1 | 1 | no_contribution |
| PreReleaseTagPipelineBuild | ["QA","IMPLEMENT"] | 4.1, 7.1 | 2 | feature_addition |
| ReleaseCandidateSignOff | ["QA"] | 7.1 | 1 | no_contribution |
| ReleaseCandidateDocumentedApproval | ["QA","DOCUMENT"] | 2.1, 7.1 | 2 | no_contribution |
| NightlyBuildPipelineValidation | ["QA","IMPLEMENT"] | 4.1, 7.1 | 2 | feature_addition |
| WeeklyReleaseProcessDocumentation | ["DOCUMENT"] | 2.1 | 1 | no_contribution |
| MonthlyReleaseGateWithDeploymentGap | ["QA","DOCUMENT"] | 2.1, 7.1 | 2 | no_contribution |
| QuarterlyReleaseFourPredicate | ["QA","IMPLEMENT","DOCUMENT","SECURITY"] | 4.1, 2.1, 3.4, 7.1 | 4 | feature_addition |
| HotfixTagEmergencyApproval | ["QA"] | 7.1 | 1 | no_contribution |
| PatchReleaseNotesSignOff | ["QA","DOCUMENT"] | 2.1, 7.1 | 2 | no_contribution |
| PatchReleaseSecurityAudit | ["QA","SECURITY"] | 3.4, 7.1 | 2 | no_contribution |

---

## B38-0B-030-002 — QA ordering invariant across all versioning scenarios

QA ordering rule (rule 12 — absolute) holds for all versioning scenarios:
- Single predicate ["QA"]: QA is at position 1 (trivially last)
- Two predicates: QA is at position 2 (last)
- Three predicates: QA is at position 3 (last)
- Four predicates: QA is at position 4 (last)
- Zero predicates: knowledge_edge (QA does not participate)

---

## B38-0B-030-003 — Gap warnings never generate routing decisions

For versioning scenarios, the following gap warnings may appear:
- "deployment" (rule 4 — operational deployment)
- "rollout" (B38 gap — rule 5)
- "cutover" (B38 gap — rule 5)
- "go-live" (B38 gap — rule 5)

In ALL cases:
- Gap warnings are attached to routing result as metadata
- Gap warnings do NOT generate routing decisions
- Gap warnings do NOT increase DecisionCount
- Gap warnings do NOT change TransitionType

Example: ["QA"] with CapabilityGapWarnings: ["rollout","go-live"] → DecisionCount: 1 (not 3)
