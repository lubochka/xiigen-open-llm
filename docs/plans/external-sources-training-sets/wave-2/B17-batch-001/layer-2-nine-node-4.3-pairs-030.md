# Layer 2 — Nine-Node 4.3 Pairs (030)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: nine-node-4.3

---

## B17-L2-030-001 — AlgoliaConnector nine-node flow

**A (Input):** SelectedLogicalTransition: "Build the Algolia search connector with index synchronization and admin API token."

**B (Nine-node execution):**
```
N01: ["IMPLEMENT", "API_CONTRACT", "SECURITY"] — index objects, search, delete index entries
N02: Algolia Search API v1 — 3 handlers: IndexObjectsHandler, SearchIndexHandler, DeleteObjectsHandler
N03: IAlgoliaConnector: IndexObjectsAsync, SearchAsync, DeleteObjectsAsync
N04: HttpClient (injected), IAlgoliaCredentialStore (ApplicationId + AdminApiToken), IAlgoliaRequestBuilder
N05: 4 files (connector, interface, credential store, models)
N06: POST /1/indexes/{indexName}/batch, POST /1/indexes/{indexName}/query, DELETE /1/indexes/{indexName}/deleteByQuery; X-Algolia-Application-Id + X-Algolia-API-Key headers
N07: ArbiterSelectorUnit selects completeness + credential security review
N08: PASS — 3 handlers implemented, admin API token injected via credential store, batch indexing present, filesGenerated: 4
N09: AlgoliaConnector complete, 4 files generated
```

Gate B: 3 handlers + admin API token in credential store + batch index support
