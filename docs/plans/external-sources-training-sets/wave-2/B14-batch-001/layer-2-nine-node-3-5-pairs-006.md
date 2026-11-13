# Layer 2 — Nine-Node Flow Pairs (006)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: nine_node_flow (InfrastructureArchitectureFlow35)

---

## B14-nine-006 — Service mesh sidecar configuration

Request: "Architect the service mesh sidecar proxy configuration for the microservices platform — define mTLS (mutual TLS) policy, circuit breaker thresholds, and health check intervals."

### Flow: InfrastructureArchitectureFlow35

**Node 1 — Input parsing**
  Target: Service mesh configuration | Platform: Istio sidecar proxies
  Architecture class: service mesh topology + security policy

**Node 2 — Requirements extraction**
  Functional: mTLS between all services; circuit breaker on downstream failures; health check for traffic routing
  Non-functional: circuit breaker response < 10ms; mTLS overhead < 1ms

**Node 3 — Option generation**
  mTLS mode A: STRICT — all traffic must be mTLS; blocks non-TLS calls
  mTLS mode B: PERMISSIVE — allows both mTLS and plaintext; useful during migration
  Circuit breaker: consecutive 5xx threshold (3, 5, 10) for ejection trigger

**Node 4 — Trade-off analysis**
  mTLS STRICT: selected for production; eliminates plaintext east-west traffic; requires all services enrolled in mesh
  Circuit breaker threshold: 5 consecutive 5xx → 30-second ejection; balances stability and availability
  Health check: 5-second interval; 3 consecutive failures → unhealthy

**Node 5 — ADR**
  Decision: STRICT mTLS + circuit breaker (5 consecutive 5xx → 30s ejection) + 5s health check intervals
  Rationale: STRICT mTLS enforces zero-trust east-west; PERMISSIVE only during onboarding
  Trade-off: STRICT mTLS requires all services mesh-enrolled; gradual rollout via PERMISSIVE → STRICT migration

**Node 6 — Component placement**
  Istio PeerAuthentication: STRICT mode per namespace | DestinationRule: outlierDetection enabled per service
  Circuit breaker: consecutiveGatewayErrors: 5; interval: 10s; baseEjectionTime: 30s

**Node 7 — Validation**
  mTLS coverage: 100% of service-to-service calls encrypted → PASS
  Circuit breaker: 5 errors triggers ejection in 10s window → PASS
  Arbiter: ArchitectureCompletenessArbiter

**Node 8 — Output**
  ADR: ServiceMesh-ADR-006-sidecar-config.md | Istio policy manifests

**Node 9 — Verdict**
  PASS — Service mesh configuration complete; mTLS, circuit breaker, and health checks all defined

