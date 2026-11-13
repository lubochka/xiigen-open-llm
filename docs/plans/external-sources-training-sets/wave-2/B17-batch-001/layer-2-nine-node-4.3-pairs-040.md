# Layer 2 — Nine-Node 4.3 Pairs (040)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: nine-node-4.3

---

## B17-L2-040-001 — DatadogConnector nine-node trace

**A (N01 input):** SelectedLogicalTransition{ predicates: ["IMPLEMENT", "API_CONTRACT"], connector: "Datadog", domain: "observability" }

**B (N01 → N09 trace):**
```
N01: ["IMPLEMENT", "API_CONTRACT"] — Datadog metrics, API key auth (no token)
N02: Datadog API v2 — 3 handlers: SubmitMetricsHandler, CreateMonitorHandler, QueryLogsHandler
N03: IDatadogConnector: SubmitMetricsAsync, CreateMonitorAsync, QueryLogsAsync
N04: HttpClient (injected), IDatadogApiKeyStore (API key credential)
N05: 3 files (connector, interface, models)
N06: POST /api/v2/series, POST /api/v1/monitor, GET /api/v2/logs/events/search
N08: PASS — 3 handlers implemented, API key stored securely, filesGenerated: 3
N09: DatadogConnector complete, 3 files generated
```

Note: No HMAC webhook validation required (API key only, SECURITY not compiled). SecurityAuditFlow34 not selected.
