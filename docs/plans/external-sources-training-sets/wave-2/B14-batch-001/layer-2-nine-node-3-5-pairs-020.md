# Layer 2 — Nine-Node Flow Pairs (020)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: nine_node_flow (InfrastructureArchitectureFlow35)

---

## B14-nine-020 — ObservabilityStack — ARCHITECTURE+IMPLEMENT

Request: "Design the observability infrastructure topology and implement the OTel (OpenTelemetry) collector pipeline and Prometheus scrape configuration."

### Flow: InfrastructureArchitectureFlow35 + BackendServiceFlow41

**Node 1 — DomainSplitter**
  SelectedLogicalTransition: ARCHITECTURE → 3.5 + IMPLEMENT → 4.1
  Domains: metrics_topology, logging_topology, tracing_topology, alerting_layer

**Node 2 — DomainTranslator**
  metrics_topology: Prometheus scrape → Thanos (long-term storage); 15-second scrape interval; metric cardinality budget
  logging_topology: Fluentd (log collector) → Loki; label-only indexing; 30-day retention
  tracing_topology: OTel SDK (Software Development Kit) → OTel Collector → Jaeger; 5% trace sampling in production
  alerting_layer: AlertManager; PagerDuty integration; severity: critical/warning/info; deduplication window 5 minutes

**Node 3 — SchemePlanner**
  Pattern: OTel vendor-neutral collection layer; Prometheus for metrics; Loki for logs; Jaeger for traces
  Decision: 5% trace sampling in production — reduces storage cost; critical traces always sampled (head-based + tail-based)
  Decision: Loki label-only indexing — no full-text index; lower cost; query via LogQL

**Node 4 — DomainProcessor**
  Decision: OTel Collector as centralized pipeline
  Rationale: Vendor-neutral; single egress point; can fan-out to multiple backends
  Trade-off: OTel Collector is a single point of failure (SPOF); must run as DaemonSet (one per node)
  Decision: 5% production trace sampling
  Rationale: Full tracing too expensive at scale; 5% sufficient for p99 latency analysis
  Trade-off: Low-frequency bugs may not be captured; tail-based sampling for error traces mitigates

**Node 5 — Applier**
  Constraints: OTel SDK required in all services; correlation ID propagated as trace context; error traces always sampled (100%)

**Node 6 — ResultTranslator**
  Output: observability-topology-adr.md + OTel collector config + Prometheus scrape config

**Node 7 — ArbiterSelector**
  SelectedArbiter: ArchitectureCompletenessArbiter
  Config: required: metrics/logs/traces topology; sampling decision + trade-off

**Node 8 — Arbiter**
  metrics_topology: Prometheus + Thanos ✓ | PASS
  logging_topology: Fluentd → Loki + label-only ✓ | trade_off: query-only LogQL ✓ | PASS
  tracing_topology: OTel → Jaeger + 5% sampling ✓ | trade_off: low-freq bug miss risk ✓ | PASS
  alerting_layer: AlertManager + PagerDuty ✓ | PASS
  OverallVerdict: PASS

**Node 9 — Formatter**
  Delivery: observability-topology-adr.md + otel-collector-config.yaml | Status: ready | Arbiter: PASS
