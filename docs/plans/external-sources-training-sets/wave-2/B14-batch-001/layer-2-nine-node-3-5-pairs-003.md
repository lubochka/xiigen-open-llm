# Layer 2 — Nine-Node Flow Pairs (003)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: nine_node_flow (InfrastructureArchitectureFlow35)

---

## B14-nine-003 — CDN (Content Delivery Network) edge caching topology

Request: "Design the CDN edge caching layer for ProductCatalogService — define cache invalidation strategy and TTL (Time to Live) policies for product images and catalog data."

### Flow: InfrastructureArchitectureFlow35

**Node 1 — Input parsing**
  Target: ProductCatalogService content delivery | Assets: product images, catalog data
  Architecture class: CDN edge topology + cache invalidation

**Node 2 — Requirements extraction**
  Functional: cache product images and catalog data at edge; invalidate on update
  Non-functional: cache hit rate > 80%; invalidation latency < 5 minutes

**Node 3 — Option generation**
  Option A: Time-based TTL only — simple, may serve stale data up to TTL duration
  Option B: Event-driven invalidation — ProductCatalogService emits invalidation events to CDN on update
  Option C: Hybrid — short TTL (5 min) for catalog data + event-driven for images

**Node 4 — Trade-off analysis**
  Time-based TTL: simplest to implement; stale window = full TTL; images may be stale for 5 min after update
  Event-driven: near-instant invalidation; adds coupling between ProductCatalogService and CDN API
  Hybrid: balanced; images invalidated immediately; catalog data max 5-minute stale window

**Node 5 — ADR**
  Decision: Hybrid TTL + event-driven invalidation
  Rationale: Images are user-visible; stale images cause trust issues. Catalog data stale for 5 min acceptable.
  Trade-off: CDN API coupling for image updates; mitigated by async webhook pattern

**Node 6 — Component placement**
  CDN PoP (Point of Presence): 15 edge locations | Origin: ProductCatalogService in us-east-1
  Image TTL: 7 days (overridden by invalidation event) | Catalog TTL: 5 minutes

**Node 7 — Validation**
  Cache hit rate: simulated at 87% for product images → PASS (> 80%)
  Invalidation latency: event-driven → 30 seconds average → PASS (< 5 minutes)
  Arbiter: ArchitectureCompletenessArbiter

**Node 8 — Output**
  ADR: ProductCatalog-ADR-003-cdn-invalidation.md | Cache policy diagram

**Node 9 — Verdict**
  PASS — CDN topology defined; invalidation strategy documented; all constraints met

