# Layer 2 — Nine-Node Flow Pairs (005)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: nine_node_flow (InfrastructureArchitectureFlow35)

---

## B14-nine-005 — Container orchestration scheduling architecture

Request: "Design the container orchestration scheduling architecture for the platform — define pod resource limits, namespace boundaries, and anti-affinity rules for replica distribution across availability zones."

### Flow: InfrastructureArchitectureFlow35

**Node 1 — Input parsing**
  Target: Kubernetes scheduling architecture | Scope: resource limits, namespace boundaries, anti-affinity
  Architecture class: container orchestration topology

**Node 2 — Requirements extraction**
  Functional: namespace isolation per team; anti-affinity for HA (High Availability); resource quotas
  Non-functional: no single AZ (Availability Zone) failure causes service downtime; quota enforcement prevents noisy neighbours

**Node 3 — Option generation**
  Namespace model A: one namespace per service — fine-grained; high namespace count
  Namespace model B: one namespace per team domain (orders, payments, catalog) — balanced isolation
  Anti-affinity: soft (preferred) vs hard (required) — hard guarantees AZ spread but may prevent scheduling

**Node 4 — Trade-off analysis**
  Namespace model B (per domain): 3 namespaces; team-scoped RBAC (Role-Based Access Control); resource quotas per domain
  Anti-affinity: preferredDuringScheduling (soft) — avoids scheduling failures on AZ capacity pressure
  Resource limits: CPU 500m request / 2000m limit; Memory 512Mi request / 2Gi limit (per container)

**Node 5 — ADR**
  Decision: Domain-scoped namespaces + soft anti-affinity + explicit resource limits
  Rationale: Hard anti-affinity risks unschedulable pods during AZ outage; soft provides best-effort AZ spread
  Trade-off: Soft anti-affinity may result in AZ imbalance under capacity pressure; monitored by alert

**Node 6 — Component placement**
  Namespaces: orders-ns, payments-ns, catalog-ns | Resource quotas: per namespace
  Anti-affinity: topology key = topology.kubernetes.io/zone; mode = preferredDuringSchedulingIgnoredDuringExecution

**Node 7 — Validation**
  AZ distribution: 3 replicas per service → 1 per AZ under normal conditions → PASS
  Quota enforcement: namespace quota prevents single service consuming > 30% cluster CPU → PASS
  Arbiter: ArchitectureCompletenessArbiter

**Node 8 — Output**
  ADR: Platform-ADR-005-scheduling-architecture.md | Namespace topology diagram

**Node 9 — Verdict**
  PASS — Scheduling architecture complete; all trade-offs documented; anti-affinity and quota constraints validated

