# Layer 0-A — Compiler Pairs (026)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-026-001 — ForeignKeyMigration: predicate firing

**Request:** "Write a migration to add a foreign key constraint from orders.customer_id to customers.id."

**Token scan:**
- "write a migration" → IMPLEMENT fires (Rule 2: absolute; migration artifact)
- "orders.customer_id", "customers.id" → 4.4 vocabulary (database table/column vocabulary)
- "foreign key constraint" → 4.4 DDL (Data Definition Language) migration vocabulary
- No "structure", "architecture" → ARCHITECTURE does not fire
- No security tokens → SECURITY does not fire

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-026-002 — ForeignKeyMigration with ARCHITECTURE

**Request:** "Design the foreign key architecture between the orders, customers, and products tables before writing the migration."

**Token scan:**
- "design" → ARCHITECTURE fires (Rule 11)
- "foreign key architecture" → "architecture" → ARCHITECTURE fires (Rule 7, absolute)
- "writing the migration" → IMPLEMENT fires (Rule 2: migration artifact)
- "orders, customers, and products tables" → 4.4 vocabulary
- No refactoring verbs → TransitionType: feature_addition

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 3.1
- DomainId: software-development

---

## B18-0A-026-003 — ForeignKeyMigration: "rename column" disambiguation

**Request:** "Rename the column user_id to customer_id in the orders table and add a foreign key constraint."

**Token scan:**
- "rename" → IMPLEMENT fires; "rename" is a refactoring verb → TransitionType: refactoring (Rule 9)
- "column" → table/column vocabulary reinforces 4.4 sub-phase
- "add a foreign key constraint" → IMPLEMENT reinforcement; but rename takes priority for TransitionType
- No ARCHITECTURE trigger tokens

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-026-004 — ForeignKeyMigration with DOCUMENT

**Request:** "Add foreign key constraints between all related tables and document the referential integrity rules."

**Token scan:**
- "add foreign key constraints" → IMPLEMENT fires (new constraint artifacts)
- "related tables" → 4.4 data vocabulary
- "document" → DOCUMENT fires (Rule 4: document verb)
- "referential integrity rules" → documentation target
- No ARCHITECTURE or SECURITY tokens

**Output:**
- predicates: ["IMPLEMENT", "DOCUMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 2.1
- DomainId: software-development

---

## B18-0A-026-005 — ForeignKeyMigration: "consolidate" disambiguation

**Request:** "Consolidate the foreign key constraints across the billing tables into a consistent pattern."

**Token scan:**
- "consolidate" → IMPLEMENT fires; consolidate is a refactoring verb → TransitionType: refactoring (Rule 20)
- "billing tables" → 4.4 vocabulary
- "consistent pattern" → "pattern" → ARCHITECTURE fires (Rule 7, absolute)
- TransitionType: refactoring wins (Rule 9 hierarchy: refactoring > feature_addition)

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: refactoring
- sub_phase: 4.4 + 3.1
- DomainId: software-development
