# Layer 0-A — Compiler Pairs (033)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-033-001 — LogAggregation: ARCHITECTURE trigger

A: "Design the log aggregation topology with a centralized pipeline, structured parsing, and retention layers."
  CompiledPredicateIds: ["ARCHITECTURE"]
  ARCHITECTURE: "design" + "topology" + "pipeline" (infra context) + "layers" + "centralized" (structural)
  Gap: "log aggregation" (tool/pattern name alone); fired by architecture vocabulary

---

## B14-compiler-033-002 — LogAggregation: IMPLEMENT trigger

A: "Implement the Fluent Bit parser configuration and write the log routing pipeline manifest."
  CompiledPredicateIds: ["IMPLEMENT"]
  IMPLEMENT: "implement" + "parser configuration" (code artifact) | "write" + "pipeline manifest" (code artifact)
  Gap: "Fluent Bit" (tool name); code artifact nouns fire IMPLEMENT

---

## B14-compiler-033-003 — LogAggregation: SECURITY trigger

A: "Encrypt log streams in transit and enforce TLS on the log ingestion endpoint."
  CompiledPredicateIds: ["SECURITY"]
  SECURITY: "encrypt" (any form) + "TLS" | "in transit" compound fires SECURITY via "encryption in transit"
  Gap: "log streams" (operational term alone), "ingestion endpoint" (runtime term)

---

## B14-compiler-033-004 — LogAggregation: ARCHITECTURE + IMPLEMENT

A: "Architect the centralized logging infrastructure layer and scaffold the Loki stack configuration."
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT"]
  ARCHITECTURE: "architect" + "infrastructure layer" + "centralized" | IMPLEMENT: "scaffold" + "configuration" (code artifact context)
  Gap: "Loki" (tool name); scaffold + configuration fires IMPLEMENT

---

## B14-compiler-033-005 — LogAggregation: DOCUMENT trigger

A: "Document the log aggregation architecture and diagram the ingestion flow topology."
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE"]
  DOCUMENT: "document" + "architecture" | ARCHITECTURE: "diagram" + "topology" + "flow" (infra context)
  TransitionType: documentation (no IMPLEMENT; recording verbs dominate)
