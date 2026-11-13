# Layer 2 — Nine-Node 4.3 Pairs (047)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: nine-node-4.3

---

## B17-L2-047-001 — PostgreSQLConnector nine-node trace

**A (N01 input):** SelectedLogicalTransition{ predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY"], connector: "PostgreSQL", domain: "relational-database" }

**B (N01 → N09 trace):**
```
N01: ["IMPLEMENT", "API_CONTRACT", "SECURITY"] — PostgreSQL database connector, bearer token (service account access token) for row-level security
N02: PostgreSQL connection via Npgsql — 3 handlers: ExecuteQueryHandler, ExecuteCommandHandler, ExecuteTransactionHandler
N03: IPostgreSQLConnector: ExecuteQueryAsync, ExecuteCommandAsync, ExecuteTransactionAsync
N04: NpgsqlDataSource (injected), IPostgreSQLTokenStore (service account access token)
N05: 4 files (connector, interface, token store, models)
N06: SELECT (parameterized), INSERT/UPDATE/DELETE (parameterized), BEGIN/COMMIT/ROLLBACK + bearer token auth for row-level security
N08: PASS — 3 handlers implemented, service account access token stored securely, filesGenerated: 4
N09: PostgreSQLConnector complete, 4 files generated
```

Teaching: The "connector exposes an API endpoint and uses a bearer token" pattern produces API_CONTRACT + SECURITY independently — both compile and route to separate flows.
