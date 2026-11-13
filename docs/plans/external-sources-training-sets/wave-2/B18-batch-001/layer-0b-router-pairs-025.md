# Layer 0-B — Router Pairs (025)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-025-001 — IndexCreation routing

**Input:** predicates ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Routing decision:**
- IMPLEMENT + 4.4 (index creation = database migration artifact) → DataMigrationFlow44
- No ARCHITECTURE, SECURITY, TEST, DOCUMENT → single flow

**Output:** DataMigrationFlow44

---

## B18-0B-025-002 — IndexCreation with ARCHITECTURE routing

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE"], TransitionType: feature_addition, sub-phase: 4.4 + 3.1

**Routing decision:**
- IMPLEMENT + 4.4 → DataMigrationFlow44
- ARCHITECTURE + 3.1 → ArchitectureFlow31
- ARCHITECTURE must precede IMPLEMENT (schema/index design before creation)

**Execution order:** ArchitectureFlow31 → DataMigrationFlow44

**Output:** ArchitectureFlow31 → DataMigrationFlow44

---

## B18-0B-025-003 — IndexCreation with TEST routing

**Input:** predicates ["IMPLEMENT", "TEST"], TransitionType: feature_addition, sub-phase: 4.4 + 5.x

**Routing decision:**
- IMPLEMENT + 4.4 → DataMigrationFlow44
- TEST + 5.x → UnitTestingFlow51
- TEST always after IMPLEMENT in migration scenarios (hard dependency)

**Execution order:** DataMigrationFlow44 → UnitTestingFlow51

**Output:** DataMigrationFlow44 → UnitTestingFlow51

---

## B18-0B-025-004 — IndexCreation: IMPLEMENT only (refactoring variant)

**Input:** predicates ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Routing decision:**
- IMPLEMENT + feature_addition + 4.4 → DataMigrationFlow44
- Single flow; no additional predicates

**Note:** "rebuild index" (data vocabulary) → always DataMigrationFlow44, never BackendServiceFlow41.

**Output:** DataMigrationFlow44

---

## B18-0B-025-005 — IndexCreation anti-pattern: v6-03 correction

**Incorrect v6 routing:** "create index on users table" → BackendServiceFlow41 (v6 misroutes to 4.1)

**Correction:**
- "create index" = database DDL artifact → 4.4 vocabulary
- Correct flow: DataMigrationFlow44 (not BackendServiceFlow41)
- Error type: v6-03 (data vocabulary routed to 4.1 instead of 4.4)

**Output:** DataMigrationFlow44 (corrected)
