# Layer 0-C — Knowledge-Edge Pairs (042)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-042-001 — Edge disambiguation: "test" in migration context

**"test" token disambiguation in migration requests:**

| Phrase | Fires TEST? | Sub-phase |
|---|---|---|
| "write tests to verify index" | YES | 5.x |
| "test database" | NO — "test" modifies "database" (noun compound) | 4.4 |
| "test data fixtures" | NO — "test data" = seed vocabulary | 4.4 |
| "test the index with a query" | NO — "test" is verb without "tests" object | Neither |
| "unit tests for migration" | NO — noun form | Neither |
| "add unit tests for the migration" | YES — "add tests" verb form | 5.x |
| "integration tests pass" | NO — noun form | Neither |
| "write integration tests" | YES — "write tests" verb form | 5.x |

---

## B18-0C-042-002 — Edge disambiguation: DBMS names vs API names

**When does a database name fire ApiContractFlow43 vs DataMigrationFlow44?**

| Token | Flow | Reason |
|---|---|---|
| "MySQL" (as source DB) | DataMigrationFlow44 | DBMS = database system, not API |
| "PostgreSQL" (as target DB) | DataMigrationFlow44 | DBMS = database system, not API |
| "MongoDB" (as source DB) | DataMigrationFlow44 | NoSQL DBMS |
| "DynamoDB" (as AWS source DB) | DataMigrationFlow44 | Cloud DBMS |
| "DynamoDB API calls" | ApiContractFlow43 | "API calls" = connector vocabulary |
| "Stripe API" | ApiContractFlow43 | External API service |
| "REST API migration" | ApiContractFlow43 | API connector vocabulary |

**Key rule:** Database system names alone → DataMigrationFlow44. "API" in database name context → only fires 4.3 if explicitly referring to API calls, not DB operations.

---

## B18-0C-042-003 — Edge disambiguation: "implement" in migration vs code context

**"implement" fires different edges depending on object:**

| Phrase | Predicate | Sub-phase | Flow |
|---|---|---|---|
| "implement the migration" | IMPLEMENT | 4.4 | DataMigrationFlow44 |
| "implement the CQRS projections" | IMPLEMENT | 4.4 | DataMigrationFlow44 |
| "implement the UserService" | IMPLEMENT | 4.1 | BackendServiceFlow41 |
| "implement the API connector" | IMPLEMENT | 4.3 | ApiContractFlow43 |
| "implement index creation" | IMPLEMENT | 4.4 | DataMigrationFlow44 |
| "implement the OrderRepository" | IMPLEMENT | 4.1 | BackendServiceFlow41 |

---

## B18-0C-042-004 — Edge reference: compound noun with "structure" always fires ARCHITECTURE

**Compound nouns containing "structure" ALWAYS fire ARCHITECTURE:**

| Compound noun | ARCHITECTURE fires? |
|---|---|
| "data structure" | YES |
| "schema structure" | YES |
| "index structure" | YES |
| "migration pattern structure" | YES |
| "shard key structure" | YES |
| "audit log structure" | YES |
| "event sourcing schema structure" | YES |
| "multi-tenant table structure" | YES |
| "rollback plan structure" | YES |

**Exception:** "structured" (adjective) does NOT fire ARCHITECTURE. "structure" must be a noun in the token.

---

## B18-0C-042-005 — Edge reference: 037-060 TransitionType determination summary

**TransitionType edge summary for 037-060 domains:**

| Scenario | Dominant verb | TransitionType edge |
|---|---|---|
| "write migration to anonymize" | write migration | feature_addition |
| "restructure table for sharding" | restructure | refactoring (Rule 1) |
| "consolidate shard key constraints" | consolidate | refactoring (Rule 20) |
| "rename deleted_at to removed_at" | rename | refactoring (Rule 9) |
| "refactor rollback scripts" | refactor | refactoring (Rule 9) |
| "migrate from MySQL to PostgreSQL" | migrate | feature_addition (Rule 5) |
| "create composite index" | create | feature_addition (Rule 2) |
| "add tenant_id column" | add | feature_addition (Rule 2) |
| "archive records older than 3 years" | archive | feature_addition (Rule 5) |
| "consolidate cross-database migrations" | consolidate | refactoring (Rule 20) |
