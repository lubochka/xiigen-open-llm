# Layer 0-A — Compiler Pairs (039) — ConditionalStageGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-039-001 — "conditional" alone is gap

A: "Add a conditional stage gate to the pipeline."
  "add" → gap (no test-writing context) | "conditional" → gap | "stage gate" → gap
  Teaching: "add" without test-writing context is gap. "conditional stage gate" is pipeline vocabulary → gap.
  CompiledPredicateIds: []

---

## B28-compiler-039-002 — IMPLEMENT+TEST+QA for conditional gate service

A: "Build the conditional gate evaluator service, write tests for each condition branch, and QA the gate logic."
  "build" → IMPLEMENT | "service" → IMPLEMENT (dedup) | "write tests" → TEST | "QA" → QA
  Teaching: Build+service → IMPLEMENT. Test-writing verb → TEST. QA last.
  CompiledPredicateIds: ["IMPLEMENT", "TEST", "QA"]

---

## B28-compiler-039-003 — SECURITY+QA for conditional security gate

A: "Secure the conditional gate endpoint and QA the access control rules."
  "secure" → SECURITY | "QA" → QA
  Teaching: "secure" verb fires SECURITY. QA fires on abbreviation. Conditional context does not change predicate.
  CompiledPredicateIds: ["SECURITY", "QA"]

---

## B28-compiler-039-004 — "if-branch" and "else-branch" are gap

A: "Configure the if-branch gate and else-branch fallback."
  "configure" → gap | "if-branch" → gap | "else-branch fallback" → gap
  Teaching: Branch/conditional flow control vocabulary is gap.
  CompiledPredicateIds: []

---

## B28-compiler-039-005 — ARCHITECTURE+IMPLEMENT+DOCUMENT+QA for conditional gate design

A: "Design the conditional stage gate architecture, implement the evaluator, document the gate criteria, and QA sign-off."
  "design" → ARCHITECTURE | "architecture" → ARCHITECTURE (dedup) | "implement" → IMPLEMENT | "document" → DOCUMENT | "criteria" → DOCUMENT (dedup) | "QA sign-off" → QA
  Teaching: Full four-predicate set. QA last.
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT", "DOCUMENT", "QA"]
