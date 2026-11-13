# Layer 0-A — Compiler Pairs (033) — CodeCoverageGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-033-001 — "CodeCoverageGate" vocabulary

A: "Ensure the CodeCoverageGate threshold of 80% is met before the merge."
  "CodeCoverageGate" → TEST (code coverage = test metric gate) | "merge" → gap
  Teaching: CodeCoverageGate is a test metric gate; fires TEST. "merge" is CI/CD execution vocabulary; gap.
  CompiledPredicateIds: ["TEST"]

---

## B28-compiler-033-002 — "code coverage" + "QA sign-off"

A: "Verify code coverage meets the threshold and QA sign-off on the gate."
  "verify code coverage" → TEST (verify + coverage = test validation) | "QA sign-off" → QA
  Teaching: "verify coverage" fires TEST; "QA sign-off" fires QA.
  CompiledPredicateIds: ["TEST", "QA"]

---

## B28-compiler-033-003 — "write tests" to increase coverage

A: "Write unit tests to increase code coverage and implement the missing payment service logic."
  "write unit tests" → TEST | "implement" → IMPLEMENT | "service" → IMPLEMENT (dedup)
  Teaching: "write tests" fires TEST; "implement" fires IMPLEMENT; "service" fires IMPLEMENT (deduplicated).
  CompiledPredicateIds: ["TEST", "IMPLEMENT"]

---

## B28-compiler-033-004 — "document coverage criteria"

A: "Document the coverage criteria and QA approve the threshold policy."
  "document" → DOCUMENT | "criteria" → DOCUMENT (dedup) | "QA approve" → QA
  Teaching: DOCUMENT fires twice (deduplicated). "QA approve" fires QA.
  CompiledPredicateIds: ["DOCUMENT", "QA"]

---

## B28-compiler-033-005 — Pure gap: "enable coverage reporting"

A: "Enable code coverage reporting in the CI pipeline for the next release."
  "enable" → gap | "CI pipeline" → gap
  Teaching: "enable" and "CI pipeline" are execution configuration vocabulary; pure gap.
  CompiledPredicateIds: [] → gap (knowledge_edge)
