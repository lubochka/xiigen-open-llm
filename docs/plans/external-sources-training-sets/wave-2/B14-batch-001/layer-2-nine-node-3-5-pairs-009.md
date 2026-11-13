# Layer 2 — Nine-Node Flow Pairs (009)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: nine_node_flow (InfrastructureArchitectureFlow35)

---

## B14-nine-009 — Auto-scaling architecture

Request: "Structure the horizontal auto-scaling architecture for the OrderService compute layer — define HPA (Horizontal Pod Autoscaler) thresholds, cooldown periods, and minimum/maximum replica boundaries."

### Flow: InfrastructureArchitectureFlow35

**Node 1 — Input parsing**
  Target: OrderService auto-scaling | Scope: HPA configuration, replica boundaries, cooldown
  Architecture class: compute scaling topology

**Node 2 — Requirements extraction**
  Functional: scale-out on CPU > 70%; scale-in below 30%; minimum 3 replicas (for AZ coverage); maximum 20 replicas
  Non-functional: scale-out time < 2 minutes; scale-in cooldown 5 minutes (prevent thrashing)

**Node 3 — Option generation**
  Metric A: CPU utilization — simple; may not reflect request-driven load
  Metric B: Custom metric — requests-per-second per pod (more accurate for I/O-bound service)
  Option: CPU + custom metric (both triggers) — more responsive scaling

**Node 4 — Trade-off analysis**
  CPU-only: proven reliability; tuned at 70% CPU threshold; chosen for initial design
  Custom metric: requires metric server + adapter; adds complexity; planned for iteration 2
  Cooldown: 5-minute scale-in cooldown prevents fleet oscillation on transient traffic spikes

**Node 5 — ADR**
  Decision: CPU-based HPA, 70% scale-out threshold, 5-minute stabilization window, min 3 max 20 replicas
  Rationale: CPU is available without custom metric setup; 70% provides headroom before latency degrades
  Trade-off: CPU lags behind request queue growth; custom metric planned as enhancement

**Node 6 — Component placement**
  HPA manifest: targetCPUUtilizationPercentage: 70; minReplicas: 3; maxReplicas: 20
  Stabilization window: scaleDown: 300s; scaleUp: 60s
  Cluster Autoscaler: scales underlying nodes when pod pending > 30s

**Node 7 — Validation**
  Scale-out test: traffic spike → CPU > 70% → new pods scheduled in 90 seconds → PASS (< 2 min)
  Scale-in test: load drops → stabilization window prevents premature scale-in → PASS
  Arbiter: ArchitectureCompletenessArbiter

**Node 8 — Output**
  ADR: OrderService-ADR-009-autoscaling.md | HPA manifest configuration

**Node 9 — Verdict**
  PASS — Auto-scaling architecture complete; thresholds, boundaries, and cooldowns all defined

