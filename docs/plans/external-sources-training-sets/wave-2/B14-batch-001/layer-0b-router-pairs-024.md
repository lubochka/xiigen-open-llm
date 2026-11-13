# Layer 0-B — Router Pairs (024)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: router

---

## B14-router-024-001 — Complete routing rule: ARCHITECTURE → 3.5 disambiguation

Sub-phase 3.5 (Infrastructure and DevOps Architecture) is selected when:
  - ARCHITECTURE predicate fires AND the compiled text contains infra vocabulary: infrastructure, topology, cluster, service mesh, network, CDN, VPC, deployment layer, node group, observability, pipeline (infra context), container, GitOps, Kubernetes, EKS
  - Sub-phase 3.1 (System Architecture) is selected instead when compiled text contains purely app-level vocabulary: domain model, service boundary, bounded context, aggregate, module (app-level), CQRS, SAGA

---

## B14-router-024-002 — Complete routing rule: SECURITY → 6.3

SECURITY → SubPhase 6.3 (Security and Compliance)
  Fires when: secret, certificate, tls, ssl, encrypt, credential, key rotation, vault (tool), zero-trust, mTLS appear in compiled text
  Always concurrent with ARCHITECTURE at 3.5 when both predicates fire
  Does not change PrimarySubPhaseId — ARCHITECTURE remains primary

---

## B14-router-024-003 — Complete routing rule: multi-predicate flow ordering

When multiple predicates fire:
  Each predicate creates an independent flow instance
  TransitionType: feature_addition if IMPLEMENT is present; documentation if only ARCHITECTURE+DOCUMENT; unknown if zero predicates
  PrimarySubPhaseId: determined by first or dominant predicate (ARCHITECTURE takes primacy over DOCUMENT; IMPLEMENT takes primacy in feature_addition contexts)
  Sequential ordering in Layer 1: ARCHITECTURE → SECURITY → IMPLEMENT → TEST → DOCUMENT → QA

---

## B14-router-024-004 — Complete routing rule: knowledge_edge routing

When compiled_predicate_ids: [] (zero predicates):
  TrustState: knowledge_edge_synthetic
  RoutingDecisions: []
  TransitionType: unknown
  SubPhaseId: null
  CapabilityGapSignalForwarded: true
  Gap vocabulary always emits CapabilityGapWarnings but never suppresses predicates that fired on other vocabulary in the same text
