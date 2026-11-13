# Layer 0-D — Flow Factory Pairs (032)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B14-factory-032-001 — AlertManager: FlowFactory for ARCHITECTURE alone

FlowFactory.Create("ARCHITECTURE"):
  → InfrastructureArchitectureFlow35 (ARCHITECTURE / 3.5) | TrustState: deterministic_generic
  "Design the AlertManager routing topology with receiver hierarchy"
  FlowCount: 1 | TransitionType: feature_addition

---

## B14-factory-032-002 — AlertManager: FlowFactory for IMPLEMENT alone

FlowFactory.Create("IMPLEMENT"):
  → BackendServiceFlow41 (IMPLEMENT / 4.1) | TrustState: deterministic_generic
  "Implement the AlertManager configuration file and write routing tree YAML"
  FlowCount: 1 | TransitionType: feature_addition

---

## B14-factory-032-003 — AlertManager: FlowFactory for ARCHITECTURE + IMPLEMENT + DOCUMENT

FlowFactory.CreateMulti(["ARCHITECTURE", "IMPLEMENT", "DOCUMENT"]):
  → InfrastructureArchitectureFlow35 (ARCHITECTURE / 3.5)
  → BackendServiceFlow41 (IMPLEMENT / 4.1)
  → DocumentationFlow62 (DOCUMENT / 6.2)
  FlowCount: 3 | TransitionType: feature_addition (IMPLEMENT overrides DOCUMENT)
  Sequential: 3.5 → 4.1 → 6.2

---

## B14-factory-032-004 — AlertManager: FlowFactory for SECURITY alone (TLS + API key)

FlowFactory.Create("SECURITY"):
  → SecurityAuditFlow63 (SECURITY / 6.3) | TrustState: deterministic_generic
  "Configure TLS for AlertManager and enforce API key authentication"
  FlowCount: 1 | TransitionType: feature_addition
