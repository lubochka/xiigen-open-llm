# Layer 0-B — Router Pairs (028)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: router

---

## B14-router-028-001 — VaultSecrets routing: SECURITY is primary

SECURITY routes to 6.3 and is primary sub-phase for Vault requests:
  When ARCHITECTURE also fires: PrimarySubPhaseId = 3.5 (ARCHITECTURE takes primacy in multi-predicate flows when design verb is primary action)
  When only SECURITY fires: PrimarySubPhaseId = 6.3
  "Vault" tool name always produces SECURITY predicate → always routes to 6.3

---

## B14-router-028-002 — VaultSecrets routing: IMPLEMENT concurrent with SECURITY

When IMPLEMENT fires alongside SECURITY for Vault:
  Both 4.1 and 6.3 are active routing targets
  Sequential: 6.3 (SECURITY) before 4.1 (IMPLEMENT) — security design precedes code
  TransitionType: feature_addition
  Example: "write Vault policy files" → IMPLEMENT (4.1) + SECURITY via context (6.3)

---

## B14-router-028-003 — VaultSecrets routing: AUTHORIZATION concurrent

When AUTHORIZATION fires alongside SECURITY:
  AUTHORIZATION routes to its sub-phase concurrently with 6.3
  "Authorize service accounts to read Vault secrets" → AUTHORIZATION + SECURITY (6.3)
  Sequential ordering: SECURITY (6.3) → AUTHORIZATION in ordering sequence

---

## B14-router-028-004 — VaultSecrets routing: never 3.1

Vault secrets vocabulary never routes to 3.1 (System Architecture):
  Vault always belongs to infra-security domain (3.5 + 6.3 combination)
  "Design the secrets management architecture" → 3.5 (infra vocabulary: topology, management, architecture in infra context)
  3.1 (app domain) only when bounded context / aggregate / domain model vocabulary present with no infra terms
