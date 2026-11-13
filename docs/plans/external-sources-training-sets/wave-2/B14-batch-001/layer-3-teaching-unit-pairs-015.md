# Layer 3 — Teaching Unit Pairs (015)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## T015-001 — "vault" fires SECURITY as a tool name

A: "Implement the HashiCorp Vault integration." Does SECURITY fire?
B: Yes. "vault" (the tool name) is in the SECURITY predicate lexicon. "implement" → IMPLEMENT. Result: ["IMPLEMENT", "SECURITY"]. Both fire.

---

## T015-002 — "secret rotation" fires SECURITY

A: "Define the secret rotation policy for the production services." Predicates?
B: ["ARCHITECTURE", "SECURITY"]. "define" → ARCHITECTURE. "secret rotation" → SECURITY ("secret", "key rotation"). Both fire independently.

---

## T015-003 — "rotate the secrets" alone is gap

A: "Rotate the database secrets." Any predicates?
B: Zero predicates. "rotate" alone is operational gap. "database secrets" — "secrets" alone without design/implement context → gap as operational target. Result: [] → knowledge_edge. To fire SECURITY: "implement the secret rotation policy" or "define the secrets management architecture."

---

## T015-004 — "encrypt" fires SECURITY independently

A: "Encrypt the application secrets at rest." Predicates?
B: ["SECURITY"]. "encrypt" → SECURITY. "secrets at rest" → SECURITY ("secret"). No design verb, no ARCHITECTURE. Only SECURITY fires. Route to 6.3.

---

## T015-005 — certificate lifecycle fires SECURITY

A: "Design the certificate lifecycle management topology for the services." Predicates?
B: ["ARCHITECTURE", "SECURITY"]. "design" → ARCHITECTURE. "certificate lifecycle" → SECURITY ("certificate"). "management topology" → ARCHITECTURE ("topology"). Three vocabulary items across two predicates. Routes: 3.5 + 6.3.
