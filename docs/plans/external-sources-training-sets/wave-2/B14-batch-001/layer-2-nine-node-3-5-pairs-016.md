# Layer 2 — Nine-Node Flow Pairs (016)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: nine_node_flow (InfrastructureArchitectureFlow35)

---

## B14-nine-016 — CDNAndEdgeTopology — ARCHITECTURE alone

Request: "Design the CDN (Content Delivery Network) and edge topology — global PoP (Point of Presence) placement, origin failover, cache hierarchy, and TLS termination strategy."

### Flow: InfrastructureArchitectureFlow35

**Node 1 — DomainSplitter**
  SelectedLogicalTransition: ARCHITECTURE → InfrastructureArchitectureFlow35
  Domains: pop_placement, cache_hierarchy, origin_failover, tls_termination

**Node 2 — DomainTranslator**
  pop_placement: 8 global PoPs; latency-based routing; Anycast IP
  cache_hierarchy: L1 edge cache (PoP); L2 regional cache; origin shield before origin
  origin_failover: Primary origin + hot standby; health-check interval 10 seconds; failover threshold: 2 consecutive failures
  tls_termination: TLS terminates at edge PoP; wildcard certificate; HSTS (HTTP Strict Transport Security) preload

**Node 3 — SchemePlanner**
  Pattern: CDN L1→L2→Shield→Origin hierarchy; all TLS at edge
  Decision: Origin shield reduces origin traffic by ~80% at cost of 20ms additional latency
  Decision: Wildcard certificate — single cert covers all subdomains; simpler rotation

**Node 4 — DomainProcessor**
  Decision: Latency-based routing via Anycast
  Rationale: Client routes to nearest PoP automatically; no DNS (Domain Name System) TTL latency
  Trade-off: Anycast requires BGP (Border Gateway Protocol) prefix announcement; complex network ops
  Decision: Cache TTL hierarchy — edge TTL 5 minutes; regional TTL 30 minutes; origin TTL 24 hours
  Trade-off: Longer TTL means staleness; shorter TTL means higher origin load; 5/30/1440 minute ladder balances both

**Node 5 — Applier**
  Constraints: TLS must terminate at edge; no HTTP (plaintext); wildcard cert renewed 30 days before expiry; origin shield mandatory for all origins

**Node 6 — ResultTranslator**
  Output: cdn-edge-topology-adr.md + cache TTL policy matrix

**Node 7 — ArbiterSelector**
  SelectedArbiter: ArchitectureCompletenessArbiter
  Config: required: PoP placement decision + trade-off; cache hierarchy; TLS strategy

**Node 8 — Arbiter**
  pop_placement: Anycast 8 PoPs ✓ | trade_off: BGP complexity ✓ | PASS
  cache_hierarchy: L1→L2→Shield hierarchy ✓ | trade_off: TTL staleness ladder ✓ | PASS
  origin_failover: Hot standby + health check ✓ | PASS
  tls_termination: Edge TLS + wildcard cert + HSTS ✓ | PASS
  OverallVerdict: PASS

**Node 9 — Formatter**
  Delivery: cdn-edge-topology-adr.md | Status: ready | Arbiter: PASS
