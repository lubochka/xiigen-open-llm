# Layer 0-D — Flow Factory Pairs (038)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B14-factory-038-001 — SpotInstances: FlowFactory for IMPLEMENT + SECURITY

FlowFactory.CreateMulti(["IMPLEMENT", "SECURITY"]):
  → BackendServiceFlow41 (4.1)
  → SecurityAuditFlow63 (6.3)
  FlowCount: 2 | TransitionType: feature_addition
  "Write the spot node group Terraform module and enforce IMDSv2"

---

## B14-factory-038-002 — SpotInstances: FlowFactory for ARCHITECTURE + IMPLEMENT + SECURITY

FlowFactory.CreateMulti(["ARCHITECTURE", "IMPLEMENT", "SECURITY"]):
  → InfrastructureArchitectureFlow35 (3.5)
  → BackendServiceFlow41 (4.1)
  → SecurityAuditFlow63 (6.3)
  FlowCount: 3 | TransitionType: feature_addition
  Sequential: 3.5 → 6.3 → 4.1

---

## B14-factory-038-003 — SpotInstances: FlowFactory for ARCHITECTURE + IMPLEMENT + SECURITY + DOCUMENT

FlowFactory.CreateMulti(["ARCHITECTURE", "IMPLEMENT", "SECURITY", "DOCUMENT"]):
  → InfrastructureArchitectureFlow35 (3.5)
  → BackendServiceFlow41 (4.1)
  → SecurityAuditFlow63 (6.3)
  → DocumentationFlow62 (6.2)
  FlowCount: 4 | TransitionType: feature_addition
  Sequential: 3.5 → 6.3 → 4.1 → 6.2

---

## B14-factory-038-004 — SpotInstances: FlowFactory zero predicates

FlowFactory.Create([]) → GapHandler for "use spot instances", "request spot capacity"
  FlowCount: 0 | TrustState: knowledge_edge_synthetic
  CapabilityGapWarnings: gap-devops-cloud-001
