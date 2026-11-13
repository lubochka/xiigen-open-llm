# Layer 0-B — Router Pairs (053)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-053-001 — ProvenanceVerification routing: QA alone

Input: "Verify the provenance of all release artifacts before approval."
- PredicateSet: ["QA"]
- Routing: QA → Phase 7.1 (ReleaseGateFlow71)
- DecisionCount: 1
- TransitionType: no_contribution

---

## B38-0B-053-002 — ProvenanceVerification routing: knowledge_edge

Input: "Check the provenance chain for the release bundle."
- PredicateSet: []
- Routing: no flows created
- ResultState: knowledge_edge

---

## B38-0B-053-003 — ProvenanceVerification routing: DOCUMENT alone

Input: "Document the provenance verification policy and criteria for all release artifacts."
- PredicateSet: ["DOCUMENT"]
- Routing: DOCUMENT → Phase 2.1 (DocumentationFlow)
- DecisionCount: 1
- TransitionType: no_contribution

---

## B38-0B-053-004 — ProvenanceVerification routing: QA+SECURITY

Input: "Audit the provenance security chain and sign off the release."
- PredicateSet: ["QA","SECURITY"]
- Routing:
  1. SECURITY → Phase 3.4 (SecurityAuditFlow)
  2. QA → Phase 7.1 (ReleaseGateFlow71) — last
- DecisionCount: 2
- TransitionType: no_contribution

---

## B38-0B-053-005 — ProvenanceVerification routing: QA+IMPLEMENT+DOCUMENT

Input: "Build the provenance verification service, document the verification criteria, and approve the release."
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT"]
- Routing:
  1. IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
  2. DOCUMENT → Phase 2.1 (DocumentationFlow)
  3. QA → Phase 7.1 (ReleaseGateFlow71) — last
- DecisionCount: 3
- TransitionType: feature_addition
