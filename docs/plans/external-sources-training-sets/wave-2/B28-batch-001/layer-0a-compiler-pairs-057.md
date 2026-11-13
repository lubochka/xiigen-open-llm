# Layer 0-A — Compiler Pairs (057) — ObservabilityCheck
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-057-001 — "observability check" fires TEST

A: "Run the observability check gate before promoting to production."
  "observability check gate" → TEST (check gate in release context → TEST)
  Teaching: "check" in gate context fires TEST. Observability context does not change predicate.
  CompiledPredicateIds: ["TEST"]

---

## B28-compiler-057-002 — "metrics", "traces", "logs" alone are gap

A: "Configure metrics collection, trace sampling, and log aggregation."
  "configure metrics collection" → gap | "trace sampling" → gap | "log aggregation" → gap
  Teaching: Observability setup vocabulary is gap. No predicates fire.
  CompiledPredicateIds: []

---

## B28-compiler-057-003 — IMPLEMENT+TEST+QA for observability service

A: "Build the observability check service, write tests for metric collection, and QA the dashboard coverage."
  "build" → IMPLEMENT | "service" → IMPLEMENT (dedup) | "write tests" → TEST | "QA" → QA
  Teaching: IMPLEMENT via build+service. TEST via test-writing. QA last.
  CompiledPredicateIds: ["IMPLEMENT", "TEST", "QA"]

---

## B28-compiler-057-004 — "telemetry" and "dashboards" are gap

A: "Set up telemetry pipelines and configure observability dashboards."
  "set up telemetry pipelines" → gap | "configure observability dashboards" → gap
  Teaching: Telemetry and dashboard setup vocabulary is gap.
  CompiledPredicateIds: []

---

## B28-compiler-057-005 — TEST+DOCUMENT+QA for observability gate with criteria

A: "Execute the observability check gate, document the coverage criteria, and QA the trace completeness."
  "execute the observability check gate" → TEST | "document" → DOCUMENT | "criteria" → DOCUMENT (dedup) | "QA" → QA
  Teaching: Gate execution → TEST. DOCUMENT via verb and noun. QA last.
  CompiledPredicateIds: ["TEST", "DOCUMENT", "QA"]
