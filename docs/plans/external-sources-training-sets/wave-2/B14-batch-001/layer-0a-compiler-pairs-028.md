# Layer 0-A — Compiler Pairs (028)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-028-001 — VaultSecrets: SECURITY trigger (primary)

A: "Enforce secret rotation and configure the Vault PKI (Public Key Infrastructure) engine for certificate issuance."
  CompiledPredicateIds: ["SECURITY"]
  SECURITY: "secret rotation" + "vault" (tool name = SECURITY direct trigger) + "certificate" + "PKI"
  Gap: "configure" (operational); fired by SECURITY vocabulary independently

---

## B14-compiler-028-002 — VaultSecrets: ARCHITECTURE + SECURITY

A: "Design the Vault secrets topology and enforce encryption at rest for all credentials."
  CompiledPredicateIds: ["ARCHITECTURE", "SECURITY"]
  ARCHITECTURE: "design" + "topology" | SECURITY: "vault" + "encryption at rest" + "credentials"
  Gap: "Vault" as tool name contributes to SECURITY (not ARCHITECTURE)

---

## B14-compiler-028-003 — VaultSecrets: IMPLEMENT + SECURITY

A: "Write the Vault policy files and implement the secrets injection sidecar."
  CompiledPredicateIds: ["IMPLEMENT", "SECURITY"]
  IMPLEMENT: "write" + "policy files" (code artifact) | "implement" + "secrets injection sidecar" (code artifact)
  SECURITY: "secrets injection" (compound phrase fires SECURITY) + "vault" context

---

## B14-compiler-028-004 — VaultSecrets: gap boundary

A: "Rotate the Vault token and renew the lease."
  CompiledPredicateIds: ["SECURITY"]
  SECURITY: "vault" (tool name) + "token" (auth context) | Note: "rotate" alone is gap but "vault" + "token" fires SECURITY
  Teaching: "vault" tool name is a direct SECURITY trigger regardless of surrounding operational verbs

---

## B14-compiler-028-005 — VaultSecrets: AUTHORIZATION trigger

A: "Authorize the service accounts to read secrets from the Vault path."
  CompiledPredicateIds: ["AUTHORIZATION", "SECURITY"]
  AUTHORIZATION: "authorize" + "service accounts" (RBAC/IAM roles) | SECURITY: "secrets" + "vault"
  TransitionType: feature_addition (AUTHORIZATION alone → feature_addition)
