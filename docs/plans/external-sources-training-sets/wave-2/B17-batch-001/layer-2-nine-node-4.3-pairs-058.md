# Layer 2 — Nine-Node 4.3 Pairs (058)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: nine-node-4.3

---

## B17-L2-058-001 — AzureBlobConnector nine-node trace

**A (N01 input):** SelectedLogicalTransition{ predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"], connector: "AzureBlob", domain: "cloud-blob-storage" }

**B (N01 → N09 trace):**
```
N01: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"] — Azure Blob Storage connector, OAuth 2.0 AAD access token + SAS token
N02: Azure Blob Storage REST API — 3 handlers: UploadBlobHandler, DownloadBlobHandler, DeleteBlobHandler
N03: IAzureBlobConnector: UploadBlobAsync, DownloadBlobAsync, DeleteBlobAsync
N04: BlobServiceClient (injected), IAzureBlobTokenStore (SAS token + AAD access token), IAzureOAuthRefresher
N05: 5 files (connector, interface, token store, OAuth refresher, models)
N06: PUT /{container}/{blob}, GET /{container}/{blob}, DELETE /{container}/{blob} + SAS token or Bearer AAD access token + OAuth /token endpoint
N08: PASS — 3 handlers implemented, OAuth 2.0 AAD token exchange present, SAS token and AAD access token stored securely, filesGenerated: 5
N09: AzureBlobConnector complete, 5 files generated
```
