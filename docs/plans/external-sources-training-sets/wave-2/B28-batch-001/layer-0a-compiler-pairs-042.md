# Layer 0-A — Compiler Pairs (042) — RollbackTrigger
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-042-001 — "rollback" alone is gap

A: "Trigger a rollback if the health check fails."
  "trigger" → gap | "rollback" → gap | "health check fails" → gap
  Teaching: "rollback", "trigger", and "health check" are all gap vocabulary.
  CompiledPredicateIds: []

---

## B28-compiler-042-002 — IMPLEMENT+TEST for rollback trigger service

A: "Implement the rollback trigger service and write tests for the failure detection logic."
  "implement" → IMPLEMENT | "service" → IMPLEMENT (dedup) | "write tests" → TEST
  Teaching: Service noun + implement verb → IMPLEMENT. Test-writing verb → TEST.
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]

---

## B28-compiler-042-003 — SECURITY+QA for rollback authorization

A: "Secure the rollback trigger endpoint against unauthorized invocation and QA the permission model."
  "secure" → SECURITY | "QA" → QA
  Teaching: "secure" verb fires SECURITY. "unauthorized invocation" is context, not predicate. QA last.
  CompiledPredicateIds: ["SECURITY", "QA"]

---

## B28-compiler-042-004 — "rollback threshold" and "rollback policy" are gap

A: "Set the rollback threshold and configure the rollback policy."
  "set" → gap | "rollback threshold" → gap | "configure" → gap | "rollback policy" → gap
  Teaching: All rollback configuration vocabulary is gap.
  CompiledPredicateIds: []

---

## B28-compiler-042-005 — IMPLEMENT+DOCUMENT+QA for rollback trigger with criteria

A: "Build the rollback trigger service, document the trigger criteria, and QA the recovery path."
  "build" → IMPLEMENT | "service" → IMPLEMENT (dedup) | "document" → DOCUMENT | "criteria" → DOCUMENT (dedup) | "QA" → QA
  Teaching: Three predicates. QA validates after IMPLEMENT and DOCUMENT.
  CompiledPredicateIds: ["IMPLEMENT", "DOCUMENT", "QA"]
