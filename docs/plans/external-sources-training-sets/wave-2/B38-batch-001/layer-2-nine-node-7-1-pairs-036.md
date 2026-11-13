# Layer 2 — Nine-Node 7.1 Pairs (036)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: nine_node_flow

---

## B38-L2-036-001 — Complete nine-node rule summary for B38 sub-phase 7.1 (extended to 060)

This file summarizes all nine-node rules for B38 sub-phase 7.1 extended through file 060.

### Node responsibilities (unchanged)
| Node | Unit | A input | B output |
|---|---|---|---|
| N01 | DomainSplitterUnit | SelectedLogicalTransition | DomainPacket[] |
| N02 | DomainTranslatorUnit | DomainPacket | TranslatedDomainStatement |
| N03 | SchemePlannerUnit | TranslatedDomainStatement[] | WorkBranch[] |
| N04 | DomainProcessorUnit | WorkBranch | ProcessedBranchResult |
| N05 | ApplierUnit | ProcessedBranchResult | AppliedBranchResult |
| N06 | ResultTranslatorUnit | AppliedBranchResult[] | TranslatedResultFragment[] |
| N07 | ArbiterSelectorUnit | TranslatedResultFragment[] | ArbiterReviewRequest[] |
| N08 | ArbiterUnit | ArbiterReviewRequest | ArbiterVerdict |
| N09 | FormatterUnit | ReviewedResultSet | HumanResult |

---

## B38-L2-036-002 — New N09 result patterns confirmed in 025-060

| Pattern | Example | Key rule |
|---|---|---|
| knowledge_edge | CalendarVersioningPlanOnly | Plan/generate/track = gap verbs |
| IMPLEMENT alone | SBOM build pipeline | build always fires |
| DOCUMENT alone | WeeklyReleaseProcessDoc | document + criteria |
| SECURITY alone | CVESecurityAlone | CVE fires SECURITY |
| IMPLEMENT+DOCUMENT (no QA) | MigrationServiceDocumentation | valid two-flow, feature_addition |
| DOCUMENT+SECURITY (no QA) | SecurityAdvisoryDocumentation | valid two-flow, no_contribution |
| QA+SECURITY | ArtifactSigningSecuritySignOff | audit rule 19 |
| QA+DOCUMENT+SECURITY | CVEDocumentationApproval | three-flow, no_contribution |
| QA+IMPLEMENT+SECURITY | ChangelogServiceSecurityApproval | three-flow, feature_addition |
| QA+IMPLEMENT+DOCUMENT | SBOMServiceValidation | three-flow, feature_addition |
| QA+IMPLEMENT+DOCUMENT+SECURITY | All full gate scenarios | four-flow, feature_addition |

---

## B38-L2-036-003 — Three anti-corruption rules for nine-node execution (final)

v6-01 (QA suppression): QA MUST fire from review/approve/verify/validate/sign-off regardless of all 025-060 scenario nouns. These nouns are context — never suppressors.

v6-03 (predicate collapse): DecisionCount MUST equal len(PredicateSet) for all scenarios. Collapse is always wrong.

v6-04 (no-QA flow completeness): Non-QA predicate sets are valid factory outputs. ["DOCUMENT","SECURITY"], ["IMPLEMENT","DOCUMENT"], ["SECURITY"] produce their full FlowCount — never collapsed to knowledge_edge.
