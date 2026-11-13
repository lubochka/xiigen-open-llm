# Layer 0-A — Compiler Pairs (027) — RegressionGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-027-001 — "RegressionGate" vocabulary

A: "Pass the RegressionGate before promoting to staging."
  "RegressionGate" → TEST (regression = test artifact + gate) | "promoting" → gap
  Teaching: RegressionGate is a test gate artifact. "promoting" alone is gap (pipeline execution vocabulary).
  CompiledPredicateIds: ["TEST"]

---

## B28-compiler-027-002 — "regression test" + "QA sign-off"

A: "Run regression tests on the OrderService and QA sign-off."
  "regression tests" → TEST (run + test noun) | "QA sign-off" → QA (abbreviation compound)
  Teaching: "regression tests" fires TEST; "QA sign-off" fires QA.
  CompiledPredicateIds: ["TEST", "QA"]

---

## B28-compiler-027-003 — "write regression tests" + "implement fix"

A: "Write regression tests for the bug and implement the fix in the payment service."
  "write regression tests" → TEST (write + test noun) | "implement" → IMPLEMENT | "service" → IMPLEMENT (dedup)
  Teaching: "write tests" fires TEST; "implement" fires IMPLEMENT; "service" fires IMPLEMENT (deduplicated).
  CompiledPredicateIds: ["TEST", "IMPLEMENT"]

---

## B28-compiler-027-004 — "document regression criteria"

A: "Document the regression criteria and QA the gate before release."
  "document" → DOCUMENT | "criteria" → DOCUMENT (dedup) | "QA" → QA
  Teaching: Two DOCUMENT triggers deduplicate to one. QA fires on abbreviation.
  CompiledPredicateIds: ["DOCUMENT", "QA"]

---

## B28-compiler-027-005 — Pure gap: "trigger regression gate"

A: "Trigger the RegressionGate pipeline run automatically."
  "trigger" → gap | "automatically" → gap
  Teaching: "trigger" alone and "automatically" are execution vocabulary; pure gap.
  CompiledPredicateIds: [] → gap (knowledge_edge)
