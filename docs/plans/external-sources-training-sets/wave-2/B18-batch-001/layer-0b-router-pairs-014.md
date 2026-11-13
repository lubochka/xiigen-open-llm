# Layer 0-B — Router Pairs (014)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-014-001 — Analytics/EventLog: IMPLEMENT (ETL) → DataMigrationFlow44

**Input:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase signal: 4.4 (ETL, event log, analytics vocabulary)
- ConnectorName: AnalyticsEventLogMigration

**Routing decision:**
- IMPLEMENT + feature_addition + 4.4 vocabulary → DataMigrationFlow44
- ETL vocabulary (Extract, Transform, Load) is 4.4-exclusive; does not route to 4.3

**Output:** flow: DataMigrationFlow44, sub-phase: 4.4

---

## B18-0B-014-002 — EventLog routing: ETL is NOT a 4.3 signal

**ETL vocabulary routing clarification:**

| Vocabulary | Correct sub-phase | Incorrect (v6 misrouting) |
|---|---|---|
| ETL pipeline | 4.4 (DataMigrationFlow44) | 4.3 (API connector) — WRONG |
| transform data | 4.4 | 4.3 — WRONG |
| load into database | 4.4 | any other sub-phase — WRONG |

**Rule:** ETL vocabulary routes to 4.4. 4.3 is for connector/webhook/API integration vocabulary ("connect to API", "send webhook", "call external service").

---

## B18-0B-014-003 — EventLog + SECURITY → DataMigrationFlow44 + SecurityReviewFlow

**Input:**
- predicates: ["IMPLEMENT", "SECURITY"]
- TransitionType: feature_addition
- Sub-phase signals: 4.4 (event log) + security (encrypted, PII)

**Routing decision:**
- IMPLEMENT → DataMigrationFlow44, sub-phase: 4.4
- SECURITY → SecurityReviewFlow
- Ordering: DataMigrationFlow44 first; SecurityReviewFlow after

**Output:** flows: [DataMigrationFlow44, SecurityReviewFlow], sub-phases: [4.4, security]

---

## B18-0B-014-004 — EventLog + ARCHITECTURE → two flows

**Input:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- Sub-phase signals: 4.4 (event log, analytics) + 3.1 (architecture pattern)

**Routing decision:**
- IMPLEMENT → DataMigrationFlow44, sub-phase: 4.4
- ARCHITECTURE → ArchitectureFlow31, sub-phase: 3.1
- Ordering: ArchitectureFlow31 first

**Output:** flows: [ArchitectureFlow31, DataMigrationFlow44], sub-phases: [3.1, 4.4]

---

## B18-0B-014-005 — EventLog: three predicates → three flows

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
