# Layer 2 — Nine-Node Flow Pairs (011)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: nine_node_flow (InfrastructureArchitectureFlow35)

---

## B14-nine-011 — Disaster recovery topology

Request: "Architect the disaster recovery (DR) topology for the payments platform — active-active cross-region with RTO (Recovery Time Objective) < 5 minutes and RPO (Recovery Point Objective) < 30 seconds."

### Flow: InfrastructureArchitectureFlow35

**Node 1 — Input parsing**
  Target: Payments platform DR topology | Constraints: RTO < 5 min, RPO < 30s, active-active
  Architecture class: disaster recovery + high availability topology

**Node 2 — Requirements extraction**
  Functional: active-active in two regions (us-east-1, eu-west-1); automatic failover; data synchronisation < 30s lag
  Non-functional: RTO < 5 minutes (detection + failover + DNS propagation); RPO < 30 seconds

**Node 3 — Option generation**
  Option A: Active-active with synchronous DB replication — RPO = 0; higher write latency
  Option B: Active-active with asynchronous DB replication — RPO = replication lag; lower write latency
  Option C: Active-passive — RTO 5-15 min (cold standby); too slow for < 5 min RTO

**Node 4 — Trade-off analysis**
  Synchronous replication: write latency + ~8ms for cross-region roundtrip; acceptable for payment transactions
  Active-passive: rejected — warm-up time violates RTO < 5 minutes
  Global load balancer: Cloudflare (CDN/load balancing) with 60-second health-check interval

**Node 5 — ADR**
  Decision: Active-active, synchronous PostgreSQL replication, global load balancer with 60s health-check
  Rationale: RPO = 0 with synchronous replication; write latency increase acceptable for payment SLA
  Trade-off: Synchronous replication adds ~8ms write latency; mitigated by regional routing (users in EU → eu-west-1)

**Node 6 — Component placement**
  Regions: us-east-1 (primary traffic), eu-west-1 (secondary + EU user primary)
  DB: PostgreSQL synchronous streaming replication across regions
  Failover: Global load balancer failover within 60s detection + 30s DNS TTL = ~90s total → PASS (< 5 min)

**Node 7 — Validation**
  RPO simulation: synchronous replication — committed transactions appear in replica immediately → RPO = 0 → PASS (< 30s)
  RTO simulation: primary region failure → failover in 90 seconds → PASS (< 5 min)
  Arbiter: ArchitectureCompletenessArbiter

**Node 8 — Output**
  ADR: Payments-ADR-011-dr-topology.md | Active-active topology diagram

**Node 9 — Verdict**
  PASS — DR topology complete; RTO and RPO constraints met with explicit trade-offs documented

