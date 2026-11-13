# Layer 0-B — Router Pairs (039)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-039-001 — ReadReplicaPromotion routing

**Input:** predicates ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Routing decision:**
- IMPLEMENT + 4.4 (replica promotion = database operation) → DataMigrationFlow44
- Single flow; no additional predicates

**Output:** DataMigrationFlow44

---

## B18-0B-039-002 — ReadReplicaPromotion with TEST routing

**Input:** predicates ["IMPLEMENT", "TEST"], TransitionType: feature_addition, sub-phase: 4.4 + 5.x

**Routing decision:**
- IMPLEMENT + 4.4 → DataMigrationFlow44
- TEST + 5.x → UnitTestingFlow51 (after migration completes)
- TEST always after IMPLEMENT: tests verify the promoted primary accepts writes

**Execution order:** DataMigrationFlow44 → UnitTestingFlow51

**Output:** DataMigrationFlow44 → UnitTestingFlow51

---

## B18-0B-039-003 — ReplicationSetup with SECURITY routing

**Input:** predicates ["IMPLEMENT", "SECURITY"], TransitionType: feature_addition, sub-phase: 4.4 + security

**Routing decision:**
- IMPLEMENT + 4.4 → DataMigrationFlow44 (encrypted replication migration)
- SECURITY → SecurityReviewFlow (verify TLS/encryption configuration)
- SecurityReviewFlow after IMPLEMENT

**Execution order:** DataMigrationFlow44 → SecurityReviewFlow

**Output:** DataMigrationFlow44 → SecurityReviewFlow

---

## B18-0B-039-004 — FullTextSearchIndex routing

**Input:** predicates ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Routing decision:**
- IMPLEMENT + 4.4 (FTS index creation = DDL migration) → DataMigrationFlow44
- Full-text search index is a database artifact; always routes to DataMigrationFlow44 (not BackendServiceFlow41)

**Note:** "create full-text search index" = DDL vocabulary = 4.4. Anti-pattern v6-03: do not route to 4.1.

**Output:** DataMigrationFlow44

---

## B18-0B-039-005 — GeoIndex routing

**Input:** predicates ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Routing decision:**
- IMPLEMENT + 4.4 (GiST/GeoIndex = DDL migration artifact) → DataMigrationFlow44
- Spatial index types (GiST, GeoIndex, PostGIS) are DDL vocabulary → always 4.4

**Output:** DataMigrationFlow44
