# Layer 0-C — Knowledge Edge Pairs (020)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-020-001 — "OTel" fires ARCHITECTURE via "topology" context

A: "Design the OTel (OpenTelemetry) collector topology — receiver pipelines, processor chains, and exporter routing."
  "design" → ARCHITECTURE | "OTel collector topology" → ARCHITECTURE ("topology") | "processor chains" → ARCHITECTURE ("layer") | "exporter routing" → ARCHITECTURE
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "OTel" → gap (tool abbreviation). "collector topology" → ARCHITECTURE. Gap vocabulary does not suppress predicates fired on other terms.

---

## B14-edge-020-002 — "Prometheus" alone does not fire ARCHITECTURE

A: "Configure Prometheus to scrape metrics from all pods every 15 seconds."
  "configure" → IMPLEMENT (configuration context) | "Prometheus" → gap (tool name) | "scrape metrics" → gap
  CompiledPredicateIds: ["IMPLEMENT"]
  Teaching: "configure" in concrete configuration context fires IMPLEMENT. "Prometheus" is gap but "configure" fires IMPLEMENT independently.

---

## B14-edge-020-003 — "distributed tracing" architecture fires ARCHITECTURE

A: "Architect the distributed tracing topology — trace propagation headers, sampling strategy, and Jaeger backend topology."
  "architect" → ARCHITECTURE | "distributed tracing topology" → ARCHITECTURE ("topology", "distributed") | "sampling strategy" → ARCHITECTURE ("strategy") | "backend topology" → ARCHITECTURE ("topology")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "distributed" is an ARCHITECTURE trigger noun. "topology" reinforces it multiple times.

---

## B14-edge-020-004 — "SLO" monitoring layer fires ARCHITECTURE

A: "Structure the SLO (Service Level Objective) monitoring layer with error budget tracking and burn rate alerting."
  "structure" → ARCHITECTURE | "SLO monitoring layer" → ARCHITECTURE ("layer") | "error budget tracking" → ARCHITECTURE (monitoring design)
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "structure" + "monitoring layer" → ARCHITECTURE. "SLO", "burn rate" → gap (operational metric vocabulary).
