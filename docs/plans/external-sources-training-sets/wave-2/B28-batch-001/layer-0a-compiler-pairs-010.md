# Layer 0-A — Compiler Pairs (010)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: compiler

---

## B28-compiler-010-001 — QA: "QA environment" fires QA

A: "Set up the QA environment for the sprint release testing."
  "QA environment" → QA (compound noun with QA abbreviation) | "set up" → gap (alone)
  CompiledPredicateIds: ["QA"]
  Teaching: "QA environment" fires QA via compound noun. Even though "set up" is gap, QA fires on the compound noun.

---

## B28-compiler-010-002 — QA+TEST: acceptance test + QA gate

A: "Write acceptance tests for the UserManagement module and QA sign-off before deployment."
  "write acceptance tests" → TEST | "QA sign-off" → QA | "deployment" → gap
  CompiledPredicateIds: ["QA", "TEST"]

---

## B28-compiler-010-003 — gap: "continuous deployment"

A: "Enable continuous deployment for the staging pipeline."
  "enable continuous deployment" → gap | "staging pipeline" → gap
  CompiledPredicateIds: [] → gap
  Teaching: "continuous deployment" and "CD" (as pipeline concept) are gap. Not QA, not IMPLEMENT.

---

## B28-compiler-010-004 — QA+IMPLEMENT+TEST+DOCUMENT: full release prep

A: "Implement automated tests, write release notes, run QA sign-off, and document the test results."
  "implement automated tests" → IMPLEMENT + TEST | "write release notes" → DOCUMENT | "QA sign-off" → QA | "document the test results" → DOCUMENT
  CompiledPredicateIds: ["QA", "IMPLEMENT", "TEST", "DOCUMENT"]

---

## B28-compiler-010-005 — QA: "QA pass" fires QA

A: "The OrderService must achieve a QA pass before merging to main."
  "QA pass" → QA | "merging to main" → gap
  CompiledPredicateIds: ["QA"]

