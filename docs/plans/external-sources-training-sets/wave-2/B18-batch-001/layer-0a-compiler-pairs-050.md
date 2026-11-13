# Layer 0-A — Compiler Pairs (050)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-050-001 — SoftDeleteMigration with TEST

**Request:** "Write a migration to add soft-delete columns to all entity tables and write tests to verify hard-deleted records are excluded from queries."

**Token scan:**
- "write a migration" → IMPLEMENT fires (Rule 2)
- "write tests" → TEST fires (Rule 12: verb form)
- "soft-delete columns" → 4.4 DDL vocabulary
- "all entity tables" → 4.4 vocabulary
- No ARCHITECTURE or SECURITY tokens

**Output:**
- predicates: ["IMPLEMENT", "TEST"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 5.x
- DomainId: software-development

---

## B18-0A-050-002 — MultiTenantSchema with TEST

**Request:** "Write the multi-tenant isolation migration and write tests to verify tenant data is completely isolated."

**Token scan:**
- "write the multi-tenant isolation migration" → IMPLEMENT fires (Rule 2)
- "write tests" → TEST fires (Rule 12: verb form)
- "tenant data is completely isolated" → TEST context (isolation verification)
- No ARCHITECTURE or SECURITY tokens

**Output:**
- predicates: ["IMPLEMENT", "TEST"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 5.x
- DomainId: software-development

---

## B18-0A-050-003 — SchemaVersioning with TEST

**Request:** "Write the schema version tracking migration and write tests to verify migrations are recorded in correct sequence."

**Token scan:**
- "write the schema version tracking migration" → IMPLEMENT fires (Rule 2)
- "write tests" → TEST fires (Rule 12: verb form)
- "migrations are recorded in correct sequence" → TEST context
- No ARCHITECTURE or SECURITY tokens

**Output:**
- predicates: ["IMPLEMENT", "TEST"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 5.x
- DomainId: software-development

---

## B18-0A-050-004 — SeedDataMigration with ARCHITECTURE

**Request:** "Design the seed data pattern for the reference tables and implement the seed migrations."

**Token scan:**
- "design" → ARCHITECTURE fires (Rule 11)
- "seed data pattern" → "pattern" → ARCHITECTURE fires (Rule 7, absolute)
- "implement the seed migrations" → IMPLEMENT fires (Rule 2)
- "reference tables" → 4.4 vocabulary
- No security or TEST tokens

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 3.1
- DomainId: software-development

---

## B18-0A-050-005 — TestDataMigration with SECURITY

**Request:** "Write the test data seeding migration and ensure all PII fields are anonymized before population."

**Token scan:**
- "write the test data seeding migration" → IMPLEMENT fires (Rule 2)
- "PII fields" → SECURITY fires (Rule 14, absolute)
- "anonymized" → SECURITY reinforcement (anonymization = PII handling)
- "population" → 4.4 seed vocabulary
- No ARCHITECTURE or TEST tokens

**Output:**
- predicates: ["IMPLEMENT", "SECURITY"]
- TransitionType: feature_addition
- sub_phase: 4.4 + security
- DomainId: software-development
