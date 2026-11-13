# Layer 0-C — Knowledge Edge Pairs (015)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-015-001 — "vault" (tool name) fires SECURITY

A: "Implement the HashiCorp Vault integration for dynamic secret generation."
  "implement" → IMPLEMENT | "Vault" → SECURITY (tool name: "vault" is in SECURITY lexicon as tool name) | "secret generation" → SECURITY ("secret")
  CompiledPredicateIds: ["IMPLEMENT", "SECURITY"]
  Teaching: "vault" as a tool name fires SECURITY directly. "secret" fires SECURITY. "implement" fires IMPLEMENT.

---

## B14-edge-015-002 — "credential" fires SECURITY in configuration context

A: "Configure the service account credentials and certificate bindings for the secrets pipeline."
  "configure" → IMPLEMENT (in concrete coding/IaC context) | "credentials" → SECURITY ("credential") | "certificate bindings" → SECURITY ("certificate") | "secrets pipeline" → SECURITY ("secret")
  CompiledPredicateIds: ["IMPLEMENT", "SECURITY"]
  Teaching: "credential", "certificate", "secret" all independently fire SECURITY.

---

## B14-edge-015-003 — "key" alone does not fire SECURITY

A: "Store the API key in the configuration file."
  "store" → gap | "API key" → SECURITY? — boundary: "API key" as a credential fires SECURITY ("key" in credential context)
  Boundary refinement: "API key" = a secret credential → SECURITY fires on "key" in credential context.
  "store" → gap (not an IMPLEMENT action verb in this context).
  CompiledPredicateIds: ["SECURITY"]
  Teaching: "API key" = credential → SECURITY. But "key" as a map/dictionary key (programming) does not fire SECURITY.

---

## B14-edge-015-004 — "encrypt" fires SECURITY independently

A: "Encrypt the secrets at rest in the S3 bucket using KMS (Key Management Service)."
  "encrypt" → SECURITY | "secrets at rest" → SECURITY ("secret") | "KMS" → gap (tool name) | "S3 bucket" → gap
  CompiledPredicateIds: ["SECURITY"]
  Teaching: "encrypt" fires SECURITY without any design/implement framing. SECURITY vocabulary fires independently.
