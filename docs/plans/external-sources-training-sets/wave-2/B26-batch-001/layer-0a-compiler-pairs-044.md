# Layer 0-A — Compiler Pairs (044) — InsecureDeserializationAudit
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: compiler

---

## C044-001 — Insecure deserialization audit → SECURITY

A: "Audit all deserialization endpoints for insecure deserialization vulnerabilities enabling RCE (Remote Code Execution)."
  "audit" → SECURITY | "insecure deserialization" → SECURITY (attack vector) | "vulnerabilities" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3 | TrustState: deterministic_generic

---

## C044-002 — Newtonsoft.Json TypeNameHandling → SECURITY

A: "Review all JSON (JavaScript Object Notation) deserializers for TypeNameHandling.All exploitation vectors."
  "review" → SECURITY | "exploitation vectors" → SECURITY (attack-vector vocabulary)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C044-003 — Gadget chain audit → SECURITY

A: "Scan the application for deserialization gadget chains that could lead to privilege escalation."
  "scan" → SECURITY | "privilege escalation" → SECURITY+AUTHORIZATION (dual)
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"] | SubPhase: 6.3

---

## C044-004 — "switch to JSON deserializer" → gap

A: "Switch from BinaryFormatter to System.Text.Json for all object serialization."
  "switch" → gap | "serialization" → gap (migration task, no vulnerability/audit vocabulary)
  CompiledPredicateIds: [] → knowledge_edge

---

## C044-005 — Deserialization + IMPLEMENT+SECURITY → feature_addition

A: "Implement a type allowlist for deserialization and audit all existing endpoints for gadget chain exposure."
  "implement" → IMPLEMENT | "audit" → SECURITY | "gadget chain" = vulnerability context → SECURITY
  CompiledPredicateIds: ["IMPLEMENT", "SECURITY"] | TransitionType: feature_addition
