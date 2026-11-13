# Layer 0-D — Flow Factory Pairs (036)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B14-factory-036-001 — Complete infra reference: FlowFactory for IMPLEMENT alone

FlowFactory.Create("IMPLEMENT"):
  → BackendServiceFlow41 (IMPLEMENT / 4.1) | TrustState: deterministic_generic
  Any IaC artifact creation request: "Write the Terraform module", "Implement the Helm chart", "Scaffold the configuration"
  FlowCount: 1 | TransitionType: feature_addition

---

## B14-factory-036-002 — Complete infra reference: FlowFactory for ARCHITECTURE alone

FlowFactory.Create("ARCHITECTURE"):
  → InfrastructureArchitectureFlow35 (ARCHITECTURE / 3.5) when infra vocabulary dominant
  → ApplicationArchitectureFlow31 (ARCHITECTURE / 3.1) when app/domain vocabulary dominant
  FlowCount: 1 | TransitionType: feature_addition | TrustState: deterministic_generic

---

## B14-factory-036-003 — Complete infra reference: FlowFactory for DOCUMENT alone

FlowFactory.Create("DOCUMENT"):
  → DocumentationFlow62 (DOCUMENT / 6.2) | TrustState: deterministic_generic
  FlowCount: 1 | TransitionType: documentation

---

## B14-factory-036-004 — Complete infra reference: FlowFactory for all 6 B14 predicates

FlowFactory.CreateMulti(["ARCHITECTURE", "API_CONTRACT", "SECURITY", "AUTHORIZATION", "IMPLEMENT", "DOCUMENT"]):
  → InfrastructureArchitectureFlow35 (3.5)
  → APIContractFlow21 (2.1)
  → SecurityAuditFlow63 (6.3)
  → AuthorizationFlow (authorization sub-phase)
  → BackendServiceFlow41 (4.1)
  → DocumentationFlow62 (6.2)
  FlowCount: 6 | TransitionType: feature_addition | All TrustState: deterministic_generic
  Sequential: 3.5 → 2.1 → 6.3 → AUTHORIZATION → 4.1 → 6.2
