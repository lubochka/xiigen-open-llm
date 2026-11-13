# Layer 0-B — Router Pairs (020)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-020-001 — SessionData: IMPLEMENT (4.4) → DataMigrationFlow44

**Input:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase signal: 4.4 (session data, session_store table)
- ConnectorName: SessionDataMigration

**Routing decision:**
- IMPLEMENT + feature_addition + 4.4 → DataMigrationFlow44

**Output:** flow: DataMigrationFlow44, sub-phase: 4.4

---

## B18-0B-020-002 — SessionData + SECURITY → DataMigrationFlow44 + SecurityReviewFlow

**Input:**
- predicates: ["IMPLEMENT", "SECURITY"]
- TransitionType: feature_addition
- Sub-phase signals: 4.4 + security (encrypted, session tokens)

**Routing decision:**
- IMPLEMENT → DataMigrationFlow44, sub-phase: 4.4
- SECURITY → SecurityReviewFlow
- SecurityReviewFlow reviews token storage security after migration

**Output:** flows: [DataMigrationFlow44, SecurityReviewFlow], sub-phases: [4.4, security]

---

## B18-0B-020-003 — SessionData + ARCHITECTURE → ArchitectureFlow31 + DataMigrationFlow44

**Input:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- Sub-phase signals: 4.4 (session data) + 3.1 (session storage structure)

**Routing decision:**
- IMPLEMENT → DataMigrationFlow44, sub-phase: 4.4
- ARCHITECTURE → ArchitectureFlow31, sub-phase: 3.1
- Ordering: ArchitectureFlow31 first

**Output:** flows: [ArchitectureFlow31, DataMigrationFlow44], sub-phases: [3.1, 4.4]

---

## B18-0B-020-004 — SessionData: refactoring (SessionManager) → BackendServiceFlow41

**Input:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring
- Sub-phase signal: 4.1 (SessionManager class, ISessionStore — code vocabulary)

**Routing decision:**
- IMPLEMENT + refactoring + 4.1 → BackendServiceFlow41

**Output:** flow: BackendServiceFlow41, sub-phase: 4.1

---

## B18-0B-020-005 — SessionData: combined refactoring + migration → two flows

**Input:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring
- Sub-phase signals: 4.1 (SessionManager code) + 4.4 (session records data)

**Routing decision:**
- IMPLEMENT + refactoring + 4.1 → BackendServiceFlow41
- IMPLEMENT + 4.4 → DataMigrationFlow44
- Both sub-phases present → both flows route

**Output:** flows: [BackendServiceFlow41, DataMigrationFlow44], sub-phases: [4.1, 4.4]
