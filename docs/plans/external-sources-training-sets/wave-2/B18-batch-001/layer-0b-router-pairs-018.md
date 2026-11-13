# Layer 0-B — Router Pairs (018)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-018-001 — TaxRecord: IMPLEMENT (4.4) → DataMigrationFlow44

**Input:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase signal: 4.4 (tax records, tax_history table)
- ConnectorName: TaxRecordMigration

**Routing decision:**
- IMPLEMENT + feature_addition + 4.4 → DataMigrationFlow44
- Tax data is data vocabulary; always 4.4

**Output:** flow: DataMigrationFlow44, sub-phase: 4.4

---

## B18-0B-018-002 — TaxRecord + SECURITY → DataMigrationFlow44 + SecurityReviewFlow

**Input:**
- predicates: ["IMPLEMENT", "SECURITY"]
- TransitionType: feature_addition
- Sub-phase signals: 4.4 (tax records) + security (compliance vocabulary)

**Routing decision:**
- IMPLEMENT → DataMigrationFlow44, sub-phase: 4.4
- SECURITY → SecurityReviewFlow
- Ordering: DataMigrationFlow44 first; SecurityReviewFlow after (compliance review of migrated data)

**Output:** flows: [DataMigrationFlow44, SecurityReviewFlow], sub-phases: [4.4, security]

---

## B18-0B-018-003 — TaxRecord: refactoring (restructure tables) → DataMigrationFlow44

**Input:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring
- Sub-phase signal: 4.4 (tax_data tables — database vocabulary)

**Routing decision:**
- IMPLEMENT + refactoring + 4.4 database vocabulary → DataMigrationFlow44 (not BackendServiceFlow41)
- "restructure" verb applies to database tables, not code classes → 4.4 vocabulary determines routing

**Output:** flow: DataMigrationFlow44, sub-phase: 4.4, TransitionType: refactoring

---

## B18-0B-018-004 — TaxRecord: ETL → DataMigrationFlow44 (not 4.3)

**Input:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase signal: 4.4 (ETL, tax data, extract/transform/load vocabulary)

**Routing decision:**
- ETL vocabulary exclusively → 4.4 (DataMigrationFlow44)
- ETL does NOT route to 4.3 (connector/API integration flow)

**Output:** flow: DataMigrationFlow44, sub-phase: 4.4

---

## B18-0B-018-005 — TaxRecord + ARCHITECTURE + SECURITY → three flows

**Input:**
- predicates: ["IMPLEMENT", "ARCHITECTURE", "SECURITY"]
- TransitionType: feature_addition
- Sub-phase signals: 4.4 + 3.1 + security

**Routing decision:**
- IMPLEMENT → DataMigrationFlow44, sub-phase: 4.4
- ARCHITECTURE → ArchitectureFlow31, sub-phase: 3.1
- SECURITY → SecurityReviewFlow
- Ordering: ArchitectureFlow31 → DataMigrationFlow44 → SecurityReviewFlow

**Output:** flows: [ArchitectureFlow31, DataMigrationFlow44, SecurityReviewFlow], sub-phases: [3.1, 4.4, security]
