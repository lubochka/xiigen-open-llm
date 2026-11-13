# Layer 2 — Nine-Node 4.3 Pairs (052)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: nine-node-4.3

---

## B17-L2-052-001 — SupabaseConnector nine-node trace

**A (N01 input):** SelectedLogicalTransition{ predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY"], connector: "Supabase", domain: "backend-as-a-service" }

**B (N01 → N09 trace):**
```
N01: ["IMPLEMENT", "API_CONTRACT", "SECURITY"] — Supabase connector, JWT (JSON Web Token) access for row-level policy
N02: Supabase REST API — 3 handlers: InsertRowHandler, SelectRowsHandler, HandleRealtimeEventHandler
N03: ISupabaseConnector: InsertRowAsync, SelectRowsAsync, HandleRealtimeEventAsync
N04: HttpClient (injected), ISupabaseTokenStore (JWT access token), ISupabaseRealtimeClient
N05: 4 files (connector, interface, token store, models)
N06: POST /rest/v1/{table}, GET /rest/v1/{table}?select=*, GET /realtime/v1/websocket + Bearer JWT access token
N08: PASS — 3 handlers implemented, JWT (JSON Web Token) access token stored securely, filesGenerated: 4
N09: SupabaseConnector complete, 4 files generated
```
