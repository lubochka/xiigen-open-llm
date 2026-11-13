# Layer 0-C — Knowledge Edge Pairs (028)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-028-001 — VaultSecrets: "vault" tool name fires SECURITY directly

SECURITY fires from "vault" tool name in any context:
  "vault" alone in sentence → SECURITY fires
  "Rotate the Vault token and renew the lease" → SECURITY fires from "vault" + "token" (auth context)
  Teaching: "vault" is a direct SECURITY trigger regardless of surrounding operational verbs. It is listed in SECURITY lexicon as a tool name trigger.

---

## B14-edge-028-002 — VaultSecrets: "rotate" alone is gap but "vault" + "rotate" fires SECURITY

Gap: "rotate the API keys" alone (without vault or certificate context) — "rotate" alone is operational
SECURITY: "rotate the Vault credentials" — "vault" fires SECURITY; "rotate" adds context
SECURITY: "credential rotation for the Vault PKI engine" — "credential rotation" compound fires SECURITY
Teaching: "rotate" alone is gap. With "vault", "certificate", or "credentials" it becomes SECURITY context.

---

## B14-edge-028-003 — VaultSecrets: AUTHORIZATION vs SECURITY for IAM Vault policies

AUTHORIZATION fires: "authorize the service accounts to read Vault secrets" — "authorize" + "service accounts" = AUTHORIZATION
SECURITY fires: "enforce IAM permission boundaries on Vault paths" — "IAM" (security binding) + boundary enforcement
Both can fire: "authorize service accounts (AUTHORIZATION) and enforce TLS (SECURITY) on Vault endpoints"
Teaching: AUTHORIZATION = role/policy binding; SECURITY = encryption/credential/key/TLS enforcement. Both can fire from the same Vault request.

---

## B14-edge-028-004 — VaultSecrets: "secrets injection" fires SECURITY

SECURITY fires: "secrets injection" compound phrase is listed in SECURITY vocabulary
"implement the secrets injection sidecar" → IMPLEMENT fires from "implement" + code artifact; SECURITY fires from "secrets injection"
Both IMPLEMENT and SECURITY fire from same sentence — no suppression
Teaching: Code artifact creation and security vocabulary in same sentence → both predicates fire independently.
