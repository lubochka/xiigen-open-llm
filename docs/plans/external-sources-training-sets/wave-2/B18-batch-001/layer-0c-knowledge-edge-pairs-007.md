# Layer 0-C — Knowledge-Edge Pairs (007)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: knowledge-edge

## B18-0C-007-001 — TransitionType determination: refactoring vs feature_addition for B18

**TransitionType hierarchy (Rule 8):** refactoring > feature_addition > documentation > no contribution > none

**Primary rule:** The TransitionType is determined by the DOMINANT ACTION VERB, not by what files are created.

---

## B18-0C-007-002 — TransitionType: refactoring word list

**Complete list of words that produce TransitionType: refactoring:**

| Word | Rule | Example |
|---|---|---|
| refactor | Rule 1 | "Refactor the OrderService" |
| extract (code) | Rule 1 | "Extract PaymentProcessor from OrderService" |
| rename | Rule 19 | "Rename CustomerController to AccountController" |
| consolidate | Rule 20 | "Consolidate three repositories into one" |
| restructure | Rule 1 | "Restructure the routing methods" |
| reorganize | Rule 1 | "Reorganize the domain model" |
| move (code) | Rule 1 | "Move the helper functions to a utility class" |
| clean up | Rule 1 | "Clean up the legacy code" |
| migrate (code context) | Rule 1 | "Migrate stored procedures to service methods" |

---

## B18-0C-007-003 — TransitionType: feature_addition word list

**Complete list of words that produce TransitionType: feature_addition:**

| Word/phrase | Rule | Example |
|---|---|---|
| write migration | Rule 2 | "Write the customer migration" |
| create migration | Rule 2 | "Create migration for payment table" |
| build migration | Rule 2 | "Build the ETL migration" |
| seed | Rule 4 | "Seed the product database" |
| fixture | Rule 4 | "Create category fixtures" |
| migrate (data context) | Rule 5 | "Migrate customer records" |
| transform (data) | Rule 5 | "Transform legacy CSV data" |
| ETL | Rule 5 | "Build ETL pipeline" |
| load (data) | Rule 5 | "Load data into warehouse" |
| implement (new artifact) | General | "Implement the migration script" |
| build (new artifact) | General | "Build the ETL pipeline" |

---

## B18-0C-007-004 — Conflict resolution: refactoring verb + feature_addition signal

**Rule 8 hierarchy:** refactoring > feature_addition

**When both are present:**

Scenario 1: "Refactor the OrderService and write the migration for the orders table."
- "refactor" → refactoring signal
- "write migration" → feature_addition signal
- Resolution: refactoring > feature_addition → TransitionType: refactoring
- But both sub-phases fire (4.1 for refactor vocabulary, 4.4 for migration vocabulary)

Scenario 2: "Write a migration to consolidate the three order_status tables."
- "write migration" → feature_addition signal (Rule 2 — explicit new artifact)
- "consolidate" → refactoring signal
- Resolution: "write migration" is the primary task verb (Rule 2 explicitly makes this feature_addition)
- TransitionType: feature_addition (Rule 2 wins over consolidate when migration is the primary artifact)

**Key distinction:** Rule 2 creates a new artifact (the migration file). The consolidation is what the migration does, not the primary action. The PRIMARY artifact being created determines Rule 2 application.

---

## B18-0C-007-005 — SchemaConsolidation TransitionType analysis

**Request:** "Refactor the three overlapping customer tables into one normalized schema and write the consolidation migration."

**Competing signals:**
1. "refactor" → refactoring (Rule 9 — strong)
2. "write migration" → feature_addition (Rule 2 — strong)

**Resolution:** Rule 9 states "refactor" → TransitionType: refactoring "overrides feature_addition even if IMPLEMENT fires for multiple reasons."

**TransitionType: refactoring** — Rule 9 wins even though Rule 2 also fires.

**However, sub-phases:** Both 4.1 (refactoring vocabulary: tables/schema being refactored) and 4.4 (migration vocabulary: "write migration") fire.

This is why SchemaConsolidation is described as having "both refactoring and feature_addition" — it has both signals, but TransitionType resolves to refactoring.

---

## B18-0C-007-006 — TransitionType summary table for B18 scenarios

| Scenario | Primary Verb | TransitionType | Notes |
|---|---|---|---|
| CustomerDataMigration | migrate (data) | feature_addition | Data move |
| OrderHistoryMigration | migrate/write migration | feature_addition | New migration artifact |
| ProductCatalogMigration | migrate/write migration | feature_addition | New migration artifact |
| UserAccountMigration | migrate/write migration | feature_addition | New migration artifact |
| LegacyPaymentMigration | write migration | feature_addition | New migration artifact |
| ProductInventorySeeding | seed | feature_addition | New data artifact |
| CategoryFixtureSeeding | fixture/write | feature_addition | New data artifact |
| ETLPipelineBuild | build ETL | feature_addition | New pipeline artifact |
| OrderServiceRefactoring | refactor/extract | refactoring | Code restructuring |
| PaymentServiceRefactoring | refactor/rename/consolidate | refactoring | Code restructuring |
| SchemaConsolidation | refactor + write migration | refactoring | Rule 9 wins over Rule 2 |
| MultiStepMigration | write migration | feature_addition | New migration artifact |
| OrderArchiveMigration | write migration | feature_addition | New migration artifact |
| GdprUserDataMigration | write migration | feature_addition | New migration artifact |
