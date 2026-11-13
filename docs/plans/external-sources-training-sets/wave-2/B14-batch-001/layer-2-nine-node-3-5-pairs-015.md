# Layer 2 — Nine-Node Flow Pairs (015)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: nine_node_flow (InfrastructureArchitectureFlow35)

---

## B14-nine-015 — SecretsManagementTopology — ARCHITECTURE+SECURITY

Request: "Design the secrets management topology — HashiCorp Vault cluster, dynamic secret generation, certificate lifecycle management, and key rotation policies."

### Flow: InfrastructureArchitectureFlow35 + SecurityAuditFlow63

**Node 1 — DomainSplitter**
  SelectedLogicalTransition: ARCHITECTURE → 3.5 + SECURITY → 6.3
  Domains: vault_cluster_topology, secret_generation, certificate_lifecycle, key_rotation

**Node 2 — DomainTranslator**
  vault_cluster_topology: 3-node Vault cluster; Raft consensus; unsealing via auto-unseal (AWS KMS)
  secret_generation: Dynamic database credentials; TTL (Time to Live) 1 hour; lease renewal
  certificate_lifecycle: Vault PKI (Public Key Infrastructure) secrets engine; intermediate CA (Certificate Authority); 90-day leaf cert rotation
  key_rotation: Transit secrets engine; key versioning; automatic rotation every 30 days

**Node 3 — SchemePlanner**
  Pattern: Vault as single secrets plane; all services fetch credentials at startup; no static secrets in config
  Decision: Dynamic database credentials — services receive fresh credentials per connection
  Decision: Vault PKI for all TLS (Transport Layer Security) certificates; no manually managed certificates

**Node 4 — DomainProcessor**
  Decision: Auto-unseal via AWS KMS (Key Management Service)
  Rationale: Prevents manual unseal requirement on Vault restart; KMS protects unseal keys
  Trade-off: AWS KMS dependency; Vault unavailable if KMS unreachable
  Decision: 1-hour TTL on database credentials
  Rationale: Short TTL limits blast radius if credentials leaked
  Trade-off: Higher database connection overhead from frequent credential renewal

**Node 5 — Applier**
  Constraints: No static secrets in any configuration file; all services must use Vault Agent for credential injection; cert rotation automated via Vault PKI

**Node 6 — ResultTranslator**
  Output: secrets-management-topology-adr.md + Vault policy definitions

**Node 7 — ArbiterSelector**
  SelectedArbiter: ArchitectureCompletenessArbiter
  Config: required: vault topology decision + trade-off; secret TTL policy; certificate automation

**Node 8 — Arbiter**
  vault_cluster_topology: 3-node Raft + auto-unseal ✓ | trade_off: KMS dependency ✓ | PASS
  secret_generation: Dynamic credentials + 1h TTL ✓ | trade_off: connection overhead ✓ | PASS
  certificate_lifecycle: Vault PKI + 90-day rotation ✓ | PASS
  key_rotation: Transit engine + 30-day rotation ✓ | PASS
  OverallVerdict: PASS

**Node 9 — Formatter**
  Delivery: secrets-management-topology-adr.md | Status: ready | Arbiter: PASS
