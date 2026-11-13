# Layer 0-B — Router Pairs (038)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-038-001 — ShardingMigration routing (feature_addition)

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE"], TransitionType: feature_addition, sub-phase: 4.4 + 3.1

**Routing decision:**
- ARCHITECTURE + 3.1 → ArchitectureFlow31 (shard architecture must precede migration)
- IMPLEMENT + 4.4 → DataMigrationFlow44 (migrate data to shards)
- No SECURITY or TEST

**Execution order:** ArchitectureFlow31 → DataMigrationFlow44

**Output:** ArchitectureFlow31 → DataMigrationFlow44

---

## B18-0B-038-002 — ShardingMigration routing (refactoring)

**Input:** predicates ["IMPLEMENT"], TransitionType: refactoring, sub-phase: 4.4

**Routing decision:**
- IMPLEMENT + refactoring + 4.4 → DataMigrationFlow44
- TransitionType: refactoring does NOT change the flow selection to BackendServiceFlow41 when 4.4 vocabulary is present
- "restructure for sharding" = data vocabulary (4.4) regardless of refactoring TransitionType

**Key anti-pattern:** "restructure" sets TransitionType: refactoring but does NOT reroute to 4.1 when the target is a database table (4.4 vocabulary).

**Output:** DataMigrationFlow44 (with TransitionType: refactoring)

---

## B18-0B-038-003 — DataArchival with SECURITY routing

**Input:** predicates ["IMPLEMENT", "SECURITY"], TransitionType: feature_addition, sub-phase: 4.4 + security

**Routing decision:**
- IMPLEMENT + 4.4 → DataMigrationFlow44
- SECURITY → SecurityReviewFlow (after IMPLEMENT; PII/GDPR archival requires security review)

**Execution order:** DataMigrationFlow44 → SecurityReviewFlow

**Output:** DataMigrationFlow44 → SecurityReviewFlow

---

## B18-0B-038-004 — ReplicationSetup routing

**Input:** predicates ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Routing decision:**
- IMPLEMENT + 4.4 (replication setup = database migration artifact) → DataMigrationFlow44
- No ARCHITECTURE, SECURITY, TEST → single flow

**Output:** DataMigrationFlow44

---

## B18-0B-038-005 — ReplicationSetup with ARCHITECTURE routing

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE"], TransitionType: feature_addition, sub-phase: 4.4 + 3.1

**Routing decision:**
- ARCHITECTURE + 3.1 → ArchitectureFlow31 (replication topology design first)
- IMPLEMENT + 4.4 → DataMigrationFlow44

**Execution order:** ArchitectureFlow31 → DataMigrationFlow44

**Output:** ArchitectureFlow31 → DataMigrationFlow44
