# Layer 0-A — Compiler Pairs (005)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: compiler

---

## B28-compiler-005-001 — QA: sprint demo gate

A: "QA the UserService before the sprint demo — ensure all acceptance criteria are met."
  "QA" → QA | "ensure" → gap (alone) | "acceptance criteria" → QA (release gating concept)
  CompiledPredicateIds: ["QA"]

---

## B28-compiler-005-002 — QA+IMPLEMENT: QA harness build

A: "Implement the QA acceptance harness for the OrderService release gate."
  "implement" → IMPLEMENT | "QA acceptance harness" → QA + IMPLEMENT | "release gate" → QA
  CompiledPredicateIds: ["QA", "IMPLEMENT"]

---

## B28-compiler-005-003 — gap: "set up CI/CD"

A: "Set up the CI/CD (Continuous Integration/Continuous Delivery) pipeline for automated deployments."
  "set up" → gap | "CI/CD pipeline" → gap | "automated deployments" → gap
  CompiledPredicateIds: [] → gap
  Teaching: "set up CI/CD" is gap. Neither IMPLEMENT nor QA fires on "set up" or CI/CD vocabulary.

---

## B28-compiler-005-004 — QA+DOCUMENT: release notes + QA sign-off

A: "Write the release notes and get QA sign-off before publishing the v4.0 release."
  "write the release notes" → DOCUMENT | "QA sign-off" → QA
  CompiledPredicateIds: ["QA", "DOCUMENT"]

---

## B28-compiler-005-005 — QA: "QA" in compound noun (QA team, QA engineer)

A: "The QA team needs to validate the OrderService API before release."
  "QA team" → QA (compound noun with QA abbreviation) | "validate" → TEST
  CompiledPredicateIds: ["QA", "TEST"]
  Teaching: "QA team", "QA engineer", "QA department" — compound nouns containing QA abbreviation fire QA.

