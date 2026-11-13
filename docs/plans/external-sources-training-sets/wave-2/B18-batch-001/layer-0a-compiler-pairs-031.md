# Layer 0-A — Compiler Pairs (031)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-031-001 — SequenceReset: predicate firing

**Request:** "Write a migration to reset the sequence on the orders_id_seq to start from 100000 after data import."

**Token scan:**
- "write a migration" → IMPLEMENT fires (Rule 2, absolute)
- "reset the sequence" → 4.4 DDL vocabulary (sequence is a database object)
- "orders_id_seq" → 4.4 data vocabulary
- "after data import" → 4.4 data loading vocabulary
- No ARCHITECTURE, SECURITY, or TEST tokens

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-031-002 — SequenceReset: "restructure sequence" disambiguation

**Request:** "Restructure all auto-increment sequences to use a uniform naming pattern and reset values."

**Token scan:**
- "restructure" → IMPLEMENT fires; restructure is a refactoring verb → TransitionType: refactoring (Rule 1)
- "auto-increment sequences" → 4.4 database vocabulary
- "uniform naming pattern" → "pattern" → ARCHITECTURE fires (Rule 7, absolute)
- TransitionType: refactoring wins (Rule 9 hierarchy)

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: refactoring
- sub_phase: 4.4 + 3.1
- DomainId: software-development

---

## B18-0A-031-003 — SequenceReset with DOCUMENT

**Request:** "Reset all primary key sequences after the legacy data import and document the new starting values."

**Token scan:**
- "reset all primary key sequences" → IMPLEMENT fires (new sequence state = new database configuration)
- "legacy data import" → 4.4 data import vocabulary
- "document the new starting values" → DOCUMENT fires (Rule 4: document verb)
- No ARCHITECTURE, SECURITY tokens

**Output:**
- predicates: ["IMPLEMENT", "DOCUMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 2.1
- DomainId: software-development

---

## B18-0A-031-004 — SequenceReset: "populate" after sequence reset

**Request:** "Populate the employee ID sequence cache and reset the HR data sequences to match the imported records."

**Token scan:**
- "populate" → IMPLEMENT fires (Rule: populate = seeding-equivalent → feature_addition + 4.4)
- "employee ID sequence" → 4.4 vocabulary
- "HR data sequences" → 4.4 data vocabulary
- "imported records" → 4.4 data import vocabulary
- No ARCHITECTURE, SECURITY, or TEST tokens

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-031-005 — SequenceReset with ARCHITECTURE

**Request:** "Design the sequence architecture for the multi-tenant ID generation system and create the reset migration."

**Token scan:**
- "design" → ARCHITECTURE fires (Rule 11)
- "sequence architecture" → "architecture" → ARCHITECTURE fires (Rule 7, absolute)
- "create the reset migration" → IMPLEMENT fires (Rule 2)
- "multi-tenant ID generation system" → 4.4 database vocabulary
- No refactoring verbs → TransitionType: feature_addition

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 3.1
- DomainId: software-development
