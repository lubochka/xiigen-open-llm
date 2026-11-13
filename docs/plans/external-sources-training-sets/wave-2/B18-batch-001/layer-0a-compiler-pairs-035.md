# Layer 0-A — Compiler Pairs (035)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-035-001 — NullToNotNull: predicate firing

**Request:** "Write a migration to add a NOT NULL constraint to the email column in the users table, backfilling existing nulls."

**Token scan:**
- "write a migration" → IMPLEMENT fires (Rule 2, absolute)
- "NOT NULL constraint" → 4.4 DDL vocabulary (constraint modification is a migration)
- "email column" → 4.4 column vocabulary
- "backfilling existing nulls" → 4.4 data backfill vocabulary
- "users table" → 4.4 data vocabulary

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-035-002 — NullToNotNull: backfill data migration

**Request:** "Migrate all NULL values in the customer_tier column to 'standard' before adding the NOT NULL constraint."

**Token scan:**
- "migrate all NULL values" → IMPLEMENT fires (Rule 5: data vocabulary)
- "customer_tier column" → 4.4 column vocabulary
- "NULL values" → 4.4 data state vocabulary
- "adding the NOT NULL constraint" → IMPLEMENT reinforcement (DDL migration)
- No ARCHITECTURE, SECURITY, or TEST tokens

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-035-003 — NullToNotNull with TEST

**Request:** "Add NOT NULL constraints to all required product fields and write tests to verify no null values remain."

**Token scan:**
- "add NOT NULL constraints" → IMPLEMENT fires (new constraint artifacts)
- "required product fields" → 4.4 data vocabulary
- "write tests" → TEST fires (Rule 12: verb form)
- "verify no null values remain" → TEST reinforcement
- No ARCHITECTURE or SECURITY tokens

**Output:**
- predicates: ["IMPLEMENT", "TEST"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 5.x
- DomainId: software-development

---

## B18-0A-035-004 — NullToNotNull with ARCHITECTURE

**Request:** "Design the null-safety pattern for all critical business tables and implement the constraint migration."

**Token scan:**
- "design" → ARCHITECTURE fires (Rule 11)
- "null-safety pattern" → "pattern" → ARCHITECTURE fires (Rule 7, absolute)
- "implement the constraint migration" → IMPLEMENT fires (Rule 2: migration artifact)
- "critical business tables" → 4.4 data vocabulary
- No refactoring verbs → TransitionType: feature_addition

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 3.1
- DomainId: software-development

---

## B18-0A-035-005 — NullToNotNull: "consolidate" null handling

**Request:** "Consolidate all nullable columns in the legacy orders table into a structured schema with proper NOT NULL constraints."

**Token scan:**
- "consolidate" → IMPLEMENT fires; consolidate is a refactoring verb → TransitionType: refactoring (Rule 20)
- "nullable columns" → 4.4 column vocabulary
- "legacy orders table" → 4.4 data vocabulary
- "structured schema" → "structured" is adjective (not "structure" noun) → ARCHITECTURE does NOT fire
- TransitionType: refactoring (consolidate wins)

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring
- sub_phase: 4.4
- DomainId: software-development
