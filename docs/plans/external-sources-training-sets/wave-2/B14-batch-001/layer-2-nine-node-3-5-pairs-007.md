# Layer 2 — Nine-Node Flow Pairs (007)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: nine_node_flow (InfrastructureArchitectureFlow35)

---

## B14-nine-007 — API gateway routing architecture

Request: "Design the API gateway architecture for the platform public API — define routing rules, rate limiting tiers, and mTLS termination strategy."

### Flow: InfrastructureArchitectureFlow35

**Node 1 — Input parsing**
  Target: Public API gateway | Scope: routing, rate limiting, TLS termination
  Architecture class: ingress + API gateway topology

**Node 2 — Requirements extraction**
  Functional: route requests to downstream services; enforce per-consumer rate limits; terminate TLS at gateway
  Non-functional: gateway latency overhead < 5ms; rate limit enforcement < 1ms

**Node 3 — Option generation**
  Routing model A: path-based routing — /orders → OrderService; /products → ProductService
  Routing model B: header-based routing — X-Service-Name header drives routing (more flexible, complex)
  Rate limiting: in-memory (per-instance) vs distributed (Redis-backed)

**Node 4 — Trade-off analysis**
  Path-based routing: simpler; coupling between URL path and service; chosen for initial design
  Distributed rate limiting: consistent across gateway instances; adds Redis dependency; chosen
  TLS termination: at gateway; backend communication over mTLS via service mesh

**Node 5 — ADR**
  Decision: Path-based routing + Redis-backed rate limiting + TLS termination at gateway
  Rationale: Path-based is predictable and observable; distributed rate limiting prevents bypass via multiple gateway replicas
  Trade-off: Redis as dependency; mitigated by Redis Sentinel (Redis high-availability mode)

**Node 6 — Component placement**
  Gateway: Kong (API gateway) with 3 replicas | Redis: Sentinel cluster (3 nodes)
  Rate limit tiers: Free (100 req/min), Pro (5,000 req/min), Enterprise (unlimited with burst cap)
  TLS: Let's Encrypt (certificate authority) wildcard cert; renewed automatically

**Node 7 — Validation**
  Rate limit enforcement: 100 req/min tier tested at 101 req/min → 429 returned → PASS
  Gateway latency: p99 measured at 3.8ms → PASS (< 5ms)
  Arbiter: ArchitectureCompletenessArbiter

**Node 8 — Output**
  ADR: APIGateway-ADR-007-routing-architecture.md | Kong routing configuration

**Node 9 — Verdict**
  PASS — API gateway architecture complete; routing, rate limiting, and TLS all defined with trade-offs

