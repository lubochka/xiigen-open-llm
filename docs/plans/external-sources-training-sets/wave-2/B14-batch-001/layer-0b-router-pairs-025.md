# Layer 0-B — Router Pairs (025)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: router

---

## B14-router-025-001 — ContainerRegistry routing

ARCHITECTURE → 3.5 when compiled text contains: "topology", "registry", "container" (infra context), "layer"
IMPLEMENT → 4.1 when compiled text contains: "implement", "write", "scaffold" + code artifact noun
SECURITY → 6.3 when compiled text contains: "TLS", "image signing", "credential", "certificate"
API_CONTRACT → 2.1 when compiled text contains: "API contract", "define" + endpoint/interface noun

---

## B14-router-025-002 — ContainerRegistry: ARCHITECTURE disambiguation 3.5 vs 3.1

3.5 (infra) selected when: "container registry topology", "image layer architecture", "registry cluster", "network topology"
3.1 (app) selected instead when: "OrderService registry integration", "domain-level container boundary" — app-domain vocabulary dominant
Teaching: Container registry architecture is always infra vocabulary → 3.5

---

## B14-router-025-003 — ContainerRegistry: SECURITY routing with API_CONTRACT

When SECURITY + API_CONTRACT both fire:
  SECURITY → 6.3 (concurrent)
  API_CONTRACT → 2.1 (concurrent)
  PrimarySubPhaseId: determined by first-declared predicate; if ARCHITECTURE also present → ARCHITECTURE (3.5) is primary
  Sequential: 3.5 → 2.1 → 6.3 if all three fire

---

## B14-router-025-004 — ContainerRegistry: zero-predicate routing

When compiled_predicate_ids: [] (gap vocabulary only):
  "Push to ECR" → knowledge_edge_synthetic
  No routing decisions emitted
  CapabilityGapSignalForwarded: true
  GapHandler receives: gap-devops-cloud-001 ("ECR", "push" are gap)
