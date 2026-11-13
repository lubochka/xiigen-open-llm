# Layer 0-D — Flow Factory Pairs (049)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B14-factory-049-001 — ServiceAccountConfig: FlowFactory for IMPLEMENT + AUTHORIZATION + SECURITY

FlowFactory.CreateMulti(["IMPLEMENT", "AUTHORIZATION", "SECURITY"]):
  → BackendServiceFlow41 (4.1)
  → AuthorizationFlow (authorization sub-phase)
  → SecurityAuditFlow63 (6.3)
  "Implement the IRSA Terraform module, bind the ClusterRole, and enforce mTLS"
  FlowCount: 3 | TransitionType: feature_addition
  Sequential: 6.3 → AUTHORIZATION → 4.1

---

## B14-factory-049-002 — ServiceAccountConfig: FlowFactory for ARCHITECTURE + AUTHORIZATION + SECURITY

FlowFactory.CreateMulti(["ARCHITECTURE", "AUTHORIZATION", "SECURITY"]):
  → InfrastructureArchitectureFlow35 (3.5)
  → SecurityAuditFlow63 (6.3)
  → AuthorizationFlow (authorization sub-phase)
  FlowCount: 3 | TransitionType: feature_addition
  Sequential: 3.5 → 6.3 → AUTHORIZATION

---

## B14-factory-049-003 — ServiceAccountConfig: FlowFactory for AUTHORIZATION alone

FlowFactory.Create("AUTHORIZATION"):
  → AuthorizationFlow (authorization sub-phase) | TransitionType: feature_addition
  "Bind the db-reader ClusterRole to the application service account"
  FlowCount: 1 | TrustState: deterministic_generic

---

## B14-factory-049-004 — ServiceAccountConfig: FlowFactory zero predicates

FlowFactory.Create([]) → GapHandler for "create a service account", "delete the service account"
  FlowCount: 0 | TrustState: knowledge_edge_synthetic
  CapabilityGapWarnings: gap-devops-cloud-001
  Note: "create service account" without IaC artifact noun = operational gap; no flow created.
