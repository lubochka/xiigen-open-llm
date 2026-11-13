# Layer 0-B — Router Pairs (022)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-022-001 — ApiKeyRotation: IMPLEMENT+SECURITY → DataMigrationFlow44 + SecurityReviewFlow

**Input:**
- predicates: ["IMPLEMENT", "SECURITY"]
- TransitionType: feature_addition
- Sub-phase signals: 4.4 (API keys, api_credentials table) + security (credentials, hashing)
- ConnectorName: ApiKeyRotationMigration

**Routing decision:**
- IMPLEMENT → DataMigrationFlow44, sub-phase: 4.4
- SECURITY → SecurityReviewFlow
- Ordering: DataMigrationFlow44 → SecurityReviewFlow (review after migration)

**Output:** flows: [DataMigrationFlow44, SecurityReviewFlow], sub-phases: [4.4, security]

---

## B18-0B-022-002 — ApiKeyRotation + ARCHITECTURE + SECURITY → three flows

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

---

## B18-0B-022-003 — ApiKeyRotation: refactoring (class) → BackendServiceFlow41

**Input:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring
- Sub-phase signal: 4.1 (ApiKeyManager class, KeyRotationService)

**Routing decision:**
- IMPLEMENT + refactoring + 4.1 → BackendServiceFlow41

**Output:** flow: BackendServiceFlow41, sub-phase: 4.1

---

## B18-0B-022-004 — ApiKeyRotation: credentials vocabulary always routes to SECURITY flow

**Routing rule:**
- "credentials", "API keys", "tokens", "authentication" → SECURITY fires → SecurityReviewFlow routes
- SECURITY predicate is always included in the flow list when these tokens appear

---

## B18-0B-022-005 — ApiKeyRotation: four-predicate case

**Input:**
- predicates: ["IMPLEMENT", "ARCHITECTURE", "SECURITY", "DOCUMENT"]
- TransitionType: feature_addition
- Sub-phase signals: 4.4 + 3.1 + security + 2.1

**Routing decision:**
- IMPLEMENT → DataMigrationFlow44, sub-phase: 4.4
- ARCHITECTURE → ArchitectureFlow31, sub-phase: 3.1
- SECURITY → SecurityReviewFlow
- DOCUMENT → DocumentationFlow21, sub-phase: 2.1 (parallel-eligible)
- Ordering: ArchitectureFlow31 → DataMigrationFlow44 → SecurityReviewFlow; DocumentationFlow21 parallel

**Output:** flows: [ArchitectureFlow31, DataMigrationFlow44, SecurityReviewFlow, DocumentationFlow21]
