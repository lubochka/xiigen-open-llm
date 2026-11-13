# Layer 2 — Nine-Node Flow Pairs (022)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: nine_node_flow (InfrastructureArchitectureFlow35)

---

## B14-nine-022 — NetworkSecurityTopology — ARCHITECTURE+SECURITY

Request: "Design the zero-trust network security topology — VPC (Virtual Private Cloud) isolation, PrivateLink boundaries, deny-all NetworkPolicy baseline, and mTLS (mutual TLS) enforcement."

### Flow: InfrastructureArchitectureFlow35 + SecurityAuditFlow63

**Node 1 — DomainSplitter**
  SelectedLogicalTransition: ARCHITECTURE → 3.5 + SECURITY → 6.3
  Domains: vpc_isolation, privatelink_boundaries, network_policy, mtls_enforcement

**Node 2 — DomainTranslator**
  vpc_isolation: Public subnet (load balancer only); private subnet (services); database subnet (isolated); no public IP on services
  privatelink_boundaries: AWS PrivateLink for S3 and DynamoDB access; no internet egress for data plane
  network_policy: Kubernetes NetworkPolicy deny-all ingress baseline; explicit allow rules per service
  mtls_enforcement: Istio STRICT mTLS between all services; cert-manager for certificate lifecycle; 90-day cert rotation

**Node 3 — SchemePlanner**
  Pattern: Zero-trust — no implicit trust between services; every connection requires mTLS + explicit NetworkPolicy
  Decision: Deny-all NetworkPolicy baseline + explicit allow rules — least-privilege network access
  Decision: PrivateLink for AWS service access — data never traverses public internet

**Node 4 — DomainProcessor**
  Decision: Zero-trust deny-all NetworkPolicy baseline
  Rationale: Prevents lateral movement; any newly deployed service has no network access until explicitly granted
  Trade-off: Requires NetworkPolicy for every service pair; high ops overhead for large microservice estates
  Decision: STRICT mTLS Istio enforcement
  Rationale: All inter-service traffic encrypted and authenticated; certificate identity replaces IP-based trust
  Trade-off: ~10% latency overhead; certificate management complexity; cert-manager required

**Node 5 — Applier**
  Constraints: No service may have ingress without explicit NetworkPolicy allow; STRICT mTLS mode (no PERMISSIVE fallback); PrivateLink for all AWS data services

**Node 6 — ResultTranslator**
  Output: network-security-topology-adr.md + NetworkPolicy manifests + Istio PeerAuthentication config

**Node 7 — ArbiterSelector**
  SelectedArbiter: ArchitectureCompletenessArbiter
  Config: required: VPC isolation decision + trade-off; NetworkPolicy strategy; mTLS enforcement decision

**Node 8 — Arbiter**
  vpc_isolation: Public/private/database subnet separation ✓ | PASS
  privatelink_boundaries: PrivateLink for AWS data services ✓ | PASS
  network_policy: Deny-all baseline + explicit allow ✓ | trade_off: ops overhead ✓ | PASS
  mtls_enforcement: STRICT mTLS + cert-manager ✓ | trade_off: latency + cert complexity ✓ | PASS
  OverallVerdict: PASS

**Node 9 — Formatter**
  Delivery: network-security-topology-adr.md | Status: ready | Arbiter: PASS
