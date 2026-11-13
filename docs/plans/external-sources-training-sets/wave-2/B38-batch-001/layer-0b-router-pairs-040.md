# Layer 0-B — Router Pairs (040)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-040-001 — UpgradeGuide routing: DOCUMENT alone

Input: "Document the upgrade guide for the platform v3 release."
- PredicateSet: ["DOCUMENT"]
- Routing: DOCUMENT → Phase 2.1 (DocumentationFlow)
- DecisionCount: 1
- TransitionType: no_contribution

---

## B38-0B-040-002 — UpgradeGuide routing: knowledge_edge

Input: "Distribute the upgrade guide to all customer teams."
- PredicateSet: []
- CapabilityGapWarnings: ["distribute"]
- Routing: no flows created
- ResultState: knowledge_edge

---

## B38-0B-040-003 — UpgradeGuide routing: QA alone

Input: "Review the upgrade guide for completeness and approve it for distribution."
- PredicateSet: ["QA"]
- CapabilityGapWarnings: ["distribution"]
- Routing: QA → Phase 7.1 (ReleaseGateFlow71)
- DecisionCount: 1
- TransitionType: no_contribution

---

## B38-0B-040-004 — UpgradeGuide routing: QA+DOCUMENT

Input: "Document the upgrade readiness criteria and sign off the upgrade guide."
- PredicateSet: ["QA","DOCUMENT"]
- Routing:
  1. DOCUMENT → Phase 2.1 (DocumentationFlow)
  2. QA → Phase 7.1 (ReleaseGateFlow71) — last
- DecisionCount: 2
- TransitionType: no_contribution

---

## B38-0B-040-005 — UpgradeGuide routing: QA+IMPLEMENT+DOCUMENT

Input: "Build the upgrade service, document the upgrade acceptance criteria, and validate the process."
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT"]
- Routing:
  1. IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
  2. DOCUMENT → Phase 2.1 (DocumentationFlow)
  3. QA → Phase 7.1 (ReleaseGateFlow71) — last
- DecisionCount: 3
- TransitionType: feature_addition
