# Layer 0-D — Flow Factory Pairs (020)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B14-factory-020-001 — ARCHITECTURE / 3.5 → InfrastructureArchitectureFlow35 (observability stack)

A:
  sub_phase: 3.5
  predicate: ARCHITECTURE
  compiled_text: "Design the observability infrastructure — metrics pipeline, log aggregation layer, and distributed tracing topology."

B:
  TrustState: deterministic_generic
  FlowId: "InfrastructureArchitectureFlow35"
  SelectionReason: "ARCHITECTURE at 3.5 — observability infrastructure = infra design; multiple topology/layer signals"
  NodeCount: 9

---

## B14-factory-020-002 — ARCHITECTURE+IMPLEMENT / 3.5+4.1 → two flows (OTel collector)

A:
  sub_phase: [3.5, 4.1]
  predicate: ["ARCHITECTURE", "IMPLEMENT"]
  compiled_text: "Architect the OTel collector topology and implement the collector pipeline configuration."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "InfrastructureArchitectureFlow35"
      predicate: ARCHITECTURE
      sub_phase: 3.5
    - FlowId: "BackendServiceFlow41"
      predicate: IMPLEMENT
      sub_phase: 4.1
  SelectionReason: "ARCHITECTURE (topology) + IMPLEMENT → feature_addition"

---

## B14-factory-020-003 — ARCHITECTURE+DOCUMENT / 3.5+6.2 → two flows (observability ADR)

A:
  sub_phase: [3.5, 6.2]
  predicate: ["ARCHITECTURE", "DOCUMENT"]
  compiled_text: "Define the observability architecture — Prometheus scrape topology, Jaeger trace sampling strategy — and document the retention policies."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "InfrastructureArchitectureFlow35"
      predicate: ARCHITECTURE
      sub_phase: 3.5
    - FlowId: "DocumentationFlow62"
      predicate: DOCUMENT
      sub_phase: 6.2
  SelectionReason: "ARCHITECTURE (topology, strategy, architecture) + DOCUMENT → documentation TransitionType"

---

## B14-factory-020-004 — IMPLEMENT / 4.1 → BackendServiceFlow41 (configure Prometheus)

A:
  sub_phase: 4.1
  predicate: IMPLEMENT
  compiled_text: "Configure Prometheus to scrape metrics from all pods every 15 seconds."

B:
  TrustState: deterministic_generic
  FlowId: "BackendServiceFlow41"
  SelectionReason: "IMPLEMENT only (configure in concrete config context) → 4.1; Prometheus, scrape metrics → gap but don't degrade"
  NodeCount: 9
