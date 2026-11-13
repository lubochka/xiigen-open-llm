# Layer 0-B — Router Pairs (003)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: router

## B18-0B-003-001 — [IMPLEMENT, ARCHITECTURE] migration → 4.4 + 3.1 (product migration)

**Scenario:** ProductCatalogMigration

**Router input:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- Vocabulary: "migrate", "product records", "CSV", "normalized", "structure" → 4.4 + 3.1 signals
- Request: "Migrate all product records from the legacy CSV flat file to the new normalized product/category/variant schema structure."

**Routing logic:**
1. Predicate: IMPLEMENT
   - TransitionType: feature_addition (data migration)
   - Vocabulary signals: migrate, product records, CSV, normalized → 4.4
   - Route: DataMigrationFlow44, sub-phase 4.4

2. Predicate: ARCHITECTURE
   - Trigger: "structure" (Rule 3)
   - Sub-phase: 3.1 (application/domain architecture — schema design)
   - Route: ArchitectureFlow31, sub-phase 3.1

**Router output:**
- DataMigrationFlow44 (4.4) — primary flow
- ArchitectureFlow31 (3.1) — secondary flow
- Flows: [DataMigrationFlow44, ArchitectureFlow31]

---

## B18-0B-003-002 — ARCHITECTURE sub-phase 3.1 vs 3.5 for B18

**Sub-phase 3.1 (Application/Domain Architecture):**
- Triggered by: schema design, data model design, domain structure, table structure, application architecture
- Most common in B18 (database/data layer design is application architecture)

**Sub-phase 3.5 (Infrastructure Architecture):**
- Triggered by: infrastructure pattern, deployment architecture, cloud architecture, system infrastructure design
- Rare in B18 (B18 is development-focused, not infrastructure-focused)

**B18 default:** ARCHITECTURE always routes to 3.1 unless explicit infrastructure vocabulary is present.

---

## B18-0B-003-003 — ProductCatalogMigration: full router trace

**ConnectorName:** ProductCatalogMigration

**Input:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- Sub-phases: 4.4, 3.1

**Router sequence:**
1. Select flow for each predicate:
   - IMPLEMENT (feature_addition, 4.4) → DataMigrationFlow44
   - ARCHITECTURE (3.1) → ArchitectureFlow31
2. Check factory availability (both flows available in B18)
3. No conflict between flows (parallel execution eligible)
4. ConnectorName passed to DataMigrationFlow44 as primary

**Final routing output:**
```
{
  "primary_flow": "DataMigrationFlow44",
  "secondary_flows": ["ArchitectureFlow31"],
  "sub_phases": ["4.4", "3.1"],
  "connector_name": "ProductCatalogMigration",
  "transition_type": "feature_addition"
}
```

---

## B18-0B-003-004 — schema structure → always ARCHITECTURE+DataMigrationFlow44

**Pattern:** "migrate data + schema structure" always routes to both flows.

**Examples:**
1. "Migrate product data to the new table structure" → [DataMigrationFlow44, ArchitectureFlow31]
2. "Move user records to the normalized schema structure" → [DataMigrationFlow44, ArchitectureFlow31]
3. "Write migration for new data structure" → [DataMigrationFlow44, ArchitectureFlow31]

**Reason:** "structure" always fires ARCHITECTURE (Rule 3), and any data migration vocabulary fires DataMigrationFlow44.

---

## B18-0B-003-005 — "normalize" without "structure" → DataMigrationFlow44 only

**Request:** "Write the ProductCatalogMigration to normalize the product data into separate category and variant tables."

**Router input:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Vocabulary: "migration", "normalize", "product data", "tables" → 4.4
- "structure" NOT present → ARCHITECTURE does NOT fire

**Router output:**
- DataMigrationFlow44, sub-phase 4.4 only
- Flows: [DataMigrationFlow44]

**Contrast:** "normalized table structure" (with "structure") → both flows. "Normalize data" (without "structure") → DataMigrationFlow44 only.

---

## B18-0B-003-006 — IMPLEMENT+ARCHITECTURE with schema vocabulary edge case

**Request:** "Migrate customer data and design the database schema for the new normalized tables."

**Router input:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- Vocabulary: "migrate", "customer data" → 4.4; "design", "schema" (paired with "design" structural noun) → 3.1

**Routing:**
- IMPLEMENT → DataMigrationFlow44, 4.4
- ARCHITECTURE (design + schema paired) → ArchitectureFlow31, 3.1

**Router output:** Flows: [DataMigrationFlow44, ArchitectureFlow31]

**Note:** "schema" alone does not fire ARCHITECTURE (Rule 16). But "schema" paired with "design" = structural noun context → ARCHITECTURE fires.
