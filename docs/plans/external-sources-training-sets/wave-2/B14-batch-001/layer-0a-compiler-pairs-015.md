# Layer 0-A — Compiler Pairs (015)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-015-001 — ARCHITECTURE+SECURITY: secrets topology

A: "Design the secrets management topology — Vault cluster, dynamic secret rotation, and certificate lifecycle."
  "design" → ARCHITECTURE | "secrets management topology" → ARCHITECTURE ("topology") + SECURITY ("secret") | "certificate lifecycle" → SECURITY ("certificate")
  CompiledPredicateIds: ["ARCHITECTURE", "SECURITY"]
  Gap: "Vault cluster", "dynamic secret rotation" → tool/operational vocabulary, gap

---

## B14-compiler-015-002 — SECURITY+IMPLEMENT: secret rotation policy

A: "Implement the secret rotation policy and key rotation schedule for the production credential store."
  "implement" → IMPLEMENT | "secret rotation policy" → SECURITY ("secret", "key rotation") | "credential store" → SECURITY ("credential")
  CompiledPredicateIds: ["IMPLEMENT", "SECURITY"]
  Gap: "production credential store" → operational vocabulary, gap

---

## B14-compiler-015-003 — ARCHITECTURE+SECURITY: AWS Secrets Manager integration

A: "Architect the AWS Secrets Manager integration topology — secret injection, IAM (Identity and Access Management) role binding, and rotation triggers."
  "architect" → ARCHITECTURE | "integration topology" → ARCHITECTURE ("topology") | "secret injection" → SECURITY ("secret") | "IAM role binding" → SECURITY ("credential")
  CompiledPredicateIds: ["ARCHITECTURE", "SECURITY"]
  Gap: "AWS Secrets Manager", "IAM", "rotation triggers" → platform/tool vocabulary, gap

---

## B14-compiler-015-004 — SECURITY: encryption at rest

A: "Define the encryption strategy for secrets at rest — key hierarchy, envelope encryption, and audit logging for key access."
  "define" → ARCHITECTURE | "encryption strategy" → SECURITY ("encrypt") | "key hierarchy" → SECURITY ("key rotation") | "audit logging" → SECURITY (access audit context)
  CompiledPredicateIds: ["ARCHITECTURE", "SECURITY"]
  Gap: "envelope encryption" → cryptographic vocabulary, gap

---

## B14-compiler-015-005 — gap: "rotate the secrets"

A: "Rotate the database credentials in AWS Secrets Manager."
  "rotate" → gap (operational action not in SECURITY predicate lexicon as a trigger; "key rotation" fires SECURITY but "rotate" alone does not) | "database credentials" → SECURITY? ("credential")
  Boundary: "credential" in isolation without design/implement context → gap; "rotate the credentials" is a DevOps operation, not a design or implementation task.
  CompiledPredicateIds: []
  Teaching: "rotate" alone is operational gap. SECURITY fires on: secret, certificate, tls, ssl, encrypt, credential in a design/implement framing.
