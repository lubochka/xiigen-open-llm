# Layer 0-C — Knowledge Edge Pairs (044)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-044-001 — DatabaseCluster: gap vocabulary — DB runtime operations

Confirmed gap vocabulary for database runtime operations (zero predicates):
  Action verbs: connect, query, insert, update, delete (SQL), select, index, vacuum, analyze, reindex, failover (operational), promote (replica — operational), pause, resume, stop, start
  Runtime nouns: connection string, DSN (Data Source Name), query plan, index, table, row, schema (database schema in runtime context)

---

## B14-edge-044-002 — DatabaseCluster: disambiguation — "schema" in API vs DB context

"schema" in API context: "specify the schema contract" → API_CONTRACT (schema + contract vocabulary)
"schema" in database DDL (Data Definition Language) context: "design the schema" → ARCHITECTURE if design verb present; "schema migration" → gap (operational)
"schema" in event bus context: "specify the CloudEvents schema" → API_CONTRACT
Rule: "schema" fires API_CONTRACT when accompanied by contract/API vocabulary; fires ARCHITECTURE when with design verb in infra design context; is gap in runtime DDL context.

---

## B14-edge-044-003 — DatabaseCluster: SECURITY boundary — DB encryption compound phrases

SECURITY fires for:
  "TDE (Transparent Data Encryption) at rest", "TLS for database connections", "rotate database master credentials", "vault dynamic credentials for database", "encrypt database at rest"
  "failover to replica" → gap (operational) | "promote read replica" → gap (operational)

---

## B14-edge-044-004 — DatabaseCluster: API_CONTRACT boundary — DB connection contract

API_CONTRACT fires when database connectivity is framed as a service contract:
  "define the database connection API contract" → API_CONTRACT
  "expose the database read endpoint as a service interface" → API_CONTRACT
  "connect to the database" → gap (operational, not a contract definition)
  "specify the connection pool contract" → API_CONTRACT ("specify" + "contract")
