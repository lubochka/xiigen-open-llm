# Layer 0-A — Compiler Pairs (032)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-032-001 — EnumTypeChange: predicate firing

**Request:** "Write a migration to add 'cancelled' and 'refunded' to the order_status enum type."

**Token scan:**
- "write a migration" → IMPLEMENT fires (Rule 2, absolute)
- "order_status enum type" → 4.4 DDL vocabulary (enum type is a database type object)
- No "structure", "architecture", "pattern" → ARCHITECTURE does not fire
- No security tokens, no TEST verb

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-032-002 — EnumTypeChange: "rename enum value" disambiguation

**Request:** "Rename the 'active' enum value to 'enabled' across all tables that use the status_type enum."

**Token scan:**
- "rename" → IMPLEMENT fires; rename is a refactoring verb → TransitionType: refactoring (Rule 9)
- "enum value" → 4.4 database type vocabulary
- "status_type enum" → 4.4 vocabulary
- No ARCHITECTURE tokens; "type" alone does not fire ARCHITECTURE

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-032-003 — EnumTypeChange with ARCHITECTURE

**Request:** "Design the enum type structure for all state machine tables and write the migration to apply the new types."

**Token scan:**
- "design" → ARCHITECTURE fires (Rule 11)
- "enum type structure" → "structure" → ARCHITECTURE fires (Rule 3, absolute)
- "write the migration" → IMPLEMENT fires (Rule 2, absolute)
- "state machine tables" → 4.4 data vocabulary
- No refactoring verbs → TransitionType: feature_addition

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 3.1
- DomainId: software-development

---

## B18-0A-032-004 — EnumTypeChange: migrate enum data

**Request:** "Migrate all rows using the deprecated 'pending_review' status to the new 'under_review' enum value."

**Token scan:**
- "migrate all rows" → IMPLEMENT fires (Rule 5: data vocabulary)
- "deprecated 'pending_review' status" → 4.4 data migration vocabulary
- "new 'under_review' enum value" → 4.4 vocabulary
- No ARCHITECTURE, SECURITY, or TEST tokens

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-032-005 — EnumTypeChange with TEST

**Request:** "Add the 'archived' value to the document_state enum and write tests to verify all state transitions still work."

**Token scan:**
- "add the 'archived' value to the document_state enum" → IMPLEMENT fires (new artifact state)
- "document_state enum" → 4.4 vocabulary
- "write tests" → TEST fires (Rule 12: verb form)
- "verify all state transitions still work" → TEST reinforcement; "write tests" is the trigger
- No ARCHITECTURE or SECURITY tokens

**Output:**
- predicates: ["IMPLEMENT", "TEST"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 5.x
- DomainId: software-development
