# Layer 0-D — Flow Factory Pairs (029)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B14-factory-029-001 — IstioMesh: FlowFactory for ARCHITECTURE + SECURITY + IMPLEMENT

FlowFactory.CreateMulti(["ARCHITECTURE", "SECURITY", "IMPLEMENT"]):
  → InfrastructureArchitectureFlow35 (ARCHITECTURE / 3.5)
  → SecurityAuditFlow63 (SECURITY / 6.3)
  → BackendServiceFlow41 (IMPLEMENT / 4.1)
  FlowCount: 3 | TransitionType: feature_addition
  Sequential: 3.5 → 6.3 → 4.1

---

## B14-factory-029-002 — IstioMesh: FlowFactory for ARCHITECTURE alone

FlowFactory.Create("ARCHITECTURE"):
  → InfrastructureArchitectureFlow35 (ARCHITECTURE / 3.5) | TrustState: deterministic_generic
  "Design the Istio service mesh topology" → single flow; TransitionType: feature_addition

---

## B14-factory-029-003 — IstioMesh: FlowFactory for SECURITY alone (mTLS)

FlowFactory.Create("SECURITY"):
  → SecurityAuditFlow63 (SECURITY / 6.3) | TrustState: deterministic_generic
  "Enforce mTLS across all namespaces" → single SecurityAuditFlow63
  FlowCount: 1 | TransitionType: feature_addition

---

## B14-factory-029-004 — IstioMesh: FlowFactory zero predicates (enable sidecar)

FlowFactory.Create([]) → GapHandler for "enable Istio sidecar injection"
  FlowCount: 0 | TrustState: knowledge_edge_synthetic
  CapabilityGapWarnings: gap-devops-cloud-001 ("enable", "sidecar injection", "Istio" are gap)
  All vocabulary is operational → zero predicates → knowledge_edge_synthetic
