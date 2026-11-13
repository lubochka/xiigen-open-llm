# Layer 2 — Nine-Node Flow Pairs (013)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: nine_node_flow (InfrastructureArchitectureFlow35)

---

## B14-nine-013 — EKS Cluster Topology — ARCHITECTURE alone

Request: "Design the EKS (Elastic Kubernetes Service) cluster topology — node group isolation, namespace boundaries, and control plane availability."

### Flow: InfrastructureArchitectureFlow35

**Node 1 — DomainSplitter**
  SelectedLogicalTransition: ARCHITECTURE → InfrastructureArchitectureFlow35
  Target: EKS cluster topology | Domains: control_plane, node_groups, namespace_isolation, networking

**Node 2 — DomainTranslator**
  control_plane: managed EKS control plane; multi-AZ (Availability Zone) endpoint; API server HA (High Availability)
  node_groups: on-demand group for stateful; Spot group for batch; taint-based scheduling
  namespace_isolation: production/staging/dev namespaces; ResourceQuota per namespace; NetworkPolicy deny-all baseline
  networking: VPC CNI (Container Network Interface) plugin; pod IP from VPC CIDR

**Node 3 — SchemePlanner**
  Schema: 4 domains × 3 components each = 12 components
  Pattern: Cluster topology C4 Level 2 — namespace boundary diagram + node group topology
  Decision: On-demand nodes for stateful workloads; Spot nodes tolerate interruptions; namespaces enforce quota isolation

**Node 4 — DomainProcessor**
  Decision: Multi-AZ node groups — 3 AZs, 2 nodes minimum per AZ
  Trade-off: Higher cost vs availability; accept for production SLA
  Decision: Namespace ResourceQuota — CPU (Central Processing Unit) and memory limits per namespace
  Trade-off: Quota requires careful sizing; too tight blocks legitimate workloads

**Node 5 — Applier**
  Constraints: All node groups must have at least 2 nodes; all namespaces must have ResourceQuota; NetworkPolicy deny-all is mandatory baseline

**Node 6 — ResultTranslator**
  Output: EKS cluster topology ADR (Architecture Decision Record) — node group isolation strategy + namespace boundary design

**Node 7 — ArbiterSelector**
  SelectedArbiter: ArchitectureCompletenessArbiter
  Config: required per domain: design_decision + trade_off; required cross-cutting: isolation, HA

**Node 8 — Arbiter**
  control_plane: Multi-AZ endpoint ✓ | trade_off: cost vs HA ✓ | PASS
  node_groups: On-demand + Spot separation ✓ | trade_off: cost optimization ✓ | PASS
  namespace_isolation: ResourceQuota + NetworkPolicy ✓ | trade_off: sizing risk ✓ | PASS
  OverallVerdict: PASS

**Node 9 — Formatter**
  Delivery: eks-cluster-topology-adr.md | Status: ready | Arbiter: PASS
