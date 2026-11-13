# Layer 0-D — Flow Factory Pairs (025)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B14-factory-025-001 — ContainerRegistry: FlowFactory output for ARCHITECTURE + IMPLEMENT

FlowFactory.CreateMulti(["ARCHITECTURE", "IMPLEMENT"]):
  → InfrastructureArchitectureFlow35 (ARCHITECTURE / 3.5) | TrustState: deterministic_generic
  → BackendServiceFlow41 (IMPLEMENT / 4.1) | TrustState: deterministic_generic
  FlowCount: 2 | TransitionType: feature_addition (IMPLEMENT present)

---

## B14-factory-025-002 — ContainerRegistry: FlowFactory output for SECURITY alone

FlowFactory.Create("SECURITY"):
  → SecurityAuditFlow63 (SECURITY / 6.3) | TrustState: deterministic_generic
  FlowCount: 1 | TransitionType: feature_addition (SECURITY alone → feature_addition; not documentation)

---

## B14-factory-025-003 — ContainerRegistry: FlowFactory output for ARCHITECTURE + API_CONTRACT

FlowFactory.CreateMulti(["ARCHITECTURE", "API_CONTRACT"]):
  → InfrastructureArchitectureFlow35 (ARCHITECTURE / 3.5)
  → APIContractFlow21 (API_CONTRACT / 2.1)
  FlowCount: 2 | TransitionType: feature_addition (ARCHITECTURE alone with design verb)
  Sequential ordering: 3.5 → 2.1 (design before contract specification)

---

## B14-factory-025-004 — ContainerRegistry: FlowFactory zero predicates

FlowFactory.Create([]) → GapHandler
  FlowCount: 0 | TrustState: knowledge_edge_synthetic
  CapabilityGapSignalForwarded: true
  No flow instances created; GapHandler receives gap vocabulary for capability gap warning
