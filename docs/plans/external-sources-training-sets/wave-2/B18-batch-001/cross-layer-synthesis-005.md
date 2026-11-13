# Cross-Layer Synthesis (005)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: cross-layer-synthesis

## B18-CLS-005 — Full Seven-Layer Trace: AddressBookMigration (IMPLEMENT alone, feature_addition)

**Request:** "Write the AddressBookMigration to move all legacy address book entries from the flat contacts table to the new normalized addresses, phone_numbers, and emails tables."

---

### Layer 0-A: Compiler

**Input tokens:** write, migration, address book entries, legacy, contacts table, normalized, addresses, phone_numbers, emails

**Predicate firing:**
- "write migration" → IMPLEMENT fires (Rule 2)
- "address book entries", "contacts table" → 4.4 sub-phase signal (data vocabulary)
- No refactoring verbs → feature_addition
- "normalized" → NOT an ARCHITECTURE trigger (data adjective, not structural noun)
- No "structure", "architecture", "pattern" → ARCHITECTURE does NOT fire
- No test/document/security vocabulary → TEST, DOCUMENT, SECURITY do not fire

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- DomainId: software-development
- GapHandler: none

---

### Layer 0-B: Router

**Input:** predicates ["IMPLEMENT"], vocabulary: address book entries, migration, contacts table

**Routing decisions:**
- IMPLEMENT + feature_addition + 4.4 vocabulary → DataMigrationFlow44
- No secondary routing

**Output:**
- IMPLEMENT → DataMigrationFlow44 (sub-phase 4.4)

---

### Layer 0-C: Knowledge Edge

**Active edges:**
- "write migration" → IMPLEMENT + feature_addition (Rule 2)
- "address book entries" → 4.4 routing edge
- "contacts table" → 4.4 routing edge (database vocabulary)
- "normalized" → data action adjective; no ARCHITECTURE edge
- No "structure" → ARCHITECTURE edge inactive

**Key disambiguation:** "normalized" does not fire ARCHITECTURE. Only "structure"/"architecture"/"pattern"/"design" fire ARCHITECTURE.

---

### Layer 0-D: Flow Factory

**Factory output:**
```
FlowFactoryEntry {
  ScenarioId: "AddressBookMigration",
  Flows: ["DataMigrationFlow44"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_accepted",
  GateB: {
    Type: "row_count_assertion",
    Condition: "count(addresses) + proportional(phone_numbers, emails) == count(legacy_contacts)"
  }
}
```

---

### Layer 1: Transition Orchestrator

**Ordering input:** [DataMigrationFlow44]

**Orchestration rules applied:**
- Single flow — no ordering dependencies
- TransitionType: feature_addition — new migration artifact; no regression check
- No ARCHITECTURE → no ArchitectureFlow31 prerequisite

**Execution schedule:**
1. DataMigrationFlow44 — migrate 750K contacts to normalized three-table schema

---

### Layer 2: Nine-Node Trace (DataMigrationFlow44)

**N01 — Input:** "move legacy address book entries to normalized addresses/phone_numbers/emails"

**N02 — Scope:** 750,000 contacts; target: addresses + phone_numbers + emails

**N03 — Analysis:** flat contacts table; 48 columns; avg 1.4 phones, 1.2 emails per contact

**N04 — Specification:**
- AddressBookMigration_up.sql
- AddressBookMigration_down.sql
- AddressBookMigration_idempotency.sql
- migration_log_table.sql

**N05 — Generation:** batch 500; INSERT addresses → phone_numbers → emails; NULL handling for phone2/email2

**N06 — Execution:** 1,500 batches; log each contact_id in migration_log

**N07 — Pre-Gate:** count 750K source; estimate target proportions

**N08 — Gate B:**
- Type: row_count_assertion
- Condition: count(addresses) == 750,000 AND all FKs valid
- Status: PASS

**N09 — Output:** AddressBookMigration complete; 4 files

---

### Layer 3: Teaching Point

**Core lesson from AddressBookMigration:**

"normalized" does not fire ARCHITECTURE. Students often expect normalization language to trigger architectural review. It does not. Only explicit structural tokens ("structure", "architecture", "pattern", "design") fire ARCHITECTURE.

**Test question:** "Migrate contacts to normalize address storage" → does ARCHITECTURE fire?
- Answer: NO. "normalize" is not "structure". No ARCHITECTURE trigger.

**Test question:** "Migrate contacts to redesign the address structure" → does ARCHITECTURE fire?
- Answer: YES. "structure" fires absolutely. "redesign" reinforces it.

---

### Cross-Layer Invariants Verified

| Layer | Input | Output | Consistent? |
|---|---|---|---|
| 0-A Compiler | "write migration" + "normalized" (no "structure") | ["IMPLEMENT"], feature_addition | YES |
| 0-B Router | ["IMPLEMENT"], 4.4 vocabulary | DataMigrationFlow44 (4.4) | YES |
| 0-C Knowledge | "normalized" ≠ ARCHITECTURE edge | ARCHITECTURE edge inactive | YES |
| 0-D Factory | AddressBookMigration | probationary_accepted, row_count_assertion | YES |
| L1 Orchestrator | [DataMigrationFlow44] | single flow, no dependencies | YES |
| L2 Nine-Node | DataMigrationFlow44 | Gate B: row_count_assertion PASS | YES |
| L3 Teaching | "normalized" vs "structure" | Only "structure" fires ARCHITECTURE | YES |
