# Layer 0-B — Router Pairs (049)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-049-001 — ArtifactProvenanceTracking routing: DOCUMENT alone

Input: "Document the artifact provenance tracking policy for the release pipeline."
- PredicateSet: ["DOCUMENT"]
- Routing: DOCUMENT → Phase 2.1 (DocumentationFlow)
- DecisionCount: 1
- TransitionType: no_contribution

---

## B38-0B-049-002 — ArtifactProvenanceTracking routing: IMPLEMENT alone

Input: "Track artifact provenance throughout the build pipeline."
- PredicateSet: ["IMPLEMENT"]
- Routing: IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
- DecisionCount: 1
- TransitionType: feature_addition

---

## B38-0B-049-003 — ArtifactProvenanceTracking routing: QA alone

Input: "Review the artifact provenance records and approve the release."
- PredicateSet: ["QA"]
- Routing: QA → Phase 7.1 (ReleaseGateFlow71)
- DecisionCount: 1
- TransitionType: no_contribution

---

## B38-0B-049-004 — ArtifactProvenanceTracking routing: QA+SECURITY

Input: "Audit the security of the artifact provenance tracking system and approve the release."
- PredicateSet: ["QA","SECURITY"]
- Routing:
  1. SECURITY → Phase 3.4 (SecurityAuditFlow)
  2. QA → Phase 7.1 (ReleaseGateFlow71) — last
- DecisionCount: 2
- TransitionType: no_contribution

---

## B38-0B-049-005 — ArtifactProvenanceTracking routing: QA+IMPLEMENT+DOCUMENT+SECURITY

Input: "Review the provenance tracking service, audit the security controls, document the provenance criteria, and approve the release."
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT","SECURITY"]
- Routing:
  1. IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
  2. DOCUMENT → Phase 2.1 (DocumentationFlow)
  3. SECURITY → Phase 3.4 (SecurityAuditFlow)
  4. QA → Phase 7.1 (ReleaseGateFlow71) — last
- DecisionCount: 4
- TransitionType: feature_addition
