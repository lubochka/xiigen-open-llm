# Layer 2 — Nine-Node Flow Pairs (010)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: nine_node_flow (InfrastructureArchitectureFlow35)

---

## B14-nine-010 — Network security architecture (zero-trust)

Request: "Design the zero-trust network architecture for the microservices platform — define mutual authentication boundaries and east-west traffic encryption policy."

### Flow: InfrastructureArchitectureFlow35

**Node 1 — Input parsing**
  Target: Platform network security architecture | Principle: zero-trust
  Architecture class: network security topology (fires ARCHITECTURE + SECURITY)

**Node 2 — Requirements extraction**
  Functional: all east-west service calls authenticated and encrypted; no implicit trust based on network location
  Non-functional: mTLS overhead < 1ms; certificate rotation without service restart

**Node 3 — Option generation**
  Implementation A: Istio service mesh with STRICT mTLS — automated certificate management via cert-manager
  Implementation B: Manual mTLS configuration per service — high operational burden; error-prone
  Implementation C: WireGuard overlay network — layer-3 encryption; less granular than service-level mTLS

**Node 4 — Trade-off analysis**
  Istio STRICT mTLS: automated via service mesh; certificates managed by cert-manager + Let's Encrypt
  Covers all service-to-service calls without application code changes; chosen
  WireGuard: lower overhead but no service-level identity; rejected — zero-trust requires service identity

**Node 5 — ADR**
  Decision: Istio STRICT mTLS with cert-manager for automated certificate lifecycle
  Rationale: Zero-trust requires service-level identity; mTLS provides both encryption and identity verification
  Trade-off: Requires all services to be Istio-enrolled; enforced via namespace-level PeerAuthentication policy

**Node 6 — Component placement**
  Istio: STRICT mode on all service namespaces | cert-manager: ClusterIssuer with 24-hour certificate TTL
  NetworkPolicy: deny-all default; explicit allow rules for required service communication paths
  Egress: all outbound traffic via egress gateway; allowlist for external endpoints

**Node 7 — Validation**
  mTLS coverage: 100% east-west traffic encrypted and authenticated → PASS
  Certificate rotation: auto-rotation at 80% of 24h TTL without service restart → PASS
  Arbiter: ArchitectureCompletenessArbiter

**Node 8 — Output**
  ADR: Network-ADR-010-zero-trust.md | Zero-trust boundary diagram

**Node 9 — Verdict**
  PASS — Zero-trust network architecture complete; mTLS, NetworkPolicy, and egress controls all defined

