# Layer 0-A — Compiler Pairs (053)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-053-001 — CrossDatabaseMigration: full five-predicate

**Request:** "Design the cross-database migration pattern, implement the data transfer from MySQL to PostgreSQL, ensure all PII fields are encrypted at rest, write tests to verify all records transferred, and document the migration runbook."

**Token scan:**
- "design" → ARCHITECTURE fires (Rule 11)
- "migration pattern" → "pattern" → ARCHITECTURE fires (Rule 7, absolute)
- "implement the data transfer" → IMPLEMENT fires (Rule 2)
- "PII fields" → SECURITY fires (Rule 14, absolute)
- "encrypted at rest" → SECURITY fires (Rule 14: encrypted, absolute)
- "write tests" → TEST fires (Rule 12: verb form)
- "document the migration runbook" → DOCUMENT fires (Rule 4: document verb)
- No refactoring verbs → TransitionType: feature_addition

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE", "SECURITY", "TEST", "DOCUMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 3.1 + security + 5.x + 2.1
- DomainId: software-development

---

## B18-0A-053-002 — EventSourcingSchema: full five-predicate

**Request:** "Design the event sourcing architecture, write the migration for the event_store schema, ensure events with PII are encrypted, write tests for event storage, and document the event schema."

**Token scan:**
- "design" → ARCHITECTURE fires (Rule 11)
- "event sourcing architecture" → "architecture" → ARCHITECTURE fires (Rule 7, absolute)
- "write the migration" → IMPLEMENT fires (Rule 2)
- "PII are encrypted" → SECURITY fires (PII + encrypted, both absolute)
- "write tests" → TEST fires (Rule 12)
- "document the event schema" → DOCUMENT fires (Rule 4)

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE", "SECURITY", "TEST", "DOCUMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 3.1 + security + 5.x + 2.1
- DomainId: software-development

---

## B18-0A-053-003 — MultiTenantSchema: refactoring with all predicates

**Request:** "Restructure the schema for multi-tenant isolation pattern, write the migration, add row-level security controls, write integration tests, and document the tenant boundary rules."

**Token scan:**
- "restructure" → IMPLEMENT fires; TransitionType: refactoring (Rule 1)
- "multi-tenant isolation pattern" → "pattern" → ARCHITECTURE fires (Rule 7, absolute)
- "write the migration" → IMPLEMENT reinforcement
- "row-level security controls" → SECURITY fires (Rule 14: security)
- "write integration tests" → TEST fires (Rule 12: verb form)
- "document the tenant boundary rules" → DOCUMENT fires (Rule 4)
- TransitionType: refactoring (verb "restructure" dominates)

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE", "SECURITY", "TEST", "DOCUMENT"]
- TransitionType: refactoring
- sub_phase: 4.4 + 3.1 + security + 5.x + 2.1
- DomainId: software-development

---

## B18-0A-053-004 — SchemaVersioning: refactoring with DOCUMENT

**Request:** "Rename the migration_history columns to follow the new versioning schema and document the updated versioning rules."

**Token scan:**
- "rename" → IMPLEMENT fires; "rename" is a refactoring verb → TransitionType: refactoring (Rule 9)
- "document" → DOCUMENT fires (Rule 4)
- "migration_history columns" → 4.4 vocabulary
- "new versioning schema" → 4.4 vocabulary
- No ARCHITECTURE or SECURITY tokens (no "structure", "architecture", "pattern", no PII/GDPR)

**Output:**
- predicates: ["IMPLEMENT", "DOCUMENT"]
- TransitionType: refactoring
- sub_phase: 4.4 + 2.1
- DomainId: software-development

---

## B18-0A-053-005 — RollbackScript: TEST only (no IMPLEMENT)

**Request:** "Write tests to verify that the rollback scripts correctly restore all tables to their pre-migration state."

**Token scan:**
- "write tests" → TEST fires (Rule 12: verb form "write tests")
- "rollback scripts" → 4.4 vocabulary (database migration)
- No "write migration" or explicit IMPLEMENT verb → IMPLEMENT does not fire
- "verify" alone does NOT fire TEST; but "write tests" does
- No ARCHITECTURE or SECURITY tokens

**Key disambiguation:** "write tests to verify rollback" → TEST fires from "write tests" (verb form). The subject "rollback scripts" is the test target, not a migration verb.

**Output:**
- predicates: ["TEST"]
- TransitionType: feature_addition
- sub_phase: 5.x
- DomainId: software-development
