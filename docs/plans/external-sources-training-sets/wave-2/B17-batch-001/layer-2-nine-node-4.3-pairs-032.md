# Layer 2 — Nine-Node 4.3 Pairs (032)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: nine-node-4.3

---

## B17-L2-032-001 — MixpanelConnector nine-node flow

**A (Input):** SelectedLogicalTransition: "Build the Mixpanel product analytics connector with event ingestion endpoint and project token."

**B (Nine-node execution):**
```
N01: ["IMPLEMENT", "API_CONTRACT", "SECURITY"] — track events, identify users, export data
N02: Mixpanel Ingestion API v1 — 3 handlers: TrackEventHandler, IdentifyUserHandler, ImportEventsHandler
N03: IMixpanelConnector: TrackEventAsync, IdentifyUserAsync, ImportEventsAsync
N04: HttpClient (injected), IMixpanelCredentialStore (ProjectToken as query param or header), IMixpanelEventSerializer
N05: 3 files (connector, interface, models)
N06: POST /track, POST /engage, POST /import; token passed as project_token query parameter or Authorization header; UTF-8 URL-encoded JSON
N07: ArbiterSelectorUnit selects completeness + credential review
N08: PASS — 3 handlers implemented, project token stored and injected securely, event serialization present, filesGenerated: 3
N09: MixpanelConnector complete, 3 files generated
```

Gate B: 3 handlers + project token securely stored + event serialization
