# Layer 0-A — Compiler Pairs (038) — ParallelStageExecution
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-038-001 — "parallel" alone is gap

A: "Run the stages in parallel for faster delivery."
  "run" → gap | "parallel" → gap | "faster delivery" → gap
  Teaching: "parallel" execution vocabulary is gap. No predicates fire.
  CompiledPredicateIds: []

---

## B28-compiler-038-002 — IMPLEMENT+TEST+QA for parallel stage runner

A: "Implement the parallel stage runner, add tests for concurrent execution, and QA the results."
  "implement" → IMPLEMENT | "add tests" → TEST | "concurrent execution" → gap | "QA" → QA
  Teaching: "implement" → IMPLEMENT. "add tests" → TEST. QA fires on abbreviation.
  CompiledPredicateIds: ["IMPLEMENT", "TEST", "QA"]

---

## B28-compiler-038-003 — "concurrent" and "parallel" both gap

A: "Enable concurrent stage execution and parallel test runs."
  "enable" → gap | "concurrent" → gap | "parallel" → gap | "stage execution" → gap
  Teaching: Both "concurrent" and "parallel" execution vocabulary are pure gap.
  CompiledPredicateIds: []

---

## B28-compiler-038-004 — ARCHITECTURE for parallel stage design

A: "Design the parallel execution architecture for the release pipeline stages."
  "design" → ARCHITECTURE | "architecture" → ARCHITECTURE (dedup) | "parallel execution" → gap | "pipeline stages" → gap
  Teaching: Design verb + architecture noun → ARCHITECTURE. Parallelism context remains gap.
  CompiledPredicateIds: ["ARCHITECTURE"]

---

## B28-compiler-038-005 — DOCUMENT+QA for parallel execution criteria

A: "Document the parallel stage execution criteria and QA the ordering rules."
  "document" → DOCUMENT | "criteria" → DOCUMENT (dedup) | "QA" → QA
  Teaching: DOCUMENT via verb and noun. QA absolute last.
  CompiledPredicateIds: ["DOCUMENT", "QA"]
