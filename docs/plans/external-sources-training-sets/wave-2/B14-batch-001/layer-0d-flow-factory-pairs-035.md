# Layer 0-D — Flow Factory Pairs (035)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B14-factory-035-001 — CrossAccountIAM: FlowFactory for AUTHORIZATION alone

FlowFactory.Create("AUTHORIZATION"):
  → AuthorizationFlow (AUTHORIZATION / authorization sub-phase) | TrustState: deterministic_generic
  "Grant the cross-account IAM (Identity and Access Management) role for shared services access"
  FlowCount: 1 | TransitionType: feature_addition

---

## B14-factory-035-002 — CrossAccountIAM: FlowFactory for ARCHITECTURE + AUTHORIZATION

FlowFactory.CreateMulti(["ARCHITECTURE", "AUTHORIZATION"]):
  → InfrastructureArchitectureFlow35 (ARCHITECTURE / 3.5)
  → AuthorizationFlow (AUTHORIZATION / authorization sub-phase)
  FlowCount: 2 | TransitionType: feature_addition
  Sequential: 3.5 → AUTHORIZATION (design IAM trust model before defining role bindings)

---

## B14-factory-035-003 — CrossAccountIAM: FlowFactory for ARCHITECTURE + AUTHORIZATION + SECURITY

FlowFactory.CreateMulti(["ARCHITECTURE", "AUTHORIZATION", "SECURITY"]):
  → InfrastructureArchitectureFlow35 (ARCHITECTURE / 3.5)
  → AuthorizationFlow (AUTHORIZATION)
  → SecurityAuditFlow63 (SECURITY / 6.3)
  FlowCount: 3 | TransitionType: feature_addition
  Sequential: 3.5 → 6.3 → AUTHORIZATION

---

## B14-factory-035-004 — CrossAccountIAM: FlowFactory zero predicates (assume-role operations)

FlowFactory.Create([]) → GapHandler for "assume role" or "switch account"
  FlowCount: 0 | TrustState: knowledge_edge_synthetic
  CapabilityGapWarnings: gap-devops-cloud-001 ("assume", "switch account", "cross-account" are gap operational vocabulary)
  "assume role" is not AUTHORIZATION; "switch account" is not ARCHITECTURE
