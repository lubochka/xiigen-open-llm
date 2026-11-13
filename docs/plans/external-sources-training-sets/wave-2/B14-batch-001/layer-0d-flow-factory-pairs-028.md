# Layer 0-D — Flow Factory Pairs (028)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B14-factory-028-001 — VaultSecrets: FlowFactory for SECURITY alone

FlowFactory.Create("SECURITY"):
  → SecurityAuditFlow63 (SECURITY / 6.3) | TrustState: deterministic_generic
  FlowCount: 1 | TransitionType: feature_addition
  "Enforce secret rotation and configure Vault PKI" → single SecurityAuditFlow63

---

## B14-factory-028-002 — VaultSecrets: FlowFactory for ARCHITECTURE + SECURITY

FlowFactory.CreateMulti(["ARCHITECTURE", "SECURITY"]):
  → InfrastructureArchitectureFlow35 (ARCHITECTURE / 3.5)
  → SecurityAuditFlow63 (SECURITY / 6.3)
  FlowCount: 2 | TransitionType: feature_addition
  Sequential: 3.5 → 6.3 (design secrets topology before enforcing security)

---

## B14-factory-028-003 — VaultSecrets: FlowFactory for IMPLEMENT + SECURITY

FlowFactory.CreateMulti(["IMPLEMENT", "SECURITY"]):
  → BackendServiceFlow41 (IMPLEMENT / 4.1)
  → SecurityAuditFlow63 (SECURITY / 6.3)
  FlowCount: 2 | TransitionType: feature_addition
  Sequential: 6.3 → 4.1 (security design precedes implementation code)

---

## B14-factory-028-004 — VaultSecrets: FlowFactory for AUTHORIZATION + SECURITY

FlowFactory.CreateMulti(["AUTHORIZATION", "SECURITY"]):
  → authorization sub-phase flow (AUTHORIZATION)
  → SecurityAuditFlow63 (SECURITY / 6.3)
  FlowCount: 2 | TransitionType: feature_addition
  "Authorize service accounts to read Vault secrets" → both flows created concurrently
