# Layer 2 — Nine-Node 4.3 Pairs (046)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: nine-node-4.3

---

## B17-L2-046-001 — MongoDBConnector nine-node trace

**A (N01 input):** SelectedLogicalTransition{ predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY"], connector: "MongoDB", domain: "document-database" }

**B (N01 → N09 trace):**
```
N01: ["IMPLEMENT", "API_CONTRACT", "SECURITY"] — MongoDB Atlas connector, access token auth
N02: MongoDB Atlas Data API v1 — 3 handlers: InsertDocumentHandler, FindDocumentsHandler, UpdateDocumentHandler
N03: IMongoDBConnector: InsertDocumentAsync, FindDocumentsAsync, UpdateDocumentAsync
N04: HttpClient (injected), IMongoTokenStore (Atlas access token credential)
N05: 4 files (connector, interface, token store, models)
N06: POST /action/insertOne, POST /action/find, POST /action/updateOne + Bearer access token in Authorization header
N08: PASS — 3 handlers implemented, Atlas access token stored securely, filesGenerated: 4
N09: MongoDBConnector complete, 4 files generated
```
