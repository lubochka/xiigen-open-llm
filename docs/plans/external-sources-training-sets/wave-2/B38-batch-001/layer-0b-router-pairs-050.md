# Layer 0-B — Router Pairs (050)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-050-001 — SBOMGeneration routing: DOCUMENT alone

Input: "Document the SBOM (Software Bill of Materials) generation process for the release."
- PredicateSet: ["DOCUMENT"]
- Routing: DOCUMENT → Phase 2.1 (DocumentationFlow)
- DecisionCount: 1
- TransitionType: no_contribution

---

## B38-0B-050-002 — SBOMGeneration routing: knowledge_edge

Input: "Generate the SBOM for the release artifact."
- PredicateSet: []
- Routing: no flows created
- ResultState: knowledge_edge

---

## B38-0B-050-003 — SBOMGeneration routing: QA alone

Input: "Review the SBOM for completeness and approve the release."
- PredicateSet: ["QA"]
- Routing: QA → Phase 7.1 (ReleaseGateFlow71)
- DecisionCount: 1
- TransitionType: no_contribution

---

## B38-0B-050-004 — SBOMGeneration routing: QA+IMPLEMENT+DOCUMENT

Input: "Build the SBOM generation service, document the SBOM criteria, and validate the output."
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT"]
- Routing:
  1. IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
  2. DOCUMENT → Phase 2.1 (DocumentationFlow)
  3. QA → Phase 7.1 (ReleaseGateFlow71) — last
- DecisionCount: 3
- TransitionType: feature_addition

---

## B38-0B-050-005 — SBOMGeneration routing: QA+IMPLEMENT+DOCUMENT+SECURITY

Input: "Review the SBOM generation service, audit security of all components, document the SBOM criteria, and approve the release."
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT","SECURITY"]
- Routing:
  1. IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
  2. DOCUMENT → Phase 2.1 (DocumentationFlow)
  3. SECURITY → Phase 3.4 (SecurityAuditFlow)
  4. QA → Phase 7.1 (ReleaseGateFlow71) — last
- DecisionCount: 4
- TransitionType: feature_addition
