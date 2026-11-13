# Layer 0-A — Compiler Pairs (034)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-034-001 — TimestampTimezone: predicate firing

**Request:** "Write a migration to convert all timestamp columns to timestamptz (timestamp with time zone) in the events table."

**Token scan:**
- "write a migration" → IMPLEMENT fires (Rule 2, absolute)
- "timestamp columns" → 4.4 DDL vocabulary (column type change is a migration)
- "timestamptz" → 4.4 database type vocabulary
- "events table" → 4.4 data vocabulary
- No ARCHITECTURE, SECURITY, or TEST tokens

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-034-002 — TimestampTimezone: data migration of timestamp values

**Request:** "Migrate all UTC timestamps in the bookings table to the new timezone-aware format, converting existing values to UTC+0."

**Token scan:**
- "migrate all UTC timestamps" → IMPLEMENT fires (Rule 5: data vocabulary)
- "bookings table" → 4.4 data vocabulary
- "timezone-aware format" → 4.4 data transformation vocabulary
- "converting existing values" → 4.4 ETL-adjacent (transform vocabulary)
- No ARCHITECTURE, SECURITY, or TEST tokens

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-034-003 — TimestampTimezone with ARCHITECTURE

**Request:** "Design the timestamp standardization pattern for all time-sensitive tables before running the migration."

**Token scan:**
- "design" → ARCHITECTURE fires (Rule 11)
- "timestamp standardization pattern" → "pattern" → ARCHITECTURE fires (Rule 7, absolute)
- "running the migration" → IMPLEMENT fires (Rule 5: migration vocabulary)
- "time-sensitive tables" → 4.4 data vocabulary
- No refactoring verbs → TransitionType: feature_addition

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 3.1
- DomainId: software-development

---

## B18-0A-034-004 — TimestampTimezone: "rename created_at" disambiguation

**Request:** "Rename the created_date column to created_at and convert it to timestamptz in the users table."

**Token scan:**
- "rename" → IMPLEMENT fires; rename is a refactoring verb → TransitionType: refactoring (Rule 9)
- "created_date column" → 4.4 column vocabulary
- "convert it to timestamptz" → 4.4 DDL migration vocabulary
- "users table" → 4.4 data vocabulary
- No ARCHITECTURE tokens

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-034-005 — TimestampTimezone with DOCUMENT

**Request:** "Convert all timestamp columns to timezone-aware and document the timezone handling policy for the data layer."

**Token scan:**
- "convert all timestamp columns" → IMPLEMENT fires (data transformation = feature_addition)
- "timezone-aware" → 4.4 DDL vocabulary
- "document the timezone handling policy" → DOCUMENT fires (Rule 4)
- "data layer" → 4.4 data vocabulary
- No ARCHITECTURE or SECURITY tokens

**Output:**
- predicates: ["IMPLEMENT", "DOCUMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 2.1
- DomainId: software-development
