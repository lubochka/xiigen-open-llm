# Layer 2 — Nine-Node 4.3 Pairs (057)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: nine-node-4.3

---

## B17-L2-057-001 — GCSConnector nine-node trace

**A (N01 input):** SelectedLogicalTransition{ predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"], connector: "GCS", domain: "cloud-object-storage" }

**B (N01 → N09 trace):**
```
N01: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"] — GCS (Google Cloud Storage) connector, OAuth 2.0 service account access token
N02: Google Cloud Storage JSON API v1 — 3 handlers: UploadObjectHandler, DownloadObjectHandler, ListObjectsHandler
N03: IGCSConnector: UploadObjectAsync, DownloadObjectAsync, ListObjectsAsync
N04: StorageClient (injected), IGCSTokenStore (service account access token), IGCSTokenRefresher (OAuth 2.0 service account)
N05: 5 files (connector, interface, token store, token refresher, models)
N06: POST /upload/storage/v1/b/{bucket}/o, GET /storage/v1/b/{bucket}/o/{object}, GET /storage/v1/b/{bucket}/o + Bearer service account access token + OAuth /token endpoint
N08: PASS — 3 handlers implemented, OAuth 2.0 service account token exchange present, service account access token stored securely, filesGenerated: 5
N09: GCSConnector complete, 5 files generated
```
