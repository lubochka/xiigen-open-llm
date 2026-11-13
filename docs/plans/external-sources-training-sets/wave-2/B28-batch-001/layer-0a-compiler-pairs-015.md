# Layer 0-A — Compiler Pairs (015)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-015-001 — QA+IMPLEMENT: rollback automation

A: "Build the automated rollback service and QA the trigger conditions before release."
  "build" → IMPLEMENT | "automated rollback service" → IMPLEMENT | "QA" → QA | "trigger conditions" → gap
  CompiledPredicateIds: ["QA", "IMPLEMENT"]

---

## B28-compiler-015-002 — QA+TEST+IMPLEMENT: rollback procedure validation

A: "Implement rollback triggers, verify that rollback restores the previous state, and QA sign-off."
  "implement" → IMPLEMENT | "verify" → TEST | "QA sign-off" → QA | "previous state" → gap
  CompiledPredicateIds: ["QA", "IMPLEMENT", "TEST"]

---

## B28-compiler-015-003 — gap: rollback execution command

A: "Roll back the OrderService deployment to v2.1 and restart the pods."
  "roll back" → gap (rollback execution) | "restart pods" → gap | "deployment" → gap
  CompiledPredicateIds: [] → gap

---

## B28-compiler-015-004 — QA+DOCUMENT: rollback runbook documentation

A: "Document the rollback procedure and QA review the runbook before go-live."
  "document" → DOCUMENT | "rollback procedure" → DOCUMENT + gap | "QA review" → QA | "go-live" → gap
  CompiledPredicateIds: ["QA", "DOCUMENT"]

---

## B28-compiler-015-005 — QA+IMPLEMENT+TEST+DOCUMENT: rollback lifecycle

A: "Build rollback automation, write tests for the revert logic, document the cutover runbook, and QA gate the procedure."
  "build" → IMPLEMENT | "write tests" → TEST | "document" → DOCUMENT | "QA gate" → QA | "cutover" → gap
  CompiledPredicateIds: ["QA", "IMPLEMENT", "TEST", "DOCUMENT"]
