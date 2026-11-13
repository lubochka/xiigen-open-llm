# Layer 0-A — Compiler Pairs (028) — PerformanceGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-028-001 — "PerformanceGate" vocabulary

A: "The release must pass the PerformanceGate before going live."
  "PerformanceGate" → TEST (performance gate = test gate artifact) | "going live" → gap
  Teaching: PerformanceGate is a test gate; "pass" + gate noun fires TEST. "going live" is gap.
  CompiledPredicateIds: ["TEST"]

---

## B28-compiler-028-002 — "load test" + "performance gate" + "QA"

A: "Run load tests, validate the PerformanceGate thresholds, and QA approve."
  "load tests" → TEST | "validate" → TEST (dedup) | "QA approve" → QA
  Teaching: "load tests" and "validate" both fire TEST (deduplicated). "QA approve" fires QA.
  CompiledPredicateIds: ["TEST", "QA"]

---

## B28-compiler-028-003 — "implement" performance threshold + "document criteria"

A: "Implement the performance threshold service and document the gate criteria."
  "implement" → IMPLEMENT | "service" → IMPLEMENT (dedup) | "document" → DOCUMENT | "criteria" → DOCUMENT (dedup)
  Teaching: IMPLEMENT fires twice (deduplicated); DOCUMENT fires twice (deduplicated).
  CompiledPredicateIds: ["IMPLEMENT", "DOCUMENT"]

---

## B28-compiler-028-004 — "design architecture" + performance gate

A: "Design the performance gate architecture and write performance tests."
  "design" → ARCHITECTURE | "architecture" → ARCHITECTURE (dedup) | "write performance tests" → TEST
  Teaching: "design" fires ARCHITECTURE; "architecture" fires ARCHITECTURE (deduplicated). "write tests" fires TEST.
  CompiledPredicateIds: ["ARCHITECTURE", "TEST"]

---

## B28-compiler-028-005 — Pure gap: "monitor performance after release"

A: "Monitor performance metrics after the release deployment."
  "monitor" → gap | "deployment" → gap
  Teaching: "monitor" is operational vocabulary; alone it is gap. "deployment" is gap.
  CompiledPredicateIds: [] → gap (knowledge_edge)
