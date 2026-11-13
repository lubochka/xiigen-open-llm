# Layer 0-D — Flow Factory Pairs (050)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B14-factory-050-001 — RBACRoles: FlowFactory for AUTHORIZATION alone

FlowFactory.Create("AUTHORIZATION"):
  → AuthorizationFlow (authorization sub-phase) | TransitionType: feature_addition
  "Define a ClusterRole for namespace-admin operations" / "Grant read-only ClusterRole to monitoring service account"
  FlowCount: 1 | TrustState: deterministic_generic

---

## B14-factory-050-002 — RBACRoles: FlowFactory for ARCHITECTURE + AUTHORIZATION + IMPLEMENT

FlowFactory.CreateMulti(["ARCHITECTURE", "AUTHORIZATION", "IMPLEMENT"]):
  → InfrastructureArchitectureFlow35 (3.5)
  → AuthorizationFlow (authorization sub-phase)
  → BackendServiceFlow41 (4.1)
  FlowCount: 3 | TransitionType: feature_addition
  Sequential: 3.5 → AUTHORIZATION → 4.1

---

## B14-factory-050-003 — RBACRoles: FlowFactory for AUTHORIZATION + DOCUMENT

FlowFactory.CreateMulti(["AUTHORIZATION", "DOCUMENT"]):
  → AuthorizationFlow (authorization sub-phase)
  → DocumentationFlow62 (6.2)
  FlowCount: 2 | TransitionType: documentation (IMPLEMENT absent; DOCUMENT present)
  Sequential: AUTHORIZATION → 6.2

---

## B14-factory-050-004 — RBACRoles: FlowFactory zero predicates

FlowFactory.Create([]) → GapHandler for "list all RBAC roles", "check access permissions"
  FlowCount: 0 | TrustState: knowledge_edge_synthetic
  CapabilityGapWarnings: gap-devops-cloud-001
