# Layer 0-A — Compiler Pairs (020)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-020-001 — ARCHITECTURE: observability stack topology

A: "Design the observability infrastructure — metrics pipeline, log aggregation layer, and distributed tracing topology."
  "design" → ARCHITECTURE | "observability infrastructure" → ARCHITECTURE ("infrastructure") | "metrics pipeline" → ARCHITECTURE ("layer") | "log aggregation layer" → ARCHITECTURE ("layer") | "distributed tracing topology" → ARCHITECTURE ("topology")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Gap: "metrics pipeline", "log aggregation" → operational tool vocabulary, gap

---

## B14-compiler-020-002 — ARCHITECTURE+IMPLEMENT: OTel collector config

A: "Architect the OpenTelemetry (OTel) collector topology and implement the collector pipeline configuration."
  "architect" → ARCHITECTURE | "collector topology" → ARCHITECTURE ("topology") | "implement" → IMPLEMENT | "collector pipeline configuration" → IMPLEMENT
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT"]
  Gap: "OpenTelemetry", "OTel" → tool name, gap

---

## B14-compiler-020-003 — ARCHITECTURE+DOCUMENT: observability ADR

A: "Define the observability architecture — Prometheus scrape topology, Jaeger trace sampling strategy — and document the retention policies."
  "define" → ARCHITECTURE | "observability architecture" → ARCHITECTURE | "Prometheus scrape topology" → ARCHITECTURE ("topology") | "trace sampling strategy" → ARCHITECTURE ("strategy") | "document" → DOCUMENT
  CompiledPredicateIds: ["ARCHITECTURE", "DOCUMENT"]
  Gap: "Prometheus", "Jaeger", "retention policies" → tool/operational vocabulary, gap

---

## B14-compiler-020-004 — ARCHITECTURE: SLO (Service Level Objective) layer

A: "Structure the SLO (Service Level Objective) monitoring layer — error budget tracking, burn rate alerting, and service dependency graph."
  "structure" → ARCHITECTURE | "SLO monitoring layer" → ARCHITECTURE ("layer") | "service dependency graph" → ARCHITECTURE ("dependency")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Gap: "error budget", "burn rate alerting" → operational vocabulary, gap

---

## B14-compiler-020-005 — gap: "add a Prometheus alert"

A: "Add a Prometheus alert rule for CPU utilization above 80%."
  "add" → gap (not in any predicate lexicon as a trigger) | "Prometheus alert rule" → gap | "CPU utilization" → gap
  CompiledPredicateIds: []
  Teaching: "add" as a standalone operational action is gap. To fire IMPLEMENT: "implement the alerting configuration for the observability stack."
