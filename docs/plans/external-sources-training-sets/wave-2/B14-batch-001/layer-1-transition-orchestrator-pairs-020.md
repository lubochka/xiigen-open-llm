# Layer 1 — Transition Orchestrator Pairs (020)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B14-trans-020-001 — ARCHITECTURE / feature_addition (observability stack)

A:
  compiled_text: "Design the observability infrastructure — metrics pipeline, log aggregation layer, and distributed tracing topology."
  compiled_predicate_ids: ["ARCHITECTURE"]
  flow_id: "InfrastructureArchitectureFlow35"

B:
  TransitionType: feature_addition
  TransitionEvidence:
    - "'design' the observability infrastructure — creating a new observability architecture = feature_addition"
    - "Multiple structural vocabulary: infrastructure, pipeline, layer, topology → ARCHITECTURE reinforced"
  SubPhaseId: 3.5
  TransitionConfidence: 0.92

---

## B14-trans-020-002 — ARCHITECTURE+IMPLEMENT / feature_addition (OTel collector)

A:
  compiled_text: "Architect the OTel collector topology and implement the collector pipeline configuration."
  compiled_predicate_ids: ["ARCHITECTURE", "IMPLEMENT"]
  flow_ids: ["InfrastructureArchitectureFlow35", "BackendServiceFlow41"]

B:
  TransitionType: feature_addition
  TransitionEvidence:
    - "'implement' the collector pipeline configuration — IMPLEMENT drives feature_addition"
  SubPhaseId: 3.5
  TransitionConfidence: 0.92

---

## B14-trans-020-003 — ARCHITECTURE+DOCUMENT / documentation (observability ADR)

A:
  compiled_text: "Define the observability architecture and document the retention policies."
  compiled_predicate_ids: ["ARCHITECTURE", "DOCUMENT"]
  flow_ids: ["InfrastructureArchitectureFlow35", "DocumentationFlow62"]

B:
  TransitionType: documentation
  TransitionEvidence:
    - "'document' the retention policies — primary output is documentation = documentation"
    - "No IMPLEMENT"
  SubPhaseId: 3.5
  TransitionConfidence: 0.92

---

## B14-trans-020-004 — IMPLEMENT / feature_addition (configure Prometheus)

A:
  compiled_text: "Configure Prometheus to scrape metrics from all pods every 15 seconds."
  compiled_predicate_ids: ["IMPLEMENT"]
  flow_id: "BackendServiceFlow41"

B:
  TransitionType: feature_addition
  TransitionEvidence:
    - "'configure' in IaC context → IMPLEMENT → feature_addition"
  SubPhaseId: 4.1
  TransitionConfidence: 0.92
