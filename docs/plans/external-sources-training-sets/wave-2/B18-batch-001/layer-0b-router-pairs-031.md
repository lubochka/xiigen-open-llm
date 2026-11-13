# Layer 0-B — Router Pairs (031)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-031-001 — SequenceReset routing

**Input:** predicates ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Routing decision:**
- IMPLEMENT + 4.4 (sequence reset = database DDL operation) → DataMigrationFlow44
- Single flow

**Output:** DataMigrationFlow44

---

## B18-0B-031-002 — SequenceReset: refactoring variant routing

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE"], TransitionType: refactoring, sub-phase: 4.4 + 3.1

**Routing decision:**
- IMPLEMENT + refactoring + 4.4 ("restructure all sequences") → DataMigrationFlow44
- ARCHITECTURE + 3.1 ("uniform naming pattern" trigger) → ArchitectureFlow31
- ARCH before IMPL (schema/naming design before execution)

**Output:** ArchitectureFlow31 → DataMigrationFlow44

---

## B18-0B-031-003 — SequenceReset with DOCUMENT routing

**Input:** predicates ["IMPLEMENT", "DOCUMENT"], TransitionType: feature_addition, sub-phase: 4.4 + 2.1

**Routing decision:**
- IMPLEMENT + 4.4 → DataMigrationFlow44
- DOCUMENT + 2.1 → DocumentationFlow21 (parallel-eligible)

**Output:** DataMigrationFlow44 ‖ DocumentationFlow21

---

## B18-0B-031-004 — SequenceReset with ARCHITECTURE routing

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE"], TransitionType: feature_addition, sub-phase: 4.4 + 3.1

**Routing decision:**
- ARCHITECTURE ("sequence architecture") + 3.1 → ArchitectureFlow31
- IMPLEMENT ("create the reset migration") + 4.4 → DataMigrationFlow44

**Execution order:** ArchitectureFlow31 → DataMigrationFlow44

**Output:** ArchitectureFlow31 → DataMigrationFlow44

---

## B18-0B-031-005 — SequenceReset: v6-03 correction

**Incorrect v6 routing:** "populate the employee ID sequence cache" → BackendServiceFlow41

**Correction:**
- "populate" = seeding-equivalent vocabulary → feature_addition + 4.4
- Sequence cache = database object → DataMigrationFlow44
- Error type: v6-03 (populate routed to 4.1 instead of 4.4)

**Output:** DataMigrationFlow44 (corrected)
