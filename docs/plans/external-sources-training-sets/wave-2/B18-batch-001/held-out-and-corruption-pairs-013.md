# Held-Out and Corruption Pairs (013)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: held-out-corruption

## B18-HO-013-001 — HO: "address data structure" fires ARCHITECTURE (compound noun)

**Request:** "Migrate address book entries and define the address data structure for international support."

**Correct classification:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- Sub-phases: 4.4 + 3.1
- Reason: "address data structure" contains "structure" (noun) → ARCHITECTURE fires absolutely

**COR-013-001 (v6-01):** ARCHITECTURE absent despite "data structure" present.
- Corrupted output: predicates: ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4
- Corruption: "data structure" interpreted as data vocabulary only; "structure" suppressed
- Fix: Apply Rule 3 — "structure" in "data structure" always fires ARCHITECTURE

---

## B18-HO-013-002 — HO: "normalize" does NOT fire ARCHITECTURE

**Request:** "Migrate address records to normalize phone and email storage."

**Correct classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase: 4.4
- Reason: "normalize" is a data action verb; no "structure"/"architecture"/"pattern" token present

**COR-013-002:** ARCHITECTURE fires from "normalize".
- Corrupted output: predicates: ["IMPLEMENT", "ARCHITECTURE"]
- Corruption: treating "normalize" as architectural vocabulary
- Fix: "normalize" ≠ "structure". Only "structure"/"architecture"/"pattern"/"design" fire ARCHITECTURE.

---

## B18-HO-013-003 — HO: "refactor AddressService" + "migrate" → refactoring wins

**Request:** "Refactor the AddressService to clean up the address validation logic, and migrate stale address records to the archive table."

**Correct classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring (Rule 9 — "refactor" verb present)
- Sub-phases: 4.1 + 4.4

**COR-013-003 (v6-02):** TransitionType: feature_addition (migrate vocabulary overrides refactor).
- Corruption: migration vocabulary overrides refactoring verb
- Fix: Rule 9 absolute — "refactor" always sets refactoring. Feature_addition signal from "migrate" is subordinate.

---

## B18-HO-013-004 — HO: AddressBook + PII → IMPLEMENT+SECURITY

**Request:** "Migrate all address book entries including phone numbers and emails, ensuring all PII is encrypted before transfer."

**Correct classification:**
- predicates: ["IMPLEMENT", "SECURITY"]
- TransitionType: feature_addition
- Sub-phases: 4.4 + security

**COR-013-004:** SECURITY absent. Migration proceeds without SecurityReviewFlow.
- Corruption: "PII is encrypted" interpreted as a migration requirement, not a SECURITY trigger
- Fix: "PII" and "encrypted" are absolute SECURITY triggers. SecurityReviewFlow is mandatory.

---

## B18-HO-013-005 — HO: "write the AddressBookMigration" = Rule 2 feature_addition

**Request:** "Write the AddressBookMigration to move all legacy contact records to the new schema."

**Correct classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition (Rule 2 — "write migration" creates new artifact)
- Sub-phase: 4.4

**COR-013-005 (v6-02):** TransitionType: refactoring ("move" is a refactoring verb).
- Corruption: "move" in "move all legacy contact records" treated as code refactoring "move" verb
- Fix: "move" + data target (records, data) = data migration → feature_addition. "move" + code target (class, method) = refactoring.
