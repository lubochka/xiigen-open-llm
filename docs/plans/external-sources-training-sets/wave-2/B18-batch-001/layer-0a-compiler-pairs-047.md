# Layer 0-A — Compiler Pairs (047)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-047-001 — RollbackScript: predicate firing

**Request:** "Write the rollback migration script to revert the schema changes if the deployment fails."

**Token scan:**
- "write the rollback migration script" → IMPLEMENT fires (Rule 2: migration artifact)
- "revert the schema changes" → 4.4 vocabulary (DDL rollback)
- "deployment fails" → 4.4 vocabulary (deployment context)
- No ARCHITECTURE, SECURITY, or TEST tokens

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-047-002 — RollbackScript with TEST

**Request:** "Write the rollback migration and write tests to verify the rollback restores the database to its previous state."

**Token scan:**
- "write the rollback migration" → IMPLEMENT fires (Rule 2)
- "write tests" → TEST fires (Rule 12: verb form "write tests")
- "previous state" → TEST context (state restoration verification)
- No ARCHITECTURE or SECURITY tokens

**Output:**
- predicates: ["IMPLEMENT", "TEST"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 5.x
- DomainId: software-development

---

## B18-0A-047-003 — RollbackScript ARCHITECTURE context

**Request:** "Design the rollback pattern for multi-step schema migrations to ensure safe reversion."

**Token scan:**
- "design" → ARCHITECTURE fires (Rule 11)
- "rollback pattern" → "pattern" → ARCHITECTURE fires (Rule 7, absolute)
- "multi-step schema migrations" → 4.4 vocabulary
- No explicit "write migration" or "create migration" → IMPLEMENT does not fire independently
- No security or TEST tokens

**Output:**
- predicates: ["ARCHITECTURE"]
- TransitionType: feature_addition
- sub_phase: 3.1
- DomainId: software-development

---

## B18-0A-047-004 — Complete factory reference: trigger summary 043-047

**Trigger vocabulary for CrossDatabaseMigration through RollbackScript (043-047):**

| Domain | IMPLEMENT trigger | SECURITY trigger | ARCHITECTURE trigger |
|---|---|---|---|
| CrossDatabaseMigration | "write migration", "migrate data" | "PII", "encrypted" | "migration pattern", "cross-database architecture" |
| NoSQLToSQL | "migrate", "write migration" | "PII", "encrypted" | "relational schema structure" |
| EventSourcingSchema | "write migration", "create table" | (uncommon) | "event sourcing schema structure" |
| CQRSProjection | "write migration", "implement migration" | (uncommon) | "CQRS projection pattern" |
| AuditTableSetup | "write migration", "create audit tables" | "PII", "access control" | "audit pattern", "audit architecture" |
| SoftDeleteMigration | "write migration", "add column" | (uncommon) | "soft-delete pattern" |
| MultiTenantSchema | "write migration", "add column" | "row-level security" | "multi-tenant schema structure" |
| SchemaVersioning | "write migration", "add table" | (uncommon) | "schema versioning architecture" |
| SeedDataMigration | "write seed migration", "populate" | (uncommon) | (uncommon) |
| TestDataMigration | "write migration", "populate" | (uncommon) | (uncommon) |
| RollbackScript | "write rollback migration" | (uncommon) | "rollback pattern" |

---

## B18-0A-047-005 — Complete reference: ARCHITECTURE+IMPLEMENT combo rules

**When both ARCHITECTURE and IMPLEMENT fire in the same request:**

| Scenario | ARCHITECTURE trigger | IMPLEMENT trigger | Execution order |
|---|---|---|---|
| "design X structure + write migration" | "structure" (absolute) | "write migration" (Rule 2) | ArchitectureFlow31 → DataMigrationFlow44 |
| "design X pattern + implement migration" | "pattern" (absolute) | "implement" (Rule 2) | ArchitectureFlow31 → DataMigrationFlow44 |
| "design X architecture + create indexes" | "architecture" (absolute) | "create index" (Rule 2) | ArchitectureFlow31 → DataMigrationFlow44 |
| "design first + migrate next" | "design" verb (Rule 11) | "migrate" (Rule 5) | ArchitectureFlow31 → DataMigrationFlow44 |

**Key rule:** ARCHITECTURE always runs before IMPLEMENT (schema/design must exist before migration executes). This ordering is absolute in B18 DataMigrationFlow44 scenarios.
