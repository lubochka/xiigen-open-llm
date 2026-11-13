# Layer 0-A — Compiler Pairs (054)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-054-001 — AuditTableSetup: full five-predicate

**Request:** "Design the GDPR-compliant audit architecture, write the migration to create audit tables, ensure all PII fields are masked, write tests to verify audit trigger coverage, and document the audit trail policy."

**Token scan:**
- "design" → ARCHITECTURE fires (Rule 11)
- "GDPR-compliant audit architecture" → "architecture" → ARCHITECTURE fires (Rule 7, absolute)
- "write the migration" → IMPLEMENT fires (Rule 2)
- "GDPR-compliant" → SECURITY fires (Rule 14: GDPR, absolute)
- "PII fields are masked" → SECURITY fires (Rule 14: PII, absolute; reinforcement)
- "write tests" → TEST fires (Rule 12: verb form)
- "document the audit trail policy" → DOCUMENT fires (Rule 4)
- No refactoring verbs → TransitionType: feature_addition

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE", "SECURITY", "TEST", "DOCUMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 3.1 + security + 5.x + 2.1
- DomainId: software-development

---

## B18-0A-054-002 — SoftDeleteMigration: four-predicate

**Request:** "Write the soft-delete migration, write tests to verify deleted records are excluded from default queries, and document the soft-delete usage pattern."

**Token scan:**
- "write the soft-delete migration" → IMPLEMENT fires (Rule 2)
- "write tests" → TEST fires (Rule 12: verb form)
- "document the soft-delete usage pattern" → DOCUMENT fires (Rule 4)
- "pattern" in "usage pattern" is a noun → ARCHITECTURE fires? NO — "usage pattern" in a documentation/naming context, with "document" preceding it, refers to a "pattern of use" not a design trigger. **Disambiguation:** "document the ... pattern" fires DOCUMENT (strong verb). ARCHITECTURE fires only when "pattern" describes an artifact to be designed, not when it is the name of a document.
- No SECURITY tokens

**Key disambiguation:** "document the X pattern" — DOCUMENT fires from "document" verb. ARCHITECTURE does not double-fire here because "pattern" names what is being documented, not what is being designed. ARCHITECTURE fires on "design the X pattern" or "X pattern structure", not "document the X pattern".

**Output:**
- predicates: ["IMPLEMENT", "TEST", "DOCUMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 5.x + 2.1
- DomainId: software-development

---

## B18-0A-054-003 — SeedDataMigration + TestDataMigration combined

**Request:** "Write the seed data migrations for production reference tables and the test data fixtures for the staging environment."

**Token scan:**
- "write the seed data migrations" → IMPLEMENT fires (Rule 2: migration artifacts)
- "test data fixtures" → 4.4 vocabulary (fixtures = seed vocabulary; not TEST predicate)
- "production reference tables" → 4.4 vocabulary
- "staging environment" → 4.4 vocabulary (not TEST trigger)
- No "write tests" → TEST does NOT fire (no verb form)
- No ARCHITECTURE or SECURITY tokens

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-054-004 — CrossDatabaseMigration refactoring + ARCHITECTURE

**Request:** "Consolidate the cross-database migration scripts into a reusable pattern and update the existing MySQL-to-PostgreSQL migration."

**Token scan:**
- "consolidate" → IMPLEMENT fires; "consolidate" is a refactoring verb → TransitionType: refactoring (Rule 20)
- "reusable pattern" → "pattern" → ARCHITECTURE fires (Rule 7, absolute)
- "update the existing migration" → IMPLEMENT reinforcement
- "MySQL-to-PostgreSQL migration" → 4.4 vocabulary
- TransitionType: refactoring wins

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: refactoring
- sub_phase: 4.4 + 3.1
- DomainId: software-development

---

## B18-0A-054-005 — NoSQLToSQL with full five-predicate

**Request:** "Design the NoSQL-to-SQL mapping structure, implement the migration from DynamoDB to PostgreSQL, ensure sensitive user PII is encrypted at rest, write tests to verify all records are migrated, and document the mapping rules."

**Token scan:**
- "design" → ARCHITECTURE fires (Rule 11)
- "mapping structure" → "structure" → ARCHITECTURE fires (Rule 3, absolute)
- "implement the migration" → IMPLEMENT fires (Rule 2)
- "sensitive user PII" → SECURITY fires (Rule 14: PII, absolute; "sensitive" reinforcement)
- "encrypted at rest" → SECURITY fires (Rule 14: encrypted, absolute)
- "write tests" → TEST fires (Rule 12: verb form)
- "document the mapping rules" → DOCUMENT fires (Rule 4)

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE", "SECURITY", "TEST", "DOCUMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 3.1 + security + 5.x + 2.1
- DomainId: software-development
