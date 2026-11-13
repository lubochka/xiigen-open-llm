# Layer 0-A — Compiler Pairs (045)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-045-001 — AuditTableSetup with SECURITY

**Request:** "Write a migration to create audit tables for the customers records and ensure all PII data is access-controlled."

**Token scan:**
- "write a migration" → IMPLEMENT fires (Rule 2)
- "PII data" → SECURITY fires (Rule 14, absolute: PII token)
- "access-controlled" → SECURITY fires (Rule 14: access control token)
- "audit tables", "customers records" → 4.4 vocabulary
- No ARCHITECTURE or TEST tokens

**Output:**
- predicates: ["IMPLEMENT", "SECURITY"]
- TransitionType: feature_addition
- sub_phase: 4.4 + security
- DomainId: software-development

---

## B18-0A-045-002 — SoftDeleteMigration: predicate firing

**Request:** "Write a migration to add a deleted_at column to the users table and backfill NULL for all active records."

**Token scan:**
- "write a migration" → IMPLEMENT fires (Rule 2)
- "add a deleted_at column" → 4.4 DDL vocabulary (column addition)
- "backfill NULL" → 4.4 data migration vocabulary
- "users table" → 4.4 table vocabulary
- No ARCHITECTURE, SECURITY, or TEST tokens

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-045-003 — SoftDeleteMigration with ARCHITECTURE

**Request:** "Design the soft-delete pattern for the orders and customers tables and implement the migration."

**Token scan:**
- "design" → ARCHITECTURE fires (Rule 11)
- "soft-delete pattern" → "pattern" → ARCHITECTURE fires (Rule 7, absolute)
- "implement the migration" → IMPLEMENT fires (Rule 2)
- "orders and customers tables" → 4.4 vocabulary
- No security or TEST tokens

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 3.1
- DomainId: software-development

---

## B18-0A-045-004 — MultiTenantSchema: predicate firing

**Request:** "Write a migration to add tenant_id column to all shared tables and backfill the default tenant ID."

**Token scan:**
- "write a migration" → IMPLEMENT fires (Rule 2)
- "add tenant_id column" → 4.4 DDL vocabulary (column addition)
- "backfill the default tenant ID" → 4.4 data migration vocabulary
- No ARCHITECTURE, SECURITY, or TEST tokens

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-045-005 — MultiTenantSchema with ARCHITECTURE + SECURITY

**Request:** "Design the multi-tenant schema structure for row-level security and implement the migration to add tenant isolation."

**Token scan:**
- "design" → ARCHITECTURE fires (Rule 11)
- "multi-tenant schema structure" → "structure" → ARCHITECTURE fires (Rule 3, absolute)
- "row-level security" → SECURITY fires (Rule 14: security token)
- "implement the migration" → IMPLEMENT fires (Rule 2)
- No TEST tokens; no refactoring verbs → TransitionType: feature_addition

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE", "SECURITY"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 3.1 + security
- DomainId: software-development
