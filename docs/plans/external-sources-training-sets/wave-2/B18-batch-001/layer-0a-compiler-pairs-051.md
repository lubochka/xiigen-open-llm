# Layer 0-A — Compiler Pairs (051)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-051-001 — RollbackScript with ARCHITECTURE

**Request:** "Design the rollback architecture for multi-step migrations and implement the rollback scripts."

**Token scan:**
- "design" → ARCHITECTURE fires (Rule 11)
- "rollback architecture" → "architecture" → ARCHITECTURE fires (Rule 7, absolute)
- "implement the rollback scripts" → IMPLEMENT fires (Rule 2)
- "multi-step migrations" → 4.4 vocabulary
- No security or TEST tokens

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 3.1
- DomainId: software-development

---

## B18-0A-051-002 — CrossDatabaseMigration + EventSourcingSchema combined

**Request:** "Migrate all historical events from the legacy MySQL event_log table to the new PostgreSQL event_store schema."

**Token scan:**
- "migrate" → IMPLEMENT fires (Rule 5)
- "historical events" → 4.4 vocabulary (event data)
- "legacy MySQL event_log table to PostgreSQL event_store schema" → 4.4 vocabulary (cross-database migration)
- No ARCHITECTURE, SECURITY, or TEST tokens

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-051-003 — MultiTenantSchema with ARCHITECTURE + SECURITY + TEST

**Request:** "Design the multi-tenant row-level security structure, implement the isolation migration, and write tests to verify tenant data cannot cross tenant boundaries."

**Token scan:**
- "design" → ARCHITECTURE fires (Rule 11)
- "row-level security structure" → "structure" → ARCHITECTURE fires (Rule 3, absolute)
- "row-level security" → SECURITY fires (Rule 14: security)
- "implement the isolation migration" → IMPLEMENT fires (Rule 2)
- "write tests" → TEST fires (Rule 12: verb form)
- No refactoring verbs → TransitionType: feature_addition

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE", "SECURITY", "TEST"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 3.1 + security + 5.x
- DomainId: software-development

---

## B18-0A-051-004 — SoftDeleteMigration refactoring disambiguation

**Request:** "Consolidate all hard-delete operations to use soft-delete pattern and write the migration."

**Token scan:**
- "consolidate" → IMPLEMENT fires; "consolidate" is a refactoring verb → TransitionType: refactoring (Rule 20)
- "soft-delete pattern" → "pattern" → ARCHITECTURE fires (Rule 7, absolute)
- "write the migration" → IMPLEMENT fires (Rule 2: migration artifact)
- TransitionType: refactoring wins (Rule 9 hierarchy)

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: refactoring
- sub_phase: 4.4 + 3.1
- DomainId: software-development

---

## B18-0A-051-005 — SchemaVersioning refactoring

**Request:** "Restructure the migration history table to support the new versioning scheme."

**Token scan:**
- "restructure" → IMPLEMENT fires; "restructure" is a refactoring verb → TransitionType: refactoring (Rule 1)
- "migration history table" → 4.4 vocabulary (database table)
- "new versioning scheme" → 4.4 vocabulary
- No ARCHITECTURE tokens (no "structure", "architecture", "pattern")

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring
- sub_phase: 4.4
- DomainId: software-development
