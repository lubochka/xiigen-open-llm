# Layer 0-B — Router Pairs (044) — InsecureDeserializationAudit Routing
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: router

---

## R044-001 — Insecure deserialization audit → 6.3

R044-001 test: "Audit all deserialization endpoints for insecure deserialization vulnerabilities."
  SECURITY → 6.3 | CompiledPredicateIds: ["SECURITY"] | TrustState: deterministic_generic

---

## R044-002 — Gadget chain → 6.3 (SECURITY+AUTHORIZATION)

R044-002 test: "Scan for deserialization gadget chains leading to privilege escalation."
  SECURITY → 6.3 | AUTHORIZATION → 6.3 | Dual predicates, same sub-phase

---

## R044-003 — "switch to System.Text.Json" → GapHandler

R044-003 test: "Switch from BinaryFormatter to System.Text.Json for all object serialization."
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING

---

## R044-004 — Deserialization + IMPLEMENT → 4.1 + 6.3

R044-004 test: "Implement a type allowlist for deserialization and audit all existing endpoints for gadget chain exposure."
  IMPLEMENT → 4.1 | SECURITY → 6.3 | TransitionType: feature_addition

---

## R044-005 — TypeNameHandling review → 6.3

R044-005 test: "Review JSON deserializers for TypeNameHandling.All exploitation vectors."
  SECURITY → 6.3 | CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3
