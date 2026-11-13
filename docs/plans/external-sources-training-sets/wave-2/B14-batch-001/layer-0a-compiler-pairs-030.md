# Layer 0-A — Compiler Pairs (030)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-030-001 — PrometheusStack: ARCHITECTURE trigger

A: "Design the Prometheus monitoring stack topology with federation and long-term storage."
  CompiledPredicateIds: ["ARCHITECTURE"]
  ARCHITECTURE: "design" + "stack" (structural) + "topology" + "storage" (infra context)
  Gap: "Prometheus" (tool name), "federation" (operational term alone)

---

## B14-compiler-030-002 — PrometheusStack: IMPLEMENT trigger

A: "Implement the Prometheus scrape configuration and write the alerting rule files."
  CompiledPredicateIds: ["IMPLEMENT"]
  IMPLEMENT: "implement" + "scrape configuration" (code artifact — configuration file) | "write" + "alerting rule files" (code artifact)
  Gap: "Prometheus" (tool name); fired by implement/write + code artifact nouns

---

## B14-compiler-030-003 — PrometheusStack: ARCHITECTURE + IMPLEMENT

A: "Architect the observability infrastructure layer and scaffold the Prometheus operator manifests."
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT"]
  ARCHITECTURE: "architect" + "infrastructure layer" + "observability" | IMPLEMENT: "scaffold" + "manifests" (code artifact)
  Gap: "Prometheus operator" (tool); scaffolding manifests fires IMPLEMENT

---

## B14-compiler-030-004 — PrometheusStack: gap vocabulary

A: "Start the Prometheus server and check the targets."
  CompiledPredicateIds: []
  Gap: "start" (operational), "check" (operational), "Prometheus" (tool), "targets" (runtime term)
  TrustState: knowledge_edge_synthetic

---

## B14-compiler-030-005 — PrometheusStack: DOCUMENT trigger

A: "Document the Prometheus scraping strategy and diagram the metrics collection topology."
  CompiledPredicateIds: ["ARCHITECTURE", "DOCUMENT"]
  ARCHITECTURE: "diagram" (documentary artifact but also structural) + "topology" | DOCUMENT: "document" + "strategy"
  TransitionType: documentation (ARCHITECTURE + DOCUMENT, no IMPLEMENT; "document" and "diagram" are recording verbs)
