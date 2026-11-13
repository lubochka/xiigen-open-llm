# Layer 0-B — Router Pairs (016)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-016-001 — ProductVariants: seed → DataMigrationFlow44

**Input:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase signal: 4.4 (seed, product_variants table, variant data)
- ConnectorName: ProductVariantsSeeding

**Routing decision:**
- IMPLEMENT + feature_addition + seed + 4.4 vocabulary → DataMigrationFlow44
- No secondary routing

**Output:** flow: DataMigrationFlow44, sub-phase: 4.4

---

## B18-0B-016-002 — ProductVariants + ARCHITECTURE + DOCUMENT → three flows

**Input:**
- predicates: ["IMPLEMENT", "ARCHITECTURE", "DOCUMENT"]
- TransitionType: feature_addition
- Sub-phase signals: 4.4 (seed, product variants) + 3.1 (data structure) + 2.1 (document)

**Routing decision:**
- IMPLEMENT → DataMigrationFlow44, sub-phase: 4.4
- ARCHITECTURE → ArchitectureFlow31, sub-phase: 3.1
- DOCUMENT → DocumentationFlow21, sub-phase: 2.1
- DOCUMENT is parallel-eligible with DataMigrationFlow44
- Ordering: ArchitectureFlow31 first; DataMigrationFlow44 second; DocumentationFlow21 parallel

**Output:** flows: [ArchitectureFlow31, DataMigrationFlow44, DocumentationFlow21], sub-phases: [3.1, 4.4, 2.1]

---

## B18-0B-016-003 — ProductVariants: refactoring "consolidate" → BackendServiceFlow41... wait — data tables

**Input:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring
- Sub-phase signal: 4.4 (product_variant tables — database vocabulary)

**Routing decision:**
- IMPLEMENT + refactoring + 4.4 vocabulary → DataMigrationFlow44
- Note: when refactoring verb applies to database tables (not code classes), routing stays 4.4
- BackendServiceFlow41 routes only when 4.1 (service/class) vocabulary is the sub-phase signal

**Output:** flow: DataMigrationFlow44, sub-phase: 4.4, TransitionType: refactoring

**Teaching point:** TransitionType: refactoring does NOT always route to BackendServiceFlow41. If the sub-phase vocabulary is 4.4 (data tables), the router sends to DataMigrationFlow44 even with refactoring TransitionType.

---

## B18-0B-016-004 — ProductVariants: feature_addition always routes to DataMigrationFlow44 (4.4)

**Routing invariant:**
- Any IMPLEMENT + feature_addition + 4.4 vocabulary → DataMigrationFlow44 (always)
- This applies regardless of scenario name (ProductVariants, AddressBook, TaxRecord, etc.)

---

## B18-0B-016-005 — ProductVariants: IMPLEMENT+SECURITY → two flows

**Input:**
- predicates: ["IMPLEMENT", "SECURITY"]
- TransitionType: feature_addition
- Sub-phase signals: 4.4 (seed, product variants) + security (PII in preferences)

**Routing decision:**
- IMPLEMENT → DataMigrationFlow44, sub-phase: 4.4
- SECURITY → SecurityReviewFlow

**Output:** flows: [DataMigrationFlow44, SecurityReviewFlow], sub-phases: [4.4, security]
