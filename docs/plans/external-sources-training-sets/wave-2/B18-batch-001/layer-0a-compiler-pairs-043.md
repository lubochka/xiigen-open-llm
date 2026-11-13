# Layer 0-A — Compiler Pairs (043)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-043-001 — CrossDatabaseMigration: predicate firing

**Request:** "Write a migration to move all product catalog data from the legacy MySQL database to the new PostgreSQL database."

**Token scan:**
- "write a migration" → IMPLEMENT fires (Rule 2: migration artifact)
- "move all product catalog data" → 4.4 vocabulary (data migration)
- "MySQL database to PostgreSQL database" → 4.4 vocabulary (cross-database)
- No "structure", "architecture", "pattern" → ARCHITECTURE does not fire
- No security tokens → SECURITY does not fire

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-043-002 — CrossDatabaseMigration with ARCHITECTURE

**Request:** "Design the cross-database migration pattern and implement the data transfer from MySQL to PostgreSQL."

**Token scan:**
- "design" → ARCHITECTURE fires (Rule 11)
- "cross-database migration pattern" → "pattern" → ARCHITECTURE fires (Rule 7, absolute)
- "implement the data transfer" → IMPLEMENT fires (Rule 2: data migration artifact)
- "MySQL to PostgreSQL" → 4.4 vocabulary
- No security or TEST tokens

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 3.1
- DomainId: software-development

---

## B18-0A-043-003 — NoSQLToSQL: predicate firing

**Request:** "Migrate all customer document records from MongoDB to the relational customers table in PostgreSQL."

**Token scan:**
- "migrate" → IMPLEMENT fires (Rule 5: data migration)
- "customer document records" → 4.4 vocabulary (data records)
- "MongoDB to the relational customers table" → 4.4 vocabulary (NoSQL to SQL migration)
- No ARCHITECTURE, SECURITY, or TEST tokens

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-043-004 — NoSQLToSQL with ARCHITECTURE

**Request:** "Design the relational schema structure for MongoDB document migration and write the migration scripts."

**Token scan:**
- "design" → ARCHITECTURE fires (Rule 11)
- "relational schema structure" → "structure" → ARCHITECTURE fires (Rule 3, absolute)
- "write the migration scripts" → IMPLEMENT fires (Rule 2)
- "MongoDB document migration" → 4.4 vocabulary
- No security or TEST tokens

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 3.1
- DomainId: software-development

---

## B18-0A-043-005 — NoSQLToSQL with SECURITY

**Request:** "Migrate MongoDB user profile records to PostgreSQL and ensure all PII fields are encrypted at rest in the relational schema."

**Token scan:**
- "migrate" → IMPLEMENT fires (Rule 5)
- "PII fields" → SECURITY fires (Rule 14, absolute)
- "encrypted at rest" → SECURITY fires (Rule 14: encrypted, absolute; reinforcement)
- "user profile records" → 4.4 vocabulary
- No ARCHITECTURE tokens (no "structure", "architecture", "pattern")

**Output:**
- predicates: ["IMPLEMENT", "SECURITY"]
- TransitionType: feature_addition
- sub_phase: 4.4 + security
- DomainId: software-development
