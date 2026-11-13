# Layer 0-A — Compiler Pairs (044)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-044-001 — DatabaseCluster: ARCHITECTURE fires on DB cluster design vocabulary

"architect the database cluster topology for high availability" → ARCHITECTURE ("architect", "topology")
"design the primary-replica database architecture" → ARCHITECTURE ("design", "architecture")
"structure the database tier with read replicas and failover" → ARCHITECTURE ("structure")
"connect to the database cluster" → gap ("connect" is operational gap)

---

## B14-compiler-044-002 — DatabaseCluster: IMPLEMENT fires on DB IaC artifacts

"implement the RDS (Relational Database Service) cluster Terraform module" → IMPLEMENT ("implement" + "module")
"write the database StatefulSet manifest" → IMPLEMENT ("write" + "manifest")
"scaffold the Patroni configuration for PostgreSQL HA (High Availability)" → IMPLEMENT ("scaffold" + "configuration")

---

## B14-compiler-044-003 — DatabaseCluster: SECURITY fires on DB encryption/credential vocabulary

"enforce TDE (Transparent Data Encryption) at rest for the database cluster" → SECURITY ("enforce", "encryption at rest")
"rotate database master credentials via Vault" → SECURITY ("rotate" credentials; "vault" = SECURITY)
"configure TLS for database client connections" → SECURITY ("TLS")

---

## B14-compiler-044-004 — DatabaseCluster: AUTHORIZATION fires on DB access control

"grant db-admin IAM role to database administrators" → AUTHORIZATION ("grant", "IAM role")
"define ClusterRole for read-only database access" → AUTHORIZATION ("ClusterRole")
"restrict database access to authorized service accounts only" → AUTHORIZATION ("restrict" in access context, "service accounts")

---

## B14-compiler-044-005 — DatabaseCluster: API_CONTRACT fires on DB endpoint exposure

"define the database connection API contract for microservices" → API_CONTRACT ("define" in contract context + "API contract")
"expose the database read endpoint as a service interface" → API_CONTRACT ("expose" + "service interface")
"specify the schema contract for database migrations" → API_CONTRACT ("specify" + "schema" in API context)
