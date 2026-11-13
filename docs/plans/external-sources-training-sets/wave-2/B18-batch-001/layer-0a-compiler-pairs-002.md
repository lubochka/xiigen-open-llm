# Layer 0-A — Compiler Pairs (002)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: compiler

## B18-0A-002-001 — "consolidate" → IMPLEMENT, TransitionType: refactoring

**Request:** "Consolidate the three repository classes into a single GenericRepository<T>."

**Compiler trace:**
- Token: "consolidate" → IMPLEMENT (refactoring vocabulary, Rule 20)
- Token: "classes" → 4.1 sub-phase signal (class vocabulary)
- Token: "repository" → 4.1 sub-phase signal (repository vocabulary)
- TransitionType: refactoring (consolidate restructures existing code, does not create new artifact)
- No ARCHITECTURE trigger: no "pattern", "structure", "architecture", or "design" noun present

**Result:** predicates: ["IMPLEMENT"], TransitionType: refactoring, sub-phase: 4.1

**Explanation:** "Consolidate" merges existing classes — this is restructuring, not a new artifact. TransitionType hierarchy: refactoring > feature_addition. Even if the GenericRepository<T> is technically a new class, the dominant action is consolidation of existing classes, so refactoring wins.

---

## B18-0A-002-002 — "consolidate" with data context → still refactoring (code)

**Request:** "Consolidate the duplicate user validation rules spread across UserService, AccountService, and AuthService into a single ValidationModule."

**Compiler trace:**
- Token: "consolidate" → IMPLEMENT (refactoring vocabulary)
- Token: "rules" → 4.1 sub-phase signal (domain/rule vocabulary)
- Token: "service" (x3) → 4.1 sub-phase signal
- Token: "module" → 4.1 sub-phase signal
- TransitionType: refactoring (consolidate restructures, Rule 20)

**Result:** predicates: ["IMPLEMENT"], TransitionType: refactoring, sub-phase: 4.1

---

## B18-0A-002-003 — "consolidate" + "structure" → IMPLEMENT+ARCHITECTURE

**Request:** "Consolidate the three overlapping customer tables and redesign the structure to eliminate redundancy."

**Compiler trace:**
- Token: "consolidate" → IMPLEMENT (refactoring vocabulary)
- Token: "structure" → ARCHITECTURE (literal rule, always fires — Rule 3)
- Token: "tables" → 4.4 sub-phase signal (database vocabulary)
- Token: "redesign" → ARCHITECTURE reinforcement
- TransitionType: refactoring (consolidate is the primary action verb)

**Result:** predicates: ["IMPLEMENT", "ARCHITECTURE"], TransitionType: refactoring, sub-phases: 4.4 + 3.1

---

## B18-0A-002-004 — "consolidate" distinguishing from "merge migration"

**Request:** "Write a migration to consolidate the three order_status tables into one unified order_status table."

**Compiler trace:**
- Token: "write" → IMPLEMENT
- Token: "migration" → IMPLEMENT; feature_addition signal (Rule 2)
- Token: "consolidate" → IMPLEMENT (refactoring vocabulary)
- TransitionType: feature_addition WINS — "write the migration" (Rule 2) overrides consolidate refactoring signal because a new migration artifact is being created
- Sub-phase: 4.4 (migration + table + database vocabulary)

**Result:** predicates: ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Key distinction:** "Write migration to consolidate" = feature_addition (new migration artifact). "Consolidate classes" = refactoring (no new migration artifact).

---

## B18-0A-002-005 — consolidate + document → IMPLEMENT+DOCUMENT

**Request:** "Consolidate the OrderService payment handling methods and document the new unified payment interface."

**Compiler trace:**
- Token: "consolidate" → IMPLEMENT (refactoring)
- Token: "methods" → 4.1 sub-phase signal (service vocabulary)
- Token: "document" → DOCUMENT (documentation vocabulary, Rule 11)
- TransitionType: refactoring (consolidate verb, no migration artifact)

**Result:** predicates: ["IMPLEMENT", "DOCUMENT"], TransitionType: refactoring, sub-phase: 4.1 + 2.1

---

## B18-0A-002-006 — v6-02 corruption: "consolidate" misclassified as feature_addition

**v6-02 corrupted result:**
- Request: "Consolidate the three repository classes into a single GenericRepository<T>."
- v6-02 predicate: ["IMPLEMENT"], TransitionType: **feature_addition** ← WRONG
- v6-02 reasoning: "GenericRepository<T> is a new class being created"

**Correct result:**
- predicates: ["IMPLEMENT"], TransitionType: **refactoring**
- Reason: "consolidate" triggers refactoring TransitionType (Rule 20). The fact that a new unified class results does not override the dominant verb. Refactoring > feature_addition in the TransitionType hierarchy (Rule 8).

**Teaching point:** TransitionType is determined by the dominant action verb, not by whether a new file is created.
