# Layer 0-A — Compiler Pairs (023)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-023-001 — QA+IMPLEMENT: hotfix service

A: "Build the emergency hotfix deployment service and QA the patch before go-live."
  "build" → IMPLEMENT | "hotfix deployment service" → IMPLEMENT | "QA" → QA | "go-live" → gap | "patch" → gap
  CompiledPredicateIds: ["QA", "IMPLEMENT"]

---

## B28-compiler-023-002 — QA+TEST+IMPLEMENT: hotfix validation

A: "Implement the hotfix, verify the regression suite passes, and QA sign-off before emergency deployment."
  "implement" → IMPLEMENT | "verify" → TEST | "QA sign-off" → QA | "deployment" → gap
  CompiledPredicateIds: ["QA", "IMPLEMENT", "TEST"]

---

## B28-compiler-023-003 — gap: hotfix deployment instruction

A: "Deploy the hotfix to production immediately and restart the affected pods."
  "deploy" → gap | "production" → gap | "restart pods" → gap | "hotfix" alone → gap
  CompiledPredicateIds: [] → gap

---

## B28-compiler-023-004 — QA alone: emergency hotfix gate

A: "QA the hotfix patch for the authentication service before the emergency cutover."
  "QA" → QA | "hotfix patch" → gap | "emergency cutover" → gap | "authentication service" → gap
  CompiledPredicateIds: ["QA"]

---

## B28-compiler-023-005 — QA+IMPLEMENT+TEST+SECURITY: secure hotfix release

A: "Build the hotfix, write regression tests, validate the security patch, and QA approve the emergency release."
  "build" → IMPLEMENT | "write tests" → TEST | "validate the security patch" → SECURITY + TEST | "QA approve" → QA
  CompiledPredicateIds: ["QA", "IMPLEMENT", "TEST", "SECURITY"]
