# Layer 0-B — Router Pairs (029)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: router

---

## B14-router-029-001 — IstioMesh routing: ARCHITECTURE → 3.5

ARCHITECTURE → 3.5 for Istio service mesh:
  "service mesh" compound phrase = 3.5 infra vocabulary (listed in Router 024-001 infra vocabulary set)
  "Istio topology" → 3.5 | "mesh architecture" → 3.5 | "sidecar layer" → 3.5
  Never 3.1: service mesh is always infrastructure-domain vocabulary

---

## B14-router-029-002 — IstioMesh routing: SECURITY → 6.3

SECURITY → 6.3 for mTLS and Istio security:
  "mTLS" → 6.3 (SECURITY) | "peer authentication" fires SECURITY → 6.3
  "zero-trust" → 6.3 | "STRICT mTLS" → 6.3
  Always concurrent with ARCHITECTURE (3.5) when both predicates fire

---

## B14-router-029-003 — IstioMesh routing: IMPLEMENT → 4.1

IMPLEMENT → 4.1 for VirtualService / DestinationRule manifest creation:
  IaC manifests for Istio (VirtualService.yaml, DestinationRule.yaml, AuthorizationPolicy.yaml) = code artifacts → 4.1
  "scaffold the VirtualService manifests" → 4.1
  Routing rules as code artifacts always map to IMPLEMENT / 4.1

---

## B14-router-029-004 — IstioMesh routing: three-way concurrent 3.5 + 6.3 + 4.1

Full Istio deployment routing:
  ARCHITECTURE → 3.5 (mesh topology design)
  SECURITY → 6.3 (mTLS enforcement)
  IMPLEMENT → 4.1 (manifest scaffolding)
  Sequential ordering: 3.5 → 6.3 → 4.1
  TransitionType: feature_addition
  Three concurrent flows, all deterministic_generic
