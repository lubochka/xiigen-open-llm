# Layer 0-B — Router Pairs (059)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-059-001 — Router disambiguation: "create" verb routing by object type

**"create" verb routing by object type:**

| "create" + object | Sub-phase | Flow |
|---|---|---|
| "create a migration" | 4.4 | DataMigrationFlow44 |
| "create a table" | 4.4 | DataMigrationFlow44 |
| "create an index" | 4.4 | DataMigrationFlow44 |
| "create a constraint" | 4.4 | DataMigrationFlow44 |
| "create a trigger" | 4.4 | DataMigrationFlow44 |
| "create a stored procedure" | 4.4 | DataMigrationFlow44 |
| "create a service class" | 4.1 | BackendServiceFlow41 |
| "create an API connector" | 4.3 | ApiContractFlow43 |
| "create a repository" | 4.1 | BackendServiceFlow41 |
| "create an event handler" | 4.1 | BackendServiceFlow41 |

**Rule:** "create" + database object → DataMigrationFlow44. "create" + code artifact → BackendServiceFlow41.

---

## B18-0B-059-002 — Router disambiguation: "implement" verb routing

**"implement" verb routing by object type:**

| "implement" + object | Sub-phase | Flow |
|---|---|---|
| "implement the migration" | 4.4 | DataMigrationFlow44 |
| "implement the index creation" | 4.4 | DataMigrationFlow44 |
| "implement the event schema" | 4.4 | DataMigrationFlow44 |
| "implement the CQRS projections" | 4.4 | DataMigrationFlow44 |
| "implement the UserService" | 4.1 | BackendServiceFlow41 |
| "implement the API connector" | 4.3 | ApiContractFlow43 |

---

## B18-0B-059-003 — Router disambiguation: "add" verb routing

**"add" verb routing by object type:**

| "add" + object | Sub-phase | Flow |
|---|---|---|
| "add a column" | 4.4 | DataMigrationFlow44 |
| "add an index" | 4.4 | DataMigrationFlow44 |
| "add a foreign key" | 4.4 | DataMigrationFlow44 |
| "add a constraint" | 4.4 | DataMigrationFlow44 |
| "add a migration" | 4.4 | DataMigrationFlow44 |
| "add a method to UserService" | 4.1 | BackendServiceFlow41 |
| "add an API endpoint" | 4.3 | ApiContractFlow43 |

---

## B18-0B-059-004 — Router disambiguation: "move" verb routing

**"move" verb routing by object type:**

| "move" + object | Sub-phase | Flow | TransitionType |
|---|---|---|---|
| "move records to archive table" | 4.4 | DataMigrationFlow44 | feature_addition |
| "move data to new schema" | 4.4 | DataMigrationFlow44 | feature_addition |
| "move (code) function to new class" | 4.1 | BackendServiceFlow41 | refactoring |
| "move service to new module" | 4.1 | BackendServiceFlow41 | refactoring |

**Key rule:** "move" on data/records → feature_addition + 4.4. "move" on code → refactoring + 4.1.

---

## B18-0B-059-005 — Complete B18 router disambiguation: 12 verb × object routing rules

**12 verb + object routing combinations that are commonly confused:**

| Verb + Object | Correct routing | Common mistake |
|---|---|---|
| write migration + table | DataMigrationFlow44 | BackendServiceFlow41 |
| migrate + records | DataMigrationFlow44 | BackendServiceFlow41 |
| create + index | DataMigrationFlow44 | BackendServiceFlow41 |
| seed + table data | DataMigrationFlow44 | UnitTestingFlow51 |
| populate + fixtures | DataMigrationFlow44 | UnitTestingFlow51 |
| archive + records | DataMigrationFlow44 | BackendServiceFlow41 |
| anonymize + data | DataMigrationFlow44 + SecurityReviewFlow | DataMigrationFlow44 only |
| mask + PII | DataMigrationFlow44 + SecurityReviewFlow | DataMigrationFlow44 only |
| migrate + from MongoDB | DataMigrationFlow44 | ApiContractFlow43 |
| create + event_store table | DataMigrationFlow44 | BackendServiceFlow41 |
| rollback + migration | DataMigrationFlow44 | BackendServiceFlow41 |
| promote + replica | DataMigrationFlow44 | BackendServiceFlow41 |
