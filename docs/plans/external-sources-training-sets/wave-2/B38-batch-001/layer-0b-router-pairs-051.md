# Layer 0-B — Router Pairs (051)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-051-001 — SignedArtifact routing: DOCUMENT alone

Input: "Document the signed artifact policy for the release pipeline."
- PredicateSet: ["DOCUMENT"]
- Routing: DOCUMENT → Phase 2.1 (DocumentationFlow)
- DecisionCount: 1
- TransitionType: no_contribution

---

## B38-0B-051-002 — SignedArtifact routing: knowledge_edge

Input: "Sign the release artifact with the code signing certificate."
- PredicateSet: []
- Routing: no flows created
- ResultState: knowledge_edge

---

## B38-0B-051-003 — SignedArtifact routing: QA alone

Input: "Verify the artifact signature and approve the release."
- PredicateSet: ["QA"]
- Routing: QA → Phase 7.1 (ReleaseGateFlow71)
- DecisionCount: 1
- TransitionType: no_contribution

---

## B38-0B-051-004 — SignedArtifact routing: QA+SECURITY

Input: "Audit the security of the artifact signing process and sign off the release."
- PredicateSet: ["QA","SECURITY"]
- Routing:
  1. SECURITY → Phase 3.4 (SecurityAuditFlow)
  2. QA → Phase 7.1 (ReleaseGateFlow71) — last
- DecisionCount: 2
- TransitionType: no_contribution

---

## B38-0B-051-005 — SignedArtifact routing: QA+IMPLEMENT+DOCUMENT+SECURITY

Input: "Review the signing service, audit the security of the signing keys, document the signing criteria, and approve the release."
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT","SECURITY"]
- Routing:
  1. IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
  2. DOCUMENT → Phase 2.1 (DocumentationFlow)
  3. SECURITY → Phase 3.4 (SecurityAuditFlow)
  4. QA → Phase 7.1 (ReleaseGateFlow71) — last
- DecisionCount: 4
- TransitionType: feature_addition
