# Layer 2 — Nine-Node 4.3 Pairs (041)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: nine-node-4.3

---

## B17-L2-041-001 — SplunkConnector nine-node trace

**A (N01 input):** SelectedLogicalTransition{ predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY"], connector: "Splunk", domain: "log-management" }

**B (N01 → N09 trace):**
```
N01: ["IMPLEMENT", "API_CONTRACT", "SECURITY"] — Splunk HEC log ingestion, HEC token auth
N02: Splunk HEC (HTTP Event Collector) API — 3 handlers: IngestEventHandler, IngestBatchHandler, HandleHealthCheckHandler
N03: ISplunkConnector: IngestEventAsync, IngestBatchAsync, HandleHealthCheckAsync
N04: HttpClient (injected), ISplunkTokenStore (HEC token credential), IHmacValidator
N05: 4 files (connector, interface, token store, models)
N06: POST /services/collector/event, POST /services/collector/event/batch, GET /services/collector/health + HMAC-SHA256 payload validation
N08: PASS — 3 handlers implemented, HEC token stored securely, HMAC-SHA256 payload signature validation present, filesGenerated: 4
N09: SplunkConnector complete, 4 files generated
```
