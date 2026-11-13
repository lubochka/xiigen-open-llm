# Layer 0-A — Compiler Pairs (044) — EnvironmentPromotion
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-044-001 — "promote" and "promotion" are gap

A: "Promote the artifact from staging to production."
  "promote" → gap | "staging to production" → gap
  Teaching: Environment promotion vocabulary is gap. No predicates fire.
  CompiledPredicateIds: []

---

## B28-compiler-044-002 — IMPLEMENT+QA for promotion service

A: "Build the environment promotion service and QA the promotion pipeline."
  "build" → IMPLEMENT | "service" → IMPLEMENT (dedup) | "QA" → QA
  Teaching: Build+service → IMPLEMENT. QA fires on abbreviation.
  CompiledPredicateIds: ["IMPLEMENT", "QA"]

---

## B28-compiler-044-003 — "dev to staging to production" is gap

A: "Move the release through dev to staging to production environments."
  "move" → gap | "dev to staging to production" → gap | "environments" → gap
  Teaching: Full environment promotion chain vocabulary is gap.
  CompiledPredicateIds: []

---

## B28-compiler-044-004 — DOCUMENT+QA for promotion criteria

A: "Document the environment promotion criteria and QA the promotion checklist."
  "document" → DOCUMENT | "criteria" → DOCUMENT (dedup) | "QA" → QA
  Teaching: DOCUMENT via verb and noun. QA last.
  CompiledPredicateIds: ["DOCUMENT", "QA"]

---

## B28-compiler-044-005 — ARCHITECTURE+IMPLEMENT+TEST+QA for promotion framework

A: "Design the environment promotion architecture, implement the promotion controller service, write tests for each promotion stage, and QA approve."
  "design" → ARCHITECTURE | "architecture" → ARCHITECTURE (dedup) | "implement" → IMPLEMENT | "service" → IMPLEMENT (dedup) | "write tests" → TEST | "QA approve" → QA
  Teaching: Four-predicate set. IMPLEMENT drives feature_addition. QA last.
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT", "TEST", "QA"]
