# Layer 2 — Nine-Node Flow Pairs (026)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: nine_node_flow (InfrastructureArchitectureFlow35)

---

## B14-nine-026 — SpotInstances: InfrastructureArchitectureFlow35

### Node 01 — SelectedLogicalTransition
A: "Architect the spot instance fleet topology for compute workloads."

### Node 02 — DomainTranslator
Domains: compute_topology (spot node groups, on-demand baseline), interruption_handling (graceful drain, checkpointing), scheduling (affinity, toleration rules), observability (spot interruption monitoring)
Targets: 70% spot for batch; 100% on-demand for production APIs; drain window 120s; interruption handler latency < 2s

### Node 03 — SchemePlanner
Pattern: Mixed instance policy with spot diversification across availability zones (AZs) and instance families
Key decisions: (1) 3-AZ spot diversification to reduce correlated interruption; (2) Spot interruption handler as DaemonSet; (3) On-demand baseline at 20% of cluster capacity; (4) Cluster Autoscaler with spot priority

### Node 04 — DomainProcessor
Decision 1: 3-AZ diversification — Trade-off: higher complexity in node group configuration; reduces simultaneous interruption from 30% to < 5%.
Decision 2: DaemonSet handler — Trade-off: resource overhead per node; enables 120s graceful drain for all workloads.
Decision 3: 20% on-demand baseline — Trade-off: cost floor guaranteed; ensures minimum capacity during spot scarcity.
Decision 4: Autoscaler spot priority — Trade-off: complex priority class configuration; 60% average cost savings.

### Node 05 — Applier
Constraints: batch pods must have spot toleration; production pods must NOT have spot toleration; IMDSv2 (Instance Metadata Service v2) required on all nodes; PodDisruptionBudget (PDB) required for all batch workloads

### Node 06 — ResultTranslator
Artifacts: adr-spot-instance-architecture.md; spot-topology-diagram.drawio

### Node 07 — ArbiterSelector
TrustState: deterministic_generic → ArchitectureCompletenessArbiter

### Node 08 — Arbiter
All 4 domains: PASS (each has decision + trade-off)
OverallVerdict: PASS

### Node 09 — Formatter
Output: adr-spot-instance-architecture.md | Status: DELIVERED
