# Layer 0-C — Knowledge-Edge Pairs (059)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: HashiCorp Vault connector

---

## B17-0C-059-001 — HashiCorpVault: non-trigger terms

**Proper nouns — no predicate:** HashiCorp Vault, Vault, HashiCorp
**Domain nouns — no predicate:** secret, lease, path, engine, policy, namespace, audit log, seal, unseal, transit, PKI (Public Key Infrastructure)
**Acronyms — no predicate:** PKI (Public Key Infrastructure), KV (Key-Value engine)

---

## B17-0C-059-002 — HashiCorpVault: "retrieve secrets" is not IMPLEMENT

"Retrieve secrets from Vault" → no predicate.
"Inject Vault secrets into the application" → no predicate.
"Rotate Vault secret leases" → no predicate.
"Build the Vault connector to retrieve and inject secrets" → IMPLEMENT from "Build"; "connector" fires API_CONTRACT.

---

## B17-0C-059-003 — HashiCorpVault: Vault token is the canonical SECURITY trigger

HashiCorp Vault uses "Vault token" as its primary auth mechanism. "Vault token" → SECURITY fires (Rule 6).
"AppRole secret ID" alone → no SECURITY (no "token").
"AppRole secret ID token" → SECURITY fires.

---

## B17-0C-059-004 — HashiCorpVault: "authenticate" fires SECURITY even without "token"

"Build the Vault connector and authenticate using the AppRole secret ID" → SECURITY fires from "authenticate" (Rule 8).
This is distinct from token-based SECURITY: "authenticate" alone is sufficient.
Both "token" and "authenticate/authentication" independently fire SECURITY (Rule 6 and Rule 8 respectively).
