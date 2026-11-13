# Layer 0-D — Flow Factory Pairs (027)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B14-factory-027-001 — ArgoCD: FlowFactory for ARCHITECTURE + IMPLEMENT + SECURITY

FlowFactory.CreateMulti(["ARCHITECTURE", "IMPLEMENT", "SECURITY"]):
  → InfrastructureArchitectureFlow35 (ARCHITECTURE / 3.5)
  → BackendServiceFlow41 (IMPLEMENT / 4.1)
  → SecurityAuditFlow63 (SECURITY / 6.3)
  FlowCount: 3 | TransitionType: feature_addition
  Sequential: 3.5 → 6.3 → 4.1

---

## B14-factory-027-002 — ArgoCD: FlowFactory for ARCHITECTURE alone

FlowFactory.Create("ARCHITECTURE"):
  → InfrastructureArchitectureFlow35 (ARCHITECTURE / 3.5) | TrustState: deterministic_generic
  FlowCount: 1 | TransitionType: feature_addition (ARCHITECTURE with design verb = feature_addition)
  "Design the ArgoCD GitOps topology" → single InfrastructureArchitectureFlow35

---

## B14-factory-027-003 — ArgoCD: FlowFactory for SECURITY alone (RBAC)

FlowFactory.Create("SECURITY"):
  → SecurityAuditFlow63 (SECURITY / 6.3) | TrustState: deterministic_generic
  FlowCount: 1 | TransitionType: feature_addition
  "Configure RBAC for ArgoCD and enforce TLS" → single SecurityAuditFlow63

---

## B14-factory-027-004 — ArgoCD: FlowFactory zero predicates (sync/refresh)

FlowFactory.Create([]) → GapHandler for "sync the ArgoCD application" or "refresh the repository"
  FlowCount: 0 | TrustState: knowledge_edge_synthetic
  CapabilityGapWarnings: gap-devops-cloud-001 ("sync", "refresh", "ArgoCD" are gap)
  No flow instances created
