# Cross-Layer Synthesis (001)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: cross-layer-synthesis

## B18-CLS-001 — Full Seven-Layer Trace: CustomerDataMigration

**Request:** "Write the CustomerDataMigration to move all customer records to the new normalized customer schema structure."

---

### Layer 0-A: Compiler

**Input tokens:** write, migration, customer records, normalized, schema, structure

**Predicate firing:**
- "write migration" → IMPLEMENT fires (Rule 2)
- "structure" → ARCHITECTURE fires (Rule 3, absolute)
- No refactoring verbs → feature_addition
- No test/document/security vocabulary → TEST, DOCUMENT, SECURITY do not fire

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- DomainId: software-development
- GapHandler: none (B18 has no gap vocabulary)

---

### Layer 0-B: Router

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE"], vocabulary: customer records, migration, schema structure

**Routing decisions:**
- IMPLEMENT → 4.4 (customer records + migration vocabulary → DataMigrationFlow44)
- ARCHITECTURE → 3.1 (architecture design → ArchitectureFlow31)
- TransitionType: feature_addition → DataMigrationFlow44 (not BackendServiceFlow41)

**Output:**
- IMPLEMENT → DataMigrationFlow44 (sub-phase 4.4)
- ARCHITECTURE → ArchitectureFlow31 (sub-phase 3.1)
- Flow ordering: ArchitectureFlow31 first (schema design before migration implementation)

---

### Layer 0-C: Knowledge Edge

**Active edges for this request:**
- "structure" → ARCHITECTURE (absolute, Rule 3)
- "write migration" → IMPLEMENT + feature_addition (Rule 2)
- "customer records" → 4.4 data vocabulary (Rule 6)
- "schema structure" = "schema" (contextual) + "structure" (absolute) → "structure" is the firing token
- No "refactor" → no refactoring edge active
- No "seed"/"fixture" → 4.4 fired by migration vocabulary, not seeding vocabulary

**Key disambiguation:** "schema structure" — "schema" alone would NOT fire ARCHITECTURE (contextual), but "structure" fires it absolutely. The compound noun does not suppress "structure".

---

### Layer 0-D: Flow Factory

**Factory mode determination:**
- CustomerDataMigration — scenario not previously registered in trusted_learned
- Factory mode: probationary_accepted (first known good classification)
- Gate B type: row_count_assertion (DataMigrationFlow44 standard gate)

**Factory output:**
```
FlowFactoryEntry {
  ScenarioId: "CustomerDataMigration",
  Flows: [ArchitectureFlow31, DataMigrationFlow44],
  TransitionType: feature_addition,
  FactoryMode: probationary_accepted,
  GateB: {
    Type: row_count_assertion,
    Condition: "migrated_count == source_count"
  }
}
```

---

### Layer 1: Transition Orchestrator

**Ordering input:** [ArchitectureFlow31, DataMigrationFlow44]

**Orchestration rules applied:**
- Rule: ARCHITECTURE before IMPLEMENT (schema design before migration)
- No TEST → no TEST dependency constraint needed
- No QA → QA last rule not applicable
- No DOCUMENT → no parallel scheduling needed

**Execution schedule:**
1. ArchitectureFlow31 — design normalized customer schema
2. DataMigrationFlow44 — migrate customer records per schema spec

**TransitionType: feature_addition** — persisted through all layers; no override.

---

### Layer 2: Nine-Node Trace (DataMigrationFlow44)

**N01 — Input:** "move all customer records to the new normalized customer schema structure"

**N02 — Scope:** CustomerDataMigration; source: legacy customers table; target: normalized customers_v2 schema

**N03 — Analysis:** record count assessment; NULL field identification; schema diff between legacy and target

**N04 — Specification:**
- CustomerDataMigration.cs
- CustomerMigrationValidator.cs (field mapping + NULL handling)
- CustomerMigrationRollback.cs (idempotency guard)
- CustomerMigrationTests.cs (integration tests, optional)

**N05 — Generation:** migration script + validator + rollback handler

**N06 — Execution:** run migration against staging; capture row counts before and after

**N07 — Pre-Gate:** regression check; no existing customer queries broken; TransactionType: feature_addition confirmed

**N08 — Gate B:**
- Type: row_count_assertion
- Condition: `migrated_customer_count == source_customer_count`
- Status: PASS (all records transferred)

**N09 — Output:** CustomerDataMigration delivered; ArchitectureFlow31 schema spec confirmed; Gate B passed.

---

### Layer 3: Teaching Point

**Core lesson from CustomerDataMigration:**

"structure" in "schema structure" fires ARCHITECTURE absolutely. Even though the phrase describes a database schema shape (seemingly a data concern), the token "structure" is an absolute ARCHITECTURE trigger with no contextual exceptions.

**Test question:** "Migrate customer records to the new schema" → does ARCHITECTURE fire?
- Answer: NO. "schema" alone is contextual. No "structure", "architecture", "pattern", or "design" present → ARCHITECTURE absent.

**Test question:** "Migrate customer records to the new schema structure" → does ARCHITECTURE fire?
- Answer: YES. "structure" fires absolutely. Sub-phases: 4.4 + 3.1.

**The one-word difference ("schema" vs "schema structure") changes the predicate output.**

---

### Cross-Layer Invariants Verified

| Layer | Input | Output | Consistent? |
|---|---|---|---|
| 0-A Compiler | "write migration" + "structure" | ["IMPLEMENT", "ARCHITECTURE"] | YES |
| 0-B Router | ["IMPLEMENT", "ARCHITECTURE"] | 4.4 + 3.1 | YES |
| 0-C Knowledge | "structure" token | ARCHITECTURE absolute edge | YES |
| 0-D Factory | CustomerDataMigration | probationary_accepted, row_count_assertion | YES |
| L1 Orchestrator | [ArchitectureFlow31, DataMigrationFlow44] | ARCH → IMPL sequential | YES |
| L2 Nine-Node | DataMigrationFlow44 | Gate B: row_count_assertion PASS | YES |
| L3 Teaching | "schema structure" vs "schema" | ARCHITECTURE fires on "structure" | YES |
