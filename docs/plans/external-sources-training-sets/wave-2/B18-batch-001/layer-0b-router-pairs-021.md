# Layer 0-B — Router Pairs (021)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-021-001 — UserPreferences: seed → DataMigrationFlow44

**Input:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase signal: 4.4 (seed, user_preferences table)
- ConnectorName: UserPreferencesSeeding

**Routing decision:**
- IMPLEMENT + feature_addition + seed + 4.4 → DataMigrationFlow44

**Output:** flow: DataMigrationFlow44, sub-phase: 4.4

---

## B18-0B-021-002 — UserPreferences + SECURITY → two flows

**Input:**
- predicates: ["IMPLEMENT", "SECURITY"]
- TransitionType: feature_addition
- Sub-phase signals: 4.4 (seed, preferences) + security (PII, encrypted)

**Routing decision:**
- IMPLEMENT → DataMigrationFlow44, sub-phase: 4.4
- SECURITY → SecurityReviewFlow
- Ordering: DataMigrationFlow44 → SecurityReviewFlow

**Output:** flows: [DataMigrationFlow44, SecurityReviewFlow], sub-phases: [4.4, security]

---

## B18-0B-021-003 — UserPreferences + ARCHITECTURE → two flows

**Input:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- Sub-phase signals: 4.4 (seed, preferences) + 3.1 (preference schema structure)

**Routing decision:**
- IMPLEMENT → DataMigrationFlow44, sub-phase: 4.4
- ARCHITECTURE → ArchitectureFlow31, sub-phase: 3.1
- Ordering: ArchitectureFlow31 → DataMigrationFlow44

**Output:** flows: [ArchitectureFlow31, DataMigrationFlow44], sub-phases: [3.1, 4.4]

---

## B18-0B-021-004 — UserPreferences: fixture → DataMigrationFlow44 (not UnitTestingFlow51)

**Key routing rule restatement for seeding scenarios:**

| Input vocabulary | Router output | What does NOT fire |
|---|---|---|
| "fixture" + data vocabulary | DataMigrationFlow44 (4.4) | UnitTestingFlow51 (no TEST predicate) |
| "fixture" + "write tests" | DataMigrationFlow44 + UnitTestingFlow51 | — |
| "fixture" alone | DataMigrationFlow44 (4.4) | — |

---

## B18-0B-021-005 — UserPreferences: migrate + seed → both are 4.4; single flow

**Input:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase signal: 4.4 (migrate preferences + seed defaults)

**Routing decision:**
- migrate + seed → both are 4.4 vocabulary
- Single flow activation: DataMigrationFlow44 handles both migration and seeding
- No duplicate flow activation for multiple 4.4 vocabulary tokens

**Output:** flow: DataMigrationFlow44, sub-phase: 4.4
