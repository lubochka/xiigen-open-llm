# Layer 0-B — Router Pairs (042)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-042-001 — EventSourcingSchema routing

**Input:** predicates ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Routing decision:**
- IMPLEMENT + 4.4 (event_store table creation = DDL migration) → DataMigrationFlow44
- Event sourcing schema = database table vocabulary → 4.4

**Output:** DataMigrationFlow44

---

## B18-0B-042-002 — EventSourcingSchema with ARCHITECTURE routing

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE"], TransitionType: feature_addition, sub-phase: 4.4 + 3.1

**Routing decision:**
- ARCHITECTURE + 3.1 → ArchitectureFlow31 (event store schema design)
- IMPLEMENT + 4.4 → DataMigrationFlow44

**Execution order:** ArchitectureFlow31 → DataMigrationFlow44

**Output:** ArchitectureFlow31 → DataMigrationFlow44

---

## B18-0B-042-003 — CQRSProjection routing

**Input:** predicates ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Routing decision:**
- IMPLEMENT + 4.4 (projection tables = DDL migration) → DataMigrationFlow44
- CQRS read-model tables = database DDL vocabulary → always 4.4

**Output:** DataMigrationFlow44

---

## B18-0B-042-004 — CQRSProjection with ARCHITECTURE + TEST routing

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE", "TEST"], TransitionType: feature_addition, sub-phase: 4.4 + 3.1 + 5.x

**Routing decision:**
- ARCHITECTURE + 3.1 → ArchitectureFlow31 (CQRS pattern design first)
- IMPLEMENT + 4.4 → DataMigrationFlow44
- TEST + 5.x → UnitTestingFlow51 (verify projection correctness)
- ARCHITECTURE before IMPLEMENT; TEST after IMPLEMENT

**Execution order:** ArchitectureFlow31 → DataMigrationFlow44 → UnitTestingFlow51

**Output:** ArchitectureFlow31 → DataMigrationFlow44 → UnitTestingFlow51

---

## B18-0B-042-005 — AuditTableSetup routing

**Input:** predicates ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Routing decision:**
- IMPLEMENT + 4.4 (audit table creation = DDL migration) → DataMigrationFlow44
- "audit" alone without PII/GDPR → SECURITY does not fire → single flow

**Output:** DataMigrationFlow44
