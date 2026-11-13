# Layer 0-A — Compiler Pairs (037) — PipelineOrchestration
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-037-001 — "orchestrate" alone is gap

A: "Orchestrate the release pipeline stages."
  "orchestrate" → gap | "pipeline stages" → gap
  Teaching: "orchestrate" alone does not fire any predicate. Pure gap → knowledge_edge.
  CompiledPredicateIds: []

---

## B28-compiler-037-002 — IMPLEMENT+orchestration vocabulary

A: "Build the pipeline orchestration service and QA the deployment order."
  "build" → IMPLEMENT | "service" → IMPLEMENT (dedup) | "orchestration" → gap | "QA" → QA
  Teaching: "service" and "build" fire IMPLEMENT. "orchestration" alone is gap. QA fires on abbreviation.
  CompiledPredicateIds: ["IMPLEMENT", "QA"]

---

## B28-compiler-037-003 — ARCHITECTURE+IMPLEMENT+QA for pipeline orchestrator

A: "Design the pipeline orchestration architecture, implement the stage sequencer, and QA the execution order."
  "design" → ARCHITECTURE | "architecture" → ARCHITECTURE (dedup) | "implement" → IMPLEMENT | "stage sequencer" → gap | "QA" → QA
  Teaching: Design+architecture → ARCHITECTURE. Implement → IMPLEMENT. QA last.
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT", "QA"]

---

## B28-compiler-037-004 — "pipeline" alone is gap

A: "Configure the CI/CD pipeline for the release orchestration."
  "configure" → gap | "CI/CD pipeline" → gap | "release orchestration" → gap
  Teaching: "configure", "CI/CD pipeline", "release orchestration" are all gap vocabulary.
  CompiledPredicateIds: []

---

## B28-compiler-037-005 — TEST+DOCUMENT+QA for pipeline orchestration criteria

A: "Write tests for the pipeline orchestrator, document the stage criteria, and QA sign-off."
  "write tests" → TEST | "document" → DOCUMENT | "criteria" → DOCUMENT (dedup) | "QA sign-off" → QA
  Teaching: TEST via test-writing verb. DOCUMENT via "document" verb and "criteria" noun. QA last.
  CompiledPredicateIds: ["TEST", "DOCUMENT", "QA"]
