# Layer 2 — Nine-Node Flow Pairs (008)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: nine_node_flow (InfrastructureArchitectureFlow35)

---

## B14-nine-008 — Observability infrastructure

Request: "Architect the observability infrastructure for the microservices platform — define the distributed tracing, metrics aggregation, and log shipping layers."

### Flow: InfrastructureArchitectureFlow35

**Node 1 — Input parsing**
  Target: Platform observability stack | Pillars: tracing, metrics, logs
  Architecture class: observability infrastructure topology

**Node 2 — Requirements extraction**
  Functional: distributed trace context propagation; metrics retention 30 days; log aggregation with full-text search
  Non-functional: trace sampling rate: 5% in production; metrics ingestion < 500ms lag; log query latency < 2s

**Node 3 — Option generation**
  Tracing: Jaeger vs Zipkin vs OpenTelemetry (OTel) collector with backend choice
  Metrics: Prometheus + Thanos for long-term storage vs Grafana Mimir
  Logs: Elasticsearch vs Loki (log aggregation by Grafana)

**Node 4 — Trade-off analysis**
  OTel collector: vendor-neutral; forwards to Jaeger (traces), Prometheus (metrics), Loki (logs) — chosen
  Prometheus + Thanos: established; long-term storage via Thanos compaction; higher ops cost than Mimir
  Loki: lightweight; does not index log content (only labels); < Elasticsearch ops cost; chosen

**Node 5 — ADR**
  Decision: OTel collector → Jaeger (traces) + Prometheus/Thanos (metrics) + Loki (logs)
  Rationale: OTel standardizes instrumentation; consistent across all three pillars
  Trade-off: Loki does not support full-text log search (label-only); mitigated by structured logging convention

**Node 6 — Component placement**
  OTel collector: DaemonSet on each node | Jaeger: all-in-one instance (production: distributed)
  Prometheus: 2 replicas + Thanos sidecar | Loki: distributed mode, 3 ingester replicas
  Grafana: 2 replicas with unified dashboard for all three pillars

**Node 7 — Validation**
  Trace propagation: end-to-end trace from API gateway to OrderService to Database → single trace → PASS
  Metrics retention: Thanos compaction storing 30-day data → PASS
  Log query: label-based query returns results in 1.4s → PASS (< 2s)
  Arbiter: ArchitectureCompletenessArbiter

**Node 8 — Output**
  ADR: Observability-ADR-008-stack.md | Topology diagram: OTel collector routing

**Node 9 — Verdict**
  PASS — Observability infrastructure architecture complete; all three pillars defined with trade-offs documented

