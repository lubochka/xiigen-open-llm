# Layer 0-A — Compiler Pairs (013)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-013-001 — AddressBook migration: "migrate" fires IMPLEMENT, feature_addition, 4.4

**Request:** "Migrate all address book entries from the legacy contacts table to the new normalized addresses/phone_numbers/emails tables."

**Compiler trace:**
- Token: "migrate" → IMPLEMENT (data migration vocabulary)
- Token: "address book entries" → 4.4 sub-phase signal (data vocabulary)
- Token: "contacts table" → 4.4 sub-phase signal (database vocabulary)
- Token: "normalized" → borderline; no "structure" token → ARCHITECTURE does NOT fire
- No refactoring verbs → feature_addition
- TransitionType: feature_addition (migrate data creates a new migration artifact)

**Result:** predicates: ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

---

## B18-0A-013-002 — AddressBook: "normalize" is NOT an ARCHITECTURE trigger

**Request:** "Migrate address records to normalize phone and email storage into separate child tables."

**Compiler trace:**
- Token: "migrate" → IMPLEMENT
- Token: "normalize" → NOT an ARCHITECTURE trigger (normalize is an action verb describing data shape, not a structural noun)
- Token: "records" → 4.4 sub-phase signal
- Token: "tables" → 4.4 sub-phase signal
- No "structure", "architecture", "pattern", "design" → ARCHITECTURE does NOT fire
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Teaching point:** "normalize" does not fire ARCHITECTURE. Only "structure", "pattern", "architecture", "design" (structural noun) trigger ARCHITECTURE.

---

## B18-0A-013-003 — AddressBook + "structure" fires ARCHITECTURE absolutely

**Request:** "Migrate address book entries and redesign the address table structure to support international formats."

**Compiler trace:**
- Token: "migrate" → IMPLEMENT
- Token: "address book entries" → 4.4 sub-phase signal
- Token: "redesign" → ARCHITECTURE (Rule 7)
- Token: "structure" → ARCHITECTURE (Rule 3, absolute)
- No refactoring verbs → feature_addition
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT", "ARCHITECTURE"], TransitionType: feature_addition, sub-phases: 4.4 + 3.1

---

## B18-0A-013-004 — AddressBook: "refactor" + "migrate" → refactoring wins

**Request:** "Refactor the AddressService and migrate stale address records to the archive_addresses table."

**Compiler trace:**
- Token: "refactor" → IMPLEMENT (refactoring vocabulary)
- Token: "migrate" → IMPLEMENT (data vocabulary); 4.4 sub-phase signal
- Token: "AddressService" → 4.1 sub-phase signal
- Token: "address records" → 4.4 sub-phase signal
- TransitionType: refactoring (Rule 9 — "refactor" verb dominates; refactoring > feature_addition)
- Sub-phases: 4.1 (refactoring) + 4.4 (migration); both fire

**Result:** predicates: ["IMPLEMENT"], TransitionType: refactoring, sub-phases: 4.1 + 4.4

---

## B18-0A-013-005 — AddressBook: "write the migration" = feature_addition (Rule 2)

**Request:** "Write the AddressBookMigration to move all legacy address records to the new contacts schema."

**Compiler trace:**
- Token: "write" → IMPLEMENT
- Token: "migration" → IMPLEMENT; feature_addition signal (Rule 2 — new migration artifact)
- Token: "address records" → 4.4 sub-phase signal
- Token: "schema" → borderline; no "structure" co-present → ARCHITECTURE does NOT fire
- TransitionType: feature_addition (Rule 2 — "write the migration" creates a new artifact)

**Result:** predicates: ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4
