# Layer 2 — Nine-Node Flow Pairs (017)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: nine_node_flow (InfrastructureArchitectureFlow35)

---

## B14-nine-017 — AutoScalingInfra — ARCHITECTURE+IMPLEMENT

Request: "Design the auto-scaling infrastructure topology and implement the HPA (Horizontal Pod Autoscaler) and cluster autoscaler configuration."

### Flow: InfrastructureArchitectureFlow35 + BackendServiceFlow41

**Node 1 — DomainSplitter**
  SelectedLogicalTransition: ARCHITECTURE → 3.5 + IMPLEMENT → 4.1
  Domains: hpa_design, cluster_autoscaler, vpa_policy, scaling_thresholds

**Node 2 — DomainTranslator**
  hpa_design: CPU-based HPA 70% threshold; custom metrics HPA for queue depth; scale-in cooldown 5 minutes
  cluster_autoscaler: Node group min/max bounds; scale-up trigger: pending pods; scale-down delay: 10 minutes
  vpa_policy: VPA (Vertical Pod Autoscaler) in recommendation mode only; HPA takes precedence
  scaling_thresholds: OrderService CPU threshold 70%; NotificationService queue depth > 1000 messages

**Node 3 — SchemePlanner**
  Pattern: HPA primary + cluster autoscaler secondary; VPA advisory only
  Decision: HPA CPU threshold 70% — 30% headroom before scale-up completes (~60 seconds)
  Decision: Cluster autoscaler scale-down delay 10 minutes — prevents thrashing on transient spikes

**Node 4 — DomainProcessor**
  Decision: CPU-based HPA at 70% utilization
  Rationale: 70% leaves adequate headroom; scale-up latency ~60 seconds; avoid 100% saturation
  Trade-off: Over-provisioned at low load → cost; accept for SLA compliance
  Decision: Cluster autoscaler 10-minute scale-down delay
  Rationale: Prevents node churn on brief traffic spikes; node startup costs ~3 minutes
  Trade-off: Excess nodes idle for 10 minutes post-spike; cost-effective vs churn cost

**Node 5 — Applier**
  Constraints: HPA on all stateless services; CPU threshold not to exceed 80%; cluster autoscaler must respect PodDisruptionBudget minimums

**Node 6 — ResultTranslator**
  Output: autoscaling-topology-adr.md + HPA and cluster-autoscaler manifest configurations

**Node 7 — ArbiterSelector**
  SelectedArbiter: ArchitectureCompletenessArbiter
  Config: required: HPA threshold decision + trade-off; cluster autoscaler delay decision + trade-off

**Node 8 — Arbiter**
  hpa_design: 70% CPU threshold + 5-min scale-in cooldown ✓ | trade_off: cost vs SLA ✓ | PASS
  cluster_autoscaler: 10-min scale-down delay ✓ | trade_off: idle cost vs churn ✓ | PASS
  vpa_policy: Advisory mode only ✓ | PASS
  OverallVerdict: PASS

**Node 9 — Formatter**
  Delivery: autoscaling-topology-adr.md + HPA-config.yaml | Status: ready | Arbiter: PASS
