# Layer 2 — Nine-Node Flow Pairs (002)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: nine_node_flow (InfrastructureArchitectureFlow35)

---

## B14-nine-002 — Multi-region database replication topology

Request: "Architect the primary-replica database topology for OrderService — multi-region, automatic failover, RPO (Recovery Point Objective) < 1 minute."

### Flow: InfrastructureArchitectureFlow35

**Node 1 — Input parsing**
  Target: OrderService data layer | Constraint: RPO < 1 minute, multi-region, automatic failover
  Architecture class: data tier topology

**Node 2 — Requirements extraction**
  Functional: primary-replica replication; automatic failover; multi-region
  Non-functional: RPO < 1 minute; consistency: eventual (replica lag)

**Node 3 — Option generation**
  Option A: Synchronous replication — RPO = 0, higher write latency
  Option B: Asynchronous replication — RPO = seconds (depends on replication lag), lower write latency
  Option C: Semi-synchronous — one replica sync, others async

**Node 4 — Trade-off analysis**
  Synchronous: guarantees RPO = 0 but adds ~5ms cross-region write latency; acceptable for payment writes
  Asynchronous: optimal throughput but RPO up to 10–30s under heavy load; violates < 1 minute constraint (acceptable)
  Selected: Asynchronous with < 1 minute lag SLO (Service Level Objective) enforced via monitoring

**Node 5 — ADR (Architecture Decision Record)**
  Decision: Asynchronous cross-region replication with automatic DNS (Domain Name System) failover
  Rationale: Write latency ≤ 2ms over synchronous; RPO < 1 minute achievable via continuous monitoring
  Trade-off: Possible stale reads on replica during lag window

**Node 6 — Component placement**
  Primary: us-east-1 | Replica: eu-west-1 | Failover DNS: Route53 health-check with 30-second TTL (Time to Live)
  Replication: PostgreSQL streaming replication + WAL (Write-Ahead Log) shipping

**Node 7 — Validation**
  RPO simulation: replica lag measured at 8 seconds under peak load → PASS (< 60s constraint)
  Failover test: primary termination → DNS promotion in 45 seconds → PASS
  Arbiter: ArchitectureCompletenessArbiter

**Node 8 — Output**
  ADR: OrderService-ADR-002-replication-topology.md
  Topology diagram: primary → replica with failover DNS overlay

**Node 9 — Verdict**
  PASS — All layers covered; trade-offs explicit; RPO constraint validated

