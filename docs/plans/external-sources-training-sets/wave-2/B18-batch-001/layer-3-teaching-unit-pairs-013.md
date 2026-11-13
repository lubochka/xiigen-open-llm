# Layer 3 — Teaching Unit Pairs (013)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: teaching-unit

## B18-TU-013-001 — Teaching: AddressBook migration → IMPLEMENT, feature_addition, 4.4

**Question:** What predicates, TransitionType, and sub-phase does "Migrate address book entries to normalized tables" produce?

**Example:** "Migrate all address book entries from the legacy contacts table to the normalized addresses/phone_numbers/emails tables."

**Explanation:**
- "migrate" → IMPLEMENT fires
- "address book entries", "contacts table" → 4.4 data vocabulary
- No refactoring verb → feature_addition
- No "structure"/"architecture"/"pattern" → ARCHITECTURE absent
- Result: ["IMPLEMENT"], feature_addition, 4.4

**Anti-example (v6-03):** Would route to BackendServiceFlow41 treating "address" as service vocabulary. Wrong: "address book entries" and "contacts table" are data vocabulary → 4.4.

---

## B18-TU-013-002 — Teaching: "normalize" is NOT an ARCHITECTURE trigger

**Question:** Does "normalize" fire ARCHITECTURE?

**Answer:** No. "normalize" is a data action verb. It describes data shape, not system architecture.

**Example A:** "Migrate to normalize phone storage" → ARCHITECTURE absent.
**Example B:** "Migrate to normalize table structure" → "structure" fires ARCHITECTURE (not "normalize").

**Teaching rule:** ARCHITECTURE fires on "structure", "architecture", "pattern", "design" (structural noun). Not on "normalize", "denormalize", "flatten", "expand".

---

## B18-TU-013-003 — Teaching: "schema structure" fires ARCHITECTURE; "normalized schema" does not

**Test questions:**
1. "Migrate to the new normalized schema" → ARCHITECTURE fires? **NO.** "schema" alone is contextual; no structural noun.
2. "Migrate to the new schema structure" → ARCHITECTURE fires? **YES.** "structure" is present (absolute trigger).
3. "Migrate to the new address structure" → ARCHITECTURE fires? **YES.** "structure" is present.
4. "Migrate to the normalized address data" → ARCHITECTURE fires? **NO.** No absolute trigger.

---

## B18-TU-013-004 — Teaching: "refactor" + "migrate" → refactoring wins (Rule 9)

**Question:** "Refactor the AddressService and migrate address records to the new table." What is TransitionType?

**Answer:** refactoring. Rule 9 is absolute: "refactor" verb present → TransitionType: refactoring.

**Both sub-phases fire:**
- 4.1 (AddressService vocabulary)
- 4.4 (migrate address records vocabulary)

**Anti-example (v6-02):** Would set feature_addition because "migration creates a new artifact". Wrong: "refactor" verb dominates regardless of whether new artifacts are created.

---

## B18-TU-013-005 — Teaching: compound noun with "structure" fires ARCHITECTURE

**Rule:** The noun "structure" fires ARCHITECTURE absolutely in any compound noun. The qualifier never suppresses it.

**All fire ARCHITECTURE:**
- "address structure" ✓
- "table structure" ✓
- "data structure" ✓
- "contact schema structure" ✓
- "normalized structure" ✓

**Does NOT fire:**
- "structured data" (adjective form)
- "address format" (no trigger word)
- "schema" alone (contextual, not absolute)
