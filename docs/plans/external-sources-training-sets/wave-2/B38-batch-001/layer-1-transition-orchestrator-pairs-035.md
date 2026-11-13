# Layer 1 — Transition Orchestrator Pairs (035)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B38-L1-035-001 — Complete orchestration reference for B38 sub-phase 7.1 (025-060 extension)

N01-N09 node behavior is unchanged from the original batch for all 025-060 scenarios.

N01 DomainSplitter: receives SelectedLogicalTransition; extracts predicates and gap warnings
N02 DomainTranslator: translates DomainPackets to TranslatedDomainStatements per predicate
N03 SchemePlanner: creates WorkBranches — one per predicate; gap warnings = CapabilityGapWarning only
N04 DomainProcessor: processes each WorkBranch
N05 Applier: applies results
N06 ResultTranslator: produces TranslatedResultFragments
N07 ArbiterSelector: selects arbiters
N08 Arbiter: renders verdict
N09 Formatter: produces HumanResult with TransitionType, FileCount, ResultState, DecisionCount

---

## B38-L1-035-002 — TransitionType rules re-confirmed for 025-060

IMPLEMENT triggers TransitionType = feature_addition in all 025-060 scenarios.
All scenarios without IMPLEMENT produce TransitionType = no_contribution or knowledge_edge.

New pattern confirmed in 027: ["IMPLEMENT","DOCUMENT"] without QA → TransitionType = feature_addition.
IMPLEMENT is the sole determinant of feature_addition — QA presence/absence does not affect it.

---

## B38-L1-035-003 — Anti-corruption for orchestration in 025-060

v6-01 (QA suppression): QA MUST fire from review/approve/verify/validate/sign-off in all 025-060 scenarios. Provenance, SBOM, regulatory, gated vocabulary co-present = CapabilityGapWarnings only, never QA suppression.

v6-03 (predicate collapse): DecisionCount MUST equal len(PredicateSet) for all 025-060 scenarios. ["QA","IMPLEMENT","DOCUMENT","SECURITY"] → DecisionCount: 4 always.

New v6-04 (no-QA flow completeness): When QA is absent, all non-QA flows MUST still execute. ["DOCUMENT","SECURITY"] must produce two flows (2.1 + 3.4) — not one, not zero.

---

## B38-L1-035-004 — Gap warnings in orchestration

Gap warnings (go-live, rollout, checklist, distribute, generate, track, archive, configure, submit) are attached to the orchestration record as CapabilityGapWarnings.
They do NOT create flows.
They do NOT reduce the PredicateSet.
They do NOT affect TransitionType.
They appear in the N09 HumanResult as metadata.
