# Layer 2 — Nine-Node Flow Pairs (014)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: nine_node_flow (InfrastructureArchitectureFlow35)

---

## B14-nine-014 — ServiceMeshDesign — ARCHITECTURE+IMPLEMENT

Request: "Architect the Istio service mesh topology and implement the mTLS (mutual TLS) policy for all service-to-service communication."

### Flow: InfrastructureArchitectureFlow35 + BackendServiceFlow41

**Node 1 — DomainSplitter**
  SelectedLogicalTransition: ARCHITECTURE → 3.5 + IMPLEMENT → 4.1
  Domains: mesh_topology, traffic_management, security_policy, observability_integration

**Node 2 — DomainTranslator**
  mesh_topology: Istio control plane (istiod); data plane (Envoy sidecars); sidecar injection per namespace
  traffic_management: VirtualService routing rules; DestinationRule load balancing; fault injection for testing
  security_policy: PeerAuthentication STRICT mTLS; AuthorizationPolicy deny-all baseline
  observability_integration: Prometheus metrics scraping; Jaeger trace propagation via Envoy

**Node 3 — SchemePlanner**
  Pattern: Sidecar proxy per pod; STRICT mTLS across all namespaces
  Decision: STRICT mTLS mode — reject all plaintext connections between services
  Decision: DestinationRule circuit breaker — 5 consecutive 5xx → eject for 30 seconds

**Node 4 — DomainProcessor**
  Decision: STRICT mTLS enforcement
  Rationale: Prevents man-in-the-middle (MITM) attacks on internal traffic; no plaintext allowed
  Trade-off: ~10% latency overhead per request from TLS handshake; acceptable for security compliance
  Decision: Deny-all AuthorizationPolicy baseline
  Rationale: Zero-trust posture; services must explicitly declare allowed communication
  Trade-off: Requires explicit policy for every service-to-service call; higher ops burden

**Node 5 — Applier**
  Constraints: STRICT mTLS on all namespaces; deny-all AuthorizationPolicy baseline; sidecar injection enabled per namespace annotation

**Node 6 — ResultTranslator**
  Output: service-mesh-topology-adr.md + Istio policy manifests (PeerAuthentication.yaml, AuthorizationPolicy.yaml)

**Node 7 — ArbiterSelector**
  SelectedArbiter: ArchitectureCompletenessArbiter
  Config: required: mTLS policy decision + trade-off; observability integration

**Node 8 — Arbiter**
  mesh_topology: Sidecar injection strategy ✓ | trade_off: latency overhead ✓ | PASS
  security_policy: STRICT mTLS + deny-all ✓ | trade_off: ops burden ✓ | PASS
  observability_integration: Prometheus + Jaeger ✓ | PASS
  OverallVerdict: PASS

**Node 9 — Formatter**
  Delivery: service-mesh-topology-adr.md | Status: ready | Arbiter: PASS
