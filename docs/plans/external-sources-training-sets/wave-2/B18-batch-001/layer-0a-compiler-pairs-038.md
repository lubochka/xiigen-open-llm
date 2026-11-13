# Layer 0-A — Compiler Pairs (038)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-038-001 — DataArchival: predicate firing

**Request:** "Write a migration to archive orders older than 3 years to the orders_archive table."

**Token scan:**
- "write a migration" → IMPLEMENT fires (Rule 2: migration artifact)
- "archive orders" → 4.4 vocabulary (data movement between tables)
- "orders older than 3 years" → 4.4 vocabulary (database record operation)
- No "structure", "architecture", "pattern" → ARCHITECTURE does not fire
- No security tokens → SECURITY does not fire

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-038-002 — DataArchival with ARCHITECTURE

**Request:** "Design the archival data structure and write the migration to move stale records to archive tables."

**Token scan:**
- "design" → ARCHITECTURE fires (Rule 11)
- "archival data structure" → "structure" → ARCHITECTURE fires (Rule 3, absolute)
- "write the migration" → IMPLEMENT fires (Rule 2)
- "archive tables" → 4.4 vocabulary
- No security tokens or refactoring verbs

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 3.1
- DomainId: software-development

---

## B18-0A-038-003 — DataArchival with SECURITY

**Request:** "Migrate customer PII data to the GDPR-compliant archive and ensure all records are encrypted at rest."

**Token scan:**
- "migrate" → IMPLEMENT fires (Rule 5: data migration)
- "customer PII data" → SECURITY fires (Rule 14: PII, absolute)
- "GDPR-compliant" → SECURITY fires (Rule 14: GDPR, absolute; reinforcement)
- "encrypted at rest" → SECURITY fires (Rule 14: encrypted, absolute; reinforcement)
- "archive" → 4.4 vocabulary (data archival)
- No ARCHITECTURE or TEST tokens

**Output:**
- predicates: ["IMPLEMENT", "SECURITY"]
- TransitionType: feature_addition
- sub_phase: 4.4 + security
- DomainId: software-development

---

## B18-0A-038-004 — ShardingMigration: predicate firing

**Request:** "Write a migration to move the users table to a sharded architecture across 4 database nodes."

**Token scan:**
- "write a migration" → IMPLEMENT fires (Rule 2)
- "sharded architecture" → "architecture" → ARCHITECTURE fires (Rule 7, absolute)
- "4 database nodes" → 4.4 vocabulary (database infrastructure)
- "move the users table" → 4.4 vocabulary (data migration)

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 3.1
- DomainId: software-development

---

## B18-0A-038-005 — ShardingMigration with refactoring

**Request:** "Restructure the payments table for horizontal sharding and migrate data to the new shard layout."

**Token scan:**
- "restructure" → IMPLEMENT fires; "restructure" is a refactoring verb → TransitionType: refactoring (Rule 1)
- "horizontal sharding" → 4.4 vocabulary (database sharding)
- "migrate data" → IMPLEMENT reinforcement (data migration)
- "new shard layout" → 4.4 vocabulary
- No ARCHITECTURE tokens (no "structure", "architecture", "pattern" present)

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring
- sub_phase: 4.4
- DomainId: software-development
