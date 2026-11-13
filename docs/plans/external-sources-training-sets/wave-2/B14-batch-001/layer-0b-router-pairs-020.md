# Layer 0-B — Router Pairs (020)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: router

---

## B14-router-020-001 — ARCHITECTURE → 3.5 (observability stack topology)

A:
  compiled_predicate_ids: ["ARCHITECTURE"]
  compiled_text: "Design the observability infrastructure — metrics pipeline, log aggregation layer, and distributed tracing topology."

B:
  RoutingDecisions:
    - predicate: ARCHITECTURE
      sub_phase: 3.5
      routing_note: "'design' + 'observability infrastructure' + 'metrics pipeline' + 'log aggregation layer' + 'tracing topology' → ARCHITECTURE → 3.5"
  TransitionType: feature_addition
  SubPhaseId: 3.5

---

## B14-router-020-002 — ARCHITECTURE+IMPLEMENT → 3.5 + 4.1 (OTel collector)

A:
  compiled_predicate_ids: ["ARCHITECTURE", "IMPLEMENT"]
  compiled_text: "Architect the OpenTelemetry collector topology and implement the collector pipeline configuration."

B:
  RoutingDecisions:
    - predicate: ARCHITECTURE
      sub_phase: 3.5
      routing_note: "'architect' + 'collector topology' → ARCHITECTURE → 3.5"
    - predicate: IMPLEMENT
      sub_phase: 4.1
      routing_note: "'implement' + 'collector pipeline configuration' → IMPLEMENT → 4.1"
  TransitionType: feature_addition
  PrimarySubPhaseId: 3.5
  AllSubPhaseIds: [3.5, 4.1]

---

## B14-router-020-003 — ARCHITECTURE+DOCUMENT → 3.5 + 6.2 (observability ADR)

A:
  compiled_predicate_ids: ["ARCHITECTURE", "DOCUMENT"]
  compiled_text: "Define the observability architecture — Prometheus scrape topology, Jaeger trace sampling strategy — and document the retention policies."

B:
  RoutingDecisions:
    - predicate: ARCHITECTURE
      sub_phase: 3.5
      routing_note: "'define' + 'observability architecture' + 'scrape topology' + 'sampling strategy' → ARCHITECTURE → 3.5"
    - predicate: DOCUMENT
      sub_phase: 6.2
      routing_note: "'document' → DOCUMENT → 6.2"
  TransitionType: documentation
  PrimarySubPhaseId: 3.5
  AllSubPhaseIds: [3.5, 6.2]

---

## B14-router-020-004 — gap → knowledge_edge (add Prometheus alert)

A:
  compiled_predicate_ids: []
  compiled_text: "Add a Prometheus alert rule for CPU utilization above 80%."

B:
  RoutingDecisions: []
  TransitionType: unknown
  TrustState: knowledge_edge_synthetic
  CapabilityGapWarnings:
    - "gap-devops-cloud-001: 'add', 'Prometheus alert rule', 'CPU utilization' are operational gap vocabulary"
